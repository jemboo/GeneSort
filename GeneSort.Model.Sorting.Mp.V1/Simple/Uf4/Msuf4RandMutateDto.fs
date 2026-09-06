
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf4

open System
open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Core.Mp
open GeneSort.Model.Mp.Sorter.Uf4

type msuf4RandMutateDto =
    { id: Guid
      rngFactoryDto: rngFactoryDto
      uf4MutationRatesArrayDtos: uf4MutationRatesDto }

module Msuf4RandMutateDto =

    let fromDomain (msuf4RandMutate: msuf4RandMutate) : msuf4RandMutateDto =
        { id = %msuf4RandMutate.Id
          rngFactoryDto = msuf4RandMutate.RngFactory |> RngFactoryDto.fromDomain
          uf4MutationRatesArrayDtos = Uf4MutationRatesDto.fromDomain msuf4RandMutate.Uf4MutationRates }

    let toDomain (dto: msuf4RandMutateDto) : msuf4RandMutate =
        msuf4RandMutate.create 
            (dto.rngFactoryDto |> RngFactoryDto.toDomain)
            (dto.uf4MutationRatesArrayDtos |> Uf4MutationRatesDto.toDomain)

