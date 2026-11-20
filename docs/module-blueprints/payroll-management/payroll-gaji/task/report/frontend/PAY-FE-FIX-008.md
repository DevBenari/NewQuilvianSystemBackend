# Laporan Perubahan Frontend — `PAY-FE-FIX-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-008` (ad-hoc, bukan kartu roadmap; melanjutkan seri `PAY-FE-FIX-001..007`) |
| Judul | Hybrid Allowance Management di layar Payroll: status persetujuan penetapan gaji, alasan tunjangan belum dibayarkan, paginasi nyata, perlindungan tunjangan struktur terpakai, dan indikator run perlu hitung ulang |
| Roadmap | **NOT APPLICABLE** (task ad-hoc atas master prompt pemilik 2026-11-20; preseden `PAY-FE-FIX-001..007`) |
| Dependency | `PAY-BE-FIX-008` (kontrak aditif, source selesai, **belum dibuild**). Tanpa BE baru, semua field baru bernilai default dan layar tetap berfungsi seperti sebelumnya (lihat bagian 5) |
| Task mode | `FRONTEND` (backend read-only sumber kontrak) |
| Branch / HEAD | `yasmina` / `9629df782336985fb10e70e7fe12a320d75d516e` (cocok snapshot). **Working tree sudah kotor sebelum task** (BPJS Payroll dan lain-lain); file di bagian 3 adalah yang disentuh task ini |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-20 |
| Status | 🟡 **Source selesai; lint dan pemeriksa CSS bersih; `VISUAL CHECK: NOT FEASIBLE`; build/dev dilarang task.** Satu butir (editor penetapan gaji HRD) tidak diubah dan dilaporkan |

## 1. Yang berubah bagi pengguna

| Layar | Sebelum | Sesudah |
| --- | --- | --- |
| Payroll > Penetapan Gaji (rekap) | Hanya kolom "Status: Aktif/Nonaktif"; penetapan aktif yang belum disetujui tidak dapat dibedakan | Kolom **Persetujuan** (Disetujui / Menunggu Persetujuan / Belum Disetujui) dengan tanggal persetujuan; kolom lama menjadi "Status Aktif" |
| Profil Penggajian > Riwayat Penetapan Gaji | Struktur, masa berlaku, gaji pokok, utama, aktif | Ditambah **Persetujuan** dan **Perubahan dari Penetapan Sebelumnya**: struktur, grade, dan gaji pokok sebelum → sesudah (nominal hanya bila BE mengirimnya, yaitu pemegang `ViewAmount`; selain itu "Tidak ditampilkan", bukan Rp 0) |
| Profil Penggajian > Tunjangan Pegawai | Satu halaman 100 baris, sisanya dipotong dengan peringatan "Hanya sebagian…" tanpa jalan melihatnya | **Paginasi sisi server** (komponen `Pagination` yang sama dengan rekap lain); alasan konkret "Belum dibayarkan: …" pada baris yang belum atau tidak dibayarkan |
| Struktur Gaji > Tunjangan Tetap | Edit/hapus langsung | Lencana **Terpakai payroll** / **Terpakai payroll yang disahkan**; mengubah nilai atau menghapus baris terpakai meminta **konfirmasi eksplisit** lebih dulu |
| Payroll Run > Detail | Tidak ada tanda hasil basi | Banner **"Payroll run perlu dihitung ulang"** beserta sumber yang berubah; Ajukan dan Sahkan ditolak BE selama masih basi |
| HRD > Penetapan gaji pegawai (editor) | Simpan selalu memunculkan toast sukses generik, sehingga pesan BE "persetujuan dibatalkan karena gaji berubah" dan penurunan penetapan utama lama tidak terlihat | Toast **"Perlu perhatian"** (tidak hilang otomatis) memuat pesan BE tersebut. **Pesan 409 (irisan, terkunci payroll final) sudah tampil** lewat jalur galat yang ada (diverifikasi statis, bagian 7) |
| HRD > Penetapan gaji, kolom Gaji Pokok | Tanpa `ViewAmount` nominal 0 tampil seperti nominal (kolom Rentang di sebelahnya sudah terselubung, kolom ini tidak) | "Terselubung" + "Memerlukan wewenang ViewAmount" |
| Struktur Gaji > Tunjangan Tetap (tanpa `ViewAmount`) | Nominal, persen, dan batas selalu tampil | "Tidak ditampilkan"; formulir ubah memberi catatan bahwa isian kosong bukan nol |

