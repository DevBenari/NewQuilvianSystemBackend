# Laporan Perubahan Frontend — `FE-RWI-099`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-099` |
| Judul | Modal Otorisasi Supervisor Override untuk Evakuasi Darurat Klinis |
| Slice | Gelombang INT-FE-2 — `INP-S22` |
| Roadmap | [`docs/module-blueprints/rawat-inap/integrasi-billing/roadmap/frontend-roadmap.md`](../../roadmap/frontend-roadmap.md) — kartu `FE-RWI-099` |
| Trace | `FR-INT-016`; `RWI-DEC-158`, `RWI-AC-238`; Kontrak Arsitektur `1.0.0` — Skema §4, Validasi `VAL-INT-004`, `VAL-INT-005`, State §1 |
| Contract version | `1.0.0` API Supervisor Override Backend (`BE-RWI-134`) |
| Wewenang UI | Roadmap Frontend `FE-RWI-099`, Skema Tampilan §4, Design Token Quilvian |
| Dependency | `FE-RWI-098` [FE] ✅, `BE-RWI-134` [BE] ✅ (Keduanya Selesai) |
| Klasifikasi | `HIGH` — Modal otorisasi darurat medis supervisor bangsal (*Clinical Emergency Evacuation Override*), validasi alasan $\ge 20$ karakter di sisi klien (`VAL-INT-004`), verifikasi PIN otorisasi (`VAL-INT-005`), penegakan hak akses `InpatientSupervisor:Override`, dan penyegaran gerbang pemulangan menjadi `OVERRIDDEN` |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `src/components/features/health-services/inpatient-management/billing-integration/`, `src/style/health-services/inpatient-management/`, `src/components/features/inpatient/billing-integration/`, `src/lib/services/health-services/inpatient-management/` |
| Tanggal | 18 September 2026 |
| Status | ✅ `SELESAI`. Seluruh acceptance criteria (AC-1 s.d AC-5) terverifikasi dengan bukti uji otomatis (5/5 PASS), lint 0 error, dan verifikasi anti-regresi. |

---

## 1. Masalah yang Diselesaikan

1. **Dilema Etik dan Hukum Keselamatan Nyawa vs Administrasi Keuangan (`FR-INT-016`, `RWI-DEC-158`):** Pada kondisi kedaruratan klinis (misalnya pasien rawat inap yang mendadak mengalami henti jantung/syok kardiogenik, emboli paru, atau koma yang memerlukan rujukan segera ke rumah sakit rujukan tersier menggunakan ambulans), pasien tidak boleh tertahan oleh ketiadaan pelunasan kasir atau sengketa klaim penjamin. Tanpa mekanisme *Supervisor Override*, gerbang pemulangan fisik terkunci mati dan membahayakan keselamatan jiwa pasien.
2. **Ketiadaan Akuntabilitas dan Risiko Penyalahgunaan Wewenang (`VAL-INT-004`, `VAL-INT-005`):** Pemulangan darurat tanpa pembayaran penuh menimbulkan potensi piutang rumah sakit yang tidak tertagih. Oleh karena itu, pelepasan darurat tidak boleh dilakukan oleh sembarang staf bangsal, melainkan wajib diotorisasi oleh Supervisor Bangsal / Kepala Ruangan melalui verifikasi kredensial rahasia (PIN) dan justifikasi klinis yang lengkap serta dapat dipertanggungjawabkan di hadapan komite medik dan audit hukum.
3. **Penyelarasan Siklus Hidup Clearance Kasir:** Setelah tindakan override disetujui, gerbang pemulangan fisik harus seketika bertransisi ke status `OVERRIDDEN`, membuka tombol kepulangan fisik, dan mencatat identitas supervisor serta stempel waktu eksekusi tanpa menunggu pelunasan tagihan kasir.

---

## 2. Proses Bisnis dari Sisi Pengguna (Skenario Rumah Sakit)

