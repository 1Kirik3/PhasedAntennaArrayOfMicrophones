using System.Diagnostics;

namespace PAAOM_Server.Services
{
	public class ChangeLogger
	{
		public static void LogChange(string propertyName, object oldValue, object newValue)
		{
			Debug.WriteLine($"[CHANGE] {propertyName}: {oldValue} -> {newValue}");
		}

	}
}
