# Laporan Perubahan Frontend — `FE-MCU-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-012` |
| Judul | Layar resume dokter |
| Slice | `MVP-3` (pasangan `BE-MCU-030`, `031`) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-012`* |
| Trace | `FR-MCU-040`, `041`, `044`; `UAT-14`, `UAT-16`, `UAT-17`; brief layar `05-screen-brief.md` §5.5, §5.7 |
| Contract version | `MCU-API-v1` grup `Mcu Resume` (`GET /`, `PUT /current`, `POST /current/approve`, `POST /reopen`) — `approved` 2026-10-21. Kalimat `VAL-MCU-35`..`38`, `40`, `46` dibaca dari `McuResumeService` |
| Wewenang UI | Brief layar `MCU-FE-002` (approved): tab Resume, modal *Setujui* dan *Buka Ulang*, *Riwayat versi* bertanda *Dikoreksi*. Komponen dasar yang ada (`BaseSelectField`, `BaseTextAreaField`, `ConfirmModal`, `StatusBadge`) dalam rentang `DEV_DISCRETION` §7 |
| Dependency | `FE-MCU-011` ✅, `FE-MCU-010` ✅ (lapisan data); backend `BE-MCU-030` ✅, `031` ✅ |
| Klasifikasi | `HIGH` — satu repository, berkas diperiksa > 20, logika sedang–tinggi (hak menurut status order + status resume + izin + dokter yang ditetapkan, versi, data basi), kontrak yang ada, tanpa database, keselamatan klinis (`VAL-MCU-38`), UI baru |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: konstanta, aturan, hook, view, uji. Backend: laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f7f60e34c` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `1b7ff563` (branch `yoga`) |
| Tanggal | 2026-10-28 |
| Status | Selesai — `UAT-14`, `16`, `17` terbukti di E2E API-tiruan; sisi server `VAL-MCU-38` dibuktikan terhadap backend lokal tanpa tulis. `UAT-14`..`17` dengan data sungguhan menjadi batas verifikasi |

Dikerjakan bersama `FE-MCU-014` dalam satu sesi; keduanya berbagi berkas konstanta/aturan dan spec E2E
([laporan `FE-MCU-014`](FE-MCU-014.md)).

---

## 1. Keadaan yang ditemukan di awal

- Lapisan data resume sudah ada sejak `FE-MCU-010`: thunk `fetchMcuResumes`, `saveMcuResume`, `approveMcuResume`,
  `returnMcuResume`, `verifyMcuResume`, dan `reopenMcuResume`.
  - Setiap aksi membawa `version` order.
  - `409` `VAL-MCU-46` memuat ulang detail order dan seluruh versi resume.
  - Sesudah berhasil, detail order ikut dimuat ulang. Khusus buka ulang, seluruh versi juga dimuat ulang.
- Tab Resume di detail order masih menampilkan *Fitur belum tersedia*.
- Aturan header `FE-MCU-001` sudah memetakan *Buka Resume* (*Menunggu Resume*) dan *Buka Ulang* (dokter yang ditetapkan).
  Handler *Buka Ulang* belum dipasang.
- Aturan server (`McuResumeService`) yang menentukan tombol:

  | Aksi | Status order | Status resume berlaku | Syarat lain |
  | --- | --- | --- | --- |
  | Simpan draf | *Dalam Pemeriksaan*, *Menunggu Resume* | Belum ada atau *Draf* | Dokter yang ditetapkan (`VAL-MCU-38`) |
  | Setujui | sama | *Draf* | Dokter yang ditetapkan. Server memeriksa komponen tuntas (`VAL-MCU-35`), skrining (`VAL-MCU-36`), lalu isi minimum (`VAL-MCU-37`) |
  | Buka ulang | *Disetujui Dokter*, *Terverifikasi*, *Diserahkan* | *Disetujui*, *Terverifikasi* | Dokter yang ditetapkan; alasan wajib (`VAL-MCU-40`) |

  `VAL-MCU-37` menyebut ruas pertama yang kosong, berurutan: kategori kelayakan, kondisi umum, rekomendasi. Approve
  memeriksa resume **tersimpan**, bukan isi formulir.
- Batas panjang: kondisi umum, temuan, dan rekomendasi masing-masing 4.000 karakter; rekomendasi kerja 1.000; alasan
  500.
