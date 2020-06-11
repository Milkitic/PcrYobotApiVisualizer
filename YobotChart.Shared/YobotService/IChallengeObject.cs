using System;

namespace YobotChart.Shared.YobotService
{
    public interface IChallengeObject
    {
        // ReSharper disable InconsistentNaming
        long QQId { get; set; }
        long? BehalfQQId { get; }
        // ReSharper restore InconsistentNaming
        int BossNum { get; }
        DateTime ChallengeTime { get; }
        int Cycle { get; }
        int Damage { get; }
        long HealthRemain { get; }
        bool IsContinue { get; }
        string Message { get; }
    }
}