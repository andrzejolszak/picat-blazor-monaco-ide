using System;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace WebApi.Controllers
{
    [Route("/")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // requires using Microsoft.Extensions.Configuration;
        private readonly IConfiguration Configuration;

        public ValuesController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ActionResult<string> Get(int mode, string program)
        {
            string executable = this.Configuration["Executable"];
            int timeoutSec = int.Parse(this.Configuration["TimeoutSeconds"]);
            program = WebUtility.UrlDecode(program);

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
            
            if (mode == 1)
            {
                process.StartInfo.Arguments = $"-g compile({sender}) " + fileName;
            }
            else if (mode == 2)
            {
                process.StartInfo.Arguments = "-g main " + fileName;
            }
            else
            {
                throw new InvalidOperationException("Wrong mode " + mode);
            }

            string res = "";
            process.OutputDataReceived += (_, data) => res += data.Data;
            process.ErrorDataReceived += (_, data) => res += data.Data;
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

            Console.WriteLine($"exit {exited} " + this.Configuration["status"]);

            System.IO.File.Delete(fileName);

            return res;
        }
    }
}
