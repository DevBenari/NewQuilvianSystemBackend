# Laporan Perubahan Frontend — `FIX-EPS-039-03-FE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-039-03-FE` (bagian frontend `FIX-EPS-039-03`) |
| Judul | Data rujukan faskes luar dikirim ke server dan ditampilkan pada langkah Konfirmasi |
| Slice | Repair admisi rawat inap — jenis kunjungan Rujukan pada Pasien Baru dan Pasien Lama |
| Roadmap | **Tidak ada entri roadmap.** Register status dipegang [PLAN-REPAIR-EPS-039](../../../docs/plan-repair/plan-repair-039-audit-lima-tipe-pendaftaran-admisi.md) bagian 1 |
| Trace | [ISSUE-EPS-039](../../../docs/issue/issue-039-audit-lima-tipe-pendaftaran-admisi.md) `ISS-EPS-039-03`; `RWI-DEC-269`, `RWI-AC-390`, `FE-RWI-223`, `FE-RWI-224`; keputusan pemilik K-01 = B, 23-10-2026 |
| Contract version | Perluasan additive `POST /episodes` dan `GET /episodes/{id}` dari [`FIX-EPS-039-03-BE`](../backend/FIX-EPS-039-03-BE.md): objek opsional `externalReferral` |
| Wewenang UI | Plan repair 039 disetujui pemilik 23-10-2026, termasuk revisi K-01 ke opsi B |
| Dependency | `FIX-EPS-039-03-BE`: selesai di source (`MHamzah` @ `488faa22`, belum di-commit); migration **belum diterapkan** |
| Klasifikasi | `MEDIUM`: satu util payload dan validator, satu normalizer, dua langkah isian, langkah Konfirmasi, view, dan hook dokter |
| Task mode | `FRONTEND` (backend baca-saja, kecuali laporan ini dan register 039) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `tests/unit/**`; backend hanya laporan ini, register plan repair 039, dan riwayat issue 039 |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `8f198ec2b` (`HamzahV2`) |
| Tanggal | 2026-10-23 |
| Status | 🟡 SEBAGIAN — keempat acceptance criteria ada di source dan lulus validasi otomatis. Belum diuji bolak-balik dengan server karena migration belum diterapkan, dan belum diuji di layar karena agent tidak dapat login |

---

## 1. Keadaan yang ditemukan di awal

| Bagian | Keadaan |
| --- | --- |
| Langkah Jenis Kunjungan (Pasien Baru) dan Jenis Kunjungan & Kategori (Pasien Lama) | Kelima isian rujukan diwajibkan dan disimpan ke `flow.referralData`, tetapi tidak pernah dikirim ke server |
| Validasi isian | Disalin sama persis di dua langkah. Hanya memeriksa isian kosong; tanggal di masa depan dan panjang isian tidak diperiksa, padahal server kini menolaknya |
| `buildInpatientAdmissionEpisodePayload` | Tidak punya field rujukan |
| `normalizeEpisodeDetail` | Tidak membaca `externalReferral` |
| Langkah Konfirmasi | Tidak ada kartu rujukan |

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas memilih **Rujukan** pada langkah Jenis Kunjungan (Pasien Baru) atau Jenis Kunjungan &
   Kategori (Pasien Lama), lalu mengisi data surat rujukan.
2. Saat menekan Lanjut, layar memeriksa isian dengan aturan yang sama seperti server:
   - kelima isian wajib (layar tetap mewajibkan dokter perujuk sesuai `RWI-DEC-269`);
   - panjang isian tidak melewati batas kolom;
   - tanggal rujukan tidak boleh di masa depan.
3. Saat episode Draft dibuat di langkah Pilih Bed, `POST /episodes` membawa objek
   `externalReferral`. Waktunya dikirim dalam UTC.
4. Langkah Konfirmasi membaca episode dari server dan menampilkan kartu **Rujukan Faskes Luar**.

Contoh:

| Kejadian | Hasil di layar |
| --- | --- |
| Rujukan nomor `0123/RUJ-PKM/X/2026`, 23-10-2026 09:30 WIB | Dikirim `referralDateTime: "2026-10-23T02:30:00.000Z"`; Konfirmasi menampilkan "23/10/2026, 09.30" |
| Tanggal rujukan besok | Lanjut ditolak: "Tanggal dan jam rujukan tidak boleh melewati waktu sekarang." |
| Nama faskes 201 karakter (hasil tempel) | Isian terpotong di 200 oleh `maxLength`; bila tetap lolos, validator menolak "Fasilitas kesehatan perujuk paling banyak 200 karakter" |
| Kunjungan Umum | Payload tanpa `externalReferral`; Konfirmasi tanpa kartu rujukan |
| Jalur Bayi Baru Lahir, Kamar Pulih, Transfer | Tidak mengirim rujukan walaupun draf lama masih menyimpan isian rujukan |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `base-component-catalog.md`,
`ui-consistency-checklist.md`; `base-form-control.jsx` (prop `maxLength` pada `BaseInputField`
dan `BaseTextAreaField`, tidak ada batas maksimum pada `BaseDateTimeField`); langkah
Konfirmasi (pola kartu `confirmationSummaryCard`/`infoRow`); `inpatient-admission-flow-constants.jsx`
(urutan langkah per jalur); `use-inpatient-admission-confirmation.jsx` (episode dibaca lewat
`GET /episodes/{id}` lalu `normalizeEpisodeDetail`). Di backend, baca-saja:
`InpEpisodeService.UpdateAdmissionAsync`, yang tidak menyentuh kolom `ExternalReferral*`, sehingga
`PUT /episodes/{id}` dari Konfirmasi tidak menghapus rujukan.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx` | `INPATIENT_EXTERNAL_REFERRAL_MAX_LENGTH` (250/200/150/1000, sama dengan kolom server) |
| `src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx` | `buildInpatientExternalReferralPayload` (hanya untuk Rujukan, trim, waktu lokal ke UTC, isian kosong menjadi `null`); `validateInpatientExternalReferralForm` (lengkap, panjang, tanggal tidak di masa depan); `buildInpatientAdmissionEpisodePayload` menerima `externalReferral` dan hanya menyertakannya bila ada |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | `normalizeExternalReferral` (camelCase dan PascalCase; `null` bila tanpa nomor) dan field `externalReferral` pada `normalizeEpisodeDetail` |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx` | Parameter `externalReferral` diteruskan ke payload `POST /episodes` dan masuk dependency `submit` |
| `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` | `externalReferral` dibentuk (memo) hanya pada jalur Pasien Baru dan Pasien Lama, lalu diberikan ke hook dokter |
| `src/components/view/health-services/inpatient-management/inpatient-admission-visit-type-step.jsx` | Validasi salinan diganti validator bersama; `maxLength` pada empat isian teks |
| `src/components/view/health-services/inpatient-management/inpatient-admission-visit-category-step.jsx` | Sama seperti langkah Jenis Kunjungan |
| `src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx` | Kartu `ConfirmationReferralSummary`, lebar penuh, tampil hanya bila episode membawa rujukan |
| `tests/unit/inpatient-admission-external-referral.test.mjs` (baru) | 7 tes: payload, validator, normalizer, jalur pengirim, batas panjang, dan kartu Konfirmasi |

### 3.3 Kepatuhan arsitektur frontend

- Alur view → hook → service tetap. Tidak ada endpoint, service, atau Redux slice baru, dan
  payload tetap dibentuk di `src/utils`.
- Rujukan masuk sebagai parameter hook dokter, sama seperti `admissionReferralId` dan
  `newbornAdmission`. Dengan begitu kedua titik panggil `submit` (langkah Dokter dan `prepare`
  di Pilih Bed) ikut membawanya.
- Validasi yang dulu disalin di dua langkah kini satu fungsi, supaya aturan layar tidak
  menyimpang lagi dari server.

**Gerbang base component**

