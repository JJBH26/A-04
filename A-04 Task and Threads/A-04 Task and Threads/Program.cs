using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A_04_Task_and_Threads
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 3) 
            {
                Console.WriteLine("Please put 3 arguments: <filename> <file size> <number of tasks>");
                return;
            }

            string fileName = args[0];
            if(!int.TryParse(args[1], out int fileSize) || fileSize <= 1000 || fileSize >=20000000) 
            {
                Console.WriteLine("Invalid Size\nGive a number between 1000 and 20,000,000");
                return;
            }
            if (!int.TryParse(args[2], out int numTask) || numTask <= 1 || numTask >= 25)
            {
                Console.WriteLine("Invalid Task number\nGive a number between 1 and 25");
                return;
            }

            if (File.Exists(fileName))
            {
                Console.WriteLine($"The file: {fileName}, already exists. \nYou want to overrided it (yes 'y', no 'n')");
                string response = Console.ReadLine();
                if (response?.ToLower() != "y")
                {
                    Console.WriteLine("Operation canceled");
                    return;
                }
                else if(response?.ToLower() != "no")
                {
                    File.Delete(fileName);
                }
                else { Console.WriteLine("You didnt select neither of the options, exiting of the program"); }
            }

            Console.WriteLine($"Filename: {fileName}");
            Console.WriteLine($"File Size: {fileSize}");
            Console.WriteLine($"Number of Tasks: {numTask}");

            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numTask; i++)
            {
                tasks.Add(Task.Run(() => WriteRandomData(fileName, fileSize)));
            }
        }

        static void WriteRandomData(string fileName, int maxSize)
        {
            Random random = new Random();
            byte[] data = new byte[36];

            using(FileStream stream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.None))
            {
                while(stream.Length < maxSize)
                {
                    random.NextBytes(data);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
    }
}
