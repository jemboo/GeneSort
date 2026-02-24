namespace GeneSort.Model.Sorting.Sorter.Rs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

/// Represents a configuration for generating random Msrs instances with specified mutation rates.
/// Combines random generation and mutation probabilities for Perm_Rs instances.
[<Struct; CustomEquality; NoComparison>]
type msrsRandGen = 
    private 
        { 
              id : Guid<sorterModelMakerID>
              rngFactory: rngFactory
              opsGenRatesArray: opsGenRatesArray
              sortingWidth: int<sortingWidth> 
        } 
    static member create 
            (rngFactory: rngFactory)
            (sortingWidth: int<sortingWidth>)
            (opsGenRatesArray: opsGenRatesArray)
            : msrsRandGen =

        if %sortingWidth < 2 then
            failwith $"SortingWidth must be at least 2, got {%sortingWidth}"
        let id =
            [
                rngFactory :> obj
                sortingWidth :> obj
                opsGenRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelMakerID>

        {
            id = id
            sortingWidth = sortingWidth
            opsGenRatesArray = opsGenRatesArray
            rngFactory = rngFactory
        }
        
    member this.Id with get () = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageLength / 2) |> UMX.tag<ceLength>
    member this.RngFactory with get () = this.rngFactory
    member this.OpsGenRatesArray with get () = this.opsGenRatesArray
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageLength with get () = this.opsGenRatesArray.Length

    override this.Equals(obj) = 
        match obj with
        | :? msrsRandGen as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.opsGenRatesArray, this.sortingWidth)

    interface IEquatable<msrsRandGen> with
        member this.Equals(other) = 
            this.Id = other.Id

        
    member this.MakeSorterModel (index: int) 
                : msrs =
        let id = CommonMaker.makeSorterModelId this.Id index
        let rng = this.RngFactory.Create %id
        let genRatesArray = this.OpsGenRatesArray
        let stageLength = %this.StageLength
        let sortingWidth = %this.SortingWidth
        let perm_Rss =
            [| for dex in 0 .. (stageLength - 1) ->
                Perm_RsOps.makeRandomPerm_Rs
                    (rng.NextIndex)
                    (rng.NextFloat)
                    (genRatesArray.[dex])
                    (sortingWidth) |]

        msrs.create id this.SortingWidth perm_Rss


module MsrsRandGen =

    /// Returns a string representation of the MsrsRandGen configuration.
    let toString (msrsGen: msrsRandGen) : string =
        sprintf "MsrsRandGen(RngType=%A, Width=%d, StageLength=%d, OpActionRatesArray=%s)" 
                msrsGen.RngFactory 
                (%msrsGen.SortingWidth) 
                (%msrsGen.StageLength) 
                (msrsGen.OpsGenRatesArray.toString())
