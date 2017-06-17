using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Web.Mvc;

namespace RaceDay
{
	public static class EnumExtensions
	{
		/// <summary>
		/// retrieves enum text from Description attribute if enum is decorated as such otherwise just calls ToString() 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerationValue"></param>
		/// <returns></returns>
		public static string GetEnumDescription<T>(this object enumerationValue)
			where T : struct
		{
			Type type = enumerationValue.GetType();

			if (!type.IsEnum)
			{
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
			}

			//Tries to find a DescriptionAttribute for a potential friendly name
			//for the enum
			MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
			if (memberInfo != null && memberInfo.Length > 0)
			{
				object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attrs != null && attrs.Length > 0)
				{
					//Pull out the description value
					return ((DescriptionAttribute)attrs[0]).Description;
				}
			}

			//If we have no description attribute, just return the ToString of the enum
			return enumerationValue.ToString();
		}

		/// <summary>
		/// takes a value, converts it to an enum and returns that enum's description.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string GetEnumDescriptionByValue<T>(this object value)
			where T : struct
		{
			Type enumType = typeof(T);

			if (!enumType.IsEnum)
			{
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
			}

			T enumVal = EnumParse<T>(value);

			//Tries to find a DescriptionAttribute for a potential friendly name
			//for the enum
			MemberInfo[] memberInfo = enumType.GetMember(enumVal.ToString());
			if (memberInfo != null && memberInfo.Length > 0)
			{
				object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attrs != null && attrs.Length > 0)
				{
					//Pull out the description value
					return ((DescriptionAttribute)attrs[0]).Description;
				}
			}

			//If we have no description attribute, just return the ToString of the enum
			return enumVal.ToString();
		}

		public static string GetDescription(Type enumType, object value)
		{
			MemberInfo[] memberInfo = enumType.GetMember(value.ToString());
			if (memberInfo != null && memberInfo.Length > 0)
			{
				object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attrs != null && attrs.Length > 0)
					return ((DescriptionAttribute)attrs[0]).Description;
			}

			return Enum.GetName(enumType, value);
		}

		/// <summary>
		/// generic for Enum.Parse
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <returns></returns>
		public static T EnumParse<T>(this object value) where T : struct
		{
			if (value == DBNull.Value || value == null)
				throw new ArgumentNullException("value");

			Type type = typeof(T);

			if (!type.IsEnum)
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");

			T enumType = (T)Enum.Parse(type, value.ToString(), true);

			return enumType;
		}

		public static List<T> ToList<T>() where T : struct
		{
			Type type = typeof(T);

			if (!type.IsEnum)
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");

			Array values = Enum.GetValues(type);
			List<T> enumValList = new List<T>(values.Length);

			foreach (int val in values)
				enumValList.Add(EnumParse<T>(val));

			return enumValList;
		}

		public static List<T> ToList<T>(int minValue) where T : struct
		{
			Type type = typeof(T);

			if (!type.IsEnum)
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");

			Array values = Enum.GetValues(type);
			List<T> enumValList = new List<T>(values.Length);

			foreach (int val in values)
			{
				if (val >= minValue)
					enumValList.Add(EnumParse<T>(val));
			}

			return enumValList;
		}

		public static List<SelectListItem> ToSelectList<T>(Int32? selected, Boolean includeEmpty = true) where T : struct
		{
			Type type = typeof(T);
			if (!type.IsEnum)
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");

			Array values = Enum.GetValues(type);
			List<SelectListItem> enumSelectList = new List<SelectListItem>(values.Length);

			foreach (int val in values)
			{
				SelectListItem item = new SelectListItem();
				item.Value = val.ToString();
				item.Text = GetEnumDescriptionByValue<T>(val);
				item.Selected = (selected.HasValue && (selected.Value == val) ? true : false);

				enumSelectList.Add(item);
			}

			if ((!selected.HasValue) && includeEmpty)
			{
				SelectListItem item = new SelectListItem();
				item.Value = "";
				item.Text = "-- No Default Selected --";
				item.Selected = true;
				enumSelectList.Insert(0, item);
			}

			return enumSelectList;
		}

		public static bool IsDefined<T>(this object value) where T : struct
		{
			return Enum.IsDefined(typeof(T), value);
		}

		public static TAttribute GetAttribute<TAttribute>(this Enum value, TAttribute defaultValue)
			where TAttribute : Attribute
		{
			var type = value.GetType();
			var name = Enum.GetName(type, value);
			var retValue = type.GetField(name)
				.GetCustomAttributes(false)
				.OfType<TAttribute>()
				.SingleOrDefault();

			return (retValue == null ? defaultValue : retValue);
		}
	}
}
