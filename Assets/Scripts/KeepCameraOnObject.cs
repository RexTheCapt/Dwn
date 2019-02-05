#region usings

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Assets.Scripts
{
    public class KeepCameraOnObject : MonoBehaviour
    {
        public Camera Camera;

        [UsedImplicitly]
        void Update()
        {
            Vector3 pos = transform.position;
            pos.z = -100;

            Camera.gameObject.transform.position = pos;
        }
    }
}