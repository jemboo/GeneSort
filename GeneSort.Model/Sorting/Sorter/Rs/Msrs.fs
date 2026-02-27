namespace GeneSort.Model.Sorting.Sorter.Rs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting

/// Represents a rank-swap-based sorting model composed of an array of Perm_Rs instances.
[<Struct; CustomEquality; NoComparison>]
type msrs = 
    private 
        { id: Guid<sorterModelId>
          sortingWidth: int<sortingWidth>
          perm_Rss: Perm_Rs array } 
    with
    /// Creates an Msrs instance with the specified ID, width, and Perm_Rs array.
    /// Throws an exception if the Perm_Rs array is empty, width is less than 1, or any Perm_Rs has a mismatched width.
    static member create 
            (id: Guid<sorterModelId>) 
            (width: int<sortingWidth>) 
            (perm_Rss: Perm_Rs array) : msrs =

        if perm_Rss.Length < 1 then
            failwith $"Must have at least 1 Perm_Rs, got %d{perm_Rss.Length}"
        else if %width < 1 then
            failwith $"Width must be at least 1, got {%width}"
        else if perm_Rss |> Array.exists (fun prs -> %prs.Order <> %width) then
            failwith $"All Perm_Rs must have width {%width}"
        else
            { id = id; sortingWidth = width; perm_Rss = perm_Rss }

    member this.Id with get () = this.id
    member this.CeLength with get () = (this.StageLength * %this.SortingWidth / 2) |> UMX.tag<ceLength>
    member this.StageLength with get () = this.perm_Rss.Length |> UMX.tag<stageLength>
    member this.SortingWidth with get () = this.sortingWidth
    member this.Perm_Rss with get () = this.perm_Rss
    member this.toString() =
        sprintf "msrs(Id=%A, Width=%d, Perm_Rs_count=%d)" 
                (%this.Id) 
                (%this.SortingWidth) 
                (this.Perm_Rss.Length)

    override this.Equals(obj) = 
        match obj with
        | :? msrs as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.id)

    interface IEquatable<msrs> with
        member this.Equals(other) = 
            this.id = other.id

    member this.MakeSorter() = 
        let ces = 
            this.Perm_Rss
            |> Array.collect (fun prs -> 
                prs.Perm_Si 
                |> Perm_Si.getTwoOrbits
                |> Array.map (fun tbit -> ce.create tbit.First tbit.Second))
        sorter.create (%this.Id |> UMX.tag<sorterId>) this.SortingWidth ces


module Msrs =

    /// Returns a string representation of the Msrs instance.
    let toString (msrs: msrs) : string =
        sprintf "Msrs(Id=%A, Width=%d, Perm_Rs_count=%d)" 
                (%msrs.Id) 
                (%msrs.SortingWidth) 
                msrs.Perm_Rss.Length 

    /// Returns the number of Perm_Rs instances in the Msrs.
    let getLength (msrs: msrs) : int =
        msrs.Perm_Rss.Length

    /// Converts an Msrs instance to a Sorter by mapping each Perm_Rs to comparison elements (Ce) via TwoOrbits.
    /// <param name="msrs">The Msrs instance to convert.</param>
    /// <returns>A Sorter instance with the same width and derived comparison elements.</returns>
    let makeSorter (msrs: msrs) : sorter =
        let ces = 
            msrs.Perm_Rss
            |> Array.collect (fun prs -> 
                prs.Perm_Si 
                |> Perm_Si.getTwoOrbits
                |> Array.map (fun tbit -> ce.create tbit.First tbit.Second))
        sorter.create (%msrs.Id |> UMX.tag<sorterId>) msrs.SortingWidth ces