using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public Tile tilePrefab;
    public TileState[] tileStates;

    private TileGrid _grid;
    private List<Tile> _tiles;
    private bool _waiting;

    private void Awake()
    {
        _grid = GetComponentInChildren<TileGrid>();
        _tiles = new List<Tile>(16);
    }

    public void ClearBoard()
    {
        foreach (var cell in _grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in _tiles)
        {
            Destroy(tile.gameObject);
        }

        _tiles.Clear();
    }

    public void CreateTile()
    {
        var tile = Instantiate(tilePrefab, _grid.transform);
        tile.SetState(tileStates[0]);
        tile.LinkCell(_grid.GetRandomEmptyCell());
        _tiles.Add(tile);
    }

    private void Move(
        Vector2Int direction,
        int startX,
        int incrementX,
        int startY,
        int incrementY
    )
    {
        var changed = false;
        for (int x = startX; x >= 0 && x < _grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < _grid.height; y += incrementY)
            {
                var cell = _grid.GetCell(x, y);

                if (cell.IsOccupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }
    }

    private IEnumerator WaitForChanges()
    {
        _waiting = true;

        yield return new WaitForSeconds(0.1f);

        _waiting = false;

        foreach (var tile in _tiles)
        {
            tile.locked = false;
        }

        if (_tiles.Count != _grid.size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            GameManager.Instance.GameOver();
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        var adjacent = _grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.IsOccupied)
            {
                if (CanMerge(tile, adjacent.tile))
                {
                    MergeTiles(tile, adjacent.tile);
                    return true;
                }

                break;
            }

            newCell = adjacent;
            adjacent = _grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.state == b.state && !b.locked;
    }

    private void MergeTiles(Tile a, Tile b)
    {
        _tiles.Remove(a);
        a.Merge(b.cell);

        var index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        var newState = tileStates[index];

        b.SetState(newState);

        GameManager.Instance.IncreaseScore(newState.number);
    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
            {
                return i;
            }
        }

        return -1;
    }

    public bool CheckForGameOver()
    {
        if (_tiles.Count != _grid.size) return false;

        foreach (var tile in _tiles)
        {
            var up = _grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            if (up != null && CanMerge(tile, up.tile))
            {
                return false;
            }

            var down = _grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            if (down != null && CanMerge(tile, down.tile))
            {
                return false;
            }

            var left = _grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            if (left != null && CanMerge(tile, left.tile))
            {
                return false;
            }

            var right = _grid.GetAdjacentCell(tile.cell, Vector2Int.right);
            if (right != null && CanMerge(tile, right.tile))
            {
                return false;
            }
        }

        return true;
    }

    private void Update()
    {
        if (_waiting) return;
        
        ListenKeyboard();
        ListenTouch();
    }

    private void ListenKeyboard()
    {
        
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down, 0, 1, _grid.height - 2, -1);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right, _grid.width - 2, -1, 0, 1);
        }
    }
    
    Vector2 startPos;
    float minSwipeDist = 100f;
    private void ListenTouch()
    {
        if (Input.touchCount == 0) return;

        var touch = Input.touches[0];

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startPos = touch.position;
                break;
            case TouchPhase.Ended:
                var swipeVector = touch.position - startPos;
                if (Mathf.Abs(swipeVector.x) > minSwipeDist && Mathf.Abs(swipeVector.x) >= Mathf.Abs(swipeVector.y))
                {
                    if (swipeVector.x > 0)
                    {
                        Move(Vector2Int.right, _grid.width - 2, -1, 0, 1);
                    }
                    else
                    {
                        Move(Vector2Int.left, 1, 1, 0, 1);
                    }
                }

                if (Mathf.Abs(swipeVector.y) > minSwipeDist && Mathf.Abs(swipeVector.y) > Mathf.Abs(swipeVector.x))
                {
                    if (swipeVector.y > 0)
                    {
                        Move(Vector2Int.up, 0, 1, 1, 1);
                    }
                    else
                    {
                        Move(Vector2Int.down, 0, 1, _grid.height - 2, -1);
                    }
                }

                break;
        }
    }
}