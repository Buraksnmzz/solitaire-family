using System;
using System.Collections.Generic;
using Card;
using Levels;

[Serializable]
public class SnapShotModel : IModel
{
    public int LevelIndex;
    public int MovesCount;
    public List<CardSnapshot> Cards = new List<CardSnapshot>();
}

[Serializable]
public class CardSnapshot
{
    public CardType Type;
    public CardCategoryType CategoryType;
    public string CategoryName;
    public string ContentName;
    public int ContentCount;
    public int CurrentContentCount;
    public bool IsFaceUp;
    public SnapshotContainerType ContainerType;
    public int ContainerIndex;
}

public enum SnapshotContainerType
{
    Dealer = 0,
    OpenDealer = 1,
    Foundation = 2,
    Pile = 3
}
