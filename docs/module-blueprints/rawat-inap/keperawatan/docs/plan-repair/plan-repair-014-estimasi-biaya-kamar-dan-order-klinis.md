# PLAN-REPAIR-014 — Perbaikan Konfigurasi Tarif Kamar ODC dan Penyempurnaan Petunjuk Alur Siklus Layanan Klinis pada Estimasi Biaya

```yaml
plan_id: PLAN-REPAIR-KEP-014
issue: ../issue/issue-014-estimasi-biaya-kamar-dan-order-klinis.md
status_rencana: DISETUJUI_SEBAGIAN
tanggal_rencana: "2026-10-20"
diputuskan_oleh: "Pengguna (Persetujuan eksekusi database Butir 1)"
tanggal_keputusan: "2026-10-20"
basis_source_backend: "6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd (MHamzah)"
basis_source_frontend: "a491b05afcb6b3ed70218ccaa557007719d45ac3 (HamzahV2)"
ditulis_dengan: "skill diagnose-module-issue"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | :---: | :---: | :---: | :---: | :---: |
| `FIX-KEP-014-01` | `ISS-KEP-014-01` | Pendaftaran dan konfigurasi data master tarif kamar ODC 321B pada Kelas ODC (`IsRoomCharge = true`) sebesar Rp 750.000/hari | Data / Backend | 1 | — | ✅ SELESAI | Database `QuilvianNewDevHamzah`: `MstTariff` ID `3e74e8c6-3a8b-4f33-83ed-80eea967650b` terverifikasi aktif |
| `FIX-KEP-014-02` | `ISS-KEP-014-02` | Penyempurnaan alur edukasi dan penegasan siklus hidup pesanan klinis menjadi fakta tagih | Dokumen / Operasional | 2 | — | BELUM DIKERJAKAN | — |
| `FIX-KEP-014-03` | `ISS-KEP-014-03` | Penambahan catatan informasi alur layanan klinis pada banner pedoman layar Estimasi Biaya | Frontend | 2 | — | BELUM DIKERJAKAN | — |

**Ringkasan: 1 dari 3 perbaikan selesai.**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-014-01 — Tarif Kamar Berstatus "Tarif Belum Ada"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Input master tarif kamar resmi di tabel `MstTariff` dengan `IsRoomCharge = true` untuk Unit Layanan ODC dan Kelas ODC. | Sesuai arsitektur backend, tidak merusak rumus penagihan, harga resmi rumah sakit terjaga. | Membutuhkan input data master oleh petugas berwenang. |
| B | Membuat fallback harga default otomatis (mis. Rp 0 atau nilai perkiraan) di dalam kode `BillingCalculationService`. | Tidak akan muncul status "Tarif Belum Ada". | **Dilarang keras oleh aturan rekayasa**: mengarang data finansial rumah sakit dan memicu sengketa hukum dengan penjamin/pasien. |

**Solusi terpilih: Opsi A.**
1. Mematuhi aturan tata kelola backend Quilvian di mana harga kamar wajib bersumber dari `MstTariff` yang valid.
2. Tidak mengubah logika kode backend penagihan yang sudah terverifikasi aman.

**Kenapa bukan opsi lain:** Opsi B melanggar prinsip konstitusi workspace Quilvian (*"AI tidak boleh mengarang keputusan bisnis atau tarif rumah sakit"*).

---

### ISS-KEP-014-02 & ISS-KEP-014-03 — Pesanan Klinis (Obat, Tindakan, Penunjang) Tidak Muncul di Estimasi Biaya

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| A | Pertahankan integritas penagihan (hanya pelayanan yang sudah selesai/diserahkan yang masuk tagihan), dan tambahkan petunjuk informatif pada banner di layar Estimasi Biaya bahwa pesanan klinis baru muncul setelah dieksekusi atau diserahkan oleh Farmasi. | Kepatuhan hukum dan audit rumah sakit 100% terjaga, mencegah klaim palsu/retur obat draf, tidak membingungkan pengguna. | Pengguna tetap harus menyelesaikan eksekusi pelayanan agar melihat biayanya di tabel. |
| B | Memaksa draf resep dan order tindakan yang belum dieksekusi langsung masuk ke tabel tagihan Estimasi Biaya. | Pengguna langsung melihat baris item begitu menekan simpan draf. | **Sangat berbahaya**: obat yang masih draf belum disetujui apoteker dan belum tentu diminum pasien; jika langsung masuk tagihan, pasien akan ditagih untuk obat yang tidak pernah diterima. |
| C | Membuat tab terpisah "Estimasi Pesanan Belum Selesai (Pending Orders)" di sebelah Estimasi Biaya Resmi. | Menunjukkan perkiraan biaya tanpa mencampurnya ke total tagihan resmi. | Menambah kompleksitas antarmuka dan membutuhkan endpoint baru serta persetujuan arsitektur. |

**Solusi terpilih: Opsi A.**
1. Menjaga batas keselamatan dan integritas finansial klinis (*Clinical Governance & Financial Compliance*).
2. Membimbing pengguna secara jelas lewat petunjuk tampilan (*clear UX guidance*) tanpa merusak data keuangan.

**Kenapa bukan opsi lain:** Opsi B dilarang karena merusak integritas penagihan; Opsi C membutuhkan persetujuan pemilik modul terlebih dahulu (Keputusan K-01).

---

## 3. Skema Tampilan Sebelum → Sesudah

### Tampilan SEBELUM Perbaikan

```text
+----------------------------------------------------------------------------------------------------+
|  Estimasi Biaya Pasien                                                                             |
|  Rincian item layanan per kelompok asuhan keperawatan dan operasional.                             |
|                                                                                      [ Muat Ulang ]|
+----------------------------------------------------------------------------------------------------+
|  (i) Pedoman Keperawatan Bangsal: Tampilan ini bersifat informatif untuk koordinasi pelayanan       |
|      klinis. Perawat dilarang memperdebatkan atau merinci harga layanan kepada pasien atau         |
|      keluarga. Rincian harga resmi tersedia di kasir.                                              |
+----------------------------------------------------------------------------------------------------+
| Kamar Rawat Inap                                                                                   |
| ITEM LAYANAN / DESKRIPSI   SATUAN             JUMLAH   HARGA SATUAN   TOTAL HARGA   WAKTU   STATUS |
| ODC 321B                   unit tarif kamar   1        -              -             ...     [Tarif ]
|                                                                                             [Belum ]
| Subtotal sementara Kamar Rawat Inap: Subtotal belum dapat dihitung                                 |
+----------------------------------------------------------------------------------------------------+
| Biaya Administrasi                                                                                 |
| ITEM LAYANAN / DESKRIPSI   SATUAN             JUMLAH   HARGA SATUAN   TOTAL HARGA   WAKTU   STATUS |
| Biaya Administrasi         transaksi          1        Rp 0,00        Rp 0,00       ...     [Aktif ]
| Subtotal Biaya Administrasi: Rp 0,00                                                               |
+----------------------------------------------------------------------------------------------------+
```

### Tampilan SESUDAH Perbaikan

```text
+----------------------------------------------------------------------------------------------------+
|  Estimasi Biaya Pasien                                                                             |
|  Rincian item layanan per kelompok asuhan keperawatan dan operasional.                             |
|                                                                                      [ Muat Ulang ]|
+----------------------------------------------------------------------------------------------------+
|  (i) Pedoman Keperawatan Bangsal: Tampilan ini bersifat informatif untuk koordinasi pelayanan       |
|      klinis. Perawat dilarang memperdebatkan atau merinci harga layanan kepada pasien atau         |
|      keluarga. Rincian harga resmi tersedia di kasir.                                              |
|                                                                                                    |
|      * Alur Layanan Klinis: Item Tindakan, Obat, dan Penunjang Medis otomatis tampil pada tabel    |
|        ini setelah tindakan selesai dieksekusi oleh dokter/perawat, obat diserahkan oleh Farmasi   |
|        (Dispensing), atau sampel laboratorium dinyatakan layak. Draf dan pesanan baru belum        |
|        menjadi beban tagihan.                                                                      |
+----------------------------------------------------------------------------------------------------+
| Kamar Rawat Inap                                                                                   |
| ITEM LAYANAN / DESKRIPSI   SATUAN             JUMLAH   HARGA SATUAN   TOTAL HARGA   WAKTU   STATUS |
| ODC 321B                   unit tarif kamar   1        Rp 750.000,00  Rp 750.000,00 ...     [Aktif ]
| Subtotal Kamar Rawat Inap: Rp 750.000,00                                                           |
+----------------------------------------------------------------------------------------------------+
| Biaya Administrasi                                                                                 |
| ITEM LAYANAN / DESKRIPSI   SATUAN             JUMLAH   HARGA SATUAN   TOTAL HARGA   WAKTU   STATUS |
| Biaya Administrasi         transaksi          1        Rp 0,00        Rp 0,00       ...     [Aktif ]
| Subtotal Biaya Administrasi: Rp 0,00                                                               |
+----------------------------------------------------------------------------------------------------+
```

### Tabel Wilayah, Tombol, dan Keadaan

| Wilayah | Isi | Sumber Data | Komponen |
| --- | --- | --- | --- |
| Banner Pedoman | Informasi etika keperawatan dan edukasi alur layanan klinis | Teks statis panduan operasional bangsal | `div.guidelineAlert` pada `nursing-billing-section.jsx` |
| Tabel Kamar Rawat Inap | Daftar baris kamar, satuan, durasi, harga satuan, total, dan status | DTO `PatientBillingEstimateResponse.Groups["ROOM"]` | `nursing-billing-section.jsx` |
| Baris Kamar ODC | Kamar ODC 321B dengan tarif aktif | `MstTariff` (sesudah diinput) via `BillingCalculationService` | Baris tabel kamar |

---

## 4. Rincian Perbaikan

### FIX-KEP-014-01 — Konfigurasi Data Master Tarif Kamar ODC 321B

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-014-01` |
| **Area** | Master Data / Database |
| **Jenis perubahan** | Konfigurasi Data Master |
| **Berkas / Target** | Data `MstTariff` dan `MstRoomChargePolicy` pada modul Master Data Billing |
| **Radius dampak** | Kalkulasi tagihan rawat inap kamar ODC pada seluruh episode ranap |
| **Bergantung pada** | P-01 (Nominal tarif resmi dari pihak RS) |

