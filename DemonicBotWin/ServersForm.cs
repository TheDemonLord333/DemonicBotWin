using DemonicBotWin.WinForms.Models;
using DemonicBotWin.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemonicBotWin.WinForms
{
    public partial class ServersForm : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly IApiService _apiService;

        // Discord-ähnliche Farben
        private readonly Color _discordBackground = Color.FromArgb(54, 57, 63);
        private readonly Color _discordDarker = Color.FromArgb(47, 49, 54);
        private readonly Color _discordLighter = Color.FromArgb(64, 68, 75);
        private readonly Color _discordBlurple = Color.FromArgb(88, 101, 242);
        private readonly Color _discordText = Color.FromArgb(220, 221, 222);
        private readonly Color _discordRed = Color.FromArgb(237, 66, 69);

        private Label lblTitle;
        private ListBox lstServers;
        private Button btnRefresh;
        private Button btnSettings;
        private Label lblError;
        private ProgressBar progressBar;

        private List<DiscordServer> _servers;

        public ServersForm(ISettingsService settingsService, IApiService apiService)
        {
            _settingsService = settingsService;
            _apiService = apiService;
            InitializeComponent();
            _ = LoadServersAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form Settings
            this.Text = "DemonicBot - Server auswählen";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _discordBackground;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "Server auswählen",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(20, 20),
                Size = new Size(300, 40)
            };

            // Settings Button
            btnSettings = new Button
            {
                Text = "Einstellungen",
                Font = new Font("Segoe UI", 10),
                Location = new Point(450, 20),
                Size = new Size(100, 30),
                BackColor = _discordLighter,
                ForeColor = _discordText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.Click += BtnSettings_Click;

            // Server List
            lstServers = new ListBox
            {
                Location = new Point(20, 70),
                Size = new Size(540, 480),
                BackColor = _discordDarker,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 60
            };
            lstServers.DrawItem += LstServers_DrawItem;
            lstServers.DoubleClick += LstServers_DoubleClick;

            // Refresh Button
            btnRefresh = new Button
            {
                Text = "Aktualisieren",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(20, 570),
                Size = new Size(540, 40),
                BackColor = _discordBlurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += async (s, e) => await LoadServersAsync();

            // Error Label
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = _discordRed,
                Location = new Point(20, 620),
                Size = new Size(540, 20),
                TextAlign = ContentAlignment.TopCenter
            };

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, 645),
                Size = new Size(540, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                btnSettings,
                lstServers,
                btnRefresh,
                lblError,
                progressBar
            });

            this.ResumeLayout(false);
        }

        private void LstServers_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var server = _servers[e.Index];
            e.DrawBackground();

            // Draw selection background
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(_discordBlurple), e.Bounds);
            }

            // Draw server icon or initial
            var iconRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 5, 50, 50);
            using (var brush = new SolidBrush(_discordBlurple))
            {
                e.Graphics.FillEllipse(brush, iconRect);
            }

            // Draw initial letter
            using (var font = new Font("Segoe UI", 20, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.White))
            {
                var initial = server.InitialLetter;
                var size = e.Graphics.MeasureString(initial, font);
                var x = iconRect.X + (iconRect.Width - size.Width) / 2;
                var y = iconRect.Y + (iconRect.Height - size.Height) / 2;
                e.Graphics.DrawString(initial, font, brush, x, y);
            }

            // Draw server name
            using (var font = new Font("Segoe UI", 12))
            using (var brush = new SolidBrush(_discordText))
            {
                var textRect = new Rectangle(e.Bounds.X + 75, e.Bounds.Y + 20, e.Bounds.Width - 85, e.Bounds.Height);
                e.Graphics.DrawString(server.DisplayName, font, brush, textRect);
            }

            e.DrawFocusRectangle();
        }

        private async Task LoadServersAsync()
        {
            btnRefresh.Enabled = false;
            progressBar.Visible = true;
            lblError.Text = "";

            try
            {
                var initialized = await _apiService.InitializeAsync();

                if (!initialized)
                {
                    lblError.Text = "API nicht initialisiert. Bitte überprüfe deine Einstellungen.";
                    Application.OpenForms[0].Show();
                    this.Close();
                    return;
                }

                _servers = await _apiService.GetServersAsync();

                lstServers.Items.Clear();
                foreach (var server in _servers)
                {
                    lstServers.Items.Add(server);
                }

                if (_servers.Count == 0)
                {
                    lblError.Text = "Keine Server gefunden. Der Bot muss möglicherweise zu deinen Servern hinzugefügt werden.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Fehler beim Laden der Server: {ex.Message}";
            }
            finally
            {
                btnRefresh.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void LstServers_DoubleClick(object sender, EventArgs e)
        {
            if (lstServers.SelectedIndex >= 0)
            {
                var selectedServer = _servers[lstServers.SelectedIndex];
                var channelsForm = new ChannelsForm(_settingsService, _apiService, selectedServer);
                channelsForm.Show();
                this.Hide();
            }
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm(_settingsService, _apiService);
            settingsForm.ShowDialog(this);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}