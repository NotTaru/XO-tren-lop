# TDD — Lễ hội pháo hoa

**Technical Design Document · Phiên bản 1.0 · 08/10/2026**

## 1. Mục tiêu và phạm vi

Game nhịp điệu một người chơi trên PC, phát triển bằng Unity. Người chơi bấm D–F–J–K khi tín hiệu của từng làn chạm vạch đánh. Bấm chính xác tạo pháo hoa; duy trì combo tạo màn trình diễn nhiều lớp. Một lượt chơi kết thúc bằng điểm số, độ chính xác, combo cao nhất và xếp hạng.

Mục tiêu của bản đầu là một vòng chơi hoàn chỉnh, dễ thao tác, thể hiện rõ việc phối hợp VFX và Sound. Dự kiến dùng một đoạn nhạc 60–90 giây, một chart luyện tập và một chart chính. Đây là phạm vi thiết kế, chưa phải nội dung đã được dựng.

**Có trong MVP:** menu, hướng dẫn, đếm ngược, bốn làn, nốt bấm đơn, ba mức đánh giá, combo, điểm, pháo hoa thưởng, pause/resume, kết quả, chơi lại, âm lượng và độ lệch input.

**Ngoài MVP:** nốt giữ, hai nốt cùng thời điểm, multiplayer, chart sinh tự động, đồng bộ tài khoản, mobile, nhân vật di chuyển và hệ thống chiến đấu.

## 2. Hiện trạng tài nguyên

Thư mục hiện tại chứa `ParticalSystem.unitypackage` và `Audio/`; chưa có cấu trúc project Unity hoàn chỉnh hoặc `ProjectVersion.txt`. Chưa xác định phiên bản Unity và chưa chạy thử render.

Gói chứa Epic Toon FX với 158 prefab (bao gồm prefab demo), 80 material, 73 PNG, 6 script C#, 11 WAV, 3 FBX và một scene `etfx_2ddemo`. Không tìm thấy README, PDF, Word hoặc tài liệu hướng dẫn trong bộ file đã kiểm tra.

Các script hiện có chỉ phục vụ chọn/spawn VFX, UI demo, xoay vật thể và giảm ánh sáng. Chưa có hệ thống rhythm, chấm điểm hoặc quản lý combo. Các prefab là hiệu ứng hình ảnh; luật chơi phải được viết riêng.

### 2.1. Bảng sử dụng asset

Các đường dẫn `Assets/Epic Toon FX/...` dưới đây là đường dẫn đích sau khi import gói, chưa tồn tại dưới dạng file rời trong thư mục hiện tại.

