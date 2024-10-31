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
            if(!int.TryParse(args[1], out int fileSize) || fileSize < 1000 || fileSize > 20000000) 
            {
                Console.WriteLine("Invalid Size\nGive a number between 1000 and 20,000,000");
                return;
            }
            if (!int.TryParse(args[2], out int numTask) || numTask < 1 || numTask > 25)
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
                else if(response?.ToLower() != "n")
                {
                    File.Delete(fileName);
                }
                else { Console.WriteLine("You didnt select neither of the options, exiting of the program"); }
            }

            Console.WriteLine($"Filename: {fileName}");
            Console.WriteLine($"File Size: {fileSize}");
            Console.WriteLine($"Number of Tasks: {numTask}\n");

            Console.WriteLine($"Starting to write to file: {fileName}");
            StartTask(fileName, fileSize, numTask);
        }

        static void StartTask(string fileName, int maxSize, int numTask)
        {
            List<Task> tasks = new List<Task>();

            for(int i = 0; i < numTask; i++)
            {
                tasks.Add(Task.Run(() => FileOperation(fileName, maxSize)));
            } 

            Task monitorTask = Task.Run(() => MonitorFileSize(fileName, maxSize, tasks));
            tasks.Add(monitorTask);

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("All task completed");
        }

        private static void WriteRandomData(FileStream fileStream, int maxSize)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            byte[] buffer = new byte[36];

            for(int i = 0; i < maxSize;i++)
            {
                for(int j = 0; j < maxSize; j++)
                {
                    buffer[j] = (byte)chars[random.Next(chars.Length)];
                }
                fileStream.Write(buffer, 0, buffer.Length);
            }
            
        }

        internal static void FileOperation(string filename, int maxSize)
        {
            FileStream fileStream = null;

            try
            {
                //open file
               fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);

                //Process file
                WriteRandomData(fileStream, maxSize);
            }   
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally //Finally is used only if we need to close the file
            {
                //test if file is open, and if so, close file
                fileStream.Close();
                Console.WriteLine("File closed successfully");
            }
        }
    }
}
