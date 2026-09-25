#!/usr/bin/env python3
"""Bảng tính cân bằng M0 (roadmap việc 2b) — dẫn xuất wave từ mô hình, không gõ tay.

Vì sao tồn tại: docs/04 §4b tuyên bố mô hình giải tích đã vượt khả năng tính tay
từ vòng 3. Ba lý do, và công cụ này giải được ba lý do rưỡi:

  1. `τ` phụ thuộc SỐ QUÂN trong wave      → tính lại cho từng wave
  2. Reference Build đổi theo NGÂN SÁCH    → mô phỏng dòng tiền, chọn build từng wave
  3. Ba hệ số bịa nhân nhau (η × σ × τ)    → KHÔNG giải được, vẫn là giả định
  4. `chord = 2R` sai                      → GIẢI XONG: đo chord thật từ path.json

Điểm 4 là lý do công cụ này khác một bảng Excel: nó lấy mẫu spline và đo đoạn
đường THẬT nằm trong vòng tròn tầm của từng ô, thay vì giả định 2R.

    python3 tools/balance_sim.py            # báo cáo
    python3 tools/balance_sim.py --solve     # dò hpMultiplier cho từng act
"""

import argparse
import json
import math
import sys
from decimal import Decimal, ROUND_HALF_UP
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from path_check import catmull_rom, arc_lengths, chord_for  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent


def load(name):
    with open(ROOT / "config" / f"{name}.json", encoding="utf-8") as f:
        return json.load(f)

DEFAULT_MAP = "m00-la-muralla"


def load_map(map_id=None):
    """File map trong `config/maps/`. `config/path.json` cũ giờ là `m00-la-muralla.json`.

    Chấp nhận cả `m01` lẫn `m01-el-potrero`: gõ tay id ngắn tiện hơn, còn tên file
    mang cả nhãn cho người đọc thư mục.
    """
    map_id = map_id or DEFAULT_MAP
    d = ROOT / "config" / "maps"
    f = d / f"{map_id}.json"
    if not f.exists():
        hits = sorted(d.glob(f"{map_id}-*.json"))
        if not hits:
            have = ", ".join(sorted(x.stem for x in d.glob("*.json")))
            raise SystemExit(f"không có map `{map_id}` trong {d} — hiện có: {have}")
        f = hits[0]
    with open(f, encoding="utf-8") as fh:
        return json.load(fh)



def rnd(x):
    """half_up theo economy.json → roundingMode. KHÔNG dùng round() (banker's)."""
    return int(Decimal(str(x)).quantize(Decimal("1"), rounding=ROUND_HALF_UP))


# ── Hình học: chord THẬT cho mỗi (ô, tầm) ──────────────────────────────────

class Geometry:
    def __init__(self, path_cfg):
        pts = [(w["x"], w["y"]) for w in path_cfg["lanes"][0]["waypoints"]]   # tuyến đầu; đa tuyến xem docs/08 §8
        self.curve = catmull_rom(pts)
        self.length, self.cum = arc_lengths(self.curve)
        self.slots = [s for s in path_cfg["slots"] if s["type"] == "field"]
        self._cache = {}

    def chord(self, slot_id, radius):
        key = (slot_id, radius)
        if key not in self._cache:
            s = next(x for x in self.slots if x["id"] == slot_id)
            cov, _ = chord_for(self.curve, self.cum, (s["x"], s["y"]), radius)
            self._cache[key] = cov
        return self._cache[key]


# ── Kinh tế: dòng tiền tích luỹ ─────────────────────────────────────────────

def hp_mult(wave, waves_cfg):
    """Hệ số HP cuối cùng = đường cong trơn × milestone gần nhất đã đi qua."""
    h = waves_cfg["hpScaling"]
    milestone = 1.0
    for item in h.get("milestones", []):
        if wave >= item["wave"]:
            milestone = item["multiplier"]
        else:
            break
    return h["base"] * h["growthPerWave"] ** (wave - 1) * milestone


def act_of(wave, acts):
    for a in acts:
        if a["waves"][0] <= wave <= a["waves"][1]:
            return a
    raise ValueError(f"wave {wave} không thuộc act nào")


