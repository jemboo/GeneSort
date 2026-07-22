
namespace GeneSort.Sorting.Sorter

open FSharp.UMX
open GeneSort.Sorting

// --- 1. Stage Type ---

/// An immutable representation of a single sorting stage.
/// Equality is based on the set of CEs contained, regardless of their order.
[<CustomEquality; NoComparison>]
type stage = 
    private { 
        ces: ce array 
        hashCode: int
        sortingWidth: int<sortingWidth>
    } with

    static member create (ces: ce array) (sortingWidth: int<sortingWidth>) =
        // Sort CEs to ensure order-independent equality
        let sortedCes = ces |> Array.sortBy (fun c -> c.Low, c.Hi)
        let mutable h = 17
        for ce in sortedCes do
            h <- h * 23 + (ce |> Ce.toIndex)

        { 
            ces = sortedCes; 
            hashCode = h 
            sortingWidth = sortingWidth}

    member this.Ces = this.ces

    member this.AverageCeLength with get() =
        if this.ces.Length = 0 then 0.0
        else this.ces |> Array.averageBy (fun c -> float c.Length)

    member this.SortingWidth with get() = this.sortingWidth

    override this.GetHashCode() = this.hashCode

    override this.Equals(obj) =
        match obj with
        | :? stage as other ->
            this.hashCode = other.hashCode &&
            this.ces.Length = other.ces.Length &&
            Array.forall2 (fun (a: ce) (b: ce) -> a.Low = b.Low && a.Hi = b.Hi) this.ces other.ces
        | _ -> false


module Stage =

   let reflect (stg: stage) : stage =
       stage.create (stg.Ces |> Array.map(Ce.reflect stg.SortingWidth)) stg.SortingWidth

   let isReflectionSymmetric (stg: stage) : bool =
        stg = (reflect stg)



[<CustomEquality; NoComparison>]
type stageSequence = 
    private { 
        sortingWidth: int<sortingWidth>
        stages: stage array 
        hashCode: int 
    } with

    static member create (stages: stage array) (sortingWidth:int<sortingWidth>) =
        let mutable h = 17
        for stage in stages do
            h <- h * 23 + stage.GetHashCode()

        { 
            sortingWidth = sortingWidth
            stages = stages 
            hashCode = h
        }

    member this.SortingWidth = this.sortingWidth
    member this.Stages = this.stages
    member this.StageLength = this.stages.Length |> UMX.tag<stageLength>

    override this.GetHashCode() : int = this.hashCode

    override this.Equals(obj) =
        match obj with
        | :? stageSequence as other ->
            // Order matters here: we check length then verify each stage matches in sequence
            this.hashCode = other.hashCode &&
            this.sortingWidth = other.sortingWidth &&
            this.stages.Length = other.stages.Length &&
            Array.forall2 (fun (s1: stage) (s2: stage) -> s1.Equals(s2)) this.stages other.stages
        | _ -> false