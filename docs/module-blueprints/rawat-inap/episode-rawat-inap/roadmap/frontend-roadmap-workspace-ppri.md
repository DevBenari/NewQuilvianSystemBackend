# Roadmap Frontend — Episode Rawat Inap, Workspace PPRI

| Field | Nilai |
|---|---|
| Roadmap | `episode-rawat-inap/roadmap/frontend-roadmap-workspace-ppri.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `9`, sub-modul `episode-rawat-inap`, kontrak **`0.11.0` `approved`** 2026-10-08 lewat `RWI-DEC-265` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-frontend` sebelum approval itu tercatat |
| Ditulis | 8 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `03-frontend-architecture.md` bagian 14 (`9ce17040…`), `contracts/api-contract.md` bagian 12 (`17119194…`), `contracts/validation-matrix.md` bagian 15 (`7c649c32…`), `contracts/permission-audit-matrix.md` bagian 10 (`04cc3b28…`), `testing/acceptance-test-matrix.md` bagian 21 (`0c34160f…`); peta menu `02-module-map.md` revision `5` bagian 8.3 (`19445a0a…`). Hash lengkap pada `../blueprint-manifest.md` bagian 12.1 |
| Keputusan | `RWI-DEC-225` s.d. `266`; terbuka `RWI-OQ-121` (judul halaman, tidak memblokir), `RWI-OQ-124` (data peran, memblokir UAT saja), `DEC-INP-020`; gate `1.11` bagian 20 |
| Source SHA | Frontend `dd2cbf7c` (branch `HamzahV2`); backend `fdf85a07` |
| Deret ID | `FE-RWI-210` s.d. `FE-RWI-221` |
| Roadmap pendamping | `backend-roadmap-workspace-ppri.md`, `requirement-traceability-workspace-ppri.md` |

**Kebijakan verifikasi frontend.** Mengikuti `rules/frontend/test-policy.md`: bukti utama verifikasi manual kontrol interaktif dengan backend berjalan, ditambah `npm run lint`, `npm run test:unit` (suite yang ada), dan `npm run build`. Test unit baru opsional; layak ditulis untuk logika murni seperti pemetaan kop, penentu tombol dari `AvailableActions`, dan normalisasi telepon. Jest dan `@testing-library` tidak dipakai. Laporan task memuat baris `AUTOMATED TEST` dan `MANUAL TEST` terpisah.

**Kewenangan UI.** Mengikat (`03-frontend-architecture.md` 14.3): empat wilayah template Workspace Keperawatan (`ClinicalPageHeader`, `ClinicalStateBoundary`, `ClinicalWorkspaceShell`, `ClinicalSectionNav`); menu dan tab di alamat halaman (`?section=`, `?tab=`); tombol "Workspace PPRI" tepat sesudah Workspace Dokter (`RWI-DEC-245`); General Consent dua tab cetak tanpa tombol Simpan; lencana berupa teks atau ikon, tidak hanya warna; isi cetakan gelang dan label; tombol bahasa Privasi; dapat dipakai di tablet ≥ 10 inci. `DEV_DISCRETION`: nama rute dan kunci section, warna, jarak, ikon, bentuk lencana, susunan tab, ukuran kertas dan CSS cetak gelang dan label (diuji dengan printer rumah sakit saat UAT, G-37), titik patah tata letak. Judul "Ruang Kerja PPRI" dan subjudul "Penerimaan Pasien Rawat Inap" tetap `draft` sampai `RWI-OQ-121` dijawab. Tidak ada butir menu baru; seluruh layar adalah layar anak Detail Episode (`02-module-map.md` 8.3).

**Satu aturan cetak untuk semua task.** Props `KopSurat` dan kota penandatanganan selalu diisi dari server (`Letterhead` dan dokumen). Nilai bawaan komponen yang menanam identitas rumah sakit client (`kop-surat.jsx:4-12`, `signature-section.jsx:20-31`) tidak pernah dipakai di Workspace PPRI (`03-frontend-architecture.md` 14.5, `RWI-AC-368`).

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

Roadmap ini memuat 12 task, 11 prasyarat dari roadmap backend, dan 1 gerbang keputusan, melewati batas 15 node satu grafik. Grafik dipecah per slice di bawah judul slice masing-masing; grafik di bawah ini adalah ringkasan antar-slice.

```text
Slice F1 Master, ruang kerja, dan cetakan ─> Slice F2 Dokumen bertanda tangan
```

Slice F2 menunggu Slice F1 lewat `FE-RWI-213`, tempat alur cetak bercatatan dan dialog alasan cetak ulang dibangun. Jumlah pasangan prasyarat→task pada kedua grafik slice: **22** (F1: 9, F2: 13), termasuk satu pasangan gerbang→task, sama dengan isi kolom `Dependency`.

