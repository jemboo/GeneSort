namespace GeneSort.Core
open System

type OpActionMode =
    | NoAction
    | Ortho
    | Para


[<Struct; CustomEquality; NoComparison>]
type OpActionRates = 
    private 
        { orthoThresh: float; paraThresh: float; }

    static member create (orthoRate: float, paraRate: float) : OpActionRates =
        let noAction = 1.0 - orthoRate - paraRate
        let epsilon = 1e-10
        if orthoRate < 0.0 || orthoRate > 1.0 then failwith "orthoRate must be between 0 and 1"
        else if paraRate < 0.0 || paraRate > 1.0 then failwith "paraRate must be between 0 and 1"
        else if noAction < -epsilon then failwith "Sum of OpActionRates rates must not exceed 1.0"
        {
            orthoThresh = orthoRate
            paraThresh = orthoRate + paraRate
        }

    static member createUniform (amt:float) : OpActionRates =
        OpActionRates.create(amt, amt)

    static member createBiased(opActionMode: OpActionMode, baseAmt:float, biasAmt: float) : OpActionRates =
        match opActionMode with
        | OpActionMode.Ortho -> OpActionRates.create(baseAmt + biasAmt, baseAmt - biasAmt)
        | OpActionMode.Para -> OpActionRates.create(baseAmt - biasAmt, baseAmt + biasAmt)
        | OpActionMode.NoAction -> failwith "NoAction mode is not valid for OpActionRates"


    member this.OrthoRate with get() = this.orthoThresh
    member this.ParaRate with get() = this.paraThresh - this.orthoThresh
    member this.NoActionRate with get() = 1.0 - this.paraThresh
    member this.toString() =
        sprintf "OpActionRates(Ortho: %.2f, Para: %.2f)" 
                this.OrthoRate this.ParaRate    

    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : OpActionMode =
        let r = floatPicker()
        if r < this.orthoThresh then
            OpActionMode.Ortho
        elif r < this.paraThresh then
            OpActionMode.Para
        else
        OpActionMode.NoAction

    override this.Equals(obj) = 
        match obj with
        | :? OpActionRates as other -> 
            this.orthoThresh = other.orthoThresh &&
            this.paraThresh = other.paraThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoThresh, this.paraThresh)

    interface IEquatable<indelRates> with
        member this.Equals(other) = 
            this.orthoThresh = other.mutationThresh &&
            this.paraThresh = other.insertionThresh


