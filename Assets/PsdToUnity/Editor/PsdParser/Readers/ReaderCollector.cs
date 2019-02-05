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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Assets.PsdToUnity.Editor.PsdParser;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser.Readers
{
    internal static class ReaderCollector
    {
        private static readonly Dictionary<string, Type> readers;

        static ReaderCollector()
        {
            var assembly = typeof(ResourceReaderBase).Assembly;

            var query = from item in assembly.GetTypes()
                where typeof(ResourceReaderBase).IsAssignableFrom(item) &&
                      (item.Attributes & TypeAttributes.Abstract) != TypeAttributes.Abstract
                select item;

            readers = new Dictionary<string, Type>(query.Count());

            foreach (var item in query)
            {
                var attrs = item.GetCustomAttributes(typeof(ResourceIDAttribute), true);

                if (attrs.Length == 0)
                    continue;

                var attr = attrs.First() as ResourceIDAttribute;
                readers.Add(attr.ID, item);
            }
        }

        public static ResourceReaderBase CreateReader(string resourceID, PsdReader reader, long length)
        {
            var readerType = typeof(EmptyResourceReader);
            if (readers.ContainsKey(resourceID)) readerType = readers[resourceID];
            return TypeDescriptor.CreateInstance(null, readerType, new[] {typeof(PsdReader), typeof(long)},
                new object[] {reader, length}) as ResourceReaderBase;
        }

        public static string GetDisplayName(Type type)
        {
            var attrs = type.GetCustomAttributes(typeof(ResourceIDAttribute), true);

            var attr = attrs.First() as ResourceIDAttribute;
            return attr.DisplayName;
        }

        public static string GetDisplayName(string resourceID)
        {
            if (readers.ContainsKey(resourceID)) return GetDisplayName(readers[resourceID]);

            return resourceID;
        }
    }
}