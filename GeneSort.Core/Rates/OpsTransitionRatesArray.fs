namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type opsTransitionRatesArray =
    private 
        { 
            opsTransitionRates: opsTransitionRates array 
            cachedHash: int
        }

    static member create (rates: opsTransitionRates array) : opsTransitionRatesArray =
        if Array.isEmpty rates then failwith "opsTransitionRatesArray cannot be empty"
        
        // Calculate hash once at construction to handle O(N) cost upfront
        let mutable h = 17
        for i = 0 to rates.Length - 1 do
            h <- h * 23 + rates.[i].GetHashCode()
            
        { opsTransitionRates = rates; cachedHash = h }

    member this.Length = this.opsTransitionRates.Length
    member this.Item(index: int) = this.opsTransitionRates.[index]
    member this.RatesArray = this.opsTransitionRates

    member this.toString() =
        String.Join(", ", Array.map (fun (r: opsTransitionRates) -> r.toString()) this.opsTransitionRates)

    override this.GetHashCode() = this.cachedHash

    override this.Equals(obj) =
        match obj with
        | :? opsTransitionRatesArray as other ->
            // Immediate O(1) exit if hashes differ
            if this.cachedHash <> other.cachedHash then false
            elif this.opsTransitionRates.Length <> other.opsTransitionRates.Length then false
            else
                Array.forall2 (fun (a: opsTransitionRates) b -> a.Equals(b)) this.opsTransitionRates other.opsTransitionRates
        | _ -> false

    interface IEquatable<opsTransitionRatesArray> with
        member this.Equals(other) =
            if this.cachedHash <> other.cachedHash then false
            elif this.opsTransitionRates.Length <> other.opsTransitionRates.Length then false
            else
                Array.forall2 (fun (a: opsTransitionRates) b -> a.Equals(b)) this.opsTransitionRates other.opsTransitionRates


module OpsTransitionRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    /// Linear interpolation between two opsActionRates at position t (0.0 = a, 1.0 = b).
    let private lerpActionRates (t: float) (a: opsActionRates) (b: opsActionRates) : opsActionRates =
        opsActionRates.create
            (a.OrthoRate + t * (b.OrthoRate - a.OrthoRate))
            (a.ParaRate + t * (b.ParaRate - a.ParaRate))
            (a.SelfReflRate + t * (b.SelfReflRate - a.SelfReflRate))

    /// Interpolates all three opsActionRates inside an opsTransitionRates.
    let private lerpTransitionRates (t: float) (a: opsTransitionRates) (b: opsTransitionRates) : opsTransitionRates =
        opsTransitionRates.create
            (lerpActionRates t a.OrthoRates b.OrthoRates)
            (lerpActionRates t a.ParaRates b.ParaRates)
            (lerpActionRates t a.SelfReflRates b.SelfReflRates)

    /// Sinusoidal offset of an opsActionRates around base, with phase shifts of 0, 2pi/3, 4pi/3.
    let private sinActionRates (t: float) (baseR: opsActionRates) (amp: opsActionRates) : opsActionRates =
        let phase2 = 2.0 * Math.PI / 3.0
        let phase3 = 4.0 * Math.PI / 3.0
        opsActionRates.create
            (clamp (baseR.OrthoRate + amp.OrthoRate * Math.Sin(t)) 0.0 1.0)
            (clamp (baseR.ParaRate + amp.ParaRate * Math.Sin(t + phase2)) 0.0 1.0)
            (clamp (baseR.SelfReflRate + amp.SelfReflRate * Math.Sin(t + phase3)) 0.0 1.0)

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation 
            (length: int) 
            (startRates: opsTransitionRates) 
            (endRates: opsTransitionRates) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = if length = 1 then 0.0 else float i / float (length - 1)
                lerpTransitionRates t startRates endRates)
        opsTransitionRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation 
            (length: int) 
            (baseRates: opsTransitionRates) 
            (amplitudes: opsTransitionRates) 
            (frequency: float) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = 
                    if length = 1 then 0.0 
                    else float i / float (length - 1) * 2.0 * Math.PI * frequency
                opsTransitionRates.create
                    (sinActionRates t baseRates.OrthoRates amplitudes.OrthoRates)
                    (sinActionRates t baseRates.ParaRates amplitudes.ParaRates)
                    (sinActionRates t baseRates.SelfReflRates amplitudes.SelfReflRates))
        opsTransitionRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot 
            (length: int) 
            (baseRates: opsTransitionRates) 
            (hotSpotIndex: int) 
            (hotSpotRates: opsTransitionRates) 
            (sigma: float) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                // weight = 0 -> baseRates, weight = 1 -> hotSpotRates
                lerpTransitionRates weight baseRates hotSpotRates)
        opsTransitionRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot 
            (length: int) 
            (baseRates: opsTransitionRates) 
            (hotSpotStart: int) 
            (hotSpotEnd: int) 
            (hotSpotRates: opsTransitionRates) : opsTransitionRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then 
            failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates)
        opsTransitionRatesArray.create rates

    /// Mutates an array based on the provided rates. Returns a new array.
    /// Unlike IndelRatesArray, no length adjustments are needed since OpsMutationMode does not include insertion or deletion.
    let mutate<'a> 
        (opsTransitionRatesArray: opsTransitionRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (selfSymMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (twoOrbitType: twoOrbitType) 
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