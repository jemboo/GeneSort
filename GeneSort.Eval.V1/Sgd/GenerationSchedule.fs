namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1

module GenerationSchedule =
    /// Adjusts at generation zero and every configured interval thereafter.
    let shouldAdjustCeLengths
            (frequency: int<adjustCeLengthFrequency>)
            (generation: int<generationNumber>) =
        if %frequency <= 0 then
            invalidArg "frequency" "AdjustCeLengthFrequency must be greater than zero."
        if %generation < 0 then
            invalidArg "generation" "Generation must not be negative."
        %generation % %frequency = 0
