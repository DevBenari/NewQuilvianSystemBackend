# Laporan Perubahan Frontend — `FE-HRD-326`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-326` |
| Judul | Formulir ubah data penggajian per orang (modal pada halaman daftar) |
| Slice | `S-A8` / `MVP-69` — gelombang 2 |
| Roadmap | `roadmap/frontend-roadmap.md` bagian Slice S-A8, kartu `FE-HRD-326` |
| Trace | `HRD-DEC-218` (perubahan mencabut persetujuan/verifikasi), `HRD-DEC-219` (detail hanya hak Update), `HRD-DEC-221` (pilihan master); API Contract §33; Validation `VR-WPD-*`; pasangan backend `BE-HRD-361`, `BE-HRD-362` |
| Contract version | `v27` bagian 33 — approved 16 November 2026 (`HRD-DEC-015`) |
| Wewenang UI | Modal ubah (dua bagian dan dua opsi) sesuai kartu roadmap. Base component bersama (`ConfirmModal`, `BaseCheckboxCard`) tidak diubah |
| Dependency | `FE-HRD-325` ✅, `BE-HRD-361` ✅, `BE-HRD-362` ✅ |
| Klasifikasi | `MEDIUM` — 2 cacat diperbaiki di source yang sudah ada, uji unit ditambah, verifikasi di browser dengan tulis sungguhan yang disetujui |
| Task mode | `FRONTEND` (instruksi pengguna 18 November 2026 "lanjut FE-HRD-326") |
| Target tulis | `QuilvianSystemFrontendDev`: `workforce-payroll-data-edit-modal.jsx`, `workforce-payroll-data-utils.js`, `tests/unit/workforce-payroll-data.test.mjs`; laporan ini, roadmap, dan traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `baad7dd62` (branch `YogaV2`); perubahan `FE-HRD-325` belum di-commit |
| Commit backend yang dijadikan rujukan | `a17a2bc3` (branch `yoga`) beserta perubahan `BE-HRD-360` s.d. `363` yang belum di-commit |
| Tanggal | 18 November 2026 |
| Status | ✅ Selesai — 6/6 acceptance criteria terbukti di browser; 2 cacat diperbaiki; uji unit 23/23 (2 baru, kontrol negatif tertangkap); ESLint dan pemeriksa CSS bersih; data uji Dewi Anggraeni dikembalikan identik |

---

## 1. Keadaan yang ditemukan di awal

Source formulir sudah ada sejak 16 November 2026 tetapi belum pernah dicoba di browser. Pemeriksaan source menemukan dua cacat:

| Cacat | Akibat bagi pengguna | Bukti |
| --- | --- | --- |
| Modal memberi `disabled={!canSubmit}` pada `ConfirmModal`. Bagi `ConfirmModal`, `disabled` berarti "sibuk": tombol **Batal**, tombol Esc, dan penutupan modal ikut terkunci | Bila detail **gagal dimuat**, Simpan nonaktif dan Batal juga nonaktif. Formulir tidak dapat ditutup sama sekali; pengguna harus memuat ulang halaman | `confirm-modal.jsx`: `isBusy = loading \|\| disabled`; Batal `disabled={isBusy}`, `handleHide` berhenti bila `isBusy` |
| Gaji tersimpan diisikan kembali dengan `String(baseSalary)` | Gaji berdesimal, misalnya Rp 12.500.000,50, muncul sebagai "12500000.5". Isian ini lalu ditolak sendiri ("Gaji pokok harus berupa angka.") karena titik dibaca pemisah ribuan, jadi orang itu tidak dapat disimpan walau gajinya tidak diubah | `parseWorkforceAmountInput("12500000.5")` → `NaN` |

Sisanya sudah sesuai kontrak: opsi setuju/verifikasi mengikuti hak `WfpSalaryAssignment : Update` dan `WorkforceBankAccount : Update`, simpan dikunci `lockRef` dari tekan ganda, galat backend ditampilkan apa adanya, dan hasil simpan dimunculkan sebagai toast.

---

## 2. Proses bisnis dari sisi pengguna

1. Di halaman **Data Penggajian Pegawai**, pemegang `WorkforcePayrollData : Update` menekan **Ubah** pada baris seseorang.
2. Formulir **Ubah Data Penggajian** terbuka dan memuat isian saat ini. Selama memuat tampil "Memuat data penggajian..." dan **Simpan Data** tidak aktif. Contoh untuk Dewi Anggraeni:
   - Struktur "Struktur UJI-362 (SST-RSMMC-00001)", gaji pokok 13000000, mulai 01 Nov 2026, dengan keterangan "Saat ini sudah disetujui."
   - Petunjuk di bawah isian gaji: "Rp 13.000.000 · Rentang grade SGR-RSMMC-00006: Rp 10.000.000 – Rp 16.000.000".
   - Rekening Bank Central Asia, nomor lengkap 9900362003, pemilik "Dewi Anggraeni S", dengan keterangan "Saat ini sudah terverifikasi."
