using System;
using System.Collections.Generic;
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
            if (!int.TryParse(args[1], out int numTask) || numTask <= 1 || numTask >= 25)
            {
                Console.WriteLine("Invalid Task number\nGive a number between 1 and 25");
                return;
            }
            Console.WriteLine($"Filename: {fileName}");
            Console.WriteLine($"File Size: {fileSize}");
            Console.WriteLine($"Number of Tasks: {numTask}");

            try
            {
                using (var fileCreation = new System.IO.FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                using (var writeOnFile = new System.IO.StreamWriter(fileCreation))
                {
                    byte data = (byte)'A';
                    int bytesWritten = 0;

                    while(bytesWritten < fileSize)
                    {
                        fileCreation.WriteByte(data);
                        bytesWritten += 1;
                    }
                }
                Console.WriteLine("File created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating the file: {ex.Message}");
            }
        }
    }
}
