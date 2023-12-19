using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [HideInInspector]
    public TileState state;
    [HideInInspector]
    public TileCell cell;
    [HideInInspector]
    public bool locked;
    
    public Image background;
    public TextMeshProUGUI number;

    private void Awake()
    {
        background = GetComponent<Image>();
        number = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState state)
    {
        this.state = state;
        background.color = state.backgroundColor;
        number.text = state.number.ToString();
        number.color = state.textColor;
    }
    
    public void LinkCell(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }
        this.cell = cell;
        this.cell.tile = this;
        transform.position = cell.transform.position;
    }

    private IEnumerator MoveAnimate(Vector3 to, bool merging)
    {
        var elapsed = 0f;
        var duration = 0.1f;
        
        var from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = to;

        if (merging)
        {
            Destroy(gameObject);
        }
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }
        this.cell = cell;
        this.cell.tile = this;
        
        StartCoroutine(MoveAnimate(cell.transform.position, false));
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }
        this.cell = null;
        cell.tile.locked = true;
        
        StartCoroutine(MoveAnimate(cell.transform.position, true));
    }
}
