namespace GeneSort.Core.Test

open Xunit
open GeneSort.Core
open BytePack

type BytePackTests() =

    // Helper function to parse a string of comma-separated bytes (e.g., "0b10110010,0b00001111")
    let parseByteArray (s: string) : byte[] =
        s.Split(',')
        |> Array.map (fun bs -> System.Convert.ToByte(bs.Trim(), 2))

    // Helper function to parse a string of comma-separated integers (e.g., "11,2,15")
    let parseIntArray (s: string) : int[] =
        s.Split(',')
        |> Array.map (fun n -> int (n.Trim()))

    let parseUInt64Array (s: string) : uint64[] =
        s.Split(',')
        |> Array.map (fun n -> uint64 (n.Trim()))

    let parseUInt8Array (s: string) : uint8[] =
        s.Split(',')
        |> Array.map (fun n -> uint8 (n.Trim()))

    let parseUInt16Array (s: string) : uint16[] =
        s.Split(',')
        |> Array.map (fun n -> uint16 (n.Trim()))

    let parseUInt32Array (s: string) : uint32[] =
        s.Split(',')
        |> Array.map (fun n -> uint32 (n.Trim()))


    [<Fact>]
    let ``toBitSequence converts byte array to correct boolean sequence`` () =
        let bp:BytePack = { bitsPerSymbol = 4; symbolCount = 2; data = parseByteArray "10110010,00001111" }
        let expected = [ true; false; true; true; false; false; true; false; 
                         false; false; false; false; true; true; true; true;  ]
        let actual = toBitSequence bp |> Seq.toList
        Assert.Equal<bool list>(expected, actual)


    [<Fact>]
    let ``toBitSequence handles empty data array`` () =
        let bp:BytePack = { bitsPerSymbol = 4; symbolCount = 0; data = [||] }
        let actual = toBitSequence bp |> Seq.toList
        Assert.Empty(actual)


    [<Theory>]
    [<InlineData(4, 3, "10110010,11110000", "11,2,15")>]
    [<InlineData(2, 4, "11001001", "3,0,2,1")>]
    [<InlineData(1, 8, "10110010", "1,0,1,1,0,0,1,0")>]
    let ``toSequenceOfInt converts bits to integers with MSB order`` (bitsPerSymbol: int, symbolCount: int, data: string, expected: string) =
        let bp:BytePack = { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = parseByteArray data }
        let expectedInts = parseIntArray expected
        let actual = toSequenceOfInt bp |> Seq.toList
        Assert.Equal<int list>(expectedInts |> Array.toList, actual)

    [<Fact>]
    let ``toSequenceOfInt throws when insufficient bits`` () =
        let bp:BytePack = { bitsPerSymbol = 4; symbolCount = 3; data = parseByteArray "10110010" } // 8 bits, need 12
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfInt bp |> Seq.toList |> ignore)
        Assert.Equal("Not enough bits in data: expected 12 bits for 3 symbols of 4 bits each, but got 8 bits", ex.Message)

    [<Theory>]
    [<InlineData("11,2,15", 4, 3, "10110010,11110000")>]
    [<InlineData("3,0,2,1", 2, 4, "11001001")>]
    [<InlineData("1,0,1,1,0,0,1,0", 1, 8, "10110010")>]
    let ``fromSequenceOfInt converts integers to BytePack with MSB order`` (ints: string, bitsPerSymbol: int, symbolCount: int, expectedData: string) =
        let inputInts = parseIntArray ints
        let bp:BytePack = fromSequenceOfInt inputInts bitsPerSymbol
        let expectedBytes = parseByteArray expectedData
        Assert.Equal(bitsPerSymbol, bp.bitsPerSymbol)
        Assert.Equal(symbolCount, bp.symbolCount)
        Assert.Equal<byte[]>(expectedBytes, bp.data)


    [<Fact>]
    let ``fromSequenceOfInt handles empty sequence`` () =
        let bp:BytePack = fromSequenceOfInt [] 4
        Assert.Equal(4, bp.bitsPerSymbol)
        Assert.Equal(0, bp.symbolCount)
        Assert.Empty(bp.data)


    [<Fact>]
    let ``fromSequenceOfInt throws on non-positive bitsPerSymbol`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfInt [1; 2; 3] 0 |> ignore)
        Assert.Equal("bitsPerSymbol must be positive", ex.Message)

    [<Theory>]
    [<InlineData("11,2,15", 4)>]
    [<InlineData("3,0,2,1", 2)>]
    [<InlineData("1,0,1,1,0,0,1,0", 1)>]
    let ``round-trip toSequenceOfInt and fromSequenceOfInt preserves data`` (ints: string, bitsPerSymbol: int) =
        let inputInts = parseIntArray ints
        let bp:BytePack = fromSequenceOfInt inputInts bitsPerSymbol
        let result = toSequenceOfInt bp |> Seq.toList
        Assert.Equal<int list>(inputInts |> Array.toList, result)


    // toBitSequence Tests
    [<Fact>]
    let ``toBitSequence converts byte array to correct boolean sequence`` () =
        let bp:BytePack = { bitsPerSymbol = 4; symbolCount = 2; data = parseByteArray "10110010,00001111" }
        let expected = [ true; false; true; true; false; false; true; false; 
                         false; false; false; false; true; true; true; true ]
        let actual = toBitSequence bp |> Seq.toList
        Assert.Equal<bool list>(expected, actual)

    [<Fact>]
    let ``toBitSequence handles empty data array`` () =
        let bp:BytePack = { bitsPerSymbol = 4; symbolCount = 0; data = [||] }
        let actual = toBitSequence bp |> Seq.toList
        Assert.Empty(actual)

    // toSequenceOfUInt64 Tests
    [<Theory>]
    [<InlineData(4, 3, "10110010,11110000", "11,2,15")>]
    [<InlineData(2, 4, "11001001", "3,0,2,1")>]
    [<InlineData(1, 8, "10110010", "1,0,1,1,0,0,1,0")>]
    [<InlineData(16, 1, "10110010,10110000", "45744")>] // 1011001011110000
    [<InlineData(32, 1, "10110010,11110000,00000000,00000000", "3002073088")>] // 10110010111100000000000000000000
    let ``toSequenceOfUInt64 converts bits to uint64 with MSB order`` (bitsPerSymbol: int, symbolCount: int, data: string, expected: string) =
        let bp:BytePack = { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = parseByteArray data }
        let expectedUInt64s = parseUInt64Array expected
        let actual = toSequenceOfUInt64 bp |> Seq.toList
        Assert.Equal<uint64 list>(expectedUInt64s |> Array.toList, actual)

    [<Fact>]
    let ``toSequenceOfUInt64 throws when bitsPerSymbol exceeds 64`` () =
        let bp:BytePack = { bitsPerSymbol = 65; symbolCount = 1; data = parseByteArray "10110010,11110000" }
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfUInt64 bp |> Seq.toList |> ignore)
        Assert.Equal("bitsPerSymbol (65) exceeds uint64 capacity (64 bits)", ex.Message)

    [<Fact>]
    let ``toSequenceOfUInt64 throws when bitsPerSymbol is non-positive`` () =
        let bp:BytePack = { bitsPerSymbol = 0; symbolCount = 1; data = parseByteArray "10110010" }
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfUInt64 bp |> Seq.toList |> ignore)
        Assert.Equal("bitsPerSymbol must be positive", ex.Message)

    [<Fact>]
    let ``toSequenceOfUInt64 throws when insufficient bits`` () =
        let bp:BytePack = { bitsPerSymbol = 4; symbolCount = 3; data = parseByteArray "10110010" } // 8 bits, need 12
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfUInt64 bp |> Seq.toList |> ignore)
        Assert.Equal("Not enough bits in data: expected 12 bits for 3 symbols of 4 bits each, but got 8 bits", ex.Message)

    // fromSequenceOfUInt64 Tests
    [<Theory>]
    [<InlineData("11,2,15", 4, 3, "10110010,11110000")>]
    [<InlineData("3,0,2,1", 2, 4, "11001001")>]
    [<InlineData("1,0,1,1,0,0,1,0", 1, 8, "10110010")>]
    [<InlineData("45744", 16, 1, "10110010,10110000")>]
    [<InlineData("3002073088", 32, 1, "10110010,11110000,00000000,00000000")>]
    let ``fromSequenceOfUInt64 converts uint64 to BytePack with MSB order`` (uint64s: string, bitsPerSymbol: int, symbolCount: int, expectedData: string) =
        let inputUInt64s = parseUInt64Array uint64s
        let bp = fromSequenceOfUInt64 inputUInt64s bitsPerSymbol
        let expectedBytes = parseByteArray expectedData
        Assert.Equal(bitsPerSymbol, bp.bitsPerSymbol)
        Assert.Equal(symbolCount, bp.symbolCount)
        Assert.Equal<byte[]>(expectedBytes, bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt64 handles empty sequence`` () =
        let bp = fromSequenceOfUInt64 [] 4
        Assert.Equal(4, bp.bitsPerSymbol)
        Assert.Equal(0, bp.symbolCount)
        Assert.Empty(bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt64 throws on non-positive bitsPerSymbol`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt64 [1uL; 2uL; 3uL] 0 |> ignore)
        Assert.Equal("bitsPerSymbol must be positive", ex.Message)

    [<Fact>]
    let ``fromSequenceOfUInt64 throws when bitsPerSymbol exceeds 64`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt64 [1uL; 2uL; 3uL] 65 |> ignore)
        Assert.Equal("bitsPerSymbol (65) exceeds uint64 capacity (64 bits)", ex.Message)

    [<Theory>]
    [<InlineData("11,2,15", 4)>]
    [<InlineData("3,0,2,1", 2)>]
    [<InlineData("1,0,1,1,0,0,1,0", 1)>]
    [<InlineData("45744", 16)>]
    [<InlineData("3003121664", 32)>]
    let ``toSequenceOfUInt64 and fromSequenceOfUInt64 round-trip preserves data`` (uint64s: string, bitsPerSymbol: int) =
        let inputUInt64s = parseUInt64Array uint64s
        let bp = fromSequenceOfUInt64 inputUInt64s bitsPerSymbol
        let result = toSequenceOfUInt64 bp |> Seq.toList
        Assert.Equal<uint64 list>(inputUInt64s |> Array.toList, result)

    // toSequenceOfUInt8 Tests
    [<Theory>]
    [<InlineData(4, 3, "10110010,11110000", "11,2,15")>]
    [<InlineData(2, 4, "11001001", "3,0,2,1")>]
    [<InlineData(1, 8, "10110010", "1,0,1,1,0,0,1,0")>]
    [<InlineData(8, 2, "10110010,11110000", "178,240")>]
    let ``toSequenceOfUInt8 converts bits to uint8 with MSB order`` (bitsPerSymbol: int, symbolCount: int, data: string, expected: string) =
        let bp:BytePack = { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = parseByteArray data }
        let expectedUInt8s = parseUInt8Array expected
        let actual = toSequenceOfUInt8 bp |> Seq.toList
        Assert.Equal<uint8 list>(expectedUInt8s |> Array.toList, actual)

    [<Fact>]
    let ``toSequenceOfUInt8 throws when bitsPerSymbol exceeds 8`` () =
        let bp:BytePack = { bitsPerSymbol = 9; symbolCount = 1; data = parseByteArray "10110010" }
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfUInt8 bp |> Seq.toList |> ignore)
        Assert.Equal("bitsPerSymbol (9) exceeds uint8 capacity (8 bits)", ex.Message)

    // fromSequenceOfUInt8 Tests
    [<Theory>]
    [<InlineData("11,2,15", 4, 3, "10110010,11110000")>]
    [<InlineData("3,0,2,1", 2, 4, "11001001")>]
    [<InlineData("1,0,1,1,0,0,1,0", 1, 8, "10110010")>]
    [<InlineData("178,240", 8, 2, "10110010,11110000")>]
    let ``fromSequenceOfUInt8 converts uint8 to BytePack with MSB order`` (uint8s: string, bitsPerSymbol: int, symbolCount: int, expectedData: string) =
        let inputUInt8s = parseUInt8Array uint8s
        let bp = fromSequenceOfUInt8 inputUInt8s bitsPerSymbol
        let expectedBytes = parseByteArray expectedData
        Assert.Equal(bitsPerSymbol, bp.bitsPerSymbol)
        Assert.Equal(symbolCount, bp.symbolCount)
        Assert.Equal<byte[]>(expectedBytes, bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt8 handles empty sequence`` () =
        let bp = fromSequenceOfUInt8 [] 4
        Assert.Equal(4, bp.bitsPerSymbol)
        Assert.Equal(0, bp.symbolCount)
        Assert.Empty(bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt8 throws on non-positive bitsPerSymbol`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt8 [1uy; 2uy; 3uy] 0 |> ignore)
        Assert.Equal("bitsPerSymbol must be positive", ex.Message)

    [<Fact>]
    let ``fromSequenceOfUInt8 throws when bitsPerSymbol exceeds 8`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt8 [1uy; 2uy; 3uy] 9 |> ignore)
        Assert.Equal("bitsPerSymbol (9) exceeds uint8 capacity (8 bits)", ex.Message)

    [<Theory>]
    [<InlineData("11,2,15", 4)>]
    [<InlineData("3,0,2,1", 2)>]
    [<InlineData("1,0,1,1,0,0,1,0", 1)>]
    [<InlineData("178,240", 8)>]
    let ``toSequenceOfUInt8 and fromSequenceOfUInt8 round-trip preserves data`` (uint8s: string, bitsPerSymbol: int) =
        let inputUInt8s = parseUInt8Array uint8s
        let bp = fromSequenceOfUInt8 inputUInt8s bitsPerSymbol
        let result = toSequenceOfUInt8 bp |> Seq.toList
        Assert.Equal<uint8 list>(inputUInt8s |> Array.toList, result)

    // toSequenceOfUInt16 Tests
    [<Theory>]
    [<InlineData(4, 3, "10110010,11110000", "11,2,15")>]
    [<InlineData(8, 2, "10110010,11110000", "178,240")>]
    [<InlineData(16, 1, "10110010,10110000", "45744")>]
    let ``toSequenceOfUInt16 converts bits to uint16 with MSB order`` (bitsPerSymbol: int, symbolCount: int, data: string, expected: string) =
        let bp:BytePack = { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = parseByteArray data }
        let expectedUInt16s = parseUInt16Array expected
        let actual = toSequenceOfUInt16 bp |> Seq.toList
        Assert.Equal<uint16 list>(expectedUInt16s |> Array.toList, actual)

    [<Fact>]
    let ``toSequenceOfUInt16 throws when bitsPerSymbol exceeds 16`` () =
        let bp:BytePack = { bitsPerSymbol = 17; symbolCount = 1; data = parseByteArray "10110010,11110000" }
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfUInt16 bp |> Seq.toList |> ignore)
        Assert.Equal("bitsPerSymbol (17) exceeds uint16 capacity (16 bits)", ex.Message)

    [<Theory>]
    [<InlineData("3,15,127", 7, 3, "00000110,00111111,11111000")>]
    [<InlineData("11,2,15", 4, 3, "10110010,11110000")>]
    [<InlineData("45744", 16, 1, "10110010,10110000")>]
    [<InlineData("15744,222", 15, 2, "01111011, 00000000, 00000011,01111000")>]
    let ``fromSequenceOfUInt16 converts uint16 to BytePack with MSB order`` (uint16s: string, bitsPerSymbol: int, symbolCount: int, expectedData: string) =
        let inputUInt16s = parseUInt16Array uint16s
        let bytePack:BytePack = fromSequenceOfUInt16 inputUInt16s bitsPerSymbol
        let expectedBytes = parseByteArray expectedData
        Assert.Equal(bitsPerSymbol, bytePack.bitsPerSymbol)
        Assert.Equal(symbolCount, bytePack.symbolCount)
        Assert.Equal<byte[]>(expectedBytes, bytePack.data)

    [<Fact>]
    let ``fromSequenceOfUInt16 handles empty sequence`` () =
        let bp = fromSequenceOfUInt16 [] 4
        Assert.Equal(4, bp.bitsPerSymbol)
        Assert.Equal(0, bp.symbolCount)
        Assert.Empty(bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt16 throws on non-positive bitsPerSymbol`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt16 [1us; 2us; 3us] 0 |> ignore)
        Assert.Equal("bitsPerSymbol must be positive", ex.Message)

    [<Fact>]
    let ``fromSequenceOfUInt16 throws when bitsPerSymbol exceeds 16`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt16 [1us; 2us; 3us] 17 |> ignore)
        Assert.Equal("bitsPerSymbol (17) exceeds uint16 capacity (16 bits)", ex.Message)

    [<Theory>]
    [<InlineData("11,2,15", 4)>]
    [<InlineData("178,240", 8)>]
    [<InlineData("45744", 16)>]
    let ``toSequenceOfUInt16 and fromSequenceOfUInt16 round-trip preserves data`` (uint16s: string, bitsPerSymbol: int) =
        let inputUInt16s = parseUInt16Array uint16s
        let bp = fromSequenceOfUInt16 inputUInt16s bitsPerSymbol
        let result = toSequenceOfUInt16 bp |> Seq.toList
        Assert.Equal<uint16 list>(inputUInt16s |> Array.toList, result)

    // toSequenceOfUInt32 Tests
    [<Theory>]
    [<InlineData(4, 3, "10110010,11110000", "11,2,15")>]
    [<InlineData(8, 2, "10110010,11110000", "178,240")>]
    [<InlineData(16, 1, "10110010,10110000", "45744")>]
    [<InlineData(32, 1, "10110010,11110000,00000000,00000000", "3002073088")>]
    [<InlineData(31, 2, "10110010,11110000,00000000,00000001,01100101,11100000,00000000,00000000", "1501036544,1501036544")>]
    let ``toSequenceOfUInt32 converts bits to uint32 with MSB order`` (bitsPerSymbol: int, symbolCount: int, data: string, expected: string) =
        let bp:BytePack = { bitsPerSymbol = bitsPerSymbol; symbolCount = symbolCount; data = parseByteArray data }
        let expectedUInt32s = parseUInt32Array expected
        let actual = toSequenceOfUInt32 bp |> Seq.toList
        Assert.Equal<uint32 list>(expectedUInt32s |> Array.toList, actual)

    [<Fact>]
    let ``toSequenceOfUInt32 throws when bitsPerSymbol exceeds 32`` () =
        let bp:BytePack = { bitsPerSymbol = 33; symbolCount = 1; data = parseByteArray "10110010,11110000" }
        let ex = Assert.Throws<System.Exception>(fun () -> toSequenceOfUInt32 bp |> Seq.toList |> ignore)
        Assert.Equal("bitsPerSymbol (33) exceeds uint32 capacity (32 bits)", ex.Message)

    // fromSequenceOfUInt32 Tests
    [<Theory>]
    [<InlineData("11,2,15", 4, 3, "10110010,11110000")>]
    [<InlineData("178,240", 8, 2, "10110010,11110000")>]
    [<InlineData("45744", 16, 1, "10110010,10110000")>]
    [<InlineData("3002073088", 32, 1, "10110010,11110000,00000000,00000000")>]
    [<InlineData("1501036544,1501036544", 31, 2, "10110010,11110000,00000000,00000001,01100101,11100000,00000000,00000000")>]
    let ``fromSequenceOfUInt32 converts uint32 to BytePack with MSB order`` (uint32s: string, bitsPerSymbol: int, symbolCount: int, expectedData: string) =
        let inputUInt32s = parseUInt32Array uint32s
        let bp = fromSequenceOfUInt32 inputUInt32s bitsPerSymbol
        let expectedBytes = parseByteArray expectedData
        Assert.Equal(bitsPerSymbol, bp.bitsPerSymbol)
        Assert.Equal(symbolCount, bp.symbolCount)
        Assert.Equal<byte[]>(expectedBytes, bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt32 handles empty sequence`` () =
        let bp = fromSequenceOfUInt32 [] 4
        Assert.Equal(4, bp.bitsPerSymbol)
        Assert.Equal(0, bp.symbolCount)
        Assert.Empty(bp.data)

    [<Fact>]
    let ``fromSequenceOfUInt32 throws on non-positive bitsPerSymbol`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt32 [1u; 2u; 3u] 0 |> ignore)
        Assert.Equal("bitsPerSymbol must be positive", ex.Message)

    [<Fact>]
    let ``fromSequenceOfUInt32 throws when bitsPerSymbol exceeds 32`` () =
        let ex = Assert.Throws<System.Exception>(fun () -> fromSequenceOfUInt32 [1u; 2u; 3u] 33 |> ignore)
        Assert.Equal("bitsPerSymbol (33) exceeds uint32 capacity (32 bits)", ex.Message)

    [<Theory>]
    [<InlineData("11,2,15", 4)>]
    [<InlineData("178,240", 8)>]
    [<InlineData("45744", 16)>]
    [<InlineData("3003121664", 32)>]
    let ``toSequenceOfUInt32 and fromSequenceOfUInt32 round-trip preserves data`` (uint32s: string, bitsPerSymbol: int) =
        let inputUInt32s = parseUInt32Array uint32s
        let bp = fromSequenceOfUInt32 inputUInt32s bitsPerSymbol
        let result = toSequenceOfUInt32 bp |> Seq.toList
        Assert.Equal<uint32 list>(inputUInt32s |> Array.toList, result)
