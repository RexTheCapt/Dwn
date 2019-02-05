#region usings

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Assets.Scripts
{
    public class ChunkRemoval : MonoBehaviour
    {
        public GameObject ChunkControllerGameObject;
        public float ChunkAge;
        public bool PlayerIsClose;

        private float MaxChunkAge
        {
            get { return ChunkControllerGameObject.GetComponent<ChunkController>().ChunkMaxAge; }
        }

        [UsedImplicitly]
        void Update()
        {
            if (!PlayerIsClose)
                ChunkAge += Time.deltaTime;
            else
                ChunkAge = 0;

            if(ChunkAge > MaxChunkAge)
                Destroy(gameObject);
        }

        [UsedImplicitly]
        // ReSharper disable once ParameterHidesMember
        void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (collider2D.gameObject.tag == "Player")
                PlayerIsClose = true;
        }

        [UsedImplicitly]
        // ReSharper disable once ParameterHidesMember
        void OnTriggerExit2D(Collider2D collider2D)
        {
            if (collider2D.gameObject.tag == "Player")
                PlayerIsClose = false;
        }
    }
}