# Laporan Perubahan Backend — `BE-FIN-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-FIX-002` |
| Judul | Serah terima piutang (`BilArHandoff`) bertipe `PAYER` membawa identitas penjamin **perusahaan**, dan pemilihan baris penjamin dibuat dapat diulang |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap. Mengikuti pola `FIX` yang sudah dipakai `BE-FIN-FIX-001` |
| Roadmap | Tidak ada baris roadmap resmi. Permintaan langsung pemilik, 5 Oktober 2026 |
| Trace | `FIN-CAP-069` (`Repair`, `01-existing-capability-map.md` bagian 21.2); butir `B4` pada `evidence/23-permintaan-konfirmasi-piutang-manfaat-karyawan.md`; `FIN-DEC-048` (Batch Tagihan hanya untuk `PAYER`) |
| Contract version | **Tidak ada perubahan kontrak API.** Nol endpoint, nol DTO, nol field baru. Yang berubah hanya **isi** `BilArHandoff.DebtorReferenceId` pada baris yang terbit setelah perubahan ini |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — repository tulis 1; berkas diubah 1; nol endpoint; nol DTO; nol migration; nol hak akses; logika terbatas pada satu kueri dan satu penugasan nilai |
| Task mode | `BACKEND`, dideklarasikan pemilik pada permintaan. Target tulis **hanya** `NewQuilvianSystemBackend`. Repository frontend tidak disentuh task ini |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `46fa2a91` (branch `Yasmina`). Working tree **tidak bersih** sebelum task: pekerjaan `BE-FIN-FIX-001` dan dokumen blueprint belum di-commit |
| Tanggal | 2026-10-05 |
| Status | 🟡 **KODE SELESAI, `dotnet build` BERHASIL (dijalankan pemilik).** Tersisa satu butir Definition of Done: verifikasi dengan data nyata. Belum boleh ditandai `✅` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Pemilik / prefix pada registry | `Bil`, baris `HealthServices \| BillingManagement / Billing`, status **ACTIVE** (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 20) |
| Keberlakuan | `TOUCHED LEGACY` — menyunting method yang sudah ada; nol entity baru, nol endpoint baru, nol skema |
| QBE ID yang benar-benar berlaku | `QBE-AUD-001` (MUST / ALL) — dipenuhi: nol logging aplikasi dan nol jejak audit database yang disentuh. Seluruh QBE lain pada kontrak berlingkup `NEW CODE`, sehingga **tidak berlaku** untuk perubahan ini |
| Catatan `TOUCHED LEGACY` | Kontrak menyatakan `TOUCHED LEGACY` **SHOULD** memperbaiki pelanggaran "hanya bila aman, terbatas cakupannya, dan diberi wewenang". Ketiganya terpenuhi: perubahan satu berkas, memakai pola yang sudah ada di modul yang sama, dan diperintahkan pemilik secara eksplisit |

### Catatan kepemilikan modul yang MUST dibaca

Berkas yang diubah milik modul **Billing** (prefix `Bil`), **bukan** Finance. Perubahan ini dikerjakan
atas **instruksi eksplisit pemilik Finance (Yasmin), 5 Oktober 2026**, sesudah disodorkan tiga pilihan
beserta konsekuensinya; pemilik memilih opsi B (perbaikan minimal **ditambah** perbaikan pemilihan baris
penjamin).

Akibatnya pada dokumen lain: butir **`B4`** pada `evidence/23-permintaan-konfirmasi-piutang-manfaat-karyawan.md`
semula berupa **pertanyaan** kepada pemilik Billing ("apakah Billing bersedia memperbaikinya?"). Butir itu
sekarang **sudah dikerjakan lebih dulu**, sehingga saat berkas itu dikirim, `B4` MUST diubah menjadi
**pemberitahuan** beserta tautan laporan ini — bukan dibiarkan sebagai pertanyaan terbuka.

---

## 1. Masalah yang diperbaiki

Ada dua cacat pada satu tempat yang sama, yaitu saat tagihan difinalisasi dan Billing menerbitkan fakta
piutang untuk Finance.

**Cacat pertama — piutang penjamin perusahaan lahir tanpa identitas debitur.**

Baris serah terima bertipe `PAYER` hanya mengambil identitas debitur dari `InsuranceProviderId`:

```csharp
DebtorReferenceId = guarantor?.InsuranceProviderId,
```

Kunjungan yang dijamin **perusahaan** menyimpan identitasnya pada `CompanyGuarantorId`, dan
`InsuranceProviderId`-nya kosong. Jadi piutangnya lahir dengan debitur kosong. Akibatnya nyata:

