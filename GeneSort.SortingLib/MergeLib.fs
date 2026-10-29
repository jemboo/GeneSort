
namespace GeneSort.SortingLib.Sorter
open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting


type mergeLibId = 
    private 
        { channelCount: int<channelCount>
          mergeDimension: int<mergeDimension>
          variant: sorterLibVariant }

    static member create (channelCount: int<channelCount>)
                         (mergeDimension: int<mergeDimension>)
                         (variant: sorterLibVariant) : mergeLibId =
        if %channelCount % %mergeDimension <> 0 then
                failwith "channelCount must be divisible by mergeDimension."
        { channelCount = channelCount; mergeDimension = mergeDimension; variant = variant }

    member this.ChannelCount with get() = this.channelCount
    member this.MergeDimension with get() = this.mergeDimension
    member this.SorterLibVariant with get() = this.variant
    member this.FactorChannelCount with get() = (%this.channelCount / %this.mergeDimension) |> UMX.tag<channelCount>

    interface IStableSerializable with
        member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
            // 1. Write the underlying integer from the UMX tag
            writer.Write(UMX.untag this.channelCount)
            // 2. Write the mergeDimension as a consistent string or integer tag
            writer.Write(UMX.untag this.mergeDimension)
            // 3. Write the variant as a consistent string or integer tag
            writer.Write(sprintf "%A" this.variant)


module MergeLibId =

    let create (channelCount: int<channelCount>) (mergeDimension: int<mergeDimension>) (variant: sorterLibVariant) : mergeLibId =
        { channelCount = channelCount; mergeDimension = mergeDimension; variant = variant }

    /// Every merge at this width whose component sorting network exists in the library.
    let availableForChannelCount (width: int<channelCount>) =
        SorterLib.ids ()
        |> Array.choose (fun sorter ->
            if int width > 0 && int width % int sorter.ChannelCount = 0 then
                Some (create width (UMX.tag (int width / int sorter.ChannelCount)) sorter.SorterVariant)
            else None)
        |> Array.sortBy (fun id -> id.MergeDimension, id.SorterLibVariant)
    let toString (key: mergeLibId) : string =
        sprintf "MergeLibId(channelCount=%d, mergeDimension=%d, Variant=%s)"
                (UMX.untag key.channelCount) (UMX.untag key.mergeDimension) (SorterLibVariant.toString key.variant)
    let fromString (s: string) : mergeLibId =
        // This is a simple parser; in a real implementation, you might want to use a more robust parsing method
        let parts = s.Trim().Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length <> 3 then
            failwith "Invalid mergeLibId string format"
        else
            let channelCountPart = parts.[0].Trim().Replace("MergeLibId(channelCount=", "").Replace(")", "")
            let mergeDimensionPart = parts.[1].Trim().Replace("mergeDimension=", "").Replace(")", "")
            let variantPart = parts.[2].Trim().Replace("Variant=", "").Replace(")", "")
            let channelCount = Int32.Parse(channelCountPart) |> UMX.tag<channelCount>
            let mergeDimension = Int32.Parse(mergeDimensionPart) |> UMX.tag<mergeDimension>
            create channelCount mergeDimension (SorterLibVariant.fromString variantPart)
