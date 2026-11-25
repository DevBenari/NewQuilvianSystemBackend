# Laporan Perubahan Frontend — `FE-HRD-307`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-307` |
| Judul | Matriks roster: Ganti Petugas, tambah dan hapus shift pada roster terbit |
| Slice | `A52` / `MVP-67` / `EPIC HRD-110` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 25, kartu `FE-HRD-307` |
| Trace | `FR-PK-003` s.d. `FR-PK-007`; `HRD-DEC-195`; `03-frontend-architecture.md` 31.2 (baris Ganti Petugas dan Hapus shift), 31.3 |
| Contract version | `v26` — `api-contract.md` bagian 32 (`replace`, `assignments`, `shifts/{id}/remove`), `approved` 12 November 2026 |
| Wewenang UI | Modal Ganti Petugas (ubah), modal Hapus Shift (baru), dan keterangan kisi pada `/hr/scheduling/roster/[id]`. Tata letak dan penempatan tombol `DEV_DISCRETION` (`00-interview-decisions.md` 52.6); base component wajib |
| Dependency | `FE-HRD-306` ✅, `BE-HRD-338` ✅, `BE-HRD-340` ✅ |
| Klasifikasi | `MEDIUM` — satu view, dua modal (satu baru), satu utilitas, uji unit dan uji layar |
| Task mode | `FRONTEND` (instruksi pengguna 23 November 2026: "lanjut sesuai saran") |
| Target tulis | `QuilvianSystemFrontendDev` (`src/components/view/hr/scheduling`, `src/utils/hr/scheduling`, `tests/unit`, `tests/e2e`); laporan ini dan tautannya di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `52d3392d2` (branch `YogaV2`) + working tree `FE-HRD-306` |
| Commit backend yang dijadikan rujukan | `8a68ab67` (branch `yoga`) |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpenuhi di browser dengan API tiruan; UAT dengan data nyata devYoga belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Bukti | Dampak |
| --- | --- | --- |
| Ganti Petugas tidak mengirim petugas yang terlihat di layar | `QuickShiftReplacementModal.jsx` hanya mengirim `replacementEmployeeId`, `reasonCategory`, `notes` | Bila petugas sudah diganti orang lain, penggantian tetap berjalan tanpa peringatan; server baru bisa menolak 409 bila ruas itu dikirim (`VR-PK-05`) |
| Galat Ganti Petugas hanya tampil sebagai satu kalimat | Modal memakai `useHrAction`, yang menyimpan teks pesan saja | Kode `B*`/`W*` tidak tampil. Peringatan W1–W7 tidak bisa disimpan dengan alasan, sehingga penggantian yang hanya memicu peringatan selalu gagal 422 |
| Tidak ada aksi Hapus Shift | Tidak ada pemanggilan `.../shifts/{id}/remove` di `src/` | `FR-PK-006` belum bisa dijalankan pengguna |
| Tambah shift pada roster terbit sudah benar | `AddRosterShiftModal.jsx` (MSL-32, 18 November 2026) mengirim alasan, menampilkan B*/W*, dan memberi tahu bahwa shift dicatat di Riwayat Revisi; tidak ada teks "roster kembali ke draft" di seluruh `src/` | Kriteria tambah shift sudah terpenuhi sebelum task; tidak diubah |

---

## 2. Proses bisnis dari sisi pengguna

Contoh: roster ICU Februari 2027 sudah terbit. Kepala Unit ICU membuka **Kisi Roster Unit** (`/hr/scheduling/roster/{id}`). Keterangan kisi kini berbunyi: "Klik sel bershift untuk mengganti petugas atau menghapus shift; klik sel kosong untuk menambah shift. Roster tetap terbit dan setiap perubahan dicatat di Riwayat Revisi."

**Ganti Petugas (UAT-PK-03).**

1. Kepala Unit mengklik sel Malam 14 Feb milik Budi. Modal **Ganti Petugas Shift** menampilkan petugas saat ini, tanggal, dan shift.
2. Ia memilih Tono sebagai pengganti, kategori "Petugas sakit mendadak", dan mengisi keterangan. Tombol **Ganti Petugas** baru aktif sesudah ketiganya terisi.
3. Saat dikirim, layar juga mengirim "petugas yang saya lihat = Budi". Selama permintaan berjalan, tombol menampilkan pemutar dan tidak bisa diklik lagi.
4. Bila berhasil, modal tertutup, muncul pemberitahuan "Petugas shift diganti dan langsung berlaku.", dan kisi dimuat ulang.

**Jalur tidak normal pada Ganti Petugas.**

