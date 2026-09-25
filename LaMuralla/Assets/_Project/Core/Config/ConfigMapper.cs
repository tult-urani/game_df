using System.Collections.Generic;
using LaMuralla.Core.Json;

namespace LaMuralla.Core.Config
{
    /// <summary>
    /// JsonValue → model typed. Chỉ ánh xạ, KHÔNG kiểm tính hợp lệ nghiệp vụ —
    /// đó là việc của ConfigValidator. Tách ra để lỗi "thiếu trường" (mapper) và
    /// lỗi "số vô lý" (validator) không lẫn vào nhau.
    ///
    /// Bỏ qua mọi khoá bắt đầu bằng `_`: đó là chú thích cho người đọc config.
    /// </summary>
    public static class ConfigMapper
    {
        public static GameConfig Map(string towersJson, string enemiesJson, string wavesJson,
                                     string economyJson, string pathJson)
        {
            JsonObject t = JsonParser.Parse(towersJson).AsObject();
            JsonObject e = JsonParser.Parse(enemiesJson).AsObject();
            JsonObject w = JsonParser.Parse(wavesJson).AsObject();
            JsonObject ec = JsonParser.Parse(economyJson).AsObject();
            JsonObject p = JsonParser.Parse(pathJson).AsObject();

            // 🔵 File map ĐƯỢC PHÉP ghi đè bốn khối: `waves`, `acts`, `hpScaling`,
            // `economy`, cộng `bossOverrides`. Map nào KHÔNG khai thì dùng nguyên bản
            // dùng chung — nhờ vậy m00 không phải chép lại một chữ số nào và cân bằng
            // của nó chứng minh được là không đổi. Xem docs/08-MAPS-ARCHITECTURE.md §2.
            JsonObject waveSrc = p.Has("waves") ? p : w;
            JsonObject actSrc = p.Has("acts") ? p : w;
            JsonObject sharedHp = w["hpScaling"].AsObject();
            JsonObject hp = p.Has("hpScaling") ? p["hpScaling"].AsObject() : sharedHp;

            return new GameConfig
            {
                MapId = p.Has("id") ? p["id"].AsString() : "m00",
                MapDisplayName = p.Has("displayName") ? p["displayName"].AsString() : "",
                Towers = MapList(t["towers"].AsArray(), MapTower),
                Enemies = MapList(e["enemies"].AsArray(), MapEnemy),
                Bosses = MapBosses(e["bosses"].AsArray(), p),
                Waves = MapList(waveSrc["waves"].AsArray(), MapWave),
                Acts = MapList(actSrc["acts"].AsArray(), MapAct),
                HpScaling = MapHpScaling(hp, sharedHp),
                Economy = MapEconomy(ec, p),
                Path = MapPath(p),
                // Không dùng `Opt(...) ?? false`: thiếu cờ này thì mặc định im lặng
                // thành "không tích luỹ" và luật 18 sẽ XANH cho một config chưa hề
                // khai báo lập trường. Thiếu = lỗi.
                AbilitiesAreCumulative = t["_upgradeRules"].AsObject()["abilitiesAreCumulative"].AsBool(),
                SpawnIntervalSec = w["spawnIntervalSec"].AsNumber(),
                RestBetweenWavesSec = w["restBetweenWavesSec"].AsNumber(),
            };
        }

        private static Match.Vec2 MapVec2(JsonObject o) =>
            new(o["x"].AsNumber(), o["y"].AsNumber());

        private static PathDef MapPath(JsonObject o)
        {
            JsonObject units = o["units"].AsObject();
            JsonObject viewport = units["viewportUnits"].AsObject();
            JsonObject ui = o["ui"].AsObject();

            return new PathDef
            {
                Lanes = MapList(o["lanes"].AsArray(), l => new LaneDef
                {
                    Id = l["id"].AsString(),
                    Waypoints = MapList(l["waypoints"].AsArray(), MapVec2),
                    DeclaredLengthUnits = l["lengthUnits"].AsNumber(),
                    SpawnPoint = MapVec2(l["spawnPoint"].AsObject()),
                    GoalPoint = MapVec2(l["goalPoint"].AsObject()),
                }),
                Slots = MapList(o["slots"].AsArray(), s => new SlotDef
                {
                    Id = s["id"].AsString(),
                    Type = s["type"].AsString(),
                    Position = MapVec2(s),
                }),
                PixelsPerUnit = units["pixelsPerUnit"].AsNumber(),
                CameraOrthographicSize = units["cameraOrthographicSize"].AsNumber(),
                ViewportWidthUnits = viewport["width"].AsNumber(),
                ViewportHeightUnits = viewport["height"].AsNumber(),
                DesignWidthPt = ui["designWidthPt"].AsNumber(),
                MinTouchTargetPt = ui["minTouchTargetPt"].AsNumber(),
                RadialMenuOuterRadiusPt = ui["radialMenuOuterRadiusPt"].AsNumber(),
            };
        }

