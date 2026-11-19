# Laporan Perubahan Backend — `PAY-BE-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-001` (ad-hoc, bukan kartu roadmap) |
| Judul | Profil Penggajian berangkat dari populasi HRD; profil payroll opsional |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap, traceability, dan kontrak `PGJ-C1` tidak disentuh (preseden `BE-BKC-FIX-025`) |
| Trace | Perintah pemilik 2026-11-17 (master prompt "Perbaikan Data Profil Penggajian Kosong"). Tanpa FR-PGJ/PRL-DEC bernomor — tidak dikarang |
| Contract version | **Tidak tercakup `PGJ-C1`.** Perubahan kontrak bersifat additive pada `PayrollCompensation` (lihat bagian 4); amandemen kontrak belum ditulis |
| Dependency | `PAY-BE-24` (pembaca sumber HRD, penyiapan profil otomatis, integrasi Payroll Run) yang sudah ada di working tree dan **belum di-commit** |
| Klasifikasi | `MEDIUM` — satu service ditulis ulang, satu evaluator baru, tiga berkas disentuh, satu endpoint `GET` baru; tanpa entity baru dan tanpa migration |
| Task mode | `BACKEND` |
| Target tulis | `PayrollCompensationService.cs`, `PayrollCompensationDtos.cs`, `PayrollCompensationsController.cs`, `PayrollReadinessEvaluator.cs` (baru), `PayrollHrdSourceReader.cs`, `PayrollDataReadinessService.cs`; addendum 10: `PayrollDateNormalizer.cs` (baru), `PayrollRunService.cs`, `PayrollProfileProvisioningService.cs`, `PayrollCalculationService.cs`, `PayrollSimulationService.cs`, `PayrollRestitutionService.cs`, `PayrollRunInitializationService.cs`; laporan ini |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-17 |
| Status | 🟡 **Source selesai ditulis dan ditinjau lewat pembacaan; `dotnet build` NOT RUN** (dilarang pada task ini; kode belum pernah dikompilasi). Uji API dan runtime NOT RUN. Checker QBE resmi NOT RUN |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik / prefix | Payroll — `Prl` (registry `ACTIVE`); tidak ada entity atau tabel baru |
| Keberlakuan | `TOUCHED LEGACY` untuk `PayrollCompensationService` (ditulis ulang); `NEW CODE` untuk `PayrollReadinessEvaluator` |
| QBE yang berlaku | `QBE-SVC-001` (akses data hanya di service; controller hanya memanggil service), `QBE-NAM-001`, `QBE-CODE-002` (tidak ada penomoran Count/Max+1) |
| Hak akses | `PayrollCompensation : Read` (daftar dan ringkasan), `ReadDetail`, `ViewAmount` — tidak berubah; tanpa `IsInRole`, nama peran, atau `UserType` |
| Hasil | Tidak ada pelanggaran pada tinjauan source; checker QBE resmi `NOT RUN` |

## 1. Akar masalah

`PayrollCompensationService.GetPagedAsync` memulai dari `PrlPayroll` (`WHERE !IsDelete AND IsActive`), lalu mencari identitas pegawai/dokter hanya untuk `WorkforceProfileId` yang sudah ditemukan di profil. `GetDetailAsync` melempar not found bila `PrlPayroll` tidak ada. Akibatnya orang yang sudah terdaftar di HRD tetapi belum punya profil payroll tidak muncul di daftar dan tidak bisa dibuka detailnya. Itu terbukti dari source.

Kondisi data runtime **tidak diperiksa**: tidak diklaim bahwa `PrlPayroll` memang kosong. Catatan memori sesi sebelumnya (16 November 2026) menyebut 0 profil payroll, tetapi itu data tanggal itu, bukan pemeriksaan hari ini.

Perbedaan dengan Kelengkapan Data Payroll: layar itu sudah berangkat dari populasi HRD (`PayrollHrdSourceReader`), sehingga orang tanpa profil tampil di sana tetapi tidak di Profil Penggajian. Dua layar itu juga menilai kelengkapan dengan kode terpisah.

## 2. Alur bisnis sesudah perbaikan

```
HRD: Pegawai/Dokter aktif (identitas)
  -> HRD: Penetapan gaji utama (berlaku hari ini, tidak masa depan/kedaluwarsa)
  -> HRD: Persetujuan penetapan gaji
  -> HRD: Rekening payroll
  -> HRD: Verifikasi rekening
  -> Profil Penggajian (baca-saja): menampilkan semua orang di atas, profil payroll opsional
  -> Payroll Run dibuka/dihitung -> PayrollProfileProvisioningService menyiapkan PrlPayroll yang belum ada (PAY-BE-24)
  -> Payroll Run: SeedEmployeesAsync hanya memakai PrlPayroll aktif + eligible + Active
  -> Perhitungan gaji
```

Tiga hal dibedakan dan tidak dianggap sama:

