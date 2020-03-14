using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace shash
{
    public partial class Form1 : Form
    {
        int d3, b, t3 = 0; //Для определения полей дамки после рубки
        int hod_a = 0, hod_b = 0;
        int direct = 0; //1 - 10:30 2 - 01:30 3 - 04:30 4 - 7:30 Для того чтобы дамка не могла рубить после рубки в противоположную сторону
        int k = 1, c = 0, d = 0, k1 = 1, d1 = 0, c1 = 0, t = 0;//Для определения ходов дамки
        bool move = true;//true-white false - black
        const int m = 30, n = 30;//постоянные переменные для доски
        int[,] place = new int[m, n]; //создание массива, где параметры обозначают какая фигура на поле или ее отсутствие
        Button[,] btns = new Button[m, n];//создание массива, где параметры обозначают кнопку
        Button movement;//когда выбрали поле куда ходить эта переменная подсказывает откуда ходим 
        int i, j; //i,j для массива
        int i1, j1; //запомнить предыдущее местоположение шашки или для перебирания массива в массиве, чтобы не сбить значения i,j
        int i2 = -1; //используем если одна шашка срублена и этой же можем срубить еще какую-нибудь
        int i3, j3; //Запоминаем координаты переменной movement
        int i4, j4; //содержат значения какой шашкой уже что-то срубили. В этом случае нельзя будет продолжить рубить другими в этот же ход
        bool positive = false;//определяем есть ли что рубить, если есть то единственные возможные ходы будут рубкой
        int savevar,savevarenemy1,savevarenemy2;

        Socket socket;
        Socket client;
        byte[] buffer;
        byte[] msg;
        MemoryStream stream;
        string[] bufferplace = new string[64];
        public Form1()
        {
            InitializeComponent();
            for (i = 11; i < m - 11; i++)
            {
                for (j = 11; j < n - 11; j++)
                {
                    if ((i + j) % 2 != 0 && (i == 18 || i == 17 || i == 16)) { place[i, j] = 2; }
                    if ((i + j) % 2 != 0 && (i == 11 || i == 12 || i == 13)) { place[i, j] = 3; }
                    if ((i + j) % 2 != 0 && (i == 14 || i == 15)) { place[i, j] = 1; }

                    btns[11, 12] = b8b; btns[13, 12] = b6b; btns[15, 12] = b4b; btns[17, 12] = b2b;
                    btns[11, 14] = d8d; btns[13, 14] = d6d; btns[15, 14] = d4d; btns[17, 14] = d2d;
                    btns[11, 16] = f8f; btns[13, 16] = f6f; btns[15, 16] = f4f; btns[17, 16] = f2f;
                    btns[11, 18] = h8h; btns[13, 18] = h6h; btns[15, 18] = h4h; btns[17, 18] = h2h;
                    btns[12, 11] = a7a; btns[14, 11] = a5a; btns[16, 11] = a3a; btns[18, 11] = a1a;
                    btns[12, 13] = c7c; btns[14, 13] = c5c; btns[16, 13] = c3c; btns[18, 13] = c1c;
                    btns[12, 15] = e7e; btns[14, 15] = e5e; btns[16, 15] = e3e; btns[18, 15] = e1e;
                    btns[12, 17] = g7g; btns[14, 17] = g5g; btns[16, 17] = g3g; btns[18, 17] = g1g;
                }
            } //инициализация значений элементов массива
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            buffer = new byte[1000000];
            msg = new byte[1000000];
            stream = new MemoryStream(buffer); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, 904));
            socket.Listen(100);
            
        }

        void directfunc()
        {
            i2 = i;
            if (i1 > i && j1 > j)
            {
                while (place[i + k, j + k] != place[i1, j1])
                {
                    place[i + k, j + k] = 1;
                    btns[i + k, j + k].BackgroundImage = null;
                    btns[i + k, j + k].Text = null;
                    k++;
                }
                k = 1; direct = 3;
            }
            else if (i1 < i && j1 < j)
            {
                while (place[i - k, j - k] != place[i1, j1])
                {
                    place[i - k, j - k] = 1;
                    btns[i - k, j - k].BackgroundImage = null;
                    btns[i - k, j - k].Text = null;
                    k++;
                }
                k = 1; direct = 1;
            }
            else if (i1 < i && j1 > j)
            {
                while (place[i - k, j + k] != place[i1, j1])
                {
                    place[i - k, j + k] = 1;
                    btns[i - k, j + k].Text = null;
                    btns[i - k, j + k].BackgroundImage = null;
                    k++;
                }
                k = 1; direct = 2;
            }
            else if (i1 > i && j1 < j)
            {
                while (place[i + k, j - k] != place[i1, j1])
                {
                    place[i + k, j - k] = 1;
                    btns[i + k, j - k].Text = null;
                    btns[i + k, j - k].BackgroundImage = null;
                    k++;
                }
                k = 1; direct = 4;
            }
        }

        void restart()
        {
            for (i = 11; i < m - 11; i++)
                for (j = 11; j < n - 11; j++)
                {
                    if ((i + j) % 2 != 0 && (i == 18 || i == 17 || i == 16))
                    { place[i, j] = 2; btns[i, j].BackgroundImage = button2.BackgroundImage; btns[i, j].Text = null; }
                    if ((i + j) % 2 != 0 && (i == 11 || i == 12 || i == 13))
                    { place[i, j] = 3; btns[i, j].BackgroundImage = button4.BackgroundImage; btns[i, j].Text = null; }
                    if ((i + j) % 2 != 0 && (i == 14 || i == 15))
                    {
                        place[i, j] = 1; btns[i, j].BackgroundImage = null; btns[i, j].Text = null;
                    }
                    move = true;
                }
        }

        void hod_dama()
        {
            btns[i, j].BackgroundImage = movement.BackgroundImage;
            movement.BackgroundImage = null;
            btns[i, j].Text = movement.Text;
            i1 = i; j1 = j;
            btns[i, j].Text = "D";
        }

        void repickobject()
        {
            for (i1 = 11; i1 < m - 11; i1++) //Снимаем выделение с полей, допустим мы выбрали чем будем ходить, но потом сменили решение, тогда надо очистить закрашенные поля прошлого выбора шашки и полей куда она могла сходить
                for (j1 = 11; j1 < n - 11; j1++)
                    if (btns[i1, j1] != null)
                        btns[i1, j1].BackColor = Color.White;
            btns[i, j].BackColor = Color.Red; // меняем фон выбранной шашки

            movement = btns[i, j]; //запоминаем значение где стоит эта шашка
            i3 = i; j3 = j;//запоминаем координаты, чтобы лишний раз не считывать их с movement
        }

        void variables()
        {
            k = 1;d = 0;c = 0;
        }

        void variables_another_one()
        {
            k1 = 1;c1 = 0;d1 = 0;
        }

        void one_more_variables()
        {
            k = 1; t3 = 0; c = 0; d = 0;
        }

        void checkplace()
        {
            while (place[i + k, j + k] == 1)
            {
                btns[i + k, j + k].BackColor = Color.Green;
                k++;
            }
            k = 1;
            while (place[i - k, j + k] == 1)
            {
                btns[i - k, j + k].BackColor = Color.Green;
                k++;
            }
            k = 1;
            while (place[i + k, j - k] == 1)
            {
                btns[i + k, j - k].BackColor = Color.Green;
                k++;
            }
            k = 1;
            while (place[i - k, j - k] == 1)
            {
                btns[i - k, j - k].BackColor = Color.Green;
                k++;
            }
            k = 1;
        }

        void clearfunc()
        {
            for (i = 11; i < m - 11; i++)
            {
                for (j = 11; j < n - 11; j++)
                {
                    if (btns[i, j] == movement)
                    {
                        place[i, j] = 1;
                        btns[i, j].Text = null;
                    }
                    if (btns[i, j] == movement && place[i1, j1] == savevar && positive == true)
                    {
                        directfunc();
                    }
                    if (btns[i, j] != null)
                        btns[i, j].BackColor = Color.White;
                }
            }
            movement = null;
            positive = false;
        }

        void clearfunc_shash()
        {
            btns[i, j].BackgroundImage = movement.BackgroundImage;
            movement.BackgroundImage = null;
            i1 = i; j1 = j;
            for (i = 11; i < m - 11; i++)
            {
                for (j = 11; j < n - 11; j++)
                {
                    if (btns[i, j] == movement)
                    {
                        place[i, j] = 1;
                    }
                    if (btns[i, j] == movement && (i + i1) % 2 == 0 && place[i1, j1] == savevar)
                    {
                        i2 = i;
                        place[(i + i1) / 2, (j + j1) / 2] = 1;
                        btns[(i + i1) / 2, (j + j1) / 2].BackgroundImage = null;
                        btns[(i + i1) / 2, (j + j1) / 2].Text = null;
                    }
                    if (btns[i, j] != null)
                        btns[i, j].BackColor = Color.White;
                }
            }
            movement = null;
        }

        void checkenemy()
        {
            if (place[i, j] == 4 || place[i, j] == 2)
            {
                savevar = place[i, j];
                savevarenemy1 = 3;
                savevarenemy2 = 5;
            }
            else
            {
                savevar = place[i, j];
                savevarenemy1 = 2;
                savevarenemy2 = 4;
            }
        }

        void check_direct()
        {
            if (direct != 1)
            {
                while ((place[i1 + k, j1 + k] == 1 || place[i1 + k, j1 + k] == savevarenemy1 || place[i1 + k, j1 + k] == savevarenemy2) && c != 2)
                {
                    if (place[i1 + k, j1 + k] == 1 && d != 0)
                    {
                        c = 0;
                    }
                    if ((place[i1 + k, j1 + k] == savevarenemy1) || (place[i1 + k, j1 + k] == savevarenemy2))
                    {
                        c++; d++;
                    }
                    if (((place[i1 + k, j1 + k] == savevarenemy2) || (place[i1 + k, j1 + k] == savevarenemy1)) && (place[i1 + k + 1, j1 + k + 1] == 1) && c != 2)
                    {
                        positive = true;
                    }
                    k++;
                }
                variables();
            }
            if (direct != 3)
            {
                while ((place[i1 - k, j1 - k] == 1 || place[i1 - k, j1 - k] == savevarenemy1 || place[i1 - k, j1 - k] == savevarenemy2) && c != 2)
                {
                    if (place[i1 - k, j1 - k] == 1 && d != 0)
                    {
                        c = 0;
                    }
                    if ((place[i1 - k, j1 - k] == savevarenemy1) || (place[i1 - k, j1 - k] == savevarenemy2))
                    {
                        c++; d++;
                    }
                    if (((place[i1 - k, j1 - k] == savevarenemy1) || (place[i1 - k, j1 - k] == savevarenemy2)) && (place[i1 - k - 1, j1 - k - 1] == 1) && c != 2)
                        positive = true;
                    k++;
                }
                variables();
            }
            if (direct != 2)
            {
                while ((place[i1 + k, j1 - k] == 1 || place[i1 + k, j1 - k] == savevarenemy1 || place[i1 + k, j1 - k] == savevarenemy2) && c != 2)
                {
                    if (place[i1 + k, j1 - k] == 1 && d != 0)
                    {
                        c = 0;
                    }
                    if ((place[i1 + k, j1 - k] == savevarenemy1) || (place[i1 + k, j1 - k] == savevarenemy2))
                    {
                        c++; d++;
                    }
                    if (((place[i1 + k, j1 - k] == savevarenemy1) || (place[i1 + k, j1 - k] == savevarenemy2)) && (place[i1 + k + 1, j1 - k - 1] == 1) && c != 2)
                        positive = true;
                    k++;
                }
                variables();
            }
            if (direct != 4)
            {
                while ((place[i1 - k, j1 + k] == 1 || place[i1 - k, j1 + k] == savevarenemy1 || place[i1 - k, j1 + k] == savevarenemy2) && c != 2)
                {
                    if (place[i1 - k, j1 + k] == 1 && d != 0)
                    {
                        c = 0;
                    }
                    if ((place[i1 - k, j1 + k] == savevarenemy1) || (place[i1 - k, j1 + k] == savevarenemy2))
                    {
                        c++; d++;
                    }
                    if (((place[i1 - k, j1 + k] == savevarenemy1) || (place[i1 - k, j1 + k] == savevarenemy2)) && (place[i1 - k - 1, j1 + k + 1] == 1) && c != 2)
                        positive = true;
                    k++;
                }
                variables();
            }
        }

        void next_move()
        {
            if (Math.Abs(i1 - i2) > 1 && i2 > 0 &&
                                       ((((place[i1 + 1, j1 + 1] == savevarenemy1) || (place[i1 + 1, j1 + 1] == savevarenemy2)) && place[i1 + 2, j1 + 2] == 1) ||
                                       (((place[i1 - 1, j1 + 1] == savevarenemy1) || (place[i1 - 1, j1 + 1] == savevarenemy2)) && place[i1 - 2, j1 + 2] == 1) ||
                                       (((place[i1 + 1, j1 - 1] == savevarenemy1) || (place[i1 + 1, j1 - 1] == savevarenemy2)) && place[i1 + 2, j1 - 2] == 1) ||
                                       (((place[i1 - 1, j1 - 1] == savevarenemy1) || (place[i1 - 1, j1 - 1] == savevarenemy2)) && place[i1 - 2, j1 - 2] == 1)))
            {
                if (savevar == 2 || savevar == 4)
                    positive = true;
                else
                    positive = false;
                i4 = i1; j4 = j1;
                btns[i1,j1].PerformClick();
            }
            else//если дальше рубить нечего, то передаем ход 
            {
                if (savevar == 2 || savevar == 4)
                {
                    move = false; button4.Visible = true; button2.Visible = false;
                }
                else
                {
                    move = true; button4.Visible = false; button2.Visible = true;
                }

                positive = false; i2 = -1;
            }
        }

        void hod_shash()
        {
            if (positive == true)
            {
                if (btns[i4, j4] == btns[i, j])
                {
                    if ((place[i4 + 1, j4 + 1] == savevarenemy1 || place[i4 + 1, j4 + 1] == savevarenemy2) && place[i4 + 2, j4 + 2] == 1)
                        btns[i4 + 2, j4 + 2].BackColor = Color.Green;
                    if ((place[i4 - 1, j4 + 1] == savevarenemy1 || place[i4 - 1, j4 + 1] == savevarenemy2) && place[i4 - 2, j4 + 2] == 1)
                        btns[i4 - 2, j4 + 2].BackColor = Color.Green;
                    if ((place[i4 + 1, j4 - 1] == savevarenemy1 || place[i4 + 1, j4 - 1] == savevarenemy2) && place[i4 + 2, j4 - 2] == 1)
                        btns[i4 + 2, j4 - 2].BackColor = Color.Green;
                    if ((place[i4 - 1, j4 - 1] == savevarenemy1 || place[i4 - 1, j4 - 1] == savevarenemy2) && place[i4 - 2, j4 - 2] == 1)
                        btns[i4 - 2, j4 - 2].BackColor = Color.Green;
                }
                if (place[i, j] == savevar && i2 < 0)
                {
                    if ((place[i + 1, j + 1] == savevarenemy1 || place[i + 1, j + 1] == savevarenemy2) && place[i + 2, j + 2] == 1)
                        btns[i + 2, j + 2].BackColor = Color.Green;
                    if ((place[i - 1, j + 1] == savevarenemy1 || place[i - 1, j + 1] == savevarenemy2) && place[i - 2, j + 2] == 1)
                        btns[i - 2, j + 2].BackColor = Color.Green;
                    if ((place[i + 1, j - 1] == savevarenemy1 || place[i + 1, j - 1] == savevarenemy2) && place[i + 2, j - 2] == 1)
                        btns[i + 2, j - 2].BackColor = Color.Green;
                    if ((place[i - 1, j - 1] == savevarenemy1 || place[i - 1, j - 1] == savevarenemy2) && place[i - 2, j - 2] == 1)
                        btns[i - 2, j - 2].BackColor = Color.Green;
                }
            }
            else
            {
                if (place[i, j] == 3)
                {
                    if (place[i + 1, j - 1] == 1)
                        btns[i + 1, j - 1].BackColor = Color.Green;
                    if (place[i + 1, j + 1] == 1)
                        btns[i + 1, j + 1].BackColor = Color.Green;
                }
                if (place[i, j] == 2)
                {
                    if (place[i - 1, j + 1] == 1)
                        btns[i - 1, j + 1].BackColor = Color.Green;
                    if (place[i - 1, j - 1] == 1)
                        btns[i - 1, j - 1].BackColor = Color.Green;
                }
            }
        }

        void hod_dam()
        {
            if (positive == true)
            {
                if (btns[i4, j4] == btns[i, j])
                {
                    while ((place[i4 + k, j4 + k] == 1 || place[i4 + k, j4 + k] == savevarenemy1 || place[i4 + k, j4 + k] == savevarenemy2) && c != 2)
                    {
                        if (place[i4 + k, j4 + k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i4 + k - k1, j4 + k + k1] == 1) || (place[i4 + k - k1, j4 + k + k1] == savevarenemy1) || (place[i4 + k - k1, j4 + k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 + k - k1, j4 + k + k1] == 1 && d1 != 0)
                                {
                                    t++;
                                    c1 = 0;
                                    btns[i4 + k, j4 + k].BackColor = Color.Green;
                                }
                                if (place[i4 + k - k1, j4 + k + k1] == savevarenemy1 || (place[i4 + k - k1, j4 + k + k1] == savevarenemy2))
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i4 + k + k1, j4 + k - k1] == 1) || (place[i4 + k + k1, j4 + k - k1] == savevarenemy1) || (place[i4 + k + k1, j4 + k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 + k + k1, j4 + k - k1] == 1 && d1 != 0)
                                {
                                    t++;
                                    c1 = 0; btns[i4 + k, j4 + k].BackColor = Color.Green;
                                }

                                if (place[i4 + k + k1, j4 + k - k1] == savevarenemy1 || place[i4 + k + k1, j4 + k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;

                                }
                                k1++;
                            }
                            variables_another_one();
                        }
                        if ((place[i4 + k, j4 + k] == savevarenemy1) || (place[i4 + k, j4 + k] == savevarenemy2))
                        {
                            c++; d++;
                        }
                        k++;
                        d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i4 + b, j4 + b] == savevarenemy1 || place[i4 + b, j4 + b] == savevarenemy2)
                                t3++;
                            if (place[i4 + b, j4 + b] == 1 && t3 > 0)
                                btns[i4 + b, j4 + b].BackColor = Color.Green;
                        }

                    }
                    else
                        t = 0;
                    one_more_variables();
                    while ((place[i4 - k, j4 - k] == 1 || place[i4 - k, j4 - k] == savevarenemy1 || place[i4 - k, j4 - k] == savevarenemy2) && c != 2)
                    {
                        if (place[i4 - k, j4 - k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i4 - k - k1, j4 - k + k1] == 1) || (place[i4 - k - k1, j4 - k + k1] == savevarenemy1) || (place[i4 - k - k1, j4 - k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 - k - k1, j4 - k + k1] == 1 && d1 != 0)
                                {
                                    t++;
                                    c1 = 0; btns[i4 - k, j4 - k].BackColor = Color.Green;
                                }

                                if (place[i4 - k - k1, j4 - k + k1] == savevarenemy1 || place[i4 - k - k1, j4 - k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;

                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i4 - k + k1, j4 - k - k1] == 1) || (place[i4 - k + k1, j4 - k - k1] == savevarenemy1) || (place[i4 - k + k1, j4 - k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 - k + k1, j4 - k - k1] == 1 && d1 != 0)
                                {
                                    t++;
                                    c1 = 0;
                                    btns[i4 - k, j4 - k].BackColor = Color.Green;
                                }

                                if (place[i4 - k + k1, j4 - k - k1] == savevarenemy1 || place[i4 - k + k1, j4 - k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                        }
                        if ((place[i4 - k, j4 - k] == savevarenemy1) || (place[i4 - k, j4 - k] == savevarenemy2))
                        {
                            c++; d++;
                        }
                        k++;
                        d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i4 - b, j4 - b] == savevarenemy1 || place[i4 - b, j4 - b] == savevarenemy2)
                                t3++;
                            if (place[i4 - b, j4 - b] == 1 && t3 > 0)
                                btns[i4 - b, j4 - b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables();
                    while ((place[i4 + k, j4 - k] == 1 || place[i4 + k, j4 - k] == savevarenemy1 || place[i4 + k, j4 - k] == savevarenemy2) && c != 2)
                    {
                        if (place[i4 + k, j4 - k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i4 + k + k1, j4 - k + k1] == 1) || (place[i4 + k + k1, j4 - k + k1] == savevarenemy1) || (place[i4 + k + k1, j4 - k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 + k + k1, j4 - k + k1] == 1 && d1 != 0)
                                {
                                    t++;
                                    c1 = 0;
                                    btns[i4 + k, j4 - k].BackColor = Color.Green;
                                }
                                if (place[i4 + k + k1, j4 - k + k1] == savevarenemy1 || place[i4 + k + k1, j4 - k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i4 + k - k1, j4 - k - k1] == 1) || (place[i4 + k - k1, j4 - k - k1] == savevarenemy1) || (place[i4 + k - k1, j4 - k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 + k - k1, j4 - k - k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i4 + k, j4 - k].BackColor = Color.Green;
                                }
                                if (place[i4 + k - k1, j4 - k - k1] == savevarenemy1 || place[i4 + k - k1, j4 - k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                        }
                        if ((place[i4 + k, j4 - k] == savevarenemy1) || (place[i4 + k, j4 - k] == savevarenemy2))
                        {
                            c++;
                            d++;
                        }
                        k++;
                        d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i4 + b, j4 - b] == savevarenemy1 || place[i4 + b, j4 - b] == savevarenemy2)
                                t3++;
                            if (place[i4 + b, j4 - b] == 1 && t3 > 0)
                                btns[i4 + b, j4 - b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables();
                    while ((place[i4 - k, j4 + k] == 1 || place[i4 - k, j4 + k] == savevarenemy1 || place[i4 - k, j4 + k] == savevarenemy2) && c != 2)
                    {
                        if (place[i4 - k, j4 + k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i4 - k + k1, j4 + k + k1] == 1) || (place[i4 - k + k1, j4 + k + k1] == savevarenemy1) || (place[i4 - k + k1, j4 + k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 - k + k1, j4 + k + k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i4 - k, j4 + k].BackColor = Color.Green;
                                }
                                if (place[i4 - k + k1, j4 + k + k1] == savevarenemy1 || place[i4 - k + k1, j4 + k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i4 - k - k1, j4 + k - k1] == 1) || (place[i4 - k - k1, j4 + k - k1] == savevarenemy1) || (place[i4 - k - k1, j4 + k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i4 - k + k1, j4 + k - k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i4 - k, j4 + k].BackColor = Color.Green;
                                }
                                if (place[i4 - k - k1, j4 + k - k1] == savevarenemy2 || place[i4 - k - k1, j4 + k - k1] == savevarenemy1)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                        }
                        if (place[i4 - k, j4 + k] == savevarenemy1 || place[i4 - k, j4 + k] == savevarenemy2)
                        {
                            c++; d++;
                        }
                        k++;
                        d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i4 - b, j4 + b] == savevarenemy1 || place[i4 - b, j4 + b] == savevarenemy2)
                                t3++;
                            if (place[i4 - b, j4 + b] == 1 && t3 > 0)
                                btns[i4 - b, j4 + b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables();
                }
                if (place[i, j] == 4 && i2 < 0)
                {
                    while ((place[i + k, j + k] == 1 || place[i + k, j + k] == savevarenemy1 || place[i + k, j + k] == savevarenemy2) && c != 2)
                    {
                        if (place[i + k, j + k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i + k - k1, j + k + k1] == 1) || (place[i + k - k1, j + k + k1] == savevarenemy1) || (place[i + k - k1, j + k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i + k - k1, j + k + k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i + k, j + k].BackColor = Color.Green;
                                }

                                if (place[i + k - k1, j + k + k1] == savevarenemy1 || place[i + k - k1, j + k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i + k + k1, j + k - k1] == 1) || (place[i + k + k1, j + k - k1] == savevarenemy1) || (place[i + k + k1, j + k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i + k + k1, j + k - k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i + k, j + k].BackColor = Color.Green;
                                }
                                if (place[i + k + k1, j + k - k1] == savevarenemy1 || place[i + k + k1, j + k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                        }
                        if ((place[i + k, j + k] == savevarenemy1) || (place[i + k, j + k] == savevarenemy2))
                        {
                            c++; d++;
                        }
                        k++; d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i + b, j + b] == savevarenemy1 || place[i + b, j + b] == savevarenemy2)
                                t3++;
                            if (place[i + b, j + b] == 1 && t3 > 0)
                                btns[i + b, j + b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables();
                    while ((place[i - k, j - k] == 1 || place[i - k, j - k] == savevarenemy1 || place[i - k, j - k] == savevarenemy2) && c != 2)
                    {
                        if (place[i - k, j - k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i - k - k1, j - k + k1] == 1) || (place[i - k - k1, j - k + k1] == savevarenemy2) || (place[i - k - k1, j - k + k1] == savevarenemy1)) && c1 != 2)
                            {
                                if (place[i - k - k1, j - k + k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i - k, j - k].BackColor = Color.Green;
                                }
                                if (place[i - k - k1, j - k + k1] == savevarenemy1 || place[i - k - k1, j - k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i - k + k1, j - k - k1] == 1) || (place[i - k + k1, j - k - k1] == savevarenemy1) || (place[i - k + k1, j - k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i - k + k1, j - k - k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i - k, j - k].BackColor = Color.Green;
                                }
                                if (place[i - k + k1, j - k - k1] == savevarenemy1 || place[i - k + k1, j - k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                        }
                        if ((place[i - k, j - k] == savevarenemy1) || (place[i - k, j - k] == savevarenemy2))
                        {
                            c++; d++;
                        }
                        k++;
                        d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i - b, j - b] == savevarenemy1 || place[i - b, j - b] == savevarenemy2)
                                t3++;
                            if (place[i - b, j - b] == 1 && t3 > 0)
                                btns[i - b, j - b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables(); ;
                    while ((place[i + k, j - k] == 1 || place[i + k, j - k] == savevarenemy1 || place[i + k, j - k] == savevarenemy2) && c != 2)
                    {
                        if (place[i + k, j - k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i + k + k1, j - k + k1] == 1) || (place[i + k + k1, j - k + k1] == savevarenemy1) || (place[i + k + k1, j - k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i + k + k1, j - k + k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i + k, j - k].BackColor = Color.Green;
                                }

                                if (place[i + k + k1, j - k + k1] == savevarenemy1 || place[i + k + k1, j - k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i + k - k1, j - k - k1] == 1) || (place[i + k - k1, j - k - k1] == savevarenemy1) || (place[i + k - k1, j - k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i + k - k1, j - k - k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i + k, j - k].BackColor = Color.Green;
                                }
                                if (place[i + k - k1, j - k - k1] == savevarenemy1 || place[i + k - k1, j - k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();

                        }
                        if ((place[i + k, j - k] == savevarenemy1) || (place[i + k, j - k] == savevarenemy2))
                        {
                            c++; d++;
                        }
                        k++; d3 = k;
                    }
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i + b, j - b] == savevarenemy1 || place[i + b, j - b] == savevarenemy2)
                                t3++;
                            if (place[i + b, j - b] == 1 && t3 > 0)
                                btns[i + b, j - b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables();
                    while ((place[i - k, j + k] == 1 || place[i - k, j + k] == savevarenemy1 || place[i - k, j + k] == savevarenemy2) && c != 2)
                    {
                        if (place[i - k, j + k] == 1 && d != 0)
                        {
                            c = 0;
                            while (((place[i - k + k1, j + k + k1] == 1) || (place[i - k + k1, j + k + k1] == savevarenemy1) || (place[i - k + k1, j + k + k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i - k + k1, j + k + k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i - k, j + k].BackColor = Color.Green;
                                }
                                if (place[i - k + k1, j + k + k1] == savevarenemy1 || place[i - k + k1, j + k + k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                            while (((place[i - k - k1, j + k - k1] == 1) || (place[i - k - k1, j + k - k1] == savevarenemy1) || (place[i - k - k1, j + k - k1] == savevarenemy2)) && c1 != 2)
                            {
                                if (place[i - k + k1, j + k - k1] == 1 && d1 != 0)
                                {
                                    c1 = 0; t++;
                                    btns[i - k, j + k].BackColor = Color.Green;
                                }
                                if (place[i - k - k1, j + k - k1] == savevarenemy1 || place[i - k - k1, j + k - k1] == savevarenemy2)
                                {
                                    c1++; d1++;
                                }
                                k1++;
                            }
                            variables_another_one();
                        }
                        if ((place[i - k, j + k] == savevarenemy1) || (place[i - k, j + k] == savevarenemy2))
                        {
                            c++; d++;
                        }
                        k++;
                        d3 = k;
                    }
                    variables();
                    if (t == 0)
                    {
                        for (b = 1; b <= d3; b++)
                        {
                            if (place[i - b, j + b] == savevarenemy1 || place[i - b, j + b] == savevarenemy2)
                                t3++;
                            if (place[i - b, j + b] == 1 && t3 > 0)
                                btns[i - b, j + b].BackColor = Color.Green;
                        }
                    }
                    else
                        t = 0;
                    one_more_variables();
                }
            }
            else
            {
                checkplace();
            }
        }

        private void ClickEventHandler(object sender)
        {
            for (i = 11; i < m - 11; i++)
            {
                for (j = 11; j < n - 11; j++)
                {
                    if (btns[i, j] == (sender as Button))
                    {
                        if (place[i, j] == 1)
                        {
                            if (move == true && btns[i, j].BackColor == Color.Green)//белые ход
                            {
                                if (i == 11 || place[i3, j3] == 4)
                                {
                                    place[i, j] = 4;
                                    checkenemy();
                                    hod_dama();
                                    clearfunc();
                                    check_direct();
                                    if (i2 > 0 && positive == true)
                                    {
                                        i4 = i1; j4 = j1; (sender as Button).PerformClick();
                                    }
                                    else
                                    {
                                        move = false; button4.Visible = true; button2.Visible = false; positive = false; i2 = -1; direct = 0;
                                    }
                                }
                                else
                                {
                                    place[i, j] = 2;
                                    checkenemy();
                                    clearfunc_shash();
                                    next_move();
                                }
                            }
                            else if (move == false && btns[i, j].BackColor == Color.Green)//черные ход
                            {
                                if (i == 18 || place[i3, j3] == 5)
                                {
                                    place[i, j] = 5;
                                    hod_dama();
                                    checkenemy();
                                    clearfunc();
                                    check_direct();
                                    if (i2 > 0 && positive == true)
                                    {
                                        i4 = i1; j4 = j1;
                                        (sender as Button).PerformClick();
                                    }
                                    else
                                    {
                                        move = true; button2.Visible = true; button4.Visible = false; positive = false; i2 = -1; direct = 0;
                                    }
                                }
                                else
                                {
                                    place[i, j] = 3;
                                    checkenemy();
                                    clearfunc_shash();
               
                                    next_move();
                                }

                            }
                        }
                        else
                        {
                            if (place[i, j] == 2 && move == true)//белые выбор шашки
                            {
                                repickobject();
                                checkenemy();
                                hod_shash();
                            }
                            else if (place[i, j] == 3 && move == false)//черные выбор шашки
                            {
                                repickobject();
                                checkenemy();
                                hod_shash();
                            }
                            else if (place[i, j] == 4 && move == true)//белые выбор шашки
                            {
                                repickobject();
                                checkenemy();
                                hod_dam();
                            }
                            else if (place[i, j] == 5 && move == false)//черные выбор шашки
                            {
                                repickobject();
                                checkenemy();
                                hod_dama();
                            }
                        }
                    }
                    for (i1 = 11; i1 < m - 11; i1++)
                    {//проверка на след ход, есть ли что рубить, но еще ничего не рубили
                        for (j1 = 11; j1 < n - 11; j1++)
                        {

                            if
                                (((place[i1, j1] == 3 && (place[i1 + 1, j1 + 1] == 2 || place[i1 + 1, j1 + 1] == 4) && place[i1 + 2, j1 + 2] == 1) ||
                                (place[i1, j1] == 3 && (place[i1 - 1, j1 + 1] == 2 || place[i1 - 1, j1 + 1] == 4) && place[i1 - 2, j1 + 2] == 1) ||
                                (place[i1, j1] == 3 && (place[i1 + 1, j1 - 1] == 2 || place[i1 + 1, j1 - 1] == 4) && place[i1 + 2, j1 - 2] == 1) ||
                                (place[i1, j1] == 3 && (place[i1 - 1, j1 - 1] == 2 || place[i1 - 1, j1 - 1] == 4) && place[i1 - 2, j1 - 2] == 1)) && move == false)
                            {
                                positive = true;
                            }
                            else if (((place[i1, j1] == 2 && (place[i1 + 1, j1 + 1] == 3 || place[i1 + 1, j1 + 1] == 5) && place[i1 + 2, j1 + 2] == 1) ||
                                (place[i1, j1] == 2 && (place[i1 - 1, j1 + 1] == 3 || place[i1 - 1, j1 + 1] == 5) && place[i1 - 2, j1 + 2] == 1) ||
                                (place[i1, j1] == 2 && (place[i1 + 1, j1 - 1] == 3 || place[i1 + 1, j1 - 1] == 5) && place[i1 + 2, j1 - 2] == 1) ||
                                (place[i1, j1] == 2 && (place[i1 - 1, j1 - 1] == 3 || place[i1 - 1, j1 - 1] == 5) && place[i1 - 2, j1 - 2] == 1)) && move == true)
                            {
                                positive = true;
                            }

                            if (place[i1, j1] == 4 && move == true)
                            {
                                if (direct != 1)
                                {
                                    while ((place[i1 + k, j1 + k] == 1 || place[i1 + k, j1 + k] == 5 || place[i1 + k, j1 + k] == 3) && c != 2)
                                    {
                                        if (place[i1 + k, j1 + k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 + k, j1 + k] == 5 || place[i1 + k, j1 + k] == 3))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 + k, j1 + k] == 5 || place[i1 + k, j1 + k] == 3) && (place[i1 + k + 1, j1 + k + 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                                if (direct != 3)
                                {
                                    while ((place[i1 - k, j1 - k] == 1 || place[i1 - k, j1 - k] == 5 || place[i1 - k, j1 - k] == 3) && c != 2)
                                    {
                                        if (place[i1 - k, j1 - k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 - k, j1 - k] == 5 || place[i1 - k, j1 - k] == 3))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 - k, j1 - k] == 5 || place[i1 - k, j1 - k] == 3) && (place[i1 - k - 1, j1 - k - 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                                if (direct != 2)
                                {
                                    while ((place[i1 + k, j1 - k] == 1 || place[i1 + k, j1 - k] == 5 || place[i1 + k, j1 - k] == 3) && c != 2)
                                    {
                                        if (place[i1 + k, j1 - k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 + k, j1 - k] == 5 || place[i1 + k, j1 - k] == 3))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 + k, j1 - k] == 5 || place[i1 + k, j1 - k] == 3) && (place[i1 + k + 1, j1 - k - 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                                if (direct != 4)
                                {
                                    while ((place[i1 - k, j1 + k] == 1 || place[i1 - k, j1 + k] == 5 || place[i1 - k, j1 + k] == 3) && c != 2)
                                    {
                                        if (place[i1 - k, j1 + k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 - k, j1 + k] == 5 || place[i1 - k, j1 + k] == 3))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 - k, j1 + k] == 5 || place[i1 - k, j1 + k] == 3) && (place[i1 - k - 1, j1 + k + 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                            }
                            else if (place[i1, j1] == 5 && move == false)
                            {
                                if (direct != 1)
                                {
                                    while ((place[i1 + k, j1 + k] == 1 || place[i1 + k, j1 + k] == 4 || place[i1 + k, j1 + k] == 2) && c != 2)
                                    {
                                        if (place[i1 + k, j1 + k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 + k, j1 + k] == 4 || place[i1 + k, j1 + k] == 2))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 + k, j1 + k] == 4 || place[i1 + k, j1 + k] == 2) && (place[i1 + k + 1, j1 + k + 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                                if (direct != 3)
                                {
                                    while ((place[i1 - k, j1 - k] == 1 || place[i1 - k, j1 - k] == 4 || place[i1 - k, j1 - k] == 2) && c != 2)
                                    {
                                        if (place[i1 - k, j1 - k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 - k, j1 - k] == 4 || place[i1 - k, j1 - k] == 2))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 - k, j1 - k] == 4 || place[i1 - k, j1 - k] == 2) && (place[i1 - k - 1, j1 - k - 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                                if (direct != 2)
                                {
                                    while ((place[i1 + k, j1 - k] == 1 || place[i1 + k, j1 - k] == 4 || place[i1 + k, j1 - k] == 2) && c != 2)
                                    {
                                        if (place[i1 + k, j1 - k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 + k, j1 - k] == 4 || place[i1 + k, j1 - k] == 2))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 + k, j1 - k] == 4 || place[i1 + k, j1 - k] == 2) && (place[i1 + k + 1, j1 - k - 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                                if (direct != 4)
                                {
                                    while ((place[i1 - k, j1 + k] == 1 || place[i1 - k, j1 + k] == 4 || place[i1 - k, j1 + k] == 2) && c != 2)
                                    {
                                        if (place[i1 - k, j1 + k] == 1 && d != 0)
                                        {
                                            c = 0;
                                        }
                                        if ((place[i1 - k, j1 + k] == 4 || place[i1 - k, j1 + k] == 2))
                                        {
                                            c++; d++;
                                        }
                                        if ((place[i1 - k, j1 + k] == 4 || place[i1 - k, j1 + k] == 2) && (place[i1 - k - 1, j1 + k + 1] == 1) && c != 2)
                                            positive = true;
                                        k++;
                                    }
                                    variables();
                                }
                            }

                            //подсчет есть ли ходы если нет то проигрывает тот чей ход
                            if ( move==true && (place[i1, j1] == 4 || place[i1, j1] == 2))
                            {
                               if ((place[i1 - 1, j1 + 1] != 1) && (place[i1 - 1, j1 - 1] != 1))
                                {
                                    while (place[i1 + k, j1 + k] == 1 && place[i1, j1] == 4) 
                                    {
                                        k++;hod_a++;
                                    }
                                    k = 1;
                                    while (place[i1 - k, j1 + k] == 1 && place[i1, j1] == 4)
                                    {
                                        k++; hod_a++;
                                    }
                                    k = 1;
                                    while (place[i1 + k, j1 - k] == 1 && place[i1, j1] == 4)
                                    {
                                        k++; hod_a++;
                                    }
                                    k = 1;
                                    while (place[i1 - k, j1 - k] == 1 && place[i1, j1] == 4)
                                    {
                                        k++; hod_a++;
                                    }
                                    k = 1;
                                }
                               if (((place[i1 - 1, j1 + 1] == 1) || (place[i1 - 1, j1 - 1] == 1)))
                                    hod_a++;
                               if (positive == true)
                                    hod_a++;
                            }

                            if ( move==false && (place[i1, j1] == 3 || place[i1, j1] == 5))
                            {
                                if ((place[i1 + 1, j1 - 1] != 1) && (place[i1 + 1, j1 + 1] != 1) )
                                {
                                    
                                    while (place[i1 + k, j1 + k] == 1 && place[i1, j1] == 5)
                                    {
                                        k++;hod_b++;
                                    }
                                    k = 1;
                                    while (place[i1 - k, j1 + k] == 1 && place[i1, j1] == 5)
                                    {
                                        k++; hod_b++;
                                    }
                                    k = 1;
                                    while (place[i1 + k, j1 - k] == 1 && place[i1, j1] == 5)
                                    {
                                        k++; hod_b++;
                                    }
                                    k = 1;
                                    while (place[i1 - k, j1 - k] == 1 && place[i1, j1] == 5)
                                    {
                                        k++; hod_b++;
                                    }
                                    k = 1;
                                    
                                }
                                if ((place[i1 + 1, j1 - 1] == 1) || (place[i1 + 1, j1 + 1] == 1))
                                    hod_b++;
                                if (positive == true)
                                    hod_b++;
                            }
                        }
                    }
                    if (hod_a == 0  && move == true)
                    {
                        MessageBox.Show("Черные выиграли", "Игра окончена");
                        restart();
                    }
                    else
                        hod_a = 0;
                    if (hod_b == 0 && move == false)
                    {
                        MessageBox.Show("Белые выиграли", "Игра окончена");
                        restart();
                    }
                    else
                        hod_b = 0;
                }
            }
        }

        private void b4b_Click_1(object sender, EventArgs e) => ClickEventHandler(sender);
        private void a3a_Click_1(object sender, EventArgs e) => ClickEventHandler(sender);
        private void c3c_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void d4d_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void a1a_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void b2b_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void c1c_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void d2d_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void e3e_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void e1e_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void f2f_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void g3g_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void g1g_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void h2h_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void f4f_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void h4h_Click(object sender, EventArgs e) => ClickEventHandler(sender);

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int butesRec = client.Receive(msg);
            string data="", str="";
            data = Encoding.ASCII.GetString(msg, 0, butesRec);

            int kb = 0, kborder = data.Length;
            int kk = 0;
            for (i1 = 11; i1 < m - 11; i1++)
            {
                for (j1 = 11; j1 < m - 11; j1++)
                {
                    if ((i1 + j1) % 2 != 0 && kb < kborder && kk == 0)
                    {
                        str = Convert.ToString(data[kb]);
                        place[i1, j1] = Convert.ToInt32(str);
                        kb++;
                        if (place[i1, j1] == 1)
                        {
                            btns[i1, j1].BackgroundImage = null;
                            btns[i1, j1].Text = null;
                        }
                        else if (place[i1, j1] == 2)
                        {
                            btns[i1, j1].BackgroundImage = button2.BackgroundImage;
                            btns[i1, j1].Text = null;
                        }
                        else if (place[i1, j1] == 3)
                        {
                            btns[i1, j1].BackgroundImage = button4.BackgroundImage;
                            btns[i1, j1].Text = null;
                        }
                        else if (place[i1, j1] == 4)
                        {
                            btns[i1, j1].BackgroundImage = button2.BackgroundImage;
                            btns[i1, j1].Text = "D";
                        }
                        else if (place[i1, j1] == 5)
                        {
                            btns[i1, j1].BackgroundImage = button4.BackgroundImage;
                            btns[i1, j1].Text = "D";
                        }
                    }
                    if (kb == 32)
                    {
                        kk++;
                        if (Convert.ToString(data[32]) == "T")
                            move = true;
                        else if (Convert.ToString(data[32]) == "F")
                            move = false;
                    }
                }
            }
        }
        
        private void новаяИграToolStripMenuItem_Click(object sender, EventArgs e)
        {
            client = socket.Accept();
        }

        private void g5g_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void e5e_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void c5c_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void a7a_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void e7e_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void g7g_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void b8b_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void d8d_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void f8f_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void h8h_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void a5a_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void h6h_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void b6b_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void d6d_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void f6f_Click(object sender, EventArgs e) => ClickEventHandler(sender);
        private void c7c_Click(object sender, EventArgs e) => ClickEventHandler(sender);
    }
}
