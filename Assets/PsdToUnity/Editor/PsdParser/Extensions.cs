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
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser
{
    public static class Extensions
    {
        public static byte[] MergeChannels(this IImageSource imageSource)
        {
            var channels = imageSource.Channels;

            var length = channels.Length;
            var num2 = channels[0].Data.Length;

            var buffer = new byte[imageSource.Width * imageSource.Height * length];
            var num3 = 0;
            for (var i = 0; i < num2; i++)
            for (var j = channels.Length - 1; j >= 0; j--)
                buffer[num3++] = channels[j].Data[i];
            return buffer;
        }

        public static IEnumerable<IPsdLayer> Descendants(this IPsdLayer layer)
        {
            return Descendants(layer, item => true);
        }

        public static IEnumerable<IPsdLayer> Descendants(this IPsdLayer layer, Func<IPsdLayer, bool> filter)
        {
            foreach (var item in layer.Childs)
            {
                if (filter(item) == false)
                    continue;

                yield return item;

                foreach (var child in item.Descendants(filter)) yield return child;
            }
        }

        internal static IEnumerable<PsdLayer> Descendants(this PsdLayer layer)
        {
            yield return layer;

            foreach (var item in layer.Childs)
            foreach (var child in item.Descendants())
                yield return child;
        }
    }
}