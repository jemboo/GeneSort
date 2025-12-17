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

          member this.mapToPoleSide
                (centerPoint: latticePoint) : latticePoint option =
              this.centerSideMap.[centerPoint]


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



    // return true if updated, false if the center side map is complete
    let initCenterSideMapStandard
            (llsm: latticeLevelSetMap) : unit =
        let keysNeedingUpdate =
            llsm.CenterSideMap
            |> Seq.filter(fun kvp -> kvp.Value.IsNone)
            |> Seq.map(fun kvp -> kvp.Key)
            |> Seq.toArray
        
        if keysNeedingUpdate.Length > 0 then
            for key in keysNeedingUpdate do
                let openSlots =
                    llsm.getPoleSidePointCandidates key
                    |> Array.filter(fun polePoint ->
                        llsm.PoleSideMap.[polePoint].Length = 0
                    )
                let poleSideTarget =
                    if openSlots.Length = 0 then
                         (llsm.getPoleSidePointCandidates key).[0]
                    else
                        openSlots.[openSlots.Length - 1]

                llsm.PoleSideMap.[poleSideTarget] <- key :: llsm.PoleSideMap.[poleSideTarget]
                llsm.CenterSideMap.[key] <- Some poleSideTarget



    // return true if updated, false if the center side map is complete
    let initCenterSideMapVV
            (llsm: latticeLevelSetMap) : unit =
        let keysNeedingUpdate =
            llsm.CenterSideMap
            |> Seq.filter(fun kvp -> kvp.Value.IsNone)
            |> Seq.map(fun kvp -> kvp.Key)
            |> Seq.toArray
        
        if keysNeedingUpdate.Length > 0 then
            for key in keysNeedingUpdate do
                let openSlots =
                    llsm.getPoleSidePointCandidates key
                    |> Array.filter(fun polePoint ->
                        llsm.PoleSideMap.[polePoint].Length = 0
                    )
                let poleSideTarget =
                    if openSlots.Length = 0 then
                         (llsm.getPoleSidePointCandidates key).[0]
                    else
                        openSlots.[openSlots.Length - 1]

                llsm.PoleSideMap.[poleSideTarget] <- key :: llsm.PoleSideMap.[poleSideTarget]
                llsm.CenterSideMap.[key] <- Some poleSideTarget


    let initMapsStandardOld
            (llsm: latticeLevelSetMap) : unit =

        let poleSideKeys = llsm.PoleSideMap.Keys |> Seq.toArray |> Array.sort
        for lp in poleSideKeys do
            let ccps = llsm.getCenterSidePointCandidates lp
                       |> Array.sort
            let ccp = if (llsm.CenterSideLevel < llsm.PoleSideLevel) then 
                       ccps.[ccps.Length - 1]
                       //  ccps.[0]
                      else
                       ccps.[0]
                       //  ccps.[ccps.Length - 1]

            llsm.PoleSideMap.[lp] <- [ccp]
            
            match llsm.CenterSideMap.[ccp] with
            | Some existingPole ->
                let kvp = llsm.CenterSideMap.Keys  
                            |> Seq.map(fun k -> (k, llsm.CenterSideMap.[k]))
                            |> Seq.filter(fun (k, v) -> v.IsNone && (ccps |> Array.contains k))
                            |> Seq.toArray
                if kvp.Length = 0 then  
                    failwith "Failed to initialize level set map: no available center side points."
                let newFinding = kvp.[0]
                llsm.CenterSideMap.[newFinding |> fst] <- Some lp

            | None -> llsm.CenterSideMap.[ccp] <- Some lp


        for lp in llsm.CenterSideMap.Keys do
            match llsm.CenterSideMap.[lp] with
            | Some psp -> ()
            | None ->
                let pspCandidates = llsm.getPoleSidePointCandidates lp
                llsm.CenterSideMap.[lp] <- Some pspCandidates.[0]
                llsm.PoleSideMap.[pspCandidates.[0]] <-
                    lp :: llsm.PoleSideMap.[pspCandidates.[0]]




    let initMapsStandard
            (llsm: latticeLevelSetMap) : unit =

        let poleSideKeys = llsm.PoleSideMap.Keys |> Seq.toArray |> Array.sort
        for lp in poleSideKeys do
            let ccps = llsm.getCenterSidePointCandidates lp
                       |> Array.sort
                       |> Array.filter(fun ccp ->
                            match llsm.CenterSideMap.[ccp] with
                            | Some _ -> false
                            | None -> true
                       )

            let ccp = if (ccps.Length > 0) then 
                          ccps.[0]
                      else
                          failwith "Failed to initialize level set map: no available center side points."

            llsm.PoleSideMap.[lp] <- ccp :: llsm.PoleSideMap.[lp]
            llsm.CenterSideMap.[ccp] <- Some lp

        for lp in llsm.CenterSideMap.Keys do
            match llsm.CenterSideMap.[lp] with
            | Some psp -> ()
            | None ->
                let pspCandidates = llsm.getPoleSidePointCandidates lp
                if (not (llsm.PoleSideMap.ContainsKey(pspCandidates.[0]))) then
                    failwith "Failed to initialize level set map: pole side point not found in map."    


                llsm.CenterSideMap.[lp] <- Some pspCandidates.[0]

                llsm.PoleSideMap.[pspCandidates.[0]] <-
                    lp :: llsm.PoleSideMap.[pspCandidates.[0]]



        

    let initMapsVV
            (llsm: latticeLevelSetMap) : unit =
        for lp in llsm.PoleSideMap.Keys do
            let ccps = llsm.getCenterSidePointCandidates lp
                       |> Array.sort
            let ccp = if (llsm.CenterSideLevel < llsm.PoleSideLevel) then 
                        ccps.[ccps.Length - 1]
                      else
                        ccps.[0]
            llsm.PoleSideMap.[lp] <- [ccp]
            llsm.CenterSideMap.[ccp] <- Some lp

        for lp in llsm.CenterSideMap.Keys do
            match llsm.CenterSideMap.[lp] with
            | Some psp -> ()
            | None ->
                let pspCandidates = llsm.getPoleSidePointCandidates lp
                llsm.CenterSideMap.[lp] <- Some pspCandidates.[0]
                llsm.PoleSideMap.[pspCandidates.[0]] <-
                    lp :: llsm.PoleSideMap.[pspCandidates.[0]]


    let getPriority (llsm: latticeLevelSetMap) (centerPoint: latticePoint) : int =
        let yab = llsm.CenterSideMap.[centerPoint]
        match yab with
        | Some polePoint -> 
            let assignedCenters = llsm.PoleSideMap.[polePoint]
            assignedCenters.Length
        | None -> 0


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
                let centerCandidates = llsm.getCenterSidePointCandidates key
                //let centerSideTarget =
                //         centerCandidates.[indexShuffler centerCandidates.Length]

                let centerSideTargets =
                    centerCandidates
                    |> Array.sortByDescending(getPriority llsm)
                let centerSideTarget = centerSideTargets.[0]

                let prevPoleSide = llsm.CenterSideMap.[centerSideTarget]
                match prevPoleSide with
                | Some psp ->
                    let oldPoleList = llsm.PoleSideMap.[psp]
                    llsm.PoleSideMap.[psp] <- (oldPoleList |> List.filter(fun lp -> lp <> centerSideTarget))
                | None -> ()

                llsm.CenterSideMap.[centerSideTarget] <- Some key
                llsm.PoleSideMap.[key] <- centerSideTarget :: llsm.PoleSideMap.[key]
            true
        else
            false



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



    let isComplete
            (llsm: latticeLevelSetMap) : bool =
        llsm.CenterSideMap
        |> Seq.forall(fun kvp -> kvp.Value.IsSome)
        &&
        llsm.PoleSideMap
        |> Seq.forall(fun kvp -> kvp.Value.Length > 0)



    let optimize
             (llsm: latticeLevelSetMap) 
             (indexShuffler: int -> int): bool =

        let mutable missingPoles = missingPoleCount llsm
        let mutable retryCount = 0
        while missingPoles > 0 do
            let resPole = updatePoleSideMap llsm indexShuffler
            missingPoles <- missingPoleCount llsm
            retryCount <- retryCount + 1
            if retryCount > 50000 then
                failwith "Failed to optimize level set map after multiple attempts."
        true


    //let optimize
    //         (llsm: latticeLevelSetMap) 
    //         (indexShuffler: int -> int): bool =

    //    let res = updateCenterSideMap llsm indexShuffler
    //    let missingPoles = missingPoleCount llsm
    //    if missingPoles = 0 then
    //        true
    //    else
    //        let res2 = updatePoleSideMap llsm indexShuffler
    //        let missingCenters = missingCenterCount llsm
    //        if missingCenters = 0 then
    //            true
    //        else
    //            let res3 = updateCenterSideMap llsm indexShuffler
    //            let missingPoles2 = missingPoleCount llsm
    //            if missingPoles2 = 0 then
    //                true
    //            else
    //                false