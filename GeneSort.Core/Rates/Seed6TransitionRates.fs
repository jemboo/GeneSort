namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type Seed6TransitionRates = 
    private 
        { 
            ortho1Rates: Seed6ActionRates
            ortho2Rates: Seed6ActionRates
            para1Rates: Seed6ActionRates
            para2Rates: Seed6ActionRates
            para3Rates: Seed6ActionRates
            para4Rates: Seed6ActionRates
            selfReflRates: Seed6ActionRates
        }

    static member create (ortho1Rates: Seed6ActionRates, ortho2Rates: Seed6ActionRates, para1Rates: Seed6ActionRates, 
                         para2Rates: Seed6ActionRates, para3Rates: Seed6ActionRates, para4Rates: Seed6ActionRates, 
                         selfReflRates: Seed6ActionRates) : Seed6TransitionRates =
        { 
            ortho1Rates = ortho1Rates
            ortho2Rates = ortho2Rates
            para1Rates = para1Rates
            para2Rates = para2Rates
            para3Rates = para3Rates
            para4Rates = para4Rates
            selfReflRates = selfReflRates
        }

    static member createUniform (amt: float) : Seed6TransitionRates =
        let rates = Seed6ActionRates.createUniform amt
        Seed6TransitionRates.create(rates, rates, rates, rates, rates, rates, rates)


    static member createBiased (seed6GenMode: Seed6GenMode) (baseAmt:float) (biasAmt:float) : Seed6TransitionRates =
        match seed6GenMode with
        | Seed6GenMode.Ortho1 -> 
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho1, baseAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)
        | Seed6GenMode.Ortho2 ->
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.Ortho2, baseAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)
        | Seed6GenMode.Para1 -> 
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt + biasAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.Para1, baseAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)
        | Seed6GenMode.Para2 -> 
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt + biasAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.Para2, baseAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)
        | Seed6GenMode.Para3 -> 
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt + biasAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.Para3, baseAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)
        | Seed6GenMode.Para4 -> 
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt + biasAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.Para4, baseAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)
        | Seed6GenMode.SelfRefl -> 
            let ortho1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt, biasAmt)
            let ortho2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt, biasAmt)
            let para1Rates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt, biasAmt)
            let para2Rates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt, biasAmt)
            let para3Rates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt, biasAmt)
            let para4Rates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt, biasAmt)
            let selfReflRates = Seed6ActionRates.createBiased(Seed6ActionMode.SelfRefl, baseAmt + biasAmt, biasAmt)
            Seed6TransitionRates.create(ortho1Rates, ortho2Rates, para1Rates, para2Rates, para3Rates, para4Rates, selfReflRates)


    member this.PickMode (floatPicker: unit -> float) (orbitType: Seed6TwoOrbitType) : Seed6ActionMode =
        match orbitType with
        | Seed6TwoOrbitType.Ortho1 -> this.ortho1Rates.PickMode floatPicker
        | Seed6TwoOrbitType.Ortho2 -> this.ortho2Rates.PickMode floatPicker
        | Seed6TwoOrbitType.Para1 -> this.para1Rates.PickMode floatPicker
        | Seed6TwoOrbitType.Para2 -> this.para2Rates.PickMode floatPicker
        | Seed6TwoOrbitType.Para3 -> this.para3Rates.PickMode floatPicker
        | Seed6TwoOrbitType.Para4 -> this.para4Rates.PickMode floatPicker
        | Seed6TwoOrbitType.SelfRefl -> this.selfReflRates.PickMode floatPicker


    member this.TransitionMode (floatPicker: unit -> float) (seed6GenMode: Seed6GenMode) : Seed6GenMode =
        match seed6GenMode with
        | Seed6GenMode.Ortho1 -> this.ortho1Rates.PickModeWithDefault floatPicker seed6GenMode
        | Seed6GenMode.Ortho2 -> this.ortho2Rates.PickModeWithDefault floatPicker seed6GenMode
        | Seed6GenMode.Para1 -> this.para1Rates.PickModeWithDefault floatPicker seed6GenMode
        | Seed6GenMode.Para2 -> this.para2Rates.PickModeWithDefault floatPicker seed6GenMode
        | Seed6GenMode.Para3 -> this.para3Rates.PickModeWithDefault floatPicker seed6GenMode
        | Seed6GenMode.Para4 -> this.para4Rates.PickModeWithDefault floatPicker seed6GenMode
        | Seed6GenMode.SelfRefl -> this.selfReflRates.PickModeWithDefault floatPicker seed6GenMode


    member this.toString() =
        sprintf "Seed6TransitionRates(Ortho1: %s, Ortho2: %s, Para1: %s, Para2: %s, Para3: %s, Para4: %s, SelfRefl: %s)"
                (this.ortho1Rates.toString())
                (this.ortho2Rates.toString())
                (this.para1Rates.toString())
                (this.para2Rates.toString())
                (this.para3Rates.toString())
                (this.para4Rates.toString())
                (this.selfReflRates.toString())


    member this.Ortho1Rates with get() = this.ortho1Rates
    member this.Ortho2Rates with get() = this.ortho2Rates
    member this.Para1Rates with get() = this.para1Rates
    member this.Para2Rates with get() = this.para2Rates
    member this.Para3Rates with get() = this.para3Rates
    member this.Para4Rates with get() = this.para4Rates
    member this.SelfReflRates with get() = this.selfReflRates


    override this.Equals(obj) = 
        match obj with
        | :? Seed6TransitionRates as other -> 
            this.ortho1Rates.Equals(other.ortho1Rates) &&
            this.ortho2Rates.Equals(other.ortho2Rates) &&
            this.para1Rates.Equals(other.para1Rates) &&
            this.para2Rates.Equals(other.para2Rates) &&
            this.para3Rates.Equals(other.para3Rates) &&
            this.para4Rates.Equals(other.para4Rates) &&
            this.selfReflRates.Equals(other.selfReflRates)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.ortho1Rates, this.ortho2Rates, this.para1Rates, this.para2Rates, 
              this.para3Rates, this.para4Rates, this.selfReflRates)

    interface IEquatable<Seed6TransitionRates> with
        member this.Equals(other) = 
            this.ortho1Rates.Equals(other.ortho1Rates) &&
            this.ortho2Rates.Equals(other.ortho2Rates) &&
            this.para1Rates.Equals(other.para1Rates) &&
            this.para2Rates.Equals(other.para2Rates) &&
            this.para3Rates.Equals(other.para3Rates) &&
            this.para4Rates.Equals(other.para4Rates) &&
            this.selfReflRates.Equals(other.selfReflRates)
