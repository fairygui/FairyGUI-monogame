namespace FairyGUI.Test.Scenes
{
    public class JoystickScene : DemoScene
    {
        GComponent _mainView;
        GTextField _text;
        JoystickModule _joystick;

        public JoystickScene()
        {
            UIPackage.AddPackage("UI/Joystick");

            _mainView = UIPackage.CreateObject("Joystick", "Main").asCom;
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            AddChild(_mainView);

            _text = _mainView.GetChild("n9").asTextField;

            _joystick = new JoystickModule(_mainView);
            _joystick.onMove.Add(__joystickMove);
            _joystick.onEnd.Add(__joystickEnd);
        }

        void __joystickMove(EventContext context)
        {
            float degree = (float)context.data;
            _text.text = "" + degree;
        }

        void __joystickEnd()
        {
            _text.text = "";
        }
    }
}