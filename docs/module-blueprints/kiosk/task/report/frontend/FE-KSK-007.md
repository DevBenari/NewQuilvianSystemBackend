# Laporan Perubahan Frontend — `FE-KSK-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-007` |
| Judul | Urutan 8 step dan sesi kiosk di Step 3 |
| Slice | EPIC KSK-03 — Pendaftaran Pasien Lama, gelombang `MVP-2` |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-007` |
| Trace | `FR-KSK-020..022`; `KSK-DEC-002/012/014`; `KSK-DSN-004/007`; `KSK-INV-007/008`; `KSK-VAL-012`; `contracts/state-transition-matrix.md` §2; api `scan-result` |
| Contract version | `KSK-CONTRACT-v1` — `approved` (Sukma Giri Pratama, 2026-09-30); kontrak `scan-result` tidak berubah |
| Wewenang UI | Urutan step dari PRD (`KSK-DEC-002/014`); kartu "Pasien Terpilih" disusun dari class Step 1 existing + `BasePatientCard` (COMPOSE, tanpa CSS baru) |
| Dependency | `FE-KSK-004` ✅, `FE-KSK-006` ✅, `KSK-OQ-004` ✅ (ditutup 30 Sep 2026 — Amendment Pass putaran 20 blueprint Laboratorium, atas persetujuan Sukma: "semua yang butuh persetujuan saya setujui") |
| Klasifikasi | `HEAVY` — 6 berkas, perubahan alur utama Pasien Lama dan saat pembentukan sesi |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("semua yang butuh persetujuan saya setujui dan lanjutkan sampai tuntas") |
| Target tulis | `V2QuilvianSystemFrontendDev`: berkas pada kartu task + konstanta teks Tujuan Layanan. Repository backend: laporan ini, baris status roadmap/traceability, penutupan `KSK-OQ-004` (kiosk) dan pencatatan amendment di blueprint Laboratorium |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend | Dikerjakan di atas `4ec51b0bf`; sudah di-commit pengguna di `694e772c1` (30 Sep 2026 22:41) |
| Commit backend yang dijadikan rujukan | `051a658a` + perubahan `BE-KSK-003` (kemudian `41a76582`) |
| Tanggal | 30 September – 1 Oktober 2026 |
| Status | ✅ SELESAI — 10 dari 10 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

1. Alur Pasien Lama dimulai dari **Tujuan Layanan**, lalu Identifikasi, lalu Review Data (warisan `FE-LAB-13`).
2. **Sesi kiosk (`scan-result`) dibentuk di Step Identifikasi** saat kartu dipindai, sehingga pasien yang memindai kartu lalu batal tetap meninggalkan sesi. Untuk Laboratorium, sesi kedua bisa terbentuk di Review.
3. Handoff `patientId` dari layar Cek No. RM (`FE-KSK-004`) belum dibaca halaman Pasien Lama.
4. Deep link `?patientId=` dari Pasien Baru langsung melompat ke Review Data.
5. Kartu asuransi/member dikenali lewat `scan-result` (`FindPatientAsync`), bukan lewat lookup.

---

## 2. Proses bisnis dari sisi pengguna

**Poliklinik:** Identifikasi → Review Data → Tujuan Layanan → Jenis Kunjungan → Pembayaran → Layanan & Dokter → Konfirmasi → Cetak Antrean.
**Laboratorium:** Identifikasi → Review Data → Tujuan Layanan → Selesai.

1. **Identifikasi (Step 1).** Pasien mengetik atau memindai kartu.
   - Kartu dengan `patientId` dimuat langsung.
   - Kartu KTP / kartu asuransi / nomor member dikenali lewat `POST kiosk-patient-lookups` (`searchType` 1 / 3 / 4).
   - **Tidak ada sesi kiosk yang dibentuk di sini**; isi kartu disimpan di memori layar.
   - Pasien yang datang dari Cek No. RM atau deep link langsung melihat kartu **"Pasien Terpilih"** dengan tombol "Lanjut ke Review Data" dan "Cari Pasien Lain".
2. **Review Data.** "Data Benar, Lanjut" selalu menuju Tujuan Layanan; tidak ada jalan melompat ke Jenis Kunjungan.
3. **Tujuan Layanan (Step 3).**
   - **Poliklinik:** sesi kiosk dibentuk **tepat sekali**, tanpa `targetService`, lalu lanjut ke Jenis Kunjungan.
   - **Laboratorium:** muncul pertanyaan surat dokter; setelah dijawab, sesi dibentuk sekali dengan `targetService = 2`, lalu layar "Silakan menuju loket Laboratorium".
   - Bila pembentukan sesi gagal, pasien tetap di Step 3 dengan pesan "Pilihan layanan belum dapat disimpan. Silakan coba lagi atau hubungi petugas." (`KSK-VAL-012`).
4. **Kembali dari Jenis Kunjungan ke Review.** Menekan lanjut lagi langsung menuju Jenis Kunjungan tanpa membentuk sesi kedua; tujuan layanan sudah terkunci.
5. **Konfirmasi.** Kunjungan membawa `kioskScanSessionId` sesi dari Step 3.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `03-frontend-architecture.md` §3 `FE-KSK-03`; `state-transition-matrix.md` §2; `validation-matrix.md` §3; hook `use-kiosk-old-patient-registration.jsx`; `kiosk-service-target-rules.js`; step `find`, `service-target`, `review`, `lab-handoff`, view Pasien Lama; `kiosk-old-patient-registration.service.js` (`createOldPatientScanSession`, `parseQrPayload`); blueprint Laboratorium (`BR-46`, `LAB-DEC-051/052`, `AC-93`, laporan `FE-LAB-13`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | Urutan `OLD_PATIENT_STEP_ITEMS`/`OLD_PATIENT_LAB_STEP_ITEMS`; step awal `find`; state `scanCapture` (hasil pindai di memori); `createServiceTargetSession` (sesi sekali, pengaman `scanSession`, pesan `KSK-VAL-012`); `handleSelectServiceTarget`/`handleSelectPhysicianRequest` membentuk sesi; `handleReviewContinue` → Tujuan Layanan (atau langsung lanjut bila sesi sudah ada); reset → Identifikasi; baca handoff (`selectKioskHandoffPatientId` → `consumeHandoffPatient` → muat detail); deep link mendarat di Step 1; `scanSessionExtraPayload` dihapus |
| `src/lib/hooks/kiosk/registration/kiosk-service-target-rules.js` | `buildScanCapture`, `buildServiceTargetScanSessionPayload` (identitas pasien + hasil pindai; Poliklinik tetap tanpa ruas tujuan) |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-find.jsx` | Jalur pindai tanpa `createOldPatientScanSession`: `patientId` → lookup `searchType 1/3/4` → pencarian existing; `runLookupSearch` mengembalikan status; kartu "Pasien Terpilih" |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-service-target.jsx` | Prop `busy`: pilihan terkunci selama sesi dibentuk |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-view.jsx` | Prop baru ke step Identifikasi/Tujuan Layanan; footer Step 3 ("Sebelumnya", "Ganti Pilihan Layanan" hanya sebelum sesi ada, label menyimpan); footer lab di Identifikasi dihapus; tombol Review disederhanakan |
| `src/lib/constants/kiosk/registration/kiosk-old-patient-service-target.constants.js` | Teks `backToReviewLabel`, `savingLabel`, `sessionFailedNotice` (`KSK-VAL-012`) |

