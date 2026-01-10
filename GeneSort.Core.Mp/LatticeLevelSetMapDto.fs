namespace GeneSort.Core.Mp

open System
open System.Collections.Generic
open GeneSort.Core
open MessagePack
open FSharp.UMX

[<MessagePackObject>]
type LatticeLevelSetMapDto =
    { [<Key(0)>] CenterSideMap: (LatticePointDto * LatticePointDto list) array
      [<Key(1)>] PoleSideMap: (LatticePointDto * LatticePointDto list) array
      [<Key(2)>] LatticeDimension: int
      [<Key(3)>] EdgeLength: int
      [<Key(4)>] PoleSideLevel: int
      [<Key(5)>] CenterSideLevel: int
      [<Key(6)>] CoverStrategy: int }

module LatticeLevelSetMapDto =

    let fromDomain (map: latticeLevelSetMap) : LatticeLevelSetMapDto =
        let transform (d: Dictionary<latticePoint, latticePoint list>) =
            d 
            |> Seq.map (fun kvp -> 
                LatticePointDto.fromDomain kvp.Key, 
                kvp.Value |> List.map LatticePointDto.fromDomain)
            |> Seq.toArray

        { CenterSideMap = transform map.CenterSideMap
          PoleSideMap = transform map.PoleSideMap
          LatticeDimension = %map.LatticeDimension
          EdgeLength = %map.EdgeLength
          PoleSideLevel = %map.PoleSideLevel
          CenterSideLevel = %map.CenterSideLevel
          CoverStrategy = int map.CoverStrategy }

    let toDomain (dto: LatticeLevelSetMapDto) : latticeLevelSetMap =
        let map = 
            latticeLevelSetMap.create 
                (UMX.tag<latticeDimension> dto.LatticeDimension)
                (UMX.tag<latticeDistance> dto.EdgeLength)
                (UMX.tag<latticeDistance> dto.PoleSideLevel)
                (UMX.tag<latticeDistance> dto.CenterSideLevel)
                (enum<coverType> dto.CoverStrategy)
        
        // Re-populate maps from DTO data
        for (kDto, vDtos) in dto.CenterSideMap do
            let key = LatticePointDto.toDomain kDto
            let values = vDtos |> List.map LatticePointDto.toDomain
            map.CenterSideMap.[key] <- values
            
        for (kDto, vDtos) in dto.PoleSideMap do
            let key = LatticePointDto.toDomain kDto
            let values = vDtos |> List.map LatticePointDto.toDomain
            map.PoleSideMap.[key] <- values
            
        map