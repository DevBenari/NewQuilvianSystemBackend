# Bank Darah — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Module name | `Bank Darah` |
| Module slug | `bank-darah` |
| Revision | `24` |
| Module status | `IN_PROGRESS` |
| Current phase | `BD-PH-007` |
| Last verified at | `2026-09-04` — hasil **`NOT_READY`** (modul); gelombang `MVP-0` **`READY_WITH_CONDITIONS`** |
| Backend source SHA | `ba75a05` cabang `sukmagp` — bukti terverifikasi di `5f7acaf`; pergerakan ke `ba75a05` **docs-only** |
| Frontend source SHA | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` |
| Decision revision | `11` — `DEC-BD-001` sampai `DEC-BD-047` |
| Domain architecture | revisi `6` — `DOMAIN_ARCHITECTURE_READY` |
| Contract version | `v4` (**`approved`**) — `Sukmagp` / `2026-09-03` |
| Roadmap | revisi `2` — **`APPROVED`** |
| Terakhir diperbarui | `2026-09-04` — verifikasi kesiapan, pemulihan build, dan impact scan revisi 4 |

## Keadaan sekarang — 4 September 2026

Modul berstatus **`IN_PROGRESS`**. Gelombang `MVP-0` **tuntas secara kode dan sudah terbukti**: build
hijau dan 101 pengujian Bank Darah lulus di `5f7acaf`. Yang menahan kesiapan sekarang bukan lagi
kerusakan, melainkan **cakupan** — baru satu dari lima gelombang yang ada.

| Cakupan | Putusan kesiapan | Syarat tersisa |
| --- | --- | --- |
| Modul Bank Darah | **`NOT_READY`** | Gelombang `MVP-1`..`MVP-4` beserta 12 task frontend. Nol dari 15 entity `Bbk*` operasional ada |
| Gelombang `MVP-0` | **`READY_WITH_CONDITIONS`** | **Satu syarat:** keempat migration dijalankan |

Seluruh fase perancangan sudah menghasilkan artefaknya, **tidak ada satu pun keputusan bisnis yang
masih memblokir**, dan tidak ada satu pun fase yang `BLOCKED`.

✅ **Kedua penanda `STALE` sudah dicabut.** Impact scan terbatas dijalankan 4 September 2026 atas
rentang `4205d18..5f7acaf` (backend) dan `afbb8ab..101ec5d3` (frontend). Peta kemampuan naik ke revisi
**4** dan berstatus `CURRENT`. Dua baris berpindah status dan **keduanya membaik**.

---

## Catatan historis — 3 September 2026

> Blok di bawah benar pada tanggalnya dan dipertahankan sebagai rekaman. Keadaan terkini ada di atas.

Modul naik dari `PARTIAL` ke `READY` pada 3 September 2026. Seluruh fase perancangan sudah
menghasilkan artefaknya, tidak ada satu pun keputusan bisnis yang masih memblokir, dan sejak hari itu
tidak ada satu pun fase yang `BLOCKED`.

**`BD-DEP-008` sudah ditutup** pada 3 September 2026 lewat commit `ed7fba8`: prefix `Bbk` terdaftar di
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, **persis seperti yang diajukan blueprint**.
Risiko "prefix berbeda → seluruh nama `Bbk*` berganti sebagai satu paket" yang tercatat sejak `v1`
**tidak terjadi**; seluruh nama pada kontrak `v4` tetap berlaku apa adanya.

**`G2b` juga sudah tertutup** pada 3 September 2026 lewat commit `8075784`: Lifecycle registri naik
dari `PLANNED` ke **`ACTIVE`**, yang menurut changelog registry "membuka wewenang implementasi entity
operasional `Bbk*` sesuai `QBE-MOD-002`".

**`G1` approval desain juga sudah tertutup** pada 3 September 2026. Owner menyatakan approval turun atas
nama **`Sukmagp`** bertanggal **`2026-09-03`**, dan keterangan itu sudah dicatat pada
`blueprint-manifest.md` revisi 20 beserta seluruh artefak set kontrak `v4`. Pertentangan pencatatan yang
sempat dicatat — changelog registry menyebut approval sudah ada sementara blueprint masih `draft` —
**selesai**: changelog registry ternyata benar, dan yang tertinggal memang pencatatan di sisi blueprint.

Dengan itu **ketiga gerbang global tertutup**: `G1` approval, `G2a` penamaan, dan `G2b` aktivasi modul.
Tidak ada lagi yang menahan penjadwalan task. Yang tetap berlaku adalah batas wewenang biasa: approval
membuka penjadwalan task lewat `build-module-backend`, sementara migration, eksekusi database di luar dev
pemilik, deployment, dan publikasi Git tetap wewenang terpisah yang diminta per tindakan.

---

## Pemeriksaan status dan impact scan 3 September 2026 — **selesai, blueprint tidak berubah**

**Pemicunya.** Backend bergerak dari `a9bc9fd` ke **`4205d18`** lewat merge `QuilvianIntegrationBackend`
ke `sukmagp`. Berbeda dengan seluruh pergerakan SHA sebelumnya pada modul ini, merge ini **membawa
perubahan source aplikasi yang nyata** — bukan hanya dokumen blueprint. Seluruh commit dokumen Bank
Darah tetap docs-only sebagaimana tercatat; yang berubah adalah keadaan sesudahnya.

`02-existing-capability-map.md` karena itu sempat ditandai `STALE`. **Impact scan terbatas sudah
dijalankan pada hari yang sama, dan penandanya dicabut.** Rincian buktinya ada di
`02-existing-capability-map.md` §Impact scan terbatas; ringkasannya di bawah.

### Cara batas scan dipertanggungjawabkan

Membatasi scan pada beberapa baris hanya sah bila baris lain memang tidak tersentuh. Itu diperiksa,
bukan diasumsikan: seluruh nama berkas `.cs` yang dikutip peta diadu dengan daftar berkas yang berubah.

| Pemeriksaan | Hasil |
| --- | --- |
| Berkas `.cs` yang dikutip peta kemampuan | 24 |
| Berkas `.cs` yang berubah karena merge | 28 |
| **Irisan keduanya** | **1 — `LabOrder.cs`**, dan perubahannya aditif |

### Hasil per area

| Area yang berubah | Kemampuan yang bergantung | Putusan |
| --- | --- | --- |
| LaboratoryManagement | `BD-CAP-014` pola API · `BD-CAP-007` pola pesanan · `BD-CAP-010` token konkurensi | **Tetap sahih.** `LabOrderController.cs` **tidak berubah**; route, `[Tags]`, `[AccessController]`, dan pembungkus `ApiResponse<T>` identik. `LabOrder.cs` bertambah satu kolom `Discipline`; seluruh field yang dikutip utuh, `Version` tidak tersentuh |
| InPatientManagement | `BD-CAP-003` sinyal penutupan kunjungan | **Tetap sahih.** `InpEpisode.cs` dan `EncounterStatus.cs` **tidak berubah**; kelima field `DEC-BD-014` utuh. Yang berubah hanya controller, yang tidak dipanggil Bank Darah |
| Migrations + snapshot | Rencana migration `02-backend-architecture.md` §I | **Tetap sahih.** Nol entity Bank Darah di snapshot, sesuai harapan. Yang bergeser hanya basis migration, kini `20260902042242_AddLabOrderDiscipline` |
| MasterData | `BD-CAP-006` | **Tetap sahih.** Yang berubah `BedController` dan `InpatientClearanceItem*`; keempat master yang dipakai Bank Darah tidak tersentuh |
| BillingManagement | `BD-CAP-015` | **Tetap `Extend`.** `BillingSourceContract.cs` **tidak berubah**; Bank Darah tetap belum ada di daftar sumber, sehingga `DEC-BD-016` tetap dibutuhkan |

**Nol baris kemampuan berpindah status. Blueprint Bank Darah tidak perlu diubah.**

### Dua temuan yang justru menguatkan blueprint

1. **Laboratory menyatakan Bank Darah di luar scope-nya, dengan kata-katanya sendiri.** Enum
   `LabDiscipline` yang baru memuat keterangan bahwa Bank Darah "sengaja tidak ada di sini karena tetap
   berada di luar scope modul". Ini menguatkan `DEC-BD-015`, `DEC-BD-018`, dan batas `BD-CTX-09` —
   batas itu kini berbukti **dua arah**, bukan hanya dari sisi Bank Darah.
2. **Pemecahan butir hak akses ternyata pola rumah.** Tim InPatient memecah `AccessAction` menjadi
   butir tersendiri (`Sign`, `SetIsolation`, `Reopen`, `MarkFinancialClearance`, `ReadFinancialClearance`)
   dengan alasan yang dinyatakan di kode: agar kasir dapat menandai tanpa ikut memperoleh akses baca
   resume pulang. Itu persis alasan `DEC-BD-043` dan `DEC-BD-044`. Rancangan hak akses `v4` terbukti
   mengikuti konvensi yang sedang berlaku.

### Satu catatan untuk task migration, bukan cacat blueprint

`MstServiceUnit` memasangkan ketiga penanda `IsAvailableFor*` yang sudah ada dengan satu index
gabungan. `BE-BD-002` menambahkan `IsAvailableForBloodOrder` tanpa menyebut index. Itu **bukan**
kekeliruan — jalur akses utamanya pemeriksaan satu unit berdasarkan `Id`, yang tidak menuntut index.
Relevan hanya bila kelak ada layar yang menyaring daftar unit berdasarkan penanda ini.

---

## Fase modul

| Fase | Nama | Status | Keterangan |
| --- | --- | --- | --- |
| `BD-PH-001` | Discovery dan Requirement | `DONE` | Sepuluh pass wawancara: scope, closure, architecture gap closure, architecture gap final closure, Storage Location, Storage Location decision, gerbang pemberian, role & authority, role residue, OQ residue. `SCOPE-BD-001`, `DEC-BD-001`..`DEC-BD-044`, `INV-BD-011`..`INV-BD-035`, `AC-BD-001`..`AC-BD-097`. |
| `BD-PH-002` | Audit kemampuan existing | `DONE` | 24 baris kemampuan pada `02-existing-capability-map.md` revisi **4**, status `CURRENT`. Audit penuh di `9522caa`; impact scan terbatas di `4205d18` (3 Sep) dan **`5f7acaf` (4 Sep)**. Dua baris membaik: `BD-CAP-005` dan `BD-CAP-018` menjadi `Ready to reuse`. **Catatan:** dua master baru belum punya baris `BD-CAP-*`; audit penuh disarankan sebelum `MVP-2`. |
| `BD-PH-003` | Gerbang kelengkapan requirement | `DONE` | `02-requirement-completeness-assessment.md` revisi 2. Delapan slice `READY_FOR_DOMAIN_DESIGN`, dua `PARTIALLY_READY`. **Catatan:** `BR-BD-020` (Storage Location) belum punya rumah slice resmi; sementara diperlakukan sebagai perluasan `BD-SLICE-03/04/10`. |
| `BD-PH-004` | Arsitektur domain rumah sakit (opsional) | `DONE` | Revisi 6, `DOMAIN_ARCHITECTURE_READY`. Sepuluh bounded context, dua puluh lima konsep domain, lima aggregate, empat invariant lintas aggregate, tujuh posisi arsitektur. Sepuluh gap arsitektur seluruhnya tertutup; nol gap terbuka. |
| `BD-PH-005` | Penyusunan blueprint target | `DONE` | Set kontrak naik empat kali: `v1` → `v2` (Storage Location) → `v3` (role & authority) → **`v4`** (role residue). **Bukti penerimaan:** set kontrak `v4` disetujui `Sukmagp` pada `2026-09-03` (`G1`), tercatat di manifest revisi 20 dan di kepala setiap artefak kontrak. |
| `BD-PH-006` | Perencanaan delivery | `DONE` | Roadmap **revisi 2** menggantikan revisi 1 yang `STALE`, dan statusnya naik dari `FORWARD-TEST / DRAFT` menjadi **`APPROVED`** ketika `G1` turun. Ketiga gerbangnya tertutup: `G1` approval, `G2a` penamaan (`ed7fba8`), `G2b` aktivasi (`8075784`); `G3` revisi 1 dihapus karena `DEF-BD-004` tertutup. |
| `BD-PH-007` | Implementasi backend | **`IN_PROGRESS`** | Gelombang `MVP-0` **tuntas secara kode dan terbukti**. `BE-BD-001`, `BE-BD-002`, `BE-BD-014` **selesai**; `BE-BD-016` **selesai sebagian** (12 dari 39 butir; sisanya arsitektural). Keempatnya meninggalkan laporan tracked. Build hijau dan **101 pengujian Bank Darah lulus** di `5f7acaf`. `MVP-1` (`BE-BD-003`) **kini aman dijadwalkan**. |
| `BD-PH-008` | Implementasi frontend | `NOT_STARTED` | Kontrak API **sudah** `approved` dan terkunci pada `v4`, sehingga gerbangnya tidak lagi menahan. Yang menahan tinggal urutan biasa: tidak ada task FE yang mendahului task BE pasangannya, dan belum ada satu pun task BE yang dijalankan. |
| `BD-PH-009` | Verifikasi kesiapan | **`IN_PROGRESS`** | Dijalankan dua kali pada 4 September 2026. Pass `f940ae3`: `NOT_READY`, dua blocker kritis. Pass `5f7acaf`: **`NOT_READY` karena cakupan**, kedua blocker kritis tertutup; gelombang `MVP-0` sendiri **`READY_WITH_CONDITIONS`**. Belum dapat ditutup `DONE`. |

### Ringkasan fase

| Fase selesai | Fase siap dimulai | Fase terblokir |
| --- | --- | --- |
| `BD-PH-001` sampai `BD-PH-006` | `BD-PH-007` berjalan · `BD-PH-009` berjalan | **Nihil** |

---

## Keadaan delivery

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| **`IN_PROGRESS`** | `NOT_STARTED` | `NOT_STARTED` | **`IN_PROGRESS`** |

Pembaginya 15 task backend (`BE-BD-001`..`012`, `014`, `015`, `016`; `BE-BD-013` berada di future
scope) dan 12 task frontend (`FE-BD-001`..`012`) — **27 task**. Angka di bawah dihitung dari
keberadaan laporan `task/report/**`, bukan diperkirakan.

| Task | Status | Yang menahan penyelesaiannya |
| --- | --- | --- |
| `BE-BD-001` | ✅ **`SELESAI`** | —. `MstBloodBankReason` diselesaikan `7d00647`; **naik dari `SELESAI SEBAGIAN`** |
| `BE-BD-002` | ✅ **`SELESAI`** | — |
| `BE-BD-014` | ✅ **`SELESAI`** | — |
| `BE-BD-016` | `SELESAI SEBAGIAN` | 12 dari 39 butir hak akses terdaftar (naik dari 8); sisanya lahir bersama controller pemakainya |
| 23 task lainnya | `NOT_STARTED` | 11 task backend dan 12 task frontend |

**Kemajuan delivery: 3 selesai penuh dan 1 selesai sebagian, dari 27 task.**

**Empat** migration sudah dibuat dan **belum satu pun dijalankan** — `AddMstBloodComponent`,
`AddServiceUnitBloodOrderFlag`, `AddMstBloodStorageLocation`, dan `AddMstBloodBankReason`. Selama
keempatnya belum dijalankan, keempat task di atas belum dapat dipakai di lingkungan mana pun.
Eksekusi database adalah wewenang terpisah, dan inilah **satu-satunya syarat** yang memisahkan
gelombang `MVP-0` dari selesai penuh.

**Bukti pengujian sudah terverifikasi.** Pada `5f7acaf`, `dotnet build` memulangkan `0 Error(s)` dan
`dotnet test` memulangkan **`Failed: 0, Passed: 101`** untuk pengujian Bank Darah serta
**`Failed: 0, Passed: 212`** untuk seluruh project unit test. Angka 101 persis sama dengan yang
diklaim keempat laporan task (26 + 8 + 25 + 30 + 12).

---

## Blocker yang masih terbuka

| Blocker ID | Ringkasan | Pemilik | Terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| `DEC-BD-016` | Persetujuan pemilik Billing atas konteks sumber biaya Bank Darah | Pemilik BillingManagement | Penyerahan biaya ke Billing | Pencatatan tindakan tetap dirancang penuh tanpa penyaluran biaya |
| `OQ-BD-011` | Mekanik label golongan darah | Pemilik proses klinis | Slice label | Pemeriksaan dan validasi golongan darah tetap dirancang penuh |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama | Pemilik proses klinis | `IMPLEMENTATION` aturan per komponen | Titik pemeriksaan kecocokan tetap dirancang |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong | Pemilik proses BDRS | Kegunaan `RETURNED_TO_PROVIDER` | Rancangannya tetap dibuat |
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan per komponen | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian | Nilainya dari konfigurasi katalog; selama kosong gerbang menolak |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru setelah dikoreksi | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi | Konsep catatan koreksi tetap dirancang penuh |
| `OQ-BD-016` | Apakah bukti pendukung koreksi menuntut lampiran berkas | Pemilik proses BDRS | Bentuk kolom bukti pendukung | Dirancang sebagai teks; lampiran kemampuan tersendiri |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada di repository | Pemilik kebutuhan | Penelusuran bukti ke kebutuhan | Perancangan tetap jalan |

**Tidak ada satu pun baris di atas yang memblokir gerbang atau fase.** Seluruhnya menyangkut scope di
luar rilis pertama (penyaluran biaya Billing), detail implementasi yang nilainya datang dari konfigurasi
master, atau satu baris seeder. Daftar ini dipertahankan supaya tidak hilang, bukan sebagai penahan.

**`DEF-BD-004` sudah tertutup seluruhnya** — keenam wewenangnya dipetakan `DEC-BD-039` sampai
`DEC-BD-044`. Ia tidak lagi menjadi blocker, dan gerbang `G3` pada roadmap revisi 1 dihapus.

⚠️ **Satu catatan pencatatan yang bukan blocker.** Approval `G1` menutup **blueprint dan set kontrak
`v4`**, sesuai bunyi gerbangnya di roadmap §B. Ia **tidak** otomatis menaikkan status register keputusan:
`DEC-BD-001` sampai `DEC-BD-044` pada `00-interview-decisions.md` tetap `draft` dengan `approved_by`
kosong. Menaikkannya menuntut pernyataan owner tersendiri. Ini **tidak menahan task mana pun** — builder
membaca kontrak, bukan register keputusan — tetapi dicatat supaya tidak dikira sudah ikut naik.

---

## Blocker yang sudah ditutup

| Blocker | Ditutup oleh |
| --- | --- |
| Sinyal penutupan kunjungan berbeda antar jenis kunjungan | `DEC-BD-014` |
| Bukti kecocokan sebelum pemberian darah | `DEC-BD-013`, `DEC-BD-017` |
| Sumber sah golongan darah | `DEC-BD-015` |
| Pengembalian dan pemakaian ulang kantong (`DEF-BD-001`) | `DEC-BD-019` |
| Penutupan administratif permintaan PMI (`DEF-BD-002`) | `DEC-BD-020` |
| Tindakan Bank Darah dan dasar biayanya | `DEC-BD-021` |
| Sampling dan batas dengan Laboratorium | `DEC-BD-018`, `DEC-BD-015` |
| Kedudukan HCLAB · laporan · setup | `DEC-BD-022`, `DEC-BD-023`, `DEC-BD-024` |
| `ARCH-BD-GAP-01`..`06` | `DEC-BD-025` sampai `DEC-BD-030` |
| `ARCH-BD-GAP-07`, `08`, `09` · `OQ-BD-013` | `DEC-BD-031` sampai `DEC-BD-034` |
| Coverage gap Storage Location | `DEC-BD-035`, `DEC-BD-036` |
| `ARCH-BD-GAP-10` nasib kantong di lokasi nonaktif | `DEC-BD-037` |
| `OQ-BD-015` gerbang pemberian dari lokasi nonaktif | `DEC-BD-038` |
| `DEF-BD-004` — validator, jalur darurat, koreksi | `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041` |
| `DEF-BD-004` — bukti kecocokan, penyelesaian, pembatalan order | `DEC-BD-042`, `DEC-BD-043`, `DEC-BD-044` |
| `BD-DEP-008` — prefix entity belum terdaftar di registry | Pendaftaran `Bbk` pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, commit `ed7fba8` 3 September 2026 |
| `BD-DEP-016` — modul belum diaktifkan (`PLANNED`) | Kenaikan Lifecycle ke `ACTIVE`, commit `8075784` 3 September 2026 |
| `OQ-BD-017` — nama peran pemegang `BloodUnit : ResolveNotUsable` | `DEC-BD-045` — kewenangan operasional BDRS, peran yang sama dengan `ResolveReturn`; ketiga butir tetap terpisah |
| `OQ-BD-018` — apakah hasil bukti kecocokan menggerbang | `DEC-BD-046` — hasil `Incompatible` menahan pemberian jalur normal; `VAL-BD-079` ditegaskan |
| `CONF-BD-006` — baris peran BDRS umum memuat `BloodUnit : Compatibility`, bertentangan dengan `DEC-BD-042`, `VAL-BD-078`, dan `AC-BD-090` | `DEC-BD-047` — butir dicabut dari baris peran umum. Ditemukan `BE-BD-016`, diserap `permission-audit-matrix.md` pada hari yang sama |
| `G1` — approval blueprint dan set kontrak `v4` | Approval owner `Sukmagp` bertanggal `2026-09-03`, dicatat pada manifest revisi 20 dan seluruh artefak set kontrak |
| Pencatatan `G1` yang bertentangan antara registry dan blueprint | Keterangan owner 3 September 2026; changelog registry terbukti benar, pencatatan blueprint yang tertinggal dan kini sudah selaras |
| Build backend rusak — `HEAD` `f940ae3` gagal dikompilasi, 217 error `CS0246` Xunit | Commit `5f7acaf` 4 September 2026. Kelima berkas test Bank Darah dipindahkan dari folder yatim `QuilvianSystemBackend.Tests/` ke `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/`. `dotnet build` kini `0 Error(s)` |
| Bukti 101 pengujian Bank Darah tidak berada di project mana pun | Commit `5f7acaf`. Git mencatat kelimanya sebagai rename `R099` — isinya utuh, bukan dihapus. `dotnet test` memulangkan `Failed: 0, Passed: 101` |

---

## Bukti yang sudah usang

| Artefak | SHA tercatat | SHA saat ini | Tinjauan dampak yang diperlukan |
| --- | --- | --- | --- |
| `02-existing-capability-map.md` | audit penuh `9522caa` · impact scan **`5f7acaf`** | `5f7acaf` | ✅ **Sudah disegarkan dan tetap sahih.** Impact scan 4 September 2026 atas rentang penuh `4205d18..5f7acaf`: dari 46 rujukan bukti hanya `MstServiceUnit.cs` tersentuh, dan perubahannya **aditif murni**. Nol berkas bukti frontend berubah. Dua baris membaik, nol memburuk |
| `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` | `8b298bb` | `5f7acaf` | Terbatas pada konfigurasi Laboratorium. Dampaknya menyempit sejak `DEC-BD-018` memisahkan sampel Bank Darah dari sampel Laboratorium |
| `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | `8b298bb` | `5f7acaf` | Sama seperti di atas. PRD §3 yang menganjurkan memakai model sampel Laboratorium sudah digantikan `DEC-BD-018` |

✅ **Frontend `afbb8ab` → `101ec5d3` sudah ikut discan.** Kesepuluh komponen dasar yang dikutip
`BD-CAP-021` / `BD-DEP-014` **tidak berubah**. Enam berkas `base-features/` lain memang berubah, tetapi
bukan yang dikutip peta — `base-editor-view.jsx` berbeda dari `base-editor-form.jsx`, dan
`resource-filter-select.jsx` berbeda dari `filter-select.jsx`. Bukti frontend **tetap sahih**.

---

## Artefak yang sudah ada

| Artefak | Keadaan |
| --- | --- |
| `00-interview-decisions.md` | Revisi **11** — `DEC-BD-001`..`047`, `INV-BD-011`..`035`, `AC-BD-001`..`097` |
| `02-existing-capability-map.md` | Revisi **4** — 24 kemampuan, `CURRENT`. Impact scan `5f7acaf` 4 September 2026 |
| `02-requirement-completeness-assessment.md` | Revisi 2 — `BR-BD-020` belum punya rumah slice |
| `01-prerequisite-readiness.md` | Revisi 3 — `BD-DEP-001`..`015` |
| `03-domain-architecture.md` | Revisi 6 — `DOMAIN_ARCHITECTURE_READY`, nol gap terbuka |
| `02-backend-architecture.md` | Kontrak `v4` (`approved`) — 15 tabel `Bbk*`, 3 master `Mst*`, 11 enum |
| `03-frontend-architecture.md` | Kontrak `v4` (`approved`) — 10 layar, peta menu, 21 kewajiban layar |
| `04-prd-to-mvp.md` | Kontrak `v4` (`approved`) — 12 epic, `UAT-01`..`21`, gelombang `MVP-0`..`MVP-4` |
| `data/data-dictionary.md` | Kontrak `v4` (`approved`) |
| `contracts/` (5 berkas) | `v4` (`approved`), kecuali `integration-contract.md` yang `last_changed_in: v2` karena isinya tidak bergerak — ia tetap ikut disetujui sebagai bagian set `v4` |
| `flowcharts/` (7 berkas) | Termasuk `penyimpanan-kantong.md` yang baru pada `v2` |
| `testing/acceptance-test-matrix.md` | Kontrak `v4` (`approved`) — `AC-BD-001`..`097` |
| `roadmap/00-delivery-plan.md` | Revisi 2 — **`APPROVED`** |
| `task/report/**` | **Empat laporan** — `BE-BD-001.md`, `BE-BD-002.md`, `BE-BD-014.md`, `BE-BD-016.md`. Seluruh bukti pengujiannya **terverifikasi lulus** 4 September 2026 |

---

## Task berikutnya yang disarankan

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **Jalankan keempat migration `MVP-0`** di dev pemilik | Pemilik database | **Wewenang terpisah.** Satu-satunya syarat yang memisahkan `MVP-0` dari selesai penuh. Tanpa ini keempat task belum dapat dipakai di lingkungan mana pun |
| 2 | ~~Impact scan terbatas~~ | Skill | ✅ **Selesai** 4 September 2026. Peta naik ke revisi 4, `CURRENT`. **Audit penuh** disarankan sebelum `MVP-2`: `MstBloodStorageLocation` dan `MstBloodBankReason` belum punya baris `BD-CAP-*` |
| 3 | **Jadwalkan `MVP-1` mulai `BE-BD-003`** (order darah) | Skill | `build-module-backend`, satu task satu wewenang. **Kini aman** — baseline hijau, 212 pengujian lulus. Dependency `BE-BD-001` dan `BE-BD-002` keduanya sudah selesai |
| 4 | Setelah pasangan BE-nya ada: mulai task frontend dari `FE-BD-001` | Skill | `build-module-frontend`. Kontrak `v4` sudah terkunci dan `approved` |
| 5 | Jalankan ulang `verify-module-readiness` setelah `MVP-1` tuntas | Skill | Verifikasi 4 September 2026 berlaku sampai gelombang berikutnya selesai |

**Migration, eksekusi database di luar dev pemilik, deployment, dan publikasi Git tetap wewenang
terpisah.** Approval `G1` tidak menyentuh keempatnya.

`grill-me` untuk keputusan bisnis **tidak** diperlukan pada scope yang dinilai — tidak ada keputusan
bisnis yang masih memblokir.

---

## Kontrak status

`DRAFT` berarti identitas modul sudah ada tetapi pengumpulan kebutuhan belum lengkap. `DISCOVERY`
berarti sedang mengumpulkan keputusan dan bukti. `READY` berarti fase yang direncanakan boleh
dimulai. `PARTIAL` berarti minimal satu fase siap sementara fase lain terblokir atau belum
diketahui. `BLOCKED` berarti tidak ada satu pun fase berarti yang dapat berjalan dengan aman.
`IN_PROGRESS` berarti ada pekerjaan aktif yang sudah diberi wewenang. `VERIFYING` berarti menunggu
bukti kesiapan. `DONE` menuntut bukti verifikasi yang memadai. `SUPERSEDED` mencatat blueprint
penggantinya.

Status fase memakai `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, dan `SUPERSEDED`.
Sebuah fase menjadi `DONE` hanya bila bukti penerimaannya tercatat. Keberadaan file saja tidak cukup.
