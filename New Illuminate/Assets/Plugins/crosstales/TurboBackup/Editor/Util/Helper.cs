using UnityEngine;
using UnityEditor;

namespace Crosstales.TB.Util
{
    /// <summary>Various helper functions.</summary>
    public abstract class Helper : Common.EditorUtil.BaseEditorHelper
    {

        #region Static variables

        private static Texture2D action_backup;
        private static Texture2D action_restore;

        private static Texture2D logo_asset;
        private static Texture2D logo_asset_small;

        private static string scanInfo;
        private static bool isScanning = false;
        public static bool isDeleting = false;

        #endregion


        #region Static properties

        public static Texture2D Action_Backup
        {
            get
            {
                return loadImage(ref action_backup, "action_backup.png");
            }
        }
        public static Texture2D Action_Restore
        {
            get
            {
                return loadImage(ref action_restore, "action_restore.png");
            }
        }

        public static Texture2D Logo_Asset
        {
            get
            {
                return loadImage(ref logo_asset, "logo_asset_pro.png");
            }
        }

        public static Texture2D Logo_Asset_Small
        {
            get
            {
                return loadImage(ref logo_asset_small, "logo_asset_small_pro.png");
            }
        }

        /// <summary>Checks if the backup for the project is enabled.</summary>
        /// <returns>True if a backup is enabled</returns>
        public static bool isBackupEnabled
        {
            get
            {
                return Config.COPY_ASSETS || Config.COPY_LIBRARY || Config.COPY_SETTINGS || Config.COPY_PACKAGES;
            }
        }

        /// <summary>Checks if a backup for the project exists.</summary>
        /// <returns>True if a backup for the project exists</returns>
        public static bool hasBackup
        {
            get
            {
                return System.IO.Directory.Exists(Config.PATH_BACKUP);
            }
        }

        /// <summary>Scans the backup usage information.</summary>
        /// <returns>Backup usage information.</returns>
        public static string BackupInfo
        {
            get
            {
                string result = Constants.TEXT_NO_BACKUP;

                if (hasBackup)
                {
                    if (!string.IsNullOrEmpty(scanInfo))
                    {
                        result = scanInfo;
                    }
                    else
                    {
                        if (!isScanning)
                        {
                            isScanning = true;

                            if (System.IO.Directory.Exists(Config.PATH_BACKUP))
                            {
                                System.Threading.Thread worker;
                                if (isWindowsEditor)
                                {
                                    worker = new System.Threading.Thread(() => scanWindows(Config.PATH_BACKUP, ref scanInfo));
                                }
                                else
                                {
                                    worker = new System.Threading.Thread(() => scanUnix(Config.PATH_BACKUP, ref scanInfo));
                                }
                                worker.Start();
                            }
                        }
                        else
                        {
                            result = "Scanning...";
                        }
                    }
                }

                return result;
            }
        }

        #endregion


        #region Public static methods

