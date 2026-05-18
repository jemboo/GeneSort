namespace GeneSort.Core

open System
open System.IO
open System.Security.Cryptography

module GuidUtils = 

    // structural equality
    let hashObjs (oes: obj seq) : byte [] =
        use stream = new MemoryStream()
        use writer = new BinaryWriter(stream)
        oes |> Seq.iter (fun o -> writer.Write(sprintf "%A" o))
        let md5 = MD5.Create()
        md5.ComputeHash(stream.ToArray())

    // structural equality
    let guidFromObjs (objs: seq<obj>) : Guid = 
        System.Guid(hashObjs objs)

    let guidOptionFromString (gstr: string) : Guid option =
            match System.Guid.TryParse(gstr) with
            | (true, g) -> Some g
            | _ -> None


//namespace GeneSort.Core

//open System
//open System.IO
//open System.Security.Cryptography
//open System.Text

//module GuidUtils = 

//    // structural equality
//    let hashObjs (oes: obj seq) : byte [] =
//        use stream = new MemoryStream()
//        use writer = new BinaryWriter(stream)
        
//        oes |> Seq.iter (fun o -> 
//            // Crucial: Use a deliberate, standardized string representation or binary write
//            // If it's a known string or primitive, write it explicitly.
//            match o with
//            | :? string as s -> writer.Write(s)
//            | :? int as i -> writer.Write(i)
//            | :? Guid as g -> writer.Write(g.ToByteArray())
//            // Fall back to a standard string format rather than reflection formatting where possible
//            | _ -> writer.Write(o.ToString()) 
//        )
        
//        // Ensure everything is pushed to the stream
//        writer.Flush() 
//        stream.Position <- 0L
        
//        // Using 'use' ensures the OS crypto provider is cleanly disposed of
//        use md5 = MD5.Create()
//        md5.ComputeHash(stream) // Hashing the stream directly avoids allocating a giant Array copy

//    // structural equality
//    let guidFromObjs (objs: seq<obj>) : Guid = 
//        Guid(hashObjs objs)

//    let guidOptionFromString (gstr: string) : Guid option =
//        match Guid.TryParse(gstr) with
//        | true, g -> Some g
//        | _ -> None