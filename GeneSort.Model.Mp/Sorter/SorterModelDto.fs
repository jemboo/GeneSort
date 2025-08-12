namespace GeneSort.Model.Mp.Sorter

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter
open GeneSort.Model.Mp.Sorter.Ce
open GeneSort.Model.Mp.Sorter.Si
open GeneSort.Model.Mp.Sorter.Rs
open GeneSort.Model.Mp.Sorter.Uf4
open GeneSort.Model.Mp.Sorter.Uf6
open MessagePack
open MessagePack.Resolvers
open MessagePack.FSharp
open GeneSort.Core.Mp
open GeneSort.Core.Mp.TwoOrbitUnfolder

[<MessagePackObject>]
type SorterModelDto =
    { [<Key(0)>] Type: string
      [<Key(1)>] Msce: MsceDto option
      [<Key(2)>] Mssi: MssiDto option
      [<Key(3)>] Msrs: MsrsDto option
      [<Key(4)>] Msuf4: Msuf4Dto option
      [<Key(5)>] Msuf6: Msuf6Dto option }

module SorterModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelDto (sorterModel: SorterModel) : SorterModelDto =
        match sorterModel with
        | Msce msce ->
            { Type = "MsceRandGen"
              Msce = Some (MsceDto.toMsceDto msce)
              Mssi = None
              Msrs = None
              Msuf4 = None
              Msuf6 = None }
        | Mssi mssi ->
            { Type = "MssiRandGen"
              Msce = None
              Mssi = Some (MssiDto.toMssiDto mssi)
              Msrs = None
              Msuf4 = None
              Msuf6 = None }
        | Msrs msrs ->
            { Type = "MsrsRandGen"
              Msce = None
              Mssi = None
              Msrs = Some (MsrsDto.toMsrsDto msrs)
              Msuf4 = None
              Msuf6 = None }
        | Msuf4 msuf4 ->
            { Type = "Msuf4RandGen"
              Msce = None
              Mssi = None
              Msrs = None
              Msuf4 = Some (Msuf4Dto.toMsuf4Dto msuf4)
              Msuf6 = None }
        | Msuf6 msuf6 ->
            { Type = "Msuf6RandGen"
              Msce = None
              Mssi = None
              Msrs = None
              Msuf4 = None
              Msuf6 = Some (Msuf6Dto.toMsuf6Dto msuf6) }

    let fromSorterModelDto (dto: SorterModelDto) : Result<SorterModel, string> =
        try
            match dto.Type with
            | "MsceRandGen" ->
                match dto.Msce with
                | Some msceDto ->
                    match MsceDto.toMsce msceDto with
                    | Ok msce -> Ok (Msce msce)
                    | Error err ->
                        let msg = match err with
                                  | MsceDto.InvalidCeCodesLength m -> m
                                  | MsceDto.InvalidSortingWidth m -> m
                        Error $"Failed to convert MsceDto: {msg}"
                | None -> Error "MsceRandGen requires Msce data"
            | "MssiRandGen" ->
                match dto.Mssi with
                | Some mssiDto ->
                    match MssiDto.toMssi mssiDto with
                    | Ok mssi -> Ok (Mssi mssi)
                    | Error err ->
                        let msg = match err with
                                  | MssiDto.InvalidPermSiCount m -> m
                                  | MssiDto.InvalidWidth m -> m
                        Error $"Failed to convert MssiDto: {msg}"
                | None -> Error "MssiRandGen requires Mssi data"
            | "MsrsRandGen" ->
                match dto.Msrs with
                | Some msrsDto ->
                    match MsrsDto.toMsrs msrsDto with
                    | Ok msrs -> Ok (Msrs msrs)
                    | Error err ->
                        let msg = match err with
                                  | MsrsDto.NullPermRssArray m -> m
                                  | MsrsDto.EmptyPermRssArray m -> m
                                  | MsrsDto.InvalidWidth m -> m
                                  | MsrsDto.MismatchedPermRsOrder m -> m
                                  | MsrsDto.PermRsConversionError e ->
                                      match e with
                                      | Perm_RsDto.Perm_RsDtoError.OrderTooSmall m -> m
                                      | Perm_RsDto.Perm_RsDtoError.OrderNotDivisibleByTwo m -> m
                                      | Perm_RsDto.Perm_RsDtoError.NotReflectionSymmetric m -> m
                                      | Perm_RsDto.Perm_RsDtoError.PermSiConversionError et ->
                                        match et with
                                        | Perm_SiDto.Perm_SiDtoError.EmptyArray m -> m
                                        | Perm_SiDto.Perm_SiDtoError.InvalidPermutation m -> m
                                        | Perm_SiDto.Perm_SiDtoError.NotSelfInverse m -> m
                                        | Perm_SiDto.Perm_SiDtoError.NullArray m -> m
                                        | Perm_SiDto.Perm_SiDtoError.PermutationConversionError e ->
                                            match e with
                                            | PermutationDto.PermutationDtoError.EmptyArray m -> m
                                            | PermutationDto.PermutationDtoError.InvalidPermutation m -> m
                                            | PermutationDto.PermutationDtoError.NullArray m -> m


                        Error $"Failed to convert MsrsDto: {msg}"
                | None -> Error "MsrsRandGen requires Msrs data"
            | "Msuf4RandGen" ->
                match dto.Msuf4 with
                | Some msuf4Dto ->
                    match Msuf4Dto.fromMsuf4Dto msuf4Dto with
                    | Ok msuf4 -> Ok (Msuf4 msuf4)
                    | Error err ->
                        let msg = match err with
                                  | Msuf4Dto.NullTwoOrbitUnfolder4sArray m -> m
                                  | Msuf4Dto.EmptyTwoOrbitUnfolder4sArray m -> m
                                  | Msuf4Dto.InvalidSortingWidth m -> m
                                  | Msuf4Dto.MismatchedTwoOrbitUnfolder4Order m -> m
                                  | Msuf4Dto.TwoOrbitUnfolder4ConversionError e ->
                                      match e with
                                      | TwoOrbitUf4Dto.TwoOrbitUf4DtoError.EmptyTwoOrbitUfSteps m -> m
                                      | TwoOrbitUf4Dto.TwoOrbitUf4DtoError.StepConversionError e -> 
                                        match e with
                                        |TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError.InvalidOrder m -> m
                                        |TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError.InvalidTwoOrbitTypesLength m -> m
                                        |TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError.NotEvenOrder m -> m


                        Error $"Failed to convert Msuf4Dto: {msg}"
                | None -> Error "Msuf4RandGen requires Msuf4 data"
            | "Msuf6RandGen" ->
                match dto.Msuf6 with
                | Some msuf6Dto ->
                    match Msuf6Dto.fromMsuf6Dto msuf6Dto with
                    | Ok msuf6 -> Ok (Msuf6 msuf6)
                    | Error err ->
                        let msg = match err with
                                  | Msuf6Dto.NullTwoOrbitUnfolder6sArray m -> m
                                  | Msuf6Dto.EmptyTwoOrbitUnfolder6sArray m -> m
                                  | Msuf6Dto.InvalidSortingWidth m -> m
                                  | Msuf6Dto.MismatchedTwoOrbitUnfolder6Order m -> m
                                  | Msuf6Dto.TwoOrbitUnfolder6ConversionError e ->
                                      match e with
                                      | TwoOrbitUf6Dto.TwoOrbitUf6DtoError.EmptyTwoOrbitUnfolderSteps m -> m
                                      | TwoOrbitUf6Dto.TwoOrbitUf6DtoError.InvalidStepOrder m -> m
                                      | TwoOrbitUf6Dto.TwoOrbitUf6DtoError.StepConversionError e ->
                                        match e with
                                        |TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError.InvalidOrder m -> m
                                        |TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError.InvalidTwoOrbitTypesLength m -> m
                                        |TwoOrbitUnfolderStepDto.TwoOrbitUnfolderStepDtoError.NotEvenOrder m -> m

                        Error $"Failed to convert Msuf6Dto: {msg}"
                | None -> Error "Msuf6RandGen requires Msuf6 data"
            | _ -> Error $"Unknown SorterModel type: {dto.Type}"
        with
        | ex -> Error $"Failed to convert SorterModelDto: {ex.Message}"