namespace GeneSort.Core

open FSharp.UMX
open System.Collections.Generic

type coverType =
    | FullCover = 0
    | VVCover = 1

type latticeLevelSetMap =
    private 
        { centerSideMap: Dictionary<latticePoint, latticePoint list>
          poleSideMap: Dictionary<latticePoint, latticePoint list>
          latticeDimension: int<latticeDimension>
          edgeLength: int<latticeDistance>
          poleSideLevel: int<latticeDistance>
          centerSideLevel: int<latticeDistance> 
          coverStrategy: coverType }

    static member create
        (latticeDimension: int<latticeDimension>)
        (edgeLength: int<latticeDistance>)
        (poleSideLevel: int<latticeDistance>)
        (centerSideLevel: int<latticeDistance>)
        (strategy: coverType)
        : latticeLevelSetMap =
        
        if %latticeDimension < 2 then
            invalidArg "latticeDimension" "Dimension must be at least 2."

        // Determine which keyMaker to use based on strategy
        let keyMaker = 
            match strategy with
            | coverType.FullCover -> LatticePoint.getLevelSet
            | coverType.VVCover   -> LatticePoint.getLevelSetVV
            | _ -> failwith "Unknown cover strategy"

        let centerSideMap = Dictionary<latticePoint, latticePoint list>()
        for point in keyMaker latticeDimension centerSideLevel edgeLength do
            centerSideMap.[point] <- []

        let poleSideMap = Dictionary<latticePoint, latticePoint list>()
        for point in keyMaker latticeDimension poleSideLevel edgeLength do
            poleSideMap.[point] <- []

        { centerSideMap = centerSideMap
          poleSideMap = poleSideMap
          latticeDimension = latticeDimension
          edgeLength = edgeLength
          poleSideLevel = poleSideLevel
          centerSideLevel = centerSideLevel
          coverStrategy = strategy }

    // --- Logic Dispatchers ---

    member private this.UnderCover(p: latticePoint) =
        match this.coverStrategy with
        | coverType.FullCover -> LatticePoint.getUnderCovers p
        | coverType.VVCover   -> LatticePoint.getUnderCoversVV p
        | _ -> [||]

    member private this.OverCover(p: latticePoint) =
        match this.coverStrategy with
        | coverType.FullCover -> LatticePoint.getOverCovers p this.EdgeLength
        | coverType.VVCover   -> LatticePoint.getOverCoversVV p this.EdgeLength
        | _ -> [||]

    // --- Public Members ---

    member this.CenterSideMap = this.centerSideMap
    member this.PoleSideMap = this.poleSideMap
    member this.CoverStrategy = this.coverStrategy
    member this.LatticeDimension = this.latticeDimension
    member this.EdgeLength = this.edgeLength    
    member this.MaxDistance with get() = this.edgeLength * this.latticeDimension
    member this.PoleSideLevel = this.poleSideLevel
    member this.CenterSideLevel = this.centerSideLevel
    
    member this.getPoleSidePointCandidates (centerPoint: latticePoint) =
        if (%this.CenterSideLevel > %this.PoleSideLevel) then
            this.UnderCover centerPoint
        else
            this.OverCover centerPoint

    member this.getCenterSidePointCandidates (polePoint: latticePoint) =
        if (%this.CenterSideLevel < %this.PoleSideLevel) then
            this.UnderCover polePoint
        else
            this.OverCover polePoint




