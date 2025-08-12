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
      [<Key(2)>] Mssi: MssiDTO option
      [<Key(3)>] Msrs: MsrsDTO option
      [<Key(4)>] Msuf4: Msuf4DTO option
      [<Key(5)>] Msuf6: Msuf6DTO option }

module SorterModelDto =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    let toSorterModelDto (sorterModel: SorterModel) : SorterModelDto =
        match sorterModel with
        | Msce msce ->
            { Type = "MsceRandGen"
              Msce = Some (MsceDto.toMsceDTO msce)
              Mssi = None
              Msrs = None
              Msuf4 = None
              Msuf6 = None }
        | Mssi mssi ->
            { Type = "MssiRandGen"
              Msce = None
              Mssi = Some (MssiDTO.toMssiDTO mssi)
              Msrs = None
              Msuf4 = None
              Msuf6 = None }
        | Msrs msrs ->
            { Type = "MsrsRandGen"
              Msce = None
              Mssi = None
              Msrs = Some (MsrsDTO.toMsrsDTO msrs)
              Msuf4 = None
              Msuf6 = None }
        | Msuf4 msuf4 ->
            { Type = "Msuf4RandGen"
              Msce = None
              Mssi = None
              Msrs = None
              Msuf4 = Some (Msuf4DTO.toMsuf4DTO msuf4)
              Msuf6 = None }
        | Msuf6 msuf6 ->
            { Type = "Msuf6RandGen"
              Msce = None
              Mssi = None
              Msrs = None
              Msuf4 = None
              Msuf6 = Some (Msuf6DTO.toMsuf6DTO msuf6) }

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
                    match MssiDTO.toMssi mssiDto with
                    | Ok mssi -> Ok (Mssi mssi)
                    | Error err ->
                        let msg = match err with
                                  | MssiDTO.InvalidPermSiCount m -> m
                                  | MssiDTO.InvalidWidth m -> m
                        Error $"Failed to convert MssiDTO: {msg}"
                | None -> Error "MssiRandGen requires Mssi data"
            | "MsrsRandGen" ->
                match dto.Msrs with
                | Some msrsDto ->
                    match MsrsDTO.toMsrs msrsDto with
                    | Ok msrs -> Ok (Msrs msrs)
                    | Error err ->
                        let msg = match err with
                                  | MsrsDTO.NullPermRssArray m -> m
                                  | MsrsDTO.EmptyPermRssArray m -> m
                                  | MsrsDTO.InvalidWidth m -> m
                                  | MsrsDTO.MismatchedPermRsOrder m -> m
                                  | MsrsDTO.PermRsConversionError e ->
                                      match e with
                                      | Perm_RsDTO.Perm_RsDTOError.OrderTooSmall m -> m
                                      | Perm_RsDTO.Perm_RsDTOError.OrderNotDivisibleByTwo m -> m
                                      | Perm_RsDTO.Perm_RsDTOError.NotReflectionSymmetric m -> m
                                      | Perm_RsDTO.Perm_RsDTOError.PermSiConversionError et ->
                                        match et with
                                        | Perm_SiDTO.Perm_SiDTOError.EmptyArray m -> m
                                        | Perm_SiDTO.Perm_SiDTOError.InvalidPermutation m -> m
                                        | Perm_SiDTO.Perm_SiDTOError.NotSelfInverse m -> m
                                        | Perm_SiDTO.Perm_SiDTOError.NullArray m -> m
                                        | Perm_SiDTO.Perm_SiDTOError.PermutationConversionError e ->
                                            match e with
                                            | PermutationDTO.PermutationDTOError.EmptyArray m -> m
                                            | PermutationDTO.PermutationDTOError.InvalidPermutation m -> m
                                            | PermutationDTO.PermutationDTOError.NullArray m -> m


                        Error $"Failed to convert MsrsDTO: {msg}"
                | None -> Error "MsrsRandGen requires Msrs data"
            | "Msuf4RandGen" ->
                match dto.Msuf4 with
                | Some msuf4Dto ->
                    match Msuf4DTO.toMsuf4 msuf4Dto with
                    | Ok msuf4 -> Ok (Msuf4 msuf4)
                    | Error err ->
                        let msg = match err with
                                  | Msuf4DTO.NullTwoOrbitUnfolder4sArray m -> m
                                  | Msuf4DTO.EmptyTwoOrbitUnfolder4sArray m -> m
                                  | Msuf4DTO.InvalidSortingWidth m -> m
                                  | Msuf4DTO.MismatchedTwoOrbitUnfolder4Order m -> m
                                  | Msuf4DTO.TwoOrbitUnfolder4ConversionError e ->
                                      match e with
                                      | TwoOrbitUf4DTO.TwoOrbitUf4DTOError.EmptyTwoOrbitUfSteps m -> m
                                      | TwoOrbitUf4DTO.TwoOrbitUf4DTOError.StepConversionError e -> 
                                        match e with
                                        |TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError.InvalidOrder m -> m
                                        |TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError.InvalidTwoOrbitTypesLength m -> m
                                        |TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError.NotEvenOrder m -> m


                        Error $"Failed to convert Msuf4DTO: {msg}"
                | None -> Error "Msuf4RandGen requires Msuf4 data"
            | "Msuf6RandGen" ->
                match dto.Msuf6 with
                | Some msuf6Dto ->
                    match Msuf6DTO.toMsuf6 msuf6Dto with
                    | Ok msuf6 -> Ok (Msuf6 msuf6)
                    | Error err ->
                        let msg = match err with
                                  | Msuf6DTO.NullTwoOrbitUnfolder6sArray m -> m
                                  | Msuf6DTO.EmptyTwoOrbitUnfolder6sArray m -> m
                                  | Msuf6DTO.InvalidSortingWidth m -> m
                                  | Msuf6DTO.MismatchedTwoOrbitUnfolder6Order m -> m
                                  | Msuf6DTO.TwoOrbitUnfolder6ConversionError e ->
                                      match e with
                                      | TwoOrbitUf6DTO.TwoOrbitUf6DTOError.EmptyTwoOrbitUnfolderSteps m -> m
                                      | TwoOrbitUf6DTO.TwoOrbitUf6DTOError.InvalidStepOrder m -> m
                                      | TwoOrbitUf6DTO.TwoOrbitUf6DTOError.StepConversionError e ->
                                        match e with
                                        |TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError.InvalidOrder m -> m
                                        |TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError.InvalidTwoOrbitTypesLength m -> m
                                        |TwoOrbitUnfolderStepDTO.TwoOrbitUnfolderStepDTOError.NotEvenOrder m -> m

                        Error $"Failed to convert Msuf6DTO: {msg}"
                | None -> Error "Msuf6RandGen requires Msuf6 data"
            | _ -> Error $"Unknown SorterModel type: {dto.Type}"
        with
        | ex -> Error $"Failed to convert SorterModelDto: {ex.Message}"