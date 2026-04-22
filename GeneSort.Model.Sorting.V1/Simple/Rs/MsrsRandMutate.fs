namespace GeneSort.Model.Sorting.V1.Simple.Rs

open FSharp.UMX
open GeneSort.Core
open System
open GeneSort.Model.Sorting.V1

[<Struct; CustomEquality; NoComparison>]
type msrsRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorId>
          msrs : msrs
          rngFactory: rngFactory
          opsActionRates: opsActionRates 
        } 
    with
    static member create 
            (rngFactory: rngFactory)
            (opsActionRates: opsActionRates)
            (msrs: msrs) : msrsRandMutate =
        
        let id =
            [
                rngFactory :> obj
                msrs.Id :> obj
                opsActionRates.GetHashCode() :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            msrs = msrs
            rngFactory = rngFactory
            opsActionRates = opsActionRates
        }

    member this.Id with get () = this.id
    member this.Msrs with get () = this.msrs
    member this.CeLength with get () = this.msrs.CeLength
    member this.RngFactory with get () = this.rngFactory
    member this.OpsActionRates with get () = this.opsActionRates
    member this.SortingWidth with get () = this.msrs.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msrsRandMutate as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.RngFactory, this.msrs, this.opsActionRates)

    interface IEquatable<msrsRandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId (index: int) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId this.Id index

    member this.MakeSimpleSorterModelFromId (id: Guid<sorterModelId>) : msrs =
        let rng = this.RngFactory.Create %id
        
        // Define specific mutation behaviors using Perm_RsOps
        let orthoMutator = fun psi -> Perm_RsOps.mutatePerm_Rs (rng.NextIndex) opsActionMode.Ortho psi 
        let paraMutator = fun psi -> Perm_RsOps.mutatePerm_Rs (rng.NextIndex) opsActionMode.Para psi 
        let selfSymMutator = fun psi -> Perm_RsOps.mutatePerm_Rs (rng.NextIndex) opsActionMode.SelfRefl psi 
        
        // Perform the mutation using the uniform rate module
        let mutated = OpsActionRates.mutate 
                        this.OpsActionRates 
                        orthoMutator 
                        paraMutator 
                        selfSymMutator
                        (rng.NextFloat) 
                        this.Msrs.Perm_Rss

        msrs.create id this.Msrs.SortingWidth mutated

    member this.MakeSimpleSorterModelFromIndex (index: int) : msrs =
        let id = this.MakeSorterModelId index
        this.MakeSimpleSorterModelFromId id

module MsrsRandMutate =
    let toString (msrsRandMutate: msrsRandMutate) : string =
        sprintf "MsrsRandMutate(RngType=%A, Msrs=%s, OpsActionRates=%s)" 
                msrsRandMutate.RngFactory 
                (msrsRandMutate.Msrs.toString())
                (msrsRandMutate.OpsActionRates.toString())