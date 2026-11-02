# Laporan Perubahan Frontend — `FE-RWI-065`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-065` |
| Judul | Petugas melihat peringatan sebelum menutup dan ringkasan akibat sesudahnya |
| Slice | Gelombang 1 — `PRD-RWI-V2-001`, `EPIC RI-41` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) bagian 4, kartu `FE-RWI-065` |
| Trace | `FR-RI-198`, `FR-RI-199`, `FR-RI-200`, `FR-RI-201`; `RWI-DEC-138`, `RWI-DEC-143`; `data/data-dictionary.md` 18.4; `contracts/api-contract.md` `0.9.0` bagian 10.4; `03-frontend-architecture.md` 12.3.3; `UAT-50`, `UAT-51` |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Wewenang UI | `FE-INP-23` — perubahan layar penutupan `FE-INP-07` |
| Dependency | `BE-RWI-084` [BE] ✅ selesai 17 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 7, berkas diubah 6 (source frontend) + 1 berkas test unit baru, kontrak API 2, UI/workflow 2 |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `QuilvianSystemFrontendDev/tests/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Branch frontend | `HamzahV2` |
| Branch backend | `MHamzah` |
| Tanggal | 22 Oktober 2026 |
| Status | ✅ **Selesai 22 Oktober 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-5) terbukti pada source, komponen visual, dan suite unit test otomatis (`34/34 passing`). `npm run lint:errors` bersih (0 error). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini diselesaikan, layar **Penutupan Episode Rawat Inap (`FE-INP-07`)** hanya memfokuskan perhatian petugas admisi pada 5 syarat penutupan:
1. Keputusan cara pulang sudah ditetapkan oleh DPJP
2. Resume medis pulang telah ditandatangani secara digital oleh DPJP
3. Seluruh butir administrasi kepulangan telah diverifikasi
4. Izin kasir / kelayakan finansial telah disetujui oleh petugas Kasir
5. Status fisik tempat tidur telah dibersihkan / diselesaikan oleh petugas ruangan

Namun, transaksi penutupan episode rawat inap di sisi sistem rumah sakit memicu serangkaian **dampak otomatis (side-effects)** yang signifikan secara administratif dan klinis:
- Konsep catatan dokter yang belum ditandatangani otomatis dikunci permanen (`UnsignedDoctorDrafts` / `lockedDraftCount`).
- Pesanan tindakan medis yang tertunda dan belum ditagih otomatis dibatalkan (`PendingProcedureOrders` / `cancelledProcedureOrderCount`).
- Pesanan tindakan yang sudah ditagih sengaja dibiarkan untuk tindak lanjut modul Billing (`BilledPendingProcedureOrders` / `billedPendingProcedureOrderCount`).
- Dosis obat masa lalu yang belum dicatat di MAR dan dosis masa depan otomatis ditandai (`UnrecordedPastDoses` / `cancelledFutureDoseCount`).

**Petugas admisi bukanlah tenaga medis**. Jika layar tidak memberikan transparansi sebelum tombol ditekan:
1. Petugas tidak mengetahui bahwa ada konsep catatan dokter yang akan terkunci selamanya tanpa tanda tangan.
2. Petugas tidak menyadari ada tindakan yang belum dilaksanakan namun akan batal.
3. Setelah penutupan berhasil, sistem tidak memperlihatkan fakta riil berapa banyak berkas atau pesanan yang benar-benar terdampak.
4. Jika peringatan dirancang secara salah (misalnya dengan banner merah galat `danger`), petugas akan merasa ragu atau salah mengira bahwa episode dilarang ditutup, padahal peringatan konsekuensi ini bersifat **non-blocking** (informatif).

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Petugas Admisi atau Supervisor Ruangan yang memiliki hak akses penutupan episode (`InpatientDischarge : Close` / `InpatientDischarge : CloseOverride`). Pengguna lain berada dalam mode tinjau (*read-only*).

**Kapan layar dibuka.** Saat pasien rawat inap berada pada status persiapan pulang (`DischargePending`) dan seluruh 5 syarat kelayakan penutupan sedang diverifikasi.

### Langkah Berurutan — Petugas Menutup Episode dengan Transparansi Konsekuensi:

1. **Membuka Layar Penutupan Episode:**
   Petugas admisi membuka layar Penutupan Episode Tn. Joko Santoso (`FE-INP-07`).
2. **Memeriksa 5 Syarat Utama:**
   Petugas melihat panel kelayakan 5 syarat. Bila seluruhnya terpenuhi, statusbadge menampilkan *"Siap Ditutup"*.
3. **Membaca Panel Peringatan Konsekuensi Otomatis (`FE-INP-23`):**
   - Tepat di bawah panel syarat, petugas melihat kartu informatif *"Peringatan Konsekuensi Penutupan"*.
   - Kartu berpenanda *"Non-blocking"* dengan palet informasi yang netral (`variant="info"`), **bukan** galat merah penolakan.
   - Layar menguraikan secara transparan daftar dampak dari endpoint kesiapan:
     * *Konsep Catatan Dokter:* 1 konsep catatan belum ditandatangani oleh dr. Budi Santoso, Sp.PD.
     * *Pesanan Tindakan Tertunda:* 1 pesanan rontgen thorax AP belum ditagih yang akan otomatis dibatalkan.
     * *Dosis Obat:* 1 dosis obat antibiotik masa depan yang akan otomatis dibatalkan.
4. **Tombol Tutup Episode Tetap Aktif (Invarian Non-blocking):**
   - Petugas memastikan bahwa tombol **"Tutup Episode"** tetap berwarna biru aktif dan dapat diklik.
   - Petugas memahami bahwa sistem memberi tahu konsekuensi administratif, bukan melarang penutupan.
5. **Menekan Tombol "Tutup Episode":**
   - Dialog konfirmasi muncul untuk memastikan kembali keputusan petugas.
   - Petugas mengonfirmasi penutupan.
6. **Menerima Ringkasan Akibat Faktual Pasca-Penutupan:**
   - Transaksi database backend selesai secara atomik.
   - Banner sukses hijau muncul: *"Episode rawat inap berhasil ditutup"*.
   - Di dalam kartu keberhasilan, layar merender panel **Ringkasan Akibat Transaksi Penutupan Faktual**:
     * 1 Konsep Catatan Dokter Dikunci
     * 1 Pesanan Tindakan Dibatalkan
     * 0 Pesanan Tertagih
     * 1 Dosis Obat Dibatalkan
   - **Prinsip Kejujuran Data (AC-4):** Angka ringkasan pasca-penutupan ini diambil **murni dari respons penutupan backend** (`closedResult.sideEffects`), bukan dari perkiraan panel kesiapan pra-penutupan.
7. **Penanganan Skenario Gagal (`UAT-51` / AC-5):**
   - Jika koneksi terputus atau server mengembalikan galat transaksi, layar menampilkan pesan jelas: *"Penutupan gagal disimpan, coba lagi"*.
   - Episode tetap bertahan pada status `DischargePending` dan tidak berubah menjadi `Closed`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Jenis | Perubahan |
| --- | --- | --- |
| `src/lib/constants/health-services/inpatient-management/inpatient-closure-constants.jsx` | Source Frontend | Menambahkan `CLOSURE_WARNING_CODES` (`UNSIGNED_DOCTOR_DRAFTS`, `PENDING_PROCEDURE_ORDERS`, `BILLED_PENDING_PROCEDURE_ORDERS`, `UNRECORDED_PAST_DOSES`) dan kamus label Bahasa Indonesia `CLOSURE_WARNING_LABELS`. |
| `src/utils/health-services/inpatient-management/inpatient-closure-utils.jsx` | Source Frontend | Memperbarui `normalizeClosureReadiness` untuk mengekstrak array `warnings` (`code`, `count`, `message`, `details`, `isMeasured`). Menambahkan fungsi normalizer `normalizeClosureSideEffects` untuk mengekstrak ringkasan akibat transaksi penutupan. |
| `src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx` | Source Frontend | Memperbarui `normalizeEpisodeDetail` agar memetakan objek `sideEffects` dari payload `InpatientEpisodeDetailResponse`. |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx` | Source Frontend | Memperbarui `handleClose` dan `handleCloseWithOverride` untuk menyematkan `sideEffects` pada state `closedResult`. Menyelaraskan pesan galat transaksi penutupan dengan kalimat standar `UAT-51`: *"Penutupan gagal disimpan, coba lagi"*. Mengekspos properti `warnings` pada return hook. |
| `src/style/health-services/inpatient-management/inpatient-closure.module.css` | Source Frontend | Menambahkan kelas CSS token-based: `.warningsSection`, `.warningsHeader`, `.warningsTitle`, `.warningsList`, `.warningItemCard`, `.warningItemHeader`, `.warningItemTitle`, `.warningItemMessage`, `.warningItemDetails`, `.sideEffectsCard`, `.sideEffectsTitle`, `.sideEffectsGrid`, `.sideEffectItem`, `.sideEffectCount`, `.sideEffectLabel`. Mematuhi 100% token CSS dan `color-mix`, 0 hardcoded colors. |
| `src/components/view/health-services/inpatient-management/inpatient-closure-view.jsx` | Source Frontend | Menyisipkan `<section className={styles.warningsSection} data-testid="closure-warnings-section">` non-blocking di antara syarat penutupan dan tabs panel. Merender kartu akibat faktual `closure-actual-side-effects` di dalam section keberhasilan penutupan. |
| `tests/unit/inpatient-closure-warnings.test.mjs` | Test Unit | Membuat unit test suite native `node --test` untuk memvalidasi konstanta peringatan, normalisasi data warnings & sideEffects, keberadaan test-id pada view, dan invarian tombol tetap aktif. |

