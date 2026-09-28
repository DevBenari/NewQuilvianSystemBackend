# Laporan Perubahan Frontend — `FE-BKC-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-042` |
| Judul | Panel Ringkasan Rawat Inap & Financial Clearance pada Menu Pembayaran Kasir |
| Slice | Gelombang 1 (`MVP-29`), `frontend-roadmap.md` |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` — bagian Task `FE-BKC-042` |
| Trace | `BKC-DEC-112`, `BKC-DEC-113`, `BKC-DEC-114`, `BKC-DEC-117`, `BKC-DEC-119`, `BKC-DEC-120`, `BKC-DES-043`, `BKC-DES-047`, `BKC-DES-049`, `FR-BKC-241`, `FR-BKC-242`, `FR-BKC-243`, `FR-BKC-244`, `FR-BKC-246`, `FR-BKC-247`, `FR-BKC-248`, `FR-BKC-250`, `03-frontend-architecture.md` Bagian 3 |
| Contract version | `BIL-API-1.4`, `BIL-STATE-1.3` — keduanya `approved` |
| Wewenang UI | Terkunci: keberadaan panel ringkasan rawat inap saat invoice bertipe `RANAP`, kosakata badge kelayakan (`CLEARED`, `BLOCKED`, `REVOKED`, `PENDING`), warna indikator, teks peringatan Auto-Reblock, batas plafon administrasi Rp6.000.000, penanda badge `[IGD]`. `DEV_DISCRETION`: tata letak kartu grid, margin, padding, dan ikon dekoratif |
| Dependency | `[BE] BE-BKC-076` — 🟡 selesai 24 September 2026 (controller integrasi rawat inap menyediakan endpoint ringkasan billing ranap dan evaluasi clearance). Lihat [laporan](../backend/BE-BKC-076.md) |
| Klasifikasi | `MEDIUM` — integrasi inquiry summary ranap, visualisasi financial clearance 4-state, animasi auto-reblock pulsating warning, rincian durasi hunian kamar pro-rata menit & tarif bertingkat, rincian biaya administrasi 7% cap Rp6.000.000, rincian kredit administrasi rawat jalan, dan penandaan visual badge `[IGD]` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — berkas komponen panel, hook, constants, slice, view menu pembayaran, view detail invoice, tabel item invoice, styling module, dan unit test. Laporan ini ditulis di `NewQuilvianSystemBackend` sesuai wewenang lintas repository sempit yang diberikan `AGENTS.md` frontend § Pelaporan Task Modul |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend saat dikerjakan | `0f022e0c45dd0b941ba83b01e2923944de49b201` |
| Commit backend yang dijadikan rujukan | `59910dc0ac04c7e99de36f9b299e3024c6f86a39` |
| Tanggal | 24 September 2026 |
| Status | 🟡 **SEBAGIAN — source dan validasi statis selesai penuh; 6/6 unit test lulus; npm run build lulus (exit code 0).** Seluruh acceptance criteria terpetakan ke source kode. Yang masih menahan `✅` adalah uji visual interaktif manual ter-autentikasi pada backend runtime aktif. |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task `FE-BKC-042` dikerjakan, layar Menu Pembayaran (`/health-services/billing-management/billing/menu-pembayaran`) dan Rincian Tagihan (`/health-services/billing-management/billing/invoices/[id]`) dirancang untuk alur kasir rawat jalan umum (*outpatient*):
1. **Tidak Ada Pembeda Tipe Pelayanan Rawat Inap:** Ketika kasir membuka invoice pasien rawat inap (`serviceType === "RANAP"`), layar menampilkan rincian flat standar poliklinik tanpa ringkasan durasi hunian kamar dan tanpa indikator kelayakan pemulangan (*Financial Clearance*).
2. **Ketiadaan Visualisasi Status Clearance:** Kasir tidak dapat mengetahui apakah pasien telah memenuhi syarat administratif untuk pulang (`CLEARED`), tertahan sisa tagihan/deposit (`BLOCKED`), atau izin pulangnya telah dibatalkan otomatis sistem karena adanya tagihan susulan baru (`REVOKED`) (`BKC-DEC-115`, `BKC-DEC-116`, `BKC-DES-046`).
3. **Perhitungan Kamar Belum Transparan:** Durasi sewa kamar bertingkat (*tier rate* jam masuk malam 22:00–06:00, kepindahan kelas kamar pro-rata menit riil) hanya tampak sebagai nominal tunggal kotor tanpa uraian jam:menit dan pengali tarif (`BKC-DEC-112`, `BKC-DEC-113`, `BKC-DES-043`).
4. **Biaya Administrasi Rawat Inap & Kredit Rajal:** Kasir tidak melihat rincian kalkulasi biaya administrasi 7% dengan pembatasan maksimal (*cap*) Rp6.000.000, serta tidak melihat pemotongan kredit biaya administrasi rawat jalan/IGD yang telah dibayar sebelumnya pada episode yang sama (`BKC-DEC-114`, `BKC-DEC-119`, `BKC-DES-047`).
5. **Ketiadaan Penanda Asal Layanan IGD:** Item tindakan atau resep obat yang dialihkan dari Instalasi Gawat Darurat (IGD) tercampur baur dengan tindakan bangsal ranap tanpa label penanda visual `[IGD]`, menyulitkan keluarga pasien dan perawat saat memverifikasi rincian biaya (`BKC-DEC-117`, `BKC-DES-049`).

---

## 2. Proses bisnis dari sisi pengguna

Pengguna utama dari antarmuka ini adalah **Petugas Kasir Rumah Sakit**, **Staf Administrasi Rawat Inap (Admission/Discharge)**, dan **Keluarga Pasien / Penjamin**:

### Skenario Nyata Rumah Sakit:
> **Contoh Kasus:** Pasien Bpk. Hendra masuk melalui IGD pada malam hari pukul 23:15 WIB (mendapat penanganan awal dan obat IGD). Pukul 02:30 WIB pasien dipindahkan ke Bangsal Rawat Inap Kelas 1 (masuk jendela jam malam). Dua hari kemudian kondisi membaik dan pasien meminta naik ke Kamar VIP pada pukul 14:30 WIB. Sebelum masuk tindakan operasi sedang, keluarga membayar deposit Rp5.000.000. Saat dokter menginstruksikan boleh pulang (KRS), kasir membuka invoice rawat inap Bpk. Hendra.

### Alur Normal Penggunaan:
1. **Identifikasi Pasien Rawat Inap:**
   - Kasir membuka Menu Pembayaran dan memilih invoice Bpk. Hendra.
   - Sistem mendeteksi bahwa tipe pelayanan adalah `"RANAP"` (`serviceType === "RANAP"`).
   - Hook `useInpatientBillingSummary` secara otomatis memicu query `GET /invoices/encounter/{encounterId}/inpatient-summary`.
2. **Inspeksi Banner Financial Clearance (Wilayah D):**
   - Panel Rawat Inap muncul mencolok di bagian atas layar pembayaran (`BillingInpatientSummaryPanel`).
   - Bila pasien masih memiliki kekurangan deposit atau sisa tagihan, banner berwarna merah menyala (`BLOCKED`) dengan keterangan jelas:
     - *"Pasien belum dapat dipulangkan karena masih memiliki kendala administrasi/finansial yang belum diselesaikan."*
     - Ditampilkan daftar blocker konkret: sisa tagihan berjalan Rp1.850.000 dan kekurangan deposit tindakan Rp2.000.000 (`BIL-VAL-121`).
3. **Pemeriksaan Rincian Sewa Kamar & Biaya Administrasi (Wilayah E):**
   - **Rincian Kamar:** Menampilkan baris pergerakan kamar:
     - Segmen 1: Kelas 1, durasi 38 jam 15 menit, jam malam (pengali tarif khusus 22:00–06:00).
     - Segmen 2: VIP, durasi 24 jam 0 menit, tarif penuh.
     - Tertera kalkulasi pro-rata durasi menit riil transparan.
   - **Biaya Administrasi 7% & Plafon:**
     - Total bruto tagihan dihitung 7%.
     - Jika nominal 7% melebihi Rp6.000.000, sistem menampilkan badge lencana *"Plafon Maksimal Diterapkan (Maks Rp6.000.000)"*.
   - **Kredit Biaya Administrasi Rawat Jalan:**
     - Tertera baris kredit: *"Potongan Biaya Administrasi Rawat Jalan/IGD Sebelumnya: -Rp50.000"*, mencegah pasien ditagih administrasi dua kali atas rujukan dari unit rawat jalan/IGD pada episode yang sama (`BKC-DEC-119`).
4. **Pemeriksaan Item Asal IGD (Wilayah F):**
   - Pada tabel rincian transaksi maupun ringkasan IGD terpadu, item tindakan dokter IGD dan farmasi IGD ditandai secara visual dengan lencana amber `[IGD]`.
   - Menampilkan subtotal tagihan IGD terpisah untuk kemudahan klarifikasi pasien.
5. **Penyelesaian Pembayaran & Kelayakan Pulang:**
   - Kasir memproses pelunasan sisa tagihan dan mencocokkan saldo deposit.
   - Setelah tagihan berstatus lunas (`LUNAS`) dan deposit terpenuhi, status clearance otomatis beralih menjadi hijau (`CLEARED / LAYAK PULANG`).
   - Perawat bangsal dan kasir mendapat kepastian bahwa berkas kepulangan (*surat izin pulang*) dapat diserahkan ke keluarga pasien.

### Alur Tidak Normal / Pengecualian (Auto-Reblock / REVOKED):
- **Tagihan Susulan Tiba Setelah Pasien Dinyatakan Lunas (`BKC-DEC-116`, `BKC-DES-046`):**
  - Pasien telah dinyatakan lunas dan berstatus `CLEARED`. Namun sesaat sebelum meninggalkan rumah sakit, Instalasi Farmasi atau Laboratorium menginput tagihan susulan atas pemeriksaan darah tambahan.
  - Begitu invoice mendeteksi penambahan item baru saat masih `OPEN`, backend mencabut izin pulang dan status clearance berubah menjadi `REVOKED`.
  - Di layar kasir, banner Financial Clearance berubah menjadi **Amber/Merah Berkedip (*Pulsating Animation*)** dengan teks peringatan keras:
    > *"PERINGATAN: Tagihan susulan baru saja tercatat setelah pasien dinyatakan lunas. Izin pulang dicabut otomatis. Minta pasien menyelesaikan sisa tagihan."*
  - Kasir segera menghentikan proses pemulangan dan meminta pasien menyelesaikan selisih tagihan susulan tersebut.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
- `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` (`FE-BKC-042`)
- `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (`BIL-API-1.4`)
- `docs/module-blueprints/billing-kasir/architecture/03-frontend-architecture.md` (Bagian 3)
- `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs`
- `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/DTOs/InpatientBillingSummaryResponse.cs`
- `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/billing-invoice-items-table.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js` | Menambahkan konstanta `INPATIENT_SERVICE_TYPE = "RANAP"`, status kelayakan `INPATIENT_CLEARANCE_STATUSES` (`CLEARED`, `BLOCKED`, `REVOKED`, `PENDING`), serta konfigurasi lencana visual `INPATIENT_CLEARANCE_STATUS_CONFIG`. |
| `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx` | Menambahkan async thunk `getInpatientBillingSummary` yang memanggil `GET /v1/health-services/billing-management/billing/invoices/encounter/{encounterId}/inpatient-summary`, state `inpatientSummary`, `inpatientSummaryLoading`, `inpatientSummaryError`, aksi `clearInpatientBillingSummary`, serta selectors `selectInpatientBillingSummary`, `selectInpatientBillingSummaryLoading`, `selectInpatientBillingSummaryError`. |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-inpatient-billing-summary.js` | Hook kustom baru untuk memicu inquiry summary ranap berbasis `encounterId`, menyajikan helper status kepulangan (`isCleared`, `isBlocked`, `isRevoked`, `canDischarge`), daftar alasan penahanan (*blocker reasons*), dan informasi defisit deposit (*deposit shortfall*). |
| `src/components/view/health-services/billing-management/billing-invoices/detail/billing-inpatient-summary-panel.jsx` | Komponen panel komprehensif baru: Wilayah D (Banner Financial Clearance, status badge, peringatan Auto-Reblock, daftar blocker, catatan defisit deposit per `BIL-VAL-121`), Wilayah E (Rincian sewa kamar pro-rata menit, biaya administrasi 7% berplafon Rp6.000.000, potongan kredit administrasi rawat jalan), dan Wilayah F (Rincian konsolidasi item IGD beserta subtotal). |
| `src/components/view/health-services/billing-management/billing-invoices/detail/billing-inpatient-summary-panel.module.css` | Styling module baru dengan token sistem Quilvian: animasi pulsating `pulseRevoked`, tata letak kartu responsif grid 2-kolom, styling tabel flat `[data-flat-table="true"]`, lencana tag pill, dan typography hirarkis. |
| `src/components/view/health-services/billing-management/billing-invoices/billing-invoice-items-table.jsx` | Memasang badge visual `[IGD]` pada baris item tagihan jika `item.sourceDomain === "EMERGENCY"` atau `item.SourceDomain === "EMERGENCY"`. |
| `src/style/health-services/billing-management/billing-invoice-items-table.module.css` | Menambahkan selector styling `.igdBadge` dengan warna amber berpenanda jelas. |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` | Mengintegrasikan hook `useInpatientBillingSummary` dan menampilkan `BillingInpatientSummaryPanel` tepat di atas grid transaksi pembayaran kasir ketika `isRanap` bernilai `true`. |
| `src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.jsx` | Mengintegrasikan hook `useInpatientBillingSummary`, merender `BillingInpatientSummaryPanel` saat `isRanap`, dan menyematkan badge `[IGD]` pada tabel rincian transaksi terkelompok. |
| `src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.module.css` | Menambahkan selector styling `.igdBadge` untuk keseragaman visual pada halaman rincian tagihan. |
| `tests/unit/billing-inpatient-summary-panel.test.mjs` | Berkas pengujian unit otomatis komprehensif memvalidasi 6 kriteria penerimaan (AC-1 s.d. AC-6). |

