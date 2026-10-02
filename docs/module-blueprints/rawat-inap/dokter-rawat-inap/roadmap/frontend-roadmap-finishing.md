# Roadmap Frontend — Dokter Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `dokter-rawat-inap/roadmap/frontend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `dokter-rawat-inap`, kontrak **`0.7.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-frontend` sebelum approval itu tercatat |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `03-frontend-architecture.md` bagian 11 (`b0de9bd5…`), `contracts/api-contract.md` bagian 13 (`bab898df…`), `testing/acceptance-test-matrix.md` bagian 15 (`e7c6da47…`); untuk laci Pasca Operasi `episode-rawat-inap` `03-frontend-architecture.md` 13.4.4 (`e31154bd…`). Hash lengkap pada `../blueprint-manifest.md` bagian 10 |
| Keputusan | `RWI-DEC-108`, `114`, `153`, `165`, `168`, `171`, `188`, `213`, `218`, `219`, `221`; gate `1.10` dan koreksi 19.12 (`DEC-INP-019`, IMP-RWF-05) |
| Source SHA | Frontend `f74758af5`; backend `bf5c6bde` |
| Deret ID | `FE-RWI-172` s.d. `FE-RWI-179` |
| Roadmap pendamping | `backend-roadmap-finishing.md`, `requirement-traceability-finishing.md`. `frontend-roadmap-v2.md` tetap berlaku untuk task lamanya; `FE-RWI-144` s.d. `FE-RWI-146` diperbaiki oleh task di sini (bagian akhir) |

**Kebijakan verifikasi frontend.** Mengikuti `rules/frontend/test-policy.md`: bukti utama verifikasi manual kontrol interaktif dengan backend berjalan, ditambah `npm run lint`, `npm run test:unit` (suite yang ada), dan `npm run build`. Test unit baru opsional, disarankan untuk logika murni seperti pemetaan `PriceStatus` ke teks. Jest dan `@testing-library` tidak dipakai. Laporan task memuat baris `AUTOMATED TEST` dan `MANUAL TEST` terpisah.

**Kewenangan UI.** Rupa tab, urutan bagian daftar gabungan, bentuk penanda dan laci, serta ikon tetap `DEV_DISCRETION` (`03-frontend-architecture.md` 11.4; `RWI-FE-006`). Yang mengikat: harga selalu berlabel perkiraan dan tidak pernah menahan tombol Kirim; tidak ada data contoh atau ID buatan (`RWI-DEC-108`); Tagihan Pasien tetap tanpa rupiah bagi perawat.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

Roadmap ini memuat 8 task dan 9 prasyarat dari roadmap lain, melewati batas 15 node satu grafik. Karena itu grafik dipecah per slice di bawah judul slice masing-masing; grafik di bawah ini adalah ringkasan antar-slice.

```text
Slice D1 Penunjang Medis, perkiraan harga, dan verifikasi

Slice D2 Penanda Pasca operasi

Slice D3 Rehab Medik ⛔
```

Ketiga slice tidak saling menunggu. Seluruh prasyaratnya berasal dari roadmap lain dan digambar pada grafik slice. Jumlah pasangan prasyarat→task pada ketiga grafik slice: **11** (D1: 9, D2: 1, D3: 1), sama dengan isi kolom `Dependency`.

| Label | Asal |
|---|---|
| `[BE]` | Task backend sub-modul ini pada `backend-roadmap-finishing.md` |
| `[V2]` | Task backend sub-modul ini pada `backend-roadmap-v2.md` |
| `[KEP]` | Task backend sub-modul `keperawatan` pada `../../keperawatan/roadmap/backend-roadmap-finishing.md` |
| `[EPS]` | Task frontend sub-modul `episode-rawat-inap` pada `../../episode-rawat-inap/roadmap/frontend-roadmap-finishing.md` |

