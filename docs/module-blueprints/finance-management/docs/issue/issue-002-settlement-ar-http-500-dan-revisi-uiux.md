# ISSUE-002 — Settlement AR: HTTP 500 pada daftar, duplikasi baris invoice batch, dan permintaan revisi total UI/UX

```yaml
issue_id: ISSUE-FIN-002
module_id: finance-management
submodule: null
layar: "Finance > Transaksi A/R > Pembayaran & Pelunasan > Settlement AR (FinanceSettlementService, FE: settlement-ar-view.jsx)"
sumber_laporan: "Laporan pemilik 2026-10-11: brief teknis 33 bagian (Bagian A-J) — debug HTTP 500, perbaikan saldo, revisi UI/UX total, tabel 7 kolom, footer, export, testing"
tanggal_issue: "2026-10-11"
status: DALAM_PERBAIKAN
keparahan_tertinggi: Blocker
source_sha_backend: "845dabf9 (Yasmina)"
source_sha_frontend: "46a1549dc (yasmina)"
rencana_perbaikan: ../plan-repair/plan-repair-002-settlement-ar-http-500-dan-revisi-uiux.md
```

> **Catatan penomoran:** Dokumen ini awalnya ditulis sebagai `issue-001`, namun sesi lain
> menulis `issue-001-laporan-ar-dan-report-ar-created.md` pada blueprint yang sama hampir
> bersamaan. Untuk menghindari dua `issue-001` yang saling menimpa, dokumen ini dan pasangan
> plan repair-nya dinomori ulang menjadi `002`. Prefix `FIN` mengikuti konvensi task ID modul
> ini (`BE-FIN-*`/`FE-FIN-*`), konsisten dengan `issue-001` yang sudah ada.

---

## 1. Ringkasan

Layar Settlement AR (Finance → Transaksi A/R → Pembayaran & Pelunasan) gagal dimuat dengan
HTTP 500 saat memfilter kategori Perusahaan. Penelusuran kode menemukan satu cacat data yang
pasti (duplikasi baris akibat join tabel membership invoice batch tidak menyaring baris
terhapus), tetapi **penyebab pasti HTTP 500 belum dapat dipastikan dari pembacaan source saja**
— seluruh jalur EF Core yang diperiksa secara struktural dapat diterjemahkan ke SQL, sehingga
dibutuhkan stack trace asli atau log server saat request direproduksi. Temuan paling berdampak
luas: implementasi backend (`FinanceSettlementService`, endpoint `receivables/settlement/*`) dan
halaman frontend dedicated yang sedang berjalan **tidak tertelusur sama sekali** pada blueprint
modul ini — dokumen desain yang disetujui justru menyatakan "Settlement AR" seharusnya memakai
ulang layar `/finance/receipts` yang sudah ada. Permintaan revisi UI/UX pada laporan (layout
filter, information card, tabel tanpa scroll horizontal, footer, dll.) karena itu diperlakukan
sebagai keputusan desain baru dari pemilik pada laporan ini sendiri, bukan sebagai bug atas
desain yang sudah terkunci sebelumnya.

---

## 2. Laporan asli

| No. (bagian laporan) | Keluhan/permintaan (diringkas) | Lampiran yang merujuknya |
| ---: | --- | --- |
| Bagian A (§2-§7) | GET `.../receivables/settlement?category=COMPANY&...` mengembalikan HTTP 500; minta audit `BuildFilteredQueryable`, `GetSettlementPagedAsync`, `GetSettlementSummaryInternalAsync`, join Invoice Batch | Tidak ada lampiran terpisah; payload request dan response disertakan dalam teks laporan |
| Bagian B (§8-§12) | Ketepatan Beginning Balance (saldo sebelum StartDate), Ending Balance (saldo s/d EndDate), label "Posisi Terkini" saat tanpa periode, validasi StartDate ≤ EndDate | Tidak ada |
| Bagian C (§13-§15) | Pertahankan endpoint, filter, dan bentuk respons existing | Tidak ada |
| Bagian D (§16-§21) | Revisi total layout: header, category tabs, filter 2 kolom (32-35% / 65-68%), 3 information card, kondisi error list/summary | Screenshot disebut ("gunakan screenshot yang diberikan sebagai kondisi awal") — **tidak benar-benar terlampir pada percakapan ini** |
| Bagian E (§22-§26) | Tabel 7 kolom tanpa horizontal scroll, proporsi kolom, `table-layout: fixed`, pagination 10 baris | Sama seperti di atas — screenshot disebut tapi tidak terlampir |
| Bagian F (§27-§28) | Navigasi Detail Settlement AR by `ReceivableId`, tombol kembali mempertahankan filter/halaman | Tidak ada |
| Bagian G (§29-§30) | Footer "terlalu besar dan menutupi bagian bawah tabel" pada screenshot; perbaiki tinggi/positioning/z-index/overlap | Screenshot disebut, tidak terlampir |
| Bagian H (§31) | Export Excel/PDF mengikuti filter yang sama, seluruh hasil filter bukan hanya 10 baris, numeric di Excel | Tidak ada |
| Bagian I (§32-§33) | Daftar skenario test BE (22 butir) dan FE (18 butir) pada berbagai viewport | Tidak ada |
| Bagian J (§J) | Format output wajib sebelum/sesudah coding; batasan: read-only report, jangan ubah saldo/ledger/schema tanpa alasan | Tidak ada |

