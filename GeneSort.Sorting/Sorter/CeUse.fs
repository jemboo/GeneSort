
namespace GeneSort.Sorting.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting



type ceUse = 
    private {
        ceIndex :int<ceIndex>
        useCount :int
        ce: ce
    }
    static member create 
                (ceIndex: int<ceIndex>) 
                (useCount: int)
                (ce: ce): ceUse =
        { ceIndex = ceIndex; useCount = useCount; ce = ce }
    member this.CeIndex with get() : int<ceIndex> = this.ceIndex
    member this.UseCount with get() : int = this.useCount
    member this.Ce with get() : ce = this.ce

module CeUse =

    let toString (ceUse: ceUse) : string =
        sprintf "[%d, %d, %s]" (%ceUse.CeIndex) ceUse.UseCount (ceUse.Ce |> Ce.toString)

    let arrayToString (ceUses: ceUse array) : string =
        ceUses
        |> Array.map toString
        |> String.concat "; "

    let ceUseStringToCes
            (ceUseString: string) : ce[] =
    
        // Helper to strip out ALL control characters (like \r, \n, tabs, etc.)
        let cleanString (str: string) =
            str |> Seq.filter (fun c -> not (System.Char.IsControl(c))) 
                |> System.String.Concat

        let ceUseStrings = ceUseString.Split([|";"|], StringSplitOptions.RemoveEmptyEntries)
        let ceUses = 
            ceUseStrings
            |> Array.map (fun s -> 
                // 1. Clean out the invisible control characters first
                let cleaned = cleanString s
                // 2. Then proceed with your brackets and spaces trim
                let trimmed = cleaned.Trim([|'['; ']'; ' '|])
            
                let parts = trimmed.Split([|','; ')';'('|], StringSplitOptions.TrimEntries)
                if parts.Length <> 6 then
                    failwithf "Invalid ceUse string format: %s" s
                
                let ceIndex = (parts.[0].Trim() |> int) |> UMX.tag<ceIndex>
                let useCount = parts.[1].Trim() |> int
                let lv = parts.[3].Trim() |> int
                let hv = parts.[4].Trim() |> int
                let ce = ce.create lv hv
                ceUse.create ceIndex useCount ce
            )
        ceUses |> Array.map (fun cu -> cu.Ce)



    let ceUseStringToSorter 
            (sorterId: Guid<sorterId>)
            (sortingWidth: int<sortingWidth>)
            (ceUseString: string) : sorter =

        let ces = ceUseStringToCes ceUseString
        sorter.create sorterId sortingWidth ces

