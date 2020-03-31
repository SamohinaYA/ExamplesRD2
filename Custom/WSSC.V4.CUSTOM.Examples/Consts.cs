namespace WSSC.V4.CUSTOM.Examples
{
	internal sealed class Consts
	{
		public static class DeliveringGroups
		{
			public const string GuidField = "AD GUID";
			public const string DBGroup = "Группа";
			public const string ADNameField = "Название группы в AD";
			public const string NameField = "Название группы";
			public const string EmailNameField = "Email рассылки";
			public const string CreatedFromADField = "Создано из AD";
			public static string ListName = "DeliveringList";
			public static string Deleted = "Элемент удален";
			public const string NotActual = "Не актуален";
			public const string AccessRestriction = "Ограничение на доступ";
		}

		/// <summary>
		/// Списки.
		/// </summary>
		public class Lists
		{
			/// <summary>
			/// «WSSC_Доступ к группам рассылки»
			/// </summary>
			public class AccessOfDelivering
			{
				public const string ListName = "WSSC_DeliveringListAccess";
				public class Fields
				{
					public const string DeliverinGroups = "Группы рассылки";
					public const string Users = "Пользователи";
					public const string UsersGroups = "Группы";
				}
			}
		}
	}
}
