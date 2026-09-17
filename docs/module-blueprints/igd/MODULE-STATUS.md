# IGD — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Module name | `IGD` / `EmergencyInstallationManagement` |
| Revision | `6` — `draft`. Irisan kontrak yang dibutuhkan `MVP-0`…`MVP-6` sudah `approved` lewat `IGD-DEC-093` dan `IGD-DEC-108`; blueprint secara keseluruhan belum disetujui |
| Module status | `PARTIAL` — pekerjaan berarti masih dapat berjalan (lihat *Next recommended task*), sementara `MVP-6` terblokir. **Dikoreksi 16 September 2026 (ketiga):** kebuntuan kunjungan `Arrived` sudah dibuka. `FE-IGD-030` ✅ terbukti lewat layar — pasien dapat dipindahkan ke penanganan, dialihkan ke Assesmen IGD, dan triage susulannya tidak memundurkan status. `FE-IGD-029` 🟡 sudah terpasang tetapi **belum dibuktikan dengan pendaftaran baru**, jadi hilangnya penolakan `409` pada jalur normal belum diuji ([evidence](evidence/2026-09-16-kunjungan-terjebak-arrived.md)) |
| Current phase | Gelombang `MVP-3`, `MVP-4`, dan `MVP-5` sedang dituntaskan. Modul IGD memakai penomoran gelombang `MVP-0`…`MVP-6`, bukan ID `IGD-PH-*` |
| Last verified at | `16 September 2026` — **uji lewat layar pertama yang pernah dijalankan pada modul ini**, oleh pemilik, untuk `FE-IGD-030`: aksi penanganan segera, pengalihan ke Assesmen IGD, dan triage susulan. Bukan UAT. Sebelumnya `15 September 2026` — pemetaan ulang acceptance criteria ke source tanpa build, test, maupun query basis data ([evidence](evidence/2026-09-15-pemeriksaan-status.md)) |
| Backend source SHA | `e89907c5` (branch `rizkiG`) — tempat pemeriksaan. SHA desain revisi 6 tetap `300922c` |
| Frontend source SHA | `43adae648` (branch `RizkiV2`) — tempat pemeriksaan. SHA desain revisi 6 tetap `96a91201` |

Dokumen ini ringkasan keadaan. Sumber kebenaran status per task tetap
[roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) dan
[roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md).

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `MVP-1`, `MVP-2`, R3.7 | `MVP-0`, `MVP-3`, `MVP-4`, `MVP-5` | `MVP-6` |

| Gelombang | Isi | Status | Yang menahan |
| --- | --- | --- | --- |
| `MVP-0` | Status kunjungan tidak dapat mundur (`BE-IGD-017`…`022`) | `IN_PROGRESS` 🟡 | `BE-IGD-017` tidak punya laporan tracked |
| `MVP-1` | Pendaftaran & encounter `Emergency` (`BE-IGD-023`, `024`) | `DONE` ✅ | — |
| `MVP-2` | Satu pasien satu episode (`BE-IGD-025`) | `DONE` ✅ | — |
| `MVP-3` | Pengkajian IGD tanpa antrean (`BE-IGD-026`…`030`) | `IN_PROGRESS` 🟡 | `BE-IGD-026`: uji langkah mundur migration belum |
| `MVP-4` | Kepergian pasien (`BE-IGD-031`…`034`) | `IN_PROGRESS` 🟡 | `BE-IGD-031`: uji `RENAME` balik belum |
| `MVP-5` | Riwayat dokter & serah terima (`BE-IGD-035`; `EPIC IGD-04`) | `IN_PROGRESS` 🟡 | `BE-IGD-035` kriteria 2 (direncanakan `BE-IGD-041`); `EPIC IGD-04` direncanakan `BE-IGD-044`, `BE-IGD-045`, `FE-IGD-027` — belum dikerjakan |
| R3.7 | Migration, master data pindah modul, kolom respons (`BE-IGD-036`…`038`) | `DONE` ✅ | — |
| `MVP-6` | Kewenangan unit (`BE-IGD-039`) | `BLOCKED` ⛔ | Security/Privacy owner; pemetaan unit 0 dari 18 |

