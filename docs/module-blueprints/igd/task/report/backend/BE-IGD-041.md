# Laporan Perubahan Backend — `BE-IGD-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-041` |
| Judul | Penolakan penutupan kunjungan menyebut pesanan yang menahannya |
| Slice | `IGD-S08` · menutup kriteria 2 `BE-IGD-035` (`EPIC IGD-07`) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.8 |
| Requirement | `FR-IGD-051` |
| Keputusan | `IGD-DEC-118` (`approved`, Rizki 15 September 2026), `IGD-DEC-106`; bukti `IGD-EV-122` |
| Contract version | Validation `0.5.0` §6 aturan 4 beserta §6.1 — `approved`. **Nol perubahan kontrak, nol endpoint baru, nol kenaikan versi** |
| Dependency | `IGD-DEC-118` ✅. Nol dependency task |
| Klasifikasi | `LIGHT`–`MEDIUM` — dua service pada satu modul, nol entity, nol migration, nol endpoint |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `27351517` (branch `rizkiG`) + working tree |
| Tanggal | 16 September 2026 |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore): 17 dari 17 acceptance terbukti** (`IGD-DEC-218`; bagian 9.12). Acceptance 14 terbukti lewat `041-S9` (`Handover` menunggu penerimaan tidak menahan) dan `041-S11b` (baris tergantikan tidak menahan); acceptance 15 lewat `041-S10` (terima saat kunjungan menunggu penutupan → tertutup) dan `041-S11a` (tolak saat menunggu penutupan → tetap tertahan) pada putaran bersama `FE-IGD-044` — bukti mentah dan log backend diperiksa agent; bukti **diterima dengan penyimpangan tercatat** (`IGD-DEC-218`). Rilis hanya bersama `FE-IGD-044` (`IGD-DEC-204`). Tanpa UAT — diserahkan ke tim UAT. *Sebelumnya:* 🟡 **SEBAGIAN — 6 Oktober 2026: 15 dari 17 acceptance terbukti** (`IGD-DEC-210`, `IGD-DEC-211`; bagian 9.11). Satu berkas, `EmergencyDepartureService.cs` (+17/−8); QBE checker Strict `PASS`; build pemilik (DLL 08.30.28 WIB, backend berjalan dari build itu; warning tidak dilaporkan). Uji API Antigravity 8 skenario, diperiksa agent pada bukti mentah dan log backend: 84 dari 84 permintaan cocok, 8 putusan `PASS`; bukti **diterima dengan penyimpangan tercatat** (`IGD-DEC-210`). **Belum:** acceptance 14 (`Handover` menunggu penerimaan; baris yang digantikan) dan 15 (`061-S12`) — diuji pada putaran bersama `FE-IGD-044` (`IGD-DEC-211`). Rilis hanya bersama `FE-IGD-044` (`IGD-DEC-204`). Tanpa UAT. *Status ✅ yang sempat ditulis agen penguji ditarik (`IGD-DEC-212`)* |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `EmergencyInstallationManagement` |
| Submodule | — |
| Pemilik / prefix registry | Prefix `Emg`, kategori `BUSINESS DOMAIN / MODULE`, lifecycle `ACTIVE / LEGACY` — terdaftar di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 19 |
| Applicability | **`TOUCHED LEGACY`** — mengubah dua method pada service yang sudah ada. Nol entity baru, nol model persisted, nol rename, nol migration |
| Status registry | Terdaftar dan `ACTIVE`; wewenang implementasi ada |
| QBE yang berlaku | `QBE-VAL-001` (invarian bisnis divalidasi sebelum penutupan kunjungan), `QBE-SVC-001` (aturan domain tinggal di Module Service, bukan controller), `QBE-API-001` (kode status dan envelope penolakan tidak berubah) |
| QBE yang dicatat, tidak ditangani | `QBE-CFG-002` tidak berlaku — nol `IEntityTypeConfiguration` disentuh. `QBE-MOD-002`/`003`, `QBE-NAM-*`, `QBE-CODE-*`, `QBE-DB-*` tidak berlaku — nol entity, nol penamaan operasional, nol nomor bisnis, nol pekerjaan database |

---

## 1. Masalah yang diperbaiki

Dokter yang menutup kunjungan IGD dan ditahan oleh pesanan yang belum diberi sikap hanya melihat
satu kalimat: *"Masih ada pesanan yang belum ditentukan sikapnya."*

Kalimat itu tidak memberitahunya **pesanan mana**. Pada pasien yang punya beberapa dokumen
kepergian dengan belasan pesanan, satu-satunya cara mengetahuinya adalah membuka setiap kepergian
satu per satu dan memeriksa sikap tiap pesanan. Ini bukti `IGD-EV-122`, dan `IGD-DEC-118`
memutuskan pesannya diperkaya.

Cacat kedua yang ikut ditutup: **aturan kuerinya ada dua tempat.**

| Tempat | Keadaan sebelum |
| --- | --- |
| `EmergencyDispositionService.ValidateVisitClosureAsync` baris 131-142 | Punya kueri sendiri terhadap `EmgHandoverOrderItem`, memakai `AnyAsync`, pesan generik |
| `EmergencyDepartureService.ValidatePesananSebelumPenutupanAsync` baris 548 | Kueri yang sama persis, sudah menyusun daftar lima uraian beserta *"dan N lainnya"* — dan **nol pemanggil** |

Dua salinan aturan yang sama adalah cara paling murah untuk membuat keduanya berbeda diam-diam
di kemudian hari. Satu diperbaiki, satunya tertinggal — persis pelajaran `BE-IGD-016`.

---

## 2. Proses bisnis

**Pelaku:** dokter atau petugas IGD berhak `EmergencyVisit : Update`.

**Kapan:** saat menutup kunjungan lewat `PATCH /emergency-visits/{id}/complete`, sesudah keputusan
tindak lanjut ditetapkan (`Disposed`).

Urutan pemeriksaannya **tidak berubah** — empat aturan berjalan berurutan, dan yang pertama gagal
langsung mengembalikan `409`:

1. Status kunjungan wajib `Disposed` → *"Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan."*
2. Tidak boleh ada observasi `Active` → *"Masih ada observasi yang belum diselesaikan."*
3. Tidak boleh ada kepergian yang fisiknya belum `Arrived`/`Cancelled` → *"Masih ada proses kepergian pasien yang belum selesai."*
4. Tidak boleh ada pesanan yang menahan → **pesannya kini menyebut pesanannya.**

**Contoh aturan 4 sesudah perubahan.** Tujuh pesanan ditolak Rawat Inap dan belum diberi sikap
pengganti:

