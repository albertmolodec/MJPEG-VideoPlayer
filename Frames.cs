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
            byte[] data = null;

            Thread t = new Thread(() =>
            {
                data = File.ReadAllBytes(savePath + "MacroscopStreamRoad");
                int beginCount = 0;
                int endCount = 0;
                List<byte> frame = new List<byte>();
                bool flag = false;

                for (int i = 1; i < data.Length; i++)
                {
                    if (data[i - 1] == 255 && data[i] == 216) // FF D8 - begin of JPEG-file
                    {
                        beginCount++;
                        flag = true;
                    }
                    if (flag)
                    {
                        frame.Add(data[i - 1]);
                    }
                    if (!flag)
                    {
                        frame.Clear();
                    }

                    if (data[i - 1] == 255 && data[i] == 217) // FF D9 - end of JPEG-file
                    {
                        endCount++;
                        frame.Add(data[i]); // We forget about D9. Need to add to the end of the frame
                        File.WriteAllBytes(savePath + @"frames\frame" + beginCount + ".jpg", frame.ToArray());
                        flag = false;
                    }
                }
            });

            t.Start();
        }
    }
}
