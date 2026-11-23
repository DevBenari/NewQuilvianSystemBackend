# Laporan Perubahan Frontend — `FE-HRD-312`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-312` |
| Judul | Layar mesin absen |
| Slice | `A52` / `MVP-67` / `EPIC HRD-112` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-312` |
| Trace | `FR-PK-013`, `FR-PK-014`; `HRD-DEC-212`; `03-frontend-architecture.md` 31.2 (baris Mesin absen), 31.3 |
| Contract version | `v26` — `api-contract.md` bagian 32 "Master Data / Attendance Device", `approved` 12 November 2026. Delta: `GET …/attendance-devices/filters/metadata` dipakai (lihat bagian 1) |
| Wewenang UI | Layar baru `/hr/attendance/devices` (rute `DEV_DISCRETION`) dan butir menu **Mesin Absen** di grup presensi. Tata letak `DEV_DISCRETION`; base component wajib |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-346` ✅ |
| Klasifikasi | `MEDIUM` — satu layar baru, satu utilitas, perluasan slice, menu, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjutkan") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/app/hr/attendance/devices`, `src/components/view/hr/attendance`, `src/utils/hr/attendance`, `src/lib/state/slice/hr/master-data`, `src/utils/menu-sidebar/menu-items.jsx`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) + working tree `FE-HRD-306` s.d. `311` |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — acceptance criteria terpenuhi di browser dengan API tiruan; `UAT-PK-10` dengan data nyata belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Belum ada layar mesin absen | Tidak ada rute atau pemanggilan `attendance-devices` di `src/` selain klien `FE-HRD-306` | Scan dari mesin yang belum terdaftar tertahan (`HRD-DEC-212`) tanpa ada cara mendaftarkannya dari aplikasi |
| **Delta kontrak**: jenis mesin hanya tersedia lewat `filters/metadata` | `CreateAttendanceDeviceRequest.DeviceType` wajib salah satu `DeviceTypeOptions` (validasi `AttendanceDeviceService`: "Jenis mesin tidak valid."). `GET …/attendance-devices/filters/metadata` ada di `AttendanceDeviceController`, tetapi tidak tercantum di tabel kontrak `v26` §32, sehingga `FE-HRD-306` tidak mengekspornya | Tanpa endpoint itu layar harus menanam daftar jenis mesin sendiri. Endpoint dipakai dan dicatat sebagai **penyegaran kontrak** untuk pass blueprint berikutnya; kontrak yang terkunci tidak disunting |
| Identitas mesin nonaktif ikut muncul sebagai "tak dikenal" | `UnregisteredDeviceResponse.InactiveAttendanceDeviceId` terisi bila identitas itu milik mesin nonaktif | Baris seperti itu cukup diaktifkan, tidak perlu didaftarkan ulang |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: petugas IT (`AttendanceDevice : Read/Create/Update/Delete`).

**Contoh UAT-PK-10.** Mesin GDB-01 belum terdaftar. Sejak 07.00 pegawai scan di mesin itu; scannya ditahan dan pegawai tetap "Belum Hadir".

1. Petugas IT membuka **Mesin Absen**. Bagian **Mesin Tak Dikenal** menampilkan baris `GDB-01` dengan **12** scan tertahan serta waktu scan pertama dan terakhir.
2. Ia menekan **Daftarkan** pada baris itu. Formulir terisi otomatis: Kode Mesin `GDB-01`, ID Eksternal `GDB-01`, Nama `Mesin GDB-01`, dan jenis mesin pertama dari server. Di atas formulir tertulis: "Identitas GDB-01 diisi otomatis dari mesin tak dikenal. Sesudah disimpan dalam keadaan aktif, 12 scan yang tertahan akan dilepas dan dihitung tanpa scan ulang."
3. Sesudah **Daftarkan**, muncul dua pemberitahuan: "Mesin GDB-01 didaftarkan." dan ringkasan dari server, misalnya "12 scan tertahan dilepas: 12 siap dihitung. Rekap kehadiran tanggal terkait dihitung ulang otomatis." Baris GDB-01 hilang dari Mesin Tak Dikenal dan muncul di Daftar Mesin Absen.
4. Identitas `GDB-02` milik mesin yang sedang nonaktif tampil dengan tombol **Aktifkan GDB-02**. Mengaktifkannya melepas 3 scan tertahan dengan ringkasan yang sama.

