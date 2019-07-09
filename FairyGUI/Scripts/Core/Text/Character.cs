namespace FairyGUI.Scripts.Core.Text
{
    public class Character : ICharacter
    {
        public bool IsUsed { get; set; }

        public int CharacterType { get; set; }

        public char Chars { get; set; }
    }

    public enum CharacterType
    {
        Char = 0,
        BackSpace = 8,
        Tab = 9,
        Enter = 13,
        Esc = 27,
    }
}