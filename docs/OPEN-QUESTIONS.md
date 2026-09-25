# Câu hỏi mở

> **Đọc file này trước khi bàn tiếp.** Đây là những gì tôi tự quyết vì cần một con số để tính, hoặc những gì thật sự cần bạn chốt.
> Xếp theo mức chặn. 🔴 chặn M0 · 🟡 chặn M1 · 🟢 chặn M2+
> Cập nhật: 2026-07-16 (vòng 3 — sau khi thêm `El Árbitro`)

---

## ✅ Q0. Mô hình cân bằng — GIẢI XONG ở vòng 5b bằng `tools/balance_sim.py`

**Đây từng là câu chặn lớn nhất.** Nó không phải câu hỏi cho bạn — nó là việc phải làm, và đã làm.

`04` §4 giờ **dẫn xuất đầy đủ W1–W20**: `python3 tools/balance_sim.py` → **20/20 wave trong dải cứng [1.05, 1.65]**, headroom 1.06–1.63. Chạy `--solve` để dò lại sau mọi thay đổi.

| # | Lý do mô hình từng bí | Trạng thái |
|---|----------------------|-----------|
| 1 | `τ` phụ thuộc **số quân** trong wave | ✅ tính lại từng wave |
| 2 | Reference Build đổi theo **ngân sách từng wave** | ✅ mô phỏng dòng tiền + **bán & xây lại** |
| 3 | 🆕 Thu tầm đổi mọi chord | ✅ **hết là giả định** — đo 4000 mẫu spline |
| 4 | Ba hệ số bịa nhân nhau `η × σ × τ` | 🔴 **KHÔNG giải được** — vẫn ±73%, chỉ M1 telemetry đo được |

**Điểm 4 là lý do headroom vẫn chỉ là THỨ TỰ, không phải giá trị.** "1.42" không nghĩa là dư 42%; nó nghĩa là wave này thoải mái hơn wave 1.06. Bảng tính làm *thứ tự* đáng tin, không làm *con số* đúng. Auto-play M1 vẫn **không phải việc có thể cắt**.

### Bảng tính bắt được 5 lỗi mà 5 vòng review không bắt

| # | Lỗi | Bằng chứng |
|---|-----|-----------|
| 1 | **La Pulga + Batigol là nội dung chết** | qua 20 wave người chơi tối ưu **không mua lần nào**. La Pulga — nổi tiếng nhất — là món tệ nhất game (0.54 vs 1.28 của El Fideo). Ngược yêu cầu gốc. |
| 2 | **chord siêu tuyến tính** | tầm ×2.4 → chord **×5.3**. Ngưỡng nổ ở R≈2.0. Tướng tầm xa nhất luôn thắng bất kể DPS. |
| 3 | **hệ số máu theo act tạo vách đứng** | W8 **1.00** và W15 **0.97** (thua) vs W14 **1.81** (quá dễ) — biên độ 1.81× trong cùng act |
| 4 | **tiền cả trận vượt trần chi tiêu** | 11 900 > 11 696 → mua được hết, cuối game hết lựa chọn |
| 5 | **boss cũ không thể thắng** | máu 5500/22000 (dẫn xuất khi tầm còn 3.0–7.0) → headroom 0.49 / 0.28 |

---

## ✅ Đã chốt ở vòng 2 — không mở lại

