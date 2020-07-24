
namespace T.Common
{
    public static class CharExtensions
    {
        public static char GetChar(this string value)
        {
            return value.GetChar(0);
        }

        public static char GetChar(this string value, int index)
        {
            if (value.IsNullOrEmpty())
                return default(char);

            if (index < 0 || (index > value.Length - 1))
                return default(char);

            return value[index];
        }

        public static char[] GetSpecialChars()
        {
            char[] chars = new[] { '♦', '♣', '♠', '•', '◘', '○', '◙', '♂', '♀', '♪', '♫', '☼', '►', '◄', '↕', '‼', '¶', '§', '_', '↨', '↑', '↓', '→', '←', '∟', '↔', '▲', '▼', '?', '!', '"', '#', '$', '%', '&', '(', ')', '*', '+', ',', '-', '/', '\\', '@' };

            return chars;
        }
    }
}
