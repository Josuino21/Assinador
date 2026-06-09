using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace AssinadorP7s
{
    internal sealed class MainForm : Form
    {
        private readonly ListBox pdfList = new ListBox();
        private readonly TextBox outputFolderTextBox = new TextBox();
        private readonly TextBox pfxTextBox = new TextBox();
        private readonly TextBox passwordTextBox = new TextBox();
        private readonly Label certificateStatusLabel = new Label();
        private readonly Label statusLabel = new Label();
        private readonly RadioButton embeddedRadio = new RadioButton();
        private readonly RadioButton detachedRadio = new RadioButton();
        private readonly CheckBox safeModeCheckBox = new CheckBox();
        private readonly CheckBox showPasswordCheckBox = new CheckBox();
        private readonly ComboBox hashComboBox = new ComboBox();
        private readonly Button openFolderButton = new Button();
        private TableLayoutPanel contentLayout;
        private Control documentsPanel;
        private Control rightPanel;

        public MainForm()
        {
            Text = AppInfo.Name;
            Width = 1040;
            Height = 720;
            MinimumSize = new Size(760, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);
            LoadWindowIcon();

            BuildLayout();
            ApplySafeMode();
        }

        private void LoadWindowIcon()
        {
            try
            {
                Icon associated = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (associated != null)
                    Icon = associated;
            }
            catch
            {
            }
        }

        private void BuildLayout()
        {
            BackColor = Color.FromArgb(238, 243, 248);

            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 3;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 104));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
            Controls.Add(root);

            root.Controls.Add(BuildHeader(), 0, 0);

            Panel scrollHost = new Panel();
            scrollHost.Dock = DockStyle.Fill;
            scrollHost.AutoScroll = true;
            scrollHost.BackColor = Color.FromArgb(238, 243, 248);

            contentLayout = new TableLayoutPanel();
            contentLayout.Dock = DockStyle.Top;
            contentLayout.AutoSize = true;
            contentLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            contentLayout.Padding = new Padding(18);
            contentLayout.ColumnCount = 2;
            contentLayout.RowCount = 1;
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            documentsPanel = BuildDocumentsPanel();
            contentLayout.Controls.Add(documentsPanel, 0, 0);

            TableLayoutPanel right = new TableLayoutPanel();
            right.Dock = DockStyle.Top;
            right.AutoSize = true;
            right.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            right.RowCount = 2;
            right.ColumnCount = 1;
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            right.Controls.Add(BuildCertificatePanel(), 0, 0);
            right.Controls.Add(BuildSignaturePanel(), 0, 1);
            rightPanel = right;
            contentLayout.Controls.Add(rightPanel, 1, 0);
            scrollHost.Controls.Add(contentLayout);
            root.Controls.Add(scrollHost, 0, 1);

            root.Controls.Add(BuildFooter(), 0, 2);
            Resize += delegate { UpdateResponsiveLayout(); };
            scrollHost.Resize += delegate { UpdateResponsiveLayout(); };
            UpdateResponsiveLayout();
        }

        private Control BuildHeader()
        {
            TableLayoutPanel header = new TableLayoutPanel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Color.FromArgb(16, 96, 168);
            header.Padding = new Padding(26, 18, 26, 14);
            header.ColumnCount = 2;
            header.RowCount = 2;
            header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            header.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

            Label title = new Label();
            title.Text = AppInfo.Name;
            title.ForeColor = Color.White;
            title.Font = new Font("Segoe UI", 21, FontStyle.Bold);
            title.Dock = DockStyle.Fill;
            title.TextAlign = ContentAlignment.BottomLeft;
            header.Controls.Add(title, 0, 0);

            Label badge = new Label();
            badge.Text = "CAdES / SHA-256";
            badge.ForeColor = Color.White;
            badge.BackColor = Color.FromArgb(8, 69, 132);
            badge.TextAlign = ContentAlignment.MiddleCenter;
            badge.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            badge.Margin = new Padding(12, 22, 0, 0);
            badge.MinimumSize = new Size(148, 28);
            badge.AutoSize = true;
            header.Controls.Add(badge, 1, 0);

            Label subtitle = new Label();
            subtitle.Text = "Gere arquivos .pdf.p7s ou .p7s com validacao local antes do envio.";
            subtitle.ForeColor = Color.White;
            subtitle.Dock = DockStyle.Fill;
            subtitle.TextAlign = ContentAlignment.TopLeft;
            subtitle.AutoEllipsis = true;
            header.Controls.Add(subtitle, 0, 1);
            header.SetColumnSpan(subtitle, 2);
            return header;
        }

        private Control BuildDocumentsPanel()
        {
            TableLayoutPanel panel = CardTable();
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 220));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            panel.Controls.Add(SectionTitle("Documentos"), 0, 0);

            pdfList.Dock = DockStyle.Fill;
            pdfList.HorizontalScrollbar = true;
            pdfList.MinimumSize = new Size(220, 110);
            pdfList.BackColor = Color.White;
            panel.Controls.Add(pdfList, 0, 1);

            FlowLayoutPanel buttons = Flow();
            Button add = Button("Adicionar PDFs", 128, 36);
            add.Click += AddPdfs;
            buttons.Controls.Add(add);
            Button remove = Button("Remover", 96, 36);
            remove.Click += delegate
            {
                while (pdfList.SelectedItems.Count > 0)
                    pdfList.Items.Remove(pdfList.SelectedItems[0]);
            };
            buttons.Controls.Add(remove);
            Button clear = Button("Limpar", 88, 36);
            clear.Click += delegate { pdfList.Items.Clear(); };
            buttons.Controls.Add(clear);
            panel.Controls.Add(buttons, 0, 2);

            panel.Controls.Add(SmallBold("Salvar em"), 0, 3);

            TableLayoutPanel outputRow = TwoColumnRow(100);
            outputFolderTextBox.Dock = DockStyle.Fill;
            Button choose = Button("Escolher", 94, 32);
            choose.Click += ChooseOutputFolder;
            outputRow.Controls.Add(outputFolderTextBox, 0, 0);
            outputRow.Controls.Add(choose, 1, 0);
            panel.Controls.Add(outputRow, 0, 4);

            return panel;
        }

        private Control BuildCertificatePanel()
        {
            TableLayoutPanel panel = CardTable();
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            panel.Controls.Add(SectionTitle("Certificado digital"), 0, 0);
            panel.Controls.Add(Muted("Use um PFX/P12 com chave privada."), 0, 1);
            panel.Controls.Add(SmallBold("Arquivo PFX/P12"), 0, 2);

            TableLayoutPanel pfxRow = TwoColumnRow(104);
            pfxTextBox.Dock = DockStyle.Fill;
            Button choose = Button("Selecionar", 98, 32);
            choose.Click += ChoosePfx;
            pfxRow.Controls.Add(pfxTextBox, 0, 0);
            pfxRow.Controls.Add(choose, 1, 0);
            panel.Controls.Add(pfxRow, 0, 3);

            panel.Controls.Add(SmallBold("Senha"), 0, 4);

            TableLayoutPanel passRow = TwoColumnRow(88);
            passwordTextBox.Dock = DockStyle.Fill;
            passwordTextBox.UseSystemPasswordChar = true;
            showPasswordCheckBox.Text = "Mostrar";
            showPasswordCheckBox.Dock = DockStyle.Fill;
            showPasswordCheckBox.CheckedChanged += delegate { passwordTextBox.UseSystemPasswordChar = !showPasswordCheckBox.Checked; };
            passRow.Controls.Add(passwordTextBox, 0, 0);
            passRow.Controls.Add(showPasswordCheckBox, 1, 0);
            panel.Controls.Add(passRow, 0, 5);

            certificateStatusLabel.Text = "Nenhum certificado selecionado";
            certificateStatusLabel.ForeColor = Color.FromArgb(85, 85, 120);
            certificateStatusLabel.Dock = DockStyle.Fill;
            certificateStatusLabel.TextAlign = ContentAlignment.MiddleCenter;
            certificateStatusLabel.AutoEllipsis = true;
            certificateStatusLabel.BackColor = Color.FromArgb(232, 240, 248);
            certificateStatusLabel.Padding = new Padding(8, 0, 8, 0);
            panel.Controls.Add(certificateStatusLabel, 0, 6);

            Button verify = Button("Verificar certificado", 180, 36);
            verify.Dock = DockStyle.Fill;
            verify.Click += VerifyCertificate;
            panel.Controls.Add(verify, 0, 7);
            return panel;
        }

        private Control BuildSignaturePanel()
        {
            TableLayoutPanel panel = CardTable();
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            panel.Controls.Add(SectionTitle("Assinatura"), 0, 0);
            panel.Controls.Add(Muted("O modo seguro e a melhor opcao para teste no portal."), 0, 1);

            safeModeCheckBox.Text = "Modo seguro para envio oficial";
            safeModeCheckBox.Dock = DockStyle.Fill;
            safeModeCheckBox.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            safeModeCheckBox.ForeColor = Color.FromArgb(6, 88, 61);
            safeModeCheckBox.Checked = true;
            safeModeCheckBox.CheckedChanged += delegate { ApplySafeMode(); };
            panel.Controls.Add(safeModeCheckBox, 0, 2);

            embeddedRadio.Text = "Gerar .pdf.p7s com o PDF embutido";
            embeddedRadio.Dock = DockStyle.Fill;
            panel.Controls.Add(embeddedRadio, 0, 3);

            detachedRadio.Text = "Gerar .p7s destacado";
            detachedRadio.Dock = DockStyle.Fill;
            panel.Controls.Add(detachedRadio, 0, 4);

            TableLayoutPanel hashRow = TwoColumnRow(140);
            hashRow.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 56);
            hashRow.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 140);
            hashRow.Controls.Add(SmallBold("Hash"), 0, 0);
            hashComboBox.Items.AddRange(new object[] { "sha256", "sha384", "sha512", "sha1" });
            hashComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            hashComboBox.Dock = DockStyle.Fill;
            hashRow.Controls.Add(hashComboBox, 1, 0);
            panel.Controls.Add(hashRow, 0, 5);

            Label validation = Muted("Validacao local: PDF, certificado, hash e permissao de saida.");
            validation.ForeColor = Color.FromArgb(16, 120, 80);
            validation.Dock = DockStyle.Top;
            panel.Controls.Add(validation, 0, 6);
            return panel;
        }

        private Control BuildFooter()
        {
            TableLayoutPanel footer = new TableLayoutPanel();
            footer.Dock = DockStyle.Fill;
            footer.BackColor = Color.FromArgb(248, 251, 253);
            footer.Padding = new Padding(18, 14, 18, 14);
            footer.ColumnCount = 2;
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 390));

            statusLabel.Text = "Modo seguro: .pdf.p7s embutido, SHA-256 e validacao basica local.";
            statusLabel.Dock = DockStyle.Fill;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.AutoEllipsis = true;
            statusLabel.MinimumSize = new Size(220, 36);
            footer.Controls.Add(statusLabel, 0, 0);

            FlowLayoutPanel actions = Flow();
            actions.Dock = DockStyle.Fill;
            actions.AutoSize = false;
            actions.WrapContents = false;
            actions.FlowDirection = FlowDirection.LeftToRight;
            actions.Height = 42;
            openFolderButton.Text = "Abrir pasta";
            openFolderButton.Enabled = false;
            openFolderButton.Width = 96;
            openFolderButton.Height = 36;
            openFolderButton.Click += OpenOutputFolder;
            actions.Controls.Add(openFolderButton);

            Button signButton = Button("Assinar documento(s)", 154, 36);
            signButton.BackColor = Color.FromArgb(16, 96, 168);
            signButton.ForeColor = Color.White;
            signButton.FlatStyle = FlatStyle.Flat;
            signButton.FlatAppearance.BorderColor = Color.FromArgb(8, 69, 132);
            signButton.Click += SignDocuments;
            actions.Controls.Add(signButton);

            Button closeButton = Button("Sair", 88, 36);
            closeButton.Click += delegate { Close(); };
            actions.Controls.Add(closeButton);
            footer.Controls.Add(actions, 1, 0);
            return footer;
        }

        private void UpdateResponsiveLayout()
        {
            if (contentLayout == null)
                return;

            bool stacked = ClientSize.Width < 900;
            contentLayout.SuspendLayout();

            if (stacked)
            {
                contentLayout.ColumnCount = 1;
                contentLayout.RowCount = 2;
                contentLayout.ColumnStyles.Clear();
                contentLayout.RowStyles.Clear();
                contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                contentLayout.SetCellPosition(documentsPanel, new TableLayoutPanelCellPosition(0, 0));
                contentLayout.SetCellPosition(rightPanel, new TableLayoutPanelCellPosition(0, 1));
            }
            else
            {
                contentLayout.ColumnCount = 2;
                contentLayout.RowCount = 1;
                contentLayout.ColumnStyles.Clear();
                contentLayout.RowStyles.Clear();
                contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
                contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
                contentLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                contentLayout.SetCellPosition(documentsPanel, new TableLayoutPanelCellPosition(0, 0));
                contentLayout.SetCellPosition(rightPanel, new TableLayoutPanelCellPosition(1, 0));
            }

            contentLayout.Width = Math.Max(420, ClientSize.Width - 18);
            contentLayout.ResumeLayout(true);
        }

        private void AddPdfs(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "PDF (*.pdf)|*.pdf|Todos os arquivos (*.*)|*.*";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (string file in dialog.FileNames)
                    {
                        if (!pdfList.Items.Contains(file))
                            pdfList.Items.Add(file);
                    }

                    if (String.IsNullOrWhiteSpace(outputFolderTextBox.Text) && dialog.FileNames.Length > 0)
                        outputFolderTextBox.Text = Path.GetDirectoryName(dialog.FileNames[0]);
                }
            }
        }

        private void ChooseOutputFolder(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    outputFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void ChoosePfx(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Certificado PFX/P12 (*.pfx;*.p12)|*.pfx;*.p12|Todos os arquivos (*.*)|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    pfxTextBox.Text = dialog.FileName;
            }
        }

        private void VerifyCertificate(object sender, EventArgs e)
        {
            try
            {
                X509Certificate2 certificate = PfxCertificateProvider.Load(pfxTextBox.Text, passwordTextBox.Text);
                certificateStatusLabel.Text = certificate.GetNameInfo(X509NameType.SimpleName, false) + " | vence em " + certificate.NotAfter.ToString("dd/MM/yyyy");
                certificateStatusLabel.ForeColor = Color.FromArgb(16, 120, 80);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                certificateStatusLabel.Text = ex.Message;
                certificateStatusLabel.ForeColor = Color.FromArgb(170, 45, 45);
            }
        }

        private void SignDocuments(object sender, EventArgs e)
        {
            try
            {
                if (pdfList.Items.Count == 0)
                    throw new InvalidOperationException("Adicione pelo menos um PDF.");

                if (String.IsNullOrWhiteSpace(outputFolderTextBox.Text))
                    throw new InvalidOperationException("Informe a pasta de saida.");

                X509Certificate2 certificate = PfxCertificateProvider.Load(pfxTextBox.Text, passwordTextBox.Text);
                string hash = (string)hashComboBox.SelectedItem ?? "sha256";
                List<string> pdfs = new List<string>();
                foreach (object item in pdfList.Items)
                    pdfs.Add(item.ToString());

                int count = BatchSigner.SignMany(pdfs, outputFolderTextBox.Text, certificate, embeddedRadio.Checked, hash);
                openFolderButton.Enabled = true;
                statusLabel.Text = count + " arquivo(s) assinado(s) em: " + outputFolderTextBox.Text;
                MessageBox.Show(this, "Assinatura concluida.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                statusLabel.Text = ex.Message;
                MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenOutputFolder(object sender, EventArgs e)
        {
            if (Directory.Exists(outputFolderTextBox.Text))
                System.Diagnostics.Process.Start(outputFolderTextBox.Text);
        }

        private void ApplySafeMode()
        {
            if (safeModeCheckBox.Checked)
            {
                embeddedRadio.Checked = true;
                detachedRadio.Checked = false;
                hashComboBox.SelectedItem = "sha256";
            }
            else if (hashComboBox.SelectedIndex < 0)
            {
                hashComboBox.SelectedItem = "sha256";
            }
        }

        private static TableLayoutPanel CardTable()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Top;
            panel.AutoSize = true;
            panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panel.Padding = new Padding(20);
            panel.Margin = new Padding(8);
            panel.BackColor = Color.FromArgb(252, 254, 255);
            panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            panel.ColumnCount = 1;
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            panel.Paint += delegate(object sender, PaintEventArgs e)
            {
                Control control = (Control)sender;
                using (Pen pen = new Pen(Color.FromArgb(205, 216, 226)))
                    e.Graphics.DrawRectangle(pen, 0, 0, control.Width - 1, control.Height - 1);
            };
            return panel;
        }

        private static TableLayoutPanel TwoColumnRow(int rightWidth)
        {
            TableLayoutPanel row = new TableLayoutPanel();
            row.Dock = DockStyle.Fill;
            row.AutoSize = true;
            row.ColumnCount = 2;
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rightWidth));
            row.Margin = new Padding(0, 4, 0, 10);
            return row;
        }

        private static FlowLayoutPanel Flow()
        {
            FlowLayoutPanel flow = new FlowLayoutPanel();
            flow.Dock = DockStyle.Fill;
            flow.AutoSize = true;
            flow.WrapContents = true;
            flow.Margin = new Padding(0, 10, 0, 12);
            return flow;
        }

        private static Label SectionTitle(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Margin = new Padding(0, 0, 0, 12);
            label.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            label.ForeColor = Color.FromArgb(15, 28, 45);
            return label;
        }

        private static Label SmallBold(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Margin = new Padding(0, 8, 0, 2);
            label.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            return label;
        }

        private static Label Muted(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.MaximumSize = new Size(560, 0);
            label.Margin = new Padding(0, 0, 0, 12);
            label.ForeColor = Color.FromArgb(80, 80, 100);
            return label;
        }

        private static Button Button(string text, int width, int height)
        {
            Button button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = height;
            button.Margin = new Padding(0, 0, 8, 4);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(190, 202, 214);
            button.BackColor = Color.FromArgb(250, 252, 255);
            return button;
        }
    }
}
