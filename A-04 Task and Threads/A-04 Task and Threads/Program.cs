/*
* File        : A-04_Task_and_Threads.cs
* Project     : PROG2121 - A-04 Task and Threads
* Programmer  : Juan Jose Bejarano
* First version: 11/04/2024
* Description:
* In this project, I implemented a console application that writes random data 
* to a specified file using multiple tasks. The application monitors the file size 
* and stops writing when the target size is reached. It handles command-line arguments 
* for the file name, size, and number of tasks, and includes cancellation token support 
* for graceful task termination. The goal is to evaluate the performance of concurrent 
* file writing in C#.
*/

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

        /*
        * Name    : Main
        * Purpose : This method processes command line arguments, initializes file writing tasks, 
        *           and monitors file size while allowing cancellation via a key press.
        * Inputs  : args[] - Command line arguments specifying the file name, file size, and number of tasks
        * Outputs : NONE
        * Returns : NONE
        */
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Please put 3 arguments: <filename> <file size> <number of tasks>");
                return;
            }

            string fileName = args[0];
            if (!int.TryParse(args[1], out int fileSize) || fileSize < 1000 || fileSize > 20000000)
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
                else if (response?.ToLower() != "n")
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

            Task.Run(() =>
            {
                Console.WriteLine("Press any key to cancel the tasks...");
                Console.ReadKey();
                cts.Cancel();
            });

            monitorTask.Wait();

        }

        /*
         * Name    : StartTask
         * Purpose : This method initiates multiple tasks for writing random data to a specified file 
         *           and monitors file size concurrently.
         * Inputs  : fileName - The name of the file to write to
         *           maxSize - The maximum size the file should reach
         *           numTask - The number of writing tasks to be executed
         * Outputs : Prints status messages indicating task completion or cancellation
         * Returns : NONE
         */
        static void StartTask(string fileName, int maxSize, int numTask)
        {
            Task monitorTask = Task.Run(() => MonitorFileSize(fileName, maxSize));
            for (int i = 0; i < numTask; i++)
            {
                try
                {
                    FileOperation(fileName, maxSize, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Task was canceled");
                    break;
                }
            }
            monitorTask.Wait();
            Console.WriteLine("All task completed sequentially");
        }

        /*
         * Name    : WriteRandomData
         * Purpose : This method writes random data to the provided file stream until the maximum size is reached.
         * Inputs  : fileStream - The file stream to write random data to
         *           maxSize - The maximum size of data to write
         * Outputs : NONE
         * Returns : NONE
         */
        private static void WriteRandomData(FileStream fileStream, int maxSize)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            byte[] buffer = new byte[36];

            for (int i = 0; i < maxSize / buffer.Length; i++)
            {
                for (int j = 0; j < buffer.Length; j++)
                {
                    buffer[j] = (byte)chars[random.Next(chars.Length)];
                }
                fileStream.Write(buffer, 0, buffer.Length);
                Task.Delay(100).Wait();
            }

        }

        /*
         * Name    : MonitorFileSize
         * Purpose : This method monitors the size of the specified file and cancels tasks when the 
         *           target size is reached.
         * Inputs  : fileName - The name of the file to monitor
         *           maxSize - The target size to reach before stopping tasks
         * Outputs : Prints current file size and a message when the target size is reached
         * Returns : NONE
         */
        static void MonitorFileSize(string fileName, int maxSize)
        {
            bool targetReached = false;

            while (!targetReached)
            {
                try
                {
                    if (!File.Exists(fileName))
                    {
                        Task.Delay(10000).Wait();
                        continue;
                    }
                    long currentSize = new FileInfo(fileName).Length;
                    Console.WriteLine($"Current file Size: {currentSize} bytes");

                    if (currentSize >= maxSize)
                    {
                        Console.WriteLine("Target file size reached.");
                        targetReached = true;
                        cts.Cancel();
                        break;
                    }
                    Task.Delay(1000).Wait();
                }
                catch (FileNotFoundException)
                {
                    Task.Delay(500).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error on file monitoring: {ex.Message}");
                    break;
                }
            }
            if (targetReached)
            {
                Console.WriteLine($"Final file size: {new FileInfo(fileName).Length} bytes");
            }
        }

        /*
         * Name    : FileOperation
         * Purpose : This method performs the file writing operation by acquiring a lock on the file 
         *           to ensure thread safety during writing.
         * Inputs  : filename - The name of the file to write to
         *           maxSize - The maximum size of data to write
         *           cancellationToken - A token to cancel the operation if needed
         * Outputs : Prints status messages indicating task cancellation or errors
         * Returns : NONE
         */
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