| Label | Asal |
|---|---|
| `[BE]` | Task backend pada `backend-roadmap-workspace-ppri.md`. Cermin baca-saja; tanda statusnya disalin dari roadmap itu |
| `[F1]` | Task Slice F1 roadmap ini, digambar lengkap di grafik Slice F1. Di grafik Slice F2 ia hanya titik sambung antar-blok, bukan task kedua |
| `{…}` | Keputusan yang belum turun, bukan task |

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-186` [BE] | `FE-RWI-210`, `FE-RWI-211` — boleh paralel |
| 1 | `BE-RWI-195` [BE] | `FE-RWI-212` |
| 2 | `FE-RWI-212`, `BE-RWI-196` [BE] | `FE-RWI-213` |
| 2 | `FE-RWI-212`, `BE-RWI-198` [BE] | `FE-RWI-214` |
| 3 | `FE-RWI-213`, `BE-RWI-197` [BE] | `FE-RWI-215` |
| 3 | `FE-RWI-213`, `BE-RWI-194` [BE] | `FE-RWI-216` |
| 4 | `FE-RWI-216`, `BE-RWI-199` [BE] | `FE-RWI-217` |
| 4 | `FE-RWI-216`, `BE-RWI-200` [BE] | `FE-RWI-218` |
| 4 | `FE-RWI-216`, `BE-RWI-201` [BE] | `FE-RWI-219` |
| 4 | `FE-RWI-216`, `BE-RWI-202` [BE] | `FE-RWI-220` |
| — | ⛔ menunggu `DEC-INP-020` (Yasmina), `BE-RWI-203` ⛔ [BE], dan `FE-RWI-216` | `FE-RWI-221` |

Gelombang dihitung dari prasyarat frontend; prasyarat `[BE]` ditulis di kolom "Boleh mulai setelah" dan tetap harus ✅ di roadmap backend sebelum task frontend dimulai.

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 24.20.** `RWA-MVP-0`: `FE-RWI-210`, `211` (`FE-INP-12`, `13`). `RWA-MVP-1`: `FE-RWI-212` s.d. `215` (`FE-INP-35`, `36`, `38`, `40`; perubahan `FE-INP-04`, `18`, `03`). `RWA-MVP-2`: `FE-RWI-216` s.d. `220` (`FE-INP-39`, `37`, `44`, `43`, `41`). Di luar gelombang: `FE-RWI-221` (`FE-INP-42`, `OPEN DECISION`). Navigasi kiri hanya menampilkan menu yang layarnya sudah dikirim (`FR-RWA-004`), sehingga menu bertambah seiring task layar selesai.

**Catatan rilis untuk keputusan pemilik.** Kelengkapan dihitung server menurut `RWI-DEC-234` dan sejak `BE-RWI-195` sudah mewajibkan Serah Terima, Nilai Kepercayaan, serta — menurut penjamin dan deposit — Selisih Biaya dan Pelunasan Deposit (`contracts/state-transition-matrix.md` 10.2). Bila `RWA-MVP-1` dirilis ke pengguna sebelum `RWA-MVP-2`, header Workspace PPRI dan Detail Episode memperingatkan "Dokumen admisi belum lengkap" untuk dokumen yang menunya belum tampil. Dua jalan yang sah tanpa mengubah desain: rilis `RWA-MVP-1` dan `RWA-MVP-2` bersama, atau terima peringatan itu selama jeda. Pilihannya milik Muhammad Hamzah saat approval roadmap; roadmap ini tidak memilihkannya.

## Slice F1 — Master, ruang kerja, dan cetakan

Hasil slice: admin mengisi pengaturan dan butir serah terima; petugas membuka Workspace PPRI dari Detail Episode, mencetak gelang dan label, General Consent, dan IPD dari data server; tombol Cetak Persetujuan dan langkah 8 Admisi memakai kop dari profil rumah sakit.

### Grafik dependency slice F1

```text
BE-RWI-186 [BE] ─┬─> FE-RWI-210
                 │
                 └─> FE-RWI-211

BE-RWI-195 [BE] ─> FE-RWI-212 ─┬──────────────────┬─> FE-RWI-214
                               │                  │
                               │ BE-RWI-198 [BE] ─┘
                               │
                               └─┬─> FE-RWI-213 ──────────────────┬─> FE-RWI-215
                                 │                                │
               BE-RWI-196 [BE] ──┘               BE-RWI-197 [BE] ─┘
```

`[BE]` = task backend, cermin baca-saja. Pasangan: 9.

### Tabel task slice F1

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-210` | Admin mengisi sebelas isian cetak di Pengaturan Rawat Inap | `FR-RWA-050`, `126`; `RWI-DEC-243`, `247`; `VAL-RWA-54`, `55` | FE 14.1 `FE-INP-12`; API 12.4 | Layar `FE-INP-12` yang ada | Sebelas isian, validasi bentuk, pesan server | `BE-RWI-186` [BE] | Kartu | Kartu | Nilai produksi diisi admin / Muhammad Hamzah | Kartu |
| `FE-RWI-211` | Admin mengelola jenis, induk, dan sumber saran butir administrasi | `FR-RWA-030`; `RWI-DEC-048`, `241`; `VAL-RWA-50` s.d. `53` | FE 14.1 `FE-INP-13`; API 12.4 | Layar `FE-INP-13` yang ada | Saringan dan kolom jenis; isian induk dan sumber saran | `BE-RWI-186` [BE] | Kartu | Kartu | Butir produksi diisi admin sesudah `E9` / Muhammad Hamzah | Kartu |
| `FE-RWI-212` | Workspace PPRI terbuka dari Detail Episode dengan header, menu berlencana, kelengkapan, dan batas keadaan; peringatan di Detail Episode; kop langkah 8 Admisi dari server | `FR-RWA-001` s.d. `008`, `142`; `RWI-DEC-226`, `234`, `245`, `247`, `257`, `258`; `UAT-RWA-01`, `02` | FE 14.1 s.d. 14.3, 14.4.1, 14.5; API 12.1, 12.2 | Template Workspace Keperawatan, `usePermission`, `KopSurat` | Rute, kerangka, header, navigasi; tombol dan peringatan `FE-INP-04`; kop `FE-INP-03` | `BE-RWI-195` [BE] | Kartu | Kartu | Rilis `RWA-MVP-1` tanpa `RWA-MVP-2` (catatan rilis) / Muhammad Hamzah | Kartu |
| `FE-RWI-213` | Gelang & Label Pasien tercetak dari server; setiap cetak tercatat dan cetak ulang beralasan | `FR-RWA-050` s.d. `053`; `RWI-DEC-240` (7), `243`, `253`, `259`; `UAT-RWA-10` s.d. `12`, `33` | FE 14.4.4; API 12.2 `/identity-labels`, `/print-logs` | `react-to-print`, `qrcode.react` | Pratinjau; alur cetak bercatatan bersama; dialog alasan; riwayat cetak | `FE-RWI-212`, `BE-RWI-196` [BE] | Kartu | Kartu | G-37 ukuran kertas, G-38 aturan sapaan / Muhammad Hamzah | Kartu |
| `FE-RWI-214` | General Consent dua tab cetak saja; Cetak Persetujuan dan tautan lama dialihkan | `FR-RWA-020` s.d. `022` (cetak), `140` s.d. `142`; `RWI-DEC-233`, `246`, `251`, `252`; `UAT-RWA-30` | FE 14.1, 14.4.3; API 12.2 `/general-consent/print-data` | `inpatient-consent-form.jsx`, hook kop `FE-RWI-212` | Dua tab, pilihan penanda tangan, tipe kamar, pengalihan `FE-INP-18`, tombol `FE-INP-04` | `FE-RWI-212`, `BE-RWI-198` [BE] | Kartu | Kartu | *Fail-closed* privasi (`RWI-DEC-230`) / Muhammad Hamzah | Kartu |
| `FE-RWI-215` | IPD terbaca dan tercetak dengan garis kosong dan cadangan aman | `FR-RWA-070` s.d. `072`; `RWI-DEC-244`, `254`, `258`; `UAT-RWA-15`, `16` | FE 14.4.5; API 12.2 `/base-data`, `/base-data/amounts` | Alur cetak `FE-RWI-213` | Lembar dua kolom, `BlankFields`, tarif kamar bersyarat hak, cetak ditahan | `FE-RWI-213`, `BE-RWI-197` [BE] | Kartu | Kartu | Tanpa isian ketik pengganti sumber / Muhammad Hamzah | Kartu |