Seluruh node berlabel adalah cermin baca-saja; tandanya disalin dari roadmap pemiliknya.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-163` [BE] | `FE-RWI-173` |
| 1 | `BE-RWI-164` [BE] | `FE-RWI-174` |
| 1 | `BE-RWI-161` [BE], `BE-RWI-162` [BE], `BE-RWI-166` [KEP] | `FE-RWI-175` |
| 1 | `BE-RWI-160` [BE], `BE-RWI-163` [BE] | `FE-RWI-176` |
| 1 | — | `FE-RWI-177` |
| 1 | `FE-RWI-196` [EPS] | `FE-RWI-178` |
| 2 | `FE-RWI-173`, `BE-RWI-104` ✅ [V2] | `FE-RWI-172` — juga menunggu bukti runtime `BE-RWI-104` (kartu) |
| — | ⛔ menunggu `DEC-INP-019` | `FE-RWI-179` |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 23.20.** `MVP-0` (`RWF-W0`): `FE-RWI-172`, `173`, `176`, `177`. `MVP-1` (`RWF-W2`): `FE-RWI-174`, `175`. `FE-RWI-178` ikut gelombang Pasca Operasi `episode-rawat-inap`. `FE-RWI-179` tidak bergelombang sampai `DEC-INP-019` dijawab.

## Gerbang rilis

Kode frontend `f74758af5` memuat lima cacat implementasi di ruang kerja dokter. Kode yang memuatnya **tidak boleh** dirilis ke pengguna sebelum task perbaikannya ✅.

| Temuan | Isi | Bukti | Diperbaiki oleh |
|---|---|---|---|
| IMP-RWF-01 | Gizi dan Bank Darah dikirim langsung ke modul tujuan dengan dokter peminta = dokter admisi | `supporting-nutrition-form.jsx:66-71`, `supporting-blood-bank-form.jsx:80-95` | `FE-RWI-174` |
| IMP-RWF-02 | `bloodComponentId` dan unit layanan berupa GUID tertanam | `supporting-blood-bank-form.jsx:80-95` | `FE-RWI-174` |
| IMP-RWF-03 | Rehab Medik memakai tujuh `procedureId` buatan | `supporting-rehab-form.jsx:20-60` | `FE-RWI-179` ⛔ |
| IMP-RWF-05 | Lab/Radiologi berharga tetap 120.000 dan memakai katalog contoh bila katalog kosong | `supporting-laboratory-form.jsx:27-40`, `:81-96`; `supporting-radiology-form.jsx:21-25`, `:103` | `FE-RWI-173` |
| IMP-RWF-06 (temuan perencanaan 2 Oktober) | Order tindakan rawat inap mengisi status tanggungan bawaan "Ditanggung" dan harga `0` karena `master-options` tidak mengirim tarif | `procedure-form-panel.jsx:102-103`, `:150`, `:360` | `FE-RWI-176` |

`FE-RWI-179` tertahan `DEC-INP-019`. Bila rilis harus berjalan sebelum keputusan itu dijawab, pemilik dapat memilih jawaban "tetap *placeholder*" lebih dulu; agent tidak memilih atas nama pemilik.

## Slice D1 — Penunjang Medis, perkiraan harga, dan verifikasi

### Grafik dependency slice D1

```text
BE-RWI-104 ✅ [V2] ──────────────┐
                                 │
BE-RWI-163 [BE] ─┬─> FE-RWI-173 ─┴─> FE-RWI-172
                 │
                 └──────────────────────┬─> FE-RWI-176
                                        │
                     BE-RWI-160 [BE] ───┘

BE-RWI-164 [BE] ─> FE-RWI-174

BE-RWI-161 [BE] ──┐
                  │
BE-RWI-162 [BE] ──┴──┬─> FE-RWI-175
                     │
BE-RWI-166 [KEP] ────┘

