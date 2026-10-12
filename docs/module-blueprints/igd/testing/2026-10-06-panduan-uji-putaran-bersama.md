# Panduan Uji Putaran Bersama — `FE-IGD-043`, `FE-IGD-044`, dan Sisa `BE-IGD-041`

| Field | Nilai |
| --- | --- |
| Tanggal disusun | 6 Oktober 2026 |
| Untuk | Agen penguji (Antigravity) yang dijalankan pemilik modul (Rizki) |
| Tujuan | Membuktikan `FE-IGD-043` acceptance 1–7 (galat aksi kepergian di modal), `FE-IGD-044` acceptance 1–11 (sikap pesanan kepergian dari tab Transfer), dan sisa `BE-IGD-041` acceptance 14–15 (`IGD-DEC-211`) dalam satu putaran (`IGD-DEC-208`) |
| Jumlah skenario | 22 — 5 uji API, 15 uji layar, 2 pemeriksaan pendukung (rekap pada bagian 8) |
| Source yang diuji | Backend `rizkiG` `3811fa06` + working tree `EmergencyDepartureService.cs` (`BE-IGD-041`) dan `EmergencyVisitController.cs` (`BE-IGD-064`) — **wajib sudah dibuild pemilik** (P2). Frontend `RizkiV2` `2a985f6d5` + working tree empat berkas `FE-IGD-044` — **wajib hasil build** (P3) |
| Kontrak | API `0.14.0` §2, §2.3, §9; validation `0.13.0` §4, §5, §5.1, §6, §7; state `0.9.0` §6a.2 — `approved` (`IGD-DEC-108`, `IGD-DEC-199`, `IGD-DEC-209`) |
| Sumber skenario | Kartu `FE-IGD-043` dan `FE-IGD-044` (roadmap frontend R3.13.2); laporan task [`FE-IGD-044`](../task/report/frontend/FE-IGD-044.md) bagian 6 dan [`BE-IGD-041`](../task/report/backend/BE-IGD-041.md) bagian 9.8 |
| Status | Panduan. Belum ada skenario yang dijalankan |

Berkas ini tidak diisi hasil. Hasil ditulis pada laporan baru — lihat bagian 9.

---

## 1. Aturan yang mengikat agen

Putaran 1–6 Oktober sebagian ditolak atau diterima dengan penyimpangan karena melanggar aturan di bawah. Patuhi
seluruhnya; pelanggaran membuat skenario yang bersangkutan **tidak sah** walaupun hasilnya tampak lulus.

