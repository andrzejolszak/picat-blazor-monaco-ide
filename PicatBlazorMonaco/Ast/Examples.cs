using System.Collections.Generic;

namespace PicatBlazorMonaco.Ast
{
    public class Examples
    {
        public static List<(string, string)> Samples = new List<(string, string)>
        {
            ("About this editor", About),
            ("Picat language introduction", LanguageIntroduction),
            ("Sevaral examples in Picat", SeveralExamples),
            ("Definite clause grammars (DCG)", Dcg),
            ("Picat links", Links)
        };

        public const string Links = @"Useful Picat links:
http://picat-lang.org/resources.html
https://github.com/nfzhou/Picat
http://www.hakank.org/picat/
http://retina.inf.ufsc.br/picat.html";

        public const string About = @"/*
Welcome to the Picat editor based on Blazor and Monaco!
This editor aims to aid in learning Picat syntax and editing Picat programs.
Below you will get a step by step introduction to the available features.
The editor relies on a compiler service to do the actual compilation and code execution,
and to enable some of the features. Nevertheless, it is still possible to get a basic
editing experience without the service running.
*/
main => foo.

% Syntax highlighting and basic operations:
% The typical editing operations known from modern code editors are available, this includes:
% copy-paste, undo, auto-intendation on Enter, selection highlighting, Alt+Shift+Click block editing, Ctrl+F search, move line Alt+Up/Down etc.
% You can try some of them by editing the below code:
foo() => bar,
         bar, bar.

% Code completion:
% You can bring up the code completion options by pressing Ctrl+Space.
% Documentation of most built-in functions is included,
% and documentation for your own functions is taken from the comment preceding the declaration.
% If the documentation is not displayed, you can open by clicking 'Read More' on the very right of a completion entry.
% Try bringing up the completion for 'fo' to see it in action:
foo2 => fo.

% Built-ins hover documentation:
% When it comes to built-in functions in existing code you can view their documentation either by:
% - Ctrl+Space to bring their completion entry
% - Mouse hover
% You can try this below:
bar => println(hello).

% Go to declaration:
% For your own functions, you can easily navigate to it's declaration by Ctrl+Click or the context menu.
% Try going to 'foo' through the below code:
car => foo.

% Syntax error highlighting (requires compiler service):
% Your code will get compiled in the background as you type, and also when you decide to run it.
% Any syntax errors discovered will be market in red, and a hover tooltip to the code describing the error.
% The compiler output will be shown in the output panel, and you can double click 
% the '*** SYNTAX ERROR ***' message to quickly navigate to the target code fragment.
% Please try it out on the below code, and use it to help you fix it.
dar =>
    true :-) .

% Code execution (requires compiler service):
% You can run your program by supplying a goal, and clicking the Run button.
% The output will be presented in the output panel.
% Please note that the editor is not meant as a full-featured runner - you should always use you local Picat compiler for serious runs.
% The editor runner limitations include: interactive user input is not supported, max execution time limited to 10s.
% Give it a go by running the 'main' goal.

% Unit tests (requires compiler service):
% The editor provides a convention for defining unit tests:
% they are argument-less predicates whose name starts with 'test_', and they can either succeed, fail, or throw.
% Unit tests are a great way to gain confidence in the individual pieces of your program, and ensure that any refactorings
% or additions of new features do not break the existing logic.
% Tests are executed in a randomized order to prevent sequence-dependence, and their total execution time is limited to 15s.
% You can double-click a test name to navigate to it's source code.
% Below you can find some example tests, press 'Run All' in the Tests panel to run them:
test_foo => foo.
test_true.
test_fail => fail.
test_dar() => dar.
test_dar2() => this_do_not_exist(A, B, c, d).
test_throw => X is 1/0.

% Custom links:
% If you want to share a code example link with someone, you can define a custom link to this editor
% by inserting your code as UrlEncoded value of the 'code' parameter like in the below example:
% https://localhost:5001?code=main+%3D%3E+bar.%0D%0Abar+%3D%3E+println(hello).

% Reporting errors:
% Please report any errors to the githu repository at: https://github.com/andrzejolszak/picat-blazor-monaco-ide
% If possible, please include the contents of the JS console log after you have triggered the error (F12, or Developers tools in most browsers)
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

