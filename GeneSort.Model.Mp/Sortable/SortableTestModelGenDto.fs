namespace GeneSort.Model.Mp.Sortable

open MessagePack
open GeneSort.Model.Sortable


[<MessagePackObject>]
type sortableTestModelGenDto = {
    [<Key(0)>] Kind: int // 0 = MsasORandGen
    [<Key(1)>] MsasORandGen: msasORandGenDto // used when Kind=0
}


module SortableTestModelGenDto =

    let fromDomain (gen: sortableTestModelGen) : sortableTestModelGenDto =
        match gen with
        | sortableTestModelGen.MsasORandGen msas ->
            { Kind = 0; MsasORandGen = MsasORandGenDto.fromDomain msas }


    let toDomain (dto: sortableTestModelGenDto) : sortableTestModelGen =
        match dto.Kind with
        | 0 -> sortableTestModelGen.MsasORandGen (MsasORandGenDto.toDomain dto.MsasORandGen)
        | k -> failwith (sprintf "Unknown SorterTestModelGenDto.Kind = %d" dto.Kind)
