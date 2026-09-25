#!/usr/bin/env python3
"""Sinh bảng markdown trong docs/ từ config/*.json.

Lý do tồn tại: chỉ số tướng từng sống ở 3 chỗ (docs/02, docs/04, docs/05) và
cùng một lớp bug trùng lặp đã xảy ra 3/3 vòng review — headroom 1.06 vs 1.08,
máu boss 7000 vs 5500, hpMultiplier 3.2 vs 3.8. Script này biến lớp bug đó
thành không thể xảy ra: JSON là nguồn duy nhất, bảng trong docs được sinh ra.

    python3 tools/gen_docs.py           # ghi lại docs
    python3 tools/gen_docs.py --check   # không ghi; exit 1 nếu docs lệch JSON

Marker trong docs:
    <!-- GEN:tên_khối -->
    ...nội dung được sinh, đừng sửa tay...
    <!-- /GEN:tên_khối -->
"""

import argparse
import json
import re
import sys
from decimal import Decimal, ROUND_HALF_UP
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CONFIG = ROOT / "config"
DOCS = ROOT / "docs"


def load(name):
    with open(CONFIG / f"{name}.json", encoding="utf-8") as f:
        return json.load(f)


def hp_mult(wave, h=None):
    """hpMultiplier(wave) — công thức trơn ở waves.json → hpScaling.
    Thay 3 hệ số bậc thang theo act (chúng tạo vách đứng ở W8/W15).

    `h` = hpScaling riêng của một map; bỏ trống thì lấy của config gốc (m00).
    Mỗi map có growth riêng vì hình học khác nhau làm tướng mạnh/yếu khác nhau —
    xem docs/09 §"độ khó ngược"."""
    shared = load("waves")["hpScaling"]
    h = h or shared
    milestone = 1.0
    # Map chỉ override base/growth vẫn phải thừa kế milestone toàn game, giống
    # ConfigMapper. Nếu không, docs map và engine nói về hai độ khó khác nhau.
    for item in h.get("milestones", shared.get("milestones", [])):
        if wave >= item["wave"]:
            milestone = item["multiplier"]
        else:
            break
    return h["base"] * h["growthPerWave"] ** (wave - 1) * milestone


def rnd(x):
    """Làm tròn half-up — 62.5 → 63.

    KHÔNG dùng round() có sẵn: Python (và C# Math.Round) mặc định dùng
    banker's rounding → round(62.5) = 62. Nếu để vậy, script này và Unity
    ra hai kết quả khác nhau trên cùng một config. Đã thực xảy ra một lần:
    Tambor thưởng 25 × 2.5 = 62.5, lệch 28 Peso trên cả Act 3.

    C# tương ứng: Math.Round(x, MidpointRounding.AwayFromZero)
    Chốt ở config/economy.json → roundingMode: "half_up".
    """
    return int(Decimal(str(x)).quantize(Decimal("1"), rounding=ROUND_HALF_UP))


def fmt(n):
    """1234567 -> '1 234 567'. Giữ float như cũ."""
    if isinstance(n, float):
        return f"{n:g}"
    return f"{n:,}".replace(",", " ")


# ---------- các khối sinh ----------