def spawn_counts(spec):
    """Gộp schema spawn theo lane thành tổng số từng loại cho mô hình M0 một tuyến."""
    counts = {}
    for spawn in spec["spawns"]:
        enemy_id = spawn["enemy"]
        counts[enemy_id] = counts.get(enemy_id, 0) + spawn["count"]
    return counts


def boss_ids(spec):
    """Trả danh sách boss từ schema `bosses` hiện tại."""
    return [boss["id"] for boss in spec.get("bosses", [])]


def wave_income(wave, waves_cfg, enemies, econ):
    """Tiền một wave sinh ra cho người chơi tối ưu (giết sạch, skip hết nghỉ)."""
    a = act_of(wave, waves_cfg["acts"])
    spec = next(w for w in waves_cfg["waves"] if w["wave"] == wave)
    base = {e["id"]: e["baseBounty"] for e in enemies["enemies"]}
    counts = spawn_counts(spec)
    total = sum(rnd(base[eid] * a["bountyMultiplier"]) * n
                for eid, n in counts.items() if n)
    for boss_id in boss_ids(spec):
        boss = next(b for b in enemies["bosses"] if b["id"] == boss_id)
        total += next(ap["bounty"] for ap in boss["appearances"] if ap["wave"] == wave)
    ov = waves_cfg["waveClearBonus"]["overrides"]
    total += ov[str(wave)] if str(wave) in ov else 20 + 5 * (wave - 1)
    total += econ["skipBonusPerSecond"] * waves_cfg["restBetweenWavesSec"]  # chơi tối ưu
    return total


def cash_before(wave, waves_cfg, enemies, econ):
    return econ["startingCash"] + sum(
        wave_income(w, waves_cfg, enemies, econ) for w in range(1, wave))


# ── Reference Build: tham lam theo sát thương/Peso, có ràng buộc ô ──────────

