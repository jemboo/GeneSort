namespace GeneSort.Project.V1.Parameters

open GeneSort.Core

type setupParameterKind = private | SetupParameterKind

/// Parameters used when preparing a run's setup data.
type setupParameters = parameterSet<setupParameterKind>

module SetupParameters =

    /// Filters a set of parameters based on a list of key-value pairs.
    let filterByParameters (setupParametersSet: setupParameters array) (filter: (string * string) array) =
        setupParametersSet
        |> Array.filter (fun rp ->
            filter |> Array.forall (fun (k, v) -> rp.ParamMap.TryFind k = Some v))

    let pickByParameters (setupParametersSet: setupParameters array) (filter: (string * string) array) =
        let results = filterByParameters setupParametersSet filter
        if results.Length = 1 then Some results.[0] else None

    let getAllKeys (setupParams: setupParameters seq) =
        setupParams |> Seq.collect (fun rp -> rp.ParamMap.Keys) |> Seq.distinct |> Seq.toArray

    let reportKvps (setupParameters: setupParameters) : string =
        let concats =
            setupParameters.ParamMap.Keys
            |> Seq.map(fun k -> sprintf "\t%s: %s" k (setupParameters.ParamMap.[k]))
            |> String.concat "\n"

        concats + "\n"


    /// Generates a structured table where rows represent individual setups and columns represent parameter keys.
    let makeIndexAndReplTable (setupParams: setupParameters seq) : string[][] =
        let keys = getAllKeys setupParams
        let headerRow = Array.append [| "Setup_Index" |] keys
        let dataRows =
            setupParams
            |> Seq.mapi (fun i rp ->
                Array.append
                    [| string i |]
                    (keys |> Array.map (fun k -> rp.ParamMap.TryFind k |> Option.defaultValue "N/A")))
            |> Seq.toArray
        Array.append [| headerRow |] dataRows

    let toStringTable (setupParams: setupParameters seq) : string =
        makeIndexAndReplTable setupParams
        |> Array.map (String.concat "\t")
        |> String.concat "\n\n"

    let getIdString (setupParameters:setupParameters) =
        setupParameters.GetId() |> UmxExt.guidOptionToString
