using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO;
using System.Web;
using System.Net.Http;

namespace MacroscopPlayer
{
    class Frames
    {
        static public void SaveFrames(string url, string savePath)
        {
            WebClient wc = new WebClient();            

            Thread t = new Thread(() =>
            {
                byte[] data = File.ReadAllBytes(savePath + "MacroscopStreamRoad");

                int framesCount = 0;
                byte prev = 0;
                byte current;
                List<byte> frame = new List<byte>();
                bool isFrame = false;

                using (MemoryStream mStream = new MemoryStream(data))
                {
                    using (BinaryReader reader = new BinaryReader(mStream))
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (i == 0)
                            {
                                prev = reader.ReadByte();
                                continue;
                            }

                            current = reader.ReadByte();

                            if (prev == 255 && current == 216) // FF D8 - begin of JPEG-file
                            {
                                isFrame = true;
                            }
                            if (isFrame)
                            {
                                frame.Add(prev);
                            }
                            if (!isFrame)
                            {
                                frame.Clear();
                            }

                            if (prev == 255 && current == 217) // FF D9 - end of JPEG-file
                            {
                                framesCount++;
                                frame.Add(current); // We forget about D9. Need to add to the end of the frame
                                File.WriteAllBytes(savePath + @"frames\frame" + framesCount + ".jpg", frame.ToArray());
                                isFrame = false;
                            }

                            prev = current;
                        }
                    }                    
                }
            });

            t.Start();
        }
    }
}