def build_for(cash, geo, towers, econ, wave_spread, use_arbitro, focus=False):
    """Chọn build tốt nhất trong ngân sách. Trả (build, cash_còn).

    build = [(tower, level_idx, slot_id)]
    Tham lam: mỗi bước chọn hành động có (Δsát thương / ΔPeso) cao nhất.
    """
    bm = econ["balanceModel"]
    fld = {t["id"]: t for t in towers["towers"] if t["slotType"] == "field"}
    dps_towers = {k: v for k, v in fld.items() if v["levels"][0].get("attackRate")}
    slots = [s["id"] for s in geo.slots]
    placed = {}          # slot_id → [tower_id, level_idx]
    spent = 0

    # Árbitro là LỰA CHỌN, không phải mặc định. Ép mua nó = giả định người chơi
    # tối ưu mua nó — đúng cái đang cần chứng minh. arb_level=None nghĩa là không mua.
    if use_arbitro is not None:
        arb = fld["el_arbitro"]
        cum = sum(arb["levels"][i]["cost"] for i in range(use_arbitro + 1))
        if cum > cash:
            return None, None
        best = max(slots, key=lambda s: geo.chord(s, arb["levels"][use_arbitro]["range"]))
        placed[best] = ["el_arbitro", use_arbitro]
        spent += cum

    def d10s_targets(lvl):
        """Số mục tiêu D10S chạm, theo BÁN KÍNH LAN của cấp đó.

        Trước B-01 đây là hằng số phẳng `d10sAverageTargetsHit = 3` cho mọi cấp.
        Được, vì cộng dồn khiến Lv2 và Lv3 cùng lan 1.7. B-01 tách chúng ra:
        Lv1 lan 1.2 · Lv2 lan 1.7 · Lv3 lan 1.2 (mất `cu_cham_thien_tai`).
        Hằng số phẳng thì Lv2 và Lv3 trông giống hệt nhau — tức sim MÙ trước
        chính đánh đổi mà B-01 tạo ra.

        Tuyến tính theo bán kính, không phải theo diện tích: chú thích của
        `_d10sNote` đã nói quân đi HÀNG DỌC trên đường, không đứng cụm tròn.
        Vòng tròn cắt một hàng dọc thì số quân chạm tỉ lệ với ĐƯỜNG KÍNH.
        Cụm tròn mới tỉ lệ với diện tích (r²).

        Neo đọc từ config (`d10sTargetsHitAtSplashRadius`), KHÔNG gõ cứng ở đây:
        hằng số 3 cũ áp cho mọi cấp trong khi các cấp có bán kính khác nhau — nó
        tự mâu thuẫn, và không ai ghi nó hiệu chuẩn cho bán kính nào. Neo phải là
        một tuyên bố đọc được, không phải một con số nấp trong hàm.
        """
        r = fld["d10s"]["levels"][lvl]["splashRadius"]
        return bm["d10sAverageTargetsHit"] * r / bm["d10sTargetsHitAtSplashRadius"]

    def dmg(tower_id, lvl, slot_id):
        t = fld[tower_id]
        if tower_id not in dps_towers:
            return 0.0
        L = t["levels"][lvl]
        dps = L["damage"] * L["attackRate"]
        if tower_id == "d10s" and not focus:
            dps *= d10s_targets(lvl)                # lan rộng hơn = chạm nhiều hơn
        ch = geo.chord(slot_id, L["range"])
        if ch <= 0:
            return 0.0
        sigma = bm["sigmaByTowerLevel"][lvl]
        if focus:
            return dps * sigma * ch / 0.4           # boss đi một mình, tốc 0.4
        return dps * sigma * (ch + wave_spread) / bm["vBarAverageSpeed"]

    def total():
        return sum(dmg(t, l, s) for s, (t, l) in placed.items())

    while True:
        cur, best, best_ratio = total(), None, 0.0
        for s in slots:
            if s in placed:
                tid, lvl = placed[s]
                if lvl >= 2 or tid not in dps_towers:
                    continue
                cost = fld[tid]["levels"][lvl + 1]["cost"]
                if spent + cost > cash:
                    continue
                placed[s] = [tid, lvl + 1]
                gain = total() - cur
                placed[s] = [tid, lvl]
                cand = (s, tid, lvl + 1, cost)
            else:
                for tid in dps_towers:
                    cost = fld[tid]["levels"][0]["cost"]
                    if spent + cost > cash:
                        continue
                    placed[s] = [tid, 0]
                    gain = total() - cur
                    del placed[s]
                    r = gain / cost
                    if r > best_ratio:
                        best_ratio, best = r, (s, tid, 0, cost)
                continue
            r = gain / cost if cost else 0
            if r > best_ratio:
                best_ratio, best = r, cand
        # BÁN & XÂY LẠI — nước đi này là ý đồ Act 3 ("ô đặt hết, phải bán và
        # xây lại", waves.json). Thiếu nó, sim kẹt ở Batigol maxed (225 sát
        # thương/ô) trong khi người chơi thật đổi sang La Pulga (592/ô) và còn
        # dư tiền. Bỏ qua = ĐÁNH GIÁ THẤP phòng ngự cuối game ~2.6× mỗi ô đổi.
        refund = econ["sellRefundRatio"]
        for s, (tid, lvl) in list(placed.items()):
            if tid not in dps_towers or lvl < 2:
                continue                      # chỉ bán tướng đã maxed mà vẫn yếu
            back = refund * sum(fld[tid]["levels"][i]["cost"] for i in range(lvl + 1))
            for new in dps_towers:
                if new == tid:
                    continue
                buy = sum(fld[new]["levels"][i]["cost"] for i in range(3))
                net = buy - back
                if net <= 0 or spent + net > cash:
                    continue
                placed[s] = [new, 2]
                gain = total() - cur
                placed[s] = [tid, lvl]
                r = gain / net
                if r > best_ratio:
                    best_ratio, best = r, (s, new, 2, net)

        if not best:
            break
        s, tid, lvl, cost = best
        placed[s] = [tid, lvl]
        spent += cost
    return placed, cash - spent


# ── Mô hình sát thương ──────────────────────────────────────────────────────

def tau_for(placed, n, wave_dur, econ):
    """τ từ cấp Árbitro + số quân. Trả 1.0 nếu không có Árbitro."""
    bm = econ["balanceModel"]
    lvl = next((l for t, l in placed.values() if t == "el_arbitro"), None)
    if lvl is None:
        return 1.0
    return bm["tauByArbitroLevel"][lvl + 1]