| No | Aturan | Contoh pelanggaran yang pernah terjadi |
| ---: | --- | --- |
| A1 | **Layar dilayani hasil build** (`node .next/standalone/server.js` atau `npm run start`), bukan `npm run dev`. Bukti: `document.querySelector('nextjs-portal') === null` pada setiap skenario layar, dicatat di JSON | 6 Oktober `064-U1` dilayani `next dev` |
| A2 | **Viewport 1440 × 900** dengan sidebar terbuka untuk semua skenario layar | — |
| A3 | **Akun peran nyata** (bagian 2.2). **SuperAdmin tidak dipakai untuk satu langkah pun** — tidak untuk login, membaca Akses Role, mendaftar pengguna, maupun menyiapkan data (`IGD-DEC-189`) | 6 Oktober seluruh uji `BE-IGD-064` siang lewat SuperAdmin; 6 Oktober sore `grant-dimas.mjs` login SuperAdmin |
| A4 | **Konfigurasi tidak boleh diubah agen**: Akses Role, penugasan HR, pemetaan unit pelayanan, dan data master — tidak lewat layar, API, maupun SQL. Bila sebuah langkah ditolak `403` karena izin, penugasan, atau pemetaan kurang, **berhenti**, catat `NOT RUN — <apa yang kurang> pada <akun/unit>`, dan minta pemilik. Satu saja permintaan ke `/api/v1/administrator/**` selain `GET` membuat **seluruh putaran tidak sah** | 6 Oktober `POST /administrator/setting/role-access/policies` empat kali lewat SuperAdmin untuk menambah izin akun perawat |
| A5 | **Sandi dan alamat surel tidak ditulis di skrip maupun laporan.** Keduanya hanya dari `process.env` (bagian 2.2), **tanpa nilai cadangan literal** (`process.env.X \|\| '…'` dilarang). Badan `POST /auth/login` pada JSON bukti disamarkan (`"password": "***"`). Laporan tidak memuat host, port basis data, token, atau connection string | 6 Oktober sandi SuperAdmin dan sandi akun perawat tertulis literal di skrip; host basis data tertulis di laporan |
| A6 | **Skrip wajib memeriksa setiap harapan.** Setiap baris *Yang diharapkan* menjadi satu entri `pemeriksaan` berisi `harapan`, `teramati` (diambil dari respons, DOM, atau kueri yang benar-benar terjadi), dan `lulus`. `putusan` dihitung `Object.values(pemeriksaan).every(p => p.lulus)`. Pemeriksaan teks layar dibatasi pada **wadah yang diuji** (modal yang terbuka, kartu kepergian, baris pesanan, atau kartu pasien), bukan seluruh halaman | 6 Oktober teks `064-U1` dibaca dari seluruh halaman termasuk menu samping |
| A7 | **Kalimat diperiksa huruf demi huruf** terhadap bagian 3.3 (`===` untuk `message` API; `includes` untuk teks layar di dalam wadahnya). `{UNIT}` dan `{UNIT-ASAL}` diganti `toServiceUnitName` / `fromServiceUnitName` dari `GET /emergency-departures/{id}`, bukan diketik | — |
| A8 | **Catat semua permintaan ke backend** (`https://localhost:7184/api/**`) per skenario — termasuk login — beserta method, URL, badan, kode status, badan respons, waktu, dan **kode akun** pengirim | 6 Oktober login tidak tercatat |
| A9 | **Tangkapan layar viewport** (bukan halaman penuh) diambil sesudah respons yang diuji tiba + 1000 ms | — |
| A10 | **Tidak ada penulisan langsung ke basis data.** Kueri hanya `SELECT` (bagian 5), dijalankan dari skrip **di dalam folder bukti** dengan teks kuerinya tersimpan. Data uji dibuat lewat API atau layar dengan akun peran nyata | 6 Oktober kueri lewat helper di luar folder bukti |
| A11 | **Setiap percobaan dilaporkan**, termasuk yang gagal, diulang, atau dibatalkan di tengah. Bukti lama tidak ditimpa — beri akhiran `-try2`, `-try3`. Skrip yang diubah di antara percobaan disimpan per versi | 6 Oktober putaran 01.46–01.50 dan 03.38–03.58 UTC tidak dilaporkan |
| A12 | **Narasi laporan sama dengan bukti mentah.** Langkah tambahan yang tidak ada di panduan wajib ditulis sebagai penyimpangan | 6 Oktober laporan menulis "tanpa SuperAdmin" padahal log mencatatnya |
| A13 | **Repository tidak diubah agen.** Source, CSS, konfigurasi, dan berkas apa pun di kedua repository tidak disunting, dipindah, atau dihapus — termasuk `Storage/uploads/**`. Backend dan frontend **tidak di-build** agen. Bila halaman gagal dikompilasi, **berhenti** dan lapor | 6 Oktober dua berkas CSS frontend diubah; dua QR code tracked dihapus |
| A14 | Skenario yang tidak dapat dijalankan ditulis **`NOT RUN` beserta alasannya**, bukan `PASS` | — |
| A15 | **Urutan dalam satu kunjungan mengikat** (bagian 6). Penolakan dijalankan sebelum aksi yang berhasil, karena aksi yang berhasil mengubah keadaan | — |
| A16 | Ini verifikasi pengembang. **Tidak ada klaim UAT** | — |
| A17 | **Dokumen blueprint tidak disunting agen**: laporan task, roadmap, traceability, decision log, dan panduan ini. Status task tidak ditulis agen | 6 Oktober laporan task `BE-IGD-041` dan `BE-IGD-064` ditandai ✅ oleh agen |
| A18 | **Peran mengikuti alur bisnis**, bukan izin yang kebetulan ada. Tindak lanjut dibuat dan dikonfirmasi `DOKTER` (`IGD-DEC-190`); encounter dibuat `LOKET`; kedatangan dan terima/tolak pesanan oleh `PENERIMA`. Walaupun akun perawat kini memegang izin tambahan (konfigurasi sementara, `IGD-DEC-216`), langkah itu **tidak** dikerjakan akun perawat | — |

---

## 2. Persiapan

### 2.1 Langkah persiapan

| Langkah | Tindakan | Syarat lanjut |
| --- | --- | --- |
| P1 | Di `NewQuilvianSystemBackend`: catat `git rev-parse --short HEAD` dan `git status --short`. Di `QuilvianSystemFrontendDev`: sama | Backend `3811fa06` atau sesudahnya, `EmergencyDepartureService.cs` dan `EmergencyVisitController.cs` berstatus `M` (atau sudah di-commit). Frontend `2a985f6d5` atau sesudahnya, empat berkas `FE-IGD-044` berstatus `M` (atau sudah di-commit): `emergency-assessment-transfer-tab.jsx`, `emergency-assessment-detail-view.jsx`, `emergency-assessment-constant.jsx`, `emergency-assessment-slice.jsx` |
| P2 | Bandingkan waktu ubah `bin/Debug/net9.0/QuilvianSystemBackend.dll` dengan kedua berkas backend pada P1 | DLL **lebih baru** dari keduanya. Bila tidak: **berhenti** — minta pemilik menjalankan `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` dan menyalakan ulang backend |
| P3 | Bandingkan waktu ubah `.next/BUILD_ID` dengan waktu ubah keempat berkas frontend pada P1 | `BUILD_ID` **lebih baru** dari keempatnya. Bila tidak: **berhenti** dan minta pemilik — agen tidak menjalankan build |
| P4 | Periksa port 3000 dan catat perintah proses yang mendengarkannya | `node .next/standalone/server.js` atau `next start`: lanjut. Kosong: jalankan `npm run start` dari akar frontend. `next dev` atau proses lain: **berhenti** dan minta pemilik menghentikannya — jangan mematikan proses yang bukan milik agen |
| P5 | Backend pemilik berjalan di `https://localhost:7184` | `GET /api/v1/auth/me` dengan akun `PERAWAT` menjawab `200` |
| P6 | Untuk setiap akun pada 2.2: login, simpan `GET /api/v1/auth/me` (ambil `id`) dan `GET /api/v1/auth/permissions` sebagai `P6-<akun>.json` | Izin pada tabel 2.2 ada. Akun **wajib** yang kurang izinnya → berhenti (A4) |
| P7 | Jalankan Q-UNIT dan Q-P2 (bagian 5), simpan sebagai `P7-unit.json` dan `P7-penempatan.json` | Lihat 2.3 dan catatan 2.2 |

