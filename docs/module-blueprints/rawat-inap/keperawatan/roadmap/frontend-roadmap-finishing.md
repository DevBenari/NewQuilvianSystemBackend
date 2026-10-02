# Roadmap Frontend — Keperawatan Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `keperawatan/roadmap/frontend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `keperawatan`, kontrak **`0.6.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-frontend` sebelum approval itu tercatat |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `03-frontend-architecture.md` bagian 11 (`4de16a5b…`), `contracts/api-contract.md` bagian 8 (`164b8ac8…`), `testing/acceptance-test-matrix.md` bagian Finishing (`91a92f39…`); Tagihan Pasien: `integrasi-billing` `contracts/api-contract.md` 3.7 dan 3.11 (`08a4be1b…`). Hash lengkap pada `../blueprint-manifest.md` bagian 9 |
| Keputusan | `RWI-DEC-170`, `172`, `178`, `179`, `200`, `202`, `203`, `207`, `209`, `211`, `212`, `216`, `219`, `221`; gate `1.10` |
| Source SHA | Frontend `f74758af5`; backend `bf5c6bde` |
| Deret ID | `FE-RWI-180` s.d. `FE-RWI-191` |
| Roadmap pendamping | `backend-roadmap-finishing.md`, `requirement-traceability-finishing.md` |

**Kebijakan verifikasi frontend.** Mengikuti `rules/frontend/test-policy.md`: bukti utama verifikasi manual kontrol interaktif dengan backend berjalan, ditambah `npm run lint`, `npm run test:unit` (suite yang ada), dan `npm run build`. Test unit baru opsional. Jest dan `@testing-library` tidak dipakai. Laporan task memuat baris `AUTOMATED TEST` dan `MANUAL TEST` terpisah.

**Kewenangan UI.** Mengikat: enam sub-menu Catatan Keperawatan dan urutannya, empat sub-tab Obat & Alkes (`RWI-DEC-172`), kolom suhu surveilans dan nomor kantong tidak dapat diketik (`RWI-DEC-202`, `203`), letak surveilans dan monitoring transfusi (`RWI-DEC-211`, `212`), letak daftar PPI (`RWI-DEC-216`), tidak ada rupiah bagi perawat. Bentuk kisi, warna status, dan ikon `DEV_DISCRETION` (`03-frontend-architecture.md` 11.6).

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

Roadmap ini memuat 12 task dan 10 prasyarat dari roadmap lain, melewati batas 15 node satu grafik. Grafik dipecah per slice di bawah judul slice masing-masing; grafik di bawah ini adalah ringkasan antar-slice.

```text
Slice K1 Catatan Keperawatan, DPO, Obat dan Alkes, WSD, Diet

Slice K2 Tagihan Pasien

Slice K3 Master dan Pemakaian Alat

Slice K4 Surveilans PPI dan Transfusi
```

Keempat slice tidak saling menunggu. Seluruh prasyarat lintas roadmap digambar pada grafik slice. Jumlah pasangan prasyarat→task pada keempat grafik slice: **15** (K1: 6, K2: 3, K3: 2, K4: 4), sama dengan isi kolom `Dependency`.

| Label | Asal |
|---|---|
| `[BE]` | Task backend sub-modul ini pada `backend-roadmap-finishing.md` |
| `[IB]` | Task backend sub-modul `integrasi-billing` pada `../../integrasi-billing/roadmap/backend-roadmap-finishing.md` |
| `[DOK]` | Task frontend sub-modul `dokter-rawat-inap` pada `../../dokter-rawat-inap/roadmap/frontend-roadmap-finishing.md` |