        /// <summary>Backup the project.</summary>
        public static void Backup()
        {
            if (!Config.CUSTOM_PATH_BACKUP && Config.VCS != 0)
            {
                if (Config.VCS == 1)
                {
                    // git
                    if (System.IO.File.Exists(Constants.APPLICATION_PATH + ".gitignore"))
                    {
                        string content = System.IO.File.ReadAllText(Constants.APPLICATION_PATH + ".gitignore");

                        if (!content.Contains(Constants.BACKUP_DIRNAME + "/"))
                        {
                            System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".gitignore", content.TrimEnd() + System.Environment.NewLine + Constants.BACKUP_DIRNAME + "/");
                        }
                    }
                    else
                    {
                        System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".gitignore", Constants.BACKUP_DIRNAME + "/");
                    }
                }
                else if (Config.VCS == 2)
                {
                    // svn
                    using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                    {
                        process.StartInfo.FileName = "svn";
                        process.StartInfo.Arguments = "propset svn: ignore " + Constants.BACKUP_DIRNAME + ".";
                        process.StartInfo.WorkingDirectory = Constants.APPLICATION_PATH;
                        process.StartInfo.UseShellExecute = false;

                        try
                        {
                            process.Start();
                            process.WaitForExit(Constants.PROCESS_KILL_TIME);
                        }
                        catch (System.Exception ex)
                        {
                            string errorMessage = "Could execute svn-ignore! Please do it manually in the console: 'svn propset svn:ignore " + Constants.BACKUP_DIRNAME + ".'" + System.Environment.NewLine + ex;
                            Debug.LogError(errorMessage);
                        }
                    }
                }
                else if (Config.VCS == 3)
                {
                    // mercurial
                    Debug.LogError("Mercurial currently not supported. Please add the following lines to your .hgignore: " + System.Environment.NewLine + "syntax: glob" + System.Environment.NewLine + Constants.BACKUP_DIRNAME + "/**");
                }
                else
                {
                    // git
                    if (System.IO.File.Exists(Constants.APPLICATION_PATH + ".collabignore"))
                    {
                        string content = System.IO.File.ReadAllText(Constants.APPLICATION_PATH + ".collabignore");

                        if (!content.Contains(Constants.BACKUP_DIRNAME + "/"))
                        {
                            System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".collabignore", content.TrimEnd() + System.Environment.NewLine + Constants.BACKUP_DIRNAME + "/");
                        }
                    }
                    else
                    {
                        System.IO.File.WriteAllText(Constants.APPLICATION_PATH + ".collabignore", Constants.BACKUP_DIRNAME + "/");
                    }
                }
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            bool success = false;
            string scriptfile = string.Empty;

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_PRE_BACKUP))
                InvokeMethod(Config.EXECUTE_METHOD_PRE_BACKUP.Substring(0, Config.EXECUTE_METHOD_PRE_BACKUP.LastIndexOf(".")), Config.EXECUTE_METHOD_PRE_BACKUP.Substring(Config.EXECUTE_METHOD_PRE_BACKUP.LastIndexOf(".") + 1));

            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                try
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;

                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TB-Backup_" + System.Guid.NewGuid() + ".cmd";

                        System.IO.File.WriteAllText(scriptfile, generateWindowsBackupScript(Config.PATH_BACKUP));

                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = "/c start  \"\" " + '"' + scriptfile + '"';
                    }
                    else if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TB-Backup_" + System.Guid.NewGuid() + ".sh";

                        System.IO.File.WriteAllText(scriptfile, generateMacBackupScript(Config.PATH_BACKUP));

                        process.StartInfo.FileName = "/bin/sh";
                        process.StartInfo.Arguments = '"' + scriptfile + "\" &";
                    }
                    else if (Application.platform == RuntimePlatform.LinuxEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TB-Backup_" + System.Guid.NewGuid() + ".sh";

                        System.IO.File.WriteAllText(scriptfile, generateLinuxBackupScript(Config.PATH_BACKUP));

                        process.StartInfo.FileName = "/bin/sh";
                        process.StartInfo.Arguments = '"' + scriptfile + "\" &";
                    }
                    else
                    {
                        Debug.LogError("Unsupported Unity Editor: " + Application.platform);
                        return;
                    }

                    Config.BACKUP_DATE = System.DateTime.Now;
                    Config.BACKUP_COUNT++;
                    Config.Save();

                    process.Start();

                    if (isWindowsEditor)
                        process.WaitForExit(Constants.PROCESS_KILL_TIME);

                    success = true;
                }
                catch (System.Exception ex)
                {
                    string errorMessage = "Could execute TB!" + System.Environment.NewLine + ex;
                    Debug.LogError(errorMessage);
                }
            }

            if (success)
                EditorApplication.Exit(0);
        }

        /// <summary>Restore the project.</summary>
        public static void Restore()
        {
            UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            bool success = false;
            string scriptfile = string.Empty;

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_PRE_RESTORE))
                InvokeMethod(Config.EXECUTE_METHOD_PRE_RESTORE.Substring(0, Config.EXECUTE_METHOD_PRE_RESTORE.LastIndexOf(".")), Config.EXECUTE_METHOD_PRE_RESTORE.Substring(Config.EXECUTE_METHOD_PRE_RESTORE.LastIndexOf(".") + 1));

            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                try
                {
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;

                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TB-Restore_" + System.Guid.NewGuid() + ".cmd";

                        System.IO.File.WriteAllText(scriptfile, generateWindowsRestoreScript(Config.PATH_BACKUP));

                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.Arguments = "/c start  \"\" " + '"' + scriptfile + '"';
                    }
                    else if (Application.platform == RuntimePlatform.OSXEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TB-Restore_" + System.Guid.NewGuid() + ".sh";

                        System.IO.File.WriteAllText(scriptfile, generateMacRestoreScript(Config.PATH_BACKUP));

                        process.StartInfo.FileName = "/bin/sh";
                        process.StartInfo.Arguments = '"' + scriptfile + "\" &";
                    }
                    else if (Application.platform == RuntimePlatform.LinuxEditor)
                    {
                        scriptfile = System.IO.Path.GetTempPath() + "TB-Restore_" + System.Guid.NewGuid() + ".sh";

                        System.IO.File.WriteAllText(scriptfile, generateLinuxRestoreScript(Config.PATH_BACKUP));

                        process.StartInfo.FileName = "/bin/sh";
                        process.StartInfo.Arguments = '"' + scriptfile + "\" &";
                    }
                    else
                    {
                        Debug.LogError("Unsupported Unity Editor: " + Application.platform);
                        return;
                    }

                    Config.RESTORE_DATE = System.DateTime.Now;
                    Config.RESTORE_COUNT++;
                    Config.Save();

                    process.Start();

                    if (isWindowsEditor)
                        process.WaitForExit(Constants.PROCESS_KILL_TIME);

                    success = true;
                }
                catch (System.Exception ex)
                {
                    string errorMessage = "Could execute TB!" + System.Environment.NewLine + ex;
                    Debug.LogError(errorMessage);
                }
            }

            if (success)
                EditorApplication.Exit(0);
        }

        /// <summary>Delete the backup for all platforms.</summary>
        public static void DeleteBackup()
        {
            if (!isDeleting && System.IO.Directory.Exists(Config.PATH_BACKUP))
            {
                isDeleting = true;

                System.Threading.Thread worker = new System.Threading.Thread(() => deleteBackup());
                worker.Start();
            }
        }

        /*
        /// <summary>Delete all shell-scripts after a platform switch.</summary>
        public static void DeleteAllScripts()
        {
            //INFO: currently disabled since it could interfer with running scripts!

            DirectoryInfo dir = new DirectoryInfo(Path.GetTempPath());

            try
            {
                foreach (FileInfo file in dir.GetFiles("TPS-" + Constants.ASSET_ID + "*"))
                {
                    if (Constants.DEBUG)
                        Debug.Log("Script file deleted: " + file);

                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Could not delete all script files!" + Environment.NewLine + ex);
            }
        }
        */

        #endregion


        #region Private static methods

        private static void scanWindows(string path, ref string key)
        {

            using (System.Diagnostics.Process scanProcess = new System.Diagnostics.Process())
            {

                string args = "/c dir * /s /a";

                if (Config.DEBUG)
                    Debug.Log("Process arguments: '" + args + "'");

                System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

                scanProcess.StartInfo.FileName = "cmd.exe";
                scanProcess.StartInfo.WorkingDirectory = path;
                scanProcess.StartInfo.Arguments = args;
                scanProcess.StartInfo.CreateNoWindow = true;
                scanProcess.StartInfo.RedirectStandardOutput = true;
                scanProcess.StartInfo.RedirectStandardError = true;
                scanProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
                scanProcess.StartInfo.UseShellExecute = false;
                scanProcess.OutputDataReceived += (sender, eventArgs) =>
                {
                    result.Add(eventArgs.Data);
                };

                bool success = true;

                try
                {
                    scanProcess.Start();
                    scanProcess.BeginOutputReadLine();
                }
                catch (System.Exception ex)
                {
                    success = false;
                    Debug.LogError("Could not start the scan process!" + System.Environment.NewLine + ex);
                }

                if (success)
                {
                    do
                    {
                        System.Threading.Thread.Sleep(50);
                    } while (!scanProcess.HasExited);

                    if (scanProcess.ExitCode == 0)
                    {
                        if (Config.DEBUG)
                            Debug.LogWarning("Scan completed: " + result.Count);

                        key = result[result.Count - 3].Trim();
                    }
                    else
                    {
                        using (System.IO.StreamReader sr = scanProcess.StandardError)
                        {
                            Debug.LogError("Could not scan the path: " + scanProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd());
                        }
                    }
                }
            }
        }

        private static void scanUnix(string path, ref string key)
        {
            using (System.Diagnostics.Process scanProcess = new System.Diagnostics.Process())
            {
                string args = "-sch \"" + path + '"';

                if (Config.DEBUG)
                    Debug.Log("Process arguments: '" + args + "'");

                System.Collections.Generic.List<string> result = new System.Collections.Generic.List<string>();

                scanProcess.StartInfo.FileName = "du";
                scanProcess.StartInfo.Arguments = args;
                scanProcess.StartInfo.CreateNoWindow = true;
                scanProcess.StartInfo.RedirectStandardOutput = true;
                scanProcess.StartInfo.RedirectStandardError = true;
                scanProcess.StartInfo.StandardOutputEncoding = System.Text.Encoding.Default;
                scanProcess.StartInfo.UseShellExecute = false;
                scanProcess.OutputDataReceived += (sender, eventArgs) =>
                {
                    result.Add(eventArgs.Data);
                };

                bool success = true;

                try
                {
                    scanProcess.Start();
                    scanProcess.BeginOutputReadLine();
                }
                catch (System.Exception ex)
                {
                    success = false;
                    Debug.LogError("Could not start the scan process!" + System.Environment.NewLine + ex);
                }

                if (success)
                {

                    while (!scanProcess.HasExited)
                    {
                        System.Threading.Thread.Sleep(50);
                    }

                    if (scanProcess.ExitCode == 0)
                    {
                        if (Config.DEBUG)
                            Debug.LogWarning("Scan completed: " + result.Count);

                        key = result[result.Count - 2].Trim();
                    }
                    else
                    {
                        using (System.IO.StreamReader sr = scanProcess.StandardError)
                        {
                            Debug.LogError("Could not scan the path: " + scanProcess.ExitCode + System.Environment.NewLine + sr.ReadToEnd());
                        }
                    }
                }
            }
        }

        private static void deleteBackup()
        {
            try
            {
                System.IO.Directory.Delete(Config.PATH_BACKUP, true);

                Config.BACKUP_COUNT = 0;
                Config.RESTORE_COUNT = 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Could not delete the backup!" + System.Environment.NewLine + ex);
            }

            isDeleting = false;
        }


        #region Windows

        private static string generateWindowsBackupScript(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("@echo off");
            sb.AppendLine("cls");

            // title
            sb.Append("title ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Backup of ");
            sb.Append(Application.productName);
            sb.AppendLine(" in progress - DO NOT CLOSE THIS WINDOW!");

            // header
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #                                                                            #");
            sb.Append("echo #  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - Windows                                       #");
            sb.AppendLine("echo #  Copyright 2018-2019 by www.crosstales.com                                 #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo #  The files will now be saved to the backup destination.                    #");
            sb.AppendLine("echo #  This will take some time, so please be patient and DON'T CLOSE THIS       #");
            sb.AppendLine("echo #  WINDOW before the process is finished!                                    #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo #  Unity will restart automatically after the backup.                        #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo " + Application.productName);
            sb.AppendLine("echo.");
            sb.AppendLine("echo.");

            // check if Unity is closed
            sb.AppendLine(":waitloop");
            sb.Append("if not exist \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp\\UnityLockfile\" goto waitloopend");
            sb.AppendLine();
            sb.AppendLine("echo.");
            sb.AppendLine("echo Waiting for Unity to close...");
            sb.AppendLine("timeout /t 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("del \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp\\UnityLockfile\" /q");
                sb.AppendLine();
            }

            sb.AppendLine("goto waitloop");
            sb.AppendLine(":waitloopend");

            // Save files
            sb.AppendLine("echo.");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #  Saving files                                                              #");
            sb.AppendLine("echo ##############################################################################");

            // Assets
            if (Config.COPY_ASSETS)
            {
                sb.Append("robocopy \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Assets\" \"");
                sb.Append(path);
                sb.Append("\\Assets");
                sb.AppendLine("\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }

            // Library
            if (Config.COPY_LIBRARY)
            {
                sb.Append("robocopy \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Library\" \"");
                sb.Append(path);
                sb.Append("\\Library");
                sb.AppendLine("\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }

            // ProjectSettings
            if (Config.COPY_SETTINGS)
            {
                sb.Append("robocopy \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("ProjectSettings\" \"");
                sb.Append(path);
                sb.Append("\\ProjectSettings");
                sb.AppendLine("\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }

#if UNITY_2017_4_OR_NEWER
            // Packages
            if (Config.COPY_PACKAGES)
            {
                sb.Append("robocopy \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Packages\" \"");
                sb.Append(path);
                sb.Append("\\Packages");
                sb.AppendLine("\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }
#endif

            // Restart Unity
            sb.AppendLine("echo.");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #  Restarting Unity                                                          #");
            sb.AppendLine("echo ##############################################################################");
            sb.Append("start \"\" \"");
            sb.Append(Helper.ValidatePath(EditorApplication.applicationPath, false));
            sb.Append("\" -projectPath \"");
            sb.Append(Constants.APPLICATION_PATH.Substring(0, Constants.APPLICATION_PATH.Length - 1));
            sb.Append("\"");

            if (Config.BATCHMODE)
            {
                sb.Append(" -batchmode");

                if (Config.QUIT)
                    sb.Append(" -quit");

                if (Config.NO_GRAPHICS)
                    sb.Append(" -nographics");
            }

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_BACKUP))
            {
                sb.Append(" -executeMethod ");
                sb.Append(Config.EXECUTE_METHOD_BACKUP);
            }

            sb.AppendLine();
            sb.AppendLine("echo.");

            // check if Unity is started
            sb.AppendLine(":waitloop2");
            sb.Append("if exist \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp\\UnityLockfile\" goto waitloopend2");
            sb.AppendLine();
            sb.AppendLine("echo Waiting for Unity to start...");
            sb.AppendLine("timeout /t 3");
            sb.AppendLine("goto waitloop2");
            sb.AppendLine(":waitloopend2");
            sb.AppendLine("echo.");
            sb.AppendLine("echo Bye!");
            sb.AppendLine("timeout /t 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        private static string generateWindowsRestoreScript(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("@echo off");
            sb.AppendLine("cls");

            // title
            sb.Append("title ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Restore of ");
            sb.Append(Application.productName);
            sb.AppendLine(" in progress - DO NOT CLOSE THIS WINDOW!");

            // header
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #                                                                            #");
            sb.Append("echo #  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - Windows                                       #");
            sb.AppendLine("echo #  Copyright 2018-2019 by www.crosstales.com                                 #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo #  The files will now be restored from the backup destination.               #");
            sb.AppendLine("echo #  This will take some time, so please be patient and DON'T CLOSE THIS       #");
            sb.AppendLine("echo #  WINDOW before the process is finished!                                    #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo #  Unity will restart automatically after the restore.                       #");
            sb.AppendLine("echo #                                                                            #");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo " + Application.productName);
            sb.AppendLine("echo.");
            sb.AppendLine("echo.");

            // check if Unity is closed
            sb.AppendLine(":waitloop");
            sb.Append("if not exist \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp\\UnityLockfile\" goto waitloopend");
            sb.AppendLine();
            sb.AppendLine("echo.");
            sb.AppendLine("echo Waiting for Unity to close...");
            sb.AppendLine("timeout /t 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("del \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp\\UnityLockfile\" /q");
                sb.AppendLine();
            }

            sb.AppendLine("goto waitloop");
            sb.AppendLine(":waitloopend");

            // Restore files
            sb.AppendLine("echo.");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #  Restoring files                                                           #");
            sb.AppendLine("echo ##############################################################################");

            // Assets
            if (Config.COPY_ASSETS)
            {
                sb.Append("robocopy \"");
                sb.Append(path);
                sb.Append("\\Assets\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Assets\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }

            // Library
            if (Config.COPY_LIBRARY)
            {
                sb.Append("robocopy \"");
                sb.Append(path);
                sb.Append("\\Library\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Library\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }

            // ProjectSettings
            if (Config.COPY_SETTINGS)
            {
                sb.Append("robocopy \"");
                sb.Append(path);
                sb.Append("\\ProjectSettings\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("ProjectSettings\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }

#if UNITY_2017_4_OR_NEWER
            // Packages
            if (Config.COPY_PACKAGES)
            {
                sb.Append("robocopy \"");
                sb.Append(path);
                sb.Append("\\Packages\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Packages\" /MIR /W:3 /MT /NFL /NDL /NJH /NJS /nc /ns /np > NUL");
            }
#endif

            // Restart Unity
            sb.AppendLine("echo.");
            sb.AppendLine("echo ##############################################################################");
            sb.AppendLine("echo #  Restarting Unity                                                          #");
            sb.AppendLine("echo ##############################################################################");
            sb.Append("start \"\" \"");
            sb.Append(Helper.ValidatePath(EditorApplication.applicationPath, false));
            sb.Append("\" -projectPath \"");
            sb.Append(Constants.APPLICATION_PATH.Substring(0, Constants.APPLICATION_PATH.Length - 1));
            sb.Append("\"");

            if (Config.BATCHMODE)
            {
                sb.Append(" -batchmode");

                if (Config.QUIT)
                    sb.Append(" -quit");

                if (Config.NO_GRAPHICS)
                    sb.Append(" -nographics");
            }

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_RESTORE))
            {
                sb.Append(" -executeMethod ");
                sb.Append(Config.EXECUTE_METHOD_RESTORE);
            }

            sb.AppendLine();
            sb.AppendLine("echo.");

            // check if Unity is started
            sb.AppendLine(":waitloop2");
            sb.Append("if exist \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp\\UnityLockfile\" goto waitloopend2");
            sb.AppendLine();
            sb.AppendLine("echo Waiting for Unity to start...");
            sb.AppendLine("timeout /t 3");
            sb.AppendLine("goto waitloop2");
            sb.AppendLine(":waitloopend2");
            sb.AppendLine("echo.");
            sb.AppendLine("echo Bye!");
            sb.AppendLine("timeout /t 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        #endregion


        #region Mac

        private static string generateMacBackupScript(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine("set +v");
            sb.AppendLine("clear");

            // title
            sb.Append("title='");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Backup of ");
            sb.Append(Application.productName);
            sb.AppendLine(" in progress - DO NOT CLOSE THIS WINDOW!'");
            sb.AppendLine("echo -n -e \"\\033]0;$title\\007\"");

            // header
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.Append("echo \"¦  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - macOS                                         ¦\"");
            sb.AppendLine("echo \"¦  Copyright 2018-2019 by www.crosstales.com                                 ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  The files will now be saved to the backup destination.                    ¦\"");
            sb.AppendLine("echo \"¦  This will take some time, so please be patient and DON'T CLOSE THIS       ¦\"");
            sb.AppendLine("echo \"¦  WINDOW before the process is finished!                                    ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  Unity will restart automatically after the backup.                        ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"" + Application.productName + "\"");
            sb.AppendLine("echo");
            sb.AppendLine("echo");

            // check if Unity is closed
            sb.Append("while [ -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to close...\"");
            sb.AppendLine("  sleep 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("  rm \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp/UnityLockfile\"");
                sb.AppendLine();
            }

            sb.AppendLine("done");

            // Save files
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Saving files                                                              ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");

            // Assets
            if (Config.COPY_ASSETS)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("Assets");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Assets\" \"");
                sb.Append(path);
                sb.AppendLine("\"");
            }

            // Library
            if (Config.COPY_LIBRARY)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("Library");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Library\" \"");
                sb.Append(path);
                sb.AppendLine("\"");
            }

            // ProjectSettings
            if (Config.COPY_SETTINGS)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("ProjectSettings");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("ProjectSettings\" \"");
                sb.Append(path);
                sb.AppendLine("\"");
            }

#if UNITY_2017_4_OR_NEWER
            // Packages
            if (Config.COPY_PACKAGES)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("Packages");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Packages\" \"");
                sb.Append(path);
                sb.AppendLine("\"");
            }
#endif

            // Restart Unity
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Restarting Unity                                                          ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.Append("open -a \"");
            sb.Append(EditorApplication.applicationPath);
            sb.Append("\" --args -projectPath \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("\"");

            if (Config.BATCHMODE)
            {
                sb.Append(" -batchmode");

                if (Config.QUIT)
                    sb.Append(" -quit");

                if (Config.NO_GRAPHICS)
                    sb.Append(" -nographics");
            }

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_BACKUP))
            {
                sb.Append(" -executeMethod ");
                sb.Append(Config.EXECUTE_METHOD_BACKUP);
            }

            sb.AppendLine();

            //check if Unity is started
            sb.AppendLine("echo");
            sb.Append("while [ ! -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to start...\"");
            sb.AppendLine("  sleep 3");
            sb.AppendLine("done");
            sb.AppendLine("echo");
            sb.AppendLine("echo \"Bye!\"");
            sb.AppendLine("sleep 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        private static string generateMacRestoreScript(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine("set +v");
            sb.AppendLine("clear");

            // title
            sb.Append("title='");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Restore of ");
            sb.Append(Application.productName);
            sb.AppendLine(" in progress - DO NOT CLOSE THIS WINDOW!'");
            sb.AppendLine("echo -n -e \"\\033]0;$title\\007\"");

            // header
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.Append("echo \"¦  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - macOS                                         ¦\"");
            sb.AppendLine("echo \"¦  Copyright 2018-2019 by www.crosstales.com                                 ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  The files will now be restored from the backup destination.               ¦\"");
            sb.AppendLine("echo \"¦  This will take some time, so please be patient and DON'T CLOSE THIS       ¦\"");
            sb.AppendLine("echo \"¦  WINDOW before the process is finished!                                    ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  Unity will restart automatically after the restore.                       ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"" + Application.productName + "\"");
            sb.AppendLine("echo");
            sb.AppendLine("echo");

            // check if Unity is closed
            sb.Append("while [ -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to close...\"");
            sb.AppendLine("  sleep 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("  rm \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp/UnityLockfile\"");
                sb.AppendLine();
            }

            sb.AppendLine("done");

            // Restore files
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Restoring files                                                           ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");

            // Assets
            if (Config.COPY_ASSETS)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("Assets");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Assets/\"");
            }

            // Library
            if (Config.COPY_LIBRARY)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("Library");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Library/\"");
            }

            // ProjectSettings
            if (Config.COPY_SETTINGS)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("ProjectSettings");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("ProjectSettings/\"");
            }

