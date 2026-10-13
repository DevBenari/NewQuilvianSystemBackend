# Notifikasi Platform — Peta Kemampuan Existing

| Field | Nilai |
| --- | --- |
| Blueprint ID | `platform-notification` — **usulan**. `blueprint-manifest.md` dan `00-interview-decisions.md` belum ada; keduanya lahir pada sesi `grill-me` berikutnya |
| Revision | `1` — audit penuh pertama |
| Status | `draft` |
| Jenis audit | Audit penuh (bukan *impact scan*) |
| Sifat audit | **Read-only.** Tidak ada satu baris source aplikasi yang diubah. Basis data hanya dibaca dengan `SELECT` di dalam transaksi `READ ONLY` |
| Pemicu | Permintaan pengguna 2026-10-12: ikon lonceng (*bell*) di header harus menampilkan notifikasi. Contoh: admisi rawat inap didaftarkan dengan DPJP dr. Rendy → notifikasi muncul di akun dr. Rendy; perawat juga menerima notifikasi; ditambah kebutuhan notifikasi lain |
| Backend SHA | `05634d1f` (branch `MHamzah`). *Working tree* memuat 4 berkas source dan 2 dokumen yang belum di-commit — seluruhnya admisi/consent rawat inap. Tidak satu pun dipakai sebagai bukti di dokumen ini, sehingga bukti di bawah sama dengan isi commit |
| Frontend SHA | `7d44526ff` (branch `HamzahV2`). *Working tree* memuat 16 berkas yang belum di-commit — admisi, bed board, consent, base form control. Tidak satu pun dipakai sebagai bukti |
| Basis data | `QuilvianNewDevHamzah` (basis data pribadi pemohon). Dua skrip baca, seluruhnya `SELECT`, nol penulisan. *Connection string*, email, dan UUID tidak dicetak |
| Masukan keputusan | Rawat Inap `00-interview-decisions.md` revision `46`; Laboratorium `LAB-DEC-012`, `LAB-DEC-016`, `LAB-COORD-001`, `LAB-COORD-017`; keputusan modul lain pada [bagian 6](#6-kebutuhan-notifikasi-yang-sudah-tercatat-di-modul-lain) |
| Kontrak yang berlaku | **Belum ada.** Modul ini belum punya kontrak API, status, maupun hak akses |
| Tanggal audit | 2026-10-12 |

**Singkatan bukti.** `BE` = repository `NewQuilvianSystemBackend`, `FE` = repository
`QuilvianSystemFrontendDev`. Bentuk bukti: `repo/path#Simbol (baris)@SHA`. Bila SHA tidak ditulis,
yang berlaku adalah SHA pada tabel di atas.

> **Cara membaca dokumen ini.**
> Dokumen ini menjawab "apa yang **sudah ada** di sistem hari ini", bukan "notifikasinya nanti
> dibuat seperti apa". Setiap baris membawa bukti berupa lokasi berkas dan nama simbol, supaya
> siapa pun bisa memeriksa ulang. Dokumen ini **tidak** merancang arsitektur dan **tidak**
> memberi izin menulis kode. Rancangan baru boleh dimulai setelah keputusan pada
> [bagian 11](#11-pertanyaan-penutup-untuk-sesi-grill-me) dijawab.

---

## 1. Ringkasan untuk pembaca umum

| No | Temuan | Artinya bagi pengguna |
| ---: | --- | --- |
| 1 | Tombol lonceng **sudah ada** di header, lengkap dengan lencana angka dan efek berdenyut | Tampilannya tidak perlu dibuat ulang |
| 2 | Tombol itu **belum tersambung ke data apa pun**. Angkanya selalu 0 dan bila diklik tidak membuka apa-apa | Itulah sebabnya lonceng pada screenshot kosong |
| 3 | **Belum ada tempat menyimpan notifikasi** — tidak ada tabel, tidak ada API | Inilah pekerjaan inti yang belum ada |
| 4 | Teknologi kirim-seketika (SignalR) **sudah terpasang**, tetapi hanya dipakai untuk antrean poli, nurse station, dan IGD | Bisa dipakai ulang, dengan satu kendala server (temuan 8) |
| 5 | Data "dr. Rendy = akun login yang mana" **sudah lengkap**: 11 dari 11 dokter aktif dan 27 dari 27 pegawai aktif punya akun login yang tertaut | Contoh pada permintaan dapat diwujudkan tanpa menambah data |
| 6 | Titik kejadian di Rawat Inap **sudah jelas**: admisi dibuka, pasien menempati tempat tidur, DPJP berganti, perawat ditugaskan, pulang, batal | Tidak perlu menebak di mana notifikasi harus lahir |
| 7 | Untuk perawat, yang bisa dikenali sistem hanya **perawat penanggung jawab** yang ditugaskan kepala ruangan. "Semua perawat di bangsal tempat pasien dirawat" **belum bisa** dikenali | Notifikasi kelompok perawat butuh data penempatan perawat lebih dulu |
| 8 | Di produksi, server dipecah menjadi container **Web** (yang memegang SignalR) dan container **Worker** (yang menjalankan pekerjaan latar belakang) | Pemberitahuan yang lahir di Worker tidak bisa "didorong" seketika ke layar tanpa penyesuaian |
| 9 | Kebijakannya **sudah pernah dibahas**: Rawat Inap menunda notifikasi seketika (PRD bagian 5.4), sedangkan Laboratorium sudah menyetujui **satu kotak masuk notifikasi bersama untuk seluruh modul** (`LAB-DEC-016`) | Permintaan ini berarti membuka kemampuan yang dulu ditunda, dengan arah yang sudah disepakati |

### Contoh: apa yang terjadi pada dr. Rendy hari ini

> Pukul 14.28 petugas admisi membuka admisi rawat inap untuk pasien Tn. B (data samaran) dan
> memilih dr. Rendy sebagai DPJP. Sistem menyimpan episode berstatus `Draft` ("Sedang
> Disiapkan") dan mencatat dr. Rendy sebagai DPJP sejak detik pertama.
>
> Pukul 14.50 pasien menempati BED 001 Kamar 01. Status berubah menjadi `Admitted` ("Sedang
> Dirawat"), dan sistem mengirim kabar ke modul **Billing** supaya tagihan dibuka.
>
> Sampai di sini **tidak ada satu pun pemberitahuan untuk dr. Rendy**. Lonceng di layarnya tetap
> 0. Ia baru tahu punya pasien baru bila membuka Daftar Pasien Rawat Inap sendiri, atau dikabari
> lewat telepon. Perawat yang kemudian ditugaskan sebagai penanggung jawab juga tidak diberi
> tahu.

---

## 2. Batas audit

| Klaster kemampuan | Yang diperiksa | Cara |
| --- | --- | --- |
| Identity / Actor | Hubungan akun login ↔ dokter ↔ pegawai, klaim token, peran | Baca `ApplicationUser`, `AuthController`, `DoctorController`, `EmployeeController`, `AccessPermissionService`; kueri DB |
| Episode / Transaction Owner | Admisi, penempatan bed, penugasan DPJP/konsulen/dokter jaga/perawat, pulang, batal | Baca service dan controller Rawat Inap |
| Workflow / Status | Satu pintu perubahan status episode dan riwayatnya | Baca `InpEpisodeService.ApplyStatusChangeAsync` |
| Location / Resource | Bangsal, kamar, nurse station, staf nurse station | Baca model; kueri DB |
| Documentation / Record | Tabel notifikasi, kotak masuk, bukti baca | `rg` seluruh BE; kueri katalog DB |
| Authorization / Audit | Pola hak akses endpoint "milik saya" | Baca aturan `role-access-rules.md`, `ProfileController`, filter hak akses |
| External Integration | SignalR, outbox, worker, WhatsApp/surel/SMS, Redis | Baca `Program.cs`, `QueueHub`, worker; `rg` paket |
| Frontend | Header, lonceng, Redux store, klien SignalR, toast, daftar pantau | Baca `navbars.jsx`, `store.jsx`, klien realtime |
| Keputusan modul lain | Seluruh blueprint yang menyebut "notifikasi" | `rg` pada `docs/module-blueprints/` |

**Di luar batas audit:** konfigurasi server produksi yang sebenarnya (jumlah container yang
hidup bersamaan), aplikasi mobile, *push notification* sistem operasi, dan kebijakan retensi
rekam medis. Klaster `Order / Result` dan `Financial` hanya diperiksa sejauh mereka sudah punya
"bibit" notifikasi (bagian 3.8 dan bagian 6).

---

## 3. Peta kemampuan

Setiap baris memakai tepat satu status: `Ready to reuse`, `Reuse with adapter`, `Extend`,
`Repair`, `Missing`, `Conflict`, atau `Unknown`.

### 3.1 Siapa penerimanya (Identity / Actor)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-01` | Mengetahui akun login milik seorang dokter, supaya notifikasi untuk "DPJP dr. Rendy" sampai ke akun dr. Rendy | Corporate/HumanResource (`MstDoctor`) + Identity (`ApplicationUser`) | `BE/Models/ApplicationUser.cs#ApplicationUser.DoctorId`; diisi saat akun dokter dibuat `BE/Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/DoctorController.cs#(pembuatan akun, L3874-3885)`; klaim `doctor_id` `BE/Controllers/AuthController.cs#(L1912)`. **DB:** 11 dari 11 dokter aktif punya akun aktif, termasuk ketiga dokter yang tampil pada screenshot | Ready to reuse | Tidak perlu data baru | Dokter yang belum punya akun tidak dapat menerima notifikasi; keadaan itu harus terlihat, bukan hilang diam-diam |
| `NTF-CAP-02` | Mengetahui akun login milik seorang pegawai/perawat | Corporate/HumanResource (`MstEmployee`) + Identity | `ApplicationUser.EmployeeId`; diisi `EmployeeController.cs#(pembuatan akun, L3590-3601)`; klaim `employee_id` `AuthController.cs#(L1911)`. **DB:** 27 dari 27 pegawai aktif punya akun aktif | Ready to reuse | Tidak perlu data baru | Sama dengan `NTF-CAP-01` |
| `NTF-CAP-03` | Satu layanan bersama yang menerjemahkan "dokter/pegawai ini" menjadi "akun penerima ini" | Platform (belum ada pemilik kode) | Yang ada hanya arah sebaliknya dan bersifat privat: `BE/Hubs/QueueHub.cs#ResolveCurrentDoctorIdAsync (L485)`, `#ResolveCurrentEmployeeAsync (L446)` — dari akun yang sedang login ke dokter/pegawai, dengan cadangan klaim `workforce_profile_id` lalu pencocokan email | Missing | Perlu penerjemah penerima bersama | Cadangan pencocokan email pada `QueueHub` tidak layak ditiru untuk notifikasi: dua akun dengan email serupa bisa membuat pemberitahuan pasien sampai ke orang yang salah |

### 3.2 Di mana kejadiannya lahir — Rawat Inap (Episode / Workflow)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-04` | Kejadian "admisi dibuka dan DPJP ditetapkan" | InPatientManagement | `BE/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs#OpenAdmissionAsync (L138)` — episode `Draft` dan `InpDoctorAssignment` berperan `Dpjp` urutan 1 dibuat dalam satu transaksi (L328-344, `INV-INP-03`) | Extend | Tidak ada kejadian apa pun yang diterbitkan saat `Draft` dibuat; outbox hanya diisi saat `Admitted` | **DB:** 12 dari 24 episode berakhir `Cancelled`. Notifikasi yang dikirim saat `Draft` bisa menjadi "pasien hantu" bila admisinya batal tanpa kabar susulan |
| `NTF-CAP-05` | Satu titik tempat seluruh perubahan status episode terjadi | InPatientManagement | `InpEpisodeService.cs#ApplyStatusChangeAsync (L622)` — menurut komentarnya sendiri "satu-satunya tempat `EpisodeStatus` boleh berubah", selalu menulis `InpStatusHistory`. Dipanggil oleh `InpBedOccupancyService.cs (L674, Draft→Admitted)`, `InpDischargeService.cs (L218, Admitted→DischargePending)`, `InpDischargeService.Closure.cs (L897, →Closed)`, `InpEpisodeService.cs (L346 buka Draft; L795 →Cancelled)` | Extend | Titik paling aman untuk menambahkan "terbitkan notifikasi" karena berada dalam transaksi yang sama dengan perubahan statusnya | Menyentuh jantung modul Rawat Inap; perubahan wajib lewat task Rawat Inap yang disetujui |
| `NTF-CAP-06` | Kejadian "DPJP berganti" | InPatientManagement | `BE/.../Services/InpEpisodeService.Assignments.cs#HandoverDoctorAsync (L268)`; baris penugasan baru L383-399 | Extend | DPJP baru dan DPJP lama sama-sama kandidat penerima | — |
| `NTF-CAP-07` | Kejadian "konsulen atau dokter jaga dilibatkan / diakhiri" | InPatientManagement | `InpEpisodeService.Assignments.cs#AssignSupportingDoctorAsync (L602; baris baru L759-776)`, `#EndSupportingAssignmentAsync (L820)`; peran `Consultant` dan `OnCallDoctor` pada `InpDoctorAssignmentRole` | Extend | — | Dokter jaga berganti mengikuti shift; tanpa aturan, satu pasien bisa memicu banyak notifikasi berurutan |
| `NTF-CAP-08` | Kejadian "perawat penanggung jawab ditugaskan" | InPatientManagement | `InpEpisodeService.Assignments.cs#AssignNurseAsync (L937; baris baru L1029)` — hanya kepala ruangan/supervisor; penugasan lama ditutup, baru dibuka. `RWI-DEC-032`: ketiadaan perawat tidak menahan apa pun | Extend | — | **DB:** 4 dari 7 episode `Admitted` belum punya perawat penanggung jawab aktif — bila notifikasi perawat hanya lewat jalur ini, mayoritas pasien hari ini tidak punya perawat penerima |
| `NTF-CAP-09` | Memakai outbox integrasi Rawat Inap sebagai antrean notifikasi | InPatientManagement → Billing | `BE/.../Models/InpIntegrationOutbox.cs#MarkPublished` — "hanya dipanggil setelah Billing mengembalikan tanda terima" (`INV-RWF-04`); isi pesan dibatasi daftar putih (`INV-RWF-05`, `InpIntegrationOutboxService.cs#InpIntegrationOutboxPayload.AllowedFields`); dikirim `InpatientIntegrationOutboxWorker`. **DB:** isinya hanya `ADMISSION_CONFIRMED` (6), `BED_OCCUPIED` (6), `OCCUPANCY_CORRECTED` (2), seluruhnya `Published` | Conflict | Tabel ini milik kontrak `integrasi-billing`; status `Published` berarti "Billing sudah menerima". **Polanya** (outbox satu transaksi + worker + coba ulang) boleh ditiru, **tabelnya** tidak boleh dipinjam | Bila dipinjam, pesan notifikasi tidak akan pernah mendapat tanda terima Billing sehingga terus dianggap gagal dan berakhir `DeadLetter` |

### 3.3 Penerima berupa kelompok (Location / Resource)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-10` | Mengirim ke "perawat di bangsal tempat pasien dirawat" | Belum ada pemilik data (kandidat: Administrator/Nurse Station bersama Keperawatan) | `InpEpisode.ServiceUnitId` — **DB:** seluruh 24 episode bernilai unit "Rawat Inap", tidak membedakan bangsal/lantai. `BE/Areas/Administrator/MasterData/Models/MstNurseStationCluster.cs` punya `ServiceUnitId`; **DB:** cluster "Rawat Inap Lantai 2/3/4" ada tetapi **staf aktifnya 0** (staf hanya terisi di IGD Triage 1 orang dan Rawat Jalan 5 orang). `BE/Areas/HealthServices/MasterData/Models/MstRoom.cs` hanya punya `FloorName` berupa teks, tanpa rujukan ke nurse station | Missing | Butuh data "perawat X bertugas di bangsal Y" sebelum notifikasi kelompok perawat mungkin | Tanpa data itu, satu-satunya pilihan adalah "semua perawat Rawat Inap", yang berarti banjir notifikasi untuk orang yang tidak merawat pasiennya |
| `NTF-CAP-11` | Mengirim ke "kepala ruangan" atau "supervisor" | Platform Authorization + Rawat Inap | `BE/.../Helpers/InpatientActorClaims.cs#SupervisorOrWardHeadRoles (L35-41)` mengasumsikan peran `KepalaRuangan`/`Kepala Ruangan`, dan komentarnya sendiri menyebut ini "asumsi yang masih perlu dikonfirmasi". **DB:** peran yang ada hanya `SuperAdmin` (13 pengguna aktif), `Supervisor` (12), `Manajer Finance` (0), `Supervisor Finance` (0), `User` (0) | Conflict | Peran kepala ruangan tidak ada di data, dan peran `Supervisor` dipegang 12 orang tanpa batas bangsal | Notifikasi "untuk kepala ruangan" akan sampai ke nol orang, atau ke 12 supervisor sekaligus |

### 3.4 Penyimpanan dan API notifikasi (Documentation / Record)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-12` | Tabel pemberitahuan tersimpan per pengguna (kotak masuk), lengkap dengan status sudah dibaca | Platform (`LAB-DEC-016`) | Tidak ada `DbSet<...Notification...>` maupun kelas `*Notification*` di `BE@05634d1f`. **DB:** tidak ada tabel bernama `%notif%`, `%inbox%`, atau `%pemberitahuan%`. Sejalan dengan `laboratorium/01-existing-capability-map.md#CAP-18` | Missing | Seluruhnya baru | — |
| `NTF-CAP-13` | API kotak masuk: daftar, jumlah belum dibaca, tandai sudah dibaca | Platform | Tidak ada controller atau route bertema notifikasi di BE | Missing | Seluruhnya baru | — |
| `NTF-CAP-14` | Bukti "sudah ditindaklanjuti" (*acknowledgement*) untuk kejadian kritis, bukan sekadar "sudah dibaca" | Platform + modul sumber | Polanya sudah ada di kotak masuk milik modul: `BE/Areas/HealthServices/BloodBankManagement/Models/BbkTransfusionReactionNotice.cs` dengan `AcknowledgeReactionNoticeRequest`; `BE/Areas/HealthServices/ClinicalManagement/Models/TrxPatientVitalSign.cs#DoctorNotifiedAt, DoctorNotifiedByUserId` (dicatat manual oleh perawat) | Reuse with adapter | Kedua pola itu milik modulnya masing-masing; kotak masuk platform perlu membedakan "dibaca" dan "di-acknowledge" | Menyamakan "dibaca" dengan "di-acknowledge" berbahaya untuk nilai kritis — IGD butir 71 dan `LAB-DEC-004` menuntut acknowledgement yang dapat dibuktikan |

### 3.5 Pengiriman seketika dan pekerjaan latar belakang (External Integration)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-15` | Saluran SignalR yang sudah terpasang dan terautentikasi | Platform / RegistrationManagement | `BE/Program.cs` — `AddSignalR` (L245), `OnMessageReceived` membaca cookie `quilvian_access_token` lalu token query string (L351-367), CORS `FrontendCorsPolicy` dengan `AllowCredentials` (L312-318), `MapHub<QueueHub>("/hubs/queues")` (L2151). `BE/Hubs/QueueHub.cs#QueueHub` ber-`[Authorize]` dengan grup per dokter, poli, nurse station, dan IGD | Extend | Belum ada grup per pengguna (tidak ada `Clients.User` maupun `IUserIdProvider`). Cadangan token lewat query string hanya berlaku untuk path `/hubs/queues`; jalur cookie tetap berlaku untuk path lain | — |
| `NTF-CAP-16` | Mendorong notifikasi seketika dari pekerjaan latar belakang | Platform | `Program.cs (L120-131)` peran runtime `All`/`Web`/`Worker`; komentar L1100-1107: *"Pada Blue-Green Production, BLUE dan GREEN memakai Web, sedangkan hanya satu container terpisah memakai Worker"*. Hub hanya dipetakan bila `runWebRuntime` (L2149-2152). Tidak ada *backplane* SignalR; Redis hanya dipakai untuk cache (`AddStackExchangeRedisCache`, L301) | Conflict | Notifikasi yang lahir di container Worker tidak dapat didorong ke browser, karena browser tersambung ke container Web | Bila tidak diperhitungkan, pemberitahuan "kadang muncul seketika, kadang tidak", tergantung proses yang membuatnya |
| `NTF-CAP-17` | Pola pekerjaan latar belakang (buka *scope*, coba ulang, sewa pemrosesan) | Berbagai modul | `InpatientIntegrationOutboxWorker`, `CliTransfusionReactionNoticeWorker`, dan sekitar 20 `AddHostedService` lain (`Program.cs` L715-717, L1110-1138) | Ready to reuse | Yang dipakai ulang adalah polanya, bukan komponennya | Hanya berjalan pada peran `Worker` atau `All` |
| `NTF-CAP-18` | Kanal di luar aplikasi: WhatsApp, surel, SMS | Platform (`LAB-COORD-011`) | Tidak ada SMTP/MailKit/SendGrid/Twilio/gerbang WhatsApp di BE; `WhatsAppNumber` hanya kolom data | Missing | — | Tidak diminta oleh permintaan saat ini |

### 3.6 Hak akses (Authorization / Audit)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-19` | Pola hak akses untuk endpoint "milik saya sendiri" | Platform Authorization | `BE/Areas/SelfServices/Account/Controllers/ProfileController.cs` memakai `[Authorize]` + `[AccessController]` + `[AccessPermission("Profile", "Read")]`; aturan `rules/backend/role-access-rules.md` bagian 2; `BE/Services/Security/AccessPermissionService.cs#HasAccessAsync` — `SuperAdmin` lolos, peran lain wajib diberi akses | Ready to reuse | Resource baru wajib diberikan ke setiap peran pemakai | Bila lupa diberikan, lonceng menghasilkan kode 403 (tidak punya hak akses) untuk dokter dan perawat — hanya SuperAdmin yang melihat notifikasinya |

### 3.7 Tampilan (Frontend)

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-20` | Tombol lonceng dengan lencana jumlah di header | Frontend shell | `FE/src/components/features/navbars/navbars.jsx#Navbars` — tombol L1282-1313 (lencana dibatasi `99+`, label aksesibilitas ID/EN), `selectUnreadNotificationCount` L722-741, `handleNotificationClick` L1170-1195. `FE/src/components/features/navbars/navbars-wrapper.jsx#NavbarsWrapper (L36)` merender `<Navbars />` tanpa props | Reuse with adapter | Angka dibaca dari `state.Notification.unreadCount` dan 9 kandidat nama lain yang **tidak ada** di store, sehingga selalu 0. Klik hanya memancarkan event `navbar-notification-click` yang tidak didengar komponen mana pun | — |
| `NTF-CAP-21` | Panel daftar notifikasi saat lonceng diklik | Frontend | Tidak ada pendengar event `navbar-notification-click`; tidak ada komponen panel | Missing | Seluruhnya baru | — |
| `NTF-CAP-22` | State Redux notifikasi | Frontend | `FE/src/lib/state/store.jsx` (503 baris) tidak punya reducer `Notification` maupun `ComplianceAlert` | Missing | Nama kunci yang sudah "ditunggu" navbar: `Notification.unreadCount` | — |
| `NTF-CAP-23` | Klien SignalR yang terbukti jalan | Frontend | `FE/src/lib/realtime/queue-realtime-client.js#createQueueRealtimeConnection` — `withCredentials`, `accessTokenFactory`, sambung ulang otomatis 0/2/5/10/30 detik, bergabung ulang ke grup setelah tersambung kembali (L200-204); dipakai 5 hook antrean dan IGD | Reuse with adapter | Terikat path `/hubs/queues` dan nama event antrean | — |
| `NTF-CAP-24` | Klien SignalR generik `signalrHubClient` | Frontend | `FE/src/lib/signalr/signalrHubClient.jsx (L2)` mengimpor `./axiosInstance/InstanceAxios`, padahal berkasnya ada di `src/lib/axiosInstance/`; nol pemakai | Repair | Jalur impor rusak — bila dipakai apa adanya, build gagal | — |
| `NTF-CAP-25` | Toast pemberitahuan seketika di pojok layar | Frontend | `FE/src/components/features/toast/toast-utils.jsx` (react-toastify 11, ikon bawaan `FaBell`); `ToastProvider` terpasang global di `FE/src/app/layout.js (L46-49)` | Ready to reuse | — | — |
| `NTF-CAP-26` | Daftar pantau yang disegarkan berkala (mekanisme "tarik") | Rawat Inap | `BE/.../Controllers/InpatientMonitoringController.cs` (10 endpoint, bagian 5.4); menu "Daftar Pantau" `FE/src/utils/menu-sidebar/menu-items.jsx (L1692)` | Ready to reuse | Tetap dipakai berdampingan. Lonceng bukan pengganti daftar pantau | Menyalin isi daftar pantau ke lonceng setiap penyegaran akan menghasilkan notifikasi berulang untuk hal yang sama |

### 3.8 Modul lain yang sudah punya "bibit" notifikasi

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `NTF-CAP-27` | Tanda vital kritis atau skor EWS tinggi → dokter perlu tahu | ClinicalManagement | `TrxPatientVitalSign.NeedDoctorNotification` disetel otomatis bila kritis atau EWS `High`/`Critical` (`BE/.../ClinicalManagement/Controllers/PatientVitalSignController.cs` L564, L713); `GET critical-alerts`; `PATCH {id}/notify-doctor` = perawat **mencatat** bahwa dokter sudah dikabari | Reuse with adapter | Saat ini pencatatan manual, bukan pengiriman | Kejadian klinis kritis — butuh keputusan acknowledgement (`NTF-CAP-14`) |
| `NTF-CAP-28` | Persetujuan HR yang menunggu | Corporate/HumanResource — WorkflowManagement | `BE/Areas/Corporate/HumanResource/WorkflowManagement/Controllers/ApprovalInboxController.cs` (`GET summary`, `GET`, `GET delegated-to-me`, aksi approve/reject); nol pemakai di FE | Reuse with adapter | Dapat menjadi sumber angka di lonceng; FE belum memakai kotak persetujuan sama sekali | — |
| `NTF-CAP-29` | Kotak masuk reaksi transfusi Bank Darah | BloodBankManagement | `BbkTransfusionReactionNotice` + `CliTransfusionReactionNoticeWorker` (`RWI-DEC-221`) | Reuse with adapter | Ini kotak masuk **unit**, bukan pribadi. Lonceng boleh menunjuk ke sana, tidak menggantikannya | — |
| `NTF-CAP-30` | Lonceng khusus persetujuan diskon dokter | Billing (peninggalan lama) | `FE/src/components/features/navbars/DiskonApprovalNotifBell.jsx` — mengimpor `@/lib/hooks/useDiskonApprovalNotif` yang tidak ada (L19), menuju rute `/kasir/diskon-approval/dokter/*` yang tidak ada di `src/app` (L26-27); nol pemakai | Conflict | Jangan dihidupkan kembali: ia membuat lonceng kedua khusus satu modul. Tata letak dropdown-nya boleh dijadikan referensi tampilan | Bertentangan dengan `LAB-DEC-016` "satu kotak masuk untuk seluruh modul" |

---

## 4. Perjalanan ujung-ke-ujung yang material (as-is)

### 4.1 Admisi rawat inap dengan DPJP

| Unsur | Isi |
| --- | --- |
| Tujuan | Membuka episode rawat inap untuk pasien, dengan DPJP yang jelas sejak awal |
| Pelaku | Petugas admisi membuka admisi; petugas admisi atau bangsal menempatkan pasien ke tempat tidur; kepala ruangan atau supervisor menugaskan perawat penanggung jawab, konsulen, dan dokter jaga serta mengganti DPJP; DPJP memutuskan pulang |
| Pemicu | Dokter memutuskan pasien dirawat inap, dari IGD, poliklinik, atau kamar pulih |
| Prasyarat | Pasien terdaftar; dokter DPJP aktif. Agar notifikasi kelak bisa sampai, dokter itu harus punya akun login (hari ini 11 dari 11 dokter aktif sudah punya) |

**Langkah hari ini:**

1. Petugas admisi mengisi formulir Admisi Rawat Inap, memilih dr. Rendy sebagai DPJP, lalu
   menyimpan. Layar memanggil `POST /episodes` (`OpenAdmission`).
2. Sistem membuat episode `Draft` ("Sedang Disiapkan"), satu baris `InpDoctorAssignment` berperan
   `Dpjp` urutan 1, dan satu baris riwayat status — semuanya dalam satu transaksi.
3. **Celah:** tidak ada pemberitahuan apa pun untuk dr. Rendy.
4. Petugas menempatkan pasien ke tempat tidur (`POST /bed-occupancies/placements`).
   Status berubah menjadi `Admitted` ("Sedang Dirawat"). Sistem menulis `ADMISSION_CONFIRMED` dan
   `BED_OCCUPIED` ke outbox — **untuk Billing**, bukan untuk manusia.
5. **Celah:** dr. Rendy dan perawat tidak diberi tahu bahwa pasien sudah di tempat tidur.
6. Kepala ruangan menugaskan perawat penanggung jawab (`POST /episodes/{id}/nurse-assignments`).
7. **Celah:** perawat yang ditugaskan tidak diberi tahu. Episode yang belum punya perawat hanya
   muncul di daftar pantau kepala ruangan (`GET /monitoring/unassigned-nurse-episodes`).

**Perubahan status hari ini, dan apakah ada pemberitahuannya:**

| Dari status | Tindakan (`actionType`) | Ke status | Siapa yang boleh (hak akses) | Endpoint | Pemberitahuan hari ini |
| --- | --- | --- | --- | --- | --- |
| — | `OpenAdmission` / `OpenAdmissionWithEncounter` | `Draft` | Petugas admisi (`InpatientEpisode : Create`) | `POST /episodes` | Tidak ada |
| `Draft` | `PlacePatient` | `Admitted` | Petugas yang menempatkan pasien, admisi atau bangsal (`InpatientBedOccupancy : Create`) | `POST /bed-occupancies/placements` | Hanya outbox Billing |
| `Admitted` | `DecideDischarge` | `DischargePending` | DPJP (`InpatientDischarge : Update`) | `POST /discharges/{episodeId}/decide` | Tidak ada; kasir melihat lewat penyegaran 10 detik |
| `DischargePending` | `CloseEpisode` / `CloseEpisodeWithOverride` | `Closed` | `InpatientDischarge : Close` / `CloseOverride` | `POST /discharges/{episodeId}/close`, `/close-with-override` | Tidak ada |
| `Draft` | `CancelAdmission` | `Cancelled` | `InpatientEpisode : Update` | `PATCH /episodes/{id}/cancel` | Tidak ada |
| `Draft` | `ExpireDraft` | `Cancelled` | Sistem, saat episode dibaca melewati batas jam | — | Tidak ada |

**Jalur tidak normal yang relevan bagi notifikasi:**

| Keadaan | Bukti | Akibat bagi notifikasi kelak |
| --- | --- | --- |
| Admisi `Draft` dibatalkan atau gugur | 12 dari 24 episode di DB berakhir `Cancelled` | DPJP yang sudah dikabari "pasien baru" perlu dikabari bahwa admisinya batal |
| DPJP berganti sebelum pasien menempati tempat tidur | `HandoverDoctorAsync` hanya menolak episode `Closed`, `Cancelled`, atau `Draft` yang sudah gugur — jadi boleh pada `Draft` | Pemberitahuan pertama menjadi usang |
| Dokter belum punya akun | Pembuatan akun dokter terpisah dari master dokter | Pemberitahuan tidak punya penerima |
| Perawat belum ditugaskan | 4 dari 7 episode `Admitted` | Tidak ada perawat penerima |
| Koneksi realtime putus | Klien antrean menyambung ulang 0/2/5/10/30 detik | Hanya pemberitahuan yang **tersimpan** yang tidak hilang |

**Hasil akhir hari ini:** data admisi, penugasan, dan riwayat status tercatat rapi dan dapat
diaudit. Yang tidak tercatat sama sekali adalah **siapa sudah diberi tahu dan kapan**.

### 4.2 Contoh kebutuhan dari pemohon (bukan rancangan)

> Admisi didaftarkan dengan DPJP dr. Rendy. Di layar dr. Rendy, lonceng menunjukkan angka 1.
> Saat diklik muncul: "Pasien baru atas nama Anda sebagai DPJP — Tn. B, RI-2610xx, Rawat Inap
> Kelas I." Hal yang sama berlaku untuk perawat yang ditugaskan merawat pasien itu.

Bentuk kalimat, kejadian mana saja yang memicu, dan siapa saja perawatnya **belum diputuskan**
(bagian 11).

---

## 5. Kontrak as-is yang menjadi sumber kejadian (gaya Swagger)

Endpoint di bawah **sudah ada**. Tidak ada endpoint notifikasi yang ditulis di sini, karena
memang belum ada.

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuka admisi rawat inap beserta DPJP-nya | `InpatientEpisode : Create` | Body `OpenAdmissionRequest` | `InpatientEpisodeDetailResponse` |
| `PATCH` | `/{id}/cancel` | Membatalkan admisi `Draft` | `InpatientEpisode : Update` | Body `CancelAdmissionRequest` | `InpatientEpisodeDetailResponse` |
| `POST` | `/{id}/doctor-assignments` | Mengganti DPJP (serah terima) | `InpatientEpisode : Update` | Body `HandoverDoctorRequest` | `InpatientDoctorAssignmentResponse` |
| `POST` | `/{id}/doctor-assignments/supporting` | Melibatkan konsulen atau dokter jaga | `InpatientEpisode : Update` | Body `AssignSupportingDoctorRequest` | `InpatientDoctorAssignmentResponse` |
| `PATCH` | `/{id}/doctor-assignments/{assignmentId}/end` | Mengakhiri penugasan konsulen/dokter jaga | `InpatientEpisode : Update` | Body `EndSupportingAssignmentRequest` (boleh kosong) | `InpatientDoctorAssignmentResponse` |
| `POST` | `/{id}/nurse-assignments` | Menugaskan perawat penanggung jawab | `InpatientEpisode : Update` | Body `AssignNurseRequest` | `InpatientNurseAssignmentResponse` |
| `GET` | `/{id}/status-history` | Membaca riwayat perubahan status episode | `InpatientEpisode : Read` | - | `List<InpatientStatusHistoryResponse>` |

Kode status yang mungkin muncul: **400** isian tidak lengkap atau formatnya salah; **403** pengguna
tidak punya hak akses, atau bukan kepala ruangan/supervisor untuk penugasan; **404** episode tidak
ditemukan; **409** episode sudah ditutup/dibatalkan/gugur, atau ada penugasan lain yang tersimpan
pada saat bersamaan; **422** aturan bisnis menolak, misalnya perawat yang dipilih sudah menjadi
penanggung jawab.

### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/placements` | Menempatkan pasien ke tempat tidur; episode `Draft` menjadi `Admitted` | `InpatientBedOccupancy : Create` | Body `PlacePatientRequest` | `BedPlacementResponse` |
| `POST` | `/placements/transfer` | Memindahkan pasien ke tempat tidur lain | `InpatientBedOccupancy : Transfer` | Body `TransferPatientRequest` | — (tidak diaudit rinci) |

Kode status `POST /placements`: **400** isian tidak lengkap; **404** episode atau tempat tidur tidak
ditemukan; **409** tempat tidur sudah dipakai pasien lain pada saat hampir bersamaan; **422** aturan
bisnis menolak penempatan (pesan penolakannya dikirim apa adanya ke layar).

### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/decide` | DPJP memutuskan pasien boleh pulang | `InpatientDischarge : Update` | Body `DecideDischargeRequest` | `InpatientEpisodeDetailResponse` |
| `POST` | `/{episodeId}/close` | Menutup episode | `InpatientDischarge : Close` | Body `CloseEpisodeRequest` (boleh kosong) | `InpatientEpisodeDetailResponse` |
| `POST` | `/{episodeId}/close-with-override` | Menutup episode tanpa kelayakan keuangan | `InpatientDischarge : CloseOverride` | Body | — (tidak diaudit rinci) |

Kode status: **400** isian tidak lengkap; **403** "Hanya DPJP episode ini yang dapat menyatakan
pasien boleh pulang" (khusus `decide`); **404** episode tidak ditemukan; **409** status episode
sudah berubah lebih dulu; **422** syarat penutupan belum terpenuhi — sistem menyebut butir mana
saja yang belum beres.

### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

Seluruhnya `GET`, hak akses `InpatientMonitoring : Read`, mengembalikan daftar berhalaman.

| Path | Kegunaan (daftar pantau yang sudah ada) | Response |
| --- | --- | --- |
| `/isolation-mismatch` | Pasien yang butuh isolasi tetapi tidak di ruang isolasi | `IsolationMismatchPagedResult` |
| `/pending-closures` | Episode yang menunggu ditutup | `PendingClosurePagedResult` |
| `/departures-before-clearance` | Pasien pulang sebelum izin kasir | — |
| `/closures-without-financial-clearance` | Episode ditutup tanpa kelayakan keuangan | `OverrideClosurePagedResult` |
| `/unassigned-nurse-episodes` | Episode tanpa perawat penanggung jawab | `UnassignedNursePagedResult` |
| `/bed-drift` | Status tempat tidur yang tidak cocok dengan hunian | `BedDriftPagedResult` |
| `/billed-pending-procedure-orders` | Order tindakan yang sudah ditagih tetapi belum dikerjakan | `BilledPendingProcedureOrderPagedResult` |
| `/deposit-shortfall` | Deposit pasien kurang | `DepositShortfallPagedResult` |
| `/pending-surgical-handovers` | Serah terima operasi yang tertunda | `PagedResult<HandoverQueueItemResponse>` |
| `/pending-admission-referrals` | Permintaan admisi yang belum diproses | `PagedResult<AdmissionReferralResponse>` |

Kode status: **200** daftar berhasil dibaca (boleh kosong); **403** pengguna tidak punya hak akses
`InpatientMonitoring : Read`.

### Health Services / Clinical Management / Patient Vital Sign

Base URL: `api/v1/health-services/clinical-management/patient-vital-signs`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/critical-alerts` | Melihat tanda vital kritis yang perlu dikabarkan ke dokter | `PatientVitalSign : Read` | Query `patientId`, `encounterId`, `onlyNeedDoctorNotification` (bawaan `true`) | `List<PatientVitalSignAlertResponse>` |
| `PATCH` | `/{id}/notify-doctor` | Perawat mencatat bahwa dokter sudah dikabari | `PatientVitalSign : NotifyDoctor` | Body `NotifyDoctorPatientVitalSignRequest` | `object` |

Kode status: **200** berhasil; **403** tidak punya hak akses; **404** catatan tanda vital tidak
ditemukan. Perhatikan: `notify-doctor` **tidak mengirim apa pun** — ia hanya mencatat waktu dan
petugas yang mengabari dokter di luar sistem.

### Corporate / Human Resource / Workflow Management / Approval Inbox

Base URL: `api/v1/corporate/human-resource/approval-inbox`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/summary` | Ringkasan jumlah persetujuan yang menunggu | `ApprovalInbox : Read` | - | `ApprovalInboxSummaryResponse` |
| `GET` | `/` | Daftar persetujuan yang menunggu | `ApprovalInbox : Read` | Query saringan | Daftar berhalaman |
| `GET` | `/delegated-to-me` | Persetujuan yang didelegasikan kepada saya | `ApprovalInbox : Read` | Query | Daftar |

Kode status: **200** berhasil; **401** belum login atau sesi habis; **403** tidak punya hak akses.

### Kanal realtime `/hubs/queues` (bukan bagian Swagger)

| Jenis | Nama | Kegunaan | Penjaga akses |
| --- | --- | --- | --- |
| Metode | `JoinDoctorQueue(doctorId)` / `JoinAccessibleDoctorQueues()` | Bergabung ke grup antrean dokter | Hanya dokter itu sendiri, atau SuperAdmin |
| Metode | `JoinNurseStationCluster(id)` / `JoinAccessibleNurseStationClusters()` | Bergabung ke grup nurse station | Staf cluster, atau SuperAdmin |
| Metode | `JoinDoctorClinicQueue(clinicId)` / `JoinAccessibleDoctorClinicQueues()` | Bergabung ke grup poli | Dokter yang berjadwal di poli itu |
| Metode | `JoinEmergencyQueue()` | Bergabung ke grup IGD | Login saja |
| Event | `QueueRealtimeEvent`, `EmergencyRealtimeEvent` | Kabar perubahan antrean | — |

Tidak ada metode maupun event untuk pemberitahuan pribadi.

---

## 6. Kebutuhan notifikasi yang sudah tercatat di modul lain

Bagian ini menjawab "kebutuhan notifikasi lain" pada permintaan. Seluruhnya diambil dari
dokumen keputusan yang sudah ada — **tidak ada yang dikarang**.

| No | Modul | Kejadian | Penerima | Status keputusan | Sumber | Keadaan source |
| ---: | --- | --- | --- | --- | --- | --- |
| 1 | Laboratorium | Nilai kritis dan koreksi hasil | Dokter tujuan | `approved` — pemberitahuan **tersimpan**, SignalR hanya pelengkap | `LAB-DEC-012`, BR-08 | Belum dibangun; tertahan `LAB-COORD-017` (menunggu kemampuan platform) |
| 2 | Laboratorium | Hasil `Definitif` menyusul `Sementara` yang sudah dirilis | Dokter pemesan | `approved` | `LAB-DEC-173`, BR-121 | Belum dibangun |
| 3 | Seluruh modul | Kotak masuk **satu untuk seluruh modul**, milik platform; Laboratorium pemakai pertama | Setiap pengguna | `approved` (pemilik Lab); pemilik platform menyepakati lewat `LAB-REQ-001` | `LAB-DEC-016`, BR-12, `LAB-COORD-001` | Nol |
| 4 | Rawat Inap — Keperawatan | Dugaan reaksi obat | Usulan: DPJP aktif dan apoteker | `PROPOSED` — sementara tampil di daftar pantau | Gate `G-15`, `RWI-DOK-RQG-001` | Daftar pantau ada; notifikasi tidak |
| 5 | Rawat Inap — Keperawatan | Instruksi sliding scale "lapor dokter" | Belum ditetapkan | `MISSING` | Gate `G-24` | Nol |
| 6 | Rawat Inap — Integrasi Billing | Status kasir dan izin pulang | Bangsal dan kasir | `DEFERRED` (PRD 5.4) — diganti penyegaran 10/30 detik | `integrasi-billing/04-prd-to-mvp.md`, gate `G-02`, `G-INT-04` | Penyegaran berkala ada |
| 7 | Rawat Inap — Bank Darah | Reaksi transfusi | Kotak masuk Bank Darah | `approved` | `RWI-DEC-221`, gate `G-23` | Ada (`BbkTransfusionReactionNotice`) |
| 8 | IGD | Hasil kritis: notifikasi, acknowledgement terbukti, eskalasi berjenjang | Pemilik review → dokter pengganti → otoritas klinis | `draft` | `igd/00-interview-decisions.md` butir 71-72, 74 | Nol |
| 9 | IGD | Mode bencana diaktifkan | Direktur dan komite bencana | `draft` | `igd/00-interview-decisions.md` (tabel aktor) | Nol |
| 10 | Kamar Operasi | Jadwal operasi dibuat, diubah, ditunda, dibatalkan; pasien siap diserahterimakan | Tim/unit terdampak; kanal dapat diatur | `approved` | `OPS-DEC-024` | Nol |
| 11 | HR | Pemanggilan kembali (*recall*) disetujui | Pegawai | `approved` | `HRD-DEC-024` | Belum diaudit apakah sudah dikirim |
| 12 | HR | Koreksi kehadiran dibuat HR atas nama pegawai | Pegawai | `approved` | `HRD-DEC-028` | Belum diaudit |
| 13 | Billing Kasir | Invoice kembali terbuka karena tender kartu/gateway ditarik | Kasir | `approved` | `BKC-DEC-130` | Belum diaudit |
| 14 | Finance | Pemisahan pegawai (resign/PHK) | Finance | `approved` — berupa **event antar-sistem**, bukan pemberitahuan untuk manusia | `FIN-DEC-194` | — |
| 15 | Farmasi | Obat siap diambil | **Pasien**, bukan staf | `open` | `PHA-OQ-009` | Di luar lonceng staf |
| 16 | Klinis | Tanda vital kritis / EWS tinggi | Dokter | Tercatat sebagai penanda `NeedDoctorNotification` | `NTF-CAP-27` | Pencatatan manual ada |

**Contoh untuk memahami bedanya penerima "pribadi" dan "unit".** Notifikasi nomor 1 ditujukan
kepada **satu dokter tertentu** (dr. Rina, pemesan pemeriksaan Kalium). Notifikasi nomor 7
ditujukan kepada **unit** Bank Darah, siapa pun petugas yang sedang berjaga. Kotak masuk
pribadi dan kotak masuk unit adalah dua hal berbeda; modul ini baru menemukan bukti untuk yang
pribadi.

---

## 7. Ketidakcocokan frontend dan backend

| No | Ketidakcocokan | Bukti | Akibat |
| ---: | --- | --- | --- |
| 1 | Navbar menunggu angka dari `state.Notification.unreadCount`, tetapi tidak ada reducer maupun API yang mengisinya | `NTF-CAP-20`, `NTF-CAP-22` | Lencana selalu 0 |
| 2 | Navbar memancarkan event `navbar-notification-click`, tidak ada yang mendengarnya | `navbars.jsx` L1188-1192 | Klik lonceng tidak berbuat apa-apa |
| 3 | `DiskonApprovalNotifBell` menunjuk hook dan rute yang tidak ada | `NTF-CAP-30` | Bila diimpor, build gagal |
| 4 | `signalrHubClient.jsx` jalur impornya rusak | `NTF-CAP-24` | Bila diimpor, build gagal |
| 5 | Kotak persetujuan HR ada di backend, tidak ada konsumen di frontend | `NTF-CAP-28` | Persetujuan menunggu tidak terlihat oleh pegawai |
| 6 | Klien antrean mengirim token lewat `accessTokenFactory`, tetapi backend hanya menerima token query string untuk path `/hubs/queues`; jalur cookie berlaku untuk semua path | `Program.cs` L360-366 | Hub baru akan bergantung pada cookie, kecuali aturan itu diperluas |

---

## 8. Conflict dan Unknown

### Conflict

| ID | Temuan | Dampak | Siapa yang memutuskan |
| --- | --- | --- | --- |
| `NTF-CF-01` | Rawat Inap menyatakan "notifikasi seketika (WebSocket) `DEFERRED` menurut PRD bagian 5.4" (`rawat-inap/00-interview-decisions.md` L327; `integrasi-billing/04-prd-to-mvp.md` L42, L236, L346). Permintaan ini meminta notifikasi muncul untuk DPJP dan perawat rawat inap | Penundaan itu harus **dibuka secara tercatat** oleh pemilik Rawat Inap sebelum sumber kejadian Rawat Inap diubah — tidak dirapikan sendiri oleh agent | Pemilik modul Rawat Inap |
| `NTF-CF-02` | Kode mengasumsikan peran `KepalaRuangan`; data hanya punya `SuperAdmin` dan `Supervisor` | Penerima "kepala ruangan" tidak dapat dikenali (`NTF-CAP-11`) | Pemilik Platform Authorization + Rawat Inap |
| `NTF-CF-03` | Outbox Rawat Inap terikat kontrak Billing (`INV-RWF-04`, `INV-RWF-05`) | Tidak boleh dipinjam sebagai antrean notifikasi (`NTF-CAP-09`) | Sudah diputuskan oleh kontrak `integrasi-billing` — dicatat, bukan ditanyakan |
| `NTF-CF-04` | Produksi Blue-Green memisahkan container Web (memegang SignalR) dan Worker | Push dari Worker tidak sampai ke browser (`NTF-CAP-16`) | Pemilik platform/DevOps |
| `NTF-CF-05` | Lonceng khusus diskon dokter (peninggalan) versus satu kotak masuk bersama | Dua lonceng di header bila dihidupkan (`NTF-CAP-30`) | Sudah diputuskan `LAB-DEC-016` — dicatat |

### Unknown

| ID | Yang belum diketahui | Kenapa penting | Cara menutup |
| --- | --- | --- | --- |
| `NTF-UK-01` | Apakah produksi saat ini benar berjalan `Web` + `Worker` terpisah, dan apakah BLUE dan GREEN pernah melayani bersamaan | Menentukan perlu tidaknya *backplane* SignalR | Konfirmasi pemilik DevOps; baca konfigurasi `Runtime:Role` di server |
| `NTF-UK-02` | Perawat mana bertugas di bangsal mana, dan apakah perawat berpindah bangsal per shift | Syarat notifikasi kelompok perawat (`NTF-CAP-10`) | Wawancara Keperawatan/HR; pengisian staf nurse station Rawat Inap |
| `NTF-UK-03` | Apakah satu orang dapat memegang dua akun (misalnya akun dokter dan akun pegawai) | Pemberitahuan ganda ke orang yang sama | Kueri `AspNetUsers` per `WorkforceProfileId` pada data produksi |
| `NTF-UK-04` | Berapa lama pemberitahuan wajib disimpan | Pemberitahuan nilai kritis dapat menjadi bukti mutu dan akreditasi | Pemilik rekam medis / mutu |
| `NTF-UK-05` | Seberapa sering admisi `Draft` dibatalkan di produksi (di DB dev 12 dari 24) | Menentukan pemberitahuan dikirim saat `Draft` atau saat `Admitted` | Data produksi atau keputusan pemilik |

---

## 9. Keputusan yang diwarisi (tidak dibuka ulang)

| Keputusan | Isi yang mengikat modul ini |
| --- | --- |
| `LAB-DEC-012` (`approved`) | Pemberitahuan **disimpan** sebagai data milik penerima, lengkap dengan status dibaca. SignalR hanya pelengkap, bukan satu-satunya jalur |
| `LAB-DEC-016` (`approved`) | Kemampuan platform bersama; satu kotak masuk untuk seluruh modul; struktur data umum tanpa istilah khas satu modul |
| `LAB-COORD-001` (`closed`) | Pemilik platform (`andryzainhome`, `sukmagp`) menyepakati kepemilikan platform, 2026-09-01 |
| `RWI-DEC-032` | Ketiadaan perawat penanggung jawab tidak menahan proses apa pun |
| `INV-RWF-04`, `INV-RWF-05` | Outbox Rawat Inap hanya untuk Billing, isi pesan daftar putih |
| IGD butir 72 dan 74 (`draft`) | Isi pemberitahuan sekecil yang diperlukan; log pemberitahuan tidak menyimpan hasil lengkap atau data kesehatan yang tidak perlu |

---

## 10. Pemicu impact scan

Peta ini menjadi `STALE` dan wajib dipindai ulang secara terbatas bila salah satu berkas berikut
berubah sesudah SHA yang diaudit:

| Repository | Path |
| --- | --- |
| BE | `Hubs/**`, `Program.cs` (bagian SignalR, autentikasi JWT, peran runtime) |
| BE | `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService*.cs`, `InpBedOccupancyService.cs`, `InpDischargeService*.cs` |
| BE | `Models/ApplicationUser.cs`, `Controllers/AuthController.cs` (klaim token), `Services/Security/AccessPermissionService.cs` |
| BE | Berkas baru bernama `*Notification*` di mana pun |
| FE | `src/components/features/navbars/**`, `src/lib/state/store.jsx`, `src/lib/realtime/**`, `src/lib/signalr/**` |
| Data | Pengisian staf nurse station Rawat Inap, atau penambahan peran kepala ruangan |

---

## 11. Pertanyaan penutup untuk sesi `grill-me`

Pertanyaan berikut adalah **kebijakan baru** yang tidak dapat dijawab dari source maupun dokumen
keputusan. Rekomendasi disertakan; keputusan tetap milik pemilik kebutuhan.

| No | Pertanyaan | Rekomendasi | Alasan |
| ---: | --- | --- | --- |
| 1 | Kejadian apa saja yang masuk rilis pertama? | Mulai dari **penugasan orang**: DPJP ditetapkan saat admisi dibuka, DPJP berganti, konsulen/dokter jaga dilibatkan, perawat penanggung jawab ditugaskan | Penerimanya pasti satu orang yang datanya sudah siap (`NTF-CAP-01`, `02`); jumlahnya kecil; nilainya langsung terasa |
| 2 | "Admisi didaftarkan" itu saat `Draft` dibuka atau saat pasien menempati tempat tidur (`Admitted`)? | Kirim saat `Draft` dibuka (sesuai contoh pemohon), **ditambah** kabar susulan bila admisi itu batal atau gugur | Separuh episode di DB dev berakhir batal; tanpa kabar susulan, DPJP menyimpan "pasien hantu" |
| 3 | Perawat yang mana yang menerima? | Rilis pertama: **perawat penanggung jawab** saat ditugaskan. Kelompok perawat bangsal menyusul setelah data penempatan perawat diisi | Data perawat bangsal belum ada (`NTF-CAP-10`); peran kepala ruangan belum ada (`NTF-CAP-11`) |
| 4 | Apakah kejadian kritis (nilai lab kritis, tanda vital kritis) ikut rilis pertama? | Belum. Masuk rilis berikutnya bersama aturan acknowledgement modul pemiliknya | "Dibaca" tidak sama dengan "ditindaklanjuti" (`NTF-CAP-14`) |
| 5 | Kanal apa saja? | Rilis pertama hanya di dalam aplikasi: lonceng, panel daftar, toast. WhatsApp/surel menyusul (`LAB-COORD-011`) | Gerbang luar belum ada sama sekali (`NTF-CAP-18`) |
| 6 | Seberapa "seketika"? | Kotak masuk tersimpan menjadi sumber kebenaran; angka lonceng disegarkan saat halaman dibuka dan berkala; push SignalR sebagai pelengkap dari proses Web | Kendala Blue-Green (`NTF-CAP-16`, `NTF-UK-01`) |
| 7 | Siapa pemilik kode dan prefix tabelnya? | Area `Platform`, folder `platform-notification`, prefix usulan `Ntf` — didaftarkan ke Module Ownership & Prefix Registry | `LAB-DEC-016` dan `LAB-COORD-001` sudah menetapkan arah platform; prefix belum terdaftar |
| 8 | Apakah penundaan "notifikasi seketika" Rawat Inap (PRD 5.4) dibuka? | Dibuka **sebagian** lewat amendment Rawat Inap: hanya notifikasi penugasan; daftar pantau tetap seperti sekarang | Menutup `NTF-CF-01` tanpa membongkar keputusan Rawat Inap lain |
| 9 | Berapa lama pemberitahuan disimpan, dan apakah boleh dihapus pengguna? | Pengguna hanya boleh menandai dibaca, tidak menghapus. Lama simpan mengikuti kebijakan rekam medis | Pemberitahuan bisa menjadi bukti "dokter sudah dikabari" (`NTF-UK-04`) |
| 10 | Kotak persetujuan HR (`NTF-CAP-28`) ikut dihitung di lonceng? | Ya, sebagai sumber angka, tanpa menyalin isinya | Persetujuan menunggu hari ini tidak terlihat di frontend sama sekali |
