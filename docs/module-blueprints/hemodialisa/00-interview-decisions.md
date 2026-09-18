# Hemodialisa — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Revision | `13` |
| Status | `approved` |
| Pass | `Scope pass` — **selesai** 18 September 2026 |
| Cakupan wawancara | **Phase 1 saja** — 25 Feature. Ditetapkan `HMD-DEC-001` |
| `blueprint_shape` | **`SINGLE`** — ditetapkan `HMD-DEC-004` |
| `shape_decided_by` | `USER_CONFIRMED` |
| Product/domain owner | **Muhammad Hamzah** — ditetapkan `HMD-DEC-006`, 18 September 2026 |
| Implementation owner | **Muhammad Hamzah** — ditetapkan `HMD-DEC-006`, 18 September 2026 |
| Registry owner | **Muhammad Hamzah** — sudah tercatat di registry sebelum sesi ini, lihat `HMD-FACT-019` |
| Clinical governance authority | **BELUM DITUNJUK** — badan klinis rumah sakit. `OPEN`, menjadi **syarat go-live**, bukan syarat desain (`HMD-DEC-010`) |
| Pemegang akun tata kelola klinis | **Muhammad Hamzah** — sementara (*acting*), `HMD-DEC-011`. Memegang hak aksesnya, **tidak** menetapkan kebijakan klinisnya |
| Backend SHA | `190c91a0` (branch `MHamzah`). Wawancara dijalankan pada `69b256ca`; impact scan 18 September 2026 menyatakan seluruh keputusan tetap berlaku — perubahannya hanya dokumen |
| Frontend SHA | `a38683142` (branch `HamzahV2`). Wawancara dijalankan pada `8143874d8`; impact scan memperbarui dua temuan frontend tanpa membatalkan keputusan mana pun |
| Tanggal sesi | 18 September 2026 |
| Capability map | `01-existing-capability-map.md` revision 2, audit pada BE `69b256ca` + FE `8143874d8`, disegarkan lewat impact scan ke `190c91a0` + `a38683142` |
| Requirement completeness | `02-requirement-completeness-assessment.md` `HMD-RCG-001` r3, **`READY_FOR_DOMAIN_DESIGN`** — 12 dari 12 slice |
| Desain | **Disetujui** 18 September 2026 — `HMD-CONTRACT-v1`, 21 berkas, status `approved` oleh Muhammad Hamzah |
| Task mode | `MODULE BLUEPRINT MODE` — hanya `docs/module-blueprints/**` yang boleh ditulis |

### Dokumen masukan

| Dokumen | Fase | Status dokumen | Baseline BE | Baseline FE | Dipakai sesi ini? |
|---|---|---|---|---|---|
| `PRD TO MVP—Hemodialisa Quilvian V2.md` | Phase 1 — Core | `DRAFT — GRILL READY` | `896014ec` | `c42d6ab2` | **Ya** |
| `PRD Phase 2 — ... Longitudinal, Pelaporan & Interoperabilitas.md` | Phase 2 | `DRAFT — GRILL READY / FUTURE RELEASE` | `69b256ca` | `8143874d8` | Tidak — dibaca sebagai konteks, di-*grill* terpisah nanti |
| `PRD Phase 3 — ... Administrative Optimization & Quality Management.md` | Phase 3 | `DRAFT — GRILL READY / FUTURE OPTIMIZATION` | `69b256ca` | `8143874d8` | Tidak — dibaca sebagai konteks, di-*grill* terpisah nanti |

Ketiganya berada di `NewQuilvianSystemBackend/docs/Modul-RS/Hemodialisa/`. Ketiganya sudah
dibaca lengkap pada sesi ini; Phase 2 dan Phase 3 dipakai hanya untuk mengenali titik sentuh
yang harus disiapkan Phase 1, bukan untuk mengunci keputusannya.

> **Cara membaca dokumen ini.**
> Dokumen ini adalah catatan wawancara. Dokumen ini **bukan** desain, **bukan** rencana kerja,
> dan **bukan** izin menulis kode.
>
> | Jenis | Artinya |
> |---|---|
> | **Fact** | Fakta yang dibuktikan langsung dari source code atau dokumen yang sudah disetujui |
> | **Decision** | Keputusan manusia yang berwenang |
> | **Assumption** | Dugaan yang belum dikonfirmasi siapa pun |
> | **Conflict** | Dua sumber yang saling bertentangan dan harus diselesaikan |
> | **Open Question** | Pertanyaan yang masih menunggu jawaban pemilik proses |

---

## Peringatan Prasyarat

Enam hal berikut wajib dibaca sebelum isi dokumen ini dipakai untuk apa pun.

### 1. Scope dikunci sebelum audit, lalu audit menyusul — dan hasilnya sejalan

Batas scope pada dokumen ini dikunci lewat `HMD-DEC-001` dan `HMD-DEC-003` **sebelum** audit
kemampuan existing dijalankan. Audit itu kemudian dijalankan pada hari yang sama dan hasilnya
tidak membatalkan satu pun keputusan scope: `01-existing-capability-map.md` revision 1
menemukan **nol duplikasi** — tidak ada tabel atau endpoint Hemodialisa yang ternyata sudah
pernah dibuat modul lain.

Audit itu justru menambah tiga temuan yang mengubah perkiraan pekerjaan, dan ketiganya wajib
dibaca sebelum desain dimulai:

| Temuan | Ringkas |
|---|---|
| Temuan Kritis 1 | Menambah `ClinicalDocumentKind.HemodialysisSession = 14` **tidak cukup**. Ada daftar tertutup kedua, `JenisYangDitegakkan`, yang menentukan apakah penguncian benar-benar ditegakkan. Bila terlewat, sesi terlihat final tetapi tetap bisa disunting |
| Temuan Kritis 2 | Hasil laboratorium hanya bisa dibaca per pesanan atau per spesimen — **tidak ada pembacaan per pasien**. Hemodialisa hanya tahu pasiennya. Ini dependency pada Laboratorium |
| Temuan Kritis 3 | Data kewenangan klinis petugas kaya dan lengkap, tetapi belum pernah dibaca modul klinis mana pun. Hemodialisa akan jadi yang pertama |

### 2. PRD Phase 1 ditulis pada snapshot source yang sudah kedaluwarsa

| Dokumen | Baseline BE | BE saat wawancara | Baseline FE | FE saat wawancara | Masih segar? |
|---|---|---|---|---|---|
| Phase 1 | `896014ec` | `69b256ca` | `c42d6ab2` | `8143874d8` | **Tidak** |
| Phase 2 | `69b256ca` | `69b256ca` | `8143874d8` | `8143874d8` | Ya |
| Phase 3 | `69b256ca` | `69b256ca` | `8143874d8` | `8143874d8` | Ya |

Klaim PRD Phase 1 tentang keadaan source yang tidak diverifikasi ulang pada dokumen ini
berstatus belum tentu masih benar. Dua belas klaim terpentingnya sudah diverifikasi ulang —
lihat `HMD-FACT-001` sampai `HMD-FACT-012`.

### 3. PRD masukan belum berstatus disetujui

Ketiganya berstatus `DRAFT`, dan letaknya di `docs/Modul-RS/`, bukan di
`docs/module-blueprints/`. Dalam alur kerja Quilvian ketiganya berkedudukan sebagai **bukti
masukan (evidence)**, sejajar dengan SOP atau notulen rapat — bukan blueprint yang sudah
disahkan, dan bukan keluaran `design-business-module`.

Keputusan yang dikunci di dalamnya — `HD-DEC-001` sampai `HD-DEC-017` pada Phase 1 — diperlakukan
di sini sebagai **usulan yang perlu ditegaskan pemiliknya**, bukan keputusan yang sudah mengikat.

### 4. Modul ini belum boleh dipakai pasien sungguhan

Ini peringatan terpenting pada dokumen ini, dan sengaja ditaruh di depan.

Kebijakan klinis Hemodialisa **belum disahkan siapa pun**. Badan klinis rumah sakit yang
berwenang menetapkannya belum ditunjuk (`HMD-OQ-000`). Desain dan implementasi boleh berjalan
penuh — itulah isi `HMD-DEC-010` — tetapi dengan satu syarat yang tidak boleh dilupakan:

> **Hemodialisa tidak boleh `go-live` melayani pasien sungguhan sebelum badan klinis rumah
> sakit mengesahkan tiga hal: batas *override* Pra-HD, kewenangan finalisasi sesi, dan
> kewenangan menolak permintaan HD.**

Sampai saat itu, ketiganya berjalan memakai **aturan bawaan paling aman** yang dicatat sebagai
`HMD-ASM-001` sampai `HMD-ASM-003`. Aturan bawaan itu **bukan** keputusan klinis yang sudah
sah. Ia hanya cara agar sistem tetap aman ketika kebijakannya belum ada.

