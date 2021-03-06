namespace MUDT.IO.InMemory

  open MUDT.Utilities.TypeUtility

  type MemoryBuffer(capacity:int) =

    let capacity = capacity

    let buffer = nullByteArray capacity

    let mutable head = ref 0
    let mutable tail = ref 0

    let mutable size = ref 0

    member x.ReadBytes(count:int) =
      [|
        for i in 0..count-1 -> 
          let b = buffer.[!head]
          decr size
          incr head
          if !head > capacity-1 then head := 0
          if !size < 0 then failwith "Took too much"
          b
      |]

    member x.WriteBytes(bytes:byte[]) =
     bytes |> Array.iter(fun b ->
       buffer.[!tail] <- b
       incr size
       incr tail
       if !tail > capacity-1 then tail := 0
       if !size > capacity then failwithf "Failed to append %d bytes. Exceeded capacity: %d" (Array.length bytes) !size
     )

    member x.AsyncWrite(bytes:byte[]) =
      async {
        x.WriteBytes(bytes)
      }