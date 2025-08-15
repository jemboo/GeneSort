
namespace GeneSort.Project

open System
open FSharp.UMX
open GeneSort.Sorter
open MessagePack
open MessagePack.FSharp
open MessagePack.Resolvers
open System.IO
open GeneSort.Core.Combinatorics
open System.Threading.Tasks



[<MessagePackObject>]
type WorkspaceDto = 
    { 
        [<MessagePack.Key("Name")>] Name: string
        [<MessagePack.Key("Description")>]  Description: string
        [<MessagePack.Key("RootDirectory")>] RootDirectory: string
        [<MessagePack.Key("ParameterSets")>] ParameterSets: list<string * list<string>>
    }


module WorkspaceDto =  

    let toWorkspaceDto (workspace: Workspace) : WorkspaceDto =
        { Name = workspace.Name
          Description = workspace.Description
          RootDirectory = workspace.RootDirectory
          ParameterSets = workspace.ParameterSets }

    let fromWorkspaceDto (dto: WorkspaceDto) : Workspace =
        Workspace.create
          dto.Name
          dto.Description
          dto.RootDirectory
          dto.ParameterSets