```mermaid
flowchart TD
    A[Pasien Rawat Inap Berstatus Clearance Tertahan: PENDING atau REVOKED] --> B{Terjadi Kondisi Kedaruratan Klinis Akut?}
    B -- Tidak: Alur Normal --> C[Keluarga Melunasi Tagihan di Loket Kasir Utama]
    B -- Ya: Pasien Kritis / Evakuasi Ambulans Darurat --> D{Apakah Pengguna Memiliki Wewenang Supervisor Bangsal?}
    
    D -- Tidak Memiliki Izin: Perawat Pelaksana / Staf Biasa --> E[Tombol Supervisor Override Disembunyikan / Tidak Ditampilkan]
    D -- Memiliki Izin: InpatientSupervisor:Override --> F[Tombol Merah '🚨 Supervisor Override (Darurat)' Ditampilkan]
    
    F --> G[Supervisor Menekan Tombol: Membuka Modal SupervisorOverrideModal]
    G --> H[Modal Menampilkan Konteks Pasien, Tagihan Tertahan, dan Peringatan Tanggung Jawab Hukum]
    H --> I[Supervisor Memasukkan Alasan Klinis Kedaruratan]
    
    I --> J{Validasi Sisi Klien: Panjang Alasan >= 20 Karakter? VAL-INT-004}
    J -- Kurang dari 20 Karakter --> K[Counter Karakter Merah, Tombol Setujui Tetap Nonaktif / Disabled]
    J -- Minimal 20 Karakter --> L[Supervisor Memasukkan PIN Otorisasi Rahasia VAL-INT-005]
    
    L --> M{PIN Telah Terisi?}
    M -- Kosong --> N[Tombol Setujui Tetap Nonaktif / Disabled]
    M -- Terisi Lengkap --> O[Tombol '🚨 Setujui Pelepasan Fisik Darurat' Menjadi AKTIF]
    
    O --> P[Supervisor Mengonfirmasi: Memanggil POST /api/v1/.../supervisor-override]
    P --> Q{Respons Backend BE-RWI-134}
    Q -- Gagal: PIN Salah / Tidak Berwenang --> R[Tampil Notifikasi Galat Merah, Form Tetap Terbuka]
    Q -- Berhasil: 200 OK --> S[Status Clearance Seketika Berubah Menjadi OVERRIDDEN]
    S --> T[Gerbang Pemulangan Membuka Tombol Kepulangan Fisik Berwarna Hijau]
    T --> U[Perawat Dapat Melakukan Pelepasan Fisik Pasien untuk Evakuasi Darurat]
```

### Skenario Konkret di Ruang Rawat Inap:
* **Latar Belakang Pasien:**
  - Pasien Tn. Ahmad Dahlan (No. RM: `RM-2026-0881`), dirawat di Ruang ICU/HCU Kamar Bougenville 01 bed A.
  - Status clearance kasir Tn. Ahmad berstatus `REVOKED` karena adanya penghentian sementara penjaminan asuransi swasta yang memerlukan klarifikasi diagnosa tambahan, dengan sisa tagihan susulan farmasi yang belum dibayar.
* **Kejadian Kedaruratan Klinis:**
  - Pada pukul 02:15 WIB dini hari, pasien mendadak mengalami infark miokard akut luas yang berkembang menjadi syok kardiogenik refrakter.
  - Dokter Jaga ICU dan DPJP dr. Faisal Sp.JP memutuskan pasien harus segera dievakuasi darurat via ambulans ICU ke Rumah Sakit Pusat Jantung Nasional Harapan Kita untuk tindakan Percutaneous Coronary Intervention (PCI) darurat dalam kurun waktu *golden hour*.
