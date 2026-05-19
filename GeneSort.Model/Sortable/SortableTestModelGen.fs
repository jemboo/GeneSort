namespace GeneSort.Model.Sortable
open GeneSort.Sorting
open FSharp.UMX


// 1. Define the DU case right at the top so everything below knows it exists
type sortableTestModelGen =
     | MsasORandGen of msasORandGen

module SortableTestModelGen =

    let makeSorterTestModels 
                (firstIndex: int) 
                (count: int) 
                (gen: sortableTestModelGen) : 
                sortableTestModel seq =
        match gen with
        | MsasORandGen msasORandGen ->
                msasORandGen.getMsasOs(firstIndex) |> Seq.take(count)

    let getId (gen: sortableTestModelGen) : Guid<sorterTestModelGenId> =
        match gen with
        | MsasORandGen msasORandGen -> msasORandGen.Id  
