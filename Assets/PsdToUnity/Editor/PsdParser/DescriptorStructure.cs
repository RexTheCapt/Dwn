#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // variable assigned but never used.

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

using Assets.PsdToUnity.Editor.PsdParser;
using SubjectNerd.PsdImporter.PsdParser.Structures;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser
{
    internal class DescriptorStructure : Properties
    {
        private readonly int version;

        public DescriptorStructure(PsdReader reader)
            : this(reader, true)
        {
        }

        public DescriptorStructure(PsdReader reader, bool hasVersion)
        {
            if (hasVersion) version = reader.ReadInt32();

            Add("Name", reader.ReadString());
            Add("ClassID", reader.ReadKey());

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadKey();
                var osType = reader.ReadType();
                if (key == "EngineData")
                {
                    Add(key.Trim(), new StructureEngineData(reader));
                }
                else
                {
                    var value = StructureReader.Read(osType, reader);
                    Add(key.Trim(), value);
                }
            }
        }
    }
}