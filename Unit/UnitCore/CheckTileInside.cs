using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CheckTileInside : MonoBehaviour
{
    private Tilemap tilemap;

    public void Start()
    {
        tilemap = GameObject.Find("Background").GetComponent<Tilemap>();
    }
    
    public bool IsTileInside(bool alreadyCheck = false)
    {
        if (alreadyCheck) return true;
        Vector3 worldPosition = transform.position;
        Vector3Int cellPosition = tilemap.WorldToCell(worldPosition);
        TileBase tile = tilemap.GetTile(cellPosition);

        if (tile != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
