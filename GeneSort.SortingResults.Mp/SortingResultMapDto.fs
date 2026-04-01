namespace GeneSort.SortingResults.Mp

open System
open System.Collections.Generic
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingResults
open GeneSort.Model.Mp.Sorting

[<MessagePackObject>]
type sortingResultMapDto = {
    [<Key(0)>]
    SortingResult: sortingResultDto
    
    /// Maps SorterId (as string/Guid) to the detailed ModelTagDto
    [<Key(1)>]
    EvalMap: IDictionary<string, modelTagDto> 
}

module SortingResultMapDto =

    let fromDomain (map: sortingResultMap) : sortingResultMapDto =
        let evalDict = Dictionary<string, modelTagDto>()
        
        for kvp in map.EvalMap do
            let sorterIdStr = (%kvp.Key).ToString()
            let tagDto = ModelTagDto.fromDomain kvp.Value
            evalDict.Add(sorterIdStr, tagDto)

        {
            SortingResult = SortingResultDto.fromDomain map.SortingResult
            EvalMap = evalDict
        }

    let toDomain (dto: sortingResultMapDto) : sortingResultMap =
        let sortingResult = SortingResultDto.toDomain dto.SortingResult
        
        let evalEntries = 
            dto.EvalMap 
            |> Seq.map (fun kvp -> 
                let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
                let modelTag = ModelTagDto.toDomain kvp.Value
                (sorterId, modelTag)
            )

        sortingResultMap.create sortingResult evalEntries