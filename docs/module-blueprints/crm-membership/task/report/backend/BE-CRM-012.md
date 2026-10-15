# Laporan Perubahan Backend — `BE-CRM-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-012` |
| Judul | Perpanjangan dan upgrade |
| Slice | `MVP-1`, `EPIC CRM-02` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-012` |
| Trace | `FR-CRM-012`, `FR-CRM-013`; `CRM-DEC-012`, `CRM-DEC-024`, `CRM-DEC-029`; `VAL-05`, `VAL-07` s.d. `VAL-09`, `VAL-17`; state `v1` |
| Contract version | API `v2`, state `v1`, validasi `v1`, semua `approved` |
| Dependency | `BE-CRM-011` (perintah daftar dan kerangka service) |
| Klasifikasi | `MEDIUM`: dua perintah service, dua endpoint, dua DTO, 18 uji + harness uji bersama |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `83ef27bd` (merge `origin/QuilvianIntegrationBackend` ke `yoga`, di atas commit owner `34abecbe`) |
| Tanggal | 2026-10-13 |
| Status | Selesai. Penyelesaian setelah lunas (Terjadwal → Aktif, lama → Upgraded) milik `EPIC CRM-08` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (`Pat`, `ACTIVE`) |
| Keberlakuan | `NEW CODE` (perintah service, DTO, aksi baru). `TOUCHED LEGACY`: `PatientMembershipController` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-CODE-001`…`005`, `QBE-PERM-001`, `QBE-DTO-001` |
| Impact scan merge `83ef27bd` | Snapshot masih memuat model membership; `Program.cs` masih mendaftarkan resolver dan service; 3 migration baru milik modul lain (HR, piutang, penjamin perusahaan), tidak menyentuh membership |

## 1. Masalah yang diperbaiki

Belum ada jalan resmi untuk memperpanjang atau menaikkan tingkat. Satu-satunya cara adalah
mengubah data membership lama, padahal itu kini dilarang (`CRM-DEC-022`), atau mendaftar ulang,
yang ditolak selama masih Aktif.

## 2. Proses bisnis

**Perpanjangan** (`POST /admin/{id}/renewals`):

1. Petugas CRM membuka membership Aktif lalu menekan Perpanjang.
2. Sistem mengunci pasien, lalu memeriksa: membership masih Aktif, `RowVersion` sama dengan yang dibuka petugas, hari ini (WIB) sudah masuk jendela 30 hari, belum ada perpanjangan Terjadwal, dan belum ada pendaftaran Menunggu Pembayaran.
3. Sistem membuat membership baru pada **tingkat yang sama**, berstatus Menunggu Pembayaran, jenis `Renewal`, menunjuk membership lama, dengan iuran tingkat saat ini.
4. Membership lama tidak berubah sama sekali.