### 3.3 Gerbang Keputusan Base Component (Base Component Decision Gate)

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Banner Clearance & Alert | `InformationAlert` / Semantic Alert Card | `src/components/features/base-features/information-alert` | `REUSE` | Digunakan kembali dengan varian alert status (hijau, merah, amber pulse). |
| Lencana Status Clearance | `StatusBadge` | `src/components/features/base-features/status-badge` | `REUSE` | Memakai `INPATIENT_CLEARANCE_STATUS_CONFIG` (`CLEARED`: active/hijau, `BLOCKED`: inactive/merah, `REVOKED`: warning/amber pulse). |
| Lencana Penanda `[IGD]` | Semantic Tag Pill / Badge Token | `src/components/view/health-services/billing-management/billing-invoices/` | `COMPOSE` | **Opsi A (Rekomendasi):** Menggunakan span semantik kelas `.igdBadge` dengan token warna amber sistem. Ringan, tidak menambah dependensi komponen berat. |
| Panel Rincian Sewa Kamar & Admin | Token Card & Semantic Flat Table | `src/components/view/health-services/billing-management/billing-invoices/detail` | `COMPOSE` | Merangkai kartu grid 2-kolom responsif dengan tabel semantik bertanda `data-flat-table="true"` sesuai aturan anti-regresi tabel. |
| Tabel Item Invoice Utama | `BillingInvoiceItemsTable` | `src/components/view/health-services/billing-management/billing-invoices/` | `EXTEND` | Menambahkan kolom penanda visual `[IGD]` pada nama item tindakan/layanan yang berasal dari instalasi gawat darurat. |

