#!/usr/bin/env python
"""Xoa checker gia (baked vao RGB, alpha=255) + duong ke luoi cua sprite sheet.

  declak.py SRC DST [--cols N] [--rows N] [--checker LO HI] [--protect-dark N]

Mac dinh = sheet hero (luoi 5x4, checker tong 215/244). Sheet quai dung tong
khac han (122/171) va luoi khac (4x2) nen phai truyen tham so:

  declak.py images/monster_lv1.png out.png --cols 4 --rows 2 \
            --checker 105 185 --protect-dark 100

--checker LO HI : coi moi px sat thap co lum trong [LO,HI] la nen. Khong truyen
                  thi dung cong thuc goc cua sheet hero.
--protect-dark  : px toi hon nguong nay = "chac chan la nhan vat" (vien den).
                  Hero de 130; sheet quai co checker toi 122 nen phai ha xuong 100,
                  neu khong chinh cai nen lai duoc bao ve.
"""
import argparse
import numpy as np
from PIL import Image
from scipy import ndimage

ap = argparse.ArgumentParser()
ap.add_argument("src"); ap.add_argument("dst")
ap.add_argument("--cols", type=int, default=5)
ap.add_argument("--rows", type=int, default=4)
ap.add_argument("--checker", type=int, nargs=2, metavar=("LO", "HI"), default=None)
ap.add_argument("--checker-auto", action="store_true",
                help="tu do dai luminance cua nen tu 2 dinh lon nhat trong histogram "
                     "cua px bao hoa thap. Nen checker chiem 50-70%% anh nen hai dinh "
                     "do luon vuot xa moi thu khac. Dung cai nay thay vi do tay: tong "
                     "checker doi giua cac lan xuat file, do lech 5 don vi la du lam "
                     "hong ket qua.")
ap.add_argument("--protect-dark", type=int, default=130)
ap.add_argument("--hline", type=int, action="append", default=[],
                help="xoa mot duong ke NGANG con sot o toa do y nay (lap lai duoc). "
                     "Dung khi luoi VE tren sheet khac luoi O logic — vd sheet boss "
                     "co ke ngang o y=384 nhung nhan vat cao ca 768. Chi xoa o cot "
                     "ma tren/duoi deu da la nen, nen khong an vao vien nhan vat.")
ap.add_argument("--min-blob", type=int, default=0,
                help="xoa not moi mang lien thong nho hon N px (0 = tat). "
                     "Sheet nhieu chi tiet roi (bui, khoi) hay sot vun checker; "
                     "kiem tra phan bo kich thuoc mang de chon nguong nam DUOI "
                     "chi tiet that nho nhat.")
args = ap.parse_args()

SRC, DST = args.src, args.dst
COLS, ROWS = args.cols, args.rows
INSET = 3           # ne mep o mot chut khi tim seed nen
PROTECT_ITERS = 3   # gian no vung "chac chan la nhan vat"
SEAM_BAND = 9       # be rong dai xoa quanh moi duong ke luoi
HLINE_REACH = 14    # --hline: no bao nhieu cot tu vung nen vao sat than nguoi

im = Image.open(SRC).convert("RGBA")
arr = np.array(im).astype(np.int16)
H, W = arr.shape[:2]
rgb = arr[:, :, :3]
alpha = np.array(im)[:, :, 3]

mx = rgb.max(axis=2)
mn = rgb.min(axis=2)
sat = mx - mn
lum = rgb.mean(axis=2)

# --checker-auto: hai dinh lon nhat cua histogram lum (tren px bao hoa thap) chinh
# la hai tong o checker — chung chiem qua nua anh nen khong the nham voi nhan vat.
if args.checker_auto:
    lowsat = lum[sat <= 25]
    hist, edges = np.histogram(lowsat, bins=np.arange(0, 261, 5))
    order = np.argsort(hist)[::-1]
    peaks = []
    for i in order:
        c = edges[i] + 2.5
        if all(abs(c - q) >= 25 for q in peaks):
            peaks.append(c)
        if len(peaks) == 2:
            break
    lo_p, hi_p = min(peaks), max(peaks)
    args.checker = [int(lo_p - 12), int(hi_p + 12)]
    # Nguong "chac chan la nhan vat" phai nam DUOI dai nen, khong thi chinh cai nen
    # toi lai duoc bao ve. Vien den cua nhan vat o lum < 40 nen con nhieu cho.
    args.protect_dark = min(args.protect_dark, args.checker[0] - 6)
    print(f"  checker-auto: dinh {lo_p:.0f} / {hi_p:.0f} "
          f"-> dai {args.checker[0]}..{args.checker[1]}, protect-dark {args.protect_dark}")

# o checker: mac dinh = tong cua sheet hero (xam nhat 215 / trang 244), bao hoa ~0.
# Truyen --checker LO HI thi doi sang mot dai luminance bat ky.
if args.checker:
    lo, hi = args.checker
    checker = (sat <= 25) & (lum >= lo) & (lum <= hi)
else:
    checker = (sat <= 22) & ((np.abs(lum - 200) <= 24) | (lum >= 236))
