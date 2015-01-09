using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLab
{
    class GameController
    {
            Form1 f1;
            ClientWorker CW;

            public GameController(Form1 f)
            {
                f1 = f;
                CW = new ClientWorker(this);
                ships = new List<Ship>();
            }


            public bool turn;
            public int bombedShips = 0;

            // board size
            public int dimension = 10;

            public int[,] my = new int[dimension+2, dimension+2];
            public int[,] enemy = new int[dimension+2, dimension+2];

            public class Ship
            {
                public int X;
                public int Y;
                public int Len;
                public bool Vertical;
                public int bombed = 0;

                public Ship(int x, int y, int len, bool vertical)
                {
                    X = x;
                    Y = y;
                    Len = len;
                    Vertical = vertical;
                }


                // x and y - mouse position
                /*public bool Mine(int x, int y)
                {
                    if (Vertical)
                        return ((mash * X <= x) && (mash * (X + 1)>= x) && (mash * Y <= y) && (mash * (Y + Len) >= y));
                    else
                        return ((mash * X <= x) && (mash * (Len + X) >= x) && (mash * Y <= y) && (mash * (Y + 1) >= y));
                }*/

            }

            public void Send(string s)
            {
                CW.Send(s);
            }

            public List<Ship> ships;

            // Figure out what to do with info from server
            public void ProcessIncoming(string s)
            {
                string[] tokens = s.Split('*');
                switch (tokens[0])
                {
                    case "GAMEON":
                        {
                            f1.gameOn = true;
                            turn = Convert.ToBoolean(tokens[1]);
                            break;
                        }
                    case "BOMBED":
                        {
                            Ship sh = new Ship(Convert.ToInt32(tokens[1]), Convert.ToInt32(tokens[2]), Convert.ToInt32(tokens[3]), Convert.ToBoolean(tokens[4]));
                            if (sh.Vertical)
                            {
                                for (int i = -1; i <= sh.Len; i++)
                                {
                                    enemy[sh.X, sh.Y + i+1] = -2;
                                    enemy[sh.X + 2, sh.Y + i+1] = -2;
                                }

                                enemy[sh.X+1, sh.Y] = -2;
                                
                                enemy[sh.X+1, sh.Y + sh.Len+1] = -2;
                                
                            }
                            else
                            {
                                for (int i = -1; i <= sh.Len; i++)
                                {
                                    enemy[sh.X + i+1, sh.Y] = -2;                                    
                                    enemy[sh.X + i+1, sh.Y + 2] = -2;
                                }
                                
                                enemy[sh.X, sh.Y+1] = -2;                                
                                enemy[sh.X + sh.Len+1, sh.Y+1] = -2;
                                
                            }
                            break;
                        }
                    case "LOST":
                        {
                            //"Вы выиграли!"
                            my = new int[dimension, dimension];
                            enemy = new int[dimension, dimension];
                            ships.Clear();
                            bombedShips = 0;
                            return;
                        }
                    case "FIRE": // we are being fired at
                        {
                            int x = Convert.ToInt32(tokens[1]);
                            int y = Convert.ToInt32(tokens[2]);
                            if (my[x + 1, y + 1] >= 0)
                            {
                                CW.Send("HIT*" + x.ToString() + "*" + y.ToString());
                                ships[my[x + 1, y + 1]].bombed++;
                                if (ships[my[x + 1, y + 1]].bombed == my[x + 1, y + 1])
                                {
                                    CW.Send("BOMBED*" + x.ToString() + "*" + y.ToString() + "*" + ships[my[x + 1, y + 1]].Len + "*" + ships[my[x + 1, y + 1]].Vertical);
                                    bombedShips++;
                                    if (bombedShips == ships.Count)
                                    {
                                        System.Threading.Thread.Sleep(500);
                                        CW.Send("LOST");
                                    }
                                }
                            }

                            else
                            {
                                CW.Send("MISS*" + x.ToString() + "*" + y.ToString());
                                my[x+1, y+1] = -2;
                                turn = true;
                            }
                            break;
                        }
                    case "HIT": // info about how much time we got left for answering 
                        {
                            int x = Convert.ToInt32(tokens[1]);
                            int y = Convert.ToInt32(tokens[2]);
                            enemy[x+1, y+1] = -1;
                            break;
                        }
                    case "MISS":
                        {
                            int x = Convert.ToInt32(tokens[1]);
                            int y = Convert.ToInt32(tokens[2]);
                            enemy[x+1, y+1] = -2;
                            turn = false;
                            break;
                        }
                    case "GAMEOFF":
                        {
                            f1.gameOn = false;
                            break;
                        }
                }
                f1.RefreshAll();
                return;
            }
    }
}
