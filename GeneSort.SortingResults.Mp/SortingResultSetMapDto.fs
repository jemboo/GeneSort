namespace GeneSort.SortingResults.Mp

open System
open System.Collections.Generic
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Mp.Sorting
open GeneSort.SortingResults

[<MessagePackObject>]
type sortingEvalSetMapDto = {
    /// Maps SortingId string to its respective result DTO
    [<Key(0)>]
    SortingEvalMap: IDictionary<string, sortingEvalDto>
    
    /// Maps SorterId string to the modelSetTag DTO
    [<Key(1)>]
    EvalMap: IDictionary<string, modelSetTagDto>
}

module SortingEvalSetMapDto =

    let fromDomain (resultSet: sortingEvalSetMap) : sortingEvalSetMapDto =
        // 1. Convert the SortingResultMap
        let resultMapDto = Dictionary<string, sortingEvalDto>()
        for kvp in resultSet.SortingEvalsMap do
            resultMapDto.Add((%kvp.Key).ToString(), SortingEvalDto.fromDomain kvp.Value)

        // 2. Convert the EvalMap
        let evalMapDto = Dictionary<string, modelSetTagDto>()
        for kvp in resultSet.EvalMap do
            evalMapDto.Add((%kvp.Key).ToString(), ModelSetTagDto.fromDomain kvp.Value)

        {
            SortingEvalMap = resultMapDto
            EvalMap = evalMapDto
        }

    let toDomain (dto: sortingEvalSetMapDto) : sortingEvalSetMap =
        // Reconstruct SortingResults sequence
        let sortingEvals = 
            dto.SortingEvalMap.Values 
            |> Seq.map SortingEvalDto.toDomain

        // Reconstruct EvalEntries sequence (Guid<sorterId> * modelSetTag)
        let evalEntries = 
            dto.EvalMap 
            |> Seq.map (fun kvp -> 
                let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
                let modelSetTag = ModelSetTagDto.toDomain kvp.Value
                (sorterId, modelSetTag)
            )

        sortingEvalSetMap.create sortingEvals evalEntries