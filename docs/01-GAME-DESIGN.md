# 01 — Game Design

> Nguồn chân lý cho **luật chơi**. Số liệu cụ thể sống ở [`04-ECONOMY-BALANCE.md`](04-ECONOMY-BALANCE.md).

---

## 1. Fantasy

Người chơi là HLV hàng thủ Argentina. Không điều khiển cầu thủ chạy — mà **quyết định ai đứng ở đâu**. Mỗi quyết định đặt tướng là một canh bạc kinh tế: tiền bỏ vào tướng đắt bây giờ hay giữ để nâng cấp sau.

Cổ động viên Bồ Đào Nha không phải kẻ thù "ác" — họ chỉ đang tràn sân ăn mừng. Tông màu vui, hài, không bạo lực. Tướng "hạ" cổ động viên = đẩy họ ngã ra khỏi sân, không phải giết.

## 2. Core Loop

```
Chuẩn bị  →  Wave chạy  →  Nhận tiền  →  Mua / Nâng cấp  →  Wave tiếp
   ↑                                                            │
   └────────────────────────────────────────────────────────────┘
                    (lặp 20 lần, hoặc tới khi thua)
```

Vòng lặp trong 1 wave:
1. Cổ động viên spawn ở vạch giữa sân, đi theo đường cố định về phía cầu môn
2. Tướng trong tầm tự động tấn công (không cần tap)
3. Cổ động viên bị hạ → rơi tiền → cộng vào ví ngay
4. Cổ động viên lọt vào cầu môn → trừ máu cầu môn → biến mất
5. Hết cổ động viên trên sân → wave clear → thưởng clear → mở wave sau

## 3. Điều kiện thắng / thua

| Kết quả | Điều kiện |
|---------|-----------|
| **THẮNG** | Clear đủ 20 wave, máu cầu môn > 0 |
| **THUA** | Máu cầu môn = 0 tại bất kỳ thời điểm nào |

Máu cầu môn khởi đầu: **20 bàn thua**. Không tự hồi (trừ khi mua sink "Sửa cầu môn" hoặc Dibu Lv3 — xem `04`).

**Xếp hạng cuối trận (3 sao):**

| Sao | Điều kiện |
|-----|-----------|
| ⭐ | Thắng, máu còn 1–9 |
| ⭐⭐ | Thắng, máu còn 10–19 |
| ⭐⭐⭐ | Thắng, máu còn đúng 20 (clean sheet — không thủng lưới lần nào) |

💡 *Clean sheet là ngôn ngữ bóng đá, không phải "perfect run" chung chung — dùng đúng từ của môn thể thao là cách rẻ nhất để theme thấm vào cơ chế.*

## 4. Các phase trong 1 trận

| Phase | Thời lượng | Người chơi làm gì | Hệ thống làm gì |
|-------|-----------|-------------------|-----------------|
| **Kickoff** | Vô hạn (chờ tap) | Xem map, đặt tướng khởi đầu bằng 550 Peso | Hiện đường chạy, các ô đặt tower, nút "Bắt đầu" |
| **Wave** | 30–120s tuỳ wave | Đặt / nâng cấp / bán tướng (được phép **trong lúc** wave chạy) | Spawn, di chuyển, chiến đấu, trả tiền |
| **Nghỉ giữa wave** | 8s (skip được) | Mua sắm | Đếm ngược; tap "Sẵn sàng" → vào wave sau ngay, được thưởng skip |
| **Kết trận** | — | Xem điểm | Tính sao, hiện nút Chơi lại / Về menu |

**Thưởng skip:** tap "Sẵn sàng" khi còn `N` giây → thưởng `N × 3` Peso. Ép người chơi giỏi phải cân: an toàn (chờ đủ) hay tham (skip lấy tiền).

💡 *Đây là cơ chế duy nhất trong MVP thưởng cho việc chơi mạo hiểm. Không có nó, người chơi tối ưu sẽ luôn chờ hết 8 giây và phase nghỉ trở thành thời gian chết vô nghĩa.*

## 5. Điều khiển (một tay, dọc màn hình)

| Hành động | Thao tác |
|-----------|----------|
| Đặt tướng | Tap ô trống → hiện radial menu 6 đơn vị (kèm giá, xám nếu không đủ tiền) → tap chọn |
| Xem thông tin tướng | Tap tướng đã đặt → panel dưới hiện chỉ số + tầm đánh (vòng tròn trên sân) |
| Nâng cấp | Tap tướng → tap nút "Nâng cấp" ở panel |
| Bán | Tap tướng → giữ nút "Bán" 0.6s (chống lỡ tay) → hoàn 60% tổng tiền đã đầu tư |
| Tăng tốc | Tap nút ×2 góc trên phải (toggle 1× / 2×) |
| Tạm dừng | Tap nút ⏸ góc trên trái |

