using LobotJR.Client;
using LobotJR.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobotJR.Modules.TwitchPlays
{
    public class TwitchPlaysModule
    {
        public static void Run(IrcClient irc)
        {
            irc.JoinRoom("lobosjr");

            Process[] p = Process.GetProcessesByName("DARKSOULS");
            IntPtr h = (IntPtr)0;
            if (p.Length > 0)
            {
                h = p[0].MainWindowHandle;
                NativeMethods.SetForegroundWindowSafe(h);
            }

            while (irc.connected)
            {
                // message[0] has username, message[1] has message
                string[] message = irc.ReadMessage();

                if (message.Length > 1)
                {
                    if (message[0] != null && message[1] != null)
                    {
                        string[] first = message[1].Split(' ');
                        string sender = message[0];
                        char[] keys = { };

                        const int MOVE_AMOUNT = 1000;
                        switch (first[0].ToLower())
                        {
                            // M preceds movement. MF = Move Forward, MB = Move Backwards, etc.
                            case "mf":
                                {
                                    NativeMethods.SendFor('W', MOVE_AMOUNT);
                                    Console.WriteLine("MF - Move Forward");
                                }
                                break;

                            case "mb":
                                {
                                    NativeMethods.SendFor('S', MOVE_AMOUNT);
                                    Console.WriteLine("MB - Move Back");
                                }
                                break;

                            case "ml":
                                {
                                    NativeMethods.SendFor('A', MOVE_AMOUNT);
                                    Console.WriteLine("ML - Move Left");
                                }
                                break;

                            case "mr":
                                {
                                    NativeMethods.SendFor('D', MOVE_AMOUNT);
                                    Console.WriteLine("MR - Move Right");
                                }
                                break;

                            // Camera Up, Down, Left, Right
                            case "cu":
                                {
                                    NativeMethods.SendFor('I', MOVE_AMOUNT);
                                    Console.WriteLine("CU - Camera Up");
                                }
                                break;

                            case "cd":
                                {
                                    NativeMethods.SendFor('K', MOVE_AMOUNT);
                                    Console.WriteLine("CD - Camera Down");
                                }
                                break;

                            case "cl":
                                {
                                    NativeMethods.SendFor('J', MOVE_AMOUNT);
                                }
                                break;

                            case "cr":
                                {
                                    NativeMethods.SendFor('L', MOVE_AMOUNT);
                                }
                                break;

                            // Lock on/off
                            case "l":
                                {
                                    NativeMethods.SendFor('O', 100);
                                }
                                break;

                            // Use item
                            case "u":
                                {
                                    NativeMethods.SendFor('E', 100);
                                }
                                break;
                            // 2h toggle
                            case "y":
                                {
                                    NativeMethods.SendFor(56, 100);
                                }
                                break;
                            // Attacks
                            case "r1":
                                {
                                    NativeMethods.SendFor('H', 100);
                                }
                                break;

                            case "r2":
                                {
                                    NativeMethods.SendFor('U', 100);
                                }
                                break;

                            case "l1":
                                {
                                    NativeMethods.SendFor(42, 100);
                                }
                                break;

                            case "l2":
                                {
                                    NativeMethods.SendFor(15, 100);
                                }
                                break;
                            // Rolling directions
                            case "rl":
                                {
                                    keys = new char[] { 'A', ' ' };
                                    NativeMethods.SendFor(keys, 100);
                                }
                                break;

                            case "rr":
                                {
                                    keys = new char[] { 'D', ' ' };
                                    NativeMethods.SendFor(keys, 100);
                                }
                                break;

                            case "rf":
                                {
                                    keys = new char[] { 'W', ' ' };
                                    NativeMethods.SendFor(keys, 100);
                                }
                                break;

                            case "rb":
                                {
                                    keys = new char[] { 'S', ' ' };
                                    NativeMethods.SendFor(keys, 100);
                                }
                                break;

                            case "x":
                                {
                                    NativeMethods.SendFor('Q', 100);
                                    NativeMethods.SendFor(28, 100);
                                }
                                break;
                            // switch LH weap
                            case "dl":
                                {
                                    NativeMethods.SendFor('C', 100);
                                }
                                break;

                            case "dr":
                                {
                                    NativeMethods.SendFor('V', 100);
                                }
                                break;

                            case "du":
                                {
                                    NativeMethods.SendFor('R', 100);
                                }
                                break;

                            case "dd":
                                {
                                    NativeMethods.SendFor('F', 100);
                                }
                                break;

                            //case "just subscribed":
                            //    {
                            //        NativeMethods.SendFor('G', 500);

                            //        NativeMethods.SendFor(13, 100);
                            //    } break;

                            default: break;



                        }
                    }
                }
            }
            Console.WriteLine("Connection terminated.");
        }
    }
}
