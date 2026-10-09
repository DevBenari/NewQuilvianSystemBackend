# Requirement Traceability — Patient Management

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `blueprint_revision` | `2` |
| `roadmap_revision` | `2` — 8 Oktober 2026 |
| Approval | Task, acceptance criteria, dan keputusan review `PAT-OQ-001`–`007` disetujui pemilik 8 Oktober 2026 |
| Snapshot source backend | `QuilvianStaDeploy` @ `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` |
| Masukan | `00-interview-decisions.md` revision `2`, `sha256:fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885` |
| Kontrak | `contracts/api-contract.md` `1.1.0`, `sha256:cb41e4d5a0f200bb8b234ad4e741413c1ba96837140fa38460a2a78dd710ad7e` |
| Roadmap | [backend-roadmap.md](backend-roadmap.md), [frontend-roadmap.md](frontend-roadmap.md) |

Dokumen ini menjawab satu pertanyaan: **requirement ini dijawab siapa, dan dengan bukti apa.**
Roadmap menjawab "task ini sampai mana".

---

## 1. Cara membaca kolom tahap

Setiap requirement melewati tiga tahap terpisah. Ketiganya tidak boleh dicampur, karena
wewenangnya berbeda.

| Kolom | Artinya | Wewenang |
| --- | --- | --- |
| **Implementasi** | Source yang memenuhi requirement sudah ditulis | `BE-PAT-MIG-001` |
| **Verifikasi** | Bukti bahwa source itu benar: test terfokus, build, penelusuran source, review diff | `BE-PAT-MIG-001` |
| **Runtime** | Bukti pada data staging nyata sesudah endpoint benar-benar dijalankan | **Bukan** `BE-PAT-MIG-001`. Menunggu otorisasi terpisah `PAT-GATE-001` |

Nilai yang dipakai:

| Nilai | Artinya |
| --- | --- |
| `NANTI` | Belum dikerjakan, dan memang bagian tahap itu |
| `TIDAK BERLAKU` | Requirement ini sudah terbukti penuh tanpa tahap itu |

Contoh: `AC-22` (tabel cadangan tidak boleh diubah) dibuktikan **dua kali**. Pertama oleh test
pada tahap verifikasi, sebelum endpoint pernah dijalankan. Kedua pada tahap runtime: sesudah
rekonsiliasi di staging, tabel cadangan masih berisi 715 baris yang sama persis.

---

## 2. Requirement → task → bukti

**Diperbarui 8 Oktober 2026 oleh `build-module-backend`.** Tahap implementasi dan verifikasi
ke-28 requirement **terpenuhi**: source ada, `dotnet test` project
`QuilvianSystemBackend.PatientManagementTests` `72` lulus / `0` gagal sesudah koreksi owner review
(sebelumnya `66`), `dotnet build -p:RunAnalyzers=false --no-incremental` `0 Error(s)`, dan QBE
`Strict` *working tree* `PASS`. Tahap runtime **NOT EXECUTED** — menunggu
`PAT-GATE-001`. Task berstatus 🟡 karena butir DoD "source sudah direview pengguna/leader" belum
terpenuhi. Pemetaan tiap requirement ke nama test ada di laporan bagian 5.1 dan 6.

**Diperbarui 9 Oktober 2026 — port semantik ke Integration.** Source dipindahkan maknanya ke branch
`BE-PAT-MIG-001` di atas `QuilvianIntegrationBackend` @ `179aea3f` dan divalidasi ulang di sana:
`dotnet test` `73` lulus / `0` gagal / `0` dilewati (72 test rujukan + 1 test delta
`PatientQrPayloadBuilder`), `dotnet build -p:RunAnalyzers=false --no-incremental` `0 Error(s)`,
QBE `Strict` *working tree* `PASS`. Kontrak SQL `IDENTICAL` dengan rujukan. Tahap implementasi dan
verifikasi ke-28 requirement tetap **terpenuhi** pada baseline baru; tahap runtime tetap
**NOT EXECUTED — PENDING SEPARATE AUTHORIZATION** (`PAT-GATE-001` terbuka). Rinciannya di laporan
bagian 9.