def gen_tower_overview(cfg):
    rows = ["| Đơn vị | Tier | Vai trò | Giá | Sát thương | Tầm | Tốc đánh | DPS |",
            "|--------|------|---------|-----|-----------|-----|----------|-----|"]
    roles = {
        "single_target_dps": "Sát thương đơn mục tiêu cao nhất, tầm xa",
        "aoe_control": "Sát thương lan (AoE), khống chế đám đông",
        "long_range_fast": "Tầm xa nhất, đánh nhanh, dame lẻ thấp",
        "burst": "Burst một phát nặng, rất chậm, tầm ngắn",
        "goalkeeper": "Thủ môn — chặn lọt lưới. Chỉ đặt ở ô thủ môn",
        "control_multiplier": "**Trọng tài** — chậm vĩnh viễn, khắc chế tank",
    }
    for t in cfg["towers"]:
        lv1 = t["levels"][0]
        dmg, rate = lv1["damage"], lv1["attackRate"]
        if dmg == 0:
            dmg_s, rate_s, dps_s = "—", "—", "**0**" if t["id"] == "el_arbitro" else "—"
        else:
            splash = lv1.get("splashRadius")
            dmg_s = f"{dmg} (lan r{splash})" if splash else str(dmg)
            rate_s = f"{rate}/s"
            dps = rnd(dmg * rate)
            dps_s = f"**{dps}**/mục tiêu" if splash else f"**{dps}**"
        tier = t["tier"] or "—"
        rows.append(f"| **{t['displayName']}** | {tier} | {roles[t['role']]} | "
                    f"{lv1['cost']} | {dmg_s} | {lv1['range']} | {rate_s} | {dps_s} |")
    return "\n".join(rows)


def gen_tower_stats(cfg):
    rows = ["| Tướng | Cấp | Giá | Sát thương | Tốc đánh | DPS | Tầm | Chord (2R) | Kỹ năng mở | Đầu tư cộng dồn |",
            "|-------|-----|-----|-----------|----------|-----|-----|-----------|-----------|-----------------|"]
    for t in cfg["towers"]:
        cum = 0
        for lv in t["levels"]:
            cum += lv["cost"]
            name = f"**{t['displayName']}**" if lv["level"] == 1 else ""
            dmg, rate = lv["damage"], lv["attackRate"]
            if dmg == 0:
                dmg_s, rate_s, dps_s = "—", "—", "**0**"
            else:
                dmg_s = f"{dmg} lan" if lv.get("splashRadius") else str(dmg)
                rate_s = str(rate)
                dps = rnd(dmg * rate)
                dps_s = f"{dps} /mt" if lv.get("splashRadius") else str(dps)
                if lv["level"] == 3:
                    dps_s = f"**{dps}**" + (" /mt" if lv.get("splashRadius") else "")
            abil = next(a["id"] for a in t["abilities"] if a["unlockLevel"] == lv["level"])
            cum_s = f"**{fmt(cum)}**" if lv["level"] == 3 else fmt(cum)
            rows.append(f"| {name} | {lv['level']} | {lv['cost']} | {dmg_s} | {rate_s} | "
                        f"{dps_s} | {lv['range']} | {lv['range'] * 2:g} | `{abil}` | {cum_s} |")
    return "\n".join(rows)


def gen_enemy_table(cfg):
    rows = ["| Loại | Máu gốc | Tốc độ | Thưởng gốc | Trừ máu khi lọt | Vai trò |",
            "|------|---------|--------|-----------|-----------------|---------|"]
    for e in cfg["enemies"]:
        rows.append(f"| **{e['displayNameLong']}** | {e['baseHp']} | {e['speed']} | "
                    f"{e['baseBounty']} | {e['leakDamage']} | {e['role']} |")
    return "\n".join(rows)


def gen_scaled_stats(enemies, waves):
    acts = waves["acts"]
    hdr = "| Loại | " + " | ".join(f"Act {a['id']} máu" for a in acts) + " | " \
          + " | ".join(f"Act {a['id']} thưởng" for a in acts) + " |"
    sep = "|------|" + "|".join(["-----------"] * (len(acts) * 2)) + "|"
    rows = [hdr, sep]
    for e in enemies["enemies"]:
        hps = [fmt(rnd(e["baseHp"] * hp_mult(a["waves"][0]))) for a in acts]
        bts = [fmt(rnd(e["baseBounty"] * a["bountyMultiplier"])) for a in acts]
        rows.append(f"| {e['displayName']} | " + " | ".join(hps) + " | " + " | ".join(bts) + " |")
    for b in enemies["bosses"]:
        hp_by_act, bt_by_act = {}, {}
        for ap in b["appearances"]:
            for a in acts:
                if a["waves"][0] <= ap["wave"] <= a["waves"][1]:
                    hp_by_act.setdefault(a["id"], []).append(f"W{ap['wave']} **{fmt(ap['hp'])}**")
                    bt_by_act.setdefault(a["id"], []).append(f"W{ap['wave']} **{fmt(ap['bounty'])}**")
        hps = [" / ".join(hp_by_act.get(a["id"], [])) or "—" for a in acts]
        bts = [" / ".join(bt_by_act.get(a["id"], [])) or "—" for a in acts]
        rows.append(f"| `{b['displayName']}` | " + " | ".join(hps) + " | " + " | ".join(bts) + " |")
    return "\n".join(rows)


