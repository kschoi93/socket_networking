using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

class MyTcpListener
{
  public static void Main()
  {
    TcpListener server=null;
    try
    {
      Int32 port = 13000;
      IPAddress localAddr = IPAddress.Parse("127.0.0.1");

      server = new TcpListener(localAddr, port);

      server.Start();

      Byte[] bytes = new Byte[256];
      String data = null;

      while(true)
      {
        Console.Write("Waiting for a connection... ");

        TcpClient client = server.AcceptTcpClient();
        Console.WriteLine("Connected!");

        data = null;

        NetworkStream stream = client.GetStream();

        bool stx = true;
        bool header = true;
  
        int startIndex = 0;
        int stxSize = 1;
        int versionSize = 4;
        int commandSize = 8;
        int payloadSize = 100;
        int etxSize = 1;
        
        int version;
        long command;
        string payload;

        int i;

        while((i = stream.Read(bytes, 0, bytes.Length))!=0)
        {
          // 1. check stx
          if(stx){
            if(bytes[0] != 0x02){
              break;
            }
            stx = false;
          }

          // 2. check header
          if(header){
            // ToInt32 이기 때문에 [1] 인덱스 부터 4Byte를 읽어 자동으로 int32로 변환
            // 1 = 0 + 1
            version = BitConverter.ToInt32(bytes, startIndex = stxSize); 
            Console.WriteLine("version : {0}", version);
            // ToInt64 이기 때문에 [5] 인덱스 부터 8Byte를 읽어 자동으로 int64(long)으로 변환
            // 5 = 1 + 4
            command = BitConverter.ToInt64(bytes, startIndex += versionSize); 
            Console.WriteLine("command : {0}", command);
            header = false;
          }

          // [13] 인덱스부터 100 byte에 대해 string으로 읽어온다
          payload = Encoding.ASCII.GetString(bytes, startIndex += commandSize, payloadSize);
          Console.WriteLine("Received: {0}", payload);

          // 이번 while loop에서 받은 데이터의 맨 끝의 값이 etx면 종료한다
          if(bytes[i-1] == 0x03){
            string str = "Add 1003 + 1 : " + (1003+1);
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(str);
            // 반환
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine("msg : {0}", str);
            Console.WriteLine("Connection Close !");
          }
          
        }

        // Shutdown and end connection
        client.Close();
      }
    }
    catch(SocketException e)
    {
      Console.WriteLine("SocketException: {0}", e);
    }
    finally
    {
       // Stop listening for new clients.
       server.Stop();
    }

    Console.WriteLine("\nHit enter to continue...");
    Console.Read();
  }
}