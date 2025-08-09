namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type OpsActionRatesArray =

    private { rates: OpsActionRates array }
    static member create (rates: OpsActionRates array) : OpsActionRatesArray =
        if Array.isEmpty rates then failwith "Rates array cannot be empty"
        { rates = rates }
    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates
    member this.toString() =
        String.Join(", ", Array.map (fun r -> r.ToString()) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? OpsActionRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.rates other.rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.rates do
            hash <- hash * 23 + rate.GetHashCode()
        hash

    interface IEquatable<OpsActionRatesArray> with
        member this.Equals(other) =
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.rates other.rates


module OpsActionRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation (length: int) (startRates: OpsActionRates) (endRates: OpsActionRates) : OpsActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let o = startRates.OrthoRate + t * (endRates.OrthoRate - startRates.OrthoRate)
                let p = startRates.ParaRate + t * (endRates.ParaRate - startRates.ParaRate)
                let s = startRates.SelfReflRate + t * (endRates.SelfReflRate - startRates.SelfReflRate)
                OpsActionRates.create (o, p, s))
        OpsActionRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation (length: int) (baseRates: OpsActionRates) (amplitudes: OpsActionRates) (frequency: float) : OpsActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let o = clamp (baseRates.OrthoRate + amplitudes.OrthoRate * Math.Sin(t)) 0.0 1.0
                let p = clamp (baseRates.ParaRate + amplitudes.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0
                let s = clamp (baseRates.SelfReflRate + amplitudes.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0
                OpsActionRates.create (o, p, s))
        OpsActionRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot (length: int) (baseRates: OpsActionRates) (hotSpotIndex: int) (hotSpotRates: OpsActionRates) (sigma: float) : OpsActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let o = baseRates.OrthoRate + (hotSpotRates.OrthoRate - baseRates.OrthoRate) * weight
                let p = baseRates.ParaRate + (hotSpotRates.ParaRate - baseRates.ParaRate) * weight
                let s = baseRates.SelfReflRate + (hotSpotRates.SelfReflRate - baseRates.SelfReflRate) * weight
                OpsActionRates.create (o, p, s))
        OpsActionRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot (length: int) (baseRates: OpsActionRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: OpsActionRates) : OpsActionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                OpsActionRates.create (rates.OrthoRate, rates.ParaRate, rates.SelfReflRate))
        OpsActionRatesArray.create rates

    /// Mutates an array based on the provided rates. Returns a new array.
    /// Unlike IndelRatesArray, no length adjustments are needed since OpsMutationMode does not include insertion or deletion.
    let mutate<'a> 
        (opsActionRatesArray: OpsActionRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (selfSymMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if opsActionRatesArray.Length <> arrayToMutate.Length then
            failwith "ARRAY length does not match rates length"
    
        Array.init arrayToMutate.Length (fun i ->
            let rate = opsActionRatesArray.Item(i)
            match rate.PickMode floatPicker with
            | OpsActionMode.Ortho -> orthoMutator arrayToMutate.[i]
            | OpsActionMode.Para -> paraMutator arrayToMutate.[i]
            | OpsActionMode.SelfRefl -> selfSymMutator arrayToMutate.[i]
            | OpsActionMode.NoAction -> arrayToMutate.[i])


    let createNewItems<'a> 
        (opsActionRatesArray: OpsActionRatesArray)
        (itemChooser: OpsActionRates -> 'a)
            : 'a[] =
        Array.init opsActionRatesArray.Length (fun i ->
            itemChooser (opsActionRatesArray.Item(i)) )
