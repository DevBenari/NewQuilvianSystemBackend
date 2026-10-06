# Laporan Perubahan Frontend — `FE-KSK-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-008` |
| Judul | Penjamin Utama per kunjungan |
| Slice | EPIC KSK-04 — Penjamin Utama, gelombang `MVP-3` |
| Roadmap | `docs/module-blueprints/kiosk/roadmap/frontend-roadmap.md` — kartu `FE-KSK-008` |
| Trace | `FR-KSK-032..034`; `KSK-DEC-008/009/013`; `KSK-INV-005`; `KSK-VAL-013/014/015`; `flowcharts/penjamin-utama.md`; api §Patient Encounter; `RWI-ENC-PAYER-001` `1.1.0` |
| Contract version | `KSK-CONTRACT-v1` — `approved`; route kiosk menerima `paymentType = 3` sejak `BE-KSK-003` ✅ |
| Wewenang UI | Layar Pembayaran existing dipakai ulang; judul "Pilih Penjamin Utama" untuk Kondisi C; tanpa CSS atau komponen baru |
| Dependency | `FE-KSK-007` ✅, `BE-KSK-003` ✅ |
| Klasifikasi | `MEDIUM` — 4 berkas; alur pembayaran dan muatan kunjungan |
| Task mode | `FRONTEND` — pengguna 2026-09-30 ("semua yang butuh persetujuan saya setujui dan lanjutkan sampai tuntas") |
| Target tulis | `V2QuilvianSystemFrontendDev`: berkas pada kartu task. Repository backend: laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend | Sebagian besar ikut di-commit pengguna di `694e772c1`; perbaikan terakhir (tab otomatis Kondisi B, label "Penjamin") **belum di-commit** |
| Commit backend yang dijadikan rujukan | `41a76582` (branch `sukmagp`) |
| Tanggal | 30 September – 1 Oktober 2026 |
| Status | ✅ SELESAI — 7 dari 7 acceptance criteria terbukti |

---

## 1. Keadaan yang ditemukan di awal

1. Pembayaran "Asuransi / Penjamin" **selalu mewajibkan asuransi**. Perusahaan hanya "opsional untuk kebutuhan billing", dan `handlePaymentContinue` memaksa pilihan `company` menjadi `insurance`.
2. `createOldPatientEncounter` **menolak** Perusahaan ("Backend … hanya menerima pembayaran Tunai atau Asuransi"). Akibatnya tab Perusahaan berakhir error (`KSK-CAP-032`).
3. Tombol **"Jadikan Utama"** mengubah penanda utama di data pasien (`PATCH …/primary`), bertentangan dengan `KSK-DEC-008`.
4. **Bug tersembunyi:** peta enum `companyPaymentType` mencari nama `["Company", "CompanyGuarantor", "Insurance"]`, sedangkan `getEnumValueByName` mengambil opsi **pertama** yang cocok. Karena opsi berurutan Tunai(1), Asuransi(2), Penjamin Perusahaan(3), Perusahaan akan terkirim sebagai **Asuransi (2)**.

---

## 2. Proses bisnis dari sisi pengguna

Di step **Pembayaran**, pasien memilih **Tunai** atau **Menggunakan Penjamin**. Bila memakai penjamin:

| Kondisi | Penjamin milik pasien | Yang dilihat pasien |
| --- | --- | --- |
| A | Hanya asuransi | "Form Data Penjamin", tab Asuransi terbuka; pilih kartu asuransi seperti biasa |
| B | Hanya perusahaan | "Form Data Penjamin", **tab Perusahaan terbuka otomatis**; pilih perusahaan |
| C | Asuransi **dan** perusahaan | **"Pilih Penjamin Utama"** — "Anda memiliki asuransi dan perusahaan penjamin. Pilih satu yang menanggung kunjungan ini." Tidak ada pilihan otomatis ("Belum ada penjamin dipilih"). Menekan Lanjut sebelum memilih → "Silakan pilih penjamin utama untuk kunjungan ini." (`KSK-VAL-013`) |