- Piutang itu tidak dapat dikelompokkan per penjamin.
- Piutang itu **tidak dapat digabung** ke Batch Tagihan, karena penggabungan memerlukan satu identitas
  penjamin sebagai kuncinya. Pekerjaan `BE-FIN-FIX-001` menandainya dengan alasan `NO_DEBTOR_REFERENCE`.

**Cacat kedua — baris penjamin dipilih tanpa urutan.**

```csharp
.Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete)
.FirstOrDefaultAsync(cancellationToken);
```

Satu kunjungan boleh memiliki lebih dari satu penjamin — modelnya memang menyediakan `IsPrimary` dan
`Priority`. Tanpa `OrderBy`, database bebas memulangkan baris mana pun, sehingga debitur pada piutang bisa
berbeda-beda untuk keadaan data yang sama. Penjamin yang sudah **dibatalkan** (`IsCancel`) pun masih ikut
terpilih, karena hanya `IsDelete` yang disaring.

Modul yang sama sudah memiliki pola yang benar untuk hal ini, pada `BillingDepositService.cs` baris 127-133.

---

## 2. Proses bisnis

**Alur normal: tagihan difinalisasi.**

1. Petugas Billing memfinalisasi tagihan sebuah kunjungan.
2. Sistem menghitung porsi penjamin. Bila porsinya lebih besar dari nol, satu baris serah terima `PAYER`
   diterbitkan untuk Finance.
3. Sistem mencari penjamin kunjungan itu: penjamin yang dibatalkan dibuang, penjamin **utama**
   diutamakan, lalu sisanya mengikuti `Priority`.
4. Identitas debitur diambil dari asuransi bila ada; bila tidak ada, dari perusahaan penjamin.
5. Finance menyalin nominal dan identitas itu apa adanya menjadi kartu piutang.

> **Contoh 1 — penjamin perusahaan.** Kunjungan Tn. Andi dijamin **PT Sehat Sejahtera**. Tagihan
> Rp 10.000.000, porsi penjamin Rp 7.000.000.
>
> | | Sebelum | Sesudah |
> |---|---|---|
> | Identitas debitur pada serah terima | *kosong* | Id `PT Sehat Sejahtera` |
> | Dapat digabung ke Batch Tagihan | **Tidak** (`NO_DEBTOR_REFERENCE`) | **Ya** |

> **Contoh 2 — asuransi, tidak berubah.** Kunjungan dijamin **Asuransi Prima**. Identitas debitur tetap
> diambil dari asuransi, persis seperti sebelumnya. Nol perubahan perilaku.

> **Contoh 3 — dua penjamin.** Kunjungan punya kartu asuransi lama yang sudah **dibatalkan** dan penjamin
> perusahaan yang ditandai **utama**. Sebelumnya hasilnya bisa menunjuk kartu yang dibatalkan itu, dan bisa
> berubah-ubah. Sesudahnya kartu yang dibatalkan dibuang dan yang terpilih selalu penjamin utama.

**Jalur tidak normal.**

| Kejadian | Hasil |
| --- | --- |
| Kunjungan tidak punya penjamin aktif sama sekali | Identitas debitur kosong, seperti sebelumnya. Perilaku ini **tidak** diubah |
| Penjamin ada, tetapi `InsuranceProviderId` dan `CompanyGuarantorId` dua-duanya kosong | Identitas debitur kosong. Perilaku ini **tidak** diubah |
| Porsi penjamin nol atau kurang | Tidak ada serah terima `PAYER` yang terbit, seperti sebelumnya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Mengapa diperiksa |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs` | Tempat satu-satunya yang membuat `BilArHandoff` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs` | Sumber pola pemilihan penjamin yang benar, pada modul yang sama (baris 127-133) |
| `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs` | Memastikan `IsPrimary` (`bool`), `Priority` (`int`), `InsuranceProviderId` (`Guid?`), dan `CompanyGuarantorId` (`Guid?`) benar-benar ada |
| `Models/IdentityModel.cs` | Memastikan `IsCancel` memang ada pada base class, bukan hanya pada entity lain |
| `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs` | Memastikan `DebtorReferenceId` bertipe `Guid?` dan tidak ada validasi yang melarang nilai dari master lain |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Memastikan Finance menyalin identitas debitur apa adanya, tanpa memvalidasinya ke satu master tertentu |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableBillingDataService.cs` | Memastikan Finance sudah memetakan identitas debitur ke **dua** master (asuransi dan perusahaan) saat menampilkan nama |

### 3.2 Berkas yang berubah

| Berkas | Perubahan | Baris |
| --- | --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingArApHandoffService.cs` | Kueri penjamin: tambah `!x.IsCancel`, tambah `OrderByDescending(IsPrimary)` dan `ThenBy(Priority)`. Identitas debitur: `InsuranceProviderId ?? CompanyGuarantorId`. Ditambah dua blok komentar beralasan | 68-89 (+13/−2) |

