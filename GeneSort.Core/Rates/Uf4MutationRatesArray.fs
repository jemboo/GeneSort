namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type Uf4MutationRatesArray =
    private 
        { rates: Uf4MutationRates array }

    static member create (rates: Uf4MutationRates array) : Uf4MutationRatesArray =
        if Array.isEmpty rates then failwith "Rates array cannot be empty"
        if Array.exists (fun r -> r.order < 4 || r.order % 2 <> 0) rates then
            failwith "All Uf4MutationRates orders must be at least 4 and even"
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (
            fun r -> sprintf "Uf4MutationRates(order=%d, seed=%s, listLength=%d)" 
                        r.order (r.seedOpsTransitionRates.toString()) r.twoOrbitPairOpsTransitionRates.Length) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? Uf4MutationRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedOpsTransitionRates.Equals(b.seedOpsTransitionRates) && 
                    a.twoOrbitPairOpsTransitionRates = b.twoOrbitPairOpsTransitionRates) this.rates other.rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.rates do
            hash <- hash * 23 + rate.order.GetHashCode()
            hash <- hash * 23 + rate.seedOpsTransitionRates.GetHashCode()
            hash <- hash * 23 + rate.twoOrbitPairOpsTransitionRates.GetHashCode()
        hash

    interface IEquatable<Uf4MutationRatesArray> with
        member this.Equals(other) =
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedOpsTransitionRates.Equals(b.seedOpsTransitionRates) && 
                    a.twoOrbitPairOpsTransitionRates = b.twoOrbitPairOpsTransitionRates) this.rates other.rates