3. Petugas mengubah isian. Gaji boleh ditulis 12500000 maupun 12.500.000.
4. Dua opsi tersedia di bawah formulir: **Setujui penetapan gaji** dan **Tandai rekening terverifikasi**. Bagi petugas tanpa hak yang sesuai, opsi itu nonaktif dengan keterangan "Peran Anda tidak memiliki hak untuk opsi ini."
5. Petugas menekan **Simpan Data**. Selama menyimpan, Simpan dan Batal sama-sama tidak dapat ditekan.
6. Bila berhasil, formulir tertutup, daftar dimuat ulang, lalu tampil toast hasil, misalnya "Data penggajian disimpan: penetapan gaji diperbarui.", disusul toast peringatan bila ada, misalnya "Penetapan gaji berubah, jadi persetujuannya dicabut dan perlu disetujui kembali."

Jalur tidak normal:

| Keadaan | Yang dilihat pengguna |
| --- | --- |
| Satu isian bagian dikosongkan (mis. gaji pokok) sementara isian lain bagian itu terisi | "Gaji pokok wajib diisi." di bawah isian; tidak ada yang dikirim |
| Gaji bukan angka (mis. "abc") | "Gaji pokok harus berupa angka." |
| Orang yang belum punya data: hanya bagian gaji diisi, bagian rekening dibiarkan kosong seluruhnya | Hanya penetapan gaji yang dikirim; rekening dikirim `null` sehingga tidak disentuh |
| Ditolak backend (mis. nomor rekening sudah tercatat) | Kotak merah "Nomor rekening sudah tercatat untuk orang ini." di dalam formulir; formulir tetap terbuka dengan isian utuh; kotak itu hilang begitu isian diubah |
| Detail gagal dimuat | Kotak merah berisi pesan galat; Simpan nonaktif; **Batal tetap dapat menutup formulir** |
| Gaji di luar rentang grade (mis. Rp 9.000.000 untuk grade 10–16 juta) | Tetap tersimpan; toast peringatan "Gaji pokok di luar rentang grade SGR-RSMMC-00006 (Rp 10.000.000 – Rp 16.000.000)." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`src/components/view/hr/workforce-core/workforce-payroll-data-edit-modal.jsx`, `workforce-payroll-data-view.jsx`; `src/lib/hooks/hr/workforce-core/use-workforce-payroll-data-edit.js`; `src/utils/hr/workforce-core/workforce-payroll-data-utils.js`; `src/lib/constants/hr/workforce-core/workforce-payroll-data-constants.js`; base component `confirm-modal.jsx`, `base-checkbox-card.jsx` beserta CSS Module-nya, `resource-filter-select.jsx`, `filter-select.jsx`, `base-form-control.jsx`; `tests/unit/workforce-payroll-data.test.mjs`; API Contract v27 §33.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-core/workforce-payroll-data-edit-modal.jsx` | `disabled={!canSubmit}` diganti `confirmProps={{ disabled: !canSubmit }}`: hanya Simpan yang dikunci saat memuat atau gagal memuat. `loading={submitting}` tetap mengunci Batal dan penutupan selama simpan berjalan (AC6) |
| `src/utils/hr/workforce-core/workforce-payroll-data-utils.js` | Fungsi pembantu `toAmountInput` menulis gaji tersimpan dengan koma desimal (12500000.5 → "12500000,5"); gaji bulat tidak berubah ("13000000") |
| `tests/unit/workforce-payroll-data.test.mjs` | 2 uji baru: gaji berdesimal diisikan kembali lalu terbaca nilai yang sama, lolos validasi, dan masuk payload; modal mengunci Simpan lewat `confirmProps` dan Batal hanya lewat `loading` |

### 3.3 Kepatuhan arsitektur frontend

Tidak ada pola baru. Alur tetap view → hook (`useWorkforcePayrollDataEdit`) → service Axios; utilitas murni untuk isian dan payload. `confirmProps` adalah prop resmi `ConfirmModal` yang sudah ada. Base component bersama tidak disentuh.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat data penggajian..."; Simpan nonaktif, Batal aktif |
| Kosong | `NOT APPLICABLE` sebagai daftar; orang tanpa data membuka formulir dengan kedua bagian kosong |
| Gagal | Gagal memuat: kotak merah berisi pesan, Simpan nonaktif, Batal menutup. Gagal simpan: kotak merah berisi pesan backend, formulir tetap terbuka |
| Tanpa hak akses | Tombol **Ubah** hanya bagi pemegang `WorkforcePayrollData : Update` (`FE-HRD-325`); opsi setuju/verifikasi nonaktif dengan keterangan bagi yang tidak berhak |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Core / Payroll Data

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-core/payroll-data/{workforceProfileId}` | Isian saat ini, termasuk gaji pokok dan nomor rekening lengkap | `WorkforcePayrollData : Update` |
| `PUT` | `/v1/corporate/human-resource/workforce-core/payroll-data/{workforceProfileId}` | Simpan bagian yang diisi beserta opsi | `WorkforcePayrollData : Update`; opsi butuh `WfpSalaryAssignment : Update` / `WorkforceBankAccount : Update` |