#if UNITY_2017_4_OR_NEWER            
            // Packages
            if (Config.COPY_PACKAGES)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("Packages");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Packages/\"");
            }
#endif

            // Restart Unity
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Restarting Unity                                                          ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.Append("open -a \"");
            sb.Append(EditorApplication.applicationPath);
            sb.Append("\" --args -projectPath \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("\"");

            if (Config.BATCHMODE)
            {
                sb.Append(" -batchmode");

                if (Config.QUIT)
                    sb.Append(" -quit");

                if (Config.NO_GRAPHICS)
                    sb.Append(" -nographics");
            }

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_RESTORE))
            {
                sb.Append(" -executeMethod ");
                sb.Append(Config.EXECUTE_METHOD_RESTORE);
            }

            sb.AppendLine();

            //check if Unity is started
            sb.AppendLine("echo");
            sb.Append("while [ ! -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to start...\"");
            sb.AppendLine("  sleep 3");
            sb.AppendLine("done");
            sb.AppendLine("echo");
            sb.AppendLine("echo \"Bye!\"");
            sb.AppendLine("sleep 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        #endregion


        #region Linux

        private static string generateLinuxBackupScript(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine("set +v");
            sb.AppendLine("clear");

            // title
            sb.Append("title='");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Backup of ");
            sb.Append(Application.productName);
            sb.AppendLine(" in progress - DO NOT CLOSE THIS WINDOW!'");
            sb.AppendLine("echo -n -e \"\\033]0;$title\\007\"");

            // header
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.Append("echo \"¦  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - Linux                                         ¦\"");
            sb.AppendLine("echo \"¦  Copyright 2018-2019 by www.crosstales.com                                 ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  The files will now be saved to the backup destination.                    ¦\"");
            sb.AppendLine("echo \"¦  This will take some time, so please be patient and DON'T CLOSE THIS       ¦\"");
            sb.AppendLine("echo \"¦  WINDOW before the process is finished!                                    ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  Unity will restart automatically after the backup.                        ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"" + Application.productName + "\"");
            sb.AppendLine("echo");
            sb.AppendLine("echo");

            // check if Unity is closed
            sb.Append("while [ -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to close...\"");
            sb.AppendLine("  sleep 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("  rm \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp/UnityLockfile\"");
                sb.AppendLine();
            }

            sb.AppendLine("done");

            // Save files
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Saving files                                                              ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");

            // Assets
            if (Config.COPY_ASSETS)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("Assets");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Assets/\" \"");
                sb.Append(path);
                //sb.Append("\"");
                sb.AppendLine("Assets/\"");
            }

            // Library
            if (Config.COPY_LIBRARY)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("Library");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Library/\" \"");
                sb.Append(path);
                //sb.Append("\"");
                sb.AppendLine("Library/\"");
            }

            // ProjectSettings
            if (Config.COPY_SETTINGS)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("ProjectSettings");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("ProjectSettings/\" \"");
                sb.Append(path);
                //sb.Append("\"");
                sb.AppendLine("ProjectSettings/\"");
            }

