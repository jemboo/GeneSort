namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<Measure>] type sorterMergeFactor

[<Struct; CustomEquality; NoComparison>]
type msasM = 
    private 
        { id: Guid<sorterTestModelID>
          sorterMergeFactor: int<sorterMergeFactor>
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            (sorterMergeFactor: int<sorterMergeFactor>)
            : msasM =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        if (%sortingWidth) % (%sorterMergeFactor) <> 0 then
            failwith "sorterMergeFactor must evenly divide sortingWidth"
        else
            let id = 
                [
                    "MsasMi" :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { 
                id = id; 
                sorterMergeFactor = sorterMergeFactor
                sortingWidth = sortingWidth; 
            }

    member this.Id with get() = this.id

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msasM as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<msasM> with
        member this.Equals(other) =  this.sortingWidth = other.sortingWidth

    member this.MakeSortableIntTests 
                (sortingWidth: int<sortingWidth>) : sortableIntTests =
        sortableIntTests.create 
                ( %this.id |> UMX.tag<sortableTestsId>) 
                sortingWidth
                (SortableIntArray.getMerge2TestCases sortingWidth)


    member this.MakeSortableBoolTests (sortingWidth: int<sortingWidth>) : sortableBoolTests =
        let sortableArrays =  SortableBoolArray.getMerge2TestCases sortingWidth
        sortableBoolTests.create 
                ( %this.id |> UMX.tag<sortableTestsId>) 
                sortingWidth
                sortableArrays



module MsasMi = ()
 
 