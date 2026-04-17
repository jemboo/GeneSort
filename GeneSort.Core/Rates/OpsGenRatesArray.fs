namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type opsGenRatesArray =
    private 
        { 
            Rates: opsGenRates array 
            // Pre-calculated hash to avoid O(N) loops in Equals/GetHashCode
            CachedHash: int
        }

    static member create (rates: opsGenRates array) : opsGenRatesArray =
        if Array.isEmpty rates then failwith "opsGenRatesArray cannot be empty"
        
        // Calculate hash once at construction
        let mutable h = 17
        for i = 0 to rates.Length - 1 do
            h <- h * 23 + rates.[i].GetHashCode()
            
        { Rates = rates; CachedHash = h }

    member this.Length = this.Rates.Length
    member this.Item(index: int) = this.Rates.[index]
    member this.RatesArray = this.Rates
    
    member this.toString() =
        String.Join(", ", Array.map (fun r -> r.ToString()) this.Rates)

    override this.GetHashCode() = this.CachedHash

    override this.Equals(obj) =
        match obj with
        | :? opsGenRatesArray as other ->
            // Performance win: short-circuit equality check using the cached hash
            if this.CachedHash <> other.CachedHash then false
            elif this.Rates.Length <> other.Rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.Rates other.Rates
        | _ -> false

    interface IEquatable<opsGenRatesArray> with
        member this.Equals(other) =
            if this.CachedHash <> other.CachedHash then false
            elif this.Rates.Length <> other.Rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.Rates other.Rates

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

    let createUniform (length: int) : opsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates = Array.init length (fun _ -> opsGenRates.createUniform())
        opsGenRatesArray.create rates

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation (length: int) (startRates: opsGenRates) (endRates: opsGenRates) : opsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let o = startRates.OrthoRate + t * (endRates.OrthoRate - startRates.OrthoRate)
                let p = startRates.ParaRate + t * (endRates.ParaRate - startRates.ParaRate)
                let s = startRates.SelfReflRate + t * (endRates.SelfReflRate - startRates.SelfReflRate)
                let (oNorm, pNorm, sNorm) = normalizeRates o p s
                opsGenRates.create (oNorm, pNorm, sNorm))
        opsGenRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation (length: int) (baseRates: opsGenRates) (amplitudes: opsGenRates) (frequency: float) : opsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let o = clamp (baseRates.OrthoRate + amplitudes.OrthoRate * Math.Sin(t)) 0.0 1.0
                let p = clamp (baseRates.ParaRate + amplitudes.ParaRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0
                let s = clamp (baseRates.SelfReflRate + amplitudes.SelfReflRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0
                let (oNorm, pNorm, sNorm) = normalizeRates o p s
                opsGenRates.create (oNorm, pNorm, sNorm))
        opsGenRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot (length: int) (baseRates: opsGenRates) (hotSpotIndex: int) (hotSpotRates: opsGenRates) (sigma: float) : opsGenRatesArray =
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
                opsGenRates.create (oNorm, pNorm, sNorm))
        opsGenRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot (length: int) (baseRates: opsGenRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: opsGenRates) : opsGenRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                opsGenRates.create (rates.OrthoRate, rates.ParaRate, rates.SelfReflRate))
        opsGenRatesArray.create rates

    /// Mutates an array based on the provided rates. Returns a new array.
    /// No length adjustments are needed since OpsGenMode does not include insertion or deletion.
    let mutate<'a> 
        (opsGenRatesArray: opsGenRatesArray) 
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
            | opsGenMode.Ortho -> orthoMutator arrayToMutate.[i]
            | opsGenMode.Para -> paraMutator arrayToMutate.[i]
            | opsGenMode.SelfRefl -> selfSymMutator arrayToMutate.[i])