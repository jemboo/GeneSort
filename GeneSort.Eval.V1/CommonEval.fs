namespace GeneSort.Eval.V1

open FSharp.UMX

[<Measure>] type distinctSorterHashes
[<Measure>] type generationNumber
[<Measure>] type sorterPoolMemberId
[<Measure>] type sorterPoolSetId
[<Measure>] type sorterPoolName
[<Measure>] type sorterPoolId
[<Measure>] type sorterCountPerPool
[<Measure>] type sortedFraction
[<Measure>] type sorterPoolCount
[<Measure>] type sorterChildCount

module GenerationNumber =
    let toString (w: int<generationNumber> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

    let standardIntervals() : int<generationNumber> seq =
        seq {
            yield 25<generationNumber>
            yield 50<generationNumber>
            yield 100<generationNumber>
            yield 200<generationNumber>
            yield 500<generationNumber>
            yield 1000<generationNumber>
            yield 2000<generationNumber>
            yield 3000<generationNumber>
            yield 4000<generationNumber>
            yield 5000<generationNumber>
            yield 10000<generationNumber>
            yield 20000<generationNumber>
            yield 30000<generationNumber>
            yield 40000<generationNumber>
            yield 50000<generationNumber>
            yield 60000<generationNumber>
            yield 70000<generationNumber>
            yield 80000<generationNumber>
            yield 90000<generationNumber>
            yield 100000<generationNumber>
            yield 200000<generationNumber>
            yield 300000<generationNumber>
            yield 400000<generationNumber>
            yield 500000<generationNumber>
        }

    let getNextGenerationNumber (current: int<generationNumber>) : int<generationNumber> =
        match current with
        | n when n < 25<generationNumber> -> 25<generationNumber>
        | n when n < 50<generationNumber> -> 50<generationNumber>
        | n when n < 100<generationNumber> -> 100<generationNumber>
        | n when n < 200<generationNumber> -> 200<generationNumber>
        | n when n < 500<generationNumber> -> 500<generationNumber>
        | n when n < 1000<generationNumber> -> 1000<generationNumber>
        | n when n < 2000<generationNumber> -> 2000<generationNumber>
        | n when n < 3000<generationNumber> -> 3000<generationNumber>
        | n when n < 4000<generationNumber> -> 4000<generationNumber>
        | n when n < 5000<generationNumber> -> 5000<generationNumber>
        | n when n < 10000<generationNumber> -> 10000<generationNumber>
        | n when n < 20000<generationNumber> -> 20000<generationNumber>
        | n when n < 30000<generationNumber> -> 30000<generationNumber>
        | n when n < 40000<generationNumber> -> 40000<generationNumber>
        | n when n < 50000<generationNumber> -> 50000<generationNumber>
        | n when n < 60000<generationNumber> -> 60000<generationNumber>
        | n when n < 70000<generationNumber> -> 70000<generationNumber>
        | n when n < 80000<generationNumber> -> 80000<generationNumber>
        | n when n < 90000<generationNumber> -> 90000<generationNumber>
        | n when n < 100000<generationNumber> -> 100000<generationNumber>
        | n when n < 200000<generationNumber> -> 200000<generationNumber>
        | n when n < 300000<generationNumber> -> 300000<generationNumber>
        | n when n < 400000<generationNumber> -> 400000<generationNumber>
        | n when n < 500000<generationNumber> -> 500000<generationNumber>
        | _ -> failwith "No next generation number available"

    // gets the largest generaionNumber item the finder can find,
    // or the default value if none is found.
    let getNextGenerationalItem<'T> 
            (finder: int<generationNumber> -> 'T option)
            (defaultValue: 'T option) : 'T option =

        // Convert to array and reverse so we start at the largest interval
        let reverseIntervals = standardIntervals() |> Seq.toArray |> Array.rev

        // Loops through the intervals. Returns the first 'Some' it encounters,
        // or 'None' if it reaches the end.
        let rec findHighestExisting index =
            if index >= reverseIntervals.Length then
                None
            else
                let currentGen = reverseIntervals.[index]
                match finder currentGen with
                | Some value -> Some value // Found it! Stop immediately.
                | None -> findHighestExisting (index + 1) // Cheap miss, keep going back.

        match findHighestExisting 0 with
        | Some value -> Some value
        | None -> defaultValue


module SorterCountPerPool =
    let toString (w: int<sorterCountPerPool> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterPoolCount =
    let toString (w: int<sorterPoolCount> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module SorterChildCount =
    let toString (w: int<sorterChildCount> option) : string =
       match w with
        | Some v -> sprintf "%d" %v
        | None -> "None"

module Common = ()

