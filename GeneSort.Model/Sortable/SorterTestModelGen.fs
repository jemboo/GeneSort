namespace GeneSort.Model.Sortable

type SorterTestModelGen =
     | MsasORandGen of MsasORandGen


module SorterTestModelGen =

    let makeSorterTestModels 
                (firstIndex: int) 
                (count: int) 
                (gen: SorterTestModelGen) : 
                SorterTestModel seq =
        match gen with
        | MsasORandGen msasORandGen ->
                msasORandGen.getMsasOs(firstIndex) |> Seq.take(count)