> *"Masih ada pesanan yang belum ditentukan sikapnya: Darah lengkap, Elektrolit, Ureum, Kreatinin,
> Gula darah sewaktu dan 2 lainnya."*

Dua pesanan saja:

> *"Masih ada pesanan yang belum ditentukan sikapnya: Darah lengkap, Elektrolit."*

Nol pesanan yang menahan → penutupan **berjalan**, persis seperti sebelumnya.

**Yang tidak berubah:** kode `409`, kondisi penolakan, dan sikap `Continue` yang memang tidak
pernah menahan (`IGD-DEC-100` butir a).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Services/EmergencyDispositionService.cs`, `Services/EmergencyDepartureService.cs`
- `Controllers/EmergencyVisitController.cs` baris 456-500 (pemanggil closure gate)
- `Program.cs` baris 435-442 (pendaftaran DI)
- `contracts/validation-matrix.md` §5 aturan 1 dan 12, §5.1, §6 aturan 1-4, §6.1
- `00-interview-decisions.md` — `IGD-DEC-118`, `IGD-DEC-102`, `IGD-DEC-106`
- `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `BACKEND_ENGINEERING_CONTRACT.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/EmergencyDepartureService.cs` | Kueri dipisahkan menjadi `AmbilPesananPenahanPenutupanAsync` yang mengembalikan daftar uraian, ditambah `RingkasUraianPesanan` statis untuk aturan lima-plus-sisa. `ValidatePesananSebelumPenutupanAsync` kini memakai keduanya; **pesannya tidak berubah sedikit pun** |
| `Services/EmergencyDispositionService.cs` | Konstruktor menerima `EmergencyDepartureService`. Kueri kembar pada `ValidateVisitClosureAsync` dihapus dan diganti pemanggilan, lalu pesan §6 aturan 4 disusun beserta daftar pesanannya |

Total `+66 / −19` pada dua berkas. Nol berkas baru.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol route baru, nol perubahan bentuk request/response. Yang berubah hanya **isi kalimat** pesan penolakan pada `409` |
| Database | **Nol** — nol migration, nol perubahan skema, nol kueri database dijalankan agent |
| Keamanan dan hak akses | Nol perubahan. `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` pada jalur penutupan tidak disentuh |
| `Program.cs` | **Nol baris baru** — kedua service sudah terdaftar `AddScoped` pada baris 440 dan 441 |
| Privasi | Uraian pesanan (`OrderDescription`) adalah nama pemeriksaan, bukan hasil maupun data diri. Ditampilkan kepada petugas yang memang sedang menutup kunjungan itu |

---

## 4. Dokumentasi endpoint

Nol endpoint baru. Endpoint yang perilaku pesannya berubah:

```yaml
PATCH /api/v1/health-services/emergency-installation-management/emergency-visits/{id}/complete
summary: Menyelesaikan kunjungan IGD secara klinis
security:
  - bearerAuth: []
permission: EmergencyVisit + Update
responses:
  "200":
    description: Kunjungan IGD berhasil diselesaikan
  "409":
    description: >
      Closure gate belum terpenuhi. Salah satu dari empat pesan validation §6,
      dan aturan 4 kini menyebut pesanan yang menahannya —
      "Masih ada pesanan yang belum ditentukan sikapnya: {daftar pesanan}."
```

---

## 5. Verifikasi

### 5.1 Perintah yang dijalankan agent

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Pencarian pemanggil `ValidatePesananSebelumPenutupanAsync` di seluruh `.cs` | Nol pemanggil di luar berkas pemiliknya — memastikan perubahan bentuknya tidak memutus siapa pun | `PASS` |
| Pencarian `new EmergencyDispositionService(` | Nol konstruksi manual — memastikan penambahan parameter konstruktor aman | `PASS` |
| Pemeriksaan DI pada `Program.cs` | `EmergencyDispositionService`, `EmergencyDepartureService`, `EmergencyDocumentNumberService`, `EmergencyUnitAuthorityService` seluruhnya `AddScoped` | `PASS` |
| `dotnet build` | **Tidak dijalankan** | `NOT RUN` |

### 5.2 Perintah build untuk Rizki

Dijalankan dari `NewQuilvianSystemBackend`:

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Bila backend sedang berjalan, hentikan lebih dulu supaya berkas keluarannya tidak terkunci.

### 5.3 Uji API manual yang disarankan (`IGD-DEC-110`)

Automated test bukan acceptance criterion — proyek test backend dihapus 11 September 2026. Tiga
skenario berikut membuktikan acceptance 1, 2, dan 5:

| No | Keadaan | Harapan |
| ---: | --- | --- |
| 1 | Kunjungan `Disposed`, **nol** pesanan ditolak | `200` — kunjungan tertutup |
| 2 | Kunjungan `Disposed`, **dua** pesanan ditolak tanpa sikap pengganti | `409` — *"Masih ada pesanan yang belum ditentukan sikapnya: {uraian 1}, {uraian 2}."* |
| 3 | Kunjungan `Disposed`, **tujuh** pesanan ditolak | `409` — lima uraian pertama, lalu *"dan 2 lainnya"* |

Tambahan untuk acceptance 4: kunjungan yang belum `Disposed`, atau masih punya observasi `Active`,
tetap menerima pesannya masing-masing lebih dulu.

### 5.4 Pemetaan pemeriksaan ke source

| Aturan | Tempat sesudah perubahan |
| --- | --- |
| §6 aturan 1-3 | `EmergencyDispositionService.ValidateVisitClosureAsync` — urutan dan teks tidak berubah |
| §6 aturan 4 — kondisi | `EmergencyDepartureService.AmbilPesananPenahanPenutupanAsync` — satu-satunya tempat |
| §6 aturan 4 — peringkasan | `EmergencyDepartureService.RingkasUraianPesanan` |
| §6 aturan 4 — kalimat | `EmergencyDispositionService.ValidateVisitClosureAsync` |
| §5 aturan 12 — kalimat | `EmergencyDepartureService.ValidatePesananSebelumPenutupanAsync` — tidak berubah |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Penutupan dengan pesanan ditolak → `409` menyebut uraian pesanannya | **Terpenuhi pada source** | `ValidateVisitClosureAsync` menyusun pesan dari daftar uraian; menunggu uji API |
| 2. Paling banyak lima uraian, selebihnya *"dan N lainnya"* | **Terpenuhi** | `RingkasUraianPesanan` — `Take(5)` beserta sisanya |
| 3. Satu sumber aturan; kueri kembar dihapus | **Terpenuhi** | Kueri `EmgHandoverOrderItem` kini hanya ada satu di seluruh modul, pada `AmbilPesananPenahanPenutupanAsync` |
| 4. Aturan §6 nomor 1-3 tetap lebih dulu, urutan dan pesan sama | **Terpenuhi** | Diff tidak menyentuh ketiga blok itu |
| 5. Kunjungan tanpa pesanan ditolak tetap dapat ditutup | **Terpenuhi pada source** | `pesananPenahan.Count > 0` — daftar kosong mengembalikan `null`; menunggu uji API |
| 6. Nol baris baru di `Program.cs` | **Terpenuhi** | `Program.cs` tidak disentuh; DI menyelesaikan konstruktor sendiri |
| 7. `BE-IGD-035` dinilai ulang | **Terpenuhi** | Kriteria 2 terpenuhi; task itu tetap 🟡 karena `BE-IGD-039` belum beres. Baris statusnya diperbarui |