Ringkasan:
```text
UI GATE: 5 elemen — REUSE 2, EXTEND 1, COMPOSE 2, WRAP 0, NEW 0
```

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat (*Loading*) | Banner status clearance menampilkan indikator biru tenang: *"Memuat status kelayakan pemulangan..."*, skeleton/loading placeholder aktif. |
| Layak Pulang (*CLEARED*) | Banner hijau tegas: *"STATUS KELAYAKAN PEMULANGAN: [ CLEARED / LAYAK PULANG ]"*, dengan pesan bahwa seluruh kewajiban administrasi dan finansial telah tuntas. |
| Belum Layak (*BLOCKED*) | Banner merah: *"STATUS KELAYAKAN PEMULANGAN: [ BLOCKED / BELUM LAYAK PULANG ]"*, disertai rincian daftar kendala penahanan (*blocker list*) dan informasi kekurangan deposit tindakan. |
| Izin Dicabut Otomatis (*REVOKED*) | Banner kuning-amber berkedip (*pulsating warning*): *"PERINGATAN: Tagihan susulan baru saja tercatat setelah pasien dinyatakan lunas. Izin pulang dicabut otomatis. Minta pasien menyelesaikan sisa tagihan."*, sisa tagihan susulan ditampilkan dengan warna merah tebal. |
| Bukan Pasien Rawat Inap (`serviceType !== "RANAP"`) | Panel rawat inap tidak dirender (`null`), layar pembayaran kembali ke tata letak rawat jalan standar tanpa gangguan visual. |
| Gagal Memuat Data (*Error*) | Banner merah ramah menginfokan kegagalan inquiry summary ranap tanpa merusak fungsi pembayaran utama kasir. |

