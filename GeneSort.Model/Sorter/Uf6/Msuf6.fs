namespace GeneSort.Model.Sorter.Uf6

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Core
open GeneSort.Model.Sorter

/// Represents a collection of TwoOrbitUnfolder4 instances with a consistent sorting width.
[<Struct; CustomEquality; NoComparison>]
type Msuf6 = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          twoOrbitUnfolder6s: TwoOrbitUf6 array } 
    with
    /// Creates an Msuf6 instance with the specified ID, sorting width, and array of TwoOrbitUnfolder4 instances.
    /// Throws an exception if the array is empty, width is less than 1, or any TwoOrbitUnfolder4 has a mismatched order.
    static member create 
            (id: Guid<sorterModelID>) 
            (sortingWidth: int<sortingWidth>) 
            (twoOrbitUnfolder6s: TwoOrbitUf6 array) : Msuf6 =
        if twoOrbitUnfolder6s.Length < 1 then
            failwith $"Must have at least 1 TwoOrbitUnfolder6, got %d{twoOrbitUnfolder6s.Length}"
        else if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        else if twoOrbitUnfolder6s |> Array.exists (fun tou -> tou.Order <> %sortingWidth) then
            failwith $"All TwoOrbitUnfolder6 must have order {%sortingWidth}"
        else
            { id = id; sortingWidth = sortingWidth; twoOrbitUnfolder6s = twoOrbitUnfolder6s }

    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.TwoOrbitUnfolder6s with get () = this.twoOrbitUnfolder6s
    member this.StageCount with get () = (this.twoOrbitUnfolder6s.Length |> UMX.tag<stageCount>)
    member this.toString() =
        sprintf "msuf6(Id=%A, SortingWidth=%d, TwoOrbitUnfolder6Count=%d)" 
                (%this.Id) 
                (%this.SortingWidth) 
                (this.TwoOrbitUnfolder6s.Length)

    override this.Equals(obj) = 
        match obj with
        | :? Msuf6 as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.id)

    interface IEquatable<Msuf6> with
        member this.Equals(other) = 
            this.id = other.id

    member this.MakeSorter() = 
        let ces = 
            this.TwoOrbitUnfolder6s
            |> Array.collect (fun tou ->
                tou.MakePerm_Si
                |> Perm_Si.getTwoOrbits
                |> Array.map Ce.fromTwoOrbit)
        Sorter.create (%this.Id |> UMX.tag<sorterId>) this.SortingWidth ces



module Msuf6 =

    /// Returns a string representation of the Msuf6 instance.
    let toString (msuf6: Msuf6) : string =
        sprintf "Msuf6(Id=%A, SortingWidth=%d, TwoOrbitUnfolder6Count=%d)" 
                (%msuf6.Id) 
                (%msuf6.SortingWidth) 
                msuf6.StageCount