# Laporan Perubahan Backend — `PAY-BE-FIX-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-016` (ad-hoc lanjutan seri `PAY-BE-FIX`) |
| Judul | Opening Tax Balance: entity, konfigurasi EF, service, controller, dan sumber saldo awal untuk mesin PPh |
| Slice | Corporate / Payroll Management (`Prl`) |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc (`NOT APPLICABLE`) |
| Trace | Rancangan dan DDL pada `PAY-BE-FIX-015` bagian 9 dan 9.1; persetujuan pemilik 23 Nov 2026 ("lanjutkan implementasi service dan controller-nya") |
| Dependency | **DDL `PAY-BE-FIX-015` bagian 9.1 diterapkan pemilik.** Tanpa DDL, backend tetap aman (lihat bagian 4), tetapi fitur dinyatakan tidak tersedia |
| Klasifikasi | `HEAVY` — skor 11 (satu repository: 0; berkas diperiksa >20: 2; berkas dibuat/diubah 9: 2; logika kompleks: 2; kontrak API baru: 2; dampak schema/entity: 2; keamanan berkaitan: 1; UI: 0) |
| Task mode | `BACKEND` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `ab340bc7de4e9a041ca3a8e0289dc910f417ec82` (branch `Yasmina`); working tree juga memuat `PAY-BE-FIX-015` |
| Tanggal | 23 November 2026 |
| Status | 🟡 **Source selesai; `dotnet build` `NOT RUN`; tidak ada eksekusi runtime dan tidak ada database yang disentuh.** Layar frontend belum dibuat |

**Backend Governance Preflight.** Area `Corporate`; Module `PayrollManagement`; pemilik/prefix `Prl` terdaftar `ACTIVE` (tidak ada penghalang QBE-MOD-002); `NEW CODE`. QBE yang berlaku: QBE-ENT-001 (entity mewarisi `IdentityModel`), QBE-NAM-002 (prefix `Prl`), QBE-CFG-001 (`IEntityTypeConfiguration`), QBE-SVC-001 (controller → service → DbContext; controller tidak mengakses `ApplicationDbContext`), QBE-API-001, QBE-PERM-001, QBE-DTO-001 (entity EF tidak dikembalikan), QBE-VAL-001, QBE-LOG-001 (audit beraktor), QBE-DEL-001 (soft delete). QBE-CODE (nomor bisnis) tidak berlaku: tidak ada kode bisnis.

---

## 1. Yang dibangun

| Berkas | Isi |
| --- | --- |
| `Models/PrlOpeningTaxBalance.cs` | Entity (`IdentityModel`), `CoveredMonths` sebagai `List<int>` ↔ `integer[]` |
| `Repositories/Configurations/.../PrlOpeningTaxBalanceConfiguration.cs` | Konfigurasi EF dengan **nama dan ekspresi constraint/indeks persis seperti DDL** (10 CHECK termasuk `CK_PrlOpeningTaxBalance_RejectedHasDecision`, 2 indeks unik tersaring, 1 indeks, 1 FK bernama; entity memuat `RejectedAt` dan `RejectedByUserId`) |
| `Repositories/ApplicationDbContext.cs` | `DbSet<PrlOpeningTaxBalance> PrlOpeningTaxBalances` |
| `DTOs/PayrollOpeningTaxBalanceDtos.cs` | Request, query, respons, aksi, metadata |
| `Services/PayrollOpeningTaxBalanceService.cs` | Seluruh aturan bisnis (bagian 2) |
| `Services/PayrollOpeningTaxBalanceStoreSource.cs` | Pemeriksa keberadaan tabel dan sumber saldo awal untuk mesin hitung |
| `Controllers/PayrollOpeningTaxBalancesController.cs` | Controller tipis |
| `Program.cs` | `IPayrollOpeningTaxBalanceSource` → `PayrollOpeningTaxBalanceStoreSource`; service didaftarkan |
| `Services/PayrollCalculationService.cs` | Satu perubahan: `AccumulatedRunStatuses` menjadi `internal` agar daftar status "run final" tidak disalin |

**Tidak diubah:** `BuildYtd` (sudah menerapkan aturan pemakaian: hanya `APPROVED`, tahun sama, sumber `SAME_EMPLOYER_PRE_CUTOVER`, irisan ditolak), `PayrollTaxPolicyEngine`, payroll run, snapshot, Finance. Tidak ada migration dan tidak ada perubahan `ModelSnapshot`.

## 2. Proses bisnis

**Pelaku.** HRD/Pajak (menyusun dan mengajukan), pihak berwenang yang **berbeda** (menyetujui atau menolak).

**Alur.**

1. Petugas menyusun **draf**: pegawai, tahun pajak, bulan yang dicakup, sumber, empat nominal, dokumen sumber.
2. Petugas **mengajukan** (dokumen sumber wajib sudah terisi).
3. Pengguna lain dengan izin `Approve` **dan** `ViewAmount` menyetujui atau menolak (penolakan wajib beralasan).
4. Saat **disetujui**: sistem memeriksa bulan yang dicakup tidak beririsan dengan payroll run final pegawai itu dan dengan saldo `APPROVED` lain (sumber berbeda); versi `APPROVED` sebelumnya dari kunci yang sama otomatis menjadi `SUPERSEDED`.
5. Mulai payroll berikutnya, mesin membaca saldo `APPROVED` dari pemberi kerja yang sama untuk rekonsiliasi masa pajak terakhir.
6. Mengubah saldo yang sudah disetujui = **revisi** (draf versi baru). Versi lama tidak berubah sampai versi baru disetujui.

