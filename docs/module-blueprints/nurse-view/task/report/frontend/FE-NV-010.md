# Laporan Perubahan Frontend — `FE-NV-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-NV-010` |
| Judul | Klien realtime panggilan pasien |
| Slice | `MVP-8` — task pendukung (gelombang frontend 1, revision 2) |
| Roadmap | `docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md` — bagian Revision 2, kartu `FE-NV-010` |
| Trace | `FR-NV-048`; `NV-AC-028` (sisi klien); `03-frontend-architecture.md` R2.5, R2.7; integration-contract R2 §8 |
| Contract version | `0.2.1` `approved` (`NV-DEC-057`); api §10 tidak berubah sejak `0.2.0` |
| Wewenang UI | Tidak ada; berkas ini bukan tampilan |
| Dependency | `BE-NV-014` ✅ (10 Nov 2026) |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), berkas diperiksa 9–20 (1), berkas diubah 1 (0), logika sedang (1), kontrak yang sudah ada (1), tanpa database (0), keamanan tidak tersentuh (0), UI tidak ada (0) |
| Task mode | `FRONTEND` — perintah Rizki 9 Nov 2026 ("lanjutkan BE-NV-014 lalu ke frontend … build dan pengujian belakangan") |
| Target tulis | `src/lib/realtime/queue-realtime-client.js` (berkas bersama, restu `NV-DEC-056`) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7d32e7cec` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `589fb513` + perubahan `BE-NV-014` belum di-commit (branch `rizkiG`) |
| Tanggal | 2026-11-09 |
| Status | ✅ **SELESAI 10 November 2026** — AC-1 dan AC-2 terpetakan ke source; AC-3 terbukti: skrip regresi TV `FE-NV-007` (F0–F5) dijalankan ulang tanpa perubahan, 6/6 `PASS` (join `JoinNurseViewFloor` tanpa argumen, muat ulang dari `NurseViewBoardChanged` ≤ 2 detik, jeda minimum 5 detik, sambung ulang dan join ulang, muat ulang 60 detik, satu-satunya method hub TV tetap `JoinNurseViewFloor`). AC-4 dibuktikan task pemakai (`FE-NV-011`, `012`, `014` lulus). `npx eslint` 0 error |

---

## 1. Keadaan yang ditemukan di awal

Klien realtime bersama sudah mengenali `NurseViewBoardChanged` dan grup lantai TV
(`joinNurseViewFloor`), tetapi belum mengenal sinyal panggilan pasien maupun grup konsol dan tablet.

## 2. Proses bisnis dari sisi pengguna

Tidak ada layar. Layar tablet (`FE-NV-011`) dan Konsol Panggilan (`FE-NV-012`) memakai fungsi ini
untuk menerima sinyal "panggilan berubah", lalu memuat ulang datanya sendiri. **Contoh:** Sari
mengklaim panggilan 326-C di konsol; tablet 326-C menerima sinyal dan langsung menampilkan
"Sari menuju".

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`queue-realtime-client.js` (pola `joinNurseViewFloor`, flag join ulang, `onreconnected`, `stop`),
`use-admission-queue-realtime.jsx`, `use-nurse-view-floor-board.jsx`, api §10, `QueueHub.cs`
hasil `BE-NV-014`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/realtime/queue-realtime-client.js` | +124/−0. Konstanta `NURSE_VIEW_PATIENT_CALL_EVENT_NAME`; opsi `onNurseViewPatientCallChanged`; `joinNurseViewCallConsole(floorCode)` / `leaveNurseViewCallConsole(floorCode)` dengan lantai terakhir diingat; `joinNurseViewBedTablet()` / `leaveNurseViewBedTablet()`; join ulang keduanya di `onreconnected`; dibersihkan di `stop()`; keempat fungsi diekspor |

### 3.3 Kepatuhan arsitektur frontend

Hanya penambahan; baris untuk grup lain tidak berubah (diff tanpa baris `-`). Pola sama dengan
`joinNurseViewFloor`.

`UI GATE: N/A — task ini hanya menyentuh klien realtime, tanpa elemen tampilan.`

## 4. State yang ditangani di layar

Tidak ada layar. Galat join diteruskan ke pemanggil (`Promise` ditolak), seperti fungsi join lain.

## 5. Endpoint yang dikonsumsi

#### Hub realtime `/hubs/queues`

| Jenis | Nama | Argumen |
| --- | --- | --- |
| Method | `JoinNurseViewCallConsole` / `LeaveNurseViewCallConsole` | `floorCode` |
| Method | `JoinNurseViewBedTablet` / `LeaveNurseViewBedTablet` | — |
| Event | `NurseViewPatientCallChanged` | `{ callId, bedId, floorCode, changeType, occurredAtUtc }` |

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint src/lib/realtime/queue-realtime-client.js` (bersama berkas lain) | 0 error, 0 warning |
| `git diff --stat` | `124 insertions(+)`, 0 baris dihapus |
| Nama method dan event dibandingkan dengan `QueueHub.cs` dan api §10 | Sama persis |
| Uji regresi TV papan (`uji-nv-007-20261103/uji-layar-sinyal.mjs`, tanpa perubahan) | F0–F5 6/6 lulus | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-007-20261103/layar-2026-11-10T01-54-59-644Z/` (JSON per skenario, rekaman video) |

`AUTOMATED TEST: SKIPPED (opsional) — repository tidak memakai Jest; uji runtime ditunda atas permintaan Rizki.`
`MANUAL TEST: PASS — 10 Nov 2026. Diuji di `next dev` port 3000 dengan backend build 9 Nov 15.59 (`dotnet run --no-build`), peramban Edge headless.`

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 nama event, method hub, argumen sama dengan api §10 | ✅ Terpetakan ke source | §5 |
| AC-2 diff hanya menambah | ✅ Terbukti | `git diff`: 0 baris dihapus |
| AC-3 regresi TV papan | ✅ Terbukti 10 Nov 2026 | F0–F5 6/6; tiga TTV uji dibatalkan kembali |
| AC-4 penerimaan sinyal di layar | Dibuktikan task pemakai (`FE-NV-011`, `012`, `014`) | — |
| DoD — lint `0 errors` | Terpenuhi | §6 |
| DoD — regresi TV tercatat | Terpenuhi 10 Nov 2026 | `QuilvianSystemFrontendDev/test-with-agy/nurse-view/uji-nv-007-20261103/layar-2026-11-10T01-54-59-644Z/` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Regresi realtime layar lain yang memakai klien ini; diff hanya menambah, tetapi baru terbukti lewat uji |
| Status Git | Sudah di-commit Rizki |
| Langkah berikutnya | — |
