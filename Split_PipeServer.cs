

using System;
using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;

namespace MyWindowsService
{
    class Split_PipeServer
    {

        #region Security

        /// <summary>
        /// Security Descriptor structure
        /// </summary>
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct SECURITY_DESCRIPTOR
        {
            public byte revision;
            public byte size;
            public short control;
            public IntPtr owner;
            public IntPtr group;
            public IntPtr sacl;
            public IntPtr dacl;
        }

        /// <summary>
        /// Security Attributes structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        /// <summary>
        /// 
        /// </summary>
        
        public class SecurityNative
        {
            /// <summary>
            /// Sets the security descriptor attributes.
            /// </summary>
            /// <param name="sd">Reference to a SECURITY_DESCRIPTOR structure.
            /// </param>
            /// <param name="bDaclPresent">A flag that indicates the presence of 
            /// a DACL in the security descriptor.</param>
            /// <param name="Dacl">A pointer to an ACL structure that specifies
            /// the DACL for the security descriptor.</param>
            /// <param name="bDaclDefaulted">A flag that indicates the source 
            /// of the DACL.</param>
            /// <returns>If the function succeeds, the function returns nonzero.
            /// </returns>
            [DllImport("Advapi32.dll", SetLastError = true)]
            public static extern bool SetSecurityDescriptorDacl(
                ref SECURITY_DESCRIPTOR sd,         // A pointer to the 
                // SECURITY_DESCRIPTOR struct
                bool bDaclPresent,
                IntPtr Dacl,                        // A pointer to an ACL struct
                bool bDaclDefaulted                 // The source of the DACL
                );

            /// <summary>
            /// Initializes a SECURITY_DESCRIPTOR structure.
            /// </summary>
            /// <param name="sd"></param>
            /// <param name="dwRevision"></param>
            /// <returns></returns>
            [DllImport("Advapi32.dll", SetLastError = true)]
            public static extern bool InitializeSecurityDescriptor(
                out SECURITY_DESCRIPTOR sd,
                int dwRevision
                );

        } // class SecurityNative

        #endregion


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

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool ImpersonateNamedPipeClient(
           SafeFileHandle hNamedPipe);

        public const uint DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }

        List<Client> clients;

        public const int BUFFER_SIZE = 4096;

        string pipeName;
        Thread listenThread;
        bool running;


        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        public bool Running
        {
            get { return this.running; }
        }

        public Split_PipeServer()
        {
            this.clients = new List<Client>();
        }


        /// <summary>
        /// Starts the pipe server
        /// </summary>
        public void Start()
        {
            //start the listening thread
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();

            this.running = true;
        }

        /// <summary>
        /// Listens for client connections
        /// </summary>
        private void ListenForClients()
        {

            // Prepare the security attributes

            IntPtr pSa = IntPtr.Zero;   // NULL
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();

            SECURITY_DESCRIPTOR sd;
            SecurityNative.InitializeSecurityDescriptor(out sd, 1);
            // DACL is set as NULL to allow all access to the object.
            SecurityNative.SetSecurityDescriptorDacl(ref sd, true, IntPtr.Zero, false);
            sa.lpSecurityDescriptor = Marshal.AllocHGlobal(Marshal.SizeOf(
                typeof(SECURITY_DESCRIPTOR)));
            Marshal.StructureToPtr(sd, sa.lpSecurityDescriptor, false);
            sa.bInheritHandle = false;              // Not inheritable
            sa.nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES));

            pSa = Marshal.AllocHGlobal(sa.nLength);
            Marshal.StructureToPtr(sa, pSa, false);

            while (true)
            {
                SafeFileHandle clientHandle =
                CreateNamedPipe(
                     this.pipeName,
                     DUPLEX | FILE_FLAG_OVERLAPPED,
                     0,
                     255,
                     BUFFER_SIZE,
                     BUFFER_SIZE,
                     0,
                     pSa);

                //could not create named pipe
                if (clientHandle.IsInvalid)
                    return;

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);
                //ImpersonateNamedPipeClient(clientHandle);
                //could not connect client
                if (success == 0)
                    return;

                Client client = new Client();
                client.handle = clientHandle;

                lock (clients)
                    this.clients.Add(client);

                Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.Start(client);
            }
        }

        /// <summary>
        /// Reads incoming data from connected clients
        /// </summary>
        /// <param name="clientObj"></param>
        private void Read(object clientObj)
        {
            Client client = (Client)clientObj;
            client.stream = new FileStream(client.handle, FileAccess.ReadWrite, BUFFER_SIZE, true);
            byte[] buffer = new byte[BUFFER_SIZE];
            ASCIIEncoding encoder = new ASCIIEncoding();
            SendMessage("Ihearby");
            while (true)
            {
                int bytesRead = 0;

                try
                {
                    bytesRead = client.stream.Read(buffer, 0, BUFFER_SIZE);
                }
                catch
                {
                    //read error has occurred
                    break;
                }
                SendMessage("this.tbSend.Text" + encoder.GetString(buffer, 0, bytesRead));

                //client has disconnected
                if (bytesRead == 0)
                    break;

              

            }

            //clean up resources
            client.stream.Close();
            client.handle.Close();
            lock (this.clients)
                this.clients.Remove(client);
        }



        /// <summary>
        /// Sends a message to all connected clients
        /// </summary>
        /// <param name="message">the message to send</param>
        public void SendMessage(string message)
        {
            lock (this.clients)
            {
                ASCIIEncoding encoder = new ASCIIEncoding();
                byte[] messageBuffer = encoder.GetBytes(message);
                foreach (Client client in this.clients)
                {
                    client.stream.Write(messageBuffer, 0, messageBuffer.Length);
                    client.stream.Flush();
                }
            }
        }








    }
}