| Tài nguyên | Đường dẫn | Vai trò dự kiến |
|---|---|---|
| Pháo hoa xanh | `Assets/Epic Toon FX/Prefabs/Environment/Firework/FireworkBlue.prefab` | Phản hồi làn D |
| Pháo hoa xanh lá | `Assets/Epic Toon FX/Prefabs/Environment/Firework/FireworkGreen.prefab` | Phản hồi làn F |
| Pháo hoa đỏ | `Assets/Epic Toon FX/Prefabs/Environment/Firework/FireworkRed.prefab` | Phản hồi làn J |
| Pháo hoa vàng | `Assets/Epic Toon FX/Prefabs/Environment/Firework/FireworkYellow.prefab` | Phản hồi làn K |
| Pháo hoa tím | `Assets/Epic Toon FX/Prefabs/Environment/Firework/FireworkPurple.prefab` | Điểm nhấn trong màn thưởng combo |
| Confetti cầu vồng | `Assets/Epic Toon FX/Prefabs/Environment/Confetti/Blast/ConfettiBlastRainbow.prefab` | Thắng màn và kết quả hạng cao |
| Sao nổ 2D | `Assets/Epic Toon FX/Prefabs 2D/Explosions/StarBurst2D.prefab` | Perfect tại vị trí vạch đánh |
| Lấp lánh | `Assets/Epic Toon FX/Prefabs/Interactive/Sparkle/TinySparkle.prefab` | Trang trí nhẹ và phản hồi Good |
| Nốt nhạc | `Assets/Epic Toon FX/Prefabs/Misc/MusicalNotes.prefab` | Trang trí menu, ngoài vùng đọc tín hiệu |
| SFX thao tác | `Audio/SFX/button click UI.mp3` | Nút menu |
| SFX bấm trong game | `Audio/SFX/button click Game.mp3` | Phản hồi đánh nốt, cần kiểm tra độ trễ |
| SFX thưởng | `Audio/SFX/get reward.mp3` | Đạt mốc combo |
| SFX chuyển màn | `Audio/SFX/next level.mp3` | Chọn màn tiếp theo |
| SFX kết quả | `Audio/SFX/win.mp3`, `Audio/SFX/failed.mp3` | Kết quả đạt/chưa đạt |
| Nhạc nền | `Audio/BG/Ha Dong - Cherry Save File.mp3` | Ứng viên nhạc màn chơi |
| Nhạc nền | `Audio/BG/Home - Pale Umbrella.mp3` | Ứng viên nhạc menu |
| Nhạc nền | `Audio/BG/Ha Dong - Glass Petal Skies 1.mp3` | Ứng viên màn bổ sung |
| Âm thanh hiệu ứng | `Assets/Epic Toon FX/Sound/etfx_explosion_minimagic.wav`, `etfx_explosion_magic2.wav` trong cùng thư mục | Ứng viên tiếng pháo hoa, cần nghe và cân âm lượng |

Phân vai nhạc và WAV là đề xuất theo thiết kế. Chưa nghe hoặc đo BPM, độ dài, mức âm lượng của từng file. Không suy ra nhịp chỉ từ tên bài.

### 2.2. Phần cần tự dựng

- UI: làn, nốt, vạch đánh, nhãn phím, điểm, combo, thanh tiến trình, menu và màn kết quả. Dùng Unity UI và TextMeshPro với hình chữ nhật/tròn đơn giản.
- Sân khấu: nền trời tối, mặt đất và các điểm phóng pháo hoa. Dùng primitive hoặc sprite tự dựng; không cần nhân vật.
- Chart nhạc: thời điểm từng nốt và làn tương ứng; tạo thủ công sau khi nghe nhạc.
- Code gameplay, cấu hình, pooling và quản lý âm thanh.

## 3. Công nghệ và bố cục

Chọn một phiên bản Unity LTS sẵn có trên máy khi bắt đầu triển khai và cố định phiên bản đó cho project. Ưu tiên Built-in Render Pipeline cho bản thử đầu, sau đó kiểm tra material Epic Toon FX thực tế. Nếu dùng URP, phải kiểm tra/chuyển shader và xác nhận lại VFX trước khi xây gameplay.

Game trình bày dưới dạng sân khấu 3D với camera cố định và UI bốn làn phía dưới. Pháo hoa chính xuất hiện ở phần trời phía trên. Canvas dùng Screen Space Overlay; vùng đọc nốt luôn nằm trên hiệu ứng thế giới. Hiệu ứng ở vạch đánh dùng vị trí world được chuyển từ tọa độ UI qua một mặt phẳng riêng, không gắn trực tiếp prefab ParticleSystem vào Canvas.

Độ phân giải tham chiếu 1920 × 1080; Canvas Scaler dùng Scale With Screen Size. Khi đổi tỉ lệ màn hình, bốn làn phải còn đủ chiều rộng và nhãn phím phải đọc được.

## 4. Luật chơi

### 4.1. Vòng chơi

`Menu → Chọn màn → Chuẩn bị → Đếm ngược → Playing → Results → Chơi lại/Menu`.

Trong Playing, Esc chuyển sang Paused. Khi resume, có đếm ngược ngắn trước khi nhạc và tín hiệu cùng chạy tiếp. Mất focus tự động pause.