        private static List<T> MapList<T>(JsonArray arr, System.Func<JsonObject, T> f)
        {
            var list = new List<T>(arr.Count);
            foreach (JsonValue v in arr.Items) list.Add(f(v.AsObject()));
            return list;
        }

        private static TowerDef MapTower(JsonObject o) => new()
        {
            Id = o["id"].AsString(),
            DisplayName = o["displayName"].AsString(),
            SlotType = o["slotType"].AsString(),
            // Vắng `maxInstances` = không giới hạn. Ở đây `?? 0` là ĐÚNG (khác với
            // abilitiesAreCumulative): 5/6 tướng không khai trường này, và "không
            // khai" có nghĩa rõ ràng — thoải mái đặt bao nhiêu con cũng được.
            MaxInstances = o.Opt("maxInstances") is { } m ? m.AsInt() : 0,
            Levels = MapList(o["levels"].AsArray(), l => new TowerLevel
            {
                Level = l["level"].AsInt(),
                Cost = l["cost"].AsInt(),
                Damage = l["damage"].AsNumber(),
                AttackRate = l["attackRate"].AsNumber(),
                Range = l["range"].AsNumber(),
                // Vắng = 0 = không lan. Chỉ D10S khai trường này.
                SplashRadius = l.Opt("splashRadius") is { } sr ? sr.AsNumber() : 0,
            }),
            Abilities = MapList(o["abilities"].AsArray(), MapAbility),
        };

        private static AbilityDef MapAbility(JsonObject o)
        {
            var ps = new Dictionary<string, JsonValue>();
            JsonValue? raw = o.Opt("params");
            if (raw != null)
                foreach (KeyValuePair<string, JsonValue> kv in raw.AsObject().Items)
                    ps[kv.Key] = kv.Value;

            return new AbilityDef
            {
                Id = o["id"].AsString(),
                UnlockLevel = o["unlockLevel"].AsInt(),
                Trigger = o["trigger"].AsString(),
                CooldownSec = o.Opt("cooldownSec")?.AsNumber() ?? 0,
                Params = ps,
            };
        }

        private static EnemyDef MapEnemy(JsonObject o) => new()
        {
            Id = o["id"].AsString(),
            DisplayName = o["displayName"].AsString(),
            BaseHp = o["baseHp"].AsInt(),
            Speed = o["speed"].AsNumber(),
            BaseBounty = o["baseBounty"].AsInt(),
            LeakDamage = o["leakDamage"].AsInt(),
            SlowResistPercent = o["slowResistPercent"].AsInt(),
        };

        private static BossDef MapBoss(JsonObject o) => new()
        {
            Id = o["id"].AsString(),
            DisplayName = o["displayName"].AsString(),
            Speed = o["speed"].AsNumber(),
            LeakDamage = o["leakDamage"].AsInt(),
            SlowResistPercent = o["slowResistPercent"].AsInt(),
            Appearances = MapList(o["appearances"].AsArray(), a => new BossAppearance
            {
                Wave = a["wave"].AsInt(),
                Hp = a["hp"].AsInt(),
                Bounty = a["bounty"].AsInt(),
            }),
        };

        /// <summary>
        /// `spawns` là MẢNG `[{enemy, count, lane, delaySec}]`.
        ///
        /// Dạng object cũ `{adepto: 8, tifoso: 6}` KHÔNG còn được chấp nhận — cố ý.
        /// Nhận cả hai thì mọi chỗ đọc phải rẽ nhánh, và nhánh đó là nơi bug im lặng
        /// sinh ra (cùng lập luận với `lanes` ở docs/08 §2.2(a)). Config cũ được
        /// chuyển một lần bằng script, không đỡ ở runtime.
        /// </summary>
        private static WaveDef MapWave(JsonObject o)
        {
            if (o["spawns"] is not JsonArray arr)
                throw new ConfigException(
                    $"wave {o["wave"].AsInt()}: `spawns` phải là MẢNG "
                    + "[{enemy, count, lane}] — dạng object cũ đã bỏ, xem docs/08 §2.2(b)");

            var spawns = new List<SpawnGroup>(arr.Count);
            foreach (JsonValue v in arr.Items)
            {
                JsonObject g = v.AsObject();
                spawns.Add(new SpawnGroup
                {
                    Enemy = g["enemy"].AsString(),
                    Count = g["count"].AsInt(),
                    Lane = g.Opt("lane")?.AsString() ?? "L1",
                    DelaySec = g.Opt("delaySec")?.AsNumber() ?? 0,
                });
            }

            return new WaveDef
            {
                Wave = o["wave"].AsInt(),
                Spawns = spawns,
                Bosses = MapBossSpawns(o),
            };
        }

