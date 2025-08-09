namespace GeneSort.Model.Sorter.Uf6

open System
open FSharp.UMX
open EvoMergeSort.Core
open EvoMergeSort.Sorter
open MathUtils
open EvoMergeSort.Core.Combinatorics
open GeneSort.Model.Sorter

[<Measure>] type uf6OrthoToParaMutationRate
[<Measure>] type uf6OrthoToSelfReflMutationRate
[<Measure>] type uf6ParaToOrthoMutationRate
[<Measure>] type uf6ParaToSelfReflMutationRate
[<Measure>] type uf6SelfReflToOrthoMutationRate
[<Measure>] type uf6SelfReflToParaMutationRate

/// Represents probabilities for mutating a TwoOrbitUnfolder4 instance.
[<Struct; CustomEquality; NoComparison>]
type Msuf6MutationModeProbabilities = 
    private 
        { modes: (Uf6MutationRates option * float) array }
    static member create 
            (order: int)
            (orthoToParaRate: float<uf6OrthoToParaMutationRate>) 
            (orthoToSelfReflRate: float<uf6OrthoToSelfReflMutationRate>) 
            (paraToOrthoRate: float<uf6ParaToOrthoMutationRate>) 
            (paraToSelfReflRate: float<uf6ParaToSelfReflMutationRate>) 
            (selfReflToOrthoRate: float<uf6SelfReflToOrthoMutationRate>) 
            (selfReflToParaRate: float<uf6SelfReflToParaMutationRate>) 
            : Msuf6MutationModeProbabilities =
        let (rates :SeedMutationRatesUf6) = 
            { OrthoToPara = %orthoToParaRate
              OrthoToSelfRefl = %orthoToSelfReflRate
              ParaToOrtho = %paraToOrthoRate
              ParaToSelfRefl = %paraToSelfReflRate
              SelfReflToOrtho = %selfReflToOrthoRate
              SelfReflToPara = %selfReflToParaRate }
        let sum = 
            %orthoToParaRate + %orthoToSelfReflRate + 
            %paraToOrthoRate + %paraToSelfReflRate + 
            %selfReflToOrthoRate + %selfReflToParaRate
        if sum > 1.0 then
            failwith $"Sum of mutation rates must not exceed 1.0, got {sum}"
        if order < 1 then
            failwith $"Order must be at least 1, got {order}"
        else if (order - 1) &&& order <> 0 then
            failwith $"Order must be a power of 2, got {order}"
        let noMutation = 1.0 - sum
        let mutationRates = 
            { Uf6MutationRates.order = order
              seedMutationRates = rates
              twoOrbitTypeMutationRates = List.init (exactLog2 (order / 4)) (fun _ -> TwoOrbitPairMutationRates.makeUniform (%sum)) }
        {
            modes = 
                [|
                    (Some mutationRates, sum)
                    (None, noMutation)
                |]
        }
    member this.Modes with get () = this.modes
    override this.Equals(obj) = 
        match obj with
        | :? Msuf6MutationModeProbabilities as other -> 
            this.modes = other.modes
        | _ -> false
    override this.GetHashCode() = 
        hash (this.GetType(), this.modes)
    interface IEquatable<Msuf6MutationModeProbabilities> with
        member this.Equals(other) = 
            this.modes = other.modes

