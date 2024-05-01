using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

using SwfLib.Tags.ActionsTags;
using ImGuiNET;
using NativeFileDialogSharp;
using Raylib_cs;
using Rl = Raylib_cs.Raylib;

using BrawlhallaSwz;
using AbcDisassembler;

namespace WallyMapSpinzor2.Raylib;

public class ImportDialog(Editor editor, string brawlPath) : IDialog
{
    private const int MAX_KEY_LENGTH = 9;

    private static string? lastLdPath;
    private static string? lastLtPath;
    private static string? lastLstPath;

    private static string _swzKey = "";
    private string _gamePath = brawlPath;
    private string _bhairPath = Path.Join(brawlPath, "BrawlhallaAir.swf");

    private readonly Dictionary<string, string> levelDescFiles = [];
    private int _pickedFileNum;
    private LevelTypes? _decryptedLt;
    private LevelSetTypes? _decryptedLst;

    private string? _loadingError;

    private bool _open = true;
    public bool Closed => !_open;

    public void Show()
    {
        ImGui.SetNextWindowSizeConstraints(new(500, 410), new(int.MaxValue));
        ImGui.Begin("Import", ref _open, ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse);

        ImGui.BeginTabBar("importTabBar", ImGuiTabBarFlags.None);
        if (ImGui.BeginTabItem("Brawlhalla"))
        {
            ShowGameImportTab();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("xml"))
        {
            ShowXmlImportTab();
            ImGui.EndTabItem();
        }

        if (_loadingError is not null)
        {
            ImGui.PushTextWrapPos();
            ImGui.Text("[Error]: " + _loadingError);
            ImGui.PopTextWrapPos();
        }

        ImGui.EndTabBar();
        ImGui.End();
    }

    private void ShowGameImportTab()
    {
        ImGui.Text("Import from game swz files");
        ImGui.Separator();
        if (ImGui.Button("Select Brawlhalla Path"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FolderPicker(_gamePath);
                if (result.IsOk)
                    _gamePath = result.Path;
            });
        }
        ImGui.Text($"Path: {_gamePath}");

        ImGui.InputText("Decryption key", ref _swzKey, MAX_KEY_LENGTH, ImGuiInputTextFlags.CharsDecimal);
        if (_swzKey.Length > 0 && _decryptedLt is null && ImGui.Button("Decrypt"))
        {
            try
            {
                DecryptSwzFiles(_gamePath);
                _loadingError = null;
            }
            catch (Exception e)
            {
                Rl.TraceLog(TraceLogLevel.Error, e.Message);
                _loadingError = $"Could not decrypt swz files. {e.Message}";
            }
        }

        if (levelDescFiles.Count > 0 && _decryptedLt is not null && _decryptedLst is not null)
        {
            ImGui.ListBox("Pick level file", ref _pickedFileNum, [.. levelDescFiles.Keys], levelDescFiles.Count, 12);
            if (ImGui.Button("Import"))
            {
                string name = levelDescFiles.Keys.ElementAt(_pickedFileNum);
                LevelDesc ld = Utils.DeserializeFromString<LevelDesc>(levelDescFiles[name]);
                editor.LoadMap(new Level(ld, _decryptedLt, _decryptedLst));
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Select BrawlhallaAir.swf"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("swf", _gamePath);
                if (result.IsOk)
                    _bhairPath = result.Path;
            });
        }
        ImGui.Text($"{_bhairPath}");

        if (ImGui.Button("Find decryption key"))
        {
            Task.Run(() =>
            {
                try
                {
                    if (Utils.GetDoABCDefineTag(_bhairPath) is DoABCDefineTag abcTag)
                    {
                        AbcFile abc = AbcFile.Read(abcTag.ABCData);
                        _swzKey = Utils.FindDecryptionKey(abc).ToString() ?? "";
                        _loadingError = null;
                    }
                }
                catch (Exception e)
                {
                    Rl.TraceLog(TraceLogLevel.Error, e.Message);
                    _loadingError = "Could not find decryption key. " + e.Message;
                }
            });
        }
    }

    private void ShowXmlImportTab()
    {
        ImGui.PushTextWrapPos();
        ImGui.Text("Import from LevelDesc xml file, LevelTypes.xml, and LevelSetTypes.xml");
        ImGui.Text("If LevelTypes.xml is not selected or it does not contain the level a default LevelType will be generated");
        ImGui.PopTextWrapPos();
        ImGui.SeparatorText("Select files");

        if (ImGui.Button("LevelDesc"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(lastLdPath));
                if (result.IsOk)
                    lastLdPath = result.Path;
            });
        }
        ImGui.SameLine();
        ImGui.Text(lastLdPath ?? "None");

        if (ImGui.Button("LevelTypes.xml (optional)"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(lastLtPath));
                if (result.IsOk)
                    lastLtPath = result.Path;
            });
        }
        ImGui.SameLine();
        if (lastLtPath is not null && ImGui.Button("x##lt")) lastLtPath = null;
        ImGui.SameLine();
        ImGui.Text(lastLtPath ?? "None");

        if (ImGui.Button("LevelSetTypes.xml (optional)"))
        {
            Task.Run(() =>
            {
                DialogResult result = Dialog.FileOpen("xml", Path.GetDirectoryName(lastLstPath));
                if (result.IsOk)
                    lastLstPath = result.Path;
            });
        }
        ImGui.SameLine();
        if (lastLstPath is not null && ImGui.Button("x##lst")) lastLstPath = null;
        ImGui.SameLine();
        ImGui.Text(lastLstPath ?? "None");

        ImGui.Separator();
        if (lastLdPath is not null && ImGui.Button("Import"))
        {
            try
            {
                editor.LoadMap(lastLdPath, lastLtPath, lastLstPath);
                _open = false;
                _loadingError = null;
            }
            catch (Exception e)
            {
                Rl.TraceLog(TraceLogLevel.Error, e.Message);
                _loadingError = $"Could not load xml file. {e.Message}";
            }
        }
    }

    private void DecryptSwzFiles(string folder)
    {
        string gamePath = Path.Join(folder, "Game.swz");
        string dynamicPath = Path.Join(folder, "Dynamic.swz");
        string initPath = Path.Join(folder, "Init.swz");
        uint key = uint.Parse(_swzKey);

        using (FileStream stream = new(dynamicPath, FileMode.Open, FileAccess.Read))
        {
            using SwzReader reader = new(stream, key);
            while (reader.HasNext())
            {
                string data = reader.ReadFile();
                string name = SwzUtils.GetFileName(data);
                levelDescFiles.Add(name, data);
            }
        }

        _decryptedLt = Utils.DeserializeSwzFromPath<LevelTypes>(initPath, "LevelTypes.xml", key);
        _decryptedLst = Utils.DeserializeSwzFromPath<LevelSetTypes>(gamePath, "LevelSetTypes.xml", key);
    }
}