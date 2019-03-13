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
  
  double[] dctY = new double[arrY.length];
  double[] dctCr = new double[arrCr.length];
  double[] dctCb = new double[arrCb.length];
  
  for (int cy = 0; cy < kitten.height / 8; cy++)
  { 
   for (int cx = 0;cx < kitten.width / 8; cx++)
   {
     double[] tmpY = new double[64];
     double[] tmpCr = new double[64];
     double[] tmpCb = new double[64];
     
     
    for (int px = 0; px < 8; px++)
    {
      for (int py = 0; py < 8; py++)
      {
        int idx = (cy * (kitten.width/8) * 64) + (cx * 8 + px) + (py * kitten.width);
        int i = px + py * 8;
        int yPix = arrY[idx];
        int crPix = arrCr[idx];
        int cbPix = arrCb[idx];
         
        tmpY[i] = (double)yPix;
        tmpCr[i] = (double)crPix;
        tmpCb[i] = (double)cbPix;
      }
     
    }
    
    tmpY = dctTransform(tmpY);
    tmpCr = dctTransform(tmpCr);
    tmpCb = dctTransform(tmpCb);
    
    for (int px = 0; px < 8; px++)
    {
      for (int py = 0; py < 8; py++)
      {
        int idx = (cy * (kitten.width/8) * 64) + (cx * 8 + px) + py * kitten.width;
        int i = px + py * 8;
        dctY[idx] = tmpY[i];
        dctCr[idx] = tmpCr[i];
        dctCb[idx] = tmpCb[i];
      }
    }
    

   }
  }
  
  // Generate DCT Matrix
  
  //for(double num : dct)
  //  print(num + "\n");
  for(double dct : dctY)
    print(dct + "\n");
  
  kitten.updatePixels();
  kittenNew.updatePixels();
  image(kittenNew, 512, 0);
  exit();
}
