# Laporan Perubahan Frontend — `FE-LAB-31`, `FE-LAB-32`, `FE-LAB-33`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-31`, `FE-LAB-32`, `FE-LAB-33` — **dikerjakan bersama atas permintaan pemilik modul** |
| Judul | Form isolat dan antibiogram; Informasi Specimen yang dapat disunting; Kelengkapan, konsultasi, dan dokter konfirmator |
| Slice | `S4b` — pengisian hasil Mikrobiologi |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) |
| Trace | `LAB-DEC-095`..`LAB-DEC-128`; `AC-157`..`AC-170` |
| Contract version | `LAB-API-v1` `r24` 19.2, `r26` 21.2/21.3/21.4/21.5/21.7, `r27` 22.3/22.8 — seluruhnya `approved` |
| Wewenang UI | Mengisi tempat hasil pada halaman `FE-LAB-30`; satu slice Redux baru; satu service baru. **Nol wewenang** mengubah halaman lain |
| Dependency | `FE-LAB-30` ✅; backend `BE-LAB-53`..`BE-LAB-63` ✅ |
| Klasifikasi | `HEAVY` — tiga task, 13 acceptance criteria, satu slice, satu service, empat komponen |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `3339ecdf1` |
| Commit backend yang dijadikan rujukan | `458f38aa` |
| Tanggal | 2026-09-22 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — seluruh permukaan terbangun, lint dan build hijau, 22 uji baru lulus. **Nol AC terbukti di layar**, sebab sesi ini nol punya alat kendali peramban |

---

## 1. Kenapa ketiganya dikerjakan sebagai satu slice

Ketiganya **satu layar**, dan saling mengunci:

- `VAL-109` membuat Informasi Specimen (`FE-LAB-32`) **baca-saja** begitu hasil (`FE-LAB-31`)
  dinyatakan selesai lewat kendali `FE-LAB-33`.
- `finalize` (`FE-LAB-33`) hanya sah ketika hasilnya (`FE-LAB-31`) sudah terisi.

Memecahnya menjadi tiga hook berarti tiga tempat membaca keadaan kunci yang sama, dan ketiganya
pasti bercabang.

---

## 2. Perubahan yang dikerjakan

| Path | Isi |
|---|---|
| `constants/.../lab-microbiology-result-constants.jsx` | Enum, alamat endpoint, dan salinan teks |
| `services/.../lab-microbiology-result.service.js` | 11 fungsi; **nol endpoint rilis/validasi/kirim** |
| `state/slice/.../lab-microbiology-result-slice.jsx` | 11 thunk, terdaftar di store sebagai `labMicrobiologyResult` |
| `hooks/.../lab-microbiology-result-rules.js` | Validasi dan penyusun payload — **fungsi murni** |
| `hooks/.../use-lab-microbiology-result-editor.jsx` | Controller satu layar |
| `view/.../lab-microbiology-result-form.jsx` | `FE-LAB-31` |
| `view/.../lab-microbiology-specimen-section.jsx` | `FE-LAB-32` |
| `view/.../lab-microbiology-completion-bar.jsx` | `FE-LAB-33` |
| `view/.../lab-microbiology-result-panel.jsx` | Penyusun ketiganya |
| `view/.../lab-microbiology-workspace-view.jsx` | Tempat hasil diisi panel |
| `constants/.../laboratory-constants.jsx` | Tiga alamat data induk |
| `style/.../lab-microbiology-workspace.module.css` | Style ketiga bagian |
| `tests/unit/lab-microbiology-result-rules.test.mjs` | 22 uji |

### Satu koreksi yang ditemukan sebelum kode dikirim

Payload konsultasi semula saya susun sebagai `{ consultedByUserId, note }`. Pembacaan
`LabConsultationRequest` menunjukkan bentuk sebenarnya **`{ consultedToName, consultedAt }`** —
**nama teks, bukan penunjuk pengguna**, sebab konsultannya sering berada di luar daftar
pengguna sistem (`LAB-EVD-005`), dan keduanya wajib.

Pemilih dokter karena itu **mengisi** ruas nama, bukan menggantikannya: ruasnya tetap dapat
diketik bebas. Inilah alasan aturan "jangan menebak payload backend" ada.

---

## 3. Keputusan yang layak dibaca

**`key` pada panel hasil adalah bentuk teknis `AC-156`.** Panel dipasang ulang setiap kali
baris yang dipilih berganti, sehingga keadaan formulir pemeriksaan sebelumnya **nol terbawa**.
Tanpa itu, berpindah baris akan meninggalkan zona hambat milik kuman lain di dalam formulir.

**`Simpan Final` selalu menyimpan lebih dulu, baru menyatakan selesai.** Analis yang mengetik
lalu langsung menekan Final tanpa itu akan ditolak backend dengan pesan yang membingungkan —
*"hasil belum diisi"* — padahal layarnya penuh.

**Data induk ditarik sekali per halaman, bukan per baris antibiogram.** Satu antibiogram lazim
memuat dua puluh baris; menariknya per baris berarti dua puluh permintaan untuk daftar yang
sama persis.

