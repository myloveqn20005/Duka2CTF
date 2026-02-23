namespace Duka2CTF;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Khởi tạo các cấu hình giao diện cho Windows Forms
        ApplicationConfiguration.Initialize();

        // Chạy màn hình chính MainForm
        Application.Run(new MainForm());
    }
}