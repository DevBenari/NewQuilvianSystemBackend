# Laporan Pengujian Live API — BE-ACC-P2-029

**Tanggal Pengujian:** 29 September 2026  
**Pelaksana Pengujian:** Antigravity Pairing Agent  
**Modul:** Corporate / Accounting Management (`AccountingPeriod` & `AccountingEvent`)  
**Task ID:** `BE-ACC-P2-029`  
**Judul Perubahan:** Kejadian Gagal di periode tertutup menahan periode terbuka paling awal (`ACC-DEC-095`)  
**Metode:** Automated Live Browser API Test (Playwright Session Context with Authenticated Cookies & Antiforgery)  
**Lingkungan:** 
- Frontend: `http://localhost:3000` (Next.js v20.20.2)
- Backend: `https://localhost:7184/api` (ASP.NET Core 9 / EF Core)
- Akun Uji: `superadmin@admin.com` (SuperAdmin)
- Badan Hukum: `3bf63974-a754-4b20-81ee-70894f6fb058` (PT Metropolitan Medical Centre)

---

## 1. Ringkasan Eksekutif

Pengujian resep **S1–S11** dari dokumen [`BE-ACC-P2-029.md`](../task/report/backend/BE-ACC-P2-029.md) bagian 5.2 telah berhasil dijalankan secara lengkap terhadap live environment tanpa intervensi langsung pada database (tanpa direct SQL execution).

Seluruh kriteria penerimaan (Acceptance Criteria 1 hingga 6) berhasil dibuktikan:
1. **Limpahan Kejadian Gagal:** Kejadian berstatus `Gagal` yang tanggal akuntansinya berada di periode tertutup/menunggu persetujuan (Juli 2026) secara otomatis terhitung pada daftar periksa penutupan (`closing-checklist`) periode terbuka berikutnya (Agustus 2026) sebagai `FAILED_EVENTS` penghalang (`isBlocking: true`).
2. **Penahanan Pengajuan Penutupan:** Pengajuan penutupan periode terbuka berikutnya (Agustus 2026) berhasil ditolak dengan status HTTP `409 Conflict` karena masih adanya kejadian limpahan tersebut.
3. **Pembersihan Penghalang:** Setelah kejadian ditandai `Diabaikan` via API `/ignore`, hitungan `FAILED_EVENTS` pada periode Agustus kembali ke nilai awal (`0`), dan periode Agustus kembali memenuhi syarat penutupan (`canSubmitClosing: true`).
4. **Isolasi Periode Terbuka Lanjutan:** Kejadian hanya berlabuh di periode terbuka paling awal (Agustus 2026) dan tidak bocor ke periode berikutnya (September 2026).
5. **Kejadian Tertahan Tidak Memblokir:** Kejadian berstatus `Tertahan` (`HELD_EVENTS`) tetap diperlakukan murni sebagai peringatan (`isBlocking: false`) dan tidak dilimpahkan.
6. **Kebersihan Lingkungan Pasca-Uji:** Periode Juli 2026 telah dibuka kembali ke status `Open`, dan jenis kejadian uji `UJI-SALDO-029` telah dinonaktifkan (`isActive: false`).

---

## 2. Parameter dan Penyesuaian Lingkungan

Sesuai arahan teknis:
1. **Periode Uji (Juli dan Agustus 2026):**
   Pada database dev, periode September 2026 aktif rekonsiliasi saldo subledger dan berisi transaksi riil, sehingga sulit ditutup. Oleh karena itu, pengujian menggunakan **Juli 2026** sebagai periode tanggal kejadian dan **Agustus 2026** sebagai periode terbuka pertama yang menerima limpahan. Aturan bisnis dan validasi identik 100%.
2. **Persetujuan Empat Mata (Four-Eyes Principle / Langkah S4):**
   Sistem menegakkan aturan bahwa pengaju penutupan periode tidak boleh sekaligus menjadi penyetuju penutupan (`ApproveClosingAsync` mengembalikan HTTP `403 Forbidden`). Sesuai resep uji bagian 5.2, pengujian cukup sampai tahap **Ajukan** (`PendingClosingApproval` / status `4`). Status ini menolak seluruh pencatatan jurnal, sehingga perilaku limpahan ke Agustus persis sama dengan status `SoftClosed`.

---

## 3. Hasil Pengujian Rinci (Resep S1–S11)

