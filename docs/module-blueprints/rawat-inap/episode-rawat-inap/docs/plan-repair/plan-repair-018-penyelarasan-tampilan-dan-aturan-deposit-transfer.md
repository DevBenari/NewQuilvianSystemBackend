# PLAN-REPAIR-018 — Penyelarasan Tampilan dan Rule Deposit Admisi Transfer Pasien terhadap Standar Pasien Lama

```yaml
plan_id: PLAN-REPAIR-EPS-018
issue: ../issue/issue-018-penyelarasan-tampilan-dan-aturan-deposit-transfer.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-14"
diputuskan_oleh: "Pemilik Sistem (User)"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah)"
basis_source_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID Perbaikan | Menutup | Ringkasan Perbaikan | Area | Prioritas | Task ID | Status | Bukti |
| :--- | :--- | :--- | :---: | :---: | :---: | :---: | :--- |
| `FIX-EPS-018-01` | `ISS-EPS-018-01` | Tampilkan spanduk kebijakan deposit resmi dari billing pada alur transfer pasien secara seragam dengan pasien lama | FE | 1 | — | ✅ SELESAI | `../../task/report/frontend/FIX-EPS-018.md` |
| `FIX-EPS-018-02` | `ISS-EPS-018-02` | Aktifkan indikator status pemenuhan deposit (*Terbayar Sebagian*, *Belum Membayar*, *Lunas*) pada alur transfer | FE | 1 | — | ✅ SELESAI | `../../task/report/frontend/FIX-EPS-018.md` |
| `FIX-EPS-018-03` | `ISS-EPS-018-03` | Standarisasi form pemilihan metode pembayaran uang muka transfer dengan label, layout kompak, dan tombol reload yang rapi | FE | 2 | — | ✅ SELESAI | `../../task/report/frontend/FIX-EPS-018.md` |
| `FIX-EPS-018-04` | `ISS-EPS-018-04` | Satukan dan eliminasi perulangan teks keterangan hint di bawah kotak input nominal deposit | FE | 3 | — | ✅ SELESAI | `../../task/report/frontend/FIX-EPS-018.md` |

**Ringkasan: 4 dari 4 perbaikan selesai (100% Terverifikasi).**

---

## 2. Solusi Terpilih Per Temuan

### ISS-EPS-018-01 — Spanduk Kebijakan Deposit Resmi pada Alur Transfer

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Biarkan pesan statis transfer dan tambahkan teks minimum secara manual di dalamnya | Cepat diubah | Tidak dinamis, membantah data kebijakan billing asli |
| **B** | Gunakan logika spanduk kebijakan yang sama persis seperti Pasien Lama (`policyLoading`, `policyError`, `minimumAmount`, `policyName`), lalu sisipkan catatan khusus transfer di dalamnya | Seragam 100% dengan alur Pasien Lama, dinamis mengikuti data master billing `MstDepositPolicy` | Memerlukan penyelarasan teks spanduk |

**Solusi Terpilih: Opsi B.**  
Tampilkan spanduk kebijakan resmi yang memuat batas minimum (Rp 10.000.000) dan batas jatuh tempo (3 hari kerja pasca admisi). Untuk alur transfer, tambahkan penegasan bahwa penerimaan uang muka saat transfer tidak menahan proses admisi dan receipt resmi diterbitkan setelah transaksi booking admisi berhasil.

---

### ISS-EPS-018-02 — Indikator Status Pemenuhan Deposit pada Alur Transfer

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Hanya tampilkan status jika nominal transfer terisi penuh (lunas) | Logika sederhana | Petugas tidak tahu bahwa pengisian Rp 1.000.000 menyisakan kekurangan Rp 9.000.000 |
| **B** | Hapus batasan `!deposit?.transfer` sehingga seluruh indikator status (*Terbayar Sebagian*, *Belum Membayar*, *Lunas*) aktif seragam seperti pada Pasien Lama | Konsisten 100% dengan `FR-RI-176`, memberikan kepastian finansial bagi keluarga dan petugas | Tidak ada kekurangan |

**Solusi Terpilih: Opsi B.**  
Menghapus `!deposit?.transfer` dari baris 112 dan 126 berkas `inpatient-admission-deposit-step.jsx`. Ketika keluarga pasien transfer membayar Rp 1.000.000, sistem langsung menampilkan peringatan kuning *"Status Pembayaran: Terbayar Sebagian — Kekurangan deposit sebesar Rp 9.000.000 akan ditagihkan dan dapat dibuatkan Surat Pernyataan Kesediaan Melunasi pada Workspace PPRI"*.

---

### ISS-EPS-018-03 — Standarisasi Tampilan Pemilihan Metode Pembayaran Transfer

| Opsi | Pendekatan | Kelebihan | Kekurangan |
| :---: | :--- | :--- | :--- |
| **A** | Tetap menggunakan tombol sekunder blok besar di bawah dropdown | Tanpa penambahan CSS baru | Tampilan kasar dan melanggar standar UI Quilvian |
| **B** | Bungkus dalam form-group terstandarisasi (`formStyles.field` atau `admissionStyles.depositMethodGroup`), beri label resmi `Metode Penerimaan Deposit *`, dan ubah tombol reload menjadi aksi kompak di samping label | Antarmuka bersih, modern, ergonomis, konsisten dengan base component Quilvian | Memerlukan penyesuaian CSS modul |

**Solusi Terpilih: Opsi B.**  
Membungkus `FilterSelect` ke dalam form-group terstruktur dengan label resmi dan tombol reload yang ringkas.

---

### ISS-EPS-018-04 — Pembersihan Teks Keterangan Duplikat

Satukan kalimat di bawah input nominal:  
*"Kosongkan bila keluarga belum membayar uang muka. Penerimaan deposit dicatat bersamaan dengan transaksi admisi. Nominal kosong tetap dapat dilanjutkan."*

---

## 3. Skema Tampilan Sebelum → Sesudah

### 3.1 Skema Langkah 4: Deposit pada Admisi Transfer Pasien

**SEBELUM (Kondisi Bermasalah di Tangkapan Layar Pelapor):**
```text
+---------------------------------------------------------------------------------+
| LANGKAH DEPOSIT                                                                 |
| Deposit / Uang Muka                                                             |
| Catat uang muka yang diterima dari keluarga pasien.                             |
+---------------------------------------------------------------------------------+
| ℹ Deposit saat admisi transfer                                                  |
| Deposit tidak menahan admisi. Jika keluarga menyerahkan uang muka, isi nominal  |
| dan metode penerimaan. Receipt diterbitkan setelah transaksi admisi berhasil.   |
+---------------------------------------------------------------------------------+
| Nominal Diterima                                                                |
| Rp [ 1.000.000                                                                ] |
|                                                                                 |
| Kosongkan bila keluarga belum membayar uang muka. Admisi tetap dapat dilanjutkan|
| Penerimaan dan receipt deposit dicatat bersama admisi saat booking.             |
| Nominal kosong tetap boleh dilanjutkan.                                         |
|                                                                                 |
| (TIDAK ADA PERINGATAN KEKURANGAN RP 9.000.000 / STATUS TERBAYAR SEBAGIAN)       |
|                                                                                 |
| [ Tunai                                                                       ▾]|  <-- Melayang tanpa label
| [                              Muat Ulang Metode                              ] |  <-- Tombol raksasa meluber
+---------------------------------------------------------------------------------+
| [ Batal Pendaftaran ]                              [ Kembali ] [ Lanjut Dokter ]|
+---------------------------------------------------------------------------------+
```

**SESUDAH (Setelah Penyelarasan terhadap Standar Pasien Lama):**
```text
+---------------------------------------------------------------------------------+
| LANGKAH DEPOSIT                                                                 |
| Deposit / Uang Muka                                                             |
| Catat uang muka yang diterima dari keluarga pasien.                             |
+---------------------------------------------------------------------------------+
| ℹ Kebijakan Deposit Rawat Inap (Pasien Umum / Tunai)                            |
| Ketentuan Minimum Deposit: Rp 10.000.000 • Batas Jatuh Tempo Pelunasan:         |
| 3 Hari Kerja Pasca Admisi. Pembayaran uang muka tidak menahan proses admisi.    |
| (Receipt resmi diterbitkan bersamaan setelah transaksi admisi transfer berhasil)|
+---------------------------------------------------------------------------------+
| Nominal Diterima                                                                |
| Rp [ 1.000.000                                                                ] |
| Kosongkan bila keluarga belum membayar uang muka. Admisi tetap dapat dilanjutkan|
|                                                                                 |
| ⚠ Status Pembayaran: Terbayar Sebagian                                          |
| Kekurangan deposit sebesar Rp 9.000.000 akan ditagihkan dan dapat dibuatkan     |
| Surat Pernyataan Kesediaan Melunasi pada Workspace PPRI.                        |
| Catatan: Pembayaran deposit di bawah batas minimum tidak membatalkan admisi.   |
+---------------------------------------------------------------------------------+
| Metode Penerimaan Deposit *                            [ 🔄 Muat Ulang Metode ] |
| [ Tunai                                                                       ▾]|
| Pilih metode pembayaran yang diserahkan keluarga pasien untuk deposit transfer. |
+---------------------------------------------------------------------------------+
| [ Batal Pendaftaran ]                              [ Kembali ] [ Lanjut Dokter ]|
+---------------------------------------------------------------------------------+
```

#### Tabel Komponen dan Keadaan Antarmuka Langkah Deposit Transfer

| Bagian Antarmuka | Kondisi / Nilai | Komponen UI | Penjelasan & Aturan |
| :--- | :--- | :--- | :--- |
| **Header Kebijakan** | Data Kebijakan Billing Aktif | `<InformationAlert variant="info">` | Menampilkan Rp 10.000.000 & 3 hari jatuh tempo, seragam dengan Pasien Lama |
| **Input Nominal** | Terisi angka rupiah (misal `1.000.000`) | `<input type="text">` dengan prefix `Rp` | Sanitasi digit otomatis, format ribuan `id-ID` |
| **Status Pembayaran** | `amount < minimumAmount` | `<InformationAlert variant="warning">` | Status *"Terbayar Sebagian"* dengan rincian selisih Rp 9.000.000 |
| **Status Kosong** | `amount == 0` | `<InformationAlert variant="warning">` | Status *"Belum Membayar Deposit"*, kekurangan penuh Rp 10.000.000 |
| **Status Lunas** | `amount >= minimumAmount` | `<InformationAlert variant="success">` | Status *"Lunas Memenuhi Kebijakan"* |
| **Metode Transfer** | Hanya muncul jika `amount > 0` | `FilterSelect` + Label + Action Button | Terbungkus rapi, tombol reload kompak di kanan atas label |
| **Navigasi** | Tombol Lanjut ke Dokter | `<BaseButton variant="primary">` | **Selalu aktif** (tidak pernah disabled sesuai `RWI-DEC-095`) |

---

## 4. Rincian Perbaikan

### FIX-EPS-018-01 — Spanduk Kebijakan Deposit Resmi pada Jalur Transfer
- Berkas target: `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx`
- Ubah blok alert kebijakan di awal langkah: alur transfer tidak lagi menimpa alert kebijakan billing, melainkan menampilkan informasi kebijakan minimum yang sama disertai keterangan spesifik transfer (atau menyatukan catatan transfer ke dalam alert informasi).

### FIX-EPS-018-02 — Pengaktifan Indikator Status Pemenuhan Deposit
- Berkas target: `QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx`
- Hapus klausa `!deposit?.transfer` pada render indikator `hasShortfall` dan `isPaidInFull`.
- Pastikan teks kekurangan deposit dan panduan pelunasan Workspace PPRI tampil akurat ketika `amount` diisi Rp 1.000.000 maupun saat kosong.

### FIX-EPS-018-03 — Standarisasi Layout Pemilihan Metode Pembayaran Transfer
- Berkas target: `inpatient-admission-deposit-step.jsx` dan `inpatient-admission.module.css`
- Tambahkan container khusus `.depositMethodGroup`:
  - Header dengan label: `Metode Penerimaan Deposit` dan tanda wajib `*` bila `hasAmount` bernilai true.
  - Tombol aksi muat ulang metode berukuran kompak (*small/ghost button*) dengan ikon putar/reload yang ditempatkan secara ergonomis.
  - Teks petunjuk bantuan (*hint*) di bawah dropdown.
  - Penanganan error state jika `methodsError` terjadi.

### FIX-EPS-018-04 — Konsolidasi Teks Bantuan Input Nominal
- Bersihkan duplikasi teks keterangan pada input nominal agar mudah dipahami petugas admisi dan tidak saling tumpang tindih.

---

## 5. Urutan Pengerjaan

```text
[FIX-EPS-018-01: Tampilkan Kebijakan Deposit Resmi]
         │
         ▼