Chart luyện tập giải thích bốn phím bằng các nốt thưa. Chart chính bắt đầu chậm, tăng mật độ vừa phải và dành đoạn kết cho màn pháo hoa thưởng.

### 4.2. Đánh giá input

Mỗi nốt có `hitTimeSeconds` trên timeline nhạc và `lane` từ 0 đến 3. Độ lệch:

`delta = inputSongTime + inputOffsetSeconds - hitTimeSeconds`.

`inputOffsetSeconds > 0` nghĩa là thời điểm bấm được đánh giá muộn hơn. Giá trị mặc định bằng 0; UI cài đặt phải giải thích dấu này rõ ràng.

| Điều kiện | Kết quả | Điểm cơ bản | Combo |
|---|---|---:|---|
| `abs(delta) <= 0.080s` | Perfect | 100 | +1 |
| `0.080s < abs(delta) <= 0.160s` | Good | 60 | +1 |
| Không đánh được trước khi quá `hitTime + 0.160s` | Miss | 0 | Về 0 |
| Bấm sai làn hoặc không có nốt trong cửa sổ | Bad Input | 0 | Về 0 |

Bad Input được thống kê riêng, không tiêu thụ nốt ở làn khác. Không bấm sai nhiều lần do giữ phím: chỉ xử lý cạnh nhấn xuống, không xử lý key repeat. Giữ phím trước khi nốt đến không tự đánh nốt.

Mỗi input chọn nốt Pending sớm nhất của chính làn đó, nếu nằm trong cửa sổ. Mỗi nốt chỉ có một kết quả cuối. Xử lý input trước, rồi mới xét nốt quá hạn trong cùng frame. Biên 160ms thuộc Good; chỉ quá biên mới Miss.

MVP cấm các cửa sổ nốt cùng làn chồng lên nhau: khoảng cách giữa hai nốt cùng làn phải lớn hơn 320ms. Chart không có chord; các nốt khác làn vẫn có thể đủ gần để tạo chuỗi nhanh.

### 4.3. Combo và điểm

Multiplier tính theo combo sau khi cộng nốt vừa đánh:

| Combo | Multiplier | Trình diễn |
|---:|---:|---|
| 1–9 | ×1 | Một pháo hoa nhỏ theo màu làn |
| 10–24 | ×2 | Pháo hoa lớn hơn; mốc 10 thưởng ba điểm phóng |
| 25–49 | ×3 | Mốc 25 thưởng năm điểm phóng, có màu tím |
| ≥50 | ×4 | Mốc 50 thưởng Grand Finale tối đa tám điểm phóng |

`score += baseScore × multiplier`.

Mốc thưởng chỉ phát một lần khi combo đi qua 10, 25 hoặc 50 trong một chuỗi liên tục. Khi combo đứt, có thể đạt lại mốc ở chuỗi mới. Không phát Grand Finale ở từng nốt sau combo 50. Hiệu ứng không thay đổi thời điểm nốt hoặc cửa sổ đánh giá.

Accuracy: `(PerfectCount + 0.6 × GoodCount) / TotalNotes × 100%`. Bad Input không đổi mẫu số hoặc trực tiếp trừ accuracy; nó giảm combo, điểm multiplier và được hiển thị riêng để thấy hành vi bấm thừa.

Xếp hạng: S ≥95%, A ≥85%, B ≥70%, C <70%. Màn đạt khi accuracy ≥70%. Người chơi luôn chơi đến hết màn; không có thanh máu hoặc game over giữa bài. Đây là các giá trị thiết kế ban đầu, điều chỉnh sau playtest.

## 5. Kiến trúc phần mềm

