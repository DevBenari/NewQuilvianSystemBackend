# Laporan Perubahan Backend — `BE-BKC-052`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-052` |
| **Judul** | Aktivasi: pengisian aturan tanggungan per perusahaan penjamin |
| **Slice** | `MVP-16` (fondasi) — eksekusi gelombang 3 |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1892 |
| **Trace** | `MPY-OQ-006`; § "Rencana data master awal" pada [`02-backend-architecture.md`](../../../02-backend-architecture.md) |
| **Contract version** | `NOT APPLICABLE` — task ini tidak membuat maupun mengubah endpoint atau kontrak API apa pun |
| **Dependency** | `BE-BKC-043` (Master aturan tanggungan perusahaan penjamin) — ✅ Selesai, layar dan endpoint aturan tanggungan sudah tersedia |
| **Klasifikasi** | `LIGHT` — bukan task implementasi source (0 repository source disentuh untuk perubahan kode; 1 dokumen roadmap dan 1 laporan tracked ditulis); tidak ada dampak kontrak API, skema database, atau keamanan; task bersifat koordinasi pengisian data master lintas peran (Admin Master Data, Finance) |
| **Task mode** | Task ini secara eksplisit **bukan task source** (lihat kolom "Kontrak" dan "Scope" pada kartu roadmap). Sesi ini menjalankan verifikasi bukti read-only atas kartu task dan source terkait, **tanpa build program** sesuai instruksi eksplisit pengguna, dan menuliskan/menyegarkan laporan tracked ini karena laporan tersebut belum pernah dibuat meski status interim sudah tercatat di roadmap sejak 12 September 2026 |
| **Target tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` — terbatas pada laporan tracked ini; **nol perubahan source** |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `e364fc411127f9cde2e7a5819f6f136b44a76cb6` (branch `Yasmina`) |
| **Tanggal** | 14 September 2026 |
| **Status** | 🟡 **Sebagian.** Data placeholder pengembangan sudah terpasang sejak 12 September 2026, tetapi acceptance criteria task ini belum terbukti penuh — nilai kontrak riil dan approval Finance masih ditunggu, dan sesi ini tidak memiliki wewenang untuk mengeksekusi query verifikasi ke database maupun mengarang nilai kontrak bisnis |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `Administrator` dan `HealthServices` (referensi saja — task ini tidak menulis source) |
| **Module / Submodule** | `Administrator / MasterData` (`MstCompanyGuarantorReimbursementRoute`), `HealthServices / MasterData` (`MstCompanyGuarantorCoverageRule`) |
| **Owner / Prefix Registry** | Prefix `Mst` — `Administrator / HealthServices / MasterData`, Category: `BUSINESS DOMAIN / MASTER / REFERENCE`, Status: `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) — sudah terdaftar sejak `BE-BKC-041` |
| **Keberlakuan** | `NOT APPLICABLE` — task ini adalah aktivasi/pengisian data master, bukan `NEW CODE`, `TOUCHED LEGACY`, maupun `LEGACY MIGRATION`. Tidak ada model, controller, atau migration baru pada task ini |
| **QBE ID yang berlaku** | Tidak ada QBE ID rekayasa yang relevan karena tidak ada source yang disentuh. Governance yang relevan adalah `AGENTS.md` bagian Keselamatan Database — eksekusi/kueri database memerlukan wewenang eksplisit terpisah, yang belum diberikan pada task ini |
| **Pengecualian / Temuan** | Sesuai instruksi eksplisit pengguna (*"lakukan tanpa build program"*), `dotnet build` tidak dijalankan pada task ini. Ini konsisten dengan sifat task yang memang tidak menyentuh source, sehingga build tidak relevan dan tidak diwajibkan `TEST_POLICY.md`/`REVIEW_RULES.md` untuk task tanpa perubahan source |

---

## 1. Masalah yang diperbaiki

Fitur mesin tanggungan perusahaan penjamin (`CompanyGuarantorCoverageService`, dibangun `BE-BKC-044`) dan seluruh CRUD master pendukungnya (`BE-BKC-042`, `BE-BKC-043`) sudah selesai secara teknis. Namun kode yang berfungsi saja tidak cukup: setiap perusahaan penjamin aktif **harus** memiliki minimal satu baris aturan tanggungan (`MstCompanyGuarantorCoverageRule`) dan satu rute reimbursement (`MstCompanyGuarantorReimbursementRoute`) sebelum fitur ini dipakai pasien sungguhan.

