using System;

namespace LiveSplit.TimeFormatters
{
    internal class DeltaSplitTimeFormatter : ITimeFormatter
    {
        public TimeAccuracy Accuracy { get; set; }
        public bool DropDecimals { get; set; }

        public DeltaSplitTimeFormatter(TimeAccuracy accuracy, bool dropDecimals)
        {
            Accuracy = accuracy;
            DropDecimals = dropDecimals;
        }

        public string Format(TimeSpan? time)
        {
            var deltaTime = new DeltaTimeFormatter
            {
                Accuracy = Accuracy,
                DropDecimals = DropDecimals
            };
            var formattedTime = deltaTime.Format(time);
            if (time == null)
            {
                return TimeFormatConstants.DASH;
            }

            return formattedTime;
        }
    }
}
