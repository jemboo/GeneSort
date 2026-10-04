namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type uf4MutationRatesArray =
    private 
        { 
            uf4MutationRates: uf4MutationRates array 
            cachedHash: int
        }

    static member create (rates: uf4MutationRates array) : uf4MutationRatesArray =
        if Array.exists (fun (r: uf4MutationRates) -> r.order < 4 || r.order % 4 <> 0) rates then
            failwith "All Uf4MutationRates orders must be at least 4 and divisible by 4"
        
        // Calculate hash once at construction
        let mutable h = 17
        for i = 0 to rates.Length - 1 do
            let r = rates.[i]
            h <- h * 23 + r.order.GetHashCode()
            h <- h * 23 + r.seedOpsTransitionRates.GetHashCode()
            h <- h * 23 + r.twoOrbitPairOpsTransitionRates.GetHashCode()
            
        { uf4MutationRates = rates; cachedHash = h }

    member this.Length = this.uf4MutationRates.Length
    member this.Item(index: int) = this.uf4MutationRates.[index]
    member this.RatesArray = this.uf4MutationRates

    member this.toString() =
        String.Join(", ", Array.map (
            fun (r: uf4MutationRates) -> 
                sprintf "Uf4MutationRates(order=%d, seed=%s, arrayLength=%d)" 
                        r.order (r.seedOpsTransitionRates.toString()) r.twoOrbitPairOpsTransitionRates.Length) this.uf4MutationRates)

    override this.GetHashCode() = this.cachedHash

    override this.Equals(obj) =
        match obj with
        | :? uf4MutationRatesArray as other ->
            // O(1) short-circuit
            if this.cachedHash <> other.cachedHash then false
            elif this.uf4MutationRates.Length <> other.uf4MutationRates.Length then false
            else
                Array.forall2 (fun (a: uf4MutationRates) (b: uf4MutationRates) -> 
                    a.order = b.order && 
                    a.seedOpsTransitionRates.Equals(b.seedOpsTransitionRates) && 
                    a.twoOrbitPairOpsTransitionRates.Equals(b.twoOrbitPairOpsTransitionRates)) this.uf4MutationRates other.uf4MutationRates
        | _ -> false

    interface IEquatable<uf4MutationRatesArray> with
        member this.Equals(other) =
            if this.cachedHash <> other.cachedHash then false
            elif this.uf4MutationRates.Length <> other.uf4MutationRates.Length then false
            else
                Array.forall2 (fun (a: uf4MutationRates) (b: uf4MutationRates) -> 
                    a.order = b.order && 
                    a.seedOpsTransitionRates.Equals(b.seedOpsTransitionRates) && 
                    a.twoOrbitPairOpsTransitionRates.Equals(b.twoOrbitPairOpsTransitionRates)) this.uf4MutationRates other.uf4MutationRates