Nol berkas lain disentuh. Nol migration dibuat.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol.** Tidak ada endpoint, DTO, atau field yang berubah bentuknya |
| Skema database | **Nol.** Tidak ada migration, tidak ada kolom, tidak ada constraint |
| Hak akses | **Nol.** Tidak ada `[AccessAction]` atau `[AccessPermission]` yang disentuh |
| Isi data baru | Mulai sekarang `BilArHandoff.DebtorReferenceId` dapat memuat Id dari **dua** master: `MstInsuranceProvider` atau `MstCompanyGuarantor`, tanpa kolom pembeda. Ini **bukan** pola baru — `BillingDepositService` sudah melakukannya, dan Finance sudah membaca kedua master saat memetakan nama |
| Data lama | **Tidak di-backfill.** Baris serah terima dan kartu piutang yang sudah ada tetap kosong identitas debiturnya. Lihat bagian 7 |
| Keamanan dan privasi | **Nol.** Tidak ada data sensitif baru, tidak ada logging baru, tidak ada secret |

---

## 4. Dokumentasi endpoint

**Tidak ada endpoint yang dibuat atau diubah.** Perubahan ini terjadi di dalam layanan yang berjalan saat
finalisasi tagihan, bukan pada permukaan API. Grup Swagger yang terdampak **isinya**, bukan bentuknya:

| Grup Swagger | Endpoint | Bentuk | Yang berubah |
| --- | --- | --- | --- |
| Billing Management | `POST /api/v1/health-services/billing-management/.../finalization` (pemanggil hulu) | Tidak berubah | Baris serah terima yang dihasilkannya kini membawa identitas debitur untuk penjamin perusahaan |
| Finance — Billing Intake | `GET /api/v1/corporate/finance-management/billing-intake` | Tidak berubah | Baris yang masuk kini membawa identitas debitur |
| Finance — Piutang | `GET /api/finance/receivable` dan `GET /api/v1/corporate/finance-management/receivables` | Tidak berubah | Kartu piutang baru untuk penjamin perusahaan kini punya identitas debitur |

---

## 5. Verifikasi

| Bukti | Hasil |
| --- | --- |
| QBE Preflight | **DICATAT** — lihat Metadata. `QBE-AUD-001` dipenuhi; QBE lain berlingkup `NEW CODE` dan tidak berlaku |
| Review diff dan cakupan | **PASS** — 1 berkas, +13/−2, tiga hunk (`+68,4`, `+73,3`, `+84,6`), seluruhnya di dalam blok `if (payerAmount > 0)`. `git status --short` memastikan tidak ada berkas backend lain yang ikut berubah oleh task ini |
| Kesesuaian tipe | **PASS** — `InsuranceProviderId` dan `CompanyGuarantorId` dua-duanya `Guid?`, dan `DebtorReferenceId` juga `Guid?`. Ekspresi `a ?? b` di sini **sama persis** dengan `BillingDepositService.cs:133` yang sudah ter-compile pada source hari ini |
| Keberadaan `IsCancel` | **PASS** — berasal dari `IdentityModel.cs:35`, berlaku untuk seluruh entity termasuk `RegPatientEncounterGuarantor` |
| Verifikasi kontrak API | **PASS (secara telaah)** — nol endpoint, DTO, dan field berubah |
| Verifikasi proses bisnis | **PASS (secara telaah)** — ketiga contoh pada bagian 2 ditelusuri terhadap kode hasil perubahan |
| `dotnet restore` | Tercakup oleh build yang dijalankan pemilik; tidak dijalankan terpisah oleh agent |
| `dotnet build` | **BERHASIL — dijalankan pemilik**, 5 Oktober 2026. Pemilik menyatakannya langsung pada sesi yang sama. **Bukti pendukung di disk:** `QuilvianSystemBackend.dll` pada `obj/Release/net9.0`, `obj/Release/net9.0/ref`, dan `bin/Release/net9.0` ketiganya bertanggal **14:48:42**, sedangkan source yang diubah task ini bertanggal **14:06:33** — jadi kompilasi berjalan **sesudah** perubahan dan menghasilkan assembly keluaran. **Yang tidak tercatat:** jumlah error dan warning apa adanya, karena keluaran console-nya tidak tersedia bagi agent. Bila angkanya perlu masuk laporan, pemilik dapat menempelkan keluarannya dan baris ini diperbarui |
| Verifikasi manual/runtime | `NOT FEASIBLE` — jalur ini hanya berjalan saat finalisasi tagihan pada backend yang tersambung database berisi kunjungan yang dijamin perusahaan. Tidak ada environment seperti itu pada sesi ini |
| `AUTOMATED TEST` | `NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)` |