- Daftar kategori kelayakan (`GET fitness-categories?isActive=true`) tidak berhalaman dan dijaga
  `McuReferenceData : Read`.

---

## 2. Proses bisnis dari sisi pengguna

1. **Sebelum pemeriksaan berjalan** — tab Resume pada order *Terjadwal*, *Tidak Hadir*, atau *Hadir* menampilkan
   keterangan brief *"Resume dapat disusun sesudah semua komponen tuntas."*
2. **Dokter yang ditetapkan menyusun draf.** Order harus *Dalam Pemeriksaan* atau *Menunggu Resume*.
   - Isian: Kategori kelayakan\*, Kondisi umum\*, Temuan penting, Rekomendasi medis\*, Rekomendasi kerja.
   - Rekomendasi kerja diberi keterangan *"Hanya teks ini yang sampai ke perusahaan tanpa persetujuan peserta."*
   - Pada *Dalam Pemeriksaan* tampil catatan bahwa draf boleh disimpan, tetapi resume baru dapat disetujui sesudah
     semua komponen tuntas.
   - *Simpan Draf* tidak menuntut isian wajib.
3. **Setujui.**
   - Layar memeriksa isi minimum dengan kalimat server lebih dulu, mis. *"Lengkapi kategori kelayakan sebelum
     menyetujui."*. Modal tidak terbuka bila belum lengkap.
   - Modal konfirmasi menyebut versi dan kategori, serta mengingatkan bahwa resume dan hasil Pemeriksaan MCU akan
     terkunci.
   - Bila formulir berubah sejak tersimpan, *Setujui* menyimpan draf dulu, lalu menyetujui dengan `Version` order dari
     jawaban simpan.
   - Penolakan server tampil apa adanya di modal, mis. *"Masih ada 1 komponen yang belum tuntas: Thorax PA."*
     (`UAT-14`).
4. **Sesudah disetujui**, seluruh isian baca-saja dengan keterangan *"Resume sudah disetujui dan terkunci…"*. Jejak
   *Disetujui dr. Rina · waktu* dan *Diverifikasi …* tampil di bawah formulir.
5. **Pengguna lain** — petugas, atau dokter yang bukan dokter yang ditetapkan:
   - Melihat resume baca-saja. Pada order yang masih terbuka tampil *"Hanya dokter MCU yang ditetapkan…"*.
   - Tidak melihat *Simpan Draf*, *Setujui*, maupun *Buka Ulang* (`UAT-16`).
6. **Buka Ulang** — dari menu ⋮ header atau tombol di tab.
   - Modal alasan wajib, berisi akibatnya: versi lama *Dikoreksi* tetap terbaca, versi baru wajib disetujui dan
     diverifikasi ulang, dan penerima hasil lama ditandai perlu diberi tahu.
   - Sesudah berhasil, tab Resume terbuka pada *Versi 2 · Draf* yang dapat diisi lagi.
7. **Riwayat versi** — pilihan *v2 · Draf (berlaku)* / *v1 · Dikoreksi*.
   - Versi lama tampil baca-saja dengan peringatan *Versi 1 dikoreksi*, alasan koreksinya, dan jejak
     disetujui/diverifikasi/dikoreksi (`UAT-17`).
8. **Dikembalikan petugas** — bila petugas mengembalikan resume (`FE-MCU-013`), alasan pengembalian tampil sebagai
   peringatan di atas draf.