Contoh konkret risikonya: bila perusahaan penjamin "PT Sejahtera Abadi" aktif di sistem tetapi belum punya baris aturan tanggungan sama sekali, maka setiap kunjungan karyawan perusahaan itu akan dihitung mesin tanggungan sebagai **0% ditanggung perusahaan** — bukan karena bug, melainkan karena memang tidak ada aturan yang bisa dipakai mesinnya. Akibatnya pasien tertagih penuh padahal seharusnya ditanggung sebagian atau seluruhnya oleh perusahaan. Ini murni masalah kelengkapan data master, bukan masalah kode.

Sebagai langkah sementara agar fitur dapat diuji ujung-ke-ujung tanpa selalu jatuh ke nol, pada 12 September 2026 pengguna telah menjalankan dua statement insert manual (60 baris total) berisi data **placeholder pengembangan** — bukan nilai kontrak kerja sama yang sebenarnya — untuk kelima perusahaan penjamin yang saat ini aktif.

---

## 2. Proses bisnis

**Tujuan:** memastikan tidak ada perusahaan penjamin aktif yang "tanggungannya nol" akibat data master belum lengkap, sebelum fitur multi-payer perusahaan diaktifkan untuk pemakaian sungguhan.

**Pelaku:** Product/Domain Owner, Admin Master Data, dan Finance (persis seperti dicatat pada `MPY-OQ-006` dan `requirement-traceability.md`). Bukan task implementer/engineering.

**Pemicu:** Seluruh CRUD master pendukung (`BE-BKC-042`, `BE-BKC-043`) dan mesin kalkulasi (`BE-BKC-044`) sudah selesai dan dapat dipakai.

**Langkah berurutan yang seharusnya terjadi:**

1. Admin Master Data mengambil nilai kontrak kerja sama riil per perusahaan penjamin (persentase tanggungan per kategori tarif/obat/tindakan, golongan karyawan, dan rute reimbursement — `SELF` atau lewat asuransi mitra).
2. Admin Master Data mengisi baris aturan tanggungan (`MstCompanyGuarantorCoverageRule`) dan rute reimbursement (`MstCompanyGuarantorReimbursementRoute`) lewat layar yang sudah dibangun `BE-BKC-042`/`BE-BKC-043` — **bukan** lewat insert manual ke database.
3. Finance mereview daftar akhir aturan tanggungan per perusahaan dan menyetujuinya.
4. Dua query verifikasi dijalankan: hitung perusahaan penjamin aktif tanpa aturan tanggungan, dan hitung perusahaan penjamin aktif tanpa rute reimbursement. Keduanya **wajib** bernilai nol.
5. Fitur baru dianggap layak diaktifkan untuk pemakaian sungguhan.

**Jalur tidak normal yang sedang terjadi saat ini:** langkah 1–3 belum selesai. Sebagai gantinya, pengguna (bukan Admin Master Data lewat layar resmi) menjalankan insert manual berisi data placeholder (`RuleCode`/`RuleName`/`Description` ditandai `[DEV PLACEHOLDER]`, `CoveragePercent=100` seragam untuk seluruh kategori tarif, `RouteType=SELF` seragam) semata-mata agar jalur kode dapat diuji. Data ini **secara eksplisit bukan** nilai kontrak riil dan **tidak boleh** dipakai untuk penagihan pasien sungguhan.

