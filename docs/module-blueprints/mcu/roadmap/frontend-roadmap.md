# Frontend Roadmap — MCU (Medical Check-Up)

| Field | Nilai |
|---|---|
| Roadmap ID | `MCU-RM-FE-001` |
| Revision | `1` |
| Status | `approved` (Yoga Aji Pratama, 2026-10-21) |
| Blueprint | `MCU-BP-001` r1, `approved` (Yoga Aji Pratama, 2026-10-21) |
| Cakupan | `MVP-0` s.d. `MVP-3` — kebutuhan layar `FE-MCU-01` s.d. `FE-MCU-10` di `03-frontend-architecture.md`. `FE-MCU-11` (tombol rekomendasi) dan `FE-MCU-12` (kiosk) di luar roadmap |
| SHA baseline | FE `7fb1e1d3e` (branch `YogaV2`), BE `4d3e4f98` (branch `yoga`) |
| Kontrak masukan | `MCU-API-v1` dan `MCU-PERM-v1` `approved` 2026-10-21, ber-hash di [`blueprint-manifest.md`](../blueprint-manifest.md). **Kerja paralel dengan backend diizinkan** untuk lapisan data karena kontraknya sudah approved dan terkunci hash-nya |
| Masukan desain | `03-frontend-architecture.md` r1, `04-prd-to-mvp.md` §18 (UAT) |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Bukan izin menulis kode. Setiap task dikerjakan lewat `build-module-frontend` dengan approval task eksplisit.
> 2. **Menu, route, tab/modal, dan tata letak mengikuti [`05-screen-brief.md`](../05-screen-brief.md)** (`MCU-FE-002`, `approved` 2026-10-21). `DEV_DISCRETION` hanya pada rentang brief bagian 7; perubahan struktur butuh revisi brief.
> 3. Warna, ikon, dan jarak adalah `DEV_DISCRETION` mengikuti design system (`MCU-FE-005`). Isian angka desimal **tidak** memakai `type: "number"` mentah; pakai pola `DECIMAL_FIELD_PROPS`/`parseDecimal` (`MCU-FE-004`).
> 4. Uji unit: `node --import ./tests/helpers/register.mjs --test tests/unit/`. E2E memakai pola spec API-tiruan standalone dengan output ke scratchpad; predikat cegatan wajib memuat `/v1/`.
> 5. Folder mengikuti pola modul yang ada: `src/lib/state/slice/health-services/medical-checkup-management/`, `src/lib/constants/...`, `src/utils/...`, `src/components/view/...`, `src/app/health-services/medical-checkup-management/` (route per layar di brief bagian 3); menu di `src/utils/menu-sidebar/menu-items.jsx` (brief bagian 2).

## Grafik Dependency

```text
FE-MCU-001 (aturan bersama, mandiri)
FE-MCU-002 ─┐ (data master)            ✅FE-MCU-000 {brief layar, MCU-FE-002}
FE-MCU-006 ─┤ (data order & check-in)        │
FE-MCU-010 ─┘ (data pemeriksaan → penyerahan) │
                                             ▼
FE-MCU-002 + 000 ─> FE-MCU-003, FE-MCU-004, FE-MCU-005
FE-MCU-006 + 000 ─> FE-MCU-007 ─> FE-MCU-008 ─> FE-MCU-009
FE-MCU-010 + 000 ─> FE-MCU-011 ─> FE-MCU-012 ─> FE-MCU-013
                    FE-MCU-011 ─> FE-MCU-014
FE-MCU-015 (label Lab/Rad, butuh BE-MCU-020)
```

| Gelombang | Task | Pasangan backend |
|---|---|---|
| Kapan saja | `FE-MCU-000` ✅ (keputusan pemilik, 2026-10-21), `FE-MCU-001` ✅ | — |
| `MVP-0` | `FE-MCU-002`, `FE-MCU-003`, `FE-MCU-004`, `FE-MCU-005` | `BE-MCU-005`, `006`, `007` |
| `MVP-1` | `FE-MCU-006`, `FE-MCU-007`, `FE-MCU-008`, `FE-MCU-009` | `BE-MCU-010`, `011`, `013`, `014` |
| `MVP-2` | `FE-MCU-010`, `FE-MCU-011`, `FE-MCU-015` | `BE-MCU-020`..`025` |
| `MVP-3` | `FE-MCU-012`, `FE-MCU-013`, `FE-MCU-014` | `BE-MCU-030`..`034` |

