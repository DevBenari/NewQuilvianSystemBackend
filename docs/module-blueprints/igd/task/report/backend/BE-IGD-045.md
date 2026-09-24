# `BE-IGD-045` — Dokter penanggung jawab ditetapkan, dialihkan, dan dicari berdasarkan waktu

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-045` |
| Gelombang | `MVP-5` · `EPIC IGD-04` · slice `IGD-S06` |
| Status | ✅ **SELESAI — 17 September 2026.** Implementation Complete, **Build Verified**, **Runtime Verified**. Build lulus nol error; **dua belas skenario uji API seluruhnya `PASS`**, termasuk concurrency S12. Dijalankan pemilik — [evidence](../evidence/2026-09-17-verifikasi-runtime-be-igd-045.md). **UAT belum dan tidak diklaim** |
| Branch | `rizkiG` `a5f4f4b8` + working tree |
| Requirement | `FR-IGD-016` sampai `FR-IGD-021` |
| Keputusan | `IGD-DEC-082` (`approved`), `IGD-DEC-107`, `IGD-DEC-116`, `IGD-DEC-117`, `IGD-DEC-129`, `IGD-DEC-130`, `IGD-DEC-131`, `IGD-DEC-135` |
| Kontrak | API `0.7.0` bagian 3, 3.1, 3.2; validation bagian 3; state bagian 6 |
| Migration | **Nol.** Tabelnya sudah ada dari `BE-IGD-044` |

---

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Prefix registry | `Emg`, lifecycle `ACTIVE / LEGACY` — terdaftar |
| Keberlakuan | **`NEW CODE`** — service, controller, dan DTO baru |
| Pengecualian QBE | Nol |
| Arketipe endpoint | **Transaksi**, sub-proses ter-scope induk (kunjungan IGD) |

### 1.1 QBE ID yang berlaku

| QBE ID | Pemenuhan |
| --- | --- |
| `QBE-SVC-001` | ✅ Seluruh aturan bisnis di `EmergencyDoctorAssignmentService`; controller **nol** akses `DbContext` |
| `QBE-API-001` | ✅ `ApiResponse<T>`, kode status, dan validasi mengikuti boundary yang sudah mapan |
| `QBE-DTO-001` | ✅ Balasan memakai `EmergencyDoctorAssignmentResponse`; entity EF tidak pernah diekspos |
| `QBE-PERM-001` | ✅ `[AccessController]` + `[AccessAction]` + `[AccessPermission]` pada keempat endpoint |
| `QBE-VAL-001` | ✅ Validation bagian 3 aturan 1–5 ditegakkan service |
| `QBE-TXN-001` | ✅ Pengalihan dan penyelarasan encounter satu `SaveChangesAsync` |
| `QBE-LOG-001` | ✅ Penetapan dan pengalihan mencatat log beserta pelaku |
| `QBE-NAM-001`, `QBE-NAM-002` | ✅ Nol `Trx*`; prefix `Emg` |
| `QBE-ENT-001`…`003`, `QBE-CFG-001` | ➖ Tidak berlaku — nol entity baru pada task ini |
| `QBE-CODE-001`…`006` | ➖ Tidak berlaku — tabel ini tidak punya nomor bisnis |
| `QBE-PAGE-001` | ➖ Riwayat satu kunjungan, bukan daftar lintas kunjungan; paging tidak dipakai |
| `QBE-OPT-001` | ➖ Transaksi bukan isi dropdown — standar endpoint transaksi bagian 1 |

**Larangan hardcode role dipatuhi.** Nol `IsInRole`, nol nama peran, nol nama departemen, nol
`UserType`. Kewenangan sepenuhnya dari metadata Access yang dicentang admin.

## 2. Berkas yang disentuh

| Berkas | Sifat |
| --- | --- |
| `Services/EmergencyDoctorAssignmentService.cs` | **Baru** |
| `Controllers/EmergencyDoctorAssignmentController.cs` | **Baru** |
| `DTOs/EmergencyDoctorAssignmentDtos.cs` | **Baru** |
| `Program.cs` | **+1 baris** — lihat bagian 3 |

**Endpoint Registrasi `PATCH /patient-encounters/{id}/doctor` tidak disentuh** (acceptance 11).
Diverifikasi: `git diff` pada `Areas/HealthServices/RegistrationManagement/` kosong.

## 3. Baris `Program.cs` yang ditambahkan — wajib dicantumkan `IGD-DEC-131`

```csharp
builder.Services.AddScoped<EmergencyDoctorAssignmentService>();
```

Disisipkan tepat sesudah `AddScoped<EmergencyUnitAuthorityService>()`. **Nol** pembersihan,
**nol** penataan ulang, dan **nol** perubahan pendaftaran DI lain — persis batas yang diberikan
`IGD-DEC-131`.

## 4. Permukaan endpoint

| Method | Path | Hak akses | Kode status |
| --- | --- | --- | --- |
| `GET` | `/?emergencyVisitId=` | `EmergencyDoctorAssignment : Read` | `200`, `400` |
| `GET` | `/active?emergencyVisitId=&at=` | `EmergencyDoctorAssignment : Read` | `200`, `400`, `404` |
| `POST` | `/` | `EmergencyDoctorAssignment : Create` | `201`, `400`, `404`, `409` |
| `POST` | `/{id}/handover` | `EmergencyDoctorAssignment : Update` | `200`, `400`, `404`, `409` |

Base URL `api/v1/health-services/emergency-installation-management/emergency-doctor-assignments`,
sesuai API bagian 3.

### 4.1 Tiga delta terhadap standar endpoint transaksi — disengaja

| Delta | Alasan |
| --- | --- |
| Kunjungan disebut lewat **query** `emergencyVisitId`, bukan path `/by-visit/{id}` seperti standar bagian 2.3 | Kontrak API bagian 3 sudah `approved` dan mengunci bentuk ini. Skill melarang mendefinisikan ulang kontrak yang disetujui secara sepihak |
| Nol `GET /filters/metadata` dan `GET /summary` | Riwayat penugasan dibaca per kunjungan dari layar triase, bukan sebagai worklist berfilter. Menambahkannya berarti permukaan yang tidak dikonsumsi siapa pun (`QBE-OPT-001`) |
| `POST /` membalas `201`, sedangkan controller Emergency lain konsisten `200` | Acceptance 1 dan API bagian 3 menyebut `201`. Kontrak menang atas kebiasaan modul; dicatat di sini supaya pembaca berikutnya tahu ini disengaja |

## 5. Acceptance criteria

| # | Kriteria | Status | Tempat di source |
| ---: | --- | :-: | --- |
| 1 | `POST /` dokter pertama → `201`; dokter tidak ada/tidak aktif → `400` | ✅ | `TetapkanAsync`; `PeriksaDokterAsync` |
| 2 | `POST /` pada kunjungan yang sudah punya dokter → `409` | ✅ | `TetapkanAsync` pemeriksaan `sudahAdaBerjalan` |
| 3 | `handover` menutup baris lama dan membuka baris baru dalam **satu transaksi** | ✅ | `AlihkanAsync` — satu `SaveChangesAsync` |
| 4 | Pengalihan tanpa alasan → `400` | ✅ | `AlihkanAsync` pemeriksaan pertama |
| 5 | Waktu lebih awal dari kedatangan → `400` | ✅ | Kedua jalur membandingkan `visit.ArrivalDateTime` |
| 6 | Dua penetapan bersamaan → satu `409`, tidak pernah dua dokter aktif | ✅ | `SimpanAsync` menangkap `23505` pada index unik bersyarat |
| 7 | `RegPatientEncounter.DoctorId` sama dengan dokter aktif, transaksi sama | ✅ | `SelaraskanDokterEncounterAsync`, dilacak sebelum `SaveChangesAsync` |
| 8 | `GET /` riwayat urut waktu lengkap dengan alasan | ✅ | `AmbilRiwayatAsync` |
| 9 | `GET /active` tanpa `at`, dengan `at`, dan `404` bila tidak ada | ✅ | `AmbilAktifAsync`; `GetActive` |
| 10 | Nol endpoint pencabutan tanpa pengganti | ✅ | Controller hanya punya empat endpoint di bagian 4 |
| 11 | Endpoint Registrasi tidak diubah | ✅ | `git diff` pada folder Registrasi kosong |
| 12 | `doctorName` dan `assignedByName` diproyeksikan, nol `N+1` | ✅ | `ProyeksiResponse` — satu expression, satu kueri |
| 13 | Penugasan dihitung **hanya** dari `EffectiveFrom`/`EffectiveTo`; nol `IsActive` | ✅ | Nol kemunculan `IsActive` pada ketiga berkas baru |

## 6. Validasi

| Jenis | Hasil |
| --- | --- |
| Governance preflight | ✅ bagian 1 |
| Keseimbangan struktur ketiga berkas baru | ✅ diperiksa |
| Nol `IsActive` sebagai status penugasan | ✅ diperiksa lewat penelusuran |
| Nol akses `DbContext` dari controller | ✅ diperiksa |
| `dotnet build` | ✅ `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` **LULUS 17 September 2026** — `Build succeeded with 207 warning(s)`, **nol error**. Jumlah warning **sama persis** dengan build sebelum `BE-IGD-044` dan `BE-IGD-045` ada, jadi ketiga berkas baru menyumbang **nol warning baru** — termasuk nol warning nullable dari perubahan `AssignmentReason` menjadi `string?` |
| Uji API | ✅ **12 dari 12 `PASS` 17 September 2026**, dijalankan pemilik — [evidence](../evidence/2026-09-17-verifikasi-runtime-be-igd-045.md) |
| Automated test | Tidak ada — proyek test backend dihapus (`IGD-DEC-110`) |
| Kueri basis data oleh agent | **Nol** |

## 7. Dua belas skenario uji API

Base URL:
`https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-doctor-assignments`

### 7.1 Prasyarat

| Kode | Prasyarat | Cara memastikan |
| --- | --- | --- |
| `P1` | **Hak akses sudah dicentang.** Controller ini baru, jadi kemampuannya baru muncul di layar Pengaturan pada boot pertama sesudah build | Jalankan backend sekali, lalu buka Pengaturan, Manajemen Role, Akses Role. Centang `EmergencyDoctorAssignment` untuk Read, Create, dan Update pada peran yang dipakai menguji. **Tanpa ini seluruh skenario membalas `403`, bukan hasil yang diharapkan** |
| `P2` | Satu kunjungan IGD yang **belum** punya penugasan berjalan | Tabel `EmgDoctorAssignment` masih kosong (`BE-IGD-048` belum jalan), jadi kunjungan IGD mana pun memenuhi syarat. Catat `Id` dan `ArrivalDateTime`-nya |
| `P3` | Dua dokter aktif berbeda | `MstDoctor` dengan `IsActive` benar dan `IsDelete` salah. Sebut `DOKTER_A` dan `DOKTER_B` |
| `P4` | Token petugas yang sama dipakai seluruh skenario | Supaya `assignedByName` konsisten dan dapat diperiksa |

| `P5` | Kunjungan pada `P2` sudah tiba **beberapa jam lalu**, bukan baru saja | Supaya `T0 + 10 menit` dan `T0 + 60 menit` keduanya di masa lalu, dan skenario temporal tidak bergantung pada kecepatan Anda menekan tombol |

Sebut `VISIT_ID` untuk `P2`, dan `T0` untuk `ArrivalDateTime`-nya.

**Waktu ditetapkan eksplisit, bukan dibiarkan memakai jam server.** Penetapan dan pengalihan bisa
berselang beberapa detik saja, sehingga rumus seperti *"satu menit sebelum pengalihan"* dapat
jatuh **sebelum** penetapan pertama dan membuat S8 salah menjawab `404`. Karena itu:

| Penanda | Nilai | Dipakai |
| --- | --- | --- |
| `WAKTU_AWAL` | `T0 + 10 menit` | dikirim S1, dibaca kembali dari responsnya |
| `WAKTU_UJI` | `T0 + 30 menit` | dikirim S8 sebagai `at` |
| `WAKTU_ALIH` | `T0 + 60 menit` | dikirim S5, dibaca kembali dari responsnya |

Ketiganya memenuhi `WAKTU_AWAL < WAKTU_UJI < WAKTU_ALIH` secara pasti, berapa pun jeda Anda
mengerjakannya.

### 7.2 Skenario

#### S1 — Menetapkan dokter pertama

| | |
| --- | --- |
| Endpoint | `POST /` |
| Prasyarat | `P1`, `P2`, `P3` |
| Acceptance | 1, 12 |

```json
{
  "emergencyVisitId": "VISIT_ID",
  "doctorId": "DOKTER_A",
  "effectiveFrom": "WAKTU_AWAL"
}
```

**Harapan `201`.** Body `data` memuat `effectiveTo` bernilai `null`, `assignmentReason` bernilai
`null`, `doctorName` berisi **nama lengkap DOKTER_A**, dan `assignedByName` berisi **nama Anda** —
keduanya nama, bukan GUID, bukan kosong. `effectiveFrom` pada respons wajib sama dengan yang
dikirim. **Catat `data.id` sebagai `ASSIGNMENT_ID`** dan pastikan `data.effectiveFrom` =
`WAKTU_AWAL`.

#### S2 — Menetapkan dokter kedua pada kunjungan yang sama

| | |
| --- | --- |
| Endpoint | `POST /` |
| Prasyarat | S1 berhasil |
| Acceptance | 2 |

```json
{
  "emergencyVisitId": "VISIT_ID",
  "doctorId": "DOKTER_B"
}
```

**Harapan `409`** dengan pesan persis:
*"Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter."*

#### S3 — Menetapkan dokter yang tidak ada

| | |
| --- | --- |
| Endpoint | `POST /` |
| Prasyarat | S1 berhasil |
| Acceptance | 1 |

```json
{
  "emergencyVisitId": "VISIT_ID",
  "doctorId": "99999999-9999-9999-9999-999999999999"
}
```

**Harapan `400`** dengan pesan persis: *"Dokter tidak ditemukan atau tidak aktif."*

Perhatikan urutannya: pemeriksaan dokter berjalan **sebelum** pemeriksaan dokter aktif, jadi
skenario ini benar membalas `400`, bukan `409`.

#### S4 — Mengalihkan tanpa alasan

| | |
| --- | --- |
| Endpoint | `POST /ASSIGNMENT_ID/handover` |
| Prasyarat | S1 berhasil |
| Acceptance | 4 |

```json
{
  "doctorId": "DOKTER_B",
  "assignmentReason": ""
}
```

**Jalankan keempat bentuk masukan kosong ini, satu per satu:**

| # | Body | Harapan |
| ---: | --- | --- |
| 4a | `{ "doctorId": "DOKTER_B" }` — ruas tidak dikirim | `400` |
| 4b | `{ "doctorId": "DOKTER_B", "assignmentReason": null }` | `400` |
| 4c | `{ "doctorId": "DOKTER_B", "assignmentReason": "" }` | `400` |
| 4d | `{ "doctorId": "DOKTER_B", "assignmentReason": "   " }` | `400` |

**Keempatnya wajib membalas pesan yang sama persis:**
*"Alasan pengalihan dokter wajib diisi."*

Bila salah satunya justru membalas `ProblemDetails` berisi daftar galat validasi per ruas
(bentuknya memuat `errors` dan `title`), berarti perbaikan nullability pada DTO belum ikut
terbawa build — lihat bagian 11. Itu **defect**, bukan hasil yang diterima.

#### S5 — Mengalihkan dengan alasan

| | |
| --- | --- |
| Endpoint | `POST /ASSIGNMENT_ID/handover` |
| Prasyarat | S1 berhasil; S4 sudah ditolak |
| Acceptance | 3, 12 |

```json
{
  "doctorId": "DOKTER_B",
  "assignmentReason": "Pergantian jaga sore",
  "effectiveFrom": "WAKTU_ALIH"
}
```

**Harapan `200`.** Body `data` adalah **baris baru**: `doctorName` DOKTER_B, `effectiveTo`
`null`, `assignmentReason` terisi, dan `effectiveFrom` sama dengan yang dikirim. **Catat
`data.id` sebagai `ASSIGNMENT_ID_2`** dan pastikan `data.effectiveFrom` = `WAKTU_ALIH`.

#### S6 — Membaca riwayat

| | |
| --- | --- |
| Endpoint | `GET /?emergencyVisitId=VISIT_ID` |
| Prasyarat | S5 berhasil |
| Acceptance | 3, 8 |

**Harapan `200` dengan tepat dua baris**, urut waktu:

| Baris | `doctorName` | `effectiveTo` | `assignmentReason` |
| ---: | --- | --- | --- |
| 1 | DOKTER_A | **terisi** = `WAKTU_ALIH` | `null` |
| 2 | DOKTER_B | `null` | "Pergantian jaga sore" |

Baris pertama **tidak boleh hilang dan tidak boleh berubah dokternya** — itulah bukti riwayat
tambah-saja `IGD-DEC-082`.

#### S7 — Dokter aktif sekarang

| | |
| --- | --- |
| Endpoint | `GET /active?emergencyVisitId=VISIT_ID` |
| Prasyarat | S5 berhasil |
| Acceptance | 9 |

**Harapan `200`** berisi **DOKTER_B**, bukan DOKTER_A.

#### S8 — Dokter aktif pada waktu sebelum pengalihan

| | |
| --- | --- |
| Endpoint | `GET /active?emergencyVisitId=VISIT_ID&at=WAKTU_UJI` |
| Prasyarat | S5 berhasil |
| Acceptance | 9 |

`WAKTU_UJI` = `T0 + 30 menit`, yang secara pasti berada **di antara** `WAKTU_AWAL` (`T0 + 10`)
dan `WAKTU_ALIH` (`T0 + 60`).

**Harapan `200`** berisi **DOKTER_A**. Inilah pertanyaan *"siapa dokter penanggung jawab pasien
ini tadi?"* yang dijawab tanpa endpoint terpisah (`IGD-DEC-117`).

> **Jangan** memakai rumus *"satu menit sebelum pengalihan"*. Bila S1 dan S5 dikerjakan berselang
> kurang dari satu menit, waktu itu jatuh sebelum penetapan pertama dan endpoint **benar**
> membalas `404` — hasilnya terbaca seolah cacat, padahal skenarionyalah yang keliru.

#### S9 — Dokter aktif pada waktu sebelum penetapan pertama

| | |
| --- | --- |
| Endpoint | `GET /active?emergencyVisitId=VISIT_ID&at=<T0 dikurangi satu jam>` |
| Prasyarat | S5 berhasil |
| Acceptance | 9 |

**Harapan `404`** dengan pesan *"Tidak ada dokter penanggung jawab pada waktu yang ditanyakan."*
Bukan `200` berisi daftar kosong.

#### S10 — Waktu penugasan mendahului kedatangan pasien

| | |
| --- | --- |
| Endpoint | `POST /` |
| Prasyarat | Kunjungan IGD **kedua** yang belum punya penugasan berjalan |
| Acceptance | 5 |

```json
{
  "emergencyVisitId": "VISIT_ID_2",
  "doctorId": "DOKTER_A",
  "effectiveFrom": "<satu hari sebelum ArrivalDateTime kunjungan itu>"
}
```

**Harapan `400`** dengan pesan persis:
*"Waktu penugasan tidak boleh lebih awal dari waktu kedatangan pasien."*

Dipakai kunjungan kedua supaya tidak tertolak `409` lebih dulu.

#### S11 — Nilai efektif pada encounter ikut berpindah

| | |
| --- | --- |
| Endpoint | — (pemeriksaan basis data, **oleh Anda**) |
| Prasyarat | S5 berhasil, dan `VISIT_ID` tertaut encounter |
| Acceptance | 7 |

```sql
SELECT e."DoctorId"
FROM public."RegPatientEncounter" e
JOIN public."EmgVisit" v ON v."EncounterId" = e."Id"
WHERE v."Id" = 'VISIT_ID';
```

**Harapan:** nilainya sama dengan **DOKTER_B**, bukan DOKTER_A. Ini membuktikan `FR-IGD-020` —
nilai efektif pada encounter selalu sama dengan dokter aktif, dan diperbarui dalam transaksi
yang sama.

#### S12 — Dua penetapan pertama yang datang bersamaan

| | |
| --- | --- |
| Endpoint | `POST /` dua kali, **hampir bersamaan** |
| Prasyarat | Kunjungan IGD **ketiga** (`VISIT_ID_3`) yang belum punya penugasan berjalan; `DOKTER_A` dan `DOKTER_B` |
| Acceptance | 6 |

**Mengapa S2 tidak cukup.** S2 adalah permintaan **berurutan**: permintaan kedua dikirim sesudah
yang pertama selesai tersimpan, sehingga ia ditolak pemeriksaan `sudahAdaBerjalan` di service.
Itu jalur yang **berbeda** dari dua permintaan yang tiba bersamaan, tempat keduanya sama-sama
lolos pemeriksaan service lalu bertabrakan di index unik bersyarat basis data. Hanya skenario ini
yang menguji jalur tersebut.

Contoh cara menjalankan dua permintaan hampir bersamaan di PowerShell:

```powershell
$url = "https://localhost:7184/api/v1/health-services/emergency-installation-management/emergency-doctor-assignments"
$headers = @{ Authorization = "Bearer <TOKEN>"; "Content-Type" = "application/json" }