Contoh banner: "Sumber kompensasi berubah setelah perhitungan terakhir … Berubah: penetapan gaji, tunjangan Struktur Gaji."

## 2. Keputusan komponen (UI GATE)

Seluruh elemen memakai komponen yang sudah ada; **tidak ada `NEW`, `EXTEND`, atau `WRAP`.**

| Elemen | Status | Komponen / bukti |
| --- | --- | --- |
| Tabel + paginasi sisi server | `REUSE` | `DataTable` dengan `pageNumber/pageSize/totalData/totalPage/onPageChange/PaginationComponent={Pagination}`, pola identik dengan `payroll-salary-assignment-view.jsx` |
| Lencana status persetujuan / terpakai | `REUSE` | `StatusBadge` (`status` + `config`, `status="info"` + `label`) |
| Konfirmasi ubah baris terpakai, peringatan hapus | `REUSE` | `ConfirmModal` (varian `warning`), `InformationAlert` |
| Banner hitung ulang | `REUSE` | `InformationAlert variant="warning"` |
| Sumber gaya | (a) bawaan base component; (b) CSS Module yang sudah dipakai berkas tersebut (`payroll-run.module.css`, `administrator-region-table.module.css`) | Tidak ada CSS baru, tidak ada nilai visual literal, tidak ada class Tailwind |

