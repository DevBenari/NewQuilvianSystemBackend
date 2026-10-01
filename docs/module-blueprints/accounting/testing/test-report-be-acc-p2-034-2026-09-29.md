# Laporan Pengujian Live API — BE-ACC-P2-034

**Tanggal Pengujian:** 29 September 2026  
**Pelaksana Pengujian:** Antigravity Pairing Agent  
**Modul:** Corporate / Accounting Management (`JournalManagement` & `AccountingEvent`)  
**Task ID:** `BE-ACC-P2-034`  
**Judul Perubahan:** Jurnal dari kejadian dikenali sebagai jalur otomatis saat diajukan (`ACC-DEC-064`)  
**Metode:** Automated Live Browser API Test (Playwright Session Context with Authenticated Cookies & Antiforgery)  
**Lingkungan:** 
- Frontend: `http://localhost:3000` (Next.js v20.20.2)
- Backend: `https://localhost:7184/api` (ASP.NET Core 9 / EF Core)
- Akun Uji: `superadmin@admin.com` (SuperAdmin)
- Badan Hukum: `3bf63974-a754-4b20-81ee-70894f6fb058` (PT Metropolitan Medical Centre)

---

## 1. Ringkasan Eksekutif

Pengujian resep **S1–S5** dari dokumen [`BE-ACC-P2-034.md`](../task/report/backend/BE-ACC-P2-034.md) bagian 5.2 telah berhasil dijalankan secara lengkap terhadap live environment tanpa intervensi langsung pada database (tanpa raw SQL).

Seluruh kriteria penerimaan berhasil dibuktikan:
1. **Pengenalan Jalur Otomatis Saat Diajukan (Acceptance 1):** Draft jurnal yang disusun secara otomatis dari kejadian akuntansi (`PATIENT_PAYMENT`) dan menyentuh control account (`1-1002 Kas Kasir` & `1-2001 Piutang Pasien Umum`) berhasil diajukan dengan status HTTP `200 OK` dan beralih ke `PendingApproval` (Menunggu Persetujuan). Sebelum task ini, pengajuan ini keliru ditolak dengan `422 Unprocessable Entity`.
2. **Perlindungan Akun Kontrol dari Jurnal Manual (Acceptance 4 / Regresi):** Pembuatan jurnal manual baru yang menyentuh `1-1002 Kas Kasir` tetap ditolak keras dengan status HTTP `422 Unprocessable Entity` (*"Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual."*).
3. **Penegakan Prinsip Empat Mata (Four-Eyes Principle / Acceptance 2):** Percobaan persetujuan jurnal (`approve`) oleh pengaju yang sama ditolak dengan status HTTP `403 Forbidden` (*"Anda tidak dapat menyetujui jurnal yang Anda buat sendiri."*).

---

## 2. Hasil Pengujian Rinci (Resep S1–S5)

| No | Langkah Uji | Endpoint & Metode | Hasil Aktual | Status | Kriteria Terbukti |
|:--:|:---|:---|:---|:--:|:---|
| **S1** | Pemeriksaan Akun Kontrol pada Draft `JU/2031/01/00001` | `GET /v1/corporate/accounting/journals?search=JU/2031/01/00001`<br>`GET /journals/{id}` | Status: Draft (`1`). Kedua baris menyentuh akun kontrol:<br>1. `1-1002 Kas Kasir` (Debit 200.000, `isControlAccount: true`)<br>2. `1-2001 Piutang Pasien Umum` (Kredit 200.000, `isControlAccount: true`) | **PASS** | Terbukti menyentuh akun kontrol |
| **S2** | Pembuatan Draft Baru lewat Kejadian `EVT-UJI-034A` | `POST /v1/corporate/accounting/accounting-events`<br>`EventTypeCode: "PATIENT_PAYMENT"` | HTTP `201 Created`, eventStatus: `"Terjurnal"`. Menghasilkan draft jurnal baru `JU/2031/01/00003` (`231bfe04-cd04-47f2-a149-f62038d40482`) dengan baris Kas Kasir & Piutang Pasien Umum. | **PASS** | Draft jurnal kejadian terbuat |
| **S3** | Pengajuan Draft Jurnal Hasil Kejadian | `POST /v1/corporate/accounting/journals/{id}/submit` | HTTP `200 OK`, pesan: *"Jurnal berhasil diajukan."* Status jurnal beralih ke `PendingApproval` (`2`). | **PASS** | **Acceptance (1)** |
| **S4** | Regresi: Jurnal Manual ke `1-1002 Kas Kasir` | `POST /v1/corporate/accounting/journals` (Manual Entry) | HTTP `422 Unprocessable Entity`, pesan: *"Akun 1-1002 Kas Kasir hanya dapat dicatat lewat kejadian akuntansi, bukan jurnal manual."* | **PASS** | **Acceptance (4)** |
| **S5** | Opsional: Setujui Jurnal (Four-Eyes Check) | `POST /v1/corporate/accounting/journals/{id}/approve` | HTTP `403 Forbidden`, pesan: *"Anda tidak dapat menyetujui jurnal yang Anda buat sendiri."* Dilewati karena single user; aturan empat mata terbukti aktif. | **SKIPPED** | **Acceptance (2)** |

---

## 3. Bukti Data Uji (Artifacts)

- **Script Pengujian:** `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\test-p2-034-s1-s5.mjs`
- **Output JSON Log:** `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\be_p2_034_s1_s5_report.json`
- **Dokumen Laporan Task:** [`C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend\docs\module-blueprints\accounting\task\report\backend\BE-ACC-P2-034.md`](../task/report/backend/BE-ACC-P2-034.md)

---

## 4. Kesimpulan

Perubahan backend pada `AccJournalService.cs` (`BerasalDariJalurOtomatisAsync`) telah terverifikasi secara tuntas di live environment:
- Memperbaiki bug di mana draft jurnal hasil aturan posting kejadian akuntansi ditolak saat diajukan.
- Menjaga kepatuhan terhadap matriks validasi dan keputusan arsitektur `ACC-DEC-064`.
- Tidak ada regresi pada perlindungan akun kontrol terhadap jurnal manual.
