# Kiosk — Matriks Hak Akses dan Audit

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v1` |
| `last_changed_in` | `v1` |
| Status | `approved` |
| Owner | Sukma Giri Pratama; security/privacy owner **belum ditunjuk** |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-09-30 |
| `input_revision` | `02-backend-architecture.md` r1 |
| Traceability | SEC-KSK-001..007, PRIV-1..5, `KSK-DEC-011/016/017`, `KSK-DSN-005/006`, `KSK-INV-006/009` |

Pemetaan endpoint → hak akses hanya ada di kolom **Hak akses** pada `contracts/api-contract.md`. Dokumen ini tidak mengulangnya.

## 1. Cara kerja hak akses di repository ini

| Lapisan | Dipakai Kiosk? | Rujukan source |
| --- | --- | --- |
| Policy perangkat `KioskRead` | **Ya** — satu-satunya penjaga endpoint Kiosk | `Program.cs#AddPolicy(AuthorizationPolicies.KioskRead)`; `Constants/AuthorizationPolicies.cs` |
| Matriks Akses Role (`[AccessPermission]`, `HasAccessAsync`) | Tidak — akun perangkat tidak punya Departemen × Posisi | `Constants/AuthorizationPolicies.cs` (komentar kelas) |
| Registry permission & verifier | Ya, sebagai gerbang build | `Services/Security/PermissionRegistryDescriptor.cs`; `tools/authorization-verifier/` |
| Rate limiter `KioskPatientLookup` | Ya — hanya endpoint lookup | `Program.cs` (baru, `KSK-DSN-005`) |

**Aturan untuk endpoint Kiosk baru** (`KSK-DSN-006`): pasang `[Authorize(Policy = AuthorizationPolicies.KioskRead)]` **tanpa** `[AccessAction]` dan tanpa `[AccessPermission]`. Memasang `[AccessAction]` saja menambah baris ke `approved-compatibility-fallback.txt`, sehingga invarian 2 verifier gagal dan hal itu butuh keputusan governance.

## 2. Peta peran ke hak akses

| Peran | Cek No. RM | Pendaftaran Pasien Lama | Jadwal Dokter | Keterangan |
| --- | :---: | :---: | :---: | --- |
| Akun perangkat Kiosk (role `Kiosk` / klaim `is_kiosk_account`) | Ya | Ya | Ya | Lewat `KioskRead` |
| `SuperAdmin`, `Administrator` | Ya | Ya | Ya | `KioskRead` juga menerima kedua role ini (existing) — untuk uji dan dukungan |
| Petugas pendaftaran | — | — | — | Memakai layar petugas, bukan Kiosk; tidak berubah |

## 3. Kewenangan yang tidak dijaga mesin hak akses

| Penjaga aturan bisnis | Letak | Apa yang **tidak** dijaganya | Risiko sisa |
| --- | --- | --- | --- |
| Rate limit 10/menit/perangkat | Middleware | Penyerang yang punya **banyak** akun perangkat; pencarian lambat di bawah batas | Enumerasi pelan tetap mungkin. Mitigasi: respons tidak memuat KTP/HP; cocok ganda/non-aktif tanpa data (`KSK-RISK-002`) |
| Aturan tepat-satu pasien | `KioskPatientLookupService` | — | — |
| Pesan netral untuk pasien non-aktif | `KioskPatientLookupService` + layar | Pengguna dapat menyimpulkan "ada data tapi bermasalah" dari pesan "hubungi petugas" | Diterima: jauh lebih kecil daripada membocorkan status meninggal (`KSK-DEC-017`) |
| Tidak mengubah default penjamin pasien | Layar Kiosk (berhenti memanggil `PATCH …/primary`) | Endpoint `PATCH …/kiosk/{id}/primary` masih bisa dipanggil akun Kiosk di luar layar resmi | `KSK-RISK-003`; penutupan endpoint = keputusan terpisah (`KSK-GAP-011`) |
| Urutan step | Layar Kiosk | Backend tidak tahu urutan step; pemanggil API langsung dapat membuat kunjungan tanpa "Review" | Sama dengan hari ini; tidak ada data klinis yang rusak karena kunjungan tetap tervalidasi backend |

## 4. Audit dan log

