# FE-BKC-039 — Verifikasi penanganan status `Closed` yang kini benar-benar muncul

## Ringkasan untuk pembaca umum

Backend (`BE-BKC-060`–`065`) sekarang benar-benar memindahkan tagihan lunas ke status "Closed" —
sebelumnya nilai itu praktis tidak pernah muncul. Task ini memeriksa apakah layar kasir menangani
kemunculan nilai itu dengan benar: tombol yang seharusnya terkunci tetap terkunci, dan tidak ada
tampilan yang diam-diam berasumsi status itu tidak akan pernah terjadi.

**Hasil: nol gap ditemukan. Nol baris source diubah.** Ketiga titik yang diperiksa sudah menangani
`CLOSED` dengan benar sejak sebelum task ini — dua di antaranya eksplisit menyamakan `CLOSED`
dengan `FINAL`, satu lainnya (tampilan waktu penutupan) ternyata belum pernah dibangun sama sekali
untuk invoice billing, sehingga tidak ada yang bisa "berasumsi kosong".

---

## Field Laporan

> **Catatan governance**: `rules/frontend/REPORT_TEMPLATE.md` yang dirujuk `AGENTS.md` frontend
> tidak ditemukan pada instalasi suite Skill saat ini (hanya `frontend-architecture.md` yang
> ada di root rules terpasang). Laporan ini disusun mengikuti field yang secara eksplisit
> diwajibkan `AGENTS.md` § "Pelaporan Task Modul" sebagai gantinya — bukan mengarang bentuk baru.

- TASK ID: FE-BKC-039
- TASK TYPE: Verifikasi (pembacaan source), **bukan** task fitur
- PERAN REPOSITORY: `QuilvianSystemFrontendDev` — source diperiksa (read-only, nol perubahan). Laporan ditulis di `NewQuilvianSystemBackend` sesuai wewenang lintas repository yang diberikan `AGENTS.md` frontend § Pelaporan Task Modul
- TASK MODE: `AUDIT MODE` secara efektif — task ini murni pembacaan, tidak ada penulisan source yang diberi wewenang maupun dibutuhkan
- BRANCH: `yasmina` (working tree bersih dikonfirmasi sebelum dan sesudah task)
- DEPENDENCY: `BE-BKC-061` (butuh tagihan `CLOSED` yang lahir normal untuk diverifikasi perilakunya) — source backend sudah lengkap tapi **build backend belum lulus** (lihat laporan `BE-BKC-065`); verifikasi task ini dilakukan lewat pembacaan source, bukan menjalankan aplikasi, sehingga tidak tertahan oleh itu
- FILES INSPECTED:
  - `src/components/view/health-services/billing-management/billing-invoices/billing-invoices-view.jsx` (baris 92-95, 261-267)
  - `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` (baris 63-65, 177-183, 739-742, 1343-1369)
  - `src/components/view/health-services/billing-management/billing-invoices/detail/billing-finalization-panel.jsx` (baris 79-183)
  - `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-tagihan-view.jsx` (pemeriksaan status invoice untuk gerbang Edit Tagihan)
  - `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` (`BILLING_INVOICE_STATUS_OPTIONS`, `BILLING_INVOICE_STATUS_BADGE_CONFIG` — dikonfirmasi ulang sudah memuat `CLOSED`, sesuai temuan `01-existing-capability-map.md` § 21)
  - Pencarian menyeluruh `closedAt`/`ClosedAt` di seluruh `src/` (15 file cocok pola `isFinal`, lebih banyak lagi untuk `closedAt` — seluruhnya diperiksa untuk memastikan mana yang benar-benar terkait invoice billing)
- FILES CHANGED: **NONE**
- IMPLEMENTATION: **Tidak ada.** Ketiga butir verifikasi dari scope (`03-frontend-architecture.md` amendment 18 September 2026) diperiksa satu per satu:

### Butir 1 — Aksi penyuntingan terkunci sama untuk `FINAL` dan `CLOSED`

**Hasil: SUDAH BENAR, nol perubahan dibutuhkan.**