| Konsep | Arti | Di mana terlihat |
| --- | --- | --- |
| Terdaftar di HRD | Pegawai/dokter aktif | Baris ada di daftar |
| Punya profil payroll operasional | `PrlPayroll` ada; operasional = aktif + eligible + status `Active` | `HasPayrollProfile`, `PayrollProfileStatus`, `PayrollProfileId` |
| Siap diproses | Sumber HRD lengkap + profil operasional + profil pajak ada + tetapan PPh dan iuran sudah ditetapkan | `IsReady`, `Issues` |

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Services/PayrollCompensationService.cs` | **Ditulis ulang.** Populasi dari `PayrollHrdSourceReader.LoadPopulationAsync(personType, search)` (disaring di database); profil, gaji berlaku, rekening, pajak digabung opsional lewat proyeksi ramping; `GetPagedAsync` (`:100`), `GetSummaryAsync` (`:156`), `GetDetailAsync` (`:170`, memakai `FindPersonAsync` `:571`). Tidak ada tulis ke database; semua pembacaan `AsNoTracking`. Ctor mendapat `PayrollHrdSourceReader` (sudah terdaftar di `Program.cs`) |
| `Services/PayrollReadinessEvaluator.cs` | **Baru.** Aturan murni bersama: `AssessHrd` (`:75`: gaji utama ada/disetujui/konflik, rekening) dan `AssessProfile` (`:131`: Siap / Akan disiapkan otomatis / Menunggu data HRD / Perlu perhatian, status tetapan PPh dan iuran). Tanpa akses data |
| `Services/PayrollHrdSourceReader.cs` | `LoadPopulationAsync(personType, search, ct)` (`:84`) menyaring jenis dan pencarian di database; `LoadSalariesAsync`/`LoadBanksAsync` menerima daftar `WorkforceProfileId` opsional; `PayrollHrdSalary.ApprovedCount` (`:175`) untuk mendeteksi dua gaji utama yang disetujui berlaku bersamaan |
| `Services/PayrollDataReadinessService.cs` | Memakai evaluator yang sama (satu aturan, bukan dua); konstanta kosakata dipindah ke evaluator. Kosakata status, bendera, dan bentuk respons tidak berubah |
| `DTOs/PayrollCompensationDtos.cs` | Additive: `HasPayrollProfile`, `PayrollProfileStatus`, `HrdComplete`, `HrdDataStatus`, `TaxInsuranceStatus`, `Notes`; `PayrollStatus` menjadi nullable; kelas baru `PayrollCompensationSummaryResponse` |
| `Controllers/PayrollCompensationsController.cs` | `GET summary` baru (`:60`) dengan pasangan izin `Read` yang sama dengan daftar |

## 4. Kontrak API

`api/v1/corporate/payroll-management/workforce-compensations` — semua `GET`, murni baca.

| Endpoint | Izin | Isi |
| --- | --- | --- |
| `GET /` | `PayrollCompensation : Read` | Halaman populasi HRD. Filter: `personType` (Karyawan/Dokter), `payrollStatus` (`Active`/`Suspended`/`OnHold`/`Terminated` = profil nyata yang aktif; `NotPrepared` = nilai virtual, orang tanpa profil), `readiness` (`Siap`/`Bermasalah`), `search`, `pageNumber`, `pageSize` (maks. 100) |
| `GET /summary` | `PayrollCompensation : Read` | `totalPeople`, `withPayrollProfile`, `activePayrollProfiles`, `notPrepared`, `hrdComplete`, `ready` — seluruh populasi, tidak dipengaruhi filter daftar |
| `GET /{workforceProfileId}` | `PayrollCompensation : ReadDetail` | Detail; **404 hanya bila identitas HRD tidak ada**, bukan bila profil payroll belum ada. Orang yang sudah tidak aktif di HRD tetap terbuka dengan `HrdDataStatus = "Tidak aktif di HRD"` |
| `GET /{workforceProfileId}/amounts` | `PayrollCompensation : ViewAmount` | Tidak berubah |

Perilaku respons untuk orang tanpa profil: `PayrollProfileId = null`, `HasPayrollProfile = false`, `PayrollStatus = null` (tidak pernah dipalsukan menjadi `Active`), `IsPayrollEligible = false`, `IsReady = false`, `Issues` memuat "Profil Payroll belum disiapkan secara otomatis.". Nominal: `BaseSalary` hanya dari penetapan gaji HRD yang **berlaku hari ini**; `null` (bukan nol) bila tanpa izin `ViewAmount` atau datanya belum ada. `TotalAllowance` `null` untuk profil yang disiapkan otomatis dan bernilai nol (nol itu penanda kompatibilitas, bukan tunjangan). Rekening dan NPWP tetap disamarkan.

## 5. Aturan kelengkapan (satu implementasi untuk dua layar)

| Dimensi | Aturan | Pesan yang muncul hanya bila benar-benar ditemukan |
| --- | --- | --- |
| Sumber HRD | Penetapan gaji utama aktif berlaku hari ini; sudah disetujui; tidak lebih dari satu yang disetujui berlaku bersamaan; rekening payroll aktif berlaku dan terverifikasi | "Belum ada penetapan gaji utama di HRD yang berlaku saat ini." (dipertajam menjadi "baru berlaku mulai …" atau "sudah berakhir pada …" bila datanya ada); "Penetapan gaji utama belum disetujui HRD."; "Terdapat lebih dari satu penetapan gaji utama yang disetujui …"; "Belum ada rekening payroll di HRD."; "Rekening payroll belum diverifikasi HRD." |
| Profil payroll | Ada atau belum; operasional = aktif + eligible + `Active`; profil `Suspended`/`OnHold`/`Terminated` ditampilkan apa adanya, tidak diaktifkan | "Profil Payroll belum disiapkan secara otomatis."; "Profil payroll tidak aktif."; "… tidak eligible."; "Status payroll X, bukan Active." |
| Pajak dan iuran | Profil pajak (`PrlTax`) ada; tetapan PPh dan iuran pada profil bukan sekadar nol bawaan | "Profil pajak belum diisi."; "Tetapan PPh dan iuran pada profil payroll belum ditetapkan (keduanya nol)." |

**Definisi yang sengaja berbeda antar layar** (dijelaskan di UI): *Lengkap* pada Kelengkapan Data = sumber HRD lengkap dan profil sudah Siap atau akan disiapkan otomatis, tetapan PPh dan iuran dilaporkan terpisah. *Siap Diproses* pada Profil Penggajian lebih ketat: profil harus sudah ada dan operasional, profil pajak ada, dan tetapan PPh dan iuran sudah ditetapkan (TC-20: nol bawaan tidak dianggap data sah). Karena itu total *Siap Diproses* boleh lebih kecil daripada *Lengkap*.

## 6. Penyiapan profil otomatis dan Payroll Run

Tidak diubah pada task ini; sudah ada dari `PAY-BE-24` (working tree, belum di-commit): `PayrollProfileProvisioningService.EnsureProfilesAsync` dipanggil di dalam transaksi `Serializable` `PayrollRunService.CreateAsync` dan `CalculateAsync`; hanya MEMBUAT profil yang belum ada (profil lama apa pun statusnya tidak disentuh), hanya bila penetapan gaji berlaku dan rekening payroll ada, tanpa nominal gaji/pajak/iuran karangan, dengan audit `PayrollProfile.AutoProvisioned`; indeks unik `WorkforceProfileId` (`IsDelete = false`) menjamin tidak ada profil ganda. `POST payroll-data-readiness/import` tetap menjawab 410. Tidak ada jalur Payroll Run kedua. Tidak ada tulis pada `GET` mana pun.

## 7. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet restore`, `dotnet build` | Tidak dijalankan | `NOT RUN` — larangan eksplisit perintah. **Belum ada bukti kompilasi.** Risiko terbesar: terjemahan EF Core atas `ToUpper().Contains` dan `Contains` pada koleksi `Guid`, tipe nullable pada tuple |
| QBE conformance | Tidak dijalankan | `NOT RUN` |
| Keseimbangan `{ } ( ) [ ]` pada enam berkas C# | Seimbang | `PASS` (pemeriksaan teks, bukan kompilasi) |
| Tabrakan nama tipe baru (`PayrollProfileFacts`, `PayrollHrdAssessment`, `PayrollProfileAssessment`, `PayrollReadinessEvaluator`, `PayrollCompensationSummaryResponse`) | Tidak ada | `PASS` (pencarian source) |
| Service Profil Penggajian tidak menulis | Tidak ada `SaveChanges`/`Add`/`Update`/`Remove` pada konteks; setiap pembacaan `AsNoTracking` | `PASS` (diperiksa uji frontend yang membaca source backend, 274 uji Payroll lulus) |
| TC-01, TC-02 pegawai/dokter HRD tanpa `PrlPayroll` tetap muncul | Populasi dari `LoadPopulationAsync`; profil opsional | `PASS` (penelusuran source) |
| TC-03 profil aktif menampilkan data existing | `BuildItem` `:348` | `PASS` (penelusuran source) |
| TC-04 detail tanpa profil terbuka | `FindPersonAsync` `:571`; 404 hanya bila identitas HRD tidak ada | `PASS` (penelusuran source) |
| TC-05, TC-06 gaji belum disetujui / rekening belum terverifikasi tampil belum siap | `AssessHrd` `:68` | `PASS` (penelusuran source) |
| TC-07 tanpa `ViewAmount` nominal tidak terlihat | Kontrak controller tidak berubah; `BuildItem` mengisi `null` | `PASS` (penelusuran source) |
| TC-09 filter Active hanya profil nyata aktif | `ApplyStatusFilter` `:332` | `PASS` (penelusuran source) |
| TC-10 filter Bermasalah mencakup penerima tanpa profil | `rows.Where(r => !r.IsReady)`; tanpa profil selalu memiliki isu | `PASS` (penelusuran source) |
| TC-11 `TotalData`/`TotalPage` setelah filter | Dihitung dari `filtered.Count` sebelum paginasi | `PASS` (penelusuran source) |
| TC-12 beberapa penetapan gaji tidak menggandakan orang | Satu orang satu baris menurut `WorkforceProfileId`; satu penetapan dipilih per orang | `PASS` (penelusuran source) |
| TC-13 GET tidak membuat/memperbarui `PrlPayroll` | Lihat baris "tidak menulis" di atas | `PASS` (penelusuran source dan uji teks) |
| TC-14, TC-15, TC-16, TC-17 idempotensi penyiapan, Payroll Run, status Suspended/OnHold/Terminated, riwayat terkunci | Milik `PAY-BE-24`, tidak diubah di sini | `PASS` hanya sejauh `PAY-BE-24`; tidak diverifikasi ulang di task ini |
| TC-18 tanpa impor CSV | `import` tetap 410; Profil Penggajian tidak punya jalur impor | `PASS` (penelusuran source) |
| TC-20 nol bawaan tidak dianggap siap | `Evaluate` `:327` | `PASS` (penelusuran source) |
| Eksekusi nyata (API, database, EF) | Tidak dijalankan | `NOT RUN` — **tidak ada TC yang berstatus PASS runtime** |

