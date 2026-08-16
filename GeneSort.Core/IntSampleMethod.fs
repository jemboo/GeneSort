namespace GeneSort.Core

open System
open System.Collections.Generic

/// Defines strategy variations for integer sampling across distinct distribution curves.
type intSampleMethod =
    | Constant of stepSize: int
    | Exponential of growthRate: float * firstDelta: int
    | Cyclic of baseMethod: intSampleMethod * cycleLength: int
    | Prefixed of prefixLength: int * prefixMethod: intSampleMethod * mainMethod: intSampleMethod

type samplingConfig = {
    Name: string
    Min: int
    MaxCount: int option
    Scale: float
    Method: intSampleMethod
}

module IntSampleMethod =

    /// Serializes an IntSampleMethod into a string representation.
    let rec toString (method: intSampleMethod) : string =
        match method with
        | Constant step -> 
            sprintf "Constant(%d)" step
        | Exponential (rate, firstDelta) -> 
            sprintf "Exponential(%f, %d)" rate firstDelta
        | Cyclic (baseMethod, cycleLen) -> 
            sprintf "Cyclic(%s, %d)" (toString baseMethod) cycleLen
        | Prefixed (prefixLen, prefixMethod, mainMethod) -> 
            sprintf "Prefixed(%d, %s, %s)" prefixLen (toString prefixMethod) (toString mainMethod)

    /// Parses a string representation back into an IntSampleMethod.
    let rec fromString (s: string) : intSampleMethod =
        let trimmed = s.Trim()
        
        let splitTopLevelArgs (str: string) =
            let mutable depth = 0
            let mutable current = ""
            let args = ResizeArray<string>()
            for ch in str do
                match ch with
                | '(' -> 
                    depth <- depth + 1
                    current <- current + string ch
                | ')' -> 
                    depth <- depth - 1
                    current <- current + string ch
                | ',' when depth = 0 ->
                    args.Add(current.Trim())
                    current <- ""
                | _ -> 
                    current <- current + string ch
            if not (String.IsNullOrWhiteSpace current) then
                args.Add(current.Trim())
            args |> Seq.toList

        if trimmed.StartsWith("Constant(") && trimmed.EndsWith(")") then
            let inner = trimmed.Substring(9, trimmed.Length - 10)
            Constant (int (inner.Trim()))

        elif (trimmed.StartsWith("Exponential(") || trimmed.StartsWith("ExponentialDecrease(")) && trimmed.EndsWith(")") then
            let prefixLen = if trimmed.StartsWith("ExponentialDecrease(") then 20 else 12
            let inner = trimmed.Substring(prefixLen, trimmed.Length - (prefixLen + 1))
            let args = splitTopLevelArgs inner
            if args.Length = 1 then
                Exponential (float args.[0], 1)
            elif args.Length = 2 then
                Exponential (float args.[0], int args.[1])
            else
                invalidArg "s" "Exponential must have 1 or 2 arguments: growthRate, [firstDelta]."

        elif trimmed.StartsWith("Cyclic(") && trimmed.EndsWith(")") then
            let inner = trimmed.Substring(7, trimmed.Length - 8)
            let args = splitTopLevelArgs inner
            if args.Length <> 2 then
                invalidArg "s" "Cyclic must have 2 arguments: baseMethod, cycleLength."
            let baseMethod = fromString args.[0]
            let cycleLen = int args.[1]
            Cyclic (baseMethod, cycleLen)

        elif trimmed.StartsWith("Prefixed(") && trimmed.EndsWith(")") then
            let inner = trimmed.Substring(9, trimmed.Length - 10)
            let args = splitTopLevelArgs inner
            if args.Length <> 3 then
                invalidArg "s" "Prefixed must have 3 arguments: prefixLength, prefixMethod, mainMethod."
            let prefixLen = int args.[0]
            let prefixMethod = fromString args.[1]
            let mainMethod = fromString args.[2]
            Prefixed (prefixLen, prefixMethod, mainMethod)

        else
            invalidArg "s" (sprintf "Unrecognized IntSampleMethod format: %s" s)

    /// Unbounded lazy sequence generator for a sampling method starting at minVal.
    let rec generateUnbounded (method: intSampleMethod) (minVal: int) : int seq =
        seq {
            match method with
            | Constant step ->
                let stepSize = max 1 step
                let mutable current = minVal
                while true do
                    yield current
                    current <- current + stepSize

            | Exponential (rate, firstDelta) ->
                let r = if rate <= 0.0 then 0.05 else rate
                let mutable current = float minVal
                let mutable step = float (max 1 firstDelta)
                while true do
                    yield int (round current)
                    current <- current + step
                    step <- step * (1.0 + r)

            | Cyclic (baseMethod, cycleLen) ->
                let len = max 1 cycleLen
                let mutable currentStart = minVal
                while true do
                    let samples = generateUnbounded baseMethod currentStart |> Seq.take len |> Seq.toList
                    yield! samples
                    if not (List.isEmpty samples) then
                        currentStart <- List.last samples + 1

            | Prefixed (prefixLen, prefixMethod, mainMethod) ->
                let pLen = max 0 prefixLen
                if pLen = 0 then
                    yield! generateUnbounded mainMethod minVal
                else
                    let prefixSamples = generateUnbounded prefixMethod minVal |> Seq.take pLen |> Seq.toList
                    yield! prefixSamples
                    let nextStart = 
                        if List.isEmpty prefixSamples then minVal 
                        else List.last prefixSamples + 1
                    yield! generateUnbounded mainMethod nextStart
        }

    /// Generates sequence values given a sampling method, start value, and count limit.
    let generate (method: intSampleMethod) (minVal: int) (count: int) : int seq =
        if count <= 0 then Seq.empty
        else generateUnbounded method minVal |> Seq.take count


