# Laporan Perubahan Backend — `BE-MCU-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-021` |
| Judul | Mulai pemeriksaan |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-021`* |
| Trace | `FR-MCU-030`, `FR-MCU-031`; `MCU-DEC-004`, `024`, `032` |
| Contract version | `MCU-API-v1` grup Mcu Examination (`POST start-examination`); `MCU-VAL-v1` `VAL-MCU-34`, `46`; `MCU-STATE-v1` (*Hadir → Dalam Pemeriksaan*; komponen *Pending → Ordered/ReadyToFill*); `MCU-PERM-v1` (`McuExamination : Start`); `MCU-INT-v1` `INT-MCU-04`..`06` — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-013` ✅, `BE-MCU-020` ✅ |
| Klasifikasi | `MEDIUM` — skor 9: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), endpoint baru (2), lintas modul Lab/Rad/Billing (2), transaksi + konkurensi (1), izin baru (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `McuExaminationService.cs`, `McuExaminationController.cs`, `McuExaminationDtos.cs` (baru); `McuOperationResult.cs` (kode `VAL-MCU-34`), `Program.cs` (registrasi DI); laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-26`, `27`, `28` terbukti di harness dengan service Lab/Rad dan produsen fakta klinis sungguhan |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (Lab/Rad hanya dipanggil lewat jalur internal `BE-MCU-020`; tidak ada perubahan modul mitra) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuExamination : Start`), `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001` (pesanan Lab/Rad + komponen + order dalam satu transaksi), `QBE-CODE-001`..`006`, `QBE-DTO-001` |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Sesudah check-in, order MCU berhenti di *Hadir*: belum ada cara menegakkan *bayar dulu* (perorangan) atau
*terjamin* (perusahaan), dan pesanan Lab/Rad paket belum pernah lahir.

**Contoh.** Budi (tunai) check-in, belum membayar, petugas menekan *Mulai pemeriksaan* → `409`
*"Tagihan paket belum lunas (sisa Rp 350.000)."*, nol pesanan — petugas Lab dan Radiologi tidak melihat
apa pun. Budi membayar Rp 350.000 → tekan lagi → satu pesanan Lab (Darah Lengkap + Gula Darah Puasa) dan
satu pesanan Rad (Thorax PA), keduanya bertanda paket atas nama dr. Rina; Pemeriksaan Fisik siap diisi;
order *Dalam Pemeriksaan*, dasar *Lunas*. Siti (PT Contoh) langsung dapat dimulai tanpa bayar — dasar
*Terjamin*.

---

## 2. Proses bisnis

1. Petugas MCU membuka order *Hadir* lalu menekan **Mulai pemeriksaan** (membawa `version` yang dibaca).
2. Sistem membaca status bayar kunjungan MCU lewat `LabPaymentClearanceRules` (`INT-MCU-04`): penjamin
   perusahaan → *Terjamin*; tunai → harus *Lunas*.
3. Satu transaksi: pesanan Lab per disiplin dan pesanan Rad per komponen dibuat lewat jalur bertanda paket
   (`BE-MCU-020`) dengan dokter MCU yang ditetapkan saat check-in; komponen Lab/Rad → *Ordered* beserta
   tautan pesanannya; komponen *Pemeriksaan MCU* → *ReadyToFill*; order → *Dalam Pemeriksaan* dengan
   `ClearanceBasis`, `ExaminationStartedAt`, riwayat *Hadir → Dalam Pemeriksaan*.
