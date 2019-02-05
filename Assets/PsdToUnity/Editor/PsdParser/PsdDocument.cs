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
using System.Collections.Generic;
using System.IO;
using SubjectNerd.PsdImporter.PsdParser;
using SubjectNerd.PsdImporter.PsdParser.Readers;

#endregion

namespace Assets.PsdToUnity.Editor.PsdParser
{
    public class PsdDocument : IPsdLayer, IDisposable
    {
        private ColorModeDataSectionReader _colorModeDataSection;
        private FileHeaderSectionReader _fileHeaderSection;
        private ImageDataSectionReader _imageDataSection;
        private ImageResourcesSectionReader _imageResourcesSection;
        private LayerAndMaskInformationSectionReader _layerAndMaskSection;

        private PsdReader _reader;
        //private Uri baseUri;

        public FileHeaderSection FileHeaderSection
        {
            get { return _fileHeaderSection.Value; }
        }

        public byte[] ColorModeData
        {
            get { return _colorModeDataSection.Value; }
        }

        public IEnumerable<ILinkedLayer> LinkedLayers
        {
            get { return _layerAndMaskSection.Value.LinkedLayers; }
        }

        public IProperties ImageResources
        {
            get { return _imageResourcesSection; }
        }

        public void Dispose()
        {
            if (_reader == null)
                return;

            _reader.Dispose();
            _reader = null;
            OnDisposed(EventArgs.Empty);
        }

        public int Width
        {
            get { return _fileHeaderSection.Value.Width; }
        }

        public int Height
        {
            get { return _fileHeaderSection.Value.Height; }
        }

        public int Depth
        {
            get { return _fileHeaderSection.Value.Depth; }
        }

        public IPsdLayer[] Childs
        {
            get { return _layerAndMaskSection.Value.Layers; }
        }

        public IProperties Resources
        {
            get { return _layerAndMaskSection.Value.Resources; }
        }

        public bool HasImage
        {
            get
            {
                if (_imageResourcesSection.Contains("Version") == false)
                    return false;
                return _imageResourcesSection.ToBoolean("Version", "HasCompatibilityImage");
            }
        }

        public static PsdDocument Create(string filename)
        {
            return Create(filename, new PathResolver());
        }

        public static PsdDocument Create(string filename, PsdResolver resolver)
        {
            var document = new PsdDocument();
            var fileInfo = new FileInfo(filename);
            var stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // ReSharper disable once AssignNullToNotNullAttribute
            document.Read(stream, resolver, new Uri(fileInfo.DirectoryName));
            return document;
        }

        public static PsdDocument Create(Stream stream)
        {
            return Create(stream, null);
        }

        public static PsdDocument Create(Stream stream, PsdResolver resolver)
        {
            var document = new PsdDocument();
            document.Read(stream, resolver, new Uri(Directory.GetCurrentDirectory()));
            return document;
        }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null) Disposed(this, e);
        }

        internal void Read(Stream stream, PsdResolver resolver, Uri uri)
        {
            _reader = new PsdReader(stream, resolver, uri);
            _reader.ReadDocumentHeader();

            _fileHeaderSection = new FileHeaderSectionReader(_reader);
            _colorModeDataSection = new ColorModeDataSectionReader(_reader);
            _imageResourcesSection = new ImageResourcesSectionReader(_reader);
            _layerAndMaskSection = new LayerAndMaskInformationSectionReader(_reader, this);
            _imageDataSection = new ImageDataSectionReader(_reader, this);
        }

        #region IPsdLayer

        IPsdLayer IPsdLayer.Parent
        {
            get { return null; }
        }

        bool IPsdLayer.IsClipping
        {
            get { return false; }
        }

        PsdDocument IPsdLayer.Document
        {
            get { return this; }
        }

        ILinkedLayer IPsdLayer.LinkedLayer
        {
            get { return null; }
        }

        string IPsdLayer.Name
        {
            get { return "Document"; }
        }

        int IPsdLayer.Left
        {
            get { return 0; }
        }

        int IPsdLayer.Top
        {
            get { return 0; }
        }

        int IPsdLayer.Right
        {
            get { return Width; }
        }

        int IPsdLayer.Bottom
        {
            get { return Height; }
        }

        BlendMode IPsdLayer.BlendMode
        {
            get { return BlendMode.Normal; }
        }

        Channel[] IImageSource.Channels
        {
            get { return _imageDataSection.Value; }
        }

        float IImageSource.Opacity
        {
            get { return 1.0f; }
        }

        bool IImageSource.HasMask
        {
            get { return FileHeaderSection.NumberOfChannels > 4; }
        }

        #endregion
    }
}