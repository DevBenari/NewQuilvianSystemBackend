# Laporan Perubahan Backend — `BE-IGD-049`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-049` |
| Judul | Nama pelaku pada event kepergian |
| Slice | `IGD-S05` · `EPIC IGD-06` · `MVP-4` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.12 |
| Trace | **`IGD-DEC-137`** (`approved` Rizki Gunawan, 21 September 2026); pola `IGD-DEC-129` dan `BE-IGD-046`; `IGD-DEC-065`, `IGD-DEC-066`; `FR-IGD-036`…`043`; `IGD-EV-123` butir 2 |
| Contract version | API **`0.9.0`** — `draft`, **aditif** (naik dari `0.8.0`); bagian baru `2.4` |
| Dependency | `BE-IGD-033` ✅, `BE-IGD-034` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: cakupan repository 0, berkas diperiksa 1 (± 15), berkas diubah 1 (3 source + dokumen), logika bisnis 1 (kueri batch), kontrak API 2 (menambah ruas), database 1 (hanya perilaku query), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — target tulis: source backend dan `docs/module-blueprints/igd/**`. Wewenang **build** diberikan pemilik 21 September 2026 (malam, "lakukan build saja"). **Tidak** ada wewenang commit, push, merge, pindah branch, migration, atau tulis basis data |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `267b56a0` pada branch `rizkiG` — **working tree belum di-commit** |
| Tanggal | 21 September 2026 (malam) |
| Status | 🟡 **Implementation Complete, Build Verified — Runtime Not Verified.** `dotnet build` → **0 Error(s), 207 Warning(s)** (sama dengan baseline; nol warning pada berkas yang diubah). Acceptance 1–4 dan 6–8 terpetakan ke source; **runtime parsial (21 September 2026, larut malam): jalur `GET` daftar terbukti lewat layar `FE-IGD-017` — pelaku tampil `SuperAdmin`.** **Acceptance 5 (satu kueri) dan uji API S2–S5 belum dijalankan** — milik pemilik. UAT belum dan tidak diklaim |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, lifecycle `ACTIVE / LEGACY` — entri **ada** (registry baris 19), nol blocker `QBE-MOD-002` |
| Keberlakuan | `TOUCHED LEGACY` — controller, service, dan DTO yang sudah ada; **nol entity, nol tabel, nol prefix baru** |
| QBE ID yang berlaku | `QBE-SVC-001` (pengambilan nama berada di service; controller tidak menyentuh context), `QBE-API-001` (envelope `ApiResponse<T>` dan kode status tidak berubah), `QBE-DTO-001` (entity EF tidak terekspos), `QBE-PERM-001` (metadata `Access*` tidak berubah) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-CODE-*`, `QBE-DB-*` (tanpa entity/migration); `QBE-LOG-001` (pengayaan nama pada respons baca; endpoint yang mengubah state tetap memakai `LogAsync` yang sudah ada) |
| Governance | `AGENTS.md`, `rules/backend/*`, kontrak rekayasa, dan registry terbaca — bukan `BLOCKED`. Branch `rizkiG` — sama dengan yang dipakai modul ini pada seluruh task sebelumnya |

---

## 1. Masalah yang diperbaiki

Layar riwayat kejadian kepergian menampilkan kolom **Pelaku** sebagai GUID mentah, sebab respons event hanya
memuat `recordedByUserId` dan `approvedByUserId`. Ini temuan `IGD-EV-123` butir 2 dan **dikonfirmasi lewat layar
oleh pemilik 21 September 2026**: kepergian `DEP-260921064559-9B2DB7` menampilkan `0ba84a1a-…` pada ketiga kejadiannya.

*Contoh.* Perawat membuka riwayat kepergian pasien untuk mengetahui siapa yang mencatat "Pasien berangkat".
Sebelum perubahan: `0ba84a1a-2559-49ba-a320-10fb1f399d70`. Sesudah: `Ns. Ani Rahmawati`.

Kontrak tidak menjanjikan nama pada event kepergian (nama hanya ada pada observasi §7 dan penugasan dokter §3.2),
dan frontend tidak punya pencarian pengguna, sehingga `FE-IGD-017` **tidak dapat** diperbaiki dari sisi layar. Pemilik
menyetujui delta backend ini pada 21 September 2026 (`IGD-DEC-137`).

---

## 2. Proses bisnis

**Pelaku:** petugas IGD yang membuka riwayat kepergian pasien.
**Pemicu:** setiap pembacaan atau perubahan kepergian yang mengembalikan event.

1. Backend memuat kepergian beserta event-nya seperti biasa.
2. Backend mengumpulkan seluruh ID pencatat dan penyetuju yang **berbeda dan tidak kosong** dari semua event pada respons.
3. Backend menjalankan **satu** kueri ke tabel pengguna untuk ID-ID itu, lalu mengisi `recordedByName` dan `approvedByName`.
4. Respons dikirim dengan ID **dan** nama.

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| Pengguna pencatat sudah tidak ada di tabel pengguna | `recordedByName = null`; layar menampilkan tanda hubung |
| Kejadian tidak butuh penyetuju (`approvedByUserId` kosong) | `approvedByName = null` |
| `recordedByUserId = Guid.Empty` (klaim pengguna tidak terbaca saat dicatat) | `recordedByName = null` — tidak pernah dikirim `Guid.Empty` sebagai nama |
| `DisplayName` pengguna kosong | Jatuh ke `UserName`, lalu `Email`, lalu `UserCode` |
| Tidak ada satu pun ID (respons tanpa event) | **Nol** kueri nama |
| Pengguna sudah tidak aktif | Nama tetap tampil (riwayat) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `BACKEND_ENGINEERING_CONTRACT`, registry; kartu `BE-IGD-049`;
`EmergencyDepartureController.cs`; `EmergencyDepartureService.cs`; `EmergencyDepartureDtos.cs`;
`EmgDepartureEvent.cs`; `EmgDepartureEventConfiguration.cs`; `ApplicationUser.cs`; `EmergencyObservationDetailController.cs`
(pola `recordedByName`); `EmergencyUnitAuthorityService.cs` (pola `Set<T>()`); `api-contract.md`; frontend
`emergency-assessment-transfer-tab.jsx` (konsumen, hanya baca).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyDepartureDtos.cs` | `EmergencyDepartureEventResponse` +2 properti `string?`: `RecordedByName`, `ApprovedByName` (+13 baris) |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs` | Tiga overload `ToResponseAsync` (satu kepergian, satu halaman kepergian, satu event), method privat `IsiNamaPelakuAsync` (satu kueri batch), dan `PilihNamaTampilan` (+85 baris). Pemeta statis `ToResponse` **tidak diubah** |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyDepartureController.cs` | Enam titik pemetaan respons memakai `ToResponseAsync` (`GetAll`, `GetById`, `Create`, `Amend`, `Reverse`, `ExecuteDeparture` untuk seluruh aksi). Atribut `[Http*]`, `[Route]`, `[Access*]`, `[Authorize]` **tidak berubah** (14 baris) |
| `docs/module-blueprints/igd/contracts/api-contract.md` | `contract_version` `0.9.0`; bagian baru `2.4` |
| Roadmap, `requirement-traceability.md`, `MODULE-STATUS.md`, laporan ini | Penandaan status |

**Tidak disentuh:** model, konfigurasi EF, `Migrations/`, snapshot, `Program.cs` (service sudah terdaftar), seluruh frontend,
modul lain. Tiga pemakaian `ToResponse` yang tersisa di controller milik **baris pesanan**, bukan event, dan tidak berubah.
Pencarian seluruh `Areas/`, `Controllers/`, dan `Services/` tidak menemukan pemakai lain `EmergencyDepartureEventResponse`
atau `EmergencyDepartureResponse` di luar controller dan service ini — tidak ada respons event yang terlewat.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** `EmergencyDepartureEventResponse` +`recordedByName`, +`approvedByName`; ruas ID tetap. Nol route baru, nol request berubah, nol ruas dihapus. Konsumen lama mengabaikan dua ruas baru. Versi naik `0.8.0` → `0.9.0` |
| Database | **NOT APPLICABLE untuk schema.** Nol entity, kolom, index, migration. Perilaku query: **satu** `SELECT` baca-saja ke `AspNetUsers` per respons yang memuat event. Nol tulis basis data |
| Keamanan/Auth | **Tidak berubah.** Atribut `Access*` dan permission sama persis; tidak ada endpoint atau hak akses baru. Nama pengguna kini ikut terkirim ke pemegang `EmergencyDeparture : Read` — sesuai `IGD-DEC-137`, yang sengaja tidak mengubah siapa boleh membaca data pengguna |

**Mengapa satu kueri batch, bukan pola `BE-IGD-046`.** `BE-IGD-046` membaca `x.RecordedByUser.DisplayName` di dalam ekspresi
kueri. Itu tidak mungkin di sini: `EmgDepartureEvent` **tidak punya navigasi maupun foreign key** ke tabel pengguna
(`RecordedByUserId` dan `ApprovedByUserId` adalah kolom `Guid` biasa), dan pemeta respons adalah **fungsi statis di memori**.
Menambah navigasi dan foreign key berarti perubahan model dan migration, yang dilarang `IGD-DEC-137` dan dapat gagal pada
data lama yang menunjuk pengguna yang sudah tidak ada.

**Satu keputusan teknis yang perlu diketahui pemilik.** `ApplicationUser.DisplayName` bertipe `string` **non-nullable**
dengan nilai bawaan `""`. Urutan `DisplayName ?? UserName ?? Email ?? UserCode` yang dipakai `BE-IGD-046` karena itu tidak
pernah jatuh ke `UserName` untuk pengguna yang `DisplayName`-nya kosong, dan menghasilkan string kosong. Task ini melewati
nilai kosong/spasi sambil **mempertahankan urutan yang sama**, sesuai acceptance 3 ("bukan string kosong"). Akibat sampingan:
untuk pengguna yang `DisplayName`-nya kosong, layar kepergian menampilkan `UserName`, sedangkan layar observasi
(`BE-IGD-046`) menampilkan string kosong. Ketidakselarasan itu **dicatat, tidak diperbaiki** — di luar cakupan task ini.

---

## 4. Dokumentasi endpoint

Route, request, dan hak akses **tidak berubah**. Hanya bentuk respons yang memuat event.

#### Health Services / Emergency Installation Management / Emergency Departure

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kepergian | `EmergencyDeparture : Read` | Tiap event pada halaman membawa nama. **Satu** kueri nama untuk seluruh halaman |
| `GET` | `/{id}` | Detail kepergian | `EmergencyDeparture : Read` | `events[]` membawa nama |
| `POST` | `/` | Membuat kepergian | `EmergencyDeparture : Create` | `events[]` (event `Prepared`) membawa nama |
| `POST` | `/{id}/submit-handover`, `/depart`, `/arrive`, `/accept-handover`, `/reject-handover` | Proses kepergian | `EmergencyDeparture : Update` | `events[]` membawa nama |
| `PATCH` | `/{id}/cancel` | Membatalkan kepergian | `EmergencyDeparture : Update` | `events[]` membawa nama |
| `POST` | `/{id}/events/{eventId}/amend` | Mengoreksi waktu kejadian | `EmergencyDeparture : Update` | Satu event berisi nama |
| `POST` | `/{id}/events/{eventId}/reverse` | Membalik kejadian | `EmergencyDeparture : Approve` | Satu event; `approvedByName` terisi |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` (21 September 2026, 4 menit 51 detik; direktori keluaran terpisah `obj/efbuild/` karena backend pemilik sedang berjalan dan mengunci DLL-nya) | `Build succeeded.` **`0 Error(s)`**, **`207 Warning(s)`** | `PASS` | Keluaran perintah; jumlah warning **sama** dengan baseline 17 September dan 21 September. Nol warning menyebut `EmergencyDepartureController`, `EmergencyDepartureService`, atau `EmergencyDepartureDtos` |
| Tinjauan diff | 3 berkas source, `+106/−6`. Atribut `Access*`, `Http*`, `Route`, `Authorize` **nol baris berubah** | `PASS` | `git diff` |
| Akhir baris berkas | Ketiganya konsisten CRLF, sama dengan `HEAD` | `PASS` | Hitungan CRLF per berkas |
| Sisa pemakai pemeta lama pada event/kepergian di controller | 0 (tersisa 3 milik baris pesanan) | `PASS` | Pencarian teks |
| Uji API S1–S5 (bagian 5.1) | **Sebagian, lewat layar:** jalur `GET` daftar kepergian yang dipakai tab kepergian mengirim `recordedByName` (lihat baris berikut). **S2 (`amend`), S3 (`reverse`), S4 (pengguna tidak ada), S5 (satu kueri) belum dijalankan** | `PASS` (jalur `GET` daftar) / `NOT RUN` (S2–S5) | Backend yang berjalan sudah memuat perubahan (dibangun ulang dan dijalankan ulang pemilik) |
| Pembuktian **satu kueri** (acceptance 5) | Tidak dijalankan | `NOT RUN` | Butuh log SQL EF atau probe service seperti `BE-IGD-048` langkah H; keduanya menyentuh basis data dan tidak dijalankan agent |
| Tampilan layar `FE-IGD-017` | **`SuperAdmin`** tampil pada kolom *Pelaku* di ketiga kejadian `DEP-260921064559-9B2DB7`, menggantikan GUID `0ba84a1a-…` | `PASS` | Tangkapan layar pemilik, 21 September 2026 (larut malam); [laporan `FE-IGD-017`](../frontend/FE-IGD-017.md) bagian 6.1. Membuktikan **acceptance 1 pada jalur `GET` daftar** dan bahwa backend baru berjalan |

**Tidak ada automated test** (proyek test dihapus 11 September 2026, `IGD-DEC-110`).

### 5.1 Uji API untuk pemilik (S1–S5)

Jalankan **sesudah** build ulang dan restart backend.

| # | Skenario | Yang diharapkan |
| ---: | --- | --- |
| S1 | `GET /{id}` pada kepergian yang punya event, misalnya `DEP-260921064559-9B2DB7` | Setiap event: `recordedByUserId` **dan** `recordedByName` terisi; `approvedByName` `null` |
| S2 | `POST /{id}/events/{eventId}/amend` | Respons berisi `recordedByName` pengguna yang mengoreksi |
| S3 | `POST /{id}/events/{eventId}/reverse` dengan penyetuju | `approvedByUserId` **dan** `approvedByName` terisi |
| S4 | Event yang penggunanya tidak ada di tabel pengguna | `recordedByName` `null`, **bukan** GUID dan bukan string kosong |
| S5 | `GET /` dengan ≥ 2 kepergian | Setiap event membawa nama; log SQL EF menunjukkan **satu** `SELECT` ke `AspNetUsers` untuk seluruh halaman |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Setiap respons yang memuat event menyertakan `recordedByName`; `approvedByName` terisi bila `approvedByUserId` ada | **Terbukti runtime untuk `GET` daftar** (`recordedByName = SuperAdmin`, 21 Sep 2026 larut malam); **`amend`, `reverse`, dan `approvedByName` terisi belum** | Keenam titik controller memakai `ToResponseAsync`; layar `FE-IGD-017`; uji API S2–S3 milik pemilik |
| 2 | `recordedByUserId` dan `approvedByUserId` tetap dikirim dengan nilai yang sama | **Terpenuhi** | `ToResponse` statis tidak diubah; ruas ID tidak disentuh |
| 3 | Pengguna tidak ditemukan, atau ruas ID kosong → `null` — bukan GUID, `Guid.Empty`, maupun string kosong | **Terpetakan ke source; runtime belum** | `Guid.Empty` disaring sebelum kueri; `GetValueOrDefault` memberi `null`; `PilihNamaTampilan` melewati nilai kosong. Uji S4 |
| 4 | Urutan nama `DisplayName`, `UserName`, `Email`, `UserCode` | **Terpenuhi** | `PilihNamaTampilan(u.DisplayName, u.UserName, u.Email, u.UserCode)` |
| 5 | **Tanpa `N+1`:** satu kueri nama per respons (satu kepergian berkejadian ≥ 3 **dan** `GET /` berisi ≥ 2 kepergian) | **Terpetakan ke source; pembuktian runtime belum** | Satu `ToListAsync` di `IsiNamaPelakuAsync`, dipanggil **sekali** per respons dengan seluruh event terkumpul (overload halaman memakai `SelectMany`). Butuh log SQL/probe — S5 |
| 6 | Nol perubahan schema: model, konfigurasi EF, snapshot, `Migrations/`; nol route baru; nol perubahan request | **Terpenuhi** | `git status --short`: hanya tiga berkas di `Areas/…` yang berubah |
| 7 | Nol perubahan authorization | **Terpenuhi** | Nol baris atribut `Access*` berubah pada diff |
| 8 | `api-contract.md` naik satu minor (aditif); bagian `2.4` memuat ruas, aturan `null`, urutan nama, larangan GUID; ruas aktor lain dicatat tidak termasuk | **Terpenuhi** | `api-contract.md` `0.9.0`, bagian 2.4 |
| 9 | `dotnet build -p:RunAnalyzers=false` → 0 error, warning sama dengan baseline (207) | **Terpenuhi** | Bagian 5 |

**Definition of Done.** Terpenuhi **dengan pengecualian yang disebut apa adanya**: acceptance 1, 3, dan 5 belum terbukti pada
runtime; uji API S1–S5 dan tampilan layar belum dijalankan. Karena itu status **🟡**, bukan ✅. Syarat pemilik untuk membuka
`FE-IGD-017` — *"kontrak selesai dan build verified"* — **terpenuhi**.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `bin/Debug/net9.0/QuilvianSystemBackend.dll` (dibangun 13:30) **belum memuat perubahan ini**. Build saya ke `obj/efbuild/` agar tidak menyentuh backend Anda yang sedang berjalan. Bangun ulang biasa dan restart backend sebelum uji API. *Diperbarui: pemilik sudah membangun ulang dan menjalankan ulang backend — terbukti lewat layar `FE-IGD-017`.* |
| Masalah yang diketahui | (1) `DisplayName` kosong menghasilkan nama berbeda antara layar kepergian (jatuh ke `UserName`) dan layar observasi (string kosong) — lihat 3.3, tidak diperbaiki. (2) Ruas aktor lain pada kontrak kepergian tetap ID (`IGD-DEC-137` sengaja sempit) |
| Risiko tersisa | Rendah. Kueri nama tambahan (satu per respons) bertambah pada semua pembacaan kepergian; tanpa indeks baru karena pencarian memakai kunci utama `Id` |
| Perubahan sampingan | Direktori build sementara `obj/efbuild/` (270 MB) **dihapus** segera setelah build. Nol berkas lain |
| Interupsi | `NONE` |
| Status Git | `M` tiga source (`EmergencyDepartureController.cs`, `EmergencyDepartureDtos.cs`, `EmergencyDepartureService.cs`); `M` dokumen `igd/**` (kontrak, roadmap, traceability, status, keputusan, laporan) dan `??` laporan/evidence baru. Sebagian dokumen sudah `M`/`??` sebelum task ini. Tidak ada stage atau commit |
| Langkah berikutnya | (1) ~~Pemilik bangun ulang + restart backend~~ — sudah; jalankan S2–S5 (S1 setara sudah lewat layar). (2) `FE-IGD-017` boleh dimulai — dua baris pada `emergency-assessment-transfer-tab.jsx` |
