#region usings

using System;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

#endregion

namespace Assets.Scripts
{
    public class ChunkController : MonoBehaviour
    {
        public GameObject PlayerGameObject;
        public Transform ChunkParentTransform;
        public Sprite[] Sprites;
        public float GenerationDelayMs = 100f;
        public int ChunkRadious = 1;
        public float ChunkMaxAge = 30f;
        public float PerlinMod = 0.01f;

        private float _generationDelayTicks;
        private Vector2 _perlinOffset = Vector2.zero;
        private Random rdm = new Random();
        private int _chunkRadious;

        [UsedImplicitly]
        void Start()
        {
            _perlinOffset = new Vector2(rdm.Next(64000), rdm.Next(64000));

            GenerateChunk(0, 0);
        }

        [UsedImplicitly]
        void Update()
        {
            _generationDelayTicks += Time.deltaTime * 1000;

            if (_generationDelayTicks > GenerationDelayMs)
            {
                _generationDelayTicks = 0;
                bool chunkGenerated = false;

                PlayerChunkTracker playerChunkTracker = PlayerGameObject.GetComponent<PlayerChunkTracker>();

                foreach (GameObject currentChunkGameObject in playerChunkTracker.CurrentChunkGameObjects)
                {
                    Vector2 currentPositionVector2 = currentChunkGameObject.GetComponent<ChunkPosition>().Position;

                    for (int cx = 0 - (_chunkRadious - 1); cx <= _chunkRadious - 1; cx++)
                    {
                        if (chunkGenerated)
                            break;

                        for (int cy = 0- (_chunkRadious - 1); cy <= _chunkRadious - 1; cy++)
                        {
                            if (chunkGenerated)
                                break;

                            Vector2 newPositionVector2 = new Vector2(cx, cy) + currentPositionVector2;
                            GameObject foundChunkGameObject = GameObject.Find(GetChunkName(newPositionVector2));

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (!foundChunkGameObject && !chunkGenerated)
                            {
                                GenerateChunk(newPositionVector2);
                                chunkGenerated = true;
                            }
                            else
                            {
                                foundChunkGameObject.GetComponent<ChunkRemoval>().ChunkAge = 0;
                                foundChunkGameObject.SetActive(true);
                            }
                        }
                    }
                }

                if (!chunkGenerated)
                {
                    _chunkRadious++;

                    if (_chunkRadious > ChunkRadious)
                        _chunkRadious = ChunkRadious;
                }
            }
        }

        [UsedImplicitly]
        private void Log(object message)
        {
            Debug.Log(string.Format("{0} : {1}", DateTime.UtcNow.TimeOfDay, message));
        }

        private void GenerateChunk(Vector2 chunkPosition)
        {
            GenerateChunk(chunkPosition.x, chunkPosition.y);
        }
        private void GenerateChunk(float cx, float cy)
        {
            GameObject chunk = new GameObject();
            ChunkPosition chunkPosition = chunk.gameObject.AddComponent<ChunkPosition>();

            chunkPosition.Position = new Vector2(cx, cy);

            chunk.transform.parent = ChunkParentTransform;
            chunk.name = "Chunk X" + chunkPosition.Position.x + " Y" + chunkPosition.Position.y;

            chunk.transform.position = chunkPosition.ActualPosition;

            chunk.AddComponent<BoxCollider2D>().offset = new Vector2(7.5f, 7.5f);
            chunk.GetComponent<BoxCollider2D>().size = new Vector2(16, 16);
            chunk.GetComponent<BoxCollider2D>().isTrigger = true;
            chunk.AddComponent<ChunkInfo>().ChunkPosition = chunkPosition;

            chunk.gameObject.tag = "Chunk";

            chunk.AddComponent<Rigidbody2D>().gravityScale = 0;
            chunk.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            ChunkRemoval chunkRemoval = chunk.AddComponent<ChunkRemoval>();
            chunkRemoval.ChunkControllerGameObject = gameObject;

            for (int bx = 0 + (int)chunk.transform.position.x; bx < 16 + (int)chunk.transform.position.x; bx++)
            {
                for (int by = 0 + (int)chunk.transform.position.y; by < 16 + (int)chunk.transform.position.y; by++)
                {
                    GameObject o = new GameObject();
                
                    o.AddComponent<SpriteRenderer>().sprite = Sprites[Convert.ToInt32(Mathf.PerlinNoise(bx * PerlinMod + _perlinOffset.x, by * PerlinMod + _perlinOffset.y) * 10)];

                    o.transform.parent = chunk.transform;

                    o.transform.position = new Vector3(bx, by, 0);
                    o.name = "X" + bx + " Y" + by;
                }
            }
        }

        private string GetChunkName(Vector2 chunkPosition)
        {
            return GetChunkName(chunkPosition.x, chunkPosition.y);
        }
        private string GetChunkName(float x, float y)
        {
            return "Chunk X" + x + " Y" + y;
        }
    }
}
