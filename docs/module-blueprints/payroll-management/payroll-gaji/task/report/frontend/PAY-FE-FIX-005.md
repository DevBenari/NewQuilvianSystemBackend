# Laporan Perubahan Frontend — `PAY-FE-FIX-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-005` (ad-hoc, bukan kartu roadmap) |
| Judul | Layar kebijakan regulasi berversi (tarif PPh, iuran BPJS), persetujuan nominal PPh manual, klasifikasi tunjangan untuk dasar upah BPJS, jejak kebijakan pada Rincian Penghasilan, dan slip gaji satu jalur resmi |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas keputusan final pemilik 2026-11-18; roadmap dan traceability tidak disentuh (preseden `PAY-FE-FIX-001..004`) |
| Trace | "Keputusan Final — Penyempurnaan PPh, BPJS, Hak Akses, dan Jalur Payroll"; branch Frontend `yasmina` (satu-satunya branch yang ada; tidak berpindah branch) |
| Dependency | Backend `PAY-BE-FIX-005` (source selesai, **belum dibuild**) dan DDL pada laporan Backend bagian 7 (**belum diterapkan**). Urutan rilis: DB → Backend → Frontend. Tanpa itu layar kebijakan menampilkan galat dari Backend (bukan data karangan) |
| Task mode | `FRONTEND` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-18 |
| Status | 🟡 **Source selesai; ESLint, pemeriksa CSS, dan uji unit lulus. `npm run build`, `npm run dev`, dan uji peramban NOT RUN** (dilarang). `VISUAL CHECK: NOT FEASIBLE` — layar belum pernah dilihat |

---

## 1. Yang dikerjakan

- **Payroll › Kebijakan Tarif PPh** (`/payroll/policies/tax`) dan **Kebijakan Iuran BPJS** (`/payroll/policies/bpjs`), satu komponen daftar bersama (`PayrollPolicyListView`, parameter `kind`). Memuat: pernyataan ketersediaan ("Menunggu Verifikasi Kebijakan" bila belum ada kebijakan yang disahkan hari ini), daftar versi (nomor versi, judul, dasar peraturan, masa berlaku, status, "Berlaku hari ini", alasan), filter status dan pencarian, paginasi dari Backend.
- **Aksi menurut status dan izin** (tombol disaring `usePermission` dan status; Backend memeriksa ulang semuanya): Lihat, Ubah (draf/ditolak), Ajukan, Setujui, Tolak (alasan wajib), Pensiunkan (alasan wajib), Buat Revisi, Hapus. Pengajuan dan pengesahan **tidak otomatis**: setiap aksi lewat dialog konfirmasi. Galat Backend (termasuk "pengaju tidak boleh mengesahkan sendiri") ditampilkan apa adanya.
- **Form kebijakan** (dialog lebar): identitas dan masa berlaku, parameter perhitungan, penerima bukan pegawai dan PPh 26, tiga tabel regulasi yang **ditempel HRD** (PTKP, TER, Pasal 17; satu baris per entri dipisah `;`, kolom dan contoh format ditampilkan, baris rusak dilaporkan per baris), atau (BPJS) kelompok risiko JKK + dasar, plafon, dan tarif. **Tidak ada nilai bawaan atau angka regulasi di layar**; form baru kosong. Mode Lihat baca saja, menampilkan daftar kekurangan yang akan menolak pengajuan. BPJS: tombol **Salin dari Pengaturan Penggajian** (draf, bukan kebijakan resmi; kelompok risiko JKK dikosongkan) dan Buat Revisi membuka dialog nomor versi dan masa berlaku, lalu langsung membuka draf hasilnya.
- **Profil Pajak pegawai** (`workforce-tax-config-editor.jsx`): skema otomatis kini dapat dipilih (opsi dari metadata Backend) dengan banner "Menunggu Verifikasi Kebijakan" bila belum ada kebijakan yang disahkan; field **Tarif P3B** dan dasarnya untuk PPh Pasal 26; **panel persetujuan nominal manual** (status Menunggu/Disetujui/Ditolak, catatan keputusan, tombol Setujui/Tolak hanya bila izin `ApproveManualAmount` + `ViewAmount`, bukan penetap, dan status menunggu; tolak wajib alasan). Keputusan memakai satu panggilan tulis bernama (`decideManualAmount`); simpan profil tetap lewat jalur Profil Pegawai yang sudah ada.
- **Master Tunjangan dan Kebijakan Tunjangan Transportasi**: dua isian baru **Tunjangan Tetap** dan **Masuk Dasar Upah BPJS** (pilihan tiga keadaan: Belum diklasifikasikan / Ya / Tidak; kosong dikirim sebagai `null`, bukan `false`), terpisah dari "Kena Pajak"; tampil juga pada halaman detail.
- **Rincian Penghasilan Payroll Run**: bagian Pajak memuat kebijakan tarif yang dipakai (versi dan dasar peraturan), status persetujuan nominal manual, bruto kena pajak, dan langkah hitung dari Backend (cara hitung, kategori/tarif TER, PKP, PPh setahun) hanya bila hasilnya final; bagian Iuran memuat versi kebijakan beserta dasar, kelompok risiko JKK, dan **daftar dasar upah per komponen** (masuk/tidak/belum diklasifikasikan beserta alasan). Layar tidak menghitung apa pun.
- **Kelengkapan Data Payroll**: catatan peringatan terpisah bila kebijakan PPh atau BPJS belum disahkan, di samping catatan konfigurasi per orang.
- **Pengaturan Penggajian (layar HR lama)**: nilai bawaan BPJS tertanam (JKK 0,89%, plafon, tarif) dihapus dari layar; banner "Menunggu Verifikasi Kebijakan" dari Backend dan klaim "Plafon regulasi resmi ... 2026" dihapus. Layar itu **tidak punya route maupun menu** (kode lama yatim); perubahan hanya agar tidak menyesatkan bila dipasang kembali.
- **Slip gaji satu jalur resmi**: `/ess/payslips` dan `/hr/self-service/payslips` kini **dialihkan** ke `/payroll/my-payslips` (jalur resmi yang membaca slip terbitan dan snapshot Backend dan menegakkan izin `MyPayslip`); tampilan slip statis lama (`payslips-ess-view.jsx`, data `INITIAL_PAYSLIPS`) **dihapus**; menu ESS "Slip Gaji Mandiri" menunjuk jalur resmi dengan izin `MyPayslip: Read`.
- **Akses Role**: tidak ada perubahan Frontend; aksi baru muncul otomatis dari atribut Backend setelah sinkronisasi.

