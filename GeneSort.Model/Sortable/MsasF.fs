namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sorter
open GeneSort.Sorter.Sortable
open GeneSort.Model.Sorter


[<Struct; CustomEquality; NoComparison>]
type MsasF = 
    private 
        { id: Guid<sorterTestModelID>
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            : MsasF =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        else
            let id = 
                [
                    "MsasF" :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { id = id; sortingWidth = sortingWidth; }

    member this.Id with get() = this.id

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? MsasF as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<MsasF> with
        member this.Equals(other) =  this.sortingWidth = other.sortingWidth

    member this.MakeSorterTest (sortingWidth: int<sortingWidth>) : sorterTests =
        let sortableArrays =  SortableBoolArray.getAllSortableBoolArrays sortingWidth
        sorterBoolTests.create ( %this.id |> UMX.tag<sorterTestsId>) sortableArrays |> sorterTests.Bools

module MsasF = ()
 
 