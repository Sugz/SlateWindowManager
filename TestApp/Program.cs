using SlateWindowManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PositionManager positionManager = new PositionManager();
            string dir = @"D:\Travail\Code\3dsMax\Persos\SlateWindowManager";
            positionManager.Init(dir);
            positionManager.Start();

            Console.ReadLine();
        }
    }
}
