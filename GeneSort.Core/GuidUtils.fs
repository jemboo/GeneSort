namespace GeneSort.Core

open System.IO
open System.Security.Cryptography

module GuidUtils = 

    // structural equality
    let hashObjs (oes: obj seq) =
        use stream = new MemoryStream()
        use writer = new BinaryWriter(stream)
        oes |> Seq.iter (fun o -> writer.Write(sprintf "%A" o))
        let md5 = MD5.Create()
        md5.ComputeHash(stream.ToArray())

    // structural equality
    let guidFromObjs (objs: seq<obj>) = 
        System.Guid(hashObjs objs)