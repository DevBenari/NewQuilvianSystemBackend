# Hemodialisa — Interview Decisions

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Pass | `Scope pass` — sedang berjalan, belum selesai |
| Product/domain owner | **BELUM DITETAPKAN** — lihat `HMD-OQ-000` |
| Clinical governance owner | **BELUM DITETAPKAN** — lihat `HMD-OQ-000` |
| Backend SHA | `69b256ca` (branch `MHamzah`) |
| Frontend SHA | `8143874d8` (branch `HamzahV2`) |
| Tanggal sesi | 18 September 2026 |
| Capability map | **TIDAK ADA** — `01-existing-capability-map.md` belum pernah dibuat. Lihat Peringatan Prasyarat butir 1 |
| Dokumen masukan | `docs/Modul-RS/Hemodialisa/PRD TO MVP—Hemodialisa Quilvian V2.md`, status `DRAFT — GRILL READY`, 18 September 2026 |
| Task mode | `MODULE BLUEPRINT MODE` — hanya `docs/module-blueprints/**` yang boleh ditulis |

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

Empat hal berikut wajib dibaca sebelum isi dokumen ini dipakai untuk apa pun.

### 1. Scope dikunci tanpa audit kemampuan existing yang formal

Belum ada `01-existing-capability-map.md` untuk modul ini. Artinya belum ada audit menyeluruh
yang memeriksa apakah kemampuan yang diminta sudah tersedia di tempat lain dalam sistem.

Batas scope pada dokumen ini diambil dari dokumen PRD masukan, bukan dari audit. Yang sudah
dilakukan hanyalah **verifikasi terarah** atas klaim-klaim PRD yang paling menentukan — hasilnya
ada pada bagian *Fakta Terverifikasi*. Verifikasi terarah tidak sama dengan audit menyeluruh:
kemungkinan duplikasi pada bagian yang tidak diverifikasi **belum diperiksa**.

**Akibatnya:** dokumen ini belum boleh dipakai sebagai dasar desain final sebelum
`trace-existing-capabilities` dijalankan.

### 2. Dokumen PRD masukan ditulis pada snapshot source yang sudah berbeda

| Repository | SHA pada PRD | SHA saat wawancara ini | Sama? |
|---|---|---|---|
| `NewQuilvianSystemBackend` | `896014ec` | `69b256ca` | **Tidak** |
| `QuilvianSystemFrontendDev` | `c42d6ab2` | `8143874d8` | **Tidak** |

Seluruh klaim PRD tentang keadaan source — entity apa yang sudah ada, enum apa yang tersedia,
apa yang sudah dipakai modul lain — dihitung pada snapshot lama. Klaim yang tidak diverifikasi
ulang pada dokumen ini berstatus **belum tentu masih benar**.

### 3. Dokumen PRD masukan belum berstatus disetujui

PRD masukan berstatus `DRAFT — GRILL READY`, bukan `APPROVED`. PRD itu sendiri menyatakan
belum boleh masuk `plan-module-delivery` sebelum empat pertanyaan pemblokirnya ditutup.

Dokumen PRD itu juga **bukan** keluaran `design-business-module` dan tidak berada di dalam
`docs/module-blueprints/`. Dalam alur kerja Quilvian ia berkedudukan sebagai **bukti masukan
(evidence)**, sejajar dengan SOP atau notulen rapat — bukan sebagai blueprint yang sudah
disahkan. Keputusan yang dikunci di dalamnya (`HD-DEC-001` sampai `HD-DEC-017`) diperlakukan
di sini sebagai **usulan yang perlu ditegaskan pemiliknya**, bukan sebagai keputusan yang
sudah mengikat.

### 4. Pemilik kebutuhan yang berwenang belum tercatat

Belum ada nama pemilik proses bisnis, pemilik klinis, maupun pemilik implementasi untuk modul
ini. Tanpa itu, tidak satu pun jawaban pada sesi ini boleh dicatat sebagai `approved`.
Seluruh keputusan pada sesi ini berstatus `draft` sampai pemiliknya ditetapkan.

---

## Batas Scope

Modul: **Hemodialisa** (`hemodialisa`).

