namespace GeneSort.Model.Mp.Sortable

open System
open GeneSort.Model.Sorter
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sortable
open GeneSort.Core.Mp
open GeneSort.Sorter


[<MessagePackObject>]
type SorterTestModelGenDto = {
    [<Key(0)>] Kind: int // 0 = MsasORandGen
    [<Key(1)>] MsasORandGen: MsasORandGenDto // used when Kind=0
}


module SorterTestModelGenDto =

    let toDtoSorterTestModelGen (gen: SorterTestModelGen) : SorterTestModelGenDto =
        match gen with
        | SorterTestModelGen.MsasORandGen msas ->
            { Kind = 0; MsasORandGen = MsasORandGenDto.fromDomain msas }


    let fromDtoSorterTestModelGen (dto: SorterTestModelGenDto) : SorterTestModelGen =
        match dto.Kind with
        | 0 -> SorterTestModelGen.MsasORandGen (MsasORandGenDto.toDomain dto.MsasORandGen)
        | k -> failwith "Unknown SorterTestModelGenDto.Kind = %d" 
