namespace GeneSort.Core.Mp

open System
open System.Collections.Generic
open GeneSort.Core
open FSharp.UMX

type latticeLevelSetMapDto = {
    centerSideMap: (latticePointDto * latticePointDto list) array
    poleSideMap: (latticePointDto * latticePointDto list) array
    latticeDimension: int
    edgeLength: int
    poleSideLevel: int
    centerSideLevel: int
    coverStrategy: int
}

module LatticeLevelSetMapDto =

    let fromDomain (map: latticeLevelSetMap) : latticeLevelSetMapDto =
        let transform (d: Dictionary<latticePoint, latticePoint list>) =
            d 
            |> Seq.map (fun kvp -> 
                LatticePointDto.fromDomain kvp.Key, 
                kvp.Value |> List.map LatticePointDto.fromDomain)
            |> Seq.toArray

        { centerSideMap = transform map.CenterSideMap
          poleSideMap = transform map.PoleSideMap
          latticeDimension = %map.LatticeDimension
          edgeLength = %map.EdgeLength
          poleSideLevel = %map.PoleSideLevel
          centerSideLevel = %map.CenterSideLevel
          coverStrategy = int map.CoverStrategy }

    let toDomain (dto: latticeLevelSetMapDto) : latticeLevelSetMap =
        let map = 
            latticeLevelSetMap.create 
                (UMX.tag<latticeDimension> dto.latticeDimension)
                (UMX.tag<latticeDistance> dto.edgeLength)
                (UMX.tag<latticeDistance> dto.poleSideLevel)
                (UMX.tag<latticeDistance> dto.centerSideLevel)
                (enum<coverType> dto.coverStrategy)
        
        // Re-populate maps from DTO data
        for (kDto, vDtos) in dto.centerSideMap do
            let key = LatticePointDto.toDomain kDto
            let values = vDtos |> List.map LatticePointDto.toDomain
            map.CenterSideMap.[key] <- values
            
        for (kDto, vDtos) in dto.poleSideMap do
            let key = LatticePointDto.toDomain kDto
            let values = vDtos |> List.map LatticePointDto.toDomain
            map.PoleSideMap.[key] <- values
            
        map