Muhammad Hamzah tercatat sebagai **pemegang akun** tata kelola klinis sementara
(`HMD-DEC-011`). Artinya ia memegang hak aksesnya di dalam sistem supaya alur finalisasi dapat
dibangun dan diuji. Itu **tidak** berarti ia menetapkan kebijakan klinisnya — sesuai preseden
`IGD-DEC-046`, Product/Domain Owner tidak mengesahkan keputusan klinis.

Daftar lengkapnya ada pada bagian *Gerbang Go-Live*.

### 5. Dua jenis kepemilikan yang sengaja dipisah

| Peran | Siapa | Berwenang atas |
|---|---|---|
| Product/domain owner + implementation owner | **Muhammad Hamzah** (`HMD-DEC-006`) | Scope, bentuk blueprint, prioritas, penerimaan operasional |
| Registry owner | **Muhammad Hamzah** (`HMD-FACT-019`) | Penamaan dan kepemilikan modul pada registry |
| Pemegang akun tata kelola klinis | **Muhammad Hamzah**, sementara (`HMD-DEC-011`) | Memegang hak pengesahan **di dalam sistem** |
| Clinical governance authority | **Belum ditunjuk** | **Menetapkan kebijakan klinisnya** |

Baris ketiga dan keempat sengaja tidak digabung, dan itu bukan formalitas. Memegang hak akses
tidak sama dengan berwenang menetapkan kebijakan.

Contoh bedanya: ketika auditor bertanya *"atas dasar apa item Pra-HD ini boleh dilewati?"*,
jawaban yang berdiri adalah *"kebijakan badan klinis tanggal sekian, dijalankan pemegang akun
X"*. Jawaban *"pemegang akun yang memutuskan sendiri"* tidak berdiri — dan preseden
`IGD-DEC-046` menyatakannya tegas: Product/Domain Owner tidak mengesahkan keputusan klinis.

Karena itu selama badan klinis belum ditunjuk, ketiga keputusan klinis Hemodialisa berjalan
memakai aturan bawaan `HMD-ASM-001` sampai `HMD-ASM-003`, bukan atas nama siapa pun.

Pola yang sudah dipakai modul lain di repository ini: blueprint Radiologi mencatat
`Clinical governance owner` sebagai **Komite Medis sebagai badan**, lalu menunjuk satu orang
sebagai pemegang akun yang menjalankannya. Pola yang sama disarankan di sini.

### 6. Wawancara Phase 2 dan Phase 3 sengaja dipisah, dan itu menanggung risiko

PRD Phase 3 bagian 22 memuat *Master One-Time Grill Contract* yang meminta ketiga PRD
di-*grill* bersamaan satu kali. Pemilik memilih jalan lain: Phase 1 lebih dulu, Phase 2 dan
Phase 3 di-*grill* terpisah nanti (`HMD-DEC-002`).

Pilihan itu sah dan dihormati. Konsekuensinya dicatat apa adanya di sini supaya tidak hilang:

| Risiko | Penjelasan |
|---|---|
| Kontrak PRD sendiri tidak diikuti | Phase 2 bagian "Status" menulis "Tidak perlu Grill ulang setelah Phase 1 selesai". Dengan keputusan ini, Phase 2 dan Phase 3 **akan** perlu pass `grill-me` sendiri |
| Entity Phase 1 berpotensi perlu diubah | Tiga tabel Phase 1 akan ditempeli Phase 2/3. Bila keputusan Phase 2/3 turun setelah tabelnya beku, perubahannya menjadi migration tambahan, bukan desain awal. Lihat *Titik Sentuh Maju* di bawah |

Ini **bukan** bantahan atas keputusan pemilik. Ini catatan agar konsekuensinya terlihat saat
Phase 2 dibuka nanti.

---

## Batas Scope

Modul: **Hemodialisa** (`hemodialisa`). Bentuk blueprint: **`SINGLE`**.

**Satu kalimat batas scope:** Hemodialisa mengelola program cuci darah seorang pasien dan
pelaksanaan setiap sesi cuci darahnya di unit HD — dari pasien dinyatakan layak, diberi resep
HD, dijadwalkan, dikerjakan, dipantau, sampai catatan sesinya dikunci dan tindakannya
diteruskan ke jalur penagihan yang sudah ada.

### Di dalam scope (MUST) — 25 Feature Phase 1

| Kode | Kemampuan | Disposisi menurut PRD |
|---|---|---|
| `FEAT-001` | Verifikasi identitas pasien dan konteks kunjungan | Pakai ulang yang sudah ada |
| `FEAT-003` | Pendaftaran episode/program dialisis | Baru |
| `FEAT-004` | Penilaian kelayakan klinis HD | Baru |
| `FEAT-005` | Status akses vaskular dan serologi | Baru + baca hasil Laboratorium |
| `FEAT-006` | Persetujuan tindakan (*consent*) | Pakai ulang `TrxPatientConsent` |
| `FEAT-007` | Resep/program HD (*prescription*) | Baru |
| `FEAT-008` | Revisi resep dan penyimpangan pelaksanaan (*deviasi*) | Baru |
| `FEAT-009` | Penjadwalan sesi | Baru |
| `FEAT-010` | Checklist Pra-HD | Baru |
| `FEAT-011` | Assessment Pra-HD | Perluasan + pakai ulang Clinical |
| `FEAT-012` | Verifikasi mesin, air, dan bahan | Baru |
| `FEAT-013` | Memulai sesi HD | Baru + pakai ulang Procedure |
| `FEAT-014` | Pemantauan berkala selama HD (*serial monitoring*) | Baru + pakai ulang VitalSign |
| `FEAT-015` | Obat dan antikoagulasi | Baru + serah terima ke Farmasi |
| `FEAT-016` | Komplikasi dan eskalasi | Baru |
| `FEAT-017` | Assessment Pasca-HD | Baru + pakai ulang VitalSign |
| `FEAT-018` | Disposisi dan tindak lanjut pasien | Baru |
| `FEAT-019` | Finalisasi sesi | Perluasan Rekam Medis |
| `FEAT-026` | Isolasi Hepatitis B | Baru |
| `FEAT-027` | Status laik mesin HD | Baru |
| `FEAT-028` | Status kesiapan pengolahan air (*water treatment*) | Baru |
| `FEAT-029` | Kesiapan obat dan BMHP | Baru + baca Farmasi |
| `FEAT-030` | Kapasitas dan kompetensi staf | Perluasan + pakai ulang HR |
| `FEAT-033` | Serah terima ke Billing | Pakai ulang Procedure → Billing |
| `FEAT-036` | Audit dan koreksi rekam medis | Perluasan + pakai ulang Rekam Medis |

### Kemampuan tambahan di luar daftar Feature analisis asal

Kemampuan berikut **tidak ada** pada 36 Feature dokumen analisis asal maupun pada ketiga PRD.
Ia lahir dari conflict yang ditemukan pada sesi ini dan ditambahkan atas keputusan pemilik
modul. Kodenya sengaja dibuat berbeda agar terlihat jelas bahwa ia tidak punya Feature asal —
tanpa itu, ia akan lolos dari seluruh berkas *traceability* nanti.

| Kode | Kemampuan | Disposisi | Asal |
|---|---|---|---|
| `HMD-CAP-001` | Permintaan HD masuk dari modul lain, mengikuti bentuk `LabOrder`/`RadOrder` | Baru | `HMD-DEC-008`, menutup `HMD-CONF-001` |

**Total kemampuan Phase 1: 25 Feature + 1 kemampuan tambahan = 26.**

### Di luar scope — ditunda ke blueprint/pass berikutnya

Sebelas Feature berikut **tidak** dibahas pada blueprint ini dan akan menempuh pass
`grill-me` sendiri ketika fasenya dibuka.

| Kode | Kemampuan | Fase asal |
|---|---|---|
| `FEAT-020` | Perhitungan dan tren adekuasi HD | Phase 2 |
| `FEAT-021` | Rencana dan hasil pemeriksaan berkala | Phase 2 |
| `FEAT-022` | Tren berat kering dan status cairan | Phase 2 |
| `FEAT-023` | Surveilans akses vaskular | Phase 2 |
| `FEAT-024` | Komplikasi akses dan rujukan | Phase 2 |
| `FEAT-025` | Pemantauan serologi dan vaksinasi Hepatitis B | Phase 2 |
| `FEAT-031` | Dataset registrasi dan laporan tahunan dialisis | Phase 2 |
| `FEAT-032` | Integrasi Uronefrologi SATUSEHAT | Phase 2 |
| `FEAT-034` | Serah terima insiden keselamatan pasien | Phase 2 |
| `FEAT-002` | Verifikasi administrasi JKN dan rujukan | Phase 3 |
| `FEAT-035` | Indikator mutu dan audit layanan HD | Phase 3 |