### 2.2 Akun uji — disiapkan pemilik, tidak diubah agen

Seluruh akun **bukan** SuperAdmin.

| Akun | Peran nyata | Dipakai untuk | Izin yang wajib ada | Penugasan HR | Variabel lingkungan |
| --- | --- | --- | --- | --- | --- |
| `LOKET` | Petugas Pendaftaran | Encounter (`R0`) | `PatientEncounter : Create`, `Read` | — | `QUILVIAN_LOKET_EMAIL`, `QUILVIAN_LOKET_PASSWORD` |
| `PERAWAT` | Perawat IGD — akun `dimas.kurniawan@rsmmc.local` (`IGD-DEC-215`) | Mulai kunjungan, kepergian, pengajuan, keberangkatan, status kunjungan, pelaksanaan tindak lanjut, **seluruh uji layar** | `EmergencyVisit : Read`, `Create`, `Update`; `EmergencyDisposition : Read`, `Update`; `EmergencyDeparture : Read`, `Create`, `Update` | Simpul unit `UNIT-IGD`, berlaku; **tidak** di simpul `UNIT-TUJUAN` | `QUILVIAN_PERAWAT_EMAIL`, `QUILVIAN_PERAWAT_PASSWORD` |
| `DOKTER` | Dokter IGD | Membuat dan mengonfirmasi tindak lanjut (`RD`) | `EmergencyDisposition : Read`, `Create`, `Update`; `EmergencyDispositionType : Read` | — | `QUILVIAN_DOKTER_EMAIL`, `QUILVIAN_DOKTER_PASSWORD` |
| `PENERIMA` | Perawat rawat inap | Kedatangan (`RT`), terima/tolak pesanan | `EmergencyDeparture : Read`, `Update` | Simpul unit `UNIT-TUJUAN`, berlaku | `QUILVIAN_PENERIMA_EMAIL`, `QUILVIAN_PENERIMA_PASSWORD` |
| Basis data dev | — | Q-UNIT, Q-P2, Q-VISIT, Q-PESANAN | `SELECT` saja | — | `QUILVIAN_DEV_DB_URL` |

**Catatan `PERAWAT`.** Q-P2 wajib menunjukkan penugasan aktif akun ini di simpul `UNIT-IGD` (syarat sikap pesanan, Blok B
dan C) dan **tidak** di simpul `UNIT-TUJUAN` (syarat `043-U1`). Bila tidak demikian: berhenti dan minta pemilik (A4).
Konfigurasi Akses Role akun ini sedang melampaui `IGD-DEC-190` sebagai konfigurasi sementara (`IGD-DEC-216`); patuhi A18.

### 2.3 Unit pelayanan — disiapkan pemilik

| Kode | Arti | Syarat | Variabel lingkungan (nama unit) |
| --- | --- | --- | --- |
| `UNIT-IGD` | Unit pelayanan IGD — unit **asal** kepergian | `OrganizationUnitId` terisi | `QUILVIAN_UNIT_IGD` |
| `UNIT-TUJUAN` | Satu unit rawat inap — unit **tujuan** | `OrganizationUnitId` terisi, berbeda dari `UNIT-IGD` | `QUILVIAN_UNIT_TUJUAN` |
| `UNIT-BELUM` | Unit pelayanan aktif lain yang **sengaja belum dipetakan** | `OrganizationUnitId` kosong | `QUILVIAN_UNIT_BELUM` |

Bila `UNIT-IGD` atau `UNIT-TUJUAN` belum dipetakan, atau `UNIT-BELUM` ternyata sudah dipetakan: berhenti dan minta
pemilik (A4). Bila tidak ada unit `UNIT-BELUM`: `043-U2` dan `043-U4` ditulis `NOT RUN`.

---

## 3. Alamat, nilai enum, dan kalimat

### 3.1 Alamat

| Resource | Alamat |
| --- | --- |
| Kunjungan IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-visits` |
| Tindak lanjut IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-dispositions` |
| Pilihan jenis tindak lanjut | `https://localhost:7184/api/v1/health-services/emergency-installation-management/master-data/emergency-disposition-types/options` |
| Kepergian IGD | `https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-departures` |
| Pesanan kepergian | `…/emergency-departures/{id}/order-items`, `…/order-items/{itemId}/action`, `…/order-items/{itemId}/accept`, `…/order-items/{itemId}/reject` |
| Encounter (petugas) | `https://localhost:7184/api/v1/health-services/registration-management/patient-encounters/admin` |
| Pengguna aktif | `https://localhost:7184/api/v1/auth/me` |
| Layar Pengkajian Pasien IGD | `http://localhost:3000/health-services/emergency-installation-management/emergency-assessment` |

Ruang Kerja Pemeriksaan dibuka dari daftar Pengkajian: cari **nomor kunjungan lengkap**, lalu *Pemeriksaan* pada barisnya,
lalu tab *Transfer Pasien* → segmen *Riwayat*.

### 3.2 Nilai enum

