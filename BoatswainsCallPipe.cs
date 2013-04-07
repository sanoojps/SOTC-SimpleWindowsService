using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;




namespace MyWindowsService
{
    public class BoatswainsCallPipe
    {

        //The first function we'll need to call is CreateNamedPipe(). 
        //This function will be creating an instance of our named pipe and 
        //return a handle to the file for subsequent operations
        /*
         HANDLE WINAPI CreateNamedPipe(
  _In_      LPCTSTR lpName,
  _In_      DWORD dwOpenMode,
  _In_      DWORD dwPipeMode,
  _In_      DWORD nMaxInstances,
  _In_      DWORD nOutBufferSize,
  _In_      DWORD nInBufferSize,
  _In_      DWORD nDefaultTimeOut,
  _In_opt_  LPSECURITY_ATTRIBUTES lpSecurityAttributes
);
         */
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

        //Now ConnectNamedPipe() comes in. 
        //This function will wait until a client connection has been established.

        /*
BOOL WINAPI ConnectNamedPipe(
  _In_         HANDLE hNamedPipe,
  _Inout_opt_  LPOVERLAPPED lpOverlapped
);

*/

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int ConnectNamedPipe(
           SafeFileHandle hNamedPipe, //A handle to the server end of a named pipe instance.
            //This handle is returned by the CreateNamedPipe function.
            IntPtr lpOverlapped);     //A pointer to an OVERLAPPED structure.
        //asynchronous or overlapped

        //pipe mode set to read-write and ASYNC
        public const uint PIPE_ACCESS_DUPLEX = (0x00000003);
        public const uint FILE_FLAG_OVERLAPPED = (0x40000000);

        //Set Data Stream Buffer Size
        public const int BUFFER_SIZE = 4096;

        string pipeName;  //PipeName
        Thread listenThread; //The ListenTHread
        bool running;
        bool youVeGotMail;

        //Declare a client class to obtain info from the
        //incoming client conenctions

        public class Client
        {
            public SafeFileHandle handle;
            public FileStream stream;
        }

        List<Client> clients;

        //getter and setter for PipeName
        public string PipeName
        {
            get { return this.pipeName; }
            set { this.pipeName = value; }
        }

        //getter and setter for running
        public bool Running
        {
            get { return this.running; }
        }


        public bool YouVeGotMail
        {
            get { return this.youVeGotMail; }
        }

        //Constructor for the Server

        public BoatswainsCallPipe()
        {
            //initialize a new list
            this.clients = new List<Client>();
        }


        /// <summary>
        /// Starts the pipe server
        /// </summary>
        public void Start()
        {
            //start the listening thread
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Name = @"ListenForClients";
            this.listenThread.Start();

            this.running = true;
        }


        //implement the ListenForClients method which listens for client 
        //connections

        /// <summary>
        /// Listens for client connections
        /// </summary>
        private void ListenForClients()
        {
            while (true)
            {
                SafeFileHandle clientHandle =
                  CreateNamedPipe(
                    this.pipeName,
                    PIPE_ACCESS_DUPLEX | FILE_FLAG_OVERLAPPED,
                    0,
                    255,
                    BUFFER_SIZE,
                    BUFFER_SIZE,
                    0,
                    IntPtr.Zero
                    );

                // Write the string to a file.
                System.IO.StreamWriter fileZ = new System.IO.StreamWriter("c:\\test-Sr.txt");
                fileZ.WriteLine(clientHandle);

                fileZ.Close();
                //could not create named pipe
                if (clientHandle.IsInvalid)
                    return;

                int success = ConnectNamedPipe(clientHandle, IntPtr.Zero);

                //could not connect client
                if (success == 0)
                    return;

                Client client = new Client();
                client.handle = clientHandle;
               

                // Write the string to a file.
                System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\test-ClientHandle.txt");
                file.WriteLine(client.handle);

                file.Close();

                lock (clients)
                    this.clients.Add(client);




                //   private void Read(object clientObj) 
                //method can be called here to read data from the client

                Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.Name = @"Read(object clientObj)";
                readThread.Start(client);
            }
        }
        StateExchange eXchange = new StateExchange();

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

                //client has disconnected
                if (bytesRead == 0)
                    break;

                string RecievedMessage =
                    encoder.GetString(buffer, 0, bytesRead);

                System.Windows.Forms.MessageBox.Show(RecievedMessage, "wALLAHA");

                // Write the string to a file.
                System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\test-Message.txt");
                file.WriteLine(RecievedMessage);

                file.Close();

                

                if (RecievedMessage != null)
                {
                    if (this.eXchange.IVeGotAnAlert)
                    {
                    SendMessage("You Son of  a Bitch!! " + RecievedMessage);
                    }

                    this.youVeGotMail = true;

                }
                else
                    SendMessage("You Son of  a Bitch!! Why this?");
                this.youVeGotMail = false;
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



/*

 * To implement this with a form
 * 
 * //Instantiate the server
 * BoatswainsCallPipe pipeServer = new BoatswainsCallPipe();
 * 
 * //provide pipe name
 * "\\\\.\\pipe\\myNamedPipe"
 *
 * //declare an event handler
 *  this.pipeServer.MessageReceived += 
                new Server.MessageReceivedHandler(pipeServer_MessageReceived);
 * 
 *  void pipeServer_MessageReceived(Server.Client client, string message)
        {
            this.Invoke(new Server.MessageReceivedHandler(DisplayMessageReceived),
                new object[] { client, message });   
        }

        void DisplayMessageReceived(Server.Client client, string message)
        {
            this.tbReceived.Text += message + "\r\n";
            MessageBox.Show(message + "\r\n");
        }
 * 
 * //start the server 
 * pipeServer.Start();

 * to send message 
pipeServer.SendMessage(this.tbSend.Text);
 * 
 * 
*/