namespace GeneSort.Core

open System
open MathUtils

[<Struct; CustomEquality; NoComparison>]
type uf6MutationRatesArray =
    private 
        { 
            uf6MutationRates: uf6MutationRates array 
            cachedHash: int
        }

    static member create (rates: uf6MutationRates array) : uf6MutationRatesArray =
        // Domain Validation
        if Array.exists (fun (r: uf6MutationRates) -> r.order < 6 || r.order % 2 <> 0) rates then
            failwith "All Uf6MutationRates orders must be at least 6 and even"
        
        // Calculate hash once at construction
        let mutable h = 17
        for i = 0 to rates.Length - 1 do
            let r = rates.[i]
            // These child .GetHashCode() calls are O(1) if they follow the caching pattern
            h <- h * 23 + r.order.GetHashCode()
            h <- h * 23 + r.seed6TransitionRates.GetHashCode()
            h <- h * 23 + r.opsTransitionRates.GetHashCode()
            
        { uf6MutationRates = rates; cachedHash = h }

    member this.Length = this.uf6MutationRates.Length
    member this.Item(index: int) = this.uf6MutationRates.[index]
    member this.RatesArray = this.uf6MutationRates

    member this.toString() =
        String.Join(", ", Array.map (
            fun (r: uf6MutationRates) -> 
                sprintf "Uf6MutationRates(order=%d, seed=%s, opsTransitionRates=%s)" 
                        r.order 
                        (r.seed6TransitionRates.toString()) 
                        (r.opsTransitionRates.toString())) this.uf6MutationRates)

    override this.GetHashCode() = this.cachedHash

    override this.Equals(obj) =
        match obj with
        | :? uf6MutationRatesArray as other ->
            // Immediate short-circuit: O(1) in most cases
            if this.cachedHash <> other.cachedHash then false
            elif this.uf6MutationRates.Length <> other.uf6MutationRates.Length then false
            else
                Array.forall2 (fun (a: uf6MutationRates) (b: uf6MutationRates) -> 
                    a.order = b.order && 
                    a.seed6TransitionRates.Equals(b.seed6TransitionRates) && 
                    a.opsTransitionRates.Equals(b.opsTransitionRates)) this.uf6MutationRates other.uf6MutationRates
        | _ -> false

    interface IEquatable<uf6MutationRatesArray> with
        member this.Equals(other) =
            if this.cachedHash <> other.cachedHash then false
            elif this.uf6MutationRates.Length <> other.uf6MutationRates.Length then false
            else
                Array.forall2 (fun (a: uf6MutationRates) (b: uf6MutationRates) -> 
                    a.order = b.order && 
                    a.seed6TransitionRates.Equals(b.seed6TransitionRates) && 
                    a.opsTransitionRates.Equals(b.opsTransitionRates)) this.uf6MutationRates other.uf6MutationRates