## 3. Berkas yang diubah (repository frontend)

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/payroll/use-payroll-workforce-allowance.js` | Nomor halaman per orang (turunan, tanpa setState di efek), `requestKey` pada permintaan berkunci, lompat ke halaman terakhir bila halaman yang diminta sudah tak ada |
| `src/components/view/payroll/compensation/payroll-workforce-allowance-section.jsx` | `DataTable` berpaginasi; peringatan "Hanya sebagian" dihapus |
| `src/components/view/payroll/compensation/payroll-workforce-allowance-table-columns.jsx` | Alasan `notPayableReason` pada sel Status |
| `src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx` | Kolom Persetujuan (rekap dan riwayat), kolom Perubahan (riwayat) |
| `src/components/view/payroll/compensation/payroll-compensation-detail-view.jsx`, `payroll-salary-assignment-view.jsx` | Peta perubahan; teks informasi tentang makna belum disetujui |
| `src/lib/hooks/payroll/use-payroll-salary-structure-allowance.js` | Alur konfirmasi dua tahap untuk baris terpakai; hapus mengirim konfirmasi |
| `src/components/view/payroll/master-data/salary-structure/detail/salary-structure-allowance-section.jsx`, `…-table-columns.jsx` | `ConfirmModal` konfirmasi, peringatan hapus, lencana |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | Banner hitung ulang |
| `src/utils/payroll/payroll-compensation-utils.js` | `getSalaryApprovalKey`, `buildSalaryChangeMap`, normalisasi `approvalStatus/approvalRequired` |
| `src/utils/payroll/payroll-workforce-allowance-utils.js`, `payroll-salary-structure-allowance-utils.js`, `payroll-run-utils.js` | Normalisasi field aditif; `hasAllowanceFinancialChange` (mencerminkan aturan BE); payload `acknowledgeUsedInPayroll` |
| `src/lib/services/payroll/payroll-salary-structure-allowance-service.js` | `remove(..., acknowledgeUsedInPayroll)` |
| `src/lib/hooks/hr/workforce-profile/use-wfp-entity-editor.jsx` (putaran 2) | Setelah simpan penetapan gaji, menampilkan `buildSalaryAssignmentSaveNotice(saveResult)` sebagai toast peringatan |
| `src/utils/hr/master-data/workforce/workforce-editor-utils.jsx` (putaran 2) | `buildSalaryAssignmentSaveNotice` (fungsi murni; bisa diuji tanpa Redux) |
| `src/components/features/workforce/workforce-profile-dashboard.jsx` (putaran 2) | Kolom Gaji Pokok menghormati `amountsVisible` (memakai `StackedTableCell` yang sudah dipakai kolom itu). Berkas ini sudah berubah di working tree sebelum task; hanya blok kolom itu yang saya sentuh |
| `src/utils/payroll/payroll-salary-structure-allowance-utils.js`, `…/salary-structure/detail/salary-structure-allowance-table-columns.jsx`, `…-form-modal.jsx` (putaran 2) | `amountsVisible`, nominal/persen `null` tetap `null`, `getAllowanceMaximumText`, catatan formulir |
| `src/lib/constants/payroll/payroll-constants.js`, `payroll-run-constants.js`, `payroll-workforce-allowance-constants.js`, `payroll-salary-structure-allowance-constants.js` | Konfigurasi badge dan teks; `truncatedNotice` diganti `notPayablePrefix`; teks hapus tunjangan struktur diluruskan (run yang dihitung tetapi belum disahkan perlu dihitung ulang) |
| `tests/unit/workforce-salary-assignment.test.mjs`, `tests/unit/payroll-allowance-policy-grade-sync.test.mjs`, `tests/unit/payroll-salary-structure-allowance.test.mjs` | Assertion yang mengunci perilaku lama (aktif = disetujui) diganti dengan aturan sakelar (bagian 6); putaran 2 menambah 3 tes: `baseSalary` null + kolom dashboard, pesan simpan penetapan gaji, nominal tunjangan struktur tersembunyi + izin `ViewAmount` |

## 4. Status, galat, dan izin

| Hal | Perilaku |
| --- | --- |
| Loading / kosong / galat / coba lagi | Dipertahankan; paginasi memakai keadaan loading yang sama |
| 400 / 403 / 409 | Pesan BE tampil apa adanya (jalur `toRequestError` yang sudah ada). 409 baru (irisan penetapan utama, terkunci payroll final, baris terpakai tanpa konfirmasi, run basi) memakai jalur itu |
| Izin | FE hanya menyesuaikan tampilan; keputusan hak tetap di BE. `ViewAmount`: nominal null tetap null dan ditampilkan sebagai "Tidak ditampilkan"; selisih nominal riwayat hanya dihitung bila kedua nilai dikirim |

## 5. Kompatibilitas API

| Konsumen | Kontrak BE | Perilaku bila BE lama |
| --- | --- | --- |
| Tunjangan Pegawai | `GET …/allowance-assignments?pageNumber&pageSize` (sudah berhalaman di BE); `notPayableReason` | Field tidak ada → `null`, kolom tidak menampilkan alasan |
| Riwayat/rekap penetapan | `approvalStatus`, `approvalRequired`, `isApproved` | `approvalRequired` kosong dianggap `true`; badge diturunkan dari `isApproved` |
| Tunjangan Struktur | `isUsedInPayroll`, `isUsedInFinalPayroll`, `acknowledgeUsedInPayroll` (PUT body, DELETE query) | Tanpa flag baris dianggap tidak terpakai |
| Payroll Run | `needsRecalculation`, `recalculationReasons` | Tanpa field banner tidak tampil |

Makna badge persetujuan mengikuti sakelar BE `RequireApprovedSalaryAssignment` (bawaan mati): bila **mati**, penetapan belum disetujui tetap dipakai payroll sehingga labelnya netral **"Belum Disetujui"** dengan keterangan "Belum disyaratkan Payroll"; bila **menyala**, labelnya **"Menunggu Persetujuan"**. Penetapan aktif tidak pernah ditampilkan sebagai "Disetujui".

## 6. Konflik dengan tes lama (keputusan pemilik: sakelar, bawaan mati)

Tes `workforce-salary-assignment.test.mjs` dan `payroll-allowance-policy-grade-sync.test.mjs` (commit `718e2c33c`, 19 Nov 2026) mengunci "peniadaan approval": larangan kolom `approval` pada riwayat, larangan `ApprovedAt` pada konsumen Payroll, dan `IsApproved = x.IsActive ||`. Itu berlawanan dengan brief; pemilik memutuskan memakai **sakelar** sehingga keputusan 19 Nov tetap berlaku selama sakelar mati. Assertion yang mengunci kode cacat diganti: kolom persetujuan wajib ada dan diturunkan dari persetujuan nyata; `IsApproved` dilarang berasal dari `IsActive`; sakelar wajib ada dengan bawaan `false`; `SalaryBasis` mengikuti sakelar. Satu tes baru ditambahkan untuk sakelar itu.

## 7. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npm run build`, `npm run dev`, server dev | **NOT RUN** (dilarang task) |
| ESLint, putaran 1: 21 berkas | **0 error, 1 warning** (`use-payroll-salary-structure-allowance.js`: `setLoading(true)` di dalam efek; kode lama yang tidak diubah → `EXISTING WARNING`) |
| ESLint, putaran 2: dashboard, hook editor, util editor, form modal tunjangan struktur | **0 error, 3 warning** di `workforce-profile-dashboard.jsx` baris 4940, 4978, 6379 (setState di efek, `<img>`); jauh dari blok yang saya ubah (±3672) → `EXISTING WARNING` |
| `check-css-usage.mjs` pada 8 JSX (putaran 1) | **OK — 8 berkas diperiksa, 0 temuan** (satu temuan awal `PROP-TIDAK-SAH` pada `StatusBadge status="used"` diperbaiki menjadi `status="info" label=…`) |
| `check-css-usage.mjs`, putaran 2 | **3 berkas payroll tunjangan struktur: 0 temuan.** `workforce-profile-dashboard.jsx` (berkas legacy ±10.000 baris): **50 temuan, semuanya kelas lama berkas itu** (`wfp-*`, `font-medium`, dan lainnya). Edit pertama saya menambah 1 temuan (menyalin `wfp-table-text` dari kolom tetangga); saya ganti dengan `StackedTableCell` sehingga edit saya tidak menambah temuan. Target "0 temuan" tidak tercapai untuk berkas legacy itu karena temuan lama → `UNRELATED EXISTING ISSUE` |
| Tes unit putaran 1 (9 berkas, `node --test` dari root workspace) | **161 lulus, 1 gagal**. Yang gagal: "menu dan rute: Payroll Run dapat dibuka…" (`payroll-run-screens.test.mjs`, `menu-items.jsx`), area yang tidak disentuh → `UNRELATED EXISTING ISSUE` |
| Tes unit putaran 2 (9 berkas: penetapan gaji, tunjangan struktur dan pegawai, kesiapan, profil, run, kebijakan, data payroll HRD; ditambah 3 tes baru) | **170 lulus, 0 gagal.** Tes kontrak yang mencocokkan `[AccessPermission]` BE dengan konstanta FE menangkap izin `ViewAmount` baru dan saya perbarui. Seluruh suite FE tidak dijalankan |
| Verifikasi statis jalur 409 di editor HRD | Alur: `workforce-resource-editor` → `onSubmit` → hook `use-wfp-entity-editor` → thunk `updateWfpResourceItem` → `rejectWithValue(getErrorPayload(error))` → `normalizeErrorMessage` membaca `response.data.errors/message` → `normalizeWfpErrorMessage` → `setLocalError` (alert di formulir) + toast bahaya yang tidak hilang otomatis. `ApiResponse.Fail(409, pesan)` BE sampai sebagai `message`. Ini penelusuran kode, **bukan** uji di browser |
| `VISUAL CHECK` | **NOT FEASIBLE**: dev server dilarang, sehingga tidak ada screenshot. Yang belum terlihat: dua modal bertumpuk pada Tunjangan Struktur (form + konfirmasi), lebar kolom baru di riwayat, dan penempatan lencana |
| `MANUAL TEST` | **NOT FEASIBLE** (alasan sama) |

