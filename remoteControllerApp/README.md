# Remote Controller App - Server

## 1. Tổng quan dự án

`remoteControllerApp` là **server trung gian** cho hệ thống điều khiển máy tính từ xa.

Mục tiêu của server là:

- Cho phép **Host** đăng ký online.
- Cho phép **Viewer** đăng ký online.
- Quản lý kết nối realtime bằng **SignalR**.
- Cho phép Viewer gửi yêu cầu điều khiển đến Host.
- Cho phép Host chấp nhận hoặc từ chối yêu cầu điều khiển.
- Quản lý phiên điều khiển tạm thời trong RAM.
- Cung cấp REST API để kiểm tra Host, Viewer và Session.

Kiến trúc hiện tại:

```text
Viewer / Host
     |
     | SignalR realtime
     v
ASP.NET Core Server
     |
     | In-memory Manager
     v
ConnectionManager + SessionManager
```

Ở giai đoạn hiện tại, server **chưa dùng database**.  
Tất cả trạng thái online và session đang được lưu tạm trong RAM.

---

## 2. Công nghệ sử dụng

```text
ASP.NET Core Web API
SignalR
C#
.NET 8 / .NET 10 template style
HTML + JavaScript test client
In-memory storage
```

SignalR được dùng để xử lý realtime communication giữa Host, Viewer và Server.

---

## 3. Các Phase đã thực hiện

## Phase 1 - Realtime Connection

Mục tiêu:

- Host kết nối được vào server.
- Viewer kết nối được vào server.
- Server lưu được `connectionId`.
- Server biết Host nào online.
- Server biết Viewer nào online.
- Server xử lý khi client disconnect.
- Có API kiểm tra danh sách Host/Viewer online.

Chức năng đã có:

```text
RegisterHost()
RegisterViewer()
PingHost()
PingViewer()
OnConnectedAsync()
OnDisconnectedAsync()
GET /api/Connection/hosts
GET /api/Connection/viewers
```

---

## Phase 2 - Control Session Request

Mục tiêu:

- Viewer chọn Host để gửi yêu cầu điều khiển.
- Server tạo phiên điều khiển trạng thái `Pending`.
- Host nhận request điều khiển realtime.
- Host có thể `Accept` hoặc `Reject`.
- Viewer nhận kết quả realtime.
- Server quản lý session trong RAM.
- Có API kiểm tra danh sách session.

Chức năng đã có:

```text
RequestControl()
AcceptControl()
RejectControl()
EndControl()
GET /api/Session
GET /api/Session/active
GET /api/Session/{sessionId}
```

---

## 4. Cấu trúc dự án hiện tại

```text
remoteControllerApp/
│
├── Controllers/
│   ├── ConnectionController.cs
│   └── SessionController.cs
│
├── DTOs/
│   ├── HostRegisterDto.cs
│   ├── ViewerRegisterDto.cs
│   ├── ConnectionInfoDto.cs
│   ├── ControlRequestDto.cs
│   ├── ControlResponseDto.cs
│   └── SessionInfoDto.cs
│
├── Hubs/
│   └── RemoteHub.cs
│
├── Manager/
│   ├── ConnectionManager.cs
│   └── SessionManager.cs
│
├── Models/
│   ├── HostConnection.cs
│   ├── ViewerConnection.cs
│   └── RemoteSession.cs
│
├── Repositories/
│
├── Services/
│   ├── ConnectionService.cs
│   └── SessionService.cs
│
├── wwwroot/
│   └── test-signalr.html
│
├── appsettings.json
├── Program.cs
├── Dockerfile
└── remoteControllerApp.http
```

---

# 5. Chức năng từng thư mục

## 5.1. Controllers/

Thư mục này chứa các REST API thông thường.

REST API dùng để:

- Kiểm tra danh sách Host online.
- Kiểm tra danh sách Viewer online.
- Kiểm tra danh sách session.
- Debug trạng thái server.

Các controller hiện tại:

```text
ConnectionController.cs
SessionController.cs
```

---

## 5.2. DTOs/

DTO là viết tắt của **Data Transfer Object**.

Thư mục này chứa các object dùng để truyền dữ liệu giữa:

```text
Client <-> Server
```

Ví dụ:

- Host gửi thông tin đăng ký lên server.
- Viewer gửi yêu cầu điều khiển Host.
- Host gửi phản hồi accept/reject.
- Server trả danh sách connection/session cho API.

