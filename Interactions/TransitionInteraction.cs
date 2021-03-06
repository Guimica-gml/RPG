using Godot;
using Godot.Collections;

public class TransitionInteraction : Interaction
{
	[Export(PropertyHint.File, "*.tscn")] public string ScenePath;
	[Export] public ItemStats Key = null;
	[Export] public string SceneEntryIndetifier = "none";
	[Export] public TransitionTypes TransitionType = TransitionTypes.Default;

	private Inventory _playerInventory;

	public override void OnTrigger()
	{
		_playerInventory = GD.Load<Inventory>("res://Inventory/PlayerInventory.tres");

		if (Key == null || _playerInventory.HasKeyItem(Key.Name))
		{
			Global.TransitionManager.Connect("TransitionEnded", this, "OnTransitionEnded");
			Global.TransitionManager.ChangeSceneTo(ScenePath, SceneEntryIndetifier, TransitionType);
		}
		else
		{
			Global.DialogManager.Connect("DialogEnded", this, "OnDialogEnded");
			Global.DialogManager.StartDialog("res://DialogBox/Scripts/NeedKey.json", new Array<string>() { Key.Name });
		}
	}

	public override bool CanTrigger()
	{
		return true;
	}

	public override void OnEnd()
	{
		return;
	}

	public override Texture GetIcon()
	{
		return GD.Load<Texture>("res://Interactions/Icons/TransitionIcon.png");
	}

	private void OnTransitionEnded()
	{
		EmitSignal(nameof(Ended));
		Global.TransitionManager.Disconnect("TransitionEnded", this, "OnTransitionEnded");
	}

	private void OnDialogEnded(string dialogPath)
	{
		EmitSignal(nameof(Ended));
		Global.DialogManager.Disconnect("DialogEnded", this, "OnDialogEnded");
	}
}
