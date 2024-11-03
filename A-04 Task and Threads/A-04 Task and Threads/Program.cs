using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace A_04_Task_and_Threads
{
    internal class Program
    {
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private static readonly object fileLock = new object();
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
                else { Console.WriteLine("You didn't select neither of the options"); }
            }

            Console.WriteLine($"Filename: {fileName}");
            Console.WriteLine($"File Size: {fileSize}");
            Console.WriteLine($"Number of Tasks: {numTask}\n");
            
            Console.WriteLine($"Starting to write to file: {fileName}\n");

            Task monitorTask = Task.Run(() => MonitorFileSize(fileName, fileSize));

            StartTask(fileName, fileSize, numTask);

            Action job = () => FileOperation(fileName, fileSize, cts.Token);

            Task task1 = Task.Factory.StartNew(job, cts.Token);
            Task task2 = Task.Factory.StartNew(job, cts.Token);
            Task task3 = Task.Factory.StartNew(job, cts.Token);


            Console.WriteLine("Press any key to cancel the tasks...");
            Console.ReadKey();

            cts.Cancel();

        }


        static void StartTask(string fileName, int maxSize, int numTask)
        {
            Task monitorTask = Task.Run(() => MonitorFileSize(fileName, maxSize));
            for(int i = 0; i < numTask; i++)
            {
                Task currentTask = new Task(() =>
                {
                    FileOperation(fileName, maxSize, cts.Token);
                }, cts.Token);

                currentTask.Start();
                try
                {
                    currentTask.Wait(cts.Token);
                }
                catch(OperationCanceledException)
                {
                    Console.WriteLine("Task was canceled");
                    break;
                }
            } 
            monitorTask.Wait();
            Console.WriteLine("All task completed sequentially");
        }

        private static void WriteRandomData(FileStream fileStream, int maxSize)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            byte[] buffer = new byte[36];

            for(int i = 0; i < maxSize / buffer.Length;i++)
            {
                for(int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = (byte)chars[random.Next(chars.Length)];
                }
                fileStream.Write(buffer, 0, buffer.Length);
                Task.Delay(100).Wait();
            }
            
        }

        static void MonitorFileSize(string fileName, int maxSize)
        {
            while(true)
            {
                try
                {
                    if (!File.Exists(fileName))
                    {
                        Task.Delay(500).Wait();
                        continue;
                    }
                    long currentSize = new FileInfo(fileName).Length;
                    Console.WriteLine($"Current file Size: {currentSize} bytes");

                    if (currentSize >= maxSize)
                    {
                        Console.WriteLine("Target file size reached.");
                        cts.Cancel();
                        break;
                    }
                    Task.Delay(1000).Wait();
                }
                catch (FileNotFoundException)
                {
                    Task.Delay(500).Wait();
                }
                catch(Exception ex)
                {
                    Console.WriteLine ($"Error on file monitoring: {ex.Message}");
                    break;
                }
            }
            Console.WriteLine($"Final file size: {new FileInfo(fileName).Length} bytes");
        }

        internal static void FileOperation(string filename, int maxSize, CancellationToken cancellationToken)
        {
            try
            {
                lock (fileLock)
                {
                    //open file
                    using (FileStream fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read))
                    {
                        //Process file
                        WriteRandomData(fileStream, maxSize);
                    }
                }
                
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task was Canceled");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally //Finally is used only if we need to close the file
            {
                //test if file is open, and if so, close file
                Console.WriteLine("File closed successfully");
            }
        }
    }
}
