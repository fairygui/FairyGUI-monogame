namespace FairyGUI.Test.Scenes
{
    public class BagScene : DemoScene
    {
        GComponent _mainView;
        BagWindow _bagWindow;

        public BagScene()
        {
            UIPackage.AddPackage("UI/Bag");
            UIObjectFactory.SetLoaderExtension(typeof(MyGLoader));

            _mainView = UIPackage.CreateObject("Bag", "Main").asCom;
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            GRoot.inst.AddChild(_mainView);

            _bagWindow = new BagWindow();
            _mainView.GetChild("bagBtn").onClick.Add(() => { _bagWindow.Show(); });
        }
    }
}