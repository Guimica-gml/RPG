using Godot;

public class GameOver : Node2D
{
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("left_click") && !Global.TransitionManager.Active)
		{
			GetTree().Quit();
		}

		@event.Dispose();
	}
}
