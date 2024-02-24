using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    public class StringComparer
    {
        public static (int place,string dif) CompareStringsIgnoringCharacters(string str1, string str2, char[]? ignoredCharacters = null)
        {
            if (ignoredCharacters == null) { ignoredCharacters = new char[] { '\r', '\n' ,' ','\\','"','\'','}', '{' }; }

            // Удаляем игнорируемые символы
            string formattedStr1 = RemoveIgnoredCharacters(str1, ignoredCharacters);
            string formattedStr2 = RemoveIgnoredCharacters(str2, ignoredCharacters);
            int mismatchIndex;
            // Сравниваем строки без учета регистра символов
            if (!CompareStringsWithCaseInsensitive(formattedStr1, formattedStr2, out mismatchIndex, out  string dif)){
                return (mismatchIndex,dif);
            }
            return (-1,null);

        }

        private static string RemoveIgnoredCharacters(string input, char[] ignoredCharacters)
        {
            // Удаляем игнорируемые символы
            foreach (char ignoredChar in ignoredCharacters)
            {
                input = input.Replace(ignoredChar.ToString(), "");
            }

            return input;
        }

        private static bool CompareStringsWithCaseInsensitive(string str1, string str2, out int mismatchIndex,out string dif)
        {
            mismatchIndex = -1;
            dif = string.Empty;
            // Определяем минимальную длину для предотвращения выхода за пределы массива
            int minLength = Math.Min(str1.Length, str2.Length);

            for (int i = 0; i < minLength; i++)
            {
                if (char.ToLowerInvariant(str1[i]) != char.ToLowerInvariant(str2[i]))
                {
					int length = Math.Min(10, Math.Min(str1.Length - i, str2.Length - i));
					string substring1 = str1.Substring(i, length);
					string substring2 = str2.Substring(i, length);

					dif = $"{substring1} <=> {substring2}   {str1[i]}!={str2[i]}  ";
					mismatchIndex = i;
                    return false;
                }
            }

            // Если все символы до минимальной длины совпали, но длины строк различны
            if (str1.Length != str2.Length)
            {
                mismatchIndex = minLength;
            }

            return true;
        }

       
    }


public static	class StringComparerWithDiff
	{
		public static (int place, string dif) Compare(string str1, string str2)
		{
            int mismatchIndex = -1; 
            string dif = string.Empty;
			var ignoredCharacters = new char[] { '\r', '\n', ' ', '\\', '"', '\'', '}', '{' };
			mismatchIndex = -1;
			dif = string.Empty;
			string formattedStr1 = RemoveIgnoredCharacters(str1, ignoredCharacters);
			string formattedStr2 = RemoveIgnoredCharacters(str2, ignoredCharacters);
			int minLength = Math.Min(formattedStr1.Length, formattedStr2.Length);

			for (int i = 0, i2 = 0 ; i < minLength; i++,i2 ++ )
			{
                var ch1 = str1[i];
                var ch2 = str2[i];
                while (ignoredCharacters.Contains(ch1)){
                    i++;
                  ch1 = str1[i];
                }
                while (ignoredCharacters.Contains(ch2)){
                    i2++;
                  ch2 = str2[i2];
                }
				if (char.ToLowerInvariant(ch1) != char.ToLowerInvariant(ch2))
				{
					mismatchIndex = i;
					int length = Math.Min(10, Math.Min(str1.Length - i, str2.Length - i));
					dif = $"{str1.Substring(i, length)} != {str2.Substring(i, length)}";
					
				}
			}

			if (str1.Length != str2.Length)
			{
				mismatchIndex = minLength;
				int length = Math.Min(10, Math.Min(str1.Length - minLength, str2.Length - minLength));
				dif = $"{str1.Substring(minLength, length)} != {str2.Substring(minLength, length)}";
                return (mismatchIndex,dif);
			}

			return  (mismatchIndex, dif);
		}


		private static string RemoveIgnoredCharacters(string input, char[] ignoredCharacters)
		{
			// Удаляем игнорируемые символы
			foreach (char ignoredChar in ignoredCharacters)
			{
				input = input.Replace(ignoredChar.ToString(), "");
			}

			return input;
		}

	}
}
