using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class HandDisplayer : Control
{
    [Export] public float CardSpacing = 120.0f; // Horizontal spacing between cards
    [Export] public PackedScene CardDisplayerScene { get; set; } // Reference to CardDisplayer scene

    [Export] public float SelectedCardYOffset = -400f;

    [Export] public Card[] TestDisplay;

    private List<CardDisplayer> cardDisplayers = new();
    private List<CardDisplayer> selected = new();
    private Hand currentHand;

    public override void _Ready()
    {
        if (CardDisplayerScene == null)
        {
            GD.PrintErr("CardDisplayerScene must be set in the inspector");
        }
        Hand hand = new()
        {
            Cards = new(TestDisplay.Select((card, _) => new PlayCard { Card = card, Selected = false }))
        };
        DisplayHand(hand);

    }
    public void DisplayHand(Hand hand)
    {
        // Clear existing card displayers
        ClearDisplayers();

        currentHand = hand;

        if (hand.Cards == null || hand.Cards.Count == 0)
            return;

        // Calculate total width needed
        int cardCount = hand.Cards.Count;
        float totalWidth = (cardCount - 1) * CardSpacing;
        float startX = -totalWidth / 2; // Center the cards

        // Create new displayers for each card
        for (int i = 0; i < cardCount; i++)
        {
            var cardDisplayer = CardDisplayerScene.Instantiate<CardDisplayer>();
            AddChild(cardDisplayer);

            cardDisplayer.DisplayCard(hand.Cards[i].Card);
            cardDisplayer.OnClick += OnCardClicked;
            cardDisplayers.Add(cardDisplayer);
        }

        SetCardPositions();
    }

    private void SetCardPositions()
    {
        var selectedCards = currentHand.Cards.Where((card) => card.Selected);

        int selectedCount = selectedCards.Count();
        int unselectedCount = currentHand.Cards.Count - selectedCount;

        float selectedTotalWidth = (selectedCount - 1) * CardSpacing;
        float selectedStartX = -selectedTotalWidth / 2; // Center the cards
        float unselectedTotalWidth = (unselectedCount - 1) * CardSpacing;
        float unselectedStartX = -unselectedTotalWidth / 2; // Center the cards

        int selectedIndex = 0;
        int unselectedIndex = 0;
        for (int i = 0; i < currentHand.Cards.Count; i++)
        {
            if (currentHand.Cards[i].Selected)
            {
                cardDisplayers[i].originalPosition = new Vector2(selectedStartX + (selectedIndex * CardSpacing), SelectedCardYOffset);
                selectedIndex++;
            }
            else
            {
                cardDisplayers[i].originalPosition = new Vector2(unselectedStartX + (unselectedIndex * CardSpacing), 0);
                unselectedIndex++;
            }
        }
    }

    private void ClearDisplayers()
    {
        foreach (var displayer in cardDisplayers)
        {
            displayer.QueueFree();
        }
        cardDisplayers.Clear();
    }

    private void OnCardClicked(CardDisplayer displayer)
    {
        int clickedIndex = cardDisplayers.FindIndex((elem) => elem == displayer);
        if (clickedIndex < 0) return;
        PlayCard clickedCard = currentHand.Cards[clickedIndex];
        currentHand.Cards[clickedIndex] = clickedCard with
        {
            Selected = !clickedCard.Selected
        };
        SetCardPositions();
    }
}
