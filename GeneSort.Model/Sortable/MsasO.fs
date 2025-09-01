namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sorter


[<Struct; CustomEquality; NoComparison>]
type MsasO = 
    private 
        { id: Guid<sorterTestModelID>
          sortableIntArrays : sortableIntArray array
          seedPermutation : Permutation
          maxOrbit: int; }

    static member create
            (seedPermutation : Permutation)
            (maxOrbit : int )
            : MsasO =
            let sias = seedPermutation |> SortableIntArray.getOrbit maxOrbit |> Seq.toArray
            let id =
                [
                    "MsasO" :> obj
                    seedPermutation :> obj
                    maxOrbit :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>

            { id = id; seedPermutation = seedPermutation; maxOrbit = maxOrbit; sortableIntArrays = sias}

    member this.Id with get() = this.id
    member this.SeedPermutation with get() = this.seedPermutation
    member this.SortingWidth with get() = (%this.seedPermutation.Order |> UMX.tag<sortingWidth>)

    override this.Equals(obj) = 
        match obj with
        | :? MsasO as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.id, this.seedPermutation, this.maxOrbit)

    interface IEquatable<MsasO> with
        member this.Equals(other) = 
            this.id = other.id

    member this.MakeSortableBoolArraySet 
            (sortingWidth: int<sortingWidth>) : sorterBoolTests =
        let ssId = %this.Id |> UMX.tag<sorterTestsId>
        let bArrays =
            this.sortableIntArrays 
            |> Array.map(fun sia -> sia.ToSortableBoolArrays())
            |> Array.collect id
            |> Array.distinct
        sorterBoolTests.create ssId bArrays


    member this.MakeSortableIntArraySet 
            (sortingWidth: int<sortingWidth>) : sorterIntTests =
        let ssId = %this.Id |> UMX.tag<sorterTestsId>
        sorterIntTests.create ssId this.sortableIntArrays


module MsasO = ()
 
 