namespace GeneSort.Core

open SysExt

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