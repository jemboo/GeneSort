namespace GeneSort.Core

open System
open System.IO
open System.Security.Cryptography

type IStableSerializable =
    abstract member WriteStableBytes : BinaryWriter -> unit

module GuidUtils = 

    let private MARKER_SERIALIZABLE = 0x01uy
    let private MARKER_STRING       = 0x02uy
    let private MARKER_INT          = 0x03uy
    let private MARKER_GUID         = 0x04uy
    let private MARKER_BOOL         = 0x05uy
    let private MARKER_INT64        = 0x06uy
    let private MARKER_NULL         = 0x07uy

    // ─────────────────────────────────────────────────────────────────
    // Fully Compliant F# ThreadStatic Storage Container
    // ─────────────────────────────────────────────────────────────────
    type private ThreadCache =
        // Adding 'private' directly after 'val' satisfies FS0881
        [<ThreadStatic; DefaultValue>] static val mutable private sha256 : SHA256
        [<ThreadStatic; DefaultValue>] static val mutable private stream : MemoryStream
        [<ThreadStatic; DefaultValue>] static val mutable private writer : BinaryWriter

        // A static property inside the class makes accessing them simple
        static member GetContext() =
            if box ThreadCache.sha256 = null then ThreadCache.sha256 <- SHA256.Create()
            if box ThreadCache.stream = null then ThreadCache.stream <- new MemoryStream(256) 
            if box ThreadCache.writer = null then ThreadCache.writer <- new BinaryWriter(ThreadCache.stream, System.Text.Encoding.UTF8, true)
            ThreadCache.sha256, ThreadCache.stream, ThreadCache.writer

    let private hashObjs (oes: obj seq) : byte [] =
        let sha256, stream, writer = ThreadCache.GetContext()
        
        // Reset the memory stream buffer for reuse
        stream.SetLength(0L)
        stream.Position <- 0L
        
        oes |> Seq.iter (fun o -> 
            match o with
            | :? IStableSerializable as serializable -> 
                writer.Write(MARKER_SERIALIZABLE)
                serializable.WriteStableBytes(writer)
            | :? string as s -> 
                writer.Write(MARKER_STRING)
                writer.Write(s) 
            | :? int as i -> 
                writer.Write(MARKER_INT)
                writer.Write(i)
            | :? Guid as g -> 
                writer.Write(MARKER_GUID)
                writer.Write(g.ToByteArray() : byte[]) 
            | :? bool as b ->
                writer.Write(MARKER_BOOL)
                writer.Write(b)
            | :? int64 as i64 ->
                writer.Write(MARKER_INT64)
                writer.Write(i64)
            | null ->
                writer.Write(MARKER_NULL) 
            | _ -> 
                failwithf "Type '%s' cannot be securely hashed." (o.GetType().FullName)
        )
        
        writer.Flush() 
        stream.Position <- 0L
        
        let fullHash = sha256.ComputeHash(stream)
        fullHash.[0..15]

    let guidFromObjs (objs: seq<obj>) : Guid = 
        Guid(hashObjs objs)