def wdb(placed, geo, towers, econ, spread):
    bm = econ["balanceModel"]
    fld = {t["id"]: t for t in towers["towers"]}
    tot = 0.0
    for slot_id, (tid, lvl) in placed.items():
        t = fld[tid]
        L = t["levels"][lvl]
        if not L.get("attackRate"):
            continue
        dps = L["damage"] * L["attackRate"]
        if tid == "d10s":
            # Cùng công thức với build_for.d10s_targets. Hai chỗ PHẢI khớp, nếu
            # không thì sim CHỌN build theo một mô hình rồi CHẤM ĐIỂM nó bằng mô
            # hình khác — và bảng cân bằng nói về một trận đấu không tồn tại.
            dps *= bm["d10sAverageTargetsHit"] * L["splashRadius"] / bm["d10sTargetsHitAtSplashRadius"]
        ch = geo.chord(slot_id, L["range"])
        tot += dps * bm["sigmaByTowerLevel"][lvl] * (ch + spread) / bm["vBarAverageSpeed"]
    return tot * bm["etaWasteFactor"]


def focus_damage(placed, geo, towers, econ, boss_speed):
    """Hoả lực tập trung lên boss đi một mình. D10S KHÔNG nhân số mục tiêu."""
    bm = econ["balanceModel"]
    fld = {t["id"]: t for t in towers["towers"]}
    tot = 0.0
    for slot_id, (tid, lvl) in placed.items():
        t = fld[tid]
        L = t["levels"][lvl]
        if not L.get("attackRate"):
            continue
        dps = L["damage"] * L["attackRate"]          # ← không ×3
        ch = geo.chord(slot_id, L["range"])
        tot += dps * bm["sigmaByTowerLevel"][lvl] * ch / boss_speed
    return tot * bm["etaWasteFactor"]


def analyse(wave, cfg, geo, use_arbitro=True):
    waves_cfg, enemies, towers, econ = cfg
    bm = econ["balanceModel"]
    a = act_of(wave, waves_cfg["acts"])
    spec = next(w for w in waves_cfg["waves"] if w["wave"] == wave)
    emap = {e["id"]: e for e in enemies["enemies"]}

    counts = spawn_counts(spec)
    n = sum(counts.values())
    spread = min((n - 1) * bm["spawnIntervalSec"] * bm["vBarAverageSpeed"], geo.length)
    hp = sum(rnd(emap[eid]["baseHp"] * hp_mult(wave, waves_cfg)) * cnt
             for eid, cnt in counts.items() if cnt)

    cash = cash_before(wave, waves_cfg, enemies, econ)

    # Người chơi tối ưu thử MỌI phương án Árbitro (không mua / Lv1 / Lv2 / Lv3)
    # và giữ cái cho sát thương cuối cao nhất. Đây là chỗ "Árbitro có phải thuế
    # không" được TRẢ LỜI thay vì được giả định.
    opts = [None] + ([0, 1, 2] if use_arbitro else [])
    best = None
    for opt in opts:
        pl, left = build_for(cash, geo, towers, econ, spread, opt)
        if pl is None:
            continue
        t = tau_for(pl, n, 0, econ)
        d = wdb(pl, geo, towers, econ, spread) * t
        if best is None or d > best[0]:
            best = (d, pl, left, t, opt)
    dmg, placed, left, tau, arb_opt = best
    head = dmg / hp if hp else float("inf")

    boss = None
    bosses = boss_ids(spec)
    if bosses:
        boss_defs = [next(x for x in enemies["bosses"] if x["id"] == boss_id)
                     for boss_id in bosses]
        # Hai boss cùng wave là hai thanh máu mà đội hình phải xử lý. Chấm một
        # con rồi bỏ quên con còn lại làm headroom W15/W20 cao giả đúng 2 lần.
        bhp = sum(next(ap["hp"] for ap in b["appearances"] if ap["wave"] == wave)
                  for b in boss_defs)
        b = boss_defs[0]
        # kháng chậm: τ chỉ ăn phần lọt qua kháng
        btau = 1 + (tau - 1) * (1 - b["slowResistPercent"] / 100)
        crowd = focus_damage(placed, geo, towers, econ, b["speed"]) * btau
        # Người chơi BIẾT W10/W20 có boss → sẽ đổi build. Chấm boss bằng build
        # tối-ưu-đám-đông rồi kêu ❌ là kêu sai: build đó THUA boss LÀ CƠ CHẾ
        # (docs/04 §3.3 — D10S mất ×3). Thước đo đúng: build CÓ TÍNH boss có
        # thắng không. Khoảng cách giữa hai con số = mức độ boss ép đổi đội hình.
        aware_pl, _ = build_for(cash, geo, towers, econ, spread, None, focus=True)
        aware = focus_damage(aware_pl, geo, towers, econ, b["speed"]) * btau
        boss = {"count": len(bosses), "hp": bhp, "dmg": aware, "head": aware / bhp,
                "crowd_head": crowd / bhp, "aware_build": aware_pl}
    return {"wave": wave, "act": a["id"], "n": n, "hp": hp, "cash": cash,
            "left": left, "placed": placed, "tau": tau, "dmg": dmg,
            "head": head, "boss": boss, "spread": spread, "arb": arb_opt}