## 8. Keputusan desain, risiko, dan hal yang belum selesai

| Hal | Isi |
| --- | --- |
| Paginasi tidak sepenuhnya di database | Penyaringan identitas (jenis, pencarian) di database. Filter status dan kesiapan serta urutan dan paginasi dikerjakan di memori atas proyeksi ramping (beberapa kolom per orang), karena kesiapan menggabungkan lima tabel dan harus identik dengan Kelengkapan Data. Jumlahnya dibatasi jumlah SDM rumah sakit; hanya baris satu halaman yang memuat detail tambahan. Memindahkannya ke SQL penuh tidak dapat diverifikasi tanpa build, sehingga tidak dikerjakan |
| Tidak ada migration | Tidak ada kolom, tabel, atau indeks baru |
| `PayrollStatus` nullable | Perubahan kontrak: sebelumnya string tidak-null. Hanya frontend Payroll yang mengonsumsinya; sudah disesuaikan |
| `HasPayrollProfile` berbeda arti di dua layar | Pada Profil Penggajian = profil ada (apa pun statusnya); pada Kelengkapan Data = profil operasional. Didokumentasikan pada kedua DTO |
| Filter `Active` dan layar Simulasi | Layar Simulasi mengirim `payrollStatus=Active`; hasilnya tetap hanya profil nyata yang aktif, jadi pemilih pegawai Simulasi tidak berubah |
| Blocker terbuka dari `PAY-BE-24` | Tetapan PPh dan iuran per orang tidak punya sumber sah di HRD dan rumus PPh/BPJS belum disahkan; profil yang disiapkan otomatis memulai dari nol. Layar menandainya "Belum ditetapkan" dan menahan *Siap Diproses*. Butuh keputusan pemilik sebelum run dibayarkan |
| Permission | Tidak berubah. `summary` memakai pasangan `Read` yang sama dengan daftar, tanpa nominal |
| Pekerjaan lain di working tree | `PAY-BE-24` (provisioning, reader, perubahan `PayrollRunService`/`PayrollCalculationService`) belum di-commit dan ikut tergantung pada `dotnet build` yang sama |
| Langkah berikutnya | (1) Pemilik menjalankan `dotnet build`; (2) uji API: daftar tanpa filter menampilkan seluruh populasi, filter `NotPrepared`/`Active`/`Siap`/`Bermasalah`, detail orang tanpa profil, `summary`; (3) uji manual layar; (4) bila diinginkan, amandemen kontrak `PGJ-C1` dan pendaftaran roadmap lewat alur resmi |

