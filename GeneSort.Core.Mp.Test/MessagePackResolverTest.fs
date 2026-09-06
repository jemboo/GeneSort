namespace GeneSort.Core.Test

open System
open Xunit
open MessagePack
open GeneSort.Core.Mp // Ensures assembly load trigger


type TestRestriction =
    | NoRestriction
    | Split

type TestUnion =
    | CaseA of string
    | CaseB of TestRestriction * int

module MessagePackSetup =

    [<Fact>]
    let ``MessagePack default options should include FSharpResolver via ModuleInitializer`` () =
        // Arrange: Create a nested F# Discriminated Union
        let original = CaseB (Split, 42)
        MessagePackSetup.configure()

        let bytes = MessagePackSerializer.Serialize(original)
        let deserialized = MessagePackSerializer.Deserialize<TestUnion>(bytes)

        // Assert
        Assert.Equal(original, deserialized)