`DONE` di sini berarti seluruh task gelombangnya ✅ menurut roadmap. Status UAT **terpisah** dan
**tidak** ditulis lulus.

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `IN_PROGRESS` — 19 ✅, 4 🟡, 1 ⛔ dari 24 task yang sudah dikerjakan; ditambah 5 task direncanakan 15 Sep 2026 (`BE-IGD-041`…`045`, satu di antaranya ⛔) dan **`BE-IGD-046`** yang direncanakan 16 Sep 2026 — **30 task** | `IN_PROGRESS` — 8 ✅ (termasuk `FE-IGD-019`, `FE-IGD-023`, `FE-IGD-024`), 5 🟡, 1 belum dikerjakan; ditambah 3 task direncanakan (`FE-IGD-025`…`027`), **`FE-IGD-028`** yang direncanakan 16 Sep 2026, serta **`FE-IGD-029`** 🟡 dan **`FE-IGD-030`** ✅ yang direncanakan **dan dikerjakan** 16 Sep 2026 (kedua); ditambah **`FE-IGD-031`** 🟡 dan **`FE-IGD-032`** 🟡 yang direncanakan **dan dikerjakan** 16 Sep 2026 (keempat) — tata letak riwayat pada ruang kerja pemeriksaan; implementasi selesai, lint dan 866 unit test lulus, `npm run build` dan uji layar belum — **22 task** | `PARTIAL` — radiologi ditahan `IGD-DEC-111`; tab laboratorium diperbaiki `FE-IGD-023`; tanda vital observasi menunggu `BE-IGD-046`/`FE-IGD-028`; billing IGD belum direncanakan | `NOT_STARTED` untuk uji lewat layar; alur simpan lewat layar belum pernah dijalankan sejak roadmap revision `1` |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `BE-IGD-039` | Kewenangan unit membandingkan `DepartmentId` dengan `OrganizationUnitId` — tidak pernah benar | Security/Privacy owner (belum ditunjuk) | `MVP-6`; route tulis `order-items`, `arrive`, `accept-handover` | Ya — seluruh gelombang lain dan layar resusitasi tidak tertahan |
| `IGD-DEC-092` (sementara) | Fail-closed + jalan keluar beralasan; jalan keluarnya belum ada di kode | Security/Privacy owner | `MVP-6` | Ya |
| Pemetaan unit | `MstServiceUnit.OrganizationUnitId` 0 dari 18 unit terisi | Master Data (belum ditunjuk) | `MVP-6` | Ya |
| `ActAsRadiologist` | Hasil bacaan radiologi belum dapat dirilis siapa pun (`FE-RAD-11`) | Yoga Aji Pratama — pemilik Radiologi | Penyambungan pemesanan radiologi IGD (`IGD-DEC-111`) | Ya — perbaikan teks layar tidak menunggu |
| `IGD-DEC-100`…`102` | Sikap pesanan, pesanan lab manual, penerimaan per pesanan masih `draft` | Clinical Governance, Nursing authority, pemilik Laboratorium | Butir 10 DoD `EPIC IGD-07` | Ya |
| `IGD-OQ-083` | Tempat menyimpan alasan pembatalan observasi | Product/Domain Owner IGD | Bagian `Cancelled` dari `IGD-DEC-115` | Ya — bagian `Completed` tidak tertahan |
| ~~`EPIC IGD-04`~~ | ~~Riwayat penugasan dokter belum punya task~~ — **ditutup 15 September 2026**: dipecah menjadi `BE-IGD-044`, `BE-IGD-045`, `FE-IGD-027` (`IGD-DEC-116`, `IGD-DEC-117`) | — | `MVP-5` | — |
| OWNER DATA CONFIRMATION | `BE-IGD-042` menunggu jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`; agent dilarang menjalankan kueri | Rizki | R3.8 | Ya — `BE-IGD-040`, `041`, `043` tidak tertahan |
| `IGD-DEC-082` | Riwayat penugasan dokter masih `draft` klinis | Clinical Governance | Butir 10 DoD `EPIC IGD-04` | Ya — pekerjaan `BE-IGD-044`/`045` boleh dimulai |
| ~~Audit Observasi V1–V2~~ | **Ditutup 16 September 2026** — `IGD-OQ-084`…`088` dijawab menjadi `IGD-DEC-122`…`126`; kontrak API dan validation naik ke `0.6.0`; `BE-IGD-046` dan `FE-IGD-028` dialokasikan ([evidence](evidence/2026-09-15-audit-observasi-v1-v2.md)) | — | R3.9, R3.7 frontend | — |
| Bentuk terstruktur alat jalan napas | `IGD-OQ-089` — OPA, NPA, ETT, LMA, stoma, bantuan napas bertekanan belum punya pemilik dan entity | Clinical Governance + Product/Domain Owner IGD | Pelaporan alat jalan napas | Ya — `BE-IGD-046` dan `FE-IGD-028` tidak tertahan |
| Entri susulan setelah periode observasi ditutup | `IGD-OQ-090` — jalur addendum belum dirancang; dilarang menumpang `BE-IGD-046` | Product/Domain Owner IGD + Nursing authority | Dokumentasi susulan observasi | Ya |
| ~~Penyelarasan teks kontrak~~ | ~~API §3, validation §1 aturan 2 dan §6 aturan 4, kamus data §4 belum mengikuti `IGD-DEC-116`…`120`~~ — **ditutup 15 September 2026**: API dan validation naik ke `0.5.0`, nama `EmgDoctorAssignment` diselaraskan, hash dihitung ulang (manifest bagian 0c) | — | R3.8, `MVP-5` | — |

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| `01-existing-capability-map.md` revision `3` + suplemen `3.1` | `f69e9e48` / `300922c` | `e89907c5` | Wajib sebelum gelombang berikutnya menyentuh modul lain; tim Registrasi, Laboratorium, dan Radiologi mengubah source sejak itu |
| ~~`blueprint-manifest.md` `artifact_hashes`~~ | 24 Agustus 2026 | 15 September 2026 | **Ditutup** — dihitung ulang pada pass penyelarasan teks kontrak (manifest bagian 0c) |
| Angka test pada laporan `BE-IGD-018`…`038` | `761 total, 759 lulus` (27 Agt) | Proyek test dihapus 11 Sep | Tidak dapat diulang; sah sebagai bukti historis (`IGD-DEC-110`) |
| ~~Penerapan migration `20260910031500` (rename encounter/resep, milik tim Registrasi)~~ | — | — | **Tidak lagi usang — dikoreksi 15 September 2026.** Migration **sudah diterapkan** pada `QuilvianNewDevRizki`. Bukti dari owner: Rizki menjalankan `dotnet ef migrations list --no-build` pada 15 September 2026, dan tidak ada migration berlabel `Pending`. Agent tidak menjalankan perintah basis data apa pun |

## Next recommended task

> **Urutan delivery ditetapkan Product/Domain Owner 16 September 2026.** Butir 1 dan 2 wajib lulus
> lebih dulu; sesudahnya pekerjaan besar berikutnya adalah `EPIC IGD-04`. Butir lain **tidak boleh**
> didahulukan, dan agent **tidak memulai task lain secara otomatis**.

> **Sisipan 16 September 2026 (keempat).** Pemilik menugaskan gelombang **R3.9 — tata letak
> riwayat pada ruang kerja pemeriksaan**: `FE-IGD-031` lalu `FE-IGD-032`
> ([evidence](evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md), `IGD-DEC-133` dan
> `IGD-DEC-134`). Letaknya **sesudah butir 1** dan **sebelum butir 4** (`EPIC IGD-04`). Keduanya
> nol backend, nol kontrak, dan tidak menunggu siapa pun selain `FE-IGD-028` ✅ yang sudah
> selesai. Butir 1 tetap didahulukan karena tinggal satu klik.
>
> **Diperbarui 16 September 2026 (keempat):** implementasi kedua task **sudah selesai** dan
> berstatus 🟡. Yang menahan keduanya sekarang hanya dua hal, dan keduanya milik Rizki:
> menjalankan `npm run build` di `QuilvianSystemFrontendDev`, lalu membuka layar Assesmen IGD
> untuk mencoba segmen Formulir/Riwayat pada tab Assesmen Awal, dan — untuk `FE-IGD-032` —
> menyelesaikan satu periode observasi beserta isian Kesimpulan serta mencatat satu putaran
> pemantauan bertanda vital. Dua delta pada `FE-IGD-032` menunggu penilaian pemilik; lihat
> [laporan](task/report/frontend/FE-IGD-032.md) bagian 8.

Seluruh task di bawah **sudah punya kartu roadmap**. Urutan yang tidak menunggu pihak lain:

1. **`FE-IGD-029`** 🟡 — satu-satunya sisa gelombang R3.8, dan tinggal **satu klik**. Kriteria 1
   sudah terbukti lewat layar 16 September 2026: pasien yang baru didaftarkan langsung berstatus
   "Menunggu Triage". Kriteria 2 belum — tekan **Isi Triage** pada pasien baru itu lalu simpan
   sampai berhasil; itu membuktikan penolakan `409` yang memicu gelombang ini benar-benar hilang.
   Pasangannya **`FE-IGD-030`** ✅ sudah terbukti penuh.
2. **`BE-IGD-046`** → **`FE-IGD-028`** — pemantauan observasi bertanda vital (`IGD-DEC-122`…`126`,
   kontrak `0.6.0`). Keputusan dan kontraknya sudah terkunci 16 September 2026; implementasinya
   sudah ditulis dan tinggal uji lewat layar.
3. ~~**`BE-IGD-041`**~~ 🟡 — **dikerjakan 16 September 2026.** Pesan penutupan kini menyebut
   pesanan yang menahannya, dan kriteria 2 `BE-IGD-035` terpenuhi pada source. Tersisa
   `dotnet build` dan uji API tiga skenario ([laporan](task/report/backend/BE-IGD-041.md)).
4. **`BE-IGD-044`** → **`BE-IGD-045`** → **`FE-IGD-027`** — riwayat penugasan dokter
   (`EPIC IGD-04`). **Pekerjaan besar berikutnya menurut keputusan owner 16 September 2026**,
   dimulai setelah butir 1 dan 2 lulus. Pada `BE-IGD-044` agent menyiapkan model, konfigurasi EF,
   kebutuhan service, dan kebutuhan migration; **agent tidak menjalankan migration** — Rizki
   menjalankannya sendiri setelah review. Kartu ketiganya sudah ditinjau ulang 16 September 2026
   terhadap `IGD-DEC-116`/`117` dan kontrak terbaru; hasilnya di bagian *Tinjauan kesiapan
   `EPIC IGD-04`* di bawah.
5. **Laporan susulan:** `BE-IGD-017`, `BE-IGD-043` (`f76ebaab`), `FE-IGD-025` (`bd1d94a8a`,
   termasuk temuan privasi `IGD-EV-117`), `FE-IGD-026` (`c8613d88c`).
6. **Uji langkah mundur migration** `20260826090500` di basis data terpisah — membuka
   `BE-IGD-026` dan `BE-IGD-031`. Butuh basis data terpisah milik Rizki.
7. **Backlog kecil, sengaja tidak didahulukan** (keputusan owner 16 September 2026):
   `FE-IGD-017` (pelaku kepergian tampil sebagai UUID) dan `FE-IGD-014` kriteria 2 — keduanya
   `IGD-EV-123`. Dikerjakan setelah `EPIC IGD-04`, bukan sebelum.

Sudah selesai implementasinya dan tinggal menunggu build serta uji lewat layar: `FE-IGD-023`,
`BE-IGD-040`, dan `FE-IGD-024`.

Yang **menunggu owner**: `BE-IGD-042` — jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`
dari kueri yang dijalankan Rizki sendiri.