## 2. File

Baru: `src/app/payroll/policies/tax/page.jsx`, `.../bpjs/page.jsx`; `src/components/view/payroll/policy/payroll-policy-{list-view,form-modal,table-columns,version-prompt}.jsx`; `src/lib/constants/payroll/payroll-policy-constants.js`; `src/lib/services/payroll/payroll-policy-service.js`; `src/lib/hooks/payroll/use-payroll-policy-{list,metadata}.js`; `src/utils/payroll/payroll-policy-utils.js`; `src/style/payroll/payroll-policy.module.css`; `tests/unit/payroll-policy.test.mjs`.

Diubah: `workforce-tax-config-editor.jsx`, `payroll-tax-config-{constants,service,utils}`, `payroll-run-{constants,utils}`, `payroll-run-employee-detail-{sections,utils}`, `payroll-data-readiness-{constants,utils}`, `allowance-type-{constants,utils}`, `transport-allowance-policy-{constants,utils}`, `payroll-settings-view.jsx`, `menu-items.jsx`, `src/app/ess/payslips/page.jsx`, `src/app/hr/self-service/payslips/page.jsx`, tes `payroll-tax-config` dan `payroll-data-readiness`.

Dihapus: `src/components/view/hr/payroll/payslips-ess-view.jsx` (data slip statis).

## 3. Tabel keputusan base component (`UI GATE`)

`UI GATE: 14 elemen — REUSE 14, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`.

| Kebutuhan UI | Base | Status | Sumber gaya |
| --- | --- | --- | --- |
| Gerbang akses, hero, banner, daftar | `AccessDeniedGate`, `Hero`, `InformationAlert`, `DataFilter`, `FilterSelect`, `DataTable`, `Pagination`, `ToastStack` | REUSE | bawaan + `administrator-region-table.module.css` / `base-data-components.module.css` (sama dengan Kelengkapan Data) |
| Status | `StatusBadge` + konfigurasi badge | REUSE | bawaan |
| Tombol aksi | `BaseButton` (`ghost`, `secondary`, `primary`, `success`, `warning`, `danger`) | REUSE | bawaan |
| Dialog form, versi, konfirmasi | `ConfirmModal` (`hideFooter`, `requireReason`) | REUSE | CSS Module domain (urutan 3) `payroll-policy.module.css`: `modal-confirm.css` mengunci 430px; pola `.modal.modal` sama dengan dialog Rincian Penghasilan |
| Isian | `BaseTextField`, `BaseTextAreaField`, `BaseDateField`, `BaseSelectField` (`menuPortal`), `BaseCheckboxField` | REUSE | bawaan |