---

## 9. Addendum 2026-11-17 (2) — Detail Profil Penggajian tanpa `PrlPayroll` ("Profil penggajian penerima tidak ditemukan")

Master prompt lanjutan untuk ID `05d2a963-fd58-465b-87c8-7209d0806c42`. Larangan tetap: tanpa `dotnet build`, tanpa migration, tanpa eksekusi database, tanpa commit.

### 9.1 Akar masalah dan kondisi ID yang diberikan

| Hal | Temuan |
| --- | --- |
| Penyebab struktural | Pada snapshot GitHub (`dbac9acc`), `GetDetailAsync` mencari `PrlPayroll` (`!IsDelete && IsActive`) sebelum membaca identitas HRD dan melempar `KeyNotFoundException("Profil penggajian penerima tidak ditemukan.")`. Teks itu sama persis dengan alert pada screenshot |
| Kondisi working tree | Teks itu **sudah tidak ada di seluruh working tree** (dicari di `*.cs`, `*.js`, `*.jsx`, `*.md` kedua repository sebelum laporan ini diperbarui). `GetDetailAsync` sudah membaca HRD lebih dulu sejak bagian 3 laporan ini. Jadi screenshot hampir pasti berasal dari backend yang berjalan dengan kode lama (perubahan belum di-commit dan belum dibuild). **Ini kesimpulan dari source, bukan dari pemeriksaan proses yang berjalan** |
| Kondisi data ID tersebut | **Tidak diperiksa.** Agent tidak menyambung ke database (`CLAUDE.md` mewajibkan konfirmasi terpisah untuk eksekusi database). Tidak diketahui apakah `PrlPayroll` ada, nonaktif, atau tidak ada; tidak diketahui pula apakah ID itu benar `WorkforceProfileId`. SQL baca-saja untuk memeriksanya ada di bagian 9.6 |

