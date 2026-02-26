# Tài liệu Dự án Flux (DATN)

Chào mừng bạn đến với dự án **Flux** - Một nền tảng giao tiếp và quản lý công việc hợp nhất (tương tự Slack, Discord kết hợp với TickTick).

## 1. Tổng quan Công nghệ (Tech Stack)

Dự án được xây dựng trên nền tảng hiện đại nhất của Microsoft:

- **Backend:** .NET 10 (ASP.NET Core API).
- **Frontend:** Blazor Server với chế độ **InteractiveServer**.
- **UI Framework:** Microsoft Fluent UI (Dựa trên kiến trúc của dự án, có thể sử dụng thêm MudBlazor).
- **Database:** PostgreSQL kết hợp với Entity Framework Core.
- **Real-time:** SignalR để xử lý tin nhắn và thông báo tức thời.
- **Kiến trúc:** **Vertical Slice Architecture** (Kiến trúc cắt dọc). Thay vì chia theo Layer (UI, Business, Data), chúng ta chia theo Feature. Mỗi tính năng sẽ nằm trong một thư mục riêng chứa cả Controller, Logic và Model liên quan.

---

## 2. Cấu trúc Thư mục

- `Domain/Entities`: Chứa các thực thể chính của hệ thống (User, Message, Channel, Workspace).
- `Features/`: Nơi chứa logic nghiệp vụ theo từng tính năng (Ví dụ: `Channels/CreateChannel`).
- `Infrastructure/`: Chứa cấu hình Database (DbContext) và SignalR Hub.
- `Components/`: Chứa các giao diện Blazor.

---

## 3. Phân tích các bước thêm tính năng mới (Không Code)

Dưới đây là quy trình thực hiện cho hai yêu cầu bạn mong muốn:

### A. Tính năng Nhắn tin riêng (Direct Messaging - DM)

Hiện tại, tin nhắn (`Message`) đang gắn liền với một Kênh (`Channel`). Để thêm nhắn tin riêng giữa 2 người:

1.  **Cập nhật Cơ sở dữ liệu (Domain):**
    - Chỉnh sửa thực thể `Message`: Cho phép `ChannelId` có thể null (vì tin nhắn riêng không thuộc channel).
    - Thêm trường `ReceiverId` (ID người nhận) vào thực thể `Message`.
    - (Nâng cao) Hoặc tạo một thực thể `Conversation` để quản lý cuộc hội thoại giữa 2 hoặc nhiều người.
2.  **Cập nhật Infrastructure:**
    - Cấu hình lại `FluxDbContext` để thiết lập quan hệ giữa Message và User người nhận.
    - Cập nhật `ChatHub` (SignalR): Thêm phương thức `SendPrivateMessage` sử dụng `Clients.User(userId)` để chỉ gửi tin nhắn cho đúng người nhận thay vì gửi cả Channel.
3.  **Tạo Feature mới (Vertical Slice):**
    - Tạo thư mục `Features/Messages/GetDirectMessages`: Chứa logic lấy lịch sử chat giữa 2 người.
    - Tạo thư mục `Features/Messages/SendDirectMessage`: Chứa logic lưu tin nhắn riêng vào DB.
4.  **Cập nhật Giao diện (UI):**
    - Sidebar: Thêm danh sách "Direct Messages" liệt kê các thành viên trong Workspace.
    - Chat Window: Logic hiển thị tin nhắn sẽ kiểm tra nếu đang ở chế độ DM thì gọi API lấy tin nhắn theo `ReceiverId`.

---

### B. Tính năng Đăng nhập & Đăng ký (Authentication & Authorization)

Vì đây là dự án Blazor Server, cách tối ưu nhất là sử dụng **ASP.NET Core Identity**.

1.  **Cấu hình Identity:**
    - Cập nhật thực thể `User` kế thừa từ `IdentityUser<Guid>` của Microsoft.
    - Chỉnh sửa `FluxDbContext` kế thừa từ `IdentityDbContext`.
2.  **Thiết lập tại Program.cs:**
    - Đăng ký dịch vụ Identity: `builder.Services.AddIdentity<User, IdentityRole>()...`
    - Cấu hình Cookie Authentication để giữ phiên đăng nhập.
    - Thêm `app.UseAuthentication()` và `app.UseAuthorization()` vào pipeline.
3.  **Tạo các Trang Giao diện (UI):**
    - `Components/Pages/Account/Register.razor`: Form đăng ký (Email, Mật khẩu, Tên hiển thị).
    - `Components/Pages/Account/Login.razor`: Form đăng nhập.
    - Sử dụng `NavigationManager` để chuyển hướng người dùng sau khi đăng nhập thành công.
4.  **Bảo mật các tính năng:**
    - Sử dụng thuộc tính `[Authorize]` trên các Controller hoặc `@attribute [Authorize]` trong các trang Blazor để yêu cầu người dùng phải đăng nhập mới được truy cập.
5.  **Quản lý Session:**
    - Sử dụng `AuthenticationStateProvider` của Blazor để hiển thị thông tin người dùng hiện tại (Avatar, Tên) trên thanh Header.

---

## 4. Nguyên tắc phát triển cần lưu ý

- **English First:** Tên biến, class, comment trong code nên dùng tiếng Anh. Tài liệu này dùng tiếng Việt để bạn dễ hình dung.
- **Result Pattern:** Luôn trả về một đối tượng Result (Success/Failure) thay vì bắn Exception để code sạch và dễ kiểm soát lỗi.
- **FluentValidation:** Luôn kiểm tra dữ liệu đầu vào (ví dụ: mật khẩu phải có 8 ký tự, tên kênh không được để trống) trước khi xử lý.
