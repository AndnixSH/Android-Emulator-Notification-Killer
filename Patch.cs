using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveToGameWpf.Logic
{
    internal class Patch
    {
        private static bool DetectPatch(byte[] sequence, int position, byte[] FindHex)
        {
            if (position + FindHex.Length > sequence.Length) return false;
            for (int p = 0; p < FindHex.Length; p++)
            {
                if (FindHex[p] != sequence[position + p] && FindHex[p] != 0x3F) // Check for wildcard byte 0x3F ('?')
                    return false;
            }
            return true;
        }

        internal static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                if (hex.Substring(i, 2) == "??")
                    bytes[i / 2] = 0x3F; // Set wildcard byte 0x3F ('?')
                else
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        internal static bool PatchFile(string originalFile, string findHex, string patchHex)
        {
            bool b = false;

            if (File.Exists(originalFile))
            {
                byte[] ReplaceHex = StringToByteArray(patchHex);
                byte[] FindHex = StringToByteArray(findHex);
                // Read file bytes.
                byte[] fileContent = File.ReadAllBytes(originalFile);

                // Detect and patch the file.
                for (int p = 0; p < fileContent.Length; p++)
                {
                    if (!DetectPatch(fileContent, p, FindHex)) continue;

                    for (int w = 0; w < ReplaceHex.Length; w++)
                    {
                        b = true;
                        fileContent[p + w] = ReplaceHex[w];
                    }
                }

                File.WriteAllBytes(originalFile, fileContent);

            }

            return b;
        }

        internal static void PatchOffset(string originalFile, string offset, string patchHex)
        {
            byte[] ReplaceHex = StringToByteArray(patchHex);
            int off = Convert.ToInt32(offset, 16);
            // Read file bytes.
            byte[] fileContent = File.ReadAllBytes(originalFile);

            // Detect and patch the file.
            for (int p = 0; p < fileContent.Length; p++)
            {
                //if (p >= off) continue;

                for (int w = 0; w < ReplaceHex.Length; w++)
                {
                    fileContent[off + w] = ReplaceHex[w];
                }
            }

            File.WriteAllBytes(originalFile, fileContent);
        }

        internal static bool PatternExists(string originalFile, string findHex)
        {
            byte[] FindHex = StringToByteArray(findHex);
            // Read file bytes.
            byte[] fileContent = File.ReadAllBytes(originalFile);

            // Detect and patch the file.
            for (int p = 0; p < fileContent.Length; p++)
            {
                if (DetectPatch(fileContent, p, FindHex))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
