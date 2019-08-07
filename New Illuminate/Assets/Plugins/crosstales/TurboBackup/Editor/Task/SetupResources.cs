using UnityEditor;
using UnityEngine;

namespace Crosstales.TB.Task
{
    /// <summary>Copies all resources to 'Editor Default Resources'.</summary>
    [InitializeOnLoad]
    public abstract class SetupResources : Common.EditorTask.BaseSetupResources
    {

        #region Constructor

        static SetupResources()
        {

#if !CT_DEVELOP
            string path = Application.dataPath;
            string assetpath = "Assets" + Util.Config.ASSET_PATH;

            string sourceFolder = path + Util.Config.ASSET_PATH + "Icons/";
            string source = assetpath + "Icons/";

            string targetFolder = path + "/Editor Default Resources/crosstales/TurboBackup/";
            string target = "Assets/Editor Default Resources/crosstales/TurboBackup/";
            string metafile = assetpath + "Icons.meta";

            setupResources(source, sourceFolder, target, targetFolder, metafile);
#endif
        }

        #endregion
    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)