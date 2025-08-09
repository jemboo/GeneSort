namespace GeneSort.Model.Sorter.Ce

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter

[<Struct; CustomEquality; NoComparison>]
type MsceRandMutate = 
    private 
        { 
          id : Guid<sorterModelMakerID>
          msce : Msce
          rngType: rngType
          indelRatesArray: IndelRatesArray
          excludeSelfCe: bool }
    static member create 
            (rngType: rngType)
            (indelRatesArray: IndelRatesArray)
            (excludeSelfCe: bool) 
            (msce : Msce): MsceRandMutate = 
        if %msce.CeCount <> indelRatesArray.Length then failwith "CeCount must match indelRatesArray.Length"
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
    member this.CeCount with get () = this.msce.CeCount
    member this.IndelRatesArray with get () = this.indelRatesArray
    member this.ExcludeSelfCe with get () = this.excludeSelfCe

    override this.Equals(obj) = 
        match obj with
        | :? MsceRandMutate as other -> 
            this.Id = other.Id

        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.indelRatesArray, this.excludeSelfCe, this.msce)

    interface IEquatable<MsceRandMutate> with
        member this.Equals(other) = 
            this.Id = other.Id

    interface ISorterModelMaker with
        member this.Id = this.id

        /// Mutates an Msce by applying ChromosomeRates.mutate to its ceCodes array.
        /// Generates a new Msce with a new ID, the same sortingWidth, and a mutated ceCodes array.
        /// The ceCodes array is modified using the provided indelRatesArray, with insertions and mutations
        /// generated via Ce.generateCeCode, and deletions handled to maintain the ceCount length.
        member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                    : ISorterModel =
            let id = SorterModelMaker.makeSorterModelId this index
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
            Msce.create id this.msce.SortingWidth ceCodes


module MsceRandMutate =

    /// Returns a string representation of the MsceRandMutate instance.
    let toString (msceMutate: MsceRandMutate) : string = 
        sprintf "MsceRandMutate(%s, %d, %s, %b)"
            (msceMutate.RngType.ToString())
            (%msceMutate.CeCount)
            (msceMutate.IndelRatesArray.toString())
            msceMutate.ExcludeSelfCe