## Slice F2 — Dokumen bertanda tangan

Hasil slice: lima dokumen bertanda tangan — Privasi, Serah Terima, Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit — dapat dibuat, dikunci, ditandatangani, dikoreksi lewat versi, dibatalkan, dan dicetak dari satu kerangka. Estimasi Biaya menunggu keputusan.

### Grafik dependency slice F2

```text
FE-RWI-213 [F1] ─┬─> FE-RWI-216 ─┬──────────────────┬─> FE-RWI-217
                 │               │                  │
BE-RWI-194 [BE] ─┘               │ BE-RWI-199 [BE] ─┘
                                 │
                                 ├──────────────────┬─> FE-RWI-218
                                 │                  │
                                 │ BE-RWI-200 [BE] ─┘
                                 │
                                 ├──────────────────┬─> FE-RWI-219
                                 │                  │
                                 │ BE-RWI-201 [BE] ─┘
                                 │
                                 ├──────────────────┬─> FE-RWI-220
                                 │                  │
                                 │ BE-RWI-202 [BE] ─┘
                                 │
                                 └──────────────────┬───┬─> FE-RWI-221 ⛔
                                                    │   │
                                BE-RWI-203 ⛔ [BE] ─┘   │
                                                        │
                                  {DEC-INP-020 ⛔} ─────┘
```

`[F1]` = titik sambung dari grafik Slice F1; `[BE]` = task backend, cermin baca-saja; `{…}` = keputusan yang belum turun. Pasangan: 13 (12 antar-task, 1 gerbang→task).

