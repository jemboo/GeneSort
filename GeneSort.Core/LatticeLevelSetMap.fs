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
          centerSideLevel: int<latticeDistance> }

          static member create
            (latticeDimension: int<latticeDimension>)
            (edgeLength: int<latticeDistance>)
            (poleSideLevel: int<latticeDistance>)
            (centerSideLevel: int<latticeDistance>)
            (keyMaker: int<latticeDimension> -> int<latticeDistance> -> int<latticeDistance> -> latticePoint seq)
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
              centerSideLevel = centerSideLevel  }


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
          member this.MaxDistance with get() = this.edgeLength
          member this.PoleSideLevel with get() = this.poleSideLevel
          member this.CenterSideLevel with get() = this.centerSideLevel
          

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
            : latticeLevelSetMap [] = 

        let maxPathLength = %edgeLength * %latticeDimension
        let midPoint = maxPathLength / 2
        
        let lowerMaps =
            [| 1 .. (midPoint - 1) |] 
                |> Array.map(UMX.tag<latticeDistance>)
                |> Array.map (fun level -> 
                latticeLevelSetMap.create latticeDimension edgeLength (level - 1<latticeDistance>) (level) keyMaker
            )
        
        let upperMaps =
            [| %midPoint .. (%maxPathLength - 1) |]
                |> Array.map(UMX.tag<latticeDistance>)
                |> Array.map (fun level -> 
                latticeLevelSetMap.create latticeDimension edgeLength (level + 1<latticeDistance>) (level) keyMaker
            )

        Array.append lowerMaps upperMaps


    let getAllLevelSetMapsStandard 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            : latticeLevelSetMap [] = 
        getAllLevelSetMaps latticeDimension edgeLength LatticePoint.boundedLevelSet


    let getAllLevelSetMapsVV 
            (latticeDimension: int<latticeDimension>) 
            (edgeLength: int<latticeDistance>) 
            : latticeLevelSetMap [] =
        getAllLevelSetMaps latticeDimension edgeLength LatticePoint.boundedLevelSetVV