## Ringkasan Task

| Task ID | Outcome | Kebutuhan layar | Dependency | Status |
|---|---|---|---|---|
| `FE-MCU-000` ✅ | Brief layar MCU: menu, route, tab/modal, tata letak — keputusan `MCU-FE-002` | Semua | Keputusan pemilik | Selesai 2026-10-21 — [`05-screen-brief.md`](../05-screen-brief.md) |
| `FE-MCU-001` ✅ | Aturan bersama tanpa tampilan: status → aksi, masking NIK, parsing desimal, peta warna status | Semua | — | Selesai 2026-10-23 — [laporan](../task/report/frontend/FE-MCU-001.md) |
| `FE-MCU-002` ✅ | Lapisan data master: paket, referensi, jadwal | `FE-MCU-01`..`03` | Kontrak API | Selesai 2026-10-23 — [laporan](../task/report/frontend/FE-MCU-002.md) |
| `FE-MCU-003` ✅ | Layar kelola paket | `FE-MCU-01` | 000, 002, `BE-MCU-005` | Selesai 2026-10-27 — [laporan](../task/report/frontend/FE-MCU-003.md) |
| `FE-MCU-004` ✅ | Layar jenis Pemeriksaan MCU dan kategori kelayakan | `FE-MCU-02` | 000, 002, `BE-MCU-006` | Selesai 2026-10-27 — [laporan](../task/report/frontend/FE-MCU-004.md) |
| `FE-MCU-005` ✅ | Layar jadwal, kuota, pelampauan, dokter jaga | `FE-MCU-03` | 000, 002, `BE-MCU-007` | Selesai 2026-10-27 — [laporan](../task/report/frontend/FE-MCU-005.md) |
| `FE-MCU-006` | Lapisan data order dan check-in | `FE-MCU-04`..`06` | Kontrak API | Siap |
| `FE-MCU-007` | Daftar Pasien MCU | `FE-MCU-04` | 000, 001, 006, `BE-MCU-010` | Siap |
| `FE-MCU-008` | Booking, jadwal ulang, batal | `FE-MCU-05` | 007, `BE-MCU-011` | Siap |
| `FE-MCU-009` | Check-in dan *walk-in* admisi | `FE-MCU-06` | 007, `BE-MCU-013`, `014` | Siap |
| `FE-MCU-010` | Lapisan data pemeriksaan, resume, penyerahan | `FE-MCU-07`..`10` | Kontrak API | Siap |
| `FE-MCU-011` | Layar pemeriksaan: komponen, mulai, isi hasil, tambah, batal komponen, batal penuh | `FE-MCU-07` | 007, 010, `BE-MCU-021`..`025` | Siap |
| `FE-MCU-012` | Layar resume dokter | `FE-MCU-08` | 011, `BE-MCU-030`, `031` | Siap |
| `FE-MCU-013` | Verifikasi, cetak, persetujuan, penyerahan | `FE-MCU-09` | 012, `BE-MCU-033`, `034` | Siap |
| `FE-MCU-014` | Ganti dokter MCU | `FE-MCU-10` | 011, `BE-MCU-032` | Siap |
| `FE-MCU-015` | Label *Dicakup paket MCU* pada detail pesanan Lab/Rad | Titik sentuh modul lain | `BE-MCU-020` | Siap sesudah backend |

## Rincian Task

### `FE-MCU-000` ✅ — Brief layar MCU (keputusan, bukan kode)

Selesai 2026-10-21: [`05-screen-brief.md`](../05-screen-brief.md), `MCU-FE-002` `approved` oleh Yoga Aji
Pratama — menu ringkas tanpa menu v1, detail order halaman bertab, Booking/*Walk-in*/Check-in di halaman
tersendiri, Jadwal & Kuota sebagai tabel per tanggal.

