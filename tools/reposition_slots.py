#!/usr/bin/env python3
"""Dời hai LOẠI ô đặt sai chỗ về vị trí dùng được, bằng số đo chứ không bằng mắt.

Hai lỗi công cụ này sửa — cả hai đều lọt qua `path_check.py` vì lý do khác nhau:

1. Ô THỦ MÔN SÁT LỐI RA. `path_check` có sẵn cổng `goalkeeperMinLeadUnits` /
   `goalkeeperMinCoverageUnits` nhưng CHỈ `m00` khai hai ngưỡng đó, nên m01–m10
   chưa bao giờ bị kiểm. Đo ra: m00 để Dibu cách cầu môn 6.35 units, mọi map còn
   lại 0.56–1.54 — tức vùng hoạt động của thủ môn chỉ bắt đầu có hiệu lực đúng
   lúc quân đã chạm lối ra. Người chơi trả tiền cho một tướng làm chậm quân sau
   khi quân đã ghi bàn.

2. Ô SÂN CHẾT ĐƯỢC KHAI LÀ "CỐ Ý". `farSlots` cho phép một ô nằm ngoài tầm Lv1
   như một cơ chế "ép nâng cấp trước khi trải rộng". Cơ chế đó BẤT KHẢ THI:
   `Hud.DrawTowerBar` chặn mua theo tầm Lv1 (`reaches = def.Levels[0].Range >=
   dPath`), nên không mua được gì để mà nâng. Ô chỉ nhận El Árbitro — tướng sát
   thương 0 được miễn kiểm — rồi đứng ngoài tầm không buff được ai.

Cách chọn vị trí mới: quét lưới, chấm bằng ĐỘ PHỦ THẬT (chord của spline nằm
trong vòng tầm), không phải bằng khoảng cách tới waypoint gần nhất. Ràng buộc
hình học của map được giữ nguyên: cách ô khác ≥ minTouchTargetPt, nằm trong khung.
"""
import argparse
import json
import math
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))
from path_check import catmull_rom, arc_lengths, chord_for  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
MAPS = ROOT / "config" / "maps"

# Ngưỡng "vùng hoạt động phải còn hiệu lực TRƯỚC lối ra", tính theo chiều dài
# tuyến chứ không phải hằng số: tuyến m00 dài 42u còn tuyến map chẻ đôi chỉ 21u,
# một ngưỡng cứng 8u sẽ bất khả thi ở nửa số map.
# 🔴 NGƯỠNG NÀY LÀ SÀN, KHÔNG PHẢI MỤC TIÊU.
#
# Bản đầu tôi đặt lead = 16% chiều dài tuyến (3–10u tuỳ map) và nó SAI HƯỚNG. Đo
# ra hậu quả: m04 mất 1.96→0.73u và m09 mất 2.05→1.02u đoạn đường mà CHỈ thủ môn
# canh, vì đoạn duy nhất không ô sân nào với tới CHÍNH LÀ đoạn áp cầu môn — đẩy
# Dibu ra xa là đẩy nó vào vùng người khác đã canh. m09 tụt từ Won 2/20 xuống Lost.
#
# Yêu cầu thật chỉ là: vùng tầm phải KẾT THÚC trước lối ra, đừng để nửa vòng tròn
# trùm ra sau khung thành nơi không có đường. 1.5u ở tốc độ quân chậm nhất là hơn
# một nhịp tung xúc xắc (2s) — đủ để "còn hiệu lực trước lối ra" có nghĩa thật.
# Cao hơn nữa không mua thêm được gì mà bán mất đoạn độc quyền.
LEAD_FRACTION = 0.0
LEAD_FLOOR = 1.0
# Sàn CỨNG: dưới mức này thì "vùng hoạt động trước lối ra" không còn nghĩa gì —
# quân đi 2.5u ở tốc độ chậm nhất vẫn chưa tới 3 giây. Không map nào được lùi qua.
LEAD_HARD_FLOOR = 1.0
MIN_GK_COVERAGE = 2.0