### 9.2 Yang sudah benar sebelum addendum (tidak ditimpa)

Identitas HRD dibaca dulu (`FindPersonAsync`), profil payroll opsional, `404` hanya untuk identitas HRD yang tidak ada, `GET` tidak menulis, izin `ReadDetail`/`ViewAmount` dan masking rekening/NPWP tidak berubah, daftar dan detail memakai `WorkforceProfileId` yang sama (`openDetail` mengirim `item.workforceProfileId`).

### 9.3 Celah terhadap master prompt yang ditutup pada addendum ini

| Butir | Perubahan |
| --- | --- |
| BE-DETAIL-002 identitas | `FindPersonAsync` (`:571`) kini juga memeriksa `MstWorkforceProfile` (ada dan belum dihapus) sebelum pegawai/dokter. Id tabel lain (profil payroll, pegawai, dokter, penetapan gaji) tidak diterima sebagai pengganti. Pesan 404 kini "Data pegawai atau dokter tidak ditemukan di HRD." (`:178`) |
| BE-DETAIL-003 pemilihan profil | `PayrollReadinessEvaluator.SelectProfile` (`:106`) dipakai oleh Profil Penggajian **dan** Kelengkapan Data: profil aktif yang sedang berlaku didahulukan, lalu profil aktif lain (masa depan/berakhir), lalu nonaktif; tanggal mulai terbaru bila sama. Profil nonaktif tetap dipilih bila hanya itu yang ada (beda dari "belum dibuat"). Lebih dari satu profil aktif yang berlaku bersamaan dilaporkan sebagai masalah integritas data |
| BE-DETAIL-003 masa berlaku | `AssessProfile` (`:131`) menerima tanggal penilaian: profil yang baru berlaku di masa depan atau sudah berakhir **tidak operasional** dan tidak pernah disebut "Siap"/"Aktif". Berlaku juga pada Kelengkapan Data |
| BE-DETAIL-005 DTO | Additive: `OperationalProfileReady` (profil ada **dan** operasional hari ini), `SalaryStatus`; pada detail: `PayrollNumber`, `PaymentFrequency`, `PayrollProfileEffectiveStartDate/EndDate`, `OrganizationUnitName`, `DepartmentName`, `PositionName`. Nama `HrdComplete`/`HrdDataStatus` dipertahankan (setara `HrdDataComplete` di prompt) |
| BE-DETAIL-006 status gaji | `SalaryStatus` eksplisit: "Belum Ada Penetapan Gaji", "Menunggu Persetujuan HRD", "Disetujui", "Belum Berlaku", "Sudah Berakhir", "Konflik Penetapan Gaji" (`ClassifyMissingSalary` `:432`, `BuildItem` `:348`). Penetapan masa depan tidak pernah dianggap berlaku |
| BE-DETAIL-004 mapping | Informasi operasional diisi hanya dari profil yang benar-benar ada; null (bukan nilai bawaan) bila profil belum dibuat |

### 9.4 Yang sengaja tidak diubah, dengan alasan

| Butir | Alasan |
| --- | --- |
| BE-DETAIL-007 rekening | Sudah sesuai: semua rekening aktif ditampilkan dengan nomor disamarkan, status verifikasi dari HRD; kosong menjadi array kosong. Tidak ada verifikasi otomatis |
| BE-DETAIL-008 pajak dan iuran | Sudah sesuai: `PrlTax`/`PrlInsurance`, `HasTaxProfile`/`HasInsuranceProfile` false bila tidak ada, nol bukan bukti profil sah (`TaxInsuranceStatus`) |
| BE-DETAIL-009 kondisi A–E | A, B, C, D dan E ditangani oleh `Evaluate` (`:302`) dan `AssessProfile`; kondisi E (Suspended/OnHold/Terminated) menampilkan status asli dan tidak diaktifkan |
| BE-DETAIL-011/012 izin dan read-only | `ReadDetail` dan `ViewAmount` tidak berubah; service tetap tanpa tulis dan seluruh pembacaan `AsNoTracking` |
| Transportasi | `WfpTransportAllowanceController` hanya memeriksa keberadaan `MstWorkforceProfile`, tidak `PrlPayroll`, sehingga tombolnya tidak perlu bergantung pada profil payroll |

### 9.5 Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` — kode belum pernah dikompilasi. Risiko: terjemahan EF atas proyeksi navigasi `x.OrganizationUnit != null ? … : null`, tuple bernama pada `ToDictionary`, `out var` di dalam lambda |
| Keseimbangan `{ } ( )` pada empat berkas C# yang diubah | Seimbang | `PASS` (teks, bukan kompilasi) |
| Seluruh pembacaan `_dbContext` memakai `AsNoTracking` (10 dari 10) | Terpenuhi | `PASS` (uji teks frontend) |
| TC-01..TC-21 master prompt | Ditelusuri pada source; tidak ada yang dijalankan | `PASS` hanya sebagai penelusuran source; **tidak ada PASS runtime** |
| TC-04 (ID tidak ada di HRD → 404), TC-15 (GET tidak membuat `PrlPayroll`), TC-17 (status tidak diaktifkan), TC-19 (nominal tidak menjadi Rp0) | Masing-masing ditangani di `FindPersonAsync`, tanpa tulis, `AssessProfile`, `BuildItem` | `PASS` (penelusuran source) |

