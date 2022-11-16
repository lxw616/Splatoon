﻿using Dalamud.Interface.Colors;
using ECommons;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Splatoon.Gui.Scripting
{
    internal static class TabScripting
    {
        internal static void Draw()
        {
            if(ImGui.Button("Rescan directory and reload all scripts"))
            {
                ScriptingProcessor.ReloadAll();
            }
            ImGui.SameLine();
            if(ImGui.Button("Install from clipboard"))
            {
                var text = ImGui.GetClipboardText();
                if (text.StartsWithAny(ScriptingProcessor.TrustedURLs, StringComparison.OrdinalIgnoreCase))
                {
                    Task.Run(delegate
                    {
                        try
                        {
                            var result = P.HttpClient.GetStringAsync(text).Result;
                            ScriptingProcessor.CompileAndLoad(result, null);
                        }
                        catch(Exception e)
                        {
                            e.Log();
                        }
                    });
                    
                    Notify.Info("Downloading script from trusted URL...");
                }
                else 
                {
                    ScriptingProcessor.CompileAndLoad(text, null);
                }
            }
            var del = -1;
            for(var i = 0;i<ScriptingProcessor.Scripts.Count;i++)
            {
                var x = ScriptingProcessor.Scripts[i];
                ImGui.PushID(x.InternalData.GUID);
                var e = !P.Config.DisabledScripts.Contains(x.InternalData.FullName);
                if(ImGui.Checkbox($"##enable", ref e))
                {
                    if (!e)
                    {
                        P.Config.DisabledScripts.Add(x.InternalData.FullName);
                    }
                    else
                    {
                        P.Config.DisabledScripts.Remove(x.InternalData.FullName);
                    }
                    ScriptingProcessor.Scripts.ForEach(x => x.UpdateState());
                }
                ImGui.SameLine();
                ImGuiEx.Text($"{x.InternalData.Namespace}/{x.InternalData.Name}");
                ImGuiEx.Tooltip($"{x.InternalData.GUID}");
                ImGui.SameLine();
                ImGuiEx.Text(x.IsEnabled?ImGuiColors.ParsedGreen:ImGuiColors.DalamudRed, x.IsEnabled ? "Enabled" : "Disabled");
                ImGui.SameLine();
                if (ImGui.Button("Unload"))
                {
                    del = i;
                }
                ImGui.SameLine();
                if (ImGui.Button("Delete"))
                {
                    if (!x.InternalData.Path.IsNullOrEmpty() && x.InternalData.Path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        del = i;
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(x.InternalData.Path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        Notify.Error("Error deleting");
                    }
                }
                ImGui.PopID();
            }
            if(del != -1)
            {
                ScriptingProcessor.Scripts[del].Disable();
                ScriptingProcessor.Scripts.RemoveAt(del);
            }
        }
    }
}
