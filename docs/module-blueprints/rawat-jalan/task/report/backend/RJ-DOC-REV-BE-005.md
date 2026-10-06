# Laporan Perubahan Backend — `RJ-DOC-REV-BE-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-005` |
| Judul | Master data aturan layanan dokter dan relasinya |
| Slice | Revisi UAT `2026-09-28` — kebutuhan data pendukung Tindakan/Resep/Billing; permintaan pemilik "buatkan data dan relasinya" |
| Roadmap | `roadmap/doctor-consultation-roadmap.md` bagian `9.2` |
| Trace | `RJ-DOC-DEC-007`; temuan audit `A3`, `A4`; `RJ-E2E-DEC-012`, `RJ-E2E-DEC-023` (urutan resolver tarif konsultasi) |
| Contract version | Tidak berubah |
| Dependency | `RJ-DOC-REV-BE-004` untuk skrip hak akses Surat Dokter |
| Klasifikasi | `MEDIUM` — data saja; nol berkas aplikasi diubah, dua skrip SQL baru |
| Task mode | `BACKEND` |
| Target tulis | `Migrations/scripts/**`; data `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `33ee2955` cabang `sukmagp` + working tree `RJ-DOC-REV-BE-001..004` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ Selesai |

---

## 1. Masalah yang diperbaiki

Audit `QuilvianNewDevSukma` `1/10/2026`:

| Master | Jumlah | Keadaan |
| --- | ---: | --- |
| `MstProcedure` | `9455` | Cukup. `5495` lolos filter tindakan dokter rawat jalan |
| `MstDrug` | `7862` | Cukup |
| `MstTariff` | `351769` | Cukup. `2130` tarif konsultasi (`IsConsultationFee`) untuk `113` tindakan konsultasi, `19` kelas |
| `MstTariffCategory` | `13` | Cukup (termasuk `CONSULTATION`) |
| `MstPatientClass` | `21` | Cukup |
| `MstServiceUnit` / `MstClinic` | `24` / `57` | Cukup |
| `MstDoctor` | `13` (11 aktif berjadwal) | Cukup |
| **`MstDoctorServiceRule`** | **`2`** | **Kurang** — keduanya data uji; **nol** aturan konsultasi untuk dokter aktif |

Tanpa aturan layanan dokter, Billing tidak menemukan tarif jasa konsultasi dari jalur aturan dokter
(`BillingSourceTariffResolver` urutan 1/1b), dan tarif klinik (urutan 2/3) juga kosong karena tidak
ada tarif konsultasi berklinik. Akibatnya jasa konsultasi masuk antrean `TariffNotFound`.

Hak akses Surat Dokter (`DoctorCertificate`) dari `RJ-DOC-REV-BE-004` juga belum dipegang jabatan mana pun.

---

## 2. Proses bisnis

1. Dokter memiliki jadwal praktik per unit layanan dan klinik (`MstDoctorSchedule`).
2. Untuk setiap pasangan (dokter, unit, klinik) pada jadwal, dibuat satu **aturan layanan konsultasi**.
3. Aturan menunjuk **tindakan konsultasi**:
   - Poli Umum dan Medical Check Up → `POLI_22377` *Jasa Konsultasi Medik Umum*.
   - Klinik lain → `POLI_26530` *Jasa Konsultasi Medik Spesialis*.
4. Saat konsultasi selesai, Billing mencari tarif tindakan itu untuk **kelas pasien** kunjungan.
   Contoh: dr. Maya Permata Sari, Poli Gigi, pasien `KELAS I` → tarif `POLI_26530-13` **Rp424.000**;
   pasien `RAWAT JALAN` → `POLI_26530-0` Rp424.000. Dr. Rendy Pangalila, Poli Umum → `POLI_22377-13` Rp242.000.
5. Kelas pasien dan tarif sengaja tidak diisi pada aturan supaya harga mengikuti kelas pasien
   (mengisi `TariffId` akan memaksa satu harga untuk semua kelas).
6. Hak akses `DoctorCertificate : Read/Create/Update/Cancel` diberikan kepada jabatan yang sudah
   memegang aksi bernama sama pada `DoctorConsultation`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstDoctorServiceRule.cs`, `DoctorServiceRuleType`/`Status`, `MstTariff.cs`, `BillingSourceTariffResolver.cs`,
