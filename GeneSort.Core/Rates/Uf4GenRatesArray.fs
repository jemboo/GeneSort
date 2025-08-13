namespace GeneSort.Core

open System
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type Uf4GenRatesArray =
    private 
        { rates: Uf4GenRates array }

    static member create (rates: Uf4GenRates array) : Uf4GenRatesArray =
        if Array.exists (fun r -> r.order < 4 || r.order % 4 <> 0) rates then
            failwith "All Uf4GenRates orders must be at least 4 and divisible by 4"
        let arrayLengths = rates |> Array.map (fun r -> r.opsGenRatesArray.Length)
        if Array.distinct arrayLengths |> Array.length > 1 then
            failwith "All Uf4GenRates must have the same opsGenRatesArray length"
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (
            fun r -> sprintf "Uf4GenRates(order=%d, seed=%s, arrayLength=%d)" 
                        r.order (r.seedOpsGenRates.toString()) r.opsGenRatesArray.Length) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? Uf4GenRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedOpsGenRates.Equals(b.seedOpsGenRates) && 
                    a.opsGenRatesArray.Equals(b.opsGenRatesArray)) this.rates other.rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.rates do
            hash <- hash * 23 + rate.order.GetHashCode()
            hash <- hash * 23 + rate.seedOpsGenRates.GetHashCode()
            hash <- hash * 23 + rate.opsGenRatesArray.GetHashCode()
        hash

    interface IEquatable<Uf4GenRatesArray> with
        member this.Equals(other) =
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedOpsGenRates.Equals(b.seedOpsGenRates) && 
                    a.opsGenRatesArray.Equals(b.opsGenRatesArray)) this.rates other.rates

module Uf4GenRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let createLinearVariation (length: int) (order: int) (startRates: Uf4GenRates) (endRates: Uf4GenRates) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        if startRates.opsGenRatesArray.Length <> endRates.opsGenRatesArray.Length then
            failwith "Start and end rates must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let seed = OpsGenRates.create(
                    startRates.seedOpsGenRates.OrthoRate + t * (endRates.seedOpsGenRates.OrthoRate - startRates.seedOpsGenRates.OrthoRate),
                    startRates.seedOpsGenRates.ParaRate + t * (endRates.seedOpsGenRates.ParaRate - startRates.seedOpsGenRates.ParaRate),
                    startRates.seedOpsGenRates.SelfReflRate + t * (endRates.seedOpsGenRates.SelfReflRate - startRates.seedOpsGenRates.SelfReflRate))
                let arrayLength = MathUtils.exactLog2 (order / 4)
                let opsGenRatesArray =
                    Array.init arrayLength (fun j ->
                        let startArray = startRates.opsGenRatesArray.RatesArray
                        let endArray = endRates.opsGenRatesArray.RatesArray
                        OpsGenRates.create(
                            startArray.[j].OrthoRate + t * (endArray.[j].OrthoRate - startArray.[j].OrthoRate),
                            startArray.[j].ParaRate + t * (endArray.[j].ParaRate - startArray.[j].ParaRate),
                            startArray.[j].SelfReflRate + t * (endArray.[j].SelfReflRate - startArray.[j].SelfReflRate)))
                { Uf4GenRates.order = order
                  seedOpsGenRates = seed
                  opsGenRatesArray = OpsGenRatesArray.create opsGenRatesArray })
        Uf4GenRatesArray.create rates

    let createSinusoidalVariation (length: int) (order: int) (baseRates: Uf4GenRates) (amplitudes: Uf4GenRates) (frequency: float) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        if baseRates.opsGenRatesArray.Length <> amplitudes.opsGenRatesArray.Length then
            failwith "Base and amplitudes must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let seed = OpsGenRates.create(
                    clamp (baseRates.seedOpsGenRates.OrthoRate + amplitudes.seedOpsGenRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.seedOpsGenRates.ParaRate + amplitudes.seedOpsGenRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                    clamp (baseRates.seedOpsGenRates.SelfReflRate + amplitudes.seedOpsGenRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)
                let arrayLength = MathUtils.exactLog2 (order / 4)
                let opsGenRatesArray =
                    Array.init arrayLength (fun j ->
                        let baseArray = baseRates.opsGenRatesArray.RatesArray
                        let ampArray = amplitudes.opsGenRatesArray.RatesArray
                        OpsGenRates.create(
                            clamp (baseArray.[j].OrthoRate + ampArray.[j].OrthoRate * Math.Sin(t)) 0.0 1.0,
                            clamp (baseArray.[j].ParaRate + ampArray.[j].ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                            clamp (baseArray.[j].SelfReflRate + ampArray.[j].SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0))
                { Uf4GenRates.order = order
                  seedOpsGenRates = seed
                  opsGenRatesArray = OpsGenRatesArray.create opsGenRatesArray })
        Uf4GenRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: Uf4GenRates) (hotSpotIndex: int) (hotSpotRates: Uf4GenRates) (sigma: float) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if baseRates.opsGenRatesArray.Length <> hotSpotRates.opsGenRatesArray.Length then
            failwith "Base and hotspot rates must have the same opsGenRatesArray length"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let seed = OpsGenRates.create(
                    baseRates.seedOpsGenRates.OrthoRate + (hotSpotRates.seedOpsGenRates.OrthoRate - baseRates.seedOpsGenRates.OrthoRate) * weight,
                    baseRates.seedOpsGenRates.ParaRate + (hotSpotRates.seedOpsGenRates.ParaRate - baseRates.seedOpsGenRates.ParaRate) * weight,
                    baseRates.seedOpsGenRates.SelfReflRate + (hotSpotRates.seedOpsGenRates.SelfReflRate - baseRates.seedOpsGenRates.SelfReflRate) * weight)
                let arrayLength = MathUtils.exactLog2 (order / 4)
                let opsGenRatesArray =
                    Array.init arrayLength (fun j ->
                        let baseArray = baseRates.opsGenRatesArray.RatesArray
                        let hotSpotArray = hotSpotRates.opsGenRatesArray.RatesArray
                        OpsGenRates.create(
                            baseArray.[j].OrthoRate + (hotSpotArray.[j].OrthoRate - baseArray.[j].OrthoRate) * weight,
                            baseArray.[j].ParaRate + (hotSpotArray.[j].ParaRate - baseArray.[j].ParaRate) * weight,
                            baseArray.[j].SelfReflRate + (hotSpotArray.[j].SelfReflRate - baseArray.[j].SelfReflRate) * weight))
                { Uf4GenRates.order = order
                  seedOpsGenRates = seed
                  opsGenRatesArray = OpsGenRatesArray.create opsGenRatesArray })
        Uf4GenRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: Uf4GenRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: Uf4GenRates) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if baseRates.opsGenRatesArray.Length <> hotSpotRates.opsGenRatesArray.Length then
            failwith "Base and hotspot rates must have the same opsGenRatesArray length"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                { order = order
                  seedOpsGenRates = OpsGenRates.create(
                      rates.seedOpsGenRates.OrthoRate,
                      rates.seedOpsGenRates.ParaRate,
                      rates.seedOpsGenRates.SelfReflRate)
                  opsGenRatesArray = rates.opsGenRatesArray })
        Uf4GenRatesArray.create rates

    let createNewItems<'a> 
        (uf4GenRatesArray: Uf4GenRatesArray)
        (itemChooser: Uf4GenRates -> 'a)
            : 'a[] =
        Array.init uf4GenRatesArray.Length (fun i ->
            itemChooser (uf4GenRatesArray.Item(i)))
