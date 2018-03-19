using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BIW.STM5000.Communication.Driver
{
   public class SocketTCP
    {
       static readonly object padlock = new object();
       byte[] data = new byte[1024];
       IPAddress _IP;
       IPEndPoint _ipEnd;
       Socket socket;
       int errorCount = 0;
       private static Object thisLock = new Object();

       public SocketTCP()
       { }
       public SocketTCP(string IP, int Port)
       {
           InitSocket(IP, Port);
       }

       public bool InitSocket(string IP, int Port)
       {
           try
           {
               _IP = IPAddress.Parse(IP);
               _ipEnd = new IPEndPoint(_IP, Port);
               socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
               socket.ReceiveTimeout = 800;
               socket.SendTimeout = 800;
               if (ConnectServer())
                   return true;
               else
                   return false;
           }
           catch
           {
               return false;
           }
       }

       public bool Connected
       {
           get
           {
               if (socket == null)
                   return false;
               return socket.Connected;
           }
       }
       public bool ConnectServer()
       {
           try
           {
               socket.Connect(_ipEnd);
               return true;
           }
           catch (Exception )
           {
               return false;
           }
       }

       public bool Close()
       {
           try
           {
               if(socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
               socket.Close();
               return true;
           }
           catch
           {
               return false;
           }
       }

       public int StatusCode
       { get; set; }
       public byte[] SendCommand(byte[] cmd)
       {
           lock (thisLock)
           {

               try
               {
                   if (socket != null)
                   {
                       if (socket.Connected)
                       {
                           socket.Send(cmd);
                           int count = socket.Receive(data);
                           if (count < 0)
                               count = 0;
                           errorCount = 0;
                           byte[] value = new byte[count];
                           Array.ConstrainedCopy(data, 0, value, 0, count);
                           return value;
                       }
                   }

                   return null;
               }

               catch (SocketException )
               {
                   System.Threading.Thread.Sleep(1000);
                   errorCount++;
                   if (errorCount > 3)
                   {
                       errorCount = 0;
                       throw new Exception("SocketError");
                   }
                   else
                   {
                       return SendCommand(cmd);
                   }
               }
               catch (Exception e)
               {
                   System.Threading.Thread.Sleep(1000);
                   throw e;
               }
           }
       }

    }
}
