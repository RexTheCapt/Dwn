#region usings

using Boo.Lang;
using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Assets.Scripts
{
    public class PlayerChunkTracker : MonoBehaviour
    {
        public List<GameObject> CurrentChunkGameObjects = new List<GameObject>();
        public GameObject[] GameObjects;

        [UsedImplicitly]
        void Update()
        {
            GameObjects = new GameObject[CurrentChunkGameObjects.Count];

            for (int i = 0; i < GameObjects.Length; i++)
            {
                GameObjects[i] = CurrentChunkGameObjects[i];
            }
        }

        [UsedImplicitly]
        // ReSharper disable once ParameterHidesMember
        void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (collider2D.gameObject.tag == "Chunk")
                CurrentChunkGameObjects.Add(collider2D.gameObject);
        }

        [UsedImplicitly]
        // ReSharper disable once ParameterHidesMember
        void OnTriggerExit2D(Collider2D collider2D)
        {
            if (CurrentChunkGameObjects.Contains(collider2D.gameObject))
                CurrentChunkGameObjects.Remove(collider2D.gameObject);
        }
    }
}