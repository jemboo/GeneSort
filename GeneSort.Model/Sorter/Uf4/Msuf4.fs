namespace GeneSort.Model.Sorter.Uf4

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Core
open GeneSort.Model.Sorter

/// Represents a collection of TwoOrbitUnfolder4 instances with a consistent sorting width.
[<Struct; CustomEquality; NoComparison>]
type Msuf4 = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          twoOrbitUnfolder4s: TwoOrbitUf4 array } 
    with
    /// Creates an Msuf4 instance with the specified ID, sorting width, and array of TwoOrbitUnfolder4 instances.
    /// Throws an exception if the array is empty, width is less than 1, or any TwoOrbitUnfolder4 has a mismatched order.
    static member create 
            (id: Guid<sorterModelID>) 
            (sortingWidth: int<sortingWidth>) 
            (twoOrbitUnfolder4s: TwoOrbitUf4 array) : Msuf4 =
        if twoOrbitUnfolder4s.Length < 1 then
            failwith $"Must have at least 1 TwoOrbitUnfolder4, got %d{twoOrbitUnfolder4s.Length}"
        else if %sortingWidth < 1 then
            failwith $"SortingWidth must be at least 1, got {%sortingWidth}"
        else if twoOrbitUnfolder4s |> Array.exists (fun tou -> tou.Order <> %sortingWidth) then
            failwith $"All TwoOrbitUnfolder4 must have order {%sortingWidth}"
        else
            { id = id; sortingWidth = sortingWidth; twoOrbitUnfolder4s = twoOrbitUnfolder4s }

    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.TwoOrbitUnfolder4s with get () = this.twoOrbitUnfolder4s
    member this.StageCount with get () = (this.twoOrbitUnfolder4s.Length |> UMX.tag<stageCount>)
    member this.toString() =
        sprintf "msuf4(Id=%A, SortingWidth=%d, TwoOrbitUnfolder4Count=%d)" 
                (%this.Id) 
                (%this.SortingWidth) 
                (this.TwoOrbitUnfolder4s.Length)

    override this.Equals(obj) = 
        match obj with
        | :? Msuf4 as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.id)

    interface IEquatable<Msuf4> with
        member this.Equals(other) = 
            this.id = other.id

    member this.MakeSorter() = 
        let ces = 
            this.TwoOrbitUnfolder4s
            |> Array.collect (fun tou ->
                tou.MakePerm_Si
                |> Perm_Si.getTwoOrbits
                |> Array.map Ce.fromTwoOrbit)
        Sorter.create (%this.Id |> UMX.tag<sorterId>) this.SortingWidth ces



module Msuf4 =

    /// Returns a string representation of the Msuf4 instance.
    let toString (msuf4: Msuf4) : string =
        sprintf "Msuf4(Id=%A, SortingWidth=%d, TwoOrbitUnfolder4Count=%d)" 
                (%msuf4.Id) 
                (%msuf4.SortingWidth) 
                msuf4.StageCount