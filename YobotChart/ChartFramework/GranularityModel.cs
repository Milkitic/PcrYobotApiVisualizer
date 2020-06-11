using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace YobotChart.ChartFramework
{
    public class GranularityModel
    {
        public GranularityModel(int phase, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            GranularityType = GranularityType.Total;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, int round, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            SelectedRound = round;
            GranularityType = GranularityType.SingleRound;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, DateTime date, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            SelectedDate = date;
            GranularityType = GranularityType.SingleDate;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, int startRound, int endRound, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            StartRound = startRound;
            EndRound = endRound;
            GranularityType = GranularityType.MultiRound;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, DateTime startDate, DateTime endDate, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            StartDate = startDate;
            EndDate = endDate;
            GranularityType = GranularityType.MultiDate;
            SelectedUserIds = new ReadOnlyCollection<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityType GranularityType { get; }

        public int SelectedPhase { get; }

        public int? SelectedRound { get; }
        public DateTime? SelectedDate { get; }

        public int? StartRound { get; }
        public int? EndRound { get; }

        public DateTime? StartDate { get; }
        public DateTime? EndDate { get; }

        public ReadOnlyCollection<string> SelectedUserIds { get; }
    }
}