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

#endregion

namespace Assets.PsdToUnity.Editor.PsdParser
{
    public class LayerRecords
    {
        private LayerBlendingRanges _blendingRanges;

        private string _name;
        private SectionType _sectionType;

        public int Left { get; set; }

        public int Top { get; set; }

        public int Right { get; set; }

        public int Bottom { get; set; }

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }

        public int ChannelCount
        {
            get
            {
                if (Channels == null)
                    return 0;
                return Channels.Length;
            }
            set
            {
                if (value > 0x38) throw new Exception(string.Format("Too many channels : {0}", value));

                Channels = new Channel[value];
                for (var i = 0; i < value; i++) Channels[i] = new Channel();
            }
        }

        public Channel[] Channels { get; private set; }

        public BlendMode BlendMode { get; set; }

        public byte Opacity { get; set; }

        public bool Clipping { get; set; }

        public LayerFlags Flags { get; set; }

        public int Filter { get; set; }

        public long ChannelSize
        {
            get { return Channels.Select(item => item.Size).Aggregate((v, n) => v + n); }
        }

        public SectionType SectionType
        {
            get { return _sectionType; }
        }

        public Guid PlacedId { get; private set; }

        public string Name
        {
            get { return _name; }
        }

        public LayerMask Mask { get; private set; }

        public object BlendingRanges
        {
            get { return _blendingRanges; }
        }

        public IProperties Resources { get; private set; }

        public void SetExtraRecords(LayerMask layerMask, LayerBlendingRanges blendingRanges, IProperties resources,
            string name)
        {
            Mask = layerMask;
            _blendingRanges = blendingRanges;
            Resources = resources;
            _name = name;

            Resources.TryGetValue(ref _name, "luni.Name");
            Resources.TryGetValue(ref _sectionType, "lsct.SectionType");

            if (Resources.Contains("SoLd.Idnt"))
                PlacedId = Resources.ToGuid("SoLd.Idnt");
            else if (Resources.Contains("SoLE.Idnt"))
                PlacedId = Resources.ToGuid("SoLE.Idnt");


            foreach (var item in Channels)
                switch (item.Type)
                {
                    case ChannelType.Mask:
                    {
                        if (Mask != null)
                        {
                            item.Width = Mask.Width;
                            item.Height = Mask.Height;
                        }
                    }
                        break;
                    case ChannelType.Alpha:
                    {
                        if (Resources.Contains("iOpa"))
                        {
                            var opa = Resources.ToByte("iOpa", "Opacity");
                            item.Opacity = opa / 255.0f;
                        }
                    }
                        break;
                }
        }

        public void ValidateSize()
        {
            var width = Right - Left;
            var height = Bottom - Top;

            if (width > 0x3000 || height > 0x3000)
                throw new NotSupportedException(string.Format("Invalidated size ({0}, {1})", width, height));
        }
    }
}