**Không có:** kéo-thả (dễ trượt), gesture đa chạm, xoay màn hình. Màn hình dọc cố định.

## 6. Map (MVP: 1 map duy nhất)

- Sân bóng nhìn từ trên xuống, màn hình dọc
- **1 đường chạy duy nhất**, uốn hình chữ S từ vạch giữa sân xuống cầu môn — độ dài ~40 đơn vị
- **11 ô đặt tướng sân**, rải hai bên đường; ô gần đường có tầm phủ tốt hơn nhưng ít hơn
- **1 ô thủ môn** ngay trước cầu môn — chỉ đặt được Dibu, không đặt được tướng khác
- Cầu môn ở cuối đường: khung thành + lưới, hiện thanh máu ngay trên xà ngang

Nhiều đường / nhiều map = **ngoài phạm vi MVP** (xem `07-ROADMAP.md` M3).

## 7. Phạm vi MVP

### Có
- 1 map, 1 đường chạy, 20 wave
- **6 đơn vị**: 5 cầu thủ + **1 trọng tài** (`El Árbitro`), mỗi cái 3 cấp — **mỗi cấp mở 1 kỹ năng mới, cộng dồn → 18 kỹ năng**
- 3 loại cổ động viên thường + **1 boss `O Capitão`** ở W10 và W20
- Kinh tế: mua / nâng cấp / bán / thưởng skip / sink sửa cầu môn
- Thắng / thua / 3 sao
- Lưu tiến độ cục bộ (sao cao nhất đạt được)

### Không có (chốt lại lần nữa cho khỏi trôi scope)
- ❌ Nhiều map / nhiều đường chạy
- ❌ Loại cổ động viên thường thứ 4 (boss là set-piece, không phải quân thường — xem `03` §2)
- ❌ Boss có kỹ năng — `O Capitão` chỉ là khối máu đi chậm
- ❌ Meta-progression giữa các trận (cây kỹ năng vĩnh viễn, thẻ tướng)
- ❌ IAP / quảng cáo
- ❌ Multiplayer / leaderboard online
- ❌ Âm thanh ngoài SFX cơ bản + 1 track nhạc nền
- ❌ Cutscene / cốt truyện

## 8. Failure modes cần xử lý (dev phải cover)

| ID | Tình huống | Hành vi đúng |
|---|-----------|--------------|
| FM-01 | Người chơi tap ô đã có tướng | Mở panel thông tin, không mở menu đặt |
| FM-02 | Không đủ tiền mua tướng đã chọn | Nút tướng xám + rung nhẹ; không trừ tiền, không đặt |
| FM-03 | Bán tướng ngay khi wave đang chạy | Cho phép; tướng biến mất ngay; hoàn 60% |
| FM-04 | Nhiều cổ động viên lọt cùng lúc, máu về 0 giữa wave | Thua ngay lập tức; freeze sân; các cổ động viên còn lại dừng |
| FM-05 | App bị kill / gọi điện giữa trận | Trận **không** lưu — vào lại là chơi lại từ đầu (MVP chấp nhận; ghi vào OPEN-QUESTIONS) |
| FM-06 | Tướng nâng cấp đúng lúc mục tiêu chết | Kỹ năng reset cooldown, không mất tiền, không crash |
| FM-07 | 2 tướng cùng bắn 1 cổ động viên đã chết | Damage thừa bỏ đi; tiền thưởng trả **1 lần duy nhất** cho tướng bắn phát cuối |
| FM-08 | Cổ động viên bị slow tới 0 tốc độ | Cấm — slow cap ở 70%; luôn phải còn di chuyển |
| FM-09 | `O Capitão` lọt lưới khi máu cầu môn ≤ 5 | Thua. Boss trừ 5 — không có luật "trừ tối đa còn 1". |
| FM-10 | Boss chạm vạch khi `Cản Phá` đang cooldown | Boss lọt, trừ 5 máu. **Không** có cơ chế cứu vớt nào khác. |
| FM-11 | Nâng cấp Lv1→Lv2 đúng lúc kỹ năng #1 đang chạy | Kỹ năng #1 **chạy tiếp không ngắt**; kỹ năng #2 bật với cooldown đầy |
| FM-12 | `Cú Vô-lê` đẩy quân lùi qua khỏi vạch spawn | Clamp ở vạch spawn. Không cho toạ độ âm trên spline. |
| FM-13 | `Cú Vô-lê` đẩy lùi `O Capitão` | Cho phép — boss **không** kháng đẩy lùi. Chỉ kháng chậm 75%. |
| FM-14 | Nhiều nguồn slow chồng lên `O Capitão` | Lấy max, cap 70%, **rồi** nhân kháng **75%** → tối đa **17.5%** |
| FM-15 | Boss chết đúng lúc đang bị đẩy lùi | Anim ngã chạy tại vị trí hiện tại; thưởng trả 1 lần |
| FM-16 | Quân **đã có thẻ vàng** đi ra khỏi tầm Trọng tài | Chậm 50% **vẫn giữ** — thẻ là vĩnh viễn, aura mới là cục bộ |
| FM-17 | Quân có thẻ vàng + đang trong tầm aura (25%) | Lấy **max**, không cộng → vẫn 50%. Aura không bao giờ làm thẻ mạnh thêm. |
| FM-18 | Quân có thẻ đỏ (70%) + D10S làm chậm (70%) | Lấy max → 70%. Kịch cap, không vượt. |
| FM-19 | Nhiều Trọng tài cùng rút thẻ 1 con trong cùng frame | Chỉ **1 thẻ** được ghi. Vàng → đỏ cần **2 lần rút ở 2 thời điểm khác nhau**. |
| FM-20 | Trọng tài bị bán khi quân đang mang thẻ | Thẻ **vẫn giữ** — quân đã bị phạt rồi. Chỉ aura biến mất. |
| FM-21 | Bán Trọng tài rồi mua lại để reset cooldown thẻ | Cho phép — nhưng bán hoàn 60%, mua lại full giá. Tự phạt. |

