﻿#region usings

using System;
using Boo.Lang;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

#endregion

public class ChunkController : MonoBehaviour
{
    public GameObject PlayerGameObject;
    public Transform ChunkParentTransform;
    public Sprite[] sprites;
    public float generationDelayMs = 100f;
    public bool generateChunkOnce = false;
    public float chunkMaxAge = 30f;
    public float perlinMod = 0.01f;

    private float _generationDelayTicks;
    private bool _generateChunkOnceTriggerd = false;
    private float perlinOffset = 0;

    void Start()
    {
        GenerateChunk(0, 0);
    }

    void Update()
    {
        _generationDelayTicks += Time.deltaTime * 1000;

        if (_generationDelayTicks > generationDelayMs)
        {
            _generationDelayTicks = 0;
            bool chunkGenerated = false;

            PlayerChunkTracker playerChunkTracker = PlayerGameObject.GetComponent<PlayerChunkTracker>();
            int chunkObjectIndex = playerChunkTracker.CurrentChunkGameObjects.Count - 1;
            ChunkPosition currentChunkPositionClass = playerChunkTracker.CurrentChunkGameObjects[chunkObjectIndex].GetComponent<ChunkPosition>();
            //Vector2 currentPositionVector2 = currentChunkPositionClass.Position;

            foreach (GameObject currentChunkGameObject in playerChunkTracker.CurrentChunkGameObjects)
            {
                Vector2 currentPositionVector2 = currentChunkGameObject.GetComponent<ChunkPosition>().Position;

                for (int cx = -5; cx <= 5; cx++)
                {
                    if (chunkGenerated)
                        break;

                    for (int cy = -5; cy <= 5; cy++)
                    {
                        if (chunkGenerated)
                            break;

                        Vector2 newPositionVector2 = new Vector2(cx, cy) + currentPositionVector2;
                        GameObject foundChunkGameObject = GameObject.Find(GetChunkName(newPositionVector2));

                        if (!foundChunkGameObject && !chunkGenerated)
                        {
                            GenerateChunk(newPositionVector2);
                            chunkGenerated = true;
                            Log("Chunk generated");
                        }
                        else
                        {
                            //foundChunkGameObject.GetComponent<ChunkRemoval>().chunkAge = 0;
                        }
                    }
                }
            }
        }
    }

    private void Log(object message)
    {
        Debug.Log(string.Format("{0} : {1}", DateTime.UtcNow.TimeOfDay, message));
    }

    private void GenerateChunk(Vector2 ChunkPosition)
    {
        GenerateChunk(ChunkPosition.x, ChunkPosition.y);
    }
    private void GenerateChunk(float cx, float cy)
    {
        if (!generateChunkOnce)
            _generateChunkOnceTriggerd = false;

        if (_generateChunkOnceTriggerd)
            return;

        if (generateChunkOnce)
            _generateChunkOnceTriggerd = true;

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

        ChunkRemoval chunkRemoval = Chunk.AddComponent<ChunkRemoval>();
        chunkRemoval.ChunkControllerGameObject = gameObject;

        for (int bx = 0 + (int)Chunk.transform.position.x; bx < 16 + (int)Chunk.transform.position.x; bx++)
        {
            for (int by = 0 + (int)Chunk.transform.position.y; by < 16 + (int)Chunk.transform.position.y; by++)
            {
                GameObject o = new GameObject();
                
                o.AddComponent<SpriteRenderer>().sprite = sprites[Convert.ToInt32(Mathf.PerlinNoise(bx * perlinMod + perlinOffset, by * perlinMod + perlinOffset) * 10)];

                o.transform.parent = Chunk.transform;

                o.transform.position = new Vector3(bx, by, 0);
                o.name = "X" + bx + " Y" + by;
            }
        }
    }

    private string GetChunkName(Vector2 ChunkPosition)
    {
        return GetChunkName(ChunkPosition.x, ChunkPosition.y);
    }
    private string GetChunkName(float x, float y)
    {
        return "Chunk X" + x + " Y" + y;
    }
}
