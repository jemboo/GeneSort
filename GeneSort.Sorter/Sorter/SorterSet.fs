
namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter

type SorterSet =
        { SorterSetId: Guid<sorterSetId>
          Sorters: Sorter array }


module SorterSet =

    let create (sorterSetId: Guid<sorterSetId>) (sorters: Sorter array) : SorterSet =
        if %sorterSetId = Guid.Empty then
            failwith "SorterSet ID must not be empty"
        else if Array.isEmpty sorters then
            failwith "Sorter set must contain at least one sorter"
        else
            { SorterSetId = sorterSetId; Sorters = sorters }


    let createWithNewId (sorters: Sorter array) : SorterSet =
        create (UMX.tag<sorterSetId> (Guid.NewGuid())) sorters

    let toString (sorterSet: SorterSet) : string =
        let sortersStr = sorterSet.Sorters |> Array.map Sorter.toString |> String.concat "; "
        sprintf "SorterSet(Id=%A, Sorters=[%s])" (%sorterSet.SorterSetId) sortersStr
