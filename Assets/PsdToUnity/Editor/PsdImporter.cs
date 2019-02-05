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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.PsdToUnity.Editor;
using Assets.PsdToUnity.Editor.PsdParser;
using SubjectNerd.PsdImporter.PsdParser;
using SubjectNerd.PsdImporter.Reconstructor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace SubjectNerd.PsdImporter
{
    public class PsdImporter
    {
        private const string DOC_ROOT = "DOCUMENT_ROOT";

        private static string GetPsdFilepath(Object psdFile)
        {
            var filepath = AssetDatabase.GetAssetPath(psdFile);
            if (string.IsNullOrEmpty(filepath))
                return string.Empty;
            if (filepath.ToLower().EndsWith(".psd") == false)
                return string.Empty;
            return filepath;
        }

        private static IEnumerator ParseLayers(IPsdLayer[] layers, bool doYield,
            Action<PsdLayer, int[]> onLayer, Action onComplete, int[] parentIndex = null)
        {
            // Loop through layers in reverse so they are encountered in same order as Photoshop
            for (var i = layers.Length - 1; i >= 0; i--)
            {
                var layerIndex = parentIndex;
                if (layerIndex == null)
                {
                    layerIndex = new[] {i};
                }
                else
                {
                    var lastIndex = layerIndex.Length;
                    Array.Resize(ref layerIndex, lastIndex + 1);
                    layerIndex[lastIndex] = i;
                }

                var layer = layers[i] as PsdLayer;
                if (layer == null)
                    continue;

                if (onLayer != null)
                    onLayer(layer, layerIndex);

                if (doYield)
                    yield return null;

                if (layer.Childs.Length > 0)
                    yield return EditorCoroutineRunner.StartCoroutine(
                        ParseLayers(layer.Childs, doYield, onLayer, null, layerIndex)
                    );
            }

            if (onComplete != null)
                onComplete();
        }

        public static void BuildImportLayerData(Object file, ImportUserData importSettings,
            Action<ImportLayerData, DisplayLayerData> callback)
        {
            var filepath = GetPsdFilepath(file);
            if (string.IsNullOrEmpty(filepath))
            {
                if (callback != null)
                    callback(null, null);
                return;
            }

            using (var psd = PsdDocument.Create(filepath))
            {
                var docImportData = new ImportLayerData
                {
                    name = DOC_ROOT,
                    indexId = new[] {-1},
                    Childs = new List<ImportLayerData>()
                };
                var docDisplayData = new DisplayLayerData
                {
                    IndexId = new[] {-1},
                    Childs = new List<DisplayLayerData>()
                };

                EditorCoroutineRunner.StartCoroutine(
                    ParseLayers(psd.Childs, false,
                        (layer, indexId) =>
                        {
                            // Walk down the index id to get the parent layers
                            // and build the full path
                            var fullPath = "";
                            var parentLayer = docImportData;
                            var parentDisplay = docDisplayData;
                            if (indexId.Length > 1)
                                for (var idIdx = 0; idIdx < indexId.Length - 1; idIdx++)
                                {
                                    var idx = indexId[idIdx];
                                    parentLayer = parentLayer.Childs[idx];
                                    parentDisplay = parentDisplay.Childs[idx];

                                    if (string.IsNullOrEmpty(fullPath) == false)
                                        fullPath += "/";
                                    fullPath += parentLayer.name;
                                }

                            if (string.IsNullOrEmpty(fullPath) == false)
                                fullPath += "/";
                            fullPath += layer.Name;

                            var layerImportData = new ImportLayerData
                            {
                                name = layer.Name,
                                path = fullPath,
                                indexId = indexId,
                                import = layer.IsVisible,
                                useDefaults = true,
                                Alignment = importSettings.DefaultAlignment,
                                Pivot = importSettings.DefaultPivot,
                                ScaleFactor = importSettings.ScaleFactor,
                                Childs = new List<ImportLayerData>()
                            };

                            var layerDisplayData = new DisplayLayerData
                            {
                                IndexId = indexId,
                                isVisible = layer.IsVisible,
                                isGroup = layer.Childs.Length > 0,
                                isOpen = layer.IsFolderOpen
                            };

                            var layerIdx = indexId[indexId.Length - 1];

                            var maxLayers = layerIdx + 1;
                            while (parentLayer.Childs.Count < maxLayers)
                                parentLayer.Childs.Add(null);

                            parentLayer.Childs[layerIdx] = layerImportData;

                            while (parentDisplay.Childs.Count < maxLayers)
                                parentDisplay.Childs.Add(null);

                            parentDisplay.Childs[layerIdx] = layerDisplayData;
                        },
                        () =>
                        {
                            if (callback != null)
                                callback(docImportData, docDisplayData);
                        })
                );
            }
        }

        private static PsdLayer GetPsdLayerByIndex(PsdDocument psdDoc, int[] layerIdx)
        {
            IPsdLayer target = psdDoc;
            foreach (var idx in layerIdx)
            {
                if (idx < 0 || idx >= target.Childs.Length)
                    return null;
                target = target.Childs[idx];
            }

            var layer = target as PsdLayer;
            return layer;
        }

        #region Layer Texture Generation

        public static Texture2D GetLayerTexture(Object psdFile, int[] layerIdx)
        {
            var setting = new ImportLayerData
            {
                Alignment = SpriteAlignment.Center,
                Pivot = new Vector2(0.5f, 0.5f),
                ScaleFactor = ScaleFactor.Full,
                Childs = new List<ImportLayerData>(),
                import = true,
                indexId = layerIdx
            };
            return GetLayerTexture(psdFile, setting);
        }

        public static Texture2D GetLayerTexture(Object psdFile, ImportLayerData setting)
        {
            var filepath = GetPsdFilepath(psdFile);
            if (string.IsNullOrEmpty(filepath))
                return null;

            Texture2D texture = null;
            using (var psd = PsdDocument.Create(filepath))
            {
                var layer = GetPsdLayerByIndex(psd, setting.indexId);
                texture = GetLayerTexture(psd, layer, setting);
            }

            return texture;
        }

        private static Texture2D GetLayerTexture(PsdDocument psdDoc, PsdLayer psdLayer, ImportLayerData setting)
        {
            if (psdLayer == null || psdLayer.IsGroup)
                return null;

            var layerTexture = GetTexture(psdLayer);
            if (setting.ScaleFactor != ScaleFactor.Full)
            {
                var mipMapLevel = setting.ScaleFactor == ScaleFactor.Half ? 1 : 2;
                layerTexture = ScaleTextureByMipmap(layerTexture, mipMapLevel);
            }

            return layerTexture;
        }

        private static Texture2D GetTexture(PsdLayer layer)
        {
            var texture = new Texture2D(layer.Width, layer.Height);
            var pixels = new Color32[layer.Width * layer.Height];

            var red = (from l in layer.Channels where l.Type == ChannelType.Red select l).First();
            var green = (from l in layer.Channels where l.Type == ChannelType.Green select l).First();
            var blue = (from l in layer.Channels where l.Type == ChannelType.Blue select l).First();
            var alpha = (from l in layer.Channels where l.Type == ChannelType.Alpha select l).FirstOrDefault();
            var mask = (from l in layer.Channels where l.Type == ChannelType.Mask select l).FirstOrDefault();

            for (var i = 0; i < pixels.Length; i++)
            {
                var r = red.Data[i];
                var g = green.Data[i];
                var b = blue.Data[i];
                byte a = 255;

                if (alpha != null)
                    a = alpha.Data[i];
                if (mask != null)
                    a *= mask.Data[i];

                var mod = i % texture.width;
                var n = texture.width - mod - 1 + i - mod;
                pixels[pixels.Length - n - 1] = new Color32(r, g, b, a);
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        private static Texture2D ScaleTextureByMipmap(Texture2D tex, int mipLevel)
        {
            if (mipLevel < 0 || mipLevel > 2)
                return null;
            var width = Mathf.RoundToInt(tex.width / (mipLevel * 2));
            var height = Mathf.RoundToInt(tex.height / (mipLevel * 2));

            // Scaling down by abusing mip maps
            var resized = new Texture2D(width, height);
            resized.SetPixels32(tex.GetPixels32(mipLevel));
            resized.Apply();
            return resized;
        }

        #endregion

        #region Layer Asset Import

        public static void ImportLayersUI(Object psdFile, ImportUserData importSettings, List<int[]> layerIndices)
        {
            var total = layerIndices.Count;
            EditorCoroutineRunner.StartCoroutineWithUI(
                ImportCoroutine(psdFile, importSettings, layerIndices, true,
                    (current, layer) =>
                    {
                        var text = string.Format("[{0}/{1}] Layer: {2}", current, total, layer.name);
                        var percent = (float) current / total;
                        EditorCoroutineRunner.UpdateUI(text, percent);
                    }
                ), "Importing PSD Layers", true
            );
        }

        public static void ImportLayers(Object psdFile, ImportUserData importSettings, List<int[]> layerIndices,
            Action<List<Sprite>> callback = null)
        {
            EditorCoroutineRunner.StartCoroutine(
                ImportCoroutine(psdFile, importSettings, layerIndices, false, completeCallback: callback)
            );
        }

        private static IEnumerator ImportCoroutine(Object psdFile, ImportUserData importSettings,
            List<int[]> layerIndices, bool doYield = false,
            Action<int, ImportLayerData> layerCallback = null,
            Action<List<Sprite>> completeCallback = null)
        {
            var filepath = GetPsdFilepath(psdFile);
            if (string.IsNullOrEmpty(filepath))
            {
                if (completeCallback != null)
                    completeCallback(null);
                yield break;
            }

            // No target directory set, use PSD file directory
            if (string.IsNullOrEmpty(importSettings.TargetDirectory))
                importSettings.TargetDirectory = filepath.Substring(0, filepath.LastIndexOf("/"));

            // Get the texture importer for the PSD
            var psdUnitySettings = (TextureImporter) AssetImporter.GetAtPath(filepath);
            var psdUnityImport = new TextureImporterSettings();
            psdUnitySettings.ReadTextureSettings(psdUnityImport);

            var importCurrent = 0;

            var sprites = new List<Sprite>();
            using (var psd = PsdDocument.Create(filepath))
            {
                foreach (var layerIdx in layerIndices)
                {
                    var layerSettings = importSettings.GetLayerData(layerIdx);
                    if (layerSettings == null)
                        continue;

                    if (layerCallback != null)
                        layerCallback(importCurrent, layerSettings);

                    var sprite = ImportLayer(psd, importSettings, layerSettings, psdUnityImport);
                    sprites.Add(sprite);
                    importCurrent++;

                    if (doYield)
                        yield return null;
                }
            }

            if (completeCallback != null)
                completeCallback(sprites);
        }

        private static Sprite ImportLayer(PsdDocument psdDoc, ImportUserData importSettings,
            ImportLayerData layerSettings, TextureImporterSettings psdUnityImport)
        {
            if (layerSettings == null)
                return null;

            var psdLayer = GetPsdLayerByIndex(psdDoc, layerSettings.indexId);
            if (psdLayer.IsGroup)
                return null;

            // Generate the texture
            var layerTexture = GetLayerTexture(psdDoc, psdLayer, layerSettings);
            if (layerTexture == null)
                return null;

            // Save the texture as an asset
            var layerSprite = SaveAsset(psdLayer, psdUnityImport, layerTexture, importSettings, layerSettings);
            return layerSprite;
        }

        private static Sprite SaveAsset(PsdLayer psdLayer, TextureImporterSettings psdUnityImport,
            Texture2D texture, ImportUserData importSettings, ImportLayerData layerSettings)
        {
            // Generate the file path for this layer
            string fileDir;
            var filepath = GetFilePath(psdLayer, importSettings, out fileDir);

            // Create the folder if non existent
            if (AssetDatabase.IsValidFolder(fileDir) == false)
            {
                var subPaths = fileDir.Split('/');
                var parentFolder = subPaths[0];
                foreach (var folder in subPaths.Skip(1))
                {
                    var targetFolder = string.Format("{0}/{1}", parentFolder, folder);
                    if (AssetDatabase.IsValidFolder(targetFolder) == false)
                        AssetDatabase.CreateFolder(parentFolder, folder);
                    parentFolder = targetFolder;
                }
            }

            // Write out the texture contents into the file
            AssetDatabase.CreateAsset(texture, filepath);
            var buf = texture.EncodeToPNG();
            File.WriteAllBytes(filepath, buf);

            AssetDatabase.ImportAsset(filepath, ImportAssetOptions.ForceUpdate);
            var textureObj = AssetDatabase.LoadAssetAtPath<Texture2D>(filepath);

            // Get the texture importer for the asset
            var textureImporter = (TextureImporter) AssetImporter.GetAtPath(filepath);
            // Read out the texture import settings so settings can be changed
            var texSetting = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(texSetting);

            var finalPPU = psdUnityImport.spritePixelsPerUnit;
            switch (layerSettings.ScaleFactor)
            {
                case ScaleFactor.Half:
                    finalPPU /= 2;
                    break;
                case ScaleFactor.Quarter:
                    finalPPU /= 4;
                    break;
            }

            // Change settings
            texSetting.spriteAlignment = (int) layerSettings.Alignment;
            texSetting.spritePivot = layerSettings.Pivot;
            texSetting.spritePixelsPerUnit = finalPPU;
            texSetting.filterMode = psdUnityImport.filterMode;
            texSetting.wrapMode = psdUnityImport.wrapMode;
            texSetting.textureType = TextureImporterType.Sprite;
            texSetting.spriteMode = (int) SpriteImportMode.Single;
            texSetting.mipmapEnabled = false;
            texSetting.alphaIsTransparency = true;
            texSetting.npotScale = TextureImporterNPOTScale.None;
            // Set the rest of the texture settings
            textureImporter.spritePackingTag = importSettings.PackingTag;
            // Write in the texture import settings
            textureImporter.SetTextureSettings(texSetting);

            EditorUtility.SetDirty(textureObj);
            AssetDatabase.WriteImportSettingsIfDirty(filepath);
            AssetDatabase.ImportAsset(filepath, ImportAssetOptions.ForceUpdate);
            return (Sprite) AssetDatabase.LoadAssetAtPath(filepath, typeof(Sprite));
        }

        public static string GetFilePath(PsdLayer layer, ImportUserData importSettings, out string dir)
        {
            var filename = string.Format("{0}.png", layer.Name);

            var folder = "";

            if (importSettings.fileNaming != NamingConvention.LayerNameOnly)
            {
                var isDir = importSettings.fileNaming == NamingConvention.CreateGroupFolders;
                var docLayer = layer.Document as IPsdLayer;
                var parent = layer.Parent;
                while (parent != null && parent.Equals(docLayer) == false)
                {
                    if (isDir)
                    {
                        if (string.IsNullOrEmpty(folder))
                            folder = parent.Name;
                        else
                            folder = string.Format("{0}/{1}", parent.Name, folder);
                    }
                    else
                    {
                        filename = string.Format("{0}_{1}", parent.Name, filename);
                    }

                    parent = parent.Parent;
                    if (importSettings.groupMode == GroupMode.ParentOnly)
                        break;
                }
            }

            var finalDir = importSettings.TargetDirectory;
            if (string.IsNullOrEmpty(folder) == false)
                finalDir = string.Format("{0}/{1}", finalDir, folder);
            // Sanitize directory
            finalDir = SanitizeString(finalDir, Path.GetInvalidPathChars());

            // Sanitize filename
            filename = SanitizeString(filename, Path.GetInvalidFileNameChars());

            var filepath = string.Format("{0}/{1}", finalDir, filename);
            dir = finalDir;
            return filepath;
        }

        private static string SanitizeString(string text, char[] cleanChars)
        {
            text = string.Join("_", text.Split(cleanChars));
            text = new string(text.Select(c =>
            {
                if (char.IsWhiteSpace(c))
                    return '_';
                return c;
            }).ToArray());
            return text;
        }

        #endregion

        #region Reconstruction

        private static ReconstructData GetReconstructData(PsdDocument psdDoc, string psdPath, Vector2 documentPivot,
            ImportUserData importSettings, ImportLayerData reconstructRoot)
        {
            // Get the texture import setting of the PSD
            var psdUnitySettings = (TextureImporter) AssetImporter.GetAtPath(psdPath);
            var psdUnityImport = new TextureImporterSettings();
            psdUnitySettings.ReadTextureSettings(psdUnityImport);

            var docSize = new Vector2(psdDoc.Width, psdDoc.Height);
            var data = new ReconstructData(docSize, documentPivot, psdUnitySettings.spritePixelsPerUnit);

            reconstructRoot.Iterate(
                layer =>
                {
                    if (layer.import == false)
                        return;

                    var psdLayer = GetPsdLayerByIndex(psdDoc, layer.indexId);

                    var layerBounds = new Rect
                    {
                        xMin = psdLayer.Left,
                        xMax = psdLayer.Right,
                        yMin = psdDoc.Height - psdLayer.Bottom,
                        yMax = psdDoc.Height - psdLayer.Top
                    };
                    data.layerBoundsIndex.Add(layer.indexId, layerBounds);

                    string layerDir;
                    var layerPath = GetFilePath(psdLayer, importSettings, out layerDir);
                    var layerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(layerPath);

                    if (layerSprite == null)
                        layerSprite = ImportLayer(psdDoc, importSettings, layer, psdUnityImport);

                    var spriteAnchor = Vector2.zero;

                    if (layerSprite != null)
                    {
                        var layerImporter = (TextureImporter) AssetImporter.GetAtPath(layerPath);
                        var layerSettings = new TextureImporterSettings();
                        layerImporter.ReadTextureSettings(layerSettings);

                        if (layerSettings.spriteAlignment == (int) SpriteAlignment.Custom)
                            spriteAnchor = layerSettings.spritePivot;
                        else
                            spriteAnchor = AlignmentToPivot((SpriteAlignment) layerSettings.spriteAlignment);
                    }

                    data.AddSprite(layer.indexId, layerSprite, spriteAnchor);
                },
                checkGroup => checkGroup.import
            );

            return data;
        }

        public static void Reconstruct(Object psdFile, ImportUserData importSettings,
            ImportLayerData reconstructRoot, Vector2 documentPivot,
            IReconstructor reconstructor)
        {
            var psdPath = GetPsdFilepath(psdFile);
            if (string.IsNullOrEmpty(psdPath))
                return;

            using (var psdDoc = PsdDocument.Create(psdPath))
            {
                var data = GetReconstructData(psdDoc, psdPath,
                    documentPivot, importSettings,
                    reconstructRoot);

                var GO = reconstructor.Reconstruct(reconstructRoot, data, Selection.activeGameObject);
                if (GO != null)
                {
                    EditorGUIUtility.PingObject(GO);
                    Selection.activeGameObject = GO;
                }
            }
        }

        public static Vector2 AlignmentToPivot(SpriteAlignment spriteAlignment)
        {
            var pivot = Vector2.zero;
            switch (spriteAlignment)
            {
                case SpriteAlignment.TopLeft:
                case SpriteAlignment.TopCenter:
                case SpriteAlignment.TopRight:
                    pivot.y = 1f;
                    break;
                case SpriteAlignment.LeftCenter:
                case SpriteAlignment.Center:
                case SpriteAlignment.RightCenter:
                    pivot.y = 0.5f;
                    break;
                case SpriteAlignment.BottomLeft:
                case SpriteAlignment.BottomCenter:
                case SpriteAlignment.BottomRight:
                    pivot.y = 0f;
                    break;
            }

            switch (spriteAlignment)
            {
                case SpriteAlignment.TopLeft:
                case SpriteAlignment.LeftCenter:
                case SpriteAlignment.BottomLeft:
                    pivot.x = 0f;
                    break;
                case SpriteAlignment.TopCenter:
                case SpriteAlignment.Center:
                case SpriteAlignment.BottomCenter:
                    pivot.x = 0.5f;
                    break;
                case SpriteAlignment.TopRight:
                case SpriteAlignment.RightCenter:
                case SpriteAlignment.BottomRight:
                    pivot.x = 1f;
                    break;
            }

            return pivot;
        }

        #endregion
    }
}