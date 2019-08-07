using UnityEditor;
using Crosstales.TB.Util;

namespace Crosstales.TB.Task
{
    /// <summary>Show the configuration window on the first launch.</summary>
    [InitializeOnLoad]
    public static class Launch
    {

        #region Constructor

        static Launch()
        {
            bool launched = EditorPrefs.GetBool(Constants.KEY_LAUNCH);

            if (!launched)
            {
                EditorIntegration.ConfigWindow.ShowWindow(3);
                EditorPrefs.SetBool(Constants.KEY_LAUNCH, true);
            }
        }

        #endregion
    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)