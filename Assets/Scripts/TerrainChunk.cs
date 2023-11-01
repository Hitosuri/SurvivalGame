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
            var tilemapRenderer = gameObject.AddComponent<TilemapRenderer>();
            if (Mathf.Abs(coord.x % 2) - Mathf.Abs(coord.y % 2) == 0) {
                tilemapRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                tilemapRenderer.sortingOrder = 100;
                var maskObject = new GameObject("mask") {
                    transform = {
                        position = new Vector3(position.x + (size / 2f), position.y + (size / 2f)),
                        localScale = new Vector3(size, size),
                        parent = gameObject.transform,
                    },
                };

                var spriteMask = maskObject.AddComponent<SpriteMask>();
                spriteMask.sprite = Resources.Load<Sprite>("BL_Cement00_1_dummy_7");
            }

            tilemap.transform.parent = parent;
            IsVisible = false;
            requestMapAction(OnMapDataReceived, position);
        }

        private void OnMapDataReceived(float[,] heightMap) {
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    TileBase tile = selectTileType.Invoke(heightMap[x, y]);
                    if (tile == null) {
                        continue;
                    }
                    tilemap.SetTile(new Vector3Int(coord.x * size + x, coord.y * size + y), tile);
                    if (x == 0 && y == 0) {
                        tilemap.SetTile(new Vector3Int(coord.x * size + x - 1, coord.y * size + y -1), tile);
                    }

                    if (x == 0 && y == size - 1) {
                        tilemap.SetTile(new Vector3Int(coord.x * size + x - 1, coord.y * size + y + 1), tile);
                    }

                    if (x == size - 1 && y == 0) {
                        tilemap.SetTile(new Vector3Int(coord.x * size + x + 1, coord.y * size + y - 1), tile);
                    }

                    if (x == size - 1 && y == size - 1) {
                        tilemap.SetTile(new Vector3Int(coord.x * size + x + 1, coord.y * size + y + 1), tile);
                    }

                    if (y == 0) {
                        TileBase g = selectTileType.Invoke(heightMap[x, y]);
                        if (g == tile) {
                            tilemap.SetTile(new Vector3Int(coord.x * size + x, coord.y * size + y - 1), tile);
                        }
                    }

                    if (y == size - 1) {
                        TileBase g = selectTileType.Invoke(heightMap[x, y]);
                        if (g == tile) {
                            tilemap.SetTile(new Vector3Int(coord.x * size + x, coord.y * size + y + 1), tile);
                        }
                    }
                    
                    if (x == 0) {
                        TileBase g = selectTileType.Invoke(heightMap[x, y]);
                        if (g == tile) {
                            tilemap.SetTile(new Vector3Int(coord.x * size + x - 1, coord.y * size + y), tile);
                        }
                    }
                    
                    if (x == size - 1) {
                        TileBase g = selectTileType.Invoke(heightMap[x, y]);
                        if (g == tile) {
                            tilemap.SetTile(new Vector3Int(coord.x * size + x + 1, coord.y * size + y), tile);
                        }
                    }
                }
            }
        }

        public bool UpdateVisibility(Vector2 observer, float allowdRadius) {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(observer));
            bool visible = viewerDstFromNearestEdge <= allowdRadius;
            IsVisible = visible;
            return visible;
        }
    }
}