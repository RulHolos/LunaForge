using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.GUI.Helpers;

internal static class ProjectFileSystem
{
    public static void CreateHiddenFolder(string path)
    {
        try
        {
            if (Directory.Exists(path))
                return;

            DirectoryInfo di = new(path);
            di.Create();
            di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public static void CreateNodeArchitecture(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    public static void CreateLunaForgeData(string pathToData)
    {
        CreateHiddenFolder(pathToData);
        CreateNodeArchitecture(Path.Combine(pathToData, "nodes"));
        // TODO: Other files maybe?
    }

    public static string[] GetPackableFiles(string path)
    {
        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            ReturnSpecialDirectories = false,
            AttributesToSkip = FileAttributes.Hidden
        };
        List<string> files = [.. Directory.GetFiles(path, "*.*", options)];
        files.RemoveAll(x => x.EndsWith(".lfp"));
        return [.. files];
    }
}
