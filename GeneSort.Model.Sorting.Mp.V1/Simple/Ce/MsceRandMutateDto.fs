namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Ce

open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Core.Mp.RatesAndOps
open GeneSort.Core.Mp

type msceRandMutateDto = 
    { rngFactoryDto: rngFactoryDto
      indelRatesDto: indelRatesDto
      excludeSelfCe: bool }

module MsceRandMutateDto =
    
    let fromDomain (msceRandMutate: msceRandMutate) : msceRandMutateDto =
        { rngFactoryDto = msceRandMutate.RngFactory |> RngFactoryDto.fromDomain
          indelRatesDto = IndelRatesDto.fromDomain msceRandMutate.IndelRates
          excludeSelfCe = %msceRandMutate.ExcludeSelfCe }
    
    let toDomain (dto: msceRandMutateDto) : msceRandMutate =
        msceRandMutate.create
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (dto.indelRatesDto |> IndelRatesDto.toDomain)
            (dto.excludeSelfCe |> UMX.tag)

