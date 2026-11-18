# LGS-BE-ADH-02 — Persetujuan permintaan barang unit (LGS-DEC-002)

> **Disusul `LGS-BE-ADH-03` (2026-11-17).** Risiko 1–3 dan 5 serta catatan peralihan data di
> bawah sudah ditangani atau diperbarui di sana: kewenangan atasan unit dari struktur organisasi,
> otorisasi CITO yang tidak dapat dilewati peran SuperAdmin, aturan peralihan, dan test backend.

- TASK ID: `LGS-BE-ADH-02` (task ad-hoc, belum ada roadmap). Mencakup BE-01 sampai BE-05 pada instruksi pemrakarsa "Instruksi Eksekusi Agent — LGS-OQ-004", 17 November 2026
- TASK TYPE: NEW CODE pada modul existing (`PharmacyManagement`), TOUCHED LEGACY pada `StockRequestService`
- COMPLEXITY: Menengah. Satu alur status, tiga endpoint baru, satu perubahan perilaku pada penyerahan
- CLASSIFICATION SCORE: tidak dihitung
- MODEL: Claude Opus 5.5
- TASK MODE: BACKEND (dinyatakan eksplisit oleh pemrakarsa)
- WRITE TARGET: `NewQuilvianSystemBackend`, worktree `C:\tmp\phm`, branch `Ikbal`, HEAD `d946b20b`
- FILES INSPECTED: `Services/StockRequestService.cs`, `Controllers/StockRequestController.cs`, `DTOs/StockRequestDtos.cs`, `Enums/StockRequestEnums.cs`, `Models/PhmStockRequest.cs`, `Services/Security/AccessPermissionService.cs`, `Attributes/AccessPermission*`, `appsettings.json`, `appsettings.Development.json`, model `MstServiceUnit`, `MstWorkforceProfile`, `ApplicationUser`
- FILES CHANGED (semua di `Areas/HealthServices/PharmacyManagement/`):
  - `Enums/StockRequestEnums.cs`: hanya komentar. Nilai enum tidak berubah (Draft=1, Submitted=2, Approved=3, Completed=4, Rejected=5, Cancelled=6)
  - `DTOs/StockRequestDtos.cs`: `SubmitStockRequestRequest.EmergencyReason`; kelas baru `ApproveStockRequestRequest`, `RejectStockRequestRequest`, `AuthorizeEmergencyStockRequestRequest`; `StockRequestPagedQuery.OrderByPriority`; ringkasan `CanApprove`, `CanAuthorizeEmergency`, `IsPartiallyFulfilled`; detail `IsOwnRequest`; riwayat `ActorUserId`, `ActorName`
  - `Services/StockRequestService.cs`: `ApproveAsync`, `RejectAsync`, `AuthorizeEmergencyAsync` lewat `DecideAsync`; `IsRequesterAsync`; perubahan `SubmitAsync`, `FulfillAsync`, `CancelAsync`, `GetPagedAsync`, `GetDetailAsync`
  - `Controllers/StockRequestController.cs`: tiga endpoint baru

## Pemetaan status (BE-01)

| Status bisnis | Nilai di source | Keterangan |
| --- | --- | --- |
| Draft | `Draft` (1) | Tidak berubah |
| Menunggu Persetujuan | `Submitted` (2) | Arti baru; dulu "terkirim ke gudang" |
| Disetujui | `Approved` (3) | Disetujui atasan atau diotorisasi CITO. Satu-satunya status yang boleh diserahkan |
| Ditolak | `Rejected` (5) | Final, wajib beralasan |
| Diproses Gudang | — | Bukan status tersendiri. Penyerahan tercatat dalam satu langkah `Approved` → `Completed` |
| Dipenuhi Sebagian | `Completed` (4) + `IsPartiallyFulfilled` | Penanda dihitung, tidak disimpan |
| Selesai | `Completed` (4) | Tidak berubah |
| Dibatalkan | `Cancelled` (6) | Kini juga boleh dari `Approved` selama barang belum diserahkan |