| Kejadian | Lapisan | Isi yang dicatat | Yang **tidak boleh** dicatat |
| --- | --- | --- | --- |
| Setiap lookup | Log terstruktur Serilog (`ILogger<KioskPatientLookupService>`), level Information | `TraceId`, user id perangkat, `searchType`, `result`, 4 digit terakhir nilai (`****7890`), durasi | Nilai KTP/HP/kartu utuh, nama pasien, No. RM hasil |
| Lookup ditolak rate limit | Log request existing (`UseSerilogRequestLogging`) level Warning, ditambah log `OnRejected` | Path, status `429`, user id perangkat | Body request |
| Request log umum | `UseSerilogRequestLogging` existing | Path saja (tanpa query string/body) — `KSK-CAP-010` | — |
| Pembentukan sesi kiosk, create encounter | `LoggerService` existing | Tidak berubah | — |

**Pengecualian terhadap konvensi logger project** (konvensi: selain `GET` dicatat lewat `LoggerService`):

| Endpoint | Pengecualian | Sebab |
| --- | --- | --- |
| `POST kiosk-patient-lookups` | **Tidak** dicatat ke `LoggerService`; cukup log terstruktur tersamar | Endpoint ini hanya membaca (POST dipakai demi privasi URL). Mencatatnya ke audit tahan-lama akan menyimpan jejak pencarian identitas yang tidak diminta PRD (`KSK-GAP-001`) |

## 5. Kolom sensitif dan masa simpan

| Kolom / nilai | Sumber | Perlakuan |
| --- | --- | --- |
| `KioskPatientLookupRequest.value` | Request | Sensitif. Tidak masuk log kecuali 4 digit terakhir; tidak disimpan |
| `MstPatient.IdentityNumber`, `PhoneNumber` | Kamus data | Sensitif. Tidak pernah ada di respons lookup |
| `KioskPatientCardResponse.*` | Respons | Data Kartu Pasien existing; tidak disimpan di browser storage atau URL |
| Hasil pindai kartu (`rawScanText`) di layar | Memori layar | Dibuang saat `SESSION_CLEARED`; dikirim sekali ke `scan-result` di Step 3 |
| Log Serilog | Server | Mengikuti retensi log server existing; tidak ada retensi baru |

Frontend: seluruh `console.log` yang mencetak URL, diagnostik, atau body respons pada service Kiosk **dihapus** (`KSK-DEC-016`).


---

