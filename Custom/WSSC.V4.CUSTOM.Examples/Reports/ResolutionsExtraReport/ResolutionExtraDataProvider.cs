using System;
using System.Collections.Generic;
using System.Linq;
using WSSC.V4.DMS.Fields.Commissions;
using WSSC.V4.DMS.Workflow;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Fields.Files;
using WSSC.V4.SYS.Fields.Lookup;
using WSSC.V4.SYS.Lib.DBObjects;

namespace WSSC.V4.DMS.CUSTOM.Examples.Reports.ResolutionsExtraReport
{
    /// <summary>
    /// Поставщик данных для отчёта по резолюциям
    /// </summary>
    internal class ResolutionsExtraDataProvider
    {
        internal ResolutionsExtraDataProvider(DBItem item)
        {
            //_item
            _item = item ?? throw new ArgumentNullException(nameof(item));

        }

        private readonly DBItem _item;

        #region API

        private ResolutionExtraReportSettings _settings;
        /// <summary>
        /// Объект со значениями из системной константы
        /// </summary>
        internal ResolutionExtraReportSettings Settings => _settings
            ?? (_settings = new ResolutionExtraReportSettings(_item));


        internal List<SolutionsHistory> _allSolutions;
        /// <summary>
        /// Принятые решения по карточке
        /// </summary>
        internal List<SolutionsHistory> AllSolutions
        {
            get
            {
                if (_allSolutions is null)
                {
                    DBObjectAdapter<SolutionsHistory> SolutionsHistoryAdapter = new DBObjectAdapter<SolutionsHistory>(_item.Site.SiteConnectionString);
                    string query = "[ListID] = {0} AND [ItemID] = {1}";
                    _allSolutions = SolutionsHistoryAdapter.GetObjects(string.Format(query, _item.List.ID, _item.ID));
                }
                return _allSolutions;
            }
        }


        internal List<SolutionsHistory> _resolutionSolutions;
        /// <summary>
        /// Решения по резолюции с комментарием, пользователей из поля указанного в настройках (или их замов)
        /// </summary>
        internal List<SolutionsHistory> ResolutionSolutions => _resolutionSolutions
            ?? (_resolutionSolutions = AllSolutions.Where(t =>
            (t.SolutionName == Consts.Reports.ResolutionsExtraReport.SolutionNameConsidered
            || t.SolutionName == Consts.Reports.ResolutionsExtraReport.SolutionNameConsideredGoNext)
            && !string.IsNullOrEmpty(t.Comment)
            && (UsersIdSet.Contains(t.UserID)
            || (DeputiesSet.FirstOrDefault(r => r.DeputyID == t.UserID) != default))
            ).ToList());


        internal HashSet<DBDeputy> _deputiesSet;
        /// <summary>
        /// Id заместителей пользователей, полученных из указанного в настройках поля
        /// </summary>
        internal HashSet<DBDeputy> DeputiesSet => _deputiesSet
            ?? (_deputiesSet = GetUsersDeputies(UsersIdSet));


        internal HashSet<int> _usersIdSet;
        /// <summary>
        /// Id пользователей, полученных из указанного в настройках поля
        /// </summary>
        internal HashSet<int> UsersIdSet => _usersIdSet
            ?? (_usersIdSet = GetUsersIDFromLookupField(Consts.Reports.ResolutionsExtraReport.FieldNameAddresses));


        internal List<CMItem> _allCommissions;
        /// <summary>
        /// Все поручения из карточки
        /// </summary>
        internal List<CMItem> AllCommissions => _allCommissions
            ?? (_allCommissions = GetAllCommissions());


        internal List<DBItem> _usersCommissions;
        /// <summary>
        /// Получение поручений, в которых полученные пользователи указаны как [Автор поручения]
        /// </summary>
        internal List<DBItem> UsersCommissions => _usersCommissions
            ?? (_usersCommissions = GetUsersCommissions());


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


        internal DBUser GetAdresseSolutionUser(int solutionUserID) => GetSolutionUserInternal(solutionUserID, UsersIdSet, DeputiesSet);


        internal DBUser GetCustomSolutionUser(int solutionUserID)
        {
            HashSet<int> users = GetUsersIDFromLookupField(Settings.SolutionRowAttributes.UsersField);
            HashSet<DBDeputy> deputies = GetUsersDeputies(users);

            return GetSolutionUserInternal(solutionUserID, users, deputies);
        }
        #endregion

        #region Methods

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

        private HashSet<DBDeputy> GetUsersDeputies(HashSet<int> users)
        {
            HashSet<DBDeputy> result = new HashSet<DBDeputy>();
            foreach (int userId in users)
            {
                DBUser user = _item.Site.GetUser(userId, true)
                    ?? throw new Exception($"Не удалось найти пользователя с ID = {userId}");

                DBUserMembershipPool pool = user.GetMembershipPool(_item.Web);
                foreach (DBDeputy deputy in pool.Deputies)
                {
                    result.Add(deputy);
                }
            }

            return result;
        }

        private HashSet<int> GetUsersIDFromLookupField(string fieldName)
        {
            DBField usersField = _item.List.GetField(fieldName, true);
            if (!usersField.IsTypeOfLookup())
                throw new Exception($"Поле {fieldName} не является полем подстановки");

            HashSet<int> result = new HashSet<int>();
            if (usersField.IsTypeOfLookupMulti())
            {
                DBFieldLookupValueCollection values = _item.GetLookupValues(fieldName);
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
                DBFieldLookupValue value = _item.GetLookupValue(fieldName);
                if (value != null)
                {
                    result.Add(value.LookupID);
                }
            }

            return result;
        }

        private List<CMItem> GetAllCommissions()
        {
            CMField field = _item.List.GetField<CMField>(Consts.Lists.CommonFields.Commission)
                         ?? throw new DBException.MissingField(_item.List, Consts.Lists.CommonFields.Commission);
            CMItemCollection commissions = field.GetCommissions(_item)
                ?? throw new Exception(string.Format("Не удалось получить '{0}' в свойстве '{1}' класса '{2}'", "CMItemCollection", "_AllCommissions", "ResolutionsExtraReport"));
            return commissions.Commissions;
        }

        private List<DBItem> GetUsersCommissions()
        {
            var usersCommissions = new List<DBItem>();
            foreach (CMItem cmitem in AllCommissions)
            {
                var currentItem = cmitem?.Item
                    ?? throw new Exception(string.Format("Не удалось получить '{0}' в свойстве '{1}' класса '{2}'", "_AllCommissions.Item", "_AddressesCommissions", "ResolutionsExtraReport"));

                int authorID = _item.GetLookupID(Consts.Lists.Commission.Author);
                if (authorID != 0 && (UsersIdSet.Contains(authorID)
                    || (DeputiesSet.FirstOrDefault(t => t.DeputyID == authorID) != default)))
                    usersCommissions.Add(_item);
            }
            return usersCommissions;
        }
        #endregion
    }
}

