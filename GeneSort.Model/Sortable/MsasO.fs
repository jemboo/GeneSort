namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

[<Struct; CustomEquality; NoComparison>]
type msasO = 
    private 
        { id: Guid<sorterTestModelID>
          sortableIntArrays : sortableIntArray array
          seedPermutation : permutation
          maxOrbit: int; }

    static member create
            (seedPermutation : permutation)
            (maxOrbit : int )
            : msasO =
            let sias = seedPermutation |> SortableIntArray.getOrbit maxOrbit |> Seq.toArray
            
            // Build the components safely using sequence matching
            let identityComponents = seq {
                box "msasO"
                box seedPermutation
                box maxOrbit
            }
            
            let id = identityComponents |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>

            { id = id; seedPermutation = seedPermutation; maxOrbit = maxOrbit; sortableIntArrays = sias}

    member this.Id with get() = this.id
    member this.MaxOrbit with get() = this.maxOrbit 
    member this.SeedPermutation with get() = this.seedPermutation
    member this.SortingWidth with get() = (%this.seedPermutation.Order |> UMX.tag<sortingWidth>)

    override this.Equals(obj) =
        match obj with
        | :? msasO as other -> this.id = other.id
        | _ -> false


    override this.GetHashCode() = hash this.id

    interface IEquatable<msasO> with
        member this.Equals(other) = this.id = other.id

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

module MsasO = ()