# Bugfix — TDZ ReferenceError pada Menu Pembayaran (`FE-BKC-011`)

| Field | Isi |
| --- | --- |
| Task ID | Bugfix ad-hoc (dilaporkan user), terkait `FE-BKC-011` (Menu Pembayaran) |
| Modul | `billing-kasir` (Blueprint `BIL-CASH-001`, revisi `0.4`) |
| Task type | Bug fix, tidak mengubah kontrak/perilaku bisnis — murni urutan deklarasi variabel |
| Task mode | `FRONTEND` |
| Write target | `QuilvianSystemFrontendDev` |
| Branch frontend | `yasmina` |
| File diubah | `src/lib/hooks/health-services/billing-management/billing-invoices/use-menu-pembayaran.js` |

## Laporan user

Runtime `ReferenceError`: `"can't access lexical declaration 'canManage' before initialization"` di
`use-menu-pembayaran.js:64`, meng-crash route `/health-services/billing-management/billing/invoices/[slug]/pembayaran`
(halaman Menu Pembayaran) — `MenuPembayaranPage` → `MenuPembayaranView` → `useMenuPembayaran`.

## Root cause

**Dua bug temporal-dead-zone (TDZ), bukan satu**, keduanya di file yang sama:

1. `canManage` dipakai di baris lama 64 dan 66 (dikirim sebagai prop ke `useBillingDeposit` dan
   `useBillingFinalization`), tapi baru dideklarasikan dengan `const` di baris lama 198 — jauh di
   bawah titik pemakaian pertama, dalam scope function yang sama. Ini persis error yang dilaporkan
   user.
2. **Bug kedua yang belum sempat terjadi karena crash pertama menghentikan eksekusi lebih dulu**:
   `refreshPreview` dipakai di dependency array `useCallback` milik `confirmAdhoc` (baris lama 146),
   tapi baru dideklarasikan dengan `const refreshPreview = useCallback(...)` di baris lama 204.
   Begini polanya identik dengan bug #1. Bila hanya bug #1 yang diperbaiki tanpa memeriksa lebih
   jauh, error yang sama akan muncul lagi begitu render mencapai `confirmAdhoc`.

Keduanya lahir dari struktur file yang menaruh hook-hook lain (`useBillingDeposit`,
`useBillingFinancialException`, `useBillingFinalization`, `confirmAdhoc`) di atas titik di mana
`invoiceId`/`invoiceStatus`/`canManage`/`refreshPreview` sebenarnya dihitung.

## Perbaikan

Memindahkan kedua deklarasi ke posisi lebih awal — tepat setelah `safeInvoiceId` dihitung, sebelum
`applyDiscount`/`settlement`/`deposit`/dst dipanggil — tanpa mengubah nilai, urutan efek, atau
perilaku apa pun selain urutan tekstual:

```js
const safeInvoiceId = invoice?.id ?? invoice?.Id;

const invoiceId = invoice?.id ?? invoice?.Id;
const invoiceStatus = String(invoice?.status ?? invoice?.Status ?? "").toUpperCase();
const canManage = invoiceStatus === OPEN_STATUS;

const refreshPreview = useCallback(() => {
  if (!invoiceId) return undefined;
  return dispatch(previewBillingInvoiceCalculation(invoiceId));
}, [dispatch, invoiceId]);
```

Deklarasi duplikat di lokasi lama dihapus (bukan disalin) — variabel/fungsi yang sama, cuma satu
lokasi. `useEffect` yang memakai `invoiceId` untuk fetch preview saat mount tetap di posisi
aslinya, hanya deklarasi yang dipindah ke atasnya. Tidak ada perubahan pada `OPEN_STATUS`,
signature hook lain, atau logic bisnis apa pun.

## Definition of Done — validasi

| Item | Status | Bukti |
| --- | --- | --- |
| `npx eslint <file>` | **PASS** | Exit code 0, tanpa output. |
| `npm run test:unit` | **PASS (403/404)** | 1 gagal (`tests/unit/inpatient-admission.test.mjs:168`, soal `bedRefreshToken`) — **dikonfirmasi tidak terkait** file yang diubah (modul Rawat Inap, bukan Billing/Kasir); pola pre-existing yang sama sudah tercatat di laporan `FE-BKC-003` sebelumnya. |
| `npm run build` | **PASS** | Exit code 0, `postbuild`/`prepare-standalone` sukses. |
| Manual verifikasi ter-autentikasi (buka `/pembayaran` sungguhan) | **NOT FEASIBLE** | Tidak ada environment ter-autentikasi pada sesi ini — konsisten dengan keterbatasan yang sama di seluruh laporan `FE-BKC-003`–`010`. |

## Temuan sampingan (di luar scope, dilaporkan bukan diperbaiki)

Setelah `npm run build`, `git status --short` menunjukkan `.env` ikut berubah (13 baris
ditambah, 4 dihapus) — file ini **bersih di awal sesi** dan **tidak disentuh** oleh perbaikan ini
secara langsung; perubahan tampak sebagai efek samping proses build/tooling repository, bukan hasil
edit manual. Sesuai aturan keselamatan lingkungan (`AGENTS.md` — jangan mencetak isi `.env`), isi
perubahan **tidak diperiksa/dicetak** di laporan ini. Percobaan mengembalikan `.env` ke versi
ter-commit (`git checkout -- .env`) **diblokir oleh classifier auto-mode** sebagai operasi yang bisa
membuang perubahan — sengaja tidak dipaksakan. **Butuh keputusan user**: apakah `.env` boleh
dikembalikan ke versi ter-commit, atau perubahan ini perlu dipertahankan/diperiksa dulu.

## Git status

```
 M .env   (belum diputuskan — lihat Temuan sampingan)
 M src/lib/hooks/health-services/billing-management/billing-invoices/use-menu-pembayaran.js
```

Tidak ada yang di-stage/commit sesuai batasan wewenang task ini.

## Langkah berikutnya

1. **Keputusan user dibutuhkan**: apakah `.env` yang berubah setelah `npm run build` boleh
   dikembalikan ke versi ter-commit, atau perlu diperiksa dulu isinya secara manual (di luar
   laporan ini, demi keselamatan lingkungan).
2. Verifikasi manual ter-autentikasi pada halaman Menu Pembayaran begitu environment tersedia,
   sekalian dengan item verifikasi manual `FE-BKC-003`–`010` yang masih tertunda.
3. Commit perbaikan ini bersama tumpukan kerja frontend lain yang sudah menunggu instruksi commit.
