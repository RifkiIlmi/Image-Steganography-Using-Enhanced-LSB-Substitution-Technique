using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public string textfile, image;
        Stopwatch timer = new Stopwatch();
        Int32 capacity;
        int i,kl=1;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) 
        {
            string impath = image;
            string back = "\\";
            impath.Replace(back,back+back);
            label3.Text = impath;
            
            try
            {
                double cap = 0;
                // Create the MATLAB instance 
                MLApp.MLApp matlab = new MLApp.MLApp();

                // Change to the directory where the function is located 
                matlab.Execute(@"cd c:\temp\example");
                label3.Text =label3.Text + "matlab call";
                // Define the output 
                object result = null;
                // Call the MATLAB function myfunc
                //Feval(String functionname, long numout, arg1, arg2, ...) As Object
                matlab.Feval("imreed", 3, out result, impath);
                label3.Text = label3.Text + "imreed call";
                // Display result 
                object[] res = result as object[];
                byte[,] red = res[0] as byte[,];
                byte[,] blue = res[1] as byte[,];
                byte[,] green = res[2] as byte[,];
                //Assign null to result and res, otherwise exception is given.
                result = null;
                res = null;
                matlab.Feval("calcByteSize", 1, out result, image);
                label3.Text = label3.Text + "callbytesize call call";
                //object[] cals 
                res = result as object[];
                capacity = Convert.ToInt32(res[0]);
                //Print capacity of size in image with our modified algorithm
                cap = capacity / 8;
                label19.Text = cap.ToString() + " bytes";
                //Capacity of size in image with LSB algorithm
                result = null;
                res = null;
                matlab.Feval("LSBSize", 1, out result, image);
                res = result as object[];
                cap = Convert.ToDouble(res[0])/8;
                label21.Text = cap.ToString()+" bytes";
                //Hide Data
                hideInImage(red,green,blue);
                //calculate PSNR
                string modded_image = "C:\\temp\\example\\Data\\after2222.png";
                result = null;
                res = null;
                matlab.Feval("psnr", 1, out result, impath,modded_image);
                res = result as object[];
                label23.Text = res[0].ToString();
                

                //New objects need to be initialized to 1 else gives error/wrong output.
                //object p = null;
                /*object imageArrayPassed = null;
                 
                matlab.PutWorkspaceData("red", "base", red);
                matlab.PutWorkspaceData("green", "base", green);
                matlab.PutWorkspaceData("blue", "base", blue);
                matlab.Execute("im = cat(3,red,green,blue)");
                label4.Text =  matlab.Execute("size(im)").ToString();*/
                //matlab.Feval("imagewrite",1, out imageArrayPassed, image, "F:\\gaurav\\major project\\stego matlab code\\ql.png");
                
                
                //Console.WriteLine(matlab.Execute("imwrite(im,'F:\\gaurav\\major project\\stego matlab code\\ql.png')"));
            }
            catch (Exception ex)
            {
                label4.Text = ex.Message;
            }
        } 

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result =  openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                textfile = openFileDialog1.FileName;
                try
                {

                    label1.Text = textfile;
                }
                catch (Exception ex)
                {
                    label4.Text = ex.Message;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog2.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                image = openFileDialog2.FileName;
                try
                {

                    label2.Text = image;
                    var form = new Form3(label2.Text);
                    form.Show();
                }
                catch (Exception ex)
                {
                    label4.Text = ex.Message;
                }
            }
        }

        private byte[] StreamFile(string filename)
        {
            // Create a byte array of file stream length
            byte[] fileData = System.IO.File.ReadAllBytes(filename);
            return fileData; //return the byte data
        }

        private int requiredBit(byte pixelVal)
        {
            int bits = 0;
            if (pixelVal > 63)
                bits += 4;
            else if (pixelVal <= 63 && pixelVal > 15)
                bits += 3;
            else if (pixelVal <= 15 && pixelVal > 3)
                bits += 2;
            else
                bits += 1;
            return bits;
        }

        private byte get_byte(int offset, byte cur_byte, int num_of_bits)
        {
            cur_byte <<= offset;
            byte re_Byte = cur_byte;
            switch (num_of_bits)
            {
                case 4:
                    re_Byte &= (byte)240;
                    break;
                case 3:
                    re_Byte &= (byte)224;
                    break;
                case 2:
                    re_Byte &= (byte)192;
                    break;
                case 1:
                    re_Byte &= (byte)128;
                    break;
                default:
                    re_Byte &= (byte)0;
                    break;
            }
            byte a = re_Byte;
            byte b = re_Byte;
            a <<= num_of_bits;
            b >>= (8 - num_of_bits);
            re_Byte = (byte) (a | b);
            return re_Byte;
        }

        private byte maskByte(byte pix, int numberBits)
        {
            byte maskedByte = pix;
            switch (numberBits)
            {
                case 4:
                    maskedByte = (byte)(maskedByte & 240);
                    break;
                case 3:
                    maskedByte = (byte)(maskedByte & 248);
                    break;
                case 2:
                    maskedByte = (byte)(maskedByte & 252);
                    break;
                case 1:
                    maskedByte = (byte)(maskedByte & 254);
                    break;
                default:
                    maskedByte = (byte)(maskedByte & 255);
                    break;
            }
            return maskedByte;
        }

        public void f(string sInputFilename, string sOutputFilename)
        {
            Aes a = Aes.Create();
            string sKey = "qawsedrftgyhujikqawsedrftgyhujik";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(sKey);
            FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(sKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            a.Key = pdb.GetBytes(32);
            a.IV = pdb.GetBytes(16);

            ICryptoTransform desencrypt = a.CreateEncryptor();//DES.CreateEncryptor();
            
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);
            
            byte[] bytearrayinput = new byte[fsInput.Length];
            byte[] byteen = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            label2.Text = bytearrayinput.Length.ToString();
            
            foreach (byte data in bytearrayinput)
            {
                cryptostream.WriteByte(data);
            }

            
            cryptostream.Close();
            fsInput.Close();
            fsInput.Close();


        }

        private void hideInImage(byte[,] red1, byte[,] green1, byte[,] blue1)
        {
            byte[,] red = red1;
            int randh = 0, randw = 0;
            byte[,] green = green1;
            byte[,] blue = blue1;
            #region hide data in image
             int height =red.GetLength(0), width=red.GetLength(1);

            /*Since data will be added from  5th pixel. 1st three pixels used to store
             * the position of lastpixel in which data is stored
             * and 4th pixel storing whether the last pixel has used all it's capacity or not
             */
            long encryption=0,hidetime;
            int j = 4;
            try
            {
                string filinter = "C:\\temp\\example\\Data\\hello.txt";
                
                timer.Start();
                f(textfile, filinter);
                timer.Stop();
                encryption = timer.ElapsedMilliseconds;
                label7.Text = (encryption.ToString());
                
                byte[] fileData = StreamFile(filinter);    
                textfile=filinter;
                int len = fileData.Length;
                int i;
                int requiredBitsRed, requiredBitsGreen, requiredBitsBlue;
                var encryptbmp = new Bitmap(width, height);

                if ((len * 8 > capacity) || (width < 5))
                    throw new ImageToSmall("Image is too small to use");
                else
                {
                    timer.Start();
                    int currentByte = 0;
                    bool isDone = false;
                    int less;
                    byte byte_to_replace,a,b;
                    int offset = 0;
                    int pixel_row =0, pixel_column=0, pixel_color=0, num_of_bits_stored=5;
                    for (i = 0; i < height; i++)
                    {
                        for (; j < width; j++)
                        {
                            a=0;b=0;
                            

                            requiredBitsRed = requiredBit(red[i, j]);
                            requiredBitsGreen = requiredBit(green[i, j]);
                            requiredBitsBlue = requiredBit(blue[i, j]);

                            #region replace red pixel
                            byte_to_replace = 0;
                            if (requiredBitsRed <= 8 - offset)
                            {
                                byte_to_replace = get_byte(offset, fileData[currentByte], requiredBitsRed);
                                offset += requiredBitsRed;
                                if (offset == 8)
                                {
                                    offset = 0;
                                    ++currentByte;
                                }
                                if (currentByte == len)
                                {
                                    pixel_row = i;
                                    pixel_column = j;
                                    pixel_color = 1;
                                    num_of_bits_stored = requiredBitsRed;
                                    isDone = true;
                                    
                                }
                            }
                            else if (requiredBitsRed > 8 - offset)
                            {
                                less = 8 - offset;
                                a = get_byte(offset,fileData[currentByte],less);
                                currentByte += 1;
                                if (currentByte == len)
                                {
                                    pixel_row = i;
                                    pixel_column = j;
                                    pixel_color = 1;
                                    num_of_bits_stored = less;
                                    b = 0;
                                    isDone = true;
                                }
                                else
                                {
                                    offset = 0;
                                    b = get_byte(offset, fileData[currentByte], requiredBitsRed - less);
                                    offset += requiredBitsRed - less;
                                }
                                byte_to_replace = (byte)((a << requiredBitsRed - less) | b);
                            }

                            
                            red[i,j] = (byte)(maskByte(red[i,j],requiredBitsRed)|byte_to_replace);
                            if (isDone)
                                break;
                            #endregion

                            #region replace blue pixel
                            byte_to_replace = 0;
                            if (requiredBitsBlue <= 8 - offset)
                            {
                                byte_to_replace = get_byte(offset, fileData[currentByte], requiredBitsBlue);
                                offset += requiredBitsBlue;
                                if (offset == 8)
                                {
                                    offset = 0;
                                    ++currentByte;
                                }
                                if (currentByte == len)
                                {
                                    pixel_row = i;
                                    pixel_column = j;
                                    pixel_color = 2;
                                    num_of_bits_stored = requiredBitsBlue;
                                    isDone = true;
                                    
                                }
                            }
                            else if (requiredBitsBlue > 8 - offset)
                            {
                                less = 8 - offset;
                                a = get_byte(offset,fileData[currentByte],less);
                                currentByte += 1;
                                if (currentByte == len)
                                {
                                    pixel_row = i;
                                    pixel_column = j;
                                    pixel_color = 2;
                                    num_of_bits_stored = less;
                                    b = 0;
                                    isDone = true;
                                }
                                else
                                {
                                    offset = 0;
                                    b = get_byte(offset, fileData[currentByte], requiredBitsBlue - less);
                                    offset += requiredBitsBlue - less;
                                }
                                byte_to_replace = (byte)((a << requiredBitsBlue - less) | b);
                            }


                            blue[i, j] = (byte)(maskByte(blue[i, j], requiredBitsBlue) | byte_to_replace);
                            if (isDone)
                                break;
                            #endregion
                              
                            #region replace green pixel
                            byte_to_replace = 0;
                            if (requiredBitsGreen <= 8 - offset)
                            {
                                byte_to_replace = get_byte(offset, fileData[currentByte], requiredBitsGreen);
                                offset += requiredBitsGreen;
                                if (offset == 8)
                                {
                                    offset = 0;
                                    ++currentByte;
                                }
                                if (currentByte == len)
                                {
                                    pixel_row = i;
                                    pixel_column = j;
                                    pixel_color = 3;
                                    num_of_bits_stored = requiredBitsGreen;
                                    isDone = true;
                                    
                                }
                            }
                            else if (requiredBitsGreen > 8 - offset)
                            {
                                less = 8 - offset;
                                a = get_byte(offset,fileData[currentByte],less);
                                currentByte += 1;
                                if (currentByte == len)
                                {
                                    pixel_row = i;
                                    pixel_column = j;
                                    pixel_color = 3;
                                    num_of_bits_stored = less;
                                    b = 0;
                                    isDone = true;
                                }
                                else
                                {
                                    offset = 0;
                                    b = get_byte(offset, fileData[currentByte], requiredBitsGreen - less);
                                    offset += requiredBitsGreen - less;
                                }
                                byte_to_replace = (byte)((a << requiredBitsGreen - less) | b);
                            }


                            green[i, j] = (byte)(maskByte(green[i, j], requiredBitsGreen) | byte_to_replace);
                            if (isDone)
                                break;
                            #endregion                            
           
                        }
                        if (isDone)
                            break;
                        j = 0;
                    }
                    byte[] numb = new byte[4], Color = new byte[4], Row = new byte[4], Col = new byte[4];
                    if (isDone)
                    {
                        Row = BitConverter.GetBytes(pixel_row);
                        Col = BitConverter.GetBytes(pixel_column);
                        Color = BitConverter.GetBytes(pixel_color);
                        numb = BitConverter.GetBytes(num_of_bits_stored);
                    }
                    for (i = 0; i < 4; i++)
                        {
                            red[0, i] = Row[i];
                            blue[0, i] = Col[i];
                        }
                        green[0, 0] = Color[0];
                        green[0, 1] = Color[1];
                        green[0, 2] = numb[0];
                        green[0, 3] = numb[1];
                        randw = (pixel_column + 1) % (width-1);
                        randh = (j == 0) ? pixel_row + 1 : pixel_row;
                }
            }
            catch (Exception ex)
            {
                label4.Text = ex.Message;
            }

            if(randh!=height)
            {
                j=randw;
                byte randRed,randGreen,randBlue;
                Random rnd = new Random();
                for (i = randh; i < height; i++)
                {
                    for (; j < width; j++)
                    {
                        int requiredBitsRed, requiredBitsGreen, requiredBitsBlue;
                        requiredBitsRed = requiredBit(red[i, j]);
                        requiredBitsGreen = requiredBit(green[i, j]);
                        requiredBitsBlue = requiredBit(blue[i, j]);
                        randRed = (byte)rnd.Next((int)Math.Pow(2, requiredBitsRed));
                        randBlue = (byte)rnd.Next((int)Math.Pow(2, requiredBitsBlue));
                        randGreen = (byte)rnd.Next((int)Math.Pow(2, requiredBitsGreen));
                        red[i, j] = (byte)(maskByte(red[i, j], requiredBitsRed) | maskByte(randRed,requiredBitsRed));
                        blue[i, j] = (byte)(maskByte(blue[i, j], requiredBitsBlue) | maskByte(randBlue, requiredBitsBlue));
                        green[i, j] = (byte)(maskByte(green[i, j], requiredBitsGreen) | maskByte(randGreen,requiredBitsGreen));
                    }
                    j = 0;

                }
            }
            #endregion
            timer.Stop();
            hidetime = timer.ElapsedMilliseconds;
            label8.Text = hidetime.ToString();
            label6.Text = (encryption + hidetime).ToString();
            MLApp.MLApp matlab = new MLApp.MLApp();
            matlab.PutWorkspaceData("red", "base", red);
            matlab.PutWorkspaceData("green", "base", green);
            matlab.PutWorkspaceData("blue", "base", blue);
            matlab.Execute("im = cat(3,red,blue,green)");
            Console.WriteLine(matlab.Execute("imwrite(im,'C:\\temp\\example\\Data\\after2222.png')"));
            label3.Text = label3.Text + "completed";
            var form = new Form2("C:\\temp\\example\\Data\\after2222.png");
            form.Show();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            
        }


        static void D(Byte []b)
        {
            string sOutputFilename = "C:\\temp\\example\\Data\\hello2.txt";
            Aes a = Aes.Create();
            string sKey = "qawsedrftgyhujikqawsedrftgyhujik";
            byte[] bytes = System.Text.UTF8Encoding.UTF8.GetBytes(sKey);
            //FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);

            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(sKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            a.Key = pdb.GetBytes(32);
            a.IV = pdb.GetBytes(16);

            ICryptoTransform desencrypt = a.CreateDecryptor();//DES.CreateEncryptor();
            //invec = aes.IV;
            //p = aes.Padding;
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            //CryptoStream cryptostream = new CryptoStream(fsEncrypted, DES.CreateEncryptor(ASCIIEncoding.ASCII.GetBytes(sKey), ASCIIEncoding.ASCII.GetBytes(sKey)), CryptoStreamMode.Write);
            //byte[] bytearrayinput = new byte[fsInput.Length -1];
            /*byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);*/
            
            cryptostream.Write(b, 0, b.Length);

            //bsize = aes.BlockSize;
            //StreamWriter sWriter = new StreamWriter(cryptostream);
            //sWriter.WriteLine(Data);
            cryptostream.Close();
            fsEncrypted.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            long total, decryption = 0, retrieve;
            // Create the MATLAB instance 
            MLApp.MLApp matlab = new MLApp.MLApp();

            // Change to the directory where the function is located 
            matlab.Execute(@"cd c:\temp\example");
            label3.Text = label3.Text + "marlab call";
            // Define the output 
            object result = null;
            string impath = "C:\\temp\\example\\Data\\after2222.png";

            


            // Call the MATLAB function myfunc
            //Feval(String functionname, long numout, arg1, arg2, ...) As Object
            matlab.Feval("imreed", 3, out result, impath);
            label3.Text = label3.Text + "imreed call";
            // Display result 
            object[] res = result as object[];
            byte[,] red = res[0] as byte[,];
            byte[,] blue = res[1] as byte[,];
            byte[,] green = res[2] as byte[,];
            int height = red.GetLength(0), width = red.GetLength(1);

            #region retrieve data
            timer.Start();
            byte[] Col1 = new byte[4];
            byte[] Row1 = new byte[4];
            byte[] Color1 = new byte[4];
            byte[] numb1 = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                Row1[i] = red[0, i];
                Col1[i] = blue[0, i];
            }
            Color1[0] = green[0, 0];
            Color1[1] = green[0, 1]; Color1[3] = 0; Color1[2] = 0;
            numb1[0] = green[0, 2];
            numb1[1] = green[0, 3]; numb1[2] = 0; numb1[3] = 0;

            int height1 = BitConverter.ToInt32(Row1, 0);
            int width1 = BitConverter.ToInt32(Col1, 0);
            int j = 4; bool isDone1 = false;
            List<Byte> Data = new List<byte>();

            int col = BitConverter.ToInt32(Color1, 0);
            int number = BitConverter.ToInt32(numb1, 0);

            int bitsCount = 0, mask = 0;
            byte tempByte = 0, byteAdded = 0;

            for (int i = 0; i <= height1; i++)
            {
                int requiredBitsRed, requiredBitsBlue, requiredBitsGreen;

                for (; j < width; j++)
                {
                    requiredBitsRed = requiredBit(red[i, j]);
                    requiredBitsGreen = requiredBit(green[i, j]);
                    requiredBitsBlue = requiredBit(blue[i, j]);

                    {
                        int bitsNeeded;
                        #region get data from red channel
                        if (i == height1 && j == width1 && col == 1)
                        {
                            tempByte = red[i, j];
                            bitsCount += number;
                            bitsNeeded = number;
                            int maskLoopLimit = requiredBitsRed - bitsNeeded;
                            mask = 0;
                            for (int k = requiredBitsRed - 1; k >= (maskLoopLimit); k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            tempByte >>= (requiredBitsRed - bitsNeeded);
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            Data.Add(byteAdded);
                            isDone1 = true;
                            break;

                        }
                        else if ((bitsCount + requiredBitsRed) <= 8)
                        {
                            tempByte = red[i, j];
                            bitsCount += requiredBitsRed;
                            mask = 0;
                            for (int k = requiredBitsRed - 1; k >= 0; k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            byteAdded <<= requiredBitsRed;
                            byteAdded = (byte)(tempByte | byteAdded);
                            if (bitsCount == 8)
                            {
                                Data.Add(byteAdded);
                                byteAdded = 0;
                                bitsCount = 0;
                            }
                        }
                        else if (bitsCount + requiredBitsRed > 8)
                        {
                            tempByte = red[i, j];
                            bitsNeeded = 8 - bitsCount;
                            int maskLoopLimit = requiredBitsRed - bitsNeeded;
                            mask = 0;
                            for (int k = requiredBitsRed - 1; k >= (maskLoopLimit); k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            tempByte >>= (requiredBitsRed - bitsNeeded);
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            bitsCount += bitsNeeded;
                            if (bitsCount == 8)
                            {
                                Data.Add(byteAdded);
                                byteAdded = 0;
                                bitsCount = 0;
                            }
                            tempByte = red[i, j];

                            bitsNeeded = requiredBitsRed - bitsNeeded;
                            mask = 0;
                            for (int k = bitsNeeded - 1; k >= 0; k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            //this statement is not need but is here nonetheless
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            bitsCount = bitsNeeded;
                        }
                        #endregion

                        #region get data from blue channel
                        if (i == height1 && j == width1 && col == 2)
                        {
                            tempByte = blue[i, j];
                            bitsCount += number;
                            bitsNeeded = number;
                            int maskLoopLimit = requiredBitsBlue - bitsNeeded;
                            mask = 0;
                            for (int k = requiredBitsBlue - 1; k >= (maskLoopLimit); k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            tempByte >>= (requiredBitsBlue - bitsNeeded);
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            Data.Add(byteAdded);
                            isDone1 = true;
                            break;

                        }
                        else if ((bitsCount + requiredBitsBlue) <= 8)
                        {
                            tempByte = blue[i, j];
                            bitsCount += requiredBitsBlue;
                            mask = 0;
                            for (int k = requiredBitsBlue - 1; k >= 0; k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            byteAdded <<= requiredBitsBlue;
                            byteAdded = (byte)(tempByte | byteAdded);
                            if (bitsCount == 8)
                            {
                                Data.Add(byteAdded);
                                byteAdded = 0;
                                bitsCount = 0;
                            }
                        }
                        else if (bitsCount + requiredBitsBlue > 8)
                        {
                            tempByte = blue[i, j];
                            bitsNeeded = 8 - bitsCount;
                            int maskLoopLimit = requiredBitsBlue - bitsNeeded;
                            mask = 0;
                            for (int k = requiredBitsBlue - 1; k >= (maskLoopLimit); k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            tempByte >>= (requiredBitsBlue - bitsNeeded);
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            bitsCount += bitsNeeded;
                            if (bitsCount == 8)
                            {
                                Data.Add(byteAdded);
                                byteAdded = 0;
                                bitsCount = 0;
                            }
                            tempByte = blue[i, j];

                            bitsNeeded = requiredBitsBlue - bitsNeeded;
                            mask = 0;
                            for (int k = bitsNeeded - 1; k >= 0; k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            //this statement is not need but is here nonetheless
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            bitsCount = bitsNeeded;
                        }
                        #endregion

                        #region get data from green channel
                        if (i == height1 && j == width1 && col == 3)
                        {
                            tempByte = green[i, j];
                            bitsCount += number;
                            bitsNeeded = number;
                            int maskLoopLimit = requiredBitsGreen - bitsNeeded;
                            mask = 0;
                            for (int k = requiredBitsGreen - 1; k >= (maskLoopLimit); k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            tempByte >>= (requiredBitsGreen - bitsNeeded);
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            Data.Add(byteAdded);
                            isDone1 = true;
                            break;

                        }
                        else if ((bitsCount + requiredBitsGreen) <= 8)
                        {
                            tempByte = green[i, j];
                            bitsCount += requiredBitsGreen;
                            mask = 0;
                            for (int k = requiredBitsGreen - 1; k >= 0; k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            byteAdded <<= requiredBitsGreen;
                            byteAdded = (byte)(tempByte | byteAdded);
                            if (bitsCount == 8)
                            {
                                Data.Add(byteAdded);
                                byteAdded = 0;
                                bitsCount = 0;
                            }
                        }
                        else if (bitsCount + requiredBitsGreen > 8)
                        {
                            tempByte = green[i, j];
                            bitsNeeded = 8 - bitsCount;
                            int maskLoopLimit = requiredBitsGreen - bitsNeeded;
                            mask = 0;
                            for (int k = requiredBitsGreen - 1; k >= (maskLoopLimit); k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            tempByte >>= (requiredBitsGreen - bitsNeeded);
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            bitsCount += bitsNeeded;
                            if (bitsCount == 8)
                            {
                                Data.Add(byteAdded);
                                byteAdded = 0;
                                bitsCount = 0;
                            }
                            tempByte = green[i, j];

                            bitsNeeded = requiredBitsGreen - bitsNeeded;
                            mask = 0;
                            for (int k = bitsNeeded - 1; k >= 0; k--)
                            {
                                mask += Convert.ToInt32(Math.Pow(2, k));
                            }
                            tempByte = (byte)(tempByte & Convert.ToByte(mask));
                            //this statement is not need but is here nonetheless
                            byteAdded <<= bitsNeeded;
                            byteAdded = (byte)(byteAdded | tempByte);
                            bitsCount = bitsNeeded;
                        }
                        #endregion


                    }
                    if (isDone1)
                        break;
                }
                j = 0;
                if (isDone1)
                    break;
            }
            timer.Stop();
            #endregion
            retrieve = timer.ElapsedMilliseconds;
            byte [] b=Data.ToArray();
            timer.Start();
            D(b);
            timer.Stop();
            decryption = timer.ElapsedMilliseconds;
            label10.Text = decryption.ToString();
            label9.Text = retrieve.ToString();
            label11.Text = (decryption + retrieve).ToString();
            #region check to see if data retrieved is equal
            List<byte> originData = new List<byte>(StreamFile(textfile));
            bool equals = originData.SequenceEqual(Data);
            if (equals)
            {
                label5.Text = "Data hidden and retrieved correctly " + (kl++).ToString() + " times";
            }
            else if (!equals)
            {
                label5.Text = "Data got corrupted";
            }
            #endregion
            label3.Text = label3.Text + "completed";
        }

        

    }
}