| Enum | Nilai |
| --- | --- |
| `visitStatus` | `InTreatment` 4, `AwaitingDisposition` 6, `Disposed` 7, `Cancelled` 8, `Completed` 9 |
| `dispositionStatus` | `Draft` 1, `Confirmed` 2, `Executed` 3 |
| `physicalStatus` | `Prepared` 1, `Departed` 2, `Arrived` 3, `Cancelled` 9 |
| `handoverStatus` | `Submitted` 1, `Pending` 2, `Accepted` 3, `Rejected` 4, `Cancelled` 9 |
| `orderKind` | `Medication` 1, `Procedure` 2, `LaboratoryOrder` 3, `RadiologyOrder` 4 |
| `orderSource` | `Internal` 1, `External` 2 |
| `action` | tanpa sikap 0, `Continue` 1, `Handover` 2, `Cancel` 9 |
| `acceptanceStatus` | `NotRequired` 1, `Pending` 2, `Accepted` 3, `Rejected` 4 |

### 3.3 Kalimat yang diperiksa huruf demi huruf

`{UNIT}` = `toServiceUnitName`, `{UNIT-ASAL}` = `fromServiceUnitName` kepergian yang diuji (A7).

| Kode | Kalimat | Sumber |
| --- | --- | --- |
| `K-TIDAK-TIBA` | Anda tidak bertugas di unit {UNIT}, sehingga tidak dapat mencatat kedatangan pasien. | validation §7 aturan 1 |
| `K-BELUM-TINJAU` | Unit {UNIT} belum dipetakan ke simpul organisasi, sehingga kewenangan meninjau serah terima belum dapat diperiksa sistem. Minta Master Data melengkapi pemetaan unit ini. | validation §7 aturan 3, `IGD-DEC-195` |
| `K-LAMA` | Lanjutkan dengan menyertakan alasan | Kalimat **lama** — tidak boleh muncul di respons maupun layar |
| `K-AJUKAN` | Masih ada {n} pesanan yang belum ditentukan sikapnya. | validation §5 aturan 1 |
| `K-PESANAN` | Masih ada pesanan yang belum ditentukan sikapnya: {uraian pesanan}. | validation §6 aturan 4 — `awaitingClosureReason` |
| `K-KEPERGIAN` | Masih ada proses kepergian pasien yang belum selesai. | validation §6 aturan 3 |
| `K-TIDAK-SIKAP` | Anda tidak bertugas di unit {UNIT-ASAL}, sehingga tidak dapat menentukan sikap pesanan. | validation §7 aturan 1 (unit asal) |
| `L-KONFIRMASI` | Perubahan akan dicatat sebagai kejadian baru dalam riwayat kepergian pasien. | Kalimat modal aksi kepergian |
| `L-BAGIAN` | Pesanan saat pasien pergi | Judul bagian pesanan di kartu kepergian |
| `L-PENUNJANG` | Pemeriksaan penunjang belum dapat dihitung otomatis oleh sistem. Daftar ini hanya memuat pesanan yang tercatat pada kepergian. | Keterangan validation §5 aturan 4 |
| `L-LAB` | Sikap pesanan laboratorium ditetapkan petugas, bukan dibaca dari sistem laboratorium. | Keterangan validation §5 aturan 5 |
| `L-BATAL` | Kepergian ini dibatalkan, sehingga pesanannya tidak memerlukan sikap dan tidak menahan penutupan kunjungan. | `IGD-DEC-205` |
| `L-KOSONG` | Tidak ada pesanan yang tercatat pada kepergian ini. | `FE-IGD-044` acceptance 2 |
| `L-TANPA-SIKAP` | Sikap: Belum ada sikap | Lencana baris pesanan tanpa sikap |
| `L-TIDAK-BERLAKU` | Tidak berlaku | Lencana baris yang digantikan |
| `L-UNIT-PENERIMA` | mengikuti unit tujuan kepergian dan tidak dapat diganti. | Modal *Handover*, didahului *"Unit penerima: {UNIT}"* |
| `L-SELESAI` | Selesai | Lencana status kunjungan di kartu pasien |

---

## 4. Data uji

### 4.1 Kunjungan

Setiap kunjungan butuh **pasien bersih**: tanpa kunjungan IGD berjalan dan tanpa encounter `Emergency` yang belum berakhir.
Pasien boleh dipakai ulang bila kunjungan sebelumnya sudah *Selesai* atau *Dibatalkan*. Bila pasien bersih kurang,
berhenti dan minta pemilik — **jangan** membuat pasien lewat akun yang perannya bukan pendaftaran (A18). Dibutuhkan
sembilan kunjungan.

### 4.2 Resep langkah

