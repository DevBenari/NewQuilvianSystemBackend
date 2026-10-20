# Requirement Traceability — Patient Management

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `blueprint_revision` | `6` — `approved` 19 Oktober 2026 (`PAT-DEC-069`–`077`); riwayat `5` (`PAT-DEC-068`), `4` (`PAT-DEC-067`), `3` (`PAT-DEC-033`); bagian 1–4 dokumen ini tetap milik revision `2` |
| `roadmap_revision` | `6` — 19 Oktober 2026: kartu `BE-PAT-MIG-003A`, `BE-PAT-MIG-003` ✅, amandemen `BE-PAT-MIG-002` (`PAT-DEC-069`–`077`). Riwayat `5`: 16 Oktober 2026, `PAT-DEC-068`. Riwayat `4`: 16 Oktober 2026, amandemen `PAT-DEC-067`. Riwayat `3`: 14 Oktober 2026, diamandemen 15 Oktober 2026 (bukti Step 16), amandemen kedua (keputusan `PAT-DEC-044`–`062`, task prasyarat `BE-PAT-MIG-003`), dan **difinalkan sesi ketiga** (`PAT-DEC-063`–`066`, `PAT-DEC-033`). Riwayat: `2` — 8 Oktober 2026 |
| Approval | `BE-PAT-MIG-001`: task, acceptance criteria, dan keputusan review `PAT-OQ-001`–`007` disetujui pemilik 8 Oktober 2026. `BE-PAT-MIG-002` (bagian 5) dan `BE-PAT-MIG-003` (bagian 6): **disetujui** 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`), sesudah `requirement-completeness-gate` `PASS`. `BE-PAT-MIG-003A` (bagian 7): **disetujui** 19 Oktober 2026 lewat instruksi tertulis pemilik (`PAT-DEC-069`–`074`) |
| Snapshot source backend | `BE-PAT-MIG-001`: `QuilvianStaDeploy` @ `103b45ccd5f0d9e2cbacff588540d8fe3d52e706`. `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`: `QuilvianIntegrationBackend` @ `a52b3c98e7e9b0fae42f9a889b84c6c8332f8923`. `BE-PAT-MIG-003A`: branch `BE-PAT-MIG-003A` @ `6ae9c251` di atas `QuilvianIntegrationBackend` @ `98fdc69ff0dcf97622d0c9c27779e9277e65d220` (dibangun semula di atas `c6537ee4`) |
| Masukan | `BE-PAT-MIG-001`: `00-interview-decisions.md` revision `2`, `sha256:fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885`. `BE-PAT-MIG-002` dan `BE-PAT-MIG-003`: revision `3` (`approved`) dan `requirement-completeness-assessment.md` — hash pada manifest |
| Kontrak | `BE-PAT-MIG-001`: `contracts/api-contract.md` `1.1.0`, `sha256:cb41e4d5a0f200bb8b234ad4e741413c1ba96837140fa38460a2a78dd710ad7e`. `BE-PAT-MIG-002`, `BE-PAT-MIG-003`, dan `BE-PAT-MIG-003A`: `1.3.0` `approved` 19 Oktober 2026 (riwayat `1.2.2`) — hash pada manifest |
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

**Catatan 14 Oktober 2026.** Pemilik melaporkan `PAT-GATE-001` `CLOSED/PASS`. Kolom Runtime pada
bagian 2 sengaja **tidak** diubah, karena bukti runtime per requirement belum diserahkan dan
belum tercatat di laporan task. Selisih ini tercatat sebagai `PAT-STALE-002` pada manifest.

---

## 5. `BE-PAT-MIG-002`

> **Status: `approved`** — 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`). Kolom Implementasi bernilai
> `BELUM` karena task belum dikerjakan dan baru boleh dimulai sesudah `BE-PAT-MIG-003` ✅ dan
> `PAT-GATE-005` `PASS`. Kolom Verifikasi berisi **rencana** bukti, bukan hasil. *Status lama:
> `draft`, diamandemen dua kali 15 Oktober 2026, menunggu `PAT-DEC-033`.*

### 5.1 Cara membaca kolom

Sama dengan bagian 1. Wewenangnya: Implementasi dan Verifikasi milik `BE-PAT-MIG-002` sesudah
disetujui; Runtime milik `PAT-GATE-002`, yang terpisah. Untuk backend, bukti verifikasi mengikuti
`TEST_POLICY`: build, QBE, review diff, penelusuran kontrak dan proses bisnis. Automated test hanya
dihitung bila `PAT-OQ-022` pilihan A disetujui — tidak adanya automated test **bukan** gap.

### 5.2 Requirement → task → bukti

