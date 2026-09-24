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
| Status | **Implementasi selesai.** Ketujuh acceptance criteria terpetakan ke source. `dotnet build` **belum dijalankan** — diserahkan kepada Rizki. Uji API manual belum dijalankan. Tanpa UAT |

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
