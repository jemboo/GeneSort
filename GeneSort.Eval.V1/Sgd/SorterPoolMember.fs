namespace GeneSort.Eval.V1.Sgd

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1

type sorterPoolMember =
    private {
        _sorterPoolMemberId:   Guid<sorterPoolMemberId>
        _sorterModel:          sorterModel
        _mutationIndex:        int<mutationIndex>
        _sorterMutationSource: sorterMutationSource option
        _sorterEval:           sorterEval option
        _birthday:             int<generationNumber>
    }

    member this.Birthday with get() = this._birthday
    member this.SorterPoolMemberId with get() = this._sorterPoolMemberId
    member this.SorterModel with get() = this._sorterModel
    // The mutationIndex is incremented each time a new mutation is applied to the sorterModel. 
    // If it wasn't, the same mutant would be created each time.
    member this.MutationIndex = this._mutationIndex
    member this.SorterMutationSource = this._sorterMutationSource
    member this.SorterEval = this._sorterEval

    static member Create 
                    sorterPoolMemberId 
                    sorterModel 
                    mutationIndex 
                    sorterMutationSource 
                    sorterEval 
                    birthday =
        { 
            _sorterPoolMemberId = sorterPoolMemberId
            _sorterModel = sorterModel
            _mutationIndex = mutationIndex 
            _sorterMutationSource = sorterMutationSource
            _sorterEval = sorterEval
            _birthday = birthday
        }


module SorterPoolMember =

    /// Increments a member's mutation index by a given integer value
    let advanceIndex (offset: int) (spm: sorterPoolMember) : sorterPoolMember =
        { spm with _mutationIndex = (%spm.MutationIndex + offset) |> UMX.tag }

    /// Increments a member's mutation index by exactly 1
    let updateIndex (spm: sorterPoolMember) : sorterPoolMember =
        advanceIndex 1 spm

    /// Updates the sorterEval of a pool member
    let withEval (eval: sorterEval option) (spm: sorterPoolMember) : sorterPoolMember =
        { spm with _sorterEval = eval }

    /// Generates 'mutantCount' new mutants, updating the parent's index by 'mutantCount'
    let mutate (sorterModelMutator: sorterModelMutator) 
               (spm: sorterPoolMember) 
               (mutantCount: int<sorterChildCount>) 
               (currentGeneration: int<generationNumber>): sorterPoolMember * sorterPoolMember [] =
        
        let countRaw = %mutantCount
        let baseIndexRaw = %spm.MutationIndex
        
        let mutatorId = SorterModelMutator.getId sorterModelMutator
        let parentModelId = SorterModel.getId spm.SorterModel

        // Generate N unique mutant pool members
        let mutants = 
            Array.init countRaw (fun i ->
                // Compute sequential mutation indices for each child
                let individualMutationIndex = (baseIndexRaw + i) |> UMX.tag<mutationIndex>
                
                let childPoolMemberId = Guid.NewGuid() |> UMX.tag<sorterPoolMemberId>
                
                let mutantModel = 
                    SorterModelMutator.makeMutantSorterModelFromIndex 
                        sorterModelMutator 
                        spm.SorterModel 
                        individualMutationIndex
                
                let mutationSource = 
                    sorterMutationSource.create 
                        mutatorId 
                        parentModelId 
                        individualMutationIndex

                sorterPoolMember.Create
                    childPoolMemberId
                    mutantModel
                    (0 |> UMX.tag)          // New mutants start at mutation index 0
                    (Some mutationSource)
                    None                    // New mutants start unevaluated
                    currentGeneration
            )

        // Increment the parent's SorterMutationIndex by the number of mutants produced
        let updatedParent = spm |> advanceIndex countRaw

        (updatedParent, mutants)
