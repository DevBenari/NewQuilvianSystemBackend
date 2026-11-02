# Laporan Perubahan Backend — `BE-IGD-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-042` |
| Judul | Encounter `Outpatient` ditolak untuk kunjungan IGD |
| Slice | `IGD-S08` · `EPIC IGD-01` · R3.8 |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.8 |
| Trace | `FR-IGD-001`, `FR-IGD-002`; `IGD-DEC-074`, `IGD-DEC-109` (syarat pencabutan), `IGD-DEC-120` (teks pesan), `IGD-DEC-373` (amandemen acceptance 5); bukti `IGD-EV-121`, `IGD-FACT-266` |
| Contract version | Validation §1 aturan 2 sebagaimana diselaraskan `IGD-DEC-120` (`approved`) — kode `400`, pesan tetap. Nol perubahan route, DTO, atau envelope |
| Dependency | Konfirmasi data owner ✅ (28 Oktober 2026 16.37, lihat bagian 1); `IGD-DEC-120` ✅ |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), ±12 berkas diperiksa (1), satu berkas source diubah (0), logika sederhana (0), kontrak yang sudah ada (1), tanpa database (0), tanpa auth (0), satu alur pendaftaran (1) |
| Task mode | `BACKEND` — atas perintah Rizki 28–29 Oktober 2026 |
| Target tulis | `NewQuilvianSystemBackend` (`rizkiG`): `EmergencyVisitService.cs` method `PeriksaJenisEncounter` saja; laporan ini, roadmap, traceability, `MODULE-STATUS.md`, `00-interview-decisions.md` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `1f4212e8` (working tree, belum di-commit) |
| Tanggal | 29 Oktober 2026 |
| Status | ✅ **SELESAI — 5 dari 5 acceptance (acceptance 5 sebagaimana diamandemen `IGD-DEC-373`).** QBE Strict `PASS`; build Rizki 29 Oktober 2026 08.55; uji API 4 dari 4 `PASS`. Implementation ✅ · DevVerif ✅ · UAT belum |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik dan prefix registry | IGD, prefix **`Emg`**, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 28) |
| Keberlakuan | `TOUCHED LEGACY` — satu method yang sudah ada; nol berkas baru, nol entity |
| QBE ID yang berlaku | `QBE-VAL-001` (validasi invarian bisnis), `QBE-SVC-001` (aturan milik service, dipakai controller), `QBE-MOD-001` (penempatan dalam modul pemilik) |
| Branch | `rizkiG`, upstream `origin/rizkiG` — sesuai penetapan pemilik |
| `Program.cs` | Nol baris |
| Migration | **Tidak ada**, tidak dibutuhkan |
| Sisa `agents/rules/` atau `.codex/` | Tidak ditemukan |

---

## 1. Masalah yang diperbaiki

Sejak `BE-IGD-023` (`IGD-DEC-109`), pendaftaran IGD menerima dua jenis encounter: `Emergency` dan `Outpatient`.
`Outpatient` sengaja diterima sebagai **masa transisi**, karena saat itu kunjungan IGD lama tercatat dengan encounter rawat
jalan. Menolaknya waktu itu akan memutus semua kunjungan yang sudah ada. Syarat pencabutannya: kunjungan lama sudah
dipindahkan dan jumlahnya cocok.

Akibatnya bagi rumah sakit: petugas tetap dapat mendaftarkan pasien IGD memakai encounter **rawat jalan**. Pasien itu lalu
ikut terhitung di laporan rawat jalan, padahal ditangani di IGD.

**Konfirmasi data owner.** Agent dilarang menjalankan kueri basis data. Rizki menjalankan kueri kartu di
`QuilvianNewDevRizki` pada 28 Oktober 2026 pukul 16.37:

| Kueri | Hasil |
| --- | ---: |
| `EmgVisit` aktif (`IsDelete = false`) yang encounter-nya `EncounterType = 1` (`Outpatient`) | **0** |
| Yang sama, hanya kunjungan yang masih terbuka (`VisitStatus` bukan 8 *Cancelled* atau 9 *Completed*) | **0** |

Karena hasilnya nol, tidak ada kunjungan lama yang terputus oleh pencabutan ini, dan syarat kartu terpenuhi. Migration
`ChangeEmergencyEncounterTypeToEmergency` yang disebut `IGD-DEC-109` tidak pernah dibuat. Syaratnya tetap terpenuhi lewat
data: jumlah baris yang dipersoalkan memang nol.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Petugas pendaftaran IGD | Memilih atau membuat encounter pasien, lalu menyimpan kunjungan IGD (`POST emergency-visits`) |
| 2 | Backend | Memeriksa jenis encounter lewat `PeriksaJenisEncounter` sebelum pemeriksaan unit, pasien, dan keunikan encounter |
| 3a | Backend | Encounter `Emergency` → lanjut ke pemeriksaan berikutnya (perilaku tidak berubah) |
| 3b | Backend | Encounter selain `Emergency`, **kini termasuk `Outpatient`** → ditolak `400`: *"Encounter yang dipilih bukan kunjungan IGD. Pilih atau buat encounter dengan jenis kunjungan gawat darurat untuk pasien ini."* |
| 4 | Petugas | Membuat encounter berjenis gawat darurat untuk pasien itu, lalu mengulang pendaftaran. Layar `FE-IGD-014` sudah mengirim `Emergency`, jadi alur normal tidak terkena |