Gap yang **tidak** diberi ID atas instruksi owner: layar resusitasi IGD (`IGD-EV-111`) dan layar
baca/aksi `order-items` (`IGD-EV-109`).

**Ditegaskan ulang 16 September 2026** sesudah `BE-IGD-041` membuat penolakan penutupan menyebut
pesanan yang menahannya: gap aksi `order-items` **tetap tidak dibuatkan task implementasi**. Ia
dicatat sebagai terblokir dengan dependency `BE-IGD-039`. Layar baca-saja **juga tidak**
diprioritaskan, karena menampilkan daftar pesanan tanpa aksi belum memberi petugas jalan
penyelesaian — hanya memindahkan kebuntuan dari pesan galat ke layar.

## Tinjauan kesiapan `EPIC IGD-04` — 16 September 2026

Dijalankan atas perintah owner sebelum coding dimulai. Ketiga kartu `BE-IGD-044`, `BE-IGD-045`,
dan `FE-IGD-027` ditinjau terhadap `IGD-DEC-116`, `IGD-DEC-117`, dan kontrak `0.5.0`.
**Nol requirement didesain ulang.**

### Ketujuh pertanyaan audit owner terjawab kontrak yang sudah terkunci

| # | Pertanyaan | Dijawab oleh | Keadaan |
| ---: | --- | --- | :-: |
| 1 | Siapa dokter penanggung jawab saat ini | `GET /active` tanpa `at` (API §3); baris dengan `EffectiveTo IS NULL`, dijaga unique bersyarat | ✅ |
| 2 | Sejak kapan | Kolom `EffectiveFrom` (kamus data §4) | ✅ |
| 3 | Siapa dokter sebelumnya | `GET /` riwayat urut waktu (`BE-IGD-045` acceptance 8); baris lama tidak pernah ditimpa | ✅ |
| 4 | Kapan pengalihan terjadi | `EffectiveTo` baris lama ditutup bersamaan dengan `EffectiveFrom` baris baru dalam satu transaksi (acceptance 3) | ✅ |
| 5 | Siapa yang mengalihkan | `AssignedByUserId` pada baris baru, diisi sistem dari pengguna aktif | ✅ |
| 6 | Alasan pengalihan | `AssignmentReason` (500), wajib saat pengalihan (acceptance 4) | ✅ |
| 7 | Dokter pada waktu tertentu | `GET /active?at={datetime}` (`IGD-DEC-117`, API §3.1, acceptance 9) | ✅ |

