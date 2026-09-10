namespace GeneSort.Project.V1


type qpSortableTestRestriction =
    | NoRestriction
    | Split


type qpSortableTestType =
    | Standard of qpSortableTestRestriction
    | Merge of qpSortableTestRestriction
    | Prefix of qpSortableTestRestriction


type qpSimpleSorter =
    | Msce
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6
    

type qpSorterType =
    | Simple of qpSimpleSorter
    | Gated of qpSimpleSorter
    | Dual of qpSimpleSorter


type qpSorterEval =
    | Standard of qpSorterType * qpSortableTestType


type qpSorterMutate =
    | Uniform of qpSorterEval
    | Variable of qpSorterEval


type qpSorterPoolStructure =
    | Singleton
    | Multiple
    | Tiled


type qpSorterPoolType =
    | Random of qpSorterPoolStructure * qpSorterEval
    | Sgd of qpSorterPoolStructure * qpSgdType

and qpSgdType =
    | FixedPools of qpSorterPoolType * qpSorterPoolType * qpSorterMutate


type queryProperties =
    | SortableTest of qpSortableTestType
    | SorterEval of qpSorterEval
    | SorterMutate of qpSorterMutate
    | SorterSgd of qpSgdType

 


module QueryProperties =

    /// Attempts to extract all specified keys from the map.
    /// Returns None if ANY key is missing.
    let private tryExtractKeys (requiredKeys: string seq) (props: Map<string, string>) : Map<string, string> option =
        let folder acc key =
            match acc, props.TryFind key with
            | Some m, Some v -> Some (m |> Map.add key v)
            | _ -> None
            
        requiredKeys |> Seq.fold folder (Some Map.empty)

    let filterPropertyMap (qp: queryProperties) (props: Map<string, string>) : Map<string, string> option =
        match qp with
        | SortableTest st -> 
            match st with
            | qpSortableTestType.Standard restriction ->
                failwith "Standard sortable test type is not supported."

            | qpSortableTestType.Merge restriction ->
                let keys = [ runParameters.sortingWidthKey; 
                             runParameters.mergeDimensionKey; 
                             runParameters.mergeSuffixTypeKey; 
                             runParameters.sortableDataFormatKey ]
                tryExtractKeys keys props

            | qpSortableTestType.Prefix restriction ->
                let keys = [ runParameters.sorterLibIdKey; 
                             runParameters.sortableDataFormatKey ]
                tryExtractKeys keys props

        | SorterEval se -> 
            match se with
            | qpSorterEval.Standard (sorterType, testType) ->
                match testType with
                | qpSortableTestType.Standard _ ->
                    let keys = [ runParameters.rngTypeKey; 
                                 runParameters.sortingWidthKey; 
                                 runParameters.simpleSorterModelTypeKey; 
                                 runParameters.sorterEvalTypeKey ]
                    tryExtractKeys keys props

                | qpSortableTestType.Merge _ ->
                    let keys = [ runParameters.rngTypeKey;
                                 runParameters.simpleSorterModelTypeKey;
                                 runParameters.sortableDataFormatKey; 
                                 runParameters.sorterEvalTypeKey; 
                                 runParameters.mergeLibIdKey ]
                    tryExtractKeys keys props

                | qpSortableTestType.Prefix _ ->
                    let keys = [ runParameters.rngTypeKey;
                                 runParameters.simpleSorterModelTypeKey; 
                                 runParameters.sortableDataFormatKey; 
                                 runParameters.prefixLibIdKey ]
                    tryExtractKeys keys props

        | SorterMutate sm -> 
            failwith "SorterMutate is not supported for property map filtering."
        | SorterSgd sgd -> 
            failwith "SorterSgd is not supported for property map filtering."