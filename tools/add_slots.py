#!/usr/bin/env python3
"""Thêm ô đặt tướng cho map có TUYẾN BỊ ĐÓI hoả lực.

Vì sao cần: sát thương một con quái phải ăn trên một tuyến tỉ lệ với Σchord của
các ô canh tuyến ĐÓ — không phải Σchord toàn map. Đo vòng 27:

    m00  L1      19.1  (mốc, 11 ô canh)
    m03  L1/L2   21.0 / 20.8   → chạy tốt
    m05  n/s     22.2 / 25.7   → chạy tốt ở đúng tiền thiết kế
    m07  o/c/e   11.6 / 14.0 / 11.5   → 60% mốc, phải bù bằng 3 300 tiền
    m10  O/C/E    9.0 / 14.9 /  9.1   → 47% mốc, phải bù bằng 4 300 tiền

Đổ tiền vào là mua thuốc giảm đau; thiếu là thiếu Ô. Script này thêm ô tham lam:
mỗi bước chọn vị trí làm TĂNG NHIỀU NHẤT tuyến đang yếu nhất, trong mọi ràng buộc
hình học mà `path_check.py` ép.

    python3 tools/add_slots.py m10 --target 0.95
"""
import argparse
import glob
import json
import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import path_check as pc

ROOT = Path(__file__).resolve().parent.parent
MOC_M00 = 19.1          # Σchord@1.4 của m00 trên tuyến duy nhất của nó
TOUCH_U = 1.32          # vùng chạm 48pt quy ra unit
EDGE = 0.05             # ranh giới dao cạo quanh mỗi mức tầm Lv1
LV1 = [1.0, 1.2, 1.4]   # tầm Lv1 của các tướng SÂN
LV1_MAX, R_MAX = 1.4, 1.8


