
namespace GeneSort.Project

open FSharp.UMX

// Run type
type run = 
    private
        { index: int<indexNumber>
          runParameters: runParameters }

    with

    static member create 
            (index: int<indexNumber>) 
            (runParameters: runParameters) : run =
        if %index < 0 then
            failwith "Run index cannot be negative"
        else
            { index = index
              runParameters = runParameters }

    member this.Index with get() = this.index
    member this.RunParameters with get() = this.runParameters
    member this.Repl with get() = this.RunParameters.GetRepl()
