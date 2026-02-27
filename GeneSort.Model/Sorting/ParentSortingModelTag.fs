namespace GeneSort.Model.Sorting

open FSharp.UMX
open System


type sortingGrandParentId = Guid<sortingId>


// used to track a sorter back to it's parent sorting, and it gives it's position 
// within it's family, and then back to the grandparent sorting.
type parentSortingTag = sortingGrandParentId * sortingTag

module ParentSortingTag =

   let create (grandParentId: Guid) (parentTag: sortingTag) : parentSortingTag =
        (grandParentId |> UMX.tag<sortingId>, parentTag)

   
   let getGrandParentId (tag: parentSortingTag) : sortingGrandParentId =
        let (grandParentId, _) = tag
        grandParentId


   let getParentTag (tag: parentSortingTag) : sortingTag =
        let (_, parentTag) = tag
        parentTag
