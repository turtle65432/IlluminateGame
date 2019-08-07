using UnityEditor;
using UnityEngine;

namespace Crosstales.TB
{
    /// <summary>Backup and restore methods.</summary>
    public static class BAR
    {
        /// <summary>Backup the current project via CLI.</summary>
        public static void BackupCLI()
        {
            //TODO add path
            Backup(Util.Helper.getCLIArgument("-tbExecuteMethod"), ("true".CTEquals(Util.Helper.getCLIArgument("-tbBatchmode")) ? true : false), ("false".CTEquals(Util.Helper.getCLIArgument("-tbQuit")) ? false : true), ("true".CTEquals(Util.Helper.getCLIArgument("-tbNoGraphics")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopyAssets")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopyLibrary")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopySettings")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopyPackages")) ? true : false));
        }

        /// <summary>Restore the current project via CLI.</summary>
        public static void RestoreCLI()
        {
            //TODO add path
            Restore(Util.Helper.getCLIArgument("-tbExecuteMethod"), ("true".CTEquals(Util.Helper.getCLIArgument("-tbBatchmode")) ? true : false), ("false".CTEquals(Util.Helper.getCLIArgument("-tbQuit")) ? false : true), ("true".CTEquals(Util.Helper.getCLIArgument("-tbNoGraphics")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopyAssets")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopyLibrary")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopySettings")) ? true : false), ("true".CTEquals(Util.Helper.getCLIArgument("-tbCopyPackages")) ? true : false));
        }

        /// <summary>Backup the current project.</summary>
        /// <param name="executeMethod">Execute method after backup (optional)</param>
        /// <param name="batchmode">Start Unity in batch-mode (default: false, optional)</param>
        /// <param name="quit">Quit Unity in batch-mode (default: true, optional)</param>
        /// <param name="noGraphics">Disable graphic devices in batch-mode (default: false, optional)</param>
        /// <param name="copyAssets">Copy the 'Assets'-folder (default: true, optional)</param>
        /// <param name="copyLibrary">Copy the 'Library'-folder (default: false, optional)</param>
        /// <param name="copySettings">Copy the 'ProjectSettings"-folder (default: true, optional)</param>
        /// <param name="copyPackages">Copy the 'Packages"-folder (default: true, optional)</param>
        public static void Backup(string executeMethod = "", bool batchmode = false, bool quit = true, bool noGraphics = false, bool copyAssets = true, bool copyLibrary = false, bool copySettings = true, bool copyPackages = true)
        {
            if (copyAssets || copyLibrary || copySettings || copyPackages)
            {
                Util.Config.EXECUTE_METHOD_BACKUP = executeMethod;
                Util.Config.BATCHMODE = batchmode;
                Util.Config.QUIT = quit;
                Util.Config.NO_GRAPHICS = noGraphics;
                Util.Config.COPY_ASSETS = copyAssets;
                Util.Config.COPY_LIBRARY = copyLibrary;
                Util.Config.COPY_SETTINGS = copySettings;
                Util.Config.COPY_PACKAGES = copyPackages;

                Util.Helper.Backup();
            }
            else
            {
                Debug.LogError("No folders selected - backup not possible!");
#if UNITY_2018_2_OR_NEWER
                if (Application.isBatchMode)
#else
                if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
#endif
                throw new System.Exception("No folders selected - backup not possible!");
                //EditorApplication.Exit(0);

            }
        }

        /// <summary>Restore the current project.</summary>
        /// <param name="executeMethod">Execute method after restore (optional)</param>
        /// <param name="batchmode">Start Unity in batch-mode (default: false, optional)</param>
        /// <param name="quit">Quit Unity in batch-mode (default: true, optional)</param>
        /// <param name="noGraphics">Disable graphic devices in batch-mode (default: false, optional)</param>
        /// <param name="restoreAssets">Restore the 'Assets'-folder (default: true, optional)</param>
        /// <param name="restoreLibrary">Restore the 'Library'-folder (default: false, optional)</param>
        /// <param name="restoreSettings">Restore the 'ProjectSettings"-folder (default: true, optional)</param>
        /// <param name="restorePackages">Restore the 'Packages"-folder (default: true, optional)</param>
        public static void Restore(string executeMethod = "", bool batchmode = false, bool quit = true, bool noGraphics = false, bool restoreAssets = true, bool restoreLibrary = false, bool restoreSettings = true, bool restorePackages = true)
        {
            if (restoreAssets || restoreLibrary || restoreSettings || restorePackages)
            {
                Util.Config.EXECUTE_METHOD_RESTORE = executeMethod;
                Util.Config.BATCHMODE = batchmode;
                Util.Config.QUIT = quit;
                Util.Config.NO_GRAPHICS = noGraphics;
                Util.Config.COPY_ASSETS = restoreAssets;
                Util.Config.COPY_LIBRARY = restoreLibrary;
                Util.Config.COPY_SETTINGS = restoreSettings;
                Util.Config.COPY_PACKAGES = restorePackages;

                Util.Helper.Restore();
            }
            else
            {
                Debug.LogError("No folders selected - restore not possible!");
#if UNITY_2018_2_OR_NEWER
                if (Application.isBatchMode)
#else
                if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
#endif
                throw new System.Exception("No folders selected - restore not possible!");
                //EditorApplication.Exit(0);
            }
        }

        /// <summary>Test the backup/restore with an execute method.</summary>
        public static void SayHello()
        {
            Debug.LogError("Hello everybody, I was called by " + Util.Constants.ASSET_NAME);
        }
    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)