```
UI GATE: 5 elemen — REUSE 4, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Batas panjang isian teks rujukan | `BaseInputField`, `BaseTextAreaField` prop `maxLength` | `base-form-control.jsx:344`, `:433` | REUSE | Prop existing |
| Pesan galat tanggal masa depan | `error` pada `BaseDateTimeField` + `InformationAlert` | Sudah dipakai langkah ini | REUSE | — |
| Kartu Rujukan Faskes Luar | Kelas `confirmationSummaryCard`, `confirmationCardHeader`, `infoRowList`/`infoRow` | Kartu Tempat Tidur pada berkas yang sama | REUSE | Pola kartu existing |
| Lencana "Rujukan Eksternal" | `StatusBadge` `info` | Label yang sama di langkah isian | REUSE | — |
| Kartu selebar grid | `referralFormFullWidth` (`grid-column: 1 / -1`) dirangkai dengan `confirmationSummaryCard` | `inpatient-admission.module.css:136` | COMPOSE | Lihat pilihan di bawah |

Pilihan untuk kartu selebar grid:

1. **(Rekomendasi)** Rangkai kelas existing `referralFormFullWidth`. Tidak ada CSS baru dan
   tidak ada risiko regresi. Biayanya hanya nama kelas yang berasal dari form, bukan dari kartu.
2. Tambah kelas baru `confirmationSummaryCardWide`. Namanya lebih tepat, tetapi menambah CSS
   yang isinya sama dengan kelas yang sudah ada.
3. Biarkan kartu selebar satu kolom. Grid dua kolom menyisakan sel kosong di sebelahnya, dan
   diagnosa panjang terlipat sempit.

Yang dipakai: pilihan 1. Tidak mengubah perilaku default base component mana pun.

Modul referensi visual: kartu Tempat Tidur dan kartu Penjamin pada langkah Konfirmasi yang sama.

---

## 4. State yang ditangani di layar

| State | Penanganan |
| --- | --- |
| Isian belum lengkap | Galat per isian dan "Seluruh data rujukan faskes luar wajib diisi lengkap (…)" (kalimat lama dipertahankan) |
| Tanggal di masa depan atau tidak valid | Galat pada isian tanggal; ringkasan "Periksa kembali data rujukan faskes luar yang ditandai." |
| Isian terlalu panjang | Dicegah `maxLength`; validator tetap menolak bila lolos |
| Server menolak (`400`) | Kalimat server diteruskan oleh jalur galat `submit` yang sudah ada di langkah Pilih Bed |
| Episode tanpa rujukan | Kartu tidak dirender |
| Rujukan tanpa dokter perujuk (data server) | Baris tampil "-" |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuka admisi; membawa `externalReferral` bila jenis kunjungan Rujukan | `InpatientEpisode : Create` |
| `GET` | `/{id}` | Detail episode; `externalReferral` dibaca untuk kartu Konfirmasi | `InpatientEpisode : Read` |

Bentuk `externalReferral` yang dikirim: `referralNumber`, `referralDateTime` (ISO UTC),
`referralFacilityName`, `referralDoctorName`, `referralDiagnosis`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 8 berkas source dan 1 berkas tes | 0 error, 0 warning | `PASS` | Keluaran ESLint |
| `npm run lint:errors` (seluruh repository) | Selesai tanpa error | `PASS` | Exit code 0 |
| Tes baru `inpatient-admission-external-referral.test.mjs` | 7/7 lulus | `PASS` | `node --test` |
| `npm run test:unit` (seluruh suite) | 3631/3674 lulus; 43 gagal | `EXISTING` | Daftar kegagalan identik dengan run `FIX-EPS-039` (45 baris, `diff` kosong); tidak ada kegagalan baru |
| `npm run build` | Exit 0; `prepare-standalone` siap | `PASS` | Dijalankan berdampingan dengan `next dev` milik user |
| Render SSR komponen asli, data samaran | 4/4 lulus: K1 Konfirmasi dengan rujukan (respons PascalCase, UTC `02:30Z` tampil "23/10/2026, 09.30" di `Asia/Jakarta`); K2 tanpa rujukan, kartu tidak ada; K3/K4 kedua langkah isian memasang `maxLength` 250/200/150/1000 | `PASS` | Skrip render di folder scratch sesi |
| Grep anti-regresi checklist UI pada baris `+` berkas task ini | 0 inline style, 0 warna/ukuran literal, 0 kontrol native, 0 UUID | `PASS` | `git diff -U0` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-admission-external-referral.test.mjs — PASS`

`MANUAL TEST: NOT FEASIBLE` — agent tidak dapat login ke aplikasi, dan migration
`20261023062116_AddInpEpisodeExternalReferral` belum diterapkan, sehingga `POST /episodes` dengan
`externalReferral` akan gagal di database sampai pemilik menjalankan `dotnet ef database update`
dan memulai ulang backend.

