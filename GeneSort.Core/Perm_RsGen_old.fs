namespace GeneSort.Core
open Combinatorics


type orbitRS_old =
    | Single of int * int
    | Unreflectable of int * int
    | Pair of (int * int) * (int * int)
    | LeftOver of int * int


module OrbitRS_old =

    let getIndexes (rfls: orbitRS_old) =
        match rfls with
        | Single (i, j)         ->  seq {i; j }
        | Unreflectable (i, j)  ->  seq { i; j }
        | Pair ((h, i), (j, k)) ->  seq { h; i; j; k }
        | LeftOver (i, j)       ->  seq { i; j }

    let writeToArray (aRet:int array) (rfls: orbitRS_old) =
        match rfls with
        | Single (i, j) ->
            aRet.[i] <- j
            aRet.[j] <- i

        | Unreflectable (i, j) ->
            aRet.[i] <- j
            aRet.[j] <- i

        | Pair ((h, i), (j, k)) ->
            aRet.[i] <- h
            aRet.[h] <- i
            aRet.[j] <- k
            aRet.[k] <- j

        | LeftOver (i, j) ->
            aRet.[i] <- j
            aRet.[j] <- i


    //makes reflective pairs to fill up order slots.
    let rndReflectivePairs (order: int) (rnd: IRando) =

        let flagedArray = Array.init order (fun i -> (i, true))

        let _availableFlags () = flagedArray |> Seq.filter (fun (_, f) -> f)

        let _canContinue () = _availableFlags () |> Seq.length > 1

        let _nextItem () =
            let nItem =  rnd.NextIndex (_availableFlags () |> Seq.length)
            let index = _availableFlags () |> Seq.item (int nItem) |> fst
            flagedArray.[index] <- (index, false)
            index

        let _getReflection (a: int) (b: int) =
            let aR = reflect order a
            let bR = reflect order b

            if (snd flagedArray.[aR]) && (snd flagedArray.[bR]) then
                flagedArray.[aR] <- (aR, false)
                flagedArray.[bR] <- (bR, false)
                Some(bR, aR)
            else
                None

        let _nextItems () =
            let nItemA = _nextItem ()
            let nItemB = _nextItem ()

            if nItemA = (reflect order nItemB) then
                (nItemA, nItemB) |> orbitRS_old.Single
            // if one of the nodes is on the center line, then make a (non-reflective)
            // pair out of them
            else if (nItemA = (reflect order nItemA)) || (nItemB = (reflect order nItemB)) then
                (nItemA, nItemB) |> orbitRS_old.Unreflectable
            else
                let res = _getReflection nItemA nItemB

                match res with
                | Some (reflA, reflB) -> ((nItemA, nItemB), (reflA, reflB)) |> orbitRS_old.Pair
                // if a reflective pair cannot be made from these two, then
                // make them into a (non-reflective) pair
                | None -> (nItemA, nItemB) |> orbitRS_old.LeftOver

        seq {
            while _canContinue () do
                yield _nextItems ()
        }

// This models reflection symmetric sorter steps
module PermRsGen_old =

    // Contains no fixed points, for even order
    let rndSymmetric (order: int) (rnd: IRando) =
        let aRet = Array.init order (id)

        OrbitRS_old.rndReflectivePairs order rnd |> Seq.iter (OrbitRS_old.writeToArray aRet)
        Perm_Si.create aRet


    // Creates a TwoCycleRS, often containing lots of fixed points
    let decodePermutation (permutation: permutation) : Perm_Si =
        let permLength = permutation.Array.Length
        let availableFlags = Array.init permLength (fun i -> true)
        let arrayRS = Array.init<int> permLength (fun i -> i)

        let _getReflections (a:int) (b:int) : seq<int*int> =
            if (availableFlags.[a] && availableFlags.[b]) then
                if b = reflect permLength a then
                  availableFlags.[a] <- false
                  availableFlags.[b] <- false
                  seq {(a, b)}
                else
                  let ar = reflect permLength a
                  let br = reflect permLength b
                  if (availableFlags.[ar] && availableFlags.[br]) then
                      availableFlags.[a] <- false
                      availableFlags.[b] <- false
                      availableFlags.[ar] <- false
                      availableFlags.[br] <- false
                      seq {(a, b); (ar, br)}
                  else
                      Seq.empty
            else
                Seq.empty

        for dex = 0 to permLength - 1 do
            let pairs = _getReflections dex permutation.Array.[dex]
            for (a, b) in pairs do
                arrayRS.[a] <- b
                arrayRS.[b] <- a

        let perm_Rs = Perm_Si.createUnsafe arrayRS
        if not (Permutation.isSelfInverse perm_Rs.Permutation) then
            failwith "Invalid TwoCycleRS: permutation must be self-inverse"
        if not (Perm_Si.isReflectionSymmetric perm_Rs) then
            failwith "Invalid TwoCycleRS: permutation must be reflection-symmetric"
        Perm_Si.createUnsafe perm_Rs.Array