### Di luar scope — milik modul lain

| Kemampuan | Modul pemilik | Titik sentuh Hemodialisa |
|---|---|---|
| Identitas dan data induk pasien | Patient Management | Membaca `MstPatient` |
| Kunjungan, penjamin, rujukan | Registration Management | Membaca `RegPatientEncounter` |
| Siklus rawat inap | Inpatient Management | Menyimpan rujukan opsional ke `InpEpisode` |
| Hasil pemeriksaan laboratorium | Laboratorium | Merujuk hasil, tidak pernah menyalin |
| Stok, batch, dan penyaluran obat | Farmasi | Meneruskan fakta pemberian ke `PhmDrugUsage` |
| Harga, diskon, invoice, pembayaran, klaim | Billing dan Kasir | Menyerahkan tindakan selesai lewat `TrxPatientProcedure` |
| Integritas dan koreksi dokumen rekam medis | Rekam Medis | Mendaftarkan dokumen sesi final, koreksi lewat *addendum* |
| Data kompetensi, STR, SIP, sertifikat petugas | Human Resource / Workforce | Merujuk profil, tidak menyalin |
| Perawatan teknis dan pengadaan mesin | Belum ada pemilik tercatat | Hanya menyimpan status laik pakai |

### Di luar scope — ditunda selamanya pada modul ini

CAPD/dialisis peritoneal, transplantasi ginjal, operasi pembuatan akses vaskular, integrasi
otomatis perangkat mesin HD, mesin alarm klinis otomatis, rekomendasi klinis AI, pemakaian
ulang dialyzer (*dialyzer reuse*), grouping INA-CBG, adjudikasi klaim, dan portal pasien.

### Di luar scope — untuk modul lain, ditemukan selama sesi ini

| Temuan | Catatan |
|---|---|
| Titik masuk permintaan HD dari layar dokter rawat inap | `HMD-CONF-001` — masih dalam scope Phase 1, lihat `HMD-OQ-006` |
| Infrastruktur integrasi keluar (*outbox*) berstatus milik Rawat Inap, bukan milik platform | `HMD-CONF-002` — **di luar scope sesi ini**, hanya menyangkut `FEAT-032` Phase 2 |

---

## Fakta Terverifikasi dari Source

Seluruh baris berikut diverifikasi pada backend `69b256ca` dan frontend `8143874d8`.

### Berlaku untuk Phase 1 — scope sesi ini

