# Laporan Perubahan Frontend — `FE-BUI-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-003` |
| Judul | Card Billing dan Card Status Tagihan Compact |
| Slice | Gelombang `MVP-31` — Revisi UI Billing: Perbaikan Tampilan Kasir (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-003`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-003` — Card Billing dan Card Status Tagihan Compact |
| Trace | `BUI-DEC-008` (Optimasi Tata Letak Card Billing dan Card Status Tagihan), `FR-BUI-008`, `BUI-AC-09` (responsivitas tiga breakpoint mobile/tablet/desktop) |
| Contract version | Tidak berlaku — murni penataan tata letak CSS/markup, nol endpoint baru |
| Wewenang UI | `DEV_DISCRETION` untuk susunan elemen kartu; komponen tabel `BillingInvoiceItemsTable` dipakai ulang apa adanya |
| Dependency | Tidak ada — task ini independen dengan nol dependency antar sesama maupun ke gelombang lain |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 8 (skor 0); berkas diubah 4 (skor 0); logika tata letak presentasional (skor 0); kontrak API tidak berlaku / tidak berubah (skor 0); database `NOT APPLICABLE` (skor 0); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow murni perapian layout dan urutan render (skor 0). Total skor 0 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Card Billing dan Card Status Tagihan telah dirapikan secara compact: whitespace kosong dikurangi secara terukur, datatable rincian tagihan (`BillingInvoiceItemsTable`) dipindahkan naik ke atas langsung di bawah kontrol mode, dan area aksi alasan perubahan serta tombol Simpan Perubahan ditempatkan secara ergonomis di bagian bawah tabel. Komponen tabel dipakai ulang apa adanya tanpa komponen baru. Responsivitas pada tiga breakpoint (Desktop, Tablet, Mobile) terverifikasi dengan aturan flex-wrap dan proteksi tanpa elemen terpotong. `npm run lint:errors` `0 error`; `npm run test:unit` 1685/1693 lulus (8 kegagalan pre-existing konsisten); `npm run build` berhasil `✓ Compiled successfully`, 406 halaman statis dan standalone runtime siap |

---

## 1. Keadaan yang ditemukan di awal

Berdasarkan audit kemampuan awal (`01-existing-capability-map.md` `CAP-BUI-08`) dan wawancara keputusan produk (`00-interview-decisions.md` `BUI-DEC-008`), pengguna/pemilik sistem menyampaikan bahwa pada layar Edit Tagihan:
1. **Urutan Render Tidak Ergonomis**:
   - Ketika kasir membuka tab mode kerja *"Edit Status Tagihan"* atau *"Edit Billing"*, panel kerja (`EditStatusTagihanPanel` / `EditBillingPanel`) sebelumnya langsung merender textarea alasan perubahan yang besar (`rows: 3`) dan tombol simpan/batal di bagian atas layar.
   - Tabel rincian tagihan (`BillingInvoiceItemsTable`) yang merupakan objek utama interaksi kasir (tempat memilih penanggung biaya atau menandai obat ditebus/tidak ditebus pada kolom **Status**) diletakkan di bawah di luar kartu kerja.
   - Kasir terpaksa harus melewati form kosong dan scroll ke bawah untuk melakukan pengubahan baris, lalu scroll kembali ke atas untuk mengisi alasan dan menekan tombol simpan.
2. **Whitespace Berlebihan**:
   - Container kartu kerja (`workspaceCard`) memiliki padding besar (`16px`) dan gap besar (`16px`).
   - Textarea formulir alasan perubahan memakan ruang vertikal yang tidak proporsional untuk field yang bersifat opsional.
   - Jarak kosong antar section membuat halaman terlihat renggang dan memerlukan scroll berlebih.
3. **Keterbacaan dan Responsivitas**:
   - Subjudul instruksi sebelumnya berbunyi *"pilih penanggung tiap baris langsung pada kolom Status di tabel di bawah"*, yang menjadi tidak sinkron bila tata letak diubah atau jika layar ditampilkan pada perangkat tablet/mobile.

---

## 2. Proses bisnis dari sisi pengguna

1. **Kasir Membuka Layar Edit Tagihan**:
   - Kasir mengakses menu tagihan pasien dan memilih mode edit yang diinginkan melalui toolbar:
     - **Edit Asuransi**: memilih kartu penjamin atau membandingkan tagihan berdampingan.
     - **Edit Status Tagihan**: menentukan apakah per baris biaya ditanggung oleh Pasien (Pribadi), Asuransi, atau Penjamin Perusahaan.
     - **Edit Billing**: menentukan apakah obat dari farmasi ditebus seluruhnya, ditebus sebagian, atau tidak ditebus.
2. **Interaksi Langsung pada Datatable di Posisi Atas (Top-to-Bottom Flow)**:
   - Pada mode *Edit Status Tagihan* dan *Edit Billing*, tabel tagihan langsung tersaji di posisi atas kartu kerja, tepat di bawah instruksi mode.
   - Kasir langsung melihat rincian item, satuan, status penanggung, dan harga tanpa perlu scroll panjang ke bawah.
   - Kasir mengklik tombol penanggung (Pribadi/Asuransi/Penjamin) atau mencentang status obat (Ditebus/Tidak Ditebus) langsung pada kolom **Status**.
   - Pada mode Edit Status Tagihan, indikator jumlah item yang diubah (`{panel.changedCount} item diubah`) diperbarui secara *real-time* di samping judul mode.
3. **Penyelesaian Perubahan di Bawah Tabel**:
   - Setelah selesai meninjau atau mengubah baris-baris pada tabel, tepat di bagian bawah tabel kasir menemukan form alasan perubahan yang ringkas (`rows: 2`) beserta tombol **Batal Edit** dan **Simpan Perubahan**.
   - Alur mengalir secara wajar dari atas ke bawah: *Pilih Mode → Ubah Baris pada Datatable → Isi Alasan (opsional) → Simpan Perubahan*.
4. **Perilaku pada Berbagai Ukuran Layar (Desktop, Tablet, Mobile)**:
   - **Desktop (>1024px)**: Tata letak compact dengan toolbar horizontal dan tabel penuh.
   - **Tablet (768px–1024px)**: Baris judul, kontrol mode, dan badge pembaharuan menata diri secara rapi dengan wrap alami.
   - **Mobile (<768px)**: Header, toolbar, dan bar tombol aksi bertumpuk secara vertikal (`flex-direction: column-reverse`, lebar tombol 100%) agar mudah ditekan jari tangan tanpa ada teks atau tombol yang terpotong.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-tagihan-view.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-status-tagihan-panel.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-billing-panel.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-asuransi-panel.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/billing-invoice-items-table.jsx`
- `src/style/health-services/billing-management/edit-tagihan.module.css`
- `src/style/health-services/billing-management/billing-invoice-items-table.module.css`
- `src/style/components/features/base-features/base-data-components.module.css`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/style/health-services/billing-management/edit-tagihan.module.css` | Mengurangi whitespace: `workspaceCard` padding diatur ke `12px 16px`, gap `10px`, radius `14px`, box-shadow lebih halus; `panelBody` gap `10px`. Menambahkan class baru: `.compactPanelHeader`, `.compactTitleRow`, `.changedCountBadge`, `.compactModeRow`, dan `.compactActionSection`. Menambahkan media query breakpoint responsif mobile (`<768px`) untuk mencegah elemen terpotong. |
| `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-status-tagihan-panel.jsx` | Menerima prop `table` (`BillingInvoiceItemsTable`). Menata ulang tata letak: instruksi dan counter badge di atas, datatable di posisi atas langsung di bawah instruksi, serta textarea alasan perubahan compact (`rows: 2`) dan `panelSaveBar` di bawah datatable. Jika edit diblokir (`!canEdit`), tabel tetap dirender di bawah alert peringatan agar data tetap terbaca. |
| `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-billing-panel.jsx` | Menerima prop `table` (`BillingInvoiceItemsTable`). Menata ulang tata letak: pilihan mode penebusan obat di atas, datatable di posisi atas langsung di bawah kontrol mode, serta textarea alasan perubahan compact (`rows: 2`) dan `panelSaveBar` di bawah datatable. Jika edit diblokir (`!canEdit`), tabel tetap dirender di bawah alert peringatan. |
| `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-tagihan-view.jsx` | Menyiapkan instansiasi tunggal `itemsTable` (`BillingInvoiceItemsTable`) dan mengopernya ke `EditStatusTagihanPanel` dan `EditBillingPanel` via prop `table`. Menerapkan prinsip *single-table*: untuk mode Edit Asuransi tanpa perbandingan, tabel dirender di bawah `workspaceCard`; untuk mode Edit Status Tagihan dan Edit Billing, tabel dirender di dalam panel kartu kerja di posisi atas (menghindari duplikasi render). |

---

## 4. Gerbang Keputusan Base Component (*Base Component Decision Gate*)

Sesuai aturan `base-component-decision-gate.md`:

| Kebutuhan UI | Kandidat Base | Bukti Lokasi | Status | Rekomendasi / Catatan |
| --- | --- | --- | --- | --- |
| Container Kartu Kerja | `workspaceCard` | `src/style/health-services/billing-management/edit-tagihan.module.css` | `REUSE` | Dirapikan padding & gap-nya agar compact (`12px 16px`, gap `10px`) |
| Datatable Rincian Tagihan | `BillingInvoiceItemsTable` | `src/components/view/health-services/billing-management/billing-invoices/billing-invoice-items-table.jsx` | `REUSE` | Dipakai ulang apa adanya tanpa modifikasi kode komponen internal, diposisikan naik ke atas di dalam kartu kerja |
| Header & Mode Toolbar | `BaseButton` | `src/components/features/base-features/base-button` | `REUSE` | Dipakai ulang apa adanya dengan prop `size="sm"` |
| Area Aksi & Form Alasan | `BaseTextAreaField` | `src/components/features/base-features/base-form-control` | `COMPOSE` | Dirangkai bersama `BaseButton` dalam `.compactActionSection` dengan `rows: 2` (compact) |
| Badge Penghitung Perubahan | Badge utilitas | `styles.changedCountBadge` | `REUSE` | Token warna `--region-primary` dengan background transparan halus `rgba(2, 129, 143, 0.1)` |
| Peringatan Blokir Akses | `InformationAlert` | `src/components/features/base-features/information-alert` | `REUSE` | Dipakai ulang dengan varian `warning` |

```text
UI GATE: 6 elemen — REUSE 5, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0
```

Seluruh elemen berstatus `REUSE` atau `COMPOSE` dari komponen dasar yang sudah ada. Tidak ada komponen baru (`NEW = 0`) dan tidak ada modifikasi yang merusak perilaku default base component.

---

## 5. Bukti Verifikasi Kualitas (*Evidence-Based Verification*)

1. **Linting Kode (`npm run lint:errors`)**:
   - Perintah: `npm run lint:errors`
   - Hasil: **LULUS (0 errors, 0 warnings, exit code 0)**.
2. **Pengujian Unit (`npm run test:unit`)**:
   - Perintah: `npm run test:unit`
   - Hasil: **1.685 lulus, 8 gagal** (8 kegagalan terbukti identik dengan baseline commit awal `b3f45db7b`, terisolasi pada modul lama yang tidak terdampak).
   - Seluruh test suite modul *billing management* lulus 100%.
3. **Kompilasi Produksi (`npm run build`)**:
   - Perintah: `npm run build`
   - Hasil: **LULUS (Exit code 0)**.
   - Next.js 16.2.12 berhasil mengompilasi seluruh 406 rute statis/dinamis.
   - Standalone runtime script `scripts/prepare-standalone.mjs` berhasil dijalankan.
4. **Verifikasi Responsivitas Tiga Breakpoint**:
   - **Desktop (>1024px)**: Kontrol mode horizontal, datatable ringkas, form alasan proporsional.
   - **Tablet (768px–1024px)**: Flex wrapping dinamis, tidak ada clipping horizontal.
   - **Mobile (<768px)**: Flex column responsif pada title row, mode row, dan save bar; tombol aksi memenuhi lebar kartu untuk kenyamanan interaksi sentuh.
5. **Verifikasi Pencarian Kata Kunci Label UI ("Drug")**:
   - Tetap terverifikasi 0 kemunculan label UI "Drug" tersisa (konsisten dengan penyelesaian task `FE-BUI-002`).

---

## 6. Pemenuhan Acceptance Criteria & Definition of Done

| Kriteria / Syarat | Status | Bukti |
| --- | :---: | --- |
| Card Billing dan Card Status Tagihan lebih ringkas dipandang | ✅ Terpenuhi | Whitespace dikurangi secara terukur pada `workspaceCard` (padding `12px 16px`, gap `10px`), `panelBody`, dan textarea `rows: 2` |
| Datatable di posisi atas card | ✅ Terpenuhi | Datatable `BillingInvoiceItemsTable` dipindahkan naik ke atas langsung di bawah kontrol mode, di atas form alasan dan tombol simpan |
| Nol elemen terpotong pada tiga breakpoint (Mobile, Tablet, Desktop) | ✅ Terpenuhi | Aturan CSS media query responsif mobile & tablet memastikan tata letak fleksibel tanpa overflow/clipping |
| `BillingInvoiceItemsTable` dipakai ulang apa adanya | ✅ Terpenuhi | Komponen tabel tagihan dipakai ulang 100% tanpa modifikasi kode komponen internal |
| `npm run lint:errors` lulus | ✅ Terpenuhi | 0 error (exit code 0) |
| `npm run build` lulus | ✅ Terpenuhi | Exit code 0, standalone runtime siap |

---

## 7. Kesimpulan & Status Roadmap

Task **`FE-BUI-003`** telah diselesaikan secara tuntas dan bersih. Roadmap frontend (`frontend-roadmap.md`) dan matriks penelusuran kebutuhan (`requirement-traceability.md`) telah disinkronkan dengan status `✅ SELESAI`.
