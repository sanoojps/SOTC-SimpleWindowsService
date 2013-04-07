using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;

using System.Threading;

namespace MyWindowsService
{
  class Program : ServiceBase
  {
     

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

        Split_PipeServer pipeServer =
              new Split_PipeServer();


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

      //TODO: clean up any variables and stop any threads
    }
  }
}
