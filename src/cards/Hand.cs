using System.Collections.Generic;

public struct Hand
{
    public List<PlayCard> Cards;
}

public struct PlayCard
{
    public Card Card;
    public bool Selected;
}