| Field | Isi |
|---|---|
| Outcome | Keputusan `MCU-FE-002` tercatat `approved`: letak menu MCU dan urutannya, route tiap layar, pola Daftar Pasien MCU → detail order (halaman, tab, atau drawer), serta bentuk formulir check-in, isi hasil, dan resume |
| Requirement/decision | `MCU-FE-002`; `MCU-DEC-016` (v1 hanya referensi tampilan) |
| Pemilik | Yoga Aji Pratama |
| Acceptance criteria | Baris `MCU-FE-002` di `00-interview-decisions.md` §9 dan `03-frontend-architecture.md` §2 berstatus `approved` dengan rentang yang diizinkan |
| Status | ✅ Kriteria terpenuhi 2026-10-21 |

### `FE-MCU-001` ✅ — Aturan bersama tanpa tampilan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/frontend/FE-MCU-001.md). `mcu-order-rules.js` (9 status angka enum + label + lencana, kunci `MCU-PERM-v1`, `resolveMcuOrderActions` mengikuti tabel header **brief layar**) dan `mcu-format-rules.js` (`maskMcuNik`, desimal memakai ulang pola Lab); 12 uji unit lulus, lint dan build lulus. Suite unit penuh membawa 46 kegagalan existing di luar MCU |
| Outcome | Modul `src/utils/health-services/medical-checkup-management/` berisi: pemetaan **status order + izin + kecocokan dokter → aksi yang ditampilkan** (`03-frontend-architecture.md` §3); masking NIK (`3201230101900001` → `3201********0001`); parsing angka desimal (`6,4` dan `6.4` → `6.4`); peta label dan warna sembilan status |
| Requirement/decision | `MCU-DEC-031`; `MCU-FE-003`, `004`, `005`; `VAL-MCU-43` |
| Kontrak | `MCU-STATE-v1`; `MCU-PERM-v1` |
| Reuse | `lab-decimal-input-rules.js` (`DECIMAL_FIELD_PROPS`/`parseDecimal`) |
| Dependency | — |
| Acceptance criteria | Uji unit: order *Terjadwal* hari ini menampilkan Check-in, Jadwal ulang, Batal; order *Disetujui Dokter* menampilkan Buka ulang **hanya** bagi dokter yang ditetapkan; masking dan parsing sesuai contoh |
| Verifikasi | `node --import ./tests/helpers/register.mjs --test tests/unit/` |
| Risiko/pemilik | Server tetap penentu akhir; aturan ini hanya menyembunyikan tombol. Pemilik: Yoga |
| DoD | Uji unit lulus; lint lulus |

### `FE-MCU-002` ✅ — Lapisan data master

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-23 — [laporan](../task/report/frontend/FE-MCU-002.md). Konstanta `MCU_API`, service 19 endpoint, slice `mcuPackage`/`mcuReferenceData`/`mcuSchedule` terdaftar di store, pembaca galat bersama `mcu-request-rules.js`; 6 uji unit (method + path persis kontrak, pesan `400`/`403`/`409` apa adanya) lulus; lint dan build lulus |
| Outcome | Slice Redux + thunk + konstanta endpoint untuk grup `Mcu Package`, `Mcu Reference Data`, `Mcu Daily Schedule` |
| Requirement/decision | `FR-MCU-002`..`006` |
| Kontrak | `MCU-API-v1` tiga grup di atas |
| Reuse | `InstanceAxios`, pola slice Lab/Rad |
| Dependency | Kontrak API (paralel dengan `BE-MCU-005`..`007`) |
| Acceptance criteria | Uji unit thunk: setiap aksi memanggil path dan method persis kontrak; pesan `400`/`403`/`409` server diteruskan apa adanya ke state galat |
| Verifikasi | Uji unit |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Uji unit lulus |

