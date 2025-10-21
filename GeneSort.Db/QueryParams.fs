
namespace GeneSort.Db

open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs.Params
open GeneSort.Runs


type queryParams =
     { 
        projectName: string
        index: int option
        repl: int option
        generation: int option
        outputDataType: outputDataType
    }

     
module QueryParams = ()
