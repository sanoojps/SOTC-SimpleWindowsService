using System;
using System.Collections.Generic;

using System.Text;


using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace MyWindowsService
{



    class Server
    {
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

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe,
           IntPtr lpOverlapped);


        public void Serveradd()
        {
            Listen();
        }


        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public const string PIPE_NAME = "\\\\.\\pipe\\myNamedPipe";
        public const int BUFFER_SIZE = 4096;

        private void Listen()
        {
            SafeFileHandle clientPipeHandle;
            while (true)
            {
                clientPipeHandle = CreateNamedPipe(
                   PIPE_NAME,
                   DUPLEX | FILE_FLAG_OVERLAPPED,
                   0,
                   255,
                   BUFFER_SIZE,
                   BUFFER_SIZE,
                   0,
                   IntPtr.Zero);

                //failed to create named pipe
                if (clientPipeHandle.IsInvalid)
                {
                    break;
                }

                else
                {
                    int success = ConnectNamedPipe(
                       clientPipeHandle,
                       IntPtr.Zero);

                    //failed to connect client pipe
                    if (success != 1)
                    {
                        break;
                    }

                    else
                    {
                        FileStream fStream =
                         new FileStream(clientPipeHandle,
                             FileAccess.ReadWrite,
                             BUFFER_SIZE,
                             true
                             );
                        while (true)
                        {
                            byte[] buffer = new byte[BUFFER_SIZE];
                            ASCIIEncoding encoder = new ASCIIEncoding();


                            int bytesRead = fStream.Read(buffer, 0, BUFFER_SIZE);



                            System.Diagnostics.Debug.WriteLine(
                               encoder.GetString(buffer) + "\n");


                            byte[] sendBuffer = encoder.GetBytes("message");
                            fStream.Write(sendBuffer, 0, sendBuffer.Length);
                            fStream.Flush();
                        }

                        //client connection successfull
                    }                    //handle client communication

                }



            }
        }
    }
}
