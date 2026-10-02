# Laporan Perubahan Frontend — `FE-KSK-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-006` |
| Judul | Privasi Step 1 (input ketik) |
| Slice | EPIC KSK-03 — Pendaftaran Pasien Lama, gelombang `MVP-2`, gelombang eksekusi 2 |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-006` |
| Trace | `FR-KSK-023/024`; `KSK-DEC-016`; `KSK-DSN-009`; `KSK-INV-002/009`; PRIV-2/3; `contracts/validation-matrix.md` §2, §4; `contracts/api-contract.md` §Kiosk Patient Lookup |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30) |
| Wewenang UI | Tidak ada elemen UI baru: arahan hasil memakai popup Step 1 yang sudah ada, teks dari `validation-matrix.md` §2 |
| Dependency | `FE-KSK-003` ✅; `BE-KSK-001` ✅ |
| Klasifikasi | `LIGHT` — 2 berkas diperbarui; tanpa layar, route, atau state baru |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("oke lanjutkan" setelah `FE-KSK-005`) |
| Target tulis | `V2QuilvianSystemFrontendDev`: dua berkas pada kartu task. Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `4ec51b0bf` (branch `sukmagpV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `051a658a` (branch `sukmagp`) + perubahan `BE-KSK-003` yang belum di-commit |
| Tanggal | 30 September 2026 |
| Status | ✅ SELESAI — 4 dari 4 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

1. Di Step 1 (Identifikasi) Pendaftaran Pasien Lama, semua isian ketik — termasuk No. KTP dan No. HP — dikirim lewat `GET …/patients?search=<nomor>`. Nomor itu ikut tercatat di URL: riwayat browser, log proxy, dan log server.
2. Service `kiosk-old-patient-registration.service.js` mencetak ke Console setiap kali kunjungan dibuat. Isinya URL request, diagnostik request (id pasien, dokter, jadwal, penjamin), Trace ID, dan **seluruh body respons** kunjungan. Bila gagal, isi yang sama dicetak lewat `console.error`.

---

## 2. Proses bisnis dari sisi pengguna

Pasien di Step 1 mengetik lalu menekan **Cari Pasien**. Isian dikenali seperti di layar Cek No. RM (`KSK-DSN-009`):

| Isian | Jalur pencarian | Contoh |
| --- | --- | --- |
| 16 digit | No. KTP → `POST kiosk-patient-lookups` (nomor di body) | `3201010101010001` |
| Diawali `0`/`62`/`+62`, 9–15 digit | No. HP → `POST kiosk-patient-lookups` | `0812-3456-7890` |
| `99-99-99-99` atau tepat 8 digit | No. RM → pencarian existing | `00-00-12-34` |
| Minimal 2 huruf | Nama → pencarian existing | `Budi Santoso` |

Hasil untuk KTP/HP:

| Hasil | Yang dilihat pasien |
| --- | --- |
| Ditemukan | Langsung ke **Review Data**. Detail pasien dimuat lewat id pasien, bukan lewat nomor |
| Belum terdaftar | Popup "Data pasien tidak ditemukan" (sama seperti sebelumnya) |
| HP dipakai lebih dari satu pasien | Popup "Data Perlu Diverifikasi" + "Nomor HP ini terdaftar untuk lebih dari satu pasien. Silakan cari memakai No. KTP atau hubungi petugas pendaftaran." — **tanpa daftar pasien** |
| KTP cocok ganda / perlu petugas | Popup "Data Perlu Diverifikasi" + "Silakan hubungi petugas pendaftaran." — tanpa daftar pasien, tanpa alasan |
| Terlalu banyak percobaan (`429`) | Popup "Data Belum Dapat Diperiksa" + "Terlalu banyak percobaan. Silakan coba lagi sebentar." |
| Gagal teknis (`5xx`, timeout, jaringan) | Popup "Data Belum Dapat Diperiksa" + "Data pasien belum dapat diperiksa. Silakan coba kembali." — tidak pernah "tidak ditemukan" |

