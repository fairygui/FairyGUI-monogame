using System;
using System.Diagnostics;

namespace FairyGUI.Utils
{
	public class Log
	{
		public static void Warning(string msg)
		{
			Trace.TraceWarning(msg);
		}

		public static void Info(string msg)
		{
			Trace.WriteLine(msg);
		}

		public static void Exception(Exception exception)
		{
			Trace.TraceError(exception.Message);
		}

		public static void Error(string msg)
		{
			Trace.TraceError(msg);
		}
	}
}