### `FE-MCU-003` ✅ — Layar kelola paket

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-27 — [laporan](../task/report/frontend/FE-MCU-003.md). Menu *Medical Check-Up › Master Data › Paket MCU*, daftar, Buat/Ubah Paket (4 jenis komponen, persyaratan, total berjalan), Harga Perusahaan (tambah/Akhiri); 7 uji unit + E2E API-tiruan 12/12 lulus; `UAT-01` dan `UAT-02` lulus terhadap backend lokal dengan data uji devYoga (`TF-RSMMC-00452`, paket `UJI-MCU-BASIC`); lint dan build lulus |
| Outcome | Admin MCU membuat/mengubah paket, komponen (Lab/Rad/Pemeriksaan MCU), persyaratan, nilai pengembalian dan jasa, harga perusahaan bermasa berlaku, dan mengaktifkan/menonaktifkan paket |
| Requirement/decision | `FR-MCU-002`, `003`; `UAT-01`, `UAT-02` |
| Kontrak | `MCU-API-v1` Mcu Package; `VAL-MCU-01`..`08` |
| Reuse | `BaseSelectField`/`FilterSelect`, tabel standar, isian desimal `MCU-FE-004` |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-002` ✅, `BE-MCU-005` ✅ |
| Acceptance criteria | `UAT-01`, `UAT-02`. **Contoh:** total nilai pengembalian Rp 390.000 pada paket Rp 350.000 → pesan server tampil apa adanya; isian tidak hilang |
| Verifikasi | E2E API-tiruan + UAT terhadap backend lokal |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-004` ✅ — Layar jenis Pemeriksaan MCU dan kategori kelayakan

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-27 — [laporan](../task/report/frontend/FE-MCU-004.md). Menu *Jenis Pemeriksaan MCU* dan *Kategori Kelayakan*, daftar jenis, Buat/Ubah jenis dengan tabel isian (nomor butir, kode, label, jenis, satuan, pilihan, wajib) dan sakelar status, daftar kategori + modal buat/ubah; 8 uji unit (uji mutasi terbukti menangkap) + E2E API-tiruan 9/9 lulus; kedua kriteria terbukti; UAT baca terhadap backend lokal lulus (urutan isian `TANDA_VITAL` sama dengan API); lint dan build lulus |
| Outcome | Admin MCU mengelola jenis Pemeriksaan MCU beserta isian ber-nomor butir (teks, angka, pilihan) dan kategori kelayakan |
| Requirement/decision | `FR-MCU-004` |
| Kontrak | `MCU-API-v1` Mcu Reference Data; `VAL-MCU-09` |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-002` ✅, `BE-MCU-006` ✅ |
| Acceptance criteria | Isian pilihan tanpa daftar pilihan ditolak dengan pesan server; urutan isian mengikuti nomor butir |
| Verifikasi | E2E API-tiruan |
| Risiko/pemilik | Isi master usulan belum diratifikasi dokter MCU (`MCU-OQ-002`/`003`). Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-005` ✅ — Layar jadwal, kuota, dan dokter jaga

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-10-27 — [laporan](../task/report/frontend/FE-MCU-005.md). Menu *Jadwal & Kuota*, tabel per tanggal satu bulan (sisa kuota *26/30*, pelampauan, dokter jaga), modal *Atur Rentang Tanggal* dan *Setujui Pelampauan*; dokter jaga dari `/doctors/admin/options` (keputusan pemilik 2026-10-27, sekaligus revisi perusahaan penjamin `FE-MCU-003`); 7 uji unit + E2E API-tiruan 6/6 lulus; ketiga kriteria terbukti; UAT baca lulus; lint dan build lulus |
| Outcome | Admin MCU mengatur hari buka, kuota, dan dokter MCU jaga untuk rentang tanggal; kepala unit menyetujui pelampauan beralasan |
| Requirement/decision | `FR-MCU-005`, `006`; `MCU-DEC-042` |
| Kontrak | `MCU-API-v1` Mcu Daily Schedule; `VAL-MCU-13`, `14` |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-002` ✅, `BE-MCU-007` ✅ |
| Acceptance criteria | Tampilan sisa kuota per tanggal (mis. *26/30*); kuota di bawah terpakai → pesan `409` server; tombol pelampauan hanya tampil bagi pemegang `McuSchedule : ApproveQuotaOverride` |
| Verifikasi | E2E API-tiruan |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-006` — Lapisan data order dan check-in

| Field | Isi |
|---|---|
| Outcome | Slice + thunk + konstanta untuk grup `Mcu Order` (kecuali rekomendasi dan konfirmasi datang) dan `Mcu Check In` |
| Requirement/decision | `FR-MCU-010`..`025` |
| Kontrak | `MCU-API-v1` dua grup di atas |
| Reuse | `InstanceAxios`; `POST patients` Patient Management yang sudah ada |
| Dependency | Kontrak API (paralel dengan `BE-MCU-010`..`014`) |
| Acceptance criteria | Uji unit thunk; setiap aksi ubah mengirim `version`; respons `409` *diubah petugas lain* memicu muat ulang detail order (`03-frontend-architecture.md` §4) |
| Verifikasi | Uji unit |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Uji unit lulus |

