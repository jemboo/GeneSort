namespace GeneSort.SortingResults.Mp

open System
open System.Collections.Generic
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Mp.Sorting
open GeneSort.SortingResults

[<MessagePackObject>]
type sortingResultSetMapDto = {
    /// Maps SortingId string to its respective result DTO
    [<Key(0)>]
    SortingResultMap: IDictionary<string, sortingResultDto>
    
    /// Maps SorterId string to the modelSetTag DTO
    [<Key(1)>]
    EvalMap: IDictionary<string, modelSetTagDto>
}

module SortingResultSetMapDto =

    let fromDomain (resultSet: sortingResultSetMap) : sortingResultSetMapDto =
        // 1. Convert the SortingResultMap
        let resultMapDto = Dictionary<string, sortingResultDto>()
        for kvp in resultSet.SortingResultMap do
            resultMapDto.Add((%kvp.Key).ToString(), SortingResultDto.fromDomain kvp.Value)

        // 2. Convert the EvalMap
        let evalMapDto = Dictionary<string, modelSetTagDto>()
        for kvp in resultSet.EvalMap do
            evalMapDto.Add((%kvp.Key).ToString(), ModelSetTagDto.fromDomain kvp.Value)

        {
            SortingResultMap = resultMapDto
            EvalMap = evalMapDto
        }

    let toDomain (dto: sortingResultSetMapDto) : sortingResultSetMap =
        // Reconstruct SortingResults sequence
        let sortingResults = 
            dto.SortingResultMap.Values 
            |> Seq.map SortingResultDto.toDomain

        // Reconstruct EvalEntries sequence (Guid<sorterId> * modelSetTag)
        let evalEntries = 
            dto.EvalMap 
            |> Seq.map (fun kvp -> 
                let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
                let modelSetTag = ModelSetTagDto.toDomain kvp.Value
                (sorterId, modelSetTag)
            )

        sortingResultSetMap.create sortingResults evalEntries