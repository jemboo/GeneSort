namespace GeneSort.Core

open System

type essData = 
    private {
        exp: float
        scale: float
        min: int
        maxCount: int option
    } with 

    static member create (exp:float) (scale:float) (min:int) (maxCt:int option) : essData =
        { exp = exp; scale = scale; min = min; maxCount = maxCt }

    /// Returns an Empty instance with maxCount = Some 0 to naturally yield empty sets.
    static member Empty : essData = 
        { exp = 1.0; scale = 1.0; min = 0; maxCount = Some 0 }

    member this.Exp with get() = this.exp
    member this.Scale with get() = this.scale
    member this.Min with get() = this.min
    member this.MaxCount with get() = this.maxCount

    member this.toString() : string =
        match this.maxCount with
        | Some mc -> sprintf "exp: %f, scale: %f, min: %d, maxCount: %d" this.exp this.scale this.min mc
        | None -> sprintf "exp: %f, scale: %f, min: %d" this.exp this.scale this.min

    static member fromString (s:string) : essData =
        let parts = s.Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length < 3 || parts.Length > 4 then
            invalidArg "s" "Input string must contain three or four parts separated by commas."
        else
            let expPart = parts.[0].Trim()
            let scalePart = parts.[1].Trim()
            let minPart = parts.[2].Trim()

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

            let minValue = 
                if minPart.StartsWith("min:") then
                    minPart.Substring(4).Trim() |> int
                else
                    invalidArg "s" "Third part must start with 'min:'."

            let maxCountValue =
                if parts.Length = 4 then
                    let maxCountPart = parts.[3].Trim()
                    if maxCountPart.StartsWith("maxCount:") then
                        Some (maxCountPart.Substring(9).Trim() |> int)
                    else
                        invalidArg "s" "Fourth part must start with 'maxCount:'."
                else
                    None

            { exp = expValue; scale = scaleValue; min = minValue; maxCount = maxCountValue }


module EssData = 

    let expSampler 
                (minInt:int) (maxInt:int) (increaseRatio:float) (maxCount:int option) : Set<int> =
        match maxCount with
        | Some mc when mc <= 0 -> 
            Set.empty
        | _ when minInt > maxInt || minInt <= 0 ->
            Set.empty
        | _ ->
            let rec computeTargets currentVal acc =
                let nextVal = currentVal * increaseRatio
                let nextInt = int (ceil nextVal)
                if nextInt >= maxInt then 
                    (maxInt :: acc) |> List.rev
                else 
                    computeTargets nextVal (nextInt :: acc)

            let rawList = computeTargets (float minInt) [minInt]
            
            match maxCount with
                    | Some mc -> 
                        rawList 
                        |> Set.ofList
                        |> Set.toSeq
                        |> Seq.sort
                        |> Seq.truncate mc
                        |> Set.ofSeq
                    | None -> 
                        rawList 
                        |> Set.ofList


    let expSampleAndScale 
                (minInt:int) (maxInt:int) 
                (increaseRatio:float) (scale:float) (maxCount:int option) : Set<int> =
        let samples = expSampler minInt maxInt increaseRatio maxCount
        samples |> Set.map (fun x -> int (ceil (float x * scale)))

    let empty = essData.Empty

    let create exp scale min maxCount = essData.create exp scale min maxCount

    let getSampleSet (ess:essData) (max:int) : Set<int> = 
        expSampleAndScale ess.Min max ess.Exp ess.Scale ess.MaxCount

    let getSamplesInOrder (ess:essData) (max:int) : int seq = 
        expSampleAndScale ess.Min max ess.Exp ess.Scale ess.MaxCount
        |> Set.toSeq 
        |> Seq.sort

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