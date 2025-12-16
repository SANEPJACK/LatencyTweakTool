using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace LatencyTweakTool
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            AuthResult authResult;
            try
            {
                authResult = AuthService.VerifyOrCreateAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "ไม่สามารถเชื่อมต่อเพื่อยืนยันสิทธิ์ได้\n" + ex.Message,
                    "Connection error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!authResult.IsAuthorized)
            {
                using var deniedForm = new AccessDeniedForm(authResult.Uuid);
                deniedForm.ShowDialog();
                return;
            }

            Application.Run(new MainForm(authResult));
        }

        public static void OpenDiscordInvite()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://discord.gg/msHbnzpzTZ",
                    UseShellExecute = true
                });
            }
            catch
            {
                // ignored
            }
        }
    }
}
