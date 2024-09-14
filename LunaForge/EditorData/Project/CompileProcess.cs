﻿using LunaForge.EditorData.Nodes.NodeData.General;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;
using LunaForge.Zip;

namespace LunaForge.EditorData.Project;

/// <summary>
/// Represents the Compile to LuaSTG readable mod data and process.<br/>
/// A lot of this code and compile process was taken and modified from LuaSTG Editor Sharp X.
/// </summary>
public class CompileProcess
{
    /// <summary>
    /// Path to the OS temp directory. aka: where the packed .zip mod is prepared.
    /// </summary>
    public string CurrentTempPath { get; set; } = string.Empty;

    /// <summary>
    /// The current compiled process for quick reference.
    /// </summary>
    public LunaForgeProject Source { get; set; }

    /// <summary>
    /// Gets the meta.dat file to use for the MD5 check for file packing.
    /// </summary>
    public string PathToMD5Meta => Path.Combine(Source.PathToProjectRoot, "meta.dat");

    /// <summary>
    /// The path the LuaSTG install executable. Supports all main branches.
    /// </summary>
    public string LuaSTGExePath => Source.PathToLuaSTGExecutable;

    /// <summary>
    /// Path to the LuaSTG install folder where the exectuable is located.<br/>
    /// Set Automatically.
    /// </summary>
    public string LuaSTGFolderPath => Path.GetDirectoryName(LuaSTGExePath);

    /// <summary>
    /// Path to the LuaSTG "mod" folder. aka: where to pack the .zip file.
    /// </summary>
    public string ModFolderPath => Path.Combine(LuaSTGFolderPath, "mod");

    /// <summary>
    /// Path to the final mod zip in <see cref="ModFolderPath"/>.
    /// </summary>
    public string FinalZipPath => Path.Combine(ModFolderPath, $"{Source.ProjectName}.zip");

    /// <summary>
    /// The code to put in the root.lua file (entrypoint of the mod).
    /// </summary>
    public string RootCode { get; set; } = string.Empty;

    /// <summary>
    /// Where to insert <see cref="RootCode"/>.
    /// </summary>
    public string RootLuaPath => Path.Combine(CurrentTempPath, "root.lua");

    public async Task ExecuteProcess(bool SCDebug, bool StageDebug)
    {
        /*
         * Processus de compilation:
         * -> Si le zip target n'existe pas, repack l'intégralité du projet (ignorer la partie juste après et prendre tous les fichiers du dossier projet)
         * -> Check le hash de tous les fichiers récursivement et copier les fichiers qui ne correspondent pas au hash déjà enregistré (ou si y'en a aucun d'enregistré)
         * dans le dossier temp avec le chemin d'accès correspondant dans le dir tree.
         * (ignore les fichiers .lfp)
         * -> A partir des fichier copiés dans temp: Générer le code avec SCDebug et StageDebug et supprimer les fichiers lfd après chaque génération.
         * -> Générer le code root.
         * -> Créer ou update le target zip à partir des fichiers copiés dans le dossier temp.
         * -> Supprimer le dossier temp.
         */

        // TODO: Force repack on option (or not use md5) or just button.
        List<string> filesToPack = await CheckMetaParity(!File.Exists(FinalZipPath));
        foreach (string file in filesToPack)
        {
            string relativePath = Path.GetRelativePath(Source.PathToProjectRoot, file);
            string dir = Path.GetDirectoryName(Path.Combine(CurrentTempPath, relativePath));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.Copy(file, Path.Combine(CurrentTempPath, relativePath), true);
        }

        WriteRootCode();
        await GenerateCode(SCDebug, StageDebug); // Wait for all the code to be generated.

        PackFiles();

        if (Directory.Exists(CurrentTempPath)) // Delete the temp directory with all currently existing files.
            Directory.Delete(CurrentTempPath, true);
    }

    public async Task GenerateCode(bool SCDebug, bool StageDebug)
    {
        if (SCDebug)
        {
            //Source.SaveSCDebugCode();
        }
        else if (StageDebug)
        {
            //Source.SaveStageDebugCode();
        }
        else
        {
            await Source.SaveCode();
        }
    }

    /// <summary>
    /// This method gets the MD5 Hash for a given file.
    /// </summary>
    /// <param name="filePath">The target file path.</param>
    /// <returns>A MD5 Hash in a string representation.</returns>
    public static string GetMD5HashFromFile(string filePath)
    {
        using (FileStream fs = new(filePath, FileMode.Open, FileAccess.Read))
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(fs);
            return Convert.ToHexString(bytes);
        }
    }

    /// <summary>
    /// Writes <see cref="RootCode"/> to the corresponding .lua file.
    /// </summary>
    public void WriteRootCode()
    {
        using FileStream fs = new(RootLuaPath, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new(fs);
        try
        {
            sw.Write(RootCode);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Checks the entire project tree for files registered in the project .meta file.<br/>
    /// If a file isn't registered of the hash doesn't correspond, it registers it and add it to the returned array.
    /// </summary>
    /// <param name="forceRepack">Ignore MD5 checks and returns all file paths.</param>
    /// <returns>An array of mismatched files' path (files to re-pack.)</returns>
    public async Task<List<string>> CheckMetaParity(bool forceRepack)
    {
        string[] allProjectFiles = Directory.GetFiles(Source.PathToProjectRoot, "*.*", SearchOption.AllDirectories);
        List<string> filesToPack = [];
        if (!File.Exists(PathToMD5Meta))
            forceRepack = true;

        if (!forceRepack)
        {
            Dictionary<string, string> hashToPath = [];
            using (StreamReader sr = new(PathToMD5Meta))
            {
                while (sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] parts = line.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    hashToPath.Add(parts[0], parts[1]);
                }
            }

            Parallel.ForEach(allProjectFiles, file =>
            {
                if (file.EndsWith(".dat"))
                    return;
                if (!hashToPath.ContainsKey(GetMD5HashFromFile(file)))
                {
                    lock (filesToPack)
                    {
                        filesToPack.Add(file);
                    }
                }
            });
        }

        using FileStream fs = new(PathToMD5Meta, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        using (StreamWriter sr = new(fs))
        {
            foreach (string file in allProjectFiles)
            {
                if (file.EndsWith(".dat"))
                    continue;
                string hash = await Task.Run(() => GetMD5HashFromFile(file));
                await sr.WriteLineAsync($"{hash}|{file}");
            }
        }

        List<string> returnList = forceRepack ? [.. allProjectFiles] : filesToPack;
        returnList.RemoveAll(x => x.EndsWith(".lfp") || x.EndsWith(".dat"));
        return returnList;
    }

    public void PackFiles()
    {
        Dictionary<string, string> archivefiles = [];
        foreach (string file in Directory.GetFiles(CurrentTempPath, "*.*", SearchOption.AllDirectories))
            archivefiles.Add(Path.GetRelativePath(CurrentTempPath, file), file);
        ZipCompressor compressor;
        if (Source.UseFolderPacking)
        {
            compressor = new PlainCopy(FinalZipPath);
        }
        /*else if (currentApp.BatchPacking)
        {
            compressor = new ZipCompressorBatch(targetZipPath, zipExePath, rootZipPackPath);
        }*/
        else
        {
            compressor = new ZipCompressorInternal(FinalZipPath);
        }
        foreach (string e in compressor.PackByDictReporting(archivefiles, false))
        { } // Actually needed, don't remove the foreach.
    }
}