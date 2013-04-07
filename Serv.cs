using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Threading;

namespace MyWindowsService
{
    class Serv
    {

        public Serv()
        {
            //ListenForClients();
        }

        //DLL import for CreateNamedPipe

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern SafeFileHandle CreateNamedPipe(
           String pipeName,
           uint dwOpenMode,
           uint dwPipeMode,
           uint nMaxInstances,
           uint nOutBufferSize,
           uint nInBufferSize,
           uint nDefaultTimeOut,
           IntPtr lpSecurityAttributes);


        //DllImport for ConnectNamedPipe

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);

        //Dll Import for  DisconnectNamedPipe
        [DllImport("kernel32.dll")]
        static extern bool DisconnectNamedPipe(IntPtr hNamedPipe);


        //pipe mode set to read-write and ASYNC
        public const uint PIPE_ACCESS_DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        //Set Data Stream Buffer Size
        public const int BUFFER_SIZE = 4096;

        //Set Pipe Name
        public const string PIPE_NAME = "\\\\.\\pipe\\myNamedPipe";


        /// <summary>
        /// Starts the pipe server
        /// </summary>
       



        //Listen For Connections

        //implement the ListenForClients method which listens for client 
        //connections

        /// <summary>
        /// Listens for client connections
        /// </summary>
        public void ListenForClients()
        {
           
            while (true)
            {
                
                    SafeFileHandle clientHandle =
                      CreateNamedPipe(
                        PIPE_NAME,
                        PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED,
                        0,
                        255,
                        BUFFER_SIZE,
                        BUFFER_SIZE,
                        0,
                        IntPtr.Zero
                        );


                    if (clientHandle.IsInvalid)
                    {
                        System.Diagnostics.Debug.WriteLine("PIpe Creation Failed");



                        return;
                    }
                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                //could not connect client
                if (success == 0)
                    return;





                //   private void Read(object clientObj) 
                //method can be called here to read data from the client

              /*
                Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.Name = @"Read(object clientObj)";
                readThread.Start(clientHandle);
            */
            
                Read(clientHandle);


           }
        }

        /// <summary>
        /// Reads incoming data from connected clients
        /// </summary>
        /// <param name="clientObj"></param>
        private void Read(object clientObj)
        {
            SafeFileHandle clientHandle = (SafeFileHandle)clientObj;
            FileStream fstream = new FileStream(clientHandle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();

            while (true)
            {
                int bytesRead = 0;

                try
                {

                    bytesRead = fstream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    //read error has occurred
                    //break;
                }

                //client has disconnected
                if (bytesRead == 0)
                    // break;
                    encoder.GetString(buffer, 0, bytesRead);
                
            string RecievedMessage = encoder.GetString(buffer, 0, bytesRead).ToString();

                System.Windows.Forms.MessageBox.Show(RecievedMessage, "wALLAHA");



                if (RecievedMessage != null)
                {

                    {
                        SendMessage("You Son of  a Bitch!! " + RecievedMessage, fstream);
                    }



                }
                else
                    SendMessage("You Son of  a Bitch!! Why this?", fstream);

            }

            //clean up resources
            //fstream.Close();
            clientHandle.Close();
            

        }


        /// <summary>
        /// Sends a message to all connected clients
        /// </summary>
        /// <param name="message">the message to send</param>
        public void SendMessage(string message, FileStream theStream)
        {

            ASCIIEncoding encoder = new ASCIIEncoding();
            byte[] messageBuffer = encoder.GetBytes(message);

            theStream.Write(messageBuffer, 0, messageBuffer.Length);
            theStream.Flush();

        }



    }
}
