using Godot;
using System;

public partial class CardDisplayer : Control
{
    public Vector2 originalPosition;
    public Vector2 originalScale;
    private bool wasHovering = false;
    [Export] public float HoverOffsetY = -10.0f; // How far the card moves up
    [Export] public float HoverScale = 1.1f; // How much the card scales up
    [Export] public float AnimationSpeed = 0.5f; // Animation speed (lower is faster)

    public event Action<CardDisplayer> OnClick;

    private Sprite2D icon;
    private Label titleLabel;
    private Label descriptionLabel;
    private TextureButton background;

    public override void _Ready()
    {
        icon = GetNode<Sprite2D>("Icon");
        titleLabel = GetNode<Label>("TitleLabel");
        descriptionLabel = GetNode<Label>("DescriptionLabel");
        background = GetNode<TextureButton>("Background");

        // Store original transform data
        originalPosition = Position;
        originalScale = Scale;
    }

    public void DisplayCard(Card card)
    {
        // Set the card icon if available
        if (card.Icon != null)
        {
            icon.Texture = card.Icon;
            icon.Visible = true;
        }
        else
        {
            icon.Visible = false;
        }

        // Set the title and description
        titleLabel.Text = card.Name;
        descriptionLabel.Text = card.Description;
    }

    public override void _Process(double delta)
    {
        bool isHovering = background.IsHovered();
        if (isHovering)
        {
            // Smoothly animate to hover state
            Position = Position.Lerp(originalPosition + new Vector2(0, HoverOffsetY), (float)(1 - Math.Pow(AnimationSpeed, delta * 60)));
            Scale = Scale.Lerp(originalScale * HoverScale, (float)(1 - Math.Pow(AnimationSpeed, delta * 60)));
            if (!wasHovering)
            {
                // draw hovered card on top of all others
                GetParent().MoveChild(this, -1);
                ZIndex = 1;
            }
        }
        else
        {
            if (wasHovering)
            {
                // move below selected card
                ZIndex = 0;
            }
            // Smoothly animate back to original state
            Position = Position.Lerp(originalPosition, (float)(1 - Math.Pow(AnimationSpeed, delta * 60)));
            Scale = Scale.Lerp(originalScale, (float)(1 - Math.Pow(AnimationSpeed, delta * 60)));
        }
        wasHovering = isHovering;
    }

    private void OnButtonClicked()
    {
        OnClick?.Invoke(this);
    }
}
