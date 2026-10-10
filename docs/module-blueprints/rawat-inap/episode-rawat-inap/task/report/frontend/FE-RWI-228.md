# Laporan Perubahan Frontend — `FE-RWI-228`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-228` |
| Judul | Komponen Tampilan Antrean Rujukan Transfer IGD/RJ & Filter Status Menunggu |
| Slice | Slice Transfer IGD / RJ; Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/frontend-roadmap-admisi-transfer-igd.md`](../../../roadmap/frontend-roadmap-admisi-transfer-igd.md) — kartu `FE-RWI-228` |
| Trace | `FR-RI-187`, `FR-RI-188`, `FR-RI-190`; `RWI-DEC-274`, `RWI-DEC-276`; `RWI-AC-396`, `RWI-AC-397`, `RWI-AC-401`; API Contract `0.12.0` (11.7.1) |
| Contract version | `0.12.0` **`approved`** (10 Oktober 2026) |
| Dependency | `BE-RWI-206` ✅, `FE-RWI-227` ✅ |
| Klasifikasi | `HIGH` — Komponen antrean transfer terpadu, integrasi Axios service, filtering overdue > 60m & pencarian |
| Task mode | `FRONTEND` |
| Target tulis | `inpatient-admission-transfer.service.js`, `inpatient-admission-transfer-list.jsx` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-396`, `RWI-AC-397`, dan `RWI-AC-401` terimplementasi bersih dan tervalidasi. |

---

## 1. Kebutuhan Bisnis

Sesuai kebutuhan Analisis Bisnis (Tahap A dan B alur admisi transfer IGD):
1. **Daftar Antrean Pasien IGD (`RWI-DEC-274`, `RWI-AC-396`):** Petugas admisi di loket membutuhkan layar tabel yang menampilkan pasien yang dirujuk dari IGD secara *real-time* lengkap dengan nama, No. RM, jenis kelamin, dan nomor kunjungan IGD.
2. **Peringatan Waktu Tunggu Overdue (`RWI-DEC-276`, `RWI-AC-397`):** Menampilkan durasi berapa lama pasien menunggu kamar sejak instruksi dokter IGD. Pasien yang menunggu lebih dari 60 menit ditandai dengan sorotan warna kuning/merah (*Overdue*) dan dapat difilter secara khusus.
3. **Pencarian Cepat & Tombol Aksi:** Petugas dapat mencari berdasarkan nama pasien atau No. RM saat keluarga mendatangi loket, lalu mengklik tombol `[Proses Admisi]` untuk membuka stepper pendaftaran.

---

## 2. Rincian Perubahan Source Code

### 2.1 `src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js`
- Membuat service API terstruktur `inpatientAdmissionTransferService` dengan Axios instance:
  - `getAdmissionTransfers(params)` — mengambil daftar antrean pasien transfer dari endpoint `/v1/health-services/inpatient-management/admission-transfers`.
  - `getAdmissionTransferById(id)` — mengambil detail satu rujukan transfer.
  - `admitFromTransfer(payload)` — mengirim payload eksekusi admisi ke `/admit`.

### 2.2 `src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.jsx`
- Membuat komponen antarmuka reaktif `InpatientAdmissionTransferList`:
  - Panel header dengan badge total pasien dan badge peringatan pasien overdue.
  - Fitur pencarian instan (nama, No RM, no kunjungan) dan tombol toggle filter overdue (> 60m).
  - Tabel antrean dengan kolom Pasien, Asal Kunjungan (Badge Merah IGD), Waktu & Lama Tunggu, DPJP Dituju, Diagnosa Klinis IGD, dan Tombol `[Proses Admisi]`.
  - Penanganan status kosong (*empty state*), loading spinner, dan pesan error jika jaringan gagal.

---

## 3. Bukti Verifikasi
- Tabel antrean terisi secara otomatis dengan data dari backend.
- Filter overdue menyaring pasien yang menunggu > 60 menit secara tepat.
- Tombol "Proses Admisi" memicu callback `onSelectTransfer` ke komponen induk.
