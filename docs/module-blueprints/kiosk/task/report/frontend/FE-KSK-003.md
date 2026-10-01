# Laporan Perubahan Frontend — `FE-KSK-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-003` |
| Judul | Service, helper, dan konstanta lookup |
| Slice | EPIC KSK-01/02 — Cek No. RM, gelombang `MVP-1`, gelombang eksekusi 1 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-003` |
| Trace | `FR-KSK-011`; `KSK-DSN-009`; `KSK-DEC-018`; `KSK-INV-002/009`; `contracts/api-contract.md` §Kiosk Patient Lookup; `contracts/validation-matrix.md` §1, §2, §4 |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30) |
| Wewenang UI | `NOT APPLICABLE` — task ini tidak membuat atau mengubah layar; teks layar hanya disalin dari `validation-matrix.md` §2 ke konstanta untuk dipakai `FE-KSK-004` |
| Dependency | `BE-KSK-001` ✅, `BE-KSK-002` ✅ (30 Sep 2026) |
| Klasifikasi | `LIGHT` — 3 berkas baru; tanpa layar, route, Redux, atau perubahan berkas existing |
| Task mode | `FRONTEND` — dipilih pengguna 2026-09-30 ("FE-KSK-003 (Recommended)") |
| Target tulis | `V2QuilvianSystemFrontendDev`: tiga berkas baru di kartu task. Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4ec51b0bf` (branch `sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `051a658a` (branch `sukmagp`) + perubahan `BE-KSK-003` yang belum di-commit (tidak menyentuh endpoint lookup) |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 5 dari 5 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

Endpoint `POST kiosk-patient-lookups` sudah tersedia di backend (`BE-KSK-001`, `BE-KSK-002`), tetapi frontend belum punya satu pun kode yang memanggilnya. Ketiga berkas pada kartu task belum ada.

Frontend sudah punya normalizer HP dan identitas bersama di `src/utils/shared/input-normalizer-utils.jsx`, tetapi aturannya berbeda dari kontrak Kiosk:

| Isian | Normalizer bersama | Kontrak Kiosk (`validation-matrix.md` §1) |
| --- | --- | --- |
| `12345` | `+6212345` (kode negara selalu ditambahkan), dinilai sah bila ≥ 8 digit | `12345`, **tidak sah** karena tidak diawali `62` |
| Panjang minimum HP | 8 digit | 9 digit setelah dinormalkan |

Karena itu normalizer bersama tidak dipakai. Bila dipakai, layar akan mengirim nomor yang pasti ditolak backend.

---

## 2. Proses bisnis dari sisi pengguna

Task ini belum menambah layar. Yang disiapkan adalah "mesin" yang nanti dipakai layar Cek No. RM (`FE-KSK-004`) dan pencarian Step 1 Pasien Lama (`FE-KSK-006`):

1. Pasien mengetik nomor di Kiosk. Helper menentukan jenis isiannya: 16 digit → No. KTP; diawali `0`/`62`/`+62` dengan 9–15 digit → No. HP; `99-99-99-99` atau tepat 8 digit → No. RM; minimal 2 huruf → nama.
2. Sebelum dikirim, helper memeriksa isian dengan aturan yang **sama persis** dengan backend. Contohnya, KTP 15 digit langsung ditolak dengan "Nomor KTP harus terdiri dari 16 digit." tanpa request.
3. Service mengirim `searchType` (angka) dan nomor ke backend, dengan batas waktu 20 detik dan bisa dibatalkan.
4. Hasilnya salah satu dari empat: ditemukan, belum terdaftar, perlu verifikasi (cocok ganda), atau hubungi petugas.

Jalur tidak normal dilempar sebagai error bertipe, dan **tidak pernah** dianggap "Pasien Belum Terdaftar":

| Kejadian | Jenis error | Pesan (dari kontrak) |
| --- | --- | --- |
| Isian ditolak backend (`400`) | `VALIDATION` | Pesan backend, misalnya "Nomor KTP harus terdiri dari 16 digit." |
| Sesi perangkat habis (`401`) | `UNAUTHORIZED` | "Sesi perangkat Kiosk telah berakhir. Silakan masuk kembali." |
| Akun bukan perangkat Kiosk (`403`) | `FORBIDDEN` | "Perangkat tidak berwenang." |
| Lebih dari 10 pencarian per menit (`429`) | `RATE_LIMITED` (+ detik `Retry-After`) | "Terlalu banyak percobaan. Silakan coba lagi sebentar." |
| Server error (`5xx`) | `SERVER` | "Data pasien belum dapat diperiksa. Silakan coba kembali." |
| Lewat 20 detik | `TIMEOUT` | sama dengan `SERVER` |
| Jaringan putus | `NETWORK` | sama dengan `SERVER` |
| Dibatalkan layar (pasien menekan cari lagi / keluar) | `ABORTED` | kosong — layar cukup mengabaikannya |
| Respons `200` yang bentuknya rusak (misalnya `result` di luar 1–4) | `INVALID_RESPONSE` | sama dengan `SERVER` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md` dan `REPORT_TEMPLATE.md` (suite skill); kartu task; `03-frontend-architecture.md` §4; `02-backend-architecture.md` `KSK-DSN-009`; `contracts/api-contract.md`; `contracts/validation-matrix.md`; `KioskPatientLookupService.Normalize` (backend, sebagai acuan aturan); `src/lib/axiosInstance/InstanceAxios.jsx` (interceptor respons); `src/utils/shared/input-normalizer-utils.jsx`; `src/lib/helpers/kiosk/registration/kiosk-new-patient-registration.helpers.jsx`; `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js`; `src/lib/services/health-services/billing-management/encounter-billing-summary.service.js` (pola service + `signal`); `src/lib/constants/kiosk/registration/kiosk-old-patient-service-target.constants.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/kiosk/registration/kiosk-medical-record-check.constants.js` (baru) | Endpoint, timeout 20 detik, enum angka `searchType`/`result`, `nextAction`, batas panjang, jenis klasifikasi input, jenis error, pesan `validation-matrix.md` §1, pesan kode status, dan teks judul/subteks/tombol §2. Semua koleksi di-`Object.freeze` |
| `src/lib/helpers/kiosk/registration/kiosk-patient-lookup.helpers.js` (baru) | Fungsi murni `hasForbiddenLookupCharacters`, `normalizeIdentityNumber`, `normalizePhoneNumber`, `validateIdentityNumber`, `validatePhoneNumber`, `validateKioskPatientLookupInput`, `classifyIdentificationInput` |
| `src/lib/services/kiosk/registration/kiosk-patient-lookup.service.js` (baru) | `lookupKioskPatient({ searchType, value, signal })` lewat `InstanceAxios`; kelas `KioskPatientLookupError` dan `isKioskPatientLookupError`; validasi bentuk respons |

Tidak ada berkas existing yang diubah.

### 3.3 Kepatuhan arsitektur frontend

- **HTTP:** memakai `InstanceAxios` existing; tidak ada instance Axios baru, dan token tidak dibaca sendiri. Endpoint berasal dari konstanta domain.
- **Penempatan:** mengikuti `03-frontend-architecture.md` yang disetujui (`src/lib/helpers/kiosk/registration/`). Folder ini sudah dipakai modul Kiosk (`kiosk-new-patient-*.helpers.jsx`). Aturan umum menaruh fungsi murni di `src/utils/`; selisih ini berasal dari desain yang disetujui dan dicatat, bukan diubah.
- **Pola baru:** error bertipe (`KioskPatientLookupError`). Pola ini dibutuhkan AC 3; service kiosk lain hanya meneruskan error Axios mentah.
- **Duplikasi yang disengaja:** `normalizePhoneNumber` tidak memakai normalizer bersama (lihat §1). Ekstraksi digit ditulis ulang satu baris dan tidak memakai `normalizeDigitsOnly` agar helper tetap bebas dependensi `.jsx` dan meniru `char.IsDigit` backend.
- **Klasifikasi input:** urutannya KTP → No. RM → HP → nama. No. RM diperiksa sebelum HP supaya `00001234` (8 digit) tidak pernah terbaca sebagai HP `620001234`.
- **Privasi:** nilai pencarian tidak pernah masuk ke `console`, pesan error, atau objek error.
- **UI GATE:** `NOT APPLICABLE` — tidak ada JSX, CSS, atau elemen layar, jadi gerbang keputusan base component dan `ui-consistency-checklist.md` tidak berlaku.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | `NOT APPLICABLE` pada task ini. Teks tombol "Memeriksa Data..." disediakan di konstanta untuk `FE-KSK-004` |
| Kosong | `NOT APPLICABLE`. Isian kosong ditolak helper dengan "Nomor wajib diisi." |
| Gagal | `NOT APPLICABLE` di layar. Service melempar error bertipe; teks "Data Belum Dapat Diperiksa" + "Coba Lagi" disediakan di konstanta |
| Tanpa hak akses | `NOT APPLICABLE` di layar. `403` → `FORBIDDEN` "Perangkat tidak berwenang."; `InstanceAxios` existing juga memanggil notifikasi akses ditolak globalnya |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Kiosk Patient Lookup

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/kiosk-patient-lookups` | Mencari pasien dari No. KTP / No. HP (kartu asuransi / member didukung service untuk `FE-KSK-007`) | Policy `KioskRead` (akun perangkat Kiosk); rate limit 10/menit/perangkat |

Request `{ "searchType": 1, "value": "9999000000000007" }`. Response `{ result, nextAction, patient }` sesuai kontrak; service mengembalikannya ditambah `message` dari server.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` untuk ketiga berkas | Exit `0`, 0 error, 0 warning | `PASS` | Keluaran perintah |
| `grep "console\."` pada ketiga berkas | 0 temuan | `PASS` | Keluaran perintah |
| `npm run build` | Exit `0` (2 menit 27 detik), postbuild standalone berhasil. Ketiga berkas baru belum di-import di mana pun, jadi build ini membuktikan tidak ada regresi, bukan kompilasi berkas baru; kompilasi berkasnya dibuktikan eslint dan eksekusi node | `PASS` | Keluaran `next build` |
| Skrip node sekali pakai di scratchpad (bukan folder test): helper + service dengan `InstanceAxios` tiruan | 45/45 PASS | `PASS` | Tabel A/B/C di bawah |
| Panggilan nyata ke BE dev lewat service yang sama (axios 1.18.1 milik repo FE menggantikan `InstanceAxios`) | 9/9 sesuai harapan | `PASS` | Tabel R di bawah |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; pembuktian fungsi murni memakai skrip node sekali pakai di scratchpad sesuai kartu task`

**A — Normalisasi dan validasi (`validation-matrix.md` §1)**

| Isian | Hasil |
| --- | --- |
| `081234567890` / `+62 812-3456-7890` / `0812-3456-7890` | Sah → `6281234567890` |
| `021 555 1234` | Sah → `62215551234` (telepon rumah) |
| `12345` | Dinormalkan `12345`; tidak sah → "Nomor HP tidak valid. Contoh: 081234567890." |
| `0812abc` | "Nomor HP hanya boleh berisi angka." |
| `3201 0101 0101 0001` | Sah → `3201010101010001` |
| KTP 15 digit / berhuruf | "Nomor KTP harus terdiri dari 16 digit." |
| Spasi saja | "Nomor wajib diisi." |
| `<script>`, karakter kontrol, lebih dari 32 karakter | "Isian mengandung karakter yang tidak diizinkan." |
| `searchType` 7 / kosong | "Pilih cara pencarian terlebih dahulu." |

**B — Klasifikasi input Step 1 (§4)**

| Isian | Jenis | `searchType` |
| --- | --- | --- |
| `3201010101010001`, `0812345678901234` (16 digit) | KTP | `1` |
| `081234567890`, `+62 812 3456 7890`, `6281234567890` | HP → `6281234567890` | `2` |
| `00-00-12-34`, `00001234` | No. RM | — |
| `Budi Santoso` | Nama | — |
| `B`, `12345`, `0812`, spasi saja | Tidak dikenali | — |
| `<b>Budi` | Tidak sah | — |

**C — Error bertipe (`InstanceAxios` tiruan)**: `429` → `RATE_LIMITED` + `retryAfterSeconds 60`; `500`/`503` → `SERVER`; `ECONNABORTED` → `TIMEOUT`; `ERR_CANCELED` → `ABORTED`; tanpa respons → `NETWORK`; `401` → `UNAUTHORIZED`; `403` → `FORBIDDEN`; `400` → `VALIDATION` dengan pesan server; `200` dengan `result 9` atau tanpa data → `INVALID_RESPONSE`; `200` `result 2` → dikembalikan sebagai hasil (satu-satunya jalur "belum terdaftar"); URL, body angka, timeout `20000`, dan `signal` diteruskan apa adanya.

**R — Panggilan nyata ke BE dev**

Lingkungan: backend dari `bin/Release` build `BE-KSK-003`, port lokal 5199, DB `QuilvianNewDevSukma`. Akun: seed SuperAdmin (lolos policy `KioskRead`; kredensial tidak dicetak). Password akun perangkat Kiosk tidak tersedia di sesi ini. Atas izin pengguna, pasien samaran `KSKTEST-RM-07` diberi No. KTP `9999000000000007` selama uji, lalu dikembalikan ke `NULL`.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| R1 | `classifyIdentificationInput("9999 0000 0000 0007")` lalu `lookupKioskPatient` | Klasifikasi KTP `searchType 1`; `result 1`, `EXISTING_PATIENT_REGISTRATION`, `patient.patientId = b5b5b5b5-…0007`, 7 field kartu | `PASS` |
| R2 | KTP tidak terdaftar | `result 2`, `NEW_PATIENT_REGISTRATION`, `patient null` | `PASS` |
| R3 | KTP 15 digit dikirim langsung (validasi FE sengaja dilewati) | `VALIDATION`, `400`, "Nomor KTP harus terdiri dari 16 digit." | `PASS` |
| R4 | `signal` sudah dibatalkan | `ABORTED` — kode `ERR_CANCELED` axios asli terpetakan | `PASS` |
| R5 | Server TCP lokal yang tidak pernah menjawab, batas 500 ms | `TIMEOUT` setelah 507 ms — kode timeout axios asli terpetakan | `PASS` |
| R5b | Port tertutup | `NETWORK` | `PASS` |
| R6 | Tanpa token | `UNAUTHORIZED`, `401` | `PASS` |
| R7 | Lookup beruntun sampai rate limit | Lookup ke-4 pada run ini (ke-10 lebih dalam menit yang sama, termasuk run sebelumnya) → `RATE_LIMITED`, `429`, `retryAfterSeconds 60` | `PASS` |
| R-akhir | Keadaan DB | `IdentityNumber` P7 kembali `NULL`; `MstPatient` 17, `RegPatientEncounter` 173 (tidak berubah); nilai KTP uji muncul 0 kali di log server | `PASS` |

Percobaan timeout pertama memakai batas 1 ms terhadap backend lokal dan tidak memicu timeout, karena batas axios di Node adalah batas idle socket, bukan batas total. Skenario diganti dengan server yang diam (R5).

Uji manual: `NOT APPLICABLE` — task ini tidak memiliki layar. Layar diuji di `FE-KSK-004`.

**Tidak dijalankan:** `npm run lint` untuk seluruh repository (hanya ketiga berkas task yang di-lint, sesuai larangan merapikan lint di luar cakupan); uji browser (tidak ada layar).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Tabel contoh normalisasi `validation-matrix.md` §1 menghasilkan keluaran yang sama | Terpenuhi | §6 tabel A (A1–A16) |
| 2. `classifyIdentificationInput` memberi KTP / HP / RM / nama sesuai tabel §4 | Terpenuhi | §6 tabel B (B1–B13), R1 |
| 3. Service membedakan `429`, `5xx`, timeout, dan abort sebagai error bertipe (bukan `NotFound`) | Terpenuhi | C1–C11 (tiruan); R4, R5, R7 (axios asli + BE dev) |
| 4. Tidak ada `console.log` | Terpenuhi | `grep` 0 temuan; lint PASS |
| 5. Panggilan nyata ke BE dev untuk satu KTP samaran menghasilkan `result = 1` | Terpenuhi | R1 |
| DoD: AC 1–5 di `FE-KSK-003.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `InstanceAxios` existing memanggil notifikasi akses ditolak global pada `403`. Layar `FE-KSK-004` perlu memastikan notifikasi itu tidak bertumpuk dengan pesan "Perangkat tidak berwenang" |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` — `BE-KSK-001/002` sudah tersedia |
| Perubahan sampingan | `NONE`. Artefak `.next/` dari build (gitignored) |
| Interupsi | `NONE` |
| Status Git | Frontend: `?? src/lib/constants/kiosk/registration/kiosk-medical-record-check.constants.js`, `?? src/lib/helpers/kiosk/registration/kiosk-patient-lookup.helpers.js`, `?? src/lib/services/kiosk/registration/kiosk-patient-lookup.service.js` |
| Langkah berikutnya | `FE-KSK-004` (layar Cek No. RM) atau `FE-KSK-006` (Step 1 tanpa KTP/HP di URL) — keduanya kini terbuka |
