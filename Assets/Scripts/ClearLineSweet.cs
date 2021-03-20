using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearLineSweet : ClearedSweet 
{

    public bool isRow;

    public override void Clear()
    {
        base.Clear();
        if (isRow)
        {
            sweet.GM.ClearRow(sweet.Y);
        }
        else
        {
            sweet.GM.ClearColumn(sweet.X);
        }
    }
}