FE-RWI-177
```

Pasangan: 9. `FE-RWI-174` cukup menunggu `BE-RWI-164`, karena `BE-RWI-164` sudah menunggu `BE-RWI-163` (status tanggungan).

### Tabel task slice D1

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-172` | Perawat memesan Lab/Radiologi atas instruksi dokter, dengan tanggungan dan perkiraan harga | `FR-RWF-030`, `034`, `036`; `RWI-DEC-114`, `153`, `168`, `218`; `UAT-RWF-04`, `36` | `0.7.0` FE 11.1 `FE-DOK-18`; API 13.1, 13.2 | Form Lab/Rad hasil `FE-RWI-173`; `nursing-ancillary-section.jsx` | Lepas kunci tombol; isian dokter pemberi instruksi | `FE-RWI-173`, `BE-RWI-104` ✅ [V2] | Kartu | Kartu | Regresi `BE-RWI-104` `NOT RUN` / Muhammad Hamzah | Kartu |
| `FE-RWI-173` | Form Lab/Radiologi dokter tanpa harga tetap dan tanpa katalog contoh | `FR-RWF-033`, `034`; `RWI-DEC-108`, `218`, `219`; `RWI-AC-335`, `337`; IMP-RWF-05 | FE 11.4, 11.5 `FE-DOK-13`; API 13.2 | Form dan keranjang `FE-RWI-144` | Cabut katalog contoh dan harga 120.000; pakai `coverage-status` | `BE-RWI-163` [BE] | Kartu | Kartu | Gerbang rilis / Muhammad Hamzah | Kartu |
| `FE-RWI-174` | Konsultasi Gizi dan Bank Darah lewat adapter dari ruang kerja dokter dan perawat | `FR-RWF-031` s.d. `033`, `036` s.d. `038`; `RWI-DEC-171`, `188`, `219`; `UAT-RWF-07`, `08`, `30`; IMP-RWF-01, 02 | FE 11.3 `FE-DOK-17`; API 13.2 | Form `FE-RWI-145`; master komponen darah | Kirim lewat adapter; peminta benar; komponen dari master; pasang di menu perawat | `BE-RWI-164` [BE] | Kartu | Kartu | Gerbang rilis; hak `BloodComponent : Read` bagi peran bangsal / Muhammad Hamzah | Kartu |
| `FE-RWI-175` | Daftar "Perlu Diverifikasi" dokter memuat enam sumber | `FR-RWF-037`; `RWI-DEC-188`, `191`; `AC-RWF-035`; `NFR-RWF-11` | FE 11.3 `FE-DOK-15`; API 13.3, 13.4 | `physician-needs-review-view.jsx`, `use-physician-review-worklist.js` | Tambah sumber gizi, darah, diet; dimuat terpisah | `BE-RWI-161` [BE], `BE-RWI-162` [BE], `BE-RWI-166` [KEP] | Kartu | Kartu | Enam permission verifikasi / Muhammad Hamzah | Kartu |
| `FE-RWI-176` | Katalog tindakan rawat inap dengan perkiraan harga, tanpa data karangan | `FR-RWF-070`, `034`; `RWI-DEC-165` (1), `108`, `218`; `UAT-RWF-31`; IMP-RWF-06 | FE 11.3 `FE-DOK-19`, 11.5; API 13.5, 13.2 | `procedure-form-panel.jsx`, tiga hook tindakan rawat inap | Parameter katalog; harga dari `coverage-status` | `BE-RWI-160` [BE], `BE-RWI-163` [BE] | Kartu | Kartu | Gerbang rilis / Muhammad Hamzah | Kartu |
| `FE-RWI-177` | Harga obat di tab Resep berlabel perkiraan | `RWI-DEC-218`, `219`; `RWI-AC-337` | FE 11.4, 11.5 `FE-DOK-10`; API 13.2 (harga obat) | `prescription-regular-drug-form.jsx`, `prescribing-drug.service.js` | Label perkiraan pada seluruh formulir resep berharga | — | Kartu | Kartu | Hak lihat harga obat mengikuti hak baca / Muhammad Hamzah | Kartu |

## Slice D2 — Penanda Pasca operasi

### Grafik dependency slice D2

```text
FE-RWI-196 [EPS] ─> FE-RWI-178
```

Pasangan: 1. Endpoint ringkasan operasi (`BE-RWI-180` milik `episode-rawat-inap`) sudah terbawa lewat `FE-RWI-196`.

