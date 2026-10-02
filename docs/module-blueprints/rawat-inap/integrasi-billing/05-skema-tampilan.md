# Skema Tampilan Antarmuka — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| Versi Dokumen | `1.0.0` (Draft) — 17 September 2026 |
| Target Framework | Next.js 14+ App Router, React 18, Tailwind CSS, Quilvian Design Tokens |
| Aturan Tampilan | Seluruh teks label dan informasi disajikan dalam **Bahasa Indonesia** yang mudah dipahami staf bangsal, dan **steril dari nominal rupiah** kecuali pemegang izin khusus. |

---

## 1. Skematik 1: Kolom Status Kasir pada Dashboard Sensus Bangsal (`FE-INP-01`)

### 1.1 Wireframe Skematik
```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│ SENSUS PASIEN RAWAT INAP — BANGSAL MELATI (LANTAI 3)                       [Filter Kamar ▼] [Cari]│
├─────────┬──────────────┬─────────────┬──────────┬──────────────┬───────────────────┬─────────────┤
│ Kamar   │ No. RM & Nama│ Dokter DPJP │ Tgl Masuk│ Status Medis │ Status Kasir      │ Aksi        │
├─────────┼──────────────┼─────────────┼──────────┼──────────────┼───────────────────┼─────────────┤
│ MEL-01A │ RM-2026-081  │ dr. Anwar   │ 14/09/26 │ Perawatan    │ [Tagihan Berjalan]│ [Lihat Bed] │
│         │ Siti Rahayu  │ Sp.PD       │          │              │ (Biru Muda)       │             │
├─────────┼──────────────┼─────────────┼──────────┼──────────────┼───────────────────┼─────────────┤
│ MEL-02A │ RM-2026-088  │ dr. Anwar   │ 15/09/26 │ Izin Pulang  │ [Menunggu Kasir]  │ [Proses Pulang]
│         │ Budi Santoso │ Sp.PD       │          │ (Discharge)  │ (Kuning Amber)    │             │
├─────────┼──────────────┼─────────────┼──────────┼──────────────┼───────────────────┼─────────────┤
│ MEL-03A │ RM-2026-092  │ dr. Maya    │ 16/09/26 │ Izin Pulang  │ [Clearance Lunas] │ [Proses Pulang]
│         │ Hendra Jaya  │ Sp.A        │          │ (Discharge)  │ (Hijau Emerald)   │             │
└─────────┴──────────────┴─────────────┴──────────┴──────────────┴───────────────────┴─────────────┘
```

### 1.2 Tabel Spesifikasi Wilayah Komponen
| Elemen | Sumber Data Endpoint | Hak Akses | Keadaan Kosong / Error |
|---|---|---|---|
| Lencana Status Kasir | `GET /api/v1/health-services/inpatient-management/episodes/{id}/billing-status` | `InpatientNurse:Read` | Loading: Skeleton abu-abu.<br>Error: Lencana abu-abu bertuliskan *"Status Kasir Tidak Tersedia"*. |

---

## 2. Skematik 2: Kartu Status Kasir pada Detail Episode Pasien (`FE-INP-04`)