$kirim = {
    param($url, $headers, $visitId, $doctorId)
    $body = @{ emergencyVisitId = $visitId; doctorId = $doctorId } | ConvertTo-Json
    try {
        $r = Invoke-WebRequest -Uri $url -Method Post -Headers $headers -Body $body -SkipCertificateCheck
        [pscustomobject]@{ Status = $r.StatusCode; Body = $r.Content }
    } catch {
        [pscustomobject]@{ Status = $_.Exception.Response.StatusCode.value__; Body = $_.ErrorDetails.Message }
    }
}

$a = Start-Job -ScriptBlock $kirim -ArgumentList $url, $headers, "VISIT_ID_3", "DOKTER_A"
$b = Start-Job -ScriptBlock $kirim -ArgumentList $url, $headers, "VISIT_ID_3", "DOKTER_B"
Wait-Job $a, $b | Out-Null
Receive-Job $a
Receive-Job $b
```

Pada PowerShell 5.1, `-SkipCertificateCheck` tidak tersedia; pakai sertifikat dev yang sudah
dipercaya, atau jalankan dari dua jendela terminal secara bersamaan. Yang penting **keduanya
berangkat sebelum salah satunya selesai** — bila satu sudah selesai lebih dulu, yang terjadi
adalah S2, bukan S12.

**Invarian yang wajib dipenuhi, diperiksa lewat basis data (oleh Anda):**

```sql
SELECT COUNT(*) AS penugasan_berjalan
FROM public."EmgDoctorAssignment"
WHERE "EmergencyVisitId" = 'VISIT_ID_3'
  AND "EffectiveTo" IS NULL;