**Diperbarui 9 Oktober 2026 — review source leader `APPROVED`.** Butir DoD terakhir yang menahan
🟡 terpenuhi, sehingga `BE-PAT-MIG-001` kini ✅ untuk cakupannya. **SOURCE IMPLEMENTATION:
APPROVED** — kolom Implementasi dan Verifikasi ke-28 requirement tetap `TERPENUHI`/`PASS`.
**RUNTIME: PENDING** — kolom Runtime **tidak** berubah: 15 baris `NANTI` tetap menunggu
`PAT-GATE-001`, yang masih terbuka. Rinciannya di laporan bagian 10.

Sebelum pembaruan 8 Oktober 2026, seluruh baris berstatus belum dikerjakan.

| Requirement | Ringkasan | Keputusan | Kontrak `1.1.0` | Task BE | Task FE | Implementasi | Verifikasi | Runtime | Laporan |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PAT-MIG-AC-01` | Aturan memilih pasien: batch, `FINALIZED`, crosswalk, Id cocok, MRN berbeda | `PAT-DEC-004`, `006` | Bagian 3 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test *dry run* dan urutan; penelusuran query | `NANTI` — *dry run* staging pertama menampilkan 715 calon | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-02` | MRN tujuan hanya dari perencana, bukan `normalized_mrn` | `PAT-DEC-005` | Bagian 3 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — penelusuran source; test | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-03` | `dryRun=true` tanpa perubahan database dan tanpa tulis file | `PAT-DEC-011`, `PAT-OQ-003` | Bagian 2.1, 4.2 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test *dry run*, termasuk body tanpa kolom `dryRun` | `NANTI` — sesudah *dry run* staging, database dan folder QR tidak berubah | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-04` | Isi minimal baris dan urutan `legacy_pid` | `PAT-DEC-011` | Bagian 2.2 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test; verifikasi kontrak DTO | `NANTI` — keluaran *dry run* staging | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-05` | Gerbang environment, produksi ditolak, tak dikenal ditolak | `PAT-DEC-011`, `PAT-OQ-002`, `PAT-OQ-006` | Bagian 6 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test gerbang environment untuk kedua mode, termasuk `staging`, salah ketik, kosong, `Production`; penelusuran source bahwa pemeriksaannya daftar izin positif | `TIDAK BERLAKU` — tidak pernah dijalankan di produksi | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-06` | Wajib login dan izin Patient Update atau lebih ketat | `PAT-DEC-011` | Bagian 7 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — atribut terpasang pada controller; test otorisasi | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-07` | `Id`, `PatientCode`, demografi, relasi, `CreateDateTime`, `CreateBy` tetap | `PAT-DEC-007` | Bagian 9 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test pelestarian identitas | `NANTI` — dibandingkan dengan tabel cadangan | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-08` | Hanya empat field yang berubah | `PAT-DEC-007` | Bagian 9 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test; penelusuran source | `NANTI` — dibandingkan dengan tabel cadangan | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-09` | QR baru dari MRN kanonik | `PAT-DEC-008` | Bagian 5 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test perilaku QR | `NANTI` — file QR baru ada di folder MRN kanonik | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-10` | QR lama tidak tersentuh | `PAT-DEC-008` | Bagian 5 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test perilaku QR | `NANTI` — 715 file QR lama masih ada | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-11` | QR gagal → pasien tidak berubah | `PAT-DEC-008` | Bagian 4.4 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test kegagalan QR | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-12` | Database gagal sesudah QR → hanya QR baru dibersihkan | `PAT-DEC-008` | Bagian 4.4 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test kegagalan database sesudah QR | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-13` | Artefak QR tujuan yang sudah ada tidak ditimpa | `PAT-DEC-008`, `PAT-OQ-005` | Bagian 4.2, 4.3, 5 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test: bila artefak tujuan ada, helper tidak dipanggil dan status `CONFLICT`; penelusuran urutan pemeriksaan pada source | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-14` | Dapat dilanjutkan dan idempoten; `ALREADY_RECONCILED` | `PAT-DEC-011`, `PAT-OQ-004` | Bagian 3, 4.3 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test idempotensi | `NANTI` — panggilan terakhir melaporkan 715 `ALREADY_RECONCILED` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-15` | MRN kanonik tetapi QR tidak sesuai → `QR_INCONSISTENT` | `PAT-DEC-008`, `PAT-OQ-004` | Bagian 3, 4.3 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test: dilaporkan, tidak diubah, tidak dihitung dalam `limit` | `NANTI` — jumlah `QR_INCONSISTENT` dilaporkan | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-16` | Pembangkit MRN normal tidak dipanggil | `PAT-DEC-005` | Bagian 9 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — penelusuran source; review diff | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-17` | Tidak ada `PatientCode` baru | `PAT-DEC-007` | Bagian 9 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test pelestarian `PatientCode`; penelusuran source | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-18` | Tidak ada `MstPatient` baru | `PAT-DEC-007` | Bagian 9 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test; penelusuran source | `NANTI` — jumlah pasien Pilot tetap 715 | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-19` | Tidak ada `MstPatient` dihapus lalu dibuat ulang | `PAT-DEC-007` | Bagian 9 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test pelestarian identitas | `NANTI` — 715 `Id` sama dengan tabel cadangan | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-20` | Crosswalk tidak diubah | `PAT-DEC-006` | Bagian 8 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test keutuhan crosswalk | `NANTI` — crosswalk tidak berubah | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-21` | Perencana tidak diubah | `PAT-DEC-005` | Bagian 8 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test keutuhan perencana | `NANTI` — perencana tidak berubah | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-22` | Tabel cadangan tidak diubah | `PAT-DEC-009` | Bagian 8 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test keutuhan cadangan | `NANTI` — tabel cadangan tetap 715 baris yang sama | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-23` | 704.508 pasien lain di luar cakupan | `PAT-DEC-016` | Bagian 3 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — review diff dan cakupan | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-24` | Paling banyak `limit` baris, urutan deterministik | `PAT-DEC-011`, `PAT-OQ-004` | Bagian 2.1, 3 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test `limit` dan urutan; pasien kanonik tidak menghabiskan `limit` | `NANTI` — setiap panggilan memproses paling banyak `limit` pasien | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-25` | Satu transaksi per pasien | `PAT-DEC-011` | Bagian 4.2, 4.4 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test kegagalan di tengah batch | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-26` | Berhenti pada kegagalan tak terduga pertama | `PAT-DEC-011` | Bagian 4.2, 4.4 | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — test | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-27` | Test terfokus untuk 14 topik | `PAT-DEC-014`, `PAT-OQ-001` | — | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — keluaran perintah test, lulus/gagal apa adanya; Integration `179aea3f` 9 Oktober 2026: `73/73` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |
| `PAT-MIG-AC-28` | `dotnet build -p:RunAnalyzers=false` dan test terfokus | `PAT-DEC-014` | — | `BE-PAT-MIG-001` | — | `TERPENUHI` | `PASS` — keluaran build, jumlah error dan warning; Integration `179aea3f` 9 Oktober 2026: `0` error, `256` warning, nol dari berkas baru | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-001.md) |

Jumlah: **28 requirement**, seluruhnya dipetakan ke `BE-PAT-MIG-001`, tidak ada yang dipetakan ke
task frontend. 15 requirement punya bukti tahap runtime; 13 lainnya sudah terbukti penuh pada
tahap verifikasi.

---

## 3. Keputusan → requirement

| Keputusan | Requirement yang menjawabnya |
| --- | --- |
| `PAT-DEC-001` Penempatan modul | Seluruh baris — task berada di PatientManagement |
| `PAT-DEC-002` Bentuk `SINGLE` | Lokasi roadmap dan laporan |
| `PAT-DEC-003` Task dan klasifikasi | Seluruh baris |
| `PAT-DEC-004` Batch kanonik | `AC-01` |
| `PAT-DEC-005` Sumber MRN tujuan | `AC-02`, `AC-16`, `AC-21` |
| `PAT-DEC-006` Pemetaan identitas | `AC-01`, `AC-20` |
| `PAT-DEC-007` Identitas tetap | `AC-07`, `AC-08`, `AC-17`, `AC-18`, `AC-19` |
| `PAT-DEC-008` QR baru, QR lama tetap | `AC-09` sampai `AC-13`, `AC-15` |
| `PAT-DEC-009` Cadangan tak boleh diubah | `AC-22` |
| `PAT-DEC-010` Pilot lebih dulu | Urutan gerbang `PAT-GATE-001`; bukan kriteria source |
| `PAT-DEC-011` Bentuk mekanisme | `AC-01`, `AC-03` sampai `AC-06`, `AC-14`, `AC-24` sampai `AC-26` |
| `PAT-DEC-012` Batas wewenang | Verifikasi V7–V9 pada kartu task; gerbang `PAT-GATE-001` |
| `PAT-DEC-013` Tabel `migration.rsmmc_*` dipakai apa adanya | `AC-20` sampai `AC-22`; risiko `PAT-RSK-001` |
| `PAT-DEC-014` Automated test diminta | `AC-27`, `AC-28` |
| `PAT-DEC-015` Tanpa frontend | `frontend-roadmap.md` kosong |
| `PAT-DEC-016` Pasien lain di luar cakupan | `AC-23` |
| `PAT-OQ-001` Project xUnit khusus dengan governance guard | `AC-27`, `AC-28`; guard test pada kartu task |
| `PAT-OQ-002` Hanya `Staging` persis, kedua mode | `AC-05` |
| `PAT-OQ-003` `dryRun` tidak dikirim = `true` | `AC-03` |
| `PAT-OQ-004` Pasien kanonik tidak dihitung dalam `limit`; `QR_INCONSISTENT` dilaporkan | `AC-14`, `AC-15`, `AC-24` |
| `PAT-OQ-005` Periksa QR tujuan sebelum helper; `CONFLICT` | `AC-13` |
| `PAT-OQ-006` Daftar izin positif, bukan `!IsProduction()` saja | `AC-05` |
| `PAT-OQ-007` Tanpa EF migration dan perubahan skema | Verifikasi V11 pada kartu task; `PAT-DEC-012` |

Tidak ada keputusan yang tidak dijawab requirement, gerbang, atau dokumen. Ketujuh keputusan
`PAT-OQ` **mempertajam** requirement yang sudah ada; jumlah requirement tetap 28.

---

## 4. Gap requirement → bukti verifikasi

| Gap | Keadaan |
| --- | --- |
| Requirement tanpa rencana bukti verifikasi | **Tidak ada.** Ke-28 requirement punya bukti tahap verifikasi |
| Topik `AC-27` yang mungkin tidak dapat dibuktikan test EF Sqlite | **Tidak terjadi** untuk 37 topik yang diminta — seluruhnya punya test yang lulus (8 Oktober 2026). Yang tetap **tidak** terbukti oleh test: teks SQL PostgreSQL pada `RsmmcPilotMigrationSource`, karena tabel `migration.rsmmc_*` diganti tiruan. Pembuktiannya pada simulasi staging pertama sesudah `PAT-GATE-001`. Pembaruan 9 Oktober 2026: teks SQL tidak berubah pada port (`IDENTICAL`), sehingga bukti kompatibilitas PostgreSQL hanya-baca yang dilaporkan pemilik pada instruksi port tetap berlaku; agent tidak mengakses database |
| Bukti runtime 15 requirement | **Belum dapat dikumpulkan**, dan memang bukan bagian task ini. Menunggu `PAT-GATE-001` |
