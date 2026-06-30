namespace GeneSort.Core
open LanguagePrimitives
open System

type uniformSteps = {
    firstStep: int
    stepSize: int
    stepCount: int
}

type stepBlock = 
    private {
        firstStep: int
        offsets: int[]
    }
    static member create (firstStep:int) (offsets: int[]) : stepBlock =
        {
            stepBlock.firstStep = firstStep
            stepBlock.offsets = offsets
        }


//module StepBlock =

//    let expSample (firstStep:int) (lastStep:int) = ()

//type intSteps =
//    | Uniform of uniformSteps

//module IntSteps =

//    let yab (s: Set<int>) =
//        s |> Set.toSeq |> Seq.sortDescending