module Uf4MutationRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let createLinearVariation (length: int) (order: int) (startRates: Uf4MutationRates) (endRates: Uf4MutationRates) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let seed = OpsTransitionRates.create(
                    OpsActionRates.create(
                        startRates.seedOpsTransitionRates.OrthoRates.OrthoRate + t * (endRates.seedOpsTransitionRates.OrthoRates.OrthoRate - startRates.seedOpsTransitionRates.OrthoRates.OrthoRate),
                        startRates.seedOpsTransitionRates.OrthoRates.ParaRate + t * (endRates.seedOpsTransitionRates.OrthoRates.ParaRate - startRates.seedOpsTransitionRates.OrthoRates.ParaRate),
                        startRates.seedOpsTransitionRates.OrthoRates.SelfReflRate + t * (endRates.seedOpsTransitionRates.OrthoRates.SelfReflRate - startRates.seedOpsTransitionRates.OrthoRates.SelfReflRate)),
                    OpsActionRates.create(
                        startRates.seedOpsTransitionRates.ParaRates.OrthoRate + t * (endRates.seedOpsTransitionRates.ParaRates.OrthoRate - startRates.seedOpsTransitionRates.ParaRates.OrthoRate),
                        startRates.seedOpsTransitionRates.ParaRates.ParaRate + t * (endRates.seedOpsTransitionRates.ParaRates.ParaRate - startRates.seedOpsTransitionRates.ParaRates.ParaRate),
                        startRates.seedOpsTransitionRates.ParaRates.SelfReflRate + t * (endRates.seedOpsTransitionRates.ParaRates.SelfReflRate - startRates.seedOpsTransitionRates.ParaRates.SelfReflRate)),
                    OpsActionRates.create(
                        startRates.seedOpsTransitionRates.SelfReflRates.OrthoRate + t * (endRates.seedOpsTransitionRates.SelfReflRates.OrthoRate - startRates.seedOpsTransitionRates.SelfReflRates.OrthoRate),
                        startRates.seedOpsTransitionRates.SelfReflRates.ParaRate + t * (endRates.seedOpsTransitionRates.SelfReflRates.ParaRate - startRates.seedOpsTransitionRates.SelfReflRates.ParaRate),
                        startRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate + t * (endRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate - startRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate)))
                let listLength = MathUtils.exactLog2 (order / 4)
                let opsTransitionRatesList =
                    List.init listLength (fun j ->
                        let startList = startRates.twoOrbitPairOpsTransitionRates
                        let endList = endRates.twoOrbitPairOpsTransitionRates
                        if j >= startList.Length || j >= endList.Length then OpsTransitionRates.createUniform(0.0)
                        else
                            OpsTransitionRates.create(
                                OpsActionRates.create(
                                    startList.[j].OrthoRates.OrthoRate + t * (endList.[j].OrthoRates.OrthoRate - startList.[j].OrthoRates.OrthoRate),
                                    startList.[j].OrthoRates.ParaRate + t * (endList.[j].OrthoRates.ParaRate - startList.[j].OrthoRates.ParaRate),
                                    startList.[j].OrthoRates.SelfReflRate + t * (endList.[j].OrthoRates.SelfReflRate - startList.[j].OrthoRates.SelfReflRate)),
                                OpsActionRates.create(
                                    startList.[j].ParaRates.OrthoRate + t * (endList.[j].ParaRates.OrthoRate - startList.[j].ParaRates.OrthoRate),
                                    startList.[j].ParaRates.ParaRate + t * (endList.[j].ParaRates.ParaRate - startList.[j].ParaRates.ParaRate),
                                    startList.[j].ParaRates.SelfReflRate + t * (endList.[j].ParaRates.SelfReflRate - startList.[j].ParaRates.SelfReflRate)),
                                OpsActionRates.create(
                                    startList.[j].SelfReflRates.OrthoRate + t * (endList.[j].SelfReflRates.OrthoRate - startList.[j].SelfReflRates.OrthoRate),
                                    startList.[j].SelfReflRates.ParaRate + t * (endList.[j].SelfReflRates.ParaRate - startList.[j].SelfReflRates.ParaRate),
                                    startList.[j].SelfReflRates.SelfReflRate + t * (endList.[j].SelfReflRates.SelfReflRate - startList.[j].SelfReflRates.SelfReflRate))))
                { Uf4MutationRates.order = order; seedOpsTransitionRates = seed; twoOrbitPairOpsTransitionRates = opsTransitionRatesList })
        Uf4MutationRatesArray.create rates

    let createSinusoidalVariation (length: int) (order: int) (baseRates: Uf4MutationRates) (amplitudes: Uf4MutationRates) (frequency: float) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let seed = OpsTransitionRates.create(
                    OpsActionRates.create(
                        clamp (baseRates.seedOpsTransitionRates.OrthoRates.OrthoRate + amplitudes.seedOpsTransitionRates.OrthoRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seedOpsTransitionRates.OrthoRates.ParaRate + amplitudes.seedOpsTransitionRates.OrthoRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                        clamp (baseRates.seedOpsTransitionRates.OrthoRates.SelfReflRate + amplitudes.seedOpsTransitionRates.OrthoRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0),
                    OpsActionRates.create(
                        clamp (baseRates.seedOpsTransitionRates.ParaRates.OrthoRate + amplitudes.seedOpsTransitionRates.ParaRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seedOpsTransitionRates.ParaRates.ParaRate + amplitudes.seedOpsTransitionRates.ParaRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                        clamp (baseRates.seedOpsTransitionRates.ParaRates.SelfReflRate + amplitudes.seedOpsTransitionRates.ParaRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0),
                    OpsActionRates.create(
                        clamp (baseRates.seedOpsTransitionRates.SelfReflRates.OrthoRate + amplitudes.seedOpsTransitionRates.SelfReflRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                        clamp (baseRates.seedOpsTransitionRates.SelfReflRates.ParaRate + amplitudes.seedOpsTransitionRates.SelfReflRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                        clamp (baseRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate + amplitudes.seedOpsTransitionRates.SelfReflRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0))
                let listLength = MathUtils.exactLog2 (order / 4)
                let opsTransitionRatesList =
                    List.init listLength (fun j ->
                        let baseList = baseRates.twoOrbitPairOpsTransitionRates
                        let ampList = amplitudes.twoOrbitPairOpsTransitionRates
                        if j >= baseList.Length || j >= ampList.Length then OpsTransitionRates.createUniform(0.0)
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
                { Uf4MutationRates.order = order; seedOpsTransitionRates = seed; twoOrbitPairOpsTransitionRates = opsTransitionRatesList })
        Uf4MutationRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: Uf4MutationRates) (hotSpotIndex: int) (hotSpotRates: Uf4MutationRates) (sigma: float) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let seed = OpsTransitionRates.create(
                    OpsActionRates.create(
                        baseRates.seedOpsTransitionRates.OrthoRates.OrthoRate + (hotSpotRates.seedOpsTransitionRates.OrthoRates.OrthoRate - baseRates.seedOpsTransitionRates.OrthoRates.OrthoRate) * weight,
                        baseRates.seedOpsTransitionRates.OrthoRates.ParaRate + (hotSpotRates.seedOpsTransitionRates.OrthoRates.ParaRate - baseRates.seedOpsTransitionRates.OrthoRates.ParaRate) * weight,
                        baseRates.seedOpsTransitionRates.OrthoRates.SelfReflRate + (hotSpotRates.seedOpsTransitionRates.OrthoRates.SelfReflRate - baseRates.seedOpsTransitionRates.OrthoRates.SelfReflRate) * weight),
                    OpsActionRates.create(
                        baseRates.seedOpsTransitionRates.ParaRates.OrthoRate + (hotSpotRates.seedOpsTransitionRates.ParaRates.OrthoRate - baseRates.seedOpsTransitionRates.ParaRates.OrthoRate) * weight,
                        baseRates.seedOpsTransitionRates.ParaRates.ParaRate + (hotSpotRates.seedOpsTransitionRates.ParaRates.ParaRate - baseRates.seedOpsTransitionRates.ParaRates.ParaRate) * weight,
                        baseRates.seedOpsTransitionRates.ParaRates.SelfReflRate + (hotSpotRates.seedOpsTransitionRates.ParaRates.SelfReflRate - baseRates.seedOpsTransitionRates.ParaRates.SelfReflRate) * weight),
                    OpsActionRates.create(
                        baseRates.seedOpsTransitionRates.SelfReflRates.OrthoRate + (hotSpotRates.seedOpsTransitionRates.SelfReflRates.OrthoRate - baseRates.seedOpsTransitionRates.SelfReflRates.OrthoRate) * weight,
                        baseRates.seedOpsTransitionRates.SelfReflRates.ParaRate + (hotSpotRates.seedOpsTransitionRates.SelfReflRates.ParaRate - baseRates.seedOpsTransitionRates.SelfReflRates.ParaRate) * weight,
                        baseRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate + (hotSpotRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate - baseRates.seedOpsTransitionRates.SelfReflRates.SelfReflRate) * weight))
                let listLength = MathUtils.exactLog2 (order / 4)
                let opsTransitionRatesList =
                    List.init listLength (fun j ->
                        let baseList = baseRates.twoOrbitPairOpsTransitionRates
                        let hotSpotList = hotSpotRates.twoOrbitPairOpsTransitionRates
                        if j >= baseList.Length || j >= hotSpotList.Length then OpsTransitionRates.createUniform(0.0)
                        else
                            OpsTransitionRates.create(
                                OpsActionRates.create(
                                    baseList.[j].OrthoRates.OrthoRate + (hotSpotList.[j].OrthoRates.OrthoRate - baseList.[j].OrthoRates.OrthoRate) * weight,
                                    baseList.[j].OrthoRates.ParaRate + (hotSpotList.[j].OrthoRates.ParaRate - baseList.[j].OrthoRates.ParaRate) * weight,
                                    baseList.[j].OrthoRates.SelfReflRate + (hotSpotList.[j].OrthoRates.SelfReflRate - baseList.[j].OrthoRates.SelfReflRate) * weight),
                                OpsActionRates.create(
                                    baseList.[j].ParaRates.OrthoRate + (hotSpotList.[j].ParaRates.OrthoRate - baseList.[j].ParaRates.OrthoRate) * weight,
                                    baseList.[j].ParaRates.ParaRate + (hotSpotList.[j].ParaRates.ParaRate - baseList.[j].ParaRates.ParaRate) * weight,
                                    baseList.[j].ParaRates.SelfReflRate + (hotSpotList.[j].ParaRates.SelfReflRate - baseList.[j].ParaRates.SelfReflRate) * weight),
                                OpsActionRates.create(
                                    baseList.[j].SelfReflRates.OrthoRate + (hotSpotList.[j].SelfReflRates.OrthoRate - baseList.[j].SelfReflRates.OrthoRate) * weight,
                                    baseList.[j].SelfReflRates.ParaRate + (hotSpotList.[j].SelfReflRates.ParaRate - baseList.[j].SelfReflRates.ParaRate) * weight,
                                    baseList.[j].SelfReflRates.SelfReflRate + (hotSpotList.[j].SelfReflRates.SelfReflRate - baseList.[j].SelfReflRates.SelfReflRate) * weight)))
                { Uf4MutationRates.order = order; seedOpsTransitionRates = seed; twoOrbitPairOpsTransitionRates = opsTransitionRatesList })
        Uf4MutationRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: Uf4MutationRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: Uf4MutationRates) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                { Uf4MutationRates.order = order; 
                  seedOpsTransitionRates = OpsTransitionRates.create(
                      OpsActionRates.create(rates.seedOpsTransitionRates.OrthoRates.OrthoRate, rates.seedOpsTransitionRates.OrthoRates.ParaRate, rates.seedOpsTransitionRates.OrthoRates.SelfReflRate),
                      OpsActionRates.create(rates.seedOpsTransitionRates.ParaRates.OrthoRate, rates.seedOpsTransitionRates.ParaRates.ParaRate, rates.seedOpsTransitionRates.ParaRates.SelfReflRate),
                      OpsActionRates.create(rates.seedOpsTransitionRates.SelfReflRates.OrthoRate, rates.seedOpsTransitionRates.SelfReflRates.ParaRate, rates.seedOpsTransitionRates.SelfReflRates.SelfReflRate));
                  twoOrbitPairOpsTransitionRates = rates.twoOrbitPairOpsTransitionRates })
        Uf4MutationRatesArray.create rates

    let mutate<'a> 
        (uf4MutationRatesArray: Uf4MutationRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (selfSymMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (twoOrbitType: TwoOrbitType) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if uf4MutationRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
    
        Array.init arrayToMutate.Length (fun i ->
            let rate = uf4MutationRatesArray.Item(i)
            match rate.seedOpsTransitionRates.PickMode floatPicker twoOrbitType with
            | OpsActionMode.Ortho -> orthoMutator arrayToMutate.[i]
            | OpsActionMode.Para -> paraMutator arrayToMutate.[i]
            | OpsActionMode.SelfRefl -> selfSymMutator arrayToMutate.[i]
            | OpsActionMode.NoAction -> arrayToMutate.[i])

    let createNewItems<'a> 
        (uf4MutationRatesArray: Uf4MutationRatesArray)
        (itemChooser: Uf4MutationRates -> 'a)
            : 'a[] =
        Array.init uf4MutationRatesArray.Length (fun i ->
            itemChooser (uf4MutationRatesArray.Item(i)))