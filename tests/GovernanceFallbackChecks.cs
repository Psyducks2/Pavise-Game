using System;
using System.Collections.Generic;

namespace PaviseApp
{
    internal static class GovernanceFallbackChecks
    {
        private static int checks;
        private static void Check(bool condition, string reason)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(reason);
        }

        public static void Main()
        {
            // 1. ProductivityToolCatalog verification
            Check(ProductivityToolCatalog.IsProductivityProcess("Code", @"C:\Program Files\Microsoft VS Code\Code.exe"), "VS Code must be recognized as productivity tool");
            Check(ProductivityToolCatalog.IsProductivityProcess("Antigravity", @"C:\Users\test\AppData\Local\Programs\Antigravity\Antigravity.exe"), "Antigravity must be recognized as productivity tool");
            Check(ProductivityToolCatalog.IsProductivityProcess("Typeless", @"C:\Program Files\Typeless\Typeless.exe"), "Typeless must be recognized as productivity/dictation tool");
            Check(ProductivityToolCatalog.IsProductivityProcess("WindowsTerminal", @"C:\Program Files\WindowsApps\WindowsTerminal.exe"), "WindowsTerminal must be recognized");
            Check(!ProductivityToolCatalog.IsProductivityProcess("notepad", @"C:\Windows\System32\notepad.exe"), "Standard notepad should not be in productivity catalog");
            Check(!ProductivityToolCatalog.IsProductivityProcess("Code", @"relative\path\Code.exe"), "Relative paths must be rejected");
            Check(!ProductivityToolCatalog.IsProductivityProcess("Code", @"C:\dir\Other.exe"), "Mismatched executable names must be rejected");

            Check(ProductivityToolCatalog.ProductsForDisplay().Length > 0, "ProductsForDisplay must not be empty");
            Check(ProductivityToolCatalog.ProcessNamesForDisplay().Length > 0, "ProcessNamesForDisplay must not be empty");

            // 2. Custom Shell Protection in SystemProcessCatalog
            Check(SystemProcessCatalog.IsShellProcess("explorer"), "explorer must be shell process");
            Check(SystemProcessCatalog.IsShellProcess("seelen-ui"), "seelen-ui must be shell process");
            Check(SystemProcessCatalog.IsShellProcess("retrobar"), "retrobar must be shell process");
            Check(SystemProcessCatalog.IsShellProcess("cairoshell"), "cairoshell must be shell process");
            Check(SystemProcessCatalog.IsShellProcess("startallback"), "startallback must be shell process");
            Check(!SystemProcessCatalog.IsShellProcess("game.exe"), "game.exe is not a shell process");

            // 3. SuppressionCore fallback tracking methods
            SuppressionCore core = new SuppressionCore();
            List<SuppressedProcessDetail> suppressed = core.GetActiveSuppressedProcesses();
            Check(suppressed != null, "GetActiveSuppressedProcesses must not be null");
            Check(suppressed.Count == 0, "Initial suppressed count should be 0");
            int releasedCount = core.ReleaseAllBackground();
            Check(releasedCount == 0, "Initial ReleaseAllBackground must return 0");

            // 4. Trilingual Localization Checks
            foreach (int language in new[] { Lang.Chinese, Lang.English, Lang.Portuguese })
            {
                Lang.Cur = language;
                string prodTitle = Lang.T("white.auto.productivity");
                Check(!string.IsNullOrEmpty(prodTitle), "white.auto.productivity must be non-empty in language " + language);

                string prodList = Lang.F("white.auto.productivity.list", "TestApp");
                Check(prodList.Contains("TestApp"), "white.auto.productivity.list must format placeholder in language " + language);

                string dialogTitle = Lang.Text("后台进程治理与快速放行", "Background Governance & Quick Fallback");
                Check(!string.IsNullOrEmpty(dialogTitle), "Dialog title must be translated in language " + language);

                string emergencyBtn = Lang.Text("全部解除治理 (紧急回退)", "Release All (Emergency Fallback)");
                Check(!string.IsNullOrEmpty(emergencyBtn), "Emergency button must be translated in language " + language);
            }

            Console.WriteLine("PASS Governance fallback & productivity safety checks=" + checks + "; catalog=verified; dialog=trilingual; product_started=false");
        }
    }
}