### Tabel task slice D2

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-178` | Dokter membuka ringkasan operasi dari penanda di Konteks pasien | `FR-RWF-081`, `082`; `RWI-DEC-213`; `RWI-AC-339`; `UAT-RWF-38` | FE 11.5 `FE-DOK-09`; `episode-rawat-inap` FE 13.4.4 | Laci `FE-INP-28` dari `FE-RWI-196` | Penanda per kasus OK `Completed`; laci baca-saja | `FE-RWI-196` [EPS] | Kartu | Kartu | Menunggu sub-modul lain / Muhammad Hamzah | Kartu |

## Slice D3 — Rehab Medik

### Grafik dependency slice D3

```text
{DEC-INP-019 ⛔} ─> FE-RWI-179 ⛔
```

Pasangan: 1 (keputusan `DEC-INP-019` dihitung sebagai satu entri).

### Tabel task slice D3

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-179` ⛔ | Kartu Rehab Medik tidak lagi mengirim pesanan ber-ID buatan; perilaku akhirnya mengikuti `DEC-INP-019` | `FR-RWF-035`; `RWI-DEC-108`; KK-3; `DEC-INP-019`; IMP-RWF-03 | `04-prd-to-mvp.md` 23.8 (Rehab ditunda) | `supporting-rehab-form.jsx` | Bergantung jawaban keputusan | `DEC-INP-019` ⛔ | Kartu | Kartu | Keputusan pemilik terbuka / Muhammad Hamzah | Kartu |

## Kartu task

### `FE-RWI-172` — Pesanan Lab dan Radiologi oleh perawat

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Perawat memesan Lab dan Radiologi dari menu Penunjang Medis ruang kerja perawat atas instruksi dokter berpenugasan, lengkap dengan status tanggungan dan perkiraan harga per pemeriksaan |
| **Requirement/decision** | `FR-RWF-030`, `034`, `036`; `RWI-DEC-114`, `153`, `168`, `218`; `AC-RWF-030`, `031`; `RWI-AC-337`; `UAT-RWF-04`, `UAT-RWF-36` |
| **Kontrak** | `0.7.0`: frontend 11.1, 11.2 (`FE-DOK-18`); API 13.1 (`POST lab-orders`, `POST rad-orders`), 13.2 (`coverage-status`); validasi `VAL-RWF-65` |
| **Reuse** | Form Lab/Radiologi yang sudah diperbaiki `FE-RWI-173`; `nursing-ancillary-section.jsx` dan `nursing-ancillary-order-table` (daftar pesanan dan hasil) |
| **Cakupan** | Lepas kunci `disabled={true}` (`nursing-ancillary-section.jsx:77`, `:188-196`); pasang form Lab/Radiologi dengan isian "Dokter pemberi instruksi" (hanya dokter berpenugasan aktif); kirim pesanan beserta instruksi; daftar pesanan menampilkan status verifikasi |
| **Dependency** | `FE-RWI-173`, `BE-RWI-104` ✅ [V2] |
| **Prasyarat bukti** | `BE-RWI-104` ✅ pada `backend-roadmap-v2.md`, tetapi laporannya mencatat **regresi Lab/Rad `NOT RUN`**, sedangkan `RWI-DEC-168` mensyaratkan "terbukti berjalan". Langkah pertama task ini: verifikasi runtime pesanan Lab perawat ber-instruksi sampai masuk worklist Laboratorium. Bila tidak dapat dijalankan, tombol tetap terkunci dan task berhenti sebagai 🟡 |
| **Acceptance criteria** | 1. Perawat memesan "Darah Lengkap" dengan dokter pemberi instruksi → pesanan masuk worklist Laboratorium (`AC-RWF-030`). 2. Tanpa dokter → ditolak dengan pesan modul Lab (`AC-RWF-031`, `UAT-RWF-04`). 3. Pilihan dokter hanya dokter berpenugasan aktif; kosong → "Belum ada dokter yang ditugaskan pada pasien ini." 4. Status tanggungan dan perkiraan harga tampil bagi pemegang `LabOrder : Create` (`UAT-RWF-36`). 5. Pesanan tampil pada daftar "Perlu Diverifikasi" dokter pemberi instruksi. 6. Klik ganda tidak membuat dua pesanan |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi runtime `BE-RWI-104`; verifikasi manual ujung ke ujung dengan modul Lab dan Radiologi sungguhan |
| **Risiko/pemilik** | Bila regresi `BE-RWI-104` gagal, task kembali ke backend. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan `../task/report/frontend/FE-RWI-172.md` memuat `AUTOMATED TEST`, `MANUAL TEST`, dan hasil runtime `BE-RWI-104`; roadmap dan traceability diperbarui |

