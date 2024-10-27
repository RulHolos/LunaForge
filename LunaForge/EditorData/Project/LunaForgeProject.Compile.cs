using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.EditorData.Project;

public enum TargetVersion
{
    Plus,
    Sub,
    Evo,
    x
}

public partial class LunaForgeProject
{
    public void SetTargetVersion(string? tempPath = null)
    {
        FileVersionInfo LuaSTGExecutableInfos = FileVersionInfo.GetVersionInfo(tempPath ?? PathToLuaSTGExecutable);
        if (LuaSTGExecutableInfos.ProductName.Contains("Plus"))
            TargetLuaSTG = TargetVersion.Plus;
        else if (LuaSTGExecutableInfos.ProductName.Contains("Sub"))
            TargetLuaSTG = TargetVersion.Sub;
        else if (LuaSTGExecutableInfos.ProductName.Contains("-x"))
            TargetLuaSTG = TargetVersion.x;
        else if (LuaSTGExecutableInfos.ProductName.Contains("Evo"))
            TargetLuaSTG = TargetVersion.Evo;
    }

    public void GatherCompileInfo()
    {
        CompileProcess c = new();
        string tempPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "LunaForge Editor"));
        if (!Directory.Exists(tempPath))
            Directory.CreateDirectory(tempPath);

        c.CurrentTempPath = tempPath;
        c.Source = this;
        // TODO: Be able to set a root code in the settings.
        c.RootCode = $"require(\'THlib.lua\')\nrequire(\'{Path.ChangeExtension(EntryPointRelative, ".lua")}\')";

        CompileProcess = c;
    }

    public async Task SaveCode()
    {
        string[] listOfDefinitions = Directory.GetFiles(CompileProcess.CurrentTempPath, "*.lfd", SearchOption.AllDirectories);
        foreach (string definitionPath in listOfDefinitions)
        {
            try
            {
                LunaDefinition def = await LunaDefinition.CreateFromFile(this, definitionPath);
                if (def.TreeNodes[0] != null)
                {
                    string pathToTemp = Path.GetRelativePath(PathToProjectRoot, definitionPath);
                    using FileStream fs = new(Path.Combine(CompileProcess.CurrentTempPath, Path.ChangeExtension(pathToTemp, ".lua")), FileMode.Create, FileAccess.Write);
                    using (StreamWriter sw = new(fs))
                    {
                        foreach (string code in def.TreeNodes[0].TryToLua(0))
                            sw.Write(code);
                    }
                }
                File.Delete(definitionPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public async Task SaveShaders()
    {
        string[] listOfShaders = Directory.GetFiles(CompileProcess.CurrentTempPath, "*.lfs", SearchOption.AllDirectories);
        foreach (string shaderPath in listOfShaders)
        {
            try
            {
                LunaShader shader = await LunaShader.CreateFromFile(this, shaderPath);
                string pathToTemp = Path.GetRelativePath(PathToProjectRoot, shaderPath);
                using FileStream fs = new(Path.Combine(CompileProcess.CurrentTempPath, Path.ChangeExtension(pathToTemp, shader.FileFormat)), FileMode.Create, FileAccess.Write);
                using (StreamWriter sw = new(fs))
                {
                    sw.Write("Yellow.");
                }
                File.Delete(shaderPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public async Task SaveSCDebugCode()
    {
        string[] listOfDefinitions = Directory.GetFiles(CompileProcess.CurrentTempPath, "*.lfd", SearchOption.AllDirectories);
        foreach (string definitionPath in listOfDefinitions)
        {
            try
            {
                LunaDefinition def = await LunaDefinition.CreateFromFile(this, definitionPath);
                if (def.TreeNodes[0] != null)
                {
                    string pathToTemp = Path.GetRelativePath(PathToProjectRoot, definitionPath);
                    using FileStream fs = new(Path.Combine(CompileProcess.CurrentTempPath, Path.ChangeExtension(pathToTemp, ".lua")), FileMode.Create, FileAccess.Write);
                    using (StreamWriter sw = new(fs))
                    {
                        foreach (string code in def.TreeNodes[0].TryToLua(0))
                            sw.Write(code);
                    }
                }
                File.Delete(definitionPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
