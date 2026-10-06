namespace rec GeneSort.Project.Mp.V1

open GeneSort.Project.V1

// --- qpSortableTestsRestriction ---

type qpSortableTestsRestrictionDto =
    | NoRestriction
    | Split

module QpSortableTestsRestrictionDto =

    let fromDomain (domain: qpSortableTestsRestriction) : qpSortableTestsRestrictionDto =
        match domain with
        | qpSortableTestsRestriction.NoRestriction -> NoRestriction
        | qpSortableTestsRestriction.Split -> Split

    let toDomain (dto: qpSortableTestsRestrictionDto) : qpSortableTestsRestriction =
        match dto with
        | NoRestriction -> qpSortableTestsRestriction.NoRestriction
        | Split -> qpSortableTestsRestriction.Split


// --- qpSortableTestsType ---

type qpSortableTestsTypeDto =
    | Standard of qpSortableTestsRestrictionDto
    | Merge of qpSortableTestsRestrictionDto
    | Prefix of qpSortableTestsRestrictionDto

module QpSortableTestsTypeDto =

    let fromDomain (domain: qpSortableTestsType) : qpSortableTestsTypeDto =
        match domain with
        | qpSortableTestsType.Standard r -> qpSortableTestsTypeDto.Standard (QpSortableTestsRestrictionDto.fromDomain r)
        | qpSortableTestsType.Merge r -> Merge (QpSortableTestsRestrictionDto.fromDomain r)
        | qpSortableTestsType.Prefix r -> Prefix (QpSortableTestsRestrictionDto.fromDomain r)

    let toDomain (dto: qpSortableTestsTypeDto) : qpSortableTestsType =
        match dto with
        | qpSortableTestsTypeDto.Standard rDto -> qpSortableTestsType.Standard (QpSortableTestsRestrictionDto.toDomain rDto)
        | Merge rDto -> qpSortableTestsType.Merge (QpSortableTestsRestrictionDto.toDomain rDto)
        | Prefix rDto -> qpSortableTestsType.Prefix (QpSortableTestsRestrictionDto.toDomain rDto)


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
    | Standard of qpSorterTypeDto * qpSortableTestsTypeDto

module QpSorterEvalDto =

    let fromDomain (domain: qpSorterEval) : qpSorterEvalDto =
        match domain with
        | qpSorterEval.Standard (st, tt) ->
            Standard (QpSorterTypeDto.fromDomain st, QpSortableTestsTypeDto.fromDomain tt)

    let toDomain (dto: qpSorterEvalDto) : qpSorterEval =
        match dto with
        | Standard (stDto, ttDto) ->
            qpSorterEval.Standard (QpSorterTypeDto.toDomain stDto, QpSortableTestsTypeDto.toDomain ttDto)


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

type queryPropertiesDto =
    | SortableTests of qpSortableTestsTypeDto
    | SorterEval of qpSorterEvalDto
    | SorterMutate of qpSorterMutateDto
    | SorterSgd of qpSgdTypeDto

module QueryPropertiesDto =

    let fromDomain (domain: queryProperties) : queryPropertiesDto =
        match domain with
        | queryProperties.SortableTests st -> SortableTests (QpSortableTestsTypeDto.fromDomain st)
        | queryProperties.SorterEval se -> SorterEval (QpSorterEvalDto.fromDomain se)
        | queryProperties.SorterMutate sm -> SorterMutate (QpSorterMutateDto.fromDomain sm)
        | queryProperties.SorterSgd sgd -> SorterSgd (QpSgdTypeDto.fromDomain sgd)

    let toDomain (dto: queryPropertiesDto) : queryProperties =
        try
            match dto with
            | SortableTests stDto -> queryProperties.SortableTests (QpSortableTestsTypeDto.toDomain stDto)
            | SorterEval seDto -> queryProperties.SorterEval (QpSorterEvalDto.toDomain seDto)
            | SorterMutate smDto -> queryProperties.SorterMutate (QpSorterMutateDto.toDomain smDto)
            | SorterSgd sgdDto -> queryProperties.SorterSgd (QpSgdTypeDto.toDomain sgdDto)
        with
        | ex -> failwith $"Failed to convert QueryParamTypeDto: {ex.Message}"