| Kejadian | Yang dilihat Kepala Unit |
| --- | --- |
| Server menolak dengan blokir, misalnya Rina masih Didampingi (UAT-PK-04) | Kotak merah "B7: Rina masih Didampingi dan shift ini tanpa perawat Mandiri.". Tombol simpan terkunci. Bila pengganti diubah, kotak itu hilang dan tombol aktif lagi, sebab pengganti baru akan diperiksa ulang server |
| Pengganti sedang ditempatkan sementara di unit lain (UAT-PK-05) | Kode dan pesan server tampil apa adanya, misalnya "B8: Tono ditempatkan di HCU 1–14 Maret 2027."; tombol terkunci |
| Server memberi peringatan, misalnya W2 lima hari berturut-turut | Pesan server "Pilih alasan untuk setiap peringatan sebelum menyimpan." dan kotak kuning "W2: …". Tombol berubah menjadi **Ganti dengan Alasan**; kategori dan keterangan yang sudah diisi dikirim sebagai alasan untuk setiap kode peringatan |
| Petugas sudah diganti orang lain sejak layar dibuka (409) | Pesan server "Shift ini baru saja diubah orang lain. Muat ulang lalu coba lagi." dan tombol **Muat Ulang Roster**. Simpan terkunci. Tombol itu menutup modal dan memuat ulang kisi |
| Unit di luar cakupan atau periode ditutup (403) | Pesan server, misalnya "Anda tidak berwenang mengubah roster unit ini." |

**Hapus Shift (UAT-PK-06).**

1. Kepala Unit mengklik sel Siang 21 Feb milik Tono. Di bagian bawah modal Ganti Petugas ada tombol merah **Hapus Shift Ini**.
2. Modal konfirmasi **Hapus Shift dari Roster Terbit?** menampilkan petugas, tanggal, dan shift, serta menjelaskan bahwa roster tetap terbit dan penghapusan dicatat di Riwayat Revisi.
3. Kategori alasan dan keterangan wajib diisi; tombol **Hapus Shift** terkunci sampai keduanya terisi.
4. Bila berhasil, muncul "Shift dihapus dari roster dan dicatat di Riwayat Revisi." dan kisi dimuat ulang. Bila 409, tampil pesan server dan **Muat Ulang Roster**; bila 403, tampil pesan server.

**Peran.**

| Hak pengguna | Perilaku sel bershift pada roster terbit |
| --- | --- |
| `ShiftReplacement : Update` dan `RosterPeriod : RemoveShift` | Membuka Ganti Petugas, dengan tombol Hapus Shift Ini |
| Hanya `ShiftReplacement : Update` | Membuka Ganti Petugas tanpa tombol Hapus Shift Ini |
| Hanya `RosterPeriod : RemoveShift` | Langsung membuka modal Hapus Shift |
| Tanpa keduanya (misalnya Kepala Bidang yang hanya membaca) | Sel terkunci |

Menyembunyikan tombol bukan pengganti pemeriksaan hak: server tetap memeriksa hak dan cakupan unit.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-HRD-307`; `api-contract.md` 32; `03-frontend-architecture.md` 31.2–31.3; `04-prd-to-mvp.md` (`UAT-PK-03` s.d. `UAT-PK-06`); `ShiftReplacementService.cs` dan `RosterPeriodService.RemovePublishedShiftAsync` (bentuk galat 422/409/403); DTO `DirectShiftReplacementRequest`, `RemovePublishedShiftRequest`, `WarningReasonItemRequest`; `roster-matrix-detail-view.jsx`, `QuickShiftReplacementModal.jsx`, `AddRosterShiftModal.jsx`, `RetroactiveShiftCorrectionModal.jsx`; `confirm-modal.jsx`, `filter-select.jsx`, `use-hr-remote-data.js`; spec `crm-membership-registration-screen.spec.mjs` sebagai templat uji layar.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx` | Mengirim `expectedWorkforceProfileId` dan `warningReasons`. Menampilkan kode dan pesan B*/W* dari 422; blokir mengunci simpan sampai pengganti diganti; peringatan disimpan ulang dengan alasan. 409 menampilkan pesan server dan **Muat Ulang Roster**. Tombol **Hapus Shift Ini** bila berhak |
| `src/components/view/hr/scheduling/modals/RemovePublishedShiftModal.jsx` (baru) | Modal konfirmasi hapus shift: ringkasan shift, kategori dan keterangan wajib, tombol bertanda bahaya; 409 dan 403 ditangani |
| `src/components/view/hr/scheduling/roster-matrix-detail-view.jsx` | Hak `RosterPeriod : RemoveShift`; sel terbit bershift dapat dibuka bila berhak ganti **atau** hapus; modal hapus; muat ulang sesudah 409; keterangan kisi diperbarui; komentar path `shift-replacements` |
| `src/components/view/hr/scheduling/modals/RetroactiveShiftCorrectionModal.jsx` | Komentar path diperbarui ke `shift-replacements` (path kodenya sudah berubah di `FE-HRD-306`) |
| `src/utils/hr/scheduling/roster-shift-change-utils.js` (baru) | `classifyRosterChangeError` (blokir, peringatan, basi 409, pesan server), `buildWarningReasons` (satu alasan per kode), `formatViolation` |
| `tests/unit/hr-roster-shift-change-utils.test.mjs` (baru) | 4 uji utilitas |
| `tests/e2e/hr-roster-published-change.spec.mjs` (baru) | 10 skenario layar dengan API tiruan |

