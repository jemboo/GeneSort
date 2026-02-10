
namespace GeneSort.Core

open System

[<Struct; CustomEquality; NoComparison>]
type indelRatesArray =

    private { Rates: indelRates array }
    static member create (rates: indelRates array) : indelRatesArray =
        if Array.isEmpty rates then failwith "Rates array cannot be empty"
        { Rates = rates }
    member this.Length = this.Rates.Length
    member this.Item(index: int) = this.Rates.[index]
    member this.RatesArray = this.Rates
    member this.toString() =
        String.Join(", ", Array.map (fun r -> r.ToString()) this.Rates)

    override this.Equals(obj) =
        match obj with
        | :? indelRatesArray as other ->
            if this.Rates.Length <> other.Rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.Rates other.Rates
        | _ -> false

    override this.GetHashCode() =
        let mutable hash = 17
        for rate in this.Rates do
            hash <- hash * 23 + rate.GetHashCode()
        hash

    interface IEquatable<indelRatesArray> with
        member this.Equals(other) =
            if this.Rates.Length <> other.Rates.Length then false
            else
                Array.forall2 (fun a b -> a.Equals(b)) this.Rates other.Rates


module IndelRatesArray =

    let private clamp (value: float) (min: float) (max: float) =
        Math.Max(min, Math.Min(max, value))

    // Smooth variation: Linear interpolation from startRates to endRates
    let createLinearVariation (length: int) (startRates: indelRates) (endRates: indelRates) : indelRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1)
                let m = startRates.MutationRate + t * (endRates.MutationRate - startRates.MutationRate)
                let i = startRates.InsertionRate + t * (endRates.InsertionRate - startRates.InsertionRate)
                let d = startRates.DeletionRate + t * (endRates.DeletionRate - startRates.DeletionRate)
                indelRates.create (m, i, d))
        indelRatesArray.create rates

    // Smooth variation: Sinusoidal variation around base rates
    let createSinusoidalVariation (length: int) (baseRates: indelRates) (amplitudes: indelRates) (frequency: float) : indelRatesArray =
        if length <= 0 then failwith "Length must be positive"
        let rates =
            Array.init length (fun i ->
                let t = float i / float (length - 1) * 2.0 * Math.PI * frequency
                let m = clamp (baseRates.MutationRate + amplitudes.MutationRate * Math.Sin(t)) 0.0 1.0
                let i = clamp (baseRates.InsertionRate + amplitudes.InsertionRate * Math.Sin(t + 2.0 * Math.PI / 3.0)) 0.0 1.0
                let d = clamp (baseRates.DeletionRate + amplitudes.DeletionRate * Math.Sin(t + 4.0 * Math.PI / 3.0)) 0.0 1.0
                indelRates.create (m, i, d))
        indelRatesArray.create rates

    // Hot spot: Gaussian peak at specified index
    let createGaussianHotSpot (length: int) (baseRates: indelRates) (hotSpotIndex: int) (hotSpotRates: indelRates) (sigma: float) : indelRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotIndex < 0 || hotSpotIndex >= length then failwith "HotSpotIndex out of range"
        if sigma <= 0.0 then failwith "Sigma must be positive"
        let rates =
            Array.init length (fun i ->
                let x = float (i - hotSpotIndex)
                let weight = Math.Exp(-x * x / (2.0 * sigma * sigma))
                let m = baseRates.MutationRate + (hotSpotRates.MutationRate - baseRates.MutationRate) * weight
                let i = baseRates.InsertionRate + (hotSpotRates.InsertionRate - baseRates.InsertionRate) * weight
                let d = baseRates.DeletionRate + (hotSpotRates.DeletionRate - baseRates.DeletionRate) * weight
                indelRates.create (m, i, d))
        indelRatesArray.create rates

    // Hot spot: Step function creating a region of elevated rates
    let createStepHotSpot (length: int) (baseRates: indelRates) (hotSpotStart: int) (hotSpotEnd: int) (hotSpotRates: indelRates) : indelRatesArray =
        if length <= 0 then failwith "Length must be positive"
        if hotSpotStart < 0 || hotSpotStart >= length || hotSpotEnd < hotSpotStart || hotSpotEnd >= length then failwith "Invalid hot spot range"
        let rates =
            Array.init length (fun i ->
                let rates = if i >= hotSpotStart && i <= hotSpotEnd then hotSpotRates else baseRates
                indelRates.create (rates.MutationRate, rates.InsertionRate, rates.DeletionRate))
        indelRatesArray.create rates

    /// Mutates an arry based on the provided rates. Returns a new array, with the same length as the input array.
    /// If Deletion is done d times, and Insertion is done i times, and p = d - i > 0, then Insertion is called p times at the end so the 
    /// returned array has the same length as the input array.
    let mutate<'a> 
        (indelRatesArray: indelRatesArray) 
        (inserter: unit -> 'a) 
        (mutator: 'a -> 'a) 
        (floatPicker: unit -> float) 
        (arrayToMutate: 'a[]) : 'a[] = 
        if indelRatesArray.Length <> arrayToMutate.Length then
            failwith "Array length does not match rates length"
    
        let mutable deletionCount = 0
        let mutable insertionCount = 0
    
        let results = 
            seq {
                for i in 0 .. arrayToMutate.Length - 1 do
                    let rate = indelRatesArray.Item(i)
                    match rate.PickMode floatPicker with
                    | IndelMode.Mutation -> 
                        yield mutator arrayToMutate.[i]
                    | IndelMode.Insertion -> 
                        insertionCount <- insertionCount + 1
                        yield inserter ()
                        yield arrayToMutate.[i]
                    | IndelMode.Deletion -> 
                        deletionCount <- deletionCount + 1
                    | IndelMode.NoAction -> 
                        yield arrayToMutate.[i]
            } |> Seq.toArray
    
        let p = deletionCount - insertionCount
        if p > 0 then
            // Append p insertions to match input length
            Array.append results (Array.init p (fun _ -> inserter ()))
        elif p < 0 then
            // Trim excess elements to match input length
            Array.take arrayToMutate.Length results
        else
            results