namespace GeneSort.Core

open System

type essData = 
    private {
        exp: float
        scale: float
        min: int
        maxCount: int option
        maxCycles: int option
        name: string
    } with 

    static member create (exp:float) (scale:float) (min:int) (maxCt:int option) (maxCy:int option) (name:string) : essData =
        { exp = exp; scale = scale; min = min; maxCount = maxCt; maxCycles = maxCy; name = name }

    /// Returns an Empty instance with maxCount = Some 0 to naturally yield empty sets.
    static member Empty : essData = 
        { exp = 1.0; scale = 1.0; min = 0; maxCount = Some 0; maxCycles = Some 0; name = "Empty" }

    member this.Exp with get() = this.exp
    member this.Scale with get() = this.scale
    member this.Min with get() = this.min
    member this.MaxCount with get() = this.maxCount
    member this.MaxCycles with get() = this.maxCycles
    member this.Name with get() = this.name

    member this.toString() : string =
        let baseStr = sprintf "name: %s, exp: %f, scale: %f, min: %d" this.name this.exp this.scale this.min
        let mcStr = match this.maxCount with Some mc -> sprintf ", maxCount: %d" mc | None -> ""
        let cyStr = match this.maxCycles with Some cy -> sprintf ", maxCycles: %d" cy | None -> ""
        baseStr + mcStr + cyStr

    static member fromString (s:string) : essData =
        let parts = s.Split([|','|], StringSplitOptions.RemoveEmptyEntries)
        if parts.Length < 4 || parts.Length > 6 then
            invalidArg "s" "Input string must contain between 4 and 6 parts separated by commas."
        else
            let namePart = parts.[0].Trim()
            let expPart = parts.[1].Trim()
            let scalePart = parts.[2].Trim()
            let minPart = parts.[3].Trim()

            let nameValue =
                if namePart.StartsWith("name:") then namePart.Substring(5).Trim()
                else invalidArg "s" "First part must start with 'name:'."

            let expValue = 
                if expPart.StartsWith("exp:") then expPart.Substring(4).Trim() |> float
                else invalidArg "s" "Second part must start with 'exp:'."

            let scaleValue = 
                if scalePart.StartsWith("scale:") then scalePart.Substring(6).Trim() |> float
                else invalidArg "s" "Third part must start with 'scale:'."

            let minValue = 
                if minPart.StartsWith("min:") then minPart.Substring(4).Trim() |> int
                else invalidArg "s" "Fourth part must start with 'min:'."

            let mutable maxCountValue = None
            let mutable maxCyclesValue = None

            for i in 4 .. parts.Length - 1 do
                let p = parts.[i].Trim()
                if p.StartsWith("maxCount:") then
                    maxCountValue <- Some (p.Substring(9).Trim() |> int)
                elif p.StartsWith("maxCycles:") then
                    maxCyclesValue <- Some (p.Substring(10).Trim() |> int)

            { exp = expValue; scale = scaleValue; min = minValue; maxCount = maxCountValue; maxCycles = maxCyclesValue; name = nameValue }

module EssData = 

    let expSampler 
                (minInt:int) (maxInt:int) (increaseRatio:float) (maxCount:int option) (maxCycles:int option) : Set<int> =
        
        let totalLimit = 
            match maxCount, maxCycles with
            | Some mc, Some cy -> Some (mc * cy)
            | Some mc, None -> Some mc
            | None, Some cy -> Some cy
            | None, None -> None

        match totalLimit with
        | Some limit when limit <= 0 -> 
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
            
            match totalLimit with
            | Some limit -> 
                rawList 
                |> Set.ofList
                |> Set.toSeq
                |> Seq.sort
                |> Seq.truncate limit
                |> Set.ofSeq
            | None -> 
                rawList 
                |> Set.ofList

    let expSampleAndScale 
                (minInt:int) (maxInt:int) 
                (increaseRatio:float) (scale:float) (maxCount:int option) (maxCycles:int option) : Set<int> =
        let samples = expSampler minInt maxInt increaseRatio maxCount maxCycles
        samples |> Set.map (fun x -> int (ceil (float x * scale)))

    let empty = essData.Empty

    let create exp scale min maxCount maxCycles name = essData.create exp scale min maxCount maxCycles name

    let getSampleSet (ess:essData) (max:int) : Set<int> = 
        expSampleAndScale ess.Min max ess.Exp ess.Scale ess.MaxCount ess.MaxCycles

    let getSamplesInOrder (ess:essData) (max:int) : int seq = 
        let preScaled = (float max / ess.Scale) |> floor |> int
        expSampleAndScale ess.Min preScaled ess.Exp ess.Scale ess.MaxCount ess.MaxCycles
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