### Empat hal yang perlu diputuskan — **SELURUHNYA DIPUTUSKAN OWNER 16 September 2026**

| No | Temuan | Keputusan |
| ---: | --- | --- |
| 1 | **`BE-IGD-045` tidak menjanjikan nama.** `FE-IGD-027` acceptance 6 menuntut dokter dan penugas tampil sebagai **nama**, bukan ID (`IGD-EV-123`), tetapi acceptance `BE-IGD-045` tidak menyebut proyeksi nama sama sekali. Bila backend hanya mengirim `DoctorId` dan `AssignedByUserId`, frontend tidak dapat memenuhi kriterianya tanpa panggilan tambahan | **`IGD-DEC-129` — DISETUJUI.** Proyeksi aditif `doctorName` dan `assignedByName` pada `GET /` dan `GET /active`; `doctorId` dan `assignedByUserId` tetap dikirim. Satu kueri berproyeksi, nol `N+1`, nol kolom baru. API naik `0.7.0` bagian 3.2; acceptance `BE-IGD-045` bertambah butir 12 |
| 2 | **`IsActive` dan `EffectiveTo` sama-sama menyatakan "aktif".** Kamus data §4 memuat keduanya, sedangkan unique bersyarat hanya menjaga `EffectiveTo IS NULL` | **`IGD-DEC-130` — DIPUTUSKAN.** `IsActive` **dihapus dari rancangan sebelum model dan migration dibuat**. Penugasan berjalan = `EffectiveTo IS NULL`; pada waktu `T` = `EffectiveFrom <= T AND (EffectiveTo IS NULL OR T < EffectiveTo)`. Kamus data §4 sudah diselaraskan; acceptance `BE-IGD-044` butir 2 dan 3 serta `BE-IGD-045` butir 13 mengikuti |
| 3 | **`BE-IGD-045` menuntut baris baru di `Program.cs`.** Service dan controller `EmergencyDoctorAssignment` sepenuhnya baru, jadi pendaftaran DI tidak terhindarkan. Kartunya sudah mensyaratkan agent berhenti dan meminta persetujuan owner | **`IGD-DEC-131` — DISETUJUI TERBATAS.** Hanya untuk pendaftaran DI service Emergency Doctor Assignment yang memang baru; dilarang cleanup atau refactor DI lain; baris pendaftarannya wajib dicantumkan pada laporan task |
| 4 | **Kontrak menyebut nama tabel yang sudah diganti.** API §3 masih menulis *"memperbarui `TrxPatientEncounter.DoctorId`"*, padahal tabel itu sudah menjadi `RegPatientEncounter` sejak `58c61a5b` (10 September 2026) | **`IGD-DEC-132` — DISETUJUI.** Kontrak memakai `RegPatientEncounter`. Sudah diterapkan pada API §3 dan integration contract. ERD dan kamus data §5.3 **masih** memakai nama lama dan berada di luar lingkup keputusan ini |

