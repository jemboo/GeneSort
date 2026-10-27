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
    /// Initialize the execution origin and progress; recorded file references stay untouched.
    let initializeExecution (start: evoStart) (parameters: runParameters) =
        parameters.WithGenerationFirst(Some start.FirstGeneration)
                  .WithGenerationCurrent(Some start.FirstGeneration)
                  .WithRunFinished(Some false)
