namespace GeneSort.Core

open System
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type uf6GenRatesArray =
    private 
        { rates: uf6GenRates array }

    static member create (rates: uf6GenRates array) : uf6GenRatesArray =
        if Array.exists (fun r -> r.order < 6 || r.order % 6 <> 0) rates then
            failwith "All Uf6GenRates orders must be at least 6 and divisible by 6"
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (
            fun r -> sprintf "Uf6GenRates(order=%d, seed=%s, arrayLength=%d)" 
                        r.order (r.seedGenRatesUf6.toString()) r.opsGenRatesArray.Length) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? uf6GenRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedGenRatesUf6.Equals(b.seedGenRatesUf6) && 
                    a.opsGenRatesArray.Equals(b.opsGenRatesArray)) this.rates other.rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.rates do
            hash <- hash * 23 + rate.order.GetHashCode()
            hash <- hash * 23 + rate.seedGenRatesUf6.GetHashCode()
            hash <- hash * 23 + rate.opsGenRatesArray.GetHashCode()
        hash

    interface IEquatable<uf6GenRatesArray> with
        member this.Equals(other) =
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedGenRatesUf6.Equals(b.seedGenRatesUf6) && 
                    a.opsGenRatesArray.Equals(b.opsGenRatesArray)) this.rates other.rates



