using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace BitmapViewer
{
    public partial class ImageWindow : Form
    {
        byte[] image_data;                 // データ格納用配列
        Color[] palette = new Color[256];  // パレットデータ

        public ImageWindow()
        {
            InitializeComponent();
        }

        // 画像ファイルを読み込む
        public bool read_image(String filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            int file_size = (int)fs.Length;    // ファイルサイズ
            image_data = new byte[file_size];  // データ格納用配列

            int read_size;           // Readメソッドで読み込んだバイト数
            int remain = file_size;  // 読み込むべき残りのバイト数
            int pos = 0;             // データ格納用配列内の追加位置

            while (0 < remain)
            {
                // 1024Byteずつ読み込む
                read_size = fs.Read(image_data, pos, Math.Min(1024, remain));
                pos += read_size;
                remain -= read_size;
            }

            fs.Dispose();  // ファイルを閉じる

            // ビットマップ画像の生成
            bool result = create_bitmap();

            return result;
        }

        // ビットマップ画像ウィンドウを表示する
         public void show()
         {
             this.ShowDialog();
         }

        // メッセージボックスにエラー出力をしてfalseを返す
        private bool error_return(string fmt, params object[]? args)
        {
            MessageBox.Show(String.Format(fmt, args), "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        // ビットマップ画像の生成
        private bool create_bitmap()
        {
            int cur = 0;  // データ配列の現在の位置

            // ※bfType以外のデータはリトルエンディアンで記録されている
            // ファイルヘッダ
            uint bfOffBits;  // ファイル先頭から画像データまでのオフセット (byte)
            // 情報ヘッダ
            int  biWidth;         // 画像の幅 (px)
            int  biHeight;        // 画像の高さ (px)
            uint biBitCount;      // 色ビット数 (bit)  ※このソフトウェアでは8bitのみ対応する
            uint biCompression;   // 圧縮形式  ※このソフトウェアではa `0`(無圧縮)のみ対応する
            uint biClrUsed;       // 格納パレット数 (0|256)
            uint biCirImportant;  // 重要色数

            // ファイルヘッダ解析 (14byte)
            {
                // ファイルサイズが14byte以下ならヘッダすら定義されていない
                if (image_data.Length <= 14)
                {
                    return error_return("Not a Bitmap");
                }
                // ファイルタイプのチェック 'BM' から始まる
                if (image_data[cur] is not (byte)'B' || image_data[++cur] is not (byte)'M')
                {
                    return error_return("Not a Bitmap: mismatched file types");
                }
                // ファイル先頭から画像データまでのオフセット
                bfOffBits = BitConverter.ToUInt32(image_data, cur = 10);
                // ファイルヘッダの最後までスキップ
                cur = 14;
            }

            // 情報ヘッダ解析 (40byte)
            {
                // 情報ヘッダサイズ
                uint biSize = BitConverter.ToUInt32(image_data, cur);
                if (biSize is not 40) {
                    return error_return("invalid Bitmap Information Header size\nexpect `40`, but got {0}", biSize);
                }
                // 画像の幅
                biWidth = BitConverter.ToInt32(image_data, cur += 4);
                // 画像の高さ
                biHeight = BitConverter.ToInt32(image_data, cur += 4);
                // 色ビット数
                biBitCount = (uint)BitConverter.ToUInt16(image_data, cur += 6);
                if (biBitCount is not 0 and not 8) {
                    return error_return("invalid bit count\nexpect `0` or `8`, but got {0}", biBitCount);
                }
                // 圧縮形式
                biCompression = BitConverter.ToUInt32(image_data, cur += 2);
                if (biCompression is not 0) {
                    return error_return("invalid compressed format\nexpect `0`, but got {0}", biCompression);
                }
                // 格納パレット数
                biClrUsed = BitConverter.ToUInt32(image_data, cur += 16);
                // 重要色数
                biCirImportant = BitConverter.ToUInt32(image_data, cur += 4);
                // 情報ヘッダを最後までスキップ
                cur += 4;
                Debug.Assert(cur == 14 + 40);
                Debug.Assert(biWidth * biHeight <= Int32.MaxValue);
            }

            // パレットデータの読み込み
            if (cur != bfOffBits) {
                if (biClrUsed is not 0 and not 256) {
                    return error_return("not supported `Palette Data` :(");
                }
                for(int i = 0; i < 256; ++i) {
                    byte Blue  = image_data[cur++];  // 青 0-255
                    byte Green = image_data[cur++];  // 緑 0-255
                    byte Red   = image_data[cur++];  // 赤 0-255
                    Debug.Assert(image_data[cur++] == 0);  // 予約領域をスキップ
                    palette[i] = Color.FromArgb(Red, Green, Blue);
                }
            }

            // キャンバスサイズの定義
            picture_box.Size = new Size(biWidth, biHeight);
            Bitmap canvas = new Bitmap(biWidth, biHeight);
            Graphics g = Graphics.FromImage(canvas);

            // 描画処理
            for (int h = biHeight-1; 0 <= h; --h)
            {
                for (int w = 0; w < biWidth; ++w)
                {
                    byte idx = image_data[cur++];
                    g.FillRectangle(new SolidBrush(palette[idx]), w, h, 1, 1);
                }
                // 4byteの境界に揃える
                for (int padding = biWidth % 4; 0 < padding; --padding, ++cur);
            }

            g.Dispose();

            picture_box.Image = canvas;
            picture_box.Location = new Point(0, 0);
            this.ClientSize = new Size(biWidth, biHeight);

            return true;
        }
    }
}

