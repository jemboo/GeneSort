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

    let ksample10K = 1.0049
    let ksample50K = 1.0068
    let ksample100K = 1.0077
    let ksample500K = 1.0095
    let ksample1000K = 1.0103

    let expSampler (maxInt:int) (stepIncrease:float)=
        let rec computeTargets currentVal acc =
            let nextVal = currentVal * stepIncrease
            let nextInt = int (ceil nextVal)
            if nextInt >= maxInt then 
                (maxInt :: acc) |> Set.ofList
            else 
                computeTargets nextVal (nextInt :: acc)

        computeTargets 1.0 [1;] 



    let getTimestampString () =
        let now = DateTime.Now
        // MM-dd for month-day
        // HH:mm:ss.f for hours (24h):minutes:seconds.tenths
        now.ToString("MM-dd HH:mm:ss.fff")
