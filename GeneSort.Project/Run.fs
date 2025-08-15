
namespace GeneSort.Project


open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.IO
open System.Threading
open System.Threading.Tasks

// Event type for Run completion
//type RunCompletedEventArgs = { Index: int }

// Run type
type Run = 
    { Index: int
      Parameters: Map<string, string> }