---

## 5. Endpoint yang dikonsumsi

#### [Tags("BillingInvoices")]
Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/encounter/{encounterId}/inpatient-summary` | Mengambil data agregasi ringkasan rawat inap: status financial clearance, rincian kamar, sewa pro-rata menit, biaya administrasi 7%, kredit admin rajal, deposit shortfall, dan item IGD. | `BillingInvoices : Read` / `BillingInpatient : Read` |
| `GET` | `/{id}` | Mengambil detail faktur utama termasuk seluruh daftar item, rincian breakdown sewa kamar, dan status pelunasan. | `BillingInvoices : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint src/components/... src/lib/...` | Lulus tanpa kesalahan (0 error, 0 warning) | `PASS` | Lint pada 8 berkas fitur yang disentuh |
| `node --test tests/unit/billing-inpatient-summary-panel.test.mjs` | Seluruh 6 subtest lulus penuh (AC-1 s.d. AC-6) | `PASS` | 6 tests passed, duration 91ms |
| `npm run build` | Kompilasi Next.js 16 (Turbopack) sukses penuh, 406 halaman statis tergenerasi, output standalone siap | `PASS` | Exit code 0, build berhasil dalam 41 detik |
| Grep Anti-Regresi `!important` | 0 kemunculan `!important` | `PASS` | Bebas override paksa |
| Grep Anti-Regresi Tabel Mentah Tanpa Atribut | 0 tag tabel ilegal; tabel internal terproteksi `data-flat-table="true"` | `PASS` | Sesuai aturan arsitektur UI |
| Uji Manual Browser Interaktif Ter-autentikasi | Pengujian visual desktop/tablet dan interaksi langsung dengan backend live | `NOT FEASIBLE` | Membutuhkan backend API server dan database migrasi runtime aktif |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria / Syarat DoD | Status | Bukti |
| --- | --- | --- |
| Banner status clearance tampil mencolok di atas ringkasan pembayaran saat tipe `RANAP` | Terpenuhi | Terpasang `BillingInpatientSummaryPanel` pada `menu-pembayaran-view.jsx` dan `detail-invoice-billing-view.jsx` saat `isRanap`, terbukti pada AC-4 & AC-6. |
| Peringatan auto-reblock (`REVOKED`) muncul jika ada tagihan susulan setelah lunas | Terpenuhi | Banner pulsating warning memuat teks *"PERINGATAN: Tagihan susulan baru saja tercatat setelah pasien dinyatakan lunas. Izin pulang dicabut otomatis. Minta pasien menyelesaikan sisa tagihan."*, terbukti pada AC-4. |
| Rincian kamar menampilkan durasi jam:menit riil dan kalkulasi sewa kamar pro-rata | Terpenuhi | Tabel rincian kamar menampilkan jam & menit riil terformat dari durasi menit backend, terbukti pada AC-4. |
| Rincian administrasi menampilkan 7% dengan penanda plafon maksimal Rp6.000.000 | Terpenuhi | Ditampilkan indikator lencana *"Plafon Maksimal Diterapkan (Maks Rp6.000.000)"* bila `isCapApplied` bernilai true, terbukti pada AC-4. |
| Biaya admin rajal terbayar tampil sebagai pengurang tagihan (kredit administrasi) | Terpenuhi | Ditampilkan baris pengurang kredit *"Potongan Biaya Administrasi Rawat Jalan/IGD Sebelumnya: -Rp..."*, terbukti pada AC-4. |
| Item tindakan/obat asal instalasi gawat darurat memiliki penanda visual `[IGD]` | Terpenuhi | Terpasang badge visual `[IGD]` pada baris item tagihan di `BillingInvoiceItemsTable` dan `DetailInvoiceBillingView`, terbukti pada AC-5. |
| Panel rawat inap sinkron saat nomor encounter/invoice berubah; `npm run build` lulus | Terpenuhi | Hook bereaksi terhadap perubahan props `encounterId` / `invoice`; build Next.js lulus dengan exit code 0. |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada error/warning baru pada seluruh berkas yang dimodifikasi. |
| Masalah yang diketahui | Uji manual live browser ter-autentikasi ditandai `NOT FEASIBLE` pada sesi ini karena backend runtime dan migrasi database terpadu dijalankan secara terpisah. |
| Dependency backend | `BE-BKC-076` telah terpasang di backend dan menyediakan endpoint agregasi ringkasan ranap. |
| Perubahan sampingan | `NONE` — tidak ada refactor oportunistik maupun penghapusan file di luar scope task. |
| Interupsi | `NONE`. |
| Status Git | Seluruh perubahan terisolasi rapi pada komponen menu pembayaran, rincian tagihan, panel ringkasan, hook, slice, dan test terkait. |
| Langkah berikutnya | Gelombang 1 (`MVP-29`: `FE-BKC-041` dan `FE-BKC-042`) kini telah selesai diimplementasikan secara komprehensif, siap untuk pengujian end-to-end terpadu pada lingkungan staging. |
