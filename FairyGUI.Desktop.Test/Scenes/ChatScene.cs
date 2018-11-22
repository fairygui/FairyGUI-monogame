using System;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI.Test.Scenes
{
	public class ChatScene : DemoScene
	{
		GComponent _mainView;
		GList _list;
		GTextInput _input;
		GComponent _emojiSelectUI;

		class Message
		{
			public string sender;
			public string senderIcon;
			public string msg;
			public bool fromMe;
		}
		List<Message> _messages;

		public ChatScene()
		{
			UIPackage.AddPackage("UI/Emoji");

			UIConfig.verticalScrollBar = "ui://Emoji/ScrollBar_VT";
			UIConfig.defaultScrollBarDisplay = ScrollBarDisplayType.Auto;

			_messages = new List<Message>();

			_mainView = UIPackage.CreateObject("Emoji", "Main").asCom;
			_mainView.MakeFullScreen();
			_mainView.AddRelation(GRoot.inst, RelationType.Size);
			AddChild(_mainView);

			_list = _mainView.GetChild("list").asList;
			_list.SetVirtual();
			_list.itemProvider = GetListItemResource;
			_list.itemRenderer = RenderListItem;

			_input = _mainView.GetChild("input").asTextInput;
			_input.onSubmit.Add(onSubmit);

			_mainView.GetChild("btnSend").onClick.Add(__clickSendBtn);
			_mainView.GetChild("btnEmoji").onClick.Add(__clickEmojiBtn);

			_emojiSelectUI = UIPackage.CreateObject("Emoji", "EmojiSelectUI").asCom;
			_emojiSelectUI.GetChild("list").asList.onClickItem.Add(__clickEmoji);
		}


		void AddMsg(string sender, string senderIcon, string msg, bool fromMe)
		{
			bool isScrollBottom = _list.scrollPane.isBottomMost;

			Message newMessage = new Message();
			newMessage.sender = sender;
			newMessage.senderIcon = senderIcon;
			newMessage.msg = msg;
			newMessage.fromMe = fromMe;
			_messages.Add(newMessage);

			if (newMessage.fromMe)
			{
				if (_messages.Count == 1 || ToolSet.Random(0f, 1f) < 0.5f)
				{
					Message replyMessage = new Message();
					replyMessage.sender = "FairyGUI";
					replyMessage.senderIcon = "r1";
					replyMessage.msg = "Today is a good day. [:cool]";
					replyMessage.fromMe = false;
					_messages.Add(replyMessage);
				}
			}

			if (_messages.Count > 100)
				_messages.RemoveRange(0, _messages.Count - 100);

			_list.numItems = _messages.Count;

			if (isScrollBottom)
				_list.scrollPane.ScrollBottom();
		}

		string GetListItemResource(int index)
		{
			Message msg = _messages[index];
			if (msg.fromMe)
				return "ui://Emoji/chatRight";
			else
				return "ui://Emoji/chatLeft";
		}

		void RenderListItem(int index, GObject obj)
		{
			GButton item = (GButton)obj;
			Message msg = _messages[index];
			if (!msg.fromMe)
				item.GetChild("name").text = msg.sender;
			item.icon = UIPackage.GetItemURL("Emoji", msg.senderIcon);

			//Recaculate the text width
			GRichTextField tf = item.GetChild("msg").asRichTextField;
			tf.width = tf.initWidth;
			tf.text = EmojiParser.inst.Parse(msg.msg);
			tf.width = tf.textWidth;
		}

		void __clickSendBtn(EventContext context)
		{
			string msg = _input.text;
			if (msg.Length == 0)
				return;

			AddMsg("Unity", "r0", msg, true);
			_input.text = "";
		}

		void __clickEmojiBtn(EventContext context)
		{
			GRoot.inst.ShowPopup(_emojiSelectUI, (GObject)context.sender, false);
		}

		void __clickEmoji(EventContext context)
		{
			GButton item = (GButton)context.data;
			_input.ReplaceSelection("[:" + item.text + "]");
		}

		void onSubmit(EventContext context)
		{
			_mainView.GetChild("btnSend").onClick.Call();
		}
	}
}