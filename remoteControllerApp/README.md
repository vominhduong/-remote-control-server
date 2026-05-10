# Remote Controller App - Server

## 1. Tổng quan

`remoteControllerApp` là server trung gian cho hệ thống điều khiển máy tính từ xa.

Server hiện đang xử lý các phần chính:

```text
Host / Viewer kết nối realtime
Quản lý trạng thái online
Tạo phiên điều khiển
Host chấp nhận hoặc từ chối phiên
Host gửi màn hình sang Viewer
Viewer gửi thao tác chuột / bàn phím về Host
```

Kiến trúc hiện tại:

```text
Host App / Test HTML
        |
        | SignalR
        v
ASP.NET Core Server
        |
        | In-memory managers
        v
ConnectionManager + SessionManager
        |
        v
REST API kiểm tra trạng thái
```

Hiện tại hệ thống đang ở mức **MVP server + HTML test**. Chưa dùng database, Firebase, JWT hoặc WinForms thật.

---

## 2. Công nghệ sử dụng

```text
ASP.NET Core Web API
SignalR
C#
HTML + JavaScript SignalR Client
In-memory storage
```

SignalR dùng cho realtime:

```text
Host online
Viewer online
Request control
Accept / Reject session
Screen streaming
Mouse event
Keyboard event
```

REST API dùng để kiểm tra trạng thái:

```text
Danh sách Host online
Danh sách Viewer online
Danh sách session
```

---

## 3. Các phase đã thực hiện

## Phase 1 — Host / Viewer kết nối realtime

Mục tiêu:

```text
Host kết nối được vào SignalR Server
Viewer kết nối được vào SignalR Server
Server biết Host nào đang online
Server biết Viewer nào đang online
Server xử lý khi client disconnect
```

Đã thực hiện:

```text
RegisterHost()
RegisterViewer()
PingHost()
PingViewer()
OnConnectedAsync()
OnDisconnectedAsync()
ConnectionManager
ConnectionController
ConnectionService
```

Kết quả:

```text
Host đăng ký online thành công
Viewer đăng ký online thành công
API xem được danh sách Host/Viewer online
```

---

## Phase 2 — Tạo phiên điều khiển

Mục tiêu:

```text
Viewer yêu cầu điều khiển Host
Server tạo session trạng thái Pending
Host nhận request realtime
Host Accept hoặc Reject
Viewer nhận kết quả
```

Đã thực hiện:

```text
RemoteSession model
SessionManager
SessionService
SessionController
RequestControl()
AcceptControl()
RejectControl()
EndControl()
```

Trạng thái session:

```text
Pending   : Viewer đã gửi yêu cầu, Host chưa phản hồi
Accepted  : Host đã đồng ý
Rejected  : Host đã từ chối
Ended     : Phiên đã kết thúc
```

Kết quả:

```text
Viewer gửi yêu cầu điều khiển Host
Host nhận request
Host đồng ý hoặc từ chối
Server quản lý session trong RAM
```

---

## Phase 3 — Truyền màn hình Host sang Viewer

Mục tiêu:

```text
Host gửi frame màn hình lên server
Server kiểm tra session đã Accepted
Server relay frame tới đúng Viewer
Viewer hiển thị màn hình
Viewer thấy metadata màn hình và tọa độ chuột Host
```

Đã thực hiện:

```text
ScreenFrameDto
SendScreenFrame()
ReceiveScreenFrame
MaximumReceiveMessageSize trong SignalR
Fake screen streaming trong test-signalr.html
```

Dữ liệu frame gồm:

```text
SessionId
ImageBase64
ScreenWidth
ScreenHeight
FrameWidth
FrameHeight
MouseX
MouseY
SentAt
```

Lưu ý:

```text
Frame màn hình không lưu vào database
Frame màn hình không lưu vào Firebase
Frame chỉ relay realtime qua SignalR
```

---

