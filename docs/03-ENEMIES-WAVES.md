# 03 — Cổ động viên & Wave

> 3 loại cổ động viên thường + **boss `O Capitão`** ở W5/W10/W15/W20, số lượng 1/1/2/2.
> ⚠️ Bảng wave ở đây **được dẫn xuất từ mô hình cân bằng** ở [`04-ECONOMY-BALANCE.md`](04-ECONOMY-BALANCE.md) §4. Đổi bất kỳ số nào ở đây → phải chạy lại kiểm chứng ở doc 04.

---

## 1. Ba loại cổ động viên thường

<!-- GEN:enemy_table -->
| Loại | Máu gốc | Tốc độ | Thưởng gốc | Trừ máu khi lọt | Vai trò |
|------|---------|--------|-----------|-----------------|---------|
| **Adepto (fan thường)** | 100 | 1.0 | 5 | 1 | Số đông. Dạy người chơi luật. |
| **Tifoso Kèn (fan cuồng cầm kèn)** | 220 | 1.4 | 11 | 1 | Nhanh — trừng phạt tướng tầm ngắn / tốc đánh chậm |
| **Tambor Giáp (fan vác trống)** | 550 | 0.6 | 24 | 2 | Tank. Trừng phạt đội hình thiếu sát thương dồn |
<!-- /GEN:enemy_table -->

**Ghi chú thiết kế:**

- **Adepto** đi đúng tốc chuẩn (1.0). Mọi thứ khác so tương đối với nó.
- **Tifoso** nhanh gấp 1.4 lần → thời gian nằm trong tầm tướng chỉ còn **71%**. Đây là lý do Batigol (2.5 giây/phát) vật vã trước nó.
- **Tambor** chậm 0.6 → nằm trong tầm **167%** thời gian bình thường, nhưng 550 máu và lọt lưới **trừ 2 máu**. Một Tambor lọt = mất 10% máu cầu môn.

---

## 2. Boss — `O Capitão`

> Thủ lĩnh hội cổ động viên Bồ Đào Nha. Có **6 boss cả trận**: W5/W10 một con, W15/W20 hai con.

| Thuộc tính | Giá trị |
|-----------|---------|
| Máu mỗi boss | **W5: 1 000** · **W10: 2 500** · **W15: 2 800** · **W20: 5 000** (ghi thẳng, không nhân `hpScaling`) |
| Tốc độ | **0.4** — chậm nhất game |
| Thưởng mỗi boss | W5: **40** · W10: **80** · W15: **110** · W20: **180** |
| **Trừ máu khi lọt** | **5** — một phần tư cầu môn |
| **Kháng chậm** | **75%** — mọi hiệu ứng chậm chỉ còn 1/4 (cap thực tế: **17.5%**) |
| Truất quyền thi đấu | **Không tồn tại trong game** — thẻ đỏ chỉ làm chậm, không xoá sổ ai (`02` §4.6) |
| Kỹ năng | **Không có.** Chỉ là một khối máu đi chậm. |
| Thời điểm spawn | **Cuối cùng**, sau khi toàn bộ quân thường của wave đã ra |

### Vì sao boss thiết kế thế này

| Quyết định | Lý do |
|-----------|-------|
| Máu ghi thẳng, không nhân hệ số act | Boss là **set-piece**, không phải quân thường được scale. Nhét vào công thức nhân sẽ ra một con số không ai chọn, chỉ là hệ quả của phép nhân. Máu boss được dẫn xuất ngược từ mô hình hoả lực tập trung ở `04` §3.3. |
| Spawn cuối cùng | Boss đi một mình → ăn **toàn bộ hoả lực tập trung**. Đây là kiểu tính toán hoàn toàn khác với đám đông (`04` §3.5). |
| **Kháng chậm 75%** (nâng từ 50%) | Xem hộp bên dưới — 50% không sống nổi sau khi có Trọng tài. |
| Trừ 5 máu | Đủ đau để không ai thả trôi, chưa đủ để một lần lọt là thua (còn 15/20). |
| Không kỹ năng | Boss có kỹ năng = thêm một trục cân bằng. Mô hình ở doc 04 vốn đã chồng ba giả định rồi. |

### ⚠️ Vì sao kháng chậm phải nâng 50% → 75%

Con số 50% được đặt khi game chưa có Trọng tài. Sau khi thêm `El Árbitro`, nó **sụp ngay**:

Boss là **mồi ngon nhất** của cơ chế thẻ — nó chậm nhất game (0.4), nên ở trong tầm Trọng tài lâu nhất, nên chắc chắn ăn cả thẻ vàng lẫn thẻ đỏ.

