# Laporan Perubahan Frontend — `FIX-EPS-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-039` (mencakup `FIX-EPS-039-01` s.d. `FIX-EPS-039-09`) |
| Judul | Ketahanan muat ulang dan konsistensi data pada lima tipe pendaftaran Admisi Rawat Inap |
| Slice | Repair alur admisi: Pasien Baru, Pasien Lama, Bayi Baru Lahir, Kamar Pulih, Transfer IGD/Poli |
| Roadmap | **Tidak ada entri roadmap.** Mengikuti pola `FIX-EPS-030`/`-031`/`-038`: register status dipegang [PLAN-REPAIR-EPS-039](../../../docs/plan-repair/plan-repair-039-audit-lima-tipe-pendaftaran-admisi.md) bagian 1 |
| Trace | [ISSUE-EPS-039](../../../docs/issue/issue-039-audit-lima-tipe-pendaftaran-admisi.md); `FE-RWI-032` (pelanjutan), `RWI-DEC-201`/`VAL-RWF-87/88` (Kamar Pulih), `FIX-EPS-029`, `FIX-EPS-038` |
| Contract version | Endpoint backend existing dipakai apa adanya; tidak ada kontrak baru |
| Wewenang UI | Disetujui pemilik 23-10-2026: seluruh plan repair 039 beserta K-01 (opsi A) dan K-02 |
| Dependency | `FIX-EPS-038-01` (gerbang `isOwnEpisode`, sudah di HEAD `229f071bb`). `FIX-EPS-039-03` terblokir dependency backend — lihat bagian 7 |
| Klasifikasi | `HEAVY` — view admisi bersama lima route, tiga hook pengendali (alur, dokter, transfer), satu hook dan satu util baru |
| Task mode | `FRONTEND` (backend baca-saja, kecuali laporan ini dan register issue/plan repair 039) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; backend hanya berkas laporan dan register 039 |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `229f071bb` (`HamzahV2`), working tree berisi perubahan pihak lain yang belum di-commit (lihat bagian 8) |
| Commit backend yang dijadikan rujukan | `b6c638ee` (`MHamzah`) |
| Tanggal | 2026-10-23 |
| Status | 🟡 SEBAGIAN — 8 dari 9 perbaikan selesai di source dan lulus validasi otomatis; uji layar alur hidup belum dijalankan (agent tidak dapat login). `FIX-EPS-039-03` dikerjakan terpisah sesudah K-01 direvisi ke opsi B — lihat [`FIX-EPS-039-03-FE`](FIX-EPS-039-03-FE.md) |

---

## 1. Keadaan yang ditemukan di awal

Seluruh temuan ada di ISSUE-EPS-039 dengan bukti `path:baris`. Ringkasnya: Draft dibuat otomatis
di Pilih Bed tetapi tidak dicatat di URL dan DPJP tidak disimpan (01); permintaan Kamar Pulih hanya
di state view (02); data rujukan faskes tidak dikirim ke server (03); kunci draf `new:active`
dipakai bersama (04); sumber Transfer dimuat ulang per langkah (05); penjamin tak terbaca ditebak
"Umum/Tunai" (06); daftar Kamar Pulih hanya 50 baris (07); kunci deposit bukan GUID (08); argumen
`onChange` cabang transfer salah (09).

Ditemukan tambahan saat implementasi: view mengirim `transfer.storageKey` ke hook dokter untuk
**semua** jalur. Nilainya tetap berupa teks walau bukan transfer
(`quilvian:transfer-admission:<owner>::`), sehingga isian DPJP jalur non-transfer ditulis ke satu
kunci bersama. Diperbaiki sekalian dalam `FIX-EPS-039-01`.

---

## 2. Proses bisnis dari sisi pengguna

