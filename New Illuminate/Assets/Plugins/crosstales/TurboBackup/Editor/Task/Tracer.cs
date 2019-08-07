using UnityEditor;
using Crosstales.TB.Util;

namespace Crosstales.TB.Task
{
    /// <summary>Gather some tracing data for the asset.</summary>
    [InitializeOnLoad]
    public static class Tracer
    {
        #region Constructor

        static Tracer()
        {
            string lastDate = EditorPrefs.GetString(Constants.KEY_TRACER_DATE);

            string date = System.DateTime.Now.ToString("yyyyMMdd"); // every day
            //string date = System.DateTime.Now.ToString("yyyyMMddHHmm"); // every minute (for tests)

            if (!date.Equals(lastDate))
            {
                GAApi.Event(typeof(Tracer).Name, "Startup");

                EditorPrefs.SetString(Constants.KEY_TRACER_DATE, date);
            }
        }

        #endregion

    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)