| # | Câu hỏi | Chốt | Ảnh hưởng |
|---|---------|------|-----------|
| 1 | Ăn theo World Cup 2026? | **Không** — làm tử tế, không deadline thị trường | `07` giữ nguyên; store listing bỏ mọi tham chiếu giải đấu |
| 2 | Dibu không thuộc top-5 nổi tiếng | **Giữ Dibu** — thủ môn hợp theme, chấp nhận lệch tiêu chí | `02` §1 ghi rõ đây là lệch chuẩn có chủ đích |
| 3 | Phe địch vô danh, không có Ronaldo | **Thêm boss `O Capitão`** ở W5/W10/W15/W20, số lượng 1/1/2/2 | Đảo ràng buộc "chỉ 3 loại"; `03` §2, `04` §3.3 |
| 4 | "Kỹ năng khi lên cấp" nghĩa là gì | **Mỗi cấp mở 1 kỹ năng MỚI, cộng dồn** → 18 kỹ năng (sau khi thêm Trọng tài) | `02` viết lại; `05` schema `abilities[]` |
| 5 | Tên boss (CR7 là nhãn hiệu đăng ký) | **`O Capitão`** — generic, không ai sở hữu | `03` §2, `06` §5 |
| 6 | Thêm tướng Trọng tài | **`El Árbitro`** — tướng thứ 6, 0 DPS, chậm vĩnh viễn bằng thẻ | `02` §4.6, `04` §3.4b |
| 7 | Kể chuyện Trọng tài | **Thiên vị** — trò đùa "bị mua". Tôi có nêu rằng nó hàm ý "Argentina thắng nhờ gian lận"; user chốt vẫn làm. | `06` §5 |
| 8 | Thẻ Đỏ mạnh tới đâu | **Không xoá sổ** — chỉ chậm 70% vĩnh viễn | `02` §4.6 |
| 9 | Ô đặt Trọng tài | **Chiếm 1 trong 11 ô sân** — 0 DPS mà tốn ô là cái giá thật | `02` §2 |

---

## 🔴 Chặn M0 — cần chốt trước khi viết dòng code đầu tiên

### ✅ Q1. Layout map — ĐÓNG ở vòng 5

Toạ độ thật ở `config/path.json`, đo bằng `tools/path_check.py` (4000 mẫu spline, `--check` cho CI):

| Ràng buộc | Kết quả |
|-----------|---------|
| Độ dài spline vs `lengthUnits` 40 | **40.65** = lệch 1.6% ✅ |
| Hộp bao trong viewport | x[-4.6, 4.3] y[-7.8, 8.0] ✅ |
| Ô chết (không tướng Lv1 nào với tới) | **0** ✅ |
| Thang tầm có ý nghĩa | 4 bậc thật ✅ |

14 waypoints, 4 lượt quét ngang. 11 ô sân rải theo **quãng đường** (f=0.04→0.91) chứ không theo toạ độ — để không ô nào phủ trùng phần đường ô khác đã phủ. Mỗi ô ở một tầng khoảng cách cố ý: Batigol dùng được 5/11 ô · D10S 6/11 · El Árbitro 6/11 · La Pulga 8/11 · El Fideo 11/11.

**Câu hỏi này đã giấu một câu lớn hơn.** Nó hỏi "ô ở đâu"; câu trả lời là **tầm sai**. Hành lang `2 × R × 40.65` ở tầm nhỏ nhất cũ (3.0) = 118% diện tích màn → không tồn tại ô nào ngoài tầm bất kỳ tướng nào, thang tầm 3.0→7.0 là trang trí. User chốt thu tầm hệ số **0.36** → `chord/2R` từ 1.56 về **0.96** (giả định `04` §3.2 thành đúng), hành lang về 41% màn, `2R/đường` về 8.0% (khớp chuẩn thể loại ~7.5%).

🔴 **Cái giá:** sát thương mô hình giảm ~2.8× → **Q0 to ra**, cả W1–W20 phải dẫn xuất lại chứ không chỉ W12–W20.

**Vòng 5b đặt lại toàn bộ ô** sau khi nén tầm lần hai (≤2.0). Ô cao nhất giờ là f01 ở y=**7.1**, radial menu xoè lên tới y=9.49 < mép 9.6 ✅ — `path_check.py` thi hành luật này. Đoạn đường dài nhất không ô nào canh: **11%** (luật: ≤15%).

### Q2. Tên game

"La Muralla" là tên tôi tạm đặt (tiếng Tây Ban Nha: "Bức tường"). Chưa kiểm tra trùng trên store.

**Lựa chọn:** giữ · đổi · để sau M2.

### Q3. Trận đang chơi dở có lưu không?

`01` §8 **FM-05** hiện ghi **không lưu** — gọi điện giữa trận là mất trận. Với trận ~11 phút trên mobile, đây là quyết định khá tàn nhẫn.

