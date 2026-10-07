# Arsitektur Frontend — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| Versi Desain | `1.0.0` (Draft) — 17 September 2026 |
| Target Framework | Next.js 14+ (App Router), React 18, Redux Toolkit, Axios, Tailwind CSS, Quilvian Design Tokens |
| Prinsip UI Utama | **Privasi Finansial di Bangsal:** Layar operasional perawat strictly steril dari nominal rupiah; fokus pada status operasional kasir dan kendala blocker ([`RWI-DEC-160`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/00-interview-decisions.md)). |

---

## 1. Prinsip Desain Antarmuka Pengguna

Pengintegrasian data keuangan dengan operasional bangsal rawat inap menerapkan prinsip *Role-Based Information Segregation*:

1. **Staf Keperawatan Bangsal (Nurse & Head Nurse):**
   - Tidak memerlukan informasi nominal rupiah (Total Biaya, Tagihan Berjalan, Sisa Tagihan).
   - Membutuhkan kejelasan **Status Operasional Tagihan**: Apakah tagihan berstatus `Belum Lunas`, `Menunggu Penyelesaian Kasir`, atau `Clearance Kasir Disetujui`.
   - Membutuhkan daftar **Alasan Kendala (Blockers)** dalam bahasa Indonesia yang manusiawi (contoh: *"Keluarga belum menyelesaikan administrasi di loket kasir"*, *"Terdapat tagihan susulan resep farmasi"*).
   - Mengontrol tombol operasional: `Konfirmasi Pasien Pulang Fisik`.

2. **Supervisor Bangsal (Nurse Supervisor / Duty Manager):**
   - Memiliki wewenang darurat klinis melalui fitur **Supervisor Override Pemulangan**.
   - Digunakan ketika pasien kritis/rujukan gawat darurat harus segera dilepaskan secara fisik meskipun kasir belum menerbitkan clearance atau clearance dicabut (*Auto-Reblock*).
   - Wajib memasukkan alasan klinis yang sah dan kredensial/PIN otorisasi.

3. **Staf Keuangan / Billing Viewer (`InpatientBilling:View`):**
   - Pengguna dengan izin khusus ini dapat membuka panel ekspansi (*drawer*) untuk melihat breakdown rincian rupiah secara transparan (sewa kamar, visite dokter, tindakan keperawatan, obat farmasi, lab, radiologi).

---

## 2. Peta Butir Menu dan Jalur Navigasi

Sesuai aturan arsitektur frontend Quilvian, fitur integrasi ini **TIDAK MENAMBAH MENU SIDEBAR BARU** yang memadati navigasi. Seluruh antarmuka disematkan secara kontekstual pada layar-layar yang sudah ada:

| Kode Layar | Nama Layar Induk | Penempatan Komponen Integrasi | Hak Akses Pengakses |
|---|---|---|---|
| `FE-INP-01` | **Sensus Bangsal (Ward Census Dashboard)** | Kolom status kasir berupa lencana kecil pada tabel pasien kamar: `BillingStatusBadge` (Lunas / Menunggu Kasir). | `InpatientEpisode:Read` / `InpatientNurse:Read` |
| `FE-INP-04` | **Detail Episode Pasien (Patient Inpatient Workspace)** | Kartu Ringkasan Administrasi Kasir (`BillingSummaryCard`) yang menampilkan status folio, status clearance, dan kendala blocker tanpa rupiah. | `InpatientEpisode:Read` / `InpatientNurse:Read` |
| `FE-INP-04-FIN` | **Drawer Rincian Finansial (Financial Breakdown Drawer)** | Panel geser kanan yang hanya dapat dibuka jika user memiliki hak akses `InpatientBilling:View`. | `InpatientBilling:View` |
| `FE-INP-06` | **Alur Pemulangan Pasien (Patient Discharge Modal)** | Komponen `DischargeClearanceGateCard`: tombol `Pasien Pulang Fisik` terkunci (disabled) selama status clearance belum `Cleared`, kecuali di-override oleh supervisor. | `InpatientNurse:Write` / `InpatientSupervisor:Override` |

---

## 3. Komponen Frontend Baru & Pola State Management

### 3.1 Struktur Komponen (Atomic Components)

