using System;
using System.IO;

namespace BMP_example
{
    class Program
    {
        static void Main(string[] args)
        {
            //Sudaromas bmp formato monochrominis 1000x1000 taškų paveikslėlis
            var header = new byte[62]
                {
                    //Antraštė
                    0x42, 0x4d,
                    0x0, 0x0, 0x0, 0x0, //0x3e, 0xf4, 0x1, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    //Antraštės informacija
                    0x28, 0x0, 0x0, 0x0,
                    0xe8, 0x3, 0x0, 0x0,
                    0xe8, 0x3, 0x0, 0x0,
                    0x1, 0x0,
                    0x1, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0x0, 0x0,
                    //Spalvų lentelė 
                    0xff, 0x0, 0x0, 0x0,
                    0x0, 0x0, 0xff, 0x0
                };
            using (FileStream file = new FileStream("sample.bmp", FileMode.Create, FileAccess.Write))
            {
                file.Write(header);
                //Suskaičiuojame bmp paveikslėlio eilutės duomenų kiekį baitais (4 kartotinis) (1000 + 31) / 32 * 4  = 128 
                int l = (1000 + 31) / 32 * 4;
                //Apibrėžiame taškų masyvą. Masyvo pirmo taško spalvą atitiks masyvo pirmo bito reikšmė
                var t = new byte[1000 * l];
                //Paišome kvadratą 128x128 taškų, kurio kairys apatinis kampas sutampa su bmp paveikslėlio apatiniu kairiu kampu.
                for (int i = 0; i < 128; i++)
                {
                    for (int j = i * l; j < i * l + 128 / 8; j++)
                    {
                        t[j] = 0xFF;
                    }
                }
                //Paišome vertikalę liniją per visą paveikslėlį y = 7.
                for (int i = 0; i < 1000 * l; i += l)
                    t[i] ^= 1;
                //Paišome įstrižai liniją per visą paveikslėlį nuo (0, 0) iki (999, 999)
                byte patern = 0b10000000;
                int p = 0;
                for (int i = 0; i < 1000; i++)
                {
                    
                    t[p] ^= patern;
                    patern = patern == 1 ? (byte)(0b10000000) : (byte)(patern >> 1);
                    p += l + (patern >> 7);
                }
                file.Write(t);
                file.Close();
                using (FileStream file2 = new FileStream("sample2.bmp", FileMode.Create, FileAccess.Write))
                {
                    var header2 = new byte[54];
                    Array.Copy(header, header2, header.Length - 8);
                    header2[0x1C] = 8;
                    //Pataisome paveikslėlio duomenų pradžią
                    //Array.Copy(BitConverter.GetBytes((int)(54 + 256 * 256)), 0, header2, 0xA, sizeof(int));
                    //Array.Copy(BitConverter.GetBytes((int)(0x100)), 0, header2, 0x2E, sizeof(int));
                    //Pataisome paveikslėlio plotį į 1600
                    Array.Copy(BitConverter.GetBytes((int)1600),0,header2,0x12,sizeof(int));
                    //Pataisome paveikslėlio aukštį į 1600
                    Array.Copy(BitConverter.GetBytes((int)1600), 0, header2, 0x16, sizeof(int));
                    //Susirandame baitų skaičių paveikslėlio eilutėje
                    int l2 = (1600 + 3) / 4 * 4; 
                    //pataisome bylos dydį
                    //Array.Copy(BitConverter.GetBytes((int)(52 + 256 * 4 + 1600 * l2)), 0, header2, 0x2, sizeof(int));
                    //Spalvų lentelė
                    var colors = new byte[4 * 256];
                    //Nustatome spalvas (pagal savo poreikius)
                    int tmp = 0;
                    for (int i = 0; i <= 255; i+=85)
                    {
                    
                        for (int j = 0; j <= 255; j += 36)
                        {
                           
                            for (int k = 0; k <= 255; k+=36)
                            {
                                //Console.WriteLine($"i={i} j={j} k={k}");
                                colors[tmp] = (byte)i;
                                tmp++;
                                colors[tmp] = (byte)k;
                                tmp++;
                                colors[tmp] = (byte)j;
                                tmp += 2;
                            }
                        }
                    }
                    
                    var data = new byte[54 + 254 * 4 + 1600 * l2];
                    //Nudažome paveikslėlį
                    for (int i = 0; i < 1600; i++)
                        for (int j = 0; j < 1600; j++)
                        {
                            //data[i + j * l2] = (byte)( (i / 100 + 1) * (j / 100 + 1) - 1);
                            data[i + j * l2] = (byte)(16 * (i / 100) + (j / 100));
                        }

                    //Paišome kvadratą, kurio centras (500, 500) įstrižainės ilgis 200)
                    int hlp = 200 / 2;
                    for (int y = -hlp; y <= hlp; y++)
                        for (int x = -(hlp - Math.Abs(y)); x <= hlp - Math.Abs(y); x++)
                        {
                            data[500 + x + (500 + y) * l2] = 255;
                        }
                    //Paišome sritulį, kurio centras (1000, 1000) spindulio ilgis 100)
                    hlp = 100;
                    for (int y = -hlp; y <= hlp; y++)
                        for (int x = -(int)Math.Sqrt(hlp*hlp-y*y); x <= (int)Math.Sqrt(hlp * hlp - y * y); x++)
                        {
                            data[1000 + x + (1000 + y) * l2] = 0;
                        }
                    //Išsaugome paveikslėlį
                    file2.Write(header2);
                    file2.Write(colors);
                    file2.Write(data);
                    file2.Close();
                }

            }
        }
    }
}