module Uf6GenRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let createUniform (length: int) (order: int) : uf6GenRatesArray =
        let rates = Array.init length (fun _ -> Uf6GenRates.makeUniform order)
        uf6GenRatesArray.create rates


    let createLinearVariation (length: int) (order: int) (startRates: uf6GenRates) (endRates: uf6GenRates) : uf6GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 6 <> 0 then failwith "Order must be at least 6 and divisible by 6"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        if startRates.opsGenRatesArray.Length <> endRates.opsGenRatesArray.Length then
            failwith "Start and end rates must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let seed = Seed6GenRates.create(
                    startRates.seedGenRatesUf6.Ortho1Rate + t * (endRates.seedGenRatesUf6.Ortho1Rate - startRates.seedGenRatesUf6.Ortho1Rate),
                    startRates.seedGenRatesUf6.Ortho2Rate + t * (endRates.seedGenRatesUf6.Ortho2Rate - startRates.seedGenRatesUf6.Ortho2Rate),
                    startRates.seedGenRatesUf6.Para1Rate + t * (endRates.seedGenRatesUf6.Para1Rate - startRates.seedGenRatesUf6.Para1Rate),
                    startRates.seedGenRatesUf6.Para2Rate + t * (endRates.seedGenRatesUf6.Para2Rate - startRates.seedGenRatesUf6.Para2Rate),
                    startRates.seedGenRatesUf6.Para3Rate + t * (endRates.seedGenRatesUf6.Para3Rate - startRates.seedGenRatesUf6.Para3Rate),
                    startRates.seedGenRatesUf6.Para4Rate + t * (endRates.seedGenRatesUf6.Para4Rate - startRates.seedGenRatesUf6.Para4Rate),
                    startRates.seedGenRatesUf6.SelfReflRate + t * (endRates.seedGenRatesUf6.SelfReflRate - startRates.seedGenRatesUf6.SelfReflRate))
                let arrayLength = MathUtils.exactLog2 (order / 6)
                let opsGenRatesArray =
                    Array.init arrayLength (fun j ->
                        let startArray = startRates.opsGenRatesArray.RatesArray
                        let endArray = endRates.opsGenRatesArray.RatesArray
                        OpsGenRates.create(
                            startArray.[j].OrthoRate + t * (endArray.[j].OrthoRate - startArray.[j].OrthoRate),
                            startArray.[j].ParaRate + t * (endArray.[j].ParaRate - startArray.[j].ParaRate),
                            startArray.[j].SelfReflRate + t * (endArray.[j].SelfReflRate - startArray.[j].SelfReflRate)))
                uf6GenRates.create order seed (OpsGenRatesArray.create opsGenRatesArray) )
        uf6GenRatesArray.create rates

    let createSinusoidalVariation (length: int) (order: int) (baseRates: uf6GenRates) (amplitudes: uf6GenRates) (frequency: float) : uf6GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 6 <> 0 then failwith "Order must be at least 6 and divisible by 6"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        if baseRates.opsGenRatesArray.Length <> amplitudes.opsGenRatesArray.Length then
            failwith "Base and amplitudes must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let phaseShift = 2.0 * Math.PI / 7.0
                let seed = Seed6GenRates.create(
                    clamp (baseRates.seedGenRatesUf6.Ortho1Rate + amplitudes.seedGenRatesUf6.Ortho1Rate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.seedGenRatesUf6.Ortho2Rate + amplitudes.seedGenRatesUf6.Ortho2Rate * Math.Sin(t + phaseShift)) 0.0 1.0,
                    clamp (baseRates.seedGenRatesUf6.Para1Rate + amplitudes.seedGenRatesUf6.Para1Rate * Math.Sin(t + 2.0 * phaseShift)) 0.0 1.0,
                    clamp (baseRates.seedGenRatesUf6.Para2Rate + amplitudes.seedGenRatesUf6.Para2Rate * Math.Sin(t + 3.0 * phaseShift)) 0.0 1.0,
                    clamp (baseRates.seedGenRatesUf6.Para3Rate + amplitudes.seedGenRatesUf6.Para3Rate * Math.Sin(t + 4.0 * phaseShift)) 0.0 1.0,
                    clamp (baseRates.seedGenRatesUf6.Para4Rate + amplitudes.seedGenRatesUf6.Para4Rate * Math.Sin(t + 5.0 * phaseShift)) 0.0 1.0,
                    clamp (baseRates.seedGenRatesUf6.SelfReflRate + amplitudes.seedGenRatesUf6.SelfReflRate * Math.Sin(t + 6.0 * phaseShift)) 0.0 1.0)
                let arrayLength = MathUtils.exactLog2 (order / 6)
                let opsGenRatesArray =
                    Array.init arrayLength (fun j ->
                        let baseArray = baseRates.opsGenRatesArray.RatesArray
                        let ampArray = amplitudes.opsGenRatesArray.RatesArray
                        OpsGenRates.create(
                            clamp (baseArray.[j].OrthoRate + ampArray.[j].OrthoRate * Math.Sin(t)) 0.0 1.0,
                            clamp (baseArray.[j].ParaRate + ampArray.[j].ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                            clamp (baseArray.[j].SelfReflRate + ampArray.[j].SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0))
                uf6GenRates.create order seed (OpsGenRatesArray.create opsGenRatesArray))

        uf6GenRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: uf6GenRates) (hotSpotIndex: int) (hotSpotRates: uf6GenRates) (sigma: float) : uf6GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 6 <> 0 then failwith "Order must be at least 6 and divisible by 6"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        if baseRates.opsGenRatesArray.Length <> hotSpotRates.opsGenRatesArray.Length then
            failwith "Base and hotspot rates must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let seed = Seed6GenRates.create(
                    baseRates.seedGenRatesUf6.Ortho1Rate + (hotSpotRates.seedGenRatesUf6.Ortho1Rate - baseRates.seedGenRatesUf6.Ortho1Rate) * weight,
                    baseRates.seedGenRatesUf6.Ortho2Rate + (hotSpotRates.seedGenRatesUf6.Ortho2Rate - baseRates.seedGenRatesUf6.Ortho2Rate) * weight,
                    baseRates.seedGenRatesUf6.Para1Rate + (hotSpotRates.seedGenRatesUf6.Para1Rate - baseRates.seedGenRatesUf6.Para1Rate) * weight,
                    baseRates.seedGenRatesUf6.Para2Rate + (hotSpotRates.seedGenRatesUf6.Para2Rate - baseRates.seedGenRatesUf6.Para2Rate) * weight,
                    baseRates.seedGenRatesUf6.Para3Rate + (hotSpotRates.seedGenRatesUf6.Para3Rate - baseRates.seedGenRatesUf6.Para3Rate) * weight,
                    baseRates.seedGenRatesUf6.Para4Rate + (hotSpotRates.seedGenRatesUf6.Para4Rate - baseRates.seedGenRatesUf6.Para4Rate) * weight,
                    baseRates.seedGenRatesUf6.SelfReflRate + (hotSpotRates.seedGenRatesUf6.SelfReflRate - baseRates.seedGenRatesUf6.SelfReflRate) * weight)
                let arrayLength = MathUtils.exactLog2 (order / 6)
                let opsGenRatesArray =
                    Array.init arrayLength (fun j ->
                        let baseArray = baseRates.opsGenRatesArray.RatesArray
                        let hotSpotArray = hotSpotRates.opsGenRatesArray.RatesArray
                        OpsGenRates.create(
                            baseArray.[j].OrthoRate + (hotSpotArray.[j].OrthoRate - baseArray.[j].OrthoRate) * weight,
                            baseArray.[j].ParaRate + (hotSpotArray.[j].ParaRate - baseArray.[j].ParaRate) * weight,
                            baseArray.[j].SelfReflRate + (hotSpotArray.[j].SelfReflRate - baseArray.[j].SelfReflRate) * weight))
                uf6GenRates.create order seed (OpsGenRatesArray.create opsGenRatesArray) )

        uf6GenRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: uf6GenRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: uf6GenRates) : uf6GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 6 || order % 6 <> 0 then failwith "Order must be at least 6 and divisible by 6"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        if baseRates.opsGenRatesArray.Length <> hotSpotRates.opsGenRatesArray.Length then
            failwith "Base and hotspot rates must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                { order = order
                  seedGenRatesUf6 = Seed6GenRates.create(
                      rates.seedGenRatesUf6.Ortho1Rate,
                      rates.seedGenRatesUf6.Ortho2Rate,
                      rates.seedGenRatesUf6.Para1Rate,
                      rates.seedGenRatesUf6.Para2Rate,
                      rates.seedGenRatesUf6.Para3Rate,
                      rates.seedGenRatesUf6.Para4Rate,
                      rates.seedGenRatesUf6.SelfReflRate)
                  opsGenRatesArray = rates.opsGenRatesArray })
        uf6GenRatesArray.create rates

    let mutate<'a> 
        (uf6GenRatesArray: uf6GenRatesArray) 
        (ortho1Mutator: 'a -> 'a) 
        (ortho2Mutator: 'a -> 'a) 
        (para1Mutator: 'a -> 'a) 
        (para2Mutator: 'a -> 'a) 
        (para3Mutator: 'a -> 'a) 
        (para4Mutator: 'a -> 'a) 
        (selfReflMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (seed6TwoOrbitType: TwoOrbitTripleType) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if uf6GenRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
        Array.init arrayToMutate.Length (fun i ->
            let rate = uf6GenRatesArray.Item(i)
            match rate.seedGenRatesUf6.PickMode floatPicker with
            | Seed6GenMode.Ortho1 -> ortho1Mutator arrayToMutate.[i]
            | Seed6GenMode.Ortho2 -> ortho2Mutator arrayToMutate.[i]
            | Seed6GenMode.Para1 -> para1Mutator arrayToMutate.[i]
            | Seed6GenMode.Para2 -> para2Mutator arrayToMutate.[i]
            | Seed6GenMode.Para3 -> para3Mutator arrayToMutate.[i]
            | Seed6GenMode.Para4 -> para4Mutator arrayToMutate.[i]
            | Seed6GenMode.SelfRefl -> selfReflMutator arrayToMutate.[i])

    let createNewItems<'a> 
        (uf6GenRatesArray: uf6GenRatesArray)
        (itemChooser: uf6GenRates -> 'a)
            : 'a[] =
        Array.init uf6GenRatesArray.Length (fun i ->
            itemChooser (uf6GenRatesArray.Item(i)))