| Requirement | Ringkasan | Keputusan | Kontrak `1.2.0` | Task BE | Task FE | Implementasi | Verifikasi — rencana | Runtime | Laporan |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PAT-MIG2-AC-01` | Hanya `Staging` persis, kedua mode | `PAT-DEC-019`, `028` | B7 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran pemakaian `RsmmcPilotEnvironmentGate` sebelum pembacaan apa pun | `NANTI` — respons simulasi pertama menyebut `Staging` | — |
| `PAT-MIG2-AC-02` | Batch kanonik dan baris batch | `PAT-DEC-004`, `028` | B2.1, B4.2 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran gerbang `APPROVED_BATCH`, `BATCH_ROW_EXISTS` | `NANTI` — gerbang pada simulasi | — |
| `PAT-MIG2-AC-03` | Preview 704.506 dan unik | `PAT-DEC-020` | B4.2 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran gerbang `PREVIEW_ROWS`, `PREVIEW_UNIQUE` | `NANTI` — gerbang pada simulasi | — |
| `PAT-MIG2-AC-04` | Perencana `FINALIZED` 705.223 | `PAT-DEC-021` | B4.2 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran gerbang `PLANNER_FINALIZED_ROWS` | `NANTI` — gerbang pada simulasi | — |
| `PAT-MIG2-AC-05` | Himpunan tertunda dan kode cadangan | `PAT-DEC-030` | B4.2 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran `DEFERRED_SET`, `DEFERRED_RESERVATIONS_UNUSED` | `NANTI` — gerbang pada simulasi dan audit | — |
| `PAT-MIG2-AC-06` | Baseline `MstPatient` | `PAT-DEC-034`, `PAT-DEC-042` (semula `PAT-OQ-020`, kini `SUPERSEDED`) | B4.2 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran `PATIENT_BASELINE` dengan contoh berangka | `NANTI` — 741 sebelum, 741 + jumlah manifest eksekusi sesudah | — |
| `PAT-MIG2-AC-07` | Tabrakan `PatientCode` | `PAT-DEC-023` | B3.2, B4.2, B4.3 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran gerbang dan pemeriksaan per pasien sesudah kunci | `NANTI` — gerbang pada simulasi | — |
| `PAT-MIG2-AC-08` | Tabrakan MRN akhir | `PAT-DEC-023` | B3.2, B4.2, B4.3 | `BE-PAT-MIG-002` | — | `BELUM` | Sama dengan `AC-07` untuk MRN | `NANTI` — gerbang pada simulasi | — |
| `PAT-MIG2-AC-09` | Manifest eksekusi `RSMMC-CANARY-A` v1: 25 kandidat dalam urutan yang sama, tanpa sort ulang; identifier, versi, dan sidik jari berurutan pada respons | `PAT-DEC-026`, `040`, `044`, `045`, `056` | B1, B2.2, B3.1, B4.2 | `BE-PAT-MIG-002` | — | `BELUM` | Isi dan urutan konstanta manifest dibandingkan dengan `00-interview-decisions.md` bagian 11; sidik jari berurutan dihitung ulang | `NANTI` — identifier, versi, dan sidik jari pada respons simulasi | — |
| `PAT-MIG2-AC-10` | Crosswalk atomik, ulang → `ALREADY_MIGRATED` | `PAT-DEC-022`, `023` | B4.3, B5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran transaksi: insert pasien dan crosswalk di transaksi yang sama | `NANTI` — `PAT-MIG2-AC-35` | — |
| `PAT-MIG2-AC-11` | Kunci dua bagian, konversi Int32 tervalidasi, `CONCURRENT_RUN` tanpa menunggu, baca ulang sesudah kunci | `PAT-DEC-023`, `PAT-DEC-048` | B4.3, B5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran langkah 1–6 dan pemeriksaan rentang key2; test konversi termasuk `legacy_pid` negatif | `TIDAK BERLAKU` — tidak diuji dengan dua proses di Staging | — |
| `PAT-MIG2-AC-12` | Kasus A | `PAT-DEC-022` | B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran jalur normal | `NANTI` — seluruh pasien manifest eksekusi dibuat | — |
| `PAT-MIG2-AC-13` | Kasus B pada Canary-A | `PAT-DEC-026`, `027` | B3.2, B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: tidak ada `UPDATE` crosswalk | `NANTI` — simulasi menunjukkan seluruh baris manifest eksekusi `MISSING` | — |
| `PAT-MIG2-AC-14` | Kasus C+D | `PAT-DEC-023` | B3.2, B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran klasifikasi | `NANTI` — eksekusi ulang | — |
| `PAT-MIG2-AC-15` | Kasus E | `PAT-DEC-022` | B3.2, B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: pasien tidak dibuat ulang | `TIDAK BERLAKU` — tidak diharapkan terjadi | — |
| `PAT-MIG2-AC-16` | Kasus F dan target ganda | `PAT-DEC-022` | B3.2, B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran klasifikasi | `TIDAK BERLAKU` — tidak diharapkan terjadi | — |
| `PAT-MIG2-AC-17` | Kasus G, `CreateNew`, path `rsmmc-bulk` | `PAT-DEC-024`, `PAT-DEC-052` | B4.5, B6 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran pemakaian `RsmmcPilotQrArtifactStore` apa adanya dengan layout `rsmmc-bulk` | `NANTI` — QR baru di `rsmmc-bulk` | — |
| `PAT-MIG2-AC-18` | Kasus I, bukti kepemilikan | `PAT-DEC-024` | B4.5, B6 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran pemanggilan `TryDeleteCreated` | `TIDAK BERLAKU` — jalur gagal | — |
| `PAT-MIG2-AC-19` | Kasus J, *commit* tidak pasti | `PAT-DEC-024` | B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran tahap `COMMIT_OUTCOME_UNCERTAIN` | `TIDAK BERLAKU` — jalur gagal | — |
| `PAT-MIG2-AC-20` | Kasus H, QR tidak pasti | `PAT-DEC-024` | B4.5 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: tidak ada *commit* sesudah galat QR | `TIDAK BERLAKU` — jalur gagal | — |
| `PAT-MIG2-AC-21` | Simulasi tanpa mutasi; boleh membaca keberadaan, metadata, dan hash berkas | `PAT-DEC-028`, `PAT-DEC-046` | B4.4 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: cabang simulasi kembali sebelum transaksi tulis; penentu lokasi tidak membuat folder | `NANTI` — jumlah data dan isi folder QR tidak berubah sesudah simulasi | — |
| `PAT-MIG2-AC-22` | Satu transaksi per pasien | `PAT-DEC-023` | B4.3 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran batas transaksi | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-23` | Log dengan aktor, tanpa data pribadi | `PAT-DEC-025`; `PAT-OQ-023` | B11 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran isi log | `NANTI` — log eksekusi berisi aktor | — |
| `PAT-MIG2-AC-24` | Tanpa pembangkit `PatientCode` normal | `PAT-DEC-020` | B10 | `BE-PAT-MIG-002` | — | `BELUM` | Pencarian pemanggilan pembangkit pada diff | `NANTI` — kode sama dengan preview pada audit | — |
| `PAT-MIG2-AC-25` | Tanpa pembangkit MRN normal | `PAT-DEC-020`, `021` | B10 | `BE-PAT-MIG-002` | — | `BELUM` | Sama dengan `AC-24` | `NANTI` — MRN sama dengan perencana pada audit | — |
| `PAT-MIG2-AC-26` | Tanpa inferensi NIK | `PAT-DEC-022` | B3.2 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: NIK hanya dipakai untuk pemeriksaan tabrakan | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-27` | Tanpa nama pengganti | `PAT-DEC-030` | B3.2, B10 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran `PREVIEW_VALUE_INVALID` | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-28` | Dua `legacy_pid` tertunda dikecualikan | `PAT-DEC-030` | B3.2 | `BE-PAT-MIG-002` | — | `BELUM` | Manifest dan penolakan defensif | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-29` | Pilot 715 tidak tersentuh | `PAT-DEC-019` | B4.2, B9 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: hanya `INSERT` pada `MstPatient` dan crosswalk | `NANTI` — audit `PAT-MIG2-AC-36` | — |
| `PAT-MIG2-AC-30` | QR Pilot lama tidak tersentuh | `PAT-DEC-024` | B6 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran jalur pembersihan | `NANTI` — audit QR | — |
| `PAT-MIG2-AC-31` | Produksi mustahil | `PAT-DEC-019` | B7 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: tidak ada pengaturan yang melonggarkan gerbang | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-32` | Tabel migrasi lain tidak diubah | `PAT-DEC-019`, `PAT-DEC-035` | B9 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: satu-satunya tulis migrasi adalah `INSERT` crosswalk | `NANTI` — audit | — |
| `PAT-MIG2-AC-33` | Nilai tersimpan = preview, termasuk `IsMember = false` dan `IsActive = true` | `PAT-DEC-041`; `PAT-OQ-029` (semula `PAT-OQ-017`, kini `SUPERSEDED`) | B4.3, B10 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran pembanding baca ulang | `NANTI` — audit nilai seluruh pasien canary | — |
| `PAT-MIG2-AC-34` | Tanpa EF migration dan perubahan model | `PAT-DEC-019` | B9 | `BE-PAT-MIG-002` | — | `BELUM` | Review diff `Migrations/`, `Models/`, `Repositories/` | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-35` | Eksekusi ulang tanpa duplikat | `PAT-DEC-023`, `029` | B5 | `BE-PAT-MIG-002` | — | `BELUM` | — (runtime) | `NANTI` — `PAT-GATE-002` langkah 10 | — |
| `PAT-MIG2-AC-36` | Audit sesudah Canary-A | `PAT-DEC-029` | B4.1, B9 | `BE-PAT-MIG-002` | — | `BELUM` | — (runtime) | `NANTI` — `PAT-GATE-002` langkah 9 | — |
| `PAT-MIG2-AC-37` | Gelombang besar terpisah | `PAT-DEC-029`; `PAT-OQ-012`, `026` | B1, B12 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: mode selain `CANARY_A` ditolak; tidak ada *hosted service* | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-38` | Kontrak respons | `PAT-DEC-028` | B2.2 | `BE-PAT-MIG-002` | — | `BELUM` | Verifikasi DTO terhadap B2.2 | `NANTI` — respons simulasi | — |
| `PAT-MIG2-AC-39` | Izin dan aktor | `PAT-DEC-028` | B8 | `BE-PAT-MIG-002` | — | `BELUM` | Atribut terpasang; penolakan aktor tidak dikenal | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-40` | Validasi request | `PAT-DEC-028`; `PAT-OQ-012` | B2.1 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran validasi dengan contoh batas 0, 1, 25, 26 | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-41` | Build, QBE, dan automated test **wajib** | `PAT-DEC-037` | — | `BE-PAT-MIG-002` | — | `BELUM` | Keluaran perintah apa adanya, termasuk `dotnet test` dan topik urutan manifest serta konversi kunci | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-42` | `BE-PAT-MIG-001` tidak berubah | `PAT-DEC-028` | B2 | `BE-PAT-MIG-002` | — | `BELUM` | Review diff: berkas `RsmmcPilot*` tidak tersentuh | `TIDAK BERLAKU` | — |
| `PAT-MIG2-AC-43` | 715 QR historis Pilot tidak disentuh; pasien bertabrakan tidak terblokir | `PAT-DEC-039`, `PAT-DEC-043`; `PAT-OQ-028` | B6, B3.2 | `BE-PAT-MIG-002` | — | `BELUM` | Test path strategi; penelusuran bahwa path bawaan tidak pernah ditulis | `NANTI` — SHA-256 QR historis sebelum dan sesudah sama | — |
| `PAT-MIG2-AC-44` | *Dipindah* — guard reservasi `PatientCode` | `PAT-DEC-057` | B14 | **`BE-PAT-MIG-003`** — lihat `PAT-MIG3-AC-03`–`05` | — | `DIPINDAH` | — | — | — |
| `PAT-MIG2-AC-45` | *Dipindah* — guard reservasi MRN | `PAT-DEC-057` | B14 | **`BE-PAT-MIG-003`** — lihat `PAT-MIG3-AC-06` | — | `DIPINDAH` | — | — | — |
| `PAT-MIG2-AC-46` | Gerbang `NORMAL_ALLOCATION_GUARD` menuntut pemeriksa `BE-PAT-MIG-003` melaporkan `overallGuardReady = true`; pembekuan saja tidak cukup. *Diamandemen `1.3.0` (`PAT-DEC-072`): `overallGuardReady` kini mensyaratkan pendaftaran normal **dan** bayi terlindungi* | `PAT-DEC-042`, `055`, `064`, `066`, `067` | B4.2, B14.2 | `BE-PAT-MIG-002` | — | `BELUM` | Test gerbang: pemeriksa tidak tersedia, `overallGuardReady = false`, atau nomor di bawah lantai → eksekusi ditolak | `NANTI` — gerbang pada simulasi sesudah `PAT-GATE-005` | — |
| `PAT-MIG2-AC-47` | Persistensi fisik `IsMember = false` dengan bukti provider-real. *Diamandemen `1.3.0` (`PAT-DEC-075`): INSERT menyimpan `false` dan dibaca ulang di transaksi yang sama; tanpa UPDATE `PERSIST_IS_MEMBER`* | `PAT-DEC-037`, `041`, `053`, `065` | B4.3, B10, B15 | `BE-PAT-MIG-002` | — | `BELUM` | Test persistensi terfokus wajib **dan** test PostgreSQL 15 sekali pakai atas tujuh hal `PAT-DEC-065` | `NANTI` — audit fisik PostgreSQL | — |
| `PAT-MIG2-AC-48` | Kandidat tidak dieksekusi sebelum syarat `PAT-DEC-056`; sesudahnya menjadi manifest tanpa penggantian | `PAT-DEC-040`, `PAT-DEC-056` | B1, B3.1 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran: konstanta manifest berasal dari bagian 11 dalam urutan yang sama | `NANTI` — sidik jari manifest eksekusi pada respons | — |
| `PAT-MIG2-AC-49` | Kolom crosswalk baru tepat sesuai `PAT-DEC-047`; `normalized_nik`/`normalized_mrn` tidak pernah dibaca untuk mencocokkan | `PAT-DEC-047`, `PAT-DEC-051` | B4.3, B9 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran `INSERT` crosswalk berparameter; pencarian pembacaan `normalized_*` pada diff | `NANTI` — audit kolom crosswalk pasien canary | — |
| `PAT-MIG2-AC-50` | Telepon `SAFE_TO_STORE` yang bergeser → `PHONE_ALREADY_USED`; nilai preview tidak dibuang | `PAT-DEC-050` | B3.2 | `BE-PAT-MIG-002` | — | `BELUM` | Test konflik telepon | `TIDAK BERLAKU` — jalur konflik | — |
| `PAT-MIG2-AC-51` | SQL bertipe tanpa `::text`; gerbang `PREVIEW_SCHEMA_SIGNATURE`; NULL/nilai enum tidak dikenal → `PREVIEW_VALUE_INVALID` | `PAT-DEC-063`; `PAT-GAP-009`, `010` | B4.2, B9.1, B10 | `BE-PAT-MIG-002` | — | `BELUM` | Penelusuran SQL pada diff tanpa `::text`; test gerbang tanda tangan skema dan pemetaan NULL/nilai tidak dikenal | `NANTI` — hasil gerbang dan klasifikasi pada simulasi Canary-A | — |

Jumlah per sesi ketiga 15 Oktober 2026: **51 ID requirement**. 49 aktif untuk `BE-PAT-MIG-002`;
`AC-44` dan `AC-45` dipindah ke `BE-PAT-MIG-003` (bagian 6) dan ID-nya dipertahankan sebagai jejak.
Tidak ada yang dipetakan ke task frontend (`PAT-DEC-060`). Dari 49 yang aktif, 31 punya bukti tahap
runtime pada `PAT-GATE-002` dan 18 cukup dibuktikan pada tahap verifikasi. *Amandemen kedua: 50 ID,
48 aktif, 30 runtime, 18 verifikasi.*

### 5.3 Keputusan → requirement

| Keputusan | Requirement yang menjawabnya |
| --- | --- |
| `PAT-DEC-017` Task baru | Seluruh baris bagian 5 |
| `PAT-DEC-018` Approval terpisah | `PAT-DEC-033` — `approved` 15 Oktober 2026 |
| `PAT-DEC-019` Batas keselamatan | `AC-01`, `29`, `31`, `32`, `34` |
| `PAT-DEC-020` Preview sebagai masukan | `AC-03`, `24`, `25` |
| `PAT-DEC-021` MRN otoritatif | `AC-04`, `25` |
| `PAT-DEC-022` Identitas `legacy_pid` | `AC-10`, `12`, `15`, `16`, `26` |
| `PAT-DEC-023` Konkurensi | `AC-07`, `08`, `10`, `11`, `14`, `22`, `35` |
| `PAT-DEC-024` Keselamatan QR | `AC-17`–`20`, `30` |
| `PAT-DEC-025` Pembuatan dari preview | `AC-23`, `33` |
| `PAT-DEC-026` Canary-A | `AC-09`, `13` |
| `PAT-DEC-027` Canary-B terpisah | `AC-13`; `PAT-GATE-003` |
| `PAT-DEC-028` Endpoint baru | `AC-01`, `02`, `21`, `38`–`40`, `42` |
| `PAT-DEC-029` Gerbang runtime | `AC-35`–`37`; `PAT-GATE-002` |
| `PAT-DEC-030` Tertunda dan tanpa nama pengganti | `AC-05`, `27`, `28` |
| `PAT-DEC-031` Bukti historis 285 baris | `AC-13`, `32` |
| `PAT-DEC-032` Provenance bukan `MIGRATED_API_PILOT` | `PAT-OQ-008`; `AC-06` (rumus baseline memakai nilai provenance) |
| `PAT-DEC-033` Persetujuan desain | Prasyarat seluruh baris |
| `PAT-DEC-034` Provenance `MIGRATED_BULK_WORKER` | `AC-06`, `36` |
| `PAT-DEC-035` Preview immutable | `AC-32` |
| `PAT-DEC-036` Eksekusi berhenti pada konflik/gagal tak terduga | `AC-14`, `19`, `22` |
| `PAT-DEC-037` Test wajib | `AC-41`, `47` |
| `PAT-DEC-038` `legacy_pid` Int64 | `AC-09`, `41` |
| `PAT-DEC-039` Pilot dan QR lama immutable | `AC-29`, `30`, `43` |
| `PAT-DEC-040` Daftar 25 hanya kandidat | `AC-09`, `48` |
| `PAT-DEC-041` `IsMember = false` wajib tersimpan | `AC-33`, `47` |
| `PAT-DEC-042` Perlindungan reservasi tahan lama | `AC-06`, `44`–`46` |
| `PAT-DEC-043` Tabrakan QR adalah P0 | `AC-43` |
| `PAT-DEC-044` Route dan request; identifier/versi/sidik jari manifest | `AC-09`, `38`, `40` |
| `PAT-DEC-045` `CANARY_A`, `limit` 1–25, urutan manifest | `AC-09`, `37`, `40`, `41` |
| `PAT-DEC-046` Simulasi membaca filesystem tanpa menulis | `AC-21`, `43` |
| `PAT-DEC-047` Kolom crosswalk dari preview | `AC-49` |
| `PAT-DEC-048` Kunci dua bagian | `AC-11` |
| `PAT-DEC-049` Data pribadi pada respons dan log | `AC-23`, `38` |
| `PAT-DEC-050` `PHONE_ALREADY_USED` | `AC-50` |
| `PAT-DEC-051` `TARGET_CREATED` | `AC-49` |
| `PAT-DEC-052` Path QR `rsmmc-bulk` | `AC-17`, `43` |
| `PAT-DEC-053` Persistensi `IsMember` dan bukti provider-real | `AC-33`, `47` |
| `PAT-DEC-054` Lantai `PatientCode` 705.250 | `PAT-MIG3-AC-03`–`05` |
| `PAT-DEC-055` Pembekuan Staging; gerbang menuntut `BE-PAT-MIG-003` | `AC-46`; `PAT-GATE-002` |
| `PAT-DEC-056` Manifest eksekusi = 25 kandidat dalam urutan yang sama | `AC-09`, `48` |
| `PAT-DEC-057` Task prasyarat `BE-PAT-MIG-003` | `PAT-MIG3-AC-01`–`12`; `AC-44`/`45` dipindah |
| `PAT-DEC-058` Transisi Canary-B | `PAT-GATE-003` — belum ada requirement build |
| `PAT-DEC-059` 22 baris dikecualikan | `PAT-GATE-003` — belum ada requirement build |
| `PAT-DEC-060` Tanpa frontend | `frontend-roadmap.md` |
| `PAT-DEC-061` Gelombang besar ditunda | `AC-37`; `PAT-GATE-004` |
| `PAT-DEC-062` Inventaris filesystem wajib | `PAT-GATE-002`, `PAT-GATE-004` |
| `PAT-DEC-063` Skema fisik; SQL bertipe tanpa `::text` | `AC-03`–`08`, `11`, `49`, `51` |
| `PAT-DEC-064` Lantai MRN 890.188 | `AC-46`; `PAT-MIG3-AC-06`, `12`, `13` |
| `PAT-DEC-065` PostgreSQL 15 sekali pakai | `AC-47`; `PAT-MIG3-AC-13` |
| `PAT-DEC-066` Guard hanya `Staging` persis dan `Enabled == true` | `AC-46`; `PAT-MIG3-AC-08`, `12`, `14` |
| `PAT-DEC-067` Amandemen: `Enabled = false` di Staging gagal tertutup; endpoint pemeriksa | `AC-46`; `PAT-MIG3-AC-07`, `08`, `12`, `14` |
| `PAT-DEC-068` Kunci allocator dua bagian tetap; konfirmasi endpoint pemeriksa | `PAT-MIG3-AC-05`, `07`, `13` |

`AC-xx` pada tabel ini berarti `PAT-MIG2-AC-xx`. Butir `PAT-OQ-008`–`026` mempertajam requirement
di atas dan **tidak** menambah requirement baru; bila pemilik memilih opsi yang berbeda dari
rekomendasi, requirement yang terdampak ditulis ulang pada revisi berikutnya.

### 5.4 Gap

| Gap | Keadaan |
| --- | --- |
| Requirement tanpa rencana bukti verifikasi | **Tidak ada** — 49 dari 49 punya rencana bukti |
| Requirement yang bergantung pada keputusan yang masih `BLOCKING` | **Tidak ada** per sesi ketiga 15 Oktober 2026. *Riwayat amandemen kedua: `PAT-OQ-019` → `AC-03`–`08`, `09`, `11`, `49`; `PAT-OQ-031` → `AC-46`, `PAT-MIG3-AC-06`; `PAT-OQ-036` → `AC-47`; `PAT-OQ-037` → `AC-46`, `PAT-MIG3-AC-08` — seluruhnya kini diputuskan (`PAT-DEC-063`–`066`)* |
| Data yang dibutuhkan build dari pemilik | Paket bukti skema dan MRN **diterima** (`PAT-FACT-079`–`089`). Yang tersisa untuk build: server PostgreSQL 15 sekali pakai beserta connection string-nya (`PAT-GAP-014`, `PAT-RSK-038`) |
| Bukti runtime | Belum dapat dikumpulkan; milik `PAT-GATE-002` |

---

## 6. `BE-PAT-MIG-003` ✅

> **Diperbarui 19 Oktober 2026:** ✅ — source di-*merge* pemilik (PR #290, `ab45dd3c`, merge `3c457ab4`), dideploy ke DEV dan STAGING, dan `PAT-GATE-005` generasi 1 `PASS` (`PAT-FACT-090`), sehingga `AC-12` terpenuhi. Generasi 1 hanya membuktikan `CreatePatient`; jalur bayi menjadi `BE-PAT-MIG-003A` (bagian 7).
> **Diperbarui 17 Oktober 2026 oleh `build-module-backend`, di atas Integration `0c6e3145`:** 14 dari 14 requirement tahap build terpenuhi; `dotnet test` `70` test baru dan `73` regresi `BE-PAT-MIG-001` lulus, build `0` error, QBE `Strict` `PASS`, `has-pending-model-changes` tanpa perubahan, dan **uji PostgreSQL 15.15 provider-real `10`/`10` PASS** (dijalankan pemilik). `AC-12` milik runtime `PAT-GATE-005`. [laporan](../task/report/backend/BE-PAT-MIG-003.md).
> **Status: `approved`** — 15 Oktober 2026, sesi ketiga (`PAT-DEC-033`). Task prasyarat
> (`PAT-DEC-057`); boleh dimulai. Kolom Implementasi `BELUM`; kolom Verifikasi berisi rencana. Kolom
> Runtime milik `PAT-GATE-005`. *Status lama: `draft`, menunggu `PAT-DEC-033`, `PAT-OQ-031`, dan
> `PAT-OQ-037`.*

| Requirement | Ringkasan | Keputusan | Kontrak `1.2.0` | Task BE | Task FE | Implementasi | Verifikasi — rencana | Runtime | Laporan |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PAT-MIG3-AC-01` | Allocator di module service; controller tidak mengalokasikan | `PAT-DEC-054` | B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — pembangkit dihapus dari controller; `Controller_TidakLagiMemilikiPembangkitNomorLama`; QBE `Strict` `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-02` | Ketiga rute memakai allocator yang sama | `PAT-DEC-054` | B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — `KetigaRutePendaftaran_BermuaraPadaDuaActionYangDiketahui`; uji rute biasa/kiosk dan admin | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-03` | Suffix pertama yang belum dipakai ≥ 705.250 | `PAT-DEC-054` | B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — uji lantai dan celah (705.250, 705.252) | `TIDAK BERLAKU` — dibuktikan `AC-12` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-04` | Tidak pernah 742–705.249, termasuk dua kode tertunda | `PAT-DEC-054` | B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — uji dengan `PAT-RSMMC-00742`, `101416`, `705249` di database | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-05` | Serialisasi database; indeks unik pengaman terakhir | `PAT-DEC-042`, `054` | B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` di source — `pg_advisory_xact_lock(1346458697, 1313821261)` di dalam transaksi (`PAT-DEC-068`); bukti bersamaan PostgreSQL `PASS` (`AC-13`, 17 Oktober 2026) | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-06` | Lantai MRN: pertama yang belum dipakai ≥ 890.188; tidak pernah MRN akhir `FINALIZED` | `PAT-DEC-042`, `064` | B14, B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — uji lantai dan celah MRN; allocator tidak membaca `migration.*` | `TIDAK BERLAKU` — dibuktikan `AC-12` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-07` | Pemeriksa baca-saja tanpa alokasi, kunci, dan tulis; keluaran `1.2.1` B14.2; endpoint `GET /admin/migration/rsmmc-reservation-guard` khusus Staging dengan `Patient : Update` | `PAT-DEC-055`, `057`, `067`; `PAT-GAP-008` | B14.2 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — `Pemeriksa_*`; `Pemeriksa_RuteDanIzinSesuaiKontrak` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-08` | Staging persis wajib allocator aman reservasi dengan `Enabled == true`; di luar itu algoritma lama tanpa perubahan hasil | `PAT-DEC-066`, `067`; `PAT-GAP-005` | B14.1, B14.3 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — matriks penilaian; pembanding pembangkit lama `a52b3c98` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-09` | Bentuk endpoint, perilaku `IsMember`, dan QR pendaftaran tidak berubah | `PAT-DEC-053`, `057` | B14.1 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — review diff; QR ke folder MRN baru | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-10` | Test terfokus wajib, termasuk tabel environment/konfigurasi B14.3 | `PAT-DEC-057` | B14.3 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — 70 test baru lulus | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-11` | Build dan QBE `Strict` | — | — | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — build `0` error; QBE `Strict` `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-12` | Verifikasi runtime di Staging: `overallGuardReady = true`, kode ≥ `PAT-RSMMC-705250`, MRN ≥ `00-89-01-88` | `PAT-DEC-055`, `057`, `064`, `066`, `067` | B14.2 | `BE-PAT-MIG-003` | — | `TERPENUHI` | — (runtime) | `PASS` — `PAT-GATE-005` generasi 1, dilaporkan pemilik 19 Oktober 2026: `ACTIVE`, `PAT-RSMMC-705250`, `00-89-01-88`, `overallGuardReady = true` (`PAT-FACT-090`) | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-13` | Uji PostgreSQL provider-real allocator pada PostgreSQL 15 sekali pakai | `PAT-DEC-065`; `PAT-GAP-006` | B15 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — 7 method / 10 kasus lulus pada PostgreSQL 15.15, dijalankan pemilik 17 Oktober 2026; 0 basis data `pat_proof_*` tersisa | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |
| `PAT-MIG3-AC-14` | Gagal tertutup di `Staging` persis bila `Enabled` `false`/hilang/salah bentuk/tidak terbaca → `503` sebelum mutasi; lantai tidak dapat diubah lewat konfigurasi | `PAT-DEC-066`, `067`; `PAT-GAP-002`, `003` | B14.3 | `BE-PAT-MIG-003` | — | `TERPENUHI` | `PASS` — `StagingTidakSah_Ditolak503_SebelumInsertQrDanFoto` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003.md) |

Jumlah per sesi ketiga: 14 requirement; 1 dengan bukti runtime pada `PAT-GATE-005`, 13 dibuktikan
pada tahap verifikasi. Tidak ada yang menunggu keputusan. *Amandemen kedua: 12 requirement;
`PAT-MIG3-AC-06` dan `08` menunggu keputusan `BLOCKING`.*

---

## 7. `BE-PAT-MIG-003A` 🟡

> **Diperbarui 19 Oktober 2026 — koreksi review final `F-07`/`F-13`/`F-14`:** 11 requirement tahap build
> terbukti; `AC-12` milik runtime `PAT-GATE-005` generasi 2. uji PostgreSQL 15 provider-real `23`/`23` PASS — smoke `1`/`1`, 0 gagal, 0 dilewati, 4259,7 detik, 0 basis data `pat_proof_*` tersisa, kontainer dan image `postgres:15` sekali pakai dihapus (dijalankan pemilik 19 Oktober 2026 pada commit `6ae9c251`; server bukti `max_locks_per_transaction = 2048`, hanya server uji). Test SQLite sesudah koreksi
> `206` lulus / `0` gagal / `16` dilewati (17 kasus koreksi `F-07`); build Release `0` error; QBE `Strict` `PASS`. 9 kasus provider-real yang memanggil
> `AllocateReservedAsync` dijalankan ulang pemilik sesudah koreksi `F-07`. *Teks awal: ditulis di atas
> `c6537ee4`; `AC-09` menunggu bukti provider-real pemilik.*

| Requirement | Ringkasan | Keputusan | Kontrak `1.3.0` | Task BE | Task FE | Implementasi | Verifikasi | Runtime | Laporan |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PAT-MIG3A-AC-01` | Staging persis + `Enabled = true`: nomor bayi ≥ lantai lewat allocator bersama; `NumberSeries` tidak disentuh | `PAT-DEC-070`, `071` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite) — `T6_StagingAktif_*`; provider-real `T6_*` `PASS` | `NANTI` — `AC-12` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-02` | Staging persis tanpa guard sah → `503` sebelum akses basis data; tidak jatuh ke `NumberSeries` | `PAT-DEC-070` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite) — `T3_T4_T5_*` (0 perintah SQL), `T5_*TidakTerbaca`, `Rute_StagingTidakSah_Ditolak503*` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-03` | Di luar Staging persis `NumberSeries` bayi tidak berubah; konfigurasi guard tidak dibaca; tanpa kunci `(PATI, NORM)` | `PAT-DEC-070` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite, cabang) — `T1_T2_*`, `T2_*TidakDibaca`; ujung-ke-ujung `T1_T2_*` provider-real `PASS`; diff: method `NumberSeries` tidak berubah; `F-07`: guard buatan di luar Staging persis ditolak sebelum SQL (`AlokasiBerlantaiGuardBuatanTests`) | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-04` | Kunci `(1346458697, 1313821261)` di transaksi bayi sesudah kunci idempotency/kasus; tanpa siklus *deadlock* | `PAT-DEC-071` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | Penelusuran urutan kunci (laporan bagian F); `T10_*` provider-real `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-05` | Normal↔normal, normal↔bayi, bayi↔bayi tanpa nomor ganda dan tidak di bawah lantai | `PAT-DEC-071` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite, berurutan) — `T7_*`; bersamaan `T7_T9_*`, `T8_*` provider-real `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-06` | Semantik idempotency bayi tidak berubah | `PAT-DEC-070` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite) — `T15_*`; bersamaan `T15_*` provider-real `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-07` | Semantik urutan kelahiran per kasus tidak berubah | `PAT-DEC-070` | B14.4 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite) — `T16_*`; kembar bersamaan `T16_*` provider-real `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-08` | Pemeriksa melaporkan kedua jalur; `overallGuardReady` mensyaratkan keduanya; tetap baca-saja | `PAT-DEC-072` | B14.2 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` (SQLite) — `T11_T12_T14_*`, `T13_*`, `Pemeriksa_BukanStaging_*`; `Pemeriksa_RuteDanIzinSesuaiKontrak` (lama) tetap lulus | `NANTI` — `AC-12` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-09` | Bukti provider-real PostgreSQL 15 sekali pakai | `PAT-DEC-065`, `070`, `071` | B15 | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` — `23`/`23` (13 kasus task ini + 10 `BE-PAT-MIG-003`) pada PostgreSQL 15 sekali pakai, dijalankan pemilik 19 Oktober 2026 pada `6ae9c251`; 0 `pat_proof_*` tersisa. Uji ulang 9 kasus terdampak `F-07` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-10` | Tidak ada regresi `BE-PAT-MIG-003`/`001`; build Release; QBE `Strict` | — | — | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` — 70 + 73 regresi lulus; Release `0` error; QBE `Strict` `PASS` | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-11` | Tidak ada EF migration baru | `PAT-DEC-074` | — | `BE-PAT-MIG-003A` | — | `TERPENUHI` | `PASS` — `has-pending-model-changes`: tanpa perubahan | `TIDAK BERLAKU` | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |
| `PAT-MIG3A-AC-12` | Runtime `PAT-GATE-005` generasi 2 di Staging: kedua jalur terlindungi, `overallGuardReady = true` | `PAT-DEC-069`, `072` | B14.2 | `BE-PAT-MIG-003A` | — | `BELUM` | — (runtime) | `NANTI` — `PAT-GATE-005` generasi 2 | [laporan](../task/report/backend/BE-PAT-MIG-003A.md) |

Jumlah: 12 requirement; 1 menunggu runtime, 11 terbukti pada tahap verifikasi. *Sebelum 19 Oktober 2026
malam: 1 menunggu bukti provider-real pemilik.* Tidak ada yang menunggu keputusan.
