namespace GeneSort.Model.Sorter.Uf4

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open MathUtils
open GeneSort.Core.Combinatorics
open GeneSort.Model.Sorter


/// Represents a configuration for mutating Msuf4 instances with specified mutation probabilities.
[<Struct; CustomEquality; NoComparison>]
type Msuf4RandMutate = 
    private 
        { rngType: rngType
          stageCount: int<stageCount>
          probabilities: Msuf4MutationModeProbabilities array
          pickers: Lazy<((unit -> float) -> Uf4MutationRates option)[]> } 
    static member create 
            (rngType: rngType)
            (stageCount: int<stageCount>)
            (order: int)
            (orthoToParaRates: float<uf4OrthoToParaMutationRate>[]) 
            (orthoToSelfReflRates: float<uf4OrthoToSelfReflMutationRate>[]) 
            (paraToOrthoRates: float<uf4ParaToOrthoMutationRate>[]) 
            (paraToSelfReflRates: float<uf4ParaToSelfReflMutationRate>[]) 
            (selfReflToOrthoRates: float<uf4SelfReflToOrthoMutationRate>[]) 
            (selfReflToParaRates: float<uf4SelfReflToParaMutationRate>[]) 
            : Msuf4RandMutate =
        if rngType = Unchecked.defaultof<rngType> then
            failwith "rngType must be specified"
        else if %stageCount < 1 then
            failwith $"StageCount must be at least 1, got {%stageCount}"
        else if orthoToParaRates.Length <> %stageCount || 
                orthoToSelfReflRates.Length <> %stageCount || 
                paraToOrthoRates.Length <> %stageCount || 
                paraToSelfReflRates.Length <> %stageCount || 
                selfReflToOrthoRates.Length <> %stageCount || 
                selfReflToParaRates.Length <> %stageCount then
            failwith $"Number of mutation rates must equal stageCount {%stageCount}"

        let probabilities = 
            Array.zip3 orthoToParaRates orthoToSelfReflRates paraToOrthoRates
            |> Array.zip3 paraToSelfReflRates selfReflToOrthoRates 
            |> Array.zip selfReflToParaRates
            |> Array.map (fun (selfReflToParaRate, (paraToSelfReflRate, selfReflToOrthoRate, (orthoToParaRate, orthoToSelfReflRate, paraToOrthoRate))) -> 
                Msuf4MutationModeProbabilities.create order orthoToParaRate orthoToSelfReflRate paraToOrthoRate paraToSelfReflRate selfReflToOrthoRate selfReflToParaRate)

        let pickers = lazy (probabilities |> Array.map (fun prob -> pick prob.Modes))
        {
            rngType = rngType
            stageCount = stageCount
            probabilities = probabilities
            pickers = pickers
        }

    static member createFromSingleRate
            (rngType: rngType)
            (stageCount: int<stageCount>)
            (order: int)
            (orthoToParaRate: float<uf4OrthoToParaMutationRate>)
            (orthoToSelfReflRate: float<uf4OrthoToSelfReflMutationRate>)
            (paraToOrthoRate: float<uf4ParaToOrthoMutationRate>)
            (paraToSelfReflRate: float<uf4ParaToSelfReflMutationRate>)
            (selfReflToOrthoRate: float<uf4SelfReflToOrthoMutationRate>)
            (selfReflToParaRate: float<uf4SelfReflToParaMutationRate>) 
            : Msuf4RandMutate =
        let orthoToParaRates = Array.create (%stageCount) orthoToParaRate
        let orthoToSelfReflRates = Array.create (%stageCount) orthoToSelfReflRate
        let paraToOrthoRates = Array.create (%stageCount) paraToOrthoRate
        let paraToSelfReflRates = Array.create (%stageCount) paraToSelfReflRate
        let selfReflToOrthoRates = Array.create (%stageCount) selfReflToOrthoRate
        let selfReflToParaRates = Array.create (%stageCount) selfReflToParaRate
        Msuf4RandMutate.create rngType stageCount order 
            orthoToParaRates orthoToSelfReflRates paraToOrthoRates 
            paraToSelfReflRates selfReflToOrthoRates selfReflToParaRates

    member this.RngType with get () = this.rngType
    member this.StageCount with get () = this.stageCount
    member this.OrthoToParaRates with get () = 
        this.probabilities 
        |> Array.map (fun prob -> 
            prob.Modes 
            |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
            |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates.OrthoToPara) 
            |> Option.defaultValue 0.0 
            |> LanguagePrimitives.FloatWithMeasure)
    member this.OrthoToSelfReflRates with get () = 
        this.probabilities 
        |> Array.map (fun prob -> 
            prob.Modes 
            |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
            |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates.OrthoToSelfRefl) 
            |> Option.defaultValue 0.0 
            |> LanguagePrimitives.FloatWithMeasure)
    member this.ParaToOrthoRates with get () = 
        this.probabilities 
        |> Array.map (fun prob -> 
            prob.Modes 
            |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
            |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates.ParaToOrtho) 
            |> Option.defaultValue 0.0 
            |> LanguagePrimitives.FloatWithMeasure)
    member this.ParaToSelfReflRates with get () = 
        this.probabilities 
        |> Array.map (fun prob -> 
            prob.Modes 
            |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
            |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates.ParaToSelfRefl) 
            |> Option.defaultValue 0.0 
            |> LanguagePrimitives.FloatWithMeasure)
    member this.SelfReflToOrthoRates with get () = 
        this.probabilities 
        |> Array.map (fun prob -> 
            prob.Modes 
            |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
            |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates.SelfReflToOrtho) 
            |> Option.defaultValue 0.0 
            |> LanguagePrimitives.FloatWithMeasure)
    member this.SelfReflToParaRates with get () = 
        this.probabilities 
        |> Array.map (fun prob -> 
            prob.Modes 
            |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
            |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates.SelfReflToPara) 
            |> Option.defaultValue 0.0 
            |> LanguagePrimitives.FloatWithMeasure)
    member this.Pickers with get () = this.pickers

    override this.Equals(obj) = 
        match obj with
        | :? Msuf4RandMutate as other -> 
            this.rngType = other.rngType && 
            this.stageCount = other.stageCount &&
            this.probabilities = other.probabilities
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.rngType, this.stageCount, this.probabilities)

    interface IEquatable<Msuf4RandMutate> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.stageCount = other.stageCount &&
            this.probabilities = other.probabilities



