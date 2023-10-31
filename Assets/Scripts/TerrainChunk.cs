using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts {
    public class TerrainChunk {
        public Vector2Int coord;
        public Bounds bounds;
        public Tilemap tilemap;
        public int size;
        public Func<float, TileBase> selectTileType;
        public Action<object> lk;

        public bool IsVisible {
            get => tilemap.gameObject.activeSelf;
            set => tilemap.gameObject.SetActive(value);
        }

        public TerrainChunk(
            Vector2Int coord,
            int size,
            Transform parent,
            Action<Action<float[,]>, Vector2> requestMapAction,
            Func<float, TileBase> selectTileType,
            Action<object> lk
        ) {
            this.coord = coord;
            this.size = size;
            this.selectTileType = selectTileType;
            this.lk = lk;

            var position = coord * size;
            bounds = new Bounds(new Vector3(position.x, position.y), Vector2.one * size);
            var gameObject = new GameObject($"{coord.x}_{coord.y}") {
                transform = {
                    position = Vector3.zero
                },
            };
            tilemap = gameObject.AddComponent<Tilemap>();
            gameObject.AddComponent<TilemapRenderer>();
            tilemap.transform.parent = parent;
            IsVisible = false;
            requestMapAction(OnMapDataReceived, position);
        }

        private void OnMapDataReceived(float[,] heightMap) {
            int b = 0;
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    TileBase tile = selectTileType.Invoke(heightMap[x, y]);
                    if (tile == null) {
                        continue;
                    }

                    b++;
                    tilemap.SetTile(new Vector3Int(coord.x * size + x, coord.y * size + y), tile);
                }
            }

            lk(b);
        }

        public bool UpdateVisibility(Vector2 observer, float allowdRadius) {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(observer));
            bool visible = viewerDstFromNearestEdge <= allowdRadius;
            IsVisible = visible;
            return visible;
        }
    }
}