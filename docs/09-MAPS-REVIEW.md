# 09 — REVIEW TOÀN BỘ 10 MAP

> **Trạng thái:** 🟢 User đã duyệt; G1 + đợt 1 đã làm (2026-08-21). Xem `docs/08` §11.
> L1/L2/L3 và Q1–Q5 đã xử lý — ghi kết quả ngay dưới mỗi mục.
> **Đầu vào:** `docs/08-MAPS-ARCHITECTURE.md` + `docs/maps/M01..M10.md`
> **Tài liệu này là:** vòng soi lại sau khi có đủ 10 plan — cái gì sai, cái gì mâu thuẫn, cái gì phải sửa trước khi code.

---

## 1. Bảng đối chiếu 10 map

| Map | Tuyến | Dài (u) | Ô sân | Kháng chậm | Khó | Cơ chế mới | Cần đổi kiến trúc? |
|-----|------:|--------:|------:|-----------:|:---:|------------|:---:|
| M01 El Potrero | 1 | 43.85 | 11 | 75% | ★ | zigzag, vòng tầm cắt 2 đoạn | ❌ không |
| M02 La Bombonera | 1 | 44.49 | 10 | 75% | ★★ | combo pocket | ❌ không |
| M03 Dos Ríos | 2 | 38.06 / 38.00 | 12 | 78% | ★★ | hai tuyến luân phiên | ✅ lane |
| M04 El Cruce | 2 | 23.29 / 32.47 | 12 | 80% | ★★★ | hai tuyến đồng thời, cắt nhau | ✅ lane |
| M05 La Confluencia | 2→1 | 28.31 / 28.40 | 11 | 80% | ★★★ | hợp lưu | ✅ lane |
| M06 El Caracol | 1 | 52.18 | 9 | 82% | ★★★★ | xoáy ốc, qua cụm ô 2 lần | ❌ không |
| M07 Tres Puertas | 3 | 24.40 / 22.12 / 23.52 | 13 | 85% | ★★★★ | ba tuyến lệch nhịp | ✅ lane + delay |
| M08 El Mirador | 2 | 24.83 / 23.56 | 10 | 85% | ★★★★ | ô tầm xa | ✅ lane |
| M09 La Horquilla | 1→2→1 | 28.11 / 30.63 | 11 | 88% | ★★★★★ | tách rồi nhập | ✅ lane |
| M10 La Muralla Final | 3 | 22.51 / 20.43 / 21.17 | 12 | 90% | ★★★★★ | tổng hợp + 2 boss | ✅ lane |

**Ba map (M01, M02, M06) chạy được trên engine hôm nay** — chỉ cần dữ liệu + tải config theo map + sửa lớp vẽ.
Đây là lát cắt rẻ nhất, xem §6.

---

## 2. 🔴 Lỗi CHẶN — phải sửa trước khi code

### L1. Toàn bộ thang "kháng chậm 75→90" là núm xoay GIẢ

Chi tiết đầy đủ ở `docs/08` §3B. Tóm tắt: cả `nhan_quan` (+25%) lẫn `ban_thang_the_ky` (+20%) đều kiểm
`slowOnTarget > 0` — **nhị phân**. Kháng 75% và kháng 90% cho **cùng một** hệ số 1.50×.

**Ảnh hưởng:** cột "kháng chậm" trong bảng §1 hiện **vô nghĩa** ở 9/10 map. Mọi lập luận
"map cao hơn thì synergy yếu hơn" trong 10 file đều dựa trên tiền đề sai.

**Phải làm:** user chọn (A) ngưỡng hay (B) buff tỉ lệ → sửa `AbilityEngine` ở G1 → **rồi mới**
dẫn xuất lại máu boss cho cả 10 map.

### L2. M06 VI PHẠM luật đoạn hở — số trong file sai  → ✅ ĐÃ SỬA

**Cách sửa (đo, không phải nới cho qua):** dò TOÀN BỘ vị trí khả dĩ cho ô yếu nhất cho thấy
9 ô trên đường 52.18u **không thể** đạt 15% — tối ưu tuyệt đối là 16.3%. Dời `f07`
(-3.70,-4.40) → (-0.60,-8.70) chạm đúng tối ưu đó; đánh đổi đo được: phủ ở R=1.0 tăng
11.1% → 16.5% (đúng chỗ thủng, đầu trận), phủ ở R=1.8 giảm 73.0% → 69.8%. Đặt
`maxGapFraction: 0.165` cho riêng m06, có ghi lý do trong file. Vị trí đầu tiên thử
(y=-9.00) bị `path_check` bắt vì vùng chạm tràn mép dưới 0.06u → nâng lên -8.70.



