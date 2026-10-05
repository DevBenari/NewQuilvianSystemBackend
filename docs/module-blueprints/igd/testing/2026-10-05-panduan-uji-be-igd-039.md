# Panduan Uji `BE-IGD-039` — Kewenangan Unit lewat Simpul Organisasi Penugasan HR, beserta Uji Ulang `BE-IGD-061`

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 5 Oktober 2026 |
| Untuk | Agen penguji (Antigravity) yang dijalankan pemilik modul (Rizki) |
| Tujuan | Membuktikan `BE-IGD-039` (acceptance 1–9, 11, 12) dan, pada putaran yang sama, acceptance 2–3 `BE-IGD-061` yang tertahan sejak 2 Oktober (`061-S2`, `S3`, `S4` kaki kepergian dan pesanan, `S11` kaki `reject-handover`, `S12`) |
| Jumlah skenario | 17 — 15 uji API, 2 uji layar (rekap pada bagian 8) |
| Source yang diuji | Backend `rizkiG` `8d81d361` + working tree `EmergencyUnitAuthorityService.cs` (`BE-IGD-039`, belum di-commit) — **wajib sudah dibuild pemilik** (langkah P2). Frontend `RizkiV2` `57b1d360f`, hasil build 5 Oktober 2026 |
| Kontrak | validation `0.12.0` §7 aturan 1–9; permission/audit `0.7.0` §3, §3.1 — `approved` (`IGD-DEC-199`). API `0.14.0` §9 untuk penutupan susulan |
| Sumber skenario | Laporan [`BE-IGD-039`](../task/report/backend/BE-IGD-039.md) bagian 5.3 dan [`BE-IGD-061`](../task/report/backend/BE-IGD-061.md) bagian 5.1. Resep data di sini **menggantikan** resep panduan 4 Oktober untuk kepergian — lihat bagian 4.3 |
| Status | Panduan. Belum ada skenario yang dijalankan |

Berkas ini tidak diisi hasil. Hasil ditulis pada laporan baru — lihat bagian 9.

---

## 1. Aturan yang mengikat agen

Putaran 1–4 Oktober sebagian ditolak karena melanggar aturan di bawah. Patuhi seluruhnya; pelanggaran membuat skenario
yang bersangkutan **tidak sah** walaupun hasilnya tampak lulus.

| No | Aturan | Contoh pelanggaran yang pernah terjadi |
| ---: | --- | --- |
| A1 | **Layar dilayani hasil build** (`node .next/standalone/server.js` atau `npm run start`), bukan `npm run dev`. Bukti: `document.querySelector('nextjs-portal') === null` pada setiap skenario layar, dicatat di JSON | 2 Oktober seluruh uji layar dilayani `next dev` |
| A2 | **Viewport 1440 × 900** dengan sidebar terbuka untuk semua skenario layar | — |
| A3 | **Akun peran nyata** (bagian 2.2). **SuperAdmin tidak dipakai untuk satu langkah pun**, termasuk sekadar membaca Akses Role (`IGD-DEC-189`) | 3 dan 4 Oktober agen login SuperAdmin |
| A4 | **Konfigurasi tidak boleh diubah agen**: Akses Role, penugasan HR, pemetaan unit pelayanan, dan data master — tidak lewat layar, API, maupun SQL. Bila sebuah langkah ditolak karena izin, penugasan, atau pemetaan kurang, **berhenti**, catat `NOT RUN — <apa yang kurang> pada <akun/unit>`, dan minta pemilik. Skrip yang memanggil endpoint peran, izin, penugasan, atau unit selain `GET`, atau menjalankan `INSERT`/`UPDATE`/`DELETE`, membuat seluruh putaran tidak sah | 4 Oktober agen menyimpan Akses Role Perawat IGD lewat SuperAdmin (`apply-perawat-role.mjs`) |
| A5 | **Sandi tidak ditulis di skrip maupun laporan.** Kredensial hanya dari `process.env` (bagian 2.2). Badan `POST /auth/login` pada JSON bukti disamarkan (`"password": "***"`). Laporan tidak memuat host, port basis data, token, atau connection string | 4 Oktober sandi SuperAdmin tertulis di lima skrip |
| A6 | **Skrip wajib memeriksa setiap harapan.** Setiap baris *Yang diharapkan* menjadi satu entri `pemeriksaan` berisi `harapan`, `teramati` (diambil dari respons, DOM, atau kueri yang benar-benar terjadi), dan `lulus`. `putusan` dihitung `Object.values(pemeriksaan).every(p => p.lulus)`. Menulis `lulus: true` tetap, `putusan: "PASS"`, atau kalimat harapan sebagai nilai teramati **tidak diterima** | 2 Oktober `041-U6` ditulis lulus tanpa pemeriksaan |
| A7 | **Kalimat diperiksa huruf demi huruf** terhadap bagian 3.3 (`===` untuk `message` API; `includes` untuk teks layar). `{UNIT}` diganti `toServiceUnitName` dari `GET /emergency-departures/{id}` kepergian yang diuji, bukan diketik | — |
| A8 | **Catat semua permintaan ke backend** (`https://localhost:7184/api/**`) per skenario: method, URL, badan permintaan, kode status, badan respons, waktu, dan **akun yang mengirim** | 4 Oktober catatan jaringan `042-U13` disusun ulang, bukan direkam |
| A9 | **Tangkapan layar viewport** (bukan halaman penuh) diambil sesudah respons yang diuji tiba + 1000 ms | — |
| A10 | **Tidak ada penulisan langsung ke basis data.** Kueri hanya `SELECT` (bagian 5). Data uji dibuat lewat API atau layar dengan akun peran nyata | — |
| A11 | **Setiap percobaan dilaporkan**, termasuk yang gagal atau diulang. Bukti lama tidak ditimpa — beri akhiran `-try2`, `-try3` | — |
| A12 | **Narasi laporan sama dengan bukti mentah.** Langkah tambahan yang tidak ada di panduan wajib ditulis sebagai penyimpangan | 4 Oktober `PATCH visit-status` `{ 6 }` disisipkan skrip tanpa dilaporkan |
| A13 | **Source tidak diubah**, frontend maupun backend. Backend dan frontend **tidak di-build** oleh agen. Skenario yang gagal dilaporkan `FAIL` apa adanya | — |
| A14 | Skenario yang tidak dapat dijalankan ditulis **`NOT RUN` beserta alasannya**, bukan `PASS` | — |
| A15 | **Urutan dalam satu kunjungan mengikat** (bagian 6). Penolakan `403` dijalankan sebelum aksi yang berhasil, karena aksi yang berhasil mengubah keadaan kepergian | — |
| A16 | Ini verifikasi pengembang. **Tidak ada klaim UAT** | — |

