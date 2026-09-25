# Laporan Perubahan Backend — `BE-HMD-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-19` |
| Judul | Uji Kepatuhan Kontrak API Otomatis, Penegakan Otorisasi Endpoint, dan Audit Trail Server |
| Slice | Lintas — Task Lintas Potong |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.7 |
| Trace | `NFR-005`, `NFR-006`, `NFR-007`; `contracts/permission-audit-matrix.md` seluruh bagian; seluruh berkas `contracts/` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-01` s.d. `BE-HMD-18` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 2, berkas diubah 0, logika 1, kontrak API 1, database 0, keamanan 2, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | Tidak ada source yang ditulis khusus untuk task ini; pemeriksaan atas source `BE-HMD-01` s.d. `18` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | 🟡 **Sebagian** — kriteria 1 dan 2 terpetakan ke source; kriteria 3 terpenuhi sebagian (alamat IP hanya tersimpan di database untuk 1 dari 10 peristiwa). Butuh keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-PERM-001`, `QBE-API-001`, `QBE-LOG-001`, `QBE-AUD-001`, `QBE-DTO-001` |

---

## 1. Masalah yang diperbaiki

Modul yang menyimpan status Hepatitis B, Hepatitis C, dan HIV pasien tidak boleh punya satu pun
endpoint yang terbuka tanpa izin, dan setiap keputusan penting — penolakan, penghentian,
pengesahan — harus dapat ditelusuri siapa pelakunya bertahun-tahun kemudian.

Task ini membuktikan hal itu atas 100 endpoint yang dibangun `BE-HMD-01` s.d. `18`.

---

## 2. Proses bisnis

Task ini adalah pemeriksaan, bukan alur pengguna. Urutannya:

1. **Pemindaian reflektif atribut akses.** Skrip membaca seluruh 18 controller di
   `Areas/HealthServices/HemodialysisManagement/Controllers/` dan setiap action-nya, lalu
   memeriksa: `[Authorize]` pada kelas; `[AccessAction]` dan `[AccessPermission]` pada setiap
   action; argumen ke-1 `[AccessPermission]` sama persis dengan `ControllerName`; argumen ke-2 sama
   persis dengan argumen ke-1 `[AccessAction]`; `AccessType` hanya `Read/Create/Update/Delete`.
2. **Pencocokan kontrak.** Setiap baris endpoint `api-contract.md` dicocokkan method, path, dan hak
   aksesnya dengan source.
3. **Pemeriksaan pencatatan dan privasi.** Titik pencatatan logger, isi payload-nya, dan jejak
   tahan lama di database untuk sepuluh peristiwa penting.

**Mengapa satu controller per Resource.** Layar Akses Role mendaftarkan aksi per `ControllerName`.
Karena itu 18 Resource hak akses dilayani 18 controller, beberapa di antaranya berbagi base URL
yang sama (misalnya enam controller di bawah `hemodialysis-sessions`). Roadmap menyebut
"7 controller" — jumlah itu tidak lagi berlaku.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- 18 controller dan `HmdHttp.cs`; `Constants/HemodialysisPermissions.cs`
- `contracts/api-contract.md` (78 baris endpoint), `permission-audit-matrix.md` bagian 5, 6, dan 7
- `Services/Logging/LoggerService.cs`, `Program.cs` (Serilog, `UseAuthentication`/`UseAuthorization`)
- `MrcClinicalDocumentIntegrity` (kolom `SignatureIpAddress`), kolom pelaku pada tabel `Hmd*`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada. Pencatatan logger sudah dipasang di `HmdHttp.RespondAsync` pada task-task sebelumnya |

Skrip audit reflektif dijalankan dari folder kerja sementara di luar repository — bukan project
test, bukan kelas di dalam aplikasi — sesuai `TEST_POLICY.md`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 78 dari 78 endpoint kontrak ada dengan hak akses sama persis; 22 endpoint delta tercatat pada laporan task masing-masing |
| Database | Tidak ada perubahan |
| Keamanan/Auth | Terverifikasi — lihat bagian 5 |