Tôi dựng lại toạ độ M06 và chạy `tools/path_check.py` **gốc**:

```
Đoạn đường dài nhất không ô nào canh: f=0.27→0.45 = 17% (8.9 units)  ❌
❌ VI PHẠM: quân đi tự do qua đó
```

File M06 ghi "khe hở 8% ✅". Lệch vì file đo ở tầm cấp cao, còn `path_check` đo ở **tầm Lv1**.
M06 là map **một tuyến** nên không viện được lý do "đa tuyến làm thưa ô" — 9 ô trên 52.18u là thưa thật
(map gốc: 11 ô trên 42.28u).

**Phải làm:** hoặc thêm ô, hoặc rút ngắn xoáy ốc, hoặc tuyên bố khe hở là cơ chế cố ý và nới luật CÓ LẬP LUẬN.
Không được để nguyên rồi hy vọng.

### L3. `path_check.py` báo "ô chết" GIẢ trên map đa tuyến

Ô phục vụ tuyến B bị tính là chết vì nó xa tuyến A. `--check` sẽ exit 1 và **chặn build** dù map đúng.
Ảnh hưởng 7/10 map. Phải sửa ở G1 (`--map`, `d = min` qua các tuyến) — `docs/08` §8 đã ghi.

### L4. Sáu map đa tuyến CHƯA được xác minh độc lập

| Map | Xác minh bằng `path_check.py` gốc |
|-----|---|
| M01 | ✅ khớp tuyệt đối (43.85u lệch 0.0%, khe hở 14%, 51pt) |
| M02 | ✅ khớp tuyệt đối ("mọi ràng buộc đạt", khe hở 14%, 63pt) |
| M06 | ❌ **sai** (xem L2) |
| M03, M04, M05, M07, M08, M09, M10 | ⬜ **không kiểm được** — công cụ gốc không biết đa tuyến |

Mỗi agent tự viết một bản phái sinh riêng để đo, và **M06 chứng minh bản phái sinh có thể sai**.
→ Số hình học của 6 map đa tuyến phải coi là **CHƯA XÁC MINH** cho tới khi có công cụ đa tuyến chung.

---

## 3. 🟡 Mâu thuẫn trong chính đề bài của tôi — agent phản biện ĐÚNG

| # | Tôi giao | Thực tế | Xử lý |
|---|----------|---------|-------|
| C1 | M05: "quân lọt trước khi tới điểm hợp lưu" | **Bất khả về hình học** — mọi con đều phải đi qua hợp lưu để tới lưới. Muốn rò rỉ ở nhánh thì cần **hai cầu môn phụ**, tức map khác | Chấp nhận cách agent viết lại (bão hoà thông lượng + thân chung 43%) |
| C2 | M01 "phải dễ hơn map gốc" | Hai đòn bẩy làm dễ đều nhỏ; mà giảm số quân lại **giảm tiền theo** → có thể không dễ hơn thật | Phải ĐO, không tuyên bố |
| C3 | Ít ô = khó hơn | **Sai.** Trần chi tiêu = `ô × 1020 + 476`, nên ít ô buộc **cắt số quân** (M02 −14%, M06 −25%), tức map NHẸ đi về khối lượng | Ít ô làm khó ở khâu CHỌN, không ở khối lượng. Sửa lại lập luận độ khó |

C3 là phát hiện đáng giá nhất của vòng này: **số ô không phải núm độ khó** — nó là núm *trần kinh tế*.
Hai agent tìm ra độc lập.

---

## 4. Rủi ro xuyên suốt

| # | Rủi ro | Bằng chứng | Giảm thiểu |
|---|--------|-----------|------------|
| X1 | **Máu boss trong docs chỉ đúng cho đường 42.28u** | Tuyến ở 10 map dài 20.43–52.18u. Chuẩn đo: ~237 máu / tower-giây | Dẫn xuất lại theo tower-giây từng map, **sau** khi chốt L1 |
| X2 | **Không được cộng chord của mọi tuyến** | M09 `chord/2R` tb = 1.41, M06 = 1.29 (ô đếm 2 lần) → đánh giá cao phe thủ 20–40% | Headroom tính **theo từng tuyến** |
| X3 | **Map đa tuyến rò rỉ hơn về cấu trúc** | Độ phủ 46–55% (M04/M07) vs 68% map gốc, dù số ô bằng/hơn. M07 `oeste` đã ở 14% | `MAX_GAP` thành tham số theo map (`docs/08` §3C) |
| X4 | **Đỉnh quân đồng thời chưa đo cho map đa tuyến** | Map gốc đo được **27**. M03 ước 30–34; M07/M10 3 tuyến chưa ai ước | Trần 40; đo trong chính test winnability |
| X5 | **220 wave phải cân** | Vòng 22 cân MỘT map tốn cả buổi và lộ ra mô hình sai | Tự động hoá quét `startingCash`; chấp nhận "chơi được" trước "hoàn hảo" |