**Jalur tidak normal yang ikut berubah.**

- **Mengubah kunjungan** (`PUT emergency-visits/{id}`): memakai pemeriksaan yang sama. Kunjungan dengan encounter
  `Outpatient` tidak lagi lolos validasi ubah. Menurut kueri owner, jumlahnya nol.
- **Penutupan encounter saat kunjungan selesai** (`ApplyEncounterClosureAsync`): encounter `Outpatient` kini tidak ikut
  ditutup oleh IGD. Kunjungan terbuka yang terkena juga nol.

**Contoh.** Pasien A punya encounter `ENC-RSMMC-00231` berjenis rawat jalan.
- Sebelum perubahan: mendaftarkannya ke IGD lolos pemeriksaan jenis, lalu baru tertahan bila unitnya bukan IGD.
- Sesudah perubahan: langsung ditolak dengan pesan di atas, apa pun unitnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs`: `ValidateRequestAsync`, `PeriksaJenisEncounter`, `ApplyEncounterClosureAsync`.
- `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs`: `Create`, `Update`, method privat `ValidateRequestAsync`.
- `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyEpisodeRule.cs`.
- `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyEncounterReconciliation.cs`.
- `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs`, untuk filter `encounterType` pada uji.
- `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.
- `rules/backend/TASK_CLASSIFICATION.md`, `REPORT_TEMPLATE.md`.
- Kartu `BE-IGD-042`, serta `IGD-DEC-109` dan `IGD-DEC-120`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs` | (1) `PeriksaJenisEncounter`: syarat `encounterType is EncounterType.Emergency or EncounterType.Outpatient` menjadi `encounterType is EncounterType.Emergency`. (2) Paragraf `<para>` *"Masa transisi"* (9 baris `///`, termasuk baris kosong pemisah) dihapus tanpa pengganti, sesuai `IGD-DEC-373`. Sisa doc-comment method itu tidak disentuh. Diff: 1 baris ditambah, 10 dihapus |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route, DTO, dan envelope tidak berubah. Perilaku validasi berubah: `Outpatient` kini `400` dengan pesan `IGD-DEC-120`, sesuai validation §1 aturan 2. Konsumen frontend (`FE-IGD-014`) sudah mengirim `Emergency` |
| Database | `NOT APPLICABLE` — tanpa schema, entity, atau migration. Nol baris data berubah oleh uji |
| Keamanan/Auth | `NOT APPLICABLE` — `[AccessPermission("EmergencyVisit", "Create")]` dan `"Update"` tidak disentuh |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru atau berubah bentuk. Endpoint yang perilaku validasinya berubah:

#### Emergency Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/emergency-installation-management/emergency-visits` | Mendaftarkan kunjungan IGD; encounter `Outpatient` kini ditolak `400` | `EmergencyVisit : Create` |
| `PUT` | `/api/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Mengubah kunjungan IGD; pemeriksaan jenis encounter yang sama | `EmergencyVisit : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE Strict, `-Path` berkas yang berubah | 1 berkas, `VIOLATION 0`, `REVIEW 0`, `INFO 0`, `Final result: PASS` | `PASS` | Keluaran perintah, 29 Oktober 2026 |
| `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` | Dijalankan Rizki. DLL tertulis 29 Oktober 2026 08.55.16, backend mulai 08.55.43 | `PASS` (pernyataan Rizki + waktu berkas) | `bin/Debug/net9.0/QuilvianSystemBackend.dll` |
| Uji API run 1 (09.50 WIB, `2026-10-29T01-50-09-680Z`) | Prasyarat gagal: daftar `outpatient-encounters` kosong untuk hari itu; skrip diganti memakai `patient-encounters?encounterType=1` | `EXISTING / ENVIRONMENT ISSUE` | `*-hasil.json`, `*-jaringan.json` |
| Uji API run 2 (`2026-10-29T01-50-39-416Z`) | A1 `FAIL` — pesan cek unit, bukan pesan jenis. Penyebab: backend berjalan dengan DLL 28 Oktober 15.32, belum hasil build baru. Run ini menjadi **pembanding sebelum perubahan**: `Outpatient` lolos cek jenis | `EXISTING / ENVIRONMENT ISSUE` | `*-hasil.json`, `*-jaringan.json` |
| Uji API run 3, sesudah build (`2026-10-29T01-59-51-638Z`) | Prasyarat, A1, A2, A3 — 4 dari 4 `PASS`; 6 `GET 200`, 2 `POST 400`; nol kunjungan baru | `PASS` | `*-hasil.json`, `*-jaringan.json` |

