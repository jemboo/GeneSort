namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open FsToolkit.ErrorHandling


type simpleMutatorCase =
    | Msce of excludeSelfCe: bool<excludeSelfCe>
            * mutationRate: float<mutationRate>
            * insertionRate: float<insertionRate>
            * deletionRate: float<deletionRate>

    | Mssi of excludeSelfCe: bool<excludeSelfCe>
            * orthoRate: float<orthoRate>
            * paraRate: float<paraRate>

    | Msrs of excludeSelfCe: bool<excludeSelfCe>
            * orthoRate: float<orthoRate>
            * paraRate: float<paraRate>
            * selfSymRate: float<selfSymRate>

    | Msuf4 of excludeSelfCe: bool<excludeSelfCe>
             * seedModificationRate: float<seedModificationRate>
             * orthoRate: float<orthoRate>
             * paraRate: float<paraRate>
             * selfSymRate: float<selfSymRate>

    | Msuf6 of excludeSelfCe: bool<excludeSelfCe>
             * seedModificationRate: float<seedModificationRate>
             * orthoRate: float<orthoRate>
             * paraRate: float<paraRate>
             * selfSymRate: float<selfSymRate>


type simpleMutatorParams = 
    { sortingWidth: int<sortingWidth>
      rngType: rngType
      mutCase: simpleMutatorCase }

    static member create (sortingWidth: int<sortingWidth>) (rngType: rngType) (case: simpleMutatorCase) =
        { sortingWidth = sortingWidth; rngType = rngType; mutCase = case }

    member this.SortingWidth = this.sortingWidth
    member this.RngType = this.rngType
    member this.Case = this.mutCase



