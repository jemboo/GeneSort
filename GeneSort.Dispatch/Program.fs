// Bitonic Sort Network Generator
// Generates a sorting network based on Batcher's Bitonic Sort algorithm
// Network size must be a power of 2

module BitonicSort

/// Represents a single comparator in the network
/// (i, j) means compare positions i and j, ensuring arr[i] <= arr[j]
type Comparator = int * int

/// Generate comparators for a bitonic merge
/// Merges a bitonic sequence into a sorted sequence
let rec bitonicMerge (lo: int) (n: int) : Comparator list =
    if n > 1 then
        let m = n / 2
        let comparators = 
            [ for i in lo .. lo + m - 1 -> (i, i + m)]
        let left = bitonicMerge lo m
        let right = bitonicMerge (lo + m) m
        comparators @ left @ right
    else
        []

/// Generate comparators for bitonic sort
/// Recursively builds bitonic sequences and merges them
let rec bitonicSort (lo: int) (n: int) : Comparator list =
    if n > 1 then
        let m = n / 2
        let left = bitonicSort lo m
        let right = bitonicSort (lo + m) m
        let merge = bitonicMerge lo n
        left @ right @ merge
    else
        []

/// Generate a complete bitonic sorting network for n elements
/// n must be a power of 2
let generateBitonicNetwork (n: int) : Comparator list =
    // Verify n is a power of 2
    if n <= 0 || (n &&& (n - 1)) <> 0 then
        failwith "Network size must be a power of 2"
    
    bitonicSort 0 n

/// Count the number of comparators in the network
let countComparators (network: Comparator list) : int =
    List.length network

/// Calculate the depth (number of parallel stages) of the network
let calculateDepth (network: Comparator list) : int =
    let rec findDepth (remaining: Comparator list) (depth: int) (usedPositions: Set<int>) =
        match remaining with
        | [] -> depth
        | _ ->
            // Find all comparators that can execute in parallel (don't share positions)
            let rec findParallel (comps: Comparator list) (current: Comparator list) (used: Set<int>) (rest: Comparator list) =
                match comps with
                | [] -> (current, rest)
                | (i, j) :: tail ->
                    if Set.contains i used || Set.contains j used then
                        // This comparator conflicts, skip it for now
                        findParallel tail current used ((i, j) :: rest)
                    else
                        // This comparator can run in parallel
                        let newUsed = used |> Set.add i |> Set.add j
                        findParallel tail ((i, j) :: current) newUsed rest
            
            let (parallel, rest) = findParallel remaining [] Set.empty []
            findDepth (List.rev rest) (depth + 1) Set.empty
    
    findDepth network 0 Set.empty

/// Format a comparator for display
let formatComparator (i: int, j: int) : string =
    sprintf "(%d, %d)" i j

/// Print the network in a readable format
let printNetwork (network: Comparator list) : unit =
    printfn "Bitonic Sort Network"
    printfn "===================="
    printfn "Total comparators: %d" (countComparators network)
    printfn "Depth: %d" (calculateDepth network)
    printfn "\nComparators (i, j) - ensures arr[i] <= arr[j]:"
    network |> List.iteri (fun idx comp -> 
        printfn "%3d: %s" (idx + 1) (formatComparator comp))

/// Apply the sorting network to an array
let applySortingNetwork (network: Comparator list) (arr: 'a array) : 'a array =
    let result = Array.copy arr
    for (i, j) in network do
        // Comparator (i,j) ensures arr[i] <= arr[j]
        if result.[i] > result.[j] then
            let temp = result.[i]
            result.[i] <- result.[j]
            result.[j] <- temp
    result

/// Test the network with a sample array
let testNetwork (n: int) : unit =
    printfn "\nTesting Bitonic Sort Network for n=%d" n
    printfn "========================================"
    
    let network = generateBitonicNetwork n
    
    // Test with various inputs
    let testCases = [
        Array.init n (fun i -> n - i - 1)  // Reverse order
        Array.init n (fun i -> i)          // Already sorted
    ]
    
    for testCase in testCases do
        let sorted = applySortingNetwork network testCase
        printfn "Input:  %A" testCase
        printfn "Output: %A" sorted
        printfn ""

/// Generate statistics for networks of different sizes
let generateStatistics () : unit =
    printfn "Bitonic Sort Network Statistics"
    printfn "================================"
    printfn "%-6s %-12s %-12s %-12s" "n" "Comparators" "Depth" "Theoretical"
    printfn "%s" (String.replicate 50 "-")
    
    for k in 1..10 do
        let n = 1 <<< k  // 2^k
        let network = generateBitonicNetwork n
        let comparators = countComparators network
        let depth = calculateDepth network
        let theoretical = (k * (k + 1)) / 2  // log²n depth
        printfn "%-6d %-12d %-12d %-12d" n comparators depth theoretical


// Main execution
[<EntryPoint>]
let main argv =
    // Generate and print a network for n=8
    let n = 16
    let network = generateBitonicNetwork n
    printNetwork network
    
    // Test the network
    testNetwork n
    
    // Show statistics
    printfn ""
    generateStatistics ()
    
    
    0 // Return success