#if !NO_UNITY

#region usings

using System;
using System.Collections.Generic;
using SubjectNerd.PsdImporter.FullSerializer.Internal.DirectConverters;
using UnityEngine;

#endregion

namespace SubjectNerd.PsdImporter.FullSerializer
{
    partial class FsConverterRegistrar
    {
        public static GradientDirectConverter RegisterGradientDirectConverter;
    }
}

namespace SubjectNerd.PsdImporter.FullSerializer.Internal.DirectConverters
{
    public class GradientDirectConverter : fsDirectConverter<Gradient>
    {
        protected override fsResult DoSerialize(Gradient model, Dictionary<string, fsData> serialized)
        {
            var result = fsResult.Success;

            result += SerializeMember(serialized, null, "alphaKeys", model.alphaKeys);
            result += SerializeMember(serialized, null, "colorKeys", model.colorKeys);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref Gradient model)
        {
            var result = fsResult.Success;

            GradientAlphaKey[] t0;
            result += DeserializeMember(data, null, "alphaKeys", out t0);
            model.alphaKeys = t0;

            GradientColorKey[] t1;
            result += DeserializeMember(data, null, "colorKeys", out t1);
            model.colorKeys = t1;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType)
        {
            return new Gradient();
        }
    }
}
#endif