## Phase 4 — Relay chuột và bàn phím từ Viewer về Host

Mục tiêu:

```text
Viewer thao tác chuột trên màn hình remote
Viewer nhấn phím trên màn hình remote
Server kiểm tra session Accepted
Server relay mouse / keyboard event về đúng Host
Host nhận và log event
```

Đã thực hiện:

```text
MouseEventDto
KeyboardEventDto
SendMouseEvent()
SendKeyboardEvent()
ReceiveMouseEvent
ReceiveKeyboardEvent
HTML test bắt mouse/keyboard trên remoteScreenWrapper
```

Mouse actions hiện hỗ trợ ở mức relay:

```text
MouseDown
MouseUp
MouseMove
LeftClick
RightClick
DoubleClick
Scroll
```

Keyboard actions hiện hỗ trợ ở mức relay:

```text
KeyDown
KeyUp
```

Kết quả:

```text
Viewer click / right click / double click / scroll trên ảnh remote
Server gửi event về Host
Host nhận được ReceiveMouseEvent
Viewer nhấn phím
Host nhận được ReceiveKeyboardEvent
```

Ở Phase 4 hiện tại, Host mới **nhận và log event**, chưa thực thi chuột/phím thật trên máy tính.

---

## 4. Cấu trúc project hiện tại

```text
remoteControllerApp/
│
├── Controllers/
│   ├── ConnectionController.cs
│   └── SessionController.cs
│
├── DTOs/
│   ├── ConnectionInfoDto.cs
│   ├── ControlRequestDto.cs
│   ├── ControlResponseDto.cs
│   ├── HostRegisterDto.cs
│   ├── KeyboardEventDto.cs
│   ├── MouseEventDto.cs
│   ├── ScreenFrameDto.cs
│   ├── SessionInfoDto.cs
│   └── ViewerRegisterDto.cs
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
│   ├── RemoteSession.cs
│   └── ViewerConnection.cs
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

## 5. Chức năng từng thư mục

## Controllers/

Chứa REST API để kiểm tra trạng thái hệ thống.

Hiện có:

```text
ConnectionController.cs
SessionController.cs
```

Dùng cho:

```text
Xem Host online
Xem Viewer online
Xem session
Debug quá trình test
```

---

## DTOs/

Chứa object truyền dữ liệu giữa client và server.

Hiện có:

```text
HostRegisterDto
ViewerRegisterDto
ConnectionInfoDto
ControlRequestDto
ControlResponseDto
SessionInfoDto
ScreenFrameDto
MouseEventDto
KeyboardEventDto
```

DTO không chứa business logic. DTO chỉ mô tả dữ liệu gửi/nhận.

---

## Hubs/

Chứa SignalR Hub.

File chính:

```text
RemoteHub.cs
```

Đây là phần realtime trung tâm của hệ thống.

Xử lý:

```text
Register Host
Register Viewer
Request control
Accept / Reject control
End control
Send screen frame
Send mouse event
Send keyboard event
```

---

## Manager/

Chứa các class quản lý trạng thái trong RAM.

Hiện có:

```text
ConnectionManager.cs
SessionManager.cs
```

Dữ liệu trong Manager sẽ mất khi restart server.

---

## Models/

Chứa model chính của hệ thống:

```text
HostConnection
ViewerConnection
RemoteSession
```

---

## Services/

Chứa business logic trung gian giữa Controller/Hub và Manager.

Hiện có:

```text
ConnectionService.cs
SessionService.cs
```

---

## Repositories/

Hiện chưa dùng.

Sau này nếu tích hợp Firebase hoặc database khác, có thể thêm:

```text
UserRepository
HostRepository
SessionRepository
ActivityLogRepository
PermissionRepository
```

---

## wwwroot/

Chứa static file test.

Hiện có:

```text
test-signalr.html
```

File này là client test tạm thời cho:

```text
Host giả lập
Viewer giả lập
Screen streaming giả lập
Mouse event giả lập
Keyboard event giả lập
```

---

## 6. Chức năng từng file chính

## Program.cs

Chức năng:

```text
Đăng ký Controllers
Đăng ký SignalR
Tăng MaximumReceiveMessageSize
Đăng ký ConnectionManager / SessionManager
Đăng ký ConnectionService / SessionService
Bật static files
Map Controllers
Map RemoteHub
```

Cấu hình quan trọng:

```csharp
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});
```

Mục đích:

```text
Cho phép gửi frame ảnh Base64 lớn hơn giới hạn mặc định 32KB của SignalR
```

Endpoint chính:

```text
/remoteHub
```

---

## Hubs/RemoteHub.cs

File xử lý realtime chính.

Method hiện có:

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
SendScreenFrame()
SendMouseEvent()
SendKeyboardEvent()
```