**Perpindahan status.**

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| — | Buat | `DRAFT` | `Create` + `ViewAmount` | Isian valid; tidak ada rantai terbuka untuk kunci yang sama |
| `DRAFT` | Ubah | `DRAFT` | `Update` + `ViewAmount` | Kunci (pegawai, tahun, sumber) tidak berubah |
| `DRAFT` | Ajukan | `PENDING_APPROVAL` | `Submit` | Dokumen sumber terisi |
| `PENDING_APPROVAL` | Setujui | `APPROVED` | `Approve` + `ViewAmount`, **bukan pengaju** | Tidak beririsan (lihat langkah 4) |
| `PENDING_APPROVAL` | Tolak | `REJECTED` | `Reject` + `ViewAmount`; **pengaju yang sama boleh menolak** (dibedakan lewat izin role) | Alasan wajib; `RejectedAt`/`RejectedByUserId` dicatat |
| `APPROVED` / `REJECTED` | Revisi | `DRAFT` (versi +1) | `Create` + `ViewAmount` | Tidak ada draf/pengajuan terbuka |
| `APPROVED` | Versi baru disetujui | `SUPERSEDED` | otomatis | — |
| `DRAFT` / `REJECTED` | Hapus | soft delete | `Delete` | Yang lain adalah riwayat dan tidak dihapus |

**Contoh.** Pegawai bekerja sejak Januari 2026; Quilvian dipakai mulai November. HRD menyusun saldo: tahun 2026, bulan 1–10, sumber `SAME_EMPLOYER_PRE_CUTOVER`, penghasilan Rp100.000.000, PPh Rp2.000.000, dokumen "Rekap slip Jan–Okt, arsip HRD no. 12". Setelah disetujui orang lain, Desember 2026 memakai Januari–Oktober dari saldo ini dan November dari payroll run. Bila saldo itu mencantumkan bulan 11 padahal run November sudah disahkan, persetujuan ditolak (409): November akan terhitung dua kali.

**Jalur tidak normal.**

| Kejadian | Hasil |
| --- | --- |
| Pengaju mencoba menyetujui sendiri | 403 |
| Tanpa izin `ViewAmount` | Nominal pada respons `null` (bukan nol); tidak dapat mengisi, menyetujui, atau menolak (403) |
| Bulan beririsan dengan run final atau saldo lain | 409 dengan daftar bulan |
| Kunci sama sudah punya draf/pengajuan/persetujuan | 409: gunakan ubah atau revisi |
| Mengubah pegawai, tahun, atau sumber pada draf | 422: hapus dan buat baru |
| Sumber `PREVIOUS_EMPLOYER` | Boleh disimpan dan disetujui sebagai arsip, tetapi **tidak dipakai mesin** (respons `isUsedByEngine = false` dengan keterangan) |
| Tabel belum ada di database | 409 dengan pesan jelas; payroll tetap berjalan (bagian 4) |

## 3. Kontrak API

#### Corporate / Payroll Management / Opening Tax Balance

Base URL: `api/v1/corporate/payroll-management/opening-tax-balances`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Kosakata status/sumber dan `storageAvailable` | `PayrollOpeningTaxBalance : Read` |
| `GET` | `/` | Daftar berhalaman (`pegawai`, `taxYear`, `status`, `employerSource`) | `Read` |
| `GET` | `/{id}` | Detail dan aksi yang boleh | `Read` |
| `GET` | `/{id}/amount` | Pintu izin nominal agar kemampuan muncul di Akses Role | `ViewAmount` |
| `POST` | `/` | Menyusun draf | `Create` |
| `PUT` | `/{id}` | Mengubah draf | `Update` |
| `POST` | `/{id}/revise` | Draf versi baru dari yang disetujui/ditolak | `Create` |
| `POST` | `/{id}/submit` | Mengajukan | `Submit` |
| `POST` | `/{id}/approve` | Menyetujui (body opsional `note`) | `Approve` |
| `POST` | `/{id}/reject` | Menolak (body `note` wajib) | `Reject` |
| `DELETE` | `/{id}` | Soft delete draf/ditolak | `Delete` |

Bentuk standar: `ApiResponse<T>`; daftar memakai `PagedResult<T>`. Kode status mengikuti `PayrollControllerSupport`: 400 aksi tidak sah untuk status, 403 tidak berwenang, 404 tidak ada, 409 bentrok, 422 data tidak valid. Tidak ada `GET /options` dan `PATCH status` generik (arketipe master ber-lifecycle; saldo awal bukan isi dropdown). **Izin baru wajib dicentang admin di Akses Role** (kemampuan muncul otomatis setelah backend berjalan).

