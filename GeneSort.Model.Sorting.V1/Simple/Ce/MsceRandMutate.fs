namespace GeneSort.Model.Sorting.V1.Simple.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting

[<Struct; CustomEquality; NoComparison>]
type msceRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorId>
          rngFactory: rngFactory
          indelRates: indelRates
          excludeSelfCe: bool<excludeSelfCe> }
    with
    static member create 
            (rngFactory: rngFactory)
            (indelRates: indelRates) // Single value
            (excludeSelfCe: bool<excludeSelfCe>) : msceRandMutate = 
        
        let id =
            [
                box "msceRandMutate"
                box rngFactory
                box (indelRates.GetHashCode())
                box (excludeSelfCe |> UMX.untag)
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            rngFactory = rngFactory
            indelRates = indelRates
            excludeSelfCe = excludeSelfCe
        }
        
    member this.Id with get () = this.id
    member this.RngFactory with get () = this.rngFactory
    member this.IndelRates with get () = this.indelRates
    member this.ExcludeSelfCe with get () = this.excludeSelfCe

    override this.Equals(obj) = 
        match obj with
        | :? msceRandMutate as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.Id)

    interface IEquatable<msceRandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSorterModelId (parent: msce) (index: int<mutationIndex>) : Guid<sorterModelId> =
        CommonMutator.makeSorterModelId parent.Id this.Id index


    member this.MakeSorterModelFromId 
                            (parent: msce) 
                            (id: Guid<sorterModelId>) : msce =
        let rng = this.RngFactory.Create %id
        let excludeSelfCe = this.ExcludeSelfCe
        let sortingWidth = %parent.SortingWidth
        
        let ceCodeInserter = fun () -> Ce.generateCeCode %excludeSelfCe sortingWidth (rng.NextIndex)
        let ceCodeMutator = fun _ -> Ce.generateCeCode %excludeSelfCe sortingWidth (rng.NextIndex)
        
        let ceCodes = IndelRates.mutate 
                        this.IndelRates 
                        ceCodeInserter 
                        ceCodeMutator 
                        (rng.NextFloat) 
                        parent.CeCodes

        msce.create id parent.SortingWidth ceCodes


    member this.MakeSorterModelFromIndex (parent: msce) (index: int<mutationIndex>) : msce =
        let id = this.MakeSorterModelId parent index
        this.MakeSorterModelFromId parent id



module MsceRandMutate =
    let toString (msceMutate: msceRandMutate) : string = 
        sprintf "MsceRandMutate(%s, %s, %b)"
            (msceMutate.RngFactory.ToString())
            (msceMutate.IndelRates.toString())
            %msceMutate.ExcludeSelfCe