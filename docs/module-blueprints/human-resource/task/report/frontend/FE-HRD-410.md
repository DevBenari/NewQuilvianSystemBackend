# Laporan Perubahan Frontend — `FE-HRD-410`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-410` |
| Judul | Klien API kontrak `v31` |
| Slice | `A52-B` / `MVP-78` — pembuka task tertahan Penjadwalan & Kehadiran |
| Roadmap | `roadmap/frontend-roadmap.md`, kartu `FE-HRD-410` (amandemen `A52-B`) |
| Trace | `FR-PK-035` s.d. `FR-PK-038`; `HRD-DEC-251` s.d. `253`, `256`; `api-contract.md` 37.4–37.7 |
| Contract version | `v31` — approved 24 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Tidak ada layar yang dibuat atau diubah. Task ini hanya menyediakan fungsi service Axios; layar menjadi lingkup `FE-HRD-411` s.d. `FE-HRD-415` dan `FE-HRD-100` |
| Dependency | Kontrak `v31` ✅. Backend: `BE-HRD-359`, `BE-HRD-465`, `BE-HRD-466`, `BE-HRD-467` ✅ (endpoint sudah ada; diuji asap di bagian 6) |
| Klasifikasi | `MEDIUM` — tiga service diperluas, satu utilitas diperluas, satu berkas uji baru; tanpa layar, slice, atau store |
| Task mode | `FRONTEND` (instruksi pengguna 26 November 2026: "lanjutkan langkah berikutnya", sesudah `BE-HRD-468`) |
| Target tulis | `QuilvianSystemFrontendDev` (`src/lib/services/hr`, `src/utils/hr`, `tests/unit`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `11b3aaece` (branch `YogaV2`) + perubahan Lab milik sesi lain di working tree (tidak disentuh) |
| Commit backend yang dijadikan rujukan | `cce1dbf5` (branch `yoga`) + perubahan `BE-HRD-467`/`468` yang belum di-commit |
| Tanggal | 26 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Tidak ada pemanggilan ESS izin pulang cepat maupun ESS dinas luar | Pencarian `self-services/human-resource/early-leave-permissions` dan `…/official-duties` di `src/` tidak menemukan apa pun. Layar HR memakai jalur `attendance-management/early-leave-permissions` dan `…/official-duties` (`early-leave-service.js`, `official-duty-service.js`) | Pegawai belum punya jalur data untuk mengajukan sendiri (`FE-HRD-100`) |
| Tidak ada pemanggilan Perubahan Shift dan Tukar Dinas sisi HR | `employee-self-service.js` hanya punya `createScheduleChangeRequest`/`createShiftSwapRequest` versi layanan mandiri; pencarian `scheduling-management/schedule-change-requests` dan `shift-swap-requests` di `src/` kosong | Layar HR `FE-HRD-413`/`414` belum punya jalur data |
| Tidak ada pemanggilan daftar penempatan sementara | Pencarian `temporary-assignments` di `src/` kosong | Layar `FE-HRD-415` belum punya jalur data |
| Pembentuk query yang ada hanya membuang `undefined`/`null`/`""` | `buildQueryString` di tiap service | Teks berisi spasi saja tetap terkirim, dan `pageSize` tidak dibatasi |
| Nama berkas unduhan dari header sudah punya pembaca | `readAttachmentFileName` di `attendance-management-service.js` (`FE-HRD-306`/`315`) | Dipakai ulang untuk unduhan ESS dinas luar |
| Detail roster dan tutup periode | Respons diperluas di `BE-HRD-463`/`464`; fungsi `getRosterPeriodById` dan `closePeriod` meneruskan respons apa adanya | Sesuai kartu, tidak perlu fungsi baru |

---

## 2. Proses bisnis dari sisi pengguna

Task ini tidak menampilkan layar. Fungsi-fungsinya dipakai oleh layar berikut:

| Pengguna | Layar (task) | Fungsi yang dipakai |
| --- | --- | --- |
| Pegawai (mis. Sari, perawat ICU) | Layanan Mandiri → Izin Pulang Cepat (`FE-HRD-100`) | Muat pilihan status, lihat daftar dan detail izin miliknya, ajukan (tanggal, jam pulang, alasan), batalkan yang masih Diajukan dengan alasan |
| Pegawai | Layanan Mandiri → Dinas Luar (`FE-HRD-100`) | Muat kategori dan status, unggah foto surat tugas, ajukan dengan kunci lampiran itu, unduh lampiran surat tugasnya, batalkan yang masih Diajukan |
| Kepala Unit / Admin HRD | Perubahan Shift dan Tukar Dinas sisi HR (`FE-HRD-413`/`414`) | Metadata filter, ringkasan, daftar dan detail dalam cakupan unit, riwayat workflow, Terapkan dengan catatan |
| Kepala Unit, Kepala Bidang, Admin HRD | Penugasan Tambahan / Penempatan Sementara (`FE-HRD-415`) | Pilihan status waktu, daftar penempatan sementara dalam cakupan |

Contoh alur dinas luar yang dilayani fungsi baru:

1. Sari memilih berkas foto surat tugas. Layar memanggil `uploadMyOfficialDutyAttachment(file)` dan menerima `key`, misalnya `attendance-official-duty/2027/02/…/abc123.pdf`.
2. Sari mengisi kategori "Pengawalan Ambulans Rujukan", nomor surat, jam mulai/selesai, tujuan, dan uraian, lalu mengajukan. Layar memanggil `createMyOfficialDuty({ …, attachmentUrl: key })`. Bila layar ikut membawa `workforceProfileId` dari data lain, ruas itu **dibuang** sebelum dikirim, karena identitas pegawai diambil server dari sesi (`VR-PK-34`).
3. Sari membuka detailnya dan mengunduh lampiran. Layar memanggil `downloadMyOfficialDutyAttachment(key)` dan menerima `{ blob, fileName }`. Bila API berada di origin lain sehingga header `Content-Disposition` disembunyikan peramban, nama berkas tetap `lampiran-attendance-official-duty.pdf`, sama dengan nama dari server.

Jalur tidak normal diteruskan apa adanya kepada layar. Pesan server tetap terbaca karena galat diteruskan utuh. Pada unduhan, badan galat yang berbentuk Blob diurai menjadi JSON.

| Keadaan | Jawaban server yang diteruskan |
| --- | --- |
| Akun tanpa profil pegawai | `404` "Akun ini belum terhubung ke profil pegawai." |
| Lampiran bukan milik sendiri | `404` "Lampiran tidak ditemukan." |
| Batal sesudah diputuskan | `409` "Pengajuan sudah diputuskan dan tidak dapat dibatalkan." |
| Filter unit penempatan di luar cakupan | `403` |
| Terapkan perubahan shift unit lain | `403` "Anda tidak berwenang mengubah roster unit ini" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/frontend-architecture.md`, `test-policy.md`, `REPORT_TEMPLATE.md` (suite skill); kontrak `api-contract.md` 37.4–37.7; kartu `FE-HRD-410`; controller backend `EarlyLeavePermissionSelfServiceController`, `OfficialDutySelfServiceController`, `TemporaryAssignmentController`, `ScheduleChangeController`, `ShiftSwapController` beserta DTO-nya; `HrAttachmentStorage.Open` (format nama unduhan); service frontend `employee-self-service.js`, `attendance-management-service.js`, `roster-scheduling-service.js`, `workforce-core-cross-employee-service.js`, `early-leave-service.js`, `official-duty-service.js`; `src/utils/hr/hr-api-utils.js`; uji `tests/unit/hr-a52-api-client.test.mjs` (pola).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/hr/hr-api-utils.js` | `HR_MAX_PAGE_SIZE` (100). `sanitizeHrListQuery(params)`: salinan query yang hanya memuat parameter berisi (membuang `undefined`, `null`, teks kosong atau spasi saja, dan memangkas teks) dengan `pageSize` dibulatkan ke 1..100; `pageSize` bukan angka dibuang. `pickHrPayload(payload, keys)`: hanya ruas kontrak yang dikirim |
| `src/lib/services/hr/employee-self-service.js` | Lima fungsi ESS izin pulang cepat (`getMyEarlyLeaveFilterMetadata`, `getMyEarlyLeavePermissions`, `getMyEarlyLeavePermissionById`, `createMyEarlyLeavePermission`, `cancelMyEarlyLeavePermission`) dan tujuh fungsi ESS dinas luar (`getMyOfficialDutyFilterMetadata`, `getMyOfficialDuties`, `getMyOfficialDutyById`, `uploadMyOfficialDutyAttachment`, `downloadMyOfficialDutyAttachment`, `createMyOfficialDuty`, `cancelMyOfficialDuty`). Payload diambil dari daftar ruas kontrak. Unduhan mengembalikan `{ blob, fileName }` dengan nama cadangan berformat server |
| `src/lib/services/hr/roster-scheduling-service.js` | Enam fungsi Perubahan Shift sisi HR dan enam fungsi Tukar Dinas sisi HR (metadata, ringkasan, daftar, detail, workflow, terapkan). Terapkan hanya mengirim `notes` |
| `src/lib/services/hr/workforce-core-cross-employee-service.js` | `getTemporaryAssignmentFilterMetadata`, `getTemporaryAssignments` |
| `tests/unit/hr-a52b-api-client.test.mjs` (baru) | 8 uji: utilitas query/payload, path dan method tiap kelompok, payload ESS tanpa `workforceProfileId`, unduhan dengan dan tanpa header, galat unduh Blob, serta daftar putih URL kontrak `v31` dengan kontrol pola |

### 3.3 Kepatuhan arsitektur frontend

- Semua request memakai `InstanceAxios`; tidak ada Axios instance baru. `signal` diteruskan pada setiap GET.
- Fungsi ditambahkan ke service domain yang sudah ada, sesuai kartu (`employee-self-service.js`, `workforce-core-cross-employee-service.js`, pola `attendance-management-service.js`). Fungsi Perubahan Shift dan Tukar Dinas sisi HR ditempatkan di `roster-scheduling-service.js`, karena berbagi base `scheduling-management` dan belum ada service HR lain untuk keduanya.
- Pembentuk query lokal (`buildQueryString`) dan `unwrap` di tiap service dipakai seperti apa adanya. Yang baru hanya dua helper murni bersama di `hr-api-utils.js`, supaya batas 100 dan saringan ruas tidak ditulis ulang di tiga berkas.
- `readAttachmentFileName` dan `withHrJsonErrorBody` dipakai ulang dari `FE-HRD-306`/`315`.
- Tidak ada Redux slice baru. Kartu hanya meminta fungsi service; state layar menjadi keputusan task layar.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` — task ini tanpa layar |
| Kosong | `NOT APPLICABLE` — task ini tanpa layar |
| Gagal | `NOT APPLICABLE` di layar. Lapisan service meneruskan galat Axios utuh, sehingga pesan server dapat ditampilkan layar (`toHrApiError`) |
| Tanpa hak akses | `NOT APPLICABLE` di layar. `403` diteruskan utuh |

---

## 5. Endpoint yang dikonsumsi

#### Self Services / Human Resource / Early Leave Permission

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/self-services/human-resource/early-leave-permissions/filters/metadata` | Pilihan status | `MyEarlyLeavePermission : Read` |
| `GET` | `/v1/self-services/human-resource/early-leave-permissions` | Daftar milik sendiri (`fromDate`, `toDate`, `status`, `pageNumber`, `pageSize` ≤ 100) | `MyEarlyLeavePermission : Read` |
| `GET` | `/v1/self-services/human-resource/early-leave-permissions/{id}` | Detail | `MyEarlyLeavePermission : Read` |
| `POST` | `/v1/self-services/human-resource/early-leave-permissions` | Ajukan `{ attendanceDate, requestedEarlyLeaveAt, reason }` | `MyEarlyLeavePermission : Create` |
| `POST` | `/v1/self-services/human-resource/early-leave-permissions/{id}/cancel` | Batal `{ reason }` | `MyEarlyLeavePermission : Cancel` |

#### Self Services / Human Resource / Official Duty

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/self-services/human-resource/official-duties/filters/metadata` | Pilihan kategori dan status | `MyOfficialDuty : Read` |
| `GET` | `/v1/self-services/human-resource/official-duties` | Daftar milik sendiri | `MyOfficialDuty : Read` |
| `GET` | `/v1/self-services/human-resource/official-duties/{id}` | Detail | `MyOfficialDuty : Read` |
| `POST` | `/v1/self-services/human-resource/official-duties/attachments` | Unggah lampiran (`multipart/form-data`, `file`) | `MyOfficialDuty : Create` |
| `GET` | `/v1/self-services/human-resource/official-duties/attachments/download?url=` | Unduh lampiran milik sendiri | `MyOfficialDuty : Read` |
| `POST` | `/v1/self-services/human-resource/official-duties` | Ajukan `{ dutyCategory, assignmentOrderNumber, startAt, endAt, destination, purpose, latitude?, longitude?, attachmentUrl? }` | `MyOfficialDuty : Create` |
| `POST` | `/v1/self-services/human-resource/official-duties/{id}/cancel` | Batal `{ reason }` | `MyOfficialDuty : Cancel` |

#### Corporate / Human Resource / Workforce Core / Temporary Assignment

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-core/temporary-assignments/filters/metadata` | Pilihan status waktu | `TemporaryAssignment : Read` |
| `GET` | `/v1/corporate/human-resource/workforce-core/temporary-assignments` | Daftar (`organizationUnitId`, `fromDate`, `toDate`, `timeStatus`, `search`, `pageNumber`, `pageSize` ≤ 100) | `TemporaryAssignment : Read` |

#### Corporate / Human Resource / Scheduling Management / Schedule Change Administration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/filters/metadata` | Pilihan jenis dan status | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/summary` | Ringkasan per status | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests` | Daftar dalam cakupan | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/{id}` | Detail | `ScheduleChangeRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/{id}/workflow` | Riwayat workflow | `ScheduleChangeRequest : Read` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/schedule-change-requests/{id}/apply` | Terapkan `{ notes? }` | `ScheduleChangeRequest : Update` |

#### Corporate / Human Resource / Scheduling Management / Shift Swap Administration

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/filters/metadata` | Pilihan status | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/summary` | Ringkasan per status | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests` | Daftar dalam cakupan | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}` | Detail | `ShiftSwapRequest : Read` |
| `GET` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}/workflow` | Riwayat workflow | `ShiftSwapRequest : Read` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}/apply` | Terapkan `{ notes? }` | `ShiftSwapRequest : Update` |

