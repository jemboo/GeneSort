namespace GeneSort.Core
open System

type OpsGenMode =
| Ortho
| Para
| SelfRefl


[<Struct; CustomEquality; NoComparison>]
type OpsGenRates = 
    private 
        { orthoThresh: float; paraThresh: float; selfReflThresh: float }

    static member create (orthoRate: float, paraRate: float, selfSymRate: float) : OpsGenRates =
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

    static member createUniform () : OpsGenRates =
        OpsGenRates.create(1.0 / 3.0, 1.0 / 3.0, 1.0 / 3.0)

    static member createBiased(opsGenMode: OpsGenMode, bias: float) : OpsGenRates =
        match opsGenMode with
        | OpsGenMode.Ortho -> OpsGenRates.create(bias, (1.0 - bias) / 2.0, (1.0 - bias) / 2.0)
        | OpsGenMode.Para -> OpsGenRates.create((1.0 - bias) / 2.0, bias, (1.0 - bias) / 2.0)
        | OpsGenMode.SelfRefl -> OpsGenRates.create((1.0 - bias) / 2.0, (1.0 - bias) / 2.0, bias)


    member this.OrthoRate with get() = this.orthoThresh
    member this.ParaRate with get() = this.paraThresh - this.orthoThresh
    member this.SelfReflRate with get() = this.selfReflThresh - this.paraThresh
    member this.toString() =
        sprintf "OpsGenRates(Ortho: %.2f, Para: %.2f, SelfSym: %.2f)" 
                this.OrthoRate this.ParaRate this.SelfReflRate

    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : OpsGenMode =
        let r = floatPicker()
        if r < this.orthoThresh then
            OpsGenMode.Ortho
        elif r < this.paraThresh then
            OpsGenMode.Para
        else
            OpsGenMode.SelfRefl

    override this.Equals(obj) = 
        match obj with
        | :? OpsGenRates as other -> 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfReflThresh = other.selfReflThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoThresh, this.paraThresh, this.selfReflThresh)

    interface IEquatable<OpsGenRates> with
        member this.Equals(other) = 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfReflThresh = other.selfReflThresh


module OpsGenMode =

    let fromTwoOrbitType (twoOrbitType:TwoOrbitType) : OpsGenMode =
            match twoOrbitType with
            | TwoOrbitType.Ortho -> OpsGenMode.Ortho
            | TwoOrbitType.Para -> OpsGenMode.Para
            | TwoOrbitType.SelfRefl ->  OpsGenMode.SelfRefl

    let toTwoOrbitType (opsGenMode:OpsGenMode) : TwoOrbitType =
            match opsGenMode with
            | OpsGenMode.Ortho -> TwoOrbitType.Ortho
            | OpsGenMode.Para -> TwoOrbitType.Para
            | OpsGenMode.SelfRefl ->  TwoOrbitType.SelfRefl

