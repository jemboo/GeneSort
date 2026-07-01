namespace GeneSort.Core
open LanguagePrimitives
open System

type blockSize =
    | X1
    | X5
    | C1
    | C5
    | K1
    | K5
    | K10
    | K50
    | K100
    | K500
    | M1


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

    member this.FirstStep with get() = this.firstStep
    member this.Offsets with get() = this.offsets
    member this.OffsetsInOrder with get() = this.offsets |> Array.sort




//module StepBlock =

//    let expSample (firstStep:int) (lastStep:int) = ()

//type intSteps =
//    | Uniform of uniformSteps

//module IntSteps =

//    let yab (s: Set<int>) =
//        s |> Set.toSeq |> Seq.sortDescending
