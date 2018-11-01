namespace FairyGUI.Test.Scenes
{
    public class ListEffectScene : DemoScene
    {
        GComponent _mainView;
        GList _list;

        public ListEffectScene()
        {
            UIPackage.AddPackage("UI/Extension");
            UIObjectFactory.SetPackageItemExtension("ui://Extension/mailItem", typeof(MailItem));

            _mainView = UIPackage.CreateObject("Extension", "Main").asCom;
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            AddChild(_mainView);

            _list = _mainView.GetChild("mailList").asList;
            for (int i = 0; i < 10; i++)
            {
                MailItem item = (MailItem)_list.AddItemFromPool();
                item.setFetched(i % 3 == 0);
                item.setRead(i % 2 == 0);
                item.setTime("5 Nov 2015 16:24:33");
                item.title = "Mail title here";
            }

            _list.EnsureBoundsCorrect();
            float delay = 0f;
            for (int i = 0; i < 10; i++)
            {
                MailItem item = (MailItem)_list.GetChildAt(i);
                if (_list.IsChildInView(item))
                {
                    item.PlayEffect(delay);
                    delay += 0.2f;
                }
                else
                    break;
            }
        }
    }
}