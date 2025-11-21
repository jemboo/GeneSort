namespace GeneSort.Core
open LanguagePrimitives
open System
open System.Collections
open System.Runtime.CompilerServices
open System.Collections.Generic

type subSortedArray<'a when 'a : comparison> =
    private { 
        values: 'a[] 
        partitionPoints: int[]
    }
    
    member this.PartitionCount = this.partitionPoints.Length + 1
    
    member this.GetPartition(index: int) : 'a[] =
        if index < 0 || index >= this.PartitionCount then
            invalidArg "index" "Partition index out of range"
        
        let startIdx = if index = 0 then 0 else this.partitionPoints.[index - 1]
        let endIdx = if index = this.partitionPoints.Length then this.values.Length else this.partitionPoints.[index]
        
        this.values.[startIdx .. endIdx - 1]
    
    member this.GetLatticePoint(threshold: 'a) : int[] =
        Array.init this.PartitionCount (fun i ->
            let partition = this.GetPartition(i)
            // Since partition is sorted, we can use binary search for efficiency
            let firstGreaterOrEqual = 
                partition |> Array.tryFindIndex (fun x -> x >= threshold)
            
            match firstGreaterOrEqual with
            | Some idx -> partition.Length - idx
            | None -> 0  // All elements are less than threshold
        )


module SubSortedArray =
    let private isSorted (arr: 'a[]) =
        arr |> Array.pairwise |> Array.forall (fun (a, b) -> a <= b)
    
    let create (values: 'a[]) (partitionPoints: int[]) : subSortedArray<'a> =
        // Sort partition points to ensure they're in order
        let sortedPartitions = Array.sort partitionPoints
        
        // Validate partition points are within bounds
        if sortedPartitions |> Array.exists (fun p -> p < 0 || p >= values.Length) then
            invalidArg "partitionPoints" "Partition points must be within array bounds"
        
        // Define segment boundaries
        let boundaries = Array.append [|0|] (Array.append sortedPartitions [|values.Length|])
        
        // Validate each segment is sorted
        for i in 0 .. boundaries.Length - 2 do
            let startIdx = boundaries.[i]
            let endIdx = boundaries.[i + 1]
            let segment = values.[startIdx .. endIdx - 1]
            
            if not (isSorted segment) then
                invalidArg "values" (sprintf "Segment %d (indices %d to %d) is not sorted" i startIdx (endIdx - 1))
        
        { values = values; partitionPoints = sortedPartitions }
    
    let getValues (arr: subSortedArray<'a>) = arr.values
    let getPartitionPoints (arr: subSortedArray<'a>) = arr.partitionPoints

