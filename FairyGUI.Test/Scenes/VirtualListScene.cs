namespace FairyGUI.Test.Scenes
{
    public class VirtualListScene : DemoScene
    {
        GComponent _mainView;
        GList _list;

        public VirtualListScene()
        {
            UIPackage.AddPackage("UI/VirtualList");
            UIObjectFactory.SetPackageItemExtension("ui://VirtualList/mailItem", typeof(MailItem));

            _mainView = UIPackage.CreateObject("VirtualList", "Main").asCom;
            _mainView.GetChild("n6").onClick.Add(() => { _list.AddSelection(500, true); });
            _mainView.GetChild("n7").onClick.Add(() => { _list.scrollPane.ScrollTop(); });
            _mainView.GetChild("n8").onClick.Add(() => { _list.scrollPane.ScrollBottom(); });
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            AddChild(_mainView);

            _list = _mainView.GetChild("mailList").asList;
            _list.SetVirtual();

            _list.itemRenderer = RenderListItem;
            _list.numItems = 1000;
        }

        void RenderListItem(int index, GObject obj)
        {
            MailItem item = (MailItem)obj;
            item.setFetched(index % 3 == 0);
            item.setRead(index % 2 == 0);
            item.setTime("5 Nov 2015 16:24:33");
            item.title = index + " Mail title here";
        }
    }
}