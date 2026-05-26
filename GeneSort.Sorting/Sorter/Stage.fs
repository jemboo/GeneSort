
namespace GeneSort.Sorting.Sorter

open System
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
    } with

    static member create (ces: ce array) =
        // Sort CEs to ensure order-independent equality
        let sortedCes = ces |> Array.sortBy (fun c -> c.Low, c.Hi)
        
        // Pre-calculate hash code using the sorted content
        let h = sortedCes |> Array.fold (fun acc c -> HashCode.Combine(acc, c.Low, c.Hi)) 0
        { ces = sortedCes; hashCode = h }

    member this.Ces = this.ces

    member this.AverageCeLength with get() =
        if this.ces.Length = 0 then 0.0
        else this.ces |> Array.averageBy (fun c -> float c.Length)

    override this.GetHashCode() = this.hashCode

    override this.Equals(obj) =
        match obj with
        | :? stage as other ->
            this.hashCode = other.hashCode &&
            this.ces.Length = other.ces.Length &&
            Array.forall2 (fun (a: ce) (b: ce) -> a.Low = b.Low && a.Hi = b.Hi) this.ces other.ces
        | _ -> false



[<CustomEquality; NoComparison>]
type stageSequence = 
    private { 
        sortingWidth: int<sortingWidth>
        stages: stage array 
        hashCode: int 
    } with

    static member create (stages: stage array) (sortingWidth:int<sortingWidth>) =
        // Compute sequence hash based on the ordered hashes of the stages
        let h = stages |> Array.fold (fun acc s -> HashCode.Combine(acc, s.GetHashCode())) 0
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