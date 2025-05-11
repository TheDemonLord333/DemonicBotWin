using DemonicBotWin.WinForms.Models;
using DemonicBotWin.WinForms.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemonicBotWin.WinForms
{
    public partial class EmbedCreatorForm : Form
    {
        private readonly ISettingsService _settingsService;
        private readonly IApiService _apiService;
        private readonly DiscordServer _server;
        private readonly DiscordChannel _channel;

        // Discord-ähnliche Farben
        private readonly Color _discordBackground = Color.FromArgb(54, 57, 63);
        private readonly Color _discordDarker = Color.FromArgb(47, 49, 54);
        private readonly Color _discordLighter = Color.FromArgb(64, 68, 75);
        private readonly Color _discordBlurple = Color.FromArgb(88, 101, 242);
        private readonly Color _discordText = Color.FromArgb(220, 221, 222);
        private readonly Color _discordRed = Color.FromArgb(237, 66, 69);

        private TabControl tabControl;
        private TabPage tabBasic;
        private TabPage tabFields;
        private TabPage tabPreview;

        // Basic Tab Controls
        private Label lblChannelInfo;
        private Label lblTitle;
        private TextBox txtTitle;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblColor;
        private ComboBox cmbColor;
        private Label lblImageUrl;
        private TextBox txtImageUrl;
        private Label lblThumbnailUrl;
        private TextBox txtThumbnailUrl;
        private Label lblFooter;
        private TextBox txtFooter;
        private CheckBox chkTimestamp;

        // Fields Tab Controls
        private ListBox lstFields;
        private Button btnAddField;
        private Button btnRemoveField;
        private TextBox txtFieldName;
        private TextBox txtFieldValue;
        private CheckBox chkFieldInline;

        // Preview Tab Controls
        private Panel pnlPreview;
        private RichTextBox rtbPreview;

        // Action Buttons
        private Button btnSend;
        private Button btnBack;
        private Label lblError;
        private ProgressBar progressBar;

        private List<EmbedField> _fields = new List<EmbedField>();

        public EmbedCreatorForm(ISettingsService settingsService, IApiService apiService,
            DiscordServer server, DiscordChannel channel)
        {
            _settingsService = settingsService;
            _apiService = apiService;
            _server = server;
            _channel = channel;
            InitializeComponent();
            UpdatePreview();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Form Settings
            this.Text = $"DemonicBot - Embed erstellen für #{_channel.Name}";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = _discordBackground;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Tab Control
            tabControl = new TabControl
            {
                Location = new Point(20, 60),
                Size = new Size(740, 520),
                Appearance = TabAppearance.FlatButtons,
                BackColor = _discordBackground
            };

            // Basic Tab
            tabBasic = new TabPage("Basis")
            {
                BackColor = _discordBackground
            };
            InitializeBasicTab();
            tabControl.TabPages.Add(tabBasic);

            // Fields Tab
            tabFields = new TabPage("Felder")
            {
                BackColor = _discordBackground
            };
            InitializeFieldsTab();
            tabControl.TabPages.Add(tabFields);

            // Preview Tab
            tabPreview = new TabPage("Vorschau")
            {
                BackColor = _discordBackground
            };
            InitializePreviewTab();
            tabControl.TabPages.Add(tabPreview);

            // Channel Info
            lblChannelInfo = new Label
            {
                Text = $"Senden an #{_channel.Name} in {_server.Name}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(20, 20),
                Size = new Size(500, 30)
            };

            // Back Button
            btnBack = new Button
            {
                Text = "Zurück",
                Font = new Font("Segoe UI", 10),
                Location = new Point(680, 20),
                Size = new Size(80, 30),
                BackColor = _discordLighter,
                ForeColor = _discordText,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Click += BtnBack_Click;

            // Send Button
            btnSend = new Button
            {
                Text = "Embed senden",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(20, 590),
                Size = new Size(740, 40),
                BackColor = _discordBlurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += async (s, e) => await SendEmbedAsync();

            // Error Label
            lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9),
                ForeColor = _discordRed,
                Location = new Point(20, 640),
                Size = new Size(740, 20),
                TextAlign = ContentAlignment.TopCenter
            };

            // Progress Bar
            progressBar = new ProgressBar
            {
                Location = new Point(20, 665),
                Size = new Size(740, 10),
                Style = ProgressBarStyle.Marquee,
                Visible = false
            };

            // Add controls to form
            this.Controls.AddRange(new Control[]
            {
                lblChannelInfo,
                btnBack,
                tabControl,
                btnSend,
                lblError,
                progressBar
            });

            this.ResumeLayout(false);
        }

        private void InitializeBasicTab()
        {
            int y = 20;

            lblTitle = CreateLabel("Titel", 20, y);
            txtTitle = CreateTextBox(20, y + 25, 700, "Embed-Titel eingeben");
            txtTitle.TextChanged += (s, e) => UpdatePreview();

            y += 70;
            lblDescription = CreateLabel("Beschreibung", 20, y);
            txtDescription = CreateTextBox(20, y + 25, 700, "Embed-Beschreibung eingeben", 80);
            txtDescription.Multiline = true;
            txtDescription.ScrollBars = ScrollBars.Vertical;
            txtDescription.TextChanged += (s, e) => UpdatePreview();

            y += 130;
            lblColor = CreateLabel("Farbe", 20, y);
            cmbColor = new ComboBox
            {
                Location = new Point(20, y + 25),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = _discordLighter,
                ForeColor = _discordText
            };
            cmbColor.Items.AddRange(new[] { "Blurple", "Grün", "Gelb", "Rot", "Grau", "Schwarz", "Weiß" });
            cmbColor.SelectedIndex = 0;
            cmbColor.SelectedIndexChanged += (s, e) => UpdatePreview();

            y += 60;
            lblImageUrl = CreateLabel("Bild URL (optional)", 20, y);
            txtImageUrl = CreateTextBox(20, y + 25, 700, "URL zu einem Bild");
            txtImageUrl.TextChanged += (s, e) => UpdatePreview();

            y += 60;
            lblThumbnailUrl = CreateLabel("Thumbnail URL (optional)", 20, y);
            txtThumbnailUrl = CreateTextBox(20, y + 25, 340, "URL zu einem Thumbnail");

            lblFooter = CreateLabel("Footer Text (optional)", 380, y);
            txtFooter = CreateTextBox(380, y + 25, 340, "Footer Text");
            txtFooter.TextChanged += (s, e) => UpdatePreview();

            y += 60;
            chkTimestamp = new CheckBox
            {
                Text = "Zeitstempel hinzufügen",
                Location = new Point(20, y),
                Size = new Size(200, 25),
                ForeColor = _discordText,
                Checked = true
            };
            chkTimestamp.CheckedChanged += (s, e) => UpdatePreview();

            tabBasic.Controls.AddRange(new Control[]
            {
                lblTitle, txtTitle,
                lblDescription, txtDescription,
                lblColor, cmbColor,
                lblImageUrl, txtImageUrl,
                lblThumbnailUrl, txtThumbnailUrl,
                lblFooter, txtFooter,
                chkTimestamp
            });
        }

        private void InitializeFieldsTab()
        {
            lstFields = new ListBox
            {
                Location = new Point(20, 20),
                Size = new Size(300, 400),
                BackColor = _discordDarker,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.None
            };

            btnAddField = new Button
            {
                Text = "Feld hinzufügen",
                Location = new Point(340, 20),
                Size = new Size(150, 30),
                BackColor = _discordBlurple,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddField.Click += BtnAddField_Click;

            btnRemoveField = new Button
            {
                Text = "Feld entfernen",
                Location = new Point(340, 60),
                Size = new Size(150, 30),
                BackColor = _discordRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRemoveField.Click += BtnRemoveField_Click;

            var lblFieldName = CreateLabel("Feldname", 340, 110);
            txtFieldName = CreateTextBox(340, 135, 360, "Feldname");

            var lblFieldValue = CreateLabel("Feldinhalt", 340, 170);
            txtFieldValue = CreateTextBox(340, 195, 360, "Feldinhalt", 100);
            txtFieldValue.Multiline = true;
            txtFieldValue.ScrollBars = ScrollBars.Vertical;

            chkFieldInline = new CheckBox
            {
                Text = "Inline-Anzeige",
                Location = new Point(340, 310),
                Size = new Size(200, 25),
                ForeColor = _discordText
            };

            tabFields.Controls.AddRange(new Control[]
            {
                lstFields,
                btnAddField,
                btnRemoveField,
                lblFieldName, txtFieldName,
                lblFieldValue, txtFieldValue,
                chkFieldInline
            });
        }

        private void InitializePreviewTab()
        {
            rtbPreview = new RichTextBox
            {
                Location = new Point(20, 20),
                Size = new Size(700, 470),
                BackColor = _discordDarker,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Font = new Font("Segoe UI", 10)
            };

            tabPreview.Controls.Add(rtbPreview);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = _discordText,
                Location = new Point(x, y),
                Size = new Size(200, 20)
            };
        }

        private TextBox CreateTextBox(int x, int y, int width, string placeholder, int height = 25)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(x, y),
                Size = new Size(width, height),
                PlaceholderText = placeholder,
                BackColor = _discordLighter,
                ForeColor = _discordText,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void BtnAddField_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFieldName.Text) || string.IsNullOrEmpty(txtFieldValue.Text))
            {
                lblError.Text = "Bitte gib Feldname und Inhalt ein.";
                return;
            }

            var field = new EmbedField
            {
                Name = txtFieldName.Text,
                Value = txtFieldValue.Text,
                Inline = chkFieldInline.Checked
            };

            _fields.Add(field);
            lstFields.Items.Add($"{field.Name}: {field.Value}");

            txtFieldName.Text = "";
            txtFieldValue.Text = "";
            chkFieldInline.Checked = false;
            lblError.Text = "";

            UpdatePreview();
        }

        private void BtnRemoveField_Click(object sender, EventArgs e)
        {
            if (lstFields.SelectedIndex >= 0)
            {
                _fields.RemoveAt(lstFields.SelectedIndex);
                lstFields.Items.RemoveAt(lstFields.SelectedIndex);
                UpdatePreview();
            }
        }

        private void UpdatePreview()
        {
            rtbPreview.Clear();
            rtbPreview.SelectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            rtbPreview.SelectionColor = _discordText;
            rtbPreview.AppendText($"{(string.IsNullOrEmpty(txtTitle.Text) ? "Vorschau-Titel" : txtTitle.Text)}\n\n");

            rtbPreview.SelectionFont = new Font("Segoe UI", 10);
            rtbPreview.SelectionColor = _discordText;
            rtbPreview.AppendText($"{(string.IsNullOrEmpty(txtDescription.Text) ? "Vorschau-Beschreibung" : txtDescription.Text)}\n\n");

            if (_fields.Count > 0)
            {
                rtbPreview.AppendText("Felder:\n");
                foreach (var field in _fields)
                {
                    rtbPreview.SelectionFont = new Font("Segoe UI", 10, FontStyle.Bold);
                    rtbPreview.AppendText($"{field.Name}: ");
                    rtbPreview.SelectionFont = new Font("Segoe UI", 10);
                    rtbPreview.AppendText($"{field.Value}\n");
                }
            }

            if (!string.IsNullOrEmpty(txtFooter.Text))
            {
                rtbPreview.AppendText($"\n{txtFooter.Text}");
            }

            if (chkTimestamp.Checked)
            {
                rtbPreview.AppendText($"\n{DateTime.Now:dd.MM.yyyy HH:mm}");
            }
        }

        private async Task SendEmbedAsync()
        {
            if (string.IsNullOrEmpty(txtTitle.Text) || string.IsNullOrEmpty(txtDescription.Text))
            {
                lblError.Text = "Titel und Beschreibung sind erforderlich.";
                return;
            }

            btnSend.Enabled = false;
            progressBar.Visible = true;
            lblError.Text = "";

            try
            {
                var embed = new EmbedMessage
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    Color = GetColorFromSelection(),
                    Fields = _fields,
                    Timestamp = chkTimestamp.Checked ? DateTime.Now : null
                };

                if (!string.IsNullOrEmpty(txtImageUrl.Text))
                    embed.Image = new EmbedImage { Url = txtImageUrl.Text };

                if (!string.IsNullOrEmpty(txtThumbnailUrl.Text))
                    embed.Thumbnail = new EmbedThumbnail { Url = txtThumbnailUrl.Text };

                if (!string.IsNullOrEmpty(txtFooter.Text))
                    embed.Footer = new EmbedFooter { Text = txtFooter.Text };

                var success = await _apiService.SendEmbedAsync(_channel.Id, embed);

                if (success)
                {
                    MessageBox.Show("Embed-Nachricht wurde erfolgreich gesendet!",
                        "Erfolg", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BtnBack_Click(this, EventArgs.Empty);
                }
                else
                {
                    lblError.Text = "Beim Senden der Embed-Nachricht ist ein Fehler aufgetreten.";
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Fehler: {ex.Message}";
            }
            finally
            {
                btnSend.Enabled = true;
                progressBar.Visible = false;
            }
        }

        private string GetColorFromSelection()
        {
            return cmbColor.SelectedItem.ToString() switch
            {
                "Blurple" => "#5865F2",
                "Grün" => "#57F287",
                "Gelb" => "#FEE75C",
                "Rot" => "#ED4245",
                "Grau" => "#95A5A6",
                "Schwarz" => "#000000",
                "Weiß" => "#FFFFFF",
                _ => "#5865F2"
            };
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            var channelsForm = new ChannelsForm(_settingsService, _apiService, _server);
            channelsForm.Show();
            this.Close();
        }
    }
}