### `FE-RWI-173` — Form Lab dan Radiologi dokter tanpa harga tetap dan tanpa katalog contoh

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Form Lab dan Radiologi di tab Penunjang Medis dokter hanya menampilkan pemeriksaan dari katalog modul pemiliknya, dengan status tanggungan dan perkiraan harga sesuai penjamin dan kelas. Tidak ada lagi harga tetap 120.000 dan katalog contoh |
| **Requirement/decision** | `FR-RWF-033`, `034`; `RWI-DEC-108`, `218`, `219`; `RWI-AC-335`, `337`; `UAT-RWF-36`, `37`; gerbang rilis IMP-RWF-05 |
| **Kontrak** | Frontend 11.4, 11.5 (`FE-DOK-13`); API 13.2 (`coverage-status`, `ItemType` `Laboratory`/`Radiology`, `ItemIds` = `ProcedureId`) |
| **Reuse** | Form dan keranjang hasil `FE-RWI-144` (`supporting-laboratory-form.jsx`, `supporting-radiology-form.jsx`) |
| **Cakupan** | Cabut `DEFAULT_LAB_EXAMS` (`supporting-laboratory-form.jsx:27-40`) dan `DEFAULT_RAD_PROCEDURES` (`supporting-radiology-form.jsx:21-25`, `:103`); cabut harga tetap (`:81-96`); katalog kosong atau gagal tampil apa adanya; status tanggungan dan perkiraan harga per item; `NOT_ESTIMABLE` → "tarif belum tersedia"; `NOT_PERMITTED` → tanpa harga. Bila keranjang menampilkan total, total berlabel perkiraan dan menyebut jumlah pemeriksaan tanpa tarif |
| **Dependency** | `BE-RWI-163` [BE] |
| **Acceptance criteria** | 1. Pencarian kode: nol `DEFAULT_LAB_EXAMS`, `DEFAULT_RAD_PROCEDURES`, dan harga 120000 pada kedua form. 2. Katalog kosong → keadaan kosong tanpa data contoh. 3. Katalog gagal → pesan gagal dan "Coba lagi". 4. "Darah Lengkap" untuk pasien BPJS kelas 2 → "Ditanggung" beserta perkiraan harga berlabel "perkiraan — tagihan final di kasir". 5. Tarif tidak ada → "tarif belum tersedia" dan tombol Kirim tetap aktif (`RWI-AC-335`). 6. `coverage-status` gagal → "Status tanggungan tidak dapat dibaca", pesanan tetap boleh dikirim |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan katalog berisi, kosong, dan gagal, memakai akun pemegang dan bukan pemegang `LabOrder : Create`. Test unit pemetaan `PriceStatus`: opsional |
| **Risiko/pemilik** | Gerbang rilis IMP-RWF-05. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-174` — Konsultasi Gizi dan Bank Darah lewat adapter Rawat Inap

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Dokter dan perawat memesan konsultasi gizi dan darah dari bangsal lewat adapter Rawat Inap. Dokter peminta diambil dari akun login bila yang memesan dokter, atau dipilih dari dokter berpenugasan bila yang memesan perawat. Komponen darah dipilih dari master |
| **Requirement/decision** | `FR-RWF-031`, `032`, `033`, `036`, `037`, `038`; `RWI-DEC-171`, `188`, `219`; `AC-RWF-032` s.d. `034`; `UAT-RWF-07`, `08`, `30`; gerbang rilis IMP-RWF-01, IMP-RWF-02 |
| **Kontrak** | Frontend 11.1, 11.3 (`FE-DOK-17`, komponen bersama dua ruang kerja); API 13.2 |
| **Reuse** | Form hasil `FE-RWI-145` (`supporting-nutrition-form.jsx`, `supporting-blood-bank-form.jsx`); `GET master-data/blood-components/options` (`BloodComponent : Read`, sudah ada); daftar baca `nutrition-order.service.js`, `blood-order.service.js` |
| **Cakupan** | 1. Kirim lewat `POST …/ancillary-orders/nutrition-consultations`, `…/blood-orders`, dan `…/blood-orders/confirm-duplicate`, bukan langsung ke modul tujuan (`use-inpatient-supporting-service.jsx`). 2. Cabut `requestingDoctorId: episode?.admissionDoctorId` (`supporting-blood-bank-form.jsx:80-95`, `supporting-nutrition-form.jsx:66-71`). 3. Cabut `bloodComponentId` dan unit layanan bertanam GUID; komponen dari master. 4. Penginput tampil dari akun login. 5. Status tanggungan dari `coverage-status` (`Nutrition`, `Blood`) → "tarif belum tersedia". 6. Komponen bersama yang sama dipasang di menu Penunjang Medis perawat; "Integrasi belum tersedia" untuk Gizi dan Bank Darah dicabut (`nursing-ancillary-section.jsx:72-75`, `:130`). 7. Daftar pesanan menampilkan status pesanan, status verifikasi, peminta, dan penginput |
| **Dependency** | `BE-RWI-164` [BE] |
| **Acceptance criteria** | 1. Dokter memesan konsultasi gizi → peminta akun dokter, `NotRequired` (`UAT-RWF-07`). 2. Perawat memesan 2 PRC atas instruksi dokter jaga → `Pending`, penginput perawat (`UAT-RWF-08`). 3. Perawat memilih dokter tanpa penugasan → pesan "Dokter yang dipilih tidak sedang menangani pasien ini." (`UAT-RWF-30`). 4. Pencarian kode: nol `admissionDoctorId` sebagai peminta dan nol GUID komponen atau unit tertanam. 5. Pesanan mirip → dialog konfirmasi Bank Darah. 6. Gizi dan darah menampilkan "tarif belum tersedia". 7. Menu perawat tidak lagi menampilkan "Integrasi belum tersedia" untuk Gizi dan Bank Darah. 8. Hemodialisa tidak berubah (`FR-RWF-035`) |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual ujung ke ujung dengan modul Gizi dan Bank Darah sungguhan; pencarian kode |
| **Risiko/pemilik** | Gerbang rilis IMP-RWF-01 dan 02. Peran bangsal perlu `BloodComponent : Read` dan permission baca pesanan pada konfigurasi Akses Role. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-175` — Perlu Diverifikasi enam sumber

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Dokter melihat satu daftar "Perlu Diverifikasi" berisi tindakan, Lab, Radiologi, gizi, darah, dan diet, lalu memverifikasi per baris. Satu sumber gagal tidak menyembunyikan sumber lain |
| **Requirement/decision** | `FR-RWF-037`; `RWI-DEC-188`, `191`; `AC-RWF-035`; `NFR-RWF-11`; `INV-RWF-23`; `VAL-RWF-63`, `64` |
| **Kontrak** | Frontend 11.3 (`FE-DOK-15`); API 13.3, 13.4; diet: `keperawatan` API (`GET nutrition-management/diets/instruction-verification-worklist`, `NutritionPatientDiet : VerifyInstruction`) |
| **Reuse** | `physician-needs-review-view.jsx`, `physician-needs-review-client.jsx`, `use-physician-review-worklist.js` (sudah memuat tindakan, Lab, Radiologi) |
| **Cakupan** | Tambah tiga sumber dengan pemuatan terpisah; tombol Verifikasi memanggil `verify-instruction` modul pemilik dengan `ExpectedVersion`; 403 → "Hanya dokter yang memberi instruksi yang dapat memverifikasi"; 409 → "Pesanan ini sudah diverifikasi" lalu muat ulang; kosong → "Tidak ada yang perlu diverifikasi." |
| **Dependency** | `BE-RWI-161` [BE], `BE-RWI-162` [BE], `BE-RWI-166` [KEP] |
| **Acceptance criteria** | 1. Pesanan darah perawat atas nama dokter A tampil di daftar dokter A, tidak di daftar dokter B. 2. Verifikasi menyimpan status, waktu, dan nama pemverifikasi pada pesanan di Bank Darah (`AC-RWF-035`). 3. Sumber diet gagal → hanya bagian diet menampilkan pesan gagal, lima lainnya tetap tampil (`NFR-RWF-11`). 4. Dokter lain → pesan 403. 5. Sudah diverifikasi → pesan 409. 6. Bagian sumber tidak tampil bila pengguna tidak memegang permission verifikasinya |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan dua akun dokter dan satu akun perawat |
| **Risiko/pemilik** | Enam permission verifikasi harus terpasang pada peran dokter. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-176` — Katalog tindakan rawat inap dengan perkiraan harga

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Pemilih tindakan dokter dan perawat di bangsal menampilkan tindakan rawat inap — perawat juga tindakan khusus perawat — beserta status tanggungan dan perkiraan harga dari resolver penjamin. Status "Ditanggung" bawaan dan harga Rp 0 karangan hilang |
| **Requirement/decision** | `FR-RWF-070`, `FR-RWF-034`; `RWI-DEC-165` butir 1, `RWI-DEC-108`, `218`, `219`; `AC-RWF-070`; `UAT-RWF-31`; gerbang rilis IMP-RWF-06 |
| **Kontrak** | Frontend 11.3 (`FE-DOK-19`), 11.5; API 13.5 (`careSetting`, `audience`), 13.2 (`ItemType = Procedure`) |
| **Reuse** | `procedure-form-panel.jsx`; hook `use-inpatient-procedure-tab.jsx`, `use-inpatient-nursing-procedure.jsx`, `use-inpatient-prescription-procedure.jsx` |
| **Cakupan** | Ketiga hook rawat inap mengirim `careSetting=Inpatient` dan `audience=Doctor` atau `Nurse`; `use-doctor-procedure.js` (rawat jalan) tidak diubah. Cabut `unitPrice: option.tariff \|\| option.totalPrice \|\| 0` dan `coverageStatus: option.coverageStatus \|\| "Ditanggung"` (`procedure-form-panel.jsx:102-103`); harga dan tanggungan dari `coverage-status`; "Tarif Satuan" (`:360`) menjadi "tarif belum tersedia" bila tidak `AVAILABLE`; total (`:150`) hanya menjumlahkan perkiraan yang tersedia dan berlabel perkiraan |
| **Dependency** | `BE-RWI-160` [BE], `BE-RWI-163` [BE] |
| **Acceptance criteria** | 1. Tindakan khusus rawat inap tampil di bangsal, tidak di poliklinik (`UAT-RWF-31`). 2. Pemilih perawat menampilkan tindakan khusus perawat. 3. Status tanggungan tidak pernah tampil "Ditanggung" tanpa data server. 4. Harga tidak pernah tampil Rp 0 karena data kosong; yang tampil "tarif belum tersedia". 5. Perkiraan berlabel "perkiraan — tagihan final di kasir". 6. Layar tindakan poliklinik tidak berubah |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual di ruang kerja dokter, ruang kerja perawat, dan poliklinik (regresi) |
| **Risiko/pemilik** | Gerbang rilis IMP-RWF-06. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-177` — Harga obat di tab Resep berlabel perkiraan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Harga dan tanggungan per obat di tab Resep tampil sebagai perkiraan berlabel "perkiraan — tagihan final di kasir", seragam pada seluruh formulir resep rawat inap yang menampilkan harga |
| **Requirement/decision** | `RWI-DEC-218`, `RWI-DEC-219`; `RWI-AC-337` |
| **Kontrak** | Frontend 11.4, 11.5 (`FE-DOK-10`); API 13.2 catatan harga obat (`GET clinical-management/prescribing-drugs`, ✅ tersedia) |
| **Reuse** | `prescription-regular-drug-form.jsx:49`, `:77` (sudah menampilkan harga, "belum tersedia", dan "Ditanggung"/"Tidak Ditanggung"); `prescription-builder-panel.jsx`; `prescribing-drug.service.js` |
| **Cakupan** | Label perkiraan di samping harga; diseragamkan pada formulir resep lain di tab Resep yang menampilkan harga; tanpa endpoint baru |
| **Dependency** | — |
| **Acceptance criteria** | 1. Setiap harga obat di tab Resep disertai label perkiraan. 2. Obat tanpa harga tampil "belum tersedia", bukan Rp 0. 3. Status tanggungan hanya tampil bila `IsCoverageApplicable`. 4. Layar Farmasi dan kasir tidak berubah |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual |
| **Risiko/pemilik** | Hak lihat harga obat mengikuti hak baca `PrescribingDrug : Read`, sudah diketahui pemilik (`02-backend-architecture.md` 12.13). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-178` — Penanda "Pasca operasi" di Konteks pasien

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Dokter melihat penanda "Pasca operasi" pada panel Konteks pasien bila pasien punya kasus OK `Completed`, dan membuka ringkasan operasi baca-saja. Delapan tab tidak berubah |
| **Requirement/decision** | `FR-RWF-081`, `082`; `RWI-DEC-213`; `RWI-AC-339`; `UAT-RWF-38` |
| **Kontrak** | Frontend 11.5 (wilayah baru `FE-DOK-09`); `episode-rawat-inap` frontend 13.4.4 (`FE-INP-28` mode baca-saja) |
| **Reuse** | Laci `FE-INP-28` dari `FE-RWI-196` |
| **Cakupan** | Penanda per kasus OK `Completed`, terbaru di depan, dari `GET operating-room-management/cases?encounterId=`; klik membuka laci tanpa Terima/Tolak; gagal → penanda tidak tampil dan tidak menghalangi isi lain |
| **Dependency** | `FE-RWI-196` [EPS] |
| **Acceptance criteria** | 1. Pasien tanpa kasus `Completed` → penanda tidak tampil. 2. Pasien dengan kasus `Completed` → penanda membuka ringkasan baca-saja tanpa Terima/Tolak (`UAT-RWF-38`). 3. Laporan draft → "Laporan operasi belum final". 4. Jumlah dan urutan delapan tab tidak berubah. 5. Tanpa `OperatingRoomCase : Read` → penanda tidak tampil |
| **Verifikasi** | `npm run lint`; `npm run test:unit`; `npm run build`; verifikasi manual dengan dua pasien |
| **Risiko/pemilik** | Menunggu task sub-modul lain. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

### `FE-RWI-179` ⛔ — Rehab Medik di tab Penunjang dokter

| Field | Isi |
|---|---|
| **Status** | ⛔ **Terblokir** — menunggu `DEC-INP-019` (pemilik Muhammad Hamzah; jalur penutupan `grill-me`) |
| **Outcome** | Kartu Rehab Medik tidak lagi mengirim pesanan ber-ID tindakan buatan; perilaku akhirnya mengikuti jawaban `DEC-INP-019` |
| **Requirement/decision** | `FR-RWF-035`; `RWI-DEC-108`; KK-3 rencana kerja Penunjang Medis rev 2.0; `DEC-INP-019`; gerbang rilis IMP-RWF-03 |
| **Kontrak** | `04-prd-to-mvp.md` 23.8 (Rehab Medik ditunda sebagai *placeholder*) |
| **Reuse** | `supporting-rehab-form.jsx` (`FE-RWI-146`) |
| **Cakupan** | Jawaban "tetap *placeholder*": kartu kembali "Integrasi belum tersedia" tanpa request jaringan dan form dicabut. Jawaban "dibuka": pesanan memakai katalog prosedur fisioterapi dari master lewat alur order tindakan biasa (perkiraan harga dan verifikasi ikut), dan **memerlukan kontrak baru** lewat `design-business-module` sebelum task ini boleh dijalankan |
| **Dependency** | `DEC-INP-019` ⛔ |
| **Acceptance criteria** | Berlaku untuk kedua jawaban: 1. Pencarian kode: nol `procedureId` buatan `…0101` s.d. `…0107` (`supporting-rehab-form.jsx:20-60`). 2. Tidak ada pesanan tindakan dari kartu Rehab sebelum jawaban "dibuka" disetujui beserta kontraknya. 3. Hemodialisa tidak berubah |
| **Verifikasi** | `npm run lint`; `npm run build`; verifikasi manual kartu Rehab; pencarian kode |
| **Risiko/pemilik** | Selama terblokir, kode Rehab saat ini tetap menjadi gerbang rilis. Pemilik: Muhammad Hamzah |
| **DoD** | Keputusan tercatat; kriteria terbukti; lint dan build lulus; laporan tracked; roadmap dan traceability diperbarui |

## Rujukan silang `frontend-roadmap-v2.md`

| Task lama | Keadaan di roadmap lama | Diperbaiki oleh |
|---|---|---|
| `FE-RWI-144` | Register 🟡, kartu "✅ Selesai di tingkat source" | `FE-RWI-173` (IMP-RWF-05) |
| `FE-RWI-145` | Sama | `FE-RWI-174` (IMP-RWF-01, 02) |
| `FE-RWI-146` | Sama | `FE-RWI-179` ⛔ (IMP-RWF-03); bagian Hemodialisa tidak diubah |

Tanda pada roadmap lama tidak diubah oleh dokumen ini. Selisih tanda register 🟡 dan kartu ✅ pada ketiga task itu dicatat untuk pemilik roadmap tersebut.