---

## 5. Hiệu chỉnh đề xuất

1. **Bỏ cột "kháng chậm" khỏi thang độ khó** cho tới khi chốt L1. Nếu chọn (A) ngưỡng thì chỉ còn
   **hai** mức có nghĩa: 75 (synergy sống) và 85 (synergy chết) — dùng như CÔNG TẮC, không phải thang điểm.
2. **Viết lại lập luận độ khó của M02/M06** theo C3: khó vì *chọn ô*, không vì *khối lượng quân*.
3. **M06 phải sửa hình học** (L2) trước khi vào bất kỳ giai đoạn nào.
4. **Hoãn mọi con số máu boss** tới sau L1 + công cụ đa tuyến. Hiện chúng là ước lượng chồng ước lượng.
5. **Thêm luật validator #21** (trần chi tiêu theo map) — vòng 22 đã cho thấy ghi chú không đủ.

---

## 6. Đề xuất cắt phạm vi — lát cắt rẻ nhất trước

Nếu làm cả 10 map cùng lúc thì rủi ro X5 (220 wave) sẽ nuốt toàn bộ thời gian.
Đề xuất chia làm hai đợt, **đợt 1 không cần đụng kiến trúc đa tuyến**:

**Đợt 1 — 3 map một tuyến (M01, M02, M06).**
Chỉ cần: schema map + tải config theo map + sửa `path_check`/`balance_sim` nhận `--map` + 3 test winnability.
KHÔNG cần `LaneId`, KHÔNG cần đổi `Enemy`/`ScheduledSpawn`, KHÔNG cần vẽ nhiều đường.
→ Xong đợt này là có **4 map chơi được** (kể cả map gốc) và nền đa map đã đứng.

**Đợt 2 — 7 map đa tuyến (M03, M04, M05, M07, M08, M09, M10).**
Đây là đợt có `LaneId` vào vòng lặp mô phỏng, và là đợt đắt.

Lợi ích của việc tách: sau đợt 1 bạn đã **chơi thử được map mới trên máy thật** và biết cảm giác có đúng
không, trước khi bỏ công vào phần đắt nhất.

---

## 7. Câu hỏi cần user quyết (gộp cả `docs/08` §10)

| # | Câu hỏi | Đề xuất của tôi |
|---|---------|-----------------|
| Q1 | Kháng chậm: (A) ngưỡng hay (B) buff tỉ lệ? | **(B)** — núm xoay thật, quân thường không đổi. Cái giá: đo lại máu boss |
| Q2 | Giữ map gốc thành `m00` (tổng 11 map)? | **Giữ** — nó là map đã cân xong, dùng làm mốc đối chiếu |
| Q3 | Làm màn chọn map lần này? | **Hoãn** — đổi map bằng config là đủ để chơi thử; màn chọn là scene thứ hai, scope riêng |
| Q4 | Cắt phạm vi? | **Có** — làm đợt 1 (3 map một tuyến) trước, xem cảm giác rồi mới quyết đợt 2 |
| Q5 | M06 khe hở 17% xử sao? | Rút ngắn xoáy ốc hoặc thêm 1 ô. Không nới luật khi chưa có lý do thiết kế |

---

## 8. 🔴 L5 — KINH TẾ ĐA TUYẾN: mọi map đa tuyến đều không thắng nổi ở tiền khởi đầu đã thiết kế

**Đo 2026-08-22, engine thật, `GreedyPlayer` 20 wave, đúng số trong `config/maps/`:**

| map | tuyến | ô sân | tiền đầu (thiết kế) | kết cục |
|-----|------|-------|---------------------|---------|
| m00 | 1 | 11 | 700 | ✅ thắng, còn 11/20 máu |
| m01 | 1 | 11 | 700 | ✅ thắng, còn 10 |
| m02 | 1 | 10 | 700 | ✅ thắng, còn 9 |
| m06 | 1 | 9 | 600 | ✅ thắng, còn 3 |
| m03 | 2 | 12 | 700 | ❌ **thua W4** |
| m04 | 2 | 12 | 700 | ❌ **thua W6** |
| m05 | 2 | 11 | 680 | ❌ **thua W9** |
| m07 | 3 | 13 | 730 | ❌ **thua W5** |
| m08 | 2 | 10 | 760 | ❌ **thua W5** |
| m09 | 2 | 11 | 780 | ❌ **thua W7** |
| m10 | 3 | 12 | 700 | ❌ **thua W4** |

