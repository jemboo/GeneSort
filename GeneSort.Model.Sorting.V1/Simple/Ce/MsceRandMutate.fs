namespace GeneSort.Model.Sorting.V1.Simple.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1

[<Struct; CustomEquality; NoComparison>]
type msceRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorId>
          msce : msce
          rngFactory: rngFactory
          indelRates: indelRates // Changed from indelRatesArray
          excludeSelfCe: bool }
    with
    static member create 
            (rngFactory: rngFactory)
            (indelRates: indelRates) // Single value
            (excludeSelfCe: bool) 
            (msce : msce) : msceRandMutate = 
        
        let id =
            [
                msce.Id :> obj
                rngFactory :> obj
                indelRates.GetHashCode() :> obj
                excludeSelfCe :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorId>

        {
            id = id
            msce = msce
            rngFactory = rngFactory
            indelRates = indelRates
            excludeSelfCe = excludeSelfCe
        }
        
    member this.Id with get () = this.id
    member this.Msce with get () = this.msce
    member this.RngFactory with get () = this.rngFactory
    member this.CeLength with get () = this.msce.CeLength
    member this.IndelRates with get () = this.indelRates
    member this.ExcludeSelfCe with get () = this.excludeSelfCe
    member this.SortingWidth with get () = this.msce.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msceRandMutate as other -> this.Id = other.Id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.indelRates, this.excludeSelfCe, this.msce)

    interface IEquatable<msceRandMutate> with
        member this.Equals(other) = this.Id = other.Id

    member this.MakeSimpleSorterModelId (index: int) : Guid<simpleSorterModelId> =
        CommonMutator.makeSorterModelId this.Id index


    member this.MakeSimpleSorterModelFromId (id: Guid<simpleSorterModelId>) : msce =
        let rng = this.RngFactory.Create %id
        let excludeSelfCe = this.ExcludeSelfCe
        let sortingWidth = %this.msce.SortingWidth
        
        // Define generation logic
        let ceCodeInserter = fun () -> Ce.generateCeCode excludeSelfCe sortingWidth (rng.NextIndex)
        let ceCodeMutator = fun _ -> Ce.generateCeCode excludeSelfCe sortingWidth (rng.NextIndex)
        
        // Directly use the uniform rates without an intermediate array allocation
        let ceCodes = IndelRates.mutate 
                        this.IndelRates 
                        ceCodeInserter 
                        ceCodeMutator 
                        (rng.NextFloat) 
                        this.msce.CeCodes

        msce.create id this.msce.SortingWidth ceCodes



    member this.MakeSimpleSorterModelFromIndex (index: int) : msce =
        let id = this.MakeSimpleSorterModelId index
        this.MakeSimpleSorterModelFromId id



module MsceRandMutate =
    let toString (msceMutate: msceRandMutate) : string = 
        sprintf "MsceRandMutate(%s, %d, %s, %b)"
            (msceMutate.RngFactory.ToString())
            (%msceMutate.CeLength)
            (msceMutate.IndelRates.toString())
            msceMutate.ExcludeSelfCe