### Yang sudah siap dan tidak perlu dipertanyakan lagi

Pola reuse `BE-IGD-044` terbukti ada: `InPatientManagement/Models/InpDoctorAssignment.cs` beserta
`Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs`.
Hak akses `EmergencyDoctorAssignment` sudah terdaftar pada permission matrix baris 41 dengan
`Read`, `Create`, dan `Update`. Nama tabel, kolom, index, dan unique bersyarat sudah terkunci
`IGD-DEC-116` dan kamus data §4.

### Gerbang yang tetap terbuka

`IGD-DEC-082` masih `draft` menunggu Clinical Governance. Itu menahan **butir 10 Definition of
Done** `BE-IGD-045`, **bukan** dimulainya pekerjaan. `BE-IGD-044` acceptance 5 menuntut langkah
mundur migration diuji di **basis data terpisah** — milik Rizki, dan menjadi syarat ✅ task itu.

## Backlog lintas modul — `ClinicalManagement`

Ditemukan pada uji layar `FE-IGD-028` 16 September 2026, dan **atas keputusan owner tidak masuk
lingkup `EPIC IGD-04`**. Keduanya milik formulir tanda vital bersama, bukan modul IGD.

| Temuan | Isi | Bukti |
| --- | --- | --- |
| Jenis oksigen bertentangan dengan laju alir | Baris tersimpan dengan penanda memakai oksigen bernilai benar, jenis "tidak menggunakan", dan laju `50 L/menit`. Riwayat menampilkan *"Oksigen: Tidak menggunakan · 50 L/menit"* — tampilannya benar, isiannya yang longgar | Laporan `FE-IGD-028` butir (4) |
| Nilai tanda vital ekstrem diterima tanpa peringatan | Nadi `250 x/menit` dan frekuensi napas `80 x/menit` tersimpan. Penanda *"Tanda Vital Kritis"* menyala, tetapi nol batas kewajaran yang menahan salah ketik | Laporan `FE-IGD-028` butir (5) |