```text
src/components/features/inpatient/billing-integration/
├── BillingStatusBadge.jsx               # Lencana warna-warni status operasional kasir
├── BillingSummaryCard.jsx                # Kartu status kasir di halaman detail episode bangsal
├── DischargeClearanceGateCard.jsx        # Komponen gerbang pemulangan fisik & auto-reblock
├── SupervisorOverrideModal.jsx           # Modal verifikasi alasan darurat & PIN supervisor
├── BillingFinancialDetailsDrawer.jsx     # Panel rincian nominal rupiah (khusus InpatientBilling:View)
└── hooks/
    ├── useInpatientBillingStatus.js      # Hook SWR/React Query untuk polling status kasir
    └── useClearanceGateAction.js         # Hook mutasi pelepasan fisik & supervisor override
```

### 3.2 Pola Polling & Sinyal Real-Time (*Reactive Clearance*)

Untuk mengantisipasi perubahan status seketika (misal saat kasir menyetujui clearance atau mendadak mencabut clearance karena tagihan susulan):
- Hook `useInpatientBillingStatus` menerapkan polling cerdas (*smart polling*) setiap **30 detik** saat berada di halaman Detail Episode Pasien, dan polling cepat setiap **10 detik** saat Modal Pemulangan Pasien (`FE-INP-06`) sedang dibuka.
- Jika status clearance berubah dari `Cleared` menjadi `Revoked`, UI seketika memicu notifikasi peringatan (*Alert Banner*) merah:
  > *"Peringatan: Persetujuan kasir telah dibatalkan! Terdapat tagihan susulan. Tombol pemulangan fisik dikunci kembali."*
- Tombol `Konfirmasi Pasien Pulang Fisik` langsung berubah menjadi disabled (*Auto-Reblock*).

---

## 4. Skema Visual dan State Interaksi

### 4.1 Status Lencana Operasional Kasir (`BillingStatusBadge`)

| Status Kasir | Warna Lencana (Tailwind) | Teks yang Tampil | Keterangan Operasional untuk Perawat |
|---|---|---|---|
| `OPEN` / `None` | `bg-blue-100 text-blue-800 border-blue-300` | **Tagihan Berjalan** | Pasien masih dalam perawatan aktif; tagihan terbuka normal. |
| `PENDING_CLEARANCE` | `bg-amber-100 text-amber-800 border-amber-300` | **Menunggu Kasir** | DPJP sudah izinkan pulang; keluarga diarahkan ke kasir untuk pelunasan. |
| `CLEARED` | `bg-emerald-100 text-emerald-800 border-emerald-300` | **Clearance Disetujui** | Administrasi kasir beres; tombol pemulangan fisik aktif. |
| `REVOKED` | `bg-rose-100 text-rose-800 border-rose-300 animate-pulse` | **Clearance Dicabut** | Ada tagihan susulan (*late charge*); pasien dilarang keluar fisik (*Auto-Reblock*). |
| `OVERRIDDEN` | `bg-purple-100 text-purple-800 border-purple-300` | **Override Supervisor** | Dilepaskan atas izin darurat medis oleh Supervisor Bangsal. |
| `CLOSED` | `bg-slate-100 text-slate-700 border-slate-300` | **Tagihan Selesai** | Invoice kasir sudah final; episode rawat inap ditutup. |

---

## 5. Alur Interaksi Pengguna di Antarmuka Bangsal

### 5.1 Alur Normal: Pelunasan Kasir dan Pelepasan Fisik (Happy Path)
1. Perawat membuka halaman Detail Pasien Budi Santoso (`FE-INP-04`).
2. DPJP telah mengisi instruksi pulang medis. Lencana kasir berubah menjadi `Menunggu Kasir` (Kuning).
3. Perawat mengarahkan keluarga pasien ke loket kasir utama.
4. Keluarga melunasi tagihan di kasir.
5. Dalam waktu kurang dari 10 detik, polling mendeteksi clearance kasir. Lencana berubah menjadi hijau: `Clearance Disetujui`.
6. Perawat menekan tombol `Proses Pemulangan Fisik`.
7. Modal Pemulangan (`FE-INP-06`) terbuka: Tombol hijau `Konfirmasi Pasien Pulang Fisik` berstatus aktif.
8. Perawat mengklik tombol tersebut → Waktu kepulangan fisik tercatat presisi (`PhysicallyLeftAt = Sekarang`) → Tempat tidur berubah status menjadi `Perlu Pembersihan`.

