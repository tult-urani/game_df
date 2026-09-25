# 07 — Roadmap

> Mốc đo bằng **thứ chứng minh được**, không đo bằng "xong khoảng 80%".
> Ước lượng theo **người-ngày**, giả định 1 dev toàn thời gian + art thuê ngoài.

---

## M0 — Xương sống chạy được (≈ 6 ngày)

**Mục tiêu:** một wave chạy được trên iPhone thật. Xấu cũng được. **Và biết Act 3 có chơi được không.**

⚠️ **Việc 2b (bảng tính) chuyển lên từ M1 và là việc chặn.** `04` §4b: mô hình cân bằng đã vượt khả năng tính tay từ W12 — ba giả định bịa (`η × σ × τ`) nhân nhau, `τ` phụ thuộc số quân, build đổi thành phần theo ngân sách. **Nửa ngày viết bảng tính** rẻ hơn nhiều so với ba tuần cân bằng dựa trên số ảo.

| # | Việc | Xong khi |
|---|------|----------|
| 1 | Tạo project Unity 6, URP 2D, cây thư mục theo `05` §2 | Project mở được, `Assets/_Project/` đúng cấu trúc |
| 2 | 4 file config JSON + `ConfigLoader` + **16 luật validate** | Test EditMode: config hỏng → throw |
| 2b | 🔴 **Bảng tính cân bằng** — mô phỏng chi tiêu + WDB × σ × τ từng wave | **Dẫn xuất được W12–W20**, thay chỗ 🔴 ở `04` §4 |
| 3 | Scene `Match` với sân, đường spline, 12 ô placeholder | Nhìn thấy trên màn |
| 4 | `PathFollower` + `WaveSpawner` — 1 loại quân, 1 wave | Adepto spawn và đi tới cầu môn |
| 5 | `GoalHealth` — quân lọt thì trừ máu | Máu 20 → 19 khi quân chạm cầu môn |
| 6 | 1 tướng (El Fideo) tự bắn, quân chết | Quân chết trước khi tới cầu môn |
| 7 | **Build và chạy trên iPhone thật** | App bật lên, wave 1 chạy được |

**Definition of Done:** trên một iPhone thật, wave 1 chạy trọn vẹn — quân spawn, El Fideo bắn, quân chết hoặc lọt lưới trừ máu. Không UI, không tiền, không đồ hoạ đẹp.

⚠️ **Việc số 7 là cửa ải thật, không phải thủ tục.** Toolchain iOS (chứng chỉ, IL2CPP, provisioning) hay tốn nguyên một ngày cho người làm lần đầu. Đâm vào nó ở M0 khi chưa có gì để mất, đừng để tới tuần cuối.

---

## M1 — Trận đấu hoàn chỉnh (≈ 17 ngày)

**Mục tiêu:** chơi được trọn 20 wave, thắng/thua thật, số liệu khớp doc 04.

| # | Việc | Xong khi |
|---|------|----------|
| 1 | 3 loại cổ động viên + **tách `hpMultiplier` / `bountyMultiplier`** | Chỉ số khớp bảng `03` §3 |
| 2 | **Cả 6 đơn vị** (5 cầu thủ + `El Árbitro`), cả 3 cấp | Chỉ số khớp bảng `04` §2 |
| 3 | **18 kỹ năng, cộng dồn** | Mỗi kỹ năng có test PlayMode riêng; Lv3 chạy 3 cái cùng lúc |
| 3b | **Hệ thống thẻ** — `scope: permanent` vs `in_range` | 6 test `Card_*` xanh (`05` §5) |
| 4 | **`O Capitão`** — spawn cuối, kháng chậm **75%**, lọt trừ 5 | Test: `Slow_BossCapsAtSeventeenPointFivePercent` xanh |
| 5 | `EconomyService` — mua/bán/nâng cấp/thưởng/skip/sửa cầu môn | Test: thưởng trả **đúng một lần** |
| 6 | `SlotManager` — 11 ô sân + 1 ô thủ môn | Test: không đặt được Dibu ra ô sân |
| 7 | UI: HUD, radial menu, panel tướng (**hiện 1–3 kỹ năng đang bật**), kết quả | Chơi được không cần console |
| 8 | Đủ 20 wave từ `waves.json` | Chơi từ W1 tới W20 |
| 9 | Thắng / thua / 3 sao | Cả 3 mức sao đều đạt được |
| 10 | Nút ×2 | Trận rút từ ~11 xuống ~7 phút |
| 11 | **Test `Balance_WdbMeetsHeadroomForAll20Waves`** | Chạy lại bảng `04` §4 bằng code, xanh |
| 12 | **Test `Balance_BossFocusFireMeetsHeadroom`** | `04` §3.3 — fail nếu D10S bị tính ×3 trước boss |
| 13 | **Công cụ auto-play, 100 trận với Reference Build** | Báo cáo 6 chỉ số ở `04` §8 |

**Definition of Done:**
- Chơi hết 20 wave trên iPhone thật, thắng được và thua được
- Auto-play 100 trận → tỉ lệ thắng **75–85%** với Reference Build (`04` §3.5)
- **Tổng thu của người chơi tối ưu < 11 696 Peso** (`04` §6) — margin giờ chỉ còn 2.2%, đây là ngưỡng dễ vỡ nhất
- Ngoài khoảng → chỉnh theo đúng thứ tự `04` §8: `σ` → `η` → `hpMultiplier` → `bountyMultiplier` → máu boss. **Chỉ số tướng chỉnh cuối cùng.**