Aturan pemilihan:
- Memilih perusahaan mengosongkan asuransi, dan sebaliknya. Satu kunjungan = **satu penanggung** (`KSK-INV-005`).
- Pilihan ini **tidak** mengubah penjamin utama di data pasien, dan tombol "Jadikan Utama" tidak ada lagi.

Setelah memilih:
- Pasien melengkapi masa aktif kartu, memilih layanan dan dokter, lalu **Buat Antrean** di Konfirmasi.
- Bila backend menolak relasi perusahaan (misalnya kedaluwarsa), pesan tampil sebagai "**Penjamin perusahaan tidak dapat dipakai:** Penjamin perusahaan sudah kedaluwarsa pada tanggal kunjungan." (`KSK-VAL-015`).
- Setelah tiket tercetak tidak ada tombol kembali ke Pembayaran (`KSK-DEC-009`); perubahan penjamin dilakukan petugas.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `flowcharts/penjamin-utama.md`; `00-interview-decisions.md` (`KSK-DEC-008/009/013`); `validation-matrix.md` §3; step Pembayaran (1.748 baris), Konfirmasi, Tiket; hook `handlePaymentContinue`; service `createOldPatientEncounter`, `getEnumValueByName`, `buildSelectedGuarantor` (ternyata tidak dipakai di mana pun — tidak diubah); enum backend `EncounterPaymentType`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-payment.jsx` | `SavedPayerCard` tanpa "Jadikan Utama"; handler, state, dan import "primary" dihapus; pilihan asuransi/perusahaan saling eksklusif (`guarantorKey`); Kondisi A/B/C dan `canContinue` untuk perusahaan; pesan `KSK-VAL-013/014`; judul "Pilih Penjamin Utama" (C) / "Form Data Penjamin"; label tab "Opsional"/"Utama" → "Penjamin"; tab otomatis Perusahaan untuk Kondisi B; peta enum `companyPaymentType` diperbaiki (`["CompanyGuarantor", "Company", "Penjamin Perusahaan"]`) |
| `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx` | `INITIAL_PAYMENT_FORM.guarantorKey`; `handlePaymentContinue` menurunkan `paymentKey` (`cash`/`insurance`/`company`) dan `paymentTypeValue` dari pilihan, serta mengosongkan penjamin yang tidak dipilih |
| `src/lib/services/kiosk/registration/kiosk-old-patient-registration.service.js` | `createOldPatientEncounter` menerima `company`: `paymentType` Penjamin Perusahaan + `patientCompanyGuarantorId`, tanpa `patientInsuranceId`/`paymentMethodId` |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-confirm.jsx` | Penolakan `400` untuk perusahaan diberi awalan "Penjamin perusahaan tidak dapat dipakai:" (`KSK-VAL-015`) |

### 3.3 Kepatuhan arsitektur frontend

- Tidak ada komponen, CSS, atau endpoint baru; layar existing dipakai ulang — `UI GATE: N/A — tidak ada elemen UI baru; perubahan teks, aturan pilihan, dan penghapusan tombol pada layar existing`.
- Endpoint `PATCH …/kiosk/{id}/primary` **tidak dihapus** dari backend (`KSK-GAP-011`); layar ini hanya berhenti memakainya. Fungsi service `setPatient…PrimaryFromKiosk` dibiarkan (tidak dipakai lagi oleh Kiosk).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat data pembayaran pasien..." (existing) |
| Kosong | Tidak punya penjamin: daftar kosong + tombol Tambah (existing); pasien dapat memilih Tunai |
| Gagal | Validasi: pesan `KSK-VAL-013/014` di layar Pembayaran; penolakan backend: `KSK-VAL-015` di Konfirmasi |
| Tanpa hak akses | Penjaga akses kiosk existing |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/health-services/registration-management/patient-encounters` (alias route kiosk) | Kunjungan dengan satu penanggung: Tunai (1), Asuransi (2), **Penjamin Perusahaan (3)** | Policy `KioskRead` |

Endpoint `PATCH …/patient-insurances/kiosk/{id}/primary` dan `…/patient-company-guarantors/kiosk/{id}/primary` **tidak lagi dipanggil**.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 4 berkas task | 0 error; 6 warning **identik dengan `HEAD`** (Konfirmasi: 3× `react-hooks/refs`, 2× `<img>`; Pembayaran: 1× memoization) — bergeser baris saja | `PASS` (tanpa warning baru) | Keluaran perintah + pembanding `git show HEAD:… \| eslint --stdin` |
| `npm run build` (bersama `FE-KSK-007`) | Exit `0`, "Compiled successfully in 90s" | `PASS` | Keluaran `next build` |
| Uji browser Playwright run 1 | 14 PASS; 1 FAIL (Kondisi B: tab Perusahaan tidak terbuka otomatis) | Bug terbukti | `run1.log` |
| Perbaikan: tab otomatis diturunkan dari data (bukan diset saat klik, karena daftar penjamin dimuat asinkron) | — | — | Diff |
| Uji browser run 2 — hanya AC 2 (tanpa membuat kunjungan baru) | 5/5 PASS | `PASS` | `run2-ac2.log` |
| Cek DB baca-saja | Kunjungan dan penanda utama sesuai | `PASS` | Tabel DB di bawah |

`AUTOMATED TEST: SKIPPED (opsional) — uji browser lewat skrip Playwright sekali pakai di scratchpad; repository tidak memakai Jest`

### Uji browser

Lingkungan: `next dev` → backend `bin/Release` `https://localhost:7184` → DB `QuilvianNewDevSukma`. Akun perangkat Kiosk `kiosk-test` (kredensial dari pengguna, tidak dicatat).