#if UNITY_2017_4_OR_NEWER
            // Packages
            if (Config.COPY_PACKAGES)
            {
                sb.Append("mkdir -p \"");
                sb.Append(path);
                sb.Append("Packages");
                sb.Append('"');
                sb.AppendLine();
                sb.Append("rsync -aq --delete \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Packages/\" \"");
                sb.Append(path);
                //sb.Append("\"");
                sb.AppendLine("Packages/\"");
            }
#endif

            // Restart Unity
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Restarting Unity                                                          ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            //sb.Append("nohup \"");
            sb.Append('"');
            sb.Append(EditorApplication.applicationPath);
            sb.Append("\" --args -projectPath \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("\"");

            if (Config.BATCHMODE)
            {
                sb.Append(" -batchmode");

                if (Config.QUIT)
                    sb.Append(" -quit");

                if (Config.NO_GRAPHICS)
                    sb.Append(" -nographics");
            }

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_BACKUP))
            {
                sb.Append(" -executeMethod ");
                sb.Append(Config.EXECUTE_METHOD_BACKUP);
            }

            sb.Append(" &");
            sb.AppendLine();

            // check if Unity is started
            sb.AppendLine("echo");
            sb.Append("while [ ! -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to start...\"");
            sb.AppendLine("  sleep 3");
            sb.AppendLine("done");
            sb.AppendLine("echo");
            sb.AppendLine("echo \"Bye!\"");
            sb.AppendLine("sleep 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        private static string generateLinuxRestoreScript(string path)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            // setup
            sb.AppendLine("#!/bin/bash");
            sb.AppendLine("set +v");
            sb.AppendLine("clear");

            // title
            sb.Append("title='");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" - Restore of ");
            sb.Append(Application.productName);
            sb.AppendLine(" in progress - DO NOT CLOSE THIS WINDOW!'");
            sb.AppendLine("echo -n -e \"\\033]0;$title\\007\"");

            // header
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.Append("echo \"¦  ");
            sb.Append(Constants.ASSET_NAME);
            sb.Append(" ");
            sb.Append(Constants.ASSET_VERSION);
            sb.AppendLine(" - Linux                                         ¦\"");
            sb.AppendLine("echo \"¦  Copyright 2018-2019 by www.crosstales.com                                 ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  The files will now be restored from the backup destination.               ¦\"");
            sb.AppendLine("echo \"¦  This will take some time, so please be patient and DON'T CLOSE THIS       ¦\"");
            sb.AppendLine("echo \"¦  WINDOW before the process is finished!                                    ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"¦  Unity will restart automatically after the restore.                       ¦\"");
            sb.AppendLine("echo \"¦                                                                            ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"" + Application.productName + "\"");
            sb.AppendLine("echo");
            sb.AppendLine("echo");

            // check if Unity is closed
            sb.Append("while [ -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to close...\"");
            sb.AppendLine("  sleep 3");

            if (Config.DELETE_LOCKFILE)
            {
                sb.Append("  rm \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.Append("Temp/UnityLockfile\"");
                sb.AppendLine();
            }

            sb.AppendLine("done");

            // Restore files
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Restoring files                                                           ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");

            // Assets
            if (Config.COPY_ASSETS)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("Assets");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Assets/\"");
            }

            // Library
            if (Config.COPY_LIBRARY)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("Library");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Library/\"");
            }

            // ProjectSettings
            if (Config.COPY_SETTINGS)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("ProjectSettings");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("ProjectSettings/\"");
            }

