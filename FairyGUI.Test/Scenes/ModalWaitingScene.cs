namespace FairyGUI.Test.Scenes
{
    public class ModalWaitingScene : DemoScene
    {
        GComponent _mainView;
        Window _testWin;

        public ModalWaitingScene()
        {
            UIPackage.AddPackage("UI/ModalWaiting");
            UIConfig.globalModalWaiting = "ui://ModalWaiting/GlobalModalWaiting";
            UIConfig.windowModalWaiting = "ui://ModalWaiting/WindowModalWaiting";

            _mainView = UIPackage.CreateObject("ModalWaiting", "Main").asCom;
            _mainView.GetChild("n0").onClick.Add(() => { _testWin.Show(); });
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            AddChild(_mainView);

            _testWin = new Window();
            _testWin.contentPane = UIPackage.CreateObject("ModalWaiting", "TestWin").asCom;
            _testWin.contentPane.GetChild("n1").onClick.Add(() =>
            {
                _testWin.ShowModalWait();
                Timers.inst.Add(3, 1, (object param) => { _testWin.CloseModalWait(); });
            });

            GRoot.inst.ShowModalWait();
            Timers.inst.Add(3, 1, (object param) => { GRoot.inst.CloseModalWait(); });
        }
    }
}