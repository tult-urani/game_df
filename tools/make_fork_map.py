#!/usr/bin/env python3
"""Dựng lại hình học một map theo kiểu MỘT THÂN CHUNG, HAI LỐI RA.

Vì sao kiểu này: đo vòng 27 cho thấy giá của map đa tuyến không nằm ở "nhiều
tuyến" mà ở "ô bị khoá cứng vào một tuyến". m05 có 2 tuyến mà vẫn chạy đúng
700 Peso vì bốn ô ở eo canh được CẢ HAI tuyến; m03/m04/m08 cũng 2 tuyến nhưng
ô tách hẳn nên phải 2 100–2 700.

Thân chung dài + chẻ đôi ở cuối cho cả hai thứ: mọi ô cạnh thân đều canh hai
tuyến (rẻ), còn đoạn chẻ vẫn bắt người chơi chia lực (bài học của map).

    python3 tools/make_fork_map.py m03 --preview
    python3 tools/make_fork_map.py m03 --write
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

# Mỗi map một dáng riêng. Khác nhau ở: biên độ zigzag, số nhịp, chỗ chẻ, độ phình
# của hai nhánh, và bên nào phình rộng hơn — đủ để nhìn ra là hai sân khác nhau.
# 🔴 NHỊP ĐƯỜNG LẤY THEO m00, KHÔNG TỰ BỊA. Bản đầu tôi dựng zigzag dày (bước dọc
# 1.5u) và đo ra MỘT ô canh 5.3u đường — gấp 3 lần mức 1.74u của m00, tức map dễ đi
# gấp ba mà không ai thấy. Đúng lỗi "hình học thú vị làm map dễ" của vòng 26.
#
# m00 đi hết bề ngang sân (|x| tới 4.3) với bước dọc ~2.3u, nên một vòng tầm 1.4 chỉ
# bắt được 1–2 nếp. Giữ nguyên nhịp đó; mỗi map chỉ đổi biên độ, chiều lật và chỗ chẻ.
THAN_MOC = [
    (0.00, 9.36), (-4.00, 8.42), (-4.30, 5.85), (-1.00, 4.68),
    (3.00, 3.98), (4.30, 1.87), (2.00, 0.23), (-2.50, -0.47),
]

DANG = {
    #       biên độ  lật  số điểm thân  phình  lệch
    "m03": (1.00,  +1,  7,  4.00,  0.00),
    "m04": (0.95,  -1,  8,  3.80,  0.15),
    "m07": (1.02,  +1,  8,  4.10, -0.12),
    "m08": (0.92,  -1,  7,  3.70,  0.18),
    "m10": (0.98,  +1,  8,  4.20, -0.15),
}

TOP, GOAL = 9.36, -9.13


def than_va_nhanh(bien_do, lat, n_than, phinh, lech, _unused=None):
    """Trả (thân chung, nhánh trái, nhánh phải)."""
    than = [(round(x * bien_do * lat, 2), y) for x, y in THAN_MOC[:n_than]]
    x_che, y_che = than[-1]

    def nhanh(dau):
        w = phinh * (1 + lech * dau)
        b = max(y_che - GOAL, 4.0)
        return [
            (round(dau * w * 0.55 + x_che * 0.35, 2), round(y_che - b * 0.22, 2)),
            (round(dau * w, 2),                       round(y_che - b * 0.50, 2)),
            (round(dau * w * 0.80, 2),                round(y_che - b * 0.74, 2)),
            (round(dau * w * 0.36, 2),                round(y_che - b * 0.90, 2)),
            (0.0, GOAL),
        ]

    return than, nhanh(-1), nhanh(+1)


def dai(pts):
    return pc.arc_lengths(pc.catmull_rom([tuple(p) for p in pts]))[0]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("map_id")
    ap.add_argument("--write", action="store_true")
    a = ap.parse_args()

    if a.map_id not in DANG:
        raise SystemExit(f"chưa khai dáng cho `{a.map_id}` — thêm vào DANG")
    than, trai, phai = than_va_nhanh(*DANG[a.map_id])

    f = Path(sorted(glob.glob(str(ROOT / "config" / "maps" / f"{a.map_id}*.json")))[0])
    cfg = json.loads(f.read_text(encoding="utf-8"))
    vp = cfg["units"]["viewportUnits"]
    hw, hh = vp["width"] / 2, vp["height"] / 2

    lanes = []
    for lid, nh in zip([l["id"] for l in cfg["lanes"][:2]] or ["L1", "L2"], [trai, phai]):
        pts = than + nh
        for x, y in pts:
            if abs(x) > hw or abs(y) > hh:
                raise SystemExit(f"điểm ({x},{y}) tràn viewport {hw}×{hh}")
        lanes.append((lid, pts, dai(pts)))

    print(f"{a.map_id}: thân chung {len(than)} điểm, chẻ tại y={than[-1][1]}")
    for lid, pts, L in lanes:
        print(f"  {lid:8} {len(pts):2} điểm, dài {L:5.2f}u")
    chung = dai(than)
    print(f"  đoạn chung dài {chung:5.2f}u = {chung / lanes[0][2] * 100:.0f}% một tuyến")

    if a.write:
        cfg["lanes"] = [
            {
                "id": lid,
                "interpolation": "catmull-rom",
                "lengthUnits": round(L, 2),
                "spawnPoint": {"x": pts[0][0], "y": pts[0][1]},
                "goalPoint": {"x": pts[-1][0], "y": pts[-1][1]},
                "waypoints": [{"x": x, "y": y} for x, y in pts],
            }
            for lid, pts, L in lanes
        ]
        cfg["_laneShapeNote"] = (
            f"Vòng 27: dựng lại theo kiểu MỘT THÂN CHUNG, HAI LỐI RA. Đoạn chung dài "
            f"{chung:.1f}u ({chung / lanes[0][2] * 100:.0f}% chiều dài một tuyến) — mọi ô cạnh thân "
            "canh được CẢ HAI tuyến, nên tiền khởi đầu về mức map một tuyến. Đoạn chẻ "
            "cuối vẫn bắt chia lực. Lý do: docs/09 §8 — giá của map đa tuyến nằm ở ô bị "
            "khoá cứng vào một tuyến, không nằm ở số tuyến.")
        f.write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        print(f"  ghi {f.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
