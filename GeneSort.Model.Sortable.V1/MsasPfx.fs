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
          pfxLibId: prefixLibId }

    static member create
            (pfxLibId: prefixLibId) : msasPfx =
            let id =
                [
                    "MsasPfx" :> obj
                    pfxLibId :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>

            { id = id; pfxLibId = pfxLibId;}

    member this.Id with get() = this.id
    member this.PfxLibId with get() = this.pfxLibId
    member this.SortingWidth with get() = this.pfxLibId.SortingWidth
    member this.StageLength with get() = this.pfxLibId.StageLength

    override this.Equals(obj) =
        match obj with
        | :? msasO as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.id, this.pfxLibId)

    interface IEquatable<msasPfx> with
        member this.Equals(other) = 
            this.id = other.id

    member this.MakeSortableBoolTest 
            (sorterTestId: Guid<sortableTestId>) : sortableBinaryTest =
        let ceArray = (SorterDataParse.getCeArrayFromPrefixLib this.PfxLibId).Value
        let bArrays = SortableBoolArray.getAllPossibleResultsFromCeArray
                        ceArray
                        this.SortingWidth
                      |> Seq.toArray
        sortableBinaryTest.create 
                sorterTestId 
                this.SortingWidth
                bArrays

    member this.MakeSortableBitv512Test 
            (sorterTestId: Guid<sortableTestId>) : sortableBitv512Test =
        let ceArray = (SorterDataParse.getCeArrayFromPrefixLib this.PfxLibId).Value
        let bArrays = SortableBoolArray.getAllPossibleResultsFromCeArray
                        ceArray
                        this.SortingWidth
                      |> Seq.toArray
        SortableBitv512Test.fromBoolArrays sorterTestId this.SortingWidth bArrays



module MsasPfx = ()
 
 