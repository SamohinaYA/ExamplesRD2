using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WSSC.V4.DMS.TMK.EDMS.Utils.RestoreZinstructions
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Manager manager = new Manager();
				manager.Restore();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка:{Environment.NewLine}{ex.ToString()}");
			}
			finally
			{
				Console.WriteLine($"Завершено");
				Console.ReadKey();
			}
		}
	}
}