| Kháng chậm | Thẻ Đỏ (70%) tác động thật | Boss ở lại sân lâu hơn | Headroom boss W20 |
|-----------|---------------------------|------------------------|-------------------|
| **50%** (bản cũ) | 35% | **+54%** | **1.68×** — boss thành bao cát |
| **75%** (bản này) | **17.5%** | +21% | **1.32×** — Trọng tài giúp, không giải hộ |

Ở 50%, một tướng duy nhất xoá sổ bài kiểm tra căng nhất game (boss W20 vốn ở 1.18×). Ở 75%, người chơi **không** có Trọng tài vẫn qua được ở 1.18×, người **có** thì dễ hơn chút — 1.32×. Cả hai lối chơi đều sống.

💡 *Đây là kiểu hỏng mà chỉ lộ ra khi tính lại chứ không lộ ra khi đọc lại. "Boss kháng chậm 50%" đọc vẫn hợp lý ở mọi vòng review — chỉ có phép nhân mới nói cho biết rằng một tướng mới vừa vô hiệu hoá nó.*

### Bản quyền — ⚠️ ĐỌC KỸ

`O Capitão` (tiếng Bồ: *Đội trưởng*) là từ **generic, không ai sở hữu**. Nhận diện đến từ **ngữ cảnh** — thủ lĩnh phe Bồ Đào Nha, xuất hiện ở wave trùm — chứ không từ tên.

| ❌ TUYỆT ĐỐI KHÔNG | Vì sao |
|-------------------|--------|
| **"CR7"** | **Nhãn hiệu đã đăng ký** — Ronaldo kinh doanh thật dưới thương hiệu này (quần áo, khách sạn, nước hoa). Được bảo hộ mạnh hơn cả tên riêng. |
| **Số áo 7** | Đã bị cấm bởi `06` §5 (không số áo thật) |
| Tên thật, mặt thật, dáng ăn mừng đặc trưng | `06` §5 |

💡 *Người ta hay tưởng parody thì miễn nhiễm. Parody bảo vệ được **bình luận**, không bảo vệ được việc **dùng nhãn hiệu để bán sản phẩm**. Một con boss trong game trả phí là dùng để bán. "O Capitão" thì không ai kiện được, mà người chơi vẫn hiểu ngay đó là ai.*

---

## 3. Công thức scaling

Độ khó tăng bằng đường cong trơn cộng bốn milestone Hard; **máu và thưởng dùng hai hệ số KHÁC NHAU**:

```
Máu    = round(máu_gốc × 0.67 × 1.09^(wave-1) × milestone_gần_nhất)
Thưởng = round(thưởng_gốc × bountyMultiplier)
Tốc độ, trừ máu = KHÔNG đổi
```

| Mốc | W1–4 | W5–9 | W10–14 | W15–19 | W20 |
|-----|------|------|--------|--------|-----|
| Milestone HP | ×1.0 | **×1.2** | **×1.4** | **×1.7** | **×2.0** |

✅ **`hpMultiplier` không còn là hệ số theo act.** Công thức hiện tại là `0.67 × 1.09^(wave-1) × milestone_gần_nhất`; milestone không cộng dồn. Bốn bước tăng W5/W10/W15/W20 khắc phục việc tướng Lv3 AoE quét sạch phần cuối trận.

🔴 **Vì sao bỏ hệ số theo act:** bậc thang tạo **vách đứng** ngay sau ranh giới act. Đo được: W8 headroom **1.00** và W15 **0.97** (người chơi tối ưu *thua*), trong khi W14 cuối act **1.81** (quá dễ) — biên độ **1.81× trong cùng một act**. Một hệ số cho 7 wave không thể vừa cứu đầu act vừa ghì cuối act. Act giờ chỉ điều khiển **thưởng** và ý đồ kể chuyện.

### ⚠️ Sửa lỗi thiết kế của bản trước

Bản đầu dùng **một hệ số cho cả máu lẫn thưởng**, và tôi còn khen đó là thiết kế đẹp. **Nó sai.**

Máu và thưởng scale cùng nhịp nghĩa là: quân khoẻ gấp đôi thì bạn cũng giàu gấp đôi, mua được gấp đôi hoả lực. **Độ khó thực tế đứng yên.** Cả trận là một đường phẳng đội lốt đường dốc.

Tách đôi thì máu vượt lên trước thưởng → sức mua tụt lại sau độ khó → người chơi thật sự phải chơi giỏi hơn, không chỉ mua nhiều hơn. Giờ máu chạy theo `0.67 × 1.09^(w-1) × milestone` (W20 ≈ **×6.89**) còn thưởng vẫn bậc thang theo act (**1.0 / 1.6 / 2.2**) — sức mua không tăng kịp HP cuối trận.

💡 *Bài học: "một hệ số, hai cột" nghe gọn gàng nên nghe có vẻ đúng. Sự gọn gàng không phải bằng chứng. Cái làm lộ ra lỗi là phép tính ở doc 04 §6, không phải việc đọc lại doc.*

