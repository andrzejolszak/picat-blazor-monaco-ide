using System.Collections.Generic;

namespace PicatBlazorMonaco.Ast
{
    public class Examples
    {
        public static List<(string, string)> Samples = new List<(string, string)>
        {
            ("About this editor", About),
            ("Picat language introduction", LanguageIntroduction),
            ("Definite clause grammars (DCG)", Dcg),
        };

        public const string About = @"Welcome to the Picat editor based on Blazor and Monaco!
This editor aims to aid in learning Picat syntax and editing Picat programs.

Some of the features provided by the editor:
- Syntax highlighting
- Code completion with documentation for the built-in APIs (CTRL+Space)
- Various standard editor features like indentation helpers, occurences highlighting, search, etc.
- Several pre-defined code examples
- Compilation/execution of code via the associated service
- You can define custom links to this editor by inserting your code as UrlEncoded value of the 'code' parameter like in the below example:
  https://localhost:5001?code=foo+%3D%3E+bar.%0D%0Abar+%3D%3E+println(hello).

The editor is based on 2 components:
1. The front-end page: this where the core editing experience resides. It's built using Blazor and Monaco, and runs using client-side WASM.
2. The Web Api compiler service. A thin wrapper around the Picat compiler command line program. It's task is to actually compile and run programs.
   Please note that you can still use the front end for basic editing even if you can't connect to a compiler service, however you will not be able to compile and run programs.

Caveats:
- Only tested on Windows. Though it's built on .net core, so should be possible to build for Linux/Mac with minimal or no code changes.
- Currently the Web API compiler service comes bundled with the Windows compiler - using on Linux/Mac would require the respective compiler version to be included.
- Currently the Web API compiler service is not safe to be exposed on the network where anybody can run arbitrary code on it. Please only use on localhost or on trusted networks.

";

        public const string Dcg = @"/* 
  a^nb^nc^n in Picat v3.

  From comp.lang.prolog 2008-01-22.
  Using DCG.