| Jalur | Yang berubah bagi petugas |
| --- | --- |
| Pasien Baru / Lama / Kamar Pulih | Begitu langkah Pilih Bed terbuka, nomor episode Draft langsung tercatat di alamat halaman. Muat ulang (F5) membaca episode yang sama, bukan membuat admisi baru. DPJP, dokter pendukung, catatan, isolasi, dan tingkat ketergantungan tetap terisi setelah muat ulang di langkah Dokter. |
| Kamar Pulih | Permintaan terpilih tercatat di URL (`referralId`). Setelah muat ulang, permintaan dibaca ulang dari server. Bila sudah diproses, dibatalkan, atau milik pasien lain, langkah Informasi Pasien menampilkan "Permintaan Kamar Pulih ini sudah tidak berlaku…" dengan tombol **Kembali ke Daftar**, dan "Lanjut" ditahan. Daftar permintaan kini berhalaman dan dicari di server; lencana kartu menampilkan jumlah sebenarnya. |
| Pasien Baru | Isian langkah 1–2 (jenis kunjungan, rujukan, kategori) tidak lagi terbawa ke pendaftaran pasien berikutnya. Konsekuensinya: muat ulang sebelum pasien tersimpan mengosongkan langkah 1–2. |
| Transfer | Pindah langkah tidak lagi menampilkan kotak "Memuat dan memeriksa permintaan…"; setelah memesan tempat tidur tidak muncul banner "Melanjutkan admisi". |
| Semua (Konfirmasi), Bayi | Penjamin yang tidak terbaca ditulis "Belum terbaca" dengan peringatan dan tombol **Muat Ulang Ringkasan** pada alur pelanjutan, bukan "Umum / Tunai / Mandiri". Pasien tunai yang terbaca normal tetap "Tunai / Umum". |

Jalur tidak normal: permintaan Kamar Pulih gagal dimuat → pesan galat + **Coba Lagi**; tanpa hak baca
permintaan → pesan `403` server; pencarian Kamar Pulih kosong → keadaan kosong existing.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