### 5.2 Alur Pengecualian: *Auto-Reblock* dan *Supervisor Override* (Exception Path)
1. Pasien sudah berstatus `Clearance Disetujui` (Hijau).
2. Tiba-tiba kasir mencabut clearance karena bagian Farmasi memasukkan resep obat darurat.
3. Layar perawat mendeteksi sinyal pencabutan: Lencana berubah menjadi merah berkedip `Clearance Dicabut`, muncul banner peringatan, dan tombol `Konfirmasi Pasien Pulang Fisik` seketika terkunci (*disabled*).
4. Namun, pasien mendadak drop dan membutuhkan evakuasi ambulans segera ke RS rujukan spesialis jantung.
5. Supervisor Bangsal (yang memiliki hak `InpatientSupervisor:Override`) menekan tombol darurat `Supervisor Override`.
6. Modal `SupervisorOverrideModal.jsx` muncul:
   - Menampilkan peringatan resiko keuangan.
   - Menyediakan kolom input wajib: `Alasan Kedaruratan Klinis / Rujukan`.
   - Meminta input PIN Supervisor untuk otorisasi tanda tangan digital.
7. Supervisor mengetik alasan: *"Pasien darurat syok kardiogenik, rujukan ambulans prioritas 1 ke RS Harapan Kita. Administrasi kasir dilanjutkan pihak keluarga penjamin di loket kasir."*
8. Supervisor klik `Setujui Pelepasan Darurat`.
9. Sistem mengesahkan kepulangan fisik, mencatat stempel waktu dan ID Supervisor, serta menerbitkan event outbox pelepasan bed ke kasir.

---

## 6. Amandemen kontrak `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

Bagian ini menggantikan bagian 1 s.d. 5 untuk setiap layar yang disebut di bawah. Komponen yang memanggil webhook, supervisor override, atau `confirm-physical-discharge` **dicabut**.

### 6.1 Kebutuhan layar

| ID | Layar | Jenis | Pelaku | Kemampuan |
|---|---|---|---|---|
| `FE-INT-01` | Keluar ruangan dengan peringatan kasir | Dialog pada Detail Episode `FE-INP-04` / Pencatatan Kepergian `FE-INP-14` | Perawat pelaksana, kepala ruangan, petugas admisi, supervisor | `CAP-RWF-01` |
| `FE-INT-02` | Gerbang kasir pada Penutupan Episode | Bagian dari `FE-INP-07` | Petugas admisi, supervisor | `CAP-RWF-01`, `CAP-RWF-03` |
| `FE-INT-03` | Daftar "Pulang sebelum izin kasir" | Daftar di dalam Daftar Pantau `FE-INP-09` | Kasir, admisi, kepala ruangan | `CAP-RWF-01` |
| `FE-INT-04` | Koreksi penempatan | Dialog pada riwayat penempatan di Detail Episode `FE-INP-04` | Kepala ruangan, petugas admisi | `CAP-RWF-04` |
| `FE-INT-05` | Antrean "perlu diperiksa" kasir | Daftar dan tanda pada layar invoice kasir (`billing-management`) | Kasir | `CAP-RWF-04` |
| `FE-INT-06` | Kartu status kasir (pengganti `discharge-clearance-gate-card.jsx`) | Kartu pada Detail Episode dan ruang kerja keperawatan | Seluruh pemegang `InpatientBillingOperational : Read` | `CAP-RWF-03` |

Layar Tagihan Pasien di bangsal adalah milik `keperawatan` (`FE-KEP-23`); sub-modul ini hanya menyediakan endpoint-nya.

### 6.2 Peta butir menu

Sub-modul ini **tidak menambah butir menu**. Peta seluruh modul dipegang `02-module-map.md` bagian 7.3.

| Layar | Jalan masuk | Butir hak akses yang menjaga |
|---|---|---|
| `FE-INT-01` | Tombol "Catat pasien meninggalkan ruangan" pada `FE-INP-04` dan `FE-INP-14` | `InpatientDischarge : RecordDeparture` |
| `FE-INT-02` | `FE-INP-07` | `InpatientDischarge : Read`; tombol Tutup `InpatientDischarge : Close`; tombol override `InpatientDischarge : CloseOverride` |
| `FE-INT-03` | Daftar baru pada `FE-INP-09`, diletakkan di antara daftar milik `episode-rawat-inap` | `InpatientMonitoring : Read` |
| `FE-INT-04` | Riwayat penempatan pada `FE-INP-04` | `InpatientBedOccupancy : Correct` |
| `FE-INT-05` | Butir menu kasir yang sudah ada (Billing → Invoice); daftar ditambahkan pada halaman itu | `BillingInvoice : Read` |
| `FE-INT-06` | `FE-INP-04` dan ruang kerja keperawatan `FE-KEP-07` | `InpatientBillingOperational : Read` |