## 4. Keselamatan rilis

Urutan sah: **DDL → backend → frontend**. Urutan terbalik tidak merusak payroll: `PayrollOpeningTaxBalanceStorage` memeriksa tabel lewat `to_regclass` (tidak melempar galat) sebelum membaca. Ini penting karena pada PostgreSQL query ke tabel yang tidak ada **membatalkan seluruh transaksi** payroll yang sedang berjalan, dan tidak bisa dipulihkan dengan `try/catch`. Bila tabel belum ada, sumber menyatakan dirinya **tidak tersedia** (sama seperti perilaku sebelumnya), bukan saldo nol; `Opening Tax Balance` pada rincian PPh tetap menyatakan fitur belum tersedia. Hasil "ada" disimpan di memori proses; hasil "tidak ada" diperiksa ulang pada pemanggilan berikutnya, jadi tidak perlu restart setelah DDL diterapkan.

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` (dilarang). **Kompilasi belum dibuktikan** |
| `payroll-opening-tax-balance.test.mjs` | **6 lulus, 0 gagal** (dijalankan ulang 23 Nov 2026 bersama `payroll-opening-tax-balance-screen.test.mjs`: 14 lulus, 0 gagal) | `PASS` — mencocokkan **teks** source dan DDL: 14 nama constraint/indeks pada konfigurasi EF = DDL dan ≤63 karakter; kosakata status/sumber = CHECK; 11 pasangan `AccessAction`/`AccessPermission` cocok; tidak ada `PATCH`/bypass/`IsInRole`; controller tidak memakai `ApplicationDbContext`; maker-checker, `ViewAmount`, irisan, dua langkah simpan saat penggantian, log tanpa nominal, pemeriksaan `to_regclass`, registrasi DI |
| Test tetangga (`payroll-tax-history-context` 12, `payroll-tax-pph21-ytd-engine` 20, `payroll-simulation` 19) | 0 gagal | `PASS` |
| Eksekusi C#, SQL hasil EF, constraint pada database nyata | Tidak dilakukan | `NOT RUN` |
| Akhir baris | Berkas baru LF; yang diedit tetap CRLF | `PASS` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`. Test Node di atas ada di repository frontend karena pola repo ini sudah demikian; ia bukan bukti eksekusi backend.

**Titik paling berisiko tanpa build:** (1) `Database.SqlQuery<bool>` dengan kolom `Value`; (2) pemetaan `List<int>` ke `integer[]` dan `HasCheckConstraint` dengan operator `<@`; (3) terjemahan `ids.Contains` dan filter status pada query EF; (4) metode grup `Where(runMonths.Contains)`. Perlu dilihat pada `dotnet build` dan uji pertama.

## 6. Acceptance dan pekerjaan berikutnya

| Kriteria | Status |
| --- | --- |
| Entity, konfigurasi, service, controller mengikuti governance dan DDL | Terpenuhi pada source |
| Hanya `APPROVED` yang dipakai; penyetuju ≠ pengaju; nominal mengikuti `ViewAmount` | Terpenuhi pada source |
| Irisan dengan run final dan saldo lain ditolak; tidak ada hitung ganda | Terpenuhi pada source |
| Aman bila DDL belum diterapkan | Terpenuhi pada source |
| Build berhasil, uji runtime, dan DDL terbukti diterapkan | **Belum terpenuhi** |
| Layar frontend pengelolaan saldo awal | Source selesai di PAY-FE-FIX-016 (belum dibuild/dilihat) |
| Menyetujui saldo awal tidak mengubah payroll final | Terpenuhi (tidak ada penulisan ke run/snapshot) |

**Keputusan terbuka:** (a) **diputuskan pemilik 23 Nov 2026**: penolakan dan pengajuan boleh oleh orang yang sama dan dibedakan lewat role/izin; maker-checker hanya untuk persetujuan, dan pelaku penolakan dicatat; (b) rentang bulan terhadap tanggal cutover tidak divalidasi saat disetujui (mesin menolak bulan ≥ masa pajak berjalan saat dipakai); (c) aturan `PREVIOUS_EMPLOYER`; (d) ~~layar frontend dan menu~~ (dikerjakan di PAY-FE-FIX-016).

```text
$ git status --short   (backend, branch Yasmina, HEAD ab340bc7)
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M Areas/Corporate/PayrollManagement/Services/PayrollCalculationService.cs
?? Areas/Corporate/PayrollManagement/Controllers/PayrollOpeningTaxBalancesController.cs
?? Areas/Corporate/PayrollManagement/DTOs/PayrollOpeningTaxBalanceDtos.cs
?? Areas/Corporate/PayrollManagement/Models/PrlOpeningTaxBalance.cs
?? Areas/Corporate/PayrollManagement/Services/PayrollOpeningTaxBalanceService.cs
?? Areas/Corporate/PayrollManagement/Services/PayrollOpeningTaxBalanceStoreSource.cs
?? Repositories/Configurations/Corporate/PayrollManagement/PrlOpeningTaxBalanceConfiguration.cs
(+ perubahan PAY-BE-FIX-015 pada working tree yang sama)
```
