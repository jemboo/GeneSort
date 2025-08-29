namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter.Sorter

[<Measure>] type ceBlockLength

type ceBlock = 
    private { 
        ces: ce array 
    }

    static member create(length: int<ceBlockLength>) =
        if %length < 0 then
            invalidArg "length" "ceBlockLength must be non-negative"
        { ces = Array.zeroCreate %length }

    member this.Ces with get()  = this.ces
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