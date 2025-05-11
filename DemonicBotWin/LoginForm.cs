using DemonicBotWin.WinForms.Services;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemonicBotWin.WinForms
{
    public partial class LoginForm : Form
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

        // Controls
        private Label lblTitle;
        private Label lblApiUrl;
        private TextBox txtApiUrl;
        private Label lblApiSecret;
        private TextBox txtApiSecret;
        private Label lblUserName;
        private TextBox txtUserName;
        private Button btnLogin;
        private Label lblError;
        private ProgressBar progressBar;

        public LoginForm()
        {
            _settingsService = new SettingsService();
            _apiService = new ApiService(_settingsService);
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form Settings
            this.Text = "DemonicBot - Login";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _discordBackground;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "Discord Bot App",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(0, 40),
                Size = new Size(this.ClientSize.Width, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // API URL
            lblApiUrl = new Label
            {
                Text = "API URL",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(40, 120),
                Size = new Size(100, 20)
            };

            txtApiUrl = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 145),
                Size = new Size(300, 25),
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
                Location = new Point(40, 190),
                Size = new Size(100, 20)
            };

            txtApiSecret = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 215),
                Size = new Size(300, 25),
                PlaceholderText = "Dein geheimer API-Schlüssel",
                BackColor = _discordLighter,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            // User Name
            lblUserName = new Label
            {
                Text = "Benutzername (optional)",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(40, 260),
                Size = new Size(200, 20)
            };

            txtUserName = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 285),
                Size = new Size(300, 25),
                PlaceholderText = "Dein Benutzername",
                BackColor = _discordLighter,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Login Button
            btnLogin = new Button
            {
                Text = "Anmelden",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(40, 340),
                Size = new Size(300, 40),
                BackColor = _discordBlurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Error Label
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = _discordRed,
                Location = new Point(40, 390),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.TopCenter
            };

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(40, 420),
                Size = new Size(300, 10),
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
                btnLogin,
                lblError,
                progressBar
            });

            this.ResumeLayout(false);
        }

        private async void LoadSettings()
        {
            txtApiUrl.Text = _settingsService.GetSetting(SettingsKeys.API_URL_KEY);
            txtApiSecret.Text = _settingsService.GetSetting(SettingsKeys.API_SECRET_KEY);
            txtUserName.Text = _settingsService.GetSetting(SettingsKeys.USER_NAME_KEY);

            // Auto-login if settings exist
            if (!string.IsNullOrEmpty(txtApiUrl.Text) && !string.IsNullOrEmpty(txtApiSecret.Text))
            {
                await LoginAsync();
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            await LoginAsync();
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(txtApiUrl.Text) || string.IsNullOrEmpty(txtApiSecret.Text))
            {
                lblError.Text = "Bitte gib die API-URL und den API-Schlüssel ein.";
                return;
            }

            btnLogin.Enabled = false;
            progressBar.Visible = true;
            lblError.Text = "";

            try
            {
                // Save settings
                _settingsService.SaveSetting(SettingsKeys.API_URL_KEY, txtApiUrl.Text);
                _settingsService.SaveSetting(SettingsKeys.API_SECRET_KEY, txtApiSecret.Text);
                if (!string.IsNullOrEmpty(txtUserName.Text))
                {
                    _settingsService.SaveSetting(SettingsKeys.USER_NAME_KEY, txtUserName.Text);
                }

                // Initialize API
                var success = await _apiService.InitializeAsync();

                if (!success)
                {
                    lblError.Text = "Die API konnte nicht initialisiert werden.";
                    return;
                }

                // Navigate to servers form
                var serversForm = new ServersForm(_settingsService, _apiService);
                serversForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                lblError.Text = $"Fehler bei der Anmeldung: {ex.Message}";
            }
            finally
            {
                btnLogin.Enabled = true;
                progressBar.Visible = false;
            }
        }
    }
}