| ID | Fakta | Bukti |
|---|---|---|
| `HMD-FACT-001` | Registry kepemilikan modul **belum** memuat Hemodialisa. Pencarian kata `hemodial` dan prefix `Hmd` tidak menghasilkan satu baris pun | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — 0 kecocokan |
| `HMD-FACT-002` | Prefix `Hmd` **belum dipakai** modul mana pun. Daftar kepanjangan prefix memuat 23 prefix dan `Hmd` tidak termasuk | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:36-58` |
| `HMD-FACT-003` | `ServiceUnitType` berisi nilai 0–9 lalu langsung `Other = 99`. Nilai `10` **kosong**, sehingga usulan `Hemodialysis = 10` tidak menabrak nilai lain | `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs:3-16` |
| `HMD-FACT-004` | `ClinicalDocumentKind` berhenti di `Consent = 13`. Nilai `14` **kosong**, sehingga usulan `HemodialysisSession = 14` tidak menabrak nilai lain | `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs:11-26` |
| `HMD-FACT-005` | `TrxPatientProcedure.DoctorId` bertipe `Guid` **wajib diisi** (`[Required]`) | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:29-30` |
| `HMD-FACT-006` | `TrxPatientProcedure` **juga** memiliki `InstructingDoctorId` bertipe `Guid?` (boleh kosong) beserta status verifikasi instruksi dokter | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:246-252` |
| `HMD-FACT-007` | Tidak ada satu pun berkas `.cs` backend yang menyebut hemodialisa/dialysis. Klaim bahwa bounded context ini `MISSING / NEW` **masih benar** | Pencarian `hemodial\|dialisa\|dialysis` pada `**/*.cs` — 0 berkas |
| `HMD-FACT-008` | Frontend **sudah** memesan kunci menu bersarang bernama `menuHemodialisa`, tetapi kunci itu hanya terdaftar sebagai nama; belum ada butir menu, route, atau layar apa pun di baliknya | `QuilvianSystemFrontendDev/src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:9` |
| `HMD-FACT-009` | Layar *physician workspace* Rawat Inap sudah menampilkan kartu layanan penunjang **Hemodialisa** berlabel "Integrasi belum tersedia", dengan keterangan "Pelayanan hemodialisis rutin dan cito rawat inap, pemantauan adekuasi dialisis, dan akses vaskular" | `.../inpatient-management/inpatient-supporting-service-constants.jsx:12-18, 70-79` |
| `HMD-FACT-010` | Prosedur pendaftaran modul/prefix baru terdiri dari enam langkah; langkah ke-5 berbunyi "Minta persetujuan pemilik modul, lalu tambahkan barisnya ke tabel dan catat di *Catatan perubahan lifecycle*", dan langkah ke-6 "Baru buat model pertama" | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:63-75` |
| `HMD-FACT-011` | Persetujuan registry **hanya** memberi wewenang penamaan dan kepemilikan; ia tidak memberi wewenang implementasi, migration, pekerjaan database, maupun deployment | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:5` |
| `HMD-FACT-012` | Registry di repository backend pernah **tertinggal** dari registry canonical pada suite Skill, dan selisih itu memblokir entity baru pada Accounting (`ACC-DEP-007`) dan Radiologi (`RAD-REQ-001`). Selisih dua salinan registry tercatat masih terbuka | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:113-114` |
| `HMD-FACT-019` | Registry menamai **Muhammad Hamzah sebagai pemilik registry**, pada catatan perubahan lifecycle Radiologi 2026-09-10. Branch kerja backend saat ini adalah `MHamzah` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:114`; `git branch --show-current` |
| `HMD-FACT-020` | Ada preseden bahwa **pemilik modul menambahkan sendiri baris registry-nya** sesuai langkah 5: Gizi/`Gz` oleh Ikbal Yuliyanto (2026-09-08) dan Accounting/`Acc` oleh Rizki (2026-09-08) | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:111, 113` |
| `HMD-FACT-021` | Pemeriksa QBE menerima sebuah baris registry hanya bila empat syarat terpenuhi sekaligus: kolom `Module/pemilik` cocok dengan path folder source dan tidak ambigu; `Category` diawali `BUSINESS DOMAIN`; seluruh token `Lifecycle` termasuk `active` atau `legacy`; nama entity diawali prefix lalu huruf kapital. Nilai `PLANNED` **ditolak** dengan alasan `non-active Lifecycle` | `tooling/qbe/Invoke-QbeConformanceCheck.ps1:405-424` |
| `HMD-FACT-022` | Prefix `Hmd` kosong di **kedua** salinan registry, dan kedua salinan itu saat ini **identik** pada tabel kepemilikan — tidak ada selisih yang harus dibereskan lebih dulu | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dan `<suite-skill>/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, dibandingkan pada 18 September 2026 |
| `HMD-FACT-023` | `routeKey: "hemodialysis"` pada konstanta layanan penunjang Rawat Inap **tidak pernah dipakai untuk navigasi** di mana pun. Ia hanya didefinisikan, tidak dikonsumsi | Pencarian `routeKey` pada `src/` — hanya muncul di berkas konstanta itu sendiri dan di modul HR yang tidak berkaitan |
| `HMD-FACT-024` | Panel layanan penunjang yang belum terintegrasi memang **sengaja** dibuat kosong. Komentar berkasnya menuliskan syarat kritis `AC-4`: "Nol permintaan jaringan ke modul terkait. Nol formulir input, data tiruan, atau aksi penyimpanan palsu", mengikuti `RWI-DEC-108` dan `FR-DOK-111` | `.../supporting-service/supporting-unavailable-panel.jsx:18-26` |
| `HMD-FACT-027` | `PatientProcedureStatus` hanya memuat lima nilai — `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled`. **Tidak ada status "dikerjakan sebagian"**, sehingga sesi yang dihentikan harus dipetakan ke salah satu nilai yang ada | `Areas/HealthServices/ClinicalManagement/Enums/PatientProcedureStatus.cs:3-10@69b256ca` |
| `HMD-FACT-028` | Fakta tagihan yang dikirim ke Billing membawa `Quantity`, `Unit`, dan `TariffSnapshot` berisi `unitPrice`, `totalPrice`, `isFreeOfCharge`, dan `isBillable`. Penagihan sebagian secara teknis mungkin | `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs:1517-1545@69b256ca` |
| `HMD-FACT-029` | Pembatalan tagihan normal hanya diizinkan dari status sumber `CONFIRMED` atau `ACCEPTED`. **Begitu mencapai `COMPLETED` atau `PERFORMED`, jalur batal normal tertutup** | `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs:23-26@69b256ca` |
| `HMD-FACT-030` | Sumber tagihan `ADHOC_CATALOG` sudah ada dan memang dirancang untuk entri kasir dari katalog tarif, dengan harga ditetapkan server — terpisah dari sumber klinis | `BillingChargeSourceAdapter.cs:47-52@69b256ca`; `BillingInvoiceService.cs:1121@69b256ca` |
| `HMD-FACT-026` | `TrxPatientProcedure` menyediakan alur lengkap "perawat mengerjakan atas instruksi dokter": `InstructingDoctorId`, `InstructionVerificationStatus` (`NotRequired`/`Pending`/`Verified`), `InstructionVerifiedAt`, `InstructionVerifiedByUserId`, dan `OrderedByUserId`. `ConsultationId` juga sudah dilonggarkan menjadi boleh kosong sejak R7 khusus untuk "pesanan rawat inap perawat atas instruksi dokter" | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:20-24, 240-256` |
| `HMD-FACT-025` | `LabOrder` dan `RadOrder` memakai **bentuk yang sama persis** untuk menerima permintaan dari rawat inap: `EncounterId` wajib, `InpEpisodeId` boleh kosong, `OrderStatus` bermula di `Requested`, dan `StatusBeforeHold` untuk pola penahanan | `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs:21, 41, 57, 63`; `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs:34, 47, 53, 59` |

### Diverifikasi sesi ini, dipakai nanti pada Phase 2 dan Phase 3

Fakta berikut sudah terlanjur diverifikasi dan disimpan supaya pass berikutnya tidak
mengulang pekerjaan yang sama. Tidak satu pun memengaruhi scope Phase 1.

| ID | Fakta | Bukti |
|---|---|---|
| `HMD-FACT-013` | `RegPatientEncounterGuarantor` **sudah ada**. Klaim Phase 3 bahwa konteks penjamin dimiliki Registration terbukti benar | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs` |
| `HMD-FACT-014` | `PatientEncounterGuarantorType` **sudah** memuat `BPJS = 4` | `Areas/HealthServices/RegistrationManagement/Enums/PatientEncounterGuarantorType.cs:3-11` |
| `HMD-FACT-015` | **Tidak ada** implementasi SATUSEHAT maupun FHIR di backend. Satu-satunya berkas yang menyebutnya adalah seeder ICD-10 | Pencarian `satusehat\|fhir` pada `**/*.cs` |
| `HMD-FACT-016` | **Tidak ada** implementasi VClaim maupun penerbitan SEP di backend | Pencarian `vclaim` pada `**/*.cs` |
| `HMD-FACT-017` | **Tidak ada** infrastruktur indikator mutu/KPI apa pun di backend | Pencarian `QualityIndicator\|KpiDefinition` pada `**/*.cs` — 0 berkas |
| `HMD-FACT-018` | Pola *Transactional Outbox* sudah ada dan berjalan, lengkap dengan `IdempotencyKey`, `SourceDomain`, `EventType`, `PayloadJson`, `RetryCount`, dan worker latar belakang — tetapi **dimiliki modul Rawat Inap** (`InpIntegrationOutbox`, tabel `InpIntegrationOutboxes`), bukan kemampuan platform bersama | `Areas/HealthServices/InPatientManagement/Models/InpIntegrationOutbox.cs:7-50`; `Workers/InpatientIntegrationOutboxWorker.cs`; migration `20260917000000_AddInpatientBillingIntegrationOutbox` |

---

## Conflict yang Ditemukan Sesi Ini

### `HMD-CONF-001` — Titik masuk permintaan HD dari Rawat Inap tidak punya pemilik

**Status scope:** di dalam Phase 1.

**Bukti yang bertentangan:**

| Sumber | Yang dinyatakan |
|---|---|
| Frontend `inpatient-supporting-service-constants.jsx:70-79` (`HMD-FACT-009`) | Dokter rawat inap akan punya kartu "Hemodialisa" pada layar layanan penunjang, untuk "hemodialisis rutin dan cito rawat inap". Statusnya "Integrasi belum tersedia" — artinya layar itu **menunggu backend Hemodialisa** |
| PRD Phase 1 bagian 9, langkah 1 | Alur dimulai dari "Pasien datang / permintaan HD diterima" |
| PRD Phase 1 bagian 12.3 | Daftar 15 entity baru **tidak memuat satu pun** entity permintaan/order HD |
| PRD Phase 1 bagian 13 | Tidak ada satu pun endpoint yang menerima permintaan HD dari modul lain |

**Mengapa ini material:** PRD mengunci alur yang dimulai dari "permintaan HD diterima", tetapi
tidak pernah menetapkan **siapa yang membuat permintaan itu dan di mana ia disimpan**. Pada saat
yang sama, layar Rawat Inap yang sudah jadi menjanjikan tombol yang akan memanggil Hemodialisa.

Contoh nyata akibatnya: seorang dokter di bangsal menemukan pasiennya perlu HD cito malam ini.
Ia membuka layar layanan penunjang, menekan kartu Hemodialisa — dan tidak terjadi apa-apa,
karena tidak ada satu pun endpoint Hemodialisa yang menerima permintaan. Yang tersisa hanyalah
menelepon unit HD, lalu koordinator mengetik jadwal manual. Permintaan itu tidak pernah menjadi
data, sehingga tidak bisa diaudit.

**Status:** **TERTUTUP** oleh `HMD-DEC-008`, 18 September 2026. Hemodialisa mendapat entity
permintaan masuk sendiri (`HMD-CAP-001`), mengikuti bentuk `LabOrder` dan `RadOrder`
(`HMD-FACT-025`).

### `HMD-CONF-002` — Infrastruktur integrasi keluar belum berstatus milik platform

**Status scope:** **di luar Phase 1.** Hanya menyangkut `FEAT-032` SATUSEHAT pada Phase 2.
Dicatat di sini supaya tidak hilang, dan dibawa ke pass `grill-me` Phase 2.

**Ringkas:** PRD Phase 2 menulis "Tidak otomatis dibuat `HmdSatusehatTransaction` bila platform
sudah mempunyai canonical integration transaction". Jawaban dari source adalah **setengah**:
polanya sudah ada dan terbukti jalan, tetapi pemiliknya modul Rawat Inap (`HMD-FACT-018`).
Syarat itu tidak terpenuhi dan juga tidak gagal, sehingga kalimat PRD tidak bisa dieksekusi apa
adanya.

---

## Titik Sentuh Maju ke Phase 2 dan Phase 3

Karena Phase 2 dan Phase 3 sengaja dipisah (`HMD-DEC-002`), tiga tabel Phase 1 berikut sudah
diketahui akan ditempeli kemampuan fase berikutnya. Daftar ini **bukan** keputusan desain dan
**bukan** perluasan scope. Ia catatan agar desain Phase 1 tidak menutup pintunya tanpa sadar.

| Tabel Phase 1 | Akan ditempeli | Dari |
|---|---|---|
| `HmdEpisode` | Evaluasi adekuasi, rencana pemeriksaan berkala, tinjauan berat kering, tinjauan pengendalian infeksi, dataset pelaporan | `FEAT-020`, `021`, `022`, `025`, `031` |
| `HmdVascularAccess` | Riwayat surveilans akses dan tindak lanjut rujukan | `FEAT-023`, `024` |
| `HmdSessionComplication` | Serah terima insiden keselamatan pasien | `FEAT-034` |

Yang perlu dipastikan saat desain Phase 1: ketiga tabel itu punya kunci utama yang stabil dan
dapat dirujuk tabel lain, serta tidak dirancang sebagai tabel yang isinya ditimpa. Tidak ada
kolom Phase 2/3 yang dibuat sekarang.

---

## Decision Log

Keputusan bisnis dan struktur sudah disetujui pemilik modul. Keputusan tata kelola klinis
belum, dan tidak boleh disetujui oleh pemilik modul.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HMD-DEC-006` | Decision | Pemilik proses bisnis sekaligus pemilik implementasi modul Hemodialisa adalah **Muhammad Hamzah**. Pemilik tata kelola klinis menyusul sebagai badan klinis, belum ditetapkan | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | Jawaban pemilik pada sesi ini; `HMD-FACT-019` |
| `HMD-DEC-001` | Decision | Scope blueprint ini adalah **Phase 1 saja**, 25 Feature, sesuai daftar **Di dalam scope (MUST)** | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | Jawaban pemilik; PRD Phase 1 bagian 7.1 |
| `HMD-DEC-002` | Decision | Phase 2 dan Phase 3 **tidak** di-*grill* pada sesi ini; keduanya menempuh pass `grill-me` sendiri ketika fasenya dibuka | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | Jawaban pemilik. Menyimpang dari *Master One-Time Grill Contract* PRD Phase 3 bagian 22; konsekuensinya dicatat pada Peringatan Prasyarat butir 6 |
| `HMD-DEC-003` | Decision | Kemampuan pada daftar **Di luar scope** tidak dikerjakan pada blueprint ini | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | PRD Phase 1 bagian 5.3 dan 8 |
| `HMD-DEC-004` | Decision | `blueprint_shape` = **`SINGLE`**. Satu himpunan 14 berkas di `docs/module-blueprints/hemodialisa/`, tanpa sub-modul dan tanpa `02-module-map.md` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | Jawaban pemilik |
| `HMD-DEC-005` | Fact | `ServiceUnitType.Hemodialysis = 10` dan `ClinicalDocumentKind.HemodialysisSession = 14` tidak menabrak nilai existing | — | — | — | `HMD-FACT-003`, `HMD-FACT-004` |
| `HMD-DEC-013` | Decision | **Kewenangan klinis petugas ditampilkan dan ditandai sekarang, ditegakkan kemudian.** Penugasan petugas ke sesi menyimpan status verifikasi kewenangan dengan tiga nilai: `Terverifikasi`, `Tidak berwenang`, dan `Belum dapat diverifikasi`. Selama `HMD-DEP-002` belum tersedia, nilainya `Belum dapat diverifikasi` — **tidak boleh** diisi `Terverifikasi`. Penegakan dinyalakan sebagai pengaturan begitu Human Resource membuka pembacaan kewenangan, tanpa perubahan tabel. Menutup `DEC-HMD-001` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `HMD-RCG-001` `DEC-HMD-001`; `HMD-DEP-002`; capability map *Temuan Kritis 3* |
| `HMD-DEC-012` | Decision | **Sesi HD yang dihentikan di tengah jalan (`Stopped`) tidak menerbitkan tagihan otomatis.** Tindakan tetap tercatat lengkap secara klinis dengan `IsBillable = false` beserta alasan penghentiannya. Bila rumah sakit berhak menagih bahan yang terpakai, kasir menambahkannya lewat jalur `ADHOC_CATALOG` yang sudah ada — harga ditetapkan server dari katalog tarif, terlacak terpisah dari tagihan klinis. Sesi yang dibatalkan sebelum dimulai tidak pernah menagih. Menutup `DEC-HMD-002` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `HMD-FACT-027` sampai `HMD-FACT-030`; `HMD-RCG-001` `DEC-HMD-002` |
| `HMD-DEC-010` | Decision | Kewenangan tata kelola klinis Hemodialisa **tetap `OPEN`** dan menjadi **syarat go-live, bukan syarat desain**. Keputusan klinis tidak disahkan oleh Product/Domain Owner; selama belum ditunjuk, keputusan klinis memakai regulasi yang berlaku sebagai acuan dan aturan bawaan paling aman (*fail-closed*) yang dicatat sebagai `HMD-ASM-001` sampai `HMD-ASM-003`. Mengikuti preseden `IGD-DEC-046` | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `igd/00-interview-decisions.md` `IGD-DEC-046` |
| `HMD-DEC-011` | Decision | **Pemegang akun tata kelola klinis Hemodialisa sementara dipegang Muhammad Hamzah** (*acting*). Ia memegang hak akses pengesahan di dalam sistem agar alur finalisasi dapat dibangun dan diuji. Kewenangan **menetapkan kebijakan klinis** tetap pada badan klinis rumah sakit dan belum ditunjuk; ratifikasi menjadi syarat go-live. Mengikuti bentuk `RAD-DEC-017`, dengan badan penetapnya masih kosong | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `radiologi/00-interview-decisions.md` `RAD-DEC-016`, `RAD-DEC-017` |
| `HMD-DEC-009` | Decision | Pemetaan dokter pada `TrxPatientProcedure`: **`DoctorId` = dokter penanggung jawab sesi HD**, **`InstructingDoctorId` = dokter pembuat `HmdPrescription`**. Alur `InstructionVerificationStatus` yang sudah ada dipakai apa adanya. Konsekuensi yang ikut disetujui: setiap sesi **wajib** punya dokter penanggung jawab sebelum boleh dimulai | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `HMD-FACT-005`, `HMD-FACT-006`, `HMD-FACT-026` |
| `HMD-DEC-008` | Decision | Hemodialisa **memiliki entity permintaan masuk sendiri** (`HMD-CAP-001`), mengikuti bentuk `LabOrder`/`RadOrder`: `EncounterId` wajib, `InpEpisodeId` boleh kosong, status bermula di `Requested`, dan pola penahanan `StatusBeforeHold`. Nama entity bersifat usulan sampai desain dijalankan | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `HMD-CONF-001`; `HMD-FACT-009`, `HMD-FACT-023`, `HMD-FACT-024`, `HMD-FACT-025` |
| `HMD-DEC-007` | Decision | Pendaftaran registry **disetujui** dengan baris `\| HealthServices \| HemodialysisManagement / Hemodialysis \| BUSINESS DOMAIN / MODULE \| Hmd \| ACTIVE \|`. Persetujuan ini hanya mencakup **penamaan dan kepemilikan**; ia bukan izin implementasi, migration, eksekusi database, maupun deployment | Muhammad Hamzah | `approved` | Muhammad Hamzah, 18 September 2026 | `HMD-FACT-002`, `HMD-FACT-010`, `HMD-FACT-011`, `HMD-FACT-019` sampai `HMD-FACT-022` |

