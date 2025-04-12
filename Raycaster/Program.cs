/*--------------------------------------------------------------
*		HTBLA-Leonding / Class: 1CHIF
*--------------------------------------------------------------
*              Stefan Brandmair
*--------------------------------------------------------------
* Description:
* A C# console raycaster!! Taken from http://lodev.org/cgtutor/raycasting.html
* and improved by enom 
* adjustments:
* -added more pixesels by adjusting the font size using my custom built consolehelper
* -in the 5 seconds it takes to start put it in fullscrene for best exsperiances
*--------------------------------------------------------------
*/


using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Xml.Linq;

public class ConsoleHelper
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetCurrentConsoleFontEx(IntPtr hConsoleOutput, bool bMaximumWindow, ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFontEx);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    private const int STD_OUTPUT_HANDLE = -11;
    private const int LF_FACESIZE = 32;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CONSOLE_FONT_INFO_EX
    {
        public uint cbSize;
        public uint nFont;
        public Coord dwFontSize;
        public int FontFamily;
        public int FontWeight;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
        public string FaceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Coord
    {
        public short X;
        public short Y;

        public Coord(short x, short y)
        {
            X = x;
            Y = y;
        }
    }

    public static void SetConsoleFont(string fontName, short size)
    {
        IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
        CONSOLE_FONT_INFO_EX info = new CONSOLE_FONT_INFO_EX();
        info.cbSize = (uint)Marshal.SizeOf(info);
        info.FaceName = fontName;
        info.dwFontSize = new Coord(0, size);
        info.FontFamily = 54;
        info.FontWeight = 400;

        SetCurrentConsoleFontEx(hnd, false, ref info);
    }
}


namespace Raycaster
{
    class Program
    {

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);
        
