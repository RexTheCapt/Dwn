#region usings

using UnityEngine;

#endregion

public class ChunkRemoval : MonoBehaviour
{
    public GameObject ChunkControllerGameObject;
    public float chunkAge;
    public bool playerIsClose;

    private float _maxChunkAge
    {
        get { return ChunkControllerGameObject.GetComponent<ChunkController>().chunkMaxAge; }
    }

    void Update()
    {
        if (!playerIsClose)
            chunkAge += Time.deltaTime;
        else
            chunkAge = 0;

        if(chunkAge > _maxChunkAge)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.tag == "Player")
            playerIsClose = true;
    }

    void OnTriggerExit2D(Collider2D collider2D)
    {
        if (collider2D.gameObject.tag == "Player")
            playerIsClose = false;
    }
}