`DoctorServiceRuleController.cs` (pola kode `DSR-RSMMC-`), `Migrations/scripts/README.md`,
`grant-stock-request-access.sql` (pola grant), schema & index `MstDoctorServiceRule`, `SysAccessPolicy`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/scripts/seed-doctor-service-rules-outpatient.sql` (baru) | Seed idempoten aturan konsultasi dari jadwal dokter, dengan penjaga prasyarat |
| `Migrations/scripts/grant-doctor-certificate-access.sql` (baru) | Grant idempoten hak akses Surat Dokter mengikuti pemegang `DoctorConsultation` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Data saja, tanpa perubahan schema. `20` baris `MstDoctorServiceRule`, `12` baris `SysAccessPolicy` (4 aksi × 3 jabatan) di `QuilvianNewDevSukma` |
| Keamanan/Auth | Hak akses diberikan per **jabatan**, bukan orang, lewat data — bukan hardcode. Dapat dicabut di layar Akses Role |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint yang berubah.

---

## 5. Verifikasi

| Skenario | Hasil sebenarnya | Klasifikasi |
| --- | --- | :---: |
| R1 Jalankan seed di `QuilvianNewDevSukma` | `20` aturan `DSR-RJ-*` untuk `11` dokter aktif berjadwal | `PASS` |
| R2 Jalankan seed kedua kalinya | Tetap `20` — idempoten | `PASS` |
| R3 Emulasi resolver urutan 1b untuk setiap aturan baru | `0` aturan tanpa tarif konsultasi. Contoh pada bagian 2 langkah 4 | `PASS` |
| R4 `GET /health-services/master-data/doctor-service-rules?search=DSR-RJ` | `200`, `totalData = 20`; relasi terbaca: dokter, unit `Rawat Jalan`, klinik, kategori `Consultation`, tindakan konsultasi | `PASS` |
| R5 Jalankan grant dua kali | `Read/Create/Update/Cancel` masing-masing `3` jabatan; jalankan ulang tidak menambah | `PASS` |
| `dotnet build`, `has-pending-model-changes`, QBE | — | `NOT RUN` — tidak ada perubahan source C# |

### 5.1 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| `MstDoctorServiceRule` | `22` baris: `20` baru `DSR-RJ-*` + `2` data uji lama (tidak disentuh) |
| `SysAccessPolicy` | `+12` baris untuk `DoctorCertificate` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seeder idempoten: setiap dokter aktif berjadwal punya `MstDoctorServiceRule` konsultasi per (unit, klinik) | Terpenuhi — berbentuk skrip SQL idempoten mengikuti pola `Migrations/scripts`, bukan seeder startup (lihat 7) | R1, R2 |
| Menunjuk tindakan konsultasi bertarif `IsConsultationFee` | Terpenuhi | R3 |
| Relasi `MstDoctor`, `MstServiceUnit`, `MstClinic`, `MstTariffCategory`, `MstTariff`, `MstProcedure`, `MstPatientClass` valid | Terpenuhi — `MstTariff`/`MstPatientClass` dirujuk lewat tindakan, sengaja tidak dikunci pada aturan | R3, R4 |
| Resolver tarif konsultasi menemukan tarif untuk kelas `RAWAT JALAN` | Terpenuhi | R3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Skrip SQL dipilih alih-alih seeder startup supaya data bisnis tidak otomatis tertanam di setiap database yang menjalankan aplikasi. Lingkungan lain menjalankan skrip ini secara sadar |
| Masalah yang diketahui | Tarif konsultasi impor berkategori `OTHER`, sementara aturan berkategori `CONSULTATION`. Resolver tidak memakai kategori, sehingga tidak berdampak harga; penyamaan kategori tarif adalah wewenang pemilik Billing |
| Risiko tersisa | Pemilihan tindakan konsultasi (umum vs spesialis) berdasarkan nama klinik — tinjau oleh pemilik bila ada klinik yang tarifnya berbeda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` dua skrip SQL dan laporan ini |
| Langkah berikutnya | `RJ-DOC-REV-FE-004`/`FE-005` menampilkan master obat/tindakan; daftarkan kedua skrip di `Migrations/scripts/README.md` bila pemilik setuju |
