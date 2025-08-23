namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Sorter



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
                msasORandGen.getMsasOs(firstIndex)
                    |> Seq.take(count)
