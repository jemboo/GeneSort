namespace GeneSort.Core
open System

type OpsActionMode =
    | NoAction
    | Ortho
    | Para
    | SelfRefl


[<Struct; CustomEquality; NoComparison>]
type OpsActionRates = 
    private 
        { orthoThresh: float; paraThresh: float; selfSymThresh: float; }

    static member create (orthoRate: float, paraRate: float, selfSymRate: float) : OpsActionRates =
        let noAction = 1.0 - orthoRate - paraRate - selfSymRate
        let epsilon = 1e-10
        if orthoRate < 0.0 || orthoRate > 1.0 then failwith "orthoRate must be between 0 and 1"
        else if paraRate < 0.0 || paraRate > 1.0 then failwith "paraRate must be between 0 and 1"
        else if noAction < -epsilon then failwith "Sum of SiMutationRates rates must not exceed 1.0"
        {
            orthoThresh = orthoRate
            paraThresh = orthoRate + paraRate
            selfSymThresh = orthoRate + paraRate + selfSymRate
        }

    static member createUniform (amt:float) : OpsActionRates =
            OpsActionRates.create(amt, amt, amt)

    static member createBiased(opsActionMode: OpsActionMode, baseAmt:float, biasAmt: float) : OpsActionRates =
        match opsActionMode with
        | OpsActionMode.Ortho -> OpsActionRates.create(baseAmt + biasAmt, baseAmt - (biasAmt / 2.0), baseAmt - (biasAmt / 2.0))
        | OpsActionMode.Para -> OpsActionRates.create(baseAmt - (biasAmt / 2.0), baseAmt + biasAmt, baseAmt - (biasAmt / 2.0))
        | OpsActionMode.SelfRefl -> OpsActionRates.create(baseAmt - (biasAmt / 2.0), baseAmt - (biasAmt / 2.0), baseAmt + biasAmt)
        | OpsActionMode.NoAction -> failwith "NoAction mode is not valid for OpsActionRates"


    member this.OrthoRate with get() = this.orthoThresh
    member this.ParaRate with get() = this.paraThresh - this.orthoThresh
    member this.SelfReflRate with get() = this.selfSymThresh - this.paraThresh
    member this.NoActionRate with get() = 1.0 - this.selfSymThresh
    member this.toString() =
        sprintf "OpsActionRates(Ortho: %.2f, Para: %.2f, SelfSym: %.2f)" 
                this.OrthoRate this.ParaRate this.SelfReflRate


    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : OpsActionMode =
        let r = floatPicker()
        if r < this.orthoThresh then
            OpsActionMode.Ortho
        elif r < this.paraThresh then
            OpsActionMode.Para
        elif r < this.selfSymThresh then
            OpsActionMode.SelfRefl
        else
        OpsActionMode.NoAction


    member this.PickModeWithDefault 
                (opsGenMode:OpsGenMode) 
                (floatPicker: unit -> float) : OpsGenMode =

        let r = floatPicker()
        if r < this.orthoThresh then
            OpsGenMode.Ortho
        elif r < this.paraThresh then
            OpsGenMode.Para
        elif r < this.selfSymThresh then
            OpsGenMode.SelfRefl
        else
            opsGenMode


    override this.Equals(obj) = 
        match obj with
        | :? OpsActionRates as other -> 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfSymThresh = other.selfSymThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoThresh, this.paraThresh)

    interface IEquatable<OpsActionRates> with
        member this.Equals(other) = 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfSymThresh = other.selfSymThresh


