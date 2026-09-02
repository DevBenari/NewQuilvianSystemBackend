# Bank Darah — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Module name | `Bank Darah` |
| Module slug | `bank-darah` |
| Revision | `16` |
| Module status | `PARTIAL` |
| Current phase | `BD-PH-006` |
| Last verified at | `belum pernah diverifikasi` |
| Backend source SHA | `4205d18a6d656555eedd781f14e8a18fb5ea20d1` cabang `sukmagp` |
| Frontend source SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Decision revision | `9` — `DEC-BD-001` sampai `DEC-BD-044` |
| Domain architecture | revisi `6` — `DOMAIN_ARCHITECTURE_READY` |
| Contract version | `v4` (`draft`) |
| Roadmap | revisi `2` — `FORWARD-TEST / DRAFT` |
| Terakhir diperbarui | `2026-09-03` |

Modul tetap `PARTIAL`. Seluruh fase perancangan sudah menghasilkan artefaknya, dan **tidak ada satu pun
keputusan bisnis yang masih memblokir**. Yang menahan implementasi tinggal dua hal: approval owner atas
set kontrak `v4`, dan pendaftaran prefix entity di registry (`BD-DEP-008`).

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
| `BD-PH-001` | Discovery dan Requirement | `DONE` | Delapan pass wawancara: scope, closure, architecture gap closure, architecture gap final closure, Storage Location, Storage Location decision, gerbang pemberian, role & authority, role residue. `SCOPE-BD-001`, `DEC-BD-001`..`DEC-BD-044`, `INV-BD-011`..`INV-BD-035`, `AC-BD-001`..`AC-BD-097`. |
| `BD-PH-002` | Audit kemampuan existing | `DONE` | 24 baris kemampuan pada `02-existing-capability-map.md` revisi **3**. Audit penuh di `9522caa`; **impact scan terbatas di `4205d18` sudah dijalankan** 3 September 2026 dan penanda `STALE` dicabut. Nol baris berpindah status. |
| `BD-PH-003` | Gerbang kelengkapan requirement | `DONE` | `02-requirement-completeness-assessment.md` revisi 2. Delapan slice `READY_FOR_DOMAIN_DESIGN`, dua `PARTIALLY_READY`. **Catatan:** `BR-BD-020` (Storage Location) belum punya rumah slice resmi; sementara diperlakukan sebagai perluasan `BD-SLICE-03/04/10`. |
| `BD-PH-004` | Arsitektur domain rumah sakit (opsional) | `DONE` | Revisi 6, `DOMAIN_ARCHITECTURE_READY`. Sepuluh bounded context, dua puluh lima konsep domain, lima aggregate, empat invariant lintas aggregate, tujuh posisi arsitektur. Sepuluh gap arsitektur seluruhnya tertutup; nol gap terbuka. |
| `BD-PH-005` | Penyusunan blueprint target | `IN_PROGRESS` | Set kontrak naik empat kali: `v1` → `v2` (Storage Location) → `v3` (role & authority) → **`v4`** (role residue). Seluruhnya `draft`. Belum `DONE` — approval owner belum ada. |
| `BD-PH-006` | Perencanaan delivery | `IN_PROGRESS` | Roadmap **revisi 2** (`FORWARD-TEST / DRAFT`) menggantikan revisi 1 yang `STALE`. Dua gerbang: `G1` approval, `G2` `BD-DEP-008`. `G3` revisi 1 dihapus karena `DEF-BD-004` tertutup. |
| `BD-PH-007` | Implementasi backend | `BLOCKED` | Terhalang `G1` (approval) dan `G2` (`BD-DEP-008`). |
| `BD-PH-008` | Implementasi frontend | `NOT_STARTED` | Menunggu kontrak API di-approve dan terkunci hash. |
| `BD-PH-009` | Verifikasi kesiapan | `NOT_STARTED` | Belum ada implementasi untuk diverifikasi. |

### Ringkasan fase

| Fase selesai | Fase berjalan | Fase terblokir |
| --- | --- | --- |
| `BD-PH-001`, `BD-PH-002`, `BD-PH-003`, `BD-PH-004` | `BD-PH-005`, `BD-PH-006` | `BD-PH-007` |

---

## Keadaan delivery

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

Belum ada `task/report/**` pada folder modul — tidak ada satu pun task implementasi yang pernah
dijalankan. Kemajuan delivery **belum dapat dihitung**: roadmap revisi 2 belum di-approve, sehingga
belum ada pembagi yang sah. Persentase tidak boleh diperkirakan.

---

## Blocker yang masih terbuka