9. **Data basi.** `409` *diubah petugas lain* memuat ulang detail dan seluruh versi, lalu pesan server tampil di bawah
   formulir.
   - Ketikan dokter **tidak** hilang bila isi resume di server tidak berubah.
   - Kiriman berikutnya memakai versi baru.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../mcu-patients/mcu-resume-constants.js` (baru) | `MCU_RESUME_LIMITS` (4.000 / 1.000 / 500) dan `MCU_RESUME_CONFIG` (teks keterangan, catatan modal, pesan cadangan). Ikut memuat `MCU_REASSIGN_DOCTOR_CONFIG` untuk `FE-MCU-014` |
| `src/utils/.../mcu-patients/mcu-resume-utils.js` (baru) | `resolveMcuResumeAccess`, `describeMcuResumeState`, `pickMcuCurrentResume`, `sortMcuResumeVersions`, `describeMcuResumeVersion`, `getMcuResumeStatusBadge`, formulir (`createMcuResumeForm`, `isMcuResumeFormDirty`, `validateMcuResumeForm`, `validateMcuResumeApproval`, `buildMcuResumePayload`), `toMcuFitnessCategoryOptions`, `validateMcuReasonForm`, `buildMcuReopenResumePayload`. Ikut memuat aturan ganti dokter `FE-MCU-014` |
| `src/lib/hooks/.../mcu-patients/use-mcu-resume.jsx` (baru) | Memuat versi saat tab aktif, kategori kelayakan sekali saat formulir dapat diisi, formulir tersalin dari versi tersimpan, *Simpan Draf*, *Setujui* (simpan-lalu-setujui bila berubah), *Buka Ulang*, pemilih versi |
| `src/lib/hooks/.../mcu-patients/use-mcu-order-detail.jsx` | Memasang `useMcuResume` dan handler header `reopenResume`; sesudah buka ulang pindah ke tab Resume |
| `src/components/view/.../mcu-patients/detail/mcu-resume-tab.jsx` (baru) | Tab Resume |
| `src/components/view/.../mcu-patients/detail/mcu-resume-modals.jsx` (baru) | Modal *Setujui* dan *Buka Ulang* |
| `src/components/view/.../mcu-patients/detail/mcu-order-detail-view.jsx` | Tab Resume dan modalnya dipasang menggantikan *Fitur belum tersedia* |
| `tests/unit/mcu-resume-screen-rules.test.mjs` (baru) | 9 uji aturan: 7 untuk resume, 2 untuk ganti dokter |
| `tests/e2e/mcu-resume-screen.spec.mjs` (baru) | 6 skenario API-tiruan: 4 untuk resume, 2 untuk ganti dokter |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice → service`.
  - Thunk resume dari `FE-MCU-010`, tanpa thunk baru.
  - Satu-satunya panggilan service langsung adalah pilihan kategori kelayakan. Polanya sama dengan pilihan jenis
    Pemeriksaan MCU di `FE-MCU-011`.
- Aturan murni terpisah dari hook dan diuji unit. Aksi header tetap lewat `resolveMcuOrderActions` (`FE-MCU-001`).
- Formulir tersalin dari versi tersimpan memakai pola "state dari render sebelumnya" tanpa efek.
  - Kuncinya: id, status, dan isi versi berlaku. Muat ulang yang isinya sama tidak menimpa ketikan.
  - Pemuat kategori tidak memanggil `setState` sinkron di dalam efek, sehingga lint React Compiler lulus.
- Tidak ada dependency, pola state, atau komponen dasar baru.

---

## 4. State yang ditangani