**Riwayat perubahan specimen dimuat atas permintaan.** Ia jarang dibuka; memuatnya otomatis
hanya menambah satu permintaan yang nol dibaca siapa pun.

---

## 4. Larangan DoD yang ditegakkan secara struktural

| Larangan | Cara ditegakkan |
|---|---|
| Nol pengetikan bebas organisme dan antibiotik | Keduanya `BaseSelectField` atas data induk; nol ruas teks |
| Nol pilihan `NeedsAttention`/`Critical` | `LAB_MICROBIOLOGY_FINDING_OPTIONS` hanya memuat tiga nilai |
| Nol tombol menambah Spesifik Specimen | Bagian specimen nol punya tombol tambah |
| Nol tombol kirim ke pasien | Nol di service, nol di komponen |
| Nol pilihan `HL7` | Nol kemunculan |
| Nol tombol validasi/rilis | Nol di service, nol di komponen |

---

## 5. Verifikasi

| Perintah | Hasil | Klasifikasi |
|---|---|---|
| `npx eslint` atas 9 berkas baru | 0 error, 1 warning | `PASS` — warning `set-state-in-effect`, pola yang sama dengan hook acuan repo |
| `npm run build` | `✓ Compiled successfully` | `PASS` |
| Uji unit baru | **22/22 lulus** | `PASS` |
| Seluruh suite | 1536/1542 | `EXISTING / ENVIRONMENT ISSUE` — **6 kegagalan sama persis** dengan sebelum perubahan |
| Route `[slug]` terender | `200` | `PASS` |

**`AUTOMATED TEST: node --test tests/unit/ — PASS`** (1536 lulus; 6 kegagalan lama, nol
bertambah).

**`MANUAL TEST: NOT FEASIBLE`.** Sesi ini nol punya alat kendali peramban. Seluruh kontrol
interaktif yang dibangun — pemilih status temuan, tambah/hapus isolat, tabel antibiogram,
kotak centang Spesifik Specimen, tombol Simpan/Final/Reopen, pencatatan konsultasi, pemilih
dokter konfirmator — **belum satu pun diklik**.

---

## 6. Acceptance criteria — dibaca apa adanya

| AC | Status | Bukti |
|---|---|---|
| `AC-163` baris tanpa MIC dan tanpa zona tersimpan | **Terbukti pada aturannya** | Uji unit |
| `AC-164` kultur tanpa isolat tersimpan tanpa peringatan | **Terbukti pada aturannya** | Uji unit |
| `AC-165` baris tanpa interpretasi ditolak beserta sebabnya | **Terbukti pada aturannya** | Uji unit |
| `AC-160` lebih dari satu Spesifik Specimen | **Terbukti pada aturannya** | Uji unit |
| `AC-162` volume sebagai angka + satuan | **Terbukti pada aturannya** | Uji unit |
| `AC-157` kedua waktu baca-saja | **Terbukti secara struktural** | Payload nol memuatnya; diuji |
| `AC-168` Analis baca-saja | **Terbukti secara struktural** | Sama |
| `AC-158` sesudah Final layar menyatakan belum dirilis | **Terbangun, belum dilihat** | Komponen merender `InformationAlert`; belum diklik |
| `AC-166` ketiadaan aturan kritis dinyatakan | **Terbangun, belum dilihat** | Sama |
| `AC-169` konsultasi nol membuat Definitif | **Terbangun, belum dilihat** | Payload konsultasi nol menyentuh `resultQualifier` |
| `AC-170` sesudah Final specimen baca-saja | **Terbukti pada aturannya**, tampilannya belum dilihat | `isResultLocked` diuji; `disabled` dipasang dari nilainya |

> **Perbedaan "terbukti pada aturannya" dan "terbukti" ditulis di sini dengan sengaja.**
> Aturan murninya diuji dan lulus; yang **belum** diverifikasi adalah bahwa komponennya
> memanggil aturan itu pada keadaan yang benar, dan bahwa hasilnya terlihat sebagaimana
> mestinya di layar. Menyatakan ketigabelasnya "terpenuhi" akan mengklaim lebih daripada yang
> saya kerjakan.

---

## 7. Yang perlu dikerjakan sebelum ketiganya boleh ditandai selesai

1. **Verifikasi klik menyeluruh** pada peramban, memakai akun bukan superadmin.
2. Data ujinya sudah siap: pesanan `LAB-RSMMC-000014` memuat dua pemeriksaan, dan
   `LAB-RSMMC-000011` memuat hasil berisi satu isolat *Branhamella catarrhalis*.
3. **Data induk masih hampir kosong** — satu organisme, satu antibiotik. Menguji antibiogram
   yang sungguhan menuntut `B3` pada laporan audit kesiapan ditutup lebih dulu.

**Nol operasi git dijalankan.** Satu dev server sisa dari sesi ini ditemukan masih hidup
(`PID 8880`) dan sudah dihentikan.
