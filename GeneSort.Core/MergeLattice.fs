namespace GeneSort.Core
open FSharp.UMX
open System.Collections.Generic
open System


type mergeLattice =
    private 
        { 
          latticeDimension: int<latticeDimension>
          edgeLength: int<latticeDistance> 
        }

          static member create
            (latticeDimension: int<latticeDimension>)
            (edgeLength: int<latticeDistance>)
            : mergeLattice =
            if %latticeDimension < 2 then
                invalidArg "latticeDimension" "latticeDimension length must be at least 2."
            { 
              latticeDimension = latticeDimension
              edgeLength = edgeLength
            }

          member this.LatticeDimension with get() = this.latticeDimension
          member this.EdgeLength with get() = this.edgeLength
          member this.MaxDistance with get() = this.edgeLength * this.latticeDimension


module MergeLattice =

    let create  (latticeDimension: int<latticeDimension>)
                (edgeLength: int<latticeDistance>)
        : mergeLattice =
        mergeLattice.create latticeDimension edgeLength


    let getOptimalLevelSetMapsStandard
        (ml: mergeLattice)
        : latticeLevelSetMap seq =
        
        let shuffles dex = 0
        seq {
            for yab in LatticeLevelSetMap.getAllLevelSetMapsStandard ml.LatticeDimension ml.EdgeLength do

                    let qua = LatticeLevelSetMap.optimize yab shuffles
                    if qua then
                        yield yab
                    else
                        failwith "Failed to optimize level set map."
        }


    let getOptimalLevelSetMapsVV
        (ml: mergeLattice)
        : latticeLevelSetMap seq =
        
        let shuffles dex = 0
        seq {
            for yab in LatticeLevelSetMap.getAllLevelSetMapsVV ml.LatticeDimension ml.EdgeLength do

                    let qua = LatticeLevelSetMap.optimize yab shuffles
                    if qua then
                        yield yab
                    else
                        failwith "Failed to optimize level set map."
        }

