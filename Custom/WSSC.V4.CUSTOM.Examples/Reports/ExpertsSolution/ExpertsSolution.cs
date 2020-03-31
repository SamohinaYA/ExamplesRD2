using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WSSC.V4.DMS.Workflow;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Lib.DBObjects;

namespace WSSC.V4.DMS.CUSTOM.Reports
{
    /// <summary>
    /// Кастомный лист согласования для [Таблица экспертов] для роли "Эксперты" по решениям "Предоставить комментарий"
    /// </summary>
    internal class ExpertsSolution
    {
        #region prop

        private DBItem _item;

        private List<SolutionsHistory> _solutionsAll;
        /// <summary>
        /// Принятые решения отсортированые по ID desc
        /// </summary>
        private List<SolutionsHistory> SolutionsAll
        {
            get
            {
                if (_solutionsAll is null)
                {
                    DBObjectAdapter<SolutionsHistory> adapter = new DBObjectAdapter<SolutionsHistory>(_item.Site.PrimaryDatabase.Connection.ConnectionString);
                    _solutionsAll = adapter.GetObjects(string.Format("[ListID] = {0} AND [ItemID] = {1}", _item.List.ID, _item.ID), "ORDER BY [ID] DESC");
                }
                return _solutionsAll;
            }

        }


        private List<SolutionsHistory> _solutionsExpert;
        /// <summary>
        /// История решений "Предоставить комментарий" принятых ролью "Эксперты"
        /// </summary>
        private List<SolutionsHistory> SolutionsExpert
        {
            get
            {
                if (_solutionsExpert is null)
                {

                    if (SolutionsAll != null)
                        _solutionsExpert = SolutionsAll
                                               .Where(x =>
                                                   !string.IsNullOrEmpty(x.Roles) &&
                                                   Regex.IsMatch(x.Roles, string.Format(@"(^|;)\s*{0}\s*($|;)", Consts.Roles.Name.Experts), RegexOptions.IgnoreCase) &&
                                                   x.SolutionName == Consts.Solutions.ProvideComment)
                                               .ToList();
                }
                return _solutionsExpert;
            }
        }



        private string _dateSendToConsiderationExperts;
        /// <summary>
        /// дата последнего принятия решения «Направить на рассмотрение экспертам»
        /// </summary>
        public string DateSendToConsiderationExperts
        {
            get
            {
                if (_dateSendToConsiderationExperts is null)
                {
                    var solution = this.SolutionsAll.FirstOrDefault(sol => sol.SolutionName == Consts.Solutions.SendToConsiderationExperts);
                    _dateSendToConsiderationExperts = solution != null ? solution.Date.ToString(Consts.Report.ExpertsSolution.DateFormat) : string.Empty;
                }
                return _dateSendToConsiderationExperts;
            }
        }


        #endregion

        #region methods
        /// <summary>
        /// Получаем значение [Результат решения (текст)] указанного решения
        /// </summary>
        /// <param name="solutionName">Имя решения</param>
        private string GetSolutionResult(string solutionName)
        {
            DMSLogic logic = new DMSLogic(new DMSDocument(new DMSContext(_item.Web), _item))
                ?? throw new ArgumentNullException("'DMSLogic'");

            if (logic.SolutionAdapter is null)
                throw new ArgumentNullException("'DMSLogic.SolutionAdapter'");

            var solution = logic.SolutionAdapter.GetObjectByName(solutionName)
                ?? throw new ArgumentNullException(string.Format("'DMSLogic.SolutionAdapter.GetObjectByName({0})'", solutionName));

            return solution.SolutionResultText ?? string.Empty;
        }