| Thành phần | Trách nhiệm |
|---|---|
| `GameFlowController` | State Menu, Countdown, Playing, Paused, Results; bắt đầu/kết thúc phiên |
| `SongClock` | Timeline dựa trên DSP, lịch nhạc, pause/resume và mốc kết thúc |
| `ChartRunner` | Khởi tạo dữ liệu nốt runtime, spawn view, xử lý nốt quá hạn |
| `InputRouter` | Ánh xạ D/F/J/K, thu cạnh nhấn xuống, chuyển timestamp cho judge |
| `NoteJudge` | Chọn nốt hợp lệ, trả Perfect/Good/Miss/BadInput, không điều khiển VFX |
| `ScoreManager` | Điểm, combo, multiplier, accuracy, max combo và thống kê |
| `FireworkDirector` | Ánh xạ làn sang VFX, trình diễn mốc combo, giới hạn tải |
| `AudioManager` | Music, gameplay SFX, UI SFX, âm lượng và số voice |
| `NoteViewPool` | Tái sử dụng GameObject nốt; cập nhật vị trí theo timeline |
| `VfxPool` | Tái sử dụng prefab hiệu ứng và trả đối tượng về pool |
| `HudController` | Điểm, combo, judgment, tiến trình và nhãn phím |
| `ResultsController` | Kết quả và lựa chọn chơi lại/menu |

Luồng chính: `InputRouter → NoteJudge → JudgmentResult → ScoreManager → HUD/FireworkDirector/AudioManager`.

`ChartRunner` gửi Miss qua cùng đường kết quả. Controller điều phối bằng reference được gán qua Inspector; không dùng `Find` trong vòng Update. Hủy đăng ký event khi disable để chơi lại không nhận kết quả hai lần.

### 5.1. Dữ liệu

**`SongDefinition : ScriptableObject`**

- `songId`, `displayName`, `AudioClip`, `ChartDefinition`.
- `audioStartOffsetSeconds`: vị trí bắt đầu đoạn nhạc trong clip.
- `chartDurationSeconds`: độ dài phần chơi tính từ điểm bắt đầu đoạn nhạc.
- `previewStartSeconds`: đoạn nghe thử trong menu, tùy chọn.

**`ChartDefinition : ScriptableObject`**

- `chartId`, `difficulty`, danh sách `NoteData` theo thời gian tăng dần.
- `NoteData`: `id`, `lane`, `hitTimeSeconds`.
- Thời điểm nốt tính bằng giây từ đầu đoạn chơi; BPM chỉ là công cụ hỗ trợ authoring nếu đã đo được.

**`GameplayConfig : ScriptableObject`**

- Cửa sổ Perfect/Good, điểm cơ bản, mốc combo, multiplier.
- `noteTravelSeconds = 2.0`, `inputOffsetSeconds`, thời gian countdown.
- Danh sách prefab theo làn, vị trí phóng, giới hạn hiệu ứng và audio voice.

**Dữ liệu runtime** tách khỏi ScriptableObject: mỗi nốt có trạng thái Pending/Perfect/Good/Miss; `SessionResult` chứa điểm và bộ đếm. Không sửa chart gốc khi chơi.

**Lưu cục bộ:** PlayerPrefs cho âm lượng, offset và best score theo `songId + chartId`. Không lưu dữ liệu phiên đang chơi. Khi cập nhật chart hoặc công thức điểm, thay phiên bản khóa best score.

### 5.2. Kiểm tra chart trước khi chơi

- Có clip, chart và ít nhất một nốt; ID không trùng; lane thuộc 0–3.
- Tất cả timestamp hữu hạn, không âm, tăng dần và nằm trong đoạn nhạc.
- Nốt cuối có đủ 160ms để đánh giá trước điểm kết thúc gameplay.
- Đoạn nhạc không vượt quá `AudioClip.length`; offset clip không âm.
- Không có chord và không có cửa sổ chồng lấn trong cùng làn.
- Nếu không hợp lệ, không chạy phiên; báo lỗi rõ trong Editor và UI hiển thị màn chưa sẵn sàng.

## 6. Đồng bộ thời gian và chuyển động

