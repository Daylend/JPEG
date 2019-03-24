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

		int kittenWidth = 512;
		int kittenHeight = 512;


		int index(int x, int y)
		{
			return x + y * kittenWidth;
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

			int[] arrY = new int[kittenWidth * kittenHeight];
			int[] arrCr = new int[kittenWidth * kittenHeight];
			int[] arrCb = new int[kittenWidth * kittenHeight];

			// Convert RGB to YCbCr
			for (int y = 0; y < kittenHeight; y++)
			{
				for (int x = 0; x < kittenWidth; x++)
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




			// debug code
			//for(double dct : dctY)
			//  print(dct + "\n");

			// update the kittens
			//kitten.updatePixels();
			//kittenNew.updatePixels();
			//image(kittenNew, 512, 0);
			//exit();
		}

		void WriteStringToBinaryFile(string str)
		{
			Stream stream = new FileStream("D:\\test.dat", FileMode.Create);
			BinaryWriter bw = new BinaryWriter(stream);
			
			bw.Write(str);

			bw.Flush();
			bw.Close();
		}

		string HuffmanEncode(double[] arr)
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
						encoded += "1"; // storage required to store 0
						encoded += "0"; // actualy encoded value of 0 is 0

						zeroCount -= 16;
					}

					encoded += IntToPaddedBinary(zeroCount);

					if (dcCount++ % 64 == 0)
					{
						currDCValue = (int)arr[i];
						diffDC = (int)arr[i] - prevDCValue;
						prevDCValue = currDCValue;


						encoded += EncodeIntToBinary(diffDC);
					}
					else
					{
						encoded += EncodeIntToBinary((int)arr[i]);
					}

					zeroCount = 0;
				}
			}

			// if there are all zeros after the last encoded non-zero value then enter 00
			if (zeroCount > 0)
			{
				encoded += "00";
			}

			return encoded;
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

			bin = IntToBinary(BitCount) + bin;

			return bin;
		}

		string IntToBinary(int val)
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

			return bin;
		}

		string IntToPaddedBinary(int val)
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

			while (bin.Length < 4)
			{
				bin += "0" + bin;
			}

			return bin;
		}
	}
}