---

## 4. Bukti Verifikasi dan Pengujian

### 4.1 Verifikasi Unit Test Otomatis (`node --test`)

Pengujian dijalankan pada suite pengujian penutupan episode:
```bash
node --test tests/unit/inpatient-closure-warnings.test.mjs tests/unit/inpatient-closure.test.mjs tests/unit/inpatient-closure-redesign.test.mjs
```

**Hasil:**
```text
✔ Closure UI Redesign: Seluruh komponen modular penutupan episode terpasang (9.9913ms)
✔ Closure UI Redesign: InpatientClosureView mengintegrasikan modul dan segmented tabs (2.6754ms)
✔ Closure UI Redesign: Seluruh Test IDs penting dipertahankan untuk kompatibilitas E2E (2.2807ms)
✔ FE-RWI-065 Constants: Penanda peringatan konsekuensi penutupan terdefinisi lengkap (1.3992ms)
✔ FE-RWI-065 Utils: normalizeClosureReadiness menormalisasi warnings dan menjaga isReady tetap true (1.8577ms)
✔ FE-RWI-065 Utils: normalizeClosureSideEffects mengekstrak dampak riil dari transaksi penutupan (1.7084ms)
✔ FE-RWI-065 Utils: normalizeEpisodeDetail menyertakan sideEffects dari respons penutupan (1.7214ms)
✔ FE-RWI-065 View: InpatientClosureView merender panel peringatan non-blocking dan kartu ringkasan akibat (7.9647ms)
✔ kriteria 1: kelima syarat terbaca beserta tanda sudah atau belum (3.2662ms)
... [seluruh 34 test lulus tanpa kegagalan]
ℹ tests 34
ℹ suites 0
ℹ pass 34
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 287.6208
```