---

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `ISS-FIN-002-01` | Bagian A | HTTP 500 saat memuat daftar Settlement AR kategori COMPANY | `BUG` | Backend | Blocker | SUDAH-VERIFIKASI (log server asli) | `FIX-FIN-002-01` |
| `ISS-FIN-002-02` | Bagian A §5 | Duplikasi baris akibat `batchSubquery` tidak menyaring `IsDelete` | `BUG` | Backend | High | SUDAH-VERIFIKASI | `FIX-FIN-002-02` |
| `ISS-FIN-002-03` | Bagian G | Footer global berpotensi menimpa konten Settlement AR | `BUG` (mekanisme terverifikasi), dampak visual belum dikonfirmasi | Frontend | Medium | SUDAH-VERIFIKASI (mekanisme); PERLU_KONFIRMASI (dampak nyata di layar) | `FIX-FIN-002-03` |
| `ISS-FIN-002-04` | Bagian D, E, F | Revisi total layout filter, information card, tabel 7 kolom, navigasi detail | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI (tidak ada desain terkunci sebelumnya untuk layar dedicated ini) | `FIX-FIN-002-04` s/d `FIX-FIN-002-07` |
| `ISS-FIN-002-05` | Bagian B | Ketepatan Beginning/Ending Balance dan "Posisi Terkini" | Sebagian besar **sudah sesuai** permintaan; satu titik perlu klarifikasi (makna "Posisi Terkini" saat tanpa tanggal namun `BeginningBalance` historis lain ada) | Backend | Low | SUDAH-VERIFIKASI | `FIX-FIN-002-08` |
| `ISS-FIN-002-06` (temuan tambahan) | — | `FinanceSettlementService` dan halaman FE dedicated tidak tertelusur di blueprint — desain terkunci menyatakan Settlement AR = reuse `/finance/receipts` | `RULE_VIOLATION` (traceability) / `PERLU_KONFIRMASI` | Backend + Frontend + Dokumentasi | High | SUDAH-VERIFIKASI | `FIX-FIN-002-09` |
| `ISS-FIN-002-07` (temuan tambahan) | — | Dua route frontend untuk "Settlement AR" (`/finance/receivable/settlement` dan `/finance/settlement`) | `CONTRACT_MISMATCH` / `PERLU_KONFIRMASI` | Frontend | Medium | SUDAH-VERIFIKASI (dua file route ada); DUGAAN (mana yang aktif dipakai) | `FIX-FIN-002-10` |
| `ISS-FIN-002-08` (temuan tambahan) | — | `SETTLEMENT_SORT_OPTIONS` di FE tidak menyediakan opsi urut untuk 3 dari 6 kolom tabel (`beginningBalance`, `endingBalance`, `totalPiutang`) meski backend `ApplySorting` sudah mendukungnya | `CONTRACT_MISMATCH` (kapabilitas backend tidak dipakai FE) | Frontend | Low | SUDAH-VERIFIKASI | `FIX-FIN-002-11` |

---

## 4. Rincian per temuan

### ISS-FIN-002-01 — HTTP 500 saat memuat daftar Settlement AR kategori COMPANY

| | |
| --- | --- |
| **No. laporan** | Bagian A |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI — ditemukan exception asli pada `Logs/quilvian-backend-20261011.json` (berulang sejak 10:53:45 UTC hari ini, 5+ kejadian) |
| **Perbaikan** | `FIX-FIN-002-01` |

**Apa yang terjadi.** Petugas Finance membuka Settlement AR pada tab Perusahaan dan sistem
menampilkan pesan gagal generik ("Terjadi kesalahan pada server") alih-alih daftar piutang.

**Kenapa terjadi.** Audit statis awal (tanpa stack trace) tidak menemukan cacat translasi yang
pasti. Log aplikasi asli (`Logs/quilvian-backend-20261011.json`, entri `@t:
"2026-10-11T10:53:45.5569400Z"` dan empat entri identik berikutnya di hari yang sama) membuktikan
akar masalahnya:

```
System.InvalidOperationException: The LINQ expression '...
    .Select(ti5 => new SettlementQueryRow(
        ..., 
        ti5.Outer.Outer.Inner != null ? ti5.Outer.Outer.Inner.CompanyGuarantorName
            : ti5.Outer.Inner != null ? ti5.Outer.Inner.InsuranceProviderName
            : ti5.Outer.Outer.Outer.Inner != null ? ti5.Outer.Outer.Outer.Inner.FullName
            : ti5.Outer.Outer.Outer.Outer.Inner != null ? ti5.Outer.Outer.Outer.Outer.Inner.FullName
            : null, 
        ti5.Inner.Inner.BatchNumber
    ).debtorResolvedName)' could not be translated. Either rewrite the query in a form that
can be translated, or switch to client evaluation explicitly...
   at ... FinanceSettlementService.GetSettlementPagedAsync(...)
   at ... FinanceReceivablesController.GetSettlement(...)
```

Penyebabnya: `ApplySorting` (`Services/FinanceSettlementService.cs:798-827`), cabang default
untuk `sortBy=patientName` (persis parameter yang dilaporkan):

