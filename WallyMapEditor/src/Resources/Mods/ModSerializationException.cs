using System;

namespace WallyMapEditor.Mod;

[Serializable]
public sealed class ModSerializationException : Exception
{
    public ModSerializationException() { }
    public ModSerializationException(string message) : base(message) { }
    public ModSerializationException(string message, Exception inner) : base(message, inner) { }
}