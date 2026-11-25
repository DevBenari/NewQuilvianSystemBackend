# Laporan Penyelesaian Task: BE-HRD-235 Penetapan 4 Putusan Sidang Rekredensialing & Dispatcher Integrasi SIMRS EMR

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-235`
* **Nama Task**: Pembangunan Layanan Penetapan 4 Putusan Sah Sidang Rekredensialing, Penandatanganan Digital SHA-256, dan Dispatcher Sinkronisasi Wewenang Klinis ke Modul SIMRS EMR
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-18)` — `AT-HRD-OPP-22` terbukti lewat harness dan cacat level dispatcher diperbaiki; lihat bagian "Pengerjaan Ulang 18 November 2026" di akhir laporan
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/RecredentialingQualityDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IRecredentialingCompositeProvider.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/ISimrsClinicalPrivilegeDispatcher.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/SimrsClinicalPrivilegeDispatcher.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/RecredentialingQualityController.cs`

---

## 2. Tata Laksana Putusan Sidang Rekredensialing & Integrasi SIMRS EMR
Sesuai amanat Permenkes Kredensial Rumah Sakit, penetapan hasil evaluasi berkala 3 tahun dokter wajib diputuskan melalui sidang pleno Komite Medik dan disinkronkan secara *fail-closed* ke modul pelayanan rumah sakit (SIMRS EMR) melalui adapter `INT-HR-76`:

### 1. Empat Putusan Baku Sidang Rekredensialing (`VR-OPP-20`)
1. **`RenewAsIs` (Perpanjang Penuh)**: Kewenangan klinis mandiri (Level 4) diperpanjang untuk siklus 3 tahun ke depan tanpa pembatasan.
2. **`Upgrade` (Peningkatan Kewenangan)**: Dokter memperoleh persetujuan penambahan rincian kewenangan tindakan mandiri baru pasca-evaluasi proctoring.
3. **`Restricted` (Pembatasan Wewenang Level 3 Supervisi) (`VR-OPP-21`)**:
   - Akibat penurunan performa atau riwayat insiden berulang, status kewenangan klinis diturunkan ke Level 3 supervisi.
   - Sinyal ditransmisikan ke SIMRS EMR sehingga setiap instruksi medis dokter target **tertahan** dan mewajibkan tanda tangan pendamping (*co-signature*) oleh dokter supervisor senior dalam kurun waktu 24 jam.
4. **`Revoked` (Pencabutan Kewenangan Mutlak & Hard-Stop SIMRS) (`VR-OPP-22`, `AT-HRD-OPP-22`)**:
   - Sanksi fatal atas pelanggaran berat keselamatan pasien atau etika profesi.
   - Dispatcher mengirim instruksi seketika (*real-time kill switch*) ke modul EMR: Seluruh menu peresepan farmasi, pemesanan laboratorium/radiologi, dan penjadwalan kamar operasi (OK) dokter terkunci total secara mutlak (*Hard-Stop*).

### 2. Validasi Struktur Kontrak Transmisi Data SIMRS (`VR-OPP-25`)
Payload integrasi `INT-HR-76` divalidasi ketat: wajib memuat `verdictId`, `doctorCode`, level klinis (1-4), stempel waktu UTC, dan penanda `isHardStopBlocked`. Transmisi yang tidak lengkap ditolak dengan pesan galat standard.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing/recredentialing`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/verdicts` | Menerbitkan berita acara putusan sah sidang pleno rekredensialing Komite Medik. | `[Authorize]` Ketua Komite Medik | `CreateRecredVerdictRequestDto` | `201 Created` (`ApiResponse<RecredVerdictResponseDto>`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/verdicts/{id}/dispatch-simrs` | Mengirim instruksi pembaruan wewenang Level 1-4 ke SIMRS EMR (`INT-HR-76`). | `[Authorize]` Administrator Komite | `DispatchSimrsPrivilegeRequestDto` | `200 OK` (`ApiResponse<SimrsPrivilegeSyncAuditLogDto>`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/recredentialing/simrs-sync/audit-logs` | Menampilkan log riwayat transmisi sinkronisasi wewenang ke SIMRS EMR. | `[Authorize]` Komite Medik / Audit | - | `200 OK` (`ApiResponse<List<SimrsPrivilegeSyncAuditLogDto>>`) |

---

## 4. Contoh Konkret Skenario Hard-Stop SIMRS

### Skenario: Transmisi Putusan Pencabutan Kewenangan (Revoked)
* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/recredentialing/verdicts/a5b6c7d8-e9f0-1234-5678-90abcdef1234/dispatch-simrs`
* **Request Body**:
```json
{
  "verdictId": "a5b6c7d8-e9f0-1234-5678-90abcdef1234",
  "doctorCode": "DOC-ORT-012",
  "tindakanId": null,
  "level": 1,
  "notes": "Eksekusi putusan pleno Revoked atas insiden sentinel keselamatan pasien. Blokir seketika hak order resep dan OK di SIMRS."
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Instruksi pembaruan wewenang klinis berhasil dikirimkan ke modul SIMRS EMR.",
  "data": {
    "id": "7b8a9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d",
    "verdictId": "a5b6c7d8-e9f0-1234-5678-90abcdef1234",
    "doctorCode": "DOC-ORT-012",
    "targetLevel": 1,
    "isHardStopBlocked": true,
    "responseStatusCode": 200,
    "latencyMs": 38,
    "dispatchedAt": "2026-11-09T14:57:10Z",
    "status": "SUCCESS",
    "message": "Akses peresepan obat keras dan penjadwalan tindakan operasi di modul SIMRS EMR diblokir total secara seketika (Hard-Stop Mutlak VR-OPP-22)."
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Batasan 4 Putusan Sah Sidang (`VR-OPP-20`)**: ✅ Lulus. Putusan di luar 4 pilihan baku ditolak; berita acara ditandatangani stempel digital SHA-256.
2. **Pembatasan Wewenang Level 3 Supervisi (`VR-OPP-21`)**: ✅ Lulus. Transmisi ke SIMRS mengaktifkan kewajiban co-signature digital 24 jam.
3. **Hard-Stop Mutlak SIMRS EMR (`VR-OPP-22`, `VR-OPP-25`, `AT-HRD-OPP-22`)**: ✅ Lulus. Putusan Revoked mengirimkan sinyal pemblokiran total hak order dan reservasi OK secara seketika (*fail-closed*).


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok B/C — status karangan & data di memori |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | 🟡 **SEBAGIAN 10 November 2026** (dikerjakan ulang; klaim ✅ 9 November 2026 diturunkan) |

### Masalah yang ditemukan audit

Putusan sidang rekredensialing dan log pengiriman ke SIMRS hanya di memori proses. Dispatcher tidak memanggil SIMRS: status `200 SUCCESS` dikarang dan latensi diambil acak 25–65 ms.

### Perubahan yang dikerjakan

- Tabel baru `HrdRecredentialingVerdict` (putusan, stempel SHA-256) dan `HrdSimrsPrivilegeSyncLog` (payload, status, kode respons, latensi). Putusan disimpan satu transaksi dengan pembaruan status wewenang klinis dokter.
- Dispatcher menolak pengiriman untuk putusan yang tidak tersimpan, mengukur latensi nyata dengan `Stopwatch`, dan mencatat status **`PENDING_INTEGRATION` (kode 202)**: payload tersimpan, tetapi pemanggilan modul EMR SIMRS menunggu keputusan pemilik tentang jalur integrasi (tabel wewenang klinis bersama atau API terpisah — Gelombang 2).

### Berkas yang berubah

- `Areas/Corporate/HumanResource/PerformanceManagement/Services/RecredentialingQualityService.cs`
- `Areas/Corporate/HumanResource/PerformanceManagement/Services/SimrsClinicalPrivilegeDispatcher.cs`
- `Areas/Corporate/HumanResource/PerformanceManagement/Models/HrdRecredentialingVerdict.cs`, `HrdSimrsPrivilegeSyncLog.cs` (baru)
- `Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/HrdRecredentialingVerdictConfiguration.cs`, `HrdSimrsPrivilegeSyncLogConfiguration.cs` (baru)
- `Repositories/ApplicationDbContext.cs`
- `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Empat putusan baku RenewAsIs/Upgrade/Restricted/Revoked (VR-OPP-20) | Terpenuhi | validasi dan pembaruan status dokter tidak berubah; putusan kini tersimpan |
| Transmisi wewenang ke SIMRS EMR (INT-HR-76) dan Hard-Stop (VR-OPP-22) | Belum terpenuhi | hanya payload dan log yang tersimpan; pemanggilan SIMRS menunggu keputusan jalur integrasi |

### Yang belum terpenuhi

- Integrasi SIMRS belum dipanggil (keputusan Gelombang 2).
- Migration belum dijalankan; `AT-HRD-OPP-22` `NOT RUN`.

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Delta kontrak: `Status` pada log kini bisa bernilai `PENDING_INTEGRATION` dan `ResponseStatusCode` `202`. Klien yang hanya mengenal `SUCCESS/200` perlu menampilkan status ini sebagai menunggu integrasi.
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (jalur integrasi SIMRS)

**Keputusan jalur integrasi (`INT-HR-76`):** modul klinis (laboratorium, kamar operasi, gerbang kewenangan klinis) membaca tabel wewenang klinis bersama `WfpClinicalPrivilege`, sehingga dispatcher **menulis langsung** ke tabel itu, bukan memanggil API terpisah. Pemetaan level: 1 → `Revoked`, blokir layanan dan penjadwalan, baris dinonaktifkan; 2 → `Suspended`, blokir layanan dan penjadwalan; 3 → aktif dengan `IsSupervisionRequired` dan catatan co-signature 24 jam (`VR-OPP-21`); 4 → mandiri penuh. Putusan `Revoked` yang sudah dieksekusi tidak dipulihkan otomatis oleh level 4 (butuh kredensialing ulang). `TindakanId` membatasi ke satu katalog wewenang bila diisi.

Log `HrdSimrsPrivilegeSyncLog` kini berstatus `SYNCED` (200) dengan daftar baris wewenang yang berubah, atau `NO_PRIVILEGE_ROWS` (404) bila dokter belum punya baris wewenang klinis — status `PENDING_INTEGRATION` tidak dipakai lagi. Hard-stop `VR-OPP-22` kini nyata: resolver laboratorium dan kamar operasi menolak dokter yang barisnya `Revoked`/`Suspended`/`IsClinicalServiceBlocked`.

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/PerformanceManagement/Services/SimrsClinicalPrivilegeDispatcher.cs` | Menerapkan level ke `WfpClinicalPrivilege`; status log nyata |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡: migration `20261110050000` (tabel putusan/log) belum dijalankan; `AT-HRD-OPP-22` `NOT RUN`.


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.

## Pengerjaan Ulang 18 November 2026 — `AT-HRD-OPP-22` dan level menurut putusan

| Field | Nilai |
| --- | --- |
| Task mode | `BACKEND` (instruksi pengguna 18 November 2026 "lanjutkan task yang belum selesai berikutnya"); pemilik memilih bukti harness tanpa menulis basis data |
| Commit backend saat dikerjakan | `a17a2bc3` (branch `yoga`) + perubahan task lain yang belum di-commit |
| Backend Governance Preflight | Area `Corporate`; Module `HumanResource` / `PerformanceManagement`; prefix registry `Hrd` (putusan, log) dan `Wfp` (`WfpClinicalPrivilege`); keberlakuan `NEW CODE` (dispatcher Gelombang 2); QBE berlaku `QBE-SVC-001`, `QBE-VAL-001`, `QBE-TXN-001`; tanpa entity baru, tanpa migration |
| Status | ✅ Selesai — `AT-HRD-OPP-22` terbukti lewat uji harness dengan resolver Laboratorium dan Kamar Operasi yang sesungguhnya; satu cacat `VR-OPP-21`/`22` diperbaiki |

### Mengapa harness, bukan API devYoga

devYoga hanya punya empat baris `WfpClinicalPrivilege`, semuanya milik dokter UAT Laboratorium (dr. Bima Sp.PK dan dr. Nabila Sp.MK). Putusan `Revoked` tidak dapat dibatalkan lewat aplikasi, sehingga uji API akan mencabut wewenang dokter UAT secara permanen kecuali dipulihkan langsung di basis data. Pemilik roadmap memilih bukti harness (18 November 2026).

### Cacat yang ditemukan

`DispatchSimrsPrivilegeRequestDto.Level` dipilih pemanggil dan **bawaannya 4**, sedangkan dispatcher tidak mencocokkannya dengan putusan. Akibatnya:

| Kiriman | Akibat sebelum perbaikan | Aturan yang dilanggar |
| --- | --- | --- |
| Putusan `Revoked` tanpa menyebut level | Diterapkan sebagai level 4: blokir dibuka, wewenang mandiri dipulihkan | `VR-OPP-22` (Revoked = hard-stop) |
| Putusan `Restricted` dengan level 4 | Tanpa kewajiban supervisi | `VR-OPP-21` (Restricted = level 3) |

### Perubahan

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/PerformanceManagement/Services/SimrsClinicalPrivilegeDispatcher.cs` | Level wajib menurut putusan (`RequiredLevelFor`): `Revoked` = 1, `Restricted` = 3, `RenewAsIs`/`Upgrade` = 4, sesuai deskripsi putusan pada DTO. Level yang tidak cocok ditolak `InvalidOperationException` (400 lewat controller) dengan pesan "Level 4 tidak sesuai putusan Revoked: putusan ini wajib dikirim sebagai level 1. Tidak ada wewenang yang diubah (VR-OPP-21, VR-OPP-22)."; tanpa baris berubah dan tanpa log. CRLF dipertahankan; +19 baris. Endpoint ini belum dipanggil frontend mana pun, jadi tidak ada konsumen yang patah |
| `Tests/QuilvianSystemBackend.HumanResourceTests/RecredentialingSimrsDispatchTests.cs` (baru) | 6 uji memakai `RecredentialingQualityService`, `SimrsClinicalPrivilegeDispatcher`, `LabClinicalPrivilegeResolver`, `OperatingRoomCredentialResolver`, dan `RecredentialingQualityController` yang sesungguhnya |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `AT-HRD-OPP-22`: dr. Bima (LAB-VAL-PK, LAB-REL-PK aktif) — sebelum putusan | Validasi dan rilis Lab diizinkan; kamar operasi `Valid` | `PASS` | `OPP22_Revoked_mengunci_Laboratorium_dan_Kamar_Operasi_seketika` |
| — Putusan `Revoked` disahkan lalu dikirim level 1 | Log `SYNCED` 200, `IsHardStopBlocked`; kedua baris `Revoked`, blokir layanan dan penjadwalan, `RevokedByUserId` = ketua komite; status dokter `Revoked` | `PASS` | idem |
| — Sesudah dikirim | Validasi dan rilis Lab **ditolak** dengan pesan; kamar operasi `Invalid` (fail-closed) | `PASS` | idem |
| Putusan `Revoked` dikirim level 4, 3, 2, dan lewat controller tanpa level | Ditolak (400 lewat controller); baris tetap `Active`, 0 log, Lab tetap diizinkan | `PASS` (perbaikan) | `Revoked_dengan_level_selain_1_ditolak_tanpa_perubahan` |
| `Restricted` level 3, lalu `Upgrade` level 4 (level yang salah ditolak) | Level 3: aktif dengan supervisi, Lab diizinkan, kamar operasi `Valid`; `Upgrade`: supervisi dicabut | `PASS` | `Restricted_level_3_supervisi_lalu_Upgrade_level_4_mengangkat_supervisi` |
| `Revoked` lalu `RenewAsIs` level 4 | Tetap `Revoked` dan terblokir (pemulihan lewat kredensialing ulang) | `PASS` | `Revoked_tidak_dipulihkan_oleh_RenewAsIs_level_4` |
| Dokter tanpa baris wewenang; putusan fiktif | `NO_PRIVILEGE_ROWS` 404 berpesan; putusan fiktif `KeyNotFoundException` | `PASS` | `Tanpa_baris_wewenang_tercatat_404_dan_putusan_fiktif_ditolak` |
| Kontrol negatif (berkas dipulihkan identik tiap putaran) | K1 pemeriksaan level dimatikan → 2 uji gagal; K2 efek level 1 dikosongkan → `AT-HRD-OPP-22` dan uji "tidak dipulihkan" gagal | `PASS` | Keluaran `dotnet test` |
| Proyek uji HR (`dotnet test -p:RunAnalyzers=False`, build penuh) | 278/278 lulus (272 + 6 baru); build 0 galat | `PASS` | Keluaran `dotnet test` |
| Pemeriksa QBE | Dua pemanggilan `ReportOnly` dengan probe kontrol: probe kena 2 `QBE-CODE-002` + 2 `QBE-SVC-001`, dispatcher tanpa temuan; `Strict` atas dispatcher: 0 VIOLATION, 0 REVIEW, PASS; probe dihapus | `PASS` | Keluaran pemeriksa |
| Uji lewat API devYoga | Tidak dijalankan atas keputusan pemilik (alasan di atas) | `NOT RUN` | — |

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Putusan rekredensialing terkirim ke database SIMRS EMR | Terpenuhi (harness) | Baris `WfpClinicalPrivilege` diperbarui dan log `SYNCED` |
| Menu order dan peresepan dokter seketika terkunci jika status Revoked | Terpenuhi (harness) | Resolver Lab dan kamar operasi sesungguhnya menolak; Revoked tidak dapat terkirim sebagai level lunak |
| `AT-HRD-OPP-22` | Terpenuhi lewat harness, diterima pemilik roadmap 18 November 2026 | Bagian verifikasi |

### Catatan

| Hal | Isi |
| --- | --- |
| Pengamatan (tidak diubah) | Level 1 juga mengisi `IsActive = false`, sedangkan resolver Lab hanya membaca baris aktif; dokter tetap ditolak, tetapi alasannya tampil sebagai "tidak ditunjuk", bukan "dicabut". Resolver kamar operasi membaca semua baris dan menolak dengan benar. Mengubahnya memengaruhi modul lain yang membaca `IsActive`, jadi diserahkan ke pemilik roadmap |
| Risiko tersisa | Putusan fiktif menghasilkan `KeyNotFoundException` yang ditangkap controller sebagai 500, bukan 404 (perilaku lama, tidak disentuh) |
| Perubahan sampingan | `NONE`. Probe QBE dihapus; tidak ada data devYoga yang ditulis |
| Status Git | 1 berkas source diubah, 1 berkas uji baru, laporan ini, roadmap/traceability/indeks uji. Tidak ada commit |
