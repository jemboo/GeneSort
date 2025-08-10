namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type OpsTransitionRates = 
    private 
        { orthoRates: OpsActionRates
          paraRates: OpsActionRates
          selfReflRates: OpsActionRates }

    static member create (orthoRates: OpsActionRates, paraRates: OpsActionRates, selfReflRates: OpsActionRates) : OpsTransitionRates =
        { 
            orthoRates = orthoRates
            paraRates = paraRates
            selfReflRates = selfReflRates
        }

    static member createUniform (amt: float) : OpsTransitionRates =
        let rates = OpsActionRates.createUniform amt
        OpsTransitionRates.create(rates, rates, rates)


    static member createBiased (twoOrbitType: TwoOrbitType) (baseAmt:float) (biasAmt:float) : OpsTransitionRates =
        match twoOrbitType with
        | TwoOrbitType.Ortho -> 
            let orthoRates = OpsActionRates.createBiased(OpsActionMode.Ortho, baseAmt, biasAmt)
            let paraRates = OpsActionRates.createBiased(OpsActionMode.Ortho, baseAmt, biasAmt)
            let selfReflRates = OpsActionRates.createBiased(OpsActionMode.Ortho, baseAmt, biasAmt)
            OpsTransitionRates.create(orthoRates, paraRates, selfReflRates)
        | TwoOrbitType.Para -> 
            let orthoRates = OpsActionRates.createBiased(OpsActionMode.Para, baseAmt, biasAmt)
            let paraRates = OpsActionRates.createBiased(OpsActionMode.Para, baseAmt + biasAmt, biasAmt)
            let selfReflRates = OpsActionRates.createBiased(OpsActionMode.Para, baseAmt, biasAmt)
            OpsTransitionRates.create(orthoRates, paraRates, selfReflRates)
        | TwoOrbitType.SelfRefl -> 
            let orthoRates = OpsActionRates.createBiased(OpsActionMode.SelfRefl, baseAmt, biasAmt)
            let paraRates = OpsActionRates.createBiased(OpsActionMode.SelfRefl, baseAmt, biasAmt)
            let selfReflRates = OpsActionRates.createBiased(OpsActionMode.SelfRefl, baseAmt, biasAmt)
            OpsTransitionRates.create(orthoRates, paraRates, selfReflRates)


    member this.OrthoRates with get() = this.orthoRates
    member this.ParaRates with get() = this.paraRates
    member this.SelfReflRates with get() = this.selfReflRates

    member this.PickMode (floatPicker: unit -> float) (twoOrbitType: TwoOrbitType) : OpsActionMode =
        match twoOrbitType with
        | TwoOrbitType.Ortho -> this.orthoRates.PickMode floatPicker
        | TwoOrbitType.Para -> this.paraRates.PickMode floatPicker
        | TwoOrbitType.SelfRefl -> this.selfReflRates.PickMode floatPicker


    member this.TransitionMode (floatPicker: unit -> float) (opsGenMode : OpsGenMode) : OpsGenMode =
        match opsGenMode with 
        | OpsGenMode.Ortho -> this.orthoRates.PickModeWithDefault opsGenMode floatPicker
        | OpsGenMode.Para -> this.paraRates.PickModeWithDefault opsGenMode floatPicker
        | OpsGenMode.SelfRefl -> this.selfReflRates.PickModeWithDefault opsGenMode floatPicker


    member this.toString() =
        sprintf "TwoOrbitPairActionRates(Ortho: %s, Para: %s, SelfRefl: %s)"
                (this.orthoRates.toString())
                (this.paraRates.toString())
                (this.selfReflRates.toString())

    override this.Equals(obj) = 
        match obj with
        | :? OpsTransitionRates as other -> 
            this.orthoRates.Equals(other.orthoRates) &&
            this.paraRates.Equals(other.paraRates) &&
            this.selfReflRates.Equals(other.selfReflRates)
        | _ -> false

    override this.GetHashCode() = 
        hash (this.orthoRates, this.paraRates, this.selfReflRates)

    interface IEquatable<OpsTransitionRates> with
        member this.Equals(other) = 
            this.orthoRates.Equals(other.orthoRates) &&
            this.paraRates.Equals(other.paraRates) &&
            this.selfReflRates.Equals(other.selfReflRates)