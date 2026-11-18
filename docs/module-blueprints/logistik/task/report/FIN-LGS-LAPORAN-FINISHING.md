# FIN-LGS — Laporan Finishing Logistik V2 Pasca-Integration

| Field | Nilai |
| --- | --- |
| Blueprint | `LGS-BP-001` r6 |
| Keputusan yang berlaku | `LGS-DEC-001` sampai `LGS-DEC-006` (tidak ada keputusan baru) |
| Pertanyaan baru | `LGS-OQ-024` sampai `LGS-OQ-028` (OPEN) |
| Backend | `NewQuilvianSystemBackend`, branch `Ikbal` @ `33dd4eb1` (sama dengan integration `ffedc61c` = merge PR #356) + perubahan lokal belum di-commit |
| Frontend | `QuilvianSystemFrontendDev`, branch `Ikbalv2` @ `0aae59c80` (sama dengan `QuilvianIntegrationFrontend`) + perubahan lokal belum di-commit |
| Tanggal | 18 November 2026 |
| Batas verifikasi | Pemeriksaan statis dan uji unit frontend saja. **Tidak ada** `dotnet build`, `dotnet test`, `npm run build/dev`, runtime, migrasi, perubahan basis data, commit, maupun push |

---

## 1. Executive Summary

Fitur persetujuan permintaan barang unit sudah lengkap di level source, termasuk:

- persetujuan atasan unit;
- otorisasi CITO;
- verifikasi susulan;
- batas aktivasi.

Fitur ini sudah masuk integration lewat PR #356. Pada tahap finishing ini ditambahkan empat hal:

| Penambahan | Isi |
| --- | --- |
| **"Unit yang Saya Pimpin"** | Daftar persetujuan dan verifikasi kini dapat dibatasi ke unit yang benar-benar dipimpin pengguna. Batasan itu dihitung backend dari struktur organisasi, tidak dari daftar unit yang dikirim layar |
| **Penunjukan jabatan CITO** | Otorisasi CITO kini butuh dua syarat sekaligus: jabatan yang ditunjuk manajemen, dan izin eksplisit. Penunjukan yang kosong berarti tidak ada yang berwenang |
| **Tenggat verifikasi susulan** | Dapat dikonfigurasi, tanpa nilai bawaan. Keadaannya ada empat: belum jatuh tempo, mendekati tenggat, terlambat, dan belum dikonfigurasi |
| **Laporan kesiapan data organisasi** | Hanya membaca dan tidak memuat identitas pegawai. Menunjukkan apa yang masih harus diisi HRD dan manajemen |

**Fitur belum dapat dipakai pengguna** karena data operasionalnya belum ada:

- tautan unit layanan ke unit organisasi masih 0 dari 20;
- penempatan manajerial masih 0;
- belum ada jabatan yang diberi izin persetujuan;
- belum ada jabatan CITO yang ditunjuk.

Dua bagian terblokir di luar data:

- **Tindak lanjut temuan CITO** membutuhkan tabel baru (`BLOCKED_MIGRATION`).
- **Notifikasi** membutuhkan keputusan infrastruktur (`LGS-OQ-024`).

---

## 2. Status integrasi backend dan frontend

| Repo | Integration | Lokal | Selisih |
| --- | --- | --- | --- |
| Backend | `ffedc61c` (merge PR #356) | `33dd4eb1` | Integration = lokal + merge commit. Tidak ada perubahan lain pada berkas Logistik |
| Frontend | `0aae59c80` | `0aae59c80` | Sama |

Hasil audit FIN-LGS-BE-001 dengan membaca source integration:

| Butir | Hasil | Bukti |
| --- | --- | --- |
| Endpoint approve, reject, authorize-emergency | Ada | `StockRequestController.cs` |
| Endpoint verify-emergency | Ada | `StockRequestController.cs` |
| Filter `emergencyReviewState` di backend | Ada | `StockRequestService.GetPagedAsync` |
| Detail mengembalikan kewenangan pengguna | Ada | `hasApproveAuthority`, `hasUnitAuthority`, `hasEmergencyAuthority`, `canVerifyEmergency` |
| Izin dan kewenangan unit diperiksa di service | Ada | `EvaluateDeciderAsync`, `StockRequestApprovalAuthority` |
| SuperAdmin tidak dapat melewati otorisasi kritis | Ada di service; kebijakan global tidak diubah | Pemeriksaan izin eksplisit tanpa jalan pintas SuperAdmin |
| Transisi status sesuai lifecycle | Ada | `DecideAsync`, `FulfillAsync`, token versi |
| Kontrak frontend dan backend konsisten | Konsisten; diperluas pada tahap ini | Bagian 5 |

Semua butir di atas dinilai dari **pembacaan source**, bukan dari uji runtime. Uji otomatis
backend yang pernah dijalankan (64/64, 17 November 2026) belum mencakup penambahan tahap ini.

---

## 3. Matriks task FIN-LGS

| Task | Status | Ringkasan |
| --- | --- | --- |
| FIN-LGS-BE-001 Audit integrasi | COMPLETED_SOURCE | Audit dengan membaca source; tidak ditemukan ketidaksesuaian yang perlu diperbaiki |
| FIN-LGS-BE-002 Kewenangan unit | COMPLETED_SOURCE + BLOCKED_DATA | Unit yang dipimpin, endpoint cakupan, dan endpoint kesiapan selesai. Data organisasi belum ada |
| FIN-LGS-BE-003 Otorisasi CITO | COMPLETED_SOURCE + BLOCKED_DATA | Syarat jabatan yang ditunjuk ditambahkan. Penunjukan dan izinnya belum ada |
| FIN-LGS-BE-004 Tindak lanjut temuan | PARTIAL / BLOCKED_MIGRATION | Keadaan "Ada Temuan" dan pemberitahuan di layar tersedia. PIC, status tindak lanjut, dan penyelesaian butuh entity baru; PIC operasional belum ditetapkan |
| FIN-LGS-BE-005 Tenggat verifikasi | COMPLETED_SOURCE + BLOCKED_DATA | Tenggat dapat dikonfigurasi, dihitung dalam UTC, dan dapat disaring. Nilainya belum ditetapkan. Pengingat terblokir (lihat BE-007) |
| FIN-LGS-BE-006 Filter "Unit yang Saya Pimpin" | COMPLETED_SOURCE | `scope=1` dihitung backend; ringkasan, halaman, dan pencarian ikut konsisten |
| FIN-LGS-BE-007 Notifikasi | BLOCKED (keputusan infrastruktur, `LGS-OQ-024`) | Tidak ada notifikasi tiruan; spesifikasi kebutuhan ada di bagian 9 |
| FIN-LGS-BE-008 Keamanan dan konsistensi API | COMPLETED_SOURCE | Ditinjau di bagian 6 |
| FIN-LGS-FE-001 Validasi Atasan | COMPLETED_SOURCE | Skema tampilan diterapkan (bagian 7) |
| FIN-LGS-FE-002 Verifikasi Susulan CITO | COMPLETED_SOURCE | Skema tampilan diterapkan (bagian 7) |
| FIN-LGS-FE-003 Notifikasi/status | PARTIAL | Strip "Perlu Perhatian Anda" dari hitungan backend. Notifikasi sungguhan terblokir `LGS-OQ-024` |
| FIN-LGS-FE-004 Konsistensi UI/UX | PARTIAL | Hanya base component Quilvian; loading, kosong, error, coba lagi, dan izin ditangani. Uji responsif di browser belum dilakukan |
| FIN-LGS-QA-001 Persiapan UAT | COMPLETED_SOURCE (skenario) | Bagian 12; semua kasus **NOT RUN** |
| FIN-LGS-QA-002 Konkurensi PostgreSQL | BLOCKED_SANDBOX | Prosedur di bagian 15 |

---

## 4. Berkas source yang diubah

### Backend (`C:\tmp\phm`)

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalAuthority.cs` | Penambahan berikut: (1) `GetSupervisedServiceUnitsAsync`: unit yang dipimpin. (2) `CheckEmergencyAuthorityAsync`: jabatan ditunjuk + izin eksplisit. (3) `ResolveDesignatedPositionIdsAsync`. (4) Pembatasan izin per jabatan pada `HasExplicitGrantAsync` |
| `Areas/HealthServices/PharmacyManagement/Services/StockRequestOperationalSettings.cs` (baru) | `StockRequestEmergencyAuthorizerDesignation` dan `StockRequestEmergencyReviewDeadline`, keduanya fungsi murni |
| `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` | Penambahan berikut: (1) Filter `Scope` dan `EmergencyReviewDueState`. (2) Kolom otorisasi CITO dan tenggat per baris. (3) `GetApprovalScopeAsync`. (4) `GetApprovalReadinessAsync`. (5) Pesan otorisasi CITO yang lebih jelas. (6) Pencatatan perubahan konfigurasi ke log |
| `Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs` | Field dan respons baru (bagian 5) |
| `Areas/HealthServices/PharmacyManagement/Enums/StockRequestEnums.cs` | `StockRequestEmergencyReviewDueState`, `StockRequestListScope` |
| `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` | `GET approval-scope`, `GET approval-readiness` |
| `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs` | Konfigurasi jabatan CITO dan tenggat; atasan dua unit; jabatan uji; `MundurkanOtorisasiAsync` |
| `Tests/.../StockRequestApprovalTests/ApprovalScopeTests.cs` (baru) | 15 metode uji |
| `Tests/.../StockRequestApprovalTests/OperationalSettingsUnitTests.cs` (baru) | 6 metode uji |
| `docs/module-blueprints/logistik/00-interview-decisions.md` | r6: `LGS-OQ-024` sampai `028` |
| `docs/module-blueprints/logistik/blueprint-manifest.md` | r6 |
| `docs/module-blueprints/logistik/evidence/05-aktivasi-batas-persetujuan.md` | Konfigurasi jabatan CITO dan tenggat |
| `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` (baru) | Laporan ini |

### Frontend (`QuilvianSystemFrontendDev`)

Semua berkas komponen berada di `src/components/view/health-services/pharmacy-management/stock-request-view/`, disingkat `.../`.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../stock-request-constant.jsx` | Cakupan, keadaan tenggat, label, dan varian |
| `src/lib/services/.../stock-request.service.js` | `getStockRequestApprovalScope` |
| `src/lib/hooks/.../use-stock-request-approval-scope.jsx` (baru) | Cakupan kewenangan |
| `src/lib/hooks/.../use-stock-request-approval-summary.jsx` | Set kartu sesuai skema; cakupan ikut dikirim |
| `src/lib/hooks/.../use-stock-request-list.jsx` | Filter `scope` dan `emergencyReviewDueState` |
| `src/utils/.../stock-request-approval-utils.js` | Label dan varian tenggat |
| `.../stock-request-list-view.jsx` | Validasi Permintaan Barang sesuai skema; strip perhatian di riwayat dan antrean gudang |
| `.../stock-request-emergency-review-view.jsx` | Verifikasi Susulan CITO sesuai skema |
| `.../stock-request-detail-view.jsx` | Informasi Kewenangan, Otorisasi Awal, dan Tindak Lanjut |
| `.../components/stock-request-emergency-review-panel.jsx` | Hanya hasil dan form verifikasi |
| `.../components/stock-request-summary-cards.jsx` (baru) | Kartu ringkasan |
| `.../components/stock-request-authority-info.jsx` (baru) | Informasi kewenangan pengguna |
| `.../components/stock-request-decision-authority-info.jsx` (baru) | Informasi kewenangan atas satu permintaan |
| `.../components/stock-request-emergency-authorization-info.jsx` (baru) | Alasan CITO, otorisasi awal, tenggat |
| `.../components/stock-request-emergency-follow-up-notice.jsx` (baru) | Pemberitahuan tindak lanjut yang masih terblokir |
| `.../components/stock-request-attention-strip.jsx` (baru) | "Perlu Perhatian Anda" |
| `tests/unit/stock-request-approval.test.mjs` | 23 test; 3 test baru dan 1 test diganti |

Menu tidak diubah; 67 entri `LGS-DEC-001` tetap utuh.

---

## 5. Perubahan API dan DTO

Base: `api/v1/health-services/pharmacy-management/stock-requests`

### `GET /stock-requests/approval-scope` (baru)

- Izin: `StockRequest:Read`
- Body: tidak ada. Kewenangan dihitung dari pengguna yang login, bukan dari masukan klien.
- `200`:

```json
{
  "hasApprovePermission": true,
  "hasEmergencyAuthority": false,
  "emergencyDesignationConfigured": true,
  "supervisedUnits": [
    { "serviceUnitId": "…", "serviceUnitCode": "SU-IPD-001", "serviceUnitName": "Rawat Inap", "organizationUnitName": "Instalasi Rawat Inap" }
  ],
  "message": null
}
```

- `message` menjelaskan bila cakupan kosong. Contoh: "Penempatan jabatan Anda belum diberi izin StockRequest:Approve…", atau "Anda belum memiliki penempatan manajerial aktif…".
- `401` bila belum login; `403` bila izin `Read` tidak ada.

### `GET /stock-requests/approval-readiness` (baru)

- Izin: `StockRequest:ViewApprovalReadiness` (baru, tidak diberikan kepada siapa pun; `LGS-OQ-028`)
- `200`: jumlah unit aktif, unit tertaut, unit tertaut yang punya atasan, penempatan manajerial, izin Approve, izin CITO, jabatan CITO yang ditunjuk dan yang sudah diberi izin, keadaan batas aktivasi, keadaan tenggat, `isReadyForApproval`, `isReadyForEmergency`, `blockers[]`, dan `units[]` (kode, nama unit, unit organisasi, jumlah atasan aktif). **Tanpa identitas pegawai.**

### `GET /stock-requests` (diperluas, kompatibel)

| Query baru | Nilai | Arti |
| --- | --- | --- |
| `scope` | `0` (bawaan) / `1` | `1` = "Unit yang Saya Pimpin": unit yang dipimpin bila izin Approve eksplisit ada, ditambah permintaan CITO bila pengguna berwenang CITO. Tanpa keduanya hasilnya kosong |
| `emergencyReviewDueState` | `2` belum jatuh tempo, `3` mendekati, `4` terlambat | Hanya CITO yang menunggu verifikasi; kosong bila tenggat belum dikonfigurasi |

Field ringkasan baru:

- `emergencyAuthorizedAt`
- `emergencyAuthorizedByName`
- `emergencyReviewDueAt`
- `emergencyReviewDueState` (0 tidak berlaku, 1 belum dikonfigurasi, 2 belum jatuh tempo, 3 mendekati, 4 terlambat)

Detail mewarisi field yang sama.

Tidak ada endpoint lama yang berubah perilakunya. Bila `scope` tidak dikirim, hasilnya sama
seperti sebelumnya.

---

## 6. Pemeriksaan otorisasi (FIN-LGS-BE-008)

| Aspek | Hasil |
| --- | --- |
| Autentikasi | `[Authorize]` pada controller; tanpa login → `401` |
| Otorisasi endpoint | `[AccessPermission]` pada setiap endpoint, termasuk dua endpoint baru |
| Otorisasi kritis di service | Pemeriksaan berikut **tidak** dilewati SuperAdmin maupun `Security:Authorization:Enabled=false`: (1) setuju dan tolak: izin Approve eksplisit + atasan unit; (2) CITO: jabatan ditunjuk + izin eksplisit; (3) verifikasi: izin Approve + atasan unit, bukan pemohon, bukan pengotorisasi |
| Bawaan TOLAK | Penunjukan CITO kosong → tidak ada yang berwenang. Cakupan tanpa kewenangan → daftar kosong. Izin `ViewApprovalReadiness` tidak diberikan |
| Cakupan unit | Dihitung dari data server; daftar unit dari klien tidak pernah dipakai |
| Validasi | Alasan wajib (`PHM014`, `PHM015`, `PHM008`); nilai enum yang tidak dikenal → `400` (model binding) atau `422 PHM011` |
| Kode HTTP | `403` tidak berwenang, `404` tidak ditemukan, `409` benturan (`PHM004`, `PHM010`, `PHM012`, `PHM013`, `PHM016`, `PHM018`, `PHM019`), `422` prasyarat |
| Idempotensi | Kunci + sidik jari pada riwayat untuk setiap perintah |
| Konkurensi | Token versi; keputusan dan verifikasi menaikkan versi |
| Transaksi | Status dan riwayat tersimpan dalam satu `SaveChanges` |
| Jejak audit | Riwayat (pelaku, waktu, status, alasan); log audit untuk keputusan, verifikasi, penolakan kewenangan, dan perubahan konfigurasi |
| Null | Permintaan tanpa `SubmittedAt` tidak pernah dianggap permintaan lama; nama pengotorisasi boleh null |
| Kebijakan global | Tidak diubah: `AccessPermissionService`, filter, dan `appsettings` tetap seperti semula |

Catatan: daftar tanpa cakupan (`scope=0`) tetap memperlihatkan semua unit kepada pemegang
`StockRequest:Read`. Ini perilaku lama yang dibutuhkan gudang. Pembatasannya diajukan sebagai
`LGS-OQ-027`.

---

## 7. Skema tampilan frontend yang diterapkan

### Validasi Permintaan Barang (`/stock-requests/approvals`)

```text
HEADER         Validasi Permintaan Barang
SUMMARY        Menunggu Persetujuan · CITO Prioritas · Sudah Disetujui · Ditolak   (ikut cakupan)
               Informasi Kewenangan (izin, unit yang dipimpin, kewenangan CITO, catatan)
FILTER         Cakupan (Unit yang Saya Pimpin | Semua) · Status · Prioritas · Tanggal · Pencarian
TABEL          Nomor · Tanggal · Unit Pemohon · Pemohon · Jumlah Item · Prioritas · Status · Aksi
DETAIL         Informasi Pemohon → Daftar Barang → Alasan → Riwayat → Informasi Kewenangan → Setujui / Tolak
```

### Verifikasi Susulan CITO (`/stock-requests/emergency-reviews`)

```text
HEADER         Verifikasi Susulan CITO
SUMMARY        Menunggu Verifikasi · Mendekati Tenggat · Terlambat · Ada Temuan   (ikut cakupan)
FILTER         Cakupan · Status Verifikasi · Tenggat · Unit · Tanggal · Pencarian
TABEL          Nomor · Unit · Waktu CITO · Pemberi Otorisasi · Tenggat · Status Verifikasi · Aksi
DETAIL         Permintaan Awal → Alasan CITO & Otorisasi Awal → Barang yang Diproses → Riwayat
               → Form Verifikasi / Catatan Temuan → Tindak Lanjut (pemberitahuan terblokir)
```

### Riwayat dan antrean gudang

Bagian atas memuat strip "Perlu Perhatian Anda":

- Menunggu Persetujuan;
- CITO Menunggu Otorisasi;
- Menunggu Verifikasi Susulan;
- Verifikasi Terlambat;
- Temuan CITO.

Hitungannya mengikuti cakupan kewenangan, dan setiap kartu membuka halaman terkait.

Tombol Setujui, Tolak, Otorisasi CITO, dan Verifikasi mengikuti penanda dari backend. Semua
halaman memakai base component Quilvian, dan menangani keadaan loading, kosong, error dengan
"Coba lagi", serta `403` lewat `AccessDeniedGate`.

---

## 8. Proses bisnis CITO

```text
Pengajuan CITO (alasan wajib)
  → Otorisasi petugas berwenang          [jabatan ditunjuk manajemen + izin eksplisit]
  → Disetujui; gudang dapat memproses     [stok berkurang hanya saat penyerahan]
  → Menunggu Verifikasi Susulan           [tenggat: belum jatuh tempo / mendekati / terlambat]
  → Pemeriksaan atasan unit               [bukan pemohon, bukan pengotorisasi]
  → Verifikasi Selesai  atau  Ada Temuan  [catatan wajib bila ada temuan; stok dan status tetap]
  → Tindak Lanjut Temuan                  [BLOCKED_MIGRATION + PIC belum ditetapkan]
  → Penyelesaian Temuan                   [BLOCKED_MIGRATION]
```

| Kebutuhan | Status |
| --- | --- |
| Identitas pemeriksa, tanggal, hasil, catatan temuan | Tersedia (riwayat + log audit) |
| Riwayat perubahan dan jejak audit | Tersedia |
| Pencegahan pemrosesan ganda | Tersedia (`PHM019` + token versi) |
| PIC tindak lanjut | **BLOCKED_MIGRATION** + PIC operasional belum ditetapkan (`LGS-OQ-025`) |
| Status tindak lanjut dan penyelesaian | **BLOCKED_MIGRATION** |

### Rancangan entity tindak lanjut (belum dibuat, butuh migration)

`PhmStockRequestEmergencyFollowUp`:

| Kolom | Tipe | Keterangan |
| --- | --- | --- |
| `Id` | uuid | |
| `StockRequestId` | uuid | FK `PhmStockRequest` |
| `DeviationHistoryId` | uuid | FK riwayat temuan; unik (satu temuan, satu tindak lanjut) |
| `PicUserId` | uuid | PIC yang ditetapkan |
| `AssignedByUserId`, `AssignedAt` | uuid, timestamptz | |
| `Status` | int | Terbuka, Ditangani, Selesai, Dibatalkan (menunggu `LGS-OQ-025`) |
| `DueAt` | timestamptz null | Tenggat tindak lanjut |
| `ResolutionNote` | varchar(1000) null | Wajib saat Selesai |
| `ResolvedByUserId`, `ResolvedAt` | uuid null, timestamptz null | |
| `Version` | int | Token konkurensi |
| Kolom audit `IdentityModel` | | |

Kontrak yang diusulkan, juga belum dibuat:

- `POST /stock-requests/{id}/emergency-follow-up/assign`: menetapkan PIC; atasan unit atau manajemen.
- `POST /stock-requests/{id}/emergency-follow-up/resolve`: PIC menyelesaikan, catatan wajib.
- `GET /stock-requests?emergencyFollowUpState=…`

Tidak satu pun dari ketiganya mengubah stok.

---

## 9. Status notifikasi (FIN-LGS-BE-007)

**BLOCKED, menunggu keputusan infrastruktur `LGS-OQ-024`.** Tidak ada notifikasi tiruan yang dibuat.

Temuan (dari membaca source):

- Satu-satunya mekanisme notifikasi adalah `HrdNotification` + `HrdNotificationService` + hub
  `/hubs/notifications`, milik modul HR. Kategorinya khusus SDM, dan endpoint bacanya
  `api/v1/self-service/hr/notifications` dengan izin `HrdNotification:ViewOwn`.
- Frontend **belum memiliki** pusat notifikasi maupun konsumen endpoint itu.
- Menulis notifikasi Logistik ke tabel milik HR berarti mengubah kepemilikan data lintas modul,
  sementara penggunanya tidak akan dapat melihat notifikasi tersebut.
- Pengingat "mendekati" dan "melewati tenggat" membutuhkan penjadwal berkala (background worker).
  Pengingat tidak boleh memverifikasi otomatis.

Spesifikasi kebutuhan, untuk dibangun setelah `LGS-OQ-024` diputuskan:

| Peristiwa | Penerima berwenang | Kunci anti-duplikat |
| --- | --- | --- |
| Permintaan baru menunggu persetujuan | Atasan unit peminta dengan izin Approve | `SRQ-SUBMIT:{id}:{penerima}` |
| Disetujui / ditolak | Pemohon | `SRQ-DECIDE:{id}:{penerima}` |
| CITO menunggu otorisasi | Pemegang jabatan CITO yang ditunjuk + izin | `SRQ-CITO:{id}:{penerima}` |
| CITO menunggu verifikasi | Atasan unit peminta | `SRQ-REVIEW:{id}:{penerima}` |
| Mendekati / melewati tenggat | Atasan unit peminta | `SRQ-DUE:{id}:{keadaan}:{penerima}` |
| Temuan memerlukan tindak lanjut | PIC tindak lanjut (`LGS-OQ-025`) | `SRQ-FINDING:{id}:{penerima}` |

Penerima dihitung dengan aturan yang sama dengan `StockRequestApprovalAuthority`. Notifikasi
dikirim setelah transaksi tersimpan, sehingga kunci yang sama tidak mengirim ulang saat terjadi
retry. Pengiriman sungguhan dimatikan di lingkungan pengujian.

---

## 10. Status filter kewenangan (FIN-LGS-BE-006)

| Ketentuan | Hasil |
| --- | --- |
| Dihitung backend dari kewenangan aktual | Ya (`ApplyAuthorityScopeAsync`) |
| Tidak mempercayai daftar unit dari klien | Ya; tidak ada parameter unit untuk cakupan |
| Beberapa unit | Semua unit yang sah tampil (diuji: atasan dua unit) |
| Tanpa penempatan | Daftar kosong + penjelasan di `approval-scope` |
| CITO sesuai kewenangan | Pemegang kewenangan CITO melihat permintaan CITO lintas unit; permintaan rutin tidak |
| Halaman, pencarian, ringkasan | Cakupan diterapkan sebelum hitungan dan halaman; ringkasan frontend mengirim cakupan yang sama |
| Data lintas unit | Tidak dibuka lewat `scope=1`; `scope=0` tetap seperti sebelumnya (`LGS-OQ-027`) |

---

## 11. Data organisasi yang dibutuhkan

Diisi pihak berwenang, bukan oleh pengembang:

| Data | Pengelola | Keadaan basis data tim (audit hanya baca, 17 November 2026) |
| --- | --- | --- |
| Tautan `MstServiceUnit.OrganizationUnitId` | HRD / administrator organisasi | 0 dari 20 unit aktif |
| Penempatan manajerial `WfpOrganizationAssignment.IsManagerialAssignment` | HRD | 0 |
| Izin `StockRequest:Approve` ke jabatan atasan unit | Administrator Akses Role | Belum terdaftar di basis data tim |
| Penunjukan jabatan CITO (`EmergencyAuthorizerPositions`) | Manajemen → operator konfigurasi | Belum |
| Izin `StockRequest:AuthorizeEmergency` ke jabatan yang ditunjuk | Administrator Akses Role | Belum |
| Tenggat verifikasi (`EmergencyReviewDueHours`, opsional `WarningHours`) | Manajemen → operator konfigurasi | Belum |
| Batas aktivasi per lingkungan | Operator, saat aktivasi | Belum |

Setelah diisi, endpoint `GET /stock-requests/approval-readiness` memperlihatkan apakah data
sudah cukup (`isReadyForApproval`, `isReadyForEmergency`, `blockers`). Kandidat identifier unit
dan jabatan ada di `evidence/06-dataset-uji-persetujuan.md`.

---

## 12. Test case dan hasil verifikasi yang benar-benar dilakukan

### Yang benar-benar dijalankan pada tahap ini

| Pemeriksaan | Hasil |
| --- | --- |
| Uji unit frontend `stock-request-approval.test.mjs` + `logistics-module-navigation.test.mjs` (node, tanpa build) | **31/31 lolos** |
| Uji frontend yang menyentuh menu, stok, dan permintaan (602 kasus) | 23 gagal, **sama persis dengan baseline**; tidak ada kegagalan baru |
| ESLint pada berkas frontend yang diubah | 0 error; warning `set-state-in-effect` adalah pola yang sudah ada di hook lain |
| Keseimbangan `{}`, `()`, `[]` pada seluruh berkas backend dan uji yang diubah | Seimbang |
| Pemeriksaan nama DbSet, properti, dan konstanta yang dirujuk | Ada di source |

### Disiapkan, belum dijalankan

Backend: 81 metode uji di `Tests/.../StockRequestApprovalTests/`, termasuk 21 metode baru:

- `OperationalSettingsUnitTests`
- `ApprovalScopeTests`

Status: **NOT RUN**. Uji ini membutuhkan tahap verifikasi `dotnet build` + `dotnet test` terpisah
lewat proyek uji terisolasi, karena proyek `PharmacyTests` tidak terkompilasi akibat berkas uji
Billing milik modul lain.

### Skenario UAT Logistik (FIN-LGS-QA-001)

Semua berstatus **NOT RUN**. Prasyarat umum:

- data organisasi pada bagian 11 sudah diisi di lingkungan uji yang disetujui;
- backend berjalan dengan `Security__Authorization__Enabled=true`.

| No | Skenario | Prasyarat | Peran | Data | Langkah | Hasil yang diharapkan | Bukti | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Pemohon membuat permintaan | Unit layanan aktif, obat aktif | Pemohon | 1 permintaan, 1 baris | Buat Permintaan → simpan | Status Draft | Tangkapan layar detail + riwayat | NOT RUN |
| 2 | Permintaan diajukan | No 1 | Pemohon | — | Ajukan Persetujuan → konfirmasi | Menunggu Persetujuan | Riwayat "Diajukan" | NOT RUN |
| 3 | Atasan melihat permintaan | Atasan unit tertaut + izin Approve | Atasan unit A | No 2 | Buka Validasi Atasan (cakupan bawaan) | Permintaan tampil; Informasi Kewenangan memuat unit A | Tangkapan layar | NOT RUN |
| 4 | Atasan menyetujui | No 3 | Atasan unit A | — | Detail → Setujui → konfirmasi | Disetujui; stok tetap | Riwayat + saldo stok sebelum/sesudah | NOT RUN |
| 5 | Atasan menolak dengan alasan | Permintaan baru | Atasan unit A | Alasan | Tolak → isi alasan → konfirmasi | Ditolak; tanpa alasan → `422` | Riwayat + respons jaringan | NOT RUN |
| 6 | Pemohon tidak bisa menyetujui sendiri | Pemohon juga atasan berizin | Pemohon | — | Buka detail permintaannya | Tanpa tombol; API → `403` | Respons `403` | NOT RUN |
| 7 | Atasan A tidak bisa memutus unit B | Dua unit tertaut | Atasan unit A | Permintaan unit B | Panggil approve | `403`; tidak tampil di cakupan A | Respons + daftar | NOT RUN |
| 8 | Gudang hanya memproses permintaan sah | Permintaan menunggu dan yang disetujui | Petugas gudang | — | Serahkan keduanya | Yang menunggu → `409 PHM016`; yang disetujui → Selesai | Respons + saldo | NOT RUN |
| 9 | CITO melalui otorisasi khusus | Jabatan CITO ditunjuk + izin | Petugas CITO | Permintaan CITO + alasan | Otorisasi CITO | Disetujui; Menunggu Verifikasi | Riwayat | NOT RUN |
| 10 | CITO masuk verifikasi susulan | No 9 | Atasan unit | — | Buka Verifikasi Susulan | Tampil dengan tenggat | Tangkapan layar | NOT RUN |
| 11 | Verifikasi tidak mengubah stok dua kali | No 9 + penyerahan | Atasan unit | Saldo awal dicatat | Benarkan atau Catat Temuan | Saldo dan mutasi tetap | Saldo sebelum/sesudah | NOT RUN |
| 12 | Temuan tercatat | No 9 | Atasan unit | Catatan temuan | Catat Temuan | "Ada Temuan"; riwayat + log audit | Riwayat + log | NOT RUN |
| 13 | SuperAdmin dibatasi | Akun SuperAdmin tanpa penempatan | SuperAdmin | — | Approve / CITO / verifikasi | `403` | Respons | NOT RUN |
| 14 | Batas aktivasi | Batas diatur sebelum permintaan lama | Petugas gudang | Permintaan sebelum/sesudah batas | Serahkan | Lama boleh; baru `409` | Respons + riwayat | NOT RUN |
| 15 | Filter sesuai unit | Atasan dua unit + atasan satu unit | Keduanya | Permintaan beberapa unit | Buka Validasi Atasan | Setiap atasan hanya melihat unitnya | Tangkapan layar | NOT RUN |
| 16 | Notifikasi tidak bocor lintas unit | **Terblokir `LGS-OQ-024`** | — | — | — | — | — | BLOCKED |

---

## 13. BLOCKED_DATA

1. Tautan unit layanan ke unit organisasi (0/20 di basis data tim).
2. Penempatan manajerial aktif (0).
3. Izin `StockRequest:Approve` ke jabatan atasan unit.
4. Penunjukan jabatan CITO oleh manajemen. Jabatan bernama "Kepala Instalasi" maupun "Penanggung Jawab Gudang" tidak ditemukan di basis data tim.
5. Izin `StockRequest:AuthorizeEmergency` ke jabatan yang ditunjuk.
6. Nilai tenggat verifikasi susulan.
7. PIC operasional tindak lanjut temuan.
8. Nilai batas aktivasi per lingkungan, ditentukan saat aktivasi.

## 14. BLOCKED_MIGRATION

1. `PhmStockRequestEmergencyFollowUp`: tindak lanjut temuan CITO (rancangan di bagian 8).
2. Tabel aktivasi batas persetujuan, supaya perubahan surut dapat ditolak secara teknis.
3. Tabel notifikasi Logistik, bila `LGS-OQ-024` memilih penyimpanan sendiri.
4. Master penunjukan jabatan CITO dengan jejak perubahan, bila `LGS-OQ-026` memilih master data.

## 15. BLOCKED_SANDBOX (FIN-LGS-QA-002)

Uji konkurensi PostgreSQL belum dapat dijalankan karena belum ada basis data sandbox yang
disetujui. Uji tidak boleh menulis ke basis data tim maupun produksi.

Prosedur yang disiapkan:

1. **Siapkan sandbox.** Salin skema ke basis data sandbox yang disetujui, lalu isi data organisasi uji (bagian 11) hanya di sana.
2. **Dua persetujuan bersamaan.** Dua sesi API (atasan A1 dan A2 pada unit yang sama) mengirim `approve` untuk satu permintaan dengan `expectedVersion` yang sama secara serentak, misalnya lewat skrip paralel. Harapan: satu `200`, satu `409 PHM012` atau `PHM018`, dan satu baris riwayat Disetujui.
3. **Persetujuan bersamaan dengan penolakan.** Harapan sama: hanya satu keputusan sah.
4. **Dua otorisasi CITO.** Dua petugas CITO serentak. Harapan: satu sah.
5. **Dua verifikasi susulan.** Dua atasan serentak. Harapan: satu sah; yang lain `409`.
6. **Permintaan duplikat dan retry.** Kirim ulang kunci idempotensi yang sama. Harapan: hasil sama tanpa baris baru. Kunci sama dengan isi berbeda: `409 PHM013`.
7. **Bukti.** Respons HTTP, isi `PhmStockRequestHistory`, dan `Version` akhir.
8. **Pembersihan.** Buang sandbox setelah uji.

Di SQLite, **logika penjaga** yang sama (token versi `Version` dan penolakan `PHM012`) diuji pada
17 November 2026 dan lolos (`ConcurrencyTests`, dengan keputusan pesaing yang disisipkan tepat
sebelum simpanan). Itu **bukan** bukti perilaku PostgreSQL: SQLite menyerialkan penulisan pada satu
koneksi, sedangkan PostgreSQL menjalankan dua transaksi yang benar-benar bersamaan. Perilaku
PostgreSQL hanya terbukti lewat prosedur di atas. *(Dikoreksi pada FINAL VERIFICATION.)*

## 16. Open Question baru

| ID | Pertanyaan |
| --- | --- |
| `LGS-OQ-024` | Infrastruktur notifikasi Logistik (pakai `HrdNotification` sebagai platform, tabel sendiri, atau tunda) |
| `LGS-OQ-025` | PIC, status, tenggat, dan penutup tindak lanjut temuan CITO |
| `LGS-OQ-026` | Penunjukan jabatan CITO: cukup lewat konfigurasi, atau master data dengan jejak perubahan |
| `LGS-OQ-027` | Pembatasan baca lintas unit pada daftar tanpa cakupan |
| `LGS-OQ-028` | Siapa yang diberi izin `StockRequest:ViewApprovalReadiness` |

Rinciannya ada di `00-interview-decisions.md` r6, bagian 8. Semuanya OPEN; tidak satu pun
dianggap disetujui.

## 17. Langkah UAT berikutnya

1. **Verifikasi build dan uji terisolasi, dengan izin terpisah.** Jalankan `dotnet build -p:SkipMigrationMetadata=true`, lalu 81 metode uji `StockRequestApprovalTests` lewat proyek uji terisolasi. Jalankan juga ESLint dan uji unit frontend.
2. **Review dan commit**, dengan izin terpisah.
3. **HRD dan manajemen mengisi data bagian 11** di lingkungan uji yang disetujui.
4. **Periksa `GET /approval-readiness`** sampai `isReadyForApproval` dan `isReadyForEmergency` bernilai benar.
5. **Jalankan skenario UAT 1–15** dengan `Security__Authorization__Enabled=true`.
6. **Jalankan prosedur konkurensi** di sandbox PostgreSQL.
7. **Putuskan `LGS-OQ-024` dan `025`**, lalu bangun notifikasi dan tindak lanjut.

## 18. Kesimpulan kesiapan fitur Logistik

| Bagian | Kesiapan |
| --- | --- |
| Persetujuan atasan unit, penolakan, larangan memutus sendiri, keputusan ganda | Siap secara source; belum dapat dipakai sampai data organisasi diisi |
| "Unit yang Saya Pimpin" dan Informasi Kewenangan | Siap secara source; uji backend belum dijalankan |
| Otorisasi CITO dengan penunjukan jabatan | Siap secara source; menunggu penunjukan dan izin |
| Verifikasi susulan CITO | Siap secara source (sudah di integration); tenggat menunggu nilai konfigurasi |
| Tindak lanjut temuan | Belum siap (`BLOCKED_MIGRATION`, PIC belum ditetapkan) |
| Notifikasi | Belum siap (`LGS-OQ-024`) |
| Modul Logistik secara keseluruhan | Belum selesai. `LGS-OQ-001` sampai `019` (BLM, Goods Receipt ke stok, valuasi, stock opname, dan lain-lain) masih terbuka; 24 dari 67 entri menu yang sudah tertaut |

**Tidak ada bagian yang berstatus RUNTIME_VERIFIED pada tahap ini.**

---

# VERIFIKASI SOURCE PASCA-FINISHING

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026 |
| Lingkup | FIN-LGS-VER-001 sampai VER-007, khusus modul Logistik |
| Batas | Pemeriksaan statis, perbaikan source, uji unit frontend (node, tanpa build), dan query basis data **hanya baca**. **Tidak** menjalankan `dotnet build`, `dotnet test`, `npm run build/dev`, runtime, migrasi, perubahan basis data, commit, push, maupun merge |

Tiga jenis bukti dibedakan sepanjang bagian ini:

- **Implementasi source**: kode yang ditulis atau diubah.
- **Verifikasi statis**: pembacaan kode, ESLint, keseimbangan kurung, pencocokan kontrak, uji unit frontend tanpa build.
- **Pengujian runtime**: build backend, test runner backend, aplikasi berjalan, dan browser. **Belum ada satu pun pada tahap ini.**

## V1. Kondisi branch dan working tree

| Repo | Branch lokal | Integration | Selisih commit | Keterangan |
| --- | --- | --- | --- | --- |
| Backend | `Ikbal` @ `33dd4eb1` (= `origin/Ikbal`) | `ffedc61c` (merge PR #356) | Integration +1 (merge commit saja) | Integration tidak mengubah berkas Logistik sejak HEAD; tidak ada konflik |
| Frontend | `Ikbalv2` @ `0aae59c80` | `0aae59c80` | 0 | Sama |

Seluruh perubahan pada bagian ini **belum di-commit**. Berkas di luar Logistik yang ada di working
tree bukan milik pekerjaan ini dan tidak disentuh:

- backend: `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/`;
- frontend: `test-results/.last-run.json` dan 16 berkas `tests/e2e/zz-*`.

## V2. Berkas backend aktual

Laporan sebelumnya menyebut **12** berkas. Saat dicek, jumlahnya **13**: laporan master ini
sendiri belum terhitung. Setelah verifikasi, jumlahnya menjadi **15** karena dua berkas uji baru
(`Infrastructure/CatatanLogUji.cs` dan `StockRequestApprovalTests/RobustnessAndAuditTests.cs`).
Daftar lengkapnya ada di V10.

## V3. Berkas frontend aktual

Jumlahnya **17**, cocok dengan laporan sebelumnya. Isinya 10 berkas diubah dan 7 berkas baru,
sama dengan daftar pada bagian 4 dan V10. Pada tahap verifikasi, empat berkas disunting lagi:

- `stock-request-constant.jsx`
- `stock-request-list-view.jsx`
- `stock-request-emergency-review-view.jsx`
- `tests/unit/stock-request-approval.test.mjs`

## V4. Perubahan source pada tahap verifikasi

| No | Temuan | Severity | Perbaikan | Berkas |
| --- | --- | --- | --- | --- |
| 1 | Jabatan CITO yang ditunjuk **lewat id** tetap memberi kewenangan walau jabatannya sudah dinonaktifkan. Penunjukan lewat kode sudah memeriksa status aktif | MEDIUM | `ResolveDesignatedPositionIdsAsync` kini mencocokkan id maupun kode ke `MstPosition` yang aktif dan tidak dihapus, dalam satu query | `StockRequestApprovalAuthority.cs` |
| 2 | Cakupan "Unit yang Saya Pimpin" untuk pemegang kewenangan CITO ikut memperlihatkan **draf** CITO unit lain, padahal draf belum diajukan dan belum memerlukan otorisasi | MEDIUM | Cabang CITO lintas unit mengecualikan status Draft | `StockRequestService.cs` |
| 3 | Log audit permintaan stok mencatat **id permintaan sebagai identitas pelaku**: `LoggerService` mengambil `UserId` dari properti data bernama `Id`. Berlaku pada 6 titik: buat, ajukan, keputusan, verifikasi/temuan, serah, penolakan kewenangan. Riwayat di basis data tetap benar | MEDIUM | Data audit memakai `StockRequestId` dan `UserId = actorUserId` yang eksplisit. `LoggerService` milik platform tidak diubah | `StockRequestService.cs` |
| 4 | Laporan kesiapan dapat menyatakan **siap** padahal izin Approve diberikan kepada jabatan yang bukan atasan unit | LOW | Atasan dicocokkan dengan akun aktif yang memegang izin Approve eksplisit. Ditambahkan `mappedServiceUnitsWithAuthorizedSupervisor`, `units[].authorizedSupervisors`, dan blocker baru. `isReadyForApproval` kini memakai hitungan ini | `StockRequestApprovalAuthority.cs` (`GetUsersWithExplicitGrantAsync`), `StockRequestService.cs`, `StockRequestDtos.cs` |
| 5 | Pilihan **Cakupan** di layar memakai placeholder "Unit yang Saya Pimpin", padahal nilai kosongnya berarti **semua unit** | LOW | Placeholder kini "Semua unit yang dapat saya lihat"; pilihan yang tersedia hanya "Unit yang Saya Pimpin" | `stock-request-constant.jsx`, `stock-request-list-view.jsx`, `stock-request-emergency-review-view.jsx` |

Tidak ada perubahan pada modul lain, konfigurasi keamanan global, `appsettings`, migrasi, maupun
basis data.

## V5. Pemeriksaan metode uji backend

Jumlahnya kini **91 metode** (81 + 10 baru) di 8 kelas, ditambah fixture dan logger penangkap.
Status: **NOT_RUN**. Jumlah metode tidak sama dengan jumlah uji yang lolos; kasus `[Theory]`
dihitung per kombinasi saat dijalankan.

| Kategori wajib | Cakupan | Kelas |
| --- | --- | --- |
| Hak akses persetujuan | Ada | `AuthorizationTests` |
| Larangan memutus sendiri | Ada (pemohon, pembuat, pemohon yang juga atasan, pemohon pemegang izin CITO) | `AuthorizationTests`, `EmergencyReviewTests` |
| Akses lintas unit | Ada | `AuthorizationTests`, `ApprovalScopeTests` |
| Penempatan manajerial tidak aktif | Ada (berakhir; profil tenaga kerja nonaktif) | `AuthorizationTests` |
| Unit organisasi tidak aktif | **Ditambahkan** (nonaktif; masa berlaku habis) | `RobustnessAndAuditTests` |
| Izin tidak diberikan | Ada | `AuthorizationTests`, `ApprovalScopeTests` |
| Otorisasi CITO | Ada, termasuk penunjukan jabatan (kosong, kode, tidak ditunjuk) dan **jabatan dinonaktifkan** (ditambahkan) | `AuthorizationTests`, `ApprovalScopeTests`, `RobustnessAndAuditTests` |
| SuperAdmin tanpa kewenangan | Ada (setuju, tolak, CITO, verifikasi, cakupan) | `AuthorizationTests`, `EmergencyReviewTests`, `ApprovalScopeTests` |
| Persetujuan ganda | Ada | `StatusTransitionTests`, `EmergencyReviewTests` |
| Transisi status | Ada | `StatusTransitionTests` |
| Batas aktivasi | Ada (unit dan integrasi) | `CutoverUnitTests`, `StatusTransitionTests` |
| Filter cakupan | Ada, termasuk **draf CITO lintas unit** (ditambahkan) | `ApprovalScopeTests`, `RobustnessAndAuditTests` |
| Verifikasi susulan | Ada | `EmergencyReviewTests` |
| Tenggat | Ada (unit dan daftar) | `OperationalSettingsUnitTests`, `ApprovalScopeTests` |
| Jejak audit | Riwayat sudah diuji. **Log audit ditambahkan** lewat `CatatanLogUji`: penolakan kewenangan (Warning, identitas pelaku), keputusan (Information), temuan (Audit + Warning) | `RobustnessAndAuditTests` |
| Kegagalan dependensi | **Ditambahkan sebagian**: akun nonaktif, unit organisasi nonaktif/berakhir, jabatan nonaktif. Kegagalan basis data atau koneksi tidak disimulasikan | `RobustnessAndAuditTests` |
| Data null atau tidak sah | Ada (batas kosong/tidak sah, permintaan tanpa `SubmittedAt`, hasil verifikasi tidak dikenal, alasan kosong) + **id yang tidak ada** (ditambahkan) | beberapa kelas |

Fixture: SQLite dalam memori per uji, dengan data sintetis berawalan "Uji". Tidak ada koneksi ke
basis data tim maupun lokal, dan tidak ada `WebApplicationFactory`.

Proyek uji terisolasi di scratchpad (di luar repo) sudah ditambah `CatatanLogUji.cs`. Proyek ini
dipakai karena proyek `PharmacyTests` tidak terkompilasi akibat empat berkas uji Billing milik modul
lain.

Uji yang disesuaikan: `Jabatan_berizin_yang_tidak_ditunjuk_tidak_berwenang_CITO` kini memakai
jabatan yang benar-benar ada di master jabatan uji. Tanpa itu, uji lolos karena alasan lain
(penunjukan dianggap kosong), bukan karena jabatannya tidak ditunjuk.

## V6. Hasil audit keamanan

| Severity | Temuan | Status |
| --- | --- | --- |
| CRITICAL | Tidak ditemukan | — |
| HIGH | Tidak ditemukan | — |
| MEDIUM | Jabatan CITO nonaktif tetap berwenang (V4 no 1) | Diperbaiki di source |
| MEDIUM | Draf CITO lintas unit terlihat (V4 no 2) | Diperbaiki di source |
| MEDIUM | Identitas pelaku salah di log audit (V4 no 3) | Diperbaiki di source |
| MEDIUM | Daftar tanpa cakupan (`scope=0`) dan `GET /{id}` memperlihatkan permintaan semua unit kepada pemegang `StockRequest:Read`. Perilaku lama, dibutuhkan gudang | **Terbuka, butuh keputusan** `LGS-OQ-027` |
| LOW | Laporan kesiapan dapat menyatakan siap secara keliru (V4 no 4) | Diperbaiki di source |
| LOW | Placeholder cakupan menyesatkan (V4 no 5) | Diperbaiki di source |
| LOW | Dengan `Security:Authorization:Enabled=false` (Development), endpoint kesiapan dapat dibuka siapa pun yang login. Isinya jumlah dan nama unit, tanpa identitas pegawai | Diterima; izin produksi menunggu `LGS-OQ-028` |
| LOW | Perubahan penunjukan jabatan CITO dan tenggat hanya dikendalikan lewat manajemen konfigurasi; sistem mencatatnya di log tetapi tidak menolaknya | Terbuka, `LGS-OQ-026` |

Hasil pemeriksaan butir lain:

- **Persetujuan tanpa izin, CITO tanpa otorisasi, SuperAdmin:** ditutup di service, tidak bergantung filter controller.
- **Race condition dan aksi ganda:** dijaga token versi dan kunci idempotensi.
- **Perubahan stok tidak sah:** tidak ada mutasi stok pada keputusan maupun verifikasi.
- **Salah klasifikasi data lama:** permintaan tanpa `SubmittedAt` tidak dianggap lama; batas kosong, tidak sah, atau di masa depan tidak membuka jalan pintas.
- **Konfigurasi keamanan:** `appsettings` tidak berubah.
- **Pesan error di layar:** hanya memuat nama unit permintaan yang memang boleh dibaca pengguna.

## V7. Konsistensi API frontend dan backend

Diperiksa secara terprogram, dengan membandingkan kode frontend terhadap properti DTO backend.

| Pemeriksaan | Hasil |
| --- | --- |
| Parameter daftar frontend vs `StockRequestPagedQuery` | Semua ada (termasuk `scope`, `emergencyReviewDueState`, `emergencyReviewState`, `fulfillableOnly`, `orderByPriority`) |
| Field yang dibaca layar vs DTO ringkasan, detail, cakupan, riwayat, dan baris | Semua ada. Temuan sisa adalah positif palsu: variabel lokal, form pembuatan, dan opsi dropdown |
| Endpoint yang dipanggil frontend vs route controller | Semua ada: `GET /`, `GET /{id}`, `POST /{id}/submit\|cancel\|approve\|reject\|authorize-emergency\|verify-emergency\|fulfill`, `GET /approval-scope` |
| `GET /approval-readiness` | Ada di backend; **belum ada halaman frontend** (izinnya belum diberikan, `LGS-OQ-028`). "Informasi Kesiapan Approval" di layar: NOT_STARTED |
| Role di-hardcode / jalan pintas peran di layar | Tidak ada |
| Data dummy | Tidak ada |
| Nilai enum | Sama dengan backend (dikunci uji di kedua sisi) |

Hasil uji frontend yang dijalankan:

- `stock-request-approval.test.mjs` + `logistics-module-navigation.test.mjs`: **32/32 lolos**.
- 603 uji yang menyentuh menu, stok, dan permintaan: **23 gagal, sama persis dengan baseline**.
- ESLint pada berkas Logistik: **0 error**, 4 warning dari pola yang sudah ada.

## V8. Pemeriksaan data organisasi (hanya baca, 18 November 2026)

| Ukuran | `QuilvianNewDevMaster` (tim) | `QuilvianNewDevIkbalFr` (lokal) |
| --- | --- | --- |
| Unit layanan aktif | 20 | 5 |
| Unit dengan `OrganizationUnitId` | **0** | **0** |
| Unit organisasi aktif | 5 | 0 |
| Penempatan manajerial aktif dan berlaku | **0** | **0** |
| Izin `StockRequest:Approve` terdaftar / diberikan | Terdaftar / **0 jabatan** | Terdaftar / **0 jabatan** |
| Izin `StockRequest:AuthorizeEmergency` terdaftar / diberikan | Terdaftar / **0 jabatan** | Terdaftar / **0 jabatan** |
| Izin `StockRequest:ViewApprovalReadiness` | Belum terdaftar (kode baru belum dijalankan) | Belum terdaftar |
| Permintaan berstatus Submitted | 0 | 0 |
| Konfigurasi jabatan CITO, tenggat, batas aktivasi | Tidak ada di `appsettings.json` maupun `appsettings.Development.json` | sama |

Perubahan sejak audit 17 November: di basis data tim, izin `Approve` dan `AuthorizeEmergency`
**kini sudah terdaftar**. Kemungkinan backend versi PR #356 sudah dijalankan di sana. Keduanya
masih belum diberikan kepada jabatan mana pun.

Kebutuhan pengisian untuk administrator berwenang:

1. **HRD / administrator organisasi:** tautkan unit layanan ke unit organisasi yang benar. Kandidat ada di `evidence/06-dataset-uji-persetujuan.md`.
2. **HRD:** tandai penempatan manajerial yang aktif untuk atasan setiap unit.
3. **Administrator Akses Role:** beri `StockRequest:Approve` kepada departemen × jabatan atasan unit.
4. **Manajemen:** tetapkan jabatan CITO. **Operator konfigurasi:** isi `PharmacyManagement__StockRequest__EmergencyAuthorizerPositions`. **Administrator Akses Role:** beri `StockRequest:AuthorizeEmergency` kepada jabatan itu.
5. **Manajemen → operator konfigurasi:** tenggat `EmergencyReviewDueHours` dan, bila perlu, `EmergencyReviewWarningHours`.
6. **Operator, saat aktivasi:** batas aktivasi; boleh kosong selama tidak ada permintaan Submitted.
7. **Administrator Akses Role:** setelah `LGS-OQ-028` diputuskan, beri `StockRequest:ViewApprovalReadiness` untuk memantau kesiapan.

## V9. Status kesiapan UAT

| Bagian | Status |
| --- | --- |
| Source persetujuan, CITO, verifikasi, cakupan, tenggat, kesiapan | COMPLETED_SOURCE |
| Uji otomatis backend (91 metode) | NOT_RUN |
| Data organisasi dan izin | BLOCKED_DATA |
| Tindak lanjut temuan | BLOCKED_MIGRATION |
| Notifikasi | PARTIAL (strip hitungan) / terblokir `LGS-OQ-024` |
| Konkurensi PostgreSQL | BLOCKED_SANDBOX |
| Skenario UAT 1–16 | NOT_RUN |
| Halaman kesiapan di frontend | NOT_STARTED |

## V10. Berkas yang aman untuk di-commit

> **Digantikan** oleh *FINAL VERIFICATION & RUNTIME READINESS*, butir 8. Daftar di bawah adalah
> keadaan 18 November sebelum verifikasi akhir.

**Backend (15 berkas):**

```text
Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs
Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs
Areas/HealthServices/PharmacyManagement/Enums/StockRequestEnums.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalAuthority.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestOperationalSettings.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs
Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/CatatanLogUji.cs
Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApprovalScopeTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/OperationalSettingsUnitTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/RobustnessAndAuditTests.cs
docs/module-blueprints/logistik/00-interview-decisions.md
docs/module-blueprints/logistik/blueprint-manifest.md
docs/module-blueprints/logistik/evidence/05-aktivasi-batas-persetujuan.md
docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md
```

**Frontend (17 berkas):**

```text
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-detail-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-emergency-review-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-list-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-attention-strip.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-authority-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-authority-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-authorization-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-follow-up-notice.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-review-panel.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-summary-cards.jsx
src/lib/constants/health-services/pharmacy-management/stock-request-constant.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-scope.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-summary.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx
src/lib/services/health-services/pharmacy-management/stock-request.service.js
src/utils/health-services/pharmacy-management/stock-request-approval-utils.js
tests/unit/stock-request-approval.test.mjs
```

**Tidak boleh ikut:**

- backend: `SeedData/ICD10/*`, `Storage/uploads/*`;
- frontend: `test-results/.last-run.json`, `tests/e2e/zz-*`.

Pemindaian pada daftar di atas tidak menemukan password, host, connection string, token, kunci
privat, maupun berkas sementara atau hasil uji.

## V11. Saran pesan commit

> **Digantikan** oleh *FINAL VERIFICATION & RUNTIME READINESS*, butir 8.

Backend:

```text
feat(logistik): cakupan "Unit yang Saya Pimpin", penunjukan jabatan CITO, tenggat verifikasi

- GET /stock-requests?scope=1 dihitung dari kewenangan aktual (unit dipimpin + izin Approve
  eksplisit; CITO lintas unit tanpa draf); GET approval-scope dan approval-readiness.
- Otorisasi CITO wajib jabatan aktif yang ditunjuk manajemen + izin eksplisit
  (PharmacyManagement:StockRequest:EmergencyAuthorizerPositions).
- Tenggat verifikasi susulan dapat dikonfigurasi (UTC, tanpa bawaan) + filter keadaan tenggat.
- Kesiapan mencocokkan atasan unit dengan izin Approve-nya.
- Log audit permintaan stok mencatat identitas pelaku yang benar (StockRequestId/UserId).
- Uji: 91 metode StockRequestApprovalTests.
- Blueprint logistik r6; laporan FIN-LGS-LAPORAN-FINISHING.md.

Tanpa entity, tabel, maupun migration baru.
```

Frontend:

```text
feat(logistik): Validasi Atasan dan Verifikasi Susulan CITO sesuai skema finishing

- Validasi Permintaan Barang: cakupan "Unit yang Saya Pimpin" dari backend, ringkasan 4 kartu
  sekonsisten tabel, Informasi Kewenangan, kolom sesuai skema.
- Verifikasi Susulan CITO: ringkasan, kolom Waktu CITO/Pemberi Otorisasi/Tenggat, detail
  berurutan sampai Tindak Lanjut (pemberitahuan terblokir, tanpa formulir tiruan).
- Strip "Perlu Perhatian Anda" dari hitungan backend.
- Placeholder Cakupan tidak lagi menyesatkan.

Uji: tests/unit/stock-request-approval.test.mjs.
```

Kalimat jumlah uji yang lolos ditambahkan setelah tahap V13 benar-benar dijalankan.

## V12. Blocker yang masih tersisa

| Jenis | Blocker |
| --- | --- |
| BLOCKED_DATA | Tautan unit 0/20; penempatan manajerial 0; izin Approve dan AuthorizeEmergency 0 jabatan; jabatan CITO belum ditunjuk; tenggat belum ditetapkan; PIC tindak lanjut belum ditetapkan |
| BLOCKED_MIGRATION | Entity tindak lanjut temuan; tabel aktivasi batas; (bila dipilih) tabel notifikasi dan master penunjukan CITO |
| BLOCKED_SANDBOX | Uji konkurensi PostgreSQL |
| Keputusan terbuka | `LGS-OQ-024` notifikasi; `025` tindak lanjut; `026` penunjukan via konfigurasi vs master; `027` baca lintas unit; `028` izin kesiapan |
| Di luar Logistik | Proyek `PharmacyTests` tidak terkompilasi karena 4 berkas uji Billing modul lain |

## V13. Tahap verifikasi runtime yang diperlukan

Setiap tahap di bawah memerlukan izin terpisah.

1. **Build dan uji backend terisolasi.** Jalankan `dotnet build -p:SkipMigrationMetadata=true` lewat proyek uji terisolasi di scratchpad, lalu `dotnet test --no-build --filter "FullyQualifiedName~StockRequestApprovalTests"`. Harapannya seluruh kasus dari 91 metode lolos. Bila ada yang gagal, perbaiki sebelum commit.
2. **Uji unit frontend dan ESLint** dijalankan ulang setelah review.
3. **Commit** dengan daftar V10, lalu push dan PR sesuai prosedur tim.
4. **Aplikasi berjalan** dengan `Security__Authorization__Enabled=true` pada lingkungan uji yang disetujui. Periksa `GET /approval-readiness` sampai siap.
5. **Skenario UAT 1–15** di browser, dengan bukti tangkapan layar, respons jaringan, dan saldo stok.
6. **Prosedur konkurensi** di sandbox PostgreSQL.

**Tidak ada bagian yang berstatus RUNTIME_VERIFIED.**

---

# FINAL VERIFICATION & RUNTIME READINESS

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026 |
| Lingkup | FIN-LGS-VER-008 sampai VER-012, khusus modul Logistik |
| Batas | Pemeriksaan dan perbaikan source, uji unit frontend dengan node (tanpa build), ESLint. **Tidak** menjalankan `dotnet build`, `dotnet test`, `npm run build/dev`, server, migrasi, perubahan atau query basis data, commit, push, merge, maupun deployment |
| Base path API | `/api/v1/health-services/pharmacy-management/stock-requests` |

## Ringkasan status

| Kategori | Isi |
| --- | --- |
| **Source implemented** | Seluruh fitur FIN dan VER, ditambah tiga perbaikan tahap ini (butir 2) |
| **Static verified** | Pembacaan kode 93 metode uji terhadap source; keseimbangan kurung 12 berkas C#; pencocokan kontrak frontend–backend; pemindaian rahasia, hardcode, dan jalan pintas izin; uji unit frontend 32/32; ESLint 0 error |
| **Runtime not tested** | ~~93 metode / 107 kasus uji backend~~ — **sudah dijalankan, lihat *HASIL EKSEKUSI VERIFIKASI*: 108/108 lolos**; aplikasi berjalan dengan pengguna sungguhan; skenario UAT |
| Bukti runtime historis | 64/64 kasus lolos pada 17 November 2026 (15.56 dan 16.00) untuk snapshot commit `b50034c5`, yang masuk PR #356. **Tidak berlaku** untuk perubahan lokal sesudahnya |
| Build dan start (atas permintaan, 18 November 11.39) | Build **proyek utama** (`-p:SkipMigrationMetadata=true`): **0 error**, sehingga seluruh source lokal termasuk perubahan tahap ini ter-compile. Proyek uji belum di-build dan uji belum dijalankan. Backend berjalan di DB lokal `QuilvianNewDevIkbal*`, dengan `Seeders__ContinueOnFailure=true` hanya untuk proses itu: 6 seeder modul lain dilewati karena migrasinya belum diterapkan di DB lokal. Smoke test: `GET /`, `/approval-scope`, dan `/approval-readiness` tanpa login → 401; halaman `/stock-requests/approvals` → 200; 0 galat StockRequest di log. Ini **bukan** UAT |
| **BLOCKED_DECISION** | `LGS-OQ-027` cakupan baca lintas unit; `LGS-OQ-029` cakupan perintah tulis (baru); `LGS-OQ-024`, `025`, `026`, `028` |
| **BLOCKED_DATA** | Tautan unit layanan ke unit organisasi 0/20; penempatan manajerial 0; izin Approve dan AuthorizeEmergency 0 jabatan; jabatan CITO, tenggat, dan batas aktivasi belum diisi |
| **BLOCKED_MIGRATION** | Entity tindak lanjut temuan CITO; tabel aktivasi batas; tabel notifikasi dan master penunjukan CITO bila dipilih |
| **BLOCKED_SANDBOX** | Uji konkurensi PostgreSQL |

## 1. Hasil pemeriksaan metode uji backend

Seluruh metode dibaca satu per satu dan setiap asersi dicocokkan dengan source. Contohnya, angka
kesiapan unit A (5 atasan aktif, 4 berizin) dihitung ulang dari data harness dan dari query
`GetApprovalReadinessAsync`. Jumlahnya kini **93 metode / 107 kasus** (sebelumnya 91 metode).

| Kelas | Metode | Kasus | Perubahan pada tahap ini |
| --- | --- | --- | --- |
| `ApiContractTests` | 4 | 4 | Angka `Priority` 1/2, `EmergencyReviewDueState` 0–4, dan `StockRequestListScope` 0/1 kini dikunci. Sebelumnya hanya status, review state, dan outcome yang dikunci, padahal frontend membaca ketiganya |
| `ApprovalScopeTests` | 15 | 17 | — |
| `AuthorizationTests` | 13 | 13 | — |
| `ConcurrencyTests` | 6 | 6 | — |
| `CutoverUnitTests` | 8 | 11 | — |
| `EmergencyReviewTests` | 12 | 12 | — |
| `OperationalSettingsUnitTests` | 6 | 14 | Dua kasus luapan nilai tenggat (`1000000`, `1e300`) |
| `RobustnessAndAuditTests` | 12 | 12 | +2 metode; dua uji audit diperketat |
| `StatusTransitionTests` | 17 | 18 | — |

| Pemeriksaan VER-008 | Hasil |
| --- | --- |
| Memakai source Logistik terbaru | Ya. Proyek terisolasi merujuk `C:\tmp\phm\QuilvianSystemBackend.csproj` (working tree, termasuk perubahan belum commit) dan meng-compile berkas uji langsung dari folder repo (`StockRequestApprovalTests\*.cs`, harness, `TestDatabase`, `CatatanLogUji`). Tidak ada salinan yang bisa basi |
| Tidak terkoneksi ke basis data tim atau produksi | Ya. `TestDatabase` memakai SQLite `Filename=:memory:` per uji. Tidak ada connection string, `appsettings`, maupun `WebApplicationFactory`; pencarian `Host=`, `Server=`, `UseNpgsql`, dan `ConnectionStrings` di folder uji menghasilkan 0 temuan |
| Tidak mengubah basis data operasional | Ya. Basis data uji hilang saat koneksi ditutup. `LoggerService` diberi logger penangkap atau `NullLogger`, sehingga tidak menulis berkas maupun layanan log |
| Dependency dan konfigurasi valid | Paket sama dengan `PharmacyTests` (`Microsoft.EntityFrameworkCore.Sqlite` 9.0.18, `xunit` 2.9.2, `Microsoft.NET.Test.Sdk` 17.12.0). `SkipMigrationMetadata=true`. Konfigurasi per uji lewat `AddInMemoryCollection`. Build proyek ini pada 17 November: 0 error |
| Otorisasi: SuperAdmin | `Kasus4`, `Kasus5`, `Kasus14` (verifikasi), dan teori cakupan `superadmin`. SuperAdmin tanpa penempatan selalu ditolak di domain |
| Otorisasi: izin | `Kasus3`, `Cakupan_kosong...`, `Jabatan_berizin_yang_tidak_ditunjuk...`, dan uji baru **controller izin nonaktif** |
| Otorisasi: kewenangan unit | `Kasus2`, staf non-manajerial, penempatan berakhir, profil nonaktif, unit belum tertaut, unit organisasi nonaktif atau berakhir, akun nonaktif |
| Klaim konkurensi | Kelas `ConcurrencyTests` sudah menyatakan batas SQLite. **Laporan bagian 15 mengklaim berlebih** ("perilaku yang sama sudah diuji") dan sudah dikoreksi: yang diuji adalah logika token versi, bukan dua transaksi PostgreSQL bersamaan |
| Identitas pelaku pada audit | **Sebelumnya kurang.** Hanya entri penolakan yang diperiksa, dan pemeriksaannya mencari teks di seluruh pesan. Kini kolom `UserId="..."` dibandingkan persis lewat `CatatanLogUji.PelakuPada` untuk `AuthorityDenied`, `Approve`, `Submit`, `Fulfill`, `VerifyEmergency`, dan `EmergencyDeviation`, ditambah asersi bahwa id permintaan tidak pernah tercatat sebagai pelaku. Reject dan AuthorizeEmergency memakai objek data yang sama dengan Approve (`DecideAsync`). **Create belum diuji**, karena membutuhkan master obat yang tidak disediakan harness |

Uji yang ditambahkan:

- `Pengajuan_dan_penyerahan_mencatat_pelaku_bukan_id_permintaan`;
- `Izin_pada_controller_nonaktif_tidak_berlaku_dan_tidak_dihitung_kesiapan`. Uji ini gagal pada kode sebelum tahap ini, karena laporan kesiapan masih menghitung 1 izin.

Uji yang diperketat:

- `Keputusan_dan_penolakan_kewenangan_tercatat_di_log_audit`;
- `Temuan_penyimpangan_tercatat_sebagai_audit_dan_peringatan`;
- `Kesiapan_tidak_menyatakan_siap_bila_atasan_tidak_memegang_izin` (mengikuti pesan blocker baru).

## 2. Perubahan source terbaru

| No | Temuan | Severity | Perbaikan | Berkas |
| --- | --- | --- | --- | --- |
| 1 | **Konfigurasi tenggat yang terlalu besar menjatuhkan seluruh endpoint permintaan stok.** `TimeSpan.FromHours` melempar `OverflowException` di atas ±2,56×10⁸ jam (misalnya `1e300`). Tenggat dibaca di konstruktor `StockRequestService`, sehingga semua endpoint ikut gagal 500. Nilai di bawah batas itu tetapi masih sangat besar juga dapat menggagalkan filter tenggat (`now - tenggat` keluar dari rentang `DateTime`) | MEDIUM (ketersediaan) | Batas teknis `MaxConfigurableHours` = 100 tahun. Nilai di atasnya diperlakukan sama dengan nilai tidak sah lain: tenggat dianggap belum dikonfigurasi. Batas ini bukan kebijakan; tenggat operasional tetap keputusan rumah sakit | `StockRequestOperationalSettings.cs` |
| 2 | **Tiga salinan penyaring izin, satu sudah menyimpang.** `CountStockRequestGrantsAsync` (laporan kesiapan) tidak memeriksa controller access yang nonaktif atau terhapus, maupun `IsSystemOnly`. Akibatnya kesiapan dapat melaporkan izin "sudah diberikan" padahal penegakan menolaknya | LOW (konsistensi) | Satu penyaring: `FindGrantableActionAsync` (aksi yang dapat diberikan) + `GrantingPlacements` (penempatan yang memegangnya) + `CountPolicyGrantsAsync`. Dipakai `HasExplicitGrantAsync`, `GetUsersWithExplicitGrantAsync`, dan laporan kesiapan. Salinan di service dihapus | `StockRequestApprovalAuthority.cs`, `StockRequestService.cs` |
| 3 | Pesan blocker kesiapan menyebut atasan "belum memegang izin", padahal pada kasus yang sama sebabnya bisa akun yang nonaktif | LOW (ketepatan) | Pesan: "Belum ada atasan unit tercatat yang akunnya aktif dan memegang izin … (periksa status akun dan Akses Role)" | `StockRequestService.cs` |

Tidak ada perubahan frontend pada tahap ini. Tidak ada perubahan pada modul lain, `appsettings`,
`Program.cs`, csproj, migrasi, maupun basis data.

Dokumen yang diperbarui:

- `00-interview-decisions.md` r7: rancangan `LGS-OQ-027` sebagai usulan; `LGS-OQ-029` dibuka;
- `blueprint-manifest.md` r7;
- laporan ini: bagian 15 dikoreksi, V10 dan V11 ditandai digantikan.

## 3. Audit `LGS-OQ-027` — cakupan baca lintas unit

### Keadaan endpoint saat ini

| Endpoint | Izin controller | Pembatasan di domain saat ini | Dampak bila kebijakan diberlakukan |
| --- | --- | --- | --- |
| `GET /` tanpa `scope` | `StockRequest:Read` | **Tidak ada.** Semua unit terlihat | Predikat cakupan baca dipasang sebelum `CountAsync`. Halaman, total, pencarian, dan saringan otomatis ikut, karena memakai query yang sama |
| `GET /?scope=1` | `StockRequest:Read` | Sudah dibatasi: unit yang dipimpin dengan izin Approve, ditambah CITO non-draf bagi pemegang otorisasi CITO | Tetap; menjadi bagian dari predikat |
| `GET /{id}` | `StockRequest:Read` | **Tidak ada.** Detail unit mana pun terlihat, termasuk riwayat beserta nama petugas | Predikat yang sama. Di luar cakupan dijawab **404** (usulan), supaya keberadaan permintaan tidak terungkap |
| `GET /approval-scope` | `StockRequest:Read` | Hanya data pengguna sendiri | Tidak berdampak |
| `GET /approval-readiness` | `StockRequest:ViewApprovalReadiness` | Jumlah dan nama unit, tanpa identitas pegawai | Tidak berdampak (izin menunggu `LGS-OQ-028`) |
| Ringkasan di layar | — | Memakai `GET /` dengan `pageSize=1` dan `scope` yang sama dengan tabel | Otomatis ikut predikat |
| Ekspor | — | **Tidak ada** fitur ekspor | Bila kelak dibuat, wajib memakai query yang sama |
| `POST /{id}/approve`, `reject`, `authorize-emergency`, `verify-emergency` | Approve / AuthorizeEmergency | Sudah dibatasi kewenangan domain | Sebaiknya memeriksa cakupan baca lebih dulu (404), karena pesan penolakan saat ini menyebut nama unit permintaan |
| `POST /`, `PUT /{id}`, `submit`, `cancel` | Create / Update / Cancel | **Tidak ada** pemeriksaan pemohon maupun unit | Di luar `OQ-027`; dicatat sebagai **`LGS-OQ-029`** |
| `POST /{id}/fulfill` | Fulfill | Status saja | Bergantung keputusan cakupan gudang |

SuperAdmin saat ini membaca semua unit: filter platform meloloskannya, dan domain tidak membatasi
pembacaan. Ini bertentangan dengan rekomendasi butir 3.

### Kepatuhan terhadap rancangan kebijakan

| Butir rancangan | Keadaan source |
| --- | --- |
| 1. Pengguna operasional hanya membaca unit sendiri dan unit dalam kewenangannya | **Belum.** Hanya halaman Validasi Atasan dan Verifikasi CITO yang memakai cakupan |
| 2. Pemantauan lintas unit butuh izin khusus | **Belum.** Izin belum dinamai dan belum terdaftar |
| 3. SuperAdmin tidak otomatis lintas unit | **Sebagian.** Berlaku untuk keputusan dan `scope=1`; tidak berlaku untuk daftar tanpa cakupan dan detail |
| 4. Pembatasan di backend | **Sudah** untuk `scope=1` |
| 5. Filter layar bukan satu-satunya kontrol | **Sudah** untuk `scope=1`: daftar unit tidak pernah dikirim layar |
| 6. Detail setara daftar | **Belum** |
| 7. Halaman, ringkasan, pencarian, dan ekspor sama | **Sudah** untuk cakupan yang ada; ekspor tidak ada |
| 8. Akses tidak sah tidak membocorkan informasi | **Belum** untuk detail; pesan penolakan aksi memuat nama unit |

### Yang aman disiapkan tanpa mengubah hak akses

Tidak ada perubahan source untuk `LGS-OQ-027` pada tahap ini. Setiap pembatasan baca
pada `GET /` maupun `GET /{id}` mengubah hak akses yang berlaku hari ini, termasuk gudang dan farmasi
yang memenuhi permintaan. Perubahan seperti itu dilarang sebelum keputusan definitif.

Fondasi untuk penerapannya sudah ada di source:

- kewenangan dihitung di server (`ResolveAuthorityScopeAsync`, `ApplyAuthorityScopeAsync`);
- daftar dan ringkasan memakai satu query, sehingga konsistensi butir 7 otomatis.

Rancangan teknis yang belum diterapkan:

| Komponen | Rancangan |
| --- | --- |
| Predikat cakupan baca | Gabungan dari: unit sendiri (definisi menunggu D1); unit yang dipimpin dengan izin Approve (sudah ada); CITO non-draf bagi pemegang otorisasi CITO (sudah ada); permintaan yang dibuat atau diajukan pengguna sendiri; gudang tujuan (menunggu D2); semua unit bagi pemegang izin pemantauan (menunggu D3) |
| Detail | Jalur baca publik terpisah dari `GetDetailAsync` internal yang dipanggil setelah setiap perintah, supaya pelaku sah tetap menerima hasilnya |
| Aktivasi | Saklar per lingkungan dengan nilai bawaan "perilaku lama", dinyalakan setelah data organisasi terisi. Bila dinyalakan sekarang, daftar menjadi kosong bagi hampir semua orang karena tautan unit 0/20 |
| Uji | Teori per peran (pemohon, atasan, petugas CITO, gudang, pemantau, SuperAdmin) × endpoint (`GET /`, `GET /{id}`, ringkasan, pencarian, halaman) |

### BLOCKED_DECISION — sub-keputusan yang dibutuhkan

| ID | Pertanyaan |
| --- | --- |
| D1 | Apa definisi "unit sendiri"? Pilihan: penempatan HR aktif apa pun (`WfpOrganizationAssignment`, manajerial maupun tidak) pada unit organisasi yang ditautkan, atau hanya permintaan yang dibuat dan diajukan sendiri |
| D2 | Apa cakupan gudang dan farmasi pemenuhan? Kandidat sumber data: `MstDrugStorageLocation.ServiceUnitId` (gudang → unit layanan → penempatan). Isinya di basis data tim **belum diukur** pada tahap ini |
| D3 | Apa nama izin pemantauan lintas unit (usulan teknis: `StockRequest:ReadAllUnits`), dan jabatan mana yang memegangnya? |
| D4 | Apakah SuperAdmin pendukung teknis tetap membutuhkan penempatan berizin pemantauan? |
| D5 | Apakah pemohon yang pindah unit tetap melihat permintaannya sendiri? |
| D6 | Di luar cakupan, apakah jawabannya 404 (usulan, tidak membocorkan keberadaan) atau 403? |
| D7 | Bagaimana urutan aktivasinya terhadap pengisian data organisasi (BLOCKED_DATA)? |

## 4. Risiko keamanan tersisa

| Severity | Risiko | Status |
| --- | --- | --- |
| MEDIUM | Pembacaan lintas unit pada `GET /` tanpa cakupan dan `GET /{id}`, termasuk SuperAdmin | BLOCKED_DECISION `LGS-OQ-027` |
| MEDIUM | Perintah tulis lintas unit: membuat permintaan atas nama unit lain dengan petugas peminta pilihan layar, mengubah atau mengajukan draf unit lain, membatalkan permintaan unit lain termasuk yang sudah Disetujui. Perilaku sejak `aec9842e` (2 September 2026), bukan dari Logistik V2 | BLOCKED_DECISION `LGS-OQ-029` |
| MEDIUM | **Integration saat ini masih mencatat id permintaan sebagai pelaku di log audit.** Perbaikannya ada di perubahan lokal yang belum di-commit | Selesai setelah commit backend masuk integration |
| LOW | Pengulangan kunci idempotensi oleh pengguna lain mengembalikan detail tanpa pemeriksaan kewenangan ulang. Tidak ada perubahan data; kuncinya UUID buatan klien | Diterima; dicatat |
| LOW | Pesan penolakan aksi memuat nama unit permintaan | Ditangani bersama `LGS-OQ-027` |
| LOW | Penunjukan jabatan CITO dan tenggat hanya lewat konfigurasi | `LGS-OQ-026` |
| LOW | Dengan `Security:Authorization:Enabled=false` (Development), laporan kesiapan terbuka bagi siapa pun yang login | `LGS-OQ-028` |
| Di luar Logistik | `LoggerService` milik platform mengambil `UserId` dari properti data bernama `Id`. Modul lain yang mengirim objek berisi `Id` ikut mencatat identitas yang salah | Dilaporkan ke pemilik platform; tidak diubah |
| Di luar Logistik | Peringatan NuGet kerentanan `SixLabors.ImageSharp` 3.1.11 pada proyek utama (log build 17 November) | Informasi untuk pemilik platform |

Tidak ditemukan temuan CRITICAL maupun HIGH.

## 5. Matriks kontrak backend–frontend dan readiness integrasi

### Kondisi branch

| Repo | Lokal | Integration | Catatan |
| --- | --- | --- | --- |
| Backend | `Ikbal` @ `33dd4eb1` (= `origin/Ikbal`) + 16 berkas belum commit | `75e3c5e6`, maju 4 commit sejak `ffedc61c` (PR #357 milik pengembang lain: HR Job Description, pensiun, laporan) | `33dd4eb1` adalah leluhur integration. `git merge-tree` HEAD × integration bersih. Integration hanya menyentuh `Program.cs` dan `ApplicationDbContext.cs` (registrasi HR), dan keduanya tidak kita ubah. Tidak ada berkas Logistik yang beririsan |
| Frontend | `Ikbalv2` @ `0aae59c80` + 17 berkas belum commit | `0aae59c80` | Sama; tidak ada commit integration baru |

**Fitur yang sudah di-merge tidak tertimpa.** Controller dan DTO lokal hanya menambah, tanpa baris
yang dihapus. Perubahan pada kode yang sudah di-merge ada tiga:

- refaktor penyaring izin;
- data audit `Id` → `StockRequestId`/`UserId`;
- **otorisasi CITO kini juga mensyaratkan penunjukan jabatan** (`LGS-DEC-004`).

Akibat perubahan ketiga: setelah backend lokal masuk, otorisasi CITO tertutup sampai
`EmergencyAuthorizerPositions` diisi. Dalam praktik belum ada perbedaan, karena izin AuthorizeEmergency
masih 0 jabatan.

**Syarat urutan:** commit backend harus masuk integration **sebelum atau bersamaan** dengan commit
frontend. Backend integration saat ini belum memiliki `GET /approval-scope` (layar akan menerima 404).
Backend itu juga mengabaikan `scope` dan `emergencyReviewDueState`, sehingga "Unit yang Saya Pimpin"
akan menampilkan semua unit, dan kartu "Mendekati Tenggat"/"Terlambat" menghitung semua CITO yang
menunggu.

### Matriks

| Endpoint | DTO | Izin controller | Penjagaan domain | Service | Konsumen frontend | Status | Keterbatasan verifikasi |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET /` | `StockRequestPagedQuery` → `PagedResult<StockRequestSummaryResponse>` | Read | `scope=1` dihitung server; tanpa cakupan: tidak ada | `GetPagedAsync` | `useStockRequestList`, `useStockRequestApprovalSummary` | Merged, diperluas lokal (`scope`, `emergencyReviewDueState`, field CITO/tenggat) | Uji SQLite NOT_RUN; runtime belum |
| `GET /approval-scope` | `StockRequestApprovalScopeResponse` | Read | Data pengguna sendiri | `GetApprovalScopeAsync` | `useStockRequestApprovalScope` | Lokal, belum commit | sama |
| `GET /approval-readiness` | `StockRequestApprovalReadinessResponse` | ViewApprovalReadiness | — | `GetApprovalReadinessAsync` | Tidak ada (halaman NOT_STARTED, `LGS-OQ-028`) | Lokal, belum commit | sama |
| `GET /{id}` | `StockRequestDetailResponse` | Read | Tidak ada (`LGS-OQ-027`) | `GetDetailAsync` | `useStockRequestDetail`, `useStockRequestForm` | Merged, diperluas lokal (penanda kewenangan, tenggat) | sama |
| `POST /` | `CreateStockRequestRequest` | Create | Validasi referensi saja (`LGS-OQ-029`) | `CreateAsync` | `useStockRequestForm` | Merged (lama) | Audit Create tidak diuji |
| `PUT /{id}` | `UpdateStockRequestRequest` | Update | Status Draft saja | `UpdateAsync` | `useStockRequestForm` | Merged (lama) | — |
| `POST /{id}/submit` | `SubmitStockRequestRequest` | Update | Status, alasan CITO | `SubmitAsync` | `useStockRequestDetail` | Merged | NOT_RUN |
| `POST /{id}/cancel` | `CancelStockRequestRequest` | Cancel | Status saja | `CancelAsync` | `useStockRequestDetail` | Merged (lama) | — |
| `POST /{id}/approve`, `/reject` | `Approve…`/`RejectStockRequestRequest` | Approve | Bukan pemohon + izin eksplisit + atasan unit | `DecideAsync` | `useStockRequestDetail` | Merged | NOT_RUN |
| `POST /{id}/authorize-emergency` | `AuthorizeEmergencyStockRequestRequest` | AuthorizeEmergency | Bukan pemohon + jabatan ditunjuk + izin eksplisit | `DecideAsync` | `useStockRequestDetail` | Merged, **diperketat** lokal (penunjukan) | NOT_RUN |
| `POST /{id}/verify-emergency` | `VerifyEmergencyStockRequestRequest` | Approve | Bukan pemohon/pengotorisasi + izin + atasan unit | `VerifyEmergencyAsync` | `useStockRequestDetail` | Merged | NOT_RUN |
| `POST /{id}/fulfill` | `FulfillStockRequestRequest` | Fulfill | Disetujui atau aturan peralihan | `FulfillAsync` | `useStockRequestDetail` | Merged | NOT_RUN |

Nilai enum yang dibaca layar dikunci di kedua sisi:

- backend: `ApiContractTests`;
- frontend: `stock-request-approval.test.mjs`.

## 6. Kesiapan build backend terisolasi

| Prasyarat | Keadaan saat diperiksa |
| --- | --- |
| Backend tidak berjalan, supaya DLL di `C:\tmp\phm\bin` tidak terkunci | Terpenuhi. Satu-satunya `dotnet.exe` adalah build host ekstensi C# Dev Kit milik VS Code, bukan backend |
| Tidak ada build lain berjalan bersamaan (keterbatasan memori PC ini) | Jalankan sendirian, di foreground, tanpa build paralel |
| `SkipMigrationMetadata=true` | Sudah ada di csproj. Tetap disebut di perintah sebagai pengaman |
| Proyek uji terisolasi | Ada di `C:\Users\BenariDev02\AppData\Local\Temp\claude\12442addadf15847\84187131-d50d-4a26-8f54-4d54b0e016a9\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj`. Folder sementara dapat terhapus saat reboot; isinya di bawah dapat dibuat ulang di folder mana pun di luar repo |
| Paket NuGet | Sudah ter-restore pada 17 November (folder `obj` ada) |
| Sinkron dengan integration sebelum uji (opsional) | Diperkirakan aman bagi SQLite. Konfigurasi entity HR baru hanya memakai `CURRENT_TIMESTAMP` dan tipe `timestamp with time zone`, keduanya diterima SQLite. Ini penilaian statis |

Isi csproj terisolasi:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <AssemblyName>StockRequestApprovalIsolated.Tests</AssemblyName>
    <RootNamespace>QuilvianSystemBackend.Tests.Pharmacy</RootNamespace>
    <SkipMigrationMetadata>true</SkipMigrationMetadata>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.18" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>
  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="C:\tmp\phm\QuilvianSystemBackend.csproj" />
    <Compile Include="C:\tmp\phm\Tests\QuilvianSystemBackend.PharmacyTests\Infrastructure\TestDatabase.cs" Link="Infrastructure\TestDatabase.cs" />
    <Compile Include="C:\tmp\phm\Tests\QuilvianSystemBackend.PharmacyTests\Infrastructure\StockRequestApprovalHarness.cs" Link="Infrastructure\StockRequestApprovalHarness.cs" />
    <Compile Include="C:\tmp\phm\Tests\QuilvianSystemBackend.PharmacyTests\Infrastructure\CatatanLogUji.cs" Link="Infrastructure\CatatanLogUji.cs" />
    <Compile Include="C:\tmp\phm\Tests\QuilvianSystemBackend.PharmacyTests\StockRequestApprovalTests\*.cs" LinkBase="StockRequestApprovalTests" />
  </ItemGroup>
</Project>
```

Perintah build (PowerShell, dari folder proyek terisolasi):

```powershell
dotnet build .\StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1 *> build.log
```

Kriteria lolos: `0 Error(s)`. Peringatan CS1573 dan NU190x yang sudah ada di proyek utama bukan
kegagalan.

Risiko compile yang perlu diperhatikan pada build pertama, karena perubahan tahap ini belum pernah
di-compile:

- `GrantableAction?` sebagai hasil `FirstOrDefaultAsync`;
- komposisi `GrantingPlacements` dengan join `Users`;
- `CatatanLogUji.PelakuPada`.

## 7. Kesiapan test runner terisolasi

```powershell
dotnet test .\StockRequestApprovalIsolated.Tests.csproj --no-build --filter "FullyQualifiedName~StockRequestApprovalTests" --logger "console;verbosity=normal" *> test.log
```

| Hal | Nilai |
| --- | --- |
| Jumlah yang diharapkan | **107 kasus** dari 93 metode |
| Basis data | SQLite dalam memori per uji. Tidak perlu connection string maupun variabel lingkungan |
| Durasi acuan | ±75 detik untuk 64 kasus pada 17 November |
| Bila ada yang gagal | Perbaiki source atau uji, ulangi build dan test, lalu catat jumlah lolos di laporan dan pesan commit. Jangan menandai RUNTIME_VERIFIED untuk kasus yang tidak dijalankan |
| Yang tidak dibuktikan | Perilaku PostgreSQL (konkurensi, indeks unik bersyarat, check constraint), filter `[AccessPermission]`, dan aplikasi berjalan |

## 8. Berkas yang aman untuk commit

**Backend — 16 berkas:**

```text
Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs
Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs
Areas/HealthServices/PharmacyManagement/Enums/StockRequestEnums.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalAuthority.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestOperationalSettings.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs
Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/CatatanLogUji.cs
Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApiContractTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApprovalScopeTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/OperationalSettingsUnitTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/RobustnessAndAuditTests.cs
docs/module-blueprints/logistik/00-interview-decisions.md
docs/module-blueprints/logistik/blueprint-manifest.md
docs/module-blueprints/logistik/evidence/05-aktivasi-batas-persetujuan.md
docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md
```

Daftar ini bertambah satu dibanding V10: `ApiContractTests.cs`.

**Frontend — 17 berkas:** sama dengan V10. Tidak ada perubahan frontend pada tahap ini.

**Dikecualikan** (bukan milik pekerjaan ini):

- backend: `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/`;
- frontend: `test-results/.last-run.json` dan 16 berkas `tests/e2e/zz-*`.

Stage berkas satu per satu menurut daftar. Jangan memakai `git add -A` atau `git add .`.

**Audit rahasia:** diff dan berkas baru di kedua repo dipindai untuk password, connection string,
host, token, kunci privat, alamat server basis data, dan alamat email. Temuannya 0; satu-satunya
kecocokan adalah kalimat laporan ini sendiri. Tidak ada GUID atau kode jabatan yang di-hardcode di
source non-uji, tidak ada literal peran `"SuperAdmin"`, `AllowAnonymous`, maupun pengecualian
otorisasi.

**Risiko konflik:**

- backend tidak beririsan dengan integration `75e3c5e6` (lihat butir 5);
- frontend sama dengan integration;
- risiko utamanya urutan merge: backend dulu (butir 5).

**Saran pesan commit backend:**

```text
feat(logistik): cakupan "Unit yang Saya Pimpin", penunjukan jabatan CITO, tenggat verifikasi

- GET /stock-requests?scope=1 dihitung dari kewenangan aktual (unit dipimpin + izin Approve
  eksplisit; CITO lintas unit tanpa draf); GET approval-scope dan approval-readiness.
- Otorisasi CITO wajib jabatan aktif yang ditunjuk manajemen + izin eksplisit
  (PharmacyManagement:StockRequest:EmergencyAuthorizerPositions).
- Tenggat verifikasi susulan dapat dikonfigurasi (UTC, tanpa bawaan, batas teknis 100 tahun)
  + filter keadaan tenggat.
- Satu penyaring izin eksplisit untuk penegakan dan laporan kesiapan; kesiapan mencocokkan
  atasan unit dengan izin Approve-nya.
- Log audit permintaan stok mencatat identitas pelaku yang benar (StockRequestId/UserId).
- Uji: 93 metode StockRequestApprovalTests.
- Blueprint logistik r7 (LGS-OQ-027 usulan, LGS-OQ-029 dibuka); laporan FIN-LGS.

Tanpa entity, tabel, maupun migration baru.
```

**Pesan commit frontend:** sama dengan V11.

Setelah test runner benar-benar dijalankan, tambahkan kalimat "Uji: N/107 kasus lolos (SQLite)".

## 9. Daftar blocker

| Jenis | Blocker | Pemilik |
| --- | --- | --- |
| BLOCKED_DECISION | `LGS-OQ-027` cakupan baca beserta sub-keputusan D1–D7 | Pemilik Farmasi + manajemen |
| BLOCKED_DECISION | `LGS-OQ-029` cakupan perintah tulis dan sumber petugas peminta | Pemilik Farmasi + manajemen |
| BLOCKED_DECISION | `LGS-OQ-024` notifikasi, `025` tindak lanjut temuan, `026` penunjukan CITO lewat konfigurasi vs master, `028` izin kesiapan | lihat decision log |
| BLOCKED_DATA | Tautan unit 0/20; penempatan manajerial 0; Approve dan AuthorizeEmergency 0 jabatan; jabatan CITO, tenggat, dan batas aktivasi kosong; data gudang → unit untuk D2 belum diukur | HRD, administrator Akses Role, manajemen, operator konfigurasi |
| BLOCKED_MIGRATION | Tindak lanjut temuan CITO; aktivasi batas; notifikasi dan master penunjukan bila dipilih | Pemilik modul + registry |
| BLOCKED_SANDBOX | Konkurensi PostgreSQL (prosedur bagian 15) | Pemilik basis data |
| Menunggu izin | Build dan test runner terisolasi (butir 6–7); commit dan push (butir 8) | Pemrakarsa |
| Di luar Logistik | `PharmacyTests` tidak terkompilasi karena 4 berkas uji Billing; `LoggerService` mengambil `Id` sebagai pelaku | Pemilik Billing; pemilik platform |

## 10. Instruksi verifikasi runtime berikutnya

Setiap langkah membutuhkan izin terpisah dan dijalankan berurutan, tidak paralel.

1. **Build terisolasi** (butir 6). Gagal compile: perbaiki, lalu ulangi.
2. **Test runner terisolasi** (butir 7). Target 107/107. Catat hasil aktual di laporan ini.
3. **Uji unit frontend dan ESLint** dijalankan ulang tepat sebelum commit.
4. **Commit backend lebih dulu**, lalu frontend, menurut daftar butir 8. Push, lalu PR ke integration sesuai prosedur tim. Jangan membuat ulang PR #356.
5. **Aplikasi berjalan** di lingkungan uji yang disetujui dengan `Security__Authorization__Enabled=true`. Periksa `GET /approval-readiness` sampai `isReadyForApproval` bernilai benar. Ini baru mungkin setelah BLOCKED_DATA terisi.
6. **Skenario UAT 1–16** (bagian 12) di browser, dengan bukti tangkapan layar, respons jaringan, isi riwayat, dan saldo stok.
7. **Prosedur konkurensi PostgreSQL** (bagian 15) di sandbox.
8. **`LGS-OQ-027` / `029`** diterapkan setelah keputusan dan data siap, dengan uji per peran × endpoint (butir 3).

**Tidak ada bagian yang berstatus RUNTIME_VERIFIED.** Satu-satunya bukti runtime adalah run historis
64/64 untuk snapshot `b50034c5`. *(Diperbarui oleh bagian berikut.)*

---

# HASIL EKSEKUSI VERIFIKASI

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026, 14.25–14.32 WIB |
| Lingkup | TASK 1 build backend terisolasi, TASK 2 uji backend Logistik, TASK 3 finalisasi keamanan, TASK 4 persiapan commit |
| Batas | Tanpa migrasi, tanpa penulisan basis data operasional, tanpa build/dev server frontend, tanpa perubahan modul lain, tanpa commit/push/merge |
| Prasyarat yang dijalankan | Backend dan dev server frontend yang dinyalakan sebelumnya atas permintaan **dihentikan** lebih dulu (PID per PID). Port 3000, 5107, dan 7184 bebas; tidak ada proses backend yang mengunci DLL |

## Ringkasan

| Hal | Hasil |
| --- | --- |
| **Build backend terisolasi** | **PASS**: 0 error, 1 menit 17 detik; build ulang setelah penambahan uji: 0 error, 4 detik. Tidak ada peringatan dari source Logistik maupun berkas uji. Peringatan yang tersisa milik proyek utama (CS1573, EF1002, NU190x `SixLabors.ImageSharp`) |
| **Uji backend Logistik** | **108 Passed / 0 Failed / 0 Skipped** dari 94 metode, dalam satu run penuh (2 menit 4 detik). Run sebelumnya (107 kasus, sebelum uji HTTP tenggat ditambahkan): 107/107 |
| Basis data uji | SQLite `:memory:` per uji. Log uji tidak memuat `Npgsql`, `Host=`, maupun port 5432; tidak ada startup aplikasi (`--no-build`, tanpa host web) |
| Konkurensi | Yang lolos adalah **logika token versi pada SQLite** (`ConcurrencyTests`, 6/6). **Konkurensi PostgreSQL belum diuji** (BLOCKED_SANDBOX) |
| Status | Uji otomatis backend Logistik: **RUNTIME_VERIFIED untuk lingkup SQLite** (108 kasus di atas). Aplikasi, filter `[AccessPermission]`, PostgreSQL, dan UAT: **belum** |
| Keputusan commit | **GO** (backend dulu, lalu frontend), menunggu persetujuan eksplisit. Bukan GO untuk UAT maupun produksi |

## TASK 1 — Build backend terisolasi

| Pemeriksaan | Hasil |
| --- | --- |
| 16 berkas backend | Sama dengan butir 8 *FINAL VERIFICATION*; satu-satunya perubahan sesudahnya adalah komentar XML `HasExplicitGrantAsync` dan satu uji baru |
| Build tidak menjalankan migrasi, startup, atau basis data | `QuilvianSystemBackend.csproj` hanya punya satu target kustom, `ValidateMigrationHistoryCoverage`, yang menghitung berkas (tanpa `Exec`, tanpa koneksi). Proyek terisolasi tidak punya target kustom. Build memakai `-p:SkipMigrationMetadata=true` |
| Perintah | `dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true` (foreground, sendirian) |
| Hasil | `Build succeeded. 0 Error(s)` — tiga konstruksi baru yang belum pernah di-compile (`GrantableAction?`, komposisi `GrantingPlacements`, `CatatanLogUji.PelakuPada`) lolos compile |
| Perbaikan yang diperlukan | Tidak ada |

## TASK 2 — Uji backend Logistik

Perintah:

```text
dotnet test StockRequestApprovalIsolated.Tests.csproj --no-build --filter "FullyQualifiedName~StockRequestApprovalTests" --logger "console;verbosity=normal" --logger "trx;LogFileName=run4.trx"
```

| Kelas | Kasus | Passed | Failed | Skipped |
| --- | --- | --- | --- | --- |
| `ApiContractTests` | 5 | 5 | 0 | 0 |
| `ApprovalScopeTests` | 17 | 17 | 0 | 0 |
| `AuthorizationTests` | 13 | 13 | 0 | 0 |
| `ConcurrencyTests` (SQLite) | 6 | 6 | 0 | 0 |
| `CutoverUnitTests` | 11 | 11 | 0 | 0 |
| `EmergencyReviewTests` | 12 | 12 | 0 | 0 |
| `OperationalSettingsUnitTests` | 14 | 14 | 0 | 0 |
| `RobustnessAndAuditTests` | 12 | 12 | 0 | 0 |
| `StatusTransitionTests` | 18 | 18 | 0 | 0 |
| **Total** | **108** | **108** | **0** | **0** |

Tidak ada kegagalan, sehingga tidak ada perbaikan source maupun uji akibat kegagalan.

Satu uji ditambahkan, yaitu `ApiContractTests.Konfigurasi_tenggat_tidak_sah_tidak_membuat_endpoint_gagal_500`. Alasannya: klaim "konfigurasi tenggat tidak menyebabkan HTTP 500" sebelumnya hanya dibuktikan di tingkat fungsi. Uji ini memanggil `GetPaged` dan `GetById` lewat controller dengan tenggat `1e300` dan memastikan keduanya menjawab 200.

Berkas hasil berada di luar repo:

- `...\scratchpad\srq-isolated-tests\test4.log`;
- `...\scratchpad\srq-isolated-tests\TestResults\run4.trx`.

## TASK 3 — Finalisasi keamanan

| Perbaikan | Uji penjaga yang lolos |
| --- | --- |
| Audit mencatat identitas pelaku yang benar | `Keputusan_dan_penolakan_kewenangan_tercatat_di_log_audit`, `Pengajuan_dan_penyerahan_mencatat_pelaku_bukan_id_permintaan`, `Temuan_penyimpangan_tercatat_sebagai_audit_dan_peringatan`. Kolom `UserId` dibandingkan persis, dan id permintaan tidak pernah muncul sebagai pelaku |
| Jabatan CITO nonaktif tidak berwenang | `Jabatan_CITO_yang_dinonaktifkan_tidak_lagi_memberi_kewenangan` |
| Draf CITO tidak bocor lintas unit | `Draf_CITO_unit_lain_tidak_terlihat_oleh_petugas_CITO` |
| Izin dan cakupan diperiksa backend | `Kasus2`–`Kasus5`, `Staf_berizin_tanpa_penempatan_manajerial_ditolak`, `Izin_pada_controller_nonaktif_tidak_berlaku_dan_tidak_dihitung_kesiapan`, `Pengguna_tanpa_kewenangan_mendapat_daftar_kosong_bukan_seluruh_data` (3 kasus, termasuk SuperAdmin), `Atasan_hanya_melihat_permintaan_unit_yang_dipimpinnya` |
| Konfigurasi tenggat tidak menyebabkan 500 | `Nilai_tenggat_atau_peringatan_yang_tidak_sah...` (8 kasus, termasuk `1000000` dan `1e300`) dan `Konfigurasi_tenggat_tidak_sah_tidak_membuat_endpoint_gagal_500` |

### Lembar keputusan `LGS-OQ-027` dan `LGS-OQ-029`

Tidak ada kebijakan yang diubah. Kolom "rekomendasi" berisi saran teknis, bukan approval.

| ID | Pertanyaan | Rekomendasi teknis | Alasan |
| --- | --- | --- | --- |
| D1 | Definisi "unit sendiri" | Penempatan HR aktif (`WfpOrganizationAssignment`, manajerial maupun tidak) pada unit organisasi yang ditautkan ke unit layanan, ditambah permintaan yang dibuat atau diajukan sendiri | Sumber data sama dengan `LGS-DEC-003`; tanpa tabel baru |
| D2 | Cakupan gudang | Gudang tujuan lewat `MstDrugStorageLocation.ServiceUnitId` → unit layanan gudang → penempatan petugas | Tanpa tabel baru; isi datanya harus diukur dan dilengkapi lebih dulu |
| D3 | Izin pemantauan lintas unit | `StockRequest:ReadAllUnits`, diberikan manajemen kepada jabatan pengawas (mis. Kepala Instalasi Farmasi/Logistik, audit internal) | Memenuhi butir 2 rancangan |
| D4 | SuperAdmin | Tidak otomatis; harus memegang penempatan berizin pemantauan | Konsisten dengan aturan CITO dan persetujuan |
| D5 | Pemohon pindah unit | Tetap melihat permintaan yang ia buat | Jejak pekerjaannya sendiri |
| D6 | Jawaban di luar cakupan | 404 | Tidak mengungkap keberadaan permintaan (butir 8) |
| D7 | Aktivasi | Saklar per lingkungan dengan bawaan perilaku lama; dinyalakan setelah `GET /approval-readiness` menunjukkan data organisasi terisi, disertai uji per peran × endpoint | Mengaktifkan sekarang mengosongkan daftar hampir semua orang (tautan unit 0/20) |
| W1 (`029`) | Sumber petugas peminta | Diambil dari akun yang login (`WorkforceProfileId`), bukan dari layar | Mencegah permintaan atas nama orang lain |
| W2 (`029`) | Buat, ubah, ajukan | Hanya untuk unit sendiri (D1); ubah dan ajukan hanya oleh pemohon atau rekan unit yang sama | Sejalan dengan cakupan baca |
| W3 (`029`) | Batal | Pemohon atau unit sendiri selama Draft/Menunggu; setelah Disetujui hanya atasan unit, dan hanya sebelum diserahkan | Pembatalan permintaan yang sudah disetujui adalah keputusan, bukan administrasi |

Risiko bila belum diputuskan: **MEDIUM**. Semua pemegang `StockRequest:Read` dapat membaca permintaan
lintas unit, dan pemegang Create/Update/Cancel dapat bertindak lintas unit. Larangan memutus permintaan
sendiri dan kewenangan atasan/CITO **tetap** ditegakkan, sehingga persetujuan tidak dapat
disalahgunakan lewat celah ini.

### Risiko keamanan yang tersisa

| Severity | Risiko | Status |
| --- | --- | --- |
| MEDIUM | Baca lintas unit (`LGS-OQ-027`) | BLOCKED_DECISION |
| MEDIUM | Tulis lintas unit dan petugas peminta dari layar (`LGS-OQ-029`, perilaku lama) | BLOCKED_DECISION |
| MEDIUM | Integration masih mencatat id permintaan sebagai pelaku di log audit | Selesai begitu commit backend ini masuk integration |
| LOW | Replay kunci idempotensi oleh pengguna lain mengembalikan detail; penunjukan CITO dan tenggat lewat konfigurasi (`026`); laporan kesiapan terbuka di Development (`028`) | Diterima / terbuka |
| Belum diuji | Konkurensi PostgreSQL, filter `[AccessPermission]`, aplikasi dengan pengguna sungguhan | BLOCKED_SANDBOX / BLOCKED_DATA |

## TASK 4 — Persiapan commit

| Pemeriksaan | Hasil |
| --- | --- |
| Backend | **16 berkas** (daftar butir 8 *FINAL VERIFICATION*), seluruhnya di `PharmacyManagement/{Controllers,DTOs,Enums,Services}/StockRequest*`, `PharmacyTests` (harness, logger penangkap, `StockRequestApprovalTests`), dan `docs/module-blueprints/logistik/` |
| Frontend | **17 berkas** (daftar V10); tidak berubah sejak verifikasi sebelumnya |
| Berkas developer lain | Tidak ikut: `SeedData/ICD10/*`, `Storage/uploads/*`, `test-results/.last-run.json`, `tests/e2e/zz-*` |
| Berkas sementara | 0 di working tree kedua repo. Log, `.trx`, dan proyek terisolasi ada di scratchpad, di luar repo |
| Kredensial | 0. Tiga kecocokan pola di backend adalah kalimat laporan ini yang menjelaskan pemindaian |
| Integration terbaru | Backend `bc14e6d1` (PR #358, HR/payroll) dan frontend `cf915ddb5` (PR #209, payroll). Tidak ada irisan dengan berkas Logistik; `git merge-tree` bersih di kedua repo |
| Urutan | **Backend dulu**, lalu frontend. Frontend membutuhkan `GET /approval-scope` dan parameter `scope`/`emergencyReviewDueState` |

Berkas siap pakai di scratchpad (di luar repo):

- `be-stage.txt` (16 baris) dan `commit-be.txt`;
- `fe-stage.txt` (17 baris) dan `commit-fe.txt`.

Pesan commit sudah memuat hasil uji (108/108, SQLite) dan baris atribusi. Perintah yang **akan**
dijalankan setelah persetujuan:

```text
# Backend (C:\tmp\phm, branch Ikbal)
git add --pathspec-from-file=<scratchpad>\be-stage.txt
git commit -F <scratchpad>\commit-be.txt

# Frontend (QuilvianSystemFrontendDev, branch Ikbalv2)
git add --pathspec-from-file=<scratchpad>\fe-stage.txt
git commit -F <scratchpad>\commit-fe.txt
```

## Kesimpulan

| Keputusan | Status | Syarat |
| --- | --- | --- |
| Commit backend lalu frontend | **GO** | Persetujuan eksplisit; jalankan dengan daftar stage di atas, bukan `git add -A` |
| Push dan PR ke integration | Menunggu persetujuan terpisah | Backend lebih dulu; jangan membuat ulang PR #356 |
| UAT dan produksi | **NO-GO** | BLOCKED_DATA (data organisasi, izin, jabatan CITO, tenggat); keputusan `LGS-OQ-027`/`029`; konkurensi PostgreSQL di sandbox |
