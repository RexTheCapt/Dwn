/*
MIT License

Copyright (c) 2017 Jeiel Aranal

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#region usings

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace SubjectNerd.PsdImporter
{
    public enum ScaleFactor
    {
        Full,
        Half,
        Quarter
    }

    public enum NamingConvention
    {
        LayerNameOnly,
        CreateGroupFolders,
        PrefixGroupNames
    }

    public enum GroupMode
    {
        ParentOnly,
        FullPath
    }

    public class ImportLayerData
    {
        public SpriteAlignment Alignment;

        public List<ImportLayerData> Childs;
        public bool import;
        public int[] indexId;
        public string name;
        public string path;
        public Vector2 Pivot;
        public ScaleFactor ScaleFactor;
        public bool useDefaults;

        public void Iterate(Action<ImportLayerData> layerCallback,
            Func<ImportLayerData, bool> canEnterGroup = null,
            Action<ImportLayerData> enterGroupCallback = null,
            Action<ImportLayerData> exitGroupCallback = null)
        {
            for (var i = Childs.Count - 1; i >= 0; i--)
            {
                var layer = Childs[i];
                if (layer == null)
                    continue;

                if (layerCallback != null)
                    layerCallback(layer);

                var isGroup = layer.Childs.Count > 0;

                if (isGroup)
                {
                    var enterGroup = true;
                    if (canEnterGroup != null)
                        enterGroup = canEnterGroup(layer);

                    if (enterGroup)
                    {
                        if (enterGroupCallback != null)
                            enterGroupCallback(layer);

                        layer.Iterate(layerCallback, canEnterGroup, enterGroupCallback, exitGroupCallback);

                        if (exitGroupCallback != null)
                            exitGroupCallback(layer);
                    }
                }
            }
        }
    }

    public class ImportUserData
    {
        public bool AutoImport;
        public SpriteAlignment DefaultAlignment = SpriteAlignment.Center;
        public Vector2 DefaultPivot = new Vector2(0.5f, 0.5f);
        public SpriteAlignment DocAlignment = SpriteAlignment.Center;
        public Vector2 DocPivot = new Vector2(0.5f, 0.5f);

        public ImportLayerData DocRoot;
        public NamingConvention fileNaming;
        public GroupMode groupMode;
        public string PackingTag;
        public ScaleFactor ScaleFactor = ScaleFactor.Full;
        public string TargetDirectory;

        public ImportLayerData GetLayerData(int[] layerIdx)
        {
            if (DocRoot == null)
                return null;

            var currentLayer = DocRoot;
            foreach (var idx in layerIdx)
            {
                if (idx < 0 || idx >= currentLayer.Childs.Count)
                    return null;
                currentLayer = currentLayer.Childs[idx];
            }

            return currentLayer;
        }
    }
}