def fmt_build(placed):
    order = ["la_pulga", "d10s", "batigol", "el_arbitro"]
    short = {"la_pulga": "Pulga", "d10s": "D10S",
             "batigol": "Bati", "el_arbitro": "Árb"}
    c = {}
    for tid, lvl in placed.values():
        c[(tid, lvl)] = c.get((tid, lvl), 0) + 1
    return " ".join(f"{short[t]}{l+1}×{n}" for (t, l), n in
                    sorted(c.items(), key=lambda kv: (order.index(kv[0][0]), kv[0][1])))


def target_headroom(wave, acts):
    """Đường cong headroom mục tiêu: GIẢM dần trong mỗi act = căng thẳng TĂNG dần.

    Hệ số act dạng bậc thang không làm được điều này — nó cho cả 7 wave cùng một
    con số, nên đầu act luôn vỡ (máu nhảy bậc, tiền chưa theo kịp) và cuối act
    luôn quá dễ (tiền dồn, máu đứng yên). Đo được: Act 2 biên độ 1.81×.
    """
    a = act_of(wave, acts)
    lo_w, hi_w = a["waves"]
    t = (wave - lo_w) / max(1, hi_w - lo_w)          # 0 đầu act → 1 cuối act
    start, end = {1: (1.40, 1.15), 2: (1.38, 1.12), 3: (1.35, 1.10)}[a["id"]]
    return start + (end - start) * t


