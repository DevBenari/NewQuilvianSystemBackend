# Sub-modul Episode Rawat Inap — Desain Amandemen Admisi Operasi & Bedah Obgyn

| Metadata | Nilai |
| :--- | :--- |
| **Dokumen ID** | `DSN-RWI-EPS-20261023` |
| **Sub-Modul** | `HealthServices / InPatientManagement / episode-rawat-inap` |
| **Status Dokumen** | `PROPOSED DRAFT` (Menunggu Pengesahan Pemilik Kebutuhan) |
| **Tanggal Terbit** | 23 Oktober 2026 |
| **Dasar Keputusan** | `RWI-DEC-355` s.d. `RWI-DEC-358` & `OPS-DEC-038` s.d. `OPS-DEC-042` |
| **Dokumen Utama Lintas Modul** | [05-desain-amandemen-operasi-dan-admisi.md](../../operations/05-desain-amandemen-operasi-dan-admisi.md) |

---

## 1. Pokok Amandemen pada Admisi Rawat Inap

1. **Alur Pre-Op Admission untuk Operasi Terencana (`RWI-DEC-355`)**:
   * Pasien operasi elektif dan bedah persalinan SC mendaftar dan memperoleh kamar rawat inap terlebih dahulu di bangsal sebelum tindakan bedah.
   * Ranjang bangsal berstatus `Occupied / In-Use (Sedang Operasi)` selama pasien diantar ke IBS dan kamar pulih.
   * Pasien kembali ke kamar bangsal semula melalui serah terima (*Handover*) antar-perawat tanpa admisi ulang.

2. **Reposisi Kartu Admisi Kamar Pulih (`RWI-DEC-355`)**:
   * Kartu *"Admisi dari Kamar Pulih"* pada beranda Admisi (`/health-services/inpatient-management/admissions`) direposisi menjadi **Jalur Eskalasi Kasus ODC / Komplikasi Tak Terduga**.
   * Diberi penanda visual (*warning badge*) bahwa jalur ini bukan untuk operasi umum, melainkan khusus pasien rawat jalan/ODC yang harus dialihkan opname karena kondisi pasca-bedah memburuk.

3. **Handshake Tautan Jadwal IBS Read-Only (`RWI-DEC-356`)**:
   * Di Langkah 1 Admisi (*Pencarian Pasien & Rujukan*), sistem memanggil API `GET /api/v1/health-services/operating-room-management/schedules/lookup-by-patient`.
   * Data jadwal operasi (No. Booking, Ruang OK, Jam, Operator, Anestesi, Anak) tampil secara *read-only*. Petugas admisi tidak memilih ruang bedah.

4. **Kategori Kasus Bedah Obgyn & Rawat Gabung Boks Bayi (`RWI-DEC-357`)**:
   * Pilihan sub-kategori: `Bedah Obstetri (Sectio Caesarea / SC)` dan `Bedah Ginekologi (Kista / Miom / Kuretase)`.
   * Khusus SC: Filter kamar rawat inap otomatis mengunci **Bangsal Maternitas / Ruang Nifas**, dan disediakan toggle **[✓] Fasilitas Rawat Gabung (*Rooming-in*)** untuk mencadangkan 1 unit boks bayi di samping ranjang ibu.

5. **Skema Paket Melahirkan (Maternity Package) & Billing (`RWI-DEC-358`)**:
   * Mendukung tarif paket terpadu (Paket SC ERACS Kelas 1 / VIP) yang mencakup kamar OK, jasa tri-dokter, dan perawatan bayi dasar.
   * Biaya di luar paket otomatis ditagihkan sebagai *excess bill*. Untuk pasien BPJS, klaim ditagihkan sesuai tarif INA-CBGs persalinan.

---

## 2. Rincian Teknis Kontrak & Flowchart

Rincian lengkap diagram alur bisnis, kontrak endpoint API gaya Swagger, skema entity, dan spesifikasi layar UI tercatat secara terpadu pada dokumen kanonis lintas modul:  
👉 [Dokumen Desain Arsitektur Lengkap (DSN-OPR-RWI-20261023)](../../operations/05-desain-amandemen-operasi-dan-admisi.md).
