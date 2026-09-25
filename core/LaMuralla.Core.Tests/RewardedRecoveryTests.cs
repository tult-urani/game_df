using System;
using LaMuralla.Core.Match;
using Xunit;

namespace LaMuralla.Core.Tests
{
    public class RewardedRecoveryTests
    {
        private static MatchController New() => new(RealConfigValidateTests.Load(), seed: 7);

        [Fact]
        public void Hoi_mau_chi_duoc_mot_lan_khi_dang_nghi()
        {
            MatchController m = New();
            Assert.False(m.RewardedRecovery.CanHeal);
            m.StartNextWave();
            while (m.Phase == MatchPhase.Fighting) m.Tick(0.1);
            m.Goal.Leak(3, "adepto");

            Assert.True(m.RewardedRecovery.TryHeal());
            Assert.Equal(14, m.Goal.Current);
            Assert.False(m.RewardedRecovery.TryHeal());
            Assert.Equal(1, m.RewardedRecovery.HealUses);
        }

        [Fact]
        public void Hoi_mau_khong_vuot_tran_va_khong_tieu_luot_khi_day_mau()
        {
            MatchController m = New();
            m.StartNextWave();
            while (m.Phase == MatchPhase.Fighting) m.Tick(0.1);
            m.Goal.Heal(m.Goal.Max - m.Goal.Current);

            Assert.False(m.RewardedRecovery.TryHeal());
            Assert.Equal(20, m.Goal.Current);
            Assert.Equal(0, m.RewardedRecovery.HealUses);
        }

        [Fact]
        public void Continue_chi_duoc_mot_lan_sau_khi_thua()
        {
            MatchController m = New();
            m.StartNextWave();
            m.Tick(0.1);
            m.Goal.Leak(20, "o_capitao");

            Assert.Equal(MatchPhase.Lost, m.Phase);
            Assert.True(m.RewardedRecovery.TryContinue());
            Assert.Equal(MatchPhase.Fighting, m.Phase);
            Assert.Equal(5, m.Goal.Current);
            Assert.Equal(1, m.RewardedRecovery.ContinueUses);

            m.Goal.Leak(5, "o_capitao");
            Assert.False(m.RewardedRecovery.TryContinue());
            Assert.Equal(MatchPhase.Lost, m.Phase);
        }

        [Fact]
        public void Continue_khong_duoc_trao_khi_tran_chua_thua()
        {
            MatchController m = New();
            Assert.False(m.RewardedRecovery.TryContinue());
            Assert.Equal(0, m.RewardedRecovery.ContinueUses);
        }

        [Fact]
        public void Gateway_chi_trao_thuong_sau_callback_reward()
        {
            var gateway = new FakeRewardedAdGateway();
            var coordinator = new RewardedAdCoordinator(gateway);
            int rewards = 0;

            Assert.True(coordinator.TryShow(() => rewards++));
            gateway.CloseWithoutReward();
            Assert.Equal(0, rewards);

            Assert.True(coordinator.TryShow(() => rewards++));
            gateway.EarnReward();
            Assert.Equal(1, rewards);
        }

        [Fact]
        public void Gateway_chan_hai_quang_cao_chay_dong_thoi()
        {
            var gateway = new FakeRewardedAdGateway();
            var coordinator = new RewardedAdCoordinator(gateway);

            Assert.True(coordinator.TryShow(() => { }));
            Assert.False(coordinator.TryShow(() => { }));
            gateway.CloseWithoutReward();
            Assert.True(coordinator.TryShow(() => { }));
        }

        private sealed class FakeRewardedAdGateway : IRewardedAdGateway
        {
            private Action? _rewarded;
            private Action? _closed;

            public bool IsReady => true;

            public void Show(Action rewarded, Action closedWithoutReward)
            {
                _rewarded = rewarded;
                _closed = closedWithoutReward;
            }

            public void EarnReward()
            {
                Action? callback = _rewarded;
                Clear();
                callback?.Invoke();
            }

            public void CloseWithoutReward()
            {
                Action? callback = _closed;
                Clear();
                callback?.Invoke();
            }

            private void Clear()
            {
                _rewarded = null;
                _closed = null;
            }
        }
    }
}