### Catatan atas `HMD-DEC-004`

Hasil uji lima syarat pemecahan pada revision 2 menunjukkan tiga rumpun yang masing-masing
memenuhi kelima syarat, sehingga saran agent saat itu adalah `COMPOSITE`. Pemilik memilih
`SINGLE`, dan pilihan itu diikuti tanpa ditawar sesuai `rules/rule-output/bentuk-blueprint.md`
bagian 4.

Perlu dicatat bahwa dengan `HMD-DEC-001` yang mempersempit scope menjadi Phase 1 saja, dasar
saran `COMPOSITE` itu **sebagian besar gugur dengan sendirinya**: dua dari tiga rumpun
(Perawatan Longitudinal serta Pelaporan dan Mutu) seluruhnya berada di luar scope sekarang.
Yang tersisa hanya rumpun Pelayanan Inti, dan satu rumpun tidak bisa dipecah. Jadi `SINGLE`
konsisten dengan scope yang dipilih, bukan bertentangan dengannya.

Bila Phase 2 dibuka kelak dan digabungkan ke blueprint yang sama, bentuknya boleh dinaikkan
ke `COMPOSITE` lewat revisi material sesuai `bentuk-blueprint.md` bagian 6.

### Keputusan yang diwarisi dari PRD masukan

Ketujuh belas keputusan `HD-DEC-001` sampai `HD-DEC-017` pada PRD Phase 1 Appendix D
diperlakukan sebagai **usulan yang perlu ditegaskan**, bukan keputusan yang sudah mengikat,
karena PRD-nya berstatus `DRAFT` dan pemiliknya belum tercatat. Penegasannya dilakukan sekali
saat pemilik ditetapkan, tidak diulang satu per satu.

---

## Aturan Bawaan Sementara — Fail-Closed

Ketiga baris berikut **bukan keputusan klinis yang sudah sah**. Ia aturan bawaan agar sistem
tetap aman selama kebijakan klinisnya belum ada, sesuai `HMD-DEC-010`. Jenisnya `Assumption`,
statusnya `draft`, dan tidak seorang pun mengesahkannya.

Prinsipnya satu: **bila ragu, tutup.** Sistem lebih baik menolak sesuatu yang sebenarnya boleh,
daripada mengizinkan sesuatu yang sebenarnya dilarang.

