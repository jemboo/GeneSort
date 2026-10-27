namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Project.V1.Parameters

/// Two prepared Setup files and the generation contained in the initial population.
/// Creating or importing these files is independent of executing an evoPath.
type evoStart = {
    Tests: queryParams
    Pool: queryParams
    FirstGeneration: int<generationNumber>
}

module EvoStart =
    let attach (start: evoStart) (parameters: runParameters) =
        SetupInputReferences.recordFiles [start.Tests; start.Pool] parameters
        |> fun rp -> rp.WithGenerationFirst(Some start.FirstGeneration)
                        .WithGenerationCurrent(Some start.FirstGeneration)
                        .WithRunFinished(Some false)
