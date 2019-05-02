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
        public static Encoding baseEncoding = Encoding.Unicode;
        public static Encoding networkEncoding = Encoding.UTF8;
        public static string separator1 = ".:.";
        public static string separator2 = ":";

        public static void SendBytes(TcpClient c, BaseEncode BaseEncode, byte ender, Action complete = null)
        {
            //declare variables
            byte[] bytes = new byte[1024];
            var utf = BaseEncode.GetnetworkEncoding();
            
            NetworkStream s = c.GetStream();
            MemoryStream ms = new MemoryStream();
            ms.Write(utf.data, 0, utf.data.Length);
            ms.WriteByte(ender);

            foreach(byte b in ms.ToArray())
            {
                Console.WriteLine(new NetworkEncoding(b).GetBaseEncode().String());
                s.WriteByte(b);
            }
            
            
            if(complete != null)
            {
                complete();
            }
        }
        public static void RecieveBytes(TcpClient c, byte ender, Action<BaseEncode> complete)
        {
            // Retrieve the network stream.
            Console.WriteLine("1");
            NetworkStream s = c.GetStream();
            MemoryStream ms = new MemoryStream();
            while (true)
            {
                try
                {
                    if (s.DataAvailable)
                    {
                        byte b = Convert.ToByte(s.ReadByte());
                        if (b.Equals(ender))
                        {
                            break;
                        }
                        
                        ms.WriteByte(b);
                        Console.WriteLine(new NetworkEncoding(b).GetBaseEncode().String());
                    }
                }
                catch (Exception)
                {

                }
            }


            var data1 = new NetworkEncoding(ms.ToArray());
            
            complete(data1.GetBaseEncode());
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

        private static byte ConvertBoolArrayToByte(bool[] source)
        {
            byte result = 0;
            // This assumes the array never contains more than 8 elements!
            int index = 8 - source.Length;

            // Loop through the array
            foreach (bool b in source)
            {
                // if the element is 'true' set the bit at that position
                if (b)
                    result |= (byte)(1 << (7 - index));

                index++;
            }

            return result;
        }

        private static bool[] ConvertByteToBoolArray(byte b)
        {
            // prepare the return result
            bool[] result = new bool[8];

            // check each bit in the byte. if 1 set to true, if 0 set to false
            for (int i = 0; i < 8; i++)
                result[i] = (b & (1 << i)) == 0 ? false : true;

            // reverse the array
            Array.Reverse(result);

            return result;
        }
    }
}
