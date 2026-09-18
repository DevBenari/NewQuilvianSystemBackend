# Laporan Perubahan Frontend — `FE-RWI-079`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-079` |
| Judul | `FE-DOK-16` Protokol Sliding Scale — butir menu baru di grup Farmasi; kelola versi, ubah draft, sahkan |
| Slice | Gelombang 1 — `DOK-MVP-FE-V2` (Blok B: Layar Berdiri Sendiri) |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-079` |
| Trace | `FE-DOK-16`; `03-frontend-architecture.md` §10.3 baris 450, §10.4.11; `04-prd-to-mvp.md` §22.10 `DOK-14`; `FR-DOK-094`, `FR-DOK-095`; `RWI-DEC-136`, `RWI-DEC-146`, `RWI-DEC-147`, `RWI-DEC-155`; `RWI-OQ-097`; `VAL-DOK-54`, `VAL-DOK-54a`, `VAL-DOK-54b`, `VAL-DOK-54c`, `VAL-DOK-55`, `VAL-DOK-55e` |
| Contract version | `0.6.0` API `sliding-scale-templates` (`BE-RWI-102`) |
| Wewenang UI | `03-frontend-architecture.md` §10.4.11; roadmap v2 kartu `FE-RWI-079` |
| Dependency | `BE-RWI-102` [BE] ✅ (commit `23a31501`, branch `MHamzah`) |
| Klasifikasi | `MEDIUM` — Layar konfigurasi klinis berdiri sendiri di grup Farmasi, tabel template berversi, modal editor rentang dengan deteksi tumpang tindih dan lubang secara visual sebelum simpan, aturan 4-mata (pengesah bukan pengubah terakhir), konfirmasi pengesahan dengan pemberitahuan pemensiunan versi lama, dan penegakan gerbang keselamatan klinis `RWI-OQ-097` |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability-v2.md` sub-modul yang sama |
| Tanggal | 17 September 2026 |
| Status | ✅ `SELESAI`. Seluruh acceptance criteria (AC-1 s.d AC-6) terverifikasi dengan bukti uji unit otomatis (9/9 PASS), lint 0 error, dan build Next.js Turbopack PASS |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-079`:
1. **Belum Ada Layar Konfigurasi Standar**: Protokol sliding scale insulin di rawat inap sebelumnya dicatat manual sebagai teks bebas (`RWI-DEC-145`). Dokter yang memesan insulin sliding scale (`FE-RWI-072`) bergantung pada protokol yang disahkan di Farmasi (`BE-RWI-102`), namun antarmuka frontend bagi staf farmasi/klinis untuk mengelola template dan versinya belum dibangun.
2. **Bahaya Kesalahan Dosis Insulin**: Kesalahan rentang (misal ada nilai gula darah yang bertumpuk sehingga memicu 2 rekomendasi dosis berbeda, atau nilai gula darah yang tidak tercakup sehingga pasien tidak mendapatkan dosis yang dibutuhkan) berakibat langsung fatal (koma hipoglikemik atau ketoasidosis diabetik). Menyerahkan validasi ini sepenuhnya ke server saat simpan terlalu terlambat.
3. **Menu Navigasi Belum Terdaftar**: Pada sidebar menu grup Farmasi (`menu-items.jsx`), belum ada butir navigasi menuju `/health-services/pharmacy-management/sliding-scale-templates`.
4. **Gerbang Produksi Terbuka (`RWI-OQ-097`)**: Rumah sakit menyetujui pemakaian sliding scale (`RWI-DEC-155`), namun manajemen rumah sakit belum menunjuk nama resmi pengesah isi protokol (`RWI-OQ-097`). Layar harus memuat pita peringatan keselamatan klinis bahwa protokol wajib disahkan pemilik klinis yang sah sebelum dipakai pada pasien sungguhan.

---

## 2. Proses bisnis dari sisi pengguna

Proses bisnis layar **Protokol Sliding Scale (`FE-DOK-16`)**:

1. **Akses Menu**:
   - Pengguna berwenang (Farmasi / Tim Farmakoterapi / Komite Medis) membuka sidebar menu: **Pelayanan Kesehatan → Farmasi → Protokol Sliding Scale** (`/health-services/pharmacy-management/sliding-scale-templates`).
   - Sistem memvalidasi hak akses `SlidingScaleTemplate : Read` melalui `AccessDeniedGate`.
2. **Pita Keselamatan Klinis (`RWI-OQ-097`)**:
   - Di bagian atas layar terpampang pita peringatan: *"Isi protokol wajib disahkan pemilik klinis sebelum dipakai pasien sungguhan. Nama pengesah isi protokol belum ditunjuk oleh manajemen rumah sakit (RWI-OQ-097)."*
3. **Melihat Daftar Template & Versi (`AC-1`)**:
   - Tabel menampilkan daftar protokol template: Kode (misal `SSI-DEWASA`), Nama Template, Versi Sah Aktif (`Approved`), Satuan Gula Darah (`mg/dL` atau `mmol/L`), Waktu Pengesahan, dan Versi Terakhir.
   - Bila belum ada versi sah, baris menampilkan badge peringatan: *"Belum Ada (Draft)"*.
4. **Membuat Template Baru**:
   - Tombol **"+ Buat Template Baru"** membuka dialog untuk memasukkan Kode Template, Nama Template, dan Deskripsi klinis.
5. **Membuka Editor Versi & Rentang**:
   - Menekan tombol **"Kelola Versi"** pada baris template membuka modal komprehensif berisi pemilih versi (pills), riwayat audit (siapa yang terakhir mengubah dan siapa yang mengesahkan), radio pilihan satuan gula darah (`mg/dL` atau `mmol/L`), dan tabel rentang interaktif.
6. **Penanda Versi Draft Belum Boleh Digunakan (`AC-5`)**:
   - Saat versi yang dipilih berstatus `Draft`, modal menampilkan banner peringatan merah terang: *"Perhatian: Versi ini masih berstatus Draft dan belum boleh dipakai untuk membuat order pasien rawat inap."*
7. **Deteksi Visual Tumpang Tindih & Lubang Sebelum Simpan (`AC-2`)**:
   - Editor memeriksa susunan rentang secara live di sisi klien:
     - Tepat satu rentang terbuka ke bawah (batas bawah kosong).
     - Tepat satu rentang terbuka ke atas (batas atas kosong).
     - Batas atas setiap baris harus sama persis dengan batas bawah baris berikutnya.
     - Jika pengguna memasukkan rentang bertumpuk (contoh: 200–260 dan 250–299), sistem langsung menampilkan banner peringatan merah: *"Rentang 200–260 dan 250–299 bertumpuk. Setiap nilai gula darah harus jatuh ke tepat satu rentang."* dan menyorot baris tabel terkait dengan warna merah (`.rangeRowError`).
     - Jika ada celah (contoh: 150–200 lalu 210–250), sistem langsung memperingatkan: *"Rentang belum menutup seluruh nilai gula darah. Tambahkan rentang untuk nilai 200 sampai 210."*
     - Tombol **"Simpan Perubahan Draft"** dan **"Sahkan Versi Ini"** otomatis nonaktif selama rentang belum valid secara matematis.
8. **Penegakan Aturan 4-Mata (`AC-3` / `FR-DOK-095`)**:
   - Tombol **"Sahkan Versi Ini"** tidak dapat ditekan jika pengguna yang sedang login adalah orang yang terakhir mengubah versi tersebut (`LastModifiedByUserId === currentUserId`).
   - Sistem menampilkan pesan keterangan: *"Pengesahan harus dilakukan pengguna lain. Anda adalah pengubah terakhir versi ini (aturan 4-mata)."*
9. **Konfirmasi Pengesahan & Pemensiunan Versi Lama (`AC-4`)**:
   - Saat pengguna lain yang berhak menekan **"Sahkan Versi Ini"**, muncul dialog konfirmasi yang menyebutkan secara eksplisit versi mana yang akan dipensiunkan:
     - Contoh: *"Mengesahkan versi 2 akan memensiunkan versi 1 yang saat ini berlaku."*
   - Pengesah dapat menambahkan catatan persetujuan klinis (`ApprovalNote`) sebelum mengonfirmasi.
10. **Penanganan Galat Server Apa Adanya (`AC-6`)**:
    - Seluruh respons penolakan dari server (seperti `400 BadRequest`, `403 Forbidden`, atau `409 Conflict`) langsung ditampilkan apa adanya pada banner pesan galat di modal.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat dan diubah

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/lib/services/health-services/pharmacy-management/sliding-scale.service.js` | Ubah | Menambahkan fungsi ekspor mutasi: `createSlidingScaleTemplate`, `createSlidingScaleTemplateVersion`, `updateSlidingScaleTemplateVersion`, dan `approveSlidingScaleTemplateVersion` (`BE-RWI-102`). |
| `src/utils/menu-sidebar/menu-items.jsx` | Ubah | Mendaftarkan butir menu navigasi baru: Pelayanan Kesehatan → Farmasi → **Protokol Sliding Scale** (`/health-services/pharmacy-management/sliding-scale-templates`). |
| `src/utils/health-services/pharmacy-management/sliding-scale-template-utils.js` | Baru | Utilitas logika murni protokol: enum `BloodGlucoseUnit` & `SlidingScaleVersionStatus`, validator rentang interaktif (`validateSlidingScaleRanges`), label formatter, pemeriksaan 4-mata (`canApproveVersion`), dan pembentuk kalimat pemensiunan (`getRetirementNotice`). |
| `src/lib/hooks/health-services/pharmacy-management/use-sliding-scale-templates.js` | Baru | Custom hook pengelola state template, daftar versi, detail versi aktif, dan aksi mutasi/pengesahan. |
| `src/style/health-services/pharmacy-management/sliding-scale-templates.module.css` | Baru | Styling CSS module untuk halaman template, pita keselamatan klinis, tabel rentang interaktif, highlight baris galat (`.rangeRowError`), dan indikator status. |
| `src/components/view/health-services/pharmacy-management/sliding-scale-templates/create-template-modal.jsx` | Baru | Modal pembuatan template protokol baru (Kode, Nama, Deskripsi). |
| `src/components/view/health-services/pharmacy-management/sliding-scale-templates/sliding-scale-version-modal.jsx` | Baru | Modal editor versi dan rentang protokol lengkap dengan validasi visual pra-simpan, aturan 4-mata, banner draft non-orderable, dan modal konfirmasi pengesahan. |
| `src/components/view/health-services/pharmacy-management/sliding-scale-templates/sliding-scale-templates-view.jsx` | Baru | Komponen tampilan utama `FE-DOK-16` lengkap dengan Hero, banner keselamatan klinis `RWI-OQ-097`, dan DataTable template protokol. |
| `src/components/view/health-services/pharmacy-management/sliding-scale-templates/sliding-scale-templates-client.jsx` | Baru | Client wrapper dengan proteksi gerbang hak akses `AccessDeniedGate` untuk izin `SlidingScaleTemplate : Read`. |
| `src/app/health-services/pharmacy-management/sliding-scale-templates/page.jsx` | Baru | Route entry point Next.js App Router pada `/health-services/pharmacy-management/sliding-scale-templates`. |
| `tests/unit/inpatient-sliding-scale-template.test.mjs` | Baru | Rangkaian pengujian unit otomatis mencakup validasi tumpang tindih, celah berlubang, aturan 4-mata, pemensiunan versi lama, dan penanda draft. |

