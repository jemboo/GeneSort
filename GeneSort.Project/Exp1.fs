
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


module Exp1 =
    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let excludeSelfCe = true
    let parameterSet = 
        [ ParamHelpers.getSortingWidths(); ParamHelpers.getSorterModels() ]

    let workspace = Workspace.create "Exp1" "Exp1" projectDir parameterSet

    let executor (workspace:Workspace) (run: Run) =
        // Simulate some work for the run
        let id =
            [
                randomType :> obj
                1 :> obj
                excludeSelfCe :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        let qua = hash (randomType, 111111111, excludeSelfCe)

        let yab = rngType.Lcg

        Console.WriteLine (sprintf "Executing Run %d   %A   %d" run.Index (%id) qua)

    let RunAll() =
        WorkspaceOps.executeWorkspace workspace 6 executor