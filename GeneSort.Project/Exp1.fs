
namespace GeneSort.Project


open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.IO
open System.Threading
open System.Threading.Tasks


module Exp1 =
    let projectDir = "c:\Projects"
    let parameterSet = 
        [ ("SorterModel", ["Mcse"; "Mssi"; "Msrs"; "Msuf4"])
          ("SortingWidth", ["8"; "16"; "32"; "64"]) ]

    let workspace = Workspace.create "Exp1" "Exp1" projectDir parameterSet

    let executor (workspace:Workspace) (run: Run) =
        Console.WriteLine (sprintf "Executing Run %d" run.Index)

    let RunAll() =
        WorkspaceOps.executeWorkspace workspace 6 executor