**Chỉ số sau khi nhân:**

<!-- GEN:scaled_stats -->
| Loại | Act 1 máu | Act 2 máu | Act 3 máu | Act 1 thưởng | Act 2 thưởng | Act 3 thưởng |
|------|-----------|-----------|-----------|-----------|-----------|-----------|
| Adepto | 67 | 147 | 381 | 5 | 8 | 11 |
| Tifoso Kèn | 147 | 323 | 837 | 11 | 18 | 24 |
| Tambor Giáp | 369 | 808 | 2 093 | 24 | 38 | 53 |
| `O Capitão` | W5 **1 000** | W10 **2 500** | W15 **2 800** / W20 **5 000** | W5 **40** | W10 **80** | W15 **110** / W20 **180** |
<!-- /GEN:scaled_stats -->

---

## 4. Bảng 20 wave

**Spawn interval: 0.7s** · **Nghỉ giữa wave: 8s** (skip được, thưởng = giây còn lại × 3)

Cột `Ví sau wave` = tổng tiền đã cầm được từ đầu trận (khởi đầu 550), **chưa trừ chi tiêu**.

<!-- GEN:wave_tables -->
### Act 1 — W1–W7 (máu ×0.67→×1.35 · thưởng ×1)

| W | Adepto | Tifoso | Tambor | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví sau wave |
|---|--------|--------|--------|------|------|----------|-----------|-------|-----------|-------------|
| 1 | 8 | — | — | — | 8 | 536 | 40 | 20 | 60 | 760 |
| 2 | 13 | — | — | — | 13 | 949 | 65 | 25 | 90 | 850 |
| 3 | 10 | 4 | — | — | 14 | 1 500 | 94 | 30 | 124 | 974 |
| 4 | 10 | 8 | — | — | 18 | 2 398 | 138 | 35 | 173 | 1 147 |
| **5** | 8 | 6 | 1 | **1** | 16 | **4 028** | 130 + **40** | 40 | 210 | 1 357 |
| 6 | 10 | 8 | 1 | — | 19 | 4 096 | 162 | 45 | 207 | 1 564 |
| 7 | 8 | 8 | 3 | — | 19 | 5 682 | 200 | 50 | 250 | 1 814 |

### Act 2 — W8–W14 (máu ×1.47→×2.88 · thưởng ×1.6)

| W | Adepto | Tifoso | Tambor | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví sau wave |
|---|--------|--------|--------|------|------|----------|-----------|-------|-----------|-------------|
| 8 | 8 | 6 | 1 | — | 15 | 3 922 | 210 | 55 | 265 | 2 079 |
| 9 | 10 | 8 | 1 | — | 19 | 5 297 | 262 | 60 | 322 | 2 401 |
| **10** | 8 | 8 | 3 | **1** | 20 | **11 076** | 322 + **80** | 65 | 467 | 2 868 |
| 11 | 10 | 9 | 3 | — | 22 | 10 284 | 356 | 70 | 426 | 3 294 |
| 12 | 10 | 10 | 4 | — | 24 | 13 064 | 412 | 75 | 487 | 3 781 |
| 13 | 10 | 13 | 4 | — | 27 | 15 984 | 466 | 80 | 546 | 4 327 |
| 14 | 13 | 13 | 5 | — | 31 | 19 883 | 528 | 85 | 613 | 4 940 |

### Act 3 — W15–W20 (máu ×3.81→×6.89 · thưởng ×2.2)

| W | Adepto | Tifoso | Tambor | Boss | Quân | Tổng máu | Thưởng hạ | Clear | Tiền wave | Ví sau wave |
|---|--------|--------|--------|------|------|----------|-----------|-------|-----------|-------------|
| **15** | 10 | 10 | 4 | **2** | 26 | **26 152** | 562 + **220** | 90 | 872 | 5 812 |
| 16 | 10 | 13 | 4 | — | 27 | 25 147 | 634 | 95 | 729 | 6 541 |
| 17 | 13 | 13 | 5 | — | 31 | 31 246 | 720 | 100 | 820 | 7 361 |
| 18 | 13 | 15 | 6 | — | 34 | 38 935 | 821 | 105 | 926 | 8 287 |
| 19 | 15 | 16 | 8 | — | 39 | 50 607 | 973 | 110 | 1 083 | 9 370 |
| **20** | 18 | 20 | 9 | **2** | 49 | **86 823** | 1 155 + **360** | **150** | 1 665 | **11 035** |
<!-- /GEN:wave_tables -->

**Nhịp dạy chơi:** W1–2 chỉ Adepto (học đặt tướng). W3 giới thiệu Tifoso. W5 giới thiệu Tambor. W7 là bài kiểm tra — cả 3 loại, và là wave sát nút nhất Act 1.