### Delta terhadap kontrak dan kartu

| Hal | Kontrak/kartu | Pelaksanaan | Alasan |
| --- | --- | --- | --- |
| `filters/metadata` Perubahan Shift / Tukar Dinas | Kartu menyebut "metadata"; kontrak 37.7 tidak mencantumkannya | Dibuat | Endpoint sudah ada di backend sejak sebelum `A52-B` dan dibutuhkan filter layar |
| `POST {id}/workflow/synchronize` | Tabel API roadmap bagian 32 menyebutnya; kartu dan kontrak 37.7 tidak | Tidak dibuat | Di luar daftar fungsi kartu. Dapat ditambah bila layar `FE-HRD-413`/`414` membutuhkannya |
| Ruas tambahan pada payload | AC1: "payload ESS tanpa `workforceProfileId`" | Hanya ruas DTO kontrak yang dikirim, untuk semua POST baru (termasuk Terapkan) | Ruas lain tidak pernah sampai ke server, walau layar keliru membawanya |
| Teks spasi saja | AC2: "hanya parameter berisi" | Ikut dibuang; teks dipangkas | Spasi saja tidak berisi. Semula `buildQueryString` meneruskannya (`search=++`) |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hr-a52b-api-client.test.mjs tests/unit/hr-a52-api-client.test.mjs` | 20/20 lulus (8 baru + 12 `FE-HRD-306`) | `PASS` | Keluaran perintah |
| Kontrol negatif: 14 mutasi pada service/utilitas (path salah, payload mentah, tanpa saringan, batas 1000, spasi tidak dibuang, query `key`, nama cadangan lain, galat Blob tidak diurai, `PUT`, path workflow, path penempatan, tanpa header multipart) | 14/14 tertangkap; hash SHA-256 keempat berkas sama dengan sebelum mutasi | `PASS` | `mut410.cjs`, `mut410-hasil.log`, `hash.txt` di scratchpad sesi (`fe410/`) |
| Uji unit yang mengimpor modul yang diubah (6 berkas) | 48/52 lulus. 4 gagal: `ESS-004` (teks view profil ESS) dan 3 uji `FIX-HRD-001` yang membuka berkas yang sudah tidak ada (`salary-assignments-view.jsx`, `payroll-restitution-installment-view.jsx`, `payroll-integration-service.js`) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; tidak satu pun menyentuh fungsi yang diubah |
| `npm run test:unit` (suite penuh) | 5.437 lulus, 152 gagal dari 5.589. Tidak ada kegagalan di uji klien A52/A52-B. Kegagalan berada di modul lain (menu, payroll, rawat inap, billing, Lab, dll.). Baseline terakhir yang tercatat 46/4.612 (9 November), sebelum merge `11b3aaece` | `EXISTING / ENVIRONMENT ISSUE` | `fe410/unit.log` |
| `npx eslint` atas lima berkas yang diubah/baru | 0 galat, 0 peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` (seluruh repository) | Tanpa galat | `PASS` | `fe410/lint.log` |
| `npm run build` | "Compiled successfully in 101s", 931 halaman statis, `prepare-standalone` siap. Percobaan pertama ditolak "Another next build process is already running" karena build sesi lain; diulang saat frontend tidak dipakai | `PASS` | `fe410/build.log` |
| Uji asap baca-saja ke backend lokal (superadmin seed; 13 GET baru + 1 path salah sebagai kontrol) | Semua path ada di backend: metadata ESS izin/dinas luar `200`; daftar ESS dan unduh `404` "Akun ini belum terhubung ke profil pegawai." (superadmin tanpa profil pegawai, `VR-PK-34`); penempatan sementara metadata dan daftar `200` (2 data uji); Perubahan Shift dan Tukar Dinas metadata/ringkasan/daftar `200`; path salah `404` tanpa pesan | `PASS` | `fe410/asap.mjs`, `fe410/asap.log` |

