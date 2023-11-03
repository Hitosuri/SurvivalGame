using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts {
    public class TerrainChunk {
        public Vector2Int coord;
        public Bounds bounds;
        public int size;
        public Action<object> print;
        public Vector3Int[][] tilePositions;
        public GameObject chunkPlant;
        public bool dataCompleted;

        private Transform observer;
        private float allowdRadius;
        private TerrainTileType[] terrainTileTypes;

        public bool IsVisible { get; private set; }

        public TerrainChunk(
            Vector2Int coord,
            int size,
            Action<TerrainChunk> requestMapAction,
            Action<object> print,
            TerrainTileType[] terrainTileTypes,
            Transform observer,
            float allowdRadius
        ) {
            this.coord = coord;
            this.size = size;
            this.print = print;
            this.observer = observer;
            this.terrainTileTypes = terrainTileTypes;
            this.allowdRadius = allowdRadius;

            var position = coord * size;
            bounds = new Bounds(new Vector3(position.x, position.y), Vector2.one * size);
            IsVisible = false;
            requestMapAction(this);
        }

        private void ShowChunk() {
            if (IsVisible) {
                return;
            }
            IsVisible = true;
            for (int i = 0; i < terrainTileTypes.Length; i++) {
                if (tilePositions[i].Length == 0) {
                    break;
                }
                terrainTileTypes[i].tilemap.SetTiles(tilePositions[i], terrainTileTypes[i].tiles);
            }
        }

        private void HideChunk() {
            if (!IsVisible) {
                return;
            }

            IsVisible = false;
            for (int i = 0; i < terrainTileTypes.Length; i++) {
                terrainTileTypes[i].tilemap.SetTiles(tilePositions[i], TerrainGenerator.emptyTiles);
            }
        }

        private void OnMapDataReceived() {
            CheckLoad(GameManager.Instance);
        }

        public void CheckLoad(GameManager manager) {
            if (!dataCompleted) {
                return;
            }
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(observer.position));
            bool visible = viewerDstFromNearestEdge <= allowdRadius;

            if (visible && !IsVisible) {
                ShowChunk();
                GameManager.Instance.OneSecondTick += CheckLoad;
            }

            if (!visible && IsVisible) {
                GameManager.Instance.OneSecondTick -= CheckLoad;
                HideChunk();
            }
        }
    }
}