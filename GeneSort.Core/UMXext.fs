
namespace GeneSort.Core

open FSharp.UMX

module Uoms =
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


    let intString ( x : int<'u>) = sprintf "%d" x
    let floatString ( x : float<'u>) = sprintf "%f" x
    let guidString ( x : Guid<'u>) = sprintf "%O" x
           