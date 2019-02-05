#region License

//Ntreev Photoshop Document Parser for .Net
//
//Released under the MIT License.
//
//Copyright (c) 2015 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region usings

using System;
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser
{
    public class Channel : IChannel
    {
        private float opacity = 1.0f;
        private int[] rlePackLengths;

        public Channel(ChannelType type, int width, int height, long size)
        {
            Type = type;
            Width = width;
            Height = height;
            Size = size;
        }

        public Channel()
        {
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public float Opacity
        {
            get { return opacity; }
            set { opacity = value; }
        }

        public long Size { get; set; }

        public byte[] Data { get; private set; }

        public ChannelType Type { get; set; }

        public void ReadHeader(PsdReader reader, CompressionType compressionType)
        {
            if (compressionType != CompressionType.RLE)
                return;

            rlePackLengths = new int[Height];
            if (reader.Version == 1)
                for (var i = 0; i < Height; i++)
                    rlePackLengths[i] = reader.ReadInt16();
            else
                for (var i = 0; i < Height; i++)
                    rlePackLengths[i] = reader.ReadInt32();
        }

        public void Read(PsdReader reader, int bpp, CompressionType compressionType)
        {
            switch (compressionType)
            {
                case CompressionType.Raw:
                    ReadData(reader, bpp, compressionType, null);
                    return;

                case CompressionType.RLE:
                    ReadData(reader, bpp, compressionType, rlePackLengths);
                    return;
            }
        }

        private void ReadData(PsdReader reader, int bps, CompressionType compressionType, int[] rlePackLengths)
        {
            var length = PsdUtility.DepthToPitch(bps, Width);
            Data = new byte[length * Height];
            switch (compressionType)
            {
                case CompressionType.Raw:
                    reader.Read(Data, 0, Data.Length);
                    break;

                case CompressionType.RLE:
                    for (var i = 0; i < Height; i++)
                    {
                        var buffer = new byte[rlePackLengths[i]];
                        var dst = new byte[length];
                        reader.Read(buffer, 0, rlePackLengths[i]);
                        DecodeRLE(buffer, dst, rlePackLengths[i], length);
                        for (var j = 0; j < length; j++) Data[i * length + j] = (byte) (dst[j] * opacity);
                    }

                    break;
            }
        }

        private static void DecodeRLE(byte[] src, byte[] dst, int packedLength, int unpackedLength)
        {
            var index = 0;
            var num2 = 0;
            var num3 = 0;
            byte num4 = 0;
            var num5 = unpackedLength;
            var num6 = packedLength;
            while (num5 > 0 && num6 > 0)
            {
                num3 = src[index++];
                num6--;
                if (num3 != 0x80)
                {
                    if (num3 > 0x80) num3 -= 0x100;
                    if (num3 < 0)
                    {
                        num3 = 1 - num3;
                        if (num6 == 0) throw new Exception("Input buffer exhausted in replicate");
                        if (num3 > num5)
                            throw new Exception(
                                string.Format("Overrun in packbits replicate of {0} chars", num3 - num5));
                        num4 = src[index];
                        while (num3 > 0)
                        {
                            if (num5 == 0) break;
                            dst[num2++] = num4;
                            num5--;
                            num3--;
                        }

                        if (num5 > 0)
                        {
                            index++;
                            num6--;
                        }

                        continue;
                    }

                    num3++;
                    while (num3 > 0)
                    {
                        if (num6 == 0) throw new Exception("Input buffer exhausted in copy");
                        if (num5 == 0) throw new Exception("Output buffer exhausted in copy");
                        dst[num2++] = src[index++];
                        num5--;
                        num6--;
                        num3--;
                    }
                }
            }

            if (num5 > 0)
                for (num3 = 0; num3 < num6; num3++)
                    dst[num2++] = 0;
        }
    }
}