| ID | Menggantikan sementara | Aturan bawaan | Alasan |
|---|---|---|---|
| `HMD-ASM-001` | `HMD-OQ-003` — batas *override* Pra-HD | Mekanisme *override* **dibangun lengkap** (alasan wajib, pencatat, waktu, jejak audit), tetapi **daftar item yang boleh di-*override* dikosongkan**. Selama kosong, kekurangan Pra-HD apa pun membuat sesi tidak dapat dinyatakan `Ready` | Membangun mekanismenya sekarang berarti badan klinis nanti cukup mengisi daftarnya, bukan menunggu pengembangan ulang. Daftar kosong adalah keadaan paling aman |
| `HMD-ASM-002` | `HMD-OQ-004` — kewenangan finalisasi sesi | Dua langkah: **perawat menyelesaikan dokumentasi**, lalu **dokter penanggung jawab sesi mengesahkan** dan mengunci. Ini pilihan paling ketat dari tiga opsi PRD | Sejalan dengan `HMD-DEC-009` yang sudah mewajibkan setiap sesi punya dokter penanggung jawab, dan dengan Permenkes 24/2022 tentang Rekam Medis. Bila badan klinis kelak memilih yang lebih longgar, pelonggaran lebih mudah daripada pengetatan |
| `HMD-ASM-003` | `HMD-OQ-007` — menolak/menahan permintaan HD | **Menahan** (`OnHold`) boleh koordinator unit — alasannya operasional, misalnya mesin penuh. **Menolak** (`Rejected`) hanya dokter — alasannya klinis, misalnya HD tidak diindikasikan. Alasan wajib diisi pada keduanya | Memisahkan alasan operasional dari alasan klinis. Koordinator tidak pernah memutuskan indikasi medis |

**Contoh nyata `HMD-ASM-001`:** perawat membuka Pra-HD, seluruh item hijau kecuali hasil
serologi yang belum ditinjau. Dokter menekan "Nyatakan Ready" — ditolak, karena daftar item
yang boleh dilewati masih kosong. Yang bisa dilakukan hanyalah menahan sesi atau melengkapi
itemnya. Setelah badan klinis memutuskan bahwa "tinjauan serologi" boleh dilewati dengan alasan
tertulis, item itu masuk daftar dan tombolnya bekerja — tanpa satu baris kode pun berubah.

---

## Gerbang Go-Live

Hemodialisa **tidak boleh dipakai melayani pasien sungguhan** sebelum seluruh baris berikut
tertutup. Daftar ini adalah satu-satunya tempat gerbang itu dicatat.

| ID | Yang harus terjadi | Siapa | Akibat bila dilewati |
|---|---|---|---|
| `HMD-GATE-001` | Badan klinis rumah sakit ditunjuk sebagai pemegang kewenangan tata kelola klinis Hemodialisa | Manajemen rumah sakit | Tidak ada pihak sah yang bertanggung jawab atas keputusan keselamatan pasien pada modul ini |
| `HMD-GATE-002` | Badan klinis mengesahkan daftar item Pra-HD yang boleh di-*override*, menggantikan `HMD-ASM-001` | Badan klinis | Unit HD terkunci pada aturan paling ketat, berpotensi menghambat tindakan yang sebenarnya sah |
| `HMD-GATE-003` | Badan klinis mengesahkan kewenangan finalisasi sesi, menggantikan `HMD-ASM-002` | Badan klinis | Pengesahan catatan klinis final berjalan atas dasar aturan bawaan, bukan kebijakan rumah sakit |
| `HMD-GATE-004` | Badan klinis mengesahkan kewenangan menolak permintaan HD, menggantikan `HMD-ASM-003` | Badan klinis | Penolakan indikasi medis berjalan tanpa dasar kebijakan tertulis |
| `HMD-GATE-005` | Pemegang akun tata kelola klinis dikukuhkan badan klinis, menggantikan penunjukan sementara `HMD-DEC-011` | Badan klinis | Hak pengesahan dipegang atas dasar penunjukan sendiri |
| `HMD-GATE-006` | Orang kedua ditunjuk bila aturan melarang penyusun sekaligus pengesah | Badan klinis | Jebakan `RAD-OPEN-012` pada Radiologi terulang: satu pengesah membuat alurnya macet |

Kelima gerbang pertama lahir dari `HMD-DEC-010`. `HMD-GATE-006` lahir dari pelajaran modul
Radiologi, bukan dari kebutuhan Hemodialisa sendiri — dan justru karena itu mudah terlewat.

---

## Open Questions dan Blocker

Empat pertanyaan tersisa: `HMD-OQ-000`, `HMD-OQ-003`, `HMD-OQ-004`, dan `HMD-OQ-007`.

Keempatnya keputusan tata kelola klinis. Setelah `HMD-DEC-010`, **tidak satu pun lagi memblokir
`DESIGN`** — keempatnya berpindah menjadi gerbang `GO-LIVE`, dan selama belum tertutup masing
masing berjalan memakai aturan bawaan pada `HMD-ASM-001` sampai `HMD-ASM-003`.

Artinya: `design-business-module` boleh dijalankan sekarang. Yang tidak boleh adalah melayani
pasien sungguhan.

| ID | Setara | Pertanyaan | Owner yang dibutuhkan | Memblokir |
|---|---|---|---|---|
| `HMD-OQ-000` | — | Siapa badan tata kelola klinis modul ini, dan siapa pemegang akun yang menjalankannya? **Sebagian tertutup** — pemilik bisnis dan implementasi sudah ditetapkan `HMD-DEC-006`; yang tersisa hanya pemilik klinis | Manajemen rumah sakit | `GO-LIVE` — `HMD-GATE-001`. Pemegang akun sementara sudah ditunjuk `HMD-DEC-011` |
| ~~`HMD-OQ-002`~~ | `GRILL-HD-001` | **TERTUTUP** oleh `HMD-DEC-007`, 18 September 2026 | — | — |
| `HMD-OQ-003` | `GRILL-HD-002` | Pada kekurangan klinis di Pra-HD, item apa yang sama sekali tidak boleh di-*override*, dan item mana yang boleh di-*override* dokter dengan alasan tercatat? | Badan klinis | `GO-LIVE` — `HMD-GATE-002`. Sementara berjalan dengan `HMD-ASM-001` |
| `HMD-OQ-004` | `GRILL-HD-003` | Siapa yang berwenang membuat sesi HD menjadi `Finalized`? | Badan klinis | `GO-LIVE` — `HMD-GATE-003`. Sementara berjalan dengan `HMD-ASM-002` |
| ~~`HMD-OQ-005`~~ | `GRILL-HD-004` | **TERTUTUP** oleh `HMD-DEC-009`, 18 September 2026 | — | — |
| ~~`HMD-OQ-006`~~ | — | **TERTUTUP** oleh `HMD-DEC-008`, 18 September 2026 | — | — |
| `HMD-OQ-007` | — | Siapa yang boleh **menolak** atau **menahan** permintaan HD yang masuk, dan apa alasan sah untuk menolaknya? Lahir dari `HMD-DEC-008`. **Menyempit setelah audit:** kosakata statusnya tidak perlu diputuskan lagi — `Rejected`, `OnHold`, dan `CancelRequested` sudah ada pada `RadOrderStatus`; tersisa siapa orangnya dan apa alasan sahnya | Badan klinis + koordinator unit | `GO-LIVE` — `HMD-GATE-004`. Sementara berjalan dengan `HMD-ASM-003` |

### Keputusan pemblokir dari gerbang kelengkapan requirement

Dua keputusan berikut lahir dari `HMD-RCG-001`, bukan dari wawancara ini. Keduanya
**memblokir desain** untuk slice-nya masing-masing — berbeda dari `HMD-OQ-000`, `003`, `004`,
dan `007` yang hanya memblokir go-live.

| ID | Pertanyaan | Owner yang dibutuhkan | Memblokir |
|---|---|---|---|
| ~~`DEC-HMD-001`~~ | **TERTUTUP** oleh `HMD-DEC-013`, 18 September 2026. Slice `S7` kembali `READY_FOR_DOMAIN_DESIGN` | — | — |
| ~~`DEC-HMD-002`~~ | **TERTUTUP** oleh `HMD-DEC-012`, 18 September 2026. Slice `S11` kembali `READY_FOR_DOMAIN_DESIGN` | — | — |

`DEC-HMD-002` punya preseden yang jelas: Radiologi menjawab pertanyaan setara lewat
`RJ-BIL-GATE-DEC-004`, yang menyatakan `Requested`, `Accepted`, dan `Scheduled` bukan pemicu
tagihan — hanya pemeriksaan yang benar-benar dikerjakan. Hemodialisa membutuhkan pernyataan
setara dan belum punya.

### Dependency lintas modul yang ditemukan audit

Keduanya **bukan** pertanyaan bisnis, melainkan koordinasi jadwal dengan modul lain. Tercatat
lengkap pada `01-existing-capability-map.md`.

