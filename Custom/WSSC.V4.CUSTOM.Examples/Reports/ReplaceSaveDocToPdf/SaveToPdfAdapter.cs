using Aspose.Pdf.Generator;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using WSSC.V4.DMS.Reports;
using WSSC.V4.SYS.DBFramework;
using WSSC.V4.SYS.Lib.Aspose;

namespace WSSC.V4.DMS.CUSTOM.Report
{
    /// <summary>
    /// Вспомогательный класс для сохранения листа визирования в формате pdf
    /// </summary>
    public class SaveToPdfAdapter : IDisposable
    {
        public SaveToPdfAdapter(DBItem item)
        {
            _item = item
                ?? throw new ArgumentNullException("item");
        }

        private DBItem _item;

        /// <summary>
        /// Освобождение ресурсов.
        /// </summary>
        public void Dispose() { }


        /// <summary>
        /// Возвращает лист визирования в формате pdf
        /// </summary>
        /// <param name="currentUser">Пользователь из текущего контекста.</param>
        /// <param name="codeLanguage">Код языка</param>
        /// <returns></returns>
        public Pdf GeneratePdf(DBUser currentUser)
        {
            if (currentUser == null) throw new ArgumentNullException("currentUser");

            return GeneratePdf(currentUser, null);
        }

        /// <summary>
        /// Возвращает лист визирования в формате pdf
        /// </summary>
        /// <param name="currentUser">Пользователь из текущего контекста.</param>
        /// <param name="codeLanguage">Код языка</param>
        /// <returns></returns>
        public Pdf GeneratePdf(DBUser currentUser, string codeLanguage)
        {
            if (currentUser == null) throw new ArgumentNullException("currentUser");

            string html = $"<br><br>{GetAgreementSheetHtml(currentUser)}";

            AsposeContext.SetLicense<Aspose.Pdf.License>();
            Pdf pdf = new Pdf();
            pdf.HtmlInfo.PageWidth = 530;
            pdf.SetUnicode();
            Section sec1 = pdf.Sections.Add();
            html = HtmlFormatMoney(html);
            html = HtmlFormatTender(html, codeLanguage);
            html = HtmlFormatRiskLevel(html, codeLanguage);
            html = Regex.Replace(html, @"<img src='[^']+' class='bc_img'/>", "");
            Text text = new Text(html);
            sec1.PageInfo.PageBorderMargin.Left = 0;
            sec1.PageInfo.PageBorderMargin.Top = 35;
            pdf.PageSetup.PageWidth = 500;
            sec1.PageInfo.Margin = new MarginInfo() { Left = 0, Right = 0 };
            sec1.PageInfo.PageWidth = 500;
            text.FixedWidth = 500;
            text.TextWidth = 500;
            text.IsHtmlTagSupported = true;
            text.IsHtml5Supported = true;
            text.TextInfo.IsUnicode = true;
            text.IfHtmlTagSupportedOverwriteHtmlFontNames = true;

            text.IsHtmlTagSupported = true;
            sec1.Paragraphs.Add(text);

            return pdf;
        }

        /// <summary>
        /// Получить html листа согласования в указанном языке
        /// </summary>
        /// <param name="currentUser">Пользователь из текущего контекста.</param>
        /// <returns></returns>
        private string GetAgreementSheetHtml(DBUser currentUser)
        {
            if (currentUser is null)
                throw new ArgumentNullException("currentUser");

            DefaultAgreementSheetBuilderInvoker builder = new DefaultAgreementSheetBuilderInvoker(_item, currentUser);


            string html = builder.GetReportHtml();
            //Затычки на кривоту ASPOSE
            html = $"<style type='text/css'>{style}</style>{html}";
            html = html.Replace("<table cellspac", "<table border=1 width=\"600px\" cellspac");
            return html;
        }

        /// <summary>
        /// Форматирует внутренний текст для элементов с name="tender_type"
        /// </summary>
        /// <param name="html">html-документ страницы листа визирования</param>
        /// <param name="codeLanguage">код языка</param>
        private string HtmlFormatTender(string html, string codeLanguage)
        {
            int startIndex = 0;
            int startReplaceIndex;
            int endReplaceIndex;
            string toReplace = html.Clone().ToString();
            string value;

            while (html.Substring(startIndex).Contains("name=\"tender_type\""))
            {
                startIndex = html.IndexOf("name=\"tender_type\"", startIndex);
                startReplaceIndex = html.IndexOf('>', startIndex);
                endReplaceIndex = html.IndexOf("</td>", startReplaceIndex) - 1;
                toReplace = html.Substring(0, startReplaceIndex + 1);

                value = html.Substring(startReplaceIndex + 1, endReplaceIndex - startReplaceIndex);

                if (value == "&nbsp;")
                {
                    value = string.IsNullOrEmpty(codeLanguage) ? _item.Site.Translator.Translate("Нет необходимости")
                                                               : _item.Site.Translator.Translate("Нет необходимости", null, codeLanguage);
                }


                toReplace += value;
                startIndex = toReplace.Length;
                toReplace += html.Substring(endReplaceIndex + 1);
            }
            return toReplace;
        }

