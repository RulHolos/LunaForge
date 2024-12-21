using LunaForge.EditorData.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Execution;

public class XExecution(LunaForgeProject proj) : LSTGExecution
{
    protected override LunaForgeProject ProjectExec => proj;

    public override void BeforeRun()
    {
        Parameters = "\"" + string.Format(ProjectExec.LaunchArguments,
            ProjectExec.Windowed.ToString().ToLower(),
            ProjectExec.DebugRes.X,
            ProjectExec.DebugRes.Y,
            ProjectExec.Cheat.ToString().ToLower(),
            $"\'{ProjectExec.ProjectName}\'") + "\"";
        UseShellExecute = false;
        CreateNoWindow = true;
        RedirectStandardError = true;
        RedirectStandardOutput = true;
    }

    protected override string LogFileName => "lstg_log.txt";
}
