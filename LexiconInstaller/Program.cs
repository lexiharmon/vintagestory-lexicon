using System.Diagnostics;
using System.Reflection;

namespace LexiconInstaller;

/// <summary>
/// Installs, updates or removes the Lexicon mod for Vintage Story by copying the embedded mod zip into the Mods folder.
///
/// Switches (all optional):
///   --silent             no dialogs; installs, or uninstalls when combined with --uninstall
///   --uninstall          remove Lexicon
///   --mods-dir "path"    use a different Mods folder (default: %APPDATA%\VintagestoryData\Mods)
/// </summary>
static class Program
{
    const string Title = "Lexicon Setup";
    static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

    [STAThread]
    static int Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        bool silent = args.Contains("--silent");
        bool uninstall = args.Contains("--uninstall");
        int dirArg = Array.IndexOf(args, "--mods-dir");
        string modsDir = dirArg >= 0 && dirArg + 1 < args.Length
            ? args[dirArg + 1]
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VintagestoryData", "Mods");

        try
        {
            return Run(modsDir, silent, uninstall);
        }
        catch (Exception e)
        {
            Show(silent, $"Something went wrong:\n\n{e.Message}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }

    static int Run(string modsDir, bool silent, bool uninstall)
    {
        string[] installed = Directory.Exists(modsDir) ? Directory.GetFiles(modsDir, "Lexicon_*.zip") : Array.Empty<string>();

        if (!silent && GameIsRunning())
        {
            Show(false, "Vintage Story is running. Please close the game and run this setup again.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 2;
        }

        if (!silent)
        {
            if (installed.Length == 0)
            {
                var answer = Show(false, $"Install Lexicon {Version} (exact-match handbook search)?\n\nIt will be copied to:\n{modsDir}", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (answer != DialogResult.OK) return 0;
            }
            else
            {
                var answer = Show(false,
                    $"Lexicon is already installed ({Path.GetFileName(installed[0])}).\n\nYes: update to {Version}\nNo: uninstall\nCancel: do nothing",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (answer == DialogResult.Cancel) return 0;
                uninstall = answer == DialogResult.No;
            }
        }

        foreach (string file in installed) File.Delete(file);

        if (uninstall)
        {
            Show(silent, installed.Length == 0 ? "Lexicon was not installed." : "Lexicon has been uninstalled.\n\nYour settings file (lexicon.json in ModConfig) was left in place.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return 0;
        }

        Directory.CreateDirectory(modsDir);
        string target = Path.Combine(modsDir, $"Lexicon_{Version}.zip");
        using (Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Lexicon.zip"))
        using (FileStream file = File.Create(target))
        {
            resource.CopyTo(file);
        }

        Show(silent, $"Lexicon {Version} is installed.\n\nStart Vintage Story and open the Survival Handbook to try the new search.", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return 0;
    }

    static bool GameIsRunning() => Process.GetProcessesByName("Vintagestory").Length > 0;

    static DialogResult Show(bool silent, string message, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        return silent ? DialogResult.OK : MessageBox.Show(message, Title, buttons, icon);
    }
}
