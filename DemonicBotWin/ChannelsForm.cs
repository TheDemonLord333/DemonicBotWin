using DemonicBotWin.WinForms.Models;
using DemonicBotWin.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemonicBotWin.WinForms
{
    public partial class ChannelsForm : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly IApiService _apiService;
        private readonly DiscordServer _server;

        // Discord-ähnliche Farben
        private readonly Color _discordBackground = Color.FromArgb(54, 57, 63);
        private readonly Color _discordDarker = Color.FromArgb(47, 49, 54);
        private readonly Color _discordLighter = Color.FromArgb(64, 68, 75);
        private readonly Color _discordBlurple = Color.FromArgb(88, 101, 242);
        private readonly Color _discordText = Color.FromArgb(220, 221, 222);
        private readonly Color _discordRed = Color.FromArgb(237, 66, 69);

        private Label lblTitle;
        private ListBox lstChannels;
        private Button btnBack;
        private Button btnRefresh;
        private Label lblError;
        private ProgressBar progressBar;

        private List<DiscordChannel> _channels;

        public ChannelsForm(ISettingsService settingsService, IApiService apiService, DiscordServer server)
        {
            _settingsService = settingsService;
            _apiService = apiService;
            _server = server;
            InitializeComponent();
            _ = LoadChannelsAsync();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form Settings
            this.Text = $"DemonicBot - Kanäle in {_server.Name}";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _discordBackground;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = $"Kanäle in {_server.Name}",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(20, 20),
                Size = new Size(400, 40)
            };

            // Back Button
            btnBack = new Button
            {
                Text = "Zurück",
                Font = new Font("Segoe UI", 10),
                Location = new Point(480, 20),
                Size = new Size(80, 30),
                BackColor = _discordLighter,
                ForeColor = _discordText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;

            // Channel List
            lstChannels = new ListBox
            {
                Location = new Point(20, 70),
                Size = new Size(540, 480),
                BackColor = _discordDarker,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 12),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 50
            };
            lstChannels.DrawItem += LstChannels_DrawItem;
            lstChannels.DoubleClick += LstChannels_DoubleClick;

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
            btnRefresh.Click += async (s, e) => await LoadChannelsAsync();

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
                btnBack,
                lstChannels,
                btnRefresh,
                lblError,
                progressBar
            });

            this.ResumeLayout(false);
        }

        private void LstChannels_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var channel = _channels[e.Index];
            e.DrawBackground();

            // Draw selection background
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(_discordBlurple), e.Bounds);
            }

            // Draw hash icon
            using (var font = new Font("Segoe UI", 16, FontStyle.Bold))
            using (var brush = new SolidBrush(_discordText))
            {
                e.Graphics.DrawString("#", font, brush, e.Bounds.X + 10, e.Bounds.Y + 10);
            }

            // Draw channel name
            using (var font = new Font("Segoe UI", 12))
            using (var brush = new SolidBrush(_discordText))
            {
                var textRect = new Rectangle(e.Bounds.X + 40, e.Bounds.Y + 15, e.Bounds.Width - 50, e.Bounds.Height);
                e.Graphics.DrawString(channel.Name, font, brush, textRect);
            }

            e.DrawFocusRectangle();
        }

        private async Task LoadChannelsAsync()
        {
            btnRefresh.Enabled = false;
            progressBar.Visible = true;
            lblError.Text = "";

            try
            {
                _channels = await _apiService.GetChannelsAsync(_server.Id);

                lstChannels.Items.Clear();
                foreach (var channel in _channels)
                {
                    lstChannels.Items.Add(channel);
                }

                if (_channels.Count == 0)
                {
                    lblError.Text = "Keine Textkanäle in diesem Server gefunden.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Fehler beim Laden der Kanäle: {ex.Message}";
            }
            finally
            {
                btnRefresh.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private void LstChannels_DoubleClick(object sender, EventArgs e)
        {
            if (lstChannels.SelectedIndex >= 0)
            {
                var selectedChannel = _channels[lstChannels.SelectedIndex];
                var embedForm = new EmbedCreatorForm(_settingsService, _apiService, _server, selectedChannel);
                embedForm.Show();
                this.Hide();
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            var serversForm = Application.OpenForms["ServersForm"] as ServersForm;
            if (serversForm != null)
            {
                serversForm.Show();
            }
            else
            {
                serversForm = new ServersForm(_settingsService, _apiService);
                serversForm.Show();
            }
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                BtnBack_Click(this, EventArgs.Empty);
            }
        }
    }
}