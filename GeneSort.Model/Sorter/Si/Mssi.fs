namespace GeneSort.Model.Sorter.Si

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Component
open GeneSort.Component.Sorter
open GeneSort.Component.Sortable
open GeneSort.Model.Sorter
 
[<Struct; CustomEquality; NoComparison>]
type Mssi = 
    private 
        { id: Guid<sorterModelID>
          sortingWidth: int<sortingWidth>
          perm_Sis: Perm_Si array } 
    with
    static member create 
            (id: Guid<sorterModelID>) 
            (sortingWidth: int<sortingWidth>) 
            (perm_Sis: Perm_Si array) : Mssi =
        if perm_Sis.Length < 1 then
            failwith "Must have at least 1 Perm_Si"
        else if %sortingWidth < 1 then
            failwith "Width must be at least 1"
        else
            { id = id; sortingWidth = sortingWidth; perm_Sis = perm_Sis }

    member this.Id with get () = this.id
    member this.SortingWidth with get () = this.sortingWidth
    member this.CeLength with get () = (this.StageLength * %this.SortingWidth / 2) |> UMX.tag<ceLength>
    member this.StageLength with get () = this.perm_Sis.Length |> UMX.tag<stageLength>
    member this.Perm_Sis with get () = this.perm_Sis
    member this.toString() =
        sprintf "mssi(Id=%A, SortingWidth=%d, StageLength=%d)" 
                (%this.Id) 
                (%this.SortingWidth)
                (this.StageLength)

    override this.Equals(obj) = 
        match obj with
        | :? Mssi as other -> 
            this.id = other.id && 
            this.sortingWidth = other.sortingWidth && 
            this.perm_Sis = other.perm_Sis
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.id, this.sortingWidth, this.perm_Sis)

    interface IEquatable<Mssi> with
        member this.Equals(other) = 
            this.id = other.id &&  
            this.sortingWidth = other.sortingWidth && 
            this.perm_Sis = other.perm_Sis

    member this.MakeSorter() = 
        let ces = this.perm_Sis
                    |> Array.map (fun psi -> psi |> Perm_Si.getTwoOrbits)
                    |> Array.collect(id)
                    |> Array.map(fun tbit -> ce.create tbit.First tbit.Second)
        sorter.create (%this.Id |> UMX.tag<sorterId>) this.SortingWidth ces




module Mssi =

    let toString (mssi: Mssi) : string =
        sprintf "Mssi(Id=%A, Width=%d, StageLength=%d)" 
                (%mssi.Id) 
                (%mssi.SortingWidth) 
                mssi.StageLength

    let makeSorter (mssi: Mssi) : sorter =
        let ces = mssi.perm_Sis
                    |> Array.map (fun psi -> psi |> Perm_Si.getTwoOrbits)
                    |> Array.collect(id)
                    |> Array.map(fun tbit -> ce.create tbit.First tbit.Second)
        sorter.create (%mssi.Id |> UMX.tag<sorterId>) mssi.SortingWidth ces
         