---

## Manager/ConnectionManager.cs

Quản lý trạng thái online của Host và Viewer.

Lưu trong RAM:

```text
HostId -> HostConnection
ViewerId -> ViewerConnection
```

Method chính:

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

---

## Manager/SessionManager.cs

Quản lý session điều khiển trong RAM.

Lưu:

```text
SessionId -> RemoteSession
```

Method chính:

```text
CreateSession()
GetSessionById()
GetAcceptedSession()
GetAllSessions()
GetActiveSessions()
AcceptSession()
RejectSession()
EndSession()
EndSessionsByConnectionId()
```

`GetAcceptedSession()` được dùng ở Phase 3 và Phase 4 để đảm bảo chỉ relay dữ liệu khi Host đã đồng ý điều khiển.

---

## Services/ConnectionService.cs

Trả dữ liệu Host/Viewer online cho REST API.

Method chính:

```text
GetOnlineHosts()
GetOnlineViewers()
```

---

## Services/SessionService.cs

Xử lý logic session.

Method chính:

```text
CreateControlRequest()
AcceptSession()
RejectSession()
EndSession()
GetAllSessions()
GetActiveSessions()
GetSessionById()
```

---

## Controllers/ConnectionController.cs

REST API kiểm tra Host/Viewer online.

Endpoints:

```text
GET /api/Connection/hosts
GET /api/Connection/viewers
```

---

## Controllers/SessionController.cs

REST API kiểm tra session.

Endpoints:

```text
GET /api/Session
GET /api/Session/active
GET /api/Session/{sessionId}
```

---

## DTOs/ScreenFrameDto.cs

Dùng khi Host gửi frame màn hình lên server.

Fields:

```text
SessionId
ImageBase64
ScreenWidth
ScreenHeight
FrameWidth
FrameHeight
MouseX
MouseY
SentAt
```

---

## DTOs/MouseEventDto.cs

Dùng khi Viewer gửi thao tác chuột về Host.

Fields:

```text
SessionId
Action
X
Y
ScreenWidth
ScreenHeight
Button
Delta
SentAt
```

Action có thể là:

```text
MouseDown
MouseUp
MouseMove
LeftClick
RightClick
DoubleClick
Scroll
```

---

## DTOs/KeyboardEventDto.cs

Dùng khi Viewer gửi thao tác bàn phím về Host.

Fields:

```text
SessionId
Action
Key
Code
CtrlKey
ShiftKey
AltKey
SentAt
```

Action có thể là:

```text
KeyDown
KeyUp
```

---

## wwwroot/test-signalr.html

File test bằng browser.

Chức năng:

```text
Connect SignalR
Register Host
Register Viewer
Ping Host
Ping Viewer
Request Control
Accept Control
Reject Control
End Control
Send fake screen frame
Start fake screen streaming
Stop fake screen streaming
Click / right click / double click / scroll trên remote screen
Gửi keyboard event
Mở REST API kiểm tra
```

Ghi chú:

