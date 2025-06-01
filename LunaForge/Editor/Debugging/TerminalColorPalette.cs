using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Debugging;

public class TerminalColorPalette
{
    private readonly Vector4[] values;

    public TerminalColorPalette()
    {
        values = new Vector4[Enum.GetValues<TerminalColor>().Length];
        this[TerminalColor.Black] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        this[TerminalColor.DarkBlue] = new Vector4(0.0f, 0.0f, 0.5f, 1.0f);
        this[TerminalColor.DarkGreen] = new Vector4(0.0f, 0.5f, 0.0f, 1.0f);
        this[TerminalColor.DarkCyan] = new Vector4(0.0f, 0.5f, 0.5f, 1.0f);
        this[TerminalColor.DarkRed] = new Vector4(0.5f, 0.0f, 0.0f, 1.0f);
        this[TerminalColor.DarkMagenta] = new Vector4(0.5f, 0.0f, 0.5f, 1.0f);
        this[TerminalColor.DarkYellow] = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
        this[TerminalColor.Gray] = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        this[TerminalColor.DarkGray] = new Vector4(0.25f, 0.25f, 0.25f, 1.0f);
        this[TerminalColor.Blue] = new Vector4(0.0f, 0.0f, 1.0f, 1.0f);
        this[TerminalColor.Green] = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        this[TerminalColor.Cyan] = new Vector4(0.0f, 1.0f, 1.0f, 1.0f);
        this[TerminalColor.Red] = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        this[TerminalColor.Magenta] = new Vector4(1.0f, 0.0f, 1.0f, 1.0f);
        this[TerminalColor.Yellow] = new Vector4(1.0f, 1.0f, 0.0f, 1.0f);
        this[TerminalColor.White] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    }

    public ref Vector4 this[TerminalColor index]
    {
        get { return ref values[(int)index]; }
    }
}

public enum TerminalColor
{
    Black = 0,
    DarkBlue = 1,
    DarkGreen = 2,
    DarkCyan = 3,
    DarkRed = 4,
    DarkMagenta = 5,
    DarkYellow = 6,
    Gray = 7,
    DarkGray = 8,
    Blue = 9,
    Green = 10,
    Cyan = 11,
    Red = 12,
    Magenta = 13,
    Yellow = 14,
    White = 15
}
