namespace GeneSort.Core
open LanguagePrimitives
open System
open System.Collections
open System.Runtime.CompilerServices
open System.Collections.Generic

module Map =
    /// Merges a sequence of maps into a single map.
    /// Throws an exception if any key appears in more than one map.
    let mergeUnique (maps: Map<'Key, 'Value> seq) : Map<'Key, 'Value> =
        let mutable result = Map.empty
        let mutable duplicateKeys = []
        
        for map in maps do
            for kvp in map do
                if result.ContainsKey kvp.Key then
                    duplicateKeys <- kvp.Key :: duplicateKeys
                else
                    result <- result.Add(kvp.Key, kvp.Value)
        
        if not (List.isEmpty duplicateKeys) then
            failwith (sprintf "Duplicate keys found during map merge: %A" (List.distinct duplicateKeys))
        
        result

module CollectionUtils =

    /// Stores a sequence of values in a dictionary using a key generator function.
    /// If a dictionary is provided, updates it; otherwise, creates a new one.
    /// Throws an exception if a generated key already exists and allowOverwrite is false.
    let toDictionary 
            (allowOverwrite: bool) 
            (dict: Dictionary<'K, 'T> option) 
            (keyGen: 'T -> 'K) 
            (source: seq<'T>) : Dictionary<'K, 'T> =
        let resultDict = match dict with
                         | Some d -> d
                         | None -> Dictionary<'K, 'T>()
        for item in source do
            let key = keyGen item
            if not allowOverwrite && resultDict.ContainsKey key then
                failwithf "Duplicate key '%O' generated for item '%O'" key item
            resultDict.[key] <- item
        resultDict

    let rec compareAny 
                (o1: obj) 
                (o2: obj) 
        =
        match (o1, o2) with
        | (:? IComparable as o1), (:? IComparable as o2) -> Some(compare o1 o2)
        | (:? IEnumerable as arr1), (:? IEnumerable as arr2) ->
            Seq.zip (arr1 |> Seq.cast) (arr2 |> Seq.cast)
            |> Seq.choose (fun (a, b) -> compareAny a b)
            |> Seq.skipWhile ((=) 0)
            |> Seq.tryHead
            |> Option.defaultValue 0
            |> Some
        | (:? ITuple as tup1), (:? ITuple as tup2) ->
            let tupleToSeq (tuple: ITuple) =
                seq {
                    for i in 0 .. tuple.Length do
                        yield tuple.[i]
                }

            compareAny (tupleToSeq tup1) (tupleToSeq tup2)
        | _ -> None

    let areEqual (o1: obj) (o2: obj) =
        match compareAny o1 o2 with
        | Some v -> v = 0
        | None -> false


    // orderItems orders two items of a type that supports comparison.
    let inline orderItems< ^T when ^T : comparison> (x: ^T) (y: ^T) : ^T * ^T =
        if x <= y then (x, y) else (y, x)

    // Generic array equality check
    let inline arrayEquals< ^a when ^a: equality> (xs: ^a[]) (ys: ^a[]) : bool =
        if xs.Length <> ys.Length then false
        else
            let mutable i = 0
            let mutable equal = true
            while i < xs.Length && equal do
                equal <- xs.[i] = ys.[i]
                i <- i + 1
            equal


    //returns the last n items of the list in the original order
    let rec last n xs =
        if List.length xs <= n then xs else last n xs.Tail

    //returns the first n items of the list in the original order,
    //or all the items if it's shorter than n
    let first n (xs: 'a list) =
        let mn = min n xs.Length
        xs |> List.take mn

    let takeUpToOrWhile (n: int) (predicate: 'a -> bool) (source: seq<'a>) : seq<'a> =
        if n < 0 then
            invalidArg "n" "Number of items must be non-negative"
        Seq.takeWhile (fun x -> predicate x) (Seq.truncate n source)


    let flattenTupleSeq (tuples: seq<'T * 'T>) : seq<'T> =
        tuples |> Seq.collect (fun (x, y) -> [x; y])

    // filterByIndices filters a sequence by the specified indices.
    let filterByIndices (indices: int[]) (source: seq<'T>) : seq<'T> =
        source 
        |> Seq.indexed 
        |> Seq.filter (fun (i, _) -> Array.contains i indices)
        |> Seq.map snd

    // pairsWithNext can zip two sequences of unequal length
    let pairWithNext (source: seq<'a>) : seq<'a * ('a option)> =
        seq {
            let enumerator = source.GetEnumerator()
            while enumerator.MoveNext() do
                let current = enumerator.Current
                if enumerator.MoveNext() then
                    yield (current, Some enumerator.Current)
                else
                    yield (current, None)
        }

    // returns a sequence of items that occur more than once
    let itemsOccuringMoreThanOnce items =
        seq {
            let d = System.Collections.Generic.Dictionary()
            for i in items do
                match d.TryGetValue(i) with
                | false, _ -> d.[i] <- false // first observance
                | true, false ->
                    d.[i] <- true
                    yield i // second observance
                | true, true -> () // already seen at least twice
        }

    // filters the sequence, blocking the emission of a given value more than max times.
    let getItemsUpToMaxTimes<'k,'v when 'k:equality> 
            (lookup: 'v->'k)
            (max:int) 
            (items:'v seq)  =
        seq {
            let d = System.Collections.Generic.Dictionary()
            for i in items do
                let key = lookup i
                match d.TryGetValue(key) with
                | false, _ ->
                    d.[key] <- 1
                    yield i
                | true, ct ->
                    d.[key] <- ct + 1
                    if (ct < max) then
                        yield i
        }

    // chunks a sequence by powers of two, starting with a given multiple.
    let chunkByPowersOfTwo (startingMultiple:int) 
                           (source: seq<'T>) : seq<seq<'T>> =
        let rec chunk (src: 'T list) power : seq<seq<'T>> =
            if src.IsEmpty then
                Seq.empty
            else
                let chunkSize = (1 <<< power) * startingMultiple
                let fp, sp =
                    if src.Length <= chunkSize then
                        src, []
                    else
                        List.splitAt chunkSize src
                seq {
                    yield fp |> Seq.ofList
                    yield! (chunk sp (power + 1))
                }
    
        chunk (source |> Seq.toList) 0


    // converts a density distr to a cumulative distr.
    let asCumulative (startVal: float) (weights: float[]) =
        weights |> Array.scan (fun cum cur -> cum + cur) startVal


    // a sequence of int*int where snd - fst = offset, and snd < order
    let steppedOffsetPairs start order offset =
        // Validate inputs: n must be positive, k must be positive and less than n
        if order <= 0 || offset <= 0 || offset >= order then
            Seq.empty
        else
            seq {
                let mutable kk = 0
                let mutable i = start
                while i < order - offset do
                    yield (i, i + offset)
                    kk <- kk + 1
                    i <- i + 1
                    if (kk = offset) then
                        kk <- 0
                        i <- i + offset
            }
    // circularzes and rotates a list so that the minimum element comes first.
    let rotateToMin (xs: int list) : int list =
        match xs with
        | [] -> []
        | _ ->
            let minIndex = xs |> List.indexed |> List.minBy snd |> fst
            let beforeMin, afterMin = xs |> List.splitAt minIndex
            afterMin @ beforeMin