| ID | Dependency | Pemilik | Dampak bila tidak tersedia |
|---|---|---|---|
| `HMD-DEP-001` | Pembacaan hasil laboratorium per pasien per jenis pemeriksaan | Laboratorium | `FEAT-005` dan `FEAT-026` tidak bisa menampilkan status serologi terakhir. Menyalin hasil Lab ke tabel Hemodialisa **dilarang** |
| `HMD-DEP-002` | Pembacaan kewenangan klinis yang masih berlaku untuk seorang petugas | Human Resource / Credentialing | **Tidak lagi memblokir desain** setelah `HMD-DEC-013`. Selama belum tersedia, status verifikasi bernilai `Belum dapat diverifikasi`; begitu tersedia, penegakan tinggal dinyalakan |

### Dibawa ke pass berikutnya, bukan sekarang

| ID lama | Setara | Pertanyaan | Dibuka saat |
|---|---|---|---|
| `HMD-OQ-P2-001` | `GRILL-HD-005` | Metode perhitungan adekuasi dan cara menentukan targetnya | `grill-me` Phase 2 |
| `HMD-OQ-P2-002` | `GRILL-HD-006` | Irama pemeriksaan berkala, surveilans akses, serologi, vaksinasi, dan pemicu tinjauan berat kering | `grill-me` Phase 2 |
| `HMD-OQ-P2-003` | — | Infrastruktur integrasi keluar untuk SATUSEHAT — lihat `HMD-CONF-002` | `grill-me` Phase 2 |
| `HMD-OQ-P3-001` | `GRILL-HD-007` | Batas integrasi JKN: hanya membaca Registration, atau menghubungi BPJS/VClaim langsung | `grill-me` Phase 3 |
| `HMD-OQ-P3-002` | `GRILL-HD-008` | KPI HD resmi, penyetujunya, dan target lokalnya | `grill-me` Phase 3 |

---

## Acceptance Criteria yang Sudah Dapat Diuji

Butir berikut sudah cukup tegas untuk diuji tanpa menunggu jawaban open question mana pun.
Seluruhnya berada di dalam scope Phase 1.

| ID | Kriteria | Cara menguji |
|---|---|---|
| `HMD-AC-001` | Satu pasien tidak boleh punya dua episode HD berstatus `Active` untuk program yang sama | Buat episode kedua saat yang pertama masih `Active`; harus ditolak |
| `HMD-AC-002` | Satu mesin tidak boleh dipakai dua sesi yang waktunya bertumpang tindih | Jadwalkan dua pasien pada mesin dan waktu yang sama; hanya satu berhasil |
| `HMD-AC-003` | Satu station tidak boleh dipakai dua sesi yang waktunya bertumpang tindih | Sama seperti `HMD-AC-002`, pada station |
| `HMD-AC-004` | Satu pasien tidak boleh terjadwal pada dua sesi yang waktunya bertumpang tindih | Jadwalkan pasien yang sama dua kali pada waktu bertumpang tindih |
| `HMD-AC-005` | Mesin `Blocked`, `Maintenance`, atau `NotEligible` tidak boleh dipakai menjadwalkan maupun memulai sesi | Ubah status mesin, lalu coba jadwalkan dan mulai |
| `HMD-AC-006` | Menekan tombol Mulai dua kali menghasilkan tepat satu sesi berjalan dan tepat satu `TrxPatientProcedure` | Kirim dua permintaan `start` dengan kunci idempotency yang sama |
| `HMD-AC-007` | Pencatatan pemantauan berkala tidak saling menimpa; seluruh riwayat tampil urut waktu | Simpan lima observasi berurutan, lalu baca kembali |
| `HMD-AC-008` | Sesi `Finalized` menolak perubahan langsung; koreksi hanya lewat *addendum* | Kirim permintaan ubah pada sesi final; harus ditolak |
| `HMD-AC-009` | Kegagalan serah terima Billing tidak mengubah sesi dari `Finalized` menjadi tidak final | Gagalkan Billing secara sengaja, lalu periksa status sesi |
| `HMD-AC-010` | Bila konteks pasien atau sesi gagal diverifikasi, tidak ada penulisan data klinis yang diterima | Putus verifikasi konteks, lalu coba menyimpan data klinis |
| `HMD-AC-011` | Waktu `StartedAt`, `CompletedAt`, dan `FinalizedAt` memakai waktu server, bukan waktu peramban | Kirim waktu palsu dari klien; nilai tersimpan tetap waktu server |
| `HMD-AC-012` | Hak akses ditegakkan di server, bukan sekadar tombol dimatikan di layar | Panggil endpoint langsung dengan pengguna tanpa hak; harus 403 |
| `HMD-AC-013` | Permintaan HD yang masuk wajib membawa konteks kunjungan; permintaan tanpa `EncounterId` ditolak | Kirim permintaan tanpa konteks kunjungan; harus ditolak dengan pesan yang jelas |
| `HMD-AC-014` | Permintaan HD yang masuk **tidak** langsung menjadi sesi terjadwal. Station, mesin, resep aktif, dan perawat tetap ditentukan unit HD | Buat permintaan dari layar dokter bangsal, lalu periksa belum ada sesi berstatus `Scheduled` yang terbentuk |
| `HMD-AC-015` | Permintaan HD dari pasien rawat inap menyimpan rujukan ke episode rawat inapnya; permintaan dari rawat jalan tidak mengharuskannya | Buat satu permintaan dari bangsal dan satu dari rawat jalan; periksa `InpEpisodeId` terisi pada yang pertama dan kosong pada yang kedua |
| `HMD-AC-016` | Sesi tanpa dokter penanggung jawab tidak dapat dimulai | Kosongkan dokter penanggung jawab pada jadwal, lalu tekan Mulai; harus ditolak dengan pesan yang jelas |
| `HMD-AC-017` | Tindakan yang diserahkan ke Billing mencatat dokter penanggung jawab sesi pada `DoctorId`, dan dokter pembuat resep pada `InstructingDoctorId` | Jalankan satu sesi dengan dua dokter berbeda pada kedua peran, lalu periksa kedua kolom terisi orang yang berbeda dan benar |
| `HMD-AC-018` | Mengganti dokter penanggung jawab sesi setelah sesi final tidak boleh mengubah tindakan yang sudah diserahkan ke Billing | Ubah penugasan setelah finalisasi; periksa `DoctorId` pada tindakan tidak ikut berubah |
| `HMD-AC-019` | Sesi yang dihentikan di tengah jalan **tidak** menerbitkan tagihan otomatis; tindakannya tercatat dengan penanda tidak dapat ditagih beserta alasan penghentiannya | Hentikan sesi setelah 40 menit, finalisasi, lalu periksa tidak ada baris tagihan terbit dan alasan tersimpan |
| `HMD-AC-020` | Sesi yang selesai normal menerbitkan tepat satu tagihan bersumber `Procedure` | Selesaikan satu sesi, lalu hitung baris tagihan yang terbit |
| `HMD-AC-021` | Sesi yang dibatalkan sebelum dimulai tidak pernah menerbitkan tagihan | Batalkan sesi berstatus `Scheduled`, lalu periksa tidak ada tagihan |
| `HMD-AC-022` | Penugasan petugas ke sesi menyimpan status verifikasi kewenangan, dan selama pembacaan dari Human Resource belum tersedia nilainya `Belum dapat diverifikasi` — **bukan** `Terverifikasi` | Tugaskan seorang perawat, lalu baca status verifikasi pada sesi |
| `HMD-AC-023` | Menyalakan penegakan kewenangan dilakukan lewat pengaturan, tanpa mengubah struktur tabel | Nyalakan pengaturan penegakan, lalu periksa tidak ada migration yang dibutuhkan |
| `HMD-AC-024` | Ketika penegakan menyala dan petugas terbaca tidak berwenang, penugasan ditolak dengan pesan yang jelas | Nyalakan penegakan, tugaskan petugas tanpa kewenangan dialisis |

---

## Yang Sengaja Tidak Ditanyakan

PRD Phase 1 bagian 20.4 memuat daftar hal yang sudah dijelaskan dan tidak boleh ditanyakan
ulang. Daftar itu dipatuhi.

Ringkasnya, sesi ini **tidak** menanyakan: pemakaian pasien dan kunjungan yang sudah ada, letak
menu Hemodialisa, jumlah submenu, route utama, kepemilikan hasil laboratorium dan stok farmasi,
pembuatan `SourceDomain` Billing baru, pembagian sesi menjadi Pra/Intra/Pasca, siklus hidup
sesi, cara koreksi setelah final, integrasi perangkat, KPI, SATUSEHAT, interval pemantauan
wajib, penjadwalan prioritas otomatis, dan ambang alarm klinis otomatis.

