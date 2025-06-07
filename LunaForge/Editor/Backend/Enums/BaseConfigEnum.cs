using LunaForge.Editor.Backend.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Enums;

public enum BaseConfigEnum
{
    // ## GENERAL ## //
    [BaseConfig(ConfigSystemCategory.General, false)] SetupDone,
    [BaseConfig(ConfigSystemCategory.General, "")] ProjectsFolder,
    [BaseConfig(ConfigSystemCategory.General, "")] SelectedLayout,
    [BaseConfig(ConfigSystemCategory.General, false)] BypassLauncher,

    // ## SERVICES ## //
    [BaseConfig(ConfigSystemCategory.Services, false)] UseDiscordRPC,

    // ## DEFAULT PROJECT ## //
    [BaseConfig(ConfigSystemCategory.DefaultProject, "John Dough")] ProjectAuthor,
}