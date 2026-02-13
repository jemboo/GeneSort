namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type opsTransitionRates = 
    private 
        { orthoRates: opsActionRates
          paraRates: opsActionRates
          selfReflRates: opsActionRates }

    static member create (orthoRates: opsActionRates, paraRates: opsActionRates, selfReflRates: opsActionRates) : opsTransitionRates =
        { 
            orthoRates = orthoRates
            paraRates = paraRates
            selfReflRates = selfReflRates
        }

    static member createUniform (amt: float) : opsTransitionRates =
        let rates = opsActionRates.createUniform amt
        opsTransitionRates.create(rates, rates, rates)


    static member createBiased (twoOrbitType: TwoOrbitType) (baseAmt:float) (biasAmt:float) : opsTransitionRates =
        match twoOrbitType with
        | TwoOrbitType.Ortho -> 
            let orthoRates = opsActionRates.createBiased(opsActionMode.Ortho, baseAmt, biasAmt)
            let paraRates = opsActionRates.createBiased(opsActionMode.Ortho, baseAmt, biasAmt)
            let selfReflRates = opsActionRates.createBiased(opsActionMode.Ortho, baseAmt, biasAmt)
            opsTransitionRates.create(orthoRates, paraRates, selfReflRates)
        | TwoOrbitType.Para -> 
            let orthoRates = opsActionRates.createBiased(opsActionMode.Para, baseAmt, biasAmt)
            let paraRates = opsActionRates.createBiased(opsActionMode.Para, baseAmt + biasAmt, biasAmt)
            let selfReflRates = opsActionRates.createBiased(opsActionMode.Para, baseAmt, biasAmt)
            opsTransitionRates.create(orthoRates, paraRates, selfReflRates)
        | TwoOrbitType.SelfRefl -> 
            let orthoRates = opsActionRates.createBiased(opsActionMode.SelfRefl, baseAmt, biasAmt)
            let paraRates = opsActionRates.createBiased(opsActionMode.SelfRefl, baseAmt, biasAmt)
            let selfReflRates = opsActionRates.createBiased(opsActionMode.SelfRefl, baseAmt, biasAmt)
            opsTransitionRates.create(orthoRates, paraRates, selfReflRates)


    member this.OrthoRates with get() = this.orthoRates
    member this.ParaRates with get() = this.paraRates
    member this.SelfReflRates with get() = this.selfReflRates

    member this.PickMode (floatPicker: unit -> float) (twoOrbitType: TwoOrbitType) : opsActionMode =
        match twoOrbitType with
        | TwoOrbitType.Ortho -> this.orthoRates.PickMode floatPicker
        | TwoOrbitType.Para -> this.paraRates.PickMode floatPicker
        | TwoOrbitType.SelfRefl -> this.selfReflRates.PickMode floatPicker


    member this.TransitionMode (floatPicker: unit -> float) (opsGenMode : opsGenMode) : opsGenMode =
        match opsGenMode with 
        | opsGenMode.Ortho -> this.orthoRates.PickModeWithDefault opsGenMode floatPicker
        | opsGenMode.Para -> this.paraRates.PickModeWithDefault opsGenMode floatPicker
        | opsGenMode.SelfRefl -> this.selfReflRates.PickModeWithDefault opsGenMode floatPicker


    member this.toString() =
        sprintf "TwoOrbitPairActionRates(Ortho: %s, Para: %s, SelfRefl: %s)"
                (this.orthoRates.toString())
                (this.paraRates.toString())
                (this.selfReflRates.toString())

    override this.Equals(obj) = 
        match obj with
        | :? opsTransitionRates as other -> 
            this.orthoRates.Equals(other.orthoRates) &&
            this.paraRates.Equals(other.paraRates) &&
            this.selfReflRates.Equals(other.selfReflRates)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoRates, this.paraRates, this.selfReflRates)

    interface IEquatable<opsTransitionRates> with
        member this.Equals(other) = 
            this.orthoRates.Equals(other.orthoRates) &&
            this.paraRates.Equals(other.paraRates) &&
            this.selfReflRates.Equals(other.selfReflRates)