```text
Fake screen được tạo bằng canvas
Ảnh gửi bằng JPEG Base64
Chuột Host fake đứng yên ở giữa màn hình
Mouse event được bắt trên remoteScreenWrapper
Keyboard event hoạt động khi focus vào remoteScreenWrapper
```

---

# 7. REST API hiện có

## 7.1. Connection APIs

## GET `/api/Connection/hosts`

Lấy danh sách Host đang online.

Response ví dụ:

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

Dùng để kiểm tra:

```text
Host đã RegisterHost thành công chưa
Host còn online không
```

---

## GET `/api/Connection/viewers`

Lấy danh sách Viewer đang online.

Response ví dụ:

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

Dùng để kiểm tra:

```text
Viewer đã RegisterViewer thành công chưa
Viewer còn online không
```

---

## 7.2. Session APIs

## GET `/api/Session`

Lấy tất cả session trong RAM.

Response ví dụ:

```json
[
  {
    "sessionId": "923dcb05cc5a48c7adc04359202014d4",
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

Dùng để kiểm tra:

```text
Session đã được tạo chưa
Session đang Pending / Accepted / Rejected / Ended
```

---

## GET `/api/Session/active`

Lấy các session đang hoạt động.

Active gồm:

```text
Pending
Accepted
```

Không gồm:

```text
Rejected
Ended
```

---

## GET `/api/Session/{sessionId}`

Lấy chi tiết một session theo `sessionId`.

Nếu không tìm thấy:

```json
{
  "message": "Session not found."
}
```

---

# 8. SignalR Hub endpoint

## Endpoint

```text
/remoteHub
```

URL local thường dùng:

```text
http://localhost:5271/remoteHub
```

Lưu ý:

```text
Không mở trực tiếp /remoteHub bằng browser
SignalR client phải kết nối qua thư viện client
```

HTML test kết nối bằng:

```javascript
connection = new signalR.HubConnectionBuilder()
    .withUrl("/remoteHub")
    .withAutomaticReconnect()
    .build();
```

---

# 9. SignalR methods client gọi lên server

## 9.1. RegisterHost

Host gọi để đăng ký online.

```text
RegisterHost(HostRegisterDto request)
```

Payload:

```json
{
  "hostId": "HOST_001",
  "computerName": "Test Computer"
}
```

Server trả event:

```text
RegisterHostSuccess
RegisterHostFailed
```

---

## 9.2. RegisterViewer

Viewer gọi để đăng ký online.

```text
RegisterViewer(ViewerRegisterDto request)
```

Payload:

```json
{
  "viewerId": "VIEWER_001",
  "viewerName": "Test Viewer"
}
```

Server trả event:

```text
RegisterViewerSuccess
RegisterViewerFailed
```

---

## 9.3. PingHost

Host ping để cập nhật `LastSeenAt`.

```text
PingHost(hostId)
```

Server trả:

```text
Pong
```

---

## 9.4. PingViewer

Viewer ping để cập nhật `LastSeenAt`.

```text
PingViewer(viewerId)
```

Server trả:

```text
Pong
```

---

## 9.5. RequestControl

Viewer gửi yêu cầu điều khiển Host.

```text
RequestControl(ControlRequestDto request)
```

Payload:

```json
{
  "hostId": "HOST_001",
  "viewerId": "VIEWER_001",
  "viewerName": "Test Viewer"
}
```

Server gửi tới Host:

```text
ReceiveControlRequest
```

Server gửi về Viewer:

```text
ControlRequestSent
ControlRequestFailed
```

---

## 9.6. AcceptControl

Host chấp nhận yêu cầu điều khiển.

```text
AcceptControl(ControlResponseDto response)
```

Payload:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "accepted": true,
  "reason": null
}
```

Server gửi về Viewer:

```text
ControlAccepted
```

Server gửi về Host:

```text
AcceptControlSuccess
AcceptControlFailed
```

---

## 9.7. RejectControl

