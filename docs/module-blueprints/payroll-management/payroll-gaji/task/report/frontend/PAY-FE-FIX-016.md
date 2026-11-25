# Laporan Perubahan Frontend — `PAY-FE-FIX-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-016` (ad-hoc lanjutan seri `PAY-FE-FIX`); pasangan `PAY-BE-FIX-016` |
| Judul | Layar Saldo Awal PPh (Opening Tax Balance) |
| Slice | Payroll Management → Kebijakan dan Kesiapan → Saldo Awal PPh |
| Roadmap | Tidak ada baris roadmap untuk seri ad-hoc (`NOT APPLICABLE`) |
| Contract version | Mengonsumsi `PayrollOpeningTaxBalancesController` (`api/v1/corporate/payroll-management/opening-tax-balances`) apa adanya; tidak ada payload tebakan |
| Wewenang UI | Permintaan pemilik 23 Nov 2026: "kerjakan layar frontend saldo awalnya". Semua elemen `REUSE` base component; tanpa CSS Module baru |
| Dependency | `PAY-BE-FIX-016` **belum dibuild** dan DDL `PrlOpeningTaxBalance` (laporan `PAY-BE-FIX-015` §9.1, termasuk patch kolom penolakan) harus sudah diterapkan pemilik. Tanpa tabel, backend menjawab `storageAvailable=false` dan layar menampilkan peringatan, bukan saldo nol |
| Klasifikasi | `HEAVY` — layar baru (route, view daftar, modal form, hook, service, util, konstanta, menu) |
| Task mode | `FRONTEND` |
| Commit frontend saat dikerjakan | `ab5843be7` (branch `yasmina`), working tree belum di-commit |
| Tanggal | 23 November 2026 |
| Status | 🟡 **Source, lint, pemeriksa CSS, dan tes teks selesai; tampilan belum dilihat.** `VISUAL CHECK: NOT FEASIBLE` (server dev dan build dilarang pada task ini) |

---

## 1. Keadaan yang ditemukan di awal

Backend sudah menyediakan CRUD dan alur ajukan/setujui/tolak/revisi untuk saldo awal, tetapi tidak ada layar. HRD tidak dapat mengisi riwayat PPh bulan-bulan sebelum cutover, sehingga rekonsiliasi PPh masa pajak terakhir tertahan.

## 2. Proses bisnis dari sisi pengguna

1. Menu **Payroll → Kebijakan dan Kesiapan → Saldo Awal PPh** (izin `PayrollOpeningTaxBalance : Read`).
2. Daftar dengan filter Tahun Pajak, Status, Sumber, dan jumlah baris. Nominal yang disembunyikan backend (tanpa `ViewAmount`) tampil sebagai teks, **bukan Rp 0**.
3. **Tambah** (tombol hanya bila `Create` **dan** `ViewAmount`): pilih pegawai dari daftar bercari (tanpa mengetik id), tahun pajak, sumber, **12 kotak centang bulan** (bulan tidak berurutan tetap bisa dipilih), empat nominal, dan dokumen sumber. Disimpan sebagai **draf**.
4. **Ubah** hanya untuk draf; pegawai, tahun, dan sumber terkunci. **Ajukan** mensyaratkan dokumen sumber.
5. **Setujui** oleh pengguna lain (backend menolak bila penyetuju = pengaju). **Tolak** wajib alasan dan **boleh oleh pengaju yang sama**; pembeda hanya izin role (keputusan pemilik 23 Nov 2026).
6. **Buat Revisi** dari saldo disetujui/ditolak membuat draf versi baru dan langsung membuka form ubah. **Hapus** hanya draf/ditolak.
7. Saldo `PREVIOUS_EMPLOYER` ditandai "disimpan sebagai arsip, belum dipakai mesin hitung".
8. Aksi di setiap baris ditentukan `item.actions` dari backend (status dan izin); backend tetap memeriksa ulang. Galat 400/403/409/422 ditampilkan apa adanya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas baru

| Berkas | Isi |
| --- | --- |
| `src/lib/constants/payroll/payroll-opening-tax-balance-constants.js` | Endpoint, resource, izin, status, sumber, bulan, teks Indonesia |
| `src/lib/services/payroll/payroll-opening-tax-balance-service.js` | Metadata, list, get, create, update, revise, submit, approve, reject, remove |
| `src/utils/payroll/payroll-opening-tax-balance-utils.js` | Normalisasi (nominal null tetap null), form, parser angka, validasi cermin, payload |
| `src/lib/hooks/payroll/use-payroll-opening-tax-balance.js` | Daftar, filter, editor, aksi, toast (tanpa Redux, mengikuti pola Payroll non-master) |
| `src/components/view/payroll/opening-tax-balance/payroll-opening-tax-balance-list-view.jsx` | Hero, peringatan, filter, tabel, dialog konfirmasi |
| `.../payroll-opening-tax-balance-form-modal.jsx` | Form tambah/ubah/lihat |
| `.../payroll-opening-tax-balance-table-columns.jsx` | Kolom dan aksi baris |
| `src/app/payroll/opening-tax-balances/page.jsx` | Route |
| `tests/unit/payroll-opening-tax-balance-screen.test.mjs` | 8 tes (kontrak, util, izin, penguncian) |

