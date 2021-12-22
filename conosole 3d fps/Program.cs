using System;
using System.Collections.Generic;
using System.Linq;

namespace conosole_3d_fps
{
    internal class Program
    {
        private const int WidthScreen = 240;
        private const int HeightScreen = 120;

        private const int MapWidth = 40;
        private const int MapScreen = 30;

        private const double Fov = Math.PI / 3;
        private const double Depth = 24;

        private static double _playerX = 3;
        private static double _playerY = 3;
        private static double _playerA = 0;

        private static string _map = "";
        private static readonly char[] Screen = new char[WidthScreen*HeightScreen];

        static void Main(string[] args)
        {
            Console.SetWindowSize(WidthScreen, HeightScreen);
            Console.SetBufferSize(WidthScreen, HeightScreen);
            Console.CursorVisible = false;


            _map += "########################################";
            _map += "#............##........................#";
            _map += "#.............##.......................#";
            _map += "#......................................#";
            _map += "#...............#######................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#.........#####........................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "#......................................#";
            _map += "########################################";

            DateTime dateTimeFrom = DateTime.Now;

            while (true)
            {
                DateTime dateTimeTo = DateTime.Now;
                double elapsedTime = (dateTimeTo - dateTimeFrom).TotalSeconds;
                dateTimeFrom = DateTime.Now;

                if(Console.KeyAvailable)
                {
                    ConsoleKey consoleKey = Console.ReadKey(true).Key;

                    switch (consoleKey)
                    {
                        case ConsoleKey.A:
                            _playerA += 2 * elapsedTime; break;
                        case ConsoleKey.D:
                            _playerA -= 2 * elapsedTime; break;
                        case ConsoleKey.W:
                            _playerX += Math.Sin(_playerA) * 10 * elapsedTime;
                            _playerY += Math.Cos(_playerA) * 10 * elapsedTime;

                            if(_map[(int) _playerY * MapWidth + (int) _playerX] == '#')
                            {
                                _playerX -= Math.Sin(_playerA) * 10 * elapsedTime;
                                _playerY -= Math.Cos(_playerA) * 10 * elapsedTime;
                            }

                            break;
                        case ConsoleKey.S:
                            _playerX -= Math.Sin(_playerA) * 10 * elapsedTime;
                            _playerY -= Math.Cos(_playerA) * 10 * elapsedTime;

                            if (_map[(int)_playerY * MapWidth + (int)_playerX] == '#')
                            {
                                _playerX += Math.Sin(_playerA) * 10 * elapsedTime;
                                _playerY += Math.Cos(_playerA) * 10 * elapsedTime;
                            }
                            break;
                    }
                }

                for (int x = 0; x < WidthScreen; x++)
                {
                    double rayAngle = _playerA + Fov / 2 - x * Fov / WidthScreen;

                    double rayX = Math.Sin(rayAngle);
                    double rayY = Math.Cos(rayAngle);

                    double distanceToWall = 0;
                    bool hitWall = false;
                    bool isBounds = false;

                    while (!hitWall && distanceToWall < Depth)
                    {
                        distanceToWall += 0.1;

                        int testX = (int)(_playerX + rayX * distanceToWall);
                        int testY = (int)(_playerY + rayY * distanceToWall);

                        if(testX < 0 || testX >= Depth + _playerX || testY < 0 || testY >= Depth + _playerY)
                        {
                            hitWall = true;
                            distanceToWall = Depth;
                        }
                        else
                        {
                            char testCell = _map[testY * MapWidth + testX];

                            if (testCell == '#')
                            {
                                hitWall = true;

                                var boundsVectorList = new List<(double module, double cos)>();

                                for (int tx = 0; tx < 2; tx++)
                                {
                                    for (int ty = 0; ty < 2; ty++)
                                    {
                                        double vx = testX + tx - _playerX;
                                        double vy = testY + ty - _playerY;

                                        double vectorModule = Math.Sqrt(vx * vx + vy * vy);
                                        double cosAngle = rayX * vx / vectorModule + rayY * vy / vectorModule;

                                        boundsVectorList.Add((vectorModule, cosAngle));
                                    }
                                }

                                boundsVectorList = boundsVectorList.OrderBy(v => v.module).ToList();

                                double boundAngle = 0.03;
                                if(Math.Acos(boundsVectorList[0].cos) < boundAngle ||
                                    Math.Acos(boundsVectorList[1].cos) < boundAngle)
                                {
                                    isBounds = true;
                                }
                            }
                        }
                    }

                    int celling = (int)(HeightScreen / 2d - HeightScreen / distanceToWall);
                    int floor = HeightScreen - celling;

                    char wallShade;

                    if (isBounds)
                        wallShade = '|';
                    else if (distanceToWall <= Depth / 4d)
                        wallShade = '\u2588';
                    else if (distanceToWall <= Depth / 3d)
                        wallShade = '\u2593';
                    else if (distanceToWall <= Depth / 2d)
                        wallShade = '\u2592';
                    else if (distanceToWall <= Depth)
                        wallShade = '\u2591';
                    else
                        wallShade = ' ';

                    for (int y = 0; y < HeightScreen; y++)
                    {
                        if (y <= celling)
                        {
                            Screen[y * WidthScreen + x] = ' ';
                        }
                        else if (y > celling && y <= floor)
                        {
                            Screen[y * WidthScreen + x] = wallShade;
                        }
                        else
                        {
                            char floorShade;

                            double b = 1 - (y - HeightScreen / 2d) / (HeightScreen / 2d);

                            if (b < 0.25)
                                floorShade = '#';
                            else if (b < 0.5)
                                floorShade = 'x';
                            else if (b < 0.75)
                                floorShade = '-';
                            else if (b < 0.9)
                                floorShade = '.';
                            else
                                floorShade = ' ';

                            Screen[y * WidthScreen + x] = floorShade;
                        }
                    }
                }
                char[] stats = $"X: {_playerX}, Y: {_playerY}, A: {_playerA}, FPS: {(int)(1 / elapsedTime)}".ToCharArray();
                stats.CopyTo(Screen, 0);

                for (int x = 0; x < MapWidth; x++)
                {
                    for (int y = 0; y < MapScreen; y++)
                    {
                        Screen[(y + 1) * WidthScreen + x] = _map[y * MapWidth + x];
                    }
                }

                Screen[(int)(_playerY + 1) * WidthScreen + (int)_playerX] = 'P';


                Console.SetCursorPosition(0, 0);
                Console.Write(Screen);
            }
        }
    }
}
