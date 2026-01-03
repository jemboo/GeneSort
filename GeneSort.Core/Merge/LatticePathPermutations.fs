namespace GeneSort.Core
open FSharp.UMX


type latticePathPermutations =  private { items: latticePathPermtation []; level: int<latticeDistance> }
     with
        static member create(items: latticePathPermtation [], level: int<latticeDistance>) =
            { items = items; level = level }
        member this.Level with get() = this.level
        member this.Item
            with get(i:int) = this.items.[i]
        member this.Count with get() = this.items.Length



module LatticePathPermutations =

   let createLevelZero 
            (latticeDimension: int<latticeDimension>) 
            (pathLength:int<latticeDistance>) : latticePathPermutations =
       let lpPerms = [| latticePathPermtation.createEmpty latticeDimension pathLength |]
       latticePathPermutations.create(lpPerms, 0<latticeDistance>)

   let update (lpPerms : latticePathPermutations)
              (lssm:latticeLevelSetMap) : latticePathPermutations =

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
          latticePathPermutations.create(newLpPerms, lpPerms.Level + 1<latticeDistance>)

       else
          if %lpPerms.Level <> %lssm.CenterSideLevel then
              invalidArg "latticeLevelSetMap" "Level set map CenterSideLevel does not match lattice path permutations level."

          let newLpPerms = 
              seq {
                      for dex in 0 .. lpPerms.Count - 1 do
                          let lpPerm = lpPerms.[dex]
                          let mappedPoints = lssm.CenterSideMap[lpPerm.LatticePoint]
                          for x in 0 .. mappedPoints.Length - 1 do
                              yield LatticePathPermtation.updateWithLatticePoint 
                                        lssm.EdgeLength 
                                        (%lssm.MaxDistance - %lssm.CenterSideLevel - 1) 
                                        mappedPoints.[x]
                                        lpPerm
                  } |> Seq.toArray
          latticePathPermutations.create(newLpPerms, lpPerms.Level + 1<latticeDistance>)


   let toPermutations (lpPerms:latticePathPermutations) : permutation [] =
        seq {
            for dex in 0 .. lpPerms.Count - 1 do
                let lpPerm = lpPerms.[dex]
                yield LatticePathPermtation.toPermutation lpPerm
        } |> Seq.toArray