DTO không nên chứa business logic.

---

## 5.3. Hubs/

Thư mục này chứa SignalR Hub.

SignalR Hub là phần quan trọng nhất của server.

Nhiệm vụ:

- Nhận kết nối realtime từ Host/Viewer.
- Xử lý đăng ký Host.
- Xử lý đăng ký Viewer.
- Nhận yêu cầu điều khiển từ Viewer.
- Gửi request đến Host.
- Nhận accept/reject từ Host.
- Gửi kết quả về Viewer.
- Xử lý disconnect.

File chính:

```text
RemoteHub.cs
```

---

## 5.4. Manager/

Thư mục này chứa các class quản lý trạng thái realtime trong RAM.

Hiện tại có:

```text
ConnectionManager.cs
SessionManager.cs
```

Manager không làm việc với database.  
Manager chỉ giữ dữ liệu tạm khi app đang chạy.

Nếu server restart, dữ liệu trong Manager sẽ mất.

---

## 5.5. Models/

Thư mục này chứa các model chính của hệ thống.

Model thể hiện các thực thể trong domain:

```text
HostConnection
ViewerConnection
RemoteSession
```

Các model này được dùng bởi Manager và Service.

---

## 5.6. Services/

Thư mục này chứa business logic.

Service đóng vai trò trung gian giữa:

```text
Controller / Hub
        |
        v
Manager
```

Service giúp code rõ ràng hơn, tránh để toàn bộ logic trong Hub hoặc Controller.

Hiện tại có:

```text
ConnectionService.cs
SessionService.cs
```

---

## 5.7. Repositories/

Thư mục này hiện tại chưa sử dụng.

Sau này nếu tích hợp database như Firebase, SQL Server hoặc PostgreSQL, có thể dùng thư mục này để chứa các class xử lý đọc/ghi dữ liệu.

Ví dụ sau này có thể thêm:

```text
UserRepository.cs
HostRepository.cs
SessionRepository.cs
ActivityLogRepository.cs
```

---

## 5.8. wwwroot/

Thư mục này chứa static file.

Hiện tại có file:

```text
test-signalr.html
```

File này dùng để test SignalR bằng trình duyệt.

Nó đóng vai trò client giả lập:

```text
Host giả lập
Viewer giả lập
```

Không phải app chính thức, chỉ là tool test nhanh trong quá trình phát triển.

---

# 6. Chức năng từng file

## 6.1. Program.cs

File khởi động chính của ASP.NET Core app.

Chức năng:

- Đăng ký Controller.
- Đăng ký SignalR.
- Đăng ký Dependency Injection.
- Đăng ký static files.
- Map REST API.
- Map SignalR Hub endpoint.

Các cấu hình chính:

```csharp
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<SessionManager>();

builder.Services.AddScoped<ConnectionService>();
builder.Services.AddScoped<SessionService>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<RemoteHub>("/remoteHub");
```

Endpoint SignalR chính:

```text
/remoteHub
```

---

## 6.2. Hubs/RemoteHub.cs

Đây là file xử lý realtime chính.

Các method hiện có:

```text
OnConnectedAsync()
OnDisconnectedAsync()

RegisterHost()
RegisterViewer()

PingHost()
PingViewer()

RequestControl()
AcceptControl()
RejectControl()
EndControl()
```

### OnConnectedAsync()

Được gọi khi client kết nối vào SignalR server.

Tác dụng:

- Log `connectionId`.
- Xác nhận client đã kết nối.

---

### OnDisconnectedAsync()

Được gọi khi client ngắt kết nối.

Tác dụng:

- Đánh dấu Host/Viewer offline.
- Kết thúc các session liên quan đến connection bị ngắt.
- Log trạng thái disconnect.

---

### RegisterHost()

Được Host gọi khi mở app và muốn đăng ký online.

Input:

```csharp
HostRegisterDto
```

Server sẽ lưu:

```text
HostId
ConnectionId
ComputerName
ConnectedAt
LastSeenAt
IsOnline
```

Sau khi đăng ký thành công, server gửi event:

```text
RegisterHostSuccess
```

Nếu thất bại:

```text
RegisterHostFailed
```

---

### RegisterViewer()

Được Viewer gọi khi mở app và muốn đăng ký online.

Input:

```csharp
ViewerRegisterDto
```

Server sẽ lưu:

```text
ViewerId
ConnectionId
ViewerName
ConnectedAt
LastSeenAt
IsOnline
```

Sau khi đăng ký thành công, server gửi event:

```text
RegisterViewerSuccess
```

Nếu thất bại:

```text
RegisterViewerFailed
```

---

### PingHost()

Host gọi định kỳ để báo vẫn còn sống.

Tác dụng:

- Cập nhật `LastSeenAt`.
- Trả về event `Pong`.

---

### PingViewer()

Viewer gọi định kỳ để báo vẫn còn sống.

Tác dụng:

- Cập nhật `LastSeenAt`.
- Trả về event `Pong`.

---

### RequestControl()

Viewer gọi method này để yêu cầu điều khiển một Host.

Input:

```csharp
ControlRequestDto
```

Luồng xử lý:

```text
Viewer gọi RequestControl()
Server kiểm tra Host online
Server kiểm tra Viewer online
Server tạo RemoteSession trạng thái Pending
Server gửi ReceiveControlRequest đến Host
Server gửi ControlRequestSent về Viewer
```

Event gửi đến Host:

```text
ReceiveControlRequest
```

Event gửi về Viewer:

```text
ControlRequestSent
```

Nếu lỗi:

```text
ControlRequestFailed
```

---

### AcceptControl()

Host gọi khi chấp nhận yêu cầu điều khiển.

Input:

```csharp
ControlResponseDto
```

Luồng xử lý:

```text
Host gọi AcceptControl()
Server tìm session
Server đổi trạng thái session thành Accepted
Server gửi ControlAccepted về Viewer
Server gửi AcceptControlSuccess về Host
```

Event gửi về Viewer:

```text
ControlAccepted
```

Event gửi về Host:

```text
AcceptControlSuccess
```

Nếu lỗi:

```text
AcceptControlFailed
```

---

### RejectControl()

Host gọi khi từ chối yêu cầu điều khiển.

Input:

```csharp
ControlResponseDto
```

Luồng xử lý:

```text
Host gọi RejectControl()
Server tìm session
Server đổi trạng thái session thành Rejected
Server lưu RejectReason
Server gửi ControlRejected về Viewer
Server gửi RejectControlSuccess về Host
```

Event gửi về Viewer:

```text
ControlRejected
```

Event gửi về Host:

```text
RejectControlSuccess
```

Nếu lỗi:

```text
RejectControlFailed
```

---

### EndControl()

Viewer hoặc Host gọi khi muốn kết thúc phiên điều khiển.

Input:

```csharp
string sessionId
```

Luồng xử lý:

```text
Client gọi EndControl(sessionId)
Server tìm session
Server đổi trạng thái session thành Ended
Server gửi ControlEnded đến cả Host và Viewer
```

Event gửi đến cả hai phía:

```text
ControlEnded
```

Nếu lỗi:

```text
EndControlFailed
```

---

## 6.3. Manager/ConnectionManager.cs

File này quản lý trạng thái online của Host và Viewer trong RAM.

Dữ liệu đang lưu:

```text
HostId -> HostConnection
ViewerId -> ViewerConnection
```

Các method chính:

```text
AddOrUpdateHost()
AddOrUpdateViewer()

GetHostById()
GetViewerById()

GetHostByConnectionId()
GetViewerByConnectionId()

GetOnlineHosts()
GetOnlineViewers()

MarkDisconnected()

UpdateHostLastSeen()
UpdateViewerLastSeen()
```

### AddOrUpdateHost()

Thêm mới hoặc cập nhật Host đang online.

Dùng khi Host gọi:

```text
RegisterHost()
```

---

### AddOrUpdateViewer()

Thêm mới hoặc cập nhật Viewer đang online.

Dùng khi Viewer gọi:

```text
RegisterViewer()
```

---

### GetHostById()

Tìm Host theo `hostId`.

Dùng khi Viewer gửi request điều khiển Host.

---

### GetViewerById()

Tìm Viewer theo `viewerId`.

Dùng khi tạo session điều khiển.

---

### MarkDisconnected()

Khi một connection bị ngắt, method này kiểm tra connection đó thuộc Host hay Viewer, sau đó đánh dấu offline.

---

### UpdateHostLastSeen()

Cập nhật thời điểm Host gần nhất còn sống.

---

### UpdateViewerLastSeen()

Cập nhật thời điểm Viewer gần nhất còn sống.

