using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts {
    public class PerlinNoise {
        public static float[,] GenerateHeightMap(int chunkSize, int mapSize, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, bool useFalloff) {
            float[,] noiseMap = new float[chunkSize, chunkSize];

            Random random = new Random(seed);
            Vector2[] octaveOffets = new Vector2[octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;

            for (int i = 0; i < octaves; i++) {
                float offsetX = random.Next(-100000, 100000) + offset.x;
                float offsetY = random.Next(-100000, 100000) + offset.y;
                octaveOffets[i] = new Vector2(offsetX, offsetY);
                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            if (scale <= 0) {
                scale = 0.0001f;
            }

            float maxLocalNoiseHeight = float.MinValue;
            float minLocalNoiseHeight = float.MaxValue;

            float halfSize = chunkSize / 2f;

            for (int y = 0; y < chunkSize; y++) {
                for (int x = 0; x < chunkSize; x++) {

                    amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++) {
                        float sampleX = (x - halfSize + octaveOffets[i].x) / scale * frequency;
                        float sampleY = (y - halfSize + octaveOffets[i].y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight) {
                        maxLocalNoiseHeight = noiseHeight;
                    } else if (noiseHeight < minLocalNoiseHeight) {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int y = 0; y < chunkSize; y++) {
                for (int x = 0; x < chunkSize; x++) {
                    float normalizeHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2.2f);
                    noiseMap[x, y] = Mathf.Clamp(normalizeHeight, 0, int.MaxValue);

                    if (useFalloff) {
                        float distanceX = Mathf.Abs(offset.x);
                        if (offset.x > 0) {
                            distanceX += x;
                        } else {
                            distanceX += chunkSize - x;
                        }
                        float distanceY = Mathf.Abs(offset.y);
                        if (offset.y > 0) {
                            distanceY += y;
                        } else {
                            distanceY += chunkSize - y;
                        }
                        float maxDistance = Mathf.Max(distanceX, distanceY) / (mapSize / 2f);
                        float falloffValue = FalloffCalcutate(maxDistance);
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffValue);
                    }
                }
            }
            return noiseMap;
        }

        public static float FalloffCalcutate(float value) {
            float a = 3;
            float b = 2.5f;
            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }
}