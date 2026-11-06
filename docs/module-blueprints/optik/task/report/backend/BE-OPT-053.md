# Laporan Perubahan Backend — `BE-OPT-053`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-053` |
| Judul | Uji menyeluruh: privasi log, registri izin, alur penuh di harness |
| Slice | `MVP-4` — `EPIC OPT-00`..`OPT-11` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-053` |
| Trace | `NFR-001`..`007` (PRD §17) |
| Contract version | `OPT-API-v1` seluruh grup — `approved` sisi Optik |
| Penyetuju modul lain | Tidak ada perubahan kode modul lain |
| Dependency | `BE-OPT-052` ✅ (dan seluruh task `MVP-0`..`MVP-4` sebelumnya ✅) |
| Klasifikasi | `LOW` — hanya berkas uji |
| Task mode | `BACKEND` (verifikasi) |
| Target tulis | `NewQuilvianSystemBackend`: `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003`, `030`..`052` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | Tidak ada kode aplikasi yang berubah; hanya `Tests/` |
| QBE yang berlaku | Tidak ada berkas produksi yang disentuh, sehingga checker tidak dijalankan untuk task ini |
| Wewenang terpisah | Source uji ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Tiap task backend Optik sudah punya uji sendiri, tetapi belum ada bukti bahwa (a) **seluruh** endpoint Optik
berizin sesuai matriks izin kontrak, (b) alur pelayanan dari kunjungan sampai remake garansi berjalan utuh lewat
controller dengan satu tagihan yang benar, dan (c) log audit tidak membocorkan isi klinis atau data pribadi
(`NFR-001`..`007`).

---

## 2. Proses bisnis yang diuji

### 2.1 Matriks izin

Daftar 44 endpoint Optik (method, path, resource, action) ditulis sebagai matriks di uji, lalu dibandingkan dengan
endpoint yang benar-benar ada di controller lewat refleksi. Uji gagal bila ada endpoint baru tanpa baris matriks,
baris matriks tanpa endpoint, atau izin yang berbeda.

### 2.2 Alur penuh lewat controller

**Contoh:** pasien datang ke Optik, lalu:

1. Kunjungan Optik dibuat.
2. RO memeriksa: visus 6/21 dan 6/24, SPH −2,75/−3,25, PD 63,5; pemeriksaan difinalkan dan resep diterbitkan.
3. Pesanan kacamata dibuat (frame + lensa) lalu ditagih; pemeriksaan ikut ditagih.
4. Pasien membayar DP Rp 600.000; pesanan dikirim ke pengerjaan (stok frame keluar), lalu ditandai siap.
5. Pasien melunasi Rp 525.000 dan kacamata diserahkan kepada **Dewi Lestari**.
6. Hasilnya: **satu** tagihan Rp 1.125.000 dengan baris pemeriksaan `COMPLETED` dan dua baris barang `HANDED_OVER`;
   tagihan final dan kunjungan `Completed`.
7. Hari ke-10 pasien mengeluh "pusing berputar-putar saat membaca"; pemeriksaan ulang menemukan "sumbu silinder
   bergeser"; klaim garansi disetujui dan pesanan remake dikirim, siap, lalu diserahkan **tanpa tagihan baru**.

### 2.3 Privasi log

Seluruh log yang ditulis selama alur di atas ditangkap. Log wajib memuat nomor pesanan dan nomor klaim (agar bisa
dilacak), tetapi tidak boleh memuat nama penerima, isi keluhan, temuan pemeriksaan, atau nilai resep.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak `OPT-API-v1` (seluruh grup) dan matriks izin; semua controller Optik; `LoggerService`; harness
`PesananUji` dari task sebelumnya.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Tests/QuilvianSystemBackend.OpticalTests/Be053EndToEndTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 2 uji: matriks 44 endpoint dan alur penuh + privasi log |
| `Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/PesananUji.cs` | **Diperbarui** (diabaikan Git). Penangkap log, konfigurasi uji, `BayarAsync` menerima kunjungan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak berubah. Uji mengunci 44 endpoint sesuai kontrak |
| Database | Tidak ada |
| Keamanan/Auth | Tidak berubah. Uji gagal bila izin endpoint menyimpang dari matriks |

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet` berjalan | `PASS` | `tasklist` |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 239 lulus, 0 gagal (2 baru) | `PASS` | Keluaran `dotnet test` |
| Matriks izin | 44 endpoint sama persis dengan controller (method, path, resource, action) | `PASS` | Uji refleksi |
| Alur penuh | Satu tagihan Rp 1.125.000; status baris benar; tagihan final; kunjungan `Completed`; remake tanpa tagihan baru | `PASS` | Uji E2E |
| Privasi log | Nomor pesanan dan klaim ada; nama penerima, keluhan, temuan, dan resep tidak ada | `PASS` | Uji E2E |
| Uji mutasi: nama penerima dimasukkan ke log serah-terima | Uji privasi gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| Uji mutasi: izin "tandai siap" diubah menjadi `Update` | Uji matriks gagal; berkas dipulihkan | `PASS` | Keluaran `dotnet test` |
| Regresi Kamar Operasi (`OperatingRoomTests`) | 142 lulus | `PASS` | Keluaran `dotnet test` |
| Regresi Rekam Medis (`MedicalRecordTests`) | 225 lulus | `PASS` | Keluaran `dotnet test` |
| `PatientManagementTests`, `PharmacyTests` | Gagal kompilasi karena masalah lama yang tidak terkait Optik (`BayiUji` dan `hospitalSites`) | `PRE-EXISTING` | Keluaran build |