```csharp
_ => desc
    ? query.OrderByDescending(x => x.pat != null ? x.pat.FullName : x.debtorResolvedName).ThenBy(x => x.r.Id)
    : query.OrderBy(x => x.pat != null ? x.pat.FullName : x.debtorResolvedName).ThenBy(x => x.r.Id)
```

mengurutkan berdasarkan `x.debtorResolvedName` — sebuah field yang **sudah dihitung** sebagai
ternary bersarang empat tingkat (`comp → ins → emp → pat`, `BuildFilteredQueryable:758-761`) di
dalam proyeksi `SettlementQueryRow`, hasil dari enam `LEFT JOIN` berantai yang masing-masing
dibungkus `TransparentIdentifier`. EF Core (Npgsql provider, EF Core 9.0.18) tidak dapat
menerjemahkan ulang referensi ke anggota hasil proyeksi (`...).debtorResolvedName`) ketika dipakai
lagi pada `OrderBy` yang dipasang **setelah** `Select` — ini bukan soal tipe data atau navigation
property yang salah konfigurasi (keduanya sudah diperiksa dan benar), melainkan batas nyata
penerjemah LINQ-to-SQL EF Core pada kedalaman join/pembungkusan ini. Filter `Search`
(`BuildFilteredQueryable:782-793`, baris 792: `x.debtorResolvedName != null &&
x.debtorResolvedName.ToUpper().Contains(search)`) memakai pola identik dan akan gagal dengan
exception yang sama persis begitu parameter `Search` diisi, meski belum tertangkap di log hari
ini karena repro yang dijalankan pelapor tidak menyertakan `Search`.

`FinanceReceivablesController.GetSettlement` (baris 219-225) memanggil
`GetSettlementPagedAsync` tanpa `try/catch` (berbeda dari aksi lain pada controller yang sama),
sehingga `InvalidOperationException` ini diteruskan apa adanya ke middleware global dan
dibungkus menjadi envelope 500 generik — ini menjelaskan *bagaimana* 500 muncul ke klien,
sementara akar sebenarnya adalah kegagalan translasi EF Core di atas.

**Apakah ini menyimpang dari desain?** Tidak relevan — kegagalan fungsional murni.

**Dampak nyata.** Petugas Finance tidak dapat membuka Settlement AR sama sekali untuk kategori
Perusahaan (dan kategori lain mana pun, karena `sortBy=patientName` adalah default untuk
seluruh kategori), menghambat proses pembayaran & pelunasan sehari-hari sejak pagi ini.

**Rekomendasi.** Perbaikan **tidak boleh** memakai `.AsEnumerable()`/`.ToList()` sebagai jalan
pintas sebelum filtering/pagination (dilarang eksplisit oleh pemilik). Solusi yang tepat:
`SettlementQueryRow` perlu membawa `comp`/`ins` (entitas penjamin) sebagai anggota sendiri,
bukan hanya `debtorResolvedName` yang sudah dihitung — sehingga `ApplySorting` dan filter
`Search` dapat menghitung ulang ternary yang sama langsung dari anggota entitas (`x.pat`,
`x.emp`, `x.comp`, `x.ins`) pada titik pemakaian, bukan merujuk ulang ke hasil proyeksi
`debtorResolvedName`. Rincian lengkap di `FIX-FIN-002-01` pada plan repair.

---

### ISS-FIN-002-02 — Duplikasi baris akibat `batchSubquery` tidak menyaring `IsDelete`

| | |
| --- | --- |
| **No. laporan** | Bagian A §5 ("Jangan menghasilkan baris duplikat... Validasi: COUNT DISTINCT ReceivableId harus sesuai") |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-002-02` |

**Apa yang terjadi.** Bila satu piutang pernah memiliki lebih dari satu baris keanggotaan batch
invoice (`FinReceivableInvoiceBatchItem`) — satu di antaranya soft-delete, satu lagi aktif —
piutang itu berpotensi muncul dua kali pada daftar Settlement AR, dan `TotalData`/`TotalPage`
ikut menghitung ganda.

**Kenapa terjadi.** `batchSubquery`
(`Services/FinanceSettlementService.cs:734-735`):

```csharp
var batchSubquery = _dbContext.FinReceivableInvoiceBatchItems.AsNoTracking()
    .Where(b => b.IsActiveMembership && b.Batch!.Status != FinReceivableInvoiceBatchStatuses.Cancelled);
