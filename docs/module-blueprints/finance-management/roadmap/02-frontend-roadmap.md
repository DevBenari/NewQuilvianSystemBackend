# Finance Management — Roadmap Frontend

## 1. Identitas

```yaml
roadmap_id: FIN-ROADMAP-FE-001
parent_roadmap: FIN-ROADMAP-001 revisi 3
roadmap_revision: 2
roadmap_status: ACTIVE — UI brief closed 2026-09-23, menunggu verifikasi dependency backend per task (lihat tabel task bagian 4)
blueprint_id: FIN-BP-001
blueprint_revision: 3
frontend_commit_sha: abed49b03
frontend_branch: (branch aktif QuilvianSystemFrontendDev saat audit)
frontend_authority: Product Owner (Yasmin) — UI brief closed 2026-09-23, FIN-DEC-024..029 di 00-interview-decisions.md
contracts:
  FIN-API-1.0: locked 2026-09-20 (tidak bergerak pada revisi 3)
  FIN-PERM-1.0: locked 2026-09-20 (tidak bergerak pada revisi 3)
  FIN-STATE-1.1: locked 2026-09-25 (dirujuk FE-FIN-007 untuk daftar status kejadian)
  FIN-MVP-1.1: locked 2026-09-25
roadmap_revision_2_note: >
  Revisi 2 (25 September 2026) MENAMBAHKAN satu task FE-FIN-007 untuk AMENDMENT REVISI 3
  blueprint, dan MENAMBAHKAN bagian Grafik Urutan Dependency yang sebelumnya belum ada pada
  berkas ini. TIDAK ada task lama yang dinomori ulang, diubah outcome-nya, atau diturunkan
  statusnya. Kriteria FE-FIN-006 yang menyebut HELD_FOR_FINALIZATION diberi catatan, bukan
  diubah: task itu sudah selesai dan buktinya tetap berlaku untuk keadaan saat dikerjakan.
```

Payung roadmap ada di `00-delivery-roadmap.md`. Pasangan backend-nya ada di
`01-backend-roadmap.md`.

**Berkas ini tidak menyatakan satu pun pekerjaan selesai, dan tidak memutuskan satu pun
rancangan antarmuka.**

## 2. Dua hal berbeda yang menahan frontend

Penting dipisahkan, karena obatnya berbeda:

| Penahan | Menahan apa | Keadaan |
|---|---|---|
| Kontrak API belum terkunci | Kerja paralel dengan backend | **Sudah tercabut** — `FIN-API-1.0` terkunci 20 September 2026 |
| ~~UI brief belum ada~~ | Seluruh task `FE-FIN-*` | **Sudah tercabut 23 September 2026** — keenam keputusan Product Owner pada bagian 5 tercatat `FIN-DEC-024`..`029` (approved, Yasmin) di `docs/module-blueprints/finance-management/00-interview-decisions.md` bagian `Frontend Decision Authority` |
| Endpoint belum dibangun | Pengujian nyata tiap layar | Menyusul per gelombang backend — lihat dependency per task pada tabel bagian 4. **Pembaruan 23 September 2026**: `BE-FIN-004` kini ✅ selesai (build PASS, migration diterapkan, endpoint diuji langsung — [laporan](../task/report/backend/BE-FIN-004.md)), jadi `FE-FIN-001` tidak lagi tertahan di titik ini |

Dengan UI brief tertutup, penahan yang tersisa untuk tiap task `FE-FIN-*` murni dependency
backend-nya masing-masing pada tabel bagian 4 — bukan lagi keputusan Product Owner.

`03-frontend-architecture.md` menetapkan **kontrak fungsional** — kemampuan apa yang MUST ada,
data apa yang MUST terlihat, aksi apa yang MUST disembunyikan. Ia **bukan** rancangan
antarmuka.

## 3. Keadaan frontend sekarang

Diverifikasi langsung pada `abed49b03`:

