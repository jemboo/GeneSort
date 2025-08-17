namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type OpsGenRatesArray =

    private { opsGenRatesArray: OpsGenRates array }
    static member create (opsGenRatesArray: OpsGenRates array) : OpsGenRatesArray =
        { opsGenRatesArray = opsGenRatesArray }
    member this.Length = this.opsGenRatesArray.Length
    member this.Item(index: int) = this.opsGenRatesArray.[index]
    member this.RatesArray = this.opsGenRatesArray
    member this.toString() =
        String.Join(", ", Array.map (fun r -> r.ToString()) this.opsGenRatesArray)

    override this.Equals(obj) =
        match obj with
        | :? OpsGenRatesArray as other ->
            if this.opsGenRatesArray.Length <> other.opsGenRatesArray.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.opsGenRatesArray other.opsGenRatesArray
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.opsGenRatesArray do
            hash <- hash * 23 + rate.GetHashCode()
        hash

    interface IEquatable<OpsGenRatesArray> with
        member this.Equals(other) =
            if this.opsGenRatesArray.Length <> other.opsGenRatesArray.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.opsGenRatesArray other.opsGenRatesArray


module OpsGenRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let private normalizeRates (ortho: float) (para: float) (selfSym: float) : float * float * float =
        let sum = ortho + para + selfSym
        if sum <= 0.0 then
            (1.0/3.0, 1.0/3.0, 1.0/3.0) // Default to equal distribution if sum is zero
        else
            let scale = 1.0 / sum
            (ortho * scale, para * scale, selfSym * scale)

    let createUniform (length: int) : OpsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates = Array.init length (fun _ -> OpsGenRates.createUniform())
        OpsGenRatesArray.create rates

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation (length: int) (startRates: OpsGenRates) (endRates: OpsGenRates) : OpsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let o = startRates.OrthoRate + t * (endRates.OrthoRate - startRates.OrthoRate)
                let p = startRates.ParaRate + t * (endRates.ParaRate - startRates.ParaRate)
                let s = startRates.SelfReflRate + t * (endRates.SelfReflRate - startRates.SelfReflRate)
                let (oNorm, pNorm, sNorm) = normalizeRates o p s
                OpsGenRates.create (oNorm, pNorm, sNorm))
        OpsGenRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation (length: int) (baseRates: OpsGenRates) (amplitudes: OpsGenRates) (frequency: float) : OpsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let o = clamp (baseRates.OrthoRate + amplitudes.OrthoRate * Math.Sin(t)) 0.0 1.0
                let p = clamp (baseRates.ParaRate + amplitudes.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0
                let s = clamp (baseRates.SelfReflRate + amplitudes.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0
                let (oNorm, pNorm, sNorm) = normalizeRates o p s
                OpsGenRates.create (oNorm, pNorm, sNorm))
        OpsGenRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot (length: int) (baseRates: OpsGenRates) (hotSpotIndex: int) (hotSpotRates: OpsGenRates) (sigma: float) : OpsGenRatesArray =
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
                let (oNorm, pNorm, sNorm) = normalizeRates o p s
                OpsGenRates.create (oNorm, pNorm, sNorm))
        OpsGenRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot (length: int) (baseRates: OpsGenRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: OpsGenRates) : OpsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                OpsGenRates.create (rates.OrthoRate, rates.ParaRate, rates.SelfReflRate))
        OpsGenRatesArray.create rates

    /// Mutates an array based on the provided rates. Returns a new array.
    /// No length adjustments are needed since OpsGenMode does not include insertion or deletion.
    let mutate<'a> 
        (opsGenRatesArray: OpsGenRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (selfSymMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if opsGenRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
    
        Array.init arrayToMutate.Length (fun i ->
            let rate = opsGenRatesArray.Item(i)
            match rate.PickMode floatPicker with
            | OpsGenMode.Ortho -> orthoMutator arrayToMutate.[i]
            | OpsGenMode.Para -> paraMutator arrayToMutate.[i]
            | OpsGenMode.SelfRefl -> selfSymMutator arrayToMutate.[i])