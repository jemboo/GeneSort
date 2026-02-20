namespace GeneSort.Model.Sorting.ModelParams

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting

type mcseRandMutateParams = 
    private 
        { 
          id: Guid<sortingParamsId>
          rngType: rngType
          indelRatesArray: indelRatesArray
          excludeSelfCe: bool }
    static member create
            (rngType: rngType)
            (indelRatesArray: indelRatesArray)
            (excludeSelfCe: bool): mcseRandMutateParams = 
        let id =
            [
                rngType :> obj
                indelRatesArray :> obj
                excludeSelfCe :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingParamsId>
        {
            id = id
            rngType = rngType
            indelRatesArray = indelRatesArray
            excludeSelfCe = excludeSelfCe
        }
    
    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.IndelRatesArray with get () = this.indelRatesArray
    member this.ExcludeSelfCe with get () = this.excludeSelfCe


type mssiRandMutateParams = 
    private 
        { 
          id: Guid<sortingParamsId>
          rngType: rngType
          opActionRatesArray: opActionRatesArray
         }
    static member create
            (rngType: rngType)
            (opActionRatesArray: opActionRatesArray): mssiRandMutateParams = 
        let id =
            [
                rngType :> obj
                opActionRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingParamsId>
        {
            id = id
            rngType = rngType
            opActionRatesArray = opActionRatesArray
        }

    member this.Id with get () = this.id    
    member this.RngType with get () = this.rngType
    member this.OpActionRatesArray with get () = this.opActionRatesArray


type msrsRandMutateParams = 
    private 
        { 
          id: Guid<sortingParamsId>
          rngType: rngType
          opsActionRatesArray: opsActionRatesArray
         }
    static member create
            (rngType: rngType)
            (opsActionRatesArray: opsActionRatesArray): msrsRandMutateParams = 
        let id =
            [
                rngType :> obj
                opsActionRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingParamsId>
        {
            id = id
            rngType = rngType
            opsActionRatesArray = opsActionRatesArray
        }
        
    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.OpsActionRatesArray with get () = this.opsActionRatesArray


type msuf4RandMutateParams = 
    private 
        {
          id: Guid<sortingParamsId>
          rngType: rngType
          uf4MutationRatesArray: uf4MutationRatesArray
         }
    static member create
            (rngType: rngType)
            (uf4MutationRatesArray: uf4MutationRatesArray): msuf4RandMutateParams = 
        let id =
            [
                rngType :> obj
                uf4MutationRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingParamsId>
        {
            id = id
            rngType = rngType
            uf4MutationRatesArray = uf4MutationRatesArray
        }
        
    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.Uf4MutationRatesArray with get () = this.uf4MutationRatesArray 


type msuf6RandMutateParams = 
    private 
        {
          id: Guid<sortingParamsId>
          uf6MutationRatesArray: uf6MutationRatesArray
          rngType: rngType
         }
    static member create
            (rngType: rngType)
            (uf6MutationRatesArray: uf6MutationRatesArray): msuf6RandMutateParams = 
        let id =
            [
                rngType :> obj
                uf6MutationRatesArray :> obj
            ] |> GuidUtils.guidFromObjs |> UMX.tag<sortingParamsId>
        {
            id = id
            rngType = rngType
            uf6MutationRatesArray = uf6MutationRatesArray
        }

    member this.Id with get () = this.id
    member this.RngType with get () = this.rngType
    member this.Uf6MutationRatesArray with get () = this.uf6MutationRatesArray


type sorterModelMutateParams = 
    | McseRandMutateParams of mcseRandMutateParams
    | MssiRandMutateParams of mssiRandMutateParams
    | MsrsRandMutateParams of msrsRandMutateParams
    | Msuf4RandMutateParams of msuf4RandMutateParams
    | Msuf6RandMutateParams of msuf6RandMutateParams


module SorterModelMutateParams = 
   
    let getId (sorterModelMutateParams: sorterModelMutateParams) : Guid<sortingParamsId> =

        match sorterModelMutateParams with
        | McseRandMutateParams prams -> prams.Id
        | MssiRandMutateParams prams -> prams.Id
        | MsrsRandMutateParams prams -> prams.Id
        | Msuf4RandMutateParams prams -> prams.Id
        | Msuf6RandMutateParams prams -> prams.Id

    let getRngType (sorterModelMutateParams: sorterModelMutateParams) : rngType =
        match sorterModelMutateParams with
        | McseRandMutateParams prams -> prams.RngType
        | MssiRandMutateParams prams -> prams.RngType
        | MsrsRandMutateParams prams -> prams.RngType
        | Msuf4RandMutateParams prams -> prams.RngType
        | Msuf6RandMutateParams prams -> prams.RngType
        
    let makeUniformMsceMutator 
                (ceLength: int<ceLength>)
                (mutationRate: float<mutationRate>)
                (insertionRate: float<mutationRate>)
                (deletionRate: float<mutationRate>)
                (rngType: rngType) 
                (excludeSelfCe: bool option) : sorterModelMutateParams =

        let indelRates = indelRates.create (%mutationRate, %insertionRate, %deletionRate)
        let indelRatesArray = IndelRatesArray.createUniform %ceLength indelRates
        let mcseParams = mcseRandMutateParams.create rngType indelRatesArray (excludeSelfCe |> Option.defaultValue true)
        McseRandMutateParams mcseParams


    let makeUniformMssiMutator 
                (stageLength: int<stageLength>)
                (orthoRate: float<mutationRate>)
                (paraRate: float<mutationRate>)
                (rngType: rngType) : sorterModelMutateParams =
                let opActionRatesArray = OpActionRatesArray.createUniform %stageLength %orthoRate %paraRate
                let mssiParams = mssiRandMutateParams.create rngType opActionRatesArray
                MssiRandMutateParams mssiParams


    let makeUniformMsrsMutator 
                (stageLength: int<stageLength>)
                (orthoRate: float<mutationRate>)
                (paraRate: float<mutationRate>)
                (selfSymRate: float<mutationRate>)
                (rngType: rngType) : sorterModelMutateParams =
                let opsActionRatesArray = OpsActionRatesArray.createUniform %stageLength %orthoRate %paraRate %selfSymRate
                let msrsParams = msrsRandMutateParams.create rngType opsActionRatesArray
                MsrsRandMutateParams msrsParams


    let makeUniformMsuf4Mutator 
                (stageLength: int<stageLength>)
                (sortingWidth: int<sortingWidth>)
                (seed4MutationRates: float<mutationRate>)
                (twoOrbitMutationRate: float<mutationRate>)
                (rngType: rngType) : sorterModelMutateParams =
                let uf4MutationRates = Uf4MutationRates.makeUniform %sortingWidth %seed4MutationRates %twoOrbitMutationRate
                let uf4MutationRatesArray = Uf4MutationRatesArray.createUniform %stageLength %sortingWidth uf4MutationRates
                let msuf4Params = msuf4RandMutateParams.create rngType uf4MutationRatesArray
                Msuf4RandMutateParams msuf4Params


    let makeUniformMsuf6Mutator 
                (stageLength: int<stageLength>)
                (sortingWidth: int<sortingWidth>)
                (seed6MutationRates: float<mutationRate>)
                (twoOrbitMutationRate: float<mutationRate>)
                (rngType: rngType) : sorterModelMutateParams =
                let uf6MutationRates = Uf6MutationRates.makeUniform %sortingWidth %seed6MutationRates %twoOrbitMutationRate
                let uf6MutationRatesArray = Uf6MutationRatesArray.createUniform %stageLength %sortingWidth uf6MutationRates
                let msuf6Params = msuf6RandMutateParams.create rngType uf6MutationRatesArray
                Msuf6RandMutateParams msuf6Params


    let makeUniformMutatorForSorterModel 
                (sorterModelType: sorterModelType)
                (stageLength: int<stageLength>)
                (sortingWidth: int<sortingWidth>)
                (mutationRate: float<mutationRate>)
                (rngType: rngType) : sorterModelMutateParams =

        let insertionRate = mutationRate
        let deletionRate = mutationRate
        let orthoRate = mutationRate
        let paraRate = mutationRate
        let selfSymRate = mutationRate
        let twoOrbitMutationRate = mutationRate
        let seed4MutationRates = mutationRate
        let seed6MutationRates = mutationRate

        match sorterModelType with
        | sorterModelType.Msce -> 
                let ceLenth = stageLength |> StageLength.toCeLength %sortingWidth
                makeUniformMsceMutator ceLenth mutationRate insertionRate deletionRate rngType None
        | sorterModelType.Mssi -> makeUniformMssiMutator stageLength orthoRate paraRate rngType
        | sorterModelType.Msrs -> makeUniformMsrsMutator stageLength orthoRate paraRate selfSymRate rngType
        | sorterModelType.Msuf4 -> makeUniformMsuf4Mutator stageLength sortingWidth seed4MutationRates twoOrbitMutationRate rngType
        | sorterModelType.Msuf6 -> makeUniformMsuf6Mutator stageLength sortingWidth seed6MutationRates twoOrbitMutationRate rngType