| State | Perilaku |
| --- | --- |
| Memuat | *"Mengambil resume MCU..."*; pilihan kategori ber-`loading`; tombol *Simpan Draf* ber-`loading` |
| Kosong | Order berjalan tanpa resume: formulir kosong berlencana *Belum disimpan* bagi dokter yang ditetapkan, atau keterangan bagi pengguna lain. Order dibatalkan tanpa resume: *"Order dibatalkan; resume tidak disusun."* |
| Gagal | Muat versi: pesan + *Coba lagi*. Kategori: peringatan, dan kategori tersimpan tetap tampil sebagai pilihan. Aksi: pesan server di formulir atau modal |
| Kiriman ganda | Tombol formulir nonaktif dan tombol modal *Memproses…* selama permintaan |
| Validasi | Kalimat server sebelum kirim: `VAL-MCU-37` (urutan server), batas panjang, `VAL-MCU-40` |
| Tanpa hak akses | Tombol disembunyikan menurut izin + dokter yang ditetapkan; `403` server tampil apa adanya |
| Data basi | `409` memuat ulang detail + versi; pesan tampil; ketikan dipertahankan bila isi server sama; kiriman ulang memakai versi baru (E2E) |
| Privasi | Resume hanya di detail order berizin; tidak ada isi resume di URL, toast, atau log |
| Aksesibilitas | Label setiap isian (wajib bertanda `*`), formulir ber-`aria-label`, jejak resume ber-`aria-label`, pesan galat di `InformationAlert` |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Dipakai untuk |
| --- | --- | --- |
| `GET` | `orders/{id}/resumes` | Seluruh versi (terbaru lebih dulu) |
| `PUT` | `orders/{id}/resumes/current` | Simpan draf — `SaveMcuResumeRequest` |
| `POST` | `orders/{id}/resumes/current/approve` | Setujui — `{ version }` |
| `POST` | `orders/{id}/resumes/reopen` | Buka ulang — `{ reason, version }` |
| `GET` | `fitness-categories?isActive=true` | Pilihan kategori kelayakan |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tests/unit/mcu-resume-screen-rules.test.mjs` | 9/9 lulus. Cakupan: hak edit/approve per status, dokter yang ditetapkan + izin, Buka Ulang (`UAT-16`/`17`) dan header, keterangan tab, urutan + label versi, formulir (salinan, perubahan, batas, `VAL-MCU-37`), body DTO, pilihan kategori | `PASS` | `# pass 9` |
| Kontrol negatif uji unit | Buka Ulang tanpa syarat dokter yang ditetapkan → uji 2 gagal; berkas dipulihkan (`cmp` sama) → 9/9 | `PASS` | Keluaran perintah |
| **`UAT-14`** E2E | (1) *Setujui* kosong → *"Lengkapi kategori kelayakan sebelum menyetujui."*, modal tidak terbuka, nol tulis. (2) Isi → `PUT current` persis DTO (`importantFindings: null`, `version: 5`), lalu *Versi 1 · Draf* dan detail dimuat ulang. (3) *Setujui* → satu `POST approve` `{ version: 6 }` tanpa `PUT` ulang; pesan *"Masih ada 1 komponen yang belum tuntas: Thorax PA."* tampil apa adanya; status tetap *Menunggu Resume* | `PASS` (tiruan) | Spec skenario 1 |
| Basi + simpan-lalu-setujui E2E | (1) `409` `VAL-MCU-46` → pesan; `GET resumes` dimuat ulang; ketikan *"Tekanan darah 150/95"* tetap. (2) Kirim ulang → `version: 8`. (3) Ubah rekomendasi lalu *Setujui* → `PUT` `version: 9`, lalu `POST approve` `{ version: 10 }` dari jawaban simpan. (4) Hasil: *Disetujui Dokter*, terkunci, *Disetujui dr. Rina*, tombol *Buka Ulang* tampil | `PASS` (tiruan) | Spec skenario 2 |
| **`UAT-16`** E2E | dr. Hadi (bukan dokter yang ditetapkan, izin resume lengkap) atas resume *Disetujui Dokter*. Hasil: keterangan terkunci, empat isian nonaktif, tanpa *Buka Ulang*/*Setujui*/*Simpan Draf*, tanpa menu ⋮, nol tulis | `PASS` (tiruan) | Spec skenario 3 |
| **`UAT-17`** E2E | Resume *Diserahkan*. (1) ⋮ *Buka Ulang* alasan kosong → *"Alasan wajib diisi."*, nol tulis. (2) Dengan alasan → `POST reopen` `{ reason, version: 5 }`. (3) Tab Resume *Versi 2 · Draf* dapat diisi; status *Menunggu Resume*. (4) Pilih *v1 · Dikoreksi* → baca-saja, alasan koreksi, *Diverifikasi Dewi*, *Dikoreksi dr. Rina*, isian lama | `PASS` (tiruan) | Spec skenario 4 |
| UAT backend lokal + devYoga (tanpa tulis) | `MCU-RSMMC-000004` (*Hadir*, versi 1, dr. Bima). (1) `GET resumes` → `200 []`. (2) Superadmin tanpa `doctorId`: `PUT current`, `POST approve`, `POST reopen` → ketiganya `403` *"Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini."* (`VAL-MCU-38`, sisi server `UAT-16`). (3) Layar tab Resume → *"Resume dapat disusun sesudah semua komponen tuntas."* (4) Sesudahnya order tetap versi 1, riwayat 2, resume 0 | `PASS` | Keluaran `uat-012.mjs` (scratchpad sesi `cbe51345`) |
| Seluruh spec E2E MCU (8 berkas, 59 skenario) | 58 lulus, 1 gagal: `mcu-check-in` *UAT-07 (server)* — klik *Check-in* di dasar halaman tidak terkirim (berkas check-in tidak diubah; tercatat flaky di `FE-MCU-011`). Ulang `mcu-check-in` + `mcu-patient-list`: 13/13 lulus | `PASS` / `FLAKY EXISTING` | Keluaran Playwright |
| `npx eslint` (berkas layar pasien MCU + dua uji baru) | Exit 0 | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.923 uji: 3.881 lulus, 42 gagal; daftar nama **identik** baseline (`diff` kosong), semuanya di luar MCU | `EXISTING ISSUE` | Keluaran + `diff` |
| `npm run build` (dua kali, terakhir sesudah perbaikan kunci salinan formulir) | Exit 0 | `PASS` | Keluaran build |

