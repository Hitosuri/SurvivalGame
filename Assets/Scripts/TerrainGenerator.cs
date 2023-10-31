using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts {
    [Serializable]
    public struct TerrainTileType {
        public TileBase tile;
        public float weight;
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

        private int chunkVisibleInViewDistance;
        private Dictionary<Vector2Int, TerrainChunk> terrainCheckDictionary;
        List<TerrainChunk> terrainChunksVisibleLastUpdate;
        private Queue<Tuple<Action<float[,]>, float[,]>> chunkGenerateResultQueue;

        private void Start() {
            chunkGenerateResultQueue = new Queue<Tuple<Action<float[,]>, float[,]>>();
            terrainCheckDictionary = new Dictionary<Vector2Int, TerrainChunk>();
            terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
            chunkVisibleInViewDistance = Mathf.CeilToInt(viewDistance / chunkSize);
        }

        private void FixedUpdate() {
            UpdateVisibleChunk();
            if (chunkGenerateResultQueue.Count > 0) {
                lock (chunkGenerateResultQueue) {
                    var result = chunkGenerateResultQueue.Dequeue();
                    result.Item1.Invoke(result.Item2);
                }
            }
        }

        private void RequestMapData(Action<float[,]> callback, Vector2 chunkOffset) {
            Thread thread = new Thread(
                () => {
                    float[,] heightMap = PerlinNoise.GenerateHeightMap(
                        chunkSize, mapSize, seed, noiseScale, octaves, persistance, lacunarity, chunkOffset, true
                    );
                    lock (chunkGenerateResultQueue) {
                        chunkGenerateResultQueue.Enqueue(new Tuple<Action<float[,]>, float[,]>(callback, heightMap));
                    }
                }
            );
            thread.Start();
        }

        private TileBase SelectTileType(float height) {
            for (int i = terrainTileType.Length - 1; i >= 0; i--) {
                if (height >= terrainTileType[i].weight) {
                    return terrainTileType[i].tile;
                }
            }

            return null;
        }

        private void UpdateVisibleChunk() {
            for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
                terrainChunksVisibleLastUpdate[i].IsVisible = false;
            }
            terrainChunksVisibleLastUpdate.Clear();

            Vector2 observerPosition = observer.position;
            int currentChunkCoordX = Mathf.RoundToInt(observerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(observerPosition.y / chunkSize);
            int halfMapSize = mapSize / 2;

            for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++) {
                for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++) {
                    var viewdChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if (Mathf.Abs(viewdChunkCoord.x) * chunkSize > halfMapSize || Mathf.Abs(viewdChunkCoord.y) * chunkSize > halfMapSize) {
                        continue;
                    }

                    if (terrainCheckDictionary.TryGetValue(viewdChunkCoord, out var currentTerrain)) {
                        bool isVisible = currentTerrain.UpdateVisibility(observerPosition, viewDistance);
                        if (isVisible) {
                            terrainChunksVisibleLastUpdate.Add(currentTerrain);
                        }
                    } else {
                        terrainCheckDictionary.Add(viewdChunkCoord, new TerrainChunk(viewdChunkCoord, chunkSize, mapParent, RequestMapData, SelectTileType, print));
                    }
                }
            }
        }
    }
}