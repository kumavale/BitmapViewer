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
        static byte[] pow_of_2 = new byte[] { 1, 2, 4, 8, 16, 32, 64, 128 };  // power of 2

        byte[] image_data;                           // データ格納用配列
        SolidBrush[] palette = new SolidBrush[256];  // パレットデータ

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
            uint biCompression;   // 圧縮形式 (0|1|2) ※無圧縮, RLE8, RLE4
            uint biClrUsed;       // 格納パレット数 (0|2|16|256)
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
                if (biWidth < 0 || biHeight < 0) {
                    return error_return("not supported minus size"); // TODO
                }
                // 色ビット数
                biBitCount = (uint)BitConverter.ToUInt16(image_data, cur += 6);
                if (biBitCount is not 1 and not 4 and not 8 and not 24 and not 32) {
                    return error_return("invalid bit count\nexpect `1`, `4`, `8`, `24` or `32`, but got {0}",
                            biBitCount);
                }
                // 圧縮形式
                biCompression = BitConverter.ToUInt32(image_data, cur += 2);
                if (biCompression is not 0 and not 1 and not 2) {
                    return error_return("invalid compressed format\nexpect `0`, `1` or `2`, but got {0}",
                            biCompression);
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
                if (biClrUsed is 0) {
                    switch (biBitCount) {
                        case 1: biClrUsed =   2; break;
                        case 4: biClrUsed =  16; break;
                        case 8: biClrUsed = 256; break;
                        default: return error_return("not supported `Palette Data` :(");
                    }
                }
                for(int i = 0; i < biClrUsed; ++i) {
                    byte Blue  = image_data[cur++];  // 青 0-255
                    byte Green = image_data[cur++];  // 緑 0-255
                    byte Red   = image_data[cur++];  // 赤 0-255
                    Debug.Assert(image_data[cur++] == 0);  // 予約領域をスキップ
                    palette[i] = new SolidBrush(Color.FromArgb(Red, Green, Blue));
                }
                if (biClrUsed is 2) {
                    for (int i = 1; i < 8; ++i) {
                        palette[pow_of_2[i]] = palette[1];
                    }
                }
                if (biClrUsed is 16) {
                    for (int i = 0; i < 16; ++i) {
                        palette[i << 4] = palette[i];
                    }
                }
            }

            // サイズ定義
            picture_box.Size = new Size(biWidth, biHeight);
            this.ClientSize = new Size(biWidth, biHeight);

            // 描画処理
            switch (biCompression) {
                case 0: draw_rgb (biBitCount, biWidth, biHeight, cur); break;
                case 1: draw_rle8(biBitCount, biWidth, biHeight, cur); break;
                case 2: draw_rle4(biBitCount, biWidth, biHeight, cur); break;
            }

            return true;
        }

        // キャンバスに描画する (無圧縮)
        private void draw_rgb(uint biBitCount, int biWidth, int biHeight, int cur) {
            // キャンバスサイズの定義
            Bitmap canvas = new Bitmap(biWidth, biHeight);
            Graphics g = Graphics.FromImage(canvas);
            picture_box.Image = canvas;

            // 色ビット数毎に処理が異なる
            switch (biBitCount) {
                case 1:
                    for (int h = biHeight-1; 0 <= h; --h) {
                        for (int w = 0; w < biWidth; ) {
                            byte idx8 = image_data[cur++];
                            for (int i = 7; w < biWidth && 0 <= i; --i, ++w) {
                                g.FillDot(palette[idx8 & pow_of_2[i]], w, h);
                            }
                        }
                        // 4byteの境界に揃える
                        for (int padding = biWidth / 8; padding % 4 is not 0; ++padding, ++cur);
                    }
                    break;

                case 4:
                    for (int h = biHeight-1; 0 <= h; --h) {
                        for (int w = 0; w < biWidth; ++w) {
                            byte idx2 = image_data[cur++];
                            g.FillDot(palette[idx2 & 0b1111_0000],   w, h);
                            g.FillDot(palette[idx2 & 0b0000_1111], ++w, h);
                        }
                        // 4byteの境界に揃える
                        for (int padding = biWidth / 2; padding % 4 is not 0; ++padding, ++cur);
                    }
                    break;

                case 8:
                    for (int h = biHeight-1; 0 <= h; --h) {
                        for (int w = 0; w < biWidth; ++w) {
                            byte idx = image_data[cur++];
                            g.FillDot(palette[idx], w, h);
                        }
                        // 4byteの境界に揃える
                        for (int padding = biWidth; padding % 4 is not 0; ++padding, ++cur);
                    }
                    break;

                case 24:
                    for (int h = biHeight-1; 0 <= h; --h) {
                        for (int w = 0; w < biWidth; ++w) {
                            byte Blue  = image_data[cur++];  // 青 0-255
                            byte Green = image_data[cur++];  // 緑 0-255
                            byte Red   = image_data[cur++];  // 赤 0-255
                            g.FillDot(new SolidBrush(Color.FromArgb(Red, Green, Blue)), w, h);
                        }
                        // 4byteの境界に揃える
                        for (int padding = biWidth * 3; padding % 4 is not 0; ++padding, ++cur);
                    }
                    break;

                case 32:
                    for (int h = biHeight-1; 0 <= h; --h) {
                        for (int w = 0; w < biWidth; ++w) {
                            byte Blue  = image_data[cur++];  // 青 0-255
                            byte Green = image_data[cur++];  // 緑 0-255
                            byte Red   = image_data[cur++];  // 赤 0-255
                            Debug.Assert(image_data[cur++] == 0);  // 予約領域をスキップ
                            g.FillDot(new SolidBrush(Color.FromArgb(Red, Green, Blue)), w, h);
                        }
                        // 4byteの境界に揃える必要はない
                    }
                    break;
            }

            g.Dispose();
        }

        // キャンバスに描画する (Run-Length-Encoded 8bits/pixel)
        private void draw_rle8(uint biBitCount, int biWidth, int biHeight, int cur) {
            // キャンバスサイズの定義
            Bitmap canvas = new Bitmap(biWidth, biHeight);
            Graphics g = Graphics.FromImage(canvas);
            picture_box.Image = canvas;

            Debug.Assert(biBitCount == 8);

            for (int h = biHeight-1; 0 <= h; --h) {
                int w = 0;
                while (true) {
                    byte first_byte = image_data[cur++];  // 第一バイト
                    if (first_byte is 0) {
                        byte second_byte = image_data[cur++];  // 第二バイト
                        if (second_byte is 0) { break; }          // 行の終端
                        if (second_byte is 1) { h = -1; break; }  // イメージの終端
                        if (second_byte is 2) {
                            // 位置移動
                            // 意図する場合を除き、エンコーダは位置移動情報を利用するべきではない。(らしい)
                            w += (sbyte)image_data[cur++];  // 水平移動値(-128~127)
                            h += (sbyte)image_data[cur++];  // 垂直移動値(-128~127)
                        } else {
                            // 絶対モード
                            // 第1バイト:     `0`
                            // 第2バイト:     連続しないデータの数(3～255)
                            // 第3バイト以降: カラーインデックスコード
                            for (int count = 0; count < second_byte; ++count) {
                                byte idx = image_data[cur++];
                                g.FillDot(palette[idx], w++, h);
                            }
                            // 第3バイト以降が奇数バイトの場合、詰物として`0`が入っているのでスキップ
                            if ((second_byte & 1) != 0) {
                                Debug.Assert(image_data[cur++] == 0);
                            }
                        }
                    } else {
                        // コード化モード
                        // 第1バイト: 連続する数（1～255）
                        // 第2バイト: カラーインデックスコード
                        byte idx = image_data[cur++];
                        for (int count = 0; count < first_byte; ++count) {
                            g.FillDot(palette[idx], w++, h);
                        }
                    }
                }
            }

            g.Dispose();
        }

        // キャンバスに描画する (Run-Length-Encoded 4bits/pixel)
        private void draw_rle4(uint biBitCount, int biWidth, int biHeight, int cur) {
            // キャンバスサイズの定義
            Bitmap canvas = new Bitmap(biWidth, biHeight);
            Graphics g = Graphics.FromImage(canvas);
            picture_box.Image = canvas;

            Debug.Assert(biBitCount == 4);

            for (int h = biHeight-1; 0 <= h; --h) {
                int w = 0;
                while (true) {
                    byte first_byte = image_data[cur++];  // 第一バイト
                    if (first_byte is 0) {
                        byte second_byte = image_data[cur++];  // 第二バイト
                        if (second_byte is 0) { break; }          // 行の終端
                        if (second_byte is 1) { h = -1; break; }  // イメージの終端
                        if (second_byte is 2) {
                            // 位置移動
                            // 意図する場合を除き、エンコーダは位置移動情報を利用するべきではない。(らしい)
                            w += (sbyte)image_data[cur++];  // 水平移動値(-128~127)
                            h += (sbyte)image_data[cur++];  // 垂直移動値(-128~127)
                        } else {
                            // 絶対モード
                            // 第1バイト:     `0`
                            // 第2バイト:     連続しないデータの数(3～255)
                            // 第3バイト以降: カラーインデックスコード
                            for (int count = 0; count < second_byte; ++count) {
                                byte idx2 = image_data[cur++];
                                g.FillDot(palette[idx2 & 0b1111_0000], w++, h);
                                if (++count < second_byte) {
                                    g.FillDot(palette[idx2 & 0b0000_1111], w++, h);
                                }
                            }
                            // 第3バイト以降が奇数バイトの場合、詰物として`0`が入っているのでスキップ
                            if (((second_byte+1) / 2 & 1) != 0) {
                                Debug.Assert(image_data[cur++] == 0);
                            }
                        }
                    } else {
                        // コード化モード
                        // 第1バイト: 連続する数（1～255）
                        // 第2バイト: カラーインデックスコード
                        byte idx2 = image_data[cur++];
                        for (int count = 0; count < first_byte; ++count) {
                            g.FillDot(palette[idx2 & 0b1111_0000], w++, h);
                            if (++count < first_byte) {
                                g.FillDot(palette[idx2 & 0b0000_1111], w++, h);
                            }
                        }
                    }
                }
            }

            g.Dispose();
        }
    }

    // Graphicsの拡張メソッド
    static class GraphicsExtensions
    {
        // FillRectangleで1ドット塗りつぶす
        // brush: 色
        // x: x座標
        // y: y座標
        public static void FillDot(this Graphics g, Brush brush, int x, int y)
        {
            g.FillRectangle(brush, x, y, 1, 1);
        }
    }
}

