namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter.Sorter

[<Measure>]
type ceIndex

[<Measure>] 
type ceBlockId

[<Measure>] 
type ceBlockLength

type ceBlock = 
    private { 
        ces: ce array 
        ceBlockId: Guid<ceBlockId>
    }

    static member create (ceBlockId: Guid<ceBlockId>) (ces: ce array) = { ceBlockId = ceBlockId; ces = ces }

    static member Empty = { ceBlockId = Guid.Empty |> UMX.tag<ceBlockId>; ces = [||] }

    member this.getCe (dex:int) = 
        //if dex < 0 || dex >= this.ces.Length then
        //    invalidArg "dex" $"Index {dex} is out of bounds for Ce array of length {this.ces.Length}."
        this.ces.[dex]

    member this.CeArray with get() = Array.copy this.ces

    member this.CeBlockId with get() = this.ceBlockId

    member this.Length with get() = this.ces.Length |> UMX.tag<ceBlockLength>


module CeBlock =

    let breakUpIntoBlocks (ces:ce[]) (blockLength:int<ceBlockLength>) =
        let totalCes = ces.Length
        let blocks = 
            [|
                let mutable index = 0
                while index < totalCes do
                    let blockCes = ces.[index .. Math.Min(index + %blockLength - 1, totalCes - 1)]
                    yield ceBlock.create (Guid.NewGuid() |> UMX.tag) blockCes
                    index <- index + %blockLength
            |]
        blocks