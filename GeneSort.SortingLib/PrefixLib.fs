
namespace GeneSort.SortingLib.Sorter
open FSharp.UMX
open GeneSort.Sorting
open System
open GeneSort.Core


type prefixLibVariant =
    | PrefixA
    | PrefixB
    | PrefixC


module PrefixLibVariant =

    let toString (variant: prefixLibVariant) : string =
        match variant with
        | PrefixA -> "PrefixA"
        | PrefixB -> "PrefixB"
        | PrefixC -> "PrefixC"

    let fromString (s: string) : prefixLibVariant =
        match s with
        | "PrefixA" -> PrefixA
        | "PrefixB" -> PrefixB
        | "PrefixC" -> PrefixC
        | _ -> failwithf "Unknown prefixLibVariant: %s" s


type prefixLibId = 
    private 
        { sortingWidth: int<sortingWidth>
          stageLength: int<stageLength>
          variant: prefixLibVariant }

    static member create (sortingWidth: int<sortingWidth>) 
                         (stageLength: int<stageLength>)
                         (variant: prefixLibVariant) : prefixLibId =
        { sortingWidth = sortingWidth; stageLength = stageLength; variant = variant }  

    member this.SortingWidth with get() = this.sortingWidth
    member this.StageLength with get() = this.stageLength
    member this.PrefixLibVariant with get() = this.variant
    
    interface IStableSerializable with
        member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
            // 1. Write the underlying integer from the UMX tag
            writer.Write(UMX.untag this.sortingWidth)
            // 2. Write the stage length as an integer
            writer.Write(UMX.untag this.stageLength)
            // 3. Write the variant as a consistent string or integer tag
            writer.Write(sprintf "%A" this.variant)


module PrefixLibId =

    let toString (key: prefixLibId) : string =
        sprintf "PrefixLibId(sortingWidth=%d, stageLength=%d, Variant=%s)" 
                (UMX.untag key.sortingWidth) (UMX.untag key.stageLength) (PrefixLibVariant.toString key.variant)
    let fromString (s: string) : prefixLibId =
        // This is a simple parser; in a real implementation, you might want to use a more robust parsing method
        let parts = s.Trim().Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length <> 3 then
            failwith "Invalid prefixLibId string format"
        else
            let sortingWidthPart = parts.[0].Trim().Replace("PrefixLibId(sortingWidth=", "").Replace(")", "")
            let stageLengthPart = parts.[1].Trim().Replace("stageLength=", "").Replace(")", "")
            let variantPart = parts.[2].Trim().Replace("Variant=", "").Replace(")", "")
            let sortingWidth = Int32.Parse(sortingWidthPart) |> UMX.tag<sortingWidth>
            let stageLength = Int32.Parse(stageLengthPart) |> UMX.tag<stageLength>
            prefixLibId.create sortingWidth stageLength (PrefixLibVariant.fromString variantPart)



module PrefixLib =

    let private AllNetworksList = 
    
        [ 
            prefixLibId.create (8<sortingWidth>) (3<stageLength>) prefixLibVariant.PrefixA,
            "[(0,2),(1,3),(4,6),(5,7)]
             [(0,4),(1,5),(2,6),(3,7)]
             [(0,1),(2,3),(4,5),(6,7)]"

             
            //13,520 test cases remaining
            prefixLibId.create (24<sortingWidth>) (3<stageLength>) prefixLibVariant.PrefixA,
            "[(0,20),(1,12),(2,16),(3,23),(4,6),(5,10),(7,21),(8,14),(9,15),(11,22),(13,18),(17,19)]
             [(0,3),(1,11),(2,7),(4,17),(5,13),(6,19),(8,9),(10,18),(12,22),(14,15),(16,21),(20,23)]
             [(0,1),(2,4),(3,12),(5,8),(6,9),(7,10),(11,20),(13,16),(14,17),(15,18),(19,21),(22,23)]"


            //8000 test cases remaining
            prefixLibId.create (24<sortingWidth>) (3<stageLength>) prefixLibVariant.PrefixB,
            "[(0,1),(2,3),(4,5),(6,7),(8,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23)]
             [(0,2),(1,3),(4,6),(5,7),(8,10),(9,11),(12,14),(13,15),(16,18),(17,19),(20,22),(21,23)]
             [(0,4),(1,5),(2,6),(3,7),(8,12),(9,13),(10,14),(11,15),(16,20),(17,21),(18,22),(19,23)]"


            //5340 test cases remaining : base = 4 Stages, 44 Ces
            prefixLibId.create (24<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA,
            "[(0,20),(1,12),(2,16),(3,23),(4,6),(5,10),(7,21),(8,14),(9,15),(11,22),(13,18),(17,19)]
             [(0,3),(1,11),(2,7),(4,17),(5,13),(6,19),(8,9),(10,18),(12,22),(14,15),(16,21),(20,23)]
             [(0,1),(2,4),(3,12),(5,8),(6,9),(7,10),(11,20),(13,16),(14,17),(15,18),(19,21),(22,23)]
             [(2,5),(4,8),(6,11),(7,14),(9,16),(12,17),(15,19),(18,21)]"


            //2520 test cases remaining : base = 4 Stages, 46 Ces
            prefixLibId.create (24<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixB,
            "[(0,1),(2,3),(4,5),(6,7),(8,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23)]
             [(0,2),(1,3),(4,6),(5,7),(8,10),(9,11),(12,14),(13,15),(16,18),(17,19),(20,22),(21,23)]
             [(0,4),(1,5),(2,6),(3,7),(8,12),(9,13),(10,14),(11,15),(16,20),(17,21),(18,22),(19,23)]
             [(0,16),(1,18),(2,17),(3,19),(4,20),(5,22),(6,21),(7,23),(9,10),(13,14)]"


            prefixLibId.create (28<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA,
            "[(0,9),(1,20),(2,21),(3,22),(4,19),(5,24),(6,25),(7,26),(8,23),(10,15),(11,13),(12,17),(14,16),(18,27)]
             [(0,18),(1,7),(2,6),(3,5),(4,8),(9,27),(10,12),(11,14),(13,16),(15,17),(19,23),(20,26),(21,25),(22,24)]
             [(1,2),(3,4),(5,19),(6,20),(7,21),(8,22),(9,18),(10,11),(12,14),(13,15),(16,17),(23,24),(25,26)]
             [(0,3),(1,10),(5,8),(6,7),(11,13),(14,16),(17,26),(19,22),(20,21),(24,27)]"


            prefixLibId.create (32<sortingWidth>) (4<stageLength>) prefixLibVariant.PrefixA,
            "[(0,1),(2,3),(4,5),(6,7),(8,9),(10,11),(12,13),(14,15),(16,17),(18,19),(20,21),(22,23),(24,25),(26,27),(28,29),(30,31)]
             [(0,2),(1,3),(4,6),(5,7),(8,10),(9,11),(12,14),(13,15),(16,18),(17,19),(20,22),(21,23),(24,26),(25,27),(28,30),(29,31)]
             [(0,4),(1,5),(2,6),(3,7),(8,12),(9,13),(10,14),(11,15),(16,20),(17,21),(18,22),(19,23),(24,28),(25,29),(26,30),(27,31)]
             [(0,8),(1,9),(2,10),(3,11),(4,12),(5,13),(6,14),(7,15),(16,24),(17,25),(18,26),(19,27),(20,28),(21,29),(22,30),(23,31)]"


        ] |> Map.ofList



    /// Safely attempts to find a network string by its key properties.
    let tryGet (prefixKey: prefixLibId) =
        Map.tryFind prefixKey AllNetworksList