module Program

open System
open FSharp.UMX

let startTime = System.DateTime.Now
printfn $"**** Q22Q888 ******** {startTime.ToString()}"



let endTime = System.DateTime.Now
let duration = endTime - startTime
System.Threading.Thread.Sleep(100)
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

Console.ReadLine() |> ignore
