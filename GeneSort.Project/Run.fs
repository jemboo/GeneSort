
namespace GeneSort.Project

// Run type
type run = 
    private
        { index: int
          repl: int<replNumber>
          runParameters: runParameters }

    with

    static member create 
            (index: int) 
            (repl: int<replNumber>) 
            (runParameters: runParameters) : run =
        if index < 0 then
            failwith "Run index cannot be negative"
        else
            { index = index
              repl = repl
              runParameters = runParameters }

    member this.Index with get() = this.index
    member this.Repl with get() = this.repl
    member this.RunParameters with get() = this.runParameters


// Run type
type run2 = 
    private
        { index: int
          runParameters: runParameters }

    with

    static member create 
            (index: int) 
            (runParameters: runParameters) : run2 =
        if index < 0 then
            failwith "Run index cannot be negative"
        else
            { index = index
              runParameters = runParameters }

    member this.Index with get() = this.index
    member this.RunParameters with get() = this.runParameters
    member this.Repl with get() = this.RunParameters.GetRepl()
