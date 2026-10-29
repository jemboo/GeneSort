namespace GeneSort.SortingOps.Mp

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Mp.Sorter
open GeneSort.SortingOps

type ceBlockDto = {
    CeBlockId: Guid
    ChannelCount: int
    Ces: ceDto array
}

module CeBlockDto =
    let toCeBlockDto (ceBlock: ceBlock) : ceBlockDto =
        { 
            CeBlockId = %ceBlock.CeBlockId
            Ces = ceBlock.CeArray |> Array.map CeDto.fromDomain
            ChannelCount = %ceBlock.ChannelCount
        }

    let fromCeBlockDto (dto: ceBlockDto) : ceBlock =
        ceBlock.create 
            (dto.CeBlockId |> UMX.tag<ceBlockId>) 
            (dto.ChannelCount |> UMX.tag<channelCount>)
            (dto.Ces |> Array.map CeDto.toDomain)