module SamplingConfig =

    let toString (config: samplingConfig) : string =
        sprintf "Name: %s, Min: %d, MaxCount: %s, Scale: %f, Method: %s" 
            config.Name 
            config.Min 
            (match config.MaxCount with | Some c -> string c | None -> "None") 
            config.Scale 
            (IntSampleMethod.toString config.Method)

    let fromString (s: string) : samplingConfig =
        let parts = s.Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length <> 5 then
            invalidArg "s" "Invalid samplingConfig string format. Expected 5 comma-separated values."
        let name = parts.[0].Trim().Substring(6).Trim()
        let min = int (parts.[1].Trim().Substring(5).Trim())
        let maxCountStr = parts.[2].Trim().Substring(9).Trim()
        let maxCount = if maxCountStr = "None" then None else Some (int maxCountStr)
        let scale = float (parts.[3].Trim().Substring(7).Trim())
        let methodStr = parts.[4].Trim().Substring(7).Trim()
        let method = IntSampleMethod.fromString methodStr
        { Name = name; Min = min; MaxCount = maxCount; Scale = scale; Method = method }


    /// Evaluates the method to yield a scaled set of integer samples constrained by maxBound.
    let getSampleSetMaxBound (config: samplingConfig) (maxBound: int) : Set<int> =
        let rawSeq = IntSampleMethod.generateUnbounded config.Method config.Min
        
        let boundedSeq = 
            match config.MaxCount with
            | Some maxCount -> rawSeq |> Seq.take maxCount
            | None -> rawSeq

        boundedSeq
        |> Seq.map (fun x -> int (ceil (float x * config.Scale)))
        |> Seq.takeWhile (fun scaledVal -> scaledVal <= maxBound)
        |> Set.ofSeq


    /// Evaluates the method to yield a scaled set of the first sampleCount integer samples strictly larger than minBound.
    let getSamplesWithMinBound (config: samplingConfig) (minBound: int) : seq<int> =
        let rawSeq = IntSampleMethod.generateUnbounded config.Method config.Min
            
        let boundedSeq = 
            match config.MaxCount with
            | Some maxCount -> rawSeq |> Seq.take maxCount
            | None -> rawSeq

        boundedSeq
        |> Seq.map (fun x -> int (ceil (float x * config.Scale)))
        |> Seq.filter (fun scaledVal -> scaledVal > minBound)
            

    /// Evaluates the method to yield a scaled set of the first sampleCount integer samples strictly larger than minBound.
    let getSampleSetWithMinBound (config: samplingConfig) (minBound: int) (sampleCount: int) : Set<int> =
            if sampleCount <= 0 then
                Set.empty
            else
                getSamplesWithMinBound config minBound |> Seq.take sampleCount |> Set.ofSeq