```

tidak menyertakan `!b.IsDelete`. Konfigurasi EF
(`Repositories/Configurations/.../FinReceivableInvoiceBatchItemConfiguration.cs:38-41`) hanya
menjamin keunikan `(ReceivableId)` di antara baris yang **belum dihapus** dengan
`IsActiveMembership = true` (`HasFilter("\"IsDelete\" = false AND \"IsActiveMembership\" = true")`).
Baris yang sudah soft-delete tetapi kolom `IsActiveMembership`-nya masih `true` (tidak pernah
di-reset saat soft-delete) **tidak melanggar** index parsial itu, sehingga dua baris —
satu soft-deleted, satu aktif baru — dapat sama-sama lolos filter `batchSubquery` untuk
`ReceivableId` yang sama. Join `LEFT JOIN` pada baris 750-751 lalu menghasilkan dua
`SettlementQueryRow` untuk satu `FinReceivable` yang sama.

**Apakah ini menyimpang dari desain?** `BUG` — pemilik secara eksplisit meminta pada laporan
(Bagian A §5): *"Jangan menghasilkan baris duplikat... Pilih relasi aktif sesuai lifecycle
batch... Validasi: COUNT DISTINCT ReceivableId... harus sesuai jumlah receivable sebenarnya."*
Tidak ada dokumen desain yang menyetujui perilaku duplikat ini (lihat `ISS-FIN-002-06` — tidak
ada dokumen desain untuk service ini sama sekali).

**Dampak nyata.** Satu piutang perusahaan (pseudonim: PT Contoh Abadi) yang invoice batch-nya
pernah dibatalkan lalu diterbitkan ulang berpotensi muncul dua kali pada tabel Settlement AR,
dan nominal total piutang pada summary card ikut terhitung dua kali untuk piutang tersebut.

**Rekomendasi.** Tambahkan `!b.IsDelete` pada `batchSubquery` (`FIX-FIN-002-02`). Ini tidak
mengubah skema maupun data — hanya memperbaiki predikat filter query baca.

---

### ISS-FIN-002-03 — Footer global berpotensi menimpa konten Settlement AR

| | |
| --- | --- |
| **No. laporan** | Bagian G (§29-§30) |
| **Jenis** | `BUG` (mekanisme) |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI (mekanisme); PERLU_KONFIRMASI (kejadian nyata di layar, karena laporan menyebut screenshot yang tidak benar-benar terlampir pada percakapan ini) |
| **Perbaikan** | `FIX-FIN-002-03` |

**Apa yang terjadi.** Pemilik melaporkan footer "terlalu besar dan menutupi bagian bawah tabel"
berdasarkan screenshot yang disebut dalam laporan, namun screenshot tersebut tidak benar-benar
terlampir pada pesan yang diterima.

**Kenapa terjadi.** Tidak ada footer khusus Finance — aplikasi memakai satu footer global
(`src/components/features/footers/footer.jsx`, fungsi `Footer`, export default baris 103) yang
di-*portal*-kan langsung ke `document.body` lewat `createPortal` (baris 156-163) — di luar pohon
layout halaman biasa. Komponen ini menghitung "ruang aman" (`applyFooterSafeSpace`, baris 14-43)
dengan mengukur tinggi dirinya sendiri lalu menambahkan class `app-footer-scroll-safe` **hanya**
pada elemen berid `content-page` atau berclass `.content-page` (baris 31-42,
`document.getElementById("content-page")` / `.content-page`). Pembungkus utama halaman
Settlement AR memakai class `finance-settlement-page`
(`src/components/view/finance/receivable/settlement/settlement-ar-view.jsx:100`), **bukan**
`content-page`/`.content-page`. Bila kontainer scroll yang sesungguhnya pada layar ini bukan
elemen yang dicari `footer.jsx`, class penambah ruang aman tidak pernah terpasang pada kontainer
itu, dan footer yang fixed/portaled berpotensi menimpa baris tabel terbawah.

`FooterWrapper` (`src/components/features/footers/footer-wrapper.jsx:9-15`) tidak
menyembunyikan footer untuk rute `/finance/*` mana pun, jadi footer ini selalu dirender pada
halaman Settlement AR.

**Apakah ini menyimpang dari desain?** Tidak ada ketentuan desain Finance mengenai footer pada
blueprint (digeledah: tidak ada hasil untuk "footer" pada `03-frontend-architecture.md`), jadi
ini murni cacat teknis pada implementasi existing (mismatch id/class antara `footer.jsx` dan
pembungkus halaman Settlement AR), bukan penyimpangan dari keputusan desain tertentu.

**Dampak nyata.** Baris terbawah tabel Settlement AR (dan tombol pagination/export) berpotensi
tertutup sebagian oleh footer, khususnya saat tabel bertambah tinggi setelah data dimuat.

> **Koreksi 2026-10-11 (saat implementasi `FE-FIN-044`):** Dugaan di atas **tidak terbukti**.
> Pembacaan `src/components/ui/content-page.jsx` dan `src/components/providers/root-layout-shell.jsx`
> membuktikan setiap halaman normal (bukan kiosk/nurse-view/login/error) — termasuk Settlement AR —
> sudah otomatis dibungkus `<div id="content-page" className="content-page">` oleh shell aplikasi,
> pada level yang lebih tinggi dari class `finance-settlement-page` milik halaman ini. Karena itu
> `footer.jsx` **berhasil** menemukan kontainer dan mekanisme *safe space*-nya sudah bekerja benar
> tanpa perubahan apa pun. **Tidak ada perubahan source yang diterapkan untuk temuan ini** — lihat
> `plan-repair-002` register `FIX-FIN-002-03` dan laporan `FE-FIN-044.md` bagian 1. Dampak visual
> yang dilaporkan pemilik kemungkinan bersumber dari penyebab lain yang tidak teridentifikasi dari
> pembacaan source saja (screenshot asli masih dibutuhkan — lihat P-01).

**Rekomendasi.** Opsi ada di plan repair (`FIX-FIN-002-03`). Karena mekanisme sudah terverifikasi
dari source namun dampak visual nyata belum dikonfirmasi dari screenshot asli, perbaikan ini
disarankan diverifikasi ulang secara visual (browser) sebelum ditandai selesai — lihat P-01 pada
bagian 6.

---

### ISS-FIN-002-04 — Revisi total layout filter, information card, tabel 7 kolom, navigasi detail

| | |
| --- | --- |
| **No. laporan** | Bagian D, E, F (§16-§28) |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI — tidak ada rancangan tampilan terkunci sebelumnya untuk layar dedicated ini (lihat `ISS-FIN-002-06`) |
| **Perbaikan** | `FIX-FIN-002-04`, `FIX-FIN-002-05`, `FIX-FIN-002-06`, `FIX-FIN-002-07` |

**Apa yang terjadi.** Pemilik meminta perombakan layout: kategori tab di atas, filter 2 kolom
(32-35% kiri / 65-68% kanan di desktop), 3 information card dengan tinggi seragam, tabel 7 kolom
dengan proporsi tetap tanpa scroll horizontal di desktop/laptop, dan halaman detail yang
mempertahankan filter saat kembali.

**Kenapa ditulis sebagai `DESIGN_CHANGE`, bukan `BUG`.** Blueprint modul ini (`03-frontend-architecture.md`
§17.2, baris 631) menyatakan Settlement AR seharusnya **memakai ulang** layar `/finance/receipts`
yang sudah ada (`FE-FIN-004`) — bukan layar dedicated dengan filter/card/tabel sendiri. Tidak ada
satu pun bagian pada blueprint ini (digeledah untuk "footer", "scroll horizontal", "responsif",
"kartu informasi", "category tab", "COMPANY"/"PATIENT"/"EMPLOYEE" sebagai label tab) yang memuat
rancangan tampilan untuk layar dedicated yang sekarang berjalan. Karena itu permintaan ini **bukan**
perbaikan atas penyimpangan dari desain yang disetujui — ini adalah keputusan desain baru, yang
kebetulan baru saja diambil pemilik lewat laporan ini sendiri.

**Apa yang sudah ada vs. yang diminta (ringkas; rincian skema di plan repair).** Tabel saat ini
sudah menampilkan 6 kolom data + 1 kolom Aksi sesuai struktur yang diminta
(`settlement-ar-table-columns.jsx:20-145`); kategori tab, hook, dan endpoint sudah berjalan
(`use-settlement-ar.jsx`). Yang belum ada: proporsi lebar kolom tetap (`table-layout: fixed`),
tata letak filter 2-kolom 32/68, dan jaminan "tanpa horizontal scroll" di lebar viewport yang
disebut pemilik.

**Dampak nyata.** Petugas Finance saat ini harus scroll menyamping atau berhadapan dengan layout
yang kurang proporsional saat memakai Settlement AR di laptop standar.

**Rekomendasi.** Karena ini `DESIGN_CHANGE`, dokumen hulu (`03-frontend-architecture.md` §17.2,
dan idealnya satu bagian baru yang mendokumentasikan layar dedicated V2 ini) perlu direvisi
sejalan dengan source — lihat bagian 8 pada plan repair. Rincian solusi dan skema
SEBELUM→SESUDAH ada pada `FIX-FIN-002-04` s/d `FIX-FIN-002-07`.

---

### ISS-FIN-002-05 — Ketepatan Beginning/Ending Balance dan label "Posisi Terkini"

| | |
| --- | --- |
| **No. laporan** | Bagian B (§8-§12) |
| **Jenis** | Sebagian besar **sesuai** permintaan (bukan `BUG`); satu titik `PERLU_KONFIRMASI` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-002-08` (dokumentasi/penguatan test saja, bukan perubahan logika) |

**Apa yang terjadi.** Pemilik meminta Beginning Balance dihitung sebagai saldo tepat sebelum
`StartDate`, Ending Balance sebagai saldo s/d `EndDate`, dan label "Posisi Terkini" dipakai
hanya saat tidak ada periode dipilih.

**Kenapa ditulis sebagai sesuai, bukan bug.** `CalculateBalances`
(`Services/FinanceSettlementService.cs:616-677`) sudah mengimplementasikan persis logika ini:
Beginning Balance diambil dari `BalanceAfter` mutasi terakhir sebelum `StartDate` (baris
631-650), Ending Balance dari `BalanceAfter` mutasi terakhir s/d `EndDate` (baris 657-670), dan
bila keduanya kosong, memakai `OriginalAmount`/`OutstandingAmount` berjalan (baris 622-625).
`FormatPeriodLabel` (baris 605-614) sudah mengembalikan tepat *"Semua Periode (Posisi Terkini)"*
saat tidak ada tanggal yang dipilih sama sekali — sesuai §11 pada laporan.

**Satu titik yang perlu konfirmasi (bukan bug).** §11 pada laporan menyebut: *"Jika Beginning
Balance tanpa periode menggunakan original amount, jelaskan semantiknya melalui label seperti
'Nilai Awal Piutang'."* Backend saat ini memang memakai `OriginalAmount` untuk
`BeginningBalance` saat tanpa tanggal (baris 622-625), tetapi **tidak ada field/label terpisah**
yang menjelaskan semantik "Nilai Awal Piutang" ini ke frontend — `FinanceSettlementRowResponse`
(`DTOs/FinanceSettlementDtos.cs:118`) hanya berkomentar *"Saldo tepat sebelum awal periode (atau
saldo awal berjalan)"*, tanpa field label semantik terpisah. Ini murni soal bagaimana FE
menerjemahkan nilai yang sudah benar ke dalam teks UI — dicatat sebagai bagian dari
`FIX-FIN-002-04` (revisi information card), bukan perbaikan backend.

**Dampak nyata.** Tidak ada cacat data. Risiko yang ada murni presentasional di FE bila label
tidak dibedakan antara "posisi berjalan" dan "nilai awal piutang".

**Rekomendasi.** Tidak perlu perubahan logika backend. Pastikan FE memakai label berbeda sesuai
ada/tidaknya periode — lihat `FIX-FIN-002-04`.

---

### ISS-FIN-002-06 (temuan tambahan) — `FinanceSettlementService` dan halaman FE dedicated tidak tertelusur di blueprint

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan saat menetapkan "desain yang disetujui" untuk mengklasifikasikan `ISS-FIN-002-04`; berdampak pada cara seluruh permintaan UI/UX pada laporan ini harus diperlakukan |
| **Jenis** | `RULE_VIOLATION` (traceability) / `PERLU_KONFIRMASI` |
| **Area** | Backend + Frontend + Dokumentasi blueprint |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-002-09` |

**Apa yang terjadi.** Implementasi backend yang sedang berjalan dan dilaporkan error
(`FinanceSettlementService`, endpoint `GET/{v1}/corporate/finance-management/receivables/settlement*`)
beserta halaman frontend dedicated-nya (`settlement-ar-view.jsx`, `use-settlement-ar.jsx`, dll.)
**tidak muncul sama sekali** di `00-interview-decisions.md`, `02-backend-architecture.md`,
`03-frontend-architecture.md`, `contracts/api-contract.md`,
`contracts/validation-matrix.md`/`permission-audit-matrix.md`, maupun
`roadmap/01-backend-roadmap.md`/`roadmap/02-frontend-roadmap.md` pada blueprint
`finance-management` — digeledah menyeluruh untuk istilah `FinanceSettlementService`,
`BuildFilteredQueryable`, `GetSettlementPagedAsync`, `GetSettlementSummaryInternalAsync`, dan
`receivables/settlement`, nol hasil pada seluruhnya.

Desain yang **benar-benar terkunci dan disetujui** untuk menu "Settlement AR"
(`03-frontend-architecture.md` §17.2, baris 631; keputusan pemilik `FIN-DEC-107`,
`00-interview-decisions.md:1223`) adalah: menu ini memakai ulang layar penerimaan/alokasi
pembayaran yang sudah ada di `/finance/receipts` (dibangun lewat `FE-FIN-004`/`BE-FIN-016..018`),
dengan hak akses `FinanceReceipt : Read` — **bukan** layar dedicated dengan endpoint, service,
dan tab kategori sendiri. Laporan task `task/report/frontend/FE-FIN-016.md` (baris 43, 118, 138)
bahkan secara eksplisit mengoreksi dugaan rute lama (`/finance/receivable?status=SETTLED`)
menjadi `/finance/receipts` sesuai keputusan tersebut.

Dengan kata lain: source code "V2" yang sekarang berjalan dan sedang diminta untuk diperbaiki
dan direvisi UI/UX-nya dibangun **di luar** jalur traceability blueprint modul ini.

**Apakah ini menyimpang dari desain?** Ya, secara dokumentasi — tetapi pelapor pada percakapan
ini jelas memperlakukan implementasi V2 ini sebagai yang sah dan ingin dilanjutkan/diperbaiki,
bukan dibongkar untuk kembali ke `/finance/receipts`. Keputusan mana yang berlaku ke depan tetap
perlu konfirmasi eksplisit pemilik — lihat K-01 pada bagian 6, karena ini menentukan dokumen mana
yang harus direvisi dan apakah pekerjaan UI/UX pada laporan ini boleh dilanjutkan sebagai
perluasan resmi blueprint atau harus menunggu keputusan arsitektur terlebih dahulu.

**Dampak nyata.** Tanpa keputusan ini, setiap task roadmap baru yang mengerjakan perbaikan pada
`FinanceSettlementService`/halaman dedicated akan terus menjadi pekerjaan "di luar radar"
blueprint — risiko yang sama bisa terulang untuk modul lain bila pola ini dibiarkan.

**Rekomendasi.** Pemilik memilih salah satu dari tiga opsi pada `K-01` (bagian 6). Rekomendasi:
Opsi tetapkan V2 dedicated sebagai desain resmi baru (lihat alasannya di plan repair) — paling
kecil radius dampaknya karena tidak membongkar implementasi yang sudah berjalan dan sudah
menjadi fokus perbaikan pada laporan ini.

---

### ISS-FIN-002-07 (temuan tambahan) — Dua route frontend untuk "Settlement AR"

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri rute halaman untuk memetakan `layar` pada metadata issue ini |
| **Jenis** | `CONTRACT_MISMATCH` / `PERLU_KONFIRMASI` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI (dua file route ada); DUGAAN (mana yang benar-benar dipakai/ditautkan menu) |
| **Perbaikan** | `FIX-FIN-002-10` |

**Apa yang terjadi.** Ditemukan dua route Next.js dengan judul "Settlement AR": 
`src/app/finance/receivable/settlement/page.jsx` (memuat `SettlementArView` lengkap dengan
breadcrumb "Pembayaran & Pelunasan" yang cocok dengan laporan) dan
`src/app/finance/settlement/page.jsx` (judul sama, isi belum ditelusuri lebih lanjut).

**Kenapa terjadi.** Belum dapat dipastikan dari pembacaan source saja apakah route kedua adalah
sisa percobaan lama, alias duplikat yang sengaja dibuat, atau benar-benar ditautkan dari menu
sidebar yang berbeda.

**Dampak nyata.** Berpotensi membingungkan petugas bila dua URL berbeda menampilkan konten yang
mirip namun tidak identik, atau salah satu route adalah kode mati yang seharusnya dibersihkan.

**Rekomendasi.** `FIX-FIN-002-10` pada plan repair: audit konfigurasi menu/sidebar untuk
memastikan route mana yang benar-benar ditautkan, lalu tandai yang lain sebagai dihapus atau
diarahkan ulang (`redirect`) sesuai temuan.

---

### ISS-FIN-002-08 (temuan tambahan) — Opsi urut FE tidak mencakup kolom nominal

| | |
| --- | --- |
| **Kenapa dicantumkan** | Ditemukan saat menelusuri kesesuaian `ApplySorting` backend dengan kemampuan sorting yang diekspos FE |
| **Jenis** | `CONTRACT_MISMATCH` (kapabilitas backend tidak dipakai FE) |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-FIN-002-11` |

**Apa yang terjadi.** `ApplySorting` backend (`Services/FinanceSettlementService.cs:798-827`)
mendukung pengurutan `invoicenumber`, `duedate`, `outstandingamount`, `totalpiutang`, dan
`patientname` (default). `SETTLEMENT_SORT_OPTIONS` pada FE
(`src/lib/constants/finance/receivable/settlement-ar-constants.jsx:45-54`) hanya mendaftarkan
`patientName`, `invoiceNumber`, `dueDate`, `outstandingAmount` — tidak ada opsi urut untuk
`totalPiutang`, `beginningBalance`, maupun `endingBalance`, padahal tiga kolom nominal itu
tampil di tabel.

**Dampak nyata.** Petugas tidak dapat mengurutkan tabel berdasarkan Total Piutang meski kolom
itu ditampilkan dan backend sudah mendukungnya untuk `totalpiutang`.

> **Koreksi 2026-10-11 (saat implementasi `FE-FIN-044`):** Premis "opsi kurang lengkap" **keliru**.
> `grep` menyeluruh membuktikan `SETTLEMENT_SORT_OPTIONS` tidak dirujuk di mana pun selain
> deklarasinya sendiri — tidak ada dropdown atau kontrol sort apa pun yang pernah dirender untuk
> Settlement AR. Ini bukan "3 opsi yang hilang dari daftar aktif", melainkan kode mati yang
> disiapkan untuk kontrol yang belum pernah dibangun. Menambah entri ke konstanta ini tidak
> berdampak apa pun selama tidak ada UI yang memakainya, dan membangun kontrol sort baru adalah
> penambahan fitur yang tidak diminta eksplisit pada laporan manapun. **Tidak dikerjakan** — lihat
> `plan-repair-002` register `FIX-FIN-002-11` dan laporan `FE-FIN-044.md` bagian 1.

**Rekomendasi.** `FIX-FIN-002-11`: tambahkan opsi `totalPiutang` ke `SETTLEMENT_SORT_OPTIONS`
FE (backend sudah mendukung). Untuk `beginningBalance`/`endingBalance`, backend **belum**
mendukungnya di `ApplySorting` — bila pemilik ingin kolom ini juga bisa diurutkan, itu
penambahan baru yang dicatat sebagai keputusan terpisah (lihat K-02 pada bagian 6), bukan bug,
karena perhitungan balance historis tidak selalu berupa kolom SQL yang bisa langsung di-`ORDER BY`.

---

## 5. Temuan tambahan

Sudah dicantumkan sebagai `ISS-FIN-002-06`, `ISS-FIN-002-07`, dan `ISS-FIN-002-08` di atas
(masing-masing memuat baris "Kenapa dicantumkan").

---

## 6. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:**

| No | Pertanyaan | Kenapa ditanyakan | Dampak bila belum dijawab |
| ---: | --- | --- | --- |
| P-01 | Screenshot kondisi awal yang disebut pada Bagian D/E/G belum benar-benar terlampir di percakapan ini — bisakah dilampirkan ulang (boleh data pseudonim)? | `ISS-FIN-002-03` (footer) dan detail proporsi visual pada `ISS-FIN-002-04` (filter/card/tabel) dianalisis dari source, bukan dari tangkapan layar aktual | Perbaikan footer dan layout tetap bisa dikerjakan dari mekanisme yang sudah terverifikasi di source, tetapi tidak dapat dipastikan menutup kondisi persis yang terlihat di screenshot pemilik |
| ~~P-02~~ | ~~Request yang menghasilkan 500 — bisakah disertakan stack trace asli?~~ | **Terjawab tanpa menunggu pelapor**: ditemukan di `Logs/quilvian-backend-20261011.json`, entri `InvalidOperationException` pada `debtorResolvedName` dalam `ApplySorting`, berulang sejak 10:53:45 UTC hari ini | `FIX-FIN-002-01` kini dapat dikerjakan — lihat rincian pada temuan `ISS-FIN-002-01` |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Bagaimana memperlakukan implementasi `FinanceSettlementService`/halaman dedicated yang tidak tertelusur di blueprint (`ISS-FIN-002-06`)? | (a) Tetapkan sebagai desain resmi baru — revisi `03-frontend-architecture.md`/`02-backend-architecture.md`/`contracts/api-contract.md` agar mencerminkan implementasi V2 yang sudah berjalan; (b) Bongkar V2, kembalikan menu Settlement AR ke `/finance/receipts` sesuai desain terkunci; (c) Biarkan dulu, selesaikan hanya perbaikan bug 500 + duplikasi tanpa revisi UI/UX sampai keputusan arsitektur final | **(a)** — radius dampak terkecil, tidak membongkar fitur yang sudah menjadi fokus perbaikan pada laporan ini, dan laporan pemilik sendiri memperlakukan V2 sebagai yang sah | `FIX-FIN-002-04` s/d `FIX-FIN-002-07` (seluruh revisi UI/UX); juga menentukan apakah `FIX-FIN-002-09` cukup berupa pembaruan dokumen atau butuh pembongkaran kode |

> **Keputusan diambil 2026-10-11:** Pemilik menginstruksikan langsung di chat untuk "kerjakan
> semua" perbaikan lewat `build-module-backend`/`build-module-frontend` tanpa mengecualikan
> item yang bergantung pada K-01, dan tidak pernah mengarah ke opsi (b)/(c) di titik mana pun
> pada percakapan. Diperlakukan sebagai **Opsi (a) terpilih** — konsisten dengan rekomendasi di
> atas. `FIX-FIN-002-04` s/d `-07`, `-09` dilanjutkan atas dasar ini.
| K-02 | Apakah kolom Beginning Balance dan Ending Balance perlu bisa diurutkan dari tabel (`ISS-FIN-002-08`)? | Ya, tambahkan dukungan `ApplySorting` backend untuk dua kolom ini / Tidak, cukup 4 kolom yang sudah ada | Tidak — bukan bagian dari permintaan laporan manapun, baru ditemukan sebagai gap kecil | `FIX-FIN-002-11` (bagian kolom balance saja) |

Keputusan yang **sudah diambil pemilik** pada laporan ini sendiri (dicatat, bukan ditanyakan
ulang): seluruh tata letak filter/card/tabel pada Bagian D-G (`ISS-FIN-002-04`), larangan
mengubah saldo/ledger/schema tanpa kebutuhan terbukti (Bagian J), dan keharusan memperbaiki akar
masalah backend alih-alih menyembunyikannya di frontend (§1 pembuka laporan).

---

## 7. Catatan pola

- Pola `try/catch` yang tidak konsisten antar-aksi pada controller yang sama
  (`FinanceReceivablesController`) — beberapa aksi (`GetMovements`, `RequestAdjustment`, dll.)
  menangani `KeyNotFoundException`/`ReceivableConflictException`/dst. secara eksplisit, sementara
  aksi Settlement AR (`GetSettlement`, `GetSettlementSummary`) tidak punya penanganan apa pun
  selain `GetSettlementDetail` (satu-satunya yang menangani `KeyNotFoundException`, baris
  247-250). Pola tidak konsisten ini sendiri bukan bug, tetapi membuat exception apa pun dari
  path baca-banyak (`GetSettlement`/`GetSettlementSummary`) selalu jatuh ke 500 generik — usul
  pencegahan: pola `try/catch` yang sudah ada pada aksi lain di controller yang sama bisa dipakai
  sebagai acuan bila nanti ditemukan exception jenis tertentu yang sebetulnya layak dipetakan ke
  400/404/422, bukan 500.
- Pola "layar tidak tertelusur di blueprint" (`ISS-FIN-002-06`) berpotensi terjadi pada modul lain
  bila implementasi dikerjakan tanpa melalui chain `grill-me → ... → plan-module-delivery →
  build-module-backend/build-module-frontend`. Tidak ditemukan issue serupa sebelumnya karena ini
  adalah issue pertama pada blueprint `finance-management`.

---

## 8. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-11 | Issue dibuat dari laporan teknis 33 bagian (HTTP 500 + revisi UI/UX Settlement AR) | `diagnose-module-issue` |
| 2026-10-11 | P-02 terjawab dari `Logs/quilvian-backend-20261011.json`; K-01 diperlakukan disetujui Opsi (a) atas instruksi "kerjakan semua". `BE-FIN-102` (FIX-01+02) dan `FE-FIN-044` (FIX-03 s/d 08, 10, 11) dikerjakan. Koreksi `ISS-FIN-002-03` (footer sudah benar, tidak perlu perbaikan) dan `ISS-FIN-002-08` (premis opsi sort keliru, kode mati) dicatat langsung pada temuan masing-masing | agent pemanggil (sesi ini) |