## Amendment PO — Hak Akses dan Audit Pendaftaran Pasien Online

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v2` |
| `last_changed_in` | `v2` (bagian ini saja) |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-19 (`KSK-DEC-054`) |
| `input_revision` | `00-interview-decisions.md` §13 (`KSK-DEC-050` s.d. `052`, `KSK-PO-AS-002`); impact scan `CAP-PO-02`, `28`, `30` |
| Traceability | AC 2, 18, 19, 20 |

Pemetaan endpoint ke hak akses hanya ada di kolom **Hak akses** pada `contracts/api-contract.md` *Amendment PO*. Bagian ini tidak mendaftar ulang endpoint.

### PO-P.1 Cara kerja hak akses yang dipakai

| Lapisan | Dipakai di | Rujukan source |
| --- | --- | --- |
| Tanpa login (`[AllowAnonymous]`, tanpa `[AccessController]`) | Grup *Online Registration Public* | Pola `PriorityReservationArrivalController.cs:28-36`. Didaftarkan pada verifier otorisasi dengan cara yang sama seperti controller itu |
| Policy perangkat `KioskRead`, tanpa `[AccessAction]` | Grup *Online Registration Check-In* dan `POST patient-encounters/kiosk` | `Program.cs:1908`; aturan `KSK-DSN-006` |
| Matriks Akses Role: `[AccessController]` + `[AccessAction]` + `[AccessPermission]` | Grup *Online Registration* | Seeder `AccessMenuSeeder` (`Program.cs:2833`) mendaftarkan Resource dari atribut (`PermissionRegistryDescriptor.cs:12-36`); tanpa migration |
| Batas laju | Grup publik (per IP) dan check-in (per akun kiosk) | `Program.cs:2016-2075`, empat policy baru |

String yang dipakai: `[AccessPermission("OnlineRegistration", "Read")]`, `[AccessPermission("OnlineRegistration", "Update")]`, `[AccessPermission("OnlineRegistration", "Cancel")]`. `AccessType`: `Read`, `Update`, `Update`.

### PO-P.2 Peta peran ke hak akses

| Peran | Halaman online | Check-in kiosk | Lihat | Ubah | Batal |
| --- | :---: | :---: | :---: | :---: | :---: |
| Pasien/wali (tanpa login) | Ya | — | — | — | — |
| Akun perangkat kiosk | — | Ya | — | — | — |
| Petugas admisi/pendaftaran RJ | — | — | Diatur admin | Diatur admin | Diatur admin |
| Kepala admisi | — | — | Diatur admin | Diatur admin | Diatur admin |
| `SuperAdmin`, `Administrator` | — | Ya (`KioskRead`) | Ya | Ya | Ya |

Usulan awal untuk dicentang admin di layar Akses Role: petugas admisi RJ mendapat `Read` dan `Update`; kepala admisi mendapat ketiganya. Ini `CONFIGURABLE_DEFAULT` dan dilakukan saat go-live, bukan oleh migration.

### PO-P.3 Kewenangan yang tidak dijaga mesin hak akses

| Penjaga aturan bisnis | Letak | Apa yang **tidak** dijaganya | Risiko sisa |
| --- | --- | --- | --- |
| Captcha + batas laju per IP | Halaman publik | Penyerang dengan banyak IP dan pemecah captcha | Pendaftaran palsu pasien baru menahan kuota. Mitigasi: satu per NIK per tanggal, petugas dapat membatalkan (`KSK-DEC-052`) |
| Pesan `404` seragam | Pencocokan publik dan check-in | Penyerang yang sudah tahu NIK + tanggal lahir seseorang dapat mengetahui bahwa ia pasien RS | Diterima: kedua data itu sudah merupakan rahasia pribadi; nama yang keluar tersamar |
| NIK terkunci pada hasil OCR | Layar check-in kiosk | Backend tidak menerima hasil OCR mentah; perangkat kiosk yang dimanipulasi dapat mengirim NIK lain | `GATE-PO-14`: perangkat kiosk adalah akun yang sudah login; risiko sama dengan kiosk Pasien Baru hari ini |
| Pemeriksaan isi kunjungan terhadap pendaftaran | `PrepareCheckInAsync` | — | — |
| Hanya poli yang sama saat ubah jadwal | `OnlineRegistrationService` | — | — |

### PO-P.4 Audit dan log

| Kejadian | Jejak tahan lama | Log aplikasi |
| --- | --- | --- |
| Pendaftaran tersimpan | Riwayat `SUBMITTED` (sumber halaman publik) | `OnlineRegistration.Submit` dengan `RegistrationNumber` |
| Check-in berhasil | Riwayat `CHECKED_IN` dengan `ActorUserId` akun kiosk dan `EncounterId`; kunjungan `RegistrationSource = 7` | `OnlineRegistration.CheckIn` dan log kunjungan yang sudah ada |
| Jalur lain, Tidak Datang | Riwayat `REGISTERED_ELSEWHERE` / `NO_SHOW` (sumber sistem) | Ringkasan jumlah per eksekusi penutupan |
| Ubah jadwal, ubah kontak, batal | Riwayat dengan petugas, waktu, nilai lama/baru, alasan | `OnlineRegistration.ChangeSchedule` / `ChangeContact` / `Cancel` |
| Verifikasi gagal, captcha gagal, `429` | Tidak ada | Jumlah dan waktu saja, tanpa kode, NIK, tanggal lahir, atau IP lengkap |

Log **tidak pernah** memuat kode, token, NIK, nama, tanggal lahir, kontak, isi consent, atau isi rujukan. `GET` tidak dicatat.

### PO-P.5 Kolom sensitif dan masa simpan

Kolom bertanda Sensitif ada di `data/data-dictionary.md` *Amendment PO*. Ringkasnya: `CodeHash`, `CheckInTokenHash`, data diri pasien baru, kontak, kartu baru, isi rujukan, seluruh kolom penanda tangan consent beserta gambar tanda tangan, nama berkas rujukan, dan kontak lama/baru di riwayat.

| Data | Keluar lewat | Bentuk |
| --- | --- | --- |
| Nama pasien lama | Halaman publik | Tersamar (`PatientNameMasker`) |
| Nomor kartu penjamin | Halaman publik | Tersamar, 4 digit terakhir |
| NIK, No. RM, alamat | Halaman publik | Tidak pernah |
| Seluruh data pendaftaran | Layar petugas | Utuh, hanya bagi pemegang `OnlineRegistration : Read` |

Masa simpan mengikuti catatan pendaftaran lain. Kebijakan penghapusan data pendaftaran yang tidak menjadi kunjungan menunggu aturan retensi RS (`GATE-PO-03`); pemilik keamanan/privasi di manifest masih `BELUM DITUNJUK`.