module Uf6MutationRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    let private lerp (t: float) (a: float) (b: float) : float =
        a + t * (b - a)

    // ---------- opsActionRates / opsTransitionRates helpers ----------

    let private lerpActionRates (t: float) (a: opsActionRates) (b: opsActionRates) : opsActionRates =
        opsActionRates.create
            (lerp t a.OrthoRate b.OrthoRate)
            (lerp t a.ParaRate b.ParaRate)
            (lerp t a.SelfReflRate b.SelfReflRate)

    let private lerpOps (t: float) (a: opsTransitionRates) (b: opsTransitionRates) : opsTransitionRates =
        opsTransitionRates.create
            (lerpActionRates t a.OrthoRates b.OrthoRates)
            (lerpActionRates t a.ParaRates b.ParaRates)
            (lerpActionRates t a.SelfReflRates b.SelfReflRates)

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

    // ---------- seed6 helpers ----------

    let private lerpSeed6Action (t: float) (a: seed6ActionRates) (b: seed6ActionRates) : seed6ActionRates =
        seed6ActionRates.create
            (lerp t a.Ortho1Rate b.Ortho1Rate)
            (lerp t a.Ortho2Rate b.Ortho2Rate)
            (lerp t a.Para1Rate b.Para1Rate)
            (lerp t a.Para2Rate b.Para2Rate)
            (lerp t a.Para3Rate b.Para3Rate)
            (lerp t a.Para4Rate b.Para4Rate)
            (lerp t a.SelfReflRate b.SelfReflRate)

    let private lerpSeed6 (t: float) (a: seed6TransitionRates) (b: seed6TransitionRates) : seed6TransitionRates =
        seed6TransitionRates.create
            (lerpSeed6Action t a.Ortho1Rates b.Ortho1Rates)
            (lerpSeed6Action t a.Ortho2Rates b.Ortho2Rates)
            (lerpSeed6Action t a.Para1Rates b.Para1Rates)
            (lerpSeed6Action t a.Para2Rates b.Para2Rates)
            (lerpSeed6Action t a.Para3Rates b.Para3Rates)
            (lerpSeed6Action t a.Para4Rates b.Para4Rates)
            (lerpSeed6Action t a.SelfReflRates b.SelfReflRates)

    /// Sinusoidal offset for one seed6ActionRates row. The phase of entry (row, col) is
    /// ((row + col) mod 7) * 2pi/7, matching the original hand-written phase table.
    let private sinSeed6Action (t: float) (row: int) (baseR: seed6ActionRates) (amp: seed6ActionRates) : seed6ActionRates =
        let phaseShift = 2.0 * Math.PI / 7.0
        let v (col: int) (b: float) (a: float) =
            clamp (b + a * Math.Sin(t + float ((row + col) % 7) * phaseShift)) 0.0 1.0
        seed6ActionRates.create
            (v 0 baseR.Ortho1Rate amp.Ortho1Rate)
            (v 1 baseR.Ortho2Rate amp.Ortho2Rate)
            (v 2 baseR.Para1Rate amp.Para1Rate)
            (v 3 baseR.Para2Rate amp.Para2Rate)
            (v 4 baseR.Para3Rate amp.Para3Rate)
            (v 5 baseR.Para4Rate amp.Para4Rate)
            (v 6 baseR.SelfReflRate amp.SelfReflRate)

    let private sinSeed6 (t: float) (baseR: seed6TransitionRates) (amp: seed6TransitionRates) : seed6TransitionRates =
        seed6TransitionRates.create
            (sinSeed6Action t 0 baseR.Ortho1Rates amp.Ortho1Rates)
            (sinSeed6Action t 1 baseR.Ortho2Rates amp.Ortho2Rates)
            (sinSeed6Action t 2 baseR.Para1Rates amp.Para1Rates)
            (sinSeed6Action t 3 baseR.Para2Rates amp.Para2Rates)
            (sinSeed6Action t 4 baseR.Para3Rates amp.Para3Rates)
            (sinSeed6Action t 5 baseR.Para4Rates amp.Para4Rates)
            (sinSeed6Action t 6 baseR.SelfReflRates amp.SelfReflRates)

    // ---------- uf6MutationRates helpers ----------

    let private validateOrder (order: int) =
        if order < 6 || order % 2 <> 0 then failwith "Order must be at least 6 and even"

    /// Combines two uf6MutationRates: fSeed on the seed, fOps on each opsTransitionRates entry.
    /// Where either ops list is too short, a uniform 0.1 entry is used (as in the original).
    let private combine 
            (order: int)
            (fSeed: seed6TransitionRates -> seed6TransitionRates -> seed6TransitionRates)
            (fOps: opsTransitionRates -> opsTransitionRates -> opsTransitionRates)
            (a: uf6MutationRates) 
            (b: uf6MutationRates) : uf6MutationRates =
        let listLength = exactLog2 (order / 6)
        let aList = a.opsTransitionRates.RatesArray
        let bList = b.opsTransitionRates.RatesArray
        let opsArr =
            Array.init listLength (fun j ->
                if j >= aList.Length || j >= bList.Length then opsTransitionRates.createUniformFromFloat(0.1)
                else fOps aList.[j] bList.[j])
        uf6MutationRates.create order 
            (fSeed a.seed6TransitionRates b.seed6TransitionRates) 
            (opsTransitionRatesArray.create opsArr)

    let createUniform (length: int) (order: int) (rates: uf6MutationRates) =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if rates.order <> order then failwith "Rates order must match specified order"
        let ratesArray = Array.init length (fun _ -> rates)
        uf6MutationRatesArray.create ratesArray

    let createLinearVariation 
            (length: int) 
            (order: int) 
            (startRates: uf6MutationRates) 
            (endRates: uf6MutationRates) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if startRates.order <> order || endRates.order <> order then failwith "Start and end rates must have the same order"
        let rates =
            Array.init length (fun i ->
                let t = if length = 1 then 0.0 else float i / float (length - 1)
                combine order (lerpSeed6 t) (lerpOps t) startRates endRates)
        uf6MutationRatesArray.create rates

    let createSinusoidalVariation 
            (length: int) 
            (order: int) 
            (baseRates: uf6MutationRates) 
            (amplitudes: uf6MutationRates) 
            (frequency: float) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if baseRates.order <> order || amplitudes.order <> order then failwith "Base and amplitudes must have the same order"
        let rates =
            Array.init length (fun i ->
                let t = 
                    if length = 1 then 0.0 
                    else float i / float (length - 1) * 2.0 * Math.PI * frequency
                combine order (sinSeed6 t) (sinOps t) baseRates amplitudes)
        uf6MutationRatesArray.create rates

    let createGaussianHotSpot 
            (length: int) 
            (order: int) 
            (baseRates: uf6MutationRates) 
            (hotSpotIndex: int) 
            (hotSpotRates: uf6MutationRates) 
            (sigma: float) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                // weight = 0 -> baseRates, weight = 1 -> hotSpotRates
                combine order (lerpSeed6 weight) (lerpOps weight) baseRates hotSpotRates)
        uf6MutationRatesArray.create rates

    let createStepHotSpot 
            (length: int) 
            (order: int) 
            (baseRates: uf6MutationRates) 
            (hotSpotStart: int) 
            (hotSpotEnd: int) 
            (hotSpotRates: uf6MutationRates) : uf6MutationRatesArray =
        if length <= 0 then failwith "Length must be positive"
        validateOrder order
        if baseRates.order <> order || hotSpotRates.order <> order then failwith "Base and hotspot rates must have the same order"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then 
            failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates)
        uf6MutationRatesArray.create rates

    let mutate<'a> 
        (uf6MutationRatesArray: uf6MutationRatesArray) 
        (ortho1Mutator: 'a -> 'a) 
        (ortho2Mutator: 'a -> 'a) 
        (para1Mutator: 'a -> 'a) 
        (para2Mutator: 'a -> 'a) 
        (para3Mutator: 'a -> 'a) 
        (para4Mutator: 'a -> 'a) 
        (selfReflMutator: 'a -> 'a) 
        (noActionMutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (seed6TwoOrbitType: twoOrbitTripleType) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if uf6MutationRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
        Array.init arrayToMutate.Length (fun i ->
            let rate = uf6MutationRatesArray.Item(i)
            match rate.seed6TransitionRates.PickMode floatPicker seed6TwoOrbitType with
            | seed6ActionMode.Ortho1 -> ortho1Mutator arrayToMutate.[i]
            | seed6ActionMode.Ortho2 -> ortho2Mutator arrayToMutate.[i]
            | seed6ActionMode.Para1 -> para1Mutator arrayToMutate.[i]
            | seed6ActionMode.Para2 -> para2Mutator arrayToMutate.[i]
            | seed6ActionMode.Para3 -> para3Mutator arrayToMutate.[i]
            | seed6ActionMode.Para4 -> para4Mutator arrayToMutate.[i]
            | seed6ActionMode.SelfRefl -> selfReflMutator arrayToMutate.[i]
            | seed6ActionMode.NoAction -> noActionMutator arrayToMutate.[i])

    let createNewItems<'a> 
        (uf6MutationRatesArray: uf6MutationRatesArray)
        (itemChooser: uf6MutationRates -> 'a)
            : 'a[] =
        Array.init uf6MutationRatesArray.Length (fun i ->
            itemChooser (uf6MutationRatesArray.Item(i)))