**Definition of Done:** acceptance 1-7 terpetakan ✅; laporan tracked ada ✅; roadmap dan
traceability diperbarui ✅; QBE preflight diselesaikan ✅; tanpa UAT PASS ✅. **Belum:**
`dotnet build` dan uji API manual.

---

## 7. Catatan penutup

### 7.1 Keputusan implementasi yang perlu diketahui pemilik

**Kartu meminta `ValidateVisitClosureAsync` memanggil `ValidatePesananSebelumPenutupanAsync`
langsung. Itu tidak dapat dilakukan apa adanya, dan alasannya bukan teknis melainkan kontrak.**

Dua aturan approved memakai kueri yang sama tetapi kalimat yang berbeda:

| Aturan | Kode | Kalimat approved | Keputusan |
| --- | :-: | --- | --- |
| Validation §5 aturan 12 | `409` | *"Ada pesanan yang ditolak unit penerima dan belum ditetapkan sikap penggantinya."* | `IGD-DEC-102` butir (b) dan (c) |
| Validation §6 aturan 4 | `409` | *"Masih ada pesanan yang belum ditentukan sikapnya: {daftar pesanan}."* | `IGD-DEC-118` |

`ValidatePesananSebelumPenutupanAsync` mengembalikan kalimat §5 aturan 12. Memanggilnya langsung
dari jalur penutupan berarti `IGD-DEC-118` tidak terpenuhi; mengganti kalimat di dalamnya berarti
`IGD-DEC-102` yang dikorbankan. **Nol kalimat approved diubah:** yang dibagi adalah **aturannya**
(`AmbilPesananPenahanPenutupanAsync`), bukan kalimatnya. Keduanya tetap memakai satu kueri —
§6.1 butir (c) dan (d) terpenuhi, dan titik penegakannya tetap `EmergencyDepartureService`.

Konsekuensinya satu: **scope bertambah satu berkas** dari yang tertulis di kartu.
`EmergencyDepartureService.cs` ikut disentuh, padahal kartu hanya menyebut
`EmergencyDispositionService.cs`. Perubahannya aditif — dua method baru, dan method lama tetap
berperilaku persis sama.

### 7.2 Temuan yang dilaporkan, bukan ditambal

**Kondisi §6 aturan 4 lebih sempit dari judulnya.** Judul aturannya berbunyi *"Tidak boleh ada
pesanan tanpa sikap"*, dan §5.1 menyatakan yang menahan penutupan ada **dua**: pesanan tanpa sikap
sama sekali (aturan 1) dan pesanan ditolak yang belum diberi sikap pengganti (aturan 12).

Kuerinya — baik yang lama maupun yang sekarang — hanya menyaring `AcceptanceStatus == Rejected`.
**Pesanan yang belum punya sikap sama sekali tidak menahan penutupan.**

Ini **tidak** diperbaiki, karena kartu menyatakan *"kondisi penolakan tidak berubah"* dan
`IGD-DEC-118` menegaskan hal yang sama. Memperluas kondisinya akan menahan kunjungan yang hari ini
dapat ditutup — perubahan perilaku yang butuh keputusan pemilik, bukan efek samping perbaikan
pesan. Disarankan menjadi kartu tersendiri.

### 7.3 Penutup baku

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` dari pekerjaan ini. Build belum dijalankan |
| Masalah yang diketahui | Temuan 7.2 di atas |
| Keadaan migration | **Nol** — tidak ada migration dibuat maupun dijalankan |
| Eksekusi database | **Nol** — agent tidak menjalankan kueri apa pun |
| Risiko tersisa | Jalur penutupan dipakai setiap kunjungan IGD. Ketiga aturan sebelumnya tidak disentuh, dan kondisi aturan 4 tidak berubah — yang berubah hanya kalimatnya. Risiko regresi terbatas pada susunan kalimat |
| Perubahan sampingan | `NONE`. Perubahan `BE-IGD-046` yang belum di-commit pada working tree tidak disentuh |
| Interupsi | `NONE` |
| Langkah berikutnya | (1) `dotnet build`. (2) Uji API tiga skenario bagian 5.3. (3) Putuskan apakah temuan 7.2 layak kartu sendiri |

### 7.4 `git status --short` di akhir pekerjaan

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDispositionService.cs
```

Berkas lain yang berubah pada working tree adalah milik `BE-IGD-046` dan dokumen blueprint IGD,
bukan hasil task ini.

---

## 8. Status akhir

**Implementasi `BE-IGD-041` selesai; belum diverifikasi build maupun runtime.** Ketujuh acceptance
criteria terpetakan ke source yang benar-benar ada. Yang menahan penandaan ✅ adalah `dotnet build`
dan uji API tiga skenario — keduanya milik Rizki. Tanpa UAT.

*Keadaan sesudah 6 Oktober 2026: lihat bagian 9. Temuan 7.2 kini menjadi isi pengerjaan ulang itu.*

---

## 9. Pengerjaan ulang 6 Oktober 2026 — pesanan tanpa sikap ikut menahan penutupan

### 9.1 Metadata pengerjaan ulang