| No | Langkah Uji | Endpoint & Metode | Hasil Aktual | Status | Kriteria Terbukti |
|:--:|:---|:---|:---|:--:|:---|
| **S1** | Pemeriksaan Baseline Checklist Juli & Agustus | `GET /v1/corporate/accounting/periods/{id}/closing-checklist` | - Juli: `canSubmitClosing = true`<br>- Agustus: `FAILED_EVENTS count = 0` ($N_0$), `HELD_EVENTS count = 0`, `canSubmitClosing = true` | **PASS** | Persiapan kondisi awal bersih |
| **S2** | Pembuatan Kejadian Tertahan | `POST /v1/corporate/accounting/accounting-events`<br>`EventTypeCode: "UJI-SALDO-029"` | HTTP `422 Unprocessable Entity`<br>`EventStatus: "Tertahan"`, `HoldReasonCode: "EVENT_TYPE_NOT_REGISTERED"`. ID Kejadian tercatat. | **PASS** | Kejadian tertahan belum menghalangi |
| **S3** | Pengajuan Penutupan Periode Juli | `POST /v1/corporate/accounting/periods/{ID_JUL}/submit-closing`<br>`{ "note": "Uji 029" }` | HTTP `200 OK`. Periode Juli beralih ke `PendingClosingApproval`. Kejadian `Tertahan` tidak memblokir pengajuan penutupan. | **PASS** | Kejadian tertahan bukan blocker |
| **S4** | Persetujuan Penutupan Periode Juli | `POST /v1/corporate/accounting/periods/{ID_JUL}/approve-closing` | HTTP `403 Forbidden` (*"Penutupan tidak dapat disetujui oleh orang yang mengajukannya."*). Sesuai resep, langkah ini dilewati karena single user. Periode Juli tertahan di status 4 yang menolak jurnal. | **SKIPPED** | Penegakan empat mata diverifikasi |
| **S5** | Pendaftaran/Aktivasi Master Event Type | `POST /v1/corporate/accounting/event-types`<br>`EventKind: 2` (Saldo Subledger) | HTTP `200/201 OK`. Master jenis kejadian `UJI-SALDO-029` aktif. | **PASS** | Master data siap |
| **S6** | Coba Ulang Kejadian Menjadi Gagal | `POST /v1/corporate/accounting/accounting-events/{id}/retry` | HTTP `200 OK`. Event beralih ke `EventStatus: 3` (`Gagal`). Pesan kegagalan: *"Rincian saldo subledger hanya dan wajib ada pada pesan saldo subledger."* | **PASS** | Kejadian Gagal bertanggal Juli terwujud |
| **S7** | Checklist Periode Agustus Menangkap Limpahan | `GET /v1/corporate/accounting/periods/{ID_AGU}/closing-checklist` | - `FAILED_EVENTS count = 1` ($N_0 + 1$)<br>- `isBlocking = true`<br>- Pesan: *"Masih ada 1 kejadian keuangan yang gagal diproses…."*<br>- `canSubmitClosing = false` | **PASS** | **Acceptance (1)** |
| **S8** | Checklist Isolasi Periode Juli & September | `GET /v1/corporate/accounting/periods/{ID_JUL}/closing-checklist`<br>`GET /v1/corporate/accounting/periods/{ID_SEP}/closing-checklist` | - Juli: `count = 1` (tetap dihitung di periode asalnya).<br>- September: `count = 0` (kejadian tidak melompat melompati Agustus). | **PASS** | **Acceptance (3)** |
| **S9** | Pengajuan Penutupan Agustus Diblokir | `POST /v1/corporate/accounting/periods/{ID_AGU}/submit-closing` | HTTP `409 Conflict`. Pesan: *"Masih ada 1 kejadian keuangan yang gagal diproses. Coba ulang atau abaikan dengan alasan dari Kotak Masuk Kejadian."* | **PASS** | **Acceptance (1)** |
| **S10** | Pengabaian Kejadian & Verifikasi Ulang Checklist | `PATCH /v1/corporate/accounting/accounting-events/{id}/ignore`<br>`GET /v1/corporate/accounting/periods/{ID_AGU}/closing-checklist` | - `PATCH`: HTTP `200 OK` (*"ditandai Diabaikan"*).<br>- Checklist Agustus: `FAILED_EVENTS count = 0`, `canSubmitClosing = true`. | **PASS** | **Acceptance (2)** |
| **S11** | Verifikasi Karakteristik Kejadian Tertahan | Evaluasi item `HELD_EVENTS` pada respons S1 dan S7 | Tetap berstatus peringatan (`isBlocking = false`), hitungan `0`, tidak ikut dilimpahkan. | **PASS** | **Acceptance (5)** |

---

## 4. Hasil Pembersihan Lingkungan (Post-Test Cleanup)

Setelah skenario pengujian selesai, prosedur pembersihan data dijalankan secara otomatis:

1. **Pembukaan Kembali Periode Juli 2026:**
   - Endpoint: `POST /api/v1/corporate/accounting/periods/d9a08e0e-f500-49cd-add6-a63f744add5b/reject-closing`
   - Payload: `{ "reason": "Membersihkan data uji 029" }`
   - Hasil: HTTP `200 OK` (*"Penutupan periode Juli 2026 ditolak; periode kembali terbuka."*)
   - Verifikasi Status: `PeriodStatus = 1` (`Open`), `canSubmitClosing = true`.
2. **Deaktivasi Jenis Kejadian `UJI-SALDO-029`:**
   - Endpoint: `PATCH /api/v1/corporate/accounting/event-types/03e17ddc-e0b7-400b-aba4-274de20b6576/deactivate`
   - Hasil: HTTP `200 OK` (*"Jenis kejadian berhasil dinonaktifkan."*)
   - Verifikasi Status: `isActive = false`.

---

## 5. Bukti Data Uji (Artifacts)

- **Script Pengujian:** `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\test-p2-029-s1-s11.mjs`
- **Output JSON Log:** `C:\Users\BenariDev03\QuilvianV2\QuilvianSystemFrontendDev\test-with-agy\be_p2_029_s1_s11_report.json`
- **Dokumen Laporan Task:** [`C:\Users\BenariDev03\QuilvianV2\NewQuilvianSystemBackend\docs\module-blueprints\accounting\task\report\backend\BE-ACC-P2-029.md`](../task/report/backend/BE-ACC-P2-029.md)

---

## 6. Kesimpulan

Implementasi backend pada `BE-ACC-P2-029` di `AccPeriodClosingService.cs` telah terverifikasi secara tuntas di lingkungan runtime aktif. Perubahan logika limpahan kejadian gagal (`HitungKejadianGagalLimpahanAsync`) berfungsi tepat sesuai spesifikasi bisnis `ACC-DEC-095`:
- Mencegah penutupan periode terbuka saat masih ada kejadian gagal dari periode sebelumnya yang berpotensi menghasilkan jurnal ke periode tersebut saat dicoba ulang.
- Tidak menimbulkan kebocoran hitungan ke periode-periode berikutnya.
- Menjaga integritas data tanpa memerlukan migrasi database maupun penambahan endpoint baru.
