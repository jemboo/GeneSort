namespace GeneSort.Model.Sorting.V1.Simple.Rs

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1

/// Represents a configuration for generating random Msrs instances with specified mutation rates.
/// Combines random generation and mutation probabilities for Perm_Rs instances.
[<Struct; CustomEquality; NoComparison>]
type msrsRandGen = 
    private 
        { 
              id : Guid<sorterModelGenId>
              rngFactory: rngFactory
              opsGenRates: opsGenRates
              sortingWidth: int<sortingWidth> 
              stageLength: int<stageLength>
        } 
    static member create 
            (rngFactory: rngFactory)
            (sortingWidth: int<sortingWidth>)
            (opsGenRates: opsGenRates)
            (stageLength: int<stageLength>)
            : msrsRandGen =

        if %sortingWidth < 2 then
            failwith $"SortingWidth must be at least 2, got {%sortingWidth}"
        let id =
            [
                "msrsRandGen" :> obj
                rngFactory :> obj
                sortingWidth :> obj
                stageLength :> obj
                opsGenRates.GetHashCode() :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelGenId>

        {
            id = id
            sortingWidth = sortingWidth
            opsGenRates = opsGenRates
            rngFactory = rngFactory
            stageLength = stageLength
        }
        
    member this.Id with get () = this.id
    member this.CeLength with get () = (this.SortingWidth * %this.StageLength / 2) |> UMX.tag<ceLength>
    member this.RngFactory with get () = this.rngFactory
    member this.OpsGenRates with get () = this.opsGenRates
    member this.SortingWidth with get () = this.sortingWidth
    member this.StageLength with get () = this.stageLength

    override this.Equals(obj) = 
        match obj with
        | :? msrsRandGen as other -> 
            this.id = other.id
        | _ -> false

    override this.GetHashCode() = 
        hash (this.rngFactory, this.opsGenRates, this.sortingWidth, this.stageLength)

    interface IEquatable<msrsRandGen> with
        member this.Equals(other) = 
            this.Id = other.Id

        
    member this.MakeSorterModelId (index: int) : Guid<sorterModelId> =
        CommonGen.makeSorterModelId this.Id index


    member this.MakeSorterModelFromId (id: Guid<sorterModelId>) : msrs =
        let rng = this.RngFactory.Create %id
        let stageLength = %this.StageLength
        let sortingWidth = %this.SortingWidth
        let opsGenRates = this.OpsGenRates
        let perm_Rss =
            [| for dex in 0 .. (stageLength - 1) ->
                Perm_RsOps.makeRandomPerm_Rs
                    (rng.NextIndex)
                    (rng.NextFloat)
                    (opsGenRates)
                    (sortingWidth) |]

        msrs.create id this.SortingWidth perm_Rss


    member this.MakeSorterModelFromIndex (index: int) : msrs =
        let id = this.MakeSorterModelId index
        this.MakeSorterModelFromId id


module MsrsRandGen =

    /// Returns a string representation of the MsrsRandGen configuration.
    let toString (msrsGen: msrsRandGen) : string =
        sprintf "MsrsRandGen(RngType=%A, Width=%d, StageLength=%d, OpActionRatesArray=%s)" 
                msrsGen.RngFactory 
                (%msrsGen.SortingWidth) 
                (%msrsGen.StageLength) 
                (msrsGen.OpsGenRates.toString())

