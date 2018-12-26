#if Windows
using System;
using Microsoft.Xna.Framework;

namespace FairyGUI.Scripts.Core.Text
{
	/// <summary>
	/// 
	/// </summary>
	public class IMEHandler : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		public IMENativeWindow _nativeWnd;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="showDefaultImeWindow"></param>
		public IMEHandler(Game game, bool showDefaultImeWindow = false)
		{
			this.GameInstance = game;
			Input = new InputRemapper(game);
			_nativeWnd = new IMENativeWindow(game.Window.Handle, showDefaultImeWindow);
			_nativeWnd.onCandidatesReceived += (s, e) =>
			{
				if (onCandidatesReceived != null) onCandidatesReceived.Invoke(s, e);
			};
			_nativeWnd.onCompositionReceived += (s, e) =>
			{
				if (onCompositionReceived != null) onCompositionReceived.Invoke(s, e);
			};
			_nativeWnd.onResultReceived += (s, e) =>
			{
				if (onResultReceived != null) onResultReceived.Invoke(s, e);
			};
			_nativeWnd.onStartCompositionReceived += (s, e) =>
			{
				if (onStartCompositionReceived != null) onStartCompositionReceived.Invoke(s, e);
			};
			_nativeWnd.onEndCompositionReceived += (s, e) =>
			{
				if (onEndCompositionReceived != null) onEndCompositionReceived.Invoke(s, e);
			};
			game.Exiting += (o, e) => this.Dispose();
		}

		/// <summary>
		/// Corrected mouse / keyboard state
		/// </summary>
		public InputRemapper Input { get; private set; }

		/// <summary>
		/// Called when the candidates updated
		/// </summary>
		public event EventHandler onCandidatesReceived;

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler onStartCompositionReceived;

		/// <summary>
		/// 
		/// </summary>
		public event EventHandler<IMEResultEventArgs> onEndCompositionReceived;

		/// <summary>
		/// Called when the composition updated
		/// </summary>
		public event EventHandler onCompositionReceived;

		/// <summary>
		/// Called when a new result character is coming
		/// </summary>
		public event EventHandler<IMEResultEventArgs> onResultReceived;

		/// <summary>
		/// Array of the candidates
		/// </summary>
		public string[] Candidates { get { return _nativeWnd.Candidates; } }

		/// <summary>
		/// How many candidates should display per page
		/// </summary>
		public uint CandidatesPageSize { get { return _nativeWnd.CandidatesPageSize; } }

		/// <summary>
		/// First candidate index of current page
		/// </summary>
		public uint CandidatesPageStart { get { return _nativeWnd.CandidatesPageStart; } }

		/// <summary>
		/// The selected canddiate index
		/// </summary>
		public uint CandidatesSelection { get { return _nativeWnd.CandidatesSelection; } }

		/// <summary>
		/// Composition String
		/// </summary>
		public string Composition { get { return _nativeWnd.CompositionString; } }

		/// <summary>
		/// Composition Clause
		/// </summary>
		public string CompositionClause { get { return _nativeWnd.CompositionClause; } }

		/// <summary>
		/// Composition Reading String
		/// </summary>
		public string CompositionRead { get { return _nativeWnd.CompositionReadString; } }

		/// <summary>
		/// Composition Reading Clause
		/// </summary>
		public string CompositionReadClause { get { return _nativeWnd.CompositionReadClause; } }

		/// <summary>
		/// Caret position of the composition
		/// </summary>
		public int CompositionCursorPos { get { return _nativeWnd.CompositionCursorPos; } }

		/// <summary>
		/// Result String
		/// </summary>
		public string Result { get { return _nativeWnd.ResultString; } }

		/// <summary>
		/// Result Clause
		/// </summary>
		public string ResultClause { get { return _nativeWnd.ResultClause; } }

		/// <summary>
		/// Result Reading String
		/// </summary>
		public string ResultRead { get { return _nativeWnd.ResultReadString; } }

		/// <summary>
		/// Result Reading Clause
		/// </summary>
		public string ResultReadClause { get { return _nativeWnd.ResultReadClause; } }

		/// <summary>
		/// Enable / Disable IME
		/// </summary>
		public bool Enabled
		{
			get
			{
				return _nativeWnd.IsEnabled;
			}
			set
			{
				if (value)
					_nativeWnd.enableIME();
				else
					_nativeWnd.disableIME();
			}
		}

		/// <summary>
		/// Game Instance
		/// </summary>
		public Game GameInstance { get; private set; }

		/// <summary>
		/// Get the composition attribute at character index.
		/// </summary>
		/// <param name="index">Character Index</param>
		/// <returns>Composition Attribute</returns>
		public CompositionAttributes GetCompositionAttr(int index)
		{
			return _nativeWnd.GetCompositionAttr(index);
		}

		/// <summary>
		/// Get the composition read attribute at character index.
		/// </summary>
		/// <param name="index">Character Index</param>
		/// <returns>Composition Attribute</returns>
		public CompositionAttributes GetCompositionReadAttr(int index)
		{
			return _nativeWnd.GetCompositionReadAttr(index);
		}

		/// <summary>
		/// 
		/// </summary>
		public void OpenIme()
		{
			_nativeWnd.OpenIME();
		}

		/// <summary>
		/// Dispose everything
		/// </summary>
		public void Dispose()
		{
			_nativeWnd.Dispose();
		}
	}
}
#endif