### 9.6 SQL baca-saja untuk memeriksa ID pada screenshot (dijalankan pemilik, tidak oleh agent)

Hanya `SELECT`. Ganti nilai bila perlu.

```sql
-- 1) Apakah ID ini WorkforceProfileId milik pegawai/dokter? (yang dibaca halaman detail)
SELECT 'MstWorkforceProfile' AS sumber, w."Id", w."IsDelete", w."IsActive", NULL AS nomor, w."DisplayName" AS nama
FROM public."MstWorkforceProfile" w WHERE w."Id" = '05d2a963-fd58-465b-87c8-7209d0806c42'
UNION ALL
SELECT 'MstEmployee', e."WorkforceProfileId", e."IsDelete", e."IsActive", e."EmployeeNumber", e."FullName"
FROM public."MstEmployee" e WHERE e."WorkforceProfileId" = '05d2a963-fd58-465b-87c8-7209d0806c42'
UNION ALL
SELECT 'MstDoctor', d."WorkforceProfileId", d."IsDelete", d."IsActive", d."DoctorNumber", d."FullName"
FROM public."MstDoctor" d WHERE d."WorkforceProfileId" = '05d2a963-fd58-465b-87c8-7209d0806c42';

-- 2) Bila (1) kosong: mungkinkah ID itu milik tabel lain (salah identifier)?
SELECT 'PrlPayroll.Id' AS tabel, p."Id", p."WorkforceProfileId" FROM public."PrlPayroll" p WHERE p."Id" = '05d2a963-fd58-465b-87c8-7209d0806c42'
UNION ALL SELECT 'MstEmployee.Id', e."Id", e."WorkforceProfileId" FROM public."MstEmployee" e WHERE e."Id" = '05d2a963-fd58-465b-87c8-7209d0806c42'
UNION ALL SELECT 'MstDoctor.Id', d."Id", d."WorkforceProfileId" FROM public."MstDoctor" d WHERE d."Id" = '05d2a963-fd58-465b-87c8-7209d0806c42'
UNION ALL SELECT 'WfpSalaryAssignment.Id', s."Id", s."WorkforceProfileId" FROM public."WfpSalaryAssignment" s WHERE s."Id" = '05d2a963-fd58-465b-87c8-7209d0806c42';

-- 3) Profil payroll, penetapan gaji, dan rekening untuk WorkforceProfileId itu
SELECT "Id", "PayrollStatus", "IsActive", "IsPayrollEligible", "IsDelete", "EffectiveStartDate", "EffectiveEndDate"
FROM public."PrlPayroll" WHERE "WorkforceProfileId" = '05d2a963-fd58-465b-87c8-7209d0806c42';

SELECT "Id", "IsPrimary", "IsActive", "IsDelete", "EffectiveStartDate", "EffectiveEndDate", "ApprovedAt"
FROM public."WfpSalaryAssignment" WHERE "WorkforceProfileId" = '05d2a963-fd58-465b-87c8-7209d0806c42' ORDER BY "EffectiveStartDate" DESC;

SELECT "Id", "IsPayrollAccount", "IsPrimary", "IsVerified", "IsActive", "IsDelete"
FROM public."WfpBankAccount" WHERE "WorkforceProfileId" = '05d2a963-fd58-465b-87c8-7209d0806c42';
```

Cara membaca: baris (1) berisi `MstEmployee` atau `MstDoctor` dengan `IsDelete = false` → detail akan terbuka setelah backend baru dibuild, dengan atau tanpa profil payroll. Hasil (1) kosong tetapi (2) berisi baris → URL memakai identifier yang salah; seharusnya `WorkforceProfileId` pada kolom ketiga. Keduanya kosong → identitas memang tidak ada di HRD dan 404 "Data pegawai atau dokter tidak ditemukan di HRD." adalah jawaban yang benar.

### 9.7 Risiko dan langkah berikutnya

1. `dotnet build` backend, lalu jalankan ulang backend; **tanpa itu screenshot yang sama akan muncul lagi** karena yang berjalan masih kode lama.
2. Uji API: detail untuk penerima dengan profil, tanpa profil, profil nonaktif, ID tidak ada (harus 404 bertuliskan teks produk), tanpa izin `ReadDetail` (403), tanpa `ViewAmount` (nominal `null`).
3. Perilaku baru pada Kelengkapan Data: profil yang belum/tidak lagi berlaku tampil "Perlu perhatian" (sebelumnya "Siap" bila status Active). Itu perubahan perilaku yang disengaja agar sama dengan Profil Penggajian.

---

## 10. Addendum 2026-11-17 (3) — `POST payroll-runs` menjawab 400 "Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'"

### 10.1 Gejala

