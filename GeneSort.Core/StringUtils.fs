namespace GeneSort.Core

open System
open FSharp.UMX
open System.Globalization

module StringUtils = 

    let getTimestampString () =
        let now = DateTime.Now
        // MM-dd for month-day
        // HH:mm:ss.f for hours (24h):minutes:seconds.tenths
        now.ToString("MM-dd HH:mm:ss.fff")


module CompactStringParser =

    let getValue (key: string) (input: string) : string =
        let prefix = key + "="
        if input.Contains(prefix) then
            let startIdx = input.IndexOf(prefix) + prefix.Length
            let remainder = input.Substring(startIdx)
            // Trim off trailing commas, parentheses, or extra text
            remainder.Split([| ','; ')'; ' ' |], StringSplitOptions.RemoveEmptyEntries).[0].Trim()
        else
            failwithf "Key '%s' not found in compact string '%s'" key input

    let parseBool<[<Measure>] 'm> (key: string) (input: string) : bool<'m> =
        let raw = getValue key input
        match Boolean.TryParse(raw) with
        | true, v -> UMX.tag<'m> v
        | _ -> failwithf "Failed to parse bool for key '%s' in '%s'" key input

    let parseFloat<[<Measure>] 'm> (key: string) (input: string) : float<'m> =
        let raw = getValue key input
        match Double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture) with
        | true, v -> UMX.tag<'m> v
        | _ -> failwithf "Failed to parse float for key '%s' in '%s'" key input