| Kode | Akun | Langkah | Hasil |
| --- | --- | --- | --- |
| `R0` | `LOKET` | `POST /patient-encounters/admin` dengan badan yang sama dengan permintaan layar loket IGD | `encounterId` |
| `R1` | `PERAWAT` | `R0`, lalu `POST /emergency-visits/start-triage` `{ "encounterId": "<R0>", "mode": "ImmediateCare" }` | `visitStatus` 4 |
| `RK(tujuan, pesanan)` | `PERAWAT` | `POST /emergency-departures` `{ "emergencyVisitId", "fromServiceUnitId": "<UNIT-IGD>", "toServiceUnitId": "<tujuan>", "departureReason": "Uji putaran bersama <kode>", "situationSummary": "Uji", "backgroundSummary": "Uji", "assessmentSummary": "Uji", "recommendationSummary": "Uji", "orderItems": [<pesanan>] }` | `physicalStatus` 1, `handoverStatus` 1 |
| `KOSONG(kode, kind)` | — | Isi `orderItems`: `{ "orderKind": <kind>, "orderSource": 2, "externalReference": "UJI-PB-<kode>", "orderDescription": "Uji PB <kode>" }` — **tanpa** `action` dan `toServiceUnitId` | Pesanan tanpa sikap (`action` 0) |
| `SERAH(kode)` | — | Isi `orderItems`: `{ "orderKind": 2, "orderSource": 2, "externalReference": "UJI-PB-<kode>", "orderDescription": "Uji PB <kode>", "action": 2, "toServiceUnitId": "<UNIT-TUJUAN>" }` | Pesanan `Handover`, `acceptanceStatus` 2 |
| `RS` | `PERAWAT` | `POST /emergency-departures/{id}/submit-handover` (badan kosong) | `handoverStatus` 2 |
| `RB` | `PERAWAT` | `POST /emergency-departures/{id}/depart` `{}` | `physicalStatus` 2 |
| `RM` | `PERAWAT` | `PATCH /emergency-visits/{id}/visit-status` `{ "visitStatus": 6, "notes": "Uji putaran bersama" }` | `visitStatus` 6 |
| `RD` | `DOKTER` | `GET` pilihan jenis tindak lanjut; pilih yang namanya memuat *Rawat Inap* (bila tidak ada, pilihan pertama — catat). `POST /emergency-dispositions` `{ "emergencyVisitId", "dispositionTypeId", "dispositionReason": "Uji putaran bersama" }`, lalu `PATCH /emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 2 }` | Tindak lanjut `Confirmed` |
| `RX` | `PERAWAT` | `PATCH /emergency-dispositions/{id}/disposition-status` `{ "dispositionStatus": 3 }` | Tindak lanjut `Executed` |
| `RT` | `PENERIMA` | `POST /emergency-departures/{id}/arrive` `{}` | `physicalStatus` 3 |
| `RC` | `PERAWAT` | `PATCH /emergency-departures/{id}/cancel` `{ "cancellationReason": "Uji putaran bersama <kode>" }` | `physicalStatus` 9 |

**Jangan** mengirim `occurredAt` dari skrip — jam mesin uji bisa lebih cepat dari server.

### 4.3 Kunjungan yang dibuat

| Kode | Resep (urut) | Dipakai pada |
| --- | --- | --- |
| `VA1` | `R1` → `RK(UNIT-TUJUAN, —)` → `RS` → `RB` | `043-U1`, `043-U5` |
| `VA2` | `R1` → `RK(UNIT-BELUM, —)` → `RS` → `RB` | `043-U2`, `043-U4` |
| `VB1` | `R1` → `RK(UNIT-TUJUAN, [KOSONG(B1-OBAT, 1), KOSONG(B1-LAB, 3), KOSONG(B1-TINDAKAN, 2)])` | `043-U3`, `044-U1`, `044-U3`, `044-U4`, `044-U5`, `044-U8` (`043-U6`) |
| `VB2` | `R1` → `RK(UNIT-TUJUAN, —)` | `044-U2` |
| `VB3` | `R1` → `RK(UNIT-TUJUAN, [KOSONG(B3-OBAT, 1)])` → `RC` | `044-U7` |
| `VC1` | `R1` → `RK(UNIT-TUJUAN, [SERAH(C1)])` → `RS` → `RB` → `RT` → `RM` → `RD` → `RX` | `041-S9` |
| `VC2` | `R1` → `RK(UNIT-TUJUAN, [SERAH(C2)])` → `RS` → `RB` → `RM` → `RD` → `RX` → `RT` | `041-S10` |
| `VC3` | `R1` → `RK(UNIT-TUJUAN, [SERAH(C3-TOLAK), KOSONG(C3-KOSONG, 2)])` → `RB` → `RT` → `RM` → `RD` → `RX`. **Tanpa** `RS` — pengajuan ditolak bila ada pesanan tanpa sikap | `041-S11`, `044-U6`, `044-U9` |
| `VD1` | Opsional — dipakai `044-U10` bila akun pembanding tersedia (bagian 6, Blok D) | `044-U10` |

Setiap langkah resep dicatat pada `data-uji.json`. Langkah resep yang gagal → skenario yang bergantung padanya `NOT RUN`
beserta kode dan pesannya.

---

## 5. Kueri pemeriksaan — hanya `SELECT`

Simpan teks kueri ini sebagai `kueri.sql` di folder bukti, dan jalankan dari skrip di folder yang sama (A10).

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

Email pada Q-P2 dibaca dari `process.env` dan **tidak** ditulis ke bukti; JSON memakai kode akun. `UpdateBy` dibandingkan
dengan `id` dari `GET /auth/me` akun yang bersangkutan (P6).

---

## 6. Skenario — kerjakan berurutan per blok

Semua skenario layar: akun `PERAWAT`, hasil build, 1440 × 900, rekam jaringan selama skenario, PNG viewport wajib.

