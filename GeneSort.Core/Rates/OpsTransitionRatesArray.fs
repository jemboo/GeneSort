namespace GeneSort.Core
open System

[<Struct; CustomEquality; NoComparison>]
type opsTransitionRatesArray =
    private { rates: opsTransitionRates array }

    static member create (rates: opsTransitionRates array) 
                    : opsTransitionRatesArray =
        { rates = rates }

    member this.Length = this.rates.Length
    member this.Item(index: int) = this.rates.[index]
    member this.RatesArray = this.rates

    member this.toString() =
        String.Join(", ", Array.map (fun (r: opsTransitionRates) -> r.toString()) this.rates)

    override this.Equals(obj) =
        match obj with
        | :? opsTransitionRatesArray as other ->
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.rates other.rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.rates do
            hash <- hash * 23 + rate.GetHashCode()
        hash

    interface IEquatable<opsTransitionRatesArray> with
        member this.Equals(other) =
            if this.rates.Length <> other.rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.rates other.rates


module OpsTransitionRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation (length: int) (startRates: opsTransitionRates) (endRates: opsTransitionRates) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let ortho = opsActionRates.create(
                    startRates.OrthoRates.OrthoRate + t * (endRates.OrthoRates.OrthoRate - startRates.OrthoRates.OrthoRate),
                    startRates.OrthoRates.ParaRate + t * (endRates.OrthoRates.ParaRate - startRates.OrthoRates.ParaRate),
                    startRates.OrthoRates.SelfReflRate + t * (endRates.OrthoRates.SelfReflRate - startRates.OrthoRates.SelfReflRate))
                let para = opsActionRates.create(
                    startRates.ParaRates.OrthoRate + t * (endRates.ParaRates.OrthoRate - startRates.ParaRates.OrthoRate),
                    startRates.ParaRates.ParaRate + t * (endRates.ParaRates.ParaRate - startRates.ParaRates.ParaRate),
                    startRates.ParaRates.SelfReflRate + t * (endRates.ParaRates.SelfReflRate - startRates.ParaRates.SelfReflRate))
                let selfRefl = opsActionRates.create(
                    startRates.SelfReflRates.OrthoRate + t * (endRates.SelfReflRates.OrthoRate - startRates.SelfReflRates.OrthoRate),
                    startRates.SelfReflRates.ParaRate + t * (endRates.SelfReflRates.ParaRate - startRates.SelfReflRates.ParaRate),
                    startRates.SelfReflRates.SelfReflRate + t * (endRates.SelfReflRates.SelfReflRate - startRates.SelfReflRates.SelfReflRate))
                opsTransitionRates.create(ortho, para, selfRefl))
        opsTransitionRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation (length: int) (baseRates: opsTransitionRates) (amplitudes: opsTransitionRates) (frequency: float) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let ortho = opsActionRates.create(
                    clamp (baseRates.OrthoRates.OrthoRate + amplitudes.OrthoRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.OrthoRates.ParaRate + amplitudes.OrthoRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                    clamp (baseRates.OrthoRates.SelfReflRate + amplitudes.OrthoRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)
                let para = opsActionRates.create(
                    clamp (baseRates.ParaRates.OrthoRate + amplitudes.ParaRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.ParaRates.ParaRate + amplitudes.ParaRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                    clamp (baseRates.ParaRates.SelfReflRate + amplitudes.ParaRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)
                let selfRefl = opsActionRates.create(
                    clamp (baseRates.SelfReflRates.OrthoRate + amplitudes.SelfReflRates.OrthoRate * Math.Sin(t)) 0.0 1.0,
                    clamp (baseRates.SelfReflRates.ParaRate + amplitudes.SelfReflRates.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0,
                    clamp (baseRates.SelfReflRates.SelfReflRate + amplitudes.SelfReflRates.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0)
                opsTransitionRates.create(ortho, para, selfRefl))
        opsTransitionRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot (length: int) (baseRates: opsTransitionRates) (hotSpotIndex: int) (hotSpotRates: opsTransitionRates) (sigma: float) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let ortho = opsActionRates.create(
                    baseRates.OrthoRates.OrthoRate + (hotSpotRates.OrthoRates.OrthoRate - baseRates.OrthoRates.OrthoRate) * weight,
                    baseRates.OrthoRates.ParaRate + (hotSpotRates.OrthoRates.ParaRate - baseRates.OrthoRates.ParaRate) * weight,
                    baseRates.OrthoRates.SelfReflRate + (hotSpotRates.OrthoRates.SelfReflRate - baseRates.OrthoRates.SelfReflRate) * weight)
                let para = opsActionRates.create(
                    baseRates.ParaRates.OrthoRate + (hotSpotRates.ParaRates.OrthoRate - baseRates.ParaRates.OrthoRate) * weight,
                    baseRates.ParaRates.ParaRate + (hotSpotRates.ParaRates.ParaRate - baseRates.ParaRates.ParaRate) * weight,
                    baseRates.ParaRates.SelfReflRate + (hotSpotRates.ParaRates.SelfReflRate - baseRates.ParaRates.SelfReflRate) * weight)
                let selfRefl = opsActionRates.create(
                    baseRates.SelfReflRates.OrthoRate + (hotSpotRates.SelfReflRates.OrthoRate - baseRates.SelfReflRates.OrthoRate) * weight,
                    baseRates.SelfReflRates.ParaRate + (hotSpotRates.SelfReflRates.ParaRate - baseRates.SelfReflRates.ParaRate) * weight,
                    baseRates.SelfReflRates.SelfReflRate + (hotSpotRates.SelfReflRates.SelfReflRate - baseRates.SelfReflRates.SelfReflRate) * weight)
                opsTransitionRates.create(ortho, para, selfRefl))
        opsTransitionRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot (length: int) (baseRates: opsTransitionRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: opsTransitionRates) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                opsTransitionRates.create(
                    opsActionRates.create(rates.OrthoRates.OrthoRate, rates.OrthoRates.ParaRate, rates.OrthoRates.SelfReflRate),
                    opsActionRates.create(rates.ParaRates.OrthoRate, rates.ParaRates.ParaRate, rates.ParaRates.SelfReflRate),
                    opsActionRates.create(rates.SelfReflRates.OrthoRate, rates.SelfReflRates.ParaRate, rates.SelfReflRates.SelfReflRate)))
        opsTransitionRatesArray.create rates

    /// Mutates an array based on the provided rates. Returns a new array.
    /// Unlike IndelRatesArray, no length adjustments are needed since OpsMutationMode does not include insertion or deletion.
    let mutate<'a> 
        (opsTransitionRatesArray: opsTransitionRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (selfSymMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (twoOrbitType: TwoOrbitType) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if opsTransitionRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
    
        Array.init arrayToMutate.Length (fun i ->
            let rate = opsTransitionRatesArray.Item(i)
            match rate.PickMode floatPicker twoOrbitType with
            | opsActionMode.Ortho -> orthoMutator arrayToMutate.[i]
            | opsActionMode.Para -> paraMutator arrayToMutate.[i]
            | opsActionMode.SelfRefl -> selfSymMutator arrayToMutate.[i]
            | opsActionMode.NoAction -> arrayToMutate.[i])

    let createNewItems<'a> 
        (opsTransitionRatesArray: opsTransitionRatesArray)
        (itemChooser: opsTransitionRates -> 'a)
            : 'a[] =
        Array.init opsTransitionRatesArray.Length (fun i ->
            itemChooser (opsTransitionRatesArray.Item(i)))