Transisi yang sah:

```text
Draft ──submit──▶ Submitted ──approve──────────────▶ Approved ──fulfill──▶ Completed
                     │      ──authorize-emergency──▶ (hanya bila Priority = Emergency)
                     ├──reject──▶ Rejected (final)
Draft / Submitted / Approved ──cancel──▶ Cancelled
```

## Kontrak API (BE-05)

Base: `api/v1/health-services/pharmacy-management/stock-requests`

### `POST /{id}/approve`

- Izin: `StockRequest:Approve`
- Body:

```json
{ "note": "Stok depo menipis", "expectedVersion": 3, "idempotencyKey": "phm-approve-..." }
```

- `200`: `ApiResponse<StockRequestDetailResponse>` dengan `status = 3`
- `403`: tanpa izin, atau petugas adalah pemohon
- `409 PHM018`: sudah diputuskan; `409 PHM004`: bukan Menunggu Persetujuan; `409` versi tertinggal; `409 PHM013`: kunci idempotensi dipakai permintaan lain

### `POST /{id}/reject`

- Izin: `StockRequest:Approve`
- Body: `{ "reason": "Stok depo masih cukup", "expectedVersion": 3, "idempotencyKey": "..." }`
- `400`: alasan kosong ditolak validasi model (`[Required]`); `422 PHM015` bila lolos validasi model tetapi kosong setelah dinormalkan. Kode lain sama dengan `approve`

### `POST /{id}/authorize-emergency`

- Izin: `StockRequest:AuthorizeEmergency`
- Body: `{ "reason": "Pasien IGD, stok depo habis", "expectedVersion": 3, "idempotencyKey": "..." }`
- `400` atau `422 PHM015`: alasan kosong; `422 PHM017`: permintaan bukan CITO

### Perubahan endpoint lama

- `POST /{id}/submit`: field opsional `emergencyReason`. `422 PHM014` bila prioritas CITO dan alasan kosong. Klien lama tanpa field ini tetap diterima untuk permintaan non-CITO, dan sidik jari idempotensinya tidak berubah
- `POST /{id}/fulfill`: `409 PHM016` bila permintaan masih Menunggu Persetujuan. **Perubahan perilaku**: dulu `Submitted` boleh diserahkan
- `GET /`: query opsional `orderByPriority=true` (CITO dulu, lalu yang paling lama menunggu)
- Respons ringkasan dan detail mendapat field tambahan (aditif)

## Aturan keputusan (BE-02, BE-03)

Urutan pemeriksaan di `DecideAsync` (`StockRequestService.cs` baris 482–553):

1. Format kunci idempotensi
2. Alasan wajib untuk tolak dan CITO
3. Pengulangan kunci yang sama mengembalikan hasil sebelumnya
4. Sudah Approved atau Rejected → `PHM018`
5. Bukan Submitted → `PHM004`
6. Jalur CITO untuk permintaan non-CITO → `PHM017`
7. Versi
8. Pemohon → `403` (aturan 7)
9. Ubah status, catat `DecidedAt`, `DecisionReason`, riwayat (pelaku, waktu, dari-ke status, alasan), audit log

Jalur keputusan tidak mengubah stok. Stok baru berkurang saat `fulfill`.

`IsRequesterAsync` (baris 555) menganggap petugas sebagai pemohon bila ia pembuat record, pelaku riwayat buat/ajukan, atau akunnya terhubung ke profil tenaga kerja `RequestedByWorkforceId`.

## BACKEND GOVERNANCE PREFLIGHT

- Area `HealthServices`, Module `PharmacyManagement`
- Registry `Phm`: `ACTIVE / LEGACY`
- Keberlakuan: NEW CODE di modul existing. Tidak ada entity baru, sehingga QBE-MOD-002/003 tidak berlaku
- QBE yang berlaku: QBE-SVC-001, QBE-API-001, QBE-PERM-001, QBE-DTO-001, QBE-VAL-001

## Dampak

