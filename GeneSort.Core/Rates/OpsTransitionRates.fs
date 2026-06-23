namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type opsTransitionRates = 
    private 
        { orthoRates: opsActionRates
          paraRates: opsActionRates
          selfReflRates: opsActionRates }

    static member create (
                orthoRates: opsActionRates, 
                paraRates: opsActionRates, 
                selfReflRates: opsActionRates) : opsTransitionRates =
        { 
            orthoRates = orthoRates
            paraRates = paraRates
            selfReflRates = selfReflRates
        }


    static member createUniform (amt: float) : opsTransitionRates =
        let rates = opsActionRates.createUniform amt
        opsTransitionRates.create(rates, rates, rates)

    static member createUniform2 (rates: opsActionRates) : opsTransitionRates =
        opsTransitionRates.create(rates, rates, rates)

    static member createBiased (twoOrbitType: twoOrbitType) (baseAmt:float) (biasAmt:float) : opsTransitionRates =
        match twoOrbitType with
        | twoOrbitType.Ortho -> 
            let orthoRates = opsActionRates.createBiased(opsActionMode.Ortho, baseAmt, biasAmt)
            let paraRates = opsActionRates.createBiased(opsActionMode.Ortho, baseAmt, biasAmt)
            let selfReflRates = opsActionRates.createBiased(opsActionMode.Ortho, baseAmt, biasAmt)
            opsTransitionRates.create(orthoRates, paraRates, selfReflRates)
        | twoOrbitType.Para -> 
            let orthoRates = opsActionRates.createBiased(opsActionMode.Para, baseAmt, biasAmt)
            let paraRates = opsActionRates.createBiased(opsActionMode.Para, baseAmt + biasAmt, biasAmt)
            let selfReflRates = opsActionRates.createBiased(opsActionMode.Para, baseAmt, biasAmt)
            opsTransitionRates.create(orthoRates, paraRates, selfReflRates)
        | twoOrbitType.SelfRefl -> 
            let orthoRates = opsActionRates.createBiased(opsActionMode.SelfRefl, baseAmt, biasAmt)
            let paraRates = opsActionRates.createBiased(opsActionMode.SelfRefl, baseAmt, biasAmt)
            let selfReflRates = opsActionRates.createBiased(opsActionMode.SelfRefl, baseAmt, biasAmt)
            opsTransitionRates.create(orthoRates, paraRates, selfReflRates)


    member this.OrthoRates with get() = this.orthoRates
    member this.ParaRates with get() = this.paraRates
    member this.SelfReflRates with get() = this.selfReflRates

    member this.PickMode (floatPicker: unit -> float) (twoOrbitType: twoOrbitType) : opsActionMode =
        match twoOrbitType with
        | twoOrbitType.Ortho -> this.orthoRates.PickMode floatPicker
        | twoOrbitType.Para -> this.paraRates.PickMode floatPicker
        | twoOrbitType.SelfRefl -> this.selfReflRates.PickMode floatPicker


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
            // 1. Extract the already-stable hashes from your individual component fields
            let h1 = this.orthoRates.GetHashCode()
            let h2 = this.paraRates.GetHashCode()
            let h3 = this.selfReflRates.GetHashCode()

            // 2. Combine them using the exact same deterministic Knuth-style multiplier algorithm
            let mutable hash = 17
            hash <- hash * 23 + h1
            hash <- hash * 23 + h2
            hash <- hash * 23 + h3
            hash

    interface IEquatable<opsTransitionRates> with
        member this.Equals(other) = 
            this.orthoRates.Equals(other.orthoRates) &&
            this.paraRates.Equals(other.paraRates) &&
            this.selfReflRates.Equals(other.selfReflRates)