Seluruh node berlabel adalah cermin baca-saja.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `FE-RWI-180` |
| 1 | `BE-RWI-156` [IB] | `FE-RWI-185` |
| 1 | `BE-RWI-167` [BE] | `FE-RWI-187` |
| 1 | `BE-RWI-168` [BE] | `FE-RWI-188` |
| 1 | `BE-RWI-169` [BE] | `FE-RWI-189` |
| 1 | `BE-RWI-170` [BE], `FE-RWI-174` [DOK] | `FE-RWI-190` |
| 1 | `BE-RWI-171` [BE] | `FE-RWI-191` |
| 2 | `FE-RWI-180` | `FE-RWI-181` |
| 2 | `FE-RWI-180`, `BE-RWI-165` [BE] | `FE-RWI-183` |
| 2 | `FE-RWI-180`, `BE-RWI-166` [BE] | `FE-RWI-184` |
| 2 | `FE-RWI-185`, `BE-RWI-159` [IB] | `FE-RWI-186` |
| 3 | `FE-RWI-181` | `FE-RWI-182` |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 23.20.** `MVP-0` (`RWF-W2` awal): `FE-RWI-180`, `181`, `182`. `MVP-1` (`RWF-W2`): `FE-RWI-183`, `184`, `185`. `MVP-2` (`RWF-W4`): `FE-RWI-187`, `188`. `MVP-3` (`RWF-W7`): `FE-RWI-189`, `190`. `MVP-4`: `FE-RWI-191`. `FE-RWI-186` ikut `integrasi-billing` `MVP-4` (`RWF-W7`).

**Catatan rilis.** `FE-RWI-180` membangun wadah enam sub-menu. Wadah yang isinya dibangun task lain — WSD (`FE-RWI-183`), Daftar Pemberian Obat (`FE-RWI-181`), Diet Medis (`FE-RWI-184`), dan Catatan Pra-Operasi (`episode-rawat-inap` `FE-RWI-195`) — menampilkan keterangan "belum tersedia" tanpa request jaringan sampai pengisinya rilis. `FE-RWI-182` sengaja menunggu `FE-RWI-181` supaya MAR dan Obat Bawaan tidak pernah kehilangan jalan masuk.

## Slice K1 — Catatan Keperawatan, DPO, Obat & Alkes, WSD, Diet

### Grafik dependency slice K1

```text
FE-RWI-180 ─┬─> FE-RWI-181 ─> FE-RWI-182
            │
            ├──────────────────────┬─> FE-RWI-183
            │                      │
            │    BE-RWI-165 [BE] ──┘
            │
            └──────────────────────┬─> FE-RWI-184
                                   │
                 BE-RWI-166 [BE] ──┘
```

Pasangan: 6.

### Tabel task slice K1

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-180` | Catatan Keperawatan berisi enam sub-menu V1; narasi tetap CPPT | `FR-RWF-050`, `051`, `056`; `RWI-DEC-172` | `0.6.0` FE 11.1 `FE-KEP-24`; API 8.1 | `nursing-narrative-tab.jsx` (diganti), layar cairan dan sliding scale | Wadah enam sub-menu; dua jendela; narasi dengan saringan | — | Kartu | Kartu | Wadah kosong terlihat sampai pengisi rilis / Muhammad Hamzah | Kartu |
| `FE-RWI-181` | Daftar Pemberian Obat empat bagian termasuk Efek Samping | `FR-RWF-052`, `053`; `RWI-DEC-172` | FE 11.1 `FE-KEP-26`; API 8.1 | MAR dan rekonsiliasi yang ada; endpoint ADR | Pindahkan MAR; bagian Efek Samping baru | `FE-RWI-180` | Kartu | Kartu | — / Muhammad Hamzah | Kartu |
| `FE-RWI-182` | Obat & Alkes kembali ke empat sub-tab V1 | `FR-RWF-057`, `067`; `RWI-DEC-172` (3) | FE 11.1 `FE-KEP-34` | `nursing-medication-section.jsx` | Empat sub-tab; riwayat BMHP ke Summary | `FE-RWI-181` | Kartu | Kartu | Jalan masuk MAR / Muhammad Hamzah | Kartu |
| `FE-RWI-183` | Observasi WSD per selang | `FR-RWF-054`, `058`; `RWI-DEC-200`; `UAT-RWF-09`, `22` | FE 11.1 `FE-KEP-25`; API 8.4 | Wadah `FE-RWI-180` | Daftar selang, pembacaan, riwayat, koreksi terakhir | `FE-RWI-180`, `BE-RWI-165` [BE] | Kartu | Kartu | — / Muhammad Hamzah | Kartu |
| `FE-RWI-184` | Diet Medis atas instruksi dokter | `FR-RWF-055`; `RWI-DEC-178`, `188`; `UAT-RWF-26` | FE 11.1 `FE-KEP-27`; API 8.8 | Wadah `FE-RWI-180`; riwayat diet Gizi | Formulir tetapkan, ganti, hentikan; riwayat | `FE-RWI-180`, `BE-RWI-166` [BE] | Kartu | Kartu | — / Muhammad Hamzah | Kartu |

## Slice K2 — Tagihan Pasien

### Grafik dependency slice K2

```text
BE-RWI-156 [IB] ─> FE-RWI-185 ─┬─> FE-RWI-186
                               │
