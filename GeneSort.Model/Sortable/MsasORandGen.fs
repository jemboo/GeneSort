namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting

[<Struct; CustomEquality; NoComparison>]
type msasORandGen = 
    private 
        { 
          id : Guid<sorterTestModelGenId>
          rngFactory: rngFactory
          sortingWidth: int<sortingWidth>
          maxOrbit: int 
        } 

    static member create
            (rngFactory : rngFactory)
            (sortingWidth : int<sortingWidth>)
            (maxOrbit : int )
            : msasORandGen =
            
            // Clean, deterministic sequential layout using primitive-matching and interfaces
            let identityComponents = seq {
                box "msasORandGen"
                box (sortingWidth |> UMX.untag)
                box rngFactory
                box maxOrbit
            }

            let id = identityComponents |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelGenId>

            { id = id; rngFactory = rngFactory; maxOrbit = maxOrbit; sortingWidth = sortingWidth}

    member this.Id with get() = this.id
    member this.MaxOrbit with get() = this.maxOrbit
    member this.RngFactory with get() = this.rngFactory
    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msasORandGen as other -> this.id = other.id
        | _ -> false

    // Aligned to match the explicit ID equality definition
    override this.GetHashCode() = hash this.id

    interface IEquatable<msasORandGen> with
        member this.Equals(other) = this.id = other.id

    // Interface fulfillment allowing this generator to be serialized safely inside collections
    interface IStableSerializable with
        member this.WriteStableBytes (writer: System.IO.BinaryWriter) =
            let rawGuid = UMX.untag this.id
            writer.Write(rawGuid.ToByteArray())

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
