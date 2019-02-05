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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace SubjectNerd.PsdImporter.PsdParser
{
    internal class Properties : IProperties
    {
        private readonly Dictionary<string, object> props;

        public Properties()
        {
            props = new Dictionary<string, object>();
        }

        public Properties(int capacity)
        {
            props = new Dictionary<string, object>(capacity);
        }

        public bool Contains(string property)
        {
            var ss = property.Split(new[] {'.', '[', ']'}, StringSplitOptions.RemoveEmptyEntries);

            object value = this.props;

            foreach (var item in ss)
                if (value is ArrayList)
                {
                    var arrayList = value as ArrayList;
                    int index;
                    if (int.TryParse(item, out index) == false)
                        return false;
                    if (index >= arrayList.Count)
                        return false;
                    value = arrayList[index];
                }
                else if (value is IDictionary<string, object>)
                {
                    var props = value as IDictionary<string, object>;
                    if (props.ContainsKey(item) == false) return false;

                    value = props[item];
                }

            return true;
        }

        public int Count
        {
            get { return props.Count; }
        }

        public object this[string property]
        {
            get { return GetProperty(property); }
            set { props[property] = value; }
        }

        public void Add(string key, object value)
        {
            props.Add(key, value);
        }

        private object GetProperty(string property)
        {
            var ss = property.Split(new[] {'.', '[', ']'}, StringSplitOptions.RemoveEmptyEntries);

            object value = this.props;

            foreach (var item in ss)
                if (value is ArrayList)
                {
                    var arrayList = value as ArrayList;
                    value = arrayList[int.Parse(item)];
                }
                else if (value is IDictionary<string, object>)
                {
                    var props = value as IDictionary<string, object>;
                    value = props[item];
                }
                else if (value is IProperties)
                {
                    var props = value as IProperties;
                    value = props[item];
                }

            return value;
        }

        #region IProperties

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return props.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return props.GetEnumerator();
        }

        #endregion
    }
}