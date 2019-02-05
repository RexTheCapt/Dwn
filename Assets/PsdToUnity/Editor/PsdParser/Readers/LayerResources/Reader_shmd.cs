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

#pragma warning disable 0219 // variable assigned but not used.

#region usings

using System.Collections.Generic;
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerResources
{
    [ResourceID("shmd")]
    internal class Reader_shmd : ResourceReaderBase
    {
        public Reader_shmd(PsdReader reader, long length)
            : base(reader, length)
        {
        }

        protected override void ReadValue(PsdReader reader, object userData, out IProperties value)
        {
            var props = new Properties();

            var count = reader.ReadInt32();

            var dss = new List<DescriptorStructure>();

            for (var i = 0; i < count; i++)
            {
                var s = reader.ReadAscii(4);
                var k = reader.ReadAscii(4);
                var c = reader.ReadByte();
                var p = reader.ReadBytes(3);
                var l = reader.ReadInt32();
                var p2 = reader.Position;
                var ds = new DescriptorStructure(reader);
                dss.Add(ds);
                reader.Position = p2 + l;
            }

            props["Items"] = dss;

            value = props;
        }
    }
}