| Field | Nilai |
| --- | --- |
| Dasar | `IGD-DEC-203` (pengerjaan ulang atas `IGD-CONFLICT-007`), `IGD-DEC-205` (pesanan pada kepergian yang dibatalkan tidak menahan), `IGD-DEC-204` (rilis bersama `FE-IGD-044`); fakta `IGD-FACT-051`, `054`, `055`, `057` |
| Contract version | Validation `0.13.0` §5 aturan 12, §5.1, §6 aturan 4, §6.1; state `0.9.0` §6a.2, §9.2 butir 4; API `0.14.0` §1.2, §9 — `approved` (`IGD-DEC-209`, sementara pola `IGD-DEC-174`; hash di manifest bagian 0m). Task ini tidak mengubah kontrak |
| Acceptance | 1–17 — kartu roadmap backend R3.8, termasuk bagian *Perluasan 5 Oktober 2026* |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 1, berkas diubah 0 (1 berkas), logika bisnis 1, kontrak API 1 (isi `awaitingClosureReason` dan pesan `409`, bentuk tetap), database 1 (perilaku kueri), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — izin pemilik 6 Oktober 2026 (*"mulai saja /build-module-backend BE-IGD-041"*). Frontend baca-saja |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs`; laporan ini, baris status roadmap, traceability |
| Commit backend saat dikerjakan | `3811fa06` (`rizkiG`); working tree source bersih sebelum task. Area task tidak berubah sejak `8d81d361` — commit `3811fa06` hanya memuat `EmergencyUnitAuthorityService.cs` (`BE-IGD-039`) dan dokumen |
| Commit frontend rujukan | `2a985f6d5` (`RizkiV2`), bersih |
| Model | Claude Opus 5.5 |
| Tanggal | 6 Oktober 2026 |

### 9.2 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik / prefix registry | `Emg` = *Emergency*, `BUSINESS DOMAIN / MODULE`, `ACTIVE / LEGACY` — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 26 |
| Applicability | **`TOUCHED LEGACY`** — mengubah dua method dan menambah satu method privat serta satu field statis pada service yang sudah ada. Nol entity, nol model persisted, nol rename, nol migration |
| QBE yang berlaku | `QBE-VAL-001` (invarian penutupan kunjungan), `QBE-SVC-001` (aturan tinggal di Module Service), `QBE-API-001` (kode `409`, envelope, dan bentuk respons tetap) |
| QBE yang tidak berlaku | `QBE-MOD-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*`, `QBE-PERM-001` — nol entity, nol penamaan operasional, nol konfigurasi EF, nol nomor bisnis, nol pekerjaan database, nol endpoint atau atribut akses |
| Checker | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` — 1 berkas, VIOLATION 0, REVIEW 0, INFO 0, *Final result: PASS* |

### 9.3 Masalah yang diperbaiki

Pada putaran uji `BE-IGD-039` (`PROBE-1`, 5 Oktober 2026), pesanan luar sistem dibuat tanpa sikap (`action` 0), pasien
tiba, tindak lanjut dilaksanakan — dan kunjungan langsung *Selesai*. Kontrak menyatakan pesanan tanpa sikap menahan
penutupan, tetapi kueri penahan hanya menyaring pesanan **ditolak** (`IGD-FACT-051`). Inilah temuan 7.2 laporan ini
yang 16 September 2026 sengaja tidak ditambal; pemilik memutuskannya lewat `IGD-DEC-203`.

### 9.4 Proses bisnis sesudah perubahan

Penahan penutupan kunjungan = pesanan berlaku (`IsEffective`, tidak dihapus) pada kepergian kunjungan itu yang
**tanpa sikap**, **atau** yang **ditolak** unit penerima dan belum diberi sikap pengganti — kecuali pesanan milik
kepergian yang fisiknya **dibatalkan**. Aturan ini dipakai bersama oleh tombol *Selesaikan*, penutupan susulan, dan
alasan pada daftar menunggu penutupan (`IGD-FACT-057`).

*Contoh.* Ny. Sari (data samaran) naik ke Rawat Inap Melati dengan tiga pesanan: *Resep R-0012* tanpa sikap, *Darah
lengkap* ditolak Melati, *Ureum* bersikap `Continue`. Pasien tiba dan tindak lanjut dilaksanakan → kunjungan tetap
*Menunggu penutupan* dengan alasan *"Masih ada pesanan yang belum ditentukan sikapnya: Resep R-0012, Darah lengkap."*
Perawat IGD menetapkan *Handover* untuk resep, lalu *Cancel* beralasan untuk Darah lengkap → kunjungan *Selesai* pada
permintaan terakhir itu. Pada pasien lain, kepergian ke HCU yang memuat *Resep R-0020* tanpa sikap dibatalkan → baris
itu tidak menahan dan tetap tersimpan apa adanya.

### 9.5 Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Services/EmergencyDepartureService.cs` | +17/−8. (1) Field statis `SikapTercatat = Enum.GetValues<EmergencyOrderAction>()` (baris 24). (2) Method privat `KueriPesananPenahanPenutupan` (baris 587–597) — satu-satunya tempat aturan penahan: baris berlaku pada kepergian kunjungan itu, kepergiannya tidak `Cancelled`, dan `!SikapTercatat.Contains(x.Action)` **atau** `AcceptanceStatus == Rejected`. (3) `AmbilPesananPenahanPenutupanAsync` (baris 578–585) memakai kueri itu. (4) `ValidatePesananSebelumPenutupanAsync` (baris 550–564) memakai kueri yang sama ditambah saringan `Rejected`, sehingga kalimat §5 aturan 12 tidak pernah disusun dari pesanan tanpa sikap |
| `Services/EmergencyDispositionService.cs` | **Tidak disentuh.** `ValidateVisitClosureAsync` sudah memanggil `AmbilPesananPenahanPenutupanAsync` dan menyusun kalimat §6 aturan 4 |

Akhiran baris: 964 → 973 baris, seluruhnya CRLF. Nol baris komentar baru; komentar lama tidak disunting.

**Mengapa `SikapTercatat`, bukan `Enum.IsDefined`.** Definisi *tanpa sikap* wajib sama persis dengan
`SubmitHandoverAsync` baris 182 (`!Enum.IsDefined(x.Action)`). Di sana pemeriksaannya berjalan di memori atas
koleksi yang sudah dimuat; di sini ia harus menjadi SQL, dan `Enum.IsDefined` tidak dapat diterjemahkan EF Core.
`!Enum.GetValues<EmergencyOrderAction>().Contains(x.Action)` bermakna identik — nilai yang tidak termasuk anggota enum
— dan diterjemahkan menjadi perbandingan larik oleh Npgsql. Pola larik enum di dalam `Where` sudah dipakai kueri modul
lain (`AttendanceCorrectionService` baris 287, `AccPeriodClosingService` baris 561). Bila kelak enum bertambah anggota,
kedua tempat tetap sepakat karena keduanya membaca daftar anggota enum yang sama.

| Butir kartu | Wujud di source |
| --- | --- |
| (a) hitung tanpa sikap dan ditolak | Kondisi `!SikapTercatat.Contains(x.Action) \|\| AcceptanceStatus == Rejected` |
| (b) kecualikan kepergian fisik `Cancelled` | `x.EmergencyDeparture.PhysicalStatus != EmergencyPhysicalStatus.Cancelled`; baris pesanan tidak diubah |
| (c) kalimat §6 aturan 4 tetap | `EmergencyDispositionService` tidak disentuh |
| (d) kalimat §5 aturan 12 hanya untuk ditolak | Saringan `Rejected` pada `ValidatePesananSebelumPenutupanAsync`; aturan dasarnya tetap satu tempat |
| (e) tiga pemakai penjaga tidak disentuh | `EmergencyVisitController` `:714`, `EmergencyVisitService` `:703` dan `:791`, `DenganPenutupanSusulanAsync` pada `EmergencyDepartureController` `:128` — nol perubahan |