module SampleRegistry =

    let private createConfig name min maxCount scale method = {
        Name = name
        Min = min
        MaxCount = maxCount
        Scale = scale
        Method = method
    }

    let scale1 = 1.0
    let scale2 = 2.0
    let scale5 = 5.0
    let scale10 = 10.0
    let scale50 = 50.0
    let scale100 = 100.0
    let scale500 = 500.0
    let scale1000 = 1000.0
    let noMaxCount = None
    let maxCount0 = Some 0
    let maxCount5 = Some 5
    let maxCount10 = Some 10
    let maxCount20 = Some 20
    let maxCount50 = Some 50
    let maxCount100 = Some 100
    let maxCount500 = Some 500

    let samplingConfigsDict: Dictionary<string, samplingConfig> = 

        let dict = Dictionary<string, samplingConfig>()
        let add cfg = dict.Add(cfg.Name, cfg)

        //uniformInterval2             : [2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40]
        //uniformInterval10            : [10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200]
        //uniformInterval100           : [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000]
        //uniformInterval500           : [500, 1000, 1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000, 5500, 6000, 6500, 7000, 7500, 8000, 8500, 9000, 9500, 10000]
        //uniformInterval1000          : [1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000, 13000, 14000, 15000, 16000, 17000, 18000, 19000, 20000]
        add (createConfig "uniformInterval2" 1 noMaxCount scale2 (Constant 1))
        add (createConfig "uniformInterval10" 1 noMaxCount scale10 (Constant 1))
        add (createConfig "uniformInterval100" 1 noMaxCount scale100 (Constant 1))
        add (createConfig "uniformInterval500" 1 noMaxCount scale500 (Constant 1))
        add (createConfig "uniformInterval1000" 1 noMaxCount scale1000 (Constant 1))

        //summaryInterval_C.2C         : [1, 2, 3, 4, 5, 6, 7, 9, 10, 11, 12, 14, 15, 17, 18, 20, 21, 23, 24, 26]
        //summaryInterval_C.5C         : [25, 26, 27, 28, 29, 30, 32, 33, 34, 36, 37, 39, 40, 42, 43, 45, 47, 49, 51, 53]
        //summaryInterval_C.K          : [25, 26, 27, 28, 29, 31, 32, 33, 35, 36, 38, 39, 41, 43, 45, 47, 49, 51, 53, 56]
        //summaryInterval_C.5K         : [25, 26, 27, 28, 29, 31, 32, 34, 35, 37, 39, 41, 43, 45, 48, 50, 53, 56, 59, 62]
        add (createConfig "summaryInterval_C.2C" 1 noMaxCount scale1 (Exponential (0.03, 1)))
        add (createConfig "summaryInterval_C.5C" 25 noMaxCount scale1 (Exponential (0.041, 1)))
        add (createConfig "summaryInterval_C.K" 25 noMaxCount scale1 (Exponential (0.05, 1)))
        add (createConfig "summaryInterval_C.5K" 25 noMaxCount scale1 (Exponential (0.07, 1)))

        //emptyInterval                : [<empty>]
        //uniformInterval5_L5          : [5, 10, 15, 20, 25]
        //uniformInterval5_L10         : [5, 10, 15, 20, 25, 30, 35, 40, 45, 50]
        //expInterval25_L5             : [25, 65, 125, 215, 350]
        //expInterval25_L20            : [25, 55, 94, 145, 211, 296, 408, 552, 741, 985]
        //expInterval50_L10            : [50, 125, 238, 406, 659]
        //expInterval100_L10           : [100, 250, 475, 812]
        add (createConfig "emptyInterval" 0 maxCount0 scale1 (Constant 1))
        add (createConfig "uniformInterval5_L5" 1 maxCount5 scale5 (Constant 1))
        add (createConfig "uniformInterval5_L10" 1 maxCount10 scale5 (Constant 1))
        add (createConfig "expInterval25_L5" 25 maxCount5 scale1 (Exponential (0.5, 40)))
        add (createConfig "expInterval25_L20" 25 maxCount20 scale1 (Exponential (0.3, 30)))
        add (createConfig "expInterval50_L10" 50 maxCount10 scale1 (Exponential (0.5, 75)))
        add (createConfig "expInterval100_L10" 100 maxCount10 scale1 (Exponential (0.5, 150)))

        //periodic_LinearBurst         : [1, 3, 5, 7, 9, 10, 12, 14, 16, 18, 19, 21, 23, 25, 27, 28, 30, 32, 34, 36]
        //periodic_DecayReset          : [5, 10, 15, 25, 30, 40, 45, 50, 55, 65, 70, 80, 85, 90, 95, 105, 110, 120, 125, 130]
        //prefixed_PeriodicBurst       : [1, 11, 21, 31, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56]
        add (createConfig "periodic_LinearBurst" 1 maxCount50 scale1 (Cyclic (Constant 2, 5)))
        add (createConfig "periodic_DecayReset" 1 maxCount50 scale5 (Cyclic (Exponential (0.2, 1), 6)))
        add (createConfig "prefixed_PeriodicBurst" 1 maxCount50 scale1 (Prefixed (5, Constant 10, Cyclic (Constant 1, 5))))

        dict



    /// Iterates through the sample registry and prints the first `sampleCount` members generated.
    let printFirstMembers (sampleCount: int) (maxBound: int) =
        printfn "=== Printing First %d Members of Generated Samples ===" sampleCount
        for KeyValue(name, config) in samplingConfigsDict do
            let samples = 
                SamplingConfig.getSampleSetMaxBound config maxBound
                |> Set.toSeq
                |> Seq.truncate sampleCount
                |> Seq.toList
            
            let sampleStr = 
                if List.isEmpty samples then "<empty>"
                else samples |> List.map string |> String.concat ", "

            printfn "%-28s : [%s]" name sampleStr