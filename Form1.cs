using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace jpeg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            draw();
        }


        Image image = Image.FromFile("kitten.jpg");
        Bitmap kitten = new Bitmap("kitten.jpg");

        int[] qTable50 = { 16, 11, 10, 16, 24, 40, 51, 61, 12, 12, 14, 19, 26, 58, 60, 55, 14, 13, 16, 24, 40, 57, 69, 56, 14, 17, 22, 29, 51, 87, 80, 62, 18, 22, 37, 56, 68, 109, 103, 77, 24, 35, 55, 64, 81, 104, 113, 92, 49, 64, 78, 87, 103, 121, 120, 101, 72, 92, 95, 98, 112, 100, 103, 99 };
        int[] qTable90 = { 3, 2, 2, 3, 5, 8, 10, 12, 2, 2, 3, 4, 5, 12, 12, 11, 3, 3, 3, 5, 8, 11, 14, 11, 3, 3, 4, 6, 10, 17, 16, 12, 4, 4, 7, 11, 14, 22, 21, 15, 5, 7, 11, 13, 16, 12, 23, 18, 10, 13, 16, 17, 21, 24, 24, 21, 14, 18, 19, 20, 22, 20, 20, 20 };


        // DC and AC tables from https://web.stanford.edu/class/ee398a/handouts/lectures/08-JPEG.pdf

        // the negatives can be derived from this, as well as the between values
        // For example: Lower positive range = upper positive range - prev upper range. Ex lower pos range = 7 - 3 = 4, so range is 4 to 7
        // Negative can follow the same idea, except we just negate all of the values first, or we can apply absolute to the value we are checking.
        int[] dcRanges = { 0, 1, 3, 7, 15, 31, 63, 127, 255, 511, 1023, 2047 };
        int[] dcCategory = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
        int[] dcCodeLength = { 2, 3, 3, 3, 3, 3, 4, 5, 6, 7, 8, 9 };

        // converted from binary to dec
        int[] dcCodeWord = { 0, 2, 3, 4, 5, 6, 14, 30, 62, 126, 254, 510 };


        int index(int x, int y)
        {
            return x + y * kitten.Width;
        }

        int aIndex(int x, int y, int w)
        {
            return x + (y * w);
        }

        int[] subtract(int[] v, int dif)
        {
            int[] newV = new int[v.Length];
            for (int i = 0; i < v.Length; i++)
            {
                newV[i] = v[i] - dif;
            }
            return newV;
        }

        // A and B must be 8x8 im lazy
        // Sort of taken from https://www.programiz.com/java-programming/examples/multiply-matrix-function
        double[] multiply(double[] a, double[] b)
        {
            double[] c = new double[a.Length];
            for (int i = 0; i < c.Length; i++)
                c[i] = 0;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        // product[i][j] += firstMatrix[i][k] * secondMatrix[k][j];
                        c[j + i * 8] += a[k + i * 8] * b[j + k * 8];
                    }
                }
            }

            return c;
        }

        // Input is an 8x8 matrix
        double[] quantize(double[] dct, int[] qTable)
        {
            double[] newQ = new double[dct.Length];

            // I dont know why I did it in 2 dimensions but its too late just let it happen
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int i = x + y * 8;
                    newQ[i] = Math.Round(dct[i] / qTable[i]);
                }
            }


            return newQ;
        }

        double[] dctTransform(double[] matrix)
        {
            // width and height = 8
            double[] dctMatrix = dctGenerate();
            double[] dctTransposed = dctTranspose(dctMatrix);

            double[] dct = multiply(matrix, dctTransposed);
            dct = multiply(dctMatrix, dct);
            return dct;

        }

        // found on https://www.math.cuhk.edu.hk/~lmlui/dct.pdf
        double[] dctGenerate()
        {
            double[] dctMatrix = new double[64];
            for (int i = 0; i < 64; i++)
            {
                // row
                int ii = (int)(i / 8);
                // column
                int j = i % 8;

                if (ii == 0)
                {
                    dctMatrix[i] = 1 / Math.Sqrt(8);
                }
                else
                {
                    dctMatrix[i] = Math.Sqrt(2.0 / 8.0) * Math.Cos(((2.0 * j + 1.0) * ii * 3.1415926) / (2.0 * 8.0));
                }
            }
            return dctMatrix;
        }

        double[] dctTranspose(double[] m)
        {
            double[] transposed = new double[m.Length];

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    transposed[j + i * 8] = m[i + j * 8];
                }
            }

            return transposed;
        }



        void draw()
        {

            int[] arrY = new int[kitten.Width * kitten.Height];
            int[] arrCr = new int[kitten.Width * kitten.Height];
            int[] arrCb = new int[kitten.Width * kitten.Height];

            // Convert RGB to YCbCr
            for (int y = 0; y < kitten.Height; y++)
            {
                for (int x = 0; x < kitten.Width; x++)
                {
                    Color pixel = kitten.GetPixel(x, y);
                    float R = pixel.R;
                    float G = pixel.G;
                    float B = pixel.B;

                    // As per https://en.wikipedia.org/wiki/YCbCr
                    /*float Y = 0 + (0.299 * R) + (0.587 * G) + (0.114 * B);
					float CB = 128 - (0.168736 * R) - (0.331264 * G) + (0.5 * B);
					float CR = 128 + (0.5 * R) - (0.418688 * G) - (0.081312 * B);

					float newR = Y + 1.402 * (CR - 128);
					float newG = Y - 0.344136 * (CB - 128) - 0.714136 * (CR - 128);
					float newB = Y + 1.772 * (CB - 128);
					*/

                    //ISO/IEC 10918-5:2012 page 4
                    // First formula
                    /*
					float Y = Math.min(Math.max(Math.round(0.299*R + 0.587*G + 0.114*B),0),255);
					float Cb = Math.min(Math.max(Math.round(-0.299*R - 0.587*G + 0.886*B)/ 1.772 + 128, 0),255);
					float Cr = Math.min(Math.max(Math.round( 0.701*R - 0.587*G - 0.114*B)/ 1.402 + 128, 0), 255);

					float newR = Math.min(Math.max(0, Math.round(Y + 1.402 * (Cr - 128))), 255);
					float newG = Math.min(Math.max(0, Math.round(Y - (0.114 * 1.772 * (Cb-128) + 0.299 * 1.402 * (Cr-128)))), 255);
					float newB = Math.min(Math.max(0, Math.round(Y + 1.772 * (Cb - 128))), 255);
					*/

                    //ISO/IEC 10918-5:2012 page 4
                    // Second formula 4-bit approximation

                    int Y = (int)Math.Min(Math.Max(0, Math.Round(0.299 * R + 0.587 * G + 0.114 * B)), 255);
                    int Cb = (int)Math.Min(Math.Max(0, Math.Round(-0.1687 * R - 0.3313 * G + 0.5 * B + 128)), 255);
                    int Cr = (int)Math.Min(Math.Max(0, Math.Round(0.5 * R - 0.4187 * G - 0.0813 * B + 128)), 255);

                    arrY[index(x, y)] = Y;
                    arrCb[index(x, y)] = Cb;
                    arrCr[index(x, y)] = Cr;

                    // converts back to RGB just to make sure it's working
                    int newR = (int)Math.Min(Math.Max(0, Math.Round(Y + 1.402 * (Cr - 128))), 255);
                    int newG = (int)Math.Min(Math.Max(0, Math.Round(Y - 0.3441 * (Cb - 128) - 0.7141 * (Cr - 128))), 255);
                    int newB = (int)Math.Min(Math.Max(0, Math.Round(Y + 1.772 * (Cb - 128))), 255);

                    //kittenNew.pixels[index(x, y)] = color(newR, newG, newB);
                }
            }

            // Change from 0 to 255 to -128 to 127
            arrY = subtract(arrY, 128);
            arrCr = subtract(arrCr, 128);
            arrCb = subtract(arrCb, 128);

            double[] newY = new double[arrY.Length];
            double[] newCr = new double[arrCr.Length];
            double[] newCb = new double[arrCb.Length];
            int currentBlock = 0;

            // Perform DCT for each chunk of 8x8 blocks and save each channel to dctY, dctCr, dctCb
            for (int cy = 0; cy < kitten.Height / 8; cy++)
            {
                for (int cx = 0; cx < kitten.Width / 8; cx++)
                {
                    double[] tmpY = new double[64];
                    double[] tmpCr = new double[64];
                    double[] tmpCb = new double[64];


                    for (int pix = 0; pix < 8; pix++)
                    {
                        for (int piy = 0; piy < 8; piy++)
                        {
                            int idx = (cy * (kitten.Width / 8) * 64) + (cx * 8 + pix) + (piy * kitten.Width);
                            int id = pix + piy * 8;
                            int yPix = arrY[idx];
                            int crPix = arrCr[idx];
                            int cbPix = arrCb[idx];

                            tmpY[id] = (double)yPix;
                            tmpCr[id] = (double)crPix;
                            tmpCb[id] = (double)cbPix;
                        }

                    }

                    // DCT and then quantize before the final matrix insertion
                    tmpY = dctTransform(tmpY);
                    tmpCr = dctTransform(tmpCr);
                    tmpCb = dctTransform(tmpCb);

                    tmpY = quantize(tmpY, qTable50);
                    tmpCr = quantize(tmpCr, qTable50);
                    tmpCb = quantize(tmpCb, qTable50);

                    // Put values into final matrix -- old code
                    //for (int px = 0; px < 8; px++)
                    //{
                    //  for (int py = 0; py < 8; py++)
                    //  {
                    //    int idx = (cy * (kitten.width/8) * 64) + (cx * 8 + px) + py * kitten.width;
                    //    int i = px + py * 8;
                    //    newY[idx] = tmpY[i];
                    //    newCr[idx] = tmpCr[i];
                    //    newCb[idx] = tmpCb[i];



                    //  }
                    //}

                    // Store 8x8 block into final array in zigzag pattern
                    int px = 0;
                    int py = 0;
                    int i = currentBlock++ * 8 * 8;
                    bool down = true;
                    // Loop until you're in the bottom right corner
                    while (px != 7 && py != 7)
                    {
                        int index = px + py * 8;

                        newY[i] = tmpY[index];
                        newCr[i] = tmpCr[index];
                        newCb[i] = tmpCb[index];

                        // Look for new position in zigzag
                        if (px == 0 && py == 0)
                        {
                            px = 1;
                            py = 0;
                        }
                        else
                        {
                            // Traverse down and left for zig zag, then up and right
                            // This could be cleaned up maybe
                            if (down)
                            {
                                if (px != 0)
                                {
                                    px--;
                                    py++;
                                }
                                else
                                {
                                    down = false;
                                    py++;
                                }
                            }
                            else
                            {
                                if (py != 0)
                                {
                                    px++;
                                    py--;
                                }
                                else
                                {
                                    down = true;
                                    px++;
                                }
                            }
                        }
                        i++;
                    }


                }
            }



            string qtBin = "";
            for (int i = 0; i < qTable50.Length; i++)
            {
                // pad to byte
                qtBin += IntToPaddedBinary(qTable50[i], 8);
            }

			string w = IntToPaddedBinary(kitten.Width, 16);
			string h = IntToPaddedBinary(kitten.Height, 16);

			string strY = RunLengthEncode(newY);
			string huffY = HuffmanEncode(strY);

			string strCb = RunLengthEncode(newCb);
			string huffCb = HuffmanEncode(strCb);

			string strCr = RunLengthEncode(newCr);
			string huffCr = HuffmanEncode(strCr);

			//string strY = "";
			//string strCb = "";
			//string strCr = "";
			//string w = "";
			//string h = "";
			string jpg = CraftJPG(huffY, huffCb, huffCr, qtBin, w, h);
			//string jpg = CraftJPG(strY, strCb, strCr, "", w, h);

			List<Byte> b = ConvertToByte(jpg);
			WriteByteToBinaryFile(b);

			int f = 5;
			// debug code
			//for(double dct : dctY)
			//  print(dct + "\n");

			// update the kittens
			//kitten.updatePixels();
			//kittenNew.updatePixels();
			//image(kittenNew, 512, 0);
			//exit();
		}

		void WriteByteToBinaryFile(List<Byte> b)
		{
			Stream stream = new FileStream("D:\\test.dat", FileMode.Create);
			BinaryWriter bw = new BinaryWriter(stream);

			for (int i = 0; i < b.Count; i++)
				bw.Write(b[i]);

			bw.Flush();
			bw.Close();
		}

		List<Byte> ConvertToByte(string str)
		{
			List<Byte> b = new List<Byte>();
			for (int i = 0; i < str.Length; i += 8)
			{

				if (str.Length % 8 != 0)
					str += "1";
				string st = str.Substring(i, 8);
				int s = Convert.ToInt32(st, 2);

				Byte bt = Convert.ToByte(s);
				b.Add(bt);
			}

			return b;
		}

		void WriteStringToBinaryFile(string str)
        {
            Stream stream = new FileStream("D:\\test.dat", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(str);

            bw.Flush();
            bw.Close();
        }

        string RunLengthEncode(double[] arr)
        {
            string encoded = "";

            int prevDCValue = 0;
            int currDCValue = 0;
            int diffDC = 0;

            int dcCount = 0;
            int zeroCount = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                // TODO logic for finding that there are all zeros after a value.
                if (arr[i] == 0)
                {
                    zeroCount++;
                }
                else
                {
                    while (zeroCount > 15)
                    {
                        encoded += "1111"; // takes 4 bits to store 15 bits.
                        encoded += "0000"; // storage required to store 0
                        encoded += "0"; // actualy encoded value of 0 is 0

                        zeroCount -= 16;
                    }

                    // Zero count before a non-zero, pad to 4 bits
                    encoded += IntToPaddedBinary(zeroCount, 4);

                    // All dc values
                    if (dcCount++ % 64 == 0)
                    {
                        currDCValue = (int)arr[i];
                        diffDC = (int)arr[i] - prevDCValue;
                        prevDCValue = currDCValue;


                        encoded += EncodeIntToBinary(diffDC);
                    }
                    else
                    {
                        // AC values
                        encoded += EncodeIntToBinary((int)arr[i]);
                    }

                    zeroCount = 0;
                }

                if (i % 64 == 0)
                {
                    if (zeroCount > 0)
                    {
                        encoded += "00000000"; // EOB
                    }
                }
            }

            // if there are all zeros after the last encoded non-zero value then enter 00

            return encoded;
        }

        string HuffmanEncode(string RLE)
        {
            string huffmaned = "";

            // Slide the window right and compare at each step until the window is 16 in size
            int index = 0;
            while(index<RLE.Length)
            {
                // Start with length = 1
                int offset = 1;
                for (; offset <= 16; offset++)
                {
					//string v = RLE.Substring(17904, 3);
					if (index + offset > RLE.Length)
					{
						huffmaned += RLE.Substring(index, 3);
						break;
					}
                    string code = BitToCode(RLE.Substring(index, offset));
                    if (code != "")
                    {
                        huffmaned += code;
                        break;
                    }

                }
                // Advance window to the element after the end of the previous window
                index += ++offset;
            }
			if (huffmaned.Length % 8 != 0)
			{
				for (int i = 0; i < huffmaned.Length % 8; i++)
				{
					huffmaned += "1";
				}
			}
            return huffmaned;
        }

        string BitToCode(string bit)
        {
            // Move along nothing to see here
            string[] code = { "1", "10", "11", "0", "100", "10001", "101", "10010", "100001", "110001", "1000001", "110", "10011", "1010001", "1100001", "111", "100010", "1110001",
                "10100", "110010", "10000001", "10010001", "10100001", "1000", "100011", "1000010", "10110001", "11000001", "10101", "1010010", "11010001", "11110000", "100100", "110011",
                "1100010", "1110010", "10000010", "1001", "1010", "10110", "10111", "11000", "11001", "11010", "100101", "100110", "100111", "101000", "101001", "101010", "110100", "110101",
                "110110", "110111", "111000", "111001", "111010", "1000011", "1000100", "1000101", "1000110", "1000111", "1001000", "1001001", "1001010", "1010011", "1010100", "1010101",
                "1010110", "1010111", "1011000", "1011001", "1011010", "1100011", "1100100", "1100101", "1100110", "1100111", "1101000", "1101001", "1101010", "1110011", "1110100", "1110101",
                "1110110", "1110111", "1111000", "1111001", "1111010", "10000011", "10000100", "10000101", "10000110", "10000111", "10001000", "10001001", "10001010", "10010010", "10010011",
                "10010100", "10010101", "10010110", "10010111", "10011000", "10011001", "10011010", "10100010", "10100011", "10100100", "10100101", "10100110", "10100111", "10101000", "10101001",
                "10101010", "10110010", "10110011", "10110100", "10110101", "10110110", "10110111", "10111000", "10111001", "10111010", "11000010", "11000011", "11000100", "11000101", "11000110",
                "11000111", "11001000", "11001001", "11001010", "11010010", "11010011", "11010100", "11010101", "11010110", "11010111", "11011000", "11011001", "11011010", "11100001", "11100010",
                "11100011", "11100100", "11100101", "11100110", "11100111", "11101000", "11101001", "11101010", "11110001", "11110010", "11110011", "11110100", "11110101", "11110110", "11110111",
                "11111000", "11111001", "11111010" };
            string[] bits = { "00", "01", "100", "1010", "1011", "1100", "1010", "1011", "1100", "11010", "11011", "11100", "111010", "111011", "1111000", "1111001", "1111010",
                "1111011", "11111000", "11111001", "11111010", "111110110", "111110111", "111111000", "111111001", "111111010", "1111110110", "1111110111", "1111111000", "1111111001",
                "1111111010", "11111110110", "11111110111", "11111111000", "11111111001", "111111110100", "111111110101", "111111110110", "111111110111", "111111111000000", "1111111110000010",
                "1111111110000011", "1111111110000100", "1111111110000101", "1111111110000110", "1111111110000111", "1111111110001000", "1111111110001001", "1111111110001010", "1111111110001011",
                "1111111110001100", "1111111110001101", "1111111110001110", "1111111110001111", "1111111110010000", "1111111110010001", "1111111110010010", "1111111110010011", "1111111110010100",
                "1111111110010101", "1111111110010110", "1111111110010111", "1111111110011000", "1111111110011001", "1111111110011010", "1111111110011011", "1111111110011100", "1111111110011101",
                "1111111110011110", "1111111110011111", "1111111110100000", "1111111110100001", "1111111110100010", "1111111110100011", "1111111110100100", "1111111110100101", "1111111110100110",
                "1111111110100111", "1111111110101000", "1111111110101001", "1111111110101010", "1111111110101011", "1111111110101100", "1111111110101101", "1111111110101110", "1111111110101111",
                "1111111110110000", "1111111110110001", "1111111110110010", "1111111110110011", "1111111110110100", "1111111110110101", "1111111110110110", "1111111110110111", "1111111110111000",
                "1111111110111001", "1111111110111010", "1111111110111011", "1111111110111100", "1111111110111101", "1111111110111110", "1111111110111111", "1111111111000000", "1111111111000001",
                "1111111111000010", "1111111111000011", "1111111111000100", "1111111111000101", "1111111111000110", "1111111111000111", "1111111111001000", "1111111111001001", "1111111111001010",
                "1111111111001011", "1111111111001100", "1111111111001101", "1111111111001110", "1111111111001111", "1111111111010000", "1111111111010001", "1111111111010010", "1111111111010011",
                "1111111111010100", "1111111111010101", "1111111111010110", "1111111111010111", "1111111111011000", "1111111111011001", "1111111111011010", "1111111111011011", "1111111111011100",
                "1111111111011101", "1111111111011110", "1111111111011111", "1111111111100000", "1111111111100001", "1111111111100010", "1111111111100011", "1111111111100100", "1111111111100101",
                "1111111111100110", "1111111111100111", "1111111111101000", "1111111111101001", "1111111111101010", "1111111111101011", "1111111111101100", "1111111111101101", "1111111111101110",
                "1111111111101111", "1111111111110000", "1111111111110001", "1111111111110010", "1111111111110011", "1111111111110100", "1111111111110101", "1111111111110110", "1111111111110111",
                "1111111111111000", "1111111111111001", "1111111111111010", "1111111111111011", "1111111111111100", "1111111111111101", "1111111111111110" };
            string[] category = { "" };

            for(int i = 0; i < bits.Length; i++)
            {
                if(String.Equals(bit, bits[i]))
                {
                    return code[i];
                }
            }

            return "";
        }

        // Assumed that val <= 15
        string EncodeIntToBinary(int val)
        {
            string bin = "";
            int BitCount = 0;

            while (val > 0)
            {
                if (val % 2 == 1)
                    bin += "1";
                else
                    bin += "0";

                val = val / 2;
                BitCount++;
            }

            bin = IntToPaddedBinary(BitCount, 4) + bin;

            return bin;
        }

        string IntToPaddedBinary(int val, int padding)
        {

            string bin = "";

            while (val > 0)
            {
                if (val % 2 == 1)
                    bin += "1";
                else
                    bin += "0";

                val = val / 2;
            }

            while (bin.Length < padding)
            {
                bin = "0" + bin;
            }

            return bin;
        }

        string CraftJPG(string Y, string Cb, string Cr, string QT, string width, string height)
        {
            // SOI 0xFFD8 in bin
            string jpg = "1111111111011000";

            // APP0 section
            jpg += "1111111111100000";                          // APP0 0xFFE0 in bin
            jpg += "0000000000010000";                          // Length of APP0 = 16
            jpg += "0100101001000110010010010100011000000000";// File identifier mark for jfif

			jpg += "00000001";                                  // Major revision number
            jpg += "00000010";                                  // Minor revision number
            jpg += "00000000";                                  // Units
            jpg += "0000000000000001";                           // x ratio
            jpg += "0000000000000001";                           // y ratio
            jpg += "00000000";                                  // thumbnail x
            jpg += "00000000";                                  // thumbnail y
            jpg += "0000000000000000";                          // thumbnail payload blank.

            // Quantization table
            jpg += "1111111111011011";                          // QT marker
            jpg += "0000000001000011";                          // length of our QT
            jpg += "00000000";                                 // QT info
            jpg += QT;                                          // QT values

            // Start of frame
            jpg += "1111111111000000";  // SOF0 start of frame
            jpg += "0000000000010001";  // Length
            jpg += "00001000";          // data precision
            jpg += width;               // width of image
            jpg += height;              // height of image
            jpg += "00000011";          // num components
            jpg += "000000010100010000000000"; // Component y
            jpg += "000000100100010000000000"; // Component Cb
            jpg += "000000110100010000000000"; // Component Cr

			// DHT predefined
			jpg += "111111111100010000000000101101010001000000000000000000100000000100000011000000110000001000000100000000110000010100000101000001000000010000000000000000000000000101111101000000010000001000000011000000000000010000010001000001010001001000100001001100010100000100000110000100110101000101100001000001110010001001110001000101000011001010000001100100011010000100001000001000110100001010110001110000010001010101010010110100011111000000100100001100110110001001110010100000100000100100001010000101100001011100011000000110010001101000100101001001100010011100101000001010010010101000110100001101010011011000110111001110000011100100111010010000110100010001000101010001100100011101001000010010010100101001010011010101000101010101010110010101110101100001011001010110100110001101100100011001010110011001100111011010000110100101101010011100110111010001110101011101100111011101111000011110010111101010000011100001001000010110000110100001111000100010001001100010101001001010010011100101001001010110010110100101111001100010011001100110101010001010100011101001001010010110100110101001111010100010101001101010101011001010110011101101001011010110110110101101111011100010111001101110101100001011000011110001001100010111000110110001111100100011001001110010101101001011010011110101001101010111010110110101111101100011011001110110101110000111100010111000111110010011100101111001101110011111101000111010011110101011110001111100101111001111110100111101011111011011110111111110001111100111111010"; 
			
			// start of scan predefined, and Y
			jpg += "11111111110110100000000000001100000000110000000100000000000000010000001000000011";
			jpg += "0000001000000000000000010000001000000011"; // Cr
			jpg += "0000001100000000000000010000001000000011"; // Cb

			jpg += Y;
			jpg += Cb;
			jpg += Cr;

			// end of image
			jpg += "1111111111011001";

			return jpg;
        }
    }
}
