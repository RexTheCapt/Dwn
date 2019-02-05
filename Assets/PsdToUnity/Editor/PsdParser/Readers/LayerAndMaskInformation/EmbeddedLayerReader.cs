#pragma warning disable 0219 // variable assigned but not used.

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
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation
{
    internal class EmbeddedLayerReader : ValueReader<EmbeddedLayer>
    {
        public EmbeddedLayerReader(PsdReader reader)
            : base(reader, true, null)
        {
        }

        protected override long OnLengthGet(PsdReader reader)
        {
            return (reader.ReadInt64() + 3) & ~3;
        }

        private Uri ReadAboluteUri(PsdReader reader)
        {
            IProperties props = new DescriptorStructure(reader);
            if (props.Contains("fullPath"))
            {
                var absoluteUri = new Uri(props["fullPath"] as string);
                if (File.Exists(absoluteUri.LocalPath))
                    return absoluteUri;
            }

            if (props.Contains("relPath"))
            {
                var relativePath = props["relPath"] as string;
                var absoluteUri = reader.Resolver.ResolveUri(reader.Uri, relativePath);
                if (File.Exists(absoluteUri.LocalPath))
                    return absoluteUri;
            }

            if (props.Contains("Nm"))
            {
                var name = props["Nm"] as string;
                var absoluteUri = reader.Resolver.ResolveUri(reader.Uri, name);
                if (File.Exists(absoluteUri.LocalPath))
                    return absoluteUri;
            }

            if (props.Contains("fullPath")) return new Uri(props["fullPath"] as string);

            return null;
        }

        protected override void ReadValue(PsdReader reader, object userData, out EmbeddedLayer value)
        {
            reader.ValidateSignature("liFE");

            var version = reader.ReadInt32();

            var id = new Guid(reader.ReadPascalString(1));
            var name = reader.ReadString();
            var type = reader.ReadType();
            var creator = reader.ReadType();

            var length = reader.ReadInt64();
            IProperties properties = reader.ReadBoolean() ? new DescriptorStructure(reader) : null;
            var absoluteUri = ReadAboluteUri(reader);

            value = new EmbeddedLayer(id, reader.Resolver, absoluteUri);
        }
    }
}