---

## 2. Persiapan

### 2.1 Langkah persiapan

| Langkah | Tindakan | Syarat lanjut |
| --- | --- | --- |
| P1 | Di `NewQuilvianSystemBackend`: catat `git rev-parse --short HEAD`, `git status --short`, dan waktu ubah `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyUnitAuthorityService.cs`. Di `QuilvianSystemFrontendDev`: `git rev-parse --short HEAD`, `git status --short` | Backend `8d81d361` atau sesudahnya dengan berkas itu berstatus `M` (atau sudah di-commit); frontend `57b1d360f` atau sesudahnya |
| P2 | Bandingkan waktu ubah `bin/Debug/net9.0/QuilvianSystemBackend.dll` dengan berkas service pada P1 | DLL **lebih baru**. Bila lebih tua: **berhenti** — build `BE-IGD-039` belum ada; minta pemilik menjalankan `dotnet build -p:RunAnalyzers=false` dan menyalakan ulang backend |
| P3 | Bandingkan waktu ubah `.next/BUILD_ID` frontend dengan waktu commit HEAD frontend (`git log -1 --format=%ci`) | `BUILD_ID` lebih baru. Bila lebih tua: berhenti dan minta pemilik — agen tidak menjalankan build |
| P4 | Periksa port 3000 | Bila dipakai `node .next/standalone/server.js`: lanjut. Bila kosong: jalankan `npm run start` dari akar frontend. Bila dipakai `next dev` atau proses lain: **berhenti dan minta pemilik menghentikannya** — jangan mematikan proses yang bukan milik agen |
| P5 | Backend pemilik berjalan di `https://localhost:7184` | `GET /api/v1/auth/me` dengan akun `KLINIS` menjawab `200` |
| P6 | Untuk setiap akun pada 2.2: login, simpan respons `GET /api/v1/auth/me` (ambil `id`) dan `GET /api/v1/auth/permissions` sebagai `P6-<akun>.json` | Izin pada tabel 2.2 ada. Akun **wajib** yang kurang izinnya → berhenti (A4). Akun **opsional** yang tidak tersedia → skenarionya `NOT RUN` |
| P7 | Jalankan Q-UNIT dan Q-P2 (bagian 5), simpan keluarannya sebagai `P7-unit.json` dan `P7-penempatan.json` | Lihat 2.3 |

### 2.2 Akun uji — disiapkan pemilik, tidak diubah agen

Seluruh akun **bukan** SuperAdmin. Kolom *Penugasan HR* disiapkan pemilik lewat API penugasan organisasi HR (kartu
`BE-IGD-039`, persiapan P2) dan diperiksa agen lewat Q-P2.