### Blok A — `FE-IGD-043`: galat aksi kepergian tampil di dalam modal

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `043-U1` | Ruang Kerja `VA1` → *Riwayat* → kepergian `VA1` → *Catat Tiba* → konfirmasi | `POST …/arrive` `403`, `message` === `K-TIDAK-TIBA`. Modal *"Catat Tiba?"* **tetap terbuka** dan memuat `K-TIDAK-TIBA` (`includes` di dalam modal). Sesudah modal ditutup, kartu kepergian tetap *Sudah berangkat*. `nextjsPortalNull` benar |
| `043-U5` | Lanjutan `043-U1`: tutup modal dengan *Batal*; buka lagi *Catat Tiba* **tanpa** mengonfirmasi; periksa isi modal; tutup | Saat dibuka ulang, modal memuat `L-KONFIRMASI` dan **tidak** memuat `K-TIDAK-TIBA` maupun kotak galat |
| `043-U2` | Ruang Kerja `VA2` → kepergian `VA2` → *Terima Dokumen* → konfirmasi | `POST …/accept-handover` `403`, `message` === `K-BELUM-TINJAU`. Modal tetap terbuka dan memuat `K-BELUM-TINJAU`; modal dan respons **tidak** memuat `K-LAMA` |
| `043-U4` | Ruang Kerja `VA2` → *Tolak Dokumen* → ketik alasan `Uji 043 alasan utuh` → konfirmasi | `POST …/reject-handover` `403` `message` === `K-BELUM-TINJAU`. Modal tetap terbuka; kotak alasan masih berisi `Uji 043 alasan utuh` persis |
| `043-U3` | Ruang Kerja `VB1` → *Ajukan Serah Terima* → konfirmasi | `POST …/submit-handover` `400`, `message` === `K-AJUKAN` dengan `{n}` = 3. Modal tetap terbuka dan memuat kalimat itu |

### Blok B — `FE-IGD-044`: sikap pesanan dari tab Transfer

Kerjakan pada `VB1` berurutan: `044-U1` → `U3` → `U4` → `U5` → `U8`.

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `044-U1` | Lanjutan `043-U3`: tutup modal; periksa kartu kepergian `VB1` | Kartu memuat `L-BAGIAN`, `L-PENUNJANG`, `L-LAB`; tiga baris *Uji PB B1-OBAT*, *B1-LAB*, *B1-TINDAKAN*, masing-masing dengan `L-TANPA-SIKAP`, jenis (*Obat*, *Laboratorium*, *Tindakan*), asal *Luar sistem · UJI-PB-…*, dan *Waktu sikap* `-`. Jaringan memuat `GET …/emergency-departures/{VB1}/order-items` `200`. Tidak ada tanggal `0001` di kartu. Setiap baris menampilkan tombol *Continue*, *Handover*, *Cancel* |
| `044-U3` | Baris *B1-LAB* → *Continue* → *Simpan Sikap* | `PATCH …/order-items/{id}/action` `200`; badan `{ "item": { "orderKind": 3, "orderSource": 2, "externalReference": "UJI-PB-B1-LAB", "orderDescription": "Uji PB B1-LAB", "action": 1, … } }`. Modal tertutup; disusul `GET …/order-items` `200`; baris menampilkan *Sikap: Continue* dan tanpa tombol sikap |
| `044-U4` | Baris *B1-TINDAKAN* → *Cancel*: periksa tombol *Simpan Sikap* sebelum alasan diisi; isi alasan `Uji 044 batal`; simpan | Sebelum alasan: tombol *Simpan Sikap* `disabled`. Sesudah simpan: `PATCH …/action` `200`, badan `action` 9 dan `actionReason` === `Uji 044 batal`; baris menampilkan *Sikap: Cancel* dan *Alasan sikap: Uji 044 batal* |
| `044-U5` | Baris *B1-OBAT* → *Handover*; periksa modal; *Simpan Sikap* | Modal memuat *"Unit penerima: {UNIT}"* dan `L-UNIT-PENERIMA`; tidak ada pilihan unit. `PATCH …/action` `200`, badan `action` 2 dan `toServiceUnitId` = `toServiceUnitId` kepergian `VB1`. **Nol** permintaan ke `…/master-data/service-units` sejak tombol *Handover* ditekan sampai respons tiba. Baris menampilkan *Sikap: Handover* dan *Penerimaan: Menunggu penerimaan* |
| `044-U8` (`043-U6`) | *Ajukan Serah Terima* → konfirmasi | `POST …/submit-handover` `200`. Modal tertutup sendiri; disusul `GET …/emergency-departures` dan `GET …/order-items`; kartu menampilkan *Dokumen: Menunggu unit tujuan*. Ketiga baris pesanan tanpa tombol sikap. Catat atribut `disabled` tombol konfirmasi sesaat sesudah diklik (bila tertangkap) |
| `044-U2` | Ruang Kerja `VB2` → kepergian `VB2` | Kartu memuat `L-BAGIAN` dan `L-KOSONG`; tidak ada daftar kosong tanpa kalimat |
| `044-U7` | Ruang Kerja `VB3` → kepergian `VB3` (*Dibatalkan*) | Baris *Uji PB B3-OBAT* terbaca dengan `L-TANPA-SIKAP`; keterangan memuat `L-BATAL`; **tidak ada** tombol sikap |

### Blok C — sisa `BE-IGD-041` dan penutupan dari layar

