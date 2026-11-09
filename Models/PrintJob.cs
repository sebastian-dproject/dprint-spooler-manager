namespace DPrintSpoolerManager;

public class PrintJob
{
    public string Printer { get; set; } = "";
    public string WmiName { get; set; } = "";
    public string Document { get; set; } = "";
    public string User { get; set; } = "";
    public string Status { get; set; } = "";
    public string Pages { get; set; } = "";
    public string Submitted { get; set; } = "";
}
