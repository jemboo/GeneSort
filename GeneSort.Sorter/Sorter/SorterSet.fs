
namespace GeneSort.Sorter.Sorter

open System
open FSharp.UMX
open GeneSort.Sorter

type SorterSet =
        { SorterSetId: Guid<sorterSetId>
          Sorters: sorter array }


module SorterSet =

    let create (sorterSetId: Guid<sorterSetId>) (sorters: sorter array) : SorterSet =
        if %sorterSetId = Guid.Empty then
            failwith "SorterSet ID must not be empty"
        else if Array.isEmpty sorters then
            failwith "Sorter set must contain at least one sorter"
        else
            { SorterSetId = sorterSetId; Sorters = sorters }

    let createWithNewId (sorters: sorter array) : SorterSet =
        create (UMX.tag<sorterSetId> (Guid.NewGuid())) sorters

    let toString (sorterSet: SorterSet) : string =
        let sortersStr = sorterSet.Sorters |> Array.map(fun sorter -> sorter.ToString()) |> String.concat "; "
        sprintf "SorterSet(Id=%A, Sorters=[%s])" (%sorterSet.SorterSetId) sortersStr
