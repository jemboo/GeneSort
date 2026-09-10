namespace rec GeneSort.Project.Mp.V1

open GeneSort.Project.V1

// --- qpSortableTestRestriction ---

type qpSortableTestRestrictionDto =
    | NoRestriction
    | Split

module QpSortableTestRestrictionDto =

    let fromDomain (domain: qpSortableTestRestriction) : qpSortableTestRestrictionDto =
        match domain with
        | qpSortableTestRestriction.NoRestriction -> NoRestriction
        | qpSortableTestRestriction.Split -> Split

    let toDomain (dto: qpSortableTestRestrictionDto) : qpSortableTestRestriction =
        match dto with
        | NoRestriction -> qpSortableTestRestriction.NoRestriction
        | Split -> qpSortableTestRestriction.Split


// --- qpSortableTestType ---

type qpSortableTestTypeDto =
    | Standard of qpSortableTestRestrictionDto
    | Merge of qpSortableTestRestrictionDto
    | Prefix of qpSortableTestRestrictionDto

module QpSortableTestTypeDto =

    let fromDomain (domain: qpSortableTestType) : qpSortableTestTypeDto =
        match domain with
        | qpSortableTestType.Standard r -> qpSortableTestTypeDto.Standard (QpSortableTestRestrictionDto.fromDomain r)
        | qpSortableTestType.Merge r -> Merge (QpSortableTestRestrictionDto.fromDomain r)
        | qpSortableTestType.Prefix r -> Prefix (QpSortableTestRestrictionDto.fromDomain r)

    let toDomain (dto: qpSortableTestTypeDto) : qpSortableTestType =
        match dto with
        | qpSortableTestTypeDto.Standard rDto -> qpSortableTestType.Standard (QpSortableTestRestrictionDto.toDomain rDto)
        | Merge rDto -> qpSortableTestType.Merge (QpSortableTestRestrictionDto.toDomain rDto)
        | Prefix rDto -> qpSortableTestType.Prefix (QpSortableTestRestrictionDto.toDomain rDto)


// --- qpSimpleSorter ---

type qpSimpleSorterDto =
    | Msce
    | Mssi
    | Msrs
    | Msuf4
    | Msuf6

module QpSimpleSorterDto =

    let fromDomain (domain: qpSimpleSorter) : qpSimpleSorterDto =
        match domain with
        | qpSimpleSorter.Msce -> Msce
        | qpSimpleSorter.Mssi -> Mssi
        | qpSimpleSorter.Msrs -> Msrs
        | qpSimpleSorter.Msuf4 -> Msuf4
        | qpSimpleSorter.Msuf6 -> Msuf6

    let toDomain (dto: qpSimpleSorterDto) : qpSimpleSorter =
        match dto with
        | Msce -> qpSimpleSorter.Msce
        | Mssi -> qpSimpleSorter.Mssi
        | Msrs -> qpSimpleSorter.Msrs
        | Msuf4 -> qpSimpleSorter.Msuf4
        | Msuf6 -> qpSimpleSorter.Msuf6


// --- qpSorterType ---

type qpSorterTypeDto =
    | Simple of qpSimpleSorterDto
    | Gated of qpSimpleSorterDto
    | Dual of qpSimpleSorterDto

module QpSorterTypeDto =

    let fromDomain (domain: qpSorterType) : qpSorterTypeDto =
        match domain with
        | qpSorterType.Simple s -> Simple (QpSimpleSorterDto.fromDomain s)
        | qpSorterType.Gated s -> Gated (QpSimpleSorterDto.fromDomain s)
        | qpSorterType.Dual s -> Dual (QpSimpleSorterDto.fromDomain s)

    let toDomain (dto: qpSorterTypeDto) : qpSorterType =
        match dto with
        | Simple sDto -> qpSorterType.Simple (QpSimpleSorterDto.toDomain sDto)
        | Gated sDto -> qpSorterType.Gated (QpSimpleSorterDto.toDomain sDto)
        | Dual sDto -> qpSorterType.Dual (QpSimpleSorterDto.toDomain sDto)


// --- qpSorterEval ---

type qpSorterEvalDto =
    | Standard of qpSorterTypeDto * qpSortableTestTypeDto

module QpSorterEvalDto =

    let fromDomain (domain: qpSorterEval) : qpSorterEvalDto =
        match domain with
        | qpSorterEval.Standard (st, tt) ->
            Standard (QpSorterTypeDto.fromDomain st, QpSortableTestTypeDto.fromDomain tt)

    let toDomain (dto: qpSorterEvalDto) : qpSorterEval =
        match dto with
        | Standard (stDto, ttDto) ->
            qpSorterEval.Standard (QpSorterTypeDto.toDomain stDto, QpSortableTestTypeDto.toDomain ttDto)


// --- qpSorterMutate ---

// --- qpSorterMutate ---

type qpSorterMutateDto =
    | Uniform of qpSorterEvalDto
    | Variable of qpSorterEvalDto

