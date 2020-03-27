using System;
using System.Xml;
using WSSC.V4.SYS.DBFramework;

namespace WSSC.V4.DMS.EDC.Reports.ResolutionsExtraReport
{
	/// <summary>
	/// Класс получения значений из системной константы
	/// </summary>ы
	class ResolutionExtraReportSettings
	{
        public ResolutionExtraReportSettings(DBItem item)
        {
            //_item
            _item = item ?? throw new ArgumentNullException(nameof(item));

            //_xmlDoc
            string xmlString = _item.Site.ConfigParams.GetStringValue(Consts.Reports.ResolutionsExtraReport.SettingsConstName);
            if (string.IsNullOrEmpty(xmlString))
                throw new ArgumentException($"Не удалось получить системную константу c именем {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");
            _xmlDoc = new XmlDocument();
            _xmlDoc.LoadXml(xmlString);

            //_rootElement
            _rootElement = _xmlDoc.SelectSingleNode($"Settings/Lists/List[@Name='{this._item.List.Name}' and @WebUrl='{this._item.Web.RelativeUrl.Trim('/')}']")
                                   ?? throw new Exception($"Не удалось получить узел [Settings/Lists/List] системной конcтанты {Consts.Reports.ResolutionsExtraReport.SettingsConstName} " +
                                   $"для списка '{this._item.List.Name}' узла '{this._item.Web.RelativeUrl.Trim('/')}'");

            //SolutionRowAttributes
            XmlNode solutionRow = this._rootElement.SelectSingleNode("SolutionRow")
                        ?? throw new Exception($"Не удалось получить узел 'SolutionRow' для списка {this._item.List.Name} системной константы {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");
            string solName = solutionRow.Attributes["Name"]?.Value;
            if (String.IsNullOrEmpty(solName))
                throw new Exception($"Не удалось получить значение решения для списка {this._item.List.Name} системной константы {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");
            string userFld = solutionRow.Attributes["UsersField"]?.Value;
            if (String.IsNullOrEmpty(userFld))
                throw new Exception($"Не удалось получить значение решения для списка {this._item.List.Name} системной константы {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");

            SolutionRowAttributes = new SolutionRowAttributes(solName, userFld);
        }

        /// <summary>
        /// Объект карточки
        /// </summary>
        private readonly DBItem _item;

        /// <summary>
        /// Xml настройка
        /// </summary>
        private readonly XmlDocument _xmlDoc;

        /// <summary>
        /// Настройка для текущего списка карточки
        /// </summary>
        private readonly XmlNode _rootElement;

        /// <summary>
        /// Получение объекта с данными из системной константы
        /// </summary>
        internal readonly SolutionRowAttributes SolutionRowAttributes;
	}

	/// <summary>
	/// Объект с настройками из системной константы
	/// </summary>
	internal struct SolutionRowAttributes
	{
		public SolutionRowAttributes(string solName, string userFLd)
		{
			SolutionName = solName;
			UsersField = userFLd;
		}

        public readonly string SolutionName;
        public readonly string UsersField;
	}
}