---

## 6.4. Manager/SessionManager.cs

File này quản lý session điều khiển trong RAM.

Dữ liệu đang lưu:

```text
SessionId -> RemoteSession
```

Các method chính:

```text
CreateSession()
GetSessionById()
GetAllSessions()
GetActiveSessions()

AcceptSession()
RejectSession()
EndSession()
EndSessionsByConnectionId()
```

### CreateSession()

Tạo một phiên điều khiển mới với trạng thái:

```text
Pending
```

Được gọi khi Viewer yêu cầu điều khiển Host.

---

### AcceptSession()

Đổi trạng thái session thành:

```text
Accepted
```

Được gọi khi Host đồng ý cho Viewer điều khiển.

---

### RejectSession()

Đổi trạng thái session thành:

```text
Rejected
```

Đồng thời lưu lý do từ chối nếu có.

---

### EndSession()

Đổi trạng thái session thành:

```text
Ended
```

Được gọi khi Viewer hoặc Host kết thúc phiên.

---

### EndSessionsByConnectionId()

Khi Host hoặc Viewer disconnect, server tự động kết thúc các session liên quan đến connection đó.

---

## 6.5. Services/ConnectionService.cs

Service này xử lý logic liên quan đến kết nối Host/Viewer.

Các method chính:

```text
GetOnlineHosts()
GetOnlineViewers()
```

Nó lấy dữ liệu từ `ConnectionManager`, sau đó convert sang `ConnectionInfoDto` để trả về API.

---

## 6.6. Services/SessionService.cs

Service này xử lý logic liên quan đến session điều khiển.

Các method chính:

```text
CreateControlRequest()
AcceptSession()
RejectSession()
EndSession()

GetAllSessions()
GetActiveSessions()
GetSessionById()
```

### CreateControlRequest()

Luồng xử lý:

```text
Kiểm tra Host có online không
Kiểm tra Viewer có online không
Tạo session Pending
Trả session về RemoteHub
```

Nếu Host hoặc Viewer không online, method sẽ throw lỗi.

---

### GetAllSessions()

Trả về toàn bộ session trong RAM.

---

### GetActiveSessions()

Trả về các session đang ở trạng thái:

```text
Pending
Accepted
```

---

### GetSessionById()

Trả về chi tiết một session theo `sessionId`.

---

## 6.7. Controllers/ConnectionController.cs

REST API kiểm tra trạng thái kết nối.

Endpoint:

```text
GET /api/Connection/hosts
GET /api/Connection/viewers
```

### GET /api/Connection/hosts

Trả danh sách Host đang online.

Ví dụ response:

```json
[
  {
    "id": "HOST_001",
    "connectionId": "abc123",
    "name": "Test Computer",
    "isOnline": true,
    "connectedAt": "2026-05-10T05:00:00Z",
    "lastSeenAt": "2026-05-10T05:01:00Z"
  }
]
```

---

### GET /api/Connection/viewers

Trả danh sách Viewer đang online.

Ví dụ response:

```json
[
  {
    "id": "VIEWER_001",
    "connectionId": "xyz456",
    "name": "Test Viewer",
    "isOnline": true,
    "connectedAt": "2026-05-10T05:00:00Z",
    "lastSeenAt": "2026-05-10T05:01:00Z"
  }
]
```

---

## 6.8. Controllers/SessionController.cs

REST API kiểm tra session.

Endpoint:

```text
GET /api/Session
GET /api/Session/active
GET /api/Session/{sessionId}
```

### GET /api/Session

Trả về tất cả session.

Ví dụ:

```json
[
  {
    "sessionId": "47324479dfe4eeab222f265edd486d2",
    "hostId": "HOST_001",
    "viewerId": "VIEWER_001",
    "status": "Accepted",
    "createdAt": "2026-05-10T05:18:59Z",
    "acceptedAt": "2026-05-10T05:19:12Z",
    "rejectedAt": null,
    "endedAt": null,
    "rejectReason": null
  }
]
```

---

### GET /api/Session/active

Trả về các session đang active.

Active bao gồm:

```text
Pending
Accepted
```

---

### GET /api/Session/{sessionId}

Trả về chi tiết một session theo `sessionId`.

Nếu không tìm thấy, trả về `404 Not Found`.

---

## 6.9. Models/HostConnection.cs

Model đại diện cho một Host đang kết nối.