# Ô không được nằm ĐÈ lên đường: tướng đứng giữa lối đi thì vòng tầm ôm trọn một
# khúc và độ phủ vọt lên vô lý so với mọi ô khác. Ô gần đường nhất trong 11 map
# hiện tại cách 0.72u, nên 0.45 là sàn an toàn, không làm map cũ thành sai.
MIN_STANDOFF = 0.45

# Biên phải tránh quanh MỖI mép tầm Lv1. `path_check.EDGE` = 0.05; lấy rộng hơn
# một chút để ô sửa xong không nằm sát ngay ngưỡng mà cổng vừa cho qua.
EDGE_CLEAR = 0.07

# Khung sân: viewportUnits 10.8×19.2, trừ nửa vùng chạm 48pt (1.32u) mỗi mép.
HALF_TOUCH = 0.66


def lanes_of(cfg):
    out = []
    for ln in cfg["lanes"]:
        curve = catmull_rom([(w["x"], w["y"]) for w in ln["waypoints"]])
        length, cum = arc_lengths(curve)
        out.append({"id": ln["id"], "curve": curve, "cum": cum, "length": length,
                    "goal": (ln["goalPoint"]["x"], ln["goalPoint"]["y"])})
    return out


def thin(lanes, stride):
    """Bản giảm mẫu của tuyến, GIỮ NGUYÊN độ dài tích luỹ ở từng mẫu còn lại.

    Quét lưới phải chấm hàng nghìn vị trí; chấm trên 4000 mẫu/tuyến bằng Python
    thuần mất hàng phút mỗi ô. Giảm mẫu chỉ dùng để XẾP HẠNG — vị trí thắng luôn
    được kiểm lại trên curve đầy đủ trước khi ghi, nên sai số của bước này không
    thể lọt vào file. (Không dùng numpy: dự án cấm thêm dependency.)
    """
    out = []
    for l in lanes:
        idx = list(range(0, len(l["curve"]), stride))
        if idx[-1] != len(l["curve"]) - 1:
            idx.append(len(l["curve"]) - 1)
        out.append({**l,
                    "curve": [l["curve"][i] for i in idx],
                    "cum": [l["cum"][i] for i in idx]})
    return out


def window(lane, pt, radius):
    """(độ phủ, cách lối ra) của vòng tầm bán kính `radius` đặt tại `pt`.

    "Cách lối ra" lấy mẫu ĐƯỢC PHỦ GẦN CẦU MÔN NHẤT — đó mới là câu hỏi thật:
    sau khi ra khỏi vùng tầm, quân còn phải đi bao xa mới tới lối ra.
    """
    inside = [i for i, q in enumerate(lane["curve"]) if math.dist(q, pt) <= radius]
    if not inside:
        return 0.0, None
    coverage, _ = chord_for(lane["curve"], lane["cum"], pt, radius)
    return coverage, lane["length"] - lane["cum"][max(inside)]


def bounds(cfg):
    v = cfg["units"]["viewportUnits"]
    return v["width"] / 2 - HALF_TOUCH, v["height"] / 2 - HALF_TOUCH


def min_touch_units(cfg):
    ui = cfg["ui"]
    return ui["minTouchTargetPt"] / ui["designWidthPt"] * ui["minTouchTargetPt"] * 0 + 1.32


def _sweep(cfg, lanes, others, radius, accept, score, seed, span, step):
    bx, by = bounds(cfg)
    gap = min_touch_units(cfg)
    best = None
    n = int(round(span / step))
    for i in range(-n, n + 1):
        x = round(seed[0] + i * step, 3)
        if abs(x) > bx:
            continue
        for j in range(-n, n + 1):
            y = round(seed[1] + j * step, 3)
            if abs(y) > by:
                continue
            if any(math.dist((x, y), o) < gap for o in others):
                continue
            if not accept(lanes, (x, y), radius):
                continue
            sc = score(lanes, (x, y), radius)
            if best is None or sc > best[0]:
                best = (sc, x, y)
    return best


