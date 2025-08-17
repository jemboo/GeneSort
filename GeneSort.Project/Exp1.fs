
namespace GeneSort.Project

open System
open System.IO
open System.Threading
open System.Threading.Tasks

open FSharp.UMX
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers

open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model
open GeneSort.Model.Sorter
open GeneSort.Project.Params


module Exp1 =
    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let excludeSelfCe = true
    let parameterSet = 
        [ swFull.standardMapVals(); SorterModels.standardMapVals() ]

    let workspace = Workspace.create "Exp1" "Exp1" projectDir parameterSet

    let executor (workspace:Workspace) (cycle: int<cycleNumber>) (run: Run) =
        
        //let swFull = run.Parameters |> Map.find "swFull" |> UMX.untag<swFull>
        //let sorterModel = run.Parameters |> Map.find "sorterModel" |> UMX.untag<sorterModel>

        Console.WriteLine (sprintf "Executing Run %d   %A " run.Index run.Parameters)

    let RunAll() =
        let cycle = 1<cycleNumber>
        WorkspaceOps.executeWorkspace workspace cycle 6 executor