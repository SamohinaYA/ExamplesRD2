using System;
using System.Xml;
using WSSC.V4.SYS.DBFramework;

namespace WSSC.V4.DMS.CUSTOM.Reports
{
    /// <summary>
    /// Класс получения значений из системной константы
    /// </summary>ы
    internal class ResolutionExtraReportSettings
    {
        internal ResolutionExtraReportSettings(DBItem item)
        {
            _item = item ?? throw new ArgumentNullException(nameof(item));
            _rootElement = ReadRootElement();
        }

        /// <summary>
        /// Объект карточки
        /// </summary>
        private readonly DBItem _item;

        /// <summary>
        /// Xml настройка
        /// </summary>
        private XmlDocument _xmlDoc;

        /// <summary>
        /// Настройка для текущего списка карточки
        /// </summary>
        private readonly XmlNode _rootElement;


        private SolutionRowAttributes? _solutionRowAttributes;
        /// <summary>
        /// Получение объекта с данными из системной константы
        /// </summary>
        internal SolutionRowAttributes SolutionRowAttributes
        {
            get
            {
                if (_solutionRowAttributes is null)
                {
                    _solutionRowAttributes = GetSolutionRowAttributes();
                }
                return _solutionRowAttributes.Value;
            }
        }


        private XmlNode ReadRootElement()
        {
            //_xmlDoc
            string xmlString = _item.Site.ConfigParams.GetStringValue(Consts.Reports.ResolutionsExtraReport.SettingsConstName);
            if (string.IsNullOrEmpty(xmlString))
                throw new ArgumentException($"Не удалось получить системную константу c именем {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");
            _xmlDoc = new XmlDocument();
            _xmlDoc.LoadXml(xmlString);

            //_rootElement
            return _xmlDoc.SelectSingleNode($"Settings/Lists/List[@Name='{_item.List.Name}' and @WebUrl='{_item.Web.RelativeUrl.Trim('/')}']")
                                   ?? throw new Exception($"Не удалось получить узел [Settings/Lists/List] системной конcтанты {Consts.Reports.ResolutionsExtraReport.SettingsConstName} " +
                                   $"для списка '{_item.List.Name}' узла '{_item.Web.RelativeUrl.Trim('/')}'");
        }

        private SolutionRowAttributes GetSolutionRowAttributes()
        {
            XmlNode solutionRow = _rootElement.SelectSingleNode("SolutionRow")
                      ?? throw new Exception($"Не удалось получить узел 'SolutionRow' для списка {_item.List.Name} системной константы {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");
            string solName = solutionRow.Attributes["Name"]?.Value;
            if (String.IsNullOrEmpty(solName))
                throw new Exception($"Не удалось получить значение решения для списка {_item.List.Name} системной константы {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");
            string userFld = solutionRow.Attributes["UsersField"]?.Value;
            if (String.IsNullOrEmpty(userFld))
                throw new Exception($"Не удалось получить значение решения для списка {_item.List.Name} системной константы {Consts.Reports.ResolutionsExtraReport.SettingsConstName}");

            return new SolutionRowAttributes(solName, userFld);
        }
    }

    /// <summary>
    /// Объект с настройками из системной константы
    /// </summary>
    internal readonly struct SolutionRowAttributes
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