        public const string SeveralExamples = @"/*; Several examples in Picat */
/*** begin file exs.pi ***/
import cp, planner.   

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% PREDICATES AND FUNCTIONS
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Here are several versions for computing Fibonacci numbers 
% A predicate
fibp(0,F) => F = 1.
fibp(1,F) => F = 1.
fibp(N,F),N>1 => fibp(N-1,F1), fibp(N-2,F2), F = F1+F2.

% A function
fibf(0)=F => F = 1.
fibf(1)=F => F = 1.
fibf(N)=F, N>1 => F = fibf(N-1)+fibf(N-2).

% A function with function facts
fibfa(0) = 1.
fibfa(1) = 1.
fibfa(N) = fibfa(N-1)+fibfa(N-2).

% Using if-then-else
fibi(N) = F => 
    if N == 0 then
        F = 1
    elseif N == 1 then
        F = 1
    else
        F =  fibg(N-1)+fibg(N-2)
    end.

% Using Prolog-style if-then-else
fibg(N) = F => 
    (N == 0 -> 
        F = 1
    ;N == 1 ->
        F = 1
    ; 
        F = fibg(N-1)+fibg(N-2)
    ).

% Using a conditional expression
fibc(N) = cond((N == 0; N == 1), 1, fibc(N-1)+fibc(N-2)).

% A tabled function
table
fibt(0) = 1.
fibt(1) = 1.
fibt(N) = fibt(N-1)+fibt(N-2).

% A nondeterministic predicate with a backtrackable rule
my_member(Y,[X|_]) ?=> Y = X.
my_member(Y,[_|L]) => my_member(Y,L).

my_between(From,To,X), From == To => X = From.
my_between(From,To,X),From < To => X = From; my_between(From+1,To,X).

my_select(Y,[X|L],LR) ?=> Y = X, LR = L.
my_select(Y,[X|L],LR) => LR = [X|LRR], my_select(Y,L,LRR).

my_permutation([],P) => P = [].
my_permutation(L,P) => 
    P = [X|PR],                    
    my_select(X,L,LR),
    my_permutation(LR,PR).

% predicate facts
index(+,-) (-,+)
edge(1,2).
edge(1,3).
edge(2,3).
edge(3,2).

% several sort algorithms
merge_sort([]) = [].
merge_sort([X]) = [X].
merge_sort(L) = SL => split(L,L1,L2), SL = merge(merge_sort(L1),merge_sort(L2)).

split([X,Y|Zs],L1,L2) => L1 = [X|LL1], L2 = [Y|LL2], split(Zs,LL1,LL2).
split(Zs,L1,L2) => L1 = Zs,L2 = [].

merge([],Ys) = Ys.
merge(Xs,[]) = Xs.
merge([X|Xs],Ys@[Y|_]) = [X|Zs], X < Y => Zs = merge(Xs,Ys).  % Ys@[Y|_] is an as-pattern
merge(Xs,[Y|Ys]) = [Y|Zs] => Zs = merge(Xs,Ys).

insert_sort([]) = [].
insert_sort([H|T]) = insert(H,insert_sort(T)).

private
insert(X,[]) = [X].
insert(X,Ys@[Y|_]) = Zs, X =< Y => Zs = [X|Ys].
insert(X,[Y|Ys]) = [Y|insert(X,Ys)].

% two versions that return the minumum and maximum of a list
% a predicate
min_max_p([H|T],Min,Max) => min_max_p_aux(T,H,Min,H,Max).

% A private function is not visiable outside
private
min_max_p_aux([],CMin,Min,CMax,Max) => CMin = Min,CMax = Max.
min_max_p_aux([H|T],CMin,Min,CMax,Max) => min_max_p_aux(T,min(CMin,H),Min,max(CMax,H),Max).

% a function that returns the minimum and maximum of a list as a pair 
min_max([H|T]) = min_max_aux(T,H,H).

private
min_max_aux([],CMin,CMax) = (CMin,CMax).
min_max_aux([H|T],CMin,CMax) = min_max_aux(T,min(CMin,H),max(CMax,H)).

% return the sum of a list
sum_list(L) = Sum =>
    sum_list_aux(L,0,Sum).

% a private predicate is never exported
private
sum_list_aux([],Acc,Sum) => Sum = Acc.
sum_list_aux([X|L],Acc,Sum) => sum_list_aux(L,Acc+X,Sum).

% two lists that are structually equal, e.g., struct_equal(X,[a]) fails
struct_equal(A,B),atomic(A) => A == B.
struct_equal([H1|T1],[H2|T2]) =>
  struct_equal(H1,H2),
  struct_equal(T1,T2).

is_sorted([]) => true.
is_sorted([_]) => true.
is_sorted([X|L@[Y|_]]) =>X @<= Y, is_sorted(L).

% An empty tree is represented by {}, and a non-empty binary tree is 
% represented by its root, which takes form {Val,Left,Right}.

is_btree({}) => true.
is_btree({_Val,Left,Right}) =>
    is_btree(Left),
    is_btree(Right).

inorder({}) = [].
inorder({Val,Left,Right}) = inorder(Left) ++ [Val] ++ inorder(Right).

% binary search tree
is_bstree({}) => true.
is_bstree(BT@{Val,Left,Right}) =>
    is_bstree(Left,min_bstree(BT),Val),
    is_bstree(Right,Val,max_bstree(BT)).

is_bstree({},_,_) => true.
is_bstree({Val,Left,Right},Min,Max) => 
    Val @>= Min,  Val @=< Max,
    is_bstree(Left,Min,Val),
    is_bstree(Right,Val,Max).

min_bstree({Elm,{},_Right}) = Elm.
min_bstree({_Elm,Left,_Right}) = min_bstree(Left).

max_bstree({Elm,_Left,{}}) = Elm.
max_bstree({_Elm,_Left,Right}) = max_bstree(Right).

lookup_bstree({Elm,_,_},Elm) => true.
lookup_bstree({Val,Left,_},Elm), Elm < Val => 
    lookup_bstree(Left,Elm).
lookup_bstree({_,_,Right},Elm) =>
    lookup_bstree(Right,Elm).

tree_inst1 = {6, {5, {4, {}, {}}, 
                     {7, {}, {}}}, 
                 {8, {3, {}, {}}, 
                     {9, {}, {}}}}.

tree_inst2 = {7, {5, {4, {}, {}}, 
                     {6, {}, {}}}, 
                 {8, {8, {}, {}}, 
                     {9, {}, {}}}}.

test_btree =>
    Tree1 = tree_inst1(),
    println(inorder(Tree1)),
    println(cond(is_bstree(Tree1),""a binary search tree"",""not a binary search tree"")),
    Tree2 = tree_inst2(),
    println(inorder(Tree2)),
    println(cond(is_bstree(Tree2),""a binary search tree"",""not a binary search tree"")).

% An example that uses data constructors
% A term in the form of $f(X) is a data constructor 
divide_main => 
   Exp= $((((((((x/x)/x)/x)/x)/x)/x)/x)/x)/x,
   d(Exp,x,D),
   writeln(D).

d(U+V,X,D) => 
    D = $DU+DV,
    d(U,X,DU),
    d(V,X,DV).
d(U-V,X,D) =>
    D = $DU-DV,
    d(U,X,DU),
    d(V,X,DV).
d(U*V,X,D) =>
    D = $DU*V+U*DV,
    d(U,X,DU),
    d(V,X,DV).
d(U/V,X,D) =>
    D = $(DU*V-U*DV)/(^(V,2)),
    d(U,X,DU),
    d(V,X,DV).
d(^(U,N),X,D) =>
    D = $DU*N*(^(U,N1)),
    integer(N),
    N1 = N-1,
    d(U,X,DU).
d(-U,X,D) =>
    D = $-DU,
    d(U,X,DU).
d(exp(U),X,D) =>
    D = $exp(U)*DU,
    d(U,X,DU).
d(log(U),X,D) =>
    D = $DU/U,
    d(U,X,DU).
d(X,X,D) => D=1.
d(_,_,D) => D=0.

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% LOOPS
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% another version for summing up a list 
sum_list_imp(L) = Sum =>
    S = 0,
    foreach (X in L)
       S := S+X
    end,
    Sum = S.

% using a loop to find the minimum and maximum of a list
min_max_ip([H|T], Min, Max) =>
    LMin = H,
    LMax = H,
    foreach (E in T)
        LMin := min(LMin, E),
        LMax := max(LMax, E)
    end,
    Min = LMin,
    Max = LMax.

% draw the Pascal triangle
pascal =>
    print(""enter an integer:""),
    N = read_int(),
    foreach(I in 0..N)
        Num := 1,
        foreach(K in 1..I+1) 
            printf(""%d"",Num),
            Num := Num*(I-K+1) div K
        end,
        nl
    end.

% another solution
pascal2 =>
    print(""enter an integer:""),
    N = read_int(),
    Row = [1], 
    foreach(_I in 1..N)
        writeln(Row),
        Row := next_row(Row)
    end.

private
next_row(Row)=Res =>
    NewRow = [1], Prev = 1,
    foreach (K in tail(Row))
        NewRow := [Prev+K|NewRow],
        Prev := K
    end,
    Res = [1|NewRow].

/* another definition, not so efficient because Row[I] takes O(I) time
private
next_row(Row) = [1] ++ [Row[I]+Row[I+1] : I in 1..Row.length-1] ++ [1].
*/

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% LIST COMPREHENSION
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% a list comprehension inside another list comprehension
% Picat> L=list_of_lists(5)
% L = [[1],[1,2] [1,2,3],[1,2,3,4],[1,2,3,4,5]]
list_of_lists(N) = [[Y : Y in 1..X] : X in 1..N].   

% another definition
another_list_of_lists(N) = [1..X : X in 1..N].   

qsort([]) = [].
qsort([H|T]) = qsort([E : E in T, E =< H])++[H]++qsort([E : E in T, E>H]).

power_set([]) = [[]].
power_set([H|T]) = P1++P2 =>
    P1 = power_set(T),
    P2 = [[H|S] : S in P1].

% generate permutations
perm([]) = [[]].
perm(Lst) = [[E|P] : E in Lst, P in perm(Lst.delete(E))].

%another definition
perm1([]) = [[]].
perm1([H|T]) = [insert(P,I,H) : P in Ps, I in 1..P.length+1] => Ps = perm1(T).

% A*B=C
matrix_multi(A,B) = C =>
    C = new_array(A.length,B[1].length),
    foreach(I in 1..A.length, J in 1..B[1].length)
        C[I,J] = sum([A[I,K]*B[K,J] : K in 1..A[1].length])
    end.

% Sieve of Eratosthenes
my_primes(N) = L =>
    A = new_array(N),
    foreach(I in 2..floor(sqrt(N)))
        if (var(A[I])) then
            foreach(J in I**2..I..N)
                A[J] = 0
            end
         end
     end,
     L = [I : I in 2..N, var(A[I])].

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% TABLING
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% mode-directed tabling
% finding shortest paths on a graph given by the relation edge/3.
table(+,+,-,min) 
sp(X,Y,Path,W) ?=>
    Path=[(X,Y)],
    edge(X,Y,W).
sp(X,Y,Path,W) =>
    Path = [(X,Z)|PathR],
    edge(X,Z,W1),
    sp(Z,Y,PathR,W2),
    W = W1+W2.

index(+,-,-) (+,+,-)
edge(1,2,1).
edge(1,3,2).
edge(2,3,3).
edge(3,2,4).

% binomial coefficient
bc(_N,0) = 1.
bc(N,N) = 1.
bc(N,1) = N.
bc(N,K) = bc(N-1,K-1) + bc(N-1,K).

% computing the minimal editing distance of two given lists
table(+,+,min)
edit([],[],D) => D=0.
edit([X|Xs],[X|Ys],D) =>   % copy
    edit(Xs,Ys,D).
edit(Xs,[_Y|Ys],D) ?=>      % insert
    edit(Xs,Ys,D1),
    D = D1+1.
edit([_X|Xs],Ys,D) =>       % delete
    edit(Xs,Ys,D1),
    D = D1+1.

% the Farmer's problem (use planner)
farmer =>
    S0 = [s,s,s,s],
    plan(S0,Plan),
    println(Plan).

final([n,n,n,n]) => true.

action([F,F,G,C],S1,Action,ActionCost) ?=>
    Action = farmer_wolf,
    ActionCost = 1,        
    opposite(F,F1),
    S1 = [F1,F1,G,C],
    not unsafe(S1).
action([F,W,F,C],S1,Action,ActionCost) ?=>
    Action = farmer_goat,
    ActionCost = 1,        
    opposite(F,F1),
    S1 = [F1,W,F1,C],
    not unsafe(S1).
action([F,W,G,F],S1,Action,ActionCost) ?=>
    Action = farmer_cabbage,
    ActionCost = 1,        
    opposite(F,F1),
    S1 = [F1,W,G,F1],
    not unsafe(S1).
action([F,W,G,C],S1,Action,ActionCost) =>
    Action = farmer_alone,
    ActionCost = 1,        
    opposite(F,F1),
    S1 = [F1,W,G,C],
    not unsafe(S1).

index (+,-) (-,+)
opposite(n,s).
opposite(s,n).

unsafe([F,W,G,_C]),W == G,F !== W => true.
unsafe([F,_W,G,C]),G == C,F !== G => true.

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% CONSTRAINT PROGRAMS (using cp)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% SEND+MORE=MONEY
sendmory => 
    Vars = [S,E,N,D,M,O,R,Y],    % generate variables
    Vars :: 0..9,
    all_different(Vars),       % generate constraints
    S #!= 0,
    M #!= 0,
    1000*S+100*E+10*N+D+1000*M+100*O+10*R+E 
    #= 10000*M+1000*O+100*N+10*E+Y,
    solve(Vars),               %  search
    writeln(Vars).

% N-queens
queens(N) =>
    Qs = new_array(N),
    Qs :: 1..N,
    foreach (I in 1..N-1, J in I+1..N)
        Qs[I] #!= Qs[J],
        abs(Qs[I]-Qs[J]) #!= J-I
    end,
    solve([ff],Qs),
    writeln(Qs).

% another program for N-queens
queens2(N, Q) =>
    Q = new_list(N),
    Q :: 1..N,
    Q2 = [$Q[I]+I : I in 1..N],
    Q3 = [$Q[I]-I : I in 1..N],
    all_different(Q),
    all_different(Q2),
    all_different(Q3),       
    solve([ff],Q).

% graph coloring (reuse edge/2 defined above)
color(NV,NC) =>
    A = new_array(NV),
    A :: 1..NC,
    foreach(I in 1..NV-1, J in I+1..NV)
        if edge(I,J);edge(J,I) then
             A[I] #!= A[J]
        end
    end,
    solve(A),
    writeln(A).

% a 0-1 integer model for graph coloring
bcolor(NV,NC) =>
    A = new_array(NV,NC),
    A :: [0,1],
    foreach(I in 1..NV)
        sum([A[I,K] : K in 1..NC]) #= 1
    end,
    foreach(I in 1..NV-1, J in I+1..NV)
        if edge(I,J);edge(J,I) then
             foreach(K in 1..NC)
                 #~ A[I,K] #\/ #~ A[J,K]
             end
        end
    end,
    solve(A),
    writeln(A).


%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% I/O
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% Read a list of integers, stopping when 0 is read
read_array_main =>
    A = new_array(100),
    Len = read_array(A),
    foreach (I in 1..Len)
        writeln(A[I])
    end.

read_array(A) = Len =>
    Count = 0,
    E = read_int(),         % read from stdin
    while (E != 0) 
        Count := Count+1,
        A[Count] = E,
        E := read_int()
    end,
    Len = Count.

% copy a text file line-by-line
copy(IName,OName) =>
    IStream = open(IName),
    OStream = open(OName,write),
    Line = IStream.read_line(),
    while (Line != end_of_file)
        OStream.printf(""%s%n"",Line),
        Line := IStream.read_line()
    end,
    close(IStream),
    close(OStream).

% Picat> output_students([$student(""john"",""cs"",3),$student(""mary"",""math"",4.0)])
%      john         cs  3.00
%      mary       math  4.00
output_students(Students) =>
    foreach($student(Name,Major,GPA) in Students)
        printf(""%10s %10s %5.2f%n"",Name,Major,to_real(GPA))
    end.

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% HIGHER-ORDER (not recommended because of poor performance)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Picat> my_map(-,[1,2,3]) = L
% L = [-1,-2,-3]
% Picat> my_map(+,[1,2,3],[4,5,6]) = L
% L = [5,6,7]
% Picat> my_fold(+,0,[1,2,3]) = S
% S = 6

my_map(_F,[]) = [].
my_map(F,[X|Xs]) = [apply(F,X)|my_map(F,Xs)].

my_map(_F,[],[]) = [].
my_map(F,[X|Xs],[Y|Ys]) = [apply(F,X,Y)|my_map(F,Xs,Ys)].

my_fold(_F,Acc,[]) = Acc.
my_fold(F,Acc,[H|T]) = my_fold(F, apply(F,H,Acc),T).

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% ACTION RULES
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
test_ar =>
    watch_event(X), 
    watch_dom(X),
    watch_dom_any(X),
    watch_ins(X),
    watch_bound(X),
    X.post_event(event),
    X.post_event_dom(dom),
    X.post_event_ins(),
    X.post_event_bound(),
    X.post_event_any(any).

watch_event(X), 
    {event(X,T)}
=> 
    writeln($event(T)).

watch_dom(X),
    {dom(X,T)}
=>
    writeln($dom(T)).

watch_dom_any(X),
    {dom_any(X,T)}
=>
    writeln($dom_any(T)).

watch_ins(X),
    {ins(X)}
=>
    writeln($ins(X)).

watch_bound(X),
    {bound(X)}
=>
    writeln($bound(X)).

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% EXCEPTIONS
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

catch_divided_by_zero =>
   catch(write(myd(4,0)),E, $handle(E)).

myd(X,Y)=X/Y.

handle(E) =>
    writeln(E),
    throw(E).  % just re-throw it

/* end file exs.pi */";

