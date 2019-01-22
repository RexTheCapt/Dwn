#region usings

using Boo.Lang;
using UnityEngine;

#endregion

public class PlayerChunkTracker : MonoBehaviour
{
    public List<GameObject> CurrentChunkGameObjects = new List<GameObject>();
    public GameObject[] GameObjects;

    void Update()
    {
        GameObjects = new GameObject[CurrentChunkGameObjects.Count];

        for (int i = 0; i < GameObjects.Length; i++)
        {
            GameObjects[i] = CurrentChunkGameObjects[i];
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Chunk")
            CurrentChunkGameObjects.Add(collider.gameObject);
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (CurrentChunkGameObjects.Contains(collider.gameObject))
            CurrentChunkGameObjects.Remove(collider.gameObject);
    }
}