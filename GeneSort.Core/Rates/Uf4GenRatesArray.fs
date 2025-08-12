namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type Uf4GenRatesArray =
    private 
        { rates: Uf4GenRates array }

    static member create (rates: Uf4GenRates array) : Uf4GenRatesArray =
        if Array.isEmpty rates then failwith "Rates array cannot be empty"
        if Array.exists (fun r -> r.order < 4 || r.order % 2 <> 0) rates then
            failwith "All Uf4GenRates orders must be at least 4 and even"
        let listLengths = rates |> Array.map (fun r -> r.opsGenRatesArray.Length)
        if Array.distinct listLengths |> Array.length > 1 then
            failwith "All Uf4GenRates must have the same opsGenRatesList length"
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (
            fun r -> sprintf "Uf4GenRates(order=%d, seed=%s, listLength=%d)" 
                        r.order (r.seedOpsGenRates.toString()) r.opsGenRatesArray.Length) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? Uf4GenRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> 
                    a.order = b.order && 
                    a.seedOpsGenRates.Equals(b.seedOpsGenRates) && 
                    a.opsGenRatesArray = b.opsGenRatesArray) this.rates other.rates
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
                    a.opsGenRatesArray = b.opsGenRatesArray) this.rates other.rates

module Uf4GenRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let createLinearVariation (length: int) (order: int) (startRates: Uf4GenRates) (endRates: Uf4GenRates) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        if startRates.opsGenRatesArray.Length <> endRates.opsGenRatesArray.Length then
            failwith "Start and end rates must have the same opsGenRatesList length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let seed = OpsGenRates.create(
                    startRates.seedOpsGenRates.OrthoRate + t * (endRates.seedOpsGenRates.OrthoRate - startRates.seedOpsGenRates.OrthoRate),
                    startRates.seedOpsGenRates.ParaRate + t * (endRates.seedOpsGenRates.ParaRate - startRates.seedOpsGenRates.ParaRate),
                    startRates.seedOpsGenRates.SelfReflRate + t * (endRates.seedOpsGenRates.SelfReflRate - startRates.seedOpsGenRates.SelfReflRate))
                let listLength = MathUtils.exactLog2 (order / 4)
                let opsGenRatesList =
                    Array.init listLength (fun j ->
                        let startList = startRates.opsGenRatesArray
                        let endList = endRates.opsGenRatesArray
                        if j >= startList.Length || j >= endList.Length then OpsGenRates.createUniform()
                        else
                            OpsGenRates.create(
                                startList.[j].OrthoRate + t * (endList.[j].OrthoRate - startList.[j].OrthoRate),
                                startList.[j].ParaRate + t * (endList.[j].ParaRate - startList.[j].ParaRate),
                                startList.[j].SelfReflRate + t * (endList.[j].SelfReflRate - startList.[j].SelfReflRate)))
                { Uf4GenRates.order = order; seedOpsGenRates = seed; opsGenRatesArray = opsGenRatesList })
        Uf4GenRatesArray.create rates

    let createSinusoidalVariation (length: int) (order: int) (baseRates: Uf4GenRates) (amplitudes: Uf4GenRates) (frequency: float) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        if baseRates.opsGenRatesArray.Length <> amplitudes.opsGenRatesArray.Length then
            failwith "Base and amplitudes must have the same opsGenRatesList length"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let seed = OpsGenRates.create(
                    clamp (baseRates.seedOpsGenRates.OrthoRate + amplitudes.seedOpsGenRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.seedOpsGenRates.ParaRate + amplitudes.seedOpsGenRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                    clamp (baseRates.seedOpsGenRates.SelfReflRate + amplitudes.seedOpsGenRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)
                let listLength = MathUtils.exactLog2 (order / 4)
                let opsGenRatesList =
                    Array.init listLength (fun j ->
                        let baseList = baseRates.opsGenRatesArray
                        let ampList = amplitudes.opsGenRatesArray
                        if j >= baseList.Length || j >= ampList.Length then OpsGenRates.createUniform()
                        else
                            OpsGenRates.create(
                                clamp (baseList.[j].OrthoRate + ampList.[j].OrthoRate * Math.Sin(t)) 0.0 1.0,
                                clamp (baseList.[j].ParaRate + ampList.[j].ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                                clamp (baseList.[j].SelfReflRate + ampList.[j].SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0))
                { Uf4GenRates.order = order; seedOpsGenRates = seed; opsGenRatesArray = opsGenRatesList })
        Uf4GenRatesArray.create rates

    let createGaussianHotSpot (length: int) (order: int) (baseRates: Uf4GenRates) (hotSpotIndex: int) (hotSpotRates: Uf4GenRates) (sigma: float) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if baseRates.opsGenRatesArray.Length <> hotSpotRates.opsGenRatesArray.Length then
            failwith "Base and hotspot rates must have the same opsGenRatesList length"
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
                let listLength = MathUtils.exactLog2 (order / 4)
                let opsGenRatesList =
                    Array.init listLength (fun j ->
                        let baseList = baseRates.opsGenRatesArray
                        let hotSpotList = hotSpotRates.opsGenRatesArray
                        if j >= baseList.Length || j >= hotSpotList.Length then OpsGenRates.createUniform()
                        else
                            OpsGenRates.create(
                                baseList.[j].OrthoRate + (hotSpotList.[j].OrthoRate - baseList.[j].OrthoRate) * weight,
                                baseList.[j].ParaRate + (hotSpotList.[j].ParaRate - baseList.[j].ParaRate) * weight,
                                baseList.[j].SelfReflRate + (hotSpotList.[j].SelfReflRate - baseList.[j].SelfReflRate) * weight))
                { Uf4GenRates.order = order; seedOpsGenRates = seed; opsGenRatesArray = opsGenRatesList })
        Uf4GenRatesArray.create rates

    let createStepHotSpot (length: int) (order: int) (baseRates: Uf4GenRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: Uf4GenRates) : Uf4GenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if order < 4 || order % 2 <> 0 then failwith "Order must be at least 4 and even"
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if baseRates.opsGenRatesArray.Length <> hotSpotRates.opsGenRatesArray.Length then
            failwith "Base and hotspot rates must have the same opsGenRatesList length"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                { Uf4GenRates.order = order; 
                  seedOpsGenRates = OpsGenRates.create(rates.seedOpsGenRates.OrthoRate, rates.seedOpsGenRates.ParaRate, rates.seedOpsGenRates.SelfReflRate);
                  opsGenRatesArray = rates.opsGenRatesArray })
        Uf4GenRatesArray.create rates

    let createNewItems<'a> 
        (uf4GenRatesArray: Uf4GenRatesArray)
        (itemChooser: Uf4GenRates -> 'a)
            : 'a[] =
        Array.init uf4GenRatesArray.Length (fun i ->
            itemChooser (uf4GenRatesArray.Item(i)))