### 6.3 Skema fitur per layar

#### `FE-INT-01` Keluar ruangan dengan peringatan kasir

```text
+- Catat pasien meninggalkan ruangan -------------------------- FE-INT-01 -+
| Tn. Contoh A  •  Melati 2 Bed 3  •  Boleh pulang 4 Okt 08.30            |
| Waktu keluar  [04/10/2026 09.05]                                        |
| [ status kasir: badge ]                                                 |
|   CLEARED   -> tanpa peringatan                                         |
|   lainnya   -> ! Kasir belum memberi izin pulang                        |
|                 Kendala: <daftar kendala tanpa rupiah>                  |
|   gagal     -> ! Status kasir tidak dapat dibaca                        |
|                                    [Batal]  [Catat keluar ruangan]      |
+-------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Kepala | Nama pasien, bed, waktu keputusan pulang | Data Detail Episode yang sudah dimuat | `InpatientEpisode : Read` | — |
| Badge status kasir | Status dan kendala | `GET episodes/{episodeId}/billing-status` | `InpatientBillingOperational : Read` | Gagal → "Status kasir tidak dapat dibaca"; tombol tetap aktif dengan peringatan |
| Tombol Catat | Kirim `ClearanceWarningAcknowledged = false` lebih dulu | `POST discharges/{episodeId}/record-departure` | `InpatientDischarge : RecordDeparture` | 409 `INP-DEP-001` → tampilkan peringatan, ganti tombol menjadi "Tetap catat keluar ruangan", kirim ulang dengan `true` |
| Hasil | "Pasien tercatat keluar pukul 09.05. Bed sudah kosong." ditambah peringatan tindak lanjut bila ada | Respons `InpatientDepartureResponse` | — | — |

#### `FE-INT-02` Gerbang kasir pada Penutupan Episode

```text
+- Penutupan Episode — syarat ---------------------------------- FE-INP-07 -+
| 1 Keputusan pulang DPJP        [ok]                                      |
| 2 Resume ditandatangani        [ok]                                      |
| 3 Butir administrasi           [ok]                                      |
| 4 Izin kasir (dibaca langsung) [ badge ]  diperbarui tiap 10 detik       |
| 5 Bed sudah dilepas            [ok]                                      |
|                         [Tutup Episode]   [Tutup tanpa izin kasir...]   |
+--------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Daftar syarat | Lima syarat; syarat 4 dari Billing | `GET discharges/{episodeId}/closure-readiness`, disegarkan 10 detik | `InpatientDischarge : Read` | Gagal → "Syarat penutupan gagal dimuat." [Coba lagi] |
| Tombol Tutup Episode | Aktif hanya bila seluruh syarat terpenuhi | `POST discharges/{episodeId}/close` | `InpatientDischarge : Close` | 422 `INP-CLS-010`/`011` → banner merah dengan pesan server |
| Tombol tanpa izin kasir | Dialog alasan wajib | `POST discharges/{episodeId}/close-with-override` | `InpatientDischarge : CloseOverride` | Disembunyikan bagi yang tidak berhak. **Tidak ada isian PIN** |

#### `FE-INT-03` Daftar "Pulang sebelum izin kasir"

```text
+- Pulang sebelum izin kasir ----------------------------------- FE-INT-03 -+
| [Unit v] [Periode v] [x] Termasuk episode sudah ditutup                   |
| Episode | Pasien | Unit | Keluar | Dicatat oleh | Kasir saat keluar | Kasir sekarang |
| memuat -> kerangka baris                                                  |
| kosong -> "Tidak ada pasien yang pulang sebelum izin kasir pada saringan ini."
| gagal  -> "Data gagal dimuat." [Coba lagi]                                |
+---------------------------------------------- [< Sebelumnya] [Berikutnya >]+
```

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Tabel | Kolom sesuai `DepartureBeforeClearanceItem` | `GET monitoring/departures-before-clearance` | `InpatientMonitoring : Read` | Lihat skema |
| Kolom kasir sekarang | `Unreadable` tampil "Tidak dapat dibaca" | Sama | — | — |
| Baris | Tautan ke Detail Episode | — | `InpatientEpisode : Read` | — |

