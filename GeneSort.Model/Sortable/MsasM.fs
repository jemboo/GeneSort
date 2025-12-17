namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<Struct; CustomEquality; NoComparison>]
type msasM = 
    private 
        { id: Guid<sorterTestModelID>
          mergeDimension: int<mergeDimension>
          mergeFillType: mergeFillType
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            (mergeDimension: int<mergeDimension>)
            (mergeFillType: mergeFillType)
            : msasM =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        if (%sortingWidth) % (%mergeDimension) <> 0 then
            failwith "mergeDimension must evenly divide sortingWidth"
        else
            let id = 
                [
                    "MsasMi" :> obj
                    mergeDimension :> obj
                    mergeFillType :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { 
                id = id; 
                mergeDimension = mergeDimension
                mergeFillType = mergeFillType
                sortingWidth = sortingWidth
            }

    member this.Id with get() = this.id

    member this.MergeDimension with get() = this.mergeDimension
    member this.MergeFillType with get() = this.mergeFillType

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

    member this.MakeSortableIntTests : sortableIntTests =
        let intArrays = SortableIntArray.getMergeTestCases 
                            this.SortingWidth
                            this.MergeDimension
                            this.mergeFillType

        sortableIntTests.create 
                ( %this.id |> UMX.tag<sortableTestsId>) 
                this.SortingWidth
                intArrays


    member this.MakeSortableBoolTests : sortableBoolTests =

        let sortableArrays = SortableBoolArray.getMergeTestCases
                                    this.sortingWidth
                                    this.mergeDimension
                                    this.mergeFillType  

        sortableBoolTests.create 
                ( %this.id |> UMX.tag<sortableTestsId>) 
                this.sortingWidth
                sortableArrays


module MsasMi = ()
 
 