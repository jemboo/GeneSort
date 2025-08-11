namespace GeneSort.Core

open Combinatorics

/// The seven reflection symmetric order 6 TwoCycles.
type TwoOrbitTripleType =
    | Ortho1
    | Ortho2
    | Para1
    | Para2
    | Para3
    | Para4
    | SelfRefl


module TwoOrbitTripleType =

    // twoOrbit6RS
    let getTwoOrbits (seedType:TwoOrbitTripleType) : TwoOrbit list =
        match seedType with
        | Ortho1 -> [TwoOrbit.create [0; 1]; TwoOrbit.create [4; 5]; TwoOrbit.create [2; 3]]
        | Ortho2 -> [TwoOrbit.create [0; 2]; TwoOrbit.create [3; 5]; TwoOrbit.create [1; 4]]
        | Para1 -> [TwoOrbit.create [0; 3]; TwoOrbit.create [2; 5]; TwoOrbit.create [1; 4]]
        | Para2 -> [TwoOrbit.create [0; 4]; TwoOrbit.create [1; 5]; TwoOrbit.create [2; 3]]
        | Para3 -> [TwoOrbit.create [1; 3]; TwoOrbit.create [2; 4]; TwoOrbit.create [0; 5]]
        | Para4 -> [TwoOrbit.create [0; 3]; TwoOrbit.create [1; 4]; TwoOrbit.create [2; 5]]
        | TwoOrbitTripleType.SelfRefl -> [TwoOrbit.create [0; 5]; TwoOrbit.create [1; 4]; TwoOrbit.create [2; 3]]
