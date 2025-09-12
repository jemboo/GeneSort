namespace GeneSort.Core
open System
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type uf6MutationRatesArray =
    private 
        { rates: uf6MutationRates array }

    static member create (rates: uf6MutationRates array) : uf6MutationRatesArray =
        if Array.exists (fun r -> r.order < 6 || r.order % 2 <> 0) rates then
            failwith "All Uf6MutationRates orders must be at least 6 and even"
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (
            fun r -> sprintf "Uf6MutationRates(order=%d, seed=%s, opsTransitionRates=%s)" 
                        r.order 
                        (r.seed6TransitionRates.toString()) 
                        (r.opsTransitionRates.toString())) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? uf6MutationRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seed6TransitionRates.Equals(b.seed6TransitionRates) && 
                    a.opsTransitionRates.Equals(b.opsTransitionRates)) this.rates other.rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.rates do
            hash <- hash * 23 + rate.order.GetHashCode()
            hash <- hash * 23 + rate.seed6TransitionRates.GetHashCode()
            hash <- hash * 23 + rate.opsTransitionRates.GetHashCode()
        hash

    interface IEquatable<uf6MutationRatesArray> with
        member this.Equals(other) =
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seed6TransitionRates.Equals(b.seed6TransitionRates) && 
                    a.opsTransitionRates.Equals(b.opsTransitionRates)) this.rates other.rates