        /// <summary>
        /// Построитель html-таблицы (Для каждого решения "Предоставить комментарий" роли "Эксперты" построить строку в таблице)
        /// </summary>
        private string BuildTable()
        {
            StringBuilder body = new StringBuilder();                                                   //строки в таблице, после заголовка

            //Если из роли "Эксперты" никто не принял решения "Предоставить комментарий" - строить нечего - выходим  
            if (this.SolutionsExpert != null)
            {
                string solutionResult = this.GetSolutionResult(Consts.Solutions.ProvideComment);            //Результат решения "Предоставить комментарий" 

                //Для каждого решения "Предоставить комментарий" роли "Эксперты" построить строку в таблице
                foreach (SolutionsHistory solution in this.SolutionsExpert)
                {
                    DBUser user = _item.Site.GetUser(solution.UserID);                                         // Юзер
                    string userPost = user.UserItem.GetStringValue(Consts.Lists.CommonFields.UserPost);     // [Должность] юзера
                    userPost = !string.IsNullOrEmpty(userPost) ? userPost : string.Empty;                   // Если должность не заполнена приводим к ""

                    body.Append(
                               string.Format(Consts.Report.ExpertsSolution.TableRow, userPost, user.Name, this.DateSendToConsiderationExperts,
                                             solution.Date.ToString(Consts.Report.ExpertsSolution.DateFormat), solutionResult, solution.Comment));
                }
            }
            return !string.IsNullOrEmpty(body.ToString())
                   ? string.Format("<table style='width:600px;' class='MsoNormalTable' border='0' cellspacing='0' cellpadding='0'>{0}{1}</table>", Consts.Report.ExpertsSolution.TableHeader, body)
                   : string.Empty;
        }


        /// <summary>
        /// Обработчка кастомного для смены формата даты
        /// </summary>
        private string RenderCustomDateFormat(string html)
        {
            //Находим все теги для смены даты на нужный формат (группа format) и заменяем их на преоразованную дату
            Match match = Regex.Match(html, @"#Date\((?<date>.*?)\)Format\((?<format>.*?)\)#");
            while (match.Success)
            {
                string date = match.Groups["date"]?.Value;          //Дата
                string format = match.Groups["format"]?.Value;      //Формат

                //если дата корректная и формат не пусто то конвертируем, иначе заменяем тег на группу date
                if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(format))
                {

                    html = (DateTime.TryParse(date, out DateTime datetime))
                            ? html.Replace(match.Value, datetime.ToString(format))
                            : html.Replace(match.Value, date);
                }
                else
                {
                    html = html.Replace(match.Value, date);
                }

                match = match.NextMatch();
            }

            return html;
        }
        #endregion

        /// <summary>
        /// Заменяем  [Таблица экспертов] на таблицу по постановке:
        ///  1)    Эксперты; - отображается должность сотрудника, входящего в роль «Эксперты», принявшего решение «Предоставить комментарий».
        ///  2)    ФИО; - отображается ФИО сотрудника, входящего в роль «Эксперты», принявшего решение «Предоставить комментарий».
        ///  3)    Дата получения; - отображается дата последнего принятия решения «Направить на рассмотрение экспертам».
        ///  4)    Дата согласования; - отображается дата принятия решения «Предоставить комментарий» сотрудником, указанным в столбце «ФИО».
        ///  5)    Результат согласования; - отображается результат принятого сотрудником решения - «Предоставлен комментарий»;
        ///  6)    Комментарии. – отображается комментарий к решению «Предоставить комментарий», принятому сотрудником, указанным в столбце «ФИО».
        /// Так же дополнительно реализуем кейс "Реализовать возможность сохранения отчета в формате PDF"
        /// </summary>
        internal string GetBoundDocumentsHtml(string html, DBItem item)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentNullException("html");

            if (html.IndexOf(Consts.Report.ExpertsSolution.TableOfExperts) == -1)
                throw new Exception(string.Format("В таблице настройке отсутствует тег '{0}'", Consts.Report.ExpertsSolution.TableOfExperts));

            _item = item ?? throw new ArgumentNullException("DBItem");

            html = this.RenderCustomDateFormat(html);   //меняем даты на нужный формат

            StringBuilder result = new StringBuilder();
            result.Append(html.Replace(Consts.Report.ExpertsSolution.TableOfExperts, this.BuildTable())); //Заменяем в html [Таблица экспертов] на построенную таблицу
            result.Append(string.Format(Consts.Report.HtmlButtonSaveToPdf, _item.ID, _item.List.ID));     //Заменяем кнопку "Сохранить в Word" на "Сохранить в PDF" 
            return result.ToString();
        }
    }
}
