using LunaForge.EditorData.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Execution;

public class SubExecution(LunaForgeProject proj) : LSTGExecution
{
    protected override LunaForgeProject ProjectExec => proj;

    public override void BeforeRun()
    {
        Parameters = "\"" + string.Format(ProjectExec.LaunchArguments,
            ProjectExec.Windowed.ToString().ToLower(),
            ProjectExec.DebugRes.X,
            ProjectExec.DebugRes.Y,
            ProjectExec.Cheat.ToString().ToLower(),
            $"\'{ProjectExec.ProjectName}\'") + "\""
            + (ProjectExec.LogWindowSub ? "--log-window" : "");
        UseShellExecute = false;
        CreateNoWindow = true;
        RedirectStandardError = false;
        RedirectStandardOutput = false;
    }

    protected override string LogFileName => "engine.log";
}