**Lựa chọn:**
- **(A)** Giữ không lưu — đơn giản nhất *(đang chọn)*
- **(B)** Lưu snapshot giữa wave — +2 ngày, phải serialize toàn bộ state trận
- **(C)** Chỉ pause khi app vào nền, mất khi bị kill — dung hoà, +0.5 ngày

**Khuyến nghị: (C).** Rẻ, và giải quyết 90% trường hợp thật.

---

## 🟡 Chặn M1 — cần chốt trước khi cân bằng

### Q4. ⚠️ Mô hình cân bằng giờ chồng HAI giả định bịa ra

Đây là **rủi ro lớn nhất của project**, và nó vừa xấu đi đáng kể ở vòng 2.

| Giả định | Là gì | Tôi lấy ở đâu ra |
|----------|-------|------------------|
| `η = 0.75` | Hao phí do overkill | Nghe hợp lý. Không đo. |
| `σ = 1.10 / 1.20 / 1.35` | Bù cho 18 kỹ năng mà mô hình không biết | Nghe hợp lý. Không đo. |
| **`τ = 1.07 / 1.20 / 1.26`** | **Bù cho Trọng tài làm chậm vĩnh viễn** | **Nghe hợp lý. Không đo.** |

**Sai số nhân nhau, không cộng.** Ba hệ số, mỗi cái lệch 20% → tổng lệch **73%**. Xem Q0.

**Cần:** không cần bạn quyết gì. Nhưng cần bạn **biết** rằng bảng số ở doc 04 trông chính xác hơn thực tế rất nhiều — đó là lý do W12+ giờ để trống thay vì điền số đẹp. Bảng tính (M0) và auto-play (M1) **không phải việc có thể cắt** nếu lịch căng.

### Q5. D10S chạm trung bình mấy mục tiêu?

Tôi giả định **3** (đám đông) và **1** (boss). Con số này quyết định D10S là tướng mạnh nhất game hay tướng vô dụng nhất.

Với bán kính lan 1.7 (sau `Cú Chạm Thiên Tài`) và quân cách nhau 0.77 đơn vị, 3 nghe hợp lý. Nhưng quân đi thành **hàng dọc** trên đường, không đứng thành cụm tròn — số thật có thể gần **2**.

**Cần:** đo ở M1. Nếu ra 2 → D10S cần bán kính lan lớn hơn hoặc giá rẻ hơn.

### ✅ Q6. Margin "tiền luôn khan hiếm" — ĐÃ VỠ THẬT, đã chữa

Câu này dự đoán đúng. Vòng 5b, sau khi nén tầm + cân lại giá tướng, `balance_sim.py` đo được tiền cả trận **11 900** vs trần chi **11 696** → **biên ÂM 1.7%**: người chơi tối ưu mua được **hết**, Act 3 mất sạch sức căng kinh tế.

Chữa đúng theo khuyến nghị đã ghi sẵn ở đây: **`bountyMultiplier` Act 3 2.5 → 2.2**, không động giá tướng. Kết quả: tiền cả trận **11 276**, biên **+3.6%**, và sim xác nhận dư tiền mỗi wave chỉ **10–230 Peso**.

💡 *Đáng ghi lại: câu hỏi mở này viết ở vòng 3 đã nêu đúng triệu chứng, đúng nguyên nhân, và đúng cách chữa — nhưng phải tới khi có công cụ đo mới biết nó thật sự vỡ. Nghi ngờ có cơ sở ≠ bằng chứng.*

### Q7. Wave 7 headroom 1.21× — còn là bài kiểm tra không?

Sau khi thêm `σ`, W7 nới từ 1.08× lên **1.21×** — không còn là wave sát nút nhất Act 1 nữa. Giờ dòng căng nhất toàn game là **boss W20 ở 1.18×**.

**Câu hỏi:** Act 1 giờ có bài kiểm tra nào không, hay người chơi trôi tuột tới W10 rồi mới gặp tường?

**Lựa chọn:**
- **(A)** Để yên — boss W10 là bài kiểm tra đầu tiên *(đang chọn)*
- **(B)** Siết W7 lại về ~1.10× (thêm 1 Tifoso) — giữ nhịp dạy chơi như thiết kế ban đầu