---

## 4. Dokumentasi endpoint

Seluruh endpoint didokumentasikan pada laporan task yang membangunnya. Ringkasan per `[Tags]`:

| Nilai `[Tags]` | Controller | Endpoint |
| --- | --- | ---: |
| `Health Services / Hemodialysis Management / Hemodialysis Order` | `HmdOrderController` | 10 |
| `Health Services / Hemodialysis Management / Hemodialysis Episode` | `HmdEpisodeController`, `HmdEligibilityController`, `HmdVascularAccessController`, `HmdSerologyController`, `HmdIsolationController` | 16 |
| `Health Services / Hemodialysis Management / Hemodialysis Prescription` | `HmdPrescriptionController` | 6 |
| `Health Services / Hemodialysis Management / Hemodialysis Schedule` | `HmdScheduleController` | 8 |
| `Health Services / Hemodialysis Management / Hemodialysis Session` | `HmdSessionController`, `HmdObservationController`, `HmdMedicationController`, `HmdComplicationController`, `HmdRecordController` | 25 |
| `Health Services / Hemodialysis Management / Hemodialysis Unit Readiness` | `HmdUnitReadinessController` | 6 |
| `Health Services / Hemodialysis Management / Master Data / Hemodialysis Machine` | `HmdMachineController` | 10 |
| `Health Services / Hemodialysis Management / Master Data / Hemodialysis Station` | `HmdStationController` | 9 |
| `Health Services / Hemodialysis Management / Master Data / Hemodialysis Setting` | `HmdSettingController` | 2 |
| `Health Services / Hemodialysis Management / Master Data / Hemodialysis Checklist Item` | `HmdChecklistItemController` | 8 |
| **Jumlah** | **18** | **100** |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Audit reflektif atribut akses | 18 controller, 100 action, **0 masalah atribut**; 18 `[Authorize]`, 0 `[AllowAnonymous]` | `PASS` | Skrip audit 22 September 2026 |
| Pencocokan kontrak | 78/78 endpoint kontrak ada; 0 hak akses berbeda; 22 endpoint delta | `PASS` | Skrip audit 22 September 2026 |
| Titik pencatatan logger | 59 = 57 endpoint selain `GET` + 2 pengecualian `GET` (`serology-reviews`, `GET /{id}` sesi); pengulangan Billing termasuk yang 57 | `PASS` | `grep RespondAsync` per controller |
| Payload logger bebas kolom sensitif | Hanya id baris, controller, aksi, pelaku, dan ringkasan jumlah | `PASS` | `HmdHttp.cs`; `permission-audit-matrix.md` bagian 7 |
| Tempat log aplikasi | `LoggerService` menulis lewat Serilog ke console dan berkas; tidak ada `DbContext` | Fakta | `LoggerService.cs`, `Program.cs` baris 83 dan 155–156 |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 12.19 WIB |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Uji runtime `401`/`403` | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | Suite `HemodialysisPermissionAndContractTests` (>30 test case) **tidak dibuat — dikecualikan atas keputusan pengguna 22 September 2026**; diganti audit reflektif di atas |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime `401`/`403` — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

### Sepuluh peristiwa penting — jejak di database

Roadmap dan `permission-audit-matrix.md` bagian 6 menyebut daftar yang sedikit berbeda. Keduanya
dipetakan:

| Peristiwa (roadmap) | Jejak tahan lama di database | Alamat IP di database |
| --- | --- | :---: |
| Buat permintaan | `HmdOrder` kolom audit `CreateBy/CreateDateTime`, peminta | Tidak |
| Tolak permintaan | `HmdOrder.DecisionReason`, `DecisionByUserId`, `DecisionAt` | Tidak |
| Aktivasi episode | `HmdEpisode.ActivatedAt/ActivatedByUserId` | Tidak |
| Aktivasi resep | `HmdPrescription.ActivatedAt/ActivatedByUserId`, `SupersededByPrescriptionId` | Tidak |
| Ubah status mesin | Satu baris `HmdMachineStatusHistory` dengan alasan dan pelaku | Tidak |
| Pernyataan kesiapan unit | `HmdUnitReadiness.DeclaredByUserId/DeclaredAt`, `NotReadyReason` | Tidak |
| Mulai sesi | `HmdSession.StartedAt/StartedByUserId`, `IdempotencyKey` | Tidak |
| Penghentian sesi | `HmdSession.StopReason/StopNote/EndedAt/EndedByUserId` | Tidak |
| Pengesahan catatan | `HmdSession.SignedByUserId/SignedAt/RecordHash` + baris `MrcClinicalDocumentIntegrity` | **Ya** — `SignatureIpAddress`, `SignatureDeviceInfo` |
| Serah terima penagihan | `HmdSession.BillingHandoffStatus/At/Error` + fakta di Billing | Tidak |

Peristiwa tambahan pada kontrak — pelewatan butir checklist, dokumentasi diselesaikan, penutupan
episode, dan koreksi sesudah pengesahan — juga melekat pada barisnya (`OverrideReason`,
`DocumentedByUserId`, `ClosureReason`, baris addendum Rekam Medis).

Untuk seluruh peristiwa, alamat IP, id pengguna, nama aksi, dan payload ringkas **ada** di log
aplikasi, tetapi log aplikasi bukan database dan masa simpannya terbatas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. 100% action method terbukti beratribut otorisasi; tidak ada endpoint terbuka tanpa izin | Terpenuhi | 100/100 action, 0 masalah atribut; 18/18 controller `[Authorize]`, 0 `[AllowAnonymous]` |
| 2. Tanpa autentikasi → `401`; token tanpa klaim hak akses → `403` | Terpenuhi (source) | `[Authorize]` + `UseAuthentication`/`UseAuthorization`; `[AccessPermission]` menolak `403`. Runtime `NOT RUN` |
| 3. Sepuluh peristiwa menulis jejak audit **ke basis data** dengan **alamat IP**, id pengguna, nama aksi, dan payload ringkas tanpa data serologi | **Belum terpenuhi penuh** | Id pengguna, waktu, dan isi keputusan tersimpan di database untuk 10 dari 10 peristiwa, tanpa data serologi. **Alamat IP tersimpan di database hanya untuk 1 dari 10** (pengesahan); 9 lainnya hanya di log aplikasi |
| DoD: reflection test hak akses lulus 100% | Audit reflektif lulus 100%; suite test otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |
| DoD: seluruh 10 event audit terbukti tercatat | Sebagian — lihat kriteria 3 | — |
| DoD: kontrak Swagger tervalidasi otomatis | Dicocokkan skrip terhadap `api-contract.md`, bukan terhadap dokumen Swagger yang dihasilkan runtime | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kriteria 3 roadmap bertentangan dengan kontrak yang disetujui: `permission-audit-matrix.md` bagian 6 menetapkan jejak "melekat pada baris data itu sendiri" dan tidak mewajibkan alamat IP, sedangkan roadmap meminta IP di database. `QBE-AUD-001` juga menuntut audit database terpisah dari log aplikasi, sehingga log aplikasi tidak boleh dianggap pengganti |
| Masalah yang diketahui | **Perlu keputusan pemilik**, salah satu: (a) terima kontrak apa adanya — kriteria 3 dianggap terpenuhi oleh jejak pada baris data, dan IP cukup di log aplikasi; atau (b) tambah penyimpanan IP untuk sembilan peristiwa lain — butuh kolom atau tabel baru, perubahan kamus data, dan migration baru |
| Risiko tersisa | Tanpa suite test otomatis, endpoint baru yang lupa diberi atribut akses tidak menggagalkan build; `Invoke-QbeConformanceCheck.ps1` tidak memeriksa `QBE-PERM-001`. Mitigasi sementara: jalankan ulang audit reflektif setiap kali controller Hemodialisa berubah |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Keputusan pemilik atas pilihan (a) atau (b); uji runtime `401`/`403` oleh pemilik |
