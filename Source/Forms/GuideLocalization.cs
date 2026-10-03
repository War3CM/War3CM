using System;
using System.Collections.Generic;

namespace Phanmemwar3.Forms
{
    public class GuideStepInfo
    {
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
    }

    public class GuideContent
    {
        public string BannerTitle { get; set; } = "";
        public string BannerSubtitle { get; set; } = "";
        public string Step1Title { get; set; } = "";
        public string Step1Desc { get; set; } = "";
        public string BtnCardDrive { get; set; } = "";
        public string BtnCardCopy { get; set; } = "";
        public string CommTitle { get; set; } = "";
        public string CommDesc { get; set; } = "";
        public string BtnDiscord { get; set; } = "";
        public string BtnYouTube { get; set; } = "";
        public string WarnTitle { get; set; } = "";
        public string WarnBody { get; set; } = "";
        public GuideStepInfo Step2 { get; set; } = new();
        public GuideStepInfo Step3 { get; set; } = new();
        public GuideStepInfo Step4 { get; set; } = new();
        public GuideStepInfo Step5 { get; set; } = new();
        public GuideStepInfo Step6 { get; set; } = new();
        public GuideStepInfo Step7 { get; set; } = new();
        public GuideStepInfo Step8 { get; set; } = new();
    }

    public static class GuideLocalization
    {
        public const string DRIVE_URL = "https://drive.google.com/drive/folders/1BbGpDIKfuIFfC2PXpDfE18QDmPQxarAQ?usp=sharing";

        private static readonly Dictionary<string, GuideContent> _contents = new(StringComparer.OrdinalIgnoreCase)
        {
            ["EN"] = new GuideContent
            {
                BannerTitle = "📖 Warcraft Platform - Setup & Quick Start Guide",
                BannerSubtitle = "Step-by-step instructions for clean game setup, plugin updates, VIP save loading, Map Level, and automatic progress sync.",
                Step1Title = "1. Download Clean Game (Warcraft_1.27.5_clean.zip) & Install aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Download Warcraft_1.27.5_clean.zip and aio-runtimes_v2.4.9.exe from the Google Drive link below.\n" +
                            "• IMPORTANT: Run and install aio-runtimes_v2.4.9.exe first to install all essential Visual C++ runtimes (2005-2022) and DirectX. This prevents missing DLL crashes when launching Warcraft or loading KKWE plugins.\n" +
                            "• Google Drive: " + DRIVE_URL,
                BtnCardDrive = "🌐 Open Google Drive Link",
                BtnCardCopy = "📋 Copy Download Link",
                CommTitle = "💬 Join Community & Video Channel (Discord & YouTube)",
                CommDesc = "Connect with our active Warcraft RPG community, share maps, find teammates, and watch tutorials:",
                BtnDiscord = "💬 Join Discord Server",
                BtnYouTube = "▶ Watch YouTube Channel",
                WarnTitle = "⚠️ CRITICAL WARNING: Map File Name Length",
                WarnBody = "The Warcraft III engine has a strict 54-byte internal buffer limit on map launch paths!\n" +
                           "• Never use overly long map filenames or deep subfolder paths.\n" +
                           "• Recommended: Rename your map to a short name before playing (e.g. kiemthe.w3x).\n" +
                           "• Overly long names WILL crash Warcraft III or fail to sync save slot data!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Extract Clean Warcraft 1.27.5",
                    Body = "Extract the zip file to any folder on your computer (e.g. D:\\Warcraft_1.27.5 or C:\\Games\\Warcraft3).\n" +
                           "💡 Tip: Avoid non-ASCII characters or accented paths to prevent plugin loading issues."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Copy Maps into the Maps Directory",
                    Body = "Copy custom RPG map files (.w3x or .w3m) into the Maps directory inside your game folder (e.g. Maps\\Download\\).\n" +
                           "💡 Reminder: Always keep map filenames short and clean."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Set Game Library Path in Launcher",
                    Body = "Open Warcraft Platform Manager:\n" +
                           "• In 'Select War3 Folder', click 'Browse...' and choose the folder containing war3.exe.\n" +
                           "• Click '↗' to quickly open and verify the target game directory."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Update & Select Latest KKWE Plugin",
                    Body = "• Click 'Updates' in the top header to fetch and install the latest KKWE plugins.\n" +
                           "• In 'Plugin Version', select the newest profile (detected e.g. KKWE 2.0.12.2606).\n" +
                           "• Optional: Click 'YDWE' to fine-tune advanced plugin settings."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Save Profiles, Load Save (VIP), Clean Data, Map Level (1-100) & Rank 1",
                    Body = "• Select your map from the dropdown (or drag & drop map files directly onto the launcher).\n" +
                           "• Create & Delete: Click '+ New' to create an isolated save for each character; click 'Delete' to remove unwanted slots.\n" +
                           "• Load Save (Import VIP Save): Click 'Load Save' to pick any external save file (.ini, .txt, VIP shared saves), enter a new profile name, and immediately use it in the selected map.\n" +
                           "• Clean Data: Click 'Clean Data' to selectively clean temporary game files accumulated during play (staged map cache in Maps\\WPM, save trash & history snapshots, game error logs...) to reclaim disk space.\n" +
                           "• Map Level (1-100): Enter your desired level (1 to 100) in the input box and click 'Set' (or press Enter) to apply it across DzAPI and KKAPI standards.\n" +
                           "• Toggle Rank 1: Click 'Rank 1' (turns Amber Gold when active) to enable top-rank perks and leaderboard privileges in supported maps."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Tune In-Game Options",
                    Body = "Click '⚙ IN-GAME OPTIONS':\n" +
                           "• Set your in-game User Name for multiplayer and save identification.\n" +
                           "• Enable 'Lock Mouse' to keep cursor contained inside windowed mode.\n" +
                           "• Enable 'WideScreen 16:9' and 'Fix Aspect Ratio' for optimal visuals.\n" +
                           "• Enable 'Fast Load' and 'Mute Save Value' to block annoying score popups."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Launch Game & Automatic Sync (Play Map / Play War3)",
                    Body = "• Choose Graphics (OpenGL / DirectX) and Display Mode (Full Screen / Borderless / Windowed).\n" +
                           "• Play Map: Directly launches the selected map with your active save slot mounted. Progress automatically syncs when exiting game.\n" +
                           "• Play War3: Launches Warcraft III with all active plugins directly to Main Menu for LAN games or settings."
                }
            },

