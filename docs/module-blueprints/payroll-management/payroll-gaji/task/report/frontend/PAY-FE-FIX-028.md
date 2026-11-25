# PAY-FE-FIX-028 — Detail Payroll Run: status, riwayat, kolom Aksi, nama komponen (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina` (dikonfirmasi pemilik pada sesi ini; `yasmin` tidak ada di GitHub), HEAD `b05275ec` (working tree memuat PAY-FE-FIX-025/026/027 yang belum di-commit). Backend: NewQuilvianSystemBackend `Yasmina`, **NO CHANGE REQUIRED**.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang; empat screenshot pengguna tidak terlampir pada pesan, sehingga analisis berdasarkan source). Status tugas UI: paling tinggi sebagian.

## Akar masalah (dari source)
| Revisi | Akar masalah |
| --- | --- |
| 1 Status | Dua tata letak berbeda ditumpuk: `div.statusRow` (flex dengan item selebar isinya, jarak antarkolom tidak merata) di atas `PayrollInfoGrid` (4 kolom sama lebar). Kolom atas dan bawah tidak sejajar. Tanggal kosong ditulis "Belum ada" |
| 2 Riwayat | Sudah memakai `DataTable` standar dan nomor urut dari `DataTable` (memperhitungkan halaman). Kekurangan nyata: galat dimuat tampil sebagai peringatan **di atas tabel yang tetap menampilkan "Belum ada riwayat status"** (galat dan kosong tercampur); pelaku kosong tampil "-" |
| 3 Aksi | Header `Aksi` diratakan tengah oleh `DataTable` (`align: "center"`), tetapi isi sel memakai `.actionButtons`, kelas **bilah aksi halaman** (`justify-content: flex-end`, dan di mobile `> * { width: 100% }`). Akibatnya tombol rata kanan dan bergeser terhadap header. Kelas `.rowActions` yang memang dibuat untuk sel aksi terpusat tidak dipakai |
| 4 Komponen | Satu renderer `buildComponentColumns` menulis `componentName` lalu `componentCode` sebagai baris kedua untuk semua kelompok |

## Perubahan (IMPLEMENTED di source, VERIFIED STATIC)
- **Status Payroll Run** (`payroll-run-detail-view.jsx`): satu `PayrollInfoGrid` berisi 8 informasi, urutan Status, Periode Penggajian, Rentang Periode, Tanggal Bayar / Dihitung, Diajukan, Disahkan, Versi Kebijakan Hitung. Grid memakai aturan responsif yang sudah ada (4 kolom, 2 kolom tablet, 1 kolom mobile, nilai membungkus). Badge status existing. Nilai kosong tampil "-".
- **Riwayat**: tetap `DataTable` standar dan sumber data endpoint `status-history`. Saat galat: `InformationAlert` (danger; warning bila 403) dengan "Coba Lagi" dan tabel tidak dirender, sehingga galat tidak lagi terlihat sebagai riwayat kosong. Loading dan kosong tetap bawaan `DataTable`. Pelaku kosong = "Tidak tercatat"; catatan panjang membungkus. Tidak ada baris palsu.
- **Aksi**: `payroll-run-table-columns.jsx` memakai `.rowActions` pada tabel pegawai dan tabel pembalikan (halaman yang sama); lebar minimum kolom 230 menjadi 200. Tanpa margin, transform, atau posisi mutlak. Handler `onViewDetail(item)` dan label tidak berubah.
- **Komponen**: kolom Komponen hanya merender nama lewat `getComponentDisplayName` (di `payroll-run-utils.js`). Nama kosong atau berupa kode teknis (pola `HURUF_KAPITAL_DENGAN_GARIS_BAWAH`) memakai "Komponen tanpa nama"; kode tidak pernah menjadi label utama. Kode tetap ada pada data dan API.

## Pemeriksaan backend (NO CHANGE REQUIRED)
`GetEmployeeDetailAsync` memetakan `ComponentCode = ComponentCodeSnapshot` dan `ComponentName = ComponentNameSnapshot`. Engine menulis nama ramah ("Gaji Pokok", "Tunjangan Transportasi", "Potongan Cicilan Piutang RS (angsuran ke-n)") dan kode pada kolom terpisah. `GetStatusHistoryAsync` membaca `PrlPayrollApproval` dan mengembalikan `OccurredAt`, `Action`, `Status`, `ActorName`, `Comments`. Riwayat kosong untuk run Calculated yang belum diajukan adalah perilaku benar. Tanpa perubahan data, snapshot, izin, atau lifecycle.

## File
| File | Perubahan |
| --- | --- |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | Status satu grid; riwayat galat vs kosong |
| `src/components/view/payroll/run/payroll-run-table-columns.jsx` | `.rowActions` (pegawai, pembalikan); kolom Komponen tanpa kode; fallback pelaku |
| `src/utils/payroll/payroll-run-utils.js` | `getComponentDisplayName`, `COMPONENT_NAME_FALLBACK` |
| `tests/unit/payroll-run-detail-layout.test.mjs` (baru) | 7 tes |

## UI GATE
Semua REUSE: `PayrollInfoGrid`, `StatusBadge`, `DataTable`, `InformationAlert`, `BaseButton`, kelas `.rowActions`/`.infoGrid` yang sudah ada. Tanpa CSS atau komponen baru.

## Validasi
`AUTOMATED TEST`: `payroll-*.test.mjs` dari root workspace: 542 tes, 539 lulus, 3 gagal lama (module-contract x2, run-screens x1; tidak dibandingkan dengan HEAD bersih); 7 tes baru lulus. ESLint: 0 error. `check-css-usage.mjs`: 0 temuan.

## Skenario (20) — statis
| # | Status |
| --- | --- |
| 1-6 | VERIFIED STATIC (badge, tanggal, "-", periode, rentang, versi kebijakan dalam satu grid); NEEDS RUNTIME UAT untuk tampilan |
| 7-8 | VERIFIED STATIC (empty state dan nomor urut dari `DataTable`); NEEDS RUNTIME UAT |
| 9-11 | VERIFIED STATIC (`.rowActions` terpusat, nama panjang membungkus, handler Rincian tidak berubah); NEEDS RUNTIME UAT |
| 12-15 | VERIFIED STATIC (tes perilaku `getComponentDisplayName` dan renderer tunggal untuk semua kelompok) |
| 16-17 | VERIFIED STATIC (nominal masih dari respons; tidak ada tulis ke data/snapshot) |
| 18 | NOT RUN (responsif butuh peramban; aturan breakpoint existing terverifikasi di CSS) |
| 19 | VERIFIED STATIC (loading, galat, kosong dibedakan) |
| 20 | VERIFIED STATIC (izin dan aksi lifecycle tidak disentuh) |

## Risiko dan butir terbuka
- Tampilan belum dilihat; perlu UAT visual terutama kolom Aksi dan grid Status.
- Tabel daftar Payroll Run (halaman daftar) memakai pola `.actionButtons` yang sama pada kolom Aksi-nya; **tidak diubah** (di luar cakupan), NEEDS CONFIRMATION bila ingin diselaraskan.
- Halaman Penghasilan Variabel masih menampilkan kode komponen sebagai subteks; di luar cakupan (revisi hanya untuk popup Rincian Pegawai).
- Label "Komponen tanpa nama" bersifat netral; nama baru tidak pernah menimpa snapshot.
