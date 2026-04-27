namespace GeneSort.Model.Sorting.V1
open FSharp.UMX

open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1.Simple


type sorterModelSet =
      private
        { 
          id : Guid<sorterModelSetId>
          sortingMap : Map<Guid<sorterModelId>, sorterModel>
        }
        with
        static member create 
            (id : Guid<sorterModelSetId>) 
            (models : sorterModel[]) : sorterModelSet =

            let modelMap = 
                models 
                |> Array.map (fun sm -> (SorterModel.getId sm, sm))
                |> Map.ofArray

            { id = id; sortingMap = modelMap  }

        member this.Count with get() = this.sortingMap.Count
        member this.Id with get() = this.id
        member this.SorterModels with get() = this.sortingMap |> Map.toArray |> Array.map snd


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
                      (modelSet: sorterModelSet) : sorterSet =
        let sorters = 
            modelSet.SorterModels 
            |> Array.map (fun sm -> SorterModel.makeSorter sm)
        sorterSet.create id sorters