BE-RWI-159 [IB] ───────────────┘
```

Pasangan: 3.

### Tabel task slice K2

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-185` | Tagihan Pasien per kelompok V1 tanpa rupiah; subtotal hanya bagi pemegang izin rupiah | `FR-RWF-020` s.d. `025`; `RWI-DEC-170`; `UAT-RWF-10` | FE 11.1 `FE-KEP-23`, 11.5; `integrasi-billing` API 3.7 | `nursing-billing-section.jsx` (diganti) | Rincian per kelompok; `/amounts` bersyarat permission | `BE-RWI-156` [IB] | Kartu | Kartu | Kebocoran rupiah / Muhammad Hamzah | Kartu |
| `FE-RWI-186` | Baris operasi kunjungan asal tampil di kelompok Operasi | `RWI-DEC-207`; `RWI-AC-330`; `UAT-RWF-35` | FE 11.7; `integrasi-billing` API 3.11 | Layar `FE-RWI-185` | Label kunjungan asal pada baris tertaut | `FE-RWI-185`, `BE-RWI-159` [IB] | Kartu | Kartu | Satu kwitansi di luar task ini (G-27) / Muhammad Hamzah | Kartu |

## Slice K3 — Master dan Pemakaian Alat

### Grafik dependency slice K3

```text
BE-RWI-167 [BE] ─> FE-RWI-187

BE-RWI-168 [BE] ─> FE-RWI-188
```

Pasangan: 2.

### Tabel task slice K3

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-187` | Master Alat Medis; formulir master tarif menerima jenis alat dan komponen operasi | `FR-RWF-060`, `061`, `047`; `RWI-DEC-179`, `180`, `193`, `196` | FE 11.1, 11.2 `FE-KEP-32`; API 8.2; `episode-rawat-inap` API 11.9 | `tariff-form-view.jsx` | Butir menu baru; daftar dan formulir; empat isian baru formulir tarif | `BE-RWI-167` [BE] | Kartu | Kartu | Isian tarif tidak disebut kontrak frontend (gap dokumen) / Muhammad Hamzah | Kartu |
| `FE-RWI-188` | Pemakaian Alat: Order dan History tanpa rupiah | `FR-RWF-062` s.d. `066`, `069`; `RWI-DEC-179`, `219`; `UAT-RWF-06`, `27` | FE 11.1 `FE-KEP-28`; API 8.3 | Menu Pemakaian Alat (*placeholder*) | Dua tab; mulai, selesai, batal, koreksi waktu | `BE-RWI-168` [BE] | Kartu | Kartu | — / Muhammad Hamzah | Kartu |

## Slice K4 — Surveilans PPI dan Transfusi

### Grafik dependency slice K4

```text
BE-RWI-169 [BE] ─> FE-RWI-189

BE-RWI-170 [BE] ──┬─> FE-RWI-190
                  │
FE-RWI-174 [DOK] ─┘

BE-RWI-171 [BE] ─> FE-RWI-191
```

Pasangan: 4.

### Tabel task slice K4

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-189` | Surveilans per pasien dan daftar surveilans PPI | `FR-RWF-083`, `084`, `091`, `092`; `RWI-DEC-202`, `211`, `216`; `UAT-RWF-18`, `28`, `34` | FE 11.1, 11.7 `FE-KEP-29`, `30`; API 8.5 | Daftar Pantau `FE-INP-09` | Sub-tab bersyarat; isian harian; daftar PPI; tanda dicurigai | `BE-RWI-169` [BE] | Kartu | Kartu | Gerbang produksi G-21 / Muhammad Hamzah | Kartu |
| `FE-RWI-190` | Monitoring transfusi per kantong di tab Bank Darah | `FR-RWF-085`, `093`; `RWI-DEC-203`, `212`; `UAT-RWF-19`, `29`, `34` | FE 11.1, 11.7 `FE-KEP-31`; API 8.6 | Tab Bank Darah dari `FE-RWI-174` | Pilih kantong, titik ukur, hentikan, selesai, reaksi | `BE-RWI-170` [BE], `FE-RWI-174` [DOK] | Kartu | Kartu | Gerbang produksi G-21, G-22 / Muhammad Hamzah | Kartu |
| `FE-RWI-191` | Kotak masuk Reaksi Transfusi di Bank Darah | `FR-RWF-085`; `RWI-DEC-203`, `209`; `UAT-RWF-19` | FE 11.1, 11.2 `FE-KEP-33`; API 8.7 | — | Butir menu baru; daftar, detail, tindak lanjut | `BE-RWI-171` [BE] | Kartu | Kartu | Modul Bank Darah milik Sukma Giri Pratama | Kartu |

