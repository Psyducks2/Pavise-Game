using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaviseApp
{
    internal sealed class SuppressedProcessesDialog : Form
    {
        private readonly GameMode gameMode;
        private readonly DBPanel listViewport;
        private readonly LibraryDialogButton btnReleaseAll;
        private readonly LibraryDialogButton btnClose;
        private readonly Label lblCount;

        internal SuppressedProcessesDialog(GameMode gameMode)
        {
            this.gameMode = gameMode;
            base.AutoScaleMode = AutoScaleMode.None;
            base.FormBorderStyle = FormBorderStyle.None;
            base.StartPosition = FormStartPosition.CenterParent;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.ShowInTaskbar = false;
            BackColor = Theme.Bg;
            ForeColor = Theme.Fg;
            Font = Theme.UI(9f, false);
            DoubleBuffered = true;

            string title = Lang.Text("后台进程治理与快速放行", "Background Governance & Quick Fallback");
            Text = title;
            base.AccessibleName = Text;

            int dialogW = Theme.S(580);
            int dialogH = Theme.S(520);
            base.ClientSize = new Size(dialogW, dialogH);

            int left = Theme.S(24);
            int contentW = dialogW - left * 2;

            Label badge = MakeLabel("BACKGROUND // GOVERNANCE FALLBACK", Theme.Mono(7.5f), Theme.Accent);
            badge.SetBounds(left, Theme.S(18), contentW, Theme.S(18));

            Label lblTitle = MakeLabel(title, Theme.UI(14f, true), Theme.Fg);
            lblTitle.SetBounds(left, Theme.S(40), contentW, Theme.S(28));

            string desc = Lang.Text(
                "以下进程当前处于后台性能治理状态。如果您在使用某个程序时遇到卡顿，可在此一键放行并加白，或直接解除全部治理。",
                "The processes below are currently throttled by Pavise. If you experience lag or stuttering in any app, click to unthrottle and whitelist it, or release all processes at once.");
            Label lblDesc = MakeLabel(desc, Theme.UI(8.8f, false), Theme.Dim);
            lblDesc.AutoEllipsis = false;
            int descH = TextRenderer.MeasureText(desc, Theme.UI(8.8f, false), new Size(contentW, int.MaxValue), TextFormatFlags.WordBreak).Height + Theme.S(8);
            lblDesc.SetBounds(left, Theme.S(72), contentW, descH);

            int listTop = lblDesc.Bottom + Theme.S(10);
            int footerH = Theme.S(64);
            int listH = dialogH - listTop - footerH - Theme.S(12);

            lblCount = MakeLabel("", Theme.UI(8.5f, false), Theme.Dim);
            lblCount.SetBounds(left, listTop, contentW, Theme.S(20));

            listTop += Theme.S(24);
            listH -= Theme.S(24);

            listViewport = new DBPanel
            {
                Name = "SuppressedListViewport",
                BackColor = Theme.Card,
                AutoScroll = true
            };
            Native.Dark(listViewport);
            listViewport.SetBounds(left, listTop, contentW, listH);

            // Bottom action buttons
            int bottomY = dialogH - footerH + Theme.S(14);
            int btnH = Theme.S(38);

            btnReleaseAll = new LibraryDialogButton(Lang.Text("全部解除治理 (紧急回退)", "Release All (Emergency Fallback)"), false);
            btnReleaseAll.Click += OnReleaseAllClicked;
            btnReleaseAll.SetBounds(left, bottomY, Theme.S(230), btnH);

            btnClose = new LibraryDialogButton(Lang.Text("关闭", "Close"), true);
            btnClose.DialogResult = DialogResult.OK;
            btnClose.SetBounds(dialogW - left - Theme.S(110), bottomY, Theme.S(110), btnH);

            base.Controls.AddRange(new Control[] { badge, lblTitle, lblDesc, lblCount, listViewport, btnReleaseAll, btnClose });
            base.AcceptButton = btnClose;
            base.CancelButton = btnClose;

            PopulateProcesses();
        }

        private void PopulateProcesses()
        {
            listViewport.SuspendLayout();
            listViewport.Controls.Clear();

            List<SuppressedProcessDetail> list = gameMode != null
                ? gameMode.GetCurrentlySuppressedProcesses()
                : new List<SuppressedProcessDetail>();

            lblCount.Text = Lang.Format("当前受治理进程数: {0}", "Governed processes: {0}", list.Count);

            if (list.Count == 0)
            {
                Label empty = MakeLabel(
                    Lang.Text("当前没有被后台治理限制的进程。", "No background processes are currently throttled."),
                    Theme.UI(9.5f, false), Theme.Dim);
                empty.TextAlign = ContentAlignment.MiddleCenter;
                empty.Dock = DockStyle.Fill;
                listViewport.Controls.Add(empty);
                btnReleaseAll.Enabled = false;
            }
            else
            {
                btnReleaseAll.Enabled = true;
                int rowH = Theme.S(44);
                int y = Theme.S(6);
                int rowW = listViewport.ClientSize.Width - Theme.S(14);
                if (rowW < Theme.S(300)) rowW = listViewport.Width - Theme.S(28);

                foreach (var detail in list)
                {
                    DBPanel row = new DBPanel
                    {
                        BackColor = Theme.Inset,
                        Height = rowH,
                        Width = rowW,
                        Left = Theme.S(6),
                        Top = y
                    };

                    Label nameLbl = MakeLabel(detail.Name, Theme.UI(9.2f, true), Theme.Fg);
                    nameLbl.SetBounds(Theme.S(10), Theme.S(6), Theme.S(240), Theme.S(18));

                    string info = string.Format("PID: {0}  [{1}]", detail.Pid, detail.Level);
                    Label pidLbl = MakeLabel(info, Theme.Mono(7.8f), Theme.Dim);
                    pidLbl.SetBounds(Theme.S(10), Theme.S(24), Theme.S(240), Theme.S(16));

                    var detailRef = detail;
                    LibraryDialogButton btnWhitelist = new LibraryDialogButton(Lang.Text("+ 加白放行", "+ Whitelist"), false);
                    btnWhitelist.Font = Theme.UI(8.2f, false);
                    btnWhitelist.SetBounds(rowW - Theme.S(120), Theme.S(7), Theme.S(112), Theme.S(28));
                    btnWhitelist.Click += delegate
                    {
                        if (gameMode != null)
                        {
                            gameMode.ReleaseAndWhitelistProcess(detailRef.Pid, detailRef.Name);
                            btnWhitelist.Text = Lang.Text("✓ 已放行", "✓ Whitelisted");
                            btnWhitelist.Enabled = false;
                        }
                    };

                    row.Controls.Add(nameLbl);
                    row.Controls.Add(pidLbl);
                    row.Controls.Add(btnWhitelist);

                    listViewport.Controls.Add(row);
                    y += rowH + Theme.S(4);
                }
            }

            listViewport.ResumeLayout(true);
        }

        private void OnReleaseAllClicked(object sender, EventArgs e)
        {
            if (gameMode != null)
            {
                int released = gameMode.ReleaseAllBackgroundSuppression();
                btnReleaseAll.Text = Lang.Text("✓ 已全部解除", "✓ All Released");
                btnReleaseAll.Enabled = false;
                PopulateProcesses();
                PaviseDialog.Info(this, "PAVISE AUTO",
                    Lang.Format("已紧急解除 {0} 个进程的后台治理！", "Emergency release completed: {0} processes restored to normal.", released));
            }
        }

        private static Label MakeLabel(string text, Font font, Color color)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = font;
            label.ForeColor = color;
            label.BackColor = Color.Transparent;
            label.UseCompatibleTextRendering = false;
            label.UseMnemonic = false;
            return label;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            btnClose.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, base.Width - 1, base.Height - 1);
            using (GraphicsPath path = Theme.TechPath(r, Theme.S(12)))
            {
                using (Pen pen = new Pen(Theme.StrokeHi))
                {
                    graphics.DrawPath(pen, path);
                }
            }
            using (Pen pen2 = new Pen(Theme.Accent, Math.Max(1, Theme.S(2))))
            {
                graphics.DrawLine(pen2, Theme.S(1), Theme.S(1), Theme.S(120), Theme.S(1));
                graphics.DrawLine(pen2, base.Width - Theme.S(14), Theme.S(1), base.Width - Theme.S(1), Theme.S(14));
            }
            int dividerY = btnClose.Top - Theme.S(12);
            using (Pen pen3 = new Pen(Theme.Stroke))
            {
                graphics.DrawLine(pen3, Theme.S(24), dividerY, base.Width - Theme.S(24), dividerY);
            }
        }
    }
}
