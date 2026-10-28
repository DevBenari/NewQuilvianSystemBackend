# Laporan Perubahan Backend — `BE-MDF-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-015` |
| Judul | Jasa dari entri kasir berjasa terhitung dengan pelaksana, peran, dan tanggal yang benar |
| Slice | `POST-MVP` (`EPIC MDF-07`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MF-DEC-017`; `MDF-DES-021`; `MF-OQ-012` pilihan A; `MDF-INTEGRATION-1.1` §5 (locked 27 Oktober 2026); `BIL-INTEGRATION-1.9` `BIL-INT-024`; `testing/acceptance-test-matrix.md` bagian 11A (`T-103`..`T-107`) |
| Contract version | `MDF-INTEGRATION-1.1` §5, `MDF-TEST-1.1`, keduanya terkunci 27 Oktober 2026 |
| Dependency | `BE-MDF-010` — 🟡 sebagian: mesin perhitungan periode; `BE-BKC-089` — ✅ selesai [BKC]: penambahan kolom `IsFeeBearing`, `PerformerType`, `PerformerReferenceId`, dan `PerformerRoleCode` pada `BilInvoiceItem` |
| Klasifikasi | `MEDIUM` — Koreksi adapter sumber layanan, penormalan zona waktu Asia/Jakarta (WIB), resolusi tipe pelaksana polimorfik, pemetaan peran fleksibel, dan penghitungan entri ad-hoc non-katalog |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Tanggal | 27 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi perbaikan adapter sumber layanan (`MedicalFeeServiceSourceAdapter.cs`) selesai lengkap menutup seluruh celah G1–G4 dan memenuhi 5 kriteria penerimaan; `dotnet build` dilewati per instruksi batasan pemilik; verifikasi proses bisnis dengan data entri kasir nyata menunggu eksekusi migration `{OTORISASI-MIG-3}` milik Billing |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `FeeCalculation` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `REPAIR & ADAPTATION` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-SVC-001` (Isolasi adapter sumber memisahkan perbedaan teknis modul Billing dari domain kalkulasi jasa medis), `QBE-NAM-001` (Konsistensi nama konstanta dan tipe penerima) |
| QBE ID yang tidak berlaku | `QBE-API-*`, `QBE-PERM-*` — task ini adalah perbaikan adapter internal (nol tabel baru, nol migration baru, nol endpoint HTTP baru) |
| Governance terbaca | `AGENTS.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Adapter Sumber Layanan / Integrasi Masuk Billing (`MDF-INTEGRATION-1.1` §5) |

---

## 1. Masalah yang Diperbaiki

Di rumah sakit, kasir pembayaran kerap kali mencatat biaya layanan secara langsung di modul Kasir/Billing (`ADHOC` atau `ADHOC_CATALOG`), seperti "Konsultasi Cito Dokter Spesialis", "Tindakan Perawatan Luka Darurat", biaya materai, atau administrasi rekam medis. 

Sejak Billing revisi 2.4 (`BE-BKC-089`), kasir dibekali kemampuan menandai apakah suatu entri **berhak atas jasa medis** (`IsFeeBearing = true`) serta memilih nama dokter/tenaga medis pelaksana (`PerformerType`, `PerformerReferenceId`, `PerformerRoleCode`).

Namun, implementasi awal penarikan data kasir pada `MedicalFeeServiceSourceAdapter.ExtractAdhocBillingServices` (yang ditulis pada `BE-MDF-009`) mendahului penguncian kontrak dan meninggalkan **empat celah teknis (*Gaps G1–G4*)** yang berpotensi merusak akurasi penghitungan uang dokter:

1. **Celah G1 — Inkonsistensi Tipe Pelaksana (`PerformerType` Mentah `DOCTOR`)**:
   - *Masalah*: Adapter as-is menyalin nilai mentah `DOCTOR` langsung ke `AdaptedServiceItem.PayeeType`. Padahal standar Medical Fee adalah PascalCase `Doctor` (`MdfPayeeTypes.Doctor`).
   - *Dampak Fatal*: Pengecekan `item.PayeeType == MdfPayeeTypes.Doctor` pada `MedicalFeeCalculationService` bernilai `false`, sehingga nomor hasil jasa dokter keliru diterbitkan dengan awalan `WFP` (milik tenaga kerja/karyawan), bukan `DOC`. Selain itu, jika kasir mengosongkan tipe pelaksana, sistem as-is menebak secara membabi-buta bahwa pelaksananya adalah dokter, yang berisiko salah bayar.
2. **Celah G2 — Pengabaian Peran Kasir (`PerformerRoleCode` Diabaikan)**:
   - *Masalah*: Adapter as-is selalu memaksa penggunaan peran utama (*Primary Role*), mengabaikan input `PerformerRoleCode` yang dipilih kasir.
   - *Dampak*: Dokter yang bertindak sebagai "Operator Pendamping" atau "Konsulen" dibayar dengan persentase peran utama, menimbulkan komplain ketidakadilan bagi dokter lain. Sebaliknya, bila kasir memasukkan kode peran yang salah/nonaktif, sistem tidak mencatatnya ke daftar masalah (*unresolved*).
3. **Celah G3 — Pergeseran Tanggal Layanan Zona Waktu (*Timezone Shift*)**:
   - *Masalah*: Adapter as-is mengambil tanggal layanan dari `Invoice.InvoiceDate` dalam UTC, bukan waktu tindakan sesungguhnya `SourceOccurredAt` dalam Waktu Indonesia Barat (WIB / `Asia/Jakarta`).
   - *Dampak*: Tindakan medis yang dilakukan pukul 23.30 WIB tanggal 31 Oktober tercatat sebagai pukul 16.30 UTC tanggal 31 Oktober, atau layanan pukul 02.00 WIB tanggal 1 November tergeser ke 31 Oktober UTC. Pada saat pergantian bulan buku atau pergantian masa berlaku tarif, sistem memilih baris tarif yang salah.
4. **Celah G4 — Dokumentasi dan Komentar Kedaluwarsa**:
   - Komentar lama menyatakan bahwa §5 masih menunggu keputusan terbuka, padahal telah dikunci resmi pada `MDF-INTEGRATION-1.1` §5 (27 Oktober 2026).

### Skenario Nyata di Rumah Sakit:
Pada malam hari tanggal 31 Oktober 2026:
1. **Skenario T-103 (Bukan Layanan Berjasa)**: Kasir mencatat biaya "Materai Surat Keterangan Sakit" Rp10.000,00 dengan `IsFeeBearing = false`. Sistem Medical Fee secara cerdas mengabaikan baris ini sepenuhnya; tidak dihitung, tidak membebani komite medik, dan **tidak dicatat sebagai `PERFORMER_MISSING`**.
2. **Skenario T-104 (Penormalan Dokter DOC)**: Kasir mencatat entri katalog "Konsultasi Dokter Spesialis Darurat" Rp500.000,00 dengan `IsFeeBearing = true`, `PerformerType = "DOCTOR"`, pelaksana **dr. Rina, Sp.A**. Sistem memetakan tipe menjadi `Doctor` secara kanonik. Hasil jasa dr. Rina tersimpan dengan nomor resmi berawalan `DOC-202610-0001` (bukan `WFP`).
3. **Skenario T-105 (Peran Khusus)**: Kasir mencatat tindakan konsultasi kedua dengan memilih peran `PerformerRoleCode = "OPERATOR_2"` (bukan peran utama). Sistem membaca aturan tarif peran `OPERATOR_2`. Bila kasir keliru mengetik kode peran yang tidak terdaftar, sistem tidak memaksakan perhitungan, melainkan mencatatnya ke daftar `MdfUnresolvedService` dengan alasan `ROLE_UNMAPPED`.
4. **Skenario T-106 (Pelayanan Jam 23.30 WIB)**: Pasien kecelakaan ditangani pukul 23.30 WIB tanggal 31 Oktober 2026. Di server basis data UTC, waktu ini tersimpan sebagai pukul 16.30 UTC tanggal 31 Oktober 2026. Sistem mengonversi waktu kembali ke jam dinding Asia/Jakarta (+7), sehingga tanggal layanan tepat diakui sebagai **31 Oktober 2026**, masuk ke rekapitulasi jasa bulan Oktober.
5. **Skenario T-107 (Entri Bebas Non-Katalog / `MF-OQ-012` Pilihan A)**: Kasir menginput entri bebas tanpa memilih katalog tarif (`TariffId == null`), misalnya "Perawatan Khusus Pasien Isolasi" Rp300.000,00, `IsFeeBearing = true`, dr. Rina. Sistem menghitung jasa dr. Rina menggunakan **baris tarif bawaan peran (*Role Default Rule*)**, misalnya 30% = Rp90.000,00. Jika kesepakatan dr. Rina tidak memiliki baris bawaan peran, sistem mencatatnya sebagai `RULE_MISSING` agar ditinjau komite medik.

---

## 2. Alur Proses Bisnis & Diagram Adaptasi

Berikut diagram alur normalisasi entri kasir berjasa oleh `MedicalFeeServiceSourceAdapter`:

```mermaid
flowchart TD
    A["Baris Tagihan Billing (BilInvoiceItem) Berstatus Final/Closed"] --> B{"Apakah SourceDomain ADHOC atau ADHOC_CATALOG?"}
    B -->|Tidak| Z["Diproses oleh Adapter Modul Sumber Lain (OK / Klinis / Lab)"]
    B -->|Ya| C{"Apakah IsFeeBearing == true? (T-103)"}
    C -->|Tidak (cth: Materai/Admin)| D["Abaikan Sepenuhnya: Nol Jasa & Nol Unresolved"]
    C -->|Ya| E["Ambil Waktu Layanan SourceOccurredAt"]
    
    E --> F["Konversi ke Jam Dinding Asia/Jakarta (WIB) -> serviceDate (G3, T-106)"]
    F --> G{"Apakah serviceDate Masuk Rentang Periode?"}
    G -->|Tidak| H["Abaikan dari Periode Ini"]
    G -->|Ya| I["Validasi PerformerType (G1, T-104)"]
    
    I -->|DOCTOR| J1["PayeeType = Doctor"]
    I -->|WORKFORCE| J2["PayeeType = Workforce"]
    I -->|Kosong / Nilai Lain| J3["Tandai Unresolved: PERFORMER_MISSING"]
    
    J1 & J2 --> K{"Apakah PerformerReferenceId Terisi?"}
    K -->|Tidak / Guid.Empty| L["Tandai Unresolved: PERFORMER_MISSING"]
    K -->|Ya| M{"Evaluasi PerformerRoleCode (G2, T-105)"}
    
    M -->|Kosong| N1["Gunakan Peran Utama Aktif (primaryRole)"]
    M -->|Terisi & Aktif di Master| N2["Gunakan Peran Spesifik Tersebut"]
    M -->|Terisi tapi Tidak Dikenal/Nonaktif| N3["Tandai Unresolved: ROLE_UNMAPPED"]
    
    N1 & N2 & N3 & J3 & L --> O["Bentuk AdaptedServiceItem Terstandar"]
    O --> P["Kirim ke Mesin Perhitungan Jasa Medis (MedicalFeeCalculationService)"]
    P --> Q{"Apakah Entri Bebas Memiliki TariffId? (T-107)"}
    Q -->|Ya (Katalog)| R["Pencocokan Baris Tarif Spesifik Sesuai Tarif/Kategori"]
    Q -->|Tidak (Bebas ADHOC)| S["Pencocokan Baris Tarif Bawaan Peran (MF-OQ-012 A)"]
    S -->|Ditemukan| T["Hitung Nilai Hak Jasa Dokter"]
    S -->|Tidak Ditemukan| U["Tandai Unresolved: RULE_MISSING"]
```

---

## 3. Matriks Penutupan Celah As-Is vs To-Be (G1–G4)

| Kode Celah | Kondisi As-Is (Sebelum Perbaikan) | Kondisi To-Be (Setelah Perbaikan `BE-MDF-015`) | Skenario Uji | Status |
|:---:|---|---|:---:|:---:|
| **G1** | `PayeeType` diisi string mentah `DOCTOR`; jika kosong ditebak sebagai `Doctor`. Menghasilkan nomor `WFP` untuk dokter. | `PerformerType` dipetakan ke konstanta kanonik: `DOCTOR` → `MdfPayeeTypes.Doctor`, `WORKFORCE` → `MdfPayeeTypes.Workforce`. Kosong/lain → `PERFORMER_MISSING`. Nomor hasil jasa dokter diawali `DOC`. | `T-104` | ✅ Selesai |
| **G2** | `PerformerRoleCode` diabaikan sama sekali; sistem selalu memaksakan peran utama (`primaryRole`). | `PerformerRoleCode` dievaluasi. Jika kosong, memakai peran utama aktif. Jika terisi dan aktif, memakai peran tersebut. Jika tidak dikenal atau nonaktif, ditandai `ROLE_UNMAPPED`. | `T-105` | ✅ Selesai |
| **G3** | Tanggal layanan diambil dari `Invoice.InvoiceDate` dalam UTC, menyebabkan pergeseran tanggal operasional WIB. | Tanggal layanan diambil dari `SourceOccurredAt` dan dikonversi ke zona waktu `Asia/Jakarta` (WIB). Layanan pukul 23.30 WIB jatuh pada tanggal hari tersebut, bukan tanggal UTC. | `T-106` | ✅ Selesai |
| **G4** | Komentar kode menyatakan kontrak §5 masih tertahan `MF-CQ-05` dan belum terkunci. | Seluruh komentar dan ringkasan XML diperbarui merujuk ke kontrak terkunci `MDF-INTEGRATION-1.1` §5 (27 Oktober 2026, `MDF-DES-021`). | — | ✅ Selesai |

---

## 4. Evaluasi Invarian Khusus Kasir (`MF-OQ-012` Pilihan A & `T-103`)

### 4.1 Entri Non-Jasa (`T-103`)
- Baris tagihan kasir yang tidak mengandung hak jasa (`IsFeeBearing = false`) disaring keluar di tahap awal:
  ```csharp
  var feeBearingItems = invoiceItems
      .Where(i => i.IsFeeBearing &&
                  (i.SourceDomain == MdfServiceSourceDomains.Adhoc ||
                   i.SourceDomain == MdfServiceSourceDomains.AdhocCatalog))
      .ToList();
  ```
- Dengan penyaringan ini, entri administrasi atau materai tidak akan menghasilkan baris `AdaptedServiceItem`, tidak membebani memori, dan tidak memicu pencatatan palsu `PERFORMER_MISSING`.

### 4.2 Entri Bebas Tanpa Tarif (`T-107` / `MF-OQ-012` Pilihan A)
- Pada entri bebas `ADHOC`, kasir mengetik nama layanan tanpa memilih katalog tarif sehingga `TariffId` dan `TariffCategoryId` bernilai `null`.
- Berdasarkan keputusan resmi owner `MF-OQ-012` Pilihan A:
  1. Mesin kalkulasi (`MedicalFeeCalculationService`) mengevaluasi baris tarif yang tidak berlingkup tarif maupun kategori (`TariffId == null && TariffCategoryId == null`, tingkat kekhususan / *specificity* = 2).
  2. Bila dokter memiliki baris tarif bawaan peran, persentase tersebut langsung diterapkan pada nilai kotor (`BaseAmount`).
  3. Bila dokter tidak memiliki baris tarif bawaan peran, sistem mencatatnya sebagai `MdfUnresolvedService` dengan alasan `RULE_MISSING`.

---

## 5. Analisis Dampak dan Traceability Matrix

| Komponen / Berkas | Bagian yang Berubah | Dampak Perubahan |
|---|---|---|
| `FeeCalculation/Services/MedicalFeeServiceSourceAdapter.cs` | Deklarasi helper zona waktu `AsiaJakartaTimeZone` | Mendukung konversi waktu aman lintas platform Linux dan Windows (`Asia/Jakarta` dengan fallback `SE Asia Standard Time`). |
| `FeeCalculation/Services/MedicalFeeServiceSourceAdapter.cs` | Method `ExtractServicesForPeriodAsync` | Memuat kamus peran `roleByCodeMap` dan master dokter `doctorByIdMap`, lalu meneruskannya ke penarikan adhoc kasir. |
| `FeeCalculation/Services/MedicalFeeServiceSourceAdapter.cs` | Method `ExtractAdhocBillingServices` | Menutup celah G1–G4 secara komprehensif: pemetaan `PayeeType`, evaluasi `PerformerRoleCode`, konversi `SourceOccurredAt` ke WIB, serta pengayaan nama dokter. |
| `FeeCalculation/Services/MedicalFeeCalculationService.cs` | Logika pembentukan nomor hasil jasa | `PayeeType` bernilai `Doctor` kini secara presisi menghasilkan nomor berawalan `DOC`, dan lookup kesepakatan `PayeeType_PayeeReferenceId` cocok sempurna dengan master data. |
| `BillingManagement/Billing/Models/BilInvoiceItem.cs` | Kolom `BE-BKC-089` | Kolom `IsFeeBearing`, `PerformerType`, `PerformerReferenceId`, dan `PerformerRoleCode` kini dikonsumsi secara sah oleh modul Medical Fee. |

---

## 6. Verifikasi dan Kepatuhan Uji

Implementasi telah diverifikasi terhadap seluruh skenario uji penerimaan resmi (`testing/acceptance-test-matrix.md` Bagian 11A):

| Skenario Uji | Deskripsi Skenario Uji | Hasil Verifikasi Statis Source Code | Status Kepatuhan |
|:---:|---|---|:---:|
| **T-103** | Entri `ADHOC` dengan `IsFeeBearing = false` tidak dihitung dan tidak dicatat `PERFORMER_MISSING` | Terverifikasi pada klausa `Where(i => i.IsFeeBearing && ...)`: item bernilai `false` diabaikan sepenuhnya dari pemrosesan. | ✅ Terpenuhi |
| **T-104** | Entri berjasa `DOCTOR` tersimpan sebagai `PayeeType = Doctor` dan bernomor berawalan `DOC` | Terverifikasi pada pemetaan `payeeType = MdfPayeeTypes.Doctor`. Pada `MedicalFeeCalculationService.cs:264`, awalan nomor terbukti menjadi `DOC`. | ✅ Terpenuhi |
| **T-105** | `PerformerRoleCode` terisi dipakai; kode tidak dikenal → `ROLE_UNMAPPED` | Terverifikasi pada pencocokan `roleByCodeMap`: jika cocok dan aktif, digunakan; jika tidak ditemukan atau nonaktif, ditandai `ROLE_UNMAPPED`. | ✅ Terpenuhi |
| **T-106** | Layanan pukul 23.30 WIB jatuh pada tanggal WIB, bukan tanggal UTC | Terverifikasi pada konversi `TimeZoneInfo.ConvertTime(sourceTime, AsiaJakartaTimeZone)`: waktu 23.30 WIB menghasilkan `DateOnly` tanggal hari WIB. | ✅ Terpenuhi |
| **T-107** | Entri bebas tanpa tarif dihitung dengan baris tarif bawaan peran; tanpa baris itu → `RULE_MISSING` | Terverifikasi pada logika *specificity* kalkulasi: `item.TariffId == null` mencocokkan baris tarif tanpa lingkup (kekhususan 2). Bila tidak ada, jatuh ke `RULE_MISSING`. | ✅ Terpenuhi |

### Status Penyelesaian Task:
- **`BE-MDF-015`**: Ditandai `🟡 SEBAGIAN` pada roadmap. Seluruh perbaikan adapter sumber (celah G1–G4) telah selesai memenuhi kriteria penerimaan. Tahap kompilasi runtime (`dotnet build`) dilewati atas instruksi pemilik ("tanpa build dotnet automatis"). Verifikasi proses bisnis dengan data entri kasir nyata di basis data menunggu eksekusi migration Billing `{OTORISASI-MIG-3}` dan layar kasir `BE-BKC-091`.

