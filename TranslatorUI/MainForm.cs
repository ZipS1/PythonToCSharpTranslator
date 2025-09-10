using System;
using System.IO;
using System.Windows.Forms;
using TranslatorCore;

namespace TranslatorUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            btnOpenFile.Click += BtnOpenFile_Click;
            btnTranslate.Click += BtnTranslate_Click;
            btnSaveResult.Click += BtnSaveResult_Click;
        }

        private void BtnOpenFile_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog();
            dlg.Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtSource.Text = File.ReadAllText(dlg.FileName);
                lblWarning.Text = string.Empty;
                txtResult.Text = string.Empty;
            }
        }

        private void BtnTranslate_Click(object? sender, EventArgs e)
        {
            lblWarning.Text = string.Empty;
            txtResult.Text = string.Empty;
            try
            {
                var source = txtSource.Text;
                var tokenizer = new Tokenizer(source);
                var tokens = tokenizer.Tokenize();
                var parser = new Parser(tokens);
                var program = parser.Parse();
                var translator = new Translator();
                var result = translator.Translate(program);
                txtResult.Text = result;
                if (translator.Warnings.Count > 0)
                {
                    lblWarning.Text = string.Join(Environment.NewLine, translator.Warnings);
                }
            }
            catch (TranslationException ex)
            {
                lblWarning.Text = ex.Message;
            }
            catch (Exception ex)
            {
                lblWarning.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void BtnSaveResult_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtResult.Text)) return;
            using var dlg = new SaveFileDialog();
            dlg.Filter = "C# Files (*.cs)|*.cs|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(dlg.FileName, txtResult.Text);
            }
        }
    }
}
