using Aspose.Words;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using WSSC.V4.DMS.Reports;
using WSSC.V4.DMS.Reports.AsposeUtils;
using WSSC.V4.DMS.Reports.PrintForm;
using WSSC.V4.DMS.Workflow;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Fields.Lookup;
using AW = Aspose.Words;

namespace WSSC.V4.DMS.EDC.Reports
{
    /// <summary>
    /// Отчёт по резолюциям
    /// </summary>
    internal class ResolutionsExtraReportBuilder : PFCustomReport
    {
        internal ResolutionsExtraReportBuilder(PFBuilder builder) : base(builder)
        {
            _dataProvider = new ResolutionsExtraDataProvider(Item);

            _resolutionTable = BuildTable();
        }

        /// <summary>
        /// Помощник построения отчёта
        /// </summary>
        private readonly ResolutionsExtraDataProvider _dataProvider;

        /// <summary>
        /// Html таблица отчёта по резолюциям
        /// </summary>
        private readonly Table _resolutionTable;


        /// <summary>
        /// Добавляет ячейку в строку
        /// </summary>
        /// <param name="row">строка</param>
        /// <param name="text">текст</param>
        /// <param name="isFirst">первая ячейка?</param>
        private void AddCell(TableRow row, string text = null, bool isFirst = false)
        {
            row.Cells.Add(new TableCell()
            {
                CssClass = isFirst ? "ResolutionsExtraReportTableCellFirst" : "ResolutionsExtraReportTableCellOthers",
                Text = text ?? string.Empty,
            });
        }


        /// <summary>
        /// Возвращает данные таблицы
        /// </summary>
        private List<TableRow> BuildData()
        {
            List<TableRow> result = new List<TableRow>();
            TableRow row;

            if (_dataProvider.UsersIdSet.Count > 0)
            {
                /* информация по решениям адресатов «Рассмотрено» и «Рассмотрено(доп. адресаты)», если заполняется «Текст резолюции». 
                     Если решений с текстом резолюции несколько, то для каждого решения отдельная строчка*/
                string initor = Item.GetStringValue(Consts.Lists.CommonFields.Initiator);
                foreach (SolutionsHistory solution in _dataProvider.ResolutionSolutions)
                {
                    DBUser user = _dataProvider.GetAdresseSolutionUser(solution.UserID);

                    row = new TableRow();
                    AddCell(row, initor);
                    AddCell(row);
                    AddCell(row);
                    AddCell(row, solution.Comment);
                    AddCell(row, $"{solution.Date.ToShortDateString()} {solution.Date.ToShortTimeString()}");
                    AddCell(row);
                    AddCell(row, $"{user.Name}{Environment.NewLine}{_dataProvider.GetFaximile(user.UserItem)}");

                    result.Add(row);
                }

                /*информация из полей вложенных поручений, проставленных адресатами. Если вложенных поручений несколько, то для каждого поручения отдельная строчка.*/
                foreach (DBItem cmitem in _dataProvider.UsersCommissions)
                {
                    DBUser user = _dataProvider.GetAdresseSolutionUser(cmitem.GetLookupID(Consts.Lists.Commission.Author));

                    row = new TableRow();
                    AddCell(row, cmitem.GetStringValue(Consts.Lists.Commission.Executor));
                    AddCell(row, cmitem.GetStringValue(Consts.Lists.Commission.CoExecutors));
                    AddCell(row, cmitem.GetStringValue(Consts.Lists.Commission.Controller));
                    AddCell(row, cmitem.GetStringValue(Consts.Lists.Commission.Content));
                    AddCell(row, cmitem.GetStringValue(Consts.Lists.Commission.DateCreation));
                    AddCell(row, cmitem.GetStringValue(Consts.Lists.Commission.ExpireDate));
                    AddCell(row, $"{user.Name}{Environment.NewLine}{_dataProvider.GetFaximile(user.UserItem)}");

                    result.Add(row);
                }

                /*информация по решениям, указанным в настройках*/
                SolutionsHistory sol = _dataProvider.GetSolution();
                if (sol != null)
                {
                    DBUser user = _dataProvider.GetCustomSolutionUser(sol.UserID);

                    row = new TableRow();
                    AddCell(row, string.Empty);
                    AddCell(row, string.Empty);
                    AddCell(row, string.Empty);
                    AddCell(row, sol.Comment);
                    AddCell(row, $"{sol.Date.ToShortDateString()} {sol.Date.ToShortTimeString()}");
                    AddCell(row, string.Empty);
                    AddCell(row, $"{user.Name}{Environment.NewLine}{_dataProvider.GetFaximile(user.UserItem)}");

                    result.Add(row);
                }
            }
            return result;
        }


        /// <summary>
        /// Возвращает заголовк таблицы
        /// </summary>
        private TableRow BuildHeader()
        {
            TableRow result = new TableRow();
            result.Cells.AddRange(new TableCell[]
            {
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderFirst",
                    Text = Consts.Reports.ResolutionsExtraReport.TableExecutor,
                },
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderOthers",
                    Text = Consts.Reports.ResolutionsExtraReport.TableCoexecutors,
                },
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderOthers",
                    Text = Consts.Reports.ResolutionsExtraReport.TableController,
                },
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderOthers",
                    Text = Consts.Reports.ResolutionsExtraReport.TableContent,
                },
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderOthers",
                    Text = Consts.Reports.ResolutionsExtraReport.TableDateCreation,
                },
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderOthers",
                    Text = Consts.Reports.ResolutionsExtraReport.TableExecutionTime,
                },
                new TableCell()
                {
                    CssClass = "ResolutionsExtraReportTableHeaderOthers",
                    Text = Consts.Reports.ResolutionsExtraReport.TableSignature,
                },
            });

            return result;
        }

        /// <summary>
        /// Возвращает таблицу резолюций
        /// </summary>
        private Table BuildTable()
        {
            Table result = new Table() { CssClass = "ResolutionsExtraReportTable" };
            TableRow header = BuildHeader();
            TableRow[] data = BuildData().ToArray();
            result.Rows.Add(header);
            result.Rows.AddRange(data);

            return result;
        }

        /// <summary>
        /// Кастом для html тэга
        /// </summary>
        [PFHtmlTag("ResolutionTableHTMLTag")]
        internal void CustomTagResolutionTable(HtmlTextWriter htw, PFTag tag)
        {
            _resolutionTable.RenderControl(htw);
            Item.Site.AppContext.ScriptManager.RegisterResource("Reports/ResolutionsExtraReport/ResolutionsExtraReport.css", VersionProvider.ModulePath);
        }

        /// <summary>
        /// Кастом для Microsoft Word
        /// </summary>
        [PFWordTag("ResolutionTableWordTag")]
        internal void CustomTagResolutionTable(Bookmark bookmark, DocumentBuilder builder, PFTag tag)
        {

            AW.Document doc = builder.Document;
            doc.ResourceLoadingCallback = new AsposeResourceLoadingCallback();
            builder.PageSetup.Orientation = AW.Orientation.Landscape;

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                {
                    _resolutionTable.RenderControl(htw);
                    builder.InsertHtml($"<html><head><style>{Properties.Resources.ResolutionsExtraReport}</style></head><body>{sw.ToString()}</body><html>");
                }
            }
        }
    }
}
