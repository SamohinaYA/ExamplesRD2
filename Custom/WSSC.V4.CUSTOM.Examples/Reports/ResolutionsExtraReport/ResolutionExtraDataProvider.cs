using System;
using System.Collections.Generic;
using System.Linq;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.DMS.Fields.Commissions;
using WSSC.V4.DMS.Workflow;
using WSSC.V4.SYS.Lib.DBObjects;
using WSSC.V4.SYS.Fields.Files;
using WSSC.V4.SYS.Fields.Lookup;

namespace WSSC.V4.DMS.EDC.Reports.ResolutionsExtraReport
{
	/// <summary>
	/// Поставщик данных для отчёта по резолюциям
	/// </summary>
	public class ResolutionsExtraDataProvider
	{
        internal ResolutionsExtraDataProvider(DBItem item)
        {
            //_item
            _item = item ?? throw new ArgumentNullException(nameof(item));

            //Settings
            Settings = new ResolutionExtraReportSettings(this._item);

            //AllSolutions
            DBObjectAdapter<SolutionsHistory> SolutionsHistoryAdapter = new DBObjectAdapter<SolutionsHistory>(this._item.Site.SiteConnectionString);
            string query = "[ListID] = {0} AND [ItemID] = {1}";
            AllSolutions = SolutionsHistoryAdapter.GetObjects(string.Format(query, this._item.List.ID, this._item.ID));

            //ResolutionSolutions
            ResolutionSolutions = AllSolutions.Where(
                        t => (t.SolutionName == Consts.Reports.ResolutionsExtraReport.SolutionNameConsidered
                        || t.SolutionName == Consts.Reports.ResolutionsExtraReport.SolutionNameConsideredGoNext)
                        && !string.IsNullOrEmpty(t.Comment)
                        && (this.UsersIdSet.Contains(t.UserID) || (this.DeputiesSet.FirstOrDefault(r => r.DeputyID == t.UserID) != default))
                        ).ToList();

            //DeputiesSet
            DeputiesSet = this.GetUsersDeputies(this.UsersIdSet);

            //UsersIdSet
            UsersIdSet = this.GetUsersIDFromLookupField(Consts.Reports.ResolutionsExtraReport.FieldNameAddresses);

            //AllCommissions
            CMField field = this._item.List.GetField<CMField>(Consts.Lists.CommonFields.Commission) 
                ?? throw new DBException.MissingField(this._item.List, Consts.Lists.CommonFields.Commission);
            CMItemCollection commissions = field.GetCommissions(this._item) 
                ?? throw new Exception(string.Format("Не удалось получить '{0}' в свойстве '{1}' класса '{2}'", "CMItemCollection", "_AllCommissions", "ResolutionsExtraReport"));
            AllCommissions = commissions.Commissions;

            //UsersCommissions
            UsersCommissions = new List<DBItem>();
            foreach (CMItem cmitem in this.AllCommissions)
            {
                var currentItem = cmitem?.Item 
                    ?? throw new Exception(string.Format("Не удалось получить '{0}' в свойстве '{1}' класса '{2}'", "_AllCommissions.Item", "_AddressesCommissions", "ResolutionsExtraReport"));

                int authorID = item.GetLookupID(Consts.Lists.Commission.Author);
                if (authorID != 0 && (this.UsersIdSet.Contains(authorID)
                    || (this.DeputiesSet.FirstOrDefault(t => t.DeputyID == authorID) != default)))
                    UsersCommissions.Add(item);
            }
        }

        #region Properties

        private readonly DBItem _item;

        /// <summary>
        /// Объект со значениями из системной константы
        /// </summary>
        internal readonly ResolutionExtraReportSettings Settings;

        /// <summary>
        /// Принятые решения по карточке
        /// </summary>
        internal readonly List<SolutionsHistory> AllSolutions;

        /// <summary>
        /// Решения по резолюции с комментарием, пользователей из поля указанного в настройках (или их замов)
        /// </summary>
        internal readonly List<SolutionsHistory> ResolutionSolutions;

        /// <summary>
        /// Id заместителей пользователей, полученных из указанного в настройках поля
        /// </summary>
        internal readonly HashSet<DBDeputy> DeputiesSet;


