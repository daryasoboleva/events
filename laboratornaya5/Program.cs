using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ос5_4
{
    class Program
    {
        static List<EventWaitHandle> events = new List<EventWaitHandle>();

        static bool close_prog = false;
        static EventWaitHandle exit_event_listener = new EventWaitHandle(true, EventResetMode.AutoReset, $"exit_from_menu_Event");
        static EventWaitHandle exit_event_sender = new EventWaitHandle(true, EventResetMode.AutoReset, $"exit_from_worker_Event");

        static void exit()
        {
            exit_event_listener.WaitOne();
            exit_event_sender.Set();
            //Console.Write("\n[worker] Консоль закрывается...");
            Environment.Exit(0);
        }

        class Actions
        {
            public void task1()
            {
                while (!close_prog)
                {
                    events[0].WaitOne();

                    Console.WriteLine("Умножение двух чисел");
                    double x, y;
                    Console.Write("Введите первое число: "); x = double.Parse(Console.ReadLine());
                    Console.Write("Введите второе число: "); y = double.Parse(Console.ReadLine());
                    Console.WriteLine($"Результат умножения: {x * y}");

                    events[0].Set();
                    Thread.Sleep(5);
                }
            }
            public void task2()
            {
                while (!close_prog)
                {
                    events[1].WaitOne();

                    Console.WriteLine("Удаление дубликатов из массива вещественных чисел");
                    string path = @"task2in.txt";
                    string path_out = @"text2out.txt";
                    List<string> numbers_l = new StreamReader(path).ReadToEnd().Split(' ').ToList();
                    List<string> numbers_no_dubl = numbers_l.Distinct().ToList();

                    Console.Write("Исходный массив: ");
                    foreach (string num in numbers_l)
                        Console.Write($"{num} ");

                    //numbers_l = numbers_l.Distinct().ToList();
                    Console.Write("\nМассив без дубликатов:");
                    foreach (string num in numbers_no_dubl)
                        Console.Write($"{num} ");

                    StreamWriter wr = new StreamWriter(path_out);
                    wr.WriteLine(numbers_l.Aggregate((a, b) => a + " " + b));
                    wr.Close();
                    events[1].Set();
                    Thread.Sleep(5);
                    Console.WriteLine("");
                }
            }
            public void task3()
            {
                while (!close_prog)
                {
                    events[2].WaitOne();

                    Console.WriteLine("Вывести самое часто встречающееся слово в тексте и количество вхождений");
                    string path = @"task3.txt";
                    Dictionary<string, int> dict = new Dictionary<string, int>();
                    List<string> words = new StreamReader(path).ReadToEnd().Split(' ').ToList();
                    foreach (string word in words)
                        if (!dict.ContainsKey(word)) dict.Add(word, 1);
                        else dict[word]++;
                    string max_key = "";
                    int max_value = 0;
                    foreach (var item in dict)
                        if (item.Value > max_value)
                        {
                            max_value = item.Value;
                            max_key = item.Key;
                        }
                    Console.WriteLine($"Чаще всего встречаемое слово : {max_key} встречается {max_value} раз");

                    events[2].Set();
                    Thread.Sleep(5);
                }
            }
            public void task4()
            {
                while (!close_prog)
                {
                    events[3].WaitOne();
                    Random rnd = new Random();
                    int n;
                    Console.Write("Введите размерность матрицы: "); n = int.Parse(Console.ReadLine());
                    double[][] matrix = new double[n][];
                    for (int i = 0; i < n; ++i)
                        matrix[i] = new double[n];
                    for (int i = 0; i < n; ++i)
                    {
                        for (int j = 0; j < n; ++j)
                        {

                            matrix[i][j] = rnd.Next(0, 100);
                            Console.Write(matrix[i][j] + "\t");
                        }
                        Console.WriteLine();
                    }
                    double min_under_main = matrix[1][0];
                    double max_above_antidiagonal = matrix[0][0];

                    for (int i = 0; i < n; ++i)
                    {
                        for (int j = 0; j < n; ++j)
                        {
                            if (i > j)
                                if (matrix[i][j] < min_under_main) min_under_main = matrix[i][j];
                            if (i + j < n - 1)
                                if (matrix[i][j] > max_above_antidiagonal) max_above_antidiagonal = matrix[i][j];
                        }
                    }
                    Console.WriteLine($"Минимальный элемент среди элементов, расположеннных ниже главной диагонали: {min_under_main}");
                    Console.WriteLine($"Максимальный элемент среди элементов, расположеннных выше побочной диагонали: {max_above_antidiagonal}");

                    events[3].Set();
                    Thread.Sleep(5);
                }
            }
            public void task5()
            {
                while (!close_prog)
                {
                    events[4].WaitOne();

                    int n;
                    Console.WriteLine("Нахождение всех простых чисел от 0 до n (алгоритм <Решето Эратосфена>)"); ;
                    Console.Write("Введите n: "); n = int.Parse(Console.ReadLine());

                    List<int> numbers = new List<int>();
                    //заполнение списка числами от 2 до n-1
                    for (int i = 2; i < n; i++)
                    {
                        numbers.Add(i);
                    }

                    for (int i = 0; i < numbers.Count; i++)
                    {
                        for (int j = 2; j < n; j++)
                        {
                            //удаляем кратные числа из списка
                            numbers.Remove(numbers[i] * j);
                        }
                    }

                    Console.Write("Простые числа: ");
                    foreach (int number in numbers)
                        Console.Write($"{number} ");
                    Console.WriteLine();

                    events[4].Set();
                }
            }
        }


        static void Main()
        {
            exit_event_sender.WaitOne();
            Thread.Sleep(100);
            Thread exit_waiter = new Thread(exit);
            exit_waiter.Start();

            Console.CancelKeyPress += Console_CancelKeyPress;
            Thread.Sleep(1000);
            int commands_amnt = 5;

            List<Thread> actions_l = new List<Thread>();

            Actions actions = new Actions();

            for (int i = 0; i < commands_amnt; ++i)
            {
                events.Add(new EventWaitHandle(false, EventResetMode.AutoReset));
                events[i] = EventWaitHandle.OpenExisting($"task{i + 1}_Event");
            }
            events.Add(new EventWaitHandle(false, EventResetMode.AutoReset));
            //events[events.Count - 1] = EventWaitHandle.OpenExisting($"exit_Event");

            actions_l.Add(new Thread(actions.task1));
            actions_l.Add(new Thread(actions.task2));
            actions_l.Add(new Thread(actions.task3));
            actions_l.Add(new Thread(actions.task4));
            actions_l.Add(new Thread(actions.task5));

            foreach (Thread thr in actions_l) thr.Start();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            exit_event_sender.Set();
            Environment.Exit(0);
        }
    }
}