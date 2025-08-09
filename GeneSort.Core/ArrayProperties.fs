﻿namespace GeneSort.Core
open LanguagePrimitives

module ArrayProperties =

    let inline distanceSquared< ^a when ^a: (static member Zero: ^a)
                                        and ^a: (static member (+): ^a * ^a -> ^a)
                                        and ^a: (static member (-): ^a * ^a -> ^a)
                                        and ^a: (static member (*): ^a * ^a -> ^a)>
                    (a: ^a[]) (b: ^a[]) : ^a =
        let mutable acc = GenericZero<^a>
        let mutable i = 0
        while i < a.Length do
            acc <- acc + (a.[i] - b.[i]) * (a.[i] - b.[i])
            i <- i + 1
        acc


    let inline distanceSquaredOffset< ^a when ^a: (static member Zero: ^a)
                                                    and ^a: (static member (+): ^a * ^a -> ^a)
                                                    and ^a: (static member (-): ^a * ^a -> ^a)
                                                    and ^a: (static member (*): ^a * ^a -> ^a)>
            (longArray: ^a[]) (shortArray: ^a[]) : ^a[] =
        let n = shortArray.Length
        let m = longArray.Length / n
        let result = Array.zeroCreate m
        for i = 0 to m - 1 do
            let mutable acc = GenericZero<^a>
            for j = 0 to n - 1 do
                let diff = longArray.[i * n + j] - shortArray.[j]
                acc <- acc + diff * diff
            result.[i] <- acc
        result


    let inline isSorted< ^a when ^a: comparison> (values: ^a[]) : bool =
        if isNull values then 
            failwith "Array cannot be null"
        elif values.Length <= 1 then true
        else
            let mutable i = 1
            let mutable isSorted = true
            while (i < values.Length && isSorted) do
                isSorted <- (values.[i - 1] <= values.[i])
                i <- i + 1
            isSorted


    let inline isSortedOffset< ^a when ^a: comparison> 
                    (values: ^a[]) 
                    (offset:int) 
                    (length:int) : bool =
        if isNull values then failwith "Array cannot be null"
        elif offset < 0 then failwithf "Invalid offset: %d" offset
        elif length < 0 then failwithf "Invalid length: %d" length
        elif offset + length > values.Length then 
            failwithf "Offset plus length exceeds array size: offset=%d, length=%d, array size=%d" offset length values.Length
        elif length <= 1 then true
        else
            let mutable i = 1
            let mutable isSorted = true
            while (i < length && isSorted) do
                isSorted <- (values.[i + offset - 1] <= values.[i + offset])
                i <- i + 1
            isSorted