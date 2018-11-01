namespace FairyGUI.Test.Scenes
{
    public class DemoScene : GComponent 
    {
        public DemoScene()
        {
            UIPackage.AddPackage("UI/MainMenu");


            UIPackage.AddPackage("UI/Basics");

            GObject closeButton = UIPackage.CreateObject("MainMenu", "CloseButton");
            closeButton.SetPosition(GRoot.inst.width - closeButton.width - 10, GRoot.inst.height - closeButton.height - 10);
            closeButton.AddRelation(GRoot.inst, RelationType.Right_Right);
            closeButton.AddRelation(GRoot.inst, RelationType.Bottom_Bottom);
            closeButton.sortingOrder = 100000;
            closeButton.onClick.Add(OnClose);

            GRoot.inst.AddChild(closeButton);
        }

        void OnClose()
        {
            if (this is MenuScene)
            {
                Stage.game.Exit();
            }
            else
            {
                GRoot.inst.RemoveChildren(0, -1, true);
                GRoot.inst.AddChild(new MenuScene());
            }
        }
    }
}