**Hasil akhir saat ini:** task tetap terbuka. Query verifikasi kemungkinan besar akan mengembalikan nol untuk kelima perusahaan yang sudah diisi placeholder, tetapi nilai substansinya belum benar secara bisnis dan belum direview Finance, sehingga acceptance criteria task ini belum dapat dinyatakan terpenuhi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Membaca kartu task `BE-BKC-052`, status interim, dan grafik dependency |
| `docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md` | Membaca jejak `MPY-OQ-006` dan catatan interim 12 September 2026 |
| `docs/module-blueprints/billing-kasir/04-prd-to-mvp.md` | Memeriksa definisi `MPY-OQ-006` sebagai open question yang memblokir aktivasi, bukan pembangunan |
| `docs/module-blueprints/billing-kasir/00-interview-decisions.md` | Memeriksa keputusan bisnis terkait `MPY-OQ-006` |
| `docs/module-blueprints/billing-kasir/blueprint-manifest.md` | Memeriksa status blocking question `MPY-OQ-006` pada manifest blueprint |
| `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` | Memeriksa kolom `IsActive`/`IsDelete` sebagai definisi "perusahaan penjamin aktif" untuk query verifikasi |
| `Areas/HealthServices/MasterData/Models/MstCompanyGuarantorCoverageRule.cs` | Memeriksa kolom kunci `CompanyGuarantorId` untuk query verifikasi aturan tanggungan |
| `Areas/Administrator/MasterData/Models/MstCompanyGuarantorReimbursementRoute.cs` | Memeriksa kolom kunci `CompanyGuarantorId` untuk query verifikasi rute reimbursement |
| `Areas/HealthServices/ClinicalManagement/Services/CompanyGuarantorCoverageService.cs` | Memastikan mesin tanggungan yang bergantung pada data ini memang sudah ada (`BE-BKC-044`) |
| `Areas/Administrator/MasterData/Controllers/CompanyGuarantorReimbursementRouteController.cs` | Memastikan layar resmi pengisian rute reimbursement sudah tersedia (`BE-BKC-042`) |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Konfirmasi status registry prefix `Mst` masih `ACTIVE` |
| `git status --short`, `git diff --stat`, `git log -1` pada repository backend | Menentukan state Git saat ini dan memastikan tidak ada perubahan sampingan dari task ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-052.md` | **Baru.** Laporan tracked task ini, belum pernah ada sebelumnya walau status interim sudah tercatat di roadmap sejak 12 September 2026 |

Tidak ada berkas source aplikasi (`Areas/`, `Controllers/`, `DTOs/`, `Models/`, `Services/`, `Repositories/`) yang diperiksa maupun diubah isinya pada task ini, sesuai kolom "Scope" kartu roadmap yang menyatakan "Nol perubahan source".

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint dibuat atau diubah |
| Database | `NOT APPLICABLE` untuk sesi ini — tidak ada perubahan schema/migration. **Catatan penting:** data baris (60 baris insert manual placeholder) telah dimasukkan oleh pengguna secara terpisah pada 12 September 2026 ke tabel `MstCompanyGuarantorReimbursementRoute` dan `MstCompanyGuarantorCoverageRule`; sesi ini tidak mengeksekusi maupun memverifikasi data tersebut langsung ke database karena eksekusi/kueri database memerlukan wewenang eksplisit terpisah yang tidak diberikan pada task ini |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint apa pun. Endpoint CRUD yang dipakai untuk pengisian data (`Company Guarantor Reimbursement Route`, `Company Guarantor Coverage Rule`) sudah didokumentasikan pada laporan `BE-BKC-042` dan `BE-BKC-043`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Task tidak menyentuh source (lihat kartu roadmap "Nol perubahan source"); pengguna secara eksplisit meminta *"lakukan tanpa build program"* pada sesi ini |
| Query hitung perusahaan penjamin aktif tanpa aturan tanggungan (`MstCompanyGuarantorCoverageRule`) — **wajib nol** sebelum aktivasi | Tidak dijalankan pada sesi ini | `NOT RUN` | Eksekusi query terhadap database memerlukan wewenang terpisah sesuai `AGENTS.md` bagian Keselamatan Database; sesi ini tidak diberi wewenang tersebut |
| Query hitung perusahaan penjamin aktif tanpa rute reimbursement (`MstCompanyGuarantorReimbursementRoute`) — **wajib nol** sebelum aktivasi | Tidak dijalankan pada sesi ini | `NOT RUN` | Sama seperti di atas |
| Review daftar akhir aturan tanggungan oleh Finance | Belum dilakukan | `NOT RUN` | Bukti persetujuan Finance belum ada pada dokumen blueprint mana pun yang diperiksa |
| Konfirmasi data yang terpasang saat ini bersifat placeholder, bukan kontrak riil | Terkonfirmasi lewat narasi roadmap dan `requirement-traceability.md` | `PASS` | `backend-roadmap.md` baris 1907 dan `requirement-traceability.md` baris 445, keduanya menandai `[DEV PLACEHOLDER]` secara eksplisit |
| `git status --short` pada repository backend | `M Migrations/20260910153119_AddBbkBloodOrder.cs` (pekerjaan lain, tidak disentuh task ini), `M docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` (perubahan status interim yang sudah ada sebelum task ini dimulai) | `PASS` (tidak ada perubahan sampingan baru dari task ini) | Keluaran perintah, lihat bagian 7 |

Uji manual: `NOT APPLICABLE` — tidak ada UI atau endpoint baru untuk diuji manual pada task ini.

**Tidak dijalankan:** `dotnet build` dan kedua query verifikasi database sengaja tidak dijalankan karena task ini tidak mengubah source (build tidak relevan) dan eksekusi database memerlukan otorisasi eksplisit terpisah yang belum diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nol perusahaan penjamin aktif yang tidak punya aturan tanggungan | **Belum terbukti** | Data placeholder sudah terpasang untuk kelima perusahaan aktif saat ini (`backend-roadmap.md` baris 1907), tetapi query verifikasi belum dijalankan pada sesi ini dan tidak ada bukti tertulis hasil query tersebut di dokumen manapun |
| Nol perusahaan penjamin aktif yang tidak punya rute reimbursement | **Belum terbukti** | Sama seperti di atas — 5 baris `RouteType=SELF` `IsDefault=true` sudah diinsert manual, tetapi belum diverifikasi ulang lewat query pada sesi ini |
| Query verifikasi mengembalikan nol (DoD) | **Belum terpenuhi** | Query belum dijalankan pada sesi ini; wewenang eksekusi database terpisah belum diberikan |
| Daftar perusahaan beserta aturannya direview Finance (DoD) | **Belum terpenuhi** | Tidak ada bukti persetujuan Finance pada dokumen blueprint manapun yang diperiksa |

Butir yang belum terpenuhi disebabkan oleh dua blocker non-teknis yang berada di luar wewenang task backend ini: (1) nilai kontrak kerja sama riil per perusahaan penjamin belum diserahkan Admin Master Data, dan (2) review/approval Finance atas daftar akhir belum dilakukan. Task ini **tidak dapat** ditandai selesai dengan mengarang nilai kontrak atau menyimpulkan approval Finance yang tidak ada buktinya.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Data yang saat ini ada di tabel `MstCompanyGuarantorReimbursementRoute` dan `MstCompanyGuarantorCoverageRule` untuk kelima perusahaan aktif bersifat **placeholder pengembangan**, bukan nilai kontrak riil. **Jangan** memakainya untuk penagihan pasien sungguhan sebelum diganti nilai asli dan disetujui Finance |
| Masalah yang diketahui | Insert data dilakukan manual langsung ke database oleh pengguna pada 12 September 2026, bukan lewat layar CRUD resmi (`BE-BKC-042`/`BE-BKC-043`). Ini konsisten dengan sifat task sebagai pengisian data uji sementara, tetapi berarti jejak audit `IdentityModel` (`CreatedBy`, dsb.) pada 60 baris tersebut kemungkinan tidak mencerminkan alur normal aplikasi |
| Risiko tersisa | Selama task ini belum ditutup dengan nilai kontrak riil dan approval Finance, mengaktifkan fitur multi-payer perusahaan untuk pemakaian sungguhan berisiko menagih pasien dengan persentase tanggungan yang salah (seragam 100% untuk semua kategori, alih-alih sesuai kontrak per perusahaan yang sesungguhnya bervariasi) |
| Perubahan sampingan | `NONE` — laporan ini hanya menambah satu berkas dokumentasi tracked baru; tidak ada berkas source yang disentuh |
| Interupsi | `NONE` |
| Status Git | ```M Migrations/20260910153119_AddBbkBloodOrder.cs``` (perubahan pekerjaan lain yang sudah ada sebelum task ini dimulai, tidak disentuh) dan ```M docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md``` (perubahan status interim 12 September 2026 yang sudah ada sebelum task ini dimulai) — plus penambahan berkas laporan ini dan pembaruan `requirement-traceability.md` yang menyertai penutupan task ini |
| Langkah berikutnya | (1) Admin Master Data menyerahkan nilai kontrak kerja sama riil per perusahaan penjamin aktif; (2) nilai tersebut diinput ulang lewat layar resmi `BE-BKC-042`/`BE-BKC-043`, menggantikan seluruh baris `[DEV PLACEHOLDER]`; (3) Finance mereview dan menyetujui daftar akhir; (4) jalankan ulang kedua query verifikasi dengan wewenang database eksplisit dan catat hasil sebenarnya; (5) baru task ini ditandai ✅ pada roadmap |