| Akun | Wajib? | Peran nyata | Izin yang wajib ada | Penugasan HR (simpul organisasi) | Variabel lingkungan |
| --- | :-: | --- | --- | --- | --- |
| `LOKET` | Ya | Petugas Pendaftaran | `PatientEncounter : Create`, `Read`; `EmergencyVisit : Create` | — | `QUILVIAN_LOKET_EMAIL`, `QUILVIAN_LOKET_PASSWORD` |
| `KLINIS` | Ya | Perawat IGD | `EmergencyVisit : Read`, `Create`, `Update`; `EmergencyObservation : Read`, `Create`, `Update`; `EmergencyDisposition : Read`, `Update`; `EmergencyDispositionType : Read`; `EmergencyDeparture : Read`, `Create`, `Update` | Simpul unit `UNIT-IGD`, berlaku | `QUILVIAN_PERAWAT_EMAIL`, `QUILVIAN_PERAWAT_PASSWORD` |
| `DOKTER` | Ya | Dokter IGD | `EmergencyDisposition : Read`, `Create`, `Update`; `EmergencyDispositionType : Read`; `EmergencyVisit : Read` | — | `QUILVIAN_DOKTER_EMAIL`, `QUILVIAN_DOKTER_PASSWORD` |
| `PENERIMA` | Ya | Perawat rawat inap | `EmergencyDeparture : Read`, `Update` | Simpul unit `UNIT-TUJUAN`, berlaku | `QUILVIAN_PENERIMA_EMAIL`, `QUILVIAN_PENERIMA_PASSWORD` |
| `SAUDARA` | Ya | Perawat rawat inap | Sama dengan `PENERIMA` | Simpul **lain** — sebaiknya satu departemen dengan simpul `UNIT-TUJUAN` | `QUILVIAN_SAUDARA_EMAIL`, `QUILVIAN_SAUDARA_PASSWORD` |
| `INDUK` | Opsional | Perawat rawat inap | Sama dengan `PENERIMA` | **Hanya** simpul induk dari simpul `UNIT-TUJUAN` | `QUILVIAN_INDUK_EMAIL`, `QUILVIAN_INDUK_PASSWORD` |
| `BERAKHIR` | Opsional | Perawat rawat inap | Sama dengan `PENERIMA` | Simpul `UNIT-TUJUAN`, masa berlaku sudah lewat | `QUILVIAN_BERAKHIR_EMAIL`, `QUILVIAN_BERAKHIR_PASSWORD` |
| `SEKUNDER` | Opsional | Perawat rawat inap | Sama dengan `PENERIMA` | Utama di simpul lain **dan** sekunder di simpul `UNIT-TUJUAN`, keduanya berlaku | `QUILVIAN_SEKUNDER_EMAIL`, `QUILVIAN_SEKUNDER_PASSWORD` |
| `TANPAIZIN` | Opsional | Peran apa pun **tanpa** `EmergencyDeparture : Update` | — | Simpul `UNIT-TUJUAN`, berlaku | `QUILVIAN_TANPAIZIN_EMAIL`, `QUILVIAN_TANPAIZIN_PASSWORD` |
| `WARISAN` | Opsional | Perawat rawat inap | Sama dengan `PENERIMA` | Hanya penempatan warisan tanpa sumber (`SourceAssignmentId` kosong) | `QUILVIAN_WARISAN_EMAIL`, `QUILVIAN_WARISAN_PASSWORD` |
| Basis data dev | Ya | — | `SELECT` saja | — | `QUILVIAN_DEV_DB_URL` |

`KLINIS` **tidak** memegang `EmergencyDisposition : Create` (`IGD-DEC-190`): tindak lanjut dibuat dan dikonfirmasi
`DOKTER`, dilaksanakan `KLINIS`. `KLINIS` juga tidak memegang `EmergencyDeparture : Approve` (`IGD-DEC-191`).

### 2.3 Unit pelayanan — disiapkan pemilik

| Kode | Arti | Syarat | Variabel lingkungan (nama unit) |
| --- | --- | --- | --- |
| `UNIT-IGD` | Unit pelayanan IGD — unit **asal** kepergian | `OrganizationUnitId` terisi | `QUILVIAN_UNIT_IGD` |
| `UNIT-TUJUAN` | Satu unit rawat inap — unit **tujuan** | `OrganizationUnitId` terisi, berbeda dari `UNIT-IGD` | `QUILVIAN_UNIT_TUJUAN` |
| `UNIT-BELUM` | Unit pelayanan aktif lain yang **sengaja belum dipetakan** | `OrganizationUnitId` kosong | `QUILVIAN_UNIT_BELUM` |

Agen mencari `Id` ketiganya dengan Q-UNIT. Bila `UNIT-IGD` atau `UNIT-TUJUAN` belum dipetakan, atau `UNIT-BELUM` ternyata
sudah dipetakan: berhenti dan minta pemilik (A4).

---

## 3. Alamat, nilai enum, dan kalimat

### 3.1 Alamat

| Resource | Alamat |
| --- | --- |
| Kunjungan IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-visits` |
| Tindak lanjut IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-dispositions` |
| Pilihan jenis tindak lanjut | `https://localhost:7184/api/v1/health-services/emergency-installation-management/master-data/emergency-disposition-types/options` |
| Kepergian IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-departures` |
| Encounter (petugas) | `https://localhost:7184/api/v1/health-services/registration-management/patient-encounters/admin` |
| Pengguna aktif | `https://localhost:7184/api/v1/auth/me` |
| Layar Pengkajian Pasien IGD | `http://localhost:3000/health-services/emergency-installation-management/emergency-assessment` |

Ruang Kerja Pemeriksaan dibuka dari daftar Pengkajian: cari **nomor kunjungan lengkap**, lalu tombol *Pemeriksaan* pada
barisnya, lalu tab *Transfer Pasien*.

### 3.2 Nilai enum

| Enum | Nilai |
| --- | --- |
| `visitStatus` | `InTreatment` 4, `AwaitingDisposition` 6, `Disposed` 7, `Cancelled` 8, `Completed` 9 |
| `dispositionStatus` | `Draft` 1, `Confirmed` 2, `Executed` 3 |
| `physicalStatus` | `Prepared` 1, `Departed` 2, `Arrived` 3, `Cancelled` 9 |
| `handoverStatus` | `Submitted` 1, `Pending` 2, `Accepted` 3, `Rejected` 4, `Cancelled` 9 |
| `orderKind` | `Procedure` 2 |
| `orderSource` | `External` 2 |
| `action` (sikap pesanan) | tanpa sikap 0, `Continue` 1, `Handover` 2, `Cancel` 9 |
| `acceptanceStatus` | `NotRequired` 1, `Pending` 2, `Accepted` 3, `Rejected` 4 |
| `encounterStatus` | `Completed` 9 |

