using Godot;

public partial class Card : Resource
{
    [Export] public string Name;
    [Export(PropertyHint.MultilineText)] public string Description;
    [Export] public Texture2D Icon;

    public virtual void Play(Hand hand, int index) { }
}
