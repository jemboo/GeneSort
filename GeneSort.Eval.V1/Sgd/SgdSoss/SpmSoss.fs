namespace GeneSort.Eval.V1.Sgd.Soss

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Sgd

type spmSoss =
    private {
        _sorterPoolMemberId:   Guid<sorterPoolMemberId>
        _sorterModel:          sorterModel
        _mutationIndex:        int<mutationIndex>
        _mutationMod:          int<mutationMod>
        _sorterMutationSource: sorterMutationSource option
        _sorterEvalMap:        Map<string<sortableTestsSubsetId>, sorterEval>
        _birthday:             int<generationNumber>
    }

    member this.Birthday with get() = this._birthday
    member this.SorterPoolMemberId with get() = this._sorterPoolMemberId
    member this.SorterModel with get() = this._sorterModel
    // The mutationIndex is incremented each time a new mutation is applied to the sorterModel. 
    // If it wasn't, the same mutant would be created each time.
    member this.MutationIndex = this._mutationIndex
    member this.MutationMod = this._mutationMod
    member this.SorterMutationSource = this._sorterMutationSource
    member this.SorterEvalMap = this._sorterEvalMap
    member this.SorterEval = Map.tryFind SortableTestsSubsetId.Default this._sorterEvalMap

    static member create 
                    sorterPoolMemberId 
                    sorterModel 
                    mutationIndex
                    mutationMod
                    sorterMutationSource 
                    sorterEval 
                    birthday =
        { 
            _sorterPoolMemberId = sorterPoolMemberId
            _sorterModel = sorterModel
            _mutationIndex = mutationIndex 
            _mutationMod = mutationMod
            _sorterMutationSource = sorterMutationSource
            _sorterEvalMap = sorterEval
            _birthday = birthday
        }


module SpmSoss =

    /// Increments a member's mutation index by a given integer value
    let advanceIndex (offset: int) (spm: spmSoss) : spmSoss =
        { spm with _mutationIndex = (%spm.MutationIndex + offset) |> UMX.tag }

    /// Increments a member's mutation index by exactly 1
    let updateIndex (spm: spmSoss) : spmSoss =
        advanceIndex 1 spm

    /// Derives a child pool member for a new pool branch:
    /// Assigns a fresh sorterPoolMemberId, updates mutationMod, resets mutationIndex to 0,
    /// and preserves the existing sorterMutationSource and sorterModel.
    let deriveForChildPool 
            (newMemberId: Guid<sorterPoolMemberId>) 
            (newMod: int<mutationMod>) 
            (spm: spmSoss) : spmSoss =
        { spm with 
            _sorterPoolMemberId = newMemberId
            _mutationMod = newMod
            _mutationIndex = 0 |> UMX.tag<mutationIndex> }

    /// Adds or replaces one subset evaluation; None clears every cached evaluation.
    let withEval (eval: sorterEval option) (spm: spmSoss) : spmSoss =
        match eval with
        | Some value ->
            { spm with
                _sorterEvalMap =
                    Map.add (SorterEval.getSortableTestsSubsetId value) value spm._sorterEvalMap }
        | None -> { spm with _sorterEvalMap = Map.empty }

    /// Generates 'mutantCount' new mutants, updating the parent's index by 'mutantCount'
    let mutate (sorterModelMut: sorterModelMutator) 
               (spm: spmSoss) 
               (spId: Guid<sorterPoolId>) 
               (mutantCount: int<sorterChildCount>) 
               (currentGeneration: int<generationNumber>): spmSoss * spmSoss [] =
        
        let countRaw = %mutantCount
        let baseIndexRaw = %spm.MutationIndex
        
        let mutatorId = SorterModelMutator.getId sorterModelMut

        // Generate N unique mutant pool members
        let mutants = 
            Array.init countRaw (fun i ->
                // Compute sequential mutation indices for each child
                let individualMutationIndex = (baseIndexRaw + i) |> UMX.tag<mutationIndex>
                
                let childPoolMemberId = Guid.NewGuid() |> UMX.tag<sorterPoolMemberId>
                
                let mutantModel = 
                    SorterModelMutator.makeMutantSorterModelFromIndexAndMod 
                        sorterModelMut 
                        spm.SorterModel 
                        individualMutationIndex
                        spm.MutationMod

                let mutationSource = 
                    sorterMutationSource.create 
                        mutatorId 
                        spm.SorterPoolMemberId
                        spId
                        individualMutationIndex

                spmSoss.create
                    childPoolMemberId
                    mutantModel
                    (0 |> UMX.tag)          // New mutants start at mutation index 0
                    spm.MutationMod
                    (Some mutationSource)
                    Map.empty               // New mutants start unevaluated
                    currentGeneration
            )

        // Increment the parent's SorterMutationIndex by the number of mutants produced
        let updatedParent = spm |> advanceIndex countRaw

        (updatedParent, mutants)
