using LunaForge.EditorData.Nodes;
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
    public List<CachedDefinition> Definitions;
    public List<string> AccessibleFrom;
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

    public List<CachedDefinitionFile> Definitions = [];

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

    public void AddToCache(LunaDefinition loadedDefinition)
    {
        CachedDefinitionFile def = new()
        {
            PathToDefinition = Path.GetRelativePath(loadedDefinition.ParentProject.PathToProjectRoot, loadedDefinition.FullFilePath),
            Definitions = loadedDefinition.Definitions ?? [],
            AccessibleFrom = loadedDefinition.AccessibleFrom ?? [],
        };
        Definitions.RemoveAll(x => x.PathToDefinition == def.PathToDefinition);
        Definitions.Add(def);
        Save();
    }

    public void AddToCache(string relativeDefPath, string relativeSrcPath)
    {
        CachedDefinitionFile def = new()
        {
            PathToDefinition = relativeDefPath,
            Definitions = [],
            AccessibleFrom = [relativeSrcPath],
        };
        Definitions.Add(def);
        Save();
    }

    public void RemoveFromCache(LunaDefinition loadedDefinition)
    {
        Definitions.RemoveAll(x => x.PathToDefinition == Path.GetRelativePath(loadedDefinition.ParentProject.PathToProjectRoot, loadedDefinition.FullFilePath));
        Save();
    }

    public void AddAccessibleFrom(string relativeDefPath, string relativeSrcPath)
    {
        foreach (CachedDefinitionFile def in Definitions)
        {
            if (def.PathToDefinition == relativeDefPath)
            {
                def.AccessibleFrom.Add(relativeSrcPath);
                Save();
                break;
            }
        }
    }

    public void RemoveAccessibleFrom(string relativeDefPath, string relativeSrcPath)
    {
        foreach (CachedDefinitionFile def in Definitions)
        {
            if (def.PathToDefinition == relativeDefPath)
            {
                def.AccessibleFrom.Remove(relativeSrcPath);
                Save();
                break;
            }   
        }
    }

    #endregion
}