[FIX-EPS-018-02: Aktifkan Indikator Status Pemenuhan Deposit (Kekurangan/Lunas)]
         │
         ▼
[FIX-EPS-018-03: Standarisasi Kontrol Pemilihan Metode Pembayaran Transfer]
         │
         ▼
[FIX-EPS-018-04: Konsolidasi Teks Keterangan & Pembersihan Layout]
         │
         ▼
[Verifikasi End-to-End: Uji Input Kosong, Parsial Rp 1 Juta, dan Lunas Rp 10 Juta]
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

| No | Keputusan yang Dibutuhkan | Dampak Bila Disetujui | Rekomendasi |
| :---: | :--- | :--- | :--- |
| **K-01** | Menyetujui rencana perbaikan `PLAN-REPAIR-EPS-018` untuk menyelaraskan tampilan dan aturan deposit transfer pasien dengan standar pasien lama. | Frontend admisi transfer akan menampilkan kebijakan minimum Rp 10 Juta, indikator kekurangan bayar, dan kontrol metode penerimaan yang rapi. | **(Disetujui)** Lanjutkan ke implementasi frontend satu task terarah. |

---

## 7. Dokumen Hulu yang Ikut Direvisi

Tidak ada dokumen arsitektur atau kontrak hulu yang perlu direvisi karena perbaikan ini sepenuhnya mengembalikan implementasi frontend agar **patuh** pada PRD MVP `FR-RI-174` s.d. `FR-RI-178` dan invariant `RWI-DEC-095`.

