# Laporan Perubahan Backend — `BE-BKC-FIX-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-004` (ad-hoc, di luar roadmap, dua keputusan disetujui terpisah lewat `AskUserQuestion` pada task yang sama) |
| Judul | (1) Item tanpa rule asuransi SAMA SEKALI kini otomatis Mandiri (bukan unresolved); (2) PPN atas Obat/Alkes kini juga mempertimbangkan rawat jalan vs rawat inap, bukan cuma kategori |
| Slice | Lanjutan investigasi laporan bug pengguna atas invoice Allianz nyata, setelah `BE-BKC-FIX-003`/`FE-BKC-FIX-008` |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan endpoint/skema; murni perubahan interpretasi bisnis pada logika yang sudah ada |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — sudah terdaftar. Keberlakuan: `TOUCHED LEGACY` |
| Dependency | `BE-BKC-FIX-003` (task ini melanjutkan source yang sama) |
| Klasifikasi | `HIGH` — mengubah keputusan bisnis inti waterfall coverage (`BE-BKC-021`/`BKC-DEC-062`, sebagian) dan basis PPN, keduanya berdampak finansial langsung |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `BillingCoverageAdapter.cs`, `BillingCalculationService.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai. Build/test **TIDAK dijalankan** (instruksi eksplisit pengguna). **Belum diverifikasi hidup** — menunggu rebuild |

---

## 1. Masalah

Pengujian langsung `BE-BKC-FIX-003`/`FE-BKC-FIX-008` pada invoice Allianz nyata menemukan dua hal
yang pengguna anggap masih salah, KEDUANYA soal interpretasi bisnis (bukan bug kalkulasi):

### 1.1 "Tidak ada rule" seharusnya Mandiri, bukan unresolved

Item "Konsultasi Dokter Umum Rajal" (Procedure) tidak punya rule Allianz yang menyasarnya sama
sekali → sebelumnya jatuh ke "Menunggu Verifikasi" (unresolved, gating `BE-BKC-021`). Pengguna:
"Subtotal mandiri harusnya 150000 tanpa pajak. Yaitu item Konsultasi Dokter Umum Rajal" — bila
provider memang TIDAK punya rule untuk kategori ini sama sekali, tidak ada yang perlu diverifikasi
- itu otomatis tanggungan pasien. Disetujui lewat `AskUserQuestion` (opsi rekomendasi dipilih).
Item "ABBOTIC GRANUL..." (Drug) TETAP "Menunggu Verifikasi" setelah perbaikan ini — bedanya, Drug
memang belum punya rule Allianz APAPUN (beda dari Procedure yang SENGAJA tidak dicover) - begitu
pengguna menambahkan rule Allianz untuk kategori Drug/Pharmacy (aksi self-service yang sama dengan
Laboratory/Administration sebelumnya), item itu akan otomatis resolve ke Penjamin lewat rule
tersebut, bukan lewat perubahan ini.

### 1.2 PPN Obat/Alkes: rawat jalan vs rawat inap

Pengguna mengirim tabel klarifikasi:

| Kondisi | Kena PPN? |
| --- | --- |
| Obat/Alkes rawat jalan, dicover asuransi | Ya |
| Obat/Alkes rawat jalan, dibayar mandiri (excess) | Ya |
| Obat/Alkes rawat inap, dicover asuransi | Tidak (dibebaskan) |
| Obat/Alkes rawat inap, dibayar mandiri | Tidak (dibebaskan) |

Perbaikan PPN sebelumnya (`BE-BKC-FIX-003`'s pendahulu) hanya membatasi berdasarkan kategori
(`IsPharmacy`), belum mempertimbangkan rawat jalan/inap. Faktor penentu SESUNGGUHNYA adalah rawat
jalan vs rawat inap SAJA — status coverage (asuransi vs mandiri) TIDAK relevan untuk PPN.

---

## 2. Perubahan yang dikerjakan

### 2.1 `BillingCoverageAdapter.cs` — `ResolveAsync`

Cabang `if (rule is null)` (tidak ada rule yang cocok SAMA SEKALI untuk komponen ini) tidak lagi
menambah ke `unresolved` — outcome-nya jadi `(PrimaryAmount: 0, UnresolvedAmount: 0)`, yang berarti
porsi Patient implisit (`component.Amount - 0 - 0`) menjadi PENUH `component.Amount`, alias
langsung Mandiri. **Cabang-cabang LAIN yang tetap unresolved (TIDAK berubah oleh perbaikan ini):**
rule ada tapi `CoverageStatus="NeedApproval"` atau punya limit bulanan sungguhan; rule ada dengan
`CoverageStatus="NotCovered"` dan `IsAllowExcessPaymentByPatient=false`; rule ada tapi
`MaxAmountPerVisit` habis dan `IsAllowExcessPaymentByPatient=false`. Semua itu masih dianggap
butuh verifikasi manual seperti sebelumnya (`BE-BKC-021`/`BKC-DEC-062` tetap berlaku untuk
kasus-kasus itu) - perbaikan ini HANYA menyempit ke kasus "tidak ada rule apa pun yang menyasar
komponen ini".

### 2.2 `BillingCalculationService.cs` — `ApplyInvoiceTax`

Tambah parameter `bool isOutpatient`, dihitung sekali oleh pemanggil (`CalculateAsync`) dari
`invoice.ServiceType != AdministrationFeeServiceTypes.Ranap` (pola yang sama dengan pengecekan
RANAP untuk room charge yang sudah ada). Basis pajak sekarang kosong total (`return empty`)
bila `!isOutpatient` — jadi Obat/Alkes rawat inap TIDAK PERNAH kena PPN, terlepas dari status
coverage-nya (yang memang tidak pernah dijadikan faktor sejak awal - PPN dihitung dari kategori
item, bukan siapa yang membayar).

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Grep pemanggil `ApplyInvoiceTax` | Hanya 1 titik panggil, sudah diperbarui menyertakan `isOutpatientForTax` | `PASS` |
| Penelusuran manual: komponen tanpa rule (Procedure) | Outcome `(0,0)` → Patient implisit = `component.Amount` penuh - konsisten dengan permintaan "Subtotal Mandiri = 150.000 item Konsultasi" | `PASS` (analisis) |
| Penelusuran manual: komponen dengan rule tapi `NeedApproval`/limit bulanan/`NotCovered` | Cabang-cabang itu TIDAK disentuh sama sekali oleh perbaikan ini - tetap unresolved seperti sebelumnya | `PASS` (analisis) |

**`AUTOMATED TEST: BLOCKED`** — build/test tidak dijalankan. **`MANUAL TEST: BLOCKED`** — belum
di-rebuild pengguna.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko finansial | Perubahan 2.1 membuat SEMUA item tanpa rule (untuk provider manapun, bukan cuma Allianz) otomatis jadi tanggungan pasien alih-alih ditahan untuk verifikasi manual - ini keputusan bisnis yang disengaja (disetujui eksplisit), tapi berlaku GLOBAL untuk seluruh sistem asuransi, bukan cuma invoice yang dilaporkan. Perlu disadari: pasien dengan penjamin yang benar-benar TIDAK mengonfigurasi rule apa pun (mis. integrasi baru, provider baru belum sempat diisi rule-nya) akan langsung menagih PENUH ke pasien, bukan menahan untuk diperiksa finance/AR dulu |
| Belum diverifikasi hidup | Sama seperti `BE-BKC-FIX-003` - laporan ini berdasar penelusuran kode, bukan hasil kalkulasi nyata pasca-rebuild |
| Perubahan sampingan | `NONE` |
| Status Git | Modified: `BillingCoverageAdapter.cs`, `BillingCalculationService.cs`. Belum staged/commit |
| Langkah berikutnya | Rebuild backend, lalu verifikasi ulang invoice IKBAL YULIYANTO: Konsultasi → Tunai/Subtotal Mandiri Rp150.000; Drug tetap Menunggu Verifikasi sampai rule Drug/Pharmacy Allianz ditambahkan; pastikan PPN tetap kena untuk kasus ini (RAJAL, per gambar unit layanan "Rawat Jalan") |