### 3.3 Kalimat yang diperiksa huruf demi huruf

`{UNIT}` = `toServiceUnitName` kepergian yang diuji (A7).

| Kode | Kalimat | Sumber |
| --- | --- | --- |
| `K-TIDAK-TIBA` | Anda tidak bertugas di unit {UNIT}, sehingga tidak dapat mencatat kedatangan pasien. | validation §7 aturan 1 |
| `K-TIDAK-TINJAU` | Anda tidak bertugas di unit {UNIT}, sehingga tidak dapat meninjau serah terima. | validation §7 aturan 1 |
| `K-TIDAK-PESANAN` | Anda tidak bertugas di unit {UNIT}, sehingga tidak dapat menerima atau menolak pesanan. | validation §7 aturan 1 |
| `K-BELUM-TIBA` | Unit {UNIT} belum dipetakan ke simpul organisasi, sehingga kewenangan mencatat kedatangan pasien belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini. | validation §7 aturan 3, `IGD-DEC-195` |
| `K-BELUM-TINJAU` | Unit {UNIT} belum dipetakan ke simpul organisasi, sehingga kewenangan meninjau serah terima belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini. | validation §7 aturan 3, `IGD-DEC-195` |
| `K-LAMA` | Lanjutkan dengan menyertakan alasan | Kalimat **lama** — tidak boleh muncul di respons mana pun |
| `K-KEPERGIAN` | Masih ada proses kepergian pasien yang belum selesai. | validation §6 — `awaitingClosureReason` |
| `K-PESANAN` | Masih ada pesanan yang belum ditentukan sikapnya: {uraian pesanan}. | validation §6 aturan 4 — `awaitingClosureReason` |

---

## 4. Data uji

### 4.1 Kunjungan

Setiap kunjungan butuh **pasien bersih**: tanpa kunjungan IGD berjalan dan tanpa encounter `Emergency` yang belum
berakhir. Pasien boleh dipakai ulang bila kunjungan sebelumnya sudah *Selesai* atau *Dibatalkan*. Dibutuhkan tujuh
kunjungan, `V1`…`V7`.

### 4.2 Resep langkah

| Kode | Akun | Langkah | Hasil |
| --- | --- | --- | --- |
| `R0` | `LOKET` | `POST /patient-encounters/admin` dengan badan yang sama dengan permintaan layar loket IGD (alur 4 Oktober) | `encounterId` |
| `R1` | `KLINIS` | `R0`, lalu `POST /emergency-visits/start-triage` `{ "encounterId": "<R0>", "mode": "ImmediateCare" }` | `visitStatus` 4 |
| `RK(tujuan, pesanan)` | `KLINIS` | `POST /emergency-departures` `{ "emergencyVisitId", "fromServiceUnitId": "<UNIT-IGD>", "toServiceUnitId": "<tujuan>", "departureReason": "Uji BE-IGD-039 <kode kunjungan>", "situationSummary": "Uji", "backgroundSummary": "Uji", "assessmentSummary": "Uji", "recommendationSummary": "Uji", "orderItems": [<pesanan>] }` | `200`; `physicalStatus` 1, `handoverStatus` 1 |
| `PESANAN-SERAH(kode)` | — | Isi `orderItems`: `{ "orderKind": 2, "orderSource": 2, "externalReference": "UJI-039-<kode>", "orderDescription": "Uji pesanan 039 <kode>", "action": 2, "toServiceUnitId": "<UNIT-TUJUAN>" }` | Pesanan `acceptanceStatus` 2 (menunggu unit tujuan) |
| `PESANAN-KOSONG(kode)` | — | Sama, tetapi **tanpa** ruas `action` dan `toServiceUnitId` | Pesanan tanpa sikap (`action` 0) |
| `RS` | `KLINIS` | `POST /emergency-departures/{id}/submit-handover` (badan kosong) | `200`; `handoverStatus` 2 |
| `RB` | `KLINIS` | `POST /emergency-departures/{id}/depart` `{}` | `200`; `physicalStatus` 2 |
| `RM` | `KLINIS` | `PATCH /emergency-visits/{id}/visit-status` `{ "visitStatus": 6, "notes": "Uji BE-IGD-039" }` | `200`; `visitStatus` 6 |
| `RD` | `DOKTER` | `GET` pilihan jenis tindak lanjut; pilih yang namanya memuat *Rawat Inap* (bila tidak ada, pilihan pertama — catat). `POST /emergency-dispositions` `{ "emergencyVisitId", "dispositionTypeId", "dispositionReason": "Uji BE-IGD-039" }`, lalu `PATCH /emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 2 }` | Tindak lanjut `Confirmed` |
| `RX` | `KLINIS` | `PATCH /emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 3 }` | `200`; tindak lanjut `Executed` |
| `RT` | `PENERIMA` | `POST /emergency-departures/{id}/arrive` `{}` | `200`; `physicalStatus` 3 |

### 4.3 Koreksi terhadap resep panduan 4 Oktober

