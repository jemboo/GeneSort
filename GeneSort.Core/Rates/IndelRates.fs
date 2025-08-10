namespace GeneSort.Core
open System
open Combinatorics

type IndelMode =
    | NoAction
    | Mutation
    | Insertion
    | Deletion


[<Struct; CustomEquality; NoComparison>]
type IndelRates = 
    private 
        { mutationThresh: float; insertionThresh: float; deletionThresh: float }

    static member create (mutationRate: float, insertionRate: float, deletionRate: float) : IndelRates =
        let noAction = 1.0 - mutationRate - insertionRate - deletionRate
        let epsilon = 1e-10
        if mutationRate < 0.0 || mutationRate > 1.0 then failwith "mutationRate must be between 0 and 1"
        else if insertionRate < 0.0 || insertionRate > 1.0 then failwith "insertionRate must be between 0 and 1"
        else if deletionRate < 0.0 || deletionRate > 1.0 then failwith "deletionRate must be between 0 and 1"
        else if noAction < -epsilon then failwith "Sum of Indel rates must not exceed 1.0"
        {
            mutationThresh = mutationRate
            insertionThresh = mutationRate + insertionRate
            deletionThresh = mutationRate + insertionRate + deletionRate
        }

    member this.MutationRate with get() = this.mutationThresh
    member this.InsertionRate with get() = this.insertionThresh - this.mutationThresh
    member this.DeletionRate with get() = this.deletionThresh - this.insertionThresh
    member this.NoActionRate with get() = 1.0 - this.deletionThresh
    member this.toString() =
        sprintf "IndelRates(Mutation: %.2f, Insertion: %.2f, Deletion: %.2f)" 
                this.MutationRate this.InsertionRate this.DeletionRate  

    /// Assumes that floatPicker returns a float in the range [0.0, 1.0).
    member this.PickMode (floatPicker: unit -> float) : IndelMode =
        let r = floatPicker()
        if r < this.mutationThresh then
            IndelMode.Mutation
        elif r < this.insertionThresh then
            IndelMode.Insertion
        elif r < this.deletionThresh then
            IndelMode.Deletion
        else
        IndelMode.NoAction

    override this.Equals(obj) = 
        match obj with
        | :? IndelRates as other -> 
            this.mutationThresh = other.mutationThresh &&
            this.insertionThresh = other.insertionThresh &&
            this.deletionThresh = other.deletionThresh
        | _ -> false

    override this.GetHashCode() = 
        hash (this.mutationThresh, this.insertionThresh, this.deletionThresh)

    interface IEquatable<IndelRates> with
        member this.Equals(other) = 
            this.mutationThresh = other.mutationThresh &&
            this.insertionThresh = other.insertionThresh &&
            this.deletionThresh = other.deletionThresh


