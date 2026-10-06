namespace GeneSort.Model.Mp.Sortable

open MessagePack
open GeneSort.Model.Sortable


[<MessagePackObject>]
type sortableTestsModelGenDto = {
    [<Key(0)>] Kind: int // 0 = MsasORandGen
    [<Key(1)>] MsasORandGen: msasORandGenDto // used when Kind=0
}


module SortableTestsModelGenDto =

    let fromDomain (gen: sortableTestsModelGen) : sortableTestsModelGenDto =
        match gen with
        | sortableTestsModelGen.MsasORandGen msas ->
            { Kind = 0; MsasORandGen = MsasORandGenDto.fromDomain msas }


    let toDomain (dto: sortableTestsModelGenDto) : sortableTestsModelGen =
        match dto.Kind with
        | 0 -> sortableTestsModelGen.MsasORandGen (MsasORandGenDto.toDomain dto.MsasORandGen)
        | k -> failwith (sprintf "Unknown SorterTestModelGenDto.Kind = %d" dto.Kind)
