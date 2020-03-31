namespace WSSC.V4.DMS.CUSTOM
{
    public static partial class Consts
    {

        public class Roles
        {
            public class Name
            {
                /// <summary>
                /// Эксперты
                /// </summary>
                public const string Experts = "Эксперты";
            }
        }

        public class Solutions
        {
            /// <summary>
            /// Согласовать
            /// </summary>
            public const string Agree = "Согласовать";
            /// <summary>
            /// Направить на рассмотрение экспертам
            /// </summary>
            public const string SendToConsiderationExperts = "Направить на рассмотрение экспертам";
            /// <summary>
            /// Предоставить комментарий
            /// </summary>
            public const string ProvideComment = "Предоставить комментарий";
            /// <summary>
            /// Отправить на доработку
            /// </summary>
            public const string Revised = "Отправить на доработку";
            /// <summary>
            /// Запросить предоставление обратной связи по договору.
            /// </summary>
            public const string RequestFeedback = "Запросить предоставление обратной связи по договору";
            /// <summary>
            /// Отправить комментарий
            /// </summary>
            public const string SendComment = "Отправить комментарий";
            /// <summary>
            /// Заключение
            /// </summary>
            public const string Сonclusion = "Заключение";
        }


        public class Report
        {
            public const string HtmlButtonSaveToPdf = @"
                                    <script> 
                                        var wordHref =  document.getElementById('HyperLink1')
                                        if(wordHref == null)
                                            alert('Не найден элемент ссылки на формирование отчета в word');
                                        wordHref.setAttribute('href','/_LAYOUTS/WSS/WSSC.V4.DMS.LFR/Reports/SaveToPdf/SaveToPdfPage.ashx?itemID={0}&listID={1}'); 
                                        wordHref.setAttribute('style', 'z-index: 99');
                                        wordHref.innerHTML='Сохранить в PDF';
                                        document.title = 'Лист визирования';
                                  </script>";

            public class ExpertsSolution
            {

                /// <summary>
                /// Формат даты
                /// </summary>
                public const string DateFormat = "dd.MM.yyyy HH:mm:ss";

                /// <summary>
                /// Кастомный тег для смены формата даты
                /// </summary>
                public const string DateTag = "#Date({0})Format({1})#";

                /// <summary>
                /// [Результат решения (текст)]
                /// </summary>
                public const string FieldSolutionResult = "Результат решения (текст)";
                /// <summary>
                /// WSSC_Solutions
                /// </summary>
                public const string SolutionSQLTable = "WSSC_Solutions";

                /// <summary>
                /// [Таблица экспертов] в html
                /// </summary>
                public const string TableOfExperts = "[Таблица экспертов]";

                public const string TableHeader = @"
                            <tr>
                                <td class='headerFirst' valign='center'>Эксперты</td>
                                <td class='headerOther' valign='center'>ФИО</td>
                                <td class='headerOther' valign='center'>Дата получения</td>
                                <td class='headerOther' valign='center'>Дата согласования</td>
                                <td class='headerOther' valign='center'>Результат согласования</td>
                                <td class='headerOther' valign='center'>Комментарий</td>
                            </tr>";

                public const string TableRow = @"
                           <tr>
                                <td class='cellFirst' valign='top'>{0}</td>
                                <td class='cellOther' valign='top'>{1}</td>
                                <td class='cellOther' valign='top'>{2}</td>
                                <td class='cellOther' valign='top'>{3}</td>
                                <td class='cellOther' valign='top'>{4}</td>
                                <td class='cellOther' valign='top'>{5}</td>
                           </tr>";
            } 
        }

        public class Lists
        {
            public class CommonFields
            {
                public const string UserPost = "Должность";
                public const string Commission = "Поручения";
                public const string Stage = WSSC.V4.DMS.Workflow.Consts.list_Requests_col_Stage;
                public const string Name = "Название";
                public const string RegNumber = "Регистрационный номер";
                public const string Adresats = "Адресаты";
                public const string Initiator = "Инициатор";
                public const string Content = "Содержание";
                public const string DocumentFiles = "Файлы документа";
                public const string IsSavedByUser = "Элемент сохранен пользователем";
                public const string Company = "Компания";
                public const string PreviousItem = "Предыдущая заявка";
                public const string AgrStage = "Согласование";
            }

            public class Commission
            {
                public const string Author = "Автор поручения";
                public const string Executor = "Исполнитель";
                public const string CoExecutors = "Соисполнители";
                public const string Content = "Содержание";
                public const string Controller = "Контролер";
                public const string ExpireDate = "Исполнить до";
                public const string DateCreation = "Дата создания";
            } 
        }

        public class Reports
        {
            public class ResolutionsExtraReport
            {

                public const string FieldNameAddresses = "Адресаты";

                /// <summary>
                /// Название системной константы
                /// </summary>
                public const string SettingsConstName = "EDC.ResolutionsExtraReport";
                /// <summary>
                /// Решение [Списать в дело]
                /// </summary>
                public const string SolutionNameWriteToBusiness = "Списать в дело";
                /// <summary>
                /// Решение [Рассмотрено]
                /// </summary>
                public const string SolutionNameConsidered = "Рассмотрено";
                /// <summary>
                /// Решение [Рассмотрено (доп.адресаты)]
                /// </summary>
                public const string SolutionNameConsideredGoNext = "Рассмотрено (доп.адресаты)";
                /// <summary>
                /// [Факсимиле]
                /// </summary>
                public const string FieldNameFaximile = "Факсимиле";
                /// <summary>
                /// Исполнитель
                /// </summary>
                public const string TableExecutor = "Исполнитель";
                /// <summary>
                /// Соисполнители
                /// </summary>
                public const string TableCoexecutors = "Соисполнители";
                /// <summary>
                /// Контролер
                /// </summary>
                public const string TableController = "Контролер";
                /// <summary>
                /// Содержание
                /// </summary>
                public const string TableContent = "Содержание";
                /// <summary>
                /// Дата создания
                /// </summary>
                public const string TableDateCreation = "Дата создания";
                /// <summary>
                /// Срок исполнения
                /// </summary>
                public const string TableExecutionTime = "Срок исполнения";
                /// <summary>
                /// Подпись автора
                /// </summary>
                public const string TableSignature = "Подпись автора";


                public const string HtmlTableStyle = "style='table-layout: fixed; font-size: 12pt; font-weight: normal; font-family: \"Arial Narrow\", Arial, sans-serif; text-align: left;'";
                public const string HtmlTDHeaderFirst = "style='border:1px solid #000000; text-align: center; background-color: #F2F2F2; width: 140px; height: 60px;'";
                public const string HtmlTDHeaderOthers = "style='border:1px solid #000000; border-left-style: none; text-align: center; background-color: #F2F2F2; width: 140px; height: 60px;'";
                public const string HtmlTDCellFirst = "style='border:1px solid #000000; border-top-style: none; text-align: left; vertical-align: top;'";
                public const string HtmlTDCellOthers = "style='border:1px solid #000000; border-top-style: none; text-align: left; vertical-align: top;'";
            }
        }
    }
}