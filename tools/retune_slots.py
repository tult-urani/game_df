#!/usr/bin/env python3
"""
Kéo ô lại gần đường để NÂNG SÀN của thang tầm, mà không làm phẳng nó.

Vì sao: người chơi báo trên máy thật — "tầm 1 số tướng quá ngắn nên không thể gây
dame dù đặt cạnh đường". Đo được: Batigol (tầm 1.0) chỉ dùng được 5/11 ô = 45%.
Ba lần họ báo "tướng không gây dame", cả ba đều là ô nằm ngoài tầm.

Vì sao KÉO Ô chứ không TĂNG TẦM: vòng 5 đã thu tầm ×0.36 vì chord SIÊU TUYẾN TÍNH —
đo được tầm ×2.4 thì chord ×5.3, nên tướng tầm xa luôn thắng bất kể DPS. El Fideo
thống trị, La Pulga/Batigol thành nội dung chết. Ngưỡng nổ ở R≈2.0. Tăng tầm là đi
ngược lại bài học đắt nhất của dự án.

Ràng buộc phải giữ:
  · `path_check.py` luật "thang tầm suy biến": tướng ngắn tầm nhất KHÔNG được với
    tới mọi ô, nếu không thì chọn ô vô nghĩa và ta quay lại nội dung chết.
  · Vị trí theo QUÃNG ĐƯỜNG (f) phải giữ — nó quyết định phân bố ô dọc đường.
    Lệch f là tạo lỗ hổng không ô nào canh.
  · Vùng chạm 48pt phải nằm trong khung (|x| ≤ 4.74).
  · Ranh giới dao cạo: |d − tầm| ≥ 0.05.

Chỉ IN đề xuất. Không ghi đè path.json.
"""
import json
import math
import pathlib
import sys

sys.path.insert(0, str(pathlib.Path(__file__).parent))
from path_check import arc_lengths, catmull_rom, load  # noqa: E402

# d MỤC TIÊU cho từng ô. Thiết kế thang mới:
#   Batigol 1.0 → 7/11   (được 2 ô: f02, f08)
#   D10S/Árbitro 1.2 → 9/11
#   La Pulga 1.4 → 10/11
#   El Fideo 1.6 → 11/11
# Vẫn còn 4 tầng thật, và f03 vẫn là ô CHỈ El Fideo với tới.
TARGET = {
    "f01": 0.60,   # giữ
    "f02": 0.88,   # 1.31 → Batigol với tới (0.88 chứ không 0.95: cách tầm 1.0 đủ xa, tránh ranh giới dao cạo)
    "f03": 1.49,   # giữ — ô độc quyền El Fideo, giữ đỉnh thang
    "f04": 1.10,   # giữ — D10S ✅, Batigol ❌
    "f05": 0.61,   # giữ
    "f06": 1.12,   # 1.50 → D10S với tới (cách tầm 1.2 đủ xa)
    "f07": 0.79,   # giữ
    "f08": 0.88,   # 1.30 → Batigol với tới
    "f09": 1.32,   # 1.50 → La Pulga (1.4) với tới, D10S (1.2) không — cách cả hai mép ≥ 0.12
    "f10": 0.90,   # giữ
    "f11": 0.50,   # giữ
}


def nearest(curve, cum, p):
    best_d2, best_i = float("inf"), 0
    for i, q in enumerate(curve):
        d2 = (q[0] - p[0]) ** 2 + (q[1] - p[1]) ** 2
        if d2 < best_d2:
            best_d2, best_i = d2, i
    return math.sqrt(best_d2), cum[best_i] / cum[-1]


def main():
    cfg = load("path")
    pts = [(w["x"], w["y"]) for w in cfg["path"]["waypoints"]]
    curve = catmull_rom(pts)
    _, cum = arc_lengths(curve)

    half_w = cfg["units"]["viewportUnits"]["width"] / 2
    pt_per_u = cfg["ui"]["designWidthPt"] / cfg["units"]["viewportUnits"]["width"]
    touch_r = cfg["ui"]["minTouchTargetPt"] / pt_per_u / 2
    x_limit = half_w - touch_r

    print(f"|x| tối đa {x_limit:.2f} (vùng chạm 48pt trong khung)\n")
    print(f"{'ô':<5}{'d cũ':>7}{'d mới':>8}{'f cũ':>8}{'f mới':>8}   toạ độ mới")
    print("-" * 62)

    out = {}
    for slot in cfg["slots"]:
        sid = slot["id"]
        if sid not in TARGET:
            continue
        x0, y0 = slot["x"], slot["y"]
        d0, f0 = nearest(curve, cum, (x0, y0))
        want = TARGET[sid]

        if abs(d0 - want) < 0.01:
            print(f"{sid:<5}{d0:>7.2f}{'giữ':>8}{f0:>8.3f}{'—':>8}")
            continue

        # Quét quanh vị trí cũ: giữ f, ép d về mục tiêu, ép |x| trong mép.
        best = None
        for i in range(-90, 91):
            for j in range(-90, 91):
                x = x0 + i * 0.02
                y = y0 + j * 0.02
                if abs(x) + touch_r > half_w:
                    continue
                d, f = nearest(curve, cum, (x, y))
                # d là ràng buộc cứng; f là thứ phải giữ; xê dịch ít là ưu tiên cuối.
                cost = abs(d - want) * 30 + abs(f - f0) * 12 + math.dist((x, y), (x0, y0)) * 0.4
                if best is None or cost < best[0]:
                    best = (cost, x, y, d, f)

        _, x, y, d, f = best
        out[sid] = (round(x, 2), round(y, 2))
        print(f"{sid:<5}{d0:>7.2f}{d:>8.2f}{f0:>8.3f}{f:>8.3f}   ({x:+.2f}, {y:+.2f})")

    print("\n=== thang tầm dự kiến ===")
    towers = load("towers")
    fld = [t for t in towers["towers"] if t["slotType"] == "field"]
    tally = {t["displayName"]: 0 for t in fld}
    for slot in cfg["slots"]:
        sid = slot["id"]
        if sid not in TARGET:
            continue
        x, y = out.get(sid, (slot["x"], slot["y"]))
        d, _ = nearest(curve, cum, (x, y))
        for t in fld:
            if d <= t["levels"][0]["range"]:
                tally[t["displayName"]] += 1
    for k, v in tally.items():
        print(f"  {k:<12} {v}/11")


if __name__ == "__main__":
    main()