            ["RU"] = new GuideContent
            {
                BannerTitle = "📖 Платформа Warcraft - Руководство по установке и запуску",
                BannerSubtitle = "Пошаговая инструкция по настройке чистой игры, обновлению плагинов, загрузке VIP-сохранений, уровню карты и автосинхронизации.",
                Step1Title = "1. Скачайте чистую версию (Warcraft_1.27.5_clean.zip) и установите aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Скачайте Warcraft_1.27.5_clean.zip и aio-runtimes_v2.4.9.exe по ссылке на Google Drive ниже.\n" +
                            "• ВАЖНО: Сначала установите aio-runtimes_v2.4.9.exe для установки всех необходимых библиотек Visual C++ (2005-2022) и DirectX. Это предотвратит вылеты из-за отсутствия DLL при запуске Warcraft и плагинов KKWE.\n" +
                            "• Ссылка на Google Drive: " + DRIVE_URL,
                BtnCardDrive = "🌐 Открыть Google Drive",
                BtnCardCopy = "📋 Скопировать ссылку",
                CommTitle = "💬 Официальное сообщество и видеоканал (Discord и YouTube)",
                CommDesc = "Присоединяйтесь к нашему активному сообществу игроков Warcraft RPG, делитесь картами, находите союзников и смотрите обучающие видео:",
                BtnDiscord = "💬 Сервер Discord",
                BtnYouTube = "▶ Канал на YouTube",
                WarnTitle = "⚠️ ВАЖНОЕ ПРЕДУПРЕЖДЕНИЕ: Длина пути и имени файла карты",
                WarnBody = "Движок Warcraft III имеет жесткое ограничение внутреннего буфера в 54 байта для путей запуска карт!\n" +
                           "• Не используйте слишком длинные имена файлов карт или глубокие подпапки.\n" +
                           "• Рекомендация: перед игрой переименуйте файл карты в короткое имя (например: map.w3x).\n" +
                           "• Слишком длинные имена неизбежно приведут к вылету Warcraft III (Crash) или ошибке синхронизации сохранений!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Распакуйте чистую версию Warcraft 1.27.5",
                    Body = "Распакуйте архив в любую папку на вашем компьютере (например: D:\\Warcraft_1.27.5 или C:\\Games\\Warcraft3).\n" +
                           "💡 Совет: не используйте русские буквы, спецсимволы и длинные пути, чтобы избежать сбоев при загрузке плагинов."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Скопируйте карты в папку Maps",
                    Body = "Скопируйте файлы пользовательских RPG-карт (.w3x или .w3m) в папку Maps внутри каталога игры (например: Maps\\Download\\).\n" +
                           "💡 Напоминание: всегда сохраняйте имена файлов карт короткими и понятными."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Укажите путь к папке игры в лаунчере",
                    Body = "Запустите Warcraft Platform Manager:\n" +
                           "• В поле 'Папка Warcraft 3' нажмите 'Обзор...' и выберите каталог, содержащий war3.exe.\n" +
                           "• Нажмите кнопку '↗', чтобы быстро открыть и проверить целевую папку игры."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Обновите и выберите актуальный плагин KKWE",
                    Body = "• Нажмите 'Обновления' в верхней панели, чтобы загрузить и установить свежие версии плагинов KKWE.\n" +
                           "• В списке 'Версия плагина' выберите актуальный профиль (например: KKWE 2.0.12.2606).\n" +
                           "• При необходимости нажмите 'YDWE' для расширенной настройки параметров."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Профили сохранений, Load Save (VIP), Clean Data, уровень карты (1-100) и Rank 1",
                    Body = "• Выберите карту из списка (или перетащите файл карты прямо в окно лаунчера).\n" +
                           "• Создание и удаление: нажмите '+ Новый' для создания отдельного сохранения; нажмите 'Удалить' для очистки ненужных слотов.\n" +
                           "• Загрузка сохранения (Load Save / VIP): нажмите 'Load Save', выберите любой внешний файл сохранения (.ini, .txt, VIP-сохранения), введите новое имя и сразу используйте его на карте.\n" +
                           "• Очистка данных (Clean Data): нажмите 'Clean Data' для удаления временных файлов (кэш карт Maps\\WPM, корзина _Trash, снимки _History, логи сбоев) и освобождения места на диске.\n" +
                           "• Уровень карты (Map Level 1-100): введите желаемый уровень (от 1 до 100) и нажмите 'Lvl' (или Enter) для записи в форматы DzAPI и KKAPI.\n" +
                           "• Переключатель Rank 1: нажмите 'Rank 1' (подсвечивается золотым при активации) для разблокировки наград и привилегий первого места в таблице лидеров."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Внутриигровые настройки (IN-GAME OPTIONS)",
                    Body = "Нажмите кнопку '⚙ ВНУТРИИГРОВЫЕ ОПЦИИ':\n" +
                           "• Укажите игровое имя (User Name) для сетевой игры и привязки сохранений.\n" +
                           "• Включите 'Захват мыши', чтобы курсор не вылетал за пределы окна в оконном режиме.\n" +
                           "• Включите 'Широкий экран 16:9' и 'Исправление пропорций' для четкого изображения.\n" +
                           "• Включите 'Быструю загрузку' и 'Заглушить очки сохранения' для отключения всплывающего спама."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Запуск игры и автоматическая синхронизация (Play Map / Play War3)",
                    Body = "• Выберите графический API (OpenGL / DirectX) и режим экрана (Полноэкранный / Без рамок / Оконный).\n" +
                           "• Play Map (Запуск карты): мгновенно запускает выбранную карту с активным профилем сохранения. Прогресс автоматически сохраняется при выходе из игры!\n" +
                           "• Play War3 (В меню): запускает Warcraft III со всеми активными плагинами в главное меню для игры по локальной сети или настроек."
                }
            },

