namespace GeneSort.Dispatch.V1
open System
open System.IO
open System.Runtime
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.FileDb.V1
open GeneSort.Dispatch.V1


module DispatchLib = 

    let getQueryParamMaker = None