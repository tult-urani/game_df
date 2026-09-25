using System;
using System.Collections.Generic;

namespace LaMuralla.Core.Match
{
    /// <summary>Ai gây đòn kết liễu, và lên con nào. Phát ra khi quân chết —
    /// KHÔNG phải lệnh cộng tiền, chỉ là tin báo.</summary>
    public readonly struct KillInfo
    {
        /// <summary>Con quân đã chết.</summary>
        public int EnemyId { get; }

        /// <summary>Ô của tướng gây đòn kết liễu. `null` = không phải tướng nào
        /// giết (quân lọt lưới, hoặc Dibu cản — `can_pha` KHÔNG rơi tiền).</summary>
        public string? KillerSlotId { get; }

        /// <summary>Sát thương THỰC vào con này ở đòn cuối, đã cắt phần thừa.</summary>
        public double LethalDamage { get; }

        /// <summary>Phần đánh thừa, đã vứt. Giữ lại để đo `etaWasteFactor` = 0.75 ở
        /// M1 — nó là một trong ba hệ số bịa của mô hình cân bằng.</summary>
        public double Overkill { get; }

        public KillInfo(int enemyId, string? killerSlotId, double lethalDamage, double overkill)
        {
            EnemyId = enemyId; KillerSlotId = killerSlotId;
            LethalDamage = lethalDamage; Overkill = overkill;
        }
    }

    public readonly struct DamageResult
    {
        /// <summary>Đòn này có phải đòn kết liễu không. Đúng MỘT đòn trong đời một
        /// con quân trả về true.</summary>
        public bool Killed { get; }

        /// <summary>Sát thương thực đã trừ. Luôn ≤ máu còn lại trước đòn.</summary>
        public double Dealt { get; }

        /// <summary>Phần vượt quá máu còn lại. Vứt đi (`01` §8 **FM-07**).</summary>
        public double Overkill { get; }

        /// <summary>Đòn đánh vào con đã chết. Không phải lỗi — đạn đang bay thì
        /// mục tiêu chết là chuyện thường. Chỉ là không có gì xảy ra.</summary>
        public bool TargetAlreadyDead { get; }

        public DamageResult(bool killed, double dealt, double overkill, bool alreadyDead)
        {
            Killed = killed; Dealt = dealt; Overkill = overkill; TargetAlreadyDead = alreadyDead;
        }

        public static DamageResult Ignored => new(false, 0, 0, true);
    }

    /// <summary>
    /// 🔴 NƠI DUY NHẤT QUYẾT ĐỊNH "AI GÂY ĐÒN KẾT LIỄU" — luật bất khả xâm phạm #2,
    /// docs/05 §3.
    ///
    /// Vì sao lớp này GIỮ máu thay vì để mỗi con quân tự giữ:
    ///
    ///   Nếu quân tự giữ máu thì bất kỳ ai cũng `enemy.Hp -= dmg` được, và ai cũng
    ///   tự kết luận "tôi giết nó". Hai tướng cùng bắn phát cuối vào con còn 10 máu:
    ///       Batigol : 10 − 90 = chết → tiền
    ///       La Pulga: 10 − 56 = chết → tiền   ← lần thứ hai
    ///   Một con, trả tiền hai lần. Đó là FM-07 (`01` §8 **FM-07**): kinh tế vỡ sau ~10
    ///   wave, và bug này KHÔNG tái hiện được vì nó phụ thuộc thứ tự đạn chạm.
    ///
    ///   Ở đây, tướng KHÔNG CÓ CÁCH NÀO chạm vào máu — nó không cầm tham chiếu tới
    ///   máu, chỉ gọi được <see cref="Apply"/>. Luật được ép bằng khả năng truy cập,
    ///   không bằng kỷ luật của người viết.
    ///
    /// 🔴 KHÔNG CỘNG TIỀN Ở ĐÂY. docs/05 bảng §3 ghi rõ cột "không làm" của module
    /// này là "Cộng tiền (phát event)". Lớp này phát <see cref="EnemyKilled"/>;
    /// MatchController nghe rồi mới gọi EconomyService. Nếu gọi thẳng thì luật #1
    /// (ví có một cổng vào duy nhất) và luật #2 giẫm lên nhau, và không ai còn biết
    /// tiền được cộng từ đâu.
    /// </summary>
    public sealed class DamageSystem
    {
        private readonly Dictionary<int, double> _hp = new Dictionary<int, double>();

