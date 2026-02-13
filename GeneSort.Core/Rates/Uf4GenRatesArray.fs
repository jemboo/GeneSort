namespace GeneSort.Core

open System
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type uf4GenRatesArray =
    private 
        { rates: uf4GenRates array }

    static member create (rates: uf4GenRates array) : uf4GenRatesArray =
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
        | :? uf4GenRatesArray as other ->
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

    interface IEquatable<uf4GenRatesArray> with
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


    let createUniform (length: int) (order: int) : uf4GenRatesArray =
        let rates = Array.init length (fun _ -> Uf4GenRates.makeUniform order)
        uf4GenRatesArray.create rates


    let createLinearVariation (length: int) (order: int) (startRates: uf4GenRates) (endRates: uf4GenRates) : uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        if startRates.opsGenRatesArray.Length <> endRates.opsGenRatesArray.Length then
            failwith "Start and end rates must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let seed = opsGenRates.create(
                    startRates.seedOpsGenRates.OrthoRate + t * (endRates.seedOpsGenRates.OrthoRate - startRates.seedOpsGenRates.OrthoRate),
                    startRates.seedOpsGenRates.ParaRate + t * (endRates.seedOpsGenRates.ParaRate - startRates.seedOpsGenRates.ParaRate),
                    startRates.seedOpsGenRates.SelfReflRate + t * (endRates.seedOpsGenRates.SelfReflRate - startRates.seedOpsGenRates.SelfReflRate))
                let arrayLength = MathUtils.exactLog2 (order / 4)
                let ogra =
                    Array.init arrayLength (fun j ->
                        let startArray = startRates.opsGenRatesArray.RatesArray
                        let endArray = endRates.opsGenRatesArray.RatesArray
                        opsGenRates.create(
                            startArray.[j].OrthoRate + t * (endArray.[j].OrthoRate - startArray.[j].OrthoRate),
                            startArray.[j].ParaRate + t * (endArray.[j].ParaRate - startArray.[j].ParaRate),
                            startArray.[j].SelfReflRate + t * (endArray.[j].SelfReflRate - startArray.[j].SelfReflRate)))

                uf4GenRates.create order seed (opsGenRatesArray.create ogra) )
        uf4GenRatesArray.create rates


    let createSinusoidalVariation (length: int) (order: int) (baseRates: uf4GenRates) (amplitudes: uf4GenRates) (frequency: float) : uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        if baseRates.opsGenRatesArray.Length <> amplitudes.opsGenRatesArray.Length then
            failwith "Base and amplitudes must have the same opsGenRatesArray length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let seed = opsGenRates.create(
                    clamp (baseRates.seedOpsGenRates.OrthoRate + amplitudes.seedOpsGenRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.seedOpsGenRates.ParaRate + amplitudes.seedOpsGenRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                    clamp (baseRates.seedOpsGenRates.SelfReflRate + amplitudes.seedOpsGenRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)
                let arrayLength = MathUtils.exactLog2 (order / 4)
                let ogra =
                    Array.init arrayLength (fun j ->
                        let baseArray = baseRates.opsGenRatesArray.RatesArray
                        let ampArray = amplitudes.opsGenRatesArray.RatesArray
                        opsGenRates.create(
                            clamp (baseArray.[j].OrthoRate + ampArray.[j].OrthoRate * Math.Sin(t)) 0.0 1.0,
                            clamp (baseArray.[j].ParaRate + ampArray.[j].ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                            clamp (baseArray.[j].SelfReflRate + ampArray.[j].SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0))
                uf4GenRates.create order seed (opsGenRatesArray.create ogra) )
        uf4GenRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: uf4GenRates) (hotSpotIndex: int) (hotSpotRates: uf4GenRates) (sigma: float) : uf4GenRatesArray =
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
                let seed = opsGenRates.create(
                    baseRates.seedOpsGenRates.OrthoRate + (hotSpotRates.seedOpsGenRates.OrthoRate - baseRates.seedOpsGenRates.OrthoRate) * weight,
                    baseRates.seedOpsGenRates.ParaRate + (hotSpotRates.seedOpsGenRates.ParaRate - baseRates.seedOpsGenRates.ParaRate) * weight,
                    baseRates.seedOpsGenRates.SelfReflRate + (hotSpotRates.seedOpsGenRates.SelfReflRate - baseRates.seedOpsGenRates.SelfReflRate) * weight)
                let arrayLength = MathUtils.exactLog2 (order / 4)
                let ogra =
                    Array.init arrayLength (fun j ->
                        let baseArray = baseRates.opsGenRatesArray.RatesArray
                        let hotSpotArray = hotSpotRates.opsGenRatesArray.RatesArray
                        opsGenRates.create(
                            baseArray.[j].OrthoRate + (hotSpotArray.[j].OrthoRate - baseArray.[j].OrthoRate) * weight,
                            baseArray.[j].ParaRate + (hotSpotArray.[j].ParaRate - baseArray.[j].ParaRate) * weight,
                            baseArray.[j].SelfReflRate + (hotSpotArray.[j].SelfReflRate - baseArray.[j].SelfReflRate) * weight))
                uf4GenRates.create order seed (opsGenRatesArray.create ogra) )

        uf4GenRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: uf4GenRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: uf4GenRates) : uf4GenRatesArray =
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
                  seedOpsGenRates = opsGenRates.create(
                      rates.seedOpsGenRates.OrthoRate,
                      rates.seedOpsGenRates.ParaRate,
                      rates.seedOpsGenRates.SelfReflRate)
                  opsGenRatesArray = rates.opsGenRatesArray })
        uf4GenRatesArray.create rates

    let createNewItems<'a> 
        (uf4GenRatesArray: uf4GenRatesArray)
        (itemChooser: uf4GenRates -> 'a)
            : 'a[] =
        Array.init uf4GenRatesArray.Length (fun i ->
            itemChooser (uf4GenRatesArray.Item(i)))