Skrip dan bukti ada di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-be-igd-042-20261029/` (tidak ikut git). Akun:
perawat IGD uji. Izin akun itu bukan objek uji.

Rincian run 3:

| ID | Permintaan | Harapan | Nyata |
| --- | --- | --- | --- |
| A1 | `POST emergency-visits` memakai encounter `ENC-RSMMC-00231` (`Outpatient`, pasien lain, unit bukan IGD), body selebihnya dari kunjungan uji C | `400` dengan pesan `IGD-DEC-120` persis | `400`, pesan persis |
| A2 | `POST emergency-visits` memakai encounter `Emergency` kunjungan C (`ENC-RSMMC-00510`) | Lolos cek jenis; berhenti di aturan satu encounter satu kunjungan | `400` *"Encounter ini sudah memiliki kunjungan IGD…"* |
| A3 | `GET emergency-visits`, `GET emergency-visits/{C}` | `200`, daftar tidak kosong, detail C sama persis dengan sebelum uji | `200`, `200`, sama |

Kedua `POST` sengaja dirancang berakhir `400`. Uji tidak menambah kunjungan atau encounter di basis data dev.

Uji manual: `PASS`.

**Tidak dijalankan:**
- Uji `PUT` dengan encounter `Outpatient`: jalur ubah menolak lebih dulu setiap pergantian encounter dengan `409` (`IdentitasKunjunganBerubah`), dan kunjungan ber-encounter `Outpatient` tidak ada (kueri owner 0).
- Unit test: tidak ada proyek test (`IGD-DEC-110`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `EncounterType.Outpatient` ditolak `400` dengan pesan `IGD-DEC-120`, pada jalur controller maupun service | Terpenuhi | Satu pemeriksaan `PeriksaJenisEncounter`, dipanggil `EmergencyVisitService.ValidateRequestAsync` (baris 229). Validasi itu dipakai `EmergencyVisitController.Create` (baris 363) dan `Update` (baris 631), sedangkan method privat controller (baris 1014) memanggil method yang sama. Runtime: A1 |
| 2. `EncounterType.Emergency` tetap diterima | Terpenuhi | Runtime A2: lolos cek jenis, berhenti di aturan berikutnya |
| 3. Kunjungan lama tidak diubah; daftar dan detailnya tetap terbaca | Terpenuhi | Nol migration, nol update data; runtime A3; kueri owner 0/0 |
| 4. Frontend tidak perlu diubah — `FE-IGD-014` sudah mengirim `Emergency` | Terpenuhi | Nol berkas frontend disentuh |
| 5. Komentar masa transisi diperbarui menjadi keadaan sesudah pencabutan *(diamandemen `IGD-DEC-373`: paragraf dihapus tanpa pengganti; dasar pencabutan dicatat di laporan ini dan `IGD-FACT-266`)* | Terpenuhi sesuai amandemen | Diff bagian 3.2; nol baris komentar baru |

| DoD | Status |
| --- | --- |
| Acceptance 1–5 terpetakan | Terpenuhi |
| Laporan tracked ada | Terpenuhi — berkas ini |
| Roadmap dan traceability diperbarui | Terpenuhi |
| QBE preflight diselesaikan | Terpenuhi — Strict `PASS` |
| Tanpa UAT PASS | Dipatuhi — UAT milik tim terpisah |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Run 2 menunjukkan backend dapat berjalan dengan DLL lama sesudah "build". Periksa waktu DLL bila hasil uji tidak berubah |
| Masalah yang diketahui | (1) Doc-comment `ApplyEncounterClosureAsync` (baris ±615) masih menyebut *"`Outpatient` masa transisi"*. Komentar lama itu tidak disunting, karena `IGD-DEC-373` hanya mencakup paragraf di `PeriksaJenisEncounter`. (2) `EmergencyVisitController` punya method privat `ValidateRequestAsync` (baris 972) dan overload-nya (baris 1043) **tanpa pemanggil**. Itu kode mati, di luar lingkup, dicatat saja |
| Risiko tersisa | Rendah. Lingkungan lain (staging/produksi) yang masih punya kunjungan IGD ber-encounter `Outpatient` akan kehilangan penutupan encounter otomatis untuk kunjungan itu. Kueri prasyarat perlu diulang di lingkungan tersebut sebelum rilis |
| Perubahan sampingan | `NONE` |
| Interupsi | Backend mati saat uji pertama (sesi berganti hari), lalu DLL lama terpakai pada run 2. Pemulihan: Rizki build ulang; run 3 lulus |
| Status Git | Source: `M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs`. Dokumen `docs/module-blueprints/igd/**`: perubahan `BE-IGD-077` (28 Oktober) dan `BE-IGD-042` |
| Langkah berikutnya | Commit atas perintah Rizki; `verify-module-readiness` untuk `MVP-11`; ulangi kueri prasyarat di lingkungan rilis |
