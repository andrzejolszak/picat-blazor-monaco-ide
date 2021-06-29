using System;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cors;

namespace WebApi.Controllers
{
    [Route("/")]
    [ApiController]
    [EnableCors]
    public class ValuesController : ControllerBase
    {
        // requires using Microsoft.Extensions.Configuration;
        private readonly IConfiguration Configuration;

        public ValuesController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        [EnableCors]
        public string Get(string goal, string program)
        {
            string executable = this.Configuration["Executable"];
            int timeoutSec = int.Parse(this.Configuration["TimeoutSeconds"]);

            string sender = "s" + HttpContext.Connection.Id.ToString();
            string fileName = @".\requests\" + sender + ".pi";
            System.IO.File.WriteAllText(fileName, program);

            using var process = new Process
            {
                StartInfo =
                {
                    FileName = executable,
                    Arguments = $"{program}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            
            if (goal == "--v")
            {
                process.StartInfo.Arguments = "--v";
            }
            else if (goal == "--c")
            {
                process.StartInfo.Arguments = $"-g compile({sender}) " + fileName;
            }
            else
            {
                process.StartInfo.Arguments = "-g " + goal + " " + fileName;
            }

            string error = "";
            string output = "";
            process.OutputDataReceived += (_, data) => error += data.Data + "\r\n";
            process.ErrorDataReceived += (_, data) => output += data.Data + "\r\n";
            Console.WriteLine("starting");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            // string output = process.StandardOutput.ReadToEnd();
            // string err = process.StandardError.ReadToEnd();
            var exited = process.WaitForExit(1000 * timeoutSec);
            if (!exited)
            {
                process.Kill(true);
                return $"Execution timed out after {timeoutSec}s";
            }

            System.IO.File.Delete(fileName);

            Console.WriteLine($"exit {exited} " + this.Configuration["status"]);
            Console.WriteLine($"Out: {output}");
            Console.WriteLine($"Err: {error}");

            return output.Replace("\r\n\r\n", "\r\n");
        }
    }
}
