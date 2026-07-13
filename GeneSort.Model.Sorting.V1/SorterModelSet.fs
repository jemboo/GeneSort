namespace GeneSort.Model.Sorting.V1

open System
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1.Simple


type sorterModelSet =
      private
        { 
          id: Guid<sorterModelSetId>
          sortingMap: Map<Guid<sorterModelId>, sorterModel>
          rawCeLength: int<ceLength>
        }
        with
        static member create 
            (id : Guid<sorterModelSetId>) 
            (models : sorterModel[]) 
            (maxCeLength: int<ceLength>): sorterModelSet =
            let modelMap = 
                models 
                |> Array.map (fun sm -> (SorterModel.getId sm, sm))
                |> Map.ofArray

            { id = id; sortingMap = modelMap; rawCeLength = maxCeLength }

        member this.Count with get() = this.sortingMap.Count
        member this.Id with get() = this.id
        member this.RawCeLength with get() = this.rawCeLength
        member this.SorterModels with get() : sorterModel array = 
                    this.sortingMap |> Map.toArray |> Array.map snd
        static member empty: sorterModelSet =
            { id = Guid.Empty |> UMX.tag; sortingMap = Map.empty; rawCeLength = 0<ceLength> }


module SorterModelSet =

    let getSortingWidth (model: sorterModel) : int<sortingWidth> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getSortingWidth
        | Unknown -> failwith "Unknown sorterModel"


    let getStageLength (model: sorterModel) : int<stageLength> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getStageLength
        | Unknown -> failwith "Unknown sorterModel"

    
    let getId (model: sorterModel) : Guid<sorterModelId> =
        match model with
        | Simple sms -> sms |> SimpleSorterModel.getId
        | Unknown -> failwith "Unknown sorterModel"


    let makeSorterSet (id:Guid<sorterSetId>) 
                      (maxCeCount: int<ceLength> option)
                      (sorterModelSet: sorterModelSet) : sorterSet =
        let sorters = 
            sorterModelSet.SorterModels 
            |> Array.map (fun sm -> SorterModel.makeSorter sm maxCeCount)
        sorterSet.create id sorters

