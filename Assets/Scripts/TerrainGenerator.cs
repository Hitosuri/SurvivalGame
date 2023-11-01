using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts {
    [Serializable]
    public class TerrainTileType {
        public TileBase tile;
        public float weight;

        [NonSerialized]
        public Tilemap tilemap;

        [NonSerialized]
        public TileBase[] tiles;
    }

    public class TerrainGenerator : MonoBehaviour {
        public Transform observer;
        public Transform mapParent;
        public int chunkSize;
        public int mapSize;
        public float viewDistance;
        public int seed;
        public float noiseScale;
        public int octaves;
        public float persistance;
        public float lacunarity;
        public TerrainTileType[] terrainTileType;
        public static TileBase[] emptyTiles;

        private int chunkVisibleInViewDistance;
        private Dictionary<Vector2Int, TerrainChunk> terrainCheckDictionary;
        private Queue<Tuple<Action<Vector3Int[][]>, Vector3Int[][]>> chunkGenerateResultQueue;

        private void Start() {
            chunkGenerateResultQueue = new Queue<Tuple<Action<Vector3Int[][]>, Vector3Int[][]>>();
            terrainCheckDictionary = new Dictionary<Vector2Int, TerrainChunk>();
            chunkVisibleInViewDistance = Mathf.CeilToInt(viewDistance / chunkSize);
            emptyTiles = new TileBase[chunkSize * chunkSize];

            SetupTilemapLayer();
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
                GameManager.Instance.OneSecondTick += manager => {
                    UpdateVisibleChunk();
                    lock (chunkGenerateResultQueue) {
                        if (chunkGenerateResultQueue.Count > 0) {
                            var result = chunkGenerateResultQueue.Dequeue();
                            result.Item1.Invoke(result.Item2);
                        }
                    }
                };
            }
        }

        private void RequestMapData(Action<Vector3Int[][]> callback, Vector2Int chunkOffset) {
            Thread thread = new Thread(
                () => {
                    float[,] heightMap = PerlinNoise.GenerateHeightMap(
                        chunkSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, chunkOffset, true
                    );

                    Vector3Int[][] tilePos = new Vector3Int[terrainTileType.Length][];
                    for (int i = 0; i < terrainTileType.Length; i++) {
                        List<Vector3Int> tileInLayer = new List<Vector3Int>();
                        for (int y = 0; y < chunkSize; y++) {
                            for (int x = 0; x < chunkSize; x++) {
                                if (terrainTileType[i].weight < heightMap[x, y]) {
                                    tileInLayer.Add(new Vector3Int(chunkOffset.x + x, chunkOffset.y + y));
                                }
                            }
                        }

                        tilePos[i] = tileInLayer.ToArray();
                    }

                    lock (chunkGenerateResultQueue) {
                        chunkGenerateResultQueue.Enqueue(
                            new Tuple<Action<Vector3Int[][]>, Vector3Int[][]>(callback, tilePos)
                        );
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