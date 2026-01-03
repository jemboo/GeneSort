namespace GeneSort.Core
open FSharp.UMX
open System


type latticePathPermtation = 
    private {mutable latticePoint: latticePoint; perm: int option [] }
    with
        static member createEmpty (latticeDimension: int<latticeDimension>) (pathLength:int<latticeDistance>) = 
            { latticePoint = latticePoint.create(Array.zeroCreate %latticeDimension); 
              perm = Array.create<int option> %pathLength None}

        static member create (latticePoint:latticePoint) (perm:int option []) = 
            {latticePoint = latticePoint; perm = perm}

        member this.Length with get() = this.perm.Length
        member this.LatticePoint 
                with get() = this.latticePoint
                and set (value:latticePoint) = this.latticePoint <- value

        member this.PermutationArray with get() = this.perm

        member this.Item
            with get(i:int) = this.perm.[i]
            and set (i:int) (value: int option) =
                if this.perm.[i].IsSome then
                    invalidArg (string i) "Position is already occupied."
                this.perm.[i] <- value

        member this.IsComplete
            with get() =
                Array.forall Option.isSome this.perm




module LatticePathPermtation =

    let copy (lptp:latticePathPermtation) =
        let newPerm = Array.copy lptp.PermutationArray
        {latticePoint = lptp.LatticePoint; perm = newPerm}
        
    let updateWithLatticePoint (edgeLength: int<latticeDistance>) 
               (newLevel:int)
               (newLp:latticePoint) 
               (lpPerm:latticePathPermtation) : latticePathPermtation =
        let newLptp = copy lpPerm
        newLptp.LatticePoint <- newLp
        let index = LatticePoint.getPermutationIndex edgeLength lpPerm.LatticePoint newLp
        newLptp.[index] <- Some newLevel
        newLptp


    let toPermutation (lptp:latticePathPermtation) : permutation =
        let indexes = 
            seq {
                for i = 0 to (lptp.Length - 1) do
                    yield lptp[i].Value
            }
        permutation.create (Array.ofSeq indexes)