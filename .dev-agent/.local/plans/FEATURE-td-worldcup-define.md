# PLAN: TD World Cup — Define Phase (docs-only)

**Triage:** Complex
**Ngày:** 2026-07-16
**Deliverable:** 9 file markdown. Zero code. Zero scaffold engine.

---

## Quyết định đã chốt (user xác nhận 2026-07-16)

| # | Quyết định | Giá trị |
|---|-----------|---------|
| 1 | Engine | Unity 6 LTS, 2D URP, C# |
| 2 | Scope phase này | Chỉ `.md`; cấu trúc source được MÔ TẢ, không tạo |
| 3 | IP tên cầu thủ | Biệt danh + art cách điệu (La Pulga / D10S / El Fideo / Batigol / Dibu) |
| 4 | Góc nhìn | Top-down 2D |

## Boundaries (DO-NOT — active)

| # | Constraint | Nguồn |
|---|-----------|-------|
| 1 | DO NOT viết code (C#, shader, script) | user ý (2) |
| 2 | DO NOT tạo project Unity / file `.unity` / `.meta` / Packages | user ý (2) + Q2 |
| 3 | DO NOT dùng tên thật cầu thủ trong docs sản phẩm | Q3 |
| 4 | DO NOT thêm dependency / package | scope |

---

## Micro-ADRs

**ADR-1 — Unity 6 LTS + 2D URP**
Context: cần iOS + Android từ 1 codebase, thể loại TD 2D, dev solo.
Decision: Unity 6 LTS, 2D URP, C#.
Consequences: cần Unity Hub + Xcode cho build iOS; license free dưới $200k doanh thu; build size lớn hơn Godot (~40MB base).

**ADR-2 — Balance data-driven, tách khỏi code**
Context: giai đoạn define sẽ đổi số liệu liên tục; muốn tune không cần build lại.
Decision: mọi chỉ số (tower/enemy/wave/economy) sống trong ScriptableObject, seed từ JSON trong `Resources/Config/`.
Consequences: thêm 1 lớp loader; đổi lại designer chỉnh số không đụng code. Docs phase này định nghĩa luôn schema JSON.

**ADR-3 — Biệt danh thay tên thật**
Context: quyền hình ảnh cầu thủ có chủ sở hữu; mục tiêu là phát hành store.
Decision: mọi tên hiển thị dùng biệt danh; art cách điệu, không mặt thật, không áo đấu chính thức.
Consequences: mất một phần nhận diện; đổi lại không rủi ro takedown. `docs/02` giữ 1 bảng mapping nội bộ để anh em dev hiểu ai là ai.

**ADR-4 — Grid-based placement, top-down**
Context: TD mobile 1 tay, tap chính xác trên màn 6".
Decision: đặt tower theo ô lưới cố định cạnh đường chạy (không free-placement).
Consequences: giảm tự do chiến thuật; đổi lại pathing đơn giản (không cần re-path động), tap không trượt, level design kiểm soát được.

---

## Danh sách file (9)

| # | File | Nội dung | Dòng ~ |
|---|------|----------|--------|
| 1 | `README.md` | Tổng quan, pitch 1 đoạn, cách đọc bộ docs, trạng thái từng doc | 60 |
| 2 | `docs/01-GAME-DESIGN.md` | Core loop, win/lose, phase 1 trận, điều khiển, phạm vi MVP | 150 |
| 3 | `docs/02-TOWERS.md` | 5 tướng: stats, vai trò, kỹ năng, cây nâng cấp 3 cấp, mapping biệt danh | 200 |
| 4 | `docs/03-ENEMIES-WAVES.md` | 3 loại CĐV, thiết kế 20 wave, nhịp độ, boss wave | 150 |
| 5 | `docs/04-ECONOMY-BALANCE.md` | Công thức kinh tế, bảng số đầy đủ, TTK math, kiểm chứng cân bằng | 200 |
| 6 | `docs/05-TECH-ARCHITECTURE.md` | Unity 6, cây thư mục, module + trách nhiệm, schema JSON, quy trình build iOS | 180 |
| 7 | `docs/06-ART-UX.md` | Art direction top-down, palette, HUD, guardrail IP, spec asset | 120 |
| 8 | `docs/07-ROADMAP.md` | Milestone M0→M3, definition of done từng mốc | 90 |
| 9 | `docs/OPEN-QUESTIONS.md` | Các điểm còn phải chốt, xếp theo mức chặn | 80 |

---

## System Impact

**Impact: isolated — none.**
Evidence: `/Users/tuantu/Desktop/dev/AI/mobile/defense/` rỗng hoàn toàn (chỉ có `.dev-agent/` do agent vừa tạo). Không callers, không shared contract, không schema, không consumer, không test hiện hữu, không migration. Mọi file đều là create-new. Không có gì để hồi quy.

---

## Acceptance Criteria (Truth Matrix cho VERIFY)

- [ ] AC-1: 9 file `.md` tồn tại đúng đường dẫn trên, 0 file code, 0 file project Unity
- [ ] AC-2: `docs/02-TOWERS.md` có đủ 5 tướng, mỗi tướng có cost / damage / range / attack-rate / 1 kỹ năng / 3 cấp nâng cấp — không ô nào TBD
- [ ] AC-3: `docs/03-ENEMIES-WAVES.md` có đúng 3 loại CĐV với 3 mức máu phân biệt + bảng 20 wave
- [ ] AC-4: `docs/04-ECONOMY-BALANCE.md` chứng minh được bằng phép tính rằng tiền khởi đầu 450 + thưởng wave 1-5 đủ mua ≥2 tướng trước wave 5 (không thua ép buộc)
- [ ] AC-5: `docs/05-TECH-ARCHITECTURE.md` có cây thư mục Unity đầy đủ + trách nhiệm từng module + các bước build iOS
- [ ] AC-6: Không tên thật cầu thủ nào xuất hiện ngoài bảng mapping nội bộ trong `docs/02`
- [ ] AC-7: `docs/OPEN-QUESTIONS.md` liệt kê các điểm chưa chốt để phiên sau thảo luận
