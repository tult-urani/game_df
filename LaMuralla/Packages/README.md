# Vì sao `manifest.json` ngắn thế này

Chú thích không để trong `manifest.json` được: Package Manager kiểm **mọi** khoá
trong `dependencies` như tên package, kể cả khoá bắt đầu bằng `_`. Thả một
`"_comment"` vào là Unity từ chối resolve:

```
Project has invalid dependencies:
  _comment: Package name '_comment' is invalid.
```

(Khác với `config/*.json` của dự án, nơi khoá `_` LÀ chú thích — `ConfigMapper`
bỏ qua chúng. Hai file, hai luật.)

## Chuyện đã xảy ra

Template `2d-cross-platform` cho **17 gói**. Sau lần import đầu tiên, Unity tự
thêm thành **57**. Trong số tự thêm có:

| Gói | Vấn đề |
|---|---|
| `com.unity.purchasing` | Gắn quyền **In-App Purchase** vào app. Personal team không được phép IAP → `xcodebuild` từ chối ký. Game này không bán gì. |
| `com.unity.analytics` | Gửi dữ liệu người dùng lên Unity. Không ai yêu cầu. |
| `com.unity.multiplayer.center` | Game một người. |
| `com.unity.ai.navigation` | NavMesh 3D. Quân đi theo spline ở `config/path.json`. |
| `com.unity.xr.legacyinputhelpers` | Không có VR/AR. |
| `com.unity.visualscripting` | Logic viết bằng C#. |

Lỗi build ký tên thực ra đang bảo vệ: nó chặn một app tower defense khỏi việc
xin quyền mua hàng mà chủ dự án không hề biết.

## Luật

Danh sách hiện tại là thứ game **này** thật sự dùng. Thêm gói mới thì phải nói
được nó dùng ở đâu. Nếu Unity lại tự thêm gì đó, `packages-lock.json` sẽ lộ ra
trong diff — đó là lý do file đó được commit.