Host từ chối yêu cầu điều khiển.

```text
RejectControl(ControlResponseDto response)
```

Payload:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "accepted": false,
  "reason": "Host rejected the request."
}
```

Server gửi về Viewer:

```text
ControlRejected
```

Server gửi về Host:

```text
RejectControlSuccess
RejectControlFailed
```

---

## 9.8. EndControl

Host hoặc Viewer kết thúc session.

```text
EndControl(sessionId)
```

Payload:

```text
"923dcb05cc5a48c7adc04359202014d4"
```

Server gửi tới cả Host và Viewer:

```text
ControlEnded
EndControlFailed
```

---

## 9.9. SendScreenFrame

Host gửi frame màn hình sang Viewer.

```text
SendScreenFrame(ScreenFrameDto request)
```

Payload:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "imageBase64": "...",
  "screenWidth": 1920,
  "screenHeight": 1080,
  "frameWidth": 960,
  "frameHeight": 540,
  "mouseX": 960,
  "mouseY": 540,
  "sentAt": "2026-05-10T05:20:00Z"
}
```

Server gửi tới Viewer:

```text
ReceiveScreenFrame
```

Server gửi lỗi về Host:

```text
SendScreenFrameFailed
```

Điều kiện relay:

```text
Session phải có status = Accepted
```

---

## 9.10. SendMouseEvent

Viewer gửi thao tác chuột về Host.

```text
SendMouseEvent(MouseEventDto request)
```

Payload ví dụ click trái:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "action": "LeftClick",
  "x": 960,
  "y": 540,
  "screenWidth": 1920,
  "screenHeight": 1080,
  "button": "Left",
  "delta": 0,
  "sentAt": "2026-05-10T05:21:00Z"
}
```

Payload ví dụ scroll:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "action": "Scroll",
  "x": 960,
  "y": 540,
  "screenWidth": 1920,
  "screenHeight": 1080,
  "button": "Wheel",
  "delta": -120,
  "sentAt": "2026-05-10T05:21:00Z"
}
```

Server gửi tới Host:

```text
ReceiveMouseEvent
```

Server gửi lỗi về Viewer:

```text
SendMouseEventFailed
```

Điều kiện relay:

```text
Session phải có status = Accepted
```

---

## 9.11. SendKeyboardEvent

Viewer gửi thao tác bàn phím về Host.

```text
SendKeyboardEvent(KeyboardEventDto request)
```

Payload ví dụ:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "action": "KeyDown",
  "key": "a",
  "code": "KeyA",
  "ctrlKey": false,
  "shiftKey": false,
  "altKey": false,
  "sentAt": "2026-05-10T05:22:00Z"
}
```

Server gửi tới Host:

```text
ReceiveKeyboardEvent
```

Server gửi lỗi về Viewer:

```text
SendKeyboardEventFailed
```

Điều kiện relay:

```text
Session phải có status = Accepted
```

---

# 10. SignalR events server gửi về client

## Connection events

```text
RegisterHostSuccess
RegisterHostFailed
RegisterViewerSuccess
RegisterViewerFailed
Pong
```

## Session events

```text
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

## Screen streaming events

```text
ReceiveScreenFrame
SendScreenFrameFailed
```

## Input control events

```text
ReceiveMouseEvent
SendMouseEventFailed
ReceiveKeyboardEvent
SendKeyboardEventFailed
```

---

# 11. Luồng hoạt động tổng thể

## Host online

```text
Host
  |
  | RegisterHost(hostId, computerName)
  v
RemoteHub
  |
  v
ConnectionManager
  |
  | RegisterHostSuccess
  v
Host
```

## Viewer online

```text
Viewer
  |
  | RegisterViewer(viewerId, viewerName)
  v
RemoteHub
  |
  v
ConnectionManager
  |
  | RegisterViewerSuccess
  v
Viewer
```