**4/4 map một tuyến thắng. 7/7 map đa tuyến thua, và thua ở act 1.** Đó không phải
ngẫu nhiên, cũng không phải "cân hơi lệch".

### 8.1 Nguyên nhân — một câu

**Một khẩu chỉ bắn tuyến nó với tới, nên hai tuyến là HAI hàng thủ phải nuôi cùng
lúc; nhưng thu nhập thì không nhân đôi theo.**

m03 có 476 quân cả trận, m00 có 465 — thu nhập gần y hệt. Nhưng m00 mỗi khẩu bắn
**mọi** con quái trong trận, còn m03 khẩu đơn tuyến chỉ gặp **một nửa** (wave lẻ ra
`L1`, wave chẵn ra `L2`). Cùng một ví, phải mua gấp đôi số khẩu để đạt cùng mức
sát thương trên mỗi con.

### 8.1b Chuyện này đã xảy ra một lần, trên map gốc

`config/economy.json` → `_startingCashNote` ghi lại vòng 22: m00 từng thua vì
"W6 mới có 2 tướng trong khi wave đã 19 con", và thứ chữa được là **`startingCash`
550 → 700**, không phải bounty ("nâng bounty thì tiền dồn về cuối trận, chỗ vốn đã
dư; nâng startingCash đổ đúng vào chỗ thiếu").

Map đa tuyến là **đúng căn bệnh đó, nhân với số tuyến** — nên cũng đúng thang thuốc
đó, với liều lớn hơn. Đây không phải núm mới; nó là núm đã dùng, ở map khó hơn.

### 8.2 Vì sao bản thiết kế không thấy

Bản thiết kế từng map tính "tower-giây" theo **tổng chord qua mọi tuyến** — coi một
ô canh 1.4u trên `L1` cộng 1.4u trên `L2` là ô mạnh 2.8u. Sai ở chỗ tướng bắn **một
mục tiêu một lúc**: canh hai tuyến làm khẩu súng BẬN nhiều hơn, không làm nó MẠNH
hơn. Chỉ số `coveragePerSlot` mà `path_check.py` đang ép cũng mang đúng lỗi này —
nó cộng chord qua các tuyến.

### 8.3 Ba núm, và núm nào làm gì

Đã quét trên engine (mỗi điểm chạy 3 hạt RNG + một mức tiền thấp hơn để loại điểm
dao):

| núm | tác dụng thật |
|-----|---------------|
| `economy.startingCash` | núm của **giai đoạn đầu**. Đây là núm duy nhất cứu được map đa tuyến — và cũng đúng thứ user yêu cầu ("tiền bắt đầu phù hợp với từng map") |
| `hpScaling.growthPerWave` | núm của **độ khó cuối**: máu quái lên, tiền thưởng đứng yên |
| số con mỗi nhóm (`--scale`) | núm của **trần kinh tế**, KHÔNG phải núm độ khó — giảm quân là giảm thu nhập, và đã đo được trường hợp giảm quân làm map KHÓ hơn |

### 8.4 Hai thứ đã loại trừ trước khi kết luận

1. **"Con bot mua ngu"** — đã viết lại `GreedyPlayer` cho biết trọng số tuyến, rồi
   thử cả luật ép cân tuyến yếu nhất. Map một tuyến tốt lên (m02 7→9, m06 2→3),
   map đa tuyến **không** map nào thoát; luật ép cân còn làm m03 thua sớm hơn 2 wave
   vì dàn mỏng sớm làm hoả lực loãng.
2. **"Thiếu lệch nhịp"** — bản thiết kế M07 §5.1 quy định ba tuyến ra lệch
   0/5.6/11.2 s mà file config dựng thiếu. Đã bù (`_laneDelayNote`), xác minh lịch
   tới đúng engine (t = 0.00 / 5.60 / 11.20). Kết cục **không đổi một chữ**: ở W5
   người chơi mới có 2–3 khẩu, giãn nhịp chỉ đổi quân lọt cùng lúc thành lọt lần lượt.

3. **"Bot không đọc được thứ người chơi đọc được"** — đây là điều DUY NHẤT có tác
   dụng thật. Bản thiết kế M03 §3 ghi rõ: map luân phiên **bắt buộc** HUD phải báo
   trước wave sau ra cửa nào, không có thì là trò tung đồng xu. Cái đó chưa dựng,
   nên con bot mua mù. Đã dựng (`MatchController.LanesOfWave` + nhãn `CỬA:` trong
   lúc chuẩn bị) và cho bot dùng đúng thông tin ấy — trọng số tuyến = ½ nhu cầu cả
   trận + ½ nhu cầu wave kế tiếp. Đo lại ở đúng số thiết kế:

   | map | trước | sau |
   |-----|-------|-----|
   | m05 | thua W9 | thua **W18** |
   | m10 | thua W4 | thua **W6** |
   | m08 | thua W5 | thua W4 |
   | m00/m01/m02/m06 | 11 / 10 / 9 / 3 | **không đổi** |

   Map một tuyến không đổi một số nào — đúng như phải thế, vì trên một tuyến trọng
   số là hằng số nên thứ tự mua y hệt. Nhưng ngay cả với thông tin đó, **7/7 map đa
   tuyến vẫn thua**. Kết luận ở §8.1 đứng.


### 8.5 Đã cân nhắc "sửa lại đường đi" — và đo ra là KHÔNG chữa được

Giả thuyết hiển nhiên: tuyến map đa tuyến ngắn quá (m10 chỉ 20–22u so với 42.3u của
m00) nên phe thủ có ít thời gian bắn. Kéo dài tuyến ra là chữa được?

**Không.** Đo mật độ phủ — `Σchord ÷ độ dài tuyến`:

| tuyến | dài | Σchord@1.4 | chord mỗi unit đường |
|-------|-----|-----------|----------------------|
| m00 `L1` | 42.3u | 19.1 | **0.452** |
| m10 `O` | 22.5u | 7.2 | 0.320 |
| m10 `C` | 20.4u | 11.5 | 0.562 |
| m10 `E` | 21.2u | 9.1 | 0.431 |

Mật độ đã tương đương m00. Kéo dài tuyến mà **không thêm ô** thì phần đường mới nằm
ngoài tầm mọi ô — Σchord đứng yên, chỉ tuyến dài ra. Còn nếu rải lại 12 ô cho đều
trên ba tuyến dài 35u thì mỗi ô canh ~1.6u, tức Σchord toàn map **giảm** từ 27.8
xuống ~19 — map còn khó hơn.

Tính theo mỗi con quái, m10 cũng không thiếu phủ: `O` 0.035 · `C` 0.049 · `E` 0.054
so với mốc m00 0.041. **Chỗ thiếu không phải hình học, mà là VÍ TIỀN ở act 1** —
đúng kết luận §8.1. Vì vậy núm là `startingCash`, không phải waypoint.


### 8.6 Một lỗi trong chính bộ dò, và nó đã làm lệch mọi kết quả trước đó

Bộ dò tham số lọc điểm bằng ba ràng buộc: thắng 20 wave · đỉnh quân ≤ 40 · **tiền
kiếm cả trận < trần chi tiêu**. Ràng buộc thứ ba tôi viết là `Earned < trần × 0.95`
— tự thêm 5% biên an toàn.

m00 — map mốc, map đã cân xong — nằm ở **96.1%** (11 245 / 11 696). Nghĩa là biên 5%
bắt 10 map mới qua một luật mà **chính map gốc trượt**. Hậu quả đo được: bộ dò loại
sạch các điểm giữ nguyên số quân và chỉ còn các điểm cắt quân xuống **k = 0.6** —
tức đề nghị bỏ 40% nội dung của map, vì một con số tôi tự bịa chứ không phải vì map
không chơi được.

Kiểm chứng bằng số học, không cần chạy lại: số quân trên mỗi ô sân so với m00 —

| map | quân/ô | so m00 | k thật sự cần để về mốc |
|-----|-------:|-------:|------------------------:|
| m08 | 48.2 | ×1.14 | 0.88 |
| m09 | 48.1 | ×1.14 | 0.88 |
| m10 | 50.7 | ×1.20 | 0.83 |
| m03–m07 | 39.7–43.6 | ×0.94–1.03 | ~1.00 |

Trần chi tiêu chỉ đòi k ≈ 0.83–0.88 ở ba map, và **không đòi gì** ở năm map còn lại.
Đã sửa về đúng luật (`Earned < trần`) và quét lại.

**Bài học ghi lại vì nó sẽ tái diễn:** thêm biên an toàn vào một ràng buộc mà mình
không đo lại trên MỐC là cách âm thầm nhất để đổi kết luận. Ràng buộc trong bộ dò
phải là ĐÚNG ràng buộc trong test, không phải phiên bản chặt hơn "cho chắc".
