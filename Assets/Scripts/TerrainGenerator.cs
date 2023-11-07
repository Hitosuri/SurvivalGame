using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;
using RandomUnity = UnityEngine.Random;

namespace Assets.Scripts {
    [Serializable]
    public struct DataPair {
        public GameObject plant;
        [Range(0, 1)]
        public float rate;
    }

    [Serializable]
    public class TerrainTileType {
        public TileBase tile;
        public float weight;
        public DataPair[] plants;

        [NonSerialized]
        public Tilemap tilemap;

        [NonSerialized]
        public TileBase[] tiles;
    }

    public class TerrainGenerator : MonoBehaviour {
        public Transform observer;
        public int chunkSize;
        public int mapSize;
        public float viewDistance;
        public int seed;

        [Header("Terrain")]
        public Transform mapParent;
        public Vector2 noiseScale;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public TerrainTileType[] terrainTileType;
        public TileBase soilTileBase;
        public static TileBase[] emptyTiles;
        public static TileBase soilTileBaseStatic;

        [Header("Plant")]
        public Transform plantParent;
        public GameObject plant;
        public Vector2 plantNoiseScale;
        public int plantOctaves;
        public float plantPersistance;
        public float plantLacunatity;

        private int positionScale = 2;

        private int chunkVisibleInViewDistance;
        private Dictionary<Vector2Int, TerrainChunk> terrainCheckDictionary;
        private Queue<Tuple<TerrainChunk, List<Tuple<Vector3, GameObject>>>> chunkGenerateResultQueue;

        private void Start() {
            soilTileBaseStatic = soilTileBase;
            chunkGenerateResultQueue = new Queue<Tuple<TerrainChunk, List<Tuple<Vector3, GameObject>>>>();
            terrainCheckDictionary = new Dictionary<Vector2Int, TerrainChunk>();
            chunkVisibleInViewDistance = Mathf.CeilToInt(viewDistance / chunkSize);
            emptyTiles = new TileBase[chunkSize * chunkSize];

            SetupTilemapLayer();

            GameManager.Instance.OneSecondTick += _ => {
                UpdateVisibleChunk();
                lock (chunkGenerateResultQueue) {
                    while (chunkGenerateResultQueue.Count > 0) {
                        var result = chunkGenerateResultQueue.Dequeue();

                        var terrainChunk = result.Item1;
                        var position = terrainChunk.coord * chunkSize;
                        terrainChunk.chunkPlant = new GameObject($"{terrainChunk.coord.x}:{terrainChunk.coord.y}") {
                            transform = {
                                parent = plantParent,
                                position = new Vector3(position.x, position.y, 0),
                            },
                        };
                        terrainChunk.chunkPlant.SetActive(false);

                        for (int i = 0; i < result.Item2.Count; i++) {
                            var h = Instantiate(result.Item2[i].Item2, result.Item2[i].Item1, Quaternion.identity);
                            var spriteRenderer = h.GetComponent<SpriteRenderer>();
                            spriteRenderer.sortingLayerName = "On Ground";
                            spriteRenderer.sortingOrder = Mathf.CeilToInt(result.Item2[i].Item1.y * 4 * -1);
                            h.transform.parent = terrainChunk.chunkPlant.transform;
                        }

                        terrainChunk.dataCompleted = true;
                    }
                }
            };
        }

        private void SetupTilemapLayer() {
            for (int i = 0; i < terrainTileType.Length; i++) {
                var gameObject = new GameObject($"layer {i}") {
                    transform = {
                        parent = mapParent,
                        position = Vector3.zero
                    }
                };
                terrainTileType[i].tilemap = gameObject.AddComponent<Tilemap>();
                var tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
                tilemapRenderer.sortingOrder = i;

                terrainTileType[i].tiles = new TileBase[chunkSize * chunkSize];
                Array.Fill(terrainTileType[i].tiles, terrainTileType[i].tile);
            }

            GameManager.Instance.SoilLayer = new GameObject("soil") {
                transform = {
                    parent = mapParent,
                    position = Vector3.zero
                }
            }.AddComponent<Tilemap>();
            var soilRenderer = GameManager.Instance.SoilLayer.gameObject.AddComponent<TilemapRenderer>();
            soilRenderer.sortingOrder = terrainTileType.Length;
        }

