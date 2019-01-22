#region usings

using Boo.Lang;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

#endregion

public class ChunkController : MonoBehaviour
{
    public GameObject PlayerGameObject;
    public Transform ChunkParentTransform;
    public Sprite sprite;
    public bool nextGen = true;
    void Start()
    {
        GenerateChunk(0, 0);
    }

    private void GenerateChunk(float cx, float cy)
    {
        if(!nextGen)
            return;

        if (genCurr > genLimit)
            return;

        genCurr++;
        nextGen = false;

        GameObject Chunk = new GameObject();
        ChunkPosition chunkPosition = Chunk.gameObject.AddComponent<ChunkPosition>();

        chunkPosition.Position = new Vector2(cx, cy);

        Chunk.transform.parent = ChunkParentTransform;
        Chunk.name = "Chunk X" + chunkPosition.Position.x + " Y" + chunkPosition.Position.y;

        Chunk.transform.position = chunkPosition.ActualPosition;

        Chunk.AddComponent<BoxCollider2D>().offset = new Vector2(7.5f, 7.5f);
        Chunk.GetComponent<BoxCollider2D>().size = new Vector2(16, 16);
        Chunk.GetComponent<BoxCollider2D>().isTrigger = true;
        Chunk.AddComponent<ChunkInfo>().chunkPosition = chunkPosition;

        Chunk.gameObject.tag = "Chunk";

        Chunk.AddComponent<Rigidbody2D>().gravityScale = 0;
        Chunk.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        for (int bx = 0 + (int)Chunk.transform.position.x; bx < 16 + (int)Chunk.transform.position.x; bx++)
        {
            for (int by = 0 + (int)Chunk.transform.position.y; by < 16 + (int)Chunk.transform.position.y; by++)
            {
                GameObject o = new GameObject();

                o.AddComponent<SpriteRenderer>().sprite = sprite;
                o.transform.parent = Chunk.transform;

                o.transform.position = new Vector3(bx, by, 0);
                o.name = "X" + bx + " Y" + by;
            }
        }
    }

    private int genLimit = 20;
    private int genCurr = 0;

    void Update()
    {
        foreach (GameObject o in PlayerGameObject.GetComponent<PlayerChunkTracker>().CurrentChunkGameObjects)
        {
            ChunkPosition currentChunkPosition = o.GetComponent<ChunkPosition>();

            for (int cx = -1; cx <= 1; cx++)
            {
                for (int cy = -1; cy <= 1; cy++)
                {
                    if (!GameObject.Find("Chunk X" + (currentChunkPosition.Position.x + cx) + " Y" + (currentChunkPosition.Position.y + cy)))
                    {
                        GenerateChunk(cx, cy);
                        Debug.Log("Chunk X" + (currentChunkPosition.Position.x + cx) + " Y" + (currentChunkPosition.Position.y + cy));
                    }
                }
            }
        }
    }
}