        public const string LanguageIntroduction = @"%%% Based on: http://retina.inf.ufsc.br/picat_guide/

import cp.

%%% Data Types:

test_variables => V1 = X1, V2 = _ab, V3 = _.

test_numbers => N1 = 12, N2 = 0xf3, N3 = 1.0e8.
 
test_atoms => A1 = x1, A2 = '_AB', A3 = ''.
 
test_lists => L = [a,b,c,d].
 
% [h, e, l, l, o, p, i, c, a, t]
test_strings => write(""hello""++""picat"").

% hellopicat
test_strings2 => print(""hello""++""picat"").

% hellopicat
test_formatted_write => writef(""%s"",""hello""++""picat"").

test_structure => S = $point(1.0,2.0).

% S = point(_3b0, _3b4, _3b8)
% Name = point
% Len = 3
test_structure2 => S = new_struct(point,3), Name = name(S), Len = length(S),  Name = S.name, Len = S.length.

test_array => A = {a,b,c,d}.

test_array2 => S = new_array(3), S[1] = 11, D2 = length(S), D2 = S.length.

% M = (map)[two = 2, one = 1]
test_map => M = new_map([one=1, two=2]), put(M,three,3), Three = get(M,three), M.put(one,1), One = M.get(one).

% M = (map)[two, one, three]
test_set => M = new_set([one, two, three]), put(M,three), has_key(M,three), M.put(three).

% X = [1, 3, 5, 7, 9]
test_range => X = 1..2..10.

% X = [1,2,3,4,5]
test_range2 => X = 1..5.

test_datatype_checks => integer(5), not real(5), var(X), Y=5, not var(Y), 5 != 2+2.
 
%%% Calling Conventions:

% X = ['1','0','1']
test_callingconvention1 => X = to_binary_string(5).
 
% X = b
test_callingconvention2 => L = [a,b,c,d], X = L[2].
 
% L = [(a,1),(a,2),(b,1),(b,2)]  
test_callingconvention3 => L = [(A,I) : A in [a,b], I in 1..2].  
 
% One = 1  
test_attributedVars => put_attr(X,one,1), One = get_attr(X,one).

test_chained_call => X = 5.to_binary_string().length.

% X=3.14159
test_math => X = math.pi.

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
 
test_constraint =>  
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

% X = 1;  
% X = 2;  
% X = 3;  
% no  
test_higher_order => S = $member(X), call(S,[1,2,3]).
 
% L = [1,2,3]  
test_findall => L = findall(X,member(X,[1,2,3])).

% Z = 3
test_apply => Z = apply('+',1,2), Z = 3.

%%% Prebuilt Maps:

test_prebuilt_map1 ?=>  
    get_heap_map(h1).put(one,1),  
    get_global_map(g1).put(one,1),  
    get_table_map(t1).put(one,1).

test_prebuilt_map2 =>  
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