### `FE-MCU-007` — Daftar Pasien MCU

| Field | Isi |
|---|---|
| Outcome | Daftar peserta per tanggal dengan ringkasan per status, penyaring status/sumber/perusahaan/dokter, *n dari m komponen tuntas*, NIK dimasking, aksi menurut `FE-MCU-001` |
| Requirement/decision | `FR-MCU-037` (tampilan); `MCU-FE-003` |
| Kontrak | `MCU-API-v1` Mcu Order (GET) |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-001`, `FE-MCU-006`, `BE-MCU-010` |
| Acceptance criteria | Keadaan kosong menampilkan *"Belum ada peserta MCU pada tanggal ini."*; gagal muat menampilkan tombol *Coba lagi*; NIK tidak pernah tampil utuh di daftar |
| Verifikasi | E2E API-tiruan |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-008` — Booking, jadwal ulang, batal

| Field | Isi |
|---|---|
| Outcome | Admisi membooking pasien lama atau identitas sementara (nama, NIK 16 digit, tanggal lahir, HP), menjadwal ulang, dan membatalkan beralasan |
| Requirement/decision | `FR-MCU-010`..`014`; `UAT-03`, `UAT-04` |
| Kontrak | `VAL-MCU-15`..`18`, `23` |
| Dependency | `FE-MCU-007`, `BE-MCU-011` |
| Acceptance criteria | `UAT-03`, `UAT-04`. Pesan kuota penuh tampil apa adanya, mis. *"Kuota MCU tanggal 27-10-2026 sudah penuh (30/30)…"* |
| Verifikasi | E2E API-tiruan + UAT backend lokal |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-009` — Check-in dan *walk-in* admisi

| Field | Isi |
|---|---|
| Outcome | Admisi mencocokkan pasien (NIK + tanggal lahir; konfirmasi wajib bila tanggal lahir beda), membuat pasien baru berisi isian awal, memilih dokter MCU (bawaan dokter jaga), menandai setiap persyaratan, lalu check-in; serta mendaftarkan *walk-in* |
| Requirement/decision | `FR-MCU-020`..`025`; `MCU-DEC-039`, `042`; `UAT-06`..`08` |
| Kontrak | `VAL-MCU-24`..`30` |
| Reuse | Formulir pasien baru Patient Management |
| Dependency | `FE-MCU-007`, `BE-MCU-013`, `BE-MCU-014` |
| Acceptance criteria | `UAT-06`, `UAT-07`, `UAT-08`. Tombol check-in nonaktif selama permintaan berjalan (kiriman ganda) |
| Verifikasi | E2E API-tiruan + UAT backend lokal |
| Risiko/pemilik | Keselamatan identifikasi pasien. Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-010` — Lapisan data pemeriksaan, resume, penyerahan

| Field | Isi |
|---|---|
| Outcome | Slice + thunk + konstanta untuk grup `Mcu Examination`, `Mcu Resume`, `Mcu Delivery` |
| Requirement/decision | `FR-MCU-030`..`053` |
| Kontrak | `MCU-API-v1` tiga grup di atas |
| Dependency | Kontrak API (paralel dengan `BE-MCU-021`..`034`) |
| Acceptance criteria | Uji unit thunk; ringkasan perusahaan diambil dari `result-document?variant=fitness-summary`, **tidak** disusun dari data lengkap di browser (`MCU-FE-003`) |
| Verifikasi | Uji unit |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Uji unit lulus |

### `FE-MCU-011` — Layar pemeriksaan