* **Tindakan Otorisasi Supervisor:**
  1. Perawat pelaksana membuka gerbang pemulangan di bangsal. Tombol pemulangan normal terkunci rapat dengan lencana merah `REVOKED`.
  2. Kepala Ruangan / Supervisor Bangsal Ns. Ratna S.Kep yang memiliki wewenang `InpatientSupervisor:Override` login ke sistem.
  3. Tombol merah mencolok **"🚨 Supervisor Override (Darurat)"** tampil di kartu gerbang pemulangan (`AC-1`).
  4. Ns. Ratna menekan tombol tersebut, memicu terbukanya dialog konfirmasi **SupervisorOverrideModal** (`AC-2`).
  5. Sistem menampilkan kotak peringatan hukum berbingkai merah:
     - *"TINDAKAN INI MEMILIKI KONSEKUENSI HUKUM DAN AUDIT MEDIS KETAT. Supervisor menyatakan bahwa pemulangan fisik diizinkan atas dasar kedaruratan klinis nyawa pasien. Seluruh catatan akan direkam secara permanen pada Jejak Audit Direksi & Komite Medik."*
  6. Ns. Ratna mengisi kolom alasan: *"Pasien mengalami syok kardiogenik akut, memerlukan transfer ambulans ICU darurat segera ke RS Jantung Harapan Kita untuk PCI primer."* (138 karakter $\ge 20$ karakter, memenuhi `VAL-INT-004`).
  7. Ns. Ratna memasukkan 6-digit PIN otorisasi rahasia miliknya ke kolom verifikasi (`AC-3`, `VAL-INT-005`).
  8. Tombol konfirmasi **"🚨 Setujui Pelepasan Fisik Darurat"** aktif. Ns. Ratna menekan tombol tersebut.
  9. Sistem mengirimkan payload ke backend `POST .../supervisor-override` (`AC-4`).
  10. Backend mencatat audit log override, dan mengembalikan status clearance baru: `OVERRIDDEN`.
  11. Modal tertutup, gerbang pemulangan di bangsal seketika menyegarkan data (`AC-5`):
      - Lencana status berubah menjadi ungu `OVERRIDDEN (Pelepasan Darurat)`.
      - Tombol **"Konfirmasi Pasien Pulang Fisik"** seketika terbuka menjadi aktif.
      - Perawat bangsal segera menekan tombol konfirmasi kepulangan fisik, mencatat pelepasan bed ICU untuk evakuasi ambulans tanpa halangan administratif kasir.

---

## 3. Gerbang Keputusan Base Component (UI Gate)

| Kebutuhan UI | Kandidat Base | Bukti Lokasi | Status | Rekomendasi & Konsekuensi |
| :--- | :--- | :--- | :---: | :--- |
| **Modal Konfirmasi & Otorisasi** | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | Menggunakan `ConfirmModal` standar dengan varian `danger`, backdrop static, pemblokiran keyboard tidak sengaja, dan integrasi tombol aksi aman bawaan Quilvian. |
| **Banner Peringatan Tanggung Jawab Hukum** | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Menggunakan `InformationAlert` dengan varian `danger` dan ikon pelindung/peringatan untuk mempertegas implikasi hukum dan audit komite medik. |
| **Input Alasan Klinis Multi-Baris** | `BaseTextAreaField` | `src/components/features/base-features/base-form-control/index.js` | `REUSE` | Memanfaatkan form control standar berlabel, deskripsi bantuan, dan indikator required (*) tanpa memakai elemen `<textarea` mentah. |
| **Input PIN Otorisasi Rahasia** | `BaseTextField` | `src/components/features/base-features/base-form-control/index.js` | `REUSE` | Memanfaatkan form control teks dengan konfigurasi `type="password"`, menyamarkan masukan angka rahasia supervisor. |
| **Lencana Status Clearance Saat Ini** | `BillingStatusBadge` | `src/components/features/health-services/inpatient-management/billing-integration/billing-status-badge.jsx` | `REUSE` | Menampilkan konteks status kasir pasien (`REVOKED` atau `PENDING`) yang sedang dioverride. |
| **Komponen Feature Modal Override** | `SupervisorOverrideModal` | `src/components/features/health-services/inpatient-management/billing-integration/supervisor-override-modal.jsx` | `COMPOSE` | **Rekomendasi (Opsi A):** Mengomposisikan seluruh komponen base di atas ke dalam satu dialog feature domain rawat inap yang kohesif, mematuhi prinsip token CSS, dan menyediakan penanganan validasi formulir terpadu. |

> **Keputusan Base Component:**
> - **Opsi A (Rekomendasi): `COMPOSE` Komponen Feature `SupervisorOverrideModal`** — Menjamin seluruh interaksi tombol, modal, input form, dan alert mematuhi tata kelola base component Quilvian tanpa memodifikasi pustaka bersama atau melanggar aturan token warna.