**Khuyến nghị: (B)** — nhưng chỉ sau khi auto-play xác nhận `σ` thật là bao nhiêu. Siết bây giờ là siết dựa trên một con số bịa.

### Q8. Thứ tự mở khoá tướng?

Hiện tại: **cả 6 đơn vị có sẵn từ giây đầu**. Với 18 kỹ năng, người chơi mới còn ngợp hơn bản trước.

**Lựa chọn:**
- **(A)** Cả 6 có sẵn ngay *(đang chọn)*
- **(B)** Mở dần: W1 El Fideo + Batigol → W3 D10S → W5 La Pulga → W7 Dibu → **W11 El Árbitro**

**Khuyến nghị: (B), và giờ lý do mạnh hơn hẳn.** Reference Build vốn đã mua đúng thứ tự này. Quan trọng hơn: `04` §3.4b chứng minh **Trọng tài mua sớm là lỗ** (τ 1.07 < ngưỡng hoà vốn 1.103) vì nó nhân sức mạnh — mua khi sân còn trống là nhân với một số nhỏ. Cho người chơi mới tiếp cận Trọng tài ở W1 là mời họ mắc một cái bẫy mà chính bảng số nói là bẫy.

### Q9. ⚠️ Trọng tài có thành "thuế" không?

Một tướng **nhân** sức mạnh cả sân rất dễ trượt từ "lựa chọn hay" thành **"không mua là ngu"**. Lúc đó nó không còn là lựa chọn — nó là thuế, và game mất đi một quyết định thay vì có thêm.

**Đo ở M1** (`04` §8 mục 8): tỉ lệ thắng **có** vs **không có** Árbitro.

| Chênh lệch | Nghĩa là |
|-----------|----------|
| < 5 điểm | Trọng tài quá yếu — không ai mua |
| 5–15 điểm | ✅ Lựa chọn thật |
| **> 15 điểm** | ❌ **Thuế** — phải giảm τ (thu hẹp aura hoặc giãn cooldown thẻ), **không** phải tăng giá |

💡 *Tăng giá không sửa được vấn đề này. Nếu tướng đó cần thiết thì người chơi vẫn mua ở bất kỳ giá nào, chỉ là mua muộn hơn — và giờ ta có một tướng bắt buộc VÀ một khoản thuế tiền. Phải giảm sức mạnh, không phải tăng giá.*

---

## 🟢 Chặn M2+ — bàn sau được

### Q9. Ai làm art?

`06` §4 giờ liệt kê ~**77** asset (tăng từ ~60: +15 icon kỹ năng, +1 bộ boss). Thuê ngoài? Asset store? Tự vẽ? Ảnh hưởng thẳng tới lịch M2.

### Q10. Kiếm tiền?

Đã tích hợp **AdMob rewarded-only**: hồi 5 máu trong giờ nghỉ và continue với 5 máu sau khi thua, mỗi loại một lần/trận. Hiện dùng test ID; release guard buộc nhập App ID/ad unit ID thật, chọn audience `General`/`UnderAgeOfConsent` và tắt test mode trước khi phát hành. Không có banner/interstitial/IAP.

### Q11. Ngôn ngữ trong game?

Tiếng Việt? Tiếng Anh? Cả hai? Ảnh hưởng font (`06` §4 đã cảnh báo về dấu tiếng Việt) và bề rộng UI. **15 tên kỹ năng** giờ cũng cần dịch.

### Q12. 20 wave có đủ không?

Một trận ~11 phút. Thắng lần đầu là hết game trong 11 phút. Không meta-progression, không map khác → không lý do mở lại app.

**Đây là câu hỏi lớn nhất về vòng đời sản phẩm.** Boss có giúp chút ít (2 khoảnh khắc đáng nhớ), nhưng không giải quyết gốc.

---

## Những chỗ tôi tự quyết mà bạn có thể muốn lật lại

