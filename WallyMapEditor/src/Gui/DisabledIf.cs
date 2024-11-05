using System;
using ImGuiNET;

namespace WallyMapEditor;

public readonly struct DisabledIf : IDisposable
{
    private readonly bool _disabled;

    public static DisabledIf _(bool disabled) => new(disabled);

    private DisabledIf(bool disabled)
    {
        _disabled = disabled;
        if (_disabled) ImGui.BeginDisabled();
    }

    public void Dispose()
    {
        if (_disabled) ImGui.EndDisabled();
    }
}