def counts_of(w, order):
    """Số con mỗi loại trong wave, cộng qua MỌI tuyến.

    `spawns` là MẢNG nhóm `{enemy,count,lane}` chứ không phải object `{adepto: 8}`
    — object không nhét được `lane` và không diễn tả nổi cùng một loại ra ở hai
    tuyến (khoá trùng). Xem docs/08 §2.2(b).
    """
    n = {k: 0 for k in order}
    for g in w["spawns"]:
        n[g["enemy"]] = n.get(g["enemy"], 0) + g["count"]
    return [n[k] for k in order]


def act_of(wave, acts):
    for a in acts:
        if a["waves"][0] <= wave <= a["waves"][1]:
            return a
    raise ValueError(f"wave {wave} không thuộc act nào")


def gen_wave_tables(enemies, waves, economy):
    emap = {e["id"]: e for e in enemies["enemies"]}
    bmap = {b["id"]: b for b in enemies["bosses"]}
    order = ["adepto", "tifoso", "tambor"]
    out, wallet = [], economy["startingCash"]

    for act in waves["acts"]:
        out.append(f"### Act {act['id']} — W{act['waves'][0]}–W{act['waves'][1]} "
                   f"(máu ×{hp_mult(act['waves'][0]):.2f}→×{hp_mult(act['waves'][1]):.2f} · thưởng ×{act['bountyMultiplier']:g})\n")
        out.append("| W | Adepto | Tifoso | Tambor | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví sau wave |")
        out.append("|---|--------|--------|--------|------|------|----------|-----------|-------|-----------|-------------|")

        for w in waves["waves"]:
            n = w["wave"]
            if not (act["waves"][0] <= n <= act["waves"][1]):
                continue
            counts = counts_of(w, order)
            hp = sum(c * rnd(emap[k]["baseHp"] * hp_mult(w["wave"]))
                     for k, c in zip(order, counts))
            bounty = sum(c * rnd(emap[k]["baseBounty"] * act["bountyMultiplier"])
                         for k, c in zip(order, counts))
            qty = sum(counts)

            bosses = w.get("bosses") or []
            boss_hp = boss_bounty = 0
            for spawn in bosses:
                ap = next(a for a in bmap[spawn["id"]]["appearances"] if a["wave"] == n)
                boss_hp += ap["hp"]
                boss_bounty += ap["bounty"]
            hp += boss_hp
            bounty += boss_bounty
            qty += len(bosses)
            boss_s = f"**{len(bosses)}**" if bosses else "—"
            bounty_s = (f"{fmt(bounty - boss_bounty)} + **{fmt(boss_bounty)}**"
                        if bosses else fmt(bounty))

            ov = waves["waveClearBonus"]["overrides"].get(str(n))
            clear = ov if ov is not None else 20 + 5 * (n - 1)
            total = bounty + clear
            wallet += total

            cells = [f"**{n}**" if bosses else str(n)]
            cells += [str(c) if c else "—" for c in counts]
            cells += [boss_s, str(qty),
                      f"**{fmt(hp)}**" if bosses else fmt(hp),
                      bounty_s,
                      f"**{clear}**" if ov is not None else str(clear),
                      fmt(total),
                      f"**{fmt(wallet)}**" if n == 20 else fmt(wallet)]
            out.append("| " + " | ".join(cells) + " |")
        out.append("")
    return "\n".join(out).rstrip()