**Nhịp:** W8 cố tình **ít quân hơn W7** — nhưng máu mỗi con nhảy ×1.8, nên cảm giác là "sao tự nhiên bắn mãi không chết". Đó là tín hiệu dạy người chơi rằng đã tới lúc **nâng cấp thay vì mua thêm**.

**W10 — lần đầu gặp `O Capitão`.** 5 436 máu quân thường đi trước, rồi 5 500 máu boss đi một mình phía sau. Người chơi có ~2 863 Peso tại thời điểm này — đủ để xử lý nếu đã đầu tư sát thương dồn, không đủ nếu chỉ spam tướng rẻ. Đội hình lệch hẳn về D10S **chết đứng ở đây**: D10S mất số nhân ×3 trước mục tiêu đơn (`04` §3.3).

W13 là wave "Tifoso rush" — 10 Tifoso, tốc 1.4. Đội hình toàn Batigol (tầm 1.0, 2.5s/phát) trả giá ở đây.

✅ **Cả hai cột giờ đều dẫn xuất, không còn tạm tính.** Máu từ `hpScaling` (`balance_sim.py --solve`, 20/20 wave trong dải). Tiền từ `bountyMultiplier` Act 3 = **2.2** — hạ từ 2.5 ở vòng 5b vì tiền cả trận (11 900) đã **vượt** trần chi tiêu (11 696) → người chơi mua được hết, cuối game hết lựa chọn. Giờ biên khan hiếm **+3.6%**.

**W20 — finale.** 33 326 máu quân thường + 22 000 máu `O Capitão`. Nếu lọt hết = **49 sát thương** lên cầu môn chỉ có 20 máu. Không có chuyện "chịu đòn qua wave cuối".

Nếu `Cản Phá` của Dibu còn sẵn đúng lúc boss chạm vạch → **cứu 5 máu bằng một kỹ năng miễn phí**. Đó là phần thưởng cho việc đầu tư Dibu, và là lý do Dibu tồn tại.

### Tổng kết

<!-- GEN:wave_summary -->
| | Act 1 | Act 2 | Act 3 | **Cả trận** |
|---|-------|-------|-------|-------------|
| Số quân | 107 | 158 | 206 | **471** |
| Tổng máu | 19 189 | 79 510 | 258 910 | **357 609** |
| Tiền sinh ra | 1 114 | 3 126 | 6 095 | **10 335** |

Cộng 700 khởi đầu → **tổng tiền cả đời một trận = 11 035 Peso**.
<!-- /GEN:wave_summary -->

---

## 5. Thời lượng trận

```
Thời gian spawn   = 366 quân × 0.7s              = 256s
Đuôi wave (~12s mỗi wave, dọn nốt quân còn lại)  = 240s
Nghỉ giữa wave    = 19 × 8s                      = 152s
                                          Tổng  ≈ 648s ≈ 10.8 phút
```

Với nút **×2**: **≈ 6.7 phút**. Kỳ vọng: lần đầu chơi 1× (~11 phút), chơi lại luôn bật 2× (~7 phút).

---

## 6. Hành vi cổ động viên

| Thuộc tính | Quân thường | `O Capitão` |
|-----------|-------------|-------------|
| Pathing | Bám 1 đường cố định. Không tìm đường, không né tướng, không tấn công tướng. | Như quân thường |
| Va chạm với nhau | Không. Chồng lên nhau chỉ là vấn đề hình ảnh. | Như quân thường |
| Khi tới cầu môn | Trừ máu, tự huỷ, **không** rơi tiền | Trừ **5** máu, tự huỷ, **không** rơi tiền |
| Khi bị hạ | Rơi tiền ngay. Anim ngã 0.4s rồi huỷ. | Rơi tiền ngay. Anim ngã **1.2s** + rung màn hình. |
| Bị Dibu cản | Tự huỷ, **không** trừ máu, **không** rơi tiền | Như quân thường — **cản được**, và đó là điểm mấu chốt |
| Kháng chậm | **Không** — cap 70% | **75%** → cap thực tế **17.5%** |
| Ăn thẻ Trọng tài | Có — vàng 50%, đỏ 70%, vĩnh viễn | Có, nhưng chia 4 → vàng **12.5%**, đỏ **17.5%**. **Không bị truất quyền.** |
| Hồi máu / giáp / tàng hình | **Không có ở MVP** | **Không có ở MVP** |

💡 *Cổ động viên không đánh trả tướng — đó là quyết định có chủ đích, không phải thiếu sót. Tướng bất tử nghĩa là người chơi chỉ phải lo đúng một bài toán: đủ sát thương trước khi bọn nó tới cầu môn hay không. Thêm cơ chế tướng chết là thêm một trục cân bằng nữa, và MVP chưa cần trục đó.*
