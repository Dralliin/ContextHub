using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ContextHubDev
{
    public class MainControlPanel : Form
    {
        private List<FenceForm> activeFences = new List<FenceForm>();
        private ListBox fencesListBox;
        private TextBox newFenceTextBox;
        
        private TrackBar? widthTrackBar;
        private TrackBar? heightTrackBar;
        private Label? sizeInfoLabel;
        private int startX = 100;
        
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public Form? ToggleForm { get; set; }

        public MainControlPanel()
        {
            this.Text = "ContextHub - Контрольна Панель";
            this.Size = new Size(500, 460);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing) {
                    if (activeFences.Count == 0) {
                        Application.Exit();
                    } 
                    else {
                        e.Cancel = true;
                        this.Hide();
                    }
                }
            };

            Label nameLabel = new Label() { Text = "Назва нової плитки:", Location = new Point(20, 20), Size = new Size(150, 20) };
            this.Controls.Add(nameLabel);

            newFenceTextBox = new TextBox() { Location = new Point(20, 45), Size = new Size(240, 25) };
            this.Controls.Add(newFenceTextBox);

            Button addBtn = new Button() { Text = "Додати плитку", Location = new Point(280, 43), Size = new Size(180, 28) };
            addBtn.Click += AddNewFence;
            this.Controls.Add(addBtn);

            Label listLabel = new Label() { Text = "Активні плитки (Виберіть для зміни розміру):", Location = new Point(20, 90), Size = new Size(300, 20) };
            this.Controls.Add(listLabel);

            fencesListBox = new ListBox() { Location = new Point(20, 115), Size = new Size(440, 100) };
            fencesListBox.SelectedIndexChanged += SelectedFenceChanged;
            this.Controls.Add(fencesListBox);

            TrackBarSetup();

            Button hideFencesBtn = new Button() {
                Text = "❌ СХОВАТИ ВСІ ПЛИТКИ",
                Location = new Point(20, 375),
                Size = new Size(440, 35),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            hideFencesBtn.FlatAppearance.BorderSize = 0;
            hideFencesBtn.Click += HideAllFencesAndToggle;
            this.Controls.Add(hideFencesBtn);

            InitDefaultFences();
        }

        private void TrackBarSetup()
        {
            widthTrackBar = new TrackBar() { Location = new Point(90, 230), Size = new Size(370, 45), Minimum = 150, Maximum = 800, Value = 310 };
            widthTrackBar.Scroll += SizeTrackBarsScroll;
            this.Controls.Add(widthTrackBar);

            heightTrackBar = new TrackBar() { Location = new Point(90, 280), Size = new Size(370, 45), Minimum = 100, Maximum = 800, Value = 420 };
            heightTrackBar.Scroll += SizeTrackBarsScroll;
            this.Controls.Add(heightTrackBar);

            sizeInfoLabel = new Label() { Text = "Розмір: 310 x 420 px", Location = new Point(20, 340), Size = new Size(200, 20), ForeColor = Color.DarkCyan };
            this.Controls.Add(sizeInfoLabel);
        }

        public void InitDefaultFences()
        {
            if (activeFences.Count > 0) return;

            CreateFence("University Work", 50, 150);
            CreateFence("Entertainment", 380, 150);
            CreateFence("General Desktop", 710, 150);
            ScanDesktopAndSort();

            if (ToggleForm != null) ToggleForm.Show();
        }

        private void CreateFence(string name, int x, int y)
        {
            FenceData data = new FenceData { Name = name, X = x, Y = y, W = 310, H = 420 };
            FenceForm form = new FenceForm(data);
            form.Show();

            activeFences.Add(form);
            fencesListBox.Items.Add(data.Name);
        }

        private void ScanDesktopAndSort()
        {
            foreach (var f in activeFences) f.ClearFiles();

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (!Directory.Exists(desktopPath)) return;

            string[] files = Directory.GetFiles(desktopPath);

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string lowerName = fileName.ToLower();

                FenceForm target = activeFences.Find(f => f.Data.Name == "General Desktop")!;

                if (lowerName.Contains("code") || lowerName.Contains("vs") || lowerName.Contains("uni") || lowerName.Contains("lab") || lowerName.Contains("intern"))
                {
                    target = activeFences.Find(f => f.Data.Name == "University Work")!;
                }
                else if (lowerName.Contains("game") || lowerName.Contains("steam") || lowerName.Contains("cs") || lowerName.Contains("play"))
                {
                    target = activeFences.Find(f => f.Data.Name == "Entertainment")!;
                }

                target.AddFile(fileName, file);
            }
        }

        private void HideAllFencesAndToggle(object? sender, EventArgs e)
        {
            foreach (var fence in activeFences)
            {
                fence.Close();
            }
            activeFences.Clear();
            fencesListBox.Items.Clear();

            if (ToggleForm != null)
            {
                ToggleForm.Hide();
            }
        }

        private void AddNewFence(object? sender, EventArgs e)
        {
            string name = newFenceTextBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;
            
            if (activeFences.Count == 0) InitDefaultFences();

            startX += 30;
            CreateFence(name, startX, 300);
            newFenceTextBox.Clear();
            fencesListBox.SelectedIndex = fencesListBox.Items.Count - 1;
        }

        private void SelectedFenceChanged(object? sender, EventArgs e)
        {
            int index = fencesListBox.SelectedIndex;
            if (index >= 0 && index < activeFences.Count && widthTrackBar != null && heightTrackBar != null && sizeInfoLabel != null) {
                FenceForm selectedForm = activeFences[index];
                widthTrackBar.Value = selectedForm.Width;
                heightTrackBar.Value = selectedForm.Height;
                sizeInfoLabel.Text = $"Розмір: {selectedForm.Width} x {selectedForm.Height} px";
            }
        }

        private void SizeTrackBarsScroll(object? sender, EventArgs e)
        {
            int index = fencesListBox.SelectedIndex;
            if (index >= 0 && index < activeFences.Count && widthTrackBar != null && heightTrackBar != null && sizeInfoLabel != null) {
                FenceForm selectedForm = activeFences[index];
                selectedForm.UpdateSize(widthTrackBar.Value, heightTrackBar.Value);
                sizeInfoLabel.Text = $"Розмір: {widthTrackBar.Value} x {heightTrackBar.Value} px";
            }
        }
    }

    public class QuickToggleForm : Form
    {
        private MainControlPanel _panel;
        private const int HeaderHeight = 35;
        private string _title = "⚙️ COMPONENT HUB";

        public QuickToggleForm(MainControlPanel panel)
        {
            _panel = panel;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(50, 40);
            this.Size = new Size(220, 100);
            this.BackColor = Color.FromArgb(45, 52, 54);
            this.Opacity = 0.85;
            this.ShowInTaskbar = false;

            Button btn = new Button()
            {
                Text = "МЕНЮ КЕРУВАННЯ",
                Location = new Point(15, 48),
                Size = new Size(190, 35),
                BackColor = Color.FromArgb(65, 72, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(100, Color.White);

            btn.Click += (s, e) => {
                if (_panel.Visible) {
                    _panel.Hide();
                } else {
                    _panel.Show();
                    _panel.InitDefaultFences(); 
                }
            };

            this.Controls.Add(btn);

            this.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left && e.Y <= HeaderHeight) {
                    SystemStuff.ReleaseCapture();
                    SystemStuff.SendMessage(this.Handle, 0xA1, 0x2, 0);
                }
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (Brush b = new SolidBrush(Color.FromArgb(80, Color.Black))) {
                e.Graphics.FillRectangle(b, 0, 0, this.Width, HeaderHeight);
            }

            using (Font f = new Font("Segoe UI", 9, FontStyle.Bold)) {
                e.Graphics.DrawString(_title, f, Brushes.White, 12, 10);
            }

            using (Pen p = new Pen(Color.FromArgb(120, Color.White), 2)) {
                e.Graphics.DrawRectangle(p, 1, 1, this.Width - 2, this.Height - 2);
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            MainControlPanel mainPanel = new MainControlPanel();
            QuickToggleForm toggle = new QuickToggleForm(mainPanel);
            
            mainPanel.ToggleForm = toggle;

            mainPanel.Show(); 
            toggle.Show();

            Application.Run(mainPanel);
        }
    }
}