| Hal | Panduan 4 Oktober | Panduan ini | Dasar |
| --- | --- | --- | --- |
| Langkah sebelum tindak lanjut dilaksanakan | Tidak ada | `RM` — kunjungan dipindah ke *Menunggu keputusan* lebih dulu | `RX` hanya sah dari `AwaitingDisposition`; skrip 4 Oktober menyisipkannya tanpa melapor |
| Jenis tindak lanjut | Kode `PULANG` lewat `SELECT` | Dipilih dari `GET …/options` dengan akun `DOKTER` | Kode dev `PLG`, bukan `PULANG` |
| Pembuat tindak lanjut | `KLINIS` | `DOKTER` membuat dan mengonfirmasi; `KLINIS` melaksanakan | `IGD-DEC-190` |
| Unit asal kepergian | Tidak diisi | `fromServiceUnitId` = `UNIT-IGD` | Sikap pesanan dan pesanan luar sistem diperiksa atas unit asal |
| Alasan tolak serah terima | `{ "reason": … }` | `{ "rejectionReason": … }` | Ruas DTO `UpdateEmergencyHandoverStatusRequest` |
| Alasan batal kepergian | (beralasan) | `{ "cancellationReason": … }` | Ruas DTO `CancelEmergencyDepartureRequest` |
| Waktu kejadian | — | **Jangan** mengirim `occurredAt` dari skrip | Waktu di masa depan ditolak `400`; jam mesin uji bisa lebih cepat dari server |

### 4.4 Kunjungan yang dibuat

`arrive` **bukan** titik pemicu penutupan (kontrak; kesiapan `MVP-8` bagian 7). Karena itu sesudah `RT` kunjungan yang
tertahan kepergian tetap `Disposed` tanpa penahan. Aksi berikutnya yang merupakan pemicu — terima atau tolak serah terima,
terima atau tolak pesanan, sikap pesanan — yang menutupnya. Ini disengaja oleh resep.

| Kode | Resep (urut) | Dipakai pada |
| --- | --- | --- |
| `V1` | `R1` → `RK(UNIT-TUJUAN, —)` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` | `039-S1` (`061-S2`) |
| `V2` | `R1` → `RK(UNIT-TUJUAN, PESANAN-SERAH(V2))` → `RS` → `RB`. **Tanpa** tindak lanjut — kunjungan tetap 4 | Blok B |
| `V3` | `R1` → `RK(UNIT-BELUM, —)` → `RS` → `RB` | Blok C |
| `V4` | `R1` → `RK(UNIT-TUJUAN, PESANAN-SERAH(V4))` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` | `039-S8` (`061-S3`, `061-S12` kaki tolak) |
| `V5` | `R1` → `RK(UNIT-TUJUAN, PESANAN-SERAH(V5))` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` | `061-S12-terima` |
| `V6` | `R1` → `RK(UNIT-TUJUAN, —)` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` | `061-S11-tolak` |
| `V7` | `R1` → `RK(UNIT-TUJUAN, PESANAN-KOSONG(V7))` → `RB` → `RT` → `RM` → `RD` → `RX`. **Tanpa** `RS` — pengajuan ditolak bila ada pesanan tanpa sikap | `PROBE-1` |

Setiap langkah resep dicatat pada `data-uji.json`. Langkah resep yang gagal → skenario yang bergantung padanya `NOT RUN`
beserta kode dan pesannya. Bila `RT` ditolak `403` (*"Anda tidak bertugas…"* atau *"…belum dipetakan…"*), itu temuan
`BE-IGD-039` — catat sebagai `FAIL` pada `039-S1`, bukan `NOT RUN`.

---

## 5. Kueri pemeriksaan — hanya `SELECT`

```sql
-- Q-UNIT: tiga unit uji dan pemetaannya
SELECT su."Id", su."ServiceUnitName", su."OrganizationUnitId", ou."UnitName", ou."ParentOrganizationUnitId"
FROM public."MstServiceUnit" su
LEFT JOIN public."MstOrganizationUnit" ou ON ou."Id" = su."OrganizationUnitId"
WHERE su."IsDelete" = false AND su."ServiceUnitName" IN ('<UNIT-IGD>', '<UNIT-TUJUAN>', '<UNIT-BELUM>');

-- Q-P2: penempatan akun uji beserta simpul penugasan HR sumbernya
SELECT u."Email", p."IsActive", p."IsPrimary", p."EffectiveStartDate", p."EffectiveEndDate", p."DepartmentId",
       p."SourceAssignmentId", a."OrganizationUnitId", ou."UnitName"
FROM public."AspNetUserOrganization" p
JOIN public."AspNetUsers" u ON u."Id" = p."UserId"
LEFT JOIN public."WfpOrganizationAssignment" a ON a."Id" = p."SourceAssignmentId"
LEFT JOIN public."MstOrganizationUnit" ou ON ou."Id" = a."OrganizationUnitId"
WHERE p."IsDelete" = false AND u."Email" IN (<email akun uji dari process.env>);

-- Q-VISIT: status kunjungan, asal penutupan, pelaku, encounter
SELECT v."EmergencyVisitNumber", v."VisitStatus", v."ClosedByDispositionId", v."UpdateBy", v."VisitCompletedAt",
       e."EncounterStatus"
FROM public."EmgVisit" v
LEFT JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
WHERE v."Id" = '<visit_id>';

-- Q-PESANAN: pesanan satu kepergian
SELECT "Id", "OrderDescription", "Action", "AcceptanceStatus", "IsEffective", "SupersedesOrderItemId", "UpdateBy"
FROM public."EmgHandoverOrderItem"
WHERE "EmergencyDepartureId" = '<departure_id>' AND "IsDelete" = false
ORDER BY "CreateDateTime";
```