### 2.1 Wireframe Skematik
```text
┌──────────────────────────────────────────────────────────────────────────────────────────────────┐
│ DETAIL PASIEN RAWAT INAP: Budi Santoso (RM-2026-08891) — Kamar Melati 02A (Kelas 2)             │
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│ [ Ringkasan Medis ] [ Rencana Harian ] [ Catatan CPPT ] [ Riwayat Kamar ] [ Administrasi & Kasir ]│
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                                  │
│ ┌─ STATUS ADMINISTRASI & KASIR ────────────────────────────────────────────────────────────────┐ │
│ │ Status Folio Kasir   : OPEN (Tagihan Berjalan)                                               │ │
│ │ Status Persetujuan   : MENUNGGU PENYELESAIAN KASIR                                           │ │
│ │                                                                                              │ │
│ │ Kendala / Catatan Kasir:                                                                     │ │
│ │ • Keluarga pasien belum menyelesaikan administrasi di loket kasir utama                      │ │
│ │ • Menunggu konfirmasi verifikasi resep farmasi sore                                          │ │
│ │                                                                                              │ │
│ │ Catatan Khusus Bangsal:                                                                      │ │
│ │ "Nominal rupiah tidak ditampilkan pada layar perawat demi kerahasiaan keuangan pasien."      │ │
│ │                                                                                              │ │
│ │ [ 🔒 Buka Rincian Biaya Finansial (Hanya Staf Keuangan) ]                                    │ │
│ └──────────────────────────────────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Tabel Spesifikasi Wilayah Komponen
| Elemen | Sumber Data Endpoint | Hak Akses | Keterangan Interaktif |
|---|---|---|---|
| Status Folio & Clearance | `GET .../billing-status` | `InpatientNurse:Read` | Menampilkan label teks dalam Bahasa Indonesia tanpa angka rupiah. |
| Daftar Kendala (*Blockers*) | `response.blockerReasons[]` | `InpatientNurse:Read` | Daftar poin kendala informatif untuk staf bangsal. |
| Tombol Buka Rincian Biaya | `GET .../billing-details` | `InpatientBilling:View` | Terkunci (*hidden* / *disabled*) bagi perawat biasa; hanya aktif bagi pemegang izin keuangan. |

---

## 3. Skematik 3: Modal Pemulangan Pasien & Clearance Gate (`FE-INP-06`)

### 3.1 Kondisi A: Clearance Kasir Disetujui (Happy Path)
```text
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│ KONFIRMASI PEMULANGAN PASIEN (DISCHARGE GATE)                                        [X] │
├──────────────────────────────────────────────────────────────────────────────────────────┤
│ Pasien: Budi Santoso (RM-2026-08891) | Kamar: Melati 02A                                  │
│                                                                                          │
│ ┌─ KELAYAKAN PEMULANGAN ───────────────────────────────────────────────────────────────┐ │
│ │ 1. Izin Medis Dokter DPJP   : ✅ DISETUJUI (dr. Anwar Sp.PD, 17/09/2026 10:00)       │ │
│ │ 2. Resume Medis Terisi      : ✅ LENGKAP & DITANDATANGANI                            │ │
│ │ 3. Obat Pulang Diserahkan   : ✅ SUDAH DISERAHKAN OLEH FARMASI BANGSAL               │ │
│ │ 4. Persetujuan Kasir / Keuangan: ✅ CLEARANCE KASIR DISETUJUI (Loket Kasir 1, 11:30)  │ │
│ └──────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                          │
│ Informasi Waktu Kepulangan Fisik:                                                        │
│ Waktu Saat Ini : 17 September 2026, 12:15 WIB                                            │
│ "Menekan tombol di bawah ini akan menghentikan perhitungan sewa kamar secara presisi."   │
│                                                                                          │
│               [ Batal ]       [ ✅ KONFIRMASI PASIEN PULANG FISIK ]                      │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Kondisi B: Clearance Dicabut / Tagihan Susulan (*Auto-Reblock*)
```text
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│ KONFIRMASI PEMULANGAN PASIEN (DISCHARGE GATE)                                        [X] │
├──────────────────────────────────────────────────────────────────────────────────────────┤
│ ⚠️ PERINGATAN KERAS: PERSETUJUAN KASIR TELAH DIBATALKAN! (AUTO-REBLOCK)                 │
│ Alasan Pencabutan : Terdapat tagihan susulan resep darurat farmasi yang belum dibayar.   │
│ Waktu Pembatalan  : 17/09/2026 11:45 WIB oleh Kasir Hendra                             │
│                                                                                          │
│ Pasien DILARANG meninggalkan ruangan secara normal sebelum clearance kasir disetujui.    │
│                                                                                          │
│ [ Tombol Konfirmasi Pasien Pulang Fisik: TERKUNCI (DISABLED) ]                           │
│                                                                                          │
│ ── KEDARURATAN KLINIS ────────────────────────────────────────────────────────────────── │
│ Bila pasien dalam kondisi darurat klinis kritis / evakuasi ambulans rujukan darurat:     │
│                                                                                          │
│               [ Batal ]       [ 🚨 SUPERVISOR OVERRIDE (DARURAT) ]                       │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Skematik 4: Modal *Supervisor Override* Pelepasan Darurat (`FE-INP-06-OVR`)

### 4.1 Wireframe Skematik
```text
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│ OTORISASI DARURAT: SUPERVISOR OVERRIDE PEMULANGAN PASIEN                             [X] │
├──────────────────────────────────────────────────────────────────────────────────────────┤
│ Pasien: Budi Santoso (RM-2026-08891) | Kamar: Melati 02A                                  │
│ Status Kasir Terkini: CLEARANCE DICABUT (Ada Tagihan Susulan)                            │
│                                                                                          │
│ ⚠️ PERHATIAN SUPERVISOR:                                                                 │
│ Fitur ini HANYA digunakan untuk keadaan darurat klinis medis atau evakuasi pasien rujukan│
│ yang tidak boleh tertahan administrasi. Tindakan ini akan dicatat ke audit hukum RS.    │
│                                                                                          │
│ Masukkan Alasan Kedaruratan Klinis Wajib (*):                                            │
│ ┌──────────────────────────────────────────────────────────────────────────────────────┐ │
│ │Pasien darurat syok kardiogenik, rujukan ambulans prioritas 1 ke RS Harapan Kita.     │ │
│ │Administrasi kasir dilanjutkan keluarga penjamin di loket kasir utama.                │ │
│ └──────────────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                          │
│ Masukkan PIN Otorisasi Supervisor (*): [ • • • • • • ]                                   │
│                                                                                          │
│               [ Batal ]       [ 🚨 SETUJUI PELEPASAN FISIK DARURAT ]                     │
└──────────────────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Spesifikasi Validasi Form Modal
- Kolom `Alasan Kedaruratan Klinis`: Wajib diisi minimal **20 karakter**. Sistem menolak teks singkat seperti "darurat" atau "rujukan".
- Kolom `PIN Otorisasi`: Wajib diverifikasi terhadap password/PIN akun supervisor yang sedang login.
- Tombol aksi: Memanggil `POST /api/v1/health-services/inpatient-management/episodes/{id}/supervisor-override`.