## Optional deterministic delivery progress

| Lapisan | Rumus | Hasil |
| --- | --- | --- |
| Backend | task ✅ / seluruh task roadmap | **18 / 29 = 62%** (per 15 September 2026, sesudah perencanaan). Tidak dikecualikan: `BE-IGD-039` dan `BE-IGD-042` (⛔) tetap dihitung di penyebut |
| Frontend | task ✅ / seluruh task roadmap | **6 / 17 = 35%**. `FE-IGD-019` kini dihitung |

Persentase turun dari 78%/45% bukan karena ada yang mundur, melainkan karena penyebutnya
bertambah task baru. Tiga area R3.5 (penunjang medis, pemakaian alat, billing IGD) **tidak**
masuk penyebut karena belum punya task. Persentase ini bukan ukuran kesiapan produksi.

## Status contract

`DRAFT` has identity but incomplete intake. `DISCOVERY` is collecting decisions/evidence. `READY` means planned phases may start. `PARTIAL` means at least one phase is ready while another is blocked or unknown. `BLOCKED` means no material phase can safely proceed. `IN_PROGRESS` has authorized active work. `VERIFYING` awaits readiness evidence. `DONE` requires appropriate verification evidence. `SUPERSEDED` records the successor blueprint.

Phase statuses are `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, and `SUPERSEDED`. A phase becomes `DONE` only when its acceptance/readiness evidence is recorded; file existence is insufficient.
