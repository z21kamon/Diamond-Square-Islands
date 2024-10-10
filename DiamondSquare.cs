using UnityEngine;


[RequireComponent(typeof(TerrainCollider))]
public class DiamondSquare : MonoBehaviour {
    [Range(0.0f, 1.0f)] public float roughness;  // roughness parameter: the greater the value, the smoother the terrain
    [Range(0.0f, 1.0f)] public float waterLevel;  // desired water level in percents (lower x percents will be submerged)
    public GameObject waterPlane;  // water plane object
    public float minIslandHeight; 

    private Terrain terrain;
    private TerrainData data;
    private int size;  // heightmap resolution
    private float[,] heights;  // heightmap array
    private Texture2D texture;


    private void Start() {
        data = transform.GetComponent<TerrainCollider>().terrainData;
        terrain = transform.GetComponent<Terrain>();
        size = data.heightmapResolution;
        texture = new Texture2D((int)data.size.x, (int)data.size.z);
        texture.wrapMode = TextureWrapMode.Clamp;

        Reset();
    }

    public void Reset() {
        // setting all the height to 0 and clearing the texture
        heights = new float[size, size];
        data.SetHeights(0, 0, heights);
        for (int x = 0; x < size; ++x) {
            for (int y = 0; y < size; ++y) texture.SetPixel(x, y, Color.clear);
        }

        texture.Apply();
        TerrainLayer terrainLayer = new TerrainLayer();
        terrainLayer.diffuseTexture = texture;
        terrain.terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
    }


    public void ExecuteDiamondSquare() {
        heights = new float[size, size];
        float midpointHeight, scale = 0.5f;  // initializing parameters

        for (int sideLength = size - 1; sideLength > 1; sideLength /= 2) {
            int halfSide = sideLength / 2;

            // diamond step
            for (int x = 0; x < size - 1; x += sideLength) {
                for (int y = 0; y < size - 1; y += sideLength) {
                    midpointHeight = (heights[x, y] + heights[x + sideLength, y] + heights[x, y + sideLength] + heights[x + sideLength, y + sideLength]) / 4.0f;  // calculating the average height
                    midpointHeight += Random.Range(-1.0f, 1.0f) * scale;  // adding random displacement
                    heights[x + halfSide, y + halfSide] = midpointHeight;  // setting point height
                }
            }

            // square step
            for (int x = 0; x < size - 1; x += halfSide) {
                for (int y = (x + halfSide) % sideLength; y < size - 1; y += sideLength) {
                    midpointHeight = (heights[(x - halfSide + size - 1) % (size - 1), y] + heights[(x + halfSide) % (size - 1), y] + heights[x, (y + halfSide) % (size - 1)] + heights[x, (y - halfSide + size - 1) % (size - 1)]) / 4.0f;  // calculating the average height
                    midpointHeight += Random.Range(-1.0f, 1.0f) * scale;  // adding random displacement
                    heights[x, y] = midpointHeight;  // setting point height

                    if (x == 0) heights[size - 1, y] = midpointHeight;
                    if (y == 0) heights[x, size - 1] = midpointHeight;
                }
            }

            scale *= Mathf.Pow(2, -roughness);  // decreasing the scale
        }

        float minHeight = float.MaxValue;

        for (int x = 0; x < size; ++x)
        {
            for (int y = 0; y < size; ++y)
            {
                float heightValue = heights[x, y];
                if (heightValue < minHeight) minHeight = heightValue;
            }
        }

        if (minHeight < 0.0f) {
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    heights[x, y] += minHeight;
                }
            }
        }


        data.SetHeights(0, 0, heights);  // setting the heights
        adjustWaterLevel();
        Colorize();
    }

    private void adjustWaterLevel() {
        // calculating lowest and highest points of terrain
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int x = 0; x < size; ++x) {
            for (int y = 0; y < size; ++y) {
                float heightValue = data.GetHeight(x, y);
                if (heightValue < minHeight) minHeight = heightValue;
                if (heightValue > maxHeight) maxHeight = heightValue;
            }
        }

        waterPlane.transform.position = new Vector3(0, minHeight + (maxHeight - minHeight) * waterLevel, 0);  // setting water plane height at desired level
    }

    public void Colorize() {
        // calculating the terrain depending on the height
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

        for (int x = 0; x < size; ++x) {
            for (int y = 0; y < size; ++y) {
                float heightValue = data.GetHeight(x, y);
                if (heightValue < minHeight) minHeight = heightValue;
                if (heightValue > maxHeight) maxHeight = heightValue;
            }
        }

        float heightRange = maxHeight - minHeight;
        float interval = heightRange / 5.0f;

        Debug.Log(heightRange);
        Debug.Log(interval);
        
        for (int x = 0; x < data.size.x; ++x) { 
            for (int y = 0; y < data.size.z; ++y) {
                float heightValue = data.GetHeight(x, y);

                if (heightValue < interval) texture.SetPixel(x, y, new Color(28, 163, 236));  // water
                else if (heightValue < interval * 2.0f) texture.SetPixel(x, y, new Color(194, 178, 128)); // sand
                else if (heightValue < interval * 3.0f) texture.SetPixel(x, y, new Color(72, 144, 48));  // grass
                else if (heightValue < interval * 4.0f) texture.SetPixel(x, y, new Color(136, 140, 141));  // mountains
            }
        }

        texture.Apply();
        // terrain.materialTemplate.mainTexture = texture;

        TerrainLayer terrainLayer = new TerrainLayer();
        terrainLayer.diffuseTexture = texture;
        data.terrainLayers = new TerrainLayer[] { terrainLayer };
    }
}