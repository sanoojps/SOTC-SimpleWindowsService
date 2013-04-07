using System;
using System.Collections.Generic;
using System.Text;

namespace MyWindowsService
{
    public class StateExchange //This is to alert the pipe server that  a wmi event has happened
    {
        public bool iVeGotAnALert;
        public StateExchange()
        {
 
        }

        //getter and setter for running
        public bool IVeGotAnAlert
        {
            get
            {
                return this.iVeGotAnALert;
            }

            set 
            {
                this.iVeGotAnALert = value;
            }
           
        }
    }
}
