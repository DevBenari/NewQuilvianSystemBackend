# Laporan Perubahan Frontend — `FE-MCU-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-014` |
| Judul | Ganti dokter MCU |
| Slice | `MVP-3` (pasangan `BE-MCU-032`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-014`* |
| Trace | `FR-MCU-045`; `MCU-DEC-040`; brief layar `05-screen-brief.md` §5.5 (modal *Ganti Dokter MCU*) |
| Contract version | `MCU-API-v1` `PATCH orders/{id}/mcu-doctor` (`ReassignMcuDoctorRequest`: `doctorId`, `reason`, `version`) — `approved` 2026-10-21. Kalimat `VAL-MCU-40`, `45`, `46` dibaca dari `McuCheckInService.ReassignDoctorAsync` |
| Wewenang UI | Brief layar `MCU-FE-002` (approved): menu ⋮ header *Ganti Dokter MCU* → modal dokter pengganti + alasan. Komponen dasar yang ada (`ConfirmModal`, `BaseSelectField`, `BaseTextAreaField`) |
| Dependency | `FE-MCU-011` ✅, `FE-MCU-006` ✅ (thunk `reassignMcuDoctor`); backend `BE-MCU-032` ✅ |
| Klasifikasi | `MEDIUM` — satu repository, berkas diubah sedikit, logika rendah–sedang, kontrak yang ada, tanpa database |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: hook, view, aturan, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `1b7ff563` (branch `yoga`) |
| Tanggal | 2026-10-28 |
| Status | Selesai — kedua kriteria terbukti di E2E API-tiruan; payload layar diterima backend lokal (ditolak basi tanpa menulis) |

Dikerjakan bersama `FE-MCU-012` ([laporan](FE-MCU-012.md)). Aturan murni dan konstanta ikut di berkas resume
(`mcu-resume-utils.js`, `mcu-resume-constants.js`).

---

## 1. Keadaan yang ditemukan di awal

- Thunk `reassignMcuDoctor` sudah ada sejak `FE-MCU-006`. Ia membawa `version`, memuat ulang detail saat `409`, dan
  mengganti detail di slice sesudah berhasil.
- Aturan header `FE-MCU-001` menampilkan *Ganti Dokter MCU* di menu ⋮ pada *Hadir*, *Dalam Pemeriksaan*, *Menunggu
  Resume*, dan *Disetujui Dokter* bagi pemegang `McuOrder : ReassignDoctor`. Sampai task ini, butir itu tampil
  nonaktif *Fitur belum tersedia*.
- Urutan pemeriksaan server: alasan wajib (`VAL-MCU-40`) → versi (`VAL-MCU-46`) → order *Diserahkan*/*Dibatalkan*
  ditolak (`VAL-MCU-45`) → dokter sama → dokter aktif.
- Pilihan dokter memakai `/doctors/admin/options`, sumber yang sama dengan dokter jaga Jadwal & Kuota dan check-in
  (keputusan pemilik 2026-10-27).

---

## 2. Proses bisnis dari sisi pengguna

1. Kepala unit membuka detail order, lalu memilih menu ⋮ → *Ganti Dokter MCU*. Petugas tanpa izin tidak melihat butir
   ini.
2. Modal menampilkan nomor order, peserta, dan *Dokter sekarang: dr. Rina*. Isiannya:
   - *Dokter Pengganti*\*: dicari di server.
   - *Alasan Penggantian*\*: mis. *Dokter selesai jaga*.
   - Keterangan akibat: dokter pengganti menjadi penyusun dan pemberi persetujuan resume, serta penanggung jawab
     pesanan Lab/Rad paket yang masih terbuka.
3. Isian kosong ditolak layar dengan *"Pilih dokter pengganti."* dan *"Alasan wajib diisi."*. Kalimat kedua sama dengan
   jawaban server; memilih dokter yang sama ditolak dengan kalimat server *"Dokter pengganti sama dengan dokter MCU
   sekarang."*
4. Penolakan server tampil apa adanya di modal, mis. *"Dokter MCU tidak dapat diganti pada order yang sudah selesai."*
   (`VAL-MCU-45`).
5. Bila server menjawab `409` *diubah petugas lain*, detail dimuat ulang dan modal tetap terbuka. Kiriman berikutnya
   memakai versi baru.
6. Sesudah berhasil:
   - Header menampilkan dokter baru, dan muncul toast *Dokter MCU Diganti*.
   - Tab Riwayat dimuat ulang dan memuat penggantian beralasan.
   - Hak dokter yang ditetapkan (tab Pemeriksaan dan Resume) langsung mengikuti dokter baru, karena dihitung dari
     detail terbaru.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `src/utils/.../mcu-patients/mcu-resume-utils.js` (baru, bersama `FE-MCU-012`) | `validateMcuReassignDoctorForm`, `buildMcuReassignDoctorPayload`, `validateMcuReasonForm` |
| `src/lib/constants/.../mcu-patients/mcu-resume-constants.js` (baru, bersama) | `MCU_REASSIGN_DOCTOR_CONFIG` (keterangan akibat, pesan cadangan) |
| `src/lib/hooks/.../mcu-patients/use-mcu-reassign-doctor.jsx` (baru) | Modal: target order, formulir, pilihan dokter (`useSelectResource` aktif hanya saat modal terbuka), kirim dengan `version` terbaru, toast, `onSuccess` |
| `src/lib/hooks/.../mcu-patients/use-mcu-order-detail.jsx` | Handler header `reassignDoctor`; sesudah berhasil riwayat dimuat ulang |
| `src/components/view/.../mcu-patients/detail/mcu-reassign-doctor-modal.jsx` (baru) | Modal *Ganti Dokter MCU* |
| `src/components/view/.../mcu-patients/detail/mcu-order-detail-view.jsx` | Modal dipasang |
| `tests/unit/mcu-resume-screen-rules.test.mjs` (bersama) | 2 uji: tombol per izin + status, validasi + body |
| `tests/e2e/mcu-resume-screen.spec.mjs` (bersama) | 2 skenario |
| `tests/e2e/mcu-patient-list-screen.spec.mjs` | Harapan menu ⋮ detail diperbarui: *Ganti Dokter MCU* kini tersedia (perubahan disengaja) |

Alur `view → hook → slice → service` dengan thunk yang sudah ada. Tidak ada dependency, pola state, atau komponen dasar
baru.

---

## 4. State yang ditangani

| State | Perilaku |
| --- | --- |
| Memuat | Pilihan dokter *"Memuat dokter..."*, muat lagi per halaman; tombol *Menyimpan...* |
| Kosong | *"Dokter tidak ditemukan."* atau pesan galat sumber pilihan |
| Gagal / ditolak | Pesan server di modal; isian tetap |
| Kiriman ganda | Tombol modal nonaktif selama permintaan |
| Tanpa hak akses | Butir menu tidak tampil tanpa `McuOrder : ReassignDoctor`; `403` server tampil apa adanya |
| Data basi | `409` memuat ulang detail; kiriman ulang memakai versi baru (E2E) |
| Privasi | Alasan penggantian hanya di modal dan tab Riwayat berizin; tidak masuk toast |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit (2 dari 9 di `mcu-resume-screen-rules.test.mjs`) | Lulus. Kepala unit melihat butir pada status 3–6; petugas tidak; status 1, 2, 8, 9 tidak; header `available: true`. Validasi: kosong (dua kalimat), dokter sama (tak peka huruf), body `{ doctorId, reason, version }` | `PASS` | `# pass 9` |
| Kontrol negatif | Pemeriksaan dokter sama dimatikan → uji 9 gagal; berkas dipulihkan → 9/9 | `PASS` | Keluaran perintah |
| **Kriteria 1** E2E — tombol hanya bagi pemegang izin | Petugas (`McuOrder` Read/Update/Cancel/CheckIn): menu ⋮ berisi *Batal Penuh* tanpa *Ganti Dokter MCU*. Sesudah izin diganti `ReassignDoctor` + muat ulang, butir tampil | `PASS` (tiruan) | Spec skenario 6 |
| **Kriteria 2** E2E — alasan kosong → pesan server | *Ganti Dokter* kosong → *"Pilih dokter pengganti."* + *"Alasan wajib diisi."*, nol tulis | `PASS` (tiruan) | Spec skenario 5 |
| Basi + berhasil E2E | (1) Pilih dr. Hadi + alasan → `409` `VAL-MCU-46` → pesan di modal, detail dimuat ulang. (2) Kirim ulang → `PATCH` berturut-turut `{ doctorId, reason, version: 5 }` lalu `version: 7`. (3) Header *dr. Hadi*, toast *Dokter MCU Diganti*, riwayat dimuat ulang dan memuat *Dokter selesai jaga* | `PASS` (tiruan) | Spec skenario 5 |
| `VAL-MCU-45` E2E | Server menolak *"Dokter MCU tidak dapat diganti pada order yang sudah selesai."* → tampil apa adanya; dokter tetap dr. Rina | `PASS` (tiruan) | Spec skenario 6 |
| Backend lokal + devYoga (tanpa tulis) | `MCU-RSMMC-000004` (*Hadir*, dr. Bima, versi 1). (1) API alasan kosong → `400` *"Alasan wajib diisi."* (2) Layar: menu ⋮ `["Batal Penuh","Ganti Dokter MCU"]`; modal *Dokter sekarang: dr. Bima Prasetya, Sp.PK*; kirim kosong → dua kalimat; 16 dokter dari `/doctors/admin/options`. (3) Kirim dr. Aditya + alasan; skrip hanya menurunkan `version` → server `409` `VAL-MCU-46` (bukan galat binding), jadi bentuk payload layar diterima. Pesan tampil di modal. (4) Sesudahnya order tetap versi 1, dr. Bima, riwayat 2 | `PASS` | Keluaran `uat-012.mjs`, `uat-012-ganti-dokter-basi.png` (scratchpad sesi `cbe51345`) |
| Regresi E2E MCU, lint, unit penuh, build | Sama dengan `FE-MCU-012` §6 | `PASS` / `EXISTING ISSUE` | [laporan `FE-MCU-012`](FE-MCU-012.md) |

`MANUAL TEST`: modal diuji lewat Playwright pada build standalone (tiruan). Di layar sungguhan terhadap backend lokal,
yang diuji adalah daftar dokter, pilihan, validasi, dan pesan server. Penggantian yang benar-benar tersimpan **tidak**
dijalankan karena sesi ini tidak menulis data uji. Pemindahan pesanan Lab/Rad terbuka sudah dibuktikan sisi server di
`BE-MCU-032`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tombol hanya tampil bagi pemegang `McuOrder : ReassignDoctor` | Terpenuhi (unit + E2E + layar sungguhan superadmin) | §5 |
| Alasan kosong → pesan server | Terpenuhi: kalimat layar = kalimat server *"Alasan wajib diisi."*, dibuktikan `400` backend lokal | §5 |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Batas status | Brief 5.5 menampilkan *Ganti Dokter MCU* sampai *Disetujui Dokter*, sedangkan server masih menerima *Terverifikasi*. Layar mengikuti brief; perluasan ke *Terverifikasi* butuh revisi brief |
| Daftar dokter | Semua dokter aktif dari `/doctors/admin/options`, tidak disaring ke dokter MCU. Dokter sekarang ikut tampil, lalu ditolak layar dengan kalimat server |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE` |
| Status Git (frontend) | `??` berkas baru di §3; `mcu-patient-list-screen.spec.mjs` (belum ter-track) diubah satu baris harapan |
