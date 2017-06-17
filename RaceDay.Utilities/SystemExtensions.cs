using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RaceDay.Utilities
{
	/// <summary>
	/// SystemExtensions
	/// 
	/// Adds extension methods to System classes
	/// </summary>
	/// 
	public static class SystemExtensions
	{
		/// <summary>
		/// ToTimeString
		/// 
		/// Converts the value of the TimeSpan object this extends to a formatted string
		/// of the format [hh:]mm:ss
		/// </summary>
		/// <param name="ts"></param>
		/// <returns></returns>
		/// 
		public static String ToTimeString(this TimeSpan ts)
		{
			if (ts.Hours > 0)
				return ts.ToString(@"h\.mm\.ss");

			return ts.ToString(@"m\.ss");
		}

		/// <summary>
		/// ParseToSeconds
		/// 
		/// Parses a time string of the format [hh:]mm:ss or [hh.]mm.ss to Seconds.
		/// </summary>
		/// <param name="ts"></param>
		/// <param name="s"></param>
		/// <returns></returns>
		public static Int32 ParseToSeconds(String s)
		{
			s = s.Replace('.', ':');
			if (s.Length < 5)
				s = String.Concat("00:0", s);
			else if (s.Length < 6)
				s = String.Concat("00:", s);
			TimeSpan t = new TimeSpan();
			if (TimeSpan.TryParse(s, out t))
				return (Int32)t.TotalSeconds;

			return 0;
		}

		/// <summary>
		/// WebBase64
		/// 
		/// String extension method to cleanup base 64 encoded string.
		/// Need to pad with '=' to make divisable of 4
		/// Replace '-' with '+' and '_' with '/'
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static String WebBase64(this String value)
		{
			String s = value;
			while ((s.Length % 4) != 0)
				s += "=";
			return s.Replace('-', '+').Replace('_', '/');
		}

		/// <summary>
		/// RemoveHtml
		/// 
		/// Removes the HTML formatting from a String leaving only plain text
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// 
		/// <summary>
		/// Compiled regular expression for performance.
		/// </summary>
		static Regex _htmlRegex = new Regex("<.*?>", RegexOptions.Compiled);

		public static String RemoveHtml(this String value)
		{
			String strippedValue = _htmlRegex.Replace(value, String.Empty);
			strippedValue = strippedValue.Replace("&nbsp;", " ");
			strippedValue = strippedValue.Replace("\r", "");
			strippedValue = strippedValue.Replace("\n", " ");

			return strippedValue;
		}

		/// <summary>
		/// Left
		/// 
		/// Returns the left substring of the given string.  Checks the length of the string to make sure it is as least as long as the 
		/// requested number of characters.
		/// </summary>
		/// 
		public static String Left(this String value, int length)
		{
			if (value.Length < length)
				return value;
			else
				return value.Substring(0, length);
		}

		/// <summary>
		/// LeftEllipses
		/// 
		/// Returns the left substring of the given string.  Checks the length of the string to make sure it is as least as long as the 
		/// requested number of characters.  Adds Ellipses if the string is truuncated
		/// </summary>
		/// 
		public static String LeftEllipses(this String value, int length)
		{
			if (value.Length < length)
				return value;
			else
				return String.Concat( value.Substring(0, length), "...");
		}

		public static String LeftEllipses(this String value, int length, String postfix)
		{
			if (value.Length < length)
				return value;
			else
				return String.Concat(value.Substring(0, length), "...", postfix);
		}
	}
}