module LatticeLevelSetMap =

    let getAllLevelSetMaps 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            (strategy: coverType)
            : latticeLevelSetMap seq = 

        let maxPathLength = %edgeLength * %latticeDimension
        let midPoint = maxPathLength / 2
        
        seq {
            // Toward Center
            for level in 1 .. midPoint do
                let levelTag = UMX.tag<latticeDistance> level
                yield latticeLevelSetMap.create 
                        latticeDimension 
                        edgeLength 
                        (levelTag - 1<latticeDistance>)
                        levelTag
                        strategy

            // Toward Pole
            for level in midPoint .. (%maxPathLength - 1) do
                let levelTag = UMX.tag<latticeDistance> level
                yield latticeLevelSetMap.create 
                        latticeDimension 
                        edgeLength 
                        (levelTag + 1<latticeDistance>)
                        levelTag
                        strategy
        }

    // Facade methods are now just passing an Enum
    let getAllLevelSetMapsStandard dim edge = 
        getAllLevelSetMaps dim edge coverType.FullCover

    let getAllLevelSetMapsVV dim edge = 
        getAllLevelSetMaps dim edge coverType.VVCover


    let setupMaps
            (llsm: latticeLevelSetMap) : unit =

        let poleSideKeys = llsm.PoleSideMap.Keys |> Seq.toArray |> Array.sort
        for lp in poleSideKeys do
            let ccpCandidates =
                llsm.getCenterSidePointCandidates lp
            let ccpWinners = 
                       ccpCandidates
                       |> Array.sort
                       |> Array.filter(fun ccp -> llsm.CenterSideMap.[ccp].Length = 0)

            let ccp = if (ccpWinners.Length = 0) then 
                          ccpCandidates.[0]
                      else
                         ccpWinners.[0]
                         // failwith "Failed to initialize level set map: no available center side points."

            llsm.PoleSideMap.[lp] <- ccp :: llsm.PoleSideMap.[lp]
            llsm.CenterSideMap.[ccp] <- lp :: llsm.CenterSideMap.[ccp]

        for lp in llsm.CenterSideMap.Keys do
            match llsm.CenterSideMap.[lp] with
            | [] ->
                let pspCandidates = llsm.getPoleSidePointCandidates lp
                if (not (llsm.PoleSideMap.ContainsKey(pspCandidates.[0]))) then
                    failwith "Failed to initialize level set map: pole side point not found in map."    

                llsm.CenterSideMap.[lp] <- [ pspCandidates.[0] ]

                llsm.PoleSideMap.[pspCandidates.[0]] <-
                    lp :: llsm.PoleSideMap.[pspCandidates.[0]]

            | _ -> ()


    let missingPoleCount
            (llsm: latticeLevelSetMap) : int =
        let poleMissing =
            llsm.PoleSideMap
            |> Seq.filter(fun kvp -> kvp.Value.Length = 0)
            |> Seq.length
        poleMissing


    let missingCenterCount
            (llsm: latticeLevelSetMap) : int =
        let centerMissing =
            llsm.CenterSideMap
            |> Seq.filter(fun kvp -> kvp.Value.Length = 0)
            |> Seq.length
        centerMissing


    let isComplete
            (llsm: latticeLevelSetMap) : bool =
        llsm.CenterSideMap
        |> Seq.forall(fun kvp -> kvp.Value.Length > 0)
        &&
        llsm.PoleSideMap
        |> Seq.forall(fun kvp -> kvp.Value.Length > 0)




    let getStats =

        let latticeDimensions = [3; 4; 8] |> List.map UMX.tag<latticeDimension>
        //let edgeLengths = [16; 24; 32; 48; 64; 96; 128] |> List.map UMX.tag<latticeDistance>
        let edgeLengths = [4; 8; 16; 24; 32;] |> List.map UMX.tag<latticeDistance>

        //printfn "SortingWidth\tDimension\tlevel\tMaxPerTuple\tLevelSet\tLevelSetVV"
        //for dim in latticeDimensions do
        //    for edgeLength in edgeLengths do
        //        let sortingWidth = (%dim * %edgeLength) |> UMX.tag<latticeDistance>
        //        let level = (%sortingWidth / 2) |> UMX.tag<latticeDistance>
        //        let levelSetPoints = LatticePoint.boundedLevelSet dim level edgeLength |> Seq.toArray
        //        let levelSetPointsVV = LatticePoint.boundedLevelSetVV dim level edgeLength |> Seq.toArray
        //        printfn "%d\t%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level) (%edgeLength) (levelSetPoints.Length) (levelSetPointsVV.Length)
        //        let level2 = (24 + (%sortingWidth / 2)) |> UMX.tag<latticeDistance>
        //        let levelSetPoints2 = LatticePoint.boundedLevelSet dim level2 edgeLength |> Seq.toArray
        //        let levelSetPointsVV2 = LatticePoint.boundedLevelSetVV dim level2 edgeLength |> Seq.toArray
        //        printfn "%d\t%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level2) (%edgeLength) (levelSetPoints2.Length) (levelSetPointsVV2.Length)

        printfn "SortingWidth\tDimension\tlevel\tMaxPerTuple\tLevelSetVV"
        for dim in latticeDimensions do
            for edgeLength in edgeLengths do
                let sortingWidth = (%dim * %edgeLength) |> UMX.tag<latticeDistance>
                let level = (%sortingWidth / 2) |> UMX.tag<latticeDistance>
                let levelSetPointsVV = LatticePoint.getLevelSetVV dim level edgeLength |> Seq.toArray
                printfn "%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level) (%edgeLength) (levelSetPointsVV.Length)
                //let level2 = (24 + (%sortingWidth / 2)) |> UMX.tag<latticeDistance>
                //let levelSetPointsVV2 = LatticePoint.boundedLevelSetVV dim level2 edgeLength |> Seq.toArray
                //printfn "%d\t%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level2) (%edgeLength) (levelSetPointsVV2.Length)
                





