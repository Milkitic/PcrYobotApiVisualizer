using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace YobotChart.Shared.Win32.ChartFramework.StatsProviders
{
    public class GranularityModel
    {
        public GranularityModel()
        {
        }
        
        public GranularityModel(int phase, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            GranularityType = GranularityType.Total;
            SelectedUserIds = new List<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, int round, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            SelectedRound = round;
            GranularityType = GranularityType.SingleRound;
            SelectedUserIds = new List<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, DateTime date, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            SelectedDate = date;
            GranularityType = GranularityType.SingleDate;
            SelectedUserIds = new List<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, int startRound, int endRound, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            StartRound = startRound;
            EndRound = endRound;
            GranularityType = GranularityType.MultiRound;
            SelectedUserIds = new List<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityModel(int phase, DateTime startDate, DateTime endDate, IList<string> userIds = null)
        {
            SelectedPhase = phase;
            StartDate = startDate;
            EndDate = endDate;
            GranularityType = GranularityType.MultiDate;
            SelectedUserIds = new List<string>(userIds ?? Array.Empty<string>());
        }

        public GranularityType GranularityType { get; set; }

        public int SelectedPhase { get; set; }

        /// <summary>
        /// 已选周目，null默认为latest
        /// </summary>
        public int? SelectedRound { get; set; }

        /// <summary>
        /// 已选日期，null默认为latest
        /// </summary>
        public DateTime? SelectedDate { get; set; }

        /// <summary>
        /// 已选开始周目，null默认为earliest
        /// </summary>
        public int? StartRound { get; set; }

        /// <summary>
        /// 已选结束周目，null默认为latest
        /// </summary>
        public int? EndRound { get; set; }

        /// <summary>
        /// 已选开始日期，null默认为earliest
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 已选结束日期，null默认为latest
        /// </summary>
        public DateTime? EndDate { get; set; }

        public List<string> SelectedUserIds { get; set; }
    }
}