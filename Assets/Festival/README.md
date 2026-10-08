# Lễ hội pháo hoa — Unity 2022.3.62f3

Mở `Assets/Festival/Scenes/FestivalMain.unity`, nhấn Play. Nếu scene chưa được sinh, chọn **Festival > Create missing game assets**, rồi **Festival > Open game scene**.

- D/F/J/K: đánh nốt ở vạch sáng. Esc: pause/resume. Alt-tab tự pause.
- Luyện tập: 24 giây, 20 nốt. Đêm rực rỡ: 72 giây, 136 nốt.
- Perfect ≤80ms, Good ≤160ms; bấm sai làn tăng Bad Input. Combo 10/25/50 mở hệ số ×2/×3/×4 và màn pháo hoa thưởng.
- Cài đặt menu lưu riêng nhạc, game SFX, UI SFX, offset ±200ms và Reduced Effects. Offset dương đánh giá thao tác muộn hơn.
- Accuracy tính trên toàn chart; S/A/B/C ở 95/85/70%. Đạt màn từ 70%. Điểm cao nhất lưu theo khóa phiên bản `festival.best.v1.songId.chartId`.

## Tài nguyên và nội dung

Tích hợp Epic Toon FX thật bằng asset reference: Blue/Green/Red/Yellow theo làn; Purple cho mốc thưởng; StarBurst2D cho Perfect; TinySparkle cho Good; ConfettiBlastRainbow khi đạt màn. Một số material của package tham chiếu shader GUID không có trong project; setup tạo bản sao material/prefab trong Festival với shader particle Built-in riêng, giữ texture, UV, màu và chế độ additive/alpha. Asset gốc không sửa. Pool wrapper tắt ETFXLightFade/ETFXRotation và AudioSource trên instance, reset transform, Light và toàn bộ particle con; giới hạn 12 world VFX + 4 phản hồi ở vạch, 8 audio voice. Reduced Effects giảm world VFX còn 4.

Các MP3 đề cập trong TDD chưa có trong Assets tại lúc triển khai. Nhạc **Festival Demo** được tự tổng hợp, có nhịp gốc 120 BPM và điểm nhấn đoạn finale; chart được soạn riêng cho nhạc này. Đây không phải chart đã nghe/đánh dấu cho Ha Dong hoặc Home. SFX thao tác cũng là âm tổng hợp ngắn để tránh độ trễ MP3 chưa kiểm chứng. Chọn nhạc sản xuất sau, author lại timestamp, rồi thay AudioClip/Chart trong SongDefinition ở Inspector. Không dùng BPM suy ra từ tên file và không tự sinh chart ở runtime.

Làn/UI được dựng từ Unity UI bằng code để không phụ thuộc TMP Essentials chưa import, font LegacyRuntime hỗ trợ nhãn tiếng Việt. Camera orthographic cố định, world particle ở trên trời; hit VFX chuyển từ screen sang world, không đặt particle vào Canvas. Canvas tham chiếu 1600×900 (cùng tỷ lệ 16:9 với TDD), dùng Expand để giữ đủ bốn làn trên các tỷ lệ màn hình.

## Dữ liệu và kiểm thử

Scene, SongDefinition, ChartDefinition và GameplayConfig được tạo bằng editor setup. Các asset được lưu thật, runtime không đọc đường dẫn ngoài project. Setup chỉ tạo chart/scene còn thiếu, không ghi đè chart hoặc scene đã chỉnh. Lịch nhạc dùng DSP double và PlayScheduled; nốt dùng vị trí tuyệt đối, input xử lý trước timeout, countdown resume đóng băng nốt. Offset cũng áp dụng cho timeout để giữ nguyên cửa sổ đánh; khi kết thúc đợi tất cả nốt có kết quả.

Mở **Window > General > Test Runner** chạy EditMode và PlayMode. Các test kiểm tra biên 80/160ms, lane sai, kết quả một lần, milestone/hệ số, accuracy/rank, validator, pause/countdown, restart, phiên không bấm và tái sử dụng hiệu ứng. Thử trực tiếp 30/60/120 FPS, cảm giác nhạc/input, readability và Profiler ở combo 50 trước nghiệm thu; chưa coi FPS 60 là benchmark đã đạt.

Build Windows: File > Build Settings, FestivalMain là scene mặc định; hoặc gọi `FestivalProjectSetup.BuildWindows` bằng Unity batchmode. Game cờ caro và SampleScene vẫn được giữ riêng.
