namespace GeneSort.Core
open System

type Seed6GenMode =
    | Ortho1
    | Ortho2
    | Para1
    | Para2
    | Para3
    | Para4
    | SelfRefl


[<Struct; CustomEquality; NoComparison>]
type Seed6GenRates = 
    private 
        { 
            ortho1Thresh: float
            ortho2Thresh: float
            para1Thresh: float
            para2Thresh: float
            para3Thresh: float
            para4Thresh: float
            selfReflThresh: float
        }

    static member create (ortho1Rate: float, ortho2Rate: float, para1Rate: float, para2Rate: float, 
                         para3Rate: float, para4Rate: float, selfReflRate: float) : Seed6GenRates =
        let sum = ortho1Rate + ortho2Rate + para1Rate + para2Rate + para3Rate + para4Rate + selfReflRate
        let epsilon = 1e-10
        if ortho1Rate < 0.0 || ortho1Rate > 1.0 then failwith "ortho1Rate must be between 0 and 1"
        else if ortho2Rate < 0.0 || ortho2Rate > 1.0 then failwith "ortho2Rate must be between 0 and 1"
        else if para1Rate < 0.0 || para1Rate > 1.0 then failwith "para1Rate must be between 0 and 1"
        else if para2Rate < 0.0 || para2Rate > 1.0 then failwith "para2Rate must be between 0 and 1"
        else if para3Rate < 0.0 || para3Rate > 1.0 then failwith "para3Rate must be between 0 and 1"
        else if para4Rate < 0.0 || para4Rate > 1.0 then failwith "para4Rate must be between 0 and 1"
        else if selfReflRate < 0.0 || selfReflRate > 1.0 then failwith "selfReflRate must be between 0 and 1"
        else if abs (sum - 1.0) > epsilon then failwith "Sum of Seed6GenRates rates must equal 1.0"
        {
            ortho1Thresh = ortho1Rate
            ortho2Thresh = ortho1Rate + ortho2Rate
            para1Thresh = ortho1Rate + ortho2Rate + para1Rate
            para2Thresh = ortho1Rate + ortho2Rate + para1Rate + para2Rate
            para3Thresh = ortho1Rate + ortho2Rate + para1Rate + para2Rate + para3Rate
            para4Thresh = ortho1Rate + ortho2Rate + para1Rate + para2Rate + para3Rate + para4Rate
            selfReflThresh = ortho1Rate + ortho2Rate + para1Rate + para2Rate + para3Rate + para4Rate + selfReflRate
        }

    static member createUniform () : Seed6GenRates =
        let rate = 1.0 / 7.0
        Seed6GenRates.create(rate, rate, rate, rate, rate, rate, rate)

    static member createBiased(field: string, bias: float) : Seed6GenRates =
        if bias < 0.0 || bias > 1.0 then failwith "bias must be between 0 and 1"
        let remaining = (1.0 - bias) / 6.0
        match field.ToLower() with
        | "ortho1" -> Seed6GenRates.create(bias, remaining, remaining, remaining, remaining, remaining, remaining)
        | "ortho2" -> Seed6GenRates.create(remaining, bias, remaining, remaining, remaining, remaining, remaining)
        | "para1" -> Seed6GenRates.create(remaining, remaining, bias, remaining, remaining, remaining, remaining)
        | "para2" -> Seed6GenRates.create(remaining, remaining, remaining, bias, remaining, remaining, remaining)
        | "para3" -> Seed6GenRates.create(remaining, remaining, remaining, remaining, bias, remaining, remaining)
        | "para4" -> Seed6GenRates.create(remaining, remaining, remaining, remaining, remaining, bias, remaining)
        | "selfrefl" -> Seed6GenRates.create(remaining, remaining, remaining, remaining, remaining, remaining, bias)
        | _ -> failwith "Invalid field name for createBiased"

    member this.Ortho1Rate with get() = this.ortho1Thresh
    member this.Ortho2Rate with get() = this.ortho2Thresh - this.ortho1Thresh
    member this.Para1Rate with get() = this.para1Thresh - this.ortho2Thresh
    member this.Para2Rate with get() = this.para2Thresh - this.para1Thresh
    member this.Para3Rate with get() = this.para3Thresh - this.para2Thresh
    member this.Para4Rate with get() = this.para4Thresh - this.para3Thresh
    member this.SelfReflRate with get() = this.selfReflThresh - this.para4Thresh

    member this.toString() =
        sprintf "Seed6GenRates(Ortho1: %.2f, Ortho2: %.2f, Para1: %.2f, Para2: %.2f, Para3: %.2f, Para4: %.2f, SelfRefl: %.2f)" 
                this.Ortho1Rate this.Ortho2Rate this.Para1Rate this.Para2Rate this.Para3Rate this.Para4Rate this.SelfReflRate

    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : Seed6GenMode =
        let r = floatPicker()
        if r < this.ortho1Thresh then Ortho1
        elif r < this.ortho2Thresh then Ortho2
        elif r < this.para1Thresh then Para1
        elif r < this.para2Thresh then Para2
        elif r < this.para3Thresh then Para3
        elif r < this.para4Thresh then Para4
        else SelfRefl

    override this.Equals(obj) = 
        match obj with
        | :? Seed6GenRates as other -> 
            this.ortho1Thresh = other.ortho1Thresh &&
            this.ortho2Thresh = other.ortho2Thresh &&
            this.para1Thresh = other.para1Thresh &&
            this.para2Thresh = other.para2Thresh &&
            this.para3Thresh = other.para3Thresh &&
            this.para4Thresh = other.para4Thresh &&
            this.selfReflThresh = other.selfReflThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.ortho1Thresh, this.ortho2Thresh, this.para1Thresh, this.para2Thresh, 
              this.para3Thresh, this.para4Thresh, this.selfReflThresh)

    interface IEquatable<Seed6GenRates> with
        member this.Equals(other) = 
            this.ortho1Thresh = other.ortho1Thresh &&
            this.ortho2Thresh = other.ortho2Thresh &&
            this.para1Thresh = other.para1Thresh &&
            this.para2Thresh = other.para2Thresh &&
            this.para3Thresh = other.para3Thresh &&
            this.para4Thresh = other.para4Thresh &&
            this.selfReflThresh = other.selfReflThresh


