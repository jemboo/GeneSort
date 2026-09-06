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
    | Uniform of qpSorterType
    | Variable of qpSorterType


type qpSorterPoolStructure =
    | Singleton
    | Multiple
    | Tiled


type qpSorterPoolType =
    | Random of qpSorterPoolStructure * qpSorterEval
    | Sgd of qpSorterPoolStructure * qpSgdType

and qpSgdType =
    | FixedPools of qpSorterPoolType * qpSorterPoolType * qpSorterMutate


type queryParamType =
    | SortableTest of qpSortableTestType
    | SorterEval of qpSorterEval
    | SorterMutate of qpSorterMutate
    | SorterSgd of qpSgdType

 


module QueryParamType = 

    let getQueryParamMaker = None