Các field:

```text
HostId
ConnectionId
ComputerName
ConnectedAt
LastSeenAt
IsOnline
```

Ý nghĩa:

- `HostId`: mã định danh của máy bị điều khiển.
- `ConnectionId`: mã kết nối SignalR hiện tại.
- `ComputerName`: tên máy.
- `ConnectedAt`: thời điểm kết nối.
- `LastSeenAt`: lần cuối Host ping server.
- `IsOnline`: trạng thái online/offline.

---

## 6.10. Models/ViewerConnection.cs

Model đại diện cho một Viewer đang kết nối.

Các field:

```text
ViewerId
ConnectionId
ViewerName
ConnectedAt
LastSeenAt
IsOnline
```

Ý nghĩa:

- `ViewerId`: mã định danh của máy điều khiển.
- `ConnectionId`: mã kết nối SignalR hiện tại.
- `ViewerName`: tên Viewer.
- `ConnectedAt`: thời điểm kết nối.
- `LastSeenAt`: lần cuối Viewer ping server.
- `IsOnline`: trạng thái online/offline.

---

## 6.11. Models/RemoteSession.cs

Model đại diện cho phiên điều khiển từ xa.

Các field:

```text
SessionId
HostId
ViewerId
HostConnectionId
ViewerConnectionId
Status
CreatedAt
AcceptedAt
RejectedAt
EndedAt
RejectReason
```

Trạng thái session:

```text
Pending
Accepted
Rejected
Ended
```

Ý nghĩa:

- `SessionId`: mã phiên điều khiển.
- `HostId`: máy bị điều khiển.
- `ViewerId`: máy điều khiển.
- `HostConnectionId`: connectionId của Host.
- `ViewerConnectionId`: connectionId của Viewer.
- `Status`: trạng thái session.
- `CreatedAt`: thời điểm tạo session.
- `AcceptedAt`: thời điểm Host chấp nhận.
- `RejectedAt`: thời điểm Host từ chối.
- `EndedAt`: thời điểm kết thúc session.
- `RejectReason`: lý do từ chối nếu có.

---

## 6.12. DTOs/HostRegisterDto.cs

DTO dùng khi Host đăng ký lên server.

Field:

```text
HostId
ComputerName
```

Ví dụ request:

```json
{
  "hostId": "HOST_001",
  "computerName": "Test Computer"
}
```

---

## 6.13. DTOs/ViewerRegisterDto.cs

DTO dùng khi Viewer đăng ký lên server.

Field:

```text
ViewerId
ViewerName
```

Ví dụ request:

```json
{
  "viewerId": "VIEWER_001",
  "viewerName": "Test Viewer"
}
```

---

## 6.14. DTOs/ConnectionInfoDto.cs

DTO dùng để trả thông tin Host/Viewer online qua API.

Field:

```text
Id
ConnectionId
Name
IsOnline
ConnectedAt
LastSeenAt
```

Dùng bởi:

```text
ConnectionController
ConnectionService
```

---

## 6.15. DTOs/ControlRequestDto.cs

DTO dùng khi Viewer gửi yêu cầu điều khiển Host.

Field:

```text
HostId
ViewerId
ViewerName
```

Ví dụ:

```json
{
  "hostId": "HOST_001",
  "viewerId": "VIEWER_001",
  "viewerName": "Test Viewer"
}
```

---

## 6.16. DTOs/ControlResponseDto.cs

DTO dùng khi Host phản hồi yêu cầu điều khiển.

Field:

```text
SessionId
Accepted
Reason
```

Dùng cho cả:

```text
AcceptControl()
RejectControl()
```

Ví dụ Accept:

```json
{
  "sessionId": "47324479dfe4eeab222f265edd486d2",
  "accepted": true,
  "reason": null
}
```

Ví dụ Reject:

```json
{
  "sessionId": "47324479dfe4eeab222f265edd486d2",
  "accepted": false,
  "reason": "Host rejected the request."
}
```

---

## 6.17. DTOs/SessionInfoDto.cs

DTO dùng để trả thông tin session qua REST API.

Field:

```text
SessionId
HostId
ViewerId
Status
CreatedAt
AcceptedAt
RejectedAt
EndedAt
RejectReason
```

Dùng bởi:

```text
SessionController
SessionService
```

---

## 6.18. wwwroot/test-signalr.html

File HTML dùng để test SignalR bằng trình duyệt.

