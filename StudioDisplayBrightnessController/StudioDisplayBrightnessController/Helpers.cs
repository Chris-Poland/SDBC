using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioDisplayBrightnessController
{
    class Helpers
    {
        public static byte[] hexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }


        public static string byteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0'));
            }
            return sb.ToString().ToUpper();
        }


        public static string byteToHexString(byte data)
        {
            StringBuilder sb = new StringBuilder(2);
            sb.Append(Convert.ToString(data, 16).PadLeft(2, '0'));
            return sb.ToString().ToUpper();
        }








        public static float GammaCorrection(float inputSignal, float gamma)
        {
            return (float) Math.Pow(inputSignal, 1 / gamma);
        }


    }
}