---

## 4. Perubahan yang Dikerjakan

### 4.1 Berkas yang Dibuat

| Berkas | Peran |
| :--- | :--- |
| `src/components/features/health-services/inpatient-management/billing-integration/supervisor-override-modal.jsx` | Komponen utama modal otorisasi: merangkum `ConfirmModal`, `InformationAlert`, `BaseTextAreaField`, `BaseTextField`, validasi alasan $\ge 20$ karakter, verifikasi PIN, dan mutasi backend `submitSupervisorOverride`. |
| `src/style/health-services/inpatient-management/supervisor-override-modal.module.css` | Module CSS styling modal otorisasi: murni berbasis token desain Quilvian (`var(--color-...)`, `var(--font-...)`, `var(--space-...)`), bebas dari warna literal (`#hex`, `rgba`), dan tanpa `!important`. |
| `src/components/features/inpatient/billing-integration/SupervisorOverrideModal.jsx` | Re-export alias komponen sesuai kontrak Definition of Done (DoD) roadmap frontend. |
| `tests/unit/inpatient-supervisor-override-modal.test.mjs` | Unit test otomatis Node.js test runner untuk pengujian AC-1, AC-2, AC-3, AC-4, AC-5, dan DoD alias path. |

### 4.2 Berkas yang Diperbarui

| Berkas | Perubahan |
| :--- | :--- |
| `src/lib/services/health-services/inpatient-management/inpatient-billing.service.js` | Menambahkan fungsi API service `submitSupervisorOverride(episodeId, { reason, supervisorPin })` yang memanggil endpoint `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/supervisor-override`. |
| `src/components/features/health-services/inpatient-management/billing-integration/discharge-clearance-gate-card.jsx` | 1. Menghubungkan pemeriksaan hak akses `usePermission("InpatientSupervisor", "Override")` dan prop `isSupervisor`.<br>2. Mengatur visibilitas tombol override: hanya muncul saat status `isRevoked` atau `isPending` dan pengguna berstatus supervisor.<br>3. Mengintegrasikan modal `SupervisorOverrideModal` dan menangani penyegaran status `refreshBilling()` saat override berhasil disetujui.<br>4. Memperbaiki urutan pemanggilan React Hooks agar selalu dipanggil di bagian paling atas sebelum percabangan early return. |

---

## 5. Pemenuhan Kriteria Penerimaan (Acceptance Criteria)

| Kriteria | Status | Bukti Implementasi & Verifikasi |
| :--- | :---: | :--- |
| **AC-1**: Tombol "Supervisor Override" hanya tampil jika status clearance `Revoked` atau `Pending` DAN pengguna memiliki wewenang Supervisor Bangsal | ✅ Terpenuhi | Logika `canShowOverride = (isRevoked || isPending) && isSupervisorUser;` memastikan tombol tersembunyi bagi pengguna non-supervisor atau ketika clearance sudah `CLEARED`. Hak akses dievaluasi melalui hook `usePermission("InpatientSupervisor", "Override")` dengan opsi override prop `isSupervisor`. Diverifikasi pada test case `AC-1`. |
| **AC-2**: Modal menampilkan peringatan tanggung jawab hukum dan validasi alasan klinis $\ge 20$ karakter di sisi klien (`VAL-INT-004`) | ✅ Terpenuhi | Komponen menyajikan `InformationAlert` berisikan teks peringatan hukum konsekuensi audit medik, counter karakter `characterCounter`, dan validasi `trimmedReason.length >= MIN_REASON_LENGTH (20)`. Pesan peringatan muncul bila karakter $< 20$. Diverifikasi pada test case `AC-2`. |
| **AC-3**: Form meminta verifikasi PIN supervisor sebelum tombol submit aktif (`VAL-INT-005`) | ✅ Terpenuhi | Komponen menyediakan field PIN bertipe password. Evaluasi `isFormValid = isReasonValid && isPinValid;` memastikan tombol submit `ConfirmModal` membawa properti `disabled={!isFormValid || loading}`, sehingga tombol terkunci selama PIN kosong atau alasan $< 20$ karakter. Diverifikasi pada test case `AC-3`. |
| **AC-4**: Pengiriman form memanggil endpoint override backend `POST /api/v1/.../supervisor-override` | ✅ Terpenuhi | Fungsi `handleSubmit` memanggil service `submitSupervisorOverride(episodeId, { reason: trimmedReason, supervisorPin: trimmedPin })` dengan payload DTO yang sesuai dengan kontrak backend `BE-RWI-134`. Diverifikasi pada test case `AC-4 & AC-5`. |
| **AC-5**: Respons sukses memicu penyegaran status clearance gerbang pemulangan menjadi `Overridden` dan membuka tombol pemulangan fisik | ✅ Terpenuhi | Pada kartu gerbang pemulangan, callback `handleOverrideSuccess` memanggil `refreshBilling()`, memperbarui data kueri status kasir, mentransisikan tampilan gerbang menjadi `OVERRIDDEN`, dan mengaktifkan tombol kepulangan fisik. Diverifikasi pada test case `AC-4 & AC-5`. |

