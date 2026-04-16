namespace GeneSort.Core
open System
open System.IO
open System.Text
open System.Collections.Generic

type dataTableRecord = 
    private 
        { name: string
          timeStamp: string
          mutable sourceRows: ResizeArray<string>
          mutable errorRows: ResizeArray<string>
          dataHeaders: string[]
          dataRows: ResizeArray<string> }
    
    member this.Name with get() = this.name
    member this.TimeStamp with get() = this.timeStamp
    member this.SourceRows 
        with get() = this.sourceRows
    member this.ErrorRows 
        with get() = this.errorRows
    member this.DataHeaders with get() = this.dataHeaders
    member this.DataRows with get() = this.dataRows

    /// Helper to sanitize row data to prevent column shifting
    static member private Sanitize (row: string[]) : string [] =
        row |> Array.map (fun s -> if s = null then "" else s.Replace("\t", " "))