```

**Harus bernilai tepat `1`.** Bila `2`, index unik bersyarat tidak bekerja dan itu **defect
berat** — catat dan hentikan, jangan lanjut ke frontend.

**Respons permintaan yang kalah — laporkan apa adanya lebih dulu:**

| Yang terjadi | Artinya | Tindakan |
| --- | --- | --- |
| `409` dengan pesan *"Kunjungan ini sudah memiliki dokter penanggung jawab. Gunakan aksi pengalihan dokter."* | Target ideal. `DbUpdateException` tertangkap `SimpanAsync` dan diterjemahkan jadi konflik terkendali | Acceptance 6 **lulus** |
| `500` | `DbUpdateException` **lolos** dari penangkap. Kemungkinan nama constraint tidak cocok, atau `InnerException` bukan `PostgresException` | **Catat sebagai defect `BE-IGD-045`.** Acceptance 6 **tidak boleh** dinyatakan lulus |
| Kode lain | Belum terduga | Laporkan apa adanya; jangan disimpulkan sendiri |

**Jangan mengubah kontrak respons hanya untuk membuat hasilnya menjadi `409`.** Laporkan kondisi
aktualnya lebih dulu; perubahan kontrak menuntut evidence dan keputusan tersendiri.

**Satu hal lagi yang perlu diamati.** Kedua permintaan juga menulis kolom `DoctorId` pada baris
`RegPatientEncounter` yang sama dalam transaksi masing-masing. Bila yang muncul adalah galat
deadlock atau timeout alih-alih `23505`, catat pesannya lengkap — itu jalur kegagalan yang
berbeda dan butuh penanganan yang berbeda pula.

### 7.3 Ringkasan cakupan

| Acceptance | Dibuktikan skenario |
| ---: | --- |
| 1 | S1, S3 |
| 2 | S2 |
| 3 | S5, S6 |
| 4 | S4 |
| 5 | S10 |
| 6 | **S12** — skenario concurrency khusus. **S2 tidak menghitung**: ia berurutan dan berhenti di pemeriksaan service, bukan di index basis data |
| 7 | S11 |
| 8 | S6 |
| 9 | S7, S8, S9 |
| 10 | Pemeriksaan permukaan: controller hanya punya empat endpoint |
| 11 | `git diff` pada folder Registrasi kosong |
| 12 | S1, S5 |
| 13 | Penelusuran source: nol `IsActive` pada ketiga berkas baru |

## 8. Definition of Done

| Butir | Status |
| ---: | --- |
| Acceptance 1–13 terpetakan ke source | ✅ |
| Baris DI `Program.cs` dicantumkan (`IGD-DEC-131`) | ✅ bagian 3 |
| Laporan tracked ada | ✅ berkas ini |
| Roadmap dan traceability diperbarui | ✅ |
| QBE preflight | ✅ bagian 1 |
| **Butir 10 DoD — approval pemilik modul lain** | ✅ **terbuka.** `IGD-DEC-082` `approved` 17 September 2026; `IGD-DEC-135` memberi Product/Domain Owner wewenang approval peran yang belum ditunjuk. Keduanya wajib ditinjau ulang bila perannya kelak diisi |
| `dotnet build` dan uji API | **Belum** — status task 🟡 |
| UAT | **Tidak diklaim** |

## 9. Risiko yang tersisa

| Risiko | Keadaan |
| --- | --- |
| Menulis tabel milik Registrasi | `RegPatientEncounter.DoctorId` ditulis dari modul IGD. Disahkan API bagian 3 dan `IGD-DEC-107`; hanya satu kolom, dalam transaksi yang sama, dan endpoint Registrasi sendiri tidak disentuh |
| Kunjungan tanpa encounter | Dilewati tanpa galat; penugasannya tetap tersimpan pada riwayat IGD. Perlu dilihat pemilik saat uji layar apakah perilaku ini yang diinginkan |
| Riwayat kosong untuk kunjungan lama | Tabelnya masih kosong — `IGD-OQ-092` dan `BE-IGD-048`. `GET /` pada kunjungan lama membalas daftar kosong, dan `GET /active` membalas `404` sampai backfill dijalankan |

## 10. Task berikutnya

`FE-IGD-027` — layar triase memakai endpoint ini, bukan `PATCH /patient-encounters/{id}/doctor`
milik Registrasi. Sesudah itu, penetapan dokter baru akan selalu menghasilkan baris riwayat,
sehingga celah yang dicatat `BE-IGD-048` berhenti bertambah.

## 11. Perbaikan dari review sendiri — `AssignmentReason`

Ditemukan dalam dua tahap. Tahap kedua membatalkan anggapan bahwa tahap pertama sudah cukup.

### 11.1 Tahap pertama — mencabut `[Required]` eksplisit

`HandoverEmergencyDoctorRequest.AssignmentReason` semula memakai `[Required]`. Dengan
`[ApiController]`, ModelState yang tidak sah ditolak otomatis sebagai `ProblemDetails` generik,
sehingga pesan kontrak *"Alasan pengalihan dokter wajib diisi."* tidak pernah sampai ke
pemanggil. Atribut itu dicabut.

### 11.2 Tahap kedua — ternyata belum cukup: `[Required]` implisit

Proyek ini memakai `Nullable enable` **tanpa** menyetel
`SuppressImplicitRequiredAttributeForNonNullableReferenceTypes` pada `Program.cs`. Pada keadaan
itu, ASP.NET Core MVC **menambahkan `[Required]` implisit** ke setiap properti reference type
yang non-nullable.

Artinya `public string AssignmentReason` **tetap wajib** walaupun atribut eksplisitnya sudah
dicabut, dan tiga dari empat bentuk masukan kosong tetap ditolak lebih dulu oleh ModelState.

| Bentuk masukan | Dengan `string` | Dengan `string?` |
| --- | :-: | :-: |
| Ruas tidak dikirim | ProblemDetails generik | Pesan kontrak ✅ |
| `null` | ProblemDetails generik | Pesan kontrak ✅ |
| `""` | ProblemDetails generik | Pesan kontrak ✅ |
| `"   "` | Pesan kontrak ✅ | Pesan kontrak ✅ |

**Perbaikan yang berlaku sekarang:**

```csharp
[MaxLength(500)]
public string? AssignmentReason { get; set; }
```

`MaxLength` dipertahankan karena 500 memang batas kolom, dan atribut itu mengabaikan `null`
sehingga tidak menghidupkan kembali penolakan dini. Pemeriksaan kewajiban menjadi **satu-satunya
gerbang** di service:

```csharp
var alasanPengalihan = request.AssignmentReason?.Trim();
if (string.IsNullOrEmpty(alasanPengalihan))
    return ... 400 "Alasan pengalihan dokter wajib diisi." ...
```

Nilai yang tersimpan memakai `alasanPengalihan` yang sudah di-`Trim`, jadi spasi di tepi tidak
ikut masuk basis data.

**Dibuktikan oleh S4a sampai S4d**, bukan oleh pembacaan source saja.

### 11.3 Temuan yang tidak diperbaiki di sini

`CancelEmergencyDepartureRequest.CancellationReason` pada modul yang sama memakai `[Required]`
berdampingan dengan pemeriksaan `IsNullOrWhiteSpace` di service, jadi ia punya celah yang persis
sama. **Di luar lingkup `BE-IGD-045`** dan tidak disentuh; dicatat supaya tidak hilang.

### 11.4 Cakupan perubahan

Hanya satu properti DTO dan satu blok pemeriksaan di service. **Nol perubahan kontrak**, nol
endpoint berubah, nol perubahan pada dua belas skenario selain S4.