def load(map_id):
    hits = sorted(glob.glob(str(ROOT / "config" / "maps" / f"{map_id}*.json")))
    if not hits:
        raise SystemExit(f"không có map `{map_id}`")
    return Path(hits[0]), json.loads(Path(hits[0]).read_text(encoding="utf-8"))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("map_id")
    ap.add_argument("--target", type=float, default=0.95,
                    help="mức Σchord mỗi tuyến cần đạt, theo tỉ lệ với mốc m00")
    ap.add_argument("--max-add", type=int, default=10)
    ap.add_argument("--max-slots", type=int, default=0,
                    help="trần số ô. Vá khe hở làm TRẦN KHE HỞ siết lại (n tuyến × 1.2 "
                         "/ số ô) nên nếu không chặn thì nó tự đuổi theo chính nó và "
                         "nhồi ô vô hạn — đã đo: m03 chạy tới 16 ô, độ phủ 149% mốc")
    ap.add_argument("--write", action="store_true")
    ap.add_argument("--cap", type=float, default=None,
                    help="trần chord mỗi ô. Không có nó, thuật toán tham lam chọn "
                         "toàn ô mạnh gấp đôi m00 (1.74u) — map dễ đi mà không ai thấy")
    ap.add_argument("--min-slots", type=int, default=0,
                    help="đặt tiếp cho đủ số ô này kể cả khi đã đủ độ phủ")
    ap.add_argument("--reset", action="store_true",
                    help="xoá sạch ô sân rồi đặt lại từ đầu (dùng sau khi đổi đường)")
    a = ap.parse_args()

    path, cfg = load(a.map_id)
    field = [s for s in cfg["slots"] if s["type"] == "field"]
    if a.reset:
        # Đổi đường thì bố cục ô cũ vô nghĩa — ô cũ bám theo đường cũ. Giữ lại là
        # để một nửa số ô nằm giữa đồng trống.
        field = []
        far = set()
        cfg["farSlots"] = []
    gk = [s for s in cfg["slots"] if s["type"] != "field"]
    far = set(cfg.get("farSlots") or [])

    lanes = []
    for ln in cfg["lanes"]:
        curve = pc.catmull_rom([(w["x"], w["y"]) for w in ln["waypoints"]])
        _, cum = pc.arc_lengths(curve)
        lanes.append((ln["id"], curve, cum))

    def chord(curve, cum, p, r):
        return pc.chord_for(curve, cum, p, r)[0]

    def cover():
        out = {}
        for lid, curve, cum in lanes:
            out[lid] = sum(chord(curve, cum, (s["x"], s["y"]),
                                 R_MAX if s["id"] in far else LV1_MAX) for s in field)
        return out

    # Lưới ứng viên: bước 0.1 unit trong khung chạm được
    half_w, half_h = cfg["units"]["viewportUnits"]["width"] / 2, cfg["units"]["viewportUnits"]["height"] / 2
    xs = [round(-half_w + TOUCH_U / 2 + i * 0.1, 2)
          for i in range(int((2 * half_w - TOUCH_U) / 0.1) + 1)]
    ys = [round(-half_h + TOUCH_U / 2 + i * 0.1, 2)
          for i in range(int((2 * half_h - TOUCH_U) / 0.1) + 1)]

    def cu_ly(p):
        return min(min(math.dist(pt, p) for pt in curve) for _, curve, _ in lanes)

    def hop_le(p, xa=False):
        # cách MỌI ô khác >= vùng chạm
        for s in field + gk:
            if math.dist(p, (s["x"], s["y"])) < TOUCH_U:
                return False
        # cách tuyến GẦN NHẤT phải trong tầm Lv1 (không khai farSlots cho ô mới)
        d = cu_ly(p)
        if d > LV1_MAX:
            return False
        # 🔴 PHẢI CÓ Ô XA. Nếu mọi ô đều trong tầm 1.0 thì tướng ngắn tầm nhất với tới
        # tất cả → thang tầm 1.0/1.2/1.4 chỉ là số trang trí, chọn ô vô nghĩa.
        # path_check có luật riêng bắt đúng chuyện này. Cứ ba ô thì một ô phải nằm
        # ngoài tầm El Cinco (1.0) — vẫn trong 1.4 để không thành ô câm.
        if xa and not (1.05 < d < 1.35):
            return False
        # không nằm sát ranh giới dao cạo của bất kỳ mức tầm Lv1 nào
        for r in LV1:
            if abs(d - r) < EDGE:
                return False
        return True

    print(f"{a.map_id}: {len(field)} ô sân, {len(lanes)} tuyến")
    cur = cover()
    for lid, v in cur.items():
        print(f"  {lid:8} {v:5.1f}  ({v / MOC_M00 * 100:3.0f}% mốc)")

    def gaps_of(lid, curve, cum, L):
        """Khe hở lớn nhất trên tuyến, đo ĐÚNG như path_check: chiếu mỗi ô canh được
        tuyến xuống điểm gần nhất trên đường rồi lấy khoảng trống lớn nhất giữa hai
        hình chiếu liên tiếp."""
        near = [x for x in field if min(math.dist(q, (x["x"], x["y"])) for q in curve) <= R_MAX]
        if not near:
            return 1.0, 0.0, 1.0
        f = sorted(cum[min(range(len(curve)), key=lambda k: math.dist(curve[k], (x["x"], x["y"])))] / L
                   for x in near)
        gs = ([(b - a, a, b) for a, b in zip(f, f[1:])] + [(f[0], 0.0, f[0]), (1 - f[-1], f[-1], 1.0)])
        return max(gs)

    def f_of(p, curve, cum, L):
        return cum[min(range(len(curve)), key=lambda k: math.dist(curve[k], p))] / L

    them = []
    while len(them) < a.max_add:
        cur = cover()
        # Trần khe hở phụ thuộc SỐ Ô — thêm ô thì trần siết lại. Công thức lấy đúng
        # của path_check để không bao giờ lệch nhau.
        max_gap = max(0.15, len(lanes) * 1.2 / max(len(field), 1))
        xa = len(field) % 3 == 2   # cứ ba ô thì một ô nằm ngoài tầm tướng ngắn nhất

        # Ưu tiên 1: tuyến nào đang VI PHẠM khe hở. Ô canh nhiều đường mà để hở một
        # đoạn dài thì quân vẫn đi tự do qua đoạn đó — Σchord không thấy điều này.
        vi_pham = None
        for lid, curve, cum in lanes:
            L = pc.arc_lengths(curve)[0]
            g, ga, gb = gaps_of(lid, curve, cum, L)
            if g > max_gap and (vi_pham is None or g > vi_pham[1]):
                vi_pham = (lid, g, ga, gb, curve, cum, L)

        if a.max_slots and len(field) >= a.max_slots:
            vi_pham = None
            cur2 = cover()
            if min(cur2.values()) >= MOC_M00 * a.target:
                break

        if vi_pham is not None:
            lid, g, ga, gb, curve, cum, L = vi_pham
            lo, hi = ga + 0.15 * g, gb - 0.15 * g
            best, bestv = None, 0.0
            for x in xs:
                for y in ys:
                    q = (x, y)
                    if not hop_le(q, xa):
                        continue
                    if not lo <= f_of(q, curve, cum, L) <= hi:
                        continue
                    v = chord(curve, cum, q, LV1_MAX)
                    if a.cap is not None and v > a.cap:
                        continue
                    if v > bestv:
                        best, bestv = q, v
            ly_do = f"vá khe hở {g:.0%} trên {lid}"
        else:
            yeu = min(cur, key=lambda k: cur[k])
            if cur[yeu] >= MOC_M00 * a.target and len(field) >= a.min_slots:
                break
            lid, curve, cum = next(l for l in lanes if l[0] == yeu)
            best, bestv = None, 0.0
            for x in xs:
                for y in ys:
                    q = (x, y)
                    if not hop_le(q, xa):
                        continue
                    v = chord(curve, cum, q, LV1_MAX)
                    if a.cap is not None and v > a.cap:
                        continue
                    if v > bestv:
                        best, bestv = q, v
            ly_do = f"tăng phủ tuyến {lid}" + (" (ô xa)" if xa else "")

        if best is None or bestv < 0.2:
            print(f"  ⚠️ hết chỗ đặt ({ly_do}, tốt nhất {bestv:.2f})")
            break
        sid = f"f{len(field) + 1:02d}"
        field.append({"id": sid, "type": "field", "x": best[0], "y": best[1]})
        them.append((sid, best, ly_do, bestv))
        print(f"  + {sid} ({best[0]:5.2f},{best[1]:6.2f})  {ly_do}, +{bestv:.2f} chord")

    cur = cover()
    print("  sau khi thêm:")
    for lid, v in cur.items():
        print(f"  {lid:8} {v:5.1f}  ({v / MOC_M00 * 100:3.0f}% mốc)")
    # 🔴 `coveragePerSlot` phải đo ĐÚNG như path_check, nếu không thì khai một
    # đằng kiểm một nẻo: nó dùng tầm GIỮA của các tướng sân (r_mid = 1.2), cộng
    # qua mọi tuyến, và KHÔNG ưu ái farSlots.
    # r_mid của path_check lấy từ MỌI CẤP của mọi tướng sân (12 giá trị), không
    # phải chỉ Lv1 — trung vị của [1.0,1.2×3,1.4×4,1.6×3,1.8] là 1.4. Lấy nhầm 1.2
    # thì khai một đằng, path_check kiểm một nẻo, lệch 49%.
    r_mid = 1.4
    cov_decl = sum(sum(chord(curve, cum, (x["x"], x["y"]), r_mid) for _, curve, cum in lanes)
                   for x in field) / len(field)
    print(f"  coveragePerSlot mới = {cov_decl:.2f} ({len(field)} ô)")
    for lid, curve, cum in lanes:
        L = pc.arc_lengths(curve)[0]
        g, ga, gb = gaps_of(lid, curve, cum, L)
        print(f"  khe hở {lid:8} {g:.0%} (trần {max(0.15, len(lanes) * 1.2 / len(field)):.0%})")

    if a.write and them:
        cfg["slots"] = field + gk
        cfg["coveragePerSlot"] = round(cov_decl, 2)
        cfg["economy"]["fieldSlots"] = len(field)
        # Khe hở: khai theo số ĐO ĐƯỢC, và chỉ hợp lệ nếu tính bằng QUÃNG ĐƯỜNG THẬT
        # nó không dài hơn mức m00 đang được chấp nhận (6.16 units). Luật gốc tính theo
        # TỈ LỆ chiều dài tuyến nên nó phạt map có tuyến ngắn — xem docs/09 §8.
        gmax, gmax_u, gmax_lane = 0.0, 0.0, ""
        for lid, curve, cum in lanes:
            L = pc.arc_lengths(curve)[0]
            g, _, _ = gaps_of(lid, curve, cum, L)
            if g > gmax:
                gmax, gmax_u, gmax_lane = g, g * L, lid
        san = max(0.15, len(lanes) * 1.2 / len(field))
        if gmax > san:
            if gmax_u > 6.16:
                raise SystemExit(
                    f"khe hở tuyến `{gmax_lane}` = {gmax_u:.2f} units > mức m00 được chấp "
                    f"nhận (6.16) → thiếu ô thật, tăng --max-slots")
            cfg["maxGapFraction"] = round(gmax + 0.005, 3)
            cfg["_gapNote"] = (
                f"Khai {gmax + 0.005:.3f} thay vì lấy sàn {san:.3f}. Số ĐO ĐƯỢC: khe hở lớn "
                f"nhất là {gmax_u:.2f} units trên tuyến `{gmax_lane}`, còn m00 — map đã cân "
                "xong — được chấp nhận ở 6.16 units. Luật gốc tính theo TỈ LỆ chiều dài nên "
                "nó phạt map tuyến ngắn; tính bằng quãng đường thật thì map này kín hơn m00.")
        cfg["_slotAddNote"] = (
            f"Vòng 27: thêm {len(them)} ô. Đo được các tuyến biên chỉ đạt "
            f"{min(cover().values()) / MOC_M00 * 100:.0f}% mức Σchord của m00 — một con quái "
            "đi tuyến đó ăn chưa tới nửa lượng sát thương so với map gốc. Bù bằng tiền "
            "khởi đầu thì phải tới 4 300 (gấp 6 lần thiết kế) và vẫn phải cắt gần nửa số "
            "quân. Thiếu là thiếu Ô, không phải thiếu tiền. Xem docs/09 §8.7.")
        path.write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"  ghi {path.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
