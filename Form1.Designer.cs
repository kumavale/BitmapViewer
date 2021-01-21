
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
            this.close_all_images = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // open_image
            // 
            this.open_image.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.open_image.Location = new System.Drawing.Point(12, 12);
            this.open_image.Name = "open_image";
            this.open_image.Size = new System.Drawing.Size(120, 68);
            this.open_image.TabIndex = 0;
            this.open_image.Text = "Open Image";
            this.open_image.UseVisualStyleBackColor = true;
            this.open_image.Click += new System.EventHandler(this.open_image_Click);
            // 
            // close_all_images
            // 
            this.close_all_images.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.close_all_images.Location = new System.Drawing.Point(148, 12);
            this.close_all_images.Name = "close_all_images";
            this.close_all_images.Size = new System.Drawing.Size(120, 68);
            this.close_all_images.TabIndex = 1;
            this.close_all_images.Text = "Close All Images";
            this.close_all_images.UseVisualStyleBackColor = true;
            this.close_all_images.Click += new System.EventHandler(this.close_all_images_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 92);
            this.Controls.Add(this.close_all_images);
            this.Controls.Add(this.open_image);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "BitmapViewer";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button open_image;
        private System.Windows.Forms.Button close_all_images;
    }
}