Chức năng:

- Connect SignalR.
- Register Host.
- Register Viewer.
- Ping Host.
- Ping Viewer.
- Viewer gửi Request Control.
- Host Accept Control.
- Host Reject Control.
- Host/Viewer End Control.
- Mở API kiểm tra Host/Viewer/Session.

Cách dùng:

Mở 2 tab trình duyệt:

```text
http://localhost:5271/test-signalr.html
```

Tab 1 giả lập Host:

```text
Connect SignalR
Register Host
Nhận ReceiveControlRequest
Accept hoặc Reject
```

Tab 2 giả lập Viewer:

```text
Connect SignalR
Register Viewer
Request Control
Nhận ControlAccepted hoặc ControlRejected
End Control
```

---

# 7. Luồng hoạt động Phase 1

## 7.1. Host đăng ký online

```text
Host Client
   |
   | RegisterHost(hostId, computerName)
   v
RemoteHub
   |
   | AddOrUpdateHost()
   v
ConnectionManager
   |
   | RegisterHostSuccess
   v
Host Client
```

---

## 7.2. Viewer đăng ký online

```text
Viewer Client
   |
   | RegisterViewer(viewerId, viewerName)
   v
RemoteHub
   |
   | AddOrUpdateViewer()
   v
ConnectionManager
   |
   | RegisterViewerSuccess
   v
Viewer Client
```

---

## 7.3. Kiểm tra danh sách online

```text
Browser / Postman
   |
   | GET /api/Connection/hosts
   v
ConnectionController
   |
   v
ConnectionService
   |
   v
ConnectionManager
   |
   v
JSON response
```

---

# 8. Luồng hoạt động Phase 2

## 8.1. Viewer gửi yêu cầu điều khiển

```text
Viewer
   |
   | RequestControl(hostId, viewerId, viewerName)
   v
RemoteHub
   |
   v
SessionService
   |
   | kiểm tra Host online
   | kiểm tra Viewer online
   v
SessionManager
   |
   | tạo session Pending
   v
RemoteHub
   |
   | ReceiveControlRequest
   v
Host
```

Đồng thời Viewer nhận:

```text
ControlRequestSent
```

---

## 8.2. Host chấp nhận điều khiển

```text
Host
   |
   | AcceptControl(sessionId)
   v
RemoteHub
   |
   v
SessionService / SessionManager
   |
   | đổi status = Accepted
   v
RemoteHub
   |
   | ControlAccepted
   v
Viewer
```

Host nhận lại:

```text
AcceptControlSuccess
```

---

## 8.3. Host từ chối điều khiển

```text
Host
   |
   | RejectControl(sessionId, reason)
   v
RemoteHub
   |
   v
SessionService / SessionManager
   |
   | đổi status = Rejected
   v
RemoteHub
   |
   | ControlRejected
   v
Viewer
```

Host nhận lại:

```text
RejectControlSuccess
```

---

## 8.4. Kết thúc phiên điều khiển

```text
Viewer hoặc Host
   |
   | EndControl(sessionId)
   v
RemoteHub
   |
   v
SessionManager
   |
   | đổi status = Ended
   v
RemoteHub
   |
   | ControlEnded
   v
Host + Viewer
```

---

# 9. Các event SignalR đang dùng

## Client gọi lên Server

```text
RegisterHost
RegisterViewer
PingHost
PingViewer
RequestControl
AcceptControl
RejectControl
EndControl
```

---

## Server gửi về Client

```text
RegisterHostSuccess
RegisterHostFailed

RegisterViewerSuccess
RegisterViewerFailed

Pong

ReceiveControlRequest
ControlRequestSent
ControlRequestFailed

ControlAccepted
ControlRejected

AcceptControlSuccess
AcceptControlFailed

RejectControlSuccess
RejectControlFailed

ControlEnded
EndControlFailed
```

---

# 10. Các REST API hiện có

## Connection API

```text
GET /api/Connection/hosts
GET /api/Connection/viewers
```

---

## Session API

```text
GET /api/Session
GET /api/Session/active
GET /api/Session/{sessionId}
```

---

# 11. Cách chạy project

## 11.1. Chạy server

Chạy bằng Visual Studio hoặc terminal:

```bash
dotnet run
```

Server local hiện tại thường chạy ở:

```text
http://localhost:5271
```

SignalR endpoint:

