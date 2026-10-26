namespace GeneSort.Dispatch.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage

/// Dispatch supplies its source-aware generator; Project owns preparation and publication.
module ParamOps =
    let initRunAndParamFiles db progress configured (minReplica: int<replNumber>) (maxReplica: int<replNumber>) (allowOverwrite: bool<allowOverwrite>) makeRunParameters =
        RunPreparation.initializeAsync db progress {
            Run = configured
            FirstReplica = int minReplica
            ReplicaCount = int maxReplica - int minReplica
        } (if %allowOverwrite then runParameterWriteMode.Replace else runParameterWriteMode.CreateNew) makeRunParameters

    let resumeRunAndParamFiles db progress configured (minReplica: int<replNumber>) (maxReplica: int<replNumber>) makeRunParameters =
        RunPreparation.initializeAsync db progress {
            Run = configured
            FirstReplica = int minReplica
            ReplicaCount = int maxReplica - int minReplica
        } runParameterWriteMode.Resume makeRunParameters