| Yang sudah ada | Keadaan |
|---|---|
| Menu sidebar "Keuangan" (`corporateFinance`) | Ada, berisi **dua** butir: Kategori Petty Cash dan Anggaran Petty Cash |
| `/finance/petty-cash-budget` | Halaman nyata, 687 baris, berfungsi penuh |
| `/finance/master-data/petty-cash-category` | Halaman nyata beserta create, detail, update |
| Redux slice dan hook Petty Cash | Masih di path `health-services/billing-management/` |
| Halaman voucher di bawah `/finance/` | Belum ada |
| Layar AR, AP, pembayaran, setoran, kas harian | **Belum ada satu pun** |

Jadi pekerjaan frontend modul ini adalah **membangun dari nol** untuk AR, AP, kas, dan
pemantauan — ditambah **merapikan** yang sudah ada untuk Petty Cash.

## Grafik Urutan Dependency

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

```text
BE-FIN-004 ✅ [BE] ─> FE-FIN-001 🟡

BE-FIN-015 ✅ [BE] ─> FE-FIN-003 ✅

BE-FIN-018 ✅ [BE] ─> FE-FIN-004

FE-FIN-005 ✅

BE-FIN-009 ✅ [BE] ─┬─> FE-FIN-002 ✅
                    │
BE-FIN-012 ✅ [BE] ─┴─> FE-FIN-006 ✅ ─┐
                                       │
BE-FIN-024 ⛔ [BE] ────────────────────┴─> FE-FIN-007 ⛔
```