Timeline dùng `AudioSettings.dspTime` kiểu double; không cộng dồn `Time.deltaTime` để quyết định judgment.

Khi chuẩn bị, lên lịch nhạc bằng `AudioSource.PlayScheduled(dspStartTime)`. Đặt playback clip tại `audioStartOffsetSeconds` trước khi schedule. Countdown cung cấp đủ thời gian để các nốt đầu chạy vào làn.

Trong phiên đang chạy:

`songTime = resumeSongTime + (AudioSettings.dspTime - resumeDspTime)`.

Phiên đầu có `resumeSongTime = 0` tại thời điểm nhạc bắt đầu. Trong countdown, dùng timeline âm để nốt đầu có thể xuất hiện từ `-noteTravelSeconds`.

Vị trí nốt là hàm tuyệt đối của timeline:

`progress = 1 - (hitTimeSeconds - songTime) / noteTravelSeconds`.

Nốt spawn khi `songTime >= hitTimeSeconds - noteTravelSeconds`; progress 0 ở đầu làn và 1 tại vạch đánh. Nốt trễ tiếp tục vượt vạch đến khi bị đánh hoặc Miss. Frame rate thấp không được làm nốt chạy chậm hoặc đổi tốc độ bài.

MVP lấy timestamp DSP khi polling cạnh nhấn trong Update; sai số vẫn phụ thuộc frame và thiết bị. Nếu cần độ chính xác cao hơn, dùng input event timestamp và chuyển về cùng clock, không trộn trực tiếp hai hệ thời gian.

### 6.1. Pause/resume

Khi pause: lưu `frozenSongTime`, vị trí sample âm thanh; dừng lịch gameplay, nhạc và VFX đang hoạt động. Không ghi Miss trong pause.

Khi resume: giữ gameplay đóng băng trong countdown; đặt lại vị trí sample, lên lịch phát mới, rồi đặt `resumeSongTime = frozenSongTime` và `resumeDspTime` bằng thời điểm lịch phát mới. Clock và input chỉ mở lại tại mốc đó; loại bỏ input phát sinh trong pause/countdown.

Nốt đang nằm trong cửa sổ đánh trước pause vẫn giữ trạng thái; không spawn bản sao. Restart hủy lịch nhạc, các coroutine thưởng, active VFX và dữ liệu phiên cũ trước khi bắt đầu countdown mới.

## 7. Điều khiển VFX

Tạo wrapper `PooledEffect` cho từng prefab, lưu toàn bộ ParticleSystem con. Khi lấy từ pool: reset transform, scale, thời gian, clear hạt cũ và Play. Khi hoàn tất: Stop/Clear rồi trả về pool.

Kiểm tra duration, loop, delay và AudioSource của từng prefab sau import. Hiệu ứng one-shot trả về pool khi toàn bộ ParticleSystem con đã kết thúc. Prefab loop phải có thời gian dừng cấu hình riêng. Pool không dựa duy nhất vào `main.duration`, vì hạt con có thể sống lâu hơn.

`ETFXLightFade` hiện có thể destroy component Light; không đưa prefab có hành vi đó vào pool nếu chưa sửa wrapper/script để khôi phục trạng thái. Reset những thuộc tính thay đổi sau mỗi lần sử dụng.

Perfect phát pháo hoa màu làn và StarBurst tại vạch; Good phát pháo hoa nhỏ hơn; Miss/Bad Input chỉ hiện phản hồi UI dịu, không dùng hiệu ứng nổ lớn. Pháo hoa tại vạch phải ngắn và không che nhãn phím/nốt tiếp theo.

Grand Finale chia tối đa tám điểm phóng thành các đợt cách nhau khoảng 0.12s. Mức scale ban đầu: thường ×1, mốc 10 ×1.2, mốc 25 ×1.4, mốc 50 ×1.6; chỉ chốt sau khi kiểm tra prefab thực tế.

