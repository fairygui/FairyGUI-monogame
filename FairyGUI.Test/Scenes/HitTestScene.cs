namespace FairyGUI.Test.Scenes
{
    public class HitTestScene : DemoScene
    {
        GComponent _mainView;

        public HitTestScene()
        {
            UIPackage.AddPackage("UI/HitTest");

            _mainView = UIPackage.CreateObject("HitTest", "Main").asCom;
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            AddChild(_mainView);
        }
    }
}