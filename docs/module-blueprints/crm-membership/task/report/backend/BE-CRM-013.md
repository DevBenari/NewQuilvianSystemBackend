# Laporan Perubahan Backend — `BE-CRM-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-013` |
| Judul | Batal, verifikasi, catatan, riwayat |
| Slice | `MVP-1`, `EPIC CRM-04` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-013` |
| Trace | `FR-CRM-030` s.d. `FR-CRM-035`; `CRM-DEC-013`, `021`, `022`, `025`, `029`; `VAL-10` s.d. `VAL-17`; state `v1`; izin `v1` (`Cancel`, `Verify`, `Update`, `Read`) |
| Contract version | API `v2`, state/validasi/izin `v1`, semua `approved` |
| Dependency | `BE-CRM-011` 🟡 (kerangka service) |
| Klasifikasi | `MEDIUM`: empat perintah/kueri service, empat endpoint, tiga DTO + satu respons, satu enum, 24 uji |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/PatientManagement/`, `Tests/...PatientManagementTests/`, laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `83ef27bd` + perubahan `BE-CRM-012` yang belum di-commit |
| Tanggal | 2026-10-13 |
| Status | Selesai. Refund dan pembatalan tagihan baru **dicatat sebagai permintaan** di riwayat; pelaksanaannya menunggu `DEC-CRM-001` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (`Pat`, `ACTIVE`) |
| Keberlakuan | `NEW CODE` (perintah, DTO, enum, aksi baru). `TOUCHED LEGACY`: `PatientMembershipController` |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DEL-001` (batal = penandaan beserta aktor, riwayat tidak dihapus), `QBE-AUD-001` (jejak di `PatMembershipStatusHistory`), `QBE-PERM-001` (aksi khusus `Cancel`/`Verify` dengan `[AccessAction]`), `QBE-DTO-001` |

## 1. Masalah yang diperbaiki

Sebelumnya membership bisa dinonaktifkan tanpa alasan (`PATCH status`), dihapus (`DELETE`),
atau diubah seluruh isinya (`PUT`), dan tidak ada jejaknya. Data lama yang ditandai Perlu
Verifikasi (`BE-CRM-002`) juga belum bisa diputuskan.

## 2. Proses bisnis

**Batal** (`POST /admin/{id}/cancel`, izin `PatientMembership : Cancel`):

| Dari | Kategori | Hasil |
| --- | --- | --- |
| Menunggu Pembayaran | Pasien / salah input | Dibatalkan. Riwayat `FEE_CHARGE_CANCEL_REQUESTED` ("Menunggu modul kasir") |
| Aktif | Permintaan pasien | Dibatalkan tanpa refund. Perpanjangan Terjadwal milik pasien ikut dibatalkan dengan riwayat `REFUND_REQUESTED`. Status member pasien dikosongkan |
| Aktif | Salah input | **Ditolak 409 (fail-closed)**: pemakaian diskon dan refund belum bisa diperiksa tanpa modul kasir |
| Terjadwal | Pasien / salah input | Dibatalkan. Riwayat `REFUND_REQUESTED` |
| Perlu Verifikasi | — | 409, diputuskan lewat verifikasi |
| Kedaluwarsa / Dibatalkan / Di-upgrade | — | 409, status akhir |

Alasan wajib 10–500 karakter. Kolom `IsCancel`, `CancelDateTime`, dan `CancelBy` terisi, serta
`RowVersion` diganti.

Contoh: member Gold Aktif sampai 12 Okt 2027 yang sudah membayar perpanjangan (Terjadwal 13 Okt
2027 s.d. 12 Okt 2028) minta berhenti pada 1 Des 2026. Kedua membership menjadi Dibatalkan, yang
Terjadwal mendapat penanda "refund diminta", dan pasien terbaca Reguler.

**Verifikasi data lama** (`POST /admin/{id}/verify`, izin `PatientMembership : Verify`):

1. **Diakui:** petugas mengisi periode (mulai ≤ akhir, maksimal satu masa berlaku tingkat). Bila tanggal akhir sudah lewat, status Kedaluwarsa. Selain itu Aktif, asalkan pasien tidak punya Aktif lain, dan status member pasien terisi.
2. **Ditolak:** alasan wajib, lalu status Dibatalkan.

**Catatan** (`PATCH /admin/{id}/notes`, izin `Update`): satu-satunya ubah yang diizinkan, ditolak
pada status akhir. Riwayat `NOTES_UPDATED` menyimpan nilai lama dan baru.

**Riwayat** (`GET /admin/{id}/status-histories`, izin `Read`): terbaru di atas, dengan nama pelaku.

## 3. Perubahan yang dikerjakan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/Membership/Services/PatientMembershipService.cs` | `CancelAsync`, `VerifyLegacyAsync`, `UpdateNotesAsync`, `GetStatusHistoriesAsync`, bantuan `LoadForChangeAsync`/`MarkCancelled`/`Touch`/`AddHistory`/`TryNormalizeReason` |
| `Areas/HealthServices/PatientManagement/Membership/Enums/MembershipVerificationDecision.cs` | Baru: `Accept = 1`, `Reject = 2` |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientMembershipDtos.cs` | `CancelPatientMembershipRequest`, `VerifyLegacyPatientMembershipRequest`, `UpdatePatientMembershipNotesRequest`, `PatientMembershipStatusHistoryResponse` |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientMembershipController.cs` | Empat endpoint baru. Aksi khusus `Cancel` (SortOrder 5) dan `Verify` (SortOrder 6) dideklarasikan dengan `[AccessAction]` |
| `Tests/.../MembershipCancelVerifyNotesTests.cs` | Baru, 23 uji |
| `Tests/.../MembershipPermissionRegistryTests.cs` | Baru, 1 uji (validator registri izin) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru sesuai API `v2`. Endpoint lama `PUT`/`PATCH status`/`DELETE` belum dipensiunkan (itu `BE-CRM-015`) |
| Database | Menulis `MstPatientMembership`, `PatMembershipStatusHistory`, dan proyeksi `MstPatient`. Tanpa migration |
| Keamanan/Auth | Dua aksi izin baru `PatientMembership : Cancel` dan `PatientMembership : Verify` harus diberikan ke departemen-posisi CRM lewat Role Access. Alasan dan catatan (sensitif) tidak masuk log |

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient Membership

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/admin/{id}/cancel` | Membatalkan membership dengan alasan | `PatientMembership : Cancel` |
| `POST` | `/admin/{id}/verify` | Mengakui atau menolak membership lama | `PatientMembership : Verify` |
| `PATCH` | `/admin/{id}/notes` | Mengubah catatan | `PatientMembership : Update` |
| `GET` | `/admin/{id}/status-histories` | Riwayat status | `PatientMembership : Read` |

Kode status: 200, 400 (alasan/periode/kategori tidak sah), 403, 404, 409 (status tidak sah,
`RowVersion` usang, bertabrakan dengan Aktif lain, salah input fail-closed).

## 5. Verifikasi

| Skenario | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Alasan kosong/pendek (`AT-15`), kategori sistem | 400 | `PASS` | Uji SQLite |
| Aktif + permintaan pasien (`AT-16`) | Dibatalkan, kolom batal terisi, satu riwayat `CANCELLED`, tanpa refund, proyeksi kosong | `PASS` | idem |
| Aktif yang punya Terjadwal | Terjadwal ikut batal + `REFUND_REQUESTED` | `PASS` | idem |
| Aktif + salah input | 409 fail-closed, tidak berubah, tanpa riwayat | `PASS` | idem |
| Menunggu / Terjadwal | Dibatalkan + permintaan billing tercatat | `PASS` | idem |
| Perlu Verifikasi / Kedaluwarsa / Dibatalkan | 409 dengan pesan masing-masing | `PASS` | idem |
| Dua petugas, `RowVersion` sama (`AT-35`) | Pertama 200, kedua 409 | `PASS` | idem |
| Akui periode berjalan | Aktif, `ActivatedAt`, proyeksi terisi, `LEGACY_ACCEPTED` | `PASS` | idem |
| Akui periode lewat | Kedaluwarsa, proyeksi kosong | `PASS` | idem |
| Akui saat ada Aktif lain (`AT-28`) | 409 | `PASS` | idem |
| Periode terbalik / lebih dari 12 bulan | 400 | `PASS` | idem |
| Tolak tanpa alasan / dengan alasan | 400 / Dibatalkan + `LEGACY_REJECTED` | `PASS` | idem |
| Verifikasi bukan Perlu Verifikasi | 409 | `PASS` | idem |
| Catatan pada Aktif / status akhir | Berubah + riwayat lama-baru / 409 | `PASS` | idem |
| Riwayat lewat controller | 2 baris dengan pelaku; 404 bila tidak ada | `PASS` | idem |
| Controller cancel | 200, detail berstatus Dibatalkan dengan alasan | `PASS` | idem |
| `PermissionRegistryValidator` atas 7 aksi baru | Tanpa temuan | `PASS` | `MembershipPermissionRegistryTests` |
| `dotnet test` seluruh project | 148 lulus | `PASS` | Keluaran test |
| QBE checker | 0 VIOLATION | `PASS` | Keluaran checker |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-15` | Terpenuhi | Uji alasan kosong/pendek |
| `AT-16` | Terpenuhi | Uji Aktif + permintaan pasien |
| `AT-28` | Terpenuhi | Uji akui saat ada Aktif lain |
| `AT-35` | Terpenuhi | Uji dua petugas |
| Salah input pada membership tanpa data pemakaian manfaat ditolak fail-closed | Terpenuhi | Uji Aktif + salah input |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Temuan lama validator izin:** endpoint lama ber-policy `KioskRead` (`GetFilterMetadata`, `GetPatientMemberships`, `GetPatientMembershipOptions`, `CreatePatientMembership` di rute `/` dan `/kiosk`) muncul sebagai temuan `PermissionRegistryValidator`. Kondisi ini sudah ada sebelum crm-membership. `CreatePatientMembership` akan hilang di `BE-CRM-015` |
| Masalah yang diketahui | Membatalkan Aktif karena **salah input** belum bisa dilakukan sama sekali sampai billing menyediakan cek pemakaian manfaat (`AG-02`). Jalan sementaranya kategori "permintaan pasien" (tanpa refund) bila memang tidak ada uang yang dikembalikan |
| Risiko tersisa | Permintaan refund dan pembatalan tagihan baru berupa riwayat (`REFUND_REQUESTED`, `FEE_CHARGE_CANCEL_REQUESTED`). Kasir belum menerimanya. Rekonsiliasi `EPIC CRM-08` harus membaca riwayat ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Belum di-commit. Berkas uji baru `MembershipCancelVerifyNotesTests.cs` dan `MembershipPermissionRegistryTests.cs` diabaikan git (`/Tests/*`), perlu `git add -f` |
| Langkah berikutnya | `BE-CRM-014` (lookup pasien + status aktif), lalu `BE-CRM-015` (penutupan rute kiosk dan pensiun endpoint lama, rilis bersama `FE-CRM-001`) |
