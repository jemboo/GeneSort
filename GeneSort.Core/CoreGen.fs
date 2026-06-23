namespace GeneSort.Core
open Permutation

module CoreGen =


    let randomPermutations (order:int) (randy:IRando) : seq<permutation> =
        seq {
                while true do
                    yield randomPermutation (fun n -> randy.NextIndex n) order
            }


    let randomSaturatedPerm_Si (order:int) (randy:IRando) : seq<permSi> =
        let baseTwoCycle = PermSi.saturatedWithTwoCycles order
        seq {
                while true do
                    let conj = randomPermutation (fun n -> randy.NextIndex n) order
                    yield PermSi.conjugate baseTwoCycle conj
            }

            
    let randomUnSaturatedPerm_Si (order:int) (cycleCount:int) (randy:IRando) : seq<permSi> =
        let baseTwoCycle = PermSi.unSaturatedWithTwoCycles order cycleCount
        seq {
                while true do
                    let conj = randomPermutation (fun n -> randy.NextIndex n) order
                    yield PermSi.conjugate baseTwoCycle conj
            }


    let randomUnSaturatedPerm_Rs (order:int) (randy:IRando) : seq<permSi> =
        randomPermutations order randy 
                |> Seq.map (fun p -> p |> PermRsGen_old.decodePermutation)


    let randomSaturatedPerm_Rs (order:int) (randy:IRando) : seq<permSi> =
        seq {
                while true do
                    yield PermRsGen_old.rndSymmetric order randy
            }