Tidak ada keputusan `NEW`/`EXTEND` yang menunggu persetujuan.

## 4. Bagian 3.4 — pemeriksaan UI

| Pemeriksaan | Hasil |
| --- | --- |
| `check-css-usage.mjs` pada 6 berkas JSX baru/diubah | `OK — 6 berkas diperiksa, 0 temuan.` |
| Grep anti-regresi (literal warna, `!important`, `style={{`) pada CSS Module dan JSX kebijakan | 0 temuan |
| ESLint pada seluruh berkas yang diubah/baru | 0 galat; 1 peringatan `react-hooks/set-state-in-effect` pada efek lama `payroll-settings-view.jsx` (tidak ditulis task ini) |
| Screenshot | **Tidak ada.** `VISUAL CHECK: NOT FEASIBLE` — `npm run dev` dilarang |
| `MANUAL TEST` | **NOT FEASIBLE** — alasan sama |

## 5. Validasi

| Command | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-policy.test.mjs tests/unit/payroll-tax-config.test.mjs tests/unit/payroll-data-readiness.test.mjs tests/unit/workforce-tax-profile-tab.test.mjs tests/unit/payroll-run-employee-detail.test.mjs tests/unit/payroll-run-screens.test.mjs` | **PASS** — 89 uji sebelum perbaikan satu uji lama; dua berkas yang diubah dijalankan ulang: 30 dari 30 lulus. Uji baru `payroll-policy`: 12 uji (parsing tabel, form kosong tanpa nilai bawaan, payload, aksi menurut status, normalisasi, kontrak route/izin/aksi/DTO dengan **source Backend**, pengesah ≠ pengaju, hanya kebijakan APPROVED dibaca mesin, tidak ada tarif/rumus/angka regulasi tertanam, halaman/menu/pengalihan slip) |
| Suite unit penuh (Git Bash) | 5.120 uji, 5.068 lulus, **52 gagal** — jumlah sama dengan baseline sebelum task (Billing, RWI, IGD, HMD, HRD lifecycle, menu master data, dst.); alasan yang dicek (`FIX-HRD-001-03/04`, `MD-01`, `MD-04`, `ESS-004`) tidak menyentuh berkas Payroll atau yang diubah task ini. **0 kegagalan baru** |
| Penyesuaian uji lama | `payroll-tax-config`: asersi "Backend menolak skema otomatis" diganti menjadi "skema otomatis hanya dari kebijakan berversi"; `payroll-data-readiness`: bentuk ringkasan membawa dua field ketersediaan kebijakan |
| `npm run build` / `npm run dev` / uji peramban | **NOT RUN** |

## 6. Risiko, delta, dan dependency

- **Tampilan belum pernah dilihat**: dialog form kebijakan (panjang, tiga textarea tabel), tabel dengan sel aksi, dan panel persetujuan pada drawer Profil Pajak perlu dilihat pemilik.
- **Daftar Profil Pajak tidak otomatis menyegarkan** setelah keputusan nominal manual diambil dari drawer; panel menampilkan status baru dari jawaban Backend, tabel di belakangnya baru terbarui saat dimuat ulang (aksi workforce generik di dashboard belum menyediakan jalur muat ulang bagi aksi bernama).
- **Menempel tabel regulasi** adalah cara input yang dipilih (ratusan baris TER tidak praktis diketik per baris). Kesalahan format dilaporkan per baris; kebenaran angkanya dijaga verifikasi HRD dan pengesahan Finance, bukan Frontend.
- **Konsumen layar HR payroll lama** (`components/view/hr/payroll/*`, `lib/services/hr/payroll-service.js`) masih ada sebagai kode yatim tanpa route; selain `payslips-ess-view.jsx` tidak dihapus. Penghapusan penuh = keputusan terpisah.
- **Izin Akses Role** yang harus diberikan agar layar berfungsi: `PayrollTaxPolicy`/`PayrollBpjsPolicy` (Read untuk melihat; Create/Update/Submit untuk HRD; Approve/Reject/Retire untuk Finance), `WorkforceTaxProfile:ApproveManualAmount` (+ `ViewAmount`), `MyPayslip:Read` untuk slip pegawai.
- `git status`: tidak ada yang di-stage atau di-commit; branch aktif `yasmina`.
