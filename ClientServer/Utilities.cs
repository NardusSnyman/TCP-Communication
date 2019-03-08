using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ClientServer
{
    public class Utilities
    {
        public static string separator1 = ".:.";
        public static Encoding baseEncoding = Encoding.Unicode;
        public static Encoding networkEncoding = Encoding.UTF8;
        public static void SendBytes(TcpClient c, BaseEncode BaseEncode)
        {
            byte[] bytes = new byte[1024];
            var utf = BaseEncode.GetnetworkEncoding();
            NetworkStream s = c.GetStream();
            c.SendTimeout = 1000;
            MemoryStream ms = new MemoryStream(utf.data);
            int length = 0;
            while ((length = ms.Read(bytes, 0, bytes.Length)) > 0)//is reading something
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                // Convert byte array to string message. 							
                new MemoryStream(incommingData).CopyTo(s);
                if (length != bytes.Length)
                    break;
                Console.WriteLine(ms.Position + " bytes");
            }
        }
        public static BaseEncode RecieveBytes(TcpClient c)
        {
            byte[] bytes = new byte[1024];
            // Retrieve the network stream.  
            NetworkStream s = c.GetStream();
            c.ReceiveTimeout = 1000;
            int length;
            MemoryStream ms = new MemoryStream();
            while ((length = s.Read(bytes, 0, bytes.Length)) > 0)//is reading something
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                // Convert byte array to string message. 							
                new MemoryStream(incommingData).CopyTo(ms);
                if (length != bytes.Length)
                    break;
                Console.WriteLine(ms.Position + " bytes");
            }
            var data1 = new NetworkEncoding(ms.ToArray());

            return data1.GetBaseEncode();
        }
        public static bool ByteArrayToFile(string fileName, BaseEncode uc, Action<int, int> act = null)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    MemoryStream ms = new MemoryStream(uc.data);
                    byte[] buffer = new byte[2048];
                    int bytesRead;
                    while ((bytesRead = ms.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, bytesRead);
                        if (act != null)
                            act((int)(ms.Position * 100 / ms.Length), (int)ms.Position);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
        public static BaseEncode FromFile(string fileName)
        {
            MemoryStream ms = new MemoryStream();
            using (Stream source = File.OpenRead(fileName))
            {
                byte[] buffer = new byte[2048];
                int bytesRead;
                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }
            }
            return new BaseEncode(ms.ToArray());
        }
        public static SendPacketsElement[] BufferSplit(byte[] buffer, int blockSize)
        {
            MemoryStream ms = new MemoryStream(buffer);
            List<SendPacketsElement> list = new List<SendPacketsElement>();
            var size = buffer.Length;
            var chunkCount = blockSize;
            var bufferArray = new byte[chunkCount][];
            for (var i = 0; i < chunkCount; i++)
            {
                bufferArray[i] = new byte[chunkCount];
                for (var j = 0; j < chunkCount && i * chunkCount + j < size; j++)
                {
                    bufferArray[i][j] = buffer[i * chunkCount + j];
                }
            }
            foreach(byte[] array in bufferArray)
            {
                SendPacketsElement spe = new SendPacketsElement(array);
                list.Add(spe);
            }
            return list.ToArray();
        }
        public static List<int> GetIndexes(BaseEncode data, string separator)
        {
            byte[] data2 = data.data;
            var foundIndexes = new List<int>();
            for (int i = Array.IndexOf(data2, baseEncoding.GetBytes(separator)[0]) ; i > -1; i = Array.IndexOf(data2, baseEncoding.GetBytes(separator)[0], i + 1))
            {
                foundIndexes.Add(i);
            }
            return foundIndexes;
        }
    }
}
