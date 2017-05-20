namespace MCDTP.Net.Protocol

  open MCDTP.Logging
  open MCDTP.Utility

  type UdpPacket =
    {
      seqNum   : int64;
      dLen     : int16;
      rFlag    : byte;
      data     : byte[];
    }

    static member DefaultInstance() =
      {
        seqNum   = 0L;
        dLen     = 0s;
        rFlag    = Type.nullByte
        data     = [||]
      }

    static member DefaultSize
      with get() = 512

    static member PayloadSize
      with get() = 500

  [<RequireQualifiedAccess>]
  module Udp =

    open InternalLogging

    let packetCompare p1 p2 =
      let ret =
        if p1.seqNum < p2.seqNum then -1
        elif p1.seqNum = p2.seqNum then 0
        else 1
      logger.LogWith(LogLevel.Info,"Udp.packetCompare",(p1,p2,ret))
      ret

    module Parser =

      let tryParse (bytes:byte[]) =
        if Array.length bytes <> UdpPacket.DefaultSize then
          None
        else
          try
          let packetOption =
            {
              seqNum = Conversion.bytesToInt64 bytes.[0..7];
              dLen = Conversion.bytesToInt16 bytes.[8..9];
              rFlag = bytes.[10];
              data = bytes.[12..];
            }
            |> Some
          logger.LogWith(LogLevel.Info,"Udp.Parser.tryParse",(bytes,packetOption))
          packetOption
          with
          | ex ->
            logger.Log("[UDP] Parser threw exception", ex)
            None

    module Composer =

      let tryCompose packet =
        let insertAsBytes (x:System.Object) (offset:int) (bytes:byte[]) =
          x
          |> Conversion.getBytes
          |> Array.iteri(fun i b -> bytes.[i+offset] <- b)
          bytes

        try
        let bytes =
          UdpPacket.DefaultSize
          |> Type.nullByteArray
          |> insertAsBytes packet.seqNum 0
          |> insertAsBytes packet.dLen 8
          |> insertAsBytes packet.rFlag 10
          |> insertAsBytes packet.data 12
        logger.LogWith(LogLevel.Info,"Udp.Composer.tryCompose",(packet,bytes))
        Some bytes
        with
        | ex ->
          logger.Log("[UDP] Composer threw exception",ex)
          None