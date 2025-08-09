namespace GeneSort.Sorter

open System
open FSharp.UMX


type Sorter =
        { SorterId: Guid<sorterId>
          Width: int<sortingWidth>
          Ces: Ce array }


// Core module for Sorter operations
module Sorter =

    let create (sorterId: Guid<sorterId>) (width: int<sortingWidth>) (ces: Ce array) : Sorter =
        if %sorterId = Guid.Empty then
            failwith "Sorter ID must not be empty"
        else if %width < 1 then
            failwith "Width must be at least 1"
        else
            { SorterId = sorterId; Width = width; Ces = ces }

    let createWithNewId (width: int<sortingWidth>) (ces: Ce array) : Sorter =
        create (UMX.tag<sorterId> (Guid.NewGuid())) width ces

    let toString (sorter: Sorter) : string =
        let cesStr = sorter.Ces |> Array.map Ce.toString |> String.concat "; "
        sprintf "Sorter(Id=%A, Width=%d, Ces=[%s])" (%sorter.SorterId) (%sorter.Width) cesStr


