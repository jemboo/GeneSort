namespace GeneSort.Model.Sortable.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.SortingLib.Sorter

[<Struct; CustomEquality; NoComparison>]
type msasPfx = 
    private 
        { id: Guid<sorterTestModelID>
          sorterKey: sorterKey
          sortableIntArrays : sortableIntArray array
          seedPermutation : permutation
          maxOrbit: int; }

    static member create
            (sorterKey: sorterKey)
            (seedPermutation : permutation)
            (maxOrbit : int )
            : msasPfx =
            let sias = seedPermutation |> SortableIntArray.getOrbit maxOrbit |> Seq.toArray
            let id =
                [
                    "MsasPfx" :> obj
                    sorterKey :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>

            { id = id; 
              sorterKey = sorterKey;
              seedPermutation = seedPermutation; maxOrbit = maxOrbit; sortableIntArrays = sias}

    member this.Id with get() = this.id
    member this.MaxOrbit with get() = this.maxOrbit 
    member this.SeedPermutation with get() = this.seedPermutation
    member this.SortingWidth with get() = (%this.seedPermutation.Order |> UMX.tag<sortingWidth>)

    override this.Equals(obj) =
        match obj with
        | :? msasO as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.id, this.sorterKey, this.maxOrbit)

    interface IEquatable<msasPfx> with
        member this.Equals(other) = 
            this.id = other.id

    member this.MakeSortableBoolTest 
            (sorterTestId: Guid<sortableTestId>)
            (sortingWidth: int<sortingWidth>) : sortableBinaryTest =
        let bArrays =
            this.sortableIntArrays 
            |> Array.map(fun sia -> sia.ToSortableBoolArrays())
            |> Array.collect id
            |> Array.distinct
        sortableBinaryTest.create 
                sorterTestId 
                sortingWidth
                bArrays

    member this.MakeSortableIntTest 
            (sorterTestId: Guid<sortableTestId>)
            (sortingWidth: int<sortingWidth>) : sortableIntTest =
        sortableIntTest.create 
                sorterTestId 
                sortingWidth
                this.sortableIntArrays


module MsasPfx = ()
 
 