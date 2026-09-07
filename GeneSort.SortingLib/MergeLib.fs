
namespace GeneSort.SortingLib.Sorter
open FSharp.UMX
open GeneSort.Sorting
open System
open GeneSort.Core


type mergeLibVariant =
    | VariantA
    | VariantB
    | VariantC


module MergeLibVariant =

    let toString (variant: mergeLibVariant) : string =
        match variant with
        | VariantA -> "VariantA"
        | VariantB -> "VariantB"
        | VariantC -> "VariantC"

    let fromString (s: string) : mergeLibVariant =
        match s with
        | "VariantA" -> VariantA
        | "VariantB" -> VariantB
        | "VariantC" -> VariantC
        | _ -> failwithf "Unknown mergeLibVariant: %s" s


type mergeLibId = 
    private 
        { sortingWidth: int<sortingWidth>
          variant: mergeLibVariant }

    static member create (sortingWidth: int<sortingWidth>) 
                         (sorterVariant: mergeLibVariant) : mergeLibId =
        { sortingWidth = sortingWidth; variant = sorterVariant }  

    member this.SortingWidth with get() = this.sortingWidth
    member this.MergeLibVariant with get() = this.variant
    
    interface IStableSerializable with
        member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
            // 1. Write the underlying integer from the UMX tag
            writer.Write(UMX.untag this.sortingWidth)
            // 2. Write the variant as a consistent string or integer tag
            writer.Write(sprintf "%A" this.variant)


module MergeLibId =

    let create (sortingWidth: int<sortingWidth>) (variant: mergeLibVariant) : mergeLibId =
        { sortingWidth = sortingWidth; variant = variant }
    let toString (key: mergeLibId) : string =
        sprintf "MergeLibId(sortingWidth=%d, Variant=%s)" 
                (UMX.untag key.sortingWidth) (MergeLibVariant.toString key.variant)
    let fromString (s: string) : mergeLibId =
        // This is a simple parser; in a real implementation, you might want to use a more robust parsing method
        let parts = s.Trim().Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length <> 2 then
            failwith "Invalid sorterLibId string format"
        else
            let sortingWidthPart = parts.[0].Trim().Replace("SorterLibId(sortingWidth=", "").Replace(")", "")
            let variantPart = parts.[1].Trim().Replace("Variant=", "").Replace(")", "")
            let sortingWidth = Int32.Parse(sortingWidthPart) |> UMX.tag<sortingWidth>
            create sortingWidth (MergeLibVariant.fromString variantPart)



module MergeLib =

    let private AllNetworksList = 
        [
            { mergeLibId.sortingWidth = 2<sortingWidth>; variant = VariantA }, 
            "[(0,1)]"

            { mergeLibId.sortingWidth = 3<sortingWidth>; variant = mergeLibVariant.VariantA }, 
            "[(0,2)]
             [(0,1)]
             [(1,2)]"

        ] |> Map.ofList



    /// Safely attempts to find a network string by its key properties.
    let tryGet (mergeKey: mergeLibId) =
        Map.tryFind mergeKey AllNetworksList