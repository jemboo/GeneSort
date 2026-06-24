namespace GeneSort.Core.Mp.TwoOrbitUnfolder

open System
open GeneSort.Core
open MessagePack

[<MessagePackObject; Struct>]
type twoOrbitUf4Dto =
    { [<Key(0)>] seedType: twoOrbitPairType
      [<Key(1)>] twoOrbitUfStepDtos: twoOrbitUfStepDto array }
    
    static member Create 
                    (seedType: twoOrbitPairType) 
                    (twoOrbitUnfolderSteps: twoOrbitUfStepDto array) : twoOrbitUf4Dto =
        if Array.isEmpty twoOrbitUnfolderSteps then
            failwith "TwoOrbitUnfolderSteps array cannot be empty"
        else
            { seedType = seedType
              twoOrbitUfStepDtos = twoOrbitUnfolderSteps }

module TwoOrbitUf4Dto =

    let fromDomain (tou: twoOrbitUf4) : twoOrbitUf4Dto =
        { seedType = tou.TwoOrbitPairType
          twoOrbitUfStepDtos = tou.TwoOrbitUnfolderSteps |> Array.map TwoOrbitUnfolderStepDto.fromDomain }

    let toDomain (dto: twoOrbitUf4Dto) : twoOrbitUf4 =
        let steps = 
            dto.twoOrbitUfStepDtos 
            |> Array.map TwoOrbitUnfolderStepDto.toDomain
        
        twoOrbitUf4.create dto.seedType steps

    let getOrder (twoOrbitUnfolder: twoOrbitUf4Dto) : int =
        4 * (MathUtils.integerPower 2 (Array.length twoOrbitUnfolder.twoOrbitUfStepDtos))