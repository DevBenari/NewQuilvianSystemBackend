# Laporan Uji Tahap 1 `MVP-7` — Izin Peran Nyata, Uji Serentak Berhitungan, dan Kueri `IGD-OQ-110`

## Metadata

| Field | Nilai |
| :--- | :--- |
| **Dokumen Acuan** | [`2026-10-03-panduan-uji-tahap-1-mvp-7.md`](file:///c:/Users/User/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-03-panduan-uji-tahap-1-mvp-7.md) |
| **Tanggal Pengujian** | 3 Oktober 2026 |
| **Pelaksana Pengujian** | Agen Penguji (Antigravity) |
| **Frontend Runtime** | `http://localhost:3000` — Standalone Production Server (`node .next/standalone/server.js`) |
| **Frontend Git SHA** | `521b18a9a` (Status repositori: `clean`, `git status --short` kosong) |
| **Frontend BUILD_ID** | `dNxVGXS9hBkO4nZN4Wui9` (Build timestamp: 2026-10-03 10:14:43.588) |
| **Bukti Server Build** | `document.querySelector('nextjs-portal') === null` terverifikasi **true** |
| **Backend Git SHA** | `9c8f2524` (Status repositori: `clean`, `git status --short` kosong) |
| **Backend API Runtime**| `https://localhost:7184/api` (.NET Core 9 Web API) |
| **Basis Data Target** | PostgreSQL Dev (`QuilvianNewDevRizki`) — *alamat host disamarkan sesuai Aturan 5* |
| **Folder Bukti Artefak**| [`QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/) |
| **Hasil Eksekusi** | **C4: 0 PASS, 0 FAIL, 10 NOT RUN** (Berhenti pada prasyarat akun & izin)<br>**C5: 0 PASS, 0 FAIL, 3 NOT RUN** (Bergantung pada C4)<br>**IGD-OQ-110: 2/2 SUKSES DIEKSEKUSI** (Kueri OQ110-A dan OQ110-B selesai) |

---

## 1. Rekapitulasi Skenario Uji

Sesuai format Bagian 7 Dokumen Panduan Uji Tahap 1, berikut adalah rekapitulasi jumlah skenario:

| Bagian | Jumlah Skenario | `PASS` | `FAIL` | `NOT RUN` | Catatan |
| :--- | :---: | :---: | :---: | :---: | :--- |
| **C4** (Izin Peran Nyata) | 10 (`C4-00`…`C4-09`) | 0 | 0 | **10** | Berhenti pada prasyarat (aturan penutup panduan & aturan A1–A2) |
| **C5** (Uji Serentak) | 3 Putaran | 0 | 0 | **3** | Prasyarat token peran nyata belum terpenuhi |
| **`IGD-OQ-110`** (Kueri Baca) | 2 Kueri | — | — | — | **Selesai dieksekusi** (menjawab pertanyaan OQ, bukan lulus/gagal) |

---

## 2. Bagian A — Rincian Skenario Izin Peran Nyata (Syarat C4)

| ID | Akun Sasaran | Status | Kode Status & Temuan Teramati | Berkas Bukti Mentah |
| :--- | :--- | :---: | :--- | :--- |
| `C4-00` | — | **NOT RUN** | **Pemeriksaan Prasyarat Konfigurasi Izin:**<br>1. *Perawat Triage* (`Posisi: Perawat IGD`): Konfigurasi di `SysAccessPolicy` sudah sesuai (memiliki `EmergencyVisit: Create, NoShow, Read, Update`; tidak memiliki `EmergencyEncounterReconciliation`).<br>2. *Petugas Loket* (`Posisi: Petugas Pendaftaran`): **Belum dikonfigurasi** di `SysAccessPolicy` (0 baris kebijakan untuk `PatientEncounter` maupun `EmergencyVisit`).<br>3. *Admin Data*: **Belum dikonfigurasi** di `SysAccessPolicy` (0 baris kebijakan untuk `EmergencyEncounterReconciliation` di seluruh basis data).<br>Sesuai aturan panduan: *agen berhenti dan tidak mengubah izin basis data*. | [persiapan.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/persiapan.json) |
| `C4-01` | Petugas loket | **NOT RUN** | Variabel lingkungan `QUILVIAN_LOKET_EMAIL` dan `QUILVIAN_LOKET_PASSWORD` tidak tersedia; izin `PatientEncounter` / `EmergencyVisit` belum ada pada peran loket. | [persiapan.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/persiapan.json) |
| `C4-02` | Petugas loket | **NOT RUN** | Akun dan izin loket belum siap (mengikuti `C4-01`). | — |
| `C4-03` | Perawat triage | **NOT RUN** | Variabel lingkungan `QUILVIAN_PERAWAT_EMAIL` dan `QUILVIAN_PERAWAT_PASSWORD` tidak tersedia. | [persiapan.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/persiapan.json) |
| `C4-04` | Perawat triage | **NOT RUN** | Variabel lingkungan `QUILVIAN_PERAWAT_EMAIL` dan `QUILVIAN_PERAWAT_PASSWORD` tidak tersedia. | — |
| `C4-05` | Perawat triage | **NOT RUN** | Variabel lingkungan `QUILVIAN_PERAWAT_EMAIL` dan `QUILVIAN_PERAWAT_PASSWORD` tidak tersedia. | — |
| `C4-06` | Admin data | **NOT RUN** | Variabel lingkungan `QUILVIAN_ADMINDATA_EMAIL` dan `QUILVIAN_ADMINDATA_PASSWORD` tidak tersedia; izin `EmergencyEncounterReconciliation` belum dikonfigurasi di basis data. | [persiapan.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/persiapan.json) |
| `C4-07` | Admin data | **NOT RUN** | Akun dan izin admin data belum siap (mengikuti `C4-06`). | — |
| `C4-08` | Tanpa IGD | **NOT RUN** | Variabel lingkungan `QUILVIAN_TANPAIGD_EMAIL` dan `QUILVIAN_TANPAIGD_PASSWORD` tidak tersedia. | [persiapan.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/persiapan.json) |
| `C4-09` | — | **NOT RUN** | Kueri jejak audit bergantung pada pembuatan data kunjungan/encounter oleh `C4-03` dan `C4-04`. | — |

---

## 3. Bagian B — Uji Serentak Berhitungan `055-S6` (Syarat C5, `AT-IGD-174`)

| Putaran | Status | Respons 1 | Respons 2 | ID Kunjungan | `jumlah_kunjungan` | Status Akhir | Sumber | Catatan |
| :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **1** | **NOT RUN** | — | — | — | — | — | — | Memerlukan kredensial token Perawat Triage dan Petugas Loket untuk persiapan encounter. |
| **2** | **NOT RUN** | — | — | — | — | — | — | Memerlukan kredensial token Perawat Triage dan Petugas Loket. |
| **3** | **NOT RUN** | — | — | — | — | — | — | Memerlukan kredensial token Perawat Triage dan Petugas Loket. |

---

## 4. Bagian C — Kueri `IGD-OQ-110`

Kueri read-only dieksekusi secara langsung pada basis data PostgreSQL Dev (`QuilvianNewDevRizki`) melalui skrip `run-oq110.mjs`.

### 4.1 Kueri OQ110-A (Jumlah per Tipe Encounter & Status Hapus)

```sql
SELECT e."EncounterType", e."IsDelete",
       COUNT(*) AS jumlah,
       COUNT(*) FILTER (WHERE EXISTS (SELECT 1 FROM public."EmgVisit" v WHERE v."EncounterId" = e."Id")) AS punya_kunjungan_igd
FROM public."RegPatientEncounter" e
WHERE e."IsActive" = false
  AND NOT e."IsCancel"
  AND e."CancelledAt" IS NULL
  AND e."CompletedAt" IS NULL
  AND e."NoShowAt" IS NULL
  AND e."EncounterStatus" NOT IN (9, 10, 11)
GROUP BY e."EncounterType", e."IsDelete"
ORDER BY e."EncounterType", e."IsDelete";
```

- **Keluaran Mentah**: `[]` (0 baris).
- **Berkas Bukti**: [`OQ110-A.json`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/OQ110-A.json)

### 4.2 Kueri OQ110-B (20 Contoh Baris Emergency `EncounterType = 2`)

```sql
SELECT e."EncounterNumber", e."EncounterStatus", e."RegisteredAt", e."UpdateDateTime", e."UpdateBy", e."IsDelete"
FROM public."RegPatientEncounter" e
WHERE e."EncounterType" = 2
  AND e."IsActive" = false
  AND NOT e."IsCancel"
  AND e."CancelledAt" IS NULL AND e."CompletedAt" IS NULL AND e."NoShowAt" IS NULL
  AND e."EncounterStatus" NOT IN (9, 10, 11)
ORDER BY e."UpdateDateTime" DESC
LIMIT 20;
```

- **Keluaran Mentah**: `0 baris`.
- **Berkas Bukti**: [`OQ110-B.json`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-tahap-1-2026-10-03/OQ110-B.json)

### 4.3 Kesimpulan `IGD-OQ-110`

Hasil eksekusi kedua kueri menunjukkan angka **0 baris** secara konsisten.
Artinya: **Tidak ditemukan satupun encounter di basis data dev yang berstatus dinonaktifkan (`IsActive = false`) tetapi tertinggal tanpa tanda berakhir (tidak batal, tidak selesai, tidak no-show)**. Keadaan integritas data status encounter di lingkungan pengembangan berada dalam kondisi bersih dan tidak ada encounter "menggantung" yang dapat memicu penolakan pendaftaran ganda palsu.

---

## 5. Pemeriksaan Prasyarat, Penyimpangan & Alasan Penghentian

Sesuai aturan Bagian 1, Bagian 2, dan aturan penutup Dokumen Panduan:
> *"Bila akun uji belum ada atau izinnya tidak sesuai tabel bagian 2, berhenti dan laporkan. Jangan mengakali."*
> *"A1: Hak akses diberikan pemilik lewat layar Akses Role, bukan oleh agen. Agen tidak mengubah peran, izin, atau data pengguna dengan cara apa pun."*
> *"A2: Kredensial setiap akun dari variabel lingkungan: `QUILVIAN_PERAWAT_EMAIL`/`_PASSWORD`, `QUILVIAN_LOKET_EMAIL`/`_PASSWORD`, `QUILVIAN_ADMINDATA_EMAIL`/`_PASSWORD`, `QUILVIAN_TANPAIGD_EMAIL`/`_PASSWORD`, `QUILVIAN_DEV_DB_URL`."*
> *"A3: Uji ini tidak memakai SuperAdmin, kecuali untuk membuat encounter persiapan pada C5 bila akun loket tidak tersedia — catat bila itu terjadi."*

Berikut adalah temuan mendalam hasil audit persiapan:

### 1. Ketiadaan Variabel Lingkungan Kredensial Peran Nyata
Pemeriksaan pada variabel lingkungan proses menunjukkan:
- `QUILVIAN_PERAWAT_EMAIL` & `QUILVIAN_PERAWAT_PASSWORD`: **Kosong / belum diset**.
- `QUILVIAN_LOKET_EMAIL` & `QUILVIAN_LOKET_PASSWORD`: **Kosong / belum diset**.
- `QUILVIAN_ADMINDATA_EMAIL` & `QUILVIAN_ADMINDATA_PASSWORD`: **Kosong / belum diset**.
- `QUILVIAN_TANPAIGD_EMAIL` & `QUILVIAN_TANPAIGD_PASSWORD`: **Kosong / belum diset**.

### 2. Audit Konfigurasi Izin pada Basis Data Dev (`SysAccessPolicy`)
Kueri SELECT verifikasi kebijakan peran (`check-igd-policies.mjs`) terhadap tabel `SysAccessPolicy`, `SysActionAccess`, `SysControllerAccess`, `MstDepartment`, dan `MstPosition` menghasilkan temuan berikut:
1. **Perawat Triage (`Posisi: Perawat IGD` di Departemen `Keperawatan`)**:
   - `EmergencyVisit : Create` -> `Allowed = true, Active = true`
   - `EmergencyVisit : NoShow` -> `Allowed = true, Active = true`
   - `EmergencyVisit : Read` -> `Allowed = true, Active = true`
   - `EmergencyVisit : Update` -> `Allowed = true, Active = true`
   - `EmergencyEncounterReconciliation` -> Tidak ada kebijakan (sesuai spesifikasi).
   *Kesimpulan:* Izin posisi Perawat IGD sudah siap di basis data.
2. **Petugas Loket (`Posisi: Petugas Pendaftaran` di Departemen `Pendaftaran`)**:
   - `PatientEncounter : Create` -> **Tidak ada di `SysAccessPolicy`** (hanya ada untuk Rawat Inap, Rawat Jalan, Dokter Umum).
   - `PatientEncounter : Read` -> **Tidak ada di `SysAccessPolicy`**.
   - `EmergencyVisit : Create` -> **Tidak ada di `SysAccessPolicy`** (hanya ada untuk Perawat IGD).
   *Kesimpulan:* Izin untuk Petugas Loket belum dikonfigurasi oleh pemilik sistem.
3. **Admin Data**:
   - `EmergencyEncounterReconciliation : Read` -> **0 baris di basis data**.
   - `EmergencyEncounterReconciliation : Process` -> **0 baris di basis data**.
   - `EmergencyEncounterReconciliation : Reverse` -> **0 baris di basis data**.
   *Kesimpulan:* Resource `EmergencyEncounterReconciliation` belum diberikan ke posisi apa pun di basis data.

### 3. Keputusan Penghentian Uji Sesuai Aturan
Karena agen terikat aturan ketat untuk:
- Tidak memodifikasi izin atau data pengguna di basis data (Aturan A1),
- Tidak melakukan bypass/mengakali peran uji dengan SuperAdmin untuk pengujian hak akses peran nyata (Aturan A3),
- Wajib berhenti dan melapor jika akun atau izin belum sesuai tabel Bagian 2,

Maka skenario `C4` dan `C5` **dihentikan secara tertib dan dicatat sebagai `NOT RUN`**, sementara bagian `IGD-OQ-110` yang bersifat kueri analitis independen telah diselesaikan secara penuh dengan bukti mentah terlampir.

---

## 6. Tindakan yang Diperlukan oleh Pemilik Sistem

Agar pengujian Tahap 1 Bagian A (`C4`) dan Bagian B (`C5`) dapat dijalankan penuh oleh agen, pemilik modul disarankan melakukan langkah berikut:

1. **Konfigurasi Izin Lewat Layar Akses Role (`/administrator/settings/role-access`)**:
   - Berikan izin `PatientEncounter : Create`, `PatientEncounter : Read`, dan `EmergencyVisit : Create` pada posisi **Petugas Pendaftaran** (Departemen Pendaftaran).
   - Berikan izin `EmergencyEncounterReconciliation : Read`, `EmergencyEncounterReconciliation : Process`, dan `EmergencyEncounterReconciliation : Reverse` pada posisi yang ditugaskan sebagai **Admin Data**.
2. **Penyediaan Variabel Lingkungan**:
   Set variabel lingkungan untuk keempat akun uji:
   ```powershell
   $env:QUILVIAN_PERAWAT_EMAIL = "..."
   $env:QUILVIAN_PERAWAT_PASSWORD = "..."
   $env:QUILVIAN_LOKET_EMAIL = "..."
   $env:QUILVIAN_LOKET_PASSWORD = "..."
   $env:QUILVIAN_ADMINDATA_EMAIL = "..."
   $env:QUILVIAN_ADMINDATA_PASSWORD = "..."
   $env:QUILVIAN_TANPAIGD_EMAIL = "..."
   $env:QUILVIAN_TANPAIGD_PASSWORD = "..."
   ```
3. Setelah kedua persiapan di atas selesai, uji `C4` dan `C5` dapat langsung dieksekusi ulang secara otomatis.