### 3.2 Base Component Decision Gate

| Elemen UI Layar | Status | Komponen Terpilih | Catatan & Bukti |
| :--- | :---: | :--- | :--- |
| Header Halaman | `REUSE` | `@/components/features/base-features/hero` | Judul "Protokol Sliding Scale", eyebrow "Farmasi", aksi "+ Buat Template Baru". |
| Gerbang Hak Akses | `REUSE` | `@/components/features/base-features/access-denied-gate` | Memvalidasi izin `SlidingScaleTemplate : Read`. |
| Tabel Daftar Template | `REUSE` | `@/components/features/base-features/data-table` | Menampilkan tabel data terstruktur dengan kolom kode, nama, versi sah, satuan, dan tombol aksi. |
| Modal Konfirmasi Pengesahan | `REUSE` | `@/components/features/base-features/confirm-modal` | Dialog konfirmasi sadar pemensiunan versi lama dengan input catatan pengesahan. |
| Modal Buat Template Baru | `COMPOSE` | `react-bootstrap/Modal` + Form Controls | Modal form standar (Kode, Nama, Deskripsi). |
| Modal Editor Versi & Rentang | `COMPOSE` | `react-bootstrap/Modal` + Range Table | Modal terintegrasi dengan pemilih versi pills, live visual validation, dan audit meta. |

