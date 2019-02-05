#region usings

using UnityEngine;

#endregion

namespace Assets.Scripts
{
    public class ChunkPosition : MonoBehaviour
    {
        private float modifier = 16;

        public Vector2 Position;

        public Vector2 ActualPosition
        {
            get { return new Vector2(Position.x * modifier, Position.y * modifier); }
        }
    }
}