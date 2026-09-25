#!/usr/bin/env python3
"""Kiểm tra hình học đường chạy + ô đặt trong config/path.json.

Lý do tồn tại: docs/04 §3.2 giả định `chord = 2R` — đoạn đường một tướng phủ
được bằng đúng đường kính tầm của nó. Trên đường CONG, giả định đó sai: vòng
tròn tầm cắt nhiều đoạn đường cùng lúc. Script này tính chord THẬT bằng cách
lấy mẫu spline, thay vì đoán.

    python3 tools/path_check.py            # báo cáo
    python3 tools/path_check.py --check    # exit 1 nếu vi phạm ràng buộc
"""

import argparse
import itertools
import json
import math
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SAMPLES = 4000  # số mẫu dọc spline


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



def catmull_rom(pts, samples=SAMPLES):
    """Spline Catmull-Rom qua pts. Nhân đôi điểm đầu/cuối để spline chạm chúng."""
    p = [pts[0]] + list(pts) + [pts[-1]]
    out = []
    segs = len(p) - 3
    per = max(2, samples // segs)
    for i in range(segs):
        p0, p1, p2, p3 = p[i], p[i + 1], p[i + 2], p[i + 3]
        for j in range(per):
            t = j / per
            t2, t3 = t * t, t * t * t
            out.append((
                0.5 * ((2 * p1[0]) + (-p0[0] + p2[0]) * t
                       + (2 * p0[0] - 5 * p1[0] + 4 * p2[0] - p3[0]) * t2
                       + (-p0[0] + 3 * p1[0] - 3 * p2[0] + p3[0]) * t3),
                0.5 * ((2 * p1[1]) + (-p0[1] + p2[1]) * t
                       + (2 * p0[1] - 5 * p1[1] + 4 * p2[1] - p3[1]) * t2
                       + (-p0[1] + 3 * p1[1] - 3 * p2[1] + p3[1]) * t3),
            ))
    out.append(tuple(pts[-1]))
    return out


def arc_lengths(curve):
    """Trả (tổng_độ_dài, [độ_dài_tích_luỹ tại mỗi mẫu])."""
    cum, total = [0.0], 0.0
    for a, b in zip(curve, curve[1:]):
        total += math.dist(a, b)
        cum.append(total)
    return total, cum


def chord_for(curve, cum, centre, radius):
    """Độ dài đường NẰM TRONG vòng tròn tầm + số đoạn rời rạc bị cắt."""
    inside = [math.dist(pt, centre) <= radius for pt in curve]
    covered, runs, in_run = 0.0, 0, False
    for i in range(len(curve) - 1):
        if inside[i] and inside[i + 1]:
            covered += cum[i + 1] - cum[i]
        if inside[i] and not in_run:
            runs, in_run = runs + 1, True
        elif not inside[i]:
            in_run = False
    return covered, runs


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--map", default=None, help="id map trong config/maps/ (mặc định m00)")
    ap.add_argument("--check", action="store_true")
    args = ap.parse_args()

    path_cfg, towers = load_map(args.map), load("towers")

    # ── ĐA TUYẾN ────────────────────────────────────────────────────────────
    # Mọi phép đo về Ô phải gộp MỌI tuyến, không chỉ tuyến đầu:
    #   • `d` = khoảng cách tới tuyến GẦN NHẤT — ô phục vụ tuyến B không phải ô chết
    #     chỉ vì nó xa tuyến A. Bản cũ chỉ nhìn tuyến 0 nên báo "ô chết" GIẢ.
    #   • `chord` = TỔNG qua các tuyến — ô canh được hai tuyến thì mạnh gấp đôi, và
    #     đó chính là thứ làm map đa tuyến dễ hơn thiết kế tưởng.
    lanes = []
    for ln in path_cfg["lanes"]:
        c = catmull_rom([(w["x"], w["y"]) for w in ln["waypoints"]])
        L, cm = arc_lengths(c)
        lanes.append({"id": ln.get("id", "L?"), "curve": c, "cum": cm,
                      "length": L, "declared": ln["lengthUnits"]})

    def d_all(pt):
        """Khoảng cách tới tuyến GẦN NHẤT."""
        return min(min(math.dist(q, pt) for q in ln["curve"]) for ln in lanes)

    def chord_all(pt, R):
        """(tổng chord qua mọi tuyến, số đoạn rời nhau nhiều nhất trên một tuyến)."""
        tot, runs = 0.0, 0
        for ln in lanes:
            ch, rn = chord_for(ln["curve"], ln["cum"], pt, R)
            tot += ch
            runs = max(runs, rn)
        return tot, runs

    # Giữ tên cũ cho phần code còn nhìn một tuyến (hộp bao chung, thang vật lý).
    curve = [q for ln in lanes for q in ln["curve"]]
    total_length = sum(ln["length"] for ln in lanes)

    errs = []
    print("=" * 78)
    print(f"ĐƯỜNG CHẠY — {len(lanes)} tuyến")
    print("=" * 78)
    for ln in lanes:
        drift = abs(ln["length"] - ln["declared"]) / ln["declared"] * 100
        print(f"  {ln['id']:<4} thật {ln['length']:>7.2f} · khai {ln['declared']:>7.2f} · "
              f"lệch {drift:>4.1f}%  {'✅' if drift <= 2 else '❌ > 2%'}")
        if drift > 2:
            errs.append(f"tuyến {ln['id']}: độ dài spline lệch {drift:.1f}% so với lengthUnits")

    vp = path_cfg["units"]["viewportUnits"]
    xs, ys = [p[0] for p in curve], [p[1] for p in curve]
    print(f"  Hộp bao               : x [{min(xs):.1f}, {max(xs):.1f}]  "
          f"y [{min(ys):.1f}, {max(ys):.1f}]")
    print(f"  Viewport              : x [{-vp['width']/2:.1f}, {vp['width']/2:.1f}]  "
          f"y [{-vp['height']/2:.1f}, {vp['height']/2:.1f}]")
    if max(abs(min(xs)), abs(max(xs))) > vp["width"] / 2:
        errs.append("đường chạy tràn ra ngoài viewport theo trục X")

    # Tầm dùng cho bảng ô SÂN phải lấy từ tướng SÂN. Dibu tầm 2.0 nhưng chỉ
    # đứng ô thủ môn — trộn nó vào đây làm bảng chord vô nghĩa.
    fld_towers = [t for t in towers["towers"] if t["slotType"] == "field"]
    fld_ranges = [lv["range"] for t in fld_towers for lv in t["levels"]]
    r_min, r_max = min(fld_ranges), max(fld_ranges)
    # Ai đang giữ tầm lớn nhất — ĐỌC TỪ CONFIG, không viết cứng. Trước đây nhãn
    # ghi thẳng "El Fideo Lv3"; tướng bị gỡ ngày 2026-08-20 và nhãn nói dối.
    r_max_owner = next(f"{t['displayName']} Lv{i + 1}"
                       for t in fld_towers
                       for i, lv in enumerate(t["levels"]) if lv["range"] == r_max)
    # Cột giữa phải là tầm CÓ THẬT của một tướng, không phải hằng số gõ tay —
    # gõ tay thì thu tầm xong bảng vẫn in một tầm không ai có.
    r_mid = sorted(fld_ranges)[len(fld_ranges) // 2]
    lv1_max = max(t["levels"][0]["range"] for t in fld_towers)

    field = [s for s in path_cfg["slots"] if s["type"] == "field"]
    gk = [s for s in path_cfg["slots"] if s["type"] == "goalkeeper"]

    print()
    print("=" * 78)
    print(f"CHORD THẬT vs GIẢ ĐỊNH 2R  —  {len(field)} ô sân")
    print("=" * 78)
    print(f"{'ô':<6}{'d(đường)':>10}{'R=' + str(r_min):>10}{'':>6}"
          f"{'R=' + str(r_mid):>10}{'':>6}{'R=' + str(r_max):>10}{'':>7}")
    print(f"{'':<6}{'':>10}{'chord':>10}{'/2R':>6}{'chord':>10}{'/2R':>6}"
          f"{'chord':>10}{'/2R':>7}{'đoạn':>6}")
    print("-" * 78)

    ratios = {r_min: [], r_mid: [], r_max: []}
    max_runs = 0
    for s in sorted(field, key=lambda s: s["id"]):
        c = (s["x"], s["y"])
        d = d_all(c)
        row = f"{s['id']:<6}{d:>10.2f}"
        runs_here = 0
        for R in (r_min, r_mid, r_max):
            ch, runs = chord_all(c, R)
            ratio = ch / (2 * R)
            ratios[R].append(ratio)
            runs_here = max(runs_here, runs)
            row += f"{ch:>10.2f}{ratio:>6.2f}"
        max_runs = max(max_runs, runs_here)
        row += f"{runs_here:>6}"
        print(row)
        # Ô xa đường KHÔNG phải lỗi — đó là yếu tố thiết kế (chỉ tướng tầm xa
        # dùng được). Lỗi thật là ô mà KHÔNG tướng Lv1 nào với tới → ô chết.
        # 🔵 Ô ngoài tầm Lv1 KHÔNG tự động là lỗi — có map lấy đó làm cơ chế
        # ("ô tầm xa", ép nâng cấp trước khi trải rộng). Nhưng nó phải được KHAI,
        # không được lặng lẽ tồn tại: khai thì đó là thiết kế, không khai thì đó là
        # 240 Peso người chơi ném đi mà không hiểu vì sao tướng đứng im.
        #
        # Ô ngoài tầm LỚN NHẤT của game thì vẫn chết, khai cũng không cứu được.
        far_ok = s["id"] in path_cfg.get("farSlots", [])
        if d > r_max:
            errs.append(f"ô {s['id']} cách đường {d:.2f} > tầm LỚN NHẤT trong game "
                        f"{r_max} → Ô CHẾT THẬT, không tướng nào ở cấp nào với tới")
        elif d > lv1_max and not far_ok:
            errs.append(f"ô {s['id']} cách đường {d:.2f} > tầm Lv1 lớn nhất {lv1_max} "
                        f"→ không tướng Lv1 nào với tới. Nếu là CỐ Ý thì khai vào "
                        f"`farSlots` của map; nếu không thì dời ô lại gần.")

    print("-" * 78)
    for R in (r_min, r_mid, r_max):
        v = ratios[R]
        print(f"  R={R:<4} chord/2R:  min {min(v):.2f}  tb {sum(v)/len(v):.2f}  max {max(v):.2f}")

    print()
    print("=" * 78)
    print("KẾT LUẬN VỀ GIẢ ĐỊNH `chord = 2R` (docs/04 §8 mục 5)")
    print("=" * 78)
    avg_max = sum(ratios[r_max]) / len(ratios[r_max])
    print(f"  Tầm lớn nhất trong game (R={r_max}, {r_max_owner}):")
    print(f"    chord/2R trung bình = {avg_max:.2f}")
    print(f"    Số đoạn đường bị cắt tối đa bởi 1 vòng tròn tầm = {max_runs}")
    if max_runs >= 2:
        print(f"    → XÁC NHẬN: vòng tròn tầm cắt NHIỀU đoạn đường.")
    if avg_max > 1.0:
        print(f"    → Mô hình ĐÁNH GIÁ THẤP phe phòng ngự ~{avg_max:.0%} với tướng tầm xa.")
    elif avg_max < 0.9:
        print(f"    → Mô hình ĐÁNH GIÁ CAO phe phòng ngự — chord thật chỉ {avg_max:.0%} của 2R.")
    else:
        print(f"    → Giả định 2R xấp xỉ đúng ({avg_max:.2f}).")

    # Ô nào dùng được tướng nào (ở Lv1) — thông tin thiết kế, không phải lỗi
    print()
    print("=" * 78)
    print("ĐỘ ĐA DẠNG Ô — tướng nào với tới đường từ ô nào (tầm Lv1)")
    print("=" * 78)
    fld = [t for t in towers["towers"] if t["slotType"] == "field"]
    gkt = [t for t in towers["towers"] if t["slotType"] == "goalkeeper"]
    fld.sort(key=lambda t: t["levels"][0]["range"])
    print(f"{'ô':<6}{'d':>6}  " + "".join(f"{t['displayName'][:9]:<11}" for t in fld))
    print("-" * 78)
    tally = {t["displayName"]: 0 for t in fld}
    for s in sorted(field, key=lambda s: s["id"]):
        d = d_all((s["x"], s["y"]))
        marks = ""
        for t in fld:
            ok = d <= t["levels"][0]["range"]
            tally[t["displayName"]] += ok
            marks += f"{'  ✅' if ok else '  ❌':<11}"
        print(f"{s['id']:<6}{d:>6.2f}  {marks}")
    print("-" * 78)
    print("  Số ô dùng được: " + " · ".join(
        f"{k} {v}/{len(field)}" for k, v in tally.items()))

    # Ranh giới dao cạo: ô cách đường ~ĐÚNG BẰNG tầm một tướng. Lệch 0.01 units
    # (= 0.4pt trên máy) lật ✅ thành ❌ — người chơi thấy tướng không bắn và
    # tưởng game lỗi, chứ không đọc ra "ngoài tầm 1cm". Đã tự vấp 2 lần (f04, f01).
    EDGE = 0.05
    # Ô THỦ MÔN cũng phải bị quét. Luật này từng chỉ duyệt `field`, và hậu quả:
    # Dibu Lv1 tầm 0.7 với gk01 cách đường 0.73 → thủ môn KHÔNG VỚI TỚI ĐƯỜNG, chưa
    # từng cản được gì, tướng 140 Peso vô dụng — và không luật nào thấy. Chỉ có test
    # xác suất bắt ra (40 ván cứu đúng 0 con, kỳ vọng ~48).
    all_slots = field + gk
    for s in sorted(all_slots, key=lambda s: s["id"]):
        d = min(math.dist(pt, (s["x"], s["y"])) for pt in curve)
        # Tướng SÂN so với ô SÂN; thủ môn so với ô thủ môn. Trộn vào nhau thì
        # bảng vô nghĩa — đã vấp một lần ở vòng 5.
        cands = gkt if s in gk else fld
        for t in cands:
            r = t["levels"][0]["range"]
            if abs(d - r) < EDGE:
                errs.append(
                    f"ô {s['id']} cách đường {d:.2f} ≈ tầm Lv1 của "
                    f"{t['displayName']} ({r}) — lệch {abs(d-r):.2f} < {EDGE} → "
                    f"RANH GIỚI DAO CẠO, dời ô ra xa ranh giới")

    # Nếu tướng ngắn tầm nhất với tới MỌI ô thì tầm không còn là đánh đổi —
    # thang tầm 3.0→6.0 chỉ là số trang trí, mọi ô tương đương nhau.
    shortest = fld[0]
    if tally[shortest["displayName"]] == len(field):
        errs.append(
            f"mọi ô đều trong tầm Lv1 của {shortest['displayName']} "
            f"({shortest['levels'][0]['range']}) → tầm không phải đánh đổi, "
            f"chọn ô vô nghĩa")

    # Ô phải rải đều theo QUÃNG ĐƯỜNG, không theo toạ độ. Ô nằm gần nhau trong
    # không gian vẫn có thể cùng canh một đoạn, để hở đoạn khác — quân đi tự do
    # qua đó. Đã suýt lọt 17% đầu đường vì ràng buộc sai (ép vị trí, không ép đoạn).
    # 🔵 2026-08-21 — đọc từ file map, không còn là hằng số cứng.
    # 0.15 viết cho map MỘT tuyến 11 ô. Map đa tuyến chia cùng ngân sách ô cho
    # nhiều tuyến nên sàn của đoạn hở cao hơn hẳn (~số_tuyến × 1.2 / số_ô) và luật
    # 0.15 sẽ báo đỏ cho MỌI map đa tuyến khả thi. Xem docs/08 §3C.
    # Nới thì phải nới TRONG FILE MAP, có tên và có lý do — không sửa lén ở đây.
    # Trần khe hở. Mặc định KHÔNG phải hằng số 0.15 — nó là SÀN TOÁN HỌC của bố cục:
    # mỗi ô "giữ" được khoảng một tầm đường, nên với `n` tuyến chia `k` ô thì khe hở
    # nhỏ nhất có thể là ~`n × 1.2 / k`. Với map 1 tuyến 11 ô con số đó là 0.11 nên
    # 0.15 vẫn là ràng buộc thật; với map 2 tuyến 12 ô nó là 0.20 và luật 0.15 sẽ báo
    # đỏ cho MỌI bố cục khả thi — tức luật vô nghĩa, và cách "chữa" duy nhất là nhồi
    # thêm ô, phá luôn ý đồ khan hiếm ô. Xem docs/08 §3C.
    #
    # Map vẫn được KHAI `maxGapFraction` để siết chặt hơn (hoặc nới, có lý do ghi rõ) —
    # khai thì khai thắng, không khai thì lấy sàn.
    _floor = max(0.15, len(lanes) * 1.2 / max(len(field), 1))
    MAX_GAP = path_cfg.get("maxGapFraction") or _floor   # `null` trong JSON = không khai
    print()
    # 🔵 Kiểm TỪNG TUYẾN. Gộp mọi tuyến vào một trục `f` là vô nghĩa — hai tuyến
    # song song đều được canh kín vẫn cho ra "khe hở" giả ở chỗ trục nối chúng.
    for ln in lanes:
        cvl, cml, Ll = ln["curve"], ln["cum"], ln["length"]
        # Chỉ tính ô THỰC SỰ canh tuyến này. Ngưỡng là tầm LỚN NHẤT trong game, không
        # phải tầm Lv1: luật này hỏi "có ai canh nổi đoạn này không", mà ô tầm xa ở Lv3
        # thì canh được — chỉ là muộn hơn. Lấy tầm Lv1 sẽ báo đỏ oan cho mọi map dùng
        # cơ chế "ô tầm xa". Vấn đề ĐẦU TRẬN (chưa nâng cấp) do bài kiểm 20 wave bắt,
        # không phải luật hình học.
        near = [s for s in field
                if min(math.dist(q, (s["x"], s["y"])) for q in cvl) <= r_max]
        if not near:
            errs.append(f"tuyến {ln['id']}: KHÔNG ô nào với tới — tuyến không có ai canh")
            continue
        fpos = sorted(cml[min(range(len(cvl)),
                              key=lambda k: math.dist(cvl[k], (s["x"], s["y"])))] / Ll
                      for s in near)
        gaps = ([(b - a, a, b) for a, b in zip(fpos, fpos[1:])]
                + [(fpos[0], 0.0, fpos[0]), (1 - fpos[-1], fpos[-1], 1.0)])
        g, ga, gb = max(gaps)
        print(f"  {ln['id']:<4} đoạn hở lớn nhất: f={ga:.2f}→{gb:.2f} = "
              f"{g:.0%} ({g*Ll:.1f} units, trần {MAX_GAP:.0%})  {'✅' if g <= MAX_GAP else '❌'}")
        if g > MAX_GAP:
            errs.append(f"tuyến {ln['id']}: đoạn f={ga:.2f}→{gb:.2f} ({g:.0%}, {g*Ll:.1f} units) "
                        f"không ô nào canh → quân đi tự do qua đó")

    # ── NGÂN SÁCH ĐỘ PHỦ ────────────────────────────────────────────────────
    #
    # 🔴 LUẬT NÀY SINH RA TỪ MỘT LỖI THẬT. Ba map đầu (m00/m01/m02/m06) được thiết
    # kế với hình học "thú vị" — zigzag, túi combo, xoáy ốc — và người chơi báo độ
    # khó GIẢM dần qua bốn map. Nguyên nhân đo được: hình học đó làm ĐƯỜNG TỰ ÁP
    # SÁT CHÍNH NÓ, nên một vòng tầm cắt nhiều khúc đường rời nhau và một tướng
    # đánh được nhiều lượt. Sức phòng ngự trên mỗi ô: m00 1.74 → m02 2.90 → m06 3.03,
    # tức map càng "thú vị" tướng càng mạnh và map càng DỄ.
    #
    # Con số này phải là RÀNG BUỘC KHAI BÁO, không phải thứ phát hiện sau khi chơi:
    # map khai `coveragePerSlot` mình nhắm tới, lệch quá 8% thì đỏ. Nhờ vậy sửa
    # waypoint mà vô tình làm map dễ đi thì biết NGAY, không đợi người chơi báo.
    cov_total = sum(chord_all((s["x"], s["y"]), r_mid)[0] for s in field)
    cov_per_slot = cov_total / len(field)
    print()
    print("=" * 78)
    print("NGÂN SÁCH ĐỘ PHỦ — sức phòng ngự trên mỗi ô")
    print("=" * 78)
    print(f"  Σchord@R={r_mid} = {cov_total:.1f} units trên {len(field)} ô sân")
    print(f"  → mỗi ô canh {cov_per_slot:.2f} units đường")
    declared_cov = path_cfg.get("coveragePerSlot")
    if declared_cov is None:
        print("  ⚠️ map CHƯA khai `coveragePerSlot` — thêm vào file map để khoá con số này")
        errs.append("map chưa khai `coveragePerSlot`; không có nó thì sửa waypoint làm "
                    "map dễ đi mà không gì báo")
    else:
        off = abs(cov_per_slot - declared_cov) / declared_cov * 100
        print(f"  Khai trong map        : {declared_cov:.2f}  → lệch {off:.1f}%  "
              f"{'✅' if off <= 8 else '❌ > 8%'}")
        if off > 8:
            errs.append(f"độ phủ mỗi ô {cov_per_slot:.2f} lệch {off:.1f}% so với khai "
                        f"{declared_cov:.2f} → độ khó đã trôi, chỉnh lại hình học "
                        f"hoặc cập nhật khai báo VÀ hpScaling")

    # ── ĐỘ PHỦ THEO TỪNG TUYẾN ──────────────────────────────────────────────
    #
    # 🔴 `Σchord` cộng qua các tuyến ĐẾM THỪA trên map đa tuyến. Một tướng bắn MỘT
    # mục tiêu một lúc: canh được hai tuyến làm nó BẬN hơn, không làm nó MẠNH hơn.
    # Đo vòng 27 trên engine thật: 4/4 map một tuyến thắng ở tiền thiết kế, 7/7 map
    # đa tuyến THUA trong act 1 — dù `coveragePerSlot` của chúng cao gấp đôi m00.
    # Xem docs/09 §8.
    #
    # Con số nói thật hơn là **đường canh được trên mỗi CON QUÁI của tuyến đó**. m00 —
    # map đã cân xong, dùng làm mốc — đạt 19.1 / 465 = 0.041.
    #
    # ⚠️ ĐÂY LÀ ĐIỀU KIỆN CẦN, KHÔNG PHẢI ĐIỀU KIỆN ĐỦ. Nó giả định người chơi đã
    # MUA HẾT ô; mà 7 map đa tuyến thua ở act 1, lúc mới có 2–3 khẩu. Đo lại sau khi
    # tôn trọng `farSlots` thì cả 7 map đều ✅ — tức chỉ số này KHÔNG dự báo được
    # những lần thua đó. Cổng thật là `MapPlayableTests` chạy 20 wave trên engine.
    # Giữ mục này vì nó bắt được một lớp lỗi KHÁC: một tuyến bị bỏ đói hoàn toàn.
    M00_CHORD_PER_ENEMY = 0.041

    waves_of_map = path_cfg.get("waves")
    if waves_of_map is None:
        waves_of_map = load("waves")["waves"]

    print()
    print("=" * 78)
    print("ĐỘ PHỦ THEO TỪNG TUYẾN — con số quyết định trên map đa tuyến")
    print("=" * 78)
    print(f"  {'tuyến':<10}{'Σchord':>9}{'quân cả trận':>15}{'chord/quân':>13}   so m00")
    lane_bad = []
    for lane in lanes:
        cum_l = arc_lengths(lane["curve"])[1]
        # Ô khai trong `farSlots` là ô CỐ Ý nằm ngoài tầm Lv1 — bản thiết kế bảo
        # người chơi phải nâng tầm mới dùng được. Đo chúng ở tầm Lv1 là đo sai ý đồ,
        # nên dùng tầm LỚN NHẤT cho đúng những ô đó và tầm giữa cho phần còn lại.
        far = set(path_cfg.get("farSlots") or [])
        cov_l = sum(chord_for(lane["curve"], cum_l, (s["x"], s["y"]),
                              r_max if s["id"] in far else r_mid)[0] for s in field)
        n_l = sum(g["count"] for w in waves_of_map for g in w["spawns"]
                  if g.get("lane", "L1") == lane["id"])
        per = cov_l / n_l if n_l else 0.0
        ratio = per / M00_CHORD_PER_ENEMY if M00_CHORD_PER_ENEMY else 0.0
        mark = "✅" if ratio >= 0.60 else "❌ < 60%"
        print(f"  {lane['id']:<10}{cov_l:>9.1f}{n_l:>15}{per:>13.3f}   ×{ratio:.2f} {mark}")
        if ratio < 0.60:
            lane_bad.append((lane["id"], per, ratio))

    if lane_bad:
        print()
        print("  ⚠️ Tuyến dưới 60% mốc m00 = hàng thủ trên tuyến đó ĐÓI. Ba cách chữa,")
        print("     theo thứ tự nên thử: (1) kéo dài tuyến để nó đi qua nhiều ô hơn,")
        print("     (2) bớt quân trên tuyến đó, (3) thêm ô canh tuyến đó.")
        print("     KHÔNG chữa bằng cách tăng `coveragePerSlot` — nó là số cộng thừa.")
        for lid, per, ratio in lane_bad:
            errs.append(f"tuyến `{lid}` chỉ canh {per:.3f} units đường trên mỗi con quái "
                        f"(×{ratio:.2f} mốc m00) → hàng thủ tuyến này đói, map sẽ không "
                        f"thắng nổi ở tiền khởi đầu bình thường")

    # ── Thang vật lý: unit → px → pt ────────────────────────────────────────
    # Hình học đúng trong "unit" vẫn có thể KHÔNG BẤM ĐƯỢC trên máy thật.
    # Thu tầm vòng 5 làm mọi thứ nhỏ lại, nên phải kiểm bằng pt, không phải unit.
    ui = path_cfg.get("ui", {})
    pt_w = ui.get("designWidthPt", 393)
    touch_pt = ui.get("minTouchTargetPt", 48)
    u2pt = ppu_to_pt = path_cfg["units"]["pixelsPerUnit"] / (vp["width"] * 100 / pt_w)
    touch_u = touch_pt / u2pt
    top = vp["height"] / 2

    print()
    print("=" * 78)
    print("THANG VẬT LÝ — hình học đúng vẫn có thể không bấm được")
    print("=" * 78)
    print(f"  1 unit = {path_cfg['units']['pixelsPerUnit']} px = {u2pt:.1f} pt "
          f"(thiết kế {vp['width']*100:.0f}px ≈ {pt_w}pt)")
    print(f"  Vùng chạm tối thiểu {touch_pt}pt = {touch_u:.2f} units")

    pairs = sorted(
        (math.dist((a["x"], a["y"]), (b["x"], b["y"])), a["id"], b["id"])
        for a, b in itertools.combinations(path_cfg["slots"], 2))
    d0, a0, b0 = pairs[0]
    print(f"  Hai ô gần nhau nhất: {a0}–{b0} = {d0:.2f} units = {d0*u2pt:.0f} pt "
          f"{'❌' if d0 < touch_u else '✅'}")
    if d0 < touch_u:
        errs.append(f"ô {a0} và {b0} cách {d0*u2pt:.0f}pt < vùng chạm {touch_pt}pt → chạm nhầm")

    # 🔵 VÒNG 21: luật RADIAL MENU đã GỠ. Hud.cs bỏ radial menu (nút xoè quanh ô
    # bị khuất/tràn mép trên máy thật) → thay bằng thanh bottom cố định ở đáy màn
    # (DrawTowerBar). Nút mua/nâng/bán KHÔNG còn phụ thuộc vị trí ô, nên ô sát mép
    # trên KHÔNG còn làm nút bị cắt. Ràng buộc còn lại là VÙNG CHẠM 48pt của chính
    # ô — kiểm ở khối "VÙNG CHẠM phải nằm trọn trong khung" ngay dưới.

    # VÙNG CHẠM phải nằm trọn trong khung, không chỉ TÂM ô.
    #
    # Luật này thiếu cho tới vòng 8, và cái giá là: công cụ in "✅ Mọi ràng buộc
    # hình học đều đạt" trong khi trên iPhone 12 thật, f02/f05 (|x|=4.9) có vùng
    # chạm tràn mép 0.16 unit và vòng tròn bị cắt mất một phần. Mắt người dùng
    # bắt được, còn 15 luật ở đây thì không — vì tất cả đều hỏi về TÂM ô.
    #
    # Ô có tâm trong màn nhưng vùng chạm tràn mép = ngón tay không bấm hết được.
    half_w, half_h = vp["width"] / 2, vp["height"] / 2
    x_limit = half_w - touch_u / 2
    print(f"  Vùng chạm nằm trọn trong khung → |x| tối đa {x_limit:.2f} units")
    for s in path_cfg["slots"]:
        ox = abs(s["x"]) + touch_u / 2 - half_w
        oy = abs(s["y"]) + touch_u / 2 - half_h
        if ox > 0:
            errs.append(f"ô {s['id']} (x={s['x']}) — vùng chạm {touch_pt}pt tràn mép "
                        f"ngang {ox:.2f} units → ngón tay không bấm hết được")
            print(f"  ❌ {s['id']}: vùng chạm tràn mép ngang {ox:.2f} units")
        if oy > 0:
            errs.append(f"ô {s['id']} (y={s['y']}) — vùng chạm {touch_pt}pt tràn mép "
                        f"dọc {oy:.2f} units")
            print(f"  ❌ {s['id']}: vùng chạm tràn mép dọc {oy:.2f} units")

    if gk:
        g = gk[0]
        d = min(math.dist(pt, (g["x"], g["y"])) for pt in curve)
        gp = path_cfg["lanes"][0]["goalPoint"]
        print()
        print(f"  Ô thủ môn {g['id']}: cách đường {d:.2f}, "
              f"cách cầu môn {math.dist((g['x'], g['y']), (gp['x'], gp['y'])):.2f}")

    print()
    if errs:
        print("❌ VI PHẠM:")
        for e in errs:
            print(f"   - {e}")
        return 1 if args.check else 0
    print("✅ Mọi ràng buộc hình học đều đạt.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
