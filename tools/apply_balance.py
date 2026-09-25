#!/usr/bin/env python3
"""Ghi bộ số cân bằng đã ĐO được vào file map.

    python3 tools/apply_balance.py m07 --cash 2100 --growth 1.03 --scale 0.7

Ba núm, đúng ba thứ có nghĩa vật lý khác nhau:

  --cash    tiền khởi đầu. Núm của GIAI ĐOẠN ĐẦU. Map nhiều tuyến cần nhiều hơn
            vì một khẩu chỉ bắn tuyến nó với tới — hai tuyến là hai hàng thủ phải
            nuôi cùng lúc trong khi thu nhập không đổi.
  --growth  hpScaling.growthPerWave. Núm của ĐỘ KHÓ CUỐI: máu quái lên, tiền
            thưởng đứng yên.
  --scale   nhân số con mỗi nhóm. Núm của TRẦN KINH TẾ: ít quân = ít tiền, nên nó
            KHÔNG phải núm độ khó (giảm quân từng làm map KHÓ hơn — xem docs/09).
            Dùng nó để kéo tổng tiền cả trận xuống dưới trần chi tiêu.

Làm tròn phải khớp C# `Math.Round(double)` (banker's rounding) vì bộ dò tham số
chạy bằng engine C#; lệch một con là lệch cả kết quả đo.
"""
import argparse
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
MAPS = ROOT / "config" / "maps"


def find(map_id):
    hits = sorted(MAPS.glob(f"{map_id}*.json"))
    if not hits:
        raise SystemExit(f"không có map `{map_id}` trong {MAPS}")
    return hits[0]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("map_id")
    ap.add_argument("--cash", type=int)
    ap.add_argument("--growth", type=float)
    ap.add_argument("--scale", type=float)
    ap.add_argument("--note", default="")
    a = ap.parse_args()

    f = find(a.map_id)
    cfg = json.loads(f.read_text(encoding="utf-8"))

    if a.cash is not None:
        old = cfg["economy"]["startingCash"]
        cfg["economy"]["startingCash"] = a.cash
        print(f"  tiền khởi đầu {old} → {a.cash}")

    if a.growth is not None:
        old = cfg["hpScaling"]["growthPerWave"]
        cfg["hpScaling"]["growthPerWave"] = a.growth
        print(f"  growthPerWave {old} → {a.growth}")

    if a.scale is not None and abs(a.scale - 1.0) > 1e-9:
        before = sum(g["count"] for w in cfg["waves"] for g in w["spawns"])
        for w in cfg["waves"]:
            for g in w["spawns"]:
                g["count"] = max(1, round(g["count"] * a.scale))
        after = sum(g["count"] for w in cfg["waves"] for g in w["spawns"])
        print(f"  số quân {before} → {after} (×{a.scale})")

    if a.note:
        cfg["_balanceNote"] = a.note

    f.write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"  ghi {f.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