Uji manual: `NOT APPLICABLE` — task ini tidak membuat atau mengubah kontrol interaktif. Layar diverifikasi manual di `FE-HRD-411` s.d. `415` dan `FE-HRD-100`.

**Tidak dijalankan:** POST ke backend sungguhan (ajukan, batal, unggah, terapkan), karena membutuhkan akun pegawai berprofil dan berizin ESS (G3) serta menulis data. Bentuk payload dibuktikan di uji unit dan dicocokkan dengan DTO backend.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Path dan method sama persis dengan kontrak 37.4–37.7; payload ESS tanpa `workforceProfileId` | Terpenuhi | Uji path/method per kelompok dan daftar putih URL `v31`; uji payload menyisipkan `workforceProfileId`/`attendanceDailyId` lalu memastikan tidak terkirim; uji asap: semua GET dijawab rute backend yang sebenarnya |
| 2. Query daftar mengirim hanya parameter berisi; `pageSize` maksimal 100 | Terpenuhi | Uji utilitas (`500→100`, `0→1`, `25.9→25`, `"abc"` dibuang, spasi/kosong/`null` dibuang); uji daftar ESS, penempatan, dan Perubahan Shift/Tukar Dinas |
| 3. Unduh lampiran ESS memakai nama cadangan bila `Content-Disposition` tidak terbaca (CORS) | Terpenuhi | Uji unduhan: header ada → nama dari header; header hilang → `lampiran-attendance-official-duty.pdf` (format `HrAttachmentStorage.Open`); kunci tanpa ekstensi → tanpa ekstensi |
| DoD: AC terbukti, laporan `task/report/frontend/FE-HRD-410.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Suite unit penuh memuat 152 kegagalan yang sudah ada di modul lain; perlu baseline baru sesudah merge `11b3aaece` |
| Masalah yang diketahui | `workflow/synchronize` belum punya fungsi (lihat delta). Bentuk respons ditelusuri dari DTO backend, bukan dari kontrak 37.7 yang tidak merinci ruas |
| Dependency backend | Tidak ada yang tertunda. Hak ESS dan `TemporaryAssignment` belum diberikan ke peran mana pun (G3), sehingga di devYoga layar akan menerima `403` sampai Admin Sistem memberikannya |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi lain sedang menjalankan E2E Lab dengan server standalone 3710 dan build sendiri. Build task ini ditunda sampai frontend tidak dipakai, supaya `.next/standalone` yang sedang dipakai tidak tertimpa. Percobaan pertama bertabrakan dengan build sesi itu (ditolak oleh `.next/lock`, tanpa kerusakan) lalu diulang |
| Status Git | Berkas task ini: `M src/lib/services/hr/employee-self-service.js`, `M src/lib/services/hr/roster-scheduling-service.js`, `M src/lib/services/hr/workforce-core-cross-employee-service.js`, `M src/utils/hr/hr-api-utils.js`, `?? tests/unit/hr-a52b-api-client.test.mjs`. Perubahan Lab di working tree milik sesi lain. Tidak ada stage, commit, atau push |
| Langkah berikutnya | `FE-HRD-411` (matriks roster menampilkan petugas pengganti) |