def solve(cfg, geo):
    """Dò hpMultiplier cho TỪNG wave + máu boss để headroom rơi đúng đường cong.

    Giải được vì headroom = sát_thương / máu, mà máu ∝ hpMultiplier tuyến tính:
    lặp vài vòng là hội tụ (build phụ thuộc tiền, tiền phụ thuộc thưởng — không
    phụ thuộc máu, nên vòng lặp không có phản hồi ngược).
    """
    waves_cfg, enemies, towers, econ = cfg
    emap = {e["id"]: e for e in enemies["enemies"]}
    mults, boss_hp = {}, {}
    for w in range(1, 21):
        r = analyse(w, cfg, geo)
        spec = next(x for x in waves_cfg["waves"] if x["wave"] == w)
        base = sum(emap[eid]["baseHp"] * n for eid, n in spawn_counts(spec).items() if n)
        tgt = target_headroom(w, waves_cfg["acts"])
        mults[w] = round(r["dmg"] / tgt / base, 2)
        if r["boss"]:
            # boss: hoả lực tập trung, mục tiêu 1.30 — chặt hơn wave thường vì
            # thua boss là thua nguyên trận (leakDamage 5)
            b = r["boss"]
            boss_hp[w] = int(round(b["dmg"] / 1.30 / b["count"] / 100) * 100)
    return mults, boss_hp


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--map", default=None, help="id map trong config/maps/ (mặc định m00)")
    ap.add_argument("--no-arbitro", action="store_true")
    ap.add_argument("--solve", action="store_true",
                    help="dò hpMultiplier từng wave + máu boss, in ra (không ghi file)")
    args = ap.parse_args()

    if args.solve:
        cfg = (load("waves"), load("enemies"), load("towers"), load("economy"))
        geo = Geometry(load_map(args.map))
        mults, boss_hp = solve(cfg, geo)
        print("=" * 70)
        print("DÒ hpMultiplier THEO TỪNG WAVE (thay hệ số bậc thang theo act)")
        print("=" * 70)
        # `a['hpMultiplier']` KHÔNG CÒN TỒN TẠI: hệ số bậc thang theo act đã bị thay
        # bằng công thức trơn `hpScaling` (waves.json → _whyNotPerAct). Cột "hiện tại"
        # phải đọc từ chính công thức đó, nếu không `--solve` ném KeyError — và nó ĐÃ
        # ném, im lặng suốt nhiều vòng, trong khi cả `waves.json → hpScaling._derivedBy`
        # lẫn `enemies.json → _hpDerivedBy` vẫn trỏ người đọc tới lệnh này.
        print(f"{'W':>3}{'act':>5}{'hpMult hiện tại':>17}{'hpMult MỚI':>13}{'head đích':>11}")
        for w in range(1, 21):
            a = act_of(w, cfg[0]["acts"])
            print(f"{w:>3}{a['id']:>5}{hp_mult(w, cfg[0]):>17.3f}{mults[w]:>13}"
                  f"{target_headroom(w, cfg[0]['acts']):>11.2f}")
        print()
        for w, hp in boss_hp.items():
            old = next(ap_["hp"] for b in cfg[1]["bosses"]
                       for ap_ in b["appearances"] if ap_["wave"] == w)
            count = len(boss_ids(next(x for x in cfg[0]["waves"] if x["wave"] == w)))
            print(f"  Boss W{w}: {count} con · máu mỗi con {old} → {hp}")
        return 0

    cfg = (load("waves"), load("enemies"), load("towers"), load("economy"))
    geo = Geometry(load_map(args.map))
    econ = cfg[3]
    bm = econ["balanceModel"]
    lo, hi = bm["headroomTargetMin"], bm["headroomTargetMax"]
    hard_lo, hard_hi = bm["headroomMin"], bm["headroomMax"]

    print("=" * 100)
    print("BẢNG TÍNH CÂN BẰNG M0 — chord ĐO THẬT từ path.json, không phải 2R")
    print("=" * 100)
    print(f"  Đường {geo.length:.2f} units · {len(geo.slots)} ô sân · "
          f"η={bm['etaWasteFactor']} σ={bm['sigmaByTowerLevel']} "
          f"target headroom {lo}–{hi}")
    print()
    print(f"{'W':>3}{'act':>4}{'n':>4}{'tiền':>7}{'dư':>6}{'máu':>8}{'τ':>6}"
          f"{'sát thương':>12}{'head':>7}  {'verdict':<9}build")
    print("-" * 100)

    rows, fails = [], []
    for w in range(1, 21):
        r = analyse(w, cfg, geo, use_arbitro=not args.no_arbitro)
        rows.append(r)
        ok = hard_lo <= r["head"] <= hard_hi
        band = lo <= r["head"] <= hi
        v = "✅" if band else ("⚠️ ngoài" if ok else "❌ VỠ")
        if not ok:
            fails.append(r)
        print(f"{r['wave']:>3}{r['act']:>4}{r['n']:>4}{r['cash']:>7.0f}"
              f"{r['left']:>6.0f}{r['hp']:>8}{r['tau']:>6.2f}{r['dmg']:>12.0f}"
              f"{r['head']:>7.2f}  {v:<9}{fmt_build(r['placed'])}")
        if r["boss"]:
            b = r["boss"]
            bv = "✅" if hard_lo <= b["head"] <= hard_hi else "❌ VỠ"
            print(f"{'':>3}{'':>4}{('B×' + str(b['count'])):>4}{'':>7}{'':>6}{b['hp']:>8}"
                  f"{'':>6}{b['dmg']:>12.0f}{b['head']:>7.2f}  {bv:<9}"
                  f"build có-tính-boss · build đám-đông chỉ {b['crowd_head']:.2f}"
                  f"{' → ÉP đổi đội hình ✅' if b['crowd_head'] < 1.0 else ' → boss KHÔNG ép gì'}")
            if not (hard_lo <= b["head"] <= hard_hi):
                fails.append({"wave": w, "boss": True, "head": b["head"]})

    print("-" * 100)
    heads = [r["head"] for r in rows]
    print(f"  headroom: min {min(heads):.2f} · tb {sum(heads)/len(heads):.2f} · max {max(heads):.2f}")
    print(f"  wave ngoài dải cứng [{hard_lo}, {hard_hi}]: {len(fails)}")
    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
