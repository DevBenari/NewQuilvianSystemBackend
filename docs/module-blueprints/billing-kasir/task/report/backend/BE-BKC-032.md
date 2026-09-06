# Laporan Perubahan Backend — `BE-BKC-032`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-032` — Regresi dan bukti keluar lintas gelombang |
| `TASK TYPE` | Audit/verifikasi penutup (bukan pekerjaan kode) — mengumpulkan bukti keluar seluruh gelombang `MVP-4`–`MVP-12` |
| `COMPLEXITY` | `LIGHT` untuk bagian yang dapat dikerjakan agent (audit data); sisanya di luar kapasitas agent (bukti UI ter-autentikasi) |
| `CLASSIFICATION SCORE` | 0 — tidak ada source yang ditulis |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` (audit/verifikasi, bukan implementasi) |
| `WRITE TARGET` | Tidak ada source yang ditulis. Wewenang tulis terbatas pada `task/report/backend/BE-BKC-032.md` beserta bukti roadmap/traceability |
| Gelombang | Penutup (`MVP-4` sampai `MVP-12`) |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement` (lintas submodule: `Billing`, `MasterData`) |
| Status registry | **`ACTIVE`** |
| Keberlakuan | Tidak ada `NEW CODE`/`TOUCHED LEGACY`/`LEGACY MIGRATION` — task ini murni audit/verifikasi, tidak menyentuh source atau schema |
| QBE ID yang berlaku | Tidak ada — tidak ada source yang ditulis |
| Otorisasi akses database | Query read-only tambahan dijalankan dengan mengandalkan otorisasi eksplisit yang sudah diberikan pengguna pada task `BE-BKC-031` untuk audit data serupa di database dev yang sama |

---

## 1. Sifat task ini dan blocker yang ditemukan di awal

