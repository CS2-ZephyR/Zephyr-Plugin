using System.Runtime.CompilerServices;
using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;

namespace ZephyrPlugin.Module.Whitelist.Util;

public class PlayerName<T> : NativeObject where T : NativeObject
{
    internal PlayerName(T instance) : base(Schema.GetSchemaValue<nint>(instance.Handle, typeof(T).Name!, "m_iszPlayerName"))
    { }

    internal unsafe void Set(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);

        for (var i = 0; i < bytes.Length; i++)
        {
            Unsafe.Write((void*)(Handle.ToInt64() + i), bytes[i]);
        }

        Unsafe.Write((void*)(Handle.ToInt64() + bytes.Length), 0);
    }
}
