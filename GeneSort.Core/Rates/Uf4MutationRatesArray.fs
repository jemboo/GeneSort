namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type Uf4MutationRatesArray =
    private 
        { rates: Uf4MutationRates array }

    static member create (rates: Uf4MutationRates array) : Uf4MutationRatesArray =
        if Array.exists (fun r -> r.order < 4 || r.order % 4 <> 0) rates then
            failwith "All Uf4MutationRates orders must be at least 4 and divisible by 4"
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (
            fun r -> sprintf "Uf4MutationRates(order=%d, seed=%s, arrayLength=%d)" 
                        r.order (r.seedOpsTransitionRates.toString()) r.twoOrbitPairOpsTransitionRates.Length) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? Uf4MutationRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedOpsTransitionRates.Equals(b.seedOpsTransitionRates) && 
                    a.twoOrbitPairOpsTransitionRates.Equals(b.twoOrbitPairOpsTransitionRates)) this.rates other.rates
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
                    a.twoOrbitPairOpsTransitionRates.Equals(b.twoOrbitPairOpsTransitionRates)) this.rates other.rates

module Uf4MutationRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let createLinearVariation (length: int) (order: int) (startRates: Uf4MutationRates) (endRates: Uf4MutationRates) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        if startRates.twoOrbitPairOpsTransitionRates.Length <> endRates.twoOrbitPairOpsTransitionRates.Length then
            failwith "Start and end rates must have the same twoOrbitPairOpsTransitionRates length"
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
                let opsTransitionRatesArray =
                    Array.init listLength (fun j ->
                        let startArray = startRates.twoOrbitPairOpsTransitionRates.RatesArray
                        let endArray = endRates.twoOrbitPairOpsTransitionRates.RatesArray
                        OpsTransitionRates.create(
                            OpsActionRates.create(
                                startArray.[j].OrthoRates.OrthoRate + t * (endArray.[j].OrthoRates.OrthoRate - startArray.[j].OrthoRates.OrthoRate),
                                startArray.[j].OrthoRates.ParaRate + t * (endArray.[j].OrthoRates.ParaRate - startArray.[j].OrthoRates.ParaRate),
                                startArray.[j].OrthoRates.SelfReflRate + t * (endArray.[j].OrthoRates.SelfReflRate - startArray.[j].OrthoRates.SelfReflRate)),
                            OpsActionRates.create(
                                startArray.[j].ParaRates.OrthoRate + t * (endArray.[j].ParaRates.OrthoRate - startArray.[j].ParaRates.OrthoRate),
                                startArray.[j].ParaRates.ParaRate + t * (endArray.[j].ParaRates.ParaRate - startArray.[j].ParaRates.ParaRate),
                                startArray.[j].ParaRates.SelfReflRate + t * (endArray.[j].ParaRates.SelfReflRate - startArray.[j].ParaRates.SelfReflRate)),
                            OpsActionRates.create(
                                startArray.[j].SelfReflRates.OrthoRate + t * (endArray.[j].SelfReflRates.OrthoRate - startArray.[j].SelfReflRates.OrthoRate),
                                startArray.[j].SelfReflRates.ParaRate + t * (endArray.[j].SelfReflRates.ParaRate - startArray.[j].SelfReflRates.ParaRate),
                                startArray.[j].SelfReflRates.SelfReflRate + t * (endArray.[j].SelfReflRates.SelfReflRate - startArray.[j].SelfReflRates.SelfReflRate))))
                { Uf4MutationRates.order = order
                  seedOpsTransitionRates = seed
                  twoOrbitPairOpsTransitionRates = OpsTransitionRatesArray.create opsTransitionRatesArray })
        Uf4MutationRatesArray.create rates

    let createSinusoidalVariation (length: int) (order: int) (baseRates: Uf4MutationRates) (amplitudes: Uf4MutationRates) (frequency: float) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        if baseRates.twoOrbitPairOpsTransitionRates.Length <> amplitudes.twoOrbitPairOpsTransitionRates.Length then
            failwith "Base and amplitudes must have the same twoOrbitPairOpsTransitionRates length"
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
                let opsTransitionRatesArray =
                    Array.init listLength (fun j ->
                        let baseArray = baseRates.twoOrbitPairOpsTransitionRates.RatesArray
                        let ampArray = amplitudes.twoOrbitPairOpsTransitionRates.RatesArray
                        OpsTransitionRates.create(
                            OpsActionRates.create(
                                clamp (baseArray.[j].OrthoRates.OrthoRate + ampArray.[j].OrthoRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                                clamp (baseArray.[j].OrthoRates.ParaRate + ampArray.[j].OrthoRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                clamp (baseArray.[j].OrthoRates.SelfReflRate + ampArray.[j].OrthoRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0),
                            OpsActionRates.create(
                                clamp (baseArray.[j].ParaRates.OrthoRate + ampArray.[j].ParaRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                                clamp (baseArray.[j].ParaRates.ParaRate + ampArray.[j].ParaRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                clamp (baseArray.[j].ParaRates.SelfReflRate + ampArray.[j].ParaRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0),
                            OpsActionRates.create(
                                clamp (baseArray.[j].SelfReflRates.OrthoRate + ampArray.[j].SelfReflRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                                clamp (baseArray.[j].SelfReflRates.ParaRate + ampArray.[j].SelfReflRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                clamp (baseArray.[j].SelfReflRates.SelfReflRate + ampArray.[j].SelfReflRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)))
                { Uf4MutationRates.order = order
                  seedOpsTransitionRates = seed
                  twoOrbitPairOpsTransitionRates = OpsTransitionRatesArray.create opsTransitionRatesArray })
        Uf4MutationRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: Uf4MutationRates) (hotSpotIndex: int) (hotSpotRates: Uf4MutationRates) (sigma: float) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        if baseRates.twoOrbitPairOpsTransitionRates.Length <> hotSpotRates.twoOrbitPairOpsTransitionRates.Length then
            failwith "Base and hotspot rates must have the same twoOrbitPairOpsTransitionRates length"
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
                let opsTransitionRatesArray =
                    Array.init listLength (fun j ->
                        let baseArray = baseRates.twoOrbitPairOpsTransitionRates.RatesArray
                        let hotSpotArray = hotSpotRates.twoOrbitPairOpsTransitionRates.RatesArray
                        OpsTransitionRates.create(
                            OpsActionRates.create(
                                baseArray.[j].OrthoRates.OrthoRate + (hotSpotArray.[j].OrthoRates.OrthoRate - baseArray.[j].OrthoRates.OrthoRate) * weight,
                                baseArray.[j].OrthoRates.ParaRate + (hotSpotArray.[j].OrthoRates.ParaRate - baseArray.[j].OrthoRates.ParaRate) * weight,
                                baseArray.[j].OrthoRates.SelfReflRate + (hotSpotArray.[j].OrthoRates.SelfReflRate - baseArray.[j].OrthoRates.SelfReflRate) * weight),
                            OpsActionRates.create(
                                baseArray.[j].ParaRates.OrthoRate + (hotSpotArray.[j].ParaRates.OrthoRate - baseArray.[j].ParaRates.OrthoRate) * weight,
                                baseArray.[j].ParaRates.ParaRate + (hotSpotArray.[j].ParaRates.ParaRate - baseArray.[j].ParaRates.ParaRate) * weight,
                                baseArray.[j].ParaRates.SelfReflRate + (hotSpotArray.[j].ParaRates.SelfReflRate - baseArray.[j].ParaRates.SelfReflRate) * weight),
                            OpsActionRates.create(
                                baseArray.[j].SelfReflRates.OrthoRate + (hotSpotArray.[j].SelfReflRates.OrthoRate - baseArray.[j].SelfReflRates.OrthoRate) * weight,
                                baseArray.[j].SelfReflRates.ParaRate + (hotSpotArray.[j].SelfReflRates.ParaRate - baseArray.[j].SelfReflRates.ParaRate) * weight,
                                baseArray.[j].SelfReflRates.SelfReflRate + (hotSpotArray.[j].SelfReflRates.SelfReflRate - baseArray.[j].SelfReflRates.SelfReflRate) * weight)))
                { Uf4MutationRates.order = order
                  seedOpsTransitionRates = seed
                  twoOrbitPairOpsTransitionRates = OpsTransitionRatesArray.create opsTransitionRatesArray })
        Uf4MutationRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: Uf4MutationRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: Uf4MutationRates) : Uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        if baseRates.twoOrbitPairOpsTransitionRates.Length <> hotSpotRates.twoOrbitPairOpsTransitionRates.Length then
            failwith "Base and hotspot rates must have the same twoOrbitPairOpsTransitionRates length"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                { Uf4MutationRates.order = order
                  seedOpsTransitionRates = OpsTransitionRates.create(
                      OpsActionRates.create(rates.seedOpsTransitionRates.OrthoRates.OrthoRate, rates.seedOpsTransitionRates.OrthoRates.ParaRate, rates.seedOpsTransitionRates.OrthoRates.SelfReflRate),
                      OpsActionRates.create(rates.seedOpsTransitionRates.ParaRates.OrthoRate, rates.seedOpsTransitionRates.ParaRates.ParaRate, rates.seedOpsTransitionRates.ParaRates.SelfReflRate),
                      OpsActionRates.create(rates.seedOpsTransitionRates.SelfReflRates.OrthoRate, rates.seedOpsTransitionRates.SelfReflRates.ParaRate, rates.seedOpsTransitionRates.SelfReflRates.SelfReflRate))
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