        /// <summary>
        /// Форматирует внутренний текст для элементов с name="risk_level"
        /// </summary>
        /// <param name="html">html-документ страницы листа визирования</param>
        /// <param name="codeLanguage">код языка</param>
        private string HtmlFormatRiskLevel(string html, string codeLanguage)
        {
            int startIndex = 0;
            int startReplaceIndex;
            int endReplaceIndex;
            string toReplace = html.Clone().ToString();
            string value;

            while (html.Substring(startIndex).Contains("name=\"risk_level\""))
            {
                startIndex = html.IndexOf("name=\"risk_level\"", startIndex);
                startReplaceIndex = html.IndexOf('>', startIndex);
                endReplaceIndex = html.IndexOf("</td>", startReplaceIndex) - 1;
                toReplace = html.Substring(0, startReplaceIndex + 1);

                value = html.Substring(startReplaceIndex + 1, endReplaceIndex - startReplaceIndex);
                string[] splitValues = value.Split(',');

                if (splitValues[0] == "&nbsp;" || splitValues[0] == "")
                {
                    value = string.IsNullOrEmpty(codeLanguage) ? _item.Site.Translator.Translate("Нет необходимости")
                                                               : _item.Site.Translator.Translate("Нет необходимости", null, codeLanguage);
                }


                toReplace += value;
                startIndex = toReplace.Length;
                toReplace += html.Substring(endReplaceIndex + 1);
            }
            return toReplace;
        }

        /// <summary>
        /// Форматирует внутренний текст для элементов с name="money_rub"
        /// </summary>
        /// <param name="html">html-документ страницы листа визирования</param>
        private string HtmlFormatMoney(string html)
        {
            int startIndex = 0;
            int startReplaceIndex = 0;
            int endReplaceIndex = 0;
            string ToReplace = html.Clone().ToString();
            string value = string.Empty;
            string resValue = string.Empty;
            int x = 0;

            while (html.Substring(startIndex).Contains("name=\"money_rub\""))
            {
                startIndex = html.IndexOf("name=\"money_rub\"", startIndex);
                startReplaceIndex = html.IndexOf('>', startIndex);
                endReplaceIndex = html.IndexOf("</td>", startReplaceIndex) - 1;
                ToReplace = html.Substring(0, startReplaceIndex + 1);

                value = html.Substring(startReplaceIndex + 1, endReplaceIndex - startReplaceIndex);
                string[] num = string.Join(string.Empty, value.Where(y => Char.IsDigit(y) || y == ',').Select(z => z.ToString()).ToArray()).Split(',');

                string afterDot = string.Empty;
                string beforeDot = string.Empty;
                if (num.Length > 1)
                {
                    beforeDot = num[0];
                    afterDot = num[1];
                }
                else if (num.Length == 1)
                {
                    beforeDot = num[0];
                    afterDot = "00";
                }

                string currency = string.Join(string.Empty, value.Where(Char.IsLetter).Select(z => z.ToString()).ToArray());

                x = 0;
                resValue = $"{resValue}{beforeDot.Substring(0, beforeDot.Length % 3)} ";

                while ((x + 3) <= beforeDot.Length)
                {
                    resValue = $"{resValue}{beforeDot.Substring(beforeDot.Length % 3 + x, 3)} ";
                    x += 3;
                }

                resValue = string.Format("{0},{1} {2}", resValue.Trim(), afterDot, currency);

                ToReplace = $"{ToReplace}{ resValue}";
                startIndex = ToReplace.Length;
                ToReplace = $"{ToReplace}{html.Substring(endReplaceIndex + 1)}";
            }
            return ToReplace;
        }

        /// <summary>
        /// Заплатка ГЛЮКА рендера расчета стилей и размеров 
        /// </summary>
        private const string style = @"
            .h_div {font-family:tahoma;font-size:12px;}
            td.style1{font-weight:bold;width:200px;vertical-align:top;padding-bottom:5px}
            td.style2{width:420px;vertical-align:top;padding-bottom:5px}table#agr_sheet_tbl{width:520px}
                            .headerFirst
                            {
                                border: solid 1px #8aa4bb;
                                text-align: center;
                                padding: 5px;
                                height: 60px;
                                background-color: #f2f2f2;
                                font-size: 10px;
                            }

                            .headerOther
                            {
                                border: solid 1px #8aa4bb;
                                border-left: none;
                                text-align: center;
                                padding: 5px;
                                background-color: #f2f2f2;
                                font-size: 10px;
                            }

                            .cellFirst
                            {
                                border: solid 1px #8aa4bb;
                                border-top: none;
                                text-align: center;
                                padding: 5px;
                                font-size: 10px;
                            }
                            .cellOther
                            {
                                border: solid 1px #8aa4bb;
                                border-left: none;
                                border-top: none;
                                text-align: center;
                                padding: 5px;
                                font-size: 10px;
                            }

                            p, li.MsoNormal, div.MsoNormal
                            {
                                mso-style-unhide: no;
                                mso-style-qformat: yes;
                                mso-style-parent: "";
                                margin-top: 0cm;
                                margin-right: 0cm;
                                margin-bottom: 6.0pt;
                                margin-left: 0cm;
                                mso-pagination: widow-orphan;
                                font-size: 12.0pt;
                                mso-ansi-language: EN-US;
                                mso-fareast-language: EN-US;
                                mso-bidi-language: EN-US;
                            }
                            .div_addAgreement
                            {
                                padding: 25px 0px;
                            }
                            .tbl_header
                            {
                                font-size: 13pt;
                                font-weight: bold;
                                padding-bottom: 7px;
                            }
";

        /// <summary>
        /// Обертка для доступа к коробочному отчету типа "Лист согласования".
        /// </summary>
        private class DefaultAgreementSheetBuilderInvoker : AgreementSheetBuilder
        {
            public DefaultAgreementSheetBuilderInvoker(DBItem item, DBUser user)
                : base(item, user)
            {
            }

            /// <summary>
            /// Получить html листа согласования
            /// </summary>
            /// <returns></returns>
            public string GetReportHtml()
            {
                return base.DrawReport();
            }
        }
    }
}