### 3.3 Kepatuhan arsitektur frontend

View tetap memanggil `RosterSchedulingService` dari `FE-HRD-306`, sama seperti modal tetangganya (`AddRosterShiftModal`), sehingga tidak ada state Redux paralel untuk halaman yang seluruhnya memakai `useHrRemoteData`. Pemilahan galat ditaruh sebagai fungsi murni di `src/utils/hr/scheduling` (tanpa request, tanpa Redux) dan dibangun di atas `toHrApiError`. Seluruh kontrol memakai base component: `ConfirmModal`, `BaseSelectField`, `BaseTextAreaField`, `InformationAlert`, `BaseButton`, `HrInfoGrid`. Tidak ada warna atau gaya baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol konfirmasi menampilkan pemutar dan terkunci selama permintaan berjalan (uji klik ganda: satu permintaan) |
| Kosong | Sel tanpa shift menampilkan "–"; perilaku kisi kosong tidak berubah |
| Gagal | Pesan server apa adanya; 422 per kode `B*`/`W*`; 409 dengan **Muat Ulang Roster** |
| Tanpa hak akses | Tombol atau sel disembunyikan/terkunci sesuai hak; 403 dari server menampilkan pesan server, misalnya "Anda tidak berwenang mengubah roster unit ini." |

---

## 5. Endpoint yang dikonsumsi

