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
    member _.``qpSortableTestRestrictionDto converts back and forth accurately`` () =
        let cases = [ qpSortableTestRestriction.NoRestriction; qpSortableTestRestriction.Split ]
        
        for case in cases do
            let dto = QpSortableTestRestrictionDto.fromDomain case
            let roundtripped = QpSortableTestRestrictionDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSortableTestTypeDto converts back and forth accurately`` () =
        let cases = 
            [ qpSortableTestType.Standard qpSortableTestRestriction.NoRestriction
              qpSortableTestType.Merge qpSortableTestRestriction.Split
              qpSortableTestType.Prefix qpSortableTestRestriction.NoRestriction ]

        for case in cases do
            let dto = QpSortableTestTypeDto.fromDomain case
            let roundtripped = QpSortableTestTypeDto.toDomain dto
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
                qpSortableTestType.Merge qpSortableTestRestriction.Split
            )

        let dto = QpSorterEvalDto.fromDomain sorterEval
        let roundtripped = QpSorterEvalDto.toDomain dto
        
        Assert.Equal(sorterEval, roundtripped)
        TestHelpers.assertMpRoundtrip dto

    [<Fact>]
    member _.``qpSorterMutateDto converts back and forth accurately`` () =
        let eval1 = qpSorterEval.Standard (qpSorterType.Simple qpSimpleSorter.Msce, qpSortableTestType.Standard qpSortableTestRestriction.NoRestriction)
        let eval2 = qpSorterEval.Standard (qpSorterType.Dual qpSimpleSorter.Msuf4, qpSortableTestType.Prefix qpSortableTestRestriction.Split)

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
        let eval1 = qpSorterEval.Standard (qpSorterType.Simple qpSimpleSorter.Msce, qpSortableTestType.Standard qpSortableTestRestriction.NoRestriction)
        let eval2 = qpSorterEval.Standard (qpSorterType.Dual qpSimpleSorter.Msuf6, qpSortableTestType.Prefix qpSortableTestRestriction.Split)

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
        let eval = qpSorterEval.Standard (qpSorterType.Gated qpSimpleSorter.Msrs, qpSortableTestType.Merge qpSortableTestRestriction.NoRestriction)
        let pool1 = qpSorterPoolType.Random (qpSorterPoolStructure.Singleton, eval)
        let pool2 = qpSorterPoolType.Random (qpSorterPoolStructure.Multiple, eval)
        let mutate = qpSorterMutate.Variable eval
        let sgd = qpSgdType.FixedPools (pool1, pool2, mutate)

        let cases = 
            [ queryProperties.SortableTest (qpSortableTestType.Standard qpSortableTestRestriction.Split)
              queryProperties.SorterEval eval
              queryProperties.SorterMutate mutate
              queryProperties.SorterSgd sgd ]

        for case in cases do
            let dto = QueryParamTypeDto.fromDomain case
            let roundtripped = QueryParamTypeDto.toDomain dto
            Assert.Equal(case, roundtripped)
            TestHelpers.assertMpRoundtrip dto