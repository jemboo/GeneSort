namespace GeneSort.Core
open FSharp.UMX
open System
open System.Collections.Generic

type latticeLevelSetMap =
    private 
        { mapToPole: Dictionary<latticePoint,latticePoint>
          mapFromPole: Dictionary<latticePoint, latticePoint list>
          arrayLength: int
          latticePointMaxValue: int
          mapLevel: int }

          static member create
            (arrayLength: int)
            (latticePointMaxValue: int)
            (mapLevel: int)
            : latticeLevelSetMap =
            if arrayLength < 1 then
                invalidArg "arrayLength" "Array length must be at least 1."
            if mapLevel < 1 then
                invalidArg "mapLevel" "Map level must be at least 1."
            { mapToPole = Dictionary<latticePoint,latticePoint>()
              mapFromPole = Dictionary<latticePoint, latticePoint list>()
              arrayLength = arrayLength
              latticePointMaxValue = latticePointMaxValue
              mapLevel = mapLevel  }

          member this.MapToPole with get() = this.mapToPole
          member this.MapFromPole with get() = this.mapFromPole
          member this.ArrayLength with get() = this.arrayLength
          member this.LatticePointMaxValue with get() = this.latticePointMaxValue
          member this.BooleanDimension with get() = 
                (this.LatticePointMaxValue + 1) * this.ArrayLength
          // the lowest level latticeLevelSetMap where mapToPole contains just one key - the zero point, is level 1
          // the highest level is where mapToPole contains just one key - Array.create (this.ArrayLength) (this.LatticePointMaxValue),
          // is level (this.BooleanDimension - 1)
          member this.MapLevel with get() = this.mapLevel
          

module LatticeLevelSetMap =
    
    let getAllLevelSetMaps 
            (latticePointMaxValue: int) 
            (arrayLength: int) : latticeLevelSetMap [] = 
        
        let maxLevel = (latticePointMaxValue + 1) * arrayLength - 1
        
        Array.init maxLevel (fun level -> 
            latticeLevelSetMap.create arrayLength latticePointMaxValue (level + 1)
        )
            
        


    let addMapping 
            (llsm: latticeLevelSetMap) 
            (fromPoint: latticePoint) 
            (toPole: latticePoint) : unit =
        llsm.MapToPole.[fromPoint] <- toPole
        if llsm.MapFromPole.ContainsKey(toPole) then
            llsm.MapFromPole.[toPole] <- fromPoint :: llsm.MapFromPole.[toPole]
        else
            llsm.MapFromPole.[toPole] <- [fromPoint]