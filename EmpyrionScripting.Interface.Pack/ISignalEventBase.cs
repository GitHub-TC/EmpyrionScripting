using System;

namespace EmpyrionScripting.Interface
{
    public interface ISignalEventBase
    {
        string Name { get; set; }
        bool State { get; set; }
        DateTime TimeStamp { get; set; }
        int TriggeredByEntityId { get; set; }
    }
}