        /// <summary>Phát ĐÚNG MỘT LẦN cho mỗi con quân, tại đòn kết liễu.</summary>
        public event Action<KillInfo>? EnemyKilled;

        /// <summary>Số con còn sống. Dùng để biết wave đã dọn xong chưa.</summary>
        public int AliveCount => _hp.Count;

        /// <summary>Đưa một con quân vào hệ. Gọi lúc spawn.</summary>
        public void Register(int enemyId, double maxHp)
        {
            if (maxHp <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHp), maxHp, "máu khởi tạo phải > 0");
            if (_hp.ContainsKey(enemyId))
                throw new InvalidOperationException(
                    $"quân `{enemyId}` đã đăng ký rồi — id phải duy nhất trong một trận, " +
                    "trùng id thì hai con dùng chung một thanh máu");
            _hp[enemyId] = maxHp;
        }

        public bool IsAlive(int enemyId) => _hp.ContainsKey(enemyId);

        public double HpOf(int enemyId) => _hp.TryGetValue(enemyId, out double h) ? h : 0;

        /// <summary>
        /// Gây sát thương. Đây là cách DUY NHẤT máu thay đổi.
        /// </summary>
        /// <param name="enemyId">Con quân.</param>
        /// <param name="amount">Sát thương định gây. Phần vượt máu còn lại bị vứt.</param>
        /// <param name="killerSlotId">Ô của tướng bắn. `null` nếu không do tướng nào
        /// (Dibu cản, hoặc quân tự bị xoá) — lúc đó không ai được ghi công.</param>
        public DamageResult Apply(int enemyId, double amount, string? killerSlotId)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "sát thương phải ≥ 0");

            // Con đã chết: đạn tới muộn. KHÔNG ném, KHÔNG phát event lần hai.
            // Đây chính là cửa chặn FM-07 — mọi đòn sau đòn kết liễu đều rơi vào đây.
            if (!_hp.TryGetValue(enemyId, out double hp))
                return DamageResult.Ignored;

            if (amount < hp)
            {
                _hp[enemyId] = hp - amount;
                return new DamageResult(false, amount, 0, false);
            }

            // Đòn kết liễu. Xoá TRƯỚC khi phát event: nếu người nghe gọi ngược lại
            // Apply/IsAlive trên con này (ví dụ hiệu ứng dây chuyền), nó phải thấy
            // con đã chết — nếu không thì event có thể phát hai lần.
            _hp.Remove(enemyId);

            double dealt = hp;                  // chỉ trừ được đúng phần máu còn lại
            double overkill = amount - hp;      // phần thừa: vứt (`01` §8 **FM-07**)

            EnemyKilled?.Invoke(new KillInfo(enemyId, killerSlotId, dealt, overkill));
            return new DamageResult(true, dealt, overkill, false);
        }

        /// <summary>
        /// Xoá quân không qua sát thương — quân lọt lưới, hoặc Dibu `can_pha`.
        ///
        /// Tách khỏi <see cref="Apply"/> vì đây KHÔNG phải cái chết có người ghi công:
        /// `can_pha` khai rõ `dropsBounty: false`. Dùng Apply với sát thương vô cực
        /// thì nó sẽ phát KillInfo và MatchController sẽ trả tiền cho một cú cản —
        /// tức người chơi được thưởng vì để quân tới tận cầu môn.
        /// </summary>
        /// <returns>false nếu con đó đã chết rồi.</returns>
        public bool RemoveWithoutKill(int enemyId) => _hp.Remove(enemyId);
    }
}
