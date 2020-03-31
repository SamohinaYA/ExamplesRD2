using System;
using System.Text;
using WSSC.V4.SYS.DBFramework;

namespace WSSC.V4.DMS.CUSTOM.Reports
{
    /// <summary>
    /// Кастомный лист согласования. Для сохранения кнопки в pdf вместо doc.
    /// </summary>
    internal class ReplaceSaveDocToPdf
    {
        private const string HtmlButtonSaveToPdf = @"
<script> 
    var wordHref =  document.getElementById('HyperLink1')
    if(wordHref == null)
        alert('Не найден элемент ссылки на формирование отчета в word');
    wordHref.setAttribute('href','/_LAYOUTS/WSS/WSSC.V4.DMS.LFR/Reports/SaveToPdf/SaveToPdfPage.ashx?itemID={0}&listID={1}'); 
    wordHref.setAttribute('style', 'z-index: 99');
    wordHref.innerHTML='Сохранить в PDF';
    document.title = 'Лист визирования';
</script>";


        /// <summary>
        /// Заменяем кнопку сохранить в word на pdf.
        /// </summary>
        internal string GetBoundDocumentsHtml(string html, DBItem item)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentNullException(nameof(html));

            if (item == null)
                throw new ArgumentNullException(nameof(item));


            StringBuilder result = new StringBuilder();
            result.Append(html);
            result.AppendFormat(Consts.Report.HtmlButtonSaveToPdf, item.ID, item.List.ID);

            return result.ToString();
        }
    }
}