💡 *Việc 13 là thứ biến doc 04 từ giả thuyết thành sự thật. Mô hình giờ chồng **hai** giả định bịa ra (`η` và `σ`) nhân với nhau — sai số nhân, không cộng. Auto-play là cách duy nhất biết cái nào sai và sai bao nhiêu, mà không cần 100 giờ tự chơi tay.*

⚠️ **M1 tăng từ 12 → 20 ngày** qua 3 vòng. Nguồn: 18 kỹ năng thay vì 5 (+3), boss + mô hình hoả lực tập trung (+2), Trọng tài + hệ thống thẻ (+3).

---

## M2 — Trình bày (≈ 10 ngày)

**Mục tiêu:** trông giống một game, không giống prototype.

| # | Việc | Xong khi |
|---|------|----------|
| 1 | Thay toàn bộ art placeholder bằng asset thật (`06` §4) | Không còn hình vuông xám |
| 2 | Toàn bộ game feel (`06` §6) | Rung màn khi lọt lưới, tiền bay lên, nháy trắng khi trúng |
| 3 | SFX + nhạc nền | Có tiếng |
| 4 | Menu chính + màn kết quả | Vào game không qua Editor |
| 5 | `SaveService` — lưu số sao cao nhất | Tắt app mở lại vẫn còn |
| 6 | Pass hiệu năng — ngân sách `05` §7 | 60fps trên iPhone 11, 0 byte cấp phát trong vòng chiến đấu |
| 7 | Kiểm tra guardrail bản quyền (`06` §5) | Rà từng asset theo bảng, ký xác nhận |
| 8 | **Build Android** | Chạy được trên 1 máy Android thật |

**Definition of Done:** đưa máy cho một người chưa từng thấy game, họ chơi hết 1 trận mà không cần hỏi gì.

---

## M3 — Phát hành (≈ 8 ngày)

| # | Việc | Xong khi |
|---|------|----------|
| 1 | Icon, ảnh chụp màn hình, mô tả store | Sẵn sàng nộp |
| 2 | Rà lại mô tả store theo guardrail bản quyền | Không có tên thật, không "World Cup", không tên giải |
| 3 | TestFlight beta ~10 người | Có phản hồi thật |
| 4 | Chỉnh cân bằng theo dữ liệu beta | Sửa `Resources/Config/`, không sửa code |
| 5 | Nộp App Store | Chờ duyệt |
| 6 | Nộp Google Play | Chờ duyệt |

---

## Tổng

| Mốc | Người-ngày | Cộng dồn |
|-----|-----------|----------|
| M0 | **6** | 6 |
| M1 | **20** | 26 |
| M2 | **11** | 37 |
| M3 | 8 | **45** |

**≈ 9 tuần** cho 1 dev toàn thời gian. **Chưa gồm thời gian làm art** (thuê ngoài, chạy song song từ M1).

| | Vòng 1 | Vòng 2 | Vòng 3 | Vì sao |
|---|--------|--------|--------|--------|
| M0 | 5 | 5 | **6** | Bảng tính cân bằng (+1, chuyển lên từ M1) |
| M1 | 12 | 17 | **20** | Vòng 2: 15 kỹ năng (+3), boss (+2). Vòng 3: Trọng tài + hệ thống thẻ (+3) |
| M2 | 10 | 10 | **11** | +18 asset (sprite Trọng tài, 3 icon kỹ năng, 3 VFX thẻ) |
| **Tổng** | **35** | **40** | **45** | |

**Không ăn theo World Cup 2026** (chốt 2026-07-16) → không có deadline thị trường. 45 ngày là ước lượng, không phải cam kết.


### Nguồn rủi ro trượt tiến độ

| Rủi ro | Khả năng | Ảnh hưởng | Giảm thiểu |
|--------|----------|-----------|-----------|
| 🔴 **Cân bằng Act 3 hiện KHÔNG có số** (`04` §4b) | **Đã xảy ra** | +5–8 ngày nếu bảng tính lộ ra đường cong sai | Bảng tính ở M0 việc 2b. Đây là rủi ro số 1 và nó không còn là rủi ro — nó là hiện trạng. |
| **Mô hình chồng 3 giả định bịa** (`η × σ × τ`) | **Rất cao** — mỗi cái lệch 20% → tổng lệch 73% | +5–8 ngày | Auto-play ở M1 đo cả ba bằng số thật |
| **Trọng tài thành "thuế" thay vì lựa chọn** | **Cao** — tướng nhân sức mạnh rất dễ trượt thành bắt buộc | +2–3 ngày | Test `04` §8 mục 8: chênh tỉ lệ thắng có/không Árbitro > 15 điểm → giảm τ |
| **Margin khan hiếm tiền chỉ còn 2.2%** (`04` §6) | **Cao** — build tối ưu giờ rẻ hơn trần | +1–2 ngày | Nếu vỡ: giảm `bountyMultiplier` Act 3 (2.5 → ~2.2), không động giá tướng |
| **18 kỹ năng combo ngoài dự kiến** | Trung bình | +2–3 ngày | 5/6 combo đi qua cơ chế slow (`02` §6) — nếu vỡ, vỡ ở đó trước |
| Art trễ | Trung bình | +? | Placeholder ở M0/M1 phải chơi được — không chờ art mới test được gameplay |
| Toolchain iOS | Trung bình | +1–2 ngày | Đâm vào nó ở M0 việc số 7 |
| Store từ chối vì bản quyền | **Thấp nhưng chí mạng** | +2 tuần | Guardrail `06` §5 áp dụng từ ngày đầu, không phải trước lúc nộp |

### Ngoài phạm vi (nếu bàn tới → là project mới)

Nhiều map · loại quân thứ 4 / boss · meta-progression · IAP / ads · leaderboard · multiplayer · nhiều chủ đề đội bóng
