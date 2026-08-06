namespace GeneSort.Core

open System


type essData = 
    private {
        exp:float
        scale:float
        maxCount:int option
    } with 

    static member create (exp:float) (scale:float) (maxCt:int option) : essData =
        { exp = exp; scale = scale; maxCount = maxCt }

    static member Empty : essData = 
            { exp = 1.0; scale = 1.0; maxCount = Some 0 }

    member this.Exp with get() = this.exp
    member this.MaxCount with get() = this.maxCount
    member this.Scale with get() = this.scale

    member this.toString() : string =
        match this.maxCount with
        | Some mc -> sprintf "exp: %f, scale: %f, maxCount: %d" this.exp this.scale mc
        | None -> sprintf "exp: %f, scale: %f" this.exp this.scale

    static member fromString (s:string) : essData =
        let parts = s.Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length < 2 || parts.Length > 3 then
            invalidArg "s" "Input string must contain two or three parts separated by commas."
        else
            let expPart = parts.[0].Trim()
            let scalePart = parts.[1].Trim()

            let expValue = 
                if expPart.StartsWith("exp:") then
                    expPart.Substring(4).Trim() |> float
                else
                    invalidArg "s" "First part must start with 'exp:'."

            let scaleValue = 
                if scalePart.StartsWith("scale:") then
                    scalePart.Substring(6).Trim() |> float
                else
                    invalidArg "s" "Second part must start with 'scale:'."

            let maxCountValue =
                if parts.Length = 3 then
                    let maxCountPart = parts.[2].Trim()
                    if maxCountPart.StartsWith("maxCount:") then
                        Some (maxCountPart.Substring(9).Trim() |> int)
                    else
                        invalidArg "s" "Third part must start with 'maxCount:'."
                else
                    None

            { exp = expValue; scale = scaleValue; maxCount = maxCountValue }



module EssData = 

    let expSampler 
                (minInt:int) (maxInt:int) (increaseRatio:float) (maxCount:int option) : Set<int> =
        match maxCount with
        | Some mc when mc <= 0 -> 
            Set.empty
        | _ ->
            let rec computeTargets currentVal acc =
                let nextVal = currentVal * increaseRatio
                let nextInt = int (ceil nextVal)
                if nextInt >= maxInt then 
                    (maxInt :: acc) |> List.rev
                else 
                    computeTargets nextVal (nextInt :: acc)

            let rawList = computeTargets minInt [minInt]
        
            match maxCount with
            | Some mc when mc = 1 ->
                Set.singleton (List.last rawList)
            | Some mc when mc > 1 && rawList.Length > mc ->
                let step = float (rawList.Length - 1) / float (mc - 1)
                [ 0 .. mc - 1 ]
                |> List.map (fun i -> 
                    let idx = int (round (float i * step))
                    rawList.[idx])
                |> Set.ofList
            | _ -> 
                rawList |> Set.ofList


    let expSampleAndScale
                (minInt:int) (maxInt:int) 
                (increaseRatio:float) (scale:float) (maxCount:int option) : Set<int> =
        expSampler minInt maxInt increaseRatio maxCount
        |> Set.map (fun x -> int (ceil (float x * scale)))


    let create exp scale maxCount = essData.create exp scale maxCount

    let getSampleSet (ess:essData) min max : Set<int> = 
        expSampleAndScale min max ess.Exp ess.Scale ess.MaxCount

    let getSamplesInOrder (ess:essData) min max = 
        expSampler min max ess.Exp ess.MaxCount
        |> Set.toSeq |> Seq.sort

    let toString (ess:essData option) = 
        match ess with
        | Some v -> v.toString()
        | None -> "None"
    
    let fromString s = essData.fromString s

    let xSampleC = 1.582
    let xSample5C = 1.992
    let xSample1K = 2.153
    
    let cSampleC = 1.001
    let cSample5C = 1.041
    let cSample1K = 1.05
    let cSample5K = 1.07
    let cSample10K = 1.08
    let cSample50K = 1.1
    let cSample100K = 1.1113
    let cSample500K = 1.13

    let kSample5K = 1.004
    let kSample10K = 1.0049
    let ksample50K = 1.0068
    let ksample100K = 1.0077
    let ksample500K = 1.0095
    let ksample1M = 1.0103