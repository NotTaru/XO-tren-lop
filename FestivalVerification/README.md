# Kiểm tra bản game — 08/10/2026

- Unity 2022.3.62f3, Built-in Render Pipeline, Windows x64: build thành công.
- EditMode: **25/25 đạt**, gồm judgment 80/160ms, lane sai, một kết quả/nốt, combo/multiplier, accuracy/rank, validator, giữ phím qua countdown và tham chiếu material/texture của tám VFX.
- PlayMode sau khi chuyển material: **3/3 đạt**, gồm menu/pause/countdown/restart, lượt không bấm kết thúc với tất cả Miss, VFX và dữ liệu phiên tái sử dụng.
- Chạy tự động toàn màn 72 giây với timestamp DSP: **136 Perfect, 0 Good, 0 Miss, 0 Bad Input, 46.200 điểm, combo 136, accuracy 100%, S**. Có pause ở khoảng 20,2 giây, resume countdown, chơi lại và về menu. Log không có exception/runtime error. Đây là input tự động để kiểm tra hệ thống, không phải kết quả người chơi hoặc đo độ trễ bàn phím.
- Kiểm tra ảnh render bằng GPU sau khi chuyển shader: menu, gameplay và pause hiển thị đúng chữ tiếng Việt, bốn làn, texture và màu pháo hoa. GPU được Unity nhận diện: NVIDIA GeForce RTX 3060 Laptop GPU, Direct3D 11.

File bằng chứng: `EditMode.xml`, `PlayMode.xml`, `Session.txt`, `Standalone.log`, `Preview/`. Phiên chạy toàn màn kiểm tra logic được thực hiện trước bước đổi shader; bản build sau đổi shader được kiểm tra lại bằng Test Runner và ảnh render đồ họa.

Nhạc hiện dùng Festival Demo tự tạo; chưa có các MP3 Ha Dong/Home trong Assets. Còn cần playtest người thật để cân cảm giác nhịp, offset, độ lớn VFX, âm lượng; chưa có benchmark Profiler ở 30/60/120 FPS hoặc nghiệm thu chart trên các bài nhạc sản xuất trong TDD.

Bản chơi: `Build/Festival/LeHoiPhaoHoa.exe` (giữ cùng thư mục Data, MonoBleedingEdge và DLL). Scene chỉnh sửa: `Assets/Festival/Scenes/FestivalMain.unity`.
