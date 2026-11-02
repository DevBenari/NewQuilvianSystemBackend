# Laporan Perubahan Frontend — `FE-RWI-288`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-288` |
| Judul | Penyempurnaan status ketersediaan boks bayi kamar ibu dan penyegaran denah bed aktif pasca-penolakan service unit |
| Slice | Admisi Rawat Inap (`FE-INP-03`) — Penempatan & Denah Rawat Inap Bayi (Langkah 4) |
| Roadmap | [roadmap-repair-044.md](../../roadmap/roadmap-repair-044.md) |
| Trace | [ISSUE-EPS-044](../../docs/issue/issue-044-temuan-pengujian-admisi-bayi-baru-lahir.md), [PLAN-REPAIR-EPS-044](../../docs/plan-repair/plan-repair-044-temuan-pengujian-admisi-bayi-baru-lahir.md) (`FIX-EPS-044-04`, `FIX-EPS-044-05`) |
| Contract version | Selaras alur 8 langkah admisi bayi baru lahir |
| Dependency | `BE-RWI-258` |
| Klasifikasi | `MEDIUM` — 3 berkas frontend |
| Task mode | `FRONTEND` |
| Target tulis | `src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js`, `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`, `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx` |
| Branch | `HamzahV2` (SHA basis `9c4000724`) |
| Tanggal | 2026-11-02 |
| Status | ✅ SELESAI (Unit test PASS 24/24, ESLint PASS 0 error) |

---

## 1. Ringkasan Perubahan

1. **Pemisahan Status Boks Rawat Gabung (`inpatient-newborn-admission-utils.js`):**
   - Pada fungsi `pickRoomingInCrib`, jika `availableCribs` kosong tetapi boks fisik ditemukan di kamar ibu:
     - Dilakukan pengecekan `hasOccupied = cribs.some((crib) => crib.isOccupied === true)`.
     - Jika tidak ada boks yang terisi pasien lain (`isOccupied: false`), status yang dikembalikan adalah `unavailable` (bukan lagi salah menduga `occupied`).
     - Status `occupied` kini murni hanya dikembalikan bila boks tersebut memang nyata-nyata berstatus `isOccupied: true`.
2. **Banner Informatif Penataan Boks di Langkah 4 (`inpatient-newborn-admission-steps.jsx`):**
   - Menambahkan penanganan status `unavailable` pada komponen `RoomingInPlacementConfirmation`.
   - Menampilkan alert peringatan bernada informatif: *"Boks Bayi di Kamar Ibu Belum Siap Dipakai. Boks bayi di kamar ini sedang tidak tersedia atau dalam proses penataan/pemeliharaan."*
   - Memberikan tombol pengalihan yang jelas ke *"Alihkan ke Perawatan Perinatologi"* tanpa menuduh ibu memiliki bayi kembar yang sudah menempati boks.
3. **Penyegaran Denah Aktif Saat Penolakan Service Unit (`use-inpatient-admission-bed.jsx`):**
   - Pada penanganan kegagalan reservasi di mana backend mengembalikan galat `"service unit tidak valid, tidak aktif, atau tidak tersedia untuk registrasi"`, hook kini tidak hanya memanggil `legacyBoard.refresh()`, melainkan juga mengeksekusi `await effectiveFloorPlan?.refresh?.()`.
   - Memastikan denah visual kamar, status eligibilitas bed, dan timestamp pembaruan data disinkronkan secara aktual dengan kondisi database terbaru.

---

## 2. Berkas yang Diubah

1. `QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js`
2. `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx`
3. `QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx`
4. `QuilvianSystemFrontendDev/tests/unit/inpatient-newborn-admission.test.mjs`

---

## 3. Bukti Verifikasi

1. **Unit Testing:**
   - Menjalankan `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-newborn-admission.test.mjs`:
     - Pengujian unit `FE-RWI-287 dan FE-RWI-288 / ISSUE-044: Validasi perbaikan temuan pengujian admisi bayi` berhasil memvalidasi:
       - Boks unavailable dengan `isAvailable: false, isOccupied: false` terdeteksi berstatus `unavailable`.
       - Boks terisi dengan `isOccupied: true` terdeteksi berstatus `occupied`.
       - Pemanggilan `effectiveFloorPlan?.refresh?.()` hadir saat penolakan reservasi service unit.
     - Total 24 test case PASS (`pass 24, fail 0`).
2. **Linting:**
   - Menjalankan `npx eslint` pada seluruh berkas terdampak:
     - Hasil: `0 errors, 17 warnings` (bebas error).

---

## 4. Kesimpulan & Status

Task `FE-RWI-288` berstatus **✅ SELESAI**.
Temuan 4 (pesan keliru boks terisi bayi kembar) dan temuan 5 (denah tidak me-refresh setelah penolakan) telah tuntas diselesaikan.