Pencarian nama dan No. RM tidak berubah sama sekali.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `03-frontend-architecture.md` §3 `FE-KSK-03`; `validation-matrix.md` §2, §4; `kiosk-old-patient-step-find.jsx` (jalur ketik `runSearch`, jalur pindai `processQrValue`); `kiosk-old-patient-registration.service.js`; seluruh modul yang di-import alur Pasien Lama (hanya service ini yang memanggil Console).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx` | `runSearch` mengklasifikasi isian dengan `classifyIdentificationInput` (`FE-KSK-003`). KTP/HP diarahkan ke fungsi baru `runLookupSearch` (`lookupKioskPatient` → `getOldPatientById` → Review, atau popup arahan). Jenis lain tetap ke jalur existing tanpa perubahan |
| `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` | `requestJson`: seluruh `console.groupCollapsed/log/error/groupEnd` dihapus. Diagnostik tetap menempel pada objek error (`error.requestDiagnostic`) untuk pemanggil; perilaku request/response tidak berubah |

### 3.3 Kepatuhan arsitektur frontend

- Tidak ada HTTP baru di view: pencarian lookup memakai service `FE-KSK-003` (`InstanceAxios`).
- Popup arahan memakai popup Step 1 existing (`kioskPopupOverlay` + "Mengerti") — `UI GATE: N/A — tidak ada elemen UI baru; arahan memakai popup Step 1 yang sudah ada (REUSE)`.
- **Tidak termasuk** (sesuai kartu): jalur pindai kartu dan pembentukan sesi kiosk tidak diubah. Catatan: bila hasil pindai gagal membentuk sesi, alur pindai memanggil `runSearch` sebagai cadangan. Bila kata kuncinya kebetulan KTP/HP, cadangan itu kini juga lewat `POST`. Efeknya hanya menambah privasi; jalur pindainya sendiri tetap.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol "Mencari Data Pasien..." terkunci (existing) |
| Kosong | Popup "Data pasien tidak ditemukan" (existing) |
| Gagal | Popup "Data Belum Dapat Diperiksa" + pesan teknis/rate limit |
| Tanpa hak akses | Penjaga akses kiosk existing; `403` → popup "Perangkat tidak berwenang." |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Kiosk Patient Lookup

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/kiosk-patient-lookups` | No. KTP / No. HP yang diketik di Step 1 | Policy `KioskRead`; rate limit 10/menit/perangkat |

#### Health Services / Patient Management / Patient (existing, tidak berubah)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients?search=` | Nama dan No. RM | Existing |
| `GET` | `/v1/health-services/patient-management/master-data/patients/{id}` | Detail pasien setelah lookup ditemukan | Existing |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` untuk 2 berkas task | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `grep console.` pada seluruh modul alur Pasien Lama (service, 11 view step, hook, rules, hook timeout) | 0 temuan | `PASS` | Keluaran grep |
| `npm run build` | Exit `0`, "Compiled successfully in 71s" | `PASS` | Keluaran `next build` |
| Uji browser Playwright (Network + Console) | 24/24 PASS | `PASS` | Tabel di bawah + screenshot |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser

Lingkungan: `next dev` port 3000 → backend `bin/Release` di `https://localhost:7184` → DB `QuilvianNewDevSukma`. Login akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat).

Data:
- **Backend asli:** pasien samaran `KSKTEST-RM-07` diberi No. KTP `9999000000000007` selama uji, lalu kembali `NULL`. Dipakai untuk KTP ditemukan, HP tidak terdaftar, nama, dan No. RM tidak terdaftar.
- **Respons disimulasikan (`page.route`):** cocok ganda, hubungi petugas, `429`, dan `500`.