`BE-BKC-032` berstatus `BLOCKED` di roadmap dengan dependency **"seluruh task gelombang ini"**
(`BE-BKC-022`–`030`) plus dua gerbang eksternal: `BKC-GATE-03` (penilaian Security untuk
`BE-BKC-023`) dan `BKC-GATE-09` (otorisasi eksekusi migration untuk `BE-BKC-027`). Sebelum task
ini dimulai, seluruh sembilan task pendahulu berstatus "source selesai, menunggu `dotnet
build`/`test` pengguna" — belum satu pun terverifikasi lulus secara tercatat.

**Pengguna mengonfirmasi secara eksplisit pada task ini** bahwa `dotnet build` dan `dotnet test`
atas seluruh backlog yang menumpuk (`BE-BKC-022` sampai `BE-BKC-030`) sudah dijalankan mandiri dan
**berhasil**. Konfirmasi ini diterima sebagai bukti sah untuk butir keluar #1 (lihat §3) — agent
sendiri **tidak** menjalankan `dotnet build`/`dotnet test` pada sesi ini, sesuai instruksi
eksplisit task untuk tidak menyentuh siklus build/test milik pengguna.

## 2. Metode verifikasi tambahan yang dijalankan

Karena beberapa butir bukti keluar (§ berikutnya) dapat diverifikasi lewat pembacaan data tanpa
menjalankan aplikasi, dan pengguna sebelumnya sudah memberi otorisasi eksplisit untuk audit
read-only ke database dev pada `BE-BKC-031`, tiga pemeriksaan tambahan dijalankan lewat query
`SELECT` read-only (memakai `Npgsql.dll` yang sudah ter-build sebelumnya, dijalankan lewat
`dotnet fsi` sebagai host skrip — bukan `dotnet build`/`test` project ini):

1. **Status migration `BE-BKC-027`** — apakah `20260904232421_AddWriteOffCategoryAndNonBillableResidual`
   sudah tercatat di `__EFMigrationsHistory`, dan apakah kolom fisik `BilWriteOffCase.Category`
   serta `BilCalculationVersion.NonBillableResidualAmount` sudah ada.
2. **Butir bukti #7** — hitungan `MstInsuranceCoverageRule` aktif yang menandai selisihnya tidak
   boleh ditagihkan ke pasien (`IsAllowExcessPaymentByPatient = false`), dipecah `NotCovered` versus
   `Covered` dengan tanggungan sebagian (`CoveragePercent < 100`).
3. **Butir bukti #5 (prasyarat)** — apakah sudah ada tagihan `RANAP` di database dev yang bisa
   dipakai sebagai contoh `BIL-AT-044`.

Tidak ada `INSERT`/`UPDATE`/`DELETE` yang dijalankan. Kredensial database tidak dicetak.

## 3. Hasil per butir bukti keluar (Definition of Done `BE-BKC-032`)

| # | Bukti yang diminta | Status | Detail |
| --- | --- | --- | --- |
| 1 | `dotnet build` dan `dotnet test` **benar-benar dijalankan** dan lulus | **TERPENUHI** | Dikonfirmasi eksplisit oleh pengguna 5 September 2026 mencakup seluruh backlog `BE-BKC-022`–`030`. Agent tidak menjalankan build/test sendiri pada sesi ini |
| 2 | Satu contoh tanggapan lembar Invoice Asuransi tersanitasi untuk masing-masing empat keadaan penjamin | **BELUM** | Butuh memanggil endpoint `GET .../invoices/{id}/insurance-invoice-sheet` (`BE-BKC-023`) terhadap data nyata di lingkungan ter-autentikasi — di luar kapasitas agent (tidak ada akses HTTP ter-autentikasi/browser pada sesi ini) |
| 3 | Satu berkas PDF hasil cetak yang memperlihatkan seluruh kolom tabel terbaca utuh pada kertas A4 | **BELUM** | Butuh mencetak dokumen lewat UI frontend — di luar kapasitas agent |
| 4 | Satu tangkapan layar Menu Pembayaran untuk tagihan beranomali, tersanitasi | **BELUM** | Butuh UI frontend ter-autentikasi — di luar kapasitas agent |
| 5 | Hasil `BIL-AT-044` untuk satu tagihan rawat inap uji berisi obat, memperlihatkan PPN tidak dikenakan sejak tagihan pertama | **BELUM — data prasyarat tidak ada.** Query terhadap `BilInvoice` (`ServiceType='RANAP'`) di database dev mengembalikan **nol baris** — belum ada satu pun tagihan rawat inap uji yang bisa dijadikan contoh. `BIL-AT-044` sudah dibuktikan lewat unit test (`InpatientPharmacyItemsAreExemptFromTax`, `BE-BKC-026`), tetapi bukti keluar #5 secara spesifik meminta **hasil nyata dari tagihan uji**, bukan hasil unit test |
| 6 | Hasil pemeriksaan nilai cara pembagian PPN yang aktif di lingkungan uji | **TERPENUHI** — sudah dikerjakan `BE-BKC-031`: tarif aktif `PPN-001`, `AllocationRule=PROPORTIONAL`, hanya satu tarif aktif pada satu waktu. Lihat `task/report/backend/BE-BKC-031.md` |
| 7 | Hitungan aturan tanggungan aktif yang menandai selisihnya tidak boleh ditagihkan, dipecah `NotCovered` vs tanggungan sebagian | **Angka dikumpulkan, tapi hasilnya NOL keduanya** — dari 8 aturan `MstInsuranceCoverageRule` aktif di database dev: **0** berstatus `NotCovered` dengan `IsAllowExcessPaymentByPatient=false`, dan **0** berstatus `Covered` dengan tanggungan sebagian (`CoveragePercent<100`) dan `IsAllowExcessPaymentByPatient=false`. Lihat catatan §4 — ini **bukan bukti fitur berjalan**, karena kolom yang menampungnya (`BE-BKC-027`) belum ada secara fisik |
| 8 | Satu contoh kasus penanggungan lengkap beserta jejak auditnya, tersanitasi | **BELUM** | Butuh transaksi nyata di lingkungan ter-autentikasi — di luar kapasitas agent |

## 4. Temuan kritis: `BKC-GATE-09` masih menahan, dibuktikan ulang secara langsung

Query terhadap `__EFMigrationsHistory` dan `information_schema.columns` di database dev
(`QuilvianNewDevYasmina`) mengonfirmasi:

- Migration `20260904232421_AddWriteOffCategoryAndNonBillableResidual` (`BE-BKC-027`) **tidak**
  muncul pada 10 migration terbaru yang tercatat di `__EFMigrationsHistory`.
- Kolom `BilWriteOffCase.Category` — **tidak ada secara fisik**.
- Kolom `BilCalculationVersion.NonBillableResidualAmount` — **tidak ada secara fisik**.

**Konsekuensi langsung:** seluruh fitur perutean selisih tidak dapat ditagihkan (`BE-BKC-028`,
`029`, `030`) — meski source-nya sudah ditulis dan `dotnet test` (yang memakai EF Core In-Memory
provider, bukan Postgres nyata) sudah lulus — **belum bisa dijalankan sama sekali** terhadap
database manapun yang belum menerima migration ini. Ini persis peringatan yang sudah dicatat
`BE-BKC-028.md` dan `BE-BKC-029.md`: "Kode task ini AKAN GAGAL runtime bila diaktifkan sebelum
migration itu dieksekusi." Task ini membuktikan ulang **secara langsung** (bukan menduga dari
laporan lama) bahwa peringatan itu masih berlaku persis pada 5 September 2026.

**Akibatnya, angka nol pada butir #7 di atas tidak dapat ditafsirkan sebagai "beban kerja
penanggungan Finance sangat rendah."** Angka itu murni mencerminkan bahwa data induk
(`MstInsuranceCoverageRule`) saat ini memang belum ada satu pun aturan yang dikonfigurasi dengan
kombinasi status yang relevan — terlepas dari itu, fitur perutean ke akumulator
`NonBillableResidualAmount` **tetap tidak dapat diuji ujung-ke-ujung** sampai `BKC-GATE-09`
ditutup dan migration benar-benar dieksekusi.

## 5. Regresi yang wajib lulus (daftar dari roadmap) — status verifikasi

| Yang diperiksa | Status verifikasi pada task ini |
| --- | --- |
| Total tagihan pasien tunai tidak berubah sama sekali | **Tercakup unit test** (`SelfPayCoverageAdapter` dipakai luas di seluruh test `BillingCalculationServiceTests`, jalur `SelfPay()` tidak disentuh task manapun di gelombang ini). Bukti nyata terhadap tagihan sungguhan: **belum** — butuh lingkungan ter-autentikasi |
| Kwitansi dan Struk Pasien tetap mencetak angka yang sama untuk tagihan `FINAL` lama | **Belum diverifikasi pada task ini** — butuh contoh cetak nyata (sama seperti butir #3) |
| Tagihan rawat inap pertama menghitung PPN dengan benar sejak awal | **Tercakup unit test** (`BE-BKC-026`); bukti data nyata **tidak tersedia** — nol tagihan `RANAP` di database dev (§3 butir 5) |
| Batas per kunjungan masih berlaku | **Tercakup unit test** (`RegistrationCoverageAdapterStillEnforcesPerVisitLimit`, `BE-BKC-024`) |
| Galat "lebih dari satu tarif pajak aktif" masih muncul pada tagihan rawat inap | **Tercakup secara struktural** — `LoadInvoiceTaxRuleAsync` melempar `BillingCalculationConflictException` bila lebih dari satu rule aktif ditemukan pada rentang waktu yang sama; dikonfirmasi hanya ada 1 tarif aktif saat ini (`BE-BKC-031`) sehingga jalur galat ini belum pernah teruji dengan data nyata (tidak ada kondisi pemicu di database dev saat ini) |
| Write-off piutang pasien berperilaku persis seperti sebelumnya | **Tidak dapat diverifikasi** — fitur write-off/`Category` bergantung pada migration `BE-BKC-027` yang belum dieksekusi (§4) |
| Angka baris "Selisih Tidak Ditagihkan" di layar kasir tidak berubah | **Tidak dapat diverifikasi** — butuh UI frontend, di luar kapasitas agent |
| Kwitansi dan Struk Pasien tetap tercetak pada kertas A5 | **Tidak dapat diverifikasi** — butuh UI frontend, di luar kapasitas agent |

## 6. Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Nihil** — tidak ada source yang diubah task ini |
| `DATABASE IMPACT` | **Nihil** — murni `SELECT` tambahan, tidak ada skema yang diubah |
| `SECURITY IMPACT` | **Nihil** — akses read-only, kredensial tidak dicetak |
| `VISUAL REFERENCE` | `NOT REQUIRED` |

## 7. Acceptance criteria dan Definition of Done

**Definition of Done `BE-BKC-032`:** *"seluruh acceptance `BIL-AT-029`–`061` punya bukti; seluruh
regresi lulus; tidak ada data pasien asli pada contoh mana pun; matriks bukti diperbarui."*

| Kriteria | Keadaan |
| --- | --- |
| Seluruh acceptance `BIL-AT-029`–`061` punya bukti | **Sebagian** — tercakup lewat unit test in-memory untuk hampir seluruh acceptance (dikonfirmasi lulus oleh pengguna); bukti berupa **data/dokumen nyata** (butir #2, #3, #4, #5, #8) **belum ada** |
| Seluruh regresi lulus | **Sebagian** — lihat §5. Regresi yang dapat diverifikasi lewat unit test dan struktur kode: lulus. Regresi yang menuntut lingkungan nyata (kwitansi/struk, tagihan rawat inap sungguhan, UI Menu Pembayaran): **belum terverifikasi** |
| Tidak ada data pasien asli pada contoh mana pun | **Tidak relevan saat ini** — belum ada contoh nyata yang dihasilkan sama sekali pada butir #2–5, #8 |
| Matriks bukti diperbarui | **Terpenuhi untuk task ini** — roadmap dan `requirement-traceability.md` diperbarui mencerminkan status sebenarnya per 5 September 2026 |

**Definition of Done BELUM tercapai.** Task ini **tidak** ditandai selesai. Blocker yang tersisa
murni bersifat prosedural/lingkungan, bukan kekurangan kode:

1. **`BKC-GATE-09`** (otorisasi eksekusi migration `BE-BKC-027`) — dibuktikan ulang masih tertutup.
   Tanpa ini, fitur write-off (`BE-BKC-028`–`030`) tidak bisa diuji dengan data nyata sama sekali.
2. **Bukti manual dari lingkungan ter-autentikasi** (butir #2, #3, #4, #8, dan bagian data-nyata
   dari butir #5) — di luar kapasitas agent pada sesi ini (tidak ada akses browser/UI/print).

## 8. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Angka nol pada butir bukti #7 **jangan** dibaca sebagai "risiko write-off sangat kecil" — itu murni cerminan data induk saat ini, dan fitur yang menghitungnya sendiri belum bisa berjalan sampai migration `BE-BKC-027` dieksekusi (§4) |
| `KNOWN ISSUES` | Sama seperti dicatat `BE-BKC-028.md`/`029.md`/`030.md`: seluruh kode wave write-off akan gagal runtime terhadap database mana pun yang belum menerima migration `BE-BKC-027`. Dikonfirmasi ulang berlaku pada database dev per 5 September 2026 |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — task ini secara definisi menuntut bukti dari lingkungan ter-autentikasi yang tidak tersedia untuk agent |
| `INCIDENTAL CHANGES` | `NONE` |
| `INTERRUPTIONS` | `NONE` |
| `GIT STATUS` | Nol berkas source berubah akibat task ini. Roadmap dan `requirement-traceability.md` diperbarui untuk seluruh task `BE-BKC-022`–`032` mencerminkan konfirmasi build/test pengguna dan temuan `BKC-GATE-09` — ini disengaja karena statusnya saling terkait langsung dengan closure `BE-BKC-032` |
| `NEXT RECOMMENDED STEP` | (1) Minta otorisasi `BKC-GATE-09` dan jalankan `dotnet ef database update` untuk migration `BE-BKC-027` ke database dev. (2) Setelah migration jalan, buat satu tagihan `RANAP` uji berisi obat untuk memenuhi butir bukti #5 dengan data nyata. (3) Kumpulkan butir #2, #3, #4, #8 lewat sesi manual di lingkungan ter-autentikasi (screenshot/PDF/response API tersanitasi). (4) Setelah keempatnya lengkap, `BE-BKC-032` dapat ditandai `DONE` dan seluruh rilis `MVP-4`–`MVP-12` modul Billing dan Kasir siap ditutup |
