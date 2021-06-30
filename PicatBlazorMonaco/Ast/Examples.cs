using System.Collections.Generic;

namespace PicatBlazorMonaco.Ast
{
    public class Examples
    {
        public static List<(string, string)> Samples = new List<(string, string)> 
        { 
            ("About this editor", About),
            ("Picat language introduction", LanguageIntroduction),
        };

        public const string About = @"Welcome
You can define custom links to this editor by inserting your code as UrlEncoded value of the 'code' parameter like in the below example:
https://localhost:5001?code=foo+%3D%3E+bar.%0D%0Abar+%3D%3E+println(hello).
";

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

        public const string Utils = @"/* Several examples in Picat 
*/
/**** begin file exs.pi 
****/
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
    Tree2 = tree_inst2(),
    println(inorder(Tree2)).

% An example that uses data constructors
% A term in the form of $f(X) is a data constructor
divide_main =>
   Exp = $((((((((x/x)/x)/x)/x)/x)/x)/x)/x)/x,
   d(Exp, x, D),
   writeln(D).

d(U+V, X, D) =>
    D = $DU+DV,
    d(U, X, DU),
    d(V, X, DV).
d(U-V, X, D) =>
    D = $DU-DV,
    d(U, X, DU),
    d(V, X, DV).
d(U* V, X, D) =>
    D = $DU* V+U* DV,
     d(U, X, DU),
     d(V, X, DV).
d(U/V, X, D) =>
    D = $(DU* V-U* DV)/(^(V,2)),
    d(U, X, DU),
    d(V, X, DV).
d(^(U, N), X, D) =>
    D = $DU* N*(^(U, N1)),
    integer(N),
    N1 = N-1,
    d(U, X, DU).
d(-U, X, D) =>
    D = $-DU,
    d(U, X, DU).
d(exp(U), X, D) =>
    D = $exp(U)*DU,
    d(U, X, DU).
d(log(U), X, D) =>
    D = $DU/U,
    d(U, X, DU).
d(X, X, D) => D = 1.
d(_, _, D) => D=0.

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

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% LIST COMPREHENSION
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% a list comprehension inside another list comprehension
% Picat> L=list_of_lists(5)
% L = [[1],[1,2] [1,2,3],[1,2,3,4],[1,2,3,4,5]]
list_of_lists(N) = [[Y: Y in 1..X] : X in 1..N].   

% another definition
another_list_of_lists(N) = [1..X : X in 1..N].   

qsort([]) = [].
qsort([H|T]) = qsort([E: E in T, E =< H])++[H]++qsort([E: E in T, E>H]).

power_set([]) = [[]].
power_set([H|T]) = P1++P2 =>
    P1 = power_set(T),
    P2 = [[H|S] : S in P1].

% generate permutations
perm([]) = [[]].
perm(Lst) = [[E|P] : E in Lst, P in perm(Lst.delete(E))].

%another definition
perm1([]) = [[]].
perm1([H|T]) = [insert(P, I, H) : P in Ps, I in 1..P.length+1] => Ps = perm1(T).

% A* B = C
matrix_multi(A, B) = C =>
    C = new_array(A.length, B[1].length),
    foreach(I in 1..A.length, J in 1..B[1].length)
        C[I, J] = sum([A[I, K]* B[K, J] : K in 1..A[1].length])
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
     L = [I: I in 2..N, var(A[I])].

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% TABLING
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% mode-directed tabling
% finding shortest paths on a graph given by the relation edge/3.
table(+,+,-, min)
sp(X, Y, Path, W) ?=>
    Path=[(X, Y)],
    edge(X, Y, W).
sp(X, Y, Path, W) =>
    Path = [(X, Z) | PathR],
    edge(X, Z, W1),
    sp(Z, Y, PathR, W2),
    W = W1+W2.

index(+,-,-) (+,+,-)
edge(1,2,1).
edge(1,3,2).
edge(2,3,3).
edge(3,2,4).

% binomial coefficient
bc(_N,0) = 1.
bc(N, N) = 1.
bc(N,1) = N.
bc(N, K) = bc(N-1, K-1) + bc(N-1, K).

% computing the minimal editing distance of two given lists
table(+,+, min)
edit([], [], D) => D = 0.
edit([X | Xs],[X|Ys],D) =>   % copy
edit(Xs, Ys, D).
edit(Xs, [_Y|Ys], D) ?=>      % insert
      edit(Xs, Ys, D1),
    D = D1+1.
edit([_X|Xs], Ys, D) =>       % delete
    edit(Xs, Ys, D1),
    D = D1+1.

% the Farmer's problem (use planner)
farmer =>
    S0 = [s, s, s, s],
    plan(S0, Plan),
    println(Plan).

final([n, n, n, n]) => true.

action([F, F, G, C],S1,Action,ActionCost) ?=>
    Action = farmer_wolf,
    ActionCost = 1,        
    opposite(F, F1),
    S1 = [F1, F1, G, C],
    not unsafe (S1).
action([F, W, F, C], S1, Action, ActionCost) ?=>
    Action = farmer_goat,
    ActionCost = 1,        
    opposite(F, F1),
    S1 = [F1, W, F1, C],
    not unsafe (S1).
action([F, W, G, F], S1, Action, ActionCost) ?=>
    Action = farmer_cabbage,
    ActionCost = 1,        
    opposite(F, F1),
    S1 = [F1, W, G, F1],
    not unsafe (S1).
action([F, W, G, C], S1, Action, ActionCost) =>
    Action = farmer_alone,
    ActionCost = 1,        
    opposite(F, F1),
    S1 = [F1, W, G, C],
    not unsafe (S1).

index(+,-) (-,+)
opposite(n, s).
opposite(s, n).

unsafe ([F, W, G, _C]),W == G,F !== W => true.
unsafe ([F, _W, G, C]),G == C,F !== G => true.

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% CONSTRAINT PROGRAMS(using cp)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%

% SEND+MORE=MONEY
sendmory =>
    Vars = [S, E, N, D, M, O, R, Y],    % generate variables
    Vars:: 0..9,
    all_different(Vars),       % generate constraints
    S #!= 0,
    M #!= 0,
    1000* S+100* E+10* N+D+1000* M+100* O+10* R+E
#= 10000*M+1000*O+100*N+10*E+Y,
    solve(Vars),               %  search
    writeln(Vars).

% N-queens
queens(N) =>
    Qs = new_array(N),
    Qs:: 1..N,
    foreach (I in 1..N-1, J in I+1..N)
        Qs[I] #!= Qs[J],
        abs(Qs[I]-Qs[J]) #!= J-I
    end,
    solve([ff], Qs),
    writeln(Qs).

% another program for N-queens
queens2(N, Q) =>
    Q = new_list(N),
    Q:: 1..N,
    Q2 = [$Q[I]+I : I in 1..N],
    Q3 = [$Q[I]-I : I in 1..N],
    all_different(Q),
    all_different(Q2),
    all_different(Q3),       
    solve([ff], Q).

% graph coloring(reuse edge/2 defined above)
color(NV, NC) =>
    A = new_array(NV),
    A:: 1..NC,
    foreach(I in 1..NV-1, J in I+1..NV)
        if edge(I, J); edge(J, I) then
                A[I] #!= A[J]
        end
    end,
    solve(A),
    writeln(A).

% a 0-1 integer model for graph coloring
bcolor(NV, NC) =>
    A = new_array(NV, NC),
    A:: [0, 1],
    foreach(I in 1..NV)
        sum([A[I, K] : K in 1..NC]) #= 1
    end,
    foreach(I in 1..NV-1, J in I+1..NV)
        if edge(I, J); edge(J, I) then
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

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% HIGHER-ORDER(not recommended because of poor performance)
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
% Picat> my_map(-, [1,2,3]) = L
% L = [-1,-2,-3]
% Picat> my_map(+, [1,2,3], [4,5,6]) = L
% L = [5,6,7]
% Picat> my_fold(+,0, [1,2,3]) = S
% S = 6

my_map(_F, []) = [].
my_map(F, [X|Xs]) = [apply(F, X)|my_map(F, Xs)].

my_map(_F, [], []) = [].
my_map(F, [X|Xs], [Y|Ys]) = [apply(F, X, Y)|my_map(F, Xs, Ys)].

my_fold(_F, Acc, []) = Acc.
  my_fold(F, Acc,[H | T]) = my_fold(F, apply(F, H, Acc), T).

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
    {event(X, T)}
    =>
    writeln($event(T)).

watch_dom(X),
    { dom(X, T)}
    =>
    writeln($dom(T)).

watch_dom_any(X),
    { dom_any(X, T)}
    =>
    writeln($dom_any(T)).

watch_ins(X),
    { ins(X)}
    =>
    writeln($ins(X)).

watch_bound(X),
    { bound(X)}
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
    throw(E).  % just re-throw it";
    }
}
