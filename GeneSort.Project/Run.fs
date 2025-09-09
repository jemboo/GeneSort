
namespace GeneSort.Project


[<Measure>] type cycleNumber

// Run type
type Run = 
    { Index: int
      Cycle: int<cycleNumber>
      mutable Parameters: Map<string, string> }
