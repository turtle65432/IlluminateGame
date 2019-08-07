using UnityEditor;

namespace Crosstales.TB.EditorTask
{
    /// <summary>Adds the given define symbols to PlayerSettings define symbols.</summary>
    [InitializeOnLoad]
    public class CompileDefines : Common.EditorTask.BaseCompileDefines
    {

        private const string symbol = "CT_TB";

        static CompileDefines()
        {
            addSymbolsToAllTargets(symbol);
        }
    }
}
// © 2018-2019 crosstales LLC (https://www.crosstales.com)