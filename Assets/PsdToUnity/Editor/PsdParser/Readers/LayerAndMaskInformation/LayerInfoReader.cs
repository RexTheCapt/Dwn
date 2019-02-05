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
using System.Linq;
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation
{
    internal class LayerInfoReader : ValueReader<PsdLayer[]>
    {
        public LayerInfoReader(PsdReader reader, PsdDocument document)
            : base(reader, true, document)
        {
        }

        protected override void ReadValue(PsdReader reader, object userData, out PsdLayer[] value)
        {
            var document = userData as PsdDocument;
            var layerCount = Math.Abs((int) reader.ReadInt16());

            var layers = new PsdLayer[layerCount];
            for (var i = 0; i < layerCount; i++) layers[i] = new PsdLayer(reader, document);

            foreach (var item in layers) item.ReadChannels(reader);

            layers = Initialize(null, layers);

            foreach (var item in layers.SelectMany(item => item.Descendants()).Reverse()) item.ComputeBounds();

            value = layers;
        }

        public static PsdLayer[] Initialize(PsdLayer parent, PsdLayer[] layers)
        {
            var stack = new Stack<PsdLayer>();
            var rootLayers = new List<PsdLayer>();
            var layerToChilds = new Dictionary<PsdLayer, List<PsdLayer>>();

            foreach (var item in layers.Reverse())
            {
                if (item.SectionType == SectionType.Divider)
                {
                    parent = stack.Pop();
                    continue;
                }

                if (parent != null)
                {
                    if (layerToChilds.ContainsKey(parent) == false) layerToChilds.Add(parent, new List<PsdLayer>());

                    var childs = layerToChilds[parent];
                    childs.Insert(0, item);
                    item.Parent = parent;
                }
                else
                {
                    rootLayers.Insert(0, item);
                }

                if (item.SectionType == SectionType.Opend || item.SectionType == SectionType.Closed)
                {
                    stack.Push(parent);
                    parent = item;
                }
            }

            foreach (var item in layerToChilds) item.Key.Childs = item.Value.ToArray();

            return rootLayers.ToArray();
        }
    }
}