**Satu kalimat batas scope:** Hemodialisa mengelola program cuci darah seorang pasien dan
pelaksanaan setiap sesi cuci darahnya di unit HD — dari pasien dinyatakan layak, diberi resep
HD, dijadwalkan, dikerjakan, dipantau, sampai catatan sesinya dikunci dan tindakannya
diteruskan ke jalur penagihan yang sudah ada.

### Di dalam scope (MUST)

Diambil dari 25 Feature `MUST HAVE` pada PRD masukan bagian 7.1.

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

### Di luar scope — milik modul lain

Kemampuan berikut **tidak** dibahas di sini. Titik sentuhnya saja yang dibahas.

| Kemampuan | Modul pemilik | Titik sentuh Hemodialisa |
|---|---|---|
| Identitas dan data induk pasien | Patient Management | Membaca `MstPatient` |
| Kunjungan/pendaftaran pasien | Registration Management | Membaca `RegPatientEncounter` |
| Siklus rawat inap | Inpatient Management | Menyimpan rujukan opsional ke `InpEpisode` |
| Hasil pemeriksaan laboratorium | Laboratorium | Membaca dan merujuk hasil, tidak menyalin |
| Stok, batch, dan penyaluran obat | Farmasi | Meneruskan fakta pemberian ke `PhmDrugUsage` |
| Harga, diskon, invoice, pembayaran, klaim | Billing dan Kasir | Menyerahkan tindakan selesai lewat `TrxPatientProcedure` |
| Integritas dan koreksi dokumen rekam medis | Rekam Medis | Mendaftarkan dokumen sesi final, koreksi lewat *addendum* |
| Data kompetensi, STR, SIP, sertifikat petugas | Human Resource / Workforce | Merujuk profil, tidak menyalin |
| Perawatan teknis dan pengadaan mesin | Belum ada pemilik tercatat | Hanya menyimpan status laik pakai |

### Di luar scope — ditunda ke gelombang berikutnya

`FEAT-002`, `FEAT-020` sampai `FEAT-025`, `FEAT-031`, `FEAT-032`, `FEAT-034`, `FEAT-035`.
Termasuk di dalamnya: CAPD/dialisis peritoneal, transplantasi ginjal, operasi pembuatan akses
vaskular, dashboard adekuasi/Kt/V, pemantauan longitudinal, integrasi otomatis mesin HD,
mesin alarm klinis otomatis, rekomendasi AI, KPI unit HD, SATUSEHAT Uronefrologi, pelaporan
regulator tahunan, portal pasien, grouping INA-CBG, adjudikasi klaim, dan pemakaian ulang
dialyzer (*dialyzer reuse*).

### Di luar scope — untuk modul lain, ditemukan selama sesi ini

| Temuan | Catatan |
|---|---|
| Permintaan layanan penunjang dari dokter rawat inap | Lihat `HMD-CONF-001`. Titik masuk permintaan HD sudah dijanjikan layar Rawat Inap, tetapi PRD Hemodialisa tidak memuat entity permintaan apa pun |

---

## Fakta Terverifikasi dari Source

Seluruh baris berikut diverifikasi ulang pada backend `69b256ca` dan frontend `8143874d8`,
bukan pada snapshot yang dipakai PRD.

