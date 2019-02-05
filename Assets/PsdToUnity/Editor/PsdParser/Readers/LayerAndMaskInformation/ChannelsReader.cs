﻿#region License

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

using System.IO;
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation
{
    internal class ChannelsReader : LazyValueReader<Channel[]>
    {
        public ChannelsReader(PsdReader reader, long length, PsdLayer layer)
            : base(reader, length, layer)
        {
        }

        protected override void ReadValue(PsdReader reader, object userData, out Channel[] value)
        {
            var layer = userData as PsdLayer;
            var records = layer.Records;

            using (var stream = new MemoryStream(reader.ReadBytes((int) Length)))
            using (var r = new PsdReader(stream, reader.Resolver, reader.Uri))
            {
                r.Version = reader.Version;
                ReadValue(r, layer.Depth, records.Channels);
            }

            value = records.Channels;
        }

        private void ReadValue(PsdReader reader, int depth, Channel[] channels)
        {
            foreach (var item in channels)
            {
                var compressionType = reader.ReadCompressionType();
                item.ReadHeader(reader, compressionType);
                item.Read(reader, depth, compressionType);
            }
        }
    }
}