using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject[] tilePrefabs;
    public GameObject treePrefab;
    public GameObject capsulePrefab;
    public GameObject fishPrefab;
    public GameObject gemPrefab;
    public GameObject dustCloudPrefab;

    public Text TextScore;
    public List<GameObject> spawnedTiles = new List<GameObject>();

    private Dictionary<string, List<GameObject>> tileGroups = new Dictionary<string, List<GameObject>>();
    private long dirtBB = 0;
    private long treeBB = 0;

    public void CreateBoard()
    {
        DeleteBoard();
        tileGroups.Clear();
        dirtBB = 0;
        treeBB = 0;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                int randomTile = UnityEngine.Random.Range(0, tilePrefabs.Length);
                Vector3 pos = new Vector3(c, 0, r);
                GameObject tile = Instantiate(tilePrefabs[randomTile], pos, Quaternion.identity);
                tile.name = tile.tag + "_" + r + "_" + c;

                if (!tileGroups.ContainsKey(tile.tag))
                    tileGroups[tile.tag] = new List<GameObject>();
                tileGroups[tile.tag].Add(tile);
                spawnedTiles.Add(tile);

                if (tile.tag == "Dirt")
                    dirtBB = SetCellState(dirtBB, r, c);
            }
        }

        // Start spawn rules
        InvokeRepeating(nameof(PlantTree), 0.25f, 2f);
        InvokeRepeating(nameof(SpawnCapsule), 0.5f, 3f);
        InvokeRepeating(nameof(SpawnFish), 0.25f, 1f);
        InvokeRepeating(nameof(SpawnRockGem), 0.5f, 4f);
        InvokeRepeating(nameof(SpawnDust), 0.5f, 6f);
    }

    public void DeleteBoard()
    {
        CancelInvoke();

        foreach (GameObject tile in spawnedTiles)
        {
            if (tile != null)
                DestroyImmediate(tile.gameObject);
        }

        spawnedTiles.Clear();
        tileGroups.Clear();
    }

    private void PlantTree()
    {
        TrySpawnFromTag("Dirt", treePrefab, ref treeBB);
    }

    private void SpawnCapsule()
    {
        TrySpawnFromTag("Grass", capsulePrefab);
    }

    private void SpawnFish()
    {
        TrySpawnFromTag("Water", fishPrefab);
    }

    private void SpawnRockGem()
    {
        TrySpawnFromTag("Rock", gemPrefab);
        TrySpawnFromTag("Woods", gemPrefab);
    }

    private void SpawnDust()
    {
        TrySpawnFromTag("Sand", dustCloudPrefab);
    }

    private void TrySpawnFromTag(string tag, GameObject prefab, ref long bitboard)
    {
        if (!tileGroups.ContainsKey(tag) || tileGroups[tag].Count == 0) return;

        int index = UnityEngine.Random.Range(0, tileGroups[tag].Count);
        GameObject tile = tileGroups[tag][index];

        if (tile.transform.childCount == 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.transform.parent = tile.transform;
            obj.transform.localPosition = Vector3.zero;

            string[] parts = tile.name.Split('_');
            if (parts.Length == 3 &&
                int.TryParse(parts[1], out int row) &&
                int.TryParse(parts[2], out int col))
            {
                bitboard = SetCellState(bitboard, row, col);
            }
        }
    }

    private void TrySpawnFromTag(string tag, GameObject prefab)
    {
        long dummy = 0;
        TrySpawnFromTag(tag, prefab, ref dummy);
    }

    private long SetCellState(long bitboard, int row, int col)
    {
        long newBit = 1L << (row * 8 + col);
        return (bitboard |= newBit);
    }

    private bool GetCellState(long bitboard, int row, int col)
    {
        long mask = 1L << (row * 8 + col);
        return (bitboard & mask) != 0;
    }

    private int CellCount(long bitboard)
    {
        int count = 0;
        long bb = bitboard;
        while (bb != 0)
        {
            bb &= bb - 1;
            count++;
        }
        return count;
    }
}