#### `FE-INT-04` Koreksi penempatan

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Riwayat penempatan | Baris koreksi bertanda "koreksi" dan baris lama tercoret beserta alasan | `GET bed-occupancies/placements/by-episode/{episodeId}` (ditambah `CorrectsPlacementId`, `SupersededByCorrectionId`) | `InpatientBedOccupancy : Read` | Kosong → "Belum ada penempatan." |
| Tombol Koreksi | Pada baris yang berlaku saja | — | `InpatientBedOccupancy : Correct` | Disembunyikan bila tidak berhak |
| Dialog | Bed, kelas, waktu mulai/selesai, alasan | `POST bed-occupancies/placements/{placementId}/corrections` | Sama | 422 `INP-COR-001` → "Tagihan sudah difinalkan, hubungi kasir"; 409 → muat ulang |

#### `FE-INT-05` Antrean "perlu diperiksa" kasir

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Daftar | Nomor invoice, pasien, alasan, sejak kapan | `GET billing/invoices/review-queue` | `BillingInvoice : Read` | Kosong → "Tidak ada invoice yang perlu diperiksa." |
| Tanda pada invoice | "Perlu diperiksa: biaya kamar manual dan otomatis" | `GET billing/invoices/{id}` | `BillingInvoice : Read` | — |
| Tombol Selesai diperiksa | Dialog catatan | `POST billing/invoices/{id}/review-resolution` | `BillingInvoice : Update` | 422 `BIL-REV-001` → pesan server |

#### `FE-INT-06` Kartu status kasir

Menggantikan `src/components/features/health-services/inpatient-management/billing-integration/discharge-clearance-gate-card.jsx`. Penyegaran 10 detik (`pollingIntervalMs = 10000`) dipertahankan; status `OVERRIDDEN` dibuang dari kosakata layar (`FIN-CON-04`).

| Wilayah | Isi | Sumber data | Butir hak akses | Bila kosong atau gagal |
|---|---|---|---|---|
| Badge | `PENDING` "Menunggu kasir", `BLOCKED` "Terkendala", `CLEARED` "Disetujui kasir", `REVOKED` "Izin dicabut" | `GET episodes/{episodeId}/billing-status` | `InpatientBillingOperational : Read` | Gagal → "Status kasir tidak dapat dibaca" |
| Kendala | Daftar label tanpa rupiah | Sama | Sama | Kosong → tidak ditampilkan |
| Tanda episode | "Ditutup tanpa izin kasir" bila `IsClosedWithoutFinancialClearance` | Sama | Sama | — |

### 6.4 Aksi per peran

Diturunkan dari `contracts/permission-audit-matrix.md` 5.3, tidak dikarang ulang. Tombol yang tidak berhak **disembunyikan**.

### 6.5 Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | Kerangka baris atau kerangka kartu, bukan layar kosong |
| Data basi | Kartu status kasir dan syarat penutupan menyegarkan diri tiap 10 detik. Server tetap memeriksa ulang saat tombol ditekan, sehingga layar basi tidak dapat meloloskan penutupan |
| Pengiriman ganda | Tombol dinonaktifkan selama permintaan berjalan. Keluar ruangan kedua dijawab 409 oleh server |
| Rupiah | Tidak ada satu pun layar sub-modul ini yang menampilkan rupiah kepada peran bangsal |

### 6.6 Kewenangan UI

| Hal | Kewenangan |
|---|---|
| Keberadaan peringatan kasir dan pengakuan sekali klik | Mengikat (`RWI-DEC-186` butir 2) |
| Tidak ada isian PIN dan tidak ada isian alasan pada keluar ruangan | Mengikat (`RWI-DEC-186`, `187`) |
| Letak daftar `FE-INT-03` di dalam `FE-INP-09` | Mengikat pada urutan kelompok `episode-rawat-inap` (`02-module-map.md` 3.5); posisi di dalam kelompok `DEV_DISCRETION` |
| Bentuk dialog, warna badge, ikon | `DEV_DISCRETION` |
