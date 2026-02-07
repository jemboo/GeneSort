namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting


[<Struct; CustomEquality; NoComparison>]
type MsasORandGen = 
    private 
        { 
          id : Guid<sorterTestModelMakerID>
          rngType: rngType
          sortingWidth: int<sortingWidth>
          maxOrbit: int } 

    static member create
            (rngType : rngType)
            (sortingWidth : int<sortingWidth>)
            (maxOrbit : int )
            : MsasORandGen =
            let id =
                [
                    "MsasORandGen" :> obj
                    sortingWidth :> obj
                    rngType :> obj
                    maxOrbit :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelMakerID>

            { id = id; rngType = rngType; maxOrbit = maxOrbit; sortingWidth = sortingWidth}

    member this.Id with get() = this.id
    member this.MaxOrbit with get() = this.maxOrbit
    member this.RngType with get() = this.rngType
    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? MsasORandGen as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.id, this.rngType, this.sortingWidth, this.maxOrbit)

    interface IEquatable<MsasORandGen> with
        member this.Equals(other) = 
            this.id = other.id

    member this.getMsasOs (offset: int) : sortableTestModel seq =
            let randy = Rando.create this.RngType (%this.id)
            let sw = %this.sortingWidth
            let maxO = this.maxOrbit
            let permSeq = 
                seq {   while true do
                            yield  Permutation.randomPermutation (randy.NextIndex) sw
                    }
            permSeq |> Seq.skip offset |> Seq.map(fun perm -> msasO.create perm maxO |> sortableTestModel.MsasO)


module MsasORandGen = ()
 
 