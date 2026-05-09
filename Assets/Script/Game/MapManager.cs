using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Transform player;         // Transform of the player
    public float tileSize = 1000f;    // Size of one tile (1000m)

    [Header("Desert 1 Prefabs (Root at (0,0,0)")]
    public GameObject desert1_LOD0;
    public GameObject desert1_LOD1;

    [Header("Desert 2 Prefabs (Root at (0,0,0)")]
    public GameObject desert2_LOD0;
    public GameObject desert2_LOD1;

    private struct MapTilePair
    {
        public GameObject lod0Instance;
        public GameObject lod1Instance;
    }

    private MapTilePair[,] tiles = new MapTilePair[7, 7];

    void Start()
    {
        // 1. Randomly generate either Desert 1 or Desert 2 in a 7x7 (49 tiles) area
        for (int x = 0; x < 7; x++)
        {
            for (int z = 0; z < 7; z++)
            {
                int desertType = Random.Range(0, 2);

                GameObject lod0Prefab = (desertType == 0) ? desert1_LOD0 : desert2_LOD0;
                GameObject lod1Prefab = (desertType == 0) ? desert1_LOD1 : desert2_LOD1;

                GameObject lod0 = Instantiate(lod0Prefab, transform);
                GameObject lod1 = Instantiate(lod1Prefab, transform);

                Vector3 initialPos = new Vector3((x - 3) * tileSize, 0, (z - 3) * tileSize);

                // Random rotation around the Y-axis (horizontal direction)
                Quaternion rot = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);

                lod0.transform.position = initialPos;
                lod0.transform.rotation = rot;

                lod1.transform.position = initialPos;
                lod1.transform.rotation = rot;

                tiles[x, z] = new MapTilePair
                {
                    lod0Instance = lod0,
                    lod1Instance = lod1
                };
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // 2. Calculate the player's grid coordinates
        int playerGridX = Mathf.RoundToInt(player.position.x / tileSize);
        int playerGridZ = Mathf.RoundToInt(player.position.z / tileSize);

        // 3. Iterate through the 7x7 (49 tiles) area in a single loop to control warp, deactivation, and LOD switching
        for (int ax = 0; ax < 7; ax++)
        {
            for (int az = 0; az < 7; az++)
            {
                MapTilePair tilePair = tiles[ax, az];

                // Calculate the relative grid distance to the player (-3 to +3) using modulo for fast computation
                int diffX = (ax - (playerGridX % 7) + 7) % 7;
                if (diffX > 3) diffX -= 7;

                int diffZ = (az - (playerGridZ % 7) + 7) % 7;
                if (diffZ > 3) diffZ -= 7;

                // Calculate the correct world position where the tile should be placed
                int gx = playerGridX + diffX;
                int gz = playerGridZ + diffZ;
                Vector3 targetPos = new Vector3(gx * tileSize, 0, gz * tileSize);

                // Only warp the tile when it goes out of bounds and reshuffle its rotation (Y-axis) to reduce unnecessary update load
                if (tilePair.lod0Instance.transform.position != targetPos)
                {
                    tilePair.lod0Instance.transform.position = targetPos;
                    tilePair.lod1Instance.transform.position = targetPos;

                    Quaternion newRot = Quaternion.Euler(0, Random.Range(0, 4) * 90, 0);
                    tilePair.lod0Instance.transform.rotation = newRot;
                    tilePair.lod1Instance.transform.rotation = newRot;
                }

                // 4. Extract the grid distance from the player (absolute value)
                int distX = Mathf.Abs(diffX);
                int distZ = Mathf.Abs(diffZ);

                // 5. If outside the 5x5 range (distance of 3 in the outermost 24 tiles), completely deactivate both LODs
                if (distX > 2 || distZ > 2)
                {
                    tilePair.lod0Instance.SetActive(false);
                    tilePair.lod1Instance.SetActive(false);
                }
                else
                {
                    // If within the 5x5 range, switch LOD (inner 3x3 is high quality, outer is low quality)
                    bool isInner9 = (distX <= 1 && distZ <= 1);
                    tilePair.lod0Instance.SetActive(isInner9);
                    tilePair.lod1Instance.SetActive(!isInner9);
                }
            }
        }
    }
}