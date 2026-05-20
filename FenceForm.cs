using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace ContextHubDev
{
    public class FenceForm : Form
    {
        public FenceData Data { get; private set; }
        private const int HeaderHeight = 35;
        private ListBox filesList;
        private List<string> filePaths = new List<string>();

        public FenceForm(FenceData data)
        {
            Data = data;

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(Data.X, Data.Y);
            this.Size = new Size(Data.W, Data.H);
            this.BackColor = Color.FromArgb(45, 52, 54);
            this.Opacity = 0.85;
            this.ShowInTaskbar = false; // Повертаємо справжній стиль утиліти

            // Ініціалізація списку файлів усередині плитки
            filesList = new ListBox()
            {
                Location = new Point(10, HeaderHeight + 10),
                Size = new Size(this.Width - 20, this.Height - HeaderHeight - 20),
                BackColor = Color.FromArgb(55, 62, 64),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            
            // Запуск файлу за подвійним кліком
            filesList.DoubleClick += (s, e) => {
                if (filesList.SelectedIndex >= 0) {
                    try {
                        string targetPath = filePaths[filesList.SelectedIndex];
                        Process.Start(new ProcessStartInfo(targetPath) { UseShellExecute = true });
                    }
                        catch (Exception ex) {
                        MessageBox.Show($"Не вдалося запустити: {ex.Message}");
                    }
                }
            };

            this.Controls.Add(filesList);

            // Перетягування мишкою за шапку
            this.MouseDown += (s, e) => {
                if (e.Button == MouseButtons.Left && e.Y <= HeaderHeight) {
                    SystemStuff.ReleaseCapture();
                    SystemStuff.SendMessage(this.Handle, 0xA1, 0x2, 0);
                    Data.X = this.Location.X;
                    Data.Y = this.Location.Y;
                }
            };
        }

        public void AddFile(string name, string fullPath)
        {
            filesList.Items.Add(name);
            filePaths.Add(fullPath);
        }

        public void ClearFiles()
        {
            filesList.Items.Clear();
            filePaths.Clear();
        }

        public void UpdateSize(int width, int height)
        {
            Data.W = width;
            Data.H = height;
            this.Size = new Size(width, height);
            filesList.Size = new Size(this.Width - 20, this.Height - HeaderHeight - 20);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Brush b = new SolidBrush(Color.FromArgb(90, Color.Black))) {
                e.Graphics.FillRectangle(b, 0, 0, this.Width, HeaderHeight);
            }
            using (Font f = new Font("Segoe UI", 10, FontStyle.Bold)) {
                e.Graphics.DrawString(Data.Name, f, Brushes.White, 12, 8);
            }
            using (Pen p = new Pen(Color.FromArgb(120, Color.White), 2)) {
                e.Graphics.DrawRectangle(p, 1, 1, this.Width - 2, this.Height - 2);
            }
        }
    }
}