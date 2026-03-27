namespace GeneSort.SortingResults.Bins

open FSharp.UMX
open System.Collections.Generic



type sorterEvalLayerStats = {
    isSorted:           bool
    layerCount:         int
    totalSorterIds:     int
    ceCountMean:        float
    ceCountStdDev:      float
    ceCountMin:         int
    ceCountMax:         int
    stageLengthMean:    float
    stageLengthStdDev:  float
    stageLengthMin:     int
    stageLengthMax:     int
}


module SorterEvalLayerStats =

    let private mean (xs: float array) =
        if xs.Length = 0 then 0.0
        else Array.average xs

    let private stdDev (xs: float array) =
        if xs.Length < 2 then 0.0
        else
            let m = mean xs
            xs |> Array.averageBy (fun x -> (x - m) ** 2.0) |> sqrt

    let compute (isSorted: bool) (layers: KeyValuePair<sorterEvalKey, sorterEvalLayer> seq) : sorterEvalLayerStats =
            let matched =
                layers
                |> Seq.filter (fun kvp -> kvp.Key.IsSorted = isSorted)
                |> Seq.toArray
            let ceCounts     = matched |> Array.map (fun kvp -> kvp.Key.CeCount     |> UMX.untag |> float)
            let stageLengths = matched |> Array.map (fun kvp -> kvp.Key.StageLength |> UMX.untag |> float)
            let totalIds     = matched |> Array.sumBy (fun kvp -> kvp.Value.EvalCount)
            {
                isSorted          = isSorted
                layerCount        = matched.Length
                totalSorterIds    = totalIds
                ceCountMean       = mean ceCounts
                ceCountStdDev     = stdDev ceCounts
                ceCountMin        = if ceCounts.Length = 0 then 0 else ceCounts |> Array.map int |> Array.min
                ceCountMax        = if ceCounts.Length = 0 then 0 else ceCounts |> Array.map int |> Array.max
                stageLengthMean   = mean stageLengths
                stageLengthStdDev = stdDev stageLengths
                stageLengthMin    = if stageLengths.Length = 0 then 0 else stageLengths |> Array.map int |> Array.min
                stageLengthMax    = if stageLengths.Length = 0 then 0 else stageLengths |> Array.map int |> Array.max
            }

    let report (stats: sorterEvalLayerStats) =
        let yn = if stats.isSorted then "sorted" else "unsorted"
        [ $"[{yn}]"
          $"  layers:                {stats.layerCount}"
          $"  total ids:             {stats.totalSorterIds}"
          $"  ceCount     mean:      {stats.ceCountMean:F2}"
          $"  ceCount     stdDev:    {stats.ceCountStdDev:F2}"
          $"  ceCount     min/max:   {stats.ceCountMin} / {stats.ceCountMax}"
          $"  stageLength mean:      {stats.stageLengthMean:F2}"
          $"  stageLength stdDev:    {stats.stageLengthStdDev:F2}"
          $"  stageLength min/max:   {stats.stageLengthMin} / {stats.stageLengthMax}" ]
        //|> String.concat Environment.NewLine
        //              stageLength:  mean={stats.stageLengthMean:F2}  stdDev={stats.stageLengthStdDev:F2}  min={stats.stageLengthMin}  max={stats.stageLengthMax}"""