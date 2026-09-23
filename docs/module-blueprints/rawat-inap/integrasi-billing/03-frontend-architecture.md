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
