using Godot;

public class Action : MyResource
{
	[Signal] private delegate void PPChanged(int pp);

	[Export] public string Name = "";
	[Export(PropertyHint.File, "*.tscn")] public string AnimScenePath = "";
	[Export] public bool Heal = false;
	[Export] public int Value = 1;

	[Export] public bool ConsiderPP = true;
	[Export] public int MaxPP = 1;

	private int _pp = 1;
	public int PP
	{
		get => _pp;
		set
		{
			_pp = value;
			EmitSignal(nameof(PPChanged), PP);
		}
	}

	public override void _Init()
	{
		PP = MaxPP;
	}
}
