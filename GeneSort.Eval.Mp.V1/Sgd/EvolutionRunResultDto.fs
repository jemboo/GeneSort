namespace GeneSort.Eval.Mp.V1

open System
open MessagePack
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.SorterEvolutionEngine

module EvolutionRunResultDto =

    let toDto (domain: EvolutionRunResult) : evolutionRunResultDto =
        let v1 = {
            IntermediateHistory = domain.IntermediateHistory |> Array.map SorterPoolSetDescriptionDto.toDto
            FinalPoolSet = SorterPoolSetDto.toDto domain.FinalPoolSet
        }
        evolutionRunResultDto.V1 v1