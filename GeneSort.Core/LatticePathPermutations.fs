namespace GeneSort.Core
open FSharp.UMX


type latticePathPermtations =  private { items: latticePathPermtation []; level: int<latticeDistance> }
     with
        static member create(items: latticePathPermtation [], level: int<latticeDistance>) =
            { items = items; level = level }
        member this.Level with get() = this.level
        member this.Item
            with get(i:int) = this.items.[i]
        member this.Count with get() = this.items.Length




module LatticePathPermtations =

   let createLevelZero 
            (latticeDimension: int<latticeDimension>) 
            (pathLength:int<latticeDistance>) : latticePathPermtations =
       let lpPerms = [| latticePathPermtation.createEmpty latticeDimension pathLength |]
       latticePathPermtations.create(lpPerms, 0<latticeDistance>)

   let update (lpPerms : latticePathPermtations)
              (lssm:latticeLevelSetMap) : latticePathPermtations =

       if %lssm.PoleSideLevel < %lssm.CenterSideLevel then

          if %lpPerms.Level <> %lssm.PoleSideLevel then
              invalidArg "latticeLevelSetMap" "Level set map PoleSideLevel does not match lattice path permutations level."

          let newLpPerms = 
              seq {
                      for dex in 0 .. lpPerms.Count - 1 do
                          let lpPerm = lpPerms.[dex]
                          let mappedPoints = lssm.PoleSideMap.[lpPerm.LatticePoint]
                          for x in 0 .. mappedPoints.Length - 1 do
                                yield LatticePathPermtation.updateWithLatticePoint 
                                            lssm.EdgeLength 
                                            (%lssm.MaxDistance - %lssm.PoleSideLevel - 1) 
                                            (mappedPoints.[x]) 
                                            lpPerm
                  } |> Seq.toArray
          latticePathPermtations.create(newLpPerms, lpPerms.Level + 1<latticeDistance>)

       else
          if %lpPerms.Level <> %lssm.CenterSideLevel then
              invalidArg "latticeLevelSetMap" "Level set map CenterSideLevel does not match lattice path permutations level."

          let newLpPerms = 
              seq {
                      for dex in 0 .. lpPerms.Count - 1 do
                          let lpPerm = lpPerms.[dex]
                          let mappedPoint = lssm.CenterSideMap[lpPerm.LatticePoint].Value
                          yield LatticePathPermtation.updateWithLatticePoint 
                                    lssm.EdgeLength 
                                    (%lssm.MaxDistance - %lssm.CenterSideLevel - 1) 
                                    mappedPoint 
                                    lpPerm
                  } |> Seq.toArray
          latticePathPermtations.create(newLpPerms, lpPerms.Level + 1<latticeDistance>)


   let toPermutations (lpPerms:latticePathPermtations) : Permutation [] =
        seq {
            for dex in 0 .. lpPerms.Count - 1 do
                let lpPerm = lpPerms.[dex]
                yield LatticePathPermtation.toPermutation lpPerm
        } |> Seq.toArray