module Msuf4RandMutate =

    /// Generates a unique ID for a mutated Msuf4 instance.
    let makeId (msuf4RandMutate: Msuf4RandMutate) (msuf4: Msuf4) (index: int) : Guid<sorterModelID> =
        [ 
            msuf4RandMutate.RngType :> obj
            %msuf4RandMutate.StageCount :> obj
            msuf4RandMutate.probabilities :> obj
            %msuf4.Id :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

    /// Returns a string representation of the Msuf4RandMutate configuration.
    let toString (msuf4RandMutate: Msuf4RandMutate) : string =
        let ratesStr = 
            msuf4RandMutate.probabilities 
            |> Array.mapi (fun i prob -> 
                let rates = 
                    prob.Modes 
                    |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
                    |> Option.map (fun (mode, _) -> mode.Value.seedMutationRates)
                    |> Option.defaultValue { OrthoToPara = 0.0; OrthoToSelfRefl = 0.0; ParaToOrtho = 0.0; ParaToSelfRefl = 0.0; SelfReflToOrtho = 0.0; SelfReflToPara = 0.0 }
                sprintf "[%d: OrthoToPara=%f, OrthoToSelfRefl=%f, ParaToOrtho=%f, ParaToSelfRefl=%f, SelfReflToOrtho=%f, SelfReflToPara=%f]" 
                    i 
                    rates.OrthoToPara
                    rates.OrthoToSelfRefl
                    rates.ParaToOrtho
                    rates.ParaToSelfRefl
                    rates.SelfReflToOrtho
                    rates.SelfReflToPara)
            |> String.concat ", "
        sprintf "Msuf4RandMutate(RngType=%A, StageCount=%d, MutationRates=%s)" 
                msuf4RandMutate.RngType 
                (%msuf4RandMutate.StageCount)
                ratesStr

    /// Mutates an Msuf4 instance using the specified Msuf4RandMutate configuration and random number generator.
    /// <param name="randoGen">The random number generator factory.</param>
    /// <param name="msuf4RandMutate">The Msuf4RandMutate configuration.</param>
    /// <param name="msuf4">The Msuf4 instance to mutate.</param>
    /// <param name="index">The index for ID generation.</param>
    /// <returns>A mutated Msuf4 instance.</returns>
    let mutate
            (randoGen: rngType -> Guid -> IRando)
            (msuf4RandMutate: Msuf4RandMutate)
            (msuf4: Msuf4) 
            (index: int) : Msuf4 =
        if msuf4.StageCount <> %msuf4RandMutate.StageCount then
            failwith $"Stage count of Msuf4 {%msuf4.StageCount} must match Msuf4RandMutate {%msuf4RandMutate.StageCount}"
        else if %msuf4.SortingWidth < 1 then
            failwith $"Msuf4 SortingWidth must be at least 1, got {%msuf4.SortingWidth}"
        else if msuf4RandMutate.probabilities |> Array.exists (fun prob -> 
                    prob.Modes 
                    |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
                    |> Option.map (fun (mode, _) -> mode.Value.order <> %msuf4.SortingWidth) 
                    |> Option.defaultValue false) then
            failwith $"All mutationRates must have order {%msuf4.SortingWidth}"
        let id = makeId msuf4RandMutate msuf4 index
        let rando = randoGen msuf4RandMutate.RngType %id
        let pickers = msuf4RandMutate.Pickers.Value
        let mutatedUnfolders = 
            Array.zip msuf4.TwoOrbitUnfolder4s pickers
            |> Array.map (fun (unfolder, picker) ->
                match picker rando.NextFloat with
                | Some mutationRates -> TwoOrbitUf4Ops.mutateTwoOrbitUf4 rando.NextFloat mutationRates unfolder
                | None -> unfolder)
        Msuf4.create id msuf4.SortingWidth mutatedUnfolders