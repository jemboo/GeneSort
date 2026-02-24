namespace GeneSort.Core.Mp

open System
open FSharp.UMX
open GeneSort.Core
open MessagePack

[<MessagePackObject>]
type rngTypeDto =
    | Lcg = 0
    | Net = 1

module RngTypeDto =
    let toDto (rngType: rngType) : rngTypeDto =
        match rngType with
        | Lcg -> rngTypeDto.Lcg
        | Net -> rngTypeDto.Net
    
    let fromDto (dto: rngTypeDto) : rngType =
        match dto with
        | rngTypeDto.Lcg -> Lcg
        | rngTypeDto.Net -> Net
        | _ -> failwith "Invalid rngTypeDto value"

[<MessagePackObject>]
type rngFactoryDto =
    { [<Key(0)>] Id: string
      [<Key(1)>] RngType: rngTypeDto }

module RngFactoryDto =
    let fromDomain (factory: rngFactory) : rngFactoryDto =
        { Id = (%factory.Id).ToString()
          RngType = RngTypeDto.toDto factory.RngType }
    
    let toDomain (dto: rngFactoryDto) : rngFactory =
        let rngType = RngTypeDto.fromDto dto.RngType
        let factory = rngFactory.getFactory rngType
        
        // Validate that the ID matches the expected factory ID
        let expectedId = %factory.Id
        let providedId = Guid.Parse(dto.Id)
        
        if expectedId <> providedId then
            failwithf "Factory ID mismatch: expected %A but got %A for rngType %A" 
                expectedId providedId rngType
        
        factory