Data:
- Pasien samaran P7 punya asuransi `a7` ("BPJS Kesehatan"), perusahaan `c1` (`KSKTEST-EMP-01`, berlaku sampai 30 Sep 2027), dan `c2` (`KSKTEST-EMP-02`, berakhir 29 Sep 2026). Artinya P7 berada di **Kondisi C**.
- Kondisi A dan B dibuat dengan mengosongkan salah satu daftar penjamin di respons (`page.route`), tanpa mengubah DB.
- **Kunjungan untuk AC 3/4 dibuat sungguhan atas persetujuan pengguna**, lalu dibersihkan.

| ID | Skenario | Hasil | Klasifikasi |
| --- | --- | --- | --- |
| AC1 | Kondisi C | Judul "Pilih Penjamin Utama"; "Belum ada penjamin dipilih", 0 tombol "Dipilih"; Lanjut → "Silakan pilih penjamin utama untuk kunjungan ini." dan tetap di layar | `PASS` |
| AC5 | Layar Pembayaran | Tidak ada teks/tombol "Jadikan Utama" | `PASS` |
| AC3 | Pilih perusahaan `c1` → Konfirmasi → Buat Antrean | Konfirmasi menampilkan "PT PLN Persero"; body `paymentType: 3`, `patientCompanyGuarantorId: c1`, `patientInsuranceId: null`; respons `200` `ENC-RSMMC-00174`; tiket tampil; tanpa "Asuransi pasien belum dipilih" | `PASS` |
| AC7 | Layar tiket | Tombol hanya "Kembali ke Home" dan "Selesai"; Back browser tidak membuka Pembayaran | `PASS` |
| AC4 | Pilih asuransi `a7` → Buat Antrean | Body `paymentType: 2`, `patientInsuranceId: a7`, tanpa `patientCompanyGuarantorId`; respons `200` | `PASS` |
| AC6 | Pilih perusahaan `c2` (kedaluwarsa) → Buat Antrean | Backend `400`; layar: "Penjamin perusahaan tidak dapat dipakai: Penjamin perusahaan sudah kedaluwarsa pada tanggal kunjungan." | `PASS` |
| AC2 | Kondisi A (daftar perusahaan dikosongkan) | Bukan "Pilih Penjamin Utama"; tab Asuransi; pilih asuransi → Layanan & Dokter | `PASS` |
| AC2 | Kondisi B (daftar asuransi dikosongkan) | Run 1 **FAIL** (tab Asuransi); sesudah perbaikan: tab Perusahaan terbuka otomatis; pilih perusahaan → Layanan & Dokter | `PASS` (run 2) |
| AC5 | Seluruh uji | 0 request `PATCH`/`…/primary` | `PASS` |

**Cek DB baca-saja:**

