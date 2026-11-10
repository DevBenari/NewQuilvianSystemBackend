# Laporan Perubahan Backend — `BE-HRD-060`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-060` |
| Judul | API Bank Kandidat Cadangan (Talent Pool) & Promosi Cepat Fast-Track |
| Slice | `S-D2` (Rekrutmen & Hiring / Recruitment Management) |
| Milestone | `MVP-13` (Persetujuan Bertingkat, MCU Pra-Kerja K3RS, Talent Pool & Serah Terima Onboarding) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-13 |
| Traceability | `HRD-DEC-080`, `FR-REC-11`, `FR-REC-12`, `VR-REC-18`, `VR-REC-19`, `AT-HRD-D2-10`, `UAT-REC-07` |
| Dependency | `BE-HRD-056` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | DTOs, Service, Interface, Controller, DI Program.cs, Laporan Task, Roadmap |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Dalam seleksi tenaga kesehatan rumah sakit (seperti Perawat Hemodialisa atau Radiografer), seringkali terdapat pelamar-pelamar unggul yang lulus seluruh tahapan seleksi klinis namun tidak dapat direkrut seketika karena keterbatasan kuota alokasi kursi WFP yang dibuka pada periode tersebut. Ketika kandidat utama yang menerima penawaran tiba-tiba mengundurkan diri (*Offer Rejected*) atau tidak hadir pada hari pertama kerja (*No-Show*), rumah sakit mengalami kekosongan pelayanan kritis bila harus mengulang proses rekrutmen dari awal yang memakan waktu 1–2 bulan.

Melalui task `BE-HRD-060`, sistem menyediakan bank data kandidat cadangan (*Talent Pool*) berperingkat:
1. **Pendaftaran Kandidat Cadangan Berperingkat**: Kandidat berkualitas yang belum terisi kuotanya didaftarkan ke Talent Pool lengkap dengan skor gabungan wawancara dan uji klinis serta posisi peringkatnya.
2. **Batas Retensi Maksimal 6 Bulan (`VR-REC-18`)**: Hak promosi kandidat dibatasi maksimal 6 bulan kalender sejak pendaftaran. Jika melewati batas retensi, sistem otomatis menandai status `Expired` dan menolak promosi cepat guna memastikan kompetensi klinis tetap mutakhir.
3. **Promosi Cepat Instan (*Fast-Track Promotion* — `VR-REC-19`, `AT-HRD-D2-10`)**: Mengizinkan HR mempromosikan kandidat cadangan peringkat teratas langsung ke lowongan baru tanpa mengulang tes wawancara dan uji klinis, seketika menerbitkan draf penawaran kerja baru (`HrdJobOffer`), dan memperbarui status pool menjadi `Promoted`.

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

```mermaid
flowchart TD
    A["Kandidat Lulus Seleksi tapi Kuota Formasi Penuh"] --> B["Pendaftaran ke Talent Pool (Skor & Peringkat Dicatat)"]
    B --> C["Masa Retensi Berlaku Selama 6 Bulan (VR-REC-18)"]
    
    C --> D{"Terjadi Kursi Kosong (Kandidat Utama Batal / Lowongan Baru)"}
    
    D -->|Masih Dalam Masa Retensi (<= 6 Bulan)| E["Fast-Track Promotion: Promosi Cepat (VR-REC-19)"]
    D -->|Masa Retensi Lewat (> 6 Bulan)| F["Status Expired: Wajib Mengikuti Seleksi Baru"]

    E --> G["Draf Surat Penawaran Kerja Baru Otomatis Diterbitkan"]
    G --> H["Kandidat Langsung Masuk Tahap Penawaran Tanpa Tes Ulang"]
```

---

## 3. Komponen Perubahan yang Diterapkan

### 3.1 DTO Layer (`TalentPoolDtos.cs`)
- `EnrollTalentPoolRequest`: ID kandidat, ID lowongan asal, skor peringkat (0–100), urutan peringkat, dan durasi retensi bulan.
- `FastTrackPromoteRequest`: ID lowongan sasaran baru, gaji pokok yang diajukan, skala upah, dan catatan pengajuan.
- `TalentPoolResponse` & `TalentPoolListItemResponse`: Rincian data pelamar cadangan beserta indikator kedaluwarsa (`IsExpired`).
- `TalentPoolQuery`: Filter terstruktur per lowongan dan status keaktifan.

### 3.2 Service Layer (`ITalentPoolService.cs` & `TalentPoolService.cs`)
- Evaluasi masa retensi 6 bulan kalender pada saat proses promosi cepat dipanggil.
- Penerbitan draf penawaran instan dengan penentuan otomatis tier persetujuan gaji pokok (`Tier13Standard` vs `Tier14Exceptional`).
- Pembaruan status relasional antar entitas dalam transaksi database.

### 3.3 Controller Layer (`TalentPoolController.cs`)
- Route: `api/v1/corporate/human-resource/recruitment/talent-pool`
- Tag Swagger: `Corporate / Human Resource / Recruitment Management / Talent Pool`
- Otorisasi: `[Authorize]` dan `[AccessPermission("Recruitment", "Read"|"Create"|"Update")]`

| Method | Endpoint | Deskripsi | Auth / Permission |
| :--- | :--- | :--- | :--- |
| `GET` | `/` | Mengambil daftar kandidat cadangan berperingkat | Read |
| `GET` | `/{id}` | Mengambil rincian detail profil kandidat pada talent pool | Read |
| `POST` | `/enroll` | Mendaftarkan kandidat lulus seleksi ke dalam talent pool | Create |
| `POST` | `/{id}/fast-track-promote` | Menjalankan promosi cepat (*Fast-Track*) kandidat ke kursi lowongan | Update |

---

## 4. Bukti Verifikasi
- DTOs, Service, Interface, dan Controller terpasang lengkap.
- Layanan didaftarkan di `Program.cs`:
  `builder.Services.AddScoped<ITalentPoolService, TalentPoolService>();`
