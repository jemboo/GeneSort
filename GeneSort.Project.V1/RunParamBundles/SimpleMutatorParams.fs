namespace GeneSort.Project.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open FsToolkit.ErrorHandling


type simpleMutatorParams =
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


module SimpleMutatorParams =

    let private parseBool (s: string) = 
        match System.Boolean.TryParse s with
        | true, b -> Ok (b |> UMX.tag<excludeSelfCe>)
        | false, _ -> Error $"Invalid boolean value: '{s}'"

    let parseFloat (s: string) = 
        match System.Double.TryParse s with
        | true, f -> Ok f
        | false, _ -> Error $"Invalid float value: '{s}'"


    let toString (params': simpleMutatorParams) : string =
        match params' with
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


    let fromString (str: string) : Result<simpleMutatorParams, string> =

        match str.Split(':') |> Array.toList with
        | "msce" :: exclude :: mut :: ins :: del :: [] ->
                result {
                    let! excludeSelfCe = parseBool exclude
                    let! mutationRate = parseFloat mut
                    let! insertionRate = parseFloat ins
                    let! deletionRate = parseFloat del
                    return Msce (
                        excludeSelfCe, 
                        mutationRate |> UMX.tag<mutationRate>, 
                        insertionRate |> UMX.tag<insertionRate>, 
                        deletionRate |> UMX.tag<deletionRate>
                    )
                }

        | "mssi" :: exclude :: ortho :: para :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                return Mssi (excludeSelfCe, 
                             orthoRate |> UMX.tag<orthoRate>, 
                             paraRate |> UMX.tag<paraRate>)
            }

        | "msrs" :: exclude :: ortho :: para :: selfSym :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                let! selfSymRate = parseFloat selfSym
                return Msrs (excludeSelfCe, 
                             orthoRate |> UMX.tag<orthoRate>, 
                             paraRate |> UMX.tag<paraRate>, 
                             selfSymRate |> UMX.tag<selfSymRate>)
            }

        | "msuf4" :: exclude :: seedMod :: ortho :: para :: selfSym :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! seedModificationRate = parseFloat seedMod
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                let! selfSymRate = parseFloat selfSym
                return Msuf4 (excludeSelfCe, 
                              seedModificationRate |> UMX.tag<seedModificationRate>, 
                              orthoRate |> UMX.tag<orthoRate>, 
                              paraRate |> UMX.tag<paraRate>, 
                              selfSymRate |> UMX.tag<selfSymRate>)
            }

        | "msuf6" :: exclude :: seedMod :: ortho :: para :: selfSym :: [] ->
            result {
                let! excludeSelfCe = parseBool exclude
                let! seedModificationRate = parseFloat seedMod
                let! orthoRate = parseFloat ortho
                let! paraRate = parseFloat para
                let! selfSymRate = parseFloat  selfSym
                return Msuf6 (excludeSelfCe, 
                              seedModificationRate |> UMX.tag<seedModificationRate>, 
                              orthoRate |> UMX.tag<orthoRate>, 
                              paraRate |> UMX.tag<paraRate>, 
                              selfSymRate |> UMX.tag<selfSymRate>)
            }

        | _ -> Error $"Unrecognized or malformed simpleMutatorParams string: '{str}'"


    let toModelMutator 
            (sortingWidth: int<sortingWidth>)
            (rngFactory: rngFactory)
            (modificationRate: float<modificationRate>) 
            (params': simpleMutatorParams) : simpleSorterModelMutator =
        match params' with
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