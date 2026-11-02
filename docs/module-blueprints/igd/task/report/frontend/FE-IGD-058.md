# Laporan Perubahan Frontend — `FE-IGD-058`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-058` |
| Judul | *Perlu Verifikasi* di Ruang Kerja Dokter IGD |
| Slice | R3.14 slice D2 · `SCR-IGD-D03`; ringkasan `SCR-IGD-D01` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian R3.14, kartu revisi 16 |
| Trace | `FR-IGD-102`; `AT-IGD-209` (langkah dokter), `AT-IGD-250`; `IGD-DEC-224`, `360`, `363`, `369`, `370` |
| Contract version | API **`0.21.0`** §10.7 (`instruction-verification-worklist`, `verify-instruction`); state **`0.16.0`** §10.2 — `approved` (`IGD-DEC-368`) |
| Wewenang UI | `DEV_DISCRETION`; mengikat: kalimat kosong persis *"Tidak ada pesanan yang menunggu verifikasi Anda."* (`03` §15.4 tabel C) |
| Dependency | `FE-IGD-045` ✅; `BE-IGD-071` 🟡 — worklist terbukti lewat API; verifikasi belum dapat diuji karena posisi dokter IGD tidak memegang `LabOrder : Verify`/`RadOrder : Verify` (`IGD-FACT-256`, `IGD-OQ-145`) |
| Klasifikasi | `MEDIUM` — 1 route baru, 3 view baru, 1 hook baru, 3 berkas diubah; nol state Redux baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan ini, roadmap, traceability di `NewQuilvianSystemBackend` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `1c97ebbf9` + working tree |
| Commit backend yang dijadikan rujukan | `1c00e31d` + working tree `BE-IGD-071` (belum di-commit) |
| Tanggal | 28 Oktober 2026 |
| Status | ✅ **SELESAI — 28 Oktober 2026 (siang).** Acceptance 1–5 terpenuhi: sesudah `IGD-OQ-145` dipenuhi, `uji-layar-058b.mjs` lulus 3/3 — verifikasi lab dan rad dari layar (baris hilang, jumlah dan angka ringkasan turun), dua akun sesudah pengalihan DPJP, kalimat kosong (respons dikosongkan lewat intersepsi peramban). Dirilis bersama `BE-IGD-071` ✅ dan `FE-IGD-057` ✅; belum di-commit. *Sebelumnya:* 🟡 **SEBAGIAN — 28 Oktober 2026 (pagi), sesudah uji layar.** Terbukti: acceptance 3 (angka ringkasan = halaman = API, gagal → *"—"*), 4 (`eslint` 0/0, `npm run build` lulus), dan bagian *tampil di daftar dr. Ani* dari acceptance 1. **Belum:** verifikasi dari layar (izin `Verify`, `IGD-OQ-145`), acceptance 2 (tidak ada bagian kosong untuk diamati — worklist dr. Jenah Ali berisi pesanan batal), acceptance 5 (akun dokter kedua tanpa izin baca, `IGD-FACT-257`), dependency `BE-IGD-071` 🟡 |

---

## 1. Keadaan yang ditemukan di awal

Ruang Kerja Dokter IGD belum punya daftar pesanan yang menunggu verifikasi; ringkasannya hanya memuat *Pasien saya* dan
*Menunggu tanda tangan* (`doctor-emergency-view.jsx` `summaryItems`). Rawat inap sudah punya halaman *Perlu Review*
(`doctor-inpatient/needs-review`) beserta service `inpatient-instruction-review.service` dan daftar sumber
`INSTRUCTION_REVIEW_SOURCES` — rujukan reuse kartu ini. Tiga hal membuat rujukan itu tidak dapat dipakai apa adanya:

| Temuan | Bukti | Penanganan IGD |
| --- | --- | --- |
| `normalizeInstructionReviewItem` mencari `labOrderId`, `radOrderId`, `id`, sedangkan DTO worklist lab/rad hanya membawa `orderId` (`IGD-FACT-261`) | DTO `LabOrderInstructionVerificationItemResponse`, `RadOrderInstructionVerificationItemResponse`; item mentah `20-jaringan-api.json` | Normalizer IGD sendiri. Layar Rawat Inap urusan Rawat Inap dan tidak disentuh (`IGD-DEC-370`) |
| Hook rawat inap menyaring sumber menurut izin `Verify` | `use-physician-review-worklist.js` | Dokter IGD tidak memegang `Verify` → halaman akan kosong total; IGD mengambil daftar dengan izin `Read`, tombol mengikuti `Verify` |
| Worklist backend tidak membedakan IGD dan rawat inap, dan memuat pesanan batal (`IGD-FACT-260`) | Service worklist Lab/Rad | Baris ber-episode rawat inap dibuang di layar (`IGD-DEC-370`); baris batal tampil berlencana tanpa tombol (`IGD-DEC-369`) |

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Yang dilakukan dokter IGD | Yang terjadi di layar |
| ---: | --- | --- |
| 1 | Membuka Ruang Kerja Dokter IGD | Ringkasan menampilkan angka ketiga *Perlu verifikasi* = jumlah pesanan lab + radiologi **pasien IGD** yang menunggu verifikasinya. Tombol *Perlu Verifikasi* di samping *Catatan Saya* |
| 2 | Menekan tombol atau angka itu | Halaman *Perlu Verifikasi*: bagian Laboratorium dan Radiologi, masing-masing dengan jumlah, tabel (waktu pesan, pasien, pemeriksaan, perawat penginput, status), dan *Muat Ulang* |
| 3 | Menekan *Verifikasi* pada sebuah baris | Konfirmasi; sesudah dikonfirmasi baris hilang dan angkanya turun |

Pesanan pasien yang sudah ditangani Rawat Inap (punya episode rawat inap) tidak tampil di sini — IGD berhenti di
transfer pasien; verifikasinya dari layar Rawat Inap (`IGD-DEC-370`).

Jalur tidak normal:

| Keadaan | Yang terjadi |
| --- | --- |
| Tidak ada pesanan | *"Tidak ada pesanan yang menunggu verifikasi Anda."* |
| Salah satu daftar gagal dimuat | *"Daftar pesanan laboratorium gagal dimuat."* (atau radiologi) + *Coba lagi*; bagian lain tetap jalan; angka ringkasan *"—"* |
| Akun tanpa izin `Read` | Bagian itu tidak tampil; tanpa keduanya, angka ringkasan tidak ditampilkan |
| Akun tanpa izin `Verify` | Daftar tampil; kolom *Aksi* "-" dan *"Akun Anda belum memiliki izin verifikasi untuk daftar ini."* — **keadaan dev hari ini** |
| Pesanan sudah dibatalkan | Lencana *Dibatalkan*, tanpa tombol |
| Server menolak verifikasi (`403`, `409`) | Pesan server apa adanya di dialog; pada `409` daftar dimuat ulang |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-IGD-058` revisi 16; laporan `BE-IGD-071`; rujukan rawat inap `physician-needs-review-*.jsx`,
`use-physician-review-worklist.js`, `inpatient-instruction-review.service.js`, `inpatient-instruction-review-constants.js`,
`inpatient-instruction-review-utils.js`; rujukan visual IGD *Catatan Saya* (`my-authored-emergency-notes-view.jsx`,
`emergency-locked-notes-tab.jsx`, `emergency-my-authored-notes.module.css`); `doctor-emergency-view.jsx`,
`use-emergency-admission-signature.jsx`, `use-permission`, `DataTable`, `ConfirmModal`, `StatusBadge`, `Hero`,
`InformationAlert`, `AccessDeniedGate`. Backend (baca saja): DTO worklist, batas `pageSize` 1–100 di kedua service
worklist, `verify-instruction` → `LabOrder : Verify`/`RadOrder : Verify`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/emergency-installation-management/doctor-emergency/needs-review/page.jsx` (**baru**) | Route tipis + metadata |
| `src/components/view/.../doctor-emergency/needs-review/emergency-needs-review-client.jsx` (**baru**) | `AccessDeniedGate` |
| `src/components/view/.../doctor-emergency/needs-review/emergency-needs-review-view.jsx` (**baru**) | `Hero`, `InformationAlert`, satu bagian per sumber (`DataTable` berhalaman di layar, muat ulang/coba lagi), `ConfirmModal` |
| `src/components/view/.../doctor-emergency/needs-review/emergency-needs-review-table-columns.jsx` (**baru**) | Kolom; `StatusBadge` *Menunggu verifikasi*/*Dibatalkan*; tombol *Verifikasi* hanya bila izin `Verify` dan pesanan belum batal |
| `src/lib/hooks/.../emergency-physician/use-emergency-needs-review.jsx` (**baru**) | Adapter atas service dan sumber rawat inap (`lab`, `radiology`): ambil **seluruh** halaman worklist per 100 bila izin `Read`, buang baris ber-episode rawat inap, paging dan jumlah dihitung dari baris IGD; `AbortController` per sumber; kunci verifikasi ganda; muat ulang sesudah berhasil dan sesudah `409` |
| `src/utils/.../emergency-physician-utils.js` | `normalizeEmergencyNeedsReviewItem` — membaca `orderId`, menandai `isCancelled`, `isInpatient` |
| `src/lib/constants/.../emergency-physician-constant.jsx` | Route, sumber, ukuran halaman (25) dan ukuran ambil (100), `EMERGENCY_NEEDS_REVIEW_TEXT` |
| `src/components/view/.../doctor-emergency/doctor-emergency-view.jsx` | Angka ringkasan ketiga (hook yang sama; gagal → *"—"*) dan tombol *Perlu Verifikasi* |

Dua berkas terakhir sudah berisi perubahan Rizki yang belum di-commit (penghapusan `noDoctorIdentityNote`); suntingan task
ini ditambahkan di baris lain atas persetujuannya (`IGD-DEC-369`).

### 3.3 Kepatuhan arsitektur frontend

Alur `route → client → view → hook → service → InstanceAxios` dipertahankan; tanpa slice Redux baru (pola hook tanda
tangan IGD dan hook rawat inap). Service dan daftar sumber rawat inap dipakai ulang, normalizer IGD ditulis sendiri.
Gaya memakai stylesheet *Catatan Saya* IGD — nol CSS baru, nol nilai visual literal.

**Gerbang base component.** `UI GATE: 8 elemen — REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status |
| --- | --- | --- | --- |
| Kepala halaman + kembali | `Hero` + `BaseButton as={Link}` | pola `my-authored-emergency-notes-view.jsx` | REUSE |
| Keterangan kebijakan, galat, izin | `InformationAlert` | base-features | REUSE |
| Tabel berhalaman | `DataTable` (`rowKey` fungsi, `emptyTitle`, `pageNumber`…`onPageChange`) | base-features | REUSE |
| Status baris | `StatusBadge` (`status` `pending`/`inactive`) | base-features | REUSE |
| Tombol verifikasi, muat ulang | `BaseButton` | base-features | REUSE |
| Konfirmasi verifikasi | `ConfirmModal` | base-features | REUSE |
| Gerbang akses | `AccessDeniedGate` | base-features | REUSE |
| Angka ringkasan | `ClinicalSummaryBar` item | `doctor-emergency-view.jsx` | REUSE |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka `DataTable`, jumlah *"…"*; angka ringkasan *"…"* |
| Kosong | *"Tidak ada pesanan yang menunggu verifikasi Anda."* |
| Gagal | *"Daftar pesanan laboratorium gagal dimuat."* / radiologi + *Coba lagi*; angka ringkasan *"—"* |
| Tanpa hak akses | `Read` ditolak → *"Anda tidak memiliki akses untuk membaca daftar ini."*; tanpa `Verify` → *"Akun Anda belum memiliki izin verifikasi untuk daftar ini."* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/instruction-verification-worklist?pageNumber=&pageSize=100` | Daftar dan angka ringkasan (seluruh halaman) | `LabOrder : Read` |
| `PUT` | `/v1/health-services/laboratory-management/lab-orders/{id}/verify-instruction` | Verifikasi (body `{}`) | `LabOrder : Verify` |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/radiology-management/rad-orders/instruction-verification-worklist?pageNumber=&pageSize=100` | Daftar dan angka ringkasan | `RadOrder : Read` |
| `PUT` | `/v1/health-services/radiology-management/rad-orders/{id}/verify-instruction` | Verifikasi | `RadOrder : Verify` |

**Delta source backend ↔ kebutuhan layar.** (1) Worklist memuat pesanan `Cancelled` — ditampilkan berlencana tanpa tombol
dan ikut dihitung (`IGD-DEC-369`); perbaikannya milik Laboratorium/Radiologi. (2) Worklist tidak terikat IGD — baris
ber-`inpEpisodeId` dibuang di layar (`IGD-DEC-370`); karena itu seluruh halaman worklist diambil (per 100) supaya angka
dan paging sama dengan isi daftar. Pada data uji hari ini nol baris rawat inap.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `eslint` 8 berkas, dibandingkan dengan `HEAD` lewat `--stdin` | 0 error, 0 peringatan; nol temuan baru (diulang sesudah setiap perubahan) | `PASS` | Keluaran perintah 28 Oktober 2026 |
| Komentar baru | 0 | `PASS` | Diff 3 berkas lama + grep berkas baru |
| Akhir baris | Tiga berkas CRLF tetap CRLF utuh; berkas baru LF; nol campuran | `PASS` | Hitung byte |
| Grep `ui-consistency-checklist` pada JSX baru | Nol warna literal, `style` inline, `<button>`/`<table>` mentah, utility tipografi; nol CSS baru | `PASS` | Keluaran perintah |
| `npm run build` | Tiga kali exit 0, tanpa warning; build terakhir BUILD_ID `xyBFHuOLnm2WBo0PDNvBU` | `PASS` | Log build |
| `npm run test:unit` | — | `NOT RUN` | Atas perintah pemilik |
| Uji layar `uji-layar-058.mjs` putaran 1 | Halaman *Perlu Verifikasi* **rusak dirender** (*"This page couldn't load"*) — `TypeError: M is not a function` di `DataTable`: view mengirim `rowKey="id"` (string), padahal `DataTable` memanggil `rowKey(row, index)`. **Cacat kode task ini**, lolos `eslint` dan build | `NEW ERROR` → diperbaiki | `058-hasil-run1.json`, `058-b-halaman-run1-crash.png`; probe `probe-058.mjs` |
| Uji layar putaran 2 (sesudah `rowKey={(row) => row.id}` dan build ulang) | 2 skenario: `058-gagal-dimuat` `PASS`; `058-daftar-dan-ringkasan` `FAIL` 1 cek — **salah rancang**: cek mencari *"Menunggu verifikasi"* peka huruf, `StatusBadge` menampilkan *"Menunggu Verifikasi"*. Verdict tidak diubah; diukur ulang terpisah | Lihat di bawah | `058-hasil.json`, `058-jaringan.json`, PNG `058-*`; galat halaman `[]` |
| Uji layar `uji-layar-058b.mjs` (28 Oktober siang, sesudah `IGD-OQ-145`) | 3/3 `PASS`, galat halaman `[]`: lab 11 → 10 dan rad 5 → 4 sesudah *Verifikasi* (`PUT` `200`, baris hilang, kedua pesanan `Verified`), ringkasan 16 → 14; sesudah DPJP Aldo dialihkan ke dr. B4 IGD Utama pesanan tetap bertombol di daftar Jenah dan tidak ada di daftar Utama (layar dan API `200`); worklist dikosongkan lewat `page.route` → kalimat kosong persis di kedua bagian, ringkasan 0 | `PASS` | `058b-hasil.json`, `058b-jaringan.json`, PNG `058b-*` |
| Ukur ulang cek A1 — `probe-058b.mjs` | Pesanan `Pending` atas instruksi dr. Jenah Ali tampil sebagai baris 9 dengan *Menunggu Verifikasi*, kolom *Aksi* "-" | `PASS` | `058-ukur-ulang-a1.txt` |

Nilai terukur putaran 2: angka ringkasan **11** = jumlah halaman 11 (lab 8, rad 3) = baris IGD worklist API (lab 8, rad 3,
rawat inap dibuang 0); lencana *Dibatalkan* 7 = pesanan batal API 7; tombol *Verifikasi* 0 dan peringatan izin tampil;
worklist lab dipaksa `500` → ringkasan *"Perlu verifikasi —"*, bagian lab *"Daftar pesanan laboratorium gagal dimuat."*
+ *Coba lagi*, bagian radiologi tetap 3. Pesanan persiapan dibatalkan `200`.

Uji manual: `PASS` untuk yang dapat diamati (otomatisasi peramban, akun dr. Jenah Ali — bukan SuperAdmin).

**Tidak dijalankan:** keadaan kosong pada data nyata — worklist dokter uji di dev selalu berisi pesanan batal; dibuktikan lewat intersepsi (`058b-kosong`).

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria (kartu revisi 16) | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pesanan perawat atas instruksi dr. Ani tampil di daftar dr. Ani dan dapat diverifikasi; sesudahnya hilang dari daftar | **Terpenuhi** | `058-ukur-ulang-a1.txt` (tampil); `058b-verifikasi` (verifikasi `200`, hilang, jumlah turun) |
| 2 | Daftar kosong menampilkan kalimat persis | **Terpenuhi** | `058b-kosong`: kalimat persis di bagian lab dan radiologi. Respons worklist dikosongkan lewat intersepsi peramban karena worklist dokter uji di dev tidak pernah kosong (berisi pesanan batal) |
| 3 | Angka *Perlu verifikasi* pada ringkasan sama dengan jumlah worklist; gagal dimuat → *"—"* | **Terpenuhi** | 11 = 11 = API; `500` → *"—"* |
| 4 | Diff, komentar, akhiran baris, `globals.css`; `eslint` 0 error; `npm run build` lulus | **Terpenuhi** | Bagian 6 |
| 5 | Sesudah DPJP dialihkan, pesanan tetap di daftar dokter pemberi instruksi dan tidak di daftar DPJP baru | **Terpenuhi** | `058b-dua-akun-pengalihan`: akun dr. Jenah Ali dan dr. B4 IGD Utama (penempatan diperbaiki `IGD-OQ-145`) |

**DoD.** Acceptance 1–5 terpenuhi; laporan ini ada; register, grafik, dan traceability ditandai ✅ 28 Oktober 2026.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Ruang kerja dokter mengambil seluruh worklist lab dan radiologi saat dibuka (per 100), hanya bila akun memegang `Read` — dibutuhkan supaya angka hanya menghitung pasien IGD |
| Masalah yang diketahui | (1) Worklist memuat pesanan batal — milik Laboratorium/Radiologi (`IGD-FACT-260`). (2) Angka ringkasan tidak diperbarui otomatis sesudah verifikasi di halaman lain; diperbarui saat ruang kerja dibuka kembali. (3) `DataTable` menerima `rowKey` hanya sebagai fungsi — string membuat halaman rusak saat ada baris, tidak tertangkap lint/build |
| Dependency backend | `BE-IGD-071` 🟡; izin `LabOrder : Verify`/`RadOrder : Verify` posisi dokter IGD (`IGD-OQ-145`) |
| Perubahan sampingan | `NONE`. Diff Rizki pada `doctor-emergency-view.jsx` dan `emergency-physician-constant.jsx` utuh; `procedure-catalog-panel.jsx` tidak disentuh |
| Interupsi | Backend mati 27 Oktober malam; menyala kembali 28 Oktober 09.28 |
| Status Git | `M` `doctor-emergency-view.jsx`, `emergency-physician-constant.jsx` (bersama perubahan Rizki), `emergency-physician-utils.js`; `??` `src/app/.../doctor-emergency/needs-review/`, `src/components/view/.../doctor-emergency/needs-review/`, `use-emergency-needs-review.jsx` |
| Langkah berikutnya | Commit bersama `BE-IGD-071` dan `FE-IGD-057` atas perintah Rizki |