### 4.2 Verifikasi Linter (`eslint`)

```bash
npx eslint src/components/view/health-services/inpatient-management/inpatient-closure-view.jsx src/lib/hooks/health-services/inpatient-management/use-inpatient-closure.jsx src/utils/health-services/inpatient-management/inpatient-closure-utils.jsx src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx src/lib/constants/health-services/inpatient-management/inpatient-closure-constants.jsx
```
**Hasil:** Bersih (Exit code 0, 0 warning, 0 error).

---

## 5. Pemenuhan Acceptance Criteria & Definition of Done

| Acceptance Criteria | Status | Bukti di Kode |
| --- | :---: | --- |
| **AC-1:** Panel kesiapan menampilkan ketiga jenis akibat beserta jumlahnya dari endpoint kesiapan. | ✅ LULUS | Komponen `closure-warnings-section` merender list `warnings` yang dinormalisasi dari backend: `UnsignedDoctorDrafts`, `PendingProcedureOrders`, `BilledPendingProcedureOrders`, dan `UnrecordedPastDoses` beserta jumlah (`count`) dan detail catatan. |
| **AC-2:** Tombol Tutup **tetap aktif** walau ada peringatan — peringatan tidak pernah mematikan tombol. | ✅ LULUS | Tombol `closure-close-submit` dikendalikan oleh `!isReady || closeLoading || Boolean(closedResult)`. Nilai `isReady` hanya mengevaluasi kelima `conditions`, sama sekali tidak memblokir saat `warnings.length > 0`. |
| **AC-3:** Peringatan tampil sebagai informasi, bukan sebagai galat merah yang terbaca seperti larangan. | ✅ LULUS | Menggunakan styling `bg-info-subtle`, badge `Non-blocking`, kartu beraksen `--color-info` / netral, bukan banner galat `--color-danger`. |
| **AC-4:** Setelah penutupan berhasil, ringkasan akibat berasal dari **respons penutupan** (`detail.sideEffects`), bukan dari perkiraan panel kesiapan sebelumnya. | ✅ LULUS | Komponen `closure-actual-side-effects` membaca data dari `closedResult.sideEffects` yang diekstrak langsung dari payload balasan `inpatientDischargeService.post(.../close)`. |
| **AC-5:** Penutupan yang gagal menampilkan *"Penutupan gagal disimpan, coba lagi"*, dan layar tetap pada episode berstatus `DischargePending` (`UAT-51`). | ✅ LULUS | `useInpatientClosure` menangani error response dengan menyetel pesan default `Penutupan gagal disimpan, coba lagi.` dan state `episode` tidak beralih ke `Closed`. |

---

## 6. Kesimpulan

Task `FE-RWI-065` (`FE-INP-23`) telah **selesai 100%** dengan mematuhi seluruh konstitusi rekayasa Quilvian, standar token visual, prinsip non-blocking untuk keselamatan klinis/operasional, serta pembuktian unit test otomatis.
