
namespace GeneSort.SortingLib.Sorter
open FSharp.UMX
open GeneSort.Sorting
open System
open GeneSort.Core


type mergeLibId = 
    private 
        { sortingWidth: int<sortingWidth>
          mergeDimension: int<mergeDimension>
          variant: sorterLibVariant }

    static member create (sortingWidth: int<sortingWidth>) 
                         (mergeDimension: int<mergeDimension>)
                         (variant: sorterLibVariant) : mergeLibId =
        if %sortingWidth % %mergeDimension <> 0 then
                failwith "sortingWidth must be divisible by mergeDimension."
        { sortingWidth = sortingWidth; mergeDimension = mergeDimension; variant = variant }  

    member this.SortingWidth with get() = this.sortingWidth
    member this.MergeDimension with get() = this.mergeDimension
    member this.SorterLibVariant with get() = this.variant
    member this.FactorSortingWidth with get() = (%this.sortingWidth / %this.mergeDimension) |> UMX.tag<sortingWidth>

    interface IStableSerializable with
        member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
            // 1. Write the underlying integer from the UMX tag
            writer.Write(UMX.untag this.sortingWidth)
            // 2. Write the mergeDimension as a consistent string or integer tag
            writer.Write(UMX.untag this.mergeDimension)
            // 3. Write the variant as a consistent string or integer tag
            writer.Write(sprintf "%A" this.variant)


module MergeLibId =

    let create (sortingWidth: int<sortingWidth>) (mergeDimension: int<mergeDimension>) (variant: sorterLibVariant) : mergeLibId =
        { sortingWidth = sortingWidth; mergeDimension = mergeDimension; variant = variant }
    let toString (key: mergeLibId) : string =
        sprintf "MergeLibId(sortingWidth=%d, mergeDimension=%d, Variant=%s)" 
                (UMX.untag key.sortingWidth) (UMX.untag key.mergeDimension) (SorterLibVariant.toString key.variant)
    let fromString (s: string) : mergeLibId =
        // This is a simple parser; in a real implementation, you might want to use a more robust parsing method
        let parts = s.Trim().Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length <> 3 then
            failwith "Invalid mergeLibId string format"
        else
            let sortingWidthPart = parts.[0].Trim().Replace("MergeLibId(sortingWidth=", "").Replace(")", "")
            let mergeDimensionPart = parts.[1].Trim().Replace("mergeDimension=", "").Replace(")", "")
            let variantPart = parts.[2].Trim().Replace("Variant=", "").Replace(")", "")
            let sortingWidth = Int32.Parse(sortingWidthPart) |> UMX.tag<sortingWidth>
            let mergeDimension = Int32.Parse(mergeDimensionPart) |> UMX.tag<mergeDimension>
            create sortingWidth mergeDimension (SorterLibVariant.fromString variantPart)



//module MergeLib =

//    let private AllNetworksList = 
//        [
//            { mergeLibId.sortingWidth = 2<sortingWidth>; variant = VariantA }, 
//            "[(0,1)]"

//            { mergeLibId.sortingWidth = 3<sortingWidth>; variant = mergeLibVariant.VariantA }, 
//            "[(0,2)]
//             [(0,1)]
//             [(1,2)]"

//        ] |> Map.ofList



//    /// Safely attempts to find a network string by its key properties.
//    let tryGet (mergeKey: mergeLibId) =
//        Map.tryFind mergeKey AllNetworksList