def gen_wave_summary(enemies, waves, economy):
    emap = {e["id"]: e for e in enemies["enemies"]}
    bmap = {b["id"]: b for b in enemies["bosses"]}
    order = ["adepto", "tifoso", "tambor"]
    per = {}
    for act in waves["acts"]:
        qty = hp = inc = 0
        for w in waves["waves"]:
            n = w["wave"]
            if not (act["waves"][0] <= n <= act["waves"][1]):
                continue
            counts = counts_of(w, order)
            qty += sum(counts)
            hp += sum(c * rnd(emap[k]["baseHp"] * hp_mult(w["wave"]))
                      for k, c in zip(order, counts))
            b_ = sum(c * rnd(emap[k]["baseBounty"] * act["bountyMultiplier"])
                     for k, c in zip(order, counts))
            for spawn in w.get("bosses") or []:
                ap = next(a for a in bmap[spawn["id"]]["appearances"] if a["wave"] == n)
                hp += ap["hp"]
                b_ += ap["bounty"]
                qty += 1
            ov = waves["waveClearBonus"]["overrides"].get(str(n))
            inc += b_ + (ov if ov is not None else 20 + 5 * (n - 1))
        per[act["id"]] = (qty, hp, inc)

    ids = sorted(per)
    tot_q = sum(per[i][0] for i in ids)
    tot_h = sum(per[i][1] for i in ids)
    tot_i = sum(per[i][2] for i in ids)

    rows = [
        "| | " + " | ".join(f"Act {i}" for i in ids) + " | **Cả trận** |",
        "|---|" + "|".join(["-------"] * len(ids)) + "|-------------|",
        "| Số quân | " + " | ".join(fmt(per[i][0]) for i in ids) + f" | **{fmt(tot_q)}** |",
        "| Tổng máu | " + " | ".join(f"{fmt(per[i][1])}" for i in ids) + f" | **{fmt(tot_h)}** |",
        "| Tiền sinh ra | " + " | ".join(fmt(per[i][2]) for i in ids) + f" | **{fmt(tot_i)}** |",
        "",
        f"Cộng {fmt(economy['startingCash'])} khởi đầu → **tổng tiền cả đời một trận = "
        f"{fmt(economy['startingCash'] + tot_i)} Peso**.",
    ]
    return "\n".join(rows)


def gen_abilities(cfg):
    rows = ["| Đơn vị | Lv1 | Lv2 | Lv3 |", "|--------|-----|-----|-----|"]
    for t in cfg["towers"]:
        cells = []
        for lv in (1, 2, 3):
            a = next(x for x in t["abilities"] if x["unlockLevel"] == lv)
            cells.append(f"`{a['id']}`<br>{a['_summary']}")
        rows.append(f"| **{t['displayName']}** | " + " | ".join(cells) + " |")
    return "\n".join(rows)


def gen_projectiles(cfg):
    rows = ["| Loại đạn | Tốc độ (đơn vị/giây) | Dùng bởi | Ghi chú |",
            "|----------|---------------------|----------|---------|"]
    users = {}
    for t in cfg["towers"]:
        if t.get("projectile"):
            users.setdefault(t["projectile"], []).append(t["displayName"])
    for p in cfg["projectiles"]:
        spd = "**hitscan**" if p["speedUnitsPerSec"] < 0 else f"{p['speedUnitsPerSec']:g}"
        rows.append(f"| `{p['id']}` | {spd} | {', '.join(users.get(p['id'], ['—']))} | "
                    f"{p['_note']} |")
    return "\n".join(rows)


# ---------- máy chèn marker ----------


# ---------- khối riêng từng map (docs/maps/*.md) ----------

def _map_files():
    return sorted((CONFIG / "maps").glob("*.json"))