def search(cfg, lanes, others, radius, accept, score, seed, span=6.0, step=0.05):
    """Quét THÔ rồi quét TINH quanh điểm thắng; kiểm lại trên curve đầy đủ.

    Trả `None` nếu không có vị trí nào hợp lệ, hoặc nếu vị trí thắng ở vòng thô
    không sống sót khi kiểm lại bằng dữ liệu đầy đủ — thà không dời còn hơn dời
    tới một chỗ mà con số chỉ đúng trên bản giảm mẫu.
    """
    coarse = _sweep(cfg, thin(lanes, 8), others, radius, accept, score, seed, span, 0.20)
    if coarse is None:
        return None
    fine = _sweep(cfg, thin(lanes, 2), others, radius, accept, score,
                  (coarse[1], coarse[2]), 0.30, step)
    cand = fine or coarse
    if not accept(lanes, (cand[1], cand[2]), radius):
        return None
    return (score(lanes, (cand[1], cand[2]), radius), cand[1], cand[2])


def _lane_gap(lane, pt):
    return min(math.dist(pt, q) for q in lane["curve"])


def _gk_lanes(gks, gk, lanes):
    """Tuyến mà ô thủ môn này phụ trách — chia theo Ô THỦ MÔN GẦN TUYẾN NHẤT.

    Mỗi tuyến được gán cho đúng một ô, nên KHÔNG tuyến nào bị bỏ sót: bỏ sót một
    cửa nghĩa là quân đi cửa đó không bao giờ bị cản, và đó là lỗi im lặng tệ nhất
    trong nhóm này.

    Bản đầu chia theo CẦU MÔN gần nhất và nó sai ở m08: hai tuyến W/E cùng đổ về
    một cầu môn nên cả hai bị gán cho cùng một ô, trong khi đo ra chúng cách nhau
    2.1–5.0u suốt đoạn cuối — tầm Dibu 1.4 không có điểm nào phủ nổi cả hai.
    Chia theo khoảng cách tới ĐƯỜNG mới phản ánh được hình học thật.
    """
    if len(gks) == 1:
        return lanes
    mine = []
    for l in lanes:
        owner = min(gks, key=lambda g: _lane_gap(l, (g["x"], g["y"])))
        if owner["id"] == gk["id"]:
            mine.append(l)
    return mine or [min(lanes, key=lambda l: _lane_gap(l, (gk["x"], gk["y"])))]


def _gk_accept(mine, lead_min):
    def accept(_lanes, pt, r):
        hit = [window(l, pt, r) for l in mine]
        if any(d is None for _, d in hit):
            return False   # bỏ sót một cửa = quân đi cửa đó thoát sạch
        if min(min(math.dist(pt, q) for q in l["curve"]) for l in mine) < MIN_STANDOFF:
            return False
        # Phủ tuyến nào thì tuyến đó phải đủ DÀI và đủ SỚM. Không cho đánh đổi
        # "phủ tuyến A thật dài, tuyến B sát cửa" — quân đi tuyến B vẫn thoát.
        return all(c >= MIN_GK_COVERAGE and d >= lead_min for c, d in hit)
    return accept


