using System;
using System.Numerics;
using System.Threading.Tasks;
using System.IO;
using ImGuiNET;
using NativeFileDialogSharp;

namespace WallyMapSpinzor2.Raylib;

public class ExportDialog(IDrawable? mapData, PathPreferences prefs) : IDialog
{
    public bool _open = true;
    public bool Closed { get => !_open; }
    private readonly IDrawable? _mapData = mapData;

    private string? _descPreview;
    private string? _typePreview;
    private bool _addToLt = false;

    private string? _exportError;

    private const int PREVIEW_SIZE = 25;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(425, 425), new(int.MaxValue));
        ImGui.Begin("Export", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);
        if (_mapData is null)
        {
            ImGui.Text("No map data open");
            return;
        }

        ImGui.BeginTabBar("exportTabBar", ImGuiTabBarFlags.None);

        if (ImGui.BeginTabItem("LevelDesc")) 
        {
            ShowLevelDescExportTab();
            ImGui.EndTabItem();
        }

        if (_mapData is Level l)
        {
            if (ImGui.BeginTabItem("LevelType"))
            {
                ShowLevelTypeExportTab(l);
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Playlists"))
            {
                ShowPlaylistsExportTab(l);
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        if (_exportError is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text($"[Error]: {_exportError}");
            ImGui.PopTextWrapPos();
        } 

        ImGui.End();
    }

    public void ShowLevelDescExportTab()
    {
        LevelDesc? ld = _mapData switch
        {
            Level level => level.Desc,
            LevelDesc desc => desc,
            _ => null
        };

        if (ld is null) return;

        ImGui.Text("preview");

        if (_descPreview is not null)
            ImGui.InputTextMultiline("leveldesc##preview", ref _descPreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE));
        else if (ImGui.Button("Generate preview"))
            _descPreview = Utils.SerializeToString(ld);

        if (ImGui.Button("Export"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelDescPath));
                if (result.IsOk)
                {
                    Utils.SerializeToPath(ld, result.Path);
                    prefs.LevelDescPath = result.Path;
                    _exportError = null;
                }
            });
        }
    }

    public void ShowLevelTypeExportTab(Level l)
    {
        if (l.Type is null) return;

        ImGui.Text("preview");
        if (_typePreview is not null)
            ImGui.InputTextMultiline("leveltype##preview", ref _typePreview, uint.MaxValue, new Vector2(-1, ImGui.GetTextLineHeight() * PREVIEW_SIZE));
        else if (ImGui.Button("Generate preview"))
            _typePreview = Utils.SerializeToString(l.Type);

        ImGui.Checkbox("Add to LevelTypes.xml", ref _addToLt);
        if (_addToLt)
        {
            if (ImGui.Button("LevelTypes.xml"))
            {
                Task.Run(() =>
                {
                    DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(prefs.LevelTypesPath));
                    if (result.IsOk)
                    {
                        prefs.LevelTypesPath = result.Path;
                        _exportError = null;
                    }
                });
            }
            ImGui.SameLine();
            ImGui.Text(prefs.LevelTypesPath ?? "None");

            if (ImGuiExt.WithDisabledButton(prefs.LevelTypesPath is null, "Export"))
            {
                Task.Run(() =>
                {
                    try
                    {
                        LevelTypes lts = Utils.DeserializeFromPath<LevelTypes>(prefs.LevelTypesPath!);
                        if (lts.Levels.Length == 0) throw new Exception($"Could not read LevelTypes.xml from given path {prefs.LevelTypesPath}");
                        lts.AddOrUpdateLevelType(l.Type);
                        DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelTypesPath));
                        if (result.IsOk)
                        {
                            Utils.SerializeToPath(lts, result.Path);
                            _exportError = null;
                        }
                    }
                    catch (Exception e)
                    {
                        _exportError = e.Message;
                    }
                });
            }
        }
        else if (ImGui.Button("Export"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileSave("xml", Path.GetDirectoryName(prefs.LevelTypePath));
                if (result.IsOk)
                {
                    Utils.SerializeToPath(l.Type, result.Path);
                    prefs.LevelTypePath = result.Path;
                    _exportError = null;
                }
            });
        }
    }

    public void ShowPlaylistsExportTab(Level l)
    {
        ImGui.Text($"{l.Desc.LevelName} is in playlists:");
        foreach (string playlist in l.Playlists)
            ImGui.BulletText(playlist);
    }
}