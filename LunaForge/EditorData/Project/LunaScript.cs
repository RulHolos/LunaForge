using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using LunaForge.EditorData.Traces;
using LunaForge.GUI;
using YamlDotNet.Core.Tokens;

namespace LunaForge.EditorData.Project;

public class LunaScript : LunaProjectFile
{
    public string SavedFileContent { get; private set; }

    public string FileContent;
    private const int MaxSize = 10_000_000;

    public override bool IsUnsaved
    {
        get => SavedFileContent != GenerateChecksum(FileContent);
    }

    public LunaScript(LunaForgeProject parentProj, string path)
        : base(parentProj, path)
    {

    }

    #region Rendering

    public override void Render()
    {
        ImGuiInputTextFlags flags = ImGuiInputTextFlags.AllowTabInput;
        string availableText = $"{string.Format(CultureInfo.InvariantCulture,
            "{0:0,0}",FileContent.Length)}/{string.Format(CultureInfo.InvariantCulture,
            "{0:0,0}", MaxSize)}";

        Vector2 availableSize = ImGui.GetContentRegionAvail();
        Vector2 textSize = ImGui.CalcTextSize(availableText);
        Vector2 inputSize = new(availableSize.X, availableSize.Y - textSize.Y - ImGui.GetStyle().ItemSpacing.Y);

        ImGui.InputTextMultiline($"##{FileName}_editor", ref FileContent, MaxSize, inputSize, flags);
        ImGui.Text(availableText);
    }

    #endregion
    #region IO

    public static string GenerateChecksum(string content)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            StringBuilder sb = new();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public override async Task Save(bool saveAs = false)
    {
        async Task SelectPathAsync(bool success, string path)
        {
            if (success)
            {
                FullFilePath = path;
                FileName = Path.GetFileName(path);
                PushSavedCommand();
                try
                {
                    using StreamWriter sw = new(path);
                    await SerializeToFile(sw);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

        void SelectPath(bool success, string path)
        {
            SelectPathAsync(success, path);
        }

        if (string.IsNullOrEmpty(FullFilePath) || saveAs)
        {
            string lastUsedPath = Configuration.Default.LastUsedPath;
            MainWindow.FileDialogManager.SaveFileDialog("Save Script", "Lua Script{.lua}",
                saveAs ? string.Empty : FileName, "Lua Script{.lua}", SelectPath, string.IsNullOrEmpty(lastUsedPath)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : lastUsedPath, true);
        }
        else
        {
            await SelectPathAsync(true, FullFilePath);
        }
    }

    public async Task SerializeToFile(StreamWriter sw)
    {
        await sw.WriteAsync(FileContent);
        SavedFileContent = GenerateChecksum(FileContent);
    }

    public static async Task<LunaScript> CreateFromFile(LunaForgeProject parentProject, string filePath)
    {
        LunaScript script = new(parentProject, filePath);
        try
        {
            using (StreamReader sr = new(filePath))
            {
                script.FileContent = await sr.ReadToEndAsync();
                script.FileContent = script.FileContent.Replace("\r\n", "\n"); // Avoid Windows new-line. Normalize to unix.
            }
            script.SavedFileContent = GenerateChecksum(script.FileContent);
            return script;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return default;
        }
    }

    public override void Close()
    {
        EditorTraceContainer.RemoveChecksFromSource(this);
        ParentProject.ProjectFiles.Remove(this);
        ParentProject.CurrentProjectFile = null;
    }

    #endregion
    #region Script

    public override void Delete()
    {
        throw new NotImplementedException();
    }
    public override bool Delete_CanExecute()
    {
        throw new NotImplementedException();
    }

    #endregion
}
