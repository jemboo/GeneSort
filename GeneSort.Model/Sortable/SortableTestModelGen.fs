namespace GeneSort.Model.Sortable

type sortableTestModelGen =
     | MsasORandGen of MsasORandGen


module SortableTestModelGen =

    let makeSorterTestModels 
                (firstIndex: int) 
                (count: int) 
                (gen: sortableTestModelGen) : 
                sortableTestModel seq =
        match gen with
        | MsasORandGen msasORandGen ->
                msasORandGen.getMsasOs(firstIndex) |> Seq.take(count)