---

## 6. Spesifikasi Antarmuka API Terkait (Bergaya Swagger)

Komponen beroperasi mengeksekusi otorisasi darurat terhadap kontrak backend `BE-RWI-134`:

### `[Tags("Inpatient Discharge Clearance")]`
Otorisasi pemulangan darurat klinis oleh Supervisor Bangsal.

| Aspek | Spesifikasi |
| :--- | :--- |
| **Method & Endpoint** | `POST /api/v1/health-services/inpatient-management/episodes/{episodeId}/supervisor-override` |
| **Otorisasi / Hak Akses** | `InpatientSupervisor:Override` (Supervisor Bangsal / Kepala Ruangan) |
| **Deskripsi** | Mengesampingkan pemblokiran clearance kasir secara sah untuk evakuasi darurat klinis pasien, mencatat alasan audit dan identitas supervisor. |
| **Request Model** | `SupervisorOverrideRequestDto`<br>- `reason` (string, wajib, minimal 20 karakter — `VAL-INT-004`)<br>- `supervisorPin` (string, wajib, kredensial otorisasi supervisor — `VAL-INT-005`) |
| **Response Model (200 OK)** | `InpatientBillingStatusResponseDto`<br>- `clearanceStatus`: `"OVERRIDDEN"`<br>- `operationalStatusText`: `"Otorisasi Supervisor Aktif — Pelepasan Darurat"`<br>- `canPhysicallyDischarge`: `true`<br>- `isSupervisorOverridden`: `true`<br>- `overriddenBySupervisorName`: `"Ns. Ratna S.Kep"`<br>- `overrideReason`: `"Pasien mengalami syok kardiogenik..."`<br>- `overriddenAtUtc`: `2026-09-18T02:16:00Z` |
| **Respons Galat** | - `400 Bad Request`: Alasan klinis $< 20$ karakter atau PIN salah.<br>- `403 Forbidden`: Pengguna tidak memiliki wewenang supervisor bangsal.<br>- `404 Not Found`: Episode rawat inap tidak ditemukan. |

---

## 7. Bukti Verifikasi dan Pengujian

