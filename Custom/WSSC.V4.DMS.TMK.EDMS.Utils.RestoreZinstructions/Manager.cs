using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using WSSC.V4.DMS.Workflow;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Fields.Lookup;
using WSSC.V4.SYS.Lib.Data;

namespace WSSC.V4.DMS.TMK.EDMS.Utils.RestoreZinstructions
{
	internal class Manager
	{
		internal Manager() { }



		private bool __init_Loger = false;
		private StreamWriter _Loger;
		/// <summary>
		/// Логер в файл
		/// </summary>
		private StreamWriter Loger
		{
			get
			{
				if (!__init_Loger)
				{
					_Loger = new StreamWriter($"{this.GetType().FullName}.log", true);
					__init_Loger = true;
				}
				return _Loger;
			}

		}

		/// <summary>
		/// Записываем в лог инфу о найденых карточках. Пишем ИД карточки и её рег номер
		/// </summary>
		private void WriteLogInfo(DBItemCollection items, DBList list)
		{
			Console.WriteLine($"{Environment.NewLine}Найдено карточек: {items.Count}");

			this.Loger.WriteLine($"{Environment.NewLine}Дата {DateTime.Now.ToString()}  найдено карточек: {items.Count}{Environment.NewLine}Информация о найденых карточках:");

			if (items.Count == 0)
				return;

			//устанавливаем load type
			DBField field = list.GetField(Consts.Fields.RegNumber, true);
			field.ValueLoadingType = DBFieldValueIOType.Directly;

			//собираем всю инфу (ИД карточки и её рег номер) и потом записываем разом в лог
			StringBuilder itemsinfo = new StringBuilder();
			foreach (DBItem item in items)
			{
				itemsinfo.AppendLine($"ItemID = '{item.ID}' Регистрационный номер: '{item.GetStringValue(Consts.Fields.RegNumber)}'");
			}

			this.Loger.WriteLine(itemsinfo.ToString());
		}


		/// <summary>
		/// Находим все карточки которые необходимо восстановить
		/// </summary>
		private DBItemCollection FindItems(Settings settings)
		{
			//Получаем данные для восстановления
			DBList list = settings.List.List;

			DBConnection connection = list.Site.DataAdapter.Connection;
			string versionTable = list.TableInfo.VersionsTable.GetQueryName(connection, DBPartitionDataType.All);
			string mainTable = list.TableInfo.GetQueryName(connection, DBPartitionDataType.All);

			//настройки восстановления
			string field = settings.RestoreSettings.Field;
			string wrongValue = settings.RestoreSettings.WrongValue;
			bool itsInteger = settings.RestoreSettings.IntegerValue;
			DateTime dateAt = settings.RestoreSettings.DateAt;
			DateTime dateTo = settings.RestoreSettings.DateTo;

			//Получаем все карточки с полем field =  wrongValue в диапазоне дат [dateAt; dateTo]
			StringBuilder query = new StringBuilder();

			//Если это число - можно ускорить заброс
			if (itsInteger)
				query.Append($" [{field}] = {wrongValue} ");
			else
				query.Append($" [{field}] = '{wrongValue}' ");

			query.Append($" AND [TimeModified] >= @dateAt AND [TimeModified] <= @dateTo AND [TimeCreated] < @dateAt ");
			query.Append($" AND [ID] IN (SELECT DISTINCT [ID] FROM {versionTable} AS InstructionsVersionsTable WITH(NOLOCK) ");
			query.Append($" WHERE InstructionsVersionsTable.[ID] = {mainTable}.[ID] AND (");

			if (itsInteger)
				query.Append($" InstructionsVersionsTable.[{field}] <> {wrongValue} ");
			else
				query.Append($" InstructionsVersionsTable.[{field}] <> '{wrongValue}' ");

			query.Append($" OR InstructionsVersionsTable.[{field}] IS NULL ) ");
			query.Append($" AND InstructionsVersionsTable.[TimeModified] <= @dateAt )");

			SqlParameter[] sqlParameters = new SqlParameter[]
			{
				new SqlParameter
				{
					SqlDbType = System.Data.SqlDbType.DateTime,
					ParameterName = "@dateAt",
					Value = dateAt,
				},
				new SqlParameter
				{
					SqlDbType = System.Data.SqlDbType.DateTime,
					ParameterName = "@dateTo",
					Value = dateTo,
				}
			};

			Console.WriteLine("Поиск карточек для восстановления");

			return list.GetItems(query.ToString(), sqlParameters);
		}


		/// <summary>
		/// Процесс восстановления
		/// </summary>
		internal void Restore()
		{
			this.Loger.AutoFlush = true;
			Settings settings = new Settings();

			//поле и значение по которым определяем нужно ли восстанавливать карточку
			string field = settings.RestoreSettings.Field;
			string wrongValue = settings.RestoreSettings.WrongValue;

			//Получаем поле и проверяем его наличие и версионность
			DBField dbfield = settings.List.List.GetField(field, true);
			if (!dbfield.Versioned)
				throw new Exception($"Поле '{field}' не является версионным");

			//Находим карточки которые необходимо восстановить, если таких нет - выходим
			DBItemCollection items = this.FindItems(settings);

			//Записываем о них инфу
			this.WriteLogInfo(items, settings.List.List);

			//если таких нет - у нас записана вся инфа и выходим
			if (items.Count == 0)
				return;

			//счётчик для прогресс бара
			int completed = 0;
			//Восстанавливаем ближайшую версию каждой карточки
			foreach (DBItem item in items)
			{
				//Переворачиваем массив чтобы первый элемент был самый новый
				//и находим самый первый элемент где значение было не wrongValue
				DBItemVersion correctVersion = item.Versions.Reverse().FirstOrDefault(version =>
				{
					//Получаем значение поля в этой версии
					string value = string.Empty;

					if (dbfield.IsTypeOfLookupSingle())
					{
						DBFieldLookupValue lookup = version.GetValue<DBFieldLookupValue>(field);
						if (lookup != null)
							value = lookup.LookupID.ToString();
					}
					else if (dbfield.IsTypeOfLookupMulti())
					{
						DBFieldLookupValueCollection lookup = version.GetValue<DBFieldLookupValueCollection>(field);
						if (lookup.Count > 0)
							value = string.Join(";", lookup.Select(l => l.LookupID.ToString()).ToArray());
					}
					else
					{
						object val = version.GetValue(field);
						if (val != null)
							value = val.ToString();
					}

					return !string.Equals(wrongValue, value, StringComparison.OrdinalIgnoreCase);
				});

				if (correctVersion == null)
					throw new Exception($"Для карточки '{item.ID}' не удалось получить версию для восстановления");

				//восстанавливаем
				correctVersion.Restore();

				//инфа в прогерсс бар
				Console.Write($"Восстановлено: {++completed} из {items.Count}");
				Console.CursorLeft = 0;

				//инфа в лог
				this.Loger.WriteLine($"Дата {DateTime.Now.ToString()}  карточка ID='{item.ID}' восстановлена на версию='{correctVersion.VersionNumber}'");
			}

			Console.WriteLine();
		}
	}

}

