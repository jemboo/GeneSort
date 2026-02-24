namespace GeneSort.Model.Sorting.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting

[<Struct; CustomEquality; NoComparison>]
type msceRandMutate = 
    private 
        { 
          id : Guid<sorterModelMutatorID>
          msce : msce
          rngFactory: rngFactory
          indelRatesArray: indelRatesArray
          excludeSelfCe: bool }
    with
    static member create 
            (rngFactory: rngFactory)
            (indelRatesArray: indelRatesArray)
            (excludeSelfCe: bool) 
            (msce : msce) : msceRandMutate = 
        if %msce.CeLength <> indelRatesArray.Length then failwith "CeCount must match indelRatesArray.Length"
        let id =
            [
                msce :> obj
                rngFactory :> obj
                indelRatesArray :> obj
                excludeSelfCe :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMutatorID>

        {
            id = id
            msce = msce
            rngFactory = rngFactory
            indelRatesArray = indelRatesArray
            excludeSelfCe = excludeSelfCe
        }
        
    member this.Id with get () = this.id
    member this.Msce with get () = this.msce
    member this.RngFactory with get () = this.rngFactory
    member this.CeLength with get () = this.msce.CeLength
    member this.IndelRatesArray with get () = this.indelRatesArray
    member this.ExcludeSelfCe with get () = this.excludeSelfCe
    member this.SortingWidth with get () = this.msce.SortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msceRandMutate as other -> 
            this.Id = other.Id

        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.indelRatesArray, this.excludeSelfCe, this.msce)

    interface IEquatable<msceRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id



    /// Mutates an Msce by applying IndelRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided indelRatesArray, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (index: int) 
                : msce =
        let id = CommonMutator.makeSorterModelId this.Id index
        let rng = this.RngFactory.Create %id
        let excludeSelfCe = this.ExcludeSelfCe
        let sortingWidth = %this.msce.SortingWidth
        let ceCodeInserter = fun () -> Ce.generateCeCode excludeSelfCe sortingWidth (rng.NextIndex)
        let ceCodeMutator = fun ce -> Ce.generateCeCode excludeSelfCe sortingWidth (rng.NextIndex)
        let ceCodes = IndelRatesArray.mutate 
                        this.IndelRatesArray 
                        ceCodeInserter 
                        ceCodeMutator 
                        (rng.NextFloat) 
                        this.msce.CeCodes
        msce.create id this.msce.SortingWidth ceCodes


module MsceRandMutate =

    /// Returns a string representation of the MsceRandMutate instance.
    let toString (msceMutate: msceRandMutate) : string = 
        sprintf "MsceRandMutate(%s, %d, %s, %b)"
            (msceMutate.RngFactory.ToString())
            (%msceMutate.CeLength)
            (msceMutate.IndelRatesArray.toString())
            msceMutate.ExcludeSelfCe