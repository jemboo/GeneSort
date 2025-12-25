namespace GeneSort.Project

open System

open FSharp.UMX
open System.Threading

open GeneSort.Sorter
open GeneSort.Runs
open GeneSort.Db
open GeneSort.Model.Sortable


module SortableIntMerges =

    let projectName = "SortableIntMerges"  |> UMX.tag<projectName>
    let projectDesc = "Calc and save large SortableIntMerges"


    let makeQueryParams 
            (repl: int<replNumber> option) 
            (sortingWidth: int<sortingWidth> option)
            (mergeDimension: int<mergeDimension> option)
            (mergeFillType: mergeFillType option)
            (sortableDataType: sortableDataType option)
            (outputDataType: outputDataType) =
             
        queryParams.create(
            Some projectName,
            repl,
            outputDataType,
            [|
                (runParameters.sortingWidthKey, sortingWidth |> SortingWidth.toString); 
                (runParameters.mergeDimensionKey, mergeDimension |> MergeDimension.toString);
                (runParameters.mergeFillTypeKey, mergeFillType |> MergeFillType.toString);
                (runParameters.sortableDataTypeKey, sortableDataType |> SortableDataType.toString);
            |])


    let makeQueryParamsFromRunParams
            (runParams: runParameters) 
            (outputDataType: outputDataType) =

        makeQueryParams
            (runParams.GetRepl())
            (runParams.GetSortingWidth())
            (runParams.GetMergeDimension())
            (runParams.GetMergeFillType())
            (runParams.GetSortableDataType())

            outputDataType




    let sortableArrayDataTypeKeyValues = 
            [ Some sortableDataType.Ints; Some sortableDataType.Bools;] |> List.map(SortableDataType.toString)
  
    let sortableArrayDataTypeKeys () : string*string list =
        (runParameters.sortableDataTypeKey, sortableArrayDataTypeKeyValues )

    let sortingWidthValues = 
        [16; 18; 24; 32; 36; 48; 64] |> List.map(fun d -> d.ToString())

    let sortingWidths() : string*string list =
        (runParameters.sortingWidthKey, sortingWidthValues)


    let mergeDimensionValues = 
        [2; 3; 4; 6; 8;] |> List.map(fun d -> d.ToString())

    let mergeDimensions() : string*string list =
        (runParameters.mergeDimensionKey, mergeDimensionValues)


    let mergeFillTypeValues = 
         [mergeFillType.NoFill; mergeFillType.VanVoorhis;] |> List.map(fun d -> d.ToString())

    let mergeFillTypes() : string*string list =
        (runParameters.mergeFillTypeKey, mergeFillTypeValues)


    let paramMapFilter (runParameters: runParameters) = 
        let sortingWidth = runParameters.GetSortingWidth().Value
        let mergeDimension = runParameters.GetMergeDimension().Value
        let sortableDataType = runParameters.GetSortableDataType().Value
        if (%sortingWidth % %mergeDimension = 0) then 
            if ((sortableDataType.IsBools) && %sortingWidth > 32 ) then
                None
            else
                Some runParameters 
        else None



    let paramMapRefiner (runParametersSeq: runParameters seq) : runParameters seq = 

        let enhancer (runParameters : runParameters) : runParameters =
            let queryParams = makeQueryParamsFromRunParams runParameters (outputDataType.RunParameters)
            runParameters.SetId ((queryParams.Id.ToString()) |> UMX.tag<idValue>)
            runParameters.SetRunFinished false
            runParameters.SetProjectName projectName
            runParameters


        seq {
            for runParameters in runParametersSeq do
                    let filtrate = paramMapFilter runParameters
                    if filtrate.IsSome then
                        let retVal = filtrate.Value |> enhancer
                        yield retVal
        }

    let parameterSpans = 
        [
            sortingWidths();
            sortableArrayDataTypeKeys(); 
            mergeDimensions(); 
            mergeFillTypes(); 
        ]
        
    let outputDataTypes = 
            [|                
                outputDataType.SortableTestSet None;
                outputDataType.RunParameters;
            |]

    let project = 
            project.create 
                projectName 
                projectDesc
                parameterSpans
                outputDataTypes


    let executor
            (db: IGeneSortDb)
            (runParameters: runParameters) 
            (cts: CancellationTokenSource) 
            (progress: IProgress<string> option) : Async<unit> =

        async {
            let index = runParameters.GetId().Value  
            let repl = runParameters.GetRepl().Value
            let sortingWidth = runParameters.GetSortingWidth().Value
            let mergeDimension = runParameters.GetMergeDimension().Value
            let mergeFillType = runParameters.GetMergeFillType().Value
            let sortableDataType = runParameters.GetSortableDataType().Value

            match progress with
            | Some p -> p.Report(sprintf "Executing Run %s_%d  %s" %index %repl (runParameters.toString()))
            | None -> ()
        
            // Check cancellation before starting expensive operations
            cts.Token.ThrowIfCancellationRequested()
        
            match progress with
            | Some p -> p.Report(sprintf "Run %s_%d: Creating sorter set" %index %repl)
            | None -> ()
        
            // Check cancellation before generating sorters
            cts.Token.ThrowIfCancellationRequested()

            let sortableTestModel = msasM.create sortingWidth mergeDimension mergeFillType |> sortableTestModel.MsasMi
            let sortableTests = SortableTestModel.makeSortableTests sortableTestModel sortableDataType

        
            match progress with
            | Some p -> p.Report(sprintf "Run %s_%d: Evaluating sorter set" %index %repl)
            | None -> ()


            cts.Token.ThrowIfCancellationRequested()
            match progress with
            | Some p -> p.Report(sprintf "Run %s_%d: Saving sorterSet test results" %index %repl)
            | None -> ()

            //// Save sortableTest
            let queryParamsForSortableTestSet = makeQueryParamsFromRunParams runParameters (outputDataType.SortableTest None) 
            do! db.saveAsync queryParamsForSortableTestSet (sortableTests |> outputData.SortableTest)

            //// Save sorterModelSetMaker
            //let queryParamsForSorterModelSetMaker = 
            //    queryParams.createFromRunParams (outputDataType.SorterModelSetMaker None) runParameters
            //do! db.saveAsync queryParamsForSorterModelSetMaker (sorterModelSetMaker |> outputData.SorterModelSetMaker)

            // Mark run as finished
            runParameters.SetRunFinished true
       
        }