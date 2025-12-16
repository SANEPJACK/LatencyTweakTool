using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LatencyTweakTool
{
    public sealed class AccessDeniedForm : Form
    {
        private readonly string _uuid;
        private readonly System.Windows.Forms.Timer _timer;
        private int _remainingSeconds = 15;
        private readonly Label _messageLabel;
        private readonly Label _statusLabel;

        public AccessDeniedForm(string uuid)
        {
            _uuid = uuid;

            Text = "Access denied";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Width = 520;
            Height = 380;

            ApplyBackgroundImage();

            _messageLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(400, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(30, 30, 30),
                Margin = new Padding(4, 4, 4, 12)
            };

            _statusLabel = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(400, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkGreen,
                Margin = new Padding(4, 0, 4, 12)
            };

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false,
                Padding = new Padding(10, 10, 10, 10),
                Margin = new Padding(0)
            };

            var btnDiscord = new Button { Text = "Discord", Width = 100, Height = 34 };
            var btnCopy = new Button { Text = "คัดลอก", Width = 100, Height = 34 };
            var btnOk = new Button { Text = "OK", Width = 100, Height = 34, DialogResult = DialogResult.OK };

            btnDiscord.Click += (s, e) => Program.OpenDiscordInvite();
            btnCopy.Click += (s, e) => CopyUuid();
            btnOk.Click += (s, e) => Close();

            buttons.Controls.Add(btnDiscord);
            buttons.Controls.Add(btnCopy);
            buttons.Controls.Add(btnOk);

            var centerButtons = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            centerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            centerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            centerButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            centerButtons.Controls.Add(buttons, 1, 0);

            var content = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(16, 16, 16, 16),
                BackColor = Color.FromArgb(235, Color.White),
                Anchor = AnchorStyles.None,
                MinimumSize = new Size(360, 180),
                MaximumSize = new Size(440, 0)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            content.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            content.Controls.Add(_messageLabel, 0, 0);
            content.Controls.Add(_statusLabel, 0, 1);
            content.Controls.Add(centerButtons, 0, 2);

            _messageLabel.Anchor = AnchorStyles.None;
            _statusLabel.Anchor = AnchorStyles.None;

            var outer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                BackColor = Color.Transparent
            };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            outer.Controls.Add(content, 1, 1);

            Controls.Add(outer);

            AcceptButton = btnOk;
            CancelButton = btnOk;

            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateMessage();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remainingSeconds--;
            if (_remainingSeconds <= 0)
            {
                Close();
                return;
            }

            UpdateMessage();
        }

        private void CopyUuid()
        {
            try
            {
                Clipboard.SetText(_uuid);
                _statusLabel.ForeColor = Color.DarkGreen;
                _statusLabel.Text = "คัดลอกแล้ว";
            }
            catch (Exception ex)
            {
                _statusLabel.ForeColor = Color.Maroon;
                _statusLabel.Text = "คัดลอกไม่สำเร็จ: " + ex.Message;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            base.OnFormClosed(e);
        }

        private void ApplyBackgroundImage()
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(MainForm.BackgroundBase64Large);
                using MemoryStream ms = new MemoryStream(bytes);
                BackgroundImage = Image.FromStream(ms);
                BackgroundImageLayout = ImageLayout.Stretch;
                BackColor = Color.Black;
            }
            catch
            {
                BackgroundImage = null;
                BackColor = Color.Black;
            }
        }

        private void UpdateMessage()
        {
            _messageLabel.Text = $"รหัสเครื่องของคุณ:\n{_uuid}\nยังไม่ได้รับอนุญาตให้ใช้งาน\n(จะปิดอัตโนมัติใน {_remainingSeconds} วินาที)";
        }
    }
}
