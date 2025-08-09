namespace GeneSort.Core.Test

open Xunit
open GeneSort.Core.CollectionUtils
open GeneSort.Core.ByteUtils
open FsUnit.Xunit
open GeneSort.Core
open System


type ByteUtilsTests() =


    [<Fact>]
    let ``Convert uint8 to binary string`` () =
        let value = 0b10100101uy
        let result = uint8ToBinaryString value
        let expected = "10100101"
        Assert.Equal(expected, result)

    [<Fact>]
    let ``Convert uint16 to binary string`` () =
        let value = 0b0101101010101010us
        let result = uint16ToBinaryString value
        let expected = "0101101010101010"
        Assert.Equal(expected, result)

    [<Fact>]
    let ``Convert uint32 to binary string`` () =
        let value = 0b01011010101001111000000011110000u
        let result = uint32ToBinaryString value
        let expected = "01011010101001111000000011110000"
        Assert.Equal(expected, result)

    [<Fact>]
    let ``Convert uint64 to binary string`` () =
        let value = 0b0101010110101010001100111100110000001111111100001111000011110000UL
        let result = uint64ToBinaryString value
        let expected = "0101010110101010001100111100110000001111111100001111000011110000"
        Assert.Equal(expected, result)

    [<Fact>]
    let ``Convert zero values to binary strings`` () =
        Assert.Equal("00000000", uint8ToBinaryString 0uy)
        Assert.Equal("0000000000000000", uint16ToBinaryString 0us)
        Assert.Equal("00000000000000000000000000000000", uint32ToBinaryString 0u)
        Assert.Equal("0000000000000000000000000000000000000000000000000000000000000000", uint64ToBinaryString 0UL)


