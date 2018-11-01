namespace FairyGUI.Test.Scenes
{
    public class ScrollPaneScene : DemoScene
    {
        GComponent _mainView;
        GList _list;

        public ScrollPaneScene()
        {
            UIPackage.AddPackage("UI/ScrollPane");

            _mainView = UIPackage.CreateObject("ScrollPane", "Main").asCom;
            _mainView.MakeFullScreen();
            _mainView.AddRelation(GRoot.inst, RelationType.Size);
            AddChild(_mainView);

            _list = _mainView.GetChild("list").asList;
            _list.itemRenderer = RenderListItem;
            _list.SetVirtual();
            _list.numItems = 1000;
            _list.onTouchBegin.Add(OnClickList);

            _mainView.GetChild("box").asCom.onDrop.Add(OnDrop);
        }

        void RenderListItem(int index, GObject obj)
        {
            GButton item = obj.asButton;
            item.title = "Item " + index;
            item.scrollPane.posX = 0; //reset scroll pos

            //Be carefull, RenderListItem is calling repeatedly, dont call 'Add' here!
            //请注意，RenderListItem是重复调用的，不要使用Add增加侦听！
            item.GetChild("b0").onClick.Set(OnClickStick);
            item.GetChild("b1").onClick.Set(OnClickDelete);
        }

        void OnClickList(EventContext context)
        {
            //find out if there is an item in edit status
            //查找是否有项目处于编辑状态
            int cnt = _list.numChildren;
            for (int i = 0; i < cnt; i++)
            {
                GButton item = _list.GetChildAt(i).asButton;
                if (item.scrollPane.posX != 0)
                {
                    //Check if clicked on the button
                    if (item.GetChild("b0").asButton.IsAncestorOf(GRoot.inst.touchTarget)
                        || item.GetChild("b1").asButton.IsAncestorOf(GRoot.inst.touchTarget))
                    {
                        return;
                    }
                    item.scrollPane.SetPosX(0, true);
                    //avoid scroll pane default behavior
                    //取消滚动面板可能发生的拉动。
                    item.scrollPane.CancelDragging();
                    _list.scrollPane.CancelDragging();
                    break;
                }
            }
        }

        void OnDrop(EventContext context)
        {
            _mainView.GetChild("txt").text = "Drop " + (string)context.data;
        }

        void OnClickStick(EventContext context)
        {
            _mainView.GetChild("txt").text = "Stick " + (((GObject)context.sender).parent).text;
        }

        void OnClickDelete(EventContext context)
        {
            _mainView.GetChild("txt").text = "Delete " + (((GObject)context.sender).parent).text;
        }
    }
}