| ID | Fakta | Bukti |
|---|---|---|
| `HMD-FACT-001` | Registry kepemilikan modul **belum** memuat Hemodialisa. Pencarian kata `hemodial` dan prefix `Hmd` tidak menghasilkan satu baris pun | `NewQuilvianSystemBackend/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — 0 kecocokan, SHA `69b256ca` |
| `HMD-FACT-002` | Prefix `Hmd` **belum dipakai** modul mana pun. Daftar kepanjangan prefix memuat 23 prefix dan `Hmd` tidak termasuk | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:36-58` |
| `HMD-FACT-003` | `ServiceUnitType` berisi nilai 0–9 lalu langsung `Other = 99`. Nilai `10` **kosong**, sehingga usulan `Hemodialysis = 10` tidak menabrak nilai lain | `Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs:3-16` |
| `HMD-FACT-004` | `ClinicalDocumentKind` berhenti di `Consent = 13`. Nilai `14` **kosong**, sehingga usulan `HemodialysisSession = 14` tidak menabrak nilai lain | `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs:11-26` |
| `HMD-FACT-005` | `TrxPatientProcedure.DoctorId` bertipe `Guid` **wajib diisi** (`[Required]`) | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:29-30` |
| `HMD-FACT-006` | `TrxPatientProcedure` **juga** memiliki `InstructingDoctorId` bertipe `Guid?` (boleh kosong) beserta status verifikasi instruksi dokter | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs:246-252` |
| `HMD-FACT-007` | Tidak ada satu pun berkas `.cs` backend yang menyebut hemodialisa/dialysis. Klaim PRD bahwa bounded context ini `MISSING / NEW` **masih benar** pada SHA sekarang | Pencarian `hemodial|dialisa|dialysis` pada `NewQuilvianSystemBackend/**/*.cs` — 0 berkas |
| `HMD-FACT-008` | Frontend **sudah** memesan kunci menu bersarang bernama `menuHemodialisa`, tetapi kunci itu hanya terdaftar sebagai nama; belum ada butir menu, route, atau layar apa pun di baliknya | `QuilvianSystemFrontendDev/src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:9` |
| `HMD-FACT-009` | Layar *physician workspace* Rawat Inap sudah menampilkan kartu layanan penunjang **Hemodialisa** berlabel "Integrasi belum tersedia", dengan keterangan "Pelayanan hemodialisis rutin dan cito rawat inap, pemantauan adekuasi dialisis, dan akses vaskular" | `QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx:12-18, 70-79` |
| `HMD-FACT-010` | Registry mencatat prosedur pendaftaran modul/prefix baru, dan langkah ke-5 berbunyi "Minta persetujuan pemilik modul, lalu tambahkan barisnya ke tabel dan catat di *Catatan perubahan lifecycle*" | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:63-71` |
| `HMD-FACT-011` | Persetujuan registry **hanya** memberi wewenang penamaan dan kepemilikan; ia tidak memberi wewenang implementasi, migration, pekerjaan database, maupun deployment | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:5` |
| `HMD-FACT-012` | Registry di repository backend pernah **tertinggal** dari registry canonical pada suite Skill, dan selisih itu memblokir entity baru pada Accounting (`ACC-DEP-007`) dan Radiologi (`RAD-REQ-001`). Selisih dua salinan registry tercatat masih terbuka | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md:113-114` |

---

## Conflict yang Ditemukan Sesi Ini

### `HMD-CONF-001` — Titik masuk permintaan HD dari Rawat Inap tidak punya pemilik

**Bukti yang bertentangan:**

| Sumber | Yang dinyatakan |
|---|---|
| Frontend `inpatient-supporting-service-constants.jsx:70-79` (`HMD-FACT-009`) | Dokter rawat inap akan punya kartu "Hemodialisa" pada layar layanan penunjang, untuk "hemodialisis rutin dan cito rawat inap". Statusnya "Integrasi belum tersedia" — artinya layar itu **menunggu backend Hemodialisa** |
| PRD masukan bagian 9, langkah 1 | Alur dimulai dari "Pasien datang / permintaan HD diterima" |
| PRD masukan bagian 12.3 | Daftar 15 entity baru **tidak memuat satu pun** entity permintaan/order HD |
| PRD masukan bagian 13 | Tidak ada satu pun endpoint yang menerima permintaan HD dari modul lain |

**Mengapa ini material:** PRD mengunci alur yang dimulai dari "permintaan HD diterima", tetapi
tidak pernah menetapkan **siapa yang membuat permintaan itu dan di mana ia disimpan**. Pada saat
yang sama, layar Rawat Inap yang sudah jadi menjanjikan tombol yang akan memanggil Hemodialisa.
Bila ini tidak diputuskan sekarang, hasilnya salah satu dari dua keadaan yang sama buruknya:
kartu di Rawat Inap tetap mati setelah MVP Hemodialisa selesai, atau entity permintaan lahir
belakangan tanpa pernah masuk hitungan MVP.

**Status:** terbuka. Lihat `HMD-OQ-005`.

---

## Penilaian Bentuk Blueprint

Penilaian ini adalah **saran**, bukan keputusan. Pemilik modul yang memutuskan.

### Rumpun kemampuan

Ke-25 kemampuan `MUST` jatuh ke tiga rumpun, disusun dari kepemilikan data dan siklus hidup —
bukan dari layar atau menu.

| Rumpun | Kemampuan | Calon tabel yang dimiliki | Pemilik peran |
|---|---|---|---|
| **A. Unit dan Sumber Daya** | `FEAT-012`, `FEAT-026`, `FEAT-027`, `FEAT-028`, `FEAT-029`, `FEAT-030` | `HmdMachine`, `HmdStation`, `HmdUnitReadiness`, `HmdUnitReadinessItem`, `HmdIsolationDecision` | Koordinator Unit, PPI |
| **B. Episode dan Program Klinis** | `FEAT-001`, `FEAT-003`, `FEAT-004`, `FEAT-005`, `FEAT-006`, `FEAT-007`, `FEAT-008`, `FEAT-009` | `HmdEpisode`, `HmdEligibilityAssessment`, `HmdVascularAccess`, `HmdPrescription` | Dokter Dialisis, DPJP |
| **C. Pelaksanaan Sesi dan Penutupan** | `FEAT-010`, `FEAT-011`, `FEAT-013` sampai `FEAT-019`, `FEAT-033`, `FEAT-036` | `HmdSession`, `HmdSessionAssessment`, `HmdSessionObservation`, `HmdSessionMedication`, `HmdSessionComplication`, `HmdSessionStaffAssignment` | Perawat Dialisis, Rekam Medis |

Catatan: `FEAT-009` (penjadwalan) diletakkan di rumpun B karena jadwal lahir dari resep yang
aktif, tetapi ia memakai mesin dan station milik rumpun A. Penempatannya perlu ditegaskan bila
bentuk `COMPOSITE` dipilih.

### Hasil uji lima syarat pemecahan

| Syarat | A. Unit & Sumber Daya | B. Episode & Program | C. Pelaksanaan Sesi |
|---|:---:|:---:|:---:|
| 1. Punya bounded context sendiri yang dimiliki modul ini | Ya | Ya | Ya |
| 2. Punya kosakata status sendiri yang tidak beririsan | Ya — `Ready`/`Blocked`/`Maintenance`/`NotEligible` dan `Draft`/`Ready`/`NotReady` | Ya — `Draft`/`Active`/`Suspended`/`Closed` dan `Draft`/`Active`/`Superseded`/`Cancelled` | Ya — 12 status dari `Planned` sampai `Finalized` |
| 3. Punya Resource hak akses sendiri dan pemilik peran berbeda | Ya — `HemodialysisMachine`, `HemodialysisUnitReadiness`; koordinator dan PPI | Ya — `HemodialysisEpisode`, `HemodialysisEligibility`, `HemodialysisPrescription`; dokter | Ya — `HemodialysisSession`, `HemodialysisObservation`, `HemodialysisMedication`; perawat |
| 4. Dapat dirilis sendiri sebagai gelombang MVP terpisah | Ya — pemeriksaan kesiapan unit harian sudah berguna sebelum ada sesi | Ya — pendaftaran program HD dan resep sudah berguna sebagai daftar pasien HD | **Tidak** — sesi mustahil berjalan tanpa A dan B |
| 5. Punya master data sendiri dan pemilik approval sendiri | Ya — mesin dan station adalah data induk; approval koordinator unit | Sebagian — tidak punya data induk sendiri, tetapi approval resep milik dokter | **Tidak punya data induk**; approval finalisasi masih terbuka (`HMD-OQ-003`) |
| **Skor** | **5 / 5** | **4 / 5** | **3 / 5** |

Ketiga rumpun memenuhi sekurang-kurangnya 3 dari 5 syarat. Menurut
`rules/rule-output/bentuk-blueprint.md` bagian 4.1, keadaan ini menghasilkan saran `COMPOSITE`.

### Keberatan yang harus ikut dipertimbangkan

Saran di atas punya satu keberatan yang jujur dan tidak boleh disembunyikan: **PRD masukan
mendefinisikan keberhasilan MVP sebagai satu kalimat yang melintasi ketiga rumpun** — "satu
pasien dapat menjalani satu sesi Hemodialisa secara lengkap dan aman ... dan tindakan masuk ke
jalur Billing". Dua puluh dua butir *Definition of Done* pada PRD bagian 19 juga melintasi
ketiganya.

Pada bentuk `COMPOSITE`, tidak ada satu berkas pun yang secara alami memiliki DoD lintas
rumpun itu; ia harus dititipkan ke `02-module-map.md`, dan status modul menjadi turunan dari
sub-modul terlemah.

**Status:** terbuka. Lihat `HMD-OQ-001`.

---

## Decision Log

Belum ada satu pun keputusan berstatus `approved` pada modul ini.

| Decision ID | Type | Keputusan/pertanyaan | Owner | Status | Approved by/at | Evidence |
|---|---|---|---|---|---|---|
| `HMD-DEC-001` | Decision | Batas scope modul dikunci sesuai daftar **Di dalam scope (MUST)** di atas, yaitu 25 Feature `MUST HAVE` | Menunggu pemilik | `draft` | — | PRD masukan bagian 7.1 |
| `HMD-DEC-002` | Decision | Kemampuan pada daftar **Di luar scope** tidak dikerjakan pada blueprint ini | Menunggu pemilik | `draft` | — | PRD masukan bagian 5.3 dan 8 |
| `HMD-DEC-003` | Open Question | Bentuk blueprint: `SINGLE` atau `COMPOSITE` | Menunggu pemilik | `draft` | — | Penilaian bentuk di atas |

### Keputusan yang diwarisi dari PRD masukan

Ketujuh belas keputusan `HD-DEC-001` sampai `HD-DEC-017` pada PRD masukan Appendix D
diperlakukan sebagai **usulan yang perlu ditegaskan**, bukan keputusan yang sudah mengikat,
karena PRD-nya sendiri berstatus `DRAFT` dan pemiliknya belum tercatat (Peringatan Prasyarat
butir 3 dan 4). Penegasannya dilakukan pada pass berikutnya, tidak diulang satu per satu di
sesi ini.

Dua di antaranya sudah diverifikasi **tidak menabrak source**: `HD-DEC-004`
(`ServiceUnitType.Hemodialysis = 10`, lihat `HMD-FACT-003`) dan `HD-DEC-013`
(`ClinicalDocumentKind.HemodialysisSession = 14`, lihat `HMD-FACT-004`).

---

## Open Questions dan Blocker

| ID | Pertanyaan | Owner yang dibutuhkan | Memblokir |
|---|---|---|---|
| `HMD-OQ-000` | Siapa pemilik proses bisnis, pemilik klinis, dan pemilik implementasi modul Hemodialisa? | Lead | `DESIGN` — tanpa ini tidak ada jawaban yang boleh dicatat `approved` |
| `HMD-OQ-001` | Bentuk blueprint `SINGLE` atau `COMPOSITE`? | Pemilik modul | `DESIGN` — menentukan letak 14 atau 37 berkas |
| `HMD-OQ-002` | Apakah pendaftaran registry `HealthServices / HemodialysisManagement / Hmd / ACTIVE` disetujui? | Pemilik registry | `IMPLEMENTATION` — `QBE-MOD-002` memblokir entity persisted pertama |
| `HMD-OQ-003` | Pada kekurangan klinis di Pra-HD, item apa yang sama sekali tidak boleh di-*override*, dan item mana yang boleh di-*override* dokter dengan alasan tercatat? | Pemilik klinis | `DESIGN` — menentukan *validation matrix* dan *state transition* |
| `HMD-OQ-004` | Siapa yang berwenang membuat sesi HD menjadi `Finalized`? | Pemilik klinis | `DESIGN` — menentukan authorization, penguncian, dan *addendum* |
| `HMD-OQ-005` | `TrxPatientProcedure.DoctorId` yang wajib diisi itu diisi dokter yang mana? | Pemilik klinis | `DESIGN` — menentukan kontrak serah terima Billing |
| `HMD-OQ-006` | Siapa pemilik titik masuk permintaan HD dari Rawat Inap, dan apakah ia masuk MVP ini? | Pemilik modul + pemilik Rawat Inap | `DESIGN` — lihat `HMD-CONF-001` |

---

## Acceptance Criteria yang Sudah Dapat Diuji

Butir berikut sudah cukup tegas untuk diuji tanpa menunggu jawaban open question mana pun.
Butir ini berasal dari PRD masukan bagian 18 yang sudah terverifikasi tidak bertentangan
dengan source.

| ID | Kriteria | Cara menguji |
|---|---|---|
| `HMD-AC-001` | Satu pasien tidak boleh punya dua episode HD berstatus `Active` untuk program yang sama pada saat bersamaan | Buat episode kedua saat episode pertama masih `Active`; harus ditolak |
| `HMD-AC-002` | Satu mesin tidak boleh dipakai dua sesi yang waktunya bertumpang tindih | Jadwalkan dua pasien pada mesin dan waktu yang sama; hanya satu yang berhasil |
| `HMD-AC-003` | Satu station tidak boleh dipakai dua sesi yang waktunya bertumpang tindih | Sama seperti `HMD-AC-002`, pada station |
| `HMD-AC-004` | Satu pasien tidak boleh terjadwal pada dua sesi yang waktunya bertumpang tindih | Jadwalkan pasien yang sama dua kali pada waktu bertumpang tindih |
| `HMD-AC-005` | Mesin berstatus `Blocked`, `Maintenance`, atau `NotEligible` tidak boleh dipakai menjadwalkan maupun memulai sesi | Ubah status mesin, lalu coba jadwalkan dan mulai |
| `HMD-AC-006` | Menekan tombol Mulai dua kali menghasilkan tepat satu sesi berjalan dan tepat satu `TrxPatientProcedure` | Kirim dua permintaan `start` dengan kunci idempotency yang sama |
| `HMD-AC-007` | Pencatatan pemantauan berkala tidak boleh saling menimpa; seluruh riwayat tampil urut waktu | Simpan lima observasi berurutan, lalu baca kembali |
| `HMD-AC-008` | Sesi yang sudah `Finalized` menolak perubahan langsung; koreksi hanya lewat *addendum* | Kirim permintaan ubah pada sesi final; harus ditolak |
| `HMD-AC-009` | Kegagalan serah terima Billing tidak mengubah sesi dari `Finalized` menjadi tidak final | Gagalkan Billing secara sengaja, lalu periksa status sesi |
| `HMD-AC-010` | Bila konteks pasien atau sesi gagal diverifikasi, tidak ada satu pun penulisan data klinis yang diterima | Putus verifikasi konteks, lalu coba menyimpan data klinis |
| `HMD-AC-011` | Waktu `StartedAt`, `CompletedAt`, dan `FinalizedAt` memakai waktu server, bukan waktu dari peramban pengguna | Kirim waktu palsu dari klien; nilai tersimpan harus tetap waktu server |
| `HMD-AC-012` | Hak akses ditegakkan di server, bukan sekadar tombol dimatikan di layar | Panggil endpoint langsung dengan pengguna tanpa hak; harus 403 |

---

## Yang Sengaja Tidak Ditanyakan

PRD masukan bagian 20.4 memuat daftar hal yang sudah dijelaskan dan tidak boleh ditanyakan
ulang. Daftar itu dipatuhi. Ringkasnya, sesi ini **tidak** menanyakan: pemakaian pasien dan
kunjungan yang sudah ada, letak menu Hemodialisa, jumlah submenu, route utama, kepemilikan
hasil laboratorium dan stok farmasi, pembuatan `SourceDomain` Billing baru, pembagian sesi
menjadi Pra/Intra/Pasca, siklus hidup sesi, cara koreksi setelah final, integrasi perangkat,
KPI, SATUSEHAT, interval pemantauan wajib, penjadwalan prioritas otomatis, dan ambang alarm
klinis otomatis.

Satu pertanyaan tambahan dibuat di luar empat pertanyaan yang diizinkan PRD, yaitu
`HMD-OQ-006`, karena memenuhi syarat pengecualian PRD: *conflict* baru yang material ditemukan
pada source (`HMD-CONF-001`, bukti `HMD-FACT-009`).

---

## Langkah Berikutnya

1. Tutup `HMD-OQ-000` sampai `HMD-OQ-006`.
2. Jalankan `trace-existing-capabilities` untuk membuat `01-existing-capability-map.md` —
   wajib, karena scope ini dikunci tanpa audit (Peringatan Prasyarat butir 1) dan snapshot PRD
   sudah kedaluwarsa (butir 2).
3. Setelah keduanya selesai, barulah `design-business-module` boleh dijalankan.
