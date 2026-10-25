namespace GeneSort.Dispatch.V1.SorterEval

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Model.Sorting.Simple.V1


module CommonSorterEval =

    let getSimpleUniformSorterModelGen = SeedSorterModelGeneration.getSimpleUniformSorterModelGen
