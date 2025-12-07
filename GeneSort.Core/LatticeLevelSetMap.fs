namespace GeneSort.Core
open FSharp.UMX
open System.Collections.Generic


type latticeLevelSetMap =
    private 
        { centerSideMap: Dictionary<latticePoint, latticePoint option>
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

            let centerSideMap = Dictionary<latticePoint,latticePoint option>()
            // Initialize centerSideMap with keys from latticeLevelSet
            for point in keyMaker latticeDimension centerSideLevel edgeLength do
                centerSideMap.[point] <- None

            let poleSideMap = Dictionary<latticePoint, latticePoint list>()
            // Initialize poleSideMap with keys from latticeLevelSet
            for point in keyMaker latticeDimension poleSideLevel edgeLength do
                poleSideMap.[point] <- []

            { centerSideMap = centerSideMap
              poleSideMap = poleSideMap
              latticeDimension = latticeDimension
              edgeLength = edgeLength
              poleSideLevel = poleSideLevel
              centerSideLevel = centerSideLevel  
              overCoverMap = overCoverMap
              underCoverMap = underCoverMap }


          member this.addMapping
                (fromPoint: latticePoint)
                (toPole: latticePoint) : unit =
              this.centerSideMap.[fromPoint] <- Some toPole
              if this.poleSideMap.ContainsKey(toPole) then
                  this.poleSideMap.[toPole] <- fromPoint :: this.poleSideMap.[toPole]
              else
                  this.poleSideMap.[toPole] <- [fromPoint]

          member this.CenterSideMap with get() = this.centerSideMap
          member this.PoleSideMap with get() = this.poleSideMap
          member this.LatticeDimension with get() = this.latticeDimension
          member this.EdgeLength with get() = this.edgeLength
          member this.MaxDistance with get() = this.edgeLength * this.latticeDimension
          member this.PoleSideLevel with get() = this.poleSideLevel
          member this.CenterSideLevel with get() = this.centerSideLevel
          member this.getPoleSidePoints
                (centerPoint: latticePoint) : latticePoint [] =
              if (%this.CenterSideLevel > %this.PoleSideLevel) then
                  this.underCoverMap centerPoint
              else
           
                  this.overCoverMap centerPoint (this.EdgeLength + 1<latticeDistance>)
          
          member this.getCenterSidePoint
                (polePoint: latticePoint) : latticePoint [] =
              if (%this.CenterSideLevel < %this.PoleSideLevel) then
                  this.underCoverMap polePoint
              else
           
                  this.overCoverMap polePoint this.EdgeLength

          member this.mapToPoleSide
                (centerPoint: latticePoint) : latticePoint option =
              this.centerSideMap.[centerPoint]


module LatticeLevelSetMap =

    let getStats =

        let latticeDimensions = [3; 4] |> List.map UMX.tag<latticeDimension>
        let edgeLengths = [16; 24; 32; 48; 64; 96; 128] |> List.map UMX.tag<latticeDistance>

        printfn "SortingWidth\tDimension\tlevel\tMaxPerTuple\tLevelSet\tLevelSetVV"
        for dim in latticeDimensions do
            for edgeLength in edgeLengths do
                let sortingWidth = (%dim * %edgeLength) |> UMX.tag<latticeDistance>
                let level = (%sortingWidth / 2) |> UMX.tag<latticeDistance>
                let levelSetPoints = LatticePoint.boundedLevelSet dim level edgeLength |> Seq.toArray
                let levelSetPointsVV = LatticePoint.boundedLevelSetVV dim level edgeLength |> Seq.toArray
                printfn "%d\t%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level) (%edgeLength) (levelSetPoints.Length) (levelSetPointsVV.Length)
                let level2 = (24 + (%sortingWidth / 2)) |> UMX.tag<latticeDistance>
                let levelSetPoints2 = LatticePoint.boundedLevelSet dim level2 edgeLength |> Seq.toArray
                let levelSetPointsVV2 = LatticePoint.boundedLevelSetVV dim level2 edgeLength |> Seq.toArray
                printfn "%d\t%d\t%d\t%d\t%d\t%d" (%sortingWidth) (%dim) (%level2) (%edgeLength) (levelSetPoints2.Length) (levelSetPointsVV2.Length)


    
    let getAllLevelSetMaps 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            (keyMaker: int<latticeDimension> -> int<latticeDistance> -> int<latticeDistance> -> latticePoint seq)
            (overCoverMap: latticePoint -> int<latticeDistance> -> latticePoint [])
            (underCoverMap: latticePoint -> latticePoint [])
            : latticeLevelSetMap [] = 

        let maxPathLength = %edgeLength * %latticeDimension
        let midPoint = maxPathLength / 2
        
        let lowerMaps =
            [| 1 .. (midPoint - 1) |] 
                |> Array.map(UMX.tag<latticeDistance>)
                |> Array.map (fun level -> 
                latticeLevelSetMap.create 
                        latticeDimension 
                        edgeLength 
                        (level - 1<latticeDistance>) 
                        (level) 
                        keyMaker 
                        overCoverMap 
                        underCoverMap
            )
        
        let upperMaps =
            [| %midPoint .. (%maxPathLength - 1) |]
                |> Array.map(UMX.tag<latticeDistance>)
                |> Array.map (fun level -> 
                latticeLevelSetMap.create 
                        latticeDimension 
                        edgeLength 
                        (level + 1<latticeDistance>) 
                        (level) 
                        keyMaker 
                        overCoverMap 
                        underCoverMap
            )

        Array.append lowerMaps upperMaps


    let getAllLevelSetMapsStandard 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            : latticeLevelSetMap [] = 
        getAllLevelSetMaps 
            latticeDimension 
            edgeLength
            LatticePoint.boundedLevelSet LatticePoint.getOverCovers LatticePoint.getUnderCovers


    let getAllLevelSetMapsVV 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            : latticeLevelSetMap [] =
        getAllLevelSetMaps 
            latticeDimension 
            edgeLength 
            LatticePoint.boundedLevelSetVV LatticePoint.getOverCoversVV LatticePoint.getUnderCoversVV


    // return true if updated, false if the center side map is complete
    let updateCenterSideMap
            (llsm: latticeLevelSetMap)
            (indexShuffler: int -> int) : bool =
        let keysNeedingUpdate =
            llsm.CenterSideMap
            |> Seq.filter(fun kvp -> kvp.Value.IsNone)
            |> Seq.map(fun kvp -> kvp.Key)
            |> Seq.toArray
        
        if keysNeedingUpdate.Length > 0 then
            for key in keysNeedingUpdate do
                let openSlots =
                    llsm.getPoleSidePoints key
                    |> Array.filter(fun polePoint ->
                        llsm.PoleSideMap.[polePoint].Length = 0
                    )
                let poleSideTarget =
                    if openSlots.Length = 0 then
                         (llsm.getPoleSidePoints key).[0]
                    else
                        openSlots.[indexShuffler openSlots.Length]

                llsm.PoleSideMap.[poleSideTarget] <- key :: llsm.PoleSideMap.[poleSideTarget]
                llsm.CenterSideMap.[key] <- Some poleSideTarget

            true
        else
            false



    let updatePoleSideMap 
            (llsm: latticeLevelSetMap)
            (indexShuffler: int -> int)
            : bool =
        let keysNeedingUpdate =
            llsm.PoleSideMap
            |> Seq.filter(fun kvp -> kvp.Value.Length = 0)
            |> Seq.map(fun kvp -> kvp.Key)
            |> Seq.toArray
        
        if keysNeedingUpdate.Length > 0 then
            for key in keysNeedingUpdate do
                let openSlots =
                    llsm.getCenterSidePoint key
                    |> Array.filter(fun centerPoint ->
                        llsm.CenterSideMap.[centerPoint].IsNone
                    )
                let centerSideTarget =
                    if openSlots.Length = 0 then
                         (llsm.getCenterSidePoint key).[0]
                    else
                        let cst = openSlots.[indexShuffler openSlots.Length]
                        let oldPoleList = llsm.PoleSideMap.[cst]
                        llsm.PoleSideMap.[cst] <- (oldPoleList |> List.filter(fun lp -> lp <> cst))
                        cst

                llsm.CenterSideMap.[centerSideTarget] <- Some key
                llsm.PoleSideMap.[key] <- centerSideTarget :: llsm.PoleSideMap.[key]
            true
        else
            false



    let isComplete
            (llsm: latticeLevelSetMap) : bool =
        llsm.CenterSideMap
        |> Seq.forall(fun kvp -> kvp.Value.IsSome)
        &&
        llsm.PoleSideMap
        |> Seq.forall(fun kvp -> kvp.Value.Length > 0)


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
            |> Seq.filter(fun kvp -> kvp.Value.IsNone)
            |> Seq.length
        centerMissing

