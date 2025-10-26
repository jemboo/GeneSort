﻿
namespace GeneSort.Db

open FSharp.UMX
open GeneSort.Sorter.Sortable
open GeneSort.Sorter.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Sortable
open GeneSort.SortingOps
open GeneSort.SortingResults
open GeneSort.Runs.Params
open GeneSort.Runs

type queryParams =
    private {
        projectName: string<projectName>
        index: int<indexNumber> option
        repl: int<replNumber> option
        generation: int<generationNumber> option
        outputDataType: outputDataType
    }
    member this.ProjectName with get() = this.projectName
    member this.Index with get() = this.index
    member this.Repl with get() = this.repl
    member this.Generation with get() = this.generation
    member this.OutputDataType with get() = this.outputDataType
    
    static member Create(
            projectName: string<projectName>, 
            index: int<indexNumber> option, 
            repl: int<replNumber> option, 
            generation: int<generationNumber> option, 
            outputDataType: outputDataType) : queryParams =
        {
            projectName = projectName
            index = index
            repl = repl
            generation = generation
            outputDataType = outputDataType
        }
    
    static member CreateForProject(projectName: string<projectName>) : queryParams =
        {
            projectName = projectName
            index = None
            repl = None
            generation = None
            outputDataType = outputDataType.Project
        }
