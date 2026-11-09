using System;
using System.ServiceProcess;

namespace DPrintSpoolerManager;

public static class SpoolerService
{
    private const string ServiceName = "Spooler";

    public static string GetStatus()
    {
        using var service = new ServiceController(ServiceName);
        service.Refresh();
        return service.Status.ToString();
    }

    public static string Start()
    {
        using var service = new ServiceController(ServiceName);
        service.Refresh();

        if (service.Status == ServiceControllerStatus.Running)
            return "Print Spooler is already running.";

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
        return "Print Spooler started successfully.";
    }

    public static string Stop()
    {
        using var service = new ServiceController(ServiceName);
        service.Refresh();

        if (service.Status == ServiceControllerStatus.Stopped)
            return "Print Spooler is already stopped.";

        service.Stop();
        service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        return "Print Spooler stopped successfully.";
    }

    public static string Restart()
    {
        using var service = new ServiceController(ServiceName);
        service.Refresh();

        if (service.Status != ServiceControllerStatus.Stopped)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        }

        service.Start();
        service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
        return "Print Spooler restarted successfully.";
    }
}