Kerjakan pada `VC3` berurutan: `041-S11a` → `044-U6` → `041-S11b` → `044-U9` → `041-S11c` (A15).

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `041-S9` | Siapkan `VC1` sampai `RX`; catat respons `RX`, `GET /emergency-visits/{VC1}`, Q-PESANAN, Q-VISIT | `RX` `200`; `visitStatus` **9** — pesanan `Handover` yang masih **menunggu** penerimaan tidak menahan; Q-PESANAN: `AcceptanceStatus` 2, `IsEffective` true; Q-VISIT: `ClosedByDispositionId` = tindak lanjut `VC1` |
| `041-S10` (`061-S12` kaki terima) | Siapkan `VC2` sampai `RT`; catat `GET` kunjungan (tetap 7). `PENERIMA`: `POST …/order-items/{C2}/accept` (badan kosong); lalu `GET` kunjungan dan Q-VISIT | Sebelum terima: `visitStatus` 7. Terima: `200`, `acceptanceStatus` 3; `visitStatus` 9; `ClosedByDispositionId` = tindak lanjut `VC2`; `UpdateBy` = id `PENERIMA` |
| `041-S11a` (`061-S12` kaki tolak) | Siapkan `VC3` sampai `RX`; catat `GET` kunjungan. `PENERIMA`: `POST …/order-items/{C3-TOLAK}/reject` `{ "acceptanceStatus": 4, "rejectionReason": "Uji PB tolak" }`; lalu `GET /emergency-visits/{VC3}` | Sesudah `RX`: `visitStatus` 7, `awaitingClosureReason` === `K-PESANAN` dengan uraian `Uji PB C3-KOSONG`. Tolak: `200`; `visitStatus` tetap 7; `awaitingClosureReason` memuat `Uji PB C3-KOSONG` **dan** `Uji PB C3-TOLAK` |
| `044-U6` | **Layar.** Ruang Kerja `VC3` → baris *Uji PB C3-TOLAK* | Baris memuat *Alasan penolakan: Uji PB tolak* dan tombol *Sikap pengganti: Continue*, *Sikap pengganti: Handover*, *Sikap pengganti: Cancel*. Pilih *Sikap pengganti: Continue* → *Simpan Sikap* → `PATCH …/action` `200`. Sesudahnya daftar memuat baris lama dengan `L-TIDAK-BERLAKU` **dan** baris pengganti *Sikap: Continue* yang *Berlaku* |
| `041-S11b` (`BE-IGD-041` acceptance 14 — baris tergantikan) | `GET /emergency-visits/{VC3}`; Q-PESANAN | `visitStatus` tetap 7; `awaitingClosureReason` === `K-PESANAN` dengan uraian **hanya** `Uji PB C3-KOSONG` — baris lama yang tidak berlaku dan baris pengganti `Continue` tidak menahan. Q-PESANAN: baris lama `IsEffective` false; baris pengganti `SupersedesOrderItemId` = baris lama, `Action` 1 |
| `044-U9` | **Layar.** Masih di Ruang Kerja `VC3` **tanpa memuat ulang halaman**: baris *Uji PB C3-KOSONG* → *Continue* → *Simpan Sikap* | `PATCH …/action` `200`; disusul `GET /emergency-visits/{VC3}` (muat ulang kartu pasien); lencana status kunjungan di kartu pasien memuat `L-SELESAI` tanpa memuat ulang halaman. PNG sesudah respons + 1000 ms |
| `041-S11c` | `GET /emergency-visits/{VC3}`; Q-VISIT | `visitStatus` 9; `ClosedByDispositionId` = tindak lanjut `VC3`; `UpdateBy` = id `PERAWAT`; `EncounterStatus` 9 |

### Blok D — penolakan server di modal sikap (opsional)

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `044-U10` | Hanya bila pemilik menyediakan akun perawat **tanpa** penugasan di simpul `UNIT-IGD` yang dapat membuka Ruang Kerja (`QUILVIAN_PEMBANDING_EMAIL`, `QUILVIAN_PEMBANDING_PASSWORD`). Siapkan `VD1`: `R1` → `RK(UNIT-TUJUAN, [KOSONG(D1, 2)])`. Dengan akun pembanding: baris *Uji PB D1* → *Continue* → *Simpan Sikap* | `PATCH …/action` `403`, `message` === `K-TIDAK-SIKAP`. Modal tetap terbuka dan memuat kalimat itu. Akun tidak tersedia → `NOT RUN` |

### Blok E — pemeriksaan pendukung

| ID | Langkah | Yang diharapkan |
| --- | --- | --- |
| `PB-J1` | Dari `jaringan[]` seluruh skenario | Tidak ada permintaan ke `/api/v1/administrator/**`; tidak ada login akun selain `LOKET`, `PERAWAT`, `DOKTER`, `PENERIMA` (dan pembanding bila dipakai); tidak ada `RD` oleh akun selain `DOKTER` |
| `PB-G1` | Di akhir putaran: `git status --short` kedua repository | Sama dengan P1 — tidak ada berkas yang berubah, bertambah, atau terhapus karena putaran ini |

---

## 7. Bentuk bukti mentah

Folder **baru**: `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-bersama-<YYYYMMDD>/`.

| Berkas | Isi |
| --- | --- |
| `<ID>.json` | `{ id, akun (kode akun), percobaan, mulai, selesai, viewport, nextjsPortalNull, data{}, langkah[], jaringan[], kueri[], pemeriksaan{ <nama>: { harapan, teramati, lulus } }, putusan, alasan }` — `jaringan[]` berisi seluruh permintaan ke `/api/**` termasuk login, beserta kode akun pengirim; badan login disamarkan |
| `<ID>.png` | Tangkapan layar viewport menurut A9 untuk setiap skenario layar |
| `persiapan.json` | Hasil P1–P5: SHA, `git status --short`, waktu ubah berkas backend dan DLL, waktu `BUILD_ID` dan keempat berkas frontend, perintah proses pada port 3000 |
| `P6-<akun>.json` | `GET /auth/me` (hanya `id`, nama, `isSuperAdmin`) dan `GET /auth/permissions` tiap akun |
| `P7-unit.json`, `P7-penempatan.json` | Keluaran Q-UNIT dan Q-P2 — email diganti kode akun |
| `data-uji.json` | Pasien (nama dan RM), nomor encounter, nomor kunjungan, id kepergian, pesanan, dan tindak lanjut tiap kunjungan, beserta resep dan akun tiap langkah |
| `kueri.sql` | Teks kueri bagian 5 yang benar-benar dijalankan |
| Skrip | Semua skrip yang dijalankan, apa adanya dan per versi bila diubah. Kredensial hanya lewat `process.env` tanpa nilai cadangan |