| Lokasi | Kondisi | Bukti |
| --- | --- | --- |
| `billing-invoices-view.jsx:94` | `isFinalOrClosed = rawStatus === "FINAL" \|\| rawStatus === "CLOSED"` | Eksplisit menyamakan keduanya. Dipakai baris 264 untuk pesan tooltip "Invoice sudah ditutup/final." |
| `menu-pembayaran-view.jsx:180` | `isFinal = rawStatus === FINAL_STATUS \|\| rawStatus === CLOSED_STATUS` | Eksplisit menyamakan keduanya. Dipakai baris 739 untuk mengunci tombol "Edit Tagihan", dan diteruskan ke `BillingFinalizationPanel` (baris 1365) |
| `billing-finalization-panel.jsx:109,129,177` | Tiga percabangan `!isFinal`/`isFinal` | Menerima prop `isFinal` yang SUDAH mencakup `CLOSED` dari pemanggilnya — konsisten |

**Temuan tambahan yang diverifikasi, bukan gap**: `menu-pembayaran-view.jsx:181-182` punya
`ledgerMutable = rawStatus !== CLOSED_STATUS && rawStatus !== SETTLED_BY_WRITE_OFF_STATUS` —
**sengaja tidak** mengecualikan `FINAL`. Ini **bukan** gap: `ledgerMutable` menggerbangi panel
Pengecualian Finansial (adjustment/write-off), dan kontrak backend (`state-transition-matrix.md`)
mengizinkan write-off `PATIENT_AR` diajukan atas invoice `FINAL` (hanya menolak `CLOSED`/
`SETTLED_BY_WRITE_OFF`) — perilaku frontend ini sudah cocok persis dengan gerbang backend
`CreateWriteOffAsync`.

**Edit Tagihan** (`edit-tagihan-view.jsx`): tidak ditemukan pemeriksaan status invoice di sisi
frontend sama sekali — gerbangnya sepenuhnya diserahkan ke backend (`409` untuk invoice non-`OPEN`,
mencakup `FINAL`/`CLOSED`/`SETTLED_BY_WRITE_OFF` secara seragam per `state-transition-matrix.md`
amendment 11 September 2026). Karena backend sudah memperlakukan `FINAL` dan `CLOSED` identik
pada gerbang ini **sejak sebelum** amendment `FINAL`→`CLOSED`, tidak ada perubahan perilaku yang
perlu diantisipasi frontend di titik ini.

### Butir 2 — Layar yang menampilkan waktu penutupan tidak mengandaikan nilainya kosong

**Hasil: tidak ada yang perlu diverifikasi — `BilInvoice.ClosedAt` tidak ditampilkan di mana pun.**

Pencarian menyeluruh `closedAt`/`ClosedAt` di seluruh `src/` menemukan pemakaian pada domain
**Inpatient Management** (`episode.closedAt`), **Cashier Shift** (`item.closedAt`), dan **Nutrition
Management** (`detail.closedAt`) — ketiganya konsep penutupan yang **berbeda sama sekali** dari
penutupan invoice billing. **Nol** hasil untuk `invoice.closedAt`/`invoice.ClosedAt` pada domain
Billing Invoice. Field `closedAt` pada response `GET` invoice (sudah ada di `BillingInvoiceDtos`
sejak sebelum amendment ini) belum pernah dikonsumsi satu komponen pun di frontend.

Ini bukan gap yang perlu diperbaiki task ini — scope `03-frontend-architecture.md` amendment 18
September 2026 eksplisit menyatakan **nol perubahan frontend diwajibkan**, dan menampilkan
`closedAt` di UI adalah penambahan fitur baru, bukan verifikasi. Dicatat sebagai potensi kerja
lanjutan di `KNOWN ISSUES`.

### Butir 3 — Tagihan yang kembali dari `CLOSED` ke `FINAL` muncul lagi pada daftar bersisa

**Hasil: SUDAH BENAR secara struktural, tanpa kode khusus yang dibutuhkan.**