---

## 5. Skematik 5: Drawer Rincian Finansial Kasir (`FE-INP-04-FIN`)
*(Hanya dapat diakses oleh pemegang permission `InpatientBilling:View`)*

### 5.1 Wireframe Skematik
```text
┌────────────────────────────────────────────────────────────────────┐
│ RINCIAN BIAYA RAWAT INAP (KASIR & KEUANGAN)                    [X] │
├────────────────────────────────────────────────────────────────────┤
│ Pasien: Budi Santoso (RM-2026-08891)                               │
│ Periode Rawat: 15/09/2026 09:30 s.d. 17/09/2026 12:15             │
│ Penjamin: BPJS Kesehatan / Asuransi Mandiri Inhealth               │
├────────────────────────────────────────────────────────────────────┤
│ KOMPONEN BIAYA PELAYANAN:                                          │
│ 1. Sewa Kamar Melati Kelas 2 (2 Hari)           : Rp 1.600.000     │
│ 2. Visite Dokter Spesialis (dr. Anwar, 3x)      : Rp   750.000     │
│ 3. Tindakan Keperawatan                         : Rp   450.000     │
│ 4. Obat & Alkes Farmasi                         : Rp 2.850.000     │
│ 5. Pemeriksaan Laboratorium Darah Lengkap       : Rp   650.000     │
│ 6. Biaya Administrasi Rawat Inap                : Rp   441.000     │
│ ────────────────────────────────────────────────────────────────── │
│ TOTAL BIAYA AKUMULASI                           : Rp 6.741.000     │
│                                                                    │
│ STATUS PEMBAYARAN:                                                 │
│ • Ditanggung Penjamin (Covered)                 : Rp 5.500.000     │
│ • Tanggungan Pasien (Patient Excess)            : Rp 1.241.000     │
│ • Deposit Pasien yang Telah Dibayar             : Rp 1.000.000     │
│ • Sisa Tagihan Kurang Bayar (Outstanding)       : Rp   241.000     │
│                                                                    │
│ Status Clearance Kasir: BELUM LUNAS (Kurang Bayar Rp 241.000)      │
│ [ Cetak Lembar Rincian Finansial ]       [ Tutup Panel ]           │
└────────────────────────────────────────────────────────────────────┘
```
