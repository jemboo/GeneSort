namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter.Sorter

[<Measure>] type ceBlockLength

type ceBlock = 
    private { 
        ces: ce array 
    }

    static member create(ces: ce array ) = { ces = ces }

    member this.getCe (dex:int) = 
        if dex < 0 || dex >= this.ces.Length then
            invalidArg "dex" $"Index {dex} is out of bounds for Ce array of length {this.ces.Length}."
        this.ces.[dex]

    member this.Length with get() = this.ces.Length |> UMX.tag<ceBlockLength>


module CeBlock =

    let breakUpIntoBlocks (ces:ce[]) (blockLength:int<ceBlockLength>) =
        let totalCes = ces.Length
        let blocks = 
            [|
                let mutable index = 0
                while index < totalCes do
                    let blockCes = ces.[index .. Math.Min(index + %blockLength - 1, totalCes - 1)]
                    yield { ces = blockCes }
                    index <- index + %blockLength
            |]
        blocks