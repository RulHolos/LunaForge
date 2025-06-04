using LunaForge.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Tests;

public class ConfigTests
{
    [Fact]
    public void EditorConfig_CreateConfig()
    {
        var config = new ConfigSystem();
        Assert.NotNull(config);

        config.SetOrCreate("testkey", 0, ConfigSystemCategory.General);
        Assert.Equal(0, config.Get<int>("testkey").TempValue);

        config.Set("testkey", 1);
        Assert.NotEqual(1, config.Get<int>("testkey").Value);

        config.CommitAll();
        Assert.Equal(1, config.Get<int>("testkey").Value);
    }
}