| Field | Isi |
|---|---|
| Outcome | Petugas menekan *Mulai pemeriksaan*; semua peran melihat status komponen dan menekan *Muat ulang status komponen*; dokter mengisi Pemeriksaan MCU, menambah komponen di luar paket, menjadwal ulang komponen, dan membatalkan komponen; petugas melakukan batal penuh |
| Requirement/decision | `FR-MCU-030`..`037`; `UAT-09`..`13` |
| Kontrak | `VAL-MCU-34`, `36`, `41`..`44` |
| Reuse | Isian desimal `MCU-FE-004` |
| Dependency | `FE-MCU-007`, `FE-MCU-010`, `BE-MCU-021`..`025` |
| Acceptance criteria | `UAT-09`..`UAT-13`. Isian `6.4` tersimpan `6.4` |
| Verifikasi | E2E API-tiruan + UAT backend lokal |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-012` — Layar resume dokter

| Field | Isi |
|---|---|
| Outcome | Dokter MCU yang ditetapkan menyusun draf (kategori kelayakan, kondisi umum, temuan, rekomendasi), menyetujui, membuka ulang beralasan, dan membaca versi lama bertanda *Dikoreksi* |
| Requirement/decision | `FR-MCU-040`, `041`, `044`; `UAT-14`, `UAT-16`, `UAT-17` |
| Kontrak | `VAL-MCU-35`..`38`, `40` |
| Dependency | `FE-MCU-011`, `BE-MCU-030`, `031` |
| Acceptance criteria | `UAT-14`, `UAT-16`, `UAT-17` |
| Verifikasi | E2E API-tiruan + UAT backend lokal (butuh akun dokter MCU kedua untuk `UAT-16`) |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-013` — Verifikasi, cetak, persetujuan, penyerahan

| Field | Isi |
|---|---|
| Outcome | Petugas memverifikasi atau mengembalikan resume, mencetak hasil lengkap dan ringkasan kelayakan, mencatat persetujuan pelepasan, mencatat penyerahan (saluran, tujuan, waktu), dan mencatat pemberitahuan koreksi |
| Requirement/decision | `FR-MCU-042`, `043`, `050`..`053`; `UAT-15`, `UAT-18`, `UAT-19` |
| Kontrak | `VAL-MCU-31`..`33`, `39` |
| Dependency | `FE-MCU-012`, `BE-MCU-033`, `034` |
| Acceptance criteria | `UAT-15`, `UAT-18`, `UAT-19`. Cetak ringkasan tidak memuat nilai maupun diagnosis |
| Verifikasi | E2E API-tiruan + UAT backend lokal |
| Risiko/pemilik | Bentuk bukti persetujuan menunggu `MCU-OQ-003` (go-live). Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-014` — Ganti dokter MCU

| Field | Isi |
|---|---|
| Outcome | Kepala unit memilih dokter pengganti dan menulis alasan |
| Requirement/decision | `FR-MCU-045`; `MCU-DEC-040` |
| Kontrak | `VAL-MCU-45` |
| Dependency | `FE-MCU-011`, `BE-MCU-032` |
| Acceptance criteria | Tombol hanya tampil bagi pemegang `McuOrder : ReassignDoctor`; alasan kosong → pesan server |
| Verifikasi | E2E API-tiruan |
| Risiko/pemilik | Pemilik: Yoga |
| DoD | Kriteria terbukti |

### `FE-MCU-015` — Label *Dicakup paket MCU* di Lab/Rad

| Field | Isi |
|---|---|
| Outcome | Detail pesanan Lab dan Rad menampilkan label *Dicakup paket MCU* bila ruas baca dari `BE-MCU-020` bernilai benar |
| Requirement/decision | `MCU-DEC-022`; `03-frontend-architecture.md` §6 |
| Kontrak | `MCU-API-v1` *Perubahan internal* (Lab, Rad — ruas baca) |
| Reuse | Halaman detail pesanan Lab/Rad yang ada; letak label `DEV_DISCRETION` di dalam halaman itu |
| Dependency | `BE-MCU-020` |
| Acceptance criteria | Pesanan bertanda menampilkan label; pesanan biasa tidak |
| Verifikasi | Uji unit + E2E API-tiruan |
| Risiko/pemilik | Menyentuh layar modul Lab dan Rad. Pemilik: Lab, Rad |
| DoD | Kriteria terbukti |

## Di luar Roadmap Ini

| Kebutuhan | Penyebab |
|---|---|
| `FE-MCU-11` Tombol rekomendasi MCU di RJ/RI/IGD | `EPIC MCU-09` `POST-MVP`; dikerjakan di modul masing-masing |
| `FE-MCU-12` Kiosk | `EPIC MCU-08` `POST-MVP`; milik modul kiosk (`MCU-COORD-006`) |
| Impor rombongan | `EPIC MCU-07` `POST-MVP` |
| Laporan MCU | `DEC-MCU-005` |
| Perubahan layar refund kasir akibat `BE-MCU-026` | Milik `billing-kasir` (Yasmina); ditetapkan saat `BE-MCU-026` dikerjakan |
