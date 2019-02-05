#region usings

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Assets.Scripts
{
    public class CharacterMove : MonoBehaviour
    {
        public KeyCode UpKeyCode;
        public KeyCode DownKeyCode;
        public KeyCode LeftKeyCode;
        public KeyCode RightKeyCode;

        public float Senstivity = 1;

        [UsedImplicitly]
        private void Update()
        {
            var moveVector3 = Vector3.zero;

            if (Input.GetKey(UpKeyCode))
                moveVector3.y += Senstivity * Time.deltaTime;

            if (Input.GetKey(DownKeyCode))
                moveVector3.y -= Senstivity * Time.deltaTime;

            if (Input.GetKey(LeftKeyCode))
                moveVector3.x -= Senstivity * Time.deltaTime;

            if (Input.GetKey(RightKeyCode))
                moveVector3.x += Senstivity * Time.deltaTime;

            transform.position += moveVector3;
        }
    }
}