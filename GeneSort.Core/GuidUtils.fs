namespace GeneSort.Core

open System
open System.IO
open System.Security.Cryptography

type IStableSerializable =
    abstract member WriteStableBytes : BinaryWriter -> unit

module GuidUtils = 

    // Unique type markers to prevent delimiter/structural layout collisions
    let private MARKER_SERIALIZABLE = 0x01uy
    let private MARKER_STRING       = 0x02uy
    let private MARKER_INT          = 0x03uy
    let private MARKER_GUID         = 0x04uy
    let private MARKER_BOOL         = 0x05uy
    let private MARKER_INT64        = 0x06uy
    let private MARKER_NULL         = 0x07uy

    let private hashObjs (oes: obj seq) : byte [] =
        use stream = new MemoryStream()
        // Force the BinaryWriter type annotation immediately to help the compiler
        use writer = new BinaryWriter(stream)
        
        oes |> Seq.iter (fun o -> 
            match o with
            | :? IStableSerializable as serializable -> 
                writer.Write(MARKER_SERIALIZABLE)
                serializable.WriteStableBytes(writer)
            | :? string as s -> 
                writer.Write(MARKER_STRING)
                writer.Write(s) // BinaryWriter natively length-prefixes strings
            | :? int as i -> 
                writer.Write(MARKER_INT)
                writer.Write(i)
            | :? Guid as g -> 
                writer.Write(MARKER_GUID)
                writer.Write(g.ToByteArray() : byte[]) // Type-annotated to satisfy overloads
            | :? bool as b ->
                writer.Write(MARKER_BOOL)
                writer.Write(b)
            | :? int64 as i64 ->
                writer.Write(MARKER_INT64)
                writer.Write(i64)
            | null ->
                writer.Write(MARKER_NULL) 
            | _ -> 
                failwithf "Type '%s' cannot be securely hashed. Implement IStableSerializable or add it to GuidUtils." (o.GetType().FullName)
        )
        
        writer.Flush() 
        stream.Position <- 0L
        
        use md5 = MD5.Create()
        md5.ComputeHash(stream)



    let guidFromObjs (objs: seq<obj>) : Guid = 
        Guid(hashObjs objs)
