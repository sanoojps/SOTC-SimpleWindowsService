using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using System.Data;

using System.Windows.Forms;
using System.Threading;


namespace MyWindowsService
{
    class MiddleWare
    {
        private BoatswainsCallPipe pipeServer;

        public MiddleWare()
        {
            this.pipeServer = new BoatswainsCallPipe();

        }

        //to start the server and make it Listen 
        public void ServerRun()
        {
            // Compose a string that consists of three lines.
            //string line = this.pipeServer.PipeName + "  " + DateTime.Today;

            // Write the string to a file.
            //System.IO.StreamWriter fil = new System.IO.StreamWriter("c:\\test-Ser.txt");
            //fil.WriteLine(DateTime.Today);

            //start the pipe server if it's not already running
            if (!this.pipeServer.Running)
            {

                this.pipeServer.PipeName = @"\\.\pipe\myNamedPipe";
                this.pipeServer.Start();
                  MessageBox.Show(this.pipeServer.PipeName);
                  // Compose a string that consists of three lines.
                  string lines = this.pipeServer.PipeName+ "  " + DateTime.Today;

                  // Write the string to a file.
                  System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\test-Sr.txt");
                  file.WriteLine(lines);

                  file.Close();

                  System.Diagnostics.Debug.WriteLine(this.pipeServer.PipeName);




            }
            else
            {
                MessageBox.Show("Server already running.");
            }

            MessageBox.Show(this.pipeServer.YouVeGotMail.ToString());
            if (this.pipeServer.YouVeGotMail)
            {
                this.pipeServer.SendMessage("You Son of  a Bitch!! ");
            }

        }





    }
}
