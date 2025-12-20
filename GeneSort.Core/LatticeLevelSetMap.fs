namespace GeneSort.Core
open FSharp.UMX
open System.Collections.Generic


type latticeLevelSetMap =
    private 
        { centerSideMap: Dictionary<latticePoint, latticePoint list>
          poleSideMap: Dictionary<latticePoint, latticePoint list>
          latticeDimension: int<latticeDimension>
          edgeLength: int<latticeDistance>
          poleSideLevel: int<latticeDistance>
          centerSideLevel: int<latticeDistance> 
          overCoverMap: latticePoint -> int<latticeDistance> -> latticePoint []
          underCoverMap: latticePoint -> latticePoint[] }

          static member create
            (latticeDimension: int<latticeDimension>)
            (edgeLength: int<latticeDistance>)
            (poleSideLevel: int<latticeDistance>)
            (centerSideLevel: int<latticeDistance>)
            (keyMaker: int<latticeDimension> -> int<latticeDistance> -> int<latticeDistance> -> latticePoint seq)
            (overCoverMap: latticePoint -> int<latticeDistance> -> latticePoint [])
            (underCoverMap: latticePoint -> latticePoint [])
            : latticeLevelSetMap =
            if %latticeDimension < 2 then
                invalidArg "latticeDimension" "latticeDimension length must be at least 2."

            let centerSideMap = Dictionary<latticePoint,latticePoint list>()
            // Initialize centerSideMap with keys from latticeLevelSet
            for point in keyMaker latticeDimension centerSideLevel edgeLength do
                centerSideMap.[point] <- []

            let poleSideMap = Dictionary<latticePoint, latticePoint list>()
            // Initialize poleSideMap with keys from latticeLevelSet
            for point in keyMaker latticeDimension poleSideLevel edgeLength do
                poleSideMap.[point] <- []

            { 
              centerSideMap = centerSideMap
              poleSideMap = poleSideMap
              latticeDimension = latticeDimension
              edgeLength = edgeLength
              poleSideLevel = poleSideLevel
              centerSideLevel = centerSideLevel  
              overCoverMap = overCoverMap
              underCoverMap = underCoverMap 
            }

          member this.CenterSideMap with get() = this.centerSideMap
          member this.PoleSideMap with get() = this.poleSideMap
          member this.LatticeDimension with get() = this.latticeDimension
          member this.EdgeLength with get() = this.edgeLength
          member this.MaxDistance with get() = this.edgeLength * this.latticeDimension
          member this.PoleSideLevel with get() = this.poleSideLevel
          member this.CenterSideLevel with get() = this.centerSideLevel
          member this.getPoleSidePointCandidates
                (centerPoint: latticePoint) : latticePoint [] =
              if (%this.CenterSideLevel > %this.PoleSideLevel) then
                  this.underCoverMap centerPoint
              else
                  this.overCoverMap centerPoint this.EdgeLength 
          
          member this.getCenterSidePointCandidates
                (polePoint: latticePoint) : latticePoint [] =
              if (%this.CenterSideLevel < %this.PoleSideLevel) then
                  this.underCoverMap polePoint
              else
                  this.overCoverMap polePoint this.EdgeLength

          //member this.mapToPoleSide
          //      (centerPoint: latticePoint) : latticePoint option =
          //    this.centerSideMap.[centerPoint]


module LatticeLevelSetMap =

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
                let levelSetPointsVV = LatticePoint.boundedLevelSetVV dim level edgeLength |> Seq.toArray
                printfn "%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level) (%edgeLength) (levelSetPointsVV.Length)
                //let level2 = (24 + (%sortingWidth / 2)) |> UMX.tag<latticeDistance>
                //let levelSetPointsVV2 = LatticePoint.boundedLevelSetVV dim level2 edgeLength |> Seq.toArray
                //printfn "%d\t%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level2) (%edgeLength) (levelSetPointsVV2.Length)
                


    let getAllLevelSetMaps 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            (keyMaker: int<latticeDimension> -> int<latticeDistance> -> int<latticeDistance> -> latticePoint seq)
            (overCoverMap: latticePoint -> int<latticeDistance> -> latticePoint [])
            (underCoverMap: latticePoint -> latticePoint [])
            : latticeLevelSetMap seq = 

        let maxPathLength = %edgeLength * %latticeDimension
        let midPoint = maxPathLength / 2
        
        seq {
            for level in 1 .. (midPoint) do
                let levelTag = UMX.tag<latticeDistance> level
                yield latticeLevelSetMap.create 
                        latticeDimension 
                        edgeLength 
                        (levelTag - 1<latticeDistance>) //poleSideLevel
                        (levelTag)                      //centerSideLevel
                        keyMaker 
                        overCoverMap 
                        underCoverMap

            for level in %midPoint .. (%maxPathLength - 1) do
                let levelTag = UMX.tag<latticeDistance> level
                yield latticeLevelSetMap.create 
                        latticeDimension 
                        edgeLength 
                        (levelTag + 1<latticeDistance>)  //poleSideLevel
                        (levelTag)                       //centerSideLevel
                        keyMaker 
                        overCoverMap 
                        underCoverMap
        }



    let getAllLevelSetMapsStandard 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            : latticeLevelSetMap seq = 
        getAllLevelSetMaps 
            latticeDimension 
            edgeLength
            LatticePoint.boundedLevelSet 
            LatticePoint.getOverCovers 
            LatticePoint.getUnderCovers


    let getAllLevelSetMapsVV 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            : latticeLevelSetMap seq =
        getAllLevelSetMaps 
            latticeDimension 
            edgeLength 
            LatticePoint.boundedLevelSetVV 
            LatticePoint.getOverCoversVV 
            LatticePoint.getUnderCoversVV


    let setupMaps
            (llsm: latticeLevelSetMap) : unit =

        let poleSideKeys = llsm.PoleSideMap.Keys |> Seq.toArray |> Array.sort
        for lp in poleSideKeys do
            let ccpCandidates =
                llsm.getCenterSidePointCandidates lp
            let ccpWinners = 
                       ccpCandidates
                       |> Array.sort
                       |> Array.filter(fun ccp ->
                            match llsm.CenterSideMap.[ccp] with
                            | [] -> false
                            | _ -> true
                       )

            let ccp = if (ccpWinners.Length > 0) then 
                          ccpWinners.[0]
                      else
                         ccpCandidates.[0]
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

