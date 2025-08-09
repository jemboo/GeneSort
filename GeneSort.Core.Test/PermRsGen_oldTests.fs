namespace GeneSort.Core.Test
open FSharp.UMX
open Xunit
open GeneSort.Core.CoreGen
open GeneSort.Core
open FsUnit.Xunit

type TwoCycleGenTests() =

    let parseIntArray (s: string) : int[] =
        s.Split(',')
        |> Array.map (fun n -> int (n.Trim()))

    [<Theory>]
    [<InlineData("1,0,3,2", "1,0,3,2")>]
    [<InlineData("1,0,2,4,3", "1,0,2,4,3")>]
    [<InlineData("4,1,0,3,5,2", "4,5,2,3,0,1")>]
    [<InlineData("4,1,2,7,6,5,0,3", "4,1,2,7,0,5,6,3")>]
    let ``decodePermutations`` (code: string, expected: string) =
        let codeA = parseIntArray code
        let expectedDecodeA = parseIntArray expected
        let permOfCode = Permutation.create codeA
        let perm_RsRSA = PermRsGen_old.decodePermutation permOfCode
        perm_RsRSA.Array |> should equal expectedDecodeA

    [<Fact>]
    let ``decoded TwoCycleRS orbit stats made from random Permutations`` () =
        let permSize = 512
        let permCount = 1000
        let perms = randomPermutations permSize (new randomLcg (9234UL |> UMX.tag<randomSeed>))
                    |> Seq.take permCount |> Seq.toArray
        let decodedTwoCycleRSAs = 
                perms 
                |> Array.map(PermRsGen_old.decodePermutation)


        //let orbitCounts = 
        //        decodedTwoCycleRSAs
        //        |> Array.map(fun tsRS -> tsRS.Perm_Rs |> Perm_Rs.countOrbits)


        //let ocHisto =
        //        orbitCounts
        //           |> Array.groupBy(id)
        //           |> Array.sortBy (fst)
        //           |> Array.map (fun (k, arr) -> (k, arr.Length))


        //let pos7histo = 
        //           decodedTwoCycleRSAs |> Array.map(fun dc -> dc.Array.[6])
        //           |> Array.groupBy(id)
        //           |> Array.sortBy (fst)
        //           |> Array.map (fun (k, arr) -> (k, arr.Length))


        1 |> should equal 1


    [<Fact>]
    let ``all the orbits made by TwoCycleRS rndSymmetric are RS (for even order)`` () =
        let permSize = 32
        let perm_RsCount = 10
        let randy = new randomLcg (9234UL |> UMX.tag<randomSeed>)
        let perm_RsRSAs = Array.init perm_RsCount (fun _ -> PermRsGen_old.rndSymmetric permSize randy)

        1|> should equal 1