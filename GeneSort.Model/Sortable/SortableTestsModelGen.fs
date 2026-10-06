namespace GeneSort.Model.Sortable
open GeneSort.Sorting
open FSharp.UMX


// 1. Define the DU case right at the top so everything below knows it exists
type sortableTestsModelGen =
     | MsasORandGen of msasORandGen

module SortableTestsModelGen =

    let makeSorterTestModels 
                (firstIndex: int) 
                (count: int) 
                (gen: sortableTestsModelGen) : 
                sortableTestsModel seq =
        match gen with
        | MsasORandGen msasORandGen ->
                msasORandGen.getMsasOs(firstIndex) |> Seq.take(count)

    let getId (gen: sortableTestsModelGen) : Guid<sorterTestModelGenId> =
        match gen with
        | MsasORandGen msasORandGen -> msasORandGen.Id  