Satu pertanyaan tambahan dibuat di luar empat pertanyaan Phase 1 yang diizinkan PRD, yaitu
`HMD-OQ-006`, karena memenuhi syarat pengecualian PRD: *conflict* baru yang material ditemukan
pada source (`HMD-CONF-001`, bukti `HMD-FACT-009`).

---

## Tindakan Lanjutan yang Belum Dikerjakan

`HMD-DEC-007` adalah **persetujuan**, bukan pelaksanaan. Baris registry-nya belum benar-benar
ditambahkan ke berkas mana pun. Sesi `grill-me` berjalan dalam `MODULE BLUEPRINT MODE` yang
hanya boleh menulis ke `docs/module-blueprints/**`, sehingga penyuntingan registry memerlukan
wewenang tulis terpisah.

| No | Tindakan | Berkas | Status |
|---:|---|---|---|
| 1 | Tambahkan baris `\| HealthServices \| HemodialysisManagement / Hemodialysis \| BUSINESS DOMAIN / MODULE \| Hmd \| ACTIVE \|` ke tabel kepemilikan | `NewQuilvianSystemBackend/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Belum** |
| 2 | Tambahkan `Hmd` = *Hemodialysis* ke tabel *Kepanjangan prefix* | berkas yang sama | **Belum** |
| 3 | Catat alasannya di *Catatan perubahan lifecycle*, menyebut `HMD-BP-001` dan `HMD-DEC-007` | berkas yang sama | **Belum** |
| 4 | Terapkan tindakan 1–3 yang identik pada salinan canonical suite skill | `<suite-skill>/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **Belum** |
| 5 | Commit dan push kedua repository | — | **Belum** — dijalankan pemilik |

Tindakan 4 tidak boleh dilewati. Kedua salinan sekarang identik (`HMD-FACT-022`); mengisi satu
saja melahirkan selisih baru — persis keadaan yang membuat entity Accounting (`ACC-DEP-007`) dan
Radiologi (`RAD-REQ-001`) tetap terblokir walaupun persetujuannya sudah ada.

---

## Dampak Desain yang Sudah Diketahui

Dua keputusan sesi ini mengubah bentuk Phase 1 dibanding PRD masukan. Keduanya wajib terbawa
saat `design-business-module` dijalankan.

| # | Perubahan terhadap PRD | Asal |
|---:|---|---|
| 1 | Ada entity permintaan HD masuk yang tidak ada di daftar 15 entity PRD, beserta endpoint penerimanya dan siklus status permintaannya | `HMD-DEC-008`, `HMD-CAP-001` |
| 2 | Jadwal sesi bertambah satu field wajib: **dokter penanggung jawab sesi**. PRD bagian FR-HD-008 sekarang hanya mewajibkan perawat. Tombol Mulai bertambah satu validasi | `HMD-DEC-009` |
| 3 | Sesi `Stopped` menerbitkan tindakan bertanda **tidak dapat ditagih** beserta alasan, bukan menghilangkan tindakannya. PRD hanya menggambarkan jalur sesi selesai normal | `HMD-DEC-012` |
| 4 | Penugasan petugas menyimpan **status verifikasi kewenangan bernilai tiga**, dan penegakannya berupa pengaturan. PRD menuntut validasi tanpa menyebut bagaimana bila datanya belum terbaca | `HMD-DEC-013` |

### Lima syarat yang mengikat desain

Kelimanya lahir dari satu prinsip: **melonggarkan aturan itu murah, mengetatkannya mahal.**

| # | Syarat | Asal |
|---:|---|---|
| 1 | Daftar item Pra-HD yang boleh dilewati **wajib** disimpan sebagai data, bukan ditanam di kode | `HMD-ASM-001` |
| 2 | Catatan sesi **wajib** menyimpan dua pelaku terpisah: penyelesai dokumentasi dan pengesah | `HMD-ASM-002` |
| 3 | Rasio perawat–pasien dan masa berlaku hasil pemeriksaan air **wajib** berupa pengaturan bernilai awal | `RCG-P-05`, `RCG-P-06` |
| 4 | Sesi `Stopped` **wajib** tetap menerbitkan tindakan dengan penanda tidak dapat ditagih beserta alasan | `HMD-DEC-012` |
| 5 | Status verifikasi kewenangan **wajib** punya tiga nilai sejak awal, dan penegakannya **wajib** berupa pengaturan | `HMD-DEC-013` |

Ditambah tiga titik sentuh maju ke Phase 2/3 pada bagian *Titik Sentuh Maju*: `HmdEpisode`,
`HmdVascularAccess`, dan `HmdSessionComplication` harus berkunci stabil dan bukan tabel yang
isinya ditimpa.

---

## Status Pass

| Hal | Keadaan |
|---|---|
| Keputusan `approved` | 10 — `HMD-DEC-001` sampai `004`, `006` sampai `013` |
| Pertanyaan tertutup | 3 dari 7 — `HMD-OQ-002`, `HMD-OQ-005`, `HMD-OQ-006` |
| Pertanyaan terbuka | 4 — `HMD-OQ-000`, `003`, `004`, `007`. Seluruhnya **digeser dari `DESIGN` ke `GO-LIVE`** oleh `HMD-DEC-010` |
| Aturan bawaan sementara | 3 — `HMD-ASM-001` sampai `HMD-ASM-003`, berstatus `draft`, tidak disahkan siapa pun |
| Gerbang go-live | 6 — `HMD-GATE-001` sampai `HMD-GATE-006` |
| Conflict tertutup | `HMD-CONF-001` |
| Conflict dibawa ke pass Phase 2 | `HMD-CONF-002` |
| Dependency lintas modul | 2 — `HMD-DEP-001` (Laboratorium), `HMD-DEP-002` (Human Resource) |
| Acceptance criteria dapat diuji | 24 |
| Capability map | Selesai — revision 1, nol duplikasi, 3 temuan kritis |
| Blocker `IMPLEMENTATION` | **Tidak ada** — registry disetujui `HMD-DEC-007`, tinggal 5 tindakan lanjutan |
| Blocker `DESIGN` | **Tidak ada** — `DEC-HMD-001` ditutup `HMD-DEC-013`, `DEC-HMD-002` ditutup `HMD-DEC-012`. Dua belas slice siap |
| Gerbang `GO-LIVE` | **Enam, seluruhnya terbuka** |

**Scope pass ini selesai.** Seluruh keputusan yang menjadi wewenang pemilik modul sudah
tertutup, dan yang memerlukan badan klinis sudah dipindahkan ke gerbang go-live beserta aturan
bawaannya.

Gerbang kelengkapan requirement kemudian menemukan dua keputusan pemblokir baru yang tidak
terlihat pada wawancara — `DEC-HMD-001` dan `DEC-HMD-002`. Keduanya memblokir **dua slice
saja**, bukan seluruh modul: `design-business-module` boleh dijalankan untuk sepuluh slice
sisanya, yang mencakup 24 dari 26 kemampuan.

Yang belum boleh adalah melayani pasien sungguhan — lihat *Gerbang Go-Live*.

---

## Langkah Berikutnya

1. ~~Jalankan `design-business-module`~~ — **selesai** 18 September 2026. Delapan belas berkas
   blueprint, contract version `HMD-CONTRACT-v1`, seluruhnya berstatus `draft`. Kelima syarat
   pada bagian *Dampak Desain* diwujudkan sebagai kolom dan pengaturan, bukan sebagai aturan
   yang ditanam di kode.
1b. **Mintakan persetujuan pemilik atas blueprint dan kontrak**, lalu jalankan
   `plan-module-delivery`.
2. **Kerjakan 5 tindakan lanjutan registry** pada bagian *Tindakan Lanjutan*. Butuh wewenang
   tulis terpisah karena berada di luar `docs/module-blueprints/**`.
3. **Sampaikan `HMD-DEP-001` dan `HMD-DEP-002`** ke pemilik Laboratorium dan Human Resource.
   Berjalan paralel; keduanya baru mengikat saat implementasi, bukan saat desain.
4. **Ajukan penunjukan badan klinis** ke manajemen rumah sakit (`HMD-GATE-001`). Tidak menahan
   desain maupun implementasi, tetapi menahan go-live — jadi makin awal diajukan makin baik.
5. Setelah badan klinis ditunjuk, jalankan `grill-me` lanjutan untuk menutup `HMD-OQ-003`,
   `HMD-OQ-004`, dan `HMD-OQ-007`, menggantikan ketiga aturan bawaan.

~~Jalankan `trace-existing-capabilities`~~ — **selesai** 18 September 2026,
`01-existing-capability-map.md` revision 1.