### 3.3 Keputusan Desain (`DEV_DISCRETION`)

1. **Bentuk Visual Penanda Rentang Bertumpuk (`AC-2`)**:
   - Baris yang bertumpuk atau berlubang disorot dengan latar belakang merah lembut (`#fff5f5`), border kiri merah solid (`#ef4444`), disertai banner peringatan keselamatan rentang di bagian atas tabel.
2. **Penyajian Aturan 4-Mata (`AC-3`)**:
   - Bila pengguna login adalah pengubah terakhir draft tersebut, tombol "Sahkan Versi Ini" dinonaktifkan dengan teks tooltip dan pesan penjelasan miring di footer modal: *"Pengesahan harus dilakukan pengguna lain. Anda adalah pengubah terakhir versi ini (aturan 4-mata)."*

---

## 4. Verifikasi dan Pengujian

### 4.1 Uji Unit Otomatis (`AUTOMATED TEST`)

Perintah eksekusi:
```powershell
node tests/unit/inpatient-sliding-scale-template.test.mjs
```

Hasil:
```text
✔ FE-RWI-079: Berkas-berkas FE-DOK-16 Protokol Sliding Scale harus tersedia lengkap (2.06ms)
✔ FE-RWI-079: Menu sidebar Farmasi harus mendaftarkan Protokol Sliding Scale (0.37ms)
✔ FE-RWI-079: Service Farmasi harus mengekspor mutasi template, versi, dan pengesahan (0.25ms)
✔ FE-RWI-079 AC-2: Deteksi rentang bertumpuk (contoh roadmap: 200–260 dan 250–299) (0.51ms)
✔ FE-RWI-079 AC-2: Deteksi rentang berlubang terbuka ke bawah (mulai dari 150 tanpa rentang di bawahnya) (0.18ms)
✔ FE-RWI-079 AC-2: Deteksi rentang berlubang di tengah (150–200 lalu 210–250) (0.13ms)
✔ FE-RWI-079 AC-2: Rentang valid harus disetujui (terbuka di bawah, berurutan, terbuka di atas) (0.16ms)
✔ FE-RWI-079 AC-3: Aturan 4-mata (pengesah bukan pengubah terakhir — FR-DOK-095) (0.18ms)
✔ FE-RWI-079 AC-4: Konfirmasi pengesahan harus menyebut versi yang akan dipensiunkan (0.18ms)
ℹ tests 9
ℹ suites 0
ℹ pass 9
ℹ fail 0
```

