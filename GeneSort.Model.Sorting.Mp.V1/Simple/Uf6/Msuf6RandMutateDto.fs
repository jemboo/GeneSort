
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Uf6

open System
open FSharp.UMX
open GeneSort.Model.Mp.Sorter.Uf6
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Core.Mp

type msuf6RandMutateDto =
    { id: Guid
      rngFactoryDto: rngFactoryDto
      uf6MutationRates: uf6MutationRatesDto }

module Msuf6RandMutateDto =

    let fromDomain (msuf6RandMutate: msuf6RandMutate) : msuf6RandMutateDto =
        { id = %msuf6RandMutate.Id
          rngFactoryDto = msuf6RandMutate.RngFactory |> RngFactoryDto.fromDomain
          uf6MutationRates = Uf6MutationRatesDto.fromDomain msuf6RandMutate.Uf6MutationRates }

    let toDomain (dto: msuf6RandMutateDto) : msuf6RandMutate =
        try
            let uf6MutationRates = Uf6MutationRatesDto.toDomain dto.uf6MutationRates
            let rngFactory = dto.rngFactoryDto |> RngFactoryDto.toDomain
            msuf6RandMutate.create rngFactory uf6MutationRates
        with
        | ex -> failwith $"Failed to convert Msuf6RandMutateDto: {ex.Message}"

