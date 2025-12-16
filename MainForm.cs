using Microsoft.Win32;
using System;
using System.Security.Principal;
using System.Windows.Forms;

namespace LatencyTweakTool
{
    public class MainForm : Form
    {
        private Button btnApply;
        private Button btnRestore;

        private const string BackupKeyPath = @"SOFTWARE\LatencyTweakBackup";

        public MainForm()
        {
            Text = "Latency Tweak Tool BY REDSKULL";
            Width = 360;
            Height = 180;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            btnApply = new Button
            {
                Text = "ปรับปรุง",
                Width = 120,
                Height = 40,
                Left = 40,
                Top = 50
            };
            btnApply.Click += BtnApply_Click;

            btnRestore = new Button
            {
                Text = "คืนค่า",
                Width = 120,
                Height = 40,
                Left = 180,
                Top = 50
            };
            btnRestore.Click += BtnRestore_Click;

            Controls.Add(btnApply);
            Controls.Add(btnRestore);

            if (!IsAdministrator())
            {
                MessageBox.Show("กรุณาเปิดโปรแกรมด้วย Run as Administrator",
                    "Permission Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnApply.Enabled = false;
                btnRestore.Enabled = false;
            }
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            BackupOriginalValues();
            ApplyTweaks();
            MessageBox.Show("ปรับปรุงค่าเรียบร้อย กรุณา Restart เครื่อง");
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            RestoreBackup();
            MessageBox.Show("คืนค่ากลับเรียบร้อย กรุณา Restart เครื่อง");
        }

        private void BackupOriginalValues()
        {
            using var backupKey = Registry.LocalMachine.CreateSubKey(BackupKeyPath);

            BackupValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", backupKey);
            BackupValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", backupKey);
            BackupValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", backupKey);
            BackupValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", backupKey);
        }

        private static void BackupValue(string keyPath, string valueName, RegistryKey backupKey)
        {
            using var key = Registry.LocalMachine.OpenSubKey(keyPath);
            var value = key?.GetValue(valueName);
            if (value != null)
                backupKey.SetValue(valueName, value);
        }

        private static void ApplyTweaks()
        {
            using (var tcpKey = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters"))
            {
                tcpKey.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                tcpKey.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
            }

            using (var mmKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
            {
                mmKey.SetValue("NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), RegistryValueKind.DWord);
                mmKey.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
            }
        }

        private static void RestoreBackup()
        {
            using var backupKey = Registry.LocalMachine.OpenSubKey(BackupKeyPath);
            if (backupKey == null) return;

            RestoreValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", backupKey);
            RestoreValue(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TCPNoDelay", backupKey);
            RestoreValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", backupKey);
            RestoreValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", backupKey);
        }

        private static void RestoreValue(string keyPath, string valueName, RegistryKey backupKey)
        {
            var value = backupKey.GetValue(valueName);
            using var key = Registry.LocalMachine.CreateSubKey(keyPath);

            if (value == null)
                key.DeleteValue(valueName, false);
            else
                key.SetValue(valueName, value);
        }
    }
}