Skenario uji layar untuk pemilik:

| No | Skenario | Hasil yang diharapkan |
| ---: | --- | --- |
| R1 | Pasien Baru → Rujukan, isi lengkap, lanjut sampai Konfirmasi | Kartu Rujukan Faskes Luar menampilkan kelima isian, jam sama dengan yang diketik |
| R2 | Pasien Lama → Rujukan, sama seperti R1 | Sama seperti R1 |
| R3 | Isi tanggal rujukan besok, tekan Lanjut | Ditolak dengan kalimat tanggal masa depan |
| R4 | Kunjungan Umum sampai Konfirmasi | Tidak ada kartu rujukan; tab Network: payload `POST /episodes` tanpa `externalReferral` |
| R5 | R1 lalu F5 di Konfirmasi, ubah catatan, Simpan | Rujukan tetap tampil sesudah `PUT /episodes/{id}` |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (plan repair `FIX-EPS-039-03`) | Status | Bukti |
| --- | --- | --- |
| AC1 — `POST` membawa kelima field, `GET` mengembalikannya, Konfirmasi menampilkannya | Terpenuhi di source; belum diuji bolak-balik dengan server | Tes payload dan normalizer; render K1 |
| AC2 — tanpa rujukan, payload tanpa field rujukan dan perilaku lama tidak berubah | Terpenuhi | Tes "POST /episodes membawa rujukan hanya bila ada"; render K2 |
| AC3 — rujukan tidak lengkap atau bertanggal di masa depan → `400` berbahasa Indonesia | Terpenuhi di layar (dicegah sebelum kirim) dan di server (`FIX-EPS-039-03-BE`); runtime server belum | Tes validator |
| AC4 — migration tersedia; database diperbarui pemilik | Bagian backend; berkas tersedia, penerapan menunggu pemilik | [`FIX-EPS-039-03-BE`](../backend/FIX-EPS-039-03-BE.md) |

**Definition of Done yang belum terpenuhi:**
- uji layar R1–R5;
- penerapan migration;
- revisi dokumen hulu `03-frontend-architecture.md` (plan repair bagian 7), yang di luar wewenang
  task ini.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari berkas task ini |
| Masalah yang diketahui | (1) Rujukan belum dapat dikoreksi sesudah episode dibuat, karena `PUT /episodes/{id}` tidak membawanya (sama dengan catatan laporan backend). (2) Layar mewajibkan dokter perujuk, sedangkan server menganggapnya opsional. Layar sengaja lebih ketat sesuai `RWI-DEC-269`. (3) Bila jam komputer petugas lebih cepat dari server lebih dari 5 menit, rujukan "sekarang" bisa lolos di layar tetapi ditolak server dengan kalimat yang sama |
| Dependency backend | `FIX-EPS-039-03-BE`: source selesai, migration belum diterapkan |
| Perubahan sampingan | `NONE` di repository. Skrip render ada di folder scratch sesi |
| Interupsi | Konteks sesi diringkas di tengah task. Pekerjaan dilanjutkan dari `git diff` (dua suntingan util sudah ada) tanpa penggandaan |
| Status Git | Frontend `HamzahV2` @ `8f198ec2b`: berkas task ini 8 ` M` dan 1 `??` (lihat 3.2). Perubahan pihak lain yang tidak disentuh: `nursing-billing-section.jsx` + `.module.css`, `inpatient-room-transfer-report-view.jsx`, `inpatient-newborn-admission-steps.jsx`, `tests/unit/inpatient-newborn-admission.test.mjs`. Backend `MHamzah` @ `488faa22`: laporan ini `??`; register plan repair 039, issue 039, dan baris FIX-03 di `FIX-EPS-039.md` ` M`. Perubahan dokumen pihak lain yang tidak disentuh: `Sisa-Pekerjaan-Rawat-Inap.md`, `Revisi-Rawat-Inap.md`, dan roadmap `backend-roadmap-v2`, `-admisi-transfer-igd`, `-bed-management`, `-realtime-bed-selection`, `-finishing` |
| Langkah berikutnya | Pemilik menjalankan `dotnet ef database update`, memulai ulang backend, lalu menguji R1–R5 dan S1–S10 plan repair 039 |
