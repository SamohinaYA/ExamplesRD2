using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Lib.Utilities.DBXSettingsAdapter;

namespace WSSC.V4.DMS.TMK.EDMS.Utils.RestoreZinstructions
{
	internal class Settings
	{
		internal Settings() { }

		private bool __init_Xml = false;
		private XmlDocument _Xml;
		/// <summary>
		/// Настройка Xml
		/// </summary>
		private XmlDocument Xml
		{
			get
			{
				if (!__init_Xml)
				{
					_Xml = new XmlDocument();
					_Xml.Load(@"Settings.xml");
					__init_Xml = true;
				}
				return _Xml;
			}
		}



		private bool __init_Adapter = false;
		private DBXSettingsAdapter _Adapter;
		/// <summary>
		/// Адаптер настроек
		/// </summary>
		internal DBXSettingsAdapter Adapter
		{
			get
			{
				if (!__init_Adapter)
				{
					_Adapter = new DBXSettingsAdapter(this.Xml);
					__init_Adapter = true;
				}
				return _Adapter;
			}
		}


		private bool __init_List = false;
		private DBXList _List;
		/// <summary>
		/// Список DBX
		/// </summary>
		internal DBXList List
		{
			get
			{
				if (!__init_List)
				{
					_List = this.Adapter.Lists?.FirstOrDefault() ?? throw new Exception($"Не удалось настройки для спизка из адаптера настроек");
					__init_List = true;
				}
				return _List;
			}

		}


		private bool __init_RestoreSettings = false;
		private RestoreSettings _RestoreSettings;
		/// <summary>
		/// Настройка выборки
		/// </summary>
		internal RestoreSettings RestoreSettings
		{
			get
			{
				if (!__init_RestoreSettings)
				{
					XmlNode node = this.List.Node.SelectSingleNode("RestoreSettings") ?? throw new Exception($"Не удалось получить xml-узел 'RestoreSettings'");
					_RestoreSettings = new RestoreSettings(node);
					__init_RestoreSettings = true;
				}
				return _RestoreSettings;
			}

		}




	}
}
