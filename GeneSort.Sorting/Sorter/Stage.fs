
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
        channelCount: int<channelCount>
    } with

    static member create (ces: ce array) (channelCount: int<channelCount>) =
        // Sort CEs to ensure order-independent equality
        let sortedCes = ces |> Array.sortBy (fun c -> (int c.LowChannel), (int c.HighChannel))
        let mutable h = 17
        for ce in sortedCes do
            h <- h * 23 + (ce |> Ce.toIndex)

        { 
            ces = sortedCes; 
            hashCode = h 
            channelCount = channelCount}

    member this.Ces = this.ces

    member this.AverageCeLength with get() =
        if this.ces.Length = 0 then 0.0
        else this.ces |> Array.averageBy (fun c -> float c.ChannelSpan)


    member this.GetReflectiveCount() : int<reflectiveCount>  =
            Ce.countReflectiveOrReflected this.ChannelCount this.ces

    member this.ChannelCount with get() = this.channelCount

    override this.GetHashCode() = this.hashCode

    override this.Equals(obj) =
        match obj with
        | :? stage as other ->
            this.hashCode = other.hashCode &&
            this.ces.Length = other.ces.Length &&
            Array.forall2 (fun (a: ce) (b: ce) -> (int a.LowChannel) = (int b.LowChannel) && (int a.HighChannel) = (int b.HighChannel)) this.ces other.ces
        | _ -> false


module Stage =

   let reflect (stg: stage) : stage =
       stage.create (stg.Ces |> Array.map(Ce.reflect stg.ChannelCount)) stg.ChannelCount

   let isReflectionSymmetric (stg: stage) : bool =
        stg = (reflect stg)



[<CustomEquality; NoComparison>]
type stageSequence = 
    private { 
        channelCount: int<channelCount>
        stages: stage array 
        hashCode: int 
    } with

    static member create (stages: stage array) (channelCount:int<channelCount>) =
        let mutable h = 17
        for stage in stages do
            h <- h * 23 + stage.GetHashCode()

        { 
            channelCount = channelCount
            stages = stages 
            hashCode = h
        }
        
    member this.GetReflectiveCount() : int<reflectiveCount>  =
        this.stages |> Array.sumBy(fun stg -> %stg.GetReflectiveCount())
        
    member this.ChannelCount = this.channelCount
    member this.Stages = this.stages
    member this.StageLength = this.stages.Length |> UMX.tag<stageLength>

    override this.GetHashCode() : int = this.hashCode

    override this.Equals(obj) =
        match obj with
        | :? stageSequence as other ->
            // Order matters here: we check length then verify each stage matches in sequence
            this.hashCode = other.hashCode &&
            this.channelCount = other.channelCount &&
            this.stages.Length = other.stages.Length &&
            Array.forall2 (fun (s1: stage) (s2: stage) -> s1.Equals(s2)) this.stages other.stages
        | _ -> false