| # | Quyết định | Lý do tôi chọn | Ở đâu |
|---|-----------|---------------|-------|
| 1 | Tiền khởi đầu **700** | Giá trị hiện hành ở `config/economy.json`; thay đổi phải chạy lại mô phỏng kinh tế | `04` §5 |
| 2 | Dibu là **thủ môn ô riêng**, không phải tower thường | Thủ môn đứng giữa sân bắn người là phản theme | `02` §4.5 |
| 3 | **Tách `hpMultiplier` khỏi `bountyMultiplier`** | Một hệ số chung làm độ khó **phẳng** — quân khoẻ gấp đôi thì bạn cũng giàu gấp đôi. Vòng 1 tôi làm sai và còn khen nó. | `03` §3 |
| 4 | Máu boss **ghi thẳng**, không nhân hệ số act | Boss là set-piece, dẫn xuất ngược từ mô hình hoả lực tập trung | `03` §2 |
| 5 | Boss **không có kỹ năng** | Thêm một trục cân bằng nữa vào mô hình vốn đã chồng 2 giả định | `03` §2 |
| 6 | Boss **kháng chậm 75%** (nâng từ 50% ở vòng 3) | 50% đủ chặn D10S, nhưng **không** chặn nổi Trọng tài — xem mục 16 | `03` §2 |
| 7 | **Kỹ năng cấp thấp KHÔNG tự mạnh lên** | Nếu vừa cộng dồn vừa scale, Lv3 mạnh 5–6× mà chỉ tốn 3.4× tiền → không ai xây gì khác | `02` §3.2 |
| 8 | Cổ động viên thường **không đánh trả** tướng | Bớt một trục cân bằng; MVP chưa cần | `03` §6 |
| 9 | Nâng cấp chỉ tăng **sát thương + tầm**, không tăng tốc đánh | Hai trục thì DPS scale theo tích, mọi bảng `04` sai | `02` §3.1 |
| 10 | **20 máu** cầu môn | 1 Tambor lọt = 10%, 1 boss lọt = 25% → cảm nhận được ngay | `01` §3 |
| 11 | Sink **Sửa cầu môn** giá tăng ×1.5 | Giá cố định → người chơi cố tình thả quân lọt để tiết kiệm | `04` §1 |
| 12 | **Không bạo lực** | Giữ rating 4+, hợp fantasy "tràn sân" | `06` §1 |
| 13 | Đường chạy **1 tuyến duy nhất** | Nhiều tuyến làm mô hình WDB vỡ, và làm gấp đôi số ô cần | `01` §6 |
| 14 | Boss spawn **cuối cùng**, đi một mình | Đây là giả định nền của mô hình `04` §3.3 — đổi cái này là đổi cả mô hình | `03` §2 |
| 15 | **Thẻ đỏ không xoá sổ ai** (kể cả quân thường) | User chốt. Kỹ năng xoá sổ quân là thứ dễ gãy nhất trong TD. | `02` §4.6 |
| 16 | **Boss kháng chậm 50% → 75%** | Ở 50%, Trọng tài một mình đưa boss W20 từ 1.18× lên 1.68× — xoá sổ bài kiểm tra căng nhất game | `03` §2 |
| 17 | **Thẻ vĩnh viễn, aura cục bộ** | Nếu thẻ cũng chỉ có tác dụng trong tầm thì Trọng tài chỉ là cái đèn pin, không phải tướng khắc chế tank | `01` §8 **FM-16**–17 |
| 18 | **Thẻ giữ nguyên khi bán Trọng tài** | Đúng luật bóng đá (thẻ đã rút không rút lại được), và tình cờ cũng đúng thiết kế | `01` §8 **FM-20** |
| 19 | ~~Act 3 `hpMultiplier` 3.2 → 3.8~~ **ĐÃ XOÁ ở vòng 5b** | Hệ số máu theo act bị bỏ hẳn — nó tạo vách đứng (W8 1.00, W15 0.97 = thua; W14 1.81 = quá dễ). Thay bằng công thức trơn `0.69 × 1.09^(w-1)`, dẫn xuất bằng `balance_sim.py --solve`, 20/20 wave trong dải. | `03` §3 |
| 20 | Bảng màu: **3 sắc đỏ + 2 sắc vàng** | Màu đã hết chỗ sau khi thêm thẻ. Giải bằng **hình dạng + chuyển động**, không phải chỉnh hex. | `06` §2 |
