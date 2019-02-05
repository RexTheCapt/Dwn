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
using System.Linq;
using SubjectNerd.PsdImporter.PsdParser;
using SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation;

#endregion

namespace Assets.PsdToUnity.Editor.PsdParser
{
    public class PsdLayer : IPsdLayer
    {
        private static readonly PsdLayer[] EmptyChilds = { };
        private readonly PsdDocument _document;
        private readonly LayerRecords _records;

        private ChannelsReader _channels;

        private PsdLayer[] _childs;

        private ILinkedLayer _linkedLayer;

        public PsdLayer(PsdReader reader, PsdDocument document)
        {
            _document = document;
            _records = LayerRecordsReader.Read(reader);
            _records = LayerExtraRecordsReader.Read(reader, _records);

            Left = _records.Left;
            Top = _records.Top;
            Right = _records.Right;
            Bottom = _records.Bottom;
        }

        public Channel[] Channels
        {
            get { return _channels.Value; }
        }

        public SectionType SectionType
        {
            get { return _records.SectionType; }
        }

        public bool IsVisible
        {
            get { return (_records.Flags & LayerFlags.Visible) != LayerFlags.Visible; }
        }

        public bool IsGroup
        {
            get { return _records.SectionType == SectionType.Closed || _records.SectionType == SectionType.Opend; }
        }

        public bool IsFolderClosed
        {
            get { return _records.SectionType == SectionType.Closed; }
        }

        public bool IsFolderOpen
        {
            get { return _records.SectionType == SectionType.Opend; }
        }

        public PsdLayer Parent { get; set; }

        public PsdLayer[] Childs
        {
            get
            {
                if (_childs == null)
                    return EmptyChilds;
                return _childs;
            }
            set { _childs = value; }
        }

        public LayerRecords Records
        {
            get { return _records; }
        }

        public string Name
        {
            get { return _records.Name; }
        }

        public float Opacity
        {
            get { return _records.Opacity / 255f; }
        }

        public int Left { get; private set; }

        public int Top { get; private set; }

        public int Right { get; private set; }

        public int Bottom { get; private set; }

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }

        public int Depth
        {
            get { return _document.FileHeaderSection.Depth; }
        }

        public bool IsClipping
        {
            get { return _records.Clipping; }
        }

        public BlendMode BlendMode
        {
            get { return _records.BlendMode; }
        }

        public IProperties Resources
        {
            get { return _records.Resources; }
        }

        public PsdDocument Document
        {
            get { return _document; }
        }

        public ILinkedLayer LinkedLayer
        {
            get
            {
                var placeId = _records.PlacedId;

                if (placeId == Guid.Empty)
                    return null;

                if (_linkedLayer == null)
                    _linkedLayer = _document.LinkedLayers.Where(i => i.ID == placeId && i.HasDocument).FirstOrDefault();
                return _linkedLayer;
            }
        }

        public bool HasImage
        {
            get
            {
                if (_records.SectionType != SectionType.Normal)
                    return false;
                if (Width == 0 || Height == 0)
                    return false;
                return true;
            }
        }

        public bool HasMask
        {
            get { return _records.Mask != null; }
        }

        public void ReadChannels(PsdReader reader)
        {
            _channels = new ChannelsReader(reader, _records.ChannelSize, this);
        }

        public void ComputeBounds()
        {
            var sectionType = _records.SectionType;
            if (sectionType != SectionType.Opend && sectionType != SectionType.Closed)
                return;

            var left = int.MaxValue;
            var top = int.MaxValue;
            var right = int.MinValue;
            var bottom = int.MinValue;

            var isSet = false;

            foreach (var item in this.Descendants())
            {
                if (item == this || item.HasImage == false)
                    continue;

                // 일반 레이어인데 비어 있을때
                if (item.Resources.Contains("PlLd.Transformation"))
                {
                    var transforms = (double[]) item.Resources["PlLd.Transformation"];
                    double[] xx = {transforms[0], transforms[2], transforms[4], transforms[6]};
                    double[] yy = {transforms[1], transforms[3], transforms[5], transforms[7]};

                    var l = (int) Math.Ceiling(xx.Min());
                    var r = (int) Math.Ceiling(xx.Max());
                    var t = (int) Math.Ceiling(yy.Min());
                    var b = (int) Math.Ceiling(yy.Max());
                    left = Math.Min(l, left);
                    top = Math.Min(t, top);
                    right = Math.Max(r, right);
                    bottom = Math.Max(b, bottom);
                }
                else
                {
                    left = Math.Min(item.Left, left);
                    top = Math.Min(item.Top, top);
                    right = Math.Max(item.Right, right);
                    bottom = Math.Max(item.Bottom, bottom);
                }

                isSet = true;
            }

            if (isSet == false)
                return;

            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        #region IPsdLayer

        IPsdLayer IPsdLayer.Parent
        {
            get
            {
                if (Parent == null)
                    return _document;
                return Parent;
            }
        }

        Channel[] IImageSource.Channels
        {
            get { return _channels.Value; }
        }

        IPsdLayer[] IPsdLayer.Childs
        {
            // ReSharper disable once CoVariantArrayConversion
            get { return Childs; }
        }

        #endregion
    }
}