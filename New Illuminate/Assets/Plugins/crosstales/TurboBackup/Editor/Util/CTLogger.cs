﻿
namespace Crosstales.TB.Util
{
    /// <summary>Logger for the asset.</summary>
    public static class CTLogger
    {
        private static string fileMethods = System.IO.Path.GetTempPath() + "TB_Methods.log";
        private static string fileLog = System.IO.Path.GetTempPath() + "TB.log";

        public static void Log(string log)
        {
            System.IO.File.AppendAllText(fileLog, System.DateTime.Now.ToLocalTime() + " - " + log + System.Environment.NewLine);
        }

        public static void BeforeBackup()
        {
            System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - BeforeBackup" + System.Environment.NewLine);
        }

        public static void AfterBackup()
        {
            System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - AfterBackup" + System.Environment.NewLine);
        }

        public static void BeforeRestore()
        {
            System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - BeforeRestore" + System.Environment.NewLine);
        }

        public static void AfterRestore()
        {
            System.IO.File.AppendAllText(fileMethods, System.DateTime.Now.ToLocalTime() + " - AfterRestore" + System.Environment.NewLine);
        }
    }
}
// © 2019 crosstales LLC (https://www.crosstales.com)