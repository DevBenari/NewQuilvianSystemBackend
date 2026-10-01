# Laporan Perubahan Frontend — `FE-KSK-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-004` |
| Judul | Layar Cek Nomor Rekam Medis dan handoff |
| Slice | EPIC KSK-02 — Cek No. RM, gelombang `MVP-1`, gelombang eksekusi 2 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-004` |
| Trace | `FR-KSK-010..015`; `KSK-DEC-012`; `KSK-INV-002/009`; `KSK-VAL-001..008`; `contracts/api-contract.md` §Kiosk Patient Lookup; `contracts/validation-matrix.md` §1–2; `contracts/state-transition-matrix.md` §1; `03-frontend-architecture.md` §3 `FE-KSK-02` |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30) |
| Wewenang UI | Susunan layar dari wireframe `03-frontend-architecture.md` §3; teks persis `validation-matrix.md` §2; gaya visual = keputusan gerbang UI opsi A (Sukma Giri Pratama, 30 Sep 2026) |
| Dependency | `FE-KSK-003` ✅; `BE-KSK-001` ✅, `BE-KSK-002` ✅ |
| Klasifikasi | `MEDIUM` — 5 berkas baru + 2 berkas diperbarui; layar baru, slice Redux baru, tanpa perubahan base component |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("oke lanjutkan" setelah `FE-KSK-003`) |
| Target tulis | `V2QuilvianSystemFrontendDev`: berkas pada kartu task (+ konstanta route/status di berkas `FE-KSK-003`). Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4ec51b0bf` (branch `sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `051a658a` (branch `sukmagp`) + perubahan `BE-KSK-003` yang belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 9 dari 9 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

Tile "Cek Nomor Rekam Medis" di Beranda Kiosk hanya memunculkan pesan "Fitur cek nomor rekam medis akan dilanjutkan pada tahap berikutnya." Layar, hook, dan slice handoff belum ada. Service, helper, dan konstanta lookup sudah tersedia dari `FE-KSK-003`.

Dua temuan saat membaca dokumen:

1. **Konflik dokumen yang disetujui.** `03-frontend-architecture.md` §3 dan §5 menyebut "isian terkunci" selama pemeriksaan, sedangkan AC 6 kartu task meminta "mengubah isian membatalkan request berjalan". Keduanya tidak dapat berlaku bersamaan. AC 6 diikuti karena itulah kriteria selesai task: selama memeriksa, hanya tombol Cek yang terkunci (`KSK-VAL-008`), sedangkan isian dan pilihan metode tetap dapat diubah dan otomatis membatalkan request.
2. **Nama status kontrak.** `state-transition-matrix.md` memakai satu status `MULTIPLE_MATCH`. Teks "HP" atau "hubungi petugas" dipilih dari `nextAction` backend.

---

## 2. Proses bisnis dari sisi pengguna

1. Pasien menekan tile **Cek Nomor Rekam Medis** di Beranda Kiosk.
2. Pasien memilih **No. KTP** (bawaan) atau **No. HP**, lalu mengetik nomornya. Isian KTP hanya menerima angka, maksimal 16.
3. Pasien menekan **Cek Nomor Rekam Medis**. Isian yang tidak sah langsung ditolak tanpa menghubungi server. Contohnya, KTP 15 digit memunculkan "Nomor KTP harus terdiri dari 16 digit." di bawah isian.
4. Selama pemeriksaan, tombol berubah menjadi **Memeriksa Data...** dan tidak bisa ditekan lagi.
5. Hasil tampil di bawah form:

| Hasil | Yang dilihat pasien | Tombol |
| --- | --- | --- |
| Ditemukan | "Pasien Ditemukan" + Kartu Pasien (nama lengkap, No. RM, kode pasien, tipe, jenis kelamin, golongan darah) | "Lanjut Pendaftaran Pasien Lama" → pasien diteruskan ke Pasien Lama |
| Belum terdaftar | "Pasien Belum Terdaftar" + subteks | "Daftar Sebagai Pasien Baru" |
| HP dipakai lebih dari satu pasien | "Data Perlu Diverifikasi" + saran memakai KTP | "Cari dengan No. KTP" → metode pindah ke KTP, isian kosong |
| KTP cocok ganda / perlu petugas | "Data Perlu Diverifikasi" + "Silakan hubungi petugas pendaftaran." (tanpa alasan) | "Kembali ke Beranda" |
| Gagal teknis (`429`, `5xx`, lewat 20 detik, jaringan) | "Data Belum Dapat Diperiksa" + pesan | "Coba Lagi" — **tidak pernah** "Daftar Sebagai Pasien Baru" |

Jalur tidak normal:

- Pasien mengubah nomor saat pemeriksaan berjalan → pemeriksaan lama dibatalkan dan jawabannya dibuang.
- Pasien mengubah nomor setelah hasil tampil → hasil lama hilang.
- Sesi perangkat habis (`401`) → dialihkan ke login.
- Akun bukan Kiosk (`403`) → "Perangkat tidak berwenang." + "Kembali ke Beranda".
- Belum login → `/login?redirect=/kiosk/registration/medical-record-check`.
- Login bukan akun Kiosk → `/administrator`, sama seperti halaman kiosk lain.

**Keadaan antara (dicatat sesuai kartu task):** halaman Pasien Lama belum membaca handoff (baru di `FE-KSK-007`). Sampai saat itu, "Lanjut Pendaftaran Pasien Lama" membuka Pasien Lama dengan urutan lama tanpa pasien terisi; `patientId` sudah tersimpan di slice.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `03-frontend-architecture.md` §1–5; `contracts/api-contract.md`, `validation-matrix.md`, `state-transition-matrix.md`; `src/components/view/kiosk/kiosk-home-view.jsx` (tile + penjaga akses); `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` (pola penjaga); `src/components/view/kiosk/registration/doctor-schedule/kiosk-doctor-schedule-view.jsx` dan CSS-nya (referensi visual); `src/components/features/base-features/base-patient-card.jsx`, `base-button.jsx`, `base-text-field.jsx`, `information-alert.jsx`; `src/lib/state/store.jsx`; `billing-consumer-handoff-slice.jsx`; `src/app/globals.css` (token); aturan suite `frontend-architecture.md`, `base-component-decision-gate.md`, `design-tokens.md`, `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/kiosk/registration/medical-record-check/page.jsx` (baru) | Entry point + metadata judul, pola Jadwal Dokter |
| `src/components/view/kiosk/registration/medical-record-check/kiosk-medical-record-check-view.jsx` (baru) | Layar: top bar, pilihan metode (`role="radiogroup"`), isian, tombol Cek, panel hasil dengan `BasePatientCard` |
| `src/lib/hooks/kiosk/registration/medical-record-check/use-kiosk-medical-record-check.jsx` (baru) | Penjaga akses kiosk, validasi via helper `FE-KSK-003`, kunci kirim ganda (`ref` sinkron), batal-dan-buang saat isian berubah, pemetaan hasil/error ke teks §2, navigasi tanpa query, handoff |
| `src/lib/state/slice/health-services/registration-management/kiosk-patient-handoff-slice.jsx` (baru) | State `{ patientId }` saja; `patientId` harus berbentuk GUID; aksi `setHandoffPatient`, `consumeHandoffPatient`, `clearHandoff`; selector `selectKioskHandoffPatientId` |
| `src/style/kiosk/registration/kiosk-medical-record-check.module.css` (baru) | Gaya layar, hanya token global |
| `src/lib/state/store.jsx` | Reducer `kioskPatientHandoff` didaftarkan (store tanpa persistence) |
| `src/components/view/kiosk/kiosk-home-view.jsx` | Tile Cek No. RM: `notice` dihapus, `route` ditambahkan |
| `src/lib/constants/kiosk/registration/kiosk-medical-record-check.constants.js` (dari `FE-KSK-003`) | Ditambah `KIOSK_MEDICAL_RECORD_CHECK_ROUTES`, `..._STATUSES`, `..._METHOD_OPTIONS` |

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: `page → view → hook → service/Redux → InstanceAxios`. View tidak memanggil HTTP.
- Route di `src/app` hanya entry point dan metadata.
- `BasePatientCard` dipakai ulang apa adanya; tidak ada base component yang diubah.
- Penjaga akses meniru Beranda Kiosk dan Pasien Lama. Fungsi `isKioskUser` kini tersalin di lima tempat (sudah empat sebelum task ini) — technical debt yang dicatat, tidak dirapikan di task ini.
- Privasi: KTP/HP hanya berada di state lokal hook; tidak masuk URL, Redux, maupun console (dibuktikan uji PRIV).

**Gerbang keputusan base component**

| Kebutuhan UI | Kandidat base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Kartu pasien | `BasePatientCard` | Menerima langsung field respons lookup; sudah dipakai kiosk Cetak Kartu | REUSE | Dipakai apa adanya |
| Shell + top bar | `Hero` | Untuk halaman admin; 0 layar kiosk memakainya | NEW | Pola top bar Jadwal Dokter |
| Pilihan KTP / HP | `FilterSelect` | Dropdown, tidak ramah sentuh | NEW | Dua tombol segmen |
| Isian nomor | `BaseTextField` | Tinggi 45px, font 13px; 0 pemakaian di kiosk | NEW | Isian besar |
| Tombol Cek + aksi hasil | `BaseButton` | Ukuran terbesar 48px; 0 pemakaian di kiosk | NEW | Tombol besar kiosk |
| Panel hasil / gagal | `InformationAlert` | Gaya alert admin | NEW | Panel hasil kiosk |

`UI GATE: 6 elemen — REUSE 1, EXTEND 0, COMPOSE 0, WRAP 0, NEW 5` — kelima `NEW` diputuskan pengguna: **opsi A**, yaitu pola kiosk + CSS module baru yang hanya memakai token global (Sukma Giri Pratama, 30 Sep 2026). Opsi yang ditolak: B (salin palet `--schedule-*`, melanggar aturan token) dan C (base component admin, terlalu kecil untuk layar sentuh).

Konsekuensi opsi A: warna tosca layar ini memakai token global (`--color-primary`), sedikit berbeda dari tosca lokal Jadwal Dokter.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat (akses) | "Memeriksa akses perangkat..." |
| Memuat (lookup) | Tombol "Memeriksa Data..." terkunci |
| Kosong | Isian kosong ditolak: "Nomor wajib diisi." |
| Gagal | "Data Belum Dapat Diperiksa" + pesan + "Coba Lagi" |
| Tanpa hak akses | Belum login → login; bukan akun Kiosk → `/administrator`; `403` → "Perangkat tidak berwenang." |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Kiosk Patient Lookup

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/kiosk-patient-lookups` | Memeriksa No. KTP (`searchType 1`) / No. HP (`searchType 2`) | Policy `KioskRead`; rate limit 10/menit/perangkat |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` untuk 7 berkas task | 0 error; 1 warning `@next/next/no-img-element` di `kiosk-home-view.jsx:304` yang sudah ada sebelumnya (bukan baris task) | `PASS` | Keluaran perintah |
| `npm run build` | Exit `0`, "Compiled successfully in 77s", route `○ /kiosk/registration/medical-record-check` ikut terbentuk | `PASS` | Keluaran `next build` |
| Grep anti-regresi `ui-consistency-checklist.md` | 0 hex/rgb/hsl, 0 `!important`, 0 dark mode, 0 `<table>`, 0 inline style. Satu-satunya `px` adalah breakpoint `@media` (tidak dapat memakai `var()`). 4 `<button>` mentah dan 32 deklarasi font = bagian opsi A yang disetujui; seluruhnya token dan hanya menyasar class layar ini | `PASS` (dengan keputusan opsi A) | Keluaran grep |
| Uji browser Playwright (Chromium headless) | 30/30 PASS pada run ketiga | `PASS` | Tabel di bawah + 13 screenshot di scratchpad |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser dijalankan lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser

Lingkungan: `next dev` port 3000 → backend `bin/Release` di `https://localhost:7184` → DB `QuilvianNewDevSukma`. Login akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat). Untuk AC 9 dipakai akun seed SuperAdmin.

Data:
- **Backend asli:** pasien samaran `KSKTEST-RM-07` diberi No. KTP `9999000000000007` selama uji, lalu dikembalikan ke `NULL`. Dipakai untuk hasil ditemukan dan belum terdaftar.
- **Respons disimulasikan (`page.route`):** cocok ganda, hubungi petugas, `429`, `500`, dan timeout. Tujuannya agar data pasien nyata tidak diubah.

Redux dibaca lewat stub `__REDUX_DEVTOOLS_EXTENSION_COMPOSE__` sebagai pengganti Redux DevTools. Network direkam lewat event `request`/`requestfailed`.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| AC1 | Klik tile di Beranda | Membuka `/kiosk/registration/medical-record-check` tanpa query | `PASS` |
| AC2a | KTP 15 digit → Cek | "Nomor KTP harus terdiri dari 16 digit.", 0 request lookup | `PASS` |
| AC2b | HP `12345` → Cek | "Nomor HP tidak valid. Contoh: 081234567890.", 0 request | `PASS` |
| AC3a | KTP P7 (BE asli) | "Pasien Ditemukan" + Kartu Pasien "KSKTEST Karyawan PT Samaran" / "KSKTEST-RM-07" + "Lanjut Pendaftaran Pasien Lama" | `PASS` |
| AC3b | KTP tidak terdaftar (BE asli) | "Pasien Belum Terdaftar" + subteks persis + "Daftar Sebagai Pasien Baru" | `PASS` |
| AC3c | `result 3` + `USE_IDENTITY_NUMBER_OR_CONTACT_STAFF` | Judul + subteks HP persis + "Cari dengan No. KTP"; tanpa tombol Pasien Baru. Menekan tombol → metode KTP, isian kosong, hasil hilang | `PASS` |
| AC3d/e | `result 3` + `CONTACT_STAFF`; `result 4` | "Data Perlu Diverifikasi" + "Silakan hubungi petugas pendaftaran." + "Kembali ke Beranda"; tidak ada kata meninggal/blokir/tidak aktif | `PASS` |
| AC4a | `429` + `Retry-After` | "Data Belum Dapat Diperiksa" + "Terlalu banyak percobaan…" + "Coba Lagi"; tanpa Pasien Baru | `PASS` |
| AC4b | `500` | Judul + "Data pasien belum dapat diperiksa…" + "Coba Lagi"; tanpa Pasien Baru | `PASS` |
| AC4c | Server tidak menjawab | Tombol "Memeriksa Data..." terkunci; setelah 20,5 detik → ERROR + "Coba Lagi"; tanpa Pasien Baru | `PASS` |
| AC4d | "Coba Lagi" setelah gangguan hilang | Isian yang sama dikirim ke BE asli → "Pasien Belum Terdaftar" | `PASS` |
| AC5 | Dua klik sinkron + dobel klik saat respons tertunda 1,5 detik | Tepat 1 request | `PASS` |
| AC6a | Isian diubah saat request tertunda 3 detik | Request dibatalkan (`net::ERR_ABORTED`); jawaban lama tidak ditampilkan; tombol kembali "Cek Nomor Rekam Medis" | `PASS` |
| AC6b | Isian diubah setelah hasil tampil | Panel hasil hilang | `PASS` |
| AC7 | "Daftar Sebagai Pasien Baru" | `/kiosk/registration/new-patient`, query kosong | `PASS` |
| AC8 | "Lanjut Pendaftaran Pasien Lama" | `/kiosk/registration/old-patient`, query kosong; state `kioskPatientHandoff` = `{"patientId":"b5b5b5b5-…0007"}` persis | `PASS` |
| AC9a | Login SuperAdmin (bukan akun Kiosk) lalu buka layar | Dialihkan ke `/administrator` | `PASS` |
| AC9b | Tanpa login | `/login?redirect=/kiosk/registration/medical-record-check` | `PASS` |
| PRIV | Nilai KTP di URL request dan console | 0 kemunculan; juga 0 di log backend | `PASS` |
| R-akhir | Keadaan DB | `IdentityNumber` P7 kembali `NULL`; `MstPatient` 17, `RegPatientEncounter` 173 | `PASS` |

Catatan run:
- **Run pertama:** 27/30. Tiga kegagalan AC 6 berasal dari skrip uji. Setelah fokus berpindah, kursor berada di awal isian, jadi Backspace tidak menghapus apa pun. Diagnostik terpisah membuktikan mengetik dan `fill` memang membuang hasil. Skrip diperbaiki (tekan `End` dulu).
- **Run kedua:** berhenti di login karena timeout sesaat saat `next dev` mengompilasi ulang; login berikutnya normal.
- **Run ketiga:** 30/30.

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis di atas; pemeriksaan visual dilakukan pada screenshot idle, validasi, ditemukan, dan gagal `429`.

**Tidak dijalankan:** `npm run lint` untuk seluruh repository (hanya berkas task); uji pada perangkat kiosk fisik/layar sentuh; tampilan layar sempit hanya dijamin lewat `@media`, tidak difoto.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Tile Beranda membuka layar | Terpenuhi | AC1 |
| 2. Validasi tanpa request untuk KTP 15 digit / HP `12345` | Terpenuhi | AC2a, AC2b |
| 3. Lima hasil tampil dengan teks persis `validation-matrix.md` §2 | Terpenuhi | AC3a–e, AC4a–c |
| 4. `429`/`5xx`/timeout → `ERROR` + Coba Lagi, tanpa tombol Pasien Baru | Terpenuhi | AC4a–d |
| 5. Tekan Cek dua kali cepat → satu request | Terpenuhi | AC5 |
| 6. Mengubah isian membatalkan request berjalan dan membuang hasil lama | Terpenuhi — isian sengaja tidak dikunci (lihat §1) | AC6a, AC6b |
| 7. "Daftar Sebagai Pasien Baru" → `/kiosk/registration/new-patient` tanpa query | Terpenuhi | AC7 |
| 8. "Lanjut Pendaftaran Pasien Lama" menyimpan `patientId` ke slice lalu membuka Pasien Lama tanpa query | Terpenuhi | AC8 |
| 9. Akun non-kiosk dialihkan seperti halaman kiosk lain | Terpenuhi | AC9a, AC9b |
| DoD: AC 1–9 di `FE-KSK-004.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning `<img>` lama di `kiosk-home-view.jsx:304` |
| Masalah yang diketahui | (1) Pasien Lama belum membaca handoff sampai `FE-KSK-007`. (2) Selisih "isian terkunci" di `03-frontend-architecture.md` §3/§5 vs AC 6; perlu dirapikan pemilik blueprint bila AC 6 dipertahankan. (3) `isKioskUser` tersalin di lima tempat |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE`. Artefak `.next/` (gitignored) |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M src/components/view/kiosk/kiosk-home-view.jsx`, ` M src/lib/state/store.jsx`, `?? src/app/kiosk/registration/medical-record-check/`, `?? src/components/view/kiosk/registration/medical-record-check/`, `?? src/lib/hooks/kiosk/registration/medical-record-check/`, `?? src/lib/state/slice/health-services/registration-management/kiosk-patient-handoff-slice.jsx`, `?? src/style/kiosk/registration/kiosk-medical-record-check.module.css` (+ tiga berkas `FE-KSK-003`) |
| Langkah berikutnya | `FE-KSK-005` (inactivity timeout) atau `FE-KSK-006` (Step 1 tanpa KTP/HP di URL) |
