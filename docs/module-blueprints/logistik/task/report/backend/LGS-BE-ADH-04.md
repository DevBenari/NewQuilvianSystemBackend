# LGS-BE-ADH-04 — Finishing persetujuan permintaan barang (LGS-DEC-003 sampai 006)

- TASK ID: `LGS-BE-ADH-04` (task ad-hoc). Atas instruksi pemrakarsa "Master Prompt — Finishing Persetujuan Permintaan Barang Logistik", 17 November 2026
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend`, worktree `C:\tmp\phm`, branch `Ikbal`, HEAD `d946b20b`
- MODEL: Claude Opus 5.5
- KETERBERLAKUAN: NEW CODE di modul existing `PharmacyManagement` (registry `Phm`, `ACTIVE / LEGACY`); tidak ada entity, tabel, maupun migration baru
- QBE: QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-DTO-001, QBE-VAL-001

## Status per task

| Task | Status | Ringkasan |
| --- | --- | --- |
| BE-01 Decision log | COMPLETED_SOURCE | `LGS-DEC-003` sampai `006` dicatat; `LGS-OQ-020` sampai `023` ditutup; decision log r5 |
| BE-02 Hardening otorisasi | COMPLETED_SOURCE | Izin `Approve` kini juga diperiksa eksplisit di service; profil tenaga kerja dan masa berlaku unit organisasi diperiksa; percobaan yang ditolak dicatat ke log audit |
| BE-03 Verifikasi susulan CITO | PARTIAL | Endpoint, daftar, hasil, temuan, riwayat, dan pencegahan ganda selesai. Proses tindak lanjut temuan butuh entity baru (blocker persistence) |
| BE-04 Pengendalian batas aktivasi | COMPLETED_SOURCE | Nilai kosong, tidak sah, atau di masa depan tidak pernah membuka jalan pintas; setiap nilai baru tercatat di log; dokumen aktivasi tersedia |
| BE-05 Data organisasi uji | BLOCKED | Audit ulang (hanya baca) selesai dan prosedur disiapkan. Data manajerial 0 dan penulisan data butuh izin tersendiri |
| BE-06 Test keamanan | COMPLETED_SOURCE | 60 metode test dalam 6 kelas. **Belum dijalankan** |

## File

Baru:

- `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalCutover.cs`: penafsiran batas aktivasi sebagai fungsi murni
- `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/`, berisi:
  - `Perintah.cs`
  - `CutoverUnitTests.cs`
  - `AuthorizationTests.cs`
  - `StatusTransitionTests.cs`
  - `EmergencyReviewTests.cs`
  - `ConcurrencyTests.cs`
  - `ApiContractTests.cs`
- `docs/module-blueprints/logistik/evidence/05-aktivasi-batas-persetujuan.md`
- `docs/module-blueprints/logistik/evidence/06-dataset-uji-persetujuan.md`

Diubah:

- `Services/StockRequestService.cs`
- `Services/StockRequestApprovalAuthority.cs`
- `DTOs/StockRequestDtos.cs`
- `Enums/StockRequestEnums.cs`
- `Controllers/StockRequestController.cs`
- `Tests/.../Infrastructure/StockRequestApprovalHarness.cs`

Dihapus: `Tests/.../StockRequestApprovalTests.cs`. Ini berkas buatan sesi ini sendiri; isinya dipecah ke folder di atas.

## Kontrak API dan izin

Base: `api/v1/health-services/pharmacy-management/stock-requests`

| Endpoint | Izin controller | Pemeriksaan di service (tidak dapat dilewati SuperAdmin atau `Authorization:Enabled=false`) |
| --- | --- | --- |
| `POST /{id}/approve`, `POST /{id}/reject` | `StockRequest:Approve` | Bukan pemohon; izin `Approve` eksplisit pada penempatan jabatan aktif; atasan unit peminta |
| `POST /{id}/authorize-emergency` | `StockRequest:AuthorizeEmergency` | Bukan pemohon; izin `AuthorizeEmergency` eksplisit pada penempatan jabatan aktif |
| `POST /{id}/verify-emergency` (baru) | `StockRequest:Approve` | Bukan pemohon; bukan pengotorisasi CITO; izin `Approve` eksplisit; atasan unit peminta |
| `GET /?emergencyReviewState=0..3` (baru) | `StockRequest:Read` | — |

`POST /{id}/verify-emergency`:

```json
{ "outcome": 1, "note": "Sesuai kondisi pasien", "expectedVersion": 3, "idempotencyKey": "phm-verify-cito-..." }
```

- `outcome`: `1` dibenarkan, `2` temuan penyimpangan
- `200`: detail dengan `emergencyReviewState` dan `emergencyReview`
- `403`: tidak berwenang
- `409`:
  - `PHM019`: sudah diverifikasi
  - `PHM010`: bukan jalur CITO
  - `PHM012`: versi tertinggal atau balapan
  - `PHM013`: kunci dipakai dengan isi berbeda
- `422`:
  - `PHM008`: temuan tanpa catatan
  - `PHM011`: hasil tidak dikenal

Field respons baru:

- ringkasan: `emergencyReviewState` (0 tidak perlu, 1 menunggu, 2 selesai, 3 temuan)
- detail:
  - `hasApproveAuthority`, `canVerifyEmergency`, `verifyEmergencyMessage`
  - `emergencyAuthorization {actorUserId, actorName, occurredAt, reason}`
  - `emergencyReview {…, outcome}`

Kode error yang dipakai sudah dicek bebas bentrok di seluruh modul Farmasi. PHM020–022 milik DrugStock, jadi tidak dipakai. PHM008, 010, dan 011 belum pernah dipakai, termasuk di riwayat git.

## Entity dan persistence

| Kebutuhan | Persistence | Migration |
| --- | --- | --- |
| Atasan unit | `MstServiceUnit.OrganizationUnitId`, `MstOrganizationUnit`, `WfpOrganizationAssignment.IsManagerialAssignment`, `MstWorkforceProfile.IsActive`, `ApplicationUser.WorkforceProfileId` | Tidak |
| Izin eksplisit | `ApplicationUserOrganization` × `SysAccessPolicy` × `SysActionAccess`/`SysControllerAccess` | Tidak |
| Verifikasi susulan | `PhmStockRequestHistory` (aksi `VerifyEmergencyStockRequestConfirmed`/`Deviation`); `PhmStockRequest.Version` sebagai penjaga | Tidak |
| Tindak lanjut temuan | — | **Ya. BLOCKED** |
| Kunci batas aktivasi secara teknis | — | **Ya. BLOCKED**; saat ini kendali konfigurasi dan log |

## Hardening yang ditambahkan

1. Izin `StockRequest:Approve` diperiksa eksplisit di service untuk setuju, tolak, dan verifikasi.
2. Profil tenaga kerja pemutus wajib aktif. Unit organisasi wajib aktif dan masih berlaku.
3. Penilaian kewenangan dipakai bersama oleh detail dan eksekusi, sehingga layar dan backend tidak bisa berbeda.
4. Percobaan yang ditolak dicatat (`StockRequest.AuthorityDenied`): pelaku, permintaan, aksi, dan sebab.
5. Log audit keputusan kini memuat status asal, status tujuan, aksi, alasan, dan waktu.
6. Batas aktivasi:
   - permintaan tanpa `SubmittedAt` tidak lagi dianggap permintaan lama;
   - nilai tidak sah atau di masa depan tidak lagi menggagalkan service, melainkan membuat semua permintaan wajib disetujui;
   - setiap nilai baru tercatat di log.

## Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build`, `dotnet test` | NOT RUN pada tahap source |
| Keseimbangan `{}`, `()`, `[]` | PASS untuk seluruh berkas yang diubah |
| Simbol yang dirujuk test | PASS (statis) |
| Kode error bebas bentrok | PASS (statis) |

## Risiko

1. Tanpa data organisasi, tidak ada yang dapat menyetujui di lingkungan mana pun. Ini perilaku yang benar menurut `LGS-DEC-003`, bukan kesalahan.
2. Konkurensi diuji di SQLite. Pembuktian di PostgreSQL membutuhkan sandbox.
3. `ObserveCutover` memakai penanda statis per proses, sehingga perubahan nilai pada instance lain tercatat oleh instance itu sendiri.
