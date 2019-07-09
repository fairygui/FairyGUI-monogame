using System.Collections.Generic;

namespace FairyGUI
{
	public interface IInputHandler
	{
		List<ICharacter> myCharacters { get; set; }
	}

	public interface ICharacter
	{
		bool IsUsed { get; set; }

		int CharacterType { get; set; }

		char Chars { get; set; }
	}
}