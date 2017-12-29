using System;
using System.Collections.Generic;

namespace JS.Tools.DateTimes
{
    public class DateTimeRange
    {
        public DateTimeRange(DateTime start, DateTime end)
        {
            StartDateTime = start;
            EndDateTime = end;
        }

        public DateTime StartDateTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        public TimeSpan IntervalTimeSpan
        {
            get
            {
                return EndDateTime - StartDateTime;
            }
        }

        public bool IsOverlappingWith(DateTimeRange rangeB)
        {
            // Formula verified from web math proof
            return StartDateTime < rangeB.EndDateTime && EndDateTime > rangeB.StartDateTime;
        }

        public double GetOverlappingTotalMinutesWith(DateTimeRange rangeB)
        {
            var lastestStart = this.StartDateTime < rangeB.StartDateTime ? rangeB.StartDateTime : this.StartDateTime;
            var earliestFinish = this.EndDateTime < rangeB.EndDateTime ? this.EndDateTime : rangeB.EndDateTime;
            var diffTimeSpan = (earliestFinish - lastestStart);
            var overlapMinutes = diffTimeSpan.Ticks > 0 ? diffTimeSpan.TotalMinutes : 0;
            return overlapMinutes;
        }

        public DateTimeRange CalulateOverlappingDateTimeRangeWith(DateTimeRange rangeB)
        {
            if (!IsOverlappingWith(rangeB)) throw new InvalidOperationException("Ranges do not overlap");
            var latestStart = this.StartDateTime < rangeB.StartDateTime ? rangeB.StartDateTime : this.StartDateTime;
            var earliestFinish = this.EndDateTime < rangeB.EndDateTime ? this.EndDateTime : rangeB.EndDateTime;
            return new DateTimeRange(latestStart, earliestFinish);
        }

        public IList<DateTime> GetDatesInRange()
        {
            // Calculate Dates In the Week            
            var datesInSchedule = new List<DateTime>();
            var currentDate = this.StartDateTime.Date;
            while (currentDate <= this.EndDateTime.Date)
            {
                datesInSchedule.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
            return datesInSchedule;
        }
    }
}
