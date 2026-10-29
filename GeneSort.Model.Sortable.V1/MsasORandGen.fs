namespace GeneSort.Model.Sortable.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting


//[<Struct; CustomEquality; NoComparison>]
//type MsasORandGen = 
//    private 
//        { 
//          id : Guid<sorterTestModelGenID>
//          rngFactory: rngFactory
//          channelCount: int<channelCount>
//          maxOrbit: int } 

//    static member create
//            (rngFactory : rngFactory)
//            (channelCount : int<channelCount>)
//            (maxOrbit : int )
//            : MsasORandGen =
//            let id =
//                [
//                    "MsasORandGen" :> obj
//                    channelCount :> obj
//                    rngFactory :> obj
//                    maxOrbit :> obj
//                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelGenID>

//            { id = id; rngFactory = rngFactory; maxOrbit = maxOrbit; channelCount = channelCount}

//    member this.Id with get() = this.id
//    member this.MaxOrbit with get() = this.maxOrbit
//    member this.RngFactory with get() = this.rngFactory
//    member this.ChannelCount with get() = this.channelCount

//    override this.Equals(obj) = 
//        match obj with
//        | :? MsasORandGen as other -> 
//            this.id = other.id
//        | _ -> false

//    override this.GetHashCode() = 
//        hash (this.id, this.RngFactory, this.channelCount, this.maxOrbit)

//    interface IEquatable<MsasORandGen> with
//        member this.Equals(other) = 
//            this.id = other.id

//    member this.getMsasOs (offset: int) : sortableTestsModel seq =
//            let randy = this.RngFactory.Create (%this.id)
//            let sw = %this.channelCount
//            let maxO = this.maxOrbit
//            let permSeq = 
//                seq {   while true do
//                            yield  Permutation.randomPermutation (randy.NextIndex) sw
//                    }
//            permSeq |> Seq.skip offset |> Seq.map(fun perm -> msasO.create perm maxO |> sortableTestsModel.MsasO)


//module MsasORandGen = ()
 
 