`POST /api/v1/corporate/payroll-management/payroll-runs` (tombol buka Payroll Run di `/payroll/payroll-runs`) menjawab `400` dengan `message`: *Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported. … (Parameter 'value')*. Badan permintaan hanya `PayrollPeriodId` dan `Notes`, jadi nilai DateTime itu dibuat di service, bukan dikirim layar.

Pemetaan 400: Npgsql melempar `ArgumentException`; `PayrollControllerSupport.Failure` memetakan `ArgumentException` ke 400 dan meneruskan pesan mentahnya. Transaksi `InTransactionAsync` di-rollback saat galat (pola `RollbackAsync` pada blok catch), sehingga kegagalan ini tidak meninggalkan run atau profil payroll setengah jadi. Kesimpulan rollback berasal dari pembacaan source; isi database tidak diperiksa.

### 10.2 Akar masalah

| Fakta | Bukti source |
| --- | --- |
| Lima tanggal periode berkolom `date` | `MstPayrollPeriodConfiguration`: `StartDate`, `EndDate`, `AttendanceCutoffDate`, `VariableInputCutoffDate`, `PaymentDate` → `HasColumnType("date")` |
| Npgsql membaca kolom `date` sebagai `DateTime` ber-`Kind=Unspecified` | perilaku Npgsql; model `MstPayrollPeriod` bertipe `DateTime` |
| Lima kolom snapshot run berjenis `timestamp with time zone` | `PrlPayrollRunConfiguration`: `PeriodStartDateSnapshot`, `PeriodEndDateSnapshot`, `AttendanceCutoffDateSnapshot`, `VariableInputCutoffDateSnapshot`, `PaymentDateSnapshot` |
| Kolom berlaku profil dan tunjangan transportasi juga `timestamptz` | `PrlPayrollConfiguration`, `PrlTransportAllowanceConfiguration`: `EffectiveStartDate`, `EffectiveEndDate` |
| Saklar `Npgsql.EnableLegacyTimestampBehavior` tidak diaktifkan di kode | pencarian pada seluruh `*.cs` backend: nol hasil (`csproj`, `appsettings`, dan `runtimeconfig` tidak diperiksa); pesan galat sendiri menunjukkan perilaku ketat sedang berlaku |

`PayrollRunService.CreateAsync` menyalin `period.StartDate` dst. langsung ke snapshot (tulis ke `timestamptz`), dan `SeedEmployeesAsync` membandingkan `PrlPayroll.EffectiveStartDate/EndDate` (`timestamptz`) dengan `period.EndDate/StartDate` mentah. Keduanya membawa `Kind=Unspecified` ke Npgsql. Dari pesan galat saja tidak dapat dibedakan titik mana yang melempar lebih dulu (kueri seed berjalan sebelum `SaveChanges` snapshot); kedua titik diperbaiki.

Cacat yang sama ada di jalur berikutnya yang akan dialami pengguna sesudah run terbuka, atau pada jalur lain:

| Titik | Cacat | Dampak sebelum perbaikan |
| --- | --- | --- |
| `PayrollRunService.CreateAsync` | 5 snapshot ditulis mentah | `POST payroll-runs` 400 (laporan ini) |
| `PayrollRunService.SeedEmployeesAsync` | pembanding `PrlPayroll` mentah | sama; juga dipakai saat Hitung |
| `PayrollCalculationService.CalculateRunAsync` | pembanding `PrlTransportAllowance` mentah | `POST …/{id}/calculate` akan gagal setelah Create diperbaiki |
| `PayrollSimulationService` | pembanding `PrlPayroll` mentah + 5 snapshot pada run simulasi | `POST …/simulations` gagal |
| `PayrollRestitutionService` | 5 snapshot pada run penampung restitusi | pembuatan run penampung restitusi gagal |
| `PayrollRunInitializationService` (layanan lama, rute `…/human-resource/payroll/runs`) | 5 snapshot | pembuatan run lewat rute lama gagal |

Bukan penyebab: badan permintaan, frontend, `PayrollProfileProvisioningService` (tanggal berlaku profil sudah dicap UTC lewat `DateTime.SpecifyKind`), sinkron angsuran Finance (hanya `DateTime.UtcNow`, `DateTimeOffset.UtcNow`, `DateOnly`).

### 10.3 Perbaikan

| Berkas | Perubahan |
| --- | --- |
| `Services/PayrollDateNormalizer.cs` (baru) | `ToUtcDate(DateTime)` dan `ToUtcDate(DateTime?)` = `DateTime.SpecifyKind(value.Date, DateTimeKind.Utc)` |
| `Services/PayrollRunService.cs` | `CreateAsync`: `periodStart`/`periodEnd` dinormalkan, dipakai untuk penyiapan profil dan dua snapshot; tiga snapshot lain lewat `ToUtcDate`. `SeedEmployeesAsync`: pembanding memakai `periodStart`/`periodEnd` UTC |
| `Services/PayrollProfileProvisioningService.cs` | `EnsureProfilesAsync` menormalkan `periodStart`/`periodEnd` sebelum dipakai (jalur Create dan Calculate) |
| `Services/PayrollCalculationService.cs` | `periodStart`/`periodEnd` UTC; kueri `WfpSalaryAssignment` dan `PrlTransportAllowance` memakainya |
| `Services/PayrollSimulationService.cs` | pembanding `PrlPayroll` dan lima snapshot run simulasi |
| `Services/PayrollRestitutionService.cs` | lima snapshot run penampung |
| `Services/PayrollRunInitializationService.cs` | lima snapshot; memakai alias `using` karena namespace-nya `…HumanResource.PayrollManagement.Services` |

