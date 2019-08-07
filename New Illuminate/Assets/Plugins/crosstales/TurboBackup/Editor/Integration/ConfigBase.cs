using UnityEngine;
using UnityEditor;
using Crosstales.TB.Util;
using Crosstales.TB.Task;

namespace Crosstales.TB.EditorIntegration
{
    /// <summary>Base class for editor windows.</summary>
    public abstract class ConfigBase : EditorWindow
    {

        #region Variables

        private static string updateText = UpdateCheck.TEXT_NOT_CHECKED;
        private static UpdateStatus updateStatus = UpdateStatus.NOT_CHECKED;

        private System.Threading.Thread worker;

        private Vector2 scrollPosConfig;
        private Vector2 scrollPosHelp;
        private Vector2 scrollPosAboutUpdate;
        private Vector2 scrollPosAboutReadme;
        private Vector2 scrollPosAboutVersions;

        private static string readme;
        private static string versions;

        private int aboutTab = 0;

        private static readonly string[] vcsOptions = { "None", "git", "SVN", "Mercurial", "Collab" };

        #endregion


        #region Protected methods

        protected void showBAR()
        {
            if (Helper.isEditorMode)
            {
                GUILayout.Space(3);
                GUILayout.Label("Backup", EditorStyles.boldLabel);

                if (Helper.isBackupEnabled)
                {
                    GUI.enabled = !Helper.isDeleting;

                    if (GUILayout.Button(new GUIContent(" Backup", Helper.Action_Backup, "Backup the project")))
                    {
                        if (!Config.CONFIRM_BACKUP || EditorUtility.DisplayDialog("Backup the project?",
                            Constants.ASSET_NAME + " will now close Unity and save the following folders: " + System.Environment.NewLine +
                            (Config.COPY_ASSETS ? "• Assets" + System.Environment.NewLine : string.Empty) +
                            (Config.COPY_LIBRARY ? "• Library" + System.Environment.NewLine : string.Empty) +
                            (Config.COPY_SETTINGS ? "• ProjectSettings" + System.Environment.NewLine : string.Empty) +
#if UNITY_2017_4_OR_NEWER
                            (Config.COPY_PACKAGES ? "• Packages" + System.Environment.NewLine : string.Empty) +
#endif
                            System.Environment.NewLine +
                            "Backup directory: " + Config.PATH_BACKUP +
                            System.Environment.NewLine +
                            "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to start the backup?", "Yes", "No"))
                        {
                            Helper.Backup();
                        }
                    }

                    GUILayout.Label("Last Backup:\t" + (Helper.hasBackup ? Config.BACKUP_DATE.ToString() : "never"));
                    //GUILayout.Label("Last Backup:\t" + Config.BACKUP_DATE + " (" + Config.BACKUP_COUNT + ")");

                    Helper.SeparatorUI();

                    GUILayout.Label("Restore", EditorStyles.boldLabel);

                    if (Helper.hasBackup)
                    {
                        if (GUILayout.Button(new GUIContent(" Restore", Helper.Action_Restore, "Restore the project")))
                        {
                            if (!Config.CONFIRM_RESTORE || EditorUtility.DisplayDialog("Restore the project?",
                                Constants.ASSET_NAME + " will now close Unity and restore the following folders: " + System.Environment.NewLine +
                                (Config.COPY_ASSETS ? "• Assets" + System.Environment.NewLine : string.Empty) +
                                (Config.COPY_LIBRARY ? "• Library" + System.Environment.NewLine : string.Empty) +
                                (Config.COPY_SETTINGS ? "• ProjectSettings" + System.Environment.NewLine : string.Empty) +
#if UNITY_2017_4_OR_NEWER
                                (Config.COPY_PACKAGES ? "• Packages" + System.Environment.NewLine : string.Empty) +
#endif
                                System.Environment.NewLine +
                                "Restore directory: " + Constants.APPLICATION_PATH +
                                System.Environment.NewLine +
                                "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to start the restore?", "Yes", "No"))
                            {
                                if (!Config.CONFIRM_RESTORE || !Config.CONFIRM_WARNING || !EditorUtility.DisplayDialog("Overwrite existing project?",
                                "This operation will overwrite ALL files. Any progress since the last backup will BE LOST!" + System.Environment.NewLine + System.Environment.NewLine + "Would you really want to continue?", "NO!", "Yes"))
                                {
                                    Helper.Restore();
                                }
                            }
                        }

                        GUILayout.Label("Last Restore:\t" + (Config.RESTORE_COUNT > 0 ? Config.RESTORE_DATE.ToString() : "never"));
                        //GUILayout.Label("Last Restore:\t" + Config.RESTORE_DATE + " (" + Config.RESTORE_COUNT + ")");
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("No backup found, restore is not possible. Please use 'Backup' first.", MessageType.Info);
                    }

                    GUI.enabled = true;
                }
                else
                {
                    EditorGUILayout.HelpBox("All backup folders are disabled. No actions possible.", MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Disabled in Play-mode!", MessageType.Info);
            }
        }

        protected void showConfiguration()
        {
            scrollPosConfig = EditorGUILayout.BeginScrollView(scrollPosConfig, false, false);
            {
                GUILayout.Label("General Settings", EditorStyles.boldLabel);
                Config.CUSTOM_PATH_BACKUP = EditorGUILayout.BeginToggleGroup(new GUIContent("Custom Backup Path", "Enable or disable a custom backup path (default: " + Constants.DEFAULT_CUSTOM_PATH_BACKUP + ")."), Config.CUSTOM_PATH_BACKUP);
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.SelectableLabel(Config.PATH_BACKUP);

                        if (GUILayout.Button(new GUIContent(" Select", Helper.Icon_Folder, "Select path for the backup")))
                        {
                            string path = EditorUtility.OpenFolderPanel("Select path for the backup", Config.PATH_BACKUP.Substring(0, Config.PATH_BACKUP.Length - (Constants.BACKUP_DIRNAME.Length + 1)), string.Empty);

                            if (!string.IsNullOrEmpty(path))
                            {
                                Config.PATH_BACKUP = path + "/" + Constants.BACKUP_DIRNAME;
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                if (Config.CUSTOM_PATH_BACKUP)
                {
                    GUI.enabled = false;
                }
                Config.VCS = EditorGUILayout.Popup("Version Control", Config.VCS, vcsOptions);
                GUI.enabled = true;

                Config.BATCHMODE = EditorGUILayout.BeginToggleGroup(new GUIContent("Batch mode", "Enable or disable batch mode for CLI operations (default: " + Constants.DEFAULT_BATCHMODE + ")"), Config.BATCHMODE);
                {
                    EditorGUI.indentLevel++;

                    Config.QUIT = EditorGUILayout.Toggle(new GUIContent("Quit", "Enable or disable quit Unity Editor for CLI operations (default: " + Constants.DEFAULT_QUIT + ")."), Config.QUIT);

                    Config.NO_GRAPHICS = EditorGUILayout.Toggle(new GUIContent("No graphics", "Enable or disable graphics device in Unity Editor for CLI operations (default: " + Constants.DEFAULT_NO_GRAPHICS + ")."), Config.NO_GRAPHICS);

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();

                //Config.BATCHMODE = EditorGUILayout.Toggle(new GUIContent("Batch mode", "Enable or disable batch mode for CLI operations (default: " + Constants.DEFAULT_BATCHMODE + ")"), Config.BATCHMODE);

                Config.DELETE_LOCKFILE = EditorGUILayout.Toggle(new GUIContent("Delete UnityLockfile", "Enable or disable deleting the 'UnityLockfile' (default: " + Constants.DEFAULT_DELETE_LOCKFILE + ")."), Config.DELETE_LOCKFILE);

                Config.UPDATE_CHECK = EditorGUILayout.Toggle(new GUIContent("Update Check", "Enable or disable the update-check (default: " + Constants.DEFAULT_UPDATE_CHECK + ")"), Config.UPDATE_CHECK);

                Config.REMINDER_CHECK = EditorGUILayout.Toggle(new GUIContent("Reminder Check", "Enable or disable the reminder-check (default: " + Constants.DEFAULT_REMINDER_CHECK + ")"), Config.REMINDER_CHECK);

                Config.TRACER = EditorGUILayout.Toggle(new GUIContent("Tracer", "Enable or disable anonymous tracing data (default: " + Constants.DEFAULT_TRACER + ")"), Config.TRACER);

                Helper.SeparatorUI();

                GUILayout.Label("Backup & Restore Settings", EditorStyles.boldLabel);
                Config.EXECUTE_METHOD_PRE_BACKUP = EditorGUILayout.TextField(new GUIContent("Method Before Backup", "Execute static method <ClassName.MethodName> in Unity before a backup (default: empty)."), Config.EXECUTE_METHOD_PRE_BACKUP);
                Config.EXECUTE_METHOD_BACKUP = EditorGUILayout.TextField(new GUIContent("Method After Backup", "Execute static method <ClassName.MethodName> in Unity after a backup (default: empty)."), Config.EXECUTE_METHOD_BACKUP);
                Config.EXECUTE_METHOD_PRE_RESTORE = EditorGUILayout.TextField(new GUIContent("Method Before Restore", "Execute static method <ClassName.MethodName> in Unity before a restore (default: empty)."), Config.EXECUTE_METHOD_PRE_RESTORE);
                Config.EXECUTE_METHOD_RESTORE = EditorGUILayout.TextField(new GUIContent("Method After Restore", "Execute static method <ClassName.MethodName> in Unity after a restore (default: empty)."), Config.EXECUTE_METHOD_RESTORE);

                Config.COPY_ASSETS = EditorGUILayout.Toggle(new GUIContent("Copy Assets", "Enable or disable the copying the 'Assets' folder (default: " + Constants.DEFAULT_COPY_ASSETS + ")."), Config.COPY_ASSETS);
                Config.COPY_LIBRARY = EditorGUILayout.Toggle(new GUIContent("Copy Library", "Enable or disable the copying the 'Library' folder (default: " + Constants.DEFAULT_COPY_LIBRARY + ")."), Config.COPY_LIBRARY);
                Config.COPY_SETTINGS = EditorGUILayout.Toggle(new GUIContent("Copy ProjectSettings", "Enable or disable the copying the 'ProjectSettings' folder (default: " + Constants.DEFAULT_COPY_SETTINGS + ")."), Config.COPY_SETTINGS);
#if UNITY_2017_4_OR_NEWER
                Config.COPY_PACKAGES = EditorGUILayout.Toggle(new GUIContent("Copy Packages", "Enable or disable the copying the 'Packages' folder (default: " + Constants.DEFAULT_COPY_PACKAGES + ")."), Config.COPY_PACKAGES);
#endif

                if (!Helper.isBackupEnabled)
                {
                    EditorGUILayout.HelpBox("Please enable at least one folder!", MessageType.Error);
                }
                Helper.SeparatorUI();

                GUILayout.Label("UI Settings", EditorStyles.boldLabel);
                Config.CONFIRM_BACKUP = EditorGUILayout.Toggle(new GUIContent("Confirm Backup", "Enable or disable the backup confirmation dialog (default: " + Constants.DEFAULT_CONFIRM_BACKUP + ")."), Config.CONFIRM_BACKUP);

                Config.CONFIRM_RESTORE = EditorGUILayout.BeginToggleGroup(new GUIContent("Confirm Restore", "Enable or disable the backup confirmation dialog (default: " + Constants.DEFAULT_CONFIRM_RESTORE + ")."), Config.CONFIRM_RESTORE);
                {
                    EditorGUI.indentLevel++;

                    Config.CONFIRM_WARNING = EditorGUILayout.Toggle(new GUIContent("Confirm Warning", "Enable or disable the restore warning confirmation dialog (default: " + Constants.DEFAULT_CONFIRM_WARNING + ")."), Config.CONFIRM_WARNING);

                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.EndScrollView();

            Helper.SeparatorUI();

            GUILayout.Label("Backup Usage", EditorStyles.boldLabel);
            
            GUI.skin.label.wordWrap = true;

            GUILayout.Label(Helper.BackupInfo);

            GUI.skin.label.wordWrap = false;

            GUI.enabled = Helper.hasBackup && !Helper.isDeleting;

            if (GUILayout.Button(new GUIContent(" Delete Backup", Helper.Icon_Delete, "Delete the complete backup")))
            {
                if (EditorUtility.DisplayDialog("Delete the complete backup?", "If you delete the complete backup, " + Constants.ASSET_NAME + " must store all data for the next backup." + System.Environment.NewLine + "This operation could take some time." + System.Environment.NewLine + System.Environment.NewLine + "Would you like to delete the backup?", "Yes", "No"))
                {
                    Helper.DeleteBackup();

                    if (Config.DEBUG)
                        Debug.Log("Complete backup deleted");
                }
            }

            GUI.enabled = true;
        }

        protected void showHelp()
        {
            scrollPosHelp = EditorGUILayout.BeginScrollView(scrollPosHelp, false, false);
            {
                GUILayout.Label("Resources", EditorStyles.boldLabel);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {

                        if (GUILayout.Button(new GUIContent(" Manual", Helper.Icon_Manual, "Show the manual.")))
                        {
                            Application.OpenURL(Constants.ASSET_MANUAL_URL);
                        }

                        GUILayout.Space(6);

                        if (GUILayout.Button(new GUIContent(" Forum", Helper.Icon_Forum, "Visit the forum page.")))
                        {
                            Application.OpenURL(Constants.ASSET_FORUM_URL);
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    {

                        if (GUILayout.Button(new GUIContent(" API", Helper.Icon_API, "Show the API.")))
                        {
                            Application.OpenURL(Constants.ASSET_API_URL);
                        }

                        GUILayout.Space(6);

                        if (GUILayout.Button(new GUIContent(" Product", Helper.Icon_Product, "Visit the product page.")))
                        {
                            Application.OpenURL(Constants.ASSET_WEB_URL);
                        }
                    }
                    GUILayout.EndVertical();

                }
                GUILayout.EndHorizontal();

                Helper.SeparatorUI();

                GUILayout.Label("Videos", EditorStyles.boldLabel);

                if (GUILayout.Button(new GUIContent(" Tutorial", Helper.Video_Tutorial, "View the tutorial video on 'Youtube'.")))
                {
                    Application.OpenURL(Constants.ASSET_VIDEO_TUTORIAL);
                }

                GUILayout.Space(6);

                if (GUILayout.Button(new GUIContent(" All Videos", Helper.Icon_Videos, "Visit our 'Youtube'-channel for more videos.")))
                {
                    Application.OpenURL(Constants.ASSET_SOCIAL_YOUTUBE);
                }
            }
            EditorGUILayout.EndScrollView();

            GUILayout.Space(6);
        }

        protected void showAbout()
        {
            GUILayout.Space(3);
            GUILayout.Label(Constants.ASSET_NAME, EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(60));
                {
                    GUILayout.Label("Version:");

                    GUILayout.Space(12);

                    GUILayout.Label("Web:");

                    GUILayout.Space(2);

                    GUILayout.Label("Email:");

                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Width(170));
                {
                    GUILayout.Space(0);

                    GUILayout.Label(Constants.ASSET_VERSION);

                    GUILayout.Space(12);

                    EditorGUILayout.SelectableLabel(Constants.ASSET_AUTHOR_URL, GUILayout.Height(16), GUILayout.ExpandHeight(false));

                    GUILayout.Space(2);

                    EditorGUILayout.SelectableLabel(Constants.ASSET_CONTACT, GUILayout.Height(16), GUILayout.ExpandHeight(false));
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                {
                    //GUILayout.Space(0);
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical(GUILayout.Width(64));
                {
                    if (GUILayout.Button(new GUIContent(string.Empty, Helper.Logo_Asset, "Visit asset website")))
                    {
                        Application.OpenURL(Constants.ASSET_URL);
                    }
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("© 2018-2019 by " + Constants.ASSET_AUTHOR);

            Helper.SeparatorUI();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent(" AssetStore", Helper.Logo_Unity, "Visit the 'Unity AssetStore' website.")))
                {
                    Application.OpenURL(Constants.ASSET_CT_URL);
                }

                if (GUILayout.Button(new GUIContent(" " + Constants.ASSET_AUTHOR, Helper.Logo_CT, "Visit the '" + Constants.ASSET_AUTHOR + "' website.")))
                {
                    Application.OpenURL(Constants.ASSET_AUTHOR_URL);
                }
            }
            GUILayout.EndHorizontal();

            Helper.SeparatorUI();

            aboutTab = GUILayout.Toolbar(aboutTab, new string[] { "Readme", "Versions", "Update" });

            if (aboutTab == 2)
            {
                scrollPosAboutUpdate = EditorGUILayout.BeginScrollView(scrollPosAboutUpdate, false, false);
                {
                    Color fgColor = GUI.color;

                    GUI.color = Color.yellow;

                    if (updateStatus == UpdateStatus.NO_UPDATE)
                    {
                        GUI.color = Color.green;
                        GUILayout.Label(updateText);
                    }
                    else if (updateStatus == UpdateStatus.UPDATE)
                    {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" Download", "Visit the 'Unity AssetStore' to download the latest version.")))
                        {
                            //Application.OpenURL(Constants.ASSET_URL);
                            UnityEditorInternal.AssetStore.Open("content/" + Constants.ASSET_ID);
                        }
                    }
                    else if (updateStatus == UpdateStatus.UPDATE_PRO)
                    {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" Upgrade", "Upgrade to the PRO-version in the 'Unity AssetStore'.")))
                        {
                            Application.OpenURL(Constants.ASSET_PRO_URL);
                        }
                    }
                    else if (updateStatus == UpdateStatus.UPDATE_VERSION)
                    {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" Upgrade", "Upgrade to the newer version in the 'Unity AssetStore'")))
                        {
                            Application.OpenURL(Constants.ASSET_CT_URL);
                        }
                    }
                    else if (updateStatus == UpdateStatus.DEPRECATED)
                    {
                        GUILayout.Label(updateText);

                        if (GUILayout.Button(new GUIContent(" More Information", "Visit the 'crosstales'-site for more information.")))
                        {
                            Application.OpenURL(Constants.ASSET_AUTHOR_URL);
                        }
                    }
                    else
                    {
                        GUI.color = Color.cyan;
                        GUILayout.Label(updateText);
                    }

                    GUI.color = fgColor;
                }
                EditorGUILayout.EndScrollView();

                if (updateStatus == UpdateStatus.NOT_CHECKED || updateStatus == UpdateStatus.NO_UPDATE)
                {
                    bool isChecking = !(worker == null || (worker != null && !worker.IsAlive));

                    GUI.enabled = Helper.isInternetAvailable && !isChecking;

                    if (GUILayout.Button(new GUIContent(isChecking ? "Checking... Please wait." : " Check For Update", Helper.Icon_Check, "Checks for available updates of " + Constants.ASSET_NAME)))
                    {
                        worker = new System.Threading.Thread(() => UpdateCheck.UpdateCheckForEditor(out updateText, out updateStatus));
                        worker.Start();
                    }

                    GUI.enabled = true;
                }
            }
            else if (aboutTab == 0)
            {
                if (readme == null)
                {
                    string path = Application.dataPath + Config.ASSET_PATH + "README.txt";

                    try
                    {
                        readme = System.IO.File.ReadAllText(path);
                    }
                    catch (System.Exception)
                    {
                        readme = "README not found: " + path;
                    }
                }

                scrollPosAboutReadme = EditorGUILayout.BeginScrollView(scrollPosAboutReadme, false, false);
                {
                    GUILayout.Label(readme);
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                if (versions == null)
                {
                    string path = Application.dataPath + Config.ASSET_PATH + "Documentation/VERSIONS.txt";

                    try
                    {
                        versions = System.IO.File.ReadAllText(path);
                    }
                    catch (System.Exception)
                    {
                        versions = "VERSIONS not found: " + path;
                    }
                }

                scrollPosAboutVersions = EditorGUILayout.BeginScrollView(scrollPosAboutVersions, false, false);
                {
                    GUILayout.Label(versions);
                }

                EditorGUILayout.EndScrollView();
            }

            Helper.SeparatorUI();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Discord, "Communicate with us via 'Discord'.")))
                {
                    Application.OpenURL(Constants.ASSET_SOCIAL_DISCORD);
                }

                if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Facebook, "Follow us on 'Facebook'.")))
                {
                    Application.OpenURL(Constants.ASSET_SOCIAL_FACEBOOK);
                }

                if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Twitter, "Follow us on 'Twitter'.")))
                {
                    Application.OpenURL(Constants.ASSET_SOCIAL_TWITTER);
                }

                if (GUILayout.Button(new GUIContent(string.Empty, Helper.Social_Linkedin, "Follow us on 'LinkedIn'.")))
                {
                    Application.OpenURL(Constants.ASSET_SOCIAL_LINKEDIN);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
        }

        protected static void save()
        {
            Config.Save();

            if (Config.DEBUG)
                Debug.Log("Config data saved");
        }

        #endregion


    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)