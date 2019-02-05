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
using System.IO;
using System.Text;
using SubjectNerd.PsdImporter.PsdParser;

#endregion

namespace Assets.PsdToUnity.Editor.PsdParser
{
    public class PsdReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private readonly PsdResolver _resolver;
        private readonly Stream _stream;
        private readonly Uri _uri;

        private int _version = 1;

        public PsdReader(Stream stream, PsdResolver resolver, Uri uri)
        {
            _stream = stream;
            _reader = new InternalBinaryReader(stream);
            _resolver = resolver;
            _uri = uri;
        }

        public long Position
        {
            get { return _reader.BaseStream.Position; }
            set { _reader.BaseStream.Position = value; }
        }

        public long Length
        {
            get { return _reader.BaseStream.Length; }
        }

        public int Version
        {
            get { return _version; }
            set
            {
                if (value != 1 && value != 2)
                    throw new InvalidFormatException();

                _version = value;
            }
        }

        public PsdResolver Resolver
        {
            get { return _resolver; }
        }

        public Stream Stream
        {
            get { return _stream; }
        }

        public Uri Uri
        {
            get { return _uri; }
        }

        public void Dispose()
        {
            _reader.Close();
        }

        public string ReadType()
        {
            return ReadAscii(4);
        }

        public string ReadAscii(int length)
        {
            return Encoding.ASCII.GetString(_reader.ReadBytes(length));
        }

        public bool VerifySignature()
        {
            return VerifySignature(false);
        }

        public bool VerifySignature(bool check64Bit)
        {
            var signature = ReadType();

            if (signature == "8BIM")
                return true;

            if (check64Bit && signature == "8B64")
                return true;

            return false;
        }

        public void ValidateSignature(string signature)
        {
            var s = ReadType();
            if (s != signature)
                throw new InvalidFormatException();
        }

        public void ValidateSignature()
        {
            ValidateSignature(false);
        }

        public void ValidateSignature(bool check64Bit)
        {
            if (VerifySignature(check64Bit) == false)
                throw new InvalidFormatException();
        }

        public void ValidateDocumentSignature()
        {
            var signature = ReadType();

            if (signature != "8BPS")
                throw new InvalidFormatException();
        }

        private void ValidateValue<T>(T value, string name, Func<T> readFunc)
        {
            var v = readFunc();
            if (Equals(value, v) == false) throw new InvalidFormatException("{0}의 값이 {1}이 아닙니다.", name, value);
        }

        public void ValidateInt16(short value, string name)
        {
            ValidateValue(value, name, () => ReadInt16());
        }

        public void ValidateInt32(int value, string name)
        {
            ValidateValue(value, name, () => ReadInt32());
        }

        public void ValidateType(string value, string name)
        {
            ValidateValue(value, name, () => ReadType());
        }

        public string ReadPascalString(int modLength)
        {
            var count = _reader.ReadByte();
            var text = string.Empty;
            if (count == 0)
            {
                var baseStream = _reader.BaseStream;
                baseStream.Position += modLength - 1;
                return text;
            }

            var bytes = _reader.ReadBytes(count);
            text = Encoding.UTF8.GetString(bytes);
            for (var i = count + 1; i % modLength != 0; i++)
            {
                var stream2 = _reader.BaseStream;
                stream2.Position += 1L;
            }

            return text;
        }

        public string ReadString()
        {
            var length = ReadInt32();
            if (length == 0)
                return string.Empty;

            var bytes = ReadBytes(length * 2);
            for (var i = 0; i < length; i++)
            {
                var index = i * 2;
                var b = bytes[index];
                bytes[index] = bytes[index + 1];
                bytes[index + 1] = b;
            }

            if (bytes[bytes.Length - 1] == 0 && bytes[bytes.Length - 2] == 0) length--;

            return Encoding.Unicode.GetString(bytes, 0, length * 2);
        }

        public string ReadKey()
        {
            var length = ReadInt32();
            length = length > 0 ? length : 4;
            return ReadAscii(length);
        }

        public int Read(byte[] buffer, int index, int count)
        {
            return _reader.Read(buffer, index, count);
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public char ReadChar()
        {
            return (char) ReadByte();
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public bool ReadBoolean()
        {
            return ReverseValue(_reader.ReadBoolean());
        }

        public double ReadDouble()
        {
            return ReverseValue(_reader.ReadDouble());
        }

        public double[] ReadDoubles(int count)
        {
            var values = new double[count];
            for (var i = 0; i < count; i++) values[i] = ReadDouble();
            return values;
        }

        public short ReadInt16()
        {
            return ReverseValue(_reader.ReadInt16());
        }

        public int ReadInt32()
        {
            return ReverseValue(_reader.ReadInt32());
        }

        public long ReadInt64()
        {
            return ReverseValue(_reader.ReadInt64());
        }

        public ushort ReadUInt16()
        {
            return ReverseValue(_reader.ReadUInt16());
        }

        public uint ReadUInt32()
        {
            return ReverseValue(_reader.ReadUInt32());
        }

        public ulong ReadUInt64()
        {
            return ReverseValue(_reader.ReadUInt64());
        }

        public long ReadLength()
        {
            return _version == 1 ? ReadInt32() : ReadInt64();
        }

        public void Skip(int count)
        {
            ReadBytes(count);
        }

        public void Skip(char c)
        {
            var ch = ReadChar();
            if (ch != c)
                throw new NotSupportedException();
        }

        public void Skip(char c, int count)
        {
            for (var i = 0; i < count; i++) Skip(c);
        }

        public ColorMode ReadColorMode()
        {
            return (ColorMode) ReadInt16();
        }

        public BlendMode ReadBlendMode()
        {
            return PsdUtility.ToBlendMode(ReadAscii(4));
        }

        public LayerFlags ReadLayerFlags()
        {
            return (LayerFlags) ReadByte();
        }

        public ChannelType ReadChannelType()
        {
            return (ChannelType) ReadInt16();
        }

        public CompressionType ReadCompressionType()
        {
            return (CompressionType) ReadInt16();
        }

        public void ReadDocumentHeader()
        {
            ValidateDocumentSignature();
            Version = ReadInt16();
            Skip(6);
        }

        private bool ReverseValue(bool value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToBoolean(bytes, 0);
        }

        private double ReverseValue(double value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToDouble(bytes, 0);
        }

        private short ReverseValue(short value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt16(bytes, 0);
        }

        private int ReverseValue(int value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private long ReverseValue(long value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToInt64(bytes, 0);
        }

        private ushort ReverseValue(ushort value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt16(bytes, 0);
        }

        private uint ReverseValue(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt32(bytes, 0);
        }

        private ulong ReverseValue(ulong value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }

        private class InternalBinaryReader : BinaryReader
        {
            public InternalBinaryReader(Stream stream)
                : base(stream)
            {
            }
        }
    }
}