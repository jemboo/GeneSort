
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
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter.Si
open GeneSort.Model.Sorter.Uf4
open GeneSort.Model.Sorter.Rs
open GeneSort.Model.Sortable
open GeneSort.Model.Mp.Sortable


module Exp2 = 

    let projectDir = "c:\Projects"
    let randomType = rngType.Lcg
    let excludeSelfCe = true
    let testModelCount = 10<sorterTestModelCount>
    let parameterSet = 
        [ SwFull.standardMapVals(); SorterTestModels.maxOrbit() ]

    let workspace = Workspace.create "Exp2" "Exp2" projectDir parameterSet

    let executor (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {
            let swFull = (run.Parameters["SortingWidth"]) |> SwFull.fromString
            let sortingWidth = swFull |> SwFull.toSortingWidth
            let maxOrbiit = Int32.Parse((run.Parameters["MaxOrbiit"]))
            let firstIndex = (%cycle * %testModelCount) |> UMX.tag<sorterTestModelCount>
            let sorterTestModelGen = MsasORandGen.create randomType (sortingWidth) maxOrbiit |> SorterTestModelGen.MsasORandGen
            let sorterTestModelSetMaker = SorterTestModelSetMaker.create sorterTestModelGen firstIndex testModelCount
            let sorterTestModelSetMakerDto = SorterTestModelSetMakerDto.fromDomain sorterTestModelSetMaker
            Console.WriteLine $"Executing run for cycle {cycle} with parameters: {run.Parameters}"

        }


    let RunAll() =
        for i in 1 .. 2 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 6 executor