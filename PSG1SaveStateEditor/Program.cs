namespace PSG1SaveStateEditor;

internal static class Program
{
    /// <summary>The main entry point for the application.</summary>
    [STAThread]
    static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        // Log any unhandled exception so failures are diagnosable instead of a silent crash.
        string log = Path.Combine(AppContext.BaseDirectory, "crash.log");
        Application.ThreadException += (_, e) => File.WriteAllText(log, e.Exception.ToString());
        AppDomain.CurrentDomain.UnhandledException += (_, e) => File.WriteAllText(log, e.ExceptionObject?.ToString() ?? "unknown");

        // Allow opening a .p2s passed on the command line (drag-drop / "Open with").
        string? path = args.Length > 0 && File.Exists(args[0]) ? args[0] : null;
        Application.Run(new MainForm(path));
    }
}
