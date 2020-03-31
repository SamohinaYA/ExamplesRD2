using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using WSSC.V4.DMS.LNT.Report;
using WSSC.V4.SYS.DBFramework;

namespace WSSC.V4.DMS.CUSTOM.Reports
{
    /// <summary>
    /// Сохранение листа согласования в pdf
    /// </summary>
    public class SaveToPdfPage : IHttpHandler
    {
        //cвойства
        #region prop

        public const bool IsReusable = false;

        private DBAppContext _webContext;
        /// <summary>
        /// Контекст выполнения стриницы.
        /// </summary>
        public DBAppContext WebContext => _webContext
            ?? (_webContext = DBAppContext.Current);


        private int? _listID;
        /// <summary>
        /// Идентификатор списка из параметров страницы.
        /// </summary>
        public int ListID => _listID.Value
            ?? (_listID = WebContext.GetRequestValue<int>("listID"));


        private int? _itemID;
        /// <summary>
        /// Идентификатор элемента из параметров страницы.
        /// </summary>
        public int ItemID => _itemID
            ?? (_itemID = WebContext.GetRequestValue<int>("itemID"));


        private bool _InitList = false;
        private DBList _list;
        /// <summary>
        /// Список, загруженный по переданному в параметрах идентификатору listId
        /// </summary>
        public DBList List => _list
            ?? (_list = WebContext.Site.GetList(ListID, true));



        private DBItem _item;
        /// <summary>
        /// Документ, загруженный по переданному в параметрах идентификатору itemId
        /// </summary>
        public DBItem Item
        {
            get
            {
                if (_item is null || ItemID > 0)
                {
                    _item = List.GetItem(ItemID);
                }
                return _item
                    ?? throw new DBException.MissingItem(List, ItemID);
            }
        }
        #endregion

        /// <summary>
        /// Генерация листа согласования в pdf
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {

            SaveToPdfAdapter adapter = new SaveToPdfAdapter(Item);
            try
            {
                Aspose.Pdf.Generator.Pdf pdf = adapter.GeneratePdf(WebContext.CurrentUser);

                context.Response.ClearHeaders();
                context.Response.ClearContent();

                if (string.IsNullOrEmpty(context.Response.Headers["сache-сontrol"]))
                {
                    context.Response.Headers["cache-control"] = "no-store";
                }
                else
                {
                    context.Response.Headers["cache-control"] += ", no-store";
                }
                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.HeaderEncoding = Encoding.UTF8;


                //Вместо открытия файла в браузере - сохраняем файл 
                using (MemoryStream stream = new MemoryStream())
                {

                    //Формируем имя файла под все варианты
                    string fileName = string.Format("Лист согласования {0}.pdf", Item.GetStringValue(Consts.Lists.CommonFields.RegNumber));
                    string fileNameEncoded = Uri.EscapeDataString(fileName).Replace("'", "%27");
                    string downloadName;
                    if (WebContext.BrowserIs.IE && WebContext.BrowserIs.IEVersion <= 10)
                        downloadName = string.Format("filename=\"{0}\"", fileNameEncoded);
                    else
                        downloadName = string.Format("filename*=UTF-8''{0}", fileNameEncoded);

                    pdf.Save(stream);
                    byte[] fileContent = stream.ToArray();
                    context.Response.ClearContent();
                    context.Response.ClearHeaders();
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.AddHeader("Content-Disposition", string.Format("attachment; {0}", downloadName));
                    context.Response.AppendHeader("Content-Length", fileContent.Length.ToString());
                    context.Response.Flush();
                    context.Response.OutputStream.Write(fileContent, 0, fileContent.Length);
                    context.Response.Flush();
                    context.Response.End();
                }

            }
            catch (ThreadAbortException) { }   //Aspose всегда выдает ThreadAbortException
            catch (Exception ex)
            {
                context.Response.Write(ex.ToString());
            }
            finally
            {
                adapter.Dispose();
            }
        }
    }
}