def gen_map_block(mp, enemies, waves, economy):
    """Bảng SỐ CHỐT của một map — sinh thẳng từ `config/maps/<id>.json`.

    Vì sao phải sinh: bảng wave trong `docs/maps/*.md` là bản THIẾT KẾ do agent
    viết trước khi có số đo engine. Sau vòng cân bằng, số trong config đã khác
    bản thiết kế ở gần như mọi map (tiền khởi đầu, growth, số quân). Hai bảng
    cùng nói về một thứ mà không có gì ràng chúng lại = đúng lớp bug mà file
    này sinh ra để diệt. Bảng thiết kế giữ nguyên làm lịch sử; bảng này là số thật.
    """
    import path_check as pc

    emap = {e["id"]: e for e in enemies["enemies"]}
    bmap = {b["id"]: b for b in enemies["bosses"]}
    order = ["adepto", "tifoso", "tambor"]

    hs = mp.get("hpScaling") or waves["hpScaling"]
    wv = mp.get("waves") or waves["waves"]
    cash = (mp.get("economy") or {}).get("startingCash", economy["startingCash"])

    lanes = []
    for ln in mp["lanes"]:
        pts = [(w["x"], w["y"]) for w in ln["waypoints"]]
        total, _ = pc.arc_lengths(pc.catmull_rom(pts))
        lanes.append((ln["id"], total))

    fields = [x for x in mp["slots"] if x["type"] == "field"]
    tot_q = sum(g["count"] for w in wv for g in w["spawns"])

    out = [
        f"**Nguồn: `config/maps/{mp['id']}-*.json` — sinh bằng `tools/gen_docs.py`. "
        f"Đừng sửa tay; sửa JSON rồi chạy lại.**",
        "",
        "| | |",
        "|---|---|",
        f"| Tuyến | {len(lanes)} — " + " · ".join(f"`{i}` {l:.1f}u" for i, l in lanes) + " |",
        f"| Ô sân / thủ môn | {len(fields)} / {len(mp['slots']) - len(fields)} |",
        f"| Σchord@1.4 mỗi ô | {mp['coveragePerSlot']:.2f} |",
        f"| Tiền khởi đầu | {fmt(cash)} |",
        f"| Máu quái | ×{hs['base']:g} × {hs['growthPerWave']:g}^(W−1) × milestone → W20 ×{hp_mult(20, hs):.2f} |",
        f"| Tổng quân 20 wave | {fmt(tot_q)} |",
        "",
        "| W | ×máu | " + " | ".join(f"`{i}`" for i, _ in lanes) + " | boss | quân | tổng máu |",
        "|---|------|" + "|".join(["------"] * len(lanes)) + "|------|------|----------|",
    ]

    for w in wv:
        n = w["wave"]
        m = hp_mult(n, hs)
        cells = []
        for lid, _ in lanes:
            per = {k: 0 for k in order}
            for g in w["spawns"]:
                if g.get("lane", "L1") == lid:
                    per[g["enemy"]] = per.get(g["enemy"], 0) + g["count"]
            cells.append("/".join(str(per[k]) for k in order) if sum(per.values()) else "—")

        hp = sum(g["count"] * rnd(emap[g["enemy"]]["baseHp"] * m) for g in w["spawns"])
        qty = sum(g["count"] for g in w["spawns"])
        bs = w.get("bosses") or []
        for b in bs:
            ap = next(a for a in bmap[b["id"]]["appearances"] if a["wave"] == n)
            hp += ap["hp"]
            qty += 1
        boss_s = " + ".join(f"`{b['id']}`@`{b['lane']}`" for b in bs) if bs else "—"

        out.append(f"| {'**' + str(n) + '**' if bs else n} | ×{m:.2f} | "
                   + " | ".join(cells) + f" | {boss_s} | {qty} | {fmt(hp)} |")

    out.append("")
    out.append("Ba số trong ô tuyến là `adepto/tifoso/tambor`.")
    return "\n".join(out)


