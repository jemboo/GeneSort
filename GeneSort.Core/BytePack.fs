namespace GeneSort.Core

open System
open SysExt


type BytePack = {
    bitsPerSymbol: int
    symbolCount: int
    data: byte[]
}


module BytePack =

    let toBitSequence (bytePack: BytePack) : seq<bool> =
        bytePack.data
        |> Seq.collect (fun b -> b.toBoolArrayMSB 8)

    let toSequenceOfInt (bytePack: BytePack) : seq<int> =
        let bits = toBitSequence bytePack |> Seq.toArray
        let chunkSize = bytePack.bitsPerSymbol
        let expectedBits = bytePack.symbolCount * chunkSize
        
        if bits.Length < expectedBits then
            failwithf "Not enough bits in data: expected %d bits for %d symbols of %d bits each, but got %d bits"
                expectedBits bytePack.symbolCount chunkSize bits.Length

        seq {
            for i = 0 to bytePack.symbolCount - 1 do
                let start = i * chunkSize
                let mutable value = 0
                for j = 0 to chunkSize - 1 do
                    if bits.[start + j] then
                        value <- value ||| (1 <<< (chunkSize - 1 - j))
                yield value
        }

    let fromSequenceOfInt (ints: seq<int>) (bitsPerSymbol: int) : BytePack =
        let symbols = Seq.toArray ints
        let symbolCount = symbols.Length
        let totalBits = symbolCount * bitsPerSymbol
        
        if bitsPerSymbol <= 0 then
            failwith "bitsPerSymbol must be positive"
        if symbolCount = 0 then
            { bitsPerSymbol = bitsPerSymbol; symbolCount = 0; data = [||] }
        else
            let bits =
                symbols
                |> Seq.collect (fun n ->
                    seq { for i = bitsPerSymbol - 1 downto 0 do
                            yield (n &&& (1 <<< i)) <> 0 })
            let byteCount = (totalBits + 7) / 8
            let data = Array.zeroCreate<byte> byteCount
            let mutable bitIndex = 0
            
            for b in bits do
                let byteIdx = bitIndex / 8
                let bitPos = 7 - (bitIndex % 8)
                if b then
                    data.[byteIdx] <- data.[byteIdx] ||| (1uy <<< bitPos)
                bitIndex <- bitIndex + 1
                
            { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = data }



    let toSequenceOfUInt64 (bp: BytePack) : seq<uint64> =
        let bits = toBitSequence bp |> Seq.toArray
        let chunkSize = bp.bitsPerSymbol
        let expectedBits = bp.symbolCount * chunkSize
    
        if chunkSize <= 0 then
            failwith "bitsPerSymbol must be positive"
        if chunkSize > 64 then
            failwithf "bitsPerSymbol (%d) exceeds uint64 capacity (64 bits)" chunkSize
        if bits.Length < expectedBits then
            failwithf "Not enough bits in data: expected %d bits for %d symbols of %d bits each, but got %d bits"
                expectedBits bp.symbolCount chunkSize bits.Length

        seq {
            for i = 0 to bp.symbolCount - 1 do
                let start = i * chunkSize
                let mutable value = 0uL
                for j = 0 to chunkSize - 1 do
                    if bits.[start + j] then
                        value <- value ||| (1uL <<< (chunkSize - 1 - j))
                yield value
        }

    let fromSequenceOfUInt64 (uint64s: seq<uint64>) (bitsPerSymbol: int) : BytePack =
        let symbols = Seq.toArray uint64s
        let symbolCount = symbols.Length
        let totalBits = symbolCount * bitsPerSymbol
    
        if bitsPerSymbol <= 0 then
            failwith "bitsPerSymbol must be positive"
        if bitsPerSymbol > 64 then
            failwithf "bitsPerSymbol (%d) exceeds uint64 capacity (64 bits)" bitsPerSymbol
        if symbolCount = 0 then
            { bitsPerSymbol = bitsPerSymbol; symbolCount = 0; data = [||] }
        else
            let bits = ByteUtils.bitsChunkFromUint64 bitsPerSymbol symbols
                       |> Seq.toArray

            let data = ByteUtils.boolsToBytes bits |> Seq.toArray
            
            { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = data }



    let toSequenceOfUInt8 (bp: BytePack) : seq<uint8> =
        if bp.bitsPerSymbol > 8 then
            failwithf "bitsPerSymbol (%d) exceeds uint8 capacity (8 bits)" bp.bitsPerSymbol
        toSequenceOfUInt64 bp |> Seq.map uint8

    let fromSequenceOfUInt8 (uint8s: seq<uint8>) (bitsPerSymbol: int) : BytePack =
        if bitsPerSymbol > 8 then
            failwithf "bitsPerSymbol (%d) exceeds uint8 capacity (8 bits)" bitsPerSymbol
        fromSequenceOfUInt64 (uint8s |> Seq.map uint64) bitsPerSymbol

    let toSequenceOfUInt16 (bp: BytePack) : seq<uint16> =
        if bp.bitsPerSymbol > 16 then
            failwithf "bitsPerSymbol (%d) exceeds uint16 capacity (16 bits)" bp.bitsPerSymbol
        toSequenceOfUInt64 bp |> Seq.map uint16

    let fromSequenceOfUInt16 (uint16s: seq<uint16>) (bitsPerSymbol: int) : BytePack =
        if bitsPerSymbol > 16 then
            failwithf "bitsPerSymbol (%d) exceeds uint16 capacity (16 bits)" bitsPerSymbol
        fromSequenceOfUInt64 (uint16s |> Seq.map uint64) bitsPerSymbol

    let toSequenceOfUInt32 (bp: BytePack) : seq<uint32> =
        if bp.bitsPerSymbol > 32 then
            failwithf "bitsPerSymbol (%d) exceeds uint32 capacity (32 bits)" bp.bitsPerSymbol
        toSequenceOfUInt64 bp |> Seq.map uint32

    let fromSequenceOfUInt32 (uint32s: seq<uint32>) (bitsPerSymbol: int) : BytePack =
        if bitsPerSymbol > 32 then
            failwithf "bitsPerSymbol (%d) exceeds uint32 capacity (32 bits)" bitsPerSymbol
        fromSequenceOfUInt64 (uint32s |> Seq.map uint64) bitsPerSymbol
