using System;
using System.Collections.Generic;
using System.Management;

namespace DPrintSpoolerManager;

public static class PrintQueueService
{
    // =========================
    // READ ALL PRINT JOBS
    // =========================

    public static List<PrintJob> ReadPrintJobs()
    {
        var jobs = new List<PrintJob>();

        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, Document, Owner, Status, JobStatus, " +
            "TotalPages, PagesPrinted, TimeSubmitted " +
            "FROM Win32_PrintJob");

        using var results = searcher.Get();

        foreach (ManagementObject item in results)
        {
            try
            {
                string name = GetString(item, "Name");

                int separator = name.LastIndexOf(',');

                string printer = separator >= 0
                    ? name.Substring(0, separator).Trim()
                    : name;

                string time = GetString(item, "TimeSubmitted");

                string submitted = "N/A";

                if (!string.IsNullOrWhiteSpace(time))
                {
                    try
                    {
                        submitted =
                            ManagementDateTimeConverter
                                .ToDateTime(time)
                                .ToString("dd/MM/yyyy HH:mm");
                    }
                    catch { }
                }

                int total = GetInt(item, "TotalPages");
                int printed = GetInt(item, "PagesPrinted");

                jobs.Add(new PrintJob
                {
                    Printer = printer,
                    WmiName = name,
                    Document = GetString(item, "Document"),
                    User = GetString(item, "Owner"),
                    Status = GetJobStatus(item),
                    Pages = total > 0
                        ? $"{printed}/{total}"
                        : "N/A",
                    Submitted = submitted
                });
            }
            finally
            {
                item.Dispose();
            }
        }

        return jobs;
    }

    private static string GetString(
        ManagementBaseObject item, string property)
    {
        return item[property]?.ToString() ?? "";
    }

    private static int GetInt(
        ManagementBaseObject item, string property)
    {
        try
        {
            return Convert.ToInt32(item[property] ?? 0);
        }
        catch
        {
            return 0;
        }
    }

    private static string GetJobStatus(
        ManagementObject item)
    {
        int code = GetInt(item, "JobStatus");

        if ((code & 64) != 0) return "Offline";
        if ((code & 32) != 0) return "Paper Out";
        if ((code & 1024) != 0) return "User Intervention";
        if ((code & 512) != 0) return "Blocked";
        if ((code & 2) != 0) return "Error";
        if ((code & 1) != 0) return "Paused";
        if ((code & 4) != 0) return "Deleting";
        if ((code & 16) != 0) return "Printing";
        if ((code & 8) != 0) return "Spooling";

        string status = GetString(item, "Status");

        return string.IsNullOrWhiteSpace(status)
            ? "Pending"
            : status;
    }

    // =========================
    // DELETE ONE SELECTED JOB
    // =========================

    public static bool ClearJob(string wmiName)
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PrintJob");

        using var results = searcher.Get();

        foreach (ManagementObject item in results)
        {
            using (item)
            {
                string currentName = GetString(item, "Name");

                if (!string.Equals(
                    currentName,
                    wmiName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                item.Delete();
                return true;
            }
        }

        return false;
    }

    // =========================
    // CLEAR ALL QUEUES
    // =========================

    public static int ClearJobs()
    {
        int removed = 0;

        using var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PrintJob");

        using var results = searcher.Get();

        foreach (ManagementObject item in results)
        {
            using (item)
            {
                item.Delete();
                removed++;
            }
        }

        return removed;
    }
}