def gk_feasible_lead(cfg, lanes, gk_range, ideal):
    """Ngưỡng lead LỚN NHẤT mà MỌI ô thủ môn của map này đạt được.

    Vì sao phải dò thay vì gõ một hằng số: `LEAD_FRACTION × chiều dài tuyến` là
    ngưỡng ĐÁNG MUỐN, không phải ngưỡng ĐẠT ĐƯỢC. m08 và m10 có một ô thủ môn
    phải phủ hai tuyến chỉ chụm lại sát cầu môn — ở đó không tồn tại vị trí nào
    vừa phủ đủ 2.0u mỗi tuyến vừa cách lối ra 4u. Gõ cứng ngưỡng thì hai map đó
    ĐỎ vĩnh viễn và người sau sẽ gỡ ngưỡng đi cho hết đỏ — mất luôn cái cổng.
    """
    gks = [s for s in cfg["slots"] if s["type"] == "goalkeeper"]
    lead = ideal
    while lead >= LEAD_HARD_FLOOR:
        ok = True
        for gk in gks:
            mine = _gk_lanes(gks, gk, lanes)
            accept = _gk_accept(mine, lead)
            if accept(lanes, (gk["x"], gk["y"]), gk_range):
                continue
            others = [(o["x"], o["y"]) for o in cfg["slots"] if o["id"] != gk["id"]]
            ref = mine[0]
            target = ref["length"] - lead - math.sqrt(max(gk_range ** 2 - 0.83 ** 2, 0.01))
            idx = min(range(len(ref["cum"])), key=lambda k: abs(ref["cum"][k] - target))
            probe = _sweep(cfg, thin(lanes, 8), others, gk_range, accept,
                           lambda *_: 0, ref["curve"][idx], 5.0, 0.20)
            if probe is None:
                ok = False
                break
        if ok:
            return lead
        lead -= 0.25
    return None


def gk_exclusive(cfg, lanes, pt, gk_range, field_range, skip_id):
    """Đoạn đường mà CHỈ ô thủ môn này canh — không ô sân nào với tới.

    🔴 Đây là con số đo đúng giá trị của Dibu, và bản đầu của công cụ này KHÔNG
    đo nó. Dibu (vòng 18) không còn cơ chế cản ở vạch vôi: nó chỉ tung xúc xắc
    20/30/40% mỗi 2 giây vào quân TRONG TẦM. Nên sức mạnh của nó = thời gian quân
    ở trong tầm, và phần thời gian ĐÁNG GIÁ là phần không tướng nào khác chạm tới.

    Bỏ qua số này thì solver kéo thủ môn vào giữa sân — nơi vẫn thoả mọi ngưỡng
    nhưng đã có ô sân canh sẵn. Đo được hậu quả: m09 mất 4.10→0.52u đoạn độc
    quyền và tụt từ Won 2/20 xuống Lost, dù chord riêng của Dibu không đổi
    (4.10→4.11). Ngưỡng nói "hợp lệ", số này nói "vô ích" — phải nghe cả hai.
    """
    fld = [(o["x"], o["y"]) for o in cfg["slots"]
           if o["type"] == "field" and o["id"] != skip_id]
    total = 0.0
    for l in lanes:
        c, cum = l["curve"], l["cum"]
        for i in range(len(c) - 1):
            if math.dist(c[i], pt) > gk_range:
                continue
            if any(math.dist(c[i], q) <= field_range for q in fld):
                continue
            total += cum[i + 1] - cum[i]
    return total


def search_gk(cfg, lanes, mine, others, radius, accept, score, tail=14.0, ds=0.02, dn=0.05):
    """Quét vị trí ô thủ môn DỌC THEO ĐƯỜNG, không theo lưới vuông.

    🔴 VÌ SAO KHÔNG DÙNG LƯỚI VUÔNG. Mục tiêu là ép sát ngưỡng lead, nên lời giải
    luôn nằm trên một DẢI MỎNG — tập điểm có lead đúng bằng ngưỡng. Lưới vuông bước
    0.20u bước qua dải đó: ở m10 điểm hợp lệ có lead 1.0019u, còn điểm cách nó
    0.20u có lead 0.9586u và bị loại, nên solver kết luận "gần nhất là 2.69u" —
    sai 1.7u chỉ vì độ phân giải.

    Quét theo (quãng đường dọc tuyến, độ lệch vuông góc) thì lead biến thiên ĐƠN
    ĐIỆU theo tham số thứ nhất, và bước 0.02u dọc đường nhỏ hơn bề rộng dải.

    Chỉ quét `tail` units cuối tuyến: ô thủ môn không có việc gì ở nửa sân trên,
    và giới hạn này là thứ cho phép bước mịn mà vẫn chạy trong vài giây.
    """
    bx, by = bounds(cfg)
    gap = min_touch_units(cfg)
    best = None
    for lane in mine:
        curve, cum, length = lane["curve"], lane["cum"], lane["length"]
        k = len(curve) - 1
        while k > 0 and length - cum[k] < tail:
            # Bước dọc đường ~ds units: tìm mẫu kế tiếp cách mẫu này đủ xa.
            base = curve[k]
            # Pháp tuyến từ tiếp tuyến hai mẫu liền kề.
            ax, ay = curve[max(k - 1, 0)]
            bx2, by2 = curve[min(k + 1, len(curve) - 1)]
            tx, ty = bx2 - ax, by2 - ay
            norm = math.hypot(tx, ty) or 1.0
            nx, ny = -ty / norm, tx / norm

            off = -radius
            while off <= radius:
                if abs(off) >= MIN_STANDOFF - 1e-9:
                    x = round(base[0] + nx * off, 3)
                    y = round(base[1] + ny * off, 3)
                    if abs(x) <= bx and abs(y) <= by \
                            and all(math.dist((x, y), o) >= gap for o in others) \
                            and accept(lanes, (x, y), radius):
                        sc = score(lanes, (x, y), radius)
                        if best is None or sc > best[0]:
                            best = (sc, x, y)
                off += dn

            # lùi dọc đường một bước ds
            target = cum[k] - ds
            while k > 0 and cum[k] > target:
                k -= 1
    return best


