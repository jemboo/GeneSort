namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type OpActionRatesArray =

    private { Rates: OpActionRates array }
    static member create (rates: OpActionRates array) : OpActionRatesArray =
        { Rates = rates }
    member this.Length = this.Rates.Length
    member this.Item(index: int) = this.Rates.[index]
    member this.RatesArray = this.Rates
    member this.toString() =
        String.Join(", ", Array.map (fun r -> r.ToString()) this.Rates)

    override this.Equals(obj) =
        match obj with
        | :? OpActionRatesArray as other ->
            if this.Rates.Length <> other.Rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.Rates other.Rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.Rates do
            hash <- hash * 23 + rate.GetHashCode()
        hash

    interface IEquatable<OpActionRatesArray> with
        member this.Equals(other) =
            if this.Rates.Length <> other.Rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.Rates other.Rates

module OpActionRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation (length: int) (startRates: OpActionRates) (endRates: OpActionRates) : OpActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let o = startRates.OrthoRate + t * (endRates.OrthoRate - startRates.OrthoRate)
                let p = startRates.ParaRate + t * (endRates.ParaRate - startRates.ParaRate)
                OpActionRates.create (o, p))
        OpActionRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation (length: int) (baseRates: OpActionRates) (amplitudes: OpActionRates) (frequency: float) : OpActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let o = clamp (baseRates.OrthoRate + amplitudes.OrthoRate * Math.Sin(t)) 0.0 1.0
                let p = clamp (baseRates.ParaRate + amplitudes.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0
                OpActionRates.create (o, p))
        OpActionRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot (length: int) (baseRates: OpActionRates) (hotSpotIndex: int) (hotSpotRates: OpActionRates) (sigma: float) : OpActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let o = baseRates.OrthoRate + (hotSpotRates.OrthoRate - baseRates.OrthoRate) * weight
                let p = baseRates.ParaRate + (hotSpotRates.ParaRate - baseRates.ParaRate) * weight
                OpActionRates.create (o, p))
        OpActionRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot (length: int) (baseRates: OpActionRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: OpActionRates) : OpActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                OpActionRates.create (rates.OrthoRate, rates.ParaRate))
        OpActionRatesArray.create rates

    /// Mutates an arrayToMutate based on the provided rates. Returns a new array.
    /// Unlike IndelRatesArray, no length adjustments are needed since SiMutationMode does not include insertion or deletion.
    let mutate<'a> 
        (opActionRatesArray: OpActionRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if opActionRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
    
        Array.init arrayToMutate.Length (fun i ->
            let rate = opActionRatesArray.Item(i)
            match rate.PickMode floatPicker with
            | OpActionMode.Ortho -> orthoMutator arrayToMutate.[i]
            | OpActionMode.Para -> paraMutator arrayToMutate.[i]
            | OpActionMode.NoAction -> arrayToMutate.[i])