namespace GeneSort.Core

open System

module MathUtils = 

    /// Computes  (toM)^power, where toM is an integer and power is a non-negative integer.
    let integerPower (toM:int) (power:int) =
        pown toM power

    /// Verifies if an integer is an exact power of two and returns its base-2 logarithm.
    /// Throws ArgumentException if n <= 0, or if n is not an exact power of 2.
    let exactLog2 (n: int) : int =
        if n <= 0 then
            invalidArg "n" "Input must be positive"
        // Check if n is a power of two: n & (n - 1) = 0
        if n &&& (n - 1) = 0 then
            // Compute log2 by counting trailing zeros (position of the set bit)
            let rec countTrailingZeros num count =
                if num = 0 then count
                else countTrailingZeros (num >>> 1) (count + 1)

            countTrailingZeros (n >>> 1) 0
        else
            invalidArg "n" "Input must be an exact power of 2"


    /// Determines if a number is a power of two.
    /// <param name="order">The number to check (must be positive).</param>
    /// <returns>True if the number is a power of two, false otherwise.</returns>
    /// <exception cref="System.ArgumentException">Thrown when order is non-positive.</exception>
    let isAPowerOfTwo (order: int) : bool =
        if order <= 0 then invalidArg "order" "Order must be positive"
        order > 0 && (order &&& (order - 1)) = 0

    // Samples [1 .. maxInt] with exponentailly decreasing frequency.
    // 1.0049 -> 1002 samples per 10^4
    // 1.0068 -> 1008 samples per 50K
    // 1.0077 -> 997 samples per 10^5
    // 10095 -> 1001 samples per 500K
    // 1.0103 -> 1000 samples per 10^6
    

    let xSample5C = 1.041
    let xSample1K = 1.05
    let xSample5K = 1.07
    let xSample10K = 1.08
    let xSample50K = 1.1
    let xSample100K = 1.1113
    let xSample500K = 1.13


    let cSample5C = 1.041
    let cSample1K = 1.05
    let cSample5K = 1.07
    let cSample10K = 1.08
    let cSample50K = 1.1
    let cSample100K = 1.1113
    let cSample500K = 1.13

    let kSample5K = 1.004
    let kSample10K = 1.0049
    let ksample50K = 1.0068
    let ksample100K = 1.0077
    let ksample500K = 1.0095
    let ksample1M = 1.0103

    let expSampler (minInt:int) (maxInt:int) (increaseRatio:float) :Set<int> =
        let rec computeTargets currentVal acc =
            let nextVal = currentVal * increaseRatio
            let nextInt = int (ceil nextVal)
            if nextInt >= maxInt then 
                (maxInt :: acc) |> Set.ofList
            else 
                computeTargets nextVal (nextInt :: acc)

        computeTargets minInt [minInt;] 



    let getTimestampString () =
        let now = DateTime.Now
        // MM-dd for month-day
        // HH:mm:ss.f for hours (24h):minutes:seconds.tenths
        now.ToString("MM-dd HH:mm:ss.fff")