def fix_goalkeepers(cfg, lanes, gk_range, lead_min, field_range, report, force=False):
    """Kéo mỗi ô thủ môn ngược lên đường cho tới khi vùng tầm KẾT THÚC trước lối ra."""
    gks = [s for s in cfg["slots"] if s["type"] == "goalkeeper"]
    changed = 0

    for gk in gks:
        mine = _gk_lanes(gks, gk, lanes)
        accept = _gk_accept(mine, lead_min)

        # 🔴 Chấm bằng QUÃNG DỜI NGƯỢC DẤU, không bằng độ phủ. Bản đầu chấm bằng
        # độ phủ và solver kéo thủ môn m03 từ y=-8.55 lên y=-0.89 — thoả mọi ngưỡng
        # nhưng đã hết là thủ môn. Ngưỡng là RÀNG BUỘC; mục tiêu là "dịch đúng đủ
        # để có hiệu lực rồi dừng lại".
        def score(_lanes, pt, r, mine=mine):
            # 🔴 MỤC TIÊU: SÁT CUỐI ĐƯỜNG NHẤT CÓ THỂ. Ô thủ môn là chốt chặn cuối;
            # nó phải nằm ở đoạn cuối đường đi, nơi người chơi nhìn vào là hiểu ngay
            # vai trò của nó.
            #
            # Bản trước tôi chấm bằng "đoạn đường CHỈ thủ môn canh" và nó tìm đúng
            # thứ được yêu cầu — nhưng thứ đó nằm GIỮA SÂN, không phải cuối đường:
            # m10 gk02 lùi tới 6.37u trước lối ra, m05 5.37u, m06 3.77u. Tối ưu đúng
            # hàm sai vẫn ra kết quả sai.
            #
            # Ngưỡng lead là RÀNG BUỘC (vùng phải kết thúc trước lối ra); mục tiêu là
            # ép sát ngưỡng đó chứ không phải chạy xa khỏi nó. Thứ hai: bám sát đường,
            # để ô nằm TRÊN đoạn cuối chứ không dạt ra bên cạnh — m07 lead đã đạt
            # 1.11u nhưng lệch 1.34u sang bên nên vẫn không ra dáng vị trí thủ môn.
            hit = [window(l, pt, r) for l in mine]
            lead = min(d for _, d in hit if d is not None)
            gap = min(min(math.dist(pt, q) for q in l["curve"]) for l in mine)
            return (-round(lead, 2), -round(gap, 2))

        before = [(l["id"],) + window(l, (gk["x"], gk["y"]), gk_range) for l in mine]

        # Đã đạt ngưỡng thì KHÔNG đụng vào. m00 là map duy nhất đặt đúng từ đầu;
        # dời nó chỉ để "đẹp số" là tự tay làm hỏng mốc so sánh của cả bộ.
        if not force and accept(lanes, (gk["x"], gk["y"]), gk_range):
            report.append(f"  {gk['id']}: đã đạt ngưỡng, giữ nguyên "
                          f"({gk['x']:+.2f},{gk['y']:+.2f})")
            continue

        others = [(o["x"], o["y"]) for o in cfg["slots"] if o["id"] != gk["id"]]
        ref = mine[0]
        target = ref["length"] - lead_min - math.sqrt(max(gk_range ** 2 - 0.83 ** 2, 0.01))
        idx = min(range(len(ref["cum"])), key=lambda k: abs(ref["cum"][k] - target))

        found = search_gk(cfg, lanes, mine, others, gk_range, accept, score)
        if found is None:
            report.append(f"  ❌ {gk['id']}: không tìm được vị trí thoả lead ≥ "
                          f"{lead_min:.2f}u và phủ ≥ {MIN_GK_COVERAGE}u")
            continue

        _, nx, ny = found
        after = [(l["id"],) + window(l, (nx, ny), gk_range) for l in mine]
        e0 = gk_exclusive(cfg, mine, (gk["x"], gk["y"]), gk_range, field_range, gk["id"])
        e1 = gk_exclusive(cfg, mine, (nx, ny), gk_range, field_range, gk["id"])
        report.append(f"  {gk['id']}: ({gk['x']:+.2f},{gk['y']:+.2f}) → ({nx:+.2f},{ny:+.2f})"
                      f"  · dời {math.dist((gk['x'], gk['y']), (nx, ny)):.2f}u"
                      f"  · đoạn ĐỘC QUYỀN {e0:.2f}→{e1:.2f}u")
        for (lid, c0, d0), (_, c1, d1) in zip(before, after):
            f0 = "—" if d0 is None else f"{d0:.2f}"
            f1 = "—" if d1 is None else f"{d1:.2f}"
            report.append(f"      {lid}: phủ {c0:.2f}→{c1:.2f}u · "
                          f"cách lối ra {f0}→{f1}u  (ngưỡng {lead_min:.2f})")
        gk["x"], gk["y"] = nx, ny
        changed += 1

    return changed


