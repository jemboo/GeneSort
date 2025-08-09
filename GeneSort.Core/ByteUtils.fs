namespace GeneSort.Core

open System
open SysExt

module ByteUtils = 

    // Convert uint8 to binary string (8 bits)
    let uint8ToBinaryString (value: uint8) : string =
        let sb = System.Text.StringBuilder(8)
        for i = 7 downto 0 do
            sb.Append(if value.isset i then "1" else "0") |> ignore
        sb.ToString()

    // Convert uint16 to binary string (16 bits)
    let uint16ToBinaryString (value: uint16) : string =
        let sb = System.Text.StringBuilder(16)
        for i = 15 downto 0 do
            sb.Append(if value.isset i then "1" else "0") |> ignore
        sb.ToString()

    // Convert uint32 to binary string (32 bits)
    let uint32ToBinaryString (value: uint32) : string =
        let sb = System.Text.StringBuilder(32)
        for i = 31 downto 0 do
            sb.Append(if value.isset i then "1" else "0") |> ignore
        sb.ToString()

    // Convert uint64 to binary string (64 bits)
    let uint64ToBinaryString (value: uint64) : string =
        let sb = System.Text.StringBuilder(64)
        for i = 63 downto 0 do
            sb.Append(if value.isset i then "1" else "0") |> ignore
        sb.ToString()

    // creates a bit stream from a byte stream by selecting the first bitsPerSymbol bits,
    // but in MSB order
    let bitsFromBytes (bitsPerSymbol: int) (byteSeq: seq<byte>) =
        let _byteToBits (v: byte) =
            seq { for i = (bitsPerSymbol - 1) downto 0 do v.isset i }

        seq {
            for bite in byteSeq do
                yield! _byteToBits bite
        }

    // creates a bit stream from a uint64 stream by selecting the first bitsPerSymbol bits,
    // but in MSB order
    let bitsChunkFromUint64 (bitsPerSymbol: int) (int64Seq: seq<uint64>) =
        let _byteToBits (v: UInt64) =
            seq { for i = (bitsPerSymbol - 1) downto 0 do v.isset i }

        seq {
            for bite in int64Seq do
                yield! _byteToBits bite
        }

    let boolsToBytes (bools: seq<bool>) : seq<byte> =
        let boolList = bools |> Seq.toList
        let paddingNeeded = (8 - (boolList.Length % 8)) % 8
        let paddedBools = boolList @ List.replicate paddingNeeded false
        paddedBools
        |> Seq.chunkBySize 8
        |> Seq.map (fun chunk ->
            let mutable result = 0uy
            for i in 0 .. min 7 (chunk.Length - 1) do
                if chunk.[i] then
                    result <- result ||| (1uy <<< (7 - i))
            result)