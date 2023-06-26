using System.Collections.Generic;
using System.Threading.Tasks;

namespace Simple.AMT
{
    /// <summary>
    /// CHecks sensors and generate reports the difference
    /// </summary>
    public class SensorMonitor
    {
        private readonly AMT8000 central;
        private bool[] opennedZones;
        private bool[] bypassedZones;
        private bool[] triggeredZones;

        private bool checkForOppenedZones;
        private bool checkForBypassedZones;
        private bool checkForTriggeredZones;

        public SensorMonitor(AMT8000 central)
        {
            this.central = central;
        }
        /// <summary>
        /// Initailizes the monitor for OppenedZones
        /// </summary>
        public void Setup()
            => Setup(true, false, false);
        /// <summary>
        /// Initailizes the monitor
        /// </summary>
        public async void Setup(bool checkForOppenedZones, bool checkForBypassedZones, bool checkForTriggeredZones)
        {
            this.checkForOppenedZones = checkForOppenedZones;
            this.checkForBypassedZones = checkForBypassedZones;
            this.checkForTriggeredZones = checkForTriggeredZones;

            // first pool
            if(checkForOppenedZones) opennedZones = await central.GetOpenedZonesAsync();
            if(checkForBypassedZones) bypassedZones = await central.GetBypassedZonesAsync();
            if(checkForTriggeredZones) triggeredZones = await central.GetTriggeredZonesAsync();
        }
        /// <summary>
        /// Checks for new values and reports changes.
        /// Do not call faster than 1 call every 5s if checking for all 3 options
        /// </summary>
        /// <returns>Changed zones/sensors</returns>
        public async Task<SensorMonitorEvent[]> UpdateAsync()
        {
            var events = new List<SensorMonitorEvent>();

            if (checkForOppenedZones) generateDiff(events, opennedZones, await central.GetOpenedZonesAsync(), SensorMonitorEvent.Type.Openned);
            if (checkForBypassedZones) generateDiff(events, bypassedZones, await central.GetBypassedZonesAsync(), SensorMonitorEvent.Type.Bypassed);
            if (checkForTriggeredZones) generateDiff(events, triggeredZones, await central.GetTriggeredZonesAsync(), SensorMonitorEvent.Type.Triggered);

            return events.ToArray();
        }

        private void generateDiff(List<SensorMonitorEvent> events, bool[] oldValues, bool[] newValues, SensorMonitorEvent.Type type)
        {
            for (int i = 0; i < newValues.Length; i++)
            {
                // check if changed
                if (oldValues[i] == newValues[i]) continue;
                // update
                oldValues[i] = newValues[i];
                // report
                events.Add(new SensorMonitorEvent
                {
                    SensorIndex = i,
                    EventType = type,
                    NewValue = newValues[i]
                });
            }
        }
    }
    public class SensorMonitorEvent
    {
        public enum Type
        {
            Openned,
            Bypassed,
            Triggered,
        }

        public Type EventType { get; set; }
        public int SensorIndex { get; set; }
        public bool NewValue { get; set; }

    }

}
