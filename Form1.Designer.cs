
namespace BitmapViewer
{
    partial class Form1
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
            this.open_image = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // open_image
            // 
            this.open_image.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.open_image.Location = new System.Drawing.Point(12, 12);
            this.open_image.Name = "open_image";
            this.open_image.Size = new System.Drawing.Size(200, 50);
            this.open_image.TabIndex = 0;
            this.open_image.Text = "Open Image";
            this.open_image.UseVisualStyleBackColor = true;
            this.open_image.Click += new System.EventHandler(this.open_image_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(225, 74);
            this.Controls.Add(this.open_image);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "BitmapViewer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button open_image;
    }
}

