namespace GeneSort.Core.Mp

open System
open FSharp.UMX
open GeneSort.Core

type rngTypeDto =
    | Lcg = 0
    | Net = 1

module RngTypeDto =
    let fromDomain (rngType: rngType) : rngTypeDto =
        match rngType with
        | Lcg -> rngTypeDto.Lcg
        | Net -> rngTypeDto.Net
    
    let toDomain (dto: rngTypeDto) : rngType =
        match dto with
        | rngTypeDto.Lcg -> Lcg
        | rngTypeDto.Net -> Net
        | _ -> failwith "Invalid rngTypeDto value"

type rngFactoryDto = {
    id: string
    rngTypeDto: rngTypeDto
}

module RngFactoryDto =
    let fromDomain (factory: rngFactory) : rngFactoryDto =
        { id = (%factory.Id).ToString()
          rngTypeDto = RngTypeDto.fromDomain factory.RngType }
    
    let toDomain (dto: rngFactoryDto) : rngFactory =
        let rngType = RngTypeDto.toDomain dto.rngTypeDto
        let factory = rngFactory.getFactory rngType
        
        // Validate that the ID matches the expected factory ID
        let expectedId = %factory.Id
        let providedId = Guid.Parse(dto.id)
        
        if expectedId <> providedId then
            failwithf "Factory ID mismatch: expected %A but got %A for rngType %A" 
                expectedId providedId rngType
        
        factory