```text
http://localhost:5271/remoteHub
```

HTML test page:

```text
http://localhost:5271/test-signalr.html
```

---

## 11.2. Test nhanh Phase 1

Mở:

```text
http://localhost:5271/test-signalr.html
```

Thao tác:

```text
Connect SignalR
Register Host
Register Viewer
Ping Host
Ping Viewer
```

Kiểm tra API:

```text
http://localhost:5271/api/Connection/hosts
http://localhost:5271/api/Connection/viewers
```

---

## 11.3. Test nhanh Phase 2

Mở 2 tab:

```text
Tab 1: http://localhost:5271/test-signalr.html
Tab 2: http://localhost:5271/test-signalr.html
```

Tab 1 giả lập Host:

```text
Connect SignalR
Register Host
```

Tab 2 giả lập Viewer:

```text
Connect SignalR
Register Viewer
Request Control
```

Quay lại Tab 1:

```text
Accept Control
```

Quay lại Tab 2:

```text
Kiểm tra ControlAccepted
```

Kiểm tra session:

```text
http://localhost:5271/api/Session
```

---

# 12. Trạng thái session

```text
Pending   : Viewer đã gửi yêu cầu, Host chưa phản hồi
Accepted  : Host đã chấp nhận điều khiển
Rejected  : Host đã từ chối điều khiển
Ended     : Phiên đã kết thúc
```

---

# 13. Lưu ý hiện tại

Hiện tại hệ thống mới là backend MVP.

Chưa có:

```text
Authentication
JWT
Firebase
Database
Screen streaming
Mouse control
Keyboard control
File transfer
Load balancer
Redis
HTTPS production
```

Dữ liệu đang lưu trong RAM, nên khi restart server:

```text
Danh sách Host online mất
Danh sách Viewer online mất
Danh sách Session mất
```

---

# 14. Phase tiếp theo

## Phase 3 - Screen Streaming

Mục tiêu:

```text
Host chụp màn hình
Host gửi frame màn hình lên Server
Server kiểm tra session Accepted
Server relay frame đến đúng Viewer
Viewer hiển thị màn hình Host
```

Server sẽ cần thêm:

```text
DTOs/ScreenFrameDto.cs
RemoteHub.SendScreenFrame()
```

Luồng dự kiến:

```text
Host
   |
   | SendScreenFrame(sessionId, imageBase64)
   v
Server
   |
   | kiểm tra session Accepted
   v
Viewer
   |
   | ReceiveScreenFrame
   v
Hiển thị ảnh
```

Lưu ý:

```text
Không lưu frame màn hình vào database
Không lưu frame màn hình vào Firebase
Chỉ relay realtime qua SignalR
```

---

## Phase 4 - Mouse / Keyboard Control

Mục tiêu:

```text
Viewer gửi sự kiện chuột/phím
Server kiểm tra session Accepted
Server relay event đến Host
Host thực thi input bằng Windows API
```

Server sẽ cần thêm:

```text
MouseEventDto.cs
KeyboardEventDto.cs
SendMouseEvent()
SendKeyboardEvent()
```

---

# 15. Ghi chú thiết kế

## Vì sao dùng SignalR?

Vì hệ thống cần giao tiếp realtime:

```text
Host online/offline
Viewer request control
Host accept/reject
Screen frame
Mouse event
Keyboard event
```

SignalR phù hợp hơn REST API cho các dữ liệu realtime.

---

## Vì sao chưa dùng database?

Ở giai đoạn MVP, mục tiêu chính là chứng minh luồng realtime hoạt động.

Database nên thêm sau khi đã hoàn thành:

```text
Connection
Session
Screen streaming
Input control
```

Sau này Firebase có thể dùng để lưu:

```text
User
Host list
Permission
Session history
Activity log
```

Nhưng không nên dùng Firebase để lưu:

```text
Screen frame realtime
Mouse event realtime
Keyboard event realtime
```

---

## Vì sao dùng test-signalr.html?

Vì chưa có app WinForms Host/Viewer thật.

File HTML giúp test nhanh:

```text
SignalR connection
Register Host
Register Viewer
Request Control
Accept/Reject
End Control
```

Sau này khi làm WinForms, cơ chế sẽ giống HTML test, chỉ khác client library:

```text
HTML dùng signalR JavaScript client
WinForms dùng Microsoft.AspNetCore.SignalR.Client
```
