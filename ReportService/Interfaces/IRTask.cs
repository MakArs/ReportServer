﻿namespace ReportService.Interfaces
{
    public enum RTaskType : byte
    {
        Common = 1,
        Custom = 2
    }

    public interface IRTask
    {
        void Execute(string address = null);

        // RTaskType MyType...
    }
}
