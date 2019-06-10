using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using static ClientServer.EncodingClasses;

namespace ClientServer
{
    public class SendRecieveUtil
    {
            public static void SendBytes(TcpClient c, BaseEncode BaseEncode, Action<int> packet, byte ender, int buffer_size)
            {

                //declare variables
                byte[] bytes = new byte[buffer_size];
                var utf = BaseEncode.GetnetworkEncoding();
                NetworkStream s = c.GetStream();

                MemoryStream ms = new MemoryStream();
                ms.Write(utf.data, 0, utf.data.Length);
                ms.WriteByte(ender);
                ms.Position = 0;

                int count = 0;
                int length;
                while ((length = ms.Read(bytes, 0, buffer_size)) > 0)
                {
                    count += length / buffer_size;
                    packet(count);
                    s.Write(bytes, 0, length);
                }

            }
            public static BaseEncode RecieveBytes(TcpClient c, Action<double, BaseEncode> packet, byte ender, ref MemoryStream overread, int buffer_size)
            {
                double count = 0;
                byte[] buffer = new byte[buffer_size];
                if (overread == null)
                    overread = new MemoryStream();
                overread.Position = 0;
                int index;
                if ((index = Array.IndexOf(overread.ToArray(), ender)) > -1)//overread contains ender
                {
                    count -= index / buffer_size;

                    Array.Copy(overread.ToArray(), buffer, index);//copy needed data to return

                    if (overread.Length - (index + 1) != 0)//ender is not the final character of the array
                    {
                        byte[] buff = new byte[overread.Length - (index + 1)];//size of buffer to be returned

                        Array.ConstrainedCopy(overread.ToArray(), (index + 1), buff, 0, buff.Length);//copy part to buffer
                        overread = new MemoryStream(buff); //equal to remaining piece
                    }
                    else//ender is final, so set blank
                    {
                        overread = new MemoryStream();//equal 0 characters
                    }

                    var ne = new NetworkEncoding(buffer).GetBaseEncode();
                    packet(count, ne);
                    return new NetworkEncoding(buffer).GetBaseEncode();
                }
                else//overread does not contain ender
                {

                    //read from stream
                    NetworkStream s = c.GetStream();
                    MemoryStream ms = new MemoryStream();

                    if (overread.Length > 0)//length of stored data > 0
                    {
                        ms.Write(overread.ToArray(), 0, overread.ToArray().Length);//write to main stream
                    }
                    while (true)
                    {
                        try
                        {

                            int length;
                            while ((length = s.Read(buffer, 0, buffer_size)) > 0)
                            {
                                count += length / buffer_size;

                                if ((index = Array.IndexOf(buffer, ender)) > -1)//main stream contains ender
                                {

                                    ms.Write(buffer, 0, index);//end found and writing first part to stream

                                    byte[] buff = new byte[length - (index + 1)];//buffer length accomodated for second part
                                    if (length - (index + 1) != 0)//length != 0
                                    {
                                        Array.ConstrainedCopy(overread.ToArray(), (index + 1), buff, 0, buff.Length);//get second part
                                        overread = new MemoryStream(buff);//write to overread
                                    }
                                    else
                                    {
                                        overread = new MemoryStream();
                                    }

                                    var enc = new NetworkEncoding(ms.ToArray()).GetBaseEncode();
                                    packet(count, enc);
                                    return enc;//return first part and other
                                }
                                else//no ender, still reading
                                {
                                    ms.Write(buffer, 0, length);//no end...keep writing
                                    var x = new NetworkEncoding(buffer).GetBaseEncode();
                                    packet(count, x);
                                }
                            }
                        }
                        catch (Exception e)
                        {

                        }
                    }


                }

            }
    }
}