**Langkah:**
1. Verifikasi `ServiceUnitId` dari unit kamar ODC 321B dan `PatientClassId` untuk kelas ODC.
2. Tambahkan entri tarif pada tabel `MstTariff`:
   - `TariffName`: "Tarif Kamar Rawat ODC"
   - `IsRoomCharge`: `true`
   - `ServiceUnitId`: ID unit kamar ODC
   - `PatientClassId`: ID kelas ODC
   - `NormalPrice`: sesuai nominal yang ditetapkan RS
   - `EffectiveStartDate`: tanggal efektif (sebelum tanggal admisi pasien)
   - `IsActive`: `true`, `IsDelete`: `false`
3. Pastikan `MstRoomChargePolicy` memiliki kebijakan kamar aktif dengan `TariffMoment: OCCUPANCY_START` atau `PERIOD_START`.

**Acceptance criteria:**
1. Saat membuka tab Estimasi Biaya pada episode rawat inap pasien ODC, baris kamar menampilkan harga satuan resmi dalam format Rupiah baku (contoh: `Rp 750.000,00`), bukan strip `-`.
2. Status baris kamar berubah dari badge kuning *"Tarif Belum Ada"* menjadi badge hijau *"Aktif"*.
3. Subtotal Kamar Rawat Inap menampilkan nilai nominal terhitung dan tidak lagi berstatus *"Subtotal belum dapat dihitung"*.