        /// <summary>
        /// Id пользователей, полученных из указанного в настройках поля
        /// </summary>
        internal readonly HashSet<int> UsersIdSet;

        /// <summary>
        /// Все поручения из карточки
        /// </summary>
        internal readonly List<CMItem> AllCommissions;

        /// <summary>
        /// Получение поручений, в которых полученные пользователи указаны как [Автор поручения]
        /// </summary>
        internal readonly List<DBItem> UsersCommissions;




		private HashSet<DBDeputy> GetUsersDeputies(HashSet<int> users)
		{
			HashSet<DBDeputy> result = new HashSet<DBDeputy>();
			foreach (int userId in users)
			{
				DBUser user = this._item.Site.GetUser(userId, true)
					?? throw new Exception($"Не удалось найти пользователя с ID = {userId}");

				DBUserMembershipPool pool = user.GetMembershipPool(this._item.Web);
                foreach (DBDeputy deputy in pool.Deputies)
                {
                    result.Add(deputy);
                }
			}

			return result;
		}

		private HashSet<int> GetUsersIDFromLookupField(string fieldName)
		{
			DBField usersField = this._item.List.GetField(fieldName, true);
			if (!usersField.IsTypeOfLookup())
				throw new Exception($"Поле {fieldName} не является полем подстановки");

			HashSet<int> result = new HashSet<int>();
			if (usersField.IsTypeOfLookupMulti())
			{
				DBFieldLookupValueCollection values = this._item.GetLookupValues(fieldName);
                foreach (DBFieldLookupValue user in values)
                {
                    if (user != null)
                    {
                        result.Add(user.LookupID);
                    }
                }
			}
			else if (usersField.IsTypeOfLookupSingle())
			{
				DBFieldLookupValue value = this._item.GetLookupValue(fieldName);
                if (value != null)
                {
                    result.Add(value.LookupID);
                }
			}

			return result;
		}

		#endregion

		#region Methods
		/// <summary>
		/// Разметка факсимиле
		/// </summary>
		/// <param name="item">Карточка юзера</param>
		internal string GetFaximile(DBItem userItem)
		{
            var files = userItem?.GetFiles(Consts.Reports.ResolutionsExtraReport.FieldNameFaximile);
            var isEmpty = (userItem is null || files is null || files.Count == 0);

            return isEmpty ? string.Empty : string.Format("<br/><img src=\"{0}\"/>", _item.Site.Url + files.First().ServerRelativeUrl);
        }

        /// <summary>
        /// Получение решения из системной константы
        /// </summary>
        /// <returns></returns>
        internal SolutionsHistory GetSolution() => AllSolutions.FirstOrDefault(t => t.SolutionName == Settings.SolutionRowAttributes.SolutionName);


		public DBUser GetAdresseSolutionUser(int solutionUserID) => GetSolutionUserInternal(solutionUserID, UsersIdSet, DeputiesSet);
	


		public DBUser GetCustomSolutionUser(int solutionUserID)
		{
			HashSet<int> users = GetUsersIDFromLookupField(Settings.SolutionRowAttributes.UsersField);
			HashSet<DBDeputy> deputies = GetUsersDeputies(users);

			return GetSolutionUserInternal(solutionUserID, users, deputies);
		}


		private DBUser GetSolutionUserInternal(int solutionUserID, HashSet<int> usersSet, HashSet<DBDeputy> deputiesSet)
		{
			if (solutionUserID == 0)
				throw new Exception($"Передано пустое значение пользователя");

			// проверяем есть ли пользователь, принявший решение среди тех, кто указан в поле
			int userID = usersSet.FirstOrDefault(u => u == solutionUserID);
			// если не нашли, проверям есть ли он среди заместителей и получаем
			if (userID == default)
				userID = usersSet.FirstOrDefault(u =>
				{
					int? forWhom = deputiesSet.FirstOrDefault(d => d.DeputyID == solutionUserID)?.UserID;
					bool subResult = forWhom.HasValue && forWhom.Value == u;

					return subResult;
				});

			if (userID == 0)
				throw new Exception($"Не удалось найти пользователя с ID = {solutionUserID} по решению");

			return _item.Site.GetUser(userID, true) 
                ?? throw new Exception($"Не удалось получить пользователя с ID = {userID}");
		}
		#endregion
	}
}
