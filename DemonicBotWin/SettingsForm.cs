using DemonicBotWin.WinForms.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemonicBotWin.WinForms
{
    public partial class SettingsForm : Form
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
        private Label lblApiUrl;
        private TextBox txtApiUrl;
        private Label lblApiSecret;
        private TextBox txtApiSecret;
        private Label lblUserName;
        private TextBox txtUserName;
        private Button btnSave;
        private Button btnLogout;
        private Button btnCancel;
        private Label lblError;
        private ProgressBar progressBar;

        public SettingsForm(ISettingsService settingsService, IApiService apiService)
        {
            _settingsService = settingsService;
            _apiService = apiService;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form Settings
            this.Text = "DemonicBot - Einstellungen";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = _discordBackground;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "Einstellungen",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(30, 20),
                Size = new Size(200, 40)
            };

            // API URL
            lblApiUrl = new Label
            {
                Text = "API URL",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(30, 80),
                Size = new Size(100, 20)
            };

            txtApiUrl = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 105),
                Size = new Size(420, 25),
                PlaceholderText = "http://deine-server-ip:3000",
                BackColor = _discordLighter,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.FixedSingle
            };

            // API Secret
            lblApiSecret = new Label
            {
                Text = "API Schlüssel",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(30, 150),
                Size = new Size(100, 20)
            };

            txtApiSecret = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 175),
                Size = new Size(420, 25),
                PlaceholderText = "Dein geheimer API-Schlüssel",
                BackColor = _discordLighter,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // User Name
            lblUserName = new Label
            {
                Text = "Benutzername",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(30, 220),
                Size = new Size(100, 20)
            };

            txtUserName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(30, 245),
                Size = new Size(420, 25),
                PlaceholderText = "Dein Benutzername",
                BackColor = _discordLighter,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Save Button
            btnSave = new Button
            {
                Text = "Speichern",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(30, 300),
                Size = new Size(135, 40),
                BackColor = _discordBlurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += async (s, e) => await SaveSettingsAsync();

            // Logout Button
            btnLogout = new Button
            {
                Text = "Abmelden",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(175, 300),
                Size = new Size(135, 40),
                BackColor = _discordRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += async (s, e) => await LogoutAsync();

            // Cancel Button
            btnCancel = new Button
            {
                Text = "Abbrechen",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(320, 300),
                Size = new Size(130, 40),
                BackColor = _discordLighter,
                ForeColor = _discordText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            // Error Label
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = _discordRed,
                Location = new Point(30, 355),
                Size = new Size(420, 40),
                TextAlign = ContentAlignment.TopCenter
            };

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(30, 400),
                Size = new Size(420, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblTitle,
                lblApiUrl,
                txtApiUrl,
                lblApiSecret,
                txtApiSecret,
                lblUserName,
                txtUserName,
                btnSave,
                btnLogout,
                btnCancel,
                lblError,
                progressBar
            });

            this.ResumeLayout(false);
        }

        private void LoadSettings()
        {
            txtApiUrl.Text = _settingsService.GetSetting(SettingsKeys.API_URL_KEY);
            txtApiSecret.Text = _settingsService.GetSetting(SettingsKeys.API_SECRET_KEY);
            txtUserName.Text = _settingsService.GetSetting(SettingsKeys.USER_NAME_KEY);
        }

        private async Task SaveSettingsAsync()
        {
            if (string.IsNullOrEmpty(txtApiUrl.Text) || string.IsNullOrEmpty(txtApiSecret.Text))
            {
                lblError.Text = "API-URL und API-Schlüssel sind erforderlich.";
                return;
            }

            SetControlsEnabled(false);
            progressBar.Visible = true;
            lblError.Text = "";

            try
            {
                _settingsService.SaveSetting(SettingsKeys.API_URL_KEY, txtApiUrl.Text);
                _settingsService.SaveSetting(SettingsKeys.API_SECRET_KEY, txtApiSecret.Text);
                if (!string.IsNullOrEmpty(txtUserName.Text))
                {
                    _settingsService.SaveSetting(SettingsKeys.USER_NAME_KEY, txtUserName.Text);
                }

                var success = await _apiService.InitializeAsync();

                if (success)
                {
                    MessageBox.Show("Einstellungen wurden erfolgreich gespeichert.",
                        "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    lblError.Text = "Die API konnte mit den neuen Einstellungen nicht initialisiert werden.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Fehler beim Speichern der Einstellungen: {ex.Message}";
            }
            finally
            {
                SetControlsEnabled(true);
                progressBar.Visible = false;
            }
        }

        private async Task LogoutAsync()
        {
            var result = MessageBox.Show(
                "Möchtest du dich wirklich abmelden? Alle gespeicherten Einstellungen werden gelöscht.",
                "Abmelden",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            SetControlsEnabled(false);
            progressBar.Visible = true;

            try
            {
                _settingsService.ClearSetting(SettingsKeys.API_URL_KEY);
                _settingsService.ClearSetting(SettingsKeys.API_SECRET_KEY);
                _settingsService.ClearSetting(SettingsKeys.USER_NAME_KEY);

                Application.Exit();
            }
            catch (Exception ex)
            {
                lblError.Text = $"Fehler beim Abmelden: {ex.Message}";
                SetControlsEnabled(true);
                progressBar.Visible = false;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnSave.Enabled = enabled;
            btnLogout.Enabled = enabled;
            btnCancel.Enabled = enabled;
            txtApiUrl.Enabled = enabled;
            txtApiSecret.Enabled = enabled;
            txtUserName.Enabled = enabled;
        }
    }
}