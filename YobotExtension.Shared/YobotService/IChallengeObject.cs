using System;

namespace YobotExtension.Shared.YobotService
{
    public interface IChallengeObject
    {
        long QQId { get; set; }
        long? BehalfQQId { get; }
        int BossNum { get; }
        DateTime ChallengeTime { get; }
        int Cycle { get; }
        int Damage { get; }
        long HealthRemain { get; }
        bool IsContinue { get; }
        string Message { get; }
    }
}