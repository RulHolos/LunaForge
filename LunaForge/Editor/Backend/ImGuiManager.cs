using Hexa.NET.ImGui;
using Hexa.NET.ImNodes;
using Hexa.NET.ImPlot;
using Hexa.NET.Raylib;
using LunaForge.Editor.Backend.Utilities;
using LunaForge.Editor.UI.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend;

public class ImGuiManager
{
    private ImGuiContextPtr guiContext;
    private ImNodesContextPtr nodesContext;
    private ImPlotContextPtr plotContext;
    private bool resetLayout = true;

    public unsafe ImGuiManager()
    {
        // Create ImGui context
        guiContext = ImGui.CreateContext(null);

        // Set ImGui context
        ImGui.SetCurrentContext(guiContext);

        // Set the Layout Manager state
        resetLayout = !LayoutManager.Init();

        // Set ImGui context for ImPlot
        ImPlot.SetImGuiContext(guiContext);

        // Set ImGui context for ImNodes
        ImNodes.SetImGuiContext(guiContext);

        // Create and set ImNodes context and set style
        nodesContext = ImNodes.CreateContext();
        ImNodes.SetCurrentContext(nodesContext);
        ImNodes.StyleColorsDark(ImNodes.GetStyle());

        // Create and set ImPlot context and set style
        plotContext = ImPlot.CreateContext();
        ImPlot.SetCurrentContext(plotContext);
        ImPlot.StyleColorsDark(ImPlot.GetStyle());

        // Setup ImGui config.
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;     // Enable Keyboard Controls
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;         // Enable Docking
        //io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;       // Enable Multi-Viewport / Platform Windows
        io.ConfigViewportsNoAutoMerge = false;
        io.ConfigViewportsNoTaskBarIcon = false;

        var fonts = io.Fonts;
        fonts.FontBuilderFlags = (uint)ImFontAtlasFlags.NoPowerOfTwoHeight;
        fonts.TexDesiredWidth = 2048;

        uint* glyphRanges = stackalloc uint[]
        {
                (uint)0xe005, (uint)0xe684,
                (uint)0xF000, (uint)0xF8FF,
                (uint)0 // null terminator
            };

        ImGuiFontBuilder defaultBuilder = new(fonts);
        defaultBuilder.AddFontFromFileTTF("assets/shared/fonts/ARIAL.TTF", 15)
                      .SetOption(conf => conf.GlyphMinAdvanceX = 16)
                      .AddFontFromFileTTF("assets/shared/fonts/fa-solid-900.ttf", 14, glyphRanges)
                      .AddFontFromFileTTF("assets/shared/fonts/fa-brands-400.ttf", 14, glyphRanges);
        aliasToFont.Add("Default", defaultBuilder.Font);
        defaultBuilder.Destroy();

        ImGuiFontBuilder iconsRegularBuilder = new(fonts);
        iconsRegularBuilder.AddFontFromFileTTF("assets/shared/fonts/ARIAL.TTF", 15)
                           .SetOption(conf => conf.GlyphMinAdvanceX = 16)
                           .AddFontFromFileTTF("assets/shared/fonts/fa-regular-400.ttf", 14, glyphRanges);
        aliasToFont.Add("Icons-Regular", iconsRegularBuilder.Font);
        iconsRegularBuilder.Destroy();

        fonts.Build();

        // setup ImGui style
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        colors[(int)ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int)ImGuiCol.WindowBg] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int)ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int)ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.29f);
        colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int)ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.06f, 0.06f, 0.06f, 1.00f);
        colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.CheckMark] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int)ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.00f, 0.00f, 0.00f, 0.36f);
        colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 0.33f);
        colors[(int)ImGuiCol.Separator] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int)ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabHovered] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.TabSelected] = new Vector4(0.20f, 0.20f, 0.20f, 0.36f);
        colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLines] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogram] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.33f, 0.67f, 0.86f, 1.00f);
        colors[(int)ImGuiCol.NavCursor] = new Vector4(1.00f, 0.00f, 0.00f, 1.00f);
        colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(1.00f, 0.00f, 0.00f, 0.70f);
        colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(1.00f, 0.00f, 0.00f, 0.20f);
        colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.10f, 0.10f, 0.10f, 0.00f);

        style.WindowPadding = new Vector2(8.00f, 8.00f);
        style.FramePadding = new Vector2(5.00f, 2.00f);
        style.CellPadding = new Vector2(6.00f, 6.00f);
        style.ItemSpacing = new Vector2(6.00f, 6.00f);
        style.ItemInnerSpacing = new Vector2(6.00f, 6.00f);
        style.TouchExtraPadding = new Vector2(0.00f, 0.00f);
        style.IndentSpacing = 25;
        style.ScrollbarSize = 15;
        style.GrabMinSize = 10;
        style.WindowBorderSize = 1;
        style.ChildBorderSize = 1;
        style.PopupBorderSize = 1;
        style.FrameBorderSize = 1;
        style.TabBorderSize = 1;
        style.WindowRounding = 7;
        style.ChildRounding = 4;
        style.FrameRounding = 3;
        style.PopupRounding = 4;
        style.ScrollbarRounding = 9;
        style.GrabRounding = 3;
        style.LogSliderDeadzone = 4;
        style.TabRounding = 4;

        // When viewports are enabled we tweak WindowRounding/WindowBg so platform windows can look identical to regular ones.
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            style.WindowRounding = 0.0f;
            style.Colors[(int)ImGuiCol.WindowBg].W = 1.0f;
        }

        ImGuiRaylibPlatform.Init();
    }

    public unsafe void NewFrame()
    {
        // Set ImGui context
        ImGui.SetCurrentContext(guiContext);
        // Set ImGui context for ImPlot
        ImPlot.SetImGuiContext(guiContext);
        // Set ImGui context for ImNodes
        ImNodes.SetImGuiContext(guiContext);

        // Set ImNodes context
        ImNodes.SetCurrentContext(nodesContext);
        // Set ImPlot context
        ImPlot.SetCurrentContext(plotContext);

        LayoutManager.NewFrame();

        // Start new frame, call order matters.
        ImGuiRaylibPlatform.NewFrame();
        ImGui.NewFrame();

        // Example for getting the central dockspace id of a window/viewport.
        ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
        DockSpaceId = ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode, null); // passing null as first argument will use the main viewport

        if (resetLayout)
        {
            ResetLayout();
            resetLayout = false;
        }

        ImGui.PopStyleColor(1);
    }

    public static unsafe void ResetLayout()
    {
        ImGuiP.ClearIniSettings();

        uint rootDockspace = DockSpaceId;

        ImGuiP.DockBuilderRemoveNode(rootDockspace);
        ImGuiP.DockBuilderAddNode(rootDockspace, ImGuiDockNodeFlags.PassthruCentralNode);

        uint down;
        uint up = ImGuiP.DockBuilderSplitNode(rootDockspace, ImGuiDir.Up, 0.65f, null, &down);

        uint next;
        uint left = ImGuiP.DockBuilderSplitNode(up, ImGuiDir.Left, 0.15f, null, &next);
        ImGuiP.DockBuilderDockWindow($"{FA.LinesLeaning} File Browser", left);
        uint right = ImGuiP.DockBuilderSplitNode(next, ImGuiDir.Right, 0.25f, null, &next);
        ImGuiP.DockBuilderDockWindow("Project Files", next);

        ImGuiP.DockBuilderFinish(rootDockspace);
    }

    public static uint DockSpaceId { get; private set; }

    public event Action OnRenderDrawData;

    public unsafe void EndFrame()
    {
        // Renders ImGui Data
        var io = ImGui.GetIO();
        ImGui.Render();
        ImGui.EndFrame();

        ImGuiRaylibPlatform.RenderDrawData(ImGui.GetDrawData());

        // Update and Render additional Platform Windows
        if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
        {
            ImGui.UpdatePlatformWindows();
            ImGui.RenderPlatformWindowsDefault();
        }
    }

    public void Dispose()
    {
        ImGuiRaylibPlatform.Shutdown();
    }

    private static readonly Dictionary<string, ImFontPtr> aliasToFont = new();
    private static int fontPushes = 0;

    public static void PushFont(string name)
    {
        if (aliasToFont.TryGetValue(name, out ImFontPtr fontPtr))
        {
            ImGui.PushFont(fontPtr);
            fontPushes++;
        }
    }

    public static void PushFont(string name, bool condition)
    {
        if (condition && aliasToFont.TryGetValue(name, out ImFontPtr fontPtr))
        {
            ImGui.PushFont(fontPtr);
            fontPushes++;
        }
    }

    public static void PopFont()
    {
        if (fontPushes == 0)
        {
            return;
        }

        ImGui.PopFont();
        fontPushes--;
    }
}