### 7.1 Pengujian Unit Otomatis (Node.js Test Runner)
```powershell
node --import ./tests/helpers/register.mjs --test "tests/unit/inpatient-supervisor-override-modal.test.mjs" "tests/unit/inpatient-discharge-clearance-gate.test.mjs" "tests/unit/inpatient-billing-status-badge.test.mjs" "tests/unit/inpatient-billing-census-detail.test.mjs" "tests/unit/inpatient-billing-financial-drawer.test.mjs" "tests/unit/billing-invoice-calculation-breakdown.test.mjs"
```
```text
✔ invoice tunai murni - seluruh subtotal jatuh ke Mandiri, tidak ada Pajak Asuransi (0.92ms)
✔ invoice ditanggung penuh asuransi - Subtotal Asuransi dan Pajak Asuransi menyerap seluruhnya (0.20ms)
✔ coverage sebagian - residual non-billable dikeluarkan dari Subtotal Mandiri (BKC-DES-021) (0.18ms)
✔ input kosong/null tidak melempar error dan mengembalikan seluruhnya nol (0.12ms)
✔ menerima bentuk PascalCase (unwrapEnvelope kadang meneruskan bentuk backend apa adanya) (0.15ms)
✔ FE-RWI-096 AC-1: Kolom Status Kasir terpasang di sensus bangsal dengan BillingStatusBadge (10.06ms)
✔ FE-RWI-096 AC-2: Kartu BillingSummaryCard menampilkan status kasir dan daftar kendala blocker (4.67ms)
✔ FE-RWI-096 AC-2: Kartu BillingSummaryCard steril dari angka rupiah dan privasi saldo terjaga (RWI-DEC-160) (1.59ms)
✔ FE-RWI-096 AC-3: Hook useInpatientBillingStatus mendukung polling 30 detik tanpa flicker (1.16ms)
✔ FE-RWI-096: Re-export alias pada path DoD roadmap tersedia (1.24ms)
✔ FE-RWI-097 AC-1: Tombol pembuka drawer diproteksi hak akses InpatientBilling:View (6.61ms)
✔ FE-RWI-097 AC-2: Drawer menampilkan rincian total biaya, penjamin, ekses, deposit, dan sisa kurang bayar (3.18ms)
✔ FE-RWI-097 AC-3: Format mata uang terformat rapi sesuai standar Rupiah (IDR) (23.79ms)
✔ FE-RWI-097: Re-export alias pada path DoD roadmap tersedia (2.42ms)
✔ FE-RWI-095 AC-1: Komponen mendefinisikan keenam status operasional kasir dengan label Bahasa Indonesia (7.14ms)
✔ FE-RWI-095 AC-2: Lencana steril dari angka rupiah dan saldo piutang (RWI-DEC-160, VAL-INT-006) (1.58ms)
✔ FE-RWI-095 AC-1 & AC-3: Efek visual berkedip (pulsating) pada status REVOKED terpasang (2.40ms)
✔ FE-RWI-095: Re-export alias pada path DoD roadmap tersedia (1.33ms)
✔ FE-RWI-098 AC-1: Tombol konfirmasi pulang fisik terkunci (disabled) saat status clearance PENDING (6.42ms)
✔ FE-RWI-098 AC-2: Tombol konfirmasi pulang fisik aktif saat clearance kasir disetujui (CLEARED / OVERRIDDEN) (1.94ms)
✔ FE-RWI-098 AC-3: Auto-Reblock seketika mengunci tombol dan menampilkan banner peringatan merah berkedip saat clearance REVOKED (1.78ms)
✔ FE-RWI-098 AC-4: Polling beroperasi cepat setiap 10 detik saat modal pemulangan aktif dibuka (1.36ms)
✔ FE-RWI-098 DoD & Integrasi: Komponen terpasang di inpatient-discharge-view.jsx dan re-export alias tersedia (1.50ms)
✔ FE-RWI-099 AC-1: Tombol Supervisor Override hanya tampil jika clearance Revoked/Pending DAN pengguna berwenang Supervisor (5.40ms)
✔ FE-RWI-099 AC-2: Modal menampilkan peringatan tanggung jawab hukum dan validasi alasan klinis minimal 20 karakter (VAL-INT-004) (3.49ms)
✔ FE-RWI-099 AC-3: Form meminta verifikasi PIN supervisor sebelum tombol submit aktif (VAL-INT-005) (1.79ms)
✔ FE-RWI-099 AC-4 & AC-5: Pengiriman form memanggil endpoint override backend dan menyegarkan status clearance (2.72ms)
✔ FE-RWI-099 DoD: Re-export alias SupervisorOverrideModal tersedia pada path roadmap (1.12ms)
ℹ tests 28 | suites 0 | pass 28 | fail 0 | cancelled 0 | skipped 0 | todo 0 | duration_ms 216.74
```

