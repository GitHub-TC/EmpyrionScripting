using System;

namespace EmpyrionScripting.DataWrapper
{
    public class SignalEventBase
    {

        public SignalEventBase(){}

        public SignalEventBase(SignalEventBase signalBase)
        {
            Name                 = signalBase.Name;
            TimeStamp            = signalBase.TimeStamp;
            State                = signalBase.State;
            TriggeredByEntityId  = signalBase.TriggeredByEntityId;
        }

        public string Name { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool State { get; set; }
        public int TriggeredByEntityId { get; set; }
    }
}