**Daftar Mesin Absen** menampilkan nama dan kode, jenis (label server), identitas (ID eksternal dan nomor seri), alamat IP:port, lokasi, sinkron terakhir, scan tertahan, dan status. Daftar dapat disaring dengan pencarian, jenis, dan status. Tersedia aksi **Ubah**, **Aktifkan/Nonaktifkan**, dan **Hapus**, masing-masing hanya bagi yang berhak.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Identitas bentrok dengan mesin lain (409) | Pesan server di formulir: "Identitas mesin sudah dipakai mesin lain" |
| Menghapus mesin yang sudah punya log scan (409) | Pesan server di konfirmasi: "…tidak dapat dihapus. Nonaktifkan mesin bila tidak dipakai lagi." |
| Kode, nama, atau jenis kosong; port di luar 1–65535; ruas terlalu panjang | Pesan per ruas; tidak ada permintaan simpan |
| Pelepasan scan gagal walau mesin tersimpan | Pesan server (`heldScanReleaseMessage`) tampil sebagai peringatan |
| Pelepasan sebagian ditolak (lokasi/rumah sakit tidak sesuai) atau gagal dicocokkan | Ringkasan tampil sebagai peringatan, bukan sukses |
| Tidak ada scan tertahan | "Tidak ada scan yang tertahan. Seluruh scan berasal dari mesin yang terdaftar dan aktif." |
| Gagal memuat mesin tak dikenal | Pesan server dan tombol **Coba Lagi** |
| Hanya hak baca | Tanpa tombol daftar/ubah/aktif-nonaktif/hapus; baris identitas mesin nonaktif bertuliskan "Milik mesin nonaktif GDB-02" |
| Tanpa hak baca | "Anda tidak berwenang membuka daftar mesin absen."; tidak ada permintaan data |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-312`; `api-contract.md` 32; `03-frontend-architecture.md` 31.2; `00-interview-decisions.md` `HRD-DEC-212`; `04-prd-to-mvp.md` `UAT-PK-10`; `AttendanceDeviceController.cs`, `AttendanceDeviceDtos.cs`, `AttendanceDeviceService.cs` (validasi, konflik identitas, aturan hapus), `AttendanceRawLogDtos.cs` (`AttendanceRawLogReleaseResponse`); `master-data-resource-slice-factory.jsx`, `master-data-attendance-device-slice.jsx`, `master-data-leave-type-view.jsx` (pola master data HR), `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/attendance/devices/page.jsx` (baru) | Rute `/hr/attendance/devices` |
| `src/components/view/hr/attendance/attendance-devices-view.jsx` (baru) | Bagian Mesin Tak Dikenal (Daftarkan/Aktifkan), Daftar Mesin Absen dengan filter dan paginasi, modal tambah/ubah, konfirmasi aktif/nonaktif/hapus, pemberitahuan hasil pelepasan scan |
| `src/utils/hr/attendance/attendance-device-form-utils.js` (baru) | `toDeviceForm`, `toDeviceFormFromUnregistered` (isi otomatis identitas), `validateDeviceForm` (batas sama dengan DTO server), `buildDevicePayload` (POST dengan `isActive`, PUT tanpa), `describeHeldScanRelease` |
| `src/lib/state/slice/hr/master-data/master-data-attendance-device-slice.jsx` | Mengekspor `getAttendanceDeviceFilterMetadata` dan selektornya (delta kontrak di atas) |
| `src/utils/menu-sidebar/menu-items.jsx` | Butir **Mesin Absen** di grup presensi (`AttendanceDevice : Read`); impor ikon `RiFingerprintLine` |
| `tests/unit/hr-attendance-device-form-utils.test.mjs` (baru) | 6 uji |
| `tests/e2e/hr-attendance-devices.spec.mjs` (baru) | 8 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

Data mesin memakai slice factory master data dari `FE-HRD-306` (Redux, `unwrap()` untuk galat server `{ message, statusCode }`); daftar mesin tak dikenal memakai `AttendanceManagementService.getUnregisteredAttendanceDevices` lewat `useHrRemoteData`. Hak memakai `useStrictPermissions`. Pola master data HR berbasis konfigurasi dan rute terpisah (misalnya `leave-type`) tidak diikuti, karena layar ini satu halaman dengan dua daftar yang saling terkait dan formulir dalam modal, sama seperti layar `A52` lain (`FE-HRD-307` s.d. `311`). Seluruh kontrol memakai base component. Aturan formulir berada di `src/utils/hr/attendance` sebagai fungsi murni.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kedua tabel menampilkan teks memuat; tombol simpan/konfirmasi menampilkan pemutar dan modal tidak dapat ditutup selama permintaan berjalan |
| Kosong | Mesin tak dikenal: "Tidak ada scan yang tertahan. …"; daftar mesin: "Belum ada mesin absen terdaftar." |
| Gagal | Pesan server; mesin tak dikenal dengan **Coba Lagi** |
| Tanpa hak akses | "Anda tidak berwenang membuka daftar mesin absen."; tombol aksi disembunyikan sesuai hak, server tetap memeriksa |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Master Data / Attendance Device

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/attendance-devices/filters/metadata` | Pilihan jenis mesin (`deviceTypeOptions`) — **belum tercantum di kontrak `v26`** | `AttendanceDevice : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/attendance-devices` | Daftar (`search`, `deviceType`, `isActive`, paginasi) | `AttendanceDevice : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/attendance-devices/unregistered` | Mesin tak dikenal + jumlah scan tertahan | `AttendanceDevice : Read` |
| `POST` | `/v1/corporate/human-resource/master-data/attendance-devices` | Mendaftarkan; respons `heldScanRelease` | `AttendanceDevice : Create` |
| `PUT` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}` | Mengubah | `AttendanceDevice : Update` |
| `PATCH` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}/status` | Aktif/nonaktif; mengaktifkan melepas scan tertahan | `AttendanceDevice : Update` |
| `DELETE` | `/v1/corporate/human-resource/master-data/attendance-devices/{id}` | Menghapus (ditolak bila sudah punya log) | `AttendanceDevice : Delete` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `hr-attendance-devices.spec.mjs` (build produksi, port 3710, API tiruan berkeadaan, nol tulis) | 8/8 lulus pada putaran pertama | `PASS` | Keluaran Playwright |
| — `UAT-PK-10` | Baris GDB-01 (12 scan) → Daftarkan → kode/ID eksternal/nama/jenis terisi; badan `POST` membawa `externalDeviceId: "GDB-01"`, `isActive: true`; dua pemberitahuan; GDB-01 pindah dari Mesin Tak Dikenal ke Daftar Mesin | `PASS` | Skenario 1 |
| — Identitas milik mesin nonaktif | **Aktifkan GDB-02** → `PATCH …/{id}/status` `{ isActive: true }` → ringkasan 3 scan dilepas | `PASS` | Skenario 2 |
| — Ubah + 409 | Badan `PUT` tanpa `isActive`; pesan "Identitas mesin sudah dipakai mesin lain" di formulir | `PASS` | Skenario 3 |
| — Hapus ditolak 409 | Pesan server di konfirmasi | `PASS` | Skenario 4 |
| — Validasi | Pesan wajib kode/nama; tidak ada `POST` | `PASS` | Skenario 5 |
| — Filter | `deviceType=Face`, `isActive=true` terkirim | `PASS` | Skenario 6 |
| — Hak baca saja / tanpa hak | Tanpa tombol aksi; "Milik mesin nonaktif GDB-02"; tanpa hak → pesan dan tanpa permintaan | `PASS` | Skenario 7 dan 8 |
| `tests/unit/hr-attendance-device-form-utils.test.mjs` | 6/6 lulus | `PASS` | Keluaran perintah |
| `tests/unit/hr-a52-api-client.test.mjs` (regresi `FE-HRD-306` sesudah slice diperluas) | 12/12 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif: 6 mutasi (ID eksternal tidak diisi otomatis; kode tidak dipotong; port tidak divalidasi; `isActive` ikut `PUT`; ringkasan tanpa "siap dihitung"; pesan gagal pelepasan diabaikan) | 6/6 tertangkap; source dipulihkan | `PASS` | `mut312.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.261 lulus, 100 gagal dari 5.361 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik per nama dengan putaran `FE-HRD-306`; seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npx eslint` atas berkas task | Tanpa error maupun peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — seluruh kontrol baru (Daftarkan, Aktifkan GDB-02, Daftarkan Mesin, Ubah, Simpan Perubahan, Aktifkan/Nonaktifkan, Hapus, filter jenis dan status) dijalankan di peramban Chromium lewat Playwright terhadap build produksi.