/// Represents a configuration for mutating Msuf6 instances with specified mutation probabilities.
[<Struct; CustomEquality; NoComparison>]
type Msuf6RandMutate = 
    private 
        { rngType: rngType
          stageCount: int<stageCount>
          probabilities: Msuf6MutationModeProbabilities array
          pickers: Lazy<((unit -> float) -> Uf6MutationRates option)[]> } 
    static member create 
            (rngType: rngType)
            (stageCount: int<stageCount>)
            (order: int)
            (orthoToParaRates: float<uf6OrthoToParaMutationRate>[]) 
            (orthoToSelfReflRates: float<uf6OrthoToSelfReflMutationRate>[]) 
            (paraToOrthoRates: float<uf6ParaToOrthoMutationRate>[]) 
            (paraToSelfReflRates: float<uf6ParaToSelfReflMutationRate>[]) 
            (selfReflToOrthoRates: float<uf6SelfReflToOrthoMutationRate>[]) 
            (selfReflToParaRates: float<uf6SelfReflToParaMutationRate>[]) 
            : Msuf6RandMutate =
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
                Msuf6MutationModeProbabilities.create order orthoToParaRate orthoToSelfReflRate paraToOrthoRate paraToSelfReflRate selfReflToOrthoRate selfReflToParaRate)

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
            (orthoToParaRate: float<uf6OrthoToParaMutationRate>)
            (orthoToSelfReflRate: float<uf6OrthoToSelfReflMutationRate>)
            (paraToOrthoRate: float<uf6ParaToOrthoMutationRate>)
            (paraToSelfReflRate: float<uf6ParaToSelfReflMutationRate>)
            (selfReflToOrthoRate: float<uf6SelfReflToOrthoMutationRate>)
            (selfReflToParaRate: float<uf6SelfReflToParaMutationRate>) 
            : Msuf6RandMutate =
        let orthoToParaRates = Array.create (%stageCount) orthoToParaRate
        let orthoToSelfReflRates = Array.create (%stageCount) orthoToSelfReflRate
        let paraToOrthoRates = Array.create (%stageCount) paraToOrthoRate
        let paraToSelfReflRates = Array.create (%stageCount) paraToSelfReflRate
        let selfReflToOrthoRates = Array.create (%stageCount) selfReflToOrthoRate
        let selfReflToParaRates = Array.create (%stageCount) selfReflToParaRate
        Msuf6RandMutate.create rngType stageCount order 
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
        | :? Msuf6RandMutate as other -> 
            this.rngType = other.rngType && 
            this.stageCount = other.stageCount &&
            this.probabilities = other.probabilities
        | _ -> false

    override this.GetHashCode() = 
        hash (this.GetType(), this.rngType, this.stageCount, this.probabilities)

    interface IEquatable<Msuf6RandMutate> with
        member this.Equals(other) = 
            this.rngType = other.rngType && 
            this.stageCount = other.stageCount &&
            this.probabilities = other.probabilities

module Msuf6RandMutate =

    /// Generates a unique ID for a mutated Msuf6 instance.
    let makeId (msuf6RandMutate: Msuf6RandMutate) (msuf6: Msuf6) (index: int) : Guid<sorterModelID> =
        [ 
            msuf6RandMutate.RngType :> obj
            %msuf6RandMutate.StageCount :> obj
            msuf6RandMutate.probabilities :> obj
            %msuf6.Id :> obj
            index :> obj
        ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterModelID>

    /// Returns a string representation of the Msuf6RandMutate configuration.
    let toString (msuf6RandMutate: Msuf6RandMutate) : string =
        let ratesStr = 
            msuf6RandMutate.probabilities 
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
        sprintf "Msuf6RandMutate(RngType=%A, StageCount=%d, MutationRates=%s)" 
                msuf6RandMutate.RngType 
                (%msuf6RandMutate.StageCount)
                ratesStr

    /// Mutates an Msuf6 instance using the specified Msuf6RandMutate configuration and random number generator.
    /// <param name="randoGen">The random number generator factory.</param>
    /// <param name="msuf6RandMutate">The Msuf6RandMutate configuration.</param>
    /// <param name="msuf6">The Msuf6 instance to mutate.</param>
    /// <param name="index">The index for ID generation.</param>
    /// <returns>A mutated Msuf6 instance.</returns>
    let mutate
            (randoGen: rngType -> Guid -> IRando)
            (msuf6RandMutate: Msuf6RandMutate)
            (msuf6: Msuf6) 
            (index: int) : Msuf6 =
        if msuf6.StageCount <> %msuf6RandMutate.StageCount then
            failwith $"Stage count of Msuf6 {%msuf6.StageCount} must match Msuf6RandMutate {%msuf6RandMutate.StageCount}"
        else if %msuf6.SortingWidth < 1 then
            failwith $"Msuf6 SortingWidth must be at least 1, got {%msuf6.SortingWidth}"
        else if msuf6RandMutate.probabilities |> Array.exists (fun prob -> 
                    prob.Modes 
                    |> Array.tryFind (fun (mode, _) -> mode.IsSome) 
                    |> Option.map (fun (mode, _) -> mode.Value.order <> %msuf6.SortingWidth) 
                    |> Option.defaultValue false) then
            failwith $"All mutationRates must have order {%msuf6.SortingWidth}"
        let id = makeId msuf6RandMutate msuf6 index
        let rando = randoGen msuf6RandMutate.RngType %id
        let pickers = msuf6RandMutate.Pickers.Value
        let mutatedUnfolders = 
            Array.zip msuf6.TwoOrbitUnfolder6s pickers
            |> Array.map (fun (unfolder, picker) ->
                match picker rando.NextFloat with
                | Some mutationRates -> TwoOrbitUf6Ops.mutateTwoOrbitUf6 rando.NextFloat mutationRates unfolder
                | None -> unfolder)
        Msuf6.create id msuf6.SortingWidth mutatedUnfolders