Giới hạn khởi điểm: tối đa 12 instance pháo hoa thế giới và 4 phản hồi tại vạch cùng lúc. Khi đầy, bỏ hiệu ứng phụ hoặc giảm số điểm phóng; luôn giữ kết quả gameplay. Có cấu hình Reduced Effects giảm số điểm phóng và tắt confetti trong lúc chơi.

## 8. Âm thanh

Tách ba nhóm Music, Gameplay SFX và UI SFX để chỉnh độc lập. Các AudioSource phản hồi dùng 2D, không phụ thuộc khoảng cách camera.

Nhạc có thể dùng Streaming; clip SFX ngắn ưu tiên preload và Decompress On Load sau khi kiểm tra dung lượng. Với MP3 phản hồi đánh nốt, kiểm tra khoảng lặng đầu file và độ trễ; nếu nghe trễ, tạo bản WAV/trim riêng dùng trong project, giữ file gốc.

Âm click đánh nốt cần nhẹ để không lấn nhạc. Tiếng pháo hoa dùng cho mốc thưởng hoặc phát có giới hạn; không phát một tiếng nổ lớn cho mọi nốt. Nếu prefab đã có AudioSource, tắt hoặc route đúng mixer để tránh trùng âm với AudioManager.

Giới hạn khởi điểm: 8 voice gameplay SFX đồng thời. Hiệu ứng thưởng có ưu tiên cao hơn tiếng trang trí. Offset input xử lý đánh giá thao tác; nếu hình và nhạc lệch trên thiết bị, hiệu chỉnh visual offset riêng thay vì đổi dữ liệu chart.

## 9. Scene và tổ chức project

MVP dùng một scene `FestivalMain` với panel menu/gameplay/pause/results. Scene demo Epic Toon FX giữ riêng, không đưa vào luồng chơi hoặc gọi các nút demo tải scene thiếu.

```text
Assets/
  Epic Toon FX/               # Asset gốc sau import
  Festival/
    Scenes/FestivalMain.unity
    Scripts/Core/
    Scripts/Rhythm/
    Scripts/Audio/
    Scripts/VFX/
    Scripts/UI/
    Data/Songs/
    Data/Charts/
    Data/Config/
    Prefabs/UI/
    Prefabs/VFXWrappers/
    Audio/BG/
    Audio/SFX/
    Tests/EditMode/
    Tests/PlayMode/
```

Hierarchy gameplay gồm Main Camera, AudioManager, GameFlow, RhythmSystems, FireworkStage với các SpawnPoint, Pools và Canvas. Dữ liệu dùng asset reference; không hardcode đường dẫn file âm thanh khi runtime.

## 10. Hiệu năng và xử lý lỗi

Mục tiêu ban đầu 60 FPS ở 1080p trên máy phát triển; ghi lại cấu hình máy khi nghiệm thu. Chưa có benchmark xác nhận.

- Prewarm pool trong chuẩn bị; gameplay không Instantiate/Destroy mỗi nốt.
- Mỗi làn có queue/index nốt Pending; không duyệt toàn chart cho mỗi input.
- Chỉ cập nhật các view đang hiện; HUD điểm/combo cập nhật theo event.
- Theo dõi Profiler ở Grand Finale, đặc biệt particle overdraw, số audio voice và GC allocation.
- Thiếu VFX: vẫn chấm điểm, dùng flash UI thay thế và báo cảnh báo một lần.
- Thiếu clip/chart: chặn bắt đầu, hiện thông báo màn chưa sẵn sàng.
- Nhạc kết thúc bất thường: dừng phiên có kiểm soát; không tiếp tục chạy nốt im lặng.
- Results chỉ mở khi mọi nốt đã có kết quả và đoạn gameplay kết thúc; nhạc có thể fade out ngắn.

## 11. Kế hoạch kiểm thử và nghiệm thu

### 11.1. EditMode — logic thuần

