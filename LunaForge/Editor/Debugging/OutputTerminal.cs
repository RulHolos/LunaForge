using System;
using System.Diagnostics;
using System.Text;

namespace LunaForge.Editor.Debugging;

public class OutputTerminal : TerminalBase
{
    private readonly TerminalTraceListener traceListener;
    private readonly TerminalConsoleRedirect consoleRedirect;

    public OutputTerminal()
    {
        traceListener = new(this);
        Trace.Listeners.Add(traceListener);
        consoleRedirect = new(this);
        Console.SetOut(consoleRedirect);
    }

    private class TerminalTraceListener : TraceListener
    {
        private readonly OutputTerminal terminal;

        public TerminalTraceListener(OutputTerminal terminal)
        {
            this.terminal = terminal;
        }

        public override void Write(string? message)
        {
            if (message == null)
            {
                return;
            }

            string[] lines = message.Split('\n');
            if (terminal.Messages.Count != 0 && !terminal.Messages[^1].Message.EndsWith('\n'))
            {
                var msg = terminal.Messages[^1];
                msg.Message += lines[0];
                terminal.SetMessage(terminal.Messages.Count - 1, msg);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "\f")
                        terminal.Clear();
                    else if (i > 0)
                        terminal.AddMessage(lines[i]);
                }
            }
            else
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] == "\f")
                        terminal.Clear();
                    else
                        terminal.AddMessage(lines[i]);
                }
            }
        }

        public override void WriteLine(string? message)
        {
            if (message == null)
                return;

            terminal.AddMessage(message);
        }
    }

    private class TerminalConsoleRedirect : TextWriter
    {
        private readonly OutputTerminal terminal;

        public TerminalConsoleRedirect(OutputTerminal terminal)
        {
            this.terminal = terminal;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(string? value)
        {
            if (value == null)
            {
                base.Write(value);
                return;
            }
            terminal.Write(value);
            base.Write(value);
        }
    }

    public void Write(string text)
    {
        AddMessage(text);
    }
    
    public void WriteLine(string text)
    {
        Write(text + '\n');
    }
}