**Yang MUST dikerjakan pemilik sebelum task ini boleh ditandai `✅`:**

1. ~~`dotnet build` pada project aplikasi~~ — **SUDAH, berhasil.**
2. Satu finalisasi tagihan nyata untuk kunjungan yang dijamin **perusahaan**, lalu periksa baris
   `BilArHandoff` yang terbit: `DebtorReferenceId` MUST berisi Id perusahaan penjamin.
3. Satu finalisasi tagihan untuk kunjungan yang dijamin **asuransi**, untuk memastikan jalur lama tidak
   berubah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Serah terima `PAYER` untuk kunjungan yang dijamin perusahaan membawa Id perusahaan penjamin | **Terpenuhi di source** | `BillingArApHandoffService.cs:89` |
| Jalur asuransi tidak berubah perilakunya | **Terpenuhi di source** | Asuransi tetap menang karena diuji lebih dulu pada `??` |
| Penjamin yang dibatalkan tidak lagi terpilih | **Terpenuhi di source** | `!x.IsCancel` pada baris 73 |
| Pemilihan baris penjamin dapat diulang hasilnya | **Terpenuhi di source** | `OrderByDescending(IsPrimary).ThenBy(Priority)` baris 74-75 |
| Nol perubahan kontrak API, skema, dan hak akses | **Terpenuhi** | Bagian 3.3 |
| `dotnet build` berhasil tanpa error | **Terpenuhi** | Dijalankan pemilik; artefak `Release/net9.0` bertanggal sesudah perubahan source |
| Diverifikasi dengan data nyata | **BELUM** | Tidak ada environment |

**Definition of Done belum terpenuhi** pada **satu** butir terakhir — verifikasi dengan data nyata — karena itu statusnya tetap 🟡. Butir `dotnet build` sudah tertutup.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko yang tersisa | (1) **Data lama tidak ikut terperbaiki.** Serah terima dan piutang penjamin perusahaan yang sudah terbit tetap kosong identitas debiturnya, sehingga tetap tidak dapat digabung ke Batch Tagihan. Memperbaikinya menuntut *backfill* database, yaitu wewenang terpisah yang **tidak** dimiliki task ini. (2) Pada kunjungan dengan beberapa penjamin, debitur pada serah terima **baru** dapat berbeda dari yang dihasilkan kode lama — itu memang tujuan perbaikan, dan dipilih sadar oleh pemilik |
| Temuan di luar cakupan (tidak diubah) | (a) Serah terima bertipe `PATIENT_GUARANTOR` (baris 45-62) **tidak** mengisi `DebtorReferenceId` sama sekali. Untuk jenis ini debiturnya pasien itu sendiri, dan Finance menampilkan namanya dari `PatientId` pada rincian piutang, jadi kosong tampaknya memang disengaja — tetapi **belum pernah dinyatakan** di keputusan mana pun. (b) `BilArHandoff` tidak punya kolom pembeda master untuk `DebtorReferenceId`; selama hanya ada dua master hal ini aman, tetapi perlu diingat bila master ketiga muncul |
| Dampak pada dokumen blueprint | `FIN-CAP-069` pada `01-existing-capability-map.md` bagian 21.2 diperbarui dari `Repair` menjadi diperbaiki di source. Butir `B4` pada `evidence/23` MUST diubah menjadi pemberitahuan sebelum berkas itu dikirim |
| Git | Nol stage, commit, push, pull, merge, rebase, dan deploy. Working tree memuat pula pekerjaan `BE-FIN-FIX-001` yang belum di-commit dan **tidak** disentuh task ini |
| Satu langkah berikutnya | Finalisasi satu tagihan nyata untuk kunjungan yang dijamin **perusahaan**, lalu periksa `DebtorReferenceId` pada baris `BilArHandoff` yang terbit. Sesudah itu putuskan apakah *backfill* identitas debitur untuk baris lama dikerjakan sebagai task database tersendiri |