## 8. Tidak dikerjakan dan risiko

1. **`workforce-resource-editor.jsx` (formulir penetapan gaji HRD, ±10.000 baris, sudah berubah di working tree sebelum task) tidak diubah.** Pesan 409 sudah tampil lewat hook penyimpanannya (bagian 7), dan pesan sukses BE kini ditampilkan (putaran 2). Yang tetap **tidak ada: tombol "Setujui penetapan gaji"** di FE; slice sudah punya aksi generik `approval` untuk penetapan gaji, tetapi tidak satu pun layar memanggilnya, dan endpoint `PATCH …/approval` di BE tidak punya layar. Ini prasyarat sebelum sakelar `RequireApprovedSalaryAssignment` dinyalakan. Formulir ubah bagi pengguna tanpa `ViewAmount` meminta nominal baru (nilai lama tidak dikirim BE); mengubah itu menjadi "pertahankan nilai lama" butuh perubahan kontrak BE.
2. Tidak ada perbandingan kompensasi sebelum/sesudah di Payroll Run: BE belum mengirim data pembandingnya, jadi tidak dibuat.
3. Tidak ada indikator overlap/perubahan grade per komponen di rincian run: kontrak BE belum menyediakannya.
4. Rekap Penetapan Gaji tidak menampilkan nominal (kontrak lintas-pegawai tidak mengirimnya); perbandingan nominal hanya di riwayat Profil Penggajian.
5. Tombol Ajukan/Sahkan di detail run belum dinonaktifkan oleh `needsRecalculation`; penolakan tetap di BE (409) dan banner menjelaskan sebabnya.
6. Berkas yang diubah task ini ikut tampil di `git status` bersama 81 perubahan lain yang sudah ada; tidak ada stage atau commit.

## 9. Status Git

Tidak ada stage, commit, push, pull, merge, atau perubahan remote. Perubahan sebelum task tidak dibuang atau ditimpa.