        private void RequestMapData(TerrainChunk terrainChunk) {
            Vector2Int chunkOffset = terrainChunk.coord * chunkSize;
            Thread thread = new Thread(
                () => {
                    float[,] heightMap = PerlinNoise.GenerateHeightMap(
                        chunkSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, chunkOffset, true
                    );

                    float[,] plantHeightMap = PerlinNoise.GenerateHeightMap(
                        chunkSize * positionScale, mapSize, seed, plantNoiseScale, plantOctaves, plantPersistance, plantLacunatity,
                        chunkOffset * positionScale, false
                    );

                    int randomSeed = seed + (terrainChunk.coord.x * 1000 + terrainChunk.coord.y);
                    Random random = new Random(randomSeed);

                    Vector3Int[][] tilePos = new Vector3Int[terrainTileType.Length][];
                    List<Vector3Int>[] tilePos2 = new List<Vector3Int>[terrainTileType.Length];
                    for (int i = 0; i < terrainTileType.Length; i++) {
                        tilePos2[i] = new List<Vector3Int>();
                    }

                    List<Tuple<Vector3, GameObject>> plantPositions = new List<Tuple<Vector3, GameObject>>();
                    for (int y = 0; y < chunkSize; y++) {
                        for (int x = 0; x < chunkSize; x++) {
                            DataPair[] allowPlants = Array.Empty<DataPair>();
                            for (int i = 0; i < terrainTileType.Length; i++) {
                                if (terrainTileType[i].weight < heightMap[x, y]) {
                                    allowPlants = terrainTileType[i].plants;
                                    tilePos2[i].Add(new Vector3Int(chunkOffset.x + x, chunkOffset.y + y));
                                } else {
                                    break;
                                }
                            }
                            if (allowPlants.Length == 0) {
                                continue;
                            }
                            int loopCount = random.Next(positionScale) + 1;
                            for (int i = 0; i < loopCount; i++) {
                                if (plantHeightMap[x * positionScale + i, y * positionScale + i] > 0.5) {
                                    continue;
                                }
                                float value = (float)random.NextDouble();
                                for (int a = 0; a < allowPlants.Length; a++) {
                                    if (value < allowPlants[a].rate) {
                                        Vector3 pos = new Vector3(chunkOffset.x + x, chunkOffset.y + y);
                                        pos.x += i * (1f / loopCount) + (float)(random.NextDouble() * (0.8 / loopCount));
                                        pos.y += i * (1f / loopCount) + (float)(random.NextDouble() * (0.8 / loopCount));
                                        plantPositions.Add(new Tuple<Vector3, GameObject>(pos, allowPlants[a].plant));
                                        break;
                                    }

                                    value -= allowPlants[a].rate;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < terrainTileType.Length; i++) {
                        tilePos[i] = tilePos2[i].ToArray();
                    }

                    terrainChunk.tilePositions = tilePos;

                    lock (chunkGenerateResultQueue) {
                        chunkGenerateResultQueue.Enqueue(new Tuple<TerrainChunk, List<Tuple<Vector3, GameObject>>>(terrainChunk, plantPositions));
                    }
                }
            );
            thread.Start();
        }

        private void UpdateVisibleChunk() {
            Vector2 observerPosition = observer.position;
            int currentChunkCoordX = Mathf.RoundToInt(observerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(observerPosition.y / chunkSize);
            int halfMapSize = mapSize / 2;

            for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++) {
                for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++) {
                    var viewdChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if (Mathf.Abs(viewdChunkCoord.x) * chunkSize > halfMapSize
                        || Mathf.Abs(viewdChunkCoord.y) * chunkSize > halfMapSize) {
                        continue;
                    }

                    if (terrainCheckDictionary.TryGetValue(viewdChunkCoord, out var currentTerrain)) {
                        currentTerrain.CheckLoad(GameManager.Instance);
                    } else {
                        terrainCheckDictionary.Add(
                            viewdChunkCoord,
                            new TerrainChunk(
                                viewdChunkCoord, chunkSize, RequestMapData, print, terrainTileType, observer,
                                viewDistance
                            )
                        );
                    }
                }
            }
        }
    }
}