        private static void Maximize()
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
            Console.WindowHeight = 200;
            Console.WindowWidth = 900;
        }
        //I guess mipmaps would solve this problem
        //Or better interpolation
        static int[,] texture = new int[14, 14]{
            { 0,0,0,0, 6,14,14,14,14,6, 0,0,0,0},
            { 0,0, 6,14,14,14,14,14,14,14,14,6, 0,0},
            { 0,6, 14,14,14,14,14,14,14,14,14,14, 6,0},
            { 0,14, 14,14,14,14,14,14,14,14,14,14, 14,0},
            { 6,14, 14,14,0,0,14,14,0,0,14,14, 14,6},
            { 6,14, 14,14,0,0,14,14,0,0,14,14, 14,6},
            { 14,14, 14,14,14,14,14,14,14,14,14,14, 14,14},
            { 14,14, 14,14,14,14,14,14,14,14,14,14, 14,14},
            { 6,14, 14,0,0,14,14,14,14,0,0,14, 14,6},
            { 6,14, 14,14,0,0,14,14,0,0,14,14, 14,6},
            { 0,14, 14,14,14,0,0,0,0,14,14,14, 14,0},
            { 0,6, 14,14,14,14,14,14,14,14,14,14, 6,0},
            { 0,0, 6,14,14,14,14,14,14,14,14,6, 0,0},
            { 0,0,0,0, 6,14,14,14,14,6, 0,0,0,0},
        };
        static int[,] consoleWindowMemory;
        //It's a small world
        static int[,] map = {
  {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
  {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,0,0,3,0,0,0,1},
  {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,0,0,0,0,5,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
  {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1}
};


        static double posX = 22, posY = 12;  //x and y start position
        static double dirX = -1, dirY = 0; //initial direction vector, where is he looking
        static double planeX = 0, planeY = 0.66; //the 2d raycaster version of camera plane,

        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            ConsoleHelper.SetConsoleFont("Consolas", 1); // Try to set smallest font

            Maximize();
            Console.CursorVisible = false;
            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;
            //y,x coords
            consoleWindowMemory = new int[Console.BufferHeight - 1, Console.BufferWidth];
            using (StreamWriter stream = new StreamWriter(Console.OpenStandardOutput()))
            {
                stream.AutoFlush = false;
                //Main loop
                while (true)
                {
                    for (double x = 0; x < consoleWindowMemory.GetLength(1); x++)
                    {
                        //calculate ray position and direction
                        double cameraX = 2 * x / consoleWindowMemory.GetLength(1) - 1; // x coord in cam space
                        double rayPosX = posX;
                        double rayPosY = posY;
                        double rayDirX = dirX + planeX * cameraX;
                        double rayDirY = dirY + planeY * cameraX;

                        //which box of the map we're in
                        double mapX = (int)rayPosX;
                        double mapY = (int)rayPosY;

                        //length of ray from current position to next x or y-side
                        double sideDistX;
                        double sideDistY;

                        //length of ray from one x or y-side to next x or y-side
                        double deltaDistX = Math.Sqrt(1 + (rayDirY * rayDirY) / (rayDirX * rayDirX));
                        double deltaDistY = Math.Sqrt(1 + (rayDirX * rayDirX) / (rayDirY * rayDirY));
                        double perpWallDist;

                        //what direction to step in x or y-direction (either +1 or -1)
                        double stepX;
                        double stepY;

                        double hit = 0; //was there a wall hit?
                        double side = 0; //was a NS or a EW wall hit?

                        //calculate step and initial sideDist
                        if (rayDirX < 0)
                        {
                            stepX = -1;
                            sideDistX = (rayPosX - mapX) * deltaDistX;
                        }
                        else
                        {
                            stepX = 1;
                            sideDistX = (mapX + 1.0 - rayPosX) * deltaDistX;
                        }
                        if (rayDirY < 0)
                        {
                            stepY = -1;
                            sideDistY = (rayPosY - mapY) * deltaDistY;
                        }
                        else
                        {
                            stepY = 1;
                            sideDistY = (mapY + 1.0 - rayPosY) * deltaDistY;
                        }

                        //perform DDA
                        while (hit == 0)
                        {
                            //jump to next map square, OR in x-direction, OR in y-direction
                            if (sideDistX < sideDistY)
                            {
                                sideDistX += deltaDistX;
                                mapX += stepX;
                                side = 0;
                            }
                            else
                            {
                                sideDistY += deltaDistY;
                                mapY += stepY;
                                side = 1;
                            }
                            //Check if ray has hit a wall
                            if (map[(int)mapX, (int)mapY] > 0) hit = 1;
                        }

                        //Calculate distance projected on camera direction (oblique distance will give fisheye effect!)
                        if (side == 0) perpWallDist = (mapX - rayPosX + (1 - stepX) / 2) / rayDirX;
                        else perpWallDist = (mapY - rayPosY + (1 - stepY) / 2) / rayDirY;

                        int h = consoleWindowMemory.GetLength(0);
                        int lineHeight;

                        if (perpWallDist > 0)
                        {
                            lineHeight = (int)(h / perpWallDist);
                        }
                        else
                        {
                            lineHeight = consoleWindowMemory.GetLength(0);
                        }
                        //Calculate height of line to draw on screen


                        //calculate lowest and highest pixel to fill in current stripe
                        int drawStart = -lineHeight / 2 + h / 2;
                        if (drawStart < 0) drawStart = 0;
                        int drawEnd = lineHeight / 2 + h / 2;
                        if (drawEnd >= h) drawEnd = h - 1;


                        //calculate value of wallX
                        double wallX; //where exactly the wall was hit
                        if (side == 0) wallX = rayPosY + perpWallDist * rayDirY;
                        else wallX = rayPosX + perpWallDist * rayDirX;
                        wallX -= (int)(wallX);

                        //x coordinate on the texture
                        int texX = (int)(wallX * (double)(texture.GetLength(1)));
                        if (side == 0 && rayDirX > 0) texX = texture.GetLength(1) - texX - 1;
                        if (side == 1 && rayDirY < 0) texX = texture.GetLength(1) - texX - 1;

                        #region Console color
                        //choose wall color
                        ConsoleColor color;
                        switch (map[(int)mapX, (int)mapY])
                        {
                            case 1:
                                //give x and y sides different brightness
                                if (side == 1)
                                {
                                    color = ConsoleColor.DarkRed;
                                }
                                else
                                {
                                    color = ConsoleColor.Red; //red
                                }
                                break;
                            case 2:
                                //give x and y sides different brightness
                                if (side == 1)
                                {
                                    color = ConsoleColor.DarkGreen;
                                }
                                else
                                {
                                    color = ConsoleColor.Green;//green
                                }
                                break;
                            case 3:
                                //give x and y sides different brightness
                                if (side == 1)
                                {
                                    color = ConsoleColor.DarkBlue;
                                }
                                else
                                {
                                    color = ConsoleColor.Blue;//blue
                                }
                                break;
                            case 4:
                                //give x and y sides different brightness
                                if (side == 1)
                                {
                                    color = ConsoleColor.DarkMagenta;
                                }
                                else
                                {
                                    color = ConsoleColor.Magenta;//white
                                }
                                break;
                            default:
                                //give x and y sides different brightness
                                if (side == 1)
                                {
                                    color = ConsoleColor.DarkYellow;
                                }
                                else
                                {
                                    color = ConsoleColor.Yellow;//yellow
                                }
                                break;
                        }
                        #endregion
                        //draw the pixels of the stripe as a vertical line
                        verLine((int)x, drawStart, drawEnd, color, texX);

                    }
                    //Raycasting done
                    //Keyboard input

                    //speed modifiers
                    double moveSpeed = 0.5; //the constant value is in squares/second
                    double rotSpeed = 0.1; //the constant value is in radians/second

                    //Draw BEFORE reading the NEXT key
                    drawMemoryToConsole(stream);

                    //Clear previous keys
                    while (Console.KeyAvailable)
                    {
                        Console.ReadKey(true);
                    }
                    //Read key
                    ConsoleKey currKey = Console.ReadKey(true).Key;
                    //move forward if no wall in front of you

                    double xMove = dirX * moveSpeed;
                    double yMove = dirY * moveSpeed;
                    if ((int)(posX + xMove) < map.GetLength(0) && posX + xMove >= 0 && posX >= 0 && posX < map.GetLength(0) &&
                        (int)(posY + yMove) < map.GetLength(1) && posY + yMove >= 0 && posY >= 0 && posY < map.GetLength(1) &&
                        (int)(posX - xMove) < map.GetLength(0) && posX - xMove >= 0 &&
                        (int)(posY - yMove) < map.GetLength(1) && posY - yMove >= 0)
                    {
                        if (currKey == ConsoleKey.UpArrow)
                        {
                            if (map[(int)(posX + xMove), (int)(posY)] == 0) posX += xMove;
                            if (map[(int)(posX), (int)(posY + yMove)] == 0) posY += yMove;
                        }
                        //move backwards if no wall behind you
                        if (currKey == ConsoleKey.DownArrow)
                        {
                            if (map[(int)(posX - xMove), (int)(posY)] == 0) posX -= xMove;
                            if (map[(int)(posX), (int)(posY - yMove)] == 0) posY -= yMove;
                        }
                        //rotate to the right
                        if (currKey == ConsoleKey.RightArrow)
                        {
                            //both camera direction and camera plane must be rotated
                            double oldDirX = dirX;
                            dirX = dirX * Math.Cos(-rotSpeed) - dirY * Math.Sin(-rotSpeed);
                            dirY = oldDirX * Math.Sin(-rotSpeed) + dirY * Math.Cos(-rotSpeed);
                            double oldPlaneX = planeX;
                            planeX = planeX * Math.Cos(-rotSpeed) - planeY * Math.Sin(-rotSpeed);
                            planeY = oldPlaneX * Math.Sin(-rotSpeed) + planeY * Math.Cos(-rotSpeed);
                        }
                        //rotate to the left
                        if (currKey == ConsoleKey.LeftArrow)
                        {
                            //both camera direction and camera plane must be rotated
                            double oldDirX = dirX;
                            dirX = dirX * Math.Cos(rotSpeed) - dirY * Math.Sin(rotSpeed);
                            dirY = oldDirX * Math.Sin(rotSpeed) + dirY * Math.Cos(rotSpeed);
                            double oldPlaneX = planeX;
                            planeX = planeX * Math.Cos(rotSpeed) - planeY * Math.Sin(rotSpeed);
                            planeY = oldPlaneX * Math.Sin(rotSpeed) + planeY * Math.Cos(rotSpeed);
                        }
                    }
                }
            }
        }

        //https://en.wikipedia.org/wiki/Geometric_Shapes#Compact_chart

        static void verLine(int x, int top, int bottom, ConsoleColor color, int textureX)
        {
            int lineHeight = bottom - top;
            double multiplier = texture.GetLength(0) / (double)lineHeight;
            for (int i = 0; i < top; i++)
            {
                consoleWindowMemory[i, x] = (int)ConsoleColor.Black;
            }
            if (color == ConsoleColor.Yellow || color == ConsoleColor.DarkYellow)
            {
                for (int i = top; i < bottom; i++)
                {
                    //TODO texture Y coord
                    consoleWindowMemory[i, x] = texture[(int)((i - top) * multiplier), textureX];
                }
            }
            else
            {
                for (int i = top; i < bottom; i++)
                {
                    consoleWindowMemory[i, x] = (int)color;
                }
            }
            for (int i = bottom; i < consoleWindowMemory.GetLength(0); i++)
            {
                consoleWindowMemory[i, x] = (int)ConsoleColor.Black;
            }
        }

        static void drawMemoryToConsole(StreamWriter stream)
        {

            Console.SetCursorPosition(0, 0);
            int currC = consoleWindowMemory[0, 0];
            Console.BackgroundColor = (ConsoleColor)consoleWindowMemory[0, 0];
            for (int i = 0; i < consoleWindowMemory.GetLength(0); i++)
            {
                for (int j = 0; j < consoleWindowMemory.GetLength(1); j++)
                {
                    //On color change, flush the stream (Write everything to the console)
                    if (currC != consoleWindowMemory[i, j])
                    {
                        stream.Flush();
                        currC = consoleWindowMemory[i, j];
                        //Without frequent flushing, the stream gets messed up?
                        //Stream still getting messed up?
                        Console.BackgroundColor = (ConsoleColor)consoleWindowMemory[i, j];

                        //Borders
                        //Write to the stream
                        stream.Write('|');
                    }
                    else
                    {
                        //Write to the stream
                        stream.Write(' ');
                    }
                }
            }

            stream.Flush();
        }
    }
}