module SimpleMutatorParams =

    let parseBool (s: string) = 
        match System.Boolean.TryParse s with
        | true, b -> Ok (b |> UMX.tag)
        | false, _ -> Error $"Invalid boolean value: '{s}'"

    let parseFloat (s: string) = 
        match System.Double.TryParse s with
        | true, f -> Ok f
        | false, _ -> Error $"Invalid float value: '{s}'"

    let parseInt (s: string) = 
        match System.Int32.TryParse s with
        | true, i -> Ok i
        | false, _ -> Error $"Invalid integer value: '{s}'"


    let private caseToString (case: simpleMutatorCase) : string =
        match case with
        | Msce (excludeSelfCe, mutationRate, insertionRate, deletionRate) ->
            sprintf "msce:%b:%.6f:%.6f:%.6f" 
                (%excludeSelfCe) (%mutationRate) (%insertionRate) (%deletionRate)

        | Mssi (excludeSelfCe, orthoRate, paraRate) ->
            sprintf "mssi:%b:%.6f:%.6f" 
                (%excludeSelfCe) (%orthoRate) (%paraRate)

        | Msrs (excludeSelfCe, orthoRate, paraRate, selfSymRate) ->
            sprintf "msrs:%b:%.6f:%.6f:%.6f" 
                (%excludeSelfCe) (%orthoRate) (%paraRate) (%selfSymRate)

        | Msuf4 (excludeSelfCe, seedModificationRate, orthoRate, paraRate, selfSymRate) ->
            sprintf "msuf4:%b:%.6f:%.6f:%.6f:%.6f" 
                (%excludeSelfCe) (%seedModificationRate) (%orthoRate) (%paraRate) (%selfSymRate)

        | Msuf6 (excludeSelfCe, seedModificationRate, orthoRate, paraRate, selfSymRate) ->
            sprintf "msuf6:%b:%.6f:%.6f:%.6f:%.6f" 
                (%excludeSelfCe) (%seedModificationRate) (%orthoRate) (%paraRate) (%selfSymRate)


    let toString (params': simpleMutatorParams) : string =
        sprintf "%d:%s:%s" 
            (%params'.sortingWidth) 
            (RngType.toString params'.RngType) 
            (caseToString params'.Case)


    let private parseCase (tokens: string list) : Result<simpleMutatorCase, string> =
        match tokens with
        | "msce" :: exclude :: mut :: ins :: del :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! mutationRate = parseFloat mut
                let! insertionRate = parseFloat ins
                let! deletionRate = parseFloat del
                return Msce (
                    excludeSelfCe, 
                    mutationRate |> UMX.tag, 
                    insertionRate |> UMX.tag, 
                    deletionRate |> UMX.tag
                )
            }

        | "mssi" :: exclude :: ortho :: para :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                return Mssi (excludeSelfCe, 
                             orthoRate |> UMX.tag, 
                             paraRate |> UMX.tag)
            }

        | "msrs" :: exclude :: ortho :: para :: selfSym :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                let! selfSymRate = parseFloat selfSym
                return Msrs (excludeSelfCe, 
                             orthoRate |> UMX.tag, 
                             paraRate |> UMX.tag, 
                             selfSymRate |> UMX.tag)
            }

        | "msuf4" :: exclude :: seedMod :: ortho :: para :: selfSym :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! seedModificationRate = parseFloat seedMod
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                let! selfSymRate = parseFloat selfSym
                return Msuf4 (excludeSelfCe, 
                              seedModificationRate |> UMX.tag, 
                              orthoRate |> UMX.tag, 
                              paraRate |> UMX.tag, 
                              selfSymRate |> UMX.tag)
            }

        | "msuf6" :: exclude :: seedMod :: ortho :: para :: selfSym :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! seedModificationRate = parseFloat seedMod
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                let! selfSymRate = parseFloat selfSym
                return Msuf6 (excludeSelfCe, 
                              seedModificationRate |> UMX.tag, 
                              orthoRate |> UMX.tag, 
                              paraRate |> UMX.tag, 
                              selfSymRate |> UMX.tag)
            }

        | _ -> Error $"""Unrecognized or malformed simple mutator case string: '{String.concat ":" tokens}'"""


    let fromString (str: string) : Result<simpleMutatorParams, string> =
        match str.Split(':') |> Array.toList with
        | widthStr :: rngStr :: caseTokens when caseTokens.Length > 0 ->
            result {
                let! rawWidth = parseInt widthStr
                let sortingWidth = rawWidth |> UMX.tag
                let! rngTypeVal = 
                    try Ok (RngType.fromString rngStr) 
                    with ex -> Error ex.Message
                let! caseVal = parseCase caseTokens
                return simpleMutatorParams.create sortingWidth rngTypeVal caseVal
            }
        | _ -> Error $"Unrecognized or malformed simpleMutatorParams string: '{str}'"


    let toModelMutator 
            (modificationRate: float<modificationRate>) 
            (params': simpleMutatorParams) : simpleSorterModelMutator =
        let rngFactory = RngFactory.create params'.rngType
        let sortingWidth = params'.sortingWidth
        match params'.Case with
        | Msce (excludeSelfCe, mutationRate, insertionRate, deletionRate) ->
            SimpleSorterModelMutator.getMsceModelMutator 
                rngFactory excludeSelfCe modificationRate mutationRate insertionRate deletionRate

        | Mssi (excludeSelfCe, orthoRate, paraRate) ->
            SimpleSorterModelMutator.getMssiModelMutator 
                rngFactory excludeSelfCe modificationRate orthoRate paraRate

        | Msrs (excludeSelfCe, orthoRate, paraRate, selfSymRate) ->
            SimpleSorterModelMutator.getMsrsModelMutator 
                rngFactory excludeSelfCe modificationRate orthoRate paraRate selfSymRate

        | Msuf4 (excludeSelfCe, seedModificationRate, orthoRate, paraRate, selfSymRate) ->
            SimpleSorterModelMutator.getMsuf4ModelMutator 
                sortingWidth rngFactory excludeSelfCe seedModificationRate modificationRate orthoRate paraRate selfSymRate

        | Msuf6 (excludeSelfCe, seedModificationRate, orthoRate, paraRate, selfSymRate) ->
            SimpleSorterModelMutator.getMsuf6ModelMutator 
                sortingWidth rngFactory excludeSelfCe seedModificationRate modificationRate orthoRate paraRate selfSymRate



type mutatorParams =
    | SimpleMutatorParams of simpleMutatorParams
    | ComplexMutatorParams of float


module MutatorParams =

    let SimplePrefix = "smp:"

    let ComplexPrefix = "cmp:"

    let toString (params': mutatorParams) : string =
        match params' with
        | SimpleMutatorParams smp -> sprintf "%s%s" SimplePrefix (SimpleMutatorParams.toString smp)
        | ComplexMutatorParams c -> sprintf "%s%.6f" ComplexPrefix c

    let fromString (str: string) : Result<mutatorParams, string> =
        if System.String.IsNullOrWhiteSpace(str) then
            Error "Cannot parse mutatorParams from empty string."
        elif str.StartsWith(SimplePrefix) then
            let payload = str.Substring(SimplePrefix.Length)
            SimpleMutatorParams.fromString payload
            |> Result.map SimpleMutatorParams
        elif str.StartsWith(ComplexPrefix) then
            let payload = str.Substring(ComplexPrefix.Length)
            match SimpleMutatorParams.parseFloat payload with
            | Ok v -> Ok (ComplexMutatorParams v)
            | Error e -> Error $"Failed to parse ComplexMutatorParams payload: {e}"
        else
            Error $"Unrecognized mutatorParams prefix: '{str}'"


    let toModelMutator 
            (modificationRate: float<modificationRate>) 
            (params': mutatorParams) : sorterModelMutator =
        match params' with
        | SimpleMutatorParams smp -> 
            let simpleMutator = SimpleMutatorParams.toModelMutator modificationRate smp
            sorterModelMutator.Simple simpleMutator
        | ComplexMutatorParams _ -> 
            failwith "Complex mutator parameters are not yet implemented."


    let msceParamsR10 (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msce (true |> UMX.tag, 1.01 |> UMX.tag<mutationRate>, 0.1 |> UMX.tag<insertionRate>, 0.1 |> UMX.tag<deletionRate>))
        |> SimpleMutatorParams

    let msceParamsR5 (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msce (true |> UMX.tag, 1.01 |> UMX.tag<mutationRate>, 0.2 |> UMX.tag<insertionRate>, 0.2 |> UMX.tag<deletionRate>))
        |> SimpleMutatorParams

    let mssiParamsRL (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Mssi (true |> UMX.tag, 
                                              1.01 |> UMX.tag<orthoRate>, 
                                              1.01 |> UMX.tag<paraRate>))
        |> SimpleMutatorParams

    let mssiParamsRM (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Mssi (true |> UMX.tag, 
                                              2.01 |> UMX.tag<orthoRate>, 
                                              1.01 |> UMX.tag<paraRate>))
        |> SimpleMutatorParams

    let mssiParamsRH (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Mssi (true |> UMX.tag, 
                                              4.01 |> UMX.tag<orthoRate>, 
                                              1.01 |> UMX.tag<paraRate>))
        |> SimpleMutatorParams


    let mssiParamsRVH (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Mssi (true |> UMX.tag, 
                                              6.01 |> UMX.tag<orthoRate>, 
                                              1.01 |> UMX.tag<paraRate>))
        |> SimpleMutatorParams

    let msrsParamsRL (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msrs (true |> UMX.tag, 
                                                    1.0 |> UMX.tag<orthoRate>, 
                                                    1.0 |> UMX.tag<paraRate>, 
                                                    1.0 |> UMX.tag<selfSymRate>))
        |> SimpleMutatorParams

    let msrsParamsRM (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msrs (true |> UMX.tag, 
                                                    4.01 |> UMX.tag<orthoRate>, 
                                                    0.4 |> UMX.tag<paraRate>, 
                                                    2.01 |> UMX.tag<selfSymRate>))
        |> SimpleMutatorParams

    let msrsParamsRH (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msrs (true |> UMX.tag, 
                                                    6.5 |> UMX.tag<orthoRate>, 
                                                    0.3 |> UMX.tag<paraRate>, 
                                                    1.5 |> UMX.tag<selfSymRate>))
        |> SimpleMutatorParams



    let msuf4ParamsRL (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msuf4 (true |> UMX.tag, 
                                                     0.05 |> UMX.tag<seedModificationRate>, 
                                                     3.51 |> UMX.tag<orthoRate>, 
                                                     0.4 |> UMX.tag<paraRate>, 
                                                     2.01 |> UMX.tag<selfSymRate>))
        |> SimpleMutatorParams

    let msuf4ParamsRC (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msuf4 (true |> UMX.tag, 
                                                     0.05 |> UMX.tag<seedModificationRate>, 
                                                     4.01 |> UMX.tag<orthoRate>, 
                                                     0.4 |> UMX.tag<paraRate>, 
                                                     2.01 |> UMX.tag<selfSymRate>))
        |> SimpleMutatorParams

    let msuf4ParamsRH (width: int<sortingWidth>) (rng: rngType) : mutatorParams =
        simpleMutatorParams.create width rng (Msuf4 (true |> UMX.tag, 
                                                     0.05 |> UMX.tag<seedModificationRate>, 
                                                     4.51 |> UMX.tag<orthoRate>, 
                                                     0.4 |> UMX.tag<paraRate>, 
                                                     2.01 |> UMX.tag<selfSymRate>))
        |> SimpleMutatorParams