`[BE]` = task backend pada `01-backend-roadmap.md`, **cermin baca-saja**. Tandanya disalin dari
roadmap pemiliknya, tidak pernah ditetapkan di berkas ini. `FE-FIN-005` tidak punya prasyarat
backend sama sekali — ia merapikan halaman Petty Cash yang sudah berfungsi.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-FIN-004` ✅ | `FE-FIN-001` 🟡 — sudah dikerjakan, verifikasi runtime belum dibuktikan |
| 1 | — | `FE-FIN-005` ✅ |
| 2 | `BE-FIN-009` ✅ | `FE-FIN-002` ✅ |
| 2 | `BE-FIN-015` ✅ | `FE-FIN-003` ✅ |
| 2 | `BE-FIN-009` ✅ dan `BE-FIN-012` ✅ | `FE-FIN-006` ✅ |
| 3 | `BE-FIN-018` ✅ | `FE-FIN-004` — belum dikerjakan; seluruh prasyaratnya sudah ✅ |
| — | ⛔ menunggu `BE-FIN-024` (yang sendirinya menunggu `FIN-OQ-017`) | `FE-FIN-007` |

`FE-FIN-007` **tidak diberi nomor gelombang** karena `EPIC FIN-14` berstatus `OPEN DECISION` pada
`04-prd-to-mvp.md`, sama seperti rangkaian `REV-3` pada roadmap backend.

## 4. Task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `FE-FIN-001` | Pengelolaan data induk Finance | `FR-FIN-001`..`004` | `FIN-API-1.0`, `FIN-PERM-1.0` | Pola halaman Petty Cash (`FIN-CAP-015`), group menu `corporateFinance` | Bank, rekening, mata uang, kurs — CRUD beserta aktif/nonaktif | `BE-FIN-004` ✅ selesai 23 September 2026 (UI brief closed — `FIN-DEC-024`..`029`) | Nomor rekening ganda ditolak dengan pesan yang dapat dibaca petugas | `UAT-01`, `UAT-02` — **belum dibuktikan runtime**, `npm run lint:errors`/`npm run build` `NOT RUN` ([laporan](../task/report/frontend/FE-FIN-001.md)) | Product Owner — tata letak `DEV_DISCRETION` | Aksi mengikuti `FIN-PERM-1.0`; nol perhitungan di klien |
| ✅ `FE-FIN-002` | Buku piutang dan umur piutang | `FR-FIN-020`..`024` | `FIN-API-1.0` | — | Daftar, rincian, umur empat kelompok, penelusuran ke tagihan asal | `BE-FIN-009` ✅ selesai 23 September 2026 (UI brief closed — `FIN-DEC-024`..`029`) | Piutang dapat ditelusuri ke tagihan asalnya dari layar | `UAT-03` — ✅ **Selesai 23 September 2026**; `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-002.md)) | Product Owner | Nilai uang **tidak** dibulatkan ulang di klien; kelompok umur tidak diubah layar |
| ✅ `FE-FIN-003` | Setoran bank dan kas harian | `FR-FIN-060`..`065` | `FIN-API-1.0` | — | Setoran, penutupan hari, saldo | `BE-FIN-015` ✅ selesai 23 September 2026 (UI brief closed — `FIN-DEC-024`..`029`) | Angka yang sudah ditutup ditampilkan beku; setoran melebihi kas ditolak dengan pesan yang jelas | `UAT-13`..`UAT-16` — ✅ **Selesai 23 September 2026**; `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-003.md)) | Product Owner | Saldo dibaca dari backend, tidak dihitung ulang |
| ✅ `FE-FIN-006` | Pemantauan fakta Billing dan kejadian Accounting | `FR-FIN-011`, `FR-FIN-012`, `FR-FIN-074` | `FIN-API-1.0` | Pola tab Finance & base components | Daftar gagal olah beserta pengulangan; antrean kejadian beserta status tertahan | `BE-FIN-009`, `BE-FIN-012` ✅ selesai 23 September 2026 (UI brief closed — `FIN-DEC-024`..`029`) | `HELD_FOR_FINALIZATION`, `HELD`, dan `FAILED` **dibedakan** — tindakan penggunanya berbeda | `UAT-04`, `UAT-17`..`UAT-19` — ✅ **Selesai 23 September 2026**; `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-006.md)) | Product Owner | Tanpa tombol kirim — pengiriman adalah `EPIC FIN-12` |
| `FE-FIN-004` | Penerimaan dan alokasi | `FR-FIN-030`..`046` | `FIN-API-1.0` — permukaan collection dikecualikan dari penguncian | — | Penerimaan, alokasi manual, koreksi, penghapusan, rekonsiliasi shift | `BE-FIN-016`..`018` ✅ selesai 23 September 2026 (UI brief closed — `FIN-DEC-024`..`029`) | Alokasi dipilih petugas, bukan dicocokkan otomatis | `UAT-05`..`UAT-12`, `UAT-20` | Owner Billing sudah menjawab 21 September 2026 (`BKC-DEC-106`/`108`/`109`) — tidak lagi `BLOCKED` | — |
| ✅ `FE-FIN-005` | Merapikan Petty Cash ke rute Finance | `EPIC FIN-13` | `FIN-API-1.0` | `FIN-CAP-015`, `FIN-CAP-016` — **halaman sudah berfungsi penuh** | Pindahkan hook dan Redux slice; tambah halaman voucher di `/finance/`; satu butir menu | Kapan saja | Kemampuan pengguna **tidak berkurang sedikit pun** | Uji regresi halaman Petty Cash — ✅ **Selesai 23 September 2026**; `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error) ([laporan](../task/report/frontend/FE-FIN-005.md)) | Product Owner | `POST-MVP`. Aturan bisnis Petty Cash MUST NOT diubah — milik `billing-kasir` (`FIN-DEC-009`) |
| `FE-FIN-007` ⛔ | Layar pemantauan kejadian menampilkan tujuh jenis kejadian baru, dan memperlakukan `HELD_FOR_FINALIZATION` sebagai peninggalan kebijakan lama | `FIN-DEC-030`, `039`; `FR-FIN-074` **dicabut**, `FR-FIN-076`..`080` **baru**; `03-frontend-architecture.md` bagian 3 | `FIN-STATE-1.1`, `FIN-API-1.0` (endpoint `GET /accounting-events` **tidak berubah** — hanya isi datanya yang bertambah jenis) | Layar pemantauan yang sudah dibangun `FE-FIN-006` ✅ | (a) Tujuh jenis kejadian baru dapat disaring dan terbaca namanya; (b) `HELD_FOR_FINALIZATION` diberi keterangan bahwa ia peninggalan kebijakan lama yang perlu dibetulkan, **bukan** keadaan normal yang menunggu Billing; (c) baris berstatus `ACKNOWLEDGED` dengan nomor jurnal kosong **tidak** ditampilkan sebagai kegagalan | ⛔ `BE-FIN-024` — lihat `01-backend-roadmap.md` | Petugas dapat membedakan `HELD` (menunggu Accounting) dari `FAILED` (menunggu Finance) dari baris warisan `HELD_FOR_FINALIZATION` (menunggu pembetulan data); nomor jurnal kosong pada baris `ACKNOWLEDGED` tidak memicu tombol kirim ulang | `npm run lint:errors`, `npm run build`, dan verifikasi manual ketiga keadaan di layar. Test otomatis **opsional** sesuai `rules/frontend/test-policy.md` | Product Owner — tata letak, warna, dan penempatan keterangan tetap `DEV_DISCRETION`. Yang dikunci hanya **isi dan sumber datanya** | Nol tombol kirim — pengiriman tetap `EPIC FIN-12`. Nol perhitungan nilai di klien |

**Catatan revisi 2 atas `FE-FIN-006` yang sudah ✅ selesai.** Acceptance criteria-nya menyebut
`HELD_FOR_FINALIZATION` dibedakan dari `HELD` dan `FAILED`, mengikuti `FR-FIN-074` yang berlaku
saat task itu dikerjakan. `FR-FIN-074` **dicabut** 25 September 2026 (`FIN-DEC-030`). Statusnya
**tetap** ✅ dan buktinya **tetap berlaku** — ia benar untuk keadaan saat dikerjakan; yang
menindaklanjuti perubahan kebijakan adalah `FE-FIN-007`, bukan pembukaan ulang `FE-FIN-006`.
Riwayat tidak dihapus, sesuai `rules/rule-output/status-task-roadmap.md` bagian 5.

`FE-FIN-006` **baru** pada roadmap frontend ini. Revisi payung sebelumnya melewatkannya:
`03-frontend-architecture.md` bagian 3.5 menuntut dua layar pemantauan, dan keduanya sebelumnya
tidak punya task frontend sama sekali walau backend-nya (`BE-FIN-009`, `BE-FIN-012`) ada.

## 5. Yang MUST diputuskan UI brief

**Status: DITUTUP 23 September 2026.** Keenam hal ini sebelumnya menahan seluruh task
`FE-FIN-*` sampai diputuskan Product Owner (bukan developer). Seluruhnya sudah dijawab Yasmin
(Product Owner Finance) lewat `/grill-me`, tercatat sebagai `FIN-DEC-024`..`029` pada
`docs/module-blueprints/finance-management/00-interview-decisions.md` bagian
`Frontend Decision Authority` — rujukan itu yang otoritatif untuk detail dan evidence tiap
keputusan.

| # | Yang perlu diputuskan | Kenapa bukan wewenang developer | Hasil | Decision ID |
|---:|---|---|---|---|
| 1 | Struktur rute di bawah `/finance/...` | Menentukan bentuk navigasi seluruh modul | Ikuti pola existing: master data → `/finance/master-data/<entity>`, lainnya → slug flat `/finance/<capability>` | `FIN-DEC-024` |
| 2 | Urutan dan penamaan butir menu pada group `corporateFinance` | Yang dilihat pengguna tiap hari | "Rekening Bank"/"Mata Uang & Kurs" masuk submenu Master Data; "Piutang"/"Setoran & Kas Harian"/"Pemantauan Finance" jadi butir flat | `FIN-DEC-025` |
| 3 | Bentuk penyajian umur piutang | Kelompoknya sudah dikunci `FIN-DEC-010`; penyajiannya belum | Summary card 4 bucket + satu tabel, klik baris untuk trace ke tagihan asal | `FIN-DEC-026` |
| 4 | Rekonsiliasi shift: satu halaman atau tab | Memengaruhi alur kerja kasir | Halaman penuh terpisah | `FIN-DEC-027` |
| 5 | Koreksi dan penghapusan: modal atau halaman penuh | Memengaruhi ketelitian pada aksi yang menyentuh uang | Modal | `FIN-DEC-028` |
| 6 | Cetak dan unduh | Belum ada keputusan bisnisnya sama sekali | Ditunda dari MVP `FE-FIN-001`/`002`/`003`/`006`; lingkup detail dibuka sebagai `FIN-OQ-015` | `FIN-DEC-029` |

Yang **sudah** ditetapkan dan tidak perlu ditanyakan lagi: rute berada di bawah `/finance/...`
(bukan `/health-services/billing-management/...`), dan group menu `corporateFinance` sudah ada
sehingga tidak perlu membuat group baru.

## 6. `DEV_DISCRETION`

Diputuskan developer, tanpa perlu menunggu siapa pun:

| Keputusan | Catatan |
|---|---|
| Tata letak halaman, urutan kolom tabel | Mengikuti pola halaman Finance yang sudah ada |
| Modal atau halaman penuh untuk aksi kecil | — |
| Urutan butir menu di dalam group | Group-nya sendiri sudah ditetapkan |
| Tombol Setujui: disembunyikan atau dinonaktifkan bagi pengaju | Keduanya sah; yang MUST adalah backend tetap menolak |
| Kapan angka dimuat ulang otomatis | Kecuali setelah `409`, yang **MUST** memuat ulang |

Warna, tipografi, dan spacing mengikuti design system project — bukan wewenang modul ini.

## 7. Yang frontend MUST NOT lakukan

| Larangan | Alasan |
|---|---|
| Menghitung ulang nilai uang di klien | Satu-satunya sumber angka adalah backend; dua tempat perhitungan akan berbeda |
| Membulatkan ulang nilai uang | Pembulatan hanya terjadi sekali, di backend |
| Mengubah kelompok umur piutang | Dikunci `FIN-DEC-010` |
| Mulai dengan data tiruan yang bentuknya ditebak | `FIN-API-1.0` sudah tertulis dan terkunci; bentuk itulah yang dipakai |
| Mengandalkan penyembunyian tombol sebagai penjagaan wewenang | Backend yang menjaga; layar hanya menghemat satu putaran gagal |
| Mengubah aturan bisnis Petty Cash saat merapikan rute | Keputusannya milik `billing-kasir` (`FIN-DEC-009`) |
| Menyediakan tombol kirim kejadian ke Accounting | `EPIC FIN-12` `OPEN DECISION` |

## 8. Urutan yang disarankan

Mengikuti gelombang backend, karena tiap task frontend menunggu endpoint-nya ada:

```text
UI brief turun (closed 23 September 2026 — FIN-DEC-024..029)
   ↓
