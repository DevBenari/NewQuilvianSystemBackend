# Laporan Perbaikan Frontend — `PAY-FE-FIX-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-012` (ad-hoc lanjutan seri `PAY-FE-FIX`; ID `011` dipakai pekerjaan pihak lain) |
| Judul | Modal "Tambah Tunjangan Pegawai": tombol Batal dan tutup tetap dapat dipakai saat jenis berbasis kebijakan terpilih |
| Slice | Corporate / Payroll Management, bagian Tunjangan Pegawai pada detail Profil Penggajian |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc; tidak ada roadmap atau traceability yang diubah (`NOT APPLICABLE`) |
| Trace | Audit dan desain pada `PAY-BE-FIX-012`; butir UI/UX "Tombol Batal selalu dapat digunakan saat tidak submitting" |
| Contract version | Tidak ada kontrak API yang diubah atau dikonsumsi baru |
| Wewenang UI | Perbaikan perilaku satu modal yang sudah ada; tanpa layar, route, atau komponen baru |
| Dependency | Tidak ada. Bagian besar tugas induk (dropdown bertingkat, bulk, pelepasan Transportasi) `BLOCKED_BY_SCHEMA` dan ada di `PAY-BE-FIX-012` |
| Klasifikasi | `LIGHT` — skor 2 (satu repository: 0; berkas diubah 2: 0; berkas diperiksa 9–20: 1; logika sederhana: 0; kontrak tidak ada: 0; UI satu modal: 1) |
| Task mode | `FRONTEND` (bagian dari `CROSS-REPO` yang backend-nya tidak menulis source) |
| Target tulis | `QuilvianSystemFrontendDev` ditambah berkas laporan ini |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `31df72b9d907796eb106c638a2a350405c866efa` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d642a9df9de35801ed8332adc0cdcd6127f96353` |
| Tanggal | 20 November 2026 |
| Status | 🟡 **Source selesai dan tes lulus; tampilan belum dilihat.** `VISUAL CHECK: NOT FEASIBLE` karena server dev dan build dilarang |

---

## 1. Keadaan yang ditemukan di awal

Di modal **Tambah Tunjangan Pegawai**, ketika petugas memilih jenis tunjangan berbasis kebijakan (mis. Transportasi), modal menampilkan informasi dan tombol simpan berubah menjadi "Dikelola via Kebijakan". **Tetapi tombol Batal, tombol tutup, tombol Esc, dan klik di luar modal ikut tidak berfungsi.** Petugas terkunci di modal tanpa jalan keluar kecuali memuat ulang halaman.

Penyebab, dengan bukti:

- Modal memasang `disabled={submitting || isPolicyBasedSelected}` pada `ConfirmModal` (`payroll-workforce-allowance-form-modal.jsx` baris 198 sebelum perubahan).
- Pada `ConfirmModal`, prop `disabled` berarti **sibuk**: `const isBusy = Boolean(loading || disabled)` (baris 92). `isBusy` menonaktifkan tombol Batal (baris 231), tombol tutup (baris 211), `handleCancel` (baris 116), `handleHide` (baris 137), dan Esc (`keyboard={!isBusy && keyboard}`, baris 148).
- Syarat yang khusus untuk tombol simpan seharusnya lewat `confirmProps`, yang hanya memengaruhi tombol simpan (baris 101-102).

Tes teks lama `payroll-allowance-policy-grade-sync.test.mjs` bahkan **mengunci baris yang salah itu** (asersi `disabled={submitting || isPolicyBasedSelected}`).

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka detail Profil Penggajian, bagian Tunjangan Pegawai, lalu menekan **+ Tambah Tunjangan**.
2. Petugas memilih Jenis Tunjangan.
3. Bila jenis itu berbasis kebijakan (Transportasi): modal menampilkan informasi bahwa jenis itu dikelola lewat kebijakan, tombol simpan **dinonaktifkan** ("Dikelola via Kebijakan"), dan petugas tetap dapat menekan **Batal**, tombol tutup, atau Esc.
4. Saat petugas menekan simpan pada jenis lain, seluruh tombol dikunci sampai permintaan selesai (tidak berubah).

**Jalur tidak normal.**

| Kejadian | Yang dilihat petugas |
| --- | --- |
| Jenis berbasis kebijakan dipilih | Informasi kebijakan; simpan nonaktif; **Batal aktif** |
| Sedang menyimpan | Semua tombol terkunci (dijaga `loading={submitting}`), seperti sebelumnya |
| Master kosong, error, atau tanpa izin | Pesan bawaan modal (tidak diubah) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`payroll-workforce-allowance-form-modal.jsx`, `confirm-modal.jsx`, `payroll-workforce-allowance-section.jsx`, `use-payroll-workforce-allowance.js`, `payroll-compensation-detail-view.jsx`, route dan menu terkait transport, serta tes `payroll-allowance-policy-grade-sync.test.mjs` dan `payroll-workforce-allowance.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/payroll/compensation/payroll-workforce-allowance-form-modal.jsx` | `disabled={submitting \|\| isPolicyBasedSelected}` diganti `confirmProps={{ disabled: isPolicyBasedSelected }}`, disertai komentar alasan. `loading={submitting}` tetap menjaga keadaan menyimpan (+4 baris, −1 baris) |
| `tests/unit/payroll-allowance-policy-grade-sync.test.mjs` | Asersi yang mengunci baris lama diganti: kini mengasersikan `confirmProps` dan **melarang** pola lama. Ditambah satu tes yang mendokumentasikan premisnya (`disabled` pada `ConfirmModal` = sibuk yang mengunci Batal, tutup, dan Esc) agar perubahan komponen dasar kelak terdeteksi |

Dengan sengaja **tidak diubah**: tombol "Kelola Transportasi", route `workforce-compensations/[id]/transport-allowance`, menu "Tunjangan Transportasi", hook, service, konstanta, util, komponen transport, dan CTA "Buka Penetapan Transportasi Pegawai". Menghapusnya sekarang akan menutup satu-satunya jalan mengelola transport sebelum pengganti ada (`BLOCKED_BY_SCHEMA`, lihat `PAY-BE-FIX-012`).

### 3.3 Kepatuhan arsitektur frontend

Tidak ada route, hook, slice, service, atau CSS baru. Perubahan hanya memakai prop `confirmProps` yang sudah didukung `ConfirmModal`; tidak ada perubahan pada komponen dasar.

### 3.4 Gerbang tampilan

**Modul referensi visual:** modal itu sendiri.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Modal konfirmasi dan tombolnya | `ConfirmModal` (`confirmProps`) | `REUSE` | Prop sudah ada dan didukung (`confirm-modal.jsx` baris 85, 101-102, 240-247) |

`UI GATE: PASS — 1 elemen REUSE, 0 EXTEND, 0 COMPOSE, 0 WRAP, 0 NEW`

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    src/components/view/payroll/compensation/payroll-workforce-allowance-form-modal.jsx
OK — 1 berkas diperiksa, 0 temuan.

$ grep (button mentah, utility tipografi, dialog manual, input mentah/UUID, prop tidak sah, class Tailwind)
(kosong)

$ eslint src/components/view/payroll/compensation/payroll-workforce-allowance-form-modal.jsx
(exit 0, tanpa keluaran)
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| Modal Tambah Tunjangan Pegawai, jenis berbasis kebijakan terpilih | — | `VISUAL CHECK: NOT FEASIBLE` |

`VISUAL CHECK: NOT FEASIBLE` — server development dan build dilarang pada task ini. Perilaku tombol Batal belum dicoba di peramban; buktinya adalah pembacaan source `ConfirmModal` dan tes teks. Satu catatan perilaku bawaan komponen dasar yang tidak saya ubah: `ConfirmModal` memberi kelas `is-loading` pada tombol simpan setiap kali ia nonaktif (baris 244), sehingga tombol "Dikelola via Kebijakan" mungkin tampak seperti sedang memuat. Perlu dilihat saat verifikasi tampilan.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat daftar jenis | Keadaan memuat bawaan (tidak diubah) |
| Kosong / semua nonaktif / tidak ada yang layak | Pesan bawaan modal (tidak diubah) |
| Gagal | Pesan galat bawaan dan tombol coba lagi (tidak diubah) |
| Tanpa hak akses | Pesan bawaan (tidak diubah) |
| Jenis berbasis kebijakan terpilih | Informasi, simpan nonaktif, **Batal dan tutup aktif** (diperbaiki) |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini tidak mengubah panggilan API.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `payroll-allowance-policy-grade-sync.test.mjs` | **11 lulus, 0 gagal** | `PASS` | Dieksekusi; sebelumnya 10 tes, kini 11 |
| `payroll-workforce-allowance.test.mjs` | **20 lulus, 0 gagal** | `PASS` | Dieksekusi; berkas ini juga membaca modal yang sama |
| `check-css-usage.mjs` | 0 temuan | `PASS` | Bagian 3.4 |
| Grep anti-regresi | Kosong | `PASS` | Bagian 3.4 |
| ESLint pada modal | 0 error, 0 warning | `PASS` | Bagian 3.4 |
| Akhir baris | `LF` seluruhnya, sama dengan `HEAD` (CR = 0) | `PASS` | Hitung CR terhadap `git show HEAD:` |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dilarang pada task ini |
| Verifikasi tampilan dan klik Batal di peramban | Tidak dilakukan | `NOT RUN` | `VISUAL CHECK: NOT FEASIBLE` |

`AUTOMATED TEST: node --import ./QuilvianSystemFrontendDev/tests/helpers/register.mjs --test <2 berkas> — PASS (31 dari 31)`. Ini tes **teks source**, bukan eksekusi komponen; ia mengunci perubahan tetapi bukan bukti perilaku di layar.

Uji manual: `NOT FEASIBLE`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Batal dan tutup dapat dipakai saat jenis berbasis kebijakan terpilih | Terpenuhi pada source | Bagian 1 dan 3.2 |
| Penguncian saat menyimpan tetap bekerja | Terpenuhi pada source | `loading={submitting}` tidak diubah |
| Tombol Kelola Transportasi dan jalur transport tidak dihapus sebelum pengganti siap | Terpenuhi | Bagian 3.2 |
| Tampilan sesuai di layar | **Belum terpenuhi** | `VISUAL CHECK: NOT FEASIBLE` |
| Build berhasil | **Belum terpenuhi** | `NOT RUN` |
| Butir tugas induk (dropdown bertingkat, bulk, pelepasan Transportasi) | **Di luar task ini** | `BLOCKED_BY_SCHEMA`; lihat `PAY-BE-FIX-012` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini sengaja kecil. Ia **tidak** memenuhi sebagian besar tugas induk |
| Masalah yang diketahui | Lihat `PAY-BE-FIX-012` bagian 1.6a dan 1.5 (celah anti-ganda dan tarif tertanam) |
| Dependency backend | Tidak ada untuk perbaikan ini; bagian induknya bergantung schema baru |
| Perubahan sampingan | `NONE` oleh task ini. Pada working tree ini juga terdapat perubahan pihak lain (editor workforce, `salary-structure-allowance-table-columns.jsx`, `payroll-run.module.css`, `workforce-salary-assignment.test.mjs`) dan pekerjaan `PAY-FE-FIX-010`; tidak disentuh |
| Interupsi | `NONE` |
| Status Git | Tidak ada stage, commit, push, pull, merge, atau rebase |
| Langkah berikutnya | Pemilik melihat modal di peramban (pilih Transportasi, tekan Batal); lalu keputusan D1 s.d. D8 pada `PAY-BE-FIX-012` |

```text
$ git status --short -- <berkas milik task ini>   (frontend, branch yasmina, HEAD 31df72b9)
 M src/components/view/payroll/compensation/payroll-workforce-allowance-form-modal.jsx
 M tests/unit/payroll-allowance-policy-grade-sync.test.mjs
```
