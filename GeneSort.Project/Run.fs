
namespace GeneSort.Project

open System
open GeneSort.Sorter
open GeneSort.Project.Params
open FSharp.UMX
open GeneSort.Sorter.Sortable

// Run type
type run = 
    private
        { index: int
          cycle: int<cycleNumber>
          runParameters: runParameters }

    with

    static member create 
            (index: int) 
            (cycle: int<cycleNumber>) 
            (runParameters: runParameters) : run =
        if index < 0 then
            failwith "Run index cannot be negative"
        else
            { index = index
              cycle = cycle
              runParameters = runParameters }

    member this.Index with get() = this.index
    member this.Cycle with get() = this.cycle
    member this.RunParameters with get() = this.runParameters