def gen_map_ladder(waves, economy):
    """Thang độ khó: một dòng mỗi map, theo `order`. Đây là bảng DUY NHẤT nói
    thứ tự chơi và độ khó — `MapPlayableTests` giữ đúng thang này bằng test."""
    rows = [
        "| # | Map | Tuyến | Ô | Σchord/ô | Tiền đầu | growth | Quân | Sao |",
        "|---|-----|-------|---|----------|----------|--------|------|-----|",
    ]
    maps = []
    for f in _map_files():
        with open(f, encoding="utf-8") as fh:
            maps.append(json.load(fh))
    for mp in sorted(maps, key=lambda x: x["order"]):
        hs = mp.get("hpScaling") or waves["hpScaling"]
        wv = mp.get("waves") or waves["waves"]
        cash = (mp.get("economy") or {}).get("startingCash", economy["startingCash"])
        nf = sum(1 for x in mp["slots"] if x["type"] == "field")
        tot = sum(g["count"] for w in wv for g in w["spawns"])
        rows.append(f"| {mp['order']} | **{mp['displayName']}** `{mp['id']}` | {len(mp['lanes'])} "
                    f"| {nf} | {mp['coveragePerSlot']:.2f} | {fmt(cash)} | {hs['growthPerWave']:g} "
                    f"| {fmt(tot)} | {'★' * mp['difficulty']} |")
    return "\n".join(rows)


def build_blocks():
    towers, enemies = load("towers"), load("enemies")
    waves, economy = load("waves"), load("economy")
    blocks = {
        "tower_overview": gen_tower_overview(towers),
        "tower_stats": gen_tower_stats(towers),
        "abilities": gen_abilities(towers),
        "projectiles": gen_projectiles(towers),
        "enemy_table": gen_enemy_table(enemies),
        "scaled_stats": gen_scaled_stats(enemies, waves),
        "wave_tables": gen_wave_tables(enemies, waves, economy),
        "wave_summary": gen_wave_summary(enemies, waves, economy),
        "map_ladder": gen_map_ladder(waves, economy),
    }
    for f in _map_files():
        with open(f, encoding="utf-8") as fh:
            mp = json.load(fh)
        # m00 KHÔNG có khối riêng: nó dùng thẳng `config/waves.json` và đã có
        # bảng đầy đủ ở docs/03. Sinh thêm một bảng nữa là tạo lại đúng lớp
        # trùng lặp mà file này sinh ra để diệt.
        if mp["id"] == "m00":
            continue
        blocks[f"map_{mp['id']}"] = gen_map_block(mp, enemies, waves, economy)
    return blocks


def apply(blocks, check_only):
    drift, seen = [], set()
    for md in sorted(DOCS.rglob("*.md")):
        text = original = md.read_text(encoding="utf-8")
        for name, body in blocks.items():
            pat = re.compile(
                rf"(<!-- GEN:{re.escape(name)} -->)(.*?)(<!-- /GEN:{re.escape(name)} -->)",
                re.DOTALL,
            )
            if not pat.search(text):
                continue
            seen.add(name)
            # \\g<1> tránh việc body chứa \1..\9 bị hiểu là backreference
            text = pat.sub(lambda m, b=body: m.group(1) + "\n" + b + "\n" + m.group(3), text)
        if text != original:
            if check_only:
                drift.append(md.relative_to(ROOT))
            else:
                md.write_text(text, encoding="utf-8")
                print(f"  cập nhật {md.relative_to(ROOT)}")

    unused = set(blocks) - seen
    if unused:
        print(f"  ⚠️  khối chưa có marker trong doc nào: {', '.join(sorted(unused))}")
    return drift


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--check", action="store_true",
                    help="Không ghi; exit 1 nếu docs lệch JSON (dùng cho CI)")
    args = ap.parse_args()

    blocks = build_blocks()
    drift = apply(blocks, args.check)

    if args.check:
        if drift:
            print("❌ Docs LỆCH khỏi config/*.json:")
            for d in drift:
                print(f"   - {d}")
            print("\n   Chạy: python3 tools/gen_docs.py")
            return 1
        print("✅ Docs khớp config/*.json")
    else:
        print("✅ Sinh xong.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
