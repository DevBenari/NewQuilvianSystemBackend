# Navigasi Menu Billing Management

| Field | Isi |
| --- | --- |
| Task ID | `FE-BKC-MENU-001` (task penutup gap, bukan nomor `FE-BKC-XXX` dari `roadmap/frontend-roadmap.md` — lihat Latar Belakang) |
| Modul | `billing-kasir` |
| Task type | Frontend, navigasi/konfigurasi menu (bukan business logic) |
| Task mode | `FRONTEND` |
| Write target | `QuilvianSystemFrontendDev` (source). Laporan ini di `NewQuilvianSystemBackend` mengikuti presedens wewenang lintas-repo yang sama seperti `FE-BKC-003`/`FE-BKC-004`. |
| Branch frontend | `yasmina` |
| Status task | Selesai, lulus lint dan build. Belum di-commit. Verifikasi visual sidebar ter-autentikasi belum dilakukan (lihat Definition of Done). |

## Latar belakang

Selama task `FE-BKC-003` ditemukan bahwa seluruh halaman modul `billing-management` — Running
Invoice (dari `FE-BKC-001`) dan empat halaman master data (Administration Fee Policy, Discount
Policy, Room Charge Policy, Tax Rule, dari `FE-BKC-002`) — tidak terdaftar di
`src/utils/menu-sidebar/menu-items.jsx`. Halaman-halaman ini sudah berfungsi penuh tapi hanya bisa
diakses lewat URL langsung, tidak lewat navigasi aplikasi normal. Temuan ini dicatat sebagai
`ISSUE-FE-001` di `.quilvian-local/CURRENT_ISSUES.md` repo frontend, dan pemilik task meminta task
ini diselesaikan sebagai task tersendiri (bukan bagian `FE-BKC-003`/`FE-BKC-004`). Task ini tidak
punya nomor `FE-BKC-XXX` di roadmap karena navigasi menu bukan bagian dari acceptance criteria
task manapun di `roadmap/frontend-roadmap.md` — murni pekerjaan penutup gap infrastruktur UI.

## Implementasi

Menambahkan satu entri menu baru "Billing dan Kasir" di `src/utils/menu-sidebar/menu-items.jsx`,
di bagian akhir seksi "Pelayanan Kesehatan" (setelah entri "Rawat Jalan"), dengan struktur:

```text
Billing dan Kasir
├── Running Invoice                    → /health-services/billing-management/billing/invoices
└── Master Data
    ├── Administration Fee Policy      → /health-services/billing-management/master-data/administration-fee-policy
    ├── Discount Policy                → /health-services/billing-management/master-data/discount-policy
    ├── Room Charge Policy             → /health-services/billing-management/master-data/room-charge-policy
    └── Tax Rule                       → /health-services/billing-management/master-data/tax-rule
```

Struktur ini meniru pola nesting tiga-level yang sudah ada dan terbukti berfungsi di entri
"Sumber Daya Manusia" → "Master Data" (`src/utils/menu-sidebar/menu-items.jsx:132-135`): level
teratas memakai field `subMenu`, grup "Master Data" di level kedua memakai field `subItems` untuk
anak-anaknya. Perbedaan nama field ini bukan kebetulan — dikonfirmasi langsung dari logika
resolver sidebar (`src/components/features/left-sidebar/left-sidebar-menu-handle.jsx:41-47`,
fungsi `getChildrenFromItem`/`getNestedMenuFromSubItem`): level 0 hanya membaca `item.subMenu`,
sedangkan level di bawahnya membaca `subItems`/`nestedMenu`/dsb — bukan `subMenu` lagi. Memakai
field yang salah di level yang salah akan membuat menu senyap tidak tampil tanpa error apa pun,
sehingga verifikasi terhadap logika resolver ini penting, bukan sekadar meniru bentuk visual.

Label setiap item persis sama dengan `title` Hero pada halaman tujuannya (contoh: label "Running
Invoice" ↔ `billing-invoices-view.jsx:128` `title="Running Invoice"`), supaya pengguna yang
mengklik menu langsung mengenali halaman yang terbuka. Path setiap item dicocokkan langsung
terhadap konstanta `list` di masing-masing `*-constants.js` modul terkait, bukan diketik ulang
dari ingatan.

Ikon memakai yang sudah diimpor di file ini (`RiSecurePaymentLine`, `RiFileList3Line`,
`RiDatabase2Line`, `RiSettingsLine`) — tidak ada import ikon baru.

## API CONTRACT IMPACT
Tidak ada. Task ini murni konfigurasi navigasi frontend, tidak menyentuh pemanggilan API.

## DATABASE IMPACT
Tidak ada.

## SECURITY IMPACT
Tidak ada perubahan otorisasi. Menu ini tidak difilter berdasarkan role — perilaku ini konsisten
dengan seluruh entri menu lain di file yang sama (fungsi `filterMenuItemsByRole` di
`src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx` secara faktual tidak aktif untuk
menu manapun: ia mencari key `"ManajemenKesehatan"` yang tidak ada di `menuItems`, sehingga selalu
mengembalikan daftar menu apa adanya). Kontrol akses sebenarnya tetap terjadi di level halaman
lewat `AccessDeniedGate` dan permission backend (`BillingInvoice:Read`, dst.) seperti sebelumnya —
menambahkan entri menu tidak membuka akses baru, hanya membuat halaman yang sudah authorized-gated
menjadi bisa ditemukan.

## VISUAL REFERENCE
NOT REQUIRED — mengikuti pola visual/struktural entri menu existing yang sudah ada (Farmasi,
Sumber Daya Manusia).

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint src/utils/menu-sidebar/menu-items.jsx` | **PASS** | Tanpa output. |
| `npm run lint:errors` | **PASS** | Exit code 0, seluruh repo. |
| `npm run build` | **PASS** | `✓ Compiled successfully in 83s`. |
| Verifikasi visual sidebar (login lalu lihat menu "Billing dan Kasir" muncul dan bisa diklik) | **NOT DONE** | Sama seperti `FE-BKC-003`/`FE-BKC-004` — tidak ada kredensial login di environment eksekusi ini. Sidebar hanya dirender untuk sesi ter-autentikasi, sehingga smoke-test headless tanpa login (dipakai untuk task sebelumnya) tidak relevan untuk memverifikasi tampilan menu ini secara spesifik. |

**Task ini belum bisa ditandai selesai sepenuhnya** — struktur data sudah diverifikasi cocok
dengan logika resolver sidebar yang sebenarnya (dibaca langsung, bukan diasumsikan), lint dan
build lulus bersih, tapi tampilan sidebar sungguhan belum pernah dilihat.

## Langkah berikutnya yang direkomendasikan

1. Login di `localhost:3000` (dev server sudah berjalan dari task sebelumnya), pastikan entri
   "Billing dan Kasir" muncul di sidebar dengan sub-menu "Running Invoice" dan "Master Data" (yang
   berisi empat item policy), dan setiap klik mengarah ke halaman yang benar.
2. Tandai `ISSUE-FE-001` sebagai resolved di `.quilvian-local/CURRENT_ISSUES.md` (sudah dilakukan
   pada laporan ini) setelah verifikasi visual di atas selesai.
