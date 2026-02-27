namespace GeneSort.Model.Sorting

open FSharp.UMX
open System


type sortingModelGrandParentId = Guid<sortingModelId>


// used to track a sorter back to it's parent sorting, and it gives it's position 
// within it's family, and then back to the grandparent sorting.
type parentSortingModelTag = sortingModelGrandParentId * sortingModelTag

module ParentSortingModelTag =

   let create (grandParentId: Guid) (parentTag: sortingModelTag) : parentSortingModelTag =
        (grandParentId |> UMX.tag<sortingModelId>, parentTag)

   
   let getGrandParentId (tag: parentSortingModelTag) : sortingModelGrandParentId =
        let (grandParentId, _) = tag
        grandParentId


   let getParentTag (tag: parentSortingModelTag) : sortingModelTag =
        let (_, parentTag) = tag
        parentTag
