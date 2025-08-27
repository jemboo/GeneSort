namespace GeneSort.Sorter.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open System.Linq
open System.Collections.Generic

[<Measure>] type ceBlockSize


type ceBlock = { ces: Ce array }


module CeBlock =

    //let getSingleCeBlock (sorter:Sorter) =
    //    { ces = sorter.Ces }
        

    let breakUpIntoBlocks (ces:Ce[]) (blockSize:int<ceBlockSize>) =
        let totalCes = ces.Length
        let blocks = 
            [|
                let mutable index = 0
                while index < totalCes do
                    let blockCes = ces.[index .. Math.Min(index + %blockSize - 1, totalCes - 1)]
                    yield { ces = blockCes }
                    index <- index + %blockSize
            |]
        blocks