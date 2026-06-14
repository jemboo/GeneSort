namespace GeneSort.Dispatch.V1.SortableTest

open FSharp.UMX
open GeneSort.Core
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Sorting


type executorType = 
    | Generator
    | Unknown

module ExecutorType =
    let toString = function
        | Generator -> "Generator"
        | Unknown -> "Unknown"


module CommonSortableTest =

    let projectName = "SortableTest" |> UMX.tag<projectName>
    //let queryName = "SortableTest" |> UMX.tag<queryName>
    let mergeDatabaseName = "Merge" |> UMX.tag<databaseName>

    let _projectRngType = rngType.Lcg
    let _dataFormatInt8v512 = sortableDataFormat.Int8Vector512

    let mergeDatabaseFolder = 
                            "c:\\Projects\\SortableTest\\Merge\\Data"
                            |> UMX.tag<pathToRootFolder>

    // MergeDimensions
    let allMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4; 6; 8] |> List.map string)
    let mergeDimension2 = 
            (runParameters.mergeDimensionKey, [2;] |> List.map string)
    let mergeDimension4 = 
            (runParameters.mergeDimensionKey, [4;] |> List.map string)
    let mergeDimension8 = 
            (runParameters.mergeDimensionKey, [8;] |> List.map string)
    let lowMergeDimensions = 
            (runParameters.mergeDimensionKey, [2; 3; 4;] |> List.map string)
    let highMergeDimensions = 
            (runParameters.mergeDimensionKey, [6; 8] |> List.map string)
    
    // SortingWidths
    let testSortingWidth = 
            (runParameters.sortingWidthKey, [16;] |> List.map string)

    let smallSortingWidths = 
            (runParameters.sortingWidthKey, [16; 18; 24; 32; 36; 48; 64] |> List.map string)  
            
    let mediumSortingWidths = 
            (runParameters.sortingWidthKey,  [96; 128;]  |> List.map string)

    let largeSortingWidths = 
            (runParameters.sortingWidthKey,  [192; 256; 512]  |> List.map string)

    let smallP2SortingWidths = 
            (runParameters.sortingWidthKey, [16; 32; 64;] |> List.map string)

    let mediumP2SortingWidths = 
            (runParameters.sortingWidthKey, [128;] |> List.map string)

    let largeP2SortingWidths = 
            (runParameters.sortingWidthKey, [256; 512;] |> List.map string)


    // RngType
    let projectRngType = 
            (runParameters.rngTypeKey, [_projectRngType] |> List.map RngType.toString)


    // DataFormats
    let dataFormatInt8v512 = 
            (runParameters.sortableDataFormatKey, [_dataFormatInt8v512] |> List.map SortableDataFormat.toString)

    let noSuffixSuffixType = 
            (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix] |> List.map MergeSuffixType.toString)