**Verifikasi:**
Panggil ulang endpoint `GET /api/v1/health-services/billing-management/patient-billing-summaries/episodes/{episodeId}/breakdown/estimate` dan pastikan `RoomChargeAmount > 0` serta `MissingTariff == false`.

---

### FIX-KEP-014-03 — Penambahan Catatan Alur Layanan Klinis pada Banner Estimasi Biaya

| | |
| --- | --- |
| **Menutup** | `ISS-KEP-014-02`, `ISS-KEP-014-03` |
| **Area** | Frontend |
| **Jenis perubahan** | Source Frontend |
| **Berkas yang diubah** | `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx` |
| **Radius dampak** | Layar Estimasi Biaya di Ruang Kerja Keperawatan dan Dokumen Admisi |
| **Bergantung pada** | K-01 (Persetujuan Pemilik) |

**Langkah:**
1. Buka berkas `nursing-billing-section.jsx`.
2. Pada elemen `guidelineContent` (sekitar baris 256–261), tambahkan sub-paragraf atau teks penjelas alur pesanan klinis:
   ```jsx
   <div className={styles.guidelineContent}>
     <p>
       <strong>Pedoman Keperawatan Bangsal:</strong> Tampilan ini bersifat informatif untuk koordinasi pelayanan klinis. Perawat <em>dilarang memperdebatkan atau merinci harga</em> layanan kepada pasien atau keluarga. Rincian harga resmi tersedia di kasir.
     </p>
     <p className="mt-1 text-muted small">
       <strong>Catatan Alur Layanan:</strong> Rincian Tindakan, Obat, dan Penunjang Medis otomatis tampil pada tabel ini setelah tindakan selesai dieksekusi dokter/perawat, obat diserahkan oleh Farmasi (Dispensing), atau sampel penunjang dinyatakan layak periksa. Draf dan pesanan yang belum diserahkan tidak dicantumkan sebagai tagihan.
     </p>
   </div>
   ```

