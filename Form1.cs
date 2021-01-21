using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BitmapViewer
{
    public partial class Form1 : Form
    {
        private List<ImageWindow> ImageWindows = new List<ImageWindow>();

        public Form1()
        {
            InitializeComponent();
        }

        // 画像を開く
        private void open_image_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bimap files|*.bmp";
            ofd.Title = "Select a bitmap image";

            // ファイル選択ダイアログ表示
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                // 画像表示画面作成
                ImageWindow window = new ImageWindow();
                ImageWindows.Add(window);
                // 画像表示画面に画像設定
                if (window.read_image(ofd.FileName))
                {
                    // 画面表示
                    window.show();
                }
            }
            else
            {
                MessageBox.Show("failed open file");
            }
        }

        // 全ての画像を閉じる
        private void close_all_images_Click(object sender, EventArgs e)
        {
            foreach (ImageWindow iw in ImageWindows)
            {
                iw.Close();
            }
            ImageWindows.Clear();
        }
    }
}

