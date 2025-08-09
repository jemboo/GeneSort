namespace GeneSort.Core

open Permutation
module PermutationSets =

    // Get the sequence of powers p, p^2, ..., e (identity)
    let makeCyclicGroup (perm: Permutation) : Permutation seq =
        let arr = perm.Array
        let n = Array.length arr
        let id = identity n
        let rec generatePowers (current: Permutation) acc =
            if current.Array = id.Array then
                seq { yield! acc; yield current }
            else
                let next = compose current perm
                generatePowers next (seq { yield! acc; yield current })
        generatePowers perm Seq.empty