#### HR Scheduling - Shift Assignment & Replacement

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/scheduling-management/shift-replacements/{id}/replace` | Ganti Petugas (+ `expectedWorkforceProfileId`, `warningReasons`) | `ShiftReplacement : Update` |

#### HR Scheduling - Roster Period

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}` | Memuat ulang kisi sesudah berhasil atau 409 | `RosterPeriod : Read` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/shifts/{shiftAssignmentId}/remove` | Hapus Shift dengan alasan | `RosterPeriod : RemoveShift` |
| `POST` | `/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/assignments` | Tambah shift pada roster terbit (tidak diubah; sudah sesuai `v26`) | `RosterPeriod : Create` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji layar `npx playwright test tests/e2e/hr-roster-published-change.spec.mjs` (build produksi, port 3710, API tiruan penuh, nol tulis ke backend) | 10/10 lulus | `PASS` | Keluaran Playwright |
| — UAT-PK-03 Ganti Budi → Tono | Badan permintaan memuat `expectedWorkforceProfileId` Budi tanpa `warningReasons`; pemberitahuan sukses; kisi dimuat ulang; tidak ada teks "kembali ke draft" | `PASS` | Skenario 1 |
| — UAT-PK-04 422 `B7` | "B7: …" tampil, simpan terkunci, pengganti lain membuka kunci | `PASS` | Skenario 2 |
| — UAT-PK-05 penempatan sementara | "B8: Tono ditempatkan di HCU 1–14 Maret 2027." tampil, simpan terkunci | `PASS` | Skenario 3 |
| — Peringatan `W2` | Kode tampil, **Ganti dengan Alasan** mengirim `warningReasons` satu butir untuk `W2`, lalu berhasil | `PASS` | Skenario 4 |
| — 409 Ganti Petugas | Pesan server, simpan terkunci, **Muat Ulang Roster** menutup modal dan memuat ulang | `PASS` | Skenario 5 |
| — Klik ganda | Satu permintaan saja | `PASS` | Skenario 6 |
| — UAT-PK-06 Hapus Shift | Tombol terkunci sampai kategori dan keterangan terisi; badan `{ reasonCategory, reasonNote }`; pemberitahuan sukses | `PASS` | Skenario 7 |
| — Hapus Shift 409 dan 403 | Pesan server tampil; 409 dengan Muat Ulang Roster | `PASS` | Skenario 8 |
| — Tanpa `RemoveShift` | Tombol Hapus Shift Ini tidak tampil | `PASS` | Skenario 9 |
| — Hanya `RemoveShift`; tanpa keduanya | Sel langsung membuka Hapus Shift; tanpa keduanya sel terkunci | `PASS` | Skenario 10 |
| Putaran pertama uji layar | 8 gagal karena pemilih uji salah: pemicu `FilterSelect` dinamai label ruasnya, bukan placeholder. Tangkapan layar menunjukkan modal tampil benar. Pemilih diperbaiki ke `button[aria-haspopup="listbox"]`, tanpa perubahan source | `PASS` sesudah perbaikan uji | Keluaran Playwright dan tangkapan layar |
| `tests/unit/hr-roster-shift-change-utils.test.mjs` + `hr-a52-api-client.test.mjs` | 16/16 lulus | `PASS` | Keluaran perintah |
| Kontrol negatif utilitas: 4 mutasi (409 tidak ditandai basi; blokir/peringatan tertukar; alasan ganda per kode; pegawai tidak dikirim) | 4/4 tertangkap; source dipulihkan | `PASS` | `mut307.cjs` di scratchpad sesi |
| `npm run test:unit` (seluruh suite) | 5.230 lulus, 100 gagal dari 5.330 | `EXISTING / ENVIRONMENT ISSUE` | Daftar 100 uji yang gagal identik dengan sebelum task (dibandingkan per nama dengan putaran `FE-HRD-306`); seluruhnya di modul lain |
| `npm run lint:errors` (seluruh repository) | Tanpa error | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil | `PASS` | Keluaran perintah |

AUTOMATED TEST: `npm run test:unit` — PASS untuk berkas task; suite penuh membawa 100 kegagalan lama di modul lain.

Uji manual: `PASS` — seluruh kontrol yang diubah (pilih pengganti, kategori, keterangan, Ganti Petugas, Ganti dengan Alasan, Muat Ulang Roster, Hapus Shift Ini, Hapus Shift, Batal) dijalankan di peramban Chromium lewat Playwright terhadap build produksi, dengan jawaban API tiruan untuk setiap kode status.

**Tidak dijalankan:** UAT-PK-03 s.d. UAT-PK-06 dengan data roster nyata di devYoga. Skenario itu mengganti dan menghapus shift sungguhan di basis data bersama dan memerlukan akun Kepala Unit beserta izin pengguna. Bentuk galat yang ditirukan diambil dari source backend (`ShiftReplacementService`, `RosterPeriodService`), dan perilaku backend-nya sudah dibuktikan `AT-HRD-PK-02` s.d. `-06` (API devYoga, laporan `BE-HRD-351`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 422 menampilkan kode `B*`/`W*` dan pesannya | Terpenuhi | Skenario 2, 3, 4 (B7, B8, W2 tampil sebagai "kode: pesan server") |
| 409 meminta muat ulang | Terpenuhi | Skenario 5 dan 8 (pesan server, simpan terkunci, Muat Ulang Roster memuat ulang kisi) |
| Hapus meminta konfirmasi dan alasan | Terpenuhi | Skenario 7 (modal konfirmasi; tombol terkunci sampai kategori dan keterangan terisi) |
| Outcome: Ganti Petugas mengirim `ExpectedWorkforceProfileId` dan alasan peringatan; tambah shift tidak memperingatkan "kembali ke draft" | Terpenuhi | Skenario 1 dan 4; pencarian teks di `src/` dan skenario 1 |
| Risiko: Kepala Bidang melihat tombol ubah | Ditangani | Skenario 9 dan 10 (tombol disembunyikan/sel terkunci sesuai hak); server tetap memeriksa |
| DoD umum: base component, loading/galat, tombol nonaktif saat mengirim, 403 "tidak berwenang", uji unit dan uji browser | Terpenuhi | Bagian 3.3, 4, dan 6 |
| Verifikasi kartu: `UAT-PK-03` s.d. `UAT-PK-06` di browser | Terpenuhi dengan API tiruan; data nyata devYoga `NOT RUN` | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sel roster **Draft** yang bershift tetap membuka modal koreksi seperti sebelumnya (di luar cakupan task) |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | Playwright mengubah `test-results/.last-run.json` (ter-track); dipulihkan dari `HEAD` dengan akhir baris CRLF sehingga tidak lagi muncul di `git status` |
| Interupsi | `NONE` |
| Status Git | ` M src/components/view/hr/scheduling/modals/QuickShiftReplacementModal.jsx`, ` M src/components/view/hr/scheduling/modals/RetroactiveShiftCorrectionModal.jsx`, ` M src/components/view/hr/scheduling/roster-matrix-detail-view.jsx`, `?? src/components/view/hr/scheduling/modals/RemovePublishedShiftModal.jsx`, `?? src/utils/hr/scheduling/roster-shift-change-utils.js`, `?? tests/e2e/hr-roster-published-change.spec.mjs`, `?? tests/unit/hr-roster-shift-change-utils.test.mjs`; ditambah berkas `FE-HRD-306` yang belum di-commit |
| Langkah berikutnya | `FE-HRD-308` (layar Riwayat Revisi) — memakai `fetchRosterRevisions` dari `FE-HRD-306`; perubahan dari task ini akan tampil di sana |