**Tidak dijalankan:** `UAT-PK-10` dengan data nyata. Mendaftarkan mesin di devYoga melepas scan tertahan sungguhan dan menjadwalkan hitung ulang kehadiran. Data uji mesin `UJI-A52-GDB-01`/`-02` dan log tertahan `UJI-A52-MESIN-99` sudah ada di devYoga dan dapat dipakai bila diizinkan. Perilaku backend dibuktikan `AT-HRD-PK-29`, `-30` (laporan `BE-HRD-345`, `BE-HRD-346`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Mendaftarkan mesin dari baris "mesin tak dikenal" mengisi identitas otomatis | Terpenuhi | Skenario 1; uji unit 1 dan 2; mutasi M1/M2 |
| Outcome: petugas IT mendaftarkan mesin dan melihat mesin tak dikenal beserta jumlah scan tertahan | Terpenuhi | Skenario 1, 2, dan 6 |
| DoD umum: base component, memuat/kosong/galat/coba lagi, tombol nonaktif saat mengirim, tidak berwenang, uji unit dan browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-10` | Bagian layar terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Skenario 1 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Ruas rumah sakit dan lokasi absensi (`HospitalSiteId`, `AttendanceLocationId`) belum ada di formulir, karena belum ada pemilih sumber daya lokasi absensi di frontend; keduanya opsional di server |
| Masalah yang diketahui | **Penyegaran kontrak**: `GET …/attendance-devices/filters/metadata` dipakai tetapi belum tercantum di `api-contract.md` §32. Perlu ditambahkan pada pass blueprint berikutnya (kontrak terkunci hash, tidak disunting saat delivery) |
| Dependency backend | `NONE` |
| Perubahan sampingan | Playwright mengubah `test-results/.last-run.json` (ter-track); dipulihkan dari `HEAD` dengan akhir baris CRLF |
| Interupsi | `NONE` |
| Status Git | `?? src/app/hr/attendance/devices/`, `?? src/components/view/hr/attendance/attendance-devices-view.jsx`, `?? src/utils/hr/attendance/attendance-device-form-utils.js`, `?? tests/unit/hr-attendance-device-form-utils.test.mjs`, `?? tests/e2e/hr-attendance-devices.spec.mjs`, ` M src/utils/menu-sidebar/menu-items.jsx`, slice mesin absen (berkas baru `FE-HRD-306`); ditambah berkas `FE-HRD-306` s.d. `311` yang belum di-commit |
| Langkah berikutnya | `FE-HRD-313` (layar master hari libur) |
