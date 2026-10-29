
namespace GeneSort.Sorting.Sorter

open FSharp.UMX
open GeneSort.Sorting


type stageBuilder = 
        private { 
                    channelCount: int<channelCount>;
                    ces: ResizeArray<ce>;  
                    occupied: bool[] 
                } with

        member this.ChannelCount with get() = this.channelCount

        member this.Ces with get() = this.ces.ToArray()

        member this.CeCount with get() = this.ces.Count

        member this.IsOccupied(index: int) = 
            if index < 0 || index >= %this.channelCount then
                invalidArg "index" $"Index {index} is out of bounds for Ce array of length {this.ces.Count}."
            this.occupied.[index]

        member this.AddCe(ce: ce) =
            if this.occupied.[(int ce.LowChannel)] then
                invalidArg "ce" $"Cannot add CE {ce} because position {(int ce.LowChannel)} is already occupied."
            if this.occupied.[(int ce.HighChannel)] then
                invalidArg "ce" $"Cannot add CE {ce} because position {(int ce.HighChannel)} is already occupied."
            this.ces.Add(ce)
            this.occupied.[(int ce.LowChannel)] <- true
            this.occupied.[(int ce.HighChannel)] <- true

        member this.CanAddCe(ce: ce) =
            not (this.occupied.[(int ce.LowChannel)] || this.occupied.[(int ce.HighChannel)])

// Core module for Stage operations
module StageBuilder = ()



type stageBuilderSequence = 
        private { 
                    channelCount: int<channelCount>;
                    stageBuilders: ResizeArray<stageBuilder>;  
                } with

        static member create (channelCount: int<channelCount>) : stageBuilderSequence =
            if channelCount <= 1<channelCount> then
                invalidArg "channelCount" "Channel count must be greater than 1."
            { channelCount = channelCount; stageBuilders = ResizeArray<stageBuilder>() }

        member this.ChannelCount with get() = this.channelCount

        member this.StageBuilders with get() = this.stageBuilders.ToArray()

        member this.StageLength with get() = this.stageBuilders.Count |> UMX.tag<stageLength>

        member this.AddCe (ce: ce) :  unit =
            if (int ce.LowChannel) >= (this.channelCount |> int) || (int ce.HighChannel) >= (this.channelCount |> int) then
                invalidArg "ce" $"CE {ce} has indices out of bounds for channel count {this.channelCount}."

            // Find the last stage that CANNOT accept the CE
            let firstBlockedIndex = 
                this.stageBuilders 
                |> Seq.tryFindIndexBack (fun sb -> not (sb.CanAddCe(ce)))

            // If a stage is blocked at index `i`, the target is the one right after it (`i + 1`)
            // If no stages are blocked, it means every stage from the end back to 0 can accept it, 
            // so the earliest available stage is index 0.
            let targetStageIndex = 
                match firstBlockedIndex with
                | Some i -> i + 1
                | None   -> 0

            // If the calculated index is beyond our existing stages, it means we need a new stage
            if targetStageIndex >= this.stageBuilders.Count then
                let newStage = 
                    { 
                        channelCount = this.channelCount
                        ces = ResizeArray<ce>()
                        occupied = Array.create (this.channelCount |> int) false
                    }
                newStage.AddCe(ce)
                this.stageBuilders.Add(newStage)
            else
                // Add to the earliest stage that can accept the CE
                this.stageBuilders.[targetStageIndex].AddCe(ce)

        /// Adds a CE and returns the index of the stage it was placed into.
        member this.AddCeIndexed (ce: ce) : int =
            if (int ce.LowChannel) >= (this.channelCount |> int) || (int ce.HighChannel) >= (this.channelCount |> int) then
                invalidArg "ce" $"CE {ce} has indices out of bounds for channel count {this.channelCount}."

            let firstBlockedIndex = 
                this.stageBuilders 
                |> Seq.tryFindIndexBack (fun sb -> not (sb.CanAddCe(ce)))

            let targetStageIndex = 
                match firstBlockedIndex with
                | Some i -> i + 1
                | None   -> 0

            if targetStageIndex >= this.stageBuilders.Count then
                let newStage = 
                    { 
                        channelCount = this.channelCount
                        ces = ResizeArray<ce>()
                        occupied = Array.create (this.channelCount |> int) false
                    }
                newStage.AddCe(ce)
                this.stageBuilders.Add(newStage)
            else
                this.stageBuilders.[targetStageIndex].AddCe(ce)

            targetStageIndex



