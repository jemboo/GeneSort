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
                match sorterType with
                | qpSorterType.Simple _ ->
                    match testType with
                    | qpSortableTestType.Standard r ->
                        match r with
                        | NoRestriction ->
                            let keys = [ runParameters.rngTypeKey; 
                                         runParameters.sorterEvalTypeKey; 
                                         runParameters.simpleSorterModelTypeKey;
                                         runParameters.sortingWidthKey;]
                            tryExtractKeys keys props
                        | Split ->
                            failwith "Standard sortable test type with Split restriction is not supported."

                    | qpSortableTestType.Merge r ->
                        match r with
                        | NoRestriction ->
                            let keys = [ runParameters.rngTypeKey;
                                         runParameters.sorterEvalTypeKey; 
                                         runParameters.simpleSorterModelTypeKey;
                                         runParameters.sortableDataFormatKey; 
                                         runParameters.mergeLibIdKey ]
                            tryExtractKeys keys props
                        | Split ->
                            failwith "Merge sortable test type with Split restriction is not supported."

                    | qpSortableTestType.Prefix r ->
                        match r with
                        | NoRestriction ->
                            let keys = [ runParameters.rngTypeKey;
                                         runParameters.sorterEvalTypeKey; 
                                         runParameters.simpleSorterModelTypeKey;
                                         runParameters.sortableDataFormatKey; 
                                         runParameters.prefixLibIdKey ]
                            tryExtractKeys keys props
                        | Split ->
                            failwith "Prefix sortable test type with Split restriction is not supported."

                | _ -> failwith "Only Simple sorter type is supported for property map filtering."


        | SorterMutate sm -> 
            match sm with
            | qpSorterMutate.Uniform se ->
                match se with
                | qpSorterEval.Standard (sorterType, testType) ->
                    match sorterType with
                    | qpSorterType.Simple _ ->
                        match testType with
                        | qpSortableTestType.Standard r ->
                            match r with
                            | NoRestriction ->
                                let keys = [ runParameters.rngTypeKey;
                                             runParameters.simpleMutatorParamsKey;
                                             runParameters.sorterEvalTypeKey; 
                                             runParameters.seedSorterPoolSelectionTypeKey;
                                             runParameters.simpleSorterModelTypeKey;
                                             runParameters.sortingWidthKey;]
                                tryExtractKeys keys props
                            | Split ->
                                failwith "Standard sortable test type with Split restriction is not supported."

                        | qpSortableTestType.Merge r ->
                            match r with
                            | NoRestriction ->
                                let keys = [ runParameters.rngTypeKey;
                                             runParameters.simpleMutatorParamsKey;
                                             runParameters.sorterEvalTypeKey;
                                             runParameters.seedSorterPoolSelectionTypeKey;
                                             runParameters.simpleSorterModelTypeKey;
                                             runParameters.sortableDataFormatKey; 
                                             runParameters.mergeLibIdKey ]
                                tryExtractKeys keys props
                            | Split ->
                                failwith "Merge sortable test type with Split restriction is not supported."

                        | qpSortableTestType.Prefix r ->
                            match r with
                            | NoRestriction ->
                                let keys = [ runParameters.rngTypeKey;
                                             runParameters.simpleMutatorParamsKey;
                                             runParameters.sorterEvalTypeKey; 
                                             runParameters.seedSorterPoolSelectionTypeKey;
                                             runParameters.simpleSorterModelTypeKey;
                                             runParameters.sortableDataFormatKey; 
                                             runParameters.prefixLibIdKey ]
                                tryExtractKeys keys props
                            | Split ->
                                failwith "Prefix sortable test type with Split restriction is not supported."

                    | _ -> failwith "Only Simple sorter type is supported for property map filtering."

            | qpSorterMutate.Variable se ->
                 failwith "Variable sorter mutate is not supported for property map filtering."

        | SorterSgd sgd -> 
            failwith "SorterSgd is not supported for property map filtering."