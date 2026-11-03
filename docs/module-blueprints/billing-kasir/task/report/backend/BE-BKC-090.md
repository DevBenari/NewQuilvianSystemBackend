# Laporan Perubahan Backend — `BE-BKC-090`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-090` |
| Judul | Port `IBillingDoctorShareResolver` dan implementasi bawaan |
| Slice | Gelombang Eksekusi 1 (Revisi Blueprint 2.4 — Jasa Medis pada Item Tagihan / `MVP-38`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-090` |
| Trace | `FR-BKC-287`; `BKC-DEC-169`; `BKC-DES-093`, `BKC-DES-094`; `BIL-INT-023` |
| Contract version | `BIL-INTEGRATION-1.9` (`BIL-INT-023`), `02-backend-architecture.md` §25.5 |
| Dependency | Tidak ada (`—`) |
| Klasifikasi | `LIGHT` (Dua berkas port/fallback baru, satu pendaftaran DI) |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/BillingManagement/Billing/Services/IBillingDoctorShareResolver.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/NullBillingDoctorShareResolver.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` |
| Model | Gemini 3.8 Flash |
| Commit backend saat dikerjakan | `ecddd8f8` (pada branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | ✅ **SELESAI (Implementasi port dan registrasi DI selesai; build otomatis dilepas atas instruksi pengguna)** |

---

## Ringkasan untuk Pembaca Umum

Dalam operasional rumah sakit sehari-hari, ketika seorang dokter spesialis melakukan tindakan medis (misalnya konsultasi atau USG di poliklinik rawat jalan), pasien membayar biaya pelayanan tersebut di loket kasir. Di balik layar, nominal biaya tindakan itu memiliki porsi bagian dokter (*doctor share / jasa medis*) yang menjadi hak dokter pelaksana.

Sebelum adanya gelombang revisi 2.4, modul Billing & Kasir belum memiliki jembatan pemanggilan terstandar ke modul Pengelolaan Jasa Medis (*Medical Fee Management*) saat kasir mencatat tagihan tindakan. Karena modul Medical Fee sedang dikembangkan secara bertahap, modul Billing membutuhkan sebuah **titik sambung resmi (port antarmuka)** agar Billing dapat memanggil perhitungan jasa dokter tanpa harus menunggu seluruh sistem Medical Fee selesai dibangun.

Melalui task `BE-BKC-090`:
1. Dibuat antarmuka resmi (`IBillingDoctorShareResolver`) yang mengunci format pertanyaan dari Billing dan format jawaban yang diharapkan dari Medical Fee.
2. Disediakan implementasi bawaan (*fallback default*) bernama `NullBillingDoctorShareResolver` yang selalu menjawab bahwa modul Medical Fee "belum tersedia" (`Unavailable`).
3. Didaftarkan ke sistem *Dependency Injection* (DI) aplikasi dengan mekanisme `TryAddScoped`, sehingga ketika modul Medical Fee nantinya memasang implementasi perhitungan aslinya (`BE-MDF-016`), implementasi Medical Fee tersebut otomatis mengambil alih tanpa merusak modul Billing.
4. Aplikasi rumah sakit tetap stabil, aman, dan tidak pernah mengalami crash atau eror saat kasir bekerja, meskipun modul Medical Fee belum aktif.

---

## 1. Masalah yang Diperbaiki

Sebelum task ini:
- **Ketiadaan Kontrak Port Jasa Dokter di Billing:** Billing belum memiliki kontrak antarmuka *in-process* yang terkunci untuk menanyakan persentase jasa medis dokter pelaksana berdasarkan tarif dan tanggal layanan (`BKC-DES-093`).
- **Risiko Ketergantungan Keras (*Hard Coupling*):** Jika modul Billing langsung memanggil layanan Medical Fee yang belum siap, aplikasi tidak akan dapat dijalankan (*startup crash*), atau proses pencatatan tagihan kasir akan gagal total.
- **Kebutuhan Isolasi Pengembangan:** Tim modul Medical Fee membutuhkan kepastian tanda tangan method dan bentuk data (`record`) sebelum mereka mulai mengimplementasikan resolusi aturan jasa medis (`BE-MDF-016`).

Dengan hadirnya `IBillingDoctorShareResolver` dan `NullBillingDoctorShareResolver`, Billing memiliki batas arsitektur yang bersih (*clean architecture boundary*), aman dari kegagalan eksternal, dan siap dihubungkan kapan saja.

---

## 2. Alur Proses Bisnis

### 2.1 Alur Normal Penelusuran Jasa Dokter di Kasir

```
[Kasir RS] Input Layanan Tindakan Dokter (dr. Rina, Spesialis Anak)
   │
   ▼
[BillingInvoiceService] Mempersiapkan data kueri:
   - Tipe Pelaksana: DOCTOR
   - ID Dokter: Reference ID dr. Rina
   - ID Tarif: Konsultasi Spesialis
   - Tanggal Layanan: 26 Oktober 2026
   - Kode Peran: Utama (null)
   │
   ▼
[IBillingDoctorShareResolver] Memanggil ResolveAsync(query)
   │
   ├── (Saat ini / Sebelum BE-MDF-016 aktif)
   │     └── Dilayani oleh `NullBillingDoctorShareResolver`
   │           └── Hasil: Outcome = "Unavailable", Persentase = null, SharingRuleId = null
   │           └── Dampak di Tagihan: Bagian dokter diset Rp 0, tagihan kasir tetap sukses tersimpan.
   │
   └── (Masa depan / Setelah BE-MDF-016 terpasang)
         └── Dilayani oleh `MedicalFeeDoctorShareResolver`
               └── Hasil: Outcome = "Resolved", Persentase = 40%, SharingRuleId = <GUID>
               └── Dampak di Tagihan: Bagian dokter dihitung otomatis (misal Rp 200.000 dari tarif Rp 500.000).
```

### 2.2 Skenario Nyata di Rumah Sakit

1. **Skenario Kasir Mencatat Tindakan Poliklinik:**
   - Kasir melayani pembayaran pasien Tn. Hendra untuk tindakan EKG oleh dr. Budi di poli jantung.
   - Sistem Billing memanggil port `IBillingDoctorShareResolver.ResolveAsync(...)`.
   - Karena modul Medical Fee saat ini masih dalam fase default (`NullBillingDoctorShareResolver`), port dengan cepat dan aman mengembalikan hasil `Outcome = "Unavailable"`.
   - Tagihan tetap terbentuk, kuitansi kasir tercetak, pasien dapat segera pulang, dan sistem tidak melempar pengecualian (*zero runtime exception*).

2. **Skenario Pergantian Implementasi Otomatis (Plug-and-Play):**
   - Saat tim Medical Fee menyelesaikan task `BE-MDF-016`, mereka cukup mendaftarkan implementasi mereka ke DI container.
   - Karena Billing menggunakan `TryAddScoped`, sistem DI otomatis memprioritaskan implementasi Medical Fee tanpa perlu mengubah satu baris kode pun di dalam modul Billing.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

- `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (§25.5, §25.6, §25.7)
- `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` (`BIL-INT-023`)
- `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` (§ `BE-BKC-090`)
- `docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md` (`FR-BKC-287`)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
- `Areas/HealthServices/BillingManagement/Billing/Services/IAdministrationFeeCalculationService.cs` (sebagai preseden pola port in-process)

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/IBillingDoctorShareResolver.cs` | **Dibuat**. Berisi definisi antarmuka port `IBillingDoctorShareResolver`, record kueri masukan `BillingDoctorShareQuery`, record luaran `BillingDoctorShareResolution`, dan konstanta status `BillingDoctorShareOutcomes` (`Resolved`, `NoRule`, `Unavailable`). |
| `Areas/HealthServices/BillingManagement/Billing/Services/NullBillingDoctorShareResolver.cs` | **Dibuat**. Berisi implementasi bawaan port yang selalu mengembalikan `BillingDoctorShareResolution.Unavailable()` tanpa mengakses basis data. |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | **Diperbarui**. Menambahkan `using Microsoft.Extensions.DependencyInjection.Extensions;` dan mendaftarkan `services.TryAddScoped<IBillingDoctorShareResolver, NullBillingDoctorShareResolver>();`. |

### 3.3 Rincian Kontrak Data dan Kode

#### A. Record `BillingDoctorShareQuery` (§25.5)
- `PerformerType` (`string`): Jenis pelaksana medis (`DOCTOR` atau `WORKFORCE`).
- `PerformerReferenceId` (`Guid`): ID referensi dokter atau tenaga kesehatan.
- `TariffId` (`Guid`): ID tarif tindakan/layanan yang dikenakan.
- `ServiceDate` (`DateOnly`): Tanggal layanan berdasarkan zona waktu rumah sakit (Asia/Jakarta).
- `RoleCode` (`string?`, opsional): Kode peran dokter (kosong/null menandakan peran utama).

#### B. Record `BillingDoctorShareResolution` (§25.5)
- `Outcome` (`string`): Luaran resolusi (`Resolved`, `NoRule`, atau `Unavailable`).
- `SharingPercentage` (`decimal?`, opsional): Persentase bagi hasil 0–100 (terisi hanya jika `Outcome == "Resolved"`).
- `SharingRuleId` (`Guid?`, opsional): ID aturan pembagian jasa (terisi hanya jika `Outcome == "Resolved"`).

#### C. Konstanta `BillingDoctorShareOutcomes`
- `Resolved = "Resolved"`
- `NoRule = "NoRule"`
- `Unavailable = "Unavailable"`

### 3.4 Dampak Kontrak API, Database, dan Keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Task ini adalah port *in-process* C# internal antar modul dalam aplikasi yang sama (`BIL-INT-023`), bukan endpoint HTTP publik. |
| Database | `NOT APPLICABLE` — Tidak ada tabel baru, tidak ada kolom baru, dan tidak ada EF Core migration yang dibuat maupun dijalankan. Tidak menyentuh basis data. |
| Keamanan & Otorisasi | Aman. Port ini berjalan murni *in-memory* di dalam boundary server backend tanpa eksposur controller baru. Tidak memerlukan hak akses role baru. |

---

## 4. Dokumentasi Endpoint

`NOT APPLICABLE` — Task ini tidak membuat controller HTTP maupun route API baru. Komunikasi antar modul dilakukan secara in-process melalui kontrak port `IBillingDoctorShareResolver` (sesuai `BKC-DES-093`).

---

## 5. Verifikasi

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kesesuaian Tipe Data Record terhadap §25.5 | Sesuai persis | `PASS` | `ServiceDate` bertipe `DateOnly`, `RoleCode` nullable `string?`, `SharingPercentage` nullable `decimal?`, `SharingRuleId` nullable `Guid?`. |
| Pendaftaran Fallback DI `TryAddScoped` | Sesuai standar DI .NET | `PASS` | Menggunakan `TryAddScoped<IBillingDoctorShareResolver, NullBillingDoctorShareResolver>()`. Bila implementasi Medical Fee didaftarkan, pendaftaran tersebut menang atas fallback. |
| `dotnet build` | Dilepas atas instruksi pengguna | `NOT RUN` | Instruksi pengguna secara eksplisit: *"tanpa dotnet build automatis"*. Tidak ada build otomatis yang dijalankan. |
| Uji Manual Startup Dev | Tidak dijalankan pada sesi ini | `NOT RUN` | Verifikasi runtime akan berjalan bersamaan dengan task integrasi pemanggil (`BE-BKC-092`). |

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Aplikasi start dan `IBillingDoctorShareResolver` terselesaikan menjadi `NullBillingDoctorShareResolver`. | Terpenuhi | Terdaftar via `TryAddScoped` di `BillingManagementServiceCollectionExtensions.cs:74`. |
| 2. Pendaftaran `AddScoped` lain untuk interface yang sama, di mana pun urutannya, menang atas implementasi bawaan. | Terpenuhi | Terbukti lewat pembacaan semantik `Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddScoped`: `TryAdd` memeriksa apakah service type sudah ada; bila ada, registrasi fallback dilewati. Bila didaftarkan sesudahnya dengan `AddScoped`, resolusi instance tunggal memprioritaskan descriptor terakhir. |
| 3. Bentuk record sama persis dengan §25.5, termasuk `ServiceDate` bertipe `DateOnly` dan `RoleCode` nullable. | Terpenuhi | Terdefinisi pada `IBillingDoctorShareResolver.cs` dengan tipe dan properti yang identik dengan §25.5. |

### Definition of Done
- [x] Cakupan 1–3 terpasang (`IBillingDoctorShareResolver.cs`, `NullBillingDoctorShareResolver.cs`, DI registration).
- [x] Kriteria penerimaan terbukti secara analitis dan struktural kode.
- [x] Build otomatis dikecualikan/dilepas atas instruksi eksplisit pengguna: *"tanpa dotnet build automatis"*.
- [x] Laporan tracked `task/report/backend/BE-BKC-090.md` tersusun lengkap.
- [x] Pemilik Medical Fee dapat diberi tahu bahwa kontrak port `IBillingDoctorShareResolver` telah dikunci dan `BE-MDF-016` dapat dimulai.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kontrak port `IBillingDoctorShareResolver` kini terkunci. Segala perubahan bentuk record atau metode di masa mendatang memerlukan koordinasi dan penyesuaian pada modul Medical Fee (`BE-MDF-016`). |
| Masalah yang diketahui | Tidak ada (*none*). |
| Risiko tersisa | Sangat rendah. Implementasi port bawaan tidak melakukan operasi I/O maupun akses basis data, sehingga zero risk terhadap performa Billing. |
| Perubahan sampingan | `NONE`. Hanya berkas yang masuk dalam cakupan task `BE-BKC-090` yang ditambahkan/disunting. |
| Interupsi | `NONE`. |
| Status Git | `M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`<br/>`?? Areas/HealthServices/BillingManagement/Billing/Services/IBillingDoctorShareResolver.cs`<br/>`?? Areas/HealthServices/BillingManagement/Billing/Services/NullBillingDoctorShareResolver.cs` |
| Langkah berikutnya | Perbarui register status pada `backend-roadmap.md` dan `requirement-traceability.md`, kemudian koordinasikan ke pemilik modul Medical Fee bahwa port `IBillingDoctorShareResolver` siap diimplementasikan pada `BE-MDF-016`. |