module Uf6MutationRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let private interpolateSeed6ActionRates (startRates: Seed6ActionRates) (endRates: Seed6ActionRates) (t: float) : Seed6ActionRates =
        Seed6ActionRates.create(
            startRates.Ortho1Rate + t * (endRates.Ortho1Rate - startRates.Ortho1Rate),
            startRates.Ortho2Rate + t * (endRates.Ortho2Rate - startRates.Ortho2Rate),
            startRates.Para1Rate + t * (endRates.Para1Rate - startRates.Para1Rate),
            startRates.Para2Rate + t * (endRates.Para2Rate - startRates.Para2Rate),
            startRates.Para3Rate + t * (endRates.Para3Rate - startRates.Para3Rate),
            startRates.Para4Rate + t * (endRates.Para4Rate - startRates.Para4Rate),
            startRates.SelfReflRate + t * (endRates.SelfReflRate - startRates.SelfReflRate))

    let private interpolateOpsTransitionRates (startRates: OpsTransitionRates) (endRates: OpsTransitionRates) (t: float) : OpsTransitionRates =
        OpsTransitionRates.create(
            OpsActionRates.create(
                startRates.OrthoRates.OrthoRate + t * (endRates.OrthoRates.OrthoRate - startRates.OrthoRates.OrthoRate),
                startRates.OrthoRates.ParaRate + t * (endRates.OrthoRates.ParaRate - startRates.OrthoRates.ParaRate),
                startRates.OrthoRates.SelfReflRate + t * (endRates.OrthoRates.SelfReflRate - startRates.OrthoRates.SelfReflRate)),
            OpsActionRates.create(
                startRates.ParaRates.OrthoRate + t * (endRates.ParaRates.OrthoRate - startRates.ParaRates.OrthoRate),
                startRates.ParaRates.ParaRate + t * (endRates.ParaRates.ParaRate - startRates.ParaRates.ParaRate),
                startRates.ParaRates.SelfReflRate + t * (endRates.ParaRates.SelfReflRate - startRates.ParaRates.SelfReflRate)),
            OpsActionRates.create(
                startRates.SelfReflRates.OrthoRate + t * (endRates.SelfReflRates.OrthoRate - startRates.SelfReflRates.OrthoRate),
                startRates.SelfReflRates.ParaRate + t * (endRates.SelfReflRates.ParaRate - startRates.SelfReflRates.ParaRate),
                startRates.SelfReflRates.SelfReflRate + t * (endRates.SelfReflRates.SelfReflRate - startRates.SelfReflRates.SelfReflRate)))

    let createLinearVariation (length: int) (order: int) (startRates: uf6MutationRates) (endRates: uf6MutationRates) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 2 <> 0 then failwith "Order must be at least 6 and even"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let seed = Seed6TransitionRates.create(
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.Ortho1Rates endRates.seed6TransitionRates.Ortho1Rates t,
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.Ortho2Rates endRates.seed6TransitionRates.Ortho2Rates t,
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.Para1Rates endRates.seed6TransitionRates.Para1Rates t,
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.Para2Rates endRates.seed6TransitionRates.Para2Rates t,
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.Para3Rates endRates.seed6TransitionRates.Para3Rates t,
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.Para4Rates endRates.seed6TransitionRates.Para4Rates t,
                    interpolateSeed6ActionRates startRates.seed6TransitionRates.SelfReflRates endRates.seed6TransitionRates.SelfReflRates t)
                let listLength = exactLog2 (order / 6)
                let opsTransitionRates =
                    Array.init listLength (fun j ->
                        let startList = startRates.opsTransitionRates.RatesArray
                        let endList = endRates.opsTransitionRates.RatesArray
                        if j >= startList.Length || j >= endList.Length then OpsTransitionRates.createUniform(0.1)
                        else interpolateOpsTransitionRates startList.[j] endList.[j] t)

                uf6MutationRates.create order seed (OpsTransitionRatesArray.create opsTransitionRates) )
        uf6MutationRatesArray.create rates

    let createSinusoidalVariation (length: int) (order: int) (baseRates: uf6MutationRates) (amplitudes: uf6MutationRates) (frequency: float) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 2 <> 0 then failwith "Order must be at least 6 and even"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let phaseShift = 2.0 * Math.PI / 7.0
                let seed = Seed6TransitionRates.create(
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.Ortho1Rate + amplitudes.seed6TransitionRates.Ortho1Rates.Ortho1Rate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.Ortho2Rate + amplitudes.seed6TransitionRates.Ortho1Rates.Ortho2Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.Para1Rate + amplitudes.seed6TransitionRates.Ortho1Rates.Para1Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.Para2Rate + amplitudes.seed6TransitionRates.Ortho1Rates.Para2Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.Para3Rate + amplitudes.seed6TransitionRates.Ortho1Rates.Para3Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.Para4Rate + amplitudes.seed6TransitionRates.Ortho1Rates.Para4Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho1Rates.SelfReflRate + amplitudes.seed6TransitionRates.Ortho1Rates.SelfReflRate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0),
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.Ortho1Rate + amplitudes.seed6TransitionRates.Ortho2Rates.Ortho1Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.Ortho2Rate + amplitudes.seed6TransitionRates.Ortho2Rates.Ortho2Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.Para1Rate + amplitudes.seed6TransitionRates.Ortho2Rates.Para1Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.Para2Rate + amplitudes.seed6TransitionRates.Ortho2Rates.Para2Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.Para3Rate + amplitudes.seed6TransitionRates.Ortho2Rates.Para3Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.Para4Rate + amplitudes.seed6TransitionRates.Ortho2Rates.Para4Rate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Ortho2Rates.SelfReflRate + amplitudes.seed6TransitionRates.Ortho2Rates.SelfReflRate * Math.Sin(t)) 0.0 1.0),
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.Para1Rates.Ortho1Rate + amplitudes.seed6TransitionRates.Para1Rates.Ortho1Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para1Rates.Ortho2Rate + amplitudes.seed6TransitionRates.Para1Rates.Ortho2Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para1Rates.Para1Rate + amplitudes.seed6TransitionRates.Para1Rates.Para1Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para1Rates.Para2Rate + amplitudes.seed6TransitionRates.Para1Rates.Para2Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para1Rates.Para3Rate + amplitudes.seed6TransitionRates.Para1Rates.Para3Rate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para1Rates.Para4Rate + amplitudes.seed6TransitionRates.Para1Rates.Para4Rate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para1Rates.SelfReflRate + amplitudes.seed6TransitionRates.Para1Rates.SelfReflRate * Math.Sin(t + phaseShift)) 0.0 1.0),
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.Para2Rates.Ortho1Rate + amplitudes.seed6TransitionRates.Para2Rates.Ortho1Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para2Rates.Ortho2Rate + amplitudes.seed6TransitionRates.Para2Rates.Ortho2Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para2Rates.Para1Rate + amplitudes.seed6TransitionRates.Para2Rates.Para1Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para2Rates.Para2Rate + amplitudes.seed6TransitionRates.Para2Rates.Para2Rate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para2Rates.Para3Rate + amplitudes.seed6TransitionRates.Para2Rates.Para3Rate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para2Rates.Para4Rate + amplitudes.seed6TransitionRates.Para2Rates.Para4Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para2Rates.SelfReflRate + amplitudes.seed6TransitionRates.Para2Rates.SelfReflRate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0),
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.Para3Rates.Ortho1Rate + amplitudes.seed6TransitionRates.Para3Rates.Ortho1Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para3Rates.Ortho2Rate + amplitudes.seed6TransitionRates.Para3Rates.Ortho2Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para3Rates.Para1Rate + amplitudes.seed6TransitionRates.Para3Rates.Para1Rate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para3Rates.Para2Rate + amplitudes.seed6TransitionRates.Para3Rates.Para2Rate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para3Rates.Para3Rate + amplitudes.seed6TransitionRates.Para3Rates.Para3Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para3Rates.Para4Rate + amplitudes.seed6TransitionRates.Para3Rates.Para4Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para3Rates.SelfReflRate + amplitudes.seed6TransitionRates.Para3Rates.SelfReflRate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0),
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.Para4Rates.Ortho1Rate + amplitudes.seed6TransitionRates.Para4Rates.Ortho1Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para4Rates.Ortho2Rate + amplitudes.seed6TransitionRates.Para4Rates.Ortho2Rate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para4Rates.Para1Rate + amplitudes.seed6TransitionRates.Para4Rates.Para1Rate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para4Rates.Para2Rate + amplitudes.seed6TransitionRates.Para4Rates.Para2Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para4Rates.Para3Rate + amplitudes.seed6TransitionRates.Para4Rates.Para3Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para4Rates.Para4Rate + amplitudes.seed6TransitionRates.Para4Rates.Para4Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.Para4Rates.SelfReflRate + amplitudes.seed6TransitionRates.Para4Rates.SelfReflRate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0),
                    Seed6ActionRates.create(
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.Ortho1Rate + amplitudes.seed6TransitionRates.SelfReflRates.Ortho1Rate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.Ortho2Rate + amplitudes.seed6TransitionRates.SelfReflRates.Ortho2Rate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.Para1Rate + amplitudes.seed6TransitionRates.SelfReflRates.Para1Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.Para2Rate + amplitudes.seed6TransitionRates.SelfReflRates.Para2Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.Para3Rate + amplitudes.seed6TransitionRates.SelfReflRates.Para3Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.Para4Rate + amplitudes.seed6TransitionRates.SelfReflRates.Para4Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                        clamp (baseRates.seed6TransitionRates.SelfReflRates.SelfReflRate + amplitudes.seed6TransitionRates.SelfReflRates.SelfReflRate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0))
                let listLength = exactLog2 (order / 6)
                let opsTransitionRates =
                    Array.init listLength (fun j ->
                        let baseList = baseRates.opsTransitionRates.RatesArray
                        let ampList = amplitudes.opsTransitionRates.RatesArray
                        if j >= baseList.Length || j >= ampList.Length then OpsTransitionRates.createUniform(0.1)
                        else
                            OpsTransitionRates.create(
                                OpsActionRates.create(
                                    clamp (baseList.[j].OrthoRates.OrthoRate + ampList.[j].OrthoRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                                    clamp (baseList.[j].OrthoRates.ParaRate + ampList.[j].OrthoRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                    clamp (baseList.[j].OrthoRates.SelfReflRate + ampList.[j].OrthoRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0),
                                OpsActionRates.create(
                                    clamp (baseList.[j].ParaRates.OrthoRate + ampList.[j].ParaRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                                    clamp (baseList.[j].ParaRates.ParaRate + ampList.[j].ParaRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                    clamp (baseList.[j].ParaRates.SelfReflRate + ampList.[j].ParaRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0),
                                OpsActionRates.create(
                                    clamp (baseList.[j].SelfReflRates.OrthoRate + ampList.[j].SelfReflRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                                    clamp (baseList.[j].SelfReflRates.ParaRate + ampList.[j].SelfReflRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                    clamp (baseList.[j].SelfReflRates.SelfReflRate + ampList.[j].SelfReflRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)))

                uf6MutationRates.create order seed (OpsTransitionRatesArray.create opsTransitionRates) )
        uf6MutationRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: uf6MutationRates) (hotSpotIndex: int) (hotSpotRates: uf6MutationRates) (sigma: float) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 2 <> 0 then failwith "Order must be at least 6 and even"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let seed = Seed6TransitionRates.create(
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.Ortho1Rates hotSpotRates.seed6TransitionRates.Ortho1Rates weight,
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.Ortho2Rates hotSpotRates.seed6TransitionRates.Ortho2Rates weight,
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.Para1Rates hotSpotRates.seed6TransitionRates.Para1Rates weight,
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.Para2Rates hotSpotRates.seed6TransitionRates.Para2Rates weight,
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.Para3Rates hotSpotRates.seed6TransitionRates.Para3Rates weight,
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.Para4Rates hotSpotRates.seed6TransitionRates.Para4Rates weight,
                    interpolateSeed6ActionRates baseRates.seed6TransitionRates.SelfReflRates hotSpotRates.seed6TransitionRates.SelfReflRates weight)
                let listLength = exactLog2 (order / 6)
                let opsTransitionRates =
                    Array.init listLength (fun j ->
                        let baseList = baseRates.opsTransitionRates.RatesArray
                        let hotSpotList = hotSpotRates.opsTransitionRates.RatesArray
                        if j >= baseList.Length || j >= hotSpotList.Length then OpsTransitionRates.createUniform(0.1)
                        else interpolateOpsTransitionRates baseList.[j] hotSpotList.[j] weight)
                uf6MutationRates.create order seed (OpsTransitionRatesArray.create opsTransitionRates) )
        uf6MutationRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: uf6MutationRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: uf6MutationRates) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 2 <> 0 then failwith "Order must be at least 6 and even"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                uf6MutationRates.create order rates.seed6TransitionRates rates.opsTransitionRates
                //{ Uf6MutationRates.order = order
                //  seed6TransitionRates = Seed6TransitionRates.create(
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.Ortho1Rates.Ortho1Rate,
                //          rates.seed6TransitionRates.Ortho1Rates.Ortho2Rate,
                //          rates.seed6TransitionRates.Ortho1Rates.Para1Rate,
                //          rates.seed6TransitionRates.Ortho1Rates.Para2Rate,
                //          rates.seed6TransitionRates.Ortho1Rates.Para3Rate,
                //          rates.seed6TransitionRates.Ortho1Rates.Para4Rate,
                //          rates.seed6TransitionRates.Ortho1Rates.SelfReflRate),
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.Ortho2Rates.Ortho1Rate,
                //          rates.seed6TransitionRates.Ortho2Rates.Ortho2Rate,
                //          rates.seed6TransitionRates.Ortho2Rates.Para1Rate,
                //          rates.seed6TransitionRates.Ortho2Rates.Para2Rate,
                //          rates.seed6TransitionRates.Ortho2Rates.Para3Rate,
                //          rates.seed6TransitionRates.Ortho2Rates.Para4Rate,
                //          rates.seed6TransitionRates.Ortho2Rates.SelfReflRate),
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.Para1Rates.Ortho1Rate,
                //          rates.seed6TransitionRates.Para1Rates.Ortho2Rate,
                //          rates.seed6TransitionRates.Para1Rates.Para1Rate,
                //          rates.seed6TransitionRates.Para1Rates.Para2Rate,
                //          rates.seed6TransitionRates.Para1Rates.Para3Rate,
                //          rates.seed6TransitionRates.Para1Rates.Para4Rate,
                //          rates.seed6TransitionRates.Para1Rates.SelfReflRate),
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.Para2Rates.Ortho1Rate,
                //          rates.seed6TransitionRates.Para2Rates.Ortho2Rate,
                //          rates.seed6TransitionRates.Para2Rates.Para1Rate,
                //          rates.seed6TransitionRates.Para2Rates.Para2Rate,
                //          rates.seed6TransitionRates.Para2Rates.Para3Rate,
                //          rates.seed6TransitionRates.Para2Rates.Para4Rate,
                //          rates.seed6TransitionRates.Para2Rates.SelfReflRate),
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.Para3Rates.Ortho1Rate,
                //          rates.seed6TransitionRates.Para3Rates.Ortho2Rate,
                //          rates.seed6TransitionRates.Para3Rates.Para1Rate,
                //          rates.seed6TransitionRates.Para3Rates.Para2Rate,
                //          rates.seed6TransitionRates.Para3Rates.Para3Rate,
                //          rates.seed6TransitionRates.Para3Rates.Para4Rate,
                //          rates.seed6TransitionRates.Para3Rates.SelfReflRate),
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.Para4Rates.Ortho1Rate,
                //          rates.seed6TransitionRates.Para4Rates.Ortho2Rate,
                //          rates.seed6TransitionRates.Para4Rates.Para1Rate,
                //          rates.seed6TransitionRates.Para4Rates.Para2Rate,
                //          rates.seed6TransitionRates.Para4Rates.Para3Rate,
                //          rates.seed6TransitionRates.Para4Rates.Para4Rate,
                //          rates.seed6TransitionRates.Para4Rates.SelfReflRate),
                //      Seed6ActionRates.create(
                //          rates.seed6TransitionRates.SelfReflRates.Ortho1Rate,
                //          rates.seed6TransitionRates.SelfReflRates.Ortho2Rate,
                //          rates.seed6TransitionRates.SelfReflRates.Para1Rate,
                //          rates.seed6TransitionRates.SelfReflRates.Para2Rate,
                //          rates.seed6TransitionRates.SelfReflRates.Para3Rate,
                //          rates.seed6TransitionRates.SelfReflRates.Para4Rate,
                //          rates.seed6TransitionRates.SelfReflRates.SelfReflRate))
                //  opsTransitionRates = rates.opsTransitionRates }
                  
                  
                  
                  )
        uf6MutationRatesArray.create rates

    let mutate<'a> 
        (uf6MutationRatesArray: uf6MutationRatesArray) 
        (ortho1Mutator: 'a -> 'a) 
        (ortho2Mutator: 'a -> 'a) 
        (para1Mutator: 'a -> 'a) 
        (para2Mutator: 'a -> 'a) 
        (para3Mutator: 'a -> 'a) 
        (para4Mutator: 'a -> 'a) 
        (selfReflMutator: 'a -> 'a) 
        (noActionMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (seed6TwoOrbitType: TwoOrbitTripleType) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if uf6MutationRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
        Array.init arrayToMutate.Length (fun i ->
            let rate = uf6MutationRatesArray.Item(i)
            match rate.seed6TransitionRates.PickMode floatPicker seed6TwoOrbitType with
            | Seed6ActionMode.Ortho1 -> ortho1Mutator arrayToMutate.[i]
            | Seed6ActionMode.Ortho2 -> ortho2Mutator arrayToMutate.[i]
            | Seed6ActionMode.Para1 -> para1Mutator arrayToMutate.[i]
            | Seed6ActionMode.Para2 -> para2Mutator arrayToMutate.[i]
            | Seed6ActionMode.Para3 -> para3Mutator arrayToMutate.[i]
            | Seed6ActionMode.Para4 -> para4Mutator arrayToMutate.[i]
            | Seed6ActionMode.SelfRefl -> selfReflMutator arrayToMutate.[i]
            | Seed6ActionMode.NoAction -> noActionMutator arrayToMutate.[i])

    let createNewItems<'a> 
        (uf6MutationRatesArray: uf6MutationRatesArray)
        (itemChooser: uf6MutationRates -> 'a)
            : 'a[] =
        Array.init uf6MutationRatesArray.Length (fun i ->
            itemChooser (uf6MutationRatesArray.Item(i)))