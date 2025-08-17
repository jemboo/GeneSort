namespace GeneSort.Model.Sorter.Rs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.Perm_RsOps
open GeneSort.Sorter
open GeneSort.Model.Sorter

/// Represents a configuration for generating random Msrs instances with specified mutation rates.
/// Combines random generation and mutation probabilities for Perm_Rs instances.
[<Struct; CustomEquality; NoComparison>]
type MsrsRandGen = 
    private 
        { 
              id : Guid<sorterModelMakerID>
              rngType: rngType
              opsGenRatesArray: OpsGenRatesArray
              sortingWidth: int<sortingWidth> 
        } 
    static member create 
            (rngType: rngType)
            (sortingWidth: int<sortingWidth>)
            (opsGenRatesArray: OpsGenRatesArray)
            : MsrsRandGen =

        if %sortingWidth < 2 then
            failwith $"SortingWidth must be at least 2, got {%sortingWidth}"
        let id =
            [
                rngType :> obj
                sortingWidth :> obj
                opsGenRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        {
            id = id
            sortingWidth = sortingWidth
            opsGenRatesArray = opsGenRatesArray
            rngType = rngType
        }
        
    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.OpsGenRatesArray with get () = this.opsGenRatesArray
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageCount with get () = this.opsGenRatesArray.Length

    override this.Equals(obj) = 
        match obj with
        | :? MsrsRandGen as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngType, this.opsGenRatesArray, this.sortingWidth)

    interface IEquatable<MsrsRandGen> with
        member this.Equals(other) = 
            this.Id = other.Id

        
    member this.MakeSorterModel (rngFactory: rngType -> Guid -> IRando) (index: int) 
                : Msrs =
        let id = Common.makeSorterModelId this.Id index
        let rng = rngFactory this.RngType %id
        let genRatesArray = this.OpsGenRatesArray
        let stageCount = %this.StageCount
        let sortingWidth = %this.SortingWidth
        let perm_Rss =
            [| for dex in 0 .. (stageCount - 1) ->
                Perm_RsOps.makeRandomPerm_Rs
                    (rng.NextIndex)
                    (rng.NextFloat)
                    (genRatesArray.[dex])
                    (sortingWidth) |]

        Msrs.create id this.SortingWidth perm_Rss


module MsrsRandGen =

    /// Returns a string representation of the MsrsRandGen configuration.
    let toString (msrsGen: MsrsRandGen) : string =
        sprintf "MsrsRandGen(RngType=%A, Width=%d, StageCount=%d, OpActionRatesArray=%s)" 
                msrsGen.RngType (%msrsGen.SortingWidth) (%msrsGen.StageCount) (msrsGen.OpsGenRatesArray.toString())