ISSUE/PLAN-REPAIR 039; `AGENTS.md` frontend; `rules/frontend/*`; source view admisi, hook alur,
dokter, transfer, deposit, resume; util pembayaran/deposit/transfer/session; komponen Kamar Pulih,
Transfer, Konfirmasi, Pilih Bed; backend `InpatientAdmissionReferralController`,
`AdmissionReferralQuery`, `PagedResult`, `InpEpisodeService` (`:505` kunci deposit),
`EncounterReferralService` (`UpsertAsync`, `ResolveNewTarget`, `ValidateInstitutionAndDoctorAsync`),
`ReferralTargetUnitType`, `InpEpisodeService.NewbornCandidates` (`PaymentSourceNameSnapshot`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan | FIX |
| --- | --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | Efek URL tanpa pengecualian Pilih Bed; `transferDraftKey` hanya untuk Transfer; `skipDraftRestore`; hook permintaan Kamar Pulih + keadaan di langkah Informasi Pasien; `nextReferralId`; gerbang memuat Transfer hanya muatan pertama; lencana Kamar Pulih dari `totalData`; kunci deposit `ADM-DEP-…` dihapus; aksi muat ulang penjamin | 01, 02, 05, 06, 07, 08 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | `persistDraft` (Transfer → kunci transfer, lainnya → per pasien); pemulihan isian per pasien; simpanan dihapus saat episode terbentuk; `adoptTransferEpisode` mengisi `createdEpisodeId`; kunci GUID deposit per percobaan admisi | 01, 05, 08 |
| `src/utils/health-services/inpatient-management/inpatient-admission-doctor-storage.jsx` (baru) | Baca/tulis/hapus isian Dokter di `sessionStorage` per pasien, dengan normalisasi | 01 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx` | `referralId` + `nextReferralId` (dihapus saat keluar jalur Kamar Pulih atau pasien berganti); kunci `active` tidak dipakai di semua mode; pembersihan `…:active`; migrasi kunci semua mode | 02, 04 |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `INPATIENT_ADMISSION_QUERY.REFERRAL` | 02 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-referral-selection.js` (baru) | Membaca permintaan Kamar Pulih per ID; dipakai hanya bila `Pending` dan milik pasien yang sama | 02 |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-transfer.js` | Tanpa `stepSlug` pada dependensi; sumber lama dipertahankan saat membaca ulang | 05 |
| `src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx` | "Belum terbaca" + peringatan + tombol muat ulang | 06 |
| `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx` | `inheritedPayerLabel`: "Tunai" hanya bila `paymentType` tunai, selain itu "Belum terbaca" (dua baris; perubahan bayi kembar pihak lain di berkas ini tidak disentuh) | 06 |
| `src/components/view/health-services/inpatient-management/admission-referrals/inpatient-admission-recovery-referral-step.jsx` | Pencarian server (debounce 300 ms), paging 20 + `Pagination`, `totalData`; "Asal: Poli Bedah" pengganti → "-" | 07 |
| `src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx` | `onChange={(_name, bedId) => …}` | 09 |
| `tests/unit/inpatient-admission-reload-resilience.test.mjs` (baru) | 7 tes: normalisasi simpanan DPJP (perilaku) dan regresi FIX-01 s.d. 09 | — |
| `tests/unit/inpatient-newborn-admission.test.mjs` | Asersi kunci `active` FIX-EPS-038 disesuaikan ke aturan semua jalur | 04 |

### 3.3 Kepatuhan arsitektur frontend

- Alur view → hook → service dipertahankan; panggilan API baru ada di hook
  (`use-inpatient-admission-referral-selection`), normalisasi di `src/utils`.
- Penyimpanan DPJP mengikuti pola `inpatient-admission-payment-storage` (prefix
  `quilvian:inpatient-admission:`), sehingga ikut dibersihkan `clearInpatientAdmissionSession`.
- Tidak ada endpoint, Redux slice, atau arsitektur state baru.

**Gerbang base component**

```
UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Pesan memuat/permintaan tidak berlaku (Kamar Pulih) | `InformationAlert` | `base-features/information-alert` | REUSE | `info` / `warning` |
| Tombol Coba Lagi / Kembali ke Daftar | `BaseButton` | `base-features/base-button`; kelas baris `bedBookingActions` existing | REUSE | `secondary` |
| Badge kategori penjamin tak terbaca | `StatusBadge` | sudah dipakai kartu Penjamin | REUSE | status `warning` |
| Peringatan + Muat Ulang Ringkasan | `InformationAlert`, `BaseButton` | idem | REUSE | — |
| Paging daftar Kamar Pulih | `Pagination` | `features/pagination/pagination`, dipakai daftar Transfer | REUSE | props sama dengan daftar Transfer |
| Pilihan bed transfer | `BaseSelectField` | `base-form-control.jsx` `onChange(name, value)` | REUSE | argumen diperbaiki |

Modul referensi visual: daftar Transfer (`inpatient-admission-transfer-list.jsx`) untuk paging dan
kartu Konfirmasi existing untuk penjamin.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kamar Pulih: "Memuat permintaan Kamar Pulih…"; daftar Kamar Pulih: indikator memuat existing; Transfer: kotak memuat hanya pada muatan pertama |
| Kosong | Daftar Kamar Pulih kosong: keadaan kosong existing; penjamin tak terbaca: "Belum terbaca" |
| Gagal | Permintaan Kamar Pulih: pesan server + Coba Lagi; permintaan tidak berlaku: peringatan + Kembali ke Daftar |
| Tanpa hak akses | Kamar Pulih tanpa `InpatientAdmissionReferral : Read`: pesan Akses Ditolak existing / pesan `403` server |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Admission Referral

Base URL: `api/v1/health-services/inpatient-management/admission-referrals`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?status=Pending&search=&pageNumber=&pageSize=20` | Daftar Kamar Pulih berhalaman dan pencarian server | `InpatientAdmissionReferral : Read` |
| `GET` | `/?status=Pending&pageSize=1` dan `…&overdueOnly=true&pageSize=1` | Angka lencana kartu dari `totalData` | `InpatientAdmissionReferral : Read` |
| `GET` | `/{id}` | Membaca ulang permintaan dari `referralId` di URL | `InpatientAdmissionReferral : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Tidak berubah, kecuali `depositIdempotencyKey` kini GUID per percobaan admisi | `InpatientEpisode : Create` |

Kode status: `INP-ADM-REF-001` (409) tidak lagi muncul setelah muat ulang di jalur Kamar Pulih
selama `referralId` di URL masih `Pending`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 10 berkas source yang diubah | 0 error, 6 warning: 4 sudah ada sebelumnya (`use-inpatient-admission-flow.jsx` ×2, `use-inpatient-admission-transfer.js` ×2), 2 baru `react-hooks/set-state-in-effect` (efek pemulihan DPJP `use-inpatient-admission-doctor.jsx:195`, pemuat daftar Kamar Pulih `:80`) — pola yang sama dengan efek lain di berkas tersebut | `PASS` / `EXISTING WARNING` / `NEW WARNING` | Keluaran ESLint |
| `npm run lint:errors` | Selesai tanpa error | `PASS` | Exit code 0 |
| Tes admisi bayi + tes baru | 30/30 lulus | `PASS` | `node --test` dua berkas |
| `npm run test:unit` (seluruh suite) | 3624/3667 lulus; 43 gagal | `EXISTING / ENVIRONMENT ISSUE` | Daftar 43 kegagalan identik dengan run sebelum perubahan ini (HEAD `229f071bb`); tidak ada kegagalan baru |
| `npm run build` | "Compiled successfully in 58s", 517/517 halaman, `prepare-standalone` siap | `PASS` | Exit code 0; dijalankan berdampingan dengan `next dev` milik user |
| Render SSR kartu Penjamin (komponen asli, data samaran) | 4/4 lulus: tak terbaca + muat ulang, `-` tanpa aksi, Tunai normal, Asuransi normal | `PASS` | Skrip render di scratch sesi |
| Render SSR langkah Penempatan bayi (regresi 038) | 10/11 lulus; R3 berbeda karena perubahan pihak lain yang belum di-commit (judul "…Sudah Terisi", tombol "Alihkan ke Perawatan Perinatologi") | `PASS` (bukan regresi task ini) | Skrip render di scratch sesi |
| Grep anti-regresi checklist UI pada baris yang ditambahkan (view, Konfirmasi, Kamar Pulih, Pilih Bed) | 0 warna literal, 0 tipografi, 0 `<button>` baru, 0 `<table>`, 0 `!important`, 0 inline style baru | `PASS` | `git diff` baris `+` |

Uji manual: `NOT FEASIBLE` — agent tidak dapat login ke aplikasi. Perilaku yang bergantung
navigasi, `sessionStorage`, dan jaringan (muat ulang di Pilih Bed, `referralId`, kedip Transfer,
kunci deposit di payload) hanya terbukti di source dan tes regresi, belum di layar.

**Tidak dijalankan:** `test:e2e`, `test:uat`; skenario S1–S10 plan repair 039.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| FIX-01 AC1–AC4 — URL memuat `episodeId` di Pilih Bed, F5 tanpa episode kedua, DPJP tetap | Terpenuhi di source; menunggu uji layar | Efek URL view; `persistDraft`/pemulihan hook dokter; tes regresi |
| FIX-01 AC5 — simpanan DPJP bertes | Terpenuhi | Tes `normalizeInpatientDoctorSnapshot` |
| FIX-02 AC1–AC3 — Kamar Pulih tahan muat ulang; permintaan tak berlaku ditolak dengan jelas | Terpenuhi di source; menunggu uji layar | Hook `use-inpatient-admission-referral-selection`; langkah Informasi Pasien |
| FIX-03 — data rujukan dikirim ke server | **Tidak dikerjakan di task ini.** Sesudah K-01 direvisi ke opsi B, dikerjakan di [`FIX-EPS-039-03-BE`](../backend/FIX-EPS-039-03-BE.md) dan [`FIX-EPS-039-03-FE`](FIX-EPS-039-03-FE.md). Catatan saat task ini dikerjakan: ⛔ terblokir | `PUT /patient-encounters/{id}/referral` tidak mendukung kunjungan rawat inap: `ResolveNewTarget` mencatat tujuan rawat inap sebagai `Laboratory` karena `ReferralTargetUnitType` tidak punya nilai Rawat Inap, dan faskes wajib mitra berperjanjian aktif (`Pm17`). Butuh task backend atau keputusan ulang K-01 |
| FIX-04 AC1–AC2 — draf `active` tidak terbawa; draf per pasien tetap | Terpenuhi di source; menunggu uji layar | Hook alur; tes regresi |
| FIX-05 AC1–AC2 — Transfer tanpa kedip dan tanpa banner pelanjutan | Terpenuhi di source; menunggu uji layar | Hook transfer, gerbang view, `adoptTransferEpisode` |
| FIX-06 AC1–AC2 — "Belum terbaca" + muat ulang; tunai normal tetap | Terpenuhi | Render SSR 4/4 |
| FIX-07 AC1–AC2 — pencarian server dan lencana `totalData` | Terpenuhi di source; menunggu uji layar | Komponen Kamar Pulih, efek lencana |
| FIX-08 — kunci deposit GUID per admisi | Terpenuhi di source; menunggu uji layar (payload) | `createGuid`, ref direset saat episode terbentuk/pasien berganti |
| FIX-09 — argumen `onChange` | Terpenuhi | Tes regresi; lint |

**Definition of Done yang belum terpenuhi:** uji layar S1–S10; `FIX-EPS-039-03`; revisi dokumen hulu
plan repair bagian 7 (di luar wewenang task ini).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 `NEW WARNING` `react-hooks/set-state-in-effect` (bukan error; aturan disetel `warn` di repo) |
| Masalah yang diketahui | (1) Muat ulang sebelum pasien baru tersimpan mengosongkan langkah 1–2 Pasien Baru (konsekuensi FIX-04 yang disetujui). (2) Tab ICU/Rawat Inap/Terlambat pada daftar Kamar Pulih masih menyaring halaman yang sedang tampil saja; tab "Semua" memakai `totalData`. (3) Isi tes "FIX-EPS-039: Penanganan kasus persalinan dengan bayi yang sudah terdaftar…" di `tests/unit/inpatient-newborn-admission.test.mjs` milik pekerjaan lain (bayi kembar) dan tidak termasuk plan repair 039; namanya sebaiknya diganti agar tidak tertukar |
| Dependency backend | `FIX-EPS-039-03`: perluasan `PUT /patient-encounters/{encounterId}/referral` untuk kunjungan rawat inap (tipe tujuan Rawat Inap, kebijakan faskes non-mitra) — milik modul Pendaftaran |
| Perubahan sampingan | `NONE` di repository. Skrip render ada di folder scratch sesi |
| Interupsi | `NONE` |
| Status Git | Frontend `HamzahV2` @ `229f071bb`. Berkas task ini: 10 ` M` dan 3 `??` (lihat 3.2). Perubahan pihak lain yang tidak disentuh: `.gitignore`, `inpatient-discharge-view.jsx`, `inpatient-financial-clearance-view.jsx` + `.module.css`, `inpatient-resume-tab.jsx`, `use-inpatient-newborn-admission.js`, sebagian `inpatient-newborn-admission-steps.jsx` dan `inpatient-newborn-admission.test.mjs` (bayi kembar), `docs/`. Backend `MHamzah` @ `b6c638ee`: laporan ini dan register 039 belum di-track |
| Langkah berikutnya | Pemilik menjalankan S1–S8 plan repair 039 di localhost, lalu memutuskan jalur FIX-03 (task backend modul Pendaftaran, atau K-01 opsi B) |
