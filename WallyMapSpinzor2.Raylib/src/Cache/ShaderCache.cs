using System.Collections.Generic;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

using ShaderId = System.ValueTuple<string?, string?>;

namespace WallyMapSpinzor2.Raylib;

public class ShaderCache
{
    public Dictionary<ShaderId, Shader> Cache { get; set; } = [];

    public Shader Load(string? vs, string? fs)
    {
        if (Cache.TryGetValue((vs, fs), out Shader shader))
            return shader;
        else
            return Cache[(vs, fs)] = Rl.LoadShaderFromMemory(vs, fs);
    }

    public void Clear()
    {
        foreach ((_, Shader shader) in Cache)
        {
            Rl.UnloadShader(shader);
        }
    }
}