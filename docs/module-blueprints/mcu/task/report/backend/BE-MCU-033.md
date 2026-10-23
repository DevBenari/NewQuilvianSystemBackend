# Laporan Perubahan Backend — `BE-MCU-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-033` |
| Judul | Dokumen hasil dan ringkasan kelayakan |
| Slice | `MVP-3` — `EPIC MCU-06` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-033`* |
| Trace | `FR-MCU-050`; `MCU-DEC-010`; `MCU-FE-003`; `MCU-AC-012`; `AT-MCU-42` (bagian ringkasan) |
| Contract version | `MCU-API-v1` grup Mcu Delivery (`GET orders/{id}/result-document?variant=full|fitness-summary`, `McuResultDocumentResponse` / `McuFitnessSummaryDocumentResponse`); `MCU-PERM-v1` (`McuDelivery : Read`) — `approved` 2026-10-21 |
| Dependency | `BE-MCU-030` ✅ |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), memakai kontrak yang sudah ada (1), membaca persistence modul mitra (1), privasi data kesehatan (2), workflow terbatas (1) |
| Task mode | `BACKEND` |
| Target tulis | MCU: `McuDeliveryService.cs`, `McuDeliveryController.cs`, `McuDeliveryDtos.cs` (baru); `Program.cs` (satu registrasi). Laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `cee59643` (branch `yoga`) — bergeser dari `e86d98aa` sejak `BE-MCU-032`: commit pemilik `ab87bbd3` + merge `QuilvianIntegrationBackend`. Merge tidak mengubah berkas MCU/Lab/Rad; perubahan Billing (`BillingRefundService`, `BillingInvoiceService`, dll.) terverifikasi tidak merusak alur MCU lewat suite regresi |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-42` bagian ringkasan terbukti (refleksi DTO + skenario Siti) |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE`; membaca saja dari `Lab`, `Rad` |
| Keberlakuan | `NEW CODE` (service, controller, DTO baru; tanpa entity baru) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuDelivery : Read`), `QBE-LOG-001` (tanpa log isi), `QBE-VAL-001`, `QBE-DTO-001` |
| Checker QBE | `PASS` — 3 berkas task nol temuan; kontrol negatif `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus. Pemanggilan pertama (sendirian, sesudah `dotnet test`) tidak menangkap kontrol; dua pemanggilan berikutnya menangkapnya. Sebab belum diketahui; laporan `030`–`032` dikoreksi |

---

## 1. Masalah yang diperbaiki

Sesudah resume disetujui, belum ada data untuk mencetak hasil MCU. Bila layar menyusun ringkasan untuk perusahaan dari
data lengkap, satu kesalahan tampilan cukup untuk membocorkan diagnosis peserta ke perusahaannya.

**Contoh.** Siti, karyawan PT Contoh, hasilnya *Unfit sementara* dengan temuan *Hipertensi grade 1, HBsAg reaktif*.
- `variant=fitness-summary` → hanya *Unfit sementara* dan *Disarankan tidak bertugas di bagian pengolahan makanan sampai
  pemeriksaan ulang*, nama peserta, perusahaan, nomor order, dan dokter pemberi approve. Kata *HBsAg*, nilai Lab, laporan
  Thorax, dan nomor rekam medis **tidak ada** di respons.
- `variant=full` → resume lengkap, hasil Darah Lengkap 18,5 g/dL (rujukan 12 – 16, ditandai di luar rujukan), laporan
  Thorax yang dirilis, isian Audiometri 35 dB.

---

## 2. Proses bisnis

1. Petugas MCU membuka **Cetak** pada order dan memilih *hasil lengkap* (peserta) atau *ringkasan kelayakan* (perusahaan).
2. Syarat: order tidak dibatalkan dan resume berlaku sudah *Disetujui Dokter* atau *Terverifikasi*. Respons membawa
   `IsVerified` — hanya versi terverifikasi yang boleh diserahkan (`BE-MCU-034`).
3. **Ringkasan** dibaca server dari dua kolom resume saja: kategori kelayakan dan rekomendasi kerja.
4. **Hasil lengkap** berisi identitas peserta, resume, dan setiap komponen berurutan:
   - Laboratorium — hanya pemeriksaan yang **sudah dirilis** Lab: nilai, satuan, rentang normal, tanda di luar rujukan;
   - Radiologi — versi laporan terakhir yang **dirilis**: temuan, kesan, rekomendasi, nomor laporan;
   - Pemeriksaan MCU — nilai per isian dan kesimpulan;
   - komponen dibatalkan tercetak beserta alasannya, tanpa hasil.
5. Tata cetak final mengikuti brief layar; task ini menyediakan data saja.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Resume belum disetujui (draf, belum ada, atau baru dibuka ulang) | `409` *"Resume belum disetujui dokter."* |
| Order dibatalkan | `409` *"Order sudah dibatalkan."* |
| `variant` kosong atau tidak dikenal | `400` *"Pilih variant full atau fitness-summary."* |
| Order tidak dikenal | `404` |
| Hasil Lab belum dirilis | Tidak tercetak (baris kosong) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-033`), `contracts/api-contract.md` (grup Mcu Delivery), `permission-audit-matrix.md`,
`testing/acceptance-test-matrix.md` (`AT-MCU-42`), `00-interview-decisions.md` (`MCU-DEC-010`, `MCU-FE-003`),
`03-frontend-architecture.md`, `05-screen-brief.md` (cetak), `McuOrder`, `McuResume`, `MstPatient`, `LabExamination`
(ruas hasil, `ReleasedAt`), `LabValueBound`, `LabValueOption`, `RadReport`, `RadReportVersion`, `MedicalCheckupPermissions`,
diff merge `ab87bbd3..cee59643` area MCU/Lab/Rad/Billing.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuDeliveryService.cs` (baru) | `GetFitnessSummaryAsync`, `GetFullDocumentAsync`; penjaga resume disetujui; baris hasil Lab dirilis, laporan Rad dirilis, isian Pemeriksaan MCU |
| `MedicalCheckupManagement/Controllers/McuDeliveryController.cs` (baru) | `GET result-document?variant=`, izin `McuDelivery : Read` |
| `MedicalCheckupManagement/DTOs/McuDeliveryDtos.cs` (baru) | `McuResultDocumentResponse` (+ komponen, baris), `McuFitnessSummaryDocumentResponse` (daftar ruas dikunci uji) |
| `Program.cs` | `AddScoped<McuDeliveryService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai `MCU-API-v1`. Swagger mencantumkan bentuk `full` sebagai respons `200`; bentuk ringkasan tertulis di komentar endpoint |
| Database | Tanpa migration; baca saja. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Privasi | Ringkasan tidak punya ruas nilai, temuan, kondisi umum, rekomendasi medis, kesimpulan, komponen, nomor rekam medis, maupun tanggal lahir — dikunci uji refleksi. Siapa penerima tiap varian (persetujuan pelepasan, `VAL-MCU-31`) ditegakkan saat penyerahan (`BE-MCU-034`), bukan di endpoint baca ini. Tidak ada log aplikasi (GET) |