1. Delta đúng 80ms là Perfect; trên 80ms và tới 160ms là Good; trên 160ms không đánh được.
2. Input sai làn không tiêu thụ nốt đúng; Bad Input reset combo và tăng bộ đếm một lần.
3. Mỗi nốt chỉ nhận một kết quả; input và timeout cùng frame không cộng điểm hai lần.
4. Combo 9→10, 24→25, 49→50 đổi multiplier và phát đúng một sự kiện thưởng.
5. Combo đứt rồi đạt lại mốc phát thưởng mới; nốt 51 không phát lại Grand Finale.
6. Accuracy, score, max combo và xếp hạng đúng với một danh sách kết quả biết trước.
7. Validator từ chối lane sai, ID trùng, timestamp ngoài clip, chord và cửa sổ cùng làn chồng nhau.

### 11.2. PlayMode và kiểm tra trực tiếp

| Tình huống | Kết quả cần đạt |
|---|---|
| Chơi toàn bài không bấm | Tất cả nốt Miss, accuracy 0%, vẫn tới Results |
| Giữ một phím | Không tự đánh chuỗi nốt |
| Bấm nhiều phím sai | Không lấy điểm từ làn khác; thống kê Bad Input đúng |
| Pause ngay trước nốt | Không có Miss khi pause; resume giữ đúng nhạc và vị trí nốt |
| Alt-tab | Tự pause, không mất nốt do mất focus |
| Restart nhiều lần | Không trùng nốt, event, nhạc hoặc coroutine thưởng |
| Đạt combo 50 | Màn thưởng xuất hiện, các nốt tiếp theo vẫn đọc được |
| VFX được tái sử dụng | Không còn hạt cũ, scale sai hoặc mất Light ngoài dự kiến |
| Giới hạn VFX/audio | Gameplay và điểm không đổi khi bỏ hiệu ứng phụ |
| Chạy 30/60/120 FPS | Thời điểm chart không tích lũy sai lệch theo frame rate |
| Chỉnh volume/offset rồi mở lại | Giá trị lưu và áp dụng đúng |

Tiêu chí hoàn thành MVP: có một chart luyện tập và một chart chính đã nghe/đánh dấu, vòng menu→chơi→kết quả→chơi lại hoạt động, judgment và combo qua kiểm thử, VFX/SFX đúng sự kiện, pause không làm trôi nhạc, UI không bị pháo hoa che và không có lỗi runtime ở lượt chơi hoàn chỉnh.

## 12. Thứ tự triển khai

1. **Xác nhận asset:** tạo project Unity, import package, xem năm prefab Firework, kiểm tra shader/audio/loop; nghe các clip và chọn đoạn nhạc.
2. **Prototype rhythm:** bốn làn với hình đơn giản, clock DSP, chart thử, input, judgment và Miss; xác nhận cảm giác bấm trước khi thêm hiệu ứng.
3. **Gameplay đầy đủ:** điểm, combo, multiplier, state, pause/resume/restart, HUD và Results.
4. **Tích hợp tài nguyên:** VFX pool, màu theo làn, thưởng combo, mixer và SFX; kiểm tra tải ở Grand Finale.
5. **Nội dung và hoàn thiện:** author chart luyện tập/chart chính, cân cửa sổ đánh, cài đặt offset/volume, test, build PC.

## 13. Các điểm cần xác nhận khi bắt đầu lập trình

- Phiên bản Unity và render pipeline thực tế.
- Bài nhạc, đoạn dùng, BPM nếu có và timestamp nốt sau khi nghe.
- Prefab có loop, AudioSource, LightFade hoặc shader cần chuyển đổi hay không.
- Mức âm lượng, khoảng lặng đầu SFX và cảm giác cửa sổ 80/160ms trên máy chạy.
- Máy mục tiêu và mức giảm VFX cần thiết.

Các điểm này không ngăn viết prototype bằng hình đơn giản; chúng phải được giải quyết trước nghiệm thu bản có nhạc và hiệu ứng hoàn chỉnh.
