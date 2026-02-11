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
          id : Guid<sorterModelMakerID>
          msce : msce
          rngType: rngType
          indelRatesArray: indelRatesArray
          excludeSelfCe: bool }
    static member create 
            (rngType: rngType)
            (indelRatesArray: indelRatesArray)
            (excludeSelfCe: bool) 
            (msce : msce): msceRandMutate = 
        if %msce.CeLength <> indelRatesArray.Length then failwith "CeCount must match indelRatesArray.Length"
        let id =
            [
                msce :> obj
                rngType :> obj
                indelRatesArray :> obj
                excludeSelfCe :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        {
            id = id
            msce = msce
            rngType = rngType
            indelRatesArray = indelRatesArray
            excludeSelfCe = excludeSelfCe
        }
        
    member this.Id with get () = this.id
    member this.Msce with get () = this.msce
    member this.RngType with get () = this.rngType
    member this.CeLength with get () = this.msce.CeLength
    member this.IndelRatesArray with get () = this.indelRatesArray
    member this.ExcludeSelfCe with get () = this.excludeSelfCe

    override this.Equals(obj) = 
        match obj with
        | :? msceRandMutate as other -> 
            this.Id = other.Id

        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.indelRatesArray, this.excludeSelfCe, this.msce)

    interface IEquatable<msceRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id


    /// Mutates an Msce by applying IndelRatesArray to its ceCodes array.
    /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
    /// The ceCodes array is modified using the provided indelRatesArray, with insertions and mutations
    /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                : msce =
        let id = Common.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
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
            (msceMutate.RngType.ToString())
            (%msceMutate.CeLength)
            (msceMutate.IndelRatesArray.toString())
            msceMutate.ExcludeSelfCe