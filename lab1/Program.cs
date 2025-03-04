using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection.Emit;

namespace lab1
{
    class Program
    {
        //testing
        //static long actionCount = 0;
        static int widthLimit = 6;

        static void Main(string[] args)
        {
            int resolution = 1296; //6 //36 //216 //648 //1296 //3888 //7776
            int recDepth = 3;

            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            byte[,] table = new byte[resolution, resolution];
            fillWhite(table);

            drawRecursively(table, 0, 0, recDepth, resolution);

            byte[] bpm = createBmp(table);

            var header = new byte[54]
                {
                    //Antraštė
                    0x42, 0x4d,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,

                    //Antraštės informacija
                    0x28, 0x0, 0x0, 0x0,
                    0xe8, 0x3, 0x0, 0x0,
                    0xe8, 0x3, 0x0, 0x0,
                    0x1, 0x0,
                    0x10, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0
                };

            using (FileStream file = new FileStream("test.bmp", FileMode.Create, FileAccess.Write))
            {
                Array.Copy(BitConverter.GetBytes((int)resolution), 0, header, 0x12, sizeof(int));
                Array.Copy(BitConverter.GetBytes((int)resolution), 0, header, 0x16, sizeof(int));

                file.Write(header);

                file.Write(bpm);
                file.Close();
            }

            //stopwatch.Stop();
            //Console.WriteLine($"Programa veikė: {stopwatch.ElapsedMilliseconds} ms");

            //actionCount += 14;
            //Console.WriteLine($"Programos veiksmų skaičius: {actionCount}");
        }

        static void fillWhite(byte[,] table)
        {
            drawColorSquare(0, 0, table.GetLength(0), table, 1);
            //actionCount++;
        }

        static void drawColorSquare(int x, int y, int width, byte[,] table, byte color)
        {
            for (int i = y; i < y + width; i++)
            {
                for (int j = x; j < x + width; j++)
                {
                    table[i, j] = color;
                    //actionCount++;
                }

                //actionCount++;
            }

            //actionCount++;
        }

        static void drawRecursively(byte[,] table, int x, int y, int recDepth, int width)
        {
            if (recDepth == 0)
            {
                drawFinish(x, y, width, table);
                //Console.WriteLine("Reached recursion depth limit");
                //actionCount += 2;
                return;
            }

            if (width <= widthLimit)
            {
                drawFinish(x, y, width, table);
                //Console.WriteLine("Reached width limit");
                //actionCount += 2;
                return;
            }

            drawPrimary(x, y, width, table);

            width = width / 6;

            drawRecursively(table, x + width * 4, y + width * 5, recDepth - 1, width);
            drawRecursively(table, x, y + width * 3, recDepth - 1, width);
            drawRecursively(table, x + width * 4, y + width * 2, recDepth - 1, width);
            drawRecursively(table, x + width * 5, y + width, recDepth - 1, width);
            drawRecursively(table, x + width * 3, y, recDepth - 1, width);

            //actionCount += 9;
        }

        static void drawFinish(int x, int y, int width, byte[,] table)
        {
            int w3 = width / 3;
            drawSquare(x + w3, y + w3, w3, table);
            drawFinishGrid(x, y, width, table);
            //actionCount += 3;
        }

        static void drawFinishGrid(int x, int y, int width, byte[,] table)
        {
            for (int i = 1; i < 3; i++)
            {
                drawVertical(x + width / 3 * i, y, width, table);
                drawHorizontal(x, y + width / 3 * i, width, table);
                //actionCount += 2;
            }

            //actionCount++;
        }

        static void drawPrimary(int x, int y, int width, byte[,] table)
        {
            int w3 = width / 3;
            drawSquare(x + w3, y + w3, w3, table);
            drawGrid(x, y, width, table);
            //actionCount += 3;
        }

        static void drawSquare(int x, int y, int width, byte[,] table)
        {
            drawColorSquare(x, y, width, table, 0);
            //actionCount++;
        }

        static void drawGrid(int x, int y, int width, byte[,] table)
        {
            int w6 = width / 6;
            for (int i = 1; i < 6; i++)
            {
                drawVertical(x + w6 * i, y, width, table);
                drawHorizontal(x, y + w6 * i, width, table);
                //actionCount += 2;
            }

            //actionCount += 2;
        }

        static void drawVertical(int x, int y, int width, byte[,] table)
        {
            for (int i = y; i < y + width; i++)
            {
                table[i, x] = 0;
                //actionCount++;
            }

            //actionCount++;
        }

        static void drawHorizontal(int x, int y, int width, byte[,] table)
        {
            for (int i = x; i < x + width; i++)
            {
                table[y, i] = 0;
                //actionCount++;
            }

            //actionCount++;
        }

        static byte[] createBmp(byte[,] table)
        {
            int width = table.GetLength(0);
            long line = (width * 16 + 31) / 32 * 4;
            byte[] bpm = new byte[width * line];

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ushort pixel = table[y, x];
                    ushort color = (pixel == 1) ? (ushort)0xFFFF : (ushort)0x0000;
                    long pos = y * line + x * 2;
                    bpm[pos] = (byte)(color & 0xFF);            // Low byte
                    bpm[pos + 1] = (byte)((color >> 8) & 0xFF); // High byte

                    //actionCount += 5;
                }

                //actionCount++;
            }

            //actionCount += 5;

            return bpm;
        }
    }
}
