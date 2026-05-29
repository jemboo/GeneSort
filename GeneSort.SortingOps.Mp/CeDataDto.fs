namespace GeneSort.SortingOps.Mp

open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorting.Sorter


// ---------------------------------------------------------------------
// Compressed Data Transfer Object for ceData
// ---------------------------------------------------------------------

[<MessagePackObject>]
type ceDataDto = {
    /// Accurate up to 65,535 (2 bytes)
    [<Key(0)>] CeIndex : uint16
    
    /// Approximate count packed into a 16-bit Half-Precision float (2 bytes)
    [<Key(1)>] UseCount : System.Half
    
    /// Flattened index representing the CE, accurate up to ~2 Billion (4 bytes)
    [<Key(2)>] CeInt : int
}

module CeDataDto =

    // Private inline helper to clear ambiguity for the F# compiler on explicit operators
    let private inline16Float (f: float32) : System.Half = 
        #if NET6_0_OR_GREATER
        System.Half.op_Explicit(f)
        #else
        // Alternative explicit assignment structure if standard operators hide it
        Convert.ChangeType(f, typeof<System.Half>) :?> System.Half
        #endif

    let fromDomain (domain: ceData) : ceDataDto = 
        // Cast the int count to float32, then safely downcast to 16-bit Half float
        let floatValue = float32 domain.UseCount
        let halfValue = inline16Float floatValue
        
        {
            CeIndex = uint16 %domain.CeIndex
            UseCount = halfValue
            CeInt = Ce.toIndex domain.Ce
        }

    let toDomain (dto: ceDataDto) : ceData =
        // Cast the 16-bit float back to a native 32-bit float, then to int
        let decodedCount = int (float32 dto.UseCount)
        let reconstructedCe = Ce.fromIndex dto.CeInt
        
        ceData.create 
            (int dto.CeIndex |> UMX.tag<ceIndex>) 
            decodedCount 
            reconstructedCe