        /// <summary>`bosses: [{id, lane}]`. Wave thường thì bỏ khoá này.</summary>
        private static List<BossSpawn> MapBossSpawns(JsonObject o)
        {
            var list = new List<BossSpawn>();
            if (o.Opt("bosses") is not JsonArray arr) return list;
            foreach (JsonValue v in arr.Items)
            {
                JsonObject b = v.AsObject();
                list.Add(new BossSpawn { Id = b["id"].AsString(), Lane = b.Opt("lane")?.AsString() ?? "L1" });
            }
            return list;
        }

        private static ActDef MapAct(JsonObject o)
        {
            JsonArray range = o["waves"].AsArray();
            return new ActDef
            {
                Id = o["id"].AsInt(),
                FirstWave = range[0].AsInt(),
                LastWave = range[1].AsInt(),
                BountyMultiplier = o["bountyMultiplier"].AsNumber(),
            };
        }

        private static HpScaling MapHpScaling(JsonObject o, JsonObject shared)
        {
            JsonObject milestoneSource = o.Has("milestones") ? o : shared;
            return new HpScaling
            {
                Base = o["base"].AsNumber(),
                GrowthPerWave = o["growthPerWave"].AsNumber(),
                Milestones = MapList(milestoneSource["milestones"].AsArray(), m => new HpMilestone
                {
                    Wave = m["wave"].AsInt(),
                    Multiplier = m["multiplier"].AsNumber(),
                }),
            };
        }

        /// <summary>
        /// Boss dùng chung từ `enemies.json`, nhưng file map được vá `slowResistPercent`
        /// và `appearances` (máu/thưởng theo wave) — hai thứ PHẢI đổi theo map vì độ dài
        /// tuyến khác nhau thì hoả lực dồn lên boss khác nhau.
        /// </summary>
        private static List<BossDef> MapBosses(JsonArray arr, JsonObject map)
        {
            List<BossDef> bosses = MapList(arr, MapBoss);
            if (!map.Has("bossOverrides")) return bosses;

            JsonObject ov = map["bossOverrides"].AsObject();
            foreach (BossDef b in bosses)
            {
                if (!ov.Has(b.Id)) continue;
                JsonObject o = ov[b.Id].AsObject();
                if (o.Has("slowResistPercent")) b.SlowResistPercent = o["slowResistPercent"].AsInt();
                if (o.Has("appearances"))
                    b.Appearances = MapList(o["appearances"].AsArray(), a => new BossAppearance
                    {
                        Wave = a["wave"].AsInt(),
                        Hp = a["hp"].AsInt(),
                        Bounty = a["bounty"].AsInt(),
                    });
            }
            return bosses;
        }

        /// <summary>
        /// Kinh tế dùng chung, cho phép map vá bốn trường. CỐ Ý chỉ bốn: `balanceModel`,
        /// làm tròn, ngưỡng sao... là luật chung của trò chơi, để map sửa được thì mỗi
        /// map thành một trò khác.
        /// </summary>
        private static EconomyDef MapEconomy(JsonObject o, JsonObject map)
        {
            EconomyDef ec = MapEconomy(o);
            if (!map.Has("economy")) return ec;

            JsonObject m = map["economy"].AsObject();
            if (m.Has("startingCash")) ec.StartingCash = m["startingCash"].AsInt();
            if (m.Has("goalHealth")) ec.GoalHealth = m["goalHealth"].AsInt();
            if (m.Has("fieldSlots")) ec.FieldSlots = m["fieldSlots"].AsInt();
            // Map có hai lối ra được phép cho người chơi hai vị trí thủ môn. Dibu
            // vẫn có maxInstances=1, nên đây là một lựa chọn chiến thuật giữa hai
            // cầu môn chứ không phải tăng gấp đôi sức thủ.
            if (m.Has("goalkeeperSlots")) ec.GoalkeeperSlots = m["goalkeeperSlots"].AsInt();
            return ec;
        }

        private static EconomyDef MapEconomy(JsonObject o)
        {
            JsonObject recovery = o["rewardedRecovery"].AsObject();
            return new EconomyDef
            {
                StartingCash = o["startingCash"].AsInt(),
                GoalHealth = o["goalHealth"].AsInt(),
                SlowCapPercent = o["slowCapPercent"].AsInt(),
                SkipBonusPerSecond = o["skipBonusPerSecond"].AsInt(),
                SellRefundRatio = o["sellRefundRatio"].AsNumber(),
                FieldSlots = o["fieldSlots"].AsInt(),
                GoalkeeperSlots = o["goalkeeperSlots"].AsInt(),
                RoundingMode = o["roundingMode"].AsString(),
                RewardedHealAmount = recovery["healAmount"].AsInt(),
                RewardedHealUsesPerMatch = recovery["healUsesPerMatch"].AsInt(),
                RewardedContinueHealth = recovery["continueHealth"].AsInt(),
                RewardedContinueUsesPerMatch = recovery["continueUsesPerMatch"].AsInt(),
            };
        }
    }
}