#### Corporate / Human Resource / Master Data — pilihan formulir (sudah ada)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/salary-structures/options` | Pilihan Struktur Gaji | `SalaryStructure : Read` |
| `GET` | `/v1/administrator/master-data/banks/options` | Pilihan Bank | `Bank : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/workforce-payroll-data.test.mjs` | 23/23 lulus (21 lama + 2 baru) | `PASS` | Keluaran perintah |
| Kontrol negatif: versi HEAD kedua berkas source dipasang sementara | 2 uji baru gagal, 21 lulus; berkas dipulihkan identik | `PASS` | Keluaran perintah |
| `npx eslint` atas modal, utilitas, hook ubah, dan berkas uji | 0 masalah | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` atas modal | 0 temuan | `PASS` | Keluaran perintah |
| Browser (`next dev` → backend lokal 7184, devYoga, superadmin; tulis sungguhan hanya `PUT` Dewi Anggraeni, disetujui pengguna) — memuat (detail ditunda 3 detik) | "Memuat data penggajian..." tampil; Simpan nonaktif; Batal aktif | `PASS` (AC1) | `fe-326.mjs`, `326-memuat.png` |
| — Isian saat ini | Gaji "13000000", nomor lengkap sama dengan data uji, pemilik "Dewi Anggraeni S", struktur, bank, tanggal 01 Nov 2026, status "sudah disetujui"/"sudah terverifikasi", petunjuk rentang grade | `PASS` (AC1) | `326-formulir.png`, `326-bawah-normal.png` |
| — Gagal memuat (detail `500` disuapkan) | Pesan galat server di formulir; Simpan nonaktif; Batal aktif dan **menutup** formulir | `PASS` (AC1, perbaikan cacat 1) | `326-gagal-memuat.png` |
| — Gaji dikosongkan / "abc" | "Gaji pokok wajib diisi." / "Gaji pokok harus berupa angka."; 0 `PUT` | `PASS` (AC2) | `fe-326.mjs` |
| — Orang tanpa data (`RSMMC-20260101001`): struktur, gaji 12000000, tanggal diisi; rekening dibiarkan kosong (`PUT` disuapkan, tidak sampai backend) | Payload `bankAccount: null`, `salary.baseSalary: 12000000` | `PASS` (AC2) | idem |
| — Gaji "12.500.000" tanpa opsi (tulis sungguhan) | Payload `baseSalary: 12500000`; formulir tertutup; toast "Data penggajian disimpan: penetapan gaji diperbarui." dan "Penetapan gaji berubah, jadi persetujuannya dicabut dan perlu disetujui kembali."; baris Dewi berubah menjadi **Menunggu Persetujuan** / **Belum Lengkap** | `PASS` (AC2, AC4) | `326-tersimpan.png` |
| — Gaji "9000000" + opsi setuju (tulis sungguhan) | Toast "…penetapan gaji diperbarui dan disetujui." dan peringatan rentang grade | `PASS` (AC4) | `fe-326.mjs` |
| — Penolakan backend (`400` disuapkan, tertunda 2,5 detik); Simpan ditekan dua kali; Batal dan Esc ditekan selama simpan | Hanya **1** `PUT`; Simpan dan Batal nonaktif selama simpan; formulir tetap terbuka sesudah Batal/Esc; pesan "Nomor rekening sudah tercatat untuk orang ini." tampil di formulir, isian "13.000.000" utuh; pesan hilang begitu isian diubah | `PASS` (AC5, AC6) | `326-menyimpan.png`, `326-bawah-ditolak.png` |
| — Tanpa hak opsi (`/v1/auth/permissions` disuapkan: bukan superadmin, tanpa `WfpSalaryAssignment`/`WorkforceBankAccount : Update`) | Kedua opsi nonaktif, masing-masing berketerangan "Peran Anda tidak memiliki hak untuk opsi ini."; Simpan tetap aktif | `PASS` (AC3) | `326-tanpa-hak.png`, `326-bawah-tanpa-hak.png` |
| — Ponsel 390 px | Modal 355 px, dokumen 375 px; isian bertumpuk satu kolom | `PASS` | `326-ponsel.png` |
| Pemulihan data uji | Simpan terakhir lewat formulir: Rp 13.000.000 + opsi setuju. Basis data Dewi sesudah uji **identik** dengan sebelum uji untuk gaji, struktur, tanggal mulai, status disetujui, bank, nomor, pemilik, dan status terverifikasi. Waktu persetujuan berganti ke waktu uji | `PASS` | `dewi-sebelum.json`, `dewi-sesudah.json`, `at-log.jsonl` ("FE-326 …") |

Uji manual: `PASS`.

**Tidak dijalankan:** `npm run build` sesudah perubahan ini (dijalankan pada `FE-HRD-325` sebelum perubahan; build ulang menjadi bagian `FE-HRD-327`). Gaji berdesimal tidak diuji di browser karena devYoga hanya punya gaji bulat; jalurnya dikunci uji unit.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Formulir memuat isian saat ini; selama memuat dan saat gagal memuat, tombol Simpan tidak aktif | Terpenuhi; ditambah Batal kini tetap bekerja saat gagal memuat | Bagian 6: memuat, isian saat ini, gagal memuat |
| 2. Bagian yang diisi harus utuh; bagian yang dikosongkan seluruhnya tidak dikirim. Gaji menerima 12000000 maupun 12.000.000 | Terpenuhi | Gaji kosong/"abc" ditolak; rekening kosong → `null`; "12.500.000" → 12500000; "13000000" → 13000000 |
| 3. Opsi setuju dan verifikasi nonaktif beserta keterangannya bagi pengguna tanpa hak | Terpenuhi | Uji izin yang disuapkan |
| 4. Simpan berhasil menutup formulir, memuat ulang daftar, dan menampilkan toast hasil beserta peringatannya | Terpenuhi | Dua simpan sungguhan; baris berubah tanpa memuat ulang halaman |
| 5. Penolakan Backend tampil di formulir dan tidak menutupnya; tombol tidak dapat ditekan dua kali | Terpenuhi | 1 `PUT` untuk dua tekanan; pesan di formulir |
| 6. Tombol Batal tidak bekerja saat simpan sedang berjalan | Terpenuhi | Batal nonaktif; Batal dan Esc tidak menutup formulir |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Temuan tampilan pada base component bersama (tidak diubah, di luar wewenang task): `BaseCheckboxCard` varian `compact` menampilkan tanda ✓ gelap di kotak abu-abu walaupun **tidak** dicentang; tercentang = kotak teal dengan ✓ putih. Pada opsi persetujuan gaji, tampilan ini bisa disangka sudah dipilih. Dipakai 13 view; perlu keputusan UI Lead |
| Masalah yang diketahui | Bagian yang sudah terisi dari data tersimpan tidak dapat dikosongkan (pilihan Bank/Struktur tanpa tombol kosongkan). Sesuai kontrak: bagian yang tidak diubah dikirim apa adanya dan backend menjawab "Tidak diubah"; menghapus data memang bukan cakupan slice ini |
| Dependency backend | `BE-HRD-361`, `BE-HRD-362` ✅; tidak ada yang tertunda |
| Perubahan sampingan | Data devYoga: tiga simpan sungguhan pada Dewi Anggraeni (disetujui pengguna) lalu dikembalikan; tercatat 3 baris log audit `WorkforcePayrollData.Update`. Tidak ada berkas tracked lain yang tersentuh |
| Interupsi | `NONE` |
| Status Git | ` M src/app/hr/workforce-core/payroll-data/page.jsx` (`FE-HRD-325`) · ` M src/components/view/hr/workforce-core/workforce-payroll-data-edit-modal.jsx` · ` M src/lib/hooks/hr/workforce-core/use-workforce-payroll-data.js` (`FE-HRD-325`) · ` M src/utils/hr/workforce-core/workforce-payroll-data-utils.js` · ` M tests/unit/workforce-payroll-data.test.mjs` |
| Langkah berikutnya | `FE-HRD-327` — navigasi sesuai hak, `npm run build`, dan uji layar menyeluruh |