### 3.3 Kepatuhan arsitektur frontend

- Kontrak `scan-result` tidak berubah; muatan cabang Poliklinik tetap **tanpa** `targetService`/`hasPhysicianRequest` (`KSK-DSN-007`), dibuktikan per kunci.
- Tidak ada HTTP baru di view; lookup memakai service `FE-KSK-003`.
- `UI GATE: 1 elemen — REUSE 0, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`. Elemen COMPOSE-nya kartu "Pasien Terpilih" = `oldPatientFindCard`/`oldPatientFindPrimaryButton`/`oldPatientFindSecondaryButton` existing + `BasePatientCard`, tanpa CSS baru.

### 3.4 Penutupan `KSK-OQ-004`

- Amendment dicatat di `laboratorium/00-interview-decisions.md` (Amendment Pass putaran 20, revisi 82) dan `laboratorium/03-frontend-architecture.md` §Amandemen 2026-09-30.
- Isinya: bunyi lama perilaku `FE-LAB-13` berdampingan dengan bunyi baru.
- BR-46, `LAB-DEC-051/052`, tiga jawaban pemilik modul `FE-LAB-13`, kontrak `scan-result`, dan `FE-LAB-14` diperiksa **tidak berubah**. `AC-93` tidak diubah bunyinya; tafsirnya dicatat.
- **Pemilik modul Laboratorium (Yoga Aji Pratama) perlu diberi tahu.**

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Memuat pasien handoff/deep link: layar "Memuat halaman…"; membentuk sesi: pilihan Step 3 terkunci + "Menyimpan pilihan layanan..." |
| Kosong | Pencarian tanpa hasil: popup existing |
| Gagal | Sesi gagal: tetap di Step 3 + `KSK-VAL-012`; lookup gagal: popup "Data Belum Dapat Diperiksa" |
| Tanpa hak akses | Penjaga akses kiosk existing |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Kiosk Scan Session

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/kiosk-scan-sessions/scan-result` | Membentuk sesi kiosk **sekali** di Step 3 | Existing (`KioskRead`) |

#### Health Services / Registration Management / Kiosk Patient Lookup

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/kiosk-patient-lookups` | Mengenali kartu KTP (`1`), kartu asuransi (`3`), member (`4`) di Step 1 | `KioskRead` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 6 berkas task | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `npm run build` (bersama `FE-KSK-008`) | Exit `0`, "Compiled successfully in 90s" | `PASS` | Keluaran `next build` |
| Uji browser Playwright | 26/26 PASS pada run pertama | `PASS` | Tabel di bawah + 7 screenshot |
| Query baca-saja sesi kiosk per pasien (sesuai kartu task) | 3 sesi baru untuk P7, masing-masing sesuai skenario | `PASS` | Tabel DB di bawah |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser

Lingkungan: `next dev` port 3000 → backend `bin/Release` di `https://localhost:7184` → DB `QuilvianNewDevSukma`. Akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat).

Data:
- Pasien samaran P7 diberi KTP `9999000000000007` selama uji, lalu dikembalikan ke `NULL`.
- Kartu asuransi samaran `KSKTEST-CARD-07` (baca-saja).
- Hasil lookup nomor member, hasil "ditemukan" di Cek No. RM, dan satu kegagalan `scan-result` disimulasikan.
- Pembuatan kunjungan disimulasikan, tetapi `POST` kunjungan dicegat dari kedua path.
- Sesi kiosk dibuat **sungguhan**, karena AC 5–6 menuntutnya.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| AC2 | Buka halaman langsung | Step aktif "Identifikasi" | `PASS` |
| AC1a | Bar langkah Poliklinik | Persis `Identifikasi, Review Data, Tujuan Layanan, Jenis Kunjungan, Pembayaran, Layanan & Dokter, Konfirmasi, Cetak Antrean` | `PASS` |
| AC4 | Pindai kartu `{identityNumber, fullName}` di Step 1 | Masuk Review; **0** `scan-result`; dikenali lewat lookup `searchType 1` | `PASS` |
| AC8 | Review → lanjut | Step aktif "Tujuan Layanan"; tetap 0 `scan-result` | `PASS` |
| AC5 | Pilih Poliklinik | Tepat **1** `scan-result`, kunci muatan `identityNumber, identityType, fullName, isManualInput, rawScanText, parsedJson` (tanpa `targetService`/`hasPhysicianRequest`); `isManualInput=false` dan `rawScanText` berisi hasil pindai; respons `200` | `PASS` |
| AC5/AC8 | Jenis Kunjungan → "Sebelumnya" → Review → lanjut | Langsung Jenis Kunjungan; tetap 1 `scan-result` | `PASS` |
| AC5 | Lanjut hingga Buat Antrean | Body kunjungan `kioskScanSessionId` = id sesi Step 3 (`41f17807-…`) | `PASS` |
| AC1b | Pilih Laboratorium | Bar menjadi `Identifikasi, Review Data, Tujuan Layanan, Selesai`; belum ada `scan-result` | `PASS` |
| AC6 | Jawab "Ya, Saya Membawa" | Tepat 1 `scan-result` dengan `targetService: 2`, `hasPhysicianRequest: true`; layar "Silakan menuju loket Laboratorium" | `PASS` |
| AC7 | `scan-result` dijawab `500` (simulasi) | Tetap "Tujuan Layanan" + pesan `KSK-VAL-012`; coba lagi → berhasil, lanjut Jenis Kunjungan | `PASS` |
| AC9 | Pindai `{insuranceCardNumber: "KSKTEST-CARD-07"}` | Lookup `searchType 3` (backend asli) → Review P7 | `PASS` |
| AC9 | Pindai `{memberNumber: "KSKTEST-MBR-07"}` | Lookup `searchType 4` → Review P7; kedua pindai 0 `scan-result` | `PASS` |
| AC3 | Cek No. RM → "Lanjut Pendaftaran Pasien Lama" | Step 1 menampilkan "Pasien Terpilih" P7; URL `/kiosk/registration/old-patient` tanpa query; slice `{ patientId: null }`; "Lanjut ke Review Data" → Review | `PASS` |
| AC10 | Deep link `?patientId=<P7>` | Step 1 terisi "Pasien Terpilih"; query dibersihkan | `PASS` |