#if UNITY_2017_4_OR_NEWER            
            // Packages
            if (Config.COPY_PACKAGES)
            {
                sb.Append("rsync -aq --delete \"");
                sb.Append(path);
                sb.Append("Packages");
                sb.Append("/\" \"");
                sb.Append(Constants.APPLICATION_PATH);
                sb.AppendLine("Packages/\"");
            }
#endif

            // Restart Unity
            sb.AppendLine("echo");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            sb.AppendLine("echo \"¦  Restarting Unity                                                          ¦\"");
            sb.AppendLine("echo \"+----------------------------------------------------------------------------+\"");
            //sb.Append("nohup \"");
            sb.Append('"');
            sb.Append(EditorApplication.applicationPath);
            sb.Append("\" --args -projectPath \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("\"");

            if (Config.BATCHMODE)
            {
                sb.Append(" -batchmode");

                if (Config.QUIT)
                    sb.Append(" -quit");

                if (Config.NO_GRAPHICS)
                    sb.Append(" -nographics");
            }

            if (!string.IsNullOrEmpty(Config.EXECUTE_METHOD_RESTORE))
            {
                sb.Append(" -executeMethod ");
                sb.Append(Config.EXECUTE_METHOD_RESTORE);
            }

            sb.Append(" &");
            sb.AppendLine();

            // check if Unity is started
            sb.AppendLine("echo");
            sb.Append("while [ ! -f \"");
            sb.Append(Constants.APPLICATION_PATH);
            sb.Append("Temp/UnityLockfile\" ]");
            sb.AppendLine();
            sb.AppendLine("do");
            sb.AppendLine("  echo \"Waiting for Unity to start...\"");
            sb.AppendLine("  sleep 3");
            sb.AppendLine("done");
            sb.AppendLine("echo");
            sb.AppendLine("echo \"Bye!\"");
            sb.AppendLine("sleep 1");
            sb.AppendLine("exit");

            return sb.ToString();
        }

        #endregion

        /// <summary>Loads an image as Texture2D from 'Editor Default Resources'.</summary>
        /// <param name="logo">Logo to load.</param>
        /// <param name="fileName">Name of the image.</param>
        /// <returns>Image as Texture2D from 'Editor Default Resources'.</returns>
        private static Texture2D loadImage(ref Texture2D logo, string fileName)
        {
            if (logo == null)
            {
#if CT_DEVELOP
                logo = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets" + Config.ASSET_PATH + "Icons/" + fileName, typeof(Texture2D));
#else
                logo = (Texture2D)EditorGUIUtility.Load("crosstales/TurboBackup/" + fileName);
#endif

                if (logo == null)
                {
                    Debug.LogWarning("Image not found: " + fileName);
                }
            }

            return logo;
        }

        #endregion
    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)