**Konvensi yang dipilih: tanggal kalender dicap UTC tengah malam** (`2026-11-01` → `2026-11-01T00:00:00Z`), **bukan** `AppDateTimeHelper.ToUtc`. Alasannya: pembaca hilir membaca snapshot sebagai tanggal kalender (`PayrollFinanceHandoffService` memakai `.Month`/`.Year`; `PayrollHrdImportService` memakai `DateOnly.FromDateTime`; `PayrollPayslipDistributionService` memakai `.Year`/`.Month`). Konversi dari WIB akan menghasilkan `2026-10-31T17:00:00Z`, lalu bulan periode dan slip gaji bergeser satu bulan. Konvensi yang sama sudah dipakai kode Payroll lain (`FinanceDeductionSyncService`, `PayrollProfileProvisioningService`).

### 10.4 Yang sengaja tidak diubah

- **Tidak ada perubahan tipe kolom, migration, atau `ValueConverter` EF** pada `MstPayrollPeriod`. Migration dilarang pada task ini, dan converter berefek global ke seluruh modul master data periode serta tidak dapat diverifikasi terjemahan kueri-nya tanpa build.
- `PayrollRestitutionService` (pembandingan `x.StartDate > period.StartDate`) dan `PayrollRunInitializationService` (pembandingan `WfpSalaryAssignment`) membandingkan kolom `date` dengan nilai `date`: aman, tidak disentuh.
- **Temuan di luar alur ini, belum diperbaiki:** `PayrollDisbursementReconciliationService` menetapkan `run.PaymentDateSnapshot = command.ActualDisbursedAt` dari badan permintaan. Bila klien mengirim waktu tanpa zona, cacat yang sama muncul di sana. Perlu keputusan pemilik apakah mau dinormalkan di tepi masukan.
- **Temuan di luar alur ini:** pesan mentah exception Npgsql diteruskan ke klien sebagai `message` (lewat pemetaan `ArgumentException` → 400). Tidak diubah karena menyentuh perilaku seluruh controller Payroll.

### 10.5 Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` — dilarang pada task ini; kode belum dikompilasi. Risiko rendah: hanya pemanggilan metode statis dan variabel lokal; satu alias `using` pada berkas ber-namespace berbeda |
| Keseimbangan `{ } ( ) [ ]` pada 7 berkas C# | Seimbang | `PASS` (teks, bukan kompilasi) |
| Akhir baris CRLF 7 berkas | Utuh (jumlah CR = jumlah baris) | `PASS` |
| Uji kontrak frontend `payroll-run-utc-dates.test.mjs` (membaca source backend) | 11 dari 11 lulus | `PASS` — mengunci: helper memakai `SpecifyKind(...Utc)` dan bukan konversi zona; tidak ada snapshot `period.*` mentah pada empat layanan; kelima snapshot `CreateAsync` dinormalkan; tidak ada pembandingan `Effective*Date` mentah pada tiga layanan; `EnsureProfilesAsync` menormalkan sebelum membaca HRD; alias `using` pada layanan lama |
| Uji ini dapat gagal | Dibuktikan dengan mencocokkan pola ke potongan kode sebelum perbaikan: 5 snapshot mentah dan 1 pembandingan mentah terdeteksi; sesudah perbaikan 0 | `PASS` |
| Seluruh uji Payroll frontend | 294 dari 294 lulus (sebelumnya 283), 0 dilewati | `PASS` |
| Pembuatan run, hitung, simulasi pada database nyata | Tidak dijalankan | `NOT RUN` — **belum ada bukti runtime bahwa 400 ini hilang** |
| Pembacaan atau perubahan database | Tidak ada | — |

### 10.6 Langkah berikutnya (pemilik)

1. `dotnet build` backend, jalankan ulang backend, lalu buka run untuk periode yang tadi gagal. Selama yang berjalan masih kode lama, 400 yang sama akan muncul lagi.
2. Setelah run terbuka, periksa penyimpanan snapshot (hanya `SELECT`):

```sql
SELECT "RunNumber", "PeriodStartDateSnapshot", "PeriodEndDateSnapshot", "PaymentDateSnapshot"
FROM public."PrlPayrollRun" ORDER BY "CreateDateTime" DESC LIMIT 3;
```

   Nilai yang benar berjam `00:00:00+00` (UTC tengah malam) dan tanggalnya sama dengan tanggal periode.
3. Uji berurutan di layar: buka run → Impor HRD → Hitung → Simulasi satu periode. Hitung dan Simulasi memakai titik yang sama dan sebelumnya akan gagal setelah Create diperbaiki.
4. Putuskan penanganan `PayrollDisbursementReconciliationService` (10.4) dan pesan galat mentah.
