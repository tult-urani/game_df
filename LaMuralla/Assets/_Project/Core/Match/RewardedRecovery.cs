using System;
using LaMuralla.Core.Config;

namespace LaMuralla.Core.Match
{
    /// <summary>Cổng quảng cáo Core có thể kiểm thử mà không phụ thuộc SDK Unity.</summary>
    public interface IRewardedAdGateway
    {
        bool IsReady { get; }
        void Show(Action rewarded, Action closedWithoutReward);
    }

    /// <summary>Chặn hai quảng cáo đồng thời và chỉ trao thưởng từ callback reward.</summary>
    public sealed class RewardedAdCoordinator
    {
        private readonly IRewardedAdGateway _gateway;
        private long _requestId;
        private bool _inFlight;

        public RewardedAdCoordinator(IRewardedAdGateway gateway) =>
            _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));

        public bool TryShow(Action reward)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            if (_inFlight || !_gateway.IsReady) return false;

            _inFlight = true;
            long requestId = ++_requestId;
            _gateway.Show(
                () => Complete(requestId, reward),
                () => Complete(requestId, null));
            return true;
        }

        private void Complete(long requestId, Action? reward)
        {
            if (!_inFlight || requestId != _requestId) return;
            _inFlight = false;
            reward?.Invoke();
        }
    }

    /// <summary>Luật hồi máu/continue có thưởng, tính lượt theo từng trận.</summary>
    public sealed class RewardedRecovery
    {
        private readonly MatchController _match;
        private readonly EconomyDef _economy;

        public RewardedRecovery(MatchController match, EconomyDef economy)
        {
            _match = match ?? throw new ArgumentNullException(nameof(match));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        }

        public int HealUses { get; private set; }
        public int ContinueUses { get; private set; }

        public bool CanHeal =>
            _match.Phase == MatchPhase.Preparing &&
            _match.RestRemaining > 0 &&
            _match.Goal.Current < _match.Goal.Max &&
            HealUses < _economy.RewardedHealUsesPerMatch;

        public bool CanContinue =>
            _match.Phase == MatchPhase.Lost &&
            ContinueUses < _economy.RewardedContinueUsesPerMatch;

        public bool TryHeal()
        {
            if (!CanHeal) return false;
            _match.Goal.Heal(_economy.RewardedHealAmount);
            HealUses++;
            return true;
        }

        public bool TryContinue()
        {
            if (!CanContinue) return false;
            _match.Goal.Revive(_economy.RewardedContinueHealth);
            ContinueUses++;
            _match.ResumeAfterRewardedContinue();
            return true;
        }
    }
}
