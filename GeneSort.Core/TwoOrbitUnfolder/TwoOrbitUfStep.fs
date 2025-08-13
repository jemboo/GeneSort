namespace GeneSort.Core

open TwoOrbitPairOps

/// Specifies the transformation of sequences of TwoOrbitPairs to sequences of TwoOrbitPairs.
[<CustomEquality; NoComparison>]
type TwoOrbitUfStep = private { twoOrbitTypes: TwoOrbitType array; order: int } with
    /// The TwoOrbitUfStep specifies the transformation of a group of TwoOrbits into another
    /// group of TwoOrbits (of twice the size and twice the order), via reflection and further
    /// modification according to the list of TwoOrbitTypes
    /// <param name="twoOrbitTypes">The list of TwoOrbitType values (must be non-empty).</param>
    /// <param name="order">The order TwoOrbits the Perm_RsUnfolderStep acts upon.</param>
    /// <exception cref="System.ArgumentException">Thrown when order is negative or the type types is empty.</exception>
    static member create (twoOrbitTypes: TwoOrbitType array) (order: int) : TwoOrbitUfStep =
        if order < 4 then
            failwith "TwoOrbitUfStep order must be at least 4"
        if order % 2 <> 0 then
            failwith "TwoOrbitUfStep order must be even"
        if Array.length twoOrbitTypes <> order / 2 then
            failwith $"TwoOrbitUfStep twoOrbitTypes length must be order / 2 ({order / 2})"
        { twoOrbitTypes = twoOrbitTypes; order = order }

    /// Determines whether this instance equals another Perm_RsUnfolderStep.
    override this.Equals (obj: obj) =
        match obj with
        | :? TwoOrbitUfStep as other -> this.twoOrbitTypes = other.twoOrbitTypes && this.Order = other.Order
        | _ -> false

    member this.Order
        with get () = this.order

    member this.TwoOrbitTypes
        with get () = this.twoOrbitTypes

    /// Computes the hash code for this instance.
    override this.GetHashCode () =
        hash (this.GetType(), this.twoOrbitTypes, this.Order)

    /// Returns a string representation of the TwoOrbitUnfol der.
    override this.ToString () =
        sprintf "TwoOrbitUfStep(TwoOrbitTypes=%A, Order=%d)" this.twoOrbitTypes this.Order


module TwoOrbitUfStep =

    /// creates an ordered list of TwoOrbitPairs, the same length as the TwoOrbit list
    /// provided, and twice the order of the given Perm_RsUnfolderStep.
    let unfoldTwoOrbits
        (twoOrbitUfStep:TwoOrbitUfStep) 
        (twoOrbits:TwoOrbit array) : TwoOrbitPair list =

        let zippy = twoOrbitUfStep.twoOrbitTypes |> Array.zip twoOrbits
        [  
            for zip in zippy do
                    unfoldTwoOrbitIntoTwoOrbitPair
                        (fst zip)
                        twoOrbitUfStep.Order
                        (snd zip)
        ]

    /// creates an ordered list of TwoOrbitPairs, equal in length to 
    /// twoOrbitUnfolder.TwoOrbitTypes, and twice the order of the given Perm_RsUnfolderStep.
    let unfoldTwoOrbitPairs
        (twoOrbitUfStep:TwoOrbitUfStep) 
        (twoOrbitPairs:TwoOrbitPair array) : TwoOrbitPair array =

        let twoOrbits = twoOrbitPairs |> TwoOrbitPairOps.makeTwoOrbits |> Seq.toArray
        let zippy = twoOrbitUfStep.twoOrbitTypes |> Array.zip twoOrbits
        [|
            for zip in zippy do
                    unfoldTwoOrbitIntoTwoOrbitPair
                        (fst zip)
                        twoOrbitUfStep.Order
                        (snd zip)
        |]



    