### 7.2 Pemeriksaan Kualitas Kode (ESLint)
```powershell
node ./node_modules/eslint/bin/eslint.js "src/components/features/health-services/inpatient-management/billing-integration/supervisor-override-modal.jsx" "src/components/features/inpatient/billing-integration/SupervisorOverrideModal.jsx" "src/components/features/health-services/inpatient-management/billing-integration/discharge-clearance-gate-card.jsx" "src/lib/services/health-services/inpatient-management/inpatient-billing.service.js"
```
**Hasil:** 0 error, 0 warning (Exit Code 0).

### 7.3 Pemeriksaan Anti-Regresi UI (Checklist UI Consistency)
- **Warna literal:** `git grep -nEi "#[0-9a-f]{3,8}\b|rgba?\(" src/style/health-services/inpatient-management/supervisor-override-modal.module.css` → **0 temuan** (Semua memakai token CSS `var(...)`).
- **Typography shared override:** **0 temuan**.
- **Tombol non-base (`<button`):** `git grep "<button" src/components/features/health-services/inpatient-management/billing-integration/supervisor-override-modal.jsx` → **0 temuan** (Aksi submit dan cancel didelegasikan via `ConfirmModal`).
- **Form input mentah (`<input`, `<textarea`):** `git grep -nEi "<(input|textarea)" src/components/features/health-services/inpatient-management/billing-integration/supervisor-override-modal.jsx` → **0 temuan** (Menggunakan `BaseTextAreaField` dan `BaseTextField`).
- **Aturan `!important`:** `git grep "!important" src/style/health-services/inpatient-management/supervisor-override-modal.module.css` → **0 temuan**.

---

## 8. Analisis Risiko dan Mitigasi

| Risiko | Dampak | Strategi Mitigasi |
| :--- | :--- | :--- |
| **Penyalahgunaan Wewenang Override Tanpa Justifikasi Klinis Valid** | Rumah sakit menanggung piutang macet yang besar akibat pelepasan pasien yang sebenarnya mampu membayar. | Diwajibkannya alasan minimal 20 karakter (`VAL-INT-004`), input PIN supervisor (`VAL-INT-005`), serta penayangan banner peringatan konsekuensi hukum audit direksi di dalam modal. |
| **Kredensial PIN Bocor ke Staf Bangsal Biasa** | Staf non-supervisor mengabaikan gerbang clearance kasir secara sepihak. | Kolom PIN disamarkan dengan `type="password"`, serta backend memverifikasi kepemilikan peran supervisor secara independen pada JWT token (`403 Forbidden`). |
| **Ketidaksesuaian State Pasca-Override** | Modal berhasil mengirimkan override namun antarmuka gerbang pemulangan tetap terkunci karena tidak ada penyegaran state. | Callback `onSuccess` langsung memanggil fungsi `refreshBilling()` milik hook `useInpatientBillingStatus`, memastikan transisi instan ke status `OVERRIDDEN` dan tombol pemulangan fisik aktif seketika. |

---

## 9. Keterlacakan Persyaratan (Traceability Matrix)

| Kode Kebutuhan | Deskripsi Kebutuhan | Terpenuhi Melalui |
| :--- | :--- | :---: |
| **`FR-INT-016`** | Fasilitas override kepulangan fisik oleh supervisor bangsal untuk kedaruratan klinis | Komponen `SupervisorOverrideModal.jsx` dan endpoint `POST .../supervisor-override` |
| **`VAL-INT-004`** | Alasan supervisor override wajib diisi minimal 20 karakter | Validasi `isReasonValid` di form modal dan pengecekan sisi klien |
| **`VAL-INT-005`** | Verifikasi otorisasi kredensial PIN supervisor | Field `supervisorPin` wajib terisi sebelum tombol aksi aktif |
| **`RWI-DEC-158`** | Keputusan integrasi billing dan alur pemulangan bangsal | Arsitektur modal terisolasi pada folder `billing-integration` |
| **`RWI-AC-238`** | Kriteria penerimaan clearance gate dan supervisor override | Pengujian unit otomatis `inpatient-supervisor-override-modal.test.mjs` (5/5 PASS) |
