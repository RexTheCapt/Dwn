#if !NO_UNITY

#region usings

using System;
using System.Collections.Generic;
using SubjectNerd.PsdImporter.FullSerializer;
using UnityEngine;

#endregion

namespace Assets.PsdToUnity.Editor.FullSerializer.Converters.Unity
{
    public abstract class AnimationCurveDirectConverter : fsDirectConverter<AnimationCurve>
    {
        protected override fsResult DoSerialize(AnimationCurve model, Dictionary<string, fsData> serialized)
        {
            var result = fsResult.Success;

            result += SerializeMember(serialized, null, "keys", model.keys);
            result += SerializeMember(serialized, null, "preWrapMode", model.preWrapMode);
            result += SerializeMember(serialized, null, "postWrapMode", model.postWrapMode);

            return result;
        }

        protected override fsResult DoDeserialize(Dictionary<string, fsData> data, ref AnimationCurve model)
        {
            var result = fsResult.Success;

            Keyframe[] t0;
            result += DeserializeMember(data, null, "keys", out t0);
            model.keys = t0;

            WrapMode t1;
            result += DeserializeMember(data, null, "preWrapMode", out t1);
            model.preWrapMode = t1;

            WrapMode t2;
            result += DeserializeMember(data, null, "postWrapMode", out t2);
            model.postWrapMode = t2;

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType)
        {
            return new AnimationCurve();
        }
    }
}
#endif