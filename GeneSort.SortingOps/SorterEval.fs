namespace GeneSort.SortingOps

open FSharp.UMX
open GeneSort.Sorting


type sorterEval =
    private { 
        sorterId: Guid<sorterId>
        ceBlockEval: ceBlockEval
    }

    static member create 
                (sorterId: Guid<sorterId>) 
                (ceBlockEval: ceBlockEval) =
        { 
            sorterId = sorterId
            ceBlockEval = ceBlockEval
        }


    member this.CeBlockEval with get() = this.ceBlockEval
    member this.SorterId with get() : Guid<sorterId> = this.sorterId


module SorterEval = ()