Log backend `Logs/quilvian-backend-<YYYYMMDD>.json` **tidak dihapus atau disunting** — pengembang mencocokkan bukti dengan
log itu.

---

## 8. Rekap skenario

| Blok | Skenario | Jenis | Jumlah |
| --- | --- | --- | ---: |
| A | `043-U1`, `043-U5`, `043-U2`, `043-U4`, `043-U3` | Layar | 5 |
| B | `044-U1`, `044-U3`, `044-U4`, `044-U5`, `044-U8`, `044-U2`, `044-U7` | Layar | 7 |
| C | `041-S9`, `041-S10`, `041-S11a`, `041-S11b` | API | 4 |
| C | `044-U6`, `044-U9`, `041-S11c` | Layar / API | 3 |
| D | `044-U10` (opsional) | Layar | 1 |
| E | `PB-J1`, `PB-G1` | Pemeriksaan | 2 |
| **Jumlah** | | | **22** |

| Acceptance | Skenario |
| --- | --- |
| `FE-IGD-043` 1 | `043-U1` |
| `FE-IGD-043` 2 | `043-U2` |
| `FE-IGD-043` 3 | `043-U3` (penolakan lain); keenam tombol lewat baca source pengembang |
| `FE-IGD-043` 4 | PNG `043-U1`, `043-U2` |
| `FE-IGD-043` 5 | `043-U4` (kaki *Tolak Dokumen*; kaki *Batalkan* lewat baca source — penolakan pembatalan sulit dipicu tanpa mengubah data) |
| `FE-IGD-043` 6 | `043-U5` |
| `FE-IGD-043` 7 | `044-U8` (`043-U6`) |
| `FE-IGD-044` 1, 8 | `044-U1` |
| `FE-IGD-044` 2 | `044-U2` |
| `FE-IGD-044` 3 | `044-U3` |
| `FE-IGD-044` 4 | `044-U4` |
| `FE-IGD-044` 5 | `044-U5` |
| `FE-IGD-044` 6 | `044-U6` |
| `FE-IGD-044` 7 | `044-U7`, `044-U8` |
| `FE-IGD-044` 9 | `043-U3` (jalur galat-di-modal), `044-U10` (opsional) |
| `FE-IGD-044` 10 | `044-U8` |
| `FE-IGD-044` 11 | `044-U9`, `041-S11c` |
| `BE-IGD-041` 14 | `041-S9` (`Handover` menunggu), `041-S11b` (baris tergantikan) |
| `BE-IGD-041` 15 | `041-S10` (kaki terima), `041-S11a` (kaki tolak) |

---

## 9. Pelaporan

Tulis laporan baru di folder ini: `<YYYY-MM-DD>-laporan-uji-putaran-bersama.md`. Berkas panduan ini, laporan task, roadmap,
traceability, dan decision log **tidak disunting** (A17) — penandaan status dilakukan pengembang sesudah memeriksa bukti
mentah dan log backend.

Isi laporan:

1. Metadata: tanggal, pelaksana, SHA dan `git status --short` kedua repository dari P1 **dan** `PB-G1`, waktu DLL dan
   berkas backend (P2), waktu `BUILD_ID` dan berkas frontend (P3), proses pada port 3000 (P4), viewport, basis data dev
   **tanpa** alamat host.
2. Akun: kode akun yang dipakai tiap langkah (bukan email), akun opsional yang tidak tersedia, dan pernyataan bahwa
   SuperAdmin tidak dipakai serta Akses Role, penugasan HR, dan pemetaan unit tidak diubah agen — pernyataan ini
   dicocokkan pengembang dengan log.
3. Tabel per skenario: ID, percobaan ke berapa, `PASS` / `FAIL` / `NOT RUN`, kode status dan kalimat yang teramati, nama
   berkas bukti.
4. **Daftar seluruh percobaan** beserta waktu mulai dan selesai, termasuk yang gagal atau dihentikan (A11).
5. Data uji: isi `data-uji.json` dalam bentuk tabel, serta keluaran Q-UNIT dan Q-P2 (email diganti kode akun).
6. Setiap penyimpangan dari panduan ini: data pengganti, langkah tambahan, percobaan ulang, langkah yang dilewati.

| Blok | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| --- | --- | ---: | ---: | ---: | ---: |
| A | `043-U1`, `U5`, `U2`, `U4`, `U3` | 5 | | | |
| B | `044-U1`, `U3`, `U4`, `U5`, `U8`, `U2`, `U7` | 7 | | | |
| C | `041-S9`, `S10`, `S11a`, `S11b`, `044-U6`, `044-U9`, `041-S11c` | 7 | | | |
| D | `044-U10` | 1 | | | |
| E | `PB-J1`, `PB-G1` | 2 | | | |
| **Jumlah** | | **22** | | | |
