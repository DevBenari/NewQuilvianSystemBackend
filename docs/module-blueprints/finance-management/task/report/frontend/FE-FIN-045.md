# Laporan Perubahan Frontend — `FE-FIN-045`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-045` |
| Judul | HR: enrollment benefit dan verifikasi keluarga |
| Slice | Slice A — Enrollment & Tanggungan Benefit (`REV-19`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` |
| Trace | `FR-FIN-242`..`244`, `FIN-DEC-201`, `202`, `204` |
| Kontrak | `FIN-API-1.10` R.1 & R.2 (approved sisi Finance dan HR) |
| Wewenang UI | `03-frontend-architecture.md` §24.1–§24.7 (`DEV_DISCRETION` untuk rute, komposisi, dan tata letak) |
| Dependency | `BE-FIN-103` ✅, `BE-FIN-104` ✅ |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (constants, service, slice, hooks, table-columns, views, pages, menu sidebar) |
| Tanggal | 14 Oktober 2026 |
| Status | ✅ **Selesai** |

---

## 1. Ringkasan Implementasi

Task `FE-FIN-045` menyediakan antarmuka bagi Petugas HR untuk:
1. Mendaftarkan pegawai ke program benefit rumah sakit (`Draft`).
2. Melakukan transisi status siklus hidup enrollment: `Ajukan` (`Submit`), `Setujui` (`Approve`), `Aktifkan` (`Activate`), dan `Batalkan` (`Cancel`) dengan alasan wajib.
3. Memantau saldo plafon per orang (pegawai sendiri dan seluruh tanggungan yang terdaftar) dengan angka rupiah dan sisa plafon yang dihitung serta disajikan persis dari backend.
4. Mendaftarkan anggota keluarga dari data HR aktif sebagai tanggungan benefit pegawai.
5. Melakukan aksi verifikasi pada tanggungan:
   - **Verifikasi**: menetapkan batas plafon, masa berlaku (mulai & selesai), dan catatan verifikasi. Tombol verifikasi hanya aktif untuk tanggungan berstatus `Requested` (belum terverifikasi).
   - **Tolak**: menolak pendaftaran tanggungan dengan alasan penolakan wajib.
   - **Akhiri**: mengakhiri keikutsertaan tanggungan dengan tanggal berakhir dan alasan wajib.
6. Memantau buku mutasi plafon (pergerakan saldo alokasi, pemakaian tagihan RS, pengembalian, dan penyesuaian) yang bersumber dari `GET .../movements` (`BE-FIN-104`).
7. Mendaftarkan butir menu "Enrollment Benefit Pegawai" pada sidebar menu grup Human Resource.

---

## 2. Gerbang Keputusan Base Component (UI Gate)

```text
UI GATE: 18 elemen — REUSE 14, EXTEND 0, COMPOSE 4, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base | Bukti | Status | Keputusan |
|---|---|---|---|---|
| Header halaman & aksi utama | `Hero` | `src/components/features/base-features/hero.jsx` | `REUSE` | Digunakan dengan judul, deskripsi, dan tombol aksi tambah enrollment |
| Ringkasan metrik status | `SummaryGrid` | `src/components/features/base-features/summary-grid.jsx` | `REUSE` | Menampilkan metrik status dari `GET /summary` |
| Filter pencarian & status | `DataFilter`, `FilterSelect` | `src/components/features/base-features/data-filter.jsx` | `REUSE` | Pencarian teks dan dropdown status enrollment |
| Tabel daftar enrollment | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | 8 kolom data enrollment dengan paginasi |
| Lencana status enrollment & tanggungan | `StatusBadge` | `src/components/features/base-features/status-badge.jsx` | `REUSE` | Pemetaan varian status sesuai Quilvian design system |
| Paginasi tabel | `Pagination` | `src/components/features/base-features/pagination.jsx` | `REUSE` | Paginasi server-side terhubung ke thunk Redux |
| Alert pesan galat/informasi | `InformationAlert` | `src/components/features/base-features/information-alert.jsx` | `REUSE` | Tampilan status galat dan opsi coba lagi |
| Toast notifikasi aksi | `ToastStack` | `src/components/features/base-features/toast-stack.jsx` | `REUSE` | Umpan balik transisi status dan aksi tanggungan |
| Header detail enrollment | `Hero`, `BaseButton` | `src/components/features/base-features/hero.jsx` | `REUSE` | Menampilkan breadcrumb, status badge, dan tombol transisi siklus hidup |
| Ringkasan profil benefit & saldo | `BaseDetailSection` / Overview Grid | Scoped CSS Module berbasis token | `REUSE` | Rincian plan, batas plafon, terpakai, dan sisa plafon |
| Tab navigasi (Tanggungan, Mutasi, Riwayat) | Tab Bar berbasis token | `src/style/administrator/region/administrator-region-tokens.module.css` | `COMPOSE` | Tab button segmen berbasis token tanpa library eksternal |
| Tabel daftar tanggungan | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | Kolom nama, hubungan, status, batas, terpakai, sisa plafon, dan aksi |
| Modal pendaftaran tanggungan baru | `ConfirmModal`, Form | `src/components/features/base-features/confirm-modal.jsx` | `COMPOSE` | Pemilihan anggota keluarga aktif dari data HR |
| Modal verifikasi tanggungan | `ConfirmModal`, Form | `src/components/features/base-features/confirm-modal.jsx` | `COMPOSE` | Input plafon, tanggal berlaku, dan catatan verifikasi |
| Modal penolakan / pengakhiran tanggungan | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | Modal konfirmasi dengan input alasan wajib (`requireReason`) |
| Modal pembatalan enrollment | `ConfirmModal` | `src/components/features/base-features/confirm-modal.jsx` | `REUSE` | Konfirmasi pembatalan dengan alasan wajib |
| Tabel buku mutasi plafon | `DataTable` | `src/components/features/base-features/data-table.jsx` | `REUSE` | Ledger pergerakan saldo plafon dan invoice terkait |
| Modal tambah enrollment draft baru | `ConfirmModal`, Form | `src/components/features/base-features/confirm-modal.jsx` | `COMPOSE` | Pencarian pegawai dan pemilihan program benefit |

---

## 3. Berkas yang Dibuat dan Diubah

### Berkas Baru
1. `src/lib/constants/hr/benefit/employee-benefit-enrollment-constants.js` — Endpoint, status option, badge map, dan label relasi keluarga.
2. `src/lib/services/hr/employee-benefit-enrollment-service.js` — Service panggilan HTTP Axios (`InstanceAxios`) untuk enrollment, tanggungan, pergerakan plafon, dan lookups.
3. `src/lib/state/slice/hr/benefit/employee-benefit-enrollment-slice.js` — Redux Toolkit slice dan async thunks untuk seluruh endpoint `BE-FIN-103` dan `BE-FIN-104`.
4. `src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-list.js` — Custom hook untuk pengelolaan filter, paginasi, ringkasan, dan modal create draft.
5. `src/lib/hooks/hr/benefit/use-employee-benefit-enrollment-detail.js` — Custom hook untuk detail enrollment, transisi status, manajemen tanggungan, dan buku mutasi.
6. `src/components/view/hr/benefit/enrollment-table-columns.jsx` — Definisi kolom tabel daftar enrollment.
7. `src/components/view/hr/benefit/dependent-table-columns.jsx` — Definisi kolom tabel orang/tanggungan beserta kontrol aksi verifikasi.
8. `src/components/view/hr/benefit/movement-table-columns.jsx` — Definisi kolom tabel buku mutasi plafon.
9. `src/style/hr/benefit/employee-benefit-enrollment.module.css` — Stylesheet CSS Module berbasis token Quilvian.
10. `src/components/view/hr/benefit/employee-benefit-enrollment-list-view.jsx` — Tampilan halaman list (`FE-FIN-045-A`).
11. `src/components/view/hr/benefit/employee-benefit-enrollment-detail-view.jsx` — Tampilan halaman detail (`FE-FIN-045-B`).
12. `src/app/hr/benefit/enrollments/page.jsx` — App Router entry point untuk daftar enrollment.
13. `src/app/hr/benefit/enrollments/[id]/page.jsx` — App Router entry point untuk detail enrollment.

### Berkas yang Diubah
1. `src/lib/state/store.jsx` — Pendaftaran reducer `employeeBenefitEnrollment`.
2. `src/utils/menu-sidebar/menu-items.jsx` — Pendaftaran butir menu `Enrollment Benefit Pegawai` (`/hr/benefit/enrollments`) di bawah grup `Sumber Daya Manusia`.

---

## 4. Pemenuhan Acceptance Criteria

| # | Acceptance Criteria | Bukti Implementasi |
|:--:|---|---|
| 1 | Sisa plafon dan status ditampilkan persis dari respons backend | `dependent-table-columns.jsx` dan `employee-benefit-enrollment-detail-view.jsx` membaca `item.remainingAmount`, `item.limitAmount`, dan `item.usedAmount` langsung dari DTO backend tanpa komputasi matematika di klien. |
| 2 | Tombol verifikasi hanya aktif untuk tanggungan yang belum terverifikasi | Pada `dependent-table-columns.jsx`, tombol `[Verifikasi]` dan `[Tolak]` hanya dirender saat `status === "requested"`. Tanggungan `verified` hanya menampilkan tombol `[Akhiri]`. |
| 3 | Anggota keluarga tidak aktif tidak bisa dipilih | Hook `use-employee-benefit-enrollment-detail.js` menyaring `availableFamilyMembers` dari daftar keluarga aktif dan mengecualikan anggota yang sudah terdaftar sebagai tanggungan aktif. |
| 4 | Status loading, kosong, galat, dan coba-lagi tersedia | Komponen `DataTable`, `InformationAlert`, spinner aksi, dan tombol coba-lagi diimplementasikan lengkap pada list dan detail view. |
| 5 | Menu entry terdaftar | Menu `Enrollment Benefit Pegawai` terdaftar di `menu-items.jsx` pada rute `/hr/benefit/enrollments`. |