            ["DE"] = new GuideContent
            {
                BannerTitle = "📖 Warcraft Plattform - Anleitung & Schnellstart",
                BannerSubtitle = "Schritt-für-Schritt-Anleitung für sauberes Spiel, Plugin-Updates, Laden von VIP-Spielständen, Map-Level und automatische Synchronisierung.",
                Step1Title = "1. Saubere Version herunterladen (Warcraft_1.27.5_clean.zip) & aio-runtimes_v2.4.9.exe installieren",
                Step1Desc = "• Laden Sie Warcraft_1.27.5_clean.zip und aio-runtimes_v2.4.9.exe über den folgenden Google Drive-Link herunter.\n" +
                            "• WICHTIG: Führen Sie zuerst aio-runtimes_v2.4.9.exe aus, um alle Visual C++ Runtimes (2005-2022) und DirectX zu installieren. Dies verhindert DLL-Fehler beim Starten von Warcraft oder KKWE-Plugins.\n" +
                            "• Google Drive-Link: " + DRIVE_URL,
                BtnCardDrive = "🌐 Google Drive öffnen",
                BtnCardCopy = "📋 Link kopieren",
                CommTitle = "💬 Community & Videokanal beitreten (Discord & YouTube)",
                CommDesc = "Treten Sie unserer aktiven Warcraft-RPG-Community bei, teilen Sie Maps, finden Sie Mitspieler und sehen Sie Videoanleitungen:",
                BtnDiscord = "💬 Discord beitreten",
                BtnYouTube = "▶ YouTube-Kanal ansehen",
                WarnTitle = "⚠️ WICHTIGE WARNUNG: Länge des Kartendateinamens",
                WarnBody = "Die Warcraft III-Engine besitzt ein striktes internes Pufferlimit von 54 Bytes für Kartenstartpfade!\n" +
                           "• Verwenden Sie niemals übermäßig lange Kartendateinamen oder tiefe Unterordner.\n" +
                           "• Empfohlen: Benennen Sie Ihre Karte vor dem Spielen kurz um (z. B. map.w3x).\n" +
                           "• Zu lange Namen führen zum Absturz von Warcraft III oder verhindern die Speichersynchronisation!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Sauberes Warcraft 1.27.5 entpacken",
                    Body = "Entpacken Sie das Archiv in einen beliebigen Ordner (z. B. D:\\Warcraft_1.27.5 oder C:\\Games\\Warcraft3).\n" +
                           "💡 Tipp: Vermeiden Sie Sonderzeichen oder Umlaute im Pfad, um Plugin-Ladefehler zu verhindern."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Maps in den Maps-Ordner kopieren",
                    Body = "Kopieren Sie RPG-Maps (.w3x oder .w3m) in den Maps-Ordner im Spielverzeichnis (z. B. Maps\\Download\\).\n" +
                           "💡 Erinnerung: Halten Sie Kartendateinamen stets kurz und einfach."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Spielpfad im Launcher festlegen",
                    Body = "Öffnen Sie den Warcraft Platform Manager:\n" +
                           "• Klicken Sie bei 'Warcraft 3-Ordner' auf 'Durchsuchen...' und wählen Sie den Ordner mit war3.exe.\n" +
                           "• Klicken Sie auf '↗', um das Verzeichnis direkt zu öffnen und zu überprüfen."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Neuestes KKWE-Plugin aktualisieren und wählen",
                    Body = "• Klicken Sie oben auf 'Updates', um die neuesten KKWE-Plugins herunterzuladen und zu installieren.\n" +
                           "• Wählen Sie unter 'Plugin-Version' das aktuellste Profil (z. B. KKWE 2.0.12.2606).\n" +
                           "• Optional: Klicken Sie auf 'YDWE', um erweiterte Plugineinstellungen vorzunehmen."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Speicherprofile, Load Save (VIP), Clean Data, Map-Level (1-100) & Rank 1",
                    Body = "• Wählen Sie Ihre Karte aus (oder ziehen Sie Kartendateien per Drag & Drop in den Launcher).\n" +
                           "• Erstellen & Löschen: Klicken Sie auf '+ Neu' für einen separaten Spielstand; klicken Sie auf 'Löschen', um nicht benötigte Slots zu entfernen.\n" +
                           "• Spielstand laden (Load Save / VIP): Klicken Sie auf 'Load Save', um eine externe Speicherdatei (.ini, .txt, geteilte VIP-Saves) auszuwählen, vergeben Sie einen Namen und nutzen Sie sie direkt.\n" +
                           "• Daten bereinigen (Clean Data): Klicken Sie auf 'Clean Data', um temporäre Spieldateien (Maps\\WPM-Cache, _Trash-Papierkorb, _History-Snapshots, Fehlerprotokolle) zu entfernen und Speicherplatz freizugeben.\n" +
                           "• Map-Level (1-100): Geben Sie das gewünschte Level (1 bis 100) ein und klicken Sie auf 'Lvl' (oder Enter) zur Übernahme in DzAPI- und KKAPI-Standards.\n" +
                           "• Rank 1 umschalten: Klicken Sie auf 'Rank 1' (leuchtet bei Aktivierung goldgelb), um Rang-1-Titel und Privilegien in unterstützten Karten freizuschalten."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Spieleinstellungen anpassen (IN-GAME-OPTIONEN)",
                    Body = "Klicken Sie auf '⚙ IN-GAME-OPTIONEN':\n" +
                           "• Legen Sie Ihren Spielernamen (User Name) für Mehrspieler und Speicheridentifikation fest.\n" +
                           "• Aktivieren Sie 'Maus sperren', um den Mauszeiger im Fenster zu halten.\n" +
                           "• Aktivieren Sie 'Breitbild 16:9' und 'Seitenverhältnis korrigieren' für optimale Darstellung.\n" +
                           "• Aktivieren Sie 'Schnelles Laden' und 'Speicherwert-Spam stummschalten'."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Spiel starten & automatische Synchronisierung (Play Map / Play War3)",
                    Body = "• Wählen Sie Grafik-API (OpenGL / DirectX) und Anzeigemodus (Vollbild / Rahmenlos / Fenstermodus).\n" +
                           "• Play Map (Map starten): Startet direkt die gewählte Karte mit dem aktiven Spielstand. Der Spielfortschritt wird beim Beenden automatisch synchronisiert!\n" +
                           "• Play War3 (Hauptmenü): Startet Warcraft III mit allen aktiven Plugins direkt ins Hauptmenü für LAN-Spiele oder Einstellungen."
                }
            },

            ["KO"] = new GuideContent
            {
                BannerTitle = "📖 워크래프트 플랫폼 - 설치 및 빠른 시작 가이드",
                BannerSubtitle = "클린 버전 설정, 플러그인 업데이트, VIP 세이브 로드, 맵 레벨 조정 및 자동 동기화 단계별 안내.",
                Step1Title = "1. 클린 버전 다운로드 (Warcraft_1.27.5_clean.zip) 및 aio-runtimes_v2.4.9.exe 설치",
                Step1Desc = "• 아래 Google Drive 링크에서 Warcraft_1.27.5_clean.zip 및 aio-runtimes_v2.4.9.exe를 다운로드합니다.\n" +
                            "• 중요: 먼저 aio-runtimes_v2.4.9.exe를 실행하여 필수 Visual C++ 런타임(2005-2022) 및 DirectX를 설치하세요. 워크래프트 실행이나 KKWE 플러그인 로드 시 발생하는 DLL 누락 오류를 방지합니다.\n" +
                            "• Google Drive 링크: " + DRIVE_URL,
                BtnCardDrive = "🌐 Google Drive 링크 열기",
                BtnCardCopy = "📋 다운로드 링크 복사",
                CommTitle = "💬 공식 커뮤니티 및 유튜브 채널 (Discord & YouTube)",
                CommDesc = "활발한 워크래프트 RPG 커뮤니티에 참여하여 맵을 공유하고, 팀원을 찾고, 튜토리얼 영상을 시청하세요:",
                BtnDiscord = "💬 Discord 서버 참여",
                BtnYouTube = "▶ YouTube 채널 보기",
                WarnTitle = "⚠️ 중요 경고: 맵 파일 이름 길이 제한",
                WarnBody = "워크래프트 III 엔진은 맵 실행 경로에 대해 엄격한 54바이트 내부 버퍼 제한을 갖습니다!\n" +
                           "• 너무 긴 맵 파일 이름이나 깊은 하위 폴더 경로를 사용하지 마세요.\n" +
                           "• 권장: 플레이하기 전에 맵 파일 이름을 짧게 변경하세요 (예: map.w3x).\n" +
                           "• 이름이 너무 길면 워크래프트 III가 강제 종료(Crash)되거나 세이브 슬롯 동기화에 실패합니다!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Warcraft 1.27.5 클린 버전 압축 해제",
                    Body = "컴퓨터의 원하는 디렉터리에 압축을 풉니다 (예: D:\\Warcraft_1.27.5 또는 C:\\Games\\Warcraft3).\n" +
                           "💡 팁: 플러그인 로드 오류를 방지하기 위해 경로에 한글, 특수 문자 또는 지나치게 긴 하위 경로를 피하세요."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Maps 폴더에 맵 파일 복사",
                    Body = "다운로드한 RPG 맵 파일(.w3x 또는 .w3m)을 게임 폴더 내 Maps 디렉터리에 복사합니다 (예: Maps\\Download\\).\n" +
                           "💡 알림: 맵 파일 이름은 항상 짧고 간결하게 유지하세요."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. 런처에서 게임 경로 설정",
                    Body = "Warcraft Platform Manager 런처 실행:\n" +
                           "• '워크래프트 3 폴더 선택'에서 '찾아보기...'를 클릭하여 war3.exe가 있는 폴더를 선택합니다.\n" +
                           "• 바로가기 버튼 '↗'를 클릭하여 대상 폴더를 열고 확인할 수 있습니다."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. 최신 KKWE 플러그인 업데이트 및 선택",
                    Body = "• 상단 헤더의 '업데이트' 버튼을 클릭하여 최신 KKWE 플러그인을 다운로드 및 설치합니다.\n" +
                           "• '플러그인 버전' 드롭다운에서 최신 프로필을 선택합니다 (예: KKWE 2.0.12.2606 인식).\n" +
                           "• 선택 사항: 고급 설정을 구성하려면 'YDWE'를 클릭하세요."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. 세이브 관리, Load Save (VIP), Clean Data, 맵 레벨 (1-100) 및 Rank 1",
                    Body = "• 드롭다운에서 맵을 선택합니다 (또는 맵 파일을 런처로 직접 드래그 앤 드롭).\n" +
                           "• 생성 및 삭제: '+ 신규'를 클릭하여 캐릭터별 독립 세이브를 생성하고, '삭제'로 불필요한 슬롯을 제거합니다.\n" +
                           "• 세이브 불러오기 (VIP 세이브 로드): 'Load Save'를 클릭하여 외부 세이브 파일(.ini, .txt, VIP 공유 세이브)을 선택하고, 새 슬롯 이름을 입력하여 바로 사용합니다.\n" +
                           "• 데이터 정리 (Clean Data): 'Clean Data'를 클릭하여 플레이 중 쌓인 임시 파일(Maps\\WPM 캐시, _Trash 휴지통, _History 스냅샷, 오류 로그 등)을 선택적으로 정리하여 디스크 공간을 확보합니다.\n" +
                           "• 맵 레벨 (1-100): 원하는 레벨(1~100)을 입력하고 'Lvl' 버튼(또는 Enter 키)을 클릭하여 DzAPI 및 KKAPI 표준으로 자동 동기화합니다.\n" +
                           "• Rank 1 전환: 'Rank 1' 버튼(활성화 시 황금색 테두리)을 클릭하여 지원되는 맵의 랭킹 1위 특권과 칭호를 활성화합니다."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. 게임 내 옵션 설정 (IN-GAME OPTIONS)",
                    Body = "클릭 '⚙ 게임 내 옵션':\n" +
                           "• 멀티플레이 및 세이브 식별을 위해 인게임 사용자 이름(User Name)을 설정하세요.\n" +
                           "• 창 모드에서 마우스가 밖으로 나가지 않도록 '마우스 가두기'를 활성화하세요.\n" +
                           "• 최적의 그래픽을 위해 '16:9 와이드스크린'과 '화면 비율 고정'을 활성화하세요.\n" +
                           "• 빠른 로딩과 점수 팝업을 차단하는 '세이브 점수 알림 음소거'를 활성화하세요."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. 게임 실행 및 자동 동기화 (Play Map / Play War3)",
                    Body = "• 그래픽 모드(OpenGL / DirectX)와 디스플레이 모드(전체화면 / 테두리 없음 / 창 모드)를 선택합니다.\n" +
                           "• Play Map (맵 실행): 선택한 맵과 활성 세이브 슬롯을 마운트하여 바로 실행합니다. 게임 종료 시 진행 상황이 세이브 슬롯에 자동으로 동기화됩니다!\n" +
                           "• Play War3 (메인 메뉴): 활성화된 모든 플러그인을 적용하여 LAN 게임 또는 설정을 위해 메인 메뉴로 직접 실행합니다."
                }
            },

            ["ES"] = new GuideContent
            {
                BannerTitle = "📖 Plataforma Warcraft - Guía de instalación y uso rápido",
                BannerSubtitle = "Instrucciones paso a paso para configurar el juego limpio, actualizar plugins, cargar partidas VIP, nivel de mapa y sincronización automática.",
                Step1Title = "1. Descarga el juego limpio (Warcraft_1.27.5_clean.zip) e instala aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Descargue Warcraft_1.27.5_clean.zip y aio-runtimes_v2.4.9.exe desde el enlace de Google Drive abajo.\n" +
                            "• IMPORTANTE: Ejecute e instale primero aio-runtimes_v2.4.9.exe para instalar todas las librerías Visual C++ (2005-2022) y DirectX. Esto evita errores de DLL faltantes al iniciar Warcraft o cargar plugins KKWE.\n" +
                            "• Enlace de Google Drive: " + DRIVE_URL,
                BtnCardDrive = "🌐 Abrir enlace de Google Drive",
                BtnCardCopy = "📋 Copiar enlace de descarga",
                CommTitle = "💬 Únete a la comunidad y canal de vídeo (Discord y YouTube)",
                CommDesc = "Conéctate con nuestra activa comunidad de Warcraft RPG, comparte mapas, encuentra compañeros de equipo y mira tutoriales:",
                BtnDiscord = "💬 Servidor de Discord",
                BtnYouTube = "▶ Canal de YouTube",
                WarnTitle = "⚠️ ADVERTENCIA CRÍTICA: Longitud del nombre del archivo de mapa",
                WarnBody = "¡El motor de Warcraft III tiene un límite de búfer interno estricto de 54 bytes en las rutas de lanzamiento de mapas!\n" +
                           "• Nunca use nombres de archivo de mapa demasiado largos ni rutas de subcarpetas profundas.\n" +
                           "• Recomendado: cambie el nombre de su mapa a uno corto antes de jugar (por ejemplo, map.w3x).\n" +
                           "• ¡Los nombres demasiado largos provocarán el cierre inesperado (Crash) de Warcraft III o fallos en el guardado!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Extraer Warcraft 1.27.5 limpio",
                    Body = "Extraiga el archivo comprimido en cualquier carpeta (por ejemplo: D:\\Warcraft_1.27.5 o C:\\Games\\Warcraft3).\n" +
                           "💡 Consejo: Evite caracteres especiales o rutas con tildes para evitar problemas de carga de plugins."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Copiar mapas a la carpeta Maps",
                    Body = "Copie los archivos de mapa RPG (.w3x o .w3m) en la carpeta Maps dentro del directorio del juego (ej.: Maps\\Download\\).\n" +
                           "💡 Recordatorio: Mantenga siempre nombres de archivo cortos y limpios."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Establecer la ruta del juego en el iniciador",
                    Body = "Abra Warcraft Platform Manager:\n" +
                           "• En 'Seleccionar carpeta de War3', haga clic en 'Examinar...' y elija la carpeta que contiene war3.exe.\n" +
                           "• Haga clic en '↗' para abrir y verificar rápidamente la carpeta seleccionada."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Actualizar y seleccionar el plugin KKWE más reciente",
                    Body = "• Haga clic en 'Actualizaciones' en el encabezado superior para descargar e instalar los últimos plugins KKWE.\n" +
                           "• En 'Versión del plugin', elija el perfil más reciente (ej.: KKWE 2.0.12.2606).\n" +
                           "• Opcional: Haga clic en 'YDWE' para configurar opciones avanzadas del plugin."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Perfiles de guardado, Load Save (VIP), Clean Data, nivel de mapa (1-100) y Rank 1",
                    Body = "• Seleccione su mapa en el menú desplegable (o arrastre y suelte archivos de mapa directamente en el iniciador).\n" +
                           "• Crear y eliminar: Haga clic en '+ Nuevo' para crear un guardado independiente; haga clic en 'Eliminar' para quitar ranuras no deseadas.\n" +
                           "• Cargar guardado (Load Save / VIP): Haga clic en 'Load Save' para seleccionar cualquier archivo de guardado externo (.ini, .txt, guardados VIP compartidos), ingrese un nombre y úselo de inmediato.\n" +
                           "• Limpiar datos (Clean Data): Haga clic en 'Clean Data' para limpiar archivos temporales acumulados (caché Maps\\WPM, papelera _Trash, instantáneas _History, registros de errores) y liberar espacio en disco.\n" +
                           "• Nivel de mapa (1-100): Ingrese el nivel deseado (1 a 100) y haga clic en 'Nvl' (o presione Enter) para sincronizar en formatos DzAPI y KKAPI.\n" +
                           "• Alternar Rank 1: Haga clic en 'Rank 1' (borde dorado brillante al activarse) para desbloquear títulos y beneficios del puesto 1 en mapas compatibles."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Ajustar opciones dentro del juego (OPCIONES EN JUEGO)",
                    Body = "Haga clic en '⚙ OPCIONES DENTRO DEL JUEGO':\n" +
                           "• Establezca su nombre de usuario en el juego para multijugador e identificación de guardado.\n" +
                           "• Active 'Bloquear ratón' para mantener el cursor dentro de la ventana.\n" +
                           "• Active 'Pantalla ancha 16:9' y 'Corregir proporción' para una visualización óptima.\n" +
                           "• Active 'Carga rápida' y 'Silenciar valor de guardado' para evitar spam en pantalla."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Iniciar juego y sincronización automática (Play Map / Play War3)",
                    Body = "• Elija la API de gráficos (OpenGL / DirectX) y el modo de pantalla (Pantalla completa / Sin bordes / Ventana).\n" +
                           "• Play Map (Jugar mapa): Inicia directamente el mapa seleccionado con su partida activa montada. ¡El progreso se sincroniza automáticamente al salir del juego!\n" +
                           "• Play War3 (Menú principal): Inicia Warcraft III con todos los plugins activos directamente al menú principal para partidas LAN o ajustes."
                }
            },

            ["UK"] = new GuideContent
            {
                BannerTitle = "📖 Платформа Warcraft - Посібник зі встановлення та швидкого старту",
                BannerSubtitle = "Покрокові інструкції з налаштування чистої гри, оновлення плагінів, завантаження VIP-збережень, рівня карти та автосинхронізації.",
                Step1Title = "1. Завантажте чисту версію (Warcraft_1.27.5_clean.zip) та встановіть aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Завантажте Warcraft_1.27.5_clean.zip та aio-runtimes_v2.4.9.exe за посиланням на Google Drive нижче.\n" +
                            "• ВАЖЛИВО: Спочатку встановіть aio-runtimes_v2.4.9.exe для встановлення всіх необхідних бібліотек Visual C++ (2005-2022) та DirectX. Це запобігає збоям через відсутність DLL під час запуску Warcraft або плагінів KKWE.\n" +
                            "• Посилання на Google Drive: " + DRIVE_URL,
                BtnCardDrive = "🌐 Відкрити Google Drive",
                BtnCardCopy = "📋 Скопіювати посилання",
                CommTitle = "💬 Офіційна спільнота та відеоканал (Discord і YouTube)",
                CommDesc = "Приєднуйтесь до нашої активної спільноти гравців Warcraft RPG, діліться картами, знаходьте союзників та дивіться відеоуроки:",
                BtnDiscord = "💬 Сервер Discord",
                BtnYouTube = "▶ Канал на YouTube",
                WarnTitle = "⚠️ ВАЖЛИВЕ ПОПЕРЕДЖЕННЯ: Довжина шляху та імені файлу карти",
                WarnBody = "Рушій Warcraft III має жорстке обмеження внутрішнього буфера у 54 байти для шляхів запуску карт!\n" +
                           "• Ніколи не використовуйте занадто довгі назви файлів карт або глибокі вкладені папки.\n" +
                           "• Рекомендація: перейменуйте карту на коротку назву перед грою (наприклад, map.w3x).\n" +
                           "• Занадто довгі назви призведуть до аварійного закриття Warcraft III (Crash) або збою збережень!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Розпакуйте чисту версію Warcraft 1.27.5",
                    Body = "Розпакуйте архів у будь-яку папку на диску (наприклад: D:\\Warcraft_1.27.5 або C:\\Games\\Warcraft3).\n" +
                           "💡 Порада: уникайте кирилиці, спецсимволів та занадто довгих шляхів, щоб плагіни працювали стабільно."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Скопіюйте карти до папки Maps",
                    Body = "Скопіюйте файли карт RPG (.w3x або .w3m) у папку Maps всередині каталогу гри (наприклад: Maps\\Download\\).\n" +
                           "💡 Нагадування: завжди зберігайте назви файлів карт короткими та простими."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Вкажіть шлях до папки гри в лаунчері",
                    Body = "Відкрийте лаунчер Warcraft Platform Manager:\n" +
                           "• У пункті 'Папка Warcraft 3' натисніть 'Огляд...' і виберіть каталог із war3.exe.\n" +
                           "• Натисніть кнопку '↗', щоб швидко відкрити та перевірити вибрану папку."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Оновіть та виберіть актуальний плагін KKWE",
                    Body = "• Натисніть кнопку 'Оновлення' у верхній панелі для завантаження та встановлення свіжих плагінів KKWE.\n" +
                           "• У списку 'Версія плагіна' виберіть актуальний профіль (наприклад: KKWE 2.0.12.2606).\n" +
                           "• Додатково: натисніть 'YDWE' для точного налаштування параметрів плагіна."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Профілі збережень, Load Save (VIP), Clean Data, рівень карти (1-100) та Rank 1",
                    Body = "• Виберіть карту зі списку (або перетягніть файл карти безпосередньо у вікно лаунчера).\n" +
                           "• Створення та видалення: натисніть '+ Новий' для створення окремого збереження; натисніть 'Видалити' для видалення непотрібних слотів.\n" +
                           "• Завантаження збереження (Load Save / VIP): натисніть 'Load Save', виберіть будь-який зовнішній файл збереження (.ini, .txt, VIP-збереження), введіть назву та одразу використовуйте його.\n" +
                           "• Очищення даних (Clean Data): натисніть 'Clean Data' для видалення тимчасових файлів (кеш карт Maps\\WPM, кошик _Trash, знімки _History, логи помилок) та звільнення місця на диску.\n" +
                           "• Рівень карти (Map Level 1-100): введіть бажаний рівень (від 1 до 100) та натисніть 'Рівень' (або Enter) для запису у формати DzAPI та KKAPI.\n" +
                           "• Перемикач Rank 1: натисніть 'Rank 1' (підсвічується золотим при активації) для розблокування привілеїв першого місця в рейтингу."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Внутрішньоігрові налаштування (ВНУТРІШНЬОІГРОВІ ОПЦІЇ)",
                    Body = "Натисніть '⚙ ВНУТРІШНЬОІГРОВІ ОПЦІЇ':\n" +
                           "• Вкажіть ваше ім'я в грі (User Name) для мережевої гри та ідентифікації збережень.\n" +
                           "• Увімкніть 'Захоплення миші', щоб курсор не виходив за межі вікна.\n" +
                           "• Увімкніть 'Широкий екран 16:9' та 'Виправлення пропорцій' для оптимального зображення.\n" +
                           "• Увімкніть 'Швидке завантаження' та 'Заглушити очки збереження' для блокування спаму."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Запуск гри та автоматична синхронізація (Play Map / Play War3)",
                    Body = "• Оберіть графічний API (OpenGL / DirectX) та режим відображення (Повноекранний / Без рамок / Віконний).\n" +
                           "• Play Map (Запуск карти): безпосередньо запускає вибрану карту з активним збереженням. Прогрес автоматично синхронізується при виході з гри!\n" +
                           "• Play War3 (До головного меню): запускає Warcraft III з усіма активними плагінами в головне меню для гри по мережі або налаштувань."
                }
            },

            ["FR"] = new GuideContent
            {
                BannerTitle = "📖 Plateforme Warcraft - Guide d'installation et de démarrage rapide",
                BannerSubtitle = "Instructions étape par étape pour configurer un jeu propre, mettre à jour les plugins, charger les sauvegardes VIP, le niveau de carte et la synchronisation.",
                Step1Title = "1. Téléchargez la version propre (Warcraft_1.27.5_clean.zip) et installez aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Téléchargez Warcraft_1.27.5_clean.zip et aio-runtimes_v2.4.9.exe depuis le lien Google Drive ci-dessous.\n" +
                            "• IMPORTANT : Exécutez et installez d'abord aio-runtimes_v2.4.9.exe pour installer tous les runtimes Visual C++ (2005-2022) et DirectX. Cela évite les erreurs de DLL manquantes au lancement de Warcraft ou des plugins KKWE.\n" +
                            "• Lien Google Drive : " + DRIVE_URL,
                BtnCardDrive = "🌐 Ouvrir le lien Google Drive",
                BtnCardCopy = "📋 Copier le lien",
                CommTitle = "💬 Rejoindre la communauté et la chaîne vidéo (Discord & YouTube)",
                CommDesc = "Rejoignez notre communauté active de Warcraft RPG, partagez des cartes, trouvez des coéquipiers et regardez des tutoriels :",
                BtnDiscord = "💬 Rejoindre Discord",
                BtnYouTube = "▶ Chaîne YouTube",
                WarnTitle = "⚠️ AVERTISSEMENT CRITIQUE : Longueur du nom de fichier de la carte",
                WarnBody = "Le moteur de Warcraft III a une limite stricte de tampon interne de 54 octets pour les chemins de lancement des cartes !\n" +
                           "• N'utilisez jamais de noms de fichiers de carte trop longs ni de sous-dossiers profonds.\n" +
                           "• Recommandé : Renommez votre carte avec un nom court avant de jouer (ex. : map.w3x).\n" +
                           "• Les noms trop longs feront planter Warcraft III ou échouer la synchronisation des sauvegardes !",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Extraire la version propre de Warcraft 1.27.5",
                    Body = "Extrayez l'archive dans le dossier de votre choix (ex. : D:\\Warcraft_1.27.5 ou C:\\Games\\Warcraft3).\n" +
                           "💡 Astuce : Évitez les caractères spéciaux ou accentués dans le chemin pour prévenir tout problème de chargement de plugin."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Copier les cartes dans le dossier Maps",
                    Body = "Copiez les fichiers de carte RPG (.w3x ou .w3m) dans le dossier Maps de votre jeu (ex. : Maps\\Download\\).\n" +
                           "💡 Rappel : Gardez toujours des noms de fichiers de cartes courts et simples."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Définir le chemin du jeu dans le lanceur",
                    Body = "Ouvrez Warcraft Platform Manager :\n" +
                           "• Dans 'Sélectionner le dossier War3', cliquez sur 'Parcourir...' et sélectionnez le dossier contenant war3.exe.\n" +
                           "• Cliquez sur '↗' pour ouvrir et vérifier rapidement le dossier du jeu."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Mettre à jour et sélectionner le dernier plugin KKWE",
                    Body = "• Cliquez sur 'Mises à jour' dans l'en-tête pour télécharger et installer les derniers plugins KKWE.\n" +
                           "• Dans 'Version du plugin', sélectionnez le profil le plus récent (ex. : KKWE 2.0.12.2606).\n" +
                           "• Facultatif : Cliquez sur 'YDWE' pour configurer les paramètres avancés du plugin."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Profils de sauvegarde, Load Save (VIP), Clean Data, niveau de carte (1-100) et Rank 1",
                    Body = "• Sélectionnez votre carte dans la liste déroulante (ou glissez-déposez des fichiers de carte directement dans le lanceur).\n" +
                           "• Créer et supprimer : Cliquez sur '+ Nouveau' pour créer une sauvegarde isolée ; cliquez sur 'Supprimer' pour retirer les emplacements inutiles.\n" +
                           "• Charger une sauvegarde (Load Save / VIP) : Cliquez sur 'Load Save' pour choisir un fichier externe (.ini, .txt, sauvegardes VIP partagées), saisissez un nom et jouez immédiatement.\n" +
                           "• Nettoyer les données (Clean Data) : Cliquez sur 'Clean Data' pour nettoyer les fichiers temporaires (cache Maps\\WPM, corbeille _Trash, instantanés _History, journaux d'erreurs) et libérer de l'espace disque.\n" +
                           "• Niveau de carte (1-100) : Saisissez le niveau souhaité (1 à 100) et cliquez sur 'Niv' (ou Entrée) pour appliquer aux normes DzAPI et KKAPI.\n" +
                           "• Activer Rank 1 : Cliquez sur 'Rank 1' (bordure or ambré lorsque actif) pour débloquer les privilèges du classement n°1 dans les cartes prises en charge."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Options en jeu (OPTIONS EN JEU)",
                    Body = "Cliquez sur '⚙ OPTIONS EN JEU' :\n" +
                           "• Définissez votre nom de joueur en jeu pour le multijoueur et l'identification des sauvegardes.\n" +
                           "• Activez 'Bloquer la souris' pour confiner le curseur dans la fenêtre.\n" +
                           "• Activez 'Écran large 16:9' et 'Corriger le format' pour un affichage optimal.\n" +
                           "• Activez 'Chargement rapide' et 'Masquer les points de sauvegarde' pour bloquer les spams à l'écran."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Lancement du jeu et synchronisation automatique (Play Map / Play War3)",
                    Body = "• Choisissez le moteur graphique (OpenGL / DirectX) et le mode d'affichage (Plein écran / Sans bordure / Fenêtré).\n" +
                           "• Play Map (Lancer la carte) : Lance directement la carte sélectionnée avec votre sauvegarde active associée. La progression est automatiquement synchronisée en quittant le jeu !\n" +
                           "• Play War3 (Menu principal) : Lance Warcraft III avec tous les plugins actifs directement vers le menu principal pour les parties en réseau local ou les réglages."
                }
            },

            ["PL"] = new GuideContent
            {
                BannerTitle = "📖 Platforma Warcraft - Instrukcja instalacji i szybkiego startu",
                BannerSubtitle = "Instrukcja krok po kroku dotycząca konfiguracji czystej gry, aktualizacji wtyczek, wczytywania zapisów VIP, poziomu mapy i automatycznej synchronizacji.",
                Step1Title = "1. Pobierz czystą grę (Warcraft_1.27.5_clean.zip) i zainstaluj aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Pobierz Warcraft_1.27.5_clean.zip oraz aio-runtimes_v2.4.9.exe z poniższego linku Google Drive.\n" +
                            "• WAŻNE: Najpierw uruchom i zainstaluj aio-runtimes_v2.4.9.exe, aby zainstalować biblioteki Visual C++ (2005-2022) i DirectX. Zapobiega to błędom brakujących plików DLL przy uruchamianiu Warcrafta lub wtyczek KKWE.\n" +
                            "• Link Google Drive: " + DRIVE_URL,
                BtnCardDrive = "🌐 Otwórz Google Drive",
                BtnCardCopy = "📋 Kopiuj link pobierania",
                CommTitle = "💬 Dołącz do społeczności i kanału wideo (Discord i YouTube)",
                CommDesc = "Dołącz do naszej aktywnej społeczności Warcraft RPG, dziel się mapami, znajduj znajomych do gry i oglądaj poradniki wideo:",
                BtnDiscord = "💬 Dołącz do Discorda",
                BtnYouTube = "▶ Kanał YouTube",
                WarnTitle = "⚠️ WAŻNE OSTRZEŻENIE: Długość nazwy pliku mapy",
                WarnBody = "Silnik Warcraft III posiada ścisły limit wewnętrznego bufora wynoszący 54 bajty dla ścieżek uruchamiania map!\n" +
                           "• Nigdy nie używaj zbyt długich nazw plików map ani głębokich podfolderów.\n" +
                           "• Zalecenie: przed grą zmień nazwę mapy na krótką (np. map.w3x).\n" +
                           "• Zbyt długie nazwy spowodują awarię (crash) Warcrafta III lub błąd synchronizacji zapisów!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Rozpakuj czystego Warcrafta 1.27.5",
                    Body = "Rozpakuj archiwum do dowolnego folderu (np. D:\\Warcraft_1.27.5 lub C:\\Games\\Warcraft3).\n" +
                           "💡 Wskazówka: Unikaj polskich znaków diakrytycznych i znaków specjalnych w ścieżce, aby zapobiec błędom wtyczek."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Skopiuj mapy do folderu Maps",
                    Body = "Skopiuj pliki map RPG (.w3x lub .w3m) do folderu Maps w katalogu gry (np. Maps\\Download\\).\n" +
                           "💡 Przypomnienie: Zawsze dbaj o to, aby nazwy plików map były krótkie i proste."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Ustaw ścieżkę gry w launcherze",
                    Body = "Otwórz Warcraft Platform Manager:\n" +
                           "• W sekcji 'Wybierz folder Warcraft 3' kliknij 'Przeglądaj...' i wskaż folder zawierający war3.exe.\n" +
                           "• Kliknij '↗', aby szybko otworzyć i sprawdzić wybrany folder."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Zaktualizuj i wybierz najnowszą wtyczkę KKWE",
                    Body = "• Kliknij 'Aktualizacje' na górnym pasku, aby pobrać i zainstalować najnowsze wtyczki KKWE.\n" +
                           "• W sekcji 'Wersja wtyczki' wybierz najnowszy profil (np. KKWE 2.0.12.2606).\n" +
                           "• Opcjonalnie: Kliknij 'YDWE', aby dostosować zaawansowane ustawienia wtyczki."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Profile zapisu, Load Save (VIP), Clean Data, poziom mapy (1-100) i Rank 1",
                    Body = "• Wybierz mapę z listy (lub przeciągnij i upuść pliki mapy bezpośrednio na launcher).\n" +
                           "• Tworzenie i usuwanie: Kliknij '+ Nowy', aby utworzyć niezależny zapis postaci; kliknij 'Usuń', aby skasować niepotrzebne sloty.\n" +
                           "• Wczytaj zapis (Load Save / VIP): Kliknij 'Load Save', aby wybrać zewnętrzny plik zapisu (.ini, .txt, zapisy VIP), podaj nową nazwę i od razu z niego korzystaj.\n" +
                           "• Czyszczenie danych (Clean Data): Kliknij 'Clean Data', aby selektywnie usunąć pliki tymczasowe (pamięć podręczną Maps\\WPM, kosz _Trash, migawki _History, dzienniki błędów) i odzyskać miejsce na dysku.\n" +
                           "• Poziom mapy (1-100): Wpisz pożądany poziom (od 1 do 100) i kliknij 'Poz' (lub naciśnij Enter), aby zapisać w standardach DzAPI i KKAPI.\n" +
                           "• Przełącznik Rank 1: Kliknij 'Rank 1' (podświetla się na złoto po aktywacji), aby odblokować przywileje lidera rankingu na obsługiwanych mapach."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Opcje w grze (OPCJE W GRZE)",
                    Body = "Kliknij '⚙ OPCJE W GRZE':\n" +
                           "• Ustaw swoją nazwę gracza w grze (User Name) do rozgrywki wieloosobowej i identyfikacji zapisu.\n" +
                           "• Włącz 'Blokuj kursor', aby kursor nie uciekał poza okno gry.\n" +
                           "• Włącz 'Format 16:9' oraz 'Napraw proporcje', aby uzyskać idealny obraz.\n" +
                           "• Włącz 'Szybkie ładowanie' i 'Wycisz powiadomienia o zapisie', aby zablokować wyskakujące okienka."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Uruchamianie gry i automatyczna synchronizacja (Play Map / Play War3)",
                    Body = "• Wybierz silnik graficzny (OpenGL / DirectX) i tryb wyświetlania (Pełny ekran / Bez ramek / W oknie).\n" +
                           "• Play Map (Graj w mapę): Bezpośrednio uruchamia wybraną mapę z zamontowanym aktywnym profilem zapisu. Postępy synchronizują się automatycznie po wyjściu z gry!\n" +
                           "• Play War3 (Menu główne): Uruchamia Warcrafta III ze wszystkimi aktywnymi wtyczkami bezpośrednio do menu głównego na potrzeby gier w sieci LAN lub ustawień."
                }
            },

            ["PT"] = new GuideContent
            {
                BannerTitle = "📖 Plataforma Warcraft - Guia de instalação e início rápido",
                BannerSubtitle = "Instruções passo a passo para configuração de jogo limpo, atualização de plugins, carregamento de saves VIP, nível de mapa e sincronização automática.",
                Step1Title = "1. Baixe o jogo limpo (Warcraft_1.27.5_clean.zip) e instale o aio-runtimes_v2.4.9.exe",
                Step1Desc = "• Baixe Warcraft_1.27.5_clean.zip e aio-runtimes_v2.4.9.exe no link do Google Drive abaixo.\n" +
                            "• IMPORTANTE: Execute e instale o aio-runtimes_v2.4.9.exe primeiro para instalar todos os componentes Visual C++ (2005-2022) e DirectX. Isso evita falhas de DLL ausente ao iniciar o Warcraft ou plugins KKWE.\n" +
                            "• Link do Google Drive: " + DRIVE_URL,
                BtnCardDrive = "🌐 Abrir Google Drive",
                BtnCardCopy = "📋 Copiar link de download",
                CommTitle = "💬 Participe da comunidade e canal de vídeo (Discord e YouTube)",
                CommDesc = "Conecte-se com nossa comunidade ativa de Warcraft RPG, compartilhe mapas, encontre parceiros de jogo e assista a tutoriais:",
                BtnDiscord = "💬 Servidor do Discord",
                BtnYouTube = "▶ Canal no YouTube",
                WarnTitle = "⚠️ AVISO CRÍTICO: Tamanho do nome do arquivo do mapa",
                WarnBody = "O motor do Warcraft III possui um limite rígido de buffer interno de 54 bytes nos caminhos de inicialização de mapas!\n" +
                           "• Nunca use nomes de arquivo de mapa excessivamente longos ou subpastas profundas.\n" +
                           "• Recomendado: renomeie seu mapa para um nome curto antes de jogar (ex.: map.w3x).\n" +
                           "• Nomes muito longos farão o Warcraft III travar (Crash) ou falhar na sincronização dos saves!",
                Step2 = new GuideStepInfo
                {
                    Title = "2. Extrair o Warcraft 1.27.5 limpo",
                    Body = "Extraia o arquivo zip para qualquer pasta no computador (ex.: D:\\Warcraft_1.27.5 ou C:\\Games\\Warcraft3).\n" +
                           "💡 Dica: Evite caracteres especiais ou caminhos acentuados para prevenir problemas no carregamento de plugins."
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. Copiar mapas para a pasta Maps",
                    Body = "Copie os arquivos de mapas RPG (.w3x ou .w3m) para a pasta Maps dentro do diretório do jogo (ex.: Maps\\Download\\).\n" +
                           "💡 Lembrete: Mantenha sempre os nomes de arquivo dos mapas curtos e sem espaços complexos."
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. Definir a pasta do jogo no inicializador",
                    Body = "Abra o Warcraft Platform Manager:\n" +
                           "• Em 'Selecionar pasta do War3', clique em 'Procurar...' e selecione a pasta contendo o war3.exe.\n" +
                           "• Clique em '↗' para abrir e verificar rapidamente a pasta selecionada."
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. Atualizar e selecionar o plugin KKWE mais recente",
                    Body = "• Clique em 'Atualizações' no cabeçalho superior para baixar e instalar os plugins KKWE mais recentes.\n" +
                           "• Em 'Versão do plugin', escolha o perfil mais recente (ex.: KKWE 2.0.12.2606).\n" +
                           "• Opcional: Clique em 'YDWE' para configurar opções avançadas do plugin."
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. Perfis de save, Load Save (VIP), Clean Data, nível de mapa (1-100) e Rank 1",
                    Body = "• Selecione seu mapa na lista (ou arraste e solte arquivos de mapa diretamente no inicializador).\n" +
                           "• Criar e excluir: Clique em '+ Novo' para criar um save isolado para cada personagem; clique em 'Excluir' para remover perfis indesejados.\n" +
                           "• Carregar save (Load Save / VIP): Clique em 'Load Save' para escolher qualquer arquivo de save externo (.ini, .txt, saves VIP compartilhados), defina um nome e use-o imediatamente.\n" +
                           "• Limpar dados (Clean Data): Clique em 'Clean Data' para limpar arquivos temporários (cache em Maps\\WPM, lixeira _Trash, histórico _History, relatórios de erros) e liberar espaço em disco.\n" +
                           "• Nível do mapa (1-100): Digite o nível desejado (1 a 100) e clique em 'Nív' (ou pressione Enter) para aplicar aos padrões DzAPI e KKAPI.\n" +
                           "• Alternar Rank 1: Clique em 'Rank 1' (borda dourada brilhante quando ativado) para desbloquear privilégios e títulos do topo do ranking em mapas compatíveis."
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. Ajustar opções do jogo (OPÇÕES DO JOGO)",
                    Body = "Clique em '⚙ OPÇÕES DO JOGO':\n" +
                           "• Defina seu nome de jogador para partidas multiplayer e identificação do save.\n" +
                           "• Ative 'Travar mouse' para prender o cursor dentro da janela do jogo.\n" +
                           "• Ative 'Widescreen 16:9' e 'Corrigir proporção' para a melhor qualidade visual.\n" +
                           "• Ative 'Carregamento rápido' e 'Silenciar pontuação do save' para evitar mensagens repetitivas."
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. Iniciar o jogo e sincronização automática (Play Map / Play War3)",
                    Body = "• Escolha a API gráfica (OpenGL / DirectX) e o modo de exibição (Tela cheia / Sem bordas / Janela).\n" +
                           "• Play Map (Jogar mapa): Inicia diretamente o mapa selecionado com seu save ativo montado. O progresso é sincronizado automaticamente ao sair do jogo!\n" +
                           "• Play War3 (Menu principal): Inicia o Warcraft III com todos os plugins ativos diretamente no menu principal para jogos em rede local ou ajustes."
                }
            },

            ["CN"] = new GuideContent
            {
                BannerTitle = "📖 魔兽对战与RPG平台 - 完整安装与使用指南",
                BannerSubtitle = "准备纯净版游戏、更新KKWE插件、载入VIP存档、定制地图等级以及自动同步进度的分步教程。",
                Step1Title = "1. 下载纯净版 (Warcraft_1.27.5_clean.zip) 与运行库 (aio-runtimes_v2.4.9.exe)",
                Step1Desc = "• 访问官方共享网盘，下载 Warcraft_1.27.5_clean.zip 以及必备运行库 aio-runtimes_v2.4.9.exe。\n" +
                            "• 【重要步骤】：请先运行并安装 aio-runtimes_v2.4.9.exe，自动配置好 Visual C++ (2005-2022) 与 DirectX 9.0c 运行环境。彻底解决魔兽及 KKWE 插件启动时缺失 DLL (MSVCR/D3DX) 的报错问题。\n" +
                            "• 网盘地址: " + DRIVE_URL,
                BtnCardDrive = "🌐 打开网盘下载地址",
                BtnCardCopy = "📋 复制下载链接",
                CommTitle = "💬 加入官方社区与视频频道（Discord & YouTube）",
                CommDesc = "欢迎加入我们活跃的魔兽RPG玩家社群，交流游戏心得、组队开黑并观看官方视频教程：",
                BtnDiscord = "💬 加入 Discord 社区",
                BtnYouTube = "▶ 访问 YouTube 频道",
                WarnTitle = "⚠️ 核心警告：地图文件名切勿过长！",
                WarnBody = "魔兽争霸3引擎内部对地图启动路径存在54字节的最大缓冲区限制！\n" +
                           "• 切勿将地图名称保存过长，更不要包含复杂特殊符号或多层深层子目录。\n" +
                           "• 建议在放入游戏前将地图名重命名为简短纯英文字符（例如: kiemthe.w3x）。\n" +
                           "• 若地图名过长，启动时必定造成魔兽游戏崩溃 (Crash) 或存档无法正常读取！",
                Step2 = new GuideStepInfo
                {
                    Title = "2. 解压 Warcraft_1.27.5_clean.zip",
                    Body = "解压压缩包至电脑任意磁盘目录（例如: D:\\Warcraft_1.27.5 或 C:\\Games\\Warcraft3）。\n" +
                           "💡 提示：目录路径中切勿包含中文、特殊字符或过长目录层级，以防DLL加载异常。"
                },
                Step3 = new GuideStepInfo
                {
                    Title = "3. 将地图文件放入 Maps 文件夹",
                    Body = "将你下载的 RPG 地图（.w3x 或 .w3m）复制到解压目录下的 Maps 文件夹中（例如: Maps\\Download\\）。\n" +
                           "💡 再次提醒：务必保持地图文件名简明短小。"
                },
                Step4 = new GuideStepInfo
                {
                    Title = "4. 在启动器中选择游戏目录 (Game Library)",
                    Body = "打开 Warcraft Platform Manager 启动器：\n" +
                           "• 在【选择魔兽目录】中点击【浏览...】，定位到刚才解压包含 war3.exe 的根目录。\n" +
                           "• 点击旁边快捷按钮【↗】可立即打开此文件夹进行确认。"
                },
                Step5 = new GuideStepInfo
                {
                    Title = "5. 更新与安装最新 KKWE 插件",
                    Body = "• 点击右上角的【更新】按钮，在线拉取并更新最新 KKWE 插件版本。\n" +
                           "• 在【插件版本】下拉框中选择最新插件（系统将自动识别出具体版本，例如: KKWE 2.0.12.2606）。\n" +
                           "• 如需配置高阶插件特性，可点击【YDWE】打开高级配置中心。"
                },
                Step6 = new GuideStepInfo
                {
                    Title = "6. 存档管理、Load Save (VIP)、Clean Data、地图等级 (1-100) 与 Rank 1",
                    Body = "• 在【选择地图】中选取想要体验的地图（亦可直接从桌面拖拽地图文件放入启动器）。\n" +
                           "• 创建与删除：点击【+ 新建槽】为当前角色创建专属独立存档；点击【删除】可将不再使用的槽移至回收目录。\n" +
                           "• 载入存档 (Load Save)：点击【Load Save】即可选取电脑中任意外部存档文件（VIP 存档、好友分享存档等），输入新名称后直接导入使用。\n" +
                           "• 清理数据 (Clean Data)：点击【清理数据】可自主选择清理游戏过程中积累的临时文件（Maps\\WPM 地图缓存、_Trash 存档垃圾、_History 历史快照、游戏报错日志等），一键释放磁盘空间。\n" +
                           "• 地图等级 (Map Level 1-100)：在输入框中填入期望等级（1 至 100）并点击【Lưu / Set】（或按 Enter 回车键），系统自动同步至 DzAPI 与 KKAPI 格式。\n" +
                           "• 切换 Rank 1：点击【Rank 1】按钮（激活时边框呈耀眼金黄色），一键开启地图最高天梯排名与特权。"
                },
                Step7 = new GuideStepInfo
                {
                    Title = "7. 游戏内置定制选项 (IN-GAME OPTIONS)",
                    Body = "点击【⚙ 游戏内置选项】按钮：\n" +
                           "• 修改你的游戏用户名 (User Name) 作为RPG联机唯一标识。\n" +
                           "• 勾选【锁定鼠标】防止窗口模式下光标滑出视野。\n" +
                           "• 勾选【16:9宽屏】与【4:3原生比例修复】获得最舒适画质。\n" +
                           "• 勾选【快速加载】大幅缩短载入时间，并开启【防刷屏】屏蔽自动存档积分弹窗。"
                },
                Step8 = new GuideStepInfo
                {
                    Title = "8. 启动游戏与自动同步 (Play Map / Play War3)",
                    Body = "• 选择图形渲染 API (OpenGL / DirectX) 及全屏/无边框/窗口模式。\n" +
                           "• Play Map（直接进图）：一键载入所选地图与对应存档，并启用安全短路径缓冲。退出游戏时自动同步最新进度回存档槽中，安全可靠！\n" +
                           "• Play War3（主菜单）：携带完整插件进入魔兽3主界面，用于局域网联机或游戏内设置。"
                }
            }
        };

        public static GuideContent GetContent(string? lang)
        {
            if (!string.IsNullOrEmpty(lang) && _contents.TryGetValue(lang, out var content))
            {
                return content;
            }
            return _contents["EN"];
        }
    }
}