- BLUEPRINT STATUS/EVIDENCE: `LGS-BP-001` r3, `LGS-DEC-002` `approved` (Ikbal Yuliyanto, 2026-11-17)
- DATABASE IMPACT: tidak ada. Tanpa kolom, tabel, configuration, maupun migration. Kolom status `PhmStockRequest` tidak punya CHECK constraint, sehingga nilai 3 dan 5 dapat dipakai tanpa perubahan skema. Alasan CITO pemohon disimpan di kolom `Reason` riwayat yang sudah ada
- SECURITY IMPACT: dua izin baru (`StockRequest:Approve`, `StockRequest:AuthorizeEmergency`) terdaftar otomatis dari atribut. Keduanya tidak diberikan ke peran mana pun
- VISUAL REFERENCE: NOT REQUIRED

## Validasi

| Pemeriksaan | Hasil | Jenis | Catatan |
| --- | --- | --- | --- |
| `dotnet build` | NOT RUN | — | Dilarang instruksi pemrakarsa; kompilasi belum terbukti |
| Test xUnit | NOT WRITTEN | — | Tidak dapat dikompilasi pada sesi ini. Test yang tidak bisa dijalankan berisiko merusak proyek test di CI |
| Keseimbangan `{}` dan `()` | PASS | statis | Service 88/88 dan 427/427; DTO 111/111; Controller 61/61 dan 180/180; Enum 2/2 |
| Jumlah endpoint dan atribut izin di controller | PASS | statis | 10 endpoint, 10 `AccessPermission` |
| Konsumen lain `StockRequestStatus` atau `IStockRequestService` | PASS | statis | Tidak ada di luar service dan enum; tidak ada test backend yang memakainya |
| Kecocokan kontrak dengan konsumen FE | PASS | statis | Diuji `tests/unit/stock-request-approval.test.mjs` (12 test) |

## Risiko dan known issues

1. **Pembatasan per unit belum ada.** Pemegang `StockRequest:Approve` dapat memutus permintaan unit mana pun. Sistem belum menyimpan hubungan unit ↔ atasan (`LGS-OQ-020`).
2. **SuperAdmin melewati pemeriksaan izin.** `AccessPermissionService.HasAccessAsync` menjawab benar untuk peran `SuperAdmin` selama `Security:Authorization:EnforceClinicalPolicyForSuperAdmin = false` (nilai di `appsettings.json`). Akibatnya SuperAdmin dapat menyetujui dan mengotorisasi CITO walaupun izinnya tidak diberikan. Larangan memutus permintaan sendiri tetap berlaku untuknya, karena diperiksa di service. Kebijakan platform ini tidak diubah oleh task ini.
3. **Pemeriksaan izin mati di mesin pengembang.** `appsettings.Development.json` lokal (tidak masuk git) berisi `Security:Authorization:Enabled = false`. Di mesin ini semua pengguna yang login lolos pemeriksaan izin. Uji manual izin harus memakai konfigurasi dengan pemeriksaan aktif.
4. **Peralihan data.** Permintaan berstatus `Submitted` yang dibuat sebelum perubahan kini menunggu persetujuan dan tidak bisa langsung diserahkan.
5. **Validasi atasan susulan untuk CITO** tidak diterapkan (`LGS-OQ-022`).

## Penutup

- MANUAL TEST: NOT FEASIBLE. Build, dev server, dan pemanggilan API dilarang pada sesi ini
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: pemadatan konteks satu kali; dilanjutkan dari diff terakhir tanpa penyuntingan ganda
- GIT STATUS: tujuh file `M` (termasuk tiga milik `LGS-BE-ADH-01`); blueprint `logistik/` untracked; belum di-commit
- NEXT RECOMMENDED STEP: (1) `dotnet build -p:SkipMigrationMetadata=true`; (2) berikan `StockRequest:Approve` ke peran atasan uji; (3) uji alur submit → approve/reject → fulfill dan submit CITO → authorize-emergency → fulfill dengan pemeriksaan izin aktif; (4) jawab `LGS-OQ-020`, `021`, `022`
