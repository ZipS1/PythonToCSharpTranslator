namespace TranslatorUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnOpenFile = new Button();
            txtSource = new TextBox();
            txtResult = new TextBox();
            btnTranslate = new Button();
            btnSaveResult = new Button();
            lblWarning = new Label();
            SuspendLayout();
            // 
            // btnOpenFile
            // 
            btnOpenFile.Location = new Point(16, 13);
            btnOpenFile.Name = "btnOpenFile";
            btnOpenFile.Size = new Size(212, 29);
            btnOpenFile.TabIndex = 0;
            btnOpenFile.Text = "Открыть Python-файл…";
            btnOpenFile.UseVisualStyleBackColor = true;
            // 
            // txtSource
            // 
            txtSource.Location = new Point(16, 48);
            txtSource.Multiline = true;
            txtSource.Name = "txtSource";
            txtSource.ScrollBars = ScrollBars.Both;
            txtSource.Size = new Size(405, 365);
            txtSource.TabIndex = 1;
            // 
            // txtResult
            // 
            txtResult.Location = new Point(427, 48);
            txtResult.Multiline = true;
            txtResult.Name = "txtResult";
            txtResult.ScrollBars = ScrollBars.Both;
            txtResult.Size = new Size(405, 365);
            txtResult.TabIndex = 2;
            // 
            // btnTranslate
            // 
            btnTranslate.Location = new Point(101, 419);
            btnTranslate.Name = "btnTranslate";
            btnTranslate.Size = new Size(212, 29);
            btnTranslate.TabIndex = 3;
            btnTranslate.Text = "Трансляция";
            btnTranslate.UseVisualStyleBackColor = true;
            // 
            // btnSaveResult
            // 
            btnSaveResult.Location = new Point(521, 419);
            btnSaveResult.Name = "btnSaveResult";
            btnSaveResult.Size = new Size(212, 29);
            btnSaveResult.TabIndex = 4;
            btnSaveResult.Text = "Сохранить результат";
            btnSaveResult.UseVisualStyleBackColor = true;
            // 
            // lblWarning
            // 
            lblWarning.Font = new Font("Segoe UI", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 204);
            lblWarning.ForeColor = Color.Red;
            lblWarning.Location = new Point(101, 451);
            lblWarning.Name = "lblWarning";
            lblWarning.Size = new Size(632, 55);
            lblWarning.TabIndex = 5;
            lblWarning.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(840, 515);
            Controls.Add(lblWarning);
            Controls.Add(btnSaveResult);
            Controls.Add(btnTranslate);
            Controls.Add(txtResult);
            Controls.Add(txtSource);
            Controls.Add(btnOpenFile);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnOpenFile;
        private TextBox txtSource;
        private TextBox txtResult;
        private Button btnTranslate;
        private Button btnSaveResult;
        private Label lblWarning;
    }
}
