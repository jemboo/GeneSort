namespace GeneSort.Run.Test.V1

open Xunit
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Project.V1
open GeneSort.Project.Mp.V1

module TestHelpers =

    /// Standard MessagePack options configured with FSharpResolver and StandardResolver.
    let serializerOptions =
        let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
        MessagePackSerializerOptions.Standard.WithResolver(resolver)

    /// Helper to test full roundtrip through MessagePack byte serialization.
    let assertMpRoundtrip<'T> (value: 'T) =
        let bytes = MessagePackSerializer.Serialize<'T>(value, serializerOptions)
        let deserialized = MessagePackSerializer.Deserialize<'T>(bytes, serializerOptions)
        Assert.Equal<'T>(value, deserialized)

type QueryParamMpTests () =

    [<Fact>]
    member _.``qpSortableTestsRestrictionDto converts back and forth accurately`` () =
        let cases = [ qpSortableTestsRestriction.NoRestriction; qpSortableTestsRestriction.Split ]
        
        for case in cases do
            let dto = QpSortableTestsRestrictionDto.fromDomain case
            let roundtripped = QpSortableTestsRestrictionDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSortableTestsTypeDto converts back and forth accurately`` () =
        let cases = 
            [ qpSortableTestsType.Standard qpSortableTestsRestriction.NoRestriction
              qpSortableTestsType.Merge qpSortableTestsRestriction.Split
              qpSortableTestsType.Prefix qpSortableTestsRestriction.NoRestriction ]

        for case in cases do
            let dto = QpSortableTestsTypeDto.fromDomain case
            let roundtripped = QpSortableTestsTypeDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSimpleSorterDto converts back and forth accurately`` () =
        let cases = 
            [ qpSimpleSorter.Msce
              qpSimpleSorter.Mssi
              qpSimpleSorter.Msrs
              qpSimpleSorter.Msuf4
              qpSimpleSorter.Msuf6 ]

        for case in cases do
            let dto = QpSimpleSorterDto.fromDomain case
            let roundtripped = QpSimpleSorterDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSorterTypeDto converts back and forth accurately`` () =
        let cases = 
            [ qpSorterType.Simple qpSimpleSorter.Msce
              qpSorterType.Gated qpSimpleSorter.Msrs
              qpSorterType.Dual qpSimpleSorter.Msuf6 ]

        for case in cases do
            let dto = QpSorterTypeDto.fromDomain case
            let roundtripped = QpSorterTypeDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSorterEvalDto converts back and forth accurately`` () =
        let sorterEval = 
            qpSorterEval.Standard (
                qpSorterType.Gated qpSimpleSorter.Mssi, 
                qpSortableTestsType.Merge qpSortableTestsRestriction.Split
            )

        let dto = QpSorterEvalDto.fromDomain sorterEval
        let roundtripped = QpSorterEvalDto.toDomain dto
        
        Assert.Equal(sorterEval, roundtripped)
        TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSorterMutateDto converts back and forth accurately`` () =
        let eval1 = qpSorterEval.Standard (qpSorterType.Simple qpSimpleSorter.Msce, qpSortableTestsType.Standard qpSortableTestsRestriction.NoRestriction)
        let eval2 = qpSorterEval.Standard (qpSorterType.Dual qpSimpleSorter.Msuf4, qpSortableTestsType.Prefix qpSortableTestsRestriction.Split)

        let cases = 
            [ qpSorterMutate.Uniform eval1
              qpSorterMutate.Variable eval2 ]

        for case in cases do
            let dto = QpSorterMutateDto.fromDomain case
            let roundtripped = QpSorterMutateDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``Mutually recursive qpSorterPoolType & qpSgdType convert back and forth accurately`` () =
        let eval1 = qpSorterEval.Standard (qpSorterType.Simple qpSimpleSorter.Msce, qpSortableTestsType.Standard qpSortableTestsRestriction.NoRestriction)
        let eval2 = qpSorterEval.Standard (qpSorterType.Dual qpSimpleSorter.Msuf6, qpSortableTestsType.Prefix qpSortableTestsRestriction.Split)

        let poolRandom1 = qpSorterPoolType.Random (qpSorterPoolStructure.Singleton, eval1)
        let poolRandom2 = qpSorterPoolType.Random (qpSorterPoolStructure.Tiled, eval2)
        let mutate = qpSorterMutate.Uniform eval1

        let sgdDomain = qpSgdType.FixedPools (poolRandom1, poolRandom2, mutate)
        let poolSgd = qpSorterPoolType.Sgd (qpSorterPoolStructure.Multiple, sgdDomain)

        let poolDto = QpSorterPoolTypeDto.fromDomain poolSgd
        let poolRoundtripped = QpSorterPoolTypeDto.toDomain poolDto
        Assert.Equal(poolSgd, poolRoundtripped)

        let sgdDto = QpSgdTypeDto.fromDomain sgdDomain
        let sgdRoundtripped = QpSgdTypeDto.toDomain sgdDto
        Assert.Equal(sgdDomain, sgdRoundtripped)

    [<Fact>]
    member _.``queryParamTypeDto converts back and forth across all union cases`` () =
        let eval = qpSorterEval.Standard (qpSorterType.Gated qpSimpleSorter.Msrs, qpSortableTestsType.Merge qpSortableTestsRestriction.NoRestriction)
        let pool1 = qpSorterPoolType.Random (qpSorterPoolStructure.Singleton, eval)
        let pool2 = qpSorterPoolType.Random (qpSorterPoolStructure.Multiple, eval)
        let mutate = qpSorterMutate.Variable eval
        let sgd = qpSgdType.FixedPools (pool1, pool2, mutate)

        let cases = 
            [ queryProperties.SortableTests (qpSortableTestsType.Standard qpSortableTestsRestriction.Split)
              queryProperties.SorterEval eval
              queryProperties.SorterMutate mutate
              queryProperties.SorterSgd sgd ]

        for case in cases do
            let dto = QueryPropertiesDto.fromDomain case
            let roundtripped = QueryPropertiesDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto