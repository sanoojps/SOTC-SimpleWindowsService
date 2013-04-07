using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration.Install;
using System.ComponentModel;
using System.ServiceProcess;

namespace MyWindowsService
{
  [RunInstaller(true)]
  public class MyWindowsServiceInstaller : Installer
  {
    public MyWindowsServiceInstaller()
    {
      var processInstaller = new ServiceProcessInstaller();
      var serviceInstaller = new ServiceInstaller();

      //set the privileges
      processInstaller.Account = ServiceAccount.LocalSystem;


      serviceInstaller.DisplayName = "My Service";
      serviceInstaller.StartType = ServiceStartMode.Manual;

      //must be the same as what was set in Program's constructor
      serviceInstaller.ServiceName = "My Service";

      this.Installers.Add(processInstaller);
      this.Installers.Add(serviceInstaller);
    }
  }
}
