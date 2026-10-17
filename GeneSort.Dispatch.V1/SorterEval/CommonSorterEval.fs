namespace GeneSort.Dispatch.V1.SorterEval

open GeneSort.Project.V1.Catalogs.Evolution
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Dispatch.V1.CommonParams


module CommonSorterEval =

    let getSimpleUniformSorterModelGen = SeedSorterModelGeneration.getSimpleUniformSorterModelGen