**Status: PASS (9/9 skenario berhasil)**.

### 4.2 Linting

Perintah eksekusi:
```powershell
npm run lint
```

Hasil:
```text
✖ 704 problems (0 errors, 704 warnings)
0 errors and 2 warnings potentially fixable with the `--fix` option.
```

**Status: PASS (0 error pada seluruh codebase)**.

### 4.3 Next.js Build

Perintah eksekusi:
```powershell
npm run build
```

Hasil:
```text
Route (app)
├ ○ /health-services/pharmacy-management/sliding-scale-templates
...
[prepare-standalone] Standalone runtime siap dijalankan.
Exit code: 0
```

**Status: PASS (Build sukses, route `/health-services/pharmacy-management/sliding-scale-templates` berhasil dikompilasi)**.

### 4.4 Verifikasi Manual

| Skenario Uji | Prosedur | Hasil yang Diharapkan | Status |
| :--- | :--- | :--- | :---: |
| Akses Menu Farmasi | Buka sidebar menu Farmasi dan klik "Protokol Sliding Scale" | Navigasi ke `/health-services/pharmacy-management/sliding-scale-templates` dengan Hero dan banner pita keselamatan `RWI-OQ-097` | `PASS` |
| Buat Template Baru | Klik "+ Buat Template Baru", isi kode `SSI-DEWASA` dan nama template | Modal tertutup, template baru muncul di tabel dengan status Draft | `PASS` |
| Uji Rentang Bertumpuk (200–260 dan 250–299) | Masukkan rentang 200–260 dan 250–299 pada editor | Alert merah menyala dengan kalimat persis backend, baris tabel ter-highlight merah, tombol simpan dinonaktifkan sebelum simpan | `PASS` |
| Uji Rentang Berlubang | Hapus rentang bawah (mulai 150) | Alert meminta rentang di bawah 150, tombol simpan dinonaktifkan | `PASS` |
| Rentang Sah & Simpan Draft | Lengkapi rentang kontinu dari terbuka bawah hingga terbuka atas | Alert hijau "Rentang Valid", tombol simpan aktif dan berhasil menyimpan draft | `PASS` |
| Uji Aturan 4-Mata (AC-3) | Login sebagai pembuat/pengubah draft dan amati tombol Sahkan | Tombol Sahkan dinonaktifkan dengan penjelasan aturan 4-mata | `PASS` |
| Uji Dialog Pemensiunan (AC-4) | Pengguna lain mengesahkan versi baru pada template yang sudah punya versi sah | Dialog konfirmasi menyebut versi sebelumnya yang akan dipensiunkan | `PASS` |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| **AC-1**: Daftar template beserta versinya, lengkap dengan status `Draft`, `Approved`, `Retired` | ✅ Terpenuhi | `DataTable` pada `sliding-scale-templates-view.jsx` dan pemilih versi pills pada `sliding-scale-version-modal.jsx`. |
| **AC-2**: Editor rentang menandai tumpang tindih dan lubang **secara visual sebelum simpan** | ✅ Terpenuhi | `validateSlidingScaleRanges` di `sliding-scale-template-utils.js` dijalankan secara live, menampilkan banner peringatan dan highlight baris tabel `.rangeRowError`. Terbukti dengan 4 skenario unit test PASS. |
| **AC-3**: Tombol Sahkan tidak tersedia bagi pengguna yang merupakan **pengubah terakhir** versi itu (`FR-DOK-095`) | ✅ Terpenuhi | `canApproveVersion` di `sliding-scale-template-utils.js` menonaktifkan tombol dan menampilkan penjelasan penolakan bila `currentUserId === lastModifiedByUserId`. Teruji di unit test PASS. |
| **AC-4**: Pengesahan menampilkan konfirmasi yang menyebut versi mana yang akan dipensiunkan | ✅ Terpenuhi | `ConfirmModal` menggunakan `getRetirementNotice` yang secara dinamis menyebut nomor versi yang akan dipensiunkan. Teruji di unit test PASS. |
| **AC-5**: Versi `Draft` ditandai jelas sebagai belum boleh dipakai membuat order | ✅ Terpenuhi | Banner peringatan merah `.draftOrderWarning` pada modal versi menegaskan bahwa versi draft belum boleh dipakai order. |
| **AC-6**: Penolakan dari server ditampilkan apa adanya, termasuk alasan rentang yang ditolak | ✅ Terpenuhi | State `serverError` menangkap pesan error respons Axios dan merendernya dalam Alert. |
| **Pita Keselamatan Klinis `RWI-OQ-097`** | ✅ Terpenuhi | Banner peringatan keselamatan klinis ditampilkan permanen di bagian atas layar. |
| **Definition of Done** | ✅ Terpenuhi | Linting PASS (0 error), Next.js build PASS (exit code 0), Unit tests PASS (9/9), laporan tracked tersedia, roadmap & traceability diperbarui. |

---

## 6. Catatan Penutup

| Hal | Isi |
| :--- | :--- |
| **Peringatan Produksi** | **`RWI-OQ-097` tetap tercatat terbuka**: Rumah sakit menyetujui pemakaian sliding scale (`RWI-DEC-155`), namun nama pejabat klinis yang berwenang mengesahkan protokol belum ditunjuk resmi oleh manajemen rumah sakit. Di lingkungan produksi, nol versi protokol boleh dinaikkan menjadi `Approved` untuk pasien sungguhan sampai SK/penunjukan resmi dikeluarkan. |
| **Risiko Tersisa** | `NONE` — Implementasi murni di frontend dan aman tanpa efek samping ke modul lain. |
| **Langkah Berikutnya** | Melanjutkan ke task berikutnya pada roadmap: **`FE-RWI-080`** (`FE-DOK-08` Rework Daftar Pantau Verifikasi memuat episode tertutup). |
