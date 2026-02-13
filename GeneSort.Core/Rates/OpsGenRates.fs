namespace GeneSort.Core
open System

type opsGenMode =
| Ortho
| Para
| SelfRefl


[<Struct; CustomEquality; NoComparison>]
type opsGenRates = 
    private 
        { orthoThresh: float; paraThresh: float; selfReflThresh: float }

    static member create (orthoRate: float, paraRate: float, selfSymRate: float) : opsGenRates =
        let sum = orthoRate + paraRate + selfSymRate
        let epsilon = 1e-10
        if orthoRate < 0.0 || orthoRate > 1.0 then failwith "orthoRate must be between 0 and 1"
        else if paraRate < 0.0 || paraRate > 1.0 then failwith "paraRate must be between 0 and 1"
        else if selfSymRate < 0.0 || selfSymRate > 1.0 then failwith "selfSymRate must be between 0 and 1"
        else if abs (sum - 1.0) > epsilon then failwith "Sum of OpsGenRates rates must equal 1.0"
        {
            orthoThresh = orthoRate
            paraThresh = orthoRate + paraRate
            selfReflThresh = orthoRate + paraRate + selfSymRate
        }

    static member createUniform () : opsGenRates =
        opsGenRates.create(1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0)

    static member createBiased(opsGenMode: opsGenMode, bias: float) : opsGenRates =
        match opsGenMode with
        | opsGenMode.Ortho -> opsGenRates.create(bias, (1.0 - bias) / 2.0, (1.0 - bias) / 2.0)
        | opsGenMode.Para -> opsGenRates.create((1.0 - bias) / 2.0, bias, (1.0 - bias) / 2.0)
        | opsGenMode.SelfRefl -> opsGenRates.create((1.0 - bias) / 2.0, (1.0 - bias) / 2.0, bias)


    member this.OrthoRate with get() = this.orthoThresh
    member this.ParaRate with get() = this.paraThresh - this.orthoThresh
    member this.SelfReflRate with get() = this.selfReflThresh - this.paraThresh
    member this.toString() =
        sprintf "OpsGenRates(Ortho: %.2f, Para: %.2f, SelfSym: %.2f)" 
                this.OrthoRate this.ParaRate this.SelfReflRate

    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : opsGenMode =
        let r = floatPicker()
        if r < this.orthoThresh then
            opsGenMode.Ortho
        elif r < this.paraThresh then
            opsGenMode.Para
        else
            opsGenMode.SelfRefl

    override this.Equals(obj) = 
        match obj with
        | :? opsGenRates as other -> 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfReflThresh = other.selfReflThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoThresh, this.paraThresh, this.selfReflThresh)

    interface IEquatable<opsGenRates> with
        member this.Equals(other) = 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfReflThresh = other.selfReflThresh


module OpsGenMode =

    let fromTwoOrbitType (twoOrbitType:TwoOrbitPairType) : opsGenMode =
            match twoOrbitType with
            | TwoOrbitPairType.Ortho -> opsGenMode.Ortho
            | TwoOrbitPairType.Para -> opsGenMode.Para
            | TwoOrbitPairType.SelfRefl ->  opsGenMode.SelfRefl

    let toTwoOrbitPairType (opsGenMode:opsGenMode) : TwoOrbitPairType =
            match opsGenMode with
            | opsGenMode.Ortho -> TwoOrbitPairType.Ortho
            | opsGenMode.Para -> TwoOrbitPairType.Para
            | opsGenMode.SelfRefl ->  TwoOrbitPairType.SelfRefl

