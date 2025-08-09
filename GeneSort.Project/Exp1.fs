
namespace GeneSort.Project


open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System
open System.IO
open System.Threading
open System.Threading.Tasks


module Exp1 =
    
    let parameterSet = 
        [ ("SorterType", ["Mcse"; "Mssi"; "Msrs"; "Msuf6"])
          ("SortingWidth", ["8"; "16"; "32"; "64"]) ]

    //let makeRandGenModel (paramMap: Map<string, string>) : RandGenModel =
    //    { RandGenModel.Width = width
    //      RandGenModel.Seed = 42 // Fixed seed for reproducibility
    //      RandGenModel.Count = 1000 // Number of random elements to generate
    //      RandGenModel.MaxValue = 1000 } // Maximum value for random elements