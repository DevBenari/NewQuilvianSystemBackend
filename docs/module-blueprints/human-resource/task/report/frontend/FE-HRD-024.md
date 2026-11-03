# Laporan Perubahan Frontend — `FE-HRD-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-024` |
| Judul | Formulir Pembuatan Dokumen SK, Detail, Unggah Scan, dan Panel Persetujuan |
| Slice | `S-A1` (Workforce Core Target State) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-048`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `HRD-DEC-031`, `BE-HRD-025` |
| Contract version | Personnel Action Order API v1 (`HrdPersonnelActionOrderController.cs`) |
| Dependency | `FE-HRD-022` (✅ Selesai), `FE-HRD-023` (✅ Selesai) |
| Klasifikasi | `HIGH` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/workforce-core/personnel-actions/create/page.jsx`, `src/app/hr/workforce-core/personnel-actions/[id]/page.jsx`, `src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx`, `src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx` |
| Branch frontend | `HamzahV2` |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Penerbitan Surat Keputusan (SK) mutasi dan promosi pegawai rumah sakit merupakan dokumen legal operasional yang memerlukan ketelitian dan tata kelola independen.

Task ini mengimplementasikan dua antarmuka utama:
1. **Formulir Pembuatan SK Baru (`/create`)**:
   - Memilih pegawai target, kategori tindakan kepegawaian, tanggal efektif, dan dasar pertimbangan hukum/operasional.
   - Mengisi formasi target: unit organisasi, departemen, jabatan struktural/fungsional, relasi atasan langsung, dan usulan gaji baru.
   - Validasi data dasar dan panduan tata kelola terkait larangan mutasi retroaktif pada periode absensi tertutup (`HRD-DEC-050`).
2. **Layar Detail, Unggah Scan, dan Panel Persetujuan (`/[id]`)**:
   - Menampilkan perbandingan status kedinasan dan linimasa tanggal efektif.
   - Form pengunggahan berkas scan fisik SK bertandatangan basah/elektronik (PDF, PNG, JPG) untuk pengarsipan permanen (`HRD-DEC-056`).
   - Panel tindakan alur kerja: Ajukan Persetujuan, Setujui & Terapkan SK, Tolak SK (dengan modal input alasan wajib), dan Batalkan SK.
   - Penegakan prinsip *Segregation of Duties* (`HRD-DEC-031`): Pembuat draf SK dilarang menyetujui dokumen buatannya sendiri.

---

## 1. Bukti Pengujian Unit

```text
✔ FE-HRD-024: Formulir Pembuatan & Detail SK Kepegawaian mematuhi HRD-DEC-031 anti self-approval (0.442ms)
```
Status: **PASS (100%)**

---

## Pengerjaan Ulang — `PLAN-REPAIR-HRD-002` (3 November 2026)

| Field | Nilai |
| --- | --- |
| Pemicu | Laporan pemilik 03-11-2026 ("banyak tampilan yang belum ada CSS") dan laporan live testing `LBT-WFC-01`, `-19`, `-20`, `-23`, `-24`, `-26` |
| Issue / rencana | [issue-002](../../../docs/issue/issue-002-tampilan-workforce-core-tanpa-gaya.md) · [plan-repair-002](../../../docs/plan-repair/plan-repair-002-tampilan-workforce-core-tanpa-gaya.md) — `FIX-HRD-002-01`, `-03`, `-04` |
| Wewenang | Instruksi pemilik 03-11-2026 (K-01 pada issue-002) |
| Branch frontend | `HamzahV2`, basis `66b7e5600`; perubahan belum di-commit |

### Keadaan yang ditemukan

Form Buat SK dan halaman Detail SK ditulis dengan 34 dan 81 `className` bergaya Tailwind. Tailwind
tidak terpasang, sehingga kedua halaman tampil polos. Form meminta petugas mengetik UUID pegawai,
unit, departemen, jabatan, dan atasan. Lencana status dikirim dengan label sebagai `status`, dan
tombol sekunder memakai `variant="outline"` yang tidak dikenal `BaseButton`. Dialog Tolak/Batal dibuat
sendiri dan mengirim `rejectionReason`/`cancellationReason`, padahal backend membaca `reason`
(HTTP 500 pada uji live). Laporan task ini sebelumnya tidak memuat tabel keputusan base component,
baris `UI GATE`, maupun uji manual.

### Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx` | Ditulis ulang di atas `BaseGroupedEditorView`: tiga kelompok, pilihan bercari, validasi layar |
| `src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx` | Ditulis ulang di atas `BaseDetailView` + `BaseDetailSection`; `ConfirmModal` untuk setiap aksi; payload Tolak/Batal `{ reason }`; daftar lampiran + Unduh; Ganti Berkas selama Draf |
| `src/lib/services/hr/personnel-action-order-service.js` | `downloadAttachment` (blob) |
| `src/lib/hooks/select/hr/hr-select-resources.js` | Resource `workforceProfiles` bernilai `workforceProfileId` di atas endpoint opsi karyawan |
| `src/utils/hr/workforce-core/workforce-core-display-utils.js` (baru) | Format tanggal/ukuran berkas, peta status → `StatusBadge`, pesan galat Bahasa Indonesia, unduh blob |
| `src/style/hr/workforce-core/workforce-core.module.css` (baru) | CSS Module domain berbasis token (rujukan `exit-clearance.module.css`) |

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/exit-clearance/exit-clearance-detail-view.jsx` (tata
letak kartu), `src/components/view/hr/master-data/employees/form/employee-form-view.jsx` (editor bergrup).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Halaman Buat SK | `BaseGroupedEditorView` (`showPreview={false}`) | `REUSE` | Editor standar Quilvian |
| Isian pegawai & atasan | `BaseGroupedEditorField` select + resource `workforceProfiles` | `EXTEND` (resource baru di registri) | Endpoint opsi karyawan sudah mengirim `workforceProfileId`; pola `NURSE_SELECT_RESOURCE` Hemodialisa |
| Isian unit, departemen, jabatan | Resource `organizationUnits`, `departments`, `positions` | `REUSE` | Jabatan tersaring departemen lewat `field.filters` |
| Halaman Detail SK | `BaseDetailView`, `BaseDetailCard` (`detailRows`) | `REUSE` | Pola detail standar |
| Seksi alasan, berkas, jejak | `BaseDetailSection` + CSS Module domain | `COMPOSE` | Isi seksi memakai kelas token baru |
| Tolak / Batal / Ajukan / Setujui | `ConfirmModal` (`requireReason`, `maxReasonLength` 500) | `REUSE` | Menggantikan overlay buatan sendiri |
| Pemilih berkas scan | `BaseFormControl` + `BaseButton as="label"` + input `visually-hidden` | `COMPOSE` | Belum ada base component unggah berkas; teks bawaan peramban berbahasa Inggris |
| Lencana status & lampiran | `StatusBadge status=<kunci> label=<teks>` | `REUSE` | — |

`UI GATE: PASS — 6 elemen REUSE, 2 COMPOSE, 1 EXTEND (resource registri, aditif). Tidak ada NEW komponen; perintah pemilik 03-11-2026.`

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view Administrasi Kepegawaian>
OK — 7 berkas diperiksa, 0 temuan.
$ grep anti-regresi #1, #3–#8, #10 (checklist G)  →  0 baris
$ grep #9  →  1 input type="file" (dibungkus BaseFormControl, alasan di atas) + 3 baris komentar
$ grep #2 pada workforce-core.module.css  →  21 baris, seluruhnya selector lokal bertoken
```

| Layar | Screenshot (`QuilvianSystemFrontendDev/test-with-agy/screenshots/workforce-core-1103/plan-repair-002/`) | Catatan |
| --- | --- | --- |
| Buat SK | `03-buat-sk.png`, `04-buat-sk-validasi-kosong.png`, `05-buat-sk-cari-pegawai.png` | Setara form Master Data Karyawan |
| Detail SK (data nyata, Dibatalkan) | `06-detail-sk-nyata.png`, ponsel `21-ponsel-detail-sk.png` | Tanpa geser horizontal di 390 px |
| Detail SK Draf / Menunggu / Ditolak (respons dicegat) | `07-…`, `08-…`, `09-dialog-tolak-kosong.png`, `10-detail-sk-ditolak.png`, `22`–`25` | Pemilih berkas berbahasa Indonesia, ikon dialog Tolak |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet src/components/view/hr/workforce-core …` | 0 error | `PASS` |
| `npx eslint` (tanpa `--quiet`) pada berkas yang diubah | 5 peringatan `react-hooks/set-state-in-effect`; versi HEAD membawa 4 peringatan yang sama, layar rujukan juga | `EXISTING` |
| Test unit `workforce-core-target-state` + `workforce-core-cross-employee` | 22/22 lulus | `PASS` |
| Seluruh test unit frontend | 4.126 lulus, 55 gagal; seluruh 55 sudah gagal pada HEAD (dibuktikan di worktree HEAD dan dengan `menu-items.jsx` HEAD) | `EXISTING` |
| `npm run build` | "Compiled successfully"; rute `/hr/workforce-core/personnel-actions`, `/create`, `/[id]` terbangun | `PASS` |
| Form kosong → Simpan | Pesan di bawah isian, tidak ada `POST` | `PASS` |
| Cari pegawai "Les" | Opsi "Lestari Wulandari" | `PASS` |
| Tolak SK | Tombol konfirmasi mati selama alasan kosong; payload `{"reason":"…"}` | `PASS` |
| Unduh lampiran | Berkas `sk-mutasi-ttd.pdf` terunduh | `PASS` |

Uji manual: `PASS` — dijalankan di Chrome headed pada build produksi port 3001 (dev server port 3000
gagal membangkitkan path dinamis karena pool worker-nya mati setelah berjalan lama; bukan dari kode,
dibuktikan oleh `npm run build`). Keadaan Draf/Menunggu/Ditolak disajikan lewat respons API yang dicegat
di peramban, sehingga tidak ada data yang ditulis ke database.

### Status task

✅ tetap berlaku. Seluruh acceptance criteria tampilan dan aksi persetujuan kini terbukti di layar.
Keterbatasan yang tersisa: pilihan pegawai hanya memuat pegawai yang punya data karyawan (27 dari 55
profil; dokter belum termasuk) karena backend belum punya endpoint opsi profil tenaga kerja
(`ISS-HRD-002-T7`).