Email pada Q-P2 dibaca dari `process.env` saat skrip berjalan dan **tidak** ditulis ke laporan; JSON bukti memakai kode
akun (`PENERIMA`, `SAUDARA`, …). `UpdateBy` dibandingkan dengan `id` dari `GET /auth/me` akun yang bersangkutan (P6).

---

## 6. Skenario — kerjakan berurutan per blok

### Blok A — penerimaan serah terima menutup kunjungan (`V1`)

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `039-S1` (`061-S2`) | Siapkan `V1` sampai `RX`. Catat `GET /emergency-visits/{V1}` dan `GET /emergency-visits?awaitingClosure=true&search=<nomor V1>`. Lalu `RT` oleh `PENERIMA`; catat `GET` kunjungan lagi. Lalu `PENERIMA`: `POST /emergency-departures/{id}/accept-handover` `{ "handoverStatus": 3 }`; lalu `GET` kepergian, `GET` kunjungan, dan Q-VISIT | Sesudah `RX`: `visitStatus` 7; daftar memuat satu baris `V1` dengan `awaitingClosureReason` === `K-KEPERGIAN`. `RT`: `200`, `physicalStatus` 3, `visitStatus` tetap 7 (catat `awaitingClosureReason` apa adanya). `accept-handover`: `200`; `handoverStatus` 3; `visitStatus` 9; Q-VISIT: `ClosedByDispositionId` = id tindak lanjut `V1`, `UpdateBy` = id `PENERIMA`, `EncounterStatus` 9. Tidak ada respons memuat `K-LAMA` |

### Blok B — penolakan kewenangan dan aksi tanpa penutupan (`V2`, kunjungan tetap 4)

Kerjakan **berurutan** (A15). Sesudah setiap `403`: `GET /emergency-departures/{V2}` — `physicalStatus` 2,
`handoverStatus` 2, pesanan `acceptanceStatus` 2 tidak berubah.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `039-S2` | `SAUDARA`: (a) `POST …/arrive` `{}`; (b) `POST …/accept-handover` `{ "handoverStatus": 3 }`; (c) `POST …/order-items/{pesanan V2}/accept` | (a) `403` `message` === `K-TIDAK-TIBA`. (b) `403` === `K-TIDAK-TINJAU`. (c) `403` === `K-TIDAK-PESANAN`. Keadaan kepergian tidak berubah |
| `039-S3` | `INDUK`: `accept-handover` | `403` === `K-TIDAK-TINJAU`. Akun tidak tersedia → `NOT RUN` |
| `039-S4` | `BERAKHIR`: `accept-handover` | `403` === `K-TIDAK-TINJAU`. Akun tidak tersedia → `NOT RUN` |
| `039-S5` | `WARISAN`: `accept-handover` | `403` === `K-TIDAK-TINJAU`. Akun atau baris warisan tidak tersedia → `NOT RUN` (jangan dibuat lewat SQL) |
| `039-S11` | `TANPAIZIN`: `accept-handover` | `403`; `message` **bukan** `K-TIDAK-TINJAU` (ditolak lapis izin sebelum penjaga unit). Akun tidak tersedia → `NOT RUN` |
| `039-U2` | **Layar, 1440 × 900, akun `KLINIS`.** Ruang Kerja `V2` → tab *Transfer Pasien* → kepergian `V2` → tombol *Catat Tiba* (konfirmasi bila diminta). Rekam jaringan | Permintaan `POST …/arrive` dijawab `403` `message` === `K-TIDAK-TIBA`; teks yang sama tampil di layar (`includes`); kepergian tetap *Berangkat*. `nextjsPortalNull` benar. PNG wajib |
| `039-S8a` (`061-S4` kaki pesanan) | `PENERIMA`: `POST …/order-items/{pesanan V2}/reject` `{ "acceptanceStatus": 4, "rejectionReason": "Uji 039 tolak" }`. Lalu `KLINIS`: `PATCH …/order-items/{pesanan V2}/action` `{ "item": { "orderKind": 2, "orderSource": 2, "externalReference": "UJI-039-V2", "orderDescription": "Uji pesanan 039 V2", "action": 1 } }`. Lalu Q-PESANAN dan `GET` kunjungan | Tolak: `200`, `acceptanceStatus` 4. Sikap: `200`; Q-PESANAN: baris lama `IsEffective` false, baris pengganti `Action` 1, `AcceptanceStatus` 1, `SupersedesOrderItemId` = baris lama, `UpdateBy`/pembuat = `KLINIS`. `visitStatus` tetap 4 |
| `039-S6` (`061-S4` kaki kepergian) | `SEKUNDER`: `accept-handover` `{ "handoverStatus": 3 }`; lalu `GET` kepergian dan kunjungan. Bila `SEKUNDER` tidak tersedia: jalankan dengan `PENERIMA` dan tandai `039-S6` `NOT RUN` (kaki `061-S4` tetap dinilai) | `200`; `handoverStatus` 3; `visitStatus` tetap 4 |

