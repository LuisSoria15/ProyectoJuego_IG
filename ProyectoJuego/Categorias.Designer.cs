namespace ProyectoJuego
{
    partial class Categorias
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
            this.label1 = new System.Windows.Forms.Label();
            this.btnRegresar = new System.Windows.Forms.Button();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.btnCategoria1 = new System.Windows.Forms.Button();
            this.btnCategoria2 = new System.Windows.Forms.Button();
            this.btnCategoria3 = new System.Windows.Forms.Button();
            this.btnCategoria4 = new System.Windows.Forms.Button();
            this.btnCategoria5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Ravie", 28.2F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(293, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(373, 62);
            this.label1.TabIndex = 0;
            this.label1.Text = "Categorias";
            // 
            // btnRegresar
            // 
            this.btnRegresar.Location = new System.Drawing.Point(65, 45);
            this.btnRegresar.Name = "btnRegresar";
            this.btnRegresar.Size = new System.Drawing.Size(79, 26);
            this.btnRegresar.TabIndex = 1;
            this.btnRegresar.Text = "Regresar";
            this.btnRegresar.UseVisualStyleBackColor = true;
            this.btnRegresar.Click += new System.EventHandler(this.btnRegresar_Click);
            // 
            // btnCerrar
            // 
            this.btnCerrar.Location = new System.Drawing.Point(814, 62);
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(75, 23);
            this.btnCerrar.TabIndex = 2;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            this.btnCerrar.Click += new System.EventHandler(this.btnCerrar_Click);
            // 
            // btnCategoria1
            // 
            this.btnCategoria1.Location = new System.Drawing.Point(379, 230);
            this.btnCategoria1.Name = "btnCategoria1";
            this.btnCategoria1.Size = new System.Drawing.Size(91, 35);
            this.btnCategoria1.TabIndex = 3;
            this.btnCategoria1.Text = "Categoria1";
            this.btnCategoria1.UseVisualStyleBackColor = true;
            // 
            // btnCategoria2
            // 
            this.btnCategoria2.Location = new System.Drawing.Point(569, 230);
            this.btnCategoria2.Name = "btnCategoria2";
            this.btnCategoria2.Size = new System.Drawing.Size(88, 35);
            this.btnCategoria2.TabIndex = 4;
            this.btnCategoria2.Text = "Categoria2";
            this.btnCategoria2.UseVisualStyleBackColor = true;
            // 
            // btnCategoria3
            // 
            this.btnCategoria3.Location = new System.Drawing.Point(214, 312);
            this.btnCategoria3.Name = "btnCategoria3";
            this.btnCategoria3.Size = new System.Drawing.Size(90, 36);
            this.btnCategoria3.TabIndex = 5;
            this.btnCategoria3.Text = "Categoria3";
            this.btnCategoria3.UseVisualStyleBackColor = true;
            // 
            // btnCategoria4
            // 
            this.btnCategoria4.Location = new System.Drawing.Point(476, 312);
            this.btnCategoria4.Name = "btnCategoria4";
            this.btnCategoria4.Size = new System.Drawing.Size(82, 36);
            this.btnCategoria4.TabIndex = 6;
            this.btnCategoria4.Text = "Categoria4";
            this.btnCategoria4.UseVisualStyleBackColor = true;
            // 
            // btnCategoria5
            // 
            this.btnCategoria5.Location = new System.Drawing.Point(717, 312);
            this.btnCategoria5.Name = "btnCategoria5";
            this.btnCategoria5.Size = new System.Drawing.Size(86, 36);
            this.btnCategoria5.TabIndex = 7;
            this.btnCategoria5.Text = "Categoria5";
            this.btnCategoria5.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(379, 428);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 8;
            this.button6.Text = "button6";
            this.button6.UseVisualStyleBackColor = true;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(569, 428);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 9;
            this.button7.Text = "button7";
            this.button7.UseVisualStyleBackColor = true;
            // 
            // Categorias
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1002, 565);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.btnCategoria5);
            this.Controls.Add(this.btnCategoria4);
            this.Controls.Add(this.btnCategoria3);
            this.Controls.Add(this.btnCategoria2);
            this.Controls.Add(this.btnCategoria1);
            this.Controls.Add(this.btnCerrar);
            this.Controls.Add(this.btnRegresar);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Categorias";
            this.Text = "Categorias";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Categorias_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Categorias_MouseMove);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRegresar;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnCategoria1;
        private System.Windows.Forms.Button btnCategoria2;
        private System.Windows.Forms.Button btnCategoria3;
        private System.Windows.Forms.Button btnCategoria4;
        private System.Windows.Forms.Button btnCategoria5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
    }
}