module QpSorterMutateDto =

    let fromDomain (domain: qpSorterMutate) : qpSorterMutateDto =
        match domain with
        | qpSorterMutate.Uniform se -> Uniform (QpSorterEvalDto.fromDomain se)
        | qpSorterMutate.Variable se -> Variable (QpSorterEvalDto.fromDomain se)

    let toDomain (dto: qpSorterMutateDto) : qpSorterMutate =
        match dto with
        | Uniform seDto -> qpSorterMutate.Uniform (QpSorterEvalDto.toDomain seDto)
        | Variable seDto -> qpSorterMutate.Variable (QpSorterEvalDto.toDomain seDto)


// --- qpSorterPoolStructure ---

type qpSorterPoolStructureDto =
    | Singleton
    | Multiple
    | Tiled

module QpSorterPoolStructureDto =

    let fromDomain (domain: qpSorterPoolStructure) : qpSorterPoolStructureDto =
        match domain with
        | qpSorterPoolStructure.Singleton -> Singleton
        | qpSorterPoolStructure.Multiple -> Multiple
        | qpSorterPoolStructure.Tiled -> Tiled

    let toDomain (dto: qpSorterPoolStructureDto) : qpSorterPoolStructure =
        match dto with
        | Singleton -> qpSorterPoolStructure.Singleton
        | Multiple -> qpSorterPoolStructure.Multiple
        | Tiled -> qpSorterPoolStructure.Tiled


// --- Mutually Recursive Types & Converter Modules ---

type qpSorterPoolTypeDto =
    | Random of qpSorterPoolStructureDto * qpSorterEvalDto
    | Sgd of qpSorterPoolStructureDto * qpSgdTypeDto

and qpSgdTypeDto =
    | FixedPools of qpSorterPoolTypeDto * qpSorterPoolTypeDto * qpSorterMutateDto

module QpSorterPoolTypeDto =

    let fromDomain (domain: qpSorterPoolType) : qpSorterPoolTypeDto =
        match domain with
        | qpSorterPoolType.Random (ps, se) ->
            Random (QpSorterPoolStructureDto.fromDomain ps, QpSorterEvalDto.fromDomain se)
        | qpSorterPoolType.Sgd (ps, sgd) ->
            Sgd (QpSorterPoolStructureDto.fromDomain ps, QpSgdTypeDto.fromDomain sgd)

    let toDomain (dto: qpSorterPoolTypeDto) : qpSorterPoolType =
        match dto with
        | Random (psDto, seDto) ->
            qpSorterPoolType.Random (QpSorterPoolStructureDto.toDomain psDto, QpSorterEvalDto.toDomain seDto)
        | Sgd (psDto, sgdDto) ->
            qpSorterPoolType.Sgd (QpSorterPoolStructureDto.toDomain psDto, QpSgdTypeDto.toDomain sgdDto)

module QpSgdTypeDto =

    let fromDomain (domain: qpSgdType) : qpSgdTypeDto =
        match domain with
        | qpSgdType.FixedPools (p1, p2, sm) ->
            FixedPools (
                QpSorterPoolTypeDto.fromDomain p1,
                QpSorterPoolTypeDto.fromDomain p2,
                QpSorterMutateDto.fromDomain sm
            )

    let toDomain (dto: qpSgdTypeDto) : qpSgdType =
        match dto with
        | FixedPools (p1Dto, p2Dto, smDto) ->
            qpSgdType.FixedPools (
                QpSorterPoolTypeDto.toDomain p1Dto,
                QpSorterPoolTypeDto.toDomain p2Dto,
                QpSorterMutateDto.toDomain smDto
            )


// --- queryParamType ---

type queryParamTypeDto =
    | SortableTest of qpSortableTestTypeDto
    | SorterEval of qpSorterEvalDto
    | SorterMutate of qpSorterMutateDto
    | SorterSgd of qpSgdTypeDto

module QueryParamTypeDto =

    let fromDomain (domain: queryProperties) : queryParamTypeDto =
        match domain with
        | queryProperties.SortableTest st -> SortableTest (QpSortableTestTypeDto.fromDomain st)
        | queryProperties.SorterEval se -> SorterEval (QpSorterEvalDto.fromDomain se)
        | queryProperties.SorterMutate sm -> SorterMutate (QpSorterMutateDto.fromDomain sm)
        | queryProperties.SorterSgd sgd -> SorterSgd (QpSgdTypeDto.fromDomain sgd)

    let toDomain (dto: queryParamTypeDto) : queryProperties =
        try
            match dto with
            | SortableTest stDto -> queryProperties.SortableTest (QpSortableTestTypeDto.toDomain stDto)
            | SorterEval seDto -> queryProperties.SorterEval (QpSorterEvalDto.toDomain seDto)
            | SorterMutate smDto -> queryProperties.SorterMutate (QpSorterMutateDto.toDomain smDto)
            | SorterSgd sgdDto -> queryProperties.SorterSgd (QpSgdTypeDto.toDomain sgdDto)
        with
        | ex -> failwith $"Failed to convert QueryParamTypeDto: {ex.Message}"