# nhan vat: co mau ro, hoac toi (vien den / toc / quan)
protect_seed = (sat > 30) | (lum < args.protect_dark)
protect = ndimage.binary_dilation(protect_seed, iterations=PROTECT_ITERS)

erase = np.zeros((H, W), bool)

# --- Pha 1: theo tung o, flood tu mep vao ---
xs = [round(c * W / COLS) for c in range(COLS + 1)]
ys = [round(r * H / ROWS) for r in range(ROWS + 1)]

for r in range(ROWS):
    for c in range(COLS):
        y0, y1 = ys[r] + INSET, ys[r + 1] - INSET
        x0, x1 = xs[c] + INSET, xs[c + 1] - INSET
        sub = checker[y0:y1, x0:x1]
        lab, n = ndimage.label(sub)
        if n == 0:
            continue
        edge = set(lab[0, :]) | set(lab[-1, :]) | set(lab[:, 0]) | set(lab[:, -1])
        edge.discard(0)
        if not edge:
            continue
        bg = np.isin(lab, list(edge))
        erase[y0:y1, x0:x1] |= bg

# khong xoa vung da duoc bao ve (soc ao trang nam sat mep o)
erase &= ~protect

# --- Pha 1b: lan tu nen da xoa ra vung xam ke ben (halo/bong do quanh nhan vat) ---
# vung xam nhat noi rong: bat ca cac tong 225..235 lot khe giua 2 o checker
if args.checker:
    lo, hi = args.checker
    grayish = (sat <= 26) & (lum >= lo - 10) & (lum <= hi + 10) & ~protect
else:
    grayish = (sat <= 26) & (lum >= 165) & ~protect
erase = ndimage.binary_propagation(erase & grayish, mask=grayish) | erase

# --- Pha 2: xoa vo dieu kien dai quanh moi duong ke luoi ---
seam_mask = np.zeros((H, W), bool)
for x in xs:
    seam_mask[:, max(0, x - SEAM_BAND):min(W, x + SEAM_BAND)] = True
for y in ys:
    seam_mask[max(0, y - SEAM_BAND):min(H, y + SEAM_BAND), :] = True
# trong dai seam khong co px nhan vat (da do: sat>60 = 0 px) -> xoa moi px xam thuan,
# gom ca checker, duong ke den va vien anti-alias xam (lum ~163) giua chung
seam_erase = seam_mask & (sat <= 30)
erase |= seam_erase

alpha = alpha.copy()
alpha[erase] = 0

# --- Pha 2b: duong ke ngang le loi (khong nam tren luoi o logic) ---
# Chi dong vao cot NEN: neu cach dai 14px ve hai phia deu da trong suot thi thu
# con lai trong dai chac chan la ke/halo, khong phai nguoi. Cot nao co than nguoi
# cat qua thi bo qua han -> vien den cua nhan vat con nguyen.
for hy in args.hline:
    lo, hi = max(0, hy - SEAM_BAND), min(H, hy + SEAM_BAND + 1)
    above, below = max(0, hy - 14), min(H - 1, hy + 14)
    bg_col = (alpha[above] == 0) & (alpha[below] == 0)
    strip = np.zeros((H, W), bool)
    strip[lo:hi, :] = bg_col[None, :]
    # No rong sang HLINE_REACH cot mỗi bên: ke thuong tho them vai pixel vao sat
    # than nguoi truoc khi bi vien den chan lai. Chi no theo COT, khong theo mau —
    # thu trong long nhan vat (phu hieu, chu so, quan trang) cach mep hang tram
    # pixel nen khong bao gio voi toi. Da thu loc theo do sang thay vi cach nay:
    # cua so lum an mat phan trang cua phu hieu tren nguc. Dung no cot.
    wide = ndimage.binary_dilation(bg_col[None, :], np.ones((1, 2 * HLINE_REACH + 1), bool))[0]
    strip = np.zeros((H, W), bool)
    strip[lo:hi, :] = wide[None, :]
    hit = strip & (sat <= 35) & (alpha > 0)
    print(f"  xoa ke ngang y={hy}: {int(hit.sum())} px "
          f"({int(bg_col.sum())} cot nen → {int(wide.sum())}/{W} cot sau khi no)")
    alpha[hit] = 0

# --- Pha 3: don vun. Chi chay khi duoc yeu cau, va nguong phai nam duoi chi
# tiet that nho nhat (vd bui khoi ~1000px) de khong an mat chi tiet. ---
if args.min_blob > 0:
    lab, n = ndimage.label(alpha > 0)
    sizes = ndimage.sum(alpha > 0, lab, range(1, n + 1))
    tiny = np.zeros(n + 1, bool)
    tiny[1:] = sizes < args.min_blob
    drop = tiny[lab]
    print(f"  don vun: bo {int(drop.sum())} px trong {int(tiny.sum())} mang <{args.min_blob}px")
    alpha[drop] = 0
out = np.array(im)
out[:, :, 3] = alpha
Image.fromarray(out).save(DST)
print(f"da xoa {erase.sum()} px / {H*W} ({erase.sum()*100/(H*W):.1f}%) -> {DST}")