### 3.2 Berkas diubah

- `src/utils/menu-sidebar/menu-items.jsx`: entri "Saldo Awal PPh" setelah "Kebijakan Tarif PPh" (`requiredPermission PayrollOpeningTaxBalance : Read`).
- `tests/unit/payroll-opening-tax-balance.test.mjs`: tambahan asersi bahwa `RejectAsync` mengisi `RejectedAt`/`RejectedByUserId` dan tidak membandingkan dengan pengaju.

## 3.4 Gerbang UI

**Tabel keputusan base component** (semua `REUSE`, tidak ada `NEW`/`EXTEND`):

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Judul halaman | `REUSE` `Hero` | `payroll-policy-list-view.jsx` |
| Peringatan/catatan | `REUSE` `InformationAlert` | idem |
| Filter | `REUSE` `DataFilter` + `FilterSelect` | idem |
| Tabel + paginasi | `REUSE` `DataTable` + `Pagination` | idem |
| Badge status | `REUSE` `StatusBadge` dengan config modul | kolom kebijakan |
| Dialog konfirmasi (alasan wajib untuk Tolak) | `REUSE` `ConfirmModal` (`requireReason`) | idem |
| Form | `REUSE` `ConfirmModal` + `BaseTextField`/`BaseSelectField`/`BaseTextAreaField`/`BaseCheckboxField` | `salary-structure-allowance-form-modal.jsx` |
| Pemilih pegawai | `REUSE` `BaseSelectField` + `useSelectResource("workforceProfiles")` | pemakaian di modul Payroll lain |
| Grid 12 bulan | `REUSE` utilitas Bootstrap `row g-2` / `col-6 col-md-4` | ada di `bootstrap.min.css` |
| Toast, gerbang akses | `REUSE` `ToastStack`, `AccessDeniedGate` | idem |
| Gaya | (a) bawaan base + (b) `payroll-policy.module.css` dan `payroll-run.module.css` yang sudah ada; **tidak ada CSS Module baru** | — |

`UI GATE: PASS (semua REUSE)`.

**Pemeriksa CSS** (`node QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs` pada 3 berkas JSX):

```text
OK — 3 berkas diperiksa, 0 temuan.
```

**Grep anti-regresi** (class Tailwind `grid-cols|text-slate|rounded-md|w-full|space-y|items-center` pada berkas modul): 0 temuan.

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` pada seluruh berkas baru/diubah + `menu-items.jsx` | **0 temuan** |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-opening-tax-balance.test.mjs tests/unit/payroll-opening-tax-balance-screen.test.mjs` | **14 lulus, 0 gagal** (6 teks BE + tambahan Reject, 8 layar). Tes BE mencocokkan **teks** source, bukan eksekusi C#. Alamat dan nama resource FE dicocokkan dengan `[Route]`/`ControllerName` controller backend |
| `npm run build` / dev server | `NOT RUN` (dilarang) |
| `VISUAL CHECK` | `NOT FEASIBLE` — server dev/build dilarang; **tampilan belum dilihat**, termasuk grid kotak centang bulan, pemilih pegawai, dan modal di lebar sempit |
| `MANUAL TEST` | `NOT FEASIBLE` — backend belum dibuild dan DDL belum terbukti diterapkan |

## 5. Risiko dan yang belum terpenuhi

- Perilaku `BaseSelectField` dengan `useSelectResource("workforceProfiles")` (opsi bernama pegawai, pencarian, label tersimpan) belum dilihat; mengikuti pola modul lain.
- Tautan ke Konfigurasi PPh HR dan estimasi simulasi masa pajak terakhir belum dikerjakan.
- Keputusan terbuka dari backend: rentang bulan terhadap tanggal cutover, aturan `PREVIOUS_EMPLOYER`.
- Urutan deploy: DDL (termasuk patch penolakan bila tabel dibuat dengan versi lama) → backend → frontend.

## 6. Tugas berikutnya

Pemilik menjalankan DDL dan `dotnet build`; setelah itu `VISUAL CHECK` dan uji alur Tambah → Ajukan → Setujui (pengguna lain) → Run Desember pada data uji.
