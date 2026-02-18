
namespace GeneSort.Core

open FSharp.UMX

module UmxExt =

    let sumInt ( x : int<'u>) (y: int<'u>) = x + y
    let sumFloat ( x : float<'u>) (y: float<'u>) = x + y

    let multIntUomByInt (y:int) (x:int<'u>) = 
        let fv = x |> UMX.untag<'u>
        (fv * y) |> UMX.tag<'u>

    let multFloatUomByInt (y:int) (x:float<'u>)  = 
        let fv = x |> UMX.untag<'u>
        (fv * (float y)) |> UMX.tag<'u>

    let multIntUomByFloat (y:float) (x:int<'u>) = 
        let fv = x |> UMX.untag<'u>
        (fv * (int y)) |> UMX.tag<'u>

    let multFloatUomByFloat (y:float) (x:float<'u>) = 
        let fv = x |> UMX.untag<'u>
        (fv * y) |> UMX.tag<'u>


    let floatToRaw (v: float<'u>) : string = (UMX.untag v).ToString()
    let intToRaw (v: int<'u>) : string = (UMX.untag v).ToString()
    let stringToRaw (v: string<'u>) : string = %v
    let guidToRaw (v: Guid<'u>) : string = (UMX.untag v).ToString()


    /// Converts any UMX-tagged int option to a string
    let intToString (v: int<'u> option) : string =
        match v with
        | Some value -> (UMX.untag value).ToString()
        | None -> "None"

    /// Converts any UMX-tagged int option to a string
    let stringToString (v: string<'u> option) : string =
        match v with
        | Some value -> (UMX.untag value)
        | None -> "None"

    let floatToString (v: float<'u> option) : string =
        match v with
        | Some value -> (UMX.untag value).ToString()
        | None -> "None"

    let guidToString (v: Guid<'u> option) : string =
        match v with
        | Some value -> (UMX.untag value).ToString()
        | None -> "None"