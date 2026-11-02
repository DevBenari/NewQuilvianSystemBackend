# Laporan Perubahan Backend — `BE-HRD-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-026` |
| Judul | Layanan dan Controller Profil 360 Derajat Pegawai (Employee 360) |
| Slice | `S-A1` (Workforce Core & Employee 360) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 10 |
| Trace | `FR-HRD-015`, `FR-HRD-016`, `HRD-DEC-052`, `HRD-DEC-053` |
| Dependency | `BE-HRD-023` |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | DTOs, Service, Controller, Program.cs, Laporan Task |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang Diselesaikan

Sebelumnya, informasi pegawai rumah sakit terpecah di berbagai tabel tanpa pandangan menyeluruh (360 derajat). Selain itu, terdapat risiko kebocoran data privasi dan finansial jika atasan langsung dapat melihat data gaji pokok, rekening bank, NIK, susunan keluarga, atau catatan sanksi disiplin bawahan mereka. Sesuai keputusan `HRD-DEC-053`, diperlukan mekanisme otorisasi berlapis (dual-mode security) yang memisahkan profil lengkap untuk HR Administrator dari profil kedinasan terbatas untuk Atasan Langsung.

---

## 2. Proses Bisnis Rumah Sakit

* **Rute API**: `GET /api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360`
* **Evaluasi Akses & Dual-Mode Security**:
  1. **Mode HR Administrator**:
     - Pengguna memiliki hak baca penuh HR atau role Admin.
     - Melihat seluruh data profil kedinasan, riwayat SK, riwayat penempatan, sertifikasi/kompetensi.
     - Melihat profil finansial konfidensial (`ConfidentialFinancialProfile`): gaji pokok, struktur gaji, nomor rekening bank, dan nama bank.
     - Melihat profil pribadi konfidensial (`ConfidentialPersonalAndDiscipline`): NIK, daftar keluarga, dan catatan sanksi disiplin aktif.
  2. **Mode Atasan Langsung (Direct Manager)**:
     - Pengguna terdaftar sebagai `IsPrimaryManager = true` aktif pada penempatan manajer bawahan tersebut.
     - Hanya melihat profil kedinasan, unit kerja, jabatan, riwayat mutasi/promosi, dan kompetensi bawahan.
     - `ConfidentialFinancialProfile` **wajib null** pada payload API.
     - `ConfidentialPersonalAndDiscipline` **wajib null** pada payload API.
  3. **Penolakan Akses (403 Forbidden)**:
     - Jika pemanggil bukan HR dan bukan atasan langsung aktif dari pegawai yang bersangkutan, akses ditolak dengan pesan jelas.

---

## 3. Rincian Perubahan yang Dikerjakan

1. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/DTOs/Employee360Dtos.cs` — DTO respons Employee 360 dengan pemisahan bagian publik dan konfidensial.
2. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/IHrdEmployee360Service.cs` — Antarmuka domain service Employee 360.
3. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/HrdEmployee360Service.cs` — Implementasi logika agregasi profil multi-tabel dan penegakan dual-mode security.
4. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdEmployee360Controller.cs` — Controller Web API dengan otorisasi dan respons terstruktur.
5. `NewQuilvianSystemBackend/Program.cs` — Pendaftaran service Employee 360 ke Dependency Injection container.

---

## 4. Verifikasi Keamanan Data Pribadi

* Atasan langsung tidak dapat mengekstrak nominal gaji atau NIK pegawai dari API payload karena atribut tersebut diputus di level C# service sebelum respons di-serialize ke JSON.
