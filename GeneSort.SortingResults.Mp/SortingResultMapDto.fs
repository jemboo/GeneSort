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

    let toDto (map: sortingResultMap) : sortingResultMapDto =
        let evalDict = Dictionary<string, modelTagDto>()
        
        for kvp in map.EvalMap do
            let sorterIdStr = (%kvp.Key).ToString()
            let tagDto = ModelTagDto.toDto kvp.Value
            evalDict.Add(sorterIdStr, tagDto)

        {
            SortingResult = SortingResultDto.toDto map.SortingResult
            EvalMap = evalDict
        }

    let fromDto (dto: sortingResultMapDto) : sortingResultMap =
        let sortingResult = SortingResultDto.fromDto dto.SortingResult
        
        let evalEntries = 
            dto.EvalMap 
            |> Seq.map (fun kvp -> 
                let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
                let modelTag = ModelTagDto.fromDto kvp.Value
                (sorterId, modelTag)
            )

        sortingResultMap.create sortingResult evalEntries