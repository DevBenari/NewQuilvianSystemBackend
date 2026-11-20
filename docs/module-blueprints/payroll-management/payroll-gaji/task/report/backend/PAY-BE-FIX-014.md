# Laporan Perbaikan Backend — `PAY-BE-FIX-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-014` (ad-hoc lanjutan seri `PAY-BE-FIX`); pasangan frontend `PAY-FE-FIX-014` |
| Judul | Slice master data: Grade Gaji dan Struktur Gaji pada Kebijakan Tunjangan Transportasi (Paket 1) |
| Slice | Corporate / Payroll Management (`Prl`). **Hanya F3 (master data)** dari desain `PAY-BE-FIX-013` |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Trace | Keputusan pemilik 20 Nov 2026: **D9 = Paket 1**; pemilik menerapkan DDL sendiri. Desain: `PAY-BE-FIX-013` |
| Contract version | Tidak ada kontrak bertversi; semua perubahan **additive** pada endpoint yang sudah ada |
| Dependency | **DDL Paket 1 wajib sudah diterapkan di database sebelum backend ini dijalankan** (bagian 7). `PAY-FE-FIX-014` membaca kontrak ini |
| Klasifikasi | `HEAVY` — skor 9 (berkas diperiksa >20: 2; berkas diubah 4–8: 1; logika sedang: 1; kontrak API additive: 1; database schema: 2; keamanan berkaitan: 1; UI backend: 1) |
| Task mode | `CROSS-REPO`, tahap backend; target tulis hanya `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `d642a9df9de35801ed8332adc0cdcd6127f96353` (`Yasmina`); working tree memuat pekerjaan `PAY-BE-FIX-010`, `011`, dan pihak lain |
| Tanggal | 20 November 2026 |
| Status | 🟡 **Source selesai untuk slice F3; belum dibuild dan belum diuji.** `dotnet build` `NOT RUN` (dilarang). Tidak ada klaim runtime |

**Backend Governance Preflight.** Area `Corporate`; Module `PayrollManagement`; prefix `Prl` terdaftar `ACTIVE` pada registry backend sejak 2026-11-11. Keberlakuan `TOUCHED LEGACY`: tidak ada entity baru, hanya dua properti dan dua navigasi pada entity yang sudah ada, sehingga QBE-ENT, QBE-NAM, dan QBE-MOD-002/003 tidak berlaku. QBE ID yang berlaku: QBE-API-001 (respons tetap `ApiResponse<T>`/`PagedResult<T>`, perubahan additive), QBE-PERM-001 (atribut Access tidak diubah), QBE-DTO-001 (hanya DTO), QBE-VAL-001 (validasi relasi pada Create, Update, dan aktivasi), QBE-CFG-002 (konfigurasi disempurnakan dengan aman). Berkas baru `TransportAllowancePolicyScopeRules.cs` adalah kelas aturan murni; controller lama tetap memakai `ApplicationDbContext` langsung (pola legacy, tidak diperluas).

---

## 1. Masalah yang diperbaiki

Kebijakan transport hanya punya cakupan Legal Entity, Fasilitas, dan Grade Pegawai. Tidak ada cara menyatakan "kebijakan ini untuk Struktur Gaji tertentu", sehingga kebijakan struktur A dapat ditawarkan dan ditetapkan kepada pegawai berstruktur B. Selain itu, filter `salaryGradeId` pada opsi hanya menerjemahkan grade gaji menjadi grade pegawai; ia tidak menyaring cakupan yang tersimpan pada kebijakan.

---

## 2. Proses bisnis

**Hubungan data.** `Grade Pegawai` (kepegawaian) ← `Grade Gaji` (penggajian) ← `Struktur Gaji` (milik tepat satu grade gaji). Tiga hal ini **tidak disatukan**; Grade Pegawai tetap atribut tersendiri.

**Aturan validasi** (`TransportAllowancePolicyScopeRules.ValidateScope`, satu tempat, dipakai Create, Update, dan aktivasi):

| No | Aturan | Pesan bila gagal |
| ---: | --- | --- |
| 1 | Struktur tanpa grade ditolak | "Pilih Grade Gaji terlebih dahulu sebelum memilih Struktur Gaji." |
| 2 | Grade harus ada, belum dihapus, dan aktif | "Grade Gaji tidak ditemukan atau tidak aktif." |
| 3 | Struktur harus ada, belum dihapus, dan aktif | "Struktur Gaji tidak ditemukan atau tidak aktif." |
| 4 | Struktur harus milik grade yang dipilih | "Struktur Gaji yang dipilih bukan milik Grade Gaji yang dipilih." |
| 5 | Legal entity dan fasilitas kebijakan tidak boleh bertentangan dengan struktur (bila keduanya terisi) | "Legal entity kebijakan berbeda dengan legal entity Struktur Gaji yang dipilih." |
| 6 | Masa berlaku kebijakan harus berada di dalam masa berlaku grade dan struktur | "Tanggal mulai berlaku kebijakan tidak boleh sebelum tanggal mulai Struktur Gaji (01/01/2026)." |
| 7 | Tidak boleh ada kebijakan **aktif** lain dengan cakupan persis sama dan masa berlaku beririsan | `409`: "Sudah ada kebijakan aktif (TAP-… - …) dengan cakupan yang sama dan masa berlaku yang beririsan." |

**Contoh.** Kebijakan "Transportasi Bulanan" untuk Grade Gaji `A` dan Struktur `A-Dokter` (berlaku 1 Jan 2026 – 31 Des 2026). Struktur `A-Dokter` berlaku sejak 1 Maret 2026: permintaan ditolak dengan pesan tanggal mulai. Bila tanggal mulai diubah ke 1 Maret 2026, kebijakan tersimpan dengan `ScopeStatus = Scoped`. Permintaan yang mengirim struktur milik grade `B` bersama grade `A` ditolak walau dikirim langsung lewat API.

**Status cakupan** (turunan, tidak disimpan):

| `ScopeStatus` | Arti |
| --- | --- |
| `Scoped` | Grade dan struktur terisi |
| `GradeScoped` | Grade saja: berlaku untuk seluruh struktur grade itu |
| `LegacyUnscoped` | Keduanya kosong (data lama); perilaku lama **dipertahankan**, tidak dimigrasikan, dan tidak pernah dianggap bukti kelayakan untuk grade/struktur tertentu |
| `Invalid` | Struktur tanpa grade; dicegah check constraint, hanya muncul bila data dimanipulasi di luar aplikasi |

**Aturan keselamatan perubahan (konservatif, menunggu keputusan pemilik):**

| Kejadian | Perilaku |
| --- | --- |
| `PUT` mengosongkan cakupan yang sudah ada | Ditolak `400`. Mengosongkannya diam-diam mengubah kebijakan menjadi "tanpa cakupan", dan melindungi klien lama yang tidak mengirim dua field baru |
| `PUT` mengubah cakupan kebijakan yang sudah dirujuk penetapan transport pegawai | Ditolak `409`; buat kebijakan baru |
| `PUT` pada kebijakan **nonaktif** dengan cakupan tidak berubah | Diizinkan walau grade/struktur sudah nonaktif (data historis tetap dapat disunting) |
| `PATCH /{id}/status` mengaktifkan kembali kebijakan ber-cakupan | Relasi divalidasi ulang, ditambah irisan; tidak mengaktifkan relasi yang tidak valid |
| Kebijakan legacy tanpa cakupan | Tidak dievaluasi ulang pada validasi ini |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpTransportAllowancePolicyController.cs` (seluruh bagian terkait), `WfpTransportAllowancePolicyDtos.cs`, `PrlTransportAllowancePolicy.cs`, `PrlTransportAllowancePolicyConfiguration.cs`, `MstSalaryGrade.cs`, `MstSalaryStructure.cs`, `WfpTransportAllowanceController.cs` (validasi penetapan), `PayrollCalculationService.cs` (`BuildTransportAllowanceLines`), snapshot dan migration (ketiadaan kolom).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/PayrollManagement/Services/TransportAllowancePolicyScopeRules.cs` | **Baru.** Aturan murni cakupan: `ScopeStatusOf`, `ValidateScope`, konstanta `TransportPolicyScopeStatuses`, rekaman fakta grade dan struktur |
| `Areas/Corporate/PayrollManagement/Models/PrlTransportAllowancePolicy.cs` | + `SalaryGradeId?`, `SalaryStructureId?`, navigasi `SalaryGrade`, `SalaryStructure` |
| `Repositories/Configurations/Corporate/PayrollManagement/PrlTransportAllowancePolicyConfiguration.cs` | FK `Restrict` dan dua indeks dengan nama eksplisit **identik dengan DDL**; check constraint `CK_..._StructureRequiresGrade` |
| `Areas/Corporate/PayrollManagement/DTOs/WfpTransportAllowancePolicyDtos.cs` | Request, respons, opsi, dan filter default + dua id, dua nama, `ScopeStatus` |
| `Areas/Corporate/PayrollManagement/Controllers/WfpTransportAllowancePolicyController.cs` | Filter daftar, opsi, pemetaan (tiga tempat), Create, Update, `UpdateStatus`, validasi bersama, pemetaan galat `23514` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Additive.** Field lama dan semantik lama dipertahankan. Parameter baru bersifat opsional |
| Database | **Bergantung DDL Paket 1** (bagian 7). Tidak ada migration dibuat, tidak ada perubahan `Migrations/` atau snapshot |
| Keamanan/Auth | `[AccessAction]` dan `[AccessPermission]` tidak diubah. Tidak ada pemeriksaan peran tertanam |
| Payroll | **Tidak berubah.** `BuildTransportAllowanceLines` dan validasi penetapan pegawai belum disentuh (F4 dan F6) |

---

## 4. Dokumentasi endpoint

#### Corporate / Payroll Management / Transport Allowance Policy

Base URL: `api/v1/corporate/payroll-management/transport-allowance-policies` (alias lama `api/v1/corporate/human-resource/payroll-management/transport-allowance-policies` tetap)

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kebijakan | `TransportAllowancePolicy : Read` | Query `salaryGradeId`, `salaryStructureId` menyaring cakupan **tersimpan**; item + `salaryGradeId/Name`, `salaryStructureId/Name`, `scopeStatus` |
| `GET` | `/options` | Pilihan kebijakan untuk dropdown | `TransportAllowancePolicy : Read` | Query `salaryStructureId` baru; `salaryGradeId` kini juga menyaring cakupan tersimpan (lihat di bawah); item + cakupan dan `scopeStatus` |
| `GET` | `/{id}` | Detail | `TransportAllowancePolicy : Read` | + dua id, dua nama, `scopeStatus` |
| `POST` | `/` | Membuat | `TransportAllowancePolicy : Create` | Body + `salaryGradeId`, `salaryStructureId` (opsional); validasi relasi |
| `PUT` | `/{id}` | Mengubah penuh | `TransportAllowancePolicy : Update` | Body + dua id; validasi ulang; aturan keselamatan bagian 2 |
| `PATCH` | `/{id}/status` | Aktif/nonaktif | `TransportAllowancePolicy : Update` | Mengaktifkan kebijakan ber-cakupan memvalidasi ulang relasi dan irisan |
| `DELETE` | `/{id}` | Hapus (soft delete) | `TransportAllowancePolicy : Delete` | Tidak berubah |
| `GET` | `/filters/metadata`, `/summary` | Metadata dan ringkasan | `TransportAllowancePolicy : Read` | Filter default + dua id (additive) |

**Semantik `/options` (kompatibilitas).**

| Parameter | Perilaku |
| --- | --- |
| Hanya `salaryGradeId` | Seperti sebelumnya: diterjemahkan menjadi grade pegawai yang terhubung dan menyaring kebijakan berdasarkan `EmployeeGradeId` (kebijakan tanpa grade pegawai tetap lolos). **Ditambah:** kebijakan ber-cakupan hanya muncul bila `SalaryGradeId`-nya sama; kebijakan legacy `null`/`null` tetap muncul (perilaku existing) dan ditandai `LegacyUnscoped` |
| `salaryStructureId` (± `salaryGradeId`) | Struktur menentukan grade. Hanya kebijakan struktur itu, kebijakan grade-saja dari grade yang sama, dan legacy yang muncul. Kebijakan **struktur lain tidak pernah** muncul. Pasangan yang tidak cocok → `400` |
| Tanpa keduanya | Tidak berubah |

Kode status: `200` berhasil; `400` validasi (pasangan tidak cocok, master tidak aktif, tanggal di luar masa berlaku, cakupan dikosongkan); `404` kebijakan tidak ditemukan; `409` irisan kebijakan atau cakupan dikunci karena sudah dirujuk penetapan pegawai.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Dilarang pada task ini; **kompilasi belum dibuktikan** |
| QBE preflight | Dicatat di Metadata | `PASS` | `Prl` terdaftar `ACTIVE`; tanpa entity baru |
| Review diff/scope | 5 berkas source milik task ini (4 diubah, 1 baru); `Migrations/` dan snapshot tidak berubah; tidak ada dependency atau konfigurasi aplikasi berubah | `PASS` | `git status --short` pada 5 berkas, laporan ini, dan `Migrations/` |
| Konsistensi simbol | Nama dan tipe properti `MstSalaryGrade`/`MstSalaryStructure` (`SalaryGradeName`, `SalaryStructureName`, `Guid?` legal entity/fasilitas, `DateTime?` masa berlaku, `Guid` grade pada struktur) cocok dengan record fakta dan proyeksi; namespace master sudah di-import; urutan argumen pemanggil `ValidateScopeAndOverlapAsync` (2 tempat) dan `ValidateRequestAsync` (2 tempat) cocok dengan signature | `PASS` (bacaan kode, bukan kompilasi) | Pembacaan penuh controller, aturan murni, DTO, konfigurasi; `Grep` pada model master |
| Akhir baris | Tidak ada berkas campuran: empat berkas yang diubah CRLF utuh (jumlah CR = jumlah LF per berkas), berkas baru LF | `PASS` | `tr -cd '\r' \| wc -c` per berkas |
| Verifikasi kontrak API | Tercatat pada bagian 4 | `PASS` (dari source) | Controller |
| Runtime, Swagger, SQL yang dihasilkan EF | Tidak dilakukan | `NOT RUN` | Server dev dan build dilarang |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

**Titik yang paling berisiko tanpa build:** proyeksi konstruktor `new TransportPolicyGradeFacts(...)` dan `new TransportPolicyStructureFacts(...)` di dalam `Select` EF (didukung EF Core, tetapi belum dicoba); argumen bernama non-trailing pada `ValidateRequestAsync(null, request, willBeActive: true, existing: null, cancellationToken)`; perbandingan `Guid?` terhadap `Guid?` pada query irisan. Perlu dilihat saat `dotnet build` pertama.

---

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Model dan konfigurasi memuat `SalaryGradeId`, `SalaryStructureId` dengan FK aman dan indeks | Terpenuhi pada source | Bagian 3.2 |
| Validasi pasangan grade-struktur pada Create **dan** Update | Terpenuhi pada source | `ValidateRequestAsync` memanggil validasi bersama |
| Permintaan yang dimanipulasi ditolak | Terpenuhi pada source | Validasi di backend, bukan di dropdown |
| Daftar, opsi, detail mengembalikan id dan nama | Terpenuhi pada source | Tiga pemetaan diperbarui |
| Opsi tidak merusak konsumen lama | Terpenuhi pada source | Jalur terjemahan grade pegawai dipertahankan |
| Legacy tidak dihapus atau dimigrasikan | Terpenuhi | Tidak ada penulisan data lama |
| Aktivasi tidak menerima relasi tidak valid | Terpenuhi pada source | `UpdateStatus` |
| Delete tetap menolak kebijakan terpakai | Terpenuhi | Tidak berubah |
| Validasi penetapan pegawai terhadap penetapan gaji (F4) | **Di luar slice** | Belum dikerjakan |
| Kalkulasi payroll per segmen (F6) | **Di luar slice** | Belum dikerjakan |
| Build berhasil; endpoint dicoba | **Belum terpenuhi** | `NOT RUN` |

---

## 7. Urutan rilis dan status database

**Backend ini tidak aman dijalankan sebelum DDL Paket 1 diterapkan.** Model memetakan dua kolom baru; tanpa kolom itu, **setiap pembacaan tabel kebijakan gagal (`42703`)**, termasuk daftar kebijakan, opsi, dan perhitungan Payroll (yang memuat kebijakan lewat penetapan). Urutan: (1) terapkan DDL pada `PAY-BE-FIX-013` bagian 3 (nama identifier sudah dikoreksi); (2) baru jalankan backend ini.

Ini **menyimpang dari prompt asal** ("jangan mengaktifkan endpoint tulis yang bergantung pada kolom yang belum tersedia"): tidak ada gerbang runtime yang membuat kode ini dormant. Penyimpangan itu sadar dan didasari keputusan pemilik 20 Nov 2026 (memilih Paket 1 dan menerapkan DDL sendiri). Jaminan keselamatannya adalah urutan rilis, bukan kode.

| Hal | Status |
| --- | --- |
| DDL Paket 1 | **Belum diverifikasi** terapan di database mana pun (tidak dibaca) |
| Nama constraint dan indeks | Identik antara DDL dan konfigurasi EF (diukur ≤ 58 karakter) |
| Migration EF di kemudian hari | Harus idempoten atau ditandai terapan, dan snapshot harus selaras, agar tidak terulang insiden `42P07` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Cakupan yang saling melingkupi (grade saja versus grade+struktur) **tidak** diberi prioritas otomatis; menunggu keputusan pemilik |
| Masalah yang diketahui | Pra-ada: filter `salaryGradeId` pada opsi untuk grade gaji tanpa grade pegawai tidak menyaring kebijakan legacy; dipertahankan demi kompatibilitas |
| Risiko tersisa | Kompilasi belum dibuktikan; SQL EF belum dilihat; F4 dan F6 belum ada, jadi **kebijakan struktur lain masih dapat ditetapkan ke pegawai dan dibayar** sampai fase itu selesai |
| Perubahan sampingan | `NONE` oleh task ini. Pekerjaan pihak lain (`PAY-BE-FIX-011` dan lainnya) pada working tree yang sama tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Tidak ada stage, commit, push, pull, merge, atau rebase |
| Langkah berikutnya | Pemilik menerapkan DDL; `PAY-FE-FIX-014`; lalu F4 (validasi penetapan pegawai) dan F6 (kalkulasi per segmen, jalur uang) |

```text
$ git status --short   (berkas milik task ini)
 M Areas/Corporate/PayrollManagement/Controllers/WfpTransportAllowancePolicyController.cs
 M Areas/Corporate/PayrollManagement/DTOs/WfpTransportAllowancePolicyDtos.cs
 M Areas/Corporate/PayrollManagement/Models/PrlTransportAllowancePolicy.cs
 M Repositories/Configurations/Corporate/PayrollManagement/PrlTransportAllowancePolicyConfiguration.cs
?? Areas/Corporate/PayrollManagement/Services/TransportAllowancePolicyScopeRules.cs
?? docs/module-blueprints/payroll-management/payroll-gaji/task/report/backend/PAY-BE-FIX-014.md
```