### Blok C — unit yang belum dipetakan (`V3`)

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `039-S7` | `PENERIMA`: (a) `POST …/arrive` `{}`; (b) `POST …/accept-handover` `{ "handoverStatus": 3 }` pada kepergian `V3` | (a) `403` === `K-BELUM-TIBA`. (b) `403` === `K-BELUM-TINJAU`. Kedua `message` tidak memuat `K-LAMA`. Keadaan kepergian tidak berubah |
| `039-U1` | **Layar, 1440 × 900, akun `KLINIS`.** Ruang Kerja `V3` → tab *Transfer Pasien* → kepergian `V3` → *Terima Dokumen* (konfirmasi bila diminta). Rekam jaringan | `POST …/accept-handover` `403` `message` === `K-BELUM-TINJAU`; teks yang sama tampil di layar (`includes`); teks layar **tidak** memuat `K-LAMA`. `nextjsPortalNull` benar. PNG wajib |
| `039-S9` | `KLINIS`: `PATCH …/cancel` `{ "cancellationReason": "Uji 039 batal" }` pada kepergian `V3`; lalu `GET` kepergian dan kunjungan | `200` — **tidak** ditolak kewenangan unit walau `UNIT-BELUM` belum dipetakan (`IGD-DEC-197`); `physicalStatus` 9, `handoverStatus` 9; `visitStatus` tetap 4 |

### Blok D — penutupan susulan lewat pesanan dan serah terima (`V4`…`V6`)

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `039-S8` (`061-S3`, `061-S12` kaki tolak) | Siapkan `V4` sampai `RT`; catat `GET` kunjungan (tetap 7). (1) `PENERIMA`: `POST …/order-items/{pesanan V4}/reject` `{ "acceptanceStatus": 4, "rejectionReason": "Uji 039 V4" }`; lalu `GET /emergency-visits?awaitingClosure=true&search=<nomor V4>`. (2) `KLINIS`: `PATCH …/order-items/{pesanan V4}/action` `{ "item": { "orderKind": 2, "orderSource": 2, "externalReference": "UJI-039-V4", "orderDescription": "Uji pesanan 039 V4", "action": 1 } }`; lalu `GET` kunjungan dan Q-VISIT | (1) `200`; kunjungan **tetap** 7 — pesanan yang ditolak menjadi penahan; `awaitingClosureReason` === `K-PESANAN` dengan uraian `Uji pesanan 039 V4`. (2) `200`; `visitStatus` 9; Q-VISIT: `ClosedByDispositionId` = id tindak lanjut `V4`, `UpdateBy` = id `KLINIS`, `EncounterStatus` 9 |
| `061-S12-terima` | Siapkan `V5` sampai `RT`. `PENERIMA`: `POST …/order-items/{pesanan V5}/accept` (badan kosong); lalu `GET` kunjungan dan Q-VISIT | `200`; pesanan `acceptanceStatus` 3; `visitStatus` 9; `ClosedByDispositionId` = tindak lanjut `V5`; `UpdateBy` = id `PENERIMA` |
| `061-S11-tolak` | Siapkan `V6` sampai `RT`. `PENERIMA`: `POST …/reject-handover` `{ "handoverStatus": 4, "rejectionReason": "Uji 061-S11" }`; lalu `GET` kepergian, kunjungan, dan Q-VISIT | `200`; `handoverStatus` 4; `visitStatus` 9 — dokumen yang ditolak tidak menahan penutupan bila pasien sudah tiba (`IGD-DEC-106`); `UpdateBy` = id `PENERIMA` |

### Blok E — probe pesanan tanpa sikap (`V7`) — **bukan** acceptance `BE-IGD-039`

Kontrak (validation §5.1 dan §6 aturan 4) menyebut pesanan **tanpa sikap** ikut menahan penutupan kunjungan. Pembacaan
source pengembang 5 Oktober 2026 menduga hanya pesanan yang **ditolak** yang menahan. Probe ini mengumpulkan bukti; hasil
apa pun **tidak** menurunkan status `BE-IGD-039` atau `BE-IGD-061`.

| ID | Langkah | Yang diharapkan menurut kontrak |
| --- | --- | --- |
| `PROBE-1` | Siapkan `V7` sampai `RX` (tanpa `RS`). Catat respons `RX`, `GET /emergency-visits/{V7}`, `GET /emergency-visits?awaitingClosure=true&search=<nomor V7>`, Q-PESANAN, dan Q-VISIT | `RX` `200`; `visitStatus` **7**; `awaitingClosureReason` === `K-PESANAN` dengan uraian `Uji pesanan 039 V7`. Bila teramati `visitStatus` 9: tulis `putusan` `FAIL` dengan alasan *"tidak sesuai kontrak validation §6 aturan 4"* — itu temuan untuk pengembang, bukan kegagalan putaran |

### Blok F — tindakan klinis tidak tersentuh penjaga unit

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `039-S12` | Dari `jaringan[]` seluruh skenario: kumpulkan setiap permintaan `R1`, `RM`, `RD`, `RX` (dan observasi bila ada) | Tidak satu pun dijawab `403`; tidak satu pun `message` memuat *"tidak bertugas di unit"* atau *"belum dipetakan ke simpul organisasi"* |

**Sisa data uji.** `V2` (kunjungan 4, kepergian *Berangkat*/dokumen *Diterima*) dan `V3` (kunjungan 4, kepergian
*Dibatalkan*) tetap berjalan sesudah putaran ini. `V7` bergantung hasil probe. Catat nomornya pada laporan.

---

