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

using Assets.PsdToUnity.Editor.PsdParser;

namespace SubjectNerd.PsdImporter.PsdParser
{
    internal abstract class ValueReader<T>
    {
        private readonly long length;
        private readonly long position;
        private readonly PsdReader reader;
        private readonly int readerVersion;
        private readonly object userData;
        private bool isRead;
        private T value;

        protected ValueReader(PsdReader reader, bool hasLength, object userData)
        {
            if (hasLength) length = OnLengthGet(reader);

            this.reader = reader;
            readerVersion = reader.Version;
            position = reader.Position;
            this.userData = userData;

            if (hasLength == false)
            {
                Refresh();
                length = reader.Position - position;
            }

            this.reader.Position = position + length;
        }

        protected ValueReader(PsdReader reader, long length, object userData)
        {
            if (length < 0)
                throw new InvalidFormatException();
            this.reader = reader;
            this.length = length;
            readerVersion = reader.Version;
            position = reader.Position;
            this.userData = userData;

            if (this.length == 0)
            {
                Refresh();
                this.length = reader.Position - position;
            }

            this.reader.Position = position + this.length;
        }

        public T Value
        {
            get
            {
                if (isRead == false && length > 0)
                {
                    var position = reader.Position;
                    var version = reader.Version;
                    Refresh();
                    reader.Position = position;
                    reader.Version = version;
                }

                return value;
            }
        }

        public long Length
        {
            get { return length; }
        }

        public long Position
        {
            get { return position; }
        }

        public long EndPosition
        {
            get { return position + length; }
        }

        public void Refresh()
        {
            reader.Position = position;
            reader.Version = readerVersion;
            ReadValue(reader, userData, out value);
            if (length > 0)
                reader.Position = position + length;
            isRead = true;
        }

        protected virtual long OnLengthGet(PsdReader reader)
        {
            return reader.ReadLength();
        }

        protected abstract void ReadValue(PsdReader reader, object userData, out T value);
    }
}