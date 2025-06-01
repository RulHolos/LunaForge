using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Debugging;

public struct TerminalMessage : IEquatable<TerminalMessage>
{
    public string Message;

    public TerminalColor Color;

    public long Timestamp;

    public TerminalMessage(string message, TerminalColor color)
    {
        Message = message;
        Color = color;
        Timestamp = Stopwatch.GetTimestamp();
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is TerminalMessage message && Equals(message);
    }

    public readonly bool Equals(TerminalMessage other)
    {
        return Message == other.Message &&
            Color == other.Color &&
            Timestamp == other.Timestamp;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Message, Color, Timestamp);
    }

    public static bool operator ==(TerminalMessage left, TerminalMessage right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(TerminalMessage left, TerminalMessage right)
    {
        return !(left == right);
    }
}
