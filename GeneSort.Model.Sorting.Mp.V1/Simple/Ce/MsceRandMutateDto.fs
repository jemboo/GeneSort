namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce

open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Ce
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

[<MessagePackObject>]
type msceRandMutateDto = 
    { [<Key(0)>] rngFactoryDto: rngFactoryDto
      [<Key(1)>] indelRatesDto: indelRatesDto
      [<Key(2)>] excludeSelfCe: bool }

module MsceRandMutateDto =
    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)
    
    let fromDomain (msceRandMutate: msceRandMutate) : msceRandMutateDto =
        { 
          rngFactoryDto = msceRandMutate.RngFactory |> RngFactoryDto.fromDomain
          indelRatesDto = IndelRatesDto.fromDomain msceRandMutate.IndelRates
          excludeSelfCe = %msceRandMutate.ExcludeSelfCe }
    
    let toDomain (dto: msceRandMutateDto) : msceRandMutate =
        msceRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (dto.indelRatesDto |> IndelRatesDto.toDomain)
            (dto.excludeSelfCe |> UMX.tag)