Contoh: Gold berlaku sampai 12 Okt 2027. Pada 2 Sep 2027 perpanjangan ditolak ("Perpanjangan baru
bisa mulai 12 Sep 2027."). Pada 1 Okt 2027 diterima.

**Upgrade** (`POST /admin/{id}/upgrades`):

1. Petugas memilih tingkat tujuan.
2. Tingkat tujuan wajib berbayar, beriuran, dan `PriorityLevel`-nya lebih tinggi (Silver 1 < Gold 2 < Platinum 3 < Executive 4, `CRM-DEC-029`).
3. Membership baru jenis `Upgrade` tersimpan Menunggu Pembayaran. Yang lama tetap Aktif sampai iuran upgrade lunas.
4. Upgrade tetap diizinkan walaupun ada perpanjangan Terjadwal. Terjadwal itu dibatalkan dengan refund saat upgrade lunas (`CRM-DEC-025`, `EPIC CRM-08`).

**Jalur tidak normal:**

| Keadaan | Kode | Pesan |
| --- | --- | --- |
| Membership tidak ada | 404 | "Membership tidak ditemukan." |
| Dibuka sebelum diubah petugas lain | 409 | "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi." |
| Membership bukan Aktif | 409 | "Perpanjangan hanya untuk membership yang sedang aktif." (atau "Upgrade …") |
| Belum masuk jendela | 409 | "Perpanjangan baru bisa mulai 12 Sep 2027." |
| Sudah ada Terjadwal | 409 | "Perpanjangan untuk periode berikutnya sudah dibayar." |
| Masih ada yang Menunggu | 409 | "Pasien ini sudah punya pendaftaran yang menunggu pembayaran (MBR-…)." |
| Upgrade ke tingkat sama/lebih rendah | 409 | "Upgrade hanya ke tingkat yang lebih tinggi dari Gold." |
| Tingkat tujuan tanpa iuran | 409 | "Nominal iuran tingkat Executive belum diisi. Hubungi admin master data." |

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipService.cs` | `RenewAsync`, `UpgradeAsync`. Pembuatan pendaftaran Menunggu Pembayaran dipecah menjadi bagian bersama (`CreatePendingAsync`, `LoadRegistrableTierAsync`, `CheckNoPendingAsync`, `LoadActiveForChangeAsync`) yang juga dipakai `RegisterAsync`. Konfigurasi `PatientMembership:RenewalWindowDays` (bawaan 30) |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientMembershipDtos.cs` | `RenewPatientMembershipRequest`, `UpgradePatientMembershipRequest` |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | `POST /admin/{id}/renewals`, `POST /admin/{id}/upgrades`, helper `ToCommandResponseAsync` (juga dipakai `POST /admin`) |
| `Tests/.../Infrastructure/MembershipHarness.cs` | Baru: perlengkapan uji membership bersama |
| `Tests/.../MembershipRenewUpgradeTests.cs` | Baru, 18 uji |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru sesuai API `v2` |
| Database | Menulis `MstPatientMembership`, `PatMembershipStatusHistory`, `NumNumberSeries`. Tanpa migration |
| Keamanan/Auth | `PatientMembership : Create`. Log hanya id, jenis, dan status |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin/{id}/renewals` | Mendaftarkan perpanjangan membership Aktif | `PatientMembership : Create` |
| `POST` | `/admin/{id}/upgrades` | Mendaftarkan upgrade ke tingkat lebih tinggi | `PatientMembership : Create` |

Kode status: 200 (detail membership baru), 400, 403, 404, 409 (lihat tabel jalur tidak normal).

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Perpanjangan 11 hari sebelum habis | Menunggu, `Renewal`, tingkat sama, menunjuk yang lama; yang lama utuh termasuk `RowVersion` | `PASS` | Uji SQLite |
| Perpanjangan 40 hari sebelum habis (`AT-30`) | 409 "… mulai 12 Sep 2027." | `PASS` | idem |
| Tepat 12 Sep 2027 00.01 WIB | Diterima | `PASS` | idem |
| Jendela dari konfigurasi (7 hari) | "… mulai 5 Okt 2027." | `PASS` | idem |
| Sudah ada Terjadwal / perpanjangan kedua saat masih Menunggu | 409 | `PASS` | idem |
| `RowVersion` usang | 409 | `PASS` | idem |
| Sumber Expired/Menunggu/Perlu Verifikasi; membership tidak ada | 409 / 404 | `PASS` | idem |
| Daftar baru tingkat sama setelah kedaluwarsa (`AT-29`) | Diterima | `PASS` | idem |
| Upgrade Gold → Platinum | Menunggu, `Upgrade`, iuran Platinum; Gold tetap Aktif | `PASS` | idem |
| Upgrade ke Silver/Gold (`AT-14`) | 409 | `PASS` | idem |
| Upgrade saat ada Terjadwal | Diizinkan | `PASS` | idem |
| Upgrade ke tingkat tanpa iuran | 409 | `PASS` | idem |
| Controller renewals/upgrades | 200 berisi detail; upgrade sesudah perpanjangan menunggu → 409 | `PASS` | idem |
| `dotnet test` seluruh project | 124 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-14` | Terpenuhi | Uji upgrade ke tingkat lebih rendah |
| `AT-29` | Terpenuhi | Uji daftar baru setelah kedaluwarsa |
| `AT-30` | Terpenuhi | Uji 40 hari sebelum habis |
| Perpanjangan 11 hari sebelum habis tercatat Menunggu (`Renewal`) | Terpenuhi | Uji perpanjangan 11 hari |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kasus tepi untuk `EPIC CRM-08`: perpanjangan yang didaftarkan menjelang habis bisa lunas **sesudah** periode lama berakhir (batas bayar 7 hari). Saat itu tanggal mulai "sehari setelah periode lama" sudah lewat. Cara menanganinya (langsung Aktif atau tetap menyambung mundur) perlu diputuskan saat `EPIC CRM-08` dirancang |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `POST /` dan `/kiosk` lama masih hidup sampai `BE-CRM-015` |
| Perubahan sampingan | `NONE` |
| Interupsi | Sesi dimulai ulang, lalu ditemukan commit owner `34abecbe` + merge `83ef27bd`. Impact scan dilakukan (lihat Preflight) sebelum melanjutkan |
| Status Git | Perubahan belum di-commit. Berkas uji `MembershipRenewUpgradeTests.cs`, `Infrastructure/MembershipHarness.cs`, dan tiga berkas uji membership sebelumnya diabaikan git (`/Tests/*`), sehingga perlu `git add -f` |
| Langkah berikutnya | `BE-CRM-013` (batal, verifikasi data lama, catatan, riwayat) |