module Uf4MutationRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    // ---------- opsActionRates / opsTransitionRates helpers ----------

    /// Linear interpolation between two opsActionRates (t = 0 -> a, t = 1 -> b).
    let private lerpActionRates (t: float) (a: opsActionRates) (b: opsActionRates) : opsActionRates =
        opsActionRates.create
            (a.OrthoRate + t * (b.OrthoRate - a.OrthoRate))
            (a.ParaRate + t * (b.ParaRate - a.ParaRate))
            (a.SelfReflRate + t * (b.SelfReflRate - a.SelfReflRate))

    let private lerpOps (t: float) (a: opsTransitionRates) (b: opsTransitionRates) : opsTransitionRates =
        opsTransitionRates.create
            (lerpActionRates t a.OrthoRates b.OrthoRates)
            (lerpActionRates t a.ParaRates b.ParaRates)
            (lerpActionRates t a.SelfReflRates b.SelfReflRates)

    /// Sinusoidal offset around base, with phase shifts of 0, 2pi/3, 4pi/3 for the three rates.
    let private sinActionRates (t: float) (baseR: opsActionRates) (amp: opsActionRates) : opsActionRates =
        let phase2 = 2.0 * Math.PI / 3.0
        let phase3 = 4.0 * Math.PI / 3.0
        opsActionRates.create
            (clamp (baseR.OrthoRate + amp.OrthoRate * Math.Sin(t)) 0.0 1.0)
            (clamp (baseR.ParaRate + amp.ParaRate * Math.Sin(t + phase2)) 0.0 1.0)
            (clamp (baseR.SelfReflRate + amp.SelfReflRate * Math.Sin(t + phase3)) 0.0 1.0)

    let private sinOps (t: float) (baseR: opsTransitionRates) (amp: opsTransitionRates) : opsTransitionRates =
        opsTransitionRates.create
            (sinActionRates t baseR.OrthoRates amp.OrthoRates)
            (sinActionRates t baseR.ParaRates amp.ParaRates)
            (sinActionRates t baseR.SelfReflRates amp.SelfReflRates)

    // ---------- uf4MutationRates helpers ----------

    let private validateOrder (order: int) =
        if order < 4 || order % 4 <> 0 then failwith "Order must be at least 4 and divisible by 4"

    /// Combines two uf4MutationRates element-wise (seed, then each twoOrbitPair entry) using the given
    /// opsTransitionRates combiner.
    let private combine 
            (order: int) 
            (f: opsTransitionRates -> opsTransitionRates -> opsTransitionRates)
            (a: uf4MutationRates) 
            (b: uf4MutationRates) : uf4MutationRates =
        let listLength = MathUtils.exactLog2 (order / 4)
        let aArray = a.twoOrbitPairOpsTransitionRates.RatesArray
        let bArray = b.twoOrbitPairOpsTransitionRates.RatesArray
        let otra = Array.init listLength (fun j -> f aArray.[j] bArray.[j])
        { uf4MutationRates.order = order
          seedOpsTransitionRates = f a.seedOpsTransitionRates b.seedOpsTransitionRates
          twoOrbitPairOpsTransitionRates = opsTransitionRatesArray.create otra }

    let createUniform (length: int) (order: int) (rates: uf4MutationRates) =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if rates.order <> order then failwith "Rates order must match specified order"
        let ratesArray = Array.init length (fun _ -> rates)
        uf4MutationRatesArray.create ratesArray

    let createLinearVariation 
            (length: int) 
            (order: int) 
            (startRates: uf4MutationRates) 
            (endRates: uf4MutationRates) : uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        if startRates.twoOrbitPairOpsTransitionRates.Length <> endRates.twoOrbitPairOpsTransitionRates.Length then
            failwith "Start and end rates must have the same twoOrbitPairOpsTransitionRates length"
        let rates =
            Array.init length (fun i ->
                let t = if length = 1 then 0.0 else float i / float (length - 1)
                combine order (lerpOps t) startRates endRates)
        uf4MutationRatesArray.create rates

    let createSinusoidalVariation 
            (length: int) 
            (order: int) 
            (baseRates: uf4MutationRates) 
            (amplitudes: uf4MutationRates) 
            (frequency: float) : uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        if baseRates.twoOrbitPairOpsTransitionRates.Length <> amplitudes.twoOrbitPairOpsTransitionRates.Length then
            failwith "Base and amplitudes must have the same twoOrbitPairOpsTransitionRates length"
        let rates =
            Array.init length (fun i ->
                let t = 
                    if length = 1 then 0.0 
                    else float i / float (length - 1) * 2.0 * Math.PI * frequency
                combine order (sinOps t) baseRates amplitudes)
        uf4MutationRatesArray.create rates

    let createGaussianHotSpot 
            (length: int) 
            (order: int) 
            (baseRates: uf4MutationRates) 
            (hotSpotIndex: int) 
            (hotSpotRates: uf4MutationRates) 
            (sigma: float) : uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        if baseRates.twoOrbitPairOpsTransitionRates.Length <> hotSpotRates.twoOrbitPairOpsTransitionRates.Length then
            failwith "Base and hotspot rates must have the same twoOrbitPairOpsTransitionRates length"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                // weight = 0 -> baseRates, weight = 1 -> hotSpotRates
                combine order (lerpOps weight) baseRates hotSpotRates)
        uf4MutationRatesArray.create rates

    let createStepHotSpot 
            (length: int) 
            (order: int) 
            (baseRates: uf4MutationRates) 
            (hotSpotStart: int) 
            (hotSpotEnd: int) 
            (hotSpotRates: uf4MutationRates) : uf4MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then 
            failwith "Invalid hot spot range"
        if baseRates.twoOrbitPairOpsTransitionRates.Length <> hotSpotRates.twoOrbitPairOpsTransitionRates.Length then
            failwith "Base and hotspot rates must have the same twoOrbitPairOpsTransitionRates length"
        let rates =
            Array.init length (fun i ->
                if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates)
        uf4MutationRatesArray.create rates

    let mutate<'a> 
        (uf4MutationRatesArray: uf4MutationRatesArray) 
        (orthoMutator: 'a -> 'a) 
        (paraMutator: 'a -> 'a) 
        (selfSymMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (twoOrbitType: twoOrbitType) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if uf4MutationRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
    
        Array.init arrayToMutate.Length (fun i ->
            let rate = uf4MutationRatesArray.Item(i)
            match rate.seedOpsTransitionRates.PickMode floatPicker twoOrbitType with
            | opsActionMode.Ortho -> orthoMutator arrayToMutate.[i]
            | opsActionMode.Para -> paraMutator arrayToMutate.[i]
            | opsActionMode.SelfRefl -> selfSymMutator arrayToMutate.[i]
            | opsActionMode.NoAction -> arrayToMutate.[i])

    let createNewItems<'a> 
        (uf4MutationRatesArray: uf4MutationRatesArray)
        (itemChooser: uf4MutationRates -> 'a)
            : 'a[] =
        Array.init uf4MutationRatesArray.Length (fun i ->
            itemChooser (uf4MutationRatesArray.Item(i)))