| Blocker ID | Ringkasan | Pemilik | Terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| **`G1` approval** | Owner belum menyetujui blueprint dan set kontrak `v4` | Pemilik proses BDRS + arsitektur backend | **Seluruh** task BE & FE | Seluruh pekerjaan perancangan sudah selesai; tidak ada yang menunggu selain approval |
| **`BD-DEP-008`** | Prefix entity Bank Darah belum terdaftar di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Entity operasional `BLOCKED` (`QBE-MOD-002`, `QBE-MOD-003`) | Pemilik registry engineering | `BD-PH-007`; seluruh task entity `Bbk*` | **`MVP-0` tetap jalan** — `BE-BD-001`, `BE-BD-002`, `BE-BD-014`, `BE-BD-016` memakai prefix `Mst` yang sudah sah |
| `DEC-BD-016` | Persetujuan pemilik Billing atas konteks sumber biaya Bank Darah | Pemilik BillingManagement | Penyerahan biaya ke Billing | Pencatatan tindakan tetap dirancang penuh tanpa penyaluran biaya |
| `OQ-BD-011` | Mekanik label golongan darah | Pemilik proses klinis | Slice label | Pemeriksaan dan validasi golongan darah tetap dirancang penuh |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama | Pemilik proses klinis | `IMPLEMENTATION` aturan per komponen | Titik pemeriksaan kecocokan tetap dirancang |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong | Pemilik proses BDRS | Kegunaan `RETURNED_TO_PROVIDER` | Rancangannya tetap dibuat |
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan per komponen | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian | Nilainya dari konfigurasi katalog; selama kosong gerbang menolak |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru setelah dikoreksi | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi | Konsep catatan koreksi tetap dirancang penuh |
| `OQ-BD-016` | Apakah bukti pendukung koreksi menuntut lampiran berkas | Pemilik proses BDRS | Bentuk kolom bukti pendukung | Dirancang sebagai teks; lampiran kemampuan tersendiri |
| `OQ-BD-017` | Nama peran pemegang `BloodUnit : ResolveNotUsable` | Pemilik proses BDRS | **Satu baris seeder** pada `BE-BD-016` | Butir hak akses sudah terpisah; alurnya pasti |
| `OQ-BD-018` | Apakah hasil bukti kecocokan menggerbang atau sekadar keterangan | Pemilik proses klinis | Penegasan `VAL-BD-079` | Rancangan sudah *fail-closed*; menunggu konfirmasi |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada di repository | Pemilik kebutuhan | Penelusuran bukti ke kebutuhan | Perancangan tetap jalan |

**`DEF-BD-004` sudah tertutup seluruhnya** — keenam wewenangnya dipetakan `DEC-BD-039` sampai
`DEC-BD-044`. Ia tidak lagi menjadi blocker, dan gerbang `G3` pada roadmap revisi 1 dihapus.

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

---

## Bukti yang sudah usang

| Artefak | SHA tercatat | SHA saat ini | Tinjauan dampak yang diperlukan |
| --- | --- | --- | --- |
| `02-existing-capability-map.md` | audit penuh `9522caa` · impact scan `4205d18` | `4205d18` | **Sudah disegarkan.** Impact scan 3 September 2026: dari 24 berkas bukti, hanya `LabOrder.cs` tersentuh dan perubahannya aditif. Nol baris berpindah status |
| `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` | `8b298bb` | `4205d18` | Terbatas pada konfigurasi Laboratorium. Dampaknya menyempit sejak `DEC-BD-018` memisahkan sampel Bank Darah dari sampel Laboratorium |
| `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | `8b298bb` | `4205d18` | Sama seperti di atas. PRD §3 yang menganjurkan memakai model sampel Laboratorium sudah digantikan `DEC-BD-018` |

Frontend `afbb8ab` **tidak berubah**; seluruh bukti frontend tetap sahih.

---

## Artefak yang sudah ada

| Artefak | Keadaan |
| --- | --- |
| `00-interview-decisions.md` | Revisi 9 — `DEC-BD-001`..`044`, `INV-BD-011`..`035`, `AC-BD-001`..`097` |
| `02-existing-capability-map.md` | Revisi 3 — 24 kemampuan, impact scan `4205d18` `CURRENT` |
| `02-requirement-completeness-assessment.md` | Revisi 2 — `BR-BD-020` belum punya rumah slice |
| `01-prerequisite-readiness.md` | Revisi 3 — `BD-DEP-001`..`015` |
| `03-domain-architecture.md` | Revisi 6 — `DOMAIN_ARCHITECTURE_READY`, nol gap terbuka |
| `02-backend-architecture.md` | Kontrak `v4` — 15 tabel `Bbk*`, 3 master `Mst*`, 11 enum |
| `03-frontend-architecture.md` | Kontrak `v4` — 10 layar, peta menu, 21 kewajiban layar |
| `04-prd-to-mvp.md` | Kontrak `v4` — 12 epic, `UAT-01`..`21`, gelombang `MVP-0`..`MVP-4` |
| `data/data-dictionary.md` | Kontrak `v4` |
| `contracts/` (5 berkas) | `v4`, kecuali `integration-contract.md` yang `last_changed_in: v2` karena isinya tidak bergerak |
| `flowcharts/` (7 berkas) | Termasuk `penyimpanan-kantong.md` yang baru pada `v2` |
| `testing/acceptance-test-matrix.md` | Kontrak `v4` — `AC-BD-001`..`097` |
| `roadmap/00-delivery-plan.md` | Revisi 2 — `FORWARD-TEST / DRAFT` |
| `task/report/**` | **Belum ada** — tidak ada implementasi yang pernah dijalankan |

---

## Task berikutnya yang disarankan

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **`G1` approval** — owner menyetujui blueprint dan set kontrak `v4` | Pemilik proses BDRS + arsitektur backend | **Tindakan manusia**, bukan skill |
| 2 | **`BD-DEP-008`** — daftarkan prefix entity di registry kepemilikan modul | Pemilik registry engineering | **Tindakan manusia**, bukan skill |
| 3 | `build-module-backend` per task `MVP-0` | Skill | Hanya setelah `G1`; `MVP-0` tidak menunggu `BD-DEP-008` |
| 4 | `grill-me` bila hendak menutup `OQ-BD-017` dan `OQ-BD-018` sekalian | Skill | Tidak menahan siapa pun |

**`trace-existing-capabilities` sudah dijalankan** 3 September 2026 sebagai impact scan terbatas dan
**tidak perlu diulang** sampai backend SHA bergerak lagi.

`grill-me` untuk keputusan bisnis **tidak** lagi diperlukan pada scope yang dinilai — tidak ada
keputusan bisnis yang masih memblokir.

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
