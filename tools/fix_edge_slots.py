#!/usr/bin/env python3
"""
Đẩy ô có VÙNG CHẠM tràn mép màn hình vào trong, mà KHÔNG phá hình học cân bằng.

Vì sao cần: máy thật (iPhone 12, 1170×2532) cho thấy f02/f05 ở |x|=4.9 có vùng
chạm 48pt tràn mép 0.16 unit. Ngón tay không bấm hết được → ô hỏng.

Vì sao không đơn giản là `x = ±4.74`: `d` (khoảng cách tới đường) quyết định
tướng nào dùng được ô nào — đó là toàn bộ thang tầm ở path.json §_slotDesign.
Kéo x vào gần đường thì d tụt, và ô đổi tầng. Nên phải bù bằng y.

Cách làm: cố định x ở mép cho phép, quét y để giữ NGUYÊN cả hai bất biến:
  · d  (khoảng cách tới đường)   — giữ tầng thang tầm
  · f  (vị trí theo quãng đường) — giữ phân bố ô dọc đường, không tạo lỗ hổng

Chỉ IN ra đề xuất. Không ghi đè path.json — người đọc số rồi tự quyết.
"""
import json
import math
import pathlib
import sys

sys.path.insert(0, str(pathlib.Path(__file__).parent))
from path_check import arc_lengths, catmull_rom, load  # noqa: E402


def nearest(curve, cum, p):
    """(d, f) — khoảng cách tới đường, và vị trí theo quãng đường [0,1]."""
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
    ppu_pt = cfg["ui"]["designWidthPt"] / cfg["units"]["viewportUnits"]["width"]
    touch_r = cfg["ui"]["minTouchTargetPt"] / ppu_pt / 2
    x_limit = half_w - touch_r

    print(f"nửa bề ngang {half_w:.2f} · bán kính chạm {touch_r:.2f} → |x| tối đa {x_limit:.2f}\n")

    for slot in cfg["slots"]:
        x, y = slot["x"], slot["y"]
        overflow = abs(x) + touch_r - half_w
        if overflow <= 0:
            continue

        d0, f0 = nearest(curve, cum, (x, y))
        # Lùi thêm 0.01 khỏi mép: đúng bằng mép là knife-edge, một lần làm tròn
        # là lại tràn.
        x_new = math.copysign(x_limit - 0.01, x)

        best = None
        steps = 4000
        for k in range(steps + 1):
            y_try = y - 2.0 + 4.0 * k / steps
            d, f = nearest(curve, cum, (x_new, y_try))
            # Ưu tiên giữ d (thang tầm), rồi tới f (phân bố dọc đường).
            cost = abs(d - d0) * 10 + abs(f - f0) * 3
            if best is None or cost < best[0]:
                best = (cost, y_try, d, f)

        _, y_new, d_new, f_new = best
        print(f"{slot['id']}: tràn {overflow:.2f} unit")
        print(f"  cũ   ({x:+.2f}, {y:+.2f})  d={d0:.2f}  f={f0:.3f}")
        print(f"  mới  ({x_new:+.2f}, {y_new:+.2f})  d={d_new:.2f}  f={f_new:.3f}")
        print(f"  đổi  d {d_new - d0:+.3f} · f {f_new - f0:+.3f}\n")


if __name__ == "__main__":
    main()