## 7. Bentuk bukti mentah

Folder **baru**: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-039-<YYYYMMDD>/` (tanggal putaran dijalankan).

| Berkas | Isi |
| --- | --- |
| `<ID>.json` | `{ id, akun (kode akun, bukan email), percobaan, mulai, selesai, viewport, nextjsPortalNull, data{}, langkah[], jaringan[], kueri[], pemeriksaan{ <nama>: { harapan, teramati, lulus } }, putusan, alasan }` — `jaringan[]` berisi seluruh permintaan ke `/api/**` beserta kode akun pengirim; badan login disamarkan |
| `<ID>.png` | Tangkapan layar viewport menurut A9 (`039-U1`, `039-U2`) |
| `persiapan.json` | Hasil P1–P5: SHA, `git status --short`, waktu ubah berkas service dan DLL, waktu `BUILD_ID` dan commit frontend, proses pada port 3000 |
| `P6-<akun>.json` | Respons `GET /auth/me` (hanya `id` dan nama peran) dan `GET /auth/permissions` tiap akun |
| `P7-unit.json`, `P7-penempatan.json` | Keluaran Q-UNIT dan Q-P2 — email diganti kode akun |
| `data-uji.json` | Pasien (nama dan RM), nomor encounter, nomor kunjungan, id kepergian, pesanan, dan tindak lanjut `V1`…`V7`, beserta resep dan akun tiap langkah |
| Skrip | Semua skrip yang dijalankan, apa adanya. Kredensial hanya lewat `process.env` |

Log backend `Logs/quilvian-backend-<YYYYMMDD>.json` **tidak dihapus atau disunting** — pengembang mencocokkan bukti dengan
log itu.

---

## 8. Rekap skenario

| Blok | Skenario | Jenis | Jumlah |
| --- | --- | --- | ---: |
| A | `039-S1` (`061-S2`) | API | 1 |
| B | `039-S2`, `S3`, `S4`, `S5`, `S11`, `S8a` (`061-S4`), `S6` (`061-S4`) | API | 7 |
| B | `039-U2` | Layar | 1 |
| C | `039-S7`, `039-S9` | API | 2 |
| C | `039-U1` | Layar | 1 |
| D | `039-S8` (`061-S3`, `061-S12`), `061-S12-terima`, `061-S11-tolak` | API | 3 |
| E | `PROBE-1` | API (probe) | 1 |
| F | `039-S12` | Analisis jaringan | 1 |
| **Jumlah** | | | **17** |

| Acceptance | Skenario |
| --- | --- |
| `BE-IGD-039` 1 | `039-S1` |
| `BE-IGD-039` 2 | `039-S2`, `039-U2` |
| `BE-IGD-039` 3, 4, 5, 6 | `039-S3`, `S4`, `S5`, `S6` |
| `BE-IGD-039` 7 | `039-S7`, `039-U1` |
| `BE-IGD-039` 8 | `039-S8`, `039-S8a` |
| `BE-IGD-039` 9 | `039-S9` |
| `BE-IGD-039` 11 | `039-S11` |
| `BE-IGD-039` 12 | `039-S12` |
| `BE-IGD-061` 2 | `039-S1` (`061-S2`), `061-S11-tolak` |
| `BE-IGD-061` 3 | `039-S8` (`061-S3`, `061-S12` kaki tolak), `061-S12-terima` |
| `BE-IGD-061` 4 (kaki kepergian dan pesanan) | `039-S6`, `039-S8a` |

---

## 9. Pelaporan

Tulis laporan baru di folder ini: `<YYYY-MM-DD>-laporan-uji-be-igd-039.md`. Berkas panduan ini, laporan task, roadmap,
dan dokumen blueprint lain **tidak disunting** — penandaan status dilakukan pengembang sesudah memeriksa bukti mentah dan
log backend.

Isi laporan:

1. Metadata: tanggal, pelaksana, SHA frontend dan backend, `git status --short` kedua repository dari P1, waktu DLL dan
   berkas service (P2), waktu `BUILD_ID` (P3), bukti layar dilayani hasil build, viewport, basis data dev **tanpa** alamat
   host.
2. Akun: kode akun yang dipakai tiap langkah (bukan email), akun opsional yang tidak tersedia, dan pernyataan bahwa
   Akses Role, penugasan HR, dan pemetaan unit tidak diubah agen.
3. Tabel per skenario: ID, percobaan ke berapa, `PASS` / `FAIL` / `NOT RUN`, kode status dan kalimat yang teramati, nama
   berkas bukti.
4. Data uji: isi `data-uji.json` dalam bentuk tabel, serta keluaran Q-UNIT dan Q-P2 (email diganti kode akun).
5. Setiap penyimpangan dari panduan ini: data pengganti, langkah tambahan, percobaan ulang, langkah yang dilewati.

| Blok | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | --- | ---: | ---: | ---: | ---: |
| A | `039-S1` | 1 | | | |
| B | `039-S2`, `S3`, `S4`, `S5`, `S11`, `S8a`, `S6`, `U2` | 8 | | | |
| C | `039-S7`, `S9`, `U1` | 3 | | | |
| D | `039-S8`, `061-S12-terima`, `061-S11-tolak` | 3 | | | |
| E | `PROBE-1` | 1 | | | |
| F | `039-S12` | 1 | | | |
| **Jumlah** | | **17** | | | |
