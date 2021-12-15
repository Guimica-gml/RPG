using Godot;
using Godot.Collections;

public class Manager : CanvasLayer
{
	[Signal] private delegate void GamePaused();
	[Signal] private delegate void GameUnpaused();
	
	[Export] public int GridSize = 16;
	
	private PauseDisplayer _pauseDisplayer;
	private Dictionary<string, Array> _sceneInfo = new Dictionary<string, Array>();
	
	private PackedScene _playerPreload = GD.Load<PackedScene>("res://Player/Player.tscn");
	private PackedScene _pauseDisplayerPreload = GD.Load<PackedScene>("res://PauseDisplayer/InventoryPauseDisplayer/InventoryPauseDisplayer.tscn");
	
	public async override void _Ready()
	{
		await ToSignal(GetTree(), "idle_frame");
		
		Global.TransitionManager.Connect("TransitionTriggered", this, nameof(OnTransitionTriggered));
		Global.TransitionManager.Connect("SceneChanged", this, nameof(OnSceneChanged));
	}
	
	public override void _Input(InputEvent @event)
	{
		if (Global.InteractionManager.Interaction != null || !@event.IsActionPressed("ui_pause")) return;
		
		var pauseInteraction = _pauseDisplayerPreload.Instance<PauseDisplayer>();
		PauseGame(pauseInteraction);
	}
	
	public void PauseGame(PauseDisplayer pauseDisplayer)
	{
		if (GetTree().Paused || InMenu()) return;
		
		_pauseDisplayer = pauseDisplayer;
		_pauseDisplayer.Connect("Close", this, "UnpauseGame");
		AddChild(_pauseDisplayer);
		
		GetTree().Paused = true;
		EmitSignal(nameof(GamePaused));
	}
	
	private void UnpauseGame()
	{
		_pauseDisplayer.QueueFree();
		_pauseDisplayer = null;
		
		GetTree().SetDeferred("paused", false);
		EmitSignal(nameof(GameUnpaused));
	}
	
	public bool InMenu()
	{
		var menuNodes = GetTree().GetNodesInGroup("Menu");
		return (menuNodes.Count > 0);
	}
	
	public MainCamera GetMainCamera()
	{
		return GetTree().CurrentScene.FindNode("MainCamera") as MainCamera;
	}
	
	public YSort GetYSort()
	{
		return GetTree().CurrentScene.FindNode("YSort") as YSort;
	}
	
	public TileMap GetCollisionTileMap()
	{
		return GetTree().CurrentScene.FindNode("CollisionTileMap") as TileMap;
	}
	
	public void SaveScene()
	{
		var sceneName = GetTree().CurrentScene.Name;
		var nodesToSave = new Array<Node>(GetTree().GetNodesInGroup("Save"));
		var saveList = new Array();
		
		foreach (Node2D node in nodesToSave)
		{
			var saveDict = new Dictionary();
			if (node.HasMethod("Save")) saveDict = node.Call("Save") as Dictionary;
			
			saveDict["ParentName"] = node.GetParent().Name;
			saveDict["Filename"] = node.Filename;
			saveDict["x"] = ((int) node.GlobalPosition.x / GridSize) * GridSize;
			saveDict["y"] = ((int) node.GlobalPosition.y / GridSize) * GridSize;
			
			saveList.Add(saveDict);
		}
		
		_sceneInfo[sceneName] = saveList;
	}
	
	public void LoadScene()
	{
		var sceneName = GetTree().CurrentScene.Name;
		if (!_sceneInfo.ContainsKey(sceneName)) return;
		
		var nodesToDestroy = new Array<Node>(GetTree().GetNodesInGroup("Save"));
		var nodesSaved = new Array<Dictionary>(_sceneInfo[sceneName]);
		
		foreach (var node in nodesToDestroy)
		{
			node.Name = "Deleted";
			node.QueueFree();
		}
		
		foreach (var nodeInfo in nodesSaved)
		{
			var nodePacked = GD.Load<PackedScene>(nodeInfo["Filename"] as string);
			var node = nodePacked.Instance<Node2D>();
			var parent = GetTree().CurrentScene.FindNode(nodeInfo["ParentName"] as string);
			
			node.GlobalPosition = new Vector2((int) nodeInfo["x"], (int) nodeInfo["y"]);
			
			if (node.HasMethod("Load")) node.Call("Load", nodeInfo);
			parent.AddChild(node, true);
		}
	}
	
	private void OnTransitionTriggered()
	{
		SaveScene();
	}
	
	private void OnSceneChanged()
	{
		LoadScene();
	}
}
