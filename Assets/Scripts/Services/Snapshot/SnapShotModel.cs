using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SnapShotModel : IModel
{
    public float ElapsedTimeSeconds;
    public List<ValuePlacement> Placements = new List<ValuePlacement>();
}

[Serializable]
public struct ValuePlacement
{
    public int Row;
    public int Col;
    public int Value;

    public ValuePlacement(int row, int col, int value)
    {
        Row = row;
        Col = col;
        Value = value;
    }
}
