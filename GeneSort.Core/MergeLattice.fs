namespace GeneSort.Core
open FSharp.UMX
open System.Collections.Generic
open System


type mergeLattice =
    private 
        { 
          latticeDimension: int<latticeDimension>
          edgeLength: int<latticeDistance> 
          dictByCenterSideLevel: Dictionary<int<latticeDistance>, latticeLevelSetMap>
        }

          static member create
            (latticeDimension: int<latticeDimension>)
            (edgeLength: int<latticeDistance>)
            (latticeLevelSetMaps: latticeLevelSetMap [])
            : mergeLattice =
            if %latticeDimension < 2 then
                invalidArg "latticeDimension" "latticeDimension length must be at least 2."

            // Build dictionary keyed by centerSideLevel
            let dictByCenterSideLevel = Dictionary<int<latticeDistance>, latticeLevelSetMap>()
            for levelSetMap in latticeLevelSetMaps do
                let key = Math.Min(%levelSetMap.CenterSideLevel, %levelSetMap.PoleSideLevel) * 1<latticeDistance>
                dictByCenterSideLevel.[key] <- levelSetMap
        
            { 
              latticeDimension = latticeDimension
              edgeLength = edgeLength 
              dictByCenterSideLevel = dictByCenterSideLevel
            }

          member this.LatticeDimension with get() = this.latticeDimension
          member this.EdgeLength with get() = this.edgeLength
          member this.MaxDistance with get() = this.edgeLength * this.latticeDimension


module MergeLattice =

       let createStandardMergeLattice
            (latticeDimension: int<latticeDimension>)
            (edgeLength: int<latticeDistance>)
            : mergeLattice =
            let levelSetMaps = 
                            LatticeLevelSetMap.getAllLevelSetMapsStandard
                                latticeDimension
                                edgeLength
            mergeLattice.create
                latticeDimension
                edgeLength
                levelSetMaps


       let createVVMergeLattice
            (latticeDimension: int<latticeDimension>)
            (edgeLength: int<latticeDistance>)
            : mergeLattice =
            let levelSetMaps = 
                            LatticeLevelSetMap.getAllLevelSetMapsVV
                                latticeDimension
                                edgeLength
            mergeLattice.create
                latticeDimension
                edgeLength
                levelSetMaps