type CeDisplacement = {
    Ce: ce
    /// Original 0-based index in the input array
    InputIndex: int
    /// How many stages back (left) this CE moved due to packing (0 = stayed in earliest natural stage)
    StagesMovedBack: int
}



module StageBuilderSequence =

    let fromCes (channelCount : int<channelCount>) (ces: ce array) : stageBuilderSequence =
        let stageBuilderSeq = stageBuilderSequence.create(channelCount)
        ces |> Array.iter stageBuilderSeq.AddCe
        stageBuilderSeq


    let toStageSequence (channelCount: int<channelCount>) (ces: ce array) : stageSequence =
        let stageBuilderSeq = fromCes channelCount ces
        let stages = stageBuilderSeq.StageBuilders |> Array.map(fun st -> stage.create st.Ces channelCount)
        stageSequence.create stages channelCount

    /// From Grok
    /// Returns (stageSequence, movedStages)
    /// 
    /// movedStages is array-of-arrays with the same shape as stageSeq.Stages:
    /// movedStages.[stageIndex].[ceIndexInStage] = how many stages back this CE was moved
    /// when the greedy algorithm placed it.
    /// Returns (stageSequence, movedStages)
    /// 
    /// movedStages : int[][] matches the structure of stageSeq.Stages
    /// movedStages.[stageIdx].[posInStage] = how many stages back this CE was moved.
    let toStageSequenceWithMoveInfo (channelCount: int<channelCount>) (ces: ce array) =
        let stageBuilderSeq = stageBuilderSequence.create(channelCount)
        
        // Per-stage lists of (stagesMoved, originalIndex)
        let moveTracker = ResizeArray<ResizeArray<int * int>>()

        ces |> Array.iteri (fun origIdx ce ->
            let currentCount = stageBuilderSeq.stageBuilders.Count

            let firstBlockedIndex =
                stageBuilderSeq.stageBuilders
                |> Seq.tryFindIndexBack (fun sb -> not (sb.CanAddCe(ce)))

            let targetIdx =
                match firstBlockedIndex with
                | Some i -> i + 1
                | None -> 0

            // === Updated movement calculation ===
            let stagesMoved =
                if targetIdx >= currentCount then
                    0                                   // Created a new stage
                else
                    currentCount - targetIdx - 1        // How many stages it jumped forward

            // Record the move info
            while moveTracker.Count <= targetIdx do
                moveTracker.Add(ResizeArray<int * int>())
            moveTracker.[targetIdx].Add(stagesMoved, origIdx)

            // Add the CE to the builder (existing logic)
            if targetIdx >= currentCount then
                let newSb =
                    {
                        channelCount = channelCount
                        ces = ResizeArray<ce>()
                        occupied = Array.create (channelCount |> int) false
                    }
                newSb.AddCe(ce)
                stageBuilderSeq.stageBuilders.Add(newSb)
            else
                stageBuilderSeq.stageBuilders.[targetIdx].AddCe(ce)
        )

        // Build final immutable stageSequence
        let stages = 
            stageBuilderSeq.StageBuilders 
            |> Array.map (fun sb -> stage.create sb.Ces channelCount)

        let stageSeq = stageSequence.create stages channelCount

        // Build movedStages array-of-arrays aligned with final sorted stages
        let movedStages =
            stageBuilderSeq.StageBuilders
            |> Array.mapi (fun sIdx sb ->
                let movesWithOrig = moveTracker.[sIdx]
                
                // Sort in the same order as the stage (by Low, Hi)
                let paired =
                    Array.zip sb.Ces (movesWithOrig |> Seq.toArray)
                    |> Array.sortBy (fun (c, _) -> (int c.LowChannel), (int c.HighChannel))
                
                paired |> Array.map (fun (_, (moved, _)) -> moved)
            )

        stageSeq, movedStages


    /// From Gemini
    /// Same as toStageSequence, but also returns, for each stage in the result, an array
    /// giving how many stages back each CE in that stage moved relative to its "naive"
    /// position — i.e. the stage it would occupy if CEs were added one-per-stage in input
    /// order. A CE added at input index `i` can only ever land in stage `i` or earlier
    /// (AddCe never pushes a CE later than the number of stages that exist so far), so this
    /// value is always >= 0. The inner arrays are ordered to match each stage's `.Ces`
    /// (sorted by Low, Hi), so movedBack.[s].[k] corresponds to stages.[s].Ces.[k].
    let toStageSequenceWithMovement (channelCount: int<channelCount>) (ces: ce array)
        : stageSequence * int array array =

        let stageBuilderSeq = stageBuilderSequence.create(channelCount)

        // For each input CE, record which stage it actually landed in.
        let stageIndexOfOriginal = Array.zeroCreate<int> ces.Length
        ces |> Array.iteri (fun i ce ->
            stageIndexOfOriginal.[i] <- stageBuilderSeq.AddCeIndexed ce)

        let stages = stageBuilderSeq.StageBuilders |> Array.map (fun st -> stage.create st.Ces channelCount)
        let stageSeq = stageSequence.create stages channelCount

        // Group original indices by the stage they landed in.
        let originalIndicesByStage = Array.init stages.Length (fun _ -> ResizeArray<int>())
        stageIndexOfOriginal |> Array.iteri (fun origIdx stageIdx ->
            originalIndicesByStage.[stageIdx].Add origIdx)

        // Within each stage, order the original indices the same way stage.create
        // orders the CEs themselves (by Low, Hi), then compute how far back each moved.
        let movedBack =
            originalIndicesByStage
            |> Array.mapi (fun stageIdx origIndices ->
                origIndices.ToArray()
                |> Array.sortBy (fun i -> let c = ces.[i] in (int c.LowChannel), (int c.HighChannel))
                |> Array.map (fun origIdx -> origIdx - stageIdx))

        stageSeq, movedBack



    /// From Gemini
    /// Converts input CEs into a stageSequence and returns a matching array of arrays 
    /// describing how many stages back each CE moved during stage packing.
    let toStageSequenceWithDisplacements 
        (channelCount: int<channelCount>)
        (ces: ce array) 
        : stageSequence * CeDisplacement array array =
        
        let stageBuilderSeq = stageBuilderSequence.create(channelCount)
        
        // Track the target stage each input CE ended up in during packing
        let placements = ResizeArray<int * ce>() // (TargetStageIndex, Ce)

        for i = 0 to ces.Length - 1 do
            let c = ces.[i]
            
            // Replicate packing lookup to catch target stage index
            let firstBlockedIndex = 
                stageBuilderSeq.StageBuilders 
                |> Seq.tryFindIndexBack (fun sb -> not (sb.CanAddCe(c)))

            let targetStageIndex = 
                match firstBlockedIndex with
                | Some idx -> idx + 1
                | None     -> 0

            stageBuilderSeq.AddCe(c)
            placements.Add(targetStageIndex, c)

        // Generate final canonical stages and map displacements
        // We track the highest stage index seen so far to measure relative leftward displacement
        let mutable maxStageSeen = 0

        let displacementsPerStage = 
            stageBuilderSeq.StageBuilders
            |> Array.mapi (fun stageIdx sb ->
                // Sort CEs to match `stage.create` canonical ordering
                sb.Ces
                |> Array.sortBy (fun c -> (int c.LowChannel), (int c.HighChannel))
                |> Array.map (fun c ->
                    // Find original placement info
                    let inputIdx = placements.FindIndex(fun (_, originalCe) -> originalCe = c)
                    let targetIdx, _ = placements.[inputIdx]
                    
                    // Displacement = how far behind the front line of stage construction this CE landed
                    maxStageSeen <- max maxStageSeen targetIdx
                    let movedBack = maxStageSeen - targetIdx

                    {
                        Ce = c
                        InputIndex = inputIdx
                        StagesMovedBack = movedBack
                    }
                )
            )

        let sequence = 
            stageBuilderSeq.StageBuilders 
            |> Array.map (fun sb -> stage.create sb.Ces channelCount)
            |> fun stages -> stageSequence.create stages channelCount

        sequence, displacementsPerStage