| Kunjungan | `PaymentType` | Penjamin tersimpan | Sesi kiosk | Antrean |
| --- | --- | --- | --- | --- |
| `ENC-RSMMC-00174` | 3 | `PatientCompanyGuarantorId = c1` | Ada | 1 |
| `ENC-RSMMC-00175` | 2 | `PatientInsuranceId = a7` | Ada | 1 |

Penanda `IsPrimary` sebelum dan sesudah uji identik (`a7` = True, `c1` = False, `c2` = False). Kunjungan dengan `c2` tidak tersimpan.

Catatan:
- Run 1 (AC 1, 3–7) berjalan sebelum perbaikan tab otomatis. Pada Kondisi C, tab bawaannya tetap Asuransi baik sebelum maupun sesudah perbaikan, sehingga hasil run itu tetap berlaku.
- Run 2 sempat berhenti karena tanggal berganti ke Kamis, 1 Oktober. Tidak ada jadwal poliklinik hari itu, sehingga tombol "Poli Anak" yang ditunggu skrip tidak muncul. Layar sebenarnya sudah benar di "Layanan & Dokter"; pemeriksaan diubah menjadi menunggu judul step "Pilih Layanan Tujuan".

**Pembersihan data (disetujui pengguna):**
- Dihapus: dua kunjungan uji (`00174`, `00175`) beserta 2 baris penjamin kunjungan dan 2 antrean.
- Dihapus: 15 sesi kiosk P7 dari uji `FE-KSK-007`/`008`.
- KTP P7 dikembalikan ke `NULL`.
- Sesudahnya: `RegPatientEncounter` 173, `RegPatientEncounterGuarantor` 173, `TrxQueue` 173, `TrxKioskScanSession` 16 — sama dengan sebelum uji.

Uji manual oleh orang: `NOT RUN` — digantikan uji browser otomatis dan pemeriksaan screenshot.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Kondisi C → layar Pilih Penjamin Utama tanpa preselect; Lanjutkan ditolak sebelum memilih | Terpenuhi | AC1 |
| 2. Kondisi A/B → alur existing | Terpenuhi (sesudah perbaikan tab Kondisi B) | AC2 run 2 |
| 3. Memilih Perusahaan → `paymentType = 3` + `patientCompanyGuarantorId`, kunjungan tersimpan, tanpa error "Asuransi pasien belum dipilih" | Terpenuhi | AC3 + DB |
| 4. Memilih Asuransi → `paymentType = 2` | Terpenuhi | AC4 + DB |
| 5. Tidak ada "Jadikan Utama" dan tidak ada `PATCH …/primary`; penanda utama di data pasien tidak berubah | Terpenuhi | AC5 + DB |
| 6. Penolakan `400` tampil dengan awalan "Penjamin perusahaan tidak dapat dipakai:" | Terpenuhi | AC6 |
| 7. Setelah tiket tercetak tidak ada jalan kembali ke Pembayaran | Terpenuhi | AC7 |
| DoD: AC 1–7 di `FE-KSK-008.md` | Terpenuhi | Laporan ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 6 warning lint lama (tidak ada yang baru) |
| Masalah yang diketahui | Fungsi service `setPatientInsurancePrimaryFromKiosk` / `setPatientCompanyGuarantorPrimaryFromKiosk` dan endpoint `PATCH …/primary` masih ada tetapi tidak dipakai Kiosk (`KSK-GAP-011`) |
| Dependency backend | `NONE` — `BE-KSK-003` ✅ |
| Perubahan sampingan | `NONE` di source. Data uji dibersihkan (§6) |
| Interupsi | Sesi agent terputus karena batas pemakaian di tengah perbaikan tab Kondisi B; dilanjutkan dari keadaan terverifikasi. Uji yang sudah lulus tidak diulang, dan hanya AC 2 yang dijalankan ulang agar tidak membuat kunjungan tambahan |
| Status Git | Frontend: ` M src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-payment.jsx` (perbaikan tab Kondisi B + label "Penjamin"); sisanya sudah di-commit di `694e772c1` |
| Langkah berikutnya | Commit perubahan terakhir; beri tahu pemilik modul Laboratorium tentang amendment `KSK-OQ-004`; UAT di perangkat Kiosk fisik |