Daftar tagihan (`billing-invoices-view.jsx`) dan Menu Pembayaran tidak melakukan turunan status
di sisi klien yang di-cache lintas navigasi — keduanya membaca `status` langsung dari response
`GET` terbaru pada setiap render/refetch. Tidak ditemukan state Redux maupun local state yang
menyimpan "status lama" sebuah invoice secara terpisah dari response API. Karena itu, invoice yang
backend kembalikan dari `CLOSED` ke `FINAL` (`BKC-DES-031`) otomatis tampil benar pada permintaan
berikutnya — sifat ini mengikuti pola fetch-per-view yang sudah ada, bukan logika baru yang perlu
ditulis.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
- API CONTRACT IMPACT: NONE — task ini tidak mengubah pemanggilan API apa pun
- KONTRAK YANG DIRUJUK: `BIL-API-1.2` (nilai `status`/`closedAt` pada response invoice, tidak berubah bentuk — dikonfirmasi ulang lewat pembacaan `billing-invoice-constants.js`)
- VALIDATION:
  | Command/check | Result | Classification | Evidence/note |
  | --- | --- | --- | --- |
  | `npm run lint:errors` | **Belum dijalankan** | NOT VERIFIED | Nol source diubah — tidak ada yang perlu di-lint. Sesuai instruksi eksplisit pengguna: jangan jalankan build/lint FE otomatis |
  | `npm run test:unit` | **Belum dijalankan** | NOT VERIFIED | Sama seperti di atas |
  | `npm run build` | **Belum dijalankan** | NOT VERIFIED | Sama seperti di atas |
  | Review source tiga butir verifikasi | Dilakukan | Manual | Rincian di atas, dengan sitasi baris persis |
- MANUAL TEST: **NOT FEASIBLE.** Verifikasi perilaku nyata (tombol terkunci pada invoice `CLOSED` sungguhan, badge tampil benar) menuntut environment ter-autentikasi dengan backend berjalan dan sekurang-kurangnya satu invoice berstatus `CLOSED` — backend saat ini belum lulus build (`BE-BKC-065`), dan `npm run dev` tidak dijalankan sesuai instruksi standing pengguna untuk tidak menjalankan build/dev otomatis. Verifikasi dilakukan lewat pembacaan source, sesuai yang diizinkan scope task ini sendiri ("pembacaan source, bukan asumsi").
- WARNINGS: NONE
- KNOWN ISSUES:
  1. `BilInvoice.ClosedAt` tidak ditampilkan di layar mana pun untuk domain Billing Invoice. Bukan bug — field ini memang belum pernah diminta ditampilkan (`03-frontend-architecture.md` tidak memintanya). Dicatat sebagai kandidat kerja lanjutan bila kelak Finance/kasir butuh melihat kapan tepatnya sebuah tagihan tertutup.
  2. Verifikasi ini tidak mencakup uji interaksi nyata (klik tombol, render badge sungguhan) — murni pembacaan source, karena environment ter-autentikasi tidak tersedia pada sesi ini. **MUST** diverifikasi ulang secara manual oleh pengguna begitu backend `BE-BKC-060`–`065` sudah lulus build dan invoice `CLOSED` nyata tersedia untuk diklik.
- INCIDENTAL CHANGES: NONE
- GIT STATUS: Bersih — `git status --short` nihil sebelum dan sesudah task (dikonfirmasi)
- NEXT RECOMMENDED STEP: (1) Setelah backend `BE-BKC-060`–`065` lulus build dan invoice `CLOSED` nyata tersedia, verifikasi manual sungguhan (klik "Edit Tagihan" pada invoice `CLOSED`, konfirmasi tombolnya terkunci; buka Menu Pembayaran invoice itu, konfirmasi panel finalisasi menampilkan pesan yang benar). (2) Pertimbangkan `KNOWN ISSUES` butir 1 (tampilkan `closedAt`) sebagai task terpisah bila dibutuhkan produk. (3) Dengan ini, seluruh gelombang `MVP-24` (`BE-BKC-060`–`063`, `FE-BKC-039`) dan `MVP-25` (`BE-BKC-064`–`065`) source/verifikasi-complete; satu blocker teknis tersisa lintas kedua repository: build backend.