---

## 8. Verifikasi Menyeluruh

Skenario uji yang wajib divalidasi setelah implementasi:
1. **Skenario 1 (Keluarga Belum Bayar — Rp 0):**
   - Buka alur Admisi Transfer pasien umum/tunai.
   - Periksa spanduk atas: Menampilkan kebijakan deposit minimum Rp 10.000.000 dan jatuh tempo 3 hari.
   - Biarkan input nominal kosong (Rp 0).
   - Periksa status: Menampilkan peringatan kuning *"Status Pembayaran: Belum Membayar Deposit — Seluruh kewajiban deposit sebesar Rp 10.000.000 akan dicatat sebagai kekurangan..."*.
   - Periksa bagian Metode Pembayaran: Tersembunyi otomatis karena tidak ada uang yang diterima.
   - Klik **Lanjut ke Dokter**: Berhasil lanjut tanpa tertahan.
2. **Skenario 2 (Keluarga Bayar Sebagian — Rp 1.000.000 seperti di tangkapan layar):**
   - Ketik `1.000.000` pada nominal diterima.
   - Periksa status: Menampilkan peringatan kuning *"Status Pembayaran: Terbayar Sebagian — Kekurangan deposit sebesar Rp 9.000.000 akan ditagihkan..."*.
   - Periksa bagian Metode Pembayaran: Muncul secara rapi dengan label `Metode Penerimaan Deposit *` dan pilihan dropdown (misal *Tunai*).
   - Tombol *Muat Ulang Metode* tampak kompak di kanan label.
   - Klik **Lanjut ke Dokter**: Berhasil lanjut.
3. **Skenario 3 (Keluarga Bayar Lunas — Rp 10.000.000):**
   - Ketik `10.000.000`.
   - Periksa status: Menampilkan spanduk hijau *"Status Pembayaran: Lunas Memenuhi Kebijakan"*.

---

## 9. Riwayat

| Tanggal | Status | Perubahan | Oleh |
| :--- | :--- | :--- | :--- |
| **2026-10-14** | `MENUNGGU_PERSETUJUAN` | Penyusunan draf rencana perbaikan untuk diajukan kepada pemilik sistem | Antigravity AI |
