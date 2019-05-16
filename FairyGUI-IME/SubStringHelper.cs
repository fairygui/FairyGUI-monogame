using System.Text;
using System.Text.RegularExpressions;

namespace FairyGUI_IME
{
    public static class SubStringHelper
    {
        /// <summary>
        /// 用于载取中文字符串
        /// </summary>
        /// <param name="stringToBackSpace"></param>
        /// <returns></returns>
        public static string BackSpaceString(string stringToBackSpace)
        {
            Regex regex = new Regex("[\u4e00-\u9fa5]+", RegexOptions.Compiled);
            char[] stringChar = stringToBackSpace.ToCharArray();
            StringBuilder sb = new StringBuilder();
            int nLength = 0;
            for (int i = 0; i < stringChar.Length - 1; i++)
            {
                if (regex.IsMatch((stringChar[i]).ToString()))
                {
                    sb.Append(stringChar[i]);
                    nLength += 2;
                }
                else
                {
                    sb.Append(stringChar[i]);
                    nLength = nLength + 1;
                }
            }
            return sb.ToString();
        }
    }
}