### 3.4 Delta terhadap kontrak dan roadmap

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Bentuk DTO | Hanya nama | Ruas seperti §3.2 | Kontrak tidak merinci; ringkasan mengikuti `MCU-DEC-010` |
| Syarat status | Tidak disebut | Resume berlaku *Disetujui Dokter*/*Terverifikasi*; `IsVerified` di respons | Sebelum approve, kategori kelayakan belum pasti; petugas perlu pratinjau sebelum verifikasi |
| Versi yang dicetak | — | Versi berlaku saja | Mencetak versi *Dikoreksi* belum diminta |
| Reuse *hasil Lab/Rad yang dirilis* | — | Dibaca langsung (baca saja) dari `LabExamination` dan `RadReportVersion` berstatus dirilis | Service Lab/Rad tidak punya pembaca hasil per pesanan yang cocok tanpa dependensi berat; tidak ada penulisan ke modul mitra |
| Lab mikrobiologi/PA | — | Tidak tercetak nilainya (baris tanpa nilai) | Bentuk hasil berbeda; menunggu brief layar |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Delivery

Base URL: `/api/v1/health-services/medical-checkup-management/orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/result-document?variant=full` | Data cetak hasil lengkap | `McuDelivery : Read` | `variant` | `McuResultDocumentResponse` |
| `GET` | `/result-document?variant=fitness-summary` | Ringkasan kelayakan untuk perusahaan | `McuDelivery : Read` | `variant` | `McuFitnessSummaryDocumentResponse` |

Kode status: `200`, `400`, `404`, `409`.

---

## 5. Verifikasi

Harness uji **sementara** dipulihkan dari salinan `BE-MCU-032` (SQLite, service produksi MCU/Lab/Rad/Billing/Registrasi),
dijalankan di HEAD `cee59643`. Hasil Lab dirilis dan laporan Thorax dirilis disemai langsung (alur validasi/rilis milik
Lab/Rad); Audiometri diisi lewat `SaveResultAsync`. Harness **dihapus** sesudahnya.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 41 lulus (6 dokumen + 35 regresi `024`, `030`–`032`) di HEAD sesudah merge | `PASS` | `Passed! - Failed: 0, Passed: 41` |
| `AT-MCU-42` refleksi | Ruas ringkasan sama persis dengan daftar putih 12 ruas; tidak ada nama ruas mengandung *Value/Result/Finding/Diagnos/Condition/Conclusion/Component/Line/Impression/MedicalRecord/Birth*; semua ruas bernilai sederhana | `PASS` | `AT_MCU_42_DTO_ringkasan_…` |
| `AT-MCU-42` Siti | Ringkasan: *Unfit sementara* + rekomendasi kerja, PT Contoh, dr. Rina; JSON tidak memuat *HBsAg*, *Hipertensi*, *Tekanan darah*, *penyakit dalam*, *18,5*, *35*, *Thorax normal*, nomor RM | `PASS` | `AT_MCU_42_ringkasan_Siti_…` |
| Hasil lengkap | Identitas + RM; temuan *HBsAg reaktif*; terverifikasi oleh Dewi; urutan 4 komponen; Darah Lengkap `18,5 g/dL`, rujukan `12 – 16`, di luar rujukan; Gula Darah Puasa belum dirilis → kosong; Thorax versi dirilis terakhir (bukan versi lama *Superseded*); Audiometri `35 dB` | `PASS` | `Hasil_lengkap_…` |
| Komponen dibatalkan | Thorax *Dibatalkan* tercetak dengan alasan *Kehamilan*, tanpa laporan | `PASS` | `Komponen_dibatalkan_…` |
| Penjaga | Draf → `409` kedua varian; order tidak dikenal `404`; `variant=semua` → `400`; izin `McuDelivery : Read`, template `result-document` | `PASS` | `Dokumen_ditolak_…`, `Endpoint_result_document_…` |
| Uji mutasi | Ringkasan diisi temuan penting, filter *dirilis* Lab dilepas, penjaga resume disetujui dilepas, ruas `ImportantFindings` ditambahkan ke DTO ringkasan → 4 uji **gagal**; dipulihkan identik (`cmp`), kembali 41/41 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (3 berkas + kontrol) | Nol temuan; kontrol terkena 3 aturan (pemanggilan kedua dan ketiga) | `PASS` | Keluaran checker |
| PostgreSQL, `403` lewat HTTP, data devYoga | Tidak dijalankan (devYoga kini tanpa migration tertunda menurut catatan sesi lain; uji berjalan belum diminta) | `NOT RUN` | — |

Uji manual: `NOT FEASIBLE` di sesi ini.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-42` bagian ringkasan — refleksi DTO ringkasan: nol ruas nilai/diagnosis | Terpenuhi | §5 |
| Contoh Siti — ringkasan hanya *Unfit sementara* + rekomendasi kerja; *HBsAg reaktif* tidak ada | Terpenuhi | §5 |
| `AT-MCU-42` bagian penyerahan (`409` tanpa persetujuan) | Di luar task (`BE-MCU-034`) | — |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Commit backend bergeser (`cee59643`); merge Billing tidak merusak alur MCU menurut suite regresi. Checker QBE meleset pada pemanggilan pertama sesudah `dotnet test` — kini terjadi juga saat berjalan sendirian, sehingga klaim *karena paralel* di laporan `032` dan memori dikoreksi |
| Masalah yang diketahui | Hasil Lab mikrobiologi/PA tidak tercetak nilainya; cetak versi resume *Dikoreksi* belum tersedia |
| Risiko tersisa | Tata cetak final menunggu brief layar. Siapa yang boleh menerima varian lengkap ditegakkan di `BE-MCU-034` |
| Perubahan sampingan | Folder uji sementara dipulihkan lalu dihapus (salinan diperbarui di scratchpad); kontrol negatif QBE dibuat lalu dihapus; laporan `BE-MCU-030`–`032` dikoreksi pada butir checker |
| Interupsi | `NONE` |
| Status Git | ` M` `Program.cs`, laporan `BE-MCU-030`..`032` (koreksi); `??` tiga berkas `McuDelivery*`; laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-034` (penyerahan, persetujuan pelepasan, pemberitahuan koreksi) — dependency `031` ✅, `033` ✅ |