💡 *Mục 16–17 là toàn bộ linh hồn của Trọng tài: **thẻ theo người, aura theo chỗ.** Lẫn hai cái này là biến tướng khắc chế tank thành một cái đèn pin. Mục 20 cũng vậy — thẻ vàng đã rút thì không rút lại được, kể cả khi trọng tài rời sân. Đúng luật bóng đá, và tình cờ cũng đúng thiết kế.*

💡 *Mục 7 là bug kinh điển của TD: hai tower cùng bắn phát cuối → cộng tiền hai lần → kinh tế vỡ sau 10 wave. Phải chốt "ai gây phát chết cuối" ngay ở tầng design, không để dev tự đoán.*

💡 *Mục 14 có thứ tự phép tính quan trọng: **cap trước, kháng sau**. Làm ngược lại (kháng trước rồi cap 70%) thì boss vẫn bị chậm 70% và kháng chậm thành vô nghĩa. Một dòng code, hai hành vi hoàn toàn khác — và nó quyết định boss có phải bao cát hay không.*

## 9. Thuật ngữ

| Từ | Nghĩa |
|----|-------|
| **Peso ⚽** | Đơn vị tiền trong trận. Reset mỗi trận, không mang qua trận sau. |
| **Cầu môn** | "Thành" của người chơi. Máu = số bàn thua chịu được. |
| **Tướng / đơn vị** | Tower. **6 loại**: 5 cầu thủ + 1 trọng tài. Xem `02`. |
| **`El Árbitro`** | Trọng tài. 0 sát thương, chậm vĩnh viễn bằng thẻ. Chiếm 1 ô sân. Xem `02` §4.6. |
| **Thẻ vàng / thẻ đỏ** | Chậm **50% / 70% VĨNH VIỄN** cho 1 con — theo nó tới hết đường. Khác aura (cục bộ). |
| **Cổ động viên** | Enemy thường. 3 loại, xem `03` §1. |
| **`O Capitão`** | Boss. Thủ lĩnh hội cổ động viên Bồ. Xuất hiện W10 và W20. Xem `03` §2. |
| **Lọt lưới (leak)** | Cổ động viên đi hết đường vào cầu môn → trừ máu. |
| **Clean sheet** | Thắng mà không thủng lưới lần nào → 3 sao. |
| **Ô sân** | 11 vị trí đặt tướng thường. |
| **Ô thủ môn** | 1 vị trí đặc biệt trước cầu môn, chỉ Dibu vào được. |
| **Kỹ năng cộng dồn** | Mỗi cấp mở 1 kỹ năng mới và **giữ** kỹ năng cũ. Lv3 chạy 3 cái cùng lúc. Kỹ năng cũ **không** tự mạnh lên. Xem `02` §3.2. |
| **Hoả lực tập trung** | Boss đi một mình → không chia sát thương với ai. Mô hình cân bằng khác hẳn đám đông. Xem `04` §3.3. |
