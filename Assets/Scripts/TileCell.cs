using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileCell : MonoBehaviour
{
    public Vector2Int coordinates { get; set; }
    public Tile tile { get; set; }
    
    public bool IsEmpty()
    {
        return tile == null;
    }
    
    public bool IsOccupied => tile != null;
}
