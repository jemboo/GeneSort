
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
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model
open GeneSort.Model.Sortable
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
    let sortableArrayType = SortableArrayType.Ints
    let testModelCount = 10<sorterTestModelCount>
    let parameterSet = 
        [ SwFull.standardMapVals(); SorterTestModels.maxOrbit() ]

    let workspace = Workspace.create "Exp2" "Exp2" projectDir parameterSet

    let executor (workspace: Workspace) (cycle: int<cycleNumber>) (run: Run) : Async<unit> =
        async {
            try
                Console.WriteLine(sprintf "Executing Run %d   %A" run.Index run.Parameters)

                let sortingWidth =
                    match run.Parameters.TryGetValue("SortingWidth") with
                    | true, value -> value |> SwFull.fromString |> SwFull.toSortingWidth
                    | false, _ -> failwith "SortingWidth parameter not found"
                let maxOrbiit =
                    match Int32.TryParse(run.Parameters["MaxOrbiit"]) with
                    | true, value -> value
                    | false, _ -> failwith "Invalid MaxOrbiit value"

                let firstIndex = (%cycle * %testModelCount) |> UMX.tag<sorterTestModelCount>
                let sorterTestModelGen = MsasORandGen.create randomType sortingWidth maxOrbiit |> SorterTestModelGen.MsasORandGen
                let sorterTestModelSetMaker = SorterTestModelSetMaker.create sorterTestModelGen firstIndex testModelCount
                let sorterTestModelSet = sorterTestModelSetMaker.MakeSorterTestModelSet
                let sorterTestSet = sorterTestModelSet.makeSorterTestSet sortableArrayType
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestSet |> OutputData.SorterTestSet)
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestModelSet |> OutputData.SorterTestModelSet)
                do! OutputData.saveToFile workspace.WorkspaceFolder run.Index run.Cycle (sorterTestModelSetMaker |> OutputData.SorterTestModelSetMaker)

                Console.WriteLine(sprintf "Finished executing Run %d  Cycle  %d \n" run.Index %cycle)
            with ex ->
                Console.WriteLine(sprintf "Error in Run %d, Cycle %d: %s" run.Index %cycle ex.Message)
                raise ex
        }


    let RunAll() =
        for i in 1 .. 2 do
            let cycle = i |> UMX.tag<cycleNumber>
            WorkspaceOps.executeWorkspace workspace cycle 6 executor