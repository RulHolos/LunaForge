using LunaForge.EditorData.Nodes;
using LunaForge.EditorData.Nodes.NodeData.Project;
using LunaForge.GUI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LunaForge.EditorData.Project;

public enum DefinitionMetaType
{
    Boss, BossBG, Bullet, Laser, BentLaser,
    Object, Background, Enemy, Task, Item, Player, PlayerBullet,
    BGM, Image, ImageGroup, SE, Animation, Particle, Texture, FX, Font, TTF
}

public struct CachedDefinitionFile
{
    public string PathToDefinition;
    public HashSet<CachedDefinition> Definitions;
}

public struct CachedDefinition
{
    public string ClassName;
    public string[] Parameters;
    public string MetaModelName; // Object, Item, ...
}

[Serializable]
public class DefinitionsCache
{
    #region Serialized Properties

    public HashSet<CachedDefinitionFile> Definitions = [];

    #endregion

    [YamlIgnore]
    public LunaForgeProject ParentProj { get; set; }

    [YamlIgnore]
    public string PathToCache { get; set; }

    public DefinitionsCache() { }

    #region IO

    public bool Save()
    {
        try
        {
            ISerializer serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(this);
            using StreamWriter sw = new(PathToCache);
            sw.Write(yaml);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public static DefinitionsCache LoadFromProject(LunaForgeProject parentProj)
    {
        try
        {
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            string pathToCache = Path.Combine(parentProj.PathToData, "defcache.yaml");
            using FileStream fs = new(pathToCache, FileMode.OpenOrCreate, FileAccess.Read);
            using StreamReader sr = new(fs);
            DefinitionsCache cache = deserializer.Deserialize<DefinitionsCache>(sr) ?? new();
            cache.ParentProj = parentProj;
            cache.PathToCache = pathToCache;
            return cache;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return null;
        }
    }

    #endregion
    #region Caching

    public bool DefinitionExistsInCache(string relativeDefPath) => Definitions.Any(x => x.PathToDefinition == relativeDefPath);

    private static string GetRelativePath(LunaDefinition def) => Path.GetRelativePath(def.ParentProject.PathToProjectRoot, def.FullFilePath);

    public void AddToCache(LunaDefinition loadedDefinition)
    {
        CachedDefinitionFile def = new()
        {
            PathToDefinition = GetRelativePath(loadedDefinition),
            Definitions = loadedDefinition.Definitions ?? [],
        };
        Definitions.RemoveWhere(x => x.PathToDefinition == def.PathToDefinition);
        Definitions.Add(def);
        Save();
    }

    public void RemoveFromCache(LunaDefinition loadedDefinition)
    {
        Definitions.RemoveWhere(x => x.PathToDefinition == GetRelativePath(loadedDefinition));
        Save();
    }

    public CachedDefinition[] GetDefinitionsWithType(string filter)
    {
        List<CachedDefinition> defs = [];
        foreach (CachedDefinitionFile defFile in Definitions)
            foreach (CachedDefinition def in defFile.Definitions)
                if (def.MetaModelName == filter)
                    defs.Add(def);
        defs.Sort((x, y) => x.ClassName.CompareTo(y.ClassName));
        return [.. defs];
    }

    #endregion
}
