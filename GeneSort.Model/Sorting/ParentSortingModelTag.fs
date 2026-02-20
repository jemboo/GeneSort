namespace GeneSort.Model.Sorting

open FSharp.UMX
open System


type sortingModelGrandParentId = Guid<sortingModelID>


// used to track a sorter back to it's parent sortingModel, and it gives it's position 
// within it's family, and then back to the grandparent sortingModel.
type parentSortingModelTag = sortingModelGrandParentId * sortingModelTag

module ParentSortingModelTag =

   let create (grandParentId: Guid) (parentTag: sortingModelTag) : parentSortingModelTag =
        (grandParentId |> UMX.tag<sortingModelID>, parentTag)

   
   let getGrandParentId (tag: parentSortingModelTag) : sortingModelGrandParentId =
        let (grandParentId, _) = tag
        grandParentId


   let getParentTag (tag: parentSortingModelTag) : sortingModelTag =
        let (_, parentTag) = tag
        parentTag
