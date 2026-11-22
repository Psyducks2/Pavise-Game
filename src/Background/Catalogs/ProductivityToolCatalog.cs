using System;
using System.Collections.Generic;
using System.IO;

namespace PaviseApp
{
    // 生产力与交互式辅助工具（如开发环境、AI 听写与语音转文字工具、终端和笔记应用）
    // 在对局或多任务环境中保持正常响应，避免因后台激进治理陷入卡顿
    internal static class ProductivityToolCatalog
    {
        private static readonly HashSet<string> ProcessNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Code & Development Editors
            "Code", "devenv", "idea64", "pycharm64", "webstorm64", "rider64",
            "sublime_text", "notepad++", "Antigravity",

            // Terminal & Shell Environments
            "WindowsTerminal", "wt", "alacritty", "powershell", "pwsh", "cmd",

            // AI Dictation & Speech-to-Text Tools
            "Typeless", "Kudu", "whisper", "superwhisper",

            // Modern Productivity & Notes
            "Obsidian", "Notion", "Logseq"
        };

        private static string[] Products()
        {
            return new string[]
            {
                Lang.Text("VS Code 与 Antigravity", "VS Code & Antigravity"),
                Lang.Text("Visual Studio 与 JetBrains IDE", "Visual Studio & JetBrains IDEs"),
                Lang.Text("Windows Terminal 与控制台终端", "Windows Terminal & Shells"),
                Lang.Text("AI 听写与语音转文字工具 (Typeless / Whisper)", "AI Speech-to-Text & Dictation (Typeless / Whisper)"),
                Lang.Text("Obsidian 与 Notion 笔记工具", "Obsidian & Notion Notes")
            };
        }

        internal static bool IsProductivityProcess(string name, string imagePath)
        {
            string normalizedName = WhitelistRule.NormalizeName(name);
            if (!ProcessNames.Contains(normalizedName)) return false;
            string path = WhitelistRule.NormalizeImagePath(imagePath);
            if (path.Length < 4 || !char.IsLetter(path[0]) || path[1] != ':' || path[2] != '\\') return false;
            try
            {
                return string.Equals(Path.GetExtension(path), ".exe", StringComparison.OrdinalIgnoreCase)
                    && string.Equals(normalizedName, Path.GetFileNameWithoutExtension(path), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        internal static string[] ProductsForDisplay()
        {
            return Products();
        }

        internal static string[] ProcessNamesForDisplay()
        {
            var names = new List<string>(ProcessNames);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names.ToArray();
        }
    }
}
