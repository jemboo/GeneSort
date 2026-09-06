namespace GeneSort.Dispatch.V1
open System
open FSharp.UMX
open System.Threading
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open System.Runtime
open System.IO
open GeneSort.Core


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


type qpSorterEval =
    | SorterEval of qpSorterType * qpSortableTestType


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
    | FixedPools of qpSorterPoolType * qpSorterPoolType


type queryParamType =
    | SortableTest of qpSortableTestType
    | SorterEval of qpSorterEval
    | SorterMutate of qpSorterMutate
    | SorterSgd of qpSgdType

 


module QueryParamType = 

    let getQueryParamMaker = None