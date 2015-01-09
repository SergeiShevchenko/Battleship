using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.NetworkInformation;

namespace GameLab
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GC = new GameController(this);
        }

        int mashtab;
        GameController GC;
        

        public void RefreshAll()
        {
            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
        }



        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            mashtab = pictureBox1.Width / 10;
            e.Graphics.Clear(Color.White);

            for (int i = 0; i < GC.dimension; i++)
            {
                for (int j = 0; j < GC.dimension; j++)
                {
                    if (GC.my[i, j] == -1)
                    {
                        e.Graphics.FillRectangle(Brushes.Red, i * mashtab, j * mashtab, mashtab, mashtab);
                    }
                    if (GC.my[i, j] == -2)
                    {
                        e.Graphics.FillRectangle(Brushes.Black, i * mashtab, j * mashtab, mashtab, mashtab);
                    }
                }
            }

            for (int i = 0; i <= 10; i++)
            {
                e.Graphics.DrawLine(Pens.Black, i * mashtab, 0, i * mashtab, pictureBox1.Width);
                e.Graphics.DrawLine(Pens.Black, 0, i * mashtab, pictureBox1.Height, i * mashtab);
            }

            TurnCheck();
        }

        void TurnCheck()
        {
            if (GC.turn)
                label5.Text = "Your turn";
            else
                label5.Text = "Enemy's turn";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (GC.ships.Find(sh => sh.Len == 4) == null)
            {
                GC.ships.Add(new GameController.Ship(0, 0, 4, true));
                currentShip = GC.ships.Count - 1;
                button1.Enabled = false;
            }
            pictureBox1.Invalidate();
        }

        bool gameStarted = false;

        int currentShip = -1;

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!gameStarted)
            {
                if (currentShip == -1)
                {
                    for (int i = 0; i < ships.Count; i++)
                    {
                        if (ships[i].Mine(e.X, e.Y))
                        {
                            currentShip = i;
                            return;
                        }
                    }
                }
                else
                {
                    if (e.Button == MouseButtons.Left)
                        currentShip = -1;
                    else
                        ships[currentShip].Vertical = !ships[currentShip].Vertical;
                }
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentShip != -1)
            {
                ships[currentShip].X = e.X / mashtab;
                ships[currentShip].Y = e.Y / mashtab;
                pictureBox1.Invalidate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (ships.FindAll(sh => sh.Len == 3).Count<2)
            {
                ships.Add(new Ship(0, 0, 3, true, mashtab));
                currentShip = ships.Count - 1;
                if (ships.FindAll(sh => sh.Len == 3).Count == 2)
                    button2.Enabled = false;
            }

            pictureBox1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (ships.FindAll(sh => sh.Len == 2).Count < 3)
            {
                ships.Add(new Ship(0, 0, 2, true, mashtab));
                currentShip = ships.Count - 1;
                if (ships.FindAll(sh => sh.Len == 2).Count == 3)
                    button3.Enabled = false;
            }
            pictureBox1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (ships.FindAll(sh => sh.Len == 1).Count < 4)
            {
                ships.Add(new Ship(0, 0, 1, true, mashtab));
                currentShip = ships.Count - 1;
                if (ships.FindAll(sh => sh.Len == 1).Count == 4)
                    button4.Enabled = false;
            }
            pictureBox1.Invalidate();
        }


        bool CheckAndProcess()
        {
            if (ships.Count != 10)
            {
                MessageBox.Show("Вы не расставили всех кораблей!");
                return false;
            }
            my = new int[dimension, dimension];
            foreach (Ship s in ships)
            {
                if (s.Vertical)
                {
                    for (int i = s.Y; i < s.Y+s.Len; i++)
                    {
                        if (my[s.X, i] == 0)
                            my[s.X, i] = 1;
                        else
                        {
                            MessageBox.Show("Корабли наложились друг на друга. В частности, в точке " + s.X.ToString() + ";" + i.ToString());
                            return false;
                        }
                    }
                }
                else
                {
                    for (int i = s.X; i < s.X+s.Len; i++)
                    {
                        if (my[i, s.Y] == 0)
                            my[i, s.Y] = 1;
                        else
                        {
                            MessageBox.Show("Корабли наложились друг на друга. В частности, в точке " +i.ToString()+";"+s.Y.ToString());
                            return false;
                        }
                    }
                }
            }
            if (nw.client != null)
            {
                nw.Send("READY");
                return true;
            }
            else
                MessageBox.Show("Вы не подключились к серверу!");
            return false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (CheckAndProcess())
                button6.Enabled = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
            pictureBox2.Invalidate();
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button6.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (nw.client!=null) nw.client.Close();

            GC.work = false;

            foreach (Thread t in nw.threads)
                t.Abort();

            Application.Exit();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            GC.Connect(textBox1.Text);
            if (nw.client.Connected)
            {
                label4.Text = "Подключен";
            }
        }

        public bool gameOn = false;

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            if (gameOn)
            {
                mashtab = pictureBox1.Width / 10;
                e.Graphics.Clear(Color.White);

                for (int i = 0; i < GC.dimension; i++)
                {
                    for (int j = 0; j < GC.dimension; j++)
                    {
                        if (GC.enemy[i, j] == -1)
                        {
                            e.Graphics.FillRectangle(Brushes.Red, i * mashtab, j * mashtab, mashtab, mashtab);
                        }
                        if (GC.enemy[i, j] == -2)
                        {
                            e.Graphics.FillRectangle(Brushes.Black, i * mashtab, j * mashtab, mashtab, mashtab);
                        }
                    }
                }

                for (int i = 0; i <= 10; i++)
                {
                    e.Graphics.DrawLine(Pens.Black, i * mashtab, 0, i * mashtab, pictureBox1.Width);
                    e.Graphics.DrawLine(Pens.Black, 0, i * mashtab, pictureBox1.Height, i * mashtab);
                }
            }
        }

        private void pictureBox2_MouseClick(object sender, MouseEventArgs e)
        {
            if ((gameOn)&&GC.turn)
            {
                GC.Send("FIRE*" + (e.X / mashtab).ToString() + "*" + (e.Y / mashtab).ToString());
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            ships.Add(new Ship(r.Next(9), r.Next(9), 4, Convert.ToBoolean(r.Next(1)), mashtab));

            ships.Add(new Ship(r.Next(9), r.Next(9), 3, Convert.ToBoolean(r.Next(1)), mashtab));
            ships.Add(new Ship(r.Next(9), r.Next(9), 3, Convert.ToBoolean(r.Next(1)), mashtab));

            ships.Add(new Ship(r.Next(9), r.Next(9), 2, Convert.ToBoolean(r.Next(1)), mashtab));
            ships.Add(new Ship(r.Next(9), r.Next(9), 2, Convert.ToBoolean(r.Next(1)), mashtab));
            ships.Add(new Ship(r.Next(9), r.Next(9), 2, Convert.ToBoolean(r.Next(1)), mashtab));

            ships.Add(new Ship(r.Next(9), r.Next(9), 1, Convert.ToBoolean(r.Next(1)), mashtab));
            ships.Add(new Ship(r.Next(9), r.Next(9), 1, Convert.ToBoolean(r.Next(1)), mashtab));
            ships.Add(new Ship(r.Next(9), r.Next(9), 1, Convert.ToBoolean(r.Next(1)), mashtab));
            ships.Add(new Ship(r.Next(9), r.Next(9), 1, Convert.ToBoolean(r.Next(1)), mashtab));
            pictureBox1.Invalidate();
        }







    }
}
