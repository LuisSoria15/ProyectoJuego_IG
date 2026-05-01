namespace ProyectoJuego
{
    partial class LeaderBoard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTituloLeader = new System.Windows.Forms.Label();
            this.pbCerrar = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbCerrar)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTituloLeader
            // 
            this.lblTituloLeader.AutoSize = true;
            this.lblTituloLeader.Font = new System.Drawing.Font("Consolas", 28.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTituloLeader.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTituloLeader.Location = new System.Drawing.Point(477, 86);
            this.lblTituloLeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblTituloLeader.Name = "lblTituloLeader";
            this.lblTituloLeader.Size = new System.Drawing.Size(429, 78);
            this.lblTituloLeader.TabIndex = 0;
            this.lblTituloLeader.Text = "Leaderboard";
            // 
            // pbCerrar
            // 
            this.pbCerrar.Image = global::ProyectoJuego.Properties.Resources.cerrar_normal1;
            this.pbCerrar.Location = new System.Drawing.Point(1276, 18);
            this.pbCerrar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pbCerrar.Name = "pbCerrar";
            this.pbCerrar.Size = new System.Drawing.Size(131, 112);
            this.pbCerrar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbCerrar.TabIndex = 3;
            this.pbCerrar.TabStop = false;
            this.pbCerrar.Click += new System.EventHandler(this.pbCerrar_Click);
            this.pbCerrar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbCerrar_MouseDown);
            this.pbCerrar.MouseEnter += new System.EventHandler(this.pbCerrar_MouseEnter);
            this.pbCerrar.MouseLeave += new System.EventHandler(this.pbCerrar_MouseLeave);
            this.pbCerrar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbCerrar_MouseUp);
            // 
            // LeaderBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(1422, 908);
            this.Controls.Add(this.pbCerrar);
            this.Controls.Add(this.lblTituloLeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "LeaderBoard";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LeaderBoard";
            this.Load += new System.EventHandler(this.LeaderBoard_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.LeaderBoard_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LeaderBoard_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.LeaderBoard_MouseMove);
            ((System.ComponentModel.ISupportInitialize)(this.pbCerrar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTituloLeader;
        private System.Windows.Forms.PictureBox pbCerrar;
    }
}