4. Komponen yang dijadwal ulang saat check-in tetap *Rescheduled* dan tidak dipesan.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Tunai belum lunas | `409` `VAL-MCU-34` *"Tagihan paket belum lunas (sisa Rp 350.000)."*; nol pesanan, order tidak berubah |
| Tagihan paket belum terbit | `409` `VAL-MCU-34` *"Tagihan paket belum terbit. Hubungi kasir."* |
| Order *Terjadwal* | `409` *"Peserta belum check-in."* (`MCU-STATE-v1` 1.2) |
| Order *Tidak Hadir*/*Dibatalkan*/status lain | `409` dengan nama statusnya |
| `version` berbeda, atau petugas lain menyimpan order tepat sebelum simpan | `409` `VAL-MCU-46`; seluruh pesanan yang sempat dibuat ikut batal |
| Lab atau Rad menolak pesanan (mis. modalitas Thorax nonaktif) | `409` `MCU-PARTNER-REJECTED` menyebut komponennya; pesanan Lab yang sudah dibuat ikut batal, order tetap *Hadir* |
| Kirim ulang sesudah dimulai | Hasil yang sama; tanpa pesanan kedua |
| Pengguna tanpa `McuExamination : Start` | `403` (atribut) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-021`), `contracts/api-contract.md` (grup Mcu Examination),
`validation-matrix.md` (`VAL-MCU-34`, `46`), `state-transition-matrix.md` (bagian 1 dan 2),
`permission-audit-matrix.md`, `integration-contract.md` (`INT-MCU-04`..`06`), `testing/acceptance-test-matrix.md`,
`00-interview-decisions.md` (`MCU-DEC-004`, `024`, `032`, `MCU-COORD-001`), `LabPaymentClearanceRules.cs`,
`LabOrderService.cs`, `RadOrderService.cs`, `McuCheckInService.cs`, `McuCheckInController.cs`,
`McuOrderQueryService.cs`, `McuOrderConfiguration.cs` (token `Version`), `PermissionRegistryValidator.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuExaminationService.cs` (baru) | `StartAsync`: kirim ulang aman, `VAL-MCU-46`, status, gerbang `VAL-MCU-34` lewat `LabPaymentClearanceRules`, pesanan Lab/Rad bertanda, perpindahan komponen dan order, riwayat — satu transaksi; konkurensi dan penolakan mitra di-*rollback* utuh |
| `MedicalCheckupManagement/Controllers/McuExaminationController.cs` (baru) | `POST orders/{id}/start-examination` (`McuExamination : Start`); resource `McuExamination`, tag *Mcu Examination* |
| `MedicalCheckupManagement/DTOs/McuExaminationDtos.cs` (baru) | `StartMcuExaminationRequest` (`version`) |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-34` |
| `Program.cs` | `AddScoped<McuExaminationService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST orders/{id}/start-examination` baru, sesuai kontrak; endpoint lain tidak berubah |
| Database | Tanpa migration (kolom `ClearanceBasis`, `ExaminationStartedAt`, tautan komponen sudah ada sejak `BE-MCU-002`). **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Resource baru `McuExamination` muncul di Akses Role lewat `[AccessController]`; `PermissionRegistryValidator` nol temuan untuk MCU. Hak akses diberikan lewat `SysAccessPolicies` — belum ada yang memegangnya |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Tagihan belum terbit | Hanya pesan *belum lunas (sisa …)* | `NotBilled` → *"Tagihan paket belum terbit. Hubungi kasir."* dengan kode `VAL-MCU-34` | *Sisa Rp 0* menyesatkan; kasus ini hanya mungkin bila baris paket check-in hilang |
| Kirim ulang | Tidak disebut | Order yang sudah dimulai mengembalikan hasil yang sama, sebelum pemeriksaan versi | Pola `VAL-MCU-30` check-in; tombol yang ditekan dua kali tidak membuat pesanan kedua |
| Komponen di luar paket | `MCU-DEC-036` ditagih biasa | Tidak ditangani di sini | Komponen di luar paket baru dapat ditambah sesudah order berjalan (`BE-MCU-022`); saat mulai, semua komponen adalah komponen paket |
| Indikasi klinis pesanan Rad | Tidak disebut | *"Medical Check-Up {paket} ({nomor order})"* | Petugas Radiologi melihat asal pesanan |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Examination

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/orders/{id}/start-examination` | Cek lunas/terjamin, buat pesanan Lab/Rad bertanda paket | `McuExamination : Start` | `StartMcuExaminationRequest` (`version`) | `McuOrderDetailResponse` (status *Dalam Pemeriksaan*, `clearanceBasis`, `examinationStartedAt`, komponen dengan nomor pesanan Lab/Rad) | Tersedia |

Kode status: `200`, `403`, `404`, `409`. Log aplikasi: nomor order, dasar (*Lunas*/*Terjamin*), jumlah pesanan
Lab dan Rad, dokter MCU — tanpa NIK.

---

## 5. Verifikasi

Harness uji **sementara** dibuat ulang (`Tests/QuilvianSystemBackend.McuTests`: SQLite; `LabOrderService`,
`RadOrderService`, `LabSpecimenService`, `RadStudyService`, `ClinicalMilestoneFactProducer` produksi; status
bayar dibaca dari baris `BilInvoice`/`BilInvoiceItem`/`BilSettlement`/`BilTender` yang di-*seed* seperti hasil
check-in `BE-MCU-013` dan pembayaran kasir), dijalankan, lalu **dihapus** sesuai instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 9 lulus | `PASS` | `Passed! - Failed: 0, Passed: 9` |
| `AT-MCU-26` | Belum lunas → `409` `VAL-MCU-34` pesan persis, nol pesanan, order *Hadir* versi tetap. Sesudah Rp 350.000 dibayar → *Dalam Pemeriksaan*, *Lunas*; 1 pesanan Lab (2 pemeriksaan) + 1 pesanan Rad, bertanda order MCU, `InstructingDoctorId` = dr. Rina, `NotRequired`; komponen Lab/Rad *Ordered* bertaut, Pemeriksaan Fisik *ReadyToFill*, susulan tetap *Rescheduled*; versi +1; 1 riwayat; detail memuat nomor pesanan | `PASS` | `AT_MCU_26_…` |
| Tagihan belum terbit | `409` `VAL-MCU-34` *"Tagihan paket belum terbit. Hubungi kasir."* | `PASS` | `Tagihan_yang_belum_terbit_…` |
| `AT-MCU-27` | Penjamin perusahaan tanpa pembayaran → berhasil, *Terjamin*, pesanan bertanda | `PASS` | `AT_MCU_27_…` |
| `AT-MCU-28` ujung ke ujung | Sesudah mulai: spesimen pesanan paket dinyatakan layak dan mutu Thorax diterima → handoff kosong, **nol** fakta tagih; tagihan tetap satu baris Rp 350.000 dan tetap sama dengan yang dibayar | `PASS` | `AT_MCU_28_…` |
| Status, versi, tidak ada | *Terjadwal* → *"Peserta belum check-in."*; versi lama → `VAL-MCU-46`; order tak dikenal → `404` | `PASS` | `Order_terjadwal_…` |
| Kirim ulang | Hasil sama (`examinationStartedAt` sama); tetap 1 pesanan Lab, 1 Rad, 1 riwayat | `PASS` | `Kirim_ulang_…` |
| Rad menolak di tengah | Modalitas Thorax nonaktif → `409` `MCU-PARTNER-REJECTED` menyebut Thorax; **nol** pesanan Lab dan prosedur pesanan (sudah dibuat lalu batal), nol pesanan Rad, order *Hadir* versi tetap, nol riwayat, tidak ada komponen berpindah | `PASS` | `Radiologi_menolak_…` |
| Balapan versi | Petugas lain menaikkan versi tepat sebelum simpan → `409` `VAL-MCU-46`; nol pesanan Lab/Rad, order *Hadir* | `PASS` | `Petugas_lain_mengubah_order_…` |
| Endpoint | `POST start-examination` = `McuExamination : Start`, route grup `orders/{id}`; `PermissionRegistryValidator` nol temuan MCU | `PASS` | `Endpoint_start_examination_…` |
| Uji mutasi | Gerbang `VAL-MCU-34` dilepas sementara → 2 uji gerbang **gagal**; dipulihkan identik (`cmp`), kembali 9/9 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (5 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Pembayaran lewat kasir Billing sungguhan; kunci di PostgreSQL; `403` lewat HTTP; Swagger | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-26` — belum lunas `409` tanpa pesanan; sesudah lunas pesanan Lab per disiplin + Rad per komponen, bertanda, dokter MCU | Terpenuhi | §5 |
| `AT-MCU-27` — perusahaan terjamin dimulai tanpa pembayaran | Terpenuhi | §5 |
| `AT-MCU-28` ujung ke ujung — nol fakta tagih Lab/Rad, tagihan tetap satu baris paket, kunjungan tetap lunas | Terpenuhi | §5 |
| Contoh roadmap (Budi, dr. Rina, Rp 350.000) | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Peserta perusahaan selalu *Terjamin* menurut `LabPaymentClearanceRules`; status surat jaminan (`NeedGuaranteeLetter`) tidak dibaca — sisa pertanyaan `MCU-COORD-001`/`MCU-UNK-02` yang masih `draft` |
| Masalah yang diketahui | Persetujuan tertulis pemilik Billing/Registrasi atas `BE-MCU-013` masih belum tercatat (bawaan task sebelumnya) |
| Risiko tersisa | Pembayaran di-*seed* sebagai baris Billing, bukan lewat alur kasir; kunci nomor Lab/Rad dan balapan hanya terbukti di SQLite; startup backend ke devYoga masih menunggu migration MCU; resource `McuExamination` belum diberikan ke jabatan mana pun |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus |
| Interupsi | Tidak ada |
| Status Git | Perubahan task: 3 berkas baru di folder modul MCU (`??`), `McuOperationResult.cs`, `Program.cs` (`M`); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-022` (komponen: daftar, sinkron status, tambah di luar paket, jadwal ulang) kini siap |
