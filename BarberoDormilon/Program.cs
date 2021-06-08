using System;
using System.Threading;

namespace BarberoDormilon
{
    class Program
    {
        static int nSillas = 3;
       // static Semaphore sillaBarbero = new Semaphore(1, 1);
        static Semaphore sillasBarberia = new Semaphore(nSillas, nSillas);
        static object barberoListo = new object();
        static int sillasLibres = nSillas;
        static readonly object sitsLock = new object();
        static readonly object writtingLock = new object();
        static bool toContinue = true;
        static bool barberState = false;
        static bool barberSitState = false;
        static int waitTime = 0;

        static EventWaitHandle sillaBarbero = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            DrawEverything();

            new Thread(new ThreadStart(FunctionBarberoDormilon)).Start();
            do
            {
                DrawEverything();

                switch (Console.ReadKey().KeyChar)
                {
                    case 'q':
                        toContinue = false;
                        break;
                    case 'c':
                        new Thread(new ThreadStart(FunctionCliente)).Start();
                        break;
                }
            } while (toContinue);

            Console.ForegroundColor = ConsoleColor.Red;
            
        }



        static void FunctionBarberoDormilon()
        {
            Random rng = new Random();
            bool writeState = true;// Variable para no escribir siempre que el barbero este dormido
            while (toContinue)
            {
                if (sillasLibres >= 3)
                {
                    barberState = false;
                    barberSitState = false;
                    if (writeState)
                    {
                        DrawEverything();
                        writeState = false;
                    }
                }
                else
                {
                    writeState = true;
                    barberState = true;
                    barberSitState = true;

                    waitTime = rng.Next(1, 4);
                    while (waitTime > 0)
                    {
                        
                        //DrawTimeLeft(position, waitTime);
                        Thread.Sleep(1000);
                        waitTime--;
                        DrawBarberTime();
                    }
                    // DrawTimeLeft(position, waitTime);

                    // DrawMeals(position, meals);
                    barberSitState = false;
                    DrawEverything();
                   
                    sillaBarbero.Set();
                }
            }
        }

        static void FunctionCliente()
        {
            if (sillasLibres > 0)
            {
                lock (sitsLock)
                {
                    sillasLibres--;
                    sillasBarberia.WaitOne();
                    DrawBarberShopSits();
                }
                //Esperando
                lock (barberoListo)
                {
                    sillasLibres++;
                    sillasBarberia.Release();
                    DrawEverything();
                    sillaBarbero.WaitOne();
                    if(sillasLibres > 3)
                    {
                        Console.Write("Error");
                    }
                }

            }
            else
            {
                DrawMissingSits();
                //Escribir texto de salida de la barberia 
                //por falta de sillas
            }

        }

        static void DrawEverything()
        {
            Console.Clear();
            DrawBarberShopSits();
            DrawBarberSit();
            DrawBarberState();
            DrawInitialInterface();
        }
        static void DrawInitialInterface()
        {
            lock (writtingLock)
            {
                Console.SetCursorPosition(30, 0);
                Console.Write("Barbero Dormilon");

                Console.SetCursorPosition(10, 8);
                Console.Write("Acción: ");
            }
        }

        static void DrawBarberShopSits()
        {
            lock (writtingLock)
            {
                // 0 1 2 3
                // 0 - Todas   3
                int sillas = Math.Abs(sillasLibres - 3);
                for (int i = 1; i <= nSillas; i++)
                {
                    string sitState = i > sillas  ? "Libre" : "Ocupada";
                    Console.SetCursorPosition(1, 2 + i);
                    Console.Write($"Silla {i}: {sitState}");
                }
            }
        }
        static void DrawBarberSit()
        {
            lock (writtingLock)
            {
                string sitState = barberSitState ? "Ocupada" : "libre";
                Console.SetCursorPosition(1, 2);
                Console.Write($"Silla Barbero: {sitState}");
            }
        }

        static void DrawBarberState()
        {
            lock (writtingLock)
            {
                string state = barberState ? "Despierto" : "Dormido";
                Console.SetCursorPosition(1, 1);
                Console.Write($"Barbero: {state}");
            }
        }

        static void DrawBarberTime()
        {
            lock (writtingLock)
            {
                Console.SetCursorPosition(25, 1);
                Console.Write($"Tiempo Restante: {waitTime}");
            }
        }

        static void DrawMissingSits()
        {
            lock (writtingLock)
            {
                Console.SetCursorPosition(0, 7);
                Console.Write("Hacen Falta Sillas en la barberia");
            }
        }
    }
}
