using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaForge.Editor.Backend.Utilities;

public delegate void RefValueChangedHandler<T>(object sender, T? value);

public class Ref<T>
{
    private readonly object _lock = new();
    private T? value;

    public T? Value
    {
        get => value;
        set
        {
            lock (_lock)
            {
                if (this.value.Equals(value))
                    return;

                this.value = value;
                ValueChanged?.Invoke(this, value);
            }
        }
    }

    public bool IsNull
    {
        get
        {
            lock (_lock)
            {
                return value == null;
            }
        }
    }

    public event RefValueChangedHandler<T>? ValueChanged;
}
