namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open System.Runtime.Intrinsics

[<Struct; CustomEquality; NoComparison>]
type msasM = 
    private 
        { id: Guid<sorterTestModelID>
          mergeDimension: int<mergeDimension>
          mergeFillType: mergeSuffixType
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            (mergeDimension: int<mergeDimension>)
            (mergeFillType: mergeSuffixType)
            : msasM =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        if (%sortingWidth) % (%mergeDimension) <> 0 then
            failwith "mergeDimension must evenly divide sortingWidth"
        else
            let id = 
                [
                    "MsasMi" :> obj
                    mergeDimension :> obj
                    mergeFillType :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { 
                id = id; 
                mergeDimension = mergeDimension
                mergeFillType = mergeFillType
                sortingWidth = sortingWidth
            }

    member this.Id with get() = this.id

    member this.MergeDimension with get() = this.mergeDimension
    member this.MergeFillType with get() = this.mergeFillType

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msasM as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<msasM> with
        member this.Equals(other) =  this.sortingWidth = other.sortingWidth

    member this.MakeSortableIntTest : sortableIntTest =
        let intArrays = SortableIntArray.getMergeTestCases 
                            this.SortingWidth
                            this.MergeDimension
                            this.mergeFillType

        sortableIntTest.create 
                (%this.id |> UMX.tag<sorterTestId>) 
                this.SortingWidth
                intArrays


    member this.MakeSortableBoolTest : sortableBinaryTest =

        let sortableArrays = SortableBoolArray.getMergeTestCases
                                    this.sortingWidth
                                    this.mergeDimension
                                    this.mergeFillType  

        sortableBinaryTest.create 
                ( %this.id |> UMX.tag<sorterTestId>) 
                this.sortingWidth
                sortableArrays


    member this.MakeSortableUint8v256Test : sortableUint8v256Test =
        let intArrays = SortableIntArray.getMergeTestCases 
                            this.SortingWidth
                            this.MergeDimension
                            this.mergeFillType

        SortableUint8v256Test.fromIntArrays 
                (%this.id |> UMX.tag<sorterTestId>) 
                this.SortingWidth
                intArrays


    member this.MakeSortableUint8v512Test : sortableUint8v512Test =
        let intArrays = SortableIntArray.getMergeTestCases 
                            this.SortingWidth
                            this.MergeDimension
                            this.mergeFillType

        SortableUint8v512Test.fromIntArrays 
                (%this.id |> UMX.tag<sorterTestId>) 
                this.SortingWidth
                intArrays


    member this.MakeSortableBitv512Test : sortableBitv512Test =
        let intArrays = SortableIntArray.getMergeTestCases 
                            this.SortingWidth
                            this.MergeDimension
                            this.mergeFillType

        let sw = this.SortingWidth
        let id = %this.id |> UMX.tag<sorterTestId>
    
        // Instead of converting all to bool arrays first, we process intArrays in chunks
        let blocks = 
            intArrays 
            |> Array.chunkBySize 512 
            |> Array.collect (fun (chunk: sortableIntArray[]) ->
                let inputCount = chunk.Length
                let numThresholds = %sw + 1
            
                // We are building a sequence of blocks. 
                // Since one chunk of 512 intArrays actually represents 512 * (width+1) bool cases,
                // we yield multiple simdSortBlocks.
                [| 
                    for threshold = 0 to %sw do
                        let vecs = Array.init %sw (fun wireIdx ->
                            let buffer = Array.zeroCreate<uint64> 8
                            for testIdx = 0 to inputCount - 1 do
                                // Logic: thresholding the int value directly into the bit buffer
                                if chunk.[testIdx].Values.[wireIdx] >= threshold then
                                    let lane = testIdx / 64
                                    let bit = testIdx % 64
                                    buffer.[lane] <- buffer.[lane] ||| (1uL <<< bit)
                        
                            Vector512.Create(
                                buffer.[0], buffer.[1], buffer.[2], buffer.[3],
                                buffer.[4], buffer.[5], buffer.[6], buffer.[7]
                            )
                        )
                        sortBlockBitv512.createFromVectors vecs inputCount
                |]
            )

        sortableBitv512Test.create id sw blocks







module MsasMi = ()
 
 