**Query baca-saja sesi kiosk (sesudah AC 5–7):**

| Sesi | `TargetService` | `HasPhysicianRequest` | `IsManualInput` | Pasien | Asal |
| --- | --- | --- | --- | --- | --- |
| `41f17807-…` | `NULL` | `NULL` | `False` | P7 | AC5 Poliklinik lewat pindai |
| `56bfa5b0-…` | `2` | `True` | `True` | P7 | AC6 Laboratorium |
| `7c666855-…` | `NULL` | `NULL` | `True` | P7 | AC7 coba lagi (Poliklinik, cari nama) |

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis dan pemeriksaan visual screenshot (Step 1 "Pasien Terpilih" dan lainnya).

**Pembersihan data (disetujui pengguna):** seluruh sesi kiosk P7 yang tercipta selama uji `FE-KSK-007`/`FE-KSK-008` (15 baris, semuanya dibuat sesudah 2026-09-30 15:24 UTC) dihapus, dan KTP P7 dikembalikan ke `NULL`. Sesudahnya `TrxKioskScanSession` 16 baris, sama dengan sebelum uji.

**Tidak dijalankan:** pemindai infrared fisik (hasil pindai disimulasikan dengan mengetik JSON kartu ke kolom yang sama); alur Pasien Baru (di luar cakupan).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Bar poliklinik 8 step urutan PRD; Laboratorium: Identifikasi → Review Data → Tujuan Layanan → Selesai | Terpenuhi | AC1a, AC1b |
| 2. Membuka halaman langsung → step Identifikasi | Terpenuhi | AC2 |
| 3. Dari Cek No. RM → Step 1 menampilkan pasien itu, URL tanpa query, slice kosong setelah dibaca | Terpenuhi | AC3 |
| 4. Memindai kartu di Step 1 tidak membentuk sesi | Terpenuhi | AC4, AC9 |
| 5. Poliklinik → tepat satu `scan-result` tanpa `targetService`, kunjungan membawa `kioskScanSessionId` sesi itu | Terpenuhi | AC5 + DB |
| 6. Laboratorium + surat dokter → tepat satu `scan-result` `targetService = 2`, lalu handoff lab | Terpenuhi | AC6 + DB |
| 7. `scan-result` gagal → tetap di Step 3 + `KSK-VAL-012` | Terpenuhi | AC7 |
| 8. Tidak bisa melompat dari Review ke Jenis Kunjungan | Terpenuhi | AC8 |
| 9. Kartu asuransi/member dikenali lewat lookup `searchType 3/4` | Terpenuhi | AC9 |
| 10. Deep link `?patientId=` tetap berfungsi dan mendarat di Step 1 terisi | Terpenuhi | AC10 |
| DoD: AC 1–10 di `FE-KSK-007.md`; `KSK-OQ-004` tertutup | Terpenuhi | Laporan ini; §3.4 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` baru |
| Masalah yang diketahui | (1) Pemilik modul Laboratorium perlu diberi tahu Amendment Pass putaran 20. (2) Setelah sesi terbentuk, tujuan layanan tidak dapat diganti di kiosk (sesi ditulis sekali, `KSK-DEC-014`); pasien yang keliru memilih perlu ke petugas |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` di source. Data uji dibersihkan (§6) |
| Interupsi | Sesi agent beberapa kali terputus (batas pemakaian); dilanjutkan dari keadaan Git/DB/port terverifikasi tanpa mengulang uji yang sudah lulus |
| Status Git | Seluruh berkas task ini sudah di-commit pengguna di `694e772c1` |
| Langkah berikutnya | `FE-KSK-008` (sudah dikerjakan pada sesi yang sama) |
