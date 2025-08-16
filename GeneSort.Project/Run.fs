
namespace GeneSort.Project


open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.IO
open System.Threading
open System.Threading.Tasks

[<Measure>] type cycleNumber

// Run type
type Run = 
    { Index: int
      Cycle: int<cycleNumber>
      Parameters: Map<string, string> }