**Tidak dijalankan:** dua petugas yang mengubah pesanan yang sama secara bersamaan dalam satu uji E2E (konkurensi
sudah dibuktikan per task lewat token `Version`); PostgreSQL; penolakan `403` lewat permintaan HTTP sungguhan
(dibuktikan lewat atribut dan registri izin).

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Registri izin lengkap dan sesuai matriks | Terpenuhi | Uji matriks 44 endpoint |
| Alur penuh di harness | Terpenuhi | Uji E2E lewat controller |
| Log tanpa data klinis/pribadi | Terpenuhi untuk alur yang diuji | Uji privasi log |
| Atomisitas, idempotensi, konkurensi, fail-closed (`NFR`) | Terpenuhi per task | Laporan `BE-OPT-030`..`052` |

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status roadmap | **Seluruh roadmap backend Optik `MVP-0`..`MVP-4` selesai** di harness |
| Sebelum deploy | Di devYoga (`QuilvianNewDevYoga`) kedua migration Optik sudah diterapkan (`AddOpticalManagement` 2026-11-03, `AddEncounterReferralSource` 2026-11-04; riwayat 380, nol tertunda). Database lain (staging/produksi) wajib menerima `20261104020212_AddEncounterReferralSource` sebelum atau bersamaan dengan deploy kode ini |
| Masih perlu dikonfirmasi | Ringkasan keputusan pelaksanaan di laporan `BE-OPT-031`..`052` (lunas per tagihan, kode tindakan pemeriksaan, BPJS masuk Asuransi, penjualan bruto, resep remake, batas garansi inklusif) |
| Perlu diteruskan | Ke Billing (Yasmina): adjustment tidak mengubah status baris tagihan. Ke Medical Fee: aturan bagi hasil untuk domain `OPTICAL`. Ke Ikbal: `OPT-DEC-060` |
| Risiko tersisa | Belum dicoba di PostgreSQL |
| Perubahan sampingan | `NONE` |
| Status Git | Task ini hanya mengubah berkas uji (diabaikan Git) dan dokumen: `M` roadmap, traceability; `??` laporan ini |
| Langkah berikutnya | Roadmap frontend Optik, atau penerapan migration di database lokal/dev bila diizinkan |
