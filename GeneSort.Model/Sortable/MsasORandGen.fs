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
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          maxOrbit: int } 

    static member create
            (rngFactory : rngFactory)
            (sortingWidth : int<sortingWidth>)
            (maxOrbit : int )
            : MsasORandGen =
            let id =
                [
                    "MsasORandGen" :> obj
                    sortingWidth :> obj
                    rngFactory :> obj
                    maxOrbit :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelMakerID>

            { id = id; rngFactory = rngFactory; maxOrbit = maxOrbit; sortingWidth = sortingWidth}

    member this.Id with get() = this.id
    member this.MaxOrbit with get() = this.maxOrbit
    member this.RngFactory with get() = this.rngFactory
    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? MsasORandGen as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.id, this.RngFactory, this.sortingWidth, this.maxOrbit)

    interface IEquatable<MsasORandGen> with
        member this.Equals(other) = 
            this.id = other.id

    member this.getMsasOs (offset: int) : sortableTestModel seq =
            let randy = this.RngFactory.Create (%this.id)
            let sw = %this.sortingWidth
            let maxO = this.maxOrbit
            let permSeq = 
                seq {   while true do
                            yield  Permutation.randomPermutation (randy.NextIndex) sw
                    }
            permSeq |> Seq.skip offset |> Seq.map(fun perm -> msasO.create perm maxO |> sortableTestModel.MsasO)


module MsasORandGen = ()
 
 