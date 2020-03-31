using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using WSSC.V4.SYS.Lib.Base;

namespace WSSC.V4.DMS.TMK.EDMS.Utils.RestoreZinstructions
{
	internal class RestoreSettings
	{
		XmlNode _Node;
		internal RestoreSettings(XmlNode node)
		{
			_Node = node;
		}

		private bool __init_DateAt = false;
		private DateTime _DateAt;
		/// <summary>
		/// Выбрать от "дата".
		/// </summary>
		internal DateTime DateAt
		{
			get
			{
				if (!__init_DateAt)
				{
					string value = XmlAttributeReader.GetValue(_Node, "DateAt");
					if (string.IsNullOrEmpty(value))
						throw new Exception($"Некорректная дата в артибуте 'DateAt' в настройке");

					if (!DateTime.TryParse(value, out _DateAt))
						throw new Exception($"Значение '{value}' не удалось преобразовать в дату");

					__init_DateAt = true;
				}
				return _DateAt;
			}

		}

		private bool __init_DateTo = false;
		private DateTime _DateTo;
		/// <summary>
		/// Выбрать от "дата". Может быть DateTime.Max, если дата не указана.
		/// </summary>
		internal DateTime DateTo
		{
			get
			{
				if (!__init_DateTo)
				{
					string value = XmlAttributeReader.GetValue(_Node, "DateTo");
					//Если не указано значение - макс дата
					if (string.IsNullOrEmpty(value))
					{
						_DateTo = DateTime.MaxValue;
					}
					else
					{
						if (!DateTime.TryParse(value, out _DateTo))
							throw new Exception($"Значение '{value}' не удалось преобразовать в дату");
					}

					__init_DateTo = true;
				}
				return _DateTo;
			}

		}

		private bool __init_Field = false;
		private string _Field;
		/// <summary>
		/// Название поля
		/// </summary>
		internal string Field
		{
			get
			{
				if (!__init_Field)
				{
					_Field = XmlAttributeReader.GetValue(_Node, "Field");
					if (string.IsNullOrEmpty(_Field))
						throw new Exception($"Не удалось получить значение поля из атрибута Field");

					__init_Field = true;
				}
				return _Field;
			}

		}

		private bool __init_WrongValue = false;
		private string _WrongValue;
		/// <summary>
		/// Значение которое не должно быть
		/// </summary>
		internal string WrongValue
		{
			get
			{
				if (!__init_WrongValue)
				{
					_WrongValue = XmlAttributeReader.GetValue(_Node, "WrongValue");
					if (string.IsNullOrEmpty(_WrongValue))
						throw new Exception($"Не удалось получить значение поля из атрибута WrongValue");

					__init_WrongValue = true;
				}
				return _WrongValue;
			}

		}

		private bool __init_IntegerValue = false;
		private bool _IntegerValue;
		/// <summary>
		/// Это значение цифра?
		/// </summary>
		internal bool IntegerValue
		{
			get
			{
				if (!__init_IntegerValue)
				{
					_IntegerValue = XmlAttributeReader.GetBooleanValue(_Node, "IntegerValue") && int.TryParse(this.WrongValue, out int value);
					__init_IntegerValue = true;
				}
				return _IntegerValue;
			}

		}
	}
}