  This Picat model was created by Hakan Kjellerstrand, hakank@gmail.com
  See also my Picat page: http://www.hakank.org/picat/
*/

import cp.

main => go.

% Generate [],abc,aabbcc, ... 
go ?=> 
   anbncn(Ls, []),
   println(Ls),
   % restrict the length
   (Ls.len < 100 -> fail ; true),
   nl.

go => true.

% Using v3_utils.phrase
go2 ?=> 
   phrase(anbncn,Ls),
   println(Ls),
   % restrict the length
   (Ls.len < 100 -> fail ; true),
   nl.

go2 => true.


% From https://stackoverflow.com/questions/28893324/what-does-mean-in-prolog
% Generate N=9 (length: 3*9 = 27)
go3 ?=> 
   start(9,Ls,[]),
   println(Ls),
   println(len=Ls.len),
   fail,
   nl.

go3 => true.


% Get the N of a sequence
go4 ?=> 
   start(N,""aabbcc"",[]),
   println(n=N),
   fail,
   nl.

go4 => true.



anbncn --> n_x(N, a), n_x(N, b), n_x(N, c).

n_x(0, _)    --> [].
n_x(s(N), X) --> [X], n_x(N, X).

%Yielding:
%
%   %?- anbncn(Ls, []).
%   %@ Ls = [] ;
%   %@ Ls = [a, b, c] ;
%   %@ Ls = [a, a, b, b, c, c] ;
%   %@ Ls = [a, a, a, b, b, b, c, c, c] a
%   %@ Yes


% From https://stackoverflow.com/questions/28893324/what-does-mean-in-prolog
start(N) --> as(N), bs(N), cs(N).

as(N) --> {N #> 0, M #= N-1}, [a], as(M).
as(0) --> [].

bs(N) --> {N #> 0, M #= N-1}, [b], bs(M).
bs(0) --> [].

cs(N) --> {N #> 0, M #= N-1}, [c], cs(M).
cs(0) --> [].

% phrase/2
phrase(P, L) :-
    Goal =.. [P, L,[]],
    call(Goal).

% phrase/3
phrase(P, P2, L) :-
    Goal =.. [P, P2, L,[]],
    call(Goal).";

        public const string LanguageIntroduction = @"%%% Based on: http://retina.inf.ufsc.br/picat_guide/

%%% Data Types:

V1 = X1, V2 = _ab, V3 = _       % variables  
 
N1 = 12, N2 = 0xf3, N3 = 1.0e8  % numbers  
 
A1 = x1, A2 = ’_AB’, A3 = ’’    % atoms  
 
L = [a,b,c,d]                   % a list  
 
write(""hello""++""picat"")         % strings  
% [h, e, l, l, o, p, i, c, a, t]

print(""hello""++""picat"")
% hellopicat

writef(""%s"",""hello""++""picat"")   % formatted write
% hellopicat

writef(""%-5d %5.2f"",2,2.0)      % formatted write  
% 2      2.00  

S = $point(1.0,2.0)             % a structure

S = new_struct(point,3)         % create a structure
% S = point(_3b0, _3b4, _3b8)

A = {a,b,c,d}                   % an array

A = new_array(3)                % create an array
% A = { _3b0, _3b4, _3b8 }

M = new_map([one=1, two=2])      % create a map
% M = (map)[two = 2, one = 1]

M = new_set([one, two, three])    % create a map set
% M = (map)[two, one, three]

X = 1..2..10                    % ranges
% X = [1, 3, 5, 7, 9]

X = 1..5  
% X = [1,2,3,4,5]

integer(5)  
% yes  
 
real(5)  
% no  
 
var(X)  
% yes  
 
X=5, var(X)  
% no  
 
5 != 2+2  
% yes  
 
%%% Calling Conventions:

X = to_binary_string(5)  
% X = [’1’,’0’,’1’]  
 
L = [a,b,c,d], X = L[2]  
% X = b  
 
L = [(A,I) : A in [a,b], I in 1..2].  
% L = [(a,1),(a,2),(b,1),(b,2)]  
 
put_attr(X,one,1), One = get_attr(X,one)  % attributed var  
% One = 1  
 
S = new_struct(point,3), Name = name(S), Len = length(S)  
% S = point(_3b0,_3b4,_3b8)  
% Name = point  
% Len = 3  
 
S = new_array(2,3), S[1,1] = 11, D2 = length(S[2])  
% S = {{11,_93a0,_93a4},{_938c,_9390,_9394}}  
% D2 = 3  
 
M = new_map(), put(M,one,1), One = get(M,one)  
% One = 1  
 
M = new_set(), put(M,one), has_key(M,one)

X = 5.to_binary_string()  
% X = [’1’,’0’,’1’]  
 
X = 5.to_binary_string().length  
% X = 3  
 
X.put(one,1), One = X.get(one)  
% One = 1  
 
X = math.pi  
% X=3.14159  
 
S = new_struct(point,3), Name = S.name, Len = S.length  
% S = point(_3b0,_3b4,_3b8)  
% Name = point  
% Len = 3  
 
S = new_array(2,3), S[1,1] = 11, D2 = S[2].length  
% S = {{11,_93a0,_93a4},{_938c,_9390,_9394}}  
% D2 = 3  
 
M = new_map(), M.put(one,1), One = M.one.  
% One = 1

%%% Defining Predicates:

fib(0,F) => F=1.  
fib(1,F) => F=1.  
fib(N,F),N>1 => fib(N-1,F1),fib(N-2,F2),F=F1+F2.  
fib(N,F) => throw $error(wrong_argument,fib,N).

fib2(N,F) =>  
    if (N=0; N=1) then  
        F=1  
    elseif N>1 then  
        fib2(N-1,F1),fib2(N-2,F2),F=F1+F2  
    else  
        throw $error(wrong_argument,fib2,N)  
    end.

index (+,-) (-,+)  
edge(a,b).  
edge(a,c).  
edge(b,c).  
edge(c,b).

%%% Defining Functions:

fibF(0) = F => F=1.  
fibF(1) = F => F=1.  
fibF(N) = F, N>1 => F = fibF(N-1)+fibF(N-2).  
 
qsort([]) = L => L=[].  
qsort([H|T]) = L => L = qsort([E : E in T, E=<H]) ++ [H] ++  
                        qsort([E : E in T, E>H]).

fibF2(0) = 1.  
fibF2(1) = 1.  
fibF2(N) = F, N>1 => F = fibF2(N-1)+fibF2(N-2).  
 
qsort2([]) = [].  
qsort2([H|T]) =  
    qsort2([E : E in T, E=<H]) ++ [H] ++ qsort2([E : E in T, E>H]).

fib3(N) = cond((N=0;N=1), 1, fib3(N-1)+fib3(N-2)).

%%% Assignments and Loops:

test => X=0, X:=X+1, X:=X+2, write(X).

p(A) =>  
    foreach (I in 1 .. A.length)  
        E = A[I],  
        writeln(E)  
    end.

write_map(Map) =>  
    foreach (Key=Value in Map)  
        writef("" % w =% w' n"",Key,Value)  
    end.

sum_list(L) = Sum =>    % returns sum(L)
    S=0,  
    foreach (X in L)  
        S:=S+X
    end,
    Sum = S.

read_list = List =>
    L =[],
    E = read_int(),
    while (E != 0)  
        L := [E|L],  
        E := read_int()
    end,  
    List=L.

%%% Tabling:

table  
fib4(0) = 1.  
fib4(1) = 1.  
fib4(N) = fib4(N-1)+fib4(N-2).

table(+,+,min)  
edit([],[],D) => D = 0.  
edit([X|Xs],[X|Ys],D) =>  
    edit(Xs,Ys,D).  
edit(Xs,[Y|Ys],D) ?=>      % insert  
    edit(Xs,Ys,D1),  
    D = D1+1.  
edit([X|Xs],Ys,D) =>       % delete  
    edit(Xs,Ys,D1),  
    D = D1+1.

%%% Constraints:

import cp.  
 
go =>  
    Vars = [S,E,N,D,M,O,R,Y],  % generate variables  
    Vars :: 0..9,  
    all_different(Vars),     % generate constraints  
    S #!= 0,  
    M #!= 0,  
    1000*S+100*E+10*N+D+1000*M+100*O+10*R+E  
         #= 10000*M+1000*O+100*N+10*E+Y,  
    solve(Vars),             %  search  
    writeln(Vars).

%%% Higher-Order Calls:

S = $member(X), call(S,[1,2,3])  
% X = 1;  
% X = 2;  
% X = 3;  
% no  
 
L = findall(X,member(X,[1,2,3])).  
% L = [1,2,3]  
 
Z = apply(’+’,1,2)  
% Z = 3

%%% Prebuilt Maps:

go ?=>  
    get_heap_map(h1).put(one,1),  
    get_global_map(g1).put(one,1),  
    get_table_map(t1).put(one,1),  
    fail.  
go =>  
    if (get_heap_map(h1).has_key(one)) then  
       writef(""heap map h1 has key%n"")  
    else  
       writef(""heap map h1 has no key%n"")
    end,  
    if (get_global_map(g1).has_key(one)) then
       writef(""global map g1 has key%n"")
    else  
       writef(""global map g1 has no key%n"")
    end,  
    if (get_table_map(t1).has_key(one)) then
       writef(""table map t1 has key%n"")
    else  
       writef(""table map t1 has no key%n"")
    end.
";
    }
}
