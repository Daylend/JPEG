PImage kitten;
PImage kittenNew;

void setup() {
  size(1024, 1024);
  kitten = loadImage("kitten.jpg");
  kittenNew = loadImage("kitten.jpg");
  //kitten.filter(GRAY);
  image(kitten, 0, 0);
  frameRate(1);
}

int index(int x, int y) {
  return x + y * kitten.width;
}

int aIndex(int x, int y, int w)
{
  return x + (y * w);
}

int[] subtract(int[] v, int dif) {
  int[] newV = new int[v.length];
  for(int i = 0; i < v.length; i++)
  {
    newV[i] = v[i] - dif;
  }
  return newV;
}

// A and B must be 8x8 im lazy
// Sort of taken from https://www.programiz.com/java-programming/examples/multiply-matrix-function
double[] multiply(double[] a, double[] b)
{
  double[] c = new double[a.length];
  for (int i = 0; i < c.length; i++)
    c[i] = 0;
  
  for(int i = 0; i < 8; i++)
  {
    for(int j = 0; j < 8; j++)
    {
      for(int k = 0; k < 8; k++)
      {
        // product[i][j] += firstMatrix[i][k] * secondMatrix[k][j];
        c[j + i * 8] += a[k + i * 8] * b[j + k * 8];
      }
    }
  }
  
  return c;
}

float dctTransform(int[] matrix)
{
  // width and height = 8
  
  
  return 0;
  
}

// found on https://www.math.cuhk.edu.hk/~lmlui/dct.pdf
double[] dctGenerate()
{
  double[] dctMatrix = new double[64];
  for(int i = 0; i < 64; i++)
  {
    // row
    int ii = (int)(i/8);
    // column
    int j = i % 8;
    
    if(ii == 0)
    {
      dctMatrix[i] = 1 / Math.sqrt(8);
    }
    else
    {
      dctMatrix[i] = Math.sqrt(2.0/8.0) * Math.cos( ((2.0*j+1.0)*ii*3.1415926) / (2.0*8.0) );
    }
  }
  return dctMatrix;
}

double[] dctTranspose(double[] m)
{
  double[] transposed = new double[m.length];
  
  for(int i = 0; i < 8; i++)
  {
    for(int j = 0; j < 8; j++)
    {
      transposed[j + i * 8] = m[i + j * 8];
    }
  }
  
  return transposed;
}

void draw() {
  kitten.loadPixels();
  kittenNew.loadPixels();
  
  int[] arrY = new int[kitten.width*kitten.height];
  int[] arrCr = new int[kitten.width*kitten.height];
  int[] arrCb = new int[kitten.width*kitten.height];
  
  // Convert RGB to YCbCr
  for(int y = 0; y < kitten.height; y++) {
    for(int x = 0; x < kitten.width; x++) {
      color pixel = kitten.pixels[index(x,y)];
      float R = red(pixel);
      float G = green(pixel);
      float B = blue(pixel);
      
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
      
      int Y = Math.min(Math.max(0, Math.round(0.299 * R + 0.587 * G + 0.114 * B)), 255);
      int Cb = Math.min(Math.max(0, Math.round(-0.1687*R - 0.3313*G + 0.5*B + 128)), 255);
      int Cr = Math.min(Math.max(0, Math.round(0.5*R - 0.4187*G - 0.0813*B + 128)), 255);
      
      arrY[index(x,y)] = Y;
      arrCb[index(x,y)] = Cb;
      arrCr[index(x,y)] = Cr;
      
      // converts back to RGB just to make sure it's working
      int newR = Math.min(Math.max(0, Math.round(Y + 1.402 * (Cr - 128))), 255);
      int newG = Math.min(Math.max(0, Math.round(Y - 0.3441 * (Cb - 128) - 0.7141 * (Cr-128))), 255);
      int newB = Math.min(Math.max(0, Math.round(Y + 1.772 * (Cb - 128))), 255);
      
      kittenNew.pixels[index(x,y)] = color(newR, newG, newB);
    }
  }
  
  // Change from 0 to 255 to -128 to 127
  arrY = subtract(arrY, 128);
  arrCr = subtract(arrCr, 128);
  arrCb = subtract(arrCb, 128);
  
  // Generate DCT Matrix
  double[] dctMatrix = dctGenerate();
  double[] dctTransposed = dctTranspose(dctMatrix);
  double[] testM = {26, -5, -5, -5, -5, -5, -5, 8, 64, 52, 8, 26, 26, 26, 8, -18, 126, 70, 26, 26, 52, 26, -5, -5, 111, 52, 8, 52, 52, 38, -5, -5, 52, 26, 8, 39, 38, 21, 8, 8, 0, 8, -5, 8, 26, 52, 70, 26, -5, -23, -18, 21, 8, 8, 52, 38, -18, 8, -5, -5, -5, 8, 26, 8};
  double[] testM1 = new double[64];
  double[] testM2 = new double[64];
  boolean asdf = true;
  for(int i = 0; i < 64; i++)
  {
    testM1[i] = 1;
    if(asdf)
      testM2[i] = 1;
    else
      testM2[i] = 0;
      
     asdf = !asdf;
  }
  double[] testM3 = multiply(testM1, testM2);
  double[] dct = multiply(testM, dctTransposed);
  dct = multiply(dctMatrix, dct);
  for(double num : dct)
    print(num + "\n");
  
  kitten.updatePixels();
  kittenNew.updatePixels();
  image(kittenNew, 512, 0);
}