## Tạo session điều khiển

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
  v
SessionManager tạo Pending session
  |
  | ReceiveControlRequest
  v
Host
```

## Host Accept

```text
Host
  |
  | AcceptControl(sessionId)
  v
RemoteHub
  |
  v
SessionManager đổi status = Accepted
  |
  | ControlAccepted
  v
Viewer
```

## Host gửi màn hình

```text
Host
  |
  | SendScreenFrame(sessionId, imageBase64, metadata)
  v
RemoteHub
  |
  | kiểm tra session Accepted
  v
Viewer
  |
  | ReceiveScreenFrame
  v
Hiển thị ảnh
```

## Viewer gửi chuột

```text
Viewer
  |
  | SendMouseEvent(sessionId, action, x, y)
  v
RemoteHub
  |
  | kiểm tra session Accepted
  v
Host
  |
  | ReceiveMouseEvent
  v
Log event hoặc thực thi về sau
```

## Viewer gửi bàn phím

```text
Viewer
  |
  | SendKeyboardEvent(sessionId, action, key, code)
  v
RemoteHub
  |
  | kiểm tra session Accepted
  v
Host
  |
  | ReceiveKeyboardEvent
  v
Log event hoặc thực thi về sau
```

---

# 12. Cách chạy và test

## Chạy server

```bash
dotnet run
```

Server local thường là:

```text
http://localhost:5271
```

HTML test:

```text
http://localhost:5271/test-signalr.html
```

REST API:

```text
http://localhost:5271/api/Connection/hosts
http://localhost:5271/api/Connection/viewers
http://localhost:5271/api/Session
http://localhost:5271/api/Session/active
```

---

## Test Phase 1

Mở:

```text
http://localhost:5271/test-signalr.html
```

Thao tác:

```text
Connect SignalR
Register Host
Register Viewer
```

Kiểm tra:

```text
/api/Connection/hosts
/api/Connection/viewers
```

---

## Test Phase 2

Mở 2 tab:

```text
Tab 1: Host
Tab 2: Viewer
```

Tab Host:

```text
Connect SignalR
Register Host
```

Tab Viewer:

```text
Connect SignalR
Register Viewer
Request Control
```

Tab Host:

```text
Accept Control
```

Kiểm tra:

```text
/api/Session
```

Session phải có:

```text
status = Accepted
```

---

## Test Phase 3

Sau khi session Accepted:

Tab Host:

```text
Start Fake Streaming
```

Tab Viewer:

```text
Thấy Fake Host Screen
Frame Count tăng
Screen Size hiển thị
Frame Size hiển thị
Host Mouse hiển thị
```

---

## Test Phase 4

Sau khi Viewer thấy màn hình fake:

Tab Viewer:

```text
Click vào ảnh remote screen
Right click vào ảnh
Double click vào ảnh
Scroll trên ảnh
Bấm Focus Remote Screen For Keyboard Test
Nhấn phím bất kỳ
```

Tab Host phải thấy log:

```text
HOST received ReceiveMouseEvent
HOST received ReceiveKeyboardEvent
```

Ví dụ Mouse Event:

```json
{
  "sessionId": "923dcb05cc5a48c7adc04359202014d4",
  "hostId": "HOST_001",
  "viewerId": "VIEWER_001",
  "action": "LeftClick",
  "x": 960,
  "y": 540,
  "screenWidth": 1920,
  "screenHeight": 1080,
  "button": "Left",
  "delta": 0
}
```

---

# 13. Lỗi thường gặp và cách xử lý

## API không thấy Host/Viewer

Nguyên nhân thường gặp:

```text
Chỉ bấm Connect SignalR nhưng chưa bấm Register Host / Register Viewer
ConnectionManager không đăng ký Singleton
Server restart làm mất dữ liệu RAM
```

Cách xử lý:

```text
Bấm Register Host / Register Viewer
Kiểm tra Program.cs có AddSingleton<ConnectionManager>()
Run server lại và test lại từ đầu
```

---

## The maximum message size of 32768B was exceeded

Nguyên nhân:

```text
Frame ảnh Base64 vượt giới hạn mặc định của SignalR
```

Cách xử lý trong Program.cs:

```csharp
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 10 * 1024 * 1024;
});
```

Đồng thời trong HTML dùng JPEG:

```javascript
canvas.toDataURL("image/jpeg", 0.6)
```

---

## SendScreenFrameFailed - Session is not accepted or not found

Nguyên nhân:

```text
Chưa Accept Control
SessionId rỗng
Server vừa restart
Session đã Ended
```

Cách xử lý:

```text
Register Host lại
Register Viewer lại
Request Control lại
Accept Control lại
Start Fake Streaming lại
```

---

## Click chuột không gửi event

Kiểm tra:

```text
Viewer đã thấy ảnh Fake Host Screen chưa
Session đang Accepted chưa
Click đúng vào vùng ảnh remote screen chưa
Tab Viewer có log "Click detected on remote screen" không
Tab Viewer có log "SendMouseEvent sent" không
```

Nếu Viewer có log gửi nhưng Host không nhận, kiểm tra:

```text
RemoteHub.cs có SendMouseEvent chưa
Server đang chạy bản code mới chưa
Session status có Accepted không
```

---

## Nhấn phím không gửi event

Cách xử lý:

```text
Bấm Focus Remote Screen For Keyboard Test
Sau đó mới nhấn phím
```

---

# 14. Trạng thái hiện tại

Hệ thống đã hoàn thành:

```text
Phase 1: Host / Viewer online
Phase 2: Session control
Phase 3: Screen streaming
Phase 4: Mouse / keyboard relay
```

Chưa thực hiện:

```text
WinForms Host thật
WinForms Viewer thật
Chụp màn hình thật từ Host
Thực thi chuột/phím thật trên Host
Firebase
Authentication
Authorization
Permission nâng cao
Session history persistent
Activity logs persistent
```

---

# 15. Phase tiếp theo đề xuất

## Phase 5 — Tạo WinForms Host App

Mục tiêu:

```text
Host WinForms kết nối SignalR
Host đăng ký HostId
Host nhận ReceiveControlRequest
Host có nút Accept / Reject
Host chụp màn hình thật
Host gửi SendScreenFrame thật
Host nhận ReceiveMouseEvent
Host nhận ReceiveKeyboardEvent
```

Ở giai đoạn đầu, Host WinForms có thể chỉ log mouse/keyboard event.

Sau đó mới thực thi thật bằng Windows API.

---

## Phase 6 — Tạo WinForms Viewer App

Mục tiêu:

```text
Viewer WinForms kết nối SignalR
Viewer đăng ký ViewerId
Viewer lấy danh sách Host online qua API
Viewer gửi RequestControl
Viewer nhận ReceiveScreenFrame
Viewer hiển thị màn hình bằng PictureBox
Viewer bắt chuột/phím và gửi SendMouseEvent / SendKeyboardEvent
```

---

## Phase 7 — Host thực thi input thật

Mục tiêu:

```text
Host dùng Windows API để di chuyển chuột
Host click chuột
Host scroll
Host gõ phím
```

Các API Windows có thể dùng:

```text
SetCursorPos
SendInput
mouse_event
keybd_event
```

Khuyến nghị dùng:

```text
SendInput
```

---

## Phase 8 — Firebase + Auth

Mục tiêu:

```text
Lưu user
Lưu host list
Lưu permission
Lưu session history
Lưu activity logs
Thêm JWT authentication
Kiểm tra quyền Viewer có được điều khiển Host không
```

Firebase nên lưu:

```text
users
hosts
sessions
permissions
activity_logs
```

Không nên lưu:

```text
screen frame realtime
mouse realtime
keyboard realtime
```