FE-FIN-001 🟡 source lengkap 23 September 2026, validasi belum dijalankan (data induk — paling kecil, bagus untuk menetapkan pola)
   ↓
FE-FIN-002   ✅ selesai 23 September 2026 (buku piutang — layar inti pertama)
FE-FIN-006   ✅ selesai 23 September 2026 (pemantauan — fakta intake & antrean outbox)
   ↓
FE-FIN-003   ✅ selesai 23 September 2026 (setoran dan kas harian)
   ↓
FE-FIN-004   siap                  (owner Billing sudah menjawab 21 September 2026; BE-FIN-016..018 selesai 23 September 2026)
FE-FIN-005   ✅ selesai 23 September 2026 (merapikan Petty Cash, tidak mengunci apa pun)
```

`FE-FIN-001` sengaja didahulukan bukan karena paling penting, melainkan karena paling kecil —
ia menetapkan pola halaman, penanganan `409`, dan penerapan `[AccessPermission]` di layar yang
akan diikuti seluruh layar berikutnya.

## 9. Definition of Done frontend

| # | Kriteria |
|---:|---|
| 1 | UI brief disetujui Product Owner sebelum task pertama dimulai |
| 2 | Bentuk data mengikuti `FIN-API-1.0` apa adanya — nol tebakan |
| 3 | Aksi disembunyikan atau dinonaktifkan sesuai `FIN-PERM-1.0` |
| 4 | Nol perhitungan dan nol pembulatan nilai uang di klien |
| 5 | `409` memuat ulang data dan memberi tahu pengguna, tanpa mengirim ulang diam-diam |
| 6 | Rute baru di bawah `/finance/...`; butir menu masuk group `corporateFinance` yang sudah ada |
| 7 | Layar pemantauan membedakan `HELD_FOR_FINALIZATION`, `HELD`, dan `FAILED` |
| 8 | Uji regresi Petty Cash lulus pada `FE-FIN-005` — nol kemampuan pengguna yang hilang |
| 9 | Skenario UAT yang tertaut pada kolom Verifikasi lulus |
| 10 | Nol layar untuk `EPIC FIN-04` dan `EPIC FIN-12` |