## Kartu task

### `FE-RWI-180` — Catatan Keperawatan enam sub-menu

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Sub-tab Catatan Keperawatan berisi enam sub-menu V1 dengan urutan Spooling Cairan, Observasi Pengeluaran Cairan WSD, Sliding Scale, Daftar Pemberian Obat, Catatan Pra-Operasi, Diet Medis. Narasi perawat tetap entri CPPT yang dibaca dengan saringan Naratif Keperawatan |
| **Requirement/decision** | `FR-RWF-050`, `051`, `056`; `RWI-DEC-172`; `AC-RWF-050`, `051` |
| **Kontrak** | Frontend 11.1 (`FE-KEP-24`); API 8.1 (`fluid-balance-entries`, `patient-integrated-progress-notes?noteKind=NursingNarrative`) |
| **Reuse** | `nursing-narrative-tab.jsx` (diganti); layar cairan Pengawasan Harian; pelaksanaan sliding scale yang sudah ada |
| **Cakupan** | Wadah enam sub-menu. Spooling Cairan dan Sliding Scale membuka layar yang sudah ada. WSD, Daftar Pemberian Obat, Diet Medis, dan Catatan Pra-Operasi adalah wadah untuk `FE-RWI-183`, `181`, `184`, dan `episode-rawat-inap` `FE-RWI-195`; selama pengisinya belum ada, wadah menampilkan "belum tersedia" tanpa request jaringan. Narasi lewat Catatan Terintegrasi dengan saringan `noteKind` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Enam sub-menu tampil dengan urutan V1. 2. Data cairan yang dicatat lewat Spooling Cairan tampil sama di Pengawasan Harian (`AC-RWF-051`). 3. Narasi perawat tampil dengan saringan Naratif Keperawatan, tanpa tabel baru. 4. Wadah yang isinya belum ada tidak mengirim request jaringan |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Wadah kosong terlihat pengguna sampai pengisinya rilis; sebaiknya `FE-RWI-180`, `181`, `183`, `184` dirilis berdekatan dalam `RWF-W2`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-180.md` memuat `AUTOMATED TEST` dan `MANUAL TEST`; roadmap dan traceability diperbarui |

### `FE-RWI-181` — Daftar Pemberian Obat empat bagian

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Sub-menu Daftar Pemberian Obat memuat Pemberian Obat (MAR), Riwayat Pemberian, Efek Samping, dan Rekonsiliasi Obat. Efek samping tercatat lewat endpoint ADR dan tampil di riwayat alergi atau reaksi pasien |
| **Requirement/decision** | `FR-RWF-052`, `FR-RWF-053`; `RWI-DEC-172`; `AC-RWF-053`, `054` |
| **Kontrak** | Frontend 11.1 (`FE-KEP-26`); API 8.1 (`POST patient-allergies/from-medication-administration`) |
| **Reuse** | MAR, riwayat, dan rekonsiliasi yang sudah ada (dipindah, bukan disalin); endpoint ADR |
| **Cakupan** | Pindahkan MAR, riwayat, rekonsiliasi, dan Obat Bawaan ke sub-menu ini; bagian Efek Samping baru |
| **Dependency** | `FE-RWI-180` |
| **Acceptance criteria** | 1. Empat bagian tampil. 2. Efek samping dari satu pemberian obat tersimpan dan muncul di riwayat alergi atau reaksi pasien. 3. Data MAR sama seperti sebelum dipindah; tidak ada salinan data |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-182` — Obat & Alkes empat sub-tab

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Obat & Alkes kembali ke empat sub-tab V1 — Resep, Resep Harian, Alat Kesehatan, Summary. MAR, Sliding Scale, dan Obat Bawaan tidak lagi di sini; riwayat BMHP digabung ke Summary |
| **Requirement/decision** | `FR-RWF-057`, `FR-RWF-067`; `RWI-DEC-172` butir 3 |
| **Kontrak** | Frontend 11.1 (`FE-KEP-34`) |
| **Reuse** | `nursing-medication-section.jsx`; panel `order-alat-kesehatan-panel.jsx`, `summary-alat-kesehatan-panel.jsx`, `device-usage-panel.jsx` |
| **Cakupan** | Susun ulang menjadi empat sub-tab; cabut MAR, Sliding Scale, dan Obat Bawaan dari sini; riwayat BMHP ke Summary |
| **Dependency** | `FE-RWI-181` |
| **Acceptance criteria** | 1. Empat sub-tab sesuai urutan V1. 2. MAR, Sliding Scale, dan Obat Bawaan dapat dibuka dari Catatan Keperawatan dan tidak lagi dari Obat & Alkes. 3. Riwayat BMHP tampil di Summary. 4. Alkes kecil dan BMHP tetap di Alat Kesehatan |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Bila dirilis sebelum `FE-RWI-181`, MAR kehilangan jalan masuk — karena itu dependency-nya. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-183` — Observasi WSD per selang

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Perawat mendaftarkan selang WSD dan mencatat pembacaan per shift. Layar menampilkan sisa shift lalu, volume bertambah dari server, dan riwayat; koreksi hanya untuk pembacaan terakhir |
| **Requirement/decision** | `FR-RWF-054`, `FR-RWF-058`; `RWI-DEC-200`; `AC-RWF-052`, `057`, `058`; `UAT-RWF-09`, `UAT-RWF-22` |
| **Kontrak** | Frontend 11.1 (`FE-KEP-25`); API 8.4 |
| **Reuse** | Wadah dari `FE-RWI-180` |
| **Cakupan** | Daftar selang, daftarkan, koreksi, lepas; catat pembacaan; riwayat; koreksi dan batal pembacaan terakhir; pesan `CLI-WSD-001` s.d. `003` |
| **Dependency** | `FE-RWI-180`, `BE-RWI-165` [BE] |
| **Acceptance criteria** | 1. Contoh `UAT-RWF-09` menampilkan bertambah 250 ml dari server, dan entri itu juga tampil di Spooling Cairan. 2. Dua selang tampil terpisah (`UAT-RWF-22`). 3. Pembacaan sesudah selang dilepas → pesan `CLI-WSD-002`. 4. Tombol koreksi hanya pada pembacaan terakhir; 409 → muat ulang. 5. Layar tidak menghitung volume sendiri |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-184` — Diet Medis

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Perawat, dokter, dan ahli gizi menetapkan, mengganti, atau menghentikan diet dari Catatan Keperawatan; perawat wajib memilih dokter pemberi instruksi; riwayat dibaca dari modul Gizi |
| **Requirement/decision** | `FR-RWF-055`; `RWI-DEC-178`, `RWI-DEC-188`; `AC-RWF-055`; `UAT-RWF-26` |
| **Kontrak** | Frontend 11.1 (`FE-KEP-27`); API 8.8; riwayat `GET nutrition-management/diets/history/{encounterId}` (`NutritionPatientDiet : Read`, sudah ada) |
| **Reuse** | Wadah dari `FE-RWI-180` |
| **Cakupan** | Formulir diet dengan pilihan dokter pemberi instruksi (hanya dokter berpenugasan aktif, tidak tampil bila pengguna dokter); hentikan diet dengan alasan; riwayat dan status verifikasi; `IdempotencyKey` |
| **Dependency** | `FE-RWI-180`, `BE-RWI-166` [BE] |
| **Acceptance criteria** | 1. Perawat tanpa dokter → pesan "Dokter pemberi instruksi wajib dipilih" (`UAT-RWF-26`). 2. Dengan dokter → tersimpan dengan status "menunggu verifikasi". 3. Dokter tidak berpenugasan → pesan 403. 4. Ganti diet aktif tanpa alasan → pesan `GIZ010`. 5. Riwayat tampil dari Gizi. 6. Klik ganda tidak membuat diet ganda |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-185` — Tagihan Pasien per kelompok

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Menu Tagihan Pasien menampilkan rincian per kelompok V1 tanpa rupiah. Pemegang izin rupiah melihat subtotal per kelompok dan total berjalan, tidak pernah harga per item. Tagihan yang belum terbentuk tampil apa adanya |
| **Requirement/decision** | `FR-RWF-020` s.d. `025`; `RWI-DEC-170`; `AC-RWF-020` s.d. `023`; `UAT-RWF-10` |
| **Kontrak** | Frontend 11.1 (`FE-KEP-23`), 11.5; `integrasi-billing` API 3.7 (`breakdown`, `breakdown/amounts`) |
| **Reuse** | Menggantikan `sections/billing/nursing-billing-section.jsx` yang membaca folio berupiah |
| **Cakupan** | Panggil `breakdown`; panggil `breakdown/amounts` hanya bila daftar permission memuat `PatientBillingSummary : ViewAmount`; `NOT_FORMED` → "Tagihan belum terbentuk"; `TARIFF_NOT_FOUND` → "tarif belum ada"; kelompok tanpa baris tidak tampil |
| **Dependency** | `BE-RWI-156` [IB] |
| **Acceptance criteria** | 1. Perawat melihat kelompok dan baris tanpa rupiah, dan tidak ada request ke `/amounts` (`UAT-RWF-10`). 2. Petugas admisi pemegang `ViewAmount` melihat subtotal dan total tanpa harga per item. 3. Invoice belum ada → "Tagihan belum terbentuk", bukan Rp 0. 4. Kelompok tanpa baris tidak tampil. 5. Pencarian kode: layar tidak lagi membaca folio berupiah |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan akun perawat dan akun admisi |
| **Risiko/pemilik** | Kebocoran rupiah bila syarat permission salah; server tetap menyaring. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-186` — Baris operasi kunjungan asal di Tagihan Pasien

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Kelompok Operasi menampilkan baris operasi kunjungan asal yang tertaut ke invoice `RANAP`, berlabel asalnya, tanpa rupiah |
| **Requirement/decision** | `RWI-DEC-207`; `RWI-AC-330` (tampilan); `UAT-RWF-35` |
| **Kontrak** | Frontend 11.7; `integrasi-billing` API 3.11 (`LinkedEncounter`, `LinkedEncounters`, `IncludesLinkedEncounter`) |
| **Reuse** | Layar dari `FE-RWI-185` |
| **Cakupan** | Label "dari kunjungan …" pada baris tertaut; subtotal bagi pemegang izin rupiah mengikuti `IncludesLinkedEncounter` |
| **Dependency** | `FE-RWI-185`, `BE-RWI-159` [IB] |
| **Acceptance criteria** | 1. Pasien dari Poli Bedah yang dioperasi lalu dirawat → baris berlabel "dari kunjungan Poli Bedah" tanpa rupiah (`UAT-RWF-35`). 2. Pemegang izin rupiah melihat subtotal yang menyatakan apakah kunjungan tertaut termasuk. 3. Pasien tanpa tautan → tampilan sama dengan `FE-RWI-185` |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | "Satu transaksi, satu kwitansi" di luar task ini (gap G-27, `billing-kasir`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-187` — Master Alat Medis dan empat isian baru formulir master tarif

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Admin Master Data mengelola jenis alat medis lewat butir menu Master Data → Alat Medis. Formulir master tarif menerima jenis alat serta komponen operasi (jenis komponen, dasar tagih, pembulatan), sehingga tarif alat, jasa anestesi, dan sewa kamar operasi dapat diisi |
| **Requirement/decision** | `FR-RWF-060`, `FR-RWF-061`, `FR-RWF-047` (tarif komponen operasi); `RWI-DEC-179`, `180`, `193`, `196`; `AC-RWF-060` |
| **Kontrak** | Frontend 11.1, 11.2 (`FE-KEP-32`, butir menu `/health-services/master-data/medical-equipments`, `MedicalEquipment : Read`); API 8.2; `episode-rawat-inap` API 11.9 (tiga isian komponen operasi, `Tariff : Update`); kamus data 12.14 |
| **Reuse** | Pola layar master data yang ada; `master-data/tariff/add/tariff-form-view.jsx` |
| **Cakupan** | Butir menu baru; daftar dengan saringan aktif, formulir tambah dan ubah, aktif/nonaktif; pesan `MST-EQP-001`, `002`. Formulir master tarif mendapat empat isian opsional yang dibuat `BE-RWI-172`: "Jenis alat", "Komponen operasi" (`None`, `AnesthesiaService`, `OperatingRoomRent`), "Dasar tagih" (`PerService`, `PerHour`), dan pembulatan |
| **Dependency** | `BE-RWI-167` [BE] |
| **Acceptance criteria** | 1. Butir menu tampil bagi pemegang `MedicalEquipment : Read`. 2. Tambah, ubah, dan nonaktif berfungsi; kode ganda → pesan `MST-EQP-001`. 3. Satuan tagih wajib dipilih. 4. Tarif per kelas dapat dibuat dengan jenis alat. 5. Tarif sewa kamar operasi per jam dan jasa anestesi per layanan dapat dibuat per kelas. 6. Formulir tarif tindakan dan obat tanpa isian baru tetap berperilaku sama |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Keempat isian formulir tarif tidak disebut kontrak frontend mana pun (gap dokumen TRC-RWF-02 di traceability), tetapi dibutuhkan DoD "master alat dan tarifnya terisi" dan data awal tarif komponen operasi (`episode-rawat-inap` 12.12). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-188` — Pemakaian Alat: Order dan History

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Menu Pemakaian Alat keluar dari *placeholder* dengan tab Order Alat Kesehatan dan History Alat Kesehatan. Perawat memulai, menyelesaikan, membatalkan, dan mengoreksi waktu pemakaian; layar tidak menampilkan rupiah |
| **Requirement/decision** | `FR-RWF-062` s.d. `066`, `FR-RWF-069`; `RWI-DEC-179`, `RWI-DEC-219`; `AC-RWF-062` s.d. `064`; `UAT-RWF-06`, `UAT-RWF-27` |
| **Kontrak** | Frontend 11.1 (`FE-KEP-28`); API 8.3 |
| **Reuse** | Menu Pemakaian Alat di `nursing-workspace-sections.jsx` (saat ini *placeholder*) |
| **Cakupan** | Dua tab; formulir mulai (alat aktif, dokter penanggung jawab berpenugasan, waktu mulai, jumlah bila per pemakaian); selesai, batal beralasan, koreksi waktu; tanda "perlu diperiksa perawat"; `ChargeState` sebagai teks |
| **Dependency** | `BE-RWI-168` [BE] |
| **Acceptance criteria** | 1. Dua tab seperti V1. 2. Alat bersatuan waktu memakai isian mulai dan selesai; alat per pemakaian memakai jumlah; unit tagih tampil dari server. 3. Dokter penanggung jawab hanya dari dokter berpenugasan aktif; perawat pelaksana dari akun login. 4. Batal saat invoice final → pesan `CLI-EQP-002` (`UAT-RWF-27`). 5. Pemakaian yang ditutup otomatis saat keluar ruangan bertanda "perlu diperiksa perawat". 6. `TARIFF_NOT_FOUND` tampil "tarif belum ada"; tidak ada rupiah maupun perkiraan harga (`RWI-DEC-219`). 7. 409 → muat ulang |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-189` — Surveilans infeksi luka operasi dan daftar PPI

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Sub-tab surveilans tampil di Asuhan Keperawatan hanya bila pasien punya kasus OK `Completed`; perawat mengisi harian; tim PPI melihat daftar surveilans di Daftar Pantau dan menandai "dicurigai" |
| **Requirement/decision** | `FR-RWF-083`, `084`, `091`, `092`; `RWI-DEC-202`, `211`, `216`; `AC-RWF-083`, `095`, `096`; `UAT-RWF-18`, `28`, `34` |
| **Kontrak** | Frontend 11.1, 11.7 (`FE-KEP-29`, `FE-KEP-30`); API 8.5; `GET nosocomial-infections/{id}` (sudah ada) |
| **Reuse** | Daftar Pantau `FE-INP-09` |
| **Cakupan** | Sub-tab bersyarat; isian hari ke-N sesuai definisi versi formulir, kolom suhu baca-saja; kultur dan serologi; koreksi beralasan; daftar PPI di akhir kelompok keperawatan Daftar Pantau sesudah `FE-KEP-22`; tombol "Tandai dicurigai" |
| **Dependency** | `BE-RWI-169` [BE] |
| **Acceptance criteria** | 1. Sub-tab hanya tampil pada pasien dengan kasus `Completed` (`UAT-RWF-34`). 2. Kolom suhu terbaca dari tanda vital dan tidak dapat diketik. 3. Formulir yang berhenti tetap terbaca; isian baru → pesan `CLI-SSI-001`. 4. Daftar PPI berada di akhir kelompok keperawatan dan menampilkan peringatan bila formulir belum disahkan (`UAT-RWF-28`). 5. Tombol "Tandai dicurigai" hanya bagi `SurgicalSiteSurveillance : Review` dan membuka kejadian nosokomial yang dibuat. 6. Koreksi wajib alasan; 409 → muat ulang |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan pasien ber-kasus OK `Completed` dan pasien tanpa operasi |
| **Risiko/pemilik** | Gerbang produksi G-21. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-190` — Monitoring transfusi di tab Bank Darah

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Dari tab Bank Darah menu Penunjang Medis, perawat memilih kantong yang sudah diserahkan, mencatat empat titik ukur, menghentikan atau menyelesaikan transfusi, dan mencatat reaksi |
| **Requirement/decision** | `FR-RWF-085`, `FR-RWF-093`; `RWI-DEC-203`, `RWI-DEC-212`; `AC-RWF-084`, `097`, `098`; `UAT-RWF-19`, `29`, `34` |
| **Kontrak** | Frontend 11.1, 11.7 (`FE-KEP-31`); API 8.6 |
| **Reuse** | Tab Bank Darah di menu Penunjang Medis perawat yang dipasang `FE-RWI-174` |
| **Cakupan** | Daftar kantong yang dapat dipilih; mulai monitoring; empat titik ukur dengan jatuh tempo dari server dan keterangan terlambat; reaksi; hentikan, selesai, batal; status pemberitahuan reaksi |
| **Dependency** | `BE-RWI-170` [BE], `FE-RWI-174` [DOK] |
| **Acceptance criteria** | 1. Hanya kantong milik pasien yang tampil; nomor kantong tidak dapat diketik (`UAT-RWF-29`). 2. Jatuh tempo titik dari server; titik terlambat wajib keterangan (pesan `CLI-TRF-003`). 3. Reaksi tersimpan dan status pemberitahuannya tampil. 4. Titik sesudah dihentikan ditolak dengan pesan. 5. 409 → muat ulang |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan kantong uji yang sudah diserahkan |
| **Risiko/pemilik** | Gerbang produksi G-21 dan G-22. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-191` — Kotak masuk Reaksi Transfusi

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Petugas Bank Darah membuka kotak masuk reaksi transfusi lewat butir menu Bank Darah → Reaksi Transfusi dan menyatakan sudah menindaklanjuti |
| **Requirement/decision** | `FR-RWF-085`; `RWI-DEC-203`, `RWI-DEC-209`; `UAT-RWF-19` |
| **Kontrak** | Frontend 11.1, 11.2 (`FE-KEP-33`, butir menu `/health-services/blood-bank-management/transfusion-reaction-notices`); API 8.7 |
| **Reuse** | Pola daftar modul Bank Darah yang ada |
| **Cakupan** | Butir menu baru; daftar dengan saringan status dan periode; detail; tombol tindak lanjut dengan catatan |
| **Dependency** | `BE-RWI-171` [BE] |
| **Acceptance criteria** | 1. Butir menu tampil bagi pemegang `TransfusionReactionNotice : Read`. 2. Daftar dan saringan berfungsi; kosong → pesan kosong. 3. Detail memuat pasien, kantong, reaksi, waktu, dan unit. 4. Tombol tindak lanjut hanya bagi `TransfusionReactionNotice : Acknowledge` |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual, disaksikan petugas Bank Darah bila tersedia |
| **Risiko/pemilik** | Modul Bank Darah milik Sukma Giri Pratama (persetujuan `RWI-DEC-209`) |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |
