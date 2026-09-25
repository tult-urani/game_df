# TASK ARCHIVE — TD World Cup, Define Phase

**Ngày:** 2026-07-16
**Triage:** Complex
**Workflow:** feature (docs-only)
**Kết quả:** PASS — 7/7 acceptance criteria

---

## Summary

Định nghĩa project game Tower Defense mobile chủ đề World Cup (cổ động viên Bồ Đào Nha tấn công, cầu thủ Argentina phòng ngự, thành = cầu môn). Giao 9 file markdown, zero code theo yêu cầu của user.

## Quyết định chốt (user xác nhận qua 4 câu hỏi)

| # | Quyết định | Giá trị |
|---|-----------|---------|
| 1 | Engine | Unity 6 LTS, 2D URP, C# |
| 2 | Scope | Chỉ `.md`; cấu trúc source được MÔ TẢ, không scaffold |
| 3 | IP | Biệt danh (La Pulga / D10S / El Fideo / Batigol / Dibu), không tên thật |
| 4 | Góc nhìn | Top-down 2D, grid-based placement |

## Boundaries (còn hiệu lực)

1. DO NOT viết code — phase define
2. DO NOT tạo project Unity
3. DO NOT dùng tên thật cầu thủ ngoài `docs/02-TOWERS.md` §1
4. DO NOT thêm dependency

## Files (9 created, 0 modified)

```
README.md                     73 dòng
docs/01-GAME-DESIGN.md       129
docs/02-TOWERS.md            185
docs/03-ENEMIES-WAVES.md     144
docs/04-ECONOMY-BALANCE.md   233   ← nguồn chân lý số liệu
docs/05-TECH-ARCHITECTURE.md 298
docs/06-ART-UX.md            146
docs/07-ROADMAP.md           109
docs/OPEN-QUESTIONS.md       112
```

## Truth Matrix

| AC | Nội dung | Verdict | Bằng chứng |
|----|----------|---------|-----------|
| 1 | 9 md, 0 code | ✅ | `find`: md=9, non-md=0 |
| 2 | 5 tướng × (cost/dmg/range/rate/skill/3 cấp), no TBD | ✅ | `04` §2 = 15 dòng cấp; grep TBD/TODO rỗng |
| 3 | 3 loại CĐV, 3 mức máu, 20 wave | ✅ | `03` §1 (100/220/550); bảng wave = 20 dòng |
| 4 | Chứng minh 550 + wave 1–5 đủ ≥2 tướng trước W5 | ✅ | `04` §5 — 550 ≥ 340 ngay từ W0 |
| 5 | Cây thư mục + module + build iOS | ✅ | `05` §2/§3/§6 |
| 6 | Không tên thật ngoài mapping | ✅ | grep loại trừ `02-TOWERS.md` → rỗng |
| 7 | OPEN-QUESTIONS liệt kê điểm chưa chốt | ✅ | 11 câu hỏi, 3 mức chặn |

## Worked (with evidence)

- **Mô hình cân bằng Wave Damage Budget** (`04` §3.2) — `WDB = η × Σ[DPSᵢ × (2Rᵢ + S)/v̄]`. Cho phép kiểm chứng cả 20 wave bằng số học thay vì cảm tính, và biến `range` thành thuộc tính có ý nghĩa toán học (không chỉ flavor).
- **Đường cong wave dẫn xuất từ mô hình, không áng chừng.** Bản nháp đầu (guess-first) nặng hơn dòng tiền ~50% → người chơi thua từ W3 dù chơi tối ưu. Phát hiện được chỉ vì có phép tính, không phải vì đọc lại.
- **Kiểm chứng "tiền luôn khan hiếm"** (`04` §6): chi phí max build 11 696 > tổng tiền cả đời 10 734 → quyết định kinh tế còn ý nghĩa tới W20.

## Did NOT work (and why)

- **Guess-first wave design** — viết bảng 20 wave theo trực giác trước, kiểm chứng sau. Toàn bộ bảng phải vứt và dựng lại từ công thức. Bài học: với hệ thống có ràng buộc số học, dẫn xuất trước, viết bảng sau.
- **Định cho El Fideo tăng tốc đánh theo cấp** — DPS scale theo tích (dmg × rate) → Lv3 mạnh ×3.75 thay vì ×2.5, phá mọi bảng cân bằng. Bỏ, chốt luật "một trục scale mỗi cấp" (`02` §3).
- **Spawn interval 0.7s với mô hình steady-state** — mô hình throughput (`DPS × spawn_interval ≥ HP`) cho kết quả vô lý (cần 143 DPS để qua wave 1). Sai vì wave hữu hạn bị chi phối bởi đuôi, không phải steady state. Thay bằng WDB.

## Không tự quyết được (chờ user)

11 câu ở `docs/OPEN-QUESTIONS.md`. Chặn M0: **Q1 layout map** (toàn bộ `04` giả định mọi ô phủ được đường — chưa có toạ độ thì giả định chưa được xác nhận), Q2 tên game, Q3 lưu trận dở.

## Next concrete

Chốt Q1 (layout map — bản vẽ tay đường chữ S + 12 ô) → xác nhận giả định chord=2R ở `04` §3.2 → mở M0 việc 1 (scaffold Unity theo `05` §2).

## Nợ kỹ thuật đã ghi nhận

- `η = 0.75` là giả định chưa đo (`04` §8 mục 1) — hiệu chỉnh ở M1 bằng auto-play 100 trận
- D10S "chạm 3 mục tiêu" chưa đo (`04` §8 mục 2) — quyết định D10S mạnh nhất hay vô dụng nhất
- Mô hình bỏ qua kỹ năng → lệch theo hướng an toàn (game thật dễ hơn bảng)
