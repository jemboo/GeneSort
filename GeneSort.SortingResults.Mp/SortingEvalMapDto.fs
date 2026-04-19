namespace GeneSort.SortingResults.Mp

open System
open System.Collections.Generic
open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingResults
open GeneSort.Model.Mp.Sorting

[<MessagePackObject>]
type sortingEvalMapDto = {
    [<Key(0)>]
    sortingEvalDto: sortingEvalDto
    
    /// Maps SorterId (as string/Guid) to the detailed ModelTagDto
    [<Key(1)>]
    EvalMap: IDictionary<string, modelTagDto> 
}

module SortingEvalMapDto =

    let fromDomain (map: sortingEvalMap) : sortingEvalMapDto =
        let evalDict = Dictionary<string, modelTagDto>()
        
        for kvp in map.EvalMap do
            let sorterIdStr = (%kvp.Key).ToString()
            let tagDto = ModelTagDto.fromDomain kvp.Value
            evalDict.Add(sorterIdStr, tagDto)

        {
            sortingEvalDto = SortingEvalDto.fromDomain map.SortingEval
            EvalMap = evalDict
        }

    let toDomain (dto: sortingEvalMapDto) : sortingEvalMap =
        let sortingResult = SortingEvalDto.toDomain dto.sortingEvalDto
        
        let evalEntries = 
            dto.EvalMap 
            |> Seq.map (fun kvp -> 
                let sorterId = Guid.Parse(kvp.Key) |> UMX.tag<sorterId>
                let modelTag = ModelTagDto.toDomain kvp.Value
                (sorterId, modelTag)
            )

        sortingEvalMap.create sortingResult evalEntries