module Seed6GenMode =

    let fromSeed6TwoOrbitType (seed6TwoOrbitType:TwoOrbitTripleType) : Seed6GenMode =
            match seed6TwoOrbitType with 
            | TwoOrbitTripleType.Ortho1 -> Seed6GenMode.Ortho1
            | TwoOrbitTripleType.Ortho2 -> Seed6GenMode.Ortho2
            | TwoOrbitTripleType.Para1 -> Seed6GenMode.Para1
            | TwoOrbitTripleType.Para2 -> Seed6GenMode.Para2
            | TwoOrbitTripleType.Para3 -> Seed6GenMode.Para3
            | TwoOrbitTripleType.Para4 -> Seed6GenMode.Para4
            | TwoOrbitTripleType.SelfRefl -> Seed6GenMode.SelfRefl
            

    let toSeed6TwoOrbitType (seed6GenMode:Seed6GenMode) : TwoOrbitTripleType =
            match seed6GenMode with 
            | Seed6GenMode.Ortho1 -> TwoOrbitTripleType.Ortho1
            | Seed6GenMode.Ortho2 -> TwoOrbitTripleType.Ortho2
            | Seed6GenMode.Para1 -> TwoOrbitTripleType.Para1
            | Seed6GenMode.Para2 -> TwoOrbitTripleType.Para2
            | Seed6GenMode.Para3 -> TwoOrbitTripleType.Para3
            | Seed6GenMode.Para4 -> TwoOrbitTripleType.Para4
            | Seed6GenMode.SelfRefl -> TwoOrbitTripleType.SelfRefl
