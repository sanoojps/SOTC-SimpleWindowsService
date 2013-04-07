using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;

using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace MyWindowsService
{
    

  class Program : ServiceBase
  {
      /// Return Type: BOOL->int
      ///hNamedPipe: HANDLE->void*
      [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "DisconnectNamedPipe")]
      [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
      public static extern bool DisconnectNamedPipe([System.Runtime.InteropServices.InAttribute()] SafeFileHandle hNamedPipe);

      /// Return Type: BOOL->int
      ///hObject: HANDLE->void*
      [System.Runtime.InteropServices.DllImportAttribute("kernel32.dll", EntryPoint = "CloseHandle")]
      [return: System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
      public static extern bool CloseHandle([System.Runtime.InteropServices.InAttribute()] SafeFileHandle hObject);


      Split_PipeServer pipeServer =
             new Split_PipeServer();
    static void Main(string[] args)
    {
        
      ServiceBase.Run(new Program());

    }

    public Program()
    {
      this.ServiceName = "My Service";
      

    }

    protected override void OnStart(string[] args)
    {
        

        //TODO: place your start code here

        //Serv nuSer = new Serv();
        //nuSer.ListenForClients();
       

     // WindowsServiceMonitor nuMOn = new WindowsServiceMonitor();
      //Thread t1 = new Thread(nuMOn.StartMonitor);
       // nuMOn.StartMonitor();



        Thread t = new Thread(Y);
     t.Start();
      //nu.ServerRun();

      




      base.OnStart(args);

    }


    public void Y()
    {
        //MiddleWare nu = new MiddleWare();
        //nu.ServerRun();

        //Server nu = new Server();
        //nu.Serveradd();

       


        //start the pipe server if it's not already running
        if (!pipeServer.Running)
        {
            pipeServer.PipeName = "\\\\.\\pipe\\myNamedPipe";
            pipeServer.Start();
            pipeServer.SendMessage("this.tbSend.Text");

        }
 
    }


   
    protected override void OnStop()
    {
      base.OnStop();
        if(!pipeServer.PipeHandle.IsInvalid)
            try
            {
                DisconnectNamedPipe(pipeServer.PipeHandle);
                CloseHandle(pipeServer.PipeHandle);
            }
            catch (Exception eXception)
            {
            }
      //TODO: clean up any variables and stop any threads
    }
  }
}
