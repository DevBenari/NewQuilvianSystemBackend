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
| Proyek uji terisolasi | Ada di `%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj`. Folder sementara dapat terhapus saat reboot; isinya di bawah dapat dibuat ulang di folder mana pun di luar repo |
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

---

# HASIL COMMIT LOKAL

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026, 14.58 WIB |
| Dasar | Instruksi "APPROVED FOR LOCAL COMMIT" |
| Batas | Tanpa push, tanpa PR, tanpa perubahan branch integration, tanpa migrasi, basis data, deployment, maupun dev server |

> **Catatan.** Laporan ini ikut dalam commit backend di bawah. Bagian *HASIL COMMIT LOKAL* ditulis
> **sesudah** commit, karena SHA baru diketahui setelahnya. Akibatnya bagian ini adalah perubahan
> yang **belum di-commit** pada working tree backend. Ia dapat ikut commit dokumen berikutnya bila
> disetujui.

## Commit

| Repo | Branch | SHA | Parent | Berkas | Baris |
| --- | --- | --- | --- | --- | --- |
| Backend `DevBenari/NewQuilvianSystemBackend` | `Ikbal` | `6302f5fc3d2cabc211c640f254ef32b7672804ed` | `33dd4eb1` | 16 | +3134 / −53 |
| Frontend `DevBenari/QuilvianSystemFrontendDev` | `Ikbalv2` | `002f0bcd75017a75cb95bc548cbe23689264c88e` | `0aae59c80` | 17 | +937 / −268 |

Kedua commit memakai pesan dari `commit-be.txt` dan `commit-fe.txt`, termasuk baris atribusi. Tidak
ada hook aktif di kedua repo, dan tidak ada yang dilewati.

**Berkas dalam commit backend `6302f5fc`** (sama persis dengan `be-stage.txt`):

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

**Berkas dalam commit frontend `002f0bcd`** (sama persis dengan `fe-stage.txt`):

```text
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-attention-strip.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-authority-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-authority-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-authorization-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-follow-up-notice.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-review-panel.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-summary-cards.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-detail-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-emergency-review-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-list-view.jsx
src/lib/constants/health-services/pharmacy-management/stock-request-constant.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-scope.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-summary.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx
src/lib/services/health-services/pharmacy-management/stock-request.service.js
src/utils/health-services/pharmacy-management/stock-request-approval-utils.js
tests/unit/stock-request-approval.test.mjs
```

## Pemeriksaan sebelum dan sesudah commit

| Pemeriksaan | Backend | Frontend |
| --- | --- | --- |
| Branch | `Ikbal` | `Ikbalv2` |
| Staging sebelum `git add` | Kosong | Kosong |
| Berkas ter-stage = daftar stage | Ya (16) | Ya (17) |
| Berkas di luar Logistik | 0 | 0 |
| Migrasi, `bin/obj`, `appsettings`, csproj, `package.json`, `.env` | 0 | 0 |
| Source yang di-commit = source yang diuji | Ya. Tidak ada `.cs` yang berubah setelah run 108/108 (14.32) | Ya. Tidak ada berkas `src`/`tests/unit` yang berubah sejak uji unit 32/32 |
| Perubahan Logistik yang tertinggal di working tree | 0 (sebelum bagian laporan ini ditulis) | 0 |

## Status working tree

| Repo | Sisa | Pemilik |
| --- | --- | --- |
| Backend | `?? SeedData/ICD10/MstDiagnosis.csv`, `?? SeedData/ICD10/MstDiagnosisChapter.csv`, `?? Storage/uploads/patient-qrcodes/00-00-00-03/` | Bukan pekerjaan ini; dibiarkan utuh |
| Backend | ` M docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | Bagian laporan ini (lihat catatan di atas) |
| Frontend | ` M test-results/.last-run.json`, 16 × `?? tests/e2e/zz-*` | Bukan pekerjaan ini; dibiarkan utuh |

Tidak ada berkas yang dihapus, di-reset, maupun di-stash.

## Status sinkronisasi

| Repo | Terhadap branch remote | Terhadap integration | `git merge-tree` |
| --- | --- | --- | --- |
| Backend | `origin/Ikbal`: tertinggal 0, unggul 1 | `origin/QuilvianIntegrationBackend` `bc14e6d1`: integration unggul 20 commit (pengembang lain, HR/payroll/laporan), kita unggul 1 | Bersih; tidak ada berkas Logistik yang beririsan |
| Frontend | `origin/Ikbalv2`: tertinggal 0, unggul 1 | `origin/QuilvianIntegrationFrontend` `cf915ddb5`: integration unggul 3 commit (payroll), kita unggul 1 | Bersih |

## Bukti build dan uji (sebelum commit, source sama)

| Bukti | Hasil |
| --- | --- |
| Build backend terisolasi (`-p:SkipMigrationMetadata=true`), 14.25 | **PASS**: 0 error, 1 menit 17 detik; build ulang setelah uji tambahan: 0 error |
| Uji backend Logistik, 14.30–14.32 | **108 Passed / 0 Failed / 0 Skipped** (94 metode), SQLite dalam memori; `TestResults\run4.trx` di scratchpad |
| Uji unit frontend + ESLint | 32/32 lolos; ESLint 0 error (3 warning pola lama `set-state-in-effect`) |
| Belum diuji | Konkurensi PostgreSQL, filter `[AccessPermission]`, aplikasi dengan pengguna sungguhan, UAT |

## Blocker yang masih terbuka

| Jenis | Blocker |
| --- | --- |
| BLOCKED_DECISION | `LGS-OQ-027` (baca lintas unit, D1–D7) dan `LGS-OQ-029` (tulis lintas unit, W1–W3) **tetap OPEN**; kebijakan akses tidak diubah. Juga `LGS-OQ-024`, `025`, `026`, `028` |
| BLOCKED_DATA | Tautan unit 0/20, penempatan manajerial 0, izin Approve/AuthorizeEmergency 0 jabatan, jabatan CITO, tenggat, dan batas aktivasi belum diisi |
| BLOCKED_MIGRATION | Tindak lanjut temuan CITO; aktivasi batas; notifikasi/master penunjukan bila dipilih |
| BLOCKED_SANDBOX | Konkurensi PostgreSQL |
| Lingkungan lokal | DB lokal `QuilvianNewDevIkbal*` tertinggal migrasi modul lain; backend hanya start dengan `Seeders__ContinueOnFailure=true` |
| Di luar Logistik | `PharmacyTests` tidak terkompilasi (uji Billing); `LoggerService` platform mengambil `Id` sebagai pelaku |

## Kesiapan push dan PR

| Langkah | Status | Catatan |
| --- | --- | --- |
| Push `Ikbal` (backend) | **Siap, menunggu persetujuan** | Fast-forward (tertinggal 0); tanpa force |
| Push `Ikbalv2` (frontend) | **Siap, menunggu persetujuan** | Fast-forward; **sesudah** backend |
| PR backend `Ikbal` → `QuilvianIntegrationBackend` | Menunggu persetujuan | PR **baru**, bukan PR #356. Push langsung ke integration sebelumnya ditolak (*required check* "QBE Strict GitRange"), dan `gh` tidak terpasang, sehingga PR dibuat lewat halaman compare GitHub. Integration unggul 20 commit tetapi gabungan bersih |
| PR frontend `Ikbalv2` → `QuilvianIntegrationFrontend` | Menunggu persetujuan | Di-merge **setelah** PR backend |
| Opsional sebelum PR | — | Bila tim mewajibkan branch mutakhir: gabungkan integration ke `Ikbal`, lalu jalankan ulang build dan uji terisolasi. Butuh persetujuan terpisah |

---

# LANJUTAN FINISHING (FIN-LGS-001 sampai 008)

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026 |
| Dasar | Master prompt "Lanjutkan Finishing Logistik V2 Sekaligus" |
| Titik awal | Backend `6302f5fc`, frontend `002f0bcd` (commit lokal; riwayatnya tidak diubah) |
| Batas | Tanpa commit, push, merge, PR, `dotnet build/test`, `npm run build/dev`, server, migrasi, maupun perubahan basis data. Pemeriksaan yang dijalankan: uji unit frontend (node) dan ESLint |

## Hasil per task

| Task | Hasil | Status |
| --- | --- | --- |
| FIN-LGS-001 Hardening | (1) **Pengulangan kunci idempotensi** kini hanya sah untuk permintaan yang sama, oleh pelaku yang sama, dengan isi yang sama; selain itu `409 PHM013`. Berlaku di tujuh perintah (buat, ubah, ajukan, batal, setuju/tolak/CITO, verifikasi, serah). (2) **Bug pembatalan diperbaiki:** sidik jari pembatalan hanya memuat alasan, sehingga kunci yang sama untuk permintaan lain dijawab "berhasil" padahal permintaan itu tidak dibatalkan. (3) **`Priority` di luar enum** pada buat/ubah ditolak 400; sebelumnya dapat tersimpan dan lolos dari aturan alasan CITO | COMPLETED_SOURCE |
| FIN-LGS-002 OQ-027/029 | Tidak ada perubahan kebijakan. Lembar keputusan D1–D7 dan W1–W3 (bagian *HASIL EKSEKUSI VERIFIKASI*) tetap berlaku. Butir keputusan baru di bawah | BLOCKED_DECISION |
| FIN-LGS-003 CITO, temuan, audit | (1) Detail memuat **`emergencyRequestReason`**, yaitu alasan CITO pemohon dari riwayat pengajuan, tanpa kolom baru. (2) Layar menampilkannya di **Informasi Kewenangan, sebelum panel keputusan**; sebelumnya baru terbaca setelah CITO diotorisasi. (3) **Log audit** ditambahkan untuk ubah draf (`StockRequest.Update`) dan pembatalan (`StockRequest.Cancel`, beserta status asal), dengan pelaku yang benar | COMPLETED_SOURCE |
| FIN-LGS-004 Kontrak | Saringan `status`, `priority`, `emergencyReviewState`, `emergencyReviewDueState`, dan `scope` memakai `[EnumDataType]`: nilai tak dikenal dijawab 400. Di service, nilai tak dikenal menghasilkan daftar kosong; sebelumnya `emergencyReviewState` tak dikenal ditafsirkan "tidak memerlukan verifikasi" dan `scope` tak dikenal sama dengan semua unit. Panjang teks bebas sudah dijaga `[MaxLength]` di DTO; parameter kosong dari layar dibuang `cleanParams`, sehingga `scope=` tidak pernah terkirim | COMPLETED_SOURCE |
| FIN-LGS-005 Tampilan | Alasan CITO pemohon di Informasi Kewenangan, memakai komponen dan gaya yang ada. Urutan bagian detail (dikunci uji FE-002) tidak berubah | COMPLETED_SOURCE |
| FIN-LGS-006 Uji dan UAT | Backend: kelas `HardeningTests` (+7 metode) dan kontrak nama properti diperluas. Frontend: +2 uji. Skenario UAT 17–21 di bawah. Tanpa data dummy operasional | Backend NOT_RUN; frontend lolos |
| FIN-LGS-007 Menu | 24 dari 66 entri tertaut. **"Pengajuan Kebutuhan Barang" ditautkan ke `/stock-requests/create`** (formulir nyata), sehingga kini 25. Sebelas grup dan 67 entri dipertahankan; tidak ada entri yang ditambah. 41 entri sisanya menunggu fitur atau keputusan (daftar di bawah) | COMPLETED_SOURCE / sebagian BLOCKED |
| FIN-LGS-008 Laporan | Bagian ini | — |

## Berkas yang berubah sejak commit lokal

| Repo | Berkas | Perubahan |
| --- | --- | --- |
| Backend | `Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs` | `[EnumDataType]` pada saringan dan `Priority`; field detail `EmergencyRequestReason` |
| Backend | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` | `EnsureSameReplay` di tujuh perintah; audit Update/Cancel; alasan CITO pemohon di detail; penjaga enum tak dikenal pada daftar |
| Backend | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/HardeningTests.cs` (baru) | 7 uji |
| Backend | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApiContractTests.cs` | Nama properti detail yang dibaca layar |
| Backend | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | *HASIL COMMIT LOKAL* dan bagian ini |
| Frontend | `src/utils/health-services/pharmacy-management/stock-request-approval-utils.js` | `getEmergencyRequestReason` |
| Frontend | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-authority-info.jsx` | Baris "Alasan CITO dari Pemohon" untuk permintaan CITO |
| Frontend | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-authorization-info.jsx` | Memakai `getEmergencyRequestReason` |
| Frontend | `src/utils/menu-sidebar/menu-items.jsx` | Satu baris: `pathname` untuk "Pengajuan Kebutuhan Barang" |
| Frontend | `tests/unit/stock-request-approval.test.mjs`, `tests/unit/logistics-module-navigation.test.mjs` | +2 uji |

Tidak ada perubahan pada modul lain, `appsettings`, `Program.cs`, csproj, `package.json`, migrasi, maupun basis data.

## Pemeriksaan yang benar-benar dijalankan

| Pemeriksaan | Hasil |
| --- | --- |
| Uji unit frontend `stock-request-approval` + `logistics-module-navigation` | **34/34 lolos** |
| ESLint pada 6 berkas frontend yang berubah | **0 error, 0 warning** |
| Diff `menu-items.jsx` (berkas bersama) | Tepat 1 baris tambahan |
| Keseimbangan kurung C# yang berubah | Seimbang |
| Kompatibilitas pemeriksaan pelaku | Kunci idempotensi frontend dibentuk dari waktu + acak per percobaan (`createIdempotencyKey`); tidak pernah dipakai bersama antar pengguna |
| Build dan uji backend | **NOT_RUN** (dilarang pada tahap ini). Inventaris kini **101 metode / 115 kasus**; 108 kasus lama lolos pada versi sebelum perubahan ini. Perubahan tahap ini belum di-compile |

## Menu dan fitur yang masih terblokir

| Entri | Sebab |
| --- | --- |
| Verifikasi Susulan CITO (`/stock-requests/emergency-reviews`) | Fitur dan route ada, tetapi **tidak ada entri menu**; rekonsiliasi 6.1 mencatat tidak ada entri yang ditambah dari daftar asli. Saat ini dicapai lewat strip "Perlu Perhatian Anda". **Butuh keputusan pemrakarsa:** tambah entri di grup "Permintaan Barang Unit" atau tetap tanpa menu |
| Riwayat Permintaan, Status Pemenuhan, Pemeriksaan Stok | Tidak ada halaman yang berbeda dari "Data Permintaan"; menautkannya ke halaman yang sama hanya menggandakan menu. Butuh definisi isi masing-masing |
| Penerimaan Barang (5 entri) | `LGS-OQ-017` (Goods Receipt → saldo gudang) |
| Penyimpanan & Penempatan (4), Distribusi (5), Mutasi & Retur (3) | Fitur belum ada atau menunggu `LGS-OQ-003`, `013` |
| Stock Opname & Penyesuaian (6) | `LGS-OQ-009`, butuh migrasi |
| Pengendalian: Rekomendasi Pengisian Ulang, Kebutuhan Pengadaan, Stok Lambat Bergerak | `LGS-OQ-011`, `014` |
| Laporan & Rekonsiliasi (8), Dashboard Penerimaan & Pengeluaran, Stok per Unit, Mutasi Saldo, Penanggung Jawab Gudang | `LGS-OQ-008`, `012`, `013`, `003`; sebagian butuh migrasi |

## Open Question yang memerlukan keputusan

| ID | Ringkas |
| --- | --- |
| `LGS-OQ-027` | Baca lintas unit (D1–D7) |
| `LGS-OQ-029` | Tulis lintas unit dan sumber petugas peminta (W1–W3) |
| Usulan baru (belum dicatat di decision log) | Entri menu untuk Verifikasi Susulan CITO; definisi "Riwayat Permintaan" dan "Status Pemenuhan" |
| `LGS-OQ-024`, `025`, `026`, `028` | Notifikasi, tindak lanjut temuan, penunjukan CITO lewat konfigurasi vs master, izin kesiapan |
| `LGS-OQ-001`..`019` (kecuali yang ditutup) | Keputusan operasional Logistik lainnya |

## Skenario UAT tambahan

| No | Skenario | Harapan |
| --- | --- | --- |
| 17 | Petugas CITO membuka permintaan CITO yang menunggu | Informasi Kewenangan menampilkan "Alasan CITO dari Pemohon" sebelum panel Otorisasi CITO |
| 18 | Pengguna B mengirim ulang permintaan setuju dengan kunci milik pengguna A (lewat alat API) | `409 PHM013` "dipakai pengguna lain"; tidak ada riwayat baru |
| 19 | Kunci pembatalan yang sama dipakai untuk permintaan lain | `409 PHM013`; permintaan kedua tetap berstatus semula |
| 20 | `GET /stock-requests?scope=9` atau `priority=9` | `400`, bukan daftar yang ditafsirkan |
| 21 | Menu Logistik → Permintaan Barang Unit → Pengajuan Kebutuhan Barang | Membuka formulir pembuatan permintaan |

## Status kesiapan UAT

**NO-GO**, tidak berubah. Penyebabnya:

- data organisasi, izin, jabatan CITO, dan tenggat belum diisi (BLOCKED_DATA);
- `LGS-OQ-027`/`029` belum diputuskan;
- konkurensi PostgreSQL belum diuji;
- perubahan backend tahap ini belum di-build dan diuji.

## Rangkuman untuk satu tahap commit dan push oleh pemilik proyek

Sebelum commit, jalankan build dan uji terisolasi (butir 6–7 *FINAL VERIFICATION*). Targetnya 115/115.

**Backend (`Ikbal`), 5 berkas:**

```text
Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApiContractTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/HardeningTests.cs
docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md
```

```text
fix(logistik): pengerasan permintaan stok — replay idempotensi, validasi enum, alasan CITO

- Kunci idempotensi hanya dapat diulang untuk permintaan, pelaku, dan isi yang sama (PHM013);
  memperbaiki pembatalan yang dijawab berhasil untuk permintaan lain.
- Saringan dan prioritas di luar enum ditolak 400; nilai tak dikenal tidak lagi ditafsirkan.
- Detail memuat alasan CITO pemohon (emergencyRequestReason) dari riwayat pengajuan.
- Log audit untuk ubah draf dan pembatalan dengan pelaku yang benar.
- Uji: HardeningTests (+7); laporan FIN-LGS diperbarui.

Tanpa entity, tabel, maupun migration baru.
```

**Frontend (`Ikbalv2`), 6 berkas:**

```text
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-authority-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-authorization-info.jsx
src/utils/health-services/pharmacy-management/stock-request-approval-utils.js
src/utils/menu-sidebar/menu-items.jsx
tests/unit/logistics-module-navigation.test.mjs
tests/unit/stock-request-approval.test.mjs
```

```text
feat(logistik): alasan CITO pemohon sebelum otorisasi, tautan Pengajuan Kebutuhan Barang

- Informasi Kewenangan menampilkan alasan CITO pemohon sebelum panel keputusan.
- Menu Pengajuan Kebutuhan Barang membuka formulir permintaan yang sudah ada.
- Uji unit: 34/34.
```

Berkas yang tidak boleh ikut tetap sama:

- backend: `SeedData/ICD10/*`, `Storage/uploads/*`;
- frontend: `test-results/.last-run.json`, `tests/e2e/zz-*`.

**Urutan:** backend dahulu. Frontend membaca `emergencyRequestReason`, tetapi tetap berfungsi
dengan backend lama karena memakai riwayat sebagai cadangan.

Kedua commit lokal sebelumnya (`6302f5fc`, `002f0bcd`) ikut ter-push bersama commit baru ini.

> **Digantikan** oleh daftar gabungan di bagian *LANJUTAN FINISHING 2* di bawah.

---

# LANJUTAN FINISHING 2 (FIN-LGS-009 sampai 013)

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026 |
| Titik awal | Backend `6302f5fc` + 5 berkas; frontend `002f0bcd` + 6 berkas (bagian sebelumnya) |
| Batas | Tanpa commit, push, merge, PR, `dotnet build/test`, `npm run build/dev`, test runner, ESLint, migrasi, maupun basis data. Pemeriksaan hanya statis |

## 1. Task yang selesai

| Task | Hasil | Status |
| --- | --- | --- |
| FIN-LGS-009 Keamanan | Perubahan replay, pembatalan, audit, dan validasi saringan ditinjau ulang; tidak ditemukan masalah baru. **Uji replay kini mencakup ketujuh perintah** (`IdempotencyReplayTests`, teori 7 kasus: buat, ubah, ajukan, batal, keputusan, verifikasi, serah). **Audit Create/Update kini diuji** dengan master obat uji di harness (celah "Create belum diuji" tertutup). OQ-027/029 tetap OPEN | COMPLETED_SOURCE |
| FIN-LGS-010 CITO | Alasan CITO tampil sebelum otorisasi (bagian sebelumnya). Tenggat dan status verifikasi memakai ambang yang sama di daftar, detail, dan ringkasan (sudah diuji). **Tanpa perubahan stok ganda**: uji baru membuktikan penyerahan yang diulang dengan kunci sama maupun dikirim lagi dengan kunci baru tidak mengurangi stok kedua kali. **Linimasa detail menampilkan alasan/catatan setiap langkah** (alasan CITO, penolakan, otorisasi, catatan temuan, pembatalan) dari riwayat yang sudah ada | COMPLETED_SOURCE |
| FIN-LGS-011 Halaman | **Riwayat Permintaan** = linimasa aktivitas (satu baris per kejadian) dari endpoint baru `GET /stock-requests/histories`, dengan saringan tanggal, unit, pemohon, status tujuan, dan nomor. **Status Pemenuhan** = varian daftar dengan progres Diajukan → Diputus → Diserahkan dan pemenuhan sebagian (`decisionAt`, `fulfilledAt`, `shortfallItemCount` dari riwayat), saringan pemenuhan sebagian, tanpa draf. **Saringan pemohon** juga ditambahkan ke Data Permintaan dan antrean gudang. **Verifikasi Susulan CITO** tetap diakses lewat tombol di halaman Validasi Atasan (entri menu yang ada); tidak ada entri baru | COMPLETED_SOURCE |
| FIN-LGS-012 Integrasi | Endpoint, DTO, saringan, halaman, dan status dicocokkan (tabel di bawah). Menu: "Status Pemenuhan" dan "Riwayat Permintaan" ditautkan ke route nyata. **67 entri / 66 entri klik dan 11 grup tetap**; tertaut kini 27, belum 39 | COMPLETED_SOURCE |
| FIN-LGS-013 Persiapan uji | Inventaris dan daftar periksa di bawah. Tidak ada klaim PASS untuk perubahan ini | NOT_RUN |

**Temuan yang dicatat, tidak diubah:** `FulfillAsync` dan `CancelAsync` mengisi kolom `DecidedAt` dengan waktu penyerahan/pembatalan (perilaku sejak `aec9842e`), sehingga kolom itu tidak lagi berarti waktu keputusan atasan. Kolom ini tidak dikirim ke layar; progres kini dibaca dari riwayat. Mengubah arti kolom dicatat sebagai **BLOCKED_DECISION** (LOW), karena dapat memengaruhi laporan yang kelak membacanya.

## 2. Berkas yang berubah

**Backend (`Ikbal`), 9 berkas sejak `6302f5fc`:**

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` | `GET histories` (izin `StockRequest:Read`) |
| `Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs` | `[EnumDataType]`; `EmergencyRequestReason`; saringan `RequestedByWorkforceId`, `ExcludeDraft`, `PartiallyFulfilledOnly`; `DecisionAt`, `FulfilledAt`, `ShortfallItemCount`; `StockRequestHistoryPagedQuery`, `StockRequestHistoryEntryResponse` |
| `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` | `EnsureSameReplay`; audit Update/Cancel; alasan CITO; penjaga enum; saringan dan progres; `GetHistoryPagedAsync` |
| `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs` | Master obat/satuan uji, `PemohonWorkforceId`, `StafWorkforceId`, parameter peminta |
| `Tests/.../StockRequestApprovalTests/ApiContractTests.cs` | Nama properti detail |
| `Tests/.../StockRequestApprovalTests/HardeningTests.cs` (baru) | 7 uji |
| `Tests/.../StockRequestApprovalTests/IdempotencyReplayTests.cs` (baru) | 3 metode / 9 kasus |
| `Tests/.../StockRequestApprovalTests/ProgressAndHistoryTests.cs` (baru) | 5 uji |
| `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | Laporan |

**Frontend (`Ikbalv2`), 17 berkas sejak `002f0bcd`:**

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/pharmacy-management/stock-requests/history/page.jsx`, `.../history/stock-request-history-log-client.jsx` (baru) | Route Riwayat Permintaan |
| `src/app/health-services/pharmacy-management/stock-requests/fulfillment/page.jsx`, `.../fulfillment/stock-request-fulfillment-client.jsx` (baru) | Route Status Pemenuhan |
| `src/components/view/.../stock-request-view/stock-request-history-log-view.jsx` (baru) | Linimasa aktivitas |
| `src/lib/hooks/.../use-stock-request-history-log.jsx` (baru) | Hook linimasa |
| `src/components/view/.../stock-request-view/stock-request-list-view.jsx` | Varian `fulfillment`, saringan pemohon dan pemenuhan sebagian |
| `src/components/view/.../stock-request-view/stock-request-detail-view.jsx` | Alasan/catatan per langkah riwayat |
| `src/components/view/.../components/stock-request-decision-authority-info.jsx`, `.../stock-request-emergency-authorization-info.jsx` | Alasan CITO pemohon (bagian sebelumnya) |
| `src/lib/constants/.../stock-request-constant.jsx` | Route, tahap progres, pilihan status tanpa draf |
| `src/lib/hooks/.../use-stock-request-list.jsx` | Saringan baru |
| `src/lib/services/.../stock-request.service.js` | `getStockRequestHistories` |
| `src/utils/.../stock-request-approval-utils.js` | `getEmergencyRequestReason`, `resolveFulfillmentProgress` |
| `src/utils/menu-sidebar/menu-items.jsx` | 3 baris `pathname` (Pengajuan Kebutuhan Barang, Status Pemenuhan, Riwayat Permintaan) |
| `tests/unit/stock-request-approval.test.mjs`, `tests/unit/logistics-module-navigation.test.mjs` | Uji baru |

Tidak ada perubahan modul lain, `appsettings`, `Program.cs`, csproj, `package.json`, migrasi, maupun basis data.

## 3. Hasil pemeriksaan aktual (statis)

| Pemeriksaan | Hasil |
| --- | --- |
| Parse sintaks 17 berkas JS/JSX/MJS frontend (`@babel/parser`, tanpa ESLint dan tanpa runner) | **17/17 lolos** |
| Keseimbangan kurung 8 berkas C# | Seimbang |
| Pencocokan pola uji frontend baru terhadap source | Semua pola ditemukan di source (ditinjau manual) |
| Asersi uji lama yang menyentuh berkas berubah | Tetap terpenuhi (urutan detail, varian approval/warehouse, potongan kolom persetujuan, isi hook) |
| Jumlah entri menu Logistik | 67 (tidak bertambah); tertaut 27, belum 39 |
| Diff `menu-items.jsx` | Hanya 3 baris `pathname` |
| Kunci idempotensi frontend | Waktu + acak per percobaan, sehingga pemeriksaan pelaku tidak mengganggu |
| Pemindaian kredensial | 0 di kedua repo |
| **Tidak dijalankan** | `dotnet build`, `dotnet test`, test runner frontend, ESLint. Hasil terakhir yang benar-benar dijalankan: backend 108/108 (versi sebelum dua tahap terakhir), frontend 34/34 dan ESLint 0 error (sebelum tahap ini) |

### Kontrak backend–frontend tahap ini

| Kontrak | Backend | Frontend |
| --- | --- | --- |
| `GET /stock-requests/histories` | `StockRequestHistoryPagedQuery` → `PagedResult<StockRequestHistoryEntryResponse>`, `StockRequest:Read`, urut `occurredAt` menurun | `getStockRequestHistories` → `useStockRequestHistoryLog` → `StockRequestHistoryLogView` |
| Saringan daftar baru | `requestedByWorkforceId`, `excludeDraft`, `partiallyFulfilledOnly` | `useStockRequestList` (nilai kosong dibuang `cleanParams`) |
| Field progres | `decisionAt`, `fulfilledAt`, `shortfallItemCount` (daftar dan detail) | `resolveFulfillmentProgress` |
| Alasan CITO | `emergencyRequestReason` | `getEmergencyRequestReason` (cadangan: riwayat) |
| Nilai enum tak dikenal | 400 (DTO); daftar kosong (service) | Layar hanya mengirim pilihan sah |

## 4. Blocker yang tersisa

| Jenis | Blocker |
| --- | --- |
| BLOCKED_DECISION | `LGS-OQ-027` baca lintas unit (D1–D7), **juga berlaku untuk endpoint linimasa baru**; `LGS-OQ-029` tulis lintas unit (W1–W3); `LGS-OQ-024`, `025`, `026`, `028`; arti kolom `DecidedAt`; entri menu tersendiri untuk Verifikasi Susulan CITO (saat ini lewat Validasi Atasan); isi "Pemeriksaan Stok" |
| BLOCKED_DATA | Tautan unit 0/20, penempatan manajerial 0, izin 0 jabatan, jabatan CITO, tenggat, batas aktivasi |
| BLOCKED_MIGRATION | Tindak lanjut temuan CITO (PIC/status/penutupan); aktivasi batas; notifikasi |
| BLOCKED_SANDBOX | Konkurensi PostgreSQL |
| Menu belum tertaut (39) | Penerimaan (5), Penyimpanan (4), Distribusi (5), Mutasi & Retur (3), Stock Opname (6), Pengendalian (3), Laporan (8), Dashboard (1), Master (1), Stok (2), Pemeriksaan Stok (1) — menunggu fitur atau keputusan operasional |

## 5. Kesiapan pengujian dan UAT

**Backend:** 109 metode / **129 kasus** (sebelumnya 94/108). Belum di-build maupun dijalankan; jangan diklaim PASS sebelum keduanya berjalan.

```powershell
# Prasyarat: backend tidak berjalan; satu build sendirian di foreground.
dotnet build .\StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true *> build.log
dotnet test .\StockRequestApprovalIsolated.Tests.csproj --no-build --filter "FullyQualifiedName~StockRequestApprovalTests" --logger "console;verbosity=normal" *> test.log
```

Proyek terisolasi meng-compile `StockRequestApprovalTests\*.cs`, sehingga tiga kelas baru otomatis ikut. Uji dengan risiko tertinggi karena menyentuh jalur yang belum pernah diuji:

- `Penyerahan_mengurangi_stok_sekali...` (jalur `DrugStockService` FEFO);
- teori `Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain("buat"/"ubah")` (master obat uji).

**Frontend:**

```text
node --import ./tests/helpers/register.mjs --test tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs
npx eslint <17 berkas di atas>
```

Target 39 uji. Peringatan `react-hooks/set-state-in-effect` pada hook baru mengikuti pola hook yang ada.

**Daftar periksa keamanan:**

1. Kunci milik pengguna lain → `409 PHM013` pada ketujuh perintah.
2. Kunci pembatalan untuk permintaan lain → `409`; permintaan kedua tidak berubah.
3. `scope`, `status`, `priority`, `emergencyReviewState`, `emergencyReviewDueState`, `toStatus` di luar enum → `400`.
4. Log audit: pelaku = akun login pada buat, ubah, ajukan, setuju/tolak/CITO, verifikasi, serah, batal, penolakan kewenangan.
5. `GET /histories` dengan `scope=1` → hanya unit dalam kewenangan; tanpa login → `401`.
6. SuperAdmin tanpa penempatan → tidak dapat memutus, mengotorisasi, atau memverifikasi.
7. Konfigurasi tenggat `1e300` → endpoint tetap `200`.

**Skenario UAT tambahan:**

| No | Skenario | Harapan |
| --- | --- | --- |
| 22 | Menu → Riwayat Permintaan | Linimasa terbaru lebih dahulu; nama petugas dan alasan per kejadian tampil |
| 23 | Riwayat Permintaan: saring status tujuan "Ditolak", unit, pemohon, tanggal | Jumlah dan halaman berubah sesuai saringan |
| 24 | Menu → Status Pemenuhan | Tidak ada draf; setiap baris menampilkan tahap dan waktu Diajukan/Diputus/Diserahkan |
| 25 | Status Pemenuhan: saring "Hanya yang diserahkan sebagian" | Hanya permintaan selesai yang kurang; jumlah item kurang tampil |
| 26 | Detail permintaan yang ditolak atau bertemuan CITO | Linimasa menampilkan alasan penolakan / catatan temuan |
| 27 | Penyerahan dikirim dua kali (klik ganda / ulang) | Stok berkurang sekali; kiriman kedua `409` |
| 28 | Validasi Atasan → tombol Verifikasi Susulan CITO | Halaman verifikasi terbuka tanpa entri menu baru |

**UAT: NO-GO**, tidak berubah. Penyebabnya:

- BLOCKED_DATA;
- `LGS-OQ-027`/`029`;
- konkurensi PostgreSQL;
- build dan uji backend serta uji frontend tahap ini belum dijalankan.

## 6. Daftar gabungan untuk satu tahap commit dan push

Jalankan butir 5 lebih dahulu (target backend 129/129, frontend 39/39, ESLint 0 error).

**Backend (`Ikbal`), 9 berkas:**

```text
Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs
Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs
Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApiContractTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/HardeningTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/IdempotencyReplayTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ProgressAndHistoryTests.cs
docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md
```

```text
feat(logistik): pengerasan permintaan stok, linimasa aktivitas, dan progres pemenuhan

- Kunci idempotensi hanya dapat diulang untuk permintaan, pelaku, dan isi yang sama (PHM013)
  pada tujuh perintah; memperbaiki pembatalan yang dijawab berhasil untuk permintaan lain.
- Saringan dan prioritas di luar enum ditolak 400; nilai tak dikenal tidak ditafsirkan.
- Audit ubah draf dan pembatalan; alasan CITO pemohon di detail.
- GET /stock-requests/histories: linimasa aktivitas (tanggal, unit, pemohon, status tujuan).
- Daftar: saringan pemohon, tanpa draf, pemenuhan sebagian; progres decisionAt/fulfilledAt/
  shortfallItemCount dari riwayat.
- Uji: 109 metode / 129 kasus StockRequestApprovalTests.

Tanpa entity, tabel, maupun migration baru.
```

**Frontend (`Ikbalv2`), 17 berkas:**

```text
src/app/health-services/pharmacy-management/stock-requests/fulfillment/page.jsx
src/app/health-services/pharmacy-management/stock-requests/fulfillment/stock-request-fulfillment-client.jsx
src/app/health-services/pharmacy-management/stock-requests/history/page.jsx
src/app/health-services/pharmacy-management/stock-requests/history/stock-request-history-log-client.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-authority-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-emergency-authorization-info.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-detail-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-history-log-view.jsx
src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-list-view.jsx
src/lib/constants/health-services/pharmacy-management/stock-request-constant.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-history-log.jsx
src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx
src/lib/services/health-services/pharmacy-management/stock-request.service.js
src/utils/health-services/pharmacy-management/stock-request-approval-utils.js
src/utils/menu-sidebar/menu-items.jsx
tests/unit/logistics-module-navigation.test.mjs
tests/unit/stock-request-approval.test.mjs
```

```text
feat(logistik): Riwayat Permintaan, Status Pemenuhan, dan alasan CITO sebelum otorisasi

- Riwayat Permintaan: linimasa aktivitas dari GET /stock-requests/histories.
- Status Pemenuhan: progres Diajukan → Diputus → Diserahkan dan pemenuhan sebagian.
- Saringan pemohon; alasan/catatan per langkah di detail; alasan CITO pemohon sebelum panel.
- Menu: Pengajuan Kebutuhan Barang, Status Pemenuhan, Riwayat Permintaan ditautkan
  (67 entri dan 11 grup tetap).
```

Berkas yang tidak boleh ikut:

- backend: `SeedData/ICD10/*`, `Storage/uploads/*`;
- frontend: `test-results/.last-run.json`, `tests/e2e/zz-*`.

**Urutan:** backend lebih dahulu. Frontend memanggil `GET /histories` dan membaca field progres baru; dengan backend lama halaman Riwayat Permintaan akan menjawab 404. Kedua commit lokal sebelumnya (`6302f5fc`, `002f0bcd`) ikut ter-push.

> **Digantikan** oleh *KONSOLIDASI AKHIR (FIN-LGS-014 sampai 019)* di bawah.

---

# KONSOLIDASI AKHIR (FIN-LGS-014 sampai 019)

| Field | Nilai |
| --- | --- |
| Tanggal | 18 November 2026 |
| Titik awal | Backend `6302f5fc` + 9 berkas; frontend `002f0bcd` + 17 berkas (*LANJUTAN FINISHING 2*) |
| Batas | Tanpa `dotnet build/test`, `npm run build/dev`, server, migrasi, maupun basis data. Pemeriksaan hanya statis. Commit dan push dilakukan di akhir atas instruksi pemilik proyek ("kalo sudah nanti push saja"), hanya ke branch kerja `Ikbal` dan `Ikbalv2`; tidak ke integration, tanpa PR, tanpa force |

## 1. Status task

| Task | Hasil | Status |
| --- | --- | --- |
| FIN-LGS-014 Konsistensi uji | Delapan area ditinjau terhadap implementasi: idempotensi tujuh perintah, replay antarpengguna, FEFO tanpa pengurangan ganda, master obat pada buat/ubah, audit buat/ubah/batal, endpoint histories, saringan tanggal/unit/pemohon/status, pemenuhan sebagian. Jalur FEFO hanya membaca saldo, batch, dan kedaluwarsa (`LoadFefoCandidatesAsync`) lalu menambah mutasi; prasyaratnya dipenuhi `SediakanStokAsync`. Buat/ubah hanya memerlukan `MstDrug` dan `MstMeasurement` aktif. Perbaikan uji: (1) cast eksplisit `DateTime?`/`decimal?` pada `Assert.Equal`, mengikuti pola yang ada, untuk menghindari ambiguitas overload; (2) uji saringan pemohon pada linimasa kini mengasersikan jumlah, bukan hanya `Assert.All` yang lolos pada daftar kosong | COMPLETED_SOURCE |
| FIN-LGS-015 `DecidedAt` | Diperbaiki (butir 3) | COMPLETED_SOURCE; data lama BLOCKED_DATA |
| FIN-LGS-016 Riwayat | Urutan terbaru lebih dahulu dengan pemecah seri `Id`; pemohon (`requestedByName`, dari profil peminta) dan petugas (`actorName`, dari pelaku kejadian) terpisah; saringan dan halaman di backend; satu baris per riwayat (join satu-ke-satu); identitas dengan aturan yang sama seperti detail; riwayat tidak pernah dihapus karena perubahan status (append-only). **Uji baru membuktikan linimasa tidak membuka permintaan di luar yang terlihat pada daftar**, untuk empat peran × dua cakupan. Kebijakan `LGS-OQ-027` tidak diubah | COMPLETED_SOURCE |
| FIN-LGS-017 Status Pemenuhan | Delapan keadaan dibedakan tanpa status basis data baru (butir 3). `remainingQuantity` per baris dihitung backend, sehingga diminta − diserahkan = sisa selalu konsisten; daftar menampilkan "n dari m baris lengkap" (lengkap + kurang = jumlah item) | COMPLETED_SOURCE |
| FIN-LGS-018 Navigasi | Audit statis: **11 grup, 67 entri, 66 dapat diklik, 27 tertaut, 0 tautan tanpa halaman**. Tidak ada entri baru, entri dihapus, maupun halaman dummy. 39 entri lain BLOCKED_FEATURE (butir 5) | COMPLETED_SOURCE |
| FIN-LGS-019 Konsolidasi | Bagian ini | — |

## 2. Berkas yang berubah (gabungan sejak commit lokal)

**Backend (`Ikbal`), 10 berkas sejak `6302f5fc`:**

```text
Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs
Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs
Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs
Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApiContractTests.cs
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/DecisionLifecycleTests.cs   (baru, 5 uji)
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/HardeningTests.cs           (baru, 7 uji)
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/IdempotencyReplayTests.cs   (baru, 3 metode / 9 kasus)
Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ProgressAndHistoryTests.cs  (baru, 6 uji)
docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md
```

**Frontend (`Ikbalv2`), 17 berkas sejak `002f0bcd`:** daftar *LANJUTAN FINISHING 2* butir 6. Pada tahap ini
yang disunting lagi adalah konstanta tahap, `stock-request-approval-utils.js`, list view, detail
view, dan `stock-request-approval.test.mjs`. Tidak ada berkas frontend baru.

## 3. Perbaikan `DecidedAt` (FIN-LGS-015)

| Hal | Isi |
| --- | --- |
| Penulisan sebelumnya | `DecideAsync` (setuju/tolak/CITO), `CancelAsync`, dan `FulfillAsync`. Dua yang terakhir juga menimpa `DecisionReason` dengan alasan batal/catatan serah |
| Lifecycle yang benar | Menurut `LGS-DEC-002`, keputusan adalah persetujuan, penolakan, atau otorisasi CITO oleh pihak berwenang. `DecidedAt`/`DecisionReason` kini **hanya ditulis `DecideAsync`** |
| Penyerahan / pembatalan | Tidak lagi menyentuh kolom keputusan. Waktunya diambil dari riwayat yang sudah ada: `FulfilledAt` dan **`CancelledAt` (baru di DTO, tanpa kolom baru)**; catatan dan alasannya tetap di riwayat. Pembatalan sebelum diputus dan penyerahan lewat aturan peralihan membiarkan `DecidedAt` kosong |
| Kontrak | `DecidedAt` tetap ada. Layar beralih ke `decisionAt` (riwayat) lewat `resolveDecisionTime`, yang tetap benar untuk baris lama. Label "Keputusan Terakhir Pada" diganti "Diputus Atasan Pada", ditambah "Diserahkan Pada" dan "Dibatalkan Pada" |
| Regresi | `DecisionLifecycleTests`: serah setelah setuju, batal setelah setuju, batal sebelum diputus, tolak, serah lewat peralihan |
| Data lama | **BLOCKED_DATA.** Baris yang diserahkan/dibatalkan sebelum perbaikan masih menyimpan waktu dan alasan penyerahan/pembatalan di `DecidedAt`/`DecisionReason`. Nilai benarnya dapat diturunkan dari `PhmStockRequestHistory`, tetapi koreksi otomatis tidak dijalankan. Butuh persetujuan pemilik data dan skrip yang ditinjau. Tampilan tidak terdampak karena memakai riwayat |

### Status Pemenuhan (FIN-LGS-017)

| Keadaan | Dasar (data yang sudah ada) |
| --- | --- |
| Menunggu Persetujuan | `Submitted` |
| Menunggu Penyerahan (aturan peralihan) | `Submitted` + `isLegacySubmission` |
| Disetujui · Menunggu Penyerahan | `Approved` |
| Ditolak | `Rejected` |
| Dipenuhi Sebagian | `Completed` + `shortfallItemCount > 0` |
| Selesai Diserahkan | `Completed` tanpa kekurangan |
| Dibatalkan | `Cancelled` (+ langkah "Dibatalkan" dengan `cancelledAt`) |
| CITO menunggu verifikasi susulan | Penanda terpisah dari `emergencyReviewState = Pending`, pada tahap mana pun |

## 4. Hasil pemeriksaan source dan uji

| Pemeriksaan | Hasil |
| --- | --- |
| Keseimbangan kurung 9 berkas C# yang berubah | Seimbang |
| Parse sintaks 17 berkas frontend (`@babel/parser`, bukan ESLint/runner) | 17/17 lolos |
| Audit navigasi statis | 11 / 67 / 66 / 27, 0 tautan tanpa halaman |
| Asersi uji lama yang terdampak | Tetap terpenuhi, termasuk pola `Number(row.requestedQuantity) - Number(row.fulfilledQuantity)` (dipertahankan sebagai cadangan) dan urutan bagian detail |
| Pencarian ketergantungan pada `DecidedAt`/`DecisionReason` lama | Tidak ada uji maupun layar Logistik yang bergantung padanya |
| Inventaris backend | **115 metode / 135 kasus**. Belum di-build maupun dijalankan; **tidak ada klaim PASS** |
| Inventaris frontend | **40 uji** di `stock-request-approval` + `logistics-module-navigation`; belum dijalankan |
| Hasil terakhir yang benar-benar dijalankan | Backend 108/108 (versi sebelum tiga tahap terakhir); frontend 34/34 dan ESLint 0 error (sebelum dua tahap terakhir) |

## 5. Blocker

| Jenis | Blocker |
| --- | --- |
| BLOCKED_DECISION | `LGS-OQ-027` (D1–D7, juga untuk linimasa); `LGS-OQ-029` (W1–W3); `LGS-OQ-024`, `025`, `026`, `028`; entri menu tersendiri Verifikasi Susulan CITO; isi "Pemeriksaan Stok" |
| BLOCKED_DATA | Koreksi `DecidedAt`/`DecisionReason` baris lama; tautan unit 0/20; penempatan manajerial 0; izin 0 jabatan; jabatan CITO, tenggat, batas aktivasi |
| BLOCKED_MIGRATION | Tindak lanjut temuan CITO; aktivasi batas; notifikasi |
| BLOCKED_SANDBOX | Konkurensi PostgreSQL |
| BLOCKED_FEATURE (39 entri menu) | Penerimaan Barang (5: Pemeriksaan Jumlah & Kondisi, Pencatatan Batch/Lot, Tanggal Kadaluarsa, Barang Tidak Sesuai, Dokumen Penerimaan); Penyimpanan & Penempatan (4); Distribusi & Pengeluaran (5); Mutasi & Retur (3); Stock Opname & Penyesuaian (6); Pengendalian (3: Rekomendasi Pengisian Ulang, Kebutuhan Pengadaan, Stok Lambat Bergerak); Laporan & Rekonsiliasi (8); Dashboard Penerimaan & Pengeluaran; Penanggung Jawab Gudang; Stok per Unit; Mutasi Saldo Persediaan; Pemeriksaan Stok |
| Teknis | Perubahan tiga tahap terakhir belum pernah di-compile. `PharmacyTests` tetap tidak terkompilasi karena uji Billing modul lain, sehingga uji Logistik dijalankan lewat proyek terisolasi |

## 6. Kesiapan UAT

**NO-GO.** Penyebabnya:

- BLOCKED_DATA;
- `LGS-OQ-027`/`029`;
- konkurensi PostgreSQL;
- build dan uji backend (target 135/135) serta uji frontend (target 40) dan ESLint belum dijalankan.

Skenario UAT tambahan:

| No | Skenario | Harapan |
| --- | --- | --- |
| 29 | Permintaan disetujui lalu diserahkan; buka detail | "Diputus Atasan Pada" = waktu persetujuan; "Diserahkan Pada" terisi; keduanya berbeda |
| 30 | Permintaan disetujui lalu dibatalkan | "Dibatalkan Pada" terisi; waktu persetujuan tetap |
| 31 | Status Pemenuhan: CITO diotorisasi yang belum diverifikasi | Tahap "Disetujui · Menunggu Penyerahan" + penanda "Menunggu Verifikasi Susulan" |
| 32 | Status Pemenuhan: permintaan selesai sebagian | "Dipenuhi Sebagian", "n dari m baris lengkap"; detail: sisa = diminta − diserahkan |

## 7. Rangkuman commit dan push

Atas instruksi pemilik proyek, perubahan di atas di-commit dan di-push di akhir tahap ini:

- backend `Ikbal`: 10 berkas, dahulu;
- frontend `Ikbalv2`: 17 berkas, sesudahnya.

SHA commit tercatat di riwayat git masing-masing branch. Push dilakukan sebagai fast-forward ke branch kerja, bukan ke integration, tanpa PR.

Berkas yang tidak ikut:

- backend: `SeedData/ICD10/*`, `Storage/uploads/*`;
- frontend: `test-results/.last-run.json`, `tests/e2e/zz-*`.

**Langkah berikutnya** (butuh persetujuan terpisah):

1. Build dan uji terisolasi backend (target 135/135).
2. Uji frontend (40) dan ESLint.
3. PR baru ke integration, backend dahulu.

---

# FINAL QUALITY GATE

| Field | Nilai |
| --- | --- |
| Tanggal | 19 November 2026 |
| Lingkup | Persiapan verifikasi akhir FIN-LGS-001 sampai 019 |
| Batas | Butir 1–5 disusun dengan pemeriksaan statis. **Butir 6 adalah eksekusi nyata** (build, uji backend, uji frontend, ESLint) pada 19 November 2026. Tanpa server, migrasi, basis data, commit, push, maupun PR. Bagian ini adalah satu-satunya perubahan, belum di-commit |
| Status runtime | **Uji otomatis terverifikasi untuk `a3b2ed08`/`109a4083`**: build PASS, backend 135/135 (SQLite), frontend 40/40, ESLint 0 error (butir 6). Aplikasi berjalan, UAT, dan konkurensi PostgreSQL **belum** |

## 1. Isi commit dan sinkronisasi

| Repo | Commit | Berkas | Sesuai laporan | Remote | Working tree |
| --- | --- | --- | --- | --- | --- |
| Backend `Ikbal` | `6302f5fc` | 16 | Ya | `origin/Ikbal` = `a3b2ed08` | Bersih dari Logistik; tersisa `SeedData/ICD10/*` dan `Storage/uploads/*` milik orang lain |
| Backend `Ikbal` | `a3b2ed08` | 10 | Ya (*KONSOLIDASI AKHIR* butir 2) | sama | sama |
| Frontend `Ikbalv2` | `002f0bcd` | 17 | Ya | `origin/Ikbalv2` = `109a4083` | Bersih dari Logistik; tersisa `test-results/.last-run.json` dan 16 `tests/e2e/zz-*` |
| Frontend `Ikbalv2` | `109a4083` | 17 | Ya (*LANJUTAN FINISHING 2* butir 2) | sama | sama |

Integration bergerak lagi, tetapi tidak beririsan dengan Logistik:

| Repo | Integration | Perubahan baru | Irisan berkas Logistik | `git merge-tree` |
| --- | --- | --- | --- | --- |
| Backend | `659d4635` | +8 commit sejak `bc14e6d1` (Payroll/BPJS, PR #359) | 0 | Bersih |
| Frontend | `cb5174327` | Termasuk `menu-items.jsx` | `menu-items.jsx` disentuh, tetapi hanya baris 1458–1820 (Payroll); bagian Logistik ada di baris 2775–3240 | Bersih |

Konfigurasi entity baru di integration hanya memakai `CURRENT_TIMESTAMP` dan indeks bersyarat `"IsDelete" = false`; keduanya sudah ditangani `ModelUjiSqlite`. Sinkronisasi dengan integration sebelum uji diperkirakan tidak mengganggu SQLite (penilaian statis).

## 2. Kesiapan build dan uji

### Inventaris (dihitung dari commit yang di-push)

| Kelas | Metode | Kasus | Fokus |
| --- | --- | --- | --- |
| `ApiContractTests` | 5 | 5 | Kode HTTP, enum frontend, nama properti, tenggat tidak 500 |
| `ApprovalScopeTests` | 15 | 17 | Unit yang Saya Pimpin, penunjukan CITO, tenggat di daftar |
| `AuthorizationTests` | 13 | 13 | Izin, atasan unit, SuperAdmin, memutus sendiri |
| `ConcurrencyTests` | 6 | 6 | Token versi (SQLite, **bukan** PostgreSQL) |
| `CutoverUnitTests` | 8 | 11 | Batas aktivasi |
| `DecisionLifecycleTests` | 5 | 5 | **Regresi `DecidedAt`** |
| `EmergencyReviewTests` | 12 | 12 | Verifikasi susulan CITO |
| `HardeningTests` | 7 | 7 | Replay, pembatalan, enum, alasan CITO |
| `IdempotencyReplayTests` | 3 | 9 | **Tujuh perintah**, audit buat/ubah, **FEFO tanpa ganda** |
| `OperationalSettingsUnitTests` | 6 | 14 | Tenggat dan penunjukan |
| `ProgressAndHistoryTests` | 6 | 6 | **Status Pemenuhan, linimasa, pembatasan data** |
| `RobustnessAndAuditTests` | 12 | 12 | Data tidak berlaku, audit |
| `StatusTransitionTests` | 17 | 18 | Transisi dan peralihan |
| **Total backend** | **115** | **135** | Belum dijalankan |
| Frontend (`stock-request-approval` + `logistics-module-navigation`) | 40 uji | — | Belum dijalankan |

### Pemeriksaan statis area kritis

| Area | Uji penjaga | Kesesuaian dengan implementasi |
| --- | --- | --- |
| Regresi `DecidedAt` | `DecisionLifecycleTests` (5) | `DecidedAt`/`DecisionReason` hanya ditulis `DecideAsync`; `CancelAsync` dan `FulfillAsync` tidak lagi menyentuhnya; `CancelledAt`/`FulfilledAt` dari riwayat |
| Idempotensi tujuh perintah | `IdempotencyReplayTests` (teori 7) | Ketujuh titik memanggil `EnsureSameReplay` sebelum memuat entity; kunci frontend unik per percobaan |
| FEFO tanpa ganda | `Penyerahan_mengurangi_stok_sekali...` | `LoadFefoCandidatesAsync` hanya butuh saldo Available, batch belum kedaluwarsa, dan `QuantityOnHand > 0`, yang dipenuhi `SediakanStokAsync`; kiriman kedua ditolak `PHM004` sebelum stok disentuh |
| Riwayat | `ProgressAndHistoryTests` (4 uji linimasa) | Join satu-ke-satu, urutan menurun + pemecah seri `Id`, saringan sebelum penghitungan |
| Status Pemenuhan | `ProgressAndHistoryTests` + uji frontend FIN-LGS-017 | Delapan keadaan dari status/riwayat; `remainingQuantity` dan `shortfallItemCount` dari baris yang sama |
| Pembatasan data | `Linimasa_tidak_membuka_permintaan_di_luar...` (4 peran × 2 cakupan) | Linimasa memakai `ApplyAuthorityScopeAsync` yang sama dengan daftar |

### Risiko compile dan runtime (belum pernah di-compile: isi `a3b2ed08`)

| Risiko | Penilaian |
| --- | --- |
| Collection expression `[...]` pada `Assert.Equal` dan `Validator.TryValidateObject` | Rendah. Pola yang sama sudah terbukti compile di `PatientManagementTests` (xunit 2.9, net9) |
| Cast `(DateTime?)`/`(decimal?)` pada `Assert.Equal` | Rendah. Mengikuti pola `(decimal?)8m` yang ada |
| Perbandingan `decimal` di query SQLite (`FulfilledQuantity < RequestedQuantity`) | Rendah. Pola yang sama sudah dipakai proyeksi `IsPartiallyFulfilled` pada setiap `GetPagedAsync` yang lolos 108/108 (EF Core menerjemahkannya dengan `ef_compare`) |
| Uji yang menyentuh jalur baru | Sedang. `Penyerahan_mengurangi_stok_sekali...` adalah uji pertama Logistik yang menjalankan `DrugStockService` FEFO; teori "buat"/"ubah" pertama kali memakai master obat uji |
| Uji frontend | Rendah–sedang. 17 berkas lolos parse; pola regex uji ditinjau manual terhadap source |

### Prosedur verifikasi terisolasi (butuh izin pengujian)

Prasyarat:

1. Tidak ada backend yang berjalan (DLL `C:\tmp\phm\bin` tidak terkunci). Periksa dengan `tasklist | findstr QuilvianSystemBackend`.
2. Satu build, sendirian, di foreground.
3. Proyek terisolasi ada di `%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\`. Isinya dapat dibuat ulang dari *FINAL VERIFICATION* butir 6 bila folder sementara itu hilang.

```powershell
# Backend — SQLite dalam memori; tidak membaca appsettings, tidak menjalankan host, migrasi, atau seeder.
dotnet build .\StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true *> build5.log
dotnet test  .\StockRequestApprovalIsolated.Tests.csproj --no-build --filter "FullyQualifiedName~StockRequestApprovalTests" --logger "console;verbosity=normal" --logger "trx;LogFileName=run5.trx" *> test5.log

# Frontend — node test runner dan ESLint; tanpa build, dev server, maupun jaringan.
node --import ./tests/helpers/register.mjs --test tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs
npx eslint <berkas Logistik dari 002f0bcd dan 109a4083>
```

Kriteria lolos:

- backend `0 Error(s)` dan **135/135**;
- frontend **40/40**;
- ESLint 0 error;
- log uji tidak memuat `Npgsql`, `Host=`, maupun port 5432.

Bila ada yang gagal: perbaiki hanya penyebab Logistik, ulangi, lalu jadikan commit baru (riwayat `a3b2ed08`/`109a4083` tidak diubah).

## 3. Risiko keamanan

| Severity | Risiko | Status |
| --- | --- | --- |
| MEDIUM | Baca lintas unit pada daftar, detail, dan linimasa bagi pemegang `StockRequest:Read` (termasuk SuperAdmin lewat filter platform) | BLOCKED_DECISION `LGS-OQ-027` |
| MEDIUM | Tulis lintas unit; petugas peminta dipilih dari layar | BLOCKED_DECISION `LGS-OQ-029` |
| MEDIUM | Integration masih memuat log audit dengan id permintaan sebagai pelaku, sampai backend Logistik ini di-merge | Menunggu PR |
| LOW | `DecidedAt`/`DecisionReason` baris lama masih berisi waktu/alasan penyerahan atau pembatalan | BLOCKED_DATA; tampilan memakai riwayat |
| LOW | Penunjukan CITO dan tenggat lewat konfigurasi; laporan kesiapan terbuka di Development | `LGS-OQ-026`, `028` |
| Tertutup (belum diuji runtime) | Replay antarpengguna; pembatalan yang dijawab berhasil untuk permintaan lain; enum tak dikenal; tenggat 500; identitas pelaku audit; jabatan CITO nonaktif; draf CITO lintas unit | Source + uji tertulis |

Tidak ditemukan temuan CRITICAL maupun HIGH.

## 4. Matriks UAT

| Kelompok | Skenario | Prasyarat | Blocker |
| --- | --- | --- | --- |
| Persetujuan atasan | 1–9, 12 (bagian 12) | Unit tertaut, penempatan manajerial, izin Approve | BLOCKED_DATA |
| Otorisasi CITO | 5–6, 17 | Jabatan CITO ditunjuk + izin AuthorizeEmergency | BLOCKED_DATA; `LGS-OQ-026` (cara penunjukan) |
| Verifikasi susulan dan tenggat | 13–14, 28, 31 | Tenggat dikonfigurasi | BLOCKED_DATA; tindak lanjut temuan `LGS-OQ-025` (BLOCKED_MIGRATION); notifikasi `LGS-OQ-024` |
| Batas aktivasi | 11–12 | Nilai batas saat aktivasi | BLOCKED_DATA |
| Penyerahan dan stok | 10, 27 | Stok dan batch di gudang uji | Data uji di lingkungan yang disetujui |
| Keamanan API | 18–20, daftar periksa *LANJUTAN FINISHING 2* butir 5 | Alat API, dua akun | — |
| Riwayat Permintaan | 22–23 | Ada permintaan dengan riwayat | Cakupan baca `LGS-OQ-027` |
| Status Pemenuhan | 24–26, 29–32 | Permintaan di berbagai tahap | — |
| Navigasi | 21, 28 | — | Entri menu Verifikasi CITO (keputusan pemrakarsa) |
| Kesiapan data | `GET /approval-readiness` | Izin `ViewApprovalReadiness` | `LGS-OQ-028` |
| Akses lintas unit | Belum ada skenario final | Keputusan D1–D7, W1–W3 | `LGS-OQ-027`, `029` |
| Konkurensi | Prosedur bagian 15 | Sandbox PostgreSQL | BLOCKED_SANDBOX |

### Blocker `LGS-OQ-024` sampai `029`

| ID | Status | Yang tertahan | Pemilik keputusan |
| --- | --- | --- | --- |
| `LGS-OQ-024` | OPEN | Notifikasi Logistik (saat ini hanya hitungan di layar) | Pemilik platform + HR + Farmasi |
| `LGS-OQ-025` | OPEN | Tindak lanjut temuan CITO (PIC, status, penutupan); butuh migrasi | Manajemen + Farmasi |
| `LGS-OQ-026` | OPEN | Penunjukan CITO lewat konfigurasi vs master data | Manajemen + platform |
| `LGS-OQ-027` | OPEN (rancangan D1–D7 tersedia) | Pembatasan baca daftar, detail, dan linimasa | Farmasi + manajemen |
| `LGS-OQ-028` | OPEN | Pemegang izin laporan kesiapan | Pemilik platform |
| `LGS-OQ-029` | OPEN (rancangan W1–W3 tersedia) | Pembatasan tulis dan sumber petugas peminta | Farmasi + manajemen |

## 5. Keputusan gate

> Diperbarui setelah eksekusi; lihat butir 6.

| Gerbang | Status | Alasan |
| --- | --- | --- |
| Build dan uji terisolasi | **PASS** | Butir 6 |
| PR ke integration (backend dahulu, lalu frontend) | **GO** | Build PASS, 135/135, 40/40, ESLint 0 error; tidak beririsan dengan integration terbaru (`659d4635` / `cb5174327`) dan `merge-tree` bersih. Perilaku akses existing tidak diubah (`LGS-OQ-027`/`029` tetap OPEN). PR dibuat pemilik proyek |
| UAT penuh | **NO-GO** | BLOCKED_DATA (tautan unit 0/20, penempatan manajerial 0, izin 0 jabatan, jabatan CITO, tenggat, batas aktivasi), sehingga persetujuan dan otorisasi CITO tidak dapat dijalani; `LGS-OQ-027`/`029` belum diputuskan |
| UAT terbatas | **Bersyarat** | Setelah PR di-merge dan dideploy ke lingkungan uji yang disetujui, skenario yang tidak bergantung data organisasi dapat mulai diuji: navigasi (21, 28), keamanan API (18–20), Riwayat Permintaan (22–23), Status Pemenuhan (24–26, 29–32), dengan data uji di lingkungan itu, bukan data dummy di basis data operasional |
| Produksi | **NO-GO** | UAT penuh belum berjalan; konkurensi PostgreSQL BLOCKED_SANDBOX |

## 6. Hasil eksekusi (19 November 2026)

| No | Pemeriksaan | Hasil | Bukti |
| --- | --- | --- | --- |
| 1 | **Build backend terisolasi** (`a3b2ed08`, `-p:SkipMigrationMetadata=true`, foreground) | **PASS**: 0 error, 1 menit 29 detik. **0 peringatan dari source Logistik maupun berkas uji**; 599 peringatan milik proyek utama (CS1573, EF1002, NU190x `SixLabors.ImageSharp`) | `srq-isolated-tests\build5.log` |
| 2 | **Uji backend** (115 metode, SQLite in-memory, `--no-build`) | **135 Passed / 0 Failed / 0 Skipped**, 2 menit 8 detik. Per kelas sama dengan inventaris; seluruh area kritis lolos: FEFO tanpa pengurangan ganda, idempotensi ketujuh perintah (7 kasus teori), regresi `DecidedAt` (5), linimasa termasuk pembatasan data (4 peran × 2 cakupan), Status Pemenuhan, otorisasi termasuk SuperAdmin, tenggat tidak 500. Log tidak memuat `Npgsql`, `Host=`, maupun port 5432 | `srq-isolated-tests\test5.log`, `TestResults\run5.trx` |
| 3 | **Uji frontend** (`stock-request-approval` + `logistics-module-navigation`, node test runner; tanpa build/dev server) | **40 Passed / 0 Failed / 0 Skipped** | `scratchpad\fe-test5.log` |
| 4 | **ESLint** (25 berkas JS/JSX/MJS Logistik dari `002f0bcd` dan `109a4083`) | **0 error, 4 warning**, semuanya `react-hooks/set-state-in-effect` pada hook pemuat data (`use-stock-request-approval-scope`, `-approval-summary`, `-history-log`, `-list`). Pola yang sama dengan hook yang sudah ada sebelum Logistik V2 (`use-stock-request-detail`) | Keluaran ESLint |
| 5 | **Source yang diperbaiki** | **Tidak ada.** Tidak ada kegagalan, sehingga `a3b2ed08` dan `109a4083` tetap apa adanya; working tree kedua repo tidak berubah oleh eksekusi | `git status` |
| 6 | Proses dan basis data | Tidak ada backend maupun server yang berjalan (satu-satunya `dotnet.exe` adalah build host VS Code); tidak ada migrasi, seeder, maupun koneksi ke basis data operasional | `tasklist`, log uji |

Lokasi bukti: `%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\` (folder sementara).

**Yang tetap belum terverifikasi:**

- aplikasi berjalan dengan pengguna dan data sungguhan;
- filter `[AccessPermission]` controller;
- UAT;
- konkurensi PostgreSQL (`ConcurrencyTests` hanya membuktikan logika token versi pada SQLite).

**Blocker yang tersisa:**

| Jenis | Blocker |
| --- | --- |
| BLOCKED_DECISION | `LGS-OQ-024` sampai `029`; entri menu Verifikasi CITO; isi "Pemeriksaan Stok" |
| BLOCKED_DATA | Data organisasi dan izin; koreksi `DecidedAt`/`DecisionReason` baris lama |
| BLOCKED_MIGRATION | Tindak lanjut temuan CITO; aktivasi batas; notifikasi |
| BLOCKED_SANDBOX | Konkurensi PostgreSQL |
| BLOCKED_FEATURE | 39 entri menu Logistik |

---

# FINAL INTEGRATION READINESS

| Field | Nilai |
| --- | --- |
| Tanggal | 19 November 2026 |
| Lingkup | FIN-LGS-020 sampai 023 |
| Versi yang diuji | **Backend `a3b2ed08`** (`Ikbal`, memuat `6302f5fc`), **frontend `109a4083`** (`Ikbalv2`, memuat `002f0bcd`) |
| Batas | Tanpa commit, push, PR, merge, migrasi, maupun deployment. Tidak ada pengujian ulang karena source tidak berubah sejak *FINAL QUALITY GATE* butir 6. Laporan ini satu-satunya berkas yang berubah |

## 1. Bukti pengujian (FIN-LGS-020)

| Pemeriksaan | Versi | Hasil |
| --- | --- | --- |
| Build backend terisolasi | `a3b2ed08` | PASS: 0 error; 0 peringatan dari Logistik |
| Uji backend (SQLite in-memory) | `a3b2ed08` | 135 Passed / 0 Failed / 0 Skipped (115 metode, 13 kelas) |
| Uji frontend (node test runner) | `109a4083` | 40 Passed / 0 Failed / 0 Skipped |
| ESLint (25 berkas Logistik) | `109a4083` | 0 error, 4 warning (`react-hooks/set-state-in-effect`, pola lama) |

Ringkasan dan daftar lengkap kasus dipindahkan dari folder sementara ke **Lampiran A** di akhir laporan ini, yang permanen di repo backend. Path mesin, nama pengguna, dan konfigurasi lingkungan tidak ikut; pemindaian lampiran untuk kredensial, connection string, alamat server, dan email: 0 temuan.

Berkas log asli (`build5.log`, `test5.log`, `run5.trx`, `fe-test5.log`) tetap di folder sementara dan dapat hilang saat restart. Yang permanen adalah Lampiran A.

## 2. Kesiapan integrasi (FIN-LGS-021)

| Repo | Commit untuk PR | Integration terbaru | Integration di depan | Irisan berkas | `merge-tree` |
| --- | --- | --- | --- | --- | --- |
| Backend `Ikbal` → `QuilvianIntegrationBackend` | `6302f5fc`, `a3b2ed08` | `659d4635` | 28 commit (HR, Payroll, BPJS) | **0** | Bersih |
| Frontend `Ikbalv2` → `QuilvianIntegrationFrontend` | `002f0bcd`, `109a4083` | `cb5174327` | 12 commit (Payroll, BPJS) | `menu-items.jsx` | Bersih |

| Konflik | Hasil pemeriksaan |
| --- | --- |
| Source | Tidak ada berkas yang beririsan di backend. Di frontend, integration hanya mengubah baris Payroll pada `menu-items.jsx`; **bagian Logistik pada hasil gabungan identik** dengan `Ikbalv2` |
| Dependensi bersama | Integration backend tidak menyentuh izin, logger, model organisasi, master obat/satuan, layanan stok, maupun `StockRequest*`. Integration frontend mengubah `use-select-resource.jsx`, tetapi murni penambahan (+47/−0: `meta`, `hasLoaded`, `errorStatus`, `isForbidden`); field yang dipakai pemilih Logistik tidak berubah |
| API | Tidak ada route `stock-requests` maupun kelas `StockRequest*` baru di integration |
| Menu | 11 grup, 67 entri, 66 dapat diklik, 27 tertaut; tetap sama setelah gabung |
| Migrasi | Integration memuat 3 migrasi Payroll; Logistik tidak membawa migrasi |

**Urutan wajib: backend lebih dahulu.** Frontend memanggil `GET /stock-requests/histories` dan `GET /stock-requests/approval-scope`, mengirim `scope`, `emergencyReviewDueState`, `requestedByWorkforceId`, `excludeDraft`, dan `partiallyFulfilledOnly`, serta membaca `decisionAt`/`fulfilledAt`/`cancelledAt`/`remainingQuantity`. Dengan backend lama, Riwayat Permintaan dan Informasi Kewenangan menjawab 404, dan saringan baru diabaikan. Sebaliknya, frontend integration yang lama tetap berfungsi dengan backend baru, karena perubahan API bersifat menambah.

**Perubahan perilaku begitu backend di-merge** (perlu diketahui tim):

1. Otorisasi CITO tertutup sampai `PharmacyManagement:StockRequest:EmergencyAuthorizerPositions` diisi dan jabatannya diberi `StockRequest:AuthorizeEmergency` (`LGS-DEC-004`).
2. `DecidedAt`/`DecisionReason` hanya berisi keputusan atasan; tidak lagi ditimpa penyerahan atau pembatalan.
3. Nilai enum saringan yang tidak dikenal dijawab 400.
4. Kunci idempotensi milik pengguna lain dijawab 409 `PHM013`.
5. Log audit permintaan stok mencatat pelaku yang benar. Integration saat ini masih mencatat id permintaan sebagai pelaku.
6. Izin baru `StockRequest:ViewApprovalReadiness` terdaftar saat startup dan tidak diberikan kepada siapa pun.

### Draf PR backend

```text
Judul: Logistik V2 — persetujuan permintaan barang: cakupan, CITO, linimasa, progres, pengerasan

Ringkasan
- "Unit yang Saya Pimpin" (scope=1), GET approval-scope dan approval-readiness.
- Otorisasi CITO: jabatan yang ditunjuk manajemen + izin eksplisit; tenggat verifikasi susulan.
- GET /stock-requests/histories: linimasa aktivitas (tanggal, unit, pemohon, status tujuan).
- Status Pemenuhan: decisionAt/fulfilledAt/cancelledAt/shortfallItemCount dari riwayat,
  remainingQuantity per baris; saringan pemohon, tanpa draf, pemenuhan sebagian.
- Pengerasan: replay idempotensi antarpengguna (PHM013) pada tujuh perintah, pembatalan yang
  dijawab berhasil untuk permintaan lain, enum tak dikenal → 400, tenggat tidak menjatuhkan
  endpoint, identitas pelaku pada log audit, audit ubah/batal.
- DecidedAt/DecisionReason hanya keputusan atasan (LGS-DEC-002).

Uji
- Build terisolasi PASS; StockRequestApprovalTests 135/135 (SQLite in-memory), 19 Nov 2026,
  pada a3b2ed08. Konkurensi PostgreSQL belum diuji (butuh sandbox).

Catatan
- Tanpa entity, tabel, maupun migration baru.
- Konfigurasi baru (kosong = tertutup/tidak dikonfigurasi): EmergencyAuthorizerPositions,
  EmergencyReviewDueHours, EmergencyReviewWarningHours; ApprovalEnforcedFromUtc sudah ada.
- Akses baca/tulis lintas unit tidak diubah (LGS-OQ-027/029 masih terbuka).
- Merge sebelum PR frontend Logistik.
- Laporan: docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

### Draf PR frontend

```text
Judul: Logistik V2 — Validasi Atasan, Verifikasi CITO, Riwayat Permintaan, Status Pemenuhan

Ringkasan
- Validasi Permintaan Barang dengan cakupan backend, ringkasan, Informasi Kewenangan; alasan
  CITO pemohon tampil sebelum panel keputusan.
- Verifikasi Susulan CITO (dibuka dari Validasi Atasan, tanpa entri menu baru).
- Riwayat Permintaan: linimasa aktivitas; Status Pemenuhan: delapan keadaan, penanda CITO
  menunggu verifikasi, "n dari m baris lengkap".
- Detail: Diputus Atasan / Diserahkan / Dibatalkan Pada, sisa per baris dari backend, alasan per
  langkah riwayat.
- Menu: Pengajuan Kebutuhan Barang, Status Pemenuhan, Riwayat Permintaan ditautkan
  (11 grup, 67 entri, 27 tertaut).

Uji
- tests/unit/stock-request-approval + logistics-module-navigation: 40/40; ESLint 0 error,
  19 Nov 2026, pada 109a4083.

Ketergantungan
- Membutuhkan PR backend Logistik (GET /histories, /approval-scope) sudah di-merge.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

## 3. Persiapan UAT terbatas (FIN-LGS-022)

**Aturan:**

- Dijalankan di lingkungan uji yang disetujui, sesudah PR backend lalu frontend di-merge dan dideploy.
- Data uji dibuat lewat layar di lingkungan itu. **Tidak** memakai atau mengubah data operasional tanpa izin.
- Bukti untuk setiap skenario: tangkapan layar, respons jaringan (status dan isi), dan bila perlu baris log audit.
- Konfigurasi keamanan harus `Security:Authorization:Enabled=true`. Dengan nilai `false`, pemeriksaan izin dilepas dan hasil uji otorisasi tidak sah.

### U1. Navigasi 27 menu tertaut

| Grup | Entri → route | Harapan |
| --- | --- | --- |
| Dashboard Logistik | Ringkasan Stok → `/drug-stock/summary`; Barang Hampir Habis → `/drug-stock/low-stock`; Permintaan Unit → `/stock-requests`; Barang Mendekati Kadaluarsa → `/drug-stock/expiring` | Halaman terbuka, data dari API, tanpa galat konsol |
| Master Barang & Gudang | Katalog Barang → `/master-data/drug`; Kategori Barang → `/master-data/drug-category`; Satuan & Konversi → `/master-data/drug-unit-conversions`; Daftar Gudang dan Lokasi Penyimpanan → `/master-data/drug-storage-locations` | sama |
| Permintaan Barang Unit | Pengajuan Kebutuhan Barang → `/stock-requests/create`; Data Permintaan → `/stock-requests`; Validasi Atasan → `/stock-requests/approvals`; Status Pemenuhan → `/stock-requests/fulfillment`; Riwayat Permintaan → `/stock-requests/history` | sama; tombol "Verifikasi Susulan CITO" di Validasi Atasan membuka `/stock-requests/emergency-reviews` |
| Penerimaan Barang | Pemeriksaan Hasil Pengadaan → `/finance/purchasing/goods-receipts` | Terbuka bagi pemegang `FinanceGoodsReceipt:Read`; lainnya tidak melihat entri |
| Penyimpanan & Penempatan | Penelusuran Lokasi Barang → `/drug-stock/locations` | Terbuka; menunggu obat dipilih |
| Stok & Kartu Persediaan | Stok per Gudang → `/drug-stock`; Kartu Stok dan Riwayat Barang Masuk/Keluar → `/drug-stock/card`; Batch/Lot → `/drug-stock/batches` | Terbuka |
| Distribusi & Pengeluaran | Daftar Permintaan Disetujui → `/stock-requests/approved` | Terbuka; hanya yang dapat diserahkan |
| Mutasi & Retur | Transfer Antar Gudang → `/stock-transfers`; Retur Unit → `/drug-returns`; Retur ke Penyedia → `/finance/purchasing/supplier-returns` | Terbuka sesuai izin |
| Pengendalian Persediaan | Stok Minimal/Maksimal → `/master-data/drug-stock-policy`; Kadaluarsa → `/drug-stock/expiring`; Barang Rusak → `/drug-stock/damaged` | Terbuka |

Semua route Farmasi diawali `/health-services/pharmacy-management` atau `/health-services/master-data`. Entri yang belum tertaut (39) tidak bisa dibuka dan memang tidak diuji.

### U2–U7

| ID | Area | Langkah | Harapan | Prasyarat |
| --- | --- | --- | --- | --- |
| U2.1 | Buat permintaan | Pengajuan Kebutuhan Barang → isi unit, gudang, petugas peminta, 1–2 obat → simpan | Draft tersimpan; nomor `REQ-…`; riwayat "Permintaan dibuat" | Master obat, satuan, unit, gudang aktif |
| U2.2 | Ubah draft | Buka draft → ubah jumlah/obat → simpan | Versi naik; riwayat "Permintaan diubah"; obat ganda ditolak | U2.1 |
| U2.3 | Ajukan | Ajukan draft rutin; ajukan draft CITO tanpa alasan | Rutin: Menunggu Persetujuan. CITO tanpa alasan: 422 `PHM014` | U2.1 |
| U2.4 | Batal | Batalkan permintaan yang menunggu | Status Dibatalkan; "Dibatalkan Pada" terisi; alasan di riwayat | U2.3 |
| U3.1 | Riwayat Permintaan | Buka menu → saring status tujuan, unit, pemohon, tanggal | Terbaru lebih dahulu; jumlah dan halaman berubah sesuai saringan; nama petugas dan alasan tampil | U2.x |
| U3.2 | Riwayat — detail | Klik Detail pada satu baris | Membuka permintaan yang benar (token, bukan id di alamat) | U3.1 |
| U4.1 | Status Pemenuhan | Buka menu | Tanpa draft; tahap Menunggu Persetujuan/Dibatalkan sesuai data | U2.x |
| U4.2 | Pemenuhan sebagian | Setelah ada penyerahan kurang (butuh U7) → saring "Hanya yang diserahkan sebagian" | Tahap "Dipenuhi Sebagian", "n dari m baris lengkap"; detail: sisa = diminta − diserahkan | U7 |
| U5.1 | Validasi API | `GET /stock-requests?scope=9`, `?status=99`, `/histories?toStatus=99` | 400 | Alat API + token |
| U5.2 | Tanpa login | `GET /stock-requests`, `/histories`, `/approval-scope` tanpa token | 401 | — |
| U5.3 | Replay antarpengguna | Akun A melakukan aksi dengan `idempotencyKey` K; akun B mengirim aksi yang sama dengan K | B: 409 `PHM013` "pengguna lain"; tidak ada riwayat kedua | Dua akun |
| U5.4 | Kunci batal untuk permintaan lain | Batalkan P1 dengan K; kirim batal P2 dengan K dan alasan sama | 409 `PHM013`; P2 tidak berubah | Dua permintaan |
| U5.5 | Tanpa izin | Akun tanpa `StockRequest:Read` membuka Data Permintaan | Pesan akses ditolak (403) | Akun tanpa izin |
| U6.1 | Audit pelaku | Lakukan buat, ubah, ajukan, batal dengan akun yang diketahui | Log memuat `Action="StockRequest.Create/Update/Submit/Cancel"` dengan `UserId=` akun login, bukan id permintaan | Akses log (Loki/berkas) |
| U6.2 | Riwayat pelaku | Bandingkan Riwayat Permintaan dengan akun pelaku | Kolom Petugas = akun pelaku; kolom pemohon = petugas peminta | U2.x |
| U7.1 | Persetujuan atasan | Atasan unit menyetujui / menolak (alasan wajib) | Disetujui/Ditolak; "Diputus Atasan Pada"; pemohon tidak bisa memutus sendiri | **BLOCKED_DATA**: tautan unit, penempatan manajerial, izin Approve |
| U7.2 | Penyerahan | Gudang menyerahkan sebagian/penuh; klik ganda | Stok berkurang sekali; kiriman kedua 409; "Diputus Atasan Pada" tetap waktu persetujuan | U7.1 + stok uji |
| U7.3 | CITO | Petugas CITO mengotorisasi; atasan memverifikasi (benar/temuan) | Alasan CITO pemohon terbaca sebelum otorisasi; penanda menunggu verifikasi; tenggat | **BLOCKED_DATA**: jabatan CITO, izin AuthorizeEmergency, tenggat |

## 4. Keputusan prioritas (FIN-LGS-023)

| Prioritas | Blocker | Rekomendasi | Konsekuensi bila belum diputuskan | Keputusan yang dibutuhkan dari pemrakarsa |
| --- | --- | --- | --- | --- |
| **P1** | Mapping organisasi dan izin (BLOCKED_DATA) | HRD menautkan 20 unit layanan ke unit organisasi dan menandai penempatan manajerial kepala unit; administrator Akses Role memberi `StockRequest:Approve` ke jabatan kepala unit; manajemen menunjuk jabatan CITO; operator mengisi `EmergencyAuthorizerPositions`, tenggat, dan batas aktivasi. Pantau dengan `GET /approval-readiness` | Persetujuan dan CITO tidak dapat dijalani siapa pun; UAT penuh tertahan | Siapa pelaksana HRD/Akses Role, jabatan mana yang menjadi atasan unit dan otorisasi CITO, nilai tenggat (jam), tanggal aktivasi |
| **P1** | `LGS-OQ-027` baca lintas unit | Setujui D1–D7: unit sendiri = penempatan HR aktif + permintaan sendiri; gudang lewat lokasi → unit; izin pemantauan `StockRequest:ReadAllUnits`; SuperAdmin tidak otomatis; 404 di luar cakupan; aktivasi dengan saklar setelah data P1 terisi | Semua pemegang `StockRequest:Read`, termasuk SuperAdmin, membaca permintaan, detail, dan linimasa semua unit | Setuju/ubah D1–D7, dan siapa pemegang izin pemantauan |
| **P1** | `LGS-OQ-029` tulis lintas unit | Setujui W1–W3: petugas peminta dari akun login; buat/ubah/ajukan hanya unit sendiri; batal setelah Disetujui hanya atasan unit | Permintaan dapat dibuat atas nama unit atau petugas lain; permintaan unit lain dapat diubah/dibatalkan | Setuju/ubah W1–W3; diputuskan bersama OQ-027 |
| **P2** | `LGS-OQ-026` cara penunjukan CITO | Pakai konfigurasi (sudah tersedia) untuk tahap awal; master data dengan jejak perubahan bila pengelolanya non-teknis | Penunjukan hanya dapat diubah operator konfigurasi | Terima konfigurasi, atau minta master data (butuh migrasi) |
| **P2** | `LGS-OQ-028` izin laporan kesiapan | Beri `StockRequest:ViewApprovalReadiness` kepada administrator Logistik/Farmasi dan administrator Akses Role | Tidak ada yang dapat memantau kesiapan data di produksi | Jabatan pemegang izin |
| **P2** | `LGS-OQ-025` tindak lanjut temuan CITO | Entity tindak lanjut sesuai rancangan bagian 8 (PIC, status Terbuka → Ditangani → Selesai, tenggat, penutup) | Temuan hanya tercatat di riwayat dan log, tanpa penelusuran penyelesaian | PIC operasional, alur status, persetujuan migrasi |
| **P2** | `LGS-OQ-024` notifikasi | Tunda; sementara memakai hitungan "Perlu Perhatian Anda". Tetapkan notifikasi platform lintas modul bersama pemilik HR/platform | Atasan dan petugas CITO harus membuka layar untuk tahu ada tugas | Tunda / pakai `HrdNotification` sebagai platform / tabel sendiri (migrasi) |
| **P3** | Konkurensi PostgreSQL (BLOCKED_SANDBOX) | Sediakan basis data sandbox; jalankan prosedur bagian 15 (dua keputusan serentak, dua CITO, dua verifikasi, replay) | Penjaga token versi baru terbukti di SQLite | Persetujuan dan penyedia sandbox |
| **P3** | Koreksi `DecidedAt`/`DecisionReason` baris lama (BLOCKED_DATA) | Biarkan; tampilan sudah memakai riwayat. Bila diperlukan untuk laporan, buat skrip koreksi dari `PhmStockRequestHistory` yang ditinjau dan disetujui | Laporan yang membaca kolom itu langsung dapat keliru untuk baris lama | Biarkan, atau setujui skrip koreksi |
| **P3** | Entri menu Verifikasi CITO; isi "Pemeriksaan Stok" | Tetap lewat Validasi Atasan; definisikan "Pemeriksaan Stok" sebagai cek ketersediaan sebelum penyerahan, bila diinginkan | Verifikasi CITO tidak muncul di sidebar | Tambah entri atau tidak; definisi Pemeriksaan Stok |
| **P3** | Migrasi dan 39 entri menu (BLOCKED_FEATURE) | Tetapkan urutan modul berikutnya. Usulan: Penerimaan Barang (`LGS-OQ-017`) → Stock Opname (`LGS-OQ-009`) → Laporan & Nilai Persediaan (`LGS-OQ-008`, `012`). Masing-masing butuh keputusan dan migrasi | Entri menu tetap tidak tertaut | Urutan prioritas, pemilik domain (`LGS-OQ-015`), dan wewenang migrasi |

## 5. Kesimpulan

| Gerbang | Status |
| --- | --- |
| Bukti pengujian aman (permanen) | **Selesai** (Lampiran A) |
| PR backend → integration | **GO** (dibuat pemilik proyek) |
| PR frontend → integration | **GO setelah PR backend di-merge** |
| UAT terbatas (U1–U6) | **Siap** setelah merge dan deploy ke lingkungan uji yang disetujui |
| UAT penuh (U7) | **NO-GO** sampai P1 mapping organisasi dan izin selesai |
| Produksi | **NO-GO**: UAT penuh, keputusan P1 (`LGS-OQ-027`/`029`), konkurensi PostgreSQL |

**Runtime terverifikasi hanya untuk uji otomatis** (build, 135/135, 40/40, ESLint). Aplikasi berjalan, UAT, dan konkurensi PostgreSQL belum.

---

## Lampiran A — Bukti pengujian 19 November 2026

Versi: backend `a3b2ed08`, frontend `109a4083`. Diambil dari log di folder sementara; path mesin dan identitas lingkungan dihapus. Baris `Failed`/`Skipped` tidak dicetak `dotnet test` karena nilainya nol.

### Build backend (`build5.log`)

```text
Build succeeded.
599 Warning(s)
0 Error(s)
Time Elapsed 00:01:28.91
```

### Uji backend (`test5.log`, `run5.trx`)

```text
Total tests: 135
Passed: 135
```

<details><summary><code>ApiContractTests</code> — 5 kasus</summary>

```text
✔ Konfigurasi_tenggat_tidak_sah_tidak_membuat_endpoint_gagal_500
✔ Nilai_angka_status_dan_penanda_terkunci_sesuai_konstanta_frontend
✔ Respons_detail_memakai_nama_properti_yang_dibaca_frontend
✔ Tidak_berwenang_403_benturan_409_prasyarat_422_berhasil_200
✔ Verifikasi_susulan_lewat_controller_memetakan_kode_yang_sama
```

</details>

<details><summary><code>ApprovalScopeTests</code> — 17 kasus</summary>

```text
✔ Atasan_beberapa_unit_melihat_seluruh_unit_yang_sah
✔ Atasan_hanya_melihat_permintaan_unit_yang_dipimpinnya
✔ Cakupan_atasan_memuat_unit_yang_dipimpin_tanpa_catatan
✔ Cakupan_kosong_disertai_penjelasan_yang_sesuai
✔ Daftar_menggolongkan_tenggat_sama_dengan_label_per_baris
✔ Jabatan_berizin_yang_tidak_ditunjuk_tidak_berwenang_CITO
✔ Kesiapan_data_melaporkan_jumlah_tanpa_identitas_pegawai
✔ Lingkup_bawaan_tetap_seluruh_data_demi_kompatibilitas
✔ Lingkup_konsisten_dengan_saringan_status_pencarian_dan_halaman
✔ Pengguna_tanpa_kewenangan_mendapat_daftar_kosong_bukan_seluruh_data(siapa: "staf")
✔ Pengguna_tanpa_kewenangan_mendapat_daftar_kosong_bukan_seluruh_data(siapa: "superadmin")
✔ Pengguna_tanpa_kewenangan_mendapat_daftar_kosong_bukan_seluruh_data(siapa: "tanpa-izin")
✔ Penunjukan_berdasarkan_kode_jabatan_berlaku
✔ Petugas_CITO_melihat_permintaan_CITO_lintas_unit_tetapi_bukan_permintaan_rutin
✔ Tanpa_penunjukan_jabatan_otorisasi_CITO_ditolak_walau_izin_ada
✔ Tenggat_belum_dikonfigurasi_tidak_menggolongkan_apa_pun
✔ Tenggat_tidak_berlaku_lagi_setelah_diverifikasi_dan_tidak_memverifikasi_otomatis
```

</details>

<details><summary><code>AuthorizationTests</code> — 13 kasus</summary>

```text
✔ Atasan_unit_berizin_menyetujui_dan_keputusannya_tercatat_lengkap
✔ Atasan_yang_membuatkan_permintaan_tidak_dapat_menyetujuinya
✔ Detail_menyampaikan_kewenangan_dari_server_tanpa_masukan_layar
✔ Kasus1_pemohon_berwenang_penuh_tetap_tidak_dapat_memutus_permintaannya_sendiri
✔ Kasus2_atasan_unit_A_tidak_dapat_memutus_permintaan_unit_B
✔ Kasus3_atasan_unit_tanpa_izin_Approve_eksplisit_ditolak
✔ Kasus4_SuperAdmin_tanpa_penempatan_yang_sah_ditolak
✔ Kasus5_CITO_tanpa_otorisasi_eksplisit_ditolak_termasuk_SuperAdmin_dan_atasan_unit
✔ Kasus6_CITO_dengan_otorisasi_eksplisit_dapat_segera_diproses
✔ Penempatan_manajerial_yang_berakhir_tidak_memberi_kewenangan
✔ Profil_tenaga_kerja_nonaktif_tidak_memberi_kewenangan
✔ Staf_berizin_tanpa_penempatan_manajerial_ditolak
✔ Unit_layanan_yang_belum_ditautkan_tidak_dapat_diputus_siapa_pun
```

</details>

<details><summary><code>ConcurrencyTests</code> — 6 kasus</summary>

```text
✔ Dua_persetujuan_bersamaan_hanya_satu_yang_tersimpan
✔ Dua_verifikasi_susulan_bersamaan_hanya_satu_yang_tersimpan
✔ Otorisasi_CITO_dan_penolakan_bersamaan_hanya_satu_yang_sah
✔ Setuju_dan_tolak_bersamaan_hanya_satu_keputusan_yang_sah
✔ Token_versi_menolak_penyimpanan_dari_salinan_lama
✔ Versi_yang_tertinggal_ditolak_sebelum_menyentuh_data
```

</details>

<details><summary><code>CutoverUnitTests</code> — 11 kasus</summary>

```text
✔ Batas_di_masa_depan_ditolak_karena_batas_dikunci_saat_aktivasi
✔ Batas_yang_sah_dan_sudah_lewat_memisahkan_permintaan_lama_dan_baru
✔ Nilai_berzona_waktu_dinormalkan_ke_UTC
✔ Nilai_kosong_bukan_izin_jalan_pintas(nilai: "   ")
✔ Nilai_kosong_bukan_izin_jalan_pintas(nilai: "")
✔ Nilai_kosong_bukan_izin_jalan_pintas(nilai: null)
✔ Nilai_tanpa_zona_waktu_dianggap_UTC
✔ Nilai_yang_tidak_dapat_dibaca_diabaikan_dan_semua_wajib_disetujui(nilai: "2026-13-45")
✔ Nilai_yang_tidak_dapat_dibaca_diabaikan_dan_semua_wajib_disetujui(nilai: "bukan-tanggal")
✔ Permintaan_tanpa_waktu_pengajuan_tidak_pernah_dianggap_permintaan_lama
✔ Selisih_jam_antarserver_dalam_toleransi_tetap_diterima
```

</details>

<details><summary><code>DecisionLifecycleTests</code> — 5 kasus</summary>

```text
✔ Pembatalan_sebelum_diputus_tidak_mengisi_waktu_keputusan
✔ Pembatalan_setelah_disetujui_tidak_menimpa_keputusan_atasan
✔ Penolakan_mengisi_waktu_dan_alasan_keputusan
✔ Penyerahan_lewat_aturan_peralihan_tidak_mengarang_keputusan_atasan
✔ Penyerahan_tidak_menimpa_waktu_dan_catatan_persetujuan
```

</details>

<details><summary><code>EmergencyReviewTests</code> — 12 kasus</summary>

```text
✔ Atasan_unit_membenarkan_otorisasi_CITO_tanpa_mengubah_status
✔ Atasan_unit_yang_mengotorisasi_CITO_tidak_boleh_memverifikasi_otorisasinya_sendiri
✔ Daftar_menunggu_verifikasi_hanya_memuat_CITO_terotorisasi_yang_belum_diverifikasi
✔ Detail_menyampaikan_kewenangan_verifikasi_dan_otorisasi_awal
✔ Hasil_verifikasi_yang_tidak_dikenal_ditolak
✔ Kasus13_verifikasi_susulan_tidak_mengubah_stok_walau_service_memegang_akses_stok
✔ Kasus14_verifikator_yang_tidak_berwenang_ditolak
✔ Pengulangan_kunci_yang_sama_tidak_menggandakan_verifikasi
✔ Permintaan_yang_disetujui_atasan_biasa_tidak_memerlukan_verifikasi_susulan
✔ Temuan_penyimpangan_tidak_menghapus_transaksi_fisik_yang_sudah_terjadi
✔ Temuan_penyimpangan_wajib_disertai_catatan
✔ Verifikasi_ganda_ditolak
```

</details>

<details><summary><code>HardeningTests</code> — 7 kasus</summary>

```text
✔ Detail_CITO_menyampaikan_alasan_pemohon_kepada_pengotorisasi
✔ Kunci_idempotensi_milik_pengguna_lain_ditolak_dan_tidak_membuka_hasilnya
✔ Kunci_pembatalan_yang_dipakai_ulang_untuk_permintaan_lain_tidak_dianggap_berhasil
✔ Nilai_enum_yang_tidak_dikenal_ditolak_validasi_permintaan_HTTP
✔ Pembatalan_permintaan_disetujui_tercatat_di_log_audit_dengan_pelaku_dan_status_asal
✔ Pengulangan_oleh_pelaku_yang_sama_tetap_idempoten
✔ Saringan_enum_tak_dikenal_pada_service_menghasilkan_daftar_kosong_bukan_tafsiran_lain
```

</details>

<details><summary><code>IdempotencyReplayTests</code> — 9 kasus</summary>

```text
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "ajukan")
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "batal")
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "buat")
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "keputusan")
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "serah")
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "ubah")
✔ Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain(perintah: "verifikasi")
✔ Pembuatan_dan_pengubahan_draf_tercatat_di_log_audit_dengan_pelaku_yang_benar
✔ Penyerahan_mengurangi_stok_sekali_walau_diulang_atau_dikirim_dua_kali
```

</details>

<details><summary><code>OperationalSettingsUnitTests</code> — 14 kasus</summary>

```text
✔ Klasifikasi_belum_jatuh_tempo_mendekati_dan_terlambat_mengikuti_konfigurasi
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "-5", peringatan: null)
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "0", peringatan: null)
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "1000000", peringatan: null)
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "1e300", peringatan: null)
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "24", peringatan: "-1")
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "24", peringatan: "24")
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "24", peringatan: "30")
✔ Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(jam: "abc", peringatan: null)
✔ Penunjukan_jabatan_menerima_id_dan_kode_dalam_daftar_maupun_teks_berkoma
✔ Penunjukan_kosong_berarti_tidak_ada_jabatan_berwenang
✔ Tanpa_batas_peringatan_tidak_ada_keadaan_mendekati_tenggat
✔ Tenggat_tanpa_nilai_tidak_dikonfigurasi_dan_tidak_ada_bawaan(jam: "")
✔ Tenggat_tanpa_nilai_tidak_dikonfigurasi_dan_tidak_ada_bawaan(jam: null)
```

</details>

<details><summary><code>ProgressAndHistoryTests</code> — 6 kasus</summary>

```text
✔ Daftar_dan_detail_memuat_tahapan_progres_dari_riwayat
✔ Linimasa_dengan_cakupan_mengikuti_kewenangan_dan_nilai_asing_tidak_ditafsirkan
✔ Linimasa_memuat_setiap_perubahan_terbaru_lebih_dahulu_dengan_nama_petugas
✔ Linimasa_menyaring_status_unit_pemohon_tanggal_dan_halaman
✔ Linimasa_tidak_membuka_permintaan_di_luar_yang_terlihat_pada_daftar
✔ Saringan_pemenuhan_sebagian_draf_dan_pemohon_bekerja_di_backend
```

</details>

<details><summary><code>RobustnessAndAuditTests</code> — 12 kasus</summary>

```text
✔ Akun_atasan_yang_dinonaktifkan_tidak_lagi_berwenang
✔ Draf_CITO_unit_lain_tidak_terlihat_oleh_petugas_CITO
✔ Izin_pada_controller_nonaktif_tidak_berlaku_dan_tidak_dihitung_kesiapan
✔ Jabatan_CITO_yang_dinonaktifkan_tidak_lagi_memberi_kewenangan
✔ Keputusan_dan_penolakan_kewenangan_tercatat_di_log_audit
✔ Kesiapan_mencocokkan_atasan_unit_dengan_izin_persetujuannya
✔ Kesiapan_tidak_menyatakan_siap_bila_atasan_tidak_memegang_izin
✔ Pengajuan_dan_penyerahan_mencatat_pelaku_bukan_id_permintaan
✔ Permintaan_yang_tidak_ada_ditolak_sebagai_tidak_ditemukan
✔ Temuan_penyimpangan_tercatat_sebagai_audit_dan_peringatan
✔ Unit_organisasi_nonaktif_mencabut_kewenangan_atasan_dan_cakupannya
✔ Unit_organisasi_yang_masa_berlakunya_habis_tidak_memberi_kewenangan
```

</details>

<details><summary><code>StatusTransitionTests</code> — 18 kasus</summary>

```text
✔ Antrean_gudang_memuat_yang_disetujui_dan_permintaan_lama_saja
✔ Batas_di_masa_depan_tidak_membuat_permintaan_hari_ini_lolos_tanpa_persetujuan
✔ Batas_kosong_atau_tidak_sah_tidak_membuka_jalan_pintas(batas: "bukan-tanggal")
✔ Batas_kosong_atau_tidak_sah_tidak_membuka_jalan_pintas(batas: null)
✔ Disetujui_lalu_diserahkan_menjadi_selesai_dan_penandanya_konsisten
✔ Ditolak_bersifat_final
✔ Draft_tidak_dapat_diputus_sebelum_diajukan
✔ Jalur_CITO_tidak_dapat_dipakai_untuk_permintaan_rutin
✔ Kasus11_permintaan_sebelum_batas_mengikuti_aturan_peralihan
✔ Kasus12_permintaan_baru_wajib_disetujui_sebelum_diserahkan
✔ Kasus12_permintaan_sesudah_batas_wajib_disetujui
✔ Kasus7_alasan_penolakan_wajib
✔ Kasus8_alasan_CITO_wajib_saat_pengajuan_dan_saat_otorisasi
✔ Kasus9_keputusan_kedua_ditolak_409_dan_hanya_ada_satu_keputusan
✔ Kunci_sama_dengan_isi_berbeda_ditolak
✔ Pengulangan_dengan_kunci_sama_tidak_menggandakan_keputusan
✔ Permintaan_tanpa_waktu_pengajuan_tidak_dianggap_permintaan_lama
✔ Persetujuan_tidak_mengubah_stok
```

</details>

### Uji frontend (`fe-test5.log`)

```text
tests 40
pass 40
fail 0
skipped 0
```

<details><summary>40 uji frontend</summary>

```text
✔ LGS-DEC-001: Logistik berdiri sebagai bagian navigasi sendiri
✔ LGS-DEC-001: sebelas grup menu Logistik tetap ada dengan urutan aslinya
✔ Setiap tautan menu Logistik menunjuk halaman yang benar-benar ada
✔ LGS-BE-ADH-01: layar stok memakai endpoint low-stock dan batch yang nyata
✔ Penelusuran Lokasi Barang memakai saldo nyata dan menunggu obat dipilih
✔ Layar stok hanya memuat tab yang sedang tampil
✔ Detail permintaan menampilkan kekurangan dari selisih diminta dan diserahkan
✔ Route stok Logistik membuka tab dan saringan yang tepat
✔ FIN-LGS-007: Pengajuan Kebutuhan Barang membuka formulir permintaan yang nyata
✔ FIN-LGS-011: Status Pemenuhan dan Riwayat Permintaan memakai entri menu yang sudah ada
✔ Atasan berizin dapat menyetujui dan menolak permintaan yang menunggu
✔ Aturan 7: pemohon tidak mendapat aksi keputusan atas permintaannya sendiri
✔ Tanpa izin, tidak ada aksi keputusan dan alasannya dijelaskan
✔ Aturan 5: otorisasi CITO hanya untuk permintaan CITO dan hanya bagi pemegang izinnya
✔ Persetujuan butuh izin DAN kewenangan atas unit peminta; sebabnya ditampilkan apa adanya
✔ Otorisasi CITO butuh penanda eksplisit dari backend; izin di layar (mis. SuperAdmin) saja tidak cukup
✔ Petugas CITO yang bukan atasan unit diberi tahu mengapa Setujui/Tolak tidak ada
✔ Aturan 8: permintaan yang sudah diputuskan atau ditutup tidak memiliki aksi keputusan
✔ Aturan 3 dan 5: alasan penolakan dan CITO wajib, dengan batas panjang
✔ Label status: Menunggu Persetujuan, Disetujui, Ditolak, dan Dipenuhi Sebagian
✔ Riwayat memakai label aksi berbahasa Indonesia dan membaca alasan CITO pemohon
✔ Kontrak panggilan: endpoint keputusan tidak mengirim status dari layar
✔ Detail: panel keputusan memakai izin, CITO memakai keputusan ketat
✔ Pengajuan CITO meminta alasan dan konfirmasi sebelum dikirim
✔ Halaman persetujuan dan antrean gudang memakai varian daftar yang tepat
✔ LGS-DEC-003: Setujui/Tolak butuh izin Approve eksplisit DAN atasan unit dari backend
✔ LGS-DEC-005: aksi verifikasi susulan hanya bila backend menyatakan berwenang
✔ Keadaan CITO adalah turunan riwayat dengan label yang disepakati, bukan status baru
✔ Kontrak verifikasi susulan: endpoint, hasil berupa angka, tanpa status dari layar
✔ FIN-LGS-FE-002: Verifikasi Susulan CITO mengikuti skema tampilan
✔ FIN-LGS-FE-001: Validasi Atasan memakai cakupan backend, ringkasan sekonsisten tabel, dan kolom skema
✔ FIN-LGS-FE-003: strip perhatian memakai hitungan backend dalam cakupan kewenangan, bukan notifikasi tiruan
✔ Cakupan dan tenggat: angka konstanta sama dengan enum backend, label tenggat dari backend
✔ VER-004: pilihan Cakupan tidak menyesatkan — nilai kosong berarti semua unit, bukan unit yang dipimpin
✔ FIN-LGS-003: alasan CITO pemohon terbaca sebelum petugas mengotorisasi
✔ FIN-LGS-011: progres Status Pemenuhan diturunkan dari status dan waktu backend
✔ FIN-LGS-017: Status Pemenuhan membedakan delapan keadaan tanpa status basis data baru
✔ FIN-LGS-011: Status Pemenuhan dan saringan pemohon memakai data backend
✔ FIN-LGS-011: Riwayat Permintaan adalah linimasa aktivitas dari endpoint histories
✔ FIN-LGS-010: linimasa detail menampilkan alasan atau catatan setiap langkah
```

</details>

### ESLint (`109a4083`, 25 berkas)

```text
0 error, 4 warning
warn use-stock-request-approval-scope.jsx:33 react-hooks/set-state-in-effect
warn use-stock-request-approval-summary.jsx:115 react-hooks/set-state-in-effect
warn use-stock-request-history-log.jsx:58 react-hooks/set-state-in-effect
warn use-stock-request-list.jsx:97 react-hooks/set-state-in-effect
```

---

# KEPUTUSAN FINAL DAN AKTIVASI (INTEGRATION & BUSINESS READINESS)

| Field | Nilai |
| --- | --- |
| Tanggal | 19 November 2026 |
| Versi | Backend `a3b2ed08` (135/135), frontend `109a4083` (40/40). Tidak diubah; tidak ada uji ulang |
| Batas | Tanpa commit, push, PR, merge, deployment, migrasi, maupun perubahan data operasional. Hanya laporan ini yang berubah |

## 1. Keputusan yang sudah disetujui

Yang berstatus `approved` tetap `LGS-DEC-001` sampai `006`, dan **seluruhnya sudah terimplementasi** di `a3b2ed08`/`109a4083`. `LGS-OQ-024` sampai `029` masih OPEN, dan tidak ada keputusan baru di decision log (r7). Karena itu tidak ada source yang perlu diubah pada tahap ini, dan tidak ada kebijakan akses baru yang diterapkan.

## 2. Materi PR final

**Urutan merge:** PR backend → merge → deploy backend → PR frontend → merge.

| Butir | Backend | Frontend |
| --- | --- | --- |
| Arah | `Ikbal` → `QuilvianIntegrationBackend` | `Ikbalv2` → `QuilvianIntegrationFrontend` |
| Commit | `6302f5fc`, `a3b2ed08` | `002f0bcd`, `109a4083` |
| Konflik | Tidak ada (`merge-tree` bersih; integration `659d4635` tidak menyentuh berkas maupun dependensi Logistik) | Tidak ada (`merge-tree` bersih; bagian Logistik `menu-items.jsx` hasil gabungan identik) |
| Hasil uji | Build PASS; 135/135 (SQLite in-memory) | 40/40; ESLint 0 error |
| Isi lengkap | Draf PR pada *FINAL INTEGRATION READINESS* butir 2 | Draf PR pada bagian yang sama |

**Daftar periksa reviewer (backend):**

- tidak ada migrasi;
- konfigurasi baru kosong secara bawaan sehingga CITO tertutup;
- izin baru `StockRequest:ViewApprovalReadiness`;
- `DecidedAt` hanya keputusan atasan;
- respons 400/409 baru;
- log audit memakai `StockRequestId`/`UserId`.

**Daftar periksa reviewer (frontend):**

- route baru `/stock-requests/history` dan `/stock-requests/fulfillment`;
- tiga `pathname` di menu Logistik;
- tidak ada entri menu baru;
- bergantung pada backend yang sudah di-merge.

## 3. Aktivasi approval dan CITO: data, konfigurasi, dan PIC

**Temuan baru yang memblokir:** kolom `MstServiceUnit.OrganizationUnitId` ada di basis data, tetapi **tidak ada API maupun layar yang dapat mengisinya**. `ServiceUnitDtos.cs` tidak memuatnya, dan tidak ada kode yang menulisnya. Kolom ditambahkan 27 Agustus 2026 (`f75ea039`). Master unit layanan milik modul MasterData, di luar Logistik. Tanpa tautan ini tidak ada satu pun atasan unit yang dapat ditentukan.

| Urutan | Data / konfigurasi | Tempat pengisian | Format / contoh | PIC | Bukti siap (`GET /stock-requests/approval-readiness`) |
| --- | --- | --- | --- | --- | --- |
| 1 | Tautan unit layanan → unit organisasi (20 unit aktif) | **Belum ada layar/API.** Pilihan: (a) pemilik MasterData menambah field "Unit Organisasi" pada master unit layanan (DTO + form); (b) skrip SQL sekali jalan yang ditinjau dan disetujui, dengan daftar pasangan dari HRD | Satu unit organisasi aktif per unit layanan | HRD menyiapkan daftar pasangan; pemilik MasterData (a) atau DBA + pemilik data (b) | `mappedServiceUnits` = jumlah unit yang ditautkan |
| 2 | Penempatan manajerial kepala unit | Editor tenaga kerja HR → Penempatan Organisasi → "Penempatan Manajerial", unit organisasi, tanggal berlaku | Aktif, berlaku hari ini, tidak dibatalkan | HRD | `mappedServiceUnitsWithSupervisor`, `units[].activeSupervisors` |
| 3 | Izin `StockRequest:Approve` | `/administrator/settings/role-access` → departemen × jabatan kepala unit | Aksi Approve pada resource StockRequest | Administrator Akses Role | `approvePermissionGrants`, `units[].authorizedSupervisors`, **`isReadyForApproval = true`** |
| 4 | Jabatan pengotorisasi CITO | Keputusan manajemen, lalu konfigurasi lingkungan `PharmacyManagement__StockRequest__EmergencyAuthorizerPositions` | Id atau kode jabatan `MstPosition`, dipisah koma; jabatan nonaktif diabaikan | Manajemen (keputusan); operator/DevOps (konfigurasi) | `emergencyDesignatedPositions` |
| 5 | Izin `StockRequest:AuthorizeEmergency` | `/administrator/settings/role-access` → jabatan yang ditunjuk pada butir 4 | Aksi AuthorizeEmergency | Administrator Akses Role | `emergencyDesignatedPositionsWithGrant`, **`isReadyForEmergency = true`** |
| 6 | Tenggat verifikasi susulan | Konfigurasi `…__EmergencyReviewDueHours` dan (opsional) `…__EmergencyReviewWarningHours` | Jam > 0, ≤ 100 tahun; peringatan < tenggat | Manajemen (nilai); operator (konfigurasi) | `emergencyReviewDeadline`; blocker "Tenggat…" hilang |
| 7 | Batas aktivasi persetujuan | Konfigurasi `…__ApprovalEnforcedFromUtc`, saat go-live | ISO UTC, tidak di masa depan; boleh kosong bila tidak ada permintaan Submitted lama | Operator, atas arahan pemrakarsa | `approvalCutoverState` |
| 8 | Izin laporan kesiapan | `/administrator/settings/role-access` → `StockRequest:ViewApprovalReadiness` | — | Administrator Akses Role, setelah `LGS-OQ-028` | Endpoint di atas dapat dibuka |
| 9 | Izin operasional lain | Role Access: Read/Create/Update/Cancel untuk petugas unit; Fulfill untuk gudang | Izin existing | Administrator Akses Role | Uji U1–U5 |
| 10 | Penegakan izin di lingkungan UAT | `Security__Authorization__Enabled=true` | — | Operator/DevOps | Akses ditolak untuk akun tanpa izin (U5.5) |

**Ringkas:** butir 1–3 membuka persetujuan atasan; butir 4–6 membuka CITO dan verifikasi; butir 7 saat go-live; butir 8–10 sebelum UAT.

**Keputusan yang dibutuhkan untuk butir 1:** jalur (a) atau (b), dan siapa pelaksananya. Jalur (a) adalah perubahan modul MasterData dan butuh persetujuan pemiliknya. Jalur (b) adalah perubahan data dan butuh persetujuan tertulis.

## 4. `LGS-OQ-027` dan `LGS-OQ-029`: pilihan dengan rekomendasi bawaan

Belum ada yang diimplementasikan. Setiap butir butuh persetujuan; rekomendasi adalah default yang dipakai bila disetujui tanpa perubahan.

| ID | Pertanyaan | Rekomendasi default | Dampak backend | Dampak frontend |
| --- | --- | --- | --- | --- |
| **D1** | Siapa yang termasuk "unit sendiri" | Penempatan HR aktif (manajerial maupun tidak) pada unit organisasi yang ditautkan ke unit layanan, ditambah permintaan yang dibuat, diajukan, atau dimintakan atas nama pengguna | Penyelesai cakupan baru di `StockRequestApprovalAuthority` (penempatan → unit organisasi → unit layanan). Predikat dipasang di `GetPagedAsync`, `GetHistoryPagedAsync`, dan `GetDetailAsync` sebelum penghitungan. **Bergantung pada butir 3.1** | Tidak ada perubahan wajib; daftar otomatis mengecil |
| **D2** | Apa yang dilihat gudang | Permintaan ke gudang yang `MstDrugStorageLocation.ServiceUnitId`-nya adalah unit pengguna (D1). Masa peralihan: pemegang `StockRequest:Fulfill` melihat antrean yang dapat diserahkan | Join lokasi → unit layanan pada predikat. Isi `ServiceUnitId` gudang di basis data belum diukur | Antrean gudang tetap; mungkin kosong bila `ServiceUnitId` gudang belum diisi |
| **D3** | Izin pemantauan lintas unit | Aksi baru `StockRequest:ReadAllUnits` (didaftarkan lewat `[AccessAction]`), diberikan kepada Kepala Instalasi Farmasi/Logistik dan audit internal | Pendaftaran aksi + cabang "semua unit" pada predikat | Opsional: label "Semua unit" pada daftar |
| **D4** | SuperAdmin | Tidak otomatis lintas unit; perlu penempatan dengan `ReadAllUnits` | Predikat tidak membaca peran SuperAdmin (sama dengan aturan persetujuan) | — |
| **D5** | Pemohon yang pindah unit | Tetap melihat permintaan yang ia buat atau ajukan | Cabang "milik sendiri" pada predikat (`CreateBy`, riwayat Create/Submit, `RequestedByWorkforceId`) | — |
| **D6** | Jawaban di luar cakupan | `404 Not Found` (tidak mengungkap keberadaan) | `GetDetailAsync` publik mengembalikan null; perintah memeriksa cakupan baca lebih dulu sebelum kewenangan | Pesan "Permintaan stok tidak ditemukan" sudah ada |
| **D7** | Cara aktivasi | Saklar `PharmacyManagement:StockRequest:ReadScopeEnforced` (bawaan `false`), dinyalakan per lingkungan setelah `approval-readiness` menunjukkan tautan unit lengkap; uji per peran × endpoint | Saklar + logging sekali per nilai (pola cutover); uji teori per peran | — |
| **W1** | Sumber petugas peminta | Diambil dari akun yang login (`WorkforceProfileId`); `422` bila akun tidak punya profil tenaga kerja | `CreateAsync` mengabaikan atau mencocokkan `RequestedByWorkforceId` klien | Field "Petugas Peminta" menjadi tampilan saja, tidak dipilih |
| **W2** | Buat, ubah, ajukan | Hanya untuk unit sendiri (D1); ubah/ajukan oleh pemohon atau anggota unit yang sama; `403` bila tidak | Pemeriksaan di `CreateAsync`, `UpdateAsync`, `SubmitAsync` | Pilihan unit di formulir dibatasi pada unit pengguna (butuh endpoint "unit saya", perluasan `approval-scope`) |
| **W3** | Pembatalan | Draf/Menunggu: pemohon atau anggota unit; sesudah Disetujui: hanya atasan unit, dan hanya sebelum diserahkan | Pemeriksaan di `CancelAsync`; `CanCancel` dihitung per pengguna di detail | Tombol Batal mengikuti `canCancel` dari backend |

**Perkiraan pekerjaan setelah disetujui** (tanpa migrasi):

- backend: penyelesai cakupan, predikat di tiga jalur baca, pemeriksaan di empat perintah tulis, saklar, aksi izin baru, dan uji per peran;
- frontend: formulir (W1/W2) dan tombol Batal (W3).

**Prasyarat aktivasi:** butir 3.1–3.3.

## 5. Prioritas sisa menu Logistik

Urutan berdasarkan ketergantungan: penerimaan mengisi stok masuk dan menjadi dasar nilai; opname mengoreksi saldo; laporan membutuhkan keduanya agar lengkap.

### 5.1 Penerimaan Barang (5 entri belum tertaut)

| Entri | Kemampuan existing | Dapat dikerjakan sekarang? | Butuh keputusan | Butuh migrasi |
| --- | --- | --- | --- | --- |
| Pemeriksaan Jumlah & Kondisi | `LGS-CAP-08` (kondisi di Goods Receipt Finance), *Reuse with adapter* | **Sebagian**: tampilan baca kondisi GR, setelah kontrak API dikonfirmasi pemilik Finance | Ya: arti kode kondisi dan siapa memeriksa | Tidak (tampilan baca) |
| Barang Tidak Sesuai | `LGS-CAP-08` | Sama dengan di atas (saring kondisi tidak sesuai) | Ya | Tidak (tampilan baca) |
| Pencatatan Batch/Lot, Tanggal Kadaluarsa | `LGS-CAP-07` *Conflict* (item PO teks bebas dari BLM, tanpa pemetaan ke master obat) | Tidak | `LGS-OQ-017` (penerimaan gudang sendiri vs PO tertaut katalog), `LGS-OQ-018` (peran BLM) | Ya: pemetaan produk BLM ↔ master obat, dokumen penerimaan gudang |
| Dokumen Penerimaan | Goods Receipt Finance (sudah ditautkan sebagai "Pemeriksaan Hasil Pengadaan") | Tidak tanpa menggandakan tautan | Ya: apakah ini dokumen penerimaan gudang (setelah `LGS-OQ-017`) | Ya bila dokumen gudang sendiri |

### 5.2 Stock Opname & Penyesuaian (6 entri)

| Entri | Kemampuan existing | Dapat dikerjakan sekarang? | Butuh keputusan | Butuh migrasi |
| --- | --- | --- | --- | --- |
| Jadwal, Perhitungan Fisik, Selisih Stok, Verifikasi Hasil, Persetujuan Penyesuaian, Berita Acara | `LGS-CAP-24` *Missing* (pola disetujui di `PHA-DEC-030`); `LGS-CAP-25` penyesuaian ada tetapi tanpa maker-checker | Tidak | `LGS-OQ-009` (rekomendasi: ikuti `PHA-DEC-030`), toleransi selisih, pejabat penyetuju | Ya: sesi opname, baris hitung, persetujuan; posting lewat penyesuaian existing |

### 5.3 Laporan & Rekonsiliasi (8 entri) dan entri terkait

| Entri | Kemampuan existing | Dapat dikerjakan sekarang? | Butuh keputusan | Butuh migrasi |
| --- | --- | --- | --- | --- |
| Laporan Persediaan (jumlah) | Saldo per batch/lokasi (`LGS-CAP-10`) | **Ya**: laporan jumlah, tanpa nilai | Format dan pemilik laporan (`LGS-OQ-015`) | Tidak |
| Mutasi | Kartu stok (`LGS-CAP-31`, *Ready*) | **Ya**: rekap per periode/lokasi/jenis mutasi | Sama | Tidak |
| Distribusi | Penyerahan permintaan + transfer (`LGS-CAP-17`, `20`) | **Ya**: jumlah per unit/gudang/periode | Sama | Tidak |
| Pemakaian per Unit | `PhmDrugUsages` (`LGS-CAP-23`, *Reuse with adapter*) | **Ya** (dari data pemakaian yang ada) | Sama; arti "unit memegang stok" (`LGS-OQ-003`) | Tidak |
| Dashboard Penerimaan & Pengeluaran | Mutasi masuk/keluar (`LGS-CAP-31`) | **Ya** (jumlah) | Sama | Tidak |
| Penerimaan (laporan) | `LGS-CAP-07` | Tidak | `LGS-OQ-017` | Ya |
| Stock Opname (laporan) | `LGS-CAP-24` | Tidak | `LGS-OQ-009` | Ya |
| Nilai Persediaan | `LGS-CAP-32` *Missing* (tanpa harga per batch) | Tidak | `LGS-OQ-008` (metode valuasi) | Ya: harga per batch/mutasi |
| Rekonsiliasi dengan Keuangan | `LGS-CAP-33` *Missing* | Tidak | `LGS-OQ-012` | Ya / integrasi Akuntansi |

**Rekomendasi urutan kerja:**

1. **Sekarang, tanpa migrasi:** lima laporan berbasis jumlah (Laporan Persediaan, Mutasi, Distribusi, Pemakaian per Unit, Dashboard Penerimaan & Pengeluaran) dan tampilan baca kondisi GR. Prasyaratnya: penugasan task dari pemrakarsa, pemilik domain (`LGS-OQ-015`), dan konfirmasi kontrak API Finance untuk GR.
2. **Setelah `LGS-OQ-017`/`018` dan izin migrasi:** Penerimaan Barang lengkap.
3. **Setelah `LGS-OQ-009` dan izin migrasi:** Stock Opname.
4. **Setelah `LGS-OQ-008`/`012`:** Nilai Persediaan dan Rekonsiliasi.

Entri lain yang belum tertaut (Penyimpanan 4, Distribusi 5, Mutasi & Retur 3, Pengendalian 3, Stok per Unit, Mutasi Saldo, Penanggung Jawab Gudang, Pemeriksaan Stok) tetap BLOCKED_FEATURE sesuai *KONSOLIDASI AKHIR* butir 5.

## 6. Keputusan yang diminta dari pemrakarsa (untuk menutup P1 dan melanjutkan UAT)

| No | Keputusan | Pilihan | Rekomendasi |
| --- | --- | --- | --- |
| 1 | Jalur pengisian tautan unit layanan → unit organisasi | (a) pemilik MasterData menambah field di master unit layanan; (b) skrip SQL sekali jalan yang disetujui | **(a)** untuk jangka panjang; (b) hanya bila UAT harus segera |
| 2 | PIC HRD untuk daftar tautan unit dan penempatan manajerial | Nama/jabatan | — |
| 3 | Jabatan atasan unit yang diberi `StockRequest:Approve` | Daftar departemen × jabatan | Kepala ruangan/instalasi per unit layanan |
| 4 | Jabatan pengotorisasi CITO dan izinnya | Daftar jabatan | Supervisor jaga/kepala IGD (keputusan manajemen) |
| 5 | Tenggat verifikasi susulan | Jam (dan jam peringatan) | Ditetapkan manajemen; sistem tidak memberi bawaan |
| 6 | Tanggal aktivasi persetujuan | Tanggal/jam UTC | Saat go-live, setelah butir 1–5 |
| 7 | `LGS-OQ-027` D1–D7 | Setuju default / ubah | Setuju default |
| 8 | `LGS-OQ-029` W1–W3 | Setuju default / ubah | Setuju default, bersama butir 7 |
| 9 | `LGS-OQ-028` pemegang izin kesiapan | Jabatan | Administrator Logistik/Farmasi + Akses Role |
| 10 | Mulai lima laporan berbasis jumlah tanpa migrasi | Ya / tunda | Ya, setelah PR Logistik di-merge |

**Status akhir:**

- PR backend lalu frontend: **GO**.
- UAT terbatas U1–U6: **siap setelah merge dan deploy**.
- UAT penuh U7: **NO-GO** sampai keputusan 1–6 terlaksana dan `isReadyForApproval`/`isReadyForEmergency` bernilai `true`.
- Produksi: **NO-GO**.


---

# IMPLEMENTASI LAPORAN LOGISTIK (FIN-LGS-024 s.d. FIN-LGS-027)

**Tanggal:** 19 November 2026
**Backend:** `C:\tmp\phm`, branch `Ikbal`, HEAD `a3b2ed08`, ditambah perubahan yang **belum di-commit**.
**Frontend:** `QuilvianSystemFrontendDev`, branch `Ikbalv2`, HEAD `109a40831`, ditambah perubahan yang **belum di-commit**.
**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, perubahan basis data, deployment, build backend, build frontend, dan dev server. Source MasterData juga tidak diubah.

## 1. Status task

| Task | Status | Keterangan |
| --- | --- | --- |
| FIN-LGS-024 | **SELESAI DI SOURCE, BELUM DIKOMPILASI** | Lima laporan selesai di backend dan frontend, tanpa migrasi. Uji frontend dan ESLint sudah dijalankan. Backend belum di-build dan uji backend belum dijalankan, karena keduanya dilarang pada tahap ini. |
| FIN-LGS-025 | **SELESAI (SPESIFIKASI)** | Spesifikasi `MstServiceUnit.OrganizationUnitId` ada di bagian 9. Pelaksananya pemilik MasterData: **BLOCKED_OWNER**. |
| FIN-LGS-026 | **SELESAI** | `LGS-OQ-027` dan `LGS-OQ-029` tetap pending. Tidak ada pembatasan baru (bagian 10). |
| FIN-LGS-027 | **SELESAI** | Hanya bagian ini yang ditambahkan ke laporan. |

## 2. Lima laporan dalam bahasa sehari-hari

Semua laporan **hanya membaca** data yang sudah tersimpan. Tidak ada angka contoh, angka simulasi, atau transaksi yang dikarang. Bila sumber data suatu angka belum ada, layar menampilkan kotak **"Keterbatasan laporan"** yang tidak dapat ditutup.

Ada tiga aturan angka yang berlaku untuk semua laporan:

1. **Tidak ada nilai rupiah.** Sistem belum menyimpan harga per batch (`LGS-CAP-32`), dan metode valuasinya masih menunggu `LGS-OQ-008`.
2. **Jumlah barang tidak dijumlahkan lintas obat.** Contoh: 10 tablet parasetamol ditambah 2 botol sirup tidak menghasilkan angka yang bermakna. Jumlah barang selalu ditampilkan per obat dalam satuannya sendiri.
3. **Ringkasan di bagian atas layar berupa hitungan**, misalnya jumlah transaksi, jumlah jenis obat, atau jumlah baris. Ringkasan bukan total barang.

| Laporan | Pertanyaan yang dijawab | Contoh membaca |
| --- | --- | --- |
| **Laporan Persediaan** | "Saat ini berapa stok obat X di gudang Y, dan dalam keadaan apa?" | Parasetamol di Gudang Induk: siap pakai 100, ditahan resep 10, sehingga **dapat dikeluarkan 90**; karantina 5; rusak 2; total fisik 107; kedaluwarsa terdekat 7 Jun 2027 (200 hari lagi). |
| **Laporan Mutasi Barang** | "Dalam periode ini, berapa banyak obat X yang masuk, keluar, dan disesuaikan di gudang Y?" | Pada 1–30 Nov: saldo pembuka dicatat 50, masuk 20, keluar 15, penyesuaian −2. Perubahan bersihnya +53, dan saldo saat ini 53. |
| **Laporan Distribusi Barang** | "Barang apa saja yang sudah diserahkan gudang ke unit, dan apakah ada kekurangan?" | Ruang Melati, Parasetamol (tablet): 1 permintaan, diminta 10, diserahkan 6, kurang 4 (1 baris kurang). |
| **Laporan Pemakaian per Unit** | "Unit layanan mana memakai obat atau alat kesehatan apa, berapa banyak, untuk berapa kunjungan?" | IGD, Kasa steril (lembar): dipakai 5 lembar dalam 2 dokumen pemakaian untuk 1 kunjungan. |
| **Dashboard Penerimaan & Pengeluaran** | "Bagaimana pola keluar-masuk stok per hari, dan dari dokumen apa?" | 30 hari terakhir: 12 transaksi masuk (Transfer Antar Lokasi), 40 transaksi keluar (Permintaan Unit dan Pemakaian Pasien). Grafik harian dan 10 obat dengan jumlah keluar terbesar juga ditampilkan. |

**Alur bisnis yang tercermin:**

1. Permintaan unit diserahkan gudang. Mutasi *Keluar* tercatat dengan sumber "Permintaan Unit", lalu muncul di Laporan Distribusi menurut waktu penyerahan.
2. Pemakaian pasien yang dicatat memotong stok. Muncul sebagai *Keluar* bersumber "Pemakaian Pasien", dan di Laporan Pemakaian per Unit menurut unit layanan kunjungan.
3. Transfer antar lokasi tercatat sebagai *Keluar* di lokasi asal, lalu *Masuk* di lokasi tujuan.
4. Penerimaan dari pengadaan (Goods Receipt) **belum** menambah stok gudang (`LGS-CAP-07`, `LGS-OQ-017`). Karena itu tidak muncul sebagai barang masuk, dan dashboard menyatakannya sebagai keterbatasan.

## 3. Kontrak API

Kelima endpoint memakai `GET`, hanya membaca, dan dilindungi `[Authorize]` + `[AccessPermission]` dengan **resource izin yang sudah ada**. Tidak ada aksi izin baru, tidak ada perubahan `Program.cs`, tidak ada kolom maupun tabel baru. Respons dibungkus `ApiResponse<T>` seperti endpoint lain.

**Aturan periode** (empat laporan berperiode):

- `startDate` dan `endDate` dibaca sebagai tanggal lokal server, batas akhirnya inklusif.
- Bila keduanya kosong, periode = **30 hari terakhir** sampai hari ini.
- Bila hanya `endDate` diisi, periode dimulai 29 hari sebelumnya.
- Periode yang tidak sah dijawab **422** dengan `data.code = "PHM035"`. Tidak sah berarti mulai sesudah selesai, atau rentang lebih dari **366 hari**. Batas 366 hari adalah batas teknis, karena penjumlahan dilakukan di memori sesudah data disaring di basis data.

**Halaman:** `pageNumber` (bawaan 1) dan `pageSize` (1–200, bawaan 25). Nilai di luar batas kembali ke bawaan.

### 3.1 `GET /api/v1/health-services/pharmacy-management/drug-stock/reports/inventory` — Laporan Persediaan

| Atribut | Nilai |
| --- | --- |
| Izin | `DrugStock:Read` (`SortOrder` 11) |
| Query | `storageLocationId?` (Guid), `drugId?` (Guid), `search?` (≤100, nama/kode obat), `includeEmpty` (bool, bawaan `false`), `pageNumber`, `pageSize` |
| 200 | `InventoryReportResponse` |
| 401 / 403 | Belum masuk / tidak memegang izin |

```json
{
  "generatedAtUtc": "2026-11-19T03:00:00Z",
  "summary": { "rowCount": 1, "drugCount": 1, "locationCount": 1, "rowsWithoutAvailable": 0,
               "rowsWithQuarantine": 1, "rowsWithExpired": 0, "rowsWithDamaged": 1,
               "rowsWithExpiredBatchStillAvailable": 0 },
  "rows": { "pageNumber": 1, "pageSize": 25, "totalData": 1, "totalPage": 1, "items": [
    { "drugId": "…", "drugCode": "OBT-001", "drugName": "Parasetamol 500 mg", "stockUnitName": "Tablet",
      "storageLocationId": "…", "storageLocationName": "Gudang Induk",
      "availableOnHand": 100, "reserved": 10, "availableToIssue": 90, "quarantine": 5, "expired": 0,
      "damaged": 2, "totalOnHand": 107, "batchCount": 2, "nearestExpiryDate": "2027-06-07",
      "daysToNearestExpiry": 200 } ] },
  "limitations": ["Nilai persediaan (rupiah) belum tersedia: …", "Jumlah ditampilkan per obat …",
                  "Saldo adalah keadaan saat laporan dibuat; …"]
}
```

Catatan field:

- `availableToIssue` = siap pakai − ditahan.
- `nearestExpiryDate` dihitung hanya dari batch siap pakai yang masih bersaldo.
- Baris yang semua saldonya nol dan tidak ada yang ditahan disembunyikan, kecuali `includeEmpty=true`.

### 3.2 `GET …/drug-stock/reports/mutations` — Laporan Mutasi Barang

| Atribut | Nilai |
| --- | --- |
| Izin | `DrugStock:Read` (`SortOrder` 12) |
| Query | `startDate?`, `endDate?`, `storageLocationId?`, `drugId?`, `search?`, `pageNumber`, `pageSize` |
| 200 | `MutationReportResponse` = `{ period, summary, rows, limitations }` |
| 422 | `PHM035` |

- `period` = `{ startDate, endDate, dayCount }`, yaitu periode yang benar-benar dipakai.
- `summary` = `mutationCount`, `inCount`, `outCount`, `adjustmentCount`, `openingCount`, `statusChangeCount`, `drugCount`, `locationCount`.
- Baris = `drugId/Code/Name`, `stockUnitName`, `storageLocationId/Name`, `openingRecorded`, `quantityIn`, `quantityOut` (positif), `adjustmentNet` (bertanda), `netChange`, `mutationCount`, `lastMutationAt`, `currentOnHand`.
- Perubahan status (mis. ke karantina) **tidak** dihitung sebagai masuk/keluar; hanya jumlah transaksinya yang dilaporkan.
- `currentOnHand` adalah saldo saat laporan dibuat, bukan saldo akhir periode.

### 3.3 `GET …/drug-stock/reports/flow-summary` — Dashboard Penerimaan & Pengeluaran

| Atribut | Nilai |
| --- | --- |
| Izin | `DrugStock:Read` (`SortOrder` 13) |
| Query | `startDate?`, `endDate?`, `storageLocationId?`, `drugId?` |
| 200 | `StockFlowSummaryResponse` |
| 422 | `PHM035` |

Isi respons:

- `totals`: `inMutationCount`, `outMutationCount`, `adjustmentCount`, `openingCount`, `statusChangeCount`, `drugsReceivedCount`, `drugsIssuedCount`.
- `bySource[]`: `{ direction: In|Out|Adjustment|Opening, sourceDocumentType, mutationCount, drugCount }`. Nilai `sourceDocumentType` ditampilkan apa adanya, `null` bila tidak dicatat.
- `daily[]`: satu baris untuk **setiap** hari dalam periode, berisi `{ date, inCount, outCount, adjustmentCount }`.
- `topReceivedDrugs[]` dan `topIssuedDrugs[]`: masing-masing 10 teratas, `{ drugId, drugCode, drugName, stockUnitName, quantity, mutationCount }`.
- `hasProcurementReceipts`: `true` bila ada masuk bersumber `GoodsReceipt`.
- `limitations`.

### 3.4 `GET /api/v1/health-services/pharmacy-management/stock-requests/reports/distribution` — Laporan Distribusi Barang

| Atribut | Nilai |
| --- | --- |
| Izin | `StockRequest:Read` (`SortOrder` 5) |
| Query | `startDate?`, `endDate?`, `requestingServiceUnitId?`, `storageLocationId?`, `drugId?`, `search?` (obat/unit), `scope` (`0` All, `1` MyAuthority; bawaan 0), `pageNumber`, `pageSize` |
| 200 | `{ period, summary, rows, limitations }` |
| 422 | `PHM035` |

- Sumber data: permintaan berstatus **Selesai** yang punya riwayat `FulfillStockRequest` dalam periode, ditambah baris barang yang `FulfilledQuantity`-nya terisi.
- Pengelompokan: unit peminta × obat × satuan.
- `summary` = `completedRequestCount`, `partiallyFulfilledRequestCount`, `lineCount`, `unitCount`, `drugCount`.
- Baris = `requestingServiceUnitId/Name`, `drugId/Code/Name`, `measurementId/Name`, `requestCount`, `requestedQuantity`, `fulfilledQuantity`, `shortfallQuantity`, `shortfallLineCount`, `lastFulfilledAt`.
- Cakupan baca **sama dengan daftar permintaan** (bagian 10). `scope=1` memakai penyelesai kewenangan yang sama dengan "Unit yang Saya Pimpin".

### 3.5 `GET /api/v1/health-services/pharmacy-management/drug-usages/reports/by-unit` — Laporan Pemakaian per Unit

| Atribut | Nilai |
| --- | --- |
| Izin | `DrugUsage:Read` (`SortOrder` 7) |
| Query | `startDate?`, `endDate?`, `serviceUnitId?` (unit layanan kunjungan), `storageLocationId?` (lokasi stok yang dipotong), `drugId?`, `search?` (obat/unit), `pageNumber`, `pageSize` |
| 200 | `{ period, summary, rows, limitations }` |
| 422 | `PHM035` |

- Sumber data: `PhmDrugUsage` berstatus **NotBilled** atau **Billed** dengan `UsedAt` dalam periode, di-join ke `RegPatientEncounter.ServiceUnitId`. Draf dan pemakaian yang dibatalkan tidak dihitung.
- `summary` = `usageDocumentCount`, `encounterCount`, `unitCount`, `drugCount`.
- Baris = `serviceUnitId/Name`, `drugId/Code/Name`, `measurementId/Name`, `usageDocumentCount`, `encounterCount`, `quantity`, `lastUsedAt`.

## 4. Skema tampilan

Kelima halaman berada di bawah `/health-services/pharmacy-management/logistics-reports/…`. Semuanya memakai base component Quilvian: `Hero`, `DataFilter`, `FilterSelect`, `FilterDatePicker`, `SummaryGrid`, `DataTable`, `StatusBadge`, `InformationAlert`, `BaseButton`, dan `AccessDeniedGate`. Pemilih lokasi, obat, dan unit memakai `StockRequestResourceSelect`, yaitu pembungkus `useSelectResource` yang sudah ada. Lima entri menu yang **sudah ada** kini tertaut. Tidak ada entri baru: 67 entri, dan yang tertaut naik dari 27 menjadi 32.

| Menu (grup) | Route | Saringan | Kartu ringkasan | Kolom tabel |
| --- | --- | --- | --- | --- |
| Laporan Persediaan (Laporan & Rekonsiliasi) | `…/logistics-reports/inventory` | cari obat, lokasi, obat, "termasuk saldo nol" | Baris obat × lokasi, jenis obat, lokasi, tidak ada yang dapat dikeluarkan, ada karantina/kedaluwarsa/rusak, batch lewat kedaluwarsa masih siap pakai | Obat (kode, satuan), Lokasi, Siap Pakai (+ditahan), Dapat Dikeluarkan, Karantina, Kedaluwarsa, Rusak, Total Fisik, Batch, Kedaluwarsa Terdekat (+sisa hari) |
| Mutasi (Laporan & Rekonsiliasi) | `…/logistics-reports/mutations` | cari obat, tanggal mulai/selesai, lokasi, obat | Transaksi mutasi/masuk/keluar, penyesuaian, saldo pembuka, perubahan status, jenis obat, lokasi | Obat, Lokasi, Saldo Pembuka Dicatat, Masuk, Keluar, Penyesuaian (±), Perubahan Bersih (±), Transaksi, Mutasi Terakhir, Saldo Saat Ini |
| Distribusi (Laporan & Rekonsiliasi) | `…/logistics-reports/distribution` | cari obat/unit, tanggal, unit peminta, gudang, obat, cakupan | Permintaan diserahkan, diserahkan sebagian, baris barang, unit peminta, jenis obat | Unit Peminta, Obat, Satuan, Permintaan, Diminta, Diserahkan, Kekurangan (+baris kurang), Penyerahan Terakhir |
| Pemakaian per Unit (Laporan & Rekonsiliasi) | `…/logistics-reports/usage-by-unit` | cari obat/unit, tanggal, unit layanan, lokasi stok, obat | Dokumen pemakaian, kunjungan pasien, unit layanan, jenis obat | Unit Layanan, Obat/Alkes, Satuan, Jumlah Dipakai, Dokumen, Kunjungan, Pemakaian Terakhir |
| Penerimaan & Pengeluaran (Dashboard Logistik) | `…/logistics-reports/flow-summary` | tanggal, lokasi, obat | Transaksi masuk/keluar, penyesuaian, saldo pembuka, perubahan status, jenis obat masuk/keluar | Grafik batang harian (Masuk/Keluar/Penyesuaian) + "Lihat tabel harian"; tabel Transaksi per Sumber; daftar 10 obat paling banyak masuk/keluar; tautan ke Laporan Mutasi |

**Keadaan layar** (sama di kelima halaman):

| Keadaan | Tampilan |
| --- | --- |
| Memuat | Tabel menampilkan "Mengambil data…" dan kartu berupa kerangka. Permintaan lama dibatalkan (`AbortController`) bila saringan berubah. |
| Kosong | Kalimat khusus per laporan, mis. "Tidak ada mutasi stok dalam periode dan saringan ini." |
| Galat | Panel merah berisi pesan backend dan tombol **Coba lagi**. |
| 403 | Layar "Akses Ditolak" dari `AccessDeniedGate`. |
| Periode tidak sah | Diperiksa di layar dengan aturan yang sama dengan backend (`PHM035`). Muncul peringatan kuning, permintaan tidak dikirim, dan hasil lama tidak ditampilkan. |
| Keterbatasan | Kotak biru "Keterbatasan laporan" berisi kalimat dari backend; tidak dapat ditutup. |
| Periode | Baris "Periode laporan: 1 Nov 2026 – 30 Nov 2026 (30 hari)". Laporan Persediaan menampilkan "Keadaan saldo per: …". |

**Grafik harian.** Warna seri Masuk `#0891b2`, Keluar `#d97706`, Penyesuaian `#6366f1`. Palet ini lolos validator palet untuk latar terang: pita kecerahan, chroma, jarak buta warna ΔE ≥ 19, dan kontras ≥ 3:1. Bentuk grafiknya:

- satu sumbu Y bilangan bulat;
- legenda selalu tampil;
- tooltip per hari;
- rincian tersedia sebagai tabel, sehingga identitas seri tidak bergantung pada warna saja.

Grafik menghitung **transaksi**, bukan jumlah barang. Varian gelap belum disediakan karena halaman Farmasi/Logistik memakai latar terang.

## 5. Perbaikan tambahan di layar Logistik yang sudah ada

Saat memeriksa base component, ditemukan dua kesalahan sambungan prop di layar Logistik yang sudah di-commit. Keduanya diperbaiki satu banding satu, tanpa mengubah perilaku lain:

1. **Halaman tabel tidak dapat dipindah.** Layar mengirim `pagination={{ pageNumber, pageSize, totalData, totalPage, onPageChange }}`, padahal `DataTable` (`src/components/features/base-features/data-table.jsx:143-152`) hanya membaca prop tingkat atas. Akibatnya bilah halaman selalu menampilkan "0 sampai 0 dari 0 data", dan tombol *Berikutnya* nonaktif walaupun data lebih dari satu halaman.
2. **Kotak cari tidak ikut reset.** Layar mengirim `searchValue`, padahal `DataFilter` membaca `searchKeyword` (`data-filter.jsx:293`). Akibatnya ada dua gejala:
   - kata kunci yang dihapus dengan tombol hapus atau backspace sampai kosong tetap tersimpan sebagai saringan;
   - tombol reset tidak mengosongkan kotak cari.

Layar yang diperbaiki adalah tujuh layar yang dapat dibuka dari menu Logistik:

- Data/Validasi/Antrean Permintaan (`stock-request-list-view.jsx`);
- Riwayat Permintaan;
- Verifikasi Susulan CITO;
- Stok Obat (semua tab);
- Kartu Stok (hanya halaman);
- Retur Obat;
- Transfer Stok.

Uji regresi ada di `tests/unit/logistics-reports.test.mjs`.

## 6. Daftar berkas

**Backend (`C:\tmp\phm`)**

| Status | Berkas | Isi |
| --- | --- | --- |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` | DTO query dan respons kelima laporan, `StockFlowDirections` |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` | `LogisticsReportPeriod` (`PHM035`), `LogisticsReportUnprocessableException`, query kelima laporan |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` | Delegasi Persediaan, Mutasi, Dashboard |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` | `GetDistributionReportAsync` dengan cakupan yang sama dengan daftar |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` | Delegasi Pemakaian per Unit |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` | Tiga endpoint dan helper `LogisticsReportUnprocessable` (422) |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` | `GET reports/distribution` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` | `GET reports/by-unit` |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` | 6 uji (bagian 7) |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | Bagian ini |

Di luar repo, proyek uji terisolasi `%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj` ditambah glob `LogisticsReportTests\*.cs`.

**Frontend (`QuilvianSystemFrontendDev`)**

| Status | Berkas |
| --- | --- |
| Baru | `src/lib/constants/health-services/pharmacy-management/logistics-report-constant.js` |
| Baru | `src/lib/services/health-services/pharmacy-management/logistics-report.service.js` |
| Baru | `src/lib/hooks/health-services/pharmacy-management/use-logistics-report.jsx` |
| Baru | `src/utils/health-services/pharmacy-management/logistics-report-utils.js` |
| Baru | `src/style/health-services/pharmacy-management/logistics-report.module.css` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/{inventory-report-view, mutation-report-view, distribution-report-view, usage-by-unit-report-view, stock-flow-dashboard-view}.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/components/{report-feedback, stock-flow-daily-chart}.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/{inventory, mutations, distribution, usage-by-unit, flow-summary}/{page.jsx, *-client.jsx}` |
| Baru | `tests/unit/logistics-reports.test.mjs` |
| Ubah | `src/utils/menu-sidebar/menu-items.jsx` (5 `pathname`) |
| Ubah | 7 view pada bagian 5 (`drug-return-list-view`, `drug-stock-card-view`, `drug-stock-view`, `stock-request-emergency-review-view`, `stock-request-history-log-view`, `stock-request-list-view`, `stock-transfer-list-view`) |

Berkas tak terlacak `tests/e2e/zz-*.mjs` dan `test-results/.last-run.json` di frontend **bukan** bagian dari pekerjaan ini dan tidak disentuh.

## 7. Uji yang disiapkan dan hasilnya

**Backend.** Uji ada di `LogisticsReportTests.cs`, memakai `StockRequestApprovalHarness` dan data sintetis di SQLite dalam memori:

| Uji | Yang dibuktikan |
| --- | --- |
| `Laporan_persediaan_memisahkan_saldo_per_status_untuk_satu_obat_dan_lokasi` | Pemisahan per status, ditahan, dapat dikeluarkan, total, jumlah batch, kedaluwarsa terdekat hanya dari batch siap pakai, `includeEmpty`, pencarian |
| `Laporan_mutasi_merekap_masuk_keluar_dan_penyesuaian_tanpa_perubahan_status` | Rekap masuk, keluar, penyesuaian, dan saldo pembuka; perubahan status tidak dijumlah; mutasi di luar periode bawaan tidak ikut; saldo saat ini |
| `Periode_laporan_yang_tidak_sah_ditolak_dengan_PHM035` | Mulai > selesai dan rentang > 366 hari ditolak dengan `PHM035`; controller menjawab 422 |
| `Dashboard_menghitung_transaksi_per_sumber_dan_hari_tanpa_mengarang_penerimaan_pengadaan` | Hitungan per arah dan sumber, satu baris per hari, 10 obat teratas, `hasProcurementReceipts=false` beserta keterbatasannya |
| `Laporan_distribusi_merekap_penyerahan_per_unit_dan_obat_termasuk_kekurangan` | Hanya permintaan Selesai; kekurangan 10 − 6 = 4; cakupan `MyAuthority` sama dengan daftar permintaan (atasan unit lain 0, atasan unit 1) |
| `Laporan_pemakaian_per_unit_hanya_menghitung_pemakaian_yang_sudah_dicatat` | Hanya NotBilled/Billed (Draf dan Batal tidak dihitung); pengelompokan per unit kunjungan; saringan unit |

Status uji backend:

- **BELUM DIJALANKAN** dan belum dikompilasi, karena `dotnet build`/`test` dilarang pada tahap ini.
- Kurung `()`, `{}`, `[]` seimbang di sembilan berkas C# yang berubah (pemeriksa `paren-drift`).
- Nama member, namespace, dan kolom wajib model sudah dicocokkan dengan source: harness, `Perintah`, `RegPatientEncounter`, `PhmDrugUsage`, `PhmDrugStockMutation`, `PagedResult`.
- Pola `cond ? [] : await …` dan `?? []` sudah dipakai di repo, sehingga tidak menjadi risiko kompilasi baru.

Perintah yang perlu dijalankan sebelum commit. Hasil yang diharapkan 141 uji: 135 lama + 6 baru.

```bash
# Backend: build satu proses, foreground; uji SQLite dalam memori.
dotnet build "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj" -p:SkipMigrationMetadata=true
dotnet test  "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj" --no-build
```

**Frontend.** Tanpa build dan tanpa dev server:

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs` | **53/53 lulus** (40 lama + 13 baru) |
| `npx eslint` pada seluruh berkas baru dan yang diubah | **0 error**, 1 warning lama (`stock-request-resource-select.jsx`, `preserve-manual-memoization`, tidak diubah) |
| Uji manual | **MANUAL TEST: NOT FEASIBLE.** Dev server dan build dilarang, dan backend baru belum dikompilasi. Saringan, reset, gabungan saringan, halaman, grafik, dan layar 403 perlu dicoba saat UAT terbatas. |

Isi 13 uji frontend:

- lima entri menu tertaut dan halamannya ada; jumlah entri tetap 67; yang tertaut 32;
- empat entri tanpa sumber data tetap tanpa tautan;
- lima URL endpoint benar; service hanya `GET`;
- setiap layar memakai service-nya sendiri, `AccessDeniedGate`, base component, dan keterbatasan dari backend, tanpa data buatan;
- saringan per laporan sesuai relevansinya;
- hook tidak mengirim periode tidak sah dan membatalkan permintaan lama;
- aturan periode `PHM035` sama dengan backend (366 sah, 367 ditolak);
- tanggal `DateOnly` tidak bergeser sehari;
- jumlah bertanda, sisa kedaluwarsa, label arah, dan label sumber (yang tidak dikenal ditampilkan apa adanya);
- normalisasi tidak mengarang angka (ringkasan kosong tetap `null`);
- 403 dikenali sebagai akses ditolak;
- palet grafik dan satu sumbu;
- regresi bagian 5.

## 8. Keamanan akses

- Izin memakai resource yang sudah ada:

  | Laporan | Izin |
  | --- | --- |
  | Persediaan, Mutasi, Dashboard | `DrugStock:Read` |
  | Distribusi | `StockRequest:Read` |
  | Pemakaian per Unit | `DrugUsage:Read` |

  Tidak ada aksi izin baru dan tidak ada perubahan kebijakan akses global.
- Pengguna Logistik yang tidak memegang `DrugUsage:Read` akan melihat layar "Akses Ditolak" di Laporan Pemakaian per Unit. Pemberian izinnya adalah konfigurasi lewat `/administrator/settings/role-access`, bukan perubahan kode.
- Respons hanya berisi kode/nama obat, nama lokasi/unit, jumlah, dan waktu. Tidak ada identitas pasien, nomor kunjungan, maupun nama petugas.
- Masukan dibatasi:
  - `search` paling panjang 100 karakter;
  - `scope` divalidasi `[EnumDataType]`, dan nilai tak dikenal menghasilkan daftar kosong;
  - halaman dibatasi 200 baris;
  - periode dibatasi 366 hari.
- Teks dari backend dibersihkan dengan `sanitizeText` sebelum tampil. Tautan internal memakai `next/link`.

## 9. FIN-LGS-025 — Spesifikasi perubahan MasterData: `MstServiceUnit.OrganizationUnitId`

**Status: BLOCKED_OWNER.** Ini dependensi lintas modul yang harus dikerjakan pemilik MasterData. Logistik tidak mengubah source MasterData.

**Mengapa dibutuhkan.** Persetujuan permintaan barang menentukan atasan unit melalui rantai berikut:

1. unit layanan;
2. unit organisasi;
3. penempatan jabatan manajerial di unit organisasi itu (`StockRequestApprovalAuthority.CheckUnitAuthorityAsync`).

Kolom `MstServiceUnit.OrganizationUnitId` (Guid?) sudah ada di model (`Areas/HealthServices/MasterData/Models/MstServiceUnit.cs:63-65`, sejak commit `f75ea039`). Namun:

- `ServiceUnitDtos.cs` tidak memuatnya;
- `ServiceUnitMasterWriteService` tidak menulisnya;
- layar `src/app/health-services/master-data/service-unit` tidak menampilkannya.

Akibatnya tautan unit 0/20 tanpa jalan pengisian (keputusan nomor 1 pada *KEPUTUSAN FINAL DAN AKTIVASI* bagian 6).

**9.1 Perubahan DTO** (`Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs`)

| DTO | Tambahan | Aturan |
| --- | --- | --- |
| `CreateServiceUnitRequest` | `public Guid? OrganizationUnitId { get; set; }` | Opsional. `null` = belum ditautkan. |
| `UpdateServiceUnitRequest` | Mewarisi `OrganizationUnitId`, ditambah `public bool UpdateOrganizationUnit { get; set; }` (bawaan `false`) | **Kompatibilitas:** klien lama yang tidak mengirim field tidak menghapus tautan yang sudah ada. Tautan hanya diubah bila `UpdateOrganizationUnit = true`. Dalam keadaan itu `OrganizationUnitId = null` berarti melepas tautan. |
| `ServiceUnitResponse` (daftar) dan `ServiceUnitDetailResponse` | `Guid? OrganizationUnitId`, `string? OrganizationUnitName`, `bool IsOrganizationUnitActive` | Nama dibaca lewat navigasi `OrganizationUnit`. `IsOrganizationUnitActive` memakai penyaring yang sama dengan 9.3, sehingga layar dapat menandai tautan ke unit yang sudah tidak berlaku. |
| `ServiceUnitDefaultFilterResponse` / query daftar | `bool? HasOrganizationUnit` | Saringan "Belum ditautkan" untuk pekerjaan pengisian. |
| `ServiceUnitSummaryResponse` | `int OrganizationUnitLinkedServiceUnit`, `int OrganizationUnitUnlinkedServiceUnit` | Progres pengisian, mis. 12/20. |
| `ServiceUnitFormFieldMetadataResponse` (`CreateFields`/`UpdateFields`) | Field `organizationUnitId` (tipe pilihan, opsional) | Mengikuti pola metadata formulir yang ada. |

**9.2 Pilihan Unit Organisasi di formulir**

- Komponen yang dipakai: pemilih ber-resource `organizationUnits`, yang sudah terdaftar di `src/lib/hooks/select/hr/hr-select-resources.js:324`. Endpoint-nya `GET /api/v1/corporate/human-resource/master-data/organization-units/options`.
- Opsi yang ditampilkan hanya unit yang aktif dan sedang berlaku.
- Label opsi berisi nama dan kode unit organisasi. Pilihan boleh dikosongkan ("Belum ditautkan").
- Pada formulir ubah, nilai awal diambil dari detail, dan layar selalu mengirim `UpdateOrganizationUnit = true`.
- **Izin.** Endpoint opsi itu memerlukan `OrganizationUnit:Read` milik HR. Ada dua pilihan bagi pemilik MasterData:

  | Pilihan | Isi | Catatan |
  | --- | --- | --- |
  | **(a)** | Peran admin MasterData diberi `OrganizationUnit:Read` | **Direkomendasikan**: tanpa kode baru, cukup konfigurasi Akses Role |
  | (b) | MasterData menyediakan `GET service-units/organization-unit-options` di bawah izin `ServiceUnit:Read` | Hanya membaca id, kode, dan nama unit aktif |

**9.3 Validasi referensi** (di `ValidateRequestAsync`, dipanggil saat Create, dan saat Update bila `UpdateOrganizationUnit = true`)

- Bila `OrganizationUnitId` terisi, unit organisasi harus:
  - ada;
  - `IsActive`;
  - `!IsDelete`;
  - `EffectiveStartDate` kosong atau ≤ sekarang;
  - `EffectiveEndDate` kosong atau ≥ sekarang.

  Penyaring ini **sama persis** dengan `StockRequestApprovalAuthority.cs:78-85`, sehingga tautan yang diterima MasterData pasti dianggap sah oleh persetujuan.
- Bila tidak memenuhi, kembalikan `(false, "Unit organisasi tidak ditemukan, tidak aktif, atau sudah tidak berlaku.")`, mengikuti pola galat validasi MasterData yang ada.
- Banyak unit layanan boleh menunjuk satu unit organisasi; tidak ada keunikan.
- `Guid.Empty` diperlakukan sebagai tidak sah, bukan sebagai `null`.

**9.4 Audit**

Mengubah tautan berarti mengubah siapa yang berwenang menyetujui permintaan barang. Karena itu perubahan wajib tercatat:

- minimal lewat `UpdateBy`/`UpdateDateTime` yang sudah ada;
- dianjurkan juga entri audit berisi nilai lama → baru, memakai mekanisme audit MasterData yang ada (bila tersedia).

Uji backend Logistik tidak bergantung pada pilihan ini.

**9.5 Kompatibilitas data yang ada**

- Tidak perlu migrasi, karena kolom sudah ada. Semua baris saat ini bernilai `null` dan tetap sah: unit tanpa tautan tidak dapat disetujui. Perilaku ini sudah ditangani dengan penjelasan `ServiceUnitNotLinked` pada `approval-readiness`.
- Klien lama (Update tanpa `UpdateOrganizationUnit`) tidak mengubah tautan.
- Tautan yang menunjuk unit organisasi yang belakangan dinonaktifkan **tidak dihapus otomatis**. Layar menandainya lewat `IsOrganizationUnitActive = false`, dan persetujuan tetap menolaknya dengan alasan `OrganizationUnitInactive`.
- Skrip SQL sekali jalan (pilihan 1b) tetap mungkin untuk UAT mendesak. Isinya mengikuti validasi 9.3, dan perlu persetujuan terpisah.

**9.6 Acceptance criteria**

1. Membuat unit layanan dengan unit organisasi yang sah menyimpan `OrganizationUnitId`. Detail dan daftar menampilkan nama unit organisasinya.
2. Membuat atau mengubah dengan unit organisasi yang nonaktif, terhapus, belum berlaku, sudah berakhir, atau tidak ada ditolak dengan pesan 9.3. Data tidak berubah.
3. Update tanpa `UpdateOrganizationUnit` (klien lama) mempertahankan tautan yang ada.
4. Update dengan `UpdateOrganizationUnit = true` dan `OrganizationUnitId = null` melepas tautan.
5. Saringan `HasOrganizationUnit=false` hanya menampilkan unit tanpa tautan. Ringkasan menampilkan hitungan tertaut dan belum tertaut yang konsisten dengan daftar.
6. Formulir menampilkan pemilih Unit Organisasi yang hanya berisi unit aktif dan berlaku, dengan pilihan "Belum ditautkan".
7. Pengguna tanpa `ServiceUnit:Update` tidak dapat mengubah tautan (403).
8. Sesudah unit layanan ditautkan dan atasan memiliki penempatan manajerial ber-`StockRequest:Approve`, `GET stock-requests/approval-readiness` tidak lagi melaporkan `ServiceUnitNotLinked` untuk unit itu. Atasan juga dapat menyetujui permintaan unit tersebut.
9. Uji backend MasterData mencakup butir 1–5 dan 7. Uji frontend mencakup butir 6 dan pengiriman `UpdateOrganizationUnit = true`.

**PIC:** pemilik modul MasterData Pelayanan Kesehatan. **Dampak bila tertunda:** UAT penuh persetujuan (U7) tetap NO-GO. Kelima laporan di bagian ini **tidak** bergantung pada tautan tersebut.

## 10. FIN-LGS-026 — `LGS-OQ-027` dan `LGS-OQ-029` tetap pending

- Belum ada butir D1–D7 maupun W1–W3 yang disetujui, sehingga **tidak ada pembatasan baru** yang diimplementasikan.
- Laporan Distribusi memakai cakupan baca yang **sama persis** dengan daftar permintaan saat ini: semua permintaan yang boleh dibaca pemegang `StockRequest:Read`, ditambah pilihan opsional "Unit yang Saya Pimpin". Bila D1–D7 kelak disetujui, predikat cakupan baru cukup dipasang di `StockRequestService.GetDistributionReportAsync` (variabel `visible`), di tempat yang sama dengan `GetPagedAsync`. Query laporan tidak perlu diubah.
- Laporan Persediaan, Mutasi, Dashboard, dan Pemakaian per Unit membaca lintas lokasi dan unit bagi pemegang izinnya, sama dengan layar Stok Obat dan Pemakaian Obat yang sudah ada. Pembatasan per lokasi atau unit (bila diperlukan) masuk keputusan D2/D3 dan belum diputuskan.
- Keterbatasan Laporan Distribusi menyebut hal ini secara eksplisit ("pembatasan lintas unit menunggu LGS-OQ-027").

## 11. Blocker dan risiko

| No | Butir | Jenis | Tindak lanjut |
| --- | --- | --- | --- |
| 1 | Backend belum dikompilasi dan 6 uji baru belum dijalankan | Verifikasi tertunda (larangan tahap ini) | Jalankan perintah di bagian 7 sebelum commit; hasil yang diharapkan 141/141 |
| 2 | Perilaku query di PostgreSQL belum diuji; uji hanya SQLite dalam memori | Risiko | Uji asap kelima endpoint di lingkungan dev/UAT setelah deploy |
| 3 | Penerimaan pengadaan belum menambah stok | BLOCKED_DECISION `LGS-OQ-017`/`018` | Keterbatasan tampil di Mutasi dan Dashboard |
| 4 | Nilai persediaan (rupiah) | BLOCKED_DECISION `LGS-OQ-008`, `LGS-CAP-32` | Menu "Nilai Persediaan" tetap tanpa tautan |
| 5 | Saldo awal/akhir per periode | BLOCKED_FEATURE: potret saldo per tanggal belum disimpan | Mutasi menampilkan "Saldo Saat Ini" dan menyatakannya sebagai keterbatasan |
| 6 | Arti "unit" untuk unit yang memegang stok sendiri | BLOCKED_DECISION `LGS-OQ-003` | Pemakaian per Unit memakai unit layanan kunjungan |
| 7 | Volume data: penjumlahan di memori sesudah disaring basis data | Risiko kinerja, belum diukur | Batas periode 366 hari dan halaman ≤ 200. Laporan Persediaan memuat semua saldo yang cocok dengan saringan; bila lambat di produksi, pindahkan agregasi ke SQL (PostgreSQL mendukung `SUM` decimal) |
| 8 | Izin `DrugUsage:Read` untuk peran Logistik | Konfigurasi | Diberikan lewat Akses Role bila Logistik memang boleh membaca pemakaian |
| 9 | `MstServiceUnit.OrganizationUnitId` belum dapat diisi | BLOCKED_OWNER (MasterData) | Bagian 9 |
| 10 | `SortOrder` 5 dipakai dua aksi di `StockRequestController` (laporan distribusi dan Submit) | Kosmetik | `SortOrder` hanya urutan tampilan di Akses Role (`AccessMenuSeeder.cs:156`). Duplikasi serupa sudah ada sebelumnya (3, 4) |
| 11 | Pola prop salah yang sama di luar menu Logistik: `pagination={{…}}` di Gizi (`nutrition-order-list-view`, `nutrition-patient-view`), MasterData `fee-role`, IGD `emergency-locked-notes-tab`, Farmasi `drug-usage-list-view`; `searchValue` di sekitar 12 view lain | Temuan untuk pemilik masing-masing modul | Tidak diubah (di luar scope Logistik) |

## 12. Langkah berikutnya

1. Pemrakarsa mengizinkan build dan uji terisolasi, lalu menjalankan bagian 7 (141 uji backend).
2. Pemrakarsa mengizinkan commit lokal. Backend lebih dulu, frontend kemudian, mengikuti pola commit sebelumnya.
3. Sesudah merge dan deploy, lakukan UAT terbatas untuk kelima laporan:
   - saringan, reset, gabungan saringan, halaman;
   - periode tidak sah;
   - layar 403 untuk pengguna tanpa izin;
   - kecocokan angka dengan Kartu Stok dan Detail Permintaan pada data uji.
4. Pemilik MasterData mengerjakan bagian 9. HRD/PIC mengisi tautan dan penempatan. Sesudah itu U7.
5. Keputusan `LGS-OQ-027`/`029` (D1–D7, W1–W3) tetap ditunggu sebelum pembatasan cakupan apa pun dibuat.

---

# FINALISASI LIMA LAPORAN LOGISTIK (FIN-LGS-024 s.d. FIN-LGS-027)

**Tanggal:** 19 November 2026
**Jenis pemeriksaan:** statis dan terarah pada perubahan terbaru. Yang dijalankan hanya test runner Node dan ESLint frontend. Tidak ada `dotnet build`/`dotnet test`, build frontend, dev server, migrasi, perubahan basis data, commit, push, PR, maupun merge.
**Posisi Git:**

- Backend `Ikbal` di `a3b2ed08`, sejajar dengan `origin/Ikbal`.
- Frontend `Ikbalv2` di `109a40831`.
- Seluruh perubahan di bawah ini masih lokal.

## 1. Ringkasan

| Butir | Hasil |
| --- | --- |
| Kontrak lima endpoint terhadap frontend | **COCOK**: route, izin, parameter query, dan seluruh field respons yang dibaca layar |
| Validasi periode, respons, izin | **SESUAI** (bagian 2) |
| Pagination dan reset pencarian di tujuh layar Logistik | **TERPASANG** (bagian 2.6) |
| Perubahan di Gizi, IGD, MasterData, atau modul lain | **TIDAK ADA** (bagian 3) |
| Proyek uji terisolasi | Mencakup **121 metode = 141 kasus**: 135 lama + 6 baru (bagian 4, Lampiran B) |
| Backend 141 kasus | **SIAP DIUJI, BELUM DIJALANKAN**. 135 kasus lama lulus pada run 19 Nov (TRX `run5`); 6 kasus baru belum pernah dijalankan |
| Frontend | **53/53 lulus**; ESLint **0 error**, 1 warning lama |
| Temuan baru pada pemeriksaan ini | 1 cacat kecil di hook laporan, sudah diperbaiki (bagian 2.7) |

## 2. Hasil pemeriksaan source

### 2.1 Kontrak endpoint terhadap frontend

Pemeriksa skrip `contract-check.cjs` membandingkan `LogisticsReportDtos.cs` dengan konstanta saringan, view, dan utilitas frontend.

| Laporan | Route backend → service frontend | Izin | Saringan frontend → DTO query | Field yang dibaca layar |
| --- | --- | --- | --- | --- |
| Persediaan | `drug-stock/reports/inventory` → `getInventoryReport` | `DrugStock:Read` | 6/6 dikenal; semua parameter backend dikirim | baris 16, ringkasan 8: semua ada di DTO |
| Mutasi | `drug-stock/reports/mutations` → `getMutationReport` | `DrugStock:Read` | 7/7 | baris 14, ringkasan 8 |
| Distribusi | `stock-requests/reports/distribution` → `getDistributionReport` | `StockRequest:Read` | 9/9 (termasuk `scope`) | baris 13, ringkasan 5 |
| Pemakaian per Unit | `drug-usages/reports/by-unit` → `getUsageByUnitReport` | `DrugUsage:Read` | 8/8 | baris 11, ringkasan 4 |
| Dashboard | `drug-stock/reports/flow-summary` → `getStockFlowSummary` | `DrugStock:Read` | 4/4 | totals 7; `bySource` 4; obat teratas 5; `daily` 4; `period` 3: semua ada |

Pendukung kontrak:

- JSON memakai penamaan camelCase bawaan ASP.NET. `Program.cs:259-263` hanya menambah konverter `TimeSpan` dan tidak mengubah kebijakan nama.
- `DateOnly` diserialisasi sebagai `yyyy-MM-dd` dan dibaca layar tanpa pergeseran zona waktu.
- Ketiga controller memakai `[Authorize]`.

### 2.2 Validasi periode

- Aturan backend (`LogisticsReportPeriod.Resolve`) dan frontend (`validateReportPeriod`) **sama**:
  - kosong = 30 hari terakhir;
  - mulai > selesai ditolak;
  - rentang lebih dari 366 hari ditolak dengan `PHM035` (422).
- Pemilih tanggal frontend juga memasang `min`/`max`, sehingga tanggal mulai tidak dapat dipilih sesudah tanggal selesai.
- **Perilaku yang perlu diketahui:** bila hanya tanggal mulai diisi dan jaraknya ke hari ini lebih dari 366 hari, frontend tetap mengirim permintaan. Backend menjawab 422 dengan pesan yang jelas, dan pesannya tampil di panel galat. Backend tetap menjadi penentu karena hari ini menurut server dan menurut peramban bisa berbeda.
- **PostgreSQL:** kolom waktu (`OccurredAt`, `UsedAt`) bertipe `timestamp with time zone`. Batas periode dikirim sebagai UTC (`ToUniversalTime()`), sama dengan saringan tanggal yang sudah ada di `StockRequestService.cs:251/259` dan `DrugStockService.cs:480`. Karena itu tidak ada risiko penolakan parameter bertipe `Local` oleh Npgsql.

### 2.3 Respons dan angka

- Semua angka berasal dari tabel yang tersimpan: saldo, mutasi, riwayat permintaan, item permintaan, pemakaian, dan kunjungan.
- Tidak ada nilai rupiah dan tidak ada saldo awal/akhir historis yang dikarang. Keduanya dinyatakan di `limitations`.
- Ringkasan berupa hitungan; jumlah barang tidak dijumlahkan lintas obat.
- Respons tidak memuat identitas pasien, nomor kunjungan, maupun nama petugas.

### 2.4 Kecocokan member dengan model (pengganti kompilasi)

Setiap properti, navigasi, dan `DbSet` yang dipakai `LogisticsReportQueries` dan uji barunya ditemukan di source, termasuk tipenya:

| Model | Member yang diperiksa |
| --- | --- |
| `PhmDrugStockBalance` / `PhmDrugStockMutation` | `QuantityOnHand`, `QuantityReserved`, `DrugBatchId`, `StorageLocationId`, `MutationType`, `QuantityChange`, `OccurredAt`, `SourceDocumentType` |
| `PhmDrugBatch` | `ExpiryDate` (`DateOnly?`) |
| `PhmStockRequest*` | `RequestingServiceUnit`, `FulfilledQuantity` (`decimal?`), `MeasurementNameSnapshot` (`string?`), `PhmStockRequestHistory : IdentityModel` (`OccurredAt`, `Action`) |
| `MstDrug` | `StockUnitMeasurementId` |
| `RegPatientEncounter` | `ServiceUnitId` (`Guid`, bukan nullable), `EncounterNumber`, `PatientId` |
| `ApplicationDbContext` | `RegPatientEncounters`, `PhmDrugUsages`, `PhmDrugUsageItems`, `MstDrugStorageLocations`, `PhmStockRequestHistories` |

Hal lain yang sudah dipastikan:

- Field `_dbContext` ada di ketiga service.
- Helper 422 berada di `internal static class DrugStockControllerResults` dalam assembly yang sama dengan ketiga controller.
- Kurung seimbang di sembilan berkas C#.

### 2.5 Pengaruh terhadap 135 uji lama

- Tidak ada uji di proyek terisolasi maupun di `PharmacyTests` yang memeriksa atribut controller lewat reflection, sehingga endpoint baru tidak mengubah asersi lama.
- Perubahan pada `StockRequestService`, `DrugStockService`, `DrugUsageService`, dan ketiga controller hanya **menambah** metode.
- Berkas uji lama tidak diubah.

### 2.6 Pagination dan reset pencarian di tujuh layar Logistik

| Layar | `pagination={{` | `onPageChange={…}` | `searchValue={…}` | `searchKeyword={…}` |
| --- | --- | --- | --- | --- |
| `drug-return-list-view.jsx` | 0 | 1 | 0 | 1 |
| `drug-stock-card-view.jsx` | 0 | 1 | 0 | 0 (layar ini tidak punya kotak cari; `searchValue=""` literal yang ada tidak berpengaruh) |
| `drug-stock-view.jsx` | 0 | 1 | 0 | 1 |
| `stock-request-emergency-review-view.jsx` | 0 | 1 | 0 | 1 |
| `stock-request-history-log-view.jsx` | 0 | 1 | 0 | 1 |
| `stock-request-list-view.jsx` | 0 | 1 | 0 | 1 |
| `stock-transfer-list-view.jsx` | 0 | 1 | 0 | 1 |

Diff ketujuh berkas hanya berisi penggantian prop. Uji regresinya ada di `tests/unit/logistics-reports.test.mjs`.

### 2.7 Cacat yang ditemukan dan diperbaiki pada pemeriksaan ini

**Lokasi:** `use-logistics-report.jsx`.

**Gejala:** galat dari permintaan sebelumnya masih dapat tampil bersamaan dengan peringatan "Periode laporan tidak sah". Contohnya: permintaan gagal karena server, lalu pengguna memilih periode lebih dari 366 hari.

**Perbaikan:** galat kini hanya ditampilkan bila milik permintaan yang sedang berlaku (`error: result.key === requestKey ? result.error : ""`). Uji hook yang sudah ada diperluas dengan dua asersi, tanpa menambah jumlah uji.

## 3. Batas perubahan per modul

Seluruh perubahan berada di area Logistik/Farmasi (`pharmacy-management`), uji Logistik, dan dokumen Logistik:

- **Backend:** 6 berkas `Areas/HealthServices/PharmacyManagement/**` diubah, 2 berkas baru di area yang sama, 1 berkas uji baru, dan 1 dokumen Logistik.
- **Frontend:** 7 view `pharmacy-management`, `menu-items.jsx`, 22 berkas baru di bawah `pharmacy-management`/`logistics-report*`, dan 1 uji unit.
- **Tidak ada perubahan** di Gizi (`nutrition-management`), IGD (`emergency-installation-management`), MasterData (`master-data`), Finance, Kiosk, maupun modul lain.
- **`menu-items.jsx`:** tepat 5 baris `pathname` ditambahkan, semuanya di dalam bagian Logistik (baris 2812, 3197, 3208, 3214, 3220; bagian Logistik membentang ±2780–3240). Label dan jumlah entri tidak berubah.

**Berkas di working tree yang BUKAN bagian pekerjaan ini** (jangan ikut di-commit):

| Repo | Berkas |
| --- | --- |
| Backend | `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/qrcode.png` |
| Frontend | `tests/e2e/zz-*.spec.mjs`, `tests/e2e/zz-helper*.mjs`, `test-results/.last-run.json` |

## 4. Proyek uji terisolasi dan 141 kasus

`%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj` meng-compile berkas berikut:

- `Infrastructure/TestDatabase.cs`;
- `StockRequestApprovalHarness.cs`;
- `CatatanLogUji.cs`;
- glob `StockRequestApprovalTests\*.cs`;
- glob `LogisticsReportTests\*.cs` (baru).

Hitungan dari source memakai skrip `case-list.cjs`: `[Fact]` = 1 kasus, `[Theory]` = jumlah `[InlineData]`. Tidak ada `MemberData`/`ClassData`.

| Kelas uji | Metode | Kasus | Status |
| --- | --- | --- | --- |
| ApiContractTests | 5 | 5 | Lulus (run5) |
| ApprovalScopeTests | 15 | 17 | Lulus (run5) |
| AuthorizationTests | 13 | 13 | Lulus (run5) |
| ConcurrencyTests | 6 | 6 | Lulus (run5) |
| CutoverUnitTests | 8 | 11 | Lulus (run5) |
| DecisionLifecycleTests | 5 | 5 | Lulus (run5) |
| EmergencyReviewTests | 12 | 12 | Lulus (run5) |
| HardeningTests | 7 | 7 | Lulus (run5) |
| IdempotencyReplayTests | 3 | 9 | Lulus (run5) |
| OperationalSettingsUnitTests | 6 | 14 | Lulus (run5) |
| ProgressAndHistoryTests | 6 | 6 | Lulus (run5) |
| RobustnessAndAuditTests | 12 | 12 | Lulus (run5) |
| StatusTransitionTests | 17 | 18 | Lulus (run5) |
| **LogisticsReportTests** | **6** | **6** | **Baru, belum dijalankan** |
| **Jumlah** | **121** | **141** | 135 lulus + 6 belum dijalankan |

Pencocokan dengan `TestResults/run5.trx` (19 Nov 09:55, sesudah commit `a3b2ed08`):

- 135 hasil, semuanya `Passed`;
- jumlah kasus per metode sama dengan hitungan source;
- tidak ada nama di TRX yang hilang dari source.

Daftar lengkapnya ada di **Lampiran B**.

Perintah yang dijalankan pemilik proyek. Hasil yang diharapkan `Total tests: 141, Passed: 141`.

```bash
dotnet build "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj" -p:SkipMigrationMetadata=true
dotnet test  "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj" --no-build --logger "trx;LogFileName=run6.trx"
```

Proyek `PharmacyTests` utuh tetap tidak dapat di-compile karena berkas uji Billing milik modul lain. Uji Logistik selalu dijalankan lewat proyek terisolasi di atas.

## 5. Hasil frontend

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs` | **53/53 lulus**. Tercatat sebelumnya dan dijalankan ulang sesudah perbaikan 2.7 dengan hasil sama |
| `npx eslint` pada seluruh berkas baru dan yang diubah | **0 error**, 1 warning lama: `stock-request-resource-select.jsx:38`, `react-hooks/preserve-manual-memoization`, berkas tidak diubah |
| Build frontend / dev server / uji manual | Tidak dijalankan (dilarang). **MANUAL TEST: NOT FEASIBLE**; dicoba saat UAT terbatas |

## 6. Daftar file final

**Backend:** `DevBenari/NewQuilvianSystemBackend`, branch `Ikbal`, 10 berkas di repo.

| No | Status | Berkas |
| --- | --- | --- |
| 1 | Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` |
| 2 | Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` |
| 3 | Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` |
| 4 | Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` |
| 5 | Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` |
| 6 | Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` |
| 7 | Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` |
| 8 | Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` |
| 9 | Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` |
| 10 | Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` |

Di luar repo dan tidak di-commit: proyek uji terisolasi `…\scratchpad\srq-isolated-tests\StockRequestApprovalIsolated.Tests.csproj`, yang menerima tambahan glob `LogisticsReportTests\*.cs`.

**Frontend:** `DevBenari/QuilvianSystemFrontendDev`, branch `Ikbalv2`, 31 berkas.

| No | Status | Berkas |
| --- | --- | --- |
| 1 | Baru | `src/lib/constants/health-services/pharmacy-management/logistics-report-constant.js` |
| 2 | Baru | `src/lib/services/health-services/pharmacy-management/logistics-report.service.js` |
| 3 | Baru | `src/lib/hooks/health-services/pharmacy-management/use-logistics-report.jsx` |
| 4 | Baru | `src/utils/health-services/pharmacy-management/logistics-report-utils.js` |
| 5 | Baru | `src/style/health-services/pharmacy-management/logistics-report.module.css` |
| 6–10 | Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/{inventory-report-view, mutation-report-view, distribution-report-view, usage-by-unit-report-view, stock-flow-dashboard-view}.jsx` |
| 11–12 | Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/components/{report-feedback, stock-flow-daily-chart}.jsx` |
| 13–22 | Baru | `src/app/health-services/pharmacy-management/logistics-reports/{inventory, mutations, distribution, usage-by-unit, flow-summary}/page.jsx` beserta `*-client.jsx` masing-masing (10 berkas) |
| 23 | Baru | `tests/unit/logistics-reports.test.mjs` |
| 24 | Ubah | `src/utils/menu-sidebar/menu-items.jsx` |
| 25 | Ubah | `src/components/view/health-services/pharmacy-management/drug-return-view/drug-return-list-view.jsx` |
| 26 | Ubah | `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-card-view.jsx` |
| 27 | Ubah | `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-view.jsx` |
| 28 | Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-emergency-review-view.jsx` |
| 29 | Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-history-log-view.jsx` |
| 30 | Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-list-view.jsx` |
| 31 | Ubah | `src/components/view/health-services/pharmacy-management/stock-transfer-view/stock-transfer-list-view.jsx` |

## 7. Keterbatasan dan blocker

| No | Butir | Status |
| --- | --- | --- |
| 1 | 141 kasus backend belum dijalankan. Kompilasi backend baru diwakili pemeriksaan statis 2.4 | **Wajib sebelum commit** |
| 2 | Query belum diuji di PostgreSQL (uji memakai SQLite dalam memori) | Uji asap kelima endpoint sesudah deploy dev/UAT |
| 3 | Penerimaan pengadaan belum menambah stok (`LGS-OQ-017`/`018`) | BLOCKED_DECISION, tampil sebagai keterbatasan |
| 4 | Nilai rupiah (`LGS-OQ-008`, `LGS-CAP-32`) | BLOCKED_DECISION |
| 5 | Saldo awal/akhir historis per periode | BLOCKED_FEATURE: potret saldo per tanggal belum disimpan |
| 6 | Arti "unit" untuk unit pemegang stok (`LGS-OQ-003`) | BLOCKED_DECISION |
| 7 | Kinerja pada data besar (penjumlahan di memori) belum diukur | Risiko. Dibatasi 366 hari dan ≤ 200 baris per halaman |
| 8 | `DrugUsage:Read` untuk peran Logistik | Konfigurasi Akses Role, bukan kode |
| 9 | `MstServiceUnit.OrganizationUnitId` belum dapat diisi | BLOCKED_OWNER MasterData (spesifikasi FIN-LGS-025) |
| 10 | `LGS-OQ-027`/`029` | Pending; tidak ada pembatasan baru |
| 11 | Pola prop yang sama di luar Logistik (Gizi, IGD, MasterData `fee-role`, Farmasi `drug-usage-list-view`) | Temuan untuk pemilik masing-masing modul; **sengaja tidak diubah** |
| 12 | Uji manual layar | Belum dapat dilakukan (dev server dilarang) |

## 8. Rangkuman akhir untuk commit

Hanya ada **dua commit**: satu backend, satu frontend. Laporan ini ikut commit backend, dan perbaikan pagination/pencarian ikut commit frontend. Tidak ada commit terpisah untuk keduanya.

**Prasyarat:** `dotnet test` proyek terisolasi menghasilkan 141/141 (bagian 4). Bila ada kasus gagal, perbaiki dulu dan perbarui bagian ini sebelum commit.

**Commit backend** (`Ikbal`): 10 berkas pada bagian 6. Jangan ikutkan `SeedData/ICD10/*` dan `Storage/uploads/*`.

```text
feat(logistik): lima laporan persediaan, mutasi, distribusi, pemakaian per unit, dan dashboard penerimaan-pengeluaran

- GET drug-stock/reports/{inventory,mutations,flow-summary} (DrugStock:Read),
  stock-requests/reports/distribution (StockRequest:Read), drug-usages/reports/by-unit (DrugUsage:Read)
- periode bawaan 30 hari, maksimal 366 hari; periode tidak sah 422 PHM035
- tanpa nilai rupiah dan tanpa saldo historis; keterbatasan dikirim dalam respons
- cakupan Laporan Distribusi sama dengan daftar permintaan (LGS-OQ-027 pending)
- 6 uji LogisticsReportTests; tanpa migrasi
- laporan FIN-LGS-024..027 dan spesifikasi MasterData OrganizationUnitId (FIN-LGS-025)
```

**Commit frontend** (`Ikbalv2`): 31 berkas pada bagian 6. Jangan ikutkan `tests/e2e/zz-*` dan `test-results/.last-run.json`.

```text
feat(logistik): halaman lima laporan Logistik dan perbaikan halaman/pencarian layar Logistik

- route logistics-reports/{inventory,mutations,distribution,usage-by-unit,flow-summary}
  dengan base component, saringan periode/lokasi/unit/pencarian/halaman, dan kotak keterbatasan
- lima entri menu Logistik yang sudah ada ditautkan (67 entri, 32 tertaut)
- DataTable menerima pageNumber/totalData/onPageChange sebagai prop dan DataFilter menerima
  searchKeyword pada tujuh layar Logistik, sehingga pindah halaman dan reset pencarian berfungsi
- 13 uji unit baru (53/53), ESLint 0 error
```

**Urutan:**

1. Commit backend.
2. Commit frontend.
3. Push dan PR backend lebih dulu, karena frontend membutuhkan kelima endpoint.
4. Sesudah merge dan deploy: uji asap PostgreSQL dan UAT terbatas kelima laporan (saringan, reset, gabungan saringan, halaman, periode tidak sah, layar 403, dan kecocokan angka dengan Kartu Stok).

## Lampiran B — Daftar 141 kasus backend yang siap diuji

Sumber: source `Tests/QuilvianSystemBackend.PharmacyTests/{StockRequestApprovalTests,LogisticsReportTests}`. Satu baris = satu metode; kolom Jenis menyebut jumlah kasus `Theory` (= jumlah `InlineData`). Status lama diambil dari `TestResults/run5.trx` (19 November 2026, 135/135 lulus). "BARU" = belum pernah dijalankan.

| No | Kelas | Metode | Jenis | Status |
| --- | --- | --- | --- | --- |
| 1 | ApiContractTests | `Tidak_berwenang_403_benturan_409_prasyarat_422_berhasil_200` | Fact | Lulus 1/1 (run5, run6) |
| 2 | ApiContractTests | `Verifikasi_susulan_lewat_controller_memetakan_kode_yang_sama` | Fact | Lulus 1/1 (run5, run6) |
| 3 | ApiContractTests | `Konfigurasi_tenggat_tidak_sah_tidak_membuat_endpoint_gagal_500` | Fact | Lulus 1/1 (run5, run6) |
| 4 | ApiContractTests | `Nilai_angka_status_dan_penanda_terkunci_sesuai_konstanta_frontend` | Fact | Lulus 1/1 (run5, run6) |
| 5 | ApiContractTests | `Respons_detail_memakai_nama_properti_yang_dibaca_frontend` | Fact | Lulus 1/1 (run5, run6) |
| 6 | ApprovalScopeTests | `Atasan_hanya_melihat_permintaan_unit_yang_dipimpinnya` | Fact | Lulus 1/1 (run5, run6) |
| 7 | ApprovalScopeTests | `Atasan_beberapa_unit_melihat_seluruh_unit_yang_sah` | Fact | Lulus 1/1 (run5, run6) |
| 8 | ApprovalScopeTests | `Pengguna_tanpa_kewenangan_mendapat_daftar_kosong_bukan_seluruh_data` | Theory × 3 | Lulus 3/3 (run5, run6) |
| 9 | ApprovalScopeTests | `Petugas_CITO_melihat_permintaan_CITO_lintas_unit_tetapi_bukan_permintaan_rutin` | Fact | Lulus 1/1 (run5, run6) |
| 10 | ApprovalScopeTests | `Lingkup_konsisten_dengan_saringan_status_pencarian_dan_halaman` | Fact | Lulus 1/1 (run5, run6) |
| 11 | ApprovalScopeTests | `Lingkup_bawaan_tetap_seluruh_data_demi_kompatibilitas` | Fact | Lulus 1/1 (run5, run6) |
| 12 | ApprovalScopeTests | `Cakupan_atasan_memuat_unit_yang_dipimpin_tanpa_catatan` | Fact | Lulus 1/1 (run5, run6) |
| 13 | ApprovalScopeTests | `Cakupan_kosong_disertai_penjelasan_yang_sesuai` | Fact | Lulus 1/1 (run5, run6) |
| 14 | ApprovalScopeTests | `Kesiapan_data_melaporkan_jumlah_tanpa_identitas_pegawai` | Fact | Lulus 1/1 (run5, run6) |
| 15 | ApprovalScopeTests | `Tanpa_penunjukan_jabatan_otorisasi_CITO_ditolak_walau_izin_ada` | Fact | Lulus 1/1 (run5, run6) |
| 16 | ApprovalScopeTests | `Jabatan_berizin_yang_tidak_ditunjuk_tidak_berwenang_CITO` | Fact | Lulus 1/1 (run5, run6) |
| 17 | ApprovalScopeTests | `Penunjukan_berdasarkan_kode_jabatan_berlaku` | Fact | Lulus 1/1 (run5, run6) |
| 18 | ApprovalScopeTests | `Daftar_menggolongkan_tenggat_sama_dengan_label_per_baris` | Fact | Lulus 1/1 (run5, run6) |
| 19 | ApprovalScopeTests | `Tenggat_belum_dikonfigurasi_tidak_menggolongkan_apa_pun` | Fact | Lulus 1/1 (run5, run6) |
| 20 | ApprovalScopeTests | `Tenggat_tidak_berlaku_lagi_setelah_diverifikasi_dan_tidak_memverifikasi_otomatis` | Fact | Lulus 1/1 (run5, run6) |
| 21 | AuthorizationTests | `Atasan_unit_berizin_menyetujui_dan_keputusannya_tercatat_lengkap` | Fact | Lulus 1/1 (run5, run6) |
| 22 | AuthorizationTests | `Kasus3_atasan_unit_tanpa_izin_Approve_eksplisit_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 23 | AuthorizationTests | `Staf_berizin_tanpa_penempatan_manajerial_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 24 | AuthorizationTests | `Kasus2_atasan_unit_A_tidak_dapat_memutus_permintaan_unit_B` | Fact | Lulus 1/1 (run5, run6) |
| 25 | AuthorizationTests | `Penempatan_manajerial_yang_berakhir_tidak_memberi_kewenangan` | Fact | Lulus 1/1 (run5, run6) |
| 26 | AuthorizationTests | `Profil_tenaga_kerja_nonaktif_tidak_memberi_kewenangan` | Fact | Lulus 1/1 (run5, run6) |
| 27 | AuthorizationTests | `Unit_layanan_yang_belum_ditautkan_tidak_dapat_diputus_siapa_pun` | Fact | Lulus 1/1 (run5, run6) |
| 28 | AuthorizationTests | `Kasus4_SuperAdmin_tanpa_penempatan_yang_sah_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 29 | AuthorizationTests | `Detail_menyampaikan_kewenangan_dari_server_tanpa_masukan_layar` | Fact | Lulus 1/1 (run5, run6) |
| 30 | AuthorizationTests | `Kasus6_CITO_dengan_otorisasi_eksplisit_dapat_segera_diproses` | Fact | Lulus 1/1 (run5, run6) |
| 31 | AuthorizationTests | `Kasus5_CITO_tanpa_otorisasi_eksplisit_ditolak_termasuk_SuperAdmin_dan_atasan_unit` | Fact | Lulus 1/1 (run5, run6) |
| 32 | AuthorizationTests | `Kasus1_pemohon_berwenang_penuh_tetap_tidak_dapat_memutus_permintaannya_sendiri` | Fact | Lulus 1/1 (run5, run6) |
| 33 | AuthorizationTests | `Atasan_yang_membuatkan_permintaan_tidak_dapat_menyetujuinya` | Fact | Lulus 1/1 (run5, run6) |
| 34 | ConcurrencyTests | `Dua_persetujuan_bersamaan_hanya_satu_yang_tersimpan` | Fact | Lulus 1/1 (run5, run6) |
| 35 | ConcurrencyTests | `Setuju_dan_tolak_bersamaan_hanya_satu_keputusan_yang_sah` | Fact | Lulus 1/1 (run5, run6) |
| 36 | ConcurrencyTests | `Otorisasi_CITO_dan_penolakan_bersamaan_hanya_satu_yang_sah` | Fact | Lulus 1/1 (run5, run6) |
| 37 | ConcurrencyTests | `Dua_verifikasi_susulan_bersamaan_hanya_satu_yang_tersimpan` | Fact | Lulus 1/1 (run5, run6) |
| 38 | ConcurrencyTests | `Token_versi_menolak_penyimpanan_dari_salinan_lama` | Fact | Lulus 1/1 (run5, run6) |
| 39 | ConcurrencyTests | `Versi_yang_tertinggal_ditolak_sebelum_menyentuh_data` | Fact | Lulus 1/1 (run5, run6) |
| 40 | CutoverUnitTests | `Nilai_kosong_bukan_izin_jalan_pintas` | Theory × 3 | Lulus 3/3 (run5, run6) |
| 41 | CutoverUnitTests | `Nilai_yang_tidak_dapat_dibaca_diabaikan_dan_semua_wajib_disetujui` | Theory × 2 | Lulus 2/2 (run5, run6) |
| 42 | CutoverUnitTests | `Batas_di_masa_depan_ditolak_karena_batas_dikunci_saat_aktivasi` | Fact | Lulus 1/1 (run5, run6) |
| 43 | CutoverUnitTests | `Batas_yang_sah_dan_sudah_lewat_memisahkan_permintaan_lama_dan_baru` | Fact | Lulus 1/1 (run5, run6) |
| 44 | CutoverUnitTests | `Permintaan_tanpa_waktu_pengajuan_tidak_pernah_dianggap_permintaan_lama` | Fact | Lulus 1/1 (run5, run6) |
| 45 | CutoverUnitTests | `Nilai_berzona_waktu_dinormalkan_ke_UTC` | Fact | Lulus 1/1 (run5, run6) |
| 46 | CutoverUnitTests | `Nilai_tanpa_zona_waktu_dianggap_UTC` | Fact | Lulus 1/1 (run5, run6) |
| 47 | CutoverUnitTests | `Selisih_jam_antarserver_dalam_toleransi_tetap_diterima` | Fact | Lulus 1/1 (run5, run6) |
| 48 | DecisionLifecycleTests | `Penyerahan_tidak_menimpa_waktu_dan_catatan_persetujuan` | Fact | Lulus 1/1 (run5, run6) |
| 49 | DecisionLifecycleTests | `Pembatalan_setelah_disetujui_tidak_menimpa_keputusan_atasan` | Fact | Lulus 1/1 (run5, run6) |
| 50 | DecisionLifecycleTests | `Pembatalan_sebelum_diputus_tidak_mengisi_waktu_keputusan` | Fact | Lulus 1/1 (run5, run6) |
| 51 | DecisionLifecycleTests | `Penolakan_mengisi_waktu_dan_alasan_keputusan` | Fact | Lulus 1/1 (run5, run6) |
| 52 | DecisionLifecycleTests | `Penyerahan_lewat_aturan_peralihan_tidak_mengarang_keputusan_atasan` | Fact | Lulus 1/1 (run5, run6) |
| 53 | EmergencyReviewTests | `Atasan_unit_membenarkan_otorisasi_CITO_tanpa_mengubah_status` | Fact | Lulus 1/1 (run5, run6) |
| 54 | EmergencyReviewTests | `Kasus13_verifikasi_susulan_tidak_mengubah_stok_walau_service_memegang_akses_stok` | Fact | Lulus 1/1 (run5, run6) |
| 55 | EmergencyReviewTests | `Temuan_penyimpangan_tidak_menghapus_transaksi_fisik_yang_sudah_terjadi` | Fact | Lulus 1/1 (run5, run6) |
| 56 | EmergencyReviewTests | `Temuan_penyimpangan_wajib_disertai_catatan` | Fact | Lulus 1/1 (run5, run6) |
| 57 | EmergencyReviewTests | `Hasil_verifikasi_yang_tidak_dikenal_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 58 | EmergencyReviewTests | `Kasus14_verifikator_yang_tidak_berwenang_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 59 | EmergencyReviewTests | `Atasan_unit_yang_mengotorisasi_CITO_tidak_boleh_memverifikasi_otorisasinya_sendiri` | Fact | Lulus 1/1 (run5, run6) |
| 60 | EmergencyReviewTests | `Verifikasi_ganda_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 61 | EmergencyReviewTests | `Pengulangan_kunci_yang_sama_tidak_menggandakan_verifikasi` | Fact | Lulus 1/1 (run5, run6) |
| 62 | EmergencyReviewTests | `Permintaan_yang_disetujui_atasan_biasa_tidak_memerlukan_verifikasi_susulan` | Fact | Lulus 1/1 (run5, run6) |
| 63 | EmergencyReviewTests | `Daftar_menunggu_verifikasi_hanya_memuat_CITO_terotorisasi_yang_belum_diverifikasi` | Fact | Lulus 1/1 (run5, run6) |
| 64 | EmergencyReviewTests | `Detail_menyampaikan_kewenangan_verifikasi_dan_otorisasi_awal` | Fact | Lulus 1/1 (run5, run6) |
| 65 | HardeningTests | `Kunci_idempotensi_milik_pengguna_lain_ditolak_dan_tidak_membuka_hasilnya` | Fact | Lulus 1/1 (run5, run6) |
| 66 | HardeningTests | `Kunci_pembatalan_yang_dipakai_ulang_untuk_permintaan_lain_tidak_dianggap_berhasil` | Fact | Lulus 1/1 (run5, run6) |
| 67 | HardeningTests | `Pengulangan_oleh_pelaku_yang_sama_tetap_idempoten` | Fact | Lulus 1/1 (run5, run6) |
| 68 | HardeningTests | `Pembatalan_permintaan_disetujui_tercatat_di_log_audit_dengan_pelaku_dan_status_asal` | Fact | Lulus 1/1 (run5, run6) |
| 69 | HardeningTests | `Nilai_enum_yang_tidak_dikenal_ditolak_validasi_permintaan_HTTP` | Fact | Lulus 1/1 (run5, run6) |
| 70 | HardeningTests | `Saringan_enum_tak_dikenal_pada_service_menghasilkan_daftar_kosong_bukan_tafsiran_lain` | Fact | Lulus 1/1 (run5, run6) |
| 71 | HardeningTests | `Detail_CITO_menyampaikan_alasan_pemohon_kepada_pengotorisasi` | Fact | Lulus 1/1 (run5, run6) |
| 72 | IdempotencyReplayTests | `Kunci_idempotensi_tidak_dapat_diulang_pengguna_lain` | Theory × 7 | Lulus 7/7 (run5, run6) |
| 73 | IdempotencyReplayTests | `Pembuatan_dan_pengubahan_draf_tercatat_di_log_audit_dengan_pelaku_yang_benar` | Fact | Lulus 1/1 (run5, run6) |
| 74 | IdempotencyReplayTests | `Penyerahan_mengurangi_stok_sekali_walau_diulang_atau_dikirim_dua_kali` | Fact | Lulus 1/1 (run5, run6) |
| 75 | OperationalSettingsUnitTests | `Tenggat_tanpa_nilai_tidak_dikonfigurasi_dan_tidak_ada_bawaan` | Theory × 2 | Lulus 2/2 (run5, run6) |
| 76 | OperationalSettingsUnitTests | `Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi` | Theory × 8 | Lulus 8/8 (run5, run6) |
| 77 | OperationalSettingsUnitTests | `Klasifikasi_belum_jatuh_tempo_mendekati_dan_terlambat_mengikuti_konfigurasi` | Fact | Lulus 1/1 (run5, run6) |
| 78 | OperationalSettingsUnitTests | `Tanpa_batas_peringatan_tidak_ada_keadaan_mendekati_tenggat` | Fact | Lulus 1/1 (run5, run6) |
| 79 | OperationalSettingsUnitTests | `Penunjukan_jabatan_menerima_id_dan_kode_dalam_daftar_maupun_teks_berkoma` | Fact | Lulus 1/1 (run5, run6) |
| 80 | OperationalSettingsUnitTests | `Penunjukan_kosong_berarti_tidak_ada_jabatan_berwenang` | Fact | Lulus 1/1 (run5, run6) |
| 81 | ProgressAndHistoryTests | `Daftar_dan_detail_memuat_tahapan_progres_dari_riwayat` | Fact | Lulus 1/1 (run5, run6) |
| 82 | ProgressAndHistoryTests | `Saringan_pemenuhan_sebagian_draf_dan_pemohon_bekerja_di_backend` | Fact | Lulus 1/1 (run5, run6) |
| 83 | ProgressAndHistoryTests | `Linimasa_memuat_setiap_perubahan_terbaru_lebih_dahulu_dengan_nama_petugas` | Fact | Lulus 1/1 (run5, run6) |
| 84 | ProgressAndHistoryTests | `Linimasa_tidak_membuka_permintaan_di_luar_yang_terlihat_pada_daftar` | Fact | Lulus 1/1 (run5, run6) |
| 85 | ProgressAndHistoryTests | `Linimasa_menyaring_status_unit_pemohon_tanggal_dan_halaman` | Fact | Lulus 1/1 (run5, run6) |
| 86 | ProgressAndHistoryTests | `Linimasa_dengan_cakupan_mengikuti_kewenangan_dan_nilai_asing_tidak_ditafsirkan` | Fact | Lulus 1/1 (run5, run6) |
| 87 | RobustnessAndAuditTests | `Unit_organisasi_nonaktif_mencabut_kewenangan_atasan_dan_cakupannya` | Fact | Lulus 1/1 (run5, run6) |
| 88 | RobustnessAndAuditTests | `Unit_organisasi_yang_masa_berlakunya_habis_tidak_memberi_kewenangan` | Fact | Lulus 1/1 (run5, run6) |
| 89 | RobustnessAndAuditTests | `Akun_atasan_yang_dinonaktifkan_tidak_lagi_berwenang` | Fact | Lulus 1/1 (run5, run6) |
| 90 | RobustnessAndAuditTests | `Jabatan_CITO_yang_dinonaktifkan_tidak_lagi_memberi_kewenangan` | Fact | Lulus 1/1 (run5, run6) |
| 91 | RobustnessAndAuditTests | `Permintaan_yang_tidak_ada_ditolak_sebagai_tidak_ditemukan` | Fact | Lulus 1/1 (run5, run6) |
| 92 | RobustnessAndAuditTests | `Draf_CITO_unit_lain_tidak_terlihat_oleh_petugas_CITO` | Fact | Lulus 1/1 (run5, run6) |
| 93 | RobustnessAndAuditTests | `Kesiapan_mencocokkan_atasan_unit_dengan_izin_persetujuannya` | Fact | Lulus 1/1 (run5, run6) |
| 94 | RobustnessAndAuditTests | `Kesiapan_tidak_menyatakan_siap_bila_atasan_tidak_memegang_izin` | Fact | Lulus 1/1 (run5, run6) |
| 95 | RobustnessAndAuditTests | `Izin_pada_controller_nonaktif_tidak_berlaku_dan_tidak_dihitung_kesiapan` | Fact | Lulus 1/1 (run5, run6) |
| 96 | RobustnessAndAuditTests | `Keputusan_dan_penolakan_kewenangan_tercatat_di_log_audit` | Fact | Lulus 1/1 (run5, run6) |
| 97 | RobustnessAndAuditTests | `Pengajuan_dan_penyerahan_mencatat_pelaku_bukan_id_permintaan` | Fact | Lulus 1/1 (run5, run6) |
| 98 | RobustnessAndAuditTests | `Temuan_penyimpangan_tercatat_sebagai_audit_dan_peringatan` | Fact | Lulus 1/1 (run5, run6) |
| 99 | StatusTransitionTests | `Kasus7_alasan_penolakan_wajib` | Fact | Lulus 1/1 (run5, run6) |
| 100 | StatusTransitionTests | `Kasus8_alasan_CITO_wajib_saat_pengajuan_dan_saat_otorisasi` | Fact | Lulus 1/1 (run5, run6) |
| 101 | StatusTransitionTests | `Jalur_CITO_tidak_dapat_dipakai_untuk_permintaan_rutin` | Fact | Lulus 1/1 (run5, run6) |
| 102 | StatusTransitionTests | `Kasus9_keputusan_kedua_ditolak_409_dan_hanya_ada_satu_keputusan` | Fact | Lulus 1/1 (run5, run6) |
| 103 | StatusTransitionTests | `Pengulangan_dengan_kunci_sama_tidak_menggandakan_keputusan` | Fact | Lulus 1/1 (run5, run6) |
| 104 | StatusTransitionTests | `Kunci_sama_dengan_isi_berbeda_ditolak` | Fact | Lulus 1/1 (run5, run6) |
| 105 | StatusTransitionTests | `Kasus12_permintaan_baru_wajib_disetujui_sebelum_diserahkan` | Fact | Lulus 1/1 (run5, run6) |
| 106 | StatusTransitionTests | `Disetujui_lalu_diserahkan_menjadi_selesai_dan_penandanya_konsisten` | Fact | Lulus 1/1 (run5, run6) |
| 107 | StatusTransitionTests | `Persetujuan_tidak_mengubah_stok` | Fact | Lulus 1/1 (run5, run6) |
| 108 | StatusTransitionTests | `Ditolak_bersifat_final` | Fact | Lulus 1/1 (run5, run6) |
| 109 | StatusTransitionTests | `Draft_tidak_dapat_diputus_sebelum_diajukan` | Fact | Lulus 1/1 (run5, run6) |
| 110 | StatusTransitionTests | `Kasus11_permintaan_sebelum_batas_mengikuti_aturan_peralihan` | Fact | Lulus 1/1 (run5, run6) |
| 111 | StatusTransitionTests | `Kasus12_permintaan_sesudah_batas_wajib_disetujui` | Fact | Lulus 1/1 (run5, run6) |
| 112 | StatusTransitionTests | `Batas_kosong_atau_tidak_sah_tidak_membuka_jalan_pintas` | Theory × 2 | Lulus 2/2 (run5, run6) |
| 113 | StatusTransitionTests | `Batas_di_masa_depan_tidak_membuat_permintaan_hari_ini_lolos_tanpa_persetujuan` | Fact | Lulus 1/1 (run5, run6) |
| 114 | StatusTransitionTests | `Permintaan_tanpa_waktu_pengajuan_tidak_dianggap_permintaan_lama` | Fact | Lulus 1/1 (run5, run6) |
| 115 | StatusTransitionTests | `Antrean_gudang_memuat_yang_disetujui_dan_permintaan_lama_saja` | Fact | Lulus 1/1 (run5, run6) |
| 116 | LogisticsReportTests | `Laporan_persediaan_memisahkan_saldo_per_status_untuk_satu_obat_dan_lokasi` | Fact | **Lulus (run6)** |
| 117 | LogisticsReportTests | `Laporan_mutasi_merekap_masuk_keluar_dan_penyesuaian_tanpa_perubahan_status` | Fact | **Lulus (run6)** |
| 118 | LogisticsReportTests | `Periode_laporan_yang_tidak_sah_ditolak_dengan_PHM035` | Fact | **Lulus (run6)** |
| 119 | LogisticsReportTests | `Dashboard_menghitung_transaksi_per_sumber_dan_hari_tanpa_mengarang_penerimaan_pengadaan` | Fact | **Lulus (run6)** |
| 120 | LogisticsReportTests | `Laporan_distribusi_merekap_penyerahan_per_unit_dan_obat_termasuk_kekurangan` | Fact | **Lulus (run6)** |
| 121 | LogisticsReportTests | `Laporan_pemakaian_per_unit_hanya_menghitung_pemakaian_yang_sudah_dicatat` | Fact | **Lulus (run6)** |

**Jumlah:** 121 metode = 141 kasus (115 metode / 135 kasus lama + 6 metode / 6 kasus baru). **run6 (19 November 2026): 141 Passed, 0 Failed, 0 Skipped.**

---

# FINAL TEST LIMA LAPORAN LOGISTIK (FIN-LGS-028 s.d. FIN-LGS-031)

**Tanggal:** 19 November 2026
**Hasil:** **READY FOR OWNER COMMIT**
**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, perubahan basis data, build frontend, dan dev server.
**Yang tidak disentuh:** `SeedData/*`, `Storage/*`, `tests/e2e/zz-*`, dan `test-results/*`. Commit yang sudah ada (`a3b2ed08` backend, `109a40831` frontend) tetap utuh.

## 1. FIN-LGS-028 — Build backend

| Atribut | Nilai |
| --- | --- |
| Perintah | `dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1` (foreground, satu proses) |
| Proyek | Proyek uji terisolasi `%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests\`, mereferensikan `QuilvianSystemBackend.csproj` |
| Hasil | **Build succeeded: 0 error**, 599 warning |
| Waktu | 1 menit 21 detik |
| Warning dari berkas Logistik yang berubah | **0**. Jumlah 599 sama persis dengan build sebelumnya (`build5.log`); seluruhnya warning lama modul lain |
| Migrasi / startup aplikasi / koneksi DB operasional | **Tidak ada**. Build tidak menjalankan host maupun seeder; `SkipMigrationMetadata=true` |
| Perbaikan source yang diperlukan | **Tidak ada**. Source dari pemeriksaan statis langsung ter-compile |

## 2. FIN-LGS-029 — Test backend

| Atribut | Nilai |
| --- | --- |
| Perintah | `dotnet test StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run6.trx"` |
| Basis data | SQLite dalam memori (`TestDatabase`); tidak membaca `appsettings` maupun basis data operasional |
| Hasil | **141 Passed, 0 Failed, 0 Skipped** (TRX: `total=141 executed=141 passed=141 failed=0 notExecuted=0`) |
| Waktu | 2 menit 29 detik |
| Cakupan | 121 metode = 141 kasus (Lampiran B): 135 kasus lama + 6 kasus laporan baru |
| Perbaikan yang diperlukan | **Tidak ada**. Tidak ada kasus gagal |

Bukti dari `run6.trx` bahwa keenam uji laporan baru **benar-benar dieksekusi**:

| Uji | Hasil | Durasi |
| --- | --- | --- |
| `Laporan_persediaan_memisahkan_saldo_per_status_untuk_satu_obat_dan_lokasi` | Passed | 11,3 s |
| `Laporan_mutasi_merekap_masuk_keluar_dan_penyesuaian_tanpa_perubahan_status` | Passed | 8,9 s |
| `Periode_laporan_yang_tidak_sah_ditolak_dengan_PHM035` | Passed | 10,5 s |
| `Dashboard_menghitung_transaksi_per_sumber_dan_hari_tanpa_mengarang_penerimaan_pengadaan` | Passed | 12,6 s |
| `Laporan_distribusi_merekap_penyerahan_per_unit_dan_obat_termasuk_kekurangan` | Passed | 7,7 s |
| `Laporan_pemakaian_per_unit_hanya_menghitung_pemakaian_yang_sudah_dicatat` | Passed | 54,9 s (termasuk pemanasan model EF pertama di proses uji) |

Status Lampiran B diperbarui: keenam kasus berstatus **BARU** kini **Lulus (run6)**, dan 135 kasus lama juga lulus di `run6`.

## 3. FIN-LGS-030 — Finalisasi frontend

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs` | **53 pass, 0 fail, 0 skipped** (dijalankan ulang; tidak ada berkas frontend yang berubah sejak finalisasi) |
| ESLint (finalisasi) | 0 error, 1 warning lama (`stock-request-resource-select.jsx:38`, berkas tidak diubah) |
| Perbaikan pagination dan reset pencarian | Tetap terpasang di tujuh layar Logistik (bagian *FINALISASI* 2.6) |
| Perubahan modul lain | **Tidak ada**. Di luar `pharmacy-management`/`logistics-report*`, hanya `menu-items.jsx` yang berubah, dengan 5 baris `pathname` di bagian Logistik |
| Build frontend / dev server | Tidak dijalankan |

## 4. File yang berubah

Daftar final tidak berubah dari bagian *FINALISASI* 6:

- **backend 10 berkas**: 2 baru + 6 diubah di `Areas/HealthServices/PharmacyManagement`, 1 uji baru, dan laporan ini;
- **frontend 31 berkas**: 23 baru + 8 diubah.

FIN-LGS-028/029 tidak memerlukan perubahan source. Satu-satunya perubahan sesudah finalisasi adalah bagian laporan ini.

## 5. Status kesiapan integrasi

| Butir | Status |
| --- | --- |
| Backend build | **PASS** (0 error) |
| Backend 141 kasus | **PASS** (141/141) |
| Frontend 53 uji + ESLint | **PASS** (53/53, 0 error) |
| Migrasi | Tidak diperlukan |
| Commit oleh pemilik proyek | **READY FOR OWNER COMMIT**: satu commit backend lalu satu commit frontend, sesuai *FINALISASI* bagian 8 |
| PR / merge | Backend lebih dulu, karena frontend membutuhkan kelima endpoint |
| Sesudah deploy | Uji asap PostgreSQL kelima endpoint, lalu UAT terbatas (saringan, reset, halaman, periode tidak sah, layar 403, kecocokan angka dengan Kartu Stok) |
| Tetap terbuka | `LGS-OQ-017`/`018` (penerimaan pengadaan), `LGS-OQ-008` (nilai rupiah), `LGS-OQ-003` (arti unit), `LGS-OQ-027`/`029` (cakupan), `OrganizationUnitId` (pemilik MasterData), izin `DrugUsage:Read` untuk peran Logistik (Akses Role) |

---

# PENYELESAIAN LOGISTIK V2 (FIN-LGS-032 s.d. FIN-LGS-036)

**Tanggal:** 19 November 2026

**Branch:**

- Backend `Ikbal` di `a3b2ed08`, sama dengan `origin/Ikbal`.
- Frontend `Ikbalv2` di `109a40831`, sama dengan `origin/Ikbalv2`.
- Perubahan lima laporan dan bagian ini masih lokal.

**Yang tidak dilakukan:** commit, push, PR, merge, `dotnet build`/`test`, `npm run build`/`dev`, migrasi, dan perubahan basis data. Source modul lain tidak diubah, dan tidak ada data dummy. Pemeriksaan integrasi memakai `git fetch` (hanya memperbarui ref remote), `git merge-tree`, dan simulasi `git merge-file` di scratchpad.

## 1. Status task

| Task | Status | Ringkas |
| --- | --- | --- |
| FIN-LGS-032 | **SELESAI** | Source yang lulus (141/141 backend, 53/53 frontend) tidak diubah. Satu tambahan navigasi tanpa migrasi, "Pemenuhan Sebagian" (bagian 5.5), menaikkan uji frontend menjadi **54/54**. |
| FIN-LGS-033 | **SELESAI: KOMPATIBEL, TANPA KONFLIK** | Rinciannya di bagian 2. |
| FIN-LGS-034 | **SELESAI** | Skenario UAT dengan kriteria PASS/FAIL, dipisah antara yang bisa diuji sekarang dan yang terblokir (bagian 3). |
| FIN-LGS-035 | **SELESAI (dokumen + 1 perbaikan)** | Lembar keputusan OQ-027/029, kebutuhan mapping `OrganizationUnitId`, peta izin, dan pengelompokan 34 entri menu (bagian 4–5). |
| FIN-LGS-036 | **SELESAI** | Hanya bagian ini yang ditambahkan ke laporan. |

## 2. Kesiapan integrasi (FIN-LGS-033)

**Backend:** `Ikbal` ke `origin/QuilvianIntegrationBackend` (`659d4635`, PR #359, 18 Nov; belum bergerak sejak pemeriksaan sebelumnya).

| Pemeriksaan | Hasil |
| --- | --- |
| Posisi | Merge-base `33dd4eb1`. Integration +28 commit; `Ikbal` +2 commit (`6302f5fc`, `a3b2ed08`) yang **belum** masuk integration |
| `git merge-tree --write-tree HEAD origin/QuilvianIntegrationBackend` | **0 konflik** |
| Irisan berkas (317 berkas integration) dengan 2 commit `Ikbal` dan perubahan lokal | **Tidak ada** |
| Area yang diubah integration | HR (98 berkas), Payroll (56), SelfServices HR (6), dokumen, uji, 1 migrasi Payroll (`20261118035706_AddPayrollStatutoryPolicy…`) |
| `ApplicationDbContext.cs` di integration | Hanya menambah baris; tidak ada `DbSet` Logistik yang disentuh |
| `Program.cs` di integration | +26 baris; tidak menyentuh JSON, otorisasi, maupun izin |
| Model HR yang dibaca persetujuan permintaan (`MstOrganizationUnits`, `WfpOrganizationAssignments`, `MstPositions`, `MstWorkforceProfiles`, `ApplicationUserOrganizations`) | **Tidak berubah**; tidak ada properti publik HR yang dihapus |
| Migrasi integration yang menyentuh tabel `Phm*`/`MstDrug*`/`MstServiceUnit`/`RegPatientEncounter` | **Tidak ada** |

**Frontend:** `Ikbalv2` ke `origin/QuilvianIntegrationFrontend` (`cb5174327`, PR #211, 18 Nov).

| Pemeriksaan | Hasil |
| --- | --- |
| Posisi | Merge-base `0aae59c80`. Integration +12 commit; `Ikbalv2` +2 commit (`002f0bcd7`, `109a40831`) |
| `git merge-tree` | **0 konflik** |
| Irisan berkas | Hanya `src/utils/menu-sidebar/menu-items.jsx`. Hunk integration ada di baris 1458–1820, di luar bagian Logistik (2775–3237) |
| Simulasi tiga arah `menu-items.jsx` (base = merge-base, ours = working tree dinormalkan ke LF, theirs = integration) | **0 konflik**. Hasil gabungan tetap 67 entri Logistik dan 33 tertaut |
| Base component di integration | Hanya `use-select-resource.jsx` (+47 baris, 0 dihapus: `meta`, `hasLoaded`, `errorStatus`). Field yang dipakai `StockRequestResourceSelect` (`options`, `loading`, `hasMore`, `loadMore`, `setSearch`, `refetch`, `helperText`) tidak berubah |
| `DataTable`, `DataFilter`, `FilterDatePicker`, `SummaryGrid`, `InformationAlert`, `AccessDeniedGate`, `package.json` | Tidak diubah integration |

Catatan teknis: simulasi pertama tampak konflik satu berkas penuh. Penyebabnya working tree ber-CRLF (`core.autocrlf`) sedangkan objek Git ber-LF; Git menormalkan ini saat commit. Sesudah dinormalkan, konflik 0.

**Urutan integrasi:**

1. Pemilik proyek meng-commit lalu push `Ikbal` dan `Ikbalv2`.
2. PR backend `Ikbal` ke `QuilvianIntegrationBackend` dibuat lebih dulu, karena frontend membutuhkan lima endpoint laporan, endpoint `histories`, dan field progres.
3. Sesudah PR backend di-merge, buat PR frontend `Ikbalv2` ke `QuilvianIntegrationFrontend`.
4. Lakukan deploy dan uji asap (bagian 3.1).

**Daftar gabungan perubahan untuk pemilik proyek** (menggantikan *FINALISASI* bagian 6):

| Repo | Commit sudah di-push, belum di integration | Perubahan lokal untuk satu commit |
| --- | --- | --- |
| Backend | `6302f5fc`, `a3b2ed08` | **10 berkas**: 2 baru + 6 diubah di `Areas/HealthServices/PharmacyManagement`, `Tests/…/LogisticsReportTests/LogisticsReportTests.cs`, laporan ini |
| Frontend | `002f0bcd7`, `109a40831` | **35 berkas**: 31 berkas *FINALISASI* bagian 6, ditambah 4 dari bagian 5.5 di bawah |

Empat berkas frontend tambahan:

- baru `src/app/health-services/pharmacy-management/stock-requests/fulfillment/partial/page.jsx`;
- baru `src/app/health-services/pharmacy-management/stock-requests/fulfillment/partial/stock-request-partial-fulfillment-client.jsx`;
- ubah `src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx`;
- ubah `tests/unit/logistics-module-navigation.test.mjs`.

`stock-request-list-view.jsx`, `menu-items.jsx`, dan `logistics-reports.test.mjs` sudah ada di daftar 31.

**Tetap dikecualikan dari commit:** backend `SeedData/ICD10/*`, `Storage/uploads/*`; frontend `tests/e2e/zz-*`, `test-results/.last-run.json`.

## 3. Persiapan UAT (FIN-LGS-034)

**Aturan data dan lingkungan:**

- Dijalankan hanya di lingkungan uji yang disetujui (dev/UAT dengan basis data uji), sesudah merge dan deploy. **Tidak pernah** di basis data operasional.
- Data uji dibuat penguji lewat layar aplikasi di lingkungan itu: obat uji, saldo pembuka, permintaan, penyerahan, transfer, dan pemakaian pada kunjungan uji. Agent tidak membuat data dummy maupun skrip seed.
- Beri awalan nama yang mudah dikenali, mis. "UAT-LGS", supaya tidak tercampur dengan data lain dan mudah dibersihkan oleh pemilik lingkungan.
- `Security__Authorization__Enabled=true`. Tanpa itu, hasil uji akses tidak sah.
- Bukti per skenario: tangkapan layar, status dan isi respons jaringan, dan nilai pembanding (Kartu Stok / Detail Permintaan).

**Kriteria umum.**

- **PASS**: semua harapan pada baris itu terpenuhi, tidak ada galat konsol atau 5xx, angka cocok dengan pembanding, dan tidak ada nilai rupiah.
- **FAIL**: satu harapan tidak terpenuhi, ada 5xx, angka berbeda dari pembanding, atau data pengguna lain terbuka tanpa izin.
- **BLOCKED**: prasyarat pada kolom terakhir belum terpenuhi. Catat, lalu lanjutkan skenario lain.

### 3.1 Bisa diuji sekarang (sesudah merge dan deploy)

**Navigasi dan uji asap**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| N1 | Buka ke-33 entri menu Logistik yang tertaut: 27 pada U1, 5 laporan, dan Pemenuhan Sebagian | Setiap halaman terbuka, data dari API, menu aktif tersorot hanya pada entri yang dibuka | Akun dengan izin baca Logistik |
| N2 | Uji asap PostgreSQL kelima endpoint laporan lewat layar (periode bawaan) | 200; tidak ada 5xx; waktu respons wajar dan dicatat | Deploy backend |

**Laporan Persediaan**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| R1 | Buka Laporan Persediaan; bandingkan obat uji dengan Stok Obat/Kartu Stok | Siap pakai, ditahan, dapat dikeluarkan, karantina, kedaluwarsa, rusak, dan total sama dengan saldo per batch; kedaluwarsa terdekat = batch siap pakai terdekat | Saldo pembuka obat uji di ≥ 1 lokasi |
| R2 | Saringan lokasi, obat, cari, "termasuk saldo nol"; lalu reset; lalu pindah halaman | Hasil mengecil sesuai saringan. Reset mengosongkan semua saringan dan kotak cari. Halaman 2 menampilkan baris lanjutan dan "Menampilkan x sampai y dari z" benar | > 25 baris, atau page size kecil lewat API |

**Laporan Mutasi Barang**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| R3 | Lakukan saldo pembuka, transfer, dan penyerahan pada obat uji; buka Laporan Mutasi untuk hari itu | Masuk, keluar, penyesuaian, dan saldo pembuka per obat × lokasi sama dengan Kartu Stok pada periode yang sama; perubahan status tidak masuk kolom masuk/keluar | Transaksi uji |
| R4 | Pilih periode > 366 hari, lalu mulai > selesai | Peringatan kuning "Periode laporan tidak sah"; tidak ada permintaan jaringan; tidak ada galat lama yang ikut tampil | — |
| R5 | Isi hanya tanggal mulai > 366 hari lalu | Respons 422; panel galat berisi pesan periode dari backend | — |

**Laporan Distribusi Barang**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| R6 | Saring unit dan gudang | Baris unit × obat × satuan; diminta, diserahkan, dan kekurangan = Detail Permintaan; permintaan yang belum selesai tidak muncul | Penyerahan (butuh U7.2). **Selama U7 terblokir: data lama yang diajukan sebelum batas aktivasi dapat diserahkan menurut aturan peralihan** |

**Laporan Pemakaian per Unit**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| R7 | Catat pemakaian obat pada kunjungan uji; buat juga satu draf dan satu yang dibatalkan | Hanya pemakaian tercatat yang dihitung, di bawah unit layanan kunjungan; draf/batal tidak dihitung; jumlah dokumen dan kunjungan benar | Kunjungan uji + izin `DrugUsage:Record` untuk penguji |

**Dashboard Penerimaan & Pengeluaran**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| R8 | Buka sesudah R3 | Transaksi masuk/keluar = jumlah baris mutasi di Kartu Stok; grafik harian, tabel harian, dan "Transaksi per Sumber" konsisten; keterbatasan Goods Receipt tampil | R3 |

**Seluruh laporan**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| R9 | Akun tanpa `DrugStock:Read` membuka Persediaan, Mutasi, Dashboard; tanpa `StockRequest:Read` membuka Distribusi; tanpa `DrugUsage:Read` membuka Pemakaian | Layar "Akses Ditolak" (403); tidak ada data tampil | Akun uji tanpa izin |
| R10 | Periksa semua laporan | Kotak "Keterbatasan laporan" tampil dan tidak dapat ditutup; tidak ada angka rupiah; tidak ada nama pasien atau nomor kunjungan | — |

**Permintaan barang, riwayat, dan pemenuhan**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| P1 | U2.1–U2.4: buat, ubah, ajukan, batal | Seperti tabel U2 (*FINAL INTEGRATION READINESS* 3) | Master obat/unit/gudang |
| P2 | U3.1–U3.2: Riwayat Permintaan | Seperti U3; ditambah pindah halaman dan reset kotak cari berfungsi (perbaikan *IMPLEMENTASI LAPORAN* 5) | P1 |
| P3 | U4.1: Status Pemenuhan | Seperti U4.1; pindah halaman berfungsi | P1 |
| P4 | Pemenuhan Sebagian (menu Distribusi) | Hanya permintaan Selesai yang diserahkan kurang; reset tetap pada saringan "sebagian"; Detail menampilkan kekurangan | Penyerahan sebagian (lihat catatan R6) |

**Akses dan audit**

| ID | Langkah | Harapan (PASS) | Prasyarat |
| --- | --- | --- | --- |
| A1 | U5.1–U5.5: validasi API, tanpa login, replay antarpengguna, tanpa izin | Seperti U5 | Dua akun uji, alat API |
| A2 | U6.1–U6.2: audit dan riwayat pelaku | Seperti U6 | Akses log |
| A3 | Layar Stok Obat (semua tab), Kartu Stok, Retur, Transfer: pindah halaman dan reset cari | Halaman berikutnya terbuka; reset mengosongkan kotak cari dan hasil | Data > 1 halaman |

### 3.2 Terblokir

| ID | Skenario | Pemblokir | Dibuka oleh |
| --- | --- | --- | --- |
| U7.1 | Persetujuan atasan (setuju/tolak) | Tautan unit layanan → unit organisasi (0/20), penempatan manajerial, izin `StockRequest:Approve` | Bagian 4.2 (MasterData + HRD) + Akses Role |
| U7.2 | Penyerahan sesudah persetujuan; R6/P4 dengan data baru | U7.1 | U7.1 |
| U7.3 | CITO: otorisasi dan verifikasi susulan | Jabatan CITO (keputusan manajemen), izin `AuthorizeEmergency`, tenggat | *KEPUTUSAN FINAL* 3 butir 4–6 |
| R6b | Distribusi dengan cakupan "Unit yang Saya Pimpin" | Sama dengan U7.1 | U7.1 |
| — | Pembatasan cakupan unit sendiri (D1–D7, W1–W3) | Belum diputuskan | Bagian 4.1 |
| — | 33 entri menu yang belum tertaut | Fitur belum ada | Bagian 5 |

**Kesiapan UAT:**

- UAT terbatas (N, R1–R5, R7–R10, P1–P3, A1–A3): **SIAP** sesudah merge dan deploy.
- R6, P4, dan U7: **BLOCKED_DATA**.

## 4. Penyelesaian blocker (FIN-LGS-035)

### 4.1 Lembar keputusan final `LGS-OQ-027` dan `LGS-OQ-029`

Rekomendasi tidak berubah dari *KEPUTUSAN FINAL* bagian 4. Bagian ini menambahkan bentuk keputusan yang dapat ditandatangani dan kriteria uji untuk masing-masing butir. **Belum ada yang diimplementasikan.**

| ID | Keputusan yang diminta | Rekomendasi | Alternatif | Kriteria uji sesudah implementasi | Keputusan (isi pemrakarsa) |
| --- | --- | --- | --- | --- | --- |
| D1 | Definisi "unit sendiri" | Penempatan HR aktif (manajerial maupun tidak) di unit organisasi yang tertaut ke unit layanan, ditambah permintaan milik sendiri | Hanya penempatan manajerial; atau semua unit (tanpa pembatasan) | Staf unit A tidak melihat permintaan unit B di daftar, riwayat, detail, dan laporan Distribusi | ☐ Setuju ☐ Ubah: … |
| D2 | Apa yang dilihat gudang | Permintaan ke gudang yang `ServiceUnitId`-nya adalah unit pengguna; pemegang `Fulfill` melihat antrean selama masa peralihan | Pemegang `Fulfill` melihat semua gudang | Petugas gudang X tidak melihat antrean gudang Y (sesudah peralihan) | ☐ Setuju ☐ Ubah |
| D3 | Pemantauan lintas unit | Aksi baru `StockRequest:ReadAllUnits` untuk Kepala Instalasi Farmasi/Logistik dan audit | Pakai `ViewApprovalReadiness` | Pemegang `ReadAllUnits` melihat semua unit; tanpa itu dibatasi D1 | ☐ Setuju ☐ Ubah |
| D4 | SuperAdmin | Tidak otomatis lintas unit | Lintas unit otomatis | SuperAdmin tanpa penempatan dan tanpa `ReadAllUnits` mendapat daftar kosong | ☐ Setuju ☐ Ubah |
| D5 | Pemohon pindah unit | Tetap melihat permintaan yang ia buat/ajukan | Kehilangan akses | Sesudah penempatan dipindah, permintaan lama masih terlihat | ☐ Setuju ☐ Ubah |
| D6 | Jawaban di luar cakupan | `404` | `403` | Membuka detail permintaan unit lain → 404 "tidak ditemukan" | ☐ Setuju ☐ Ubah |
| D7 | Aktivasi | Saklar `PharmacyManagement:StockRequest:ReadScopeEnforced` bawaan `false`, dinyalakan per lingkungan | Langsung aktif saat deploy | Saklar `false`: perilaku sekarang; `true`: D1–D6 berlaku | ☐ Setuju ☐ Ubah |
| W1 | Sumber petugas peminta | Dari akun login (`WorkforceProfileId`); 422 bila akun tanpa profil | Tetap dipilih bebas | Kiriman `requestedByWorkforceId` milik orang lain diabaikan/ditolak | ☐ Setuju ☐ Ubah |
| W2 | Buat, ubah, ajukan | Hanya untuk unit sendiri (D1) | Bebas untuk pemegang `Create` | Membuat permintaan untuk unit lain → 403 | ☐ Setuju ☐ Ubah |
| W3 | Pembatalan | Draf/Menunggu: pemohon atau anggota unit; sesudah Disetujui: atasan unit, sebelum penyerahan | Pemegang `Cancel` mana pun | Anggota unit lain membatalkan → 403; tombol Batal mengikuti `canCancel` | ☐ Setuju ☐ Ubah |

**Ketergantungan:** D1, D2, W2, dan W3 baru dapat bekerja sesudah tautan unit (4.2) terisi. Bila disetujui sebelum tautan ada, saklar D7 tetap `false`.

**Perkiraan implementasi sesudah disetujui** (tanpa migrasi):

- backend: penyelesai cakupan, predikat di daftar, riwayat, detail, dan laporan Distribusi, empat perintah tulis, saklar, aksi izin, serta uji per peran;
- frontend: field petugas peminta jadi tampilan saja, pilihan unit dibatasi, dan tombol Batal.

### 4.2 Kebutuhan mapping `OrganizationUnitId` untuk pemilik MasterData

Spesifikasi perubahan source ada di *IMPLEMENTASI LAPORAN* bagian 9. Bagian ini menjelaskan **data** yang perlu disiapkan.

| Kolom lembar mapping | Isi | Sumber |
| --- | --- | --- |
| Kode unit layanan | `ServiceUnitCode` | `GET /api/v1/health-services/master-data/service-units/options` di lingkungan target |
| Nama unit layanan | `ServiceUnitName` | idem |
| Kode unit organisasi | `OrganizationUnitCode` (aktif dan berlaku) | `GET /api/v1/corporate/human-resource/master-data/organization-units/options` |
| Nama unit organisasi | idem | idem |
| Kepala unit (nama, jabatan) | Pegawai yang akan diberi penempatan manajerial | HRD |
| Status | Baru / Sudah ada / Tidak perlu (unit non-klinis tanpa permintaan barang) | HRD + Logistik |
| Penanggung jawab dan tanggal verifikasi | — | HRD |

Aturan pengisian:

1. Satu unit layanan menunjuk paling banyak satu unit organisasi. Satu unit organisasi boleh dipakai beberapa unit layanan.
2. Unit organisasi harus aktif, tidak terhapus, dan sedang berlaku. Aturan ini sama dengan penyaring persetujuan, `StockRequestApprovalAuthority.cs:78-85`.
3. Unit organisasi yang dipilih harus memiliki atau akan diberi penempatan manajerial yang aktif (kepala unit).
4. Lembar mapping diverifikasi HRD, lalu diisi lewat jalur (a) form MasterData (direkomendasikan) atau (b) skrip SQL sekali jalan yang disetujui tertulis.
5. Bukti selesai: `GET /api/v1/health-services/pharmacy-management/stock-requests/approval-readiness` menunjukkan `mappedServiceUnits` = jumlah unit yang perlu ditautkan, dan `isReadyForApproval = true` sesudah izin diberikan.

Lembar ini tidak diisi agent, karena sumber kebenarannya adalah data HR di lingkungan target.

### 4.3 Izin yang diperlukan per peran

Semua aksi sudah terdaftar di backend lewat `[AccessAction]`, jadi tidak ada izin baru. Pemberiannya lewat `/administrator/settings/role-access` per departemen × jabatan.

**Permintaan barang**

| Peran | Izin wajib | Izin pendukung (pemilih di formulir/saringan) | Syarat data selain izin |
| --- | --- | --- | --- |
| Pemohon unit | `StockRequest:Read`, `Create`, `Update` (juga untuk Ajukan), `Cancel` | `Drug:Read`, `ServiceUnit:Read`, `DrugStorageLocation:Read`, `Employee:Read` | Profil tenaga kerja (petugas peminta) |
| Atasan unit (persetujuan dan verifikasi susulan) | `StockRequest:Read`, `StockRequest:Approve` (juga untuk Tolak dan Verifikasi CITO) | idem | Penempatan **manajerial** aktif di unit organisasi yang tertaut ke unit layanan peminta. Persetujuan tidak membaca peran SuperAdmin |
| Petugas pengotorisasi CITO | `StockRequest:Read`, `StockRequest:AuthorizeEmergency` | idem | Jabatannya tercantum di `PharmacyManagement:StockRequest:EmergencyAuthorizerPositions` |
| Petugas gudang | `StockRequest:Read`, `StockRequest:Fulfill`, `DrugStock:Read` | idem | Saldo stok di gudang |
| Administrator kesiapan | `StockRequest:ViewApprovalReadiness` | — | Pemegangnya menunggu `LGS-OQ-028` |

**Laporan**

| Laporan | Izin wajib | Izin pendukung |
| --- | --- | --- |
| Persediaan, Mutasi, Penerimaan & Pengeluaran | `DrugStock:Read` | `Drug:Read`, `DrugStorageLocation:Read` |
| Distribusi | `StockRequest:Read` | idem + `ServiceUnit:Read` |
| Pemakaian per Unit | `DrugUsage:Read` | idem + `ServiceUnit:Read` |

**Rekomendasi peran baca laporan:** Kepala Instalasi Farmasi/Logistik dan staf administrasi Logistik diberi `DrugStock:Read` + `StockRequest:Read` + `DrugUsage:Read` beserta izin pendukung. Pemberian `DrugUsage:Read` kepada Logistik perlu dikonfirmasi pemilik data Farmasi, karena laporan itu berasal dari pemakaian pasien.

## 5. Entri menu yang belum tertaut (FIN-LGS-035)

Sebelum tahap ini ada 34 entri belum tertaut (66 dapat diklik, 32 tertaut). Satu dapat diselesaikan tanpa migrasi maupun keputusan baru (5.5). Kini **33 belum tertaut** dan **33 tertaut**. Label dan jumlah entri (67) tidak berubah.

### 5.1 Kelompok A: tanpa migrasi, menunggu keputusan bisnis atau makna (7)

| Entri (grup) | Sumber data yang sudah ada | Keputusan yang dibutuhkan | Prioritas |
| --- | --- | --- | --- |
| Riwayat Mutasi (Mutasi & Retur) | Daftar transfer stok, Kartu Stok, Laporan Mutasi | Makna entri: riwayat transfer antar lokasi, atau kartu stok? Sesudah dikonfirmasi, cukup ditautkan | **P1 (cepat)** |
| Rekomendasi Pengisian Ulang (Pengendalian) | `MstDrugStockPolicy` (`ReorderPointQuantity`, `ReorderQuantity`, `MaximumStockQuantity`, `IsAutoReorderEnabled`) + saldo + layar Hampir Habis | Rumus: `ReorderQuantity`, atau `Maximum − siap pakai`? Apakah membuat permintaan otomatis? | **P1** |
| Mutasi Saldo Persediaan (Stok & Kartu) | Buku mutasi (`PhmDrugStockMutation`, tidak pernah diubah) | Boleh tidaknya saldo awal periode dihitung dari buku mutasi (jumlah perubahan sebelum tanggal mulai), disertai uji rekonsiliasi dengan saldo saat ini. Bila disetujui, keterbatasan "saldo per periode" di Laporan Mutasi juga terjawab | **P1** |
| Stok per Unit (Stok & Kartu) | Saldo per lokasi + `MstDrugStorageLocation.ServiceUnitId` | `LGS-OQ-003` (arti unit pemegang stok) dan pengisian `ServiceUnitId` gudang | P2 |
| Stok Lambat Bergerak (Pemantauan) | Buku mutasi (keluar terakhir per obat × lokasi) | Definisi dan ambang hari "lambat bergerak", dan siapa yang menetapkan | P2 |
| Pemeriksaan Stok (Permintaan Barang Unit) | Stok Obat / Ringkasan Stok | Makna: cek ketersediaan sebelum mengajukan, atau saat persetujuan? | P2 |
| Lokasi Penyimpanan (Penyimpanan) | Master `drug-storage-locations` (sudah ditautkan dari grup Master) | Tautkan ke master yang sama (duplikat), atau tunggu fitur rak/bin? | P3 |

### 5.2 Kelompok B: butuh keputusan dan migrasi (22)

| Entri | Dependensi | Prioritas |
| --- | --- | --- |
| Penerimaan Barang: Pemeriksaan Jumlah & Kondisi, Pencatatan Batch/Lot, Tanggal Kadaluarsa, Barang Tidak Sesuai, Dokumen Penerimaan (5) | `LGS-OQ-017`/`018`: GR pengadaan menambah stok gudang, batch dan kedaluwarsa dari GR; kontrak lintas modul Finance Purchasing | **P1**: celah terbesar, stok masuk dari pengadaan belum ada |
| Stock Opname & Penyesuaian: Jadwal, Perhitungan Fisik, Selisih, Verifikasi, Persetujuan Penyesuaian, Berita Acara (6) | `LGS-OQ-009` + tabel opname | P1/P2 |
| Distribusi: Penyiapan Barang, Pengeluaran Gudang, Pengiriman ke Unit, Serah Terima (4) | Keputusan alur pick → issue → kirim → serah terima (status baru) | P2 |
| Kebutuhan Pengadaan (Pengendalian) (1) | Kontrak permintaan pembelian Finance Purchasing; bergantung pada Rekomendasi Pengisian Ulang | P2 |
| Mutasi & Retur: Barang Tidak Sesuai (1) | Alur barang tidak sesuai (bersama Penerimaan) | P2 |
| Penyimpanan: Penempatan ke Gudang/Rak, Pemisahan Barang Rusak, Pengaturan Kondisi Penyimpanan (3) | Model rak/bin dan parameter kondisi. Pemisahan barang rusak sebagian sudah dilayani perubahan status ke Rusak + layar Barang Rusak | P3 |
| Mutasi & Retur: Perpindahan Lokasi (1) | Model rak/bin (perpindahan dalam satu gudang) | P3 |
| Penanggung Jawab Gudang (Master) (1) | Model penanggung jawab, atau keputusan memakai penempatan HR pada unit gudang | P3 |

### 5.3 Kelompok C: laporan yang bergantung pada fitur lain (4)

| Entri | Dependensi | Prioritas |
| --- | --- | --- |
| Laporan Penerimaan | Penerimaan Barang (5.2), atau tampilan baca GR dengan kontrak API Finance | P1 sesudah `LGS-OQ-017` |
| Laporan Stock Opname | Stock Opname | P2 |
| Nilai Persediaan | `LGS-OQ-008` + harga per batch (migrasi) | P2 |
| Rekonsiliasi dengan Keuangan | `LGS-OQ-012` + Nilai Persediaan | P3 |

### 5.4 Ringkasan prioritas

1. **Tanpa migrasi, sesudah keputusan singkat:**
   - Riwayat Mutasi (makna);
   - Rekomendasi Pengisian Ulang (rumus);
   - Mutasi Saldo Persediaan (metode saldo awal dari buku mutasi).
2. **Fitur besar dengan migrasi:**
   - Penerimaan Barang dan Laporan Penerimaan (`LGS-OQ-017`/`018`);
   - Stock Opname (`LGS-OQ-009`).
3. **Lainnya**, sesuai tabel di atas.

### 5.5 Perbaikan yang dikerjakan: "Pemenuhan Sebagian" (tanpa migrasi, tanpa keputusan baru)

**Alasan:**

- Backend sudah punya saringan `PartiallyFulfilledOnly` (`StockRequestService.cs:236-237`), dan layar Status Pemenuhan sudah punya pilihan "Hanya yang diserahkan sebagian".
- Entri menu "Pemenuhan Sebagian" di grup Distribusi kini membuka layar yang sama dengan saringan itu aktif sejak awal.
- Tidak ada layar tiruan, endpoint baru, maupun perubahan perilaku layar lain.

| Berkas | Perubahan |
| --- | --- |
| `use-stock-request-list.jsx` | Parameter opsional `partiallyFulfilledOnly` (bawaan `false`) masuk ke saringan awal dan nilai reset. Pemanggil lain tidak berubah |
| `stock-request-list-view.jsx` | Varian `partialFulfillment` (judul, deskripsi, catatan) yang memakai kolom dan saringan Status Pemenuhan (`isFulfillment`) |
| `stock-requests/fulfillment/partial/{page.jsx, stock-request-partial-fulfillment-client.jsx}` | Route baru |
| `menu-items.jsx` | `pathname` untuk entri "Pemenuhan Sebagian" (yang sudah ada) |
| `logistics-module-navigation.test.mjs` | Uji baru: menu, varian, saringan awal, reset |
| `logistics-reports.test.mjs` | Jumlah tertaut 32 → 33 |

Sorotan menu aktif memakai kecocokan terpanjang (`left-sidebar-items-virtualized.jsx:117-133`), sehingga `/fulfillment/partial` hanya menyorot "Pemenuhan Sebagian".

**Hasil:**

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs` | **54/54 lulus** (53 + 1 baru) |
| ESLint berkas yang diubah | **0 error**. 1 warning lama: `use-stock-request-list.jsx:109` `set-state-in-effect` pada efek `load()` yang sudah ada sebelumnya, bukan baris yang diubah |
| Backend | Tidak berubah. **141/141** (`run6`) tetap berlaku |

## 6. Rangkuman akhir

**Integrasi:** kompatibel di backend maupun frontend, 0 konflik. Urutannya backend lebih dulu, lalu frontend.

**UAT:**

- UAT terbatas **SIAP** sesudah merge dan deploy.
- R6, P4, dan U7 **BLOCKED_DATA**.
- Produksi **NO-GO** sampai U7 lulus.

**Keputusan yang dibutuhkan:**

1. Jalur dan PIC mapping `OrganizationUnitId` (4.2).
2. Jabatan atasan, CITO, dan tenggat (*KEPUTUSAN FINAL* 3).
3. D1–D7 dan W1–W3 (4.1).
4. Pemegang `ViewApprovalReadiness` (`LGS-OQ-028`) dan `DrugUsage:Read` untuk Logistik (4.3).
5. Keputusan singkat untuk P1 tanpa migrasi (5.4 butir 1).
6. `LGS-OQ-017`/`018` dan `LGS-OQ-009` untuk fitur besar.

**Langkah implementasi berikutnya yang direkomendasikan:**

1. Pemilik proyek: commit, push, PR backend, lalu PR frontend.
2. Deploy, lalu uji asap N2 dan UAT terbatas.
3. Sambil menunggu data persetujuan:
   - kerjakan P1 tanpa migrasi begitu keputusan singkatnya keluar;
   - siapkan desain Penerimaan Barang bersama Finance Purchasing untuk `LGS-OQ-017`/`018`.

---

# LANJUTAN FINISHING LOGISTIK V2 (FIN-LGS-037 s.d. FIN-LGS-041)

**Tanggal:** 19 November 2026

**Branch:**

- Backend `Ikbal` di `a3b2ed08`.
- Frontend `Ikbalv2` di `109a40831`.
- Seluruh perubahan masih lokal.

**Yang tidak dilakukan:** commit, push, PR, merge, `dotnet build`/`test`, `npm run build`/`dev`, migrasi, dan perubahan basis data. Source MasterData/HRD tidak diubah, data organisasi tidak diisi, keputusan bisnis tidak dikarang, dan data dummy tidak dipakai.

**Yang dijalankan:** uji unit frontend (Node) dan ESLint.

## 1. Status task

| Task | Status | Ringkas |
| --- | --- | --- |
| FIN-LGS-037 | **SELESAI** | Fitur yang tertaut tidak diubah. Satu cacat konsistensi label diperbaiki (bagian 2). Daftar integrasi diperbarui. |
| FIN-LGS-038 | **SELESAI** | 33 entri diklasifikasikan A/B/C/D dari source (bagian 3): **A 0, B 7, C 20, D 6**. |
| FIN-LGS-039 | **SELESAI (desain + 1 implementasi)** | Desain tiga fitur prioritas (bagian 4). Bagian yang tidak bergantung keputusan, yaitu **pemeriksaan kelengkapan buku mutasi**, diimplementasikan di backend. Belum dikompilasi. |
| FIN-LGS-040 | **SELESAI** | Checklist aktivasi untuk HRD, MasterData, Admin Akses Role, operator, dan manajemen (bagian 5). `LGS-OQ-027`/`029` tetap **BLOCKED_DECISION**. |
| FIN-LGS-041 | **SELESAI** | Daftar konsolidasi di bagian 6. Hanya laporan ini yang diperbarui. |

## 2. FIN-LGS-037: fitur yang tertaut dan daftar integrasi

**Cacat yang diperbaiki: label sumber transaksi tidak konsisten.**

- **Gejala:** Kartu Stok menamai sumber "Permintaan Stok", "Penerimaan Barang", "Transfer Stok", dan "Pemakaian Obat" (`DRUG_STOCK_SOURCE_LABEL`), sedangkan Dashboard Penerimaan & Pengeluaran menamai sumber yang sama "Permintaan Unit", "Penerimaan Pengadaan", "Transfer Antar Lokasi", dan "Pemakaian Pasien". Selain itu, konstanta laporan memuat "InitialData", yang ternyata nilai milik HR, bukan jenis sumber stok.
- **Perbaikan:** `STOCK_SOURCE_DOCUMENT_LABELS` kini diambil dari `DRUG_STOCK_SOURCE_LABEL`, ditambah `OpticalOrder` ("Pesanan Optik", ada di backend tetapi belum di Kartu Stok). "InitialData" dihapus. Uji label disesuaikan dan diperketat.
- **Hasil:** uji frontend **54/54 lulus**; ESLint berkas terkait **0 masalah**.

**Daftar integrasi (gabungan, menggantikan *PENYELESAIAN LOGISTIK V2* bagian 2):**

| Repo | Commit sudah di-push, belum di integration | Perubahan lokal | Status uji |
| --- | --- | --- | --- |
| Backend | `6302f5fc`, `a3b2ed08` | **10 berkas**, tetap sama dengan sebelumnya. Isinya bertambah: pemeriksaan buku mutasi (bagian 4.3) di `LogisticsReportDtos.cs`, `LogisticsReportQueries.cs`, `DrugStockService.cs`, `DrugStockController.cs`, ditambah 2 uji di `LogisticsReportTests.cs` | 141/141 (`run6`) berlaku untuk isi sebelum tambahan ini. **Target sekarang 123 metode / 143 kasus; belum dijalankan** |
| Frontend | `002f0bcd7`, `109a40831` | **35 berkas**, tetap. `logistics-report-constant.js` dan `logistics-reports.test.mjs` sudah termasuk | **54/54 lulus** |

**Penting untuk pemilik proyek:** jalankan lagi build dan uji terisolasi sebelum commit backend. Perintahnya sama dengan *FINALISASI* bagian 4, dan hasil yang diharapkan `Total tests: 143, Passed: 143`. Pengecualian commit tidak berubah: `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*`, `test-results/.last-run.json`.

**Fungsi 33 entri yang tertaut.** Penilaian ini berdasarkan source dan uji otomatis; UAT belum dijalankan.

| Kategori | Jumlah | Entri |
| --- | --- | --- |
| Fungsional penuh dengan data dan izin yang ada | **29** | Dashboard (5), Master Barang & Gudang (5; halaman milik MasterData), Pengajuan Kebutuhan Barang, Data Permintaan, Status Pemenuhan, Riwayat Permintaan, Pemeriksaan Hasil Pengadaan (halaman Finance), Penelusuran Lokasi Barang, Stok per Gudang, Kartu Stok, Riwayat Barang Masuk/Keluar, Batch/Lot, Transfer Antar Gudang, Retur Unit, Retur ke Penyedia (halaman Finance), Stok Minimal/Maksimal (MasterData), Kadaluarsa, Barang Rusak, Laporan Persediaan, Pemakaian per Unit, Mutasi |
| Terbuka, tetapi aksi atau data utamanya menunggu data persetujuan (U7) | **4** | Validasi Atasan (setuju/tolak); Daftar Permintaan Disetujui (permintaan baru sesudah aktivasi; permintaan lama tetap tampil menurut aturan peralihan); Pemenuhan Sebagian; Laporan Distribusi (bergantung pada penyerahan) |

## 3. FIN-LGS-038: klasifikasi 33 entri yang belum tertaut

**Kelas:**

| Kelas | Arti |
| --- | --- |
| **A** | Bisa diimplementasikan dengan tabel dan API yang ada, tanpa keputusan yang tertunda |
| **B** | Tabel/API ada, tetapi butuh keputusan bisnis |
| **C** | Butuh migrasi (dan keputusan) |
| **D** | Bergantung pada fitur lain yang belum ada |

Kelas utama dipilih dari kendala terberat. **Tidak ada entri kelas A**: ketujuh entri tanpa migrasi semuanya masih menunggu keputusan, dan keputusan itu belum disetujui.

| No | Grup | Entri | Tabel/API yang ada | Keputusan yang dibutuhkan | Migrasi | Bergantung pada | Kelas | Prioritas |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Mutasi & Retur | Riwayat Mutasi | `GET drug-stock/mutations` (saring sumber, jenis, tanggal), `GET stock-transfers`, `GET drug-returns` | Makna entri (4.1) | Tidak | — | **B** | P1 |
| 2 | Pengendalian | Rekomendasi Pengisian Ulang | `GET drug-stock/low-stock`, `MstDrugStockPolicy` | Rumus dan tindakan (4.2) | Tidak | — | **B** | P1 |
| 3 | Stok & Kartu | Mutasi Saldo Persediaan | Buku mutasi + pemeriksaan kelengkapan (4.3) | Metode saldo awal (4.3) | Tidak | — | **B** | P1 |
| 4 | Stok & Kartu | Stok per Unit | Saldo per lokasi, `MstDrugStorageLocation.ServiceUnitId` | `LGS-OQ-003`; pengisian `ServiceUnitId` gudang | Tidak | — | **B** | P2 |
| 5 | Pemantauan | Stok Lambat Bergerak | Buku mutasi (keluar terakhir) | Definisi dan ambang hari | Tidak | — | **B** | P2 |
| 6 | Permintaan Barang Unit | Pemeriksaan Stok | Stok Obat / Ringkasan Stok | Makna dan tempatnya dalam alur | Tidak | — | **B** | P2 |
| 7 | Penyimpanan | Lokasi Penyimpanan | Master `drug-storage-locations` | Duplikat tautan atau tunggu rak/bin | Tidak | — | **B** | P3 |
| 8–12 | Penerimaan Barang | Pemeriksaan Jumlah & Kondisi; Pencatatan Batch/Lot; Tanggal Kadaluarsa; Barang Tidak Sesuai; Dokumen Penerimaan | GR Finance (tanpa penambahan stok) | `LGS-OQ-017`/`018` | Ya | Kontrak Finance Purchasing | **C** | P1 |
| 13–18 | Stock Opname | Jadwal; Perhitungan Fisik; Selisih Stok; Verifikasi Hasil; Persetujuan Penyesuaian; Berita Acara | Penyesuaian manual (`ManualAdjustment`) | `LGS-OQ-009` | Ya | — | **C** | P1/P2 |
| 19–22 | Distribusi | Penyiapan Barang; Pengeluaran Gudang; Pengiriman ke Unit; Serah Terima | Penyerahan permintaan (satu langkah) | Alur dan status pick → issue → kirim → serah | Ya | — | **C** | P2 |
| 23–25 | Penyimpanan | Penempatan ke Gudang/Rak; Pemisahan Barang Rusak; Pengaturan Kondisi Penyimpanan | Status Rusak + layar Barang Rusak (sebagian) | Model rak/bin, parameter kondisi | Ya | — | **C** | P3 |
| 26 | Mutasi & Retur | Perpindahan Lokasi | Transfer antar lokasi | Model rak/bin | Ya | — | **C** | P3 |
| 27 | Master | Penanggung Jawab Gudang | Penempatan HR (tidak tertaut ke gudang) | Model penanggung jawab vs penempatan HR | Ya | — | **C** | P3 |
| 28 | Pengendalian | Kebutuhan Pengadaan | — | Kontrak permintaan pembelian | Mungkin | Rekomendasi Pengisian Ulang + Finance Purchasing | **D** | P2 |
| 29 | Mutasi & Retur | Barang Tidak Sesuai | — | Alur barang tidak sesuai | Ya | Penerimaan (no. 11) | **D** | P2 |
| 30 | Laporan | Penerimaan | GR Finance | `LGS-OQ-017` | — | Penerimaan Barang (8–12) atau tampilan baca GR | **D** | P1 sesudah `LGS-OQ-017` |
| 31 | Laporan | Stock Opname | — | — | — | Stock Opname (13–18) | **D** | P2 |
| 32 | Laporan | Nilai Persediaan | — | `LGS-OQ-008` | Ya (harga per batch) | — | **D** | P2 |
| 33 | Laporan | Rekonsiliasi dengan Keuangan | — | `LGS-OQ-012` | — | Nilai Persediaan | **D** | P3 |

**Jumlah:**

- A 0;
- B 7 (no. 1–7);
- C 20 (no. 8–27);
- D 6 (no. 28–33).

## 4. FIN-LGS-039: persiapan tiga fitur prioritas

Setiap fitur memuat data yang ada, alur bisnis, keputusan yang diminta, desain backend, skema frontend, dan bagian yang sudah diimplementasikan. **Tidak ada rancangan di bawah ini yang dianggap sudah disetujui.**

### 4.1 Riwayat Mutasi

**Data dan API yang ada:**

- `GET drug-stock/mutations`: Kartu Stok. Saringan `drugId`, `drugBatchId`, `storageLocationId`, `mutationType`, `sourceDocumentType`, `sourceDocumentId`, `startDate`, `endDate`. Baris memuat jenis, sumber, jumlah, saldo sebelum/sesudah, pelaku, dan waktu.
- `GET stock-transfers` dan `GET drug-returns`: dokumen transfer dan retur, sudah tertaut sebagai "Transfer Antar Gudang" dan "Retur Unit".

**Pilihan makna (keputusan M):**

| Pilihan | Isi | Konsekuensi |
| --- | --- | --- |
| M1 | Riwayat **dokumen** mutasi: transfer dan retur yang selesai | Tumpang tindih dengan dua entri yang sudah tertaut; butuh layar gabungan dua daftar |
| M2 | Riwayat **pergerakan stok** akibat mutasi: Kartu Stok yang disaring sumber Transfer Stok dan Retur Resep | Tidak perlu backend baru; layar Kartu Stok perlu saringan "Sumber transaksi" |
| M3 | Tidak dipakai (entri disembunyikan) | Melanggar `LGS-DEC-001` (67 entri dipertahankan). Butuh keputusan struktur |

**Alur bila M2:**

1. Petugas memilih lokasi, obat, dan periode.
2. Layar menampilkan setiap perpindahan masuk/keluar akibat transfer dan retur.
3. Klik sumber membuka dokumen asalnya.

**Desain backend:**

- M2: tidak ada perubahan (saringan sudah ada).
- M1: tidak ada endpoint baru; layar menggabungkan dua daftar yang ada.

**Skema frontend M2:**

- Route `/drug-stock/card/transfer-history`, berisi `DrugStockCardView` dengan saringan awal sumber.
- Tambahan saringan "Sumber transaksi" (memakai `DRUG_STOCK_SOURCE_LABEL`) pada Kartu Stok.
- Tautan menu "Riwayat Mutasi".
- Perkiraan: ±0,5 hari termasuk uji.

**Sudah diimplementasikan:** tidak ada, karena seluruhnya bergantung pada makna M.

### 4.2 Rekomendasi Pengisian Ulang

**Data dan API yang ada:**

- `GET drug-stock/low-stock` mengembalikan kebijakan aktif dan berlaku yang saldo siap pakainya ≤ titik pesan ulang, dengan `quantityAvailable`, `reorderPointQuantity`, `minimumStockQuantity`, `safetyStockQuantity`, `maximumStockQuantity`, `isUnitMismatch`.
- `MstDrugStockPolicy` juga menyimpan `ReorderQuantity`, `LeadTimeDays`, dan `IsAutoReorderEnabled`. Ketiganya belum ada di respons low-stock.

**Alur bisnis yang diusulkan:**

1. Petugas gudang/depo membuka daftar harian.
2. Sistem menampilkan obat × lokasi di bawah titik pesan ulang beserta jumlah usulan menurut rumus yang disetujui.
3. Petugas meninjau, lalu (bila R3 disetujui) membuat Permintaan Barang ke gudang induk dari baris itu. Untuk gudang induk, usulan masuk ke Kebutuhan Pengadaan (D, no. 28).

**Keputusan yang diminta:**

| ID | Pertanyaan | Pilihan |
| --- | --- | --- |
| R1 | Rumus jumlah usulan | (a) `ReorderQuantity` kebijakan; (b) `Maximum − siap pakai`; (c) `Maximum − (siap pakai + permintaan yang sedang berjalan)` |
| R2 | Lokasi yang dihitung | Semua lokasi berkebijakan; atau hanya depo/unit (gudang induk ke Kebutuhan Pengadaan) |
| R3 | Tindakan | Hanya daftar; mengisi formulir permintaan otomatis; atau membuat draf permintaan otomatis bila `IsAutoReorderEnabled` |
| R4 | Pembulatan | Ke satuan stok, atau ke kemasan beli (konversi satuan) |
| R5 | Kebijakan dengan satuan keliru (`isUnitMismatch`) | Dikecualikan dan ditandai (usulan bawaan sistem: tidak dihitung) |

**Desain backend (tanpa migrasi):**

- Endpoint baca `GET drug-stock/reports/replenishment` (`DrugStock:Read`).
- Memakai penyaring yang sama dengan low-stock, ditambah `reorderQuantity`, `leadTimeDays`, `isAutoReorderEnabled`, `recommendedQuantity`, dan `basis` (rumus yang dipakai).
- Untuk R1(c), jumlah dalam proses dihitung dari baris permintaan berstatus Menunggu/Disetujui untuk unit lokasi itu (butuh `ServiceUnitId` gudang).

**Skema frontend:**

- Route `/drug-stock/replenishment`, tautan menu "Rekomendasi Pengisian Ulang".
- Kolom: Obat, Lokasi, Siap Pakai, Titik Pesan Ulang, Maksimum, Jumlah Pesan Ulang (kebijakan), Usulan, Dasar Perhitungan.
- Aksi "Buat Permintaan" bila R3.
- Kotak keterbatasan untuk kebijakan bersatuan keliru.

**Sudah diimplementasikan:** tidak ada. Angka usulan adalah keputusan R1; tanpa itu, angka apa pun akan menjadi rekomendasi karangan. Daftar "di bawah titik pesan ulang" sudah tersedia di layar Barang Hampir Habis.

### 4.3 Mutasi Saldo Persediaan

**Data yang ada:**

- Buku mutasi `PhmDrugStockMutation`, yang tidak pernah diubah atau dihapus. Setiap baris menyimpan `QuantityChange`, `BalanceBefore`, `BalanceAfter`, dan status per batch × lokasi.
- Di source, **satu-satunya jalur** yang mengubah `PhmDrugStockBalance.QuantityOnHand` adalah `DrugStockService` (`DrugStockService.cs:1063-1088`), dan jalur itu selalu menulis mutasi.
- `PharmacyOpeningStockSeeder` juga menulis mutasi saldo pembuka untuk setiap saldo > 0.
- Tidak ada `ExecuteUpdate`/SQL mentah ke tabel stok.

Artinya saldo **seharusnya** dapat direkonstruksi dari buku mutasi. Ini perlu dibuktikan pada data sebenarnya.

**Alur bisnis yang diusulkan:**

1. Pilih lokasi dan periode.
2. Untuk setiap obat ditampilkan Saldo Awal, Masuk, Keluar, Penyesuaian, dan Saldo Akhir.
3. Untuk periode yang berakhir hari ini, Saldo Akhir dicocokkan dengan saldo saat ini.

**Keputusan yang diminta:**

| ID | Pertanyaan | Usulan untuk diputuskan |
| --- | --- | --- |
| S1 | Metode saldo awal | Jumlah semua `QuantityChange` sebelum tanggal mulai (dari buku mutasi) |
| S2 | Status yang dihitung | Semua status fisik (siap pakai + karantina + kedaluwarsa + rusak), atau hanya siap pakai |
| S3 | Rincian | Per obat × lokasi, atau per batch |
| S4 | Lokasi yang buku mutasinya tidak cocok (hasil pemeriksaan di bawah) | Laporan ditolak untuk lokasi itu; atau tetap tampil dengan peringatan dan daftar selisih |
| S5 | Batas hari | Tanggal lokal server, sama dengan laporan lain |

**Desain backend sesudah S1–S5:**

- Perluas `GetMutationsAsync`, atau buat `GET drug-stock/reports/balance-movement`.
- Saldo awal = Σ mutasi `OccurredAt < FromUtc` per kunci S3 untuk status S2.
- Saldo akhir = saldo awal + perubahan dalam periode.
- Lokasi wajib, untuk membatasi data yang dibaca.

**Skema frontend:**

- Route `/logistics-reports/balance-movement`, tautan menu "Mutasi Saldo Persediaan".
- Kolom: Obat, Satuan, Saldo Awal, Masuk, Keluar, Penyesuaian, Saldo Akhir, Saldo Saat Ini.
- Panel "Kelengkapan buku mutasi" dari pemeriksaan di bawah.

**Sudah diimplementasikan (tidak bergantung keputusan): pemeriksaan kelengkapan buku mutasi**

| Atribut | Isi |
| --- | --- |
| Endpoint | `GET /api/v1/health-services/pharmacy-management/drug-stock/reports/ledger-consistency` |
| Izin | `DrugStock:Read` (`SortOrder` 14), sama dengan Kartu Stok |
| Query | `storageLocationId` (**wajib**; kosong dijawab 422 `PHM036`), `drugId?`, `onlyInconsistent` (bawaan `true`), `pageNumber`, `pageSize` |
| Isi | Per batch × status pada lokasi itu: `quantityOnHand` (saldo tersimpan), `ledgerQuantity` (Σ `QuantityChange`), `difference`, `mutationCount`, `lastMutationAt`, `finding` |
| Nilai `finding` | `Match` = cocok; `SumMismatch` = ada mutasi dan saldo, jumlah berbeda; `NoMutation` = saldo tidak nol tanpa mutasi; `NoBalance` = mutasi tanpa baris saldo |
| Ringkasan | `checkedKeyCount`, `matchCount`, `sumMismatchCount`, `noMutationCount`, `noBalanceCount`, `mutationCount`, `isConsistent` |
| Sifat | Hanya membaca, tidak mengoreksi saldo. Jumlah yang ditahan tidak diperiksa (tidak dicatat di buku mutasi) |
| Berkas | `LogisticsReportDtos.cs` (DTO + `LedgerConsistencyFindings`), `LogisticsReportQueries.cs` (`GetLedgerConsistencyAsync`, kode `PHM036`), `DrugStockService.cs` (delegasi), `DrugStockController.cs` (endpoint) |
| Uji | `Pemeriksaan_buku_mutasi_menandai_saldo_yang_tidak_dapat_direkonstruksi`: keempat temuan, selisih +5, saringan bawaan, dan saldo serta jumlah mutasi tidak berubah. `Pemeriksaan_buku_mutasi_mewajibkan_lokasi_dan_membaca_satu_lokasi_saja`: `PHM036`, dan isolasi antarlokasi |
| Status | Kurung seimbang. Nama member diperiksa terhadap source (`PhmDrugBatches`, `BatchNumber`, `OccurredAt`, `QuantityChange`). **Belum dikompilasi dan belum dijalankan** (larangan tahap ini) |
| Frontend | Sengaja **tidak** dibuat: belum ada entri menu yang boleh ditautkan sebelum S1–S5. Dipakai lewat alat API/Swagger oleh administrator saat UAT |

**Cara memakainya untuk keputusan S1/S4.** Saat UAT dan sebelum go-live, jalankan endpoint untuk setiap gudang aktif dan catat `isConsistent`. Bila semua gudang `true`, metode S1 aman. Bila ada temuan, putuskan S4 dan telusuri asal saldonya (impor data atau perubahan di luar aplikasi).

## 5. FIN-LGS-040: checklist aktivasi persetujuan dan CITO

Status:

- `LGS-OQ-027` (D1–D7) dan `LGS-OQ-029` (W1–W3): **BLOCKED_DECISION**. Tidak ada yang diimplementasikan, dan lembar keputusannya ada di *PENYELESAIAN LOGISTIK V2* 4.1.
- Source MasterData/HRD tidak diubah, dan data organisasi **tidak diisi otomatis**.

Bukti setiap butir dibaca dari `GET /api/v1/health-services/pharmacy-management/stock-requests/approval-readiness`, yang memerlukan izin `StockRequest:ViewApprovalReadiness`.

**HRD**

| ☐ | Butir | Bukti selesai |
| --- | --- | --- |
| ☐ | Isi lembar mapping unit layanan → unit organisasi (*PENYELESAIAN LOGISTIK V2* 4.2) untuk setiap unit layanan yang mengajukan permintaan barang | Lembar ditandatangani HRD |
| ☐ | Pastikan setiap unit organisasi pada lembar aktif dan berlaku (tanggal mulai/berakhir) | Lembar |
| ☐ | Beri **penempatan manajerial** aktif kepada kepala setiap unit organisasi, lewat editor tenaga kerja → Penempatan Organisasi → "Penempatan Manajerial" | `mappedServiceUnitsWithSupervisor`, `units[].activeSupervisors` |
| ☐ | Pastikan akun pengguna kepala unit aktif dan tertaut ke profil tenaga kerjanya | `units[].authorizedSupervisors` > 0 sesudah izin diberikan |
| ☐ | Pastikan petugas peminta punya profil tenaga kerja (dipakai kolom petugas peminta) | Pemilih "Petugas Peminta" menemukan namanya |

**Pemilik MasterData**

| ☐ | Butir | Bukti selesai |
| --- | --- | --- |
| ☐ | Putuskan jalur pengisian: (a) field "Unit Organisasi" di master unit layanan (spesifikasi *IMPLEMENTASI LAPORAN* 9), atau (b) skrip SQL sekali jalan yang disetujui tertulis | Keputusan tertulis |
| ☐ | (a) Implementasikan DTO, validasi, dan form sesuai kriteria 9.6; atau (b) tinjau dan jalankan skrip dari lembar HRD di lingkungan uji lebih dulu | Uji MasterData lulus / log skrip |
| ☐ | Isi tautan dari lembar HRD, **tanpa** pengisian otomatis atau tebakan | `mappedServiceUnits` = jumlah unit pada lembar |
| ☐ | (Opsional) isi `MstDrugStorageLocation.ServiceUnitId` gudang/depo bila D2 atau Stok per Unit disetujui | Daftar gudang |

**Admin Akses Role** (`/administrator/settings/role-access`; izin per peran ada di *PENYELESAIAN LOGISTIK V2* 4.3)

| ☐ | Butir | Bukti selesai |
| --- | --- | --- |
| ☐ | `StockRequest:Approve` untuk departemen × jabatan kepala unit | `approvePermissionGrants` > 0; **`isReadyForApproval = true`** |
| ☐ | `StockRequest:AuthorizeEmergency` untuk jabatan CITO yang ditunjuk manajemen | `emergencyDesignatedPositionsWithGrant` > 0 |
| ☐ | `StockRequest:Read/Create/Update/Cancel` untuk petugas unit; `Fulfill` + `DrugStock:Read` untuk gudang | UAT P1, U7.2 |
| ☐ | Izin pendukung pemilih: `Drug:Read`, `ServiceUnit:Read`, `DrugStorageLocation:Read`, `Employee:Read` | Pemilih di formulir dan saringan terisi |
| ☐ | Izin laporan: `DrugStock:Read`, `StockRequest:Read`, `DrugUsage:Read` (yang terakhir sesudah konfirmasi pemilik data Farmasi) | UAT R9 |
| ☐ | `StockRequest:ViewApprovalReadiness` untuk pemegang yang ditetapkan `LGS-OQ-028` | Endpoint kesiapan dapat dibuka |

**Operator/DevOps dan manajemen**

| ☐ | Butir | PIC | Bukti selesai |
| --- | --- | --- | --- |
| ☐ | Tetapkan jabatan pengotorisasi CITO | Manajemen | Keputusan tertulis |
| ☐ | Konfigurasi `PharmacyManagement__StockRequest__EmergencyAuthorizerPositions` | Operator | `emergencyDesignatedPositions` |
| ☐ | Tetapkan dan konfigurasi tenggat verifikasi (`…__EmergencyReviewDueHours`, opsional `…WarningHours`) | Manajemen + operator | Blocker "Tenggat…" hilang; **`isReadyForEmergency = true`** |
| ☐ | Konfigurasi `…__ApprovalEnforcedFromUtc` saat go-live | Operator atas arahan pemrakarsa | `approvalCutoverState` |
| ☐ | `Security__Authorization__Enabled=true` di UAT/produksi | Operator | UAT A1/R9 |

**Urutan:**

1. HRD (lembar) → MasterData (tautan) → HRD (penempatan) → Admin Akses Role (izin); sampai di sini `isReadyForApproval` bernilai `true`.
2. Manajemen dan operator untuk CITO; sampai di sini `isReadyForEmergency` bernilai `true`.
3. UAT U7.

## 6. FIN-LGS-041: konsolidasi

**Fitur selesai** (source + uji otomatis):

- 33 entri menu tertaut: 29 fungsional penuh, 4 menunggu data persetujuan (bagian 2).
- Lima laporan.
- Permintaan barang unit lengkap: buat, ubah, ajukan, batal, setuju, tolak, CITO, verifikasi susulan, serah, sebagian, riwayat, status pemenuhan.
- Perbaikan halaman dan pencarian di tujuh layar.
- Pemenuhan Sebagian.
- Pemeriksaan kelengkapan buku mutasi: backend, menunggu build dan uji.

**Fitur siap UAT** (sesudah merge dan deploy): N1–N2, R1–R5, R7–R10, P1–P3, A1–A3 (*PENYELESAIAN LOGISTIK V2* 3.1). Tambahan untuk pemeriksaan buku mutasi:

| ID | Langkah | Harapan (PASS) |
| --- | --- | --- |
| L1 | Jalankan pemeriksaan buku mutasi untuk setiap gudang uji lewat alat API | 200; `isConsistent` dan temuan dicatat sebagai bukti S1/S4; saldo tidak berubah. Tanpa lokasi: 422 `PHM036` |

**Fitur memerlukan keputusan:**

| Fitur | Keputusan |
| --- | --- |
| Persetujuan atasan, CITO (U7) | Data mapping dan izin (bagian 5), jabatan CITO, tenggat |
| Cakupan unit sendiri | `LGS-OQ-027` D1–D7, `LGS-OQ-029` W1–W3 |
| Riwayat Mutasi | M (4.1) |
| Rekomendasi Pengisian Ulang | R1–R5 (4.2) |
| Mutasi Saldo Persediaan | S1–S5 (4.3) |
| Stok per Unit | `LGS-OQ-003` |
| Stok Lambat Bergerak | Definisi dan ambang |
| Pemeriksaan Stok | Makna |
| Lokasi Penyimpanan (grup Penyimpanan) | Duplikat atau tunggu rak/bin |
| Pemegang izin | `ViewApprovalReadiness` (`LGS-OQ-028`), `DrugUsage:Read` untuk Logistik |

**Fitur membutuhkan migrasi:**

- Penerimaan Barang (5; `LGS-OQ-017`/`018`);
- Stock Opname (6; `LGS-OQ-009`);
- Distribusi bertahap (4);
- Penyimpanan rak/bin dan kondisi (3);
- Perpindahan Lokasi;
- Penanggung Jawab Gudang;
- Nilai Persediaan (harga per batch; `LGS-OQ-008`).

Turunannya: Laporan Penerimaan, Laporan Stock Opname, Rekonsiliasi dengan Keuangan (`LGS-OQ-012`), Kebutuhan Pengadaan, dan Barang Tidak Sesuai di Mutasi & Retur.

**Prioritas implementasi berikutnya:**

1. **Sekarang:**
   - build dan uji backend terisolasi (target 143/143);
   - pemilik proyek commit dan push;
   - PR backend lalu frontend;
   - deploy, lalu UAT terbatas termasuk L1.
2. **Begitu keputusan singkat keluar (tanpa migrasi):**
   - Riwayat Mutasi (M), ±0,5 hari;
   - Rekomendasi Pengisian Ulang (R1–R5), ±1–2 hari;
   - Mutasi Saldo Persediaan (S1–S5, dengan bukti L1), ±2 hari.
3. **Sesudah data persetujuan lengkap:** UAT U7, lalu go-live persetujuan.
4. **Fitur besar dengan migrasi:**
   - Penerimaan Barang bersama Finance Purchasing (`LGS-OQ-017`/`018`);
   - Stock Opname (`LGS-OQ-009`).
5. **Lainnya** mengikuti tabel bagian 3.

---

# FINALISASI LEDGER DAN KEPUTUSAN BISNIS LOGISTIK V2 (FIN-LGS-042 s.d. FIN-LGS-046)

**Tanggal:** 19 November 2026

**Branch:**

- Backend `Ikbal` di `a3b2ed08`.
- Frontend `Ikbalv2` di `109a40831`.
- Seluruh perubahan masih lokal.

**Yang tidak dilakukan:** commit, push, PR, merge, `dotnet build`/`test`, `npm run build`/`dev`, migrasi, perubahan basis data, perubahan modul lain, keputusan bisnis yang dikarang, dan data persediaan yang dikarang.

**Yang dijalankan:** uji unit frontend (Node) dan ESLint.

**Penegasan:** belum ada klaim bahwa buku mutasi cocok pada data apa pun. Hasilnya baru diketahui sesudah L1–L6 dijalankan di lingkungan uji (bagian 6.3). Target 144 kasus backend juga **belum** dijalankan.

## 1. Status task

| Task | Status | Ringkas |
| --- | --- | --- |
| FIN-LGS-042 | **SELESAI DI SOURCE, BELUM DIKOMPILASI** | Pemeriksaan buku mutasi ditinjau ulang. **2 cacat nyata** diperbaiki, 1 uji regresi baru, 1 uji diperluas |
| FIN-LGS-043 | **SELESAI (lembar keputusan)** | S1–S5 dengan rekomendasi, konsekuensi, dan kriteria uji. Rumus historis **tidak** diimplementasikan |
| FIN-LGS-044 | **SELESAI (lembar keputusan + 1 bagian netral)** | Rekomendasi pilihan B (pakai ulang Kartu Stok). Saringan umum "Sumber Transaksi" di Kartu Stok diimplementasikan; menu "Riwayat Mutasi" **tetap tanpa tautan** |
| FIN-LGS-045 | **SELESAI (lembar keputusan + desain)** | R1–R5, desain backend/frontend, dan rencana uji. Tidak ada angka usulan yang dibuat |
| FIN-LGS-046 | **SELESAI** | Status menu, verifikasi statis, skenario UAT ledger L1–L6, daftar integrasi, dan daftar periksa 144 kasus |

## 2. FIN-LGS-042: finalisasi pemeriksaan kelengkapan buku mutasi

**Endpoint:** `GET /api/v1/health-services/pharmacy-management/drug-stock/reports/ledger-consistency`

| Pemeriksaan | Hasil | Bukti di source |
| --- | --- | --- |
| Perhitungan per lokasi, obat, batch, status | **Diperbaiki.** Kunci kini obat × batch × status pada satu lokasi; sebelumnya batch × status | `LogisticsReportQueries.GetLedgerConsistencyAsync` |
| Tidak ada saldo yang dihitung dua kali | Benar. Saldo hidup unik per batch × lokasi × status lewat indeks unik `(DrugBatchId, StorageLocationId, Status) WHERE IsDelete = false` | Model snapshot, indeks `PhmDrugStockBalance` |
| Tidak ada mutasi yang dihitung dua kali | Benar. Setiap baris mutasi dijumlah tepat sekali di kuncinya sendiri. Perubahan status menulis dua baris (status asal −x, status tujuan +x), masing-masing masuk ke status miliknya. Transfer menulis keluar di lokasi asal dan masuk di lokasi tujuan. Koreksi (`CorrectionOfMutationId`) adalah baris pembalik tersendiri, jadi wajib ikut dijumlah | `DrugStockService.cs:1063-1088`, uji `…menandai_saldo…` |
| Empat keadaan dibedakan | `Match` (cocok), `SumMismatch` (saldo berbeda), `NoMutation` (saldo tanpa mutasi), `NoBalance` (mutasi tanpa saldo) | `LedgerConsistencyFindings`, uji `…menandai_saldo…` |
| Izin `DrugStock:Read` ditegakkan | `[Authorize]` pada controller dan `[AccessPermission("DrugStock", "Read")]` pada endpoint, dibuktikan uji refleksi | uji `…lewat_controller…` |
| Lokasi wajib dan `PHM036` konsisten | Kosong, `Guid.Empty`, dan lokasi tidak terdaftar semuanya menghasilkan 422 `{ code: "PHM036" }` lewat helper yang sama dengan laporan lain | uji `…lewat_controller…`, `…lokasi_terdaftar…` |
| Read-only | `AsNoTracking`, tanpa `SaveChanges`; uji membuktikan jumlah saldo dan jumlah mutasi tidak berubah | uji `…menandai_saldo…` (`PotretStokAsync`) |
| Koreksi saldo otomatis | **Tidak ada**, dan tidak dibuat | — |

**Cacat nyata yang diperbaiki:**

1. **Lokasi tidak terdaftar dilaporkan "konsisten".** Dengan `storageLocationId` acak, endpoint menjawab 200 dengan 0 baris dan `isConsistent = true`, padahal tidak ada yang diperiksa. Kini dijawab 422 `PHM036` "Lokasi penyimpanan tidak ditemukan." Lokasi nonaktif atau terhapus tetap boleh diperiksa, karena sisa saldonya justru perlu dibuktikan. Untuk lokasi terdaftar yang tidak punya saldo maupun mutasi, `checkedKeyCount = 0` dan `isConsistent = true`: tidak ada yang perlu direkonstruksi. UAT L5 memeriksa bahwa keadaan ini terbaca jelas.
2. **Obat tidak ikut dalam kunci.** Bila ada baris yang `DrugId`-nya tidak sesuai dengan batch, saldo dan mutasinya tercampur ke obat lain tanpa terlihat. Kini baris itu tampil terpisah (mis. `NoBalance` untuk obat yang keliru), dan saldo obat yang benar tidak terpengaruh.

**Uji:**

| Uji | Isi |
| --- | --- |
| `Pemeriksaan_buku_mutasi_menandai_saldo_yang_tidak_dapat_direkonstruksi` | Tetap; tidak berubah |
| `Pemeriksaan_buku_mutasi_mewajibkan_lokasi_terdaftar_dan_memisahkan_lokasi_serta_obat` | Diperluas dan diganti nama: + lokasi tidak terdaftar → `PHM036`; + mutasi dengan obat yang tidak sesuai batch tampil terpisah |
| `Pemeriksaan_buku_mutasi_lewat_controller_dijaga_DrugStock_Read_dan_menjawab_422_PHM036` | **Baru**: atribut izin, `[Authorize]`, route, dan 422 `PHM036` untuk kosong, `Guid.Empty`, dan tidak terdaftar |

**Status:** kurung seimbang. Nama member, namespace, dan konstruktor (`DrugStockController(DrugStockService)`, `DrugStockService(ApplicationDbContext, IHttpContextAccessor, LoggerService)`, `AccessPermissionAttribute.Arguments`) diperiksa terhadap source. **Belum dikompilasi dan belum dijalankan.**

**Risiko yang tersisa:** semua mutasi satu lokasi dimuat ke memori, karena SQLite uji tidak dapat menjumlah decimal. Untuk gudang induk dengan riwayat panjang, saringan `drugId` mengurangi beban. Bila lambat di PostgreSQL, penjumlahan dapat dipindah ke SQL (PostgreSQL mendukung `SUM` decimal), dengan pola uji yang disesuaikan.

## 3. FIN-LGS-043: keputusan S1–S5 (Mutasi Saldo Persediaan)

Rumus historis **tidak** diimplementasikan sebelum lembar ini disetujui.

| ID | Keputusan | Rekomendasi | Alternatif | Konsekuensi rekomendasi | Kriteria uji sesudah implementasi | Keputusan |
| --- | --- | --- | --- | --- | --- | --- |
| **S1** | Cara menghitung saldo awal periode | **Mundur dari saldo terkini**: `Saldo awal = Saldo tersimpan saat ini − Σ mutasi sejak tanggal mulai`, **hanya** untuk kunci yang hasil pemeriksaan buku mutasinya `Match` | Maju dari awal buku mutasi (`Σ mutasi sebelum tanggal mulai`) | Bertumpu pada saldo resmi yang tersimpan. Bila buku mutasi lengkap, hasilnya identik dengan cara maju. Bila tidak lengkap, angka tidak dinyatakan valid (S4) | Untuk obat uji: saldo awal + masuk − keluar ± penyesuaian = saldo akhir; dan saldo akhir periode yang berakhir hari ini = saldo tersimpan | ☐ Setuju ☐ Ubah |
| **S2** | Status yang dihitung | **Semua status fisik** (siap pakai, karantina, kedaluwarsa, rusak) untuk ringkasan obat. Perubahan status tidak dihitung sebagai masuk/keluar karena nilai bersihnya nol untuk obat itu. Rincian per status ditampilkan terpisah | Hanya siap pakai (perubahan status tampil sebagai masuk/keluar) | Saldo cocok dengan stok fisik di gudang. Barang rusak/karantina tetap terlihat di rincian | Memindahkan 5 ke karantina tidak mengubah saldo akhir obat, tetapi rincian status bergeser 5 | ☐ Setuju ☐ Ubah |
| **S3** | Rincian | **Ringkasan per obat × lokasi**, dengan rincian per batch yang dapat dibuka | Hanya per obat; atau hanya per batch | Ringkas untuk manajemen, dapat ditelusuri untuk audit batch | Jumlah rincian batch = baris ringkasan obat | ☐ Setuju ☐ Ubah |
| **S4** | Lokasi atau kunci yang buku mutasinya tidak cocok | **Peringatan jelas.** Saldo awal/akhir kunci itu ditandai "tidak dapat dihitung" dan **tidak** dinyatakan valid. Masuk/keluar tetap tampil. Ringkasan lokasi menyebut jumlah kunci bermasalah dan menaut ke pemeriksaan buku mutasi | Tolak laporan untuk seluruh lokasi | Pengguna tetap melihat pergerakan, tetapi tidak mendapat saldo historis yang mungkin keliru | Lokasi dengan 1 kunci `SumMismatch`: kunci itu bertanda "tidak dapat dihitung"; kunci lain normal | ☐ Setuju ☐ Ubah |
| **S5** | Periode | **Sama dengan laporan yang ada**: bawaan 30 hari, maksimal 366 hari, tanggal lokal server, batas akhir inklusif, `PHM035` | Periode bulanan tetap (tutup buku) | Konsisten dengan Laporan Mutasi; tidak ada konsep tutup buku | 367 hari → 422 `PHM035` | ☐ Setuju ☐ Ubah |

**Desain sesudah disetujui** (tanpa migrasi):

- `GET drug-stock/reports/balance-movement?storageLocationId=…&startDate=…&endDate=…&drugId=…`.
- Lokasi wajib.
- Pemeriksaan buku mutasi dipakai ulang untuk menandai kunci.
- Frontend: route `/logistics-reports/balance-movement` dan tautan menu "Mutasi Saldo Persediaan". Kolom: Obat, Satuan, Saldo Awal, Masuk, Keluar, Penyesuaian, Saldo Akhir, Saldo Saat Ini, dan Status Buku (valid / tidak dapat dihitung).

## 4. FIN-LGS-044: Riwayat Mutasi

| Aspek | A. Riwayat dokumen transfer dan retur | B. Kartu Stok disaring sumber transfer/retur |
| --- | --- | --- |
| Sumber data | `GET stock-transfers` + `GET drug-returns` (dua daftar) | `GET drug-stock/mutations?sourceDocumentType=…` (satu daftar yang sudah ada) |
| Informasi | Nomor dokumen, lokasi asal/tujuan, status dokumen (termasuk **dalam perjalanan**), alasan retur | Per baris pergerakan: obat, batch, lokasi, jenis, jumlah, saldo sebelum/sesudah, pelaku, waktu, jenis dan id dokumen sumber |
| Tumpang tindih | Tinggi: kedua daftar sudah tertaut ("Transfer Antar Gudang", "Retur Unit") | Tidak ada: Kartu Stok menampilkan pergerakan, bukan dokumen |
| Penggandaan data | Ada (dua daftar digabung di layar) | **Tidak ada** |
| Kekurangan | Butuh layar gabungan baru | Barang "dalam perjalanan" tidak terlihat (baru tercatat masuk saat diterima). Itu sudah tersedia di "Transfer Antar Gudang" |
| Perkiraan | ±1,5 hari | ±0,5 hari (route + tautan; saringan sudah ada) |

**Rekomendasi: B.** Informasi pergerakan yang dibutuhkan sudah ada di Kartu Stok tanpa menggandakan data, dan status dokumen tetap dilihat di daftar transfer/retur yang sudah tertaut. **Belum aktif**: makna entri adalah keputusan pemrakarsa (☐ A ☐ B ☐ lainnya).

**Kontrak API untuk B** (sudah ada; tidak perlu perubahan backend):

| Atribut | Isi |
| --- | --- |
| Endpoint | `GET /api/v1/health-services/pharmacy-management/drug-stock/mutations` |
| Izin | `DrugStock:Read` |
| Query | `drugId?`, `drugBatchId?`, `storageLocationId?`, `mutationType?`, `sourceDocumentType?` (satu nilai: `StockTransfer`, `PrescriptionReturn`, …), `sourceDocumentId?`, `startDate?`, `endDate?`, `pageNumber`, `pageSize` |
| Respons | `PagedResult<DrugStockMutationResponse>`: `drugName`, `batchNumber`, `storageLocationName`, `status`, `mutationType`, `quantityChange`, `balanceBefore`, `balanceAfter`, `reason`, `sourceDocumentType`, `sourceDocumentId`, `actorName`, `occurredAt` |
| Catatan | Bila keputusan meminta transfer **dan** retur dalam satu daftar sekaligus, dibutuhkan perluasan kecil `sourceDocumentTypes` (banyak nilai). Bila cukup dipilih satu per satu, tidak perlu perubahan |

**Skema frontend B:**

- Route `/health-services/pharmacy-management/drug-stock/card/transfer-history` yang membuka `DrugStockCardView` dengan saringan awal sumber sesuai keputusan.
- Tautan menu "Riwayat Mutasi".
- Kolom tetap seperti Kartu Stok; kolom Sumber menaut ke dokumen asal.

**Bagian netral yang sudah diimplementasikan** (tidak mengaktifkan makna apa pun):

| Berkas | Perubahan |
| --- | --- |
| `drug-stock-constant.jsx` | `DRUG_STOCK_SOURCE_OPTIONS`, diturunkan dari `DRUG_STOCK_SOURCE_LABEL` |
| `use-drug-stock.jsx` | `sourceDocumentType: ""` pada saringan bawaan Kartu Stok (ikut reset) |
| `drug-stock-card-view.jsx` | Saringan "Sumber Transaksi" (`FilterSelect`, placeholder "Semua sumber") |
| `logistics-module-navigation.test.mjs` | Uji baru: opsi, saringan bawaan, pengiriman ke backend, dan **entri "Riwayat Mutasi" tetap tanpa tautan** |

## 5. FIN-LGS-045: keputusan R1–R5 (Rekomendasi Pengisian Ulang)

**Prinsip yang tidak bergantung keputusan** (sudah dipakai layar Hampir Habis):

- hanya kebijakan stok yang **aktif, berlaku, ber-lokasi, dan `ReorderPointQuantity > 0`**;
- tidak ada angka untuk parameter yang belum dikonfigurasi.

| ID | Keputusan | Pilihan | Catatan untuk diputuskan | Keputusan |
| --- | --- | --- | --- | --- |
| **R1** | Rumus kebutuhan | (a) `ReorderQuantity` kebijakan; (b) `Maximum − siap pakai`; (c) `Maximum − (siap pakai + permintaan yang sedang berjalan)` | (a) memerlukan `ReorderQuantity > 0`; (b)/(c) memerlukan `MaximumStockQuantity > 0`. Bila parameter yang diperlukan bernilai 0: **tidak ada usulan**, ditandai "Kebijakan belum lengkap". (c) memerlukan `ServiceUnitId` gudang untuk menemukan permintaan yang berjalan | ☐ a ☐ b ☐ c |
| **R2** | Cakupan lokasi | Semua lokasi berkebijakan; atau hanya depo/unit (gudang induk diarahkan ke Kebutuhan Pengadaan) | Gudang induk diisi dari pengadaan, bukan dari permintaan barang unit | ☐ semua ☐ depo/unit |
| **R3** | Tindakan | (i) daftar saja; (ii) tombol "Buat Permintaan" yang mengisi formulir (petugas tetap menyimpan dan mengajukan); (iii) draf otomatis bila `IsAutoReorderEnabled` | (iii) membuat transaksi tanpa tindakan petugas dan butuh keputusan tanggung jawab | ☐ i ☐ ii ☐ iii |
| **R4** | Pembulatan | Ke atas dalam satuan stok; atau ke kemasan beli (konversi satuan master) | Kemasan beli hanya relevan untuk pengadaan | ☐ satuan stok ☐ kemasan |
| **R5** | Satuan kebijakan ≠ satuan stok obat | Dikecualikan dari usulan dan ditandai "Satuan kebijakan keliru", seperti `isUnitMismatch` di Hampir Habis; atau dikonversi | Konversi bisa menyamarkan kesalahan master | ☐ kecualikan ☐ konversi |

**Desain backend sesudah disetujui** (tanpa migrasi):

- `GET drug-stock/reports/replenishment` (`DrugStock:Read`).
- Penyaring kebijakan sama dengan `GetLowStockAsync` (`DrugStockService.cs:237-`).
- Baris: `drugId/Code/Name`, `storageLocationId/Name`, `stockUnitName`, `quantityAvailable`, `reorderPointQuantity`, `maximumStockQuantity`, `reorderQuantity`, `leadTimeDays`, `inProgressQuantity` (R1c), `recommendedQuantity` (null bila tidak dapat dihitung), `basis`, `reason`.
- `reason` bernilai `Recommended`, `PolicyIncomplete`, `UnitMismatch`, atau `AboveReorderPoint`.
- Keterbatasan dikirim dalam respons.

**Skema frontend:**

- Route `/health-services/pharmacy-management/drug-stock/replenishment` dan tautan menu "Rekomendasi Pengisian Ulang".
- Kolom: Obat, Lokasi, Siap Pakai, Titik Pesan Ulang, Maksimum, Jumlah Pesan Ulang (kebijakan), Usulan, Dasar Perhitungan.
- Aksi "Buat Permintaan" hanya bila R3(ii).
- Kotak keterbatasan.

**Rencana uji** (dibuat bersama implementasi):

1. Usulan = rumus R1 untuk kebijakan lengkap.
2. `ReorderQuantity`/`Maximum` = 0 → `recommendedQuantity = null`, `reason = PolicyIncomplete`.
3. Satuan keliru → dikecualikan (R5).
4. Di atas titik pesan ulang → tidak muncul.
5. Kebijakan nonaktif atau di luar masa berlaku → tidak muncul.
6. Pembulatan R4.
7. R1(c): permintaan Menunggu/Disetujui mengurangi usulan, permintaan Selesai/Batal tidak.
8. Frontend: tidak ada angka bila `recommendedQuantity` null; tombol hanya bila R3(ii).

## 6. FIN-LGS-046: kesiapan integrasi dan UAT

### 6.1 Status 33 menu tertaut

Tetap **33 tertaut** dari 66 yang dapat diklik (67 entri):

- **29 fungsional penuh**;
- **4 terbuka tetapi menunggu data persetujuan**: Validasi Atasan, Daftar Permintaan Disetujui, Pemenuhan Sebagian, Laporan Distribusi.

Lima laporan tidak diubah, kecuali penyelarasan label pada tahap sebelumnya. Kartu Stok kini punya saringan "Sumber Transaksi" tambahan. Penilaian ini berdasarkan source dan uji otomatis; belum ada UAT.

### 6.2 Verifikasi statis perubahan ledger

| Pemeriksaan | Hasil |
| --- | --- |
| Kurung `()`, `{}`, `[]` di `LogisticsReportQueries.cs` dan `LogisticsReportTests.cs` | Seimbang |
| Member dan tipe | `PhmDrugBatches`/`BatchNumber`, `MstDrugStorageLocations.StorageLocationName`, kunci tuple `(DrugId, DrugBatchId, Status)` sama tipe di kedua kamus, `TryGetValue` + `GetValueOrDefault` |
| Benturan nama `using` baru di uji (`System.Reflection`, `System.Text.Json`, `Microsoft.AspNetCore.Authorization`/`Http`, `Microsoft.Extensions.Logging.Abstractions`, `QuilvianSystemBackend.Attributes`/`Services.Logging`) | Tidak ada. `QuilvianSystemBackend.Attributes` hanya berisi `Access*Attribute` |
| `SortOrder` 14 di `DrugStockController` | Hanya dipakai endpoint ini |
| Frontend | **55/55 lulus** (54 + 1 baru); ESLint 0 error. 1 warning lama `use-drug-stock.jsx:67` (`set-state-in-effect` pada efek `load()` yang sudah ada); diff hanya di baris 133 |

### 6.3 Skenario UAT pemeriksaan buku mutasi

Dijalankan di lingkungan uji yang disetujui, dengan data uji yang dibuat lewat layar. **Tidak** di basis data operasional. Akses lewat alat API/Swagger dengan akun ber-`DrugStock:Read`.

| ID | Langkah | Harapan (PASS) |
| --- | --- | --- |
| L1 | Untuk setiap gudang uji: `GET …/ledger-consistency?storageLocationId={gudang}&onlyInconsistent=false` | 200. Catat `checkedKeyCount`, `matchCount`, dan `isConsistent` sebagai bukti keputusan S1/S4. **Jangan** menyimpulkan cocok sebelum hasil ini terbaca |
| L2 | Lakukan saldo pembuka, penyerahan, transfer keluar/masuk, perubahan status, dan pemakaian pada obat uji; ulangi L1 | Semua kunci obat uji `Match`. `ledgerQuantity` = saldo di Kartu Stok |
| L3 | Ulangi L1 tanpa `onlyInconsistent` | Hanya baris tidak cocok yang tampil (bisa kosong) |
| L4 | Tanpa `storageLocationId`; dengan `00000000-0000-0000-0000-000000000000`; dengan id acak | 422 dengan `data.code = "PHM036"` |
| L5 | Lokasi terdaftar tanpa stok | 200, `checkedKeyCount = 0`. Dicatat sebagai "tidak ada yang diperiksa", bukan bukti cocok |
| L6 | Akun tanpa `DrugStock:Read` | 403 |
| L7 | Bandingkan jumlah saldo dan jumlah mutasi (Kartu Stok) sebelum dan sesudah L1–L6 | Tidak berubah (read-only) |

**Bila L1/L2 menemukan kunci tidak cocok:** catat obat, batch, status, dan selisihnya, lalu telusuri asal saldonya (impor data, perubahan di luar aplikasi). **Jangan** mengoreksi saldo otomatis. Koreksi hanya lewat penyesuaian manual yang disetujui.

### 6.4 Blocker

| Butir | Status |
| --- | --- |
| Persetujuan atasan dan CITO (U7, R6, P4) | **BLOCKED_DATA** sampai mapping `OrganizationUnitId`, penempatan manajerial, dan izin tersedia (*LANJUTAN FINISHING* 5) |
| `LGS-OQ-027` (D1–D7), `LGS-OQ-029` (W1–W3) | **BLOCKED_DECISION** |
| Riwayat Mutasi, Rekomendasi Pengisian Ulang, Mutasi Saldo Persediaan | **BLOCKED_DECISION** (bagian 3–5) |
| `LGS-OQ-003`, `008`, `009`, `017`, `018`, `024`–`029` | Terbuka; tidak ada yang diputuskan agent |
| Integrasi | Tanpa konflik menurut pemeriksaan sebelumnya (*PENYELESAIAN LOGISTIK V2* 2); urutan backend dulu |

### 6.5 Daftar integrasi (gabungan terbaru)

| Repo | Berkas lokal | Isi tambahan tahap ini | Status uji |
| --- | --- | --- | --- |
| Backend | **10** (7 diubah, 3 baru), jumlah tetap | Perbaikan `GetLedgerConsistencyAsync`; uji di `LogisticsReportTests.cs` | **Belum dijalankan**; target 124 metode / **144 kasus** |
| Frontend | **37** (12 diubah, 25 baru); bertambah `drug-stock-constant.jsx` dan `use-drug-stock.jsx` | Saringan Sumber Transaksi Kartu Stok | **55/55 lulus** |

Pengecualian commit tetap: `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*`, `test-results/.last-run.json`.

## 7. Daftar periksa pengujian backend 144 kasus berikutnya

Perintah dijalankan di folder proyek terisolasi, foreground, satu proses:

```bash
dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1
dotnet test  StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run7.trx"
```

| ☐ | Periksa | Harapan |
| --- | --- | --- |
| ☐ | Proyek terisolasi memuat glob `LogisticsReportTests\*.cs` | Sudah (tidak perlu diubah) |
| ☐ | Build | 0 error. Tidak ada warning baru dari `LogisticsReport*`, `DrugStockController`, `DrugStockService`, atau `LogisticsReportTests` (pembanding: 599 warning lama) |
| ☐ | Jumlah kasus | `Total tests: 144`: 135 lama + 9 `LogisticsReportTests` |
| ☐ | Hasil | `Passed: 144, Failed: 0, Skipped: 0`; TRX `notExecuted=0` |
| ☐ | Tiga uji buku mutasi tercatat di TRX | `…menandai_saldo_yang_tidak_dapat_direkonstruksi`, `…mewajibkan_lokasi_terdaftar_dan_memisahkan_lokasi_serta_obat`, `…lewat_controller_dijaga_DrugStock_Read_dan_menjawab_422_PHM036` |
| ☐ | Enam uji laporan dan 135 uji lama | Tetap lulus (pembanding `run6`) |
| ☐ | Bila uji controller gagal karena serialisasi | Periksa bentuk `ApiResponse<object>.Fail(…, new { exception.Code })`; harapan isi memuat `PHM036` |
| ☐ | Bila ada kasus gagal | Perbaiki hanya source Logistik penyebabnya, jalankan ulang uji terkait, lalu ulang 144 kasus |
| ☐ | Sesudah lulus | Perbarui laporan ini dengan hasil `run7`, lalu pemilik proyek commit backend (10 berkas) dan frontend (37 berkas) |

---

# PENUTUPAN CHECKPOINT DAN LEMBAR KEPUTUSAN FINAL (FIN-LGS-047 s.d. FIN-LGS-051)

**Tanggal:** 19 November 2026

**Branch:** backend `Ikbal` di `a3b2ed08`, frontend `Ikbalv2` di `109a40831`. Semua perubahan masih lokal.

**Yang tidak dilakukan:** commit, push, PR, merge, `dotnet build`/`test`, `npm run build`/`dev`, migrasi, perubahan basis data, keputusan yang masih pending, dan data dummy.

**Yang dijalankan:** uji unit frontend (Node) dan ESLint.

Bagian ini **tidak** mengulang analisis FIN-LGS-042–046; ia menutup checkpoint dan merangkum keputusan dalam satu lembar.

## 1. Pekerjaan independen yang selesai (FIN-LGS-051)

| Perbaikan | Berkas | Alasan |
| --- | --- | --- |
| Label "Pesanan Optik" untuk sumber `OpticalOrder` di Kartu Stok | `drug-stock-constant.jsx` | Modul Optik menulis mutasi `OpticalOrder`, tetapi Kartu Stok menampilkan kode mentahnya. Label kini ada di sumber tunggal (`DRUG_STOCK_SOURCE_LABEL`), sehingga ikut muncul di saringan "Sumber Transaksi" dan di Dashboard |
| Penyederhanaan label laporan | `logistics-report-constant.js` | `STOCK_SOURCE_DOCUMENT_LABELS` kini salinan langsung label Kartu Stok, tanpa tambahan sendiri |
| Prop cari mati di Kartu Stok dibuang | `drug-stock-card-view.jsx` | `searchValue=""` dan `onSearchChange={undefined}` tidak berfungsi; Kartu Stok memang tidak punya kotak cari |
| Uji diperketat | `logistics-module-navigation.test.mjs` | Asersi label `OpticalOrder` dan tidak adanya `searchValue` |

**Hasil:** uji frontend **55/55 lulus**; ESLint pada berkas yang diubah **0 masalah**. Jumlah berkas frontend tetap 37, karena semua berkas di atas sudah ada di daftar. Backend tidak diubah pada tahap ini.

## 2. Status 144 kasus backend dan instruksi eksekusi sekali jalan (FIN-LGS-047)

**Status:**

- Proyek terisolasi meng-compile `Infrastructure\{TestDatabase, StockRequestApprovalHarness, CatatanLogUji}.cs`, `StockRequestApprovalTests\*.cs` (14 berkas), dan `LogisticsReportTests\*.cs`.
- Hitungan dari source: **124 metode = 144 kasus**, terdiri dari 135 lama (lulus di `run6`) dan 9 `LogisticsReportTests` (6 lulus di `run6`, 3 buku mutasi belum pernah dijalankan).
- **Belum ada klaim 144 PASS.**
- Frontend **55/55 lulus**.

**Instruksi untuk pemilik proyek.** Jalankan sekali, foreground, satu proses, tanpa migrasi; uji memakai SQLite dalam memori:

```bash
# 1) Masuk ke folder proyek uji terisolasi. Bila folder sementara sudah terhapus, buat ulang
#    dari Lampiran C di bawah ke folder mana pun, lalu jalankan perintah dari folder itu.
cd "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests"

# 2) Build — harapan: "0 Error(s)"; tidak ada warning baru dari berkas Logistik
dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1

# 3) Uji — harapan: "Total tests: 144  Passed: 144"
dotnet test StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run7.trx"

# 4) Frontend — harapan: "# tests 55  # pass 55  # fail 0"
cd "<repo>\QuilvianSystemFrontendDev"
node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs
```

**Sesudah keempatnya lulus:** commit backend (10 berkas), lalu frontend (37 berkas). Daftarnya ada di *FINALISASI LEDGER* 6.5.

- Jangan ikut di-commit: `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*`, `test-results/.last-run.json`.
- PR backend lebih dulu.

**Bila ada kasus gagal:** perbaiki hanya source Logistik penyebabnya, lalu ulangi langkah 2–3. Rincian periksa ada di *FINALISASI LEDGER* 7.

## 3. Lembar keputusan final

Semua butir di bawah **belum disetujui** dan **tidak aktif**. Kolom "Syarat aktif" wajib terpenuhi sebelum implementasi dinyalakan.

### 3.1 Mutasi Saldo Persediaan (FIN-LGS-049)

| ID | Rekomendasi final | Syarat aktif | Keputusan |
| --- | --- | --- | --- |
| S1 | Saldo awal = saldo tersimpan saat ini − Σ mutasi sejak tanggal mulai. Dihitung **hanya** untuk obat × batch × status yang pemeriksaan buku mutasinya `Match` | Persetujuan S1–S5, **dan** UAT L1–L2 lulus untuk lokasi itu | ☐ Setuju ☐ Ubah: … |
| S2 | Ringkasan memakai semua status fisik. Perubahan status tidak dihitung masuk/keluar, tetapi tampil di rincian status | Persetujuan | ☐ Setuju ☐ Ubah |
| S3 | Ringkasan per obat × lokasi, dengan rincian batch yang dapat dibuka | Persetujuan | ☐ Setuju ☐ Ubah |
| S4 | Kunci yang tidak `Match`: saldo awal/akhir **tidak** ditampilkan sebagai angka valid ("tidak dapat dihitung" + peringatan + tautan ke pemeriksaan buku mutasi). Masuk/keluar tetap tampil | Persetujuan | ☐ Setuju ☐ Ubah |
| S5 | Periode sama dengan laporan yang ada: bawaan 30 hari, maksimal 366 hari, tanggal lokal server, `PHM035` | Persetujuan | ☐ Setuju ☐ Ubah |

**Aturan yang tidak dapat ditawar:** saldo historis tidak pernah dinyatakan valid untuk kunci atau lokasi yang buku mutasinya tidak konsisten, dan tidak ada koreksi saldo otomatis.

### 3.2 Rekomendasi Pengisian Ulang (FIN-LGS-050)

| ID | Rekomendasi final | Alternatif | Keputusan |
| --- | --- | --- | --- |
| R1 | **Maksimum − siap pakai**, dibulatkan menurut R4. Usulan hanya dihitung bila kebijakan memenuhi semua syarat validitas di bawah | `ReorderQuantity` kebijakan; atau Maksimum − (siap pakai + permintaan berjalan) | ☐ Setuju ☐ Ubah |
| R2 | Semua lokasi yang punya kebijakan valid tampil di daftar. Tindakan "Buat Permintaan" hanya untuk lokasi yang diisi dari gudang internal; gudang induk ditandai "diisi lewat pengadaan" (Kebutuhan Pengadaan belum tersedia) | Hanya depo/unit | ☐ Setuju ☐ Ubah |
| R3 | **Tindakan manual:** tombol "Buat Permintaan" mengisi formulir Pengajuan Kebutuhan Barang; petugas meninjau, menyimpan, dan mengajukan sendiri. **Tidak ada** pembuatan permintaan otomatis, walaupun `IsAutoReorderEnabled` bernilai `true` | Daftar saja; draf otomatis | ☐ Setuju ☐ Ubah |
| R4 | Dibulatkan **ke atas** ke bilangan bulat dalam satuan stok obat | Ke kemasan beli | ☐ Setuju ☐ Ubah |
| R5 | Satuan kebijakan ≠ satuan stok obat: **dikecualikan** dari usulan dan ditandai "Satuan kebijakan keliru" (perlu perbaikan master), tanpa konversi | Dikonversi | ☐ Setuju ☐ Ubah |

**Syarat validitas usulan.** Bila salah satu tidak terpenuhi, **tidak ada angka** yang ditampilkan, hanya alasannya:

1. Kebijakan aktif dan berlaku hari ini.
2. Kebijakan punya lokasi.
3. `ReorderPointQuantity > 0`.
4. `MaximumStockQuantity > ReorderPointQuantity`.
5. Satuan kebijakan = satuan stok obat.
6. Siap pakai ≤ titik pesan ulang.

**Syarat aktif:** persetujuan R1–R5. Implementasi sesuai desain *FINALISASI LEDGER* 5, tanpa migrasi, ±1–2 hari termasuk uji.

### 3.3 Riwayat Mutasi (FIN-LGS-048)

| ID | Keputusan yang diperlukan | Rekomendasi | Keputusan |
| --- | --- | --- | --- |
| RM-1 | Makna entri "Riwayat Mutasi" | **B:** Kartu Stok yang disaring sumber transfer dan retur. Data pergerakan sudah ada, tanpa penggandaan; status dokumen "dalam perjalanan" tetap dilihat di "Transfer Antar Gudang" | ☐ B ☐ A (riwayat dokumen) ☐ lainnya |
| RM-2 | Isi daftar | **Satu daftar gabungan** transfer (`StockTransfer`) dan retur unit (`PrescriptionReturn`). Kedua kode ini ditulis `StockTransferService` dan `DrugReturnService` | ☐ gabungan ☐ dipilih satu per satu lewat saringan |

**Perubahan yang sudah disiapkan, diterapkan sesudah RM-1/RM-2 disetujui.** Saringan "Sumber Transaksi" di Kartu Stok sudah ada (FIN-LGS-044); sisanya:

| Bagian | Bila RM-2 = gabungan | Bila RM-2 = satu per satu |
| --- | --- | --- |
| Backend | `DrugStockMutationQuery` menerima `sourceDocumentTypes` (banyak nilai); query memakai `Contains`; 1 uji. Tanpa migrasi | Tidak ada perubahan |
| Hook | `useStockMutations(initialFilters)`: saringan awal dan nilai reset dari parameter; pemanggil lama tetap memakai `MUTATION_FILTERS` | Sama |
| View | `DrugStockCardView` menerima `variant="transferHistory"` (judul "Riwayat Mutasi", saringan awal sumber) | Sama, saringan awal `StockTransfer` |
| Route | `src/app/health-services/pharmacy-management/drug-stock/card/transfer-history/page.jsx` | Sama |
| Menu | `pathname: "/health-services/pharmacy-management/drug-stock/card/transfer-history"` pada entri `healthServicesLogisticsTransferHistory` (tertaut 33 → 34) | Sama |
| Uji frontend | Menu, route, varian, dan saringan awal; ubah asersi FIN-LGS-044 ("tetap tanpa tautan") | Sama |
| Perkiraan | ±0,5–1 hari | ±0,5 hari |

## 4. Blocker yang masih menghalangi implementasi

| Blocker | Jenis | Yang menghalangi |
| --- | --- | --- |
| 144 kasus backend belum dijalankan | Verifikasi (larangan tahap ini) | Commit backend |
| S1–S5 + UAT L1–L2 | BLOCKED_DECISION + bukti data | Mutasi Saldo Persediaan |
| R1–R5 | BLOCKED_DECISION | Rekomendasi Pengisian Ulang |
| RM-1, RM-2 | BLOCKED_DECISION | Riwayat Mutasi |
| Mapping `OrganizationUnitId`, penempatan manajerial, izin `Approve`/`AuthorizeEmergency`, jabatan CITO, tenggat | BLOCKED_DATA | UAT persetujuan dan CITO (U7, R6, P4) |
| `LGS-OQ-027` (D1–D7), `LGS-OQ-029` (W1–W3) | BLOCKED_DECISION | Pembatasan cakupan unit |
| `LGS-OQ-003`, `008`, `009`, `017`, `018`, `024`–`029` | Terbuka | Stok per Unit, Nilai Persediaan, Stock Opname, Penerimaan Barang, dan entri kelas C/D lain |

## 5. Status kesiapan UAT

| Cakupan | Status |
| --- | --- |
| UAT terbatas: N1–N2, R1–R5, R7–R10, P1–P3, A1–A3, L1–L7 | **SIAP** sesudah 144/144 lulus, commit, merge, dan deploy |
| UAT persetujuan dan CITO (U7, R6, P4) | **BLOCKED_DATA** |
| Fitur yang menunggu S/R/RM | Belum dapat diuji (belum diimplementasikan) |
| Produksi | **NO-GO** sampai U7 lulus |

## Lampiran C — Isi proyek uji terisolasi (untuk dibuat ulang bila folder sementara terhapus)

Simpan sebagai `StockRequestApprovalIsolated.Tests.csproj` di folder kosong mana pun di luar repo. Path `C:\tmp\phm` adalah lokasi repo backend; sesuaikan bila berbeda.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Proyek uji SEMENTARA di luar repo: hanya memuat uji Logistik, karena proyek PharmacyTests
       tidak terkompilasi akibat berkas uji Billing milik modul lain. -->
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
    <Compile Include="C:\tmp\phm\Tests\QuilvianSystemBackend.PharmacyTests\LogisticsReportTests\*.cs" LinkBase="LogisticsReportTests" />
  </ItemGroup>
</Project>
```

---

# FINAL TEST DAN PENUTUPAN LOGISTIK V2 (FIN-LGS-052 s.d. FIN-LGS-054)

**Tanggal:** 19 November 2026
**Hasil:** **READY FOR OWNER COMMIT**: backend 144/144, frontend 55/55, ESLint 0 error.
**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, perubahan basis data, deployment, build frontend, dan dev server.
**Branch:** backend `Ikbal` di `a3b2ed08`, frontend `Ikbalv2` di `109a40831`. Seluruh perubahan di bawah masih lokal.

## 1. FIN-LGS-052 — Hasil pengujian aktual

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Build backend (1) | `dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1` | 0 error, **600** warning (baseline 599). Selisih +1: **CS8602** di `LogisticsReportTests.cs:472` (`TypeFilterAttribute.Arguments` bernilai nullable), berasal dari uji baru |
| Perbaikan | `Assert.NotNull(izin.Arguments)` sebelum indeks | Hanya berkas uji Logistik |
| Build backend (2) | Sama, inkremental | 0 error; **0 warning dari berkas Logistik**. Warning baru = 0; sisa 599 adalah warning lama modul lain |
| Test backend | `dotnet test … --no-build --logger "trx;LogFileName=run7.trx"` | **Passed 144, Failed 0, Skipped 0**. TRX: `total=144 executed=144 passed=144 failed=0 notExecuted=0`. Durasi 2 m 5 s |
| Kasus buku mutasi | `run7.trx` | `…menandai_saldo_yang_tidak_dapat_direkonstruksi`, `…mewajibkan_lokasi_terdaftar_dan_memisahkan_lokasi_serta_obat`, `…lewat_controller_dijaga_DrugStock_Read_dan_menjawab_422_PHM036`: ketiganya **Passed** |
| Enam laporan + 135 kasus lama | `run7.trx` | Semua **Passed** |
| DB operasional / migrasi / startup | — | **Tidak ada**. SQLite dalam memori (`TestDatabase`); `appsettings`, host, dan seeder tidak dijalankan; `SkipMigrationMetadata=true` |
| Test frontend | `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs` | **pass 55, fail 0, skipped 0** |
| ESLint frontend | `npx eslint` pada seluruh 37 berkas `.js/.jsx/.mjs` yang berubah | **0 error, 2 warning lama** (`use-drug-stock.jsx:67`, `use-stock-request-list.jsx:109`: `set-state-in-effect` pada efek `load()` yang sudah ada; bukan baris yang diubah) |

## 2. FIN-LGS-053 — Finalisasi perubahan

| Pemeriksaan | Hasil |
| --- | --- |
| Pemeriksaan buku mutasi tetap read-only | `GetLedgerConsistencyAsync`: 4× `AsNoTracking`, 0× `SaveChanges`/`Add`/`Update`/`Remove`/`ExecuteUpdate`. Uji membuktikan jumlah saldo dan jumlah mutasi tidak berubah (`PotretStokAsync`) |
| Label `OpticalOrder` | `DRUG_STOCK_SOURCE_LABEL.OpticalOrder = "Pesanan Optik"`, dipakai Kartu Stok, saringan Sumber Transaksi, dan Dashboard. Diasersi uji |
| Saringan sumber transaksi Kartu Stok | `sourceDocumentType` ada di saringan bawaan (ikut reset) dan terkirim lewat `cleanParams` ke `GET drug-stock/mutations?sourceDocumentType=…`, yang sudah menyaring di backend. Diasersi uji |
| Perubahan modul lain | **Tidak ada**. Backend hanya `Areas/HealthServices/PharmacyManagement/**`, `Tests/…/LogisticsReportTests/**`, `docs/module-blueprints/logistik/**`. Frontend hanya `pharmacy-management/**`, `logistics-report*`, 6 baris `pathname` di bagian Logistik `menu-items.jsx`, dan `tests/unit/logistics-*` |
| File Logistik yang belum di-commit | Teridentifikasi lengkap di bagian 3 |

## 3. FIN-LGS-054 — Daftar perubahan untuk commit gabungan pemilik proyek

**Commit backend** (`Ikbal`), **10 berkas**:

| Status | Berkas |
| --- | --- |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` |

**Commit frontend** (`Ikbalv2`), **37 berkas**:

| Status | Berkas |
| --- | --- |
| Ubah | `src/components/view/health-services/pharmacy-management/drug-return-view/drug-return-list-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-card-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-emergency-review-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-history-log-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-list-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-transfer-view/stock-transfer-list-view.jsx` |
| Ubah | `src/lib/constants/health-services/pharmacy-management/drug-stock-constant.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-drug-stock.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx` |
| Ubah | `src/utils/menu-sidebar/menu-items.jsx` |
| Ubah | `tests/unit/logistics-module-navigation.test.mjs` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/distribution/distribution-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/distribution/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/flow-summary/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/flow-summary/stock-flow-dashboard-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/inventory/inventory-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/inventory/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/mutations/mutation-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/mutations/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/usage-by-unit/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/usage-by-unit/usage-by-unit-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/stock-requests/fulfillment/partial/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/stock-requests/fulfillment/partial/stock-request-partial-fulfillment-client.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/components/report-feedback.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/components/stock-flow-daily-chart.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/distribution-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/inventory-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/mutation-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/stock-flow-dashboard-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/usage-by-unit-report-view.jsx` |
| Baru | `src/lib/constants/health-services/pharmacy-management/logistics-report-constant.js` |
| Baru | `src/lib/hooks/health-services/pharmacy-management/use-logistics-report.jsx` |
| Baru | `src/lib/services/health-services/pharmacy-management/logistics-report.service.js` |
| Baru | `src/style/health-services/pharmacy-management/logistics-report.module.css` |
| Baru | `src/utils/health-services/pharmacy-management/logistics-report-utils.js` |
| Baru | `tests/unit/logistics-reports.test.mjs` |

**Jangan ikut di-commit** (bukan pekerjaan ini): backend `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/`; frontend `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.

**Isi commit dalam satu kalimat per repo:**

- **Backend:** lima laporan Logistik (Persediaan, Mutasi, Distribusi, Pemakaian per Unit, Penerimaan & Pengeluaran) dan pemeriksaan kelengkapan buku mutasi, semuanya read-only, tanpa migrasi, dengan `PHM035`/`PHM036`; ditambah 9 uji dan laporan FIN-LGS-024–054.
- **Frontend:** lima halaman laporan; Pemenuhan Sebagian; saringan Sumber Transaksi dan label Pesanan Optik di Kartu Stok; perbaikan halaman dan reset pencarian di tujuh layar Logistik; 6 entri menu yang sudah ada kini tertaut (33 tertaut dari 67 entri); uji 55/55.

**Urutan:**

1. Commit backend.
2. Commit frontend.
3. Push dan PR backend ke `QuilvianIntegrationBackend`.
4. Sesudah merge, PR frontend ke `QuilvianIntegrationFrontend`. Pemeriksaan sebelumnya menunjukkan tidak ada konflik.
5. Deploy, lalu UAT terbatas (N1–N2, R1–R5, R7–R10, P1–P3, A1–A3, L1–L7).

**Tetap terbuka** (tidak berubah):

- UAT persetujuan atasan dan CITO: **BLOCKED_DATA**.
- `LGS-OQ-027`/`029`: **BLOCKED_DECISION**.
- Keputusan S1–S5, R1–R5, RM-1/RM-2 (*PENUTUPAN CHECKPOINT* 3).
- Produksi: **NO-GO** sampai UAT persetujuan lulus.

---

# PEMERIKSAAN ULANG INTEGRASI (20 November 2026)

**Posisi:**

- Belum ada commit baru. `Ikbal` = `origin/Ikbal` = `a3b2ed08` dengan 10 berkas lokal.
- `Ikbalv2` = `origin/Ikbalv2` = `109a40831` dengan 37 berkas lokal.
- Branch integration sudah bergerak, sehingga diperiksa ulang secara read-only: `git fetch`, `git merge-tree`, dan simulasi `git merge-file` di scratchpad.

| Repo | Integration lama → baru | Isi perubahan baru | Hasil |
| --- | --- | --- | --- |
| Backend | `659d4635` → `d17aa187` (+33 commit, 291 berkas; PR #361–#364) | HR (125), Manajemen Aset (62), Payroll (4), dokumen. Migrasi `AddHrdOvertimeSetting` dan `AddAssetMovement` | **0 konflik** (`merge-tree`). **Tidak ada irisan berkas.** `ApplicationDbContext`: 0 baris dihapus. Kedua migrasi tidak menyentuh tabel `Phm*`/`MstDrug*`/`MstServiceUnit`/`RegPatientEncounter`. Model HR yang dibaca persetujuan tidak berubah. `Program.cs` hanya menggerbangi worker HR; tidak ada perubahan JSON, otorisasi, atau registrasi Farmasi. Manajemen Aset tidak memakai kode atau tabel Farmasi |
| Frontend | `cb5174327` → `b514a2991` (+18 commit, 440 berkas) | HR (≈300 berkas), `base-file-upload`, `base-form-control`, `use-hr-select`, `select-resource-registry`, saringan izin menu | **0 konflik** (`merge-tree`). Irisan hanya `menu-items.jsx`. Hunk integration di baris 100–1020, di luar bagian Logistik (2775–3237). Simulasi tiga arah: **0 konflik**, hasil tetap 67 entri Logistik dan 33 tertaut. `filter-menu-items-by-permission.jsx` hanya menambah dukungan `requiredPermission` berupa daftar, yang tidak dipakai entri Logistik. `select-resource-registry`: 0 baris dihapus; resource `drugs`, `serviceUnits`, `drugStorageLocations`, `employees` tidak berubah. Base component yang dipakai laporan tidak disentuh |

**Kesimpulan:** status **READY FOR OWNER COMMIT** tetap berlaku tanpa perubahan source. Hasil uji `run7` (144/144) dan frontend 55/55 tetap sah, karena tidak ada berkas Logistik yang berubah sejak itu.

Urutan integrasi tidak berubah: backend lebih dulu, lalu frontend.

---

# IMPLEMENTASI TIGA FITUR YANG DISETUJUI (FIN-LGS-055 s.d. FIN-LGS-058)

**Tanggal:** 20 November 2026

**Keputusan yang disetujui pemrakarsa:**

- **RM-1/RM-2:** Kartu Stok, gabungan `StockTransfer` + `PrescriptionReturn`.
- **S1–S5** dan **R1–R5**.

Isi keputusan sama dengan lembar *PENUTUPAN CHECKPOINT* 3.

**Tidak termasuk:** migrasi, perubahan hak akses lintas unit, deployment, serta perubahan `LGS-OQ-027`/`029`.

**Yang tidak dilakukan:** commit, push, PR, merge, `dotnet build`/`test`, `npm run build`/`dev`, migrasi, perubahan basis data, perubahan modul lain, dan data dummy.

**Yang dijalankan:** uji unit frontend (Node) dan ESLint.

**Tidak ada BLOCKED_MIGRATION.** Ketiga fitur memakai tabel yang ada (`PhmDrugStockMutation`, `PhmDrugStockBalance`, `PhmDrugBatch`, `MstDrugStockPolicy`, `MstDrugStorageLocation`, `MstServiceUnit`).

## 1. Status

| Task | Status | Ringkas |
| --- | --- | --- |
| FIN-LGS-055 Riwayat Mutasi | **SELESAI DI SOURCE** | Saringan banyak sumber yang aman di backend; halaman memakai Kartu Stok; menu tertaut |
| FIN-LGS-056 Mutasi Saldo Persediaan | **SELESAI DI SOURCE; SALDO HISTORIS BELUM TERVERIFIKASI** | Endpoint dan halaman jadi. Saldo historis berstatus "Belum terverifikasi UAT" sampai lokasi lolos L1–L2 dan dicatat di konfigurasi (bagian 3) |
| FIN-LGS-057 Rekomendasi Pengisian Ulang | **SELESAI DI SOURCE** | Endpoint dan halaman jadi. Tombol "Buat Permintaan" hanya mengisi formulir yang ada; tidak ada permintaan atau PO otomatis |
| FIN-LGS-058 Konsolidasi | **SELESAI** | Satu paket perubahan; hanya laporan ini yang diperbarui |

**Status uji:**

- Backend: **belum dikompilasi dan belum dijalankan**. Target 129 metode = **149 kasus**: 144 lulus di `run7`, ditambah 5 baru.
- Frontend: **64/64 lulus**.
- ESLint: **0 error**.

## 2. FIN-LGS-055 — Riwayat Mutasi

**Backend.**

- `DrugStockMutationQuery.SourceDocumentTypes` (string, dipisah koma, `[MaxLength(300)]`) pada endpoint Kartu Stok yang sudah ada, `GET drug-stock/mutations`.
- Penafsiran `DrugStockService.ParseSourceDocumentTypes`:
  - hanya nilai konstanta `DrugStockSourceDocumentTypes` (dibaca lewat refleksi, tidak ditulis ulang);
  - huruf besar/kecil dinormalkan ke ejaan baku;
  - tanpa duplikat, paling banyak 10 nilai.
- Nilai yang tidak dikenal **tidak pernah melebarkan hasil**. Bila tidak ada satu pun yang dikenal, hasilnya kosong.
- Filter dijalankan sebagai `IN` berparameter.
- Dapat digabung dengan `SourceDocumentType` tunggal; keduanya harus terpenuhi.
- Tanpa parameter baru, perilaku Kartu Stok tidak berubah.

**Frontend.**

- `DrugStockCardView` mendapat varian `transferHistory`, memakai komponen, hook, dan data yang sama dengan Kartu Stok, tanpa salinan data.
- Saringan bawaan `TRANSFER_HISTORY_FILTERS` = `sourceDocumentTypes: "StockTransfer,PrescriptionReturn"`, sekaligus menjadi nilai reset.
- Saringan "Sumber Transaksi" hanya berisi Transfer Stok / Retur Resep, dengan placeholder "Transfer dan retur".
- Tersedia juga saringan tanggal, lokasi, obat, jenis pergerakan, dan halaman, sesuai kemampuan API.
- Halaman ini baca saja (kolom Koreksi disembunyikan); koreksi tetap dari Kartu Stok.

**Route:** `/health-services/pharmacy-management/drug-stock/card/transfer-history`. **Menu:** "Riwayat Mutasi" (grup Mutasi & Retur).

## 3. FIN-LGS-056 — Mutasi Saldo Persediaan

**Endpoint:** `GET /api/v1/health-services/pharmacy-management/drug-stock/reports/balance-movement`

| Atribut | Isi |
| --- | --- |
| Izin | `DrugStock:Read` (`SortOrder` 15) |
| Query | `storageLocationId` (**wajib**; kosong atau tidak terdaftar → 422 `PHM036`), `startDate?`, `endDate?` (bawaan 30 hari, maksimal 366 → 422 `PHM035`), `drugId?`, `search?`, `pageNumber`, `pageSize` |
| Baris (per obat) | `openingBalance?`, `openingRecorded`, `quantityIn`, `quantityOut`, `adjustmentNet`, `closingBalance?`, `currentOnHand`, `mutationCount`, `statusChangeCount`, `inconsistentKeyCount`, `status`, `batches[]` (kolom yang sama per batch, + `batchNumber`, `expiryDate`) |
| `status` | `Valid` (buku cocok **dan** lokasi terverifikasi) · `Unverified` (buku cocok, lokasi belum lolos UAT; angka **tidak** dinyatakan valid) · `NotComputable` (buku tidak cocok; `openingBalance`/`closingBalance` = **null**) |
| Ringkasan | `rowCount`, `validRowCount`, `unverifiedRowCount`, `notComputableRowCount`, `isLedgerConsistent`; ditambah `isLocationVerified` dan `limitations` |

**Rumus sesuai keputusan, per obat × batch × status pada lokasi:**

- **S1:**
  - `saldo awal = saldo tersimpan − Σ mutasi sejak tanggal mulai`;
  - `saldo akhir = saldo awal + Σ mutasi dalam periode`;
  - dihitung hanya bila `Σ semua mutasi = saldo tersimpan`, dengan aturan cocok yang sama seperti pemeriksaan buku mutasi.
- **S2:** batch dan obat menjumlah semua status fisik. Perubahan status (−x status asal, +x status tujuan) saling meniadakan dan hanya dihitung jumlah transaksinya. Masuk = `StockIn`, keluar = `StockOut`, penyesuaian = `Adjustment`, saldo pembuka dicatat = `OpeningBalance` dalam periode.
- **S3:** ringkasan per obat dengan rincian batch (diurutkan kedaluwarsa).
- **S4:** satu saja kunci yang tidak cocok membuat saldo awal/akhir batch dan obat itu bernilai null, ditampilkan sebagai "Tidak dapat dihitung". Pergerakannya tetap tampil.
- **S5:** `LogisticsReportPeriod` yang sama dengan laporan lain.

**Aktivasi dan prasyarat UAT (tanpa migrasi).**

- Lokasi yang buku mutasinya sudah lolos L1–L2 dicatat operator di konfigurasi `PharmacyManagement:Logistics:LedgerVerifiedStorageLocations`, berupa id lokasi dipisah koma atau daftar anak. Variabel lingkungannya `PharmacyManagement__Logistics__LedgerVerifiedStorageLocations`.
- Selama lokasi belum tercantum, semua baris berstatus "Belum terverifikasi UAT". Layar menampilkan pembatasan "Saldo historis belum terverifikasi", dan keterbatasan pertama menyebut kunci konfigurasinya.
- **Saat ini belum ada lokasi yang terverifikasi.** UAT L1–L2 belum dijalankan, dan tidak ada klaim bahwa buku mutasi cocok.

**Frontend.** Halaman `/health-services/pharmacy-management/logistics-reports/balance-movement`, menu "Mutasi Saldo Persediaan" (grup Stok & Kartu).

- Pemilih lokasi wajib: laporan tidak memanggil backend sebelum lokasi dipilih, lewat opsi `isReady` di `useLogisticsReport`.
- Saringan tanggal, obat, cari, dan halaman.
- Kartu ringkasan per status, dan tabel per obat dengan tombol "n batch" yang membuka rincian batch.
- Tiga pemberitahuan: pilih lokasi, belum terverifikasi, buku tidak cocok.
- `formatHistoricalBalance` tidak pernah menampilkan angka untuk `NotComputable`.

## 4. FIN-LGS-057 — Rekomendasi Pengisian Ulang

**Endpoint:** `GET /api/v1/health-services/pharmacy-management/drug-stock/reports/replenishment`

| Atribut | Isi |
| --- | --- |
| Izin | `DrugStock:Read` (`SortOrder` 16). Hanya membaca |
| Query | `storageLocationId?`, `drugId?`, `search?`, `status?` (`Recommended` / `PolicyIncomplete` / `UnitMismatch`), `pageNumber`, `pageSize` |
| Kebijakan yang dibaca | `MstDrugStockPolicy` aktif, tidak terhapus, sedang berlaku |
| `Recommended` | Lokasi terdaftar dan aktif, `ReorderPointQuantity > 0`, `MaximumStockQuantity > ReorderPointQuantity`, satuan kebijakan = satuan stok obat, **dan** siap pakai ≤ titik pesan ulang. `recommendedQuantity = ⌈maksimum − siap pakai⌉` (R1, R4) |
| `PolicyIncomplete` | "Kebijakan belum lengkap"; `incompleteReasons`: `NoLocation`, `LocationInactive`, `ReorderPointNotSet`, `MaximumNotAboveReorderPoint`; `recommendedQuantity = null` |
| `UnitMismatch` | "Satuan kebijakan keliru" (R5): dikecualikan tanpa konversi; `recommendedQuantity = null` |
| Kebijakan valid di atas titik pesan ulang | Tidak ditampilkan; dihitung di `summary.aboveReorderPointCount` |
| `canCreateRequest` | Hanya `Recommended` pada lokasi yang **bukan** gudang induk (`IsMainWarehouse = false`), yaitu yang dipenuhi dari gudang internal (R2) |
| Data awal | `requestingServiceUnitId/Name` = unit layanan pemilik lokasi; `stockUnitMeasurementId/Name` = satuan stok obat |

"Siap pakai" = saldo fisik berstatus siap pakai, sama dengan layar Barang Hampir Habis, sehingga keduanya konsisten.

**Frontend.** Halaman `/health-services/pharmacy-management/drug-stock/replenishment`, menu "Rekomendasi Pengisian Ulang" (grup Pengendalian).

- Saringan lokasi, obat, status kebijakan, dan cari.
- Kolom: Obat, Lokasi (gudang induk / unit), Siap Pakai (+ditahan), Titik Pesan Ulang, Maksimum, Jumlah Usulan ("-" bila tidak dapat dihitung), Alasan, Aksi.

**Alur tombol "Buat Permintaan" (R3, tanpa pembuatan otomatis):**

1. Halaman rekomendasi menyimpan data awal yang sudah disaring ke `sessionStorage` dengan token acak 128-bit, berlaku 30 menit, lewat `stock-request-prefill-utils.js`.
2. Pengguna diarahkan ke formulir Pengajuan Kebutuhan Barang yang sudah ada, `…/stock-requests/create?prefill=<token>`. Id unit dan obat tidak tampil di alamat.
3. Formulir membaca dan menyaring ulang data awal:
   - UUID harus sah;
   - jumlah dalam batas `STOCK_REQUEST_LIMITS`;
   - paling banyak 20 baris;
   - teks dibersihkan.
4. Formulir mengisi unit peminta, obat, satuan, jumlah, dan catatan. Gudang pemenuhan tetap dipilih petugas.
5. Permintaan **baru dibuat saat petugas menekan simpan**, lewat `POST stock-requests` yang sama. Izin `StockRequest:Create`, validasi master obat/satuan, dan aturan kewenangan backend tetap berlaku; tidak ada jalan pintas otorisasi.
6. Formulir berdata awal dirender di klien saja (`useSyncExternalStore`), dan halaman create dibungkus `Suspense` untuk `useSearchParams`.

## 5. Route, endpoint, dan menu

| Jenis | Baru |
| --- | --- |
| Endpoint | `GET drug-stock/reports/balance-movement`, `GET drug-stock/reports/replenishment`; parameter baru `sourceDocumentTypes` pada `GET drug-stock/mutations` |
| Route | `/drug-stock/card/transfer-history`, `/logistics-reports/balance-movement`, `/drug-stock/replenishment` (semua di bawah `/health-services/pharmacy-management`) |
| Menu tertaut | +3: Riwayat Mutasi, Mutasi Saldo Persediaan, Rekomendasi Pengisian Ulang. **33 → 36** dari 66 yang dapat diklik (67 entri, tanpa entri baru) |
| Konfigurasi baru (opsional) | `PharmacyManagement:Logistics:LedgerVerifiedStorageLocations` |

## 6. Daftar file (paket perubahan lengkap, belum di-commit)

**Backend:** 11 berkas (8 diubah, 3 baru).

| Status | Berkas | Isi tahap ini |
| --- | --- | --- |
| Ubah | `Areas/HealthServices/PharmacyManagement/DTOs/DrugStockDtos.cs` | **Baru di paket:** `SourceDocumentTypes` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` | `IConfiguration?` opsional, `ParseSourceDocumentTypes`, filter banyak sumber, delegasi dua laporan |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` | `GetBalanceMovement`, `GetReplenishment` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` | (tahap sebelumnya) |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` | (tahap sebelumnya) |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` | (tahap sebelumnya) |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` | (tahap sebelumnya) |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | Bagian ini |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` | + DTO Mutasi Saldo dan Rekomendasi |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` | + `LogisticsLedgerVerification`, `GetBalanceMovementAsync`, `GetReplenishmentAsync`, `RequireLocationAsync` |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` | + 5 uji |

**Frontend:** 51 berkas (16 diubah, 35 baru). Tambahan tahap ini dibanding daftar 37 berkas sebelumnya:

| Status | Berkas |
| --- | --- |
| Ubah | `src/app/health-services/pharmacy-management/stock-requests/create/page.jsx` (Suspense) |
| Ubah | `src/app/health-services/pharmacy-management/stock-requests/create/stock-request-create-client.jsx` (token data awal) |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-form-view.jsx` (data awal + pemberitahuan) |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-request-form.jsx` (parameter `prefill`) |
| Ubah (sudah di daftar) | `drug-stock-card-view.jsx` (varian), `use-drug-stock.jsx` (`TRANSFER_HISTORY_FILTERS`), `drug-stock-constant.jsx`, `menu-items.jsx` (+3 `pathname`), `logistics-report-constant.js`, `logistics-report.service.js`, `use-logistics-report.jsx` (`isReady`), `logistics-report-utils.js`, `logistics-reports.test.mjs`, `logistics-module-navigation.test.mjs` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/card/transfer-history/{page.jsx, drug-stock-transfer-history-client.jsx}` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/replenishment/{page.jsx, drug-stock-replenishment-client.jsx}` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/balance-movement/{page.jsx, balance-movement-report-client.jsx}` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/{balance-movement-report-view, replenishment-view}.jsx` |
| Baru | `src/utils/health-services/pharmacy-management/stock-request-prefill-utils.js` |
| Baru | `tests/unit/logistics-v2-features.test.mjs` |

**Batas modul:** tidak ada berkas di luar `Areas/HealthServices/PharmacyManagement`, `LogisticsReportTests`, `docs/module-blueprints/logistik` (backend), serta `pharmacy-management`, bagian Logistik `menu-items.jsx`, dan `tests/unit/logistics-*` (frontend).

**Pengecualian commit tetap:** `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*`, `test-results/.last-run.json`.

## 7. Regression test yang ditambahkan

**Backend** (`LogisticsReportTests.cs`, 5 baru):

| Uji | Yang dibuktikan |
| --- | --- |
| `Riwayat_mutasi_menggabungkan_transfer_dan_retur_dari_kartu_stok_tanpa_sumber_lain` | Gabungan 2 sumber; normalisasi huruf/spasi; nilai tidak dikenal tidak melebarkan hasil; hanya-tidak-dikenal → kosong; gabungan dengan saringan tunggal; Kartu Stok tanpa saringan tetap memuat semua |
| `Mutasi_saldo_menghitung_saldo_awal_mundur_dan_saldo_akhir_untuk_buku_yang_cocok` | Saldo awal 50 → akhir 53 (masuk 20, keluar 15, penyesuaian −2, perubahan status netral); rincian batch; periode lampau dihitung mundur melewati mutasi sesudahnya (0 → 50, pembuka dicatat 50); tanpa verifikasi → `Unverified` + keterbatasan konfigurasi |
| `Mutasi_saldo_tidak_menghitung_saldo_historis_untuk_buku_yang_tidak_cocok` | Saldo tanpa mutasi dan jumlah tidak cocok → `NotComputable`, saldo awal/akhir null walaupun lokasi terverifikasi; pergerakan tetap; `PHM036`; `PHM035` |
| `Rekomendasi_pengisian_ulang_memakai_maksimum_dikurangi_siap_pakai_dibulatkan_ke_atas` | ⌈100 − 12,5⌉ = 88; tombol hanya untuk depo; gudang induk 45 tanpa tombol; di atas titik pesan ulang tidak tampil; unit dan satuan data awal; **tidak ada permintaan yang dibuat** |
| `Rekomendasi_pengisian_ulang_tidak_mengarang_angka_untuk_kebijakan_tidak_lengkap_atau_satuan_keliru` | Empat alasan "belum lengkap", satuan keliru, kebijakan nonaktif atau berakhir tidak dibaca, semua tanpa angka; saringan status; izin `DrugStock:Read` pada kedua endpoint baru |

**Frontend:**

- `tests/unit/logistics-v2-features.test.mjs` berisi 9 uji: menu dan route ketiga fitur, varian Riwayat Mutasi, endpoint, lokasi wajib, S4 tanpa angka, data awal hanya untuk baris yang boleh, penyaringan ulang data awal, formulir hanya untuk membuat + Suspense, dan label status/alasan.
- Empat asersi lama disesuaikan dengan perilaku yang disetujui: jumlah tertaut 36, `blocked`/`isReady` di hook, `config.sourceOptions`, dan Riwayat Mutasi kini tertaut.

## 8. Uji yang sudah dan belum dijalankan

| Uji | Status |
| --- | --- |
| Frontend `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs tests/unit/logistics-v2-features.test.mjs` | **64 lulus, 0 gagal, 0 dilewati** |
| ESLint, 50 berkas `.js/.jsx/.mjs` frontend yang berubah | **0 error**; 2 warning lama (`use-drug-stock.jsx:69`, `use-stock-request-list.jsx:109`, `set-state-in-effect` pada efek `load()` yang sudah ada) |
| Backend build | **Belum** (dilarang tahap ini). Pemeriksaan statis: kurung seimbang di 5 berkas C#; member, DbSet, dan navigasi dicocokkan dengan source (`MstDrugStockPolicies`, `IsMainWarehouse`, `ServiceUnitId`, `IsActive`, `StockUnitMeasurementId`, `PhmDrugBatches`); `Assert.Equal` dengan nilai nullable diberi cast eksplisit |
| Backend 149 kasus | **Belum dijalankan.** Hitungan dari source: 129 metode = 149 kasus (135 lama + 14 `LogisticsReportTests`) |
| Uji manual layar | Belum (dev server dilarang) |

**Perintah sekali jalan untuk pemilik proyek** (proyek terisolasi tidak perlu diubah; glob sudah mencakup uji baru):

```bash
cd "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests"   # atau buat ulang dari Lampiran C
dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1
dotnet test  StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run8.trx"
# Harapan: 0 error, tidak ada warning baru dari berkas Logistik; Total tests: 149, Passed: 149
```

## 9. Blocker yang masih terbuka

| Butir | Status |
| --- | --- |
| Build dan 149 kasus backend | Wajib sebelum commit |
| UAT buku mutasi L1–L2 per lokasi | Belum. Sampai lulus dan dicatat di konfigurasi, Mutasi Saldo Persediaan berstatus "Belum terverifikasi UAT" |
| Rekomendasi: permintaan yang sedang berjalan tidak dikurangkan | Sesuai R1 yang disetujui (bukan R1c); dinyatakan di keterbatasan |
| Gudang pemenuhan pada formulir dari rekomendasi | Dipilih petugas; belum ada pemetaan lokasi → gudang pemasok (butuh keputusan bila ingin otomatis) |
| Persetujuan atasan dan CITO | **BLOCKED_DATA** (mapping `OrganizationUnitId`, penempatan, izin, jabatan CITO, tenggat) |
| `LGS-OQ-027`/`029` | **BLOCKED_DECISION**, tidak diubah |
| Entri menu tersisa | 30 belum tertaut (66 − 36): sisa kelas B (Stok per Unit, Stok Lambat Bergerak, Pemeriksaan Stok, Lokasi Penyimpanan), kelas C (migrasi), kelas D |

## 10. Kesiapan integrasi dan UAT

**Integrasi:**

- Pemeriksaan 20 Nov menunjukkan 0 konflik dengan kedua branch integration.
- Berkas baru tahap ini tidak beririsan dengan perubahan integration (HR, Aset, Payroll), sehingga status itu tetap berlaku.
- Urutan: build dan uji 149 kasus, commit backend (11 berkas), commit frontend (51 berkas), PR backend, lalu PR frontend.

**UAT tambahan untuk ketiga fitur** (data uji di lingkungan yang disetujui):

| ID | Langkah | Harapan (PASS) |
| --- | --- | --- |
| M1 | Lakukan transfer dan retur pada obat uji, lalu buka Riwayat Mutasi | Hanya transfer dan retur tampil; saringan lokasi, obat, dan tanggal berfungsi; reset kembali ke transfer + retur; tidak ada tombol Koreksi |
| B1 | Buka Mutasi Saldo Persediaan tanpa memilih lokasi | Pemberitahuan "Pilih lokasi"; tidak ada permintaan jaringan |
| B2 | Pilih gudang uji yang belum terverifikasi | Angka tampil berstatus "Belum terverifikasi UAT" dan ada pemberitahuan pembatasan |
| B3 | Sesudah L1–L2 lulus, operator mencantumkan gudang di konfigurasi; buka ulang | Status "Valid"; saldo akhir periode berakhir hari ini = Saldo Saat Ini = Kartu Stok |
| B4 | Lokasi dengan temuan buku mutasi (bila ada) | "Tidak dapat dihitung" tanpa angka saldo awal/akhir; pemberitahuan merah |
| R1 | Kebijakan uji depo: titik pesan ulang 20, maksimum 100; saldo siap pakai 12,5 | Usulan 88; tombol "Buat Permintaan" membuka formulir berisi unit, obat, satuan, dan jumlah 88; permintaan baru ada sesudah ditekan simpan |
| R2 | Kebijakan gudang induk, titik pesan ulang 0, dan satuan keliru | Gudang induk tanpa tombol ("Diisi lewat pengadaan"); "Kebijakan belum lengkap" / "Satuan kebijakan keliru" tanpa angka |
| R3 | Akun tanpa `StockRequest:Create` menekan "Buat Permintaan" lalu simpan | Formulir terbuka, tetapi simpan ditolak backend (403) |
| R4 | Akun tanpa `DrugStock:Read` | Halaman rekomendasi dan Mutasi Saldo menampilkan "Akses Ditolak" |

---

# FINAL QUALITY GATE TIGA FITUR (FIN-LGS-059 s.d. FIN-LGS-062)

**Tanggal:** 20 November 2026
**Cakupan:** hanya perubahan FIN-LGS-055–058, dibaca dari working tree.
**Yang tidak dilakukan:** commit, push, PR, merge, `dotnet build`/`test`, `npm run build`/`dev`, migrasi, perubahan basis data, dan perubahan modul lain.
**Yang dijalankan:** uji unit frontend (Node) dan ESLint.

## 1. Status final ketiga fitur

| Fitur | Status source | Batas aktivasi |
| --- | --- | --- |
| Riwayat Mutasi (055) | **Selesai** | Aktif sesudah deploy; baca saja |
| Mutasi Saldo Persediaan (056) | **Selesai** | Halaman aktif. Saldo historis **tidak** berstatus valid sampai lokasi lolos UAT L1–L2 dan dicantumkan di `PharmacyManagement:Logistics:LedgerVerifiedStorageLocations`. Sebelum itu semua baris "Belum terverifikasi UAT" |
| Rekomendasi Pengisian Ulang (057) | **Selesai** | Aktif sesudah deploy. Tombol hanya mengisi formulir; simpan tetap dijaga izin dan validasi backend |

## 2. FIN-LGS-059 — Hasil pemeriksaan backend dan prefill

| Area | Pemeriksaan | Hasil |
| --- | --- | --- |
| Riwayat Mutasi | Filter gabungan `StockTransfer` + `PrescriptionReturn` | Benar. Kedua kode ditulis `StockTransferService` dan `DrugReturnService`. Penafsiran hanya menerima konstanta `DrugStockSourceDocumentTypes`; nilai tak dikenal tidak melebarkan hasil; filter berparameter (`IN`); batas 300 karakter dan 10 nilai; tanpa parameter, Kartu Stok tidak berubah |
| Mutasi Saldo | Saldo historis | S1 mundur dari saldo tersimpan; mutasi sesudah periode ikut dikurangkan; saldo akhir = awal + mutasi periode |
| | Ledger | Aturan cocok sama dengan pemeriksaan buku mutasi: kunci obat × batch × status, jumlah semua mutasi = saldo tersimpan. Tidak cocok → saldo awal/akhir **null** walaupun lokasi terverifikasi |
| | Status fisik | Semua status dijumlah; perubahan status saling meniadakan dan tidak masuk masuk/keluar |
| | Periode | `LogisticsReportPeriod` (30 hari, ≤ 366, `PHM035`) |
| | Pembatasan UAT | Status `Valid` hanya untuk lokasi di konfigurasi; lainnya `Unverified`, disertai keterbatasan yang menyebut kunci konfigurasinya |
| Rekomendasi | Rumus | `⌈maksimum − siap pakai⌉`, hanya bila siap pakai ≤ titik pesan ulang |
| | Validasi kebijakan | Aktif, berlaku, lokasi terdaftar dan aktif, titik pesan ulang > 0, maksimum > titik pesan ulang. Indeks unik PostgreSQL `(DrugId, StorageLocationId)` menjamin satu kebijakan hidup per lokasi, sehingga tidak ada usulan ganda |
| | Satuan | Satuan kebijakan ≠ satuan stok obat (atau obat tanpa satuan stok) → `UnitMismatch`, tanpa angka dan tanpa konversi |
| | Izin | `DrugStock:Read` pada endpoint; uji refleksi |
| Prefill | Validasi ulang | Disaring saat disimpan **dan** saat dibaca: UUID, batas jumlah formulir, ≤ 20 baris, teks dibersihkan, unit tidak sah dikosongkan |
| | Token sekali pakai | **Cacat ditemukan dan diperbaiki** (bagian 3, temuan 1) |
| | Tanpa pembuatan otomatis | Halaman rekomendasi tidak memanggil API tulis apa pun. Formulir hanya terisi; `POST stock-requests` hanya terjadi saat petugas menekan simpan, dengan izin `StockRequest:Create` dan validasi backend yang sama |
| Proyek uji terisolasi | Glob `LogisticsReportTests\*.cs` | Sudah ada; kelima uji baru otomatis tercakup |

## 3. Temuan dan perbaikan source

| No | Temuan | Dampak | Perbaikan |
| --- | --- | --- | --- |
| 1 | **Token data awal tidak sekali pakai.** `readStockRequestPrefill` membaca tanpa menghapus, sehingga token dapat dipakai ulang selama 30 menit (tab lain, muat ulang, riwayat peramban). Laporan sebelumnya menyebutnya sekali pakai | Formulir dapat terisi ulang dari data lama | Diganti `consumeStockRequestPrefill`: entri dihapus dari sessionStorage pada bacaan pertama. Hasilnya disimpan di memori halaman, supaya render ganda React tetap konsisten tanpa membuka pemakaian ulang dari penyimpanan. Uji baru memakai sessionStorage tiruan: hapus saat dipakai, bacaan ulang sama, kedaluwarsa → null, token tidak sah → null, data tidak sah tidak disimpan |
| 2 | **Angka "belum terverifikasi" bergaya sama dengan angka valid** di sel Saldo Awal/Akhir (tebal), walaupun badge dan pemberitahuan sudah ada | Pengguna dapat membaca angka itu sebagai saldo resmi | Hanya status `Valid` yang tampil tebal. `Unverified` tampil biasa dengan keterangan "belum terverifikasi" di sel yang sama; `NotComputable` tetap "Tidak dapat dihitung". Diasersi uji |
| 3 | **Data uji kebijakan tidak realistis.** Satu uji membuat lima kebijakan hidup untuk obat × lokasi yang sama, padahal PostgreSQL menolaknya (SQLite uji melepas keunikan indeks bersaring) | Uji tetap lulus, tetapi tidak mencerminkan data sungguhan | Setiap kebijakan memakai lokasinya sendiri |

Tidak ditemukan cacat lain pada perhitungan, kontrak API, izin, atau integritas transaksi (semua endpoint baru hanya membaca).

## 4. Kesiapan 149 kasus backend dan status frontend

| Uji | Status |
| --- | --- |
| Backend | **Belum dijalankan** (dilarang tahap ini). Hitungan dari source: **129 metode = 149 kasus**, terdiri dari 135 lama + 14 `LogisticsReportTests` (6 laporan, 3 buku mutasi, 5 fitur baru). Proyek terisolasi tidak perlu diubah. Pemeriksaan statis: kurung seimbang; member/DbSet/navigasi dicocokkan; `Assert.Equal` nullable diberi cast |
| Frontend | **65/65 lulus** (64 + 1 uji token sekali pakai) |
| ESLint | 50 berkas `.js/.jsx/.mjs` yang berubah: **0 error**, 2 warning lama (`set-state-in-effect` pada efek `load()` yang sudah ada) |
| Route baru | `/drug-stock/card/transfer-history`, `/logistics-reports/balance-movement`, `/drug-stock/replenishment`: `page.jsx` dan client ada, diasersi uji |
| Menu | **36** dari 66 tertaut (67 entri), diasersi uji |

**Prosedur eksekusi sekali jalan** (tanpa DB operasional, migrasi, atau server; SQLite dalam memori; `SkipMigrationMetadata=true`; foreground; satu proses):

```bash
# Folder proyek terisolasi; bila sudah terhapus, buat ulang dari Lampiran C (isi sama).
cd "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests"
dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1
dotnet test  StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run8.trx"
```

| ☐ | Harapan |
| --- | --- |
| ☐ | Build 0 error. Tidak ada warning baru dari `LogisticsReport*`, `DrugStock*`, atau `LogisticsReportTests` (pembanding 599 warning lama) |
| ☐ | `Total tests: 149`, `Passed: 149`, `Failed: 0`, `Skipped: 0`; TRX `notExecuted=0` |
| ☐ | Lima uji baru tercatat: `Riwayat_mutasi_…`, `Mutasi_saldo_menghitung_…`, `Mutasi_saldo_tidak_menghitung_…`, `Rekomendasi_…dibulatkan_ke_atas`, `Rekomendasi_…tidak_mengarang_angka_…` |
| ☐ | Bila ada yang gagal: perbaiki hanya source Logistik, lalu ulangi 149 kasus |
| ☐ | Frontend: `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-reports.test.mjs tests/unit/stock-request-approval.test.mjs tests/unit/logistics-module-navigation.test.mjs tests/unit/logistics-v2-features.test.mjs` → `pass 65` |

Belum ada klaim 149/149 PASS pada tahap ini. Hasil eksekusinya ada di bagian *EKSEKUSI FINAL TEST* (FIN-LGS-067 s.d. FIN-LGS-069) di akhir dokumen.

## 5. Blocker dan batas aktivasi

| Butir | Status |
| --- | --- |
| Build dan 149 kasus backend | Wajib sebelum commit |
| UAT buku mutasi L1–L2 | Belum dijalankan. Sampai lulus **dan** lokasi dicantumkan di konfigurasi, Mutasi Saldo Persediaan tidak menyatakan saldo historis valid |
| UAT tiga fitur (M1, B1–B4, R1–R4) | Siap sesudah deploy (*IMPLEMENTASI TIGA FITUR* 10) |
| Persetujuan atasan dan CITO | **BLOCKED_DATA** |
| `LGS-OQ-027`/`029` | **BLOCKED_DECISION** |
| Rekomendasi tidak mengurangkan permintaan berjalan | Sesuai R1 yang disetujui |
| Gudang pemenuhan pada formulir dari rekomendasi | Dipilih petugas (tidak ada pemetaan otomatis) |
| Produksi | **NO-GO** sampai UAT persetujuan lulus |

## 6. Daftar akhir untuk commit/push pemilik proyek

**Backend** (`Ikbal`), 11 berkas:

| Status | Berkas |
| --- | --- |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/DTOs/DrugStockDtos.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` |

**Frontend** (`Ikbalv2`), 51 berkas:

| Status | Berkas |
| --- | --- |
| Ubah | `src/app/health-services/pharmacy-management/stock-requests/create/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/stock-requests/create/stock-request-create-client.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/drug-return-view/drug-return-list-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-card-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/drug-stock-view/drug-stock-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-emergency-review-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-form-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-history-log-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-list-view.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-transfer-view/stock-transfer-list-view.jsx` |
| Ubah | `src/lib/constants/health-services/pharmacy-management/drug-stock-constant.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-drug-stock.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-request-form.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx` |
| Ubah | `src/utils/menu-sidebar/menu-items.jsx` |
| Ubah | `tests/unit/logistics-module-navigation.test.mjs` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/card/transfer-history/drug-stock-transfer-history-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/card/transfer-history/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/replenishment/drug-stock-replenishment-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/replenishment/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/balance-movement/balance-movement-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/balance-movement/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/distribution/distribution-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/distribution/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/flow-summary/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/flow-summary/stock-flow-dashboard-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/inventory/inventory-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/inventory/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/mutations/mutation-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/mutations/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/usage-by-unit/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/usage-by-unit/usage-by-unit-report-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/stock-requests/fulfillment/partial/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/stock-requests/fulfillment/partial/stock-request-partial-fulfillment-client.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/balance-movement-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/components/report-feedback.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/components/stock-flow-daily-chart.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/distribution-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/inventory-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/mutation-report-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/replenishment-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/stock-flow-dashboard-view.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/usage-by-unit-report-view.jsx` |
| Baru | `src/lib/constants/health-services/pharmacy-management/logistics-report-constant.js` |
| Baru | `src/lib/hooks/health-services/pharmacy-management/use-logistics-report.jsx` |
| Baru | `src/lib/services/health-services/pharmacy-management/logistics-report.service.js` |
| Baru | `src/style/health-services/pharmacy-management/logistics-report.module.css` |
| Baru | `src/utils/health-services/pharmacy-management/logistics-report-utils.js` |
| Baru | `src/utils/health-services/pharmacy-management/stock-request-prefill-utils.js` |
| Baru | `tests/unit/logistics-reports.test.mjs` |
| Baru | `tests/unit/logistics-v2-features.test.mjs` |

**Jangan ikut di-commit:** backend `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/`; frontend `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.

**Urutan:**

1. Prosedur bagian 4 (149/149 dan 65/65).
2. Commit backend (11 berkas).
3. Commit frontend (51 berkas).
4. Push.
5. PR backend ke `QuilvianIntegrationBackend`.
6. Sesudah merge, PR frontend ke `QuilvianIntegrationFrontend`.
7. Deploy, lalu UAT terbatas dan UAT tiga fitur.
8. UAT L1–L2 per lokasi, lalu operator mencantumkan lokasi yang lolos di konfigurasi.

---

# EKSEKUSI FINAL TEST (FIN-LGS-067 s.d. FIN-LGS-069)

**Tanggal:** 20 November 2026
**Cakupan:** menjalankan prosedur bagian 4 *FINAL QUALITY GATE TIGA FITUR* apa adanya, terhadap working tree yang sama (backend `Ikbal` HEAD `a3b2ed08`, frontend `Ikbalv2` HEAD `109a40831`, keduanya dengan perubahan lokal yang belum di-commit).
**Yang tidak dilakukan:** migrasi, pemakaian database operasional, server, `npm run build`/`dev`, deploy, commit, push, PR, merge, perubahan modul lain.
**Perubahan source pada tahap ini:** tidak ada. Semua uji lulus pada percobaan pertama, sehingga tidak ada perbaikan.

**Hasil:** **READY FOR OWNER COMMIT** untuk source pada saat itu. **Digantikan** oleh *PENYEDERHANAAN VALIDASI ATASAN* (FIN-LGS-070 s.d. FIN-LGS-073), yang mengubah source persetujuan dan memerlukan uji ulang.

## 1. FIN-LGS-067 — Build dan uji backend

Proyek uji terisolasi (isi sama dengan Lampiran C; glob `StockRequestApprovalTests\*.cs` dan `LogisticsReportTests\*.cs`), SQLite dalam memori, satu proses, foreground.

```bash
dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1
dotnet test  StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run8.trx"
```

| Langkah | Hasil sebenarnya |
| --- | --- |
| Build | **0 error**, 599 warning (sama dengan pembanding). Tidak ada warning dari `LogisticsReport*`, `DrugStock*`, `StockRequest*`, `DrugUsage*`, atau `LogisticsReportTests`. Warning di proyek uji hanya 5 peringatan kerentanan paket `SixLabors.ImageSharp` 3.1.11 (NU1902/NU1903) yang sudah ada sebelumnya. Waktu 1 menit 37 detik |
| Uji | **Total 149, Passed 149, Failed 0, Skipped 0.** Durasi 2 menit 24 detik |
| TRX `run8.trx` | `outcome=Completed`, `total=149`, `executed=149`, `passed=149`, `failed=0`, `notExecuted=0`, `error=0`, `timeout=0`. Mulai 14.26.20, selesai 14.28.47 WIB. 149 hasil, semuanya `Passed`; 129 metode unik |

Rincian per kelas uji (dibaca dari TRX):

| Kelas | Kasus |
| --- | --- |
| `LogisticsReportTests` | 14 |
| `StatusTransitionTests` | 18 |
| `ApprovalScopeTests` | 17 |
| `OperationalSettingsUnitTests` | 14 |
| `AuthorizationTests` | 13 |
| `EmergencyReviewTests` | 12 |
| `RobustnessAndAuditTests` | 12 |
| `CutoverUnitTests` | 11 |
| `IdempotencyReplayTests` | 9 |
| `HardeningTests` | 7 |
| `ConcurrencyTests` | 6 |
| `ProgressAndHistoryTests` | 6 |
| `ApiContractTests` | 5 |
| `DecisionLifecycleTests` | 5 |
| **Jumlah** | **149** |

Kelima uji fitur baru tercatat dijalankan dan lulus:

| Uji | Fitur | Hasil |
| --- | --- | --- |
| `Riwayat_mutasi_menggabungkan_transfer_dan_retur_dari_kartu_stok_tanpa_sumber_lain` | Riwayat Mutasi | Passed |
| `Mutasi_saldo_menghitung_saldo_awal_mundur_dan_saldo_akhir_untuk_buku_yang_cocok` | Mutasi Saldo Persediaan | Passed |
| `Mutasi_saldo_tidak_menghitung_saldo_historis_untuk_buku_yang_tidak_cocok` | Mutasi Saldo Persediaan | Passed |
| `Rekomendasi_pengisian_ulang_memakai_maksimum_dikurangi_siap_pakai_dibulatkan_ke_atas` | Rekomendasi Pengisian Ulang | Passed |
| `Rekomendasi_pengisian_ulang_tidak_mengarang_angka_untuk_kebijakan_tidak_lengkap_atau_satuan_keliru` | Rekomendasi Pengisian Ulang | Passed |

Sembilan uji `LogisticsReportTests` lain (6 laporan dan 3 pemeriksaan buku mutasi) serta 135 uji regresi persetujuan permintaan stok juga lulus.

## 2. FIN-LGS-068 — Frontend

| Pemeriksaan | Hasil sebenarnya |
| --- | --- |
| Uji unit (`node --test`, empat berkas uji Logistik) | **65/65 lulus**, 0 gagal, 0 dilewati, 0 dibatalkan |
| ESLint pada 50 berkas `.js/.jsx/.mjs` yang berubah | **0 error**, 2 warning lama `react-hooks/set-state-in-effect` (`use-drug-stock.jsx:69`, `use-stock-request-list.jsx:109`), pada efek `load()` yang sudah ada sebelum pekerjaan ini |
| Token data awal sekali pakai | Uji "FIN-LGS-059: token data awal benar-benar sekali pakai dan kedaluwarsa" lulus |
| Status saldo belum terverifikasi | Uji FIN-LGS-056 lulus: pemberitahuan "Saldo historis belum terverifikasi", keterangan "belum terverifikasi" di sel, label "Belum terverifikasi UAT", dan buku tidak cocok tidak pernah menampilkan angka |
| Tiga fitur | Uji FIN-LGS-055 (Riwayat Mutasi), FIN-LGS-056 (Mutasi Saldo), dan FIN-LGS-057 (Rekomendasi: tertaut, hanya membaca, data awal disaring ulang, formulir tetap lewat tombol simpan) lulus |
| Dev server, build produksi, deploy | Tidak dijalankan |

## 3. FIN-LGS-069 — Penutupan

| Butir | Status |
| --- | --- |
| Backend 149 kasus | **149/149 PASS** |
| Frontend 65 uji | **65/65 PASS** |
| ESLint | **0 error** |
| Perubahan source tahap ini | Tidak ada (hanya dokumen ini) |
| Berkas backend | 11, sama dengan daftar bagian 6 *FINAL QUALITY GATE TIGA FITUR*; dicocokkan dengan `git status` |
| Berkas frontend | 51, sama dengan daftar bagian 6 *FINAL QUALITY GATE TIGA FITUR*; dicocokkan dengan `git status` |
| Berkas milik pihak lain | Tidak tersentuh: `SeedData/ICD10/*` (2), `Storage/uploads/patient-qrcodes/00-00-00-03/`, `tests/e2e/zz-*.mjs`, `test-results/.last-run.json` |

**Status saat itu: READY FOR OWNER COMMIT** (digantikan FIN-LGS-070 s.d. FIN-LGS-073). Daftar berkas dan urutan commit/push ada di bagian 6 *FINAL QUALITY GATE TIGA FITUR*. Langkah 1 di urutan itu (prosedur 149/149 dan 65/65) sudah terpenuhi oleh tahap ini.

Yang tetap berlaku sesudah commit:

| Butir | Status |
| --- | --- |
| UAT buku mutasi L1–L2 | Belum dijalankan. Mutasi Saldo Persediaan tidak menyatakan saldo historis valid sampai lokasi lolos **dan** dicantumkan di `PharmacyManagement:Logistics:LedgerVerifiedStorageLocations` |
| UAT tiga fitur (M1, B1–B4, R1–R4) | Sesudah deploy |
| Persetujuan atasan dan CITO | **BLOCKED_DATA** |
| `LGS-OQ-027`/`029` | **BLOCKED_DECISION** |
| Produksi | **NO-GO** sampai UAT persetujuan lulus |

---

# PENYEDERHANAAN VALIDASI ATASAN (FIN-LGS-070 s.d. FIN-LGS-073)

**Tanggal:** 20 November 2026
**Dasar:** keputusan pemilik proyek bahwa hubungan HRD `MstServiceUnit.OrganizationUnitId` → `MstOrganizationUnit` → `WfpOrganizationAssignment.IsManagerialAssignment` tidak lagi dipakai untuk menentukan penyetuju permintaan Logistik. Penyetuju ditentukan izin yang sudah ada.
**Titik awal:** working tree lokal sesudah *EKSEKUSI FINAL TEST* (backend `Ikbal` `a3b2ed08` + perubahan lokal, frontend `Ikbalv2` `109a40831` + perubahan lokal).
**Yang tidak dilakukan:** `dotnet build`/`dotnet test`, `npm run build`/`dev`, migrasi, akses atau perubahan basis data, commit, push, PR, merge, perubahan HRD/MasterData/Payroll/modul lain.
**Yang dijalankan:** uji unit frontend (Node) dan ESLint. Backend hanya diperiksa statis.

**Status saat itu: READY FOR OWNER REVIEW** (digantikan *EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073* di akhir dokumen: 144/144, READY FOR OWNER COMMIT). Saat itu belum READY FOR OWNER COMMIT, karena build dan 144 kasus backend versi baru belum dijalankan (dilarang pada tahap ini). Hasil `run8.trx` (149/149) hanya berlaku untuk source sebelum tahap ini.

## 1. Mekanisme persetujuan setelah disederhanakan

```text
Petugas Unit                      izin StockRequest:Create (buat) dan StockRequest:Update (ajukan)
    ↓
Pengajuan Permintaan              status Menunggu Persetujuan
    ↓
Validasi Atasan                   pengguna dengan izin StockRequest:Approve, untuk unit mana pun,
                                  bukan pemohon; tolak wajib beralasan
    ↓
Pemeriksaan dan Pemenuhan Gudang  izin StockRequest:Fulfill; validasi stok, pemenuhan sebagian
    ↓
Barang Diserahkan                 saldo gudang berkurang, kartu stok tertulis

Jalur CITO: izin StockRequest:AuthorizeEmergency, bukan pemohon, alasan wajib → langsung dapat
diproses gudang → verifikasi susulan oleh pemegang StockRequest:Approve (bukan pemohon dan bukan
pengotorisasi CITO-nya).
```

| Keputusan | Syarat di backend (domain) | Bila tidak terpenuhi | Jejak |
| --- | --- | --- | --- |
| Setujui / Tolak | Bukan pemohon **dan** izin `StockRequest:Approve` eksplisit | `403`, dicatat `StockRequest.AuthorityDenied` | Riwayat `ApproveStockRequest`/`RejectStockRequest` + audit `StockRequest.Approve`/`Reject` |
| Otorisasi CITO | Bukan pemohon **dan** izin `StockRequest:AuthorizeEmergency` eksplisit; alasan wajib; hanya prioritas darurat | `403` / `422` (`PHM015`, `PHM017`) | Riwayat `AuthorizeEmergencyStockRequest` + audit |
| Verifikasi susulan CITO | Bukan pemohon, bukan pengotorisasi, **dan** izin `StockRequest:Approve` | `403` | Riwayat + audit; temuan juga sebagai peringatan |

- **Sumber izin.** Izin dibaca dari Akses Role, yaitu penempatan akses akun (`ApplicationUserOrganization`) × kebijakan akses (`SysAccessPolicy`). Penyaringnya sama dengan `AccessPermissionService.HasAccessAsync`.
- **Tanpa jalan pintas.** SuperAdmin, aksi system-only, dan saklar pengembangan tidak meloloskan pemeriksaan domain ini. Izin yang dicabut di Akses Role dan akun yang dinonaktifkan langsung tidak berlaku.
- **Yang tidak lagi dibaca:** tautan unit layanan ke unit organisasi, penempatan manajerial HRD, status profil tenaga kerja HRD, dan daftar jabatan otorisasi CITO.
- **Larangan memutus sendiri.** Aturannya tetap seperti sebelumnya: pembuat, pengaju, atau pemilik akun dari profil peminta tidak boleh memutus. Tautan akun ke profil peminta hanya dipakai untuk **mencegah**, tidak pernah untuk memberi kewenangan.
- **Cakupan "Dalam Kewenangan Saya" (`scope=1`).**
  - Pemegang `Approve` melihat permintaan seluruh unit yang sudah diajukan.
  - Pemegang izin CITO saja melihat permintaan CITO yang sudah diajukan.
  - Pengguna tanpa keduanya mendapat daftar kosong.
  - Draf tidak pernah masuk cakupan ini.
  - Cakupan ini hanya mempersempit daftar yang sudah dapat dibaca pemegang `StockRequest:Read`. Hak membaca, membuat, mengubah, dan membatalkan **tidak** diperluas (`LGS-OQ-027`/`029` tetap terbuka).

## 2. Perubahan backend (FIN-LGS-070, FIN-LGS-071)

| Berkas | Perubahan |
| --- | --- |
| `Services/StockRequestApprovalAuthority.cs` | Ditulis ulang. Hanya membaca izin aktif. Dihapus: `CheckUnitAuthorityAsync`, `GetSupervisedServiceUnitsAsync`, `ResolveDesignatedPositionIdsAsync`, penunjukan jabatan, serta tipe `StockRequestUnitAuthority*` dan `StockRequestSupervisedUnit`. CITO = izin `AuthorizeEmergency` eksplisit. `CountPolicyGrantsAsync` tanpa saringan jabatan |
| `Services/StockRequestOperationalSettings.cs` | `StockRequestEmergencyAuthorizerDesignation` dihapus. Tenggat verifikasi susulan tetap |
| `Services/StockRequestService.cs` | Konstruktor tanpa penunjukan jabatan. `EvaluateDeciderAsync` = bukan pemohon + `Approve`. `EvaluateEmergencyAuthorizerAsync` = bukan pemohon + `AuthorizeEmergency`. `EvaluateEmergencyVerifierAsync` = bukan pemohon + bukan pengotorisasi + `Approve`. Cakupan `MyAuthority` sesuai bagian 1. `GetApprovalScopeAsync` dan `GetApprovalReadinessAsync` tanpa data HRD. Detail tanpa penanda atasan unit. Teks audit dan pesan "atasan unit" diganti "penyetuju" |
| `Services/StockRequestApprovalCutover.cs` | Teks pesan saja ("wajib disetujui penyetuju") |
| `DTOs/StockRequestDtos.cs` | Kontrak di bawah; dokumentasi diselaraskan |
| `Controllers/StockRequestController.cs` | Dokumentasi Swagger approve/reject/authorize-emergency/verify-emergency/approval-scope/approval-readiness. Deskripsi izin `Approve` menjadi "Menyetujui atau menolak permintaan stok unit"; deskripsi `ViewApprovalReadiness` menjadi "Melihat kesiapan izin persetujuan permintaan stok". Route, izin, dan kode HTTP tidak berubah |

**Perubahan kontrak API.** Semua endpoint, method, izin, dan kode galat tetap sama.

| Endpoint | Perubahan |
| --- | --- |
| `GET stock-requests/{id}` | Field `hasUnitAuthority` dan `unitAuthorityMessage` dihapus. `hasApproveAuthority`, `hasEmergencyAuthority`, `isOwnRequest`, `canVerifyEmergency`, `verifyEmergencyMessage` tetap |
| `GET stock-requests/approval-scope` | Field `emergencyDesignationConfigured` dan `supervisedUnits` dihapus. `hasApprovePermission`, `hasEmergencyAuthority`, `message` tetap |
| `GET stock-requests/approval-readiness` | Dihapus: `activeServiceUnits`, `mappedServiceUnits`, `mappedServiceUnitsWithSupervisor`, `mappedServiceUnitsWithAuthorizedSupervisor`, `activeManagerialAssignments`, `emergencyDesignatedPositions`, `emergencyDesignatedPositionsWithGrant`, `units`. Ditambah: `approveUsers`, `emergencyUsers` (akun aktif yang benar-benar memegang izin). `isReadyForApproval` = `approveUsers > 0`; `isReadyForEmergency` = `emergencyUsers > 0` |
| `GET stock-requests?scope=1`, `histories?scope=1`, `reports/distribution?scope=1` | Makna cakupan sesuai bagian 1 |

**Konfigurasi dan data.**

- Kunci `PharmacyManagement:StockRequest:EmergencyAuthorizerPositions` tidak lagi dibaca. Bila sudah diisi di lingkungan mana pun, aman untuk dihapus.
- Tidak ada tabel, kolom, atau migrasi baru.
- `AccessMenuSeeder` yang sudah ada memperbarui teks deskripsi dua aksi izin di atas saat aplikasi start sesudah deploy. Ini hanya teks, bukan skema.

**Yang dipertahankan tanpa perubahan logika** (FIN-LGS-070 butir 10):

- transisi status, keputusan hanya sekali (`PHM018`), dan token versi (`PHM012`);
- idempotensi (`PHM013`);
- alasan wajib (`PHM015`);
- penyerahan hanya untuk status Disetujui (`PHM016`), validasi stok, dan pemenuhan sebagian;
- pembatalan, aturan peralihan, tenggat verifikasi susulan, riwayat, dan log audit.

## 3. Perubahan frontend (FIN-LGS-071)

| Berkas | Perubahan |
| --- | --- |
| `utils/.../stock-request-approval-utils.js` | Tombol Setujui/Tolak = izin di layar + `hasApproveAuthority === true` dari backend; syarat `hasUnitAuthority` dihapus. Sebab tombol tidak tersedia memakai pesan izin. **Tanpa perubahan ini, tombol Setujui akan selalu tersembunyi**, karena backend tidak lagi mengirim penanda unit |
| `components/stock-request-decision-authority-info.jsx` | Lencana "Atasan unit peminta" dan pesan unit dihapus; ditambah keterangan bahwa pemegang izin dapat memutus permintaan unit mana pun kecuali miliknya sendiri |
| `components/stock-request-authority-info.jsx` | Daftar "Unit yang Saya Pimpin" diganti baris "Cakupan Keputusan" yang diturunkan dari dua izin |
| `constants/.../stock-request-constant.jsx` | Label cakupan "Dalam Kewenangan Saya"; label riwayat "Disetujui"/"Ditolak" |
| `stock-request-list-view.jsx`, `stock-request-emergency-review-view.jsx`, `stock-request-detail-view.jsx`, `components/stock-request-submit-panel.jsx` | Teks yang menyebut "atasan unit peminta" diganti teks sesuai aturan baru |
| `components/stock-request-decision-panel.jsx`, `components/stock-request-attention-strip.jsx`, `hooks/use-stock-request-approval-scope.jsx`, `hooks/use-stock-request-approval-summary.jsx`, `hooks/use-stock-request-list.jsx` | Komentar saja |
| `tests/unit/stock-request-approval.test.mjs` | Uji aturan atasan unit diganti uji aturan izin saja, termasuk bahwa penanda unit dari backend lama diabaikan |

Yang sudah ada dan tetap dipakai:

- submenu **"Validasi Atasan"** (nama tidak berubah), dibuka pada status Menunggu Persetujuan;
- tombol Setujui dan Tolak dengan konfirmasi;
- alasan penolakan wajib, ditampilkan di kartu alasan;
- status, riwayat keputusan beserta nama pelaku, dan ringkasan.

Aksi yang tidak berwenang disembunyikan, dan sebabnya dijelaskan. Tidak ada halaman pengaturan approver.

## 4. FIN-LGS-072 — Cakupan Logistik

- Menu tidak diubah: 11 grup dan 67 entri. Dari jumlah itu 66 dapat diklik dan 36 sudah tertaut ke halaman (diasersi uji navigasi).
- Tidak ada route, submenu, atau fitur yang dihapus atau diganti nama.
- Laporan, kartu stok, riwayat mutasi, mutasi saldo persediaan, dan rekomendasi pengisian ulang tidak disentuh.

## 5. FIN-LGS-073 — Hasil pemeriksaan dan batas verifikasi

| Alur | Pemeriksaan (statis) | Hasil |
| --- | --- | --- |
| Pengajuan permintaan | `CreateAsync`/`SubmitAsync` | Logika tidak berubah (hanya teks audit) |
| Validasi dan penolakan | `DecideAsync` | Urutan pemeriksaan tetap: alasan → idempotensi → keadaan → versi → kewenangan. Kewenangan = bukan pemohon + `Approve` |
| Otorisasi CITO | `AuthorizeEmergencyAsync` | Izin khusus, alasan wajib, hanya darurat, larangan diri sendiri, lalu verifikasi susulan tetap wajib |
| Pemeriksaan stok, pemenuhan sebagian, distribusi | `FulfillAsync` | Logika tidak berubah (hanya teks pesan `PHM016`) |
| Riwayat permintaan | Riwayat dan linimasa | Tidak berubah; cakupan linimasa = cakupan daftar (diuji) |
| Audit trail | `DeniedAsync`, `AuditAsync` | Tetap mencatat pelaku, aksi, dan alasan penolakan kewenangan |
| Hak akses | `[AccessPermission]` controller | Tidak berubah; domain tetap memeriksa ulang tanpa jalan pintas SuperAdmin |
| Sisa rujukan | Seluruh repo backend (`*.cs`) | Tidak ada rujukan ke simbol yang dihapus. Kurung seimbang di 21 berkas yang diubah. Pemanggilan `Layanan(...)` di uji tidak memakai argumen posisional yang bergeser |

**Uji backend yang diperbarui** (proyek terisolasi, glob tidak berubah):

| Kelas | Sebelum | Sesudah | Isi perubahan |
| --- | --- | --- | --- |
| `AuthorizationTests` | 13 | 13 | Lintas unit kini **boleh**; penempatan manajerial berakhir dan profil HRD nonaktif tidak lagi menentukan; unit tanpa tautan dapat diputus; izin CITO saja tidak dapat menyetujui |
| `ApprovalScopeTests` | 17 | 14 | Cakupan seluruh unit tanpa draf; cakupan sama untuk setiap penyetuju; kesiapan dari akun berizin. Tiga uji penunjukan jabatan CITO dihapus |
| `RobustnessAndAuditTests` | 12 | 12 | Unit organisasi HRD tidak berlaku tidak memengaruhi; izin yang dicabut langsung tidak berlaku; kesiapan menghitung akun aktif |
| `EmergencyReviewTests` | 12 | 12 | Penyetuju unit lain kini boleh memverifikasi |
| `OperationalSettingsUnitTests` | 14 | 12 | Dua uji parser penunjukan jabatan dihapus |
| `ProgressAndHistoryTests` | 6 | 6 | Linimasa cakupan seluruh unit tanpa draf |
| `LogisticsReportTests` | 14 | 14 | Cakupan laporan distribusi mengikuti aturan baru |
| Kelas lain | 61 | 61 | Tidak diubah |
| **Jumlah** | **149** | **144** | 122 metode (sebelumnya 129) |

Harness uji: `StafUserId` kini staf gudang tanpa izin persetujuan; parameter `jabatanCito` dihapus; ditambah `CabutIzinAsync`. Data HRD sengaja tetap disemai untuk membuktikan bahwa data itu tidak berpengaruh.

| Uji | Status |
| --- | --- |
| Backend | Saat itu belum dijalankan (`dotnet build` dilarang). Hasil sebenarnya: **144/144 lulus**, lihat *EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073* |
| Frontend | **65/65 lulus** (uji diganti, bukan ditambah) |
| ESLint | 61 berkas: **0 error**, 4 warning `react-hooks/set-state-in-effect` yang sudah ada sebelumnya. Dua di antaranya baru ikut terperiksa karena berkas hook-nya berubah, padahal yang berubah hanya komentar |
| Uji manual | **NOT FEASIBLE**: dev server dan deploy dilarang pada tahap ini |

**Prosedur untuk pemilik proyek** (tanpa DB operasional, migrasi, atau server):

```bash
cd "%LOCALAPPDATA%\Temp\claude\<sesi>\scratchpad\srq-isolated-tests"
dotnet build StockRequestApprovalIsolated.Tests.csproj -p:SkipMigrationMetadata=true -m:1
dotnet test  StockRequestApprovalIsolated.Tests.csproj --no-build --logger "trx;LogFileName=run9.trx"
```

Harapan: build 0 error dan tidak ada warning baru dari berkas Logistik; `Total tests: 144`, `Passed: 144`, `Failed: 0`, `Skipped: 0`. Bila ada yang gagal, perbaiki hanya source Logistik lalu ulangi.

## 6. Blocker yang tersisa

| Butir | Status |
| --- | --- |
| Build dan 144 kasus backend versi baru | Wajib dijalankan pemilik proyek sebelum commit |
| Pemberian izin | Operasional, bukan kode. Berikan `StockRequest:Approve` dan `StockRequest:AuthorizeEmergency` lewat Akses Role kepada jabatan yang tepat; endpoint kesiapan menghitung akun yang benar-benar memegangnya |
| UAT alur persetujuan baru | Sesudah deploy: setuju, tolak, lintas unit, larangan diri sendiri, CITO, dan verifikasi susulan |
| Tenggat verifikasi susulan CITO | Konfigurasi belum diisi; tidak menghalangi keputusan |
| `LGS-OQ-027`/`029` | **BLOCKED_DECISION**. Hanya kewenangan memutus yang lintas unit; hak baca/buat/ubah/batal tidak diperluas |
| UAT buku mutasi L1–L2 | Belum, untuk saldo historis Mutasi Saldo Persediaan |
| Produksi | **NO-GO** sampai UAT persetujuan lulus |
| ~~Persetujuan atasan dan CITO BLOCKED_DATA~~ | **Selesai karena keputusan ini.** Tidak ada lagi ketergantungan pada pemetaan `OrganizationUnitId`, penempatan manajerial, atau penunjukan jabatan CITO |

## 7. Daftar berkas untuk review dan commit pemilik proyek

**Backend** (`Ikbal`), 22 berkas saat itu; daftar final 23 berkas ada di *EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073*. Sebelas di antaranya baru berubah di tahap ini:

| Status | Berkas | Tahap |
| --- | --- | --- |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` | 024–058 |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` | 024–058 |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` | 024–058, **070** |
| Ubah | `Areas/HealthServices/PharmacyManagement/DTOs/DrugStockDtos.cs` | 024–058 |
| Ubah | `Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs` | **070** |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` | 024–058 |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` | 024–058 |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalAuthority.cs` | **070** |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalCutover.cs` | **070** |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestOperationalSettings.cs` | **070** |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` | 024–058, **070** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs` | **073** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApprovalScopeTests.cs` | **073** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/AuthorizationTests.cs` | **073** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/EmergencyReviewTests.cs` | **073** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/OperationalSettingsUnitTests.cs` | **073** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ProgressAndHistoryTests.cs` | **073** |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/RobustnessAndAuditTests.cs` | **073** |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | semua |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` | 024–058 |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` | 024–058 |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` | 024–058, **073** |

**Frontend** (`Ikbalv2`), 62 berkas: 51 berkas pada bagian 6 *FINAL QUALITY GATE TIGA FITUR*, ditambah 11 berkas ini:

| Status | Berkas |
| --- | --- |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-attention-strip.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-authority-info.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-authority-info.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-panel.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-submit-panel.jsx` |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/stock-request-detail-view.jsx` |
| Ubah | `src/lib/constants/health-services/pharmacy-management/stock-request-constant.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-scope.jsx` |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-summary.jsx` |
| Ubah | `src/utils/health-services/pharmacy-management/stock-request-approval-utils.js` |
| Ubah | `tests/unit/stock-request-approval.test.mjs` |

Tiga berkas dari daftar 51 ikut diubah lagi di tahap ini: `stock-request-list-view.jsx`, `stock-request-emergency-review-view.jsx`, `use-stock-request-list.jsx`.

**Jangan ikut di-commit:**

- backend: `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/`;
- frontend: `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.

**Urutan:**

1. Review perubahan.
2. Jalankan prosedur bagian 5 sampai 144/144 dan 65/65.
3. Commit backend, lalu frontend, lalu push.
4. Buat PR backend, lalu PR frontend.
5. Deploy.
6. Berikan izin lewat Akses Role.
7. Jalankan UAT alur persetujuan.

---

# EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073

**Tanggal:** 23 November 2026
**Dasar:** izin eksplisit pemilik proyek untuk menjalankan `dotnet build` dan `dotnet test` backend pada proyek uji terisolasi, khusus pekerjaan Logistik ini.
**Yang tidak dilakukan:** migrasi, akses basis data operasional, server aplikasi, perubahan modul lain, commit, push, PR.

**Status saat itu: READY FOR OWNER COMMIT.** Untuk backend tetap berlaku; frontend berubah lagi pada *PENYEDERHANAAN NAVIGASI LOGISTIK (FIN-LGS-074 s.d. FIN-LGS-078)* dan menunggu review pemilik.

## 1. Hasil

Proyek uji terisolasi (isi sama dengan Lampiran C), SQLite dalam memori, satu proses, foreground.

| Langkah | Hasil sebenarnya |
| --- | --- |
| Build pertama (`build10.log`) | **0 error**, 599 warning (sama dengan pembanding). Tidak ada warning dari berkas Logistik yang berubah. Warning proyek uji hanya 5 peringatan paket `SixLabors.ImageSharp` yang sudah ada |
| Uji pertama (`run9.trx`) | 143 lulus, **1 gagal**: `ApiContractTests.Respons_detail_memakai_nama_properti_yang_dibaca_frontend` — "properti hasUnitAuthority harus ada" |
| Perbaikan | Hanya berkas uji Logistik (bagian 2); source aplikasi tidak diubah |
| Build kedua (`build11.log`) | **0 error**. Inkremental: hanya proyek uji yang dikompilasi ulang |
| Uji kedua (`run10.trx`) | **Total 144, Passed 144, Failed 0, Skipped 0.** TRX: `outcome=Completed`, `executed=144`, `notExecuted=0`, `error=0`, `timeout=0`; 122 metode. Mulai 10.01.58, selesai 10.04.16 WIB |
| Frontend | **65/65 lulus**; source frontend tidak berubah sejak tahap FIN-LGS-070 |

Rincian per kelas (`run10.trx`): `StatusTransitionTests` 18, `ApprovalScopeTests` 14, `LogisticsReportTests` 14, `AuthorizationTests` 13, `EmergencyReviewTests` 12, `OperationalSettingsUnitTests` 12, `RobustnessAndAuditTests` 12, `CutoverUnitTests` 11, `IdempotencyReplayTests` 9, `HardeningTests` 7, `ConcurrencyTests` 6, `ProgressAndHistoryTests` 6, `ApiContractTests` 5, `DecisionLifecycleTests` 5. Jumlah **144**.

Seluruh uji yang ditulis atau diubah pada FIN-LGS-073 tercatat dijalankan dan lulus, antara lain:

- `Kasus2_penyetuju_dapat_memutus_permintaan_lintas_unit`;
- `Penempatan_dan_profil_HRD_tidak_lagi_menentukan_kewenangan` (2 kasus);
- `Unit_layanan_tanpa_tautan_organisasi_tetap_dapat_diputus_penyetuju`;
- `Izin_CITO_saja_tidak_memberi_kewenangan_menyetujui_atau_menolak`;
- `Kasus6_CITO_dengan_izin_eksplisit_tanpa_penunjukan_jabatan_dapat_segera_diproses`;
- `Penyetuju_melihat_permintaan_seluruh_unit_yang_sudah_diajukan_tanpa_draf`;
- `Cakupan_penyetuju_sama_tanpa_melihat_penempatan_HRD`;
- `Unit_organisasi_HRD_yang_tidak_berlaku_tidak_memengaruhi_kewenangan_penyetuju` (2 kasus);
- `Izin_yang_dicabut_di_Akses_Role_langsung_tidak_berlaku`;
- `Kesiapan_menghitung_akun_aktif_yang_benar_benar_memegang_izin`;
- `Kesiapan_tidak_menyatakan_siap_bila_tidak_ada_akun_aktif_berizin`.

Uji regresi larangan memutus sendiri, SuperAdmin tanpa izin, CITO, verifikasi susulan, idempotensi, konkurensi, transisi status, pemenuhan sebagian, dan laporan Logistik juga lulus.

## 2. Temuan dan perbaikan

| Temuan | Sebab | Perbaikan |
| --- | --- | --- |
| Uji kontrak respons detail masih mewajibkan `hasUnitAuthority` dan `unitAuthorityMessage` | Kedua field sengaja dihapus pada FIN-LGS-070. Pemeriksaan statis tahap itu mencari nama properti C# (`HasUnitAuthority`), sedangkan uji ini memakai nama JSON camelCase dalam string, sehingga tidak tertangkap | `ApiContractTests.cs`: kedua nama dikeluarkan dari daftar wajib, dan uji kini memastikan keduanya **tidak ada** lagi di respons. Pencarian ulang nama JSON camelCase seluruh field yang dihapus di backend: tidak ada sisa |

Tidak ada cacat pada source aplikasi.

## 3. Daftar akhir untuk commit/push pemilik proyek

**Backend** (`Ikbal`), 23 berkas:

| Status | Berkas |
| --- | --- |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugUsageController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/StockRequestController.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/DTOs/DrugStockDtos.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/DTOs/StockRequestDtos.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugUsageService.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalAuthority.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestApprovalCutover.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestOperationalSettings.cs` |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockRequestService.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/StockRequestApprovalHarness.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApiContractTests.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ApprovalScopeTests.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/AuthorizationTests.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/EmergencyReviewTests.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/OperationalSettingsUnitTests.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/ProgressAndHistoryTests.cs` |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/StockRequestApprovalTests/RobustnessAndAuditTests.cs` |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/LogisticsReportDtos.cs` |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/LogisticsReportQueries.cs` |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/LogisticsReportTests.cs` |

**Frontend** (`Ikbalv2`), 62 berkas:

- 51 berkas pada bagian 6 *FINAL QUALITY GATE TIGA FITUR*;
- ditambah 11 berkas pada bagian 7 *PENYEDERHANAAN VALIDASI ATASAN*.

Jumlah itu sudah dicocokkan dengan `git status`.

**Jangan ikut di-commit:**

- backend: `SeedData/ICD10/MstDiagnosis.csv`, `SeedData/ICD10/MstDiagnosisChapter.csv`, `Storage/uploads/patient-qrcodes/00-00-00-03/`;
- frontend: `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.

**Urutan:**

1. Commit backend (23 berkas).
2. Commit frontend (62 berkas).
3. Push.
4. PR backend ke `QuilvianIntegrationBackend`.
5. Sesudah merge, PR frontend ke `QuilvianIntegrationFrontend`.
6. Deploy.
7. Berikan `StockRequest:Approve` dan `StockRequest:AuthorizeEmergency` lewat Akses Role.
8. UAT alur persetujuan: setuju, tolak, lintas unit, larangan diri sendiri, CITO, verifikasi susulan.
9. UAT L1–L2 per lokasi untuk saldo historis.

## 4. Yang tetap berlaku sesudah commit

| Butir | Status |
| --- | --- |
| Pemberian izin di Akses Role | Operasional, belum dilakukan |
| UAT alur persetujuan baru | Sesudah deploy |
| Tenggat verifikasi susulan CITO | Konfigurasi belum diisi; tidak menghalangi keputusan |
| `LGS-OQ-027`/`029` | **BLOCKED_DECISION**; hak baca/buat/ubah/batal lintas unit tidak diperluas |
| UAT buku mutasi L1–L2 | Belum; saldo historis Mutasi Saldo Persediaan tetap "Belum terverifikasi UAT" |
| Produksi | **NO-GO** sampai UAT persetujuan lulus |

---

# PENYEDERHANAAN NAVIGASI LOGISTIK (FIN-LGS-074 s.d. FIN-LGS-078)

**Tanggal:** 23 November 2026
**Target:** dari 11 menu / 67 entri (66 dapat diklik) menjadi **6 menu / 20 submenu**, tanpa membuang fungsi.
**Titik awal:** working tree sesudah *EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073*.
**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, akses basis data, `dotnet build`, `npm run build`/`dev`, perubahan HRD/MasterData/Keuangan/modul lain.
**Yang dijalankan:** uji unit frontend (Node) dan ESLint.

**Status: READY FOR OWNER REVIEW.**

## 1. Struktur 6 menu dan 20 submenu

Pendekatan: submenu menunjuk route yang sudah ada. Fungsi yang berkaitan digabung dengan **tab** (`LogisticsWorkspace`), yang merender layar existing apa adanya. Hanya tab yang tampil yang memanggil backend, dan tab tidak mengubah URL, karena sidebar hanya mengenal satu alamat per submenu. Route baru hanya tiga, untuk fungsi gabungan yang belum punya alamat.

| No | Submenu | Route | Isi |
| --- | --- | --- | --- |
| 1.1 | Ringkasan Persediaan | `/drug-stock/summary` | Tab: Ringkasan Stok · Hampir Habis · Mendekati Kedaluwarsa (baca saja) |
| 2.1 | Katalog Barang | `/master-data/drug` | Layar master existing |
| 2.2 | Kategori & Satuan | **baru** `/logistics-master/categories-units` | Tab: Kategori Barang · Satuan & Konversi (layar master existing, tidak diubah) |
| 2.3 | Gudang & Lokasi | `/master-data/drug-storage-locations` | Layar master existing (rak/shelf/bin, suhu, kelembapan, cold chain, karantina) |
| 3.1 | Pengajuan Barang | `/stock-requests` | Daftar permintaan + tombol Buat Permintaan, draf, ajukan |
| 3.2 | Persetujuan Permintaan | `/stock-requests/approvals` | Validasi Atasan existing (FIN-LGS-070) + tombol Verifikasi Susulan CITO |
| 3.3 | Pemenuhan & Distribusi | `/stock-requests/approved` | Tab: Siap Diserahkan · Status Pemenuhan · Pemenuhan Sebagian |
| 3.4 | Riwayat Permintaan | `/stock-requests/history` | Linimasa aktivitas existing |
| 4.1 | Penerimaan Barang | `/finance/purchasing/goods-receipts` | Halaman Keuangan, izin `FinanceGoodsReceipt:Read` (tidak berubah) |
| 4.2 | Penyimpanan & Penempatan | `/drug-stock/locations` | Tab: Lokasi Barang · Barang Rusak (baca saja) |
| 4.3 | Mutasi & Retur | `/stock-transfers` | Tab: Transfer Antar Lokasi · Retur dari Unit · Riwayat Mutasi · Retur ke Penyedia (tautan Keuangan, hanya bila izin `FinanceSupplierReturn:Read` sudah pasti ada) |
| 4.4 | Stock Opname | — (nonaktif) | Belum tersedia, lihat bagian 6 |
| 5.1 | Stok & Kartu Persediaan | `/drug-stock` | Tab: Stok per Batch · Stok per Item · Kartu Stok (baca saja) |
| 5.2 | Pengendalian Stok | `/drug-stock/replenishment` | Tab: Stok Minimal/Maksimal · Pantau Stok · Rekomendasi Pengisian Ulang (tab bawaan) |
| 5.3 | Batch & Kadaluarsa | `/drug-stock/batches` | Tab: Daftar Batch · Kedaluwarsa (baca saja) |
| 5.4 | Penyesuaian Stok | **baru** `/drug-stock/adjustments` | Tab: Penyesuaian per Batch (Kelola: jumlah dan status; Catat Saldo Pembuka) · Koreksi Kartu Stok |
| 6.1 | Laporan Persediaan | `/logistics-reports/inventory` | Tab: Posisi Persediaan · Mutasi Saldo Persediaan |
| 6.2 | Laporan Barang Masuk & Keluar | `/logistics-reports/flow-summary` | Tab: Penerimaan & Pengeluaran · Distribusi ke Unit |
| 6.3 | Laporan Pemakaian & Mutasi | `/logistics-reports/usage-by-unit` | Tab: Pemakaian per Unit · Mutasi Persediaan |
| 6.4 | Rekonsiliasi | **baru** `/logistics-reports/reconciliation` | Rekonsiliasi saldo tersimpan × buku mutasi per batch × status (endpoint existing `reports/ledger-consistency`) |

Semua route di atas berawalan `/health-services/pharmacy-management`, kecuali yang ditulis lengkap.

**Pemisahan melihat dan mengubah.** Semua perintah yang mengubah stok (Kelola, Catat Saldo Pembuka, Koreksi) kini hanya ada di 5.4 Penyesuaian Stok. Layar stok lain hanya membaca. Izin backend `DrugStock:Adjust`/`Correct` tidak berubah.

## 2. Pemetaan fungsi lama ke struktur baru (FIN-LGS-074)

Kategori: **TERSEDIA**, **DISESUAIKAN** (digabung atau dipindah tanpa mengubah fungsinya), **BELUM TERSEDIA**, dan **KEPUTUSAN** (memerlukan keputusan bisnis).

| Menu lama | Entri lama | Lokasi baru | Kategori |
| --- | --- | --- | --- |
| Dashboard | Ringkasan Stok | 1.1 tab Ringkasan Stok; juga 5.1 Stok per Item | TERSEDIA |
| | Barang Hampir Habis | 1.1 tab Hampir Habis; juga 5.2 Pantau Stok | TERSEDIA |
| | Permintaan Unit | 3.1 Pengajuan Barang (daftar permintaan) | DISESUAIKAN |
| | Penerimaan & Pengeluaran | 6.2 tab Penerimaan & Pengeluaran | DISESUAIKAN |
| | Barang Mendekati Kadaluarsa | 1.1 tab Mendekati Kedaluwarsa; juga 5.3 | TERSEDIA |
| Master | Katalog Barang | 2.1 | TERSEDIA |
| | Kategori Barang | 2.2 tab | DISESUAIKAN |
| | Satuan & Konversi | 2.2 tab | DISESUAIKAN |
| | Daftar Gudang | 2.3 | TERSEDIA |
| | Lokasi Penyimpanan | 2.3 (sama dengan Daftar Gudang sejak awal) | TERSEDIA |
| | Penanggung Jawab Gudang | — | **KEPUTUSAN** (master lokasi tidak punya kolom penanggung jawab; perlu skema atau aturan relasi petugas) |
| Permintaan | Pengajuan Kebutuhan Barang | 3.1 tombol Buat Permintaan (`/stock-requests/create` tetap) | TERSEDIA |
| | Data Permintaan | 3.1 | TERSEDIA |
| | Validasi Atasan | 3.2 Persetujuan Permintaan | TERSEDIA |
| | Pemeriksaan Stok | 3.3 / detail: backend menolak penyerahan bila stok kurang (FEFO) | **BELUM TERSEDIA** sebagai tampilan ketersediaan per baris |
| | Status Pemenuhan | 3.3 tab | DISESUAIKAN |
| | Riwayat Permintaan | 3.4 | TERSEDIA |
| Penerimaan | Pemeriksaan Hasil Pengadaan | 4.1 (Keuangan) | TERSEDIA |
| | Pemeriksaan Jumlah & Kondisi | — | **KEPUTUSAN** (integrasi penerimaan Keuangan ke stok Logistik) |
| | Pencatatan Batch/Lot | — (sementara: Catat Saldo Pembuka di 5.4) | **KEPUTUSAN** (sama) |
| | Tanggal Kadaluarsa | — (sama) | **KEPUTUSAN** (sama) |
| | Barang Tidak Sesuai | — | **KEPUTUSAN** (sama) |
| | Dokumen Penerimaan | 4.1 (dokumen Keuangan) | **KEPUTUSAN** (sama) |
| Penyimpanan | Penempatan ke Gudang/Rak | Posisi rak di 2.3; pindah lokasi di 4.3 | DISESUAIKAN (langkah tempatkan sesudah terima menunggu integrasi penerimaan) |
| | Lokasi Penyimpanan | 2.3 dan 4.2 tab Lokasi Barang | DISESUAIKAN |
| | Pemisahan Barang Rusak | 4.2 tab Barang Rusak (lihat); 5.4 Kelola → ubah status (lakukan) | DISESUAIKAN |
| | Pengaturan Kondisi Penyimpanan | 2.3 (suhu, kelembapan, cold chain) | DISESUAIKAN (pemantauan kondisi aktual belum ada) |
| | Penelusuran Lokasi Barang | 4.2 tab Lokasi Barang | TERSEDIA |
| Stok | Stok per Gudang | 5.1 tab Stok per Batch (saringan lokasi) | TERSEDIA |
| | Stok per Unit | — | **BELUM TERSEDIA** (perlu saringan unit layanan pada endpoint saldo; tanpa skema) |
| | Kartu Stok | 5.1 tab Kartu Stok (baca); koreksi di 5.4 | DISESUAIKAN |
| | Riwayat Barang Masuk/Keluar | 5.1 tab Kartu Stok | TERSEDIA |
| | Batch/Lot | 5.3 tab Daftar Batch | TERSEDIA |
| | Mutasi Saldo Persediaan | 6.1 tab | DISESUAIKAN |
| Distribusi | Daftar Permintaan Disetujui | 3.3 tab Siap Diserahkan | TERSEDIA |
| | Penyiapan Barang | 3.3 → Detail → Serahkan (batch dipilih backend, FEFO) | DISESUAIKAN (tidak ada status "disiapkan" tersendiri) |
| | Pengeluaran Gudang | 3.3 → Serahkan (stok berkurang) | DISESUAIKAN |
| | Pengiriman ke Unit | — | **KEPUTUSAN** (perlu status/skema pengiriman) |
| | Serah Terima | 3.3 → Serahkan (pelaku dan waktu tercatat) | DISESUAIKAN (konfirmasi penerima belum disimpan: **KEPUTUSAN**) |
| | Pemenuhan Sebagian | 3.3 tab | DISESUAIKAN |
| Mutasi & Retur | Transfer Antar Gudang | 4.3 tab Transfer Antar Lokasi | TERSEDIA |
| | Perpindahan Lokasi | 4.3 tab Transfer Antar Lokasi (lokasi = gudang/rak) | DISESUAIKAN |
| | Retur Unit | 4.3 tab Retur dari Unit | TERSEDIA |
| | Retur ke Penyedia | 4.3 tab tautan Keuangan (izin) | TERSEDIA |
| | Barang Tidak Sesuai | 5.4 ubah status Karantina/Rusak; 4.2 lihat | DISESUAIKAN |
| | Riwayat Mutasi | 4.3 tab | DISESUAIKAN |
| Opname | Jadwal, Perhitungan Fisik, Selisih Stok, Verifikasi Hasil, Persetujuan Penyesuaian, Berita Acara (6) | 4.4 nonaktif | **BELUM TERSEDIA**: butuh skema (sesi, lembar hitung, verifikasi, berita acara). Sementara: penyesuaian per batch di 5.4; 6.4 hanya memeriksa saldo terhadap buku, bukan fisik |
| Pengendalian | Stok Minimal/Maksimal | 5.2 tab | DISESUAIKAN |
| | Rekomendasi Pengisian Ulang | 5.2 tab (bawaan) | TERSEDIA |
| | Kebutuhan Pengadaan | — | **KEPUTUSAN** (alur ke Pengadaan Keuangan; rekomendasi tidak boleh membuat pembelian otomatis) |
| | Pemantauan Barang (induk) | dilebur ke 1.1, 4.2, 5.3 | — |
| | › Kadaluarsa | 5.3 tab Kedaluwarsa; 1.1 | TERSEDIA |
| | › Barang Rusak | 4.2 tab Barang Rusak | DISESUAIKAN |
| | › Stok Lambat Bergerak | — | **KEPUTUSAN** (ambang lambat bergerak belum diputuskan; data mutasi tersedia, tanpa skema) |
| Laporan | Laporan Persediaan | 6.1 tab Posisi Persediaan | TERSEDIA |
| | Penerimaan | — | **KEPUTUSAN** (bergantung integrasi penerimaan) |
| | Distribusi | 6.2 tab | DISESUAIKAN |
| | Pemakaian per Unit | 6.3 tab | DISESUAIKAN |
| | Mutasi | 6.3 tab | DISESUAIKAN |
| | Stock Opname | — | **BELUM TERSEDIA** (bergantung 4.4) |
| | Nilai Persediaan | — | **KEPUTUSAN** (harga per batch belum disimpan, `LGS-OQ-008`) |
| | Rekonsiliasi dengan Keuangan | 6.4: rekonsiliasi saldo × buku mutasi (**baru**, hanya membaca) | Rekonsiliasi nilai dengan Keuangan: **KEPUTUSAN** (`LGS-OQ-008`) |

Tidak ada fungsi yang dibuang diam-diam. Semua route lama tetap ada dan dapat dibuka; route yang tidak lagi menjadi tujuan menu tetap menampilkan layar lamanya. Verifikasi Susulan CITO tetap dibuka dari halaman 3.2.

## 3. Perubahan frontend (FIN-LGS-075/076/077)

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Blok Logistik diganti struktur 6/20 (175 baris menggantikan 467). Modul lain tidak disentuh. Kunci entri yang fungsinya sama dipertahankan |
| `components/.../logistics-workspace/logistics-workspace.jsx` (baru), `style/.../logistics-workspace.module.css` (baru) | Halaman bertab yang memakai ulang layar existing; `role="tablist"`/`tab`/`tabpanel`; di layar sempit tab digeser di dalam bilahnya |
| `drug-stock-view/drug-stock-view.jsx` | Prop `tabs` (subset dan label), `manage` (Kelola dan Saldo Pembuka), serta judul per submenu. Bawaan sama seperti sebelumnya, jadi route lama tidak berubah. Pesan "aturan rekomendasi belum diputuskan" diperbarui |
| `drug-stock-view/drug-stock-card-view.jsx` | Varian `ledger` (baca saja) dan `correction` (dengan Koreksi). Varian `card` tetap untuk route lama |
| `logistics-report-view/ledger-reconciliation-view.jsx` (baru) + konstanta, service `getLedgerConsistency`, utilitas label temuan | Rekonsiliasi saldo × buku mutasi. Lokasi wajib (backend tidak dipanggil sebelum lokasi dipilih); keadaan kosong, galat, dan 403 ditangani; hanya membaca |
| Halaman route (`page.jsx`/client) | 1.1, 4.2, 5.3 memakai `DrugStockView` dengan tab terpilih. 2.2, 3.3, 4.3, 5.1, 5.2, 5.4, 6.1, 6.2, 6.3 menjadi halaman bertab. 6.4 baru. Judul halaman disesuaikan |
| `stock-request-view/stock-request-list-view.jsx` | Sisa teks FIN-LGS-070 ("unit yang Anda pimpin") diganti "dalam kewenangan Anda" |
| `tests/unit/logistics-module-navigation.test.mjs`, `logistics-reports.test.mjs`, `logistics-v2-features.test.mjs` | Diperbarui untuk struktur 6/20, halaman bertab, pemisahan lihat/ubah, route lama, dan Rekonsiliasi |

**Proses bisnis (FIN-LGS-076).** Semua alur memakai implementasi yang ada:

- permintaan: ajukan → setujui (izin, bukan pemohon, FIN-LGS-070) → serahkan (validasi stok backend, FEFO, sebagian) → selesai;
- distribusi dan pemenuhan sebagian di 3.3;
- mutasi lewat transfer (keluar asal, terima tujuan) di 4.3;
- pengendalian di 5.2 (min/maks → pantau → rekomendasi tanpa permintaan otomatis);
- laporan di 6.1–6.4 dengan saringan masing-masing.

Aturan perhitungan stok tidak diubah.

## 4. Perubahan backend (FIN-LGS-077)

**Tidak ada.** Seluruh layar memakai endpoint, DTO, dan izin yang sudah ada, termasuk `GET drug-stock/reports/ledger-consistency` (`DrugStock:Read`) untuk Rekonsiliasi. Tidak ada modul Warehouse atau database terpisah: Operasional Gudang adalah bagian dari Logistik. Transaksi stok, audit trail, izin, dan larangan menyetujui permintaan sendiri tidak berubah.

## 5. Fitur yang sudah berjalan

- Ringkasan persediaan, stok per batch/item, kartu stok, batch dan kedaluwarsa.
- Lokasi barang dan barang rusak.
- Permintaan, persetujuan (tanpa HRD), verifikasi susulan CITO, pemenuhan (termasuk sebagian), dan riwayat.
- Transfer antar lokasi, retur dari unit, riwayat mutasi, dan tautan retur ke penyedia.
- Kebijakan min/maks, pantau stok, dan rekomendasi pengisian ulang.
- Penyesuaian jumlah/status, saldo pembuka, dan koreksi kartu stok.
- Laporan persediaan, mutasi saldo, penerimaan dan pengeluaran, distribusi, pemakaian per unit, mutasi persediaan, serta rekonsiliasi saldo × buku mutasi.

## 6. Fitur yang masih perlu diselesaikan

| Fungsi | Kebutuhan | Jenis |
| --- | --- | --- |
| Stock Opname (4.4, laporan opname) | Entitas sesi opname, lembar hitung, verifikasi, berita acara | **Skema (migrasi)** + keputusan alur |
| Penerimaan ke stok Logistik (pemeriksaan, batch/kedaluwarsa, tidak sesuai, tempatkan, laporan penerimaan) | Penerimaan Keuangan belum menulis mutasi stok (tidak ada penulis `GoodsReceipt`) | **Lintas modul** (Keuangan) + keputusan |
| Penanggung Jawab Gudang | Kolom/relasi petugas pada lokasi | Skema + keputusan |
| Pengiriman ke unit, konfirmasi penerima serah terima | Status pengiriman dan tanda terima | Skema + keputusan |
| Stok per Unit | Saringan unit layanan pada endpoint saldo (lokasi sudah punya `ServiceUnitId`) | Perubahan backend kecil, tanpa skema |
| Tampilan Pemeriksaan Stok per baris permintaan | Ketersediaan per baris sebelum penyerahan | Perubahan frontend/backend kecil |
| Stok Lambat Bergerak | Ambang lambat bergerak | Keputusan bisnis |
| Kebutuhan Pengadaan | Alur ke Pengadaan Keuangan | Keputusan + lintas modul |
| Nilai Persediaan, rekonsiliasi nilai dengan Keuangan | Harga per batch (`LGS-OQ-008`) | Keputusan + skema |
| Akses lintas unit (`LGS-OQ-027`/`029`) | Tetap **BLOCKED_DECISION**; penyederhanaan menu tidak memperluas akses data | Keputusan |

## 7. Hasil pemeriksaan dan batas verifikasi (FIN-LGS-078)

| Pemeriksaan | Hasil |
| --- | --- |
| Struktur menu | **6 menu, 20 submenu**, nama dan urutan sesuai struktur final (diasersi uji) |
| Tautan | 19 tautan, semuanya unik dan menunjuk `page.jsx` yang ada; Stock Opname nonaktif (tanpa tautan). Tidak ada kunci menu ganda; kurung blok menu seimbang |
| Route lama | 13 route lama yang tidak lagi menjadi tujuan menu tetap ada (diasersi uji) |
| Tautan internal | Tombol kembali transfer → `/stock-transfers` membuka tab Transfer (isi sama). Tautan ke laporan mutasi → route lama yang tetap ada. Layar master data memakai path absolut, jadi tambah/ubah/detail berfungsi dari 2.2 dan 5.2 (sesudah simpan, pengguna kembali ke route master data) |
| Izin | Setiap layar tetap memegang `AccessDeniedGate`/izin backend-nya. 4.1 tetap `requiredPermission`. Tab Retur ke Penyedia hanya tampil bila izin Keuangan sudah pasti ada. Perintah ubah stok hanya di 5.4 |
| Loading/kosong/galat | Ditangani layar existing. Rekonsiliasi: pesan pilih lokasi, kosong per mode, galat dengan coba lagi, 403 |
| Responsif | Bilah tab membungkus; di ≤576 px digeser horizontal di dalam bilahnya, bukan menggeser halaman |
| Kontrak FE–BE | Rekonsiliasi: parameter `storageLocationId`, `drugId`, `onlyInconsistent`, `pageNumber`, `pageSize` dan field respons dicocokkan dengan `LedgerConsistencyQuery`/`LedgerConsistencyResponse`. Status stok dibaca sebagai angka enum (serializer bawaan) |
| Uji unit frontend | **70/70 lulus** (sebelumnya 65; ada 5 uji navigasi baru) |
| ESLint | 78 berkas `.js/.jsx/.mjs` berubah: **0 error**, 4 warning lama `react-hooks/set-state-in-effect` |
| Backend | Tidak ada perubahan source pada tahap ini; hasil terakhir tetap `run10.trx` **144/144** (23 November 2026) |
| Uji manual di peramban | **NOT FEASIBLE**: dev server dilarang. Tampilan tab, sorotan sidebar, dan tata letak responsif belum diamati langsung |

## 8. Daftar berkas

**Backend** (`Ikbal`): tetap 23 berkas pada bagian 3 *EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073*. Yang berubah pada tahap ini hanya dokumen ini.

**Frontend** (`Ikbalv2`): **80 berkas**, yaitu 62 berkas sebelumnya ditambah 18 berkas ini:

| Status | Berkas |
| --- | --- |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-workspace/logistics-workspace.jsx` |
| Baru | `src/style/health-services/pharmacy-management/logistics-workspace.module.css` |
| Baru | `src/components/view/health-services/pharmacy-management/logistics-report-view/ledger-reconciliation-view.jsx` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/adjustments/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/drug-stock/adjustments/drug-stock-adjustment-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-master/categories-units/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-master/categories-units/logistics-category-unit-client.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/reconciliation/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/logistics-reports/reconciliation/ledger-reconciliation-client.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/drug-stock/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/drug-stock/drug-stock-client.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/drug-stock/summary/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/drug-stock/batches/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/drug-stock/locations/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/stock-requests/approved/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/stock-requests/approved/stock-request-approved-queue-client.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/stock-transfers/page.jsx` |
| Ubah | `src/app/health-services/pharmacy-management/stock-transfers/stock-transfer-list-client.jsx` |

Berkas dari daftar 62 yang ikut diubah lagi pada tahap ini (17):

- view: `drug-stock-view.jsx`, `drug-stock-card-view.jsx`, `stock-request-list-view.jsx`;
- menu: `menu-items.jsx`;
- laporan: `logistics-report-constant.js`, `logistics-report.service.js`, `logistics-report-utils.js`;
- halaman: `drug-stock/replenishment` (page dan client), `logistics-reports/inventory` client, `logistics-reports/flow-summary` (page dan client), `logistics-reports/usage-by-unit` (page dan client);
- uji: tiga berkas uji Logistik.

**Jangan ikut di-commit:** sama seperti sebelumnya (`SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`).

## 9. Status

**READY FOR OWNER REVIEW.** Sebelum commit:

1. Tinjau navigasi dan halaman bertab di peramban (uji manual belum dilakukan).
2. Putuskan butir pada bagian 6.
3. Jalankan uji frontend (70/70).

---

# FINISHING 6 MENU / 20 SUBMENU (FIN-LGS-079 s.d. FIN-LGS-082)

**Tanggal:** 23 November 2026
**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, akses basis data operasional, menjalankan backend, perubahan HRD/Keuangan/MasterData.
**Yang dijalankan:**

- build dan uji backend (proyek uji terisolasi, SQLite dalam memori);
- uji unit frontend dan ESLint;
- dev server frontend lokal (`next dev`, terikat `127.0.0.1:3100`) untuk pemeriksaan peramban.

**Status: READY FOR OWNER REVIEW.**

## 1. FIN-LGS-079 — Pemeriksaan tampilan di peramban

**Cara aman yang dipakai.**

- Backend **tidak** dijalankan, karena saat start seeder akses menulis ke basis data.
- Hanya dev server frontend yang dinyalakan.
- Halaman dibuka Chromium headless (Playwright yang sudah terpasang di repo). Setiap permintaan ke API dijawab di peramban oleh skrip pemeriksa:
  - identitas uji untuk AuthWrapper;
  - izin superadmin supaya seluruh menu tampil;
  - respons kosong, galat 500, atau 403;
  - permintaan ke host luar dibatalkan (tercatat 0 permintaan keluar).
- Untuk panel penyerahan, satu permintaan contoh dijawab skrip, hanya selama pemeriksaan.
- Tidak ada data nyata yang dibaca atau ditulis.
- Skrip dan tangkapan layar ada di folder scratchpad sesi (sementara). Dev server sudah dihentikan dan tidak meninggalkan berkas di repo.

| Pemeriksaan | Hasil |
| --- | --- |
| 19 submenu bertaut (desktop 1440×900) | Semua status 200, tanpa 404, tanpa geser horizontal. Sidebar menandai submenu yang benar (`aria-current="page"`) di seluruh 19 halaman |
| Tab halaman gabungan | Setiap tab di 2.2, 3.3, 4.3, 5.1, 5.2, 5.4, 6.1, 6.2, 6.3 diklik dan membuka layar yang benar. Tab Retur ke Penyedia tampil (izin uji ada) |
| Stock Opname (4.4) | Tampil nonaktif tanpa tautan, sesuai rancangan |
| Keadaan kosong | Pesan kosong tiap layar tampil (mis. "Belum ada stok yang tercatat pada saringan ini.") |
| Galat 500 | 1.1, 3.3, 5.1, 6.1 menampilkan panel galat dengan pesan backend dan tombol coba lagi. Galat konsol hanya catatan kegagalan jaringan yang disimulasikan |
| 403 | 3.2, 5.4, 6.2 menampilkan "Ups! Akses Ditolak" dari `AccessDeniedGate` |
| Mobile (390×844) | 1.1, 2.2, 3.3, 4.3, 5.1, 5.2, 6.4 beserta seluruh tabnya: tanpa geser horizontal halaman |
| Ingatan tab (FIN-LGS-080) | Pilih "Satuan & Konversi" di 2.2 → pindah ke 5.1 → kembali ke 2.2: tab "Satuan & Konversi" terbuka lagi |
| Panel penyerahan (FIN-LGS-081) | Kolom "Tersedia di Gudang" tampil per baris. Baris dengan stok 6 untuk permintaan 10 diberi keterangan "Kurang dari yang diminta". Peringatan muncul saat isian 10 dan hilang saat diisi 6. Tombol Konfirmasi tetap aktif. Desktop 5 kolom; mobile satu kolom. Tanpa galat konsol |

**Masalah tampilan yang ditemukan dan diperbaiki:**

| Temuan | Perbaikan |
| --- | --- |
| Peringatan React "dua anak dengan key yang sama" (`.1`, `.3`) di 3.1 dan setiap tab 3.3. `DataFilter` (komponen bersama) meratakan Fragment dengan `Children.toArray` bertingkat, sehingga key berbasis indeks bertabrakan | `stock-request-list-view.jsx`: setiap anak saringan diberi `key` eksplisit. `DataFilter` tidak diubah. Diperiksa ulang: 0 galat konsol |
| Layar Master Data di tab 2.2 dan 5.2 menempel pada bilah tab dan tidak sejajar (layar Master Data dirancang rapat ke tepi, layar Logistik ber-inset 1rem) | Opsi tab `framed` di halaman gabungan memberi jarak 1rem atas/samping; layar Master Data tidak diubah |
| Mobile: tombol tab menyusut sehingga label terpotong dan bertumpuk | `.tab { flex: 0 0 auto }`: label utuh, bilah tab digeser di dalam bilahnya |
| Tab pertama 1.1 berlabel "Per Item", tidak sesuai laporan ("Ringkasan Stok") | Label diganti "Ringkasan Stok" |
| Angka stok yang kurang tampil lebih kecil daripada angka lain di panel penyerahan | Angka selalu ukuran normal; hanya keterangan "Kurang dari yang diminta" berwarna peringatan |

**Batas.** Pemeriksaan memakai respons simulasi, bukan data nyata. Tampilan dengan data banyak, nama panjang, dan kinerja terhadap backend sungguhan tetap perlu dilihat saat UAT.

## 2. FIN-LGS-080 — Kembali ke halaman Logistik sesudah tambah/ubah

| Butir | Status |
| --- | --- |
| Pengembalian otomatis ke tab Logistik sesudah simpan di Kategori & Satuan dan Stok Minimal/Maksimal | **BLOCKED — perubahan modul MasterData**. Hook editor, detail, dan daftar MasterData menulis tujuan navigasinya secara tetap (`DRUG_CATEGORY_TABLE_PATH`, `DRUG_UNIT_CONVERSION_TABLE_PATH`, `DRUG_STOCK_POLICY_TABLE_PATH`, dan `…/create`) tanpa dukungan alamat kembali. Logistik tidak dapat mengalihkannya tanpa mengubah MasterData (dilarang) atau menyalin formulirnya (melanggar larangan membuat ulang fitur) |
| Yang dikerjakan dari Logistik | Halaman gabungan 2.2 dan 5.2 mengingat tab terakhir selama sesi (`storageKey`). Pengguna yang kembali lewat menu Logistik mendarat di tab asalnya. Diperiksa di peramban (bagian 1) |
| Route lama | Tidak berubah |
| **Alternatif paling sederhana** untuk pemilik MasterData | Parameter opsional `returnTo` pada tiga hook daftar (diteruskan ke create/detail) dan tiga pasang hook editor/detail (dipakai untuk redirect sesudah simpan dan tombol kembali). Nilainya hanya diterima bila berupa path internal berawalan `/`. Logistik lalu mengirim `?returnTo=/health-services/pharmacy-management/logistics-master/categories-units` atau `/drug-stock/replenishment` |

## 3. FIN-LGS-081 — Penyempurnaan stok

| Fitur | Backend | Frontend |
| --- | --- | --- |
| **Stok per unit** | `ServiceUnitId` opsional pada `DrugStockBalanceQuery` dan `DrugStockSummaryQuery`. Penyaring `FilterByServiceUnit` memakai struktur yang ada: lokasi dengan `MstDrugStorageLocation.ServiceUnitId` = unit, atau lokasi tanpa unit yang induknya (satu tingkat) milik unit itu. Tanpa skema baru | Saringan "Semua unit" (resource `serviceUnits`) pada tab per batch dan per item; dipakai 5.1 dan 1.1. Field `serviceUnitId` ditambahkan pada saringan saldo dan ringkasan |
| **Ketersediaan per baris sebelum penyerahan** | `StockRequestItemResponse.AvailableQuantity` dan `StockUnitName` (field tambahan; kontrak lama tetap). Dihitung hanya bila `CanFulfill`, dengan penyaring yang sama seperti kandidat FEFO penyerahan: gudang pemenuhan, Siap Pakai, belum kedaluwarsa, `OnHand − Reserved` | Kolom "Tersedia di Gudang" di panel penyerahan, keterangan "Kurang dari yang diminta", dan peringatan bila isian melebihi stok. Tombol tidak dikunci |
| **Validasi tetap di backend** | `FulfillAsync` tidak diubah; stok kurang tetap ditolak `PHM028` (diuji) | Angka hanya informasi |

Tidak ada tabel, migrasi, atau perubahan modul di luar Logistik.

Catatan yang tercantum di dokumentasi field: baris berbeda dengan obat yang sama memakai stok yang sama, dan stok dapat berubah antara membuka layar dan menekan tombol.

## 4. FIN-LGS-082 — Hasil uji dan pemeriksaan statis

| Uji | Hasil sebenarnya |
| --- | --- |
| Build backend (`build12.log`) | **0 error**, 599 warning (sama dengan pembanding); tidak ada warning dari berkas yang berubah |
| Uji backend, percobaan pertama (`run11.trx`) | 145 lulus, 1 gagal: uji stok per unit baru. Data ujinya belum menyediakan master obat, sedangkan daftar saldo mengurutkan menurut nama obat (JOIN wajib). Diperbaiki pada data uji; penyaringnya benar |
| Uji backend akhir (`run12.trx`) | **146/146 lulus**: `total=146`, `executed=146`, `failed=0`, `notExecuted=0`; 124 metode. Termasuk `Stok_per_unit_menyaring_lokasi_milik_unit_langsung_dan_lewat_induknya` dan `Detail_permintaan_menampilkan_ketersediaan_per_baris_dengan_aturan_penyerahan`. Source backend tidak berubah sesudahnya |
| Uji unit frontend | **75/75 lulus** (sebelumnya 70; ditambah uji ingatan tab, saringan unit, panel penyerahan, perbaikan tampilan, dan perilaku `resolveLineAvailability`) |
| ESLint | 79 berkas: **0 error**, 4 warning lama `react-hooks/set-state-in-effect` |
| Kontrak FE–BE | `serviceUnitId` ↔ `ServiceUnitId` (saldo, ringkasan). `availableQuantity`/`stockUnitName` ↔ `StockRequestItemResponse`. Uji kontrak detail tetap lulus |
| Ringkasan per item dengan saringan unit | Memakai penyaring yang sama dengan saldo. Tidak dapat diuji di SQLite, karena SQLite tidak dapat menjumlah decimal pada `GroupBy`; diperiksa statis |

## 5. Blocker yang tersisa

| Butir | Status |
| --- | --- |
| Pengembalian otomatis sesudah simpan di layar MasterData | **BLOCKED** — butuh perubahan MasterData (bagian 2) |
| Stock Opname (4.4, laporan opname) | Butuh skema (sesi, lembar hitung, verifikasi, berita acara) |
| Penerimaan pengadaan ke stok Logistik | Lintas modul (Keuangan) + keputusan |
| Penanggung jawab gudang, pengiriman ke unit, konfirmasi penerima | Skema + keputusan |
| Stok lambat bergerak, kebutuhan pengadaan, nilai persediaan dan rekonsiliasi nilai Keuangan (`LGS-OQ-008`) | Keputusan bisnis (+ skema/lintas modul) |
| `LGS-OQ-027`/`029` | **BLOCKED_DECISION**; tidak ada perluasan akses lintas unit |
| UAT persetujuan dan UAT L1–L2 | Belum; produksi **NO-GO** sampai UAT persetujuan lulus |
| ~~Stok per Unit~~, ~~tampilan Pemeriksaan Stok per baris~~ (bagian 6 *PENYEDERHANAAN NAVIGASI LOGISTIK*) | **Selesai** pada tahap ini |

## 6. Daftar berkas

**Backend** (`Ikbal`), tetap **23 berkas** (daftar pada bagian 3 *EKSEKUSI UJI FIN-LGS-070 s.d. FIN-LGS-073*). Yang berubah lagi pada tahap ini:

- `DTOs/DrugStockDtos.cs` (`ServiceUnitId` pada dua query);
- `Services/DrugStockService.cs` (`FilterByServiceUnit`);
- `DTOs/StockRequestDtos.cs` (`AvailableQuantity`, `StockUnitName`);
- `Services/StockRequestService.cs` (`ApplyStockAvailabilityAsync`);
- `Tests/.../LogisticsReportTests/LogisticsReportTests.cs` (2 uji baru, parameter `induk`);
- dokumen ini.

**Frontend** (`Ikbalv2`): **82 berkas**, yaitu 80 berkas sebelumnya ditambah:

| Status | Berkas |
| --- | --- |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-warehouse-panel.jsx` |
| Ubah | `src/style/health-services/pharmacy-management/stock-request.module.css` (kelas baru `.fulfillmentRow`; kelas lain tidak berubah) |

Berkas dari daftar 80 yang ikut diubah lagi:

- `logistics-workspace.jsx` dan `logistics-workspace.module.css`;
- `drug-stock-view.jsx`, `use-drug-stock.jsx`;
- `stock-request-list-view.jsx`, `stock-request-approval-utils.js`;
- `drug-stock/summary/page.jsx`;
- `logistics-master/categories-units/logistics-category-unit-client.jsx`, `drug-stock/replenishment/drug-stock-replenishment-client.jsx`;
- `tests/unit/logistics-module-navigation.test.mjs`, `tests/unit/stock-request-approval.test.mjs`.

**Jangan ikut di-commit:** `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.


# STOCK OPNAME — MODE TERBATAS (FIN-LGS-083 s.d. FIN-LGS-087)

**Tanggal:** 24 November 2026
**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, akses basis data operasional, `dotnet build`, `dotnet test`, `npm run build`, `npm run dev`, perubahan modul di luar Logistik, penghapusan data.
**Yang dijalankan:** uji unit frontend (`node --test`) dan ESLint. Backend hanya diperiksa statis.

**Status: BLOCKED_SCHEMA** untuk Stock Opname penuh. Bagian yang tidak membutuhkan skema baru sudah dikerjakan dan **siap ditinjau pemilik**.

## Ringkasan untuk pemilik

Alur yang diminta ada tujuh langkah. Empat langkah pertama dan pratinjau langkah ketujuh berjalan dengan data yang sudah ada; sisanya butuh tabel baru.

| Langkah | Status | Keterangan |
| --- | --- | --- |
| 1. Buat Stock Opname | Sebagian | Lembar hitung dapat dibuat di layar, tetapi **tidak tersimpan** sebagai sesi |
| 2. Pilih gudang/lokasi | **Berjalan** | Lokasi wajib; dapat dipersempit per status stok |
| 3. Input hasil hitung fisik | **Berjalan** (hanya di layar) | Isian hilang bila halaman ditutup. Tidak disimpan di peramban |
| 4. Sistem menghitung selisih | **Berjalan** | Dihitung server terhadap saldo saat ini; tidak menyimpan apa pun |
| 5. Verifikasi dan persetujuan | Tampilan ringkasan berjalan; **Setujui/Tolak nonaktif** | Butuh sesi tersimpan, izin, larangan self-approval, dan audit |
| 6. Penyesuaian stok | **Nonaktif** | Tidak boleh terjadi sebelum ada persetujuan yang tersimpan |
| 7. Berita acara | **Draf saja** (tanpa nomor, bertanda "bukan dokumen resmi"); **Terbitkan nonaktif** | Berita acara resmi butuh sesi yang disetujui dan diposting |

Contoh pemakaian hari ini: petugas memilih "Gudang Induk", memuat lembar (misalnya 180 baris), menghitung rak demi rak, mengisi stok fisik, lalu menekan **Hitung Selisih**. Server menjawab, misalnya, "170 cocok, 4 lebih, 5 kurang, 1 perlu dihitung ulang karena ada pengeluaran sesudah lembar dimuat". Hasil itu dapat dibaca dan dijadikan draf berita acara, tetapi **saldo tidak berubah**. Koreksi resmi tetap lewat **Penyesuaian Stok** (5.4) sampai tabel Stock Opname tersedia.

## 1. FIN-LGS-083 — Yang dipakai ulang

| Kebutuhan opname | Yang sudah ada | Dipakai untuk |
| --- | --- | --- |
| Saldo per batch × status × lokasi | `PhmDrugStockBalance`, `GET drug-stock/balances` | Isi lembar hitung (semua status, termasuk saldo nol) |
| Mengetahui saldo berubah | `Version` saldo dan buku mutasi (setiap perubahan `OnHand` menulis mutasi) | Menandai baris "Hitung ulang" |
| Koreksi saldo yang aman | `DrugStockService.AdjustAsync`: `ExpectedVersion`, `IdempotencyKey`, alasan wajib, mutasi tercatat | Jalur posting penyesuaian kelak (belum dipanggil) |
| Sumber mutasi di Kartu Stok | `DrugStockSourceDocumentTypes.StockOpname`; label frontend "Stock Opname" | Penanda mutasi hasil opname kelak |
| Izin | `DrugStock:Read`, `DrugStock:Adjust` | Read untuk lembar dan hitung selisih; Adjust untuk posting kelak |
| Galat bisnis 422 | `LogisticsReportUnprocessableException`, kode `PHM036` (lokasi) | Pola galat endpoint baru |
| Komponen layar | `LogisticsWorkspace`, `StockRequestResourceSelect`, `DataTable`, `SummaryGrid`, `AccessDeniedGate`, konstanta status stok | Satu halaman dengan empat tab |

Yang **belum ada**: tabel sesi, baris hitung, keputusan, dan berita acara; izin khusus Stock Opname; penomoran dokumen opname.

## 2. FIN-LGS-084 — Backend tanpa migrasi

### 2.1 Endpoint hitung selisih

```
POST /api/v1/health-services/pharmacy-management/drug-stock/opname/variance-preview
Izin   : DrugStock:Read  (AccessAction "Read", SortOrder 17)
Tag    : Health Services / Pharmacy Management / Drug Stock
Sifat  : hanya membaca — tidak menyimpan, tidak menulis mutasi, tidak mengubah saldo
```

Memakai POST hanya karena lembar hitung bisa sampai 2000 baris (terlalu panjang untuk query string). Karena tidak ada yang ditulis, izinnya Read.

**Contoh permintaan:**

```json
{
  "storageLocationId": "3f1c…",
  "snapshotTakenAtUtc": "2026-11-24T02:00:00Z",
  "counts": [
    { "balanceId": "a1…", "snapshotQuantity": 10, "physicalQuantity": 10, "note": null },
    { "balanceId": "b2…", "snapshotQuantity": 3,  "physicalQuantity": 0,  "note": "kemasan rusak dibuang" },
    { "balanceId": "c3…", "snapshotQuantity": 5,  "physicalQuantity": null }
  ]
}
```

- `snapshotQuantity`: stok sistem yang tercetak di lembar saat dimuat (wajib).
- `physicalQuantity`: hasil hitung; `null` berarti belum dihitung, `0` adalah hasil yang sah.

**Contoh jawaban 200 (`data`):**

```json
{
  "storageLocationName": "Gudang Induk",
  "calculatedAtUtc": "2026-11-24T03:10:00Z",
  "isPersisted": false,
  "summary": { "lineCount": 3, "countedLineCount": 2, "matchCount": 1, "shortageCount": 1,
               "surplusCount": 0, "needsRecountCount": 0, "notCountedLineCount": 1, "isComplete": false },
  "lines": [
    { "balanceId": "b2…", "drugName": "Parasetamol 500 mg", "batchNumber": "B-77", "status": 4,
      "snapshotQuantity": 3, "currentQuantity": 3, "physicalQuantity": 0, "variance": -3,
      "result": "Shortage", "isExpired": false, "changedSinceSnapshot": false }
  ],
  "limitations": ["Hasil hitung selisih ini tidak disimpan. …"]
}
```

**Aturan hitung:**

| Keadaan baris | Hasil | Selisih |
| --- | --- | --- |
| Stok fisik kosong | `NotCounted` (Belum dihitung) | — |
| Saldo saat ini ≠ stok di lembar, **atau** ada mutasi batch × status itu di lokasi itu sesudah `snapshotTakenAtUtc` | `NeedsRecount` (Hitung ulang) | — (tidak dihitung) |
| Fisik = saldo | `Match` (Cocok) | 0 |
| Fisik > saldo | `Surplus` (Lebih) | fisik − saldo |
| Fisik < saldo | `Shortage` (Kurang) | fisik − saldo |

`isComplete` bernilai benar hanya bila tidak ada baris belum dihitung dan tidak ada yang perlu dihitung ulang. Status Rusak dan Kedaluwarsa dihitung sebagai baris sendiri. Batch yang sudah lewat tanggal kedaluwarsa ditandai `isExpired`, apa pun statusnya.

**Galat 422** (isian ditolak utuh, pesan menyebut nomor baris):

| Kode | Arti |
| --- | --- |
| `PHM036` | Lokasi kosong, tidak terdaftar, atau sudah dihapus |
| `PHM037` | Lembar kosong atau > 2000 baris; saldo kosong atau ganda; stok di lembar tidak dikirim; stok fisik negatif, > 100.000.000, atau lebih dari 3 desimal |
| `PHM038` | Ada baris yang saldonya tidak ditemukan, sudah dihapus, atau milik lokasi lain. Lembar harus dimuat ulang; tidak ada baris yang dilewati diam-diam |

### 2.2 Perubahan stok selama opname

Pembekuan stok selama opname **belum diputuskan**. Jadi yang dibuat adalah pengaman yang berlaku apa pun keputusannya kelak: baris yang saldonya bergerak sesudah lembar dimuat **tidak pernah diberi selisih**, tetapi wajib dihitung ulang. Contoh: lembar mencatat 10, lalu ada pengeluaran 2 dan pengembalian 2. Jumlahnya tetap 10, tetapi barisnya tetap ditandai Hitung ulang karena barang sempat bergerak.

Batas: waktu lembar memakai jam perangkat. Perubahan jumlah tetap terdeteksi walaupun jam perangkat keliru; yang bergantung pada jam hanya deteksi gerak yang jumlah akhirnya sama.

### 2.3 Perbaikan kecil pada daftar saldo

`GetBalancesAsync` diberi `Id` sebagai penentu urutan terakhir. Urutan yang tampil tetap sama (yang paling dekat kedaluwarsa lebih dahulu), tetapi pembagian halaman kini stabil. Tanpa ini, lembar hitung yang dimuat beberapa halaman dapat melewatkan atau menggandakan baris yang tanggal kedaluwarsa dan nama obatnya sama.

### 2.4 Berkas backend

| Status | Berkas | Isi |
| --- | --- | --- |
| Baru | `Areas/HealthServices/PharmacyManagement/DTOs/StockOpnameDtos.cs` | Permintaan, baris, ringkasan, dan jawaban hitung selisih; kode hasil |
| Baru | `Areas/HealthServices/PharmacyManagement/Services/StockOpnameQueries.cs` | `StockOpnameCalculator` (aturan murni, dipakai ulang kelak) dan `StockOpnameQueries.PreviewVarianceAsync` (hanya `AsNoTracking`, tanpa `SaveChanges`) |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/DrugStockService.cs` | Delegasi `PreviewOpnameVarianceAsync`; `ThenBy(Id)` pada daftar saldo |
| Ubah | `Areas/HealthServices/PharmacyManagement/Controllers/DrugStockController.cs` | Endpoint `POST opname/variance-preview` |
| Baru | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/StockOpnameTests.cs` | 8 metode / 10 kasus (lihat bagian 5) — **belum dijalankan** |

## 3. FIN-LGS-085 — Frontend

**Route:** `/health-services/pharmacy-management/stock-opname`. Satu halaman dengan empat tab yang memakai satu state bersama (`useStockOpname`), sehingga berpindah tab tidak membuang isian.

| Tab | Isi | Tindakan nonaktif |
| --- | --- | --- |
| Daftar Opname | Daftar sesi (kolom nomor, lokasi, tanggal, status, pembuat, penyetuju) selalu kosong, dengan penjelasan BLOCKED_SCHEMA. Lembar yang sedang dikerjakan ditampilkan terpisah sebagai "belum disimpan" | — |
| Lembar Hitung (bawaan) | Form lokasi, status stok, tanggal opname, tombol **Mulai Hitung**. Tabel: No., barang, batch + ED + tanda lewat kedaluwarsa, status, posisi rak, stok sistem, **stok fisik** (isian), selisih di lembar, **catatan** (isian). Ringkasan, pencarian, saringan (semua / belum dihitung / ada selisih / isian belum benar), halaman 50 baris, tombol **Hitung Selisih** | Simpan Hasil Hitung, Ajukan Verifikasi |
| Verifikasi | Ringkasan dari server (cocok, lebih, kurang, hitung ulang, belum dihitung), tabel hasil (bawaan: hanya yang perlu perhatian), batas hasil, kolom alasan keputusan | Setujui, Tolak, Posting Penyesuaian Stok (alasan nonaktif) |
| Berita Acara | Draf dokumen: lokasi, tanggal, waktu lembar dan hitung, ringkasan kalimat, tabel baris berselisih, kolom tanda tangan (penghitung, pemeriksa, penyetuju). Bertanda "Draf — bukan dokumen resmi" dan "belum bernomor" | Terbitkan Berita Acara |

Setiap tombol nonaktif tidak punya handler dan disertai aturan yang akan berlaku. Contoh: "Penyetuju wajib berwenang dan bukan pembuat maupun penghitung sesi (larangan self-approval)".

**Penjagaan di layar:**

- isian dicek dengan aturan yang sama dengan `PHM037`;
- isian salah tidak dikirim, dan nomor barisnya disebut;
- memuat ulang atau menutup lembar yang sudah berisi meminta konfirmasi di layar;
- hasil hitung hanya berlaku untuk isian yang sama persis. Bila isian berubah, Verifikasi dan Berita Acara menampilkan peringatan "belum memuat isian terbaru";
- lembar yang melebihi 2000 baris, atau yang jumlah barisnya tidak sama dengan total backend, diberi peringatan;
- 403 menampilkan "Akses Ditolak"; galat lain tampil di panel beserta tombol coba lagi.

## 4. FIN-LGS-086 — Integrasi

| Butir | Hasil |
| --- | --- |
| Endpoint | Hanya `GET drug-stock/balances` (sudah ada; per 200 baris, maksimal 10 halaman) dan `POST drug-stock/opname/variance-preview` (baru, baca saja) |
| DTO | `balanceId`/`snapshotQuantity`/`physicalQuantity`/`note` ↔ `StockOpnameCountInput`; jawaban dibaca lewat `normalizeVariancePreview`. `isPersisted` hanya dianggap benar bila boolean `true` (backend selalu `false`) |
| Izin | `DrugStock:Read` untuk keduanya; tidak ada izin baru |
| localStorage / sessionStorage | Tidak dipakai (dipastikan uji statis atas 13 berkas opname, termasuk tanpa `storageKey` tab) |
| Mengubah saldo dari frontend | Tidak ada. Berkas opname tidak memanggil `adjustStock`, `changeStockStatus`, `recordOpeningBalance`, atau `correctStockMutation` (diuji) |
| Menu | Struktur tetap **6 menu / 20 submenu**; nama tidak berubah |

**Keputusan yang perlu ditinjau pemilik: submenu 4.4 kini bertaut.** Sebelumnya submenu ini tanpa tautan. Sekarang ia membuka halaman di atas, yang judul dan peringatannya menyatakan "Mode terbatas: hasil opname belum dapat disimpan". Halaman ini **bukan** fitur operasional penuh: tidak ada penyimpanan, persetujuan, penyesuaian, atau berita acara resmi. Alasannya, halaman ini memang perlu dibuka untuk ditinjau, dan lembar hitung serta hitung selisih sudah berguna sebagai alat bantu. Bila pemilik ingin submenu tetap nonaktif sampai skema tersedia, cukup hapus satu baris `pathname` di `menu-items.jsx` dan kembalikan satu asersi di uji navigasi.

## 5. FIN-LGS-087 — Pemeriksaan

| Kasus | Backend (`StockOpnameTests.cs`, belum dijalankan) | Frontend (`stock-opname.test.mjs`, dijalankan) |
| --- | --- | --- |
| Selisih cocok / lebih / kurang | `Kalkulator_menghitung_selisih…` (3 kasus) dan uji utama | Selisih lembar tanpa galat pecahan (0,3 − 0,1 = 0,2) |
| Stok fisik nol | Baris Rusak dihitung 0 → Kurang −3 | `"0"` sah; kosong = belum dihitung |
| Barang rusak dan kedaluwarsa | Status Rusak, status Kedaluwarsa, dan batch lewat ED berstatus Siap Pakai (`IsExpired`) | Label status per baris |
| Perbedaan batch | Setiap batch × status satu baris; mutasi pada status lain atau lokasi lain untuk batch yang sama tidak memengaruhi | Urutan per rak → barang → ED → status |
| Perubahan stok bersamaan | Jumlah berubah → Hitung ulang; keluar-masuk dengan jumlah akhir sama → Hitung ulang; mutasi sebelum lembar dimuat → tetap Cocok | Lembar yang bergeser saat dimuat ditandai; hasil lama ditandai tidak berlaku bila isian berubah |
| Isian tidak sah | 9 kasus `PHM037`, 4 kasus `PHM036`, 3 kasus `PHM038` | Aturan yang sama di layar |
| Persetujuan tanpa izin, self-approval, transaksi ganda | Tidak dapat terjadi: tidak ada endpoint persetujuan atau penyesuaian. Aturannya menjadi syarat skema (bagian 6) | Tombol nonaktif tanpa handler (diuji) |
| Kartu stok dan audit | Hitung selisih tidak menulis mutasi apa pun, termasuk sumber `StockOpname`; saldo dan `Version` tidak berubah (diuji) | Tidak ada panggilan perintah stok (diuji) |
| Izin endpoint | `DrugStock:Read`, `[Authorize]`, template route; controller menjawab 422 `PHM036`/`PHM037` | 403 → "Akses Ditolak" |
| Navigasi | — | 20/20 submenu bertaut ke halaman yang ada; 4.4 → halaman opname |

| Uji / pemeriksaan | Hasil sebenarnya |
| --- | --- |
| Build dan uji backend | **Tidak dijalankan** (`dotnet build` dilarang pada tahap ini). Hasil terakhir yang sah tetap run12 = 146/146, sebelum perubahan ini. Uji baru menambah 8 metode / 10 kasus dan **belum terbukti lulus** |
| Pemeriksaan statis backend | Keseimbangan kurung 5 berkas: OK. Nama `DbSet`, properti entity (`MstDrugs`, `MstMeasurements`, `PhmDrugBatches`, `PhmDrugStockBalances`, `PhmDrugStockMutations`, `MstDrugStorageLocations`, `IsDelete`, `OccurredAt`), enum `DrugStockStatus.Expired/Damaged`, `LogisticsReportUnprocessableException.Code`, namespace, dan pola delegasi dicocokkan dengan source. Nama, satuan, dan batch dimuat terpisah (bukan lewat navigasi) supaya tidak bergantung pada JOIN wajib yang pernah menggagalkan run11 |
| Uji unit frontend | **86/86 lulus** (75 sebelumnya + 11 baru) |
| ESLint | Berkas opname, menu, dan dua berkas uji: **0 error, 0 warning** |
| Pemeriksaan peramban | **Tidak dapat dilakukan**: `npm run dev` dilarang pada tahap ini. Tampilan, perilaku tab, dan responsif belum dilihat langsung |

## 6. Skema minimum yang dibutuhkan (BLOCKED_SCHEMA)

Usulan ini perlu diputuskan pemilik registry dan modul sebelum dibuat. Nama mengikuti prefix `Phm` yang dipakai entity stok saat ini.

**Entity**

| Entity | Kolom inti | Relasi |
| --- | --- | --- |
| `PhmStockOpname` (sesi) | `Id`, `OpnameNumber`, `StorageLocationId`, `StatusFilter` (opsional), `OpnameDate`, `Status` (Draft, Counting, Submitted, Approved, Rejected, Posted, Cancelled), `SnapshotTakenAt`, `CreatedByUserId`, `SubmittedByUserId`/`At`, `Version` (konkurensi), kolom audit `IdentityModel` | → `MstDrugStorageLocation` |
| `PhmStockOpnameLine` | `Id`, `StockOpnameId`, `DrugStockBalanceId`, `DrugId`, `DrugBatchId`, `Status`, `SnapshotQuantity`, `SnapshotVersion`, `SnapshotTakenAt` (per baris, supaya baris dapat dimuat ulang sendiri), `PhysicalQuantity` (null = belum), `Variance`, `Note`, `CountedByUserId`/`At`, `AdjustmentMutationId` (null sampai diposting) | → sesi, saldo, batch, obat, mutasi |
| `PhmStockOpnameDecision` | `Id`, `StockOpnameId`, `Decision` (Approved/Rejected), `Reason` (wajib untuk Rejected), `DecidedByUserId`, `DecidedAt` | → sesi |
| `PhmStockOpnameReport` (berita acara) | `Id`, `StockOpnameId`, `DocumentNumber`, `IssuedAt`, `IssuedByUserId`, ringkasan tersimpan | → sesi (satu-satu) |
| Tambahan opsional | Baris "temuan tanpa saldo" (obat/batch yang ditemukan tetapi tidak punya saldo di lokasi) | → obat, batch |

**Indeks**

- Unik `OpnameNumber`; unik `DocumentNumber`.
- Unik (`StockOpnameId`, `DrugStockBalanceId`): satu saldo satu baris per sesi.
- Unik terfilter satu sesi aktif per lokasi (Status ∈ Draft, Counting, Submitted), atau per lokasi + status bila opname per status diizinkan.
- Indeks (`StorageLocationId`, `OpnameDate`) untuk daftar; (`StockOpnameId`) pada baris dan keputusan.

**Aturan transaksi**

1. Penyesuaian hanya dari sesi berstatus Approved, dalam satu transaksi basis data, lalu sesi menjadi Posted. Sesi Posted tidak dapat diposting lagi.
2. Setiap baris berselisih diposting lewat `AdjustAsync`/`ApplyMutationAsync` dengan:
   - `SourceDocumentType = StockOpname`, `SourceDocumentId` = sesi;
   - `IdempotencyKey = opname-{sesi}-{baris}`;
   - `ExpectedVersion` = versi saldo yang diperiksa ulang saat posting.
   Saldo yang berubah sejak dihitung ditolak (konflik), bukan ditimpa.
3. Larangan self-approval: penyetuju ≠ pembuat sesi dan ≠ setiap penghitung baris.
4. Izin baru yang diusulkan: `StockOpname:Create`, `StockOpname:Count`, `StockOpname:Approve`, `StockOpname:Post`. Atau pakai `DrugStock:Adjust` untuk posting, bila pemilik memilih tanpa izin baru.
5. Audit lewat `LoggerService` untuk buat, ajukan, setujui/tolak (beserta alasan), posting, dan terbitkan berita acara.
6. Berita acara hanya untuk sesi Posted; nomornya dibuat oleh basis data atau sequence, bukan hitung baris + 1.

**Keputusan yang masih terbuka**

- Pembekuan pergerakan stok lokasi selama opname: dibekukan, atau dibiarkan bergerak dengan aturan Hitung ulang seperti sekarang.
- Siapa penyetuju (izin saja, atau juga terikat unit/lokasi).
- Apakah temuan tanpa saldo boleh dibukukan dan dengan dokumen apa.
- Toleransi selisih (misalnya selisih kecil disetujui otomatis atau tetap manual).

## 7. Daftar berkas

**Backend** (`Ikbal`, HEAD `a3b2ed08`): **26 berkas** (23 sebelumnya + 3 baru).

- **Baru:** `DTOs/StockOpnameDtos.cs`, `Services/StockOpnameQueries.cs`, `Tests/.../LogisticsReportTests/StockOpnameTests.cs`.
- **Diubah lagi:** `Controllers/DrugStockController.cs`, `Services/DrugStockService.cs`, dokumen ini.

**Frontend** (`Ikbalv2`, HEAD `109a40831`): **97 berkas** (82 sebelumnya + 15 baru).

| Status | Berkas |
| --- | --- |
| Baru | `src/app/health-services/pharmacy-management/stock-opname/page.jsx` |
| Baru | `src/app/health-services/pharmacy-management/stock-opname/stock-opname-client.jsx` |
| Baru | `src/components/view/health-services/pharmacy-management/stock-opname-view/stock-opname-view.jsx` |
| Baru | `…/stock-opname-view/components/stock-opname-sheet-panel.jsx` |
| Baru | `…/stock-opname-view/components/stock-opname-review-panel.jsx` |
| Baru | `…/stock-opname-view/components/stock-opname-report-panel.jsx` |
| Baru | `…/stock-opname-view/components/stock-opname-list-panel.jsx` |
| Baru | `…/stock-opname-view/components/stock-opname-blocked-actions.jsx` |
| Baru | `…/stock-opname-view/components/stock-opname-stock-status.jsx` |
| Baru | `src/lib/hooks/health-services/pharmacy-management/use-stock-opname.jsx` |
| Baru | `src/lib/services/health-services/pharmacy-management/stock-opname.service.js` |
| Baru | `src/lib/constants/health-services/pharmacy-management/stock-opname-constant.js` |
| Baru | `src/utils/health-services/pharmacy-management/stock-opname-utils.js` |
| Baru | `src/style/health-services/pharmacy-management/stock-opname.module.css` |
| Baru | `tests/unit/stock-opname.test.mjs` |
| Diubah lagi | `src/utils/menu-sidebar/menu-items.jsx` (satu baris `pathname` pada 4.4) |
| Diubah lagi | `tests/unit/logistics-module-navigation.test.mjs` (20 tautan; 4.4 → halaman opname) |

**Jangan ikut di-commit:** `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.

## 8. Blocker dan langkah berikutnya

| Butir | Status |
| --- | --- |
| Sesi, hasil hitung, persetujuan, penyesuaian, dan berita acara resmi | **BLOCKED_SCHEMA** (bagian 6) |
| Uji backend Stock Opname | Belum dijalankan; perlu izin `dotnet build`/`dotnet test` pada proyek uji terisolasi |
| Pemeriksaan peramban halaman opname | Belum; perlu izin dev server lokal seperti pada FIN-LGS-079 |
| Penautan submenu 4.4 ke halaman mode terbatas | Menunggu persetujuan pemilik (bagian 4) |
| Pembekuan stok selama opname, penyetuju, temuan tanpa saldo, toleransi selisih | Keputusan bisnis |

**Urutan berikutnya yang disarankan:**

1. Izinkan uji backend dan pemeriksaan peramban untuk membuktikan bagian yang sudah dibuat.
2. Putuskan skema dan keputusan terbuka di bagian 6.
3. Buat migrasi oleh pihak yang berwenang.
4. Aktifkan simpan → ajukan → setujui → posting → berita acara, satu per satu, memakai aturan hitung dan `StockOpnameCalculator` yang sudah ada.


# STOCK OPNAME — UJI TERISOLASI, SKEMA DUA TABEL, DAN PENUTUPAN (FIN-LGS-088 s.d. FIN-LGS-090)

**Tanggal:** 25 November 2026
**Yang dijalankan (izin eksplisit pemilik):**

- `dotnet build` dan `dotnet test` pada proyek uji terisolasi (SQLite dalam memori);
- dev server frontend lokal (`next dev`, hanya `127.0.0.1:3100`) dengan API tiruan di peramban;
- uji unit frontend dan ESLint.

**Yang tidak dilakukan:** commit, push, PR, merge, migrasi, menjalankan server backend, koneksi ke basis data apa pun (termasuk operasional), penulisan data, perubahan modul selain Logistik.

**Status: BLOCKED_SCHEMA** untuk transaksi Stock Opname. Persistence belum ada, jadi transaksi Stock Opname **belum siap**. Mode terbatas (lembar hitung, hitung selisih, verifikasi baca, draf berita acara) sudah diuji di build, test, dan browser, dan **siap ditinjau pemilik**.

## 1. Prinsip bisnis yang disetujui pemilik

| No. | Prinsip | Diterapkan sekarang | Diterapkan saat persistence ada |
| --- | --- | --- | --- |
| 1 | Tidak ada pembekuan gudang; saldo atau pergerakan berubah → hitung ulang sebelum keputusan | Hitung selisih menandai baris "Hitung ulang" bila jumlah saldo berbeda dari lembar **atau** ada mutasi sesudah lembar dimuat | Diperiksa lagi saat ajukan dan saat setujui; baris yang berubah memblokir keputusan |
| 2 | Penyetuju = pemegang izin persetujuan Stock Opname; self-approval dilarang | Teks aturan pada tombol nonaktif | Izin `StockOpname:Approve`; penyetuju ≠ pembuat, ≠ pengaju, ≠ setiap penghitung baris |
| 3 | Temuan tanpa saldo dicatat sebagai temuan, tidak menambah stok | Tombol "Catat Temuan" nonaktif beserta aturannya; teks batasan backend diperbarui | Baris jenis Temuan di tabel detail; tidak pernah diposting |
| 4 | Toleransi selisih 0 | `StockOpnameCalculator.VarianceTolerance = 0` (backend) dan `STOCK_OPNAME_VARIANCE_TOLERANCE = 0` (frontend). Uji baru: selisih 0,001 tetap "Lebih" | Setiap selisih ≠ 0 masuk persetujuan dan posting |
| 5 | Berita acara dari sesi yang selesai | Draf tetap bertanda "bukan dokumen resmi"; tombol terbit nonaktif | Hanya untuk sesi berstatus Selesai |
| 6 | Penyesuaian diposting sekali setelah persetujuan | Tidak ada endpoint posting | Persetujuan dan posting dalam satu transaksi. Ganda dicegah oleh indeks unik mutasi yang **sudah ada** |
| 7 | Submenu tetap dibuka dalam mode terbatas | Submenu 4.4 tetap bertaut ke halaman mode terbatas | — |

**Tafsiran prinsip 1** yang perlu dikonfirmasi: "versi pergerakan" dibaca sebagai **mutasi buku stok**. Kolom `Version` saldo juga naik saat reservasi, padahal barang yang ditahan masih ada di rak. Bila `Version` dipakai sebagai pemicu hitung ulang, setiap reservasi akan memaksa hitung ulang. Karena itu `Version` dipakai sebagai penjaga konkurensi saat posting, bukan pemicu hitung ulang.

## 2. FIN-LGS-088 — Hasil build dan test aktual

| Uji | Hasil sebenarnya |
| --- | --- |
| Build proyek uji Logistik terisolasi (`build14.log`) | **0 error**, 599 warning (sama dengan pembanding). Warning yang menyebut berkas Logistik hanyalah peringatan paket NuGet (ImageSharp), bukan dari kode |
| Uji Logistik terisolasi (`run13.trx`) | **157/157 lulus** pada percobaan pertama: 146 lama + 11 Stock Opname (10 kasus FIN-LGS-084 + 1 kasus toleransi 0) |
| Proyek `QuilvianSystemBackend.PharmacyTests` utuh | **Tidak terkompilasi**: 4 error di berkas uji Billing/Inpatient Financial Clearance (`Billing.Constants` dan `EncounterStatus.InProgress` tidak ada lagi di source). Milik modul lain; tidak diubah |
| Seluruh uji Farmasi yang dapat dikompilasi (proyek terisolasi kedua, semua berkas PharmacyTests kecuali 13 berkas Billing/Finance/Inpatient yang tidak terkompilasi) — percobaan 1 (`all1.trx`) | 926 kasus: 902 lulus, **24 gagal** |
| Pemilahan 24 kegagalan | **3 terkait Logistik** (`PermissionMatrixTests`, diperbaiki, lihat di bawah). **21 di modul lain:** `FinanceEmployeeBenefitArAutoIntakeServiceTests` (7, constraint CHECK SQLite), `WorkforceBenefitOwnerAcceptanceTests` (11), `FinanceReceivableInstallmentPlanListTests` (2), `EmployeeBenefitReceivableReportTests` (1). Keempat berkas itu tidak merujuk tipe Logistik mana pun, dan tidak ada berkas berubah di luar Logistik, jadi kegagalannya tidak berasal dari tahap ini. Tidak diperbaiki karena di luar wewenang |
| Seluruh uji Farmasi — percobaan 2 sesudah perbaikan (`all2.trx`) | **927 kasus: 906 lulus, 21 gagal.** Seluruh 21 kegagalan adalah 21 kasus modul lain yang sama seperti di atas. Semua uji Logistik (157) dan `PermissionMatrixTests` (272 kasus termasuk theory) **lulus** |
| Proyek uji modul lain (SQLite dalam memori; variabel koneksi PostgreSQL bukti dihapus dari environment) | `NutritionTests` **219/219 lulus**; `OperatingRoomTests` **142/142 lulus**; `PatientManagementTests` **tidak terkompilasi** (1 error: konstruktor `NewbornPatientRegistrationService` meminta `hospitalSites`, di `Infrastructure/BayiUji.cs`). Modul PatientManagement; tidak diubah |
| Uji unit frontend | **86/86 lulus** |
| ESLint berkas Stock Opname, menu, dan uji | **0 error, 0 warning** |

**Tiga kegagalan Logistik dan perbaikannya** (`Tests/QuilvianSystemBackend.PharmacyTests/PermissionMatrixTests.cs`):

| Kegagalan | Sebab | Perbaikan |
| --- | --- | --- |
| `Endpoint_pengubah_data_tidak_pernah_cukup_dengan_izin_baca` | `POST opname/variance-preview` memakai izin baca | Pengecualian bernama satu-satu, mengikuti pola `SlidingScaleExecutionController.Preview` yang sudah ada. Alasannya: endpoint POST hanya karena badan 2000 baris, dan terbukti nol mutasi oleh uji perilaku. Ditambah uji penjaga `Hitung_selisih_stock_opname_memang_hanya_membaca` (POST, `DrugStock:Read`, `IsPersisted` false); pengecualian wajib dicabut bila endpoint mulai menyimpan |
| `Jumlah_controller_dan_endpoint_farmasi_sesuai_yang_tercatat` | Angka terkunci 23/137 sejak 5 Oktober 2026 | Diperbarui ke **24/155**. Rinciannya: +1 controller dan +1 endpoint `PrescriptionStockReservationController` (b3b0d323, sudah di HEAD); StockRequest +8, DrugStock +8, DrugUsage +1 (Logistik; 9 di antaranya belum di-commit). Setiap tambahan lulus uji login, satu izin, sumber daya Farmasi, dan izin baca untuk GET di kelas yang sama |
| `Tidak_ada_pasangan_sumber_daya_dan_tindakan_kembar` | Angka 137/85 | Diperbarui ke **155/89**. Empat pasangan baru sudah ada di HEAD (`PrescriptionStockReservation:Read`, `StockRequest:Approve`, `StockRequest:AuthorizeEmergency`, `StockRequest:ViewApprovalReadiness`); perubahan yang belum di-commit tidak menambah pasangan |

Catatan: dua angka kunci itu sebenarnya sudah tidak cocok di HEAD (24 controller, 146 endpoint) sebelum tahap ini. Angka baru ikut menghitung satu controller resep milik Farmasi, karena uji ini menghitung seluruh Farmasi.

**13 berkas yang dikeluarkan** dari proyek uji kedua (tidak terkompilasi terhadap source saat ini, milik Billing/Finance/Inpatient): `BillingCashRoundingPrecisionTests`, `BillingInpatientDepositWorkspaceTests`, `BillingSettlementEncounterCompletionTests`, `InpatientFinancialClearanceEndToEndTests`, `BillingAutoCalculationOnChargeTests`, `BillingEncounterPaymentHistoryTests`, `BillingFinalizationReadinessCodesTests`, `BillingInvoiceDepositPaymentStatusTests`, `BillingInvoiceEligibilityTests`, `BillingZeroPatientObligationTests`, `FinanceReceivableInvoiceBatchTests`, `InpatientDepositPaymentOrderTests`, `PatientBillingEstimateItemUnitTests`.

### 2.1 Pemeriksaan browser

**Cara aman.**

- Hanya `next dev` lokal yang dinyalakan (terikat `127.0.0.1:3100`); backend tidak dijalankan.
- Chromium headless (Playwright repo) mencegat seluruh panggilan API: identitas uji, izin superadmin, satu lokasi contoh, empat saldo contoh, dan jawaban hitung selisih tiruan. Satu baris dibuat berubah di server agar "Hitung ulang" muncul.
- Host luar dibatalkan (tercatat **0** permintaan keluar).
- Skrip dan tangkapan layar ada di scratchpad sesi (sementara). Dev server dihentikan sesudahnya.

| Pemeriksaan | Desktop 1440×900 | Mobile 390×844 |
| --- | --- | --- |
| Sidebar menandai "Stock Opname" (`aria-current`) | Ya | Ya |
| 4 tab (Daftar Opname, Lembar Hitung, Verifikasi, Berita Acara); tab awal Lembar Hitung | Ya | Ya |
| Pilih lokasi → Mulai Hitung → 4 baris; panggilan `GET drug-stock/balances?storageLocationId=…&pageNumber=1&pageSize=200` | Ya | Ya |
| Isian −1 menampilkan "Isi dengan angka 0 atau lebih."; isian 0 diterima | Ya | Ya |
| Hitung Selisih → "Selisih sudah dihitung"; Verifikasi menampilkan Kurang, Hitung ulang, Belum dihitung, serta peringatan "Belum lengkap" | Ya | Ya |
| Berita Acara: "Draf — bukan dokumen resmi", "belum bernomor", 1 baris berselisih | Ya | Ya |
| Daftar Opname kosong + "Lembar di halaman ini (belum disimpan)" | Ya | Ya |
| Isian tetap ada sesudah berpindah tab | Ya (`10`, `0`, `5`) | Ya |
| Mengubah isian → peringatan "Isian berubah sejak selisih dihitung" | Ya | Ya |
| Muat ulang lembar berisi → konfirmasi di layar | Ya | Ya |
| Tombol nonaktif: Simpan Hasil Hitung, Catat Temuan, Ajukan Verifikasi, Setujui & Posting Penyesuaian, Tolak, Terbitkan Berita Acara; kolom alasan nonaktif | Semua nonaktif | Semua nonaktif |
| Endpoint yang dipanggil | Hanya `GET balances` dan `POST opname/variance-preview` | Sama |
| Kunci baru di localStorage / sessionStorage | 0 / 0 | 0 / 0 |
| Geser horizontal halaman | Tidak ada (tabel menggeser di dalam wadahnya) | Tidak ada |
| Galat konsol | 0 | 0 |
| Saldo dijawab 403 | "Ups! Akses Ditolak" | — |
| Hitung selisih dijawab 422 `PHM038` | Pesan backend tampil di panel galat | — |

**Masalah tampilan yang ditemukan dan diperbaiki:**

| Temuan | Perbaikan |
| --- | --- |
| Celah kosong ±300 px di tab Verifikasi antara "Batas hasil ini" dan saringan "Tampilkan" (desktop dan mobile). `.toolbarFilters` (`flex: 1 1 22rem`) berada langsung di kartu berarah kolom, sehingga basis 22rem terbaca sebagai tinggi | Saringan dibungkus `.toolbar` (berarah baris) seperti di Lembar Hitung. Diperiksa ulang: celah 21 px (jarak kartu biasa) |
| Kartu ringkasan di mobile tampil satu per baris, sehingga halaman memanjang | **Tidak diubah.** Komponen bersama `SummaryGrid` sengaja memaksa satu kolom di layar kecil (media query di `base-data-components.module.css`), sama di seluruh aplikasi. Percobaan mengecilkan `minWidth` tidak berpengaruh, jadi dikembalikan |
| Skrip mula-mula melaporkan badge "Hitung ulang" tidak ada | Bukan cacat halaman: badge ditampilkan berhuruf kapital oleh CSS. Pencocokan skrip dibuat tidak peka huruf besar/kecil |

Footer aplikasi yang melayang tampak menutupi bagian tengah tangkapan halaman penuh. Ini artefak tangkapan halaman penuh dan sama di seluruh aplikasi; di viewport biasa konten tidak tertutup.

## 3. FIN-LGS-089 — Evaluasi skema: cukup dua tabel

**Kesimpulan: cukup dua tabel**, yaitu sesi (`PhmStockOpname`) dan detail perhitungan (`PhmStockOpnameLine`), ditambah tabel mutasi yang **sudah ada**. Tabel keputusan dan tabel berita acara dari rancangan FIN-LGS-084 tidak diperlukan.

| Kebutuhan | Disimpan di | Alasan cukup |
| --- | --- | --- |
| Keputusan (setuju/tolak, oleh siapa, kapan, alasan) | Kolom di sesi: `DecidedByUserId`, `DecidedAt`, `DecisionReason`, `Status` | Satu sesi hanya punya **satu keputusan final**. Penolakan bersifat final dan hitung ulang memakai sesi baru (`PreviousStockOpnameId`), jadi riwayat keputusan terbaca dari rantai sesi tanpa tabel riwayat |
| Audit | Kolom audit `IdentityModel` (`CreateBy`, `CreateDateTime`, `UpdateBy`, …) + `LoggerService.AuditAsync` | `LoggerService` hanya menulis ke log aplikasi, **tidak** ke basis data. Karena itu fakta yang harus dapat ditelusuri (pembuat, pengaju, penghitung, penyetuju, waktu) disimpan di kolom; log aplikasi hanya pelengkap untuk perubahan antara (isian yang ditimpa sebelum diajukan) |
| Snapshot | Kolom per baris: `SnapshotQuantity`, `SnapshotVersion`, `SnapshotTakenAt`, serta salinan nama obat, kode, batch, ED, dan satuan (pola `DrugNameSnapshot` milik permintaan stok) | Dengan waktu snapshot per baris, satu baris yang berubah dapat dimuat ulang sendiri tanpa membuang isian baris lain. Ini keterbatasan mode terbatas saat ini |
| Pencegahan posting ganda | (1) status Submitted → Completed dijaga `Version` sesi dalam satu transaksi; (2) **indeks unik yang sudah ada** `PhmDrugStockMutation (SourceDocumentType, CorrelationId)` dengan `CorrelationId = opname-{lineId}`; (3) `AdjustmentMutationId` per baris diisi sekali | Lapis (2) menolak mutasi kedua di tingkat basis data walaupun dua persetujuan berjalan bersamaan |
| Kartu stok | Mutasi existing: `SourceDocumentType = StockOpname`, `SourceDocumentId` = sesi, `Reason = "Stock Opname {nomor} baris {n}: {catatan}"` | Kartu stok, rekonsiliasi buku mutasi, dan laporan mutasi langsung membaca mutasi ini tanpa perubahan |
| Berita acara | Disusun dari sesi Selesai + baris + mutasinya; nomor berita acara = `OpnameNumber`, tanggal = `DecidedAt` | Baris dikunci sesudah diajukan dan berisi salinan nama, sehingga isi berita acara dapat disusun ulang persis sama kapan pun |
| Temuan tanpa saldo | Baris `LineType = Finding` (tanpa `DrugStockBalanceId`) | Tidak pernah diposting; tampil di berita acara. Tindak lanjutnya lewat jalur resmi yang ada (penerimaan atau saldo pembuka) oleh petugas berwenang |

### 3.1 Tabel `PhmStockOpname` (sesi)

| Kolom | Tipe | Keterangan |
| --- | --- | --- |
| `Id` | uuid | PK |
| `OpnameNumber` | varchar(30) | `SOP-{yyyyMMdd}-{6 heksa}` (pola nomor permintaan stok; bukan hitung baris + 1); juga nomor berita acara |
| `StorageLocationId` | uuid | FK `MstDrugStorageLocation` |
| `StorageLocationNameSnapshot` | varchar(200) | Salinan nama lokasi untuk berita acara |
| `StockStatusScope` | int null | Cakupan status (null = semua) |
| `OpnameDate` | date | Tanggal opname |
| `Status` | int | Counting, Submitted, Completed, Rejected, Cancelled |
| `PreviousStockOpnameId` | uuid null | Sesi yang ditolak dan dihitung ulang |
| `SubmittedByUserId`, `SubmittedAt` | uuid null, timestamptz null | Pengaju |
| `DecidedByUserId`, `DecidedAt` | uuid null, timestamptz null | Penyetuju/penolak |
| `DecisionReason` | varchar(500) null | Wajib bila Rejected |
| `Version` | int | Token konkurensi |
| kolom `IdentityModel` | — | `CreateBy` = pembuat |

### 3.2 Tabel `PhmStockOpnameLine` (detail perhitungan)

| Kolom | Tipe | Keterangan |
| --- | --- | --- |
| `Id` | uuid | PK; dipakai dalam `CorrelationId` mutasi |
| `StockOpnameId` | uuid | FK sesi |
| `LineNumber` | int | Urutan lembar |
| `LineType` | int | Balance atau Finding |
| `DrugStockBalanceId` | uuid null | FK saldo (null untuk temuan) |
| `DrugId` | uuid | FK obat |
| `DrugBatchId` | uuid null | FK batch (temuan boleh tanpa batch terdaftar) |
| `StockStatus` | int | Siap Pakai, Karantina, Kedaluwarsa, Rusak |
| `DrugCodeSnapshot`, `DrugNameSnapshot`, `BatchNumberSnapshot`, `ExpiryDateSnapshot`, `StockUnitNameSnapshot` | teks/date | Salinan untuk berita acara |
| `SnapshotQuantity` | numeric(18,3) | 0 untuk temuan |
| `SnapshotVersion` | int null | Versi saldo saat snapshot |
| `SnapshotTakenAt` | timestamptz | Per baris |
| `PhysicalQuantity` | numeric(18,3) null | null = belum dihitung |
| `VarianceQuantity` | numeric(18,3) null | Fisik − snapshot, ditetapkan saat diajukan |
| `Note` | varchar(500) null | Catatan |
| `CountedByUserId`, `CountedAt` | uuid null, timestamptz null | Penghitung, untuk larangan self-approval |
| `AdjustmentMutationId` | uuid null | FK mutasi; diisi sekali saat posting |
| kolom `IdentityModel` | — | — |

### 3.3 Indeks

- Unik `OpnameNumber` (terfilter `IsDelete = false`).
- Unik terfilter **satu sesi aktif per lokasi**: `StorageLocationId` dengan `Status IN (Counting, Submitted) AND IsDelete = false`. Tanpa pembekuan gudang, dua sesi pada lokasi yang sama bisa menyesuaikan saldo yang sama dua kali.
- Indeks `(StorageLocationId, OpnameDate)` untuk daftar.
- Unik `(StockOpnameId, LineNumber)`.
- Unik `(StockOpnameId, DrugStockBalanceId)` terfilter `DrugStockBalanceId IS NOT NULL`.
- Unik `AdjustmentMutationId` terfilter tidak null.
- Mutasi: indeks unik existing `(SourceDocumentType, CorrelationId)`; tidak ada indeks baru.

### 3.4 Alur dan aturan transaksi

```
Counting ──ajukan──► Submitted ──setujui (posting sekali)──► Completed ──► berita acara
   │                     └──tolak (alasan wajib, final)──► Rejected ──► sesi baru (PreviousStockOpnameId)
   └──batalkan──► Cancelled
```

1. **Buat:** satu sesi aktif per lokasi; baris saldo dibentuk dari saldo lokasi (semua status, termasuk nol) beserta snapshot.
2. **Hitung:** simpan fisik dan catatan per baris (`CountedBy`/`At`). Baris yang berubah dapat dimuat ulang sendiri; snapshot baru diambil dan isian baris itu dikosongkan. Temuan ditambahkan sebagai baris Finding.
3. **Ajukan:** semua baris saldo sudah dihitung dan tidak ada yang berubah (diperiksa server memakai `StockOpnameCalculator`). Selisih ditetapkan, lalu baris dikunci.
4. **Setujui (satu transaksi):**
   - Penyetuju memegang `StockOpname:Approve` dan bukan pembuat, pengaju, maupun penghitung.
   - Setiap baris saldo diperiksa ulang. Bila ada yang berubah, persetujuan ditolak dengan galat konflik, tidak ada yang tersimpan, dan sesi harus ditolak lalu dihitung ulang.
   - Setiap selisih ≠ 0 (toleransi 0) diposting lewat `ApplyMutationAsync(Adjustment, StockOpname, sesi, CorrelationId = opname-{lineId})`. Aturan existing tetap berlaku: saldo tidak boleh negatif (`PHM028`) dan tidak boleh turun di bawah jumlah yang ditahan (`PHM029`); reservasi harus diselesaikan dulu.
   - `AdjustmentMutationId` diisi, status menjadi Completed, `Version` naik, lalu satu kali `SaveChanges`.
5. **Tolak:** alasan wajib; status Rejected bersifat final.
6. **Berita acara:** hanya untuk Completed.
7. **Audit:** `LoggerService.AuditAsync` di setiap langkah, sebagai pelengkap kolom.

### 3.5 Izin dan endpoint yang diusulkan (belum dibuat)

Izin baru di resource `StockOpname`: `Read`, `Create` (buat, hitung, ajukan, batalkan), `Approve` (setujui/tolak beserta posting). Controller baru `StockOpnameController` akan mengubah angka kunci `PermissionMatrixTests` (25 controller), dan itu memang perlu ditinjau saat itu.

```
GET    /api/v1/health-services/pharmacy-management/stock-opnames                     StockOpname:Read
POST   /api/v1/health-services/pharmacy-management/stock-opnames                     StockOpname:Create
GET    /api/v1/health-services/pharmacy-management/stock-opnames/{id}                StockOpname:Read
PUT    /api/v1/health-services/pharmacy-management/stock-opnames/{id}/counts         StockOpname:Create
POST   /api/v1/health-services/pharmacy-management/stock-opnames/{id}/lines/{lineId}/refresh  StockOpname:Create
POST   /api/v1/health-services/pharmacy-management/stock-opnames/{id}/findings       StockOpname:Create
POST   /api/v1/health-services/pharmacy-management/stock-opnames/{id}/submit         StockOpname:Create
POST   /api/v1/health-services/pharmacy-management/stock-opnames/{id}/approve        StockOpname:Approve
POST   /api/v1/health-services/pharmacy-management/stock-opnames/{id}/reject         StockOpname:Approve
POST   /api/v1/health-services/pharmacy-management/stock-opnames/{id}/cancel         StockOpname:Create
GET    /api/v1/health-services/pharmacy-management/stock-opnames/{id}/report         StockOpname:Read
```

Semua perintah tulis membawa `ExpectedVersion` sesi; persetujuan juga membawa `IdempotencyKey`. Endpoint hitung selisih yang sekarang (`POST drug-stock/opname/variance-preview`) tetap baca saja.

## 4. Bagian yang masih memerlukan migrasi

| Bagian | Butuh migrasi? |
| --- | --- |
| Tabel `PhmStockOpname`, `PhmStockOpnameLine`, beserta FK dan 6 indeks di atas | **Ya** |
| Simpan sesi, hasil hitung, temuan, ajukan, setujui/posting, tolak, batalkan, daftar sesi, berita acara resmi | **Ya** (bergantung pada dua tabel itu) |
| Mutasi penyesuaian hasil opname, kartu stok, laporan mutasi, rekonsiliasi | **Tidak** (tabel dan indeks mutasi sudah ada) |
| Izin `StockOpname:*` | Tidak butuh migrasi skema. Pendaftarannya mengikuti mekanisme registry izin yang berjalan saat aplikasi start, seperti izin lain |
| Lembar hitung, hitung selisih, verifikasi baca, draf berita acara (mode terbatas) | **Tidak** (sudah berjalan) |

## 5. Langkah konkret untuk mengaktifkan transaksi Stock Opname

1. **Pemilik** menyetujui skema bagian 3 dan tafsiran prinsip 1. Prefix `Phm` sudah aktif untuk PharmacyManagement di registry.
2. **Backend (tanpa migrasi):**
   - entity, konfigurasi EF (indeks bagian 3.3), dan `DbSet`;
   - `StockOpnameService` memakai `StockOpnameCalculator` dan `ApplyMutationAsync` existing;
   - `StockOpnameController` dengan izin bagian 3.5;
   - uji SQLite untuk: satu sesi aktif per lokasi, ajukan dengan baris berubah, self-approval (pembuat/pengaju/penghitung), persetujuan bersamaan (indeks unik), posting sekali, `PHM029` saat fisik < ditahan, temuan tidak diposting, berita acara hanya untuk Completed, penolakan final;
   - perbarui angka kunci `PermissionMatrixTests`.
3. **Migrasi:** dibuat dan diterapkan oleh pihak berwenang, karena `dotnet ef` tidak dapat dijalankan di PC ini. Urutannya: DEV dulu, operasional hanya dengan persetujuan tersendiri.
4. **Frontend:**
   - Lembar Hitung beralih dari state halaman ke endpoint sesi;
   - tombol diaktifkan menurut penanda dari backend (`canSubmit`, `canApprove`, …), bukan ditebak di layar;
   - Daftar Opname membaca daftar sesi;
   - Berita Acara membaca `/report` dan menghapus tanda draf hanya untuk sesi Completed;
   - peringatan "mode terbatas" dicabut.
5. **UAT di DEV:** opname nyata satu lokasi kecil, termasuk perubahan stok saat menghitung, penolakan dan hitung ulang, temuan, dan pemeriksaan Kartu Stok serta rekonsiliasi sesudah posting.
6. Keputusan go-live oleh pemilik.

Sampai langkah 3 selesai, transaksi Stock Opname **tidak boleh** dianggap siap.

## 6. Daftar berkas yang berubah pada tahap ini

**Backend** (`Ikbal`, HEAD `a3b2ed08`): total **27 berkas** (26 sebelumnya + 1).

| Status | Berkas | Isi |
| --- | --- | --- |
| Ubah | `Areas/HealthServices/PharmacyManagement/Services/StockOpnameQueries.cs` | `VarianceTolerance = 0`; teks batasan temuan (prinsip 3) dan tanpa pembekuan (prinsip 1) |
| Ubah | `Tests/QuilvianSystemBackend.PharmacyTests/LogisticsReportTests/StockOpnameTests.cs` | Kasus toleransi 0 |
| **Baru dalam daftar** | `Tests/QuilvianSystemBackend.PharmacyTests/PermissionMatrixTests.cs` | Angka kunci 24/155/89, pengecualian `PreviewOpnameVariance`, uji penjaga |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` | Bagian ini |

**Frontend** (`Ikbalv2`, HEAD `109a40831`): tetap **97 berkas**; yang berubah lagi:

| Berkas | Isi |
| --- | --- |
| `src/lib/constants/health-services/pharmacy-management/stock-opname-constant.js` | Tindakan nonaktif sesuai prinsip (Catat Temuan; Setujui & Posting; tolak final; berita acara dari sesi Selesai); `STOCK_OPNAME_VARIANCE_TOLERANCE = 0` |
| `src/components/view/health-services/pharmacy-management/stock-opname-view/components/stock-opname-review-panel.jsx` | Tombol keputusan, teks aturan, perbaikan celah tata letak |
| `src/components/view/health-services/pharmacy-management/stock-opname-view/components/stock-opname-sheet-panel.jsx` | Tombol Catat Temuan |
| `tests/unit/stock-opname.test.mjs` | Asersi prinsip yang disetujui |

**Berkas sementara di luar repo** (scratchpad, tidak untuk di-commit): proyek uji `srq-isolated-tests` dan `pharmacy-all-isolated`, skrip `visual-opname.cjs`, dan tangkapan layar.

**Jangan ikut di-commit:** `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.


# STOCK OPNAME — TRANSAKSI SESI DAN INTEGRASI HALAMAN (FIN-LGS-091 s.d. FIN-LGS-094)

**Tanggal:** 26 November 2026
**Basis kode:** backend `Ikbal` HEAD `3b9c6f84` (tanpa perubahan backend pada tahap 092–094); frontend `Ikbalv2` HEAD `1fa534644` + perubahan lokal yang belum di-commit (bagian 7).

**Status: READY FOR OWNER REVIEW — BLOCKED_SCHEMA.**

Kode transaksi Stock Opname sudah lengkap dari backend sampai layar. Namun tabel `PhmStockOpname` dan `PhmStockOpnameLine` belum dimigrasi, sehingga di lingkungan mana pun setiap endpoint sesi menjawab `503 PHM039` dan halaman tetap berjalan dalam **mode terbatas**. Transaksi **belum boleh dianggap operasional** sampai migrasi dijalankan pihak berwenang dan UAT di DEV lulus.

**Yang tidak dilakukan pada tahap 092–094:** migrasi, akses basis data (operasional maupun lokal), `dotnet build`, `npm run build`, `npm run dev`, perubahan modul lain, commit, push, merge, PR. Server backend dan frontend lokal yang masih berjalan dari tahap sebelumnya dihentikan di awal tahap ini.

## 1. FIN-LGS-091 — Backend transaksi (sudah di `3b9c6f84`)

Ringkasan, karena rinciannya ada di commit:

- **Dua tabel** sesuai rancangan FIN-LGS-089: `PhmStockOpname` (kepala sesi) dan `PhmStockOpnameLine` (baris saldo dan baris temuan).
- **`StockOpnameController`**, route `api/v1/health-services/pharmacy-management/stock-opnames`, 13 endpoint:

| Metode | Path | Izin | Fungsi |
| --- | --- | --- | --- |
| GET | `availability` | `StockOpname:Read` | `READY` / `BLOCKED_SCHEMA` |
| GET | `/` | `StockOpname:Read` | Daftar sesi (saring status, lokasi; paginasi) |
| GET | `{id}` | `StockOpname:Read` | Detail sesi + penanda `canEdit`, `canSubmit`, `canDecide`, `canCancel` |
| GET | `{id}/report` | `StockOpname:Read` | Berita acara, hanya sesi Selesai |
| POST | `/` | `StockOpname:Create` | Buat sesi: potret saldo lokasi, nomor `SOP-000001` dari `NumberSeriesAllocator` |
| PUT | `{id}/counts` | `StockOpname:Create` | Simpan hasil hitung |
| POST | `{id}/lines/{lineId}/refresh` | `StockOpname:Create` | Muat ulang baris yang saldonya bergerak |
| POST / DELETE | `{id}/findings`, `{id}/findings/{lineId}` | `StockOpname:Create` | Catat / hapus temuan |
| POST | `{id}/submit`, `{id}/cancel` | `StockOpname:Create` | Ajukan / batalkan |
| POST | `{id}/approve` | `StockOpname:Approve` | Setujui dan posting penyesuaian sekali ke Kartu Stok (sumber `StockOpname`) |
| POST | `{id}/reject` | `StockOpname:Approve` | Tolak, wajib beralasan, final |

- **Aturan yang dijaga backend:**
  - larangan self-approval (`PHM067`): pembuat, pengaju, dan penghitung baris tidak boleh memutus;
  - izin `StockOpname:Approve` eksplisit, tanpa jalan pintas SuperAdmin (`PHM056`);
  - pemeriksaan perubahan saldo saat keputusan (`PHM055`);
  - toleransi selisih 0;
  - versi sesi (`PHM057`);
  - satu sesi aktif per lokasi (`PHM053`);
  - posting sekali (pemeriksaan service + indeks unik mutasi);
  - audit setiap tindakan lewat `LoggerService`.
- **Hasil uji (dijalankan sebelum commit `3b9c6f84`, tidak diulang pada tahap ini karena `dotnet build`/`dotnet test` dilarang):**
  - uji Logistik terisolasi 172/172 lulus, termasuk 15 uji transaksi baru;
  - seluruh uji Farmasi yang terkompilasi 932/953; 21 yang gagal ada di Finance/Workforce, bukan Logistik;
  - `PermissionMatrixTests`: 25 controller, 168 endpoint, 92 pasangan izin.

## 2. FIN-LGS-092 s.d. 094 — Fitur frontend yang selesai

### 2.1 Pengalih mode di halaman Stock Opname

Halaman bertanya ke `GET stock-opnames/availability` setiap kali dibuka.

| Jawaban backend | Yang tampil | Tombol transaksi |
| --- | --- | --- |
| Sedang memeriksa | "Memeriksa kesiapan Stock Opname..." | Tidak ada |
| `READY` | **Mode transaksi**: Daftar Opname, Lembar Hitung, Verifikasi, Berita Acara (sesi tersimpan) | Aktif menurut izin dan penanda backend |
| `BLOCKED_SCHEMA` (jawaban availability, atau `PHM039` dari tindakan apa pun) | Mode terbatas, judul "tabel Stock Opname belum dimigrasi (BLOCKED_SCHEMA)" | Nonaktif dengan alasan |
| 403 | Mode terbatas, judul "izin transaksi Stock Opname belum dimiliki" (`StockOpname:Read`) | Nonaktif |
| 404 | Mode terbatas, judul "endpoint transaksi Stock Opname belum tersedia" | Nonaktif |
| Galat lain | Mode terbatas, "kesiapan tidak dapat diperiksa" + pesan backend | Nonaktif |

Mode terbatas punya tombol **Periksa kesiapan lagi**. Contoh: setelah migrasi diterapkan di DEV, pengguna cukup menekan tombol itu (atau membuka ulang halaman) untuk pindah ke mode transaksi, tanpa perubahan kode.

### 2.2 Panel Verifikasi (mode transaksi)

- Ringkasan selisih: baris saldo, cocok, lebih, kurang, hitung ulang, temuan.
- Tabel baris yang akan diposting (hanya lebih/kurang), dengan stok sistem, stok fisik, penyesuaian, dan status Kartu Stok ("Diposting saat disetujui" atau "Sudah diposting").
- Tabel temuan tanpa saldo, dengan keterangan bahwa temuan tidak menambah stok.
- Kolom **alasan keputusan** (maks. 500 karakter; wajib untuk Tolak).
- Tombol **Setujui & Posting Penyesuaian** dan **Tolak**, masing-masing dengan konfirmasi di layar.

Tombol keputusan **nonaktif** bila salah satu berlaku, dan alasannya ditampilkan:

1. sesi tidak berstatus Diajukan;
2. pengguna belum memegang `StockOpname:Approve` (atau daftar izin belum termuat);
3. pengguna adalah pembuat, pengaju, atau penghitung sesi (petunjuk self-approval);
4. backend menyatakan `canDecide = false`, misalnya ada baris yang saldonya bergerak (teks dari `decisionBlockedReason`).

Ketiga pemeriksaan di layar hanya petunjuk. Backend tetap memeriksa ulang izin, self-approval, dan perubahan saldo saat tombol ditekan; jawaban galatnya ditampilkan apa adanya.

### 2.3 Panel Berita Acara resmi

- Hanya tampil untuk sesi berstatus **Selesai**; sesi lain mendapat keterangan "belum dapat diterbitkan".
- Isi dari `GET stock-opnames/{id}/report`: nomor dokumen, lokasi, tanggal, cakupan status, waktu selesai, pembuat, penghitung, penyetuju, catatan penyetuju, ringkasan, jumlah penyesuaian yang diposting, tabel penyesuaian, dan tabel temuan.
- Tanda tangan diisi nama dari backend.
- Tombol **Cetak Berita Acara** memakai dialog cetak peramban. Aturan `@media print` hanya mencetak dokumen; tombol dan menu tidak ikut.
- Tanda "Draf — bukan dokumen resmi" tetap hanya ada di mode terbatas.

### 2.4 Pesan hasil tindakan

- Pesan berhasil muncul **hanya setelah backend menjawab sukses**; layar tidak pernah menampilkan transaksi yang belum tersimpan sebagai berhasil.
- Pesan galat memakai teks backend, ditambah petunjuk tindak lanjut untuk:
  - `PHM055`: saldo bergerak; data dimuat ulang, tolak lalu hitung ulang;
  - `PHM057`: sesi diubah orang lain; data dimuat ulang;
  - `PHM067`: minta pengguna lain;
  - `PHM056`: izin diberikan lewat Akses Role.
- Membuka sesi dari daftar langsung menuju tab yang sesuai: Dihitung → Lembar Hitung; Diajukan/Ditolak → Verifikasi; Selesai → Berita Acara.

## 3. Integrasi API dan izin

| Kebutuhan | Endpoint / izin | Lokasi di frontend |
| --- | --- | --- |
| Kesiapan | `GET availability` | `use-stock-opname-session.jsx`, `describeAvailabilityError` |
| Daftar, detail, berita acara | `GET /`, `GET {id}`, `GET {id}/report` | hook yang sama |
| Hitung, temuan, ajukan, batal | `PUT counts`, `POST refresh/findings/submit/cancel`, `DELETE findings` | hook + panel Lembar Hitung |
| Keputusan | `POST approve`, `POST reject` | hook + panel Verifikasi |
| Izin tampilan | `usePermission("StockOpname", "Create" / "Approve")` | panel Lembar Hitung dan Verifikasi |
| Petunjuk self-approval | `createdByUserId`, `submittedByUserId`, `lines[].countedByUserId` dibandingkan dengan pengguna masuk | `isUserInvolvedInSession` |

Tidak ada endpoint backend baru dan tidak ada perubahan kontrak. Frontend membaca `errors.code` (bentuk `ApiResponse.Fail`) dengan cadangan `data.code`.

## 4. Hasil pemeriksaan aktual

| Pemeriksaan | Perintah | Hasil |
| --- | --- | --- |
| Uji unit Logistik frontend (5 berkas) | `node --test --import ./tests/helpers/register.mjs` pada berkas uji bertanda FIN-LGS/LGS | **94/94 lulus** (86 sebelumnya + 8 baru) |
| Uji unit Stock Opname saja | `tests/unit/stock-opname.test.mjs` | 19/19 lulus |
| ESLint | berkas Stock Opname, LogisticsWorkspace, hook, util, konstanta, service, uji | 0 galat, 0 peringatan |
| Build backend / frontend | — | **Tidak dijalankan** (dilarang pada tahap ini) |
| Pemeriksaan browser | — | **MANUAL TEST: NOT FEASIBLE**: `npm run dev` dilarang pada tahap ini. Panel baru belum dilihat di peramban; perlu dicek pada UAT DEV |
| Uji backend | — | Tidak diulang; tidak ada perubahan backend sejak `3b9c6f84` |

Delapan uji baru:

1. kesiapan membedakan 403, 404, dan galat lain, serta `BLOCKED_SCHEMA`;
2. mode transaksi hanya untuk `READY`;
3. petunjuk self-approval: pembuat, pengaju, penghitung (tanpa beda huruf besar/kecil);
4. baris yang diposting hanya lebih/kurang, temuan tidak pernah;
5. aturan alasan keputusan;
6. syarat nonaktif tombol keputusan;
7. pesan berhasil hanya sesudah jawaban backend, dan petunjuk kode galat;
8. berita acara hanya dari sesi Selesai, dapat dicetak, dan tanpa tanda draf.

## 5. Blocker yang tersisa

| No. | Blocker | Pemilik | Dampak bila belum selesai |
| --- | --- | --- | --- |
| 1 | **Migrasi** tabel `PhmStockOpname` dan `PhmStockOpnameLine` (termasuk indeks unik bersaring dan check constraint). `dotnet ef` tidak dapat dijalankan di PC ini | Pihak berwenang (DBA / pemegang migrasi) | Semua endpoint sesi menjawab `503 PHM039`; halaman tetap mode terbatas |
| 2 | **Izin** `StockOpname:Read`, `StockOpname:Create`, `StockOpname:Approve` ke jabatan yang tepat lewat Akses Role, setelah registry izin terisi saat backend start | Admin akses | Tanpa Read: mode terbatas (403). Tanpa Create: tidak bisa membuat/menghitung. Tanpa Approve: tombol keputusan nonaktif |
| 3 | **Penegakan indeks unik di PostgreSQL** (satu sesi aktif per lokasi; anti-posting-ganda) baru terbukti setelah migrasi. Harness SQLite melepas indeks unik bersaring, jadi yang teruji baru konfigurasinya; lapisan service sudah teruji | Diverifikasi saat UAT | Risiko rendah, karena service juga menolak |
| 4 | **UAT di DEV**: opname satu lokasi kecil, stok bergerak saat menghitung, tolak lalu hitung ulang, temuan, cetak berita acara, lalu cek Kartu Stok dan rekonsiliasi | Pemilik + petugas gudang | Belum ada bukti runtime untuk panel baru |
| 5 | **Keputusan go-live** | Pemilik | — |

Sebaiknya migrasi dibuat terpisah dan lebih dulu, supaya migrasi orang lain di cabang `Ikbal` tidak ikut membawa dua tabel ini.

## 6. Keterbatasan yang tetap tercatat

- Petunjuk self-approval di layar memakai `userId` dari sesi login. Bila nilai itu kosong, petunjuk ini tidak menonaktifkan tombol, tetapi backend tetap menolak dengan `PHM067`.
- Cetak memakai dialog peramban; tidak ada berkas PDF dari server.
- Pemeriksaan di atas hanya uji unit dan pemeriksaan statis atas kode sumber; perilaku render panel belum diuji di peramban pada tahap ini.

## 7. Daftar berkas yang berubah pada tahap 092–094

**Backend** (`Ikbal`): hanya berkas laporan ini.

| Status | Berkas |
| --- | --- |
| Ubah | `docs/module-blueprints/logistik/task/report/FIN-LGS-LAPORAN-FINISHING.md` |

**Frontend** (`Ikbalv2`, basis `1fa534644`): 10 berkas.

| Status | Berkas | Isi |
| --- | --- | --- |
| Baru | `src/components/view/health-services/pharmacy-management/stock-opname-view/session/stock-opname-session-review-panel.jsx` | Panel Verifikasi |
| Baru | `src/components/view/health-services/pharmacy-management/stock-opname-view/session/stock-opname-session-report-panel.jsx` | Berita acara resmi + cetak |
| Baru | `src/components/view/health-services/pharmacy-management/stock-opname-view/session/stock-opname-session-workspace.jsx` | Tab mode transaksi, pesan hasil, tab menurut status |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-opname-view/stock-opname-view.jsx` | Pengalih mode READY / BLOCKED_SCHEMA / 403 / 404 / galat |
| Ubah | `src/components/view/health-services/pharmacy-management/stock-opname-view/session/stock-opname-session-list-panel.jsx` | "Buka" meneruskan baris sesi agar tab mengikuti status |
| Ubah | `src/lib/hooks/health-services/pharmacy-management/use-stock-opname-session.jsx` | Sebab `UNAVAILABLE` (`reason`) |
| Ubah | `src/lib/constants/health-services/pharmacy-management/stock-opname-constant.js` | `STOCK_OPNAME_UNAVAILABLE_REASON`, `STOCK_OPNAME_PERMISSION` |
| Ubah | `src/utils/health-services/pharmacy-management/stock-opname-utils.js` | `describeAvailabilityError`, `isUserInvolvedInSession`, `getSessionDifferenceLines`, `validateDecisionReason` |
| Ubah | `src/style/health-services/pharmacy-management/stock-opname.module.css` | Aturan cetak berita acara |
| Ubah | `tests/unit/stock-opname.test.mjs` | 8 uji baru; daftar berkas Stock Opname diperluas |

**Jangan ikut di-commit:** `SeedData/ICD10/*`, `Storage/uploads/*`, `tests/e2e/zz-*.mjs`, `test-results/.last-run.json`.