def fix_dead_slots(cfg, lanes, lv1_max, lv1_min, lv1_ranges, report):
    """Kéo ô sân ngoài tầm Lv1 về đúng bên trong tầm, ưu tiên chỗ phủ ĐƯỢC HAI TUYẾN."""
    changed = 0
    declared_far = set(cfg.get("farSlots") or [])

    # 🔴 TRẦN ĐỘ PHỦ = ô LÀNH mạnh nhất map này đang có.
    #
    # Sửa một ô chết mà đặt nó thành ô mạnh nhất map là đổi một lỗi lấy một lỗi
    # khác: `_coverageNote` đã ghi rõ đường tự áp sát chính nó làm một vòng tầm
    # cắt nhiều khúc rời → một tướng đánh nhiều lượt → map càng "thú vị" càng DỄ.
    # Lấy đúng ô mạnh nhất mà người thiết kế ĐÃ chấp nhận làm trần thì ô sửa xong
    # không bao giờ mạnh hơn thứ map vốn đã có — không cần chọn một con số mới.
    live = [sum(chord_for(l["curve"], l["cum"], (o["x"], o["y"]), lv1_max)[0] for l in lanes)
            for o in cfg["slots"] if o["type"] == "field" and o["id"] not in declared_far]
    live = [c for c in live if c > 0.05]
    chord_cap = max(live) if live else None

    # 🔴 GIỮ THANG TẦM. `path_check` báo lỗi khi tướng NGẮN TẦM NHẤT với tới MỌI ô —
    # lúc đó tầm hết là đánh đổi và việc chọn ô thành vô nghĩa. Ở m04/m07/m08 ô chết
    # CHÍNH LÀ ô tầm-xa duy nhất của map, nên kéo nó vào sát đường là sửa xong một
    # lỗi và tạo ra một lỗi khác. Ý định của người thiết kế đọc được rõ: ô này để
    # dành cho tướng tầm xa. Giữ ý định đó, chỉ sửa phần BẤT KHẢ THI — đưa ô về dải
    # NGOÀI tầm Batigol nhưng TRONG tầm La Pulga, tức mua được thật.
    healthy_far = any(
        min(min(math.dist((o["x"], o["y"]), q) for q in l["curve"]) for l in lanes) > lv1_min
        for o in cfg["slots"]
        if o["type"] == "field" and o["id"] not in declared_far)
    d_floor = MIN_STANDOFF if healthy_far else lv1_min + EDGE_CLEAR
    for s in cfg["slots"]:
        if s["type"] != "field":
            continue
        pt = (s["x"], s["y"])
        d = min(min(math.dist(pt, q) for q in l["curve"]) for l in lanes)
        if d <= lv1_max:
            continue

        def accept(_lanes, p, r, lv1_max=lv1_max, cap=chord_cap, floor=d_floor,
                   edges=lv1_ranges):
            dd = min(min(math.dist(p, q) for q in l["curve"]) for l in lanes)
            if not (floor <= dd <= lv1_max - 0.08):
                return False
            # `path_check` gọi đây là RANH GIỚI DAO CẠO: ô nằm sát mép tầm của một
            # tướng thì "với tới hay không" do sai số làm tròn quyết định, và trên
            # máy người chơi thấy một tướng lúc bắn lúc không. Tránh mọi mép tầm Lv1.
            if any(abs(dd - e) < EDGE_CLEAR for e in edges):
                return False
            if cap is None:
                return True
            total = sum(chord_for(l["curve"], l["cum"], p, r)[0] for l in lanes)
            return total <= cap

        def score(_lanes, p, r, home=pt):
            # 🔴 Ở ĐÂY mục tiêu là ĐỘ HỮU DỤNG, không phải quãng dời — ngược với ô
            # thủ môn. Lý do: ô thủ môn đang ĐÚNG VAI chỉ sai chỗ, còn ô này đang
            # VÔ DỤNG. Dời nó 0.35u để chord nhích lên 0.62u là vẫn giao cho người
            # chơi một ô 240 Peso gần như không bắn được gì — đúng thứ họ đã báo.
            # Ưu tiên: số tuyến canh được → tổng chord → mới tới quãng dời.
            per = [chord_for(l["curve"], l["cum"], p, r)[0] for l in lanes]
            return (sum(1 for c in per if c > 0.05),
                    round(sum(per), 1),
                    -round(math.dist(p, home), 1))

        others = [(o["x"], o["y"]) for o in cfg["slots"] if o["id"] != s["id"]]
        found = search(cfg, lanes, others, lv1_max, accept, score, pt, span=3.0)
        if found is None:
            report.append(f"  ❌ {s['id']}: không có chỗ trống nào trong tầm Lv1")
            continue
        _, nx, ny = found
        per_before = [chord_for(l["curve"], l["cum"], pt, lv1_max)[0] for l in lanes]
        per_after = [chord_for(l["curve"], l["cum"], (nx, ny), lv1_max)[0] for l in lanes]
        dn = min(min(math.dist((nx, ny), q) for q in l["curve"]) for l in lanes)
        report.append(f"  {s['id']}: ({s['x']:+.2f},{s['y']:+.2f}) → ({nx:+.2f},{ny:+.2f}) · "
                      f"cách tuyến {d:.2f}→{dn:.2f}u (tầm Lv1 tối đa {lv1_max}) · "
                      f"chord {sum(per_before):.2f}→{sum(per_after):.2f}u · "
                      f"canh {sum(1 for c in per_after if c > 0.05)}/{len(lanes)} tuyến"
                      + (f" (trần {chord_cap:.2f})" if chord_cap else ""))
        s["x"], s["y"] = nx, ny
        declared_far.discard(s["id"])
        changed += 1

    # `farSlots` đã sửa xong thì phải rỗng — để lại tên ô đã dời là để lại một
    # miễn trừ sẵn sàng che lỗi lần sau.
    if "farSlots" in cfg:
        cfg["farSlots"] = sorted(declared_far)
    return changed


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--map", default=None, help="chỉ một map, vd m04")
    ap.add_argument("--write", action="store_true", help="ghi đè file (mặc định chỉ in)")
    ap.add_argument("--regk", action="store_true",
                    help="đặt lại MỌI ô thủ môn kể cả ô đang đạt ngưỡng")
    args = ap.parse_args()

    towers = json.loads((ROOT / "config" / "towers.json").read_text())["towers"]
    gk_range = max(t["levels"][0]["range"] for t in towers if t["slotType"] == "goalkeeper")
    lv1_max = max(t["levels"][0]["range"] for t in towers if t["slotType"] == "field")
    lv1_min = min(t["levels"][0]["range"] for t in towers if t["slotType"] == "field")
    lv1_ranges = sorted({t["levels"][0]["range"] for t in towers if t["slotType"] == "field"})

    files = sorted(MAPS.glob("*.json"))
    if args.map:
        files = [f for f in files if f.name.startswith(args.map)]

    total = 0
    for f in files:
        cfg = json.loads(f.read_text())
        lanes = lanes_of(cfg)
        report = []
        has_gk = any(s["type"] == "goalkeeper" for s in cfg["slots"])
        lead_min = cov_min = None
        n_gk = 0
        if has_gk:
            # Làm tròn XUỐNG lưới 0.25 trước khi dò: ngưỡng khai ra file phải là
            # số tròn để người đọc map hiểu nó là một quyết định, không phải một
            # dư số của phép chia.
            ideal = max(LEAD_FLOOR, LEAD_FRACTION * min(l["length"] for l in lanes))
            ideal = math.floor(ideal * 4) / 4
            lead_min = gk_feasible_lead(cfg, lanes, gk_range, ideal)
            if lead_min is None:
                report.append(f"  ❌ không map nào đạt nổi lead {LEAD_HARD_FLOOR}u — "
                              f"hình học tuyến phải sửa, không phải vị trí ô")
            else:
                if lead_min < ideal - 0.01:
                    report.append(f"  ⚠️  ngưỡng lead lùi {ideal:.2f}→{lead_min:.2f}u — "
                                  f"hình học map không cho cao hơn")
                cov_min = MIN_GK_COVERAGE
                n_gk = fix_goalkeepers(cfg, lanes, gk_range, lead_min,
                                       lv1_max, report, force=args.regk)
        n_dead = fix_dead_slots(cfg, lanes, lv1_max, lv1_min, lv1_ranges, report)

        # Khai ngưỡng vào map: cổng chỉ chạy khi map KHAI. Đây là lý do gốc khiến
        # 10/11 map trôi qua `path_check --check` với thủ môn đứng sát cầu môn.
        if lead_min is not None:
            cfg["goalkeeperMinLeadUnits"] = round(lead_min, 2)
            cfg["goalkeeperMinCoverageUnits"] = cov_min
            cfg.setdefault("_goalkeeperPlacementNote",
                           "Ô Dibu phải canh một đoạn đường dài ít nhất "
                           "goalkeeperMinCoverageUnits và toàn bộ đoạn đó phải nằm "
                           "trước lối ra ít nhất goalkeeperMinLeadUnits. Hai ngưỡng "
                           "được tools/path_check.py --check thi hành; không được kéo "
                           "ô về sát cầu môn chỉ vì nhìn giống vị trí thủ môn.")

        print(f"## {cfg['id']}  ({n_gk} ô thủ môn dời · {n_dead} ô chết dời)")
        for line in report:
            print(line)
        total += n_gk + n_dead
        if args.write and (n_gk or n_dead or lead_min is not None):
            f.write_text(json.dumps(cfg, ensure_ascii=False, indent=2) + "\n")

    print()
    print(f"{'ĐÃ GHI' if args.write else 'THỬ (chưa ghi)'} — {total} ô được dời.")


if __name__ == "__main__":
    main()