`MANUAL TEST`: kontrol interaktif diuji dengan Playwright pada build standalone. Cakupannya: pilihan kategori, empat
isian teks, *Simpan Draf*, *Setujui* + modal, *Buka Ulang* + modal alasan, pemilih *Riwayat versi*, menu ⋮ header,
serta keadaan baca-saja/terkunci. Layar sungguhan terhadap backend lokal juga dibuka untuk tab Resume order *Hadir*.

`UAT-14`, `16`, `17` dengan data sungguhan **tidak dijalankan**. Alasannya: ketiganya menuntut order *Menunggu
Resume*/*Diserahkan* di devYoga, sedangkan yang ada hanya order *Hadir* yang belum lunas. Untuk mencapainya dibutuhkan
pembayaran, pesanan Lab/Rad selesai, dan skrining. Ketiganya juga menuntut akun dokter yang ditetapkan (dr. Bima) dan
dokter kedua untuk `UAT-16`. Sesi ini tidak menulis data uji.

`AUTOMATED TEST: npm run test:unit — FAIL (42 kegagalan existing di luar MCU, identik baseline; uji MCU PASS)`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-14` | Terpenuhi di E2E (pesan menyebut Thorax PA apa adanya) | §6 |
| `UAT-16` | Terpenuhi: layar menyembunyikan tombol (E2E), server menolak `403` `VAL-MCU-38` (backend lokal) | §6 |
| `UAT-17` | Terpenuhi di E2E: v1 *Dikoreksi* terbaca, v2 *Draf*. Penanda penyerahan lama milik tab Penyerahan (`FE-MCU-013`); sisi server sudah diuji `BE-MCU-031`/`034` | §6 |
| DoD: kriteria terbukti | Terpenuhi dengan batas verifikasi di §6 | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Pembagian dengan `FE-MCU-013` | *Verifikasi* dan *Kembalikan ke Dokter* tetap tampil nonaktif *Fitur belum tersedia* di header. Brief 5.7 meletakkan keduanya di tab Resume; pemasangannya milik `FE-MCU-013` |
| Keputusan pengembang | *Buka Ulang* tampil di menu ⋮ header (brief 5.5) **dan** sebagai tombol di tab Resume. Bentuk dan route tidak berubah (§7) |
| Batas verifikasi | `UAT-14`/`16`/`17` dengan data sungguhan. Untuk menutupnya dibutuhkan: order MCU yang mencapai *Menunggu Resume* di devYoga (bayar, Lab/Rad selesai, skrining), izin tulis devYoga, sandi dr. Bima, dan satu akun dokter MCU kedua |
| Risiko izin | Pilihan kategori kelayakan menuntut `McuReferenceData : Read`. Tanpa izin itu, peringatan tampil dan dokter tidak dapat memilih kategori. Matriks izin peran dokter MCU perlu dicek saat go-live (sama dengan catatan `FE-MCU-011`) |
| Peringatan | `isApprovingDoctor` (`VAL-MCU-39`) belum diisi karena respons resume hanya memuat nama, bukan id. Pengisiannya urusan `FE-MCU-013` (Verifikasi) |
| Dependency backend | Tidak ada |
| Perubahan sampingan | `NONE`. Output Playwright ke scratchpad (`--output`), sehingga `test-results/` ter-track tidak tersentuh. Server BE 7184 dan FE 3710 dimatikan, port bebas |
| Status Git (frontend) | `??` berkas baru di §3.1 (di dalam folder `mcu-patients/` yang belum ter-track sejak `FE-MCU-007`) serta kedua berkas uji baru |
| Langkah berikutnya | `FE-MCU-013` (Verifikasi, cetak, persetujuan, penyerahan) — kini siap |
