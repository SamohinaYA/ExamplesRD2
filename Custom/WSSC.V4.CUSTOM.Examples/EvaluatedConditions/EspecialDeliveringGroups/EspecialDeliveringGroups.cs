using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Fields.Lookup;

namespace WSSC.V4.CUSTOM.Examples.EvaluatedConditions.EspecialDeliveringGroups
{
	/// <summary>
	/// Вычисляемое выражение для настройки фильтрации поля "Рассылка"
	/// </summary>
	public class EspecialDeliveringGroups
	{
		#region Свойства


		private bool __init_Context = false;
		private DBAppContext _Context;
		/// <summary>
		/// Контекст выполнения
		/// </summary>
		private DBAppContext Context
		{
			get
			{
				if (!__init_Context)
				{
					_Context = DBAppContext.Current;
					__init_Context = true;
				}
				return _Context;
			}

		}


		private bool __init_Site = false;
		private DBSite _Site;
		/// <summary>
		/// Сайт
		/// </summary>
		private DBSite Site
		{
			get
			{
				if (!__init_Site)
				{
					_Site = this.Context.Site;
					__init_Site = true;
				}
				return _Site;
			}

		}




		private bool __init_Item = false;
		private DBItem _Item;
		/// <summary>
		/// Карточка
		/// </summary>
		private DBItem Item
		{
			get
			{
				if (!__init_Item)
				{
					int listID = this.Context.GetRequestValue<int>("ListFormListID");
					int itemID = this.Context.GetRequestValue<int>("ListFormItemID");

					if (listID < 1 || itemID < 1)
						throw new Exception("Некорректные параметр 'ListFormListID' или 'ListFormItemID'");

					DBList list = this.Site.GetList(listID, true);
					_Item = list.GetItem(itemID) ?? throw new DBException.MissingItem(list, itemID);

					__init_Item = true;
				}
				return _Item;
			}

		}

		#endregion


		#region Методы
		/// <summary>
		/// Вычисляет доступные группы рассылки для текущего пользователя
		/// </summary>
		/// <returns>SQL-запрос</returns>
		public string GetGroups()
		{
			string result = string.Empty;

			//Список группы рассылки
			DBList deviringList = this.Site.RootWeb.GetList(Consts.DeliveringGroups.ListName, true);

			//список «WSSC_Доступ к группам рассылки»
			DBWeb web = this.Site.GetWeb("dms", true);
			DBList accessList = web.GetList(Consts.Lists.AccessOfDelivering.ListName, true);

			//поля в которых нужно проверять текущего юзера
			DBFieldLookupMulti fieldGroups = accessList.GetField<DBFieldLookupMulti>(Consts.Lists.AccessOfDelivering.Fields.UsersGroups, true);
			DBFieldLookupMulti fieldUsers = accessList.GetField<DBFieldLookupMulti>(Consts.Lists.AccessOfDelivering.Fields.Users, true);
			DBFieldLookupMulti fieldDevGroups = accessList.GetField<DBFieldLookupMulti>(Consts.Lists.AccessOfDelivering.Fields.DeliverinGroups, true);

			//текущий юзер
			DBUser currentUser = this.Context.CurrentUser;

			//группы текущего юзера
			IEnumerable<int> currentUsersGroups = currentUser.AllGroups.Select(group => group.ID);

			List<string> devGroupsID = new List<string>();
			foreach (DBItem accessItem in accessList.GetItems($"{fieldUsers.GetSelectCondition(currentUser.ID)} OR {fieldGroups.GetSelectCondition(currentUsersGroups)}"))
			{
				devGroupsID.AddRange(accessItem.GetLookupValues(Consts.Lists.AccessOfDelivering.Fields.DeliverinGroups).Select(lv => lv.LookupID.ToString()));
			}

			string devGroupsIDStr = string.Join(",", devGroupsID.Distinct().ToArray());
			string condition = devGroupsIDStr.Length > 0 ? $"OR[ID] IN({ devGroupsIDStr})" : string.Empty;


			result = $@"
[ID] IN (
        SELECT [ID] FROM {deviringList.TableName} WITH(NOLOCK)
        WHERE ([{Consts.DeliveringGroups.AccessRestriction}] = 0 OR [{Consts.DeliveringGroups.AccessRestriction}] IS NULL)
              {condition}
              )
";

			//throw new Exception(result);
			return result;
		}

		#endregion
	}

}