### 9.6 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol route, nol perubahan bentuk. Berubah **isi**: `409` pada `PATCH /emergency-visits/{id}/complete` dan `awaitingClosureReason` pada `GET /emergency-visits` / `GET /{id}` kini juga menyebut pesanan tanpa sikap; saringan `awaitingClosure=true` ikut memuat kunjungan yang tertahan pesanan tanpa sikap |
| Database | Nol migration, nol perubahan skema, nol penulisan massal. Agent tidak menjalankan kueri apa pun |
| Keamanan | Nol perubahan atribut akses dan pemeriksaan kewenangan unit |
| `Program.cs` | Tidak disentuh |
| Data lama | Berlaku ke depan (pola `IGD-DEC-167`): kunjungan `Completed`/`Cancelled` tidak dibuka kembali; kunjungan yang sedang menunggu penutupan dan punya pesanan tanpa sikap mulai tertahan, lalu tertutup lewat titik pemicu 3 `BE-IGD-061` begitu sikapnya ditetapkan |

### 9.7 Verifikasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path Areas/.../EmergencyDepartureService.cs -Mode Strict` | VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` |
| `git diff --stat` | 1 berkas, +17/−8 | `PASS` |
| Baris tambahan berisi `//` atau `/*` | Nol | `PASS` |
| Akhiran baris | 973 CRLF, 0 LF tunggal, 0 CR tunggal | `PASS` |
| `LangVersion` pada `QuilvianSystemBackend.csproj` | Tidak diatur (C# 13 bawaan `net9.0`) — `SikapTercatat.Contains` terikat ke `Enumerable.Contains` yang diterjemahkan EF | `PASS` |
| Pencarian pemakai `ValidateVisitClosureAsync` | Tiga pemanggil, seluruhnya tidak berubah | `PASS` |
| `dotnet build` atas diff ini | Dijalankan pemilik: DLL ditulis 6 Oktober 2026 08.30.28 WIB, sesudah berkas service berubah 08.21.49 WIB; backend dihentikan 08.28 dan dinyalakan lagi 08.41 WIB dari build itu. Pernyataan *"build aman"* yang lebih awal disampaikan sebelum diff ditulis. Jumlah warning **tidak dilaporkan** | `PASS` (error 0; warning tidak diketahui) |
| Uji API | Delapan skenario `041-S1`…`S8` oleh agen Antigravity atas nama pemilik — diperiksa agent pada bukti mentah dan log backend, bagian 9.11 | `PASS` 8/8; acceptance 14 dan 15 sebagian |

Perintah build yang dipakai pemilik, dari `NewQuilvianSystemBackend`:

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Rencana skenario uji API yang ditulis sebelum uji (pelaksanaannya di bagian 9.11):

| No | Keadaan | Harapan | Acceptance |
| ---: | --- | --- | --- |
| 1 | Ulang `PROBE-1`: satu pesanan luar sistem tanpa `action` pada kepergian yang tidak dibatalkan; pasien tiba; tindak lanjut dilaksanakan | Disposisi `Executed`; kunjungan **tetap** `Disposed` (7); `awaitingClosureReason` dan saringan `awaitingClosure=true` memuat kalimat §6 aturan 4 beserta uraiannya | 8 |
| 2 | Lanjutan 1: `PATCH /emergency-visits/{id}/complete` | `409`, kalimat yang sama | 9 |
| 3 | Lanjutan 1: perawat simpul IGD `PATCH …/order-items/{itemId}/action` | `200`; kunjungan `Completed` (9) pada permintaan itu, `ClosedByDispositionId` terisi, pelakunya perawat itu | 10 |
| 4 | Satu tanpa sikap + satu ditolak; lalu tujuh penahan | Daftar memuat keduanya; tujuh → lima uraian + *"dan 2 lainnya"* | 1, 2, 11 |
| 5 | Kepergian berisi pesanan tanpa sikap **dibatalkan**, lalu tindak lanjut dilaksanakan | Kunjungan langsung `Completed`; baris pesanan tetap `action` 0, `isEffective` benar | 12 |
| 6 | Kepergian berisi pesanan **ditolak** dibatalkan | Pesanan itu tidak menahan | 13 |
| 7 | `Continue`; `Handover` menunggu atau diterima; `Cancel`; baris tergantikan | Tidak menahan; kunjungan dapat ditutup | 5, 14 |
| 8 | Regresi `061-S3`, `S12` (kaki terima dan tolak), `BE-IGD-039` acceptance 8; kunjungan belum `Disposed`, observasi aktif, kepergian belum tiba | Perilaku dan urutan pesan tidak berubah | 4, 15 |

### 9.8 Acceptance criteria dan Definition of Done

Putusan agent dari bukti mentah dan log backend 6 Oktober 2026 (bagian 9.11), menggantikan tabel yang sempat ditulis agen
penguji (`IGD-DEC-212`).

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pesanan ditolak → `409` menyebut uraiannya | **Terpenuhi** | `041-S4` tahap B: `409` *"…: Resep R-01, Darah lengkap."* — *Darah lengkap* ditolak unit penerima |
| 2 | Paling banyak lima uraian + *"dan N lainnya"* | **Terpenuhi** | `041-S4` tahap A: tujuh penahan → lima uraian + *"dan 2 lainnya"* |
| 3 | Satu sumber aturan | **Terpenuhi** | `KueriPesananPenahanPenutupan` satu-satunya kueri penahan; kedua method publik memakainya |
| 4 | Aturan §6 nomor 1–3 lebih dulu, urutan dan pesan sama | **Terpenuhi** | `041-S8`: keempat kalimat muncul berurutan dan persis |
| 5 | Kunjungan tanpa penahan tetap dapat ditutup | **Terpenuhi** | `041-S7`: kunjungan `Completed` saat tindak lanjut dilaksanakan; juga `041-S5`, `S6` |
| 6 | Nol baris baru di `Program.cs` | **Terpenuhi** | `git diff` |
| 7 | `BE-IGD-035` dinilai ulang | **Terpenuhi** | Baris status `BE-IGD-035` diperbarui 6 Oktober 2026; kriteria 2 task itu kini terbukti runtime lewat `041-S2` dan `041-S4` |
| 8 | Ulang `PROBE-1` → tetap `Disposed`, alasan menyebut pesanan | **Terpenuhi** | `041-S1`: disposisi `Executed`, kunjungan 7, `awaitingClosureReason` pada `GET /{id}` dan saringan `awaitingClosure=true` menyebut *Resep R-0012*; baris pesanan `action` 0 |
| 9 | `complete` → `409` kalimat sama | **Terpenuhi** | `041-S2` |
| 10 | Sikap ditetapkan → kunjungan `Completed` pada permintaan itu | **Terpenuhi** | `041-S3`: `PATCH …/action` `200`; kunjungan 9, `ClosedByDispositionId` = disposisi kunjungan itu, `VisitCompletedAt` pada detik permintaan itu, pelaku akun `KLINIS` |
| 11 | Campuran dan tujuh penahan | **Terpenuhi** | `041-S4` tahap A dan B |
| 12 | Kepergian dibatalkan berisi tanpa sikap → tidak menahan; baris tidak berubah | **Terpenuhi** | `041-S5`: kunjungan 9; baris pesanan tetap `action` 0, `isEffective` benar |
| 13 | Kepergian dibatalkan berisi ditolak → tidak menahan | **Terpenuhi** | `041-S6` (putaran 01.53): pesanan `Rejected` pada kepergian `Cancelled`, kunjungan 9 |
| 14 | `Continue`, `Handover` menunggu/diterima, `Cancel`, baris tergantikan tidak menahan | **Terpenuhi** — 5 dari 5 variasi (6 Oktober 2026 sore, `IGD-DEC-218`) | `041-S7` memuat `Continue`, `Handover` **diterima**, `Cancel`. Putaran bersama (bagian 9.12): `041-S9` — `Handover` yang masih **menunggu** penerimaan (`AcceptanceStatus` 2, `IsEffective` benar) tidak menahan, kunjungan `9` saat tindak lanjut dilaksanakan; `041-S11b` — baris lama yang **digantikan** sikap pengganti (`IsEffective` false) dan baris pengganti `Continue` tidak menahan, alasan hanya *Uji PB C3-KOSONG*. *Sebelumnya: sebagian, 3 dari 5 variasi (`IGD-DEC-211`)* |
| 15 | Regresi `061-S3`, `S12`, `BE-IGD-039` acceptance 8, urutan §6 aturan 1–3 | **Terpenuhi** (6 Oktober 2026 sore, `IGD-DEC-218`) | `061-S3` dan `BE-IGD-039` acceptance 8 tercakup `041-S3`; urutan aturan 1–3 tercakup `041-S8`. `061-S12` pada putaran bersama (bagian 9.12): kaki terima `041-S10` — kunjungan `7` → `POST …/order-items/{id}/accept` `200` oleh `PENERIMA` → kunjungan `9`, `ClosedByDispositionId` terisi, `UpdateBy` = `PENERIMA`; kaki tolak `041-S11a` — `…/reject` `200`, kunjungan tetap `7`, alasan memuat kedua pesanan. *Sebelumnya: sebagian, `061-S12` belum diulang (`IGD-DEC-211`)* |
| 16 | Diff hanya `EmergencyDepartureService.cs`; nol komentar baru; komentar basi dicatat | **Terpenuhi** | Bagian 9.7 dan 9.9 |
| 17 | Build 0 error | **Terpenuhi** | Build pemilik, DLL 08.30.28 WIB; warning tidak dilaporkan |

**Definition of Done:** laporan tracked ✅; roadmap — termasuk baris Status `BE-IGD-035` — dan traceability diperbarui ✅;
QBE preflight dan checker ✅; tanpa UAT PASS ✅; acceptance 14 dan 15 pada putaran bersama `FE-IGD-044` ✅ (6 Oktober 2026
sore, bagian 9.12, `IGD-DEC-218`). *Sebelumnya: belum — acceptance 14 dan 15.*

### 9.9 Komentar lama yang menjadi basi — dicatat, tidak disunting

| Tempat | Bunyi | Mengapa basi |
| --- | --- | --- |
| `EmergencyDispositionService.cs` baris 146 | *"Kode dan kondisi penolakannya tidak berubah."* | Kondisinya kini berubah: pesanan tanpa sikap ikut menahan, kepergian dibatalkan dikecualikan |
| `EmergencyDepartureService.cs` baris 540–549 (dokumentasi `ValidatePesananSebelumPenutupanAsync`) | Menyebut method ini sebagai *pesanan yang menahan penutupan* dengan dua jenis penahan | Method ini kini hanya untuk kalimat §5 aturan 12, yaitu pesanan **ditolak** |
| `EmergencyDepartureService.cs` baris 566–577 (dokumentasi `AmbilPesananPenahanPenutupanAsync`) | *"satu-satunya tempat aturan kueri itu tinggal"*; *"dua aturan kontrak memakai kueri yang sama"* | Aturannya kini tinggal di `KueriPesananPenahanPenutupan`; aturan 12 menambah saringan `Rejected` di atasnya |

### 9.10 Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nihil dari checker. Jumlah warning build pemilik tidak dilaporkan |
| Masalah yang diketahui | (1) Pesanan resep, tindakan, atau laboratorium yang belum pernah menjadi baris pesanan kepergian tidak menahan (`IGD-FACT-054`, batas cakupan validation §5.1) — *coverage gap*, bukan cakupan task ini. (2) Urutan uraian dalam kalimat tidak diurutkan secara eksplisit — perilaku lama, tidak diubah. (3) Kueri penahan tidak menyaring kepergian yang dihapus lunak (`EmgDeparture.IsDelete`) — perilaku lama, tidak diubah |
| Keadaan migration | Nol |
| Eksekusi database | Nol |
| Risiko tersisa | **Menengah.** Kunjungan yang hari ini dapat tertutup dengan pesanan tanpa sikap akan tertahan, dan tanpa layar `FE-IGD-044` tidak dapat dibereskan petugas — karena itu rilis terpisah dilarang (`IGD-DEC-204`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | (1) `build-module-backend` `BE-IGD-064`, lalu `build-module-frontend` `FE-IGD-044`, masing-masing atas izin pemilik. (2) Satu panduan uji agent untuk putaran bersama: `FE-IGD-043` acceptance 1–7, `FE-IGD-044`, `BE-IGD-064`, dan sisa `BE-IGD-041` acceptance 14 dan 15 (`IGD-DEC-211`) |

`git status --short` di akhir pekerjaan (source):

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs
```

Berkas dokumen yang berubah pada working tree adalah laporan ini, roadmap backend, dan traceability.

### 9.11 Pemeriksaan bukti uji — 6 Oktober 2026

Uji dijalankan agen Antigravity atas nama pemilik, **tanpa** panduan dari agent. Agent memutus dari bukti mentah
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-041-20261006/` (delapan JSON dan `test_runner_be_igd_041.mjs`)
dicocokkan dengan `Logs/quilvian-backend-20261006.json`, bukan dari ringkasan
[laporan penguji](../../testing/2026-10-06-laporan-uji-be-igd-041.md). Ukuran aturannya A1–A15 panduan
[`BE-IGD-039`](../../testing/2026-10-05-panduan-uji-be-igd-039.md). Keputusan pemilik: `IGD-DEC-210`, `211`, `212`.

**Lingkungan.** Backend dihentikan 01.28.14 UTC; DLL ditulis 01.30.28 UTC; backend dinyalakan lagi 01.41.41 UTC, sehingga
putaran resmi 01.53.12–01.53.44 UTC berjalan di atas build yang memuat diff ini. Akun `LOKET`, `KLINIS`, `DOKTER`
(`isSuperAdmin: false` pada bukti 5 Oktober), `PENERIMA` — tanpa SuperAdmin; nol panggilan ke endpoint peran, akses,
penugasan HR, atau unit pelayanan. Sesi browser dan Swagger 02.05 UTC (akun lain, sesudah backend dinyalakan ulang
02.04 UTC) berada di luar putaran uji.

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `041-S1` | **Terbukti** — acceptance 8 | Disposisi `Executed`; kunjungan tetap 7; alasan menyebut *Resep R-0012* pada `GET /{id}` dan saringan `awaitingClosure=true`; baris `action` 0, `isEffective` benar |
| `041-S2` | **Terbukti** — acceptance 9 | `complete` `409`, kalimat persis; kunjungan tetap 7 |
| `041-S3` | **Terbukti** — acceptance 10; regresi `061-S3` dan acceptance 8 `BE-IGD-039` | `PATCH …/action` `200`; kunjungan 9 pada detik permintaan itu, `ClosedByDispositionId` terisi, pelaku `KLINIS` |
| `041-S4` | **Terbukti** — acceptance 1, 2, 11 | Tahap A tujuh penahan (enam tanpa sikap + satu ditolak) → lima uraian + *"dan 2 lainnya"*; tahap B *"Resep R-01, Darah lengkap"* |
| `041-S5` | **Terbukti** — acceptance 12 | Kepergian `Cancelled`; baris tetap `action` 0, `isEffective` benar; kunjungan 9 |
| `041-S6` | **Terbukti** — acceptance 13 (putaran 01.53) | Pesanan `Rejected` pada kepergian `Cancelled` tidak menahan; kunjungan 9 |
| `041-S7` | **Sebagian** — acceptance 5 terbukti, 14 tiga dari lima variasi | `Continue`, `Handover` diterima, `Cancel`; `Handover` menunggu dan baris tergantikan tidak diuji |
| `041-S8` | **Terbukti** — acceptance 4; bagian urutan acceptance 15 | Kalimat aturan 1, 2, 3, lalu 4, berurutan dan persis |

Pencocokan: **84 dari 84** permintaan yang terekam cocok dengan log (method, path, kode status, akun); dua di antaranya
baru cocok sesudah lama proses (`Elapsed` 5,5 dan 3,4 detik) diperhitungkan. Putusan dihitung ulang dari `pemeriksaan`:
delapan `PASS`. JSON bukti tidak memuat sandi.

**Penyimpangan — bukti diterima dengan penyimpangan tercatat (`IGD-DEC-210`).**

| # | Penyimpangan | Aturan | Penanganan |
| ---: | --- | --- | --- |
| a | Putaran pertama 01.46.17–01.50.54 UTC tidak dilaporkan dan buktinya tertimpa; seluruh JSON menulis `"percobaan": 1`. Pada putaran itu `041-S6` gagal — `PATCH …/cancel` `409` karena kepergian sudah *Tiba* sebelum dibatalkan (kesalahan desain uji); skrip lalu diubah | A11 | Dicatat; perilaku `409` itu benar menurut kontrak |
| b | Sandi diambil dari variabel lingkungan dengan nilai cadangan literal; alamat surel akun ditulis langsung di skrip | A5 | Dicatat; folder `test-with-agy` di-*ignore* git frontend, sehingga tidak ikut ter-commit |
| c | Laporan penguji memuat host dan port basis data dev | A5 | **Disamarkan agent** 6 Oktober 2026 |
| d | Kueri `Q-VISIT` dan `Q-PESANAN` dijalankan lewat helper di luar folder bukti; teks kuerinya tidak tersimpan | Panduan bagian 7 | Dicatat |
| e | Uji tanpa panduan agent; penguji menyunting laporan task ini, roadmap, dan traceability serta menandai ✅ | Aturan pemilik: laporan task hanya lewat `build-module` | Ditulis ulang agent sesuai putusan (`IGD-DEC-212`) |

**Acceptance 17.** Build dijalankan pemilik; error 0 terbukti dari DLL baru dan backend yang berjalan darinya. Jumlah
warning tidak dilaporkan — dicatat seperti preseden `BE-IGD-039`.

### 9.12 Pemeriksaan bukti uji putaran bersama — 6 Oktober 2026 (sore)

Uji dijalankan agen Antigravity atas nama pemilik **dengan** panduan agent
[`2026-10-06-panduan-uji-putaran-bersama.md`](../../testing/2026-10-06-panduan-uji-putaran-bersama.md) (aturan A1–A18).
Agent memutus dari bukti mentah `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-bersama-20261006/` (JSON per
skenario, `kueri.sql`, `db_helper.py`, tiga versi skrip) dicocokkan dengan `Logs/quilvian-backend-20261006.json`, bukan
dari ringkasan [laporan penguji](../../testing/2026-10-06-laporan-uji-putaran-bersama.md). Keputusan pemilik:
`IGD-DEC-218`; fakta `IGD-FACT-073`…`078` di decision log.

**Lingkungan.** Backend dinyalakan 02.32.07 UTC dari DLL 02.31.28 UTC (09.31.28 WIB) — sesudah berkas service ini
berubah (08.21.49 WIB) — dan tidak dinyalakan ulang sampai uji selesai, sehingga putaran resmi 05.17.46–05.20.02 UTC
berjalan di atas build yang memuat diff ini. Akun `LOKET` (`rendy.saputra`), `PERAWAT` (`dimas.kurniawan`, `IGD-DEC-215`),
`DOKTER` (`ranger.biru`), `PENERIMA` (`siti.nurhaliza`) — keempatnya `isSuperAdmin: false`. Sejak 04.20 UTC nol permintaan
SuperAdmin dan nol permintaan ke `/api/v1/administrator/**`. Tindak lanjut dibuat dan dikonfirmasi `DOKTER`,
dilaksanakan `PERAWAT`; kedatangan serta terima/tolak pesanan oleh `PENERIMA` (A18). Kueri pemeriksaan hanya `SELECT`,
teksnya tersimpan di folder bukti.

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `041-S9` | **Terbukti** — acceptance 14 (`Handover` menunggu) | Kunjungan `IGD-261006051932-864B26`: `RX` `200` 05.19.33 UTC (`PERAWAT`); `GET /{id}` `visitStatus` 9; Q-PESANAN `AcceptanceStatus` 2, `IsEffective` benar; Q-VISIT `ClosedByDispositionId` = `095e9000-…` (tindak lanjut kunjungan itu) |
| `041-S10` | **Terbukti** — acceptance 15, kaki terima `061-S12` | Kunjungan `IGD-261006051935-0C5265`: sesudah `RX` lalu `RT`, `visitStatus` 7; `POST …/order-items/{id}/accept` `200` 05.19.37 UTC (`PENERIMA`), `acceptanceStatus` 3; `visitStatus` 9; `ClosedByDispositionId` = `37585af3-…`; `UpdateBy` = id `PENERIMA` |
| `041-S11a` | **Terbukti** — acceptance 15, kaki tolak `061-S12` | Kunjungan `IGD-261006051937-C39A7D`: sesudah `RX`, `visitStatus` 7 dan `awaitingClosureReason` persis *"Masih ada pesanan yang belum ditentukan sikapnya: Uji PB C3-KOSONG."*; `…/reject` `200` 05.19.39 UTC (`PENERIMA`); `visitStatus` tetap 7; alasan *"…: Uji PB C3-KOSONG, Uji PB C3-TOLAK."* |
| `041-S11b` | **Terbukti** — acceptance 14 (baris tergantikan) | Sesudah sikap pengganti `Continue` dari layar (`044-U6`): `visitStatus` tetap 7; alasan persis *"…: Uji PB C3-KOSONG."* — tanpa C3-TOLAK; Q-PESANAN baris lama `IsEffective` false, baris pengganti `SupersedesOrderItemId` = baris lama dan `Action` 1 |
| `041-S11c` | **Terbukti** — penutupan susulan dari layar (`FE-IGD-044` acceptance 11) | Sesudah sikap terakhir dari layar (`044-U9`): `visitStatus` 9; Q-VISIT `ClosedByDispositionId` = `370ed76f-…`, `UpdateBy` = id `PERAWAT`, `EncounterStatus` 9 |

Pencocokan: seluruh permintaan di `jaringan[]` kelima skenario cocok dengan log (method, path, kode status, akun); putusan
dihitung ulang dari `pemeriksaan` — lima `PASS`. Pada `041-S11a` dan `041-S11b` skrip memeriksa alasan dengan
`includes`, tetapi nilai yang teramati persis sama dengan kalimat `K-PESANAN`.

**Penyimpangan — bukti diterima dengan penyimpangan tercatat (`IGD-DEC-218`).**

| # | Penyimpangan | Aturan | Penanganan |
| ---: | --- | --- | --- |
| a | Enam putaran persiapan P5/P6 (04.28–04.39 UTC), probe pilihan tindak lanjut (04.44–04.45 UTC), dan satu sesi layar (04.47–04.50 UTC) tidak dilaporkan — semuanya hanya membaca | A11 | Dicatat (`IGD-FACT-076`) |
| b | Percobaan 2 juga membuat kunjungan Blok C (VC1–VC3) dengan resep penuh; terima/tolak dijawab `404` karena path tanpa `{departureId}` (kesalahan skrip, bukan produk). Bukti percobaan 1–2 tertimpa; skrip disimpan per versi | A11, A12 | Dicatat; dua kunjungan Blok C percobaan 2 tertinggal menunggu penutupan (`IGD-FACT-077`) — tidak disentuh agent |
| c | Daftar pasien bersih ditulis tetap di skrip; kueri yang memilihnya tidak tersimpan | A10 | Dicatat |
| d | Narasi laporan penguji untuk `043-U4` dan daftar percobaannya berbeda dari bukti mentah | A12 | Dicatat (`IGD-FACT-078`); laporan penguji tidak disunting |

**Acceptance 17.** Diff source tidak berubah sejak build pemilik (bagian 9.7). Backend yang melayani putaran bersama
berjalan dari DLL 09.31.28 WIB yang dibuild agen penguji untuk `BE-IGD-064` (0 error, 235 warning menurut penguji —
`IGD-FACT-064`) dan juga memuat diff ini.

**Komentar basi.** Bagian 9.9 tetap berlaku; komentar lama tidak disunting.

| Hal | Isi |
| --- | --- |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore)**: 17 dari 17 acceptance terbukti (`IGD-DEC-218`) |
| Implementation | Selesai — satu berkas, `EmergencyDepartureService.cs` (+17/−8), tidak berubah sejak 08.21.49 WIB |
| Developer verification | QBE Strict `PASS`; build 0 error; uji API 8 skenario (bagian 9.11) + lima skenario putaran bersama pada bukti mentah dan log |
| UAT | Belum dijalankan — diserahkan ke tim UAT; bukan klaim UAT |
| Keadaan migration dan eksekusi database | Nol; agent tidak menjalankan kueri tulis |
| Risiko tersisa | **Menengah** sampai rilis: kunjungan dengan pesanan tanpa sikap mulai tertahan, sehingga `BE-IGD-041` hanya boleh dirilis bersama `FE-IGD-044` (`IGD-DEC-204`) — keduanya kini ✅ |
| Langkah berikutnya | Pemilik: commit, lalu rilis `BE-IGD-041` bersama `FE-IGD-044`. Bila pasiennya akan dipakai lagi, pemilik menutup kunjungan uji yang tertinggal (`IGD-FACT-077`) lewat layar atau API dengan akun peran nyata |

`git status --short` backend saat pemeriksaan (sebelum dokumen diperbarui):

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs
 M docs/module-blueprints/igd/00-interview-decisions.md
 M docs/module-blueprints/igd/roadmap/backend-roadmap.md
 M docs/module-blueprints/igd/roadmap/frontend-roadmap.md
 M docs/module-blueprints/igd/roadmap/requirement-traceability.md
 M docs/module-blueprints/igd/task/report/backend/BE-IGD-041.md
?? docs/module-blueprints/igd/task/report/backend/BE-IGD-064.md
?? docs/module-blueprints/igd/task/report/frontend/FE-IGD-044.md
?? docs/module-blueprints/igd/testing/2026-10-06-laporan-uji-be-igd-041.md
?? docs/module-blueprints/igd/testing/2026-10-06-laporan-uji-putaran-bersama.md
?? docs/module-blueprints/igd/testing/2026-10-06-panduan-uji-putaran-bersama.md
```
