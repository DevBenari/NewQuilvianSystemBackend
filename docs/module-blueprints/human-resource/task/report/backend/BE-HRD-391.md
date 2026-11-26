# Laporan Perubahan Backend — `BE-HRD-391`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-391` |
| Judul | Rekening bank tidak dapat diubah diam-diam |
| Slice | `S-FIN-M04` / Profil (alias `FIN-HRD-04-BE-005`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-391` |
| Trace | `HRD-DEC-052`; instruksi Menu 04 `FIN-HRD-04-BE-005` |
| Contract version | Kontrak rekening bank Workforce Core existing; tidak ada endpoint baru |
| Dependency | `BE-HRD-390` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `WorkforceCore/Controllers/WfpBankAccountController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001, QBE-PERM-001, QBE-LOG-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Rekening bank menentukan ke mana gaji ditransfer. Guard `409` untuk nomor/bank/pemilik dan larangan verifikasi oleh pembuat di `PATCH /verify` sudah ada, tetapi masih ada empat jalan pintas:

1. **`POST` langsung terverifikasi.** Pembuat boleh mengirim `IsVerified = true` saat membuat rekening — verifikasi oleh diri sendiri.
2. **`PUT` memverifikasi.** `PUT` menyalin `IsVerified` dari request, melewati larangan di `/verify`.
3. **Ganti bank tidak tertangkap.** Perubahan bank hanya dianggap terjadi bila Id bank lama dan baru sama-sama terisi; mengosongkan Id bank atau mengganti nama bank lolos.
4. **Rekening baru mengambil alih rekening gaji.** Contoh: perawat B punya rekening gaji Bank X terverifikasi. Seseorang menambah rekening Bank Y lalu menandainya rekening utama/gaji. Gaji bulan berikutnya dapat berpindah ke Bank Y tanpa petugas lain memeriksanya.

## 2. Proses bisnis

1. HR menambah rekening (`POST`): rekening selalu tersimpan **belum terverifikasi**. Mencentang "terverifikasi" saat membuat → `403 SELF_VERIFICATION_PROHIBITED`.
2. Rekening belum terverifikasi tidak boleh menjadi rekening utama/gaji bila sudah ada rekening utama/gaji lain yang terverifikasi → `409 UNVERIFIED_BANK_ACCOUNT_TAKEOVER`. Rekening pertama (belum ada rekening terverifikasi) tetap boleh.
3. Petugas lain memverifikasi lewat `PATCH /{id}/verify` (pembuat ditolak `403`).
4. Sesudah terverifikasi, rekening boleh dijadikan utama lewat `PATCH /{id}/primary` atau `PUT`.
5. `PUT` tidak dapat mengubah nomor, bank, pemilik (`409 SENSITIVE_BANK_ACCOUNT_LOCKED`) maupun status verifikasi (`409 BANK_VERIFICATION_VIA_VERIFY_ACTION`). Penggantian rekening yang sudah ada lewat Menu 06.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WfpBankAccountController.cs` (semua aksi), pembaca rekening di Payroll (`PayrollHrdImportService` mensyaratkan `IsVerified`; `PayrollCalculationService` membaca rekening aktif), `WorkforceProfileSyncService` (jalur apply tiket layanan pasca-persetujuan), frontend `workforce-profile-dashboard.jsx`/konfigurasi editor (centang "Terverifikasi").

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkforceCore/Controllers/WfpBankAccountController.cs` | `POST` menolak `IsVerified = true` dan selalu menyimpan belum terverifikasi; `PUT` menolak perubahan status verifikasi; deteksi ganti bank lewat Id maupun nama; `GuardUnverifiedTakeoverAsync` pada `POST`, `PUT`, `PATCH /primary`; cek profil tidak lagi mensyaratkan profil aktif (`BE-HRD-401`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap. `IsVerified = true` pada `POST` kini `403`; perubahan `IsVerified` lewat `PUT` kini `409`; kode baru `UNVERIFIED_BANK_ACCOUNT_TAKEOVER`. Frontend yang mencentang "Terverifikasi" saat membuat akan menerima pesan jelas |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup verifikasi oleh diri sendiri dan pengalihan rekening gaji tanpa pemeriksa |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Bank Account

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/bank-accounts` | Tambah rekening (belum terverifikasi) | `WorkforceBankAccount : Create` |
| `PUT` | `…/bank-accounts/{id}` | Ubah data non-sensitif | `WorkforceBankAccount : Update` |
| `PATCH` | `…/bank-accounts/{id}/primary` | Tetapkan rekening utama (wajib terverifikasi bila mengambil alih) | `WorkforceBankAccount : Update` |
| `PATCH` | `…/bank-accounts/{id}/verify` | Verifikasi oleh petugas selain pembuat | `WorkforceBankAccount : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: `POST` dengan `IsVerified = true` | `403`, tidak tersimpan | `PASS` (penelusuran) | `CreateBankAccount` |
| Penelusuran: `PUT` nomor rekening berubah | `409 SENSITIVE_BANK_ACCOUNT_LOCKED` | `PASS` (penelusuran) | guard lama |
| Penelusuran: `PUT` `IsVerified` false → true oleh pembuat | `409 BANK_VERIFICATION_VIA_VERIFY_ACTION` (dulu lolos) | `PASS` (penelusuran) | guard baru |
| Penelusuran: rekening Bank Y baru jadi gaji, Bank X terverifikasi | `409 UNVERIFIED_BANK_ACCOUNT_TAKEOVER` | `PASS` (penelusuran) | `GuardUnverifiedTakeoverAsync` |
| Penelusuran: `/verify` oleh pembuat | `403 SELF_VERIFICATION_PROHIBITED` | `PASS` (penelusuran) | guard lama |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) PUT rekening sensitif ditolak `409` | Terpenuhi | Guard nomor/bank (Id dan nama)/pemilik + guard status verifikasi |
| (2) Verifikasi oleh pembuat ditolak | Terpenuhi | `/verify` menolak pembuat; `POST`/`PUT` tidak lagi dapat memverifikasi |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Temuan di luar scope | `PayrollCalculationService` membaca rekening aktif tanpa mensyaratkan `IsVerified` (berbeda dari `PayrollHrdImportService`); `WorkforceProfileSyncService` (tiket layanan `S-D4`) membuat rekening langsung terverifikasi atas nama penyetuju tiket — jalur pasca-persetujuan, dicatat untuk ditinjau pemilik Payroll |
| Risiko tersisa | Frontend yang mengirim `isVerified: true` saat membuat akan menerima `403` sampai form disesuaikan (task frontend Menu 04) |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | Sesuaikan form frontend: sembunyikan centang "Terverifikasi" saat membuat |
