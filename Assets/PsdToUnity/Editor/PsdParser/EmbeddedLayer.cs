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

namespace SubjectNerd.PsdImporter.PsdParser
{
    internal class EmbeddedLayer : ILinkedLayer
    {
        private readonly Uri absoluteUri;
        private readonly int height;
        private readonly Guid id;
        private readonly PsdResolver resolver;
        private readonly int width;
        private PsdDocument document;

        public EmbeddedLayer(Guid id, PsdResolver resolver, Uri absoluteUri)
        {
            this.id = id;
            this.resolver = resolver;
            this.absoluteUri = absoluteUri;

            if (File.Exists(this.absoluteUri.LocalPath))
            {
                var header = FileHeaderSection.FromFile(this.absoluteUri.LocalPath);
                width = header.Width;
                height = header.Height;
            }
        }

        public PsdDocument Document
        {
            get
            {
                if (document == null) document = resolver.GetDocument(absoluteUri);
                return document;
            }
        }

        public Uri AbsoluteUri
        {
            get { return absoluteUri; }
        }

        public bool HasDocument
        {
            get { return File.Exists(absoluteUri.LocalPath); }
        }

        public Guid ID
        {
            get { return id; }
        }

        public string Name
        {
            get { return absoluteUri.LocalPath; }
        }

        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }
    }
}