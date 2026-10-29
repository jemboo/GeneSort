namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter

type ceBlock = 
    private { 
        ces: ce array 
        ceBlockId: Guid<ceBlockId>
        channelCount: int<channelCount>
    }

    static member create 
            (ceBlockId: Guid<ceBlockId>) 
            (channelCount: int<channelCount>)
            (ces: ce array)  = 
            { 
                ceBlockId = ceBlockId; 
                ces = ces;
                channelCount = channelCount
            }

    static member Empty = ceBlock.create (Guid.Empty |> UMX.tag<ceBlockId>) (0 |> UMX.tag<channelCount>) [||]

    member this.getCe (dex:int) = 
        //if dex < 0 || dex >= this.ces.Length then
        //    invalidArg "dex" $"Index {dex} is out of bounds for Ce array of length {this.ces.Length}."
        this.ces.[dex]

    member this.CeArray with get() = Array.copy this.ces

    member this.CeBlockId with get() = this.ceBlockId

    member this.CeLength with get() = this.ces.Length |> UMX.tag<ceLength>

    member this.ChannelCount with get() = this.channelCount

    member this.permute (perm:permutation) : ceBlock =
        ceBlock.create 
            (Guid.NewGuid() |> UMX.tag)
            this.channelCount
            (this.ces |> Array.map(Ce.permute perm))


module CeBlock =

    let breakUpIntoBlocks (channelCount: int<channelCount>) (ces:ce[]) (blockLength:int<ceLength>) =
        let totalCes = ces.Length
        let blocks = 
            [|
                let mutable index = 0
                while index < totalCes do
                    let blockCes = ces.[index .. Math.Min(index + %blockLength - 1, totalCes - 1)]
                    yield ceBlock.create (Guid.NewGuid() |> UMX.tag) channelCount blockCes
                    index <- index + %blockLength
            |]
        blocks


    let fromSorterSuffix (sorter:sorter) (offset:int<ceLength>) =
        ceBlock.create 
            (UMX.tag<ceBlockId> (Guid.NewGuid())) 
            sorter.ChannelCount
            (sorter.Ces.[%offset .. %sorter.ceLength - 1 |> int])


    let fromSorter (sorter:sorter) =
        ceBlock.create 
            (UMX.tag<ceBlockId> (Guid.NewGuid())) 
            sorter.ChannelCount
            sorter.Ces