### Tabel task slice F2

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-216` | Kerangka dokumen bertanda tangan berdiri dan Permintaan Privasi berjalan penuh sebagai jenis pertama | `FR-RWA-035`, `060` s.d. `062`, `120` s.d. `128`; `RWI-DEC-237` s.d. `240`, `263`; `UAT-RWA-13`, `14`, `25` s.d. `27` | FE 14.1, 14.4.2, 14.4.7, 14.5; API 12.2, 12.3 | Alur cetak `FE-RWI-213`, komponen surat, pola penyegaran | Formulir dan Riwayat, tombol per aksi, tanda tangan, versi, batal, penyegaran 30 detik, layar Privasi | `FE-RWI-213`, `BE-RWI-194` [BE] | Kartu | Kartu | Seluruh dokumen `RWA-MVP-2` bergantung padanya / Muhammad Hamzah | Kartu |
| `FE-RWI-217` | Serah Terima Pasien Baru dengan saran sistem dan tiga tanda tangan dari tiga layar | `FR-RWA-030` s.d. `035`; `RWI-DEC-239`, `241`, `255`, `262`; `UAT-RWA-05` s.d. `07`, `28` | FE 14.4.2; validation 15.4, 15.5 | Kerangka `FE-RWI-216` | Daftar butir, Sudah/Belum, saran, tiga slot, mode tanda-tangan-saja | `FE-RWI-216`, `BE-RWI-199` [BE] | Kartu | Kartu | Peran uji `RWI-OQ-124` / Muhammad Hamzah | Kartu |
| `FE-RWI-218` | Nilai Kepercayaan per episode dengan butir episode lalu sebagai konsep | `FR-RWA-110` s.d. `113`; `RWI-DEC-242`; `UAT-RWA-23`, `24` | FE 14.4.7; validation `VAL-RWA-16`, `23` | Kerangka `FE-RWI-216` | Penanda tangan, butir 1–5, identitas berupa teks | `FE-RWI-216`, `BE-RWI-200` [BE] | Kartu | Kartu | G-35, G-42 / Muhammad Hamzah | Kartu |
| `FE-RWI-219` | Selisih Biaya untuk penjamin asuransi atau perusahaan | `FR-RWA-100` s.d. `103`; `RWI-DEC-234`, `256`; `UAT-RWA-21`, `22` | FE 14.4.7; validation `VAL-RWA-10`, `14`, `24` | Kerangka `FE-RWI-216` | Data pasien hanya-baca, subjek, deklarer, lencana "Tidak diperlukan" | `FE-RWI-216`, `BE-RWI-201` [BE] | Kartu | Kartu | No. ID deklarer sensitif (G-35) / Muhammad Hamzah | Kartu |
| `FE-RWI-220` | Pelunasan Deposit dari angka Billing; cetak berupiah hanya bagi `ViewAmount` | `FR-RWA-080` s.d. `085`; `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263`; `UAT-RWA-17`, `18`, `29`, `31` | FE 14.4.6; API 12.2 `/documents/{id}/amounts`, `/amount-print` | Kerangka `FE-RWI-216` | Data Wali, form pernyataan, jatuh tempo, cetak berupiah | `FE-RWI-216`, `BE-RWI-202` [BE] | Kartu | Kartu | Angka uang tidak pernah diketik (G-45) / Muhammad Hamzah, Yasmina | Kartu |
| `FE-RWI-221` ⛔ | Estimasi Biaya Rekap | `FR-RWA-090` s.d. `093`; `RWI-DEC-232`, `250`, `258`; `UAT-RWA-19`, `20` | FE 14.4.8; API 12.2 `/procedure-plan-mark` | Kerangka `FE-RWI-216` | Tab Rekap, tab Rinci berisi pemberitahuan, penanda rencana tindakan | `FE-RWI-216`, `BE-RWI-203` [BE]; `{DEC-INP-020}` | Kartu | Kartu | ⛔ `OPEN DECISION` / Yasmina | Kartu |

## Kartu task

### `FE-RWI-210` — Pengaturan Rawat Inap: sebelas isian cetak (`FE-INP-12`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Admin Master Data mengisi delapan kode formulir, kota penandatanganan, batas umur gelang bayi, dan kode singkat rumah sakit pada label di layar Pengaturan Rawat Inap, sehingga cetakan Workspace PPRI tidak menanam nilai apa pun |
| **Requirement/decision** | `FR-RWA-050`, `126`; `RWI-DEC-243`, `247`; `RWI-AC-368`; `VAL-RWA-54`, `55` |
| **Kontrak** | Frontend 14.1 (layar lama `FE-INP-12`); API 12.4 (`GET /`, `PUT /{id}` sebelas isian); validation 15.7 |
| **Reuse** | Layar `FE-INP-12` yang ada; dua ambang daftar pantau dari Finishing (`FE-RWI-199`) tetap |
| **Cakupan** | Sebelas isian pada formulir dan tampilan (pengelompokan `DEV_DISCRETION`); validasi bentuk di layar sejajar `MST-IST-001`, `002` (0–16 tahun; panjang 50, 100, 30); pesan `400` server ditampilkan per isian; isian kosong tetap kosong tanpa nilai bawaan di komponen |
| **Dependency** | `BE-RWI-186` [BE] |
| **Acceptance criteria** | 1. Sebelas isian tampil dari `GET` dan tersimpan lewat `PUT`. 2. Batas umur 17 → "Batas umur gelang bayi 0 sampai 16 tahun." dan tidak tersimpan. 3. Kode formulir 51 karakter → "*{isian}* terlalu panjang." 4. Isian lama di layar ini tetap berfungsi. 5. Pencarian kode formulir V1 dan kota bawaan V1 pada berkas layar ini = nol |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan backend berjalan |
| **Risiko/pemilik** | Nilai produksi diisi admin (`RWI-DEC-048`); nilai V1 hanya ada di seeder backend. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-210.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-211` — Butir Administrasi Rawat Inap: jenis, induk, sumber saran (`FE-INP-13`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Admin membedakan butir Penutupan dan butir Serah Terima Pasien Baru, menyusun sub-butir di bawah induknya, dan memilih sumber saran sistem untuk butir serah terima |
| **Requirement/decision** | `FR-RWA-030`; `RWI-DEC-048`, `241`; `RWI-AC-360`, `361`; `VAL-RWA-50` s.d. `53` |
| **Kontrak** | Frontend 14.1 (`FE-INP-13`); API 12.4 (`GET /`, `/options`, `/summary` dengan `checklistType`; `POST`/`PUT` tiga isian); validation 15.7 |
| **Reuse** | Layar `FE-INP-13` yang ada |
| **Cakupan** | Saringan dan kolom jenis; isian induk (pilihan hanya butir utama aktif berjenis sama) dan sumber saran — keduanya hanya untuk jenis Serah Terima; pesan `MST-ICI-001` s.d. `004` dari server |
| **Dependency** | `BE-RWI-186` [BE] |
| **Acceptance criteria** | 1. Saringan Serah Terima memuat butir `STPB-*` beserta nama induk; saringan Penutupan hanya butir penutupan. 2. Formulir butir penutupan tidak menampilkan isian induk dan sumber saran. 3. Keempat penolakan `MST-ICI-001` s.d. `004` tampil dengan pesan servernya. 4. Butir lama tampil sebagai Penutupan dan tetap dapat diubah. 5. Menonaktifkan induk tidak menonaktifkan sub-butir di daftar |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan backend berjalan |
| **Risiko/pemilik** | Butir serah terima produksi diisi admin sesudah `E9` dan saringan penutupan dirilis; sebelum mundur kode, butir `STPB-*` dinonaktifkan dulu (`02-backend-architecture.md` 13.12). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-211.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-212` — Kerangka Workspace PPRI, tombol dan peringatan Detail Episode, kop langkah 8 Admisi (`FE-INP-35`, `FE-INP-04`, `FE-INP-03`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas berhak membuka Workspace PPRI dari Detail Episode dan melihat header pasien, menu berlencana, kelengkapan, status deposit menurut haknya, serta batas keadaan; Detail Episode menampilkan peringatan dokumen admisi; langkah 8 Admisi mencetak kop dari profil rumah sakit |
| **Requirement/decision** | `FR-RWA-001` s.d. `008`, `142`; `RWI-DEC-226`, `234`, `245`, `247`, `257`, `258`; `RWI-AC-343` s.d. `346`, `378`, `379`; `NFR-RWA-10`, `14`, `16`, `17`; G-30; `UAT-RWA-01`, `02` |
| **Kontrak** | Frontend 14.1, 14.2, 14.3, 14.4.1, 14.5; API 12.1 (`GET episodes/{id}` `Warnings`), 12.2 (`/summary`, `/summary/amounts`, `/letterhead`), 12.3 `AdmissionWorkspaceSummaryResponse`; permission 10.1 |
| **Reuse** | `ClinicalPageHeader`, `ClinicalStateBoundary`, `ClinicalWorkspaceShell`, `ClinicalSectionNav`; pola rute `buildInpatientNursingWorkspaceRoute`; `usePermission`; `isReadOnlyEpisodeStatus`; `KopSurat` |
| **Cakupan** | Rute layar anak dengan `?section=` dan `?tab=` (nama rute `DEV_DISCRETION`); kepala halaman; header pasien; status deposit — `/summary/amounts` dipanggil hanya bila `ViewAmount`, selain itu "Deposit: lihat kasir"; kelengkapan *x* dari *y*; navigasi kiri dari `summary.Menus` yang layarnya sudah dikirim; `Availability` `NotYetAdmitted` dan `ReadOnly`; "Akses Tidak Tersedia"; kegagalan data pasien mengganti seluruh isi; `FE-INP-04`: tombol "Workspace PPRI" tepat sesudah Workspace Dokter dan tumpukan "Perlu diketahui" dari `Warnings`; satu hook kop dari `GET …/letterhead` yang dipakai ulang layar lain, dipasang pertama pada langkah 8 `FE-INP-03` |
| **Dependency** | `BE-RWI-195` [BE] |
| **Acceptance criteria** | 1. Tombol tampil tepat sesudah Workspace Dokter hanya bagi pemegang `InpatientAdmissionDocument : Read`; tanpa hak, alamat langsung → "Akses Tidak Tersedia" (`RWI-AC-343`). 2. Navigasi tanpa Assessment Edukasi dan MP Benefit, nol panggilan `patient-assessments` (`RWI-AC-344`); Estimasi Biaya tidak tampil. 3. Pasien samaran asuransi kurang deposit "0 dari 6", tunai deposit cukup "0 dari 4" (`RWI-AC-345`). 4. Akun tanpa `ViewAmount` melihat "Deposit: lihat kasir" tanpa panggilan `/summary/amounts`; akun `ViewAmount` melihat "kurang Rp 3.000.000" (`RWI-AC-379`). 5. Episode `Draft` → "Admisi belum dikonfirmasi"; episode `Closed` → banner hanya-baca dan tombol tulis tersembunyi. 6. Layanan pasien gagal → "DATA PASIEN TIDAK DAPAT DIMUAT" + Coba Muat Ulang, tanpa form (`FR-RWA-008`). 7. Detail Episode menampilkan "Dokumen admisi belum lengkap: *n* (…)" tanpa rupiah; sumber gagal → detail tetap tampil dengan "Kelengkapan dokumen admisi tidak dapat dihitung" (`NFR-RWA-16`). 8. Langkah 8 Admisi untuk episode `Draft` mencetak kop dari profil rumah sakit; pencarian nilai bawaan `KopSurat` di berkas Workspace PPRI = nol (`RWI-AC-368`). 9. Akun yang hanya memegang `InpatientAdmissionDocument : Read` tanpa hak modul lain → nol `403` (`RWI-AC-378`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan backend berjalan memakai akun dengan dan tanpa `ViewAmount` serta akun tanpa hak (`RWI-OQ-124`); pemantauan jaringan untuk kriteria 2, 4, 9 |
| **Risiko/pemilik** | `usePermission` sengaja "boleh" sebelum daftar hak termuat (`use-permission.jsx:17-27`); server tetap menolak `403`. Catatan rilis `RWA-MVP-1` di atas. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-212.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-213` — Gelang & Label Pasien dan alur cetak bercatatan (`FE-INP-38`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas mencetak gelang dewasa atau bayi dan label pasien dari data server; setiap cetak tercatat dan cetak ulang wajib beralasan. Alur cetak bercatatan dan dialog alasan cetak ulang di task ini dipakai ulang IPD dan seluruh dokumen |
| **Requirement/decision** | `FR-RWA-050` s.d. `053`, `127`; `RWI-DEC-240` butir 7, `243`, `253`, `259`; `RWI-AC-363`, `364`, `374`, `380`; G-33, G-37, G-38; `UAT-RWA-10` s.d. `12`, `33` |
| **Kontrak** | Frontend 14.4.4; API 12.2 (`GET /identity-labels`, `GET`/`POST /print-logs`), 12.3 `IdentityLabelResponse`; validation `VAL-RWA-40`, `41`, `45`, `46` |
| **Reuse** | `react-to-print`; `qrcode.react`; kerangka `FE-RWI-212` |
| **Cakupan** | Menu "Gelang & Label Pasien"; pratinjau gelang (dewasa, atau bayi beserta dua label kecil) dan label; QR dirender dari `QrPayload`, bukan dari berkas `QrCodePath`; alur cetak bersama: catat log dengan `Idempotency-Key`, lalu dialog cetak peramban, dan cetak dibatalkan bila log gagal dicatat; dialog alasan cetak ulang (rusak, hilang, data berubah, lainnya beserta keterangan); riwayat "Cetakan ke-*n*" |
| **Dependency** | `FE-RWI-212`, `BE-RWI-196` [BE] |
| **Acceptance criteria** | 1. Empat pasien samaran — pria 45 tahun, bayi baru lahir, anak 4 tahun, wanita dengan status nikah tidak diketahui — menampilkan jenis gelang dan sapaan sesuai server (`RWI-AC-363`). 2. QR terbaca "00-12-34-56" saja (`RWI-AC-364`); pasien lama tanpa berkas QR tetap tercetak (`RWI-AC-380`). 3. Label pasien tanpa nomor kartu tidak mencetak baris No. Kartu (`RWI-AC-374`, `UAT-RWA-33`). 4. Cetak kedua tidak dapat dilakukan tanpa alasan; dengan "rusak" riwayat menampilkan "Cetakan ke-2, rusak, oleh *nama*" (`UAT-RWA-12`). 5. Episode `Closed` → cetak pertama pun meminta alasan. 6. Log gagal dicatat → dialog cetak tidak terbuka dan pesan tampil. 7. Klik Cetak dua kali → satu log |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan pasien samaran; pemindaian QR; ukuran kertas diuji dengan printer rumah sakit saat UAT (G-37) |
| **Risiko/pemilik** | Aturan sapaan dan batas umur menunggu verifikasi tim keselamatan pasien sebelum produksi (G-38). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-213.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-214` — General Consent cetak saja dan pengalihan Cetak Persetujuan (`FE-INP-36`, `FE-INP-18`, `FE-INP-04`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas mencetak Surat Persetujuan 12 butir dan Formulir General Consent V1 dari data server tanpa satu pun permintaan tulis; tombol Cetak Persetujuan dan tautan lama membuka tab Surat Persetujuan |
| **Requirement/decision** | `FR-RWA-020` s.d. `022` (bagian cetak), `140` s.d. `142`; `RWI-DEC-230`, `233`, `246`, `251`, `252`; `RWI-AC-347` s.d. `349`, `372`, `373`; `UAT-RWA-30` |
| **Kontrak** | Frontend 14.1 (`FE-INP-04` butir 2, `FE-INP-18`), 14.2, 14.4.3; API 12.2 (`/general-consent/print-data`, `/letterhead`), 12.3 `GeneralConsentPrintDataResponse` |
| **Reuse** | `inpatient-consent-form.jsx` (surat 12 butir); hook kop `FE-RWI-212`; `A4Document`, `signature-section` |
| **Cakupan** | Menu "General Consent" berlencana "Cetak saja"; dua tab cetak; pilihan hubungan ke calon penanda tangan — "Istri/Suami" → `Spouse`, "Anak" → `Child`, "Orang Tua" → `Mother`/`Father`, lebih dari satu → petugas memilih, "Lainnya" → daftar relasi, kontak darurat, atau manual; tipe kamar dari server; isian terbuka bila relasi tidak ditemukan; tombol "Cetak Persetujuan" `FE-INP-04` ke tab Surat Persetujuan, dijaga `InpatientAdmissionDocument : Read`; rute lama `episodes/[id]/consent-print` dialihkan; pilihan petugas tidak disimpan |
| **Dependency** | `FE-RWI-212`, `BE-RWI-198` [BE] |
| **Acceptance criteria** | 1. Membuka kedua tab dan mencetak → nol permintaan tulis; tidak ada tombol Simpan (`RWI-AC-347`). 2. Hubungan "Istri" mengisi nama dan alamat dari relasi; relasi tidak ditemukan → isian terbuka berketerangan "tidak ditemukan di data wali/kontak darurat" (`RWI-AC-348`). 3. Tipe kamar mengikuti server: kamar bernama "Melati Khusus" tanpa penanda → Umum; bed intensif → Khusus (`RWI-AC-372`). 4. Dua relasi `Child` → petugas memilih; kontak darurat hanya muncul di daftar pilihan (`RWI-AC-373`). 5. Tombol Cetak Persetujuan membuka tab Surat Persetujuan; pengguna yang hanya memegang `InpatientEpisode : Read` tidak melihat tombol; tautan lama dialihkan (`RWI-AC-349`). 6. Kop kedua cetakan dari profil rumah sakit (`UAT-RWA-30`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan pemantauan jaringan untuk memastikan nol permintaan tulis |
| **Risiko/pemilik** | *Fail-closed* privasi: General Consent tetap cetak saja sampai `DEC-INP-003` (`RWI-DEC-230`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-214.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-215` — IPD (`FE-INP-40`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas membaca dan mencetak Data Dasar Rawat Inap dengan tata letak V1; isian tanpa sumber dicetak garis kosong, dan cetak ditahan bila data wajib gagal terbaca |
| **Requirement/decision** | `FR-RWA-070` s.d. `072`; `RWI-DEC-244`, `254`, `258`; `RWI-AC-365`, `375`, `386`, `387`; `UAT-RWA-15`, `16` |
| **Kontrak** | Frontend 14.4.5; API 12.2 (`/base-data`, `/base-data/amounts`, `/print-logs`), 12.3 `InpatientBaseDataResponse`; validation `VAL-RWA-44` |
| **Reuse** | Alur cetak bercatatan `FE-RWI-213`; hook kop `FE-RWI-212`; `A4Document` |
| **Cakupan** | Menu "IPD"; lembar dua kolom V1; `BlankFields` sebagai garis kosong tanpa isian ketik; "Rencana @ Kamar (Rp)" hanya bagi `ViewAmount`, selain itu atau saat `NotYetAvailable` → "lihat kasir"; `CanPrint = false` → Cetak nonaktif "Cetak ditahan sampai data wajib terbaca lengkap" + Coba Lagi; cetak dan cetak ulang lewat alur `FE-RWI-213` |
| **Dependency** | `FE-RWI-213`, `BE-RWI-197` [BE] |
| **Acceptance criteria** | 1. Garis kosong untuk pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat domisili, alamat kantor, no. mutasi, persetujuan direktur, perhatian khusus, dan kasir; tidak ada isian ketik (`RWI-AC-365`). 2. Kunjungan dengan surat pengantar `Issued` menampilkan diagnosis, rencana, dan dokter; kunjungan dengan surat `Cancelled` saja atau tanpa surat → garis kosong (`RWI-AC-375`). 3. Akun tanpa `ViewAmount` → "lihat kasir" tanpa memanggil `/base-data/amounts`; akun `ViewAmount` → "lihat kasir" selama `NotYetAvailable` (`RWI-AC-387`). 4. Layanan episode dimatikan → Cetak nonaktif dengan pesan dan Coba Lagi (`UAT-RWA-16`). 5. Dokumen Nilai Kepercayaan dan Privasi `Completed` mengisi bagiannya di IPD (`UAT-RWA-15`; diperiksa dengan data uji backend, atau ulang sesudah `FE-RWI-216` dan `218`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual pada tiga kunjungan samaran |
| **Risiko/pemilik** | Tidak boleh ada isian ketik pengganti sumber (`RWI-DEC-244`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-215.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-216` — Kerangka dokumen bertanda tangan dan Permintaan Privasi (`FE-INP-39`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Satu kerangka dokumen dipakai seluruh layar dokumen — tab Formulir dan Riwayat, tombol menurut `AvailableActions` dan hak, kolom tanda tangan per slot, kunci dan buka kunci, versi koreksi, buang konsep, batal, cetak per status, penyegaran 30 detik — dan dibuktikan pada Permintaan Privasi sebagai jenis pertama |
| **Requirement/decision** | `FR-RWA-035`, `060` s.d. `062`, `120` s.d. `128`; `RWI-DEC-237` s.d. `240`, `263`; `RWI-AC-351`, `352`, `355` s.d. `358`, `384`; `NFR-RWA-15`; `UAT-RWA-13`, `14`, `25` s.d. `27` |
| **Kontrak** | Frontend 14.1 (dua tab), 14.4.2 (pola), 14.4.7 (`FE-INP-39`), 14.5; API 12.2 (`/prefill/{documentType}`, `/documents`, `lock`, `unlock`, `discard`, `revisions`, `cancel`, `signatures/*`, `/documents/{id}/print`), 12.3; validation 15.1, 15.3 (`VAL-RWA-15`), 15.4 (`22`, `27`), 15.5, 15.6 |
| **Reuse** | Alur cetak bercatatan `FE-RWI-213`; hook kop `FE-RWI-212`; `KopSurat`, `A4Document`, `signature-section`, `informasi-pasien-surat`; pola `use-inpatient-billing-status.js` (`intervalMs`) |
| **Cakupan** | Kerangka dokumen: tab Formulir dan Riwayat (versi beserta alasan, tanda tangan, log cetak); tombol dari `AvailableActions` dan `usePermission`; dialog catatan tanda tangan kertas (nama, hubungan, waktu); tombol atestasi per slot; dialog alasan buang konsep (1–500), batal (10–500), dan koreksi (10–500); keadaan kosong "Belum ada *nama dokumen* untuk episode ini"; `409 INP-ADM-DOC-004` tanpa membuang isian petugas; satu `Idempotency-Key` per niat simpan, tanda tangan, dan catat cetak; penyegaran 30 detik selama menunggu tanda tangan; pesan "Data pasien telah diperbarui sejak dokumen ini dikunci." bila `SourceChangedSinceLock`. Jenis pertama, menu "Permintaan Privasi": tiga baris kerabat, tiga baris permintaan khusus, privasi transportasi, kota, tanggal, nama penanda tangan, keterangan, tombol bahasa Indonesia/English, kolom Pasien/Keluarga dan Kepala Ruangan |
| **Dependency** | `FE-RWI-213`, `BE-RWI-194` [BE] |
| **Acceptance criteria** | 1. Privasi `Draft` → Kunci → catatan tanda tangan kertas → atestasi Kepala Ruangan oleh akun lain → `Completed`; tanda tangan pihak lain tampil ≤ 30 detik tanpa memuat ulang (`NFR-RWA-15`). 2. Cetak `Draft` bertanda "KONSEP — BELUM DITANDATANGANI"; `AwaitingSignature` "Lembar untuk ditandatangani — versi 1" tanpa tanda konsep; final memuat catatan tanda tangan (`RWI-AC-384`, `UAT-RWA-14`). 3. Cetakan memuat "Ditandatangani secara elektronik oleh Maya …, Kepala Ruangan …" (`RWI-AC-351`); akun tanpa `SignAsHeadNurse` tidak melihat tombolnya (`RWI-AC-352`). 4. Kerabat keempat tidak dapat ditambah; "Sdr. Dimas, Jr." tersimpan satu baris; dokumen `Completed` terbuka lagi (`UAT-RWA-13`). 5. Versi koreksi beralasan → Riwayat menampilkan versi 1 `Superseded` dan versi 2 (`RWI-AC-355`, `UAT-RWA-25`); tidak ada tombol hapus; batal beralasan kurang dari 10 karakter ditolak (`RWI-AC-356`, `UAT-RWA-27`). 6. Dua petugas menyimpan konsep yang sama → pesan data basi dan isian tidak hilang (`UAT-RWA-26`). 7. Klik Simpan dua kali → satu dokumen. 8. Episode `Closed` → tombol tulis tersembunyi dan cetak ulang meminta alasan (`RWI-AC-358`). 9. Tombol bahasa mengganti label layar; cetakan dwibahasa |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan dua akun (admisi dan kepala ruangan); test unit opsional untuk penentu tombol dan pembentuk payload |
| **Risiko/pemilik** | Seluruh layar dokumen `RWA-MVP-2` memakai kerangka ini. Data kerabat sensitif (G-35). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-216.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-217` — Serah Terima Pasien Baru (`FE-INP-37`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas admisi mengonfirmasi butir serah terima beserta saran sistem, mengunci, dan menandatangani; CRO dan perawat penerima masing-masing menandatangani dari layarnya sendiri |
| **Requirement/decision** | `FR-RWA-030` s.d. `035`; `RWI-DEC-239`, `241`, `255`, `262`; `RWI-AC-353`, `354`, `359`, `360`, `376`, `383`; `UAT-RWA-05` s.d. `07`, `28` |
| **Kontrak** | Frontend 14.4.2; API 12.2 (`/prefill/NewPatientHandover`, `signatures/admission-officer`, `/cro`, `/receiving-nurse`); validation `VAL-RWA-17`, `20`, `21`, `30` s.d. `34` |
| **Reuse** | Kerangka dokumen `FE-RWI-216` |
| **Cakupan** | Daftar butir beku — atau butir master beserta saran sebelum dokumen dibuat — bernomor dengan sub-butir; Sudah/Belum saling meniadakan; Belum wajib berketerangan; saran sistem tampil tanpa memilih otomatis; tiga kolom tanda tangan; pemegang `SignAsCro` atau `SignAsNurse` saja melihat dokumen hanya-baca dengan satu tombol tanda tangan; tombol Perawat nonaktif "Pasien belum menempati tempat tidur"; master kosong → "Butir serah terima belum diatur. Hubungi admin." |
| **Dependency** | `FE-RWI-216`, `BE-RWI-199` [BE] |
| **Acceptance criteria** | 1. Butir 5 belum dipilih dan butir 11 Belum tanpa keterangan → satu penolakan dengan dua pesan bernomor; status tetap Konsep (`RWI-AC-359`, `UAT-RWA-07`). 2. Saran butir 1 menyebut dokter dan tanggal surat pengantar; saran butir 9 dan 13 muncul sesudah IPD dan gelang dicetak; butir 12 tanpa saran; seluruh butir tetap wajib dipilih (`RWI-AC-383`). 3. CRO membuka sebelum dikunci → tanpa tombol tanda tangan dan "Data serah terima belum dikirim oleh petugas admisi" (`RWI-AC-354`). 4. Akun yang sama mencoba slot kedua → pesan `INP-ADM-DOC-033` (`RWI-AC-353`, `UAT-RWA-06`). 5. Perawat sebelum pasien menempati bed → tombol nonaktif; sesudah penempatan berhasil (`RWI-AC-376`, `UAT-RWA-28`). 6. Tanda tangan tiga akun tampil di layar lain ≤ 30 detik; `Completed` menaikkan kelengkapan (`UAT-RWA-05`). 7. Dokumen lama tetap menampilkan nama butir lama sesudah admin mengganti nama butir (`RWI-AC-360`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan tiga akun dan pasien samaran yang bed-nya dipesan lalu ditempati |
| **Risiko/pemilik** | Peran CRO dan data peran uji menunggu `RWI-OQ-124`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-217.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-218` — Nilai Kepercayaan (`FE-INP-44`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas mencatat hal yang bertentangan dengan nilai dan kepercayaan pasien per episode, dengan butir dari dokumen lengkap episode lalu sebagai konsep |
| **Requirement/decision** | `FR-RWA-110` s.d. `113`; `RWI-DEC-242`; `RWI-AC-362`; `UAT-RWA-23`, `24` |
| **Kontrak** | Frontend 14.4.7; API 12.2 (`/prefill/BeliefValues`); validation `VAL-RWA-16`, `23` |
| **Reuse** | Kerangka dokumen `FE-RWI-216` |
| **Cakupan** | Penanda tangan (nama, tanggal lahir, umur dihitung, jenis kelamin, hubungan, alamat); data pasien dan agama hanya-baca; blok identitas pasien dirender dari data, bukan gambar; 1–5 butir hal yang bertentangan; kolom tanda tangan pasien/keluarga |
| **Dependency** | `FE-RWI-216`, `BE-RWI-200` [BE] |
| **Acceptance criteria** | 1. Episode baru menampilkan butir episode lalu sebagai Konsep (`RWI-AC-362`, `UAT-RWA-23`). 2. Butir keenam tidak dapat ditambah; penolakan server "Maksimal 5 butir." tampil bila terjadi. 3. Kunci tanpa butir → "Minimal satu hal yang bertentangan wajib diisi." (`UAT-RWA-24`). 4. Identitas pasien berupa teks, bukan gambar (`FR-RWA-113`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan dua episode pasien samaran |
| **Risiko/pemilik** | Isi keyakinan sensitif (G-35); peringatan di Workspace Keperawatan dan Dokter belum ada (G-42). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-218.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-219` — Selisih Biaya (`FE-INP-43`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas membuat surat pernyataan selisih biaya untuk pasien dengan penjamin asuransi atau perusahaan, dengan data pasien hanya-baca dan data deklarer V1 |
| **Requirement/decision** | `FR-RWA-100` s.d. `103`; `RWI-DEC-234`, `256`; `RWI-AC-377`; `UAT-RWA-21`, `22` |
| **Kontrak** | Frontend 14.4.7; API 12.2 (`/prefill/CostDifferenceStatement`); validation `VAL-RWA-10`, `14`, `24` |
| **Reuse** | Kerangka dokumen `FE-RWI-216` |
| **Cakupan** | Data pasien hanya-baca; subjek pernyataan ("diri saya sendiri" mengisi dari data pasien; "saudara kandung lainnya" wajib keterangan); deklarer: nama, alamat, pekerjaan, tipe dan No. ID, HP maksimal 13 digit, telepon kantor; kota, tanggal; kolom Deklarer dan Petugas PPRI; pasien tunai → lencana "Tidak diperlukan" dan form tidak dapat dibuat; No. ID disamarkan pada daftar dan Riwayat |
| **Dependency** | `FE-RWI-216`, `BE-RWI-201` [BE] |
| **Acceptance criteria** | 1. Subjek "istri saya", deklarer Rina, KTP, HP `081234567890` → tersimpan; cetakan dwibahasa dengan kop dari profil (`UAT-RWA-21`). 2. Pasien tunai → "Tidak diperlukan"; HP 14 digit → "Nomor telepon maksimal 13 digit." (`UAT-RWA-22`). 3. Penjamin yang tidak mengizinkan selisih dibebankan ke pasien tetap dapat membuat surat (`RWI-AC-377`). 4. No. ID disamarkan pada daftar dan Riwayat |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual pada pasien asuransi dan tunai samaran |
| **Risiko/pemilik** | Nomor identitas deklarer sensitif (G-35). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-219.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-220` — Pelunasan Deposit (`FE-INP-41`)

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas membuat surat kesediaan melunasi deposit dari angka Billing dengan jatuh tempo bawaan dan batasnya; kasir dan pemegang `ViewAmount` mencetak surat berupiah |
| **Requirement/decision** | `FR-RWA-080` s.d. `085`; `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263`; `RWI-AC-366`, `367`, `373`, `379`, `381`, `382`, `385`; `UAT-RWA-17`, `18`, `29`, `31` |
| **Kontrak** | Frontend 14.4.6; API 12.2 (`/prefill/DepositSettlementStatement`, `/documents/{id}/amounts`, `/amount-print`, `/summary/amounts`); validation `VAL-RWA-11`, `12`, `14`, `19`, `25` |
| **Reuse** | Kerangka dokumen `FE-RWI-216` |
| **Cakupan** | Data Wali: sumber (relasi, kontak darurat, manual), Ambil Data Wali, Reset, nama, alamat, telepon; form pernyataan dengan kekurangan dan teks perhitungan dari server, tanggal surat, jatuh tempo bawaan dan batasnya; angka hanya bagi `ViewAmount`; kekurangan 0 → pesan dan Simpan nonaktif; Billing gagal → angka tidak tampil, Simpan nonaktif, Coba Lagi; cetak lewat `/amount-print`, tombol Cetak tidak tampil tanpa `ViewAmount`; tidak ada isian angka yang dapat diketik |
| **Dependency** | `FE-RWI-216`, `BE-RWI-202` [BE] |
| **Acceptance criteria** | 1. "Rp 3.000.000 (Rp 5.000.000 − Rp 2.000.000)"; surat Jumat 9 Oktober 2026 → jatuh tempo bawaan Senin 12 Oktober 11.00 WIB (`RWI-AC-366`, `UAT-RWA-17`). 2. Deposit cukup → "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan.", Simpan nonaktif (`UAT-RWA-18`). 3. Jatuh tempo 13 Oktober → "Jatuh tempo paling lambat 12 Oktober 2026 menurut kebijakan deposit." (`RWI-AC-367`). 4. Dokumen terkunci tetap Rp 3.000.000 sesudah deposit bertambah; header Rp 2.000.000 (`RWI-AC-385`, `UAT-RWA-31`). 5. Akun tanpa `ViewAmount` → tanpa angka dan tanpa tombol Cetak (`RWI-AC-379`, `UAT-RWA-29`). 6. Data Wali dari relasi `Spouse`; kontak darurat hanya di daftar pilihan; dua relasi `Child` → petugas memilih (`RWI-AC-373`). 7. Lewat jatuh tempo dengan kekurangan masih ada → peringatan di header dan Detail Episode yang dirender `FE-RWI-212` (`RWI-AC-381`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan Billing berjalan, memakai akun dengan dan tanpa `ViewAmount` |
| **Risiko/pemilik** | Angka uang tidak pernah diketik (G-45). Pemilik: Muhammad Hamzah; Yasmina untuk kebijakan deposit |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-220.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### ⛔ `FE-RWI-221` — Estimasi Biaya Rekap (`FE-INP-42`)

| Field | Isi |
|---|---|
| **Status** | ⛔ **Terblokir** — menunggu `DEC-INP-020` (`RWI-OQ-122`, Yasmina, lewat Muhammad Hamzah) dan `BE-RWI-203` ⛔. Menu Estimasi Biaya tidak ditampilkan sebelum task ini dikirim |
| **Outcome** | Petugas menjelaskan prakiraan biaya dengan harga dari tarif, baris manual beralasan, catatan dari kebijakan Billing, dan penanda rencana tindakan |
| **Requirement/decision** | `FR-RWA-090` s.d. `093`; `RWI-DEC-232`, `250`, `258`; `RWI-AC-370`, `371`; `UAT-RWA-19`, `20` |
| **Kontrak** | Frontend 14.4.8; API 12.2 (`/procedure-plan-mark`, jenis `CostEstimate`); validation `VAL-RWA-26` |
| **Reuse** | Kerangka dokumen `FE-RWI-216` |
| **Cakupan** | Tab Rekap; tab Rinci berisi pemberitahuan; penanda "ada rencana tindakan/operasi"; menu kesembilan tampil sesudah dikirim. Rincian ditetapkan ulang sesudah keputusan |
| **Dependency** | `FE-RWI-216`, `BE-RWI-203` [BE]; `{DEC-INP-020}` |
| **Acceptance criteria** | Ditetapkan final saat keputusan turun; minimal `RWI-AC-370` (baris tarif tidak ditemukan menahan kunci) dan `RWI-AC-371` (aturan wajib dari kasus OK dan penanda) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan tarif uji |
| **Risiko/pemilik** | Pemilik: Yasmina untuk aturan biaya; Muhammad Hamzah untuk layar |
| **DoD** | Ditetapkan ulang sesudah keputusan; laporan `../task/report/frontend/FE-RWI-221.md` |

## Yang sengaja tidak direncanakan

| Tidak ada task | Sebab |
|---|---|
| Butir menu baru | Seluruh layar adalah layar anak Detail Episode (`02-module-map.md` 8.3) |
| Penyimpanan General Consent, tanda tangan digital di tablet (`EPIC-RWA-02`, `13`) | `DEC-INP-003` belum turun |
| Estimasi Biaya Rinci, MP Benefit | Ditunda (`04-prd-to-mvp.md` 24.8) |
| Peringatan nilai kepercayaan dan privasi di Workspace Keperawatan dan Dokter | Amandemen terpisah; gerbang produksi G-42 |
| Perbaikan nilai bawaan `KopSurat` untuk cetakan Final lain | Issue terpisah G-RWA-02 (`RWI-DEC-247`) |
| Jest, `@testing-library`, framework test baru | `rules/frontend/test-policy.md` |
