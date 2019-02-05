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

using System.Collections.Generic;
using SubjectNerd.PsdImporter.PsdParser.Readers.LayerAndMaskInformation;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser
{
    internal class LayerAndMaskInformationSection
    {
        private readonly IProperties documentResources;
        private readonly GlobalLayerMaskInfoReader globalLayerMask;
        private readonly LayerInfoReader layerInfo;

        private ILinkedLayer[] linkedLayers;

        public LayerAndMaskInformationSection(LayerInfoReader layerInfo, GlobalLayerMaskInfoReader globalLayerMask,
            IProperties documentResources)
        {
            this.layerInfo = layerInfo;
            this.globalLayerMask = globalLayerMask;
            this.documentResources = documentResources;
        }

        public IPsdLayer[] Layers
        {
            get { return layerInfo.Value; }
        }

        public ILinkedLayer[] LinkedLayers
        {
            get
            {
                if (linkedLayers == null)
                {
                    var list = new List<ILinkedLayer>();
                    string[] ids = {"lnk2", "lnk3", "lnkD", "lnkE"};

                    foreach (var item in ids)
                        if (documentResources.Contains(item))
                        {
                            var items = documentResources.ToValue<ILinkedLayer[]>(item, "Items");
                            list.AddRange(items);
                        }

                    linkedLayers = list.ToArray();
                }

                return linkedLayers;
            }
        }

        public IProperties Resources
        {
            get { return documentResources; }
        }
    }
}