**Acceptance criteria:**
1. Banner informasi di atas tabel Estimasi Biaya memuat catatan alur layanan klinis yang jelas dan mudah dipahami staf bangsal.
2. Tampilan bersih, responsif, dan konsisten dengan token desain.

**Verifikasi:**
Jalankan validasi lint: `npm run lint:errors` pada repository frontend.

---

## 5. Urutan Pengerjaan

```text
P-01 (Jawaban pelapor mengenai nominal tarif kamar ODC)
└── FIX-KEP-014-01 (Input master data tarif kamar ODC)

K-01 (Persetujuan pemilik sistem)
└── FIX-KEP-014-03 (Penyempurnaan teks banner informasi alur layanan pada frontend)
    └── FIX-KEP-014-02 (Dokumentasi panduan alur kerja klinis dan penutupan issue)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| :---: | --- | --- | --- | :---: |
| K-01 | Persetujuan teks edukasi alur layanan klinis pada banner Estimasi Biaya | Setuju / Ubah Redaksi | Setuju | `FIX-KEP-014-03` |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada perubahan kontrak API atau skema struktur data backend/frontend. Dokumen desain blueprint tetap konsisten.

---

## 8. Verifikasi Menyeluruh

1. **Uji Kasus 1 (Kamar ODC):** Buka pasien rawat inap Farhan Alamsyah di ruang ODC 321B, buka tab Estimasi Biaya, pastikan harga kamar muncul dengan nominal rupiah dan subtotal kamar terhitung lengkap.
2. **Uji Kasus 2 (Siklus Resep Obat):**
   - Resep `RX-20261020-00001` dalam status Draf: pastikan belum muncul di tabel Obat & Alkes.
   - Dokter memfinalisasi resep, lalu petugas Farmasi melakukan telaah dan proses *Dispensing* (penyerahan obat).
   - Muat ulang tab Estimasi Biaya: pastikan kelompok **Obat & Alkes** kini otomatis muncul dengan rincian item obat yang diserahkan beserta jumlah dan harganya.
3. **Uji Kasus 3 (Siklus Tindakan):**
   - Dokter/perawat meng-order tindakan medis: pastikan belum muncul saat status masih `Ordered`.
   - Perawat melakukan eksekusi tindakan (*Complete/Execute*): muat ulang Estimasi Biaya, pastikan kelompok **Tindakan** otomatis muncul dengan rincian tindakan yang dieksekusi.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| :---: | --- | --- |
| 2026-10-20 | Rencana perbaikan dibuat, 3 butir perbaikan, status MENUNGGU_PERSETUJUAN. | `diagnose-module-issue` |
| 2026-10-20 | Persetujuan eksekusi database Butir 1 oleh pengguna. FIX-KEP-014-01 dieksekusi pada database `QuilvianNewDevHamzah`: penambahan tarif kamar ODC 321B (Rp 750.000,00) dengan ID `3e74e8c6-3a8b-4f33-83ed-80eea967650b` terverifikasi aktif. Status FIX-KEP-014-01: ✅ SELESAI. | `manage-database-operations` |