Seluruh request `/api/` dan pesan Console direkam.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| AC1a | Ketik KTP P7 | Tepat 1 `POST kiosk-patient-lookups`, lalu `GET patients/{id}`; KTP muncul di URL request **0 kali**; Review Data berisi P7 | `PASS` |
| AC1b | Ketik HP `081299990007` (tidak terdaftar) | Tepat 1 `POST`; HP (mentah maupun `62…`) di URL request 0 kali; popup "Data pasien tidak ditemukan" | `PASS` |
| AC2a | Ketik nama lengkap P7 | `GET …?search=KSKTEST…` existing, tanpa `POST` lookup; Review Data berisi P7 | `PASS` |
| AC2b | Ketik No. RM `99-99-99-98` | `GET …?search=99-99-99-98` existing; popup tidak ditemukan | `PASS` |
| AC2c | Ketik No. RM 8 digit `00001234` | `GET` existing, bukan lookup HP | `PASS` |
| AC3a | `result 3` + `USE_IDENTITY_NUMBER_OR_CONTACT_STAFF` | Popup judul + teks HP persis; tetap di Step 1; tanpa daftar pasien | `PASS` |
| AC3b | `result 3` + `CONTACT_STAFF` | Popup "Data Perlu Diverifikasi" + "Silakan hubungi petugas pendaftaran."; tanpa daftar pasien; tanpa kata meninggal/blokir/tidak aktif | `PASS` |
| AC3c | `result 4` | Sama dengan AC3b | `PASS` |
| EXTRA | `429` / `500` | "Data Belum Dapat Diperiksa" + pesan persis; bukan "tidak ditemukan" | `PASS` |
| AC4 | Console selama seluruh uji | 12 pesan, seluruhnya bawaan Next dev (`[HMR] connected`, info React DevTools). 0 pesan berisi URL request, diagnostik, Trace ID, `patient-encounters`, KTP, HP, nama, atau id P7 | `PASS` |
| R-akhir | DB | `MstPatient` 17, `RegPatientEncounter` 173, `TrxKioskScanSession` baru 0, KTP P7 `NULL`; nilai uji muncul 0 kali di log backend | `PASS` |

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis dengan rekaman Network dan Console.

**Tidak dijalankan:** submit kunjungan sampai Konfirmasi di browser (akan membuat kunjungan di DB). Jalur Console pada submit dibuktikan statis — seluruh pemanggilan Console di `requestJson` sudah dihapus dan grep 0 — bukan lewat runtime.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Mengetik KTP 16 digit atau HP → POST lookup; tidak ada GET berisi nilai itu di Network | Terpenuhi | AC1a, AC1b |
| 2. Nama dan No. RM tetap memakai pencarian existing dan hasilnya sama seperti sebelumnya | Terpenuhi | AC2a–c |
| 3. `MultipleMatch`/`ContactStaff` di Step 1 menampilkan arahan petugas, tanpa daftar pasien | Terpenuhi | AC3a–c |
| 4. Console browser tidak memuat URL, diagnostik, atau body respons pasien pada seluruh alur Pasien Lama | Terpenuhi — runtime untuk Step 1/Review; statis (grep 0) untuk submit kunjungan | AC4, grep |
| DoD: AC 1–4 di `FE-KSK-006.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | (1) Detail pasien diambil dua kali per pemilihan (`getOldPatientById` lalu `hydratePatient`), pola existing `selectAndGoReview`, tidak diubah. (2) Alur **Pasien Baru** masih mencetak URL, diagnostik, dan hasil kunjungan ke Console (`kiosk-new-patient-registration.service.js:412–436`, `kiosk-new-patient-step-payment.jsx:574/593`, `kiosk-new-patient-step-service.jsx:354/437`, `kiosk-new-patient-service.service.js:526`) — di luar cakupan task ini, perlu task tersendiri |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi agent terputus setelah uji browser selesai dan server dihentikan. Dilanjutkan dari keadaan terverifikasi (Git, DB, port), tanpa mengulang uji |
| Status Git | Frontend (task ini): ` M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx`, ` M src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js`, di samping perubahan `FE-KSK-003..005` yang belum di-commit |
| Langkah berikutnya | `FE-KSK-001` / `FE-KSK-002` (mandiri). `FE-KSK-007` kini hanya menunggu `KSK-OQ-004` |
