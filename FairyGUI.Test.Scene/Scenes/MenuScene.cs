namespace FairyGUI.Test.Scenes
{
	public class MenuScene : DemoScene
	{
		GComponent _mainView;

		public MenuScene()
		{
			UIPackage.AddPackage("UI/MainMenu");

			_mainView = UIPackage.CreateObject("MainMenu", "Main").asCom;
			_mainView.MakeFullScreen();
			_mainView.AddRelation(GRoot.inst, RelationType.Size);
			AddChild(_mainView);

			_mainView.GetChild("n1").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new BasicsScene());
			});
			_mainView.GetChild("n2").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new TransitionDemoScene());
			});
			_mainView.GetChild("n4").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new VirtualListScene());
			});
			_mainView.GetChild("n5").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new LoopListScene());
			});
			_mainView.GetChild("n6").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new HitTestScene());
			});
			_mainView.GetChild("n7").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new PullToRefreshScene());
			});
			_mainView.GetChild("n8").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new ModalWaitingScene());
			});
			_mainView.GetChild("n9").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new JoystickScene());
			});
			_mainView.GetChild("n10").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new BagScene());
			});
			_mainView.GetChild("n11").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new ChatScene());
			});
			_mainView.GetChild("n12").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new ListEffectScene());
			});
			_mainView.GetChild("n13").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new ScrollPaneScene());
			});
			_mainView.GetChild("n14").onClick.Add(() =>
			{
				GRoot.inst.RemoveChildren(0, -1, true);
				GRoot.inst.AddChild(new TreeViewScene());
			});
		}
	}
}