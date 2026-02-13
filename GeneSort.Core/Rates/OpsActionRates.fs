namespace GeneSort.Core
open System

type opsActionMode =
    | NoAction
    | Ortho
    | Para
    | SelfRefl


[<Struct; CustomEquality; NoComparison>]
type opsActionRates = 
    private 
        { orthoThresh: float; paraThresh: float; selfSymThresh: float; }

    static member create (orthoRate: float, paraRate: float, selfSymRate: float) : opsActionRates =
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

    static member createUniform (amt:float) : opsActionRates =
            opsActionRates.create(amt, amt, amt)

    static member createBiased(opsActionMode: opsActionMode, baseAmt:float, biasAmt: float) : opsActionRates =
        match opsActionMode with
        | opsActionMode.Ortho -> opsActionRates.create(baseAmt + biasAmt, baseAmt - (biasAmt / 2.0), baseAmt - (biasAmt / 2.0))
        | opsActionMode.Para -> opsActionRates.create(baseAmt - (biasAmt / 2.0), baseAmt + biasAmt, baseAmt - (biasAmt / 2.0))
        | opsActionMode.SelfRefl -> opsActionRates.create(baseAmt - (biasAmt / 2.0), baseAmt - (biasAmt / 2.0), baseAmt + biasAmt)
        | opsActionMode.NoAction -> failwith "NoAction mode is not valid for OpsActionRates"


    member this.OrthoRate with get() = this.orthoThresh
    member this.ParaRate with get() = this.paraThresh - this.orthoThresh
    member this.SelfReflRate with get() = this.selfSymThresh - this.paraThresh
    member this.NoActionRate with get() = 1.0 - this.selfSymThresh
    member this.toString() =
        sprintf "OpsActionRates(Ortho: %.2f, Para: %.2f, SelfSym: %.2f)" 
                this.OrthoRate this.ParaRate this.SelfReflRate


    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : opsActionMode =
        let r = floatPicker()
        if r < this.orthoThresh then
            opsActionMode.Ortho
        elif r < this.paraThresh then
            opsActionMode.Para
        elif r < this.selfSymThresh then
            opsActionMode.SelfRefl
        else
        opsActionMode.NoAction


    member this.PickModeWithDefault 
                (opgm:opsGenMode) 
                (floatPicker: unit -> float) : opsGenMode =

        let r = floatPicker()
        if r < this.orthoThresh then
            opsGenMode.Ortho
        elif r < this.paraThresh then
            opsGenMode.Para
        elif r < this.selfSymThresh then
            opsGenMode.SelfRefl
        else
            opgm


    override this.Equals(obj) = 
        match obj with
        | :? opsActionRates as other -> 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfSymThresh = other.selfSymThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoThresh, this.paraThresh)

    interface IEquatable<opsActionRates> with
        member this.Equals(other) = 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh &&
            this.selfSymThresh = other.selfSymThresh


