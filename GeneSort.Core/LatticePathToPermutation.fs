namespace GeneSort.Core
open FSharp.UMX
open System


type latticePathToPermtation = 
    private {mutable latticePoint: latticePoint; perm: int option [] }
    with
        static member createEmpty(latticePoint:latticePoint) (pathLength:int) = 
            {latticePoint = latticePoint; perm = Array.create<int option> pathLength None}

        static member create(latticePoint:latticePoint) (perm:int option []) = 
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




module latticePointToPermtation =

    let copy (lptp:latticePathToPermtation) =
        let newPerm = Array.copy lptp.PermutationArray
        {latticePoint = lptp.LatticePoint; perm = newPerm}
        
    let update (edgeLength: int<latticeDistance>) 
               (lptp:latticePathToPermtation)
               (newLevel:int)
               (newLp:latticePoint) : latticePathToPermtation =
        let newLptp = copy lptp
        newLptp.LatticePoint <- newLp
        let index = LatticePoint.getPermutationIndex edgeLength lptp.LatticePoint newLp
        newLptp.[index] <- Some newLevel
        newLptp

    let toPermutation (lptp:latticePathToPermtation) =
        let indexes = 